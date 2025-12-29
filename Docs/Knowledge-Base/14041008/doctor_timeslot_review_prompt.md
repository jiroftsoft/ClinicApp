ر# 🏥 پرامپت جامع بررسی و بهینه‌سازی ماژول مدیریت اسلات‌های زمانی (DoctorTimeSlot)

**تاریخ:** 1404/10/08  
**وضعیت:** ✅ **آماده برای استفاده**  
**اولویت:** 🚨 **CRITICAL - ماژول مدیریت نوبت‌ها**

---

## 📋 نقش‌های شما (7 نقش همزمان)

شما یک تیم متخصص هستید که باید به صورت **همزمان** در نقش‌های زیر عمل کنید:

1. **معمار نرم‌افزار ارشد (Senior Software Architect)**: تمرکز بر ساختار، SOLID و Clean Architecture
2. **کد ریویوئر خبره (Expert Code Reviewer)**: تمرکز بر کیفیت کد، Code Smells و Naming Conventions
3. **متخصص ASP.NET MVC**: تمرکز بر MVC Patterns، Routing و Controller Best Practices
4. **متخصص امنیت (Security Expert)**: تمرکز بر OWASP Top 10، دسترسی‌ها و Input Validation
5. **متخصص سیستم‌های پزشکی (Medical Systems Expert)**: تمرکز بر استانداردهای درمانی، Audit Trail و Data Privacy
6. **متخصص تجربه کاربری (UX Expert)**: تمرکز بر Usability، Accessibility و استانداردهای بصری
7. **متخصص پایگاه داده (Database Expert)**: تمرکز بر Performance کوئری‌ها و مدیریت تراکنش‌ها

---

## 🛡️ مرحله 0: AI Guard Check (الزامی)

قبل از شروع بررسی، مطمئن شوید:

### 📚 مطالعه الزامی قراردادها:
1. **`Docs/Knowledge-Base/AI_ASSISTANT_MASTER_CONTRACT.md`** 🎯
   > خلاصه کامل تمام نقش‌ها، قراردادها و استانداردها
2. **`Docs/DEVELOPMENT_CONTRACT.md`** ⚡
   > استانداردهای UI/UX، Strongly-Typed، Bulletproof Coding، SRP
3. **`Docs/Knowledge-Base/03-Development-Contract-Quick-Guide.md`** 📋
   > راهنمای سریع قرارداد توسعه
4. **`Docs/Knowledge-Base/05-Debugging-Specialist-Contract.md`** 🔧
   > فرآیند 6 مرحله‌ای دیباگ
5. **`Docs/Knowledge-Base/CRITICAL-FINANCIAL-MODULE-CONTRACT.md`** 💰
   > در صورت وجود عملیات مالی

### 🔍 بررسی‌های امنیتی:
- [ ] **Data Loss Prevention**: آیا بررسی باعث حذف نوبت‌های معتبر می‌شود؟ (باید از Soft Delete استفاده شود)
- [ ] **Privacy**: آیا داده‌های پزشک/بیمار در لاگ‌ها افشا می‌شوند؟ (باید PII را ماسک کنیم)
- [ ] **Medical Standards**: آیا اسلات‌ها با زمان‌های مسدود شده تداخل دارند؟
- [ ] **Security**: آیا `DoctorId` در برابر کاربر فعلی اعتبارسنجی می‌شود؟

> 🛑 **HARD STOP**: اگر هر یک از موارد بالا نقض شود، متوقف شوید و توضیح بخواهید.

---

## 🎯 هدف

بررسی دقیق و موشکافانه ماژول `DoctorTimeSlotController` (هم Backend و هم Frontend) جهت:
- ✅ انطباق کامل با `DEVELOPMENT_CONTRACT.md`
- ✅ رعایت تمام قراردادهای `AI_ASSISTANT_MASTER_CONTRACT.md`
- ✅ بهینه‌سازی Performance و UX
- ✅ اطمینان از امنیت و استانداردهای پزشکی

---

## 📂 فایل‌های هدف

### Backend:
- **Controller**: `/Areas/Admin/Controllers/DoctorTimeSlotController.cs`
- **Service**: `/Services/ClinicAdmin/DoctorTimeSlotService.cs`
- **Repository**: `/Repositories/ClinicAdmin/DoctorTimeSlotRepository.cs`
- **Interface**: `/Interfaces/ClinicAdmin/IDoctorTimeSlotService.cs`, `/Interfaces/ClinicAdmin/IDoctorTimeSlotRepository.cs`

