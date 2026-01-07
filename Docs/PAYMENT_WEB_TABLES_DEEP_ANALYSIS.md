# 🔍 تحلیل عمیق: جداول پرداخت وب (Payment Web Tables)

**تاریخ:** 2026-01-06  
**هدف:** درک کامل معماری جداول پرداخت وب برای رفع مشکل "درگاه پرداخت پیش‌فرض یافت نشد"

---

## 📊 ساختار جداول (Database Schema)

### 1. PaymentGateways (درگاه‌های پرداخت)

**جدول:** `PaymentGateways`  
**Entity:** `PaymentGateway`

#### فیلدهای کلیدی:
```sql
PaymentGatewayId (PK, int, Identity)
Name (nvarchar(100), Required) - نام درگاه
GatewayType (int, Required) - نوع درگاه (ZarinPal, PayPing, ...)
MerchantId (nvarchar(100), Required) - شناسه مرچنت
ApiKey (nvarchar(500), Required) - کلید API
PrivateKey (nvarchar(500), Optional) - کلید خصوصی
GatewayUrl (nvarchar(500), Required) - URL درگاه
CallbackUrl (nvarchar(500), Required) - ⚠️ URL بازگشت (الزامی)
SuccessUrl (nvarchar(500), Optional)
ErrorUrl (nvarchar(500), Optional)
IsActive (bit, Required) - آیا درگاه فعال است؟
IsDefault (bit, Required) - آیا درگاه پیش‌فرض است؟
IsDeleted (bit, Required) - Soft Delete
CreatedAt (datetime, Required)
CreatedByUserId (nvarchar(128), Optional)
UpdatedAt (datetime, Optional)
UpdatedByUserId (nvarchar(128), Optional)
```

#### Index ها:
- `IX_PaymentGateway_Name`
- `IX_PaymentGateway_GatewayType`
- `IX_PaymentGateway_MerchantId`
- `IX_PaymentGateway_IsActive`
- `IX_PaymentGateway_IsDeleted`

#### روابط:
- `OnlinePayments` (One-to-Many) - لیست پرداخت‌های آنلاین

---

### 2. OnlinePayments (پرداخت‌های آنلاین)

**جدول:** `OnlinePayments`  
**Entity:** `OnlinePayment`

#### فیلدهای کلیدی:
```sql
OnlinePaymentId (PK, int, Identity)
PaymentGatewayId (FK, int, Required) - ⚠️ ارجاع به PaymentGateways
ReceptionId (FK, int, Optional) - در صورت پرداخت پذیرش
AppointmentId (FK, int, Optional) - ⚠️ در صورت پرداخت نوبت
PatientId (FK, int, Required) - بیمار
PaymentType (int, Required) - نوع پرداخت (Appointment, Reception, ...)
Status (int, Required) - وضعیت (Pending, Processing, Success, Failed, ...)
Amount (decimal(18,0), Required) - مبلغ پرداخت (ریال)
GatewayFee (decimal(18,0), Optional) - کارمزد درگاه
NetAmount (decimal(18,0), Optional) - مبلغ خالص
GatewayTransactionId (nvarchar(100), Optional) - شماره تراکنش درگاه
GatewayReferenceCode (nvarchar(100), Optional) - شماره مرجع (RRN)
InternalTransactionId (nvarchar(100), Optional) - شماره تراکنش داخلی
PaymentToken (nvarchar(500), Optional) - ⚠️ توکن پرداخت (Authority در ZarinPal)
PaymentUrl (nvarchar(1000), Optional) - URL پرداخت
PaymentStartDate (datetime, Optional) - تاریخ شروع پرداخت
PaymentCompletionDate (datetime, Optional) - تاریخ تکمیل پرداخت
PaymentExpiryDate (datetime, Optional) - تاریخ انقضای پرداخت
UserIpAddress (nvarchar(50), Optional)
UserAgent (nvarchar(500), Optional)
ErrorCode (nvarchar(50), Optional)
ErrorMessage (nvarchar(1000), Optional)
Description (nvarchar(1000), Optional)
AdditionalData (nvarchar(2000), Optional) - JSON
IsRefunded (bit, Required) - آیا برگشت خورده؟
RefundDate (datetime, Optional)
RefundAmount (decimal(18,0), Optional)
RefundReason (nvarchar(500), Optional)
IsDeleted (bit, Required) - Soft Delete
RowVersion (timestamp) - ⚠️ Optimistic Concurrency Control
CreatedAt (datetime, Required)
CreatedByUserId (nvarchar(128), Optional)
UpdatedAt (datetime, Optional)
UpdatedByUserId (nvarchar(128), Optional)
```

