# 🎨 پرامپت طراحی ویوهای ماژول Appointment - محیط درمانی حرفه‌ای

**ماژول:** Appointment Booking System  
**محیط:** Medical Professional Environment  
**Target Device:** 100% Mobile-First Responsive  
**تاریخ:** ۱۴۰۳/۱۰/۰۹

---

## 📚 مرحله 0: مطالعه الزامی (MUST READ)

### قراردادهای الزامی:
1. **`Docs/DEVELOPMENT_CONTRACT.md`** - استانداردهای UI/UX (بخش 1 و 6)
2. **`Docs/Knowledge-Base/AI_ASSISTANT_MASTER_CONTRACT.md`** - نقش UX Expert
3. **`Docs/TODO_TEMPLATE.md`** - Phase 11: Medical Form Design Standards

### نکات کلیدی:
- ✅ رسمی و حرفه‌ای برای محیط درمانی
- ✅ فونت Vazir یا IRANSansX
- ✅ پالت رنگ `--medical-*`
- ✅ **هیچ رنگ جیق و جلف** (بنفش، صورتی، نارنجی تند)
- ✅ **هیچ گرادینت فانتزی**
- ✅ Mobile-First Responsive
- ✅ Persian DatePicker (نه datetime-local)
- ✅ SweetAlert2 برای Confirmations
- ✅ Toastr برای Notifications

---

## 🎯 هدف: طراحی ویوهای Appointment Module

شما باید **4 ویو اصلی** طراحی کنید:

1. **`Available.cshtml`** - نمایش نوبت‌های موجود (عمومی - بدون لاگین)
2. **`DoctorDetails.cshtml`** - جزئیات پزشک و رزومه
3. **`MyAppointments.cshtml`** - لیست نوبت‌های بیمار (نیاز به لاگین)
4. **`_AppointmentCard.cshtml`** - Partial برای کارت نوبت

---

## 🎨 استانداردهای طراحی Medical UI

### 1️⃣ پالت رنگ استاندارد (الزامی)

```css
/* ✅ رنگ‌های مجاز - فقط اینها! */
:root {
    /* Primary Colors */
    --medical-primary: #2c5aa0;      /* آبی تیره - اعتماد */
    --medical-secondary: #6c757d;    /* خاکستری */
    
    /* Status Colors */
    --medical-success: #28a745;      /* سبز ملایم */
    --medical-danger: #dc3545;       /* قرمز ملایم */
    --medical-warning: #ffc107;      /* زرد */
    --medical-info: #17a2b8;        /* آبی روشن */
    
    /* Background & Text */
    --medical-bg: #ffffff;
    --medical-bg-light: #f8f9fa;
    --medical-text: #212529;
    --medical-text-muted: #6c757d;
    --medical-border: #dee2e6;
    
    /* Button Hover */
    --medical-primary-hover: #1e4178;
    --medical-success-hover: #1e7e34;
}
```

```css
/* ❌ ممنوع - این رنگ‌ها اصلاً استفاده نشود! */
/* بنفش جیق: #9b59b6, #8e44ad */
/* صورتی: #e91e63, #f093fb */
/* نارنجی تند: #ff5722, #ff6b6b */
/* گرادینت‌های فانتزی: linear-gradient(...) */
```

---

### 2️⃣ فونت‌های حرفه‌ای فارسی

```css
/* ✅ اولویت 1 - Vazir (فعلی پروژه) */
font-family: 'Vazir', 'Tahoma', sans-serif;

/* ✅ اولویت 2 - IRANSansX (توصیه می‌شود) */
font-family: 'IRANSansX', 'Tahoma', sans-serif;

/* اندازه فونت‌ها */
--font-size-base: 14px;
--font-size-lg: 16px;
--font-size-sm: 12px;
--font-size-xs: 11px;

/* وزن فونت */
--font-weight-normal: 400;
--font-weight-medium: 500;
--font-weight-bold: 700;
```

---

### 3️⃣ Responsive Breakpoints (Mobile-First)

```css
/* Bootstrap 4 Breakpoints */
/* Extra Small (موبایل عمودی) */
@media (max-width: 575.98px) { /* xs */ }

/* Small (موبایل افقی) */
@media (min-width: 576px) and (max-width: 767.98px) { /* sm */ }

/* Medium (تبلت عمودی) */
@media (min-width: 768px) and (max-width: 991.98px) { /* md */ }

/* Large (تبلت افقی / لپتاپ کوچک) */
@media (min-width: 992px) and (max-width: 1199.98px) { /* lg */ }

/* Extra Large (دسکتاپ) */
@media (min-width: 1200px) { /* xl */ }
```