### Frontend:
- **Views**: 
  - `/Areas/Admin/Views/DoctorTimeSlot/Index.cshtml`
  - `/Areas/Admin/Views/DoctorTimeSlot/Details.cshtml`
  - سایر View های مرتبط

### ViewModels:
- `/ViewModels/Admin/TimeSlotManagement/TimeSlotIndexViewModel.cs`
- `/ViewModels/Admin/TimeSlotManagement/TimeSlotDetailsViewModel.cs`
- `/ViewModels/Admin/TimeSlotManagement/TimeSlotFilterViewModel.cs`
- `/ViewModels/Admin/TimeSlotManagement/TimeSlotStatisticsViewModel.cs`

### Constants:
- `/Constants/DoctorTimeSlotConstants.cs` (در صورت وجود)

---

## 📝 چک‌لیست‌های بررسی جامع (Comprehensive Review Checklists)

### 1️⃣ معماری و Backend (Controller & Pattern)

#### Strongly-Typed Development:
- [ ] **ViewModels**: آیا تمام View ها از ViewModel استفاده می‌کنند؟ (استفاده از `ViewBag` ممنوع است مگر برای `Title`/`MetaDescription`)
- [ ] **Constants**: آیا از Magic Strings استفاده شده؟ (باید از Constants استفاده شود)
- [ ] **Type Safety**: آیا از `dynamic` یا `object` استفاده شده؟ (باید Strongly-Typed باشد)

#### View Resolution:
- [ ] **GetViewPath()**: آیا از `GetViewPath()` برای آدرس‌دهی View ها استفاده شده است؟ (طبق قرارداد CMS Base)
- [ ] **View Location**: آیا View ها در مسیر صحیح قرار دارند؟

#### Controller Architecture:
- [ ] **Thin Controller**: آیا Logic در Controller وجود دارد؟ (باید در Service باشد)
- [ ] **SRP**: آیا Controller فقط Routing و Orchestration دارد؟
- [ ] **Dependency Injection**: آیا تمام وابستگی‌ها از طریق Constructor تزریق شده‌اند؟
- [ ] **Async/Await**: آیا تمام متدها Async هستند و `ConfigureAwait(false)` رعایت شده؟ (در صورت نیاز)

#### Notification System:
- [ ] **NotificationHelper**: آیا از `NotificationHelper` برای پیام‌ها استفاده شده؟ (حذف `TempData` مستقیم)
- [ ] **Toastr**: آیا پیام‌ها با Toastr نمایش داده می‌شوند؟
- [ ] **SweetAlert2**: آیا برای Confirmations از SweetAlert2 استفاده شده؟ (نه `confirm()`)

#### Error Handling:
- [ ] **Try-Catch**: آیا تمام متدهای async دارای try-catch هستند؟
- [ ] **Logging**: آیا از `ILogger` برای لاگ‌گذاری استفاده شده؟ (نه `Debug.WriteLine`)
- [ ] **ServiceResult**: آیا از `ServiceResult` برای بازگشت نتیجه استفاده شده؟

---

### 2️⃣ رابط کاربری و Frontend (Views)

#### Medical Design Standards:
- [ ] **Color Palette**: آیا رنگ‌بندی طبق پالت پزشکی (`--medical-*`) است؟
- [ ] **Forbidden Colors**: آیا رنگ‌های جیغ (بنفش، صورتی، نارنجی تند) حذف شده‌اند؟
- [ ] **Gradients**: آیا گرادینت‌های فانتزی حذف شده‌اند؟
- [ ] **Font**: آیا از فونت Vazir یا IRANSansX استفاده شده است؟

#### Persian DatePicker:
- [ ] **DatePicker Usage**: آیا تمام فیلدهای تاریخ از `_PersianDatePicker` استفاده می‌کنند؟
- [ ] **datetime-local**: آیا `<input type="date">` یا `<input type="datetime-local">` حذف شده است؟ (مهم)
- [ ] **Script Loading**: آیا `_PersianDatePickerScript` لود شده است؟
- [ ] **ParseDateFromHiddenInput**: آیا در POST Action ها از `ParseDateFromHiddenInput` استفاده شده؟

