# 🚫 نقشه راه: رفع مشکل مالی در لغو پذیرش

**تاریخ:** 1404/09/15  
**وضعیت:** 📋 **در انتظار پیاده‌سازی**  
**اولویت:** 🔴 **بالا** (مغایرت مالی)

---

## 📊 خلاصه اجرایی

### مشکل اصلی
وقتی پذیرش لغو می‌شود، مبالغ مالی (سهم بیمه، سهم بیمار و غیره) صفر نمی‌شوند و این باعث ایجاد مغایرت در محاسبات مالی و صندوق می‌شود.

### سناریوی مشکل
1. بیمار پذیرش می‌شود با مبلغ 1,000,000 ریال
   - سهم بیمار: 300,000 ریال
   - سهم بیمه پایه: 500,000 ریال
   - سهم بیمه تکمیلی: 200,000 ریال
2. بیمار پرداخت می‌کند: 300,000 ریال (نقدی)
3. CashSession.CashBalance += 300,000
4. بیمار منصرف می‌شود و پذیرش لغو می‌شود
5. ❌ **مشکل:** مبالغ مالی صفر نمی‌شوند!
   - TotalAmount: 1,000,000 (باید 0 شود)
   - PatientCoPay: 300,000 (باید 0 شود)
   - BasePay: 500,000 (باید 0 شود)
   - SuppPay: 200,000 (باید 0 شود)
6. ❌ **مشکل:** CashSession.CashBalance به‌روزرسانی نمی‌شود!
   - CashBalance: 300,000 (باید کاهش یابد)
   - Refund Transaction ثبت می‌شود اما Balance به‌روزرسانی نمی‌شود

---

## 🔍 تحلیل وضعیت فعلی

### کد فعلی (`ReceptionFacade.CancelReceptionAsync`)

#### ✅ کارهایی که انجام می‌شود:
1. ✅ بررسی امکان لغو
2. ✅ ثبت Refund Transaction (اگر پرداختی وجود داشته باشد)
3. ✅ تغییر Status به Cancelled
4. ✅ ثبت دلیل لغو در Notes

#### ❌ کارهایی که انجام نمی‌شود:
1. ❌ **صفر کردن مبالغ مالی:**
   - `TotalAmount` صفر نمی‌شود
   - `PatientCoPay` صفر نمی‌شود
   - `BasePay` صفر نمی‌شود
   - `SuppPay` صفر نمی‌شود
   - `InsurerShareAmount` صفر نمی‌شود

2. ❌ **به‌روزرسانی CashSession Balance:**
   - `CashSession.CashBalance` کاهش نمی‌یابد (برای Refund نقدی)
   - `CashSession.PosBalance` کاهش نمی‌یابد (برای Refund POS)

3. ❌ **بررسی استفاده در گزارش‌های مالی:**
   - باید بررسی شود که آیا Cancelled receptions در گزارش‌ها فیلتر می‌شوند یا نه

---

## 🎯 راه‌حل پیشنهادی

### گام 1: صفر کردن مبالغ مالی

**مکان:** `Services/Reception/ReceptionFacade.cs` - `CancelReceptionAsync`

**تغییرات:**
```csharp
// 5. تغییر وضعیت به Cancelled
var previousStatus = reception.Status;
reception.Status = ReceptionStatus.Cancelled;

// ✅ جدید: صفر کردن مبالغ مالی برای جلوگیری از مغایرت
reception.TotalAmount = 0;
reception.PatientCoPay = 0;
reception.BasePay = 0;
reception.SuppPay = 0;
reception.InsurerShareAmount = 0;
reception.PatientPay = 0; // اگر وجود دارد

_logger.Information("💰 FACADE: مبالغ مالی صفر شدند - ReceptionId: {ReceptionId}", 
    reception.ReceptionId);
```

**دلیل:**
- جلوگیری از مغایرت در محاسبات مالی
- اطمینان از اینکه پذیرش لغو شده در گزارش‌های مالی تاثیر نمی‌گذارد
- حفظ یکپارچگی داده‌ها

---

### گام 2: به‌روزرسانی CashSession Balance

**مکان:** `Services/Reception/ReceptionFacade.cs` - `CancelReceptionAsync`

