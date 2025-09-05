# قرارداد تبعیت هوش مصنوعی از قوانین پروژه ClinicApp

## مقدمه
این قرارداد الزام‌آور برای تمامی تعاملات هوش مصنوعی با پروژه ClinicApp است. تمامی قوانین زیر باید بدون استثنا رعایت شوند.

---

## 1. قانون تغییرات اتمی (Atomic Changes Rule)

### اصول اجباری:
- **هر تغییر باید کوچک، مرحله‌ای و مستقل (Atomic) باشد**
- **حداکثر یک فایل یا یک متد در هر گام**
- **تغییرات گسترده یکجا ممنوع است**

### پیاده‌سازی:
```csharp
// ✅ صحیح - تغییر اتمی
public async Task<ActionResult> FixSpecificIssue()
{
    // یک تغییر کوچک و مستقل
    // قابل تست و برگشت
}

// ❌ نادرست - تغییر گسترده
public async Task<ActionResult> RefactorEntireModule()
{
    // تغییرات متعدد در یک متد
    // غیرقابل تست و برگشت
}
```

### مثال عملی:
```markdown
## تغییر اتمی مجاز:
- اضافه کردن یک متد جدید به یک کلاس
- تغییر یک property در ViewModel
- اضافه کردن یک validation rule

## تغییر غیرمجاز:
- بازنویسی کل Controller
- تغییر چندین فایل همزمان
- اضافه کردن چندین feature در یک commit
```

---

## 2. قانون بررسی قبل از ایجاد (Pre-Creation Verification Rule)

### اصول اجباری:
- **قبل از افزودن هر کلاس، متد یا ماژول جدید → باید کل پروژه جستجو شود**
- **اگر موجود بود → کد و مسیر آن ارائه شود و پیشنهاد بازاستفاده داده شود**
- **اگر موجود نبود → تنها با ارائه مدرک عدم وجود، ایجاد شود**

### فرآیند اجباری:
```markdown
### مرحله 1: جستجوی سراسری
1. جستجو در تمام Controllers
2. جستجو در تمام Services
3. جستجو در تمام Repositories
4. جستجو در تمام ViewModels
5. جستجو در تمام Views

### مرحله 2: ارائه مدرک
- اگر موجود است: نمایش مسیر + خط کد + پیشنهاد بازاستفاده
- اگر موجود نیست: مدرک عدم وجود + پیشنهاد ایجاد
```

### مثال عملی:
```csharp
// ✅ جستجو قبل از ایجاد
// جستجو: "GetDoctorScheduleAsync"
// نتیجه: موجود در DoctorScheduleService.cs خط 45
// پیشنهاد: بازاستفاده از متد موجود

// ✅ مدرک عدم وجود
// جستجو: "GetDoctorSpecializationsAsync"
// نتیجه: یافت نشد در 58 فایل
// پیشنهاد: ایجاد متد جدید
```

---

## 3. قانون ممنوعیت تکرار (No Duplication Rule)

### اصول اجباری:
- **ایجاد دوباره چیزی که قبلا وجود دارد ممنوع است**
- **تغییر یا توسعه باید روی نسخه‌ی موجود انجام گیرد**

### پیاده‌سازی:
```csharp
// ❌ ممنوع - ایجاد دوباره
public class DoctorServiceCategoryService
{
    // کلاس جدید با نام مشابه موجود
}

// ✅ مجاز - توسعه موجود
public class DoctorServiceCategoryService
{
    // اضافه کردن متد جدید به کلاس موجود
    public async Task<ServiceResult> NewMethod()
    {
        // پیاده‌سازی جدید
    }
}
```

---

## 4. قانون مستندسازی اجباری (Mandatory Documentation Rule)

### اصول اجباری:
هر پیشنهاد تغییر باید شامل:

1. **هدف تغییر** (What)
2. **دلیل تغییر** (Why - Business/Technical Justification)
3. **مسیر فایل + خط کدهای مرتبط** (Where)
4. **پیش‌نویس تغییر** (How - پچ یا کد)
5. **ریسک و راه بازگشت** (Risk & Rollback Strategy)

### قالب اجباری:
```markdown
## پیشنهاد تغییر

### هدف:
[توضیح دقیق هدف تغییر]

### دلیل:
- Business: [دلیل کسب‌وکار]
- Technical: [دلیل فنی]

### مکان:
- فایل: [مسیر کامل]
- خط: [شماره خط]
- کد موجود: [کد فعلی]

### پیش‌نویس:
```csharp
// کد پیشنهادی
```

### ریسک:
- سطح: [کم/متوسط/بالا]
- توضیح: [شرح ریسک]

### راه بازگشت:
- روش: [git revert/دستی/...]
- مراحل: [مراحل دقیق]
```

---

## 5. قانون توقف و تایید (Stop and Approval Rule)

### اصول اجباری:
- **پس از ارائه هر گام، هوش مصنوعی باید توقف کند و منتظر تایید انسان بماند**
- **بدون دستور صریح من: "I APPROVE APPLY" هیچ تغییری نباید اعمال شود**

### فرآیند اجباری:
```markdown
1. ارائه پیشنهاد تغییر
2. انتظار برای تایید انسان
3. دریافت دستور "I APPROVE APPLY"
4. اعمال تغییر
5. ارائه گزارش نتیجه
6. انتظار برای گام بعدی
```

### مثال:
```markdown
## پیشنهاد تغییر ارائه شد
⏳ منتظر تایید شما...

[پس از بررسی]
✅ I APPROVE APPLY

[اعمال تغییر]
✅ تغییر اعمال شد
📊 گزارش نتیجه: [جزئیات]
```

---

## 6. قانون امنیت و کیفیت (Security and Quality Rule)

### اصول اجباری:
- **همه تغییرات باید با رعایت اصول SOLID و Clean Architecture انجام شود**
- **امنیت (SQL Injection, XSS, CSRF, IDOR) باید در اولویت بررسی باشد**
- **عملکرد و بهینه‌سازی دیتابیس باید بررسی گردد**

### چک‌لیست امنیت:
```markdown
- [ ] SQL Injection: استفاده از Parameterized Queries
- [ ] XSS: اعتبارسنجی ورودی و HTML Encoding
- [ ] CSRF: Anti-Forgery Token در تمام Forms
- [ ] IDOR: بررسی مجوز دسترسی
- [ ] Input Validation: اعتبارسنجی تمام ورودی‌ها
- [ ] Output Encoding: کدگذاری خروجی
```

### چک‌لیست کیفیت:
```markdown
- [ ] SOLID Principles: رعایت اصول SOLID
- [ ] Clean Architecture: معماری تمیز
- [ ] Performance: بهینه‌سازی عملکرد
- [ ] Error Handling: مدیریت خطا
- [ ] Logging: لاگ‌گیری مناسب
- [ ] Testing: قابلیت تست
```

---

## 7. قانون خروجی شفاف (Transparent Output Rule)

### اصول اجباری:
- **خروجی باید به صورت JSON ساختارمند یا متن دقیق، شامل مدارک و شواهد کدی باشد**
- **هیچ تغییر یا پیشنهاد مبهم یا بدون مدرک پذیرفته نیست**

