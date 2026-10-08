# AGENTS.md — Operating Constitution (Academix LMS)

This file is loaded into agent context on every invocation. It is the operational
constitution for autonomous and pair-programming agent work in this repository.

**Precedence:** follow the reading order in `docs/README.md` —
`AGENTS.md` > `CONTEXT.md` > `docs/domain.md` > `docs/architecture.md` >
`docs/quality.md` > `docs/open-decisions.md`. One exception: a non-negotiable invariant
in `docs/domain.md` is never overridden by a convenience rule here — the invariant wins
and this file must be updated.

---

## 0. What this repository is (facts — do not re-derive)

Academix, a Learning Management System: a **single ASP.NET Core 8 MVC web application**
with Admin, Instructor, and Student portals.

| Concern | Fact |
| :--- | :--- |
| Language / runtime | C# / .NET 8 (`net8.0`), nullable enabled, implicit usings |
| Web stack | ASP.NET Core MVC + Razor views, Swagger (Development only) |
| Data access | Entity Framework Core 9 (`Microsoft.EntityFrameworkCore.SqlServer`) |
| Database | SQL Server (LocalDB or full instance) |
| Schema source of truth | `docs/schema/LMS Schema.sql` — **there are no EF migrations** (decision D1 in `docs/open-decisions.md`) |
| Authentication | Custom session auth: BCrypt hashes (`BCrypt.Net-Next`), user id in `HttpContext.Session`. **ASP.NET Core Identity is NOT used.** |
| Frontend | Razor, Bootstrap 5, jQuery, vanilla JS/CSS under `wwwroot/`; Chart.js is CDN-loaded (`Views/Student/Dashboard.cshtml`), not vendored |
| Unit tests | xUnit — `tests/Lms.Tests` (no database) |
| Integration tests | xUnit — `tests/Lms.IntegrationTests` (real SQL Server via `LMS_TEST_CONNECTION`) |
| Format / lint | `.editorconfig` + `dotnet format` (whitespace, style, analyzers) |
| Dependency determinism | `packages.lock.json` per project; restore with `--locked-mode` |
| CI | GitHub Actions — `.github/workflows/verify.yml` |
| Local issue tracking | `.scratch/` (tracked in git) |

Solution layout: the web project sits at the repository root
(`Learning Management System.csproj`) and explicitly excludes `tests/**` from its compile
items. Because both a `.sln` and a `.csproj` exist at the root, **every `dotnet format`
command must name the solution explicitly** or it fails with a workspace error.

---

## 1. Non-negotiable engineering rules

1. **No speculative abstractions.** Implement the simplest concrete code that satisfies
   the requirement. No premature generic classes, plugin hooks, or indirection layers.
2. **One vertical slice at a time.** Deliver end-to-end working software
   (schema -> controller/service -> view -> test) for one feature before starting the
   next. Never leave wide, half-implemented horizontal layers.
3. **No dead-code compatibility layers.** Never retain backward compatibility with
   prototypes, removed scaffolds, or old naming. Remove superseded paths completely.
   Live data is preserved by forward changes to `docs/schema/LMS Schema.sql`, not by
   application-side adapter bloat.
4. **Real infrastructure over mocking ("real seams").** Unit tests exercise pure logic.
   Integration tests run against a **real SQL Server** created from
   `docs/schema/LMS Schema.sql`. Never use EF's in-memory provider, a mocked `DbContext`,
   or faked data-integrity paths to manufacture a pass.
5. **The SQL script owns the schema.** Any change to tables, views, triggers, stored
   procedures, or functions must be made in `docs/schema/LMS Schema.sql` first.
   `Database.EnsureCreatedAsync()` at startup is *not* an upgrade mechanism.
6. **Secrets never enter tracked files.** `appsettings.json` ships with an empty
   `BootstrapAdmin:Password`. Local credentials belong in `appsettings.Development.json`
   (gitignored). Never echo connection strings with passwords into logs, commits, or docs.
   Carve-out: the throwaway CI container credential tracked in
   `.github/workflows/verify.yml` is the single documented exception (ephemeral
   single-job container, never reused anywhere else).
7. **Authorization is not optional.** Keep the existing session/role checks
   (`IsLoggedIn`, role gates) in every portal action you touch. Never weaken or bypass
   them to make a test or a flow pass.
8. **No native browser dialogs.** `window.alert()`, `window.confirm()`, and native
   prompts are forbidden: they freeze threads, break headless CI, cannot be themed, and
   destroy accessibility. Use custom in-app modals. Existing occurrences in 6 views are
   tracked debt (audit finding) — never add new ones; `wwwroot/js/toast.js` already ships
   the confirm modal.
9. **Strict scope isolation in CSS.** Prefix feature styles under an isolated container
   root. Never modify global resets or shared shell utilities in the loaded shell
   stylesheet (`wwwroot/css/main.css`; `site.css`/`themes.css` are legacy and
   unreferenced) to fix a feature-specific layout; verify unrelated screens still render
   identically.
10. **Local host limits are not permission to weaken security.** If this Windows
    workstation lacks a capability (e.g. POSIX mode bits, raw sockets, LocalDB that will
    not start), scope that verification to CI. Never edit application code to bypass an
    environment limitation.

---

## 2. Inner loop vs. exit gate — exact commands

### 2.1 Fast inner loop (during tasks 01–07, budget < 35 min)

