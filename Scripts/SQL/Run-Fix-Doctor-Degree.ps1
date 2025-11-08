# ============================================
# PowerShell Script: اجرای Fix Doctor.Degree
# ============================================

Write-Host "🏥 ClinicApp - Fix Doctor.Degree Column" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# مسیر SQL Script
$scriptPath = Join-Path $PSScriptRoot "Fix-Doctor-Degree-Column.sql"

# بررسی وجود فایل
if (-not (Test-Path $scriptPath)) {
    Write-Host "❌ Error: SQL Script not found at: $scriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "📁 SQL Script: $scriptPath" -ForegroundColor Yellow
Write-Host ""

# اطلاعات اتصال
$server = "."
$database = "ClinicDb"

Write-Host "🔌 Connecting to:" -ForegroundColor Green
Write-Host "   Server: $server" -ForegroundColor White
Write-Host "   Database: $database" -ForegroundColor White
Write-Host "   Authentication: Windows Authentication" -ForegroundColor White
Write-Host ""

# تایید کاربر
$confirmation = Read-Host "⚠️  این Script جدول Doctors را تغییر می‌دهد. ادامه می‌دهید؟ (Y/N)"
if ($confirmation -ne 'Y' -and $confirmation -ne 'y') {
    Write-Host "❌ لغو شد توسط کاربر" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "⏳ در حال اجرای Script..." -ForegroundColor Yellow
Write-Host ""

try {
    # اجرای SQL Script با sqlcmd
    $output = sqlcmd -S $server -d $database -E -i $scriptPath -b 2>&1
    
    # نمایش خروجی
    $output | ForEach-Object {
        if ($_ -match "✅") {
            Write-Host $_ -ForegroundColor Green
        }
        elseif ($_ -match "⚠️|Warning") {
            Write-Host $_ -ForegroundColor Yellow
        }
        elseif ($_ -match "❌|Error") {
            Write-Host $_ -ForegroundColor Red
        }
        else {
            Write-Host $_
        }
    }
    
    # بررسی exit code
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "==========================================" -ForegroundColor Cyan
        Write-Host "✅ Script اجرا شد با موفقیت!" -ForegroundColor Green
        Write-Host "==========================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "📝 مراحل بعدی:" -ForegroundColor Yellow
        Write-Host "   1. Application را Restart کنید" -ForegroundColor White
        Write-Host "   2. به Admin Panel > Doctors بروید" -ForegroundColor White
        Write-Host "   3. چک کنید که لیست بدون خطا load می‌شود" -ForegroundColor White
        Write-Host ""
    }
    else {
        Write-Host ""
        Write-Host "❌ خطا در اجرای Script. Exit Code: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host ""
    Write-Host "❌ Exception occurred:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 راه‌حل‌های احتمالی:" -ForegroundColor Yellow
    Write-Host "   1. مطمئن شوید SQL Server در حال اجرا است" -ForegroundColor White
    Write-Host "   2. مطمئن شوید دسترسی Windows Authentication دارید" -ForegroundColor White
    Write-Host "   3. مطمئن شوید Database 'ClinicDb' وجود دارد" -ForegroundColor White
    Write-Host "   4. sqlcmd را نصب کنید (SQL Server Command Line Tools)" -ForegroundColor White
    Write-Host ""
    exit 1
}