### قالب خروجی اجباری:
```json
{
  "تغییرات_اتمی": [
    {
      "شناسه": "step-1",
      "عنوان": "عنوان کوتاه",
      "توجیه": "چرایی نیاز",
      "مدرک": [
        {
          "مسیر": "path/to/file.cs",
          "خط": 45,
          "کد": "کد موجود"
        }
      ],
      "پچ": {
        "عملیات": "add|modify|remove",
        "مسیر_فایل": "relative/path/to/file",
        "کد_تغییر": "unified diff یا کد پیشنهادی"
      },
      "ریسک": "خلاصه کوتاه",
      "بازگشت": "git revert یا روش دستی",
      "اطمینان": "high|medium|low"
    }
  ]
}
```

---

## 8. قانون ممنوعیت اجرای خودکار (No Auto-Execution Rule)

### اصول اجباری:
- **هوش مصنوعی حق ندارد تغییرات را به صورت خودکار Merge یا Deploy کند**
- **تنها نقش آن پیشنهاددهنده و تحلیل‌گر است**

### محدودیت‌های رفتاری:
```markdown
❌ ممنوع:
- اجرای خودکار git commit
- اجرای خودکار git push
- اجرای خودکار deployment
- تغییر خودکار فایل‌ها بدون تایید

✅ مجاز:
- ارائه پیشنهاد
- تحلیل کد
- ارائه راه‌حل
- مستندسازی
```

---

## 9. قانون پایبندی به پروژه (Project Scope Rule)

### اصول اجباری:
- **تمرکز صرفا روی پروژه ClinicApp است**
- **پیشنهادات خارج از محدوده این پروژه ممنوع است مگر با تایید من**

### محدوده پروژه:
```markdown
✅ مجاز:
- بهبود ماژول‌های موجود ClinicApp
- رفع باگ‌های ClinicApp
- بهینه‌سازی عملکرد ClinicApp
- اضافه کردن feature های مرتبط با ClinicApp

❌ ممنوع:
- پیشنهاد تغییر framework
- پیشنهاد migration به تکنولوژی دیگر
- پیشنهادات خارج از scope پزشکی
- تغییرات غیرمرتبط با ClinicApp
```

---

## 10. قانون الزام‌آوری (Mandatory Compliance Rule)

### اصول اجباری:
- **این بندها مثل یک قرارداد الزام‌آور هستند**
- **هوش مصنوعی موظف است در تمام مراحل کار طبق این قوانین رفتار کند**

### پیامدهای عدم رعایت:
```markdown
⚠️ هشدار: عدم رعایت هر یک از قوانین بالا منجر به:
- توقف فوری فرآیند
- بازنگری کامل پیشنهاد
- ارائه مجدد با رعایت قوانین
- مستندسازی خطا در knowledge base
```

---

## چک‌لیست رعایت قوانین

### قبل از هر پیشنهاد:
- [ ] تغییر اتمی و کوچک است
- [ ] جستجوی سراسری انجام شده
- [ ] مدرک عدم وجود ارائه شده
- [ ] مستندسازی کامل انجام شده
- [ ] ریسک و راه بازگشت مشخص شده

### قبل از اعمال تغییر:
- [ ] تایید صریح "I APPROVE APPLY" دریافت شده
- [ ] اصول امنیت رعایت شده
- [ ] اصول SOLID رعایت شده
- [ ] خروجی شفاف و مستند شده
- [ ] محدوده پروژه رعایت شده

### پس از اعمال تغییر:
- [ ] گزارش نتیجه ارائه شده
- [ ] Knowledge base به‌روزرسانی شده
- [ ] چک‌لیست رعایت قوانین تکمیل شده
- [ ] آماده برای گام بعدی

---

## مثال کامل رعایت قوانین

```markdown
## پیشنهاد تغییر اتمی

### هدف:
اضافه کردن Unique Index برای جلوگیری از تکرار صلاحیت‌های خدماتی

### دلیل:
- Business: جلوگیری از خطاهای کاربری و تضمین یکپارچگی داده
- Technical: رعایت اصول Database Design و جلوگیری از Data Corruption

### مکان:
- فایل: Models/Entities/Clinic.cs
- خط: 5140-5170
- کد موجود: DoctorServiceCategory entity بدون unique constraint

### مدرک عدم وجود:
```csharp
// جستجو در کل پروژه: "IX_DoctorServiceCategory_DoctorId_ServiceCategoryId"
// نتیجه: یافت نشد در 58 فایل
// نتیجه: یافت نشد در Migrations
```

### پیش‌نویس:
```csharp
// Migration: AddUniqueIndexDoctorServiceCategory.cs
public partial class AddUniqueIndexDoctorServiceCategory : DbMigration
{
    public override void Up()
    {
        CreateIndex("dbo.DoctorServiceCategories", 
            new[] { "DoctorId", "ServiceCategoryId" }, 
            unique: true, 
            name: "IX_DoctorServiceCategory_DoctorId_ServiceCategoryId");
    }
    
    public override void Down()
    {
        DropIndex("dbo.DoctorServiceCategories", "IX_DoctorServiceCategory_DoctorId_ServiceCategoryId");
    }
}
```

### ریسک:
- سطح: متوسط
- توضیح: نیاز به Migration و ممکن است داده‌های تکراری موجود باشد

### راه بازگشت:
- روش: Migration rollback
- مراحل: 
  1. `Update-Database -TargetMigration:PreviousMigration`
  2. حذف فایل Migration
  3. بررسی داده‌های تکراری

### اطمینان: high

⏳ منتظر تایید شما برای اعمال این تغییر...
```

---

## 11. قانون بررسی قبل از اقدام (Pre-Action Verification Rule)

### اصول اجباری:
- **قبل از هرگونه تغییر یا اضافه‌کردن، کل پروژه و ماژول مرتبط را بررسی کند**
- **از وجود منطق یا کلاس مشابه مطمئن شود**
- **جستجوی کامل در تمام لایه‌ها (Controller, Service, Repository, ViewModel, View)**

### پیاده‌سازی:
```csharp
// ✅ صحیح - بررسی کامل قبل از ایجاد
// 1. جستجو در کل پروژه
// 2. بررسی وجود منطق مشابه
// 3. سپس ایجاد یا بازاستفاده

// ❌ نادرست - ایجاد بدون بررسی
// ایجاد مستقیم بدون جستجو
```

---

## 12. قانون گام‌های کوچک و شفاف (Small Steps Rule)

### اصول اجباری:
- **هیچ‌گاه تغییرات بزرگ و همزمان ایجاد نکند**
- **همه پیشنهادات باید در گام‌های کوچک، واضح و قابل تست ارائه شود**
- **هر گام باید مستقل و قابل برگشت باشد**

### پیاده‌سازی:
```csharp
// ✅ صحیح - گام کوچک
public async Task<ActionResult> Step1_FixValidation()
{
    // فقط یک تغییر کوچک
}

// ❌ نادرست - تغییرات بزرگ
public async Task<ActionResult> CompleteRefactor()
{
    // تغییرات متعدد همزمان
}
```

---

## 13. قانون پرهیز از تکرار (Anti-Duplication Rule)