**تغییرات:**
```csharp
if (hasPayment && request.ProcessRefund)
{
    // ثبت تراکنش Refund
    var refundTransaction = new Models.Entities.Payment.PaymentTransaction
    {
        ReceptionId = reception.ReceptionId,
        Amount = -totalPaid, // منفی برای Refund
        Status = PaymentStatus.Canceled,
        Method = successfulPayments.FirstOrDefault()?.Method ?? PaymentMethod.Cash,
        Description = $"برگشت وجه (Refund) - دلیل: {request.RefundReason ?? request.Reason}",
        IdempotencyKey = Guid.NewGuid().ToString(),
        CreatedAt = DateTime.Now,
        CreatedByUserId = _currentUserService.UserId
    };

    _context.PaymentTransactions.Add(refundTransaction);

    // ✅ جدید: به‌روزرسانی CashSession Balance
    if (refundTransaction.Method == PaymentMethod.Cash)
    {
        // دریافت CashSession از اولین تراکنش موفق
        var firstPayment = successfulPayments.FirstOrDefault();
        if (firstPayment?.CashSessionId.HasValue == true)
        {
            var cashSession = await _context.CashSessions
                .FindAsync(firstPayment.CashSessionId.Value);
            
            if (cashSession != null)
            {
                cashSession.CashBalance -= totalPaid; // کاهش Balance
                cashSession.UpdatedAt = DateTime.Now;
                cashSession.UpdatedByUserId = _currentUserService.UserId;
                
                _logger.Information("💰 FACADE: CashSession.CashBalance کاهش یافت - SessionId: {SessionId}, Amount: {Amount}, New Balance: {NewBalance}",
                    cashSession.CashSessionId, totalPaid, cashSession.CashBalance);
            }
            else
            {
                _logger.Warning("⚠️ FACADE: CashSession یافت نشد - SessionId: {SessionId}",
                    firstPayment.CashSessionId.Value);
            }
        }
    }
    else if (refundTransaction.Method == PaymentMethod.POS)
    {
        // برای POS هم باید PosBalance کاهش یابد
        var firstPayment = successfulPayments.FirstOrDefault();
        if (firstPayment?.CashSessionId.HasValue == true)
        {
            var cashSession = await _context.CashSessions
                .FindAsync(firstPayment.CashSessionId.Value);
            
            if (cashSession != null)
            {
                cashSession.PosBalance -= totalPaid; // کاهش Balance
                cashSession.UpdatedAt = DateTime.Now;
                cashSession.UpdatedByUserId = _currentUserService.UserId;
                
                _logger.Information("💰 FACADE: CashSession.PosBalance کاهش یافت - SessionId: {SessionId}, Amount: {Amount}, New Balance: {NewBalance}",
                    cashSession.CashSessionId, totalPaid, cashSession.PosBalance);
            }
        }
    }

    refundAmount = totalPaid;
    refundProcessed = true;

    _logger.Information("💰 FACADE: Refund ثبت شد و CashSession به‌روزرسانی شد - ReceptionId: {ReceptionId}, Amount: {Amount}", 
        request.ReceptionId, totalPaid);
}
```

**دلیل:**
- حفظ یکپارچگی CashSession Balance
- جلوگیری از مغایرت در صندوق
- ردیابی دقیق تراکنش‌های مالی

---

### گام 3: بررسی گزارش‌های مالی

**مکان:** تمام گزارش‌های مالی که از Reception استفاده می‌کنند

**بررسی موارد زیر:**
1. ✅ فیلتر کردن Cancelled receptions در گزارش‌های مالی
2. ✅ استفاده از `Status != ReceptionStatus.Cancelled` در queries
3. ✅ بررسی گزارش‌های زیر:
   - گزارش درآمد روزانه
   - گزارش درآمد ماهانه
   - گزارش سهم بیمه
   - گزارش سهم بیمار
   - گزارش صندوق (CashSession)

**مثال:**
```csharp
// ❌ قبل (اشتباه):
var totalRevenue = receptions.Sum(r => r.TotalAmount);

// ✅ بعد (درست):
var totalRevenue = receptions
    .Where(r => r.Status != ReceptionStatus.Cancelled)
    .Sum(r => r.TotalAmount);
```

---

### گام 4: به‌روزرسانی ReceptionItems (اختیاری)

**نکته:** اگر می‌خواهیم ReceptionItems را هم صفر کنیم (برای سازگاری کامل):

```csharp
// صفر کردن مبالغ ReceptionItems
foreach (var item in reception.ReceptionItems.Where(i => !i.IsDeleted))
{
    item.UnitPrice = 0;
    item.PatientShareAmount = 0;
    item.InsurerShareAmount = 0;
    // یا می‌توانیم IsDeleted = true کنیم
    // item.IsDeleted = true;
}
```

**توصیه:** این کار اختیاری است و فقط در صورت نیاز انجام شود.

---

## 📋 چک‌لیست پیاده‌سازی

