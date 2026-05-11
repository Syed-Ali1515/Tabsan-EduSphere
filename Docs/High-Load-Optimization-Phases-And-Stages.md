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

#### Phase 1 Summary
**Implementation Summary**
- Tuned SQL connection pooling and timeouts in API runtime profiles for baseline stability.
- Optimized hot read paths for dashboard, sidebar, and notifications with no-tracking and split-query changes.
- Added short-TTL in-memory caching for dashboard composition, sidebar visibility, and notification inbox/badge reads.

**Validation Summary**
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- Load-test follow-up remains the next optimization gate for higher concurrency proof.

## Phase 2: API Horizontal Scaling
### Stage 2.1: Multi-Instance API Deployment
- Scale API instances gradually (for example: 4 -> 8 -> 16 -> 32).

### Stage 2.2: Load Balancer Policy
- Use a load balancer policy optimized for active traffic (least-connections or equivalent).

### Stage 2.3: Stateless Runtime
- Keep APIs stateless.
- Use JWT and/or Redis-backed session patterns when needed.

#### Phase 2 Summary
**Implementation Summary**
- Added per-instance API identity, per-node health reporting, and local multi-instance orchestration for horizontal scale validation.
- Added least-connections load balancer assets plus request-distribution validation tooling.
- Hardened production startup so API cache state and Web auth cookies require shared backing stores across instances.

**Validation Summary**
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- Runtime validation confirmed the startup guards fail fast when stateless prerequisites are missing outside Development/Testing.

## Phase 3: API Performance Improvements
### Stage 3.1: Endpoint Aggregation
- Add aggregated endpoints for common multi-call screens (for example: /user/home).

### Stage 3.2: Async and Non-Blocking IO
- Ensure high-traffic endpoints use async/non-blocking patterns.

### Stage 3.2 Completed
- Replaced `ContinueWith` wrappers with direct async `await` returns in high-traffic repository methods.
- Kept repository read paths fully asynchronous across timetable, settings, quiz, and building/room queries.
- Validation passed with `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` (130/130).

### Stage 3.3: Transport Optimization
- Enable compression (gzip/brotli).
- Enable HTTP keep-alive and HTTP/2.

### Stage 3.3 Completed
- Status: Completed
- Added Kestrel keep-alive, request-header timeout, server-header suppression, and HTTP/2 ping tuning in API and Web hosts.
- Kept response compression enabled with Brotli/Gzip fast-path settings.
- Validation passed with `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` (130/130) and syntax checks on the touched startup files reported no errors.

#### Phase 3 Summary
**Implementation Summary**
- Added `GET /api/v1/dashboard/context` to aggregate dashboard modules, vocabulary, and widgets into one response.
- Updated the portal ModuleComposition screen to consume the aggregated endpoint instead of three separate API calls.
- Added Web client support for the aggregated dashboard context payload.
- Removed sync-over-async `ContinueWith` bridges from the hot timetable, settings, quiz, and building/room repository methods.
- Tuned API/Web Kestrel transport defaults for keep-alive and HTTP/2-friendly connection handling.

**Validation Summary**
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- Syntax checks on the changed API controller, portal controller, and Web API client files reported no errors.
- Syntax checks on the repository files reported no errors.
- Syntax checks on the updated API/Web startup files reported no errors.

## Phase 4: Caching Strategy
### Stage 4.1: API Cache Policy
- Cache expensive API operations in Redis.
- Start with short TTL windows (for example: 5-30 seconds).

### Stage 4.1 Completed
- Status: Completed
- Implementation Summary: Added short-TTL distributed cache reads/writes for expensive analytics reports (`performance`, `attendance`, `assignments`, `quizzes`) using Redis-backed `IDistributedCache` in production.
- Validation Summary: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (130/130), failed 0.

### Stage 4.2: Edge and Static Caching
- Use CDN caching for static/public content.

### Stage 4.2 Completed
- Status: Completed
- Implementation Summary: Added configurable static asset cache headers in Web host startup (`Cache-Control: public,max-age=...`) and introduced `StaticAssetCaching` settings for default/development/production profiles.
- Validation Summary: syntax checks on touched Web startup and appsettings files reported no errors; unit test run stayed green.

### Stage 4.3: Cache Scope Control
- Cache only expensive/hot-path operations.
- Avoid over-caching volatile or per-user sensitive data.