### اصول اجباری:
- **قبل از ایجاد متد، کلاس یا ماژول جدید، باید بررسی کند که نمونه مشابه در پروژه وجود ندارد**
- **ترجیح بازاستفاده و بازآرایی به ایجاد جدید**
- **حذف تکرارهای موجود**

### پیاده‌سازی:
```csharp
// ✅ صحیح - بازاستفاده
var existingMethod = await _service.GetExistingData();
// استفاده از متد موجود

// ❌ نادرست - ایجاد تکراری
public async Task<Data> GetDataAgain() // تکراری
{
    // منطق مشابه
}
```

---

## 14. قانون استانداردهای معماری (Architecture Standards Rule)

### اصول اجباری:
- **کلیه تغییرات باید مطابق اصول SOLID باشند**
- **رعایت لایه‌بندی (Repository, Service, Controller)**
- **معماری MVC/EF صحیح**

### پیاده‌سازی:
```csharp
// ✅ صحیح - معماری صحیح
[Controller] → [Service] → [Repository] → [Entity]

// ❌ نادرست - نقض معماری
[Controller] → [Entity] // بدون لایه Service
```

---

## 15. قانون هماهنگی با مدل‌های دیتابیس (Database Model Coordination Rule)

### اصول اجباری:
- **هیچ تغییری در مدل‌های دیتابیس بدون بررسی وابستگی‌ها، Context و Migrations اعمال نشود**
- **بررسی کامل تأثیرات تغییرات دیتابیس**
- **هماهنگی با Entity Framework**

### پیاده‌سازی:
```csharp
// ✅ صحیح - بررسی کامل
// 1. بررسی وابستگی‌ها
// 2. بررسی Context
// 3. بررسی Migrations
// 4. سپس تغییر

// ❌ نادرست - تغییر مستقیم
// تغییر Entity بدون بررسی
```

---

## 16. قانون رعایت نام‌گذاری رسمی (Formal Naming Rule)

### اصول اجباری:
- **تمامی نام‌ها (کلاس‌ها، متدها، ViewModelها) باید رسمی، معنادار و مرتبط با حوزه درمانی باشند**
- **استفاده از نام‌های فارسی و انگلیسی مناسب**
- **پرهیز از نام‌های غیررسمی یا فانتزی**

### پیاده‌سازی:
```csharp
// ✅ صحیح - نام‌گذاری رسمی
public class DoctorServiceCategoryViewModel
public async Task<ActionResult> GetPatientMedicalHistory()

// ❌ نادرست - نام‌گذاری غیررسمی
public class DocStuff
public async Task<ActionResult> GetPatientData()
```

---

## 17. قانون استاندارد UI/UX درمانی (Medical UI/UX Standards Rule)

### اصول اجباری:
- **خروجی Razor Pages باید رسمی، ساده، کاربردی و متناسب با محیط درمانی باشد**
- **بدون المان‌های اضافی یا فانتزی**
- **رعایت استانداردهای فرم‌های رسمی**

### پیاده‌سازی:
```html
<!-- ✅ صحیح - UI رسمی -->
<div class="card">
    <h3 class="card-title">جزئیات بیمار</h3>
    <form class="medical-form">
        <!-- فرم رسمی -->
    </form>
</div>

<!-- ❌ نادرست - UI غیررسمی -->
<div class="fancy-card">
    <h3>Patient Info 🏥</h3>
    <!-- المان‌های فانتزی -->
</div>
```

---

## 18. قانون اعتبارسنجی داده‌ها (Data Validation Rule)

### اصول اجباری:
- **همه فرم‌ها و ورودی‌ها باید دارای Validation سمت سرور و کلاینت باشند**
- **پیام‌های فارسی و رسمی**
- **اعتبارسنجی جامع و امن**

### پیاده‌سازی:
```csharp
// ✅ صحیح - Validation جامع
[Required(ErrorMessage = "لطفاً نام بیمار را وارد کنید")]
[StringLength(100, ErrorMessage = "نام بیمار نمی‌تواند بیش از 100 کاراکتر باشد")]
public string PatientName { get; set; }

// ❌ نادرست - Validation ناقص
public string PatientName { get; set; } // بدون Validation
```

---

## 19. قانون امنیت (Security Rule)

### اصول اجباری:
- **همه اکشن‌ها باید CSRF Token داشته باشند**
- **کوئری‌ها ضد SQL Injection باشند**
- **نقش‌ها/مجوزها بررسی شوند**

### پیاده‌سازی:
```csharp
// ✅ صحیح - امنیت کامل
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin")]
public async Task<ActionResult> UpdatePatient(PatientViewModel model)
{
    // استفاده از Parameterized Queries
    // بررسی مجوزها
}

// ❌ نادرست - امنیت ناقص
[HttpPost]
public async Task<ActionResult> UpdatePatient(PatientViewModel model)
{
    // بدون CSRF Token
    // بدون بررسی مجوز
}
```

---

## 20. قانون مستندسازی اجباری (Mandatory Documentation Rule)

### اصول اجباری:
- **هر تغییر باید با توضیح و Documentation همراه باشد**
- **کامنت در کد + تحلیل در گزارش**
- **مستندسازی کامل و شفاف**

### پیاده‌سازی:
```csharp
/// <summary>
/// دریافت صلاحیت خدماتی بر اساس AssignmentId
/// طبق DESIGN_PRINCIPLES_CONTRACT: لاگ‌گیری جامع برای محیط پزشکی
/// </summary>
/// <param name="assignmentId">شناسه انتساب (فرمت: DoctorId_ServiceCategoryId)</param>
/// <returns>ViewModel صلاحیت خدماتی</returns>
private async Task<DoctorServiceCategoryViewModel> GetServiceCategoryByAssignmentIdAsync(string assignmentId)
{
    // پیاده‌سازی
}
```

---

## 21. قانون عدم حذف کد موجود (No Deletion Without Approval Rule)

### اصول اجباری:
- **AI اجازه حذف یا تغییر کدهای موجود را ندارد مگر با تأیید کاربر**
- **حفظ کدهای موجود و اضافه کردن بهبودها**
- **تأیید صریح برای هر حذف**

### پیاده‌سازی:
```csharp
// ✅ صحیح - حفظ کد موجود
// کد موجود حفظ می‌شود
// بهبودها اضافه می‌شوند

// ❌ نادرست - حذف بدون تأیید
// حذف کد موجود بدون تأیید کاربر
```

---

## 22. قانون سازگاری با ساختار Bundle و Config (Bundle Config Compatibility Rule)

### اصول اجباری:
- **هر اسکریپت یا استایل جدید باید در BundleConfig یا بخش App_Start به درستی اضافه شود**
- **سازگاری با ساختار موجود**
- **رعایت الگوهای Bundle**

### پیاده‌سازی:
```csharp
// ✅ صحیح - اضافه کردن به Bundle
bundles.Add(new ScriptBundle("~/bundles/medical").Include(
    "~/Scripts/Medical/PatientManagement.js",
    "~/Scripts/Medical/DoctorSchedule.js"
));

// ❌ نادرست - اضافه کردن مستقیم
// اضافه کردن Script بدون Bundle
```

---

## 23. قانون پرهیز از پیچیدگی (Simplicity Rule)