#### Index ها:
- `IX_OnlinePayment_PaymentGatewayId`
- `IX_OnlinePayment_ReceptionId`
- `IX_OnlinePayment_AppointmentId`
- `IX_OnlinePayment_PatientId`
- `IX_OnlinePayment_PaymentType`
- `IX_OnlinePayment_Status`
- `IX_OnlinePayment_Amount`
- `IX_OnlinePayment_GatewayTransactionId`
- `IX_OnlinePayment_GatewayReferenceCode`
- `IX_OnlinePayment_InternalTransactionId`
- `IX_OnlinePayment_PaymentToken` - ⚠️ برای جستجوی Callback
- `IX_OnlinePayment_PaymentStartDate`
- `IX_OnlinePayment_PaymentCompletionDate`
- `IX_OnlinePayment_PaymentExpiryDate`
- `IX_OnlinePayment_IsRefunded`
- `IX_OnlinePayment_RefundDate`
- `IX_OnlinePayment_GatewayId_Status` (Composite)
- `IX_OnlinePayment_PatientId_PaymentType` (Composite)
- `IX_OnlinePayment_Status_CreatedAt` (Composite)
- `IX_OnlinePayment_Type_Status_CreatedAt` (Composite)

#### روابط:
- `PaymentGateway` (Many-to-One, Required) - درگاه پرداخت
- `Reception` (Many-to-One, Optional) - پذیرش
- `Appointment` (Many-to-One, Optional) - ⚠️ نوبت
- `Patient` (Many-to-One, Required) - بیمار

---

### 3. PaymentTransactions (تراکنش‌های مالی)

**جدول:** `PaymentTransactions`  
**Entity:** `PaymentTransaction`

#### فیلدهای کلیدی:
```sql
PaymentTransactionId (PK, int, Identity)
ReceptionId (FK, int, Required) - پذیرش
PosTerminalId (FK, int, Optional) - دستگاه پوز
PaymentGatewayId (FK, int, Optional) - ⚠️ درگاه پرداخت (برای پرداخت آنلاین)
OnlinePaymentId (FK, int, Optional) - ⚠️ پرداخت آنلاین
CashSessionId (FK, int, Required) - شیفت صندوق
Amount (decimal(18,0), Required) - مبلغ
Status (int, Required) - وضعیت
Method (int, Required) - روش پرداخت (Cash, POS, Online)
TransactionId (nvarchar(100), Optional) - شماره تراکنش بانکی
ReferenceCode (nvarchar(100), Optional) - RRN
ReceiptNo (nvarchar(50), Optional) - شماره قبض
Description (nvarchar(500), Optional)
IsDeleted (bit, Required) - Soft Delete
CreatedAt (datetime, Required)
CreatedByUserId (nvarchar(128), Optional)
UpdatedAt (datetime, Optional)
UpdatedByUserId (nvarchar(128), Optional)
```

#### روابط:
- `Reception` (Many-to-One, Required)
- `PosTerminal` (Many-to-One, Optional)
- `PaymentGateway` (Many-to-One, Optional) - ⚠️ درگاه پرداخت
- `OnlinePayment` (Many-to-One, Optional) - ⚠️ پرداخت آنلاین
- `CashSession` (Many-to-One, Required)

---

## 🔗 روابط بین جداول (Relationships)

### نمودار روابط:

```
PaymentGateways (1) ──< (Many) OnlinePayments
                              │
                              ├──> (Optional) Appointments
                              ├──> (Optional) Receptions
                              └──> (Required) Patients

OnlinePayments (1) ──< (Optional) PaymentTransactions
                              │
                              └──> (Required) PaymentGateways
```

### جزئیات روابط:

#### 1. PaymentGateways → OnlinePayments
- **نوع:** One-to-Many
- **Foreign Key:** `OnlinePayments.PaymentGatewayId`
- **Cascade Delete:** ❌ No (WillCascadeOnDelete = false)
- **Required:** ✅ Yes (HasRequired)

#### 2. OnlinePayments → Appointments
- **نوع:** Many-to-One (Optional)
- **Foreign Key:** `OnlinePayments.AppointmentId`
- **Cascade Delete:** ❌ No
- **Required:** ❌ No (HasOptional)

#### 3. OnlinePayments → Receptions
- **نوع:** Many-to-One (Optional)
- **Foreign Key:** `OnlinePayments.ReceptionId`
- **Cascade Delete:** ❌ No
- **Required:** ❌ No (HasOptional)

#### 4. OnlinePayments → Patients
- **نوع:** Many-to-One (Required)
- **Foreign Key:** `OnlinePayments.PatientId`
- **Cascade Delete:** ❌ No
- **Required:** ✅ Yes (HasRequired)

#### 5. PaymentTransactions → OnlinePayments
- **نوع:** Many-to-One (Optional)
- **Foreign Key:** `PaymentTransactions.OnlinePaymentId`
- **Cascade Delete:** ❌ No
- **Required:** ❌ No (HasOptional)

