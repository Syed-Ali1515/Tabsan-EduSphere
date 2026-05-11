# Tabsan-Lic — License Generation Tool

A **standalone .NET 8 console application** for generating encrypted `.tablic` license files for Tabsan EduSphere.

## ⚠️ Important: Separate Application

**Tabsan-Lic is NOT part of the main EduSphere application.**

- **Location**: `tools/Tabsan.Lic/`
- **Solution**: `tools/Tabsan.Lic/Tabsan.Lic.sln` (independent)
- **Main EduSphere Solution**: `Tabsan.EduSphere.sln` (does NOT include Tabsan-Lic)
- **Deployment**: Build and publish separately from the main EduSphere app

## Isolation

✅ Tabsan-Lic is excluded from `dotnet publish Tabsan.EduSphere.sln`  
✅ Tabsan-Lic has its own dedicated solution file  
✅ No dependencies flow back to the main EduSphere codebase

## Building Tabsan-Lic

### From the root directory:
```bash
dotnet build tools/Tabsan.Lic/Tabsan.Lic.sln
```

### From the tool directory:
```bash
cd tools/Tabsan.Lic
dotnet build
```

## Publishing Tabsan-Lic

Publish as a self-contained executable:

```bash
dotnet publish tools/Tabsan.Lic/Tabsan.Lic.sln -c Release -r win-x64 --self-contained
```

Output: `tools/Tabsan.Lic/bin/Release/net8.0/win-x64/publish/tabsan-lic.exe`

## Running Tabsan-Lic

Interactive menu:

```bash
./tabsan-lic.exe
```

Or from source:

```bash
cd tools/Tabsan.Lic
dotnet run
```

## Database

- **Location**: `%APPDATA%/Tabsan/tabsan_lic.db` (SQLite)
- **Auto-created** on first run
- Contains: issued keys, hashes, expiry types, institution scope, generation timestamps, labels

## Features

1. **Generate Keys** — Single or bulk generation with 1 month, 1/2/3 year, or Permanent expiry
2. **Build `.tablic` Files** — Choose School, College, University, max users, domain lock, then AES-256 encrypt + RSA-2048 sign
3. **List Keys** — View all issued keys with metadata
4. **Export CSV** — Audit trail of generated keys for vendor records

## Architecture

| Component | Purpose |
|-----------|---------|
| `Crypto/LicCrypto.cs` | AES-256-CBC + RSA-2048 PKCS#1 encryption/signing |
| `Services/KeyService.cs` | Key generation, storage, listing |
| `Services/LicenseBuilder.cs` | `.tablic` file builder |
| `Data/LicDb.cs` | SQLite EF Core context |

## Deployment Checklist

- [ ] Build `tools/Tabsan.Lic/Tabsan.Lic.sln` independently
- [ ] Test key generation and `.tablic` file creation
- [ ] Publish as standalone executable
- [ ] Distribute to vendor/Super Admin only
- [ ] Verify EduSphere build does NOT include Tabsan-Lic binaries
- [ ] Document deployment path (vendor internal tool, not part of portal)

## Interaction with EduSphere

**Tabsan-Lic generates `.tablic` files** → **Super Admin uploads to EduSphere** → **EduSphere validates & applies license**

- EduSphere imports only the RSA public key + AES key (embedded in `Infrastructure/Licensing/EmbeddedKeys.cs`)
- Tabsan-Lic keeps the RSA private key (never shared)
- One-way flow: Tabsan-Lic → `.tablic` file → EduSphere
