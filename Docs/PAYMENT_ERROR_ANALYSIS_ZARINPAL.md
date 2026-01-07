# 🔍 تحلیل خطای پرداخت - مشکل احتمالی از ZarinPal

**تاریخ:** 2026-01-06  
**CorrelationId:** `59a38d7a-8793-4b9e-bc42-35d893ed96aa`  
**AppointmentId:** 35  
**وضعیت:** 🔴 Active Debugging

---

## 📋 خلاصه مشکل

خطای "خطا در ایجاد درخواست پرداخت در درگاه" رخ می‌دهد.

**مشاهدات:**
- ✅ CallbackUrl درست ساخته شده: `https://mehranyad.ir/Patient/AppointmentBooking/PaymentCallback`
- ✅ تنظیمات درست است (`Payment:BaseUrl`, `ZarinpalMerchantId`, `IsSandbox=false`)
- ❌ **لاگ‌های ZarinPal در لاگ‌ها نیست** (مثل `📤 ZarinPal: ارسال درخواست` یا `📥 ZarinPal: پاسخ دریافت شد`)

---

## 🔍 تحلیل

### مشکل احتمالی:

**لاگ‌های ZarinPal در لاگ‌ها نیست** یعنی:
1. ❌ درخواست به ZarinPalDriver نمی‌رسد
2. ❌ یا خطا قبل از ارسال درخواست رخ می‌دهد

### مراحل بررسی:

#### STEP 1: بررسی GetDefaultPaymentGatewayAsync

**لاگ مورد انتظار:**
```
🔍 WEB PAYMENT: شروع جستجوی درگاه پرداخت پیش‌فرض...
✅ WEB PAYMENT: درگاه پیش‌فرض یافت شد - GatewayId: {GatewayId}
```

**اگر این لاگ نیست:**
- ❌ `GetDefaultPaymentGatewayAsync` خطا می‌دهد
- ❌ یا درگاه پرداخت در Database یافت نمی‌شود

#### STEP 2: بررسی CreateGatewayPaymentRequestAsync

**لاگ مورد انتظار:**
```
🔧 WEB PAYMENT: شروع CreateGatewayPaymentRequestAsync - GatewayId: {GatewayId}
🔧 WEB PAYMENT: Driver انتخاب شد از Entity - GatewayId: {GatewayId}
```

**اگر این لاگ نیست:**
- ❌ `CreateGatewayPaymentRequestAsync` خطا می‌دهد
- ❌ یا Driver Factory خطا می‌دهد

#### STEP 3: بررسی ZarinPalDriver.RequestPaymentAsync

**لاگ مورد انتظار:**
```
💰 ZarinPal: شروع درخواست پرداخت - Amount: {Amount}
📤 ZarinPal: ارسال درخواست به {Url}
📥 ZarinPal: پاسخ دریافت شد - StatusCode: {StatusCode}
```

**اگر این لاگ نیست:**
- ❌ درخواست به ZarinPalDriver نمی‌رسد
- ❌ یا خطا قبل از ارسال درخواست رخ می‌دهد

---

## 🛠️ راه‌حل‌های پیشنهادی

### راه‌حل 1: بررسی Database (PaymentGateways)

```sql
SELECT PaymentGatewayId, Name, GatewayType, MerchantId, GatewayUrl, IsTestMode, IsActive, IsDefault, CallbackUrl, IsDeleted
FROM PaymentGateways
WHERE GatewayType = 'ZarinPal' AND IsDeleted = 0
ORDER BY IsDefault DESC, IsActive DESC;
```

**بررسی:**
- ✅ آیا درگاهی با `IsDefault = 1` وجود دارد؟
- ✅ آیا درگاهی با `IsActive = 1` وجود دارد؟
- ✅ آیا `MerchantId` درست است؟
- ✅ آیا `GatewayUrl` درست است؟

### راه‌حل 2: بررسی لاگ‌های دقیق‌تر

**جستجو در لاگ‌ها:**
```powershell
# جستجوی لاگ‌های WEB PAYMENT
Get-Content 'App_Data\Logs\clinicapp-20260106.log' | Select-String -Pattern 'WEB PAYMENT' | Select-Object -Last 20

# جستجوی لاگ‌های ZarinPal
Get-Content 'App_Data\Logs\clinicapp-20260106.log' | Select-String -Pattern 'ZarinPal' | Select-Object -Last 20
```

### راه‌حل 3: بررسی Exception در لاگ‌ها

**جستجو:**
```powershell
Get-Content 'App_Data\Logs\errors-20260106.log' | Select-String -Pattern '59a38d7a-8793-4b9e-bc42-35d893ed96aa' -Context 10,10
```

---

## 📊 چک‌لیست Debug

- [ ] ✅ `Payment:BaseUrl` در `Web.config` تنظیم شده است؟
- [ ] ✅ `ZarinpalMerchantId` درست است؟
- [ ] ✅ `Zarinpal:IsSandbox` برابر `false` است؟
- [ ] ✅ درگاه پرداخت در Database وجود دارد؟
- [ ] ✅ `IsDefault = 1` است؟
- [ ] ✅ `IsActive = 1` است؟
- [ ] ✅ `IsDeleted = 0` است؟
- [ ] ✅ لاگ‌های `GetDefaultPaymentGatewayAsync` وجود دارد؟
- [ ] ✅ لاگ‌های `CreateGatewayPaymentRequestAsync` وجود دارد؟
- [ ] ✅ لاگ‌های `ZarinPalDriver.RequestPaymentAsync` وجود دارد؟

---

## 🔗 مراجع

- `Docs/PAYMENT_DEBUG_QUICK_FIX.md` - راهنمای سریع
- `Docs/PAYMENT_ERROR_DIAGNOSIS_STEPS.md` - راهنمای کامل Debug
- `Docs/PAYMENT_DEBUG_GUIDE.md` - راهنمای جامع

---

**نکته:** اگر لاگ‌های ZarinPal در لاگ‌ها نیست، مشکل قبل از ارسال درخواست به ZarinPal است. باید لاگ‌های `GetDefaultPaymentGatewayAsync` و `CreateGatewayPaymentRequestAsync` را بررسی کنید.

