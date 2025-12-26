# 🔍 راهنمای Trace و Breakpoint برای پرداخت POS

## 📍 نقاط Breakpoint (به ترتیب اجرا)

### 1️⃣ Frontend - `finalizeAfterPayment` (خط 745)
**فایل:** `Scripts/reception.v2/payment-panel.js`  
**خط:** 745

**چک کنید:**
```javascript
// در Watch Window:
receptionId        // باید عدد باشد (مثلاً 2196)
amountIRR          // باید مبلغ پرداخت باشد (مثلاً 12000)
posData.rrn        // باید RRN از POS باشد
posData.traceNo    // باید TraceNo از POS باشد
posData.terminalId // باید TerminalId باشد
```

---

### 2️⃣ Frontend - `finalizeReception` (خط 816)
**فایل:** `Scripts/reception.v2/payment-panel.js`  
**خط:** 816

**چک کنید:**
```javascript
// در Watch Window:
payload.ReceptionId    // باید عدد باشد
payload.Amount         // باید مبلغ باشد (نه amountIRR!)
payload.PosPayment      // باید object باشد (نه pos!)
payload.PosPayment.RRN // باید RRN باشد
```

---

### 3️⃣ Backend Controller - `FinalizeWithPos` (خط 1649)
**فایل:** `Controllers/Api/ReceptionApiV1Controller.cs`  
**خط:** 1649

**چک کنید:**
```csharp
// در Watch Window:
request.ReceptionId           // باید عدد باشد
request.Amount                // باید مبلغ باشد
request.PosPayment            // باید null نباشد
request.PosPayment.RRN        // باید RRN باشد
request.IdempotencyKey        // باید GUID باشد
correlationId                 // برای tracking در Logs
```

---

### 4️⃣ Backend Facade - `FinalizePosAsync` (خط 3149)
**فایل:** `Services/Reception/ReceptionFacade.cs`  
**خط:** 3149

**چک کنید:**
```csharp
// در Watch Window:
request.ReceptionId           // باید عدد باشد
request.AmountIRR             // باید مبلغ باشد
draft                         // باید null نباشد
draft.Status                  // باید ReceptionStatus.Draft باشد
sessionResult.Success         // باید true باشد
sessionResult.Data.CashSessionId // باید عدد باشد
```

---

### 5️⃣ Backend Facade - ایجاد PaymentTransaction (خط 3225)
**فایل:** `Services/Reception/ReceptionFacade.cs`  
**خط:** 3225

**چک کنید:**
```csharp
// در Watch Window:
payment.ReceptionId       // باید عدد باشد
payment.Amount            // باید مبلغ باشد
payment.CashSessionId     // باید عدد باشد (مهم!)
payment.Status            // باید PaymentStatus.Success باشد
payment.IdempotencyKey    // باید GUID باشد
payment.Method            // باید PaymentMethod.POS باشد
payment.ReferenceCode     // باید RRN باشد
payment.TransactionId     // باید TraceNo باشد
```

**⚠️ CRITICAL:** اگر `payment.CashSessionId` صفر یا null است، مشکل از `GetOpenCashSessionAsync` است!

---

### 6️⃣ Backend Facade - SaveChangesAsync (خط 3287)
**فایل:** `Services/Reception/ReceptionFacade.cs`  
**خط:** 3287

**چک کنید:**
```csharp
// در Watch Window:
changeCount                    // باید > 0 باشد (مثلاً 3 برای PaymentTransaction, Reception, CashSession)
_context.Entry(payment).State  // باید EntityState.Added باشد
_context.Entry(draft).State    // باید EntityState.Modified باشد
_context.Entry(cashSession).State // باید EntityState.Modified باشد
```

**⚠️ CRITICAL:** اگر `changeCount == 0`، یعنی هیچ تغییری ذخیره نشده است!

---

### 7️⃣ Backend Facade - Verify PaymentTransaction (خط 3301)
**فایل:** `Services/Reception/ReceptionFacade.cs`  
**خط:** 3301

**چک کنید:**
```csharp
// در Watch Window:
savedPayment                    // باید null نباشد
savedPayment.PaymentTransactionId // باید > 0 باشد
savedPayment.Amount             // باید مبلغ باشد
savedPayment.CashSessionId      // باید عدد باشد
savedPayment.Status             // باید PaymentStatus.Success باشد
```

**⚠️ CRITICAL:** اگر `savedPayment == null`، یعنی PaymentTransaction ذخیره نشده است!

---

### 8️⃣ Backend Facade - Transaction Commit (خط 3347)
**فایل:** `Services/Reception/ReceptionFacade.cs`  
**خط:** 3347