### اصول اجباری:
- **کدها باید ساده، قابل‌فهم و Maintainable باشند**
- **از ایجاد منطق بیش‌ازحد پیچیده یا وابستگی‌های غیرضروری پرهیز شود**
- **خوانایی و نگهداری آسان**

### پیاده‌سازی:
```csharp
// ✅ صحیح - کد ساده
public async Task<PatientViewModel> GetPatient(int id)
{
    return await _patientService.GetByIdAsync(id);
}

// ❌ نادرست - کد پیچیده
public async Task<PatientViewModel> GetPatient(int id)
{
    // منطق پیچیده و غیرضروری
    // وابستگی‌های متعدد
}
```

---

## 24. قانون گزارش نهایی هر گام (Final Report Rule)

### اصول اجباری:
- **پس از هر تغییر یا پیاده‌سازی، AI موظف است گزارش تحلیلی شامل:**
  - **چه تغییر کرد**
  - **چرا تغییر کرد**
  - **تأثیر آن بر سایر بخش‌ها**
  - **گام بعدی پیشنهادی**

### پیاده‌سازی:
```markdown
## گزارش تغییر:
### چه تغییر کرد:
- اضافه شدن متد GetServiceCategoryByAssignmentIdAsync

### چرا تغییر کرد:
- نیاز به دریافت صلاحیت بر اساس AssignmentId

### تأثیر بر سایر بخش‌ها:
- بهبود عملکرد Details و Edit actions

### گام بعدی:
- بررسی و بهینه‌سازی سایر متدها
```

---

## خلاصه قوانین اجباری (به‌روزرسانی شده)

### ✅ الزامات اصلی (24 قانون):
1. **تغییرات اتمی** - کوچک، مرحله‌ای، مستقل
2. **بررسی قبل از ایجاد** - جستجوی کامل پروژه
3. **ممنوعیت تکرار** - بازاستفاده از موجودات
4. **مستندسازی اجباری** - توضیح کامل هر تغییر
5. **توقف و انتظار تایید** - بدون تایید انسان هیچ تغییری اعمال نشود
6. **امنیت و کیفیت** - رعایت SOLID، Clean Architecture، امنیت
7. **خروجی شفاف** - JSON یا متن دقیق با مدرک کد
8. **عدم اجرای خودکار** - AI فقط پیشنهاددهنده/تحلیلگر
9. **تمرکز بر ClinicApp** - فقط پروژه ClinicApp
10. **رعایت اجباری** - تمام قوانین بدون استثنا
11. **بررسی قبل از اقدام** - جستجوی کامل قبل از هر تغییر
12. **گام‌های کوچک و شفاف** - تغییرات مرحله‌ای و قابل تست
13. **پرهیز از تکرار** - بررسی وجود قبل از ایجاد
14. **استانداردهای معماری** - رعایت SOLID و لایه‌بندی
15. **هماهنگی با مدل‌های دیتابیس** - بررسی وابستگی‌ها
16. **رعایت نام‌گذاری رسمی** - نام‌های معنادار و درمانی
17. **استاندارد UI/UX درمانی** - فرم‌های رسمی و ساده
18. **اعتبارسنجی داده‌ها** - Validation جامع
19. **امنیت** - CSRF، SQL Injection، مجوزها
20. **مستندسازی اجباری** - کامنت و تحلیل
21. **عدم حذف کد موجود** - بدون تأیید کاربر
22. **سازگاری با Bundle و Config** - رعایت ساختار
23. **پرهیز از پیچیدگی** - کد ساده و قابل نگهداری
24. **گزارش نهایی هر گام** - تحلیل کامل تغییرات

### ❌ ممنوعیت‌ها:
- تغییرات گسترده یکجا
- ایجاد بدون بررسی
- تکرار موجودات
- تغییر بدون مستندسازی
- اجرای خودکار
- تمرکز بر پروژه‌های دیگر
- تخطی از قوانین
- حذف کد بدون تأیید
- پیچیدگی غیرضروری
- عدم گزارش تغییرات

---

## قرارداد تخصصی کنترلرها در پروژه کلینیک (Controller Specialized Contract)

### مقدمه
این قرارداد تخصصی برای تمامی کنترلرهای پروژه ClinicApp الزام‌آور است و باید در کنار قوانین اصلی AI_COMPLIANCE_CONTRACT رعایت شود.

---

## 25. قانون اصل SRP برای کنترلرها (Single Responsibility Principle for Controllers)

### اصول اجباری:
- **هر کنترلر فقط یک نقش مشخص داشته باشد** (مثل CRUD پزشک یا مدیریت کلینیک)
- **منطق اضافه به سرویس‌ها منتقل شود**
- **پرهیز از کنترلرهای بزرگ و پیچیده**

### پیاده‌سازی:
```csharp
// ✅ صحیح - کنترلر با مسئولیت واحد
public class DoctorController : Controller
{
    // فقط عملیات مربوط به پزشک
    public async Task<ActionResult> Index() { }
    public async Task<ActionResult> Create() { }
    public async Task<ActionResult> Edit(int id) { }
}

// ❌ نادرست - کنترلر با مسئولیت‌های متعدد
public class ClinicManagementController : Controller
{
    // عملیات پزشک + بیمار + دپارتمان + ...
}
```

---

## 26. قانون جلوگیری از تکرار در کنترلرها (Anti-Duplication in Controllers)

### اصول اجباری:
- **پیش از اضافه کردن Action یا متد جدید، بررسی شود که آیا مشابه آن وجود دارد یا خیر**
- **از ایجاد دوباره منطق یا اکشن پرهیز شود**
- **بازاستفاده از اکشن‌های موجود**

### پیاده‌سازی:
```csharp
// ✅ صحیح - بازاستفاده از اکشن موجود
[HttpGet]
public async Task<ActionResult> GetDoctorDetails(int id)
{
    return await GetDoctorById(id); // استفاده از متد موجود
}

// ❌ نادرست - ایجاد اکشن تکراری
[HttpGet]
public async Task<ActionResult> GetDoctorInfo(int id) // تکراری
{
    // منطق مشابه GetDoctorDetails
}
```

---

## 27. قانون ورودی و خروجی ایمن (Safe Input/Output)

### اصول اجباری:
- **همه متدها باید ورودی‌ها را اعتبارسنجی کنند**
- **خروجی‌ها را در قالب ViewModel یا JsonResult بازگردانند**
- **نه مدل دیتابیس خام**

### پیاده‌سازی:
```csharp
// ✅ صحیح - ورودی و خروجی ایمن
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> CreateDoctor(DoctorViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    
    var result = await _doctorService.CreateAsync(model);
    return Json(new { success = result.Success, message = result.Message });
}

// ❌ نادرست - خروجی ناامن
[HttpPost]
public async Task<ActionResult> CreateDoctor(Doctor entity)
{
    // بازگرداندن Entity خام
    return Json(entity); // خطرناک
}
```

---

## 28. قانون Async/Await اجباری (Mandatory Async/Await)

### اصول اجباری:
- **تمام عملیات I/O (دیتابیس، سرویس‌ها) باید غیرهمزمان باشند**
- **استفاده از async/await در همه جا**
- **پرهیز از عملیات همزمان**

