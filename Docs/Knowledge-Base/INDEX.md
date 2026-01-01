# 📚 فهرست کامل پایگاه دانش ClinicApp

**تاریخ:** 1404/10/05  
**وضعیت:** ✅ **فعال و به‌روز**

---

## 🎯 دسترسی سریع

| شماره | عنوان | فایل | محتوا |
|-------|--------|------|-------|
| 00 | **راهنمای اصلی** | [README.md](README.md) | راهنمای کلی و نحوه استفاده |
| 01 | **تاریخ و زمان** | [01-Helpers-DateTime.md](01-Helpers-DateTime.md) | 6 Helper + مثال‌ها |
| 02 | **اعتبارسنجی** | [02-Helpers-Validation.md](02-Helpers-Validation.md) | 6 Helper + مثال‌ها |
| 03 | **قرارداد توسعه** | [03-Development-Contract-Quick-Guide.md](03-Development-Contract-Quick-Guide.md) | خلاصه قرارداد توسعه |
| 04 | **راهنمای TODO** | [04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md) | راهنمای سریع پیاده‌سازی |
| 05 | **متخصص دیباگر** 🔧 | [05-Debugging-Specialist-Contract.md](05-Debugging-Specialist-Contract.md) | قرارداد دیباگ ارشد |
| 06 | **Quick Reference** | [06-Quick-Reference.md](06-Quick-Reference.md) | جدول سریع تمام Helpers |
| 07 | **جعبه ابزار (Toolbox)** 🧰 | [HelperExtensionsGuide.md](HelperExtensionsGuide.md) | 14 Helper/Extension + 100+ متد |
| 08 | **🚨 ماژول‌های مالی** 💰 | [CRITICAL-FINANCIAL-MODULE-CONTRACT.md](CRITICAL-FINANCIAL-MODULE-CONTRACT.md) | قرارداد Critical برای صندوق، پرداخت، گزارش‌ها |
| 09 | **🔒 احراز هویت Patient** | [PATIENT_AREA_AUTH_GUIDE.md](PATIENT_AREA_AUTH_GUIDE.md) | راهنمای یکپارچه‌سازی احراز هویت Patient Area |
| -- | **فهرست این صفحه** | [INDEX.md](INDEX.md) | شما اینجا هستید |

---

## 📖 راهنماهای کامل (با مثال)

### ✅ **ایجاد شده:**

1. **[README.md](README.md)** - راهنمای اصلی
   - هدف پایگاه دانش
   - نحوه استفاده
   - فهرست کامل
   - آمار

2. **[01-Helpers-DateTime.md](01-Helpers-DateTime.md)** - تاریخ و زمان
   - `PersianDateHelper.cs` - تبدیل میلادی ↔ شمسی
   - `PersianDatePickerHelper.cs` - DatePicker
   - `DateTimeExtensions.cs` - Extension Methods
   - `PersianDateExtensions.cs` - Extension پیشرفته
   - `TimeFormatHelper.cs` - فرمت زمان
   - `AgeCalculationHelper.cs` - محاسبه سن
   - `ControllerExtensions.ParseDateFromHiddenInput` - Parse در Controller

3. **[02-Helpers-Validation.md](02-Helpers-Validation.md)** - اعتبارسنجی
   - `IranianNationalCodeValidator.cs` - کد ملی
   - `PhoneNumberValidator.cs` - موبایل و تلفن
   - `PhoneNumberHelper.cs` - نرمال‌سازی شماره
   - `IdentityValidators.cs` - Identity
   - `ValidationResult.cs` - نتیجه Validation
   - `SecurityValidationResult.cs` - Validation امنیتی

4. **[03-Development-Contract-Quick-Guide.md](03-Development-Contract-Quick-Guide.md)** - قرارداد توسعه
   - اصول اساسی (Non-Negotiable)
   - پالت رنگ استاندارد
   - Strongly-Typed Development
   - Bulletproof Coding
   - معماری SRP
   - سیستم پیام‌ها و هشدارها
   - تقویم شمسی (Persian DatePicker)
   - سیستم آپلود تصویر
   - CKEditor (ویرایشگر متن)
   - فرم‌های درمانی (Medical Forms)
   - Checklist نهایی قبل از Commit

