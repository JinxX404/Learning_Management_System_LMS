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
- **Quizzes**: create quizzes with multiple question types (questions and options are authored inline per quiz).
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
- **Tests**: xUnit — `tests/Lms.Tests` (unit, no database) and `tests/Lms.IntegrationTests` (real SQL Server via `LMS_TEST_CONNECTION`)
- **CI**: GitHub Actions — `.github/workflows/verify.yml` (format/lint/build/unit → integration → smoke → verify-gate)
- **Working agreement**: [`AGENTS.md`](AGENTS.md) defines the engineering rules and the exact inner-loop vs. exit-gate commands

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
The tracked `appsettings.json` ships a working default:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=LMS;Trusted_Connection=True;"
}
```

Adjust `Server=` for your instance (e.g. `(localdb)\MSSQLLocalDB`). Environment-specific overrides — including `TrustServerCertificate=True` for a local instance with a self-signed certificate — belong in `appsettings.Development.json`, which is gitignored, so the certificate bypass never ships in tracked config.

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

## Verification

```bash
dotnet restore --locked-mode
dotnet format "Learning Management System.sln" --verify-no-changes    # format gate
dotnet build "Learning Management System.sln" --no-restore             # typecheck
dotnet test tests/Lms.Tests                                            # unit (no DB)
$LMS_TEST_CONNECTION="Server=.;Database=LMS;Trusted_Connection=True;TrustServerCertificate=True" \
  dotnet test tests/Lms.IntegrationTests                               # real SQL Server
```

`AGENTS.md` §2 defines the fast inner loop (single-file filters, touched files only) and
the full pre-flight exit gate; `.github/workflows/verify.yml` runs the exit gate on every
push/PR (format → lint → build → unit → integration → smoke → verify-gate).

## Project Structure

```
├── Controllers/          # Auth, Admin, Instructor, Student, Home, Preview
├── Models/               # EF Core entities + LmsContext
├── ViewModels/           # Request/Response view models (Admin, Instructor, Student, Auth, AI, Shared)
├── Views/                # Razor views, grouped per portal
├── Data/                 # SeedData
├── Helpers/              # SessionHelper
├── wwwroot/              # static assets (Bootstrap, jQuery, JS, CSS)
├── tests/                # xUnit projects: Lms.Tests (unit), Lms.IntegrationTests (real SQL Server)
├── .github/workflows/    # verify.yml — format, lint, build, unit, integration, smoke
├── .scratch/             # local issue tracker: specs, maps, 8-task tickets, templates (tracked in git)
├── docs/
│   ├── README.md         # source authority + reading order
│   ├── schema/           # LMS Schema.sql — schema source of truth
│   ├── domain.md         # invariants · architecture.md · quality.md · open-decisions.md
│   └── refactoring-analysis/  # static code review: risks + refactoring sessions
├── AGENTS.md             # operating constitution for agents (rules + commands)
├── CONTEXT.md            # ubiquitous glossary: business term → code symbol
├── design/prototype/     # static screen designs + HTML prototype, gitignored
└── private/              # local-only, gitignored (DB files, archived docs)
```

## Documentation

| Document | Location | Status |
|---|---|---|
| This README | `README.md` | current |
| Operating constitution (rules, inner-loop/exit-gate commands, approval gate) | `AGENTS.md` | current |
| Ubiquitous glossary (term → code symbol) | `CONTEXT.md` | current |
| Documentation map & reading order | `docs/README.md` | current |
| Invariants | `docs/domain.md` | current |
| Boundaries & data flows | `docs/architecture.md` | current |
| Definition of Done & verification matrix | `docs/quality.md` | current |
| Gated decisions + engineering defaults | `docs/open-decisions.md` | current |
| Database schema (tables, views, triggers, procedures) | `docs/schema/LMS Schema.sql` | current |
| CI pipeline (format, lint, build, unit, integration, smoke) | `.github/workflows/verify.yml` | current |
| Code review: security/data-integrity findings + refactoring plan | `docs/refactoring-analysis/` | current (Oct 2026 static review) |
| Active epics, specs, task tickets | `.scratch/issue-NN-*/` | per feature |
| Subtask evidence (test logs, exit codes, screenshots) | `evidence/issue-NN/` | per epic |
| Screen designs & static HTML prototype | `design/prototype/` (local only) | superseded by `Views/`, kept as design reference |
| Original project documentation & presentation | `private/archive/` (local only) | historical — as-planned Oct/Dec 2025, superseded by this README |

> The archived `.docx`/`.pptx` were written during planning. Their schema (Assignments/Submissions/Lessons/Roles tables), ".NET 6/7" requirement, and "apply database migrations" instructions do **not** match the implementation.

## Known Issues

`docs/refactoring-analysis/findings.md` documents 18 findings, including: admin bootstrap credentials (now config-driven and Development-only — see F-01), enrollment/authorization gaps, unscoped notification reads, quiz scoring that trusts client option IDs, client-only quiz timers, inconsistent anti-forgery validation, and the `EnsureCreated` vs. SQL-script schema lifecycle split. Read its README for the proposed refactoring sessions before touching those areas.

## Local-only files (never commit)

`private/` (gitignored) holds the local database files (`LMS.mdf`, `LMS_log.ldf`) and archived documents. `.gitignore` also excludes `appsettings.Development.json` (local credentials/overrides), `/design/prototype/` and `/Front/` (static prototypes and screen designs), `*.mdf`, `*.ldf`, `bin/`, `obj/`, `.vs/`, and `*.user`.
