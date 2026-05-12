# User Import Sheets

This folder contains CSV templates for bulk-importing user accounts via the admin portal.

## Template: `user-import-template.csv`

### Columns

| Column       | Required | Description                                                                 |
|--------------|----------|-----------------------------------------------------------------------------|
| Username     | Yes      | Unique login handle. Used as the initial password on import.                |
| Email        | No       | Optional email address. Must be unique if provided.                         |
| FullName     | No       | Display name (stored for reference; not enforced by the import).            |
| Role         | Yes      | Must be one of: `Admin`, `Faculty`, `Student` (SuperAdmin not allowed).     |
| DepartmentId | No       | GUID of the department. Required for Faculty accounts; optional for others. |
| InstitutionType | No    | Explicit per-user institution assignment: `School`, `College`, or `University`. |

### Rules

- The **initial password** for every imported user is set to their **Username** (case-sensitive).
- All imported users are flagged as **MustChangePassword = true**. They will be prompted to set a new password on their first login.
- Rows with duplicate usernames (across the file or already in the database) are skipped and counted as duplicates.
- Rows with missing required fields or invalid values are skipped and reported as errors.
- If `InstitutionType` is provided, it must be enabled in the active institution license policy.

### How to import

1. Open the Admin Portal → User Management → Import Users.
2. Select your completed CSV file and click **Upload**.
3. Review the import summary (imported / duplicates / errors).
4. Share the generated usernames and initial-login instructions with the new users.