5. **[04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md)** - راهنمای TODO
   - Quick Start Checklist
   - Phase-by-Phase Implementation (13 مرحله)
   - زمان‌بندی کلی
   - Checklist نهایی قبل از Commit
   - Template TODO List (آماده کپی)
   - نکات مهم قبل، حین و بعد از کار

6. **[05-Debugging-Specialist-Contract.md](05-Debugging-Specialist-Contract.md)** 🔧 - متخصص دیباگر ارشد
   - فرآیند استاندارد دیباگ (6 مرحله)
   - تحلیل علت ریشه‌ای (5 Whys)
   - رفع اتمیک (Atomic Fix)
   - ابزارهای دیباگ
   - نمونه‌های کاربردی (Use Cases)
   - چک‌لیست کامل دیباگر
   - سطوح دیباگ (Level 1-4)
   - الگوهای رایج خطا
   - نکات طلایی
   - **قانون اصلی: ممنوع رفع کورکورانه!**

6. **[06-Quick-Reference.md](06-Quick-Reference.md)** - مرجع سریع
   - جدول کامل تمام Helpers/Extensions
   - Use Case → Helper
   - مثال‌های یک خطی
   - دستور یادگیری سریع

7. **[HelperExtensionsGuide.md](HelperExtensionsGuide.md)** 🧰 - جعبه ابزار کامل
   - **5 Extensions:** StringExtensions, DateTimeExtensions, NumericExtensions, CollectionExtensions, ObjectExtensions
   - **8 Helpers:** CacheHelper, RetryHelper, SecurityHelper, ValidationHelper, FileHelper, HtmlHelper, UrlHelper, ImageHelper
   - **100+ متد** کاربردی برای استفاده در Controller/Service/View
   - مثال‌های کاربردی برای هر متد
   - رعایت SRP + XML Documentation + Null Safety

---

## 🔍 جستجو بر اساس موضوع

### **تاریخ و زمان:**
```
نیاز: تبدیل میلادی به شمسی
→ فایل: 01-Helpers-DateTime.md
→ Helper: PersianDateHelper.ToPersianDate()
→ مثال: PersianDateHelper.ToPersianDate(DateTime.Now)
→ خروجی: "1404/10/05"
```

### **اعتبارسنجی:**
```
نیاز: بررسی کد ملی
→ فایل: 02-Helpers-Validation.md
→ Helper: IranianNationalCodeValidator.IsValid()
→ مثال: IranianNationalCodeValidator.IsValid("0123456789")
→ خروجی: true/false
```

### **قرارداد توسعه:**
```
نیاز: مطالعه استانداردهای توسعه
→ فایل: 03-Development-Contract-Quick-Guide.md
→ موضوعات: رنگ‌بندی، Strongly-Typed، SRP، Bulletproof
→ Checklist: قبل از هر Commit
```

### **راهنمای TODO:**
```
نیاز: پیاده‌سازی یک ماژول جدید
→ فایل: 04-TODO-Implementation-Guide.md
→ محتوا: 13 Phase + Checklist + Template
→ زمان: 12-17 روز کاری
```

### **متخصص دیباگر:** 🔧
```
نیاز: رفع یک خطا یا باگ
→ فایل: 05-Debugging-Specialist-Contract.md
→ فرآیند: شناسایی → تحلیل → رفع → تست → گزارش
→ قانون طلایی: ممنوع رفع کورکورانه!
→ مراحل: 6 مرحله الزامی
```

### **Notification:**
```
نیاز: نمایش پیام موفقیت
→ فایل: 06-Quick-Reference.md
→ Helper (Backend): NotificationHelper.SetSuccess(TempData, "...")
→ Helper (Frontend Admin): AdminNotification.success("...")
→ Helper (Frontend Public): Notify.success("...")
```

---

## 📊 آمار کامل

### **Helpers بر اساس دسته:**

