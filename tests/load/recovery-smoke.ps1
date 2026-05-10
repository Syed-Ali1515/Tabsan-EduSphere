param(
    [string]$ApiProject = "src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj",
    [string]$BaseUrl = "http://localhost:5000",
    [int]$WarmupSeconds = 12,
    [int]$RecoveryTimeoutSeconds = 90
)

# Final-Touches Phase 31 Stage 31.3 — node/service recovery smoke certification.

$ErrorActionPreference = "Stop"

function Test-Health {
    param([string]$Url)

    try {
        $res = Invoke-WebRequest -Uri "$Url/health" -Method Get -TimeoutSec 5
        return $res.StatusCode -eq 200
    }
    catch {
        return $false
    }
}

Write-Host "[Stage31.3] Starting API process for recovery smoke..."
$process = Start-Process dotnet -ArgumentList @("run", "--project", $ApiProject, "--no-build") -PassThru

try {
    Start-Sleep -Seconds $WarmupSeconds

    if (-not (Test-Health -Url $BaseUrl)) {
        throw "Initial health check failed at $BaseUrl/health"
    }

    Write-Host "[Stage31.3] Initial health check passed. Simulating node failure..."
    Stop-Process -Id $process.Id -Force

    Write-Host "[Stage31.3] Restarting API process..."
    $process = Start-Process dotnet -ArgumentList @("run", "--project", $ApiProject, "--no-build") -PassThru

    $deadline = (Get-Date).AddSeconds($RecoveryTimeoutSeconds)
    $recovered = $false

    while ((Get-Date) -lt $deadline) {
        if (Test-Health -Url $BaseUrl) {
            $recovered = $true
            break
        }

        Start-Sleep -Seconds 2
    }

    if (-not $recovered) {
        throw "Recovery health check did not pass within $RecoveryTimeoutSeconds seconds."
    }

    Write-Host "[Stage31.3] Recovery smoke passed. Service restored and healthy."
    exit 0
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
