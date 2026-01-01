# Check SendLoginOtp Flow Logs
$logFolder = "App_Data\Logs"

Write-Host "=== Checking SendLoginOtp Flow Logs ===" -ForegroundColor Cyan
Write-Host ""

if (Test-Path $logFolder) {
    $latestLog = Get-ChildItem $logFolder -Filter "*.txt" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    if ($latestLog) {
        Write-Host "Log File: $($latestLog.Name)" -ForegroundColor Green
        Write-Host "Last Modified: $($latestLog.LastWriteTime)" -ForegroundColor Green
        Write-Host ""
        
        # Get recent logs
        $content = Get-Content $latestLog.FullName -Tail 200
        
        # Filter for SendLoginOtp flow
        Write-Host "=== SendLoginOtp Flow (Step by Step) ===" -ForegroundColor Yellow
        Write-Host ""
        
        $sendLogs = $content | Where-Object { 
            $_ -match "Controller\.SendLoginOtp|SendLoginOtp\]|SetState\]" 
        }
        
        if ($sendLogs) {
            $sendLogs | ForEach-Object {
                if ($_ -match "SUCCESS") {
                    Write-Host $_ -ForegroundColor Green
                } elseif ($_ -match "EXCEPTION|FAILED") {
                    Write-Host $_ -ForegroundColor Red
                } elseif ($_ -match "START") {
                    Write-Host $_ -ForegroundColor Cyan
                } elseif ($_ -match "Step \d+") {
                    Write-Host $_ -ForegroundColor Yellow
                } else {
                    Write-Host $_ -ForegroundColor White
                }
            }
        } else {
            Write-Host "NO SendLoginOtp logs found!" -ForegroundColor Red
            Write-Host "Please test the login flow:" -ForegroundColor Yellow
            Write-Host "  1. Go to http://localhost:3560/Account/Login" -ForegroundColor Yellow
            Write-Host "  2. Enter National Code: 5369873054" -ForegroundColor Yellow
            Write-Host "  3. Click 'Send OTP' button" -ForegroundColor Yellow
        }
        
        Write-Host ""
        Write-Host "=== Recent Errors (if any) ===" -ForegroundColor Red
        $errors = $content | Where-Object { $_ -match "\[ERR\]" } | Select-Object -Last 10
        if ($errors) {
            $errors | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        } else {
            Write-Host "No recent errors found" -ForegroundColor Green
        }
    } else {
        Write-Host "No log files found" -ForegroundColor Red
    }
} else {
    Write-Host "Log folder not found: $logFolder" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Instructions ===" -ForegroundColor Cyan
Write-Host "If you don't see SendLoginOtp logs:" -ForegroundColor White
Write-Host "1. Make sure you're testing from the LOGIN PAGE (not VerifyLoginOtp directly)" -ForegroundColor White
Write-Host "2. Enter National Code and click 'Send OTP'" -ForegroundColor White
Write-Host "3. Run this script again to see the detailed logs" -ForegroundColor White
