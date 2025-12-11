# 📋 Template TODO List
## برای استفاده در تمام توسعه‌های جدید

---

## 📌 دستورالعمل استفاده

این template را برای هر ماژول/feature جدید کپی کنید و طبق مراحل زیر پیش بروید.

---

## Phase 1: Analysis & Design (تحلیل و طراحی)

### 1.1 Requirements Analysis
- [ ] تحلیل نیازمندی‌های کاربر
- [ ] تعریف User Stories
- [ ] تعریف Acceptance Criteria
- [ ] تعریف Technical Requirements

### 1.2 Entity & Database Design
- [ ] طراحی Entity Model
- [ ] طراحی Database Schema
- [ ] تعریف Relationships
- [ ] تعریف Indexes
- [ ] ایجاد Migration Script

### 1.3 ViewModel Design
- [ ] طراحی Index ViewModel
- [ ] طراحی Create/Edit ViewModel
- [ ] طراحی Details ViewModel
- [ ] طراحی Search/Filter ViewModel
- [ ] تعریف Data Annotations

### 1.4 Interface Design
- [ ] طراحی Repository Interface
- [ ] طراحی Service Interface
- [ ] تعریف Method Signatures
- [ ] تعریف Return Types

---

## Phase 2: Backend Implementation (پیاده‌سازی Backend)

### 2.1 Repository Implementation
- [ ] پیاده‌سازی Repository Class
- [ ] پیاده‌سازی GetByIdAsync
- [ ] پیاده‌سازی GetAllAsync
- [ ] پیاده‌سازی Add/Update/Delete
- [ ] پیاده‌سازی Search/Filter Methods
- [ ] تست Repository Methods

### 2.2 Service Implementation
- [ ] پیاده‌سازی Service Class
- [ ] پیاده‌سازی Business Logic
- [ ] پیاده‌سازی Validation
- [ ] پیاده‌سازی Error Handling
- [ ] پیاده‌سازی Logging
- [ ] تست Service Methods

### 2.3 Dependency Injection
- [ ] ثبت Repository در UnityConfig
- [ ] ثبت Service در UnityConfig
- [ ] تست Dependency Injection

---

## Phase 3: Controller Implementation (پیاده‌سازی Controller)

### 3.1 Controller Setup
- [ ] ایجاد Controller Class
- [ ] تزریق Dependencies
- [ ] تعریف Route Attributes
- [ ] تعریف Authorization

### 3.2 CRUD Actions
- [ ] پیاده‌سازی Index Action
- [ ] پیاده‌سازی Create (GET) Action
- [ ] پیاده‌سازی Create (POST) Action
  - [ ] Parse کردن تاریخ‌ها از hidden input با ParseDateFromHiddenInput
- [ ] پیاده‌سازی Edit (GET) Action
- [ ] پیاده‌سازی Edit (POST) Action
  - [ ] Parse کردن تاریخ‌ها از hidden input با ParseDateFromHiddenInput
- [ ] پیاده‌سازی Details Action
- [ ] پیاده‌سازی Delete Action

### 3.3 Additional Actions
- [ ] پیاده‌سازی Search/Filter Actions
- [ ] پیاده‌سازی Publish/Unpublish Actions
- [ ] پیاده‌سازی SetFeatured Action
- [ ] پیاده‌سازی Export Actions

### 3.4 Notification Integration
- [ ] استفاده از NotificationHelper.SetSuccess
- [ ] استفاده از NotificationHelper.SetError
- [ ] استفاده از NotificationHelper.SetWarning
- [ ] استفاده از NotificationHelper.SetInfo
- [ ] حذف تمام TempData مستقیم

### 3.5 Strongly-Typed ViewModel
- [ ] ایجاد ViewModel برای Index (در صورت نیاز به داده‌های اضافی)
- [ ] حذف تمام استفاده از ViewBag برای داده‌های اصلی
- [ ] حذف تمام استفاده از ViewData برای داده‌های اصلی
- [ ] استفاده از ViewModel برای تمام داده‌های ضروری
- [ ] استفاده از ViewBag فقط برای ViewBag.Title و ViewBag.MetaDescription (مجاز)

---

## Phase 4: View Implementation (پیاده‌سازی View)

### 4.1 Index View
- [ ] ایجاد Index.cshtml
- [ ] پیاده‌سازی Search Panel
- [ ] پیاده‌سازی Data Table
- [ ] پیاده‌سازی Pagination
- [ ] پیاده‌سازی Action Buttons
- [ ] حذف Alert های Bootstrap
- [ ] اضافه کردن SweetAlert برای Confirmations