### پیاده‌سازی:
```csharp
// ✅ صحیح - Async/Await
[HttpGet]
public async Task<ActionResult> Index()
{
    var doctors = await _doctorService.GetAllAsync();
    return View(doctors);
}

// ❌ نادرست - عملیات همزمان
[HttpGet]
public ActionResult Index()
{
    var doctors = _doctorService.GetAll(); // همزمان
    return View(doctors);
}
```

---

## 29. قانون Logging با Serilog (Serilog Logging Rule)

### اصول اجباری:
- **همه اکشن‌ها باید Log داشته باشند**
- **شامل UserId، IP و جزئیات درخواست/پاسخ**
- **لاگ‌گیری جامع برای محیط پزشکی**

### پیاده‌سازی:
```csharp
// ✅ صحیح - Logging جامع
[HttpGet]
public async Task<ActionResult> GetPatient(int id)
{
    try
    {
        _logger.Information("درخواست دریافت بیمار {PatientId} توسط کاربر {UserId}, IP: {IPAddress}", 
            id, _currentUserService.UserId, GetClientIPAddress());
        
        var patient = await _patientService.GetByIdAsync(id);
        
        _logger.Information("بیمار {PatientId} با موفقیت دریافت شد. کاربر: {UserId}", 
            id, _currentUserService.UserId);
        
        return View(patient);
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در دریافت بیمار {PatientId}. کاربر: {UserId}", 
            id, _currentUserService.UserId);
        return View("Error");
    }
}
```

---

## 30. قانون Error Handling (Error Handling Rule)

### اصول اجباری:
- **تمام متدها باید مدیریت خطا (try/catch) داشته باشند**
- **نمایش پیام مناسب به کاربر**
- **لاگ‌گیری خطاها**

### پیاده‌سازی:
```csharp
// ✅ صحیح - Error Handling جامع
[HttpPost]
public async Task<ActionResult> UpdateDoctor(DoctorViewModel model)
{
    try
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "لطفاً تمام فیلدهای اجباری را پر کنید.";
            return View(model);
        }
        
        var result = await _doctorService.UpdateAsync(model);
        
        if (result.Success)
        {
            TempData["Success"] = "اطلاعات پزشک با موفقیت به‌روزرسانی شد.";
            return RedirectToAction("Index");
        }
        else
        {
            TempData["Error"] = result.Message;
            return View(model);
        }
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در به‌روزرسانی پزشک {DoctorId}", model.Id);
        TempData["Error"] = "خطا در به‌روزرسانی اطلاعات پزشک.";
        return View(model);
    }
}
```

---

## 31. قانون CSRF Protection (CSRF Protection Rule)

### اصول اجباری:
- **همه متدهای POST باید [ValidateAntiForgeryToken] داشته باشند**
- **حفاظت در برابر حملات CSRF**
- **امنیت کامل فرم‌ها**

### پیاده‌سازی:
```csharp
// ✅ صحیح - CSRF Protection
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin")]
public async Task<ActionResult> CreateDoctor(DoctorViewModel model)
{
    // پیاده‌سازی
}

// ❌ نادرست - بدون CSRF Protection
[HttpPost]
public async Task<ActionResult> CreateDoctor(DoctorViewModel model)
{
    // خطرناک - بدون حفاظت CSRF
}
```

---

## 32. قانون Authorization (Authorization Rule)

### اصول اجباری:
- **کنترلرها باید با [Authorize(Roles=...)] محدود شوند**
- **هر اکشن باید سطح دسترسی متناسب داشته باشد**
- **بررسی مجوزها در همه جا**

### پیاده‌سازی:
```csharp
// ✅ صحیح - Authorization مناسب
[Authorize(Roles = "Admin,Doctor")]
public class DoctorController : Controller
{
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Create() { }
    
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<ActionResult> View(int id) { }
}

// ❌ نادرست - بدون Authorization
public class DoctorController : Controller
{
    public async Task<ActionResult> Create() { } // بدون مجوز
}
```

---

## 33. قانون Validation قبل از سرویس (Pre-Service Validation Rule)

### اصول اجباری:
- **قبل از فراخوانی سرویس‌ها، مدل‌ها با FluentValidation یا ModelState بررسی شوند**
- **اعتبارسنجی کامل ورودی‌ها**
- **پرهیز از ارسال داده‌های نامعتبر به سرویس**

### پیاده‌سازی:
```csharp
// ✅ صحیح - Validation قبل از سرویس
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> CreatePatient(PatientViewModel model)
{
    if (!ModelState.IsValid)
    {
        TempData["Error"] = "لطفاً تمام فیلدهای اجباری را پر کنید.";
        return View(model);
    }
    
    // اعتبارسنجی اضافی
    if (model.Age < 0 || model.Age > 150)
    {
        ModelState.AddModelError("Age", "سن باید بین 0 تا 150 سال باشد.");
        return View(model);
    }
    
    var result = await _patientService.CreateAsync(model);
    // ادامه...
}
```

---

## 34. قانون Prevent Overposting (Prevent Overposting Rule)

### اصول اجباری:
- **فقط مقادیر مجاز از فرم‌ها Bind شوند**
- **با ViewModel یا [Bind]**
- **حفاظت در برابر Over-posting attacks**

### پیاده‌سازی:
```csharp
// ✅ صحیح - Prevent Overposting
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> UpdateDoctor([Bind(Include = "Id,FirstName,LastName,Specialty")] DoctorViewModel model)
{
    // فقط فیلدهای مجاز Bind می‌شوند
}

// یا با ViewModel
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> UpdateDoctor(DoctorUpdateViewModel model)
{
    // ViewModel فقط فیلدهای مجاز دارد
}
```

---

## 35. قانون AJAX Actions (AJAX Actions Rule)

### اصول اجباری:
- **اکشن‌های Ajax باید همیشه JsonResult با ساختار استاندارد بازگردانند**
- **ساختار: { success, message, data }**
- **پاسخ‌های یکپارچه و قابل پیش‌بینی**

### پیاده‌سازی:
```csharp
// ✅ صحیح - AJAX با ساختار استاندارد
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<JsonResult> DeleteDoctor(int id)
{
    try
    {
        var result = await _doctorService.DeleteAsync(id);
        
        if (result.Success)
        {
            return Json(new { 
                success = true, 
                message = "پزشک با موفقیت حذف شد.",
                data = new { deletedId = id }
            });
        }
        else
        {
            return Json(new { 
                success = false, 
                message = result.Message,
                data = null
            });
        }
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در حذف پزشک {DoctorId}", id);
        return Json(new { 
            success = false, 
            message = "خطا در حذف پزشک.",
            data = null
        });
    }
}
```

---

## 36. قانون پرهیز از منطق تجاری در کنترلر (No Business Logic in Controllers)

### اصول اجباری:
- **منطق بیزینسی نباید در کنترلر نوشته شود**
- **باید به سرویس‌ها منتقل گردد**
- **کنترلر فقط واسط کاربری باشد**

