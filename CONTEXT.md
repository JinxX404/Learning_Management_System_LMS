# CONTEXT.md — Ubiquitous Glossary (Academix LMS)

Canonical language for this repository. Every business term maps to **one** code symbol.
Never invent synonyms (`Enrollment` vs `Registration`, `Student` vs `Learner`, `Term` vs
`Semester` are not interchangeable here). If you need a term that is missing, add it to
this file in the same change that introduces the symbol.

## Roles & portals

| Term | Canonical symbol | Notes |
| :--- | :--- | :--- |
| Admin / Instructor / Student | `User.Role` string literal `"Admin"`, `"Instructor"`, `"Student"` | Exact casing; compared with `==` in `AuthController` and role gates. There is no roles table. |
| Portal | Controller + `Views/<Portal>/` folder | `AdminController`, `InstructorController`, `StudentController`. |
| Active account | `User.IsActive` (`bool`) | Login rejects inactive users. Soft delete = `IsActive = false`; rows are not removed. |
| Current user | `SessionHelper.GetUserId(session)` → session key `"UserId"` (`int`) | The only session-held identity. No claims principal. |
| Bootstrap admin | `BootstrapAdmin` config section (`Email`, `Password`) | Provisioned only in Development when the password is non-empty; see finding F-01. |

## Institution & academic structure

| Term | Canonical symbol | Notes |
| :--- | :--- | :--- |
| Institution | `Institution` (`Models/Institution.cs`) | Single-tenant-ish root; every `User` has `InstitutionId`. |
| Academic term | `AcademicTerm` (`Models/AcademicTerm.cs`) | Slang "semester"/"term" both mean this symbol. `IsActive` marks the current term. |
| Course | `Course` (`Models/Course.cs`) | Has `Status` (`"Active"` / `"Archived"`), `CurrentEnrollment`, `MaxEnrollment`, `CreditHours`. |
| Enrollment | `CourseEnrollment` (`Models/CourseEnrollment.cs`) | `Status` values in use: `"Enrolled"`, `"Completed"`. `EnrolledAt` records enrollment time. |
| Course progress / completion | `CourseEnrollment.Status == "Completed"` | **Known defect F-02:** lesson completion currently writes `CourseEnrollment` rows; enrollment and progress are conflated. Do not add new meanings — fix the model instead. |
| Roster | The set of `CourseEnrollment` rows for a `Course` | Rendered via `sp_GetCourseRoster` / `vw_CourseDetails`. |
| Lecture | `Lecture` (`Models/Lecture.cs`) | Unit of course content ordering. |
| Learning asset | `LearningAsset` (`Models/LearningAsset.cs`) | File/link material attached to a lecture. Slang "resource"/"material" = this. |

## Assessment & grading

| Term | Canonical symbol | Notes |
| :--- | :--- | :--- |
| Quiz | `Quiz` | Owned by a course. Question bank = `QuizQuestion` + `QuestionOption`. |
| Attempt | `QuizAttempt` (`AttemptId`) | `SubmittedAt == null` ⇒ in progress; `Score` nullable until grading/submission. |
| Response / answer | `QuizResponse` | One row per answered option; correctness resolved by `vw_QuizResponsesWithCorrectness`. |
| Submit quiz | `sp_SubmitQuizAttempt` stored procedure (raw `EXEC` from `StudentController`) | Scoring must stay server-authoritative; current client-option-ID trust is finding F-05. |
| Auto-save | Persisting `QuizResponse` rows before submit | Distinct from submit; must never compute final score. |
| Grade book | `GradeBook` (`Models/GradeBook.cs`) | Container per course; `Name` + `CourseId`. |
| Grade | `Grade` (`Models/Grade.cs`) | `Points` / `MaxPoints` / `Percentage` as `decimal`, plus `GradableItemType` + `GradableItemId`. |
| Transcript | `Transcript` + `TranscriptEntry` | Generated snapshot: `LetterGrade`, `NumericGrade`, `CumulativeGpa`. |
| Final grade | `CourseEnrollment.FinalGrade` (`decimal?`) | Course-level result; distinct from per-item `Grade`. |
| GPA | `StudentProfile`/`vw_StudentsWithGPAAndCourses` | Maintained by `sp_UpdateStudentGPA`. |

## Communication

| Term | Canonical symbol | Notes |
| :--- | :--- | :--- |
| Notification | `Notification` | Per-user row: `Message`, `IsRead`, `UserId`. |
| Announcement | `Notification` rows created by `InstructorController.CreateAnnouncement` | **Not a separate table.** One row per enrolled student, message prefixed `"[{course.Title}] {message}"`. |
| Notification type / template | `NotificationType`, `NotificationTemplate` | Reference data for channels/templates; announcements do not set a type. |
| Unread count | `Notification.IsRead == false` filter | Student dashboard unread badge. |

## AI features

| Term | Canonical symbol | Notes |
| :--- | :--- | :--- |
| AI model | `Aimodel` | Registry row with `IsActive`. |
| AI-generated content | `AigeneratedContent` | Generated quiz/content artifacts. |
| AI interaction | `Aiinteraction` | Audit of user↔model calls; surfaces in admin reports (`vw_AIInteractionsByUser`). |

## Runtime & configuration

| Term | Canonical symbol | Notes |
| :--- | :--- | :--- |
| Default connection | `ConnectionStrings:DefaultConnection` | SQL Server; local overrides live in `appsettings.Development.json` (gitignored). |
| Test connection | `LMS_TEST_CONNECTION` environment variable | Consumed only by `tests/Lms.IntegrationTests`. |
| Schema source of truth | `docs/schema/LMS Schema.sql` | Views (`vw_*`), procedures (`sp_*`), triggers and tables all live here. |
| Development-only behavior | `IWebHostEnvironment.IsDevelopment()` | Swagger UI and bootstrap admin provisioning. |
| Unit test | `tests/Lms.Tests` | No database, no network. |
| Integration test | `tests/Lms.IntegrationTests` | Real SQL Server; read-only queries only. |

## Vocabulary rules

- Use the exact table/column names from `docs/schema/LMS Schema.sql` in code comments and
  tickets (`CourseEnrollment`, not `EnrollmentRecord`).
- "Delete" in product language means **deactivate** (`IsActive = false`) or
  **archive** (`Course.Status = "Archived"`) unless a ticket explicitly says hard delete.
- "Complete" without qualification is ambiguous: say **lesson complete**
  (`CourseEnrollment.Status = "Completed"` path, see F-02) or **course complete**
  (`FinalGrade` set), never both.
