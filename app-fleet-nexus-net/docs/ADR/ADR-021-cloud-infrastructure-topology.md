# ADR-021: Cloud Infrastructure & Deployment Topology

* **Status**: Accepted
* **Date**: 2026-06-20

---

## Context & Problem Statement
FleetNexus requires a cost-effective, scalable, and fully managed cloud infrastructure topology to host the containerized .NET Web API, Blazor WebAssembly frontend static assets, and managed PostgreSQL database without manual server maintenance.

---

## Decision
Adopt a modern, three-tier cloud deployment topology:
1. **Backend Web API**: Docker container running ASP.NET Core 10 hosted on **Render Web Services**.
2. **Frontend UI**: Blazor WebAssembly static assets hosted on **Render Static Site**.
3. **Database & Identity**: Managed PostgreSQL and Supabase Auth hosted on **Supabase Cloud**.

---

## Alternatives Considered
* **AWS Topology**: AWS ECS (Fargate) + S3/CloudFront + AWS RDS PostgreSQL. Rejected due to higher setup complexity and recurring baseline monthly costs.
* **Azure Topology**: Azure App Service (API & Static Web Apps) + Azure SQL / Postgres Flexible Server. Rejected due to cost considerations for early-stage MVP.
* **Self-Hosted VPS**: Single Linux VM running Docker Compose. Rejected due to operational maintenance, patching, and backup management overhead.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Free/low-cost deployment tier with zero server provisioning or maintenance overhead.
  * Containerized Docker builds ensure environment parity between local development and production.
  * Automatic HTTPS, SSL certificates, and CI/CD git-push deployments managed by Render.
* **Risks & Trade-offs**: Free-tier web services on Render may sleep after periods of inactivity, introducing cold-start latency.
* **Migration Paths**: Standard Docker container and standard PostgreSQL database allow seamless migration to AWS, Azure, or GCP whenever enterprise scale warrants.
