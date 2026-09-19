# Multi-Model Capabilities & Role Mapping Matrix

This document defines the capability requirements and model assignments for each agent role in the FleetNexus V3 AI Feature Development Operating Model.

---

## 1. Model Independence Principle

The operating model is designed to be **model-agnostic**. Individual agent roles specify required capabilities rather than hardcoded vendor models, enabling flexible routing across Gemini, Claude, OpenAI, or local fine-tuned models.

---

## 2. Agent Role Capability Profiles

| Agent Role | Primary Responsibilities | Required Capabilities | Recommended Model Profiles |
| :--- | :--- | :--- | :--- |
| **Discovery Agent** | User interviewing, business analysis, edge cases, requirement formulation | - Deep reasoning & elicitation<br>- Ambiguity detection<br>- Structured markdown generation | Gemini 1.5 Pro / Claude 3.5 Sonnet / GPT-4o |
| **Architecture Agent** | Repository scanning, ADR compatibility, component boundaries, data flow | - Large context repo reasoning<br>- Architectural pattern recognition<br>- Systems design | Claude 3.5 Sonnet / Gemini 1.5 Pro / GPT-4o |
| **Planning & Test Agent** | Scope inventory, atomic task decomposition, independent test matrices | - Schema-driven planning<br>- Deterministic test case generation<br>- Dependency mapping | Gemini 1.5 Pro / Claude 3.5 Sonnet / Claude 3.5 Haiku |
| **Developer Agent** | Isolated coding in worktrees, targeted unit/integration testing | - High-precision C# / TS generation<br>- Test runner & CLI execution<br>- Bounded file editing | Claude 3.5 Sonnet / GPT-4o / Gemini 1.5 Pro |
| **Review Agent** | Structured review, contradiction detection, deviation classification | - Strict diff inspection<br>- Invariant verification<br>- Anomaly detection | Claude 3.5 Sonnet / Gemini 1.5 Pro / GPT-4o |

---

## 3. Configuration & Overrides

Model assignments can be adjusted based on latency, context window requirements, pricing, or local offline availability without modifying the workflow state machines or skill definitions.
