# Plan: Transition FleetNexus to 100% V3 AI Feature Development Workflow Compliance

This plan establishes the full operational infrastructure, skill ecosystem, artifact hierarchy, and programmatic validation required to achieve 100% compliance with the [V3 AI-Native Software Engineering Operating Model](ai_feature_development_workflow.md).

---

## User Review & Alignment

- **Documentation Consolidation**: Documentation across root `docs/` and `app-fleet-nexus-net/docs/` is consolidated so that root `docs/` serves as the single source of truth for ADRs, feature contracts, and architecture.
- **Script Environment**: Validation scripts are implemented in **PowerShell (`.ps1`)** and standard CLI tools to natively integrate with Windows workspaces and CI/CD pipelines.
- **Documentation Migration**: All existing in-flight plans and task lists (`AlphaRegistrationImplementationPlan.md`, `VehicleInventoryTasks.md`, `DashboardKPIImplementationPlan.md`, `SignupLoginImplementationPlan.md`) will be formally migrated and refactored into the compliant `/docs/features/FEATURE-XXX/` directory structure.
- **Git Worktree Isolation**: Git worktrees will default to a local `.worktrees/` directory in the workspace root (ignored by `.gitignore`), allowing AI agents to work on isolated task branches without disrupting your active editor or primary branch.

---

## Proposed System Architecture

```
fleet_solution/
├── .agents/
│   └── skills/
│       ├── record-adr/                    [EXISTS - KEEP/UPDATE]
│       ├── feature-discovery/             [NEW]
│       ├── architecture-analysis/         [NEW]
│       ├── implementation-planning/       [NEW]
│       ├── task-decomposition/            [NEW]
│       ├── test-design/                   [NEW]
│       ├── coding/                        [NEW]
│       └── code-review/                   [NEW]
├── docs/
│   ├── adr/                               [CONSOLIDATED]
│   ├── features/                          [NEW DIRECTORY]
│   │   ├── _templates/                    [NEW TEMPLATES]
│   │   │   ├── requirements.template.md
│   │   │   ├── design.template.md
│   │   │   ├── implementation-plan.template.md
│   │   │   ├── test-plan.template.md
│   │   │   ├── task.template.md
│   │   │   ├── status.template.md
│   │   │   └── evidence.template.md
│   │   ├── FEATURE-001-auth-signup-login/ [MIGRATED FEATURE]
│   │   ├── FEATURE-002-vehicle-inventory/ [MIGRATED FEATURE]
│   │   └── FEATURE-003-dashboard-kpi/     [MIGRATED FEATURE]
│   └── workflow/
│       ├── ai_feature_development_workflow.md
│       ├── v3_workflow_implementation_plan.md
│       └── model_capabilities.md          [NEW CAPABILITY MATRIX]
└── scripts/
    ├── validate-adr.ps1                   [NEW VALIDATOR]
    ├── validate-feature-state.ps1         [NEW VALIDATOR]
    ├── validate-task.ps1                  [NEW VALIDATOR]
    ├── validate-traceability.ps1          [NEW VALIDATOR]
    ├── validate-evidence.ps1              [NEW VALIDATOR]
    └── worktree-helper.ps1                [NEW WORKTREE AUTOMATOR]
```

---

### Phase 1: Foundation & Governance Artifacts

Establish the core directory layout, standard templates, and discovery/architecture skills.

#### 1.1 Templates Directory (`docs/features/_templates/`)
Standardized, schema-driven templates for feature lifecycle artifacts:
- `requirements.template.md`: Problem, Actors, Happy Path, Alternate Scenarios, Boundary Conditions, Failure Semantics, Acceptance Criteria (`AC-xxx`), Open Questions, Status.
- `design.template.md`: Current vs. Proposed Architecture, Component impact, Reused vs. New patterns, Failure/Retry/Idempotency, ADR references.
- `implementation-plan.template.md`: Component/File delta, Schema migrations, Config/Infra, Definition of Ready (DoR).
- `test-plan.template.md`: Requirements mapping, Negative/Boundary/Failure/Security test matrix (`TEST-xxx` $\leftrightarrow$ `AC-xxx`).
- `task.template.md`: Atomic task contract with dependencies, expected files, required tests, and Definition of Done (DoD).
- `evidence.template.md`: Machine-verifiable evidence table (`VERIFIED`, `SUPPORTED`, `UNVERIFIED`, `CONFLICT`).
- `status.template.md`: State tracking (`INTAKE`, `DISCOVERY`, `REQUIREMENTS_REVIEW`, `ARCHITECTURE_REVIEW`, `PLANNING`, `TEST_DESIGN`, `IMPLEMENTATION`, `VERIFICATION`, `REVIEW`, `APPROVED`, `MERGED`).

#### 1.2 Feature Discovery Skill (`.agents/skills/feature-discovery/SKILL.md`)
Skill for the **Discovery Agent**:
- Focuses on business problem elicitation, boundary conditions, edge cases, and non-functional requirements.
- Strictly prohibits inventing unverified business rules; forces questions/decisions into the `requirements.md` artifact.
- Generates `requirements.md` and halts for **Human Approval Gate 1**.

