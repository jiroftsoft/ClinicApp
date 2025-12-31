# 📚 پایگاه دانش پروژه ClinicApp

**نسخه:** 2.0.0  
**تاریخ ایجاد:** 1404/10/05 (2025-12-25)  
**وضعیت:** ✅ **فعال و به‌روز**

---

## 🎯 هدف این پایگاه دانش

این پایگاه دانش شامل **تمام اطلاعات ضروری** برای توسعه در پروژه ClinicApp است.

### ✅ محتویات:
0. **فایل پیش‌پرواز** 🛡️ - چک‌لیست اجباری قبل از هر پاسخ (جدید! الزامی!)
1. **قرارداد توسعه** ⚡ - استانداردها و قواعد (الزامی!)
2. **راهنمای TODO** ⚡ - پیاده‌سازی ماژول جدید (الزامی!)
3. **متخصص دیباگر** 🔧 - فرآیند رفع خطا (الزامی!)
4. **جعبه ابزار (Toolbox)** 🧰 - 14 Helper/Extension + 100+ متد
5. **راهنمای کامل Helpers** - 50+ Helper با مثال
6. **راهنمای کامل Extensions** - 6 Extension با مثال
7. **Quick Reference** - دسترسی سریع به توابع
8. **MVC Routing Best Practices** 🛣️ - درس‌های گرانبها از تجربه واقعی

---

## 🚨 الزامی قبل از هر کاری!

### ⚡ این فایل‌ها را باید حفظ باشید:

#### 🛡️ **فوق‌العاده مهم (جدید!):**
0. **[PREFLIGHT_CHECKLIST.md](../../PREFLIGHT_CHECKLIST.md)** 🛡️
   - **الزامی قبل از هر پاسخ AI**
   - STEP 0: AI Guard Check (15 قانون ممنوعه)
   - STEP 1: 12 دروازه امنیتی
   - STEP 2: چک‌لیست قبل از پاسخ
   - STEP 3: Hard Stop در صورت تعارض
   - STEP 4: ساختار اجباری پاسخ
   - **این فایل باید قبل از هر پاسخ بررسی شود**

#### 🚨 **فوق‌العاده مهم:**
1. **[CRITICAL-FINANCIAL-MODULE-CONTRACT.md](CRITICAL-FINANCIAL-MODULE-CONTRACT.md)** 💰
   - **الزامی برای هر تغییر در صندوق، پرداخت، گزارش، محاسبات**
   - 10 قانون طلایی
   - **کوچکترین اشتباه = مشکل حقوقی!**
   - Checklist قبل از تغییر
   - **این قرارداد نقض‌ناپذیر است**

#### ⚡ **کلیدی:**

2. **[03-Development-Contract-Quick-Guide.md](03-Development-Contract-Quick-Guide.md)**
   - استانداردهای توسعه
   - پالت رنگ
   - Strongly-Typed
   - Bulletproof Coding
   - SRP Architecture
   - **Checklist نهایی قبل از Commit**

3. **[04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md)**
   - 13 Phase پیاده‌سازی
   - Checklist هر Phase
   - زمان‌بندی (12-17 روز)
   - Template آماده

4. **[05-Debugging-Specialist-Contract.md](05-Debugging-Specialist-Contract.md)** 🔧
   - فرآیند 6 مرحله‌ای دیباگ
   - تحلیل علت ریشه‌ای (5 Whys)
   - رفع اتمیک (Atomic Fix)
   - ابزارهای دیباگ
   - **قانون طلایی: ممنوع رفع کورکورانه!**

---

## 📂 ساختار پایگاه دانش

```
Docs/Knowledge-Base/
├── README.md (این فایل)
├── INDEX.md (فهرست کامل)
├── CRITICAL-FINANCIAL-MODULE-CONTRACT.md 🚨💰 الزامی مالی!
├── 01-Helpers-DateTime.md
├── 02-Helpers-Validation.md
├── 03-Development-Contract-Quick-Guide.md ⚡ الزامی
├── 04-TODO-Implementation-Guide.md ⚡ الزامی
├── 05-Debugging-Specialist-Contract.md 🔧 الزامی
├── 06-Quick-Reference.md
├── 08-MVC-Routing-Best-Practices.md 🛣️
└── HelperExtensionsGuide.md 🧰 جعبه ابزار (100+ متد)

Root/
├── PREFLIGHT_CHECKLIST.md 🛡️ الزامی قبل از هر پاسخ AI (جدید!)
└── ...
```

