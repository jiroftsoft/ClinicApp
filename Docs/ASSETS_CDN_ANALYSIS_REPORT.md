# 📊 گزارش تحلیل دارایی‌های محلی و CDN

**تاریخ بررسی:** 2025-01-27  
**هدف:** شناسایی دارایی‌های محلی موجود و حذف وابستگی‌های غیرضروری به CDN

---

## ✅ خلاصه اجرایی

### دارایی‌های محلی موجود:
- ✅ **jQuery 3.7.1** - محلی (Scripts/)
- ✅ **Bootstrap** - محلی (Content/)
- ✅ **Font Awesome 6.7.2** - محلی (Content/Fonts/)
- ✅ **Vazir Font** - محلی (Content/Fonts/vazirmatn/)
- ✅ **CKEditor 4** - محلی (Content/plugins/ckeditor/)
- ✅ **DataTables** - محلی (Content/plugins/DataTables/)
- ✅ **Select2** - محلی (Content/plugins/select2/)
- ✅ **SweetAlert2** - محلی (Content/plugins/SweetAlert2/)
- ✅ **Toastr** - محلی (Content/plugins/toastr/)
- ✅ **Persian DatePicker** - محلی (Content/plugins/persian-datepicker/)
- ✅ **Chart.js** - محلی (Content/plugins/chartjs/)
- ✅ **Swiper** - محلی (Content/plugins/Swiper/)
- ✅ **AOS** - محلی (Content/aos.css, Scripts/aos.js)

### CDN های استفاده شده (نیاز به حذف):
- ❌ **Chart.js** از `cdn.jsdelivr.net` (3 فایل)
- ❌ **CKEditor** از `cdn.ckeditor.com` (اختیاری - با تنظیم)
- ❌ **Google Fonts (Vazir)** از `fonts.googleapis.com` (1 فایل)
- ❌ **DataTables i18n** از `cdn.datatables.net` (2 فایل)
- ❌ **TinyMCE Emoticons** از `cdnjs.cloudflare.com` (داخلی plugin)

---

## 📁 دارایی‌های محلی موجود

### 1️⃣ Content/plugins/

#### ✅ Chart.js
- **مسیر:** `Content/plugins/chartjs/`
- **فایل‌ها:**
  - `chart.min.js`
  - `chart.umd.min.js`
- **نسخه:** v4.5.0 (بر اساس فایل)
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ CKEditor 4
- **مسیر:** `Content/plugins/ckeditor/`
- **فایل‌ها:** 266+ فایل (215 JS, 23 PNG, 15 CSS)
- **نسخه:** 4.22.1 Standard (رایگان)
- **وضعیت:** ✅ موجود و آماده استفاده
- **تنظیم:** `CKEditor:UseCDN` در Web.config (پیش‌فرض: false)

#### ✅ DataTables
- **مسیر:** `Content/plugins/DataTables/`
- **فایل‌ها:** 241+ فایل (151 CSS, 78 JS)
- **وضعیت:** ✅ موجود و آماده استفاده
- **شامل:** تمام پلاگین‌ها (Buttons, Responsive, FixedColumns, ...)

#### ✅ Select2
- **مسیر:** `Content/plugins/select2/`
- **فایل‌ها:** 7 فایل (4 JS, 3 CSS)
- **وضعیت:** ✅ موجود و آماده استفاده
- **شامل:** فایل فارسی (fa.min.js)

