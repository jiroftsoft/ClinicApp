# Script to check payment configuration
# Usage: .\Scripts\CheckPaymentConfig.ps1

Write-Host "Checking payment configuration..." -ForegroundColor Cyan
Write-Host ""

# Check Web.config
$webConfigPath = "Web.config"
if (Test-Path $webConfigPath) {
    Write-Host "[OK] Web.config found" -ForegroundColor Green
    Write-Host ""
    
    [xml]$config = Get-Content $webConfigPath
    
    # Check Payment:BaseUrl
    $paymentBaseUrl = $config.configuration.appSettings.add | Where-Object { $_.key -eq "Payment:BaseUrl" }
    if ($paymentBaseUrl) {
        Write-Host "[OK] Payment:BaseUrl is set:" -ForegroundColor Green
        Write-Host "   Value: $($paymentBaseUrl.value)" -ForegroundColor White
    } else {
        Write-Host "[ERROR] Payment:BaseUrl is NOT set!" -ForegroundColor Red
    }
    Write-Host ""
    
    # Check ZarinpalMerchantId
    $merchantId = $config.configuration.appSettings.add | Where-Object { $_.key -eq "ZarinpalMerchantId" }
    if ($merchantId) {
        Write-Host "[OK] ZarinpalMerchantId is set:" -ForegroundColor Green
        $maskedId = $merchantId.value.Substring(0, [Math]::Min(8, $merchantId.value.Length)) + "..."
        Write-Host "   Value: $maskedId" -ForegroundColor White
    } else {
        Write-Host "[ERROR] ZarinpalMerchantId is NOT set!" -ForegroundColor Red
    }
    Write-Host ""
    
    # Check Zarinpal:IsSandbox
    $isSandbox = $config.configuration.appSettings.add | Where-Object { $_.key -eq "Zarinpal:IsSandbox" }
    if ($isSandbox) {
        Write-Host "[OK] Zarinpal:IsSandbox is set:" -ForegroundColor Green
        Write-Host "   Value: $($isSandbox.value)" -ForegroundColor White
        if ($isSandbox.value -eq "true") {
            Write-Host "   [WARNING] You are in Sandbox mode!" -ForegroundColor Yellow
        }
    } else {
        Write-Host "[ERROR] Zarinpal:IsSandbox is NOT set!" -ForegroundColor Red
    }
    Write-Host ""
    
} else {
    Write-Host "[ERROR] Web.config not found!" -ForegroundColor Red
    exit 1
}

# Check log files
$logPath = "App_Data\Logs"
if (Test-Path $logPath) {
    Write-Host "[OK] Log path found: $logPath" -ForegroundColor Green
    
    $logFiles = Get-ChildItem -Path $logPath -Filter "*.log" -Recurse | Sort-Object LastWriteTime -Descending
    Write-Host "   Number of log files: $($logFiles.Count)" -ForegroundColor White
    
    if ($logFiles.Count -gt 0) {
        Write-Host "   Latest log file: $($logFiles[0].Name)" -ForegroundColor White
        Write-Host "   Last modified: $($logFiles[0].LastWriteTime)" -ForegroundColor White
    }
    Write-Host ""
} else {
    Write-Host "[WARNING] Log path not found: $logPath" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host ("=" * 80) -ForegroundColor Cyan
Write-Host "[OK] Check completed!" -ForegroundColor Green
Write-Host ("=" * 80) -ForegroundColor Cyan