**اولویت طراحی:** xs → sm → md → lg → xl (Mobile First!)

---

## 📋 مرحله 1: Available.cshtml - نمایش نوبت‌های موجود

### 1.1 ساختار کلی

```html
@model ClinicApp.ViewModels.Patient.AvailableAppointmentsViewModel

@{
    ViewBag.Title = "رزرو نوبت آنلاین";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

@section Styles {
    <style>
        /* Custom Styles برای این صفحه */
    </style>
}

<!-- ⬇️ محتوا به ترتیب اولویت Mobile -->
<div class="appointment-booking-container">
    <!-- 1. Header Section -->
    <section class="page-header">...</section>
    
    <!-- 2. Filter Section (Sticky در موبایل) -->
    <section class="filter-section sticky-top">...</section>
    
    <!-- 3. Doctor List (Grid Responsive) -->
    <section class="doctor-list-section">...</section>
    
    <!-- 4. Time Slots Section (نمایش بعد از انتخاب پزشک) -->
    <section class="time-slots-section">...</section>
</div>

@section Scripts {
    <script>
        /* JavaScript Logic */
    </script>
}
```

---

### 1.2 Header Section (Mobile-Optimized)

```html
<!-- ✅ Header رسمی و ساده -->
<section class="page-header bg-medical-light py-3 py-md-4 mb-3 mb-md-4">
    <div class="container">
        <div class="row align-items-center">
            <!-- عنوان -->
            <div class="col-12 col-md-8">
                <h1 class="h3 h2-md mb-2 mb-md-0 text-medical-primary">
                    <i class="fas fa-calendar-check ml-2"></i>
                    رزرو نوبت آنلاین
                </h1>
                <p class="text-muted mb-0 d-none d-md-block">
                    انتخاب پزشک و زمان مناسب برای ویزیت
                </p>
            </div>
            
            <!-- دکمه راهنما (فقط دسکتاپ) -->
            <div class="col-md-4 text-left d-none d-md-block">
                <button type="button" class="btn btn-outline-medical-info btn-sm" 
                        data-toggle="modal" data-target="#helpModal">
                    <i class="fas fa-question-circle ml-1"></i>
                    راهنما
                </button>
            </div>
        </div>
    </div>
</section>
```

**CSS:**
```css
.page-header {
    background: var(--medical-bg-light);
    border-bottom: 2px solid var(--medical-border);
}

.text-medical-primary {
    color: var(--medical-primary) !important;
}

/* Responsive Font Sizes */
.h3 { font-size: 1.5rem; }

@media (min-width: 768px) {
    .h2-md { font-size: 2rem !important; }
}
```

---

### 1.3 Filter Section (Sticky + Mobile-First)

```html
<!-- ✅ فیلتر Sticky در موبایل -->
<section class="filter-section bg-white shadow-sm mb-3 mb-md-4 sticky-top-mobile">
    <div class="container">
        <div class="filter-card p-3 p-md-4">
            <!-- فرم جستجو -->
            <form id="filterForm" class="row g-2 g-md-3">
                
                <!-- جستجوی پزشک (Full Width در موبایل) -->
                <div class="col-12 col-md-6 col-lg-4">
                    <label class="form-label text-medical-text font-weight-medium">
                        <i class="fas fa-search text-medical-info ml-1"></i>
                        جستجوی پزشک
                    </label>
                    <input type="text" 
                           class="form-control form-control-medical"
                           id="searchDoctorInput"
                           placeholder="نام، تخصص یا کد نظام پزشکی..."
                           autocomplete="off">
                </div>
                
                <!-- انتخاب تخصص -->
                <div class="col-12 col-md-6 col-lg-4">
                    <label class="form-label text-medical-text font-weight-medium">
                        <i class="fas fa-stethoscope text-medical-info ml-1"></i>
                        تخصص
                    </label>
                    <select class="form-control form-control-medical" 
                            id="specializationSelect">
                        <option value="">همه تخصص‌ها</option>
                        <option value="general">پزشک عمومی</option>
                        <option value="pediatrics">کودکان</option>
                        <!-- ... -->
                    </select>
                </div>
                
                <!-- تاریخ (Persian DatePicker) -->
                <div class="col-12 col-md-6 col-lg-4">
                    <label class="form-label text-medical-text font-weight-medium">
                        <i class="fas fa-calendar-alt text-medical-info ml-1"></i>
                        تاریخ ویزیت
                    </label>
                    
                    @{
                        ViewBag.PersianDatePickerId = "appointmentDatePicker";
                        ViewBag.PersianDatePickerName = "AppointmentDate";
                        ViewBag.PersianDatePickerValue = Model.SelectedDate;
                        ViewBag.PersianDatePickerLabel = "";
                        ViewBag.PersianDatePickerPlaceholder = "انتخاب تاریخ";
                        ViewBag.PersianDatePickerRequired = false;
                    }
                    @Html.Partial("_PersianDatePicker")
                </div>
                
                <!-- دکمه جستجو (Full Width در موبایل) -->
                <div class="col-12 col-lg-12 mt-2">
                    <button type="button" 
                            id="btnSearch" 
                            class="btn btn-medical-primary btn-block btn-lg-auto">
                        <i class="fas fa-search ml-1"></i>
                        جستجو
                    </button>
                    <button type="button" 
                            id="btnClearFilter" 
                            class="btn btn-outline-secondary btn-sm mr-2 d-none d-md-inline-block">
                        <i class="fas fa-times ml-1"></i>
                        پاک کردن
                    </button>
                </div>
                
            </form>
        </div>
    </div>
</section>
```

