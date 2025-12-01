# راهنمای عیب‌یابی تست پرداخت
**تاریخ:** 1402-06-29  
**کد درخواست:** SSP1126(WEB)

---

## 🔍 مشکل: خطا در تست پرداخت

### علل احتمالی:

1. **Initialization Timeout**
   - Service به Initial پاسخ نمی‌دهد
   - Timeout خیلی کوتاه است

2. **SendAmount1Step مشکل دارد**
   - پارامترها نادرست هستند
   - Service پاسخ نمی‌دهد

3. **Transaction Timeout**
   - کارت کشیده نشده است
   - Timeout خیلی کوتاه است

4. **مشکل در دریافت پاسخ**
   - GetTransactionResponse دریافت نمی‌شود
   - Callback ثبت نشده است

---

## ✅ مراحل عیب‌یابی

### 1. بررسی Log ها

**بررسی Application Logs:**
```powershell
# بررسی آخرین Log ها
Get-Content "App_Data\Logs\*.log" -Tail 100 | Select-String "SamanKish SignalR"
```

**بررسی Service Logs:**
```powershell
# بررسی Log های Service
Get-Content "C:\Log\*.log" -Tail 100
```

### 2. بررسی مراحل پرداخت

**مراحل پرداخت:**
1. ✅ Connect to Hub
2. ⏳ Initial (ConnectionType, IP, COM, AccountType, Language, Timeout)
3. ⏳ Wait for GetSystemResponse (Timeout: 3 seconds)
4. ⏳ SendAmount1Step (Amount, Amounts, Additional, Reference, PurchaseID, TerminalID)
5. ⏳ Wait for GetCardSwiped (optional)
6. ⏳ Wait for GetTransactionResponse (Timeout: 60 seconds)

### 3. بررسی Timeout ها

**Timeout های فعلی:**
- ConnectionTimeout: 10 seconds
- InitializationDelay: 1 second + 2 seconds wait = 3 seconds total
- TransactionTimeout: 60 seconds

**اگر Timeout می‌گیرید:**
- افزایش InitializationDelay
- افزایش TransactionTimeout
- بررسی اینکه Service پاسخ می‌دهد

---

## 🔧 راه‌حل‌های رایج

### مشکل 1: Initialization Timeout

**علت:** Service به Initial پاسخ نمی‌دهد

**راه‌حل:**
1. بررسی Log های Service
2. بررسی اینکه Service در حال اجرا است
3. بررسی IP Address ترمینال
4. افزایش Timeout

### مشکل 2: SendAmount1Step مشکل دارد

**علت:** پارامترها نادرست هستند

**راه‌حل:**
1. بررسی TerminalId
2. بررسی Amount (باید به ریال باشد)
3. بررسی اینکه Amounts null است (برای Single Account)

### مشکل 3: Transaction Timeout

**علت:** کارت کشیده نشده است

**راه‌حل:**
1. اطمینان از اینکه کارت روی دستگاه کشیده می‌شود
2. افزایش TransactionTimeout
3. بررسی اینکه دستگاه POS آماده است

### مشکل 4: GetTransactionResponse دریافت نمی‌شود

**علت:** Callback ثبت نشده است

**راه‌حل:**
1. بررسی RegisterClientCallbacks
2. بررسی اینکه Hub Connection متصل است
3. بررسی Log ها برای خطاهای Callback

---

## 📋 Checklist

- [ ] Service در حال اجرا است
- [ ] Hub Connection متصل است
- [ ] TerminalId صحیح است
- [ ] IP Address ترمینال صحیح است
- [ ] Amount معتبر است (بیشتر از 1000 ریال)
- [ ] Timeout ها مناسب هستند
- [ ] Log ها بررسی شده‌اند
- [ ] دستگاه POS آماده است

---

## 🔍 بررسی Log ها

### Log های مهم:

1. **Initialization:**
   ```
   🏥 SamanKish SignalR: Initializing - TerminalId: {TerminalId}, IP: {IpAddress}
   ✅ SamanKish SignalR: Initialization successful
   ```

2. **SendAmount1Step:**
   ```
   🏥 SamanKish SignalR: Sending payment - Amount: {Amount:N0} Rials
   🏥 SamanKish SignalR: Payment request sent, waiting for card swipe...
   ```

3. **Transaction Response:**
   ```
   🏥 SamanKish SignalR: Transaction Response - TerminalId: {TerminalId}, ResponseCode: {ResponseCode}, RRN: {RRN}
   ✅ SamanKish SignalR: Payment successful - RRN: {RRN}, TraceNo: {TraceNo}
   ```

---

## ⚠️ نکات مهم

1. **Amount باید به ریال باشد** (نه تومان)
2. **TerminalId باید صحیح باشد** (از تنظیمات ترمینال)
3. **IP Address باید صحیح باشد** (آدرس دستگاه POS)
4. **Timeout ها باید مناسب باشند** (حداقل 60 ثانیه برای Transaction)
5. **دستگاه POS باید آماده باشد** (کارت باید کشیده شود)

---

**تاریخ:** 1402-06-29  
**وضعیت:** ✅ راهنمای عیب‌یابی

