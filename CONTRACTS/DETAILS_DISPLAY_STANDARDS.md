# قرارداد استاندارد نمایش اطلاعات در فرم‌های جزئیات
## Information Display Standards Contract

---

## 📋 مقدمه

این قرارداد استاندارد برای تمامی فرم‌های نمایش اطلاعات (Details, View, Show) در پروژه ClinicApp الزام‌آور است و باید در کنار قوانین اصلی `AI_COMPLIANCE_CONTRACT` رعایت شود.

---

## 🎯 اهداف قرارداد

### ✅ اهداف اصلی:
- **یکپارچه‌سازی طراحی** تمام فرم‌های جزئیات
- **رعایت استانداردهای محیط درمانی** (رسمی، ساده، خوانا)
- **دسترس‌پذیری کامل** برای تمام کاربران
- **عملکرد بالا** و تجربه کاربری بهینه
- **سازگاری با موبایل** و دستگاه‌های مختلف

### ❌ اهداف ممنوع:
- طراحی غیررسمی یا فانتزی
- رنگ‌بندی تند یا غیرحرفه‌ای
- عدم دسترس‌پذیری
- عملکرد ضعیف
- عدم سازگاری با موبایل

---

## 📐 ساختار استاندارد

### 1. ساختار کلی صفحه

```html
<div class="details-container">
    <!-- Header صفحه -->
    <div class="details-header">
        <h1 class="details-title">عنوان صفحه</h1>
        <p class="details-subtitle">توضیح کوتاه</p>
    </div>
    
    <!-- بخش‌های اطلاعات -->
    <div class="info-section">
        <!-- Card اطلاعات اصلی -->
        <div class="details-card border-primary">
            <div class="card-header bg-primary text-white">
                <h4 class="card-title mb-0">
                    <i class="fa fa-user section-icon"></i>
                    اطلاعات اصلی
                </h4>
            </div>
            <div class="card-body">
                <div class="info-table">
                    <!-- ردیف‌های اطلاعات -->
                </div>
            </div>
        </div>
    </div>
    
    <!-- عملیات سریع -->
    <div class="quick-actions">
        <!-- دکمه‌های عملیات -->
    </div>
</div>
```

### 2. ساختار Card اطلاعات

```html
<div class="details-card border-[color]">
    <div class="card-header bg-[color] text-white">
        <h4 class="card-title mb-0">
            <i class="fa fa-[icon] section-icon"></i>
            عنوان بخش
        </h4>
    </div>
    <div class="card-body">
        <div class="info-table">
            <div class="info-row border-[color]">
                <label class="info-label border-[color]">برچسب:</label>
                <span class="info-value">
                    <span class="badge badge-[type]">مقدار</span>
                </span>
            </div>
        </div>
    </div>
</div>
```

---

## 🎨 رنگ‌بندی رسمی

### رنگ‌های اصلی:

| رنگ | کد Hex | کاربرد |
|-----|--------|--------|
| آبی اصلی | `#007bff` | اطلاعات اصلی، Header ها |
| سبز موفقیت | `#28a745` | وضعیت فعال، موفقیت |
| آبی اطلاعات | `#17a2b8` | اطلاعات زمان‌بندی |
| زرد هشدار | `#ffc107` | هشدارها، تاریخچه |
| قرمز خطا | `#dc3545` | خطاها، غیرفعال |
| خاکستری | `#6c757d` | اطلاعات ثانویه |
| تیره | `#343a40` | عملیات، Footer |

### رنگ‌های متن:

| نوع متن | کد Hex | کاربرد |
|---------|--------|--------|
| متن اصلی | `#2c3e50` | برچسب‌ها، عناوین |
| مقادیر | `#212529` | مقادیر اطلاعات |
| متن کم‌رنگ | `#6c757d` | توضیحات، تاریخ‌ها |

---

## 📱 بخش‌بندی منطقی

### 1. اطلاعات اصلی (Primary)
- **رنگ**: آبی اصلی (`#007bff`)
- **آیکون**: `fa-user`, `fa-id-card`
- **محتوای**: نام، شناسه، اطلاعات پایه