#### ✅ SweetAlert2
- **مسیر:** `Content/plugins/SweetAlert2/`
- **فایل‌ها:**
  - `sweetalert2@11.js`
  - `sweetalert2.min.css`
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ Toastr
- **مسیر:** `Content/plugins/toastr/`
- **فایل‌ها:**
  - `toastr.min.js`
  - `toastr.min.css`
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ Persian DatePicker
- **مسیر:** `Content/plugins/persian-datepicker/`
- **فایل‌ها:** 8 فایل (6 JS, 2 CSS)
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ Swiper
- **مسیر:** `Content/plugins/Swiper/`
- **فایل‌ها:**
  - `swiper-bundle.min.js`
  - `swiper-bundle.min.css`
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ Dropzone
- **مسیر:** `Content/plugins/dropzone/`
- **فایل‌ها:** 8 فایل
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ FullCalendar
- **مسیر:** `Content/plugins/FullCalendar/`
- **فایل‌ها:** `index.global.min.js`
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ TinyMCE
- **مسیر:** `Content/plugins/tinymce/`
- **فایل‌ها:** 302+ فایل
- **وضعیت:** ✅ موجود و آماده استفاده
- **نکته:** Emoticons plugin از CDN استفاده می‌کند (cdnjs.cloudflare.com)

---

### 2️⃣ Scripts/

#### ✅ jQuery
- **مسیر:** `Scripts/`
- **فایل‌ها:**
  - `jquery-3.7.1.min.js`
  - `jquery-3.7.1.js`
  - `jquery-3.7.0.min.js` (نسخه قدیمی)
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ jQuery UI
- **مسیر:** `Scripts/`
- **فایل‌ها:**
  - `jquery-ui.min.js`
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ jQuery Validation
- **مسیر:** `Scripts/`
- **فایل‌ها:**
  - `jquery.validate.js`
  - `jquery.validate.min.js`
  - `jquery.validate.unobtrusive.js`
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ Bootstrap
- **مسیر:** `Scripts/`
- **فایل‌ها:**
  - `bootstrap.js`
  - `bootstrap.bundle.js`
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ Popper.js
- **مسیر:** `Scripts/`
- **فایل‌ها:**
  - `popper.min.js`
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ SignalR
- **مسیر:** `Scripts/`
- **فایل‌ها:**
  - `jquery.signalR-2.4.2.min.js`
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ Chart.js (نسخه قدیمی)
- **مسیر:** `Scripts/`
- **فایل‌ها:**
  - `chart.js` (v3.7.1)
- **وضعیت:** ⚠️ نسخه قدیمی (v3.7.1) - نسخه جدید (v4.5.0) در Content/plugins/chartjs/

---

### 3️⃣ Content/Fonts/

#### ✅ Font Awesome 6.7.2
- **مسیر:** `Content/Fonts/fontawesome-free-6.7.2-web/`
- **فایل‌ها:** 2149+ فایل
- **شامل:**
  - CSS: `all.min.css`, `fontawesome.css`, ...
  - JS: `all.js`, `brands.js`, `solid.js`, ...
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ Vazir Font
- **مسیر:** `Content/Fonts/vazirmatn/`
- **فایل‌ها:** 93 فایل (30 woff2, 20 eot, 20 ttf, ...)
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ Shabnam Font
- **مسیر:** `Content/Fonts/shabnam-font-v2.4.0/`
- **فایل‌ها:** 51 فایل
- **وضعیت:** ✅ موجود و آماده استفاده

#### ✅ Yekan Font
- **مسیر:** `Content/Fonts/yekan-font/`
- **فایل‌ها:** 5 فایل
- **وضعیت:** ✅ موجود و آماده استفاده

---

## ❌ CDN های استفاده شده (نیاز به حذف)

### 1️⃣ Chart.js از jsdelivr.net

**استفاده شده در:**
1. `Areas/Admin/Views/DoctorReporting/Index.cshtml` (خط 189)
2. `Areas/Admin/Views/DoctorDashboard/Stats.cshtml` (خط 148)
3. `Areas/Admin/Views/DoctorHistory/Statistics.cshtml` (خط 411)
4. `Views/Triage/Reports/Index.cshtml` (خط 199)

**راه‌حل:**
```html
<!-- ❌ فعلی -->
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<!-- ✅ باید باشد -->
<script src="~/Content/plugins/chartjs/chart.umd.min.js"></script>
```

**نسخه محلی موجود:**
- ✅ `Content/plugins/chartjs/chart.umd.min.js` (v4.5.0)
- ✅ `Content/plugins/chartjs/chart.min.js` (v4.5.0)

