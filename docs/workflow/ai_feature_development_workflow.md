## From AI Prompts to an AI-Native Software Engineering Operating Model

---

# 1. Executive Summary

The goal of this initiative is not simply to use AI to write code faster.

The goal is to create a **repeatable, controlled, traceable software engineering workflow in which AI assists with the entire feature lifecycle**:

> Human Intent → Requirements → Architecture → Plan → Tasks → Implementation → Tests → Evidence → Review → Merge

The AI should handle the majority of the administrative and analytical work while humans retain authority over:

- Business intent
- Business decisions
- Architectural decisions
- Significant requirement changes
- Final acceptance

The repository becomes the persistent source of engineering context, while AI agents operate against explicit artifacts, permissions, workflow states, and evidence.

The central philosophy is:

> **The goal is not to minimize documentation. The goal is to minimize human administrative work while maximizing machine-verifiable engineering evidence.**

The AI should own the bureaucracy.

The human should own the decisions.

---

# 2. What V3 Changes

The earlier workflow was primarily a sequential pipeline:

1. Discover
2. Design
3. Plan
4. Break into tasks
5. Implement
6. Test

The V3 model keeps those concepts but introduces several important controls.

### V3 introduces:

1. **Complexity/risk-based routing**
2. **Artifact-backed workflow**
3. **Explicit human approval gates**
4. **Agent capability boundaries**
5. **Context contracts**
6. **Evidence-based verification**
7. **Deviation detection**
8. **Contradiction detection**
9. **Git/worktree isolation**
10. **Programmatic workflow enforcement**
11. **Traceability from requirements to tests**
12. **Reconciliation when implementation differs from design**
13. **Model independence**

This changes the system from:

> "A collection of AI prompts"

into:

> **"An AI-native software engineering operating model."**

---

# 3. Core Principles

## 3.1 Artifact First

Important engineering knowledge should not live exclusively inside a chat conversation.

AI should create durable artifacts in the repository.

Examples:

