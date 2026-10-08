# Implementer Mission: Task NN — <Task Title>

> Copy to `.scratch/issue-NN-<feature-slug>/issues/NN-<slug>.md`. All 11 sections are
> mandatory; section 11 is filled in by the implementer at handoff.

## 1. Implementer Mission & Instructions

- Worktree: repository root, base branch `<branch>`, starting commit `<sha>`.
- **Mandatory reading before authoring code:** `AGENTS.md`, `CONTEXT.md`,
  `docs/domain.md`, `docs/architecture.md`, then this issue's `spec.md` and `map.md`.
- Work only inside the declared files (section 4). If a file outside the list becomes
  unavoidable, stop and report back instead of expanding scope.

## 2. User Story

- As a `<role>`, I want `<capability>`, so that `<business outcome>`.

## 3. Source Requirements & Invariants

- Governing acceptance IDs: `A0x`, `A0y` (from `spec.md`).
- Invariants applied: `docs/domain.md` Ix — <how this task upholds each>.
- Boundary constraints: `docs/architecture.md` §<n> — <which data flow/trust boundary>.

## 4. Scope of Changes (exhaustive)

- Database: `docs/schema/LMS Schema.sql` — <objects, or "unchanged">
- Backend: `Controllers/<X>Controller.cs`, `Models/…`, `Data/…`, `Helpers/…` — <details>
- Contracts: `ViewModels/<Area>/…` — <details>
- Frontend: `Views/<Portal>/…`, `wwwroot/css/…`, `wwwroot/js/…` — <details>
- Tests: `tests/Lms.Tests/…`, `tests/Lms.IntegrationTests/…` — <which test files>

Anything not listed here is out of scope.

## 5. Acceptance Scenarios

- **Happy path:** <end-to-end steps and observable result>
- **Security & permissions:** <allow/deny rows; session role checks preserved>
- **UI & accessibility:** 1280×800 viewport, light + dark themes, keyboard-only path,
  no native dialogs, feature-scoped CSS (see `docs/quality.md` §6).
- **Failure & recovery:** <invalid input, expired session, concurrent edit, partial
  write rollback>
- **Independent proof:** <exact assertion/oracle: values, counts, state transitions>

## 6. Targeted Test Scope (Inner Loop Only)

- **STRICT INNER-LOOP BUDGET (< 35 min). FORBIDDEN:** solution-wide `dotnet test`,
  full `dotnet format` without `--include`, `dotnet publish`, release packaging.
- Allowed commands, exactly these:

```powershell
# Typecheck (incremental compile — this IS the typecheck)
dotnet build "Learning Management System.sln" --no-restore

# Format only touched files
dotnet format "Learning Management System.sln" --include <path/One.cs> <path/Two.cs>

# Single unit test file
dotnet test tests/Lms.Tests --no-restore --filter "FullyQualifiedName~<TestClass>"

# Single unit test method
dotnet test tests/Lms.Tests --no-restore --filter "FullyQualifiedName~<TestClass>.<method>"

# Single integration test file (real SQL Server)
$LMS_TEST_CONNECTION = "Server=.;Database=LMS;Trusted_Connection=True;TrustServerCertificate=True"
dotnet test tests/Lms.IntegrationTests --no-restore --filter "FullyQualifiedName~<TestClass>"

# Browser/E2E single scenario: none available (decision D2) — use manual protocol §8
```

## 7. Risks & Invariant Protections

- Data integrity: <which invariant could break here, and how the change prevents it>
- Concurrency: <lock/transaction/optimistic behavior, or "N/A — single-row insert">
- Security: <role checks kept, input validated, secrets not logged>
- Blast radius: <which screens/CSS/selectors must remain untouched and how you verify>

## 8. Manual Test Protocol & User Approval Gate

1. `dotnet run` and sign in as `<role>` (`<email>`; password from
   `appsettings.Development.json`, never from a tracked file).
2. <step-by-step navigation to the changed screen>
3. Verify: <viewport, tab order, dark theme, error state, data correctness>
4. Regression glance: <one unrelated screen that must render unchanged>

**Mandatory halt.** Keep the application running and visible, then ask verbatim:

> "Task NN automated checks and manual rehearsal complete. Please review the
> application state and evidence. Do you approve: PASS or FAIL?"

## 9. Handoff Protocol

- Record evidence in `evidence/issue-NN/task-NN-evidence.md` (commands + exit codes +
  screenshots).
- Update `map.md` task status and this ticket's status; fill `handoff-summary.md`.
- Local conventional commit: `feat(<module>): <description> (Task NN)` (or
  `fix`/`test`/`docs`/`chore`).
- **STRICT REMOTE PUSH POLICY:** do not `git push` before an explicit human `PASS`.
  Commits stay local.

## 10. Completion Evidence

- Artifact paths, test output counts, exit codes, screenshots (1280×800, dark theme if
  applicable), manual PASS record.

## 11. Answer

- Initialized to: `Pending implementation and explicit human initiation.`
