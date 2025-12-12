# 📋 TODO List - Contact Form Management Module
## مدیریت فرم تماس - کلینیک درمانی شفا جیرفت

**تاریخ شروع:** 2025-12-12  
**وضعیت:** در حال پیاده‌سازی  
**اولویت:** ⭐⭐⭐⭐⭐ (بسیار بالا)

---

## Phase 1: Analysis & Design (تحلیل و طراحی)

### 1.1 Requirements Analysis ✅
- [x] تحلیل نیازمندی‌های کاربر
- [x] تعریف User Stories
- [x] تعریف Acceptance Criteria
- [x] تعریف Technical Requirements

### 1.2 Entity & Database Design
- [ ] طراحی Entity Model (ContactForm)
- [ ] طراحی Database Schema
- [ ] تعریف Relationships
- [ ] تعریف Indexes
- [ ] ایجاد Migration Script

### 1.3 ViewModel Design
- [ ] طراحی ContactFormIndexViewModel
- [ ] طراحی ContactFormCreateEditViewModel
- [ ] طراحی ContactFormDetailsViewModel
- [ ] طراحی ContactFormSearchViewModel
- [ ] تعریف Data Annotations
- [ ] تعریف Enum برای Category و Status

### 1.4 Interface Design
- [ ] طراحی IContactFormRepository Interface
- [ ] طراحی IContactFormService Interface
- [ ] تعریف Method Signatures
- [ ] تعریف Return Types

---

## Phase 2: Backend Implementation (پیاده‌سازی Backend)

### 2.1 Repository Implementation
- [ ] پیاده‌سازی ContactFormRepository Class
- [ ] پیاده‌سازی GetByIdAsync
- [ ] پیاده‌سازی GetAllAsync
- [ ] پیاده‌سازی GetUnreadCountAsync
- [ ] پیاده‌سازی GetByStatusAsync
- [ ] پیاده‌سازی GetByCategoryAsync
- [ ] پیاده‌سازی SearchAsync
- [ ] پیاده‌سازی Add/Update/Delete
- [ ] پیاده‌سازی MarkAsReadAsync
- [ ] پیاده‌سازی MarkAsRepliedAsync
- [ ] تست Repository Methods

### 2.2 Service Implementation
- [ ] پیاده‌سازی ContactFormService Class
- [ ] پیاده‌سازی Business Logic
- [ ] پیاده‌سازی Validation
- [ ] پیاده‌سازی Error Handling
- [ ] پیاده‌سازی Logging
- [ ] پیاده‌سازی SendReplyEmailAsync (یکپارچه‌سازی با Email Service)
- [ ] پیاده‌سازی SendReplySmsAsync (یکپارچه‌سازی با SMS Service)
- [ ] تست Service Methods

### 2.3 Dependency Injection
- [ ] ثبت IContactFormRepository در UnityConfig
- [ ] ثبت IContactFormService در UnityConfig
- [ ] تست Dependency Injection

---

## Phase 3: Controller Implementation (پیاده‌سازی Controller)

### 3.1 Controller Setup
- [ ] ایجاد ContactFormController Class
- [ ] ارث‌بری از BaseCMSController
- [ ] تزریق Dependencies (IContactFormService, ICurrentUserService, ILogger)
- [ ] تعریف Route Attributes
- [ ] تعریف Authorization ([Authorize(Roles = "Admin")])

### 3.2 CRUD Actions
- [ ] پیاده‌سازی Index Action (GET)
  - [ ] فیلتر بر اساس Status
  - [ ] فیلتر بر اساس Category
  - [ ] جستجو
  - [ ] Pagination
- [ ] پیاده‌سازی Details Action (GET)
- [ ] پیاده‌سازی Reply Action (GET)
- [ ] پیاده‌سازی Reply Action (POST)
  - [ ] ارسال پاسخ از طریق Email/SMS
  - [ ] به‌روزرسانی Status به Replied
  - [ ] ذخیره ReplyMessage
- [ ] پیاده‌سازی Delete Action (POST)
- [ ] پیاده‌سازی MarkAsRead Action (POST)
- [ ] پیاده‌سازی MarkAsUnread Action (POST)
- [ ] پیاده‌سازی ChangeStatus Action (POST)

### 3.3 Additional Actions
- [ ] پیاده‌سازی ExportToExcel Action
- [ ] پیاده‌سازی ExportToPdf Action
- [ ] پیاده‌سازی GetUnreadCount Action (برای Dashboard)

### 3.4 Notification Integration
- [ ] استفاده از NotificationHelper.SetSuccess
- [ ] استفاده از NotificationHelper.SetError
- [ ] استفاده از NotificationHelper.SetWarning
- [ ] استفاده از NotificationHelper.SetInfo
- [ ] حذف تمام TempData مستقیم

### 3.5 Strongly-Typed ViewModel
- [ ] ایجاد ViewModel برای Index (ContactFormIndexPageViewModel)
- [ ] حذف تمام استفاده از ViewBag برای داده‌های اصلی
- [ ] حذف تمام استفاده از ViewData برای داده‌های اصلی
- [ ] استفاده از ViewModel برای تمام داده‌های ضروری
- [ ] استفاده از ViewBag فقط برای ViewBag.Title (مجاز)

### 3.6 View Resolution
- [ ] استفاده از GetViewPath("Index") در تمام return View
- [ ] استفاده از GetViewPath("Details") در return View
- [ ] استفاده از GetViewPath("Reply") در return View

---

## Phase 4: View Implementation (پیاده‌سازی View)

### 4.1 Index View
- [ ] ایجاد Index.cshtml
- [ ] پیاده‌سازی Search Panel
  - [ ] فیلتر بر اساس Status
  - [ ] فیلتر بر اساس Category
  - [ ] جستجو در FullName, Email, Subject, Message