**CSS:**
```css
/* Sticky در موبایل فقط */
@media (max-width: 767.98px) {
    .sticky-top-mobile {
        position: sticky;
        top: 0;
        z-index: 1020;
        background: white;
    }
}

/* Form Controls */
.form-control-medical {
    border: 1px solid var(--medical-border);
    border-radius: 6px;
    padding: 0.75rem 1rem;
    font-size: 14px;
    transition: all 0.2s ease;
}

.form-control-medical:focus {
    border-color: var(--medical-primary);
    box-shadow: 0 0 0 0.2rem rgba(44, 90, 160, 0.15);
    outline: none;
}

/* Button Styles */
.btn-medical-primary {
    background-color: var(--medical-primary);
    border-color: var(--medical-primary);
    color: white;
    font-weight: 500;
    padding: 0.75rem 1.5rem;
    border-radius: 6px;
    transition: all 0.2s ease;
}

.btn-medical-primary:hover {
    background-color: var(--medical-primary-hover);
    border-color: var(--medical-primary-hover);
    transform: translateY(-1px);
    box-shadow: 0 4px 8px rgba(44, 90, 160, 0.2);
}

/* Full Width در موبایل */
@media (max-width: 991.98px) {
    .btn-block { width: 100%; }
}

@media (min-width: 992px) {
    .btn-lg-auto { width: auto; }
}
```

---

### 1.4 Doctor List (Card Grid Responsive)