| دسته | تعداد | فایل مستند |
|------|-------|-----------|
| **تاریخ و زمان** | 6 | [01-Helpers-DateTime.md](01-Helpers-DateTime.md) |
| **اعتبارسنجی** | 6 | [02-Helpers-Validation.md](02-Helpers-Validation.md) |
| **قرارداد توسعه** | - | [03-Development-Contract-Quick-Guide.md](03-Development-Contract-Quick-Guide.md) |
| **راهنمای TODO** | - | [04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md) |
| **متخصص دیباگر** 🔧 | - | [05-Debugging-Specialist-Contract.md](05-Debugging-Specialist-Contract.md) |
| **امنیت** | 5 | Quick Ref |
| **String و عمومی** | 30+ | Quick Ref |
| **Extensions** | 6 | Quick Ref |
| **جمع** | **50+** | - |

---

## 🎓 مسیر یادگیری پیشنهادی

### **مرحله 1: شروع (روز 1-2)**
1. ✅ [README.md](README.md) را بخوان
2. ✅ [03-Development-Contract-Quick-Guide.md](03-Development-Contract-Quick-Guide.md) را مطالعه کن (الزامی!)
3. ✅ [01-Helpers-DateTime.md](01-Helpers-DateTime.md) را مطالعه کن
4. ✅ مثال‌های تاریخ را امتحان کن

### **مرحله 2: Validation (روز 3-4)**
1. ✅ [02-Helpers-Validation.md](02-Helpers-Validation.md) را بخوان
2. ✅ کد ملی و موبایل را تست کن
3. ✅ در یک فرم واقعی استفاده کن

### **مرحله 3: سایر Helpers (روز 5-7)**
1. ✅ [06-Quick-Reference.md](06-Quick-Reference.md) را مرور کن
2. ✅ Helpers مورد نیاز را پیدا کن
3. ✅ در کد خودت استفاده کن

### **مرحله 4: پیاده‌سازی ماژول جدید (روز 8+)**
1. ✅ [04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md) را بخوان
2. ✅ Template TODO را کپی کن
3. ✅ Phase به Phase پیش برو
4. ✅ Checklist نهایی را قبل از Commit بررسی کن

### **مرحله 5: تسلط (روز 15+)**
1. ✅ Use Case های واقعی را ببین
2. ✅ ترکیب Helper ها را یاد بگیر
3. ✅ Best Practices را رعایت کن
4. ✅ همیشه قبل از کار، قرارداد توسعه را مرور کن

---

## 🔥 مهم‌ترین راهنماها (باید بلد باشی!)

### **⚡ الزامی قبل از هر کاری:**

1. **[03-Development-Contract-Quick-Guide.md](03-Development-Contract-Quick-Guide.md)** - قرارداد توسعه
   ```
   ✅ رنگ‌بندی استاندارد
   ✅ Strongly-Typed Development
   ✅ Bulletproof Coding
   ✅ SRP Architecture
   ✅ Notification System
   ✅ Persian DatePicker
   ✅ Image Upload
   ✅ CKEditor
   ✅ Medical Forms
   ✅ Checklist نهایی
   ```

2. **[04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md)** - راهنمای TODO
   ```
   ✅ 13 Phase پیاده‌سازی
   ✅ Checklist هر Phase
   ✅ زمان‌بندی (12-17 روز)
   ✅ Template آماده
   ```

3. **[05-Debugging-Specialist-Contract.md](05-Debugging-Specialist-Contract.md)** 🔧 - متخصص دیباگر
   ```
   ✅ فرآیند 6 مرحله‌ای
   ✅ تحلیل علت ریشه‌ای (5 Whys)
   ✅ رفع اتمیک
   ✅ ابزارهای دیباگ
   ✅ Use Cases + Examples
   ✅ چک‌لیست کامل
   ❌ ممنوع رفع کورکورانه!
   ```

---

## 🔥 مهم‌ترین Helpers (باید بلد باشی!)

### **Top 10 (اولویت بالا):**

1. **`PersianDateHelper.ToPersianDate()`** - تبدیل به شمسی
   ```csharp
   var date = PersianDateHelper.ToPersianDate(DateTime.Now);
   // "1404/10/05"
   ```