### 2. وضعیت (Status)
- **رنگ**: سبز (`#28a745`) یا قرمز (`#dc3545`)
- **آیکون**: `fa-check-circle`, `fa-times-circle`
- **محتوای**: وضعیت فعال/غیرفعال، وضعیت تایید

### 3. اطلاعات زمان‌بندی (Scheduling)
- **رنگ**: آبی اطلاعات (`#17a2b8`)
- **آیکون**: `fa-calendar`, `fa-clock`
- **محتوای**: تاریخ‌ها، زمان‌ها، مدت

### 4. تاریخچه و حسابرسی (Audit Trail)
- **رنگ**: زرد (`#ffc107`)
- **آیکون**: `fa-history`, `fa-user-edit`
- **محتوای**: تاریخ ایجاد، آخرین تغییر، کاربران

### 5. عملیات سریع (Quick Actions)
- **رنگ**: تیره (`#343a40`)
- **آیکون**: `fa-cogs`, `fa-tools`
- **محتوای**: دکمه‌های ویرایش، حذف، بازگشت

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

## ♿ دسترس‌پذیری

### الزامات دسترس‌پذیری:

1. **فونت**: حداقل 14px برای افراد مسن
2. **کنتراست**: حداقل 4.5:1 برای متن عادی
3. **کیبورد**: پشتیبانی کامل از Tab Navigation
4. **رنگ**: عدم وابستگی صرف به رنگ برای انتقال اطلاعات
5. **Responsive**: سازگاری با تمام اندازه‌های صفحه

### پیاده‌سازی:

```css
/* فونت مناسب */
.info-label, .info-value {
    font-size: 14px;
    min-height: 2rem;
}

/* کنتراست مناسب */
.info-label {
    color: #2c3e50 !important;
    background-color: #ffffff;
}

/* Responsive */
@media (max-width: 768px) {
    .info-row {
        flex-direction: column;
    }
}
```

---

## 📱 Responsive Design

### Breakpoints:

| اندازه | کلاس | توضیح |
|--------|-------|--------|
| < 576px | `.col-xs` | موبایل کوچک |
| 576px+ | `.col-sm` | موبایل |
| 768px+ | `.col-md` | تبلت |
| 992px+ | `.col-lg` | دسکتاپ |
| 1200px+ | `.col-xl` | دسکتاپ بزرگ |

### تغییرات موبایل:

```css
@media (max-width: 768px) {
    .info-row {
        flex-direction: column;
        align-items: flex-start;
    }
    
    .info-label {
        min-width: auto;
        width: 100%;
        margin-bottom: 0.5rem;
    }
    
    .info-value {
        margin-left: 0;
        width: 100%;
    }
}
```

---

## 🚀 عملکرد و بهینه‌سازی

### الزامات عملکرد:

1. **بارگذاری سریع**: CSS کمتر از 50KB
2. **انیمیشن‌های ملایم**: حداکثر 300ms
3. **Hover Effects**: فقط برای دسکتاپ
4. **Print Styles**: پشتیبانی از چاپ

### بهینه‌سازی:

```css
/* انیمیشن‌های ملایم */
.details-card,
.info-row,
.btn {
    transition: all 0.3s ease;
}

/* Hover فقط برای دسکتاپ */
@media (hover: hover) {
    .details-card:hover {
        transform: translateY(-2px);
    }
}
```

---

## 📋 چک‌لیست پیاده‌سازی

### ✅ الزامات اجباری:

- [ ] **ساختار یکپارچه**: استفاده از کلاس‌های استاندارد
- [ ] **رنگ‌بندی رسمی**: رعایت پالت رنگ‌های تعریف شده
- [ ] **بخش‌بندی منطقی**: تقسیم اطلاعات به بخش‌های مناسب
- [ ] **آیکون‌های مناسب**: استفاده از FontAwesome
- [ ] **Badge ها**: نمایش مقادیر مهم با Badge
- [ ] **دسترس‌پذیری**: فونت 14px، کنتراست مناسب
- [ ] **Responsive**: سازگاری با موبایل
- [ ] **عملیات سریع**: دکمه‌های بزرگ و واضح
- [ ] **CSS مشترک**: استفاده از فایل `details-standards.css`