```html
<!-- ✅ Grid Responsive: 1 col (mobile) → 2 col (tablet) → 3 col (desktop) -->
<section class="doctor-list-section">
    <div class="container">
        
        <!-- Loading State -->
        <div id="loadingState" class="text-center py-5 d-none">
            <div class="spinner-border text-medical-primary" role="status">
                <span class="sr-only">در حال بارگذاری...</span>
            </div>
            <p class="text-muted mt-3">در حال جستجوی پزشکان...</p>
        </div>
        
        <!-- Doctors Grid -->
        <div id="doctorsGrid" class="row">
            
            @foreach (var doctor in Model.Doctors)
            {
                <div class="col-12 col-md-6 col-lg-4 mb-3 mb-md-4 doctor-card-col">
                    <!-- Doctor Card -->
                    <div class="card doctor-card h-100 shadow-sm hover-shadow">
                        
                        <!-- آواتار + بج -->
                        <div class="card-img-top-wrapper position-relative">
                            <img src="@(doctor.ProfileImageUrl ?? "/Content/Images/default-doctor.png")" 
                                 class="card-img-top doctor-avatar"
                                 alt="@doctor.FullName">
                            
                            @if (doctor.HasActiveSchedule)
                            {
                                <span class="badge badge-success position-absolute available-badge">
                                    <i class="fas fa-check-circle ml-1"></i>
                                    قابل رزرو
                                </span>
                            }
                        </div>
                        
                        <!-- محتوا -->
                        <div class="card-body">
                            <!-- نام -->
                            <h3 class="card-title h5 text-medical-primary font-weight-bold mb-2">
                                @doctor.FullName
                            </h3>
                            
                            <!-- تخصص -->
                            <p class="card-subtitle text-medical-text-muted mb-3">
                                <i class="fas fa-stethoscope text-medical-info ml-1"></i>
                                @doctor.Specialization
                            </p>
                            
                            <!-- کد نظام پزشکی -->
                            @if (!string.IsNullOrEmpty(doctor.MedicalCouncilCode))
                            {
                                <p class="small text-muted mb-2">
                                    <i class="fas fa-id-card ml-1"></i>
                                    کد نظام پزشکی: <span class="font-weight-medium">@doctor.MedicalCouncilCode</span>
                                </p>
                            }
                            
                            <!-- سابقه -->
                            @if (doctor.ExperienceYears.HasValue && doctor.ExperienceYears.Value > 0)
                            {
                                <p class="small text-muted mb-3">
                                    <i class="fas fa-briefcase-medical ml-1"></i>
                                    @doctor.ExperienceYears سال سابقه
                                </p>
                            }
                            
                            <!-- بیوگرافی کوتاه -->
                            @if (!string.IsNullOrEmpty(doctor.Bio))
                            {
                                <p class="card-text text-justify line-clamp-2 mb-3">
                                    @doctor.Bio
                                </p>
                            }
                        </div>
                        
                        <!-- Action Buttons -->
                        <div class="card-footer bg-white border-top">
                            <div class="row g-2">
                                <!-- رزرو نوبت -->
                                <div class="col-12 col-sm-6">
                                    <button type="button" 
                                            class="btn btn-medical-primary btn-sm btn-block btn-select-doctor"
                                            data-doctor-id="@doctor.DoctorId"
                                            @(!doctor.HasActiveSchedule ? "disabled" : "")>
                                        <i class="fas fa-calendar-plus ml-1"></i>
                                        رزرو نوبت
                                    </button>
                                </div>
                                
                                <!-- جزئیات -->
                                <div class="col-12 col-sm-6">
                                    <a href="@Url.Action("DoctorDetails", new { doctorId = doctor.DoctorId })"
                                       class="btn btn-outline-medical-info btn-sm btn-block">
                                        <i class="fas fa-info-circle ml-1"></i>
                                        مشاهده رزومه
                                    </a>
                                </div>
                            </div>
                        </div>
                        
                    </div>
                </div>
            }
            
        </div>
        
        <!-- Empty State -->
        <div id="emptyState" class="text-center py-5 @(Model.Doctors.Any() ? "d-none" : "")">
            <i class="fas fa-user-md fa-3x text-muted mb-3"></i>
            <h4 class="text-muted">پزشکی با این مشخصات یافت نشد</h4>
            <p class="text-muted">لطفاً فیلتر جستجو را تغییر دهید</p>
        </div>
        
    </div>
</section>
```

**CSS:**
```css
/* Doctor Card */
.doctor-card {
    border: 1px solid var(--medical-border);
    border-radius: 12px;
    overflow: hidden;
    transition: all 0.3s ease;
}

.doctor-card:hover {
    transform: translateY(-4px);
}

.hover-shadow:hover {
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12) !important;
}

/* Avatar */
.doctor-avatar {
    width: 100%;
    height: 200px;
    object-fit: cover;
    object-position: center;
}

/* بج "قابل رزرو" */
.available-badge {
    top: 10px;
    right: 10px;
    font-size: 12px;
    padding: 0.4rem 0.8rem;
    border-radius: 20px;
}

/* Line Clamp برای Bio */
.line-clamp-2 {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
    line-height: 1.5;
    max-height: 3em; /* 2 lines */
}

/* Button Sizes در موبایل */
@media (max-width: 575.98px) {
    .btn-sm {
        font-size: 13px;
        padding: 0.5rem 0.75rem;
    }
}
```

---

### 1.5 Time Slots Section (نمایش پویا)

