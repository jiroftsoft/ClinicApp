# راهنمای حل مشکل Invalid Hostname
**تاریخ:** 1402-06-29  
**کد درخواست:** SSP1126(WEB)

---

## 🔍 مشکل: Bad Request - Invalid Hostname

### خطا:
```
HTTP Error 400. The request hostname is invalid.
```

### علت:
Service فقط `localhost` را به عنوان Hostname معتبر می‌پذیرد و IP خارجی (`192.168.1.103`) را رد می‌کند.

---

## ✅ راه‌حل

### 1. استفاده از localhost در Web.config

**تغییر:**
```xml
<!-- قبل -->
<add key="SamanKishSignalRUrl" value="http://192.168.1.103:8080/signalr" />

<!-- بعد -->
<add key="SamanKishSignalRUrl" value="http://localhost:8080/signalr" />
```

**دلیل:**
- Service روی `0.0.0.0:8080` listen می‌کند (همه IP ها)
- اما Hostname validation فقط `localhost` را می‌پذیرد
- از آنجایی که Application و Service روی همان سرور هستند، استفاده از `localhost` صحیح است

### 2. بررسی تنظیمات Service

**HostUrl در Service Config:**
```xml
<add key="HostUrl" value="192.168.1.103" />
```

**⚠️ نکته:**
- `HostUrl` برای ارتباط Service با دستگاه POS است (نه برای اتصال Application به Service)
- برای اتصال Application به Service، باید از `localhost` استفاده شود

---

## 🔧 مراحل اصلاح

### 1. به‌روزرسانی Web.config

```xml
<add key="SamanKishSignalRUrl" value="http://localhost:8080/signalr" />
```

### 2. Restart Application Pool

بعد از تغییر `Web.config`، Application Pool را Restart کنید.

### 3. تست اتصال

```powershell
# تست با localhost
Test-NetConnection -ComputerName localhost -Port 8080

# تست در Browser
http://localhost:8080/signalr/hubs
```

---

## 📋 تفاوت HostUrl و SamanKishSignalRUrl

### HostUrl (در Service Config)
- **مقصود:** آدرس IP سرور برای ارتباط Service با دستگاه POS
- **مثال:** `192.168.1.103`
- **استفاده:** Service از این IP برای ارتباط با دستگاه POS استفاده می‌کند

### SamanKishSignalRUrl (در Web.config)
- **مقصود:** URL برای اتصال Application به SignalR Service
- **مثال:** `http://localhost:8080/signalr`
- **استفاده:** Application از این URL برای اتصال به SignalR Hub استفاده می‌کند

**⚠️ نکته مهم:**
- این دو می‌توانند متفاوت باشند
- `HostUrl` برای ارتباط Service → POS
- `SamanKishSignalRUrl` برای ارتباط Application → Service

---

## 🔍 عیب‌یابی

### مشکل: هنوز Invalid Hostname می‌گیرید

**راه‌حل:**
1. بررسی کنید که Service در حال اجرا است:
   ```powershell
   Get-Service -Name "SSP1126Service1"
   ```

2. بررسی کنید که Port 8080 باز است:
   ```powershell
   netstat -ano | findstr :8080
   ```

3. تست با localhost:
   ```powershell
   Test-NetConnection -ComputerName localhost -Port 8080
   ```

4. بررسی Log های Service:
   ```powershell
   Get-Content "C:\Log\*.log" -Tail 50
   ```

### مشکل: از سرور دیگری می‌خواهید متصل شوید

**راه‌حل:**
اگر Application روی سرور دیگری است و نمی‌تواند از `localhost` استفاده کند:
1. بررسی تنظیمات Service برای پذیرش IP خارجی
2. یا استفاده از Reverse Proxy
3. یا تغییر تنظیمات Service برای پذیرش همه Hostname ها

---

## ✅ خلاصه

| مورد | مقدار |
|------|-------|
| **HostUrl (Service Config)** | `192.168.1.103` |
| **SamanKishSignalRUrl (Web.config)** | `http://localhost:8080/signalr` |
| **دلیل استفاده از localhost** | Service فقط localhost را به عنوان Hostname معتبر می‌پذیرد |

---

**تاریخ:** 1402-06-29  
**وضعیت:** ✅ راه‌حل ارائه شده

