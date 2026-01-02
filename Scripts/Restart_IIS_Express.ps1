# Restart IIS Express
Write-Host "Stopping IIS Express..." -ForegroundColor Yellow

$iisProcesses = Get-Process -Name "iisexpress" -ErrorAction SilentlyContinue

if ($iisProcesses) {
    Write-Host "Found IIS Express processes: $($iisProcesses.Count)" -ForegroundColor Green
    
    foreach ($process in $iisProcesses) {
        Write-Host "Stopping IIS Express (PID: $($process.Id))..." -ForegroundColor Cyan
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
    
    Start-Sleep -Seconds 2
    Write-Host "IIS Express stopped!" -ForegroundColor Green
} else {
    Write-Host "IIS Express is not running." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Now run the project in Visual Studio (F5)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Then in browser:" -ForegroundColor Cyan
Write-Host "1. Ctrl + Shift + Delete -> Clear Cache & Cookies" -ForegroundColor White
Write-Host "2. http://localhost:3560/Account/Logoff" -ForegroundColor White
Write-Host "3. Login with code: 5369873054" -ForegroundColor White
Write-Host "4. http://localhost:3560/Patient/Dashboard" -ForegroundColor White
Write-Host ""