```html
<!-- ✅ زمان‌های در دسترس (نمایش بعد از انتخاب پزشک) -->
<section id="timeSlotsSection" class="time-slots-section d-none">
    <div class="container">
        <div class="card shadow-sm">
            <div class="card-header bg-medical-primary text-white">
                <h4 class="mb-0">
                    <i class="fas fa-clock ml-2"></i>
                    انتخاب زمان ویزیت
                </h4>
            </div>
            
            <div class="card-body">
                
                <!-- اطلاعات پزشک انتخاب شده -->
                <div class="selected-doctor-info mb-4 p-3 bg-medical-light rounded">
                    <div class="row align-items-center">
                        <div class="col-auto d-none d-md-block">
                            <img id="selectedDoctorAvatar" 
                                 src="" 
                                 class="rounded-circle"
                                 style="width: 60px; height: 60px; object-fit: cover;"
                                 alt="">
                        </div>
                        <div class="col">
                            <h5 id="selectedDoctorName" class="mb-1"></h5>
                            <p id="selectedDoctorSpec" class="text-muted mb-0 small"></p>
                        </div>
                        <div class="col-auto">
                            <button type="button" 
                                    id="btnChangeDoctor" 
                                    class="btn btn-outline-secondary btn-sm">
                                <i class="fas fa-exchange-alt ml-1"></i>
                                تغییر پزشک
                            </button>
                        </div>
                    </div>
                </div>
                
                <!-- Loading -->
                <div id="slotsLoading" class="text-center py-4">
                    <div class="spinner-border text-medical-primary"></div>
                    <p class="text-muted mt-2">در حال بارگذاری زمان‌های در دسترس...</p>
                </div>
                
                <!-- Time Slots Grid -->
                <div id="timeSlotsGrid" class="d-none">
                    <!-- زمان‌ها به صورت Grid نمایش داده می‌شوند -->
                </div>
                
                <!-- Empty Slots -->
                <div id="emptySlots" class="text-center py-4 d-none">
                    <i class="fas fa-calendar-times fa-2x text-muted mb-3"></i>
                    <p class="text-muted">برای این تاریخ زمانی موجود نیست</p>
                </div>
                
            </div>
        </div>
    </div>
</section>
```

**JavaScript برای Time Slots:**
```javascript
// نمایش Time Slots به صورت Grid
function displayTimeSlots(slots) {
    const grid = $('#timeSlotsGrid');
    grid.empty();
    
    if (!slots || slots.length === 0) {
        $('#emptySlots').removeClass('d-none');
        $('#slotsLoading').addClass('d-none');
        grid.addClass('d-none');
        return;
    }
    
    // Grid: 2 col (mobile) → 3 col (tablet) → 4 col (desktop)
    const rowDiv = $('<div class="row g-2."></div>');
    
    slots.forEach(slot => {
        const colDiv = $(`
            <div class="col-6 col-sm-4 col-md-3">
                <button type="button" 
                        class="btn btn-outline-medical-primary btn-time-slot btn-block"
                        data-start="${slot.startTime}"
                        data-end="${slot.endTime}"
                        ${!slot.isAvailable ? 'disabled' : ''}>
                    <div class="time-text">${slot.displayTime}</div>
                    <div class="duration-text small text-muted">${slot.duration} دقیقه</div>
                </button>
            </div>
        `);
        
        rowDiv.append(colDiv);
    });
    
    grid.html(rowDiv);
    grid.removeClass('d-none');
    $('#slotsLoading').addClass('d-none');
    $('#emptySlots').addClass('d-none');
}
```

**CSS:**
```css
.btn-time-slot {
    padding: 1rem 0.5rem;
    border-radius: 8px;
    border: 2px solid var(--medical-border);
    transition: all 0.2s ease;
    text-align: center;
}

.btn-time-slot:not(:disabled):hover {
    border-color: var(--medical-primary);
    background-color: rgba(44, 90, 160, 0.05);
    transform: scale(1.05);
}

.btn-time-slot:disabled {
    opacity: 0.4;
    cursor: not-allowed;
}

.time-text {
    font-weight: 600;
    font-size: 1rem;
    color: var(--medical-primary);
}

.duration-text {
    font-size: 0.75rem;
    margin-top: 0.25rem;
}
```

---

## 📋 مرحله 2: DoctorDetails.cshtml

### 2.1 ساختار کلی

```html
@model ClinicApp.ViewModels.Patient.DoctorDetailsViewModel

@{
    ViewBag.Title = $"دکتر {Model.Doctor.FullName}";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<div class="doctor-details-container">
    <!-- 1. Header با عکس Cover -->
    <section class="doctor-header">...</section>
    
    <!-- 2. Tabs (رزومه، نوبت‌ها، نظرات) -->
    <section class="doctor-tabs">...</section>
    
    <!-- 3. محتوای Tabs -->
    <section class="tab-content">...</section>
</div>
```

---

## 📋 مرحله 3: MyAppointments.cshtml

### 3.1 لیست نوبت‌ها با Status Badges

