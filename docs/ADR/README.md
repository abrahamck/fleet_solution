# Architecture Decision Records (ADRs)

This directory contains the formal record of key architectural decisions made for FleetNexus.

## ADR Index

| ADR # | Title | Category | Status | Date |
| :--- | :--- | :--- | :--- | :--- |
| [ADR-001](ADR-001-authentication-provider-supabase.md) | Authentication Provider (Supabase Auth) | Security & Identity | Accepted | 2026-06-21 |
| [ADR-002](ADR-002-api-centric-auth-orchestration.md) | Auth Flow Orchestration (API-Centric) | Security & Identity | Accepted | 2026-06-21 |
| [ADR-003](ADR-003-multi-tenancy-shared-schema.md) | Multi-Tenancy Strategy (Shared Schema, Discriminator Column) | Multi-Tenancy | Accepted | 2026-06-21 |
| [ADR-004](ADR-004-tenant-isolation-defense-in-depth.md) | Multi-Tenant Data Isolation (EF Core Filters + PostgreSQL RLS) | Security & Multi-Tenancy | Accepted | 2026-06-21 |
| [ADR-005](ADR-005-stateless-jwt-validation.md) | Stateless JWT Validation (HS256) | Security & Auth | Accepted | 2026-06-21 |
| [ADR-006](ADR-006-relational-domain-identity-storage.md) | Relational Domain Identity & Role Storage | Data & Security | Accepted | 2026-06-21 |
| [ADR-007](ADR-007-single-tenant-membership-mvp.md) | Single Tenant Membership Model for MVP | Multi-Tenancy | Accepted | 2026-06-21 |
| [ADR-008](ADR-008-user-provisioning-db-trigger.md) | User & Organization Auto-Provisioning via DB Trigger | Data & Security | Accepted | 2026-06-21 |
| [ADR-009](ADR-009-user-invitation-system-deferred.md) | User Invitation System Architecture | Security & Auth | Deferred | 2026-06-21 |
| [ADR-010](ADR-010-role-based-access-control.md) | Role-Based Access Control (Admin, Member, SuperAdmin) | Authorization | Accepted | 2026-06-21 |
| [ADR-011](ADR-011-security-library-decoupling.md) | Security Library Decoupling (`appfleet-nexus-security`) | Architecture & Code Organization | Accepted | 2026-06-21 |
| [ADR-012](ADR-012-fmcsa-census-dataset-isolation.md) | FMCSA Census Dataset Isolation | Data Architecture | Accepted | 2026-06-21 |
| [ADR-013](ADR-013-user-profile-and-contact-separation.md) | User Profile vs. Business Contact Separation | Data Architecture | Accepted | 2026-06-21 |
| [ADR-014](ADR-014-soft-delete-and-audit-strategy.md) | Soft-Delete and Audit Column Strategy | Data Architecture | Accepted | 2026-06-21 |
| [ADR-015](ADR-015-dashboard-kpi-inmemory-caching.md) | Dashboard KPI In-Memory Caching & Event-Driven Eviction | Performance & Caching | Accepted | 2026-06-22 |
| [ADR-016](ADR-016-demo-tenant-on-demand-seeding.md) | On-Demand Demo Tenant & Vehicle Seeding | Testing & Demonstration | Accepted | 2026-06-22 |
| [ADR-017](ADR-017-tenant-scoped-soft-delete-unique-constraints.md) | Tenant-Scoped Soft-Delete Unique Constraints | Data Architecture | Accepted | 2026-06-22 |
| [ADR-018](ADR-018-multi-step-input-wizard-ux-pattern.md) | Multi-Step Input Wizard UX Pattern | Frontend & UX | Accepted | 2026-06-22 |
| [ADR-019](ADR-019-state-aware-root-routing.md) | State-Aware Dual-Mode Root Routing (`/`) | Frontend Architecture | Accepted | 2026-06-22 |
| [ADR-020](ADR-020-unified-inventory-workspace.md) | Unified Inventory Workspace with Tabbed Sub-Modules | Frontend Architecture | Accepted | 2026-06-22 |
| [ADR-021](ADR-021-cloud-infrastructure-topology.md) | Cloud Infrastructure & Deployment Topology | Infrastructure & DevOps | Accepted | 2026-06-20 |
| [ADR-022](ADR-022-vehicle-contact-many-to-many-join-entity.md) | Many-to-Many Vehicle-Contact Association via Join Entity | Data Architecture | Accepted | 2026-09-19 |
| [ADR-023](ADR-023-polymorphic-contact-point-entity.md) | Separate Typed Contact Point Tables (Phones, Emails, Addresses) | Data Architecture | Accepted | 2026-09-19 |
