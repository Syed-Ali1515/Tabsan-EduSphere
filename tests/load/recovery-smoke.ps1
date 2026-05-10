param(
    [string]$ApiProject = "src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj",
    [string]$BaseUrl = "http://localhost:5181",
    [int]$StartupTimeoutSeconds = 90,
    [int]$RecoveryTimeoutSeconds = 90,
    [string]$ConnectionString = ""
)

# Final-Touches Phase 31 Stage 31.3 — node/service recovery smoke certification.

$ErrorActionPreference = "Stop"

function Test-Health {
    param([string]$Url)

    try {
        $res = Invoke-WebRequest -Uri "$Url/health" -Method Get -UseBasicParsing -TimeoutSec 5
        return $res.StatusCode -eq 200
    }
    catch {
        return $false
    }
}

function Wait-ForHealthy {
    param(
        [string]$Url,
        [System.Diagnostics.Process]$TargetProcess,
        [int]$TimeoutSeconds,
        [string]$Phase
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    while ((Get-Date) -lt $deadline) {
        if ($TargetProcess.HasExited) {
            throw "$Phase failed: API process exited early (exit code $($TargetProcess.ExitCode)). Verify SQL Server connectivity and appsettings connection strings."
        }

        if (Test-Health -Url $Url) {
            return
        }

        Start-Sleep -Seconds 2
    }

    throw "$Phase failed: /health did not become ready within $TimeoutSeconds seconds at $Url/health."
}

Write-Host "[Stage31.3] Starting API process for recovery smoke..."
$processArgs = @("run", "--project", $ApiProject, "--no-build", "--no-launch-profile", "--urls", $BaseUrl)

function Start-ApiProcess {
    param(
        [string[]]$ApiArgs,
        [string]$ConnString
    )

    $hadPrevious = Test-Path Env:ConnectionStrings__DefaultConnection
    $previous = $env:ConnectionStrings__DefaultConnection

    try {
        if ([string]::IsNullOrWhiteSpace($ConnString)) {
            if ($hadPrevious) {
                $env:ConnectionStrings__DefaultConnection = $previous
            } else {
                Remove-Item Env:ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
            }
        } else {
            $env:ConnectionStrings__DefaultConnection = $ConnString
        }

        $argLine = ($ApiArgs | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join " "
        return Start-Process dotnet -ArgumentList $argLine -PassThru
    }
    finally {
        if ($hadPrevious) {
            $env:ConnectionStrings__DefaultConnection = $previous
        } else {
            Remove-Item Env:ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue
        }
    }
}

$process = Start-ApiProcess -ApiArgs $processArgs -ConnString $ConnectionString

try {
    # Final-Touches Phase 31 Stage 31.3 — fail fast with actionable startup diagnostics.
    Wait-ForHealthy -Url $BaseUrl -TargetProcess $process -TimeoutSeconds $StartupTimeoutSeconds -Phase "Initial startup"

    Write-Host "[Stage31.3] Initial health check passed. Simulating node failure..."
    Stop-Process -Id $process.Id -Force

    Write-Host "[Stage31.3] Restarting API process..."
    $process = Start-ApiProcess -ApiArgs $processArgs -ConnString $ConnectionString

    Wait-ForHealthy -Url $BaseUrl -TargetProcess $process -TimeoutSeconds $RecoveryTimeoutSeconds -Phase "Recovery"

    Write-Host "[Stage31.3] Recovery smoke passed. Service restored and healthy."
    exit 0
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
