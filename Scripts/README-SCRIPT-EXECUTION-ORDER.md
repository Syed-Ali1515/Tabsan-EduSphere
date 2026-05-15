# Tabsan EduSphere Database Scripts - Execution Guide

Repository Sync Note (15 May 2026):
- This guide now includes both the full demo path and the clean startup path.

## ⚠️ CRITICAL: Script Execution Order

**You MUST run these scripts in the exact order listed below. Skipping steps or running them out of order will cause errors.**

---

## Complete Execution Sequence

### Step 1: Create Database Schema (REQUIRED)
**File:** `01-Schema-Current.sql`

- Creates the database `[Tabsan-EduSphere]`
- Creates all tables, columns, and relationships
- Defines primary keys and foreign keys
- Establishes constraints and default values
- **Status:** Safe to run repeatedly; uses IF NOT EXISTS checks

**How to run:**
```bash
sqlcmd -S YOUR_SERVER -d master -i Scripts/01-Schema-Current.sql
# OR using Visual Studio:
# Right-click 01-Schema-Current.sql > Execute
```

**Expected output:**
- Multiple "CREATE TABLE" and "CREATE INDEX" messages
- No errors should occur
- Script should complete in 10-30 seconds

---

### Step 2: Seed Core Data (REQUIRED - choose one path)
**File:** `02-Seed-Core.sql`

**What it does:**
- Creates system roles: SuperAdmin, Admin, Faculty, Student
- Creates institutions: University, College, School
- Creates departments for each institution type
- Creates default users: superadmin, admin, faculty members
- Sets up modules and module permissions
- Initializes the role-based access control system

**Prerequisites:**
- ✅ `01-Schema-Current.sql` must complete successfully

**How to run:**
```bash
sqlcmd -S YOUR_SERVER -d Tabsan-EduSphere -i Scripts/02-Seed-Core.sql
```

**Expected output:**
- Multiple "MERGE" statements completing
- "Core seed data completed successfully." message
- No errors should occur
- Script should complete in 5-15 seconds

---

### Step 2A: Seed Clean Core Baseline (REQUIRED for clean startup)
**File:** `Seed-Core-Clean.sql`

**What it does:**
- Seeds startup-only baseline with no dummy/demo rows
- Seeds all core roles, only one superadmin user, baseline institution-type departments
- Seeds modules, module status, module-role permissions, baseline report/sidebar access
- Seeds required portal settings and optional baseline license row

**Prerequisites:**
- ✅ `01-Schema-Current.sql` must complete successfully

**How to run:**
```bash
sqlcmd -S YOUR_SERVER -d Tabsan-EduSphere -i Scripts/Seed-Core-Clean.sql
```

**Expected output:**
- "Seed-Core-Clean completed successfully." message
- No errors should occur
- Script should complete in 5-15 seconds

---

### Step 3: Add Comprehensive Test Data (OPTIONAL - demo path only)
**File:** `03-FullDummyData.sql`

**What it does:**
- Adds 100+ test records for demo/testing purposes
- Creates users across all roles (students, faculty, admin)
- Creates academic programs and courses (30+ entries)
- Creates buildings, rooms, and timetables
- Creates enrollments and assignments
- Creates attendance records and grades
- Creates quizzes, discussions, and notifications
- Maintains data parity across all institution types (School, College, University)

**Prerequisites:**
- ✅ `01-Schema-Current.sql` must complete successfully
- ✅ `02-Seed-Core.sql` OR `Seed-Core-Clean.sql` must complete successfully

**How to run:**
```bash
sqlcmd -S YOUR_SERVER -d Tabsan-EduSphere -i Scripts/03-FullDummyData.sql
```

**Expected output:**
- Multiple "(1 row affected)" messages as data is inserted
- "Full dummy demo data seeding completed." message
- No errors should occur
- Script should complete in 30-60 seconds

**If you see errors:**
- `Msg 1088: Cannot find the object "academic_programs"` = You skipped Step 1 or 2
- `Msg 3902: COMMIT without BEGIN TRANSACTION` = You ran this script out of order
- Solution: Run scripts 1, 2, 3 in exact sequence

