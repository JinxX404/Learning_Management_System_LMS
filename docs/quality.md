# docs/quality.md — Definition of Done & Verification Matrices

Testing and gating authority. A change is **not done** until every applicable row below
has evidence. Commands assume the repo root; `AGENTS.md` §2 owns the inner-loop vs.
exit-gate split.

## 1. Definition of Done (single checklist)

- [ ] Requirement implemented as one vertical slice (schema → controller → view → test).
- [ ] `docs/schema/LMS Schema.sql` updated first if the schema changed; no migration files added (I4).
- [ ] All touched files formatted: `dotnet format "Learning Management System.sln" --include <files>`.
- [ ] `dotnet build "Learning Management System.sln" --no-restore` clean for touched projects (no new warnings).
- [ ] Targeted unit test(s) for the changed logic, inside the inner loop.
- [ ] Targeted integration test when persistence or database objects are touched.
- [ ] New/changed terms added to `CONTEXT.md`; invariants reflected in `docs/domain.md`.
- [ ] Evidence recorded in `evidence/issue-NN/task-NN-evidence.md` (commands, exit codes, output) — repo-root folder, one per epic, separate from `.scratch/issue-NN-*/`.
- [ ] Human approval `PASS` obtained before any remote push.

## 2. Verification layers

| Layer | Scope | Tooling | Command (targeted) | Runs in CI | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| Format | Touched files | `dotnet format` | `… --include <files>` | `static-and-unit` | Active |
| Lint / analyzers | Whole solution | `dotnet format analyzers --verify-no-changes` | same, with `analyzers` | `static-and-unit` | Active |
| Typecheck / build | Whole solution | Roslyn (`dotnet build <solution>`) | `dotnet build "Learning Management System.sln" --no-restore` | `static-and-unit` | Active (3 known nullable warnings, D4) |
| Unit | Pure logic, no I/O | xUnit `tests/Lms.Tests` | `dotnet test tests/Lms.Tests --filter "FullyQualifiedName~<Class>"` | `static-and-unit` | Active (7 tests: SessionHelper, PasswordHashing) |
| Persistence / integration | Real SQL Server from the schema script | xUnit `tests/Lms.IntegrationTests` | `dotnet test tests/Lms.IntegrationTests --filter "FullyQualifiedName~<Class>"` with `$env:LMS_TEST_CONNECTION` | `integration` | Active (schema existence, EF query) |
| Contract | External HTTP/JSON contracts | — | — | — | Not applicable (no external API consumers) |
| Browser / E2E | Multi-viewport UI flows | *TBD* (decision D2) | TBD | `e2e` (reserved) | Pending — do not claim E2E coverage |
| Smoke | App boots, serves, authenticates | `dotnet run` + HTTP probes in CI | CI job `smoke` | `smoke` | Active |
| Platform proof | Windows-native behavior | On demand | Local manual / decision D8 | Pending | Deferred |

Rules:

- **No layer may be satisfied by mocks of the real seam.** Integration rows run against a
  real SQL Server created from `docs/schema/LMS Schema.sql` (never EF's in-memory
  provider, never a mocked `DbContext`).
- A failing layer blocks the exit gate; it is never waived by editing the assertion
  without an approved requirement change (see failure bucket 4 in `AGENTS.md` §5).

## 3. Inner loop vs. exit gate (summary)

| | Inner loop (tasks 01–07) | Exit gate (task 08 / before PR) |
| :--- | :--- | :--- |
| Budget | < 35 minutes | No budget, but must be green |
| Format | Touched files only | Full `--verify-no-changes` |
| Lint | — | Full analyzers verify |
| Tests | Single unit file or whole `Lms.Tests` project; single integration file | Full unit + full integration |
| Build | `dotnet build "Learning Management System.sln" --no-restore` | `-c Release` build |
| Packaging / smoke | Forbidden | CI `smoke` job |
| Remote push | Forbidden | Only after human `PASS` |

## 4. CI mapping (`.github/workflows/verify.yml`)

| Job | Proves | Blocking |
| :--- | :--- | :--- |
| `static-and-unit` | Format, analyzers, Release build, unit suite | Yes |
| `integration` | Schema script applied to a fresh SQL Server 2022 container + integration suite | Yes |
| `smoke` | Application boots against that database and serves login, Swagger, and an auth-gated redirect | Yes |
| `verify-gate` | Aggregate: all of the above succeeded | Yes (merge gate) |

Integration and smoke run on ephemeral databases; the SQL script is applied inside the
job, which doubles as a proof that a clean install works.

## 5. Evidence standard

Every completed subtask records:

1. Exact commands executed and their exit codes.
2. Test output summary (counts, not paraphrases).
3. Screenshots for any UI-visible change (standard viewport 1280×800, plus dark theme if
   the screen participates in theming).
4. The human approval answer (`PASS`/`FAIL`) with date.

Store evidence under `evidence/issue-NN/task-NN-evidence.md` (repository root — the
`evidence/` folder is created with the first epic; the ticket/spec material lives
separately under `.scratch/issue-NN-*/`).

## 6. UI quality bar (applies to every view change)

- Renders at 1280×800 without clipped or off-screen action buttons; scrolling containers
  use `overflow: auto` with `min-height: 0` so automated viewport checks can pass.
- Keyboard-operable: visible focus, logical tab order, no keyboard traps; dialogs trap
  focus and restore it on close.
- Light and dark themes both legible; contrast meets WCAG AA for text and controls.
- No native browser dialogs (`alert`/`confirm`/prompt) anywhere. Existing violations are
  tracked debt (see `AGENTS.md` rule 8) — never add new ones.
- Feature styles scoped under the feature root; unrelated screens must render unchanged
  after the change (screenshot-diff by inspection).

## 7. Performance guardrails

- List endpoints/views are paginated once a table can grow beyond a page (admin user and
  course lists are the known growth points).
- No unindexed query in a hot path: any new filter column is indexed in the schema script
  with the query that uses it.
- No N+1 query patterns introduced by loading related collections inside loops; use
  projection or `Include` deliberately.
