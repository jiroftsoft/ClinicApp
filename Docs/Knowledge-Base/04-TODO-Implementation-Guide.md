# 📋 راهنمای سریع پیاده‌سازی با TODO
## ClinicApp - Quick Implementation Checklist

**نسخه:** 1.0  
**آخرین به‌روزرسانی:** دی ۱۴۰۴  
**مرجع کامل:** `Docs/TODO_TEMPLATE.md`

---

## 🚀 راهنمای استفاده

این راهنما یک **Checklist سریع** برای پیاده‌سازی هر ماژول/feature جدید است.  
برای جزئیات کامل، به `Docs/TODO_TEMPLATE.md` مراجعه کنید.

---

## ⚡ Quick Start Checklist

### 🔍 Phase 1: Analysis & Design (1-2 روز)

```
✅ تحلیل نیازمندی‌ها
✅ طراحی Entity و Database
✅ طراحی ViewModels
✅ طراحی Repository/Service Interfaces
```

**خروجی:**
- Entity Model
- Database Schema
- ViewModel Classes
- Interface Definitions

---

### 🛠️ Phase 2: Backend Implementation (2-3 روز)

```
✅ Repository Implementation
   - GetByIdAsync
   - GetAllAsync
   - Add/Update/Delete
   - Search/Filter Methods

✅ Service Implementation
   - Business Logic
   - Validation
   - Error Handling
   - Logging

✅ Dependency Injection
   - ثبت در UnityConfig
```

**خروجی:**
- Repository Classes
- Service Classes
- Dependency Injection Configuration

---

### 🎮 Phase 3: Controller Implementation (1-2 روز)

```
✅ Controller Setup
   - تزریق Dependencies
   - Authorization

✅ CRUD Actions
   - Index (GET)
   - Create (GET/POST)
   - Edit (GET/POST)
   - Details (GET)
   - Delete (POST)

✅ Strongly-Typed ViewModels
   ❌ حذف ViewBag/ViewData برای داده‌های اصلی
   ✅ استفاده از ViewModel

✅ GetViewPath() در Admin Area
   return View(GetViewPath("Create"), model);

✅ Notification Integration
   NotificationHelper.SetSuccess/Error/Warning/Info
```

**خروجی:**
- Controller Class با تمام Actions
- Strongly-Typed ViewModels
- Notification Integration

---

### 🎨 Phase 4: View Implementation (2-3 روز)

#### 📄 Index View

```
✅ Search Panel
✅ Data Table (Responsive)
✅ Pagination
✅ Action Buttons
❌ حذف Alert های Bootstrap
✅ SweetAlert برای Confirmations
```

#### ✏️ Create/Edit Views

```
✅ Form با enctype="multipart/form-data" (در صورت نیاز)
✅ Validation Messages
✅ Persian DatePicker
   - _PersianDatePicker Partial
   - _PersianDatePickerScript در Scripts
✅ CKEditor (در صورت نیاز)
   - _CKEditorScript
   - _CKEditorInit
✅ Image Upload (در صورت نیاز)
   - File Input با Preview
   - JavaScript Validation
❌ حذف Alert های Bootstrap
```

#### 📖 Details View

```
✅ Display Template
✅ Action Buttons
✅ Related Data Display
```

---

### 🎯 Phase 5: UI/UX Optimization (1 روز)

```
✅ Design Consistency
   - فونت Vazir یا IRANSansX
   - Card Components
   - Button Styles
   - Form Styles

✅ Responsive Design
   - تست Mobile
   - تست Tablet
   - تست Desktop

✅ Accessibility
   - Alt Text برای Images
   - Title برای Links
   - ARIA Labels
   - Keyboard Navigation
```

---

### 🎨 Phase 6: Color Scheme Standardization (0.5 روز)

