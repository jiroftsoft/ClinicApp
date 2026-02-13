# تحلیل عمیق مرحله ۲.۱ — حذف ساخت دستی ProfileApiController

## هدف
حذف وابستگی `DashboardController.UpdateProfile` به ساخت دستی `ProfileApiController` و استفاده از سرویس، بدون شکست فرم پروفایل در داشبورد و در صفحهٔ جداگانهٔ پروفایل.

---

## وضعیت فعلی

### جریان کنونی
1. **DashboardController.UpdateProfile (POST /Patient/Dashboard/UpdateProfile)**
   - پارامترها: `firstName, lastName, phoneNumber, email, birthDate, gender, address`
   - با `new ProfileApiController(...)` و `DependencyResolver.Current.GetService<IPatientService>()` یک نمونه از API کنترلر می‌سازد
   - `ControllerContext` را دستی ست می‌کند تا `GetCurrentPatientIdAsync()` کار کند
   - فراخوانی `apiController.UpdateProfile(...)` و برگرداندن همان `JsonResult`

2. **ProfileApiController.UpdateProfile (POST /Patient/Api/Profile/UpdateProfile)**
   - `GetCurrentPatientIdAsync()` برای گرفتن patientId
   - اعتبارسنجی: نام، نام خانوادگی، شماره تماس الزامی
   - `_patientService.GetPatientForEditAsync(patientId)` برای مدل ویرایش
   - پر کردن فیلدها روی مدل و فراخوانی `_patientService.UpdatePatientAsync(model)`
   - برگرداندن JSON با `success`, `message`, `reload`

### وابستگی‌ها
- هر دو مسیر از **IPatientService** استفاده می‌کنند: `GetPatientForEditAsync`, `UpdatePatientAsync`
- مدل ویرایش: **PatientCreateEditViewModel** (دارای PatientId, NationalCode, FirstName, LastName, PhoneNumber, Email, BirthDate, Gender, Address و غیره)
- اعتبارسنجی در کنترلر: فقط «نام و نام خانوادگی» و «شماره تماس» الزامی؛ بقیه اختیاری

### ریسک‌ها در صورت تغییر نادرست
- اگر فقط در Dashboard از سرویس استفاده کنیم و در ProfileApiController تغییری ندهیم، رفتار یکسان می‌ماند ولی تکرار منطق در دو جا از بین نمی‌رود.
- اگر متد جدید سرویس اعتبارسنجی متفاوتی داشته باشد، ممکن است یکی از دو فرم (داشبورد یا صفحه پروفایل) رفتار متفاوت پیدا کند.
- اگر امضای متد جدید با فراخوان‌های فعلی (مثلاً فرمی که به /Patient/Api/Profile/UpdateProfile پست می‌کند) سازگار نباشد، درخواست‌های مستقیم به API شکسته می‌شوند.

---

## استراتژی پیشنهادی (اتمیک)

### اصل
- **یک نقطه حقیقت:** منطق «به‌روزرسانی پروفایل از مقادیر فرم» در یک جا (سرویس) متمرکز شود.
- **هر دو کنترلر** فقط patientId را (از Base) بگیرند و همان متد سرویس را صدا بزنند و نتیجه را به JSON تبدیل کنند.
- **بدون تغییر قرارداد API:** خروجی JSON و کدهای خطا برای فرانت یکسان بمانند.

### گام‌های اتمیک

| # | کار | خروجی | تست |
|---|-----|--------|-----|
| 2.1.1 | اضافه کردن متد `UpdatePatientProfileFromFormAsync` به **IPatientService** با پارامترهای (patientId, firstName, lastName, phoneNumber, email, birthDate, gender, address) و خروجی `ServiceResult` | اینترفیس به‌روز | بیلد |
| 2.1.2 | پیاده‌سازی در **PatientService**: اعتبارسنجی (نام، نام خانوادگی، تماس الزامی)، GetPatientForEditAsync، پر کردن مدل، UpdatePatientAsync؛ در صورت خطا برگرداندن همان پیام‌های قابل نمایش به کاربر | منطق در سرویس | واحد/دستی |
| 2.1.3 | در **ProfileApiController.UpdateProfile**: به‌جای منطق فعلی، فقط فراخوانی `_patientService.UpdatePatientProfileFromFormAsync(patientId.Value, firstName, lastName, ...)` و برگرداندن Json بر اساس نتیجه | API بدون ساخت کنترلر | پست فرم به /Patient/Api/Profile/UpdateProfile |
| 2.1.4 | در **DashboardController**: تزریق **IPatientService** (از طریق سازنده)، در **UpdateProfile** فراخوانی `GetCurrentPatientIdAsync` و در صورت وجود patientId فراخوانی `_patientService.UpdatePatientProfileFromFormAsync(...)` و برگرداندن Json؛ حذف کامل ساخت ProfileApiController و DependencyResolver | داشبورد بدون وابستگی به API کنترلر | پست فرم از تب پروفایل داشبورد |
| 2.1.5 | بررسی **UnityConfig**: اطمینان از ثبت IPatientService برای DashboardController (در صورت نیاز) | DI درست | اجرای اپ |

---

## نکات فنی

- **اعتبارسنجی:** در سرویس همان قوانین فعلی کنترلر (نام و نام خانوادگی و شماره تماس الزامی؛ بقیه اختیاری) اعمال شود تا رفتار برای هر دو مسیر یکسان بماند.
- **پیام خطا:** خروجی `ServiceResult` با `Message` برای نمایش به کاربر؛ کنترلرها همان را در JSON برمی‌گردانند.
- **PatientCreateEditViewModel:** از خروجی `GetPatientForEditAsync` استفاده می‌شود و فقط فیلدهای مربوط به فرم پروفایل (FirstName, LastName, PhoneNumber, Email, BirthDate, Gender, Address) به‌روز می‌شوند تا NationalCode و سایر فیلدها دست‌نخورده بمانند (همان رفتار فعلی ProfileApiController).

---

## چک‌لیست پس از اتمام
- [ ] به‌روزرسانی پروفایل از تب پروفایل در داشبورد (POST به Dashboard) بدون خطا و با پیام موفقیت
- [ ] به‌روزرسانی پروفایل از صفحه/فرم جداگانهٔ پروفایل (POST به Api/Profile) بدون خطا
- [ ] در صورت خطا (مثلاً نام خالی)، پیام خطای مناسب در هر دو مسیر نمایش داده شود
- [x] هیچ استفاده‌ای از `new ProfileApiController` یا `DependencyResolver` در DashboardController نمانده باشد (انجام شد)

---

## وضعیت اجرا
- **2.1.1–2.1.4** انجام شد. متد `UpdatePatientProfileFromFormAsync` در `IPatientService`/`PatientService` اضافه شد؛ هر دو کنترلر از آن استفاده می‌کنند. DI: `IPatientService` در پروژه ثبت است و `DashboardController` از طریق سازنده آن را دریافت می‌کند (کنترلرهای Area توسط همان Dependency Resolver حل می‌شوند).
