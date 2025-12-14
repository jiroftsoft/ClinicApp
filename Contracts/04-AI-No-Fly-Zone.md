# 🚫 قرارداد پرهیز هوش مصنوعی

## AI No-Fly List – Pre-Flight Mandatory Contract

> **این قرارداد بخشی از چک‌لیست پیش‌پرواز است و رعایت تمام بندهای آن الزامی می‌باشد.**  
> هرگونه نقض حتی یک بند، تغییر را **باطل (INVALID)** می‌کند.

---

## 📋 فهرست مطالب

1. [ممنوعیت حدس، فرض و تولید کد احتمالی](#1️⃣-ممنوعیت-حدس-فرض-و-تولید-کد-احتمالی)
2. [ممنوعیت نقض قراردادها](#2️⃣-ممنوعیت-نقض-قراردادها-absolute-rule)
3. [ممنوعیت دور زدن معماری لایه‌ای](#3️⃣-ممنوعیت-دور-زدن-معماری-لایه‌ای)
4. [ممنوعیت حذف یا ساده‌سازی ServiceResult](#4️⃣-ممنوعیت-حذف-یا-ساده‌سازی-serviceresult)
5. [ممنوعیت بی‌توجهی به امنیت داده‌های درمانی](#5️⃣-ممنوعیت-بی‌توجهی-به-امنیت-داده‌های-درمانی)
6. [ممنوعیت تولید کد بدون لاگ‌پذیری](#6️⃣-ممنوعیت-تولید-کد-بدون-لاگ‌پذیری)
7. [ممنوعیت تخطی از استاندارد تاریخ شمسی](#7️⃣-ممنوعیت-تخطی-از-استاندارد-تاریخ-شمسی)
8. [ممنوعیت آپلود فایل خارج از سیستم استاندارد](#8️⃣-ممنوعیت-آپلود-فایل-خارج-از-سیستم-استاندارد)
9. [ممنوعیت تغییر Silent (بی‌صدا)](#9️⃣-ممنوعیت-تغییر-silent-بی‌صدا)
10. [ممنوعیت تولید کد بدون مستندسازی](#🔟-ممنوعیت-تولید-کد-بدون-مستندسازی)
11. [ممنوعیت پیشنهاد Library یا Pattern ناسازگار](#1️⃣1️⃣-ممنوعیت-پیشنهاد-library-یا-pattern-ناسازگار)
12. [ممنوعیت تغییر رفتار سیستم بدون تست ذهنی](#1️⃣2️⃣-ممنوعیت-تغییر-رفتار-سیستم-بدون-تست-ذهنی)
13. [ممنوعیت ساده‌سازی بیش از حد](#1️⃣3️⃣-ممنوعیت-ساده‌سازی-بیش-از-حد-over-simplification)
14. [ممنوعیت تصمیم‌گیری مستقل](#1️⃣4️⃣-ممنوعیت-تصمیم‌گیری-مستقل)
15. [شرط توقف فوری](#1️⃣5️⃣-شرط-توقف-فوری-hard-stop-rule)

---

## 1️⃣ ممنوعیت حدس، فرض و تولید کد احتمالی

### ❌ هوش مصنوعی **مجاز نیست**:

* کدی را بر اساس «احتمالاً اینجا این‌طور است» تولید کند
* ساختار دیتابیس، Entity یا ViewModel را حدس بزند
* متدی را بدون مشاهده یا ارجاع واقعی پیشنهاد دهد
* از الگوهای مشابه در پروژه‌های دیگر استفاده کند بدون بررسی کد موجود

### ✅ الزام:

* در صورت نبود اطلاعات → **اعلام عدم قطعیت + درخواست شفاف‌سازی**
* قبل از تولید کد → **خواندن فایل‌های مرتبط**
* قبل از پیشنهاد → **جستجوی الگوهای موجود در پروژه**

### 📝 مثال:

```csharp
// ❌ اشتباه - حدس زدن ساختار
public class MyEntity
{
    public int Id { get; set; } // حدس زده شده
    public string Name { get; set; } // حدس زده شده
}

// ✅ درست - خواندن فایل واقعی
// ابتدا Models/Entities/MyEntity.cs را بخوان
// سپس بر اساس ساختار واقعی کد بنویس
```

---

## 2️⃣ ممنوعیت نقض قراردادها (Absolute Rule)

### ❌ ممنوع:

* تغییر یا نادیده‌گرفتن هر فایل داخل `CONTRACTS/`
* تفسیر شخصی قراردادها
* ارائه راه‌حل «ساده‌تر ولی خارج از قرارداد»
* استثنا قائل شدن برای «این مورد خاص»

### ✅ اصل:

> **قرارداد ← بالاتر از کد**  
> **قرارداد ← بالاتر از پیشنهاد هوشمندانه**  
> **قرارداد ← بالاتر از سرعت توسعه**

### 📝 قراردادهای کلیدی:

* `01-PreFlight-Protocol.md` - چک‌لیست پیش پرواز
* `02-Architecture-Guidelines.md` - راهنمای معماری
* `03-Code-Quality-Standards.md` - استانداردهای کیفیت کد
* `DEVELOPMENT_CONTRACT.md` - قرارداد توسعه
* `ground-rules.md` - قوانین پایه

### 🔍 فرآیند بررسی:

1. قبل از هر تغییر → **خواندن تمام قراردادهای مرتبط**
2. در صورت تعارض → **ارجاع به قرارداد + درخواست تأیید**
3. هرگز → **تفسیر شخصی یا استثنا بدون تأیید**

---

## 3️⃣ ممنوعیت دور زدن معماری لایه‌ای

### ❌ هوش مصنوعی حق ندارد:

* منطق کسب‌وکار را داخل Controller بنویسد
* از Controller مستقیم به Repository وصل شود
* Entity را مستقیماً به View پاس دهد
* از `ViewBag`/`ViewData` برای داده‌های اصلی استفاده کند
* از `dynamic` استفاده کند

### ✅ الزام:

```
Controller → Service → Repository
Entity → Factory → ViewModel → View
```

### 📝 مثال:

```csharp
// ❌ اشتباه - Business Logic در Controller
public ActionResult Create(MyViewModel model)
{
    var entity = new MyEntity();
    entity.Name = model.Name;
    entity.Price = model.Price * 1.09; // Business Logic در Controller!
    _context.MyEntities.Add(entity);
    await _context.SaveChangesAsync();
    return RedirectToAction("Index");
}

// ✅ درست - Business Logic در Service
public ActionResult Create(MyViewModel model)
{
    var result = await _myService.CreateAsync(model);
    if (!result.Success)
    {
        NotificationHelper.SetError(TempData, result.Message);
        return View(model);
    }
    NotificationHelper.SetSuccess(TempData, "با موفقیت ایجاد شد");
    return RedirectToAction("Index");
}
```

---

## 4️⃣ ممنوعیت حذف یا ساده‌سازی ServiceResult

### ❌ ممنوع:

* بازگرداندن `bool`، `string` یا `null`
* حذف ErrorCode، Message یا Status
* استفاده از Exception برای کنترل جریان عادی
* بازگرداندن `void` برای عملیات مهم

### ✅ تمام خروجی‌ها باید:

* `ServiceResult<T>` یا `ServiceResult` باشند
* شامل `Success`، `Message`، `Code` باشند
* قابل لاگ، قابل تست و قابل گسترش باشند

### 📝 مثال:

```csharp
// ❌ اشتباه
public bool CreateUser(User user)
{
    if (user == null) return false;
    _context.Users.Add(user);
    _context.SaveChanges();
    return true;
}

// ✅ درست
public async Task<ServiceResult<User>> CreateUserAsync(UserCreateViewModel model)
{
    if (model == null)
    {
        return ServiceResult<User>.Failure("مدل نامعتبر است", "INVALID_MODEL");
    }
    
    try
    {
        var user = model.ToEntity();
        _repository.Add(user);
        await _repository.SaveChangesAsync();
        
        _logger.Information("کاربر با شناسه {UserId} ایجاد شد", user.Id);
        return ServiceResult<User>.Successful(user, "کاربر با موفقیت ایجاد شد");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در ایجاد کاربر");
        return ServiceResult<User>.Failure("خطا در ایجاد کاربر", "CREATE_USER_ERROR")
            .WithExceptionDev(ex);
    }
}
```

---

## 5️⃣ ممنوعیت بی‌توجهی به امنیت داده‌های درمانی

### ❌ هوش مصنوعی مجاز نیست:

* داده پزشکی را بدون Validation پردازش کند
* لاگ حساس تولید کند (کد ملی، شماره تلفن، اطلاعات پزشکی)
* پیشنهاد ذخیره داده حساس در Session / ViewBag / TempData بدهد
* از `[AllowHtml]` بدون Sanitization استفاده کند
* CSRF Protection را حذف یا نادیده بگیرد

### ✅ اصل:

> **امنیت ← بالاتر از سرعت**  
> **امنیت ← بالاتر از راحتی توسعه**  
> **امنیت ← بالاتر از بهینه‌سازی**

### 📝 الزامات امنیتی:

1. **CSRF Protection:**
   ```csharp
   [HttpPost]
   [ValidateAntiForgeryToken]
   public ActionResult Create(MyViewModel model) { }
   ```

2. **Input Validation:**
   ```csharp
   [Required(ErrorMessage = "نام الزامی است")]
   [MaxLength(200)]
   public string Name { get; set; }
   ```

3. **Sensitive Data Masking:**
   ```csharp
   _logger.Information("کاربر {NationalCode} لاگین کرد", 
       SensitiveDataMaskingHelper.MaskNationalCode(nationalCode));
   ```

4. **Zero-Cache Policy:**
   ```csharp
   [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
   public ActionResult Index() { }
   ```

---

## 6️⃣ ممنوعیت تولید کد بدون لاگ‌پذیری

### ❌ ممنوع:

* عملیات حساس بدون Serilog
* حذف Context لاگ (UserId, Action, EntityId)
* لاگ کردن بدون Structure
* لاگ کردن داده‌های حساس بدون Masking

### ✅ هر عملیات مهم باید:

* قابل Audit باشد
* قابل Trace باشد
* قابل Forensic باشد

### 📝 مثال:

```csharp
// ❌ اشتباه - بدون لاگ
public async Task<ServiceResult> DeleteAsync(int id)
{
    var entity = await _repository.GetByIdAsync(id);
    _repository.Delete(entity);
    await _repository.SaveChangesAsync();
    return ServiceResult.Successful();
}

// ✅ درست - با لاگ کامل
public async Task<ServiceResult> DeleteAsync(int id)
{
    try
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            _logger.Warning("تلاش برای حذف موجودیت ناموجود: {EntityId}", id);
            return ServiceResult.Failure("موجودیت یافت نشد", "ENTITY_NOT_FOUND");
        }
        
        _logger.Information("شروع حذف موجودیت: {EntityId} توسط کاربر {UserId}", 
            id, _currentUserService.UserId);
        
        _repository.Delete(entity);
        await _repository.SaveChangesAsync();
        
        _logger.Information("موجودیت {EntityId} با موفقیت حذف شد", id);
        return ServiceResult.Successful("موجودیت با موفقیت حذف شد");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در حذف موجودیت {EntityId}", id);
        return ServiceResult.Failure("خطا در حذف موجودیت", "DELETE_ERROR")
            .WithExceptionDev(ex);
    }
}
```

---

## 7️⃣ ممنوعیت تخطی از استاندارد تاریخ شمسی

### ❌ مطلقاً ممنوع:

* DateTimePicker میلادی
* Parse دستی تاریخ
* نمایش مستقیم DateTime
* استفاده از `ToString("yyyy/MM/dd")` برای تاریخ شمسی
* استفاده از `datetime-local` input

### ✅ الزام:

* **Persian DatePicker** - استفاده از `_PersianDatePicker` partial view
* **Parse:** `this.ParseDateFromHiddenInput("FieldName", _logger)`
* **Display:** `PersianDateHelper.ToPersianDate(dateTime)`
* **Controller:** Parse کردن در POST Actions
* **View:** استفاده از `_PersianDatePickerScript` در Scripts section

### 📝 مثال:

```csharp
// ❌ اشتباه
@Html.TextBoxFor(m => m.StartDate, new { type = "datetime-local" })

// ✅ درست
@{
    ViewBag.PersianDatePickerId = "startDatePicker";
    ViewBag.PersianDatePickerName = "StartDate";
    ViewBag.PersianDatePickerValue = Model.StartDate;
    ViewBag.PersianDatePickerLabel = "تاریخ شروع";
}
@Html.Partial("_PersianDatePicker")
```

```csharp
// ❌ اشتباه - در Controller
public ActionResult Create(MyViewModel model)
{
    // تاریخ به صورت میلادی از Model Binding می‌آید
}

// ✅ درست - در Controller
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Create(MyViewModel model)
{
    model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
    model.EndDate = this.ParseDateFromHiddenInput("EndDate", _logger);
    // ...
}
```

---

## 8️⃣ ممنوعیت آپلود فایل خارج از سیستم استاندارد

### ❌ ممنوع:

* استفاده مستقیم از `HttpPostedFileBase`
* ذخیره فایل بدون `IImageUploadService`
* نداشتن Preview و Validation
* استفاده از `file.SaveAs()` مستقیم
* عدم بررسی File Signature

### ✅ الزام:

* استفاده از `IImageUploadService`
* پیاده‌سازی `ProcessImageUpload` در Controller
* Validation کامل (Type, Size, Signature, Dimension)
* Preview در Client-Side
* Error Handling کامل

### 📝 مثال:

```csharp
// ❌ اشتباه
var file = Request.Files["ImageFile"];
file.SaveAs(Server.MapPath("~/Content/Images/" + file.FileName));

// ✅ درست
private async Task ProcessImageUpload(MyViewModel model)
{
    var imageFile = Request.Files["ImageFile"];
    if (imageFile != null && imageFile.ContentLength > 0)
    {
        var uploadResult = _imageUploadService.UploadImageWithThumbnail(
            imageFile,
            "~/Content/Images/my-module",
            "~/Content/Images/my-module/thumbnails",
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

## 9️⃣ ممنوعیت تغییر Silent (بی‌صدا)

### ❌ هوش مصنوعی حق ندارد:

* کدی را تغییر دهد بدون ذکر دلیل
* Refactor انجام دهد بدون توضیح Impact
* Behavior سیستم را تغییر دهد بدون اعلام صریح
* متد را Rename کند بدون اطلاع
* Parameter را تغییر دهد بدون مستندسازی

### ✅ هر تغییر باید شامل:

* **دلیل:** چرا این تغییر لازم است؟
* **ریسک:** چه ریسک‌هایی دارد؟
* **اثر جانبی:** چه بخش‌هایی تحت تأثیر قرار می‌گیرند؟
* **تست:** چه تست‌هایی باید انجام شود؟

### 📝 مثال گزارش تغییر:

```markdown
## تغییر: بهینه‌سازی Query در GetDoctorsAsync

### دلیل:
- Query فعلی N+1 Problem دارد
- Performance در لیست‌های بزرگ کند است

### تغییرات:
- اضافه کردن `.Include(d => d.Departments)`
- استفاده از `.AsNoTracking()` برای Read-Only

### ریسک:
- ⚠️ ممکن است Navigation Properties تغییر کند
- ⚠️ نیاز به تست کامل

### اثر جانبی:
- تمام Controller هایی که از این متد استفاده می‌کنند
- View هایی که Navigation Properties را نمایش می‌دهند

### تست:
- [ ] تست لیست پزشکان
- [ ] تست فیلتر بر اساس دپارتمان
- [ ] تست Performance با 1000+ رکورد
```

---

## 🔟 ممنوعیت تولید کد بدون مستندسازی

### ❌ ممنوع:

* کلاس بدون XML Documentation
* متد بدون توضیح هدف و ورودی/خروجی
* Interface بدون توضیح Contract
* Enum بدون Description

### ✅ مستندسازی جزئی از کد است، نه اضافه‌کار

### 📝 مثال:

```csharp
// ❌ اشتباه
public class MyService
{
    public bool DoSomething(int id, string name)
    {
        // ...
    }
}

// ✅ درست
/// <summary>
/// سرویس مدیریت موجودیت‌های MyEntity
/// </summary>
public class MyService : IMyService
{
    /// <summary>
    /// انجام عملیات خاص بر روی موجودیت
    /// </summary>
    /// <param name="id">شناسه موجودیت</param>
    /// <param name="name">نام جدید</param>
    /// <returns>نتیجه عملیات</returns>
    /// <exception cref="ArgumentException">در صورت نامعتبر بودن id</exception>
    public async Task<ServiceResult<bool>> DoSomethingAsync(int id, string name)
    {
        // ...
    }
}
```

---

## 1️⃣1️⃣ ممنوعیت پیشنهاد Library یا Pattern ناسازگار

### ❌ ممنوع:

* معرفی Framework جدید (مثلاً ASP.NET Core در پروژه MVC 5)
* تغییر ORM (مثلاً Dapper در پروژه EF6)
* تغییر Pattern اصلی (مثلاً Repository به DTO Pattern)
* معرفی Library جدید بدون بررسی وابستگی‌ها

### ✅ مگر با:

* توجیه فنی مکتوب
* تأیید صریح مالک پروژه
* بررسی کامل Impact Assessment
* Migration Plan

### 📝 فرآیند تأیید:

```markdown
## پیشنهاد: استفاده از Library X

### توجیه فنی:
- مشکل فعلی: ...
- راه‌حل Library X: ...
- مزایا: ...

### بررسی وابستگی‌ها:
- سازگاری با .NET Framework 4.8: ✅
- سازگاری با ASP.NET MVC 5: ✅
- License: MIT (مجاز)

### Impact Assessment:
- فایل‌های تحت تأثیر: 5 فایل
- Breaking Changes: ندارد
- Migration Time: 2 ساعت

### تأیید مالک پروژه:
- [ ] تأیید شده
- [ ] رد شده
```

---

## 1️⃣2️⃣ ممنوعیت تغییر رفتار سیستم بدون تست ذهنی

### ❌ هوش مصنوعی نباید:

* فقط کد تولید کند
* بدون تحلیل سناریوهای واقعی کلینیک
* بدون در نظر گیری User Experience
* بدون بررسی Edge Cases

### ✅ باید:

* اثر روی بیمار، پزشک، منشی و ادمین تحلیل شود
* سناریوهای واقعی کلینیک در نظر گرفته شود
* Edge Cases شناسایی و مدیریت شود

### 📝 چک‌لیست تست ذهنی:

```markdown
## تست ذهنی: تغییر در محاسبه قیمت

### سناریو 1: بیمار عادی
- ورودی: Service با قیمت 100,000 تومان
- بیمه: ندارد
- خروجی مورد انتظار: 100,000 تومان
- ✅ صحیح

### سناریو 2: بیمار با بیمه پایه
- ورودی: Service با قیمت 100,000 تومان
- بیمه: پایه با Coverage 70%
- خروجی مورد انتظار: 30,000 تومان سهم بیمار
- ✅ صحیح

### سناریو 3: بیمار با بیمه پایه + تکمیلی
- ورودی: Service با قیمت 100,000 تومان
- بیمه: پایه 70% + تکمیلی 20%
- خروجی مورد انتظار: 10,000 تومان سهم بیمار
- ✅ صحیح

### Edge Case 1: قیمت صفر
- ورودی: Service با قیمت 0 تومان
- خروجی: خطا یا هشدار؟
- ⚠️ نیاز به بررسی

### Edge Case 2: Coverage بیش از 100%
- ورودی: Coverage 110%
- خروجی: خطا یا Cap کردن؟
- ⚠️ نیاز به بررسی
```

---

## 1️⃣3️⃣ ممنوعیت ساده‌سازی بیش از حد (Over-Simplification)

### ❌ ممنوع:

* حذف لایه‌ها برای «کوتاه شدن کد»
* حذف abstraction برای «خوانایی»
* ترکیب Service و Repository
* حذف ViewModel برای «سادگی»

### ✅ اصل:

> **سادگی ≠ ساده‌سازی خطرناک**  
> **خوانایی ≠ حذف لایه‌ها**

### 📝 مثال:

```csharp
// ❌ اشتباه - حذف Service Layer
public class MyController : Controller
{
    private readonly IMyRepository _repository;
    
    public async Task<ActionResult> Create(MyEntity entity)
    {
        // Business Logic در Controller!
        if (entity.Price < 0) return View("Error");
        entity.TotalPrice = entity.Price * 1.09; // Business Logic!
        _repository.Add(entity);
        await _repository.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}

// ✅ درست - حفظ لایه‌ها
public class MyController : Controller
{
    private readonly IMyService _service;
    
    public async Task<ActionResult> Create(MyCreateViewModel model)
    {
        var result = await _service.CreateAsync(model);
        if (!result.Success)
        {
            NotificationHelper.SetError(TempData, result.Message);
            return View(model);
        }
        NotificationHelper.SetSuccess(TempData, "با موفقیت ایجاد شد");
        return RedirectToAction("Index");
    }
}
```

---

## 1️⃣4️⃣ ممنوعیت تصمیم‌گیری مستقل

### ❌ AI تصمیم نهایی نمی‌گیرد
### ❌ AI مالک معماری نیست
### ❌ AI نمی‌تواند قراردادها را تغییر دهد

### ✅ نقش AI:

> **پیشنهاددهنده‌ی دقیق، نه تصمیم‌گیرنده**

### 📝 فرآیند تصمیم‌گیری:

```markdown
## پیشنهاد: تغییر در ساختار Entity

### تحلیل:
- مشکل فعلی: ...
- راه‌حل پیشنهادی: ...

### گزینه‌ها:
1. **گزینه A:** ...
   - مزایا: ...
   - معایب: ...
   
2. **گزینه B:** ...
   - مزایا: ...
   - معایب: ...

### توصیه:
- گزینه A را توصیه می‌کنم به دلیل ...

### نیاز به تأیید:
- [ ] تأیید مالک پروژه برای گزینه A
- [ ] یا انتخاب گزینه B
- [ ] یا راه‌حل جایگزین
```

---

## 1️⃣5️⃣ شرط توقف فوری (Hard Stop Rule)

### 🛑 اگر هر یک از موارد زیر رخ دهد، AI **باید متوقف شود**:

* عدم قطعیت در قرارداد
* تعارض بین امنیت و درخواست
* ابهام در داده‌های پزشکی
* عدم دسترسی به فایل‌های کلیدی
* تعارض بین قراردادها

### ✅ و باید اعلام کند:

> ❗ **توقف فوری - ادامه بدون شفاف‌سازی ممکن نیست**

### 📝 مثال:

```markdown
## ❗ توقف فوری

### دلیل:
- قرارداد `DEVELOPMENT_CONTRACT.md` استفاده از `IImageUploadService` را الزامی می‌کند
- اما درخواست شما استفاده مستقیم از `HttpPostedFileBase` است

### تعارض:
- ❌ درخواست شما ← نقض قرارداد
- ✅ قرارداد ← الزام استفاده از `IImageUploadService`

### نیاز به شفاف‌سازی:
1. آیا می‌خواهید قرارداد را تغییر دهید؟
2. یا می‌خواهید از `IImageUploadService` استفاده کنیم؟
3. یا دلیل خاصی برای استثنا وجود دارد؟

### ادامه:
- ⚠️ تا زمان شفاف‌سازی، نمی‌توانم کد تولید کنم
```

---

## 📌 وضعیت قرارداد

* ✅ این لیست بخشی از **Pre-Flight Checklist** است
* ✅ اجرای آن قبل از هر تغییر **الزامی** است
* ✅ عدم تبعیت = رد کامل تغییر
* ✅ نسخه: 1.0.0
* ✅ تاریخ: 2025-01-27
* ✅ وضعیت: فعال و الزامی

---

## 🔗 قراردادهای مرتبط

* [`01-PreFlight-Protocol.md`](./01-PreFlight-Protocol.md) - چک‌لیست پیش پرواز
* [`02-Architecture-Guidelines.md`](./02-Architecture-Guidelines.md) - راهنمای معماری
* [`03-Code-Quality-Standards.md`](./03-Code-Quality-Standards.md) - استانداردهای کیفیت کد
* [`Docs/DEVELOPMENT_CONTRACT.md`](../Docs/DEVELOPMENT_CONTRACT.md) - قرارداد توسعه
* [`Docs/ground-rules.md`](../Docs/ground-rules.md) - قوانین پایه

---

## ✅ چک‌لیست قبل از هر تغییر

قبل از شروع هر تغییر، این چک‌لیست را بررسی کن:

- [ ] تمام قراردادهای مرتبط را خوانده‌ام
- [ ] ساختار واقعی Entity/ViewModel را بررسی کرده‌ام
- [ ] الگوهای موجود در پروژه را جستجو کرده‌ام
- [ ] هیچ حدس و فرضی نزده‌ام
- [ ] معماری لایه‌ای را رعایت می‌کنم
- [ ] ServiceResult را استفاده می‌کنم
- [ ] امنیت داده‌های درمانی را در نظر گرفته‌ام
- [ ] لاگ‌گذاری کامل پیاده‌سازی کرده‌ام
- [ ] استاندارد تاریخ شمسی را رعایت می‌کنم
- [ ] سیستم آپلود استاندارد را استفاده می‌کنم
- [ ] تمام تغییرات را مستند کرده‌ام
- [ ] تست ذهنی انجام داده‌ام
- [ ] در صورت تعارض، توقف کرده‌ام و شفاف‌سازی خواسته‌ام

---

**تهیه شده توسط:** AI Assistant (Senior .NET Architect & Healthcare Systems Specialist)  
**تاریخ:** 2025-01-27  
**نسخه:** 1.0.0  
**وضعیت:** ✅ فعال و الزامی
