# راهنمای تنظیمات SSP1126SignalRWindowsService
**تاریخ:** 1402-06-29  
**کد درخواست:** SSP1126(WEB)

---

## 📋 خلاصه

این راهنما شامل تنظیمات `SSP1126SignalRWindowsService.exe.config` و هماهنگی آن با `Web.config` است.

---

## ⚙️ تنظیمات SSP1126SignalRWindowsService.exe.config

### 1. فایل Config

**مسیر:** `Infrastructure/SSP1126(WEB)/ServiceInstaller_1402-06-29/Install/SSP1126SignalRWindowsService.exe.config`

### 2. پارامترهای مهم

#### 2.1. HostUrl
```xml
<add key="HostUrl" value="192.168.1.103" />
```

**توضیحات:**
- آدرس IP سرور که Windows Service روی آن اجرا می‌شود
- این آدرس باید با `SamanKishSignalRUrl` در `Web.config` هماهنگ باشد
- اگر Service روی همان سرور اجرا می‌شود، می‌توان از `localhost` استفاده کرد

**⚠️ نکته مهم:**
- `HostUrl` فقط IP است (بدون `http://` و Port)
- Service به صورت پیش‌فرض روی Port 8080 اجرا می‌شود

#### 2.2. PosIP
```xml
<add key="PosIP" value="192.168.1.104" />
```

**توضیحات:**
- آدرس IP دستگاه POS
- این آدرس باید در تنظیمات ترمینال در دیتابیس هم ثبت شود

#### 2.3. LogPath
```xml
<add key="LogPath" value="C:\\Log\\" />
```

**توضیحات:**
- مسیر ذخیره لاگ‌های Windows Service
- **تغییر یافته از `D:\Log\` به `C:\Log\`**
- باید مطمئن شوید که پوشه `C:\Log\` وجود دارد و Service دسترسی نوشتن دارد

**⚠️ نکات:**
- استفاده از `\\` برای Escape کردن `\` در XML
- Service باید دسترسی نوشتن به این پوشه داشته باشد
- اگر پوشه وجود ندارد، Service آن را ایجاد می‌کند (اگر دسترسی داشته باشد)

#### 2.4. MinimumAmount
```xml
<add key="MinimumAmount" value="1000" />
```

**توضیحات:**
- حداقل مبلغ تراکنش (به ریال)
- تراکنش‌های کمتر از این مبلغ رد می‌شوند

#### 2.5. TC (Terminal Count)
```xml
<add key="TC" value="3" />
```

**توضیحات:**
- تعداد دستگاه‌های کارتخوان متصل به Service
- باید با تعداد `Field_X` هماهنگ باشد

#### 2.6. Field_1, Field_2, Field_3
```xml
<add key="Field_1" value="D481AD1A890B133D91B3358E2F5079B25AC5F62A" />
<add key="Field_2" value="3AD7CA7370B78DE45FD6531CF403DA4501D67962" />
<add key="Field_3" value="420B99889EF7F323D4A074AC6315DD668028B2F6" />
```

**توضیحات:**
- شماره ترمینال هر دستگاه کارتخوان (به صورت Hash)
- تعداد `Field_X` باید با `TC` هماهنگ باشد
- اگر `TC = 3` باشد، باید `Field_1`, `Field_2`, `Field_3` وجود داشته باشد

#### 2.7. AuthorizationId
```xml
<add key="AuthorizationId" value="2" />
```

**توضیحات:**
- شناسه مجوز/مجوزدهی Service

---

## 🔗 هماهنگی با Web.config

### 1. SamanKishSignalRUrl

**در Web.config:**
```xml
<add key="SamanKishSignalRUrl" value="http://192.168.1.103:8080/signalr" />
```

**فرمول:**
```
SamanKishSignalRUrl = http://[HostUrl]:8080/signalr
```

**مثال:**
- اگر `HostUrl = 192.168.1.103` باشد:
  - `SamanKishSignalRUrl = http://192.168.1.103:8080/signalr`
- اگر `HostUrl = localhost` باشد:
  - `SamanKishSignalRUrl = http://localhost:8080/signalr`

---

## ✅ Checklist تنظیمات

### قبل از نصب Service

