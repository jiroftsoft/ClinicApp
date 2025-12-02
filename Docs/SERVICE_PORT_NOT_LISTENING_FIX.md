# راهنمای حل مشکل: Service در حال اجرا است اما Port 8080 باز نیست

**تاریخ:** 1404/09/11  
**وضعیت:** 🔧 **نیاز به بررسی**

---

## 🔍 مشکل شناسایی شده

### وضعیت:
- ✅ **Service در حال اجرا است:** `SSP1126Service1` (Status: Running)
- ❌ **Port 8080 باز نیست:** `netstat -ano | findstr :8080` خروجی ندارد
- ❌ **اتصال به Port 8080 ناموفق:** `Test-NetConnection localhost:8080` = False

### علت احتمالی:
1. Service crash کرده است (در حال اجرا اما listen نمی‌کند)
2. Service به درستی initialize نشده است
3. Service روی Port دیگری listen می‌کند
4. Service نیاز به restart دارد

---

## ✅ راه‌حل‌های پیشنهادی

### 1. Restart Service با دسترسی Admin

**⚠️ نیاز به دسترسی Administrator:**

```powershell
# باز کردن PowerShell به عنوان Administrator
# سپس اجرای دستورات زیر:

# توقف Service
Stop-Service -Name "SSP1126Service1" -Force

# شروع Service
Start-Service -Name "SSP1126Service1"

# بررسی وضعیت
Get-Service -Name "SSP1126Service1"

# بررسی Port
netstat -ano | findstr :8080
```

### 2. بررسی Log های Service

```powershell
# بررسی Log های Service
Get-ChildItem "C:\Log" -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 5 Name, LastWriteTime

# خواندن آخرین Log
Get-Content "C:\Log\[نام فایل Log]" -Tail 50
```

**جستجو برای:**
- خطاهای Initialize
- خطاهای Port Binding
- خطاهای Network
- Crash یا Exception

### 3. بررسی Event Viewer

```powershell
# بررسی Event Log
Get-EventLog -LogName Application -Source "*SSP1126*" -Newest 10 | Format-List
```

### 4. بررسی Process Service

```powershell
# بررسی Process مربوط به Service
Get-Process | Where-Object {$_.ProcessName -like "*SSP1126*"}
```

### 5. بررسی تنظیمات Service Config

**فایل:** `Infrastructure/SSP1126(WEB)/ServiceInstaller_1402-06-29/Install/SSP1126SignalRWindowsService.exe.config`

**بررسی:**
- `HostUrl` = `192.168.1.103`
- `LogPath` = `C:\Log\`
- سایر تنظیمات

### 6. تست دستی Service

**در Browser:**
```
http://localhost:8080/signalr/hubs
```

**در PowerShell:**
```powershell
Invoke-WebRequest -Uri "http://localhost:8080/signalr/hubs" -UseBasicParsing
```

---

## 🔧 مراحل عیب‌یابی سیستماتیک

### گام 1: بررسی وضعیت Service
```powershell
Get-Service -Name "SSP1126Service1" | Select-Object Name, Status, StartType
```

### گام 2: بررسی Port
```powershell
netstat -ano | findstr :8080
```

**اگر خروجی خالی است:**
- Service listen نمی‌کند
- نیاز به restart دارد

### گام 3: Restart Service
```powershell
# با دسترسی Admin
Restart-Service -Name "SSP1126Service1" -Force
```

### گام 4: بررسی مجدد Port
```powershell
Start-Sleep -Seconds 3
netstat -ano | findstr :8080
```

**اگر هنوز خروجی خالی است:**
- Service crash کرده است
- Log ها را بررسی کنید

### گام 5: بررسی Log ها
```powershell
Get-Content "C:\Log\*.log" -Tail 100 | Select-String -Pattern "error|exception|crash|port|8080" -CaseSensitive:$false
```

---

## 📋 چک‌لیست

- [ ] Service در حال اجرا است (Status: Running)
- [ ] Port 8080 باز است (netstat خروجی دارد)
- [ ] تست اتصال موفق است (Test-NetConnection = True)
- [ ] Log های Service خطا ندارند
- [ ] Service Config صحیح است

---

## ⚠️ نکات مهم

1. **دسترسی Admin:** Restart Service نیاز به دسترسی Administrator دارد
2. **Log Path:** Log های Service در `C:\Log\` ذخیره می‌شوند
3. **Port:** Service باید روی Port 8080 listen کند
4. **URL:** Application باید به `http://localhost:8080/signalr` متصل شود

---

**تاریخ:** 1404/09/11  
**وضعیت:** 🔧 نیاز به بررسی و Restart Service

