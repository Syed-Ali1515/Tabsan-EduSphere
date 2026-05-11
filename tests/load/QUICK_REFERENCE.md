# K6 Load Testing - Quick Reference Card

**Purpose**: Simulate up to 1 million concurrent users logging into Tabsan-EduSphere

---

## ⚡ 30-Second Quick Start

```powershell
# 1. Install K6
choco install k6

# 2. Navigate to load tests
cd tests\load

# 3. Run light test locally
.\run-load-test.ps1 -Scenario light -Environment local

# Result: See response times and error rates
```

---

## 🎯 Common Commands

### Local Testing
```powershell
# Quick test (4 min)
.\run-load-test.ps1 light local

# Medium test (14 min)
.\run-load-test.ps1 medium local

# With JSON output
.\run-load-test.ps1 high local -OutputJson
```

### Staging/Production Testing
```powershell
# Staging environment
.\run-load-test.ps1 high staging

# Production (with confirmation)
.\run-load-test.ps1 extreme production

# Using K6 Cloud (distributed)
.\run-load-test.ps1 extreme production -CloudRun
```

### Direct K6 Commands
```bash
# Simple
k6 run login-load-test.js

# Custom target
k6 run -e TARGET_URL=http://api.example.com login-load-test.js

# Export results
k6 run --out json=results.json login-load-test.js
```

---

## 📊 Interpreting Results

### Expected Metrics (Healthy System)

```
✅ P95 Response: 350-500ms
✅ P99 Response: 800-1000ms
✅ Avg Response: 150-200ms
✅ Error Rate: < 0.1%
✅ Success Rate: > 99.9%
```

### Performance Scale

| VUs | Expected P95 | Status |
|-----|-------------|--------|
| 100 | 50-100ms | Excellent |
| 1,000 | 150-250ms | Good |
| 10,000 | 350-500ms | Acceptable |
| 100,000 | 800-1,500ms | Needs optimization |
| 1,000,000 | 2,000-5,000ms | Critical scaling needed |

---

## 🔴 Red Flags to Watch

| Issue | Meaning | Action |
|-------|---------|--------|
| P95 > 1000ms | Slow responses | Optimize code/DB |
| Error rate > 1% | High failures | Check server logs |
| 429 responses | Rate limited | Adjust load |
| 500 errors | Server crashes | Scale up |
| Connection pool errors | DB exhausted | Increase pool size |

---

## 🚀 Optimization Checklist

**Before Running Extreme Test:**

- [ ] Connection pooling configured (`Max Pool Size=200`)
- [ ] All DB queries are async
- [ ] Response compression enabled
- [ ] Rate limiting in place
- [ ] Caching strategy implemented
- [ ] Database indexes created
- [ ] Load balancer configured
- [ ] Monitoring/logging enabled

## 🧭 Phase 10 Progressive Gates

```powershell
# Standard gate sequence (10k -> 20k -> 50k -> 80k -> 100k)
.\run-phase10-progressive.ps1 progressive http://localhost:5181

# Extended gate sequence (adds 250k -> 500k -> 1m)
.\run-phase10-progressive.ps1 extended http://localhost:5181

# Re-test the same gate after a targeted fix
.\run-phase10-progressive.ps1 progressive http://localhost:5181 -RetestCount 3
```

The wrapper reports the first likely bottleneck class for each gate so you can apply a targeted fix before rerunning the same gate.

---

## 📚 Test Scenarios at a Glance

```
light    → 10 VUs × 4 min     (local dev)
medium   → 1K VUs × 14 min    (staging)
high     → 10K VUs × 27 min   (production-ready)
extreme  → 1M VUs × 60+ min   (scale test)
spike    → 100K VUs × 6 min   (sudden load)
soak     → 1K VUs × 2 hours   (stability)
stress   → ramp to 500K        (find limits)
```

---

## 💻 Script Files Reference

| File | Purpose | When to Use |
|------|---------|------------|
| `login-load-test.js` | Main test script | Always - base of all tests |
| `config-presets.js` | Test scenarios | For configuration variations |
| `run-load-test.ps1` | PowerShell runner | Recommended - best UX |
| `run-load-test.bat` | Batch runner | Alternative - Windows |
| `LOAD_TESTING_GUIDE.md` | Full documentation | Deep dive needed |

---

## 🔗 Key Files Location

```
tests/load/
├── login-load-test.js          ← Main test script
├── run-load-test.ps1           ← Run tests (PowerShell)
├── LOAD_TESTING_GUIDE.md       ← Full documentation
├── config-presets.js           ← Test configurations
├── run-load-test.bat           ← Run tests (Batch)
└── README.md                   ← This summary
```

---

## 🎓 Next Steps

1. **Run First Test**: `.\run-load-test.ps1 light local`
2. **Record Baseline**: Note P95, error rate
3. **Identify Issues**: Read analysis in console output
4. **Optimize**: Apply recommendations from guide
5. **Re-test**: Verify improvements
6. **Document**: Record final metrics

---

## 📞 Help & Resources

| Resource | Link |
|----------|------|
| K6 Docs | https://k6.io/docs/ |
| Full Guide | [LOAD_TESTING_GUIDE.md](LOAD_TESTING_GUIDE.md) |
| ASP.NET Tuning | [LOAD_TESTING_GUIDE.md](LOAD_TESTING_GUIDE.md#aspnet-scaling-improvements) |
| Troubleshooting | [LOAD_TESTING_GUIDE.md](LOAD_TESTING_GUIDE.md#troubleshooting-guide) |

---

## 🎯 Test Execution Timeline

### Quick Validation (30 min)
```powershell
.\run-load-test.ps1 light local      # 4 min
.\run-load-test.ps1 medium staging   # 14 min
(analyze)                             # 12 min
```

### Full Assessment (90 min)
```powershell
.\run-load-test.ps1 medium staging   # 14 min
.\run-load-test.ps1 high staging     # 27 min
.\run-load-test.ps1 extreme staging  # 60 min
(analyze and document)
```

### Production Validation (90+ min)
```powershell
.\run-load-test.ps1 extreme production -CloudRun
# (Distributed across cloud regions)
# Monitor dashboard at cloud.k6.io
```

---

## 💡 Pro Tips

1. **Start Light**: Always begin with `light` scenario locally
2. **Monitor Resources**: Watch CPU/Memory during tests
3. **Use JSON Export**: `-OutputJson` for detailed analysis
4. **Cloud for Scale**: K6 Cloud better for 100K+ VUs
5. **Batch Scripts**: Use PowerShell for easier management
6. **Version Control**: Commit successful test configs

---

## 🔐 Security Notes

- Test user credentials hardcoded in script (for load testing only)
- Don't run extreme tests against production without approval
- Coordinate with infrastructure team
- Have rollback plan ready
- Monitor production metrics during tests
- Alert on-call team before starting

---

**Last Updated**: May 11, 2026  
**K6 Version**: 0.47.0+  
**Max Scale**: 1,000,000 concurrent users  
**Status**: Production Ready
