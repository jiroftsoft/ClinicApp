# راهنمای حل مشکل: Service نمی‌تواند Start شود

**تاریخ:** 1404/09/12  
**وضعیت:** 🔧 **نیاز به دسترسی Admin**

---

## 🔍 مشکل شناسایی شده

### خطا:
```
Service 'SSP1126WindowsService (SSP1126Service1)' cannot be started due to the following error: 
Cannot open SSP1126Service1 service on computer '.'.
```

### علت احتمالی:
1. **نیاز به دسترسی Administrator** (احتمال قوی)
2. Service در حال استفاده است
3. Service crash کرده و نیاز به بررسی دارد
4. Service Config مشکل دارد

---

## ✅ راه‌حل‌های پیشنهادی

### 1. Start Service با دسترسی Admin (اولویت اول)

**⚠️ نیاز به دسترسی Administrator:**

```powershell
# 1. باز کردن PowerShell به عنوان Administrator:
#    - راست کلیک روی PowerShell
#    - انتخاب "Run as Administrator"

# 2. اجرای دستورات زیر:

# بررسی وضعیت Service
Get-Service -Name "SSP1126Service1" | Select-Object Name, Status, StartType

# Start Service
Start-Service -Name "SSP1126Service1"

# بررسی وضعیت بعد از Start
Get-Service -Name "SSP1126Service1"

# بررسی Port 8080
Start-Sleep -Seconds 5
netstat -ano | findstr :8080 | findstr LISTENING

# تست اتصال
Test-NetConnection -ComputerName localhost -Port 8080 -InformationLevel Quiet
```

### 2. بررسی Event Viewer برای خطاهای Service

```powershell
# بررسی Event Log برای خطاهای Service
Get-EventLog -LogName Application -Source "*SSP1126*" -Newest 10 -ErrorAction SilentlyContinue | 
    Where-Object {$_.EntryType -eq "Error" -or $_.EntryType -eq "Warning"} | 
    Format-List TimeGenerated, EntryType, Message

# یا بررسی تمام Event های مرتبط
Get-EventLog -LogName System -Newest 50 | 
    Where-Object {$_.Source -like "*SSP1126*" -or $_.Message -like "*SSP1126*"} | 
    Select-Object TimeGenerated, EntryType, Source, Message | 
    Format-List
```

### 3. بررسی Service Dependencies

```powershell
# بررسی Dependencies Service
$service = Get-Service -Name "SSP1126Service1"
$service.ServicesDependedOn | Select-Object Name, Status

# بررسی Services که به این Service وابسته هستند
$service.DependentServices | Select-Object Name, Status
```

### 4. بررسی Service Config

**فایل:** `Infrastructure/SSP1126(WEB)/ServiceInstaller_1402-06-29/Install/SSP1126SignalRWindowsService.exe.config`

**بررسی:**
- `HostUrl` = `192.168.1.103`
- `LogPath` = `C:\Log\` (باید پوشه وجود داشته باشد)
- سایر تنظیمات

### 5. بررسی دسترسی‌ها

```powershell
# بررسی دسترسی به Log Path
if (Test-Path "C:\Log") {
    Write-Host "Log folder exists: C:\Log"
    icacls "C:\Log"
} else {
    Write-Host "Log folder does not exist: C:\Log"
    Write-Host "Creating folder..."
    New-Item -Path "C:\Log" -ItemType Directory -Force
    icacls "C:\Log" /grant "NT AUTHORITY\SYSTEM:(OI)(CI)F"
    icacls "C:\Log" /grant "Users:(OI)(CI)M"
}
```

### 6. بررسی Service Executable

```powershell
# پیدا کردن مسیر Service Executable
$service = Get-WmiObject Win32_Service | Where-Object {$_.Name -eq "SSP1126Service1"}
$service.PathName

# بررسی اینکه فایل وجود دارد
$exePath = $service.PathName -replace '"', ''
if (Test-Path $exePath) {
    Write-Host "Service executable exists: $exePath"
} else {
    Write-Host "Service executable NOT found: $exePath"
}
```

---

## 🔧 مراحل عیب‌یابی سیستماتیک

### گام 1: بررسی دسترسی Admin
```powershell
# بررسی اینکه PowerShell با دسترسی Admin اجرا شده است
([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
```

**اگر `False` بود:**
- PowerShell را ببندید
- راست کلیک روی PowerShell
- انتخاب "Run as Administrator"
- دوباره دستورات را اجرا کنید

### گام 2: Start Service
```powershell
Start-Service -Name "SSP1126Service1"
```

### گام 3: بررسی وضعیت
```powershell
Get-Service -Name "SSP1126Service1" | Select-Object Name, Status
```

### گام 4: بررسی Port
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

### گام 5: بررسی Log ها
```powershell
if (Test-Path "C:\Log") {
    Get-ChildItem "C:\Log" -Filter "*.log" -ErrorAction SilentlyContinue | 
        Sort-Object LastWriteTime -Descending | 
        Select-Object -First 1 | 
        ForEach-Object { 
            Write-Host "=== $($_.Name) ==="; 
            Get-Content $_.FullName -Tail 50 
        }
}
```

---

## 📋 چک‌لیست

- [ ] PowerShell با دسترسی Admin اجرا شده است
- [ ] Service Start شده است (Status: Running)
- [ ] Port 8080 باز است (netstat خروجی دارد)
- [ ] تست اتصال موفق است (Test-NetConnection = True)
- [ ] Log های Service خطا ندارند
- [ ] Service Config صحیح است

---

## ⚠️ نکات مهم

1. **دسترسی Admin:** Start Service نیاز به دسترسی Administrator دارد
2. **Log Path:** Log های Service در `C:\Log\` ذخیره می‌شوند (باید پوشه وجود داشته باشد)
3. **Port:** Service باید روی Port 8080 listen کند
4. **Event Viewer:** اگر Service Start نمی‌شود، Event Viewer را بررسی کنید

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
**وضعیت:** 🔧 نیاز به دسترسی Admin برای Start Service

