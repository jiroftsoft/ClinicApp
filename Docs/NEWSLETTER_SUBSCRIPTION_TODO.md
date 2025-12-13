# 📋 TODO List - Newsletter/Subscription Management Module
## مدیریت خبرنامه و اشتراک‌ها - کلینیک درمانی شفا جیرفت

**تاریخ شروع:** 2025-12-12  
**وضعیت:** در حال تحلیل  
**اولویت:** ⭐⭐⭐⭐ (بالا)  
**زمان تخمینی:** 3-4 روز

---

## 📊 تحلیل نیازمندی‌ها (Requirements Analysis)

### 1.1 User Stories

#### US-1: ثبت اشتراک از سایت
**به عنوان** یک بازدیدکننده سایت  
**می‌خواهم** بتوانم در خبرنامه کلینیک ثبت‌نام کنم  
**تا** از آخرین اخبار و اطلاعیه‌های کلینیک مطلع شوم

**Acceptance Criteria:**
- فرم ثبت‌نام ساده و سریع (ایمیل + نام)
- تایید ایمیل (Double Opt-in)
- پیام موفقیت/خطا
- جلوگیری از ثبت‌نام تکراری

#### US-2: مدیریت لیست اشتراک‌ها
**به عنوان** ادمین  
**می‌خواهم** بتوانم لیست تمام اشتراک‌ها را مشاهده و مدیریت کنم  
**تا** کنترل کاملی بر روی مشترکین داشته باشم

**Acceptance Criteria:**
- نمایش لیست با Pagination
- جستجو و فیلتر (ایمیل، نام، وضعیت، دسته‌بندی)
- فعال/غیرفعال کردن اشتراک
- حذف اشتراک
- Export به Excel

#### US-3: گروه‌بندی اشتراک‌ها
**به عنوان** ادمین  
**می‌خواهم** بتوانم اشتراک‌ها را بر اساس علاقه‌مندی‌ها دسته‌بندی کنم  
**تا** خبرنامه‌های هدفمند ارسال کنم

**Acceptance Criteria:**
- دسته‌بندی‌های قابل تنظیم (مقالات، اطلاعیه‌ها، خدمات جدید، ...)
- امکان انتخاب چند دسته‌بندی برای هر مشترک
- فیلتر بر اساس دسته‌بندی

#### US-4: ارسال خبرنامه
**به عنوان** ادمین  
**می‌خواهم** بتوانم خبرنامه را به گروه‌های مختلف ارسال کنم  
**تا** اطلاعات مهم را به مشترکین برسانم

**Acceptance Criteria:**
- انتخاب دسته‌بندی یا تمام مشترکین
- استفاده از Template
- ارسال فوری یا زمان‌بندی شده
- ارسال ایمیل و SMS
- نمایش پیش‌نمایش

#### US-5: Template های خبرنامه
**به عنوان** ادمین  
**می‌خواهم** بتوانم Template های مختلف برای خبرنامه ایجاد کنم  
**تا** خبرنامه‌های حرفه‌ای و یکپارچه ارسال کنم

**Acceptance Criteria:**
- ایجاد/ویرایش Template
- استفاده از CKEditor
- پیش‌نمایش Template
- استفاده از Variables (نام، ایمیل، ...)

#### US-6: تاریخچه ارسال‌ها
**به عنوان** ادمین  
**می‌خواهم** بتوانم تاریخچه تمام ارسال‌های خبرنامه را مشاهده کنم  
**تا** عملکرد سیستم را بررسی کنم

**Acceptance Criteria:**
- لیست تمام ارسال‌ها
- جزئیات هر ارسال (تعداد ارسال شده، موفق، ناموفق)
- آمار باز شدن و کلیک
- فیلتر بر اساس تاریخ

#### US-7: آمار و گزارش
**به عنوان** ادمین  
**می‌خواهم** بتوانم آمار و گزارش‌های خبرنامه را مشاهده کنم  
**تا** عملکرد را تحلیل کنم

**Acceptance Criteria:**
- تعداد کل مشترکین
- تعداد مشترکین فعال
- نرخ باز شدن (Open Rate)
- نرخ کلیک (Click Rate)
- نمودارهای آماری

#### US-8: لغو اشتراک
**به عنوان** مشترک  
**می‌خواهم** بتوانم اشتراک خود را لغو کنم  
**تا** دیگر خبرنامه دریافت نکنم

