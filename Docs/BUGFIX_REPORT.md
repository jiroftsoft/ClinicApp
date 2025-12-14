# 🐛 گزارش رفع باگ‌ها

**تاریخ:** 2025-01-27  
**وضعیت:** ✅ رفع شد

---

## Bug 1: ثبت تکراری IDocumentUploadService در Unity Container

### مشکل:
`IDocumentUploadService` دو بار در Unity container ثبت شده بود:
- **خط 346:** در بخش Image Upload Service
- **خط 424:** در بخش CMS Services

### تأثیر:
- ثبت تکراری (دومی اولی را override می‌کند)
- کد غیر ضروری و گیج‌کننده
- احتمال خطا در Dependency Resolution

### راه‌حل:
حذف ثبت تکراری از خط 424 و نگه داشتن ثبت در خط 346 (بخش مناسب‌تر)

**کد قبل:**
```csharp
// خط 346
container.RegisterType<IDocumentUploadService, DocumentUploadService>(new PerRequestLifetimeManager());

// خط 424
container.RegisterType<IDocumentUploadService, DocumentUploadService>(new PerRequestLifetimeManager());
```

**کد بعد:**
```csharp
// خط 346 (نگه داشته شد)
container.RegisterType<IDocumentUploadService, DocumentUploadService>(new PerRequestLifetimeManager());

// خط 424 (حذف شد - با کامنت توضیح داده شد)
// IDocumentUploadService already registered in Image Upload Service section (line 346)
```

---

## Bug 2: حذف ناقص Thumbnail در TestimonialController

### مشکل:
هنگام حذف تصویر قدیمی در `TestimonialController.Edit`:
- فقط `DeleteImage(model.PhotoUrl)` فراخوانی می‌شد
- `thumbnailPath` پاس داده نمی‌شد
- فایل thumbnail روی دیسک باقی می‌ماند (Resource Leak)

### تأثیر:
- **Resource Leak:** فایل‌های thumbnail روی دیسک باقی می‌مانند
- **Storage Waste:** فضای دیسک هدر می‌رود
- **Inconsistency:** تصویر اصلی حذف می‌شود اما thumbnail باقی می‌ماند

### راه‌حل:
محاسبه `thumbnailPath` از `PhotoUrl` و پاس دادن به `DeleteImage`

**الگوی محاسبه:**
```
PhotoUrl: /Content/Images/testimonials/filename.jpg
ThumbnailPath: /Content/Images/testimonials/thumbnails/thumb_filename.jpg
```

**کد قبل:**
```csharp
if (isEdit && !string.IsNullOrEmpty(model.PhotoUrl))
{
    var deleteResult = _imageUploadService.DeleteImage(model.PhotoUrl);
    if (deleteResult.Success)
    {
        _logger.Information("تصویر قبلی حذف شد: {PhotoUrl}", model.PhotoUrl);
    }
}
```

**کد بعد:**
```csharp
if (isEdit && !string.IsNullOrEmpty(model.PhotoUrl))
{
    // محاسبه thumbnail path از image path
    string thumbnailPath = null;
    try
    {
        var fileName = Path.GetFileName(model.PhotoUrl);
        if (!string.IsNullOrEmpty(fileName))
        {
            var thumbnailFileName = $"thumb_{fileName}";
            var imageDirectory = Path.GetDirectoryName(model.PhotoUrl)?.Replace("\\", "/");
            if (!string.IsNullOrEmpty(imageDirectory))
            {
                thumbnailPath = $"{imageDirectory}/thumbnails/{thumbnailFileName}";
            }
        }
    }
    catch (Exception ex)
    {
        _logger.Warning(ex, "خطا در محاسبه thumbnail path برای حذف: {PhotoUrl}", model.PhotoUrl);
    }

    var deleteResult = _imageUploadService.DeleteImage(model.PhotoUrl, thumbnailPath);
    if (deleteResult.Success)
    {
        _logger.Information("تصویر قبلی و thumbnail حذف شد: {PhotoUrl}, Thumbnail: {ThumbnailPath}", 
            model.PhotoUrl, thumbnailPath ?? "N/A");
    }
    else
    {
        _logger.Warning("خطا در حذف تصویر قبلی: {Message}", deleteResult.Message);
    }
}
```

---

## Bug 3: ثبت تکراری LoadFooterAttribute در .csproj

### مشکل:
`LoadFooterAttribute.cs` دو بار در `.csproj` ثبت شده بود:
- خط 606
- خط 609

### راه‌حل:
حذف ثبت تکراری

---

## ✅ تغییرات اعمال شده

### 1. App_Start/UnityConfig.cs:
- ✅ حذف ثبت تکراری `IDocumentUploadService` از خط 424
- ✅ اضافه کردن کامنت توضیحی

### 2. Areas/Admin/Controllers/CMS/TestimonialController.cs:
- ✅ اضافه کردن `using System.IO;`
- ✅ محاسبه `thumbnailPath` از `PhotoUrl`
- ✅ پاس دادن `thumbnailPath` به `DeleteImage`
- ✅ بهبود Logging (شامل thumbnail path)
- ✅ اضافه کردن Error Handling برای محاسبه thumbnail path

### 3. ClinicApp.csproj:
- ✅ حذف ثبت تکراری `LoadFooterAttribute.cs`

---

## ⚠️ مشکلات مشابه در سایر Controller ها

بررسی نشان داد که همین مشکل در Controller های زیر هم وجود دارد:

1. **MedicalServiceInfoController.cs** (خط 547, 586):
   - `DeleteImage(model.ImageUrl)` بدون thumbnail path
   - `DeleteImage(model.ThumbnailUrl)` بدون thumbnail path

2. **MedicalEquipmentController.cs** (خط 568):
   - `DeleteImage(model.ImageUrl)` بدون thumbnail path

3. **InsuranceInfoController.cs** (خط 416, 448):
   - `DeleteImage(model.LogoUrl)` بدون thumbnail path
   - `DeleteImage(model.ThumbnailUrl)` بدون thumbnail path

**نکته:** `SliderController.cs` (خط 437) به درستی از هر دو parameter استفاده می‌کند:
```csharp
_imageUploadService.DeleteImage(model.ImageUrl, model.ThumbnailUrl);
```

---

## 📋 پیشنهادات

### 1. رفع مشکلات مشابه:
- بررسی و رفع مشکلات مشابه در سایر Controller ها
- استفاده از Helper Method برای محاسبه thumbnail path

### 2. ایجاد Helper Method:
```csharp
private string GetThumbnailPath(string imagePath, string thumbnailDirectory = "thumbnails")
{
    if (string.IsNullOrEmpty(imagePath))
        return null;
    
    var fileName = Path.GetFileName(imagePath);
    if (string.IsNullOrEmpty(fileName))
        return null;
    
    var thumbnailFileName = $"thumb_{fileName}";
    var imageDirectory = Path.GetDirectoryName(imagePath)?.Replace("\\", "/");
    
    return !string.IsNullOrEmpty(imageDirectory) 
        ? $"{imageDirectory}/{thumbnailDirectory}/{thumbnailFileName}" 
        : null;
}
```

### 3. Code Review:
- بررسی تمام استفاده‌های `DeleteImage` در پروژه
- اطمینان از پاس دادن thumbnail path

---

**تهیه شده توسط:** AI Assistant (Senior .NET Architect & Healthcare Systems Specialist)  
**تاریخ:** 2025-01-27  
**نسخه:** 1.0.0  
**وضعیت:** ✅ باگ‌ها رفع شدند
