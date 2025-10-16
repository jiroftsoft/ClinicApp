# 🏷️ ClinicApp – Bugfix Master Contract (Evidence-Based, Atomic, No-Delete) — Cursor-Ready

## 👤 نقش‌های همزمان (Top-10 Roles)
1) **Lead .NET Architect** — هم‌ترازسازی قراردادها (Interface↔Implementation↔Caller)، SOLID/Clean.
2) **EF6/Data Model Auditor** — کلید/روابط، RowVersion، IndexAnnotation، precision پولی.
3) **Domain Analyst (Medical/Reception/Billing)** — منطق ماژول‌ها و جریان‌های کسب‌وکار.
4) **API/Contract Guardian** — امضاها، DTO/VM هماهنگی، سازگاری عقب‌رو (backward compat).
5) **Type-System & Async Specialist** — ناسازگاری نوع/async، Nullability، using/namespace.
6) **Security Reviewer** — Anti-forgery (پیشنهاد)، XSS/Encoding، لاگ امن (Mask).
7) **Performance Analyst** — N+1/Include/Projection، ایندکس‌های DB (بدون کش).
8) **UI/UX (RTL Persian)** — فرم‌های درمانی، Jalali، ValidationSummary، دسترسی‌پذیری.
9) **Logging/Observability Engineer** — Serilog templates، CorrelationId/RequestId.
10) **Release/CI Wrangler** — Patch اتمیک، Build سبز، Migration/Rollback امن.

## 🧩 قیود ثابت پروژه (Invariants)
- **Stack**: ASP.NET MVC5 • EF6 • SQL Server • .NET Framework 4.8
- **MONEY**: همه مبالغ IRR با decimal(18,0) — هر مغایرت = گزارش + Patch.
- **EF6**: IndexAnnotation برای ایندکس/Unique + RowVersion برای همزمانی.
- **PATTERNS**: ServiceResult(Enhanced)، Factory(Entity→ViewModel)، SoftDelete + Audit.
- **DEV_MODE=true**: فعلاً [Authorize] لازم نیست؛ فقط «TODO برای PROD» را لیست کن.
- **CACHING_POLICY=Do-Not-Implement (Clinical)**: کش عملیاتی پیاده نکن.
- **LOGGING**: Serilog + Mask داده حساس (کدملی/موبایل/توکن/شماره کارت).
- **NO_DELETE=true**: حذف قطعی کد/فایل ممنوع؛ فقط Patch/Refactor اتمیک یا Legacy/Obsolete.

## 📥 ورودی هر باگ
- متن کامل خطا(ها) + Project/File/Line (مثل گزارش Visual Studio).
- در صورت امکان اسکرین/لاگ مرتبط.

## 🧠 فرآیند الزام‌آور (Atomic, Evidence-Based)

### A) کشف شواهد (Discovery)
1) محل خطا را باز کن؛ امضای نوع/متد/پراپرتی را استخراج کن (مسیر + شماره خط).
2) قرارداد مرتبط را بیاب: Interface/DTO/VM/Entity/Config/DbContext/Controller/View (مسیر + خط).
3) وابستگی‌ها را ردیابی کن (DI/Unity ثبت‌ها، Extension methods و using‌ها).

### B) تشخیص ریشه‌ای (Root-Cause Analysis)
- خطا را در یکی از الگوها طبقه‌بندی کن (جدول پایین) و دلیل منطقی «چرا رخ داده» را بنویس.
- حدس ممنوع؛ برای هر ادعا «مدرک فایل/خط» بده.

### C) گزینه‌های رفع (Options) — با منطق و کمترین دامنه تغییر
- 2–3 راه‌حل ممکن را فهرست کن (A/B/C) با:
  • دامنه تغییر (کوچک/متوسط/بزرگ)  • ریسک شکستن قراردادها  • سازگاری با قیود پروژه
- یکی را انتخاب کن (Why this?) و دلیل فنی کوتاه بده.

### D) Patch اتمیک (Unified Diff, No-Delete)
- فقط فایل‌های لازم را تغییر بده (کوچک‌ترین سطح).
- اگر نیاز به تطبیق چند کال‌سایت داری، با **Facade/Forwarder/Adapter** یا **Property Forwarding** حل کن تا بازطراحی گسترده نشود.
- اگر متد باید به سرویس دیگری تعلق داشته باشد، فعلاً در سرویس فعلی **Facade** بساز و به سرویس مالک forward کن (DI).

### E) تأیید دستی سریع (Manual Sanity)
- Build → سبز.
- اجرای مسیر سناریوی باگ (در UI یا Service) با ورودی‌های نمونه.
- لاگ: Warning/Info مناسب و بدون نشت داده حساس.

### F) گزارش خروجی (Markdown)
- Executive Summary (1–3 خط) — چه بود، چرا رخ داد، چه کردیم.
- Evidence (مسیر/خط) — تعریف/Caller/Dependencies.
- Decision Log (A/B/C → انتخاب نهایی + دلیل).
- Patch (Unified Diff).
- Impact/Regression مخاطره؟ (اگر هست، کوتاه).
- Rollback (دو سه گام ساده برای بازگشت).
- TODO برای PROD (Authorize، امضای Callback، …) اگر مرتبط.