**Acceptance Criteria:**
- لینک لغو اشتراک در ایمیل
- صفحه لغو اشتراک
- تایید لغو اشتراک
- ثبت تاریخ لغو

---

## Phase 1: Analysis & Design (تحلیل و طراحی)

### 1.1 Requirements Analysis ✅
- [x] تحلیل نیازمندی‌های کاربر
- [x] تعریف User Stories
- [x] تعریف Acceptance Criteria
- [x] تعریف Technical Requirements

### 1.2 Entity & Database Design
- [ ] طراحی Entity Model (NewsletterSubscription)
  - [ ] SubscriptionId (PK)
  - [ ] Email (Required, Unique, Indexed)
  - [ ] PhoneNumber (Optional)
  - [ ] FullName (Optional)
  - [ ] Categories (JSON Array یا جداول مرتبط)
  - [ ] Source (Website, Admin, Import)
  - [ ] IsActive
  - [ ] IsVerified (Double Opt-in)
  - [ ] VerificationToken
  - [ ] VerifiedAt
  - [ ] UnsubscribedAt
  - [ ] UnsubscribeToken
  - [ ] IpAddress
  - [ ] UserAgent
  - [ ] ISoftDelete, ITrackable

- [ ] طراحی Entity Model (NewsletterTemplate)
  - [ ] TemplateId (PK)
  - [ ] Name (Required)
  - [ ] Subject (Required)
  - [ ] Content (HTML, CKEditor)
  - [ ] IsActive
  - [ ] ISoftDelete, ITrackable

- [ ] طراحی Entity Model (NewsletterCampaign)
  - [ ] CampaignId (PK)
  - [ ] Title (Required)
  - [ ] Subject (Required)
  - [ ] Content (HTML)
  - [ ] TemplateId (FK, Optional)
  - [ ] Categories (JSON Array)
  - [ ] SendToAll (Boolean)
  - [ ] ScheduledAt (Optional)
  - [ ] SentAt (Optional)
  - [ ] Status (Draft, Scheduled, Sending, Sent, Failed)
  - [ ] TotalRecipients
  - [ ] SentCount
  - [ ] FailedCount
  - [ ] OpenedCount
  - [ ] ClickedCount
  - [ ] ISoftDelete, ITrackable

- [ ] طراحی Entity Model (NewsletterCampaignRecipient)
  - [ ] CampaignRecipientId (PK)
  - [ ] CampaignId (FK)
  - [ ] SubscriptionId (FK)
  - [ ] Email
  - [ ] Status (Pending, Sent, Failed, Bounced)
  - [ ] SentAt
  - [ ] OpenedAt
  - [ ] ClickedAt
  - [ ] ErrorMessage
  - [ ] ITrackable

- [ ] طراحی Database Schema
- [ ] تعریف Relationships
- [ ] تعریف Indexes
- [ ] ایجاد Migration Script

### 1.3 Enum Design
- [ ] طراحی NewsletterSubscriptionSource Enum
  - [ ] Website
  - [ ] Admin
  - [ ] Import
  - [ ] API

- [ ] طراحی NewsletterCampaignStatus Enum
  - [ ] Draft
  - [ ] Scheduled
  - [ ] Sending
  - [ ] Sent
  - [ ] Failed

- [ ] طراحی NewsletterRecipientStatus Enum
  - [ ] Pending
  - [ ] Sent
  - [ ] Failed
  - [ ] Bounced

- [ ] طراحی NewsletterCategory Enum (یا جدول جداگانه)
  - [ ] Articles (مقالات)
  - [ ] Announcements (اطلاعیه‌ها)
  - [ ] Services (خدمات جدید)
  - [ ] HealthTips (نکات سلامتی)
  - [ ] Events (رویدادها)
  - [ ] Promotions (تخفیف‌ها)

### 1.4 ViewModel Design
- [ ] طراحی NewsletterSubscriptionIndexViewModel
- [ ] طراحی NewsletterSubscriptionCreateEditViewModel
- [ ] طراحی NewsletterSubscriptionDetailsViewModel
- [ ] طراحی NewsletterSubscriptionSearchViewModel
- [ ] طراحی PublicNewsletterSubscriptionViewModel (برای فرم سایت)
- [ ] طراحی NewsletterTemplateIndexViewModel
- [ ] طراحی NewsletterTemplateCreateEditViewModel
- [ ] طراحی NewsletterCampaignIndexViewModel
- [ ] طراحی NewsletterCampaignCreateEditViewModel
- [ ] طراحی NewsletterCampaignDetailsViewModel
- [ ] طراحی NewsletterCampaignSendViewModel
- [ ] طراحی NewsletterStatisticsViewModel
- [ ] تعریف Data Annotations

