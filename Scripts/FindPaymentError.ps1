# Script to find payment error by CorrelationId
# Usage: .\Scripts\FindPaymentError.ps1 -CorrelationId "92c168d6-7a73-4f2e-bf84-1f0fc9e39822"

param(
    [Parameter(Mandatory=$true)]
    [string]$CorrelationId,
    
    [string]$LogPath = "App_Data\Logs"
)

Write-Host "Searching for payment error with CorrelationId: $CorrelationId" -ForegroundColor Cyan
Write-Host "Log path: $LogPath" -ForegroundColor Yellow
Write-Host ""

# Search in all log files
$logFiles = Get-ChildItem -Path $LogPath -Filter "*.log" -Recurse -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending

if ($logFiles.Count -eq 0) {
    Write-Host "[ERROR] No log files found!" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] Number of log files: $($logFiles.Count)" -ForegroundColor Green
Write-Host ""

# Search in each file
$found = $false
foreach ($file in $logFiles) {
    Write-Host "Checking file: $($file.Name)..." -ForegroundColor Gray
    
    $matches = Select-String -Path $file.FullName -Pattern $CorrelationId -Context 10, 10 -ErrorAction SilentlyContinue
    
    if ($matches) {
        $found = $true
        Write-Host ""
        Write-Host "[OK] Error found in file: $($file.Name)" -ForegroundColor Green
        Write-Host "Full path: $($file.FullName)" -ForegroundColor Yellow
        Write-Host ""
        Write-Host ("=" * 80) -ForegroundColor Cyan
        Write-Host "Related logs:" -ForegroundColor Cyan
        Write-Host ("=" * 80) -ForegroundColor Cyan
        Write-Host ""
        
        foreach ($match in $matches) {
            Write-Host "Line $($match.LineNumber):" -ForegroundColor Yellow
            Write-Host $match.Line -ForegroundColor White
            Write-Host ""
            
            # Show Context (before and after)
            if ($match.Context.PreContext) {
                Write-Host "Before:" -ForegroundColor Gray
                foreach ($line in $match.Context.PreContext) {
                    Write-Host "  $line" -ForegroundColor DarkGray
                }
                Write-Host ""
            }
            
            if ($match.Context.PostContext) {
                Write-Host "After:" -ForegroundColor Gray
                foreach ($line in $match.Context.PostContext) {
                    Write-Host "  $line" -ForegroundColor DarkGray
                }
                Write-Host ""
            }
            
            Write-Host ("-" * 80) -ForegroundColor DarkGray
            Write-Host ""
        }
    }
}

if (-not $found) {
    Write-Host "[ERROR] No log found with CorrelationId '$CorrelationId'!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Suggestions:" -ForegroundColor Yellow
    Write-Host "  1. Check that CorrelationId is correct"
    Write-Host "  2. Check that Application has been restarted"
    Write-Host "  3. Check that logging is enabled"
    exit 1
}

Write-Host ""
Write-Host ("=" * 80) -ForegroundColor Cyan
Write-Host "[OK] Search completed!" -ForegroundColor Green
Write-Host ("=" * 80) -ForegroundColor Cyan