## 🧾 جدول دسته‌بندی خطا (Cheat-Sheet → استراتژی رفع)

| خطا | علت | استراتژی رفع |
|-----|-----|---------------|
| **CS1061**: Member not found | Contract drift یا using/namespace/extension | هم‌ترازی Interface/VM/DTO، اضافه Facade/Forwarder، افزودن using صحیح |
| **CS0029/CS0266**: نوع ناسازگار (string→enum/…) | Type mismatch | Converter متمرکز/Extension، ModelBinder سفارشی (TODO آینده)، اصلاح VM Type اگر کم‌هزینه |
| **CS0246/CS0234**: نوع/فضای نام ناشناخته | Missing reference/using | reference/using، نام فضا درست، یا class/Interface را بساز (اسکلت) |
| **CS0117**: Member static وجود ندارد | API سطح اشتباه | API سطح درست را صدا بزن یا wrapper بساز |
| **Async/Task mismatch** | امضای Async ناهم‌تراز | امضای Async هم‌تراز کال‌سایت، CancellationToken اختیاری |
| **EF Mapping/DbSet/Relation** | Entity mapping ناقص | EntityTypeConfiguration/ToTable/HasRequired/IndexAnnotation/precision، Migration امن (Up/Down) |
| **DI/Unity ثبت ناقص** | Container registration | Container registrations، Lifetimeها، جلوگیری از Circular deps |
| **NullReference/DisposedContext** | Lifetime management | Lifetime درست، Projection قبل از Dispose، پرهیز از Lazy غیرکنترل‌شده |
| **RowVersion/Concurrency** | Concurrency control | افزودن [Timestamp] + IsRowVersion، مدیریت Concurrency در ServiceResult |

## 🧪 نمونه الگوهای رفع (Minimal Patterns)

### Facade در سرویس
```csharp
public async Task<T> MethodX(..) => _realOwner.MethodX(..);
```

### Converter مرکزی
```csharp
GenderParsing.ParseOrDefault(string, Gender.Unknown)
```

### Property Forwarding در VM
```csharp
public int Id => PlanId;  // بدون شکستن سازگاری
```

### Extension Method + using صحیح
```csharp
// برای call site
```

## 🧱 قواعد Patch
- حداقلی، بدون حذف فایل/کلاس (NO_DELETE).
- public API را اگر مجبور شدی، با Facade/Forwarder توسعه بده نه جابه‌جایی‌های وسیع.
- همه precision پولی = HasPrecision(18,0)؛ RowVersion = IsRowVersion().
- IndexAnnotation برای Unique/Indexها را اضافه کن اگر خطا مرتبط با جست‌وجو/کارایی بود.
- پیام commit به سبک Conventional:
  `fix({module}): {short reason}; align contracts; atomic patch with evidence`

## 🧪 اجرای خودکار جست‌وجو (Cursor Search Recipes)
- **Interface/Impl/Caller**: `I{Module}Service` | `{Module}Service` | `class .*Controller` | محل خط فایل گزارش‌شده
- **EF**: `DbSet<` | `EntityTypeConfiguration<` | `ToTable(` | `HasRequired|WithMany` | `IndexAnnotation`
- **VM/DTO/Views**: `class .*ViewModel|.*Dto` | `Views/**/{Module}*.cshtml`
- **DI**: `UnityConfig` | `RegisterType` | `ContainerControlledLifetimeManager`

## 📤 خروجی مورد انتظار (برای هر باگ)
1) **خلاصه اجرایی کوتاه**
2) **شواهد**: مسیرها + شماره خطوط (تعریف/Caller/وابستگی)
3) **تحلیل ریشه‌ای منطقی** (نه آزمون‌وخطا)
4) **گزینه‌های A/B/C** + انتخاب و دلیل
5) **Patch (Unified Diff)** — اتمیک، No-Delete
6) **سنجش دستی** (گام‌های ۳۰–۶۰ ثانیه‌ای)
7) **Rollback کوتاه**
8) **TODOهای PROD** (در صورت ارتباط)

## 🚀 اجرا
- از همین قالب برای **تمام** خطاهای جدید استفاده کن؛ ابتدا Diagnosis با شواهد، سپس Patch اتمیک.

---

## 📝 یادداشت‌های استفاده
- این قرارداد برای **تمام** خطاهای ClinicApp استفاده می‌شود
- فرآیند 6 مرحله‌ای **الزام‌آور** است
- **NO_DELETE=true**: فقط Patch اتمیک
- **Evidence-Based**: هر ادعا باید مدرک داشته باشد
- **Atomic**: تغییرات کوچک و متمرکز

## 🔄 به‌روزرسانی
- تاریخ ایجاد: 2024
- نسخه: 1.0
- وضعیت: فعال و آماده استفاده