```
❌ حذف رنگ‌های جیق و جلف
   - حذف Gradient های پیچیده
   - حذف رنگ‌های روشن و جیق
   - حذف رنگ‌های نئون

✅ پیاده‌سازی پالت رنگ استاندارد
   - --medical-primary: #2c5aa0
   - --medical-secondary: #6c757d
   - --medical-success: #28a745
   - --medical-danger: #dc3545
   - استفاده در تمام استایل‌ها

✅ بهینه‌سازی Header و Card Header
   - background-color: var(--medical-primary)
   - Border-radius: 12px حداکثر

✅ بهینه‌سازی Badge و Button
   - رنگ‌های ساده (نه Gradient)
   - Border-radius: 6px
```

---

### 🔔 Phase 7: Notification System (0.5 روز)

```
✅ Toastr Integration
   ❌ حذف تمام Alert های Bootstrap
   ✅ NotificationHelper.SetSuccess/Error/Warning/Info
   ✅ تست تمام پیام‌ها

✅ SweetAlert Integration
   ❌ جایگزینی confirm() با SweetAlert
   ✅ Delete Confirmation
   ✅ Publish/Unpublish Confirmation
```

---

### 📅 Phase 8: Persian DatePicker Integration (0.5 روز)

```
✅ View Implementation
   - جایگزینی datetime-local با _PersianDatePicker
   - تنظیم ViewBag برای هر فیلد
   - اضافه کردن _PersianDatePickerScript

✅ Controller Implementation
   - Parse تاریخ‌ها با ParseDateFromHiddenInput
   model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);

✅ Display Implementation
   - نمایش تاریخ شمسی با PersianDateHelper.ToPersianDate
   @PersianDateHelper.ToPersianDate(item.Date)

✅ Testing
   - تست انتخاب تاریخ
   - تست ذخیره و نمایش
```

---

### 📝 Phase 9: CKEditor Integration (0.5 روز - در صورت نیاز)

```
✅ بررسی نیاز
   - آیا فیلد متنی طولانی است؟
   - آیا نیاز به فرمت‌بندی است؟

✅ ViewModel Configuration
   [AllowHtml]
   public string Content { get; set; }

✅ Controller Configuration
   [ValidateInput(false)]

✅ View Implementation
   - TextArea با ID منحصر به فرد
   - _CKEditorScript در Scripts
   - _CKEditorInit با selector و height

✅ Testing
   - تست بارگذاری CKEditor
   - تست فرمت‌بندی فارسی
   - تست ذخیره و نمایش
```

---

### 🖼️ Phase 10: Image Upload System (1 روز - در صورت نیاز)

```
✅ Service Integration
   - تزریق IImageUploadService

✅ Controller Implementation
   - تعریف Constants (مسیرها، ابعاد)
   - پیاده‌سازی ProcessImageUpload
   - فراخوانی در Create/Edit Actions

✅ View Implementation
   - Form با enctype="multipart/form-data"
   - File Input با accept
   - Image Preview
   - JavaScript Validation

✅ Testing
   - تست آپلود تصویر
   - تست Thumbnail
   - تست Validation
```

---

### 🏥 Phase 11: Medical Form Design Standards (1 روز)

```
✅ اصول پایه
   - سادگی مطلق
   - رسمی و حرفه‌ای
   - حذف عناصر غیرضروری

✅ ساختار فرم
   - تقسیم‌بندی با Fieldset/Section

✅ رنگ‌بندی رسمی
   - پالت رنگ استاندارد
   ❌ حذف رنگ‌های ممنوع

✅ Input Design
   - Border ساده، Radius کم
   - Label و Placeholder مناسب

✅ Validation
   - Real-time Validation
   - پیام خطا واضح

✅ انیمیشن‌های مینیمال
   ✅ Fade-in ملایم (250ms)
   ❌ حذف Bounce, Shake

✅ دسترس‌پذیری
   - کنتراست رنگ
   - Tab Navigation
   - ARIA Labels

✅ بهینه‌سازی UX
   - Auto-focus روی فیلد بعدی
   - Mask برای موبایل/کد ملی
   - DatePicker شمسی
   - Auto-fill امن

✅ امنیت
   - HTTPS الزامی
   - Anti-Forgery Token
   - عدم ذخیره در LocalStorage
```

---

### 🧪 Phase 12: Testing & QA (1-2 روز)