### 4.2 Create View
- [ ] ایجاد Create.cshtml
- [ ] پیاده‌سازی Form
- [ ] پیاده‌سازی Validation Messages
- [ ] پیاده‌سازی Persian DatePicker (استفاده از _PersianDatePicker)
- [ ] اضافه کردن _PersianDatePickerScript به Scripts
- [ ] پیاده‌سازی CKEditor (در صورت نیاز به فیلدهای متنی طولانی)
- [ ] اضافه کردن _CKEditorScript و _CKEditorInit به Scripts
- [ ] پیاده‌سازی Image Upload (در صورت نیاز)
- [ ] حذف Alert های Bootstrap

### 4.3 Edit View
- [ ] ایجاد Edit.cshtml
- [ ] پیاده‌سازی Form
- [ ] پیاده‌سازی Pre-filled Values
- [ ] پیاده‌سازی Validation Messages
- [ ] پیاده‌سازی Persian DatePicker (استفاده از _PersianDatePicker)
- [ ] اضافه کردن _PersianDatePickerScript به Scripts
- [ ] پیاده‌سازی CKEditor (در صورت نیاز به فیلدهای متنی طولانی)
- [ ] اضافه کردن _CKEditorScript و _CKEditorInit به Scripts
- [ ] پیاده‌سازی Image Upload (در صورت نیاز)
- [ ] حذف Alert های Bootstrap

### 4.4 Details View
- [ ] ایجاد Details.cshtml
- [ ] پیاده‌سازی Display Template
- [ ] پیاده‌سازی Action Buttons
- [ ] پیاده‌سازی Related Data Display

---

## Phase 5: UI/UX Optimization (بهینه‌سازی UI/UX)

### 5.1 Design Consistency
- [ ] استفاده از فونت Vazir
- [ ] استفاده از Card Components
- [ ] استفاده از Button Styles
- [ ] استفاده از Form Styles

### 5.2 Responsive Design
- [ ] تست Mobile View
- [ ] تست Tablet View
- [ ] تست Desktop View
- [ ] بهینه‌سازی Table Responsive
- [ ] بهینه‌سازی Form Responsive

### 5.3 Accessibility
- [ ] اضافه کردن Alt Text برای Images
- [ ] اضافه کردن Title برای Links
- [ ] اضافه کردن ARIA Labels
- [ ] تست Keyboard Navigation
- [ ] تست Screen Reader

---

## Phase 6: Color Scheme Standardization (استانداردسازی رنگ‌بندی)

### 6.1 بررسی و حذف رنگ‌های جیق و جلف
- [ ] بررسی تمام Gradient های رنگی
- [ ] حذف تمام `linear-gradient` های پیچیده
- [ ] حذف رنگ‌های روشن و جیق (مثل `#f093fb`, `#f5576c`)
- [ ] حذف رنگ‌های نئون و درخشان
- [ ] بررسی Border-radius های بزرگ (20px+)

### 6.2 پیاده‌سازی پالت رنگ استاندارد
- [ ] تعریف CSS Variables برای رنگ‌های استاندارد:
  - [ ] `--medical-primary: #2c5aa0`
  - [ ] `--medical-secondary: #6c757d`
  - [ ] `--medical-success: #28a745`
  - [ ] `--medical-danger: #dc3545`
  - [ ] `--medical-warning: #ffc107`
  - [ ] `--medical-info: #17a2b8`
  - [ ] `--medical-light: #f8f9fa`
  - [ ] `--medical-bg: #ffffff`
  - [ ] `--medical-dark: #212529`
  - [ ] `--medical-text: #212529`
  - [ ] `--medical-text-muted: #6c757d`
  - [ ] `--medical-border: #dee2e6`
- [ ] استفاده از CSS Variables در تمام استایل‌ها

### 6.3 بهینه‌سازی Header و Card Header
- [ ] جایگزینی Gradient با `background-color: var(--medical-primary)`
- [ ] استفاده از رنگ سفید برای متن
- [ ] Border-radius مناسب (12px حداکثر)

### 6.4 بهینه‌سازی Badge و Label
- [ ] استفاده از رنگ‌های ساده (نه Gradient)
- [ ] Border-radius مناسب (6px)
- [ ] استفاده از `--medical-primary` برای Badge اصلی

