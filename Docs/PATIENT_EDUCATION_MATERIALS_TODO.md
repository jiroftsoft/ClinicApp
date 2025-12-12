# 📋 TODO List - Patient Education Materials Module
## مطالب آموزشی بیماران - کلینیک درمانی شفا جیرفت

**تاریخ شروع:** 2025-12-12  
**وضعیت:** در حال پیاده‌سازی  
**اولویت:** ⭐⭐⭐⭐ (بالا)

---

## Phase 1: Analysis & Design (تحلیل و طراحی)

### 1.1 Requirements Analysis ✅
- [x] تحلیل نیازمندی‌های کاربر
- [x] تعریف User Stories
- [x] تعریف Acceptance Criteria
- [x] تعریف Technical Requirements

### 1.2 Entity & Database Design
- [ ] طراحی Entity Model (PatientEducationMaterial)
- [ ] طراحی Database Schema
- [ ] تعریف Relationships
- [ ] تعریف Indexes
- [ ] ایجاد Migration Script

### 1.3 ViewModel Design
- [ ] طراحی PatientEducationMaterialIndexViewModel
- [ ] طراحی PatientEducationMaterialCreateEditViewModel
- [ ] طراحی PatientEducationMaterialDetailsViewModel
- [ ] طراحی PatientEducationMaterialSearchViewModel
- [ ] تعریف Data Annotations
- [ ] تعریف Enum برای Category

### 1.4 Interface Design
- [ ] طراحی IPatientEducationMaterialRepository Interface
- [ ] طراحی IPatientEducationMaterialService Interface
- [ ] تعریف Method Signatures
- [ ] تعریف Return Types

---

## Phase 2: Backend Implementation (پیاده‌سازی Backend)

### 2.1 Repository Implementation
- [ ] پیاده‌سازی PatientEducationMaterialRepository Class
- [ ] پیاده‌سازی GetByIdAsync
- [ ] پیاده‌سازی GetAllAsync
- [ ] پیاده‌سازی GetPublishedAsync
- [ ] پیاده‌سازی GetByCategoryAsync
- [ ] پیاده‌سازی SearchAsync
- [ ] پیاده‌سازی IncrementDownloadCountAsync
- [ ] پیاده‌سازی IncrementViewCountAsync
- [ ] پیاده‌سازی Add/Update/Delete
- [ ] تست Repository Methods

### 2.2 Service Implementation
- [ ] پیاده‌سازی PatientEducationMaterialService Class
- [ ] پیاده‌سازی Business Logic
- [ ] پیاده‌سازی Validation
- [ ] پیاده‌سازی Error Handling
- [ ] پیاده‌سازی Logging
- [ ] تست Service Methods

### 2.3 Dependency Injection
- [ ] ثبت IPatientEducationMaterialRepository در UnityConfig
- [ ] ثبت IPatientEducationMaterialService در UnityConfig
- [ ] تست Dependency Injection

---

## Phase 3: Controller Implementation (پیاده‌سازی Controller)

### 3.1 Controller Setup
- [ ] ایجاد PatientEducationMaterialController Class
- [ ] ارث‌بری از BaseCMSController
- [ ] تزریق Dependencies (IPatientEducationMaterialService, ICurrentUserService, IImageUploadService, ILogger)
- [ ] تعریف Route Attributes
- [ ] تعریف Authorization ([Authorize(Roles = "Admin")])

### 3.2 CRUD Actions
- [ ] پیاده‌سازی Index Action (GET)
  - [ ] فیلتر بر اساس Category
  - [ ] فیلتر بر اساس IsPublished
  - [ ] جستجو
  - [ ] Pagination
- [ ] پیاده‌سازی Create (GET) Action
- [ ] پیاده‌سازی Create (POST) Action
  - [ ] Parse کردن تاریخ‌ها از hidden input با ParseDateFromHiddenInput
  - [ ] پردازش آپلود فایل (PDF/Word/Excel)
  - [ ] پردازش آپلود تصویر (در صورت نیاز)
- [ ] پیاده‌سازی Edit (GET) Action
- [ ] پیاده‌سازی Edit (POST) Action
  - [ ] Parse کردن تاریخ‌ها از hidden input با ParseDateFromHiddenInput
  - [ ] پردازش آپلود فایل
  - [ ] پردازش آپلود تصویر
- [ ] پیاده‌سازی Details Action
- [ ] پیاده‌سازی Delete Action

### 3.3 Additional Actions
- [ ] پیاده‌سازی Download Action (برای افزایش DownloadCount)
- [ ] پیاده‌سازی View Action (برای افزایش ViewCount)
- [ ] پیاده‌سازی Publish/Unpublish Actions
- [ ] پیاده‌سازی SetFeatured Action

### 3.4 Notification Integration
- [ ] استفاده از NotificationHelper.SetSuccess
- [ ] استفاده از NotificationHelper.SetError
- [ ] استفاده از NotificationHelper.SetWarning
- [ ] استفاده از NotificationHelper.SetInfo
- [ ] حذف تمام TempData مستقیم