2. **`this.ParseDateFromHiddenInput()`** - Parse تاریخ در Controller
   ```csharp
   model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
   ```

3. **`IranianNationalCodeValidator.IsValid()`** - کد ملی
   ```csharp
   if (!IranianNationalCodeValidator.IsValid(model.NationalCode)) { ... }
   ```

4. **`PhoneNumberValidator.IsValidMobile()`** - موبایل
   ```csharp
   if (!PhoneNumberValidator.IsValidMobile(model.PhoneNumber)) { ... }
   ```

5. **`NotificationHelper.SetSuccess()`** - پیام موفقیت
   ```csharp
   NotificationHelper.SetSuccess(TempData, "عملیات موفق");
   ```

6. **`AgeCalculationHelper.CalculateAge()`** - محاسبه سن
   ```csharp
   var age = AgeCalculationHelper.CalculateAge(model.BirthDate);
   ```

7. **`ServiceResult.Successful()`** - نتیجه موفق
   ```csharp
   return ServiceResult.Successful();
   ```

8. **`PhoneNumberHelper.CleanPhoneNumber()`** - پاکسازی شماره
   ```csharp
   var cleaned = PhoneNumberHelper.CleanPhoneNumber(model.PhoneNumber);
   ```

9. **`@Html.Partial("_PersianDatePicker")`** - DatePicker
   ```razor
   @Html.Partial("_PersianDatePicker")
   ```

10. **`StringHelper.StripHtml()`** - حذف HTML
    ```csharp
    var plain = StringHelper.StripHtml(htmlContent);
    ```

---

## 📝 Checklist قبل از کد زدن

### ✅ **قبل از شروع:**
- [ ] README را خواندم
- [ ] **[03-Development-Contract-Quick-Guide.md](03-Development-Contract-Quick-Guide.md) را مطالعه کردم (الزامی!)**
- [ ] **[04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md) را بررسی کردم (برای ماژول جدید)**
- [ ] Helper مورد نیاز را پیدا کردم
- [ ] مثال را دیدم
- [ ] Best Practice را خواندم

### ✅ **در حین کد زدن:**
- [ ] از Helper موجود استفاده کردم (نه تکرار)
- [ ] مثال را دنبال کردم
- [ ] پارامترها را درست پاس دادم
- [ ] خطاها را مدیریت کردم

### ✅ **بعد از کد زدن:**
- [ ] کد را با مثال مقایسه کردم
- [ ] تست کردم
- [ ] در صورت نیاز، کد را بهینه کردم

---

## 🆘 نیاز به کمک؟

### **سوالات متداول:**

**Q1: چطور تاریخ میلادی را به شمسی تبدیل کنم؟**
```
A: PersianDateHelper.ToPersianDate(DateTime.Now)
   مستند: 01-Helpers-DateTime.md
```

**Q2: چطور کد ملی را چک کنم؟**
```
A: IranianNationalCodeValidator.IsValid("0123456789")
   مستند: 02-Helpers-Validation.md
```

**Q3: چطور پیام موفقیت نمایش بدهم؟**
```
A: NotificationHelper.SetSuccess(TempData, "...")
   مستند: 06-Quick-Reference.md
```

**Q4: چطور DatePicker شمسی اضافه کنم؟**
```
A: @Html.Partial("_PersianDatePicker")
   مستند: 01-Helpers-DateTime.md
```

**Q5: چطور تاریخ را در Controller Parse کنم؟**
```
A: this.ParseDateFromHiddenInput("Date", _logger)
   مستند: 01-Helpers-DateTime.md
```

**Q6: چطور یک ماژول جدید پیاده‌سازی کنم؟**
```
A: از راهنمای TODO استفاده کن
   مستند: 04-TODO-Implementation-Guide.md
   مراحل: 13 Phase + Template
```

**Q7: چطور از رنگ‌های استاندارد استفاده کنم؟**
```
A: از متغیرهای --medical-* استفاده کن
   مستند: 03-Development-Contract-Quick-Guide.md
   مثال: background-color: var(--medical-primary);
```

