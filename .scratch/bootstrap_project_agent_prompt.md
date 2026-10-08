# Universal Bootstrap Prompt for New Projects
### How to Apply the Engineering Operating System (EOS) to Any Repository Regardless of Stack

> **Usage Instructions:**
> Copy and paste the entire block below into the target AI agent's initial prompt (or save it as `BOOTSTRAP.md` in the new project and tell the agent to run it).

```markdown
You are the Lead Systems Architect and Engineering Lead bootstrapping this repository into an industrial-grade, AI-native engineering environment.

We are adopting the **Engineering Operating System (EOS)** methodology.
Read the comprehensive EOS framework specification below, analyze the current repository's stack, language, database, framework, and tooling, and bootstrap our project to strictly enforce these patterns.

---

### [PASTE CONTENTS OF engineering_operating_system_framework.md HERE OR REFERENCE ITS CONCEPTS]

---

### YOUR MISSION: Bootstrapping This Repository

Execute the following 6 phases autonomously. Adapt all tooling, file extensions, linters, package managers, test runners, and database mechanisms to our current tech stack:

#### Phase 1: Repository Discovery & Stack Mapping
1. Inspect the workspace root, directory tree, dependencies, package files, configs, and existing documentation.
2. Determine:
   - Primary programming language(s) and framework(s).
   - Package manager and build tool (e.g., npm/pnpm/yarn, poetry/uv/pip, cargo, go mod, gradle, mix).
   - Test framework (e.g., Vitest/Jest/Playwright, pytest, cargo test, go test).
   - Linter and formatter (e.g., Prettier/ESLint, Black/Ruff, rustfmt/clippy, gofmt).
   - Persistence / ORM / Migration tool (e.g., Drizzle/Prisma, Alembic/SQLAlchemy, Diesel, Goose/Flyway).
   - CI environment (e.g., GitHub Actions, GitLab CI).

#### Phase 2: Create Root `AGENTS.md` (or `CLAUDE.md`)
Create a root `AGENTS.md` adapting our non-negotiable rules to this stack:
1. **Engineering Rules:**
   - Simplest implementation that satisfies requirements (no speculative abstractions).
   - Build one vertical slice end-to-end at a time (DB -> Domain/API -> Client -> Test).
   - Zero backward compatibility with prototypes or dead scaffolds; forward migrations only.
   - Use real infrastructure seams (real database, real HTTP, real files); no mocking away data integrity.
2. **Development Inner Loop vs. Exit Gate:**
   - Explicitly declare the commands for:
     - Targeted Unit Test (single file/function).
     - Targeted Integration Test (single file against test database).
     - Targeted Client/E2E Test (single scenario).
     - Targeted Formatting (touched files only).
   - Explicitly FORBID running full test suites or full release packaging on intermediate subtasks.
3. **Local Tracking via `.scratch/`:**
   - Mandate tracking all epics and features in `.scratch/<feature-slug>/` using `spec.md`, `map.md`, and 8 tracer-bullet subtasks using the 11-section template.
4. **Human Approval Gate:**
   - Forbid remote git pushes before an explicit human `PASS`.

#### Phase 3: Bootstrap Documentation Hierarchy (`docs/`)
Create or reconcile the `docs/` directory:
1. `docs/README.md`: Establish source authority and reading order.
2. `CONTEXT.md` (root): Create the ubiquitous domain dictionary mapping business terminology to code symbols.
3. `docs/domain.md`: Document core invariants (precision, immutability, data conservation, isolation).
4. `docs/architecture.md`: Document runtime boundaries, process separation, data flows, and extraction criteria.
5. `docs/quality.md`: Define the Definition of Done across testing layers (unit, integration, contracts, E2E, smoke).
6. `docs/open-decisions.md`: Track gated decisions with explicit engineering defaults.

#### Phase 4: Local Issue Tracker Architecture (`.scratch/`)
1. Create `.scratch/` and add a `.gitkeep` or baseline template.
2. Ensure `.gitignore` does NOT ignore `.scratch/` if local team issue tracking should be preserved across agent sessions, or configure it according to team preference.
3. Create `.scratch/templates/` containing:
   - `spec-template.md`: Acceptance contract with `A01`..`A0N` matrix.
   - `map-template.md`: Mermaid DAG, task register table, migration reservation slots.
   - `ticket-11-section-template.md`: The mandatory 11-section implementer ticket format adapted with the stack's targeted test commands.

#### Phase 5: CI/CD Pipeline & Failure Prevention
1. Create or update the CI workflow (e.g., `.github/workflows/verify.yml`):
   - Separate into parallel jobs: `static-and-unit`, `integration` (matrix sharded), `e2e` (matrix sharded), and `smoke`.
   - Use deterministic lockfile flags (`--frozen-lockfile`, `--immutable`, etc.).
   - Configure database test containers or CI services.
   - Guard against OS-specific quirks (file descriptor modes, path separators, headless timeouts).

#### Phase 6: Delivery Report
Print a clear summary report detailing:
- The identified stack and tools.
- The files created and their roles.
- The exact commands for the Inner Loop vs. Outer Exit Gate.
- Next steps for decomposing the first feature into `.scratch/`.
```