### 1.5 Interface Design
- [ ] طراحی INewsletterSubscriptionRepository Interface
- [ ] طراحی INewsletterSubscriptionService Interface
- [ ] طراحی INewsletterTemplateRepository Interface
- [ ] طراحی INewsletterTemplateService Interface
- [ ] طراحی INewsletterCampaignRepository Interface
- [ ] طراحی INewsletterCampaignService Interface
- [ ] طراحی INewsletterEmailService Interface (برای ارسال ایمیل)
- [ ] طراحی INewsletterSmsService Interface (برای ارسال SMS)
- [ ] تعریف Method Signatures
- [ ] تعریف Return Types

---

## Phase 2: Backend Implementation (پیاده‌سازی Backend)

### 2.1 Repository Implementation
- [ ] پیاده‌سازی NewsletterSubscriptionRepository
  - [ ] GetByIdAsync
  - [ ] GetByEmailAsync
  - [ ] GetByVerificationTokenAsync
  - [ ] GetByUnsubscribeTokenAsync
  - [ ] GetAllAsync
  - [ ] GetActiveAsync
  - [ ] GetByCategoryAsync
  - [ ] GetBySourceAsync
  - [ ] SearchAsync
  - [ ] Add
  - [ ] Update
  - [ ] Delete
  - [ ] ExistsAsync

- [ ] پیاده‌سازی NewsletterTemplateRepository
  - [ ] GetByIdAsync
  - [ ] GetAllAsync
  - [ ] GetActiveAsync
  - [ ] Add
  - [ ] Update
  - [ ] Delete

- [ ] پیاده‌سازی NewsletterCampaignRepository
  - [ ] GetByIdAsync
  - [ ] GetAllAsync
  - [ ] GetByStatusAsync
  - [ ] GetScheduledAsync
  - [ ] Add
  - [ ] Update
  - [ ] Delete

- [ ] پیاده‌سازی NewsletterCampaignRecipientRepository
  - [ ] GetByCampaignIdAsync
  - [ ] GetBySubscriptionIdAsync
  - [ ] Add
  - [ ] Update
  - [ ] BulkInsert

### 2.2 Service Implementation
- [ ] پیاده‌سازی NewsletterSubscriptionService
  - [ ] GetSubscriptionsAsync (با Search و Filter)
  - [ ] GetSubscriptionDetailsAsync
  - [ ] CreateSubscriptionAsync (از سایت)
  - [ ] CreateSubscriptionByAdminAsync
  - [ ] UpdateSubscriptionAsync
  - [ ] DeleteSubscriptionAsync
  - [ ] ActivateSubscriptionAsync
  - [ ] DeactivateSubscriptionAsync
  - [ ] VerifySubscriptionAsync (Double Opt-in)
  - [ ] UnsubscribeAsync
  - [ ] ImportSubscriptionsAsync (از Excel)
  - [ ] ExportSubscriptionsAsync (به Excel)
  - [ ] GetStatisticsAsync

- [ ] پیاده‌سازی NewsletterTemplateService
  - [ ] GetTemplatesAsync
  - [ ] GetTemplateDetailsAsync
  - [ ] CreateTemplateAsync
  - [ ] UpdateTemplateAsync
  - [ ] DeleteTemplateAsync
  - [ ] ActivateTemplateAsync
  - [ ] DeactivateTemplateAsync

- [ ] پیاده‌سازی NewsletterCampaignService
  - [ ] GetCampaignsAsync
  - [ ] GetCampaignDetailsAsync
  - [ ] CreateCampaignAsync
  - [ ] UpdateCampaignAsync
  - [ ] DeleteCampaignAsync
  - [ ] SendCampaignAsync (فوری)
  - [ ] ScheduleCampaignAsync (زمان‌بندی شده)
  - [ ] CancelScheduledCampaignAsync
  - [ ] GetCampaignStatisticsAsync
  - [ ] TrackEmailOpenAsync
  - [ ] TrackEmailClickAsync

- [ ] پیاده‌سازی NewsletterEmailService
  - [ ] SendNewsletterAsync
  - [ ] SendVerificationEmailAsync
  - [ ] SendUnsubscribeConfirmationAsync
  - [ ] RenderTemplateAsync (با Variables)

