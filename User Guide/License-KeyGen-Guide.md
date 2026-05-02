# License KeyGen User Guide

Version: 1.0  
Date: 02 May 2026  
Applies to: tools/KeyGen/KeyGen.cs

## 1. Purpose

This guide explains how to use the license creation utility located in tools/KeyGen to generate cryptographic keys and issue licenses for Tabsan EduSphere.

The utility generates:
- RSA private key (PKCS#1 PEM)
- RSA public key (PKCS#1 PEM)
- AES-256 key (Base64)

These keys support secure license generation and verification workflows.

## 2. Prerequisites

- .NET 8 SDK installed
- Access to repository root
- Permission to handle cryptographic material

Optional but recommended:
- Secret manager or secure vault for key storage
- Controlled workstation (no shared desktop)

## 3. File Location

- Utility source: tools/KeyGen/KeyGen.cs
- Project file: tools/KeyGen/KeyGen.csproj

## 4. Running the Utility

From repository root, run:

1. dotnet run --project tools/KeyGen

The tool prints three outputs to terminal:
- RSA Private Key PEM block
- RSA Public Key PEM block
- AES-256 key in Base64

## 5. Output Meaning

### 5.1 RSA Private Key

Use for signing license payloads.

Critical rule:
- Never commit private key to source control.
- Never place private key on public or shared servers.

### 5.2 RSA Public Key

Use in application verification side to verify signed license data.

This key can be distributed to application environments as needed.

### 5.3 AES-256 Key

Use for symmetric encryption in your license packaging process if your internal workflow requires encrypted payload transfer.

Store and rotate this key per institutional cryptographic policy.

## 6. Recommended Secure Workflow

1. Run KeyGen on a secure machine.
2. Copy private key to vault only.
3. Copy public key to deployment secrets/config used by the app verifier.
4. Store AES key in vault with strict access controls.
5. Record key creation date, owner, and purpose.

## 7. Integrating with License Issuance Process

Suggested process:
1. Build a license payload (organization, license type, issue date, expiry date, module scope).
2. Sign payload with RSA private key.
3. Optionally encrypt payload package if policy requires it.
4. Deliver final license artifact to SuperAdmin.
5. SuperAdmin uploads license in License Update screen.
6. Confirm app shows active license state.

## 8. Verification in EduSphere

After upload, validate:
- License status is Active
- License type matches expected plan
- Expiry date is correct (if non-permanent)
- Modules expected under license are available

If verification fails:
- Re-check key pair consistency
- Confirm payload signature process
- Confirm no corruption during transfer

## 9. Key Rotation Policy (Recommended)

Define a policy covering:
- Rotation interval
- Emergency rotation trigger
- Legacy key grace period
- Revocation and re-issuance process

Minimum operational standard:
- Rotate keys on administrator turnover
- Rotate immediately on suspected key exposure

## 10. Troubleshooting

1. dotnet run fails:
- Verify .NET SDK installation
- Run restore: dotnet restore tools/KeyGen/KeyGen.csproj

2. Output is malformed after copy/paste:
- Ensure PEM header and footer lines remain intact
- Preserve line breaks in key block

3. License rejected by app:
- Check public key configured in verifier
- Check payload signing algorithm compatibility
- Check issue/expiry timestamps and timezone handling

## 11. Security Warnings

- Do not store keys in plain text files inside repository.
- Do not send private keys over chat/email.
- Do not embed private keys in application binaries.
- Audit all key access requests.

## 12. Quick Operational Checklist

Before issuing a license:
- Keys generated and stored securely
- Payload reviewed and approved
- Signature verified
- Upload tested in non-production first

After issuing a license:
- Activation confirmed by SuperAdmin
- Audit record updated
- Renewal reminder scheduled