### 6.5 بهینه‌سازی Button
- [ ] استفاده از `background-color` (نه Gradient)
- [ ] Border-radius مناسب (6px)
- [ ] استفاده از `--medical-primary` برای Button اصلی

### 6.6 بهینه‌سازی Card و Container
- [ ] استفاده از `background-color: var(--medical-bg)`
- [ ] Border ساده: `1px solid var(--medical-border)`
- [ ] Border-radius مناسب (12px حداکثر)
- [ ] Box-shadow ملایم

### 6.7 Testing
- [ ] بررسی تمام صفحات برای رنگ‌های جیق و جلف
- [ ] اطمینان از یکنواختی رنگ‌بندی
- [ ] بررسی Contrast Ratio برای خوانایی
- [ ] تست در محیط Production

**زمان تخمینی:** 0.5-1 روز

---

## Phase 7: Notification System (سیستم پیام‌ها)

### 7.1 Toastr Integration
- [ ] حذف تمام Alert های Bootstrap
- [ ] اطمینان از لود شدن Toastr
- [ ] تست Success Messages
- [ ] تست Error Messages
- [ ] تست Warning Messages
- [ ] تست Info Messages

### 7.2 SweetAlert Integration
- [ ] جایگزینی confirm() با SweetAlert
- [ ] پیاده‌سازی Delete Confirmation
- [ ] پیاده‌سازی Publish Confirmation
- [ ] پیاده‌سازی Unpublish Confirmation
- [ ] تست تمام Confirmations

---

## Phase 8: Persian DatePicker Integration (یکپارچه‌سازی تقویم شمسی)

### 8.1 View Implementation
- [ ] شناسایی تمام فیلدهای تاریخ در فرم
- [ ] جایگزینی datetime-local با _PersianDatePicker
- [ ] تنظیم ViewBag برای هر فیلد تاریخ:
  - [ ] PersianDatePickerId
  - [ ] PersianDatePickerName
  - [ ] PersianDatePickerValue
  - [ ] PersianDatePickerLabel
  - [ ] PersianDatePickerPlaceholder
  - [ ] PersianDatePickerHelpText
  - [ ] PersianDatePickerRequired
- [ ] اضافه کردن _PersianDatePickerScript به Scripts section
- [ ] حذف تمام استفاده از datetime-local

### 8.2 Controller Implementation
- [ ] اضافه کردن using ClinicApp.Helpers
- [ ] Parse کردن تاریخ‌ها در Create Action:
  - [ ] model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
  - [ ] model.EndDate = this.ParseDateFromHiddenInput("EndDate", _logger);
- [ ] Parse کردن تاریخ‌ها در Edit Action:
  - [ ] model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
  - [ ] model.EndDate = this.ParseDateFromHiddenInput("EndDate", _logger);

### 8.3 Display Implementation
- [ ] به‌روزرسانی Index View برای نمایش تاریخ شمسی:
  - [ ] استفاده از PersianDateHelper.ToPersianDate(item.Date)
- [ ] به‌روزرسانی Details View برای نمایش تاریخ شمسی:
  - [ ] استفاده از PersianDateHelper.ToPersianDate(Model.Date)

### 8.4 Testing
- [ ] تست انتخاب تاریخ در Create Form
- [ ] تست ذخیره تاریخ در دیتابیس
- [ ] تست نمایش تاریخ در Edit Form
- [ ] تست به‌روزرسانی تاریخ
- [ ] تست نمایش تاریخ در Index
- [ ] تست نمایش تاریخ در Details

---

## Phase 9: CKEditor Integration (یکپارچه‌سازی ویرایشگر متن)

### 9.1 بررسی نیاز به CKEditor
- [ ] آیا فیلد متنی طولانی است؟
- [ ] آیا نیاز به فرمت‌بندی متن است؟
- [ ] آیا محتوا شامل HTML است؟

### 9.2 ViewModel Configuration
- [ ] اضافه کردن `[AllowHtml]` به فیلدهای HTML
- [ ] اضافه کردن Validation Attributes
- [ ] اضافه کردن Display Names

### 9.3 Controller Configuration
- [ ] اضافه کردن `[ValidateInput(false)]` به POST Actions
- [ ] بررسی ModelState برای فیلدهای HTML
- [ ] مدیریت خطاهای HTML

