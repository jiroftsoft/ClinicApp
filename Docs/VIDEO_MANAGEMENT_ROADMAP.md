# 🎬 نقشه راه سیستم مدیریت ویدیو - حرفه‌ای و Production-Ready

## 📋 خلاصه پروژه
سیستم مدیریت ویدیو برای نمایش کلیپ‌های تصویری در صفحه اصلی سایت (مثل معرفی بخش اندوسکوپی)

---

## 🎯 اهداف و نیازمندی‌ها

### نیازمندی‌های اصلی:
1. ✅ آپلود ویدیو (مستقیم یا لینک YouTube/Vimeo)
2. ✅ مدیریت ویدیوها (CRUD کامل)
3. ✅ نمایش در صفحه اصلی
4. ✅ Thumbnail برای ویدیو
5. ✅ دسته‌بندی ویدیوها
6. ✅ فعال/غیرفعال
7. ✅ ترتیب نمایش
8. ✅ اطلاعات ویدیو (عنوان، توضیحات، لینک)

### ویژگی‌های پیشرفته:
- پشتیبانی از YouTube Embed
- پشتیبانی از Vimeo Embed
- آپلود مستقیم ویدیو (با محدودیت حجم)
- Auto-generate Thumbnail از ویدیو
- Player حرفه‌ای با کنترل‌های سفارشی
- Responsive Design
- SEO Optimization

---

## 🏗️ معماری و ساختار

### 1. Entity Layer
```
Models/Entities/CMS/Video.cs
- VideoId (PK)
- Title (NVARCHAR 500)
- Description (NVARCHAR 2000)
- VideoUrl (NVARCHAR 1000) - لینک YouTube/Vimeo یا مسیر فایل
- VideoType (Enum: YouTube, Vimeo, DirectUpload)
- ThumbnailUrl (NVARCHAR 500)
- Category (NVARCHAR 100) - مثل "endoscopy", "surgery", "general"
- Duration (INT) - مدت زمان به ثانیه
- ViewCount (INT)
- IsActive (BIT)
- DisplayOrder (INT)
- CreatedAt (DATETIME)
- CreatedByUserId (NVARCHAR)
- UpdatedAt (DATETIME?)
- UpdatedByUserId (NVARCHAR?)
```

### 2. Repository Layer
```
Interfaces/CMS/IVideoRepository.cs
- GetAllAsync()
- GetByIdAsync(int id)
- GetActiveVideosAsync()
- GetByCategoryAsync(string category)
- CreateAsync(Video entity)
- UpdateAsync(Video entity)
- DeleteAsync(int id)
- GetVideosForHomePageAsync(int count)

Repositories/CMS/VideoRepository.cs
- Implementation با Entity Framework
```

### 3. Service Layer
```
Interfaces/CMS/IVideoService.cs
- GetVideosAsync(VideoSearchViewModel search)
- GetVideoDetailsAsync(int id)
- GetVideoForEditAsync(int id)
- CreateVideoAsync(VideoCreateEditViewModel model)
- UpdateVideoAsync(VideoCreateEditViewModel model)
- DeleteVideoAsync(int id)
- ActivateVideoAsync(int id)
- DeactivateVideoAsync(int id)
- GetVideosForHomePageAsync(int count, string category = null)

Services/CMS/VideoService.cs
- Business Logic
- Validation
- Error Handling
```

### 4. ViewModel Layer
```
ViewModels/CMS/VideoViewModels.cs
- VideoIndexViewModel
- VideoCreateEditViewModel
- VideoDetailsViewModel
- VideoSearchViewModel
- VideoHomePageViewModel (برای نمایش در صفحه اصلی)
```

### 5. Controller Layer
```
Areas/Admin/Controllers/CMS/VideoController.cs
- Index (لیست ویدیوها)
- Details (جزئیات)
- Create (GET/POST)
- Edit (GET/POST)
- Delete (POST)
- Activate/Deactivate (POST)
```

### 6. View Layer (Admin)
```
Areas/Admin/Views/CMS/Video/
- Index.cshtml (لیست با فیلتر)
- Create.cshtml (فرم ایجاد)
- Edit.cshtml (فرم ویرایش)
- Details.cshtml (جزئیات)
```

