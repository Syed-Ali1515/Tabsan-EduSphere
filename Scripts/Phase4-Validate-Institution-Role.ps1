param(
    [Parameter(Mandatory = $false)]
    [string]$ApiBaseUrl = "http://localhost:5181",

    [Parameter(Mandatory = $false)]
    [string]$OutputRoot = "Artifacts/Phase4/Api"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function New-DirIfMissing {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path | Out-Null
    }
}

function Invoke-Api {
    param(
        [Parameter(Mandatory = $true)][ValidateSet("GET", "POST")][string]$Method,
        [Parameter(Mandatory = $true)][string]$Url,
        [Parameter(Mandatory = $false)][string]$Token,
        [Parameter(Mandatory = $false)]$Body
    )

    $headers = @{}
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }

    if ($Method -eq "GET") {
        return Invoke-RestMethod -Method Get -Uri $Url -Headers $headers
    }

    return Invoke-RestMethod -Method Post -Uri $Url -Headers $headers -Body ($Body | ConvertTo-Json -Depth 10) -ContentType "application/json"
}

function Save-Json {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]$Data
    )

    $json = $Data | ConvertTo-Json -Depth 20
    Set-Content -Path $Path -Value $json -Encoding UTF8
}

function Get-Timestamp {
    return (Get-Date).ToString("yyyyMMdd-HHmmss")
}

New-DirIfMissing -Path $OutputRoot

Write-Host "=== Phase 4 API Validation Helper ==="
Write-Host "API Base URL: $ApiBaseUrl"
Write-Host "Output Root: $OutputRoot"
Write-Host ""
Write-Host "Provide tokens for each role (press Enter to skip a role)."

$roleTokens = @{}
$roles = @("SuperAdmin", "Admin", "Faculty", "Student")

foreach ($role in $roles) {
    $token = Read-Host "Token for $role"
    $roleTokens[$role] = $token
}

$timestamp = Get-Timestamp

# Global policy evidence
$policy = Invoke-Api -Method GET -Url "$ApiBaseUrl/api/v1/institution-policy"
$labels = Invoke-Api -Method GET -Url "$ApiBaseUrl/api/v1/labels"
$matrix = Invoke-Api -Method GET -Url "$ApiBaseUrl/api/v1/portal-capabilities/matrix"

Save-Json -Path (Join-Path $OutputRoot "Policy_$timestamp.json") -Data $policy
Save-Json -Path (Join-Path $OutputRoot "Labels_$timestamp.json") -Data $labels
Save-Json -Path (Join-Path $OutputRoot "CapabilitiesMatrix_$timestamp.json") -Data $matrix

Write-Host "Saved base policy evidence files."

# Negative route checks template
$negativeChecks = @(
    @{ Name = "AdminOnlyRoute"; Url = "$ApiBaseUrl/api/v1/admin/users"; Expected = "403-or-filtered" },
    @{ Name = "SuperAdminOnlyRoute"; Url = "$ApiBaseUrl/api/v1/license/details"; Expected = "403-or-filtered" },
    @{ Name = "ReportRoute"; Url = "$ApiBaseUrl/api/v1/reports/summary"; Expected = "scoped-or-denied" }
)

foreach ($role in $roles) {
    $token = $roleTokens[$role]
    if (-not $token) {
        Write-Host "Skipping negative checks for $role (no token provided)."
        continue
    }

    foreach ($check in $negativeChecks) {
        $outName = "${role}_Negative_${($check.Name)}_$timestamp.json"
        $outPath = Join-Path $OutputRoot $outName

        try {
            $result = Invoke-Api -Method GET -Url $check.Url -Token $token
            Save-Json -Path $outPath -Data @{
                role = $role
                route = $check.Url
                expected = $check.Expected
                observed = "allowed-or-filtered"
                payload = $result
            }
        }
        catch {
            Save-Json -Path $outPath -Data @{
                role = $role
                route = $check.Url
                expected = $check.Expected
                observed = "denied-or-error"
                error = $_.Exception.Message
            }
        }
    }
}

Write-Host "Saved negative-check evidence files."
Write-Host "Done. Attach outputs to Phase 4 validation summary in docs."