- [ ] پیاده‌سازی Data Table
  - [ ] ستون‌ها: FullName, Email, PhoneNumber, Subject, Category, Status, CreatedAt, Actions
  - [ ] نمایش Badge برای Status
  - [ ] نمایش Badge برای Category
  - [ ] نمایش آیکون برای IsRead
- [ ] پیاده‌سازی Pagination
- [ ] پیاده‌سازی Action Buttons
  - [ ] View (Details)
  - [ ] Reply
  - [ ] Mark as Read/Unread
  - [ ] Delete
- [ ] حذف Alert های Bootstrap
- [ ] اضافه کردن SweetAlert برای Confirmations

### 4.2 Details View
- [ ] ایجاد Details.cshtml
- [ ] پیاده‌سازی Display Template
  - [ ] نمایش تمام اطلاعات پیام
  - [ ] نمایش تاریخ و زمان
  - [ ] نمایش Status و Category
  - [ ] نمایش IsRead
- [ ] پیاده‌سازی Action Buttons
  - [ ] Reply
  - [ ] Mark as Read/Unread
  - [ ] Change Status
  - [ ] Delete
  - [ ] Back to List
- [ ] پیاده‌سازی Reply Section
  - [ ] نمایش ReplyMessage (در صورت وجود)
  - [ ] نمایش RepliedAt و RepliedBy
- [ ] حذف Alert های Bootstrap

### 4.3 Reply View
- [ ] ایجاد Reply.cshtml
- [ ] پیاده‌سازی Form
  - [ ] نمایش اطلاعات پیام اصلی
  - [ ] TextArea برای ReplyMessage
  - [ ] Checkbox برای SendEmail
  - [ ] Checkbox برای SendSms
- [ ] پیاده‌سازی Validation Messages
- [ ] حذف Alert های Bootstrap

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

## Phase 6: Public Contact Form (فرم تماس عمومی)

### 6.1 Public Controller
- [ ] ایجاد ContactController در Controllers (نه Admin)
- [ ] پیاده‌سازی Index Action (GET) - نمایش فرم
- [ ] پیاده‌سازی Submit Action (POST) - ارسال پیام
  - [ ] Validation
  - [ ] ذخیره در دیتابیس
  - [ ] ارسال ایمیل به Admin
  - [ ] نمایش پیام موفقیت

### 6.2 Public View
- [ ] ایجاد Views/Contact/Index.cshtml
- [ ] پیاده‌سازی فرم تماس
  - [ ] FullName (Required)
  - [ ] Email (Required, Email Validation)
  - [ ] PhoneNumber (Required)
  - [ ] Subject (Required)
  - [ ] Category (Required, Dropdown)
  - [ ] Message (Required, TextArea)
  - [ ] Captcha (در صورت نیاز)
- [ ] پیاده‌سازی Validation Messages
- [ ] پیاده‌سازی Success Message
- [ ] Responsive Design

---

## Phase 7: Testing & Quality Assurance (تست و کنترل کیفیت)

### 7.1 Unit Testing
- [ ] تست Repository Methods
- [ ] تست Service Methods
- [ ] تست Controller Actions
- [ ] تست ViewModels

### 7.2 Integration Testing
- [ ] تست End-to-End Flows
- [ ] تست Database Operations
- [ ] تست Email Sending
- [ ] تست SMS Sending
- [ ] تست Notification System

### 7.3 Security Testing
- [ ] تست SQL Injection Protection
- [ ] تست XSS Protection
- [ ] تست CSRF Protection
- [ ] تست Authorization Checks
- [ ] تست Input Validation

### 7.4 Performance Testing
- [ ] تست Page Load Time
- [ ] تست Database Query Performance
- [ ] تست Search/Filter Performance
- [ ] تست Export Performance

---

## Phase 8: Code Review & Optimization (بازبینی و بهینه‌سازی)

### 8.1 Code Review Checklist
- [ ] بررسی Strongly-Typed
- [ ] بررسی SRP Principles
- [ ] بررسی Error Handling
- [ ] بررسی Logging
- [ ] بررسی Security
- [ ] بررسی View Resolution (GetViewPath)

### 8.2 Code Optimization
- [ ] بهینه‌سازی Database Queries
- [ ] حذف N+1 Queries
- [ ] بهینه‌سازی JavaScript
- [ ] بهینه‌سازی CSS

### 8.3 Documentation
- [ ] به‌روزرسانی Code Comments
- [ ] ایجاد API Documentation
- [ ] به‌روزرسانی README
- [ ] ایجاد User Guide

---

## Phase 9: Deployment Preparation (آماده‌سازی برای Production)

### 9.1 Pre-Deployment Checklist
- [ ] تمام Tests پاس شده‌اند
- [ ] تمام Linter Errors برطرف شده‌اند
- [ ] تمام Warnings برطرف شده‌اند
- [ ] تمام TODOs بررسی شده‌اند
- [ ] تمام Documentation به‌روز است

### 9.2 Production Configuration
- [ ] تنظیم Email Configuration
- [ ] تنظیم SMS Configuration
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

### زمان‌بندی پیشنهادی:
- Phase 1: 0.5 روز
- Phase 2: 1 روز
- Phase 3: 1 روز
- Phase 4: 1 روز
- Phase 5: 0.5 روز
- Phase 6: 0.5 روز
- Phase 7-8: 1 روز
- Phase 9: 0.5 روز

**کل زمان:** 6-7 روز کاری

---

## ✅ Sign-off

- [ ] تمام مراحل تکمیل شده‌اند
- [ ] تمام Checklist ها بررسی شده‌اند
- [ ] Code Review انجام شده است
- [ ] آماده برای Production است

**تاریخ تکمیل:** ___________  
**تایید کننده:** ___________