#### CSS و Styling:
- [ ] **Inline CSS**: آیا `style` بلاک‌ها حذف و به فایل CSS منتقل شده‌اند؟
- [ ] **CSS Organization**: آیا CSS در فایل‌های جداگانه و سازمان‌یافته است؟
- [ ] **Responsive Design**: آیا تمام عناصر Responsive هستند؟

#### Notifications:
- [ ] **Bootstrap Alerts**: آیا Alert های Bootstrap حذف و با Toastr جایگزین شده‌اند؟
- [ ] **SweetAlert2**: آیا برای Confirmations (حذف/آزادسازی) از SweetAlert2 استفاده شده؟
- [ ] **Alert Removal**: آیا هیچ `alert()` یا `confirm()` وجود ندارد؟

#### Data Tables:
- [ ] **Responsive Tables**: آیا جداول Responsive هستند؟
- [ ] **Pagination**: آیا Pagination به درستی پیاده‌سازی شده است؟
- [ ] **Filtering**: آیا Filtering به درستی کار می‌کند؟

---

### 3️⃣ ورودی‌ها و پردازش داده (Input Processing)

#### Date Parsing:
- [ ] **ParseDateFromHiddenInput**: آیا در POST Action ها از `ParseDateFromHiddenInput` استفاده شده؟ (برای تاریخ شمسی)
- [ ] **Date Validation**: آیا تاریخ‌ها قبل از استفاده اعتبارسنجی می‌شوند؟

#### Validation:
- [ ] **ModelState**: آیا `ModelState.IsValid` چک می‌شود؟
- [ ] **FluentValidation**: آیا از FluentValidation استفاده شده است؟
- [ ] **Client-Side Validation**: آیا Validation در سمت کلاینت نیز انجام می‌شود؟

#### Null Safety:
- [ ] **Null Checks**: آیا ورودی‌ها قبل از پردازش Null Check می‌شوند؟
- [ ] **Null Coalescing**: آیا از `??` یا `?.` استفاده شده است؟

#### Security:
- [ ] **ValidateAntiForgeryToken**: آیا `[ValidateAntiForgeryToken]` روی تمام متدهای POST وجود دارد？
- [ ] **Input Sanitization**: آیا ورودی‌ها برای جلوگیری از XSS پاکسازی می‌شوند؟
- [ ] **SQL Injection**: آیا از Parameterized Queries استفاده شده است؟

---

### 4️⃣ امنیت و دسترسی (Security)

#### Authorization:
- [ ] **Authorize Attribute**: آیا دسترسی‌ها (`[Authorize]`) به درستی کنترل شده‌اند؟
- [ ] **Role-Based Access**: آیا Role-Based Access Control پیاده‌سازی شده است؟

#### IDOR Prevention:
- [ ] **IDOR Check**: آیا کاربر فقط به اسلات‌های مجاز خود دسترسی دارد؟ (چک `DoctorId` در صورت لزوم)
- [ ] **Resource Ownership**: آیا مالکیت منابع بررسی می‌شود؟

#### Data Privacy:
- [ ] **PII Masking**: آیا اطلاعات حساس (PII) در لاگ‌ها ماسک می‌شوند؟
- [ ] **Audit Trail**: آیا Audit Trail کامل برای تمام عملیات وجود دارد؟

---

### 5️⃣ پایگاه داده و Performance

#### Query Optimization:
- [ ] **N+1 Problem**: آیا N+1 Query Problem وجود دارد؟ (باید از `Include()` استفاده شود)
- [ ] **AsNoTracking**: آیا از `AsNoTracking()` برای Read-Only Query ها استفاده شده است؟
- [ ] **Pagination**: آیا Pagination در سطح Database انجام می‌شود؟ (نه در Memory)

#### Transaction Management:
- [ ] **Transactions**: آیا برای عملیات چند مرحله‌ای از Transaction استفاده شده است؟
- [ ] **SaveChanges**: آیا `SaveChanges()` در جای مناسب فراخوانی می‌شود؟

#### Soft Delete:
- [ ] **Soft Delete**: آیا از Soft Delete استفاده شده است؟ (نه Hard Delete)
- [ ] **IsDeleted Filter**: آیا فیلتر `IsDeleted` به درستی اعمال می‌شود؟

---

### 6️⃣ استانداردهای پزشکی (Medical Standards)

