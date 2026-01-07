# 🔍 راهنمای دیباگ خطای پرداخت - CorrelationId: bd577cd3-cfff-43c0-8d50-0a1efba87f26

**تاریخ:** 2026-01-07  
**AppointmentId:** 37  
**CorrelationId:** `bd577cd3-cfff-43c0-8d50-0a1efba87f26`  
**خطا:** "خطا در ایجاد درخواست پرداخت در درگاه"

---

## 📋 خلاصه مشکل

کاربر هنگام تلاش برای پرداخت نوبت با خطای "خطا در ایجاد درخواست پرداخت در درگاه" مواجه می‌شود.

---

## 🔍 مراحل دیباگ

### 1️⃣ بررسی لاگ‌های سرور

```powershell
# جستجوی لاگ‌های مربوط به CorrelationId
Get-Content 'App_Data\Logs\clinicapp-*.log' | Select-String -Pattern 'bd577cd3-cfff-43c0-8d50-0a1efba87f26' | Select-String -Pattern 'WEB PAYMENT|ZarinPal|Gateway|CreatePaymentRequest|Driver|Authority|RequestUrl|CallbackUrl|Error' | Select-Object -First 100
```

### 2️⃣ بررسی تنظیمات Gateway در Database

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

### 3️⃣ بررسی لاگ‌های ZarinPal Driver

**لاگ‌های مورد انتظار:**
- `💰 ZarinPal: شروع درخواست پرداخت`
- `📤 ZarinPal: ارسال درخواست به {Url}`
- `🔍 ZarinPal DEBUG: IsSandbox={IsSandbox}, RequestUrl={RequestUrl}, CallbackUrl={CallbackUrl}`
- `📥 ZarinPal: پاسخ دریافت شد - StatusCode: {StatusCode}, Content: {Content}`
- `❌ ZarinPal: خطای API - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}`

### 4️⃣ بررسی لاگ‌های WebPaymentService

**لاگ‌های مورد انتظار:**
- `💰 WEB PAYMENT: شروع ایجاد درخواست پرداخت در درگاه`
- `🔍 WEB PAYMENT: شروع جستجوی درگاه پرداخت پیش‌فرض...`
- `🔧 WEB PAYMENT: شروع CreateGatewayPaymentRequestAsync`
- `❌ WEB PAYMENT: خطا در ایجاد درخواست پرداخت در درگاه`

---

## 🎯 نقاط بررسی

### ✅ Gateway Configuration
- [ ] `IsActive = 1`
- [ ] `IsDefault = 1`
- [ ] `IsTestMode = 0` (Production) یا `1` (Sandbox)
- [ ] `MerchantId` معتبر است
- [ ] `GatewayUrl` صحیح است
- [ ] `CallbackUrl` تنظیم شده است

### ✅ Callback URL
- [ ] Callback URL با دامن ثبت شده در ZarinPal مطابقت دارد
- [ ] `PaymentBaseUrl` در `Web.config` تنظیم شده است
- [ ] Callback URL به صورت Absolute است

### ✅ ZarinPal API Response
- [ ] Status Code از ZarinPal API
- [ ] Error Code (اگر موجود باشد)
- [ ] Error Message (اگر موجود باشد)
- [ ] Response Content کامل

---

## 🔧 راه‌حل‌های احتمالی

### 1️⃣ خطای Callback URL Domain Mismatch
**علت:** دامن Callback URL با دامن ثبت شده در ZarinPal مطابقت ندارد.

**راه‌حل:**
1. بررسی `PaymentBaseUrl` در `Web.config`
2. بررسی `CallbackUrl` در جدول `PaymentGateways`
3. اطمینان از اینکه دامن Callback URL با دامن ثبت شده در ZarinPal مطابقت دارد

### 2️⃣ خطای Merchant ID
**علت:** Merchant ID نامعتبر یا غیرفعال است.

**راه‌حل:**
1. بررسی Merchant ID در جدول `PaymentGateways`
2. بررسی Merchant ID در پنل ZarinPal
3. اطمینان از اینکه Merchant ID برای دامن `mehranyad.ir` فعال است

### 3️⃣ خطای ZarinPal API
**علت:** خطا از سمت ZarinPal API است.

**راه‌حل:**
1. بررسی Response Content از ZarinPal API
2. بررسی Error Code و Error Message
3. تماس با پشتیبانی ZarinPal در صورت نیاز

---

## 📝 لاگ‌های مورد نیاز

برای دیباگ کامل، نیاز به لاگ‌های زیر داریم:

1. **ZarinPal Driver:**
   - Request Body (JSON)
   - Request URL
   - Response Status Code
   - Response Content (کامل)
   - Error Code (اگر موجود باشد)
   - Error Message (اگر موجود باشد)

2. **WebPaymentService:**
   - Gateway Selection Log
   - CreateGatewayPaymentRequestAsync Log
   - Driver Response Log

3. **AppointmentBookingController:**
   - ProcessPayment Log
   - CallbackUrl Construction Log
   - Payment Request Log

---

## 🚀 مراحل بعدی

1. ✅ Application را Restart کنید
2. ✅ یک درخواست پرداخت جدید ایجاد کنید
3. ✅ لاگ‌های جدید را بررسی کنید
4. ✅ خطای دقیق را شناسایی کنید
5. ✅ راه‌حل مناسب را اعمال کنید

---

**نکته:** این راهنما برای دیباگ خطای پرداخت با CorrelationId `bd577cd3-cfff-43c0-8d50-0a1efba87f26` ایجاد شده است. برای خطاهای دیگر، CorrelationId را تغییر دهید.