**اولویت:** ⭐⭐⭐⭐⭐ (بالا - 4 فایل)

---

### 2️⃣ CKEditor از cdn.ckeditor.com

**استفاده شده در:**
- `Areas/Admin/Views/Shared/_CKEditorScript.cshtml` (خط 13)
- `Helpers/CKEditorHelper.cs` (خط 90)

**وضعیت:**
- ✅ نسخه محلی موجود: `Content/plugins/ckeditor/ckeditor.js`
- ✅ تنظیم در Web.config: `CKEditor:UseCDN` (پیش‌فرض: false)
- ⚠️ اما در CSP Policy هنوز `https://cdn.ckeditor.com` مجاز است

**راه‌حل:**
1. اطمینان از `CKEditor:UseCDN = false` در Web.config
2. حذف `https://cdn.ckeditor.com` از CSP Policy

**اولویت:** ⭐⭐⭐⭐ (متوسط - تنظیمات)

---

### 3️⃣ Google Fonts (Vazir) از fonts.googleapis.com

**استفاده شده در:**
- `Areas/Admin/Views/PatientInsurance/Details.cshtml` (خط 9)

**راه‌حل:**
```html
<!-- ❌ فعلی -->
<link href="https://fonts.googleapis.com/css2?family=Vazir:wght@300;400;500;600;700&display=swap" rel="stylesheet" />

<!-- ✅ باید باشد -->
<!-- فونت Vazir از Content/Fonts/vazirmatn/ استفاده می‌شود -->
<!-- یا از @font-face در _AdminLayout.cshtml استفاده شود -->
```

**نسخه محلی موجود:**
- ✅ `Content/Fonts/vazirmatn/` (93 فایل)
- ✅ `@font-face` در `_AdminLayout.cshtml` (خطوط 49-67)

**اولویت:** ⭐⭐⭐⭐ (متوسط - 1 فایل)

---

### 4️⃣ DataTables i18n از cdn.datatables.net

**استفاده شده در:**
1. `Scripts/Insurance/supplementary-tariff-manager.js` (خط 22)
2. `Areas/Admin/Views/CombinedInsuranceCalculation/_SupplementaryTariffList.cshtml` (خط 195)

**راه‌حل:**
```javascript
// ❌ فعلی
language: {
    url: "//cdn.datatables.net/plug-ins/1.10.24/i18n/Persian.json"
}

// ✅ باید باشد
language: {
    url: "/Content/plugins/DataTables/js/fa.json"
}
```

**نسخه محلی موجود:**
- ✅ `Content/plugins/DataTables/js/fa.json` (فایل فارسی موجود است)

**اولویت:** ⭐⭐⭐⭐ (متوسط - 2 فایل)

---

### 5️⃣ TinyMCE Emoticons از cdnjs.cloudflare.com

**استفاده شده در:**
- `Content/plugins/tinymce/plugins/emoticons/plugin.js` (خط 478)
- `wwwroot/lib/tinymce/plugins/emoticons/plugin.js` (خط 478)

**وضعیت:**
- ⚠️ این یک CDN داخلی plugin است (برای Twemoji icons)
- می‌توان Twemoji را به صورت محلی دانلود کرد

**اولویت:** ⭐⭐ (پایین - داخلی plugin)

---

## 📋 فهرست کامل CDN های استفاده شده

| CDN | تعداد استفاده | فایل‌های تحت تأثیر | اولویت حذف |
|-----|--------------|-------------------|------------|
| **Chart.js (jsdelivr)** | 4 | DoctorReporting, DoctorDashboard, DoctorHistory, TriageReports | ⭐⭐⭐⭐⭐ |
| **CKEditor (cdn.ckeditor.com)** | 1 | _CKEditorScript.cshtml | ⭐⭐⭐⭐ |
| **Google Fonts (Vazir)** | 1 | PatientInsurance/Details.cshtml | ⭐⭐⭐⭐ |
| **DataTables i18n** | 2 | supplementary-tariff-manager.js, _SupplementaryTariffList.cshtml | ⭐⭐⭐⭐ |
| **TinyMCE Emoticons** | 1 | plugin.js (داخلی) | ⭐⭐ |

