# ⚡ راهنمای سریع استفاده از CKEditor

## 🚀 شروع سریع (3 مرحله)

### مرحله 1: اضافه کردن Script ها

در View خود، در بخش `@section Scripts`:

```csharp
@section Scripts {
    @Html.Partial("_CKEditorScript")
    @{
        ViewBag.CKEditorSelector = "#myEditor";
        ViewBag.CKEditorHeight = 400;
    }
    @Html.Partial("_CKEditorInit")
}
```

### مرحله 2: ایجاد TextArea

```csharp
@Html.TextAreaFor(m => m.Content, new { 
    @class = "form-control", 
    id = "myEditor", 
    rows = 10 
})
```

### مرحله 3: آماده است! ✅

---

## 📋 مثال کامل

```csharp
@model MyModel

@using (Html.BeginForm())
{
    <div class="form-group">
        @Html.LabelFor(m => m.Content, "محتوا:")
        @Html.TextAreaFor(m => m.Content, new { 
            @class = "form-control", 
            id = "contentEditor", 
            rows = 10 
        })
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

---

## 🎯 استفاده در ماژول‌های مختلف

### FAQ Module

```csharp
ViewBag.CKEditorSelector = "#answerEditor";
ViewBag.CKEditorHeight = 300;
```

### Blog Post Module

```csharp
ViewBag.CKEditorSelector = "#contentEditor";
ViewBag.CKEditorHeight = 500;
```

### Health Tip Module

```csharp
ViewBag.CKEditorSelector = "#descriptionEditor";
ViewBag.CKEditorHeight = 400;
```

---

## ⚙️ تنظیمات پیش‌فرض

- **زبان:** فارسی (fa)
- **جهت:** راست‌به‌چپ (RTL)
- **فونت:** Tahoma
- **ارتفاع پیش‌فرض:** 300px

---

## 📚 برای اطلاعات بیشتر

راهنمای کامل: [CKEDITOR_USAGE_GUIDE.md](./CKEDITOR_USAGE_GUIDE.md)

