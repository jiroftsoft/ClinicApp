# 🚨 قرارداد Critical: ماژول‌های مالی (صندوق، پرداخت، گزارش‌ها)

**تاریخ:** 1404/10/05  
**نوع:** ⚠️ **CRITICAL CONTRACT**  
**اولویت:** **حداکثر**  
**الزامی برای:** تمام تغییرات مالی

---

## ⚠️ **هشدار بسیار مهم**

> **کوچکترین اشتباه در ماژول‌های مالی = مشکل حقوقی برای تیم برنامه‌نویسی**

```
موضوعات مالی = مسئولیت قانونی
- صندوق (CashSession)
- پرداخت (PaymentTransaction)
- گزارش‌ها (Reports)
- محاسبات (Calculations)
- رسیدها (Receipts)
```

---

## 🎯 اصول طلایی (Golden Rules)

### **1. هیچ تغییری بدون تست کامل**

```
❌ NEVER: تغییر کد مالی بدون تست
✅ ALWAYS: تست در 5 سناریو:
  1. حالت عادی
  2. Exception handling
  3. Concurrent requests
  4. Database failure
  5. Network failure
```

---

### **2. هر تراکنش = حتماً Log**

```csharp
// ❌ BAD: بدون log
_context.PaymentTransactions.Add(payment);
await _context.SaveChangesAsync();

// ✅ GOOD: با log کامل
_logger.Information("💰 PAYMENT: شروع ثبت پرداخت - ReceptionId: {ReceptionId}, Amount: {Amount}, Method: {Method}", 
    payment.ReceptionId, payment.Amount, payment.Method);

_context.PaymentTransactions.Add(payment);

try 
{
    await _context.SaveChangesAsync();
    _logger.Information("✅ PAYMENT: ثبت موفق - PaymentId: {PaymentId}", payment.PaymentTransactionId);
}
catch (Exception ex)
{
    _logger.Error(ex, "❌ PAYMENT: خطا در ثبت - ReceptionId: {ReceptionId}, Amount: {Amount}", 
        payment.ReceptionId, payment.Amount);
    throw;
}
```

---

### **3. Transaction Management الزامی**

```csharp
// ❌ BAD: بدون transaction
_context.PaymentTransactions.Add(payment);
cashSession.Balance += amount;
await _context.SaveChangesAsync();

// ✅ GOOD: با transaction
using (var transaction = _context.Database.BeginTransaction())
{
    try
    {
        _context.PaymentTransactions.Add(payment);
        cashSession.Balance += amount;
        
        await _context.SaveChangesAsync();
        
        transaction.Commit();
        _logger.Information("✅ TRANSACTION: Committed");
    }
    catch (Exception ex)
    {
        transaction.Rollback();
        _logger.Error(ex, "❌ TRANSACTION: Rollback");
        throw;
    }
}
```

---

### **4. Verification بعد از Save**

```csharp
await _context.SaveChangesAsync();

// ✅ VERIFY: بررسی واقعاً ذخیره شده؟
var saved = await _context.PaymentTransactions
    .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);

if (saved == null)
{
    _logger.Error("❌ VERIFY: PaymentTransaction ذخیره نشد!");
    throw new Exception("Payment was not saved!");
}
```

---

### **5. Idempotency برای همه پرداخت‌ها**

```csharp
// ✅ ALWAYS: چک کردن پرداخت تکراری
var existing = await _context.PaymentTransactions
    .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);

if (existing != null)
{
    _logger.Warning("⚠️ DUPLICATE: پرداخت قبلاً ثبت شده - IdempotencyKey: {Key}", idempotencyKey);
    return existing; // برگرداندن همان پرداخت قبلی
}
```

---

### **6. هیچ Hard-Delete در جداول مالی**

```csharp
// ❌ NEVER: حذف واقعی
_context.PaymentTransactions.Remove(payment);

// ✅ ALWAYS: Soft Delete
payment.IsDeleted = true;
payment.DeletedAt = DateTime.Now;
payment.DeletedByUserId = currentUserId;
_context.Entry(payment).State = EntityState.Modified;
```

---

### **7. Audit Trail کامل**

```csharp
public class PaymentTransaction : ISoftDelete, ITrackable
{
    // ✅ REQUIRED: تمام فیلدهای Audit
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedByUserId { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string DeletedByUserId { get; set; }
    
    // ✅ REQUIRED: RowVersion برای Concurrency
    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

---

## 📋 Checklist قبل از هر تغییر مالی

```
قبل از تغییر:
□ آیا این تغییر روی جداول مالی تاثیر دارد؟
□ آیا محاسبات تغییر می‌کند؟
□ آیا گزارش‌ها تحت تاثیر قرار می‌گیرند؟

