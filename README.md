# Academix — Learning Management System (LMS)

A web-based Learning Management System built with ASP.NET Core MVC for educational institutions. It provides separate portals for Administrators, Instructors, and Students covering course management, enrollment, content delivery, quizzes with auto-grading, grading, transcripts, and notifications.

Repository: https://github.com/JinxX404/Learning_Management_System_LMS

## Features

### Student Portal
- **Dashboard**: active courses, upcoming deadlines, grade trend charts.
- **My Courses / Course Content**: enrolled course lectures and learning assets.
- **Quizzes**: attempts with **auto-save**, timed submission, auto-graded results.
- **Grades / Announcements / Profile / Account settings**.

### Instructor Portal
- **Course management**: create and edit courses, lectures, and learning assets.
- **Roster**: view enrolled students per course.
- **Quizzes**: create quizzes with multiple question types; question/option bank.
- **Grading**: grade book view and grading workflows.
- **Announcements**: post announcements to students.

### Admin Portal
- **Dashboard**: user, course, enrollment, and AI-generation statistics.
- **User management**: create/edit/soft-delete students and instructors.
- **Academic setup**: academic terms, course creation, enrollment.
- **Reports**: top performers, course pass/fail rates, institution summary.
- **Settings**.

The schema also models AI features (`AIModels`, `AIGeneratedContent`, `AIInteractions`); generated-quiz data is surfaced in admin statistics.

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC (`net8.0`)
- **Data access**: Entity Framework Core 9 (`Microsoft.EntityFrameworkCore.SqlServer`)
- **Database**: SQL Server (LocalDB or a full instance)
- **Auth**: custom session-based authentication — BCrypt password hashes (`BCrypt.Net-Next`), user ID stored in `HttpContext.Session`. **ASP.NET Core Identity is not used.**
- **Frontend**: Razor views, Bootstrap 5, jQuery, Chart.js, vanilla JS/CSS

## Getting Started

### Prerequisites
- .NET 8 SDK (or a later SDK that targets `net8.0`)
- SQL Server (LocalDB or a full instance)

### 1. Clone
```bash
git clone https://github.com/JinxX404/Learning_Management_System_LMS.git
cd Learning_Management_System_LMS
```

### 2. Configure the connection string
Edit `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=LMS;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
Adjust `Server=` for your instance (e.g. `(localdb)\MSSQLLocalDB`).

### 3. Create the database
Run [`docs/schema/LMS Schema.sql`](docs/schema/LMS%20Schema.sql) against your SQL Server (SSMS or `sqlcmd`).

This script creates the `LMS` database with all tables **plus the views, triggers, stored procedures, and functions the application depends on** (e.g. `sp_SubmitQuizAttempt`, `vw_*` report views).

> The app calls `Database.EnsureCreatedAsync()` at startup, which can create tables in an empty database, but it does **not** create views, triggers, or procedures — and it is not a schema-upgrade mechanism. There are no EF migrations. The SQL script is the schema source of truth.

### 4. Run
```bash
dotnet restore
dotnet run
```

### 5. Bootstrap admin (Development only)
On startup in the **Development** environment, the app provisions an admin account from configuration:

```json
"BootstrapAdmin": {
  "Email": "admin@lms.com",
  "Password": ""
}
```

- The tracked `appsettings.json` ships with an **empty password**, so nothing is seeded until you set one.
- Put your local password in `appsettings.Development.json` (gitignored) — never in a tracked file.
- Outside Development, or when the password is empty, no account is created.
- **Change the password after first login.** See finding F-01 in [`docs/refactoring-analysis/findings.md`](docs/refactoring-analysis/findings.md).

## Project Structure

```
├── Controllers/          # Auth, Admin, Instructor, Student, Home, Preview
├── Models/               # EF Core entities + LmsContext
├── ViewModels/           # Request/Response view models (Admin, Instructor, Student, Auth, AI, Shared)
├── Views/                # Razor views, grouped per portal
├── Data/                 # SeedData
├── Helpers/              # SessionHelper
├── wwwroot/              # static assets (Bootstrap, jQuery, JS, CSS)
├── docs/
│   ├── schema/           # LMS Schema.sql — schema source of truth
│   └── refactoring-analysis/  # static code review: risks + refactoring sessions
├── private/              # local-only, gitignored (DB files, archived docs)
└── LMS UI/               # static HTML/CSS prototype, gitignored
```

## Documentation

| Document | Location | Status |
|---|---|---|
| This README | `README.md` | current |
| Database schema (tables, views, triggers, procedures) | `docs/schema/LMS Schema.sql` | current |
| Code review: security/data-integrity findings + refactoring plan | `docs/refactoring-analysis/` | current (Oct 2026 static review) |
| Original project documentation & presentation | `private/archive/` (local only) | historical — as-planned Oct/Dec 2025, superseded by this README |

> The archived `.docx`/`.pptx` were written during planning. Their schema (Assignments/Submissions/Lessons/Roles tables), ".NET 6/7" requirement, and "apply database migrations" instructions do **not** match the implementation.

## Known Issues

`docs/refactoring-analysis/findings.md` documents 18 findings, including: admin bootstrap credentials (now config-driven and Development-only — see F-01), enrollment/authorization gaps, unscoped notification reads, quiz scoring that trusts client option IDs, client-only quiz timers, inconsistent anti-forgery validation, and the `EnsureCreated` vs. SQL-script schema lifecycle split. Read its README for the proposed refactoring sessions before touching those areas.

## Local-only files (never commit)

`private/` (gitignored) holds the local database files (`LMS.mdf`, `LMS_log.ldf`) and archived documents. `.gitignore` also excludes `appsettings.Development.json` (local credentials/overrides), `/Front/`, `/LMS UI/` (static prototypes), `*.mdf`, `*.ldf`, `bin/`, `obj/`, `.vs/`, and `*.user`.