### پیاده‌سازی:
```csharp
// ✅ صحیح - بدون منطق تجاری
[HttpPost]
public async Task<ActionResult> CreateAppointment(AppointmentViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    
    var result = await _appointmentService.CreateAsync(model);
    // منطق تجاری در سرویس
}

// ❌ نادرست - منطق تجاری در کنترلر
[HttpPost]
public async Task<ActionResult> CreateAppointment(AppointmentViewModel model)
{
    // منطق تجاری در کنترلر
    if (model.AppointmentDate < DateTime.Now)
    {
        // بررسی تجاری در کنترلر - نادرست
    }
    
    // محاسبات تجاری در کنترلر - نادرست
    var duration = CalculateDuration(model);
}
```

---

## 37. قانون Performance در محیط درمانی (Medical Environment Performance Rule)

### اصول اجباری:
- **Cache روی داده‌های حساس پزشکی غیرفعال باشد**
- **OutputCache = 0 برای داده‌های پزشکی**
- **اطلاعات پزشکی همیشه به‌روز**

### پیاده‌سازی:
```csharp
// ✅ صحیح - بدون Cache برای داده‌های پزشکی
[HttpGet]
[OutputCache(Duration = 0, NoStore = true)] // غیرفعال
public async Task<ActionResult> GetPatientMedicalHistory(int patientId)
{
    // داده‌های پزشکی همیشه تازه
}

// ❌ نادرست - Cache برای داده‌های پزشکی
[HttpGet]
[OutputCache(Duration = 3600)] // خطرناک
public async Task<ActionResult> GetPatientMedicalHistory(int patientId)
{
    // داده‌های پزشکی ممکن است قدیمی باشند
}
```

---

## 38. قانون Safe File Upload (Safe File Upload Rule)

### اصول اجباری:
- **در اکشن‌های مربوط به آپلود فایل، نوع فایل، حجم و امنیت مسیر بررسی شود**
- **اعتبارسنجی کامل فایل‌ها**
- **حفاظت در برابر حملات فایل**

### پیاده‌سازی:
```csharp
// ✅ صحیح - Safe File Upload
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> UploadPatientDocument(HttpPostedFileBase file)
{
    try
    {
        // بررسی وجود فایل
        if (file == null || file.ContentLength == 0)
        {
            return Json(new { success = false, message = "لطفاً فایل را انتخاب کنید." });
        }
        
        // بررسی حجم فایل (حداکثر 5MB)
        if (file.ContentLength > 5 * 1024 * 1024)
        {
            return Json(new { success = false, message = "حجم فایل نباید بیش از 5MB باشد." });
        }
        
        // بررسی نوع فایل
        var allowedTypes = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        
        if (!allowedTypes.Contains(fileExtension))
        {
            return Json(new { success = false, message = "نوع فایل مجاز نیست." });
        }
        
        // بررسی امنیت مسیر
        var safeFileName = Path.GetFileName(file.FileName);
        var uploadPath = Path.Combine(Server.MapPath("~/Uploads/"), safeFileName);
        
        // آپلود امن
        file.SaveAs(uploadPath);
        
        return Json(new { success = true, message = "فایل با موفقیت آپلود شد." });
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در آپلود فایل");
        return Json(new { success = false, message = "خطا در آپلود فایل." });
    }
}
```

---

## 39. قانون کوچک بودن گام‌ها در کنترلرها (Small Steps in Controllers Rule)

### اصول اجباری:
- **تغییرات باید در گام‌های کوچک اعمال شود**
- **قبل از افزودن اکشن یا متد جدید، بررسی وجودی انجام شود**
- **از ایجاد موارد تکراری جلوگیری شود**

### پیاده‌سازی:
```csharp
// ✅ صحیح - گام کوچک
// مرحله 1: اضافه کردن یک اکشن
[HttpGet]
public async Task<ActionResult> GetDoctorSchedule(int doctorId)
{
    // فقط یک اکشن جدید
}

// مرحله 2: اضافه کردن اکشن بعدی (در گام جداگانه)
[HttpPost]
public async Task<ActionResult> UpdateDoctorSchedule(ScheduleViewModel model)
{
    // اکشن بعدی
}

// ❌ نادرست - تغییرات بزرگ
public class DoctorController : Controller
{
    // اضافه کردن 10 اکشن همزمان - نادرست
}
```

---

## خلاصه قوانین تخصصی کنترلرها (14 قانون)

### ✅ الزامات اصلی کنترلرها:
25. **اصل SRP** - مسئولیت واحد برای هر کنترلر
26. **جلوگیری از تکرار** - بررسی وجود قبل از ایجاد
27. **ورودی و خروجی ایمن** - ViewModel و JsonResult
28. **Async/Await اجباری** - عملیات غیرهمزمان
29. **Logging با Serilog** - لاگ‌گیری جامع
30. **Error Handling** - مدیریت خطا کامل
31. **CSRF Protection** - حفاظت در برابر CSRF
32. **Authorization** - کنترل دسترسی
33. **Validation قبل از سرویس** - اعتبارسنجی ورودی
34. **Prevent Overposting** - حفاظت در برابر Over-posting
35. **AJAX Actions** - ساختار استاندارد JSON
36. **پرهیز از منطق تجاری** - انتقال به سرویس
37. **Performance در محیط درمانی** - بدون Cache برای داده‌های پزشکی
38. **Safe File Upload** - آپلود امن فایل
39. **کوچک بودن گام‌ها** - تغییرات مرحله‌ای

### ❌ ممنوعیت‌های کنترلرها:
- کنترلرهای بزرگ با مسئولیت‌های متعدد
- اکشن‌های تکراری
- خروجی Entity خام
- عملیات همزمان
- عدم لاگ‌گیری
- عدم مدیریت خطا
- عدم حفاظت CSRF
- عدم کنترل دسترسی
- عدم اعتبارسنجی
- Over-posting
- پاسخ‌های غیراستاندارد AJAX
- منطق تجاری در کنترلر
- Cache برای داده‌های پزشکی
- آپلود ناامن فایل
- تغییرات بزرگ همزمان

---

## قرارداد استاندارد نمایش اطلاعات در فرم‌های جزئیات (Information Display Standards Contract)

### مقدمه
این قرارداد استاندارد برای تمامی فرم‌های نمایش اطلاعات (Details, View, Show) در پروژه ClinicApp الزام‌آور است و باید در کنار قوانین اصلی AI_COMPLIANCE_CONTRACT رعایت شود.

---

## 40. قانون استاندارد نمایش اطلاعات (Information Display Standards Rule)

### اصول اجباری:
- **تمام فرم‌های جزئیات باید از ساختار یکپارچه استفاده کنند**
- **رنگ‌بندی رسمی و خوانا برای محیط درمانی**
- **کنتراست مناسب برای افراد مسن و کم‌بینا**
- **ساختار کارتی با بخش‌بندی منطقی**