اگر YES:
□ با مدیر فنی مشورت کن
□ با حسابدار مشورت کن
□ تست کامل بنویس
□ Document بنویس
□ Log کامل اضافه کن
□ Transaction management اضافه کن
□ Verification اضافه کن
□ Code Review دریافت کن
□ تست در محیط Staging
□ پشتیبان از Database بگیر
□ Plan برای Rollback داشته باش
```

---

## 🚫 **NEVER DO THIS (ممنوعیت‌های مطلق)**

```csharp
// ❌ 1. تغییر مبلغ بدون log
payment.Amount = newAmount; // خطرناک!

// ❌ 2. حذف PaymentTransaction
_context.PaymentTransactions.Remove(payment); // ممنوع!

// ❌ 3. تغییر Status بدون دلیل
payment.Status = PaymentStatus.Failed; // بدون log و دلیل؟

// ❌ 4. محاسبه بدون Validation
var total = item1.Price + item2.Price; // اگر null باشند؟

// ❌ 5. SaveChanges بدون try-catch
await _context.SaveChangesAsync(); // اگر exception بیاید؟

// ❌ 6. استفاده از Floating Point برای پول
decimal price = 123.45; // ✅ GOOD
float price = 123.45;   // ❌ BAD (rounding error!)

// ❌ 7. تغییر CashSession Balance مستقیم
cashSession.Balance = newBalance; // باید += یا -= باشد

// ❌ 8. ایجاد رسید بدون شماره یکتا
receipt.No = "R123"; // باید timestamp + random داشته باشد
```

---

## ✅ **ALWAYS DO THIS (الزامات)**

```csharp
// ✅ 1. Decimal برای مبالغ مالی
public decimal Amount { get; set; }
public decimal Price { get; set; }

// ✅ 2. Validation کامل
if (amount <= 0)
{
    throw new ArgumentException("Amount must be positive");
}

// ✅ 3. Idempotency Key
public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString();

// ✅ 4. Try-Catch با Log
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    _logger.Error(ex, "Concurrency conflict");
    throw;
}
catch (DbUpdateException ex)
{
    _logger.Error(ex, "Database update failed");
    throw;
}

// ✅ 5. تایید مبلغ
var calculated = CalculateTotal();
if (request.Amount != calculated)
{
    throw new InvalidOperationException($"Amount mismatch: {request.Amount} != {calculated}");
}

// ✅ 6. محاسبه با Round
var result = Math.Round(basePrice * taxRate, 0, MidpointRounding.AwayFromZero);

// ✅ 7. NULL Safety
var total = items?.Sum(i => i.Price ?? 0) ?? 0;

// ✅ 8. شماره رسید یکتا
var receiptNo = $"R{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
```

---

## 📊 Scenarios Critical

### **Scenario 1: پرداخت POS**

```
قبل از پرداخت:
1. ✅ CashSession باز است؟
2. ✅ ReceptionId معتبر است؟
3. ✅ مبلغ مثبت است؟
4. ✅ IdempotencyKey یکتا است؟
5. ✅ PosTerminal فعال است؟

حین پرداخت:
1. ✅ Transaction شروع شد
2. ✅ PaymentTransaction ایجاد شد
3. ✅ CashSession.PosBalance += Amount
4. ✅ Reception.Status = Completed
5. ✅ SaveChangesAsync()
6. ✅ Verification
7. ✅ Transaction.Commit()

بعد از پرداخت:
1. ✅ Log موفقیت
2. ✅ رسید ایجاد شد
3. ✅ Response به Frontend
```

---

### **Scenario 2: بستن صندوق**

```
قبل از بستن:
1. ✅ تمام تراکنش‌ها Completed هستند؟
2. ✅ موجودی نقدی + POS = مجموع پرداخت‌ها؟
3. ✅ هیچ پرداخت Pending نیست؟

حین بستن:
1. ✅ محاسبه نهایی موجودی
2. ✅ مقایسه با واقعیت
3. ✅ ثبت اختلاف (اگر وجود دارد)
4. ✅ CashSession.Status = Closed
5. ✅ CashSession.ClosedAt = Now
6. ✅ SaveChangesAsync()

بعد از بستن:
1. ✅ گزارش نهایی
2. ✅ ارسال به حسابداری
3. ✅ Backup
```

---

## 🔒 Security در ماژول‌های مالی

```csharp
// ✅ 1. Authorization
[Authorize(Roles = "Cashier, Admin")]
public async Task<ActionResult> FinalizePOS(...)