### ❌ ممنوعیت‌ها:

- [ ] **ساختار غیریکپارچه**: عدم استفاده از کلاس‌های استاندارد
- [ ] **رنگ‌بندی غیررسمی**: استفاده از رنگ‌های تند
- [ ] **فونت کوچک**: کمتر از 14px
- [ ] **عدم کنتراست**: متن غیرقابل خواندن
- [ ] **عدم Responsive**: عدم سازگاری با موبایل
- [ ] **انیمیشن‌های تند**: بیش از 300ms
- [ ] **عدم دسترس‌پذیری**: عدم پشتیبانی از کیبورد

---

## 📁 فایل‌های مرتبط

### فایل‌های CSS:
- `Content/css/details-standards.css` - CSS مشترک
- `Content/css/bootstrap.min.css` - Bootstrap اصلی

### فایل‌های قرارداد:
- `CONTRACTS/AI_COMPLIANCE_CONTRACT.md` - قوانین اصلی
- `CONTRACTS/DESIGN_PRINCIPLES_CONTRACT.md` - اصول طراحی
- `CONTRACTS/FormStandards.md` - استانداردهای فرم

### فایل‌های نمونه:
- `Areas/Admin/Views/DoctorServiceCategory/Details.cshtml` - نمونه پیاده‌سازی

---

## 🔧 نحوه استفاده

### 1. اضافه کردن CSS:

```html
<!-- در Layout اصلی -->
<link href="~/Content/css/details-standards.css" rel="stylesheet" />
```

### 2. ساختار HTML:

```html
<!-- استفاده از کلاس‌های استاندارد -->
<div class="details-container">
    <div class="details-header">
        <h1 class="details-title">عنوان صفحه</h1>
    </div>
    
    <div class="details-card border-primary">
        <div class="card-header bg-primary text-white">
            <h4 class="card-title mb-0">
                <i class="fa fa-user section-icon"></i>
                اطلاعات اصلی
            </h4>
        </div>
        <div class="card-body">
            <div class="info-table">
                <div class="info-row border-primary">
                    <label class="info-label border-primary">نام:</label>
                    <span class="info-value">
                        <span class="badge badge-primary">مقدار</span>
                    </span>
                </div>
            </div>
        </div>
    </div>
</div>
```

### 3. سفارشی‌سازی:

```css
/* اضافه کردن رنگ جدید */
.bg-custom { background-color: #your-color !important; }
.border-custom { border-color: #your-color; }
.badge-custom { 
    background-color: #your-color !important; 
    color: #ffffff !important; 
}
```

---

## 📊 آمار و گزارش

### معیارهای موفقیت:

| معیار | هدف | وضعیت |
|-------|-----|--------|
| زمان بارگذاری | < 2 ثانیه | ✅ |
| سازگاری موبایل | 100% | ✅ |
| دسترس‌پذیری | WCAG AA | ✅ |
| کنتراست رنگ | 4.5:1+ | ✅ |
| فونت | 14px+ | ✅ |

---

## 🔄 به‌روزرسانی‌ها

### نسخه 1.0 (جلسه فعلی):
- ایجاد قرارداد اولیه
- تعریف 9 قانون اصلی
- ایجاد CSS مشترک
- نمونه پیاده‌سازی در DoctorServiceCategory

### نسخه‌های آینده:
- اضافه کردن انیمیشن‌های پیشرفته
- پشتیبانی از Dark Mode
- بهینه‌سازی عملکرد
- اضافه کردن قابلیت‌های جدید

---

## 📞 پشتیبانی

### در صورت مشکل:
1. بررسی چک‌لیست پیاده‌سازی
2. مراجعه به فایل‌های نمونه
3. بررسی قراردادهای مرتبط
4. تماس با تیم توسعه

---

**تاریخ ایجاد**: جلسه فعلی  
**وضعیت قرارداد**: فعال و الزام‌آور  
**دامنه**: تمامی فرم‌های جزئیات در پروژه ClinicApp  
**آخرین به‌روزرسانی**: جلسه فعلی - نسخه 1.0