- [ ] پیاده‌سازی NewsletterSmsService
  - [ ] SendNewsletterSmsAsync
  - [ ] SendVerificationSmsAsync

### 2.3 Dependency Injection
- [ ] ثبت INewsletterSubscriptionRepository در UnityConfig
- [ ] ثبت INewsletterSubscriptionService در UnityConfig
- [ ] ثبت INewsletterTemplateRepository در UnityConfig
- [ ] ثبت INewsletterTemplateService در UnityConfig
- [ ] ثبت INewsletterCampaignRepository در UnityConfig
- [ ] ثبت INewsletterCampaignService در UnityConfig
- [ ] ثبت INewsletterEmailService در UnityConfig
- [ ] ثبت INewsletterSmsService در UnityConfig
- [ ] تست Dependency Injection

---

## Phase 3: Controller Implementation (پیاده‌سازی Controller)

### 3.1 Admin Controllers
- [ ] ایجاد NewsletterSubscriptionController (Admin)
  - [ ] Index (GET) - لیست اشتراک‌ها
  - [ ] Details (GET) - جزئیات اشتراک
  - [ ] Create (GET) - فرم ایجاد
  - [ ] Create (POST) - ایجاد اشتراک
  - [ ] Edit (GET) - فرم ویرایش
  - [ ] Edit (POST) - ویرایش اشتراک
  - [ ] Delete (POST) - حذف اشتراک
  - [ ] Activate (POST) - فعال کردن
  - [ ] Deactivate (POST) - غیرفعال کردن
  - [ ] Export (GET) - Export به Excel
  - [ ] Import (GET) - فرم Import
  - [ ] Import (POST) - Import از Excel

- [ ] ایجاد NewsletterTemplateController (Admin)
  - [ ] Index (GET) - لیست Template ها
  - [ ] Details (GET) - جزئیات Template
  - [ ] Create (GET) - فرم ایجاد
  - [ ] Create (POST) - ایجاد Template
  - [ ] Edit (GET) - فرم ویرایش
  - [ ] Edit (POST) - ویرایش Template
  - [ ] Delete (POST) - حذف Template
  - [ ] Preview (GET) - پیش‌نمایش Template

- [ ] ایجاد NewsletterCampaignController (Admin)
  - [ ] Index (GET) - لیست Campaign ها
  - [ ] Details (GET) - جزئیات Campaign
  - [ ] Create (GET) - فرم ایجاد
  - [ ] Create (POST) - ایجاد Campaign
  - [ ] Edit (GET) - فرم ویرایش
  - [ ] Edit (POST) - ویرایش Campaign
  - [ ] Delete (POST) - حذف Campaign
  - [ ] Send (GET) - فرم ارسال
  - [ ] Send (POST) - ارسال فوری
  - [ ] Schedule (POST) - زمان‌بندی ارسال
  - [ ] Cancel (POST) - لغو زمان‌بندی
  - [ ] Statistics (GET) - آمار Campaign

- [ ] ایجاد NewsletterStatisticsController (Admin)
  - [ ] Index (GET) - صفحه آمار کلی
  - [ ] GetStatistics (GET) - API برای نمودارها

### 3.2 Public Controllers
- [ ] ایجاد NewsletterController (Public)
  - [ ] Subscribe (POST) - ثبت اشتراک از سایت
  - [ ] Verify (GET) - تایید ایمیل (Double Opt-in)
  - [ ] Unsubscribe (GET) - صفحه لغو اشتراک
  - [ ] Unsubscribe (POST) - تایید لغو اشتراک
  - [ ] TrackOpen (GET) - Tracking باز شدن ایمیل
  - [ ] TrackClick (GET) - Tracking کلیک روی لینک

### 3.3 Notification Integration
- [ ] استفاده از NotificationHelper.SetSuccess
- [ ] استفاده از NotificationHelper.SetError
- [ ] استفاده از NotificationHelper.SetWarning
- [ ] استفاده از NotificationHelper.SetInfo
- [ ] حذف تمام TempData مستقیم

### 3.4 Strongly-Typed ViewModel
- [ ] ایجاد ViewModel برای Index
- [ ] حذف تمام استفاده از ViewBag برای داده‌های اصلی
- [ ] حذف تمام استفاده از ViewData برای داده‌های اصلی
- [ ] استفاده از ViewModel برای تمام داده‌های ضروری

### 3.5 View Resolution
- [ ] استفاده از GetViewPath("Index") در تمام return View
- [ ] استفاده از GetViewPath("Details") در return View
- [ ] استفاده از GetViewPath("Create") در return View
- [ ] استفاده از GetViewPath("Edit") در return View

