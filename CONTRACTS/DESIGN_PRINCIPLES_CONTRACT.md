# 🎨 **قرارداد اصول طراحی - ClinicApp**

## 🎯 **هدف:**
این قرارداد شامل تمام اصول طراحی، استانداردهای UI/UX و بهترین شیوه‌های پیاده‌سازی برای سیستم ClinicApp است.

---

## 🚫 **ممنوعیت‌های مطلق طراحی:**

### **1. استفاده از رنگ‌های غیراستاندارد:**
```css
/* ❌ ممنوع - هرگز استفاده نکنید */
background-color: #FF0000; /* قرمز خام */
color: #00FF00; /* سبز خام */
border: 1px solid black; /* سیاه خام */

/* ✅ صحیح - همیشه از پالت رنگی استاندارد استفاده کنید */
background-color: #dc3545; /* Bootstrap danger */
color: #28a745; /* Bootstrap success */
border: 2px solid #e9ecef; /* Bootstrap light */
```

### **2. استفاده از فونت‌های غیراستاندارد:**
```css
/* ❌ ممنوع - هرگز استفاده نکنید */
font-family: "Comic Sans MS", cursive;
font-size: 18pt;
font-weight: bold;

/* ✅ صحیح - همیشه از فونت‌های استاندارد استفاده کنید */
font-family: "Vazir", "Tahoma", sans-serif;
font-size: 1rem;
font-weight: 600;
```

### **3. استفاده از انیمیشن‌های غیرضروری:**
```css
/* ❌ ممنوع - هرگز استفاده نکنید */
animation: bounce 2s infinite;
transform: rotate(360deg);

/* ✅ صحیح - فقط انیمیشن‌های ضروری استفاده کنید */
transition: all 0.3s ease;
transform: scale(1.05);
```

### **4. عدم استفاده از Anti-Forgery Token:**
```html
<!-- ❌ ممنوع - هرگز استفاده نکنید -->
<form method="post">
    <!-- بدون @Html.AntiForgeryToken() -->
</form>

<!-- ✅ صحیح - همیشه از Anti-Forgery Token استفاده کنید -->
<form method="post">
    @Html.AntiForgeryToken()  <!-- اجباری برای امنیت -->
    <!-- محتوای فرم -->
</form>
```

```csharp
// ❌ ممنوع - هرگز استفاده نکنید
[HttpPost]
public ActionResult ActionName(Model model) // بدون ValidateAntiForgeryToken

// ✅ صحیح - همیشه از ValidateAntiForgeryToken استفاده کنید
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult ActionName(Model model)
```

### **5. عدم استفاده از ServiceResult Enhanced:**
```csharp
// ❌ ممنوع - هرگز استفاده نکنید
public async Task<string> CreateSchedule(Model model) // return string
public async Task<Exception> UpdateSchedule(Model model) // return Exception

// ✅ صحیح - همیشه از ServiceResult Enhanced استفاده کنید
public async Task<ServiceResult<Schedule>> CreateSchedule(Model model)
public async Task<ServiceResult<Schedule>> UpdateSchedule(Model model)
```

```csharp
// ❌ ممنوع - هرگز استفاده نکنید
RuleFor(x => x.DoctorId)
    .GreaterThan(0)
    .WithMessage("شناسه پزشک نامعتبر است.");
    // بدون WithErrorCode

// ✅ صحیح - همیشه از WithErrorCode استفاده کنید
RuleFor(x => x.DoctorId)
    .GreaterThan(0)
    .WithMessage("شناسه پزشک نامعتبر است.")
    .WithErrorCode("INVALID_DOCTOR_ID");
```

---

## ✅ **استانداردهای اجباری طراحی:**

### **1. پالت رنگی استاندارد:**
```css
:root {
    /* Primary Colors */
    --primary-100: #e3f2fd;
    --primary-500: #2196f3;
    --primary-700: #1976d2;
    --primary-900: #0d47a1;
    
    /* Secondary Colors */
    --secondary-100: #f3e5f5;
    --secondary-500: #9c27b0;
    --secondary-700: #7b1fa2;
    --secondary-900: #4a148c;
    
    /* Success Colors */
    --success-100: #e8f5e8;
    --success-500: #4caf50;
    --success-700: #388e3c;
    --success-900: #1b5e20;
    
    /* Warning Colors */
    --warning-100: #fff8e1;
    --warning-500: #ff9800;
    --warning-700: #f57c00;
    --warning-900: #e65100;
    
    /* Danger Colors */
    --danger-100: #ffebee;
    --danger-500: #f44336;
    --danger-700: #d32f2f;
    --danger-900: #b71c1c;
    
    /* Neutral Colors */
    --neutral-50: #fafafa;
    --neutral-100: #f5f5f5;
    --neutral-200: #eeeeee;
    --neutral-300: #e0e0e0;
    --neutral-400: #bdbdbd;
    --neutral-500: #9e9e9e;
    --neutral-600: #757575;
    --neutral-700: #616161;
    --neutral-800: #424242;
    --neutral-900: #212121;
}
```

