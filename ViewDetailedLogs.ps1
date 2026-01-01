# View Detailed Authentication Logs
$logFolder = "App_Data\Logs"

if (Test-Path $logFolder) {
    $latestLog = Get-ChildItem $logFolder -Filter "*.txt" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    if ($latestLog) {
        Write-Host "=== LATEST LOG FILE: $($latestLog.Name) ===" -ForegroundColor Cyan
        Write-Host "=== Last Modified: $($latestLog.LastWriteTime) ===" -ForegroundColor Cyan
        Write-Host ""
        
        # Get last 150 lines and filter for auth-related logs
        $content = Get-Content $latestLog.FullName -Tail 150
        
        Write-Host "=== AUTHENTICATION FLOW LOGS ===" -ForegroundColor Green
        $content | Where-Object { 
            $_ -match "SendLoginOtp|SetState|GetState|Controller\.Send" 
        } | ForEach-Object {
            if ($_ -match "SUCCESS|✅") {
                Write-Host $_ -ForegroundColor Green
            } elseif ($_ -match "EXCEPTION|FAILED|❌|ERROR") {
                Write-Host $_ -ForegroundColor Red
            } elseif ($_ -match "WARNING|⚠️") {
                Write-Host $_ -ForegroundColor Yellow
            } else {
                Write-Host $_ -ForegroundColor White
            }
        }
        
        Write-Host ""
        Write-Host "=== ALL ERRORS (if any) ===" -ForegroundColor Red
        $content | Where-Object { $_ -match "\[ERR\]" } | ForEach-Object {
            Write-Host $_ -ForegroundColor Red
        }
    } else {
        Write-Host "No log files found" -ForegroundColor Red
    }
} else {
    Write-Host "Log folder not found: $logFolder" -ForegroundColor Red
}