#### Data Integrity:
- [ ] **Appointment Conflicts**: آیا تداخل نوبت‌ها بررسی می‌شود؟
- [ ] **Blocked Times**: آیا زمان‌های مسدود شده (`ScheduleExceptions`) بررسی می‌شوند؟
- [ ] **Time Range Validation**: آیا بازه‌های زمانی به درستی اعتبارسنجی می‌شوند؟

#### Audit Trail:
- [ ] **CreatedAt/CreatedBy**: آیا `CreatedAt` و `CreatedBy` به درستی تنظیم می‌شوند؟
- [ ] **UpdatedAt/UpdatedBy**: آیا `UpdatedAt` و `UpdatedBy` به درستی به‌روزرسانی می‌شوند؟
- [ ] **Logging**: آیا تمام عملیات لاگ می‌شوند؟

---

## 🔬 فرآیند بررسی (Review Process)

### مرحله 1: شناسایی و دسته‌بندی (Identify & Categorize)
- **نوع مشکل**: Logic Error / Security Issue / Performance Issue / Code Smell
- **شدت**: Critical / Major / Minor / Enhancement
- **دامنه**: Controller / Service / Repository / View

### مرحله 2: تحلیل علت ریشه‌ای (Root Cause Analysis)
- **5 Whys**: استفاده از تکنیک 5 چرا برای یافتن علت اصلی
- **Dependency Analysis**: بررسی وابستگی‌ها و تاثیرات
- **Impact Assessment**: ارزیابی تاثیر تغییرات

### مرحله 3: ارائه راه‌حل (Solution Proposal)
- **Atomic Fixes**: ارائه رفع‌های اتمیک و مستقل
- **Code Samples**: ارائه نمونه کد اصلاح شده
- **Migration Path**: مسیر مهاجرت از کد فعلی به کد جدید

---

## 📊 خروجی مورد انتظار (Expected Output)

لطفاً گزارش خود را در قالب زیر ارائه دهید:

### 1. خلاصه اجرایی (Executive Summary)
- **امتیاز کلی**: (0 تا 100)
- **وضعیت کلی**: (Critical Issues / Major Issues / Minor Issues / Production Ready)
- **اولویت‌بندی**: لیست مشکلات بر اساس اولویت

### 2. مشکلات حیاتی (Critical Issues) 🔴
*(مواردی که باعث Hard Stop می‌شوند یا قرارداد را نقض می‌کنند)*

**قالب گزارش:**
```
### [مشکل 1]: [عنوان]
- **فایل**: `path/to/file.cs`
- **خط**: `line number`
- **نوع**: Security / Logic Error / Contract Violation
- **توضیح**: [توضیح کامل مشکل]
- **تأثیر**: [تأثیر بر سیستم]
- **راه‌حل**: [راه‌حل پیشنهادی]
- **کد فعلی**:
```csharp
// کد مشکل‌دار
```
- **کد اصلاح شده**:
```csharp
// کد اصلاح شده
```
```

### 3. مشکلات مهم (Major Issues) 🟡
*(مواردی که باید رفع شوند اما Hard Stop نیستند)*

### 4. مشکلات جزئی (Minor Issues) 🟢
*(بهبودهای پیشنهادی)*

### 5. پیشنهادات بهبود (Improvement Plan) 🛠️

برای هر فایل، لیست تغییرات را مشخص کنید:

#### Controller (`DoctorTimeSlotController.cs`)
- [ ] تغییر 1: [توضیح]
- [ ] تغییر 2: [توضیح]

#### Service (`DoctorTimeSlotService.cs`)
- [ ] تغییر 1: [توضیح]
- [ ] تغییر 2: [توضیح]

#### Repository (`DoctorTimeSlotRepository.cs`)
- [ ] تغییر 1: [توضیح]
- [ ] تغییر 2: [توضیح]

#### View (`Index.cshtml`)
- [ ] تغییر 1: [توضیح]
- [ ] تغییر 2: [توضیح]

### 6. نمونه کد اصلاح شده (Refactored Code Samples)

برای رفع مشکلات اصلی، نمونه کد استاندارد ارائه دهید:

#### مثال 1: استفاده صحیح از NotificationHelper
```csharp
// ❌ اشتباه
TempData["Error"] = "خطا در انجام عملیات";

// ✅ درست
NotificationHelper.SetError(TempData, "خطا در انجام عملیات", "خطا");
```