---

### Step 4: Create Indexes and Views (OPTIONAL)
**File:** `04-Maintenance-Indexes-And-Views.sql`

**What it does:**
- Creates performance indexes on frequently queried columns
- Creates views for common reporting queries
- Optimizes database for search and reporting operations

**Prerequisites:**
- ✅ Steps 1, 2, 3 must complete successfully
- ⚠️ This step is optional but recommended for production

**How to run:**
```bash
sqlcmd -S YOUR_SERVER -d Tabsan-EduSphere -i Scripts/04-Maintenance-Indexes-And-Views.sql
```

**Expected output:**
- Multiple "CREATE INDEX" and "CREATE VIEW" messages
- No errors should occur
- Script should complete in 5-10 seconds

**Safe to run repeatedly:** Yes, safe to rerun after bulk imports

---

### Step 5: Validate Data Integrity (OPTIONAL)
**File:** `05-PostDeployment-Checks.sql`

**What it does:**
- Verifies that all schema objects exist
- Checks that core data was seeded correctly
- Provides summary statistics
- Validates institution types and role distribution
- Read-only validation - no data modifications

**Prerequisites:**
- ✅ Steps 1, 2, 3 should complete successfully
- Steps 4 is optional before running this

**How to run:**
```bash
sqlcmd -S YOUR_SERVER -d Tabsan-EduSphere -i Scripts/05-PostDeployment-Checks.sql
```

**Expected output:**
- Table with row counts for SchemaVersions, Roles, Modules, etc.
- Summary of data statistics
- No errors should occur
- Script should complete in 2-5 seconds

**Use this to verify deployment success:** Yes, use after running all scripts

---

### Step 5A: Validate Clean Baseline (OPTIONAL - clean path)
**File:** `05-PostDeployment-Checks-Clean.sql`

**What it does:**
- Verifies strict clean baseline after `Seed-Core-Clean.sql`
- Fails if dummy-domain data exists (programs/courses/enrollments/results/quizzes etc.)
- Validates startup permissions and access baseline (modules, role access, reports, sidebar)

**Prerequisites:**
- ✅ `01-Schema-Current.sql` must complete successfully
- ✅ `Seed-Core-Clean.sql` must complete successfully

**How to run:**
```bash
sqlcmd -S YOUR_SERVER -d Tabsan-EduSphere -i Scripts/05-PostDeployment-Checks-Clean.sql
```

**Expected output:**
- Table of check names with pass/fail values
- "Clean baseline checks passed." message when baseline is clean
- Script raises an error if any clean baseline check fails

---

## Quick Start (Copy-Paste)

If you have SQL Server installed and accessible via `sqlcmd`, paste this entire block into PowerShell:

```powershell
$server = "YOUR_SERVER"  # Replace with your SQL Server name (e.g., "localhost" or "DESKTOP-XXXXX")

cd "e:\Tabsan-EduSphere\Tabsan-EduSphere"

Write-Host "Step 1: Creating schema..." -ForegroundColor Cyan
sqlcmd -S $server -d master -i Scripts/01-Schema-Current.sql

Write-Host "Step 2: Seeding core data..." -ForegroundColor Cyan
sqlcmd -S $server -d Tabsan-EduSphere -i Scripts/02-Seed-Core.sql

Write-Host "Step 3: Adding test data..." -ForegroundColor Cyan
sqlcmd -S $server -d Tabsan-EduSphere -i Scripts/03-FullDummyData.sql

Write-Host "Step 4: Creating indexes..." -ForegroundColor Cyan
sqlcmd -S $server -d Tabsan-EduSphere -i Scripts/04-Maintenance-Indexes-And-Views.sql

Write-Host "Step 5: Running validation..." -ForegroundColor Cyan
sqlcmd -S $server -d Tabsan-EduSphere -i Scripts/05-PostDeployment-Checks.sql

Write-Host "✓ Database setup complete!" -ForegroundColor Green
```

### Clean Startup Quick Start (No Dummy Data)

