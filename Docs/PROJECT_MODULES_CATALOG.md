# 📚 کاتالوگ کامل ماژول‌ها و Helper‌های پروژه ClinicApp

**تاریخ ایجاد:** 1404/10/05 (2025-12-25)  
**هدف:** جلوگیری از ایجاد ماژول‌های تکراری و استفاده بهینه از منابع موجود  
**وضعیت:** ✅ **به‌روز و فعال**

---

## 📋 فهرست مطالب

1. [CSS Files - فایل‌های استایل](#1-css-files---فایلهای-استایل)
2. [JavaScript Files - فایل‌های جاوا اسکریپت](#2-javascript-files---فایلهای-جاوا-اسکریپت)
3. [Helper Classes - کلاس‌های کمکی](#3-helper-classes---کلاسهای-کمکی)
4. [Services - سرویس‌ها](#4-services---سرویسها)
5. [Extensions - متدهای توسعه](#5-extensions---متدهای-توسعه)
6. [Partial Views - ویوهای جزئی](#6-partial-views---ویوهای-جزئی)
7. [راهنمای استفاده سریع](#7-راهنمای-استفاده-سریع)

---

## 1️⃣ CSS Files - فایل‌های استایل

### 📂 مسیر: `Content/css/`

#### ✅ فونت‌ها و تایپوگرافی

##### **`local-fonts.css`** ⭐ **مهم**
```css
/* فونت‌های محلی پروژه */
- Vazir Font Family (Regular, Bold, Medium, Light, Thin)
- Vazirmatn Font Family (برای سازگاری)
- Shabnam Font Family (Regular, Bold, Light)
- Yekan Font Family
- Font Awesome 6 (Solid, Regular, Brands)
```

**استفاده:**
```html
<!-- در _Layout.cshtml -->
<link href="~/Content/css/local-fonts.css" rel="stylesheet" />
```

**نکته مهم:** 🚫 **فونت جدید اضافه نکنید!** از `Vazir` استفاده کنید.

---

#### ✅ سیستم طراحی و استانداردها

##### **`design-system.css`**
```css
/* سیستم طراحی استاندارد پروژه */
- CSS Variables برای رنگ‌ها
- Spacing System
- Typography System
- Component Styles
```

##### **`medical-environment.css`** و **`medical-environment-styles.css`**
```css
/* استایل‌های محیط درمانی */
- رنگ‌های پزشکی استاندارد (--medical-primary, --medical-secondary)
- Card Styles
- Button Styles
- Form Styles
```

**استفاده:**
```css
/* در CSS خود */
.my-button {
    background-color: var(--medical-primary); /* #2c5aa0 */
    color: white;
}
```

---

#### ✅ فرم‌ها و ورودی‌ها

##### **`forms-medical.css`** و **`medical-forms.css`**
```css
/* استایل‌های فرم‌های درمانی */
- Input Styles
- Select Styles
- Validation Styles
- Error Messages
```

##### **`form-standards.css`**
```css
/* استانداردهای فرم */
- Form Layout
- Fieldset Styles
- Label Styles
- Help Text
```

**استفاده:**
```html
<form class="medical-form">
    <div class="form-group">
        <label class="form-label">نام</label>
        <input type="text" class="form-control" />
    </div>
</form>
```

---

#### ✅ Navigation و Layout

##### **`modern-navigation.css`**
```css
/* منوی Navigation مدرن */
- Navbar Styles
- Mobile Menu
- Emergency Contact Link
- RTL Support
```

##### **`homepage-layout.css`**
```css
/* Layout صفحه اصلی */
- Grid System
- Section Spacing
- Container Styles
```

##### **`reception-layout.css`** ⭐ **جدید**
```css
/* Layout اختصاصی فرم پذیرش */
- Minimal Header
- Full-width Content
- Real-time Clock
- Logout Button
```

---

#### ✅ Sections صفحه اصلی

##### **Sections مهم:**
- `hero-section.css` - بخش Hero
- `stories-section.css` - استوری‌ها (مثل Instagram)
- `doctors-section.css` - پزشکان
- `services-section.css` / `modern-services-section.css` - خدمات
- `health-tips-section.css` - نکات سلامتی
- `blog-section.css` - بلاگ
- `gallery-section.css` - گالری
- `video-section.css` - ویدیو
- `testimonials-section.css` - نظرات بیماران
- `faq-section.css` - سوالات متداول
- `contact-section.css` - تماس با ما
- `insurance-info-section.css` - اطلاعات بیمه
- `medical-equipment-section.css` - تجهیزات پزشکی
- `announcements-section.css` - اطلاعیه‌ها

**نکته:** 🚫 **Section جدید اضافه نکنید!** ابتدا از موارد موجود استفاده کنید.

---

#### ✅ صفحات خاص

##### **صفحات موجود:**
- `about-page.css` - درباره ما
- `contact-form-page.css` - فرم تماس
- `contact-thankyou-page.css` - صفحه تشکر
- `contact-track-page.css` - پیگیری تماس
- `details-page-medical.css` - صفحه جزئیات (Medical)
- `details-standards.css` - استانداردهای جزئیات

---

#### ✅ ماژول‌های خاص

##### **بیمه (Insurance):**
```bash
Content/css/insurance/
├── supplementary-tariff-index.css
├── supplementary-tariff-views.css
```

- `insurance-carousel.css` - کاروسل بیمه‌ها
- `insurance-plan-index.css` - لیست طرح‌های بیمه
- `insurance-plan.css` - جزئیات طرح بیمه
- `patient-insurance.css` و `patient-insurance-enhanced.css` - بیمه بیمار

##### **پذیرش (Reception):**
```bash
Content/css/reception/
├── patient-accordion.css
├── reception-accordion.css
├── realtime-insurance-binding.css
```

- `reception-standards.css` - استانداردهای پذیرش
- `service-calculation.css` - محاسبات خدمات

##### **دیگر ماژول‌ها:**
- `doctor-assignment-index.css` - تخصیص پزشک
- `medical-select2.css` - Select2 سفارشی برای محیط پزشکی
- `notification-system.css` و `notifications.css` - سیستم اعلانات

---

#### ✅ Footer و Sidebar

- `footer.css` و `medical-footer.css` - فوتر
- `footer-slider-section.css` - اسلایدر فوتر
- `sidebar-slider-section.css` - اسلایدر سایدبار
- `medical-sidebar.css` - سایدبار پزشکی

---

#### ✅ Carousel و Hero

- `hero-carousel.css` - Hero Carousel اصلی
- `insurance-carousel.css` - Carousel بیمه‌ها

---

#### ✅ دیگر فایل‌ها

- `quick-appointment-section.css` - نوبت‌گیری سریع
- `value-proposition-section.css` - ارزش‌های ما
- `swiper-font-override.css` - Override فونت Swiper
- `select2.css` و `select2.min.css` - Select2
- `admin-layout.css` - Layout ادمین
- `homepage-sections-spacing.css` - فاصله‌گذاری Section ها
- `supplementary-tariff-styles.css` - تعرفه‌های تکمیلی

---

### 📊 خلاصه CSS Files

| دسته‌بندی | تعداد | نکته |
|---------|------|------|
| **فونت‌ها** | 1 | `local-fonts.css` - **استفاده اجباری** |
| **سیستم طراحی** | 3 | `design-system.css`, `medical-environment.css` |
| **فرم‌ها** | 3 | `forms-medical.css`, `medical-forms.css`, `form-standards.css` |
| **Navigation** | 1 | `modern-navigation.css` |
| **Sections** | 15+ | **قبل از ایجاد Section جدید، حتماً بررسی کنید** |
| **بیمه** | 7 | فولدر `insurance/` + فایل‌های `insurance-*.css` |
| **پذیرش** | 5 | فولدر `reception/` + `reception-*.css` |
| **Layout** | 3 | `homepage-layout.css`, `reception-layout.css`, `admin-layout.css` |

---

## 2️⃣ JavaScript Files - فایل‌های جاوا اسکریپت

### 📂 مسیر: `Content/js/`

#### ✅ سیستم Notification

##### **`admin-notification-service.js`** ⭐ **مهم**
```javascript
/* سرویس Notification برای Admin Panel */
- AdminNotification.success(message, title)
- AdminNotification.error(message, title)
- AdminNotification.warning(message, title)
- AdminNotification.info(message, title)
- AdminNotification.confirm(message, title, onConfirm, onCancel)
- AdminNotification.criticalError(message, title)
- AdminNotification.successAlert(message, title)
```

**استفاده:**
```javascript
// در Admin Panel
AdminNotification.success('عملیات با موفقیت انجام شد');
AdminNotification.confirm('آیا مطمئن هستید؟', 'حذف', function() {
    // حذف
});
```

##### **`notification-helper.js`** ⭐ **جدید**
```javascript
/* سرویس Notification برای Reception و صفحات عمومی */
- NotificationHelper.success(message, title, options) / Notify.success()
- NotificationHelper.error(message, title, options) / Notify.error()
- NotificationHelper.warning(message, title, options) / Notify.warning()
- NotificationHelper.info(message, title, options) / Notify.info()
- NotificationHelper.confirm(message, title, onConfirm, onCancel, options)
- NotificationHelper.showLoading(message)
- NotificationHelper.hideLoading()
- NotificationHelper.clearAll()
```

**استفاده:**
```javascript
// در Reception یا صفحات عمومی
Notify.success('پذیرش با موفقیت ثبت شد');
Notify.confirm('آیا مطمئن هستید؟', 'حذف', function() {
    // حذف
});

// Loading
Notify.showLoading('در حال پردازش...');
setTimeout(() => {
    Notify.hideLoading();
    Notify.success('تمام شد');
}, 2000);
```

**تفاوت:**
- `admin-notification-service.js` → Admin Panel
- `notification-helper.js` → Reception + Public Pages

---

#### ✅ تقویم شمسی (Persian DatePicker)

##### **`persian-datepicker-manager.js`** ⭐ **مهم**
```javascript
/* مدیریت DatePicker شمسی */
- تنظیمات خودکار
- فرمت‌بندی تاریخ
- Parse تاریخ شمسی
```

**استفاده:**
```html
<!-- در View -->
@Html.Partial("_PersianDatePicker")
@Html.Partial("_PersianDatePickerScript")
```

##### **`persian-datepicker-component.js`**
```javascript
/* کامپوننت DatePicker */
- Component-based approach
- Event Handling
```

---

#### ✅ Navigation و Layout

##### **`modern-navigation.js`**
```javascript
/* منوی Navigation مدرن */
- Mobile Menu Toggle
- Sticky Navigation
- Smooth Scroll
```

##### **`admin-layout.js`**
```javascript
/* Layout ادمین */
- Sidebar Toggle
- Mobile Menu
```

---

#### ✅ Carousel و اسلایدر

##### **`hero-carousel.js`**
```javascript
/* Hero Carousel اصلی */
- Swiper.js Configuration
- Auto-play
- Navigation
- Pagination
```

##### **`insurance-carousel.js`**
```javascript
/* Carousel بیمه‌ها */
- Multi-item Carousel
- Responsive Breakpoints
```

---

#### ✅ Stories (مثل Instagram)

##### **`stories-component.js`** ⭐ **مهم**
```javascript
/* استوری‌ها - شبیه Instagram */
- Story Modal
- Video Player
- View Count
- Close Modal (با رفع باگ X button)
```

**استفاده:**
```html
<!-- Story ها خودکار از این JS استفاده می‌کنند -->
<div class="story-item" data-story-id="1" data-video-url="/path/to/video.mp4">
    <img src="/path/to/thumbnail.jpg" />
</div>
```

---

#### ✅ Gallery و Video

##### **`gallery-lightbox.js`**
```javascript
/* Lightbox گالری */
- Image Preview
- Navigation
- Zoom
```

##### **`video-modal.js`**
```javascript
/* Modal ویدیو */
- Video Player
- Modal Control
```

---

#### ✅ FAQ و Accordion

##### **`faq-accordion.js`**
```javascript
/* Accordion سوالات متداول */
- Expand/Collapse
- Smooth Animation
```

---

#### ✅ بهینه‌سازی و Performance

##### **`performance-optimizer.js`** ⭐ **مهم**
```javascript
/* بهینه‌سازی Performance */
- Lazy Loading Images
- Debounce/Throttle
- Resource Optimization
```

##### **`image-optimization.js`**
```javascript
/* بهینه‌سازی تصاویر */
- Lazy Load
- Responsive Images
- Placeholder
```

---

#### ✅ دیگر JS Files

##### **`medical-sidebar.js`**
```javascript
/* سایدبار پزشکی */
- Sidebar Toggle
- Menu Collapse
```

##### **`medical-toast.js`**
```javascript
/* Toast Notification سبک پزشکی */
- Medical-themed Toasts
```

##### **`medical-debug.js`**
```javascript
/* دیباگ محیط پزشکی */
- Console Logging
- Debug Tools
```

##### **`jquery-protection.js`**
```javascript
/* محافظت از jQuery */
- Prevent Conflicts
- NoConflict Mode
```

---

### 📊 خلاصه JavaScript Files

| دسته‌بندی | فایل‌ها | استفاده |
|---------|---------|---------|
| **Notification** | 2 | `admin-notification-service.js` (Admin), `notification-helper.js` (Public/Reception) |
| **DatePicker** | 2 | `persian-datepicker-manager.js`, `persian-datepicker-component.js` |
| **Navigation** | 2 | `modern-navigation.js`, `admin-layout.js` |
| **Carousel** | 2 | `hero-carousel.js`, `insurance-carousel.js` |
| **Stories** | 1 | `stories-component.js` |
| **Gallery/Video** | 2 | `gallery-lightbox.js`, `video-modal.js` |
| **Optimization** | 2 | `performance-optimizer.js`, `image-optimization.js` |
| **FAQ** | 1 | `faq-accordion.js` |
| **دیگر** | 4 | `medical-sidebar.js`, `medical-toast.js`, `medical-debug.js`, `jquery-protection.js` |

**تعداد کل:** 18 فایل

---

## 3️⃣ Helper Classes - کلاس‌های کمکی

### 📂 مسیر: `Helpers/`

#### ✅ Notification و پیام‌ها

##### **`NotificationHelper.cs`** ⭐ **خیلی مهم**
```csharp
/* مدیریت Toastr Notifications */
NotificationHelper.SetSuccess(TempData, "عملیات موفق");
NotificationHelper.SetError(TempData, "خطا");
NotificationHelper.SetWarning(TempData, "هشدار");
NotificationHelper.SetInfo(TempData, "اطلاعات");
```

**استفاده:**
```csharp
// در Controller
using static ClinicApp.Helpers.NotificationHelper;

NotificationHelper.SetSuccess(TempData, "پزشک با موفقیت ایجاد شد");
return RedirectToAction("Index");
```

**نکته:** 🚫 **هرگز مستقیماً از `TempData` استفاده نکنید!**

---

#### ✅ تاریخ و زمان (Persian Date)

##### **`PersianDateHelper.cs`** ⭐ **خیلی مهم**
```csharp
/* تبدیل تاریخ شمسی */
// تبدیل میلادی به شمسی
var persianDate = PersianDateHelper.ToPersianDate(DateTime.Now);

// تبدیل شمسی به میلادی
var gregorianDate = PersianDateHelper.ParsePersianDate("1404/10/05");

// Format سفارشی
var formatted = PersianDateHelper.ToPersianDateString(DateTime.Now, "yyyy/MM/dd");
```

##### **`ControllerExtensions.cs`** ⭐ **خیلی مهم**
```csharp
/* Extension Methods برای Controller */
// Parse کردن تاریخ از hidden input
model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);

// نمایش خطای مناسب
this.ShowError("خطایی رخ داده است");
```

**استفاده:**
```csharp
[HttpPost]
public async Task<ActionResult> Create(MyViewModel model)
{
    // Parse تاریخ
    model.PublishedAt = this.ParseDateFromHiddenInput("PublishedAt", _logger);
    model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
    
    // ...
}
```

##### **`PersianDatePickerHelper.cs`**
```csharp
/* Helper برای DatePicker */
- Generate DatePicker HTML
- JavaScript Configuration
```

---

#### ✅ Validation و اعتبارسنجی

##### **`IranianNationalCodeValidator.cs`** ⭐ **مهم**
```csharp
/* اعتبارسنجی کد ملی */
var isValid = IranianNationalCodeValidator.IsValid("0123456789");
```

##### **`PhoneNumberValidator.cs`**
```csharp
/* اعتبارسنجی شماره تماس */
var isValid = PhoneNumberValidator.IsValidMobile("09123456789");
var isValid = PhoneNumberValidator.IsValidPhone("02112345678");
```

##### **`PhoneNumberHelper.cs`**
```csharp
/* Helper برای شماره تماس */
var formatted = PhoneNumberHelper.FormatMobile("09123456789");
var cleaned = PhoneNumberHelper.CleanPhoneNumber("(021) 1234-5678");
```

---

#### ✅ String و متن

##### **`StringHelper.cs`** ⭐ **مهم**
```csharp
/* Helper برای String */
// حذف HTML Tags
var clean = StringHelper.StripHtml(htmlContent);

// Truncate با حفظ کلمات
var truncated = StringHelper.Truncate(text, 100);

// Truncate HTML (برای خلاصه مقالات)
var summary = StringHelper.StripHtmlAndTruncate(htmlContent, 200);

// Slug Generation
var slug = StringHelper.GenerateSlug("عنوان فارسی");
```

##### **`PersianNumberHelper.cs`**
```csharp
/* تبدیل اعداد فارسی */
// انگلیسی به فارسی
var persian = PersianNumberHelper.ToPersianNumber("1234");

// فارسی به انگلیسی
var english = PersianNumberHelper.ToEnglishNumber("۱۲۳۴");
```

---

#### ✅ Logging و لاگ‌گیری

##### **`LoggingHelper.cs`** و **`StructuredLogger.cs`** ⭐ **مهم**
```csharp
/* Serilog Logging */
_logger.Information("عملیات {Operation} برای {EntityId} انجام شد", "Create", entityId);
_logger.Warning("هشدار: {Message}", message);
_logger.Error(exception, "خطا در {Method}", methodName);
```

##### **`LoggingConfiguration.cs`**
```csharp
/* تنظیمات Logging */
- Serilog Configuration
- File Logging
- Console Logging
```

---

#### ✅ Security و امنیت

##### **`Security/SecurityLogger.cs`**
```csharp
/* لاگ امنیتی */
SecurityLogger.LogSecurityEvent("Unauthorized Access", userId);
```

##### **`Security/SensitiveDataMaskingHelper.cs`**
```csharp
/* Mask کردن داده‌های حساس */
var masked = SensitiveDataMaskingHelper.MaskNationalCode("0123456789");
var masked = SensitiveDataMaskingHelper.MaskPhoneNumber("09123456789");
```

##### **`AntiForgeryHelper.cs`**
```csharp
/* Anti-Forgery Token */
- CSRF Protection Helper
```

---

#### ✅ Image و File

##### **`ImagePathHelper.cs`**
```csharp
/* Helper برای مسیر تصاویر */
var fullPath = ImagePathHelper.GetFullImagePath(relativeUrl);
var thumbnailPath = ImagePathHelper.GetThumbnailPath(imagePath);
```

---

#### ✅ Age و محاسبات سنی

##### **`AgeCalculationHelper.cs`** ⭐ **مهم (محیط پزشکی)**
```csharp
/* محاسبه سن */
var age = AgeCalculationHelper.CalculateAge(birthDate);
var ageString = AgeCalculationHelper.CalculateAgeString(birthDate); // "۳۵ سال"
```

---

#### ✅ Excel و گزارش‌گیری

##### **`MedicalReportExcelGenerator.cs`**
```csharp
/* تولید گزارش Excel */
- Generate Excel Reports
- Medical Reports
- EPPlus Integration
```

---

#### ✅ Template و قالب

##### **`SmartTemplateService.cs`**, **`SmartTemplateParser.cs`**, **`SmartTemplateRenderer.cs`**
```csharp
/* سیستم Template */
- پردازش قالب‌های هوشمند
- جایگزینی متغیرها
- Render قالب
```

##### **`SmartTemplateVariableHelper.cs`**
```csharp
/* متغیرهای قالب */
- تعریف متغیرها
- Parse کردن متغیرها
```

---

#### ✅ Insurance و بیمه

##### **`Insurance/InsuranceTypeHelper.cs`**
```csharp
/* Helper برای نوع بیمه */
- Get Insurance Type Name
- Get Insurance Type Icon
```

##### **`InsurancePriorityHelper.cs`**
```csharp
/* اولویت بیمه‌ها */
- Sort Insurances by Priority
- Get Priority Name
```

---

#### ✅ Enum و Enumerations

##### **`EnumExtensions.cs`** ⭐ **مهم**
```csharp
/* Extension Methods برای Enum */
// گرفتن DisplayName فارسی
var displayName = MyEnum.Value.GetDisplayName();

// گرفتن لیست تمام مقادیر
var list = EnumExtensions.GetEnumList<MyEnum>();
```

---

#### ✅ Identity و User

##### **`IdentityExtensions.cs`**
```csharp
/* Extension Methods برای Identity */
- Get User ID from ClaimsPrincipal
- Get User Name
- Get User Roles
```

##### **`Validation/IdentityValidators.cs`**
```csharp
/* Validation برای Identity */
- Password Validation
- Username Validation
```

---

#### ✅ Reception و پذیرش

##### **`ReceptionAjaxHelper.cs`**
```csharp
/* Helper برای AJAX Reception */
- JSON Response
- Error Handling
```

##### **`ReceptionApiCodes.cs`**
```csharp
/* کدهای API پذیرش */
- Success Codes
- Error Codes
```

---

#### ✅ Database و SQL

##### **`DynamicSqlHelper.cs`** و **`DynamicSqlConfiguration.cs`**
```csharp
/* SQL دینامیک */
- Build SQL Queries Dynamically
- Safe SQL Building
```

##### **`SafeSqlBuilder.cs`**
```csharp
/* SQL امن */
- Prevent SQL Injection
- Parameterized Queries
```

---

#### ✅ دیگر Helper ها

##### **`AppHelper.cs`** و **`AppSettings.cs`**
```csharp
/* تنظیمات اپلیکیشن */
- Get App Settings
- Configuration Helper
```

##### **`ApplicationVersion.cs`**
```csharp
/* ورژن اپلیکیشن */
- Get Version Number
```

##### **`CultureHelper.cs`** و **`CultureExtensions.cs`** (در Extensions)
```csharp
/* فرهنگ و زبان */
- Set Persian Culture
- Date/Time Formatting
```

##### **`RegexHelper.cs`**
```csharp
/* Regex Patterns */
- Common Regex Patterns
- National Code Pattern
- Phone Number Pattern
```

##### **`ErrorMessageHelper.cs`**
```csharp
/* پیام‌های خطا */
- Standard Error Messages
- Persian Error Messages
```

##### **`TimeFormatHelper.cs`**
```csharp
/* فرمت زمان */
- Time Formatting
- Duration Formatting
```

##### **`RateLimiter.cs`**
```csharp
/* محدودکننده نرخ */
- Rate Limiting
- Throttling
```

##### **`SystemUsers.cs`**
```csharp
/* کاربران سیستمی */
- System User IDs
- Admin User ID
```

##### **`ServiceResult.cs`** و **`ServiceResultExtensions.cs`** ⭐ **خیلی مهم**
```csharp
/* نتیجه سرویس */
var result = ServiceResult.Successful();
var result = ServiceResult.Failed("خطا");

var result = ServiceResult<MyData>.Successful(data);
var result = ServiceResult<MyData>.Failed("خطا");
```

##### **`ValidationResult.cs`** و **`SecurityValidationResult.cs`**
```csharp
/* نتیجه Validation */
var result = new ValidationResult { IsValid = true };
```

##### **`TemplateRenderResult.cs`**
```csharp
/* نتیجه Render قالب */
```

##### **`LayoutDataHelper.cs`** ⭐ **مهم**
```csharp
/* داده‌های Layout */
var layoutData = LayoutDataHelper.GetLayoutData();
```

##### **`LoadStoriesActionFilter.cs`**
```csharp
/* Action Filter برای Stories */
- Load Stories Automatically
```

##### **`CKEditorHelper.cs`**
```csharp
/* Helper برای CKEditor */
- Configuration
- Initialization
```

##### **`HtmlHelpers/NavigationHtmlHelpers.cs`**
```csharp
/* HTML Helpers برای Navigation */
- Active Menu Item
- Breadcrumb
```

---

### 📊 خلاصه Helper Classes

| دسته‌بندی | تعداد | مهم‌ترین‌ها |
|---------|------|------------|
| **Notification** | 1 | `NotificationHelper.cs` |
| **تاریخ و زمان** | 3 | `PersianDateHelper.cs`, `ControllerExtensions.cs` |
| **Validation** | 3 | `IranianNationalCodeValidator.cs`, `PhoneNumberValidator.cs` |
| **String** | 2 | `StringHelper.cs`, `PersianNumberHelper.cs` |
| **Logging** | 3 | `LoggingHelper.cs`, `StructuredLogger.cs` |
| **Security** | 3 | `SecurityLogger.cs`, `SensitiveDataMaskingHelper.cs` |
| **Image** | 1 | `ImagePathHelper.cs` |
| **Age** | 1 | `AgeCalculationHelper.cs` |
| **Excel** | 1 | `MedicalReportExcelGenerator.cs` |
| **Template** | 4 | `SmartTemplateService.cs`, ... |
| **Insurance** | 2 | `InsuranceTypeHelper.cs`, `InsurancePriorityHelper.cs` |
| **Enum** | 1 | `EnumExtensions.cs` |
| **Identity** | 2 | `IdentityExtensions.cs`, `IdentityValidators.cs` |
| **Reception** | 2 | `ReceptionAjaxHelper.cs`, `ReceptionApiCodes.cs` |
| **Database** | 3 | `DynamicSqlHelper.cs`, `SafeSqlBuilder.cs` |
| **دیگر** | 15+ | `ServiceResult.cs`, `AppHelper.cs`, ... |

**تعداد کل:** 47+ فایل Helper

---

## 4️⃣ Services - سرویس‌ها

### 📂 مسیر: `Services/`

#### ✅ CMS (Content Management System)

**📂 مسیر:** `Services/CMS/`

**تعداد:** 19 سرویس

| سرویس | توضیحات |
|------|---------|
| `AboutPageService.cs` | مدیریت صفحه درباره ما |
| `AnnouncementService.cs` | اطلاعیه‌ها |
| `BlogPostService.cs` | مقالات بلاگ |
| `BlogPostCommentService.cs` | کامنت‌های بلاگ |
| `BlogPostLikeService.cs` | لایک‌های بلاگ |
| `ClinicWorkingHoursService.cs` | ساعات کاری |
| `ContactFormService.cs` | فرم تماس |
| `EmergencyContactService.cs` | تماس‌های اضطراری |
| `FAQService.cs` | سوالات متداول |
| `GalleryService.cs` | گالری تصاویر |
| `HealthTipService.cs` | نکات سلامتی |
| `InsuranceInfoService.cs` | اطلاعات بیمه |
| `MedicalEquipmentService.cs` | تجهیزات پزشکی |
| `MedicalServiceInfoService.cs` | خدمات پزشکی |
| `NewsletterCampaignService.cs` | کمپین‌های خبرنامه |
| `NewsletterSubscriptionService.cs` | اشتراک خبرنامه |
| `NewsletterTemplateService.cs` | قالب‌های خبرنامه |
| `PatientEducationMaterialService.cs` | مواد آموزشی بیمار |
| `SliderService.cs` | اسلایدر |
| `StoryService.cs` | استوری‌ها (مثل Instagram) |
| `TestimonialService.cs` | نظرات بیماران |
| `VideoService.cs` | ویدیوها |

**نکته:** 🚫 **قبل از ایجاد سرویس CMS جدید، حتماً بررسی کنید!**

---

#### ✅ Insurance (بیمه)

**📂 مسیر:** `Services/Insurance/`

**تعداد:** 29 سرویس

**سرویس‌های کلیدی:**
- `BaseInsuranceService.cs` - کلاس پایه
- `InsuranceService.cs` - سرویس اصلی بیمه
- `InsuranceTypeService.cs` - انواع بیمه
- `InsurancePlanService.cs` - طرح‌های بیمه
- `InsuranceTariffService.cs` - تعرفه‌های بیمه
- `SupplementaryTariffService.cs` - تعرفه‌های تکمیلی
- `PatientInsuranceService.cs` - بیمه بیماران
- `InsuranceCalculationService.cs` - محاسبات بیمه
- `InsuranceValidationService.cs` - اعتبارسنجی بیمه
- و 20 سرویس دیگر...

**نکته:** سیستم بیمه بسیار جامع است. قبل از تغییر، حتماً مستندات را مطالعه کنید.

---

#### ✅ Reception (پذیرش)

**📂 مسیر:** `Services/Reception/`

**تعداد:** 37 سرویس

**سرویس‌های کلیدی:**
- `ReceptionFacade.cs` ⭐ **خیلی مهم** - Facade اصلی پذیرش (5461 خط)
- `ReceptionService.cs` - سرویس اصلی پذیرش
- `PatientService.cs` - مدیریت بیماران
- `ServiceCalculationService.cs` - محاسبات خدمات
- `InsurancePricingService.cs` - قیمت‌گذاری بیمه
- `ReceptionValidationService.cs` - اعتبارسنجی
- `ReceptionWorkflowService.cs` - Workflow پذیرش
- و 30 سرویس دیگر...

**نکته:** `ReceptionFacade.cs` بسیار بزرگ و پیچیده است. برای تغییر، حتماً با معماری آشنا شوید.

---

#### ✅ Payment (پرداخت)

**📂 مسیر:** `Services/Payment/`

**سرویس‌های کلیدی:**

**POS Payment:**
- `POS/PosPaymentService.cs` - سرویس پرداخت POS
- `POS/PosPaymentOrchestrator.cs` - Orchestrator پرداخت POS
- `POS/PosTerminalService.cs` - مدیریت ترمینال‌ها
- `POS/PosIntegrationService.cs` - یکپارچه‌سازی POS
- `POS/CashSessionService.cs` - مدیریت جلسات صندوق
- و 4 سرویس دیگر...

**Web Payment:**
- `Web/WebPaymentService.cs` - پرداخت وب

**Gateway:**
- `Gateway/PaymentGatewayService.cs` - درگاه پرداخت

**Validation:**
- `Validation/PaymentValidationService.cs` - اعتبارسنجی پرداخت

**Reporting:**
- `Reporting/PaymentReportingService.cs` - گزارش‌گیری پرداخت

---

#### ✅ Appointment (نوبت‌گیری)

**📂 مسیر:** `Services/Appointment/`

**سرویس‌ها:**
- `AppointmentBookingService.cs` - رزرو نوبت
- `AppointmentPricingService.cs` - قیمت‌گذاری نوبت
- `AppointmentValidationService.cs` - اعتبارسنجی نوبت
- `AppointmentNotificationService.cs` - اعلان‌های نوبت

---

#### ✅ Clinic Admin (مدیریت کلینیک)

**📂 مسیر:** `Services/ClinicAdmin/`

**سرویس‌های کلیدی:**
- `DoctorAssignmentService.cs` - تخصیص پزشک
- `DoctorScheduleService.cs` - برنامه پزشک
- `DoctorDashboardService.cs` - داشبورد پزشک
- `DoctorCrudService.cs` - CRUD پزشک
- `DoctorDepartmentService.cs` - بخش‌های پزشکی
- `DoctorServiceCategoryService.cs` - دسته‌بندی خدمات پزشک
- `SpecializationService.cs` - تخصص‌ها
- `ScheduleOptimizationService.cs` - بهینه‌سازی برنامه
- `AppointmentAvailabilityService.cs` - موجودی نوبت
- `EmergencyBookingService.cs` - نوبت اضطراری
- `ClinicBankAccountService.cs` - حساب‌های بانکی

**📂 زیرپوشه:** `ScheduleOptimization/`
- **Helpers:** 3 فایل
- **Strategies:** 6 فایل
- **Validators:** 2 فایل

---

#### ✅ Triage (تریاژ)

**📂 مسیر:** `Services/Triage/`

**سرویس‌ها:**
- `ITriageService.cs` و `TriageService.cs` - سرویس تریاژ
- `ITriageQueueService.cs` و `TriageQueueService.cs` - صف تریاژ
- `TriageWorkflowIntegration.cs` - یکپارچه‌سازی Workflow

---

#### ✅ Pricing (قیمت‌گذاری)

**📂 مسیر:** `Services/Pricing/`

**سرویس‌های کلیدی:**
- `Engines/PricingEngine.cs` - موتور قیمت‌گذاری
- `Coverage/InsuranceCoverageProvider.cs` - پوشش بیمه
- `Resolvers/` - 1 فایل
- `Interfaces/` - 3 فایل
- `Models/` - 1 فایل

---

#### ✅ Notification (اعلان)

**📂 مسیر:** `Services/Notification/`

- `NotificationModule.cs` - ماژول اعلانات

**دیگر سرویس‌های مرتبط:**
- `Services/MessageNotificationService.cs`
- `Services/IMessageNotificationService.cs`
- `Services/NewsletterEmailService.cs`
- `Services/NewsletterSmsService.cs`
- `Services/AsanakSmsService.cs`

---

#### ✅ Finance (مالی)

**📂 مسیر:** `Services/Finance/`

- `DbFinancialYearService.cs` - سال مالی

**📂 مسیر:** `Services/Financial/`

- `InsuranceTariffCalculationService.cs` - محاسبات تعرفه بیمه

---

#### ✅ Calculation (محاسبات)

**📂 مسیر:** `Services/Calculation/`

- `TariffCalculator.cs` - محاسبه‌گر تعرفه

---

#### ✅ System Settings

**📂 مسیر:** `Services/SystemSettings/`

- `ISystemSettingService.cs` و `SystemSettingService.cs`

---

#### ✅ User Context

**📂 مسیر:** `Services/UserContext/`

- `IUserContextService.cs` و `UserContextService.cs`

---

#### ✅ Idempotency (یکتایی عملیات)

**📂 مسیر:** `Services/Idempotency/`

- `IIdempotencyService.cs`
- `InMemoryIdempotencyService.cs`

**استفاده:** برای جلوگیری از تکرار عملیات (مثل پرداخت)

---

#### ✅ Data Seeding

**📂 مسیر:** `Services/DataSeeding/`

- `FactorSettingSeedService.cs`
- `InsuranceTypeUpdateService.cs`
- `ServiceSeedService.cs`
- `ServiceTemplateSeedService.cs`
- `SystemSeedService.cs`

---

#### ✅ دیگر سرویس‌ها

| سرویس | توضیحات |
|------|---------|
| `HomePageService.cs` | صفحه اصلی |
| `ImageUploadService.cs` ⭐ | آپلود تصویر |
| `VideoUploadService.cs` | آپلود ویدیو |
| `DocumentUploadService.cs` | آپلود سند |
| `AuthService.cs` | احراز هویت |
| `CurrentUserService.cs` | کاربر جاری |
| `BackgroundCurrentUserService.cs` | کاربر جاری (Background) |
| `SecurityTokenService.cs` | توکن امنیتی |
| `PatientService.cs` | بیماران |
| `ClinicManagementService.cs` | مدیریت کلینیک |
| `DepartmentManagementService.cs` | مدیریت بخش‌ها |
| `ServiceService.cs` | خدمات |
| `ServiceCategoryService.cs` | دسته‌بندی خدمات |
| `ServiceManagementService.cs` | مدیریت خدمات |
| `SharedServiceManagementService.cs` | مدیریت خدمات مشترک |
| `ServiceTemplateService.cs` | قالب خدمات |
| `FactorSettingService.cs` | تنظیمات فاکتور |
| `ExternalInquiryService.cs` | استعلام خارجی |
| `ShiftHelperService.cs` | Helper شیفت |

---

### 📊 خلاصه Services

| دسته‌بندی | تعداد | نکته |
|---------|------|------|
| **CMS** | 22 | سیستم مدیریت محتوا |
| **Insurance** | 29 | سیستم بیمه جامع |
| **Reception** | 37 | ماژول پذیرش (شامل `ReceptionFacade`) |
| **Payment** | 10+ | پرداخت (POS, Web, Gateway) |
| **Appointment** | 4 | نوبت‌گیری |
| **Clinic Admin** | 11+ | مدیریت کلینیک |
| **Triage** | 5 | تریاژ |
| **Pricing** | 5+ | قیمت‌گذاری |
| **Notification** | 5+ | اعلانات |
| **Finance** | 2 | مالی |
| **دیگر** | 20+ | سرویس‌های عمومی |

**تعداد کل:** 150+ سرویس

**نکته:** 🚫 **قبل از ایجاد سرویس جدید، حتماً بررسی کنید که سرویس مشابهی وجود ندارد!**

---

## 5️⃣ Extensions - متدهای توسعه

### 📂 مسیر: `Extensions/`

#### ✅ Extensions موجود

| فایل | توضیحات | مثال استفاده |
|------|---------|--------------|
| `ApplicationUserManagerExtensions.cs` | Extension Methods برای UserManager | `userManager.FindByIdAsync(userId)` |
| `CultureExtensions.cs` | فرهنگ و زبان | `SetPersianCulture()` |
| `DateTimeExtensions.cs` ⭐ | Extension برای DateTime | `date.ToPersianDateString()` |
| `EnumExtensions.cs` ⭐ | Extension برای Enum | `myEnum.GetDisplayName()` |
| `GenderParsing.cs` | Parse کردن جنسیت | `ParseGender("مرد")` |
| `PersianDateExtensions.cs` ⭐ | Extension برای تاریخ شمسی | `date.ToPersianDate()` |

**استفاده:**
```csharp
using ClinicApp.Extensions;

// DateTime Extensions
var persianDate = DateTime.Now.ToPersianDateString();

// Enum Extensions
var displayName = MyEnum.Value.GetDisplayName();

// Gender Parsing
var gender = GenderParsing.ParseGender("مرد"); // => Gender.Male
```

---

## 6️⃣ Partial Views - ویوهای جزئی

### 📂 مسیر: `Areas/Admin/Views/Shared/`

#### ✅ Partial Views موجود

| فایل | توضیحات | استفاده |
|------|---------|---------|
| `_AdminLayout.cshtml` | Layout ادمین | `Layout = "~/Areas/Admin/Views/Shared/_AdminLayout.cshtml";` |
| `_PersianDatePicker.cshtml` ⭐ | DatePicker شمسی | `@Html.Partial("_PersianDatePicker")` |
| `_PersianDatePickerScript.cshtml` ⭐ | Script DatePicker | `@Html.Partial("_PersianDatePickerScript")` |
| `_CKEditorScript.cshtml` ⭐ | Script CKEditor | `@Html.Partial("_CKEditorScript")` |
| `_CKEditorInit.cshtml` ⭐ | Initialize CKEditor | `@Html.Partial("_CKEditorInit")` |
| `_NotificationMessages.cshtml` | پیام‌های Notification | خودکار در `_AdminLayout.cshtml` |
| `_PatientInsuranceSelector.cshtml` | انتخاب بیمه بیمار | `@Html.Partial("_PatientInsuranceSelector", model)` |
| `_PatientInsuranceStatusCard.cshtml` | کارت وضعیت بیمه | `@Html.Partial("_PatientInsuranceStatusCard", model)` |
| `_ReceptionInsuranceSelector.cshtml` | انتخاب بیمه در پذیرش | `@Html.Partial("_ReceptionInsuranceSelector", model)` |
| `_Breadcrumb.cshtml` | Breadcrumb | `@Html.Partial("_Breadcrumb")` |
| `_CMSMenu.cshtml` | منوی CMS | `@Html.Partial("_CMSMenu")` |
| `_DoctorsListPartial.cshtml` | لیست پزشکان | `@Html.Partial("_DoctorsListPartial", doctors)` |
| `_ClinicsListPartial.cshtml` | لیست کلینیک‌ها | `@Html.Partial("_ClinicsListPartial", clinics)` |
| `_DepartmentsListPartial.cshtml` | لیست بخش‌ها | `@Html.Partial("_DepartmentsListPartial", departments)` |
| `_SuccessPartial.cshtml` | پیام موفقیت | `@Html.Partial("_SuccessPartial")` |
| `_ErrorPartial.cshtml` | پیام خطا | `@Html.Partial("_ErrorPartial")` |

---

### 📂 مسیر: `Views/Shared/`

| فایل | توضیحات |
|------|---------|
| `_Layout.cshtml` | Layout اصلی سایت |
| `_ReceptionLayout.cshtml` ⭐ **جدید** | Layout اختصاصی پذیرش |
| `_Breadcrumb.cshtml` | Breadcrumb عمومی |
| دیگر Partial Views... | |

---

## 7️⃣ راهنمای استفاده سریع

### ✅ سناریو 1: نیاز به Notification

**قبل از کد زدن:**
```
✅ استفاده کن از:
   - Backend: NotificationHelper.cs
   - Frontend (Admin): admin-notification-service.js
   - Frontend (Public/Reception): notification-helper.js
```

**مثال:**
```csharp
// Controller
NotificationHelper.SetSuccess(TempData, "عملیات موفق");
```

```javascript
// JavaScript (Admin)
AdminNotification.success('عملیات موفق');

// JavaScript (Reception/Public)
Notify.success('عملیات موفق');
```

---

### ✅ سناریو 2: نیاز به تاریخ شمسی

**قبل از کد زدن:**
```
✅ استفاده کن از:
   - Backend: PersianDateHelper.cs, ControllerExtensions.cs
   - Frontend: _PersianDatePicker.cshtml + _PersianDatePickerScript.cshtml
   - JavaScript: persian-datepicker-manager.js
```

**مثال:**
```csharp
// Controller - Parse
model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);

// Controller - Display
var persianDate = PersianDateHelper.ToPersianDate(model.StartDate);
```

```razor
@* View - Input *@
@{
    ViewBag.PersianDatePickerId = "startDatePicker";
    ViewBag.PersianDatePickerName = "StartDate";
    ViewBag.PersianDatePickerLabel = "تاریخ شروع";
}
@Html.Partial("_PersianDatePicker")
@Html.Partial("_PersianDatePickerScript")
```

---

### ✅ سناریو 3: نیاز به ویرایشگر متن (CKEditor)

**قبل از کد زدن:**
```
✅ استفاده کن از:
   - Partial Views: _CKEditorScript.cshtml + _CKEditorInit.cshtml
   - ViewModel: [AllowHtml] attribute
   - Controller: [ValidateInput(false)] attribute
```

**مثال:**
```csharp
// ViewModel
[AllowHtml]
public string Content { get; set; }

// Controller
[ValidateInput(false)]
public async Task<ActionResult> Create(MyViewModel model)
{
    // ...
}
```

```razor
@* View *@
@Html.TextAreaFor(m => m.Content, new { id = "contentEditor", rows = "10" })

@section Scripts {
    @Html.Partial("_CKEditorScript")
    @{
        ViewBag.CKEditorSelector = "#contentEditor";
        ViewBag.CKEditorHeight = 400;
    }
    @Html.Partial("_CKEditorInit")
}
```

---

### ✅ سناریو 4: نیاز به آپلود تصویر

**قبل از کد زدن:**
```
✅ استفاده کن از:
   - Service: IImageUploadService
   - Helper: ImagePathHelper.cs
```

**مثال:**
```csharp
// Controller
private readonly IImageUploadService _imageUploadService;

private async Task ProcessImageUpload(MyViewModel model)
{
    var imageFile = Request.Files["ImageFile"];
    if (imageFile != null && imageFile.ContentLength > 0)
    {
        var uploadResult = _imageUploadService.UploadImageWithThumbnail(
            imageFile,
            "~/Content/Images/my-module",
            "~/Content/Images/my-module/thumbnails",
            300, 300, 1920, 1080
        );
        
        if (uploadResult.Success)
        {
            model.ImageUrl = uploadResult.Data.ImageUrl;
            model.ThumbnailUrl = uploadResult.Data.ThumbnailUrl;
        }
    }
}
```

---

### ✅ سناریو 5: نیاز به Validation

**قبل از کد زدن:**
```
✅ استفاده کن از:
   - کد ملی: IranianNationalCodeValidator.cs
   - شماره تماس: PhoneNumberValidator.cs
   - شماره تماس Helper: PhoneNumberHelper.cs
```

**مثال:**
```csharp
// Validation کد ملی
if (!IranianNationalCodeValidator.IsValid(model.NationalCode))
{
    ModelState.AddModelError("NationalCode", "کد ملی نامعتبر است");
}

// Validation شماره موبایل
if (!PhoneNumberValidator.IsValidMobile(model.PhoneNumber))
{
    ModelState.AddModelError("PhoneNumber", "شماره موبایل نامعتبر است");
}

// Format شماره تماس
var formatted = PhoneNumberHelper.FormatMobile(model.PhoneNumber);
```

---

### ✅ سناریو 6: نیاز به محاسبه سن

**قبل از کد زدن:**
```
✅ استفاده کن از:
   - Helper: AgeCalculationHelper.cs
```

**مثال:**
```csharp
var age = AgeCalculationHelper.CalculateAge(patient.BirthDate);
var ageString = AgeCalculationHelper.CalculateAgeString(patient.BirthDate); // "۳۵ سال"
```

---

### ✅ سناریو 7: نیاز به String Helper

**قبل از کد زدن:**
```
✅ استفاده کن از:
   - Helper: StringHelper.cs
   - Helper: PersianNumberHelper.cs
```

**مثال:**
```csharp
// حذف HTML
var clean = StringHelper.StripHtml(htmlContent);

// Truncate
var summary = StringHelper.StripHtmlAndTruncate(htmlContent, 200);

// تبدیل اعداد
var persian = PersianNumberHelper.ToPersianNumber("1234"); // "۱۲۳۴"
var english = PersianNumberHelper.ToEnglishNumber("۱۲۳۴"); // "1234"
```

---

## 📌 نکات نهایی

### ✅ قبل از ایجاد هر چیز جدید:

1. **بررسی این مستند**
2. **جستجو در پروژه**: `Ctrl+Shift+F` در Visual Studio
3. **مشورت با تیم**

### ✅ اصول طلایی:

1. 🚫 **هرگز ماژول تکراری نسازید**
2. ✅ **از Helper های موجود استفاده کنید**
3. ✅ **از Partial View های موجود استفاده کنید**
4. ✅ **از CSS های استاندارد استفاده کنید** (`local-fonts.css`, `medical-environment.css`)
5. ✅ **از JavaScript های موجود استفاده کنید**
6. ✅ **قبل از کد زدن، این مستند را بخوانید**

---

## 📚 مراجع مرتبط

- `Docs/DEVELOPMENT_CONTRACT.md` - قرارداد توسعه
- `Docs/TODO_TEMPLATE.md` - Template TODO
- `Docs/NOTIFICATION_HELPER_USAGE_GUIDE.md` - راهنمای Notification
- `Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md` - راهنمای DatePicker
- `Docs/IMAGE_UPLOAD_SYSTEM_GUIDE.md` - راهنمای آپلود تصویر
- `Docs/CKEDITOR_USAGE_GUIDE.md` - راهنمای CKEditor
- `Docs/RECEPTION_V2_PAYMENT_POS_COMPLETE_ANALYSIS.md` - آنالیز پذیرش و پرداخت

---

**تاریخ تکمیل:** 1404/10/05  
**وضعیت:** ✅ **به‌روز و فعال**  
**نگارش:** 1.0.0

---

🎉 **این مستند باید همیشه به‌روز باشد!** 🎉

**در صورت اضافه شدن ماژول جدید، حتماً این مستند را به‌روزرسانی کنید.**