#### مثال 2: استفاده صحیح از Persian DatePicker
```razor
@* ❌ اشتباه *@
<input type="date" name="StartDate" />

@* ✅ درست *@
@{
    ViewBag.PersianDatePickerId = "startDatePicker";
    ViewBag.PersianDatePickerName = "StartDate";
    ViewBag.PersianDatePickerValue = Model.StartDate;
    ViewBag.PersianDatePickerLabel = "تاریخ شروع";
    ViewBag.PersianDatePickerPlaceholder = "تاریخ شروع";
    ViewBag.PersianDatePickerRequired = true;
}
@Html.Partial("_PersianDatePicker")
```

#### مثال 3: استفاده صحیح از Constants
```csharp
// ❌ اشتباه
var queryParam = Request.QueryString["doctorId"];

// ✅ درست
var queryParam = Request.QueryString[DoctorTimeSlotConstants.QueryParameters.DoctorId];
```

---

## 🚀 دستورالعمل برای هوش مصنوعی (Self-Check)

قبل از تولید پاسخ، مطمئن شوید:

### ✅ چک‌لیست قبل از پاسخ:
1. [ ] آیا تمام قراردادهای `AI_ASSISTANT_MASTER_CONTRACT.md` را مطالعه کرده‌اید؟
2. [ ] آیا تمام قراردادهای `DEVELOPMENT_CONTRACT.md` را در نظر گرفته‌اید؟
3. [ ] آیا فایل‌های Controller، Service، Repository و View را دیده و تحلیل کرده‌اید؟
4. [ ] آیا پیشنهادها عملی و با ذکر دقیق نام فایل و خط هستند؟
5. [ ] آیا نمونه کدهای اصلاح شده ارائه شده‌اند؟
6. [ ] آیا مشکلات بر اساس اولویت (Critical / Major / Minor) دسته‌بندی شده‌اند؟
7. [ ] آیا راه‌حل‌ها مطابق با قراردادهای پروژه هستند؟

### ❌ ممنوعیت‌ها:
- ❌ حدس زدن (No Assumption Rule)
- ❌ رفع کورکورانه
- ❌ تغییر بدون بررسی کامل
- ❌ نقض قراردادها

---

## 📚 مراجع و منابع

### قراردادهای الزامی:
- `Docs/Knowledge-Base/AI_ASSISTANT_MASTER_CONTRACT.md` 🎯
- `Docs/DEVELOPMENT_CONTRACT.md` ⚡
- `Docs/Knowledge-Base/03-Development-Contract-Quick-Guide.md` 📋
- `Docs/Knowledge-Base/05-Debugging-Specialist-Contract.md` 🔧
- `Docs/Knowledge-Base/CRITICAL-FINANCIAL-MODULE-CONTRACT.md` 💰 (در صورت نیاز)

### راهنماها:
- `Docs/Knowledge-Base/01-Helpers-DateTime.md` - Helper های تاریخ و زمان
- `Docs/Knowledge-Base/02-Helpers-Validation.md` - Helper های اعتبارسنجی
- `Docs/Knowledge-Base/08-MVC-Routing-Best-Practices.md` - بهترین روش‌های Routing
- `Docs/NOTIFICATION_SYSTEM_GUIDE.md` - راهنمای سیستم اعلان‌رسانی
- `Docs/PERSIAN_DATEPICKER_MODULE_GUIDE.md` - راهنمای تقویم شمسی

---

## ✅ تعهد AI Assistant

```
من به عنوان AI Assistant متعهد می‌شوم:

✅ رعایت تمام 7 نقش همزمان
✅ رعایت تمام قراردادهای Critical
✅ رعایت تمام استانداردهای UI/UX
✅ رعایت تمام قوانین معماری
✅ رعایت تمام Hard Stop Rules
✅ استفاده از Helpers موجود (نه تکرار)
✅ فرآیند 6 مرحله‌ای دیباگ
✅ Checklist نهایی قبل از Commit
✅ ❌ ممنوع رفع کورکورانه!
✅ ❌ ممنوع نقض قراردادها!
```

---

**نسخه:** 2.0.0  
**تاریخ:** 1404/10/08  
**وضعیت:** ✅ **آماده برای استفاده**  
**نگارنده:** AI Assistant Team

---

🎉 **این پرامپت آماده استفاده در Cursor برای بررسی جامع و بهینه‌سازی ماژول DoctorTimeSlot است!** 🎉