### 9.4 View Implementation (Create)
- [ ] اضافه کردن `@Html.Partial("_CKEditorScript")` در `@section Scripts`
- [ ] ایجاد TextArea با ID منحصر به فرد
- [ ] اضافه کردن `@Html.Partial("_CKEditorInit")` با selector و height
- [ ] اضافه کردن Validation Messages
- [ ] اضافه کردن Help Text

### 9.5 View Implementation (Edit)
- [ ] اضافه کردن `@Html.Partial("_CKEditorScript")` در `@section Scripts`
- [ ] ایجاد TextArea با ID منحصر به فرد
- [ ] اضافه کردن `@Html.Partial("_CKEditorInit")` با selector و height
- [ ] نمایش محتوای موجود در CKEditor
- [ ] اضافه کردن Validation Messages

### 9.6 Display Implementation (Index/Details)
- [ ] استفاده از `Html.Raw()` برای نمایش HTML
- [ ] استفاده از `StringHelper.StripHtmlAndTruncate()` برای خلاصه (در صورت نیاز)
- [ ] بررسی XSS Protection

### 9.7 Testing
- [ ] تست بارگذاری CKEditor
- [ ] تست فرمت‌بندی فارسی
- [ ] تست جهت راست‌به‌چپ
- [ ] تست ذخیره محتوا
- [ ] تست نمایش محتوا
- [ ] تست Validation

**زمان تخمینی:** 0.5-1 روز

---

## Phase 10: Image Upload System (سیستم آپلود تصویر)

### 10.1 Service Integration
- [ ] بررسی ثبت IImageUploadService در UnityConfig
- [ ] اطمینان از وجود ImageUploadService در پروژه

### 10.2 Controller Implementation
- [ ] تزریق IImageUploadService در Constructor
- [ ] تعریف Constants برای مسیرها:
  - [ ] ImageUploadPath (مثلاً ~/Content/Images/module-name)
  - [ ] ThumbnailUploadPath (مثلاً ~/Content/Images/module-name/thumbnails)
  - [ ] ThumbnailWidth (300)
  - [ ] ThumbnailHeight (300)
  - [ ] MaxImageWidth (1920)
  - [ ] MaxImageHeight (1080)
- [ ] پیاده‌سازی متد ProcessImageUpload:
  - [ ] دریافت ImageFile از Request.Files
  - [ ] دریافت ThumbnailFile از Request.Files (اختیاری)
  - [ ] فراخوانی UploadImageWithThumbnail برای تصویر اصلی
  - [ ] بررسی Success و مدیریت خطا
  - [ ] تنظیم model.ImageUrl و model.ThumbnailUrl
  - [ ] فراخوانی UploadImageWithThumbnail برای thumbnail جداگانه (در صورت نیاز)
  - [ ] Error Handling و Logging
- [ ] فراخوانی ProcessImageUpload در Create Action (قبل از ModelState.IsValid)
- [ ] فراخوانی ProcessImageUpload در Edit Action (قبل از ModelState.IsValid)

### 10.3 View Implementation
- [ ] اضافه کردن enctype="multipart/form-data" به Form
- [ ] اضافه کردن File Input برای تصویر اصلی:
  - [ ] id="ImageFile" name="ImageFile"
  - [ ] accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
  - [ ] class="custom-file-input"
  - [ ] label با class="custom-file-label"
- [ ] اضافه کردن Help Text:
  - [ ] فرمت‌های مجاز
  - [ ] حداکثر حجم (5 مگابایت)
  - [ ] ابعاد توصیه شده
- [ ] اضافه کردن HiddenFor برای ImageUrl
- [ ] اضافه کردن ValidationMessageFor برای ImageUrl
- [ ] اضافه کردن Image Preview:
  - [ ] div با id="imagePreview"
  - [ ] img با id="imagePreviewImg"
  - [ ] نمایش/مخفی کردن بر اساس انتخاب فایل
- [ ] اضافه کردن نمایش تصویر فعلی در Edit View:
  - [ ] بررسی !string.IsNullOrEmpty(Model.ImageUrl)
  - [ ] نمایش تصویر با img-thumbnail class
- [ ] اضافه کردن File Input برای Thumbnail (اختیاری):
  - [ ] همان ساختار تصویر اصلی
  - [ ] id="ThumbnailFile" name="ThumbnailFile"
- [ ] اضافه کردن JavaScript برای Preview و Validation:
  - [ ] بررسی نوع فایل (image.*)
  - [ ] بررسی حجم فایل (5 مگابایت)
  - [ ] نمایش Preview با FileReader
  - [ ] به‌روزرسانی Label با نام فایل
  - [ ] استفاده از AdminNotification برای خطاها