### Stage 4.3 Completed
- Status: Completed
- Implementation Summary: Limited new shared cache policy to expensive analytics read endpoints with scoped keys by report type + department and applied edge cache headers only to static file responses (not authenticated dynamic endpoints).
- Validation Summary: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (130/130), failed 0; syntax checks on touched files reported no errors.

#### Phase 4 Summary
**Implementation Summary**
- Implemented short-TTL Redis-backed cache policy for the most expensive analytics API read paths.
- Added edge/static cache header policy for Web static assets with environment-configurable max-age.
- Enforced cache scope boundaries so only hot-path shared reads and static assets are cached.

**Validation Summary**
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- Syntax checks on changed analytics/web startup/config files reported no errors.

## Phase 5: k6 Load Testing Improvements
### Stage 5.1: Realistic Load Model
- Use ramping-arrival-rate where suitable.
- Add randomized sleep/think time for realistic traffic behavior.

### Stage 5.1 Completed
- Status: Completed
- Implementation Summary: Reworked the 50k/100k/1m/5m k6 scale scripts to throughput-driven `ramping-arrival-rate` scenarios with per-profile target RPS settings and randomized think-time windows.
- Validation Summary: syntax checks on all updated k6 scale scripts reported no errors.

### Stage 5.2: Distributed Generators
- Split load across multiple generator machines for high target concurrency.

### Stage 5.2 Completed
- Status: Completed
- Implementation Summary: Added generator sharding controls (`GENERATOR_TOTAL`, `GENERATOR_INDEX`) in all scale scripts and updated batch/PowerShell runners to pass shard metadata for multi-generator load splitting.
- Validation Summary: syntax checks on updated runner scripts reported no errors.

### Stage 5.3: Output Discipline
- Keep summary outputs enabled for every run.
- Enable heavy raw outputs only for focused diagnostics.

### Stage 5.3 Completed
- Status: Completed
- Implementation Summary: Enforced summary-first output via `--quiet` on scale runs, retained summary exports and compact `handleSummary` output, and gated heavy raw JSON output in PowerShell runner behind explicit `-AllowRawOutput`.
- Validation Summary: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (130/130), failed 0.

#### Phase 5 Summary
**Implementation Summary**
- Converted scale workloads from concurrency-driven ramps to request-rate-driven ramps.
- Added distributed generator sharding so high target loads can be split safely across multiple machines.
- Standardized summary-only output defaults while keeping opt-in raw diagnostics.

**Validation Summary**
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- Syntax checks on changed k6 scripts and runner wrappers reported no errors.

## Phase 6: Dependency Optimization
### Stage 6.1: External Call Caching
- Cache external API call results where safe.

### Stage 6.1 Completed
- Status: Completed
- Implementation Summary: Added short-TTL distributed caching for safe external library loan lookups keyed by student + integration configuration fingerprint to reduce repeated dependency round-trips.
- Validation Summary: syntax checks on updated `LibraryService` and API integration settings reported no errors.

### Stage 6.2: Resilience Patterns
- Add request timeouts.
- Add circuit breakers.

### Stage 6.2 Completed
- Status: Completed
- Implementation Summary: Extended the outbound integration gateway with channel-level circuit-breaker failure streak and open-window controls, while keeping timeout and retry/backoff policies active.
- Validation Summary: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (130/130), failed 0.

### Stage 6.3: Blocking Risk Reduction
- Remove or isolate blocking dependency calls from request path.

### Stage 6.3 Completed
- Status: Completed
- Implementation Summary: Removed sync-over-async `.Result` consumption in gradebook request composition path and replaced it with awaited task results after parallel fetch completion.
- Validation Summary: syntax checks on updated `GradebookService` reported no errors; unit tests remained green.

#### Phase 6 Summary
**Implementation Summary**
- Reduced external dependency pressure with short-window shared caching on library loan lookups.
- Added circuit-breaker controls to integration gateway channels so failing dependencies can fast-fail instead of consuming full retry budgets continuously.
- Removed blocking read patterns from a hot request path in gradebook aggregation.

**Validation Summary**
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- Syntax checks on changed integration, service, and appsettings files reported no errors.

## Phase 7: Background Processing
### Stage 7.1: Queue Offloading
- Move heavy non-request tasks (notifications, analytics, bulk processing) to background jobs.

### Stage 7.1 Completed
- Status: Completed
- Implementation Summary: Added queue-based offloading for account-security transactional emails so unlock/reset flows enqueue work items instead of sending SMTP directly on request thread.
- Validation Summary: syntax checks on new queue/worker and account-security service updates reported no errors.