### 3.5 Strongly-Typed ViewModel
- [ ] ایجاد ViewModel برای Index (PatientEducationMaterialIndexPageViewModel)
- [ ] حذف تمام استفاده از ViewBag برای داده‌های اصلی
- [ ] حذف تمام استفاده از ViewData برای داده‌های اصلی
- [ ] استفاده از ViewModel برای تمام داده‌های ضروری
- [ ] استفاده از ViewBag فقط برای ViewBag.Title (مجاز)

### 3.6 View Resolution
- [ ] استفاده از GetViewPath("Index") در تمام return View
- [ ] استفاده از GetViewPath("Details") در return View
- [ ] استفاده از GetViewPath("Create") در return View
- [ ] استفاده از GetViewPath("Edit") در return View

---

## Phase 4: View Implementation (پیاده‌سازی View)

### 4.1 Index View
- [ ] ایجاد Index.cshtml
- [ ] پیاده‌سازی Search Panel
  - [ ] فیلتر بر اساس Category
  - [ ] فیلتر بر اساس IsPublished
  - [ ] جستجو در Title, Description, Content
- [ ] پیاده‌سازی Data Table
  - [ ] ستون‌ها: Title, Category, FileUrl, DownloadCount, ViewCount, IsPublished, PublishedAt, Actions
  - [ ] نمایش Badge برای Category
  - [ ] نمایش Badge برای IsPublished
- [ ] پیاده‌سازی Pagination
- [ ] پیاده‌سازی Action Buttons
  - [ ] View (Details)
  - [ ] Edit
  - [ ] Delete
  - [ ] Download (برای Admin)
- [ ] حذف Alert های Bootstrap
- [ ] اضافه کردن SweetAlert برای Confirmations

### 4.2 Create View
- [ ] ایجاد Create.cshtml
- [ ] پیاده‌سازی Form
  - [ ] Title (Required)
  - [ ] Description (Required)
  - [ ] Content (CKEditor - Required)
  - [ ] Category (Required, Dropdown)
  - [ ] File Upload (PDF/Word/Excel)
  - [ ] VideoUrl (اختیاری)
  - [ ] ImageUrl (اختیاری - برای Thumbnail)
  - [ ] PublishedAt (Persian DatePicker)
  - [ ] IsPublished (Checkbox)
  - [ ] IsFeatured (Checkbox)
  - [ ] DisplayOrder
- [ ] پیاده‌سازی Validation Messages
- [ ] پیاده‌سازی Persian DatePicker (استفاده از _PersianDatePicker)
- [ ] اضافه کردن _PersianDatePickerScript به Scripts
- [ ] پیاده‌سازی CKEditor (برای Content)
- [ ] اضافه کردن _CKEditorScript و _CKEditorInit به Scripts
- [ ] پیاده‌سازی File Upload (PDF/Word/Excel)
- [ ] پیاده‌سازی Image Upload (اختیاری)
- [ ] حذف Alert های Bootstrap

### 4.3 Edit View
- [ ] ایجاد Edit.cshtml
- [ ] پیاده‌سازی Form
- [ ] پیاده‌سازی Pre-filled Values
- [ ] پیاده‌سازی Validation Messages
- [ ] پیاده‌سازی Persian DatePicker
- [ ] اضافه کردن _PersianDatePickerScript به Scripts
- [ ] پیاده‌سازی CKEditor
- [ ] اضافه کردن _CKEditorScript و _CKEditorInit به Scripts
- [ ] پیاده‌سازی File Upload
- [ ] پیاده‌سازی Image Upload
- [ ] نمایش فایل فعلی (در صورت وجود)
- [ ] حذف Alert های Bootstrap

### 4.4 Details View
- [ ] ایجاد Details.cshtml
- [ ] پیاده‌سازی Display Template
  - [ ] نمایش تمام اطلاعات
  - [ ] نمایش فایل قابل دانلود
  - [ ] نمایش ویدیو (در صورت وجود)
  - [ ] نمایش تصویر (در صورت وجود)
- [ ] پیاده‌سازی Action Buttons
  - [ ] Edit
  - [ ] Delete
  - [ ] Download
  - [ ] Back to List

---

## Phase 5: UI/UX Optimization (بهینه‌سازی UI/UX)

### 5.1 Design Consistency
- [ ] استفاده از فونت Vazir
- [ ] استفاده از Card Components
- [ ] استفاده از Button Styles
- [ ] استفاده از Form Styles
- [ ] استفاده از Table Styles

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

### 5.4 Color Scheme
- [ ] استفاده از رنگ‌های استاندارد (--medical-*)
- [ ] حذف تمام Gradient های رنگی
- [ ] حذف رنگ‌های جیق و جلف
- [ ] استفاده از Badge های ساده

---

## Phase 6: File Upload System (سیستم آپلود فایل)

### 6.1 File Upload Service
- [ ] بررسی نیاز به File Upload Service برای PDF/Word/Excel
- [ ] بررسی استفاده از IImageUploadService برای تصاویر
- [ ] پیاده‌سازی ProcessFileUpload در Controller