```powershell
# Restore (only after dependency changes; never re-resolve versions by hand)
dotnet restore --locked-mode

# Typecheck / compile (incremental; Roslyn has no per-file typecheck, this is it)
dotnet build "Learning Management System.sln" --no-restore

# Format ONLY the files you touched (paths are relative to the repo root)
dotnet format "Learning Management System.sln" --include path/to/TouchedFile.cs

# Single unit test file
dotnet test tests/Lms.Tests --no-restore --filter "FullyQualifiedName~SessionHelperTests"

# Single unit test method
dotnet test tests/Lms.Tests --no-restore --filter "FullyQualifiedName~SessionHelperTests.ClearSession_removes_the_stored_user"

# Single integration test file against a real SQL Server
$env:LMS_TEST_CONNECTION = "Server=.;Database=LMS;Trusted_Connection=True;TrustServerCertificate=True"
dotnet test tests/Lms.IntegrationTests --no-restore --filter "FullyQualifiedName~SchemaExistenceTests"

# Browser/E2E scenario: none yet (decision D2 in docs/open-decisions.md)
```

`tests/Lms.Tests` runs in seconds, so running the whole unit project inside the inner
loop is allowed. **Running the whole solution's tests (unit + integration together) is
not** — integration tests need a provisioned database and belong to the exit gate.

If no SQL Server is reachable, the integration tests fail with an instructive message
(they never skip silently). That is failure bucket 3 below: fix or provision the
environment, and treat the CI `integration` job as the authoritative seam. Do not weaken
the assertion.

### 2.2 Forbidden during intermediate subtasks

- `dotnet test` with no filter at the repository root (runs both test projects).
- Bare `dotnet format` without `--include` (reformats the whole repository).
- `dotnet publish`, packaging, Docker release builds, or any slow release artifact.
- `dotnet restore` without `--locked-mode`, or adding/upgrading packages without
  regenerating `packages.lock.json` (`dotnet restore --force-evaluate`).
- Editing `.editorconfig` or global formatting settings to silence a local diff.

### 2.3 Pre-flight exit gate (task 08 and immediately before opening a PR)

```powershell
dotnet restore --locked-mode
dotnet format "Learning Management System.sln" --verify-no-changes          # format
dotnet format analyzers "Learning Management System.sln" --verify-no-changes # lint
dotnet build "Learning Management System.sln" --no-restore -c Release                                          # typecheck + build
dotnet test tests/Lms.Tests --no-build --no-restore -c Release               # full unit suite
$env:LMS_TEST_CONNECTION = "..." ; dotnet test tests/Lms.IntegrationTests --no-build --no-restore -c Release
```

The `smoke` proof (boot the packaged app against a provisioned SQL Server and probe
`/Auth/Login`, `/swagger`, and a protected route) runs in CI (`.github/workflows/verify.yml`).
All exit-gate commands must be green before a PR is opened.

---

## 3. Local tracking via `.scratch/`

Every epic and feature is tracked in `.scratch/issue-NN-<feature-slug>/` (tracked in git,
never ignored):

```text
.scratch/issue-NN-<feature-slug>/
├── spec.md              # acceptance contract, A01..A0N matrix
├── map.md               # Mermaid DAG, task register, migration reservations
├── GATES.md             # external gates + temporary defaults (optional)
├── handoff-summary.md   # handoff log between subtasks
└── issues/01-<slug>.md .. 08-<slug>.md   # 8 tracer-bullet subtasks, 11-section format
```

- Templates live in `.scratch/templates/` (`spec-template.md`, `map-template.md`,
  `ticket-11-section-template.md`). Starting work without a `spec.md` + `map.md` is not
  allowed.
- Decompose complex epics into the standard 8 tracer-bullets:
  01 schema/domain hook -> 02 contracts/API boundaries -> 03 primary UI -> 04 security &
  state machine -> 05 secondary workflows -> 06 atomic server operation ->
  07 review/history/a11y polish -> 08 integrated proof + exit gate.
- Evidence for each subtask is recorded in the repo-root `evidence/issue-NN/` folder
  (`evidence/issue-NN/task-NN-evidence.md`: test logs, exit codes, screenshots). This is
  **separate from** the `.scratch/issue-NN-*/` ticket folder above — one `evidence/`
  folder per epic, created on demand.

---

## 4. Human approval gate (hard stop)

- Commits stay **local** on the working branch. **DO NOT `git push` to the remote before
  the human explicitly answers `PASS`.**
- At the end of every subtask, halt with the application visible/running and ask:
  > "Task NN automated checks and manual rehearsal complete. Please review the
  > application state and evidence. Do you approve: PASS or FAIL?"
- Commits are conventional and scoped:
  `feat(<module>): <description> (Task NN)`, `fix(...)`, `docs(...)`, `test(...)`, `chore(...)`.

---

## 5. Failure classification (use before touching code)

| Bucket | Signal | Action |
| :--- | :--- | :--- |
| 1. Product/code defect | Logic or assertion is wrong | Targeted fix in the failing code |
| 2. Environment / missing prerequisite | DB down, no `LMS_TEST_CONNECTION`, port conflict | Fix the environment; **never** edit app code to bypass it |
| 3. Host / sandbox limitation | LocalDB will not start, missing OS capability | Verify in CI instead; document in the ticket |
| 4. Test drift | Requirement changed, assertion is stale | Update the assertion to the approved requirement |

---

## 6. Mandatory reading before writing code

1. `AGENTS.md` (this file)
2. `docs/README.md` — source authority and reading order
3. `CONTEXT.md` — canonical domain vocabulary (never invent synonyms)
4. `docs/domain.md` — invariants you may not violate
5. `docs/architecture.md` — boundaries and data flows
6. The active ticket: `.scratch/issue-NN-*/spec.md`, `map.md`, `issues/NN-*.md`