#### 6. PaymentTransactions → PaymentGateways
- **نوع:** Many-to-One (Optional)
- **Foreign Key:** `PaymentTransactions.PaymentGatewayId`
- **Cascade Delete:** ❌ No
- **Required:** ❌ No (HasOptional)

---

## 🔄 Flow کامل پرداخت وب (Complete Payment Flow)

### مرحله 1: ایجاد درخواست پرداخت (ProcessPayment)

```
1. User clicks "تائید و پرداخت"
   ↓
2. Reserve action: نوبت با Status = Pending ایجاد می‌شود
   ↓
3. ProcessPayment action:
   a. دریافت درگاه پیش‌فرض (GetDefaultPaymentGatewayAsync)
      - جستجوی درگاه با IsDefault = true
      - اگر یافت نشد: جستجوی ZarinPal فعال
      - اگر یافت نشد: جستجوی اولین درگاه فعال
      - اگر یافت نشد: ایجاد خودکار از Web.config
   b. ایجاد OnlinePayment با Status = Pending
   c. ایجاد درخواست پرداخت در درگاه (CreatePaymentRequestAsync)
   d. به‌روزرسانی OnlinePayment با PaymentToken و PaymentUrl
   e. برگرداندن PaymentUrl به Frontend
   ↓
4. Frontend: Redirect به PaymentUrl (درگاه پرداخت)
```

### مرحله 2: Callback از درگاه (PaymentCallback)

```
1. درگاه پرداخت: Redirect به CallbackUrl
   ↓
2. PaymentCallback action:
   a. دریافت OnlinePayment بر اساس PaymentToken (Authority)
   b. پردازش Callback (ProcessPaymentCallbackAsync)
   c. Verify پرداخت در درگاه
   d. به‌روزرسانی OnlinePayment:
      - Status = Success (اگر موفق)
      - GatewayTransactionId = RefId
      - PaymentCompletionDate = Now
   e. به‌روزرسانی Appointment:
      - Status = Scheduled (اگر موفق)
   f. ایجاد PaymentTransaction (اختیاری)
   g. Redirect به PaymentSuccess یا PaymentError
```

---

## 🔍 تحلیل مشکل فعلی

### مشکل: "درگاه پرداخت پیش‌فرض یافت نشد"

#### سناریو 1: هیچ درگاهی در دیتابیس وجود ندارد
- `GetDefaultGatewaysAsync()` → خالی
- `GetByTypeAsync(ZarinPal)` → خالی
- `GetActiveGatewaysAsync()` → خالی
- **راه‌حل:** ایجاد خودکار از Web.config

#### سناریو 2: درگاه موجود است اما CallbackUrl خالی است
- درگاه پیدا می‌شود
- اما `CallbackUrl = ""` یا `null`
- **Validation Error:** "URL بازگشت الزامی است"
- **راه‌حل:** به‌روزرسانی `CallbackUrl` در درگاه موجود

#### سناریو 3: درگاه موجود است اما غیرفعال است
- `IsActive = false`
- **راه‌حل:** فعال کردن درگاه

#### سناریو 4: درگاه موجود است اما IsDefault = false
- `IsDefault = false`
- **راه‌حل:** تنظیم `IsDefault = true`

---

## ✅ راه‌حل‌های اعمال شده

### 1. تنظیم CallbackUrl هنگام ایجاد درگاه خودکار
```csharp
var defaultCallbackUrl = "/Patient/AppointmentBooking/PaymentCallback";
var newGateway = new PaymentGateway
{
    // ...
    CallbackUrl = defaultCallbackUrl, // ✅ تنظیم شده
    // ...
};
```

### 2. به‌روزرسانی درگاه‌های موجود
```csharp
if (string.IsNullOrWhiteSpace(existingGateway.CallbackUrl))
{
    existingGateway.CallbackUrl = defaultCallbackUrl;
    needsUpdate = true;
}

if (!existingGateway.IsActive)
{
    existingGateway.IsActive = true;
    needsUpdate = true;
}

if (needsUpdate)
{
    await _paymentGatewayRepository.UpdateAsync(existingGateway);
}
```

### 3. بهبود لاگ‌ها
```csharp
if (ex is DbEntityValidationException validationEx)
{
    foreach (var validationError in validationEx.EntityValidationErrors)
    {
        foreach (var error in validationError.ValidationErrors)
        {
            _logger.Error("Validation Error - Property: {Property}, Error: {Error}",
                error.PropertyName, error.ErrorMessage);
        }
    }
}
```

---

## 📋 چک‌لیست بررسی

### بررسی دیتابیس:

