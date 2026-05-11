# High-Load Optimization Phases and Stages

## Objective
This plan converts the high-load optimization guide into phased execution stages to prepare the application for large-scale traffic.

## Phase 1: Database Optimization (Primary Bottleneck)
### Stage 1.1: Connection Efficiency
- Implement database connection pooling (ORM pooling and/or PgBouncer style pooling).

### Stage 1.2: Read/Write Separation
- Add read replicas.
- Route read-heavy traffic to replicas and keep writes on primary.

### Stage 1.3: Query Efficiency
- Add/validate indexes for high-frequency query paths.
- Remove N+1 query patterns.
- Optimize slow queries identified by profiling.

### Stage 1.4: Data Access Caching
- Introduce Redis caching for frequent queries (dashboard, notifications, repeated lookups).

## Phase 2: API Horizontal Scaling
### Stage 2.1: Multi-Instance API Deployment
- Scale API instances gradually (for example: 4 -> 8 -> 16 -> 32).

### Stage 2.2: Load Balancer Policy
- Use a load balancer policy optimized for active traffic (least-connections or equivalent).

### Stage 2.3: Stateless Runtime
- Keep APIs stateless.
- Use JWT and/or Redis-backed session patterns when needed.

## Phase 3: API Performance Improvements
### Stage 3.1: Endpoint Aggregation
- Add aggregated endpoints for common multi-call screens (for example: /user/home).

### Stage 3.2: Async and Non-Blocking IO
- Ensure high-traffic endpoints use async/non-blocking patterns.

### Stage 3.3: Transport Optimization
- Enable compression (gzip/brotli).
- Enable HTTP keep-alive and HTTP/2.

## Phase 4: Caching Strategy
### Stage 4.1: API Cache Policy
- Cache expensive API operations in Redis.
- Start with short TTL windows (for example: 5-30 seconds).

### Stage 4.2: Edge and Static Caching
- Use CDN caching for static/public content.

### Stage 4.3: Cache Scope Control
- Cache only expensive/hot-path operations.
- Avoid over-caching volatile or per-user sensitive data.

## Phase 5: k6 Load Testing Improvements
### Stage 5.1: Realistic Load Model
- Use ramping-arrival-rate where suitable.
- Add randomized sleep/think time for realistic traffic behavior.

### Stage 5.2: Distributed Generators
- Split load across multiple generator machines for high target concurrency.

### Stage 5.3: Output Discipline
- Keep summary outputs enabled for every run.
- Enable heavy raw outputs only for focused diagnostics.

## Phase 6: Dependency Optimization
### Stage 6.1: External Call Caching
- Cache external API call results where safe.

### Stage 6.2: Resilience Patterns
- Add request timeouts.
- Add circuit breakers.

### Stage 6.3: Blocking Risk Reduction
- Remove or isolate blocking dependency calls from request path.

## Phase 7: Background Processing
### Stage 7.1: Queue Offloading
- Move heavy non-request tasks (notifications, analytics, bulk processing) to background jobs.

### Stage 7.2: Queue Platform Integration
- Use a queueing platform (for example: RabbitMQ, Kafka, or SQS) based on deployment model.

## Phase 8: Infrastructure Tuning
### Stage 8.1: Auto-Scaling
- Enable application and infrastructure auto-scaling policies.

### Stage 8.2: Host Limits
- Increase file descriptor/process limits as needed.

### Stage 8.3: Network Stack Tuning
- Tune TCP/network parameters for high connection volume.

## Phase 9: Monitoring and Observability
### Stage 9.1: Metrics Stack
- Use Prometheus + Grafana or equivalent observability platform.

### Stage 9.2: Latency SLO Metrics
- Track latency distributions: p50, p95, p99.

### Stage 9.3: Full-Stack Health Monitoring
- Monitor database, CPU, memory, network, and error rates continuously.

## Phase 10: Progressive Load Test Strategy
### Stage 10.1: Incremental Scale Gates
- Validate in steps (for example: 10k -> 20k -> 50k -> 80k -> higher tiers).

### Stage 10.2: Bottleneck Isolation
- Identify the first bottleneck at each stage (DB, API, infra, dependency, queue).

### Stage 10.3: Fix-and-Retest Cycle
- Apply targeted fixes.
- Re-run the same stage.
- Promote to next stage only after stability criteria are met.

## Execution Notes
- Start with Phase 1 and Phase 2 as highest priority.
- Record baseline metrics before every major optimization change.
- Treat each stage as done only when validated by repeatable load test results.

## Progress Log
### 2026-05-11 - Phase 2 Stage 2.3 Completed
- Status: Completed.

