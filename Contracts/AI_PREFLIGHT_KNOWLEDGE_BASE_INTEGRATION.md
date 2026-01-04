# 📚 یکپارچه‌سازی Knowledge-Base با AI Preflight Protocol

**نسخه:** 1.0.0  
**تاریخ:** 2026-01-02  
**وضعیت:** ✅ **الزامی**

---

## 🎯 هدف

این سند نحوه یکپارچه‌سازی **Knowledge-Base** با **AI Preflight Protocol** را مشخص می‌کند.

---

## 📋 چک‌لیست قبل از هر پاسخ

### ✅ **مرحله 0: Knowledge-Base Check (اگر نیاز به Helper/Standard)**

```
□ آیا نیاز به Helper دارم؟
   → Contracts/Knowledge-Base/AI/Master/INDEX.md را بررسی کن
   → Contracts/Knowledge-Base/AI/Master/README.md را بخوان

□ آیا نیاز به DatePicker دارم؟
   → Contracts/Knowledge-Base/AI/Master/01-Helpers-DateTime.md را بخوان
   → استفاده از @Html.Partial("_PersianDatePicker")

□ آیا نیاز به Validation دارم؟
   → Contracts/Knowledge-Base/AI/Master/02-Helpers-Validation.md را بخوان

□ آیا نیاز به استانداردهای توسعه دارم؟
   → Contracts/Knowledge-Base/AI/Master/03-Development-Contract-Quick-Guide.md را بخوان

□ آیا باگ دارم؟
   → Contracts/Knowledge-Base/AI/PreFlight/05-Debugging-Specialist-Contract.md را بخوان

□ آیا ماژول مالی است؟
   → Contracts/Knowledge-Base/CRITICAL-FINANCIAL-MODULE-CONTRACT.md را بخوان
```

---

## 📚 فهرست Knowledge-Base

### **راهنماهای اصلی:**
1. **README.md** - راهنمای کلی و نحوه استفاده
2. **INDEX.md** - فهرست کامل تمام Helpers و راهنماها

### **Helpers:**
3. **01-Helpers-DateTime.md** - تاریخ و زمان (6 Helper)
   - `PersianDateHelper.ToPersianDate()`
   - `@Html.Partial("_PersianDatePicker")`
   - `this.ParseDateFromHiddenInput()`

4. **02-Helpers-Validation.md** - اعتبارسنجی (6 Helper)
   - `IranianNationalCodeValidator.IsValid()`
   - `PhoneNumberValidator.IsValidMobile()`

### **قراردادها:**
5. **03-Development-Contract-Quick-Guide.md** - قرارداد توسعه
   - رنگ‌بندی استاندارد
   - Strongly-Typed Development
   - Bulletproof Coding
   - SRP Architecture

6. **04-TODO-Implementation-Guide.md** - راهنمای TODO
   - 13 Phase پیاده‌سازی
   - Template TODO

7. **05-Debugging-Specialist-Contract.md** - دیباگ
   - فرآیند 6 مرحله‌ای
   - 5 Whys

### **جعبه ابزار:**
8. **HelperExtensionsGuide.md** - 14 Helper/Extension + 100+ متد

9. **06-Quick-Reference.md** - مرجع سریع

---

## 🎯 Workflow یکپارچه

### **قبل از شروع کار:**

```
1. Contracts/AI_EXECUTION_CONTRACT.md (10s)
2. Contracts/AI_PREFLIGHT_QUICK_V3.md (30s)
3. Knowledge-Base Check (اگر نیاز):
   - README.md → INDEX.md → Helper مورد نیاز
4. نوع کار را شناسایی کن:
   - معمولی → Quick
   - مالی → STEP 2
   - باگ → STEP 3
5. Implement با رعایت:
   - Helpers از Knowledge-Base
   - Standards از Development Contract
   - Security از Preflight
```

---

## ✅ مثال‌های کاربردی

### **مثال 1: استفاده از DatePicker**

```razor
@* ✅ طبق Contracts/Knowledge-Base/AI/Master/01-Helpers-DateTime.md *@
@{
    ViewBag.PersianDatePickerId = "BirthDate";
    ViewBag.PersianDatePickerName = "BirthDate";
    ViewBag.PersianDatePickerValue = Model?.BirthDate;
    ViewBag.PersianDatePickerLabel = "تاریخ تولد";
    ViewBag.PersianDatePickerPlaceholder = "1370/01/01";
    ViewBag.PersianDatePickerRequired = false;
}
@Html.Partial("~/Areas/Admin/Views/Shared/_PersianDatePicker.cshtml")
```

### **مثال 2: Parse تاریخ در Controller**

```csharp
// ✅ طبق Contracts/Knowledge-Base/AI/Master/01-Helpers-DateTime.md
model.BirthDate = this.ParseDateFromHiddenInput("BirthDate", _logger);
```

### **مثال 3: Validation**

```csharp
// ✅ طبق Contracts/Knowledge-Base/AI/Master/02-Helpers-Validation.md
if (!IranianNationalCodeValidator.IsValid(model.NationalCode))
{
    ModelState.AddModelError("NationalCode", "کد ملی نامعتبر است");
}
```

---

## 🚨 HARD STOP

اگر:
- Helper در Knowledge-Base وجود دارد اما استفاده نشده
- Standard در Development Contract وجود دارد اما رعایت نشده
- قرارداد Knowledge-Base نقض شده

→ **STOP و اطلاع به کاربر**

---

## 📁 مراجع

- `Contracts/Knowledge-Base/AI/Master/README.md`
- `Contracts/Knowledge-Base/AI/Master/INDEX.md`
- `Contracts/AI_EXECUTION_CONTRACT.md`
- `Contracts/AI_PREFLIGHT_MASTER_V3.md`

---

**نسخه:** 1.0.0  
**وضعیت:** ✅ **الزامی**

