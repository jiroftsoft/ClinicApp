# راهنمای عیب‌یابی اتصال POS (SignalR)
**تاریخ:** 1402-06-29  
**کد درخواست:** SSP1126(WEB)

---

## 🔍 مشکل: تست اتصال می‌چرخد و ارتباط برقرار نمی‌شود

### ✅ مراحل عیب‌یابی

#### 1. بررسی Windows Service

**✅ بررسی وضعیت Service:**
```powershell
# بررسی وضعیت Service
# نام واقعی Service: SSP1126Service1 (نه SSP1126SignalRWindowsService)
Get-Service -Name "SSP1126Service1" | Select-Object Name, Status, StartType

# اگر Service متوقف است، آن را شروع کنید
Start-Service -Name "SSP1126Service1"

# Restart Service
Restart-Service -Name "SSP1126Service1"

# بررسی Log های Service
# مسیر Log: C:\Log\ (طبق تنظیمات Service - به‌روزرسانی شده)
```

**✅ بررسی Port 8080:**
```powershell
# بررسی اینکه Port 8080 در حال استفاده است
netstat -ano | findstr :8080

# یا
Test-NetConnection -ComputerName localhost -Port 8080
```

#### 2. بررسی تنظیمات Web.config

**✅ بررسی URL SignalR:**
```xml
<!-- در Web.config -->
<add key="SamanKishSignalRUrl" value="http://localhost:8080/signalr" />
```

**⚠️ نکات مهم:**
- **HostUrl در SSP1126SignalRWindowsService.exe.config = `192.168.1.103`**
- بنابراین URL باید `http://192.168.1.103:8080/signalr` باشد (نه localhost)
- اگر Windows Service روی همان سرور اجرا می‌شود و HostUrl = localhost است، از `http://localhost:8080/signalr` استفاده کنید
- Port باید 8080 باشد (طبق تنظیمات Service)

#### 3. بررسی تنظیمات ترمینال

**✅ در دیتابیس:**
```sql
-- بررسی Protocol ترمینال
SELECT PosTerminalId, TerminalId, IpAddress, Port, Protocol, Provider, IsActive
FROM PosTerminal
WHERE PosTerminalId = [TerminalId شما]

-- Protocol باید = 4 (SignalR) باشد
UPDATE PosTerminal 
SET Protocol = 4  -- SignalR
WHERE PosTerminalId = [TerminalId شما]
```

**✅ بررسی تنظیمات:**
- Protocol: `SignalR` (4)
- IP Address: آدرس دستگاه POS (مثلاً `192.168.1.104`)
- TerminalId: شماره ترمینال
- MerchantId: شماره پذیرنده
- IsActive: `true`

#### 4. بررسی Network

**✅ Ping دستگاه POS:**
```powershell
ping 192.168.1.104
```

**✅ بررسی Port:**
```powershell
# تست اتصال به Port 8080
Test-NetConnection -ComputerName 192.168.1.103 -Port 8080
```

**✅ بررسی Firewall:**
- مطمئن شوید Firewall اجازه اتصال به Port 8080 را می‌دهد
- اگر Windows Service روی سرور دیگری است، Firewall آن سرور را هم بررسی کنید

#### 5. بررسی Log ها

**✅ بررسی Log های Application:**
- مسیر Log: طبق تنظیمات Serilog
- جستجو برای: `SamanKish SignalR` یا `POS Test`

**✅ بررسی Log های Windows Service:**
- مسیر: `D:\Log\` (طبق تنظیمات Service)
- بررسی خطاهای اتصال

#### 6. تست دستی SignalR

**✅ استفاده از Browser:**
```
http://localhost:8080/signalr/hubs
```

اگر صفحه باز شد، Service در حال اجرا است.

**✅ استفاده از PowerShell:**
```powershell
# تست اتصال HTTP
Invoke-WebRequest -Uri "http://localhost:8080/signalr/hubs" -UseBasicParsing
```

---

## 🔧 راه‌حل‌های رایج

### مشکل 1: Windows Service متوقف است

**راه‌حل:**
```powershell
# شروع Service
Start-Service -Name "SSP1126SignalRWindowsService"

# بررسی وضعیت
Get-Service -Name "SSP1126SignalRWindowsService"
```

### مشکل 2: URL SignalR اشتباه است

**راه‌حل:**
1. بررسی IP سرور Windows Service
2. به‌روزرسانی `Web.config`:
   ```xml
   <add key="SamanKishSignalRUrl" value="http://[IP سرور]:8080/signalr" />
   ```
3. Restart Application Pool

### مشکل 3: Protocol ترمینال اشتباه است

**راه‌حل:**
```sql
-- تغییر Protocol به SignalR
UPDATE PosTerminal 
SET Protocol = 4  -- SignalR
WHERE PosTerminalId = [TerminalId]
```

### مشکل 4: Port 8080 بسته است

**راه‌حل:**
1. بررسی Firewall
2. بررسی اینکه Service واقعاً روی Port 8080 اجرا می‌شود
3. بررسی تنظیمات Service Config

### مشکل 5: Timeout در اتصال

**راه‌حل:**
1. افزایش Timeout در `SamanKishSignalRDriver.cs`:
   ```csharp
   private const int ConnectionTimeoutMs = 30000; // 30 seconds
   ```
2. بررسی Network Latency
3. بررسی اینکه Service پاسخ می‌دهد

---

## 📋 Checklist عیب‌یابی

- [ ] Windows Service در حال اجرا است
- [ ] Port 8080 باز است
- [ ] URL SignalR در `Web.config` صحیح است
- [ ] Protocol ترمینال = SignalR (4)
- [ ] IP Address ترمینال صحیح است
- [ ] Network قابل دسترس است
- [ ] Firewall اجازه اتصال می‌دهد
- [ ] Log ها بررسی شده‌اند
- [ ] Application Pool Restart شده است

---

## 🆘 در صورت عدم حل مشکل

1. **بررسی Log های کامل:**
   - Application Logs
   - Windows Service Logs
   - Event Viewer

2. **تست دستی:**
   - استفاده از `PosTestController`
   - تست با مبلغ کم (1000 ریال)

3. **تماس با پشتیبانی:**
   - ارسال Log ها
   - ارسال تنظیمات (بدون اطلاعات حساس)
   - ارسال Screenshot از خطا

---

**تاریخ:** 1402-06-29  
**وضعیت:** ✅ آماده استفاده