---

## 🚀 نحوه استفاده

### **⚡ قبل از هر کاری (الزامی!):**
0. ✅ **[PREFLIGHT_CHECKLIST.md](../../PREFLIGHT_CHECKLIST.md) را بررسی کن** 🛡️ (قبل از هر پاسخ AI)
1. ✅ **[03-Development-Contract-Quick-Guide.md](03-Development-Contract-Quick-Guide.md) را بخوان**
2. ✅ **[04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md) را بررسی کن**
3. ✅ **[05-Debugging-Specialist-Contract.md](05-Debugging-Specialist-Contract.md) را مطالعه کن** 🔧
4. ✅ **Checklist نهایی را Bookmark کن**

### **قبل از کد زدن:**
1. ✅ مستند مربوطه را پیدا کن
2. ✅ مثال را ببین
3. ✅ کد را کپی و استفاده کن

### **مثال:**
```
نیاز دارم: تبدیل تاریخ میلادی به شمسی
→ مراجعه به: 01-Helpers-DateTime.md
→ پیدا کردن: PersianDateHelper.ToPersianDate()
→ کپی کردن مثال
→ استفاده در کد
```

```
نیاز دارم: پیاده‌سازی ماژول جدید
→ مراجعه به: 04-TODO-Implementation-Guide.md
→ کپی کردن Template TODO
→ Phase به Phase پیش رفتن
→ Checklist نهایی قبل از Commit
```

```
نیاز دارم: رفع یک خطا یا باگ
→ مراجعه به: 05-Debugging-Specialist-Contract.md 🔧
→ فرآیند: شناسایی → تحلیل → رفع → تست → گزارش
→ قانون: ممنوع رفع کورکورانه!
→ روش: 5 Whys برای علت ریشه‌ای
```

---

## 📖 فهرست مستندات

### ⚡ **0. اسناد الزامی (باید بلد باشی!)**

#### **[03-Development-Contract-Quick-Guide.md](03-Development-Contract-Quick-Guide.md)**
**محتوا:**
- اصول اساسی (Non-Negotiable)
- پالت رنگ استاندارد (`--medical-*`)
- Strongly-Typed Development
- Bulletproof Coding (try-catch, null check, validation)
- معماری SRP (Controller, Service, Repository)
- سیستم پیام‌ها (Toastr, SweetAlert2)
- تقویم شمسی (Persian DatePicker)
- سیستم آپلود تصویر
- CKEditor (ویرایشگر متن)
- فرم‌های درمانی (Medical Forms)
- **Checklist نهایی قبل از Commit**

**چرا مهم است:**
- تمام قواعد توسعه در یک فایل
- Checklist نهایی برای Quality Control
- جلوگیری از خطاهای رایج

---

#### **[04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md)**
**محتوا:**
- Quick Start Checklist
- 13 Phase پیاده‌سازی:
  1. Analysis & Design
  2. Backend Implementation
  3. Controller Implementation
  4. View Implementation
  5. UI/UX Optimization
  6. Color Scheme Standardization
  7. Notification System
  8. Persian DatePicker Integration
  9. CKEditor Integration
  10. Image Upload System
  11. Medical Form Design Standards
  12. Testing & QA
  13. Deployment Preparation
- زمان‌بندی کلی (12-17 روز کاری)
- **Template TODO آماده کپی**
- Checklist نهایی قبل از Commit

**چرا مهم است:**
- راهنمای گام‌به‌گام پیاده‌سازی
- جلوگیری از فراموش کردن موارد مهم
- زمان‌بندی واقع‌بینانه

---

### **1. Helpers - تاریخ و زمان**
**فایل:** [01-Helpers-DateTime.md](01-Helpers-DateTime.md)

