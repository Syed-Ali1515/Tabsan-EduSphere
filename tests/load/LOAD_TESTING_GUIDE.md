# K6 Load Testing Guide - Tabsan-EduSphere Login Endpoint

**Purpose**: Simulate high-traffic login scenarios to identify performance bottlenecks and validate system scalability up to 1 million concurrent users.

**Target Endpoint**: `/api/auth/login`

---

## Table of Contents

1. [Installation & Setup](#installation--setup)
2. [Running Tests Locally](#running-tests-locally)
3. [Distributed Testing (Cloud)](#distributed-testing-cloud)
4. [Interpreting Results](#interpreting-results)
5. [ASP.NET Scaling Improvements](#aspnet-scaling-improvements)
6. [Performance Tuning Checklist](#performance-tuning-checklist)

---

## Installation & Setup

### Prerequisites

- **K6**: https://k6.io/docs/getting-started/installation/
- **Node.js**: For advanced scripting (optional)
- **Your API**: Running and accessible (local or remote)

### Install K6

#### Windows (PowerShell)
```powershell
# Using Chocolatey
choco install k6

# Or download from: https://github.com/grafana/k6/releases
```

#### macOS
```bash
brew install k6
```

#### Linux (Ubuntu/Debian)
```bash
sudo apt-get update
sudo apt-get install -y gnupg2 curl lsb-release
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | \
  sudo tee /etc/apt/sources.list.d/k6-stable.list
sudo apt-get update
sudo apt-get install k6
```

### Verify Installation

```bash
k6 version
# Output: k6 v0.47.0
```

---

## Running Tests Locally

### 1. Basic Local Test (Simple)

```bash
# Navigate to project directory
cd c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\tests\load

# Run the test script (uses default configuration)
k6 run login-load-test.js

# Expected output:
# ✓ running (8m30s), 100000/100000 VUs, 10 complete and 0 interrupted iterations
# ...
```

### 2. Local Test with Custom Target URL

```powershell
# Test against specific environment
$env:TARGET_URL="http://localhost:5000"
k6 run login-load-test.js

# Or as a single command
$env:TARGET_URL="https://staging-api.example.com"; k6 run login-load-test.js
```

### 3. Local Test with Output Reporting

```bash
# Generate HTML report
k6 run --out json=results.json login-load-test.js

# View results
# On Windows: Invoke-Item results.json
# On macOS/Linux: open results.json
```

### 4. Run Test with Custom Thresholds

Create a custom configuration file `load-test-config.js`:

```javascript
import { options as defaultOptions } from './login-load-test.js';

export const options = {
  ...defaultOptions,
  // Override thresholds for this run
  thresholds: {
    'response_time': ['p(95)<1000', 'p(99)<2000'],
    'errors': ['rate<0.05'], // More lenient
  },
};
```

Then run:
```bash
k6 run load-test-config.js
```

### 5. Real-Time Monitoring (Web UI)

```bash
# Install Grafana k6 browser extension (optional)
k6 run --web login-load-test.js

# Access at: http://localhost:6565
```

### 6. Progressive Load Test (Staging Environment)

```bash
# Test against staging
$env:TARGET_URL="https://api-staging.example.com"
k6 run `
  --stage "2m:100" `
  --stage "5m:1000" `
  --stage "10m:10000" `
  login-load-test.js
```

---

## Distributed Testing (Cloud)

### Option 1: K6 Cloud (Recommended)

K6 Cloud allows distributed testing from multiple geographic locations.

#### Setup

1. **Create K6 Cloud Account**
   - Visit: https://cloud.k6.io/
   - Sign up for free trial (4-hour monthly limit)

2. **Authenticate with K6 Cloud**
   ```bash
   k6 login cloud
   # Enter API token when prompted
   ```

3. **Run Distributed Test**
   ```bash
   k6 cloud run login-load-test.js
   ```

#### Benefits
- ✅ Distributed load from multiple regions
- ✅ Real-time monitoring dashboard
- ✅ Automatic result storage
- ✅ Scale to 1M+ concurrent users
- ✅ No local resource constraints

#### Example: 100K Concurrent Users from Cloud

```bash
# This runs on K6 cloud servers
k6 cloud run `
  --vus 100000 `
  --duration 30m `
  --distribution "10%:us-east,30%:us-west,40%:eu-west,20%:ap-southeast" `
  login-load-test.js
```

---

### Option 2: Docker-Based Distributed Testing

For testing from multiple machines without cloud.

#### Setup on Master Machine

1. **Create docker-compose.yml**

```yaml
version: '3'
services:
  k6-master:
    image: grafana/k6:latest
    command: run --execution-segment "0:1" --execution-segment-sequence "0/2" /scripts/login-load-test.js
    volumes:
      - ./tests/load:/scripts
    ports:
      - "6565:6565"

  k6-worker-1:
    image: grafana/k6:latest
    command: run --execution-segment "1/2" --execution-segment-sequence "0/2" /scripts/login-load-test.js
    volumes:
      - ./tests/load:/scripts
    depends_on:
      - k6-master

  k6-worker-2:
    image: grafana/k6:latest
    command: run --execution-segment "2/2" --execution-segment-sequence "0/3" /scripts/login-load-test.js
    volumes:
      - ./tests/load:/scripts
    depends_on:
      - k6-master
```

2. **Run Distributed Test**

```bash
docker-compose up
```

---

### Option 3: Manual Multi-Machine Distribution

#### On Machine 1 (50K users)
```bash
$env:TARGET_URL="http://api.production.com"
k6 run `
  --stages "5m:50000" `
  --summary-trend-stats "avg,min,med,max,p(95),p(99)" `
  login-load-test.js
```

#### On Machine 2 (50K users)
```bash
$env:TARGET_URL="http://api.production.com"
k6 run `
  --stages "5m:50000" `
  --summary-trend-stats "avg,min,med,max,p(95),p(99)" `
  login-load-test.js
```

#### Merge Results
```bash
# Aggregate results from both machines
# (Manual consolidation required)
```

---

## Interpreting Results

### Key Metrics Explained

#### 1. **Response Time**

```
Min: 45ms        - Fastest response time
Avg: 187ms       - Average response time
P95: 412ms       - 95% of requests completed within this time
P99: 956ms       - 99% of requests completed within this time
Max: 2341ms      - Slowest response time
```

**Good targets:**
- P95 < 500ms (95% of users experience quick response)
- P99 < 1000ms (99% of users experience acceptable response)
- Avg < 200ms (good average performance)

**Interpretation:**
```
✅ P95=350ms, Avg=150ms   → Excellent performance
⚠️  P95=800ms, Avg=200ms   → Acceptable but approaching limits
❌ P95=2000ms, Avg=500ms  → Poor performance, needs optimization
```

#### 2. **Error Rate**

```
Errors: 127 out of 50,000 requests = 0.25% error rate
```

**Acceptable thresholds:**
- 0% - 0.1%: Excellent
- 0.1% - 1%: Acceptable
- 1% - 5%: Warning
- > 5%: Critical

**Common error responses:**
```
200 → Success
400 → Bad request (invalid JSON/payload)
401 → Unauthorized (wrong credentials)
429 → Rate limited (too many requests)
500 → Server error (backend issue)
503 → Service unavailable (overloaded)
```

#### 3. **Requests Per Second (RPS)**

```
Total requests: 1,234,567
Test duration: 1 hour (3,600 seconds)
RPS = 1,234,567 / 3,600 ≈ 343 requests/sec
```

**Interpretation:**
- Low RPS (< 100): System not strained
- Medium RPS (100-1000): Normal production load
- High RPS (1000+): High demand, requires optimization

#### 4. **Concurrent Users (VUs) Impact**

```
Stage 1: 100 VUs    → Avg response 50ms
Stage 2: 1,000 VUs  → Avg response 180ms
Stage 3: 10,000 VUs → Avg response 650ms
Stage 4: 100,000 VUs → Avg response 2,500ms
```

**Analysis:** Response time increases with VU count (non-linear)
- Suggests database bottleneck or connection pool exhaustion

---

### Example Test Results Analysis

#### Scenario A: Healthy System

```
Test Results:
├── Requests: 500,000 completed
├── Errors: 342 (0.07% error rate) ✅
├── Response Time:
│   ├── P95: 380ms ✅
│   ├── P99: 720ms ✅
│   └── Avg: 145ms ✅
├── RPS: 1,389 rps ✅
└── Conclusion: System handles 100K concurrent users well
```

**Recommendation:** Ready for production, consider capacity planning for future growth.

#### Scenario B: Stressed System

```
Test Results:
├── Requests: 450,000 completed
├── Errors: 67,200 (12.9% error rate) ❌
├── Response Time:
│   ├── P95: 2,100ms ❌
│   ├── P99: 4,500ms ❌
│   └── Avg: 890ms ⚠️
├── RPS: 1,250 rps ⚠️
└── Conclusion: System under severe stress at 100K VUs
```

**Immediate Actions Required:**
1. Check database connection pool
2. Monitor CPU/Memory on API servers
3. Implement rate limiting
4. Add horizontal scaling (load balancer)

---

## ASP.NET Scaling Improvements

### 1. **Connection Pooling Optimization**

**Problem:** Each request opens new database connection

**Solution in appsettings.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql-server;Database=EduSphere;User Id=sa;Password=***;Max Pool Size=200;Min Pool Size=10;Connect Timeout=30;"
  }
}
```

**Key Parameters:**
- `Max Pool Size`: 200 (increase for high concurrency)
- `Min Pool Size`: 10 (pre-allocate connections)
- `Connect Timeout`: 30 seconds

**Advanced Configuration in Program.cs:**

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            // Connection pooling
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelaySeconds: 5,
                errorNumbersToAdd: null);
            
            // Batch size for operations
            sqlOptions.MaxBatchSize(1000);
            
            // Query split behavior
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }
    )
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking) // For read-only queries
);
```

---

### 2. **Async/Await Pattern**

**Problem:** Synchronous operations block thread pool

**Current Implementation (Slow):**
```csharp
public IActionResult Login(LoginRequest request)
{
    // ❌ Blocks thread - cannot handle concurrent requests
    var user = _userRepository.GetByEmail(request.Email);
    var passwordValid = _passwordHasher.Verify(user.PasswordHash, request.Password);
    
    return Ok(new { accessToken = GenerateToken(user) });
}
```

**Optimized Implementation (Fast):**
```csharp
public async Task<IActionResult> Login(LoginRequest request)
{
    // ✅ Non-blocking - thread returns to pool
    var user = await _userRepository.GetByEmailAsync(request.Email);
    
    if (user == null)
        return Unauthorized("Invalid credentials");
    
    var passwordValid = await _passwordHasher.VerifyAsync(user.PasswordHash, request.Password);
    
    if (!passwordValid)
        return Unauthorized("Invalid credentials");
    
    var token = await _tokenService.GenerateTokenAsync(user);
    
    return Ok(new { accessToken = token });
}
```

**Repository Implementation:**
```csharp
public interface IUserRepository
{
    // ✅ Async methods
    Task<User> GetByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(int userId, string password);
    Task UpdateLastLoginAsync(int userId);
}

public class UserRepository : IUserRepository
{
    public async Task<User> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}
```

---

### 3. **Caching Strategy**

**Problem:** Database queried for same data repeatedly

**Solution: Redis Caching in Program.cs:**

```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
    options.InstanceName = "EduSphere:";
});

services.AddSingleton<ICacheService, RedisCacheService>();
```

**Implement Caching in Login:**

```csharp
private async Task<User> GetUserWithCacheAsync(string email)
{
    const string cacheKey = $"user:{email}";
    
    // Try to get from cache first
    var cachedUser = await _cacheService.GetAsync<User>(cacheKey);
    if (cachedUser != null)
        return cachedUser; // Cache hit - fast!
    
    // Cache miss - fetch from database
    var user = await _userRepository.GetByEmailAsync(email);
    
    if (user != null)
    {
        // Store in cache for 1 hour
        await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromHours(1));
    }
    
    return user;
}
```

**Cache Invalidation:**
```csharp
public async Task<IActionResult> UpdateUserProfile(UserUpdateRequest request)
{
    var user = await _userService.UpdateAsync(request);
    
    // Invalidate cache after update
    await _cacheService.RemoveAsync($"user:{user.Email}");
    
    return Ok(user);
}
```

---

### 4. **Batch Processing & Optimization**

**Problem:** Individual queries for each related entity (N+1 problem)

**Inefficient:**
```csharp
var user = await _context.Users.FindAsync(userId);
var enrollments = await _context.Enrollments.Where(e => e.StudentId == userId).ToListAsync();
var grades = await _context.Grades.Where(g => g.StudentId == userId).ToListAsync();
// 3 separate queries!
```

**Optimized (Single Query):**
```csharp
var user = await _context.Users
    .Include(u => u.Enrollments)
    .Include(u => u.Grades)
    .AsNoTracking() // Read-only query
    .FirstOrDefaultAsync(u => u.Id == userId);
```

**Batch Operations:**
```csharp
public async Task<int> BulkUpdateAttendanceAsync(List<AttendanceRecord> records)
{
    // Add all records at once
    _context.AttendanceRecords.AddRange(records);
    
    // Batch insert (single transaction)
    return await _context.SaveChangesAsync();
}
```

---

### 5. **Response Compression**

**Problem:** Large response payloads consume bandwidth

**Solution in Program.cs:**

```csharp
// Enable gzip compression
services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "text/json" }
    );
});

app.UseResponseCompression();
```

**Expected Impact:**
- JSON responses compressed by 60-80%
- Reduced bandwidth usage
- Faster transmission for high RPS scenarios

---

### 6. **Request Throttling & Rate Limiting**

**Problem:** Uncontrolled traffic can overwhelm server

**Solution using AspNetCoreRateLimit:**

```csharp
services.AddMemoryCache();
services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*/api/auth/login",
            Period = "1m",
            Limit = 100, // Max 100 logins per minute per IP
            MonitorMode = false,
        },
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 1000, // General limit
        },
    };
});

services.AddInMemoryRateLimiting();
services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
```

**Apply to Controllers:**

```csharp
[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // Rate limited: max 100 requests per minute per IP
        // Returns 429 (Too Many Requests) if exceeded
        
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }
}
```

---

### 7. **Horizontal Scaling with Load Balancing**

**Problem:** Single server has hardware limits

**Solution: Docker + Kubernetes**

Create `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "Tabsan.EduSphere.API.dll"]
```

Deploy with Docker Compose:
```yaml
version: '3.8'
services:
  api-1:
    image: edusphere-api:latest
    ports:
      - "5001:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;...
  
  api-2:
    image: edusphere-api:latest
    ports:
      - "5002:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;...
  
  api-3:
    image: edusphere-api:latest
    ports:
      - "5003:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;...
  
  nginx:
    image: nginx:latest
    ports:
      - "80:80"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
    depends_on:
      - api-1
      - api-2
      - api-3
```

NGINX load balancer config (`nginx.conf`):
```nginx
upstream backend {
    least_conn;  # Use least connections algorithm
    server api-1:80 weight=1;
    server api-2:80 weight=1;
    server api-3:80 weight=1;
}

server {
    listen 80;
    location / {
        proxy_pass http://backend;
        proxy_http_version 1.1;
        proxy_set_header Connection "";
    }
}
```

**Scale to 100K Users:**
```bash
# Deploy 10 instances, each handling 10K concurrent users
docker-compose up --scale api=10
```

---

### 8. **Database Optimization**

**Create Essential Indexes:**

```sql
-- User lookup by email (frequently used in login)
CREATE INDEX IX_Users_Email ON Users(Email);

-- Enrollment queries by student
CREATE INDEX IX_Enrollments_StudentId ON Enrollments(StudentId);

-- Attendance queries by offering and date
CREATE NONCLUSTERED INDEX IX_Attendance_OfferingDate 
    ON AttendanceRecords(CourseOfferingId, AttendanceDate)
    INCLUDE (StudentId, Status);

-- Archive old records
CREATE TABLE AttendanceRecords_Archive AS
SELECT * FROM AttendanceRecords WHERE AttendanceDate < DATEADD(YEAR, -1, GETDATE());

DELETE FROM AttendanceRecords WHERE AttendanceDate < DATEADD(YEAR, -1, GETDATE());
```

---

## Performance Tuning Checklist

### Before Load Test

- [ ] **Connection Pool Configuration**
  - [ ] Set `MaxPoolSize` to 200+
  - [ ] Set `MinPoolSize` to 10+
  - [ ] Enable retry on failure

- [ ] **Async Implementation**
  - [ ] All database queries are async
  - [ ] All HTTP calls are async
  - [ ] Controller actions return `Task<IActionResult>`

- [ ] **Caching**
  - [ ] Redis configured
  - [ ] User data cached (1-hour TTL)
  - [ ] Role data cached (daily TTL)
  - [ ] Cache invalidation implemented

- [ ] **Query Optimization**
  - [ ] No N+1 queries
  - [ ] Use `AsNoTracking()` for read-only queries
  - [ ] Create necessary database indexes
  - [ ] Archive old data (>1 year)

- [ ] **Response Compression**
  - [ ] Gzip enabled
  - [ ] Brotli enabled
  - [ ] JSON responses configured

- [ ] **Rate Limiting**
  - [ ] Login endpoint rate limited (100/min)
  - [ ] Global rate limit set (1000/min)
  - [ ] 429 responses handled gracefully

- [ ] **Monitoring & Logging**
  - [ ] Application Insights configured
  - [ ] Slow query logging enabled
  - [ ] Error tracking implemented

### During Load Test

- [ ] Monitor CPU usage (should stay < 70%)
- [ ] Monitor memory usage (should stay < 80%)
- [ ] Monitor database connections (should be < MaxPoolSize)
- [ ] Check for connection pool exhaustion errors
- [ ] Observe error rate growth pattern
- [ ] Track response time percentiles

### After Load Test

- [ ] Analyze results for bottlenecks
- [ ] Review slow query log
- [ ] Check for connection pool issues
- [ ] Verify database index effectiveness
- [ ] Document recommendations
- [ ] Plan scaling strategy

---

## Troubleshooting Guide

### Issue: "Connection pool exhausted" Errors

```
Error: Timeout expired. The timeout period elapsed prior to obtaining a 
connection from the pool. This may have occurred because all pooled 
connections were in use and max pool size was reached.
```

**Solutions:**
1. Increase `MaxPoolSize` in connection string
2. Implement connection pooling in middleware
3. Add `async` to all database operations
4. Implement request queuing with backpressure

```csharp
// Implement backpressure
services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBufferSize = 1024 * 1024; // 1MB
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(10);
});
```

### Issue: "Rate Limited" (429) Responses

```
Error: Rate limit exceeded. Maximum 100 requests per minute reached.
```

**Solutions:**
1. Verify rate limit configuration is appropriate
2. Implement exponential backoff in k6 script
3. Distribute requests across more time
4. Scale horizontally to handle request volume

### Issue: High Memory Usage

```
Process consuming > 5GB RAM with 100K concurrent users
```

**Solutions:**
1. Reduce cache TTL
2. Implement memory-efficient serialization
3. Enable garbage collection tuning
4. Use streaming responses for large payloads

### Issue: Database Connection Timeouts

**Solutions:**
1. Verify SQL Server is responsive
2. Check for long-running queries
3. Increase connection timeout value
4. Add query timeout handling

```csharp
optionsBuilder.UseSqlServer(
    connection,
    sqlOptions =>
    {
        sqlOptions.CommandTimeout(30); // 30 second timeout
    }
);
```

---

## Performance Baseline Targets

| Metric | Target | Critical |
|--------|--------|----------|
| P95 Response Time | < 500ms | > 1000ms |
| P99 Response Time | < 1000ms | > 2000ms |
| Error Rate | < 0.1% | > 5% |
| CPU Usage | < 60% | > 85% |
| Memory Usage | < 70% | > 90% |
| RPS @ 100K VUs | > 1000 | < 500 |
| Database Connections | < 180 | >= MaxPoolSize |

---

## Next Steps

1. **Run Baseline Test**: Execute script locally to establish baseline metrics
2. **Identify Bottlenecks**: Analyze results and pinpoint slow components
3. **Implement Improvements**: Apply ASP.NET optimizations from checklist
4. **Re-test**: Run load test again to verify improvements
5. **Document**: Record metrics and configuration changes
6. **Plan Scaling**: Determine horizontal scaling needs

---

## Additional Resources

- [K6 Documentation](https://k6.io/docs/)
- [K6 Best Practices](https://k6.io/docs/misc/best-practices/)
- [ASP.NET Performance Best Practices](https://learn.microsoft.com/en-us/aspnet/core/performance)
- [Database Performance Tuning](https://learn.microsoft.com/en-us/sql/relational-databases/performance)
- [Load Testing Guide](https://en.wikipedia.org/wiki/Load_testing)

---

**Last Updated**: May 11, 2026  
**Version**: 1.0  
**Tested Scenarios**: 100 → 1,000 → 10,000 → 100,000 concurrent users