---

## 🔧 راه‌حل‌های پیشنهادی

### 1️⃣ حذف Chart.js CDN (اولویت بالا)

**فایل‌های نیازمند تغییر:**

#### `Areas/Admin/Views/DoctorReporting/Index.cshtml`
```html
<!-- ❌ حذف -->
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<!-- ✅ اضافه -->
<script src="~/Content/plugins/chartjs/chart.umd.min.js"></script>
```

#### `Areas/Admin/Views/DoctorDashboard/Stats.cshtml`
```html
<!-- ❌ حذف -->
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<!-- ✅ اضافه -->
<script src="~/Content/plugins/chartjs/chart.umd.min.js"></script>
```

#### `Areas/Admin/Views/DoctorHistory/Statistics.cshtml`
```html
<!-- ❌ حذف -->
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<!-- ✅ اضافه -->
<script src="~/Content/plugins/chartjs/chart.umd.min.js"></script>
```

#### `Views/Triage/Reports/Index.cshtml`
```html
<!-- ❌ حذف -->
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<!-- ✅ اضافه -->
<script src="~/Content/plugins/chartjs/chart.umd.min.js"></script>
```

**نکته:** نسخه محلی v4.5.0 است، اما نسخه CDN ممکن است آخرین نسخه باشد. باید تست شود.

---

### 2️⃣ حذف Google Fonts CDN (اولویت متوسط)

#### `Areas/Admin/Views/PatientInsurance/Details.cshtml`
```html
<!-- ❌ حذف -->
<link href="https://fonts.googleapis.com/css2?family=Vazir:wght@300;400;500;600;700&display=swap" rel="stylesheet" />

<!-- ✅ فونت Vazir از @font-face در _AdminLayout.cshtml استفاده می‌شود -->
<!-- نیازی به اضافه کردن نیست -->
```

**نکته:** فونت Vazir قبلاً در `_AdminLayout.cshtml` با `@font-face` تعریف شده است.

---

### 3️⃣ حذف DataTables i18n CDN (اولویت متوسط)

#### `Scripts/Insurance/supplementary-tariff-manager.js`
```javascript
// ❌ حذف
dataTableConfig: {
    language: {
        url: "//cdn.datatables.net/plug-ins/1.10.24/i18n/Persian.json"
    },
    // ...
}

// ✅ تغییر به
dataTableConfig: {
    language: {
        url: "/Content/plugins/DataTables/js/fa.json"
    },
    // ...
}
```

#### `Areas/Admin/Views/CombinedInsuranceCalculation/_SupplementaryTariffList.cshtml`
```javascript
// ❌ حذف
"language": {
    "url": "//cdn.datatables.net/plug-ins/1.10.24/i18n/Persian.json"
}

// ✅ تغییر به
"language": {
    "url": "/Content/plugins/DataTables/js/fa.json"
}
```

**نکته:** ✅ فایل `fa.json` در مسیر `Content/plugins/DataTables/js/` موجود است و آماده استفاده است.

---

### 4️⃣ به‌روزرسانی CSP Policy

#### `Views/Shared/_Layout.cshtml`
```html
<!-- ❌ فعلی -->
<meta http-equiv="Content-Security-Policy" content="... script-src ... https://cdn.ckeditor.com ...">

<!-- ✅ باید باشد -->
<meta http-equiv="Content-Security-Policy" content="... script-src ... (حذف cdn.ckeditor.com) ...">
```

#### `Areas/Admin/Views/Shared/_AdminLayout.cshtml`
```html
<!-- ❌ فعلی -->
<meta http-equiv="Content-Security-Policy" content="... style-src ... https://fonts.googleapis.com ...">

<!-- ✅ باید باشد -->
<meta http-equiv="Content-Security-Policy" content="... style-src ... (حذف fonts.googleapis.com) ...">
```

