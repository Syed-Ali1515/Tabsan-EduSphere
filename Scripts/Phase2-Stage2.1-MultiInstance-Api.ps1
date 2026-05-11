param(
    [int]$InstanceCount = 4,
    [int]$BasePort = 5181,
    [switch]$Stop
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $repoRoot "src\Tabsan.EduSphere.API\Tabsan.EduSphere.API.csproj"
$pidFile = Join-Path $PSScriptRoot ".phase2-stage2.1-api-pids.txt"

if ($Stop) {
    if (-not (Test-Path $pidFile)) {
        Write-Host "No PID file found: $pidFile"
        exit 0
    }

    Get-Content $pidFile |
        Where-Object { $_ -match '^\d+$' } |
        ForEach-Object {
            try {
                Stop-Process -Id ([int]$_) -Force -ErrorAction Stop
                Write-Host "Stopped API process PID $_"
            }
            catch {
                Write-Host "PID $_ is not running."
            }
        }

    Remove-Item $pidFile -Force
    Write-Host "Stopped all Stage 2.1 local API instances."
    exit 0
}

if (-not (Test-Path $apiProject)) {
    throw "API project not found: $apiProject"
}

$startedPids = @()

for ($i = 0; $i -lt $InstanceCount; $i++) {
    $port = $BasePort + $i
    $instanceId = "api-node-$($i + 1)"

    $launchCommand = @"
`$env:ScaleOut__InstanceId = '$instanceId'
dotnet run --project `"$apiProject`" --no-launch-profile --urls http://localhost:$port
"@

    $proc = Start-Process -FilePath "powershell" `
        -ArgumentList @("-NoProfile", "-Command", $launchCommand) `
        -WorkingDirectory $repoRoot `
        -PassThru

    $startedPids += $proc.Id

    Write-Host "Started $instanceId on http://localhost:$port (PID=$($proc.Id))"
    Write-Host "Health check: http://localhost:$port/health/instance"
}

$startedPids | Set-Content $pidFile
Write-Host "Saved PIDs to $pidFile"
Write-Host "Use -Stop to stop all started instances."
