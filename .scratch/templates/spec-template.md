# spec.md — Epic Specification & Acceptance Contract

> Copy to `.scratch/issue-NN-<feature-slug>/spec.md`. Fill every section before any
> code is written. `NN` is the two-digit issue number; `<feature-slug>` is kebab-case.

# Issue NN — <Feature title>

| Field | Value |
| :--- | :--- |
| Status | Draft → Approved → In progress → Done |
| Owner | <human> |
| Authoring agent | <agent/session> |
| Related findings | `docs/refactoring-analysis/findings.md` F-xx (or: none) |
| Related decisions | `docs/open-decisions.md` Dxx (or: none) |
| Schema change | Yes / No → if Yes, `docs/schema/LMS Schema.sql` is updated first (I4) |

## 1. Problem statement

<What is broken or missing today? Cite concrete symptoms, file paths, or finding IDs.>

## 2. Scope

### In scope

- <bullet per deliverable, each mapped to an acceptance id below>

### Explicitly out of scope

- <things agents must NOT re-invent or touch — reference exclusions and open decisions>

## 3. Invariants this epic must respect

- `docs/domain.md` Ix: <which invariants apply and why>
- `docs/architecture.md` boundaries touched (if any): <none / which>

## 4. Acceptance contract (A-matrix)

Every row is testable evidence. `Proof` is the artifact that closes it.

| ID | Requirement (formal wording) | Layer | Proof |
| :--- | :--- | :--- | :--- |
| A01 | **Required:** <precise, verifiable statement> | Unit | `tests/Lms.Tests/…` |
| A02 | **Required:** <statement> | Integration | `tests/Lms.IntegrationTests/…` |
| A03 | **Required:** <statement> | UI | Manual protocol / screenshot |
| A04 | **Required:** <statement> | Smoke | CI `smoke` job |
| A05 | **Required:** <allow/deny matrix row> | Security | Unit test or manual matrix |
| A06 | **Provisional target:** <performance/usability target> | Non-functional | Measurement note |
| A07 | **Deferred:** <postponed item — data hooks preserved, no UI/API> | — | Recorded decision |
| A08 | **Excluded:** <permanently out of scope — must not appear in menus/routes/schema> | — | Reviewed in diff |

Vocabulary is fixed by `docs/README.md` §2.2 (`Required`, `Provisional target`,
`Release-gated`, `Deferred`, `Excluded`).

## 5. Security & permission matrix

| Actor | Action | Expected |
| :--- | :--- | :--- |
| Anonymous | <action> | Redirect to `/Auth/Login` |
| Student | <action> | <allowed/denied> |
| Instructor (owner) | <action> | <allowed> |
| Instructor (non-owner) | <action> | Denied (I7) |
| Admin | <action> | <allowed> |

## 6. Failure & recovery scenarios

- <concurrency / invalid input / expired session / partial write — expected observable behavior>

## 7. Migration reservations

| Object | Type | Direction | Ticket that pays it |
| :--- | :--- | :--- | :--- |
| `<Table>` | Table | Additive forward change in `docs/schema/LMS Schema.sql` | Task 01 |
| `<view/sp>` | View/Procedure | Create or alter | Task 01 |

## 8. Exit criteria

- [ ] All `Required` rows in the A-matrix have evidence.
- [ ] Exit-gate commands green (see `docs/quality.md` §3).
- [ ] Human `PASS` recorded in `evidence/issue-NN/`.