- [ ] `HostUrl` تنظیم شده است
- [ ] `PosIP` تنظیم شده است
- [ ] `LogPath` تنظیم شده است (مثلاً `C:\Log\`)
- [ ] پوشه Log وجود دارد یا Service دسترسی ایجاد آن را دارد
- [ ] `TC` با تعداد `Field_X` هماهنگ است
- [ ] `MinimumAmount` مناسب است

### بعد از نصب Service

- [ ] `SamanKishSignalRUrl` در `Web.config` با `HostUrl` هماهنگ است
- [ ] Service در حال اجرا است
- [ ] Port 8080 باز است
- [ ] Log ها در مسیر `LogPath` نوشته می‌شوند

---

## 🔧 تغییرات انجام شده

### 1. انتقال LogPath به درایو C

**قبل:**
```xml
<add key="LogPath" value="D:\\Log\\" />
```

**بعد:**
```xml
<add key="LogPath" value="C:\\Log\\" />
```

**دلایل:**
- درایو C معمولاً در همه سیستم‌ها وجود دارد
- جلوگیری از مشکلات دسترسی به درایو D
- ساده‌تر برای مدیریت

### 2. هماهنگی HostUrl با Web.config

**قبل (Web.config):**
```xml
<add key="SamanKishSignalRUrl" value="http://localhost:8080/signalr" />
```

**بعد (Web.config):**
```xml
<add key="SamanKishSignalRUrl" value="http://192.168.1.103:8080/signalr" />
```

**دلیل:**
- `HostUrl` در Service Config = `192.168.1.103`
- بنابراین URL باید `http://192.168.1.103:8080/signalr` باشد

---

## 📝 مراحل نصب و راه‌اندازی

### 1. تنظیم Config

1. باز کردن `SSP1126SignalRWindowsService.exe.config`
2. تنظیم `HostUrl` (مثلاً `192.168.1.103`)
3. تنظیم `PosIP` (مثلاً `192.168.1.104`)
4. تنظیم `LogPath` (مثلاً `C:\Log\`)
5. تنظیم `TC` و `Field_X` ها

### 2. ایجاد پوشه Log

```powershell
# ایجاد پوشه Log
New-Item -ItemType Directory -Path "C:\Log" -Force

# تنظیم دسترسی (اگر نیاز باشد)
icacls "C:\Log" /grant "NT AUTHORITY\SYSTEM:(OI)(CI)F"
```

### 3. نصب Service

```powershell
# نصب Service
InstallUtil.exe SSP1126SignalRWindowsService.exe
```

### 4. شروع Service

```powershell
# شروع Service
# نام واقعی Service: SSP1126Service1
Start-Service -Name "SSP1126Service1"

# بررسی وضعیت
Get-Service -Name "SSP1126Service1"

# Restart Service
Restart-Service -Name "SSP1126Service1"
```

### 5. تنظیم Web.config

1. باز کردن `Web.config`
2. تنظیم `SamanKishSignalRUrl`:
   ```xml
   <add key="SamanKishSignalRUrl" value="http://[HostUrl]:8080/signalr" />
   ```
3. Restart Application Pool

---

## 🔍 عیب‌یابی

### مشکل 1: Service نمی‌تواند به LogPath بنویسد

**راه‌حل:**
```powershell
# بررسی دسترسی
icacls "C:\Log"

# تنظیم دسترسی
icacls "C:\Log" /grant "NT AUTHORITY\SYSTEM:(OI)(CI)F"
icacls "C:\Log" /grant "Users:(OI)(CI)M"
```

### مشکل 2: URL در Web.config با HostUrl هماهنگ نیست

**راه‌حل:**
- بررسی `HostUrl` در `SSP1126SignalRWindowsService.exe.config`
- به‌روزرسانی `SamanKishSignalRUrl` در `Web.config`
- Restart Application Pool

### مشکل 3: Service نمی‌تواند به Port 8080 متصل شود

**راه‌حل:**
```powershell
# بررسی Port
netstat -ano | findstr :8080

# بررسی Firewall
Get-NetFirewallRule | Where-Object {$_.DisplayName -like "*8080*"}
```

---

## 📚 منابع

- ✅ `SSP1126SignalRWindowsService.exe.config` - تنظیمات Service
- ✅ `Web.config` - تنظیمات Application
- ✅ `Docs/POS_CONNECTION_TROUBLESHOOTING.md` - راهنمای عیب‌یابی

---

**تاریخ:** 1402-06-29  
**وضعیت:** ✅ به‌روزرسانی شده