```html
<!-- ✅ کارت نوبت -->
<div class="card appointment-card mb-3">
    <div class="card-body">
        <div class="row">
            <!-- Status Badge -->
            <div class="col-12 mb-2">
                @switch (appointment.Status)
                {
                    case AppointmentStatus.Pending:
                        <span class="badge badge-warning">
                            <i class="fas fa-clock ml-1"></i>
                            در انتظار تأیید
                        </span>
                        break;
                    case AppointmentStatus.Confirmed:
                        <span class="badge badge-success">
                            <i class="fas fa-check-circle ml-1"></i>
                            تأیید شده
                        </span>
                        break;
                    case AppointmentStatus.Cancelled:
                        <span class="badge badge-danger">
                            <i class="fas fa-times-circle ml-1"></i>
                            لغو شده
                        </span>
                        break;
                }
            </div>
            
            <!-- اطلاعات -->
            <div class="col-md-8">
                <h5 class="mb-2">
                    <i class="fas fa-user-md text-medical-primary ml-1"></i>
                    @appointment.DoctorName
                </h5>
                <p class="text-muted mb-1">
                    <i class="fas fa-calendar ml-1"></i>
                    @PersianDateHelper.ToPersianDate(appointment.AppointmentDate)
                </p>
                <p class="text-muted mb-0">
                    <i class="fas fa-clock ml-1"></i>
                    @appointment.TimeSlot
                </p>
            </div>
            
            <!-- دکمه‌ها -->
            <div class="col-md-4 text-left">
                @if (appointment.Status == AppointmentStatus.Confirmed)
                {
                    <button class="btn btn-danger btn-sm btn-cancel-appointment"
                            data-id="@appointment.AppointmentId">
                        <i class="fas fa-times ml-1"></i>
                        لغو نوبت
                    </button>
                }
            </div>
        </div>
    </div>
</div>
```

---

## ✅ چک‌لیست کیفی UI/UX (50+ مورد)

### ✅ **رنگ و طراحی**
```bash
- [ ] فقط از پالت `--medical-*` استفاده شده
- [ ] هیچ رنگ جیق و جلف (بنفش، صورتی) وجود ندارد
- [ ] هیچ گرادینت فانتزی وجود ندارد
- [ ] Border-radius حداکثر 12px
- [ ] Shadow ملایم (نه سنگین)
```

### ✅ **فونت و متن**
```bash
- [ ] فونت Vazir یا IRANSansX استفاده شده
- [ ] اندازه فونت: حداقل 14px (موبایل)
- [ ] Line-height: حداقل 1.5
- [ ] تمام متون راست‌چین (RTL)
- [ ] فاصله کافی بین خطوط
```

### ✅ **Responsive**
```bash
- [ ] Mobile-First Design
- [ ] Grid: 1 col (xs) → 2 col (md) → 3 col (lg)
- [ ] تست در iPhone SE (375px)
- [ ] تست در iPad (768px)
- [ ] تست در Desktop (1920px)
- [ ] Touch-friendly buttons (min 44×44px)
```

### ✅ **Persian DatePicker**
```bash
- [ ] استفاده از `_PersianDatePicker` partial
- [ ] اضافه کردن `_PersianDatePickerScript` در Scripts
- [ ] Parse با `ParseDateFromHiddenInput` در Controller
- [ ] هیچ `datetime-local` وجود ندارد
```

### ✅ **Notifications**
```bash
- [ ] Toastr برای Success/Error/Warning
- [ ] SweetAlert2 برای Confirmations
- [ ] هیچ `alert()` یا Bootstrap Alert
```

### ✅ **Accessibility**
```bash
- [ ] تمام Images دارای `alt` attribute
- [ ] تمام Buttons دارای ARIA labels
- [ ] Contrast Ratio مناسب (4.5:1)
- [ ] Keyboard Navigation کار می‌کند
- [ ] تست با Screen Reader
```

### ✅ **Performance**
```bash
- [ ] تصاویر Lazy Load
- [ ] CSS Minified
- [ ] JavaScript Minified
- [ ] Page Load < 3 seconds
```

---

## 🚀 استفاده از پرامپت

### برای Cursor/AI:
```
این پرامپت را به Cursor بدهید و بگویید:

"طبق این پرامپت، ویوهای زیر را اصلاح کن:
1. Available.cshtml
2. DoctorDetails.cshtml  
3. MyAppointments.cshtml

تمام استانداردهای موجود در پرامپت را رعایت کن.
کد CSS و JavaScript مربوط به هر View را هم بنویس."
```

---

**تهیه‌کننده:** AI UX Expert  
**تاریخ:** ۱۴۰۳/۱۰/۰۹  
**محیط:** Medical Professional - Mobile-First  
**وضعیت:** ✅ Production-Ready Prompt