```powershell
$server = "YOUR_SERVER"

cd "e:\Tabsan-EduSphere\Tabsan-EduSphere"

Write-Host "Step 1: Creating schema..." -ForegroundColor Cyan
sqlcmd -S $server -d master -i Scripts/01-Schema-Current.sql

Write-Host "Step 2A: Seeding clean core baseline..." -ForegroundColor Cyan
sqlcmd -S $server -d Tabsan-EduSphere -i Scripts/Seed-Core-Clean.sql

Write-Host "Step 5A: Running clean baseline validation..." -ForegroundColor Cyan
sqlcmd -S $server -d Tabsan-EduSphere -i Scripts/05-PostDeployment-Checks-Clean.sql

Write-Host "✓ Clean baseline setup complete!" -ForegroundColor Green
```

---

## Troubleshooting

### Error: "Database does not exist"
- You skipped Step 1
- **Solution:** Run `01-Schema-Current.sql` first

### Error: "Cannot find the object 'academic_programs'"
- You ran Step 3 before Step 2
- **Solution:** Run steps 1, 2, 3 in exact order

### Error: "COMMIT TRANSACTION request has no corresponding BEGIN TRANSACTION"
- The scripts were run out of order or interrupted
- **Solution:** Drop the database and start over from Step 1

### Script seems hung/slow
- The test data script adds 100+ records which takes time
- **Patience:** Script 3 can take 30-60 seconds; this is normal
- **Monitor:** Check SQL Server Activity Monitor for progress

### How to reset and start over
```sql
-- WARNING: This deletes everything!
DROP DATABASE [Tabsan-EduSphere];
```

Then run all 5 scripts again from the beginning.

---

## Script Details

| Script | Type | Time | Required? | Idempotent? |
|--------|------|------|-----------|-------------|
| 01-Schema-Current.sql | DDL | 10-30s | ✅ YES | ✅ YES |
| 02-Seed-Core.sql | DML | 5-15s | ✅ YES (full/demo path) | ✅ YES |
| Seed-Core-Clean.sql | DML | 5-15s | ✅ YES (clean path) | ✅ YES |
| 03-FullDummyData.sql | DML | 30-60s | ⚠️ Optional (demo path) | ✅ YES |
| 04-Maintenance | DDL | 5-10s | ⚠️ Optional | ✅ YES |
| 05-PostDeployment | SELECT | 2-5s | ⚠️ Optional | ✅ YES |
| 05-PostDeployment-Checks-Clean.sql | SELECT | 2-5s | ⚠️ Optional (clean path) | ✅ YES |

**Notes:**
- **Required:** Must run for full functionality
- **Optional:** Recommended but not required
- **Idempotent:** Safe to run multiple times without errors

---

## Data Content After Execution

After running the full demo path (`01 -> 02 -> 03 -> 04 -> 05`), your database will contain:

**Institutions:**
- 1 University (InstitutionType = 2)
- 1 College (InstitutionType = 1)
- 1 School (InstitutionType = 0)

**Users:**
- 1 SuperAdmin
- 3 Admin users (one per institution)
- 15+ Faculty members
- 30+ Students

**Academic Content:**
- 3 Departments per institution (9 total)
- 18+ Academic Programs
- 30+ Courses
- 18 Course Offerings

**Operational Data:**
- 15+ Assignments with submissions
- 6 Quizzes with questions and answers
- 30+ Attendance records
- 30+ Grade records
- 10 Payment receipts

**System Data:**
- 4 System Roles (SuperAdmin, Admin, Faculty, Student)
- 14 Modules with permissions
- Multiple buildings, rooms, and timetables

---

## Questions or Issues?

If scripts fail:
1. Check the error message - it usually tells you what went wrong
2. Verify you're running scripts in the correct order
3. Ensure prerequisites are met (database exists, schema created, core data seeded)
4. Check SQL Server is running and accessible
5. Verify you have appropriate permissions

**For support:** Check the Docs/ folder for additional documentation.

---

**Last Updated:** May 2026  
**Database Version:** Tabsan EduSphere v1.0  
**Schema:** Current (Entity Framework Core 8.0.8 compatible)