### فاز 1: اصلاح منطق لغو (اولویت بالا)
- [ ] صفر کردن `TotalAmount`
- [ ] صفر کردن `PatientCoPay`
- [ ] صفر کردن `BasePay`
- [ ] صفر کردن `SuppPay`
- [ ] صفر کردن `InsurerShareAmount`
- [ ] به‌روزرسانی `CashSession.CashBalance` (برای Refund نقدی)
- [ ] به‌روزرسانی `CashSession.PosBalance` (برای Refund POS)
- [ ] اضافه کردن Logging مناسب

### فاز 2: بررسی گزارش‌های مالی (اولویت متوسط)
- [ ] بررسی گزارش درآمد روزانه
- [ ] بررسی گزارش درآمد ماهانه
- [ ] بررسی گزارش سهم بیمه
- [ ] بررسی گزارش سهم بیمار
- [ ] بررسی گزارش صندوق
- [ ] اضافه کردن فیلتر `Status != Cancelled` در تمام گزارش‌ها

### فاز 3: تست و اعتبارسنجی (اولویت بالا)
- [ ] تست لغو پذیرش بدون پرداخت
- [ ] تست لغو پذیرش با پرداخت نقدی
- [ ] تست لغو پذیرش با پرداخت POS
- [ ] بررسی CashSession Balance بعد از Refund
- [ ] بررسی گزارش‌های مالی بعد از لغو
- [ ] تست سناریوهای Edge Case

### فاز 4: مستندسازی (اولویت پایین)
- [ ] به‌روزرسانی مستندات API
- [ ] به‌روزرسانی مستندات Business Logic
- [ ] اضافه کردن مثال‌های استفاده

---

## 🔒 نکات امنیتی و یکپارچگی

### 1. Transaction Safety
- تمام تغییرات باید در یک Transaction انجام شوند
- در صورت خطا، Rollback شود

### 2. Idempotency
- بررسی شود که Refund دوباره انجام نشود
- استفاده از `IdempotencyKey` برای جلوگیری از تراکنش‌های تکراری

### 3. Audit Trail
- تمام تغییرات باید Log شوند
- ردیابی کاربر انجام‌دهنده
- ثبت تاریخ و زمان دقیق

### 4. Validation
- بررسی شود که CashSession باز است
- بررسی شود که Balance کافی است (برای Refund)
- بررسی شود که Reception قابل لغو است

---

## 📊 مثال سناریوی کامل

### قبل از اصلاح:
```
پذیرش:
- ReceptionId: 1001
- Status: Completed
- TotalAmount: 1,000,000
- PatientCoPay: 300,000
- BasePay: 500,000
- SuppPay: 200,000

پرداخت:
- Amount: 300,000 (نقدی)
- CashSession Balance: +300,000

بعد از لغو (فعلی - اشتباه):
- Status: Cancelled
- TotalAmount: 1,000,000 ❌ (باید 0 باشد)
- PatientCoPay: 300,000 ❌ (باید 0 باشد)
- BasePay: 500,000 ❌ (باید 0 باشد)
- SuppPay: 200,000 ❌ (باید 0 باشد)
- CashSession Balance: 300,000 ❌ (باید 0 باشد)
- Refund Transaction: -300,000 ✅
```

### بعد از اصلاح:
```
بعد از لغو (درست):
- Status: Cancelled
- TotalAmount: 0 ✅
- PatientCoPay: 0 ✅
- BasePay: 0 ✅
- SuppPay: 0 ✅
- CashSession Balance: 0 ✅ (300,000 - 300,000)
- Refund Transaction: -300,000 ✅
```

---

## 🚀 اولویت‌بندی

### 🔴 فوری (این هفته):
1. صفر کردن مبالغ مالی
2. به‌روزرسانی CashSession Balance

### 🟡 مهم (این ماه):
3. بررسی گزارش‌های مالی
4. تست کامل

### 🟢 اختیاری (بعداً):
5. به‌روزرسانی ReceptionItems
6. مستندسازی کامل

---

## 📝 خلاصه

**مشکل:** مبالغ مالی در پذیرش لغو شده صفر نمی‌شوند و CashSession Balance به‌روزرسانی نمی‌شود.

**راه‌حل:**
1. صفر کردن تمام مبالغ مالی هنگام لغو
2. به‌روزرسانی CashSession Balance برای Refund
3. فیلتر کردن Cancelled receptions در گزارش‌های مالی

**اولویت:** 🔴 **بالا** - باید هرچه سریع‌تر پیاده‌سازی شود.

---

**تهیه شده توسط:** AI Assistant  
**تاریخ:** 1404/09/15  
**نسخه:** 1.0

