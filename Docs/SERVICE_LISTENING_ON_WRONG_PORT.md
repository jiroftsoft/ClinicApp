# راهنمای حل مشکل: Service روی Port اشتباه listen می‌کند

**تاریخ:** 1404/09/12  
**وضعیت:** 🔧 **مشکل شناسایی شد**

---

## 🔍 مشکل شناسایی شده

### وضعیت:
- ✅ **Service در حال اجرا است:** `SSP1126Service1` (Status: Running)
- ✅ **Service listen می‌کند:** اما روی **Port 5000** (نه Port 8080)
- ❌ **Port 8080 باز نیست:** `netstat -ano | findstr :8080` خروجی ندارد
- ✅ **Port 5000 باز است:** Event Viewer نشان می‌دهد `http://localhost:5000/`

### علت:
Service به صورت پیش‌فرض روی Port 5000 listen می‌کند، نه Port 8080. این احتمالاً به این دلیل است که:
1. Service به صورت hard-coded روی Port 5000 تنظیم شده است
2. یا Config file شامل تنظیمات Port نیست
3. یا Service از تنظیمات پیش‌فرض استفاده می‌کند

---

## ✅ راه‌حل‌های پیشنهادی

### 1. بررسی Config File برای تنظیمات Port

**بررسی:**
```powershell
$configPath = "C:\Users\Developer\Desktop\tools\SSP1126(WEB) 1402-06-29\ServiceInstaller_1402-06-29\Install\SSP1126SignalRWindowsService.exe.config"
Get-Content $configPath | Select-String -Pattern "Port|8080|5000"
```

**اگر Config file شامل تنظیمات Port نیست:**
- باید تنظیمات Port را به Config file اضافه کنید
- یا Service را به‌روزرسانی کنید

### 2. تغییر URL در Web.config

**اگر نمی‌توانید Port Service را تغییر دهید:**
- می‌توانید URL در `Web.config` را به Port 5000 تغییر دهید:

```xml
<!-- در Web.config -->
<add key="SamanKishSignalRUrl" value="http://localhost:5000/signalr" />
```

**⚠️ نکته:** این راه‌حل موقت است. بهتر است Service را به Port 8080 تغییر دهید.

### 3. بررسی Source Code Service

**اگر Source Code Service در دسترس است:**
- باید بررسی کنید که Port کجا تنظیم می‌شود
- احتمالاً در کد Service hard-coded است: `http://localhost:5000/`

### 4. تست اتصال به Port 5000

**بررسی:**
```powershell
# تست اتصال به Port 5000
Test-NetConnection -ComputerName localhost -Port 5000 -InformationLevel Quiet

# بررسی SignalR Hubs
Invoke-WebRequest -Uri "http://localhost:5000/signalr/hubs" -UseBasicParsing
```

---

## 🔧 مراحل عیب‌یابی

### گام 1: بررسی Port فعلی
```powershell
netstat -ano | findstr :5000 | findstr LISTENING
netstat -ano | findstr :8080 | findstr LISTENING
```

### گام 2: تست اتصال
```powershell
Test-NetConnection -ComputerName localhost -Port 5000 -InformationLevel Quiet
Test-NetConnection -ComputerName localhost -Port 8080 -InformationLevel Quiet
```

### گام 3: تست SignalR Hubs
```powershell
# تست Port 5000
Invoke-WebRequest -Uri "http://localhost:5000/signalr/hubs" -UseBasicParsing

# تست Port 8080
Invoke-WebRequest -Uri "http://localhost:8080/signalr/hubs" -UseBasicParsing
```

### گام 4: تغییر URL در Web.config (راه‌حل موقت)

**اگر Port 5000 کار می‌کند:**
```xml
<!-- در Web.config -->
<add key="SamanKishSignalRUrl" value="http://localhost:5000/signalr" />
```

**بعد از تغییر:**
- Application Pool را Restart کنید
- صفحه را Refresh کنید

---

## 📋 چک‌لیست

- [x] Service در حال اجرا است (Status: Running)
- [x] Service روی Port 5000 listen می‌کند
- [ ] Port 5000 تست شده است
- [ ] SignalR Hubs روی Port 5000 در دسترس است
- [ ] URL در Web.config به Port 5000 تغییر یافته است

---

## ⚠️ نکات مهم

1. **Port 5000:** Service به صورت پیش‌فرض روی Port 5000 listen می‌کند
2. **Port 8080:** این Port در Config file یا کد Service تنظیم نشده است
3. **راه‌حل موقت:** تغییر URL در Web.config به Port 5000
4. **راه‌حل دائمی:** تغییر Port Service به 8080 (نیاز به Source Code)

---

## 🔄 راه‌حل‌های پیشنهادی

### راه‌حل 1: تغییر URL در Web.config (سریع)

```xml
<!-- در Web.config -->
<add key="SamanKishSignalRUrl" value="http://localhost:5000/signalr" />
```

### راه‌حل 2: تغییر Port Service (نیاز به Source Code)

اگر Source Code Service در دسترس است:
- باید Port را از 5000 به 8080 تغییر دهید
- Service را Rebuild و Reinstall کنید

---

**تاریخ:** 1404/09/12  
**وضعیت:** 🔧 Service روی Port 5000 listen می‌کند - نیاز به تغییر URL یا Port