---

## Phase 4: View Implementation (پیاده‌سازی View)

### 4.1 Admin Views - Subscription Management
- [ ] ایجاد Index.cshtml
  - [ ] Search Panel (ایمیل، نام، وضعیت، دسته‌بندی)
  - [ ] Data Table (ایمیل، نام، دسته‌بندی‌ها، وضعیت، تاریخ ثبت)
  - [ ] Action Buttons (View, Edit, Delete, Activate/Deactivate)
  - [ ] Pagination
  - [ ] Export Button
  - [ ] Import Button

- [ ] ایجاد Create.cshtml
  - [ ] فرم ایجاد اشتراک
  - [ ] Email (Required)
  - [ ] FullName (Optional)
  - [ ] PhoneNumber (Optional)
  - [ ] Categories (Multi-select)
  - [ ] Source (Dropdown)

- [ ] ایجاد Edit.cshtml
  - [ ] فرم ویرایش اشتراک
  - [ ] تمام فیلدهای Create
  - [ ] نمایش تاریخ ثبت و تایید

- [ ] ایجاد Details.cshtml
  - [ ] نمایش تمام اطلاعات
  - [ ] تاریخچه تغییرات
  - [ ] Campaign های ارسال شده به این مشترک

- [ ] ایجاد Import.cshtml
  - [ ] فرم آپلود فایل Excel
  - [ ] Help Text برای فرمت فایل
  - [ ] Preview داده‌های Import

### 4.2 Admin Views - Template Management
- [ ] ایجاد Index.cshtml
  - [ ] لیست Template ها
  - [ ] Action Buttons (View, Edit, Delete, Preview)

- [ ] ایجاد Create.cshtml
  - [ ] فرم ایجاد Template
  - [ ] Name (Required)
  - [ ] Subject (Required)
  - [ ] Content (CKEditor)
  - [ ] Help Text برای Variables

- [ ] ایجاد Edit.cshtml
  - [ ] فرم ویرایش Template
  - [ ] تمام فیلدهای Create

- [ ] ایجاد Details.cshtml
  - [ ] نمایش Template
  - [ ] پیش‌نمایش با Sample Data

### 4.3 Admin Views - Campaign Management
- [ ] ایجاد Index.cshtml
  - [ ] لیست Campaign ها
  - [ ] فیلتر بر اساس Status
  - [ ] Action Buttons (View, Edit, Delete, Send, Statistics)

- [ ] ایجاد Create.cshtml
  - [ ] فرم ایجاد Campaign
  - [ ] Title (Required)
  - [ ] Subject (Required)
  - [ ] Content (CKEditor)
  - [ ] Template Selection (Optional)
  - [ ] Categories Selection (Multi-select)
  - [ ] SendToAll Checkbox
  - [ ] ScheduledAt (Persian DatePicker)

- [ ] ایجاد Edit.cshtml
  - [ ] فرم ویرایش Campaign
  - [ ] تمام فیلدهای Create
  - [ ] نمایش Status

- [ ] ایجاد Details.cshtml
  - [ ] نمایش Campaign
  - [ ] آمار ارسال
  - [ ] لیست Recipients
  - [ ] نمودارهای آماری

- [ ] ایجاد Send.cshtml
  - [ ] فرم ارسال Campaign
  - [ ] Preview
  - [ ] انتخاب Recipients
  - [ ] Send Now یا Schedule

### 4.4 Admin Views - Statistics
- [ ] ایجاد Index.cshtml
  - [ ] آمار کلی (تعداد مشترکین، فعال، غیرفعال)
  - [ ] نمودار رشد مشترکین
  - [ ] نمودار Open Rate
  - [ ] نمودار Click Rate
  - [ ] لیست Campaign های اخیر

### 4.5 Public Views
- [ ] ایجاد Subscribe.cshtml (Partial View برای Footer)
  - [ ] فرم ساده (ایمیل + نام)
  - [ ] AJAX Submit
  - [ ] پیام موفقیت/خطا

- [ ] ایجاد Verify.cshtml
  - [ ] پیام تایید موفقیت
  - [ ] لینک بازگشت به سایت

- [ ] ایجاد Unsubscribe.cshtml
  - [ ] فرم لغو اشتراک
  - [ ] تایید لغو اشتراک
  - [ ] پیام موفقیت

---

## Phase 5: Email & SMS Integration (یکپارچه‌سازی ایمیل و SMS)