### 7. Public View Layer
```
Views/Home/Sections/
- _VideoSection.cshtml (نمایش ویدیوها در صفحه اصلی)

Controllers/HomeController.cs
- VideoSection() - ChildAction برای نمایش در صفحه اصلی
```

### 8. Service Integration
```
Services/HomePageService.cs
- GetVideoSectionAsync(int count, string category = null)

ViewModels/HomePageViewModel.cs
- اضافه کردن VideoSection property
```

---

## 📝 مراحل پیاده‌سازی (Phase by Phase)

### Phase 1: Entity & Database ✅
1. ایجاد Entity `Video` با تمام فیلدها
2. اضافه کردن `DbSet<Video>` به `IdentityModels.cs`
3. ایجاد Migration
4. اجرای Migration

### Phase 2: Repository Layer ✅
1. ایجاد `IVideoRepository` interface
2. پیاده‌سازی `VideoRepository`
3. تست Repository Methods

### Phase 3: Service Layer ✅
1. ایجاد `IVideoService` interface
2. پیاده‌سازی `VideoService`
3. Business Logic و Validation
4. Error Handling

### Phase 4: ViewModels ✅
1. ایجاد `VideoViewModels.cs`
2. تعریف تمام ViewModels
3. Data Annotations و Validation

### Phase 5: Dependency Injection ✅
1. ثبت Repository در `UnityConfig.cs`
2. ثبت Service در `UnityConfig.cs`
3. تست DI

### Phase 6: Admin Controller ✅
1. ایجاد `VideoController` در Admin Area
2. پیاده‌سازی تمام Actions
3. استفاده از `GetViewPath()` برای routing
4. استفاده از `NotificationHelper` برای پیغام‌ها
5. Strongly-Typed Views

### Phase 7: Admin Views ✅
1. `Index.cshtml` - لیست با فیلتر و جستجو
2. `Create.cshtml` - فرم ایجاد با آپلود
3. `Edit.cshtml` - فرم ویرایش
4. `Details.cshtml` - نمایش جزئیات
5. UI/UX حرفه‌ای با Bootstrap
6. SweetAlert برای Delete Confirmation

### Phase 8: Video Upload System ✅
1. پشتیبانی از YouTube URL (Extract Video ID)
2. پشتیبانی از Vimeo URL (Extract Video ID)
3. آپلود مستقیم ویدیو (اختیاری - با محدودیت)
4. Auto-generate Thumbnail از YouTube/Vimeo
5. Validation و Error Handling

### Phase 9: Public Integration ✅
1. اضافه کردن `VideoSection` به `HomeController`
2. ایجاد `_VideoSection.cshtml` partial view
3. اضافه کردن به `HomePageService`
4. اضافه کردن به `HomePageViewModel`
5. اضافه کردن به `Views/Home/Index.cshtml`

### Phase 10: Video Player ✅
1. استفاده از Video.js یا Plyr.js
2. Responsive Player
3. Custom Controls
4. Auto-play (اختیاری)
5. Thumbnail Preview

### Phase 11: SEO & Performance ✅
1. Meta Tags برای ویدیو
2. Schema.org VideoObject
3. Lazy Loading
4. Caching
5. CDN Support (برای ویدیوهای مستقیم)

---

## 🔧 تکنولوژی‌ها و کتابخانه‌ها

### Backend:
- ASP.NET MVC 5
- Entity Framework 6
- Unity Container (DI)
- Serilog (Logging)

### Frontend:
- Bootstrap 4
- jQuery
- Video.js یا Plyr.js (Video Player)
- SweetAlert2 (Confirmations)
- Toastr (Notifications)

### Video Services:
- YouTube API (برای Thumbnail)
- Vimeo API (برای Thumbnail)

---

## 📊 Database Schema

