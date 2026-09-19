# ADR-015: Dashboard KPI In-Memory Caching & Event-Driven Eviction

* **Status**: Accepted
* **Date**: 2026-06-22

---

## Context & Problem Statement
The authenticated Home page presents Key Performance Indicators (fleet counts, compliance, fuel, registration). Calculating these aggregations on every page load would impose unnecessary database load and latency.

---

## Decision
Implement **pre-calculated in-memory caching** using ASP.NET Core `IMemoryCache`:
* Cache Key: `dashboard_kpis_{tenantId}`
* Expiration: 30-minute absolute expiration.
* Cache Eviction: Immediate cache eviction triggered upon any write action (create, update, delete) to tenant-scoped entities (e.g. vehicles).

---

## Alternatives Considered
* **Real-Time Aggregations**: Querying and computing metrics on every API request.
* **Database Materialized Views / Background Workers**: Generating aggregate tables via background jobs. Rejected due to complexity with PostgreSQL RLS and additional infrastructure overhead.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Near-zero database overhead for dashboard views.
  * Instantaneous cache invalidation guarantees users always see up-to-date counts after adding or editing vehicles.
* **Risks & Trade-offs**: In multi-instance deployments, `IMemoryCache` is local to each process unless replaced by a distributed cache (e.g. Redis).
* **Migration Paths**: Low complexity. Can swap `IMemoryCache` for `IDistributedCache` (Redis) when scaling across multiple backend instances.