#### 1.3 Architecture Analysis Skill (`.agents/skills/architecture-analysis/SKILL.md`)
Skill for the **Architecture Agent**:
- Evaluates existing repository patterns, database schemas, and active ADRs.
- Determines whether existing patterns suffice or triggers `record-adr` to draft a new ADR.
- Produces `design.md` and halts for **Human Approval Gate 2**.

---

### Phase 2: Planning, Decomposition & Independent Test Design

Provide tools for breaking down features and generating test suites derived from requirements rather than code.

#### 2.1 Implementation Planning Skill (`.agents/skills/implementation-planning/SKILL.md`)
Skill for the **Planning Agent**:
- Transforms approved requirements and design into concrete technical files/services changes.
- Validates the **Definition of Ready (DoR)** checklist.

#### 2.2 Task Decomposition Skill (`.agents/skills/task-decomposition/SKILL.md`)
Skill for breaking the implementation plan into ordered, atomic task documents under `docs/features/FEATURE-XXX/tasks/001-*.md`.
- Enforces strict context contracts (identifying exact files, interfaces, and constraints per task).

#### 2.3 Independent Test Design Skill (`.agents/skills/test-design/SKILL.md`)
Skill for the **Test Design Agent**:
- Generates test cases solely from acceptance criteria, data contracts, and failure semantics before implementation begins.
- Assigns unique `TEST-xxx` identifiers linked to `AC-xxx` / `REQ-xxx`.

---

### Phase 3: Controlled Execution, Worktrees & Evidence Collection

Ensure developers work in isolation and capture verifiable proof.

#### 3.1 Developer Skill (`.agents/skills/coding/SKILL.md`)
Skill for the **Developer Agent**:
- Governs atomic task execution with least-privilege context contracts.
- Strictly bounds file modification to the task contract.
- Runs targeted tests in .NET / frontend and generates evidence snippets.

#### 3.2 Worktree Automator (`scripts/worktree-helper.ps1`)
PowerShell utility to create, switch, and tear down isolated Git worktrees for tasks (e.g., `git worktree add .worktrees/FEATURE-123-task-001 feature/FEATURE-123`).

---

### Phase 4: Reconciliation, Review & Programmatic Validation

Automate the enforcement of workflow gates and structured reviews.

#### 4.1 Code Review Skill (`.agents/skills/code-review/SKILL.md`)
Skill for the **Review Agent**:
- Evaluates code against requirements and ADRs.
- Implements Deviation Management (Class A: auto-reconcile; Class B: design flag; Class C/D: BLOCK).
- Emits structured review reports with status (`VERIFIED`, `CONFLICT`, `BLOCKED`).

#### 4.2 Programmatic Validation Suite (`scripts/`)
- `scripts/validate-adr.ps1`: Ensures all ADRs follow naming, status headers, and master index consistency.
- `scripts/validate-feature-state.ps1`: Enforces preconditions for state transitions (e.g., cannot move to `IMPLEMENTATION` without `APPROVED` requirements and resolved ADRs).
- `scripts/validate-task.ps1`: Checks that task contracts contain dependencies, acceptance criteria, and expected file bounds.
- `scripts/validate-traceability.ps1`: Verifies that every `REQ-` has a corresponding `AC-`, `TASK-`, and `TEST-`.
- `scripts/validate-evidence.ps1`: Checks that evidence logs contain passing test executions and line-number pointers.

---

### Phase 5: Multi-Model Capabilities Matrix & Pilot Rollout

#### 5.1 Multi-Model Capabilities Matrix (`docs/workflow/model_capabilities.md`)
Defines model capabilities and assignment strategies for Discovery, Architecture, Coding, and Review roles.

#### 5.2 Legacy Documentation Migration
Migrate existing in-flight plans:
- `SignupLoginImplementationPlan.md` + `SignupLoginTasks.md` $\rightarrow$ `docs/features/FEATURE-001-auth-signup-login/`
- `Inventory_Vehicle.md` + `VehicleInventoryTasks.md` $\rightarrow$ `docs/features/FEATURE-002-vehicle-inventory/`
- `DashboardKPIImplementationPlan.md` $\rightarrow$ `docs/features/FEATURE-003-dashboard-kpi/`
- `AlphaRegistrationImplementationPlan.md` $\rightarrow$ `docs/features/FEATURE-004-alpha-registration/`

---

## Verification Plan

### Automated Preflight & Validation Checks
1. Run `scripts/validate-adr.ps1` against existing 21 ADRs.
2. Run `scripts/validate-feature-state.ps1` against migrated feature folders.
3. Run `scripts/validate-traceability.ps1` to test the traceability graph.
4. Execute `dotnet test` and ensure all test runs produce parseable evidence.

### Manual Review & Validation
- Verify that every new skill in `.agents/skills/` appears and triggers correctly in the AI assistant.
- Test the full workflow gate flow on a migrated feature from `Human Intent` $\to$ `Requirements Gate` $\to$ `ADR Gate` $\to$ `Evidence` $\to$ `Review`.