### 10.4 Testing
- [ ] تست آپلود تصویر اصلی در Create
- [ ] تست ایجاد thumbnail خودکار
- [ ] تست آپلود thumbnail جداگانه
- [ ] تست نمایش Preview
- [ ] تست Validation نوع فایل
- [ ] تست Validation حجم فایل
- [ ] تست نمایش تصویر فعلی در Edit
- [ ] تست به‌روزرسانی تصویر در Edit
- [ ] تست حذف تصویر قدیمی هنگام آپلود جدید

---

## Phase 11: Testing & Quality Assurance (تست و کنترل کیفیت)

### 11.1 Unit Testing
- [ ] تست Repository Methods
- [ ] تست Service Methods
- [ ] تست Controller Actions
- [ ] تست ViewModels

### 11.2 Integration Testing
- [ ] تست End-to-End Flows
- [ ] تست Database Operations
- [ ] تست File Upload Operations
- [ ] تست Notification System

### 11.3 Security Testing
- [ ] تست SQL Injection Protection
- [ ] تست XSS Protection
- [ ] تست CSRF Protection
- [ ] تست Authorization Checks

### 11.4 Performance Testing
- [ ] تست Page Load Time
- [ ] تست Database Query Performance
- [ ] تست Image Upload Performance
- [ ] تست Search/Filter Performance

---

## Phase 12: Code Review & Optimization (بازبینی و بهینه‌سازی)

### 12.1 Code Review Checklist
- [ ] بررسی Strongly-Typed
- [ ] بررسی SRP Principles
- [ ] بررسی Error Handling
- [ ] بررسی Logging
- [ ] بررسی Security

### 12.2 Code Optimization
- [ ] بهینه‌سازی Database Queries
- [ ] حذف N+1 Queries
- [ ] بهینه‌سازی JavaScript
- [ ] بهینه‌سازی CSS
- [ ] Minification Resources

### 12.3 Documentation
- [ ] به‌روزرسانی Code Comments
- [ ] ایجاد API Documentation
- [ ] به‌روزرسانی README
- [ ] ایجاد User Guide

---

## Phase 13: Deployment Preparation (آماده‌سازی برای Production)

### 11.1 Pre-Deployment Checklist
- [ ] تمام Tests پاس شده‌اند
- [ ] تمام Linter Errors برطرف شده‌اند
- [ ] تمام Warnings برطرف شده‌اند
- [ ] تمام TODOs بررسی شده‌اند
- [ ] تمام Documentation به‌روز است

### 11.2 Production Configuration
- [ ] تنظیم Connection Strings
- [ ] تنظیم Logging Levels
- [ ] تنظیم Error Pages
- [ ] تنظیم Caching
- [ ] تنظیم Security Headers

### 11.3 Deployment
- [ ] Backup Database
- [ ] Deploy Application
- [ ] Run Migrations
- [ ] Test Production Environment
- [ ] Monitor Logs

---

## 📝 Notes

### نکات مهم:
1. هر Phase باید به صورت کامل انجام شود قبل از رفتن به Phase بعدی
2. تمام Checklist ها باید بررسی شوند
3. در صورت نیاز به تغییر، باید با تیم فنی هماهنگ شود
4. تمام تغییرات باید طبق `DEVELOPMENT_CONTRACT.md` انجام شوند

### زمان‌بندی پیشنهادی:
- Phase 1-2: 2-3 روز
- Phase 3-4: 3-4 روز
- Phase 5: 1-2 روز (UI/UX Optimization)
- Phase 6: 0.5-1 روز (Color Scheme Standardization)
- Phase 7: 1 روز (Notification System)
- Phase 8: 1 روز (Persian DatePicker Integration)
- Phase 9: 0.5-1 روز (CKEditor Integration)
- Phase 10: 1-2 روز (Image Upload System)
- Phase 11-12: 2-3 روز
- Phase 13: 1 روز

**کل زمان:** 12-17 روز کاری

---

## ✅ Sign-off

- [ ] تمام مراحل تکمیل شده‌اند
- [ ] تمام Checklist ها بررسی شده‌اند
- [ ] Code Review انجام شده است
- [ ] آماده برای Production است

**تاریخ تکمیل:** ___________  
**تایید کننده:** ___________