**محتوا:**
- `PersianDateHelper.cs` - تبدیل میلادی ↔ شمسی
- `PersianDatePickerHelper.cs` - DatePicker در View
- `DateTimeExtensions.cs` - Extension برای DateTime
- `PersianDateExtensions.cs` - Extension تاریخ شمسی
- `TimeFormatHelper.cs` - فرمت زمان
- `AgeCalculationHelper.cs` - محاسبه سن
- `ControllerExtensions.ParseDateFromHiddenInput` - Parse تاریخ در Controller

**تعداد:** 6 Helper

---

### **2. Helpers - اعتبارسنجی**
**فایل:** [02-Helpers-Validation.md](02-Helpers-Validation.md)

**محتوا:**
- `IranianNationalCodeValidator.cs` - کد ملی
- `PhoneNumberValidator.cs` - شماره تلفن
- `PhoneNumberHelper.cs` - نرمال‌سازی تلفن
- `IdentityValidators.cs` - Identity
- `ValidationResult.cs` - نتیجه Validation
- `SecurityValidationResult.cs` - Validation امنیتی

**تعداد:** 6 Helper

---

### **3. جعبه ابزار (Toolbox)** 🧰
**فایل:** [HelperExtensionsGuide.md](HelperExtensionsGuide.md)

**محتوا:**
- **5 Extensions:** StringExtensions, DateTimeExtensions, NumericExtensions, CollectionExtensions, ObjectExtensions
- **8 Helpers:** CacheHelper, RetryHelper, SecurityHelper, ValidationHelper, FileHelper, HtmlHelper, UrlHelper, ImageHelper
- **100+ متد کاربردی** برای:
  - مدیریت String (Truncate, Mask, Slug, Email Validation)
  - عملیات DateTime (StartOfDay, CalculateAge, ToPersianDate)
  - عملیات عددی (ToCurrency, ApplyDiscount, ToFileSize)
  - Collection ها (IsNullOrEmpty, DistinctBy, Chunk, Shuffle)
  - Object ها (DeepClone, ToDictionary, ToJson)
  - Cache مدیریت (GetOrCreate, Set, Remove)
  - امنیت (HashPassword, Encrypt, Decrypt, SanitizeInput)
  - فایل ها (ReadJson, WriteJson, SafeCopy)
  - تصاویر (ResizeImage, CreateThumbnail, ToBase64)
  - HTML/URL (StripHtml, BuildLink, CombineUrl)

**ویژگی‌ها:**
- ✅ رعایت SRP
- ✅ XML Documentation کامل
- ✅ Null Safety
- ✅ Error Handling
- ✅ Performance Optimized

**تعداد:** 14 فایل + 100+ متد

**مثال:**
```csharp
// String Extension
"متن خیلی طولانی".Truncate(10); // "متن خیلی ..."

// Cache Helper
CacheHelper.GetOrCreate("users", () => db.Users.ToList(), 30);

// Security Helper
SecurityHelper.Encrypt(password, key);
```

---

### **4. Quick Reference**
**فایل:** [06-Quick-Reference.md](06-Quick-Reference.md)

**محتوا:**
- جدول کامل Security Helpers
- جدول کامل Logging Helpers
- جدول کامل Template Helpers
- جدول کامل Reception Helpers
- جدول سریع برای یادگیری

**تعداد:** 30+ Helper

---

### **5. متخصص دیباگر ارشد**
**فایل:** [05-Debugging-Specialist-Contract.md](05-Debugging-Specialist-Contract.md) 🔧

**محتوا:**
- فرآیند استاندارد دیباگ (6 مرحله)
- تحلیل علت ریشه‌ای (5 Whys)
- بررسی وابستگی‌ها
- رفع اتمیک (Atomic Fix)
- تست و اعتبارسنجی
- گزارش‌دهی حرفه‌ای
- ابزارهای دیباگ (Static Analysis, Runtime, Database)
- نمونه‌های کاربردی (Compilation Error, N+1 Query, Memory Leak)
- چک‌لیست کامل
- سطوح دیباگ (Level 1-4)
- الگوهای رایج خطا
- نکات طلایی
- **قانون اصلی: ممنوع رفع کورکورانه!**