### پیاده‌سازی:
```html
<!-- ✅ صحیح - ساختار استاندارد -->
<div class="card border-primary" style="border-width: 2px;">
    <div class="card-header bg-primary text-white" style="background-color: #007bff !important;">
        <h4 class="card-title mb-0" style="color: #ffffff !important; font-weight: 600;">
            <i class="fa fa-icon" style="color: #ffffff !important;"></i>
            عنوان بخش
        </h4>
    </div>
    <div class="card-body" style="background-color: #ffffff;">
        <div class="info-table">
            <div class="info-row" style="background-color: #f8f9fa; border-right: 4px solid #007bff;">
                <label class="info-label" style="color: #2c3e50 !important; background-color: #ffffff; border: 1px solid #007bff;">برچسب:</label>
                <span class="info-value" style="color: #212529 !important; background-color: #ffffff; border: 1px solid #e9ecef; font-weight: 600;">مقدار</span>
            </div>
        </div>
    </div>
</div>
```

---

## 41. قانون رنگ‌بندی رسمی (Formal Color Scheme Rule)

### اصول اجباری:
- **استفاده از رنگ‌های استاندارد Bootstrap با کنتراست بالا**
- **رنگ‌بندی یکپارچه در تمام بخش‌ها**
- **پرهیز از رنگ‌های تند یا غیررسمی**

### پیاده‌سازی:
```css
/* رنگ‌بندی استاندارد */
.bg-primary { background-color: #007bff !important; } /* آبی اصلی */
.bg-success { background-color: #28a745 !important; } /* سبز موفقیت */
.bg-info { background-color: #17a2b8 !important; } /* آبی اطلاعات */
.bg-warning { background-color: #ffc107 !important; } /* زرد هشدار */
.bg-danger { background-color: #dc3545 !important; } /* قرمز خطا */
.bg-secondary { background-color: #6c757d !important; } /* خاکستری */

/* متن‌ها */
.text-primary { color: #2c3e50 !important; } /* متن اصلی */
.text-value { color: #212529 !important; } /* مقادیر */
.text-muted { color: #6c757d !important; } /* متن کم‌رنگ */
```

---

## 42. قانون ساختار کارتی (Card Structure Rule)

### اصول اجباری:
- **هر بخش اطلاعات در یک Card جداگانه**
- **Header با رنگ مناسب و آیکون**
- **Body با پس‌زمینه سفید**
- **Border با ضخامت 2px**

### پیاده‌سازی:
```html
<!-- ساختار استاندارد Card -->
<div class="card border-[color]" style="border-width: 2px;">
    <div class="card-header bg-[color] text-white" style="background-color: #[hex] !important;">
        <h5 class="card-title mb-0" style="color: #ffffff !important; font-weight: 600;">
            <i class="fa fa-[icon]" style="color: #ffffff !important;"></i>
            عنوان بخش
        </h5>
    </div>
    <div class="card-body" style="background-color: #ffffff;">
        <!-- محتوای اطلاعات -->
    </div>
</div>
```

---

## 43. قانون نمایش اطلاعات (Information Display Rule)

### اصول اجباری:
- **هر ردیف اطلاعات در یک info-row**
- **برچسب در سمت راست، مقدار در سمت چپ**
- **استفاده از Badge برای مقادیر مهم**
- **پس‌زمینه خاکستری روشن برای ردیف‌ها**

### پیاده‌سازی:
```html
<div class="info-row" style="background-color: #f8f9fa; border-right: 4px solid #[color];">
    <label class="info-label" style="color: #2c3e50 !important; background-color: #ffffff; border: 1px solid #[color];">
        برچسب:
    </label>
    <span class="info-value" style="color: #212529 !important; background-color: #ffffff; border: 1px solid #e9ecef; font-weight: 600;">
        <span class="badge badge-[type]" style="background-color: #[color] !important; color: #[text-color] !important; font-weight: 600;">
            مقدار
        </span>
    </span>
</div>
```

---

## 44. قانون Badge ها و مقادیر (Badge and Values Rule)

### اصول اجباری:
- **استفاده از Badge برای مقادیر مهم**
- **رنگ‌بندی مناسب برای هر نوع مقدار**
- **فونت 600 برای خوانایی بهتر**

### پیاده‌سازی:
```css
/* Badge های استاندارد */
.badge-primary { background-color: #007bff !important; color: #ffffff !important; }
.badge-success { background-color: #28a745 !important; color: #ffffff !important; }
.badge-warning { background-color: #ffc107 !important; color: #212529 !important; }
.badge-danger { background-color: #dc3545 !important; color: #ffffff !important; }
.badge-info { background-color: #17a2b8 !important; color: #ffffff !important; }
.badge-secondary { background-color: #6c757d !important; color: #ffffff !important; }

/* متن‌های muted */
.text-muted {
    color: #6c757d !important;
    font-weight: 500;
    background-color: #f8f9fa;
    padding: 0.25rem 0.5rem;
    border-radius: 3px;
    border: 1px solid #e9ecef;
}
```

---

## 45. قانون دسترس‌پذیری (Accessibility Rule)

### اصول اجباری:
- **فونت حداقل 14px برای افراد مسن**
- **کنتراست مناسب (حداقل 4.5:1)**
- **پشتیبانی از کیبورد**
- **Responsive design**

### پیاده‌سازی:
```css
/* دسترس‌پذیری */
.info-label, .info-value {
    font-size: 14px;
    min-height: 2rem;
    display: flex;
    align-items: center;
}

/* Responsive */
@media (max-width: 768px) {
    .info-row {
        flex-direction: column;
        align-items: flex-start;
    }
    
    .info-label {
        min-width: auto;
        margin-bottom: 0.5rem;
    }
}
```

---

## 46. قانون بخش‌بندی منطقی (Logical Sectioning Rule)

### اصول اجباری:
- **بخش‌بندی اطلاعات بر اساس نوع**
- **استفاده از آیکون‌های مناسب**
- **ترتیب منطقی نمایش اطلاعات**

### پیاده‌سازی:
```html
<!-- بخش‌های استاندارد -->
<!-- 1. اطلاعات اصلی -->
<div class="card border-primary">
    <div class="card-header bg-primary">
        <i class="fa fa-user"></i> اطلاعات اصلی
    </div>
</div>

<!-- 2. وضعیت -->
<div class="card border-success">
    <div class="card-header bg-success">
        <i class="fa fa-check-circle"></i> وضعیت
    </div>
</div>

<!-- 3. زمان‌بندی -->
<div class="card border-info">
    <div class="card-header bg-info">
        <i class="fa fa-calendar"></i> اطلاعات زمان‌بندی
    </div>
</div>

<!-- 4. تاریخچه -->
<div class="card border-warning">
    <div class="card-header bg-warning">
        <i class="fa fa-history"></i> تاریخچه و حسابرسی
    </div>
</div>
```

---

## 47. قانون عملیات سریع (Quick Actions Rule)

### اصول اجباری:
- **بخش عملیات در پایین صفحه**
- **دکمه‌های بزرگ و واضح**
- **رنگ‌بندی مناسب برای هر عمل**

### پیاده‌سازی:
```html
<div class="card border-dark">
    <div class="card-header bg-dark text-white">
        <h5 class="card-title mb-0">
            <i class="fa fa-cogs"></i>
            عملیات سریع
        </h5>
    </div>
    <div class="card-body text-center">
        <div class="btn-group" role="group">
            <a href="#" class="btn btn-warning btn-lg">
                <i class="fa fa-edit"></i> ویرایش
            </a>
            <a href="#" class="btn btn-secondary btn-lg">
                <i class="fa fa-list"></i> بازگشت به لیست
            </a>
        </div>
    </div>
</div>
```

