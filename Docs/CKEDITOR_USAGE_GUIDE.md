# 📘 راهنمای جامع استفاده از CKEditor در پروژه کلینیک شفا

**نسخه:** 1.0  
**تاریخ:** 1404/09/12  
**نویسنده:** تیم توسعه کلینیک شفا

---

## 📋 فهرست مطالب

1. [معرفی](#معرفی)
2. [نصب و راه‌اندازی](#نصب-و-راه‌اندازی)
3. [استفاده پایه](#استفاده-پایه)
4. [استفاده پیشرفته](#استفاده-پیشرفته)
5. [بهینه‌سازی برای فارسی](#بهینه‌سازی-برای-فارسی)
6. [مثال‌های عملی](#مثال‌های-عملی)
7. [عیب‌یابی](#عیب‌یابی)
8. [بهترین روش‌ها](#بهترین-روش‌ها)

---

## 🎯 معرفی

CKEditor 4.22.1 Standard یک ویرایشگر WYSIWYG متن‌باز و رایگان است که برای استفاده در پروژه‌های فارسی و راست‌به‌چپ (RTL) بهینه‌سازی شده است.

### ویژگی‌های کلیدی:
- ✅ **رایگان و متن‌باز** - بدون نیاز به license
- ✅ **پشتیبانی کامل فارسی** - زبان و جهت راست‌به‌چپ
- ✅ **بهینه‌سازی برای محیط درمانی** - مناسب برای محتوای پزشکی
- ✅ **سازگار با ASP.NET MVC5** - یکپارچه با پروژه

---

## 🚀 نصب و راه‌اندازی

### 1. بررسی نسخه CKEditor

نسخه CKEditor باید **4.22.1 Standard** باشد (رایگان). نسخه‌های 4.23.0+ (LTS) نیاز به license دارند.

**مسیر نصب:** `Content/plugins/ckeditor/`

### 2. تنظیمات Web.config

```xml
<add key="CKEditor:UseCDN" value="false" />
```

- `false`: استفاده از نسخه محلی (پیش‌فرض)
- `true`: استفاده از CDN (در صورت مشکل با نسخه محلی)

---

## 📝 استفاده پایه

### روش 1: استفاده ساده (توصیه می‌شود)

در View خود، در بخش `@section Scripts`:

```csharp
@section Scripts {
    @Html.Partial("_CKEditorScript")
    @{
        ViewBag.CKEditorSelector = "#myEditor";  // ID textarea
        ViewBag.CKEditorHeight = 400;            // ارتفاع (پیش‌فرض: 300)
    }
    @Html.Partial("_CKEditorInit")
}
```

**مثال کامل:**

```html
@model MyModel

@using (Html.BeginForm())
{
    <div class="form-group">
        @Html.LabelFor(m => m.Content, "محتوا:")
        @Html.TextAreaFor(m => m.Content, new { @class = "form-control", id = "contentEditor", rows = 10 })
        @Html.ValidationMessageFor(m => m.Content)
    </div>
    
    <button type="submit" class="btn btn-primary">ذخیره</button>
}

@section Scripts {
    @Html.Partial("_CKEditorScript")
    @{
        ViewBag.CKEditorSelector = "#contentEditor";
        ViewBag.CKEditorHeight = 400;
    }
    @Html.Partial("_CKEditorInit")
}
```

### روش 2: استفاده مستقیم (پیشرفته)

اگر نیاز به تنظیمات خاص دارید:

```html
<script>
    CKEDITOR.replace('myEditor', {
        language: 'fa',
        contentsLangDirection: 'rtl',
        height: 400,
        // تنظیمات دیگر...
    });
</script>
```

---

## 🎨 استفاده پیشرفته

### تنظیمات سفارشی Toolbar

برای تغییر toolbar، در `_CKEditorInit.cshtml` یا در View خود:

```javascript
var customConfig = {
    // ... تنظیمات پایه
    toolbar: [
        { name: 'clipboard', items: [ 'Cut', 'Copy', 'Paste', '-', 'Undo', 'Redo' ] },
        { name: 'basicstyles', items: [ 'Bold', 'Italic', 'Underline' ] },
        // ... سایر گروه‌ها
    ]
};
```

### مدیریت رویدادها

```javascript
editor.on('instanceReady', function() {
    console.log('Editor ready!');
});

editor.on('change', function() {
    // انجام عملیات هنگام تغییر محتوا
});

editor.on('blur', function() {
    // انجام عملیات هنگام از دست دادن فوکوس
});
```

### دریافت و تنظیم محتوا

```javascript
// دریافت محتوا
var content = editor.getData();

// تنظیم محتوا
editor.setData('<p>متن جدید</p>');

// بررسی تغییرات
if (editor.checkDirty()) {
    // محتوا تغییر کرده است
}
```

---

## 🇮🇷 بهینه‌سازی برای فارسی

### تنظیمات خودکار

تمام تنظیمات فارسی به صورت خودکار اعمال می‌شوند:

- ✅ زبان: فارسی (`language: 'fa'`)
- ✅ جهت: راست‌به‌چپ (`contentsLangDirection: 'rtl'`)
- ✅ فونت: Tahoma (پیش‌فرض)
- ✅ استایل‌های CSS برای RTL

### فونت‌های پیشنهادی

```javascript
font_names: 'Tahoma;Arial;Verdana;Times New Roman;Courier New'
```

### تنظیمات CSS سفارشی

برای اضافه کردن CSS سفارشی:

```javascript
contentsCss: [
    'body { direction: rtl; font-family: Tahoma; }',
    'p { margin: 1em 0; }',
    // CSS های دیگر...
]
```

---

## 💡 مثال‌های عملی

### مثال 1: فرم FAQ

```csharp
@model FAQ

@using (Html.BeginForm())
{
    <div class="form-group">
        @Html.LabelFor(m => m.Answer, "پاسخ:")
        @Html.TextAreaFor(m => m.Answer, new { 
            @class = "form-control", 
            id = "answerEditor", 
            rows = 10 
        })
    </div>
    
    <button type="submit" class="btn btn-success">ذخیره</button>
}

@section Scripts {
    @Html.Partial("_CKEditorScript")
    @{
        ViewBag.CKEditorSelector = "#answerEditor";
        ViewBag.CKEditorHeight = 300;
    }
    @Html.Partial("_CKEditorInit")
}
```

### مثال 2: فرم Blog Post

```csharp
@model BlogPost

@using (Html.BeginForm())
{
    <div class="form-group">
        @Html.LabelFor(m => m.Content, "محتوا:")
        @Html.TextAreaFor(m => m.Content, new { 
            @class = "form-control", 
            id = "contentEditor", 
            rows = 15 
        })
    </div>
    
    <button type="submit" class="btn btn-primary">انتشار</button>
}

@section Scripts {
    @Html.Partial("_CKEditorScript")
    @{
        ViewBag.CKEditorSelector = "#contentEditor";
        ViewBag.CKEditorHeight = 500;
    }
    @Html.Partial("_CKEditorInit")
}
```

### مثال 3: چند Editor در یک صفحه

```csharp
@model MyModel

@using (Html.BeginForm())
{
    <div class="form-group">
        @Html.Label("توضیحات کوتاه:")
        @Html.TextArea("ShortDescription", Model.ShortDescription, new { 
            id = "shortEditor", 
            rows = 5 
        })
    </div>
    
    <div class="form-group">
        @Html.Label("توضیحات کامل:")
        @Html.TextArea("FullDescription", Model.FullDescription, new { 
            id = "fullEditor", 
            rows = 15 
        })
    </div>
    
    <button type="submit" class="btn btn-primary">ذخیره</button>
}

@section Scripts {
    @Html.Partial("_CKEditorScript")
    
    @* Editor اول *@
    @{
        ViewBag.CKEditorSelector = "#shortEditor";
        ViewBag.CKEditorHeight = 200;
    }
    @Html.Partial("_CKEditorInit")
    
    @* Editor دوم *@
    <script>
        setTimeout(function() {
            if (typeof CKEDITOR !== 'undefined') {
                CKEDITOR.replace('fullEditor', {
                    language: 'fa',
                    contentsLangDirection: 'rtl',
                    height: 400
                });
            }
        }, 500);
    </script>
}
```

---

## 🔧 عیب‌یابی

### مشکل 1: Editor لود نمی‌شود

**علت:** CKEditor script بارگذاری نشده است.

**راه‌حل:**
```html
<!-- مطمئن شوید که این خط قبل از _CKEditorInit است -->
@Html.Partial("_CKEditorScript")
```

### مشکل 2: متن فارسی به درستی نمایش داده نمی‌شود

**علت:** تنظیمات RTL اعمال نشده است.

**راه‌حل:** بررسی کنید که `language: 'fa'` و `contentsLangDirection: 'rtl'` تنظیم شده باشند.

### مشکل 3: خطای License

**علت:** استفاده از نسخه LTS (4.23.0+)

**راه‌حل:** از نسخه 4.22.1 Standard استفاده کنید.

### مشکل 4: محتوا ذخیره نمی‌شود

**علت:** محتوا قبل از submit به textarea منتقل نشده است.

**راه‌حل:** در فرم، قبل از submit:

```javascript
$('form').on('submit', function() {
    for (var instance in CKEDITOR.instances) {
        CKEDITOR.instances[instance].updateElement();
    }
});
```

---

## ✅ بهترین روش‌ها

### 1. نام‌گذاری ID ها

از نام‌های واضح و یکتا استفاده کنید:

```csharp
// ✅ خوب
id = "answerEditor"
id = "contentEditor"
id = "descriptionEditor"

// ❌ بد
id = "editor1"
id = "text1"
```

### 2. ارتفاع مناسب

بر اساس نوع محتوا ارتفاع مناسب انتخاب کنید:

- **متن کوتاه:** 200-300px
- **متن متوسط:** 300-400px
- **متن طولانی:** 400-500px

### 3. اعتبارسنجی

همیشه در Controller اعتبارسنجی انجام دهید:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Create(MyModel model)
{
    if (ModelState.IsValid)
    {
        // Sanitize HTML content
        model.Content = SanitizeHtml(model.Content);
        
        // Save to database
        _repository.Save(model);
        
        return RedirectToAction("Index");
    }
    
    return View(model);
}
```

### 4. Sanitize کردن HTML

برای امنیت، HTML را sanitize کنید:

```csharp
using System.Web;
using HtmlAgilityPack; // یا کتابخانه دیگر

private string SanitizeHtml(string html)
{
    // استفاده از کتابخانه HTML Sanitizer
    // یا HtmlAgilityPack برای حذف تگ‌های خطرناک
    return html; // پیاده‌سازی بر اساس نیاز
}
```

### 5. مدیریت خطاها

همیشه خطاها را مدیریت کنید:

```javascript
editor.on('error', function(evt) {
    console.error('CKEditor Error:', evt);
    // نمایش پیام خطا به کاربر
    alert('خطا در بارگذاری ویرایشگر. لطفاً صفحه را رفرش کنید.');
});
```

---

## 📚 منابع بیشتر

- [مستندات رسمی CKEditor 4](https://ckeditor.com/docs/ckeditor4/latest/)
- [API Reference](https://ckeditor.com/docs/ckeditor4/latest/api/index.html)
- [مثال‌های CKEditor](https://ckeditor.com/docs/ckeditor4/latest/examples/index.html)

---

## 🆘 پشتیبانی

در صورت بروز مشکل:

1. بررسی Console مرورگر برای خطاها
2. بررسی Network tab برای فایل‌های بارگذاری نشده
3. بررسی تنظیمات Web.config
4. تماس با تیم توسعه

---

**آخرین به‌روزرسانی:** 1404/09/12  
**نسخه CKEditor:** 4.22.1 Standard

