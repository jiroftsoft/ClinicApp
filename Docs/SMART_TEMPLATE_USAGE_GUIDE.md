# 📖 راهنمای استفاده از Template هوشمند

## 📌 مقدمه

سیستم Template هوشمند امکان ایجاد Template های پیشرفته با قابلیت‌های شخصی‌سازی، شرطی‌سازی و حلقه‌ها را فراهم می‌کند.

---

## 🎯 متغیرهای پیشرفته

### متغیرهای کاربری

| متغیر | توضیحات | مثال |
|-------|---------|------|
| `{{FullName}}` | نام و نام خانوادگی کامل | احمد محمدی |
| `{{FirstName}}` | نام کوچک | احمد |
| `{{LastName}}` | نام خانوادگی | محمدی |
| `{{Email}}` | ایمیل | ahmad@example.com |
| `{{PhoneNumber}}` | شماره تماس | 09123456789 |
| `{{SubscriptionDate}}` | تاریخ عضویت (yyyy/MM/dd) | 1403/09/15 |
| `{{SubscriptionDateLong}}` | تاریخ عضویت (طولانی) | یکشنبه، 15 آذر 1403 |
| `{{Category}}` | دسته‌بندی خبرنامه | مقالات، اطلاعیه‌ها |
| `{{Categories}}` | دسته‌بندی‌های خبرنامه | مقالات، اطلاعیه‌ها |
| `{{UnsubscribeUrl}}` | لینک لغو اشتراک | https://... |

### متغیرهای سیستم

| متغیر | توضیحات | مثال |
|-------|---------|------|
| `{{CurrentDate}}` | تاریخ امروز (yyyy/MM/dd) | 1403/12/12 |
| `{{CurrentDateLong}}` | تاریخ امروز (طولانی) | دوشنبه، 12 اسفند 1403 |
| `{{CurrentTime}}` | زمان فعلی (HH:mm) | 14:30 |
| `{{CurrentTimeLong}}` | زمان فعلی (HH:mm:ss) | 14:30:45 |
| `{{CurrentDateTime}}` | تاریخ و زمان کامل | 1403/12/12 14:30 |

### متغیرهای کلینیک

| متغیر | توضیحات | مثال |
|-------|---------|------|
| `{{ClinicName}}` | نام کلینیک | کلینیک شفا جیرفت |
| `{{ClinicPhone}}` | شماره تلفن کلینیک | 034-32220000 |
| `{{ClinicAddress}}` | آدرس کلینیک | جیرفت، خیابان امام خمینی |
| `{{ClinicEmail}}` | ایمیل کلینیک | info@clinicapp.com |
| `{{ClinicWebsite}}` | وب‌سایت کلینیک | https://clinicapp.com |

---

## 🔀 شرطی‌سازی (Conditional Logic)

### Syntax پایه

```
{{#if Condition}}
    محتوای شرطی
{{#else}}
    محتوای جایگزین
{{/if}}
```

### عملگرهای پشتیبانی شده

- `==` : برابر است
- `!=` : برابر نیست
- `>` : بزرگتر است
- `<` : کوچکتر است
- `>=` : بزرگتر یا مساوی
- `<=` : کوچکتر یا مساوی

### مثال‌ها

#### مثال 1: شرطی ساده
```
{{#if Category == "Articles"}}
    <h2>مقالات جدید</h2>
{{#else}}
    <h2>اطلاعیه‌ها</h2>
{{/if}}
```

#### مثال 2: شرطی با متغیر
```
{{#if PhoneNumber}}
    <p>شماره تماس: {{PhoneNumber}}</p>
{{#else}}
    <p>شماره تماس ثبت نشده است.</p>
{{/if}}
```

#### مثال 3: شرطی با مقایسه عددی
```
{{#if SubscriptionDate}}
    <p>عضو از: {{SubscriptionDate}}</p>
{{/if}}
```

---

## 🔁 حلقه‌ها (Loops)

### Syntax پایه

```
{{#for CollectionName}}
    {{ItemProperty}}
{{/for}}
```

### متغیرهای در دسترس در حلقه

- `{{Index}}` : ایندکس فعلی (شروع از 0)
- `{{Count}}` : شماره فعلی (شروع از 1)
- `{{ItemProperty}}` : ویژگی‌های هر آیتم

### مثال‌ها

#### مثال 1: حلقه ساده
```
{{#for Items}}
    <div>
        <h3>{{Title}}</h3>
        <p>{{Description}}</p>
    </div>
{{/for}}
```