---

## 48. قانون CSS یکپارچه (Unified CSS Rule)

### اصول اجباری:
- **CSS مشترک برای تمام فرم‌های جزئیات**
- **استفاده از !important برای اطمینان**
- **سازگاری با Bootstrap**

### پیاده‌سازی:
```css
/* CSS استاندارد برای تمام فرم‌های جزئیات */
.info-section { margin-bottom: 1.5rem; }
.section-title { font-weight: bold; color: #2c3e50; margin-bottom: 1rem; }
.info-table { background-color: #ffffff; }
.info-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
    padding: 0.75rem;
    background-color: #f8f9fa;
    border-radius: 5px;
}
.info-label {
    font-weight: 600;
    color: #2c3e50 !important;
    font-size: 14px;
    min-width: 140px;
    text-align: right;
    background-color: #ffffff;
    padding: 0.5rem;
    border-radius: 3px;
}
.info-value {
    color: #212529 !important;
    font-weight: 600;
    text-align: left;
    background-color: #ffffff;
    padding: 0.5rem;
    border-radius: 3px;
    min-height: 2rem;
    display: flex;
    align-items: center;
}
```

---

## خلاصه قوانین استاندارد نمایش اطلاعات (9 قانون)

### ✅ الزامات اصلی فرم‌های جزئیات:
40. **استاندارد نمایش اطلاعات** - ساختار یکپارچه
41. **رنگ‌بندی رسمی** - کنتراست مناسب
42. **ساختار کارتی** - Card با Header و Body
43. **نمایش اطلاعات** - info-row با برچسب و مقدار
44. **Badge ها و مقادیر** - رنگ‌بندی مناسب
45. **دسترس‌پذیری** - فونت 14px و کنتراست
46. **بخش‌بندی منطقی** - آیکون‌های مناسب
47. **عملیات سریع** - دکمه‌های بزرگ
48. **CSS یکپارچه** - استایل مشترک

### ❌ ممنوعیت‌های فرم‌های جزئیات:
- ساختار غیریکپارچه
- رنگ‌بندی غیررسمی
- کنتراست کم
- فونت کوچک‌تر از 14px
- عدم پشتیبانی از موبایل
- عدم استفاده از Badge
- عدم بخش‌بندی منطقی
- عدم دسترس‌پذیری

---

---

## **قوانین 49-65: قرارداد استاندارد فرم‌های ایجاد و ویرایش**

### **قانون 49: استاندارد فرم‌های رسمی محیط درمانی**
- **هدف**: ایجاد فرم‌های حرفه‌ای و کاربرپسند برای محیط درمانی
- **محدوده**: تمامی فرم‌های Create و Edit در سیستم
- **اجباری**: استفاده از CSS کلاس‌های `form-*` به جای `details-*`

### **قانون 50: رنگ‌بندی رسمی فرم‌ها**
- **رنگ‌های مجاز**: آبی (Primary)، سبز (Success)، خاکستری (Secondary)
- **ممنوع**: رنگ‌های تند، انیمیشن‌های غیرضروری، المان‌های فانتزی
- **کنتراست**: مطابق استاندارد WCAG

### **قانون 51: ساختار فرم‌ها**
- **Container**: `form-container` برای کل صفحه
- **Header**: `form-header` با `form-title` و `form-subtitle`
- **Cards**: `form-card` با `form-card-header` و `form-card-body`
- **Grid**: `form-row` و `form-col-*` برای چیدمان

### **قانون 52: فیلدهای فرم**
- **Group**: `form-group` برای هر فیلد
- **Label**: `form-label` با `required` برای فیلدهای اجباری
- **Input**: `form-control` با حالت‌های `is-valid` و `is-invalid`
- **Help**: `form-help` برای متن راهنما

### **قانون 53: دکمه‌های فرم**
- **Primary**: `btn-form btn-form-primary` برای ثبت
- **Secondary**: `btn-form btn-form-secondary` برای بازگشت
- **Danger**: `btn-form btn-form-danger` برای لغو
- **Actions**: `form-actions` با `form-actions-left` و `form-actions-right`

### **قانون 54: اعتبارسنجی فرم**
- **Error**: `form-error` برای پیام‌های خطا
- **Invalid**: `is-invalid` برای فیلدهای دارای خطا
- **Valid**: `is-valid` برای فیلدهای معتبر
- **States**: `has-success`, `has-error`, `has-warning`

### **قانون 55: تاریخ‌های شمسی**
- **Class**: `persian-datepicker` برای فیلدهای تاریخ
- **Direction**: RTL و text-align: right
- **Focus**: border-color: #007bff

### **قانون 56: Select2 و Dropdown ها**
- **Height**: 45px برای consistency
- **Border**: 2px solid #e9ecef
- **Focus**: border-color: #007bff با box-shadow

### **قانون 57: Responsive Design**
- **Mobile**: فرم‌ها در موبایل تک ستونی
- **Tablet**: فرم‌ها در تبلت دو ستونی
- **Desktop**: فرم‌ها در دسکتاپ چند ستونی

### **قانون 58: Accessibility**
- **Focus**: outline: 2px solid #007bff
- **High Contrast**: border-width: 3px
- **Keyboard**: Tab navigation کامل

### **قانون 59: Loading States**
- **Loading**: `form-loading` با spinner
- **Animation**: `form-spin` برای loading indicator
- **Pointer Events**: none در حالت loading

### **قانون 60: Form Layout Grid**
- **Row**: `form-row` برای سطرها
- **Col**: `form-col`, `form-col-6`, `form-col-4`, `form-col-3`
- **Responsive**: در موبایل همه ستون‌ها 100% عرض

### **قانون 61: Form Actions**
- **Container**: `form-actions` برای نوار عملیات
- **Left**: `form-actions-left` برای دکمه‌های سمت راست
- **Right**: `form-actions-right` برای دکمه‌های سمت چپ
- **Mobile**: در موبایل عمودی

### **قانون 62: Form Helpers**
- **Help Text**: `form-help` برای متن راهنما
- **Required**: `form-required` برای نشانگر فیلدهای اجباری
- **Icons**: استفاده از Font Awesome

### **قانون 63: Form Validation States**
- **Success**: `has-success` با رنگ سبز
- **Error**: `has-error` با رنگ قرمز
- **Warning**: `has-warning` با رنگ زرد

### **قانون 64: Form Animations**
- **Fade In**: `form-fade-in` برای کارت‌ها
- **Slide In**: `form-slide-in` برای فیلدها
- **Duration**: 0.3s ease-in-out

### **قانون 65: Form Print Styles**
- **Background**: سفید برای چاپ
- **Header**: خاکستری برای چاپ
- **Buttons**: مخفی در چاپ
- **Actions**: مخفی در چاپ

---

**تاریخ ایجاد**: جلسه فعلی  
**وضعیت قرارداد**: فعال و الزام‌آور  
**دامنه**: تمامی تعاملات هوش مصنوعی با پروژه ClinicApp  
**آخرین به‌روزرسانی**: جلسه فعلی - اضافه شدن قرارداد استاندارد فرم‌های ایجاد و ویرایش (17 قانون)