```sql
CREATE TABLE [dbo].[Videos] (
    [VideoId] INT IDENTITY(1,1) PRIMARY KEY,
    [Title] NVARCHAR(500) NOT NULL,
    [Description] NVARCHAR(2000) NULL,
    [VideoUrl] NVARCHAR(1000) NOT NULL,
    [VideoType] INT NOT NULL, -- 0: YouTube, 1: Vimeo, 2: DirectUpload
    [ThumbnailUrl] NVARCHAR(500) NULL,
    [Category] NVARCHAR(100) NULL,
    [Duration] INT NULL, -- به ثانیه
    [ViewCount] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatedByUserId] NVARCHAR(128) NULL,
    [UpdatedAt] DATETIME NULL,
    [UpdatedByUserId] NVARCHAR(128) NULL
);

CREATE INDEX IX_Videos_IsActive ON [Videos]([IsActive]);
CREATE INDEX IX_Videos_Category ON [Videos]([Category]);
CREATE INDEX IX_Videos_DisplayOrder ON [Videos]([DisplayOrder]);
```

---

## 🎨 UI/UX Design

### Admin Panel:
- لیست ویدیوها با Grid Layout
- فیلتر بر اساس Category
- جستجو بر اساس Title
- نمایش Thumbnail در لیست
- دکمه‌های Action (View, Edit, Delete, Activate/Deactivate)

### Public Page:
- نمایش ویدیوها در یک Section زیبا
- Responsive Grid (2-3 ویدیو در هر ردیف)
- Modal Player برای نمایش ویدیو
- Thumbnail با Play Button Overlay
- Title و Description

---

## 🔒 Security & Validation

### Validation:
- Title: Required, Max 500 chars
- VideoUrl: Required, Valid URL format
- VideoType: Required, Valid Enum value
- Category: Max 100 chars
- File Upload: Max 100MB (برای Direct Upload)

### Security:
- Anti-Forgery Token
- Authorization (Admin Only)
- File Type Validation
- File Size Validation
- XSS Protection
- SQL Injection Protection (EF)

---

## 📈 Performance Optimization

1. **Caching**: OutputCache برای VideoSection (5 دقیقه)
2. **Lazy Loading**: ویدیوها فقط در صورت نیاز لود شوند
3. **CDN**: برای ویدیوهای مستقیم
4. **Thumbnail Optimization**: تصاویر بهینه شده
5. **Database Indexing**: Index روی IsActive, Category, DisplayOrder

---

## 🧪 Testing Checklist

### Unit Tests:
- [ ] Repository Tests
- [ ] Service Tests
- [ ] Validation Tests

### Integration Tests:
- [ ] Controller Tests
- [ ] End-to-End Tests

### Manual Tests:
- [ ] آپلود ویدیو YouTube
- [ ] آپلود ویدیو Vimeo
- [ ] آپلود مستقیم ویدیو
- [ ] نمایش در صفحه اصلی
- [ ] Player Functionality
- [ ] Responsive Design
- [ ] Error Handling

---

## 📚 Documentation

1. **API Documentation**: توضیح تمام Methods
2. **User Guide**: راهنمای استفاده برای Admin
3. **Developer Guide**: راهنمای توسعه
4. **Video Format Guide**: فرمت‌های پشتیبانی شده

---

## 🚀 Deployment Checklist

- [ ] Database Migration
- [ ] File Upload Paths Configuration
- [ ] CDN Configuration (اگر استفاده می‌شود)
- [ ] YouTube/Vimeo API Keys (اگر نیاز باشد)
- [ ] Error Logging
- [ ] Performance Monitoring

---

## 📝 Notes

### نکات مهم:
1. برای ویدیوهای بزرگ، بهتر است از YouTube/Vimeo استفاده شود
2. آپلود مستقیم فقط برای ویدیوهای کوچک (مثلاً کمتر از 50MB)
3. Thumbnail باید از YouTube/Vimeo API گرفته شود
4. Player باید Responsive باشد
5. SEO مهم است - Schema.org VideoObject اضافه شود

---

## ✅ نتیجه

این نقشه راه یک سیستم مدیریت ویدیو **حرفه‌ای، Production-Ready و Bulletproof** را ارائه می‌دهد که:
- ✅ تمام نیازمندی‌های کارفرما را پوشش می‌دهد
- ✅ قابل توسعه و نگهداری است
- ✅ از اصول SRP و Clean Architecture پیروی می‌کند
- ✅ Strongly-Typed و Type-Safe است
- ✅ UI/UX حرفه‌ای دارد
- ✅ برای محیط Production درمانی مناسب است