### **2. Typography Standards:**
```css
/* Headings */
h1, .h1 {
    font-size: 2.5rem;
    font-weight: 700;
    line-height: 1.2;
    color: var(--neutral-900);
    margin-bottom: 1.5rem;
}

h2, .h2 {
    font-size: 2rem;
    font-weight: 600;
    line-height: 1.3;
    color: var(--neutral-800);
    margin-bottom: 1.25rem;
}

h3, .h3 {
    font-size: 1.75rem;
    font-weight: 600;
    line-height: 1.4;
    color: var(--neutral-800);
    margin-bottom: 1rem;
}

h4, .h4 {
    font-size: 1.5rem;
    font-weight: 500;
    line-height: 1.4;
    color: var(--neutral-700);
    margin-bottom: 0.75rem;
}

h5, .h5 {
    font-size: 1.25rem;
    font-weight: 500;
    line-height: 1.5;
    color: var(--neutral-700);
    margin-bottom: 0.5rem;
}

h6, .h6 {
    font-size: 1rem;
    font-weight: 500;
    line-height: 1.5;
    color: var(--neutral-600);
    margin-bottom: 0.5rem;
}

/* Body Text */
body, .body-text {
    font-size: 1rem;
    font-weight: 400;
    line-height: 1.6;
    color: var(--neutral-700);
}

/* Small Text */
.small-text {
    font-size: 0.875rem;
    font-weight: 400;
    line-height: 1.5;
    color: var(--neutral-600);
}

/* Caption Text */
.caption-text {
    font-size: 0.75rem;
    font-weight: 400;
    line-height: 1.4;
    color: var(--neutral-500);
}
```

### **3. Spacing Standards:**
```css
:root {
    /* Spacing Scale */
    --spacing-xs: 0.25rem;   /* 4px */
    --spacing-sm: 0.5rem;    /* 8px */
    --spacing-md: 1rem;      /* 16px */
    --spacing-lg: 1.5rem;    /* 24px */
    --spacing-xl: 2rem;      /* 32px */
    --spacing-2xl: 3rem;     /* 48px */
    --spacing-3xl: 4rem;     /* 64px */
}

/* Usage Examples */
.margin-xs { margin: var(--spacing-xs); }
.margin-sm { margin: var(--spacing-sm); }
.margin-md { margin: var(--spacing-md); }
.margin-lg { margin: var(--spacing-lg); }
.margin-xl { margin: var(--spacing-xl); }

.padding-xs { padding: var(--spacing-xs); }
.padding-sm { padding: var(--spacing-sm); }
.padding-md { padding: var(--spacing-md); }
.padding-lg { padding: var(--spacing-lg); }
.padding-xl { padding: var(--spacing-xl); }
```

---

## 🎨 **استانداردهای UI Components:**

### **1. Buttons:**
```css
/* Primary Button */
.btn-primary {
    background: linear-gradient(135deg, var(--primary-500) 0%, var(--primary-700) 100%);
    border: none;
    border-radius: 25px;
    padding: 0.75rem 2rem;
    font-weight: 600;
    font-size: 1rem;
    color: white;
    transition: all 0.3s ease;
    box-shadow: 0 4px 15px rgba(33, 150, 243, 0.3);
}

.btn-primary:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 20px rgba(33, 150, 243, 0.4);
}

/* Secondary Button */
.btn-secondary {
    background: linear-gradient(135deg, var(--secondary-500) 0%, var(--secondary-700) 100%);
    border: none;
    border-radius: 25px;
    padding: 0.75rem 2rem;
    font-weight: 600;
    font-size: 1rem;
    color: white;
    transition: all 0.3s ease;
    box-shadow: 0 4px 15px rgba(156, 39, 176, 0.3);
}

/* Success Button */
.btn-success {
    background: linear-gradient(135deg, var(--success-500) 0%, var(--success-700) 100%);
    border: none;
    border-radius: 25px;
    padding: 0.75rem 2rem;
    font-weight: 600;
    font-size: 1rem;
    color: white;
    transition: all 0.3s ease;
    box-shadow: 0 4px 15px rgba(76, 175, 80, 0.3);
}

/* Danger Button */
.btn-danger {
    background: linear-gradient(135deg, var(--danger-500) 0%, var(--danger-700) 100%);
    border: none;
    border-radius: 25px;
    padding: 0.75rem 2rem;
    font-weight: 600;
    font-size: 1rem;
    color: white;
    transition: all 0.3s ease;
    box-shadow: 0 4px 15px rgba(244, 67, 54, 0.3);
}

/* Outline Button */
.btn-outline-primary {
    background: transparent;
    border: 2px solid var(--primary-500);
    border-radius: 25px;
    padding: 0.75rem 2rem;
    font-weight: 600;
    font-size: 1rem;
    color: var(--primary-500);
    transition: all 0.3s ease;
}

.btn-outline-primary:hover {
    background: var(--primary-500);
    color: white;
    transform: translateY(-2px);
}
```

### **2. Form Controls:**
```css
/* Input Fields */
.form-control, .form-select {
    border-radius: 10px;
    border: 2px solid var(--neutral-300);
    padding: 0.75rem 1rem;
    font-size: 1rem;
    font-weight: 400;
    color: var(--neutral-700);
    background-color: white;
    transition: all 0.3s ease;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.form-control:focus, .form-select:focus {
    border-color: var(--primary-500);
    box-shadow: 0 0 0 0.2rem rgba(33, 150, 243, 0.25);
    outline: none;
}

.form-control:disabled, .form-select:disabled {
    background-color: var(--neutral-100);
    color: var(--neutral-500);
    cursor: not-allowed;
}

/* Form Labels */
.form-label {
    font-weight: 600;
    color: var(--neutral-700);
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
}

/* Form Groups */
.form-group {
    margin-bottom: var(--spacing-lg);
}

/* Validation States */
.form-control.is-valid {
    border-color: var(--success-500);
    box-shadow: 0 0 0 0.2rem rgba(76, 175, 80, 0.25);
}

.form-control.is-invalid {
    border-color: var(--danger-500);
    box-shadow: 0 0 0 0.2rem rgba(244, 67, 54, 0.25);
}

.valid-feedback {
    color: var(--success-700);
    font-size: 0.875rem;
    margin-top: 0.25rem;
}

.invalid-feedback {
    color: var(--danger-700);
    font-size: 0.875rem;
    margin-top: 0.25rem;
}
```

### **3. Cards:**
```css
/* Basic Card */
.card {
    background: white;
    border-radius: 15px;
    border: none;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
    transition: all 0.3s ease;
    overflow: hidden;
}

.card:hover {
    transform: translateY(-5px);
    box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
}

/* Card Header */
.card-header {
    background: linear-gradient(135deg, var(--primary-500) 0%, var(--primary-700) 100%);
    color: white;
    border: none;
    padding: 1.5rem;
    font-weight: 600;
    font-size: 1.1rem;
}

/* Card Body */
.card-body {
    padding: 1.5rem;
}

/* Card Footer */
.card-footer {
    background-color: var(--neutral-50);
    border: none;
    padding: 1rem 1.5rem;
    border-top: 1px solid var(--neutral-200);
}

/* Interactive Card */
.card-interactive {
    cursor: pointer;
    transition: all 0.3s ease;
}

.card-interactive:hover {
    transform: translateY(-3px);
    box-shadow: 0 6px 25px rgba(0, 0, 0, 0.15);
}

/* Card with Image */
.card-img-top {
    height: 200px;
    object-fit: cover;
    border-radius: 15px 15px 0 0;
}
```

### **4. Tables:**
```css
/* Table Container */
.table-responsive {
    border-radius: 15px;
    overflow: hidden;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

/* Table */
.table {
    margin-bottom: 0;
    background: white;
}

.table th {
    background: linear-gradient(135deg, var(--primary-500) 0%, var(--primary-700) 100%);
    color: white;
    font-weight: 600;
    border: none;
    padding: 1rem;
    font-size: 0.9rem;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.table td {
    padding: 1rem;
    border-bottom: 1px solid var(--neutral-200);
    vertical-align: middle;
    color: var(--neutral-700);
}

.table tbody tr {
    transition: all 0.3s ease;
}

.table tbody tr:hover {
    background-color: var(--primary-50);
    transform: scale(1.01);
}

/* Striped Table */
.table-striped tbody tr:nth-of-type(odd) {
    background-color: var(--neutral-50);
}

/* Bordered Table */
.table-bordered th,
.table-bordered td {
    border: 1px solid var(--neutral-200);
}
```

---

## 🎭 **استانداردهای Modal:**

### **1. Modal Container:**
```css
/* Modal Content */
.modal-content {
    border-radius: 20px;
    border: none;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
    overflow: hidden;
}

/* Modal Header */
.modal-header {
    background: linear-gradient(135deg, var(--primary-500) 0%, var(--primary-700) 100%);
    color: white;
    border-radius: 20px 20px 0 0;
    border: none;
    padding: 1.5rem 2rem;
}

.modal-title {
    font-weight: 600;
    font-size: 1.25rem;
}

.modal-header .btn-close {
    filter: invert(1);
    opacity: 0.8;
}

/* Modal Body */
.modal-body {
    padding: 2rem;
    background: white;
}

/* Modal Footer */
.modal-footer {
    background-color: var(--neutral-50);
    border: none;
    padding: 1.5rem 2rem;
    border-top: 1px solid var(--neutral-200);
}

/* Modal Sizes */
.modal-sm .modal-content {
    max-width: 400px;
}

.modal-lg .modal-content {
    max-width: 800px;
}

.modal-xl .modal-content {
    max-width: 1140px;
}
```

---

## 📱 **Responsive Design Standards:**

### **1. Breakpoints:**
```css
/* Extra small devices (phones, 576px and down) */
@media (max-width: 575.98px) {
    .container {
        padding-left: 1rem;
        padding-right: 1rem;
    }
    
    .card-body {
        padding: 1rem;
    }
    
    .modal-body {
        padding: 1rem;
    }
}

/* Small devices (landscape phones, 576px and up) */
@media (min-width: 576px) and (max-width: 767.98px) {
    .container {
        padding-left: 1.5rem;
        padding-right: 1.5rem;
    }
}

/* Medium devices (tablets, 768px and up) */
@media (min-width: 768px) and (max-width: 991.98px) {
    .container {
        padding-left: 2rem;
        padding-right: 2rem;
    }
}

/* Large devices (desktops, 992px and up) */
@media (min-width: 992px) and (max-width: 1199.98px) {
    .container {
        padding-left: 2.5rem;
        padding-right: 2.5rem;
    }
}

/* Extra large devices (large desktops, 1200px and up) */
@media (min-width: 1200px) {
    .container {
        padding-left: 3rem;
        padding-right: 3rem;
    }
}
```

### **2. Grid System:**
```css
/* Responsive Grid */
.row {
    margin-left: calc(-1 * var(--spacing-md));
    margin-right: calc(-1 * var(--spacing-md));
}

.col, [class*="col-"] {
    padding-left: var(--spacing-md);
    padding-right: var(--spacing-md);
}

/* Responsive Spacing */
@media (max-width: 767.98px) {
    .row {
        margin-left: calc(-1 * var(--spacing-sm));
        margin-right: calc(-1 * var(--spacing-sm));
    }
    
    .col, [class*="col-"] {
        padding-left: var(--spacing-sm);
        padding-right: var(--spacing-sm);
    }
}
```

---

## 🎨 **Animation Standards:**

### **1. Transitions:**
```css
/* Standard Transitions */
.transition-fast {
    transition: all 0.15s ease;
}

.transition-normal {
    transition: all 0.3s ease;
}

.transition-slow {
    transition: all 0.5s ease;
}

/* Hover Effects */
.hover-lift {
    transition: all 0.3s ease;
}

.hover-lift:hover {
    transform: translateY(-5px);
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
}

.hover-scale {
    transition: all 0.3s ease;
}

.hover-scale:hover {
    transform: scale(1.05);
}

.hover-glow {
    transition: all 0.3s ease;
}

.hover-glow:hover {
    box-shadow: 0 0 20px rgba(33, 150, 243, 0.5);
}
```

### **2. Loading Animations:**
```css
/* Spinner */
.spinner {
    width: 40px;
    height: 40px;
    border: 4px solid var(--neutral-200);
    border-top: 4px solid var(--primary-500);
    border-radius: 50%;
    animation: spin 1s linear infinite;
}

@keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
}

/* Pulse */
.pulse {
    width: 40px;
    height: 40px;
    background-color: var(--primary-500);
    border-radius: 50%;
    animation: pulse 1.5s ease-in-out infinite;
}

@keyframes pulse {
    0% { transform: scale(1); opacity: 1; }
    50% { transform: scale(1.1); opacity: 0.7; }
    100% { transform: scale(1); opacity: 1; }
}
```