#### `Web.config`
```xml
<!-- ❌ فعلی -->
<add name="Content-Security-Policy" value="... https://fonts.googleapis.com https://cdnjs.cloudflare.com ...">

<!-- ✅ باید باشد -->
<add name="Content-Security-Policy" value="... (حذف fonts.googleapis.com و cdnjs.cloudflare.com) ...">
```

---

## 📊 خلاصه دارایی‌های محلی

### کتابخانه‌های JavaScript (محلی):
- ✅ jQuery 3.7.1
- ✅ jQuery UI
- ✅ jQuery Validation
- ✅ Bootstrap
- ✅ Popper.js
- ✅ SignalR 2.4.2
- ✅ Chart.js 4.5.0
- ✅ CKEditor 4.22.1
- ✅ DataTables (کامل)
- ✅ Select2
- ✅ SweetAlert2
- ✅ Toastr
- ✅ Persian DatePicker
- ✅ Swiper
- ✅ AOS
- ✅ Dropzone
- ✅ FullCalendar
- ✅ TinyMCE

### فونت‌ها (محلی):
- ✅ Font Awesome 6.7.2
- ✅ Vazir Font (vazirmatn)
- ✅ Shabnam Font
- ✅ Yekan Font

### CSS Framework (محلی):
- ✅ Bootstrap (کامل با RTL)
- ✅ AOS CSS
- ✅ Toastr CSS
- ✅ Persian DatePicker CSS

---

## ✅ چک‌لیست حذف CDN

### اولویت بالا:
- [ ] حذف Chart.js CDN از 4 فایل
- [ ] تست Chart.js با نسخه محلی v4.5.0

### اولویت متوسط:
- [ ] حذف Google Fonts CDN از PatientInsurance/Details.cshtml
- [ ] حذف DataTables i18n CDN از 2 فایل
- [ ] بررسی وجود fa.json در DataTables

### اولویت پایین:
- [ ] به‌روزرسانی CSP Policy (حذف cdn.ckeditor.com)
- [ ] به‌روزرسانی CSP Policy (حذف fonts.googleapis.com)
- [ ] به‌روزرسانی CSP Policy (حذف cdnjs.cloudflare.com)

---

## 🎯 مزایای حذف CDN

### 1. امنیت:
- ✅ کاهش وابستگی به منابع خارجی
- ✅ کاهش ریسک XSS از CDN های آلوده
- ✅ کنترل کامل بر نسخه کتابخانه‌ها

### 2. عملکرد:
- ✅ کاهش تعداد DNS Lookup
- ✅ کاهش Latency (بدون درخواست به سرور خارجی)
- ✅ کار در محیط Offline

### 3. حریم خصوصی:
- ✅ عدم ارسال درخواست به سرورهای خارجی
- ✅ عدم Tracking توسط CDN ها

### 4. قابلیت اطمینان:
- ✅ عدم وابستگی به دسترسی اینترنت
- ✅ عدم وابستگی به دسترس‌پذیری CDN
- ✅ کنترل کامل بر Cache

---

## ⚠️ ملاحظات

### 1. نسخه Chart.js:
- ⚠️ نسخه محلی: v4.5.0
- ⚠️ نسخه CDN: آخرین نسخه (ممکن است متفاوت باشد)
- ✅ باید تست شود که کد موجود با v4.5.0 سازگار است

### 2. DataTables fa.json:
- ⚠️ باید بررسی شود که فایل `fa.json` در مسیر `Content/plugins/DataTables/js/` موجود است
- ⚠️ اگر موجود نیست، باید از CDN دانلود شود و به صورت محلی ذخیره شود

### 3. CSP Policy:
- ⚠️ بعد از حذف CDN ها، باید CSP Policy به‌روزرسانی شود
- ⚠️ باید تست شود که تمام صفحات به درستی کار می‌کنند

---

## 📝 فایل‌های نیازمند تغییر