// ✅ 2. Input Validation
if (request.Amount < 0 || request.Amount > 1_000_000_000)
{
    return BadRequest("Invalid amount");
}

// ✅ 3. SQL Injection Prevention
// استفاده از EF Core یا Parameterized Queries

// ✅ 4. CSRF Protection
[ValidateAntiForgeryToken]

// ✅ 5. Rate Limiting
// جلوگیری از spam کردن API پرداخت
```

---

## 🧪 Test Coverage برای ماژول‌های مالی

```
MINIMUM: 95% Code Coverage

Test Cases:
□ Happy Path (حالت عادی)
□ Invalid Amount (مبلغ نامعتبر)
□ Duplicate Payment (پرداخت تکراری)
□ Closed CashSession (صندوق بسته)
□ Database Failure (خرابی دیتابیس)
□ Network Timeout (قطع شبکه)
□ Concurrent Requests (درخواست‌های همزمان)
□ Partial Failure (شکست جزئی)
□ Rollback Scenario (برگشت تراکنش)
□ Recovery Scenario (بازیابی)

Performance Tests:
□ 100 concurrent payments
□ 1000 payments per minute
□ Database connection pool exhaustion
```

---

## 📝 Documentation الزامی

برای هر تغییر مالی:

```markdown
## تغییرات
- چه چیزی تغییر کرد؟
- چرا تغییر کرد؟
- چه تاثیری دارد؟

## محاسبات
- فرمول محاسبه
- مثال عددی
- Edge cases

## Test Results
- سناریوهای تست شده
- نتایج
- Performance metrics

## Rollback Plan
- چگونه rollback کنیم؟
- چه dataهایی نیاز به بازیابی دارند؟

## Sign-off
- Developer: [نام]
- Code Reviewer: [نام]
- QA: [نام]
- Manager: [نام]
```

---

## 🚨 در صورت خطا (Incident Response)

```
فوری:
1. ✅ Log تمام اطلاعات
2. ✅ اطلاع به مدیر فنی
3. ✅ اطلاع به حسابداری
4. ✅ Freeze کردن عملیات مالی
5. ✅ Backup فوری از Database

تحلیل:
1. ✅ Root Cause Analysis
2. ✅ Impact Assessment
3. ✅ Data Integrity Check

اصلاح:
1. ✅ Fix با Code Review
2. ✅ Test کامل
3. ✅ Deploy در Staging
4. ✅ Verification

بازیابی:
1. ✅ اصلاح دیتای خراب (با موافقت حسابداری)
2. ✅ Verification
3. ✅ Document
4. ✅ Post-Mortem Meeting
```

---

## 📞 تماس اضطراری

```
خطای مالی شناسایی شد:
1. 🔴 فوراً کار را متوقف کن
2. 📞 تماس با مدیر فنی
3. 📞 تماس با حسابداری
4. 📝 Document همه چیز
5. ⏸️ منتظر دستور بمان
```

---

## ✅ Approval Process

```
کد مالی نیاز به تایید دارد:

1. Developer: کد می‌نویسد + Self-review
2. Senior Developer: Code Review
3. Tech Lead: Architecture Review
4. QA: Test می‌کند
5. Manager: Business Logic Review
6. Accountant: Financial Logic Review (اختیاری)

همه باید OK بدهند قبل از Merge
```

---

## 📚 مراجع

- `Docs/DEBUGGING_SPECIALIST_CONTRACT.md`
- `Docs/DEVELOPMENT_CONTRACT.md`
- `Models/Entities/Payment/PaymentTransaction.cs`
- `Models/Entities/Payment/CashSession.cs`
- `Services/Reception/ReceptionFacade.cs`

---

## 🎯 خلاصه

```
موضوعات مالی = مسئولیت قانونی

قوانین طلایی:
1. Log همه چیز
2. Transaction Management
3. Verification بعد از Save
4. Idempotency همیشه
5. Soft Delete فقط
6. Audit Trail کامل
7. Test 100%
8. Document همه چیز
9. Code Review الزامی
10. NEVER تغییر بدون تایید
```

---

**🚨 این قرارداد MANDATORY است و نقض آن = مشکل حقوقی!**

**تهیه‌کننده:** AI Assistant  
**تاریخ:** 1404/10/05  
**نوع:** CRITICAL CONTRACT  
**الزامی برای:** تمام تغییرات مالی