**Q8: Checklist نهایی قبل از Commit چیست؟**
```
A: مراجعه به قرارداد توسعه
   مستند: 03-Development-Contract-Quick-Guide.md
   بخش: Checklist نهایی قبل از Commit
```

**Q9: چطور یک خطا یا باگ را رفع کنم؟**
```
A: از فرآیند 6 مرحله‌ای استفاده کن
   مستند: 05-Debugging-Specialist-Contract.md
   مراحل: شناسایی → تحلیل → رفع → تست → گزارش
   قانون: ممنوع رفع کورکورانه!
```

**Q10: چطور علت ریشه‌ای خطا را پیدا کنم؟**
```
A: از روش 5 Whys استفاده کن
   مستند: 05-Debugging-Specialist-Contract.md
   بخش: Root Cause Analysis (5 Whys)
```

---

## 🔗 لینک‌های خارجی مفید

### **اسناد اصلی پروژه:**
- **[Docs/DEVELOPMENT_CONTRACT.md](../DEVELOPMENT_CONTRACT.md)** - قرارداد توسعه کامل (مرجع اصلی)
- **[Docs/TODO_TEMPLATE.md](../TODO_TEMPLATE.md)** - Template TODO کامل (مرجع اصلی)
- **[Docs/PROJECT_MODULES_CATALOG.md](../PROJECT_MODULES_CATALOG.md)** - کاتالوگ ماژول‌ها
- **[Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md](../PERSIAN_DATEPICKER_MODULE_GUIDE.md)** - راهنمای DatePicker
- **[Docs/IMAGE_UPLOAD_SYSTEM_GUIDE.md](../IMAGE_UPLOAD_SYSTEM_GUIDE.md)** - راهنمای آپلود تصویر
- **[Docs/CKEDITOR_USAGE_GUIDE.md](../CKEDITOR_USAGE_GUIDE.md)** - راهنمای CKEditor
- **[Docs/NOTIFICATION_HELPER_USAGE_GUIDE.md](../NOTIFICATION_HELPER_USAGE_GUIDE.md)** - راهنمای Notification

### **خلاصه‌ها در پایگاه دانش (این پوشه):**
- **[03-Development-Contract-Quick-Guide.md](03-Development-Contract-Quick-Guide.md)** - خلاصه قرارداد توسعه ⚡
- **[04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md)** - خلاصه راهنمای TODO ⚡

---

## 📞 پشتیبانی و به‌روزرسانی

### **در صورت اضافه شدن Helper/Extension جدید:**
1. ✅ به مستند مربوطه اضافه کن
2. ✅ Quick Reference را به‌روز کن
3. ✅ مثال اضافه کن
4. ✅ این INDEX را به‌روز کن

### **در صورت تغییر Helper موجود:**
1. ✅ مستند را به‌روز کن
2. ✅ مثال‌ها را بررسی کن
3. ✅ Version Number را افزایش بده

---

## 📈 آمار استفاده (برای آینده)

```
TODO: اضافه کردن آمار استفاده از Helpers
- پرکاربردترین Helper ها
- Use Case های رایج
- Best Practices بر اساس تجربه
```

---

**نسخه:** 1.2.0  
**آخرین به‌روزرسانی:** 1404/11/08 (اضافه شدن راهنمای احراز هویت Patient 🔒)  
**وضعیت:** ✅ **فعال**

---

## 📊 آمار کلی پایگاه دانش

- **تعداد فایل‌های راهنما:** 8 فایل
- **فایل‌های الزامی:** 3 فایل ⚡🔧 (قرارداد توسعه + TODO + دیباگر)
- **جعبه ابزار:** 14 Helper/Extension + 100+ متد 🧰
- **تعداد Helpers مستند شده:** 50+ Helper
- **تعداد مثال‌های عملی:** 200+ مثال
- **پوشش موضوعی:** DateTime, Validation, Security, Logging, Template, Reception, Cache, Files, Images, HTML, URLs, Collections, Objects
- **وضعیت:** ✅ **فعال و به‌روز**

---

🎉 **پایگاه دانش کامل است!** 🎉

**این فایل را Bookmark کن و همیشه قبل از کد زدن مراجعه کن!** 📌