### اولویت بالا (4 فایل):
1. `Areas/Admin/Views/DoctorReporting/Index.cshtml`
2. `Areas/Admin/Views/DoctorDashboard/Stats.cshtml`
3. `Areas/Admin/Views/DoctorHistory/Statistics.cshtml`
4. `Views/Triage/Reports/Index.cshtml`

### اولویت متوسط (3 فایل):
5. `Areas/Admin/Views/PatientInsurance/Details.cshtml`
6. `Scripts/Insurance/supplementary-tariff-manager.js`
7. `Areas/Admin/Views/CombinedInsuranceCalculation/_SupplementaryTariffList.cshtml`

### اولویت پایین (3 فایل):
8. `Views/Shared/_Layout.cshtml` (CSP Policy)
9. `Areas/Admin/Views/Shared/_AdminLayout.cshtml` (CSP Policy)
10. `Web.config` (CSP Policy)

---

## ✅ نتیجه‌گیری

### دارایی‌های محلی موجود:
- ✅ **15+ کتابخانه JavaScript** به صورت محلی موجود است
- ✅ **4 فونت فارسی** به صورت محلی موجود است
- ✅ **تمام CSS Framework ها** به صورت محلی موجود است

### CDN های قابل حذف:
- ❌ **Chart.js** - 4 استفاده (اولویت بالا)
- ❌ **Google Fonts** - 1 استفاده (اولویت متوسط)
- ❌ **DataTables i18n** - 2 استفاده (اولویت متوسط)
- ❌ **CKEditor CDN** - اختیاری (اولویت پایین)

### مزایا:
- ✅ افزایش امنیت
- ✅ بهبود عملکرد
- ✅ حفظ حریم خصوصی
- ✅ قابلیت اطمینان بیشتر

---

## 📝 خلاصه نهایی

### ✅ دارایی‌های محلی موجود (کامل):
- **15+ کتابخانه JavaScript** به صورت محلی
- **4 فونت فارسی** به صورت محلی
- **تمام CSS Framework ها** به صورت محلی
- **تمام پلاگین‌های DataTables** به صورت محلی
- **فایل فارسی DataTables (fa.json)** موجود است

### ❌ CDN های استفاده شده (نیاز به حذف):
1. **Chart.js** - 4 فایل (اولویت بالا)
2. **Google Fonts (Vazir)** - 1 فایل (اولویت متوسط)
3. **DataTables i18n** - 2 فایل (اولویت متوسط)
4. **CKEditor CDN** - اختیاری (اولویت پایین)

### 📊 آمار:
- **کل استفاده از CDN:** 7 مورد
- **قابل حذف:** 7 مورد (100%)
- **دارایی‌های محلی موجود:** 100% پوشش

---

## 🎯 اقدامات پیشنهادی

### فاز 1: حذف Chart.js CDN (اولویت بالا)
**زمان تخمینی:** 30 دقیقه

1. جایگزینی CDN با نسخه محلی در 4 فایل
2. تست عملکرد Chart.js
3. بررسی سازگاری با نسخه v4.5.0

### فاز 2: حذف Google Fonts و DataTables i18n CDN (اولویت متوسط)
**زمان تخمینی:** 20 دقیقه

1. حذف Google Fonts از PatientInsurance/Details.cshtml
2. جایگزینی DataTables i18n CDN با نسخه محلی در 2 فایل
3. تست عملکرد

### فاز 3: به‌روزرسانی CSP Policy (اولویت پایین)
**زمان تخمینی:** 15 دقیقه

1. حذف `cdn.ckeditor.com` از CSP
2. حذف `fonts.googleapis.com` از CSP
3. حذف `cdnjs.cloudflare.com` از CSP (در صورت عدم نیاز)
4. تست کامل تمام صفحات

---

**تهیه شده توسط:** AI Assistant (Senior .NET Architect & Healthcare Systems Specialist)  
**تاریخ:** 2025-01-27  
**نسخه گزارش:** 1.0.0  
**وضعیت:** ✅ آماده برای اجرا