```
✅ Unit Testing
   - Repository Methods
   - Service Methods
   - Controller Actions

✅ Integration Testing
   - End-to-End Flows
   - Database Operations

✅ Security Testing
   - SQL Injection Protection
   - XSS Protection
   - CSRF Protection

✅ Performance Testing
   - Page Load Time
   - Database Query Performance
```

---

### 📦 Phase 13: Deployment Preparation (0.5 روز)

```
✅ Pre-Deployment Checklist
   - تمام Tests پاس شده‌اند
   - تمام Linter Errors برطرف شده‌اند
   - تمام Documentation به‌روز است

✅ Production Configuration
   - Connection Strings
   - Logging Levels
   - Security Headers
```

---

## ⏱️ زمان‌بندی کلی

| Phase | توضیحات | زمان تخمینی |
|-------|---------|------------|
| 1 | Analysis & Design | 1-2 روز |
| 2 | Backend Implementation | 2-3 روز |
| 3 | Controller Implementation | 1-2 روز |
| 4 | View Implementation | 2-3 روز |
| 5 | UI/UX Optimization | 1 روز |
| 6 | Color Scheme | 0.5 روز |
| 7 | Notification System | 0.5 روز |
| 8 | Persian DatePicker | 0.5 روز |
| 9 | CKEditor (اختیاری) | 0.5 روز |
| 10 | Image Upload (اختیاری) | 1 روز |
| 11 | Medical Form Design | 1 روز |
| 12 | Testing & QA | 1-2 روز |
| 13 | Deployment | 0.5 روز |
| **کل** | | **12-17 روز** |

---

## 📋 Checklist نهایی قبل از Commit

### ✅ UI/UX
- [ ] فونت Vazir یا IRANSansX
- [ ] رنگ‌های استاندارد `--medical-*`
- [ ] هیچ رنگ جیق و جلف وجود ندارد
- [ ] هیچ گرادینت فانتزی وجود ندارد
- [ ] Border-radius مناسب (4px-12px)
- [ ] Responsive تست شده

### ✅ Strongly-Typed
- [ ] تمام View ها دارای `@model`
- [ ] هیچ `ViewBag`/`ViewData` برای داده‌های اصلی
- [ ] تمام Actions از `GetViewPath()` استفاده می‌کنند

### ✅ Bulletproof
- [ ] تمام async ها دارای try-catch
- [ ] تمام null reference بررسی شده
- [ ] تمام `ModelState` بررسی شده
- [ ] تمام `ServiceResult` بررسی شده

### ✅ SRP
- [ ] Controller: routing و orchestration
- [ ] Service: business logic
- [ ] Repository: data access

### ✅ Notifications
- [ ] تمام پیام‌ها با `NotificationHelper`
- [ ] تمام confirmations با SweetAlert2
- [ ] هیچ `alert()` یا `confirm()` ندارد
- [ ] هیچ Alert Bootstrap ندارد

### ✅ Persian DatePicker
- [ ] تمام فیلدهای تاریخ از `_PersianDatePicker`
- [ ] Controller ها از `ParseDateFromHiddenInput`
- [ ] هیچ `datetime-local` ندارد

### ✅ Image Upload (در صورت نیاز)
- [ ] `IImageUploadService` تزریق شده
- [ ] `ProcessImageUpload` پیاده‌سازی شده
- [ ] Form دارای `enctype="multipart/form-data"`
- [ ] Image Preview پیاده‌سازی شده

### ✅ CKEditor (در صورت نیاز)
- [ ] `[AllowHtml]` به ViewModel
- [ ] `[ValidateInput(false)]` به POST Action
- [ ] `_CKEditorScript` و `_CKEditorInit` بارگذاری شده

### ✅ Medical Forms
- [ ] ساختار با Section/Fieldset
- [ ] استایل Input حرفه‌ای
- [ ] Real-time Validation
- [ ] انیمیشن‌های مینیمال
- [ ] دسترس‌پذیری رعایت شده

### ✅ Security
- [ ] تمام inputs validated
- [ ] تمام forms دارای CSRF protection
- [ ] تمام SQL queries parameterized

