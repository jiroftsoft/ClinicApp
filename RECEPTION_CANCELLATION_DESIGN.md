# 🚫 طراحی قابلیت لغو پذیرش (Reception Cancellation)

## 📋 Executive Summary

**هدف**: پیاده‌سازی قابلیت لغو پذیرش به صورت حرفه‌ای و امن با مدیریت کامل پرداخت‌ها و لاگ‌ها

**سناریوهای استفاده**:
1. بیمار منصرف شده است
2. بیمار پول پرداخت نکرده است
3. خطای ثبت پذیرش
4. نیاز به اصلاح پذیرش (لغو و ایجاد مجدد)

---

## 🎯 قوانین کسب‌وکار

### 1. چه پذیرش‌هایی قابل لغو هستند؟

#### ✅ قابل لغو:
- **Pending (در انتظار)**: همیشه قابل لغو
- **Completed (تکمیل شده)**: فقط در صورت وجود شرایط خاص:
  - کمتر از 24 ساعت از زمان نهایی‌سازی گذشته باشد
  - یا با تایید مدیر

#### ❌ غیرقابل لغو:
- **Cancelled (لغو شده)**: قبلاً لغو شده
- پذیرش‌هایی که بیش از 7 روز از زمان ایجاد گذشته باشند (مگر با تایید مدیر)
- پذیرش‌هایی که به بیمه ارسال شده‌اند (نیاز به بررسی)

### 2. مدیریت پرداخت‌ها

#### اگر پرداختی انجام نشده:
- ✅ فقط تغییر وضعیت به Cancelled
- ✅ ثبت دلیل لغو

#### اگر پرداختی انجام شده:
- ⚠️ نیاز به Refund (برگشت وجه)
- ⚠️ لغو تراکنش‌های POS (در صورت امکان)
- ⚠️ ثبت تراکنش Refund
- ⚠️ اطلاع‌رسانی به بیمار

### 3. محدودیت‌های زمانی

- **Pending**: بدون محدودیت زمانی
- **Completed**: 
  - کمتر از 24 ساعت: قابل لغو توسط منشی
  - بیشتر از 24 ساعت: نیاز به تایید مدیر
  - بیشتر از 7 روز: نیاز به تایید مدیر ارشد

### 4. لاگ و Audit Trail

- ✅ ثبت دلیل لغو (الزامی)
- ✅ ثبت کاربر لغوکننده
- ✅ ثبت تاریخ و زمان لغو
- ✅ ثبت وضعیت پرداخت‌ها قبل از لغو
- ✅ ثبت مبلغ‌های Refund (در صورت وجود)

---

## 🏗️ معماری

### 1. Flow Diagram

```
User clicks "Cancel" 
    ↓
Check Reception Status
    ↓
Check Business Rules (Time limits, Permissions)
    ↓
Show Cancellation Modal (Reason required)
    ↓
Check if Payment exists
    ↓
If Payment exists:
    ├─ Show Refund Warning
    ├─ User confirms Refund
    └─ Process Refund
    ↓
Update Reception Status to Cancelled
    ↓
Log Cancellation
    ↓
Return Success
```

### 2. DTOs

```csharp
public class CancelReceptionRequest
{
    public int ReceptionId { get; set; }
    public string Reason { get; set; } // الزامی
    public bool ProcessRefund { get; set; } // اگر پرداختی وجود دارد
    public string RefundReason { get; set; } // برای Refund
}

public class CancelReceptionResponse
{
    public int ReceptionId { get; set; }
    public ReceptionStatus PreviousStatus { get; set; }
    public ReceptionStatus NewStatus { get; set; }
    public bool RefundProcessed { get; set; }
    public decimal? RefundAmount { get; set; }
    public string Message { get; set; }
    public bool RequiresApproval { get; set; }
}
```

### 3. Business Rules Engine

```csharp
public class CancellationRules
{
    // بررسی امکان لغو
    bool CanCancel(Reception reception, out string errorMessage);
    
    // بررسی نیاز به تایید
    bool RequiresApproval(Reception reception);
    
    // بررسی وجود پرداخت
    bool HasPayment(Reception reception);
    
    // بررسی محدودیت زمانی
    bool IsWithinTimeLimit(Reception reception);
}
```

---

## 🔒 امنیت و اعتبارسنجی

### 1. Authorization
- ✅ بررسی نقش کاربر (منشی/مدیر)
- ✅ بررسی مجوز لغو
- ✅ بررسی محدودیت‌های زمانی

### 2. Validation
- ✅ دلیل لغو الزامی است (حداقل 10 کاراکتر)
- ✅ بررسی وجود پذیرش
- ✅ بررسی وضعیت فعلی پذیرش

### 3. Audit Trail
- ✅ ثبت کامل لاگ‌ها
- ✅ ذخیره دلیل لغو
- ✅ ذخیره اطلاعات کاربر
- ✅ ذخیره تاریخ و زمان

---

## 💰 مدیریت Refund

### 1. بررسی پرداخت‌ها
- بررسی PaymentTransactions مرتبط با Reception
- بررسی مبلغ پرداخت شده
- بررسی روش پرداخت (POS/Cash)

### 2. فرآیند Refund
- **POS**: لغو تراکنش از طریق درگاه
- **Cash**: ثبت تراکنش Refund
- ثبت تراکنش Refund در PaymentTransactions
- به‌روزرسانی PaidAmount در Reception

### 3. اطلاع‌رسانی
- نمایش پیام به کاربر
- ثبت در لاگ
- (اختیاری) ارسال SMS به بیمار

---

## 📊 UI/UX Design

### 1. دکمه لغو
- در لیست پذیرش‌ها: فقط برای Pending و Completed
- در فرم ویرایش: فقط برای Pending
- رنگ: قرمز (btn-danger)
- آیکون: fa-ban یا fa-times-circle

### 2. مودال لغو
- نمایش اطلاعات پذیرش
- فیلد الزامی برای دلیل لغو
- هشدار در صورت وجود پرداخت
- دکمه‌های "لغو" و "انصراف"

### 3. تایید
- تایید دو مرحله‌ای برای پذیرش‌های Completed
- نمایش هشدار برای Refund
- نمایش محدودیت‌های زمانی

---

## 🚀 مراحل پیاده‌سازی

### Phase 1: Backend
1. ✅ افزودن DTOs
2. ✅ پیاده‌سازی CancellationRules
3. ✅ پیاده‌سازی CancelReceptionAsync در Facade
4. ✅ مدیریت Refund

### Phase 2: API
1. ✅ افزودن API endpoint
2. ✅ اعتبارسنجی
3. ✅ Error handling

### Phase 3: Frontend
1. ✅ افزودن دکمه لغو
2. ✅ ایجاد مودال لغو
3. ✅ JavaScript handlers
4. ✅ نمایش هشدارها

### Phase 4: Testing
1. ✅ تست سناریوهای مختلف
2. ✅ تست Refund
3. ✅ تست محدودیت‌های زمانی

---

**تاریخ ایجاد**: 2025-01-17  
**نسخه**: 1.0  
**وضعیت**: طراحی اولیه

