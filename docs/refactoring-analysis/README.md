# Refactoring analysis

## Architecture overview

This is a single ASP.NET Core MVC application targeting .NET 8. `Program.cs` wires Razor MVC, SQL Server through EF Core, and ASP.NET session state. Authentication is custom: `AuthController` verifies BCrypt password hashes and stores a user ID in session; controllers then repeat role/session checks themselves. Admin, instructor, and student workflows live in three large controllers that query and mutate `LmsContext` directly, shape view data through a mix of view models and `ViewBag`, and render Razor views. `LmsContext` contains both table entities and keyless mappings to SQL views. The SQL Server schema, views, triggers, and stored procedures are also represented in `LMS Schema.sql`; the `design/prototype/` tree contains static screen designs separate from the MVC views served from `wwwroot`.

## Main risks

- **Authorization and record integrity:** student progress can create course enrollments; course content does not enforce enrollment; student announcements query notifications globally; and quiz answers are not checked against their question before scoring. See [F-02–F-05](findings.md#confirmed-findings).
- **Security baseline:** a known administrator password is seeded, several mutating actions do not validate anti-forgery tokens, and active-account checks are not consistently made after login. See [F-01, F-07, F-08](findings.md#confirmed-findings).
- **Database delivery:** startup uses `EnsureCreated`, while runtime code depends on views and a stored procedure held in a separate SQL script. No EF migration files were found in the project scan. See [F-09](findings.md#confirmed-findings).
- **Change safety:** multi-step quiz and profile writes can leave partial records; controller input validation is inconsistent; and controller responsibilities are tightly coupled. See [F-10–F-12](findings.md#confirmed-findings).

## Reports

- [Detailed findings](findings.md) — evidence, impact/confidence, recommendations, learning notes, incremental refactoring steps, and follow-up verification items.

## Proposed refactoring sessions

Keep each session reviewable and behavior-preserving where possible. Record current behavior in tests before changing shared flows.

1. **Immediate credential and test baseline — F-01, test gap.** Replace the predictable administrator bootstrap path with an explicit, deployment-safe provisioning mechanism. Add a repeatable test database and regression tests for login, role access, notifications, enrollment, and quiz scoring. Do not wait for later sessions to stop using the known credential in a deployed environment.
2. **Choose one database lifecycle — F-09.** Decide whether EF migrations or a versioned SQL deployment is the schema source of truth. Bring views, triggers, and procedures into that deployment path and document clean install/upgrade steps. This is a prerequisite for any database-shape changes in later sessions.
3. **Close access and request-forgery gaps — F-03, F-04, F-07, F-08.** Centralize current-user/role/active-account checks, enforce ownership/enrollment at query boundaries, scope notification reads, and validate anti-forgery tokens on every browser-originated mutation. Add tests for cross-user and cross-role requests.
4. **Repair enrollment and completion semantics — F-02.** Separate lesson completion from course enrollment, reject progress updates for non-enrolled users, and verify that course capacity/counts and enrollment status remain correct. Depends on the test database from session 1 and may require session 2 first if schema changes are needed.
5. **Make quiz rules server-authoritative — F-05, F-06.** Validate that each selected option belongs to the submitted question and quiz; enforce due dates and time limits in save/submit actions rather than relying on browser timers. Regression-test the full autosave/submit/result path against the deployed SQL behavior. Depends on sessions 1–2.
6. **Make multi-step writes and inputs safe — F-10, F-11.** Validate request models and cross-entity references, then make quiz/profile creation atomic. Test invalid input, duplicate email, database constraint failures, and retry behavior.
7. **Reduce coupling and maintenance drag — F-12 and low-priority items.** Extract one workflow at a time from the large controllers after behavior is covered; replace weakly typed view data where it helps, add pagination to growing admin lists, reconcile duplicate index metadata, and remove confirmed orphan assets. Refresh setup/authentication documentation as part of these changes.

## Review and validation notes

This is a static review of the checked-in source, SQL script, Razor views, and project documentation. The path scan found no test project or test source files; the former `TestController.cs` (an application controller with a `Ping` action, not a test suite) has since been removed. I did not run a build, test suite, or database integration check. A terminal inspection attempt could not start in the sandbox and was not approved to run outside it, so database/runtime behavior remains unverified where called out in the findings.