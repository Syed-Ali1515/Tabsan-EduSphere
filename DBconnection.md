# DB Connection Guide (Cloud/Portal Deployment)

This document explains what must be updated so Tabsan EduSphere can connect to the database after deployment.

## 1) Where DB config is read in code

- The API uses `ConnectionStrings:DefaultConnection`.
- If `DefaultConnection` is missing, startup fails.
- DbContext registration uses SQL Server provider.

Code locations:
- `src/Tabsan.EduSphere.API/Program.cs`
- `src/Tabsan.EduSphere.API/appsettings.Production.json`

## 2) Files and settings you must update

### A) Production app settings template

Update this file for production defaults (non-secret placeholders only):
- `src/Tabsan.EduSphere.API/appsettings.Production.json`

Key:
- `ConnectionStrings:DefaultConnection`

Current placeholder value in project:
- `REPLACE_WITH_PRODUCTION_CONNECTION_STRING_OR_SET_VIA_ENV_VAR;Min Pool Size=50;Max Pool Size=800;Connect Timeout=30`

### B) Recommended: environment variables in cloud

In production, set secrets via environment variables (not committed files):

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=<your sql connection string>`

Strongly recommended related production keys:
- `JwtSettings__SecretKey=<strong secret, 32+ chars>`
- `ScaleOut__RedisConnectionString=<redis connection string>`
- `TABSAN_SUPER_PASSWORD=<initial super admin password>`
- `TABSAN_SUPER_USERNAME=superadmin` (optional, default is `superadmin`)
- `TABSAN_SUPER_EMAIL=<admin email>` (optional)

## 3) Super admin login behavior (important)

The project does not hardcode all user passwords in scripts.

Bootstrap behavior:
- On startup, if no user with role `SuperAdmin` exists, app seeds one account.
- Username defaults to `superadmin` unless `TABSAN_SUPER_USERNAME` is set.
- Password is read from `TABSAN_SUPER_PASSWORD`.
- If `TABSAN_SUPER_PASSWORD` is missing on first seed, startup throws an error.

After first login:
- Create Admin, Faculty, and Student users from the app/admin workflow.

## 4) Connection string examples

### SQL Auth example

`Server=tcp:your-sql-host,1433;Database=TabsanEduSphere;User ID=your_user;Password=your_password;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;Min Pool Size=50;Max Pool Size=800;Connect Timeout=30`

### Windows/Integrated Auth example (VM/domain scenarios)

`Server=your-sql-host;Database=TabsanEduSphere;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;Min Pool Size=50;Max Pool Size=800;Connect Timeout=30`

## 5) Cloud deployment checklist

1. Provision SQL Server and create database `TabsanEduSphere`.
2. Apply schema/seed scripts or migrations.
3. Set production environment variables in hosting portal.
4. Set `ConnectionStrings__DefaultConnection` to production DB.
5. Set `TABSAN_SUPER_PASSWORD` before first app start.
6. Start API and verify logs show app started successfully.
7. Open API URL and test authentication.
8. Log in as super admin, then create remaining users/roles.

## 6) Quick verification after deploy

- App startup should not throw `DefaultConnection string is missing`.
- API health endpoint should respond successfully.
- Login with super admin should work using seeded credentials.
- DB should show successful read/write from app operations.

## 7) Security notes

- Never store production DB password in committed files.
- Use cloud secret manager or environment variables.
- Rotate `TABSAN_SUPER_PASSWORD` and JWT secret after first secure setup.
- Restrict SQL firewall/IP access to app hosts only.
