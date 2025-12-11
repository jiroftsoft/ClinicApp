# راهنمای سیستم آپلود تصویر - Production Ready

## 📋 فهرست مطالب
1. [معرفی](#معرفی)
2. [ویژگی‌های سیستم](#ویژگی‌های-سیستم)
3. [معماری و اصول طراحی](#معماری-و-اصول-طراحی)
4. [استفاده در Controller](#استفاده-در-controller)
5. [استفاده در View](#استفاده-در-view)
6. [امنیت و Validation](#امنیت-و-validation)
7. [بهترین روش‌ها](#بهترین-روش‌ها)

---

## معرفی

سیستم آپلود تصویر برای **محیط Production درمانی** طراحی شده است و شامل:

- ✅ **آپلود تصویر اصلی** به `/Content/Images/blog/...`
- ✅ **ایجاد خودکار Thumbnail** به `/Content/Images/blog/thumbnails/...`
- ✅ **Resize خودکار** برای بهینه‌سازی حجم
- ✅ **Validation کامل** برای امنیت
- ✅ **Production-Ready** با Error Handling و Logging

---

## ویژگی‌های سیستم

### 1. آپلود و پردازش خودکار
- آپلود تصویر اصلی
- ایجاد thumbnail خودکار (300x300)
- Resize تصویر اصلی در صورت نیاز (1920x1080)
- بهینه‌سازی کیفیت (90% برای JPEG)

### 2. امنیت
- ✅ **File Type Validation**: بررسی ContentType و Extension
- ✅ **File Signature Validation**: بررسی header فایل (امنیت بالا)
- ✅ **File Size Validation**: حداکثر 5 مگابایت
- ✅ **Dimension Validation**: حداقل 100x100، حداکثر 4000x4000
- ✅ **Filename Sanitization**: پاکسازی نام فایل برای جلوگیری از Path Traversal
- ✅ **Unique Filename**: استفاده از GUID برای جلوگیری از Overwrite

### 3. Performance
- Resize خودکار برای کاهش حجم
- Thumbnail generation با کیفیت بالا
- بهینه‌سازی کیفیت تصویر

### 4. Logging
- Logging تمام عملیات آپلود
- Logging خطاها برای Debugging
- Tracking کاربر و IP

---

## معماری و اصول طراحی

### SRP (Single Responsibility Principle)
- `IImageUploadService`: مسئولیت آپلود و پردازش تصویر
- `ImageUploadService`: پیاده‌سازی سرویس
- `BlogPostController`: استفاده از سرویس

### Dependency Injection
```csharp
// ثبت در UnityConfig
container.RegisterType<IImageUploadService, ImageUploadService>(new PerRequestLifetimeManager());
```

### Strongly-Typed
- استفاده از `ImageUploadResult` برای نتیجه
- استفاده از `ServiceResult<T>` برای Error Handling

---

## استفاده در Controller

### 1. Dependency Injection

```csharp
public class BlogPostController : BaseCMSController
{
    private readonly IImageUploadService _imageUploadService;

    public BlogPostController(
        IBlogPostService blogPostService,
        ICurrentUserService currentUserService,
        IImageUploadService imageUploadService)
    {
        _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
    }
}
```

### 2. پردازش آپلود

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(BlogPostCreateEditViewModel model)
{
    // پردازش آپلود تصویر
    await ProcessImageUpload(model);

    if (!ModelState.IsValid)
    {
        return View(GetViewPath("Create"), model);
    }

    // ادامه عملیات...
}

private async Task ProcessImageUpload(BlogPostCreateEditViewModel model)
{
    var imageFile = Request.Files["ImageFile"];
    
    if (imageFile != null && imageFile.ContentLength > 0)
    {
        var uploadResult = _imageUploadService.UploadImageWithThumbnail(
            imageFile,
            "~/Content/Images/blog",
            "~/Content/Images/blog/thumbnails",
            thumbnailWidth: 300,
            thumbnailHeight: 300,
            maxWidth: 1920,
            maxHeight: 1080);

        if (!uploadResult.Success)
        {
            NotificationHelper.SetError(TempData, uploadResult.Message);
            ModelState.AddModelError("ImageFile", uploadResult.Message);
            return;
        }

        model.ImageUrl = uploadResult.Data.ImageUrl;
        model.ThumbnailUrl = uploadResult.Data.ThumbnailUrl;
    }
}
```

---

## استفاده در View

### 1. فرم با enctype

```html
@using (Html.BeginForm("Create", "BlogPost", FormMethod.Post, 
    new { @class = "form-horizontal", role = "form", enctype = "multipart/form-data" }))
{
    <!-- فیلدهای فرم -->
}
```

### 2. Input File

```html
<div class="form-group">
    <label for="ImageFile">تصویر اصلی</label>
    <div class="custom-file">
        <input type="file" class="custom-file-input" id="ImageFile" name="ImageFile" 
               accept="image/jpeg,image/jpg,image/png,image/gif,image/webp">
        <label class="custom-file-label" for="ImageFile">انتخاب تصویر...</label>
    </div>
    <small class="form-text text-muted">
        فرمت‌های مجاز: JPG, PNG, GIF, WEBP | حداکثر حجم: 5 مگابایت
    </small>
    @Html.HiddenFor(m => m.ImageUrl)
    @Html.ValidationMessageFor(m => m.ImageUrl, "", new { @class = "text-danger" })
</div>
```

### 3. نمایش تصویر فعلی (در Edit)

```html
@if (!string.IsNullOrEmpty(Model.ImageUrl))
{
    <div class="mt-2">
        <img src="@Model.ImageUrl" alt="تصویر فعلی" 
             class="img-thumbnail" style="max-width: 200px; max-height: 200px;">
    </div>
}
```

---

## امنیت و Validation

### 1. File Type Validation

```csharp
// بررسی ContentType
private static readonly string[] AllowedImageTypes = { 
    "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" 
};

// بررسی Extension
private static readonly string[] AllowedExtensions = { 
    ".jpg", ".jpeg", ".png", ".gif", ".webp" 
};
```

### 2. File Signature Validation

```csharp
// بررسی header فایل برای امنیت
// JPEG: FF D8 FF
// PNG: 89 50 4E 47
// GIF: 47 49 46 38
// WEBP: RIFF...WEBP
```

### 3. File Size Validation

```csharp
private const int MaxFileSizeInMB = 5;
private const int MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
```

### 4. Dimension Validation

```csharp
private const int MaxImageWidth = 4000;
private const int MaxImageHeight = 4000;
private const int MinImageWidth = 100;
private const int MinImageHeight = 100;
```

### 5. Filename Sanitization

```csharp
// پاکسازی نام فایل
var sanitizedFileName = SanitizeFileName(Path.GetFileNameWithoutExtension(file.FileName));
var uniqueFileName = $"{sanitizedFileName}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{fileExtension}";
```

---

## بهترین روش‌ها

### ✅ DO (انجام دهید):

1. **استفاده از ImageUploadService**:
   ```csharp
   var uploadResult = _imageUploadService.UploadImageWithThumbnail(...);
   ```

2. **بررسی نتیجه**:
   ```csharp
   if (!uploadResult.Success)
   {
       NotificationHelper.SetError(TempData, uploadResult.Message);
       ModelState.AddModelError("ImageFile", uploadResult.Message);
       return;
   }
   ```

3. **استفاده از enctype**:
   ```html
   enctype = "multipart/form-data"
   ```

4. **نمایش پیام‌های خطا**:
   ```csharp
   NotificationHelper.SetError(TempData, "خطا در آپلود تصویر");
   ```

### ❌ DON'T (انجام ندهید):

1. **آپلود مستقیم بدون Validation**:
   ```csharp
   // ❌ اشتباه
   file.SaveAs(path);
   
   // ✅ درست
   var result = _imageUploadService.UploadImageWithThumbnail(...);
   ```

2. **استفاده از نام فایل اصلی**:
   ```csharp
   // ❌ اشتباه - خطرناک
   var fileName = file.FileName;
   
   // ✅ درست - امن
   var fileName = $"{sanitized}_{Guid.NewGuid()}{extension}";
   ```

3. **عدم بررسی File Signature**:
   ```csharp
   // ❌ اشتباه - فقط بررسی Extension
   if (extension == ".jpg") { ... }
   
   // ✅ درست - بررسی Signature
   if (IsValidImageFile(file)) { ... }
   ```

---

## مثال کامل

### Controller:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(BlogPostCreateEditViewModel model)
{
    try
    {
        // پردازش آپلود تصویر
        await ProcessImageUpload(model);

        if (!ModelState.IsValid)
        {
            return View(GetViewPath("Create"), model);
        }

        var result = await _blogPostService.CreateBlogPostAsync(model);

        if (!result.Success)
        {
            NotificationHelper.SetError(TempData, result.Message);
            return View(GetViewPath("Create"), model);
        }

        NotificationHelper.SetSuccess(TempData, "مقاله با موفقیت ایجاد شد");
        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در ایجاد مقاله");
        NotificationHelper.SetError(TempData, "خطا در ایجاد مقاله");
        return View(GetViewPath("Create"), model);
    }
}

private async Task ProcessImageUpload(BlogPostCreateEditViewModel model)
{
    var imageFile = Request.Files["ImageFile"];

    if (imageFile != null && imageFile.ContentLength > 0)
    {
        var uploadResult = _imageUploadService.UploadImageWithThumbnail(
            imageFile,
            "~/Content/Images/blog",
            "~/Content/Images/blog/thumbnails",
            thumbnailWidth: 300,
            thumbnailHeight: 300,
            maxWidth: 1920,
            maxHeight: 1080);

        if (!uploadResult.Success)
        {
            NotificationHelper.SetError(TempData, uploadResult.Message);
            ModelState.AddModelError("ImageFile", uploadResult.Message);
            return;
        }

        model.ImageUrl = uploadResult.Data.ImageUrl;
        model.ThumbnailUrl = uploadResult.Data.ThumbnailUrl;
    }
}
```

### View:

```html
@using (Html.BeginForm("Create", "BlogPost", FormMethod.Post, 
    new { @class = "form-horizontal", role = "form", enctype = "multipart/form-data" }))
{
    @Html.AntiForgeryToken()
    
    <div class="form-group">
        <label for="ImageFile">تصویر اصلی</label>
        <div class="custom-file">
            <input type="file" class="custom-file-input" id="ImageFile" name="ImageFile" 
                   accept="image/jpeg,image/jpg,image/png,image/gif,image/webp">
            <label class="custom-file-label" for="ImageFile">انتخاب تصویر...</label>
        </div>
        <small class="form-text text-muted">
            فرمت‌های مجاز: JPG, PNG, GIF, WEBP | حداکثر حجم: 5 مگابایت
        </small>
        @Html.HiddenFor(m => m.ImageUrl)
        @Html.ValidationMessageFor(m => m.ImageUrl, "", new { @class = "text-danger" })
    </div>
    
    <button type="submit" class="btn btn-primary">ذخیره</button>
}
```

---

## خلاصه

- ✅ استفاده از `IImageUploadService` برای آپلود
- ✅ Validation کامل (Type, Size, Signature, Dimension)
- ✅ ایجاد خودکار Thumbnail
- ✅ Resize خودکار برای بهینه‌سازی
- ✅ امنیت بالا (Filename Sanitization, Unique Filename)
- ✅ Production-Ready (Error Handling, Logging)
- ✅ Strongly-Typed (ViewModels, ServiceResult)
- ✅ SRP (Single Responsibility Principle)

---

**تاریخ ایجاد**: 2024  
**نسخه**: 1.0  
**نویسنده**: ClinicApp Development Team