```text
/docs/features/FEATURE-123/
    requirements.md
    design.md
    implementation-plan.md
    test-plan.md
    status.md
    tasks/
        001-domain.md
        002-service.md
        003-api.md

And:

/docs/adr/
    ADR-0012-example.md
This gives the project:Persistent contextAuditabilityTraceabilityReviewabilityReproducibilityReduced dependence on AI conversation history4. Complexity-Based Workflow RoutingNot every change deserves the same ceremony.A two-line typo fix should not trigger a complete architectural workflow.At the same time, a seemingly small change may have major architectural implications.Therefore, V3 should not use rigid feature tiers based solely on size.Instead, classify changes based on:Business impactArchitectural impactData impactSecurity impactIntegration impactOperational impactTesting complexityRegulatory/compliance impactFailure/recovery implicationsExample classification:LOWExamples: UI wording, logging improvement, simple configuration change, isolated bug fix.Workflow: Intake → Lightweight Analysis → Implementation → Tests → ReviewSTANDARDExamples: New API endpoint, new business rule, new service behavior, database change, integration modification.Workflow: Discovery → Requirements → Architecture Check → Implementation Plan → Test Design → Task Breakdown → Implementation → Verification → ReviewSIGNIFICANTExamples: Cross-system workflow, major domain change, new architectural pattern, security-sensitive capability, significant data migration, major integration, new infrastructure component.Workflow: Deep Discovery → Requirements → Architecture Analysis → ADR → Human Approval → Implementation Plan → Test Strategy → Task Breakdown → Controlled Implementation → Evidence → Reconciliation → Review → Human Approval → MergeThe classification itself should be recorded:YAMLfeature: FEATURE-123
complexity: significant
reason:
  - cross-system integration
  - new retry strategy
  - persistent state change
architectural_review_required: true
adr_required: true
5. End-to-End WorkflowPlaintext                         ┌────────────────────┐
                         │      HUMAN         │
                         │  Feature Intent    │
                         └─────────┬──────────┘
                                   │
                                   ▼
                         ┌────────────────────┐
                         │   Change Intake    │
                         └─────────┬──────────┘
                                   │
                                   ▼
                       ┌─────────────────────────┐
                       │ Complexity / Risk       │
                       │ Classification          │
                       └──────────┬──────────────┘
                                  │
                 ┌────────────────┴────────────────┐
                 │                                 │
                 ▼                                 ▼
          Lightweight Flow                  Full Discovery
                 │                                 │
                 │                                 ▼
                 │                          Requirements
                 │                                 │
                 │                                 ▼
                 │                           HUMAN GATE
                 │                                 │
                 │                                 ▼
                 │                         Architecture Analysis
                 │                                 │
                 │                                 ▼
                 │                            ADR Check
                 │                                 │
                 │                                 ▼
                 │                           HUMAN GATE
                 │                                 │
                 └────────────────┬────────────────┘
                                  │
                                  ▼
                         Implementation Plan
                                  │
                                  ▼
                           Test Design
                                  │
                                  ▼
                         Task Decomposition
                                  │
                                  ▼
                         ┌─────────────────┐
                         │ Atomic Task     │
                         │ Execution       │
                         └────────┬────────┘
                                  │
                                  ▼
                           Code + Tests
                                  │
                                  ▼
                              Evidence
                                  │
                                  ▼
                        Deviation Detection
                                  │
                    ┌─────────────┴─────────────┐
                    │                           │
              No conflict                  Conflict
                    │                           │
                    ▼                           ▼
                  Review                 Reconciliation
                    │                           │
                    │                           ▼
                    │                     Updated Artifacts
                    │                           │
                    └─────────────┬─────────────┘
                                  │
                                  ▼
                            HUMAN REVIEW
                                  │
                                  ▼
                                MERGE
6. DiscoveryThe Discovery Agent should behave less like a chatbot and more like an experienced business analyst.The objective is not to immediately propose implementation.The objective is to understand the problem.Discovery should establish:Business Problem: What problem are we solving? Who experiences the problem? Why is it important? What happens today?Happy Path: What should happen normally? What are the expected inputs? What is the expected output?Alternate Paths: What happens when something is missing? What happens when an external system is unavailable? What happens when the operation is repeated?Boundary Conditions: Empty values, maximum values, minimum values, duplicate requests, concurrent requests, timeout, partial failure, retry, replay, recovery.Non-Functional Requirements: Performance, security, availability, observability, scalability, data retention, compliance.7. Discovery Agent BehaviorThe Discovery Agent should:Ask questionsInspect the repositoryIdentify ambiguityIdentify missing informationIdentify assumptionsDocument decisionsProduce requirementsIt should not invent business decisions.For example:"Should a failed downstream call be retried three times?"is appropriate.But:"The system will retry three times."is not appropriate unless that decision is supported by an existing requirement, ADR, documented business rule, or explicit user decision.8. Requirements ArtifactExample: /docs/features/FEATURE-123/requirements.mdThe artifact should contain:Markdown# FEATURE-123 — Requirements

## Business Problem
...

## Goal
...

## Actors
...

## Happy Path
1.
2.
3.

## Alternate Scenarios
### Scenario A
...

### Scenario B
...

## Boundary Conditions
...

## Failure Conditions
...

## Acceptance Criteria
- [ ] AC-001
- [ ] AC-002
- [ ] AC-003

## Open Questions
- [ ] Q-001

## Decisions
- D-001

## Out of Scope
...

## Status
APPROVAL_REQUIRED
9. Human Gate — RequirementsThe AI should not silently convert ambiguity into requirements.Before architecture begins for significant changes:PlaintextRequirements
      ↓
Human Review
      ↓
APPROVED
Possible states:DRAFTQUESTIONS_OPENREADY_FOR_REVIEWAPPROVEDCHANGES_REQUESTEDSUPERSEDED10. Architecture AnalysisThe Architecture Agent examines:Existing architectureExisting patternsExisting servicesExisting ADRsExisting integrationsExisting infrastructureExisting conventionsExisting failure/retry strategiesExisting observability patternsThe goal is not to invent a new solution.The goal is:Prefer existing architectural patterns unless there is a documented reason not to.11. ADR ManagementBefore implementation, the Architecture Agent determines:PlaintextDoes an existing ADR already cover this?
        │
        ├── YES → Validate compatibility
        │
        └── NO
              │
              ▼
       Is architectural
       decision required?
              │
         ┌────┴────┐
         │         │
        NO        YES
         │         │
         │         ▼
         │       New ADR
         │         │
         │         ▼
         │    Human Approval
         │
         ▼
       Continue
The system should not create an ADR simply because a feature exists.An ADR is required when an architectural decision is sufficiently important, reusable, or consequential to warrant one.12. Architecture ArtifactExample: /docs/features/FEATURE-123/design.mdShould include:Markdown# FEATURE-123 — Design

## Current Architecture
...

## Proposed Architecture
...

## Components Affected
...

## Existing Patterns Reused
...

## New Patterns
...

## Data Flow
...

## Failure Handling
...

## Retry / Idempotency
...

## Security
...

## Observability
...

## Architectural Decisions
- ADR-0012

## Alternatives Considered
...

## Risks
...

## Status
APPROVAL_REQUIRED
13. Implementation PlanningOnce requirements and architecture are approved, the Planning Agent determines how the feature will actually be built.The implementation plan should answer:"What needs to change?"Not:"What code should the developer type?"The plan should identify:ComponentsFilesServicesAPIsDatabase changesConfigurationInfrastructureDependenciesMigration requirementsTest requirementsDeployment considerations14. Task DecompositionThe implementation plan is then broken into atomic tasks.Example:PlaintextFEATURE-123
├── Task 001: Domain model changes
├── Task 002: Repository changes
├── Task 003: Service implementation
├── Task 004: API endpoint
├── Task 005: Integration handling
├── Task 006: Observability
└── Task 007: Integration tests
A task should be small enough that an agent can complete it with a well-defined context.15. Task ContractEach task should have a structured format:Markdown# TASK-004

## Objective
...

## Dependencies
- TASK-001
- TASK-002

## Requirements
- REQ-003
- REQ-004

## Design References
- DESIGN-005
- ADR-0012

## Acceptance Criteria
- AC-003
- AC-004

## Files Expected to Change
...

## Tests Required
...

## Constraints
...

## Definition of Done
...

## Status
READY
16. Test Design Must Be IndependentOne of the strongest parts of the design should remain:Test design should not simply be generated from the implementation.The Test Design Agent should derive tests from:RequirementsAcceptance criteriaBusiness rulesFailure semanticsInterfacesData contractsArchitectureThis reduces the risk of:"The code passes the tests because the tests were designed around the code."17. Test ScenariosTests should cover:Happy Path: Expected normal behavior.Alternate Path: Valid but non-primary behavior.Negative Path: Invalid input or rejected operation.Boundary: Minimum, maximum, empty, null, duplicate, etc.Failure: Dependency failure, timeout, unavailable service.Recovery: Retry, replay, restart, compensation.Idempotency: Repeated request behavior.Concurrency: If applicable.Security: Authorization, authentication, data exposure.Observability: Logging, metrics, tracing where required.18. Context ContractsOne major risk with agentic development is blindly passing the entire repository or every artifact to every agent.Instead, each agent should have a context contract.Discovery AgentReceives: User request, relevant repository documentation, relevant code, existing requirements, relevant business documentation.Architecture AgentReceives: Approved requirements, relevant ADRs, architecture documentation, repository structure, relevant implementation patterns, neighboring components.Planning AgentReceives: Approved requirements, approved design, ADRs, relevant repository structure.Test AgentReceives: Requirements, acceptance criteria, design, interfaces, failure semantics.Developer AgentReceives: Current task, acceptance criteria, relevant requirements, relevant design, relevant ADR sections, existing implementation patterns, relevant files, relevant tests. (The Developer Agent does not need every artifact in the repository.)Review AgentReceives: Requirements, acceptance criteria, ADR, task, diff, test results, evidence, deviation report.19. Agent Capability BoundariesAgents should not have unlimited authority.AgentRead DocsWrite DocsCodeRun TestsArchitectural DecisionsDiscoveryYesYesNoOptionalNoArchitectureYesYesNoOptionalProposePlannerYesYesNoOptionalNoTest DesignerYesYesNoYesNoDeveloperLimitedYesYesYesNoReviewerYesReviewNoYesNoThe important principle is:An agent should only have the authority required to perform its role.20. Evidence-Based CompletionAgents should never be trusted merely because they say:"Task completed."Every meaningful claim should have evidence.Example:YAMLAC-003:
  status: VERIFIED
  evidence:
    implementation:
      - src/Orders/OrderService.cs:142
    tests:
      - tests/Orders/OrderServiceTests.cs:88
    test:
      - Should_Retry_When_Downstream_Service_Times_Out
Possible evidence states:VERIFIED: Automated test passed.SUPPORTED: Code inspection supports the claim but automated verification is unavailable.UNVERIFIED: No sufficient evidence exists.CONFLICT: Artifacts disagree.BLOCKED: Human decision or external dependency is required.21. Source-of-Truth HierarchyA useful logical hierarchy is:PlaintextBusiness Requirements
        ↓
Architecture / ADR
        ↓
Feature Design
        ↓
Implementation Plan
        ↓
Task
        ↓
Code
        ↓
Tests / Evidence
However, this is not a license to blindly treat documentation as reality.The code represents operational reality.Therefore, if artifacts disagree with code, the system should surface the contradiction.It should not silently rewrite the requirements to match the implementation.22. Contradiction DetectionExample:Requirement: Retry 3 times.ADR: Use exponential backoff.Task: Retry twice.Code: Retry four times.The system should report:PlaintextCONTRADICTION DETECTED

Requirement: 3 retries
ADR: exponential backoff
Task: 2 retries
Implementation: 4 retries

Human reconciliation required.
This is substantially safer than allowing the latest agent to decide which statement is correct.23. Deviation ManagementImplementation will sometimes discover facts that were not known during planning.That is normal.The workflow therefore needs explicit deviation classification.Class A — Implementation DetailExample: The plan said to modify OrderService.cs, but the implementation naturally requires a helper class.Action: AUTO-RECONCILE (No architectural decision required).Class B — Design DeviationExample: The planned service boundary is slightly different because the existing codebase already has a reusable abstraction.Action: UPDATE IMPLEMENTATION RECORD / FLAG FOR REVIEW.Class C — Requirement DeviationExample: The implementation reveals that the requested behavior cannot be achieved without changing the business requirement.Action: BLOCK / HUMAN DECISION REQUIRED.Class D — Architectural DeviationExample: The implementation requires a new messaging pattern that is not covered by the approved architecture.Action: BLOCK / ARCHITECTURE REVIEW / ADR UPDATE/CREATION.24. ReconciliationThe workflow should therefore include:PlaintextImplementation
      ↓
Compare:
- Requirements
- Design
- ADR
- Plan
- Task
- Code
- Tests
      ↓
Deviation Detection
      ↓
No conflict ──────→ Review
      │
      ▼
Conflict
      │
      ▼
Reconciliation
      ├── Update artifact
      ├── Update implementation
      ├── Create/update ADR
      └── Request human decision
This is essential because software development is not perfectly linear.25. Git IsolationAutonomous coding should not operate directly against the primary working tree.Preferred model:Plaintextmain
  │
  └── feature/FEATURE-123
          │
          └── agent worktree
                  │
                  ├── Task 001
                  ├── Task 002
                  ├── Task 003
                  │
                  ├── Tests
                  ├── Evidence
                  └── Review
The agent should operate in an isolated worktree or equivalent environment.Benefits:Prevent accidental main-branch modificationsEnable rollbackEnable parallel agentsSimplify reviewPreserve task boundariesReduce risk from autonomous tooling26. Programmatic Workflow EnforcementA workflow should not depend solely on an agent remembering instructions.Create validation tools such as:Plaintext/scripts/
    validate-feature-state
    validate-traceability
    validate-task
    validate-adr
    validate-evidence
Example: Before Developer Agent starts:[ ] Requirements approved[ ] Architecture approved[ ] ADR requirement satisfied[ ] Task exists[ ] Task dependencies complete[ ] Test plan exists[ ] Correct worktree[ ] Required context availableIf any required condition fails: TASK BLOCKED.27. Feature State MachineA feature can have states such as:PlaintextINTAKE
  ↓
DISCOVERY
  ↓
REQUIREMENTS_REVIEW
  ↓
ARCHITECTURE_REVIEW
  ↓
PLANNING
  ↓
TEST_DESIGN
  ↓
READY_FOR_IMPLEMENTATION
  ↓
IMPLEMENTATION
  ↓
VERIFICATION
  ↓
RECONCILIATION
  ↓
REVIEW
  ↓
APPROVED
  ↓
MERGED
Transitions should have explicit preconditions.For example, READY_FOR_IMPLEMENTATION requires:Requirements approvedArchitecture approvedADR resolvedImplementation plan existsTest plan existsTasks created28. TraceabilityEvery important requirement should have a traceability chain.Example:PlaintextREQ-004
   ↓
ADR-0012
   ↓
DESIGN-003
   ↓
TASK-007
   ↓
src/OrderService.cs
   ↓
TEST-019
   ↓
Evidence
The system should be able to answer:"Where is this requirement implemented and tested?""Why does this piece of code exist?"29. Repository StructureA possible repository structure:Plaintext/
├── src/
├── tests/
│
├── docs/
│   ├── adr/
│   │   ├── ADR-0001.md
│   │   └── ADR-0012.md
│   │
│   └── features/
│       └── FEATURE-123/
│           ├── requirements.md
│           ├── design.md
│           ├── implementation-plan.md
│           ├── test-plan.md
│           ├── status.md
│           ├── evidence.md
│           │
│           └── tasks/
│               ├── 001-domain.md
│               ├── 002-service.md
│               └── 003-api.md
│
├── .agents/
│   └── skills/
│       ├── feature-discovery/
│       ├── architecture-analysis/
│       ├── adr-management/
│       ├── implementation-planning/
│       ├── task-decomposition/
│       ├── test-design/
│       ├── coding/
│       └── code-review/
│
└── scripts/
    ├── validate-feature-state
    ├── validate-traceability
    ├── validate-task
    └── validate-evidence
30. Skills vs Agents vs WorkflowThese should remain separate concepts.SkillsReusable capabilities.Examples: feature-discovery, architecture-analysis, adr-management, task-decomposition, test-design, coding, code-review.A skill defines: How to perform a particular type of engineering work.AgentsAgents combine model, skills, tools, permissions, context, and role.Example: Developer Agent = coding skill + repository tools + test tools + task context + limited write permissions.WorkflowThe workflow determines when an agent is allowed to act.Example: Architecture Agent should not execute before Requirements = APPROVED.31. Model IndependenceThe workflow should not be tightly coupled to a particular model.Today it may be:Gemini → DiscoveryClaude → ImplementationTomorrow it could be:Model A → DiscoveryModel B → ArchitectureModel C → ImplementationModel D → ReviewThe orchestration layer should define capabilities rather than brands.For example:YAMLagent:
  role: developer
  required_capabilities:
    - code_generation
    - repository_reasoning
    - test_execution
    - tool_use
Model selection can then be configured independently.This protects the architecture from changes in model capability, pricing, context limits, tool support, availability, and vendor ecosystem.32. Definition of ReadyRequirementsBusiness problem understoodHappy path definedAlternate paths definedFailure scenarios identifiedBoundary conditions identifiedAcceptance criteria definedOpen questions resolvedRequirements approvedArchitectureExisting patterns evaluatedExisting ADRs evaluatedArchitectural impact determinedNew ADR created if requiredArchitecture approvedPlanningImplementation plan completeDependencies identifiedTasks createdTasks independently executableTestingHappy path scenariosNegative scenariosBoundary scenariosFailure scenariosRecovery scenarios where applicableExecutionCorrect worktreePreconditions satisfiedTask status READY33. Definition of DoneA feature is not complete merely because code compiles.RequirementsRequirements satisfiedAcceptance criteria verifiedArchitectureADR compliantArchitectural deviations reconciledImplementationCode completeExisting patterns followedNo unnecessary complexityTestingUnit testsIntegration tests where appropriateNegative scenariosBoundary scenariosFailure/recovery scenarios where appropriateQualityBuild passesTests passStatic analysis passesSecurity checks pass where applicableDocumentationRequirements reflect final behaviorDesign reflects final architectureADR updated if requiredTask status updatedEvidenceRequirements trace to implementationRequirements trace to testsVerification evidence recordedGovernanceReview completedHuman approval completedChanges merged through normal Git workflow34. Human vs AI ResponsibilitiesThe objective is not to eliminate humans. It is to eliminate unnecessary human administrative work.Human owns:Business IntentBusiness DecisionsRisk AcceptanceArchitectural ApprovalRequirement ChangesFinal AcceptanceAI owns:InterviewingRepository AnalysisDocumentationArchitecture AnalysisPlanningTask DecompositionTest DesignCode GenerationTest ExecutionTraceabilityEvidence CollectionDeviation DetectionReconciliation PreparationReview Preparation35. The Human Should Not Become the Workflow SecretaryA critical design principle:The human should not have to manually maintain requirements.md, design.md, plan.md, tasks.md, test-plan.md, status.md, and evidence.md.The AI should maintain these artifacts.The human should interact primarily with:QuestionsDecisionsApprovalsExceptionsFor example, instead of:"Please update requirements.md, then design.md, then task 004."The system should say:"Implementation discovered that the existing retry abstraction cannot support the approved architecture. This requires an architectural decision."Then present:Option A ...Option B ...Impact ...Recommendation basis ...Decision requiredThe human makes the decision. The AI updates the artifacts.36. Managing Documentation BloatThe concern about documentation becoming excessive is legitimate.The answer is not necessarily to eliminate logical artifacts.The answer is to make artifacts adaptive and machine-managed.For low-risk changes:/docs/features/BUG-123/spec.md may be sufficient.For significant changes:requirements.md, design.md, implementation-plan.md, test-plan.md, tasks/, and evidence.md may be appropriate.The complexity classifier determines the level of ceremony.37. Artifact LifecycleArtifacts should not accumulate indefinitely without management.Possible states:DRAFTACTIVEAPPROVEDSUPERSEDEDARCHIVEDA completed feature can eventually have status.md, requirements.md, design.md, implementation-plan.md, test-plan.md, and evidence.md with links to the final commit/PR.The repository becomes an engineering record rather than a dumping ground.38. Review AgentThe Review Agent should not simply perform a generic code review.It should answer:Requirements: Did the implementation satisfy the approved requirements?Architecture: Does the implementation comply with the approved architecture and ADRs?Tests: Do tests prove the important acceptance criteria?Deviations: What changed from the original plan?Evidence: Which claims are verified?Contradictions: Do any artifacts disagree?Quality: Are there unnecessary changes, risks, regressions, or security concerns?The output should be structured.39. Example Review ResultPlaintextFEATURE-123 REVIEW

Requirements
-------------
REQ-001  VERIFIED
REQ-002  VERIFIED
REQ-003  CONFLICT

Architecture
------------
ADR-0012 COMPLIANT

Testing
-------
AC-001 VERIFIED
AC-002 VERIFIED
AC-003 UNVERIFIED

Deviations
----------
DEV-001 Class A
DEV-002 Class B

Contradictions
--------------
REQ-003 conflicts with implementation.

Recommendation
--------------
BLOCKED — human reconciliation required.
This is much more useful than: "Looks good."40. Failure PhilosophyThe system should fail safely:If the AI is uncertain: ASKIf requirements conflict: BLOCKIf architecture conflicts: BLOCKIf evidence is missing: UNVERIFIEDIf implementation differs from plan: RECONCILEThe system should never hide uncertainty merely to keep the workflow moving.41. Phase-Based Implementation RoadmapThe workflow itself should be built incrementally.Phase 1 — FoundationImplement Discovery Skill, Requirements template, Architecture Analysis Skill, ADR Management Skill, and basic feature artifact structure.Initially: NO AUTONOMOUS CODING. Use real features to validate the discovery and architecture process.Phase 2 — Planning and VerificationAdd Implementation Planning, Task Decomposition, Test Design, Traceability IDs, Definition of Ready, Definition of Done, and preflight validation.Phase 3 — Controlled ImplementationAdd Developer Agent, atomic tasks, Git worktrees, automated tests, and evidence collection. The Developer Agent should initially operate with conservative permissions.Phase 4 — Reconciliation and EvidenceAdd deviation detection, contradiction detection, evidence classification, artifact synchronization, and Review Agent. At this point the system becomes significantly more autonomous.Phase 5 — Multi-Model OrchestrationOnly after the workflow works reliably with one model should multiple models be introduced.Potential architecture:Plaintext                 Orchestrator
                      │
       ┌──────────────┼──────────────┐
       │              │              │
   Discovery      Developer       Reviewer
       │              │              │
    Model A         Model B        Model C
Model selection should remain configurable.42. Initial PilotDo not begin with a mission-critical feature.Choose a feature that is real, useful, representative, low-to-medium risk, and easy to rollback.The pilot should measure:Engineering Quality: Defects, test coverage, architectural compliance, review findings.AI Effectiveness: Number of human interventions, number of clarification questions, number of implementation corrections, number of false assumptions.Process Efficiency: Time from request → requirements, time from requirements → implementation, human active time, total elapsed time.Documentation Quality: Traceability completeness, artifact accuracy, drift frequency.43. Success MetricsThe goal is not: "AI wrote 80% of the code." That is an incomplete metric.More useful metrics include:Human active engineering time ↓Defect rate ↓Rework ↓Architecture violations ↓Unverified requirements ↓Manual documentation effort ↓while:Traceability ↑Test coverage ↑Evidence quality ↑Development throughput ↑The ultimate goal is more engineering output with less human administrative overhead and no loss of engineering control.44. What Should Remain Human-ControlledThe following should normally require human authority:Business requirement approvalBusiness rule changesSecurity-sensitive architectural decisionsMajor architecture changesADR approvalRisk acceptanceRequirement deviationsArchitectural deviationsFinal merge/acceptanceThe AI can prepare decisions. It should not silently make consequential decisions on behalf of the organization.45. The V3 Operating ModelThe complete model can be summarized as:Plaintext                    HUMAN
                      │
                      ▼
                Business Intent
                      │
                      ▼
             AI Discovery Agent
                      │
                      ▼
                 Requirements
                      │
                      ▼
                 HUMAN GATE
                      │
                      ▼
            Architecture Agent
                      │
                      ▼
                  ADR Check
                      │
                      ▼
                 HUMAN GATE
                      │
                      ▼
             Planning Agent
                      │
                      ▼
              Test Design Agent
                      │
                      ▼
            Task Decomposition
                      │
                      ▼
             Developer Agent
                      │
                      ▼
                Code + Tests
                      │
                      ▼
                  Evidence
                      │
                      ▼
            Deviation Detection
                      │
            ┌─────────┴─────────┐
            │                   │
         Clean               Conflict
            │                   │
            ▼                   ▼
         Review           Reconciliation
            │                   │
            └─────────┬─────────┘
                      │
                      ▼
               Review Agent
                      │
                      ▼
                 HUMAN REVIEW
                      │
                      ▼
                    MERGED
46. Final Design PhilosophyThe most important conceptual shift is this:Traditional AI-assisted development:PlaintextHuman → Chat → AI → Code
(The conversation is the primary context.)V3 AI-native development:PlaintextHuman Intent → Engineering Artifacts → Controlled Agents → Repository → Code + Tests → Evidence → Review
(The repository and workflow are the primary context. The AI becomes an engineering workforce operating within explicit boundaries.)47. The Core Principles to PreserveThe strongest ideas from the original proposal should remain:Discovery Before Implementation: Do not allow the AI to jump from a vague feature request directly to code.Artifact-First: Important decisions must survive beyond the chat session.Architecture Before Implementation: Existing ADRs and architectural patterns must be evaluated before coding.Independent Test Design: Tests should originate from requirements, not merely from implementation.Task-Level Execution: Large features should be decomposed into atomic, verifiable tasks.Traceability: Requirements must be traceable through design, implementation, and tests.Human Approval Gates: Humans remain the decision authority for consequential decisions.48. The New Principles Added by V3V3 adds several controls that make the original concept substantially more robust:Complexity-Based Routing: Not every change gets the same ceremony.Context Contracts: Agents receive only the information they need.Capability Boundaries: Agents only receive the authority required for their role.Evidence-Based Verification: Completion claims require evidence.Deviation Management: Implementation differences are classified rather than ignored.Contradiction Detection: Conflicting artifacts are surfaced rather than silently reconciled.Git Isolation: Autonomous coding occurs in controlled environments.Programmatic Workflow Enforcement: The process is enforced by tooling, not just prompts.Model Independence: The workflow is independent of Gemini, Claude, or any specific model.49. Final RecommendationThe architecture should be implemented as an AI-assisted software engineering operating model, not as a collection of independent prompts.The fundamental separation should be:SKILLS: How an engineering activity is performed.AGENTS: Who performs it, with what tools and permissions.ARTIFACTS: What knowledge is persisted.WORKFLOW: When an agent is allowed to act.EVIDENCE: How we know the work is actually complete.And the fundamental responsibility split should be:HUMAN: Intent, Decisions, Approval, Risk acceptance.AI: Analysis, Documentation, Planning, Coding, Testing, Verification, Traceability, Reconciliation preparation.REPOSITORY: Persistent context, Decisions, Contracts, Code, Tests, Evidence, History.The ultimate objective is therefore not:"How do we get AI to write more code?"It is:"How do we build a software engineering system where AI can safely carry a feature from human intent to tested, reviewed, traceable implementation while humans remain in control of the decisions that matter?"