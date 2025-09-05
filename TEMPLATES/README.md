# 📋 راهنمای استفاده از قالب‌های استاندارد
## Standard Templates Usage Guide

---

## 🎯 هدف

این پوشه شامل قالب‌های استاندارد برای پیاده‌سازی سریع و یکپارچه فرم‌های مختلف در پروژه ClinicApp است.

---

## 📁 فایل‌های موجود

### 1. `DetailsPageTemplate.cshtml`
**قالب استاندارد برای فرم‌های جزئیات**

#### نحوه استفاده:
1. **کپی کردن فایل**:
   ```bash
   cp TEMPLATES/DetailsPageTemplate.cshtml Areas/Admin/Views/YourModule/Details.cshtml
   ```

2. **تغییر Model**:
   ```csharp
   @model YourViewModel  // جایگزین YourViewModel
   ```

3. **تغییر Controller**:
   ```csharp
   @Url.Action("Edit", "YourController")  // جایگزین YourController
   ```

4. **تغییر اطلاعات**:
   ```csharp
   @Model.Id, @Model.Name, @Model.Description  // تطبیق با Model واقعی
   ```

5. **اضافه کردن CSS**:
   ```html
   <link href="~/Content/css/details-standards.css" rel="stylesheet" />
   ```

#### ویژگی‌های قالب:
- ✅ ساختار کارتی یکپارچه
- ✅ رنگ‌بندی رسمی محیط درمانی
- ✅ دسترس‌پذیری کامل (فونت 14px، کنتراست مناسب)
- ✅ Responsive Design (سازگاری با موبایل)
- ✅ عملیات سریع (فعال/غیرفعال، ویرایش، بازگشت)
- ✅ JavaScript برای AJAX
- ✅ پشتیبانی از jQuery

---

## 🎨 ساختار قالب Details

### بخش‌های اصلی:

1. **Header صفحه**:
   ```html
   <div class="details-header">
       <h1 class="details-title">عنوان صفحه</h1>
       <p class="details-subtitle">توضیح کوتاه</p>
   </div>
   ```

2. **اطلاعات اصلی** (آبی):
   ```html
   <div class="details-card border-primary">
       <div class="card-header bg-primary text-white">
           <h4 class="card-title mb-0">
               <i class="fa fa-user section-icon"></i>
               اطلاعات اصلی
           </h4>
       </div>
   </div>
   ```

3. **وضعیت** (سبز/قرمز):
   ```html
   <div class="details-card border-success">
       <div class="card-header bg-success text-white">
           <h4 class="card-title mb-0">
               <i class="fa fa-check-circle section-icon"></i>
               وضعیت
           </h4>
       </div>
   </div>
   ```

4. **اطلاعات زمان‌بندی** (آبی اطلاعات):
   ```html
   <div class="details-card border-info">
       <div class="card-header bg-info text-white">
           <h4 class="card-title mb-0">
               <i class="fa fa-calendar section-icon"></i>
               اطلاعات زمان‌بندی
           </h4>
       </div>
   </div>
   ```

5. **تاریخچه و حسابرسی** (زرد):
   ```html
   <div class="details-card border-warning">
       <div class="card-header bg-warning text-white">
           <h4 class="card-title mb-0">
               <i class="fa fa-history section-icon"></i>
               تاریخچه و حسابرسی
           </h4>
       </div>
   </div>
   ```

6. **عملیات سریع** (تیره):
   ```html
   <div class="details-card border-dark">
       <div class="card-header bg-dark text-white">
           <h5 class="card-title mb-0">
               <i class="fa fa-cogs section-icon"></i>
               عملیات سریع
           </h5>
       </div>
   </div>
   ```

---

## 🏷️ Badge ها و مقادیر

### انواع Badge:

```html
<!-- وضعیت فعال -->
<span class="badge badge-success">فعال</span>

<!-- وضعیت غیرفعال -->
<span class="badge badge-danger">غیرفعال</span>

<!-- وضعیت در انتظار -->
<span class="badge badge-warning">در انتظار</span>

<!-- اطلاعات -->
<span class="badge badge-info">اطلاعات</span>

<!-- ثانویه -->
<span class="badge badge-secondary">ثانویه</span>
```

### مقادیر متنی:

```html
<!-- مقدار عادی -->
<span class="info-value">مقدار عادی</span>

<!-- مقدار کم‌رنگ -->
<span class="info-value">
    <span class="text-muted">توضیح کم‌رنگ</span>
</span>

<!-- مقدار با Badge -->
<span class="info-value">
    <span class="badge badge-primary">مقدار مهم</span>
</span>
```

---

## 📱 Responsive Design

### Breakpoints:

| اندازه | توضیح |
|--------|--------|
| < 576px | موبایل کوچک |
| 576px+ | موبایل |
| 768px+ | تبلت |
| 992px+ | دسکتاپ |
| 1200px+ | دسکتاپ بزرگ |

### تغییرات موبایل:
- ردیف‌های اطلاعات به صورت عمودی
- برچسب‌ها در بالا، مقادیر در پایین
- دکمه‌های عملیات به صورت عمودی

---

## 🚀 JavaScript Functions

### توابع موجود در قالب:

1. **activateItem(id)**:
   ```javascript
   function activateItem(id) {
       // فعال کردن مورد
   }
   ```

2. **deactivateItem(id)**:
   ```javascript
   function deactivateItem(id) {
       // غیرفعال کردن مورد
   }
   ```

3. **showAlert(type, message)**:
   ```javascript
   function showAlert(type, message) {
       // نمایش پیام موفقیت/خطا
   }
   ```

4. **ensureJQueryLoaded(callback)**:
   ```javascript
   function ensureJQueryLoaded(callback) {
       // اطمینان از بارگذاری jQuery
   }
   ```

---

## 🎨 سفارشی‌سازی

### اضافه کردن رنگ جدید:

```css
/* در فایل CSS یا <style> */
.bg-custom { background-color: #your-color !important; }
.border-custom { border-color: #your-color; }
.badge-custom { 
    background-color: #your-color !important; 
    color: #ffffff !important; 
}
```

### اضافه کردن بخش جدید:

```html
<div class="info-section">
    <div class="details-card border-custom">
        <div class="card-header bg-custom text-white">
            <h4 class="card-title mb-0">
                <i class="fa fa-your-icon section-icon"></i>
                عنوان بخش جدید
            </h4>
        </div>
        <div class="card-body">
            <div class="info-table">
                <div class="info-row border-custom">
                    <label class="info-label border-custom">برچسب:</label>
                    <span class="info-value">
                        <span class="badge badge-custom">مقدار</span>
                    </span>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## 📋 چک‌لیست پیاده‌سازی

### ✅ قبل از استفاده:

- [ ] **CSS اضافه شده**: `details-standards.css` در Layout
- [ ] **Model تغییر کرده**: YourViewModel جایگزین شده
- [ ] **Controller تغییر کرده**: YourController جایگزین شده
- [ ] **Actions تغییر کرده**: Edit, Activate, Deactivate
- [ ] **Properties تطبیق داده**: Model.Id, Model.Name, etc.
- [ ] **رنگ‌ها بررسی شده**: مطابق با پالت استاندارد
- [ ] **آیکون‌ها بررسی شده**: FontAwesome موجود
- [ ] **JavaScript تست شده**: jQuery و AJAX

### ✅ بعد از پیاده‌سازی:

- [ ] **صفحه بارگذاری می‌شود**: بدون خطا
- [ ] **رنگ‌ها صحیح**: مطابق با استاندارد
- [ ] **موبایل سازگار**: Responsive کار می‌کند
- [ ] **دکمه‌ها کار می‌کنند**: عملیات AJAX
- [ ] **پیام‌ها نمایش داده می‌شوند**: موفقیت/خطا
- [ ] **دسترس‌پذیری**: Tab Navigation
- [ ] **چاپ**: Print Styles

---

## 🔧 عیب‌یابی

### مشکلات رایج:

1. **CSS بارگذاری نمی‌شود**:
   ```html
   <!-- بررسی مسیر CSS -->
   <link href="~/Content/css/details-standards.css" rel="stylesheet" />
   ```

2. **jQuery کار نمی‌کند**:
   ```javascript
   // بررسی بارگذاری jQuery
   ensureJQueryLoaded(function() {
       // کدهای jQuery
   });
   ```

3. **رنگ‌ها نمایش داده نمی‌شوند**:
   ```css
   /* استفاده از !important */
   .bg-primary { background-color: #007bff !important; }
   ```

4. **موبایل Responsive نیست**:
   ```css
   /* بررسی Media Queries */
   @media (max-width: 768px) {
       .info-row { flex-direction: column; }
   }
   ```

---

## 📚 مراجع

### فایل‌های مرتبط:
- `CONTRACTS/DETAILS_DISPLAY_STANDARDS.md` - قرارداد کامل
- `CONTRACTS/AI_COMPLIANCE_CONTRACT.md` - قوانین 40-48
- `Content/css/details-standards.css` - CSS مشترک
- `Areas/Admin/Views/DoctorServiceCategory/Details.cshtml` - نمونه کامل

### قراردادهای مرتبط:
- `CONTRACTS/DESIGN_PRINCIPLES_CONTRACT.md`
- `CONTRACTS/FormStandards.md`

---

## 📞 پشتیبانی

### در صورت مشکل:
1. بررسی چک‌لیست پیاده‌سازی
2. مراجعه به فایل‌های نمونه
3. بررسی قراردادهای مرتبط
4. تماس با تیم توسعه

---

**تاریخ ایجاد**: جلسه فعلی  
**وضعیت**: فعال و آماده استفاده  
**دامنه**: تمامی فرم‌های جزئیات در پروژه ClinicApp  
**آخرین به‌روزرسانی**: جلسه فعلی - نسخه 1.0
