# docs/domain.md — Non-Negotiable Invariants

These are the laws of the Academix LMS domain. No pull request, ticket, or agent may
violate them, regardless of convenience. Status column says where enforcement stands
today; **Partial** means a known defect exists in `docs/refactoring-analysis/findings.md`
— reproduce the invariant in *new* code anyway, never the defect.

## I1 — Credentials are hashes, never plaintext

- Passwords are stored only as BCrypt hashes in `User.PasswordHash`
  (`BCrypt.Net.BCrypt.HashPassword` / `Verify`).
- Plaintext passwords never appear in tracked files, logs, URLs, view models returned to
  clients, or seed data.
- `appsettings.json` ships `BootstrapAdmin:Password` **empty**; a local password lives
  only in the gitignored `appsettings.Development.json`.
- Bootstrap admin provisioning runs only when `IWebHostEnvironment.IsDevelopment()` is
  true and the password is non-empty.

**Status:** Enforced (`Data/SeedData.cs`, `AuthController`, `tests/Lms.Tests/PasswordHashingTests.cs`).

## I2 — One identity, one role vocabulary

- The authenticated identity is the `int` in session key `"UserId"`
  (`SessionHelper`). Nothing else may act as "current user".
- Roles are exactly `"Admin"`, `"Instructor"`, `"Student"` — case-sensitive string
  comparisons; no alias spellings.
- Every portal action keeps its existing `IsLoggedIn` / role gate. Login rejects
  `User.IsActive == false`.
- **Partial (F-08):** the active-account check is not re-verified after login on every
  request; new code must check `IsActive` whenever it loads a user.

**Status:** Partial — see F-08.

## I3 — Exact numeric precision for all academic values

- Points, grades, GPA, percentages, credit hours use `decimal`. `float`/`double` are
  forbidden for any value that is stored, compared, aggregated, or displayed as an
  academic score.
- Derived values (percentages, GPA) are computed from stored exact values, never from
  client-supplied numbers.

**Status:** Enforced (`Grade.Points/MaxPoints/Percentage`, `TranscriptEntry.NumericGrade`,
`CourseEnrollment.FinalGrade` are all `decimal`).

## I4 — The SQL script is the schema

- `docs/schema/LMS Schema.sql` is the single source of truth for tables, views, triggers,
  stored procedures, and functions. It changes **first**, in the same change as the code
  that depends on it.
- There are no EF migrations (decision **D1**). Do not add `Migrations/` folders or call
  `Migrate()` until that gate closes.
- `Database.EnsureCreatedAsync()` cannot create the `vw_*` views or `sp_*` procedures the
  application executes; it is not an upgrade path and must not be treated as one.
- Every schema change is forward-only (additive/rename with data migration in the
  script); never a rollback that destroys live rows.

**Status:** Enforced by process; verified by
`tests/Lms.IntegrationTests/SchemaExistenceTests.cs`.

## I5 — Quiz scoring is server-authoritative

- A submitted response must belong to the submitted question and quiz; option IDs from
  the client are untrusted input.
- Due dates and time limits are enforced by server-side timestamps
  (`QuizAttempt.StartedAt` / `SubmittedAt`), never by browser timers alone.
- Final `QuizAttempt.Score` is computed on the server (currently via
  `sp_SubmitQuizAttempt`) and only after validation.

**Status:** Target — violated today (F-05, F-06). Work is gated behind refactoring
session 5; new code must not add client-trusted scoring.

## I6 — Enrollment, progress, and completion keep their meanings

- `CourseEnrollment` is the record of *membership* in a course
  (`EnrolledAt`, `Status = "Enrolled"`).
- `CourseEnrollment.Status = "Completed"` today doubles as *lesson completion* (F-02).
  Until decision **D3** closes, **no new code may write `CourseEnrollment` rows** for any
  reason other than actual enrollment/progress semantics approved in a ticket.
- `Course.CurrentEnrollment` is a denormalized counter: it must be incremented and
  decremented in the same unit of work as the corresponding `CourseEnrollment` rows.

**Status:** Partial — F-02 open.

## I7 — Records read by a user are scoped to that user

- Students read only their own notifications, grades, attempts, and enrollments.
- Instructors mutate only courses where `Course.InstructorId == current user`.
- Cross-user or cross-role reads/writes are denied, not merely hidden in the UI.

**Status:** Partial — F-03 (ownership), F-04 (unscoped notifications). New queries must
filter by the current user; do not copy the unscoped patterns.

## I8 — Browser-originated mutations are forgery-protected

- Every state-changing POST from a browser carries and validates an anti-forgery token.

**Status:** Partial — F-07 (inconsistent today). New mutating actions must include the
validation.

## I9 — Announcements fan out to enrolled students only

- An announcement creates one `Notification` row per user with an `CourseEnrollment` for
  the course, message prefixed `"[{course.Title}] {message}"`.
- Students see only their own `Notification` rows.

**Status:** Partial — fan-out is correct; scoping on read is F-04.

## I10 — History is preserved, not rewritten

- Generated `Transcript` / `TranscriptEntry` rows are snapshots; they are regenerated by
  creating a new transcript, never by editing old entries in place.
- Deactivation beats deletion: users are flagged `IsActive = false`, courses are
  `"Archived"`; hard deletes require an explicit ticket statement and a forward script
  change in `docs/schema/LMS Schema.sql`.
- Audit-ish timestamps (`CreatedAt`, `EnrolledAt`, `GradedAt`, `StartedAt`, `SubmittedAt`)
  are set once at write time and never back-dated.

**Status:** Enforced by convention; extended by schema triggers in the SQL script.

## I11 — No secret reaches a tracked artifact

- Connection strings with credentials, API keys, and passwords belong in
  `appsettings.Development.json` (gitignored) or environment variables.
- Error pages, logs, and test failure messages must redact credentials (the integration
  tests demonstrate this).

**Status:** Enforced (`tests/Lms.IntegrationTests/TestDatabase` redacts passwords).
