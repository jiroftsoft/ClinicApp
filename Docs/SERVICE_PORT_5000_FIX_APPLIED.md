# گزارش اصلاح Port Service: تغییر از 8080 به 5000

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ **اصلاح شد**

---

## 🔍 مشکل شناسایی شده

### وضعیت:
- ✅ **Service در حال اجرا است:** `SSP1126Service1` (Status: Running)
- ✅ **Service listen می‌کند:** اما روی **Port 5000** (نه Port 8080)
- ❌ **Port 8080 باز نیست:** `netstat -ano | findstr :8080` خروجی ندارد
- ✅ **Port 5000 باز است:** `netstat -ano | findstr :5000` خروجی دارد

### علت:
Service به صورت پیش‌فرض روی Port 5000 listen می‌کند، نه Port 8080. این احتمالاً به این دلیل است که:
- Service به صورت hard-coded روی Port 5000 تنظیم شده است
- Config file شامل تنظیمات Port نیست

---

## ✅ راه‌حل اعمال شده

### تغییر URL در Web.config

**قبل:**
```xml
<add key="SamanKishSignalRUrl" value="http://localhost:8080/signalr" />
```

**بعد:**
```xml
<add key="SamanKishSignalRUrl" value="http://localhost:5000/signalr" />
```

---

## 🔧 مراحل بعدی

### 1. Restart Application Pool

**⚠️ نیاز به دسترسی Administrator:**

```powershell
# Restart IIS Application Pool
Import-Module WebAdministration
Restart-WebAppPool -Name "ClinicApp"
```

**یا از IIS Manager:**
1. باز کردن IIS Manager
2. انتخاب Application Pool
3. راست کلیک → Recycle

### 2. تست اتصال

**در Browser:**
```
http://localhost:5000/signalr/hubs
```

**در PowerShell:**
```powershell
Invoke-WebRequest -Uri "http://localhost:5000/signalr/hubs" -UseBasicParsing
```

### 3. تست در Application

1. باز کردن صفحه `/PosTest`
2. بررسی اینکه خطای `ERR_CONNECTION_REFUSED` برطرف شده است
3. تست اتصال به SignalR

---

## 📋 چک‌لیست

- [x] Service در حال اجرا است (Status: Running)
- [x] Service روی Port 5000 listen می‌کند
- [x] Port 5000 تست شده است (Test-NetConnection = True)
- [x] URL در Web.config به Port 5000 تغییر یافته است
- [ ] Application Pool Restart شده است
- [ ] SignalR Hubs روی Port 5000 در دسترس است
- [ ] تست در Application موفق است

---

## ⚠️ نکات مهم

1. **Port 5000:** Service به صورت پیش‌فرض روی Port 5000 listen می‌کند
2. **Port 8080:** این Port در Config file یا کد Service تنظیم نشده است
3. **راه‌حل:** تغییر URL در Web.config به Port 5000
4. **Restart:** بعد از تغییر Web.config، Application Pool باید Restart شود

---

## 🔄 اگر مشکل ادامه داشت

### بررسی SignalR Hubs:
```powershell
Invoke-WebRequest -Uri "http://localhost:5000/signalr/hubs" -UseBasicParsing
```

### بررسی Port:
```powershell
netstat -ano | findstr :5000 | findstr LISTENING
Test-NetConnection -ComputerName localhost -Port 5000 -InformationLevel Quiet
```

### بررسی Service:
```powershell
Get-Service -Name "SSP1126Service1" | Select-Object Name, Status
```

---

**تاریخ:** 1404/09/12  
**وضعیت:** ✅ URL در Web.config به Port 5000 تغییر یافت - نیاز به Restart Application Pool