---

## 🎯 **Accessibility Standards:**

### **1. Focus States:**
```css
/* Focus Indicators */
.btn:focus,
.form-control:focus,
.form-select:focus {
    outline: 2px solid var(--primary-500);
    outline-offset: 2px;
}

/* Skip Links */
.skip-link {
    position: absolute;
    top: -40px;
    left: 6px;
    background: var(--primary-700);
    color: white;
    padding: 8px;
    text-decoration: none;
    border-radius: 4px;
    z-index: 1000;
}

.skip-link:focus {
    top: 6px;
}
```

### **2. Color Contrast:**
```css
/* High Contrast Text */
.text-high-contrast {
    color: var(--neutral-900);
    background-color: white;
}

/* Low Contrast Text (for secondary information) */
.text-low-contrast {
    color: var(--neutral-600);
}
```

---

## 📊 **Data Visualization Standards:**

### **1. Charts:**
```css
/* Chart Container */
.chart-container {
    background: white;
    border-radius: 15px;
    padding: 1.5rem;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
    margin-bottom: var(--spacing-lg);
}

/* Chart Title */
.chart-title {
    font-size: 1.25rem;
    font-weight: 600;
    color: var(--neutral-800);
    margin-bottom: 1rem;
    text-align: center;
}

/* Chart Legend */
.chart-legend {
    display: flex;
    justify-content: center;
    flex-wrap: wrap;
    gap: 1rem;
    margin-top: 1rem;
}

.legend-item {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.875rem;
    color: var(--neutral-600);
}

.legend-color {
    width: 12px;
    height: 12px;
    border-radius: 2px;
}
```

---

## 🚀 **Performance Standards:**

### **1. CSS Optimization:**
```css
/* Use CSS Variables for repeated values */
:root {
    --border-radius: 10px;
    --box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
    --transition: all 0.3s ease;
}

/* Use efficient selectors */
.card-header { /* ✅ Good */
    border-radius: var(--border-radius);
}

div > div > div > div { /* ❌ Bad */
    border-radius: 10px;
}

/* Minimize repaints */
.transform-element {
    transform: translateZ(0); /* Hardware acceleration */
    will-change: transform;
}
```

### **2. Image Optimization:**
```css
/* Responsive Images */
.responsive-image {
    max-width: 100%;
    height: auto;
    object-fit: cover;
}

/* Lazy Loading */
.lazy-image {
    opacity: 0;
    transition: opacity 0.3s ease;
}

.lazy-image.loaded {
    opacity: 1;
}
```

---

## 📝 **Implementation Checklist:**

### **✅ قبل از پیاده‌سازی:**
- [ ] پالت رنگی استاندارد تعریف شده
- [ ] Typography scale تعریف شده
- [ ] Spacing scale تعریف شده
- [ ] Breakpoints تعریف شده

### **✅ در حین پیاده‌سازی:**
- [ ] از CSS Variables استفاده شده
- [ ] Responsive design پیاده‌سازی شده
- [ ] Accessibility standards رعایت شده
- [ ] Performance optimization انجام شده

### **✅ بعد از پیاده‌سازی:**
- [ ] Cross-browser testing انجام شده
- [ ] Mobile testing انجام شده
- [ ] Accessibility testing انجام شده
- [ ] Performance testing انجام شده

### **✅ امنیت (اجباری):**
- [ ] Anti-Forgery Token در تمام فرم‌ها اضافه شده
- [ ] ValidateAntiForgeryToken در تمام POST actions استفاده شده
- [ ] Token در AJAX requests ارسال می‌شود
- [ ] CSRF protection فعال است

---

## 📝 **تاریخ و نسخه:**

- **تاریخ ایجاد:** 2025-01-01
- **نسخه:** 1.0
- **وضعیت:** فعال
- **آخرین به‌روزرسانی:** 2025-01-01

---

## ✅ **تأیید:**

این قرارداد توسط تیم توسعه ClinicApp تأیید شده و باید در تمام پروژه‌ها رعایت شود.

**⚠️ توجه:** عدم رعایت این قرارداد منجر به مشکلات طراحی، عدم سازگاری و تجربه کاربری ضعیف خواهد شد.

---

## 🔗 **مراجع:**

- Material Design Guidelines
- Bootstrap Design System
- WCAG 2.1 Accessibility Guidelines
- Google Fonts Typography Scale
- CSS Custom Properties Best Practices