#### مثال 2: حلقه با Index
```
{{#for Services}}
    <p>{{Count}}. {{ServiceName}} - {{Price}}</p>
{{/for}}
```

---

## 🎨 مثال‌های کامل

### مثال 1: Template ساده با متغیرها

```
<div style="direction: rtl; text-align: right; font-family: Vazir;">
    <h1>سلام {{FullName}}</h1>
    <p>ایمیل شما: {{Email}}</p>
    <p>تاریخ عضویت: {{SubscriptionDate}}</p>
    <p>دسته‌بندی: {{Category}}</p>
    <p>تاریخ امروز: {{CurrentDate}}</p>
    <p>نام کلینیک: {{ClinicName}}</p>
    <p>آدرس: {{ClinicAddress}}</p>
    <p>تلفن: {{ClinicPhone}}</p>
    <a href="{{UnsubscribeUrl}}">لغو اشتراک</a>
</div>
```

### مثال 2: Template با شرطی‌سازی

```
<div style="direction: rtl;">
    <h1>سلام {{FirstName}}</h1>
    
    {{#if Category == "Articles"}}
        <div class="articles-section">
            <h2>مقالات جدید برای شما</h2>
            <p>مقالات جدید در دسته‌بندی مقالات آماده است.</p>
        </div>
    {{#else}}
        <div class="announcements-section">
            <h2>اطلاعیه‌های مهم</h2>
            <p>اطلاعیه‌های جدید برای شما آماده است.</p>
        </div>
    {{/if}}
    
    {{#if PhoneNumber}}
        <p>شماره تماس شما: {{PhoneNumber}}</p>
    {{#else}}
        <p>لطفاً شماره تماس خود را در پروفایل ثبت کنید.</p>
    {{/if}}
</div>
```

### مثال 3: Template با حلقه

```
<div style="direction: rtl;">
    <h1>خدمات جدید</h1>
    
    {{#for NewServices}}
        <div class="service-item">
            <h3>{{Count}}. {{ServiceName}}</h3>
            <p>{{Description}}</p>
            <p>قیمت: {{Price}} تومان</p>
        </div>
    {{/for}}
</div>
```

### مثال 4: Template ترکیبی (شرطی + حلقه)

```
<div style="direction: rtl;">
    <h1>سلام {{FullName}}</h1>
    
    {{#if HasNewArticles}}
        <h2>مقالات جدید</h2>
        {{#for Articles}}
            <div>
                <h3>{{Title}}</h3>
                <p>{{Summary}}</p>
                <a href="{{Link}}">مطالعه بیشتر</a>
            </div>
        {{/for}}
    {{#else}}
        <p>مقاله جدیدی وجود ندارد.</p>
    {{/if}}
</div>
```

---

## 💡 نکات مهم

### 1. Case-Insensitive
تمام متغیرها و دستورات case-insensitive هستند:
- `{{FullName}}` = `{{fullname}}` = `{{FULLNAME}}`

### 2. Whitespace
فضاهای خالی در دستورات نادیده گرفته می‌شوند:
- `{{#if Category == "Articles"}}` = `{{#if Category=="Articles"}}`

### 3. Nested Conditions
می‌توانید شرطی‌های تو در تو داشته باشید:
```
{{#if Category == "Articles"}}
    {{#if HasNewContent}}
        محتوای جدید
    {{/if}}
{{/if}}
```

### 4. Empty Collections
اگر Collection خالی باشد، حلقه اجرا نمی‌شود و هیچ خروجی تولید نمی‌کند.

### 5. Missing Variables
اگر متغیری وجود نداشته باشد، مقدار خالی (`""`) جایگزین می‌شود.

---

## 🚀 استفاده در Campaign

هنگام ایجاد Campaign، می‌توانید از Template های هوشمند استفاده کنید. تمام متغیرها به صورت خودکار با داده‌های واقعی هر مشترک جایگزین می‌شوند.

---

## 📚 مراجع

- `Docs/SMART_TEMPLATE_ANALYSIS.md` - تحلیل کامل سیستم
- `Helpers/SmartTemplateVariableHelper.cs` - Helper متغیرها
- `Helpers/SmartTemplateParser.cs` - Parser Template
- `Helpers/SmartTemplateRenderer.cs` - Renderer Template

---

**تاریخ ایجاد:** 2025-12-12  
**نسخه:** 1.0.0