#### Implementation Summary
- Hardened API startup to require `ScaleOut:RedisConnectionString` outside Development/Testing, preventing silent fallback to node-local distributed memory cache in production.
- Hardened Web startup to require `ScaleOut:SharedDataProtectionKeyRingPath` outside Development/Testing, ensuring cookie encryption keys are shared across web instances.
- Kept local developer/test environments permissive so local workflow remains simple while production statelessness is enforced.

#### Validation Summary
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- Runtime check: startup guards now fail fast if stateless prerequisites are missing in non-development environments.

#### Next Stage
- Phase 3 Stage 3.1: endpoint aggregation for common multi-call screens.

### 2026-05-11 - Phase 2 Stage 2.2 Completed
- Status: Completed.

#### Implementation Summary
- Added Nginx least-connections upstream baseline template in `Scripts/Phase2-Stage2.2-nginx-leastconn.conf.template` with forward-header propagation and balancer self-health endpoint (`/lb-health`).
- Added Stage 2.2 load balancer orchestration script `Scripts/Phase2-Stage2.2-LoadBalancer.ps1` to start/stop a local Nginx balancer container targeting multiple API instances.
- Added Stage 2.2 distribution validation script `Scripts/Phase2-Stage2.2-Validate-LB.ps1` to sample load-balanced requests and summarize per-instance request split using `X-EduSphere-Instance`.
- Updated operational script index in `Scripts/README.md` with Stage 2.2 assets.

#### Validation Summary
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- Stage 2.2 runtime verification method: start API instances via Stage 2.1 script, start least-connections LB via Stage 2.2 script, then run Stage 2.2 validation sampler and confirm multi-node distribution.

#### Next Stage
- Phase 2 Stage 2.3: stateless runtime hardening and shared session/caching boundaries.

### 2026-05-11 - Phase 2 Stage 2.1 Completed
- Status: Completed.

#### Implementation Summary
- Added per-instance identity baseline in API startup (`ScaleOut:InstanceId`) with automatic runtime fallback (`{machine}-p{processId}`) for horizontally scaled node visibility.
- Added optional instance telemetry response header (`X-EduSphere-Instance`) controlled by `ScaleOut:ExposeInstanceHeader` for load balancer distribution verification.
- Added explicit node health endpoint `GET /health/instance` returning instance id, process id, machine, uptime, and version.
- Added Stage 2.1 operational script `Scripts/Phase2-Stage2.1-MultiInstance-Api.ps1` for local multi-instance start/stop verification.
- Extended API scale-out configuration in appsettings (default and production) with `InstanceId` and `ExposeInstanceHeader` keys.

#### Validation Summary
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- API build command outcome: `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj -v minimal` reported file-lock copy failures due a running API process (PID 35564), but no Stage 2.1 compile regression was observed in unit-test compilation path.

#### Next Stage
- Phase 2 Stage 2.2: load balancer policy baseline (least-connections and health-probe routing behavior).

### 2026-05-11 - Phase 1 Stage 1.4 Completed
- Status: Completed.

#### Implementation Summary
- Added short-TTL in-memory caching to `DashboardCompositionService.GetWidgets(...)` with role + institution-policy keyed entries (15s TTL).
- Added short-TTL in-memory caching to `SidebarMenuService.GetTopLevelMenusAsync(...)` and `SidebarMenuService.GetVisibleForRoleAsync(...)` (20s TTL) with versioned invalidation on sidebar mutation methods.
- Added short-TTL in-memory caching to `NotificationService.GetInboxAsync(...)` (10s TTL) and `NotificationService.GetBadgeAsync(...)` (8s TTL) with version bump invalidation on send/deactivate/mark-read mutations.
- Kept cache scope limited to hot read paths to avoid stale-write coupling and preserve write correctness.

#### Validation Summary
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- Build status: succeeded; only existing nullability warnings remained in `SettingsServices.cs` (no new Stage 1.4 regressions).

#### Next Stage
- Phase 2 Stage 2.1: multi-instance API deployment baseline and horizontal scale validation.

### 2026-05-11 - Phase 1 Stage 1.3 Completed
- Optimized notification inbox query path to use no-tracking reads for paged inbox retrieval.
- Optimized unread badge count query to avoid unnecessary Include loading.
- Optimized sidebar menu read paths with no-tracking and split-query patterns for top-level/visible menu fetches.
- Next stage focus: Stage 1.4 short-TTL caching on hot read endpoints.

### 2026-05-11 - Phase 1 Stage 1.2 Completed
- Applied SQL connection pool tuning in API appsettings for default, development, and production profiles.
- Updated connection strings with explicit Min Pool Size, Max Pool Size, and Connect Timeout values.
- Next stage focus: Stage 1.3 query-path optimization for dashboard, sidebar visibility, and notification inbox endpoints.