```sql
-- 1. بررسی وجود درگاه‌ها
SELECT * FROM PaymentGateways WHERE IsDeleted = 0;

-- 2. بررسی درگاه‌های پیش‌فرض
SELECT * FROM PaymentGateways 
WHERE IsDeleted = 0 AND IsDefault = 1;

-- 3. بررسی درگاه‌های فعال
SELECT * FROM PaymentGateways 
WHERE IsDeleted = 0 AND IsActive = 1;

-- 4. بررسی درگاه‌های ZarinPal
SELECT * FROM PaymentGateways 
WHERE IsDeleted = 0 AND GatewayType = 1; -- ZarinPal = 1

-- 5. بررسی درگاه‌های با CallbackUrl خالی
SELECT * FROM PaymentGateways 
WHERE IsDeleted = 0 AND (CallbackUrl IS NULL OR CallbackUrl = '');

-- 6. بررسی OnlinePayments مرتبط
SELECT op.*, pg.Name as GatewayName, pg.CallbackUrl
FROM OnlinePayments op
LEFT JOIN PaymentGateways pg ON op.PaymentGatewayId = pg.PaymentGatewayId
WHERE op.IsDeleted = 0
ORDER BY op.CreatedAt DESC;
```

---

## 🎯 نکات مهم

### 1. CallbackUrl الزامی است
- در Entity `PaymentGateway`: `[Required]`
- در Database: `NOT NULL`
- **مشکل:** اگر خالی باشد، Validation خطا می‌دهد

### 2. PaymentToken برای Callback
- `PaymentToken` = `Authority` در ZarinPal
- برای جستجوی `OnlinePayment` در Callback استفاده می‌شود
- Index شده: `IX_OnlinePayment_PaymentToken`

### 3. Status Flow
```
Pending → Processing → Success/Failed
```

### 4. Appointment Status Flow
```
Pending (بعد از Reserve) → Scheduled (بعد از پرداخت موفق)
```

---

## 🔧 دستورات SQL برای رفع مشکل

### اگر درگاه موجود است اما CallbackUrl خالی است:

```sql
-- به‌روزرسانی CallbackUrl
UPDATE PaymentGateways
SET CallbackUrl = '/Patient/AppointmentBooking/PaymentCallback',
    UpdatedAt = GETUTCDATE()
WHERE IsDeleted = 0 
  AND (CallbackUrl IS NULL OR CallbackUrl = '');
```

### اگر درگاه موجود است اما غیرفعال است:

```sql
-- فعال کردن درگاه
UPDATE PaymentGateways
SET IsActive = 1,
    UpdatedAt = GETUTCDATE()
WHERE IsDeleted = 0 
  AND IsActive = 0
  AND GatewayType = 1; -- ZarinPal
```

### اگر درگاه موجود است اما IsDefault = false:

```sql
-- تنظیم به عنوان پیش‌فرض
-- اول: پاک کردن تمام درگاه‌های پیش‌فرض
UPDATE PaymentGateways
SET IsDefault = 0
WHERE IsDeleted = 0 AND IsDefault = 1;

-- دوم: تنظیم درگاه ZarinPal به عنوان پیش‌فرض
UPDATE PaymentGateways
SET IsDefault = 1,
    UpdatedAt = GETUTCDATE()
WHERE IsDeleted = 0 
  AND GatewayType = 1 -- ZarinPal
  AND MerchantId = '156be6cd-e0a4-4af8-9113-83647771376f'; -- از Web.config
```

---

## 📊 خلاصه جداول

| جدول | Entity | Primary Key | Foreign Keys | Index ها |
|------|--------|-------------|--------------|----------|
| PaymentGateways | PaymentGateway | PaymentGatewayId | - | 5 |
| OnlinePayments | OnlinePayment | OnlinePaymentId | PaymentGatewayId, AppointmentId, ReceptionId, PatientId | 18 |
| PaymentTransactions | PaymentTransaction | PaymentTransactionId | ReceptionId, PosTerminalId, PaymentGatewayId, OnlinePaymentId, CashSessionId | - |

---

## ✅ نتیجه‌گیری

### مشکلات شناسایی شده:
1. ✅ `CallbackUrl` خالی در درگاه‌های موجود → **رفع شد**
2. ✅ درگاه‌های غیرفعال → **رفع شد**
3. ✅ درگاه‌های بدون `IsDefault` → **رفع شد**
4. ✅ Validation Error برای `CallbackUrl` → **رفع شد**

### راه‌حل‌های اعمال شده:
1. ✅ تنظیم `CallbackUrl` هنگام ایجاد درگاه خودکار
2. ✅ به‌روزرسانی درگاه‌های موجود با `CallbackUrl` خالی
3. ✅ فعال کردن درگاه‌های غیرفعال
4. ✅ بهبود لاگ‌ها برای debugging

---

**وضعیت:** ✅ آماده برای تست

