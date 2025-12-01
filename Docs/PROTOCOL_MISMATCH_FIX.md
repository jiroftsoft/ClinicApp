# راهنمای حل مشکل Protocol Mismatch
**تاریخ:** 1402-12-01  
**کد درخواست:** SSP1126(WEB)

---

## 🔍 مشکل: خطای ConnectionRefused با Port 5000

### خطا:
```
اتصال به دستگاه کارت‌خوان رد شد
کد خطای Socket: ConnectionRefused (10061)
IP: 192.168.1.104
Port: 5000
```

### علت:
ترمینال با **Protocol = TCP/IP** تنظیم شده است، اما شما می‌خواهید از **SignalR** استفاده کنید.

**مشکل:**
- ترمینال در دیتابیس با `Protocol = 1` (TCP/IP) تنظیم شده است
- سیستم از `SamanKishDriver` (TCP/IP) استفاده می‌کند
- اما شما می‌خواهید از `SamanKishSignalRDriver` (SignalR) استفاده کنید

---

## ✅ راه‌حل

### 1. تغییر Protocol ترمینال به SignalR

**در دیتابیس:**
```sql
-- بررسی Protocol فعلی
SELECT PosTerminalId, TerminalId, IpAddress, Port, Protocol, Provider, IsActive
FROM PosTerminal
WHERE PosTerminalId = [TerminalId شما]

-- تغییر Protocol به SignalR (4)
UPDATE PosTerminal 
SET Protocol = 4  -- SignalR
WHERE PosTerminalId = [TerminalId شما]
```

**یا از منوی مدیریت ترمینال‌ها:**
1. مراجعه به منوی مدیریت ترمینال‌ها
2. ویرایش ترمینال مورد نظر
3. تغییر Protocol از "TCP/IP" به "SignalR"
4. ذخیره تغییرات

### 2. بررسی تنظیمات ترمینال

**بعد از تغییر Protocol:**
- ✅ Protocol: `SignalR` (4)
- ✅ IP Address: آدرس دستگاه POS (مثلاً `192.168.1.104`)
- ✅ TerminalId: شماره ترمینال
- ✅ MerchantId: شماره پذیرنده
- ✅ Port: می‌تواند null باشد (برای SignalR استفاده نمی‌شود)

---

## 🔧 تفاوت TCP/IP و SignalR

### TCP/IP (Protocol = 1)
- اتصال مستقیم به دستگاه POS از طریق TCP/IP
- نیاز به Port (مثلاً 5000)
- استفاده از `SamanKishDriver`
- اتصال: `IP:Port` (مثلاً `192.168.1.104:5000`)

### SignalR (Protocol = 4)
- اتصال از طریق SignalR Hub
- نیاز به Windows Service (SSP1126SignalRWindowsService)
- استفاده از `SamanKishSignalRDriver`
- اتصال: `http://localhost:8080/signalr` (به Service)
- Service خودش با دستگاه POS ارتباط برقرار می‌کند

---

## 📋 Checklist

- [ ] Protocol ترمینال = SignalR (4)
- [ ] Windows Service در حال اجرا است
- [ ] SignalR URL در Web.config صحیح است
- [ ] IP Address ترمینال صحیح است (برای Service)
- [ ] TerminalId صحیح است

---

## 🔍 بررسی Protocol

**در SQL Server:**
```sql
-- بررسی Protocol همه ترمینال‌ها
SELECT 
    PosTerminalId,
    Title,
    TerminalId,
    IpAddress,
    Port,
    Protocol,
    CASE Protocol
        WHEN 1 THEN 'TCP/IP'
        WHEN 2 THEN 'Serial'
        WHEN 3 THEN 'API'
        WHEN 4 THEN 'SignalR'
        ELSE 'Unknown'
    END AS ProtocolName,
    Provider,
    IsActive
FROM PosTerminal
WHERE IsDeleted = 0
ORDER BY PosTerminalId
```

---

## ⚠️ نکات مهم

1. **Protocol باید با Driver هماهنگ باشد:**
   - Protocol = TCP/IP → `SamanKishDriver`
   - Protocol = SignalR → `SamanKishSignalRDriver`

2. **برای SignalR:**
   - Port در ترمینال استفاده نمی‌شود
   - IP Address برای Service است (نه برای اتصال مستقیم)
   - Windows Service باید در حال اجرا باشد

3. **بعد از تغییر Protocol:**
   - Application Pool را Restart کنید
   - تست اتصال را مجدداً انجام دهید

---

**تاریخ:** 1402-12-01  
**وضعیت:** ✅ راه‌حل ارائه شده

