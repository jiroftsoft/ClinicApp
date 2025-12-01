# PowerShell Script for Creating Log Folder and Setting Permissions
# Date: 1402-06-29
# Request Code: SSP1126(WEB)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setting up Log folder for SSP1126SignalRWindowsService" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Log Path (according to Service Config)
$logPath = "C:\Log"

Write-Host "Log Path: $logPath" -ForegroundColor Yellow
Write-Host ""

# Check if folder exists
if (Test-Path $logPath) {
    Write-Host "[OK] Log folder already exists: $logPath" -ForegroundColor Green
} else {
    Write-Host "[*] Creating Log folder..." -ForegroundColor Yellow
    try {
        New-Item -ItemType Directory -Path $logPath -Force | Out-Null
        Write-Host "[OK] Log folder created successfully: $logPath" -ForegroundColor Green
    } catch {
        Write-Host "[ERROR] Failed to create Log folder: $_" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "[*] Setting permissions..." -ForegroundColor Yellow

# Set permissions for SYSTEM (Service Account)
try {
    $acl = Get-Acl $logPath
    $systemAccess = New-Object System.Security.AccessControl.FileSystemAccessRule(
        "NT AUTHORITY\SYSTEM",
        "FullControl",
        "ContainerInherit,ObjectInherit",
        "None",
        "Allow"
    )
    $acl.SetAccessRule($systemAccess)
    Set-Acl -Path $logPath -AclObject $acl
    Write-Host "[OK] SYSTEM permissions set" -ForegroundColor Green
} catch {
    Write-Host "[WARNING] Failed to set SYSTEM permissions: $_" -ForegroundColor Yellow
}

# Set permissions for Users (for reading logs)
try {
    $acl = Get-Acl $logPath
    $usersAccess = New-Object System.Security.AccessControl.FileSystemAccessRule(
        "Users",
        "ReadAndExecute",
        "ContainerInherit,ObjectInherit",
        "None",
        "Allow"
    )
    $acl.SetAccessRule($usersAccess)
    Set-Acl -Path $logPath -AclObject $acl
    Write-Host "[OK] Users permissions set" -ForegroundColor Green
} catch {
    Write-Host "[WARNING] Failed to set Users permissions: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "[OK] Setup completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Important Notes:" -ForegroundColor Yellow
Write-Host "1. Make sure Service Config (SSP1126SignalRWindowsService.exe.config) is updated" -ForegroundColor White
Write-Host "2. LogPath should be C:\Log\" -ForegroundColor White
Write-Host "3. Restart the Service after changing Config" -ForegroundColor White
Write-Host ""
Write-Host "Service Name: SSP1126Service1" -ForegroundColor Cyan
Write-Host "To restart: Restart-Service -Name 'SSP1126Service1'" -ForegroundColor Cyan
Write-Host ""
