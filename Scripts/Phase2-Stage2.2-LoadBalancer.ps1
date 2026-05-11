param(
    [int]$ApiBasePort = 5181,
    [int]$InstanceCount = 4,
    [int]$LoadBalancerPort = 8080,
    [string]$ContainerName = "tabsan-edusphere-lb",
    [switch]$Stop
)

$ErrorActionPreference = "Stop"
$templatePath = Join-Path $PSScriptRoot "Phase2-Stage2.2-nginx-leastconn.conf.template"
$generatedConfigPath = Join-Path $PSScriptRoot ".phase2-stage2.2-nginx.conf"

if (-not (Test-Path $templatePath)) {
    throw "Template file not found: $templatePath"
}

$dockerCommand = Get-Command docker -ErrorAction SilentlyContinue
if (-not $dockerCommand) {
    throw "Docker CLI was not found. Install Docker Desktop or run Stage 2.2 in an environment with Docker available."
}

if ($Stop) {
    $existing = docker ps -a --filter "name=^/$ContainerName$" --format "{{.Names}}"
    if ($existing -contains $ContainerName) {
        docker rm -f $ContainerName | Out-Null
        Write-Host "Stopped and removed load balancer container: $ContainerName"
    }
    else {
        Write-Host "No container found with name $ContainerName"
    }

    if (Test-Path $generatedConfigPath) {
        Remove-Item $generatedConfigPath -Force
        Write-Host "Removed generated config: $generatedConfigPath"
    }

    exit 0
}

$serverLines = for ($i = 0; $i -lt $InstanceCount; $i++) {
    $port = $ApiBasePort + $i
    "        server host.docker.internal:$port max_fails=3 fail_timeout=10s;"
}

$template = Get-Content $templatePath -Raw
$configText = $template.Replace("__UPSTREAM_SERVERS__", ($serverLines -join "`n"))
Set-Content -Path $generatedConfigPath -Value $configText -NoNewline

$existingContainer = docker ps -a --filter "name=^/$ContainerName$" --format "{{.Names}}"
if ($existingContainer -contains $ContainerName) {
    docker rm -f $ContainerName | Out-Null
}

docker run -d `
    --name $ContainerName `
    -p "$LoadBalancerPort`:80" `
    -v "${generatedConfigPath}:/etc/nginx/nginx.conf:ro" `
    nginx:1.27-alpine | Out-Null

Write-Host "Started least-connections load balancer container: $ContainerName"
Write-Host "Load balancer URL: http://localhost:$LoadBalancerPort"
Write-Host "LB self health:   http://localhost:$LoadBalancerPort/lb-health"
Write-Host "API health proxy: http://localhost:$LoadBalancerPort/health/instance"
Write-Host ""
Write-Host "Tip: run Scripts/Phase2-Stage2.2-Validate-LB.ps1 -LoadBalancerPort $LoadBalancerPort to inspect instance distribution."
