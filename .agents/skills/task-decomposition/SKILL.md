---
name: task-decomposition
description: >-
  Use after implementation plan and test design are complete to break the feature down into
  atomic, sequentially ordered task contracts in docs/features/FEATURE-XXX/tasks/001-*.md.
---

# Task Decomposition Skill

This skill governs the Deconstruction of Implementation Plans into Atomic, Isolated Task Contracts.

---

## 1. Operating Rules & Boundaries

1. **Atomic Task Size**: Each task must represent a cohesive, self-contained unit of work that an agent can execute within a narrow context window (e.g. 1–3 related files).
2. **Strict Context Contracts**: Every task contract must explicitly declare the exact files in scope, required tests, dependencies, and Definition of Done (DoD).
3. **Dependency Ordering**: Tasks must be sequentially numbered (e.g. `001-data-models.md`, `002-service-layer.md`, `003-api-endpoints.md`, `004-ui-view.md`, `005-verification.md`).
4. **Artifact Production**: Write task files to `docs/features/FEATURE-XXX/tasks/###-[kebab-task-title].md` and update `status.md` to `READY_FOR_IMPLEMENTATION`.

---

## 2. Standard Task Decomposition Pattern

```
docs/features/FEATURE-XXX/tasks/
├── 001-data-models-and-migrations.md
├── 002-service-interfaces-and-logic.md
├── 003-api-controllers-and-dtos.md
├── 004-ui-components-and-forms.md
└── 005-integration-tests-and-evidence.md
```

---

## 3. Step-by-Step Task Authoring Workflow

### Step 1: Establish Sequential Layers
Break down changes from the lowest dependency layer upward:
1. **Data / Schema**: Entities, DbContext configurations, EF migrations.
2. **Domain / Services**: Core business logic, validation, event dispatching.
3. **API / Contracts**: Controllers, DTO mappings, authorization attributes.
4. **UI / Frontend**: React/Vite components, API services, routing.
5. **Verification**: End-to-end integration tests and evidence matrix completion.

### Step 2: Write Each Task Contract (`tasks/###-*.md`)
Copy `docs/features/_templates/task.template.md` and populate:
- **Task ID**: e.g. `TASK-001`.
- **Dependencies**: List predecessor tasks that must be marked `VERIFIED` first.
- **Traceability**: Connect to target `REQ-xxx`, `AC-xxx`, `ADR-xxx`, and `TEST-xxx`.
- **Files In-Scope**: Explicit whitelist of files the developer agent is allowed to edit.
- **Definition of Done (DoD)**:
  - Code compiles without warnings.
  - Specified unit tests pass.
  - No unintended file modifications.

### Step 3: Transition State Machine
Update `docs/features/FEATURE-XXX/status.md` to `READY_FOR_IMPLEMENTATION`.
The feature is now ready for the Developer Agent and Git Worktree execution.