**چک کنید:**
```csharp
// در Watch Window (قبل از Commit):
_context.Entry(payment).State  // باید EntityState.Unchanged باشد
_context.Entry(draft).State    // باید EntityState.Unchanged باشد
savedPayment.PaymentTransactionId // باید > 0 باشد

// بعد از Commit:
// بررسی کنید که آیا Exception رخ داده است یا نه
```

---

### 9️⃣ Backend Facade - Post-Commit Verification (خط 3352)
**فایل:** `Services/Reception/ReceptionFacade.cs`  
**خط:** 3352

**چک کنید:**
```csharp
// در Watch Window:
verifiedPayment                 // باید null نباشد
verifiedPayment.PaymentTransactionId // باید > 0 باشد
verifiedPayment.Amount          // باید مبلغ باشد
verifiedPayment.CashSessionId   // باید عدد باشد
```

**⚠️ CRITICAL:** اگر `verifiedPayment == null`، یعنی بعد از Commit هم PaymentTransaction در دیتابیس نیست!

---

## 🔍 سناریوهای مشکل‌دار

### ❌ سناریو 1: `changeCount == 0`
**علت:** Entity Framework تغییرات را تشخیص نداده است.  
**راه حل:**
- بررسی کنید که `_context.Entry(payment).State` برابر `EntityState.Added` باشد
- بررسی کنید که `_context.Entry(draft).State` برابر `EntityState.Modified` باشد
- بررسی کنید که `_context` همان Context است که Entity ها را اضافه کرده است

### ❌ سناریو 2: `savedPayment == null` (بعد از SaveChangesAsync)
**علت:** PaymentTransaction ذخیره نشده است.  
**راه حل:**
- بررسی کنید که `payment.CashSessionId` معتبر است و در دیتابیس وجود دارد
- بررسی کنید که Foreign Key constraint ها رعایت شده‌اند
- بررسی کنید که `IsDeleted` false است

### ❌ سناریو 3: `verifiedPayment == null` (بعد از Commit)
**علت:** Transaction Commit شده اما PaymentTransaction در دیتابیس نیست.  
**راه حل:**
- بررسی کنید که آیا Exception در Commit رخ داده است
- بررسی کنید که آیا Transaction Rollback شده است
- بررسی کنید که آیا دیتابیس دیگری در حال استفاده است

### ❌ سناریو 4: `payment.CashSessionId == 0` یا `null`
**علت:** `GetOpenCashSessionAsync` جلسه باز برنگردانده است.  
**راه حل:**
- بررسی کنید که `sessionResult.Success == true` است
- بررسی کنید که `sessionResult.Data != null` است
- بررسی کنید که `sessionResult.Data.CashSessionId > 0` است
- بررسی کنید که CashSession در دیتابیس وجود دارد

---

## 📝 لاگ‌های مهم

بعد از هر Breakpoint، لاگ‌های Serilog را بررسی کنید:

1. **App_Data/Logs/errors-YYYYMMDD.log** - برای خطاها
2. **App_Data/Logs/information-YYYYMMDD.log** - برای اطلاعات

**جستجو کنید:**
- `💰 POS PAYMENT START` - شروع پرداخت
- `💾 FACADE: SaveChangesAsync` - ذخیره تغییرات
- `✅ FACADE: PaymentTransaction با موفقیت ذخیره شد` - موفقیت
- `❌ CRITICAL` - خطاهای بحرانی

---

## ✅ Checklist برای Trace

- [ ] Breakpoint در `finalizeAfterPayment` (خط 745)
- [ ] Breakpoint در `finalizeReception` (خط 816)
- [ ] Breakpoint در `FinalizeWithPos` (خط 1649)
- [ ] Breakpoint در `FinalizePosAsync` (خط 3149)
- [ ] Breakpoint در ایجاد `PaymentTransaction` (خط 3225)
- [ ] Breakpoint در `SaveChangesAsync` (خط 3287)
- [ ] Breakpoint در Verify `PaymentTransaction` (خط 3301)
- [ ] Breakpoint در `Transaction Commit` (خط 3347)
- [ ] Breakpoint در Post-Commit Verification (خط 3352)

---

## 🎯 هدف Trace

هدف از این Trace این است که بفهمیم:
1. آیا `finalizeAfterPayment` فراخوانی می‌شود؟
2. آیا `FinalizePosAsync` فراخوانی می‌شود؟
3. آیا `PaymentTransaction` ایجاد می‌شود؟
4. آیا `SaveChangesAsync` تغییرات را ذخیره می‌کند؟
5. آیا `Transaction Commit` موفق است؟
6. آیا `PaymentTransaction` بعد از Commit در دیتابیس است؟

---

## 📞 در صورت مشکل

اگر در هر نقطه مشکلی دیدید:
1. مقدار متغیرها را در Watch Window بررسی کنید
2. لاگ‌های Serilog را بررسی کنید
3. Exception ها را بررسی کنید
4. State های Entity را بررسی کنید