---

## 🎯 نکات مهم

### ⚠️ قبل از شروع
1. ✅ مطالعه `Docs/DEVELOPMENT_CONTRACT.md`
2. ✅ مطالعه `Docs/Knowledge-Base/03-Development-Contract-Quick-Guide.md`
3. ✅ آماده کردن TODO List از Template

### ⚠️ در حین کار
1. ✅ هر Phase را به صورت کامل تمام کنید
2. ✅ Checklist ها را بررسی کنید
3. ✅ تست کنید قبل از رفتن به Phase بعدی

### ⚠️ قبل از Commit
1. ✅ تمام Checklist ها بررسی شوند
2. ✅ تمام تست‌ها پاس شوند
3. ✅ Code Review انجام شود

---

## 📚 مراجع سریع

### Documents
- `Docs/DEVELOPMENT_CONTRACT.md` - قرارداد توسعه کامل
- `Docs/TODO_TEMPLATE.md` - Template TODO کامل

### Knowledge Base
- `Docs/Knowledge-Base/03-Development-Contract-Quick-Guide.md` - راهنمای سریع قرارداد
- `Docs/Knowledge-Base/01-Helpers-DateTime.md` - Helper های تاریخ
- `Docs/Knowledge-Base/02-Helpers-Validation.md` - Helper های اعتبارسنجی
- `Docs/Knowledge-Base/06-Quick-Reference.md` - مرجع سریع

### Other Guides
- `Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md`
- `Docs/IMAGE_UPLOAD_SYSTEM_GUIDE.md`
- `Docs/CKEDITOR_USAGE_GUIDE.md`
- `Docs/NOTIFICATION_HELPER_USAGE_GUIDE.md`

---

## 💡 Template TODO List (کپی کنید)

```markdown
## 📋 TODO: [نام ماژول/Feature]

### Phase 1: Analysis & Design
- [ ] تحلیل نیازمندی‌ها
- [ ] طراحی Entity و Database
- [ ] طراحی ViewModels
- [ ] طراحی Interfaces

### Phase 2: Backend Implementation
- [ ] Repository Implementation
- [ ] Service Implementation
- [ ] Dependency Injection

### Phase 3: Controller Implementation
- [ ] CRUD Actions
- [ ] Strongly-Typed ViewModels
- [ ] GetViewPath() در Admin Area
- [ ] Notification Integration

### Phase 4: View Implementation
- [ ] Index View
- [ ] Create/Edit Views
- [ ] Details View

### Phase 5: UI/UX Optimization
- [ ] Design Consistency
- [ ] Responsive Design
- [ ] Accessibility

### Phase 6: Color Scheme Standardization
- [ ] حذف رنگ‌های جیق و جلف
- [ ] پیاده‌سازی پالت رنگ استاندارد

### Phase 7: Notification System
- [ ] Toastr Integration
- [ ] SweetAlert Integration

### Phase 8: Persian DatePicker Integration
- [ ] View Implementation
- [ ] Controller Implementation
- [ ] Testing

### Phase 9: CKEditor Integration (اختیاری)
- [ ] ViewModel Configuration
- [ ] Controller Configuration
- [ ] View Implementation

### Phase 10: Image Upload System (اختیاری)
- [ ] Service Integration
- [ ] Controller Implementation
- [ ] View Implementation

### Phase 11: Medical Form Design Standards
- [ ] اصول پایه
- [ ] ساختار فرم
- [ ] رنگ‌بندی رسمی
- [ ] Input Design
- [ ] Validation
- [ ] دسترس‌پذیری

### Phase 12: Testing & QA
- [ ] Unit Testing
- [ ] Integration Testing
- [ ] Security Testing
- [ ] Performance Testing

### Phase 13: Deployment Preparation
- [ ] Pre-Deployment Checklist
- [ ] Production Configuration

**زمان شروع:** ___________
**زمان پایان:** ___________
**وضعیت:** [ ] در حال انجام | [ ] تکمیل شده
```

---

**یادآوری:** این راهنما باید همیشه در دسترس باشد! 📌