**مرحله‌های الزامی:**
1. شناسایی و دسته‌بندی
2. تحلیل علت ریشه‌ای
3. بررسی وابستگی‌ها
4. رفع اتمیک
5. تست و اعتبارسنجی
6. گزارش‌دهی حرفه‌ای

---

### **6. فهرست کامل**
**فایل:** [INDEX.md](INDEX.md)

**محتوا:**
- لینک به تمام فایل‌ها
- جستجوی سریع بر اساس موضوع
- آمار کامل
- مسیر یادگیری پیشنهادی
- سوالات متداول (FAQ)

---

## 📊 آمار

- **تعداد فایل‌های مستند:** 8 فایل
- **فایل‌های الزامی:** 3 فایل ⚡🔧
- **تعداد Helpers مستند شده:** 50+ Helper
- **جعبه ابزار (Toolbox):** 14 فایل + 100+ متد 🧰
- **تعداد مثال‌های عملی:** 200+ مثال
- **راهنماهای توسعه:** 2 راهنمای جامع (قرارداد + TODO)
- **وضعیت:** ✅ **فعال و به‌روز**

---

## 🎓 مسیر یادگیری پیشنهادی

### **⚡ مرحله 0: قبل از شروع (الزامی!)**
- ✅ **[03-Development-Contract-Quick-Guide.md](03-Development-Contract-Quick-Guide.md) را بخوان**
- ✅ **[04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md) را بررسی کن**
- ✅ **[05-Debugging-Specialist-Contract.md](05-Debugging-Specialist-Contract.md) را مطالعه کن** 🔧
- ✅ **Checklist نهایی را Bookmark کن**

### **مرحله 1: شروع (روز 1)**
- ✅ این README را بخوان
- ✅ [INDEX.md](INDEX.md) را مرور کن
- ✅ یک Use Case ساده انتخاب کن (مثلاً تبدیل تاریخ)

### **مرحله 2: یادگیری DateTime (روز 2-3)**
- ✅ فایل [01-Helpers-DateTime.md](01-Helpers-DateTime.md) را مطالعه کن
- ✅ تمام مثال‌ها را امتحان کن
- ✅ در پروژه واقعی استفاده کن

### **مرحله 3: یادگیری Validation (روز 4-5)**
- ✅ فایل [02-Helpers-Validation.md](02-Helpers-Validation.md) را مطالعه کن
- ✅ کد ملی و موبایل را تست کن
- ✅ Validator ها را در فرم‌ها استفاده کن

### **مرحله 4: یادگیری جعبه ابزار (روز 6-8)** 🧰
- ✅ فایل [HelperExtensionsGuide.md](HelperExtensionsGuide.md) را مطالعه کن
- ✅ 14 فایل Helper/Extension را بشناس
- ✅ 100+ متد کاربردی را امتحان کن
- ✅ فایل [06-Quick-Reference.md](06-Quick-Reference.md) را مرور کن
- ✅ هر دسته را جداگانه یاد بگیر
- ✅ Best Practices را رعایت کن

### **مرحله 5: پیاده‌سازی ماژول جدید (روز 9+)**
- ✅ **[04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md) را مرور کن**
- ✅ Template TODO را کپی کن
- ✅ Phase به Phase پیش برو
- ✅ **همیشه قبل از Commit، Checklist را بررسی کن**

### **مرحله 6: تسلط (روز 15+)**
- ✅ ترکیب Helpers را یاد بگیر
- ✅ در Use Case های پیچیده استفاده کن
- ✅ بهینه‌سازی کن
- ✅ **قرارداد توسعه را همیشه در ذهن داشته باش**

---

## 🆘 سوالات متداول (FAQ)

### **Q1: از کجا شروع کنم؟**
```
A: 1. این README را بخوان
   2. قرارداد توسعه (03) را مطالعه کن (الزامی!)
   3. INDEX.md را مرور کن
   4. یک Helper ساده را امتحان کن
```

### **Q2: چطور تاریخ میلادی را به شمسی تبدیل کنم؟**
```
A: PersianDateHelper.ToPersianDate(DateTime.Now)
   مستند: 01-Helpers-DateTime.md
```

