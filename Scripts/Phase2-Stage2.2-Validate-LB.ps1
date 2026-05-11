param(
    [int]$LoadBalancerPort = 8080,
    [int]$Requests = 120,
    [string]$Path = "/health/instance"
)

$ErrorActionPreference = "Stop"
$url = "http://localhost:$LoadBalancerPort$Path"
$counts = @{}

for ($i = 1; $i -le $Requests; $i++) {
    try {
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5

        $instanceFromHeader = $response.Headers["X-EduSphere-Instance"]
        $instance = $instanceFromHeader

        if ([string]::IsNullOrWhiteSpace($instance) -and -not [string]::IsNullOrWhiteSpace($response.Content)) {
            try {
                $parsed = $response.Content | ConvertFrom-Json
                $instance = $parsed.instanceId
            }
            catch {
                $instance = "unknown"
            }
        }

        if ([string]::IsNullOrWhiteSpace($instance)) {
            $instance = "unknown"
        }

        if (-not $counts.ContainsKey($instance)) {
            $counts[$instance] = 0
        }
        $counts[$instance]++
    }
    catch {
        if (-not $counts.ContainsKey("request-error")) {
            $counts["request-error"] = 0
        }
        $counts["request-error"]++
    }
}

Write-Host "Load balancer distribution sample ($Requests requests):"
$counts.GetEnumerator() |
    Sort-Object -Property Name |
    ForEach-Object {
        $ratio = [Math]::Round(($_.Value / [double]$Requests) * 100, 2)
        Write-Host ("{0,-30} {1,6} ({2,6}%)" -f $_.Name, $_.Value, $ratio)
    }
