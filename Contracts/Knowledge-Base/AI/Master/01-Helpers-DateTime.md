# 📅 راهنمای کامل Helpers تاریخ و زمان

**نسخه:** 1.0.0  
**تاریخ:** 1404/10/05  
**تعداد Helpers:** 6

---

## 📋 فهرست

1. [PersianDateHelper.cs](#1-persiandatehelpercs)
2. [PersianDatePickerHelper.cs](#2-persiandatepickerhelpercs)
3. [DateTimeExtensions.cs](#3-datetimeextensionscs)
4. [PersianDateExtensions.cs](#4-persiandateextensionscs)
5. [TimeFormatHelper.cs](#5-timeformathelpercs)
6. [AgeCalculationHelper.cs](#6-agecalculationhelpercs)
7. [ControllerExtensions.ParseDateFromHiddenInput](#7-controllerextensionsparsedatefromhiddeninput)

---

## 1️⃣ PersianDateHelper.cs

**مسیر:** `Helpers/PersianDateHelper.cs`  
**حجم:** 31,469 بایت  
**هدف:** تبدیل تاریخ میلادی ↔ شمسی

### 📌 توابع اصلی:

#### **1.1. ToPersianDate() - تبدیل میلادی به شمسی**

```csharp
// ✅ تبدیل DateTime به string شمسی
public static string ToPersianDate(DateTime? date)
```

**مثال‌ها:**
```csharp
// مثال 1: تبدیل ساده
var persianDate = PersianDateHelper.ToPersianDate(DateTime.Now);
// خروجی: "1404/10/05"

// مثال 2: تبدیل با بررسی null
var persianDate = PersianDateHelper.ToPersianDate(model.BirthDate);
// خروجی: "1380/05/15" یا null

// مثال 3: استفاده در View
@PersianDateHelper.ToPersianDate(Model.CreatedAt)
// خروجی در View: "1404/10/05"
```

**Use Cases:**
- ✅ نمایش تاریخ در View (Index, Details)
- ✅ نمایش تاریخ در گزارش‌ها
- ✅ لاگ‌گذاری با تاریخ شمسی

---

#### **1.2. ParsePersianDate() - تبدیل شمسی به میلادی**

```csharp
// ✅ تبدیل string شمسی به DateTime میلادی
public static DateTime? ParsePersianDate(string persianDate)
```

**مثال‌ها:**
```csharp
// مثال 1: تبدیل string به DateTime
var gregorianDate = PersianDateHelper.ParsePersianDate("1404/10/05");
// خروجی: DateTime(2025, 12, 25)

// مثال 2: بررسی null
var date = PersianDateHelper.ParsePersianDate(null);
// خروجی: null

// مثال 3: Format نادرست
var date = PersianDateHelper.ParsePersianDate("1404-10-05"); // با خط فاصله
// خروجی: null (نیاز به "/" دارد)
```

**Use Cases:**
- ✅ دریافت تاریخ از فرم
- ✅ پردازش تاریخ از API
- ✅ Import داده از Excel

---

#### **1.3. ToPersianDateString() - تبدیل با فرمت سفارشی**

```csharp
// ✅ تبدیل با فرمت دلخواه
public static string ToPersianDateString(DateTime date, string format)
```

**مثال‌ها:**
```csharp
// مثال 1: فرمت کامل
var date = PersianDateHelper.ToPersianDateString(DateTime.Now, "yyyy/MM/dd");
// خروجی: "1404/10/05"

// مثال 2: فقط سال و ماه
var date = PersianDateHelper.ToPersianDateString(DateTime.Now, "yyyy/MM");
// خروجی: "1404/10"

// مثال 3: فرمت طولانی
var date = PersianDateHelper.ToPersianDateString(DateTime.Now, "yyyy/MM/dd - HH:mm");
// خروجی: "1404/10/05 - 15:40"
```

---

### 🎯 Best Practices:

```csharp
// ✅ درست - همیشه از Helper استفاده کن
var persianDate = PersianDateHelper.ToPersianDate(DateTime.Now);

// ❌ اشتباه - تبدیل دستی
var persianDate = DateTime.Now.ToString("yyyy/MM/dd"); // این میلادی است!

// ✅ درست - در View
@PersianDateHelper.ToPersianDate(Model.Date)

// ❌ اشتباه - در View
@Model.Date.ToString("yyyy/MM/dd")
```

---

## 2️⃣ PersianDatePickerHelper.cs

**مسیر:** `Helpers/PersianDatePickerHelper.cs`  
**حجم:** 14,055 بایت  
**هدف:** DatePicker شمسی در View

### 📌 استفاده:

```razor
@* در View - Create/Edit *@
@{
    ViewBag.PersianDatePickerId = "startDatePicker";
    ViewBag.PersianDatePickerName = "StartDate";
    ViewBag.PersianDatePickerValue = Model.StartDate;
    ViewBag.PersianDatePickerLabel = "تاریخ شروع";
    ViewBag.PersianDatePickerPlaceholder = "تاریخ شروع را انتخاب کنید";
    ViewBag.PersianDatePickerHelpText = "اختیاری - اگر خالی باشد، از همین الان شروع می‌شود";
    ViewBag.PersianDatePickerRequired = false;
}
@Html.Partial("_PersianDatePicker")

@* در Scripts Section *@
@section Scripts {
    @Html.Partial("_PersianDatePickerScript")
}
```

### 🎯 Best Practices:

```razor
// ✅ درست - استفاده از Partial View
@Html.Partial("_PersianDatePicker")

// ❌ اشتباه - datetime-local
<input type="datetime-local" ... />
```

---

## 3️⃣ DateTimeExtensions.cs

**مسیر:** `Extensions/DateTimeExtensions.cs`  
**حجم:** 2,714 بایت

### 📌 Extension Methods:

```csharp
// ✅ تبدیل به تاریخ شمسی (Extension Method)
public static string ToPersianDate(this DateTime date)
public static string ToPersianDate(this DateTime? date)
```

**مثال‌ها:**
```csharp
using ClinicApp.Extensions;

// مثال 1: استفاده از Extension
var persianDate = DateTime.Now.ToPersianDate();
// خروجی: "1404/10/05"

// مثال 2: با nullable
DateTime? date = model.BirthDate;
var persianDate = date.ToPersianDate();
// خروجی: "1380/05/15" یا null

// مثال 3: در LINQ
var list = patients.Select(p => new {
    Name = p.Name,
    BirthDate = p.BirthDate.ToPersianDate()
}).ToList();
```

### 🎯 Best Practices:

```csharp
// ✅ درست - Extension Method
using ClinicApp.Extensions;
var date = DateTime.Now.ToPersianDate();

// ✅ همچنین درست - Helper Method
var date = PersianDateHelper.ToPersianDate(DateTime.Now);

// هر دو صحیح هستند - انتخاب بر اساس سلیقه
```

---

## 4️⃣ PersianDateExtensions.cs

**مسیر:** `Extensions/PersianDateExtensions.cs`  
**حجم:** 12,130 بایت

### 📌 Extension Methods پیشرفته:

```csharp
// ✅ تبدیل با فرمت سفارشی
public static string ToPersianDateString(this DateTime date, string format)

// ✅ بررسی تاریخ شمسی معتبر
public static bool IsValidPersianDate(this string persianDate)

// ✅ مقایسه تاریخ‌ها
public static int ComparePersianDates(this string persianDate1, string persianDate2)
```

**مثال‌ها:**
```csharp
using ClinicApp.Extensions;

// مثال 1: فرمت سفارشی
var date = DateTime.Now.ToPersianDateString("yyyy/MM/dd - HH:mm");
// خروجی: "1404/10/05 - 15:40"

// مثال 2: Validation
var isValid = "1404/10/05".IsValidPersianDate();
// خروجی: true

var isValid = "1404/13/32".IsValidPersianDate();
// خروجی: false

// مثال 3: مقایسه
var result = "1404/10/05".ComparePersianDates("1404/09/01");
// خروجی: 1 (اولی بزرگتر است)
```

---

## 5️⃣ TimeFormatHelper.cs

**مسیر:** `Helpers/TimeFormatHelper.cs`  
**حجم:** 1,547 بایت

### 📌 توابع:

```csharp
// ✅ فرمت زمان فارسی
public static string FormatTime(DateTime dateTime)

// ✅ فرمت مدت زمان
public static string FormatDuration(TimeSpan duration)
```

**مثال‌ها:**
```csharp
// مثال 1: فرمت زمان
var time = TimeFormatHelper.FormatTime(DateTime.Now);
// خروجی: "15:40:22"

// مثال 2: فرمت مدت
var duration = TimeSpan.FromMinutes(125);
var formatted = TimeFormatHelper.FormatDuration(duration);
// خروجی: "2 ساعت و 5 دقیقه"
```

---

## 6️⃣ AgeCalculationHelper.cs

**مسیر:** `Helpers/AgeCalculationHelper.cs`  
**حجم:** 14,719 بایت

### 📌 توابع اصلی:

```csharp
// ✅ محاسبه سن (عدد)
public static int? CalculateAge(DateTime? birthDate)

// ✅ محاسبه سن (string فارسی)
public static string CalculateAgeString(DateTime? birthDate)
```

**مثال‌ها:**
```csharp
// مثال 1: محاسبه سن
var age = AgeCalculationHelper.CalculateAge(new DateTime(1990, 5, 15));
// خروجی: 35

// مثال 2: string فارسی
var ageString = AgeCalculationHelper.CalculateAgeString(new DateTime(1990, 5, 15));
// خروجی: "۳۵ سال"

// مثال 3: در ViewModel
public class PatientViewModel
{
    public DateTime? BirthDate { get; set; }
    
    public int? Age => AgeCalculationHelper.CalculateAge(BirthDate);
    
    public string AgeString => AgeCalculationHelper.CalculateAgeString(BirthDate);
}

// در View:
@Model.AgeString
// خروجی: "۳۵ سال"
```

**Validation:**
```csharp
// ✅ محدودیت سن (0-150 سال)
var age = AgeCalculationHelper.CalculateAge(new DateTime(1800, 1, 1));
// خروجی: null (خارج از محدوده)
```

---

## 7️⃣ ControllerExtensions.ParseDateFromHiddenInput

**مسیر:** `Helpers/ControllerExtensions.cs`  
**هدف:** Parse تاریخ از hidden input در Controller

### 📌 استفاده:

```csharp
using ClinicApp.Helpers;

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(MyViewModel model)
{
    // ✅ Parse تاریخ از hidden input
    model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
    model.EndDate = this.ParseDateFromHiddenInput("EndDate", _logger);
    
    if (!ModelState.IsValid)
    {
        return View(GetViewPath("Create"), model);
    }
    
    // ادامه عملیات...
}
```

**چرا از این Extension استفاده کنیم؟**
- ✅ Parse خودکار از hidden input
- ✅ لاگ خودکار خطاها
- ✅ مدیریت null
- ✅ یکپارچگی کد

---

## 📊 خلاصه و مقایسه

| Helper/Extension | استفاده | مثال |
|-----------------|---------|------|
| `PersianDateHelper.ToPersianDate()` | تبدیل به شمسی | `"1404/10/05"` |
| `PersianDateHelper.ParsePersianDate()` | تبدیل به میلادی | `DateTime(2025, 12, 25)` |
| `DateTime.ToPersianDate()` | Extension تبدیل | `DateTime.Now.ToPersianDate()` |
| `_PersianDatePicker` | DatePicker در View | `@Html.Partial("_PersianDatePicker")` |
| `ControllerExtensions.ParseDate` | Parse در Controller | `this.ParseDateFromHiddenInput("Date", _logger)` |
| `AgeCalculationHelper.CalculateAge()` | محاسبه سن | `35` |

---

## 🎯 Workflow کامل (تاریخ در Form)

### **1. در View (Create/Edit):**
```razor
@{
    ViewBag.PersianDatePickerId = "birthDatePicker";
    ViewBag.PersianDatePickerName = "BirthDate";
    ViewBag.PersianDatePickerValue = Model.BirthDate;
    ViewBag.PersianDatePickerLabel = "تاریخ تولد";
    ViewBag.PersianDatePickerRequired = true;
}
@Html.Partial("_PersianDatePicker")
@Html.Partial("_PersianDatePickerScript")
```

### **2. در Controller (POST):**
```csharp
[HttpPost]
public async Task<ActionResult> Create(PatientViewModel model)
{
    // ✅ Parse تاریخ
    model.BirthDate = this.ParseDateFromHiddenInput("BirthDate", _logger);
    
    // ✅ محاسبه سن
    var age = AgeCalculationHelper.CalculateAge(model.BirthDate);
    
    // بررسی سن
    if (age < 18)
    {
        ModelState.AddModelError("BirthDate", "سن باید بیشتر از 18 سال باشد");
    }
    
    // ادامه...
}
```

### **3. در View (Index/Details) - نمایش:**
```razor
@* نمایش تاریخ شمسی *@
@PersianDateHelper.ToPersianDate(Model.BirthDate)

@* نمایش سن *@
@AgeCalculationHelper.CalculateAgeString(Model.BirthDate)
```

---

## ⚠️ خطاهای رایج

### **❌ خطا 1: استفاده از ToString()**
```csharp
// ❌ اشتباه
var date = DateTime.Now.ToString("yyyy/MM/dd"); // میلادی است!

// ✅ درست
var date = PersianDateHelper.ToPersianDate(DateTime.Now); // شمسی
```

### **❌ خطا 2: عدم Parse در Controller**
```csharp
// ❌ اشتباه - مستقیم از Model
model.StartDate // null است یا اشتباه

// ✅ درست - Parse از hidden input
model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
```

### **❌ خطا 3: استفاده از datetime-local**
```html
<!-- ❌ اشتباه -->
<input type="datetime-local" name="BirthDate" />

<!-- ✅ درست -->
@Html.Partial("_PersianDatePicker")
```

---

## 📌 **۹. استفاده از DatePicker داخل مودال (JalaliDatePicker Enterprise)**

**مشکل:** وقتی فیلدهای تاریخ با `[data-jdp]` داخل یک مودال Bootstrap قرار دارند، ممکن است با کلیک روی اینپوت **تقویم باز نشود** یا **پشت مودال پنهان** شود.

**علت‌ها:**
1. **z-index:** پیش‌فرض کتابخانه (مثلاً 1000) پایین‌تر از مودال Bootstrap (1050) است و تقویم پشت مودال می‌ماند.
2. **زمان اتصال:** اگر مودال با AJAX لود شود، اینپوتها بعد از اجرای اولیهٔ `startWatch` به DOM اضافه می‌شوند و به تقویم وصل نمی‌شوند.

### ✅ **راه‌حل (ضد گلوله برای پروداکشن)**

#### **۱. تنظیم options پیش‌فرض (یک بار در پروژه)**

در **`Content/js/jalali-datepicker-enterprise.js`** در `config.defaultOptions` مقدارهای زیر را اضافه کنید:

```javascript
defaultOptions: {
    // ... سایر options
    container: 'body',   // تقویم به body اضافه شود (خارج از مودال)
    zIndex: 1060         // بالاتر از مودال Bootstrap (~1050)
}
```

- **`container: 'body'`** → تقویم به `body` append می‌شود و پشت مودال نمی‌ماند.
- **`zIndex: 1060`** → تقویم همیشه بالاتر از مودال نمایش داده می‌شود.

#### **۲. اجرای مجدد startWatch برای اینپوتهای داخل مودال / محتوای AJAX**

در **`jalali-datepicker-enterprise.js`** یک متد عمومی اضافه کنید:

```javascript
startWatchAgain: function() {
    if (typeof jalaliDatepicker === 'undefined') return;
    var opts = JalaliDatePickerEnterprise.config.defaultOptions;
    jalaliDatepicker.startWatch(opts);
    JalaliDatePickerEnterprise.initializeAll();
}
```

این متد را در دو حالت فراخوانی کنید:

**الف) هنگام باز شدن مودال (در اسکریپت مربوط به همان مودال):**

```javascript
// مثال: هنگام باز کردن مودال تاریخچه پزشکی
$('#medicalHistoryModal').on('shown.bs.modal', function() {
    if (typeof JalaliDatePickerEnterprise !== 'undefined' && JalaliDatePickerEnterprise.startWatchAgain) {
        setTimeout(function() {
            JalaliDatePickerEnterprise.startWatchAgain();
        }, 100);
    }
});
```

یا مستقیماً در تابعی که مودال را باز می‌کند (بعد از `modal.show()`):

```javascript
if (typeof JalaliDatePickerEnterprise !== 'undefined' && JalaliDatePickerEnterprise.startWatchAgain) {
    setTimeout(function() {
        JalaliDatePickerEnterprise.startWatchAgain();
    }, 100);
}
```

**ب) بعد از لود محتوای تب با AJAX (اگر مودال داخل آن محتواست):**

```javascript
// مثال: بعد از لود تب «پرونده پزشکی» در داشبورد
if (typeof JalaliDatePickerEnterprise !== 'undefined' && JalaliDatePickerEnterprise.startWatchAgain) {
    JalaliDatePickerEnterprise.startWatchAgain();
}
```

#### **۳. در View مودال**

از همان پارشال استاندارد DatePicker استفاده کنید (با مسیر کامل در صورت لود از Area دیگر):

```razor
@{
    ViewBag.PersianDatePickerId = "startDatePicker";
    ViewBag.PersianDatePickerName = "StartDate";
    ViewBag.PersianDatePickerValue = Model?.StartDate;
    ViewBag.PersianDatePickerLabel = "تاریخ شروع";
    ViewBag.PersianDatePickerPlaceholder = "مثال: 1400/01/01";
    ViewBag.PersianDatePickerRequired = false;
    ViewBag.PersianDatePickerCssClass = "form-control";
}
@Html.Partial("~/Areas/Admin/Views/Shared/_PersianDatePicker.cshtml")
```

#### **۴. سمت سرور (دریافت تاریخ شمسی از فرم)**

اگر فرم با AJAX ارسال می‌شود، مقدار فیلد با نام `StartDate` به صورت رشتهٔ شمسی (مثلاً `1402/06/15`) می‌آید. در Controller آن را با `PersianDateHelper.ParsePersianDate` به `DateTime?` تبدیل کنید:

```csharp
var startStr = Request.Form["StartDate"];
if (!string.IsNullOrWhiteSpace(startStr))
    model.StartDate = PersianDateHelper.ParsePersianDate(startStr.Trim());
```

### 📋 **چک‌لیست استفاده از DatePicker در مودال**

- [ ] در `defaultOptions` مقدارهای `container: 'body'` و `zIndex: 1060` تنظیم شده است.
- [ ] متد `startWatchAgain()` در ماژول Enterprise وجود دارد و پس از باز شدن مودال یا لود AJAX فراخوانی می‌شود.
- [ ] در View مودال از پارشال `_PersianDatePicker` (با مسیر صحیح) استفاده شده است.
- [ ] در Controller برای فیلدهای تاریخ ارسالی از فرم از `PersianDateHelper.ParsePersianDate` استفاده شده است.

### 📚 **مراجع مرتبط**

- `Docs/Jalili/JALALIDATEPICKER_ENTERPRISE_GUIDE.md`
- همین سند، بخش ۸ (Enterprise-Grade Date Management)

---

## 📚 مراجع

- `Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md` - راهنمای کامل DatePicker
- `Docs/DEVELOPMENT_CONTRACT.md` - استانداردهای تاریخ
- `Helpers/PersianDateHelper.cs` - کد منبع
- `Extensions/DateTimeExtensions.cs` - Extension Methods

---

## 🔄 **8️⃣ Enterprise-Grade Date Management (الزامی)**

**مرجع:** `Docs/ENTERPRISE_DATE_MIGRATION_GUIDE.md`  
**اولویت:** 🔴 **CRITICAL - Production Deployment**  
**وضعیت:** ✅ **الزامی برای تمام DatePicker ها**

### 📋 **قانون طلایی:**
> **"همیشه UTC در دیتابیس، تبدیل به timezone محلی فقط برای نمایش"**

---

### **STEP 1: Dependency Injection Setup**

```csharp
// ✅ در Global.asax.cs یا UnityConfig.cs
// تزریق ITimeProvider به DI Container
container.RegisterType<ITimeProvider, DefaultTimeProvider>(new ContainerControlledLifetimeManager());
```

---

### **STEP 2: در Services (Backend)**

#### **❌ قبل (اشتباه):**
```csharp
public class AppointmentBookingService
{
    public async Task<ServiceResult> ReserveAppointmentAsync(...)
    {
        if (date < DateTime.Today) // ❌ مشکل timezone
        {
            return ServiceResult.Failed("...");
        }
        
        var appointment = new Appointment
        {
            CreatedAt = DateTime.Now // ❌ مشکل timezone
        };
    }
}
```

#### **✅ بعد (درست - Enterprise-Grade):**
```csharp
public class AppointmentBookingService
{
    private readonly ITimeProvider _timeProvider;
    
    public AppointmentBookingService(
        ITimeProvider timeProvider,
        ...)
    {
        _timeProvider = timeProvider;
    }
    
    public async Task<ServiceResult> ReserveAppointmentAsync(...)
    {
        // ✅ استفاده از UTC
        var utcNow = _timeProvider.UtcNow;
        var iranToday = _timeProvider.GetIranToday(); // برای Validation
        
        // ✅ Validation بر اساس timezone ایران
        if (request.AppointmentDate.Date < iranToday)
        {
            return ServiceResult.Failed("...");
        }
        
        // ✅ ذخیره در دیتابیس به صورت UTC
        var appointment = new Appointment
        {
            AppointmentDate = request.AppointmentDate.ToUniversalTime(),
            CreatedAt = _timeProvider.UtcNow // ✅ UTC
        };
    }
}
```

---

### **STEP 3: در Controllers (API Endpoints)**

#### **❌ قبل (اشتباه):**
```csharp
public JsonResult GetToday()
{
    var today = DateTime.Today; // ❌ مشکل timezone
    var persianToday = PersianDateHelper.ToPersianDate(today);
    return Json(new { persianDate = persianToday });
}
```

#### **✅ بعد (درست - Enterprise-Grade):**
```csharp
public JsonResult GetToday()
{
    // ✅ ENTERPRISE: استفاده از UTC و تبدیل به timezone ایران
    var utcNow = DateTime.UtcNow;
    var iranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
    var iranNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, iranTimeZone);
    var iranToday = iranNow.Date;
    
    var persianToday = PersianDateHelper.ToPersianDate(iranToday);
    
    return Json(new
    {
        success = true,
        persianDate = persianToday,
        gregorianDate = iranToday.ToString("yyyy-MM-dd"),
        utcTimestamp = utcNow,
        timezone = "Iran Standard Time (UTC+3:30)"
    });
}
```

---

### **STEP 4: در Views (DatePicker)**

#### **✅ استفاده از JalaliDatePicker Enterprise (الزامی):**

**⚠️ CRITICAL:** فقط از **JalaliDatePicker Enterprise** استفاده کنید. الگوی قدیمی (Persian DatePicker - babakhani) حذف شده است.

**مرجع:** `Docs/Jalili/JALALIDATEPICKER_ENTERPRISE_GUIDE.md`

```razor
@* ✅ ENTERPRISE-GRADE: استفاده از JalaliDatePicker Enterprise *@
@* ✅ طبق Docs/Jalili/JALALIDATEPICKER_ENTERPRISE_GUIDE.md *@
@{
    ViewBag.PersianDatePickerId = "startDatePicker";
    ViewBag.PersianDatePickerName = "StartDate";
    ViewBag.PersianDatePickerValue = Model.StartDate; // DateTime? (UTC از دیتابیس)
    ViewBag.PersianDatePickerLabel = "تاریخ شروع";
    ViewBag.PersianDatePickerPlaceholder = "تاریخ شروع را انتخاب کنید";
    ViewBag.PersianDatePickerHelpText = "";
    ViewBag.PersianDatePickerRequired = true;
    ViewBag.PersianDatePickerCssClass = "form-control";
}
@Html.Partial("_PersianDatePicker")

@section Scripts {
    @* ✅ ENTERPRISE-GRADE: استفاده از JalaliDatePicker Enterprise *@
    @* ❌ ممنوع: persian-datepicker.min.js (الگوی قدیمی حذف شده) *@
    @Html.Partial("_PersianDatePickerScript")
}
```

#### **❌ ممنوع:**
```html
<!-- ❌ ممنوع - استفاده از datetime-local -->
<input type="datetime-local" name="StartDate" />

<!-- ❌ ممنوع - استفاده از date -->
<input type="date" name="StartDate" />
```

---

### **STEP 5: Parse در Controller (POST)**

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Create(MyViewModel model)
{
    // ✅ Parse تاریخ از hidden input (تبدیل شمسی → میلادی)
    model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
    model.EndDate = this.ParseDateFromHiddenInput("EndDate", _logger);
    
    // ✅ تبدیل به UTC قبل از ذخیره در دیتابیس
    if (model.StartDate.HasValue)
    {
        model.StartDate = model.StartDate.Value.ToUniversalTime();
    }
    
    // ادامه عملیات...
}
```

---

### **STEP 6: نمایش در View (Index/Details)**

```razor
@* ✅ نمایش تاریخ شمسی (از UTC دیتابیس) *@
@PersianDateHelper.ToPersianDate(Model.StartDate)

@* ✅ نمایش با فرمت سفارشی *@
@PersianDateHelper.ToPersianDateString(Model.StartDate, "yyyy/MM/dd - HH:mm")
```

---

### ✅ **چک‌لیست Enterprise-Grade Date Management:**

```
□ Dependency Injection برای ITimeProvider تنظیم شده
□ تمام Services از ITimeProvider استفاده می‌کنند
□ تمام DateTime.Now به _timeProvider.UtcNow تبدیل شده
□ تمام DateTime.Today به _timeProvider.GetIranToday() تبدیل شده
□ تمام تاریخ‌ها در دیتابیس UTC هستند
□ تمام DatePicker ها از _PersianDatePicker استفاده می‌کنند
□ Parse در Controller با ParseDateFromHiddenInput انجام می‌شود
□ تبدیل به UTC قبل از ذخیره در دیتابیس
□ نمایش با PersianDateHelper.ToPersianDate()
□ تست در timezone‌های مختلف انجام شده
```

---

### 🚨 **ممنوعیت‌های مطلق:**

```csharp
// ❌ ممنوع: استفاده مستقیم از DateTime.Now
var now = DateTime.Now;

// ❌ ممنوع: استفاده مستقیم از DateTime.Today
var today = DateTime.Today;

// ❌ ممنوع: ذخیره تاریخ بدون UTC
appointment.CreatedAt = DateTime.Now;

// ❌ ممنوع: استفاده از datetime-local در View
<input type="datetime-local" />

// ❌ ممنوع: Parse مستقیم از Model (بدون ParseDateFromHiddenInput)
model.StartDate = // مستقیم از Model
```

---

### 📚 **مراجع:**

- `Docs/ENTERPRISE_DATE_MIGRATION_GUIDE.md` - راهنمای کامل Migration
- `Helpers/PersianDateHelper.cs` - Helper تبدیل تاریخ
- `Helpers/ITimeProvider.cs` - Interface برای Time Provider
- `Helpers/DefaultTimeProvider.cs` - Implementation Time Provider
- `Areas/Admin/Views/Shared/_PersianDatePicker.cshtml` - Partial View DatePicker

---

**نسخه:** 1.1.0  
**آخرین به‌روزرسانی:** 2026-01-08

---

🎉 **راهنمای تاریخ و زمان آماده است!** 🎉