### 5.1 Email Service Integration
- [ ] بررسی EmailService موجود
- [ ] پیاده‌سازی NewsletterEmailService
- [ ] پیاده‌سازی Template Rendering
- [ ] پیاده‌سازی Variable Replacement
- [ ] پیاده‌سازی Tracking Pixels
- [ ] پیاده‌سازی Click Tracking

### 5.2 SMS Service Integration
- [ ] بررسی SMSService موجود
- [ ] پیاده‌سازی NewsletterSmsService
- [ ] پیاده‌سازی Template Rendering برای SMS
- [ ] محدودیت طول پیام

### 5.3 Background Job (برای ارسال زمان‌بندی شده)
- [ ] بررسی Background Job System موجود
- [ ] پیاده‌سازی Scheduled Campaign Job
- [ ] پیاده‌سازی Retry Logic
- [ ] پیاده‌سازی Error Handling

---

## Phase 6: UI/UX Optimization (بهینه‌سازی UI/UX)

### 6.1 Design Consistency
- [ ] استفاده از فونت Vazir
- [ ] استفاده از Card Components
- [ ] استفاده از Button Styles
- [ ] استفاده از Form Styles
- [ ] استفاده از Table Styles
- [ ] استفاده از رنگ‌های استاندارد (--medical-*)

### 6.2 Responsive Design
- [ ] تست Mobile View
- [ ] تست Tablet View
- [ ] تست Desktop View
- [ ] بهینه‌سازی Table Responsive
- [ ] بهینه‌سازی Form Responsive

### 6.3 Accessibility
- [ ] اضافه کردن Alt Text برای Images
- [ ] اضافه کردن Title برای Links
- [ ] اضافه کردن ARIA Labels
- [ ] تست Keyboard Navigation
- [ ] تست Screen Reader

---

## Phase 7: Advanced Features (ویژگی‌های پیشرفته)

### 7.1 Export/Import
- [ ] Export به Excel
  - [ ] استفاده از EPPlus یا ClosedXML
  - [ ] فرمت‌بندی Excel
  - [ ] شامل تمام فیلدها

- [ ] Import از Excel
  - [ ] Validation داده‌ها
  - [ ] Error Reporting
  - [ ] Bulk Insert
  - [ ] Skip Duplicates

### 7.2 Statistics & Analytics
- [ ] Dashboard با آمار کلی
- [ ] نمودار رشد مشترکین
- [ ] نمودار Open Rate
- [ ] نمودار Click Rate
- [ ] نمودار Campaign Performance
- [ ] استفاده از Chart.js یا Highcharts

### 7.3 Email Tracking
- [ ] Tracking Pixel (1x1 image)
- [ ] Click Tracking (Link Rewriting)
- [ ] Open Rate Calculation
- [ ] Click Rate Calculation
- [ ] Bounce Detection

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
- [ ] تست Email Sending
- [ ] تست SMS Sending
- [ ] تست Export/Import
- [ ] تست Tracking

### 8.3 Security Testing
- [ ] تست SQL Injection Protection
- [ ] تست XSS Protection
- [ ] تست CSRF Protection
- [ ] تست Email Validation
- [ ] تست Unsubscribe Token Security

### 8.4 Performance Testing
- [ ] تست Page Load Time
- [ ] تست Database Query Performance
- [ ] تست Bulk Email Sending
- [ ] تست Export/Import Performance

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
- [ ] بهینه‌سازی Email Sending (Batch)

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
- [ ] تنظیم SMTP Settings
- [ ] تنظیم SMS Settings
- [ ] تنظیم Background Job Settings
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
10. Double Opt-in برای ثبت‌نام
11. Tracking برای باز شدن و کلیک
12. Export/Import به Excel

### زمان‌بندی پیشنهادی:
- Phase 1: 1 روز
- Phase 2: 1.5 روز
- Phase 3: 1 روز
- Phase 4: 1 روز
- Phase 5: 0.5 روز
- Phase 6: 0.5 روز
- Phase 7: 1 روز
- Phase 8-9: 1 روز
- Phase 10: 0.5 روز

**کل زمان:** 8-9 روز کاری

---

## ✅ Sign-off

- [ ] تمام مراحل تکمیل شده‌اند
- [ ] تمام Checklist ها بررسی شده‌اند
- [ ] Code Review انجام شده است
- [ ] آماده برای Production است

**تاریخ تکمیل:** ___________  
**تایید کننده:** ___________

