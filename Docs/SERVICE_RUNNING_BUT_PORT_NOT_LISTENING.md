# راهنمای حل مشکل: Service در حال اجرا است اما Port 8080 باز نیست

**تاریخ:** 1404/09/12  
**وضعیت:** 🔧 **در حال بررسی**

---

## 🔍 مشکل شناسایی شده

### وضعیت فعلی:
- ✅ **Service در حال اجرا است:** `SSP1126Service1` (Status: Running)
- ✅ **Process در حال اجرا است:** `SSP1126SignalRWindowsService` (PID: 22852)
- ❌ **Port 8080 باز نیست:** `netstat -ano | findstr :8080` خروجی ندارد
- ❌ **Process روی هیچ Portی listen نمی‌کند**
- ❌ **Log folder خالی است:** `C:\Log\` فایل Log ندارد

### علت احتمالی:
1. **Service crash کرده است** (در حال اجرا اما listen نمی‌کند)
2. **Service به درستی initialize نشده است**
3. **Service Config مشکل دارد** (Config file در مسیر Service executable نیست)
4. **Service نیاز به بررسی Config file دارد**

---

## ✅ راه‌حل‌های پیشنهادی

### 1. بررسی Config File در مسیر Service Executable

**مسیر Service:**
```
C:\Users\Developer\Desktop\tools\SSP1126(WEB) 1402-06-29\ServiceInstaller_1402-06-29\Install\SSP1126SignalRWindowsService.exe
```

**Config File باید در همان مسیر باشد:**
```
C:\Users\Developer\Desktop\tools\SSP1126(WEB) 1402-06-29\ServiceInstaller_1402-06-29\Install\SSP1126SignalRWindowsService.exe.config
```

**بررسی:**
```powershell
$servicePath = "C:\Users\Developer\Desktop\tools\SSP1126(WEB) 1402-06-29\ServiceInstaller_1402-06-29\Install"
$configPath = Join-Path $servicePath "SSP1126SignalRWindowsService.exe.config"

if (Test-Path $configPath) {
    Write-Host "Config file exists: YES"
    Get-Content $configPath | Select-String -Pattern "HostUrl|LogPath|Port"
} else {
    Write-Host "Config file exists: NO - این مشکل است!"
    Write-Host "باید Config file را از Infrastructure/SSP1126(WEB)/ServiceInstaller_1402-06-29/Install/ کپی کنید"
}
```

### 2. Restart Service با بررسی Config

```powershell
# Stop Service
Stop-Service -Name "SSP1126Service1" -Force

# بررسی Config file
$servicePath = "C:\Users\Developer\Desktop\tools\SSP1126(WEB) 1402-06-29\ServiceInstaller_1402-06-29\Install"
$configPath = Join-Path $servicePath "SSP1126SignalRWindowsService.exe.config"

if (-not (Test-Path $configPath)) {
    Write-Host "⚠️ Config file not found! Copying from project..."
    $sourceConfig = "C:\Users\Developer\source\repos\ClinicApp\Infrastructure\SSP1126(WEB)\ServiceInstaller_1402-06-29\Install\SSP1126SignalRWindowsService.exe.config"
    if (Test-Path $sourceConfig) {
        Copy-Item $sourceConfig $configPath -Force
        Write-Host "✅ Config file copied"
    } else {
        Write-Host "❌ Source config not found: $sourceConfig"
    }
}

# Start Service
Start-Service -Name "SSP1126Service1"

# بررسی وضعیت
Start-Sleep -Seconds 5
Get-Service -Name "SSP1126Service1"
netstat -ano | findstr :8080 | findstr LISTENING
```

### 3. بررسی Event Viewer برای خطاهای Initialize

```powershell
# بررسی Event Log برای خطاهای Initialize
Get-EventLog -LogName Application -Newest 100 | 
    Where-Object {
        ($_.TimeGenerated -gt (Get-Date).AddMinutes(-10)) -and 
        ($_.Source -like "*SSP1126*" -or 
         $_.Message -like "*SSP1126*" -or 
         $_.Message -like "*8080*" -or 
         $_.Message -like "*SignalR*" -or 
         $_.Message -like "*Port*" -or 
         $_.Message -like "*Listen*" -or
         $_.Message -like "*Initialize*" -or
         $_.Message -like "*Exception*" -or
         $_.Message -like "*Error*")
    } | 
    Select-Object TimeGenerated, EntryType, Source, @{Name="Message";Expression={$_.Message.Substring(0, [Math]::Min(300, $_.Message.Length))}} | 
    Format-Table -AutoSize