### **Q3: چطور کد ملی را چک کنم؟**
```
A: IranianNationalCodeValidator.IsValid("0123456789")
   مستند: 02-Helpers-Validation.md
```

### **Q4: چطور یک ماژول جدید پیاده‌سازی کنم؟**
```
A: از راهنمای TODO استفاده کن
   مستند: 04-TODO-Implementation-Guide.md
   مراحل: 13 Phase + Template
```

### **Q5: Checklist نهایی قبل از Commit چیست؟**
```
A: مراجعه به قرارداد توسعه
   مستند: 03-Development-Contract-Quick-Guide.md
   بخش: Checklist نهایی قبل از Commit
```

**Q6: چطور یک خطا یا باگ را رفع کنم؟** 🔧
```
A: از فرآیند 6 مرحله‌ای استفاده کن
   مستند: 05-Debugging-Specialist-Contract.md
   مراحل: شناسایی → تحلیل → رفع → تست → گزارش
   قانون: ممنوع رفع کورکورانه!
```

**Q7: چطور علت ریشه‌ای خطا را پیدا کنم؟**
```
A: از روش 5 Whys استفاده کن
   مستند: 05-Debugging-Specialist-Contract.md
   بخش: Root Cause Analysis
   مثال: چرا خطا رخ داد؟ → 5 بار "چرا" بپرس
```

**Q8: جعبه ابزار (Toolbox) پروژه چیست؟** 🧰
```
A: 14 فایل Helper/Extension با 100+ متد کاربردی
   مستند: HelperExtensionsGuide.md
   شامل: StringExtensions, DateTimeExtensions, NumericExtensions,
          CollectionExtensions, ObjectExtensions, CacheHelper,
          RetryHelper, SecurityHelper, ValidationHelper, FileHelper,
          HtmlHelper, UrlHelper, ImageHelper
   مثال: "long text".Truncate(10) → "long te..."
          CacheHelper.GetOrCreate("key", () => data)
```

---

## 🔗 لینک‌های مفید

### **اسناد اصلی پروژه:**
- **[Docs/DEVELOPMENT_CONTRACT.md](../DEVELOPMENT_CONTRACT.md)** - قرارداد توسعه کامل
- **[Docs/TODO_TEMPLATE.md](../TODO_TEMPLATE.md)** - Template TODO کامل
- **[Docs/PROJECT_MODULES_CATALOG.md](../PROJECT_MODULES_CATALOG.md)** - کاتالوگ ماژول‌ها
- **[Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md](../PERSIAN_DATEPICKER_MODULE_GUIDE.md)** - راهنمای DatePicker
- **[Docs/IMAGE_UPLOAD_SYSTEM_GUIDE.md](../IMAGE_UPLOAD_SYSTEM_GUIDE.md)** - راهنمای آپلود تصویر
- **[Docs/CKEDITOR_USAGE_GUIDE.md](../CKEDITOR_USAGE_GUIDE.md)** - راهنمای CKEditor

### **خلاصه‌ها در این پایگاه دانش:**
- **[03-Development-Contract-Quick-Guide.md](03-Development-Contract-Quick-Guide.md)** ⚡
- **[04-TODO-Implementation-Guide.md](04-TODO-Implementation-Guide.md)** ⚡

---

## 📝 چک‌لیست قبل از کد زدن

### ✅ **قبل از شروع:**
- [ ] **فایل پیش‌پرواز (PREFLIGHT_CHECKLIST.md) را بررسی کردم** 🛡️
- [ ] **قرارداد توسعه (03) را خواندم**
- [ ] **راهنمای TODO (04) را بررسی کردم**
- [ ] **متخصص دیباگر (05) را مطالعه کردم** 🔧
- [ ] README را خواندم
- [ ] INDEX را مرور کردم
- [ ] Helper مورد نیاز را پیدا کردم
- [ ] مثال را دیدم
- [ ] Best Practice را خواندم

