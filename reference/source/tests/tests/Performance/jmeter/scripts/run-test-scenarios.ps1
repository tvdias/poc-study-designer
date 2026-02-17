# Common JMeter Test Scenarios for Study Designer
# Run these commands from the jmeter folder

Write-Host "Study Designer JMeter Test Scenarios" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

Write-Host "`n Available test scenarios:" -ForegroundColor Yellow
Write-Host "1. Single user test (validation)" -ForegroundColor Cyan
Write-Host "2. Light load test (50 users)" -ForegroundColor Cyan  
Write-Host "3. Medium load test (100 users)" -ForegroundColor Cyan
Write-Host "4. Heavy load test (250 users)" -ForegroundColor Cyan
Write-Host "5. Custom test" -ForegroundColor Cyan

$choice = Read-Host "`nSelect scenario (1-5)"

switch ($choice) {
    "1" {
        Write-Host "`n Running single user validation test..." -ForegroundColor Yellow
        .\scripts\run-jmeter-test.ps1 -Users 1 -RampUp 1 -Hold 0 -OpenReport
    }
    "2" {
        Write-Host "`n Running light load test (50 users, hold for 2 minutes)..." -ForegroundColor Yellow
        .\scripts\run-jmeter-test.ps1 -Users 50 -RampUp 5 -Hold 120 -OpenReport
    }
    "3" {
        Write-Host "`n Running medium load test (100 users, hold for 5 minutes)..." -ForegroundColor Yellow
        .\scripts\run-jmeter-test.ps1 -Users 100 -RampUp 5 -Hold 300 -OpenReport
    }
    "4" {
        Write-Host "`n Running heavy load test (250 users, hold for 10 minutes)..." -ForegroundColor Yellow
        .\scripts\run-jmeter-test.ps1 -Users 250 -RampUp 10 -Hold 600 -OpenReport
    }
    "5" {
        $users = Read-Host "Enter number of users"
        $ramp = Read-Host "Enter ramp-up time (seconds)"
        $hold = Read-Host "Enter hold time (seconds)"
        Write-Host "`n Running custom test..." -ForegroundColor Yellow
        .\scripts\run-jmeter-test.ps1 -Users $users -RampUp $ramp -Hold $hold -OpenReport
    }
    default {
        Write-Host "Invalid selection" -ForegroundColor Red
    }
}