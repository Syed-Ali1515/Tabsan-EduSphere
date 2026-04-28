# Tabsan EduSphere Findings and Phased TODO

**Version:** 1.0  
**Date:** 29 April 2026  
**Prepared For:** Project kickoff review and phase approval

---

## 1. Executive Findings

The provided startup documents define a strong product vision and feature set, but they were initially more product-oriented than implementation-ready. The key gap areas and what was resolved are listed below.

### 1.1 What Was Strong

- Clear business goals and licensing model
- Good role definitions and feature expectations
- Clear module concept with licensing-based enablement
- Strong principle of non-destructive academic history

### 1.2 What Needed Strengthening

- ASP.NET architecture boundaries were not explicit
- API and integration standards were not formalized
- Database lacked operational tables and index strategy details
- Module dependencies and technical activation behavior were underspecified
- Delivery sequencing was not yet sprint-ready

### 1.3 What Has Been Added

- ASP.NET implementation architecture baseline in PRD
- Expanded schema conventions and additional core tables
- Module dependency and activation rules mapped to technical implementation
- 12-sprint phased development plan with exit criteria

---

## 2. Architecture Findings

### 2.1 Recommended Architecture

- Modular monolith for v1 with clean boundaries for future service extraction
- ASP.NET Core 8 Web API + ASP.NET Core MVC/Razor UI
- EF Core with SQL Server as default data store
- Background jobs for license checks, notifications, and cleanup tasks

### 2.2 Domain Boundaries (Bounded Contexts)

- Identity and Access
- Academic Core
- Student Lifecycle
- Learning Delivery
- Assessment and Results
- Notifications
- FYP Management
- Licensing and Entitlements
- Audit and Reporting

### 2.3 Critical Cross-Cutting Concerns

- RBAC and policy-based authorization
- Immutable academic history behavior
- Auditability for all privileged operations
- License-driven entitlement checks at UI, API, and job levels
- Observability: logs, metrics, tracing, health checks

---

## 3. Feature Findings

### 3.1 Mandatory Foundation Features

- Authentication and role system
- Department and SIS baseline
- License validation and degraded mode behavior
- Core audit logging

### 3.2 High-Risk Feature Areas

- Licensing and read-only degradation
- Department-scoped data authorization
- Quiz attempt integrity and anti-duplication rules
- Attendance uniqueness and alert workflows
- Transcript export auditing

### 3.3 Recommended Release Scope

- v1.0: Core operations and licensing controls
- v1.1: Quizzes, attendance, FYP, AI baseline
- v1.2: Analytics, advanced audit, expansion capabilities

---

## 4. Database and Data Findings

### 4.1 Schema Readiness Enhancements Identified

- Need explicit indexing strategy for high-traffic read/write paths
- Need additional tables for sessions, offerings, whitelist, quiz internals, and operational audit
- Need retention and archival rules for logs and operational records
- Need migration and seeding sequencing per release phase

### 4.2 Data Integrity Priorities

- One submission per student per assignment
- One attendance record per student-course-date
- Unique registration numbers and controlled signup flow
- Immutable history for semester records

---

## 5. Phased TODO List (Stages and Checklists)

## Phase 0: Foundation and Governance (Sprint 1)

### Stage 0.1 Project Setup
- [ ] Create .NET 8 solution and project structure
- [ ] Configure environment profiles (dev/staging/prod)
- [ ] Configure centralized configuration and secrets strategy

### Stage 0.2 Engineering Guardrails
- [ ] Add CI pipeline for build, tests, and static checks
- [ ] Add coding standards and pull request template
- [ ] Add baseline logging, tracing, and health checks

### Stage 0.3 Baseline Documentation
- [ ] Finalize architecture decision records (ADRs)
- [ ] Confirm API versioning and error envelope standard

---

## Phase 1: Identity, Licensing, and Entitlements (Sprints 2-3)

### Stage 1.1 Identity and Access
- [ ] Implement ASP.NET Core Identity model
- [ ] Implement JWT and session management
- [ ] Implement role and policy authorization matrix

### Stage 1.2 Licensing
- [ ] Implement license upload endpoint and validation workflow
- [ ] Implement startup, daily, and admin-login validation checks
- [ ] Implement degraded read-only behavior for invalid/expired license

### Stage 1.3 Module Entitlements
- [ ] Implement module activation/deactivation APIs
- [ ] Enforce mandatory module protection rules
- [ ] Add module policy filters across APIs

---

## Phase 2: Academic Core and SIS (Sprints 4-5)

### Stage 2.1 Department and Program Core
- [ ] Implement departments, programs, courses, semesters CRUD
- [ ] Implement faculty-to-department assignment model
- [ ] Implement course offering model

### Stage 2.2 Student Lifecycle
- [ ] Implement registration whitelist workflow
- [ ] Implement student profile creation and enrollment
- [ ] Implement immutable semester history records

### Stage 2.3 Access Boundaries
- [ ] Enforce department-scoped faculty access
- [ ] Validate admin all-department visibility behavior

---

## Phase 3: Assignments and Results (Sprints 6-7)

### Stage 3.1 Assignment Pipeline
- [ ] Implement assignment create/publish lifecycle
- [ ] Implement student submission pipeline
- [ ] Enforce one submission per assignment per student

### Stage 3.2 Grading and Results
- [ ] Implement grading and feedback workflow
- [ ] Implement result publication and transcript generation
- [ ] Implement transcript export logs

### Stage 3.3 Quality and Security
- [ ] Add authorization integration tests for all result endpoints
- [ ] Add audit events for grading and publishing

---

## Phase 4: Notifications and Attendance (Sprints 8-9)

### Stage 4.1 Notifications
- [ ] Implement notifications and recipient tracking
- [ ] Implement read/unread and delivery status updates

### Stage 4.2 Attendance
- [ ] Implement attendance capture and uniqueness constraints
- [ ] Implement low-attendance threshold logic
- [ ] Implement alert jobs for attendance warnings

### Stage 4.3 Reliability
- [ ] Add retry policies for notification dispatch
- [ ] Add dead-letter handling for failed job execution

---

## Phase 5: Quizzes and FYP (Sprints 10-11)

### Stage 5.1 Quizzes
- [ ] Implement quiz authoring, question bank, and options
- [ ] Implement attempts and answer persistence
- [ ] Enforce attempt limits and scoring rules

### Stage 5.2 FYP
- [ ] Implement project allocation and meeting scheduling
- [ ] Implement room and panel member assignment
- [ ] Implement FYP notification triggers

### Stage 5.3 Dashboards
- [ ] Add student dashboard views for quizzes and FYP schedule
- [ ] Add faculty views for pending reviews and meetings

---

## Phase 6: AI, Analytics, and Hardening (Sprint 12)

### Stage 6.1 AI Chatbot
- [ ] Implement role-aware chat context orchestration
- [ ] Add module/license guardrails for AI access
- [ ] Add prompt safety and response audit logging

### Stage 6.2 Reporting
- [ ] Implement baseline analytics endpoints
- [ ] Implement exportable reports (PDF/Excel)

### Stage 6.3 Hardening and Release Readiness
- [ ] Run performance and load tests against p95 targets
- [ ] Complete penetration/security checklist
- [ ] Complete UAT and release candidate sign-off

---

## 6. Immediate Recommendations

### 6.1 Architecture and Delivery

- Freeze v1.0 scope now to reduce delivery risk
- Adopt an ADR process before code scaffolding starts
- Keep modular monolith boundaries strict from day one

### 6.2 Security and Compliance

- Prioritize licensing and authorization tests in earliest sprints
- Include audit logging in every privileged feature from first implementation
- Add dependency and secret scanning in CI immediately

### 6.3 Data Strategy

- Approve index and constraints strategy before first migration
- Define backup and restore runbook before production-like testing
- Establish retention policy defaults now to avoid late redesign

### 6.4 Team Execution

- Use sprint demos with phase exit criteria as acceptance gates
- Track risks weekly with named owners and mitigation deadlines
- Do not start AI features until identity, licensing, and SIS core are stable

---

## 7. Approval Checklist for Next Step

- [ ] Approve this phased TODO as the execution baseline
- [ ] Confirm v1.0 scope lock
- [ ] Confirm stack decisions (.NET 8, SQL Server, EF Core)
- [ ] Approve Phase 0 start
- [ ] Approve scaffolding and first migration implementation

---