### ✅ **در حین کد زدن:**
- [ ] از Helper موجود استفاده کردم (نه تکرار)
- [ ] مثال را دنبال کردم
- [ ] پارامترها را درست پاس دادم
- [ ] خطاها را مدیریت کردم
- [ ] **قواعد قرارداد توسعه را رعایت کردم**
- [ ] **اگر خطا دیدم، فرآیند 6 مرحله‌ای دیباگ را دنبال کردم** 🔧

### ✅ **قبل از Commit:**
- [ ] **Checklist نهایی (03) را بررسی کردم**
- [ ] کد را با مثال مقایسه کردم
- [ ] تست کردم
- [ ] Linter Errors را برطرف کردم
- [ ] Code Review انجام شد

---

## 💡 نکات مهم

### ⚠️ الزامات:
1. ✅ **همیشه قبل از شروع، قرارداد توسعه را مرور کن**
2. ✅ **برای ماژول جدید، از راهنمای TODO استفاده کن**
3. ✅ **قبل از هر Commit، Checklist نهایی را بررسی کن**
4. ✅ **از Helpers موجود استفاده کن، تکرار ننویس**

### ⚠️ ممنوعیات:
1. ❌ **استفاده از رنگ‌های جیق و جلف**
2. ❌ **استفاده از `ViewBag`/`ViewData` برای داده‌های اصلی**
3. ❌ **استفاده از `datetime-local` (باید از Persian DatePicker استفاده شود)**
4. ❌ **استفاده از `alert()` یا `confirm()` (باید از SweetAlert2 استفاده شود)**

---

## 🎯 مسیر سریع (Quick Path)

### **برای یادگیری:**
```
0. PREFLIGHT_CHECKLIST.md 🛡️ (قبل از هر پاسخ AI - الزامی!)
1. README (این فایل) ✅
2. 03-Development-Contract-Quick-Guide.md ⚡
3. 04-TODO-Implementation-Guide.md ⚡
4. 05-Debugging-Specialist-Contract.md 🔧 ⚡
5. HelperExtensionsGuide.md 🧰 (جعبه ابزار - 100+ متد)
6. INDEX.md
7. 01-Helpers-DateTime.md
8. 02-Helpers-Validation.md
9. 06-Quick-Reference.md
```

### **برای پیاده‌سازی ماژول جدید:**
```
1. 03-Development-Contract-Quick-Guide.md ⚡
2. 04-TODO-Implementation-Guide.md ⚡
3. کپی Template TODO
4. Phase به Phase پیش برو
5. Checklist نهایی قبل از Commit
```

### **برای رفع خطا یا باگ:** 🔧
```
1. 05-Debugging-Specialist-Contract.md ⚡
2. فرآیند 6 مرحله‌ای:
   - شناسایی و دسته‌بندی
   - تحلیل علت ریشه‌ای (5 Whys)
   - بررسی وابستگی‌ها
   - رفع اتمیک
   - تست و اعتبارسنجی
   - گزارش‌دهی حرفه‌ای
3. ❌ ممنوع رفع کورکورانه!
```

---

## 📞 پشتیبانی

### **در صورت نیاز به کمک:**
1. ✅ INDEX.md را مرور کن (سوالات متداول)
2. ✅ Quick Reference را ببین
3. ✅ مستندات کامل را مطالعه کن

---

## 🎉 تبریک!

**اگر تا اینجا خواندی، آماده‌ای برای شروع! 🚀**

**یادت باشه:**
- 🛡️ فایل پیش‌پرواز (PREFLIGHT_CHECKLIST.md) = الزامی قبل از هر پاسخ AI!
- ⚡ قرارداد توسعه (03) = الزامی!
- ⚡ راهنمای TODO (04) = الزامی برای ماژول جدید!
- 🔧 متخصص دیباگر (05) = الزامی برای رفع خطا!
- ✅ Checklist نهایی = قبل از هر Commit!
- ❌ ممنوع رفع کورکورانه!

---

**نسخه:** 2.2.0  
**آخرین به‌روزرسانی:** 1404/11/07 (اضافه شدن فایل پیش‌پرواز 🛡️)  
**وضعیت:** ✅ **فعال و به‌روز**

---

**📌 این فایل را Bookmark کن و همیشه قبل از کد زدن مراجعه کن!** 📌
