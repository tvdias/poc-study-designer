# Power Apps Performance Test Runner Script
# Simplified script for running Power Apps performance tests

param(
    [string]$TestType = "all",
    [switch]$Headed,
    [switch]$Debug,
    [switch]$ShowReport
)

Write-Host "=== Power Apps Performance Test Runner ===" -ForegroundColor Green
Write-Host "Test Type: $TestType" -ForegroundColor Yellow

# Change to tests directory if not already there
if (!(Test-Path "package.json")) {
    if (Test-Path "tests/package.json") {
        Set-Location -Path "tests"
    } else {
        Write-Host "Error: Cannot find tests directory or package.json" -ForegroundColor Red
        exit 1
    }
}

# Define test commands based on test type
switch ($TestType.ToLower()) {
    "all" {
        Write-Host "Running all Power Apps performance tests..." -ForegroundColor Cyan
        $command = "npm run test:performance"
    }
    "product" {
        Write-Host "Running product entity performance test..." -ForegroundColor Cyan
        $command = "npm run test:performance"
    }
    default {
        Write-Host "Invalid test type. Available options: all, product" -ForegroundColor Red
        Write-Host "Usage: .\run-performance-tests.ps1 -TestType all|product [-Headed] [-Debug] [-ShowReport]" -ForegroundColor Yellow
        exit 1
    }
}

# Add headed mode if specified
if ($Headed) {
    $command = $command -replace "test:performance", "test:performance:headed"
    Write-Host "Headed mode enabled (visible browser)" -ForegroundColor Yellow
}

# Add debug flag if specified
if ($Debug) {
    $command = $command -replace "test:performance", "test:performance:debug"
    Write-Host "Debug mode enabled (with breakpoints)" -ForegroundColor Yellow
}

# Execute the test command
Write-Host "Executing: $command" -ForegroundColor Gray
try {
    Invoke-Expression $command
    Write-Host "Power Apps performance tests completed!" -ForegroundColor Green
    
    # Show report if requested
    if ($ShowReport) {
        Write-Host "Opening test report..." -ForegroundColor Cyan
        npm run report
    } else {
        Write-Host "To view the test report, run: npm run report" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "Error running performance tests: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Power Apps Performance Test Summary ===" -ForegroundColor Green
Write-Host "Test execution completed" -ForegroundColor Green
Write-Host "Page Load Time and Time to Interactive measured" -ForegroundColor Yellow
Write-Host "Performance score calculated" -ForegroundColor Yellow
Write-Host "Reports available at: test-results/html-report/" -ForegroundColor Yellow

# Display simplified usage examples
Write-Host "`n=== Usage Examples ===" -ForegroundColor Cyan
Write-Host ".\run-performance-tests.ps1                                  # Run performance test" -ForegroundColor Gray
Write-Host ".\run-performance-tests.ps1 -Headed                         # Run with visible browser" -ForegroundColor Gray
Write-Host ".\run-performance-tests.ps1 -Debug                          # Run in debug mode" -ForegroundColor Gray
Write-Host ".\run-performance-tests.ps1 -ShowReport                     # Run and show report" -ForegroundColor Gray