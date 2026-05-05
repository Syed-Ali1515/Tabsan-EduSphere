<#
.SYNOPSIS
    Generates an Argon2id password hash and patches the seed SQL scripts.

.DESCRIPTION
    Prompts for a password, hashes it using the same Argon2id parameters
    used by the EduSphere API (memory=65536 KB, iterations=3, parallelism=4),
    and replaces the PLACEHOLDER in both seed scripts with the real hash.

.REQUIREMENTS
    • .NET 8 SDK installed
    • Run from the repository root (where Tabsan.EduSphere.sln lives)
#>

[CmdletBinding()]
param(
    [Parameter(HelpMessage = 'Password to hash. Prompted interactively if omitted.')]
    [string] $Password
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── 1. Collect password ──────────────────────────────────────────────────────
if (-not $Password) {
    $secPwd = Read-Host -Prompt 'Enter test-user password' -AsSecureString
    $BSTR   = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secPwd)
    $Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
}

if ([string]::IsNullOrWhiteSpace($Password)) {
    Write-Error 'Password must not be empty.'
    exit 1
}

# ── 2. Generate hash using a small inline C# snippet ────────────────────────
$script = @"
using Konscious.Security.Cryptography;
using System;
using System.Security.Cryptography;
using System.Text;

var password = Environment.GetEnvironmentVariable("HS_PWD") ?? throw new Exception("HS_PWD not set");
var salt = new byte[32];
RandomNumberGenerator.Fill(salt);

using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
{
    Salt            = salt,
    MemorySize      = 65536,
    Iterations      = 3,
    DegreeOfParallelism = 4
};
var hash = argon2.GetBytes(32);
Console.Write($"argon2id:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}");
"@

# Write temporary project
$tmpDir = Join-Path $env:TEMP 'TabsanHashGen'
if (Test-Path $tmpDir) { Remove-Item $tmpDir -Recurse -Force }
New-Item -ItemType Directory -Path $tmpDir | Out-Null

$scriptFile  = Join-Path $tmpDir 'Program.cs'
$projectFile = Join-Path $tmpDir 'HashGen.csproj'

Set-Content -Path $scriptFile -Value $script -Encoding UTF8

@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Konscious.Security.Cryptography.Argon2" Version="1.3.1" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path $projectFile -Encoding UTF8

# ── 3. Build + run ───────────────────────────────────────────────────────────
Write-Host 'Building hash generator (first run downloads ~1 MB NuGet package)...' -ForegroundColor Cyan
$env:HS_PWD = $Password
$hash = & dotnet run --project $projectFile --no-launch-profile 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error "Hash generation failed:`n$hash"
    exit 1
}
Remove-Item $env:HS_PWD -ErrorAction SilentlyContinue  # clear var
$env:HS_PWD = ''

$hash = $hash.Trim()
Write-Host "`nGenerated hash:`n  $hash`n" -ForegroundColor Green

# ── 4. Patch SQL scripts ─────────────────────────────────────────────────────
$scriptDir   = Join-Path $PSScriptRoot '.'
$scriptsRoot = $PSScriptRoot

$placeholder = 'PLACEHOLDER_RUN_GenerateTestHashes.ps1_TO_REPLACE'

$sqlFiles = @(
    Join-Path $scriptsRoot '1-MinimalSeed.sql'
    Join-Path $scriptsRoot '2-FullDummyData.sql'
)

foreach ($sqlFile in $sqlFiles) {
    if (-not (Test-Path $sqlFile)) {
        Write-Warning "Script not found: $sqlFile (skipping)"
        continue
    }
    $content = Get-Content $sqlFile -Raw -Encoding UTF8
    if ($content -match [regex]::Escape($placeholder)) {
        $content = $content -replace [regex]::Escape($placeholder), $hash
        Set-Content -Path $sqlFile -Value $content -Encoding UTF8 -NoNewline
        Write-Host "Patched: $(Split-Path $sqlFile -Leaf)" -ForegroundColor Green
    } else {
        Write-Host "$(Split-Path $sqlFile -Leaf) — placeholder not found (already patched?)" -ForegroundColor Yellow
    }
}

# ── 5. Clean up temp files ───────────────────────────────────────────────────
Remove-Item $tmpDir -Recurse -Force

Write-Host "`nDone. Run the SQL scripts against TabsanEduSphere database.`n" -ForegroundColor Cyan