### 6.2 File Upload Implementation
- [ ] اضافه کردن enctype="multipart/form-data" به Form
- [ ] اضافه کردن File Input برای فایل اصلی:
  - [ ] id="FileUrl" name="FileUrl"
  - [ ] accept="application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
  - [ ] Help Text: فرمت‌های مجاز: PDF, Word, Excel | حداکثر حجم: 10 مگابایت
- [ ] اضافه کردن Validation Messages
- [ ] اضافه کردن JavaScript Validation برای نوع و حجم فایل
- [ ] نمایش فایل فعلی در Edit View

---

## Phase 7: Public View (نمایش عمومی)

### 7.1 Public Controller
- [ ] ایجاد PatientEducationController در Controllers (نه Admin)
- [ ] پیاده‌سازی Index Action (GET) - نمایش لیست مطالب منتشر شده
- [ ] پیاده‌سازی Details Action (GET) - نمایش جزئیات
- [ ] پیاده‌سازی Download Action (POST) - دانلود فایل و افزایش DownloadCount
- [ ] پیاده‌سازی View Action (POST) - افزایش ViewCount

### 7.2 Public Views
- [ ] ایجاد Views/PatientEducation/Index.cshtml
- [ ] ایجاد Views/PatientEducation/Details.cshtml
- [ ] پیاده‌سازی فیلتر بر اساس Category
- [ ] پیاده‌سازی جستجو
- [ ] پیاده‌سازی Pagination
- [ ] Responsive Design

---

## Phase 8: Testing & Quality Assurance (تست و کنترل کیفیت)

### 8.1 Unit Testing
- [ ] تست Repository Methods
- [ ] تست Service Methods
- [ ] تست Controller Actions
- [ ] تست ViewModels

### 8.2 Integration Testing
- [ ] تست End-to-End Flows
- [ ] تست Database Operations
- [ ] تست File Upload Operations
- [ ] تست Download Operations
- [ ] تست Notification System

### 8.3 Security Testing
- [ ] تست SQL Injection Protection
- [ ] تست XSS Protection
- [ ] تست CSRF Protection
- [ ] تست File Upload Security
- [ ] تست Authorization Checks

### 8.4 Performance Testing
- [ ] تست Page Load Time
- [ ] تست Database Query Performance
- [ ] تست File Upload Performance
- [ ] تست Search/Filter Performance

---

## Phase 9: Code Review & Optimization (بازبینی و بهینه‌سازی)

### 9.1 Code Review Checklist
- [ ] بررسی Strongly-Typed
- [ ] بررسی SRP Principles
- [ ] بررسی Error Handling
- [ ] بررسی Logging
- [ ] بررسی Security
- [ ] بررسی View Resolution (GetViewPath)

### 9.2 Code Optimization
- [ ] بهینه‌سازی Database Queries
- [ ] حذف N+1 Queries
- [ ] بهینه‌سازی JavaScript
- [ ] بهینه‌سازی CSS

### 9.3 Documentation
- [ ] به‌روزرسانی Code Comments
- [ ] ایجاد API Documentation
- [ ] به‌روزرسانی README
- [ ] ایجاد User Guide

---

## Phase 10: Deployment Preparation (آماده‌سازی برای Production)

### 10.1 Pre-Deployment Checklist
- [ ] تمام Tests پاس شده‌اند
- [ ] تمام Linter Errors برطرف شده‌اند
- [ ] تمام Warnings برطرف شده‌اند
- [ ] تمام TODOs بررسی شده‌اند
- [ ] تمام Documentation به‌روز است

### 10.2 Production Configuration
- [ ] تنظیم File Upload Paths
- [ ] تنظیم File Size Limits
- [ ] تنظیم Logging Levels
- [ ] تنظیم Error Pages

---

## 📝 Notes

### نکات مهم:
1. تمام کدها باید طبق `DEVELOPMENT_CONTRACT.md` باشند
2. رعایت کامل اصول SRP
3. استفاده از Strongly-Typed ViewModels
4. استفاده از GetViewPath() برای تمام Views
5. استفاده از NotificationHelper برای تمام پیام‌ها
6. استفاده از SweetAlert برای Confirmations
7. رعایت استانداردهای رنگ‌بندی
8. استفاده از CKEditor برای Content
9. استفاده از Persian DatePicker برای تاریخ‌ها
10. آپلود فایل PDF/Word/Excel با Validation کامل

### زمان‌بندی پیشنهادی:
- Phase 1: 0.5 روز
- Phase 2: 1 روز
- Phase 3: 1 روز
- Phase 4: 1 روز
- Phase 5: 0.5 روز
- Phase 6: 0.5 روز
- Phase 7: 0.5 روز
- Phase 8-9: 1 روز
- Phase 10: 0.5 روز

**کل زمان:** 6-7 روز کاری

---

## ✅ Sign-off

- [ ] تمام مراحل تکمیل شده‌اند
- [ ] تمام Checklist ها بررسی شده‌اند
- [ ] Code Review انجام شده است
- [ ] آماده برای Production است

**تاریخ تکمیل:** ___________  
**تایید کننده:** ___________