```

### 4. بررسی Process و Memory

```powershell
$process = Get-Process -Id 22852 -ErrorAction SilentlyContinue
if ($process) {
    Write-Host "Process is running: YES"
    Write-Host "Memory: $([math]::Round($process.WorkingSet64/1MB, 2)) MB"
    Write-Host "CPU Time: $($process.CPU)"
    
    # بررسی اینکه Process crash کرده است
    try {
        $process.Refresh()
        Write-Host "Process is responding: YES"
    } catch {
        Write-Host "Process is responding: NO - Process may have crashed"
    }
} else {
    Write-Host "Process not found (may have crashed)"
}
```

### 5. بررسی Log Path و دسترسی‌ها

```powershell
# بررسی Log Path
$logPath = "C:\Log"
if (Test-Path $logPath) {
    Write-Host "Log folder exists: YES"
    Write-Host "Writable: YES (checked)"
    
    # بررسی دسترسی
    icacls $logPath | Select-String "SSP1126|SYSTEM|Users"
} else {
    Write-Host "Log folder exists: NO"
    Write-Host "Creating folder..."
    New-Item -Path $logPath -ItemType Directory -Force
    icacls $logPath /grant "NT AUTHORITY\SYSTEM:(OI)(CI)F"
    icacls $logPath /grant "Users:(OI)(CI)M"
}
```

---

## 🔧 مراحل عیب‌یابی سیستماتیک

### گام 1: بررسی Config File
```powershell
$servicePath = "C:\Users\Developer\Desktop\tools\SSP1126(WEB) 1402-06-29\ServiceInstaller_1402-06-29\Install"
$configPath = Join-Path $servicePath "SSP1126SignalRWindowsService.exe.config"
Test-Path $configPath
```

**اگر `False` بود:**
- Config file را از پروژه کپی کنید
- یا Config file را در مسیر Service executable ایجاد کنید

### گام 2: Restart Service
```powershell
Stop-Service -Name "SSP1126Service1" -Force
Start-Sleep -Seconds 2
Start-Service -Name "SSP1126Service1"
```

### گام 3: بررسی Port بعد از Restart
```powershell
Start-Sleep -Seconds 5
netstat -ano | findstr :8080 | findstr LISTENING
```

**اگر خروجی داشت:**
- ✅ Service در حال اجرا است و Port 8080 باز است
- می‌توانید تست کنید

**اگر خروجی نداشت:**
- ❌ Service در حال اجرا است اما Port 8080 باز نیست
- Event Viewer و Log ها را بررسی کنید

### گام 4: بررسی Event Viewer
```powershell
Get-EventLog -LogName Application -Newest 100 | 
    Where-Object {
        ($_.TimeGenerated -gt (Get-Date).AddMinutes(-10)) -and 
        ($_.Source -like "*SSP1126*" -or $_.Message -like "*SSP1126*")
    } | 
    Select-Object TimeGenerated, EntryType, Source, Message | 
    Format-List
```

---

## 📋 چک‌لیست

- [ ] Config file در مسیر Service executable وجود دارد
- [ ] Config file شامل `HostUrl` و `LogPath` است
- [ ] Service Start شده است (Status: Running)
- [ ] Port 8080 باز است (netstat خروجی دارد)
- [ ] Log folder وجود دارد و writable است
- [ ] Event Viewer خطای Initialize ندارد

---

## ⚠️ نکات مهم

1. **Config File:** Service executable و Config file باید در همان مسیر باشند
2. **Log Path:** Log folder باید وجود داشته باشد و Service دسترسی نوشتن داشته باشد
3. **HostUrl:** در Config file باید `HostUrl = 192.168.1.103` باشد
4. **Port:** Service به صورت پیش‌فرض روی Port 8080 listen می‌کند

---

## 🔄 اگر مشکل ادامه داشت

1. **Service را Reinstall کنید:**
   - Stop Service
   - Uninstall Service
   - Install Service مجدد
   - Start Service

2. **بررسی Windows Firewall:**
   ```powershell
   Get-NetFirewallRule | Where-Object {$_.DisplayName -like "*8080*" -or $_.DisplayName -like "*SSP1126*"}
   ```

3. **بررسی Antivirus:**
   - ممکن است Antivirus Service را مسدود کند
   - Service را به Exception List اضافه کنید

---

**تاریخ:** 1404/09/12  
**وضعیت:** 🔧 نیاز به بررسی Config file و Event Viewer

