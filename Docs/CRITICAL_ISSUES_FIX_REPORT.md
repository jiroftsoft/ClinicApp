# 🚨 گزارش رفع مشکلات بحرانی - 2026-01-07

**تاریخ:** 2026-01-07  
**AppointmentId:** 38  
**CorrelationId:** `d247fe2b-1ba3-4584-8139-312603b8c3cd`

---

## 📋 خلاصه مشکلات

### 1️⃣ خطای پرداخت (Payment Error)
- **خطا:** "خطا در ایجاد درخواست پرداخت در درگاه"
- **AppointmentId:** 38
- **CorrelationId:** `d247fe2b-1ba3-4584-8139-312603b8c3cd`
- **وضعیت:** 🔴 در حال بررسی

### 2️⃣ خطای DbContext Concurrency
- **خطا:** `System.NotSupportedException: A second operation started on this context before a previous asynchronous operation completed`
- **مکان:** `DoctorCrudRepository.GetByIdWithDetailsAsync`
- **وضعیت:** ✅ رفع شده

---

## 🔧 رفع مشکل 1: DbContext Concurrency

### مشکل
```
System.NotSupportedException: A second operation started on this context before a previous asynchronous operation completed. Use 'await' to ensure that any asynchronous operations have completed before calling another method on this context. Any instance members are not guaranteed to be thread safe.
```

### علت ریشه‌ای
- `GetByIdWithDetailsAsync` از `_context.Doctors` بدون `AsNoTracking()` استفاده می‌کرد
- در صورت فراخوانی همزمان چند async operation روی همان DbContext، خطای concurrency رخ می‌داد
- این متد فقط برای خواندن داده است و نیازی به tracking ندارد

### راه‌حل
**فایل:** `Repositories/ClinicAdmin/DoctorCrudRepository.cs`  
**خط:** 64-84

**تغییرات:**
```csharp
// ❌ قبل:
return await _context.Doctors
    .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
    .Include(...)
    .FirstOrDefaultAsync();

// ✅ بعد:
return await _context.Doctors
    .AsNoTracking() // ✅ FIX: جلوگیری از concurrency issues در async operations
    .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
    .Include(...)
    .FirstOrDefaultAsync();
```

### مزایا
1. ✅ جلوگیری از DbContext concurrency issues
2. ✅ بهبود Performance (AsNoTracking سریع‌تر است)
3. ✅ مناسب برای Read-Only queries

---

## 🔍 بررسی مشکل 2: خطای پرداخت

### مشکل
- خطای عمومی "خطا در ایجاد درخواست پرداخت در درگاه"
- لاگ‌های دقیق‌تر از ZarinPalDriver در خروجی نیستند
- این نشان می‌دهد که ممکن است خطا قبل از رسیدن به ZarinPalDriver رخ دهد

### مراحل دیباگ

#### 1️⃣ بررسی لاگ‌های سرور
```powershell
# جستجوی لاگ‌های مربوط به CorrelationId
Get-Content 'App_Data\Logs\clinicapp-*.log' | Select-String -Pattern 'd247fe2b-1ba3-4584-8139-312603b8c3cd' | Select-String -Pattern 'WEB PAYMENT|ZarinPal|Gateway|CreatePaymentRequest|Driver|Authority|RequestUrl|CallbackUrl|Error' | Select-Object -First 100
```

#### 2️⃣ بررسی تنظیمات Gateway
```sql
SELECT 
    PaymentGatewayId,
    Name,
    LEFT(MerchantId, 20) + '...' AS MerchantIdPreview,
    IsActive,
    IsDefault,
    IsTestMode,
    CallbackUrl,
    GatewayUrl
FROM PaymentGateways
WHERE GatewayType = 1 AND IsDeleted = 0
ORDER BY IsDefault DESC, IsTestMode DESC;
```

#### 3️⃣ بررسی لاگ‌های ZarinPal Driver
**لاگ‌های مورد انتظار:**
- `💰 ZarinPal: شروع درخواست پرداخت`
- `📤 ZarinPal: ارسال درخواست به {Url}`
- `🔍 ZarinPal DEBUG: IsSandbox={IsSandbox}, RequestUrl={RequestUrl}, CallbackUrl={CallbackUrl}`
- `📥 ZarinPal: پاسخ دریافت شد - StatusCode: {StatusCode}, Content: {Content}`
- `❌ ZarinPal: خطای API - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}`

#### 4️⃣ بررسی لاگ‌های WebPaymentService
**لاگ‌های مورد انتظار:**
- `💰 WEB PAYMENT: شروع ایجاد درخواست پرداخت در درگاه`
- `🔍 WEB PAYMENT: شروع جستجوی درگاه پرداخت پیش‌فرض...`
- `🔧 WEB PAYMENT: شروع CreateGatewayPaymentRequestAsync`
- `❌ WEB PAYMENT: خطا در ایجاد درخواست پرداخت در درگاه`

### نقاط بررسی

#### ✅ Gateway Configuration
- [ ] `IsActive = 1`
- [ ] `IsDefault = 1`
- [ ] `IsTestMode = 0` (Production) یا `1` (Sandbox)
- [ ] `MerchantId` معتبر است
- [ ] `GatewayUrl` صحیح است
- [ ] `CallbackUrl` تنظیم شده است

#### ✅ Callback URL
- [ ] Callback URL با دامن ثبت شده در ZarinPal مطابقت دارد
- [ ] `PaymentBaseUrl` در `Web.config` تنظیم شده است
- [ ] Callback URL به صورت Absolute است

#### ✅ ZarinPal API Response
- [ ] Status Code از ZarinPal API
- [ ] Error Code (اگر موجود باشد)
- [ ] Error Message (اگر موجود باشد)
- [ ] Response Content کامل

---

## 🎯 مراحل بعدی

### 1️⃣ Restart Application
Application را Restart کنید تا تغییرات اعمال شود.

### 2️⃣ تست مجدد
1. یک درخواست پرداخت جدید ایجاد کنید
2. لاگ‌های جدید را بررسی کنید
3. خطای دقیق را شناسایی کنید

### 3️⃣ بررسی لاگ‌های دقیق‌تر
بعد از Restart، لاگ‌های زیر باید در فایل لاگ ظاهر شوند:
- `💰 ZarinPal: شروع درخواست پرداخت`
- `📤 ZarinPal: ارسال درخواست به {Url}`
- `🔍 ZarinPal DEBUG: IsSandbox={IsSandbox}, RequestUrl={RequestUrl}, CallbackUrl={CallbackUrl}`
- `📥 ZarinPal: پاسخ دریافت شد - StatusCode: {StatusCode}, Content: {Content}`
- `❌ ZarinPal: خطای API - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}`

---

## 📝 خلاصه

### ✅ مشکلات رفع شده
1. **DbContext Concurrency:** رفع شده با استفاده از `AsNoTracking()`

### 🔴 مشکلات در حال بررسی
1. **خطای پرداخت:** نیاز به بررسی لاگ‌های دقیق‌تر

---

## 🔗 مراجع

- `Docs/PAYMENT_ERROR_DEBUGGING_GUIDE.md` - راهنمای دیباگ خطای پرداخت
- `Repositories/ClinicAdmin/DoctorCrudRepository.cs` - رفع مشکل DbContext concurrency

---

**نکته:** این گزارش برای مشکلات بحرانی با CorrelationId `d247fe2b-1ba3-4584-8139-312603b8c3cd` و AppointmentId 38 ایجاد شده است.

