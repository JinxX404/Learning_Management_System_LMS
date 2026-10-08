# docs/open-decisions.md — Gated Structural Decisions

Decisions that cannot be settled yet. Each row declares the **engineering default** every
agent builds against until the gate closes. Do not guess an alternative, and do not
"helpfully" implement the blocked option.

| ID | Decision | Status | Engineering default (build against this) | Gate to close | Evidence required |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **D1** | Schema lifecycle: EF Core migrations vs. versioned SQL script (finding F-09) | Open | `docs/schema/LMS Schema.sql` is the sole schema source of truth. No `Migrations/` folder, no `Migrate()`, `EnsureCreatedAsync()` is not an upgrade path. | Team picks one lifecycle and writes clean install/upgrade steps | Written decision + a successful clean-install rehearsal |
| **D2** | Browser/E2E test tooling | Open | **No E2E suite exists.** Do not claim browser coverage in tickets or docs; UI verification is the manual protocol in `docs/quality.md` §6 plus CI smoke. | Tool chosen (candidate: Playwright for .NET) and first scenario merged | One green scenario in CI |
| **D3** | Enrollment vs. lesson-progress model (finding F-02) | Open | `CourseEnrollment` may only be written by actual enrollment/progress code already present. No new writers; no new `Status` values. | Refactoring session 4 approved (separate progress concept) | Schema + code change with regression tests |
| **D4** | Treat compiler warnings as errors in CI | Open | Warnings are **not** errors: CI builds without `-warnaserror`. Baseline = 3 nullable warnings (`StudentController` CS8602/CS8629, `InstructorController` CS8600) that must not grow. | All warnings fixed or accepted | CI build log with zero *new* warnings |
| **D5** | Custom session auth vs. ASP.NET Core Identity | Open | Keep the existing custom BCrypt + session implementation (documented in `README.md` and `CONTEXT.md`). Introducing Identity is out of scope for feature tickets. | Security review concludes either way | Review document + migration plan if changed |
| **D6** | Test database strategy | Open | `LMS_TEST_CONNECTION` environment variable selects the test database; it must be created from `docs/schema/LMS Schema.sql`. CI provisions SQL Server 2022 containers; locally any SQL Server instance with the script applied is valid. | Stable shared test instance or containerized local dev is adopted | Documented, reproducible bootstrap |
| **D7** | CI matrix sharding | Open | Single shard per test job (suites are seconds long). Sharding is added only when a job exceeds ~5 minutes. | Measured CI duration warrants it | CI timing history |
| **D8** | Windows `platform-proof` CI job | Open | None. Windows-specific verification runs locally on this workstation or on demand, not on every PR. | Windows-only behavior appears in code | Red workflow triggered on demand |
| **D9** | Design/visual authority | Open | The Razor views under `Views/` are the shipped UI. `design/prototype/` (gitignored, local-only) is a reference, not an authority — it is superseded (see `README.md`). | Team declares a new visual source (e.g. Figma) | Registered prototype location + fidelity checklist |

## Rules for gated decisions

1. **Build the default.** Code that only works under the *non-default* option is a bug.
2. **Closing a gate is a documentation change plus evidence** — move the settled rule
   into `domain.md` (invariant), `architecture.md` (structure), or `quality.md` (gate),
   then delete the row here.
3. **Opening a gate** requires a row in this table before any code depends on it.
4. Findings referenced above live in `docs/refactoring-analysis/findings.md` (F-01…F-18);
   the refactoring sessions are listed in `docs/refactoring-analysis/README.md`.