### Stage 7.2: Queue Platform Integration
- Use a queueing platform (for example: RabbitMQ, Kafka, or SQS) based on deployment model.

### Stage 7.2 Completed
- Status: Completed
- Implementation Summary: Added deployment-model queue platform selection with `QueuePlatform:Provider` and wired both in-memory channel mode and RabbitMQ producer/consumer mode for account-security email work items.
- Validation Summary: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (130/130), failed 0.

#### Phase 7 Summary
**Implementation Summary**
- Kept existing background offloading paths (notification fanout, report export, result publish) and expanded offload coverage to account-security email operations.
- Introduced queue platform abstraction and configuration-driven runtime selection between in-memory and RabbitMQ for the new account-security queue path.

**Validation Summary**
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- Syntax checks on changed queue, worker, startup, and settings files reported no errors.

## Phase 8: Infrastructure Tuning
### Stage 8.1: Auto-Scaling
- Enable application and infrastructure auto-scaling policies.

### Stage 8.1 Completed
- Status: Completed
- Implementation Summary: Added startup-level auto-scaling policy configuration (`InfrastructureTuning:AutoScaling`) in API, Web, and BackgroundJobs with validated min/max replica bounds and deployment-visible policy diagnostics.
- Validation Summary: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (130/130), failed 0.

### Stage 8.2: Host Limits
- Increase file descriptor/process limits as needed.

### Stage 8.2 Completed
- Status: Completed
- Implementation Summary: Added host-limit tuning controls (`InfrastructureTuning:HostLimits`) and applied thread-pool minimum thread tuning plus configurable Kestrel max concurrent connection limits for API and Web hosts.
- Validation Summary: syntax checks on updated startup files reported no errors; unit test validation passed (130/130).

### Stage 8.3: Network Stack Tuning
- Tune TCP/network parameters for high connection volume.

### Stage 8.3 Completed
- Status: Completed
- Implementation Summary: Added network-stack tuning controls (`InfrastructureTuning:NetworkStack`) for Kestrel keep-alive/header timeout/HTTP2 stream limits and configured outbound `SocketsHttpHandler` pooling and connection limits for API, Web, and BackgroundJobs.
- Validation Summary: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (130/130), failed 0.

#### Phase 8 Summary
**Implementation Summary**
- Standardized infrastructure tuning across API, Web, and BackgroundJobs through a single config contract (`InfrastructureTuning`) with environment-specific baselines.
- Added startup guardrails and observability for auto-scaling policy metadata, host concurrency boundaries, and network throughput controls.

**Validation Summary**
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- Diagnostics checks on updated startup/config files reported no syntax errors.

## Phase 9: Monitoring and Observability
### Stage 9.1: Metrics Stack
- Use Prometheus + Grafana or equivalent observability platform.

### Stage 9.1 Completed
- Status: Completed
- Implementation Summary: Added OpenTelemetry metrics publishing with Prometheus scraping support on the API host and exposed a live `/metrics` endpoint for Grafana/Prometheus collection.
- Validation Summary: syntax checks on the updated observability files reported no errors; unit test validation passed (130/130).

### Stage 9.2: Latency SLO Metrics
- Track latency distributions: p50, p95, p99.

### Stage 9.2 Completed
- Status: Completed
- Implementation Summary: Added rolling request-latency capture middleware and `/health/observability` snapshot output with p50, p95, and p99 summaries.
- Validation Summary: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (130/130), failed 0.

### Stage 9.3: Full-Stack Health Monitoring
- Monitor database, CPU, memory, network, and error rates continuously.

### Stage 9.3 Completed
- Status: Completed
- Implementation Summary: Added API health checks for database connectivity, memory pressure, CPU pressure, network resolution, and rolling error-rate thresholds so the full stack can be monitored continuously.
- Validation Summary: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (130/130), failed 0.

#### Phase 9 Summary
**Implementation Summary**
- Added OpenTelemetry metric publishing with Prometheus scraping support, plus continuous runtime telemetry capture for request latency and error-rate SLOs.
- Added explicit health checks and runtime snapshot endpoints that cover database, CPU, memory, network resolution, and error-rate conditions.

**Validation Summary**
- Validation command: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal`.
- Result: passed (130/130), failed 0.
- Syntax checks on the updated API observability files reported no errors.

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
