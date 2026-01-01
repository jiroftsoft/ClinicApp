# Check Authentication Logs
$logFolder = "App_Data\Logs"
if (Test-Path $logFolder) {
    $latestLog = Get-ChildItem $logFolder -Filter "*.txt" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($latestLog) {
        Write-Host "=== LATEST LOG FILE: $($latestLog.Name) ===" -ForegroundColor Green
        Write-Host ""
        Get-Content $latestLog.FullName -Tail 100 | Where-Object { $_ -match "HybridOtpStateStore|SendLoginOtp|CRITICAL|ERROR" }
    } else {
        Write-Host "No log files found" -ForegroundColor Red
    }
} else {
    Write-Host "Log folder not found: $logFolder" -ForegroundColor Red
}

