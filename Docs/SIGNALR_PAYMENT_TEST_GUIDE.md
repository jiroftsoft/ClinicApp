# راهنمای تست پرداخت SignalR
**تاریخ:** 1402-12-01  
**کد درخواست:** SSP1126(WEB)

---

## ✅ وضعیت ترمینال

از داده‌های شما:
- **PosTerminalId:** 1
- **TerminalId:** 41678252
- **Protocol:** 4 (SignalR) ✅
- **IP Address:** 192.168.1.104
- **Provider:** SamanKish
- **IsActive:** true ✅

**همه چیز درست است!** حالا باید تست کنیم.

---

## 🔍 مراحل تست

### 1. بررسی Windows Service

```powershell
# بررسی وضعیت Service
Get-Service -Name "SSP1126Service1" | Select-Object Name, Status

# اگر متوقف است، شروع کنید
Start-Service -Name "SSP1126Service1"
```

### 2. بررسی Log های Service

```powershell
# بررسی Log های Service
Get-Content "C:\Log\*.log" -Tail 50
```

### 3. تست اتصال

1. مراجعه به `/PosTest`
2. انتخاب ترمینال (TerminalId: 41678252)
3. کلیک روی "تست اتصال"
4. بررسی Log ها

### 4. تست پرداخت

1. انتخاب ترمینال
2. وارد کردن مبلغ (مثلاً 1000 ریال)
3. کلیک روی "تست پرداخت"
4. **کشیدن کارت روی دستگاه POS** (مهم!)
5. بررسی Log ها

---

## 📋 Log های مورد انتظار

### تست اتصال موفق:
```
🏥 SamanKish SignalR: Connecting to Hub - http://localhost:8080/signalr
✅ SamanKish SignalR: Connected to Hub successfully
🏥 SamanKish SignalR: Initializing - TerminalId: 41678252, IP: 192.168.1.104
🏥 SamanKish SignalR: Initial method invoked, waiting for GetSystemResponse...
🏥 SamanKish SignalR: GetSystemResponse received - Message: '0'
✅ SamanKish SignalR: Initialization successful
```

### تست پرداخت موفق:
```
🏥 SamanKish SignalR: Starting payment - TerminalId: 41678252, AmountIRR: 1,000
🏥 SamanKish SignalR: Initializing...
✅ SamanKish SignalR: Initialization successful
🏥 SamanKish SignalR: Sending payment - Amount: 1,000 Rials
🏥 SamanKish SignalR: SendAmount1Step invoked successfully, waiting for card swipe...
🏥 SamanKish SignalR: Card Swiped - TerminalId: 41678252, CardMask: ...
🏥 SamanKish SignalR: Transaction Response - TerminalId: 41678252, ResponseCode: 0, RRN: ...
✅ SamanKish SignalR: Payment successful - RRN: ..., TraceNo: ...
```

---

## ⚠️ مشکلات احتمالی

### 1. Initialization Timeout

**علت:** Service به Initial پاسخ نمی‌دهد

**راه‌حل:**
- بررسی Log های Service
- بررسی IP Address (192.168.1.104)
- بررسی اینکه Service در حال اجرا است

### 2. Transaction Timeout

**علت:** کارت کشیده نشده است

**راه‌حل:**
- **کشیدن کارت روی دستگاه POS** (مهم!)
- افزایش TransactionTimeout (فعلاً 60 ثانیه)
- بررسی اینکه دستگاه POS آماده است

### 3. GetSystemResponse دریافت نمی‌شود

**علت:** Callback کار نمی‌کند

**راه‌حل:**
- بررسی Log ها برای خطاهای Callback
- بررسی اینکه Hub Connection متصل است
- Restart Service

---

## 🔧 تنظیمات مهم

### Web.config:
```xml
<add key="SamanKishSignalRUrl" value="http://localhost:8080/signalr" />
```

### Service Config:
```xml
<add key="HostUrl" value="192.168.1.103" />
<add key="PosIP" value="192.168.1.104" />
<add key="LogPath" value="C:\Log\" />
```

### Terminal:
- Protocol: SignalR (4) ✅
- IP Address: 192.168.1.104 ✅
- TerminalId: 41678252 ✅

---

## 📝 نکات مهم

1. **کارت باید کشیده شود:** بعد از SendAmount1Step، باید کارت را روی دستگاه POS بکشید
2. **Timeout:** Transaction Timeout = 60 ثانیه (کافی برای کشیدن کارت)
3. **Log ها:** همیشه Log ها را بررسی کنید

---

**تاریخ:** 1402-12-01  
**وضعیت:** ✅ آماده تست

