# JMeter Test Runner for Study Designer Performance Tests
param(
    [Parameter(Mandatory=$false)]
    [int]$Users = 50,
    
    [Parameter(Mandatory=$false)]
    [int]$RampUp = 5,
    
    [Parameter(Mandatory=$false)]
    [int]$Hold = 0,
    
    [Parameter(Mandatory=$false)]
    [string]$TestPlan = "Designer.jmx",

    [Parameter(Mandatory=$false)]
    [switch]$OpenReport
)

# Set paths
$JMeterPath = "C:\Program Files\JMeter\bin\jmeter.bat"
$TestPlanPath = Join-Path "test-plans" $TestPlan
$ResultsPath = "results"
$ReportsPath = "reports"

# Create output filenames
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$resultFile = Join-Path $ResultsPath "results_${Users}users_${timestamp}.jtl"
$reportDir = Join-Path $ReportsPath "report_${Users}users_${timestamp}"

Write-Host "Starting JMeter Performance Test" -ForegroundColor Green
Write-Host "  Users: $Users" -ForegroundColor Cyan
Write-Host "  Ramp-up: $RampUp seconds" -ForegroundColor Cyan
Write-Host "  Hold: $Hold seconds" -ForegroundColor Cyan
Write-Host "  Test Plan: $TestPlan" -ForegroundColor Cyan

# Check if test plan exists
if (!(Test-Path $TestPlanPath)) {
    Write-Host "Error: Test plan not found: $TestPlanPath" -ForegroundColor Red
    Write-Host "Available test plans:" -ForegroundColor Yellow
    Get-ChildItem "test-plans\*.jmx" | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Yellow }
    exit 1
}

# Create directories if they don't exist
New-Item -ItemType Directory -Force -Path $ResultsPath | Out-Null
New-Item -ItemType Directory -Force -Path $ReportsPath | Out-Null

# Remove existing report directory if it exists
if (Test-Path $reportDir) {
    Remove-Item -Recurse -Force $reportDir
}

# Build JMeter command
$jmeterArgs = @(
    "-n",
    "-t", $TestPlanPath,
    "-Jusers=$Users",
    "-Jramp=$RampUp", 
    "-Jhold=$Hold",
    "-l", $resultFile,
    "-e",
    "-o", $reportDir
)

Write-Host "🧪 Executing JMeter test..." -ForegroundColor Yellow
Write-Host "Command: $JMeterPath $($jmeterArgs -join ' ')" -ForegroundColor Gray

# Run JMeter
try {
    & $JMeterPath @jmeterArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Test completed successfully!" -ForegroundColor Green
        Write-Host "Results saved to: $resultFile" -ForegroundColor Cyan
        Write-Host "Report generated at: $reportDir\index.html" -ForegroundColor Cyan
        
        if ($OpenReport) {
            $reportFile = Join-Path $reportDir "index.html"
            if (Test-Path $reportFile) {
                Write-Host "🌐 Opening report in browser..." -ForegroundColor Yellow
                Start-Process $reportFile
            }
        }
    } else {
        Write-Host "JMeter test failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    }
} catch {
    Write-Host "Error running JMeter: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTo view the report manually, open:" -ForegroundColor Yellow
Write-Host "   $reportDir\index.html" -ForegroundColor White