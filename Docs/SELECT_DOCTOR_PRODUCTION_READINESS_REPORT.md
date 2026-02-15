# گزارش آماده‌سازی پروداکشن — ماژول SelectDoctor و فلو رزرو نوبت

**ماژول:** Patient → Appointment → Book → SelectDoctor  
**URL:** `http://localhost:3560/Patient/Appointment/Book/SelectDoctor`  
**تاریخ بررسی:** بر اساس کد فعلی پروژه

---

# PHASE 1 — SECURITY HARDENING

## 1) Authentication & Authorization

| موضوع | وضعیت | توضیح |
|--------|--------|--------|
| **اعتبارسنجی نقش در سرور** | 🔴 **غیرفعال** | `BasePatientController` بدون `[PatientRoleAuthorization]` است (کامنت شده). `SelectDoctor` و `SelectDate` و `SelectTime` با `[AllowAnonymous]` در دسترس همه هستند. |
| **منطق نقش در کلاینت** | 🟡 | در View فقط نمایش (نقش از سرور می‌آید). در JS هیچ چک نقش برای رزرو وجود ندارد — وابسته به سرور است. |
| **افشای User ID** | 🔴 **بله** | در `_AuthDiagnostic` وقتی `IsDebuggingEnabled` باشد، `User.Identity.GetUserId()` (GUID) و نام کاربری و لیست نقش‌ها در UI نمایش داده می‌شود. در پروداکشن با `debug=false` خاموش است، اما اگر به اشتباه با debug=true دیپلوی شود، افشا رخ می‌دهد. |
| **افشای اطلاعات دیباگ** | 🔴 **بله** | همان بلوک تشخیصی + در `_DoctorCard` وقتی `IsDebuggingEnabled` باشد متن «🔍 Debug: URL = ...» زیر دکمه نمایش داده می‌شود. |
| **Over-posting در رزرو** | 🟠 | `Reserve(AppointmentBookingViewModel model)` مدل را مستقیم می‌بندد. ViewModel دارای `Price` است؛ سرور در ساخت `AppointmentBookingRequestDto` از `model.Price` استفاده نمی‌کند و قیمت را خود محاسبه می‌کند، اما بهتر است با `[Bind(Include = "...")]` یا DTO مستقیم فقط فیلدهای مجاز بسته شوند. |

**یافته بحرانی:** در `Reserve` احراز هویت غیرفعال و `patientId = 1` (ثابت) استفاده شده است. هر کاربری می‌تواند تا Confirm پیش برود و در Reserve به‌نام بیمار ۱ رزرو ثبت شود.

```csharp
// AppointmentBookingController.cs ~798
var patientId = 1; // ⚠️ TEMPORARY: فقط برای تست
```

---

## 2) Sensitive Data Exposure

| داده | ریسک | توصیه |
|------|------|--------|
| **UserId (GUID)** | 🔴 | در تشخیصی نمایش داده می‌شود. در پروداکشن نباید در UI باشد. استفاده از یک تنظیم جدا (مثلاً `ShowAuthDiagnostic`) به‌جای وابستگی صرف به `compilation debug`. |
| **URL دیباگ در کارت** | 🟠 | اطلاعات داخلی مسیر؛ در پروداکشن با debug=false مخفی است. ترجیحاً همیشه در View پروداکشن حذف شود. |
| **کد نظام پزشکی** | 🟡 | در `_DoctorCard` نمایش `MedicalCouncilCode` — طبق قوانین معمولاً عمومی است؛ در صورت سیاست سخت‌گیرانه می‌توان فقط برای کاربر لاگین‌شده یا با ماسک نمایش داد. |
| **PII** | 🟡 | نام پزشک و تخصص از طریق API و View در دسترس است؛ برای لیست پزشکان قابل قبول است. |

---

## 3) Anti-Forgery

| اکشن | وضعیت |
|------|--------|
| **Reserve (POST)** | ✅ `[ValidateAntiForgeryToken]` اعمال شده. |
| **ProcessPayment (POST)** | ✅ `[ValidateAntiForgeryToken]` اعمال شده. |
| **SelectDoctor / SelectDate / SelectTime (GET)** | GET — نیازی به توکن نیست. |
| **CheckSlotAvailability (POST)** | بدون توکن (برای AllowAnonymous طراحی شده). |

**نتیجه:** برای مرحله رزرو و پرداخت محافظت CSRF وجود دارد.

---

## 4) Validation

| لایه | وضعیت |
|------|--------|
| **SelectDoctor** | ✅ `departmentId` و `searchTerm` (Trim، حداکثر طول ۱۰۰) اعتبارسنجی می‌شوند. |
| **SelectDate** | ✅ `doctorId` مثبت؛ پزشک وجود دارد و برنامه فعال دارد. |
| **SelectTime** | ✅ `doctorId` و `date`؛ پارس تاریخ؛ گذشته و بیش از ۹۰ روز. |
| **Reserve** | ✅ ModelState، DoctorId، تاریخ گذشته، StartTime < EndTime، double booking. |
| **دستکاری querystring** | 🟠 `SelectDate/1` و `SelectTime?doctorId=1&date=...` بدون احراز هویت در دسترس است؛ بعد از فعال شدن auth، سرور باید patientId را از session/claim بگیرد و doctorId فقط برای انتخاب پزشک استفاده شود (خطر اصلی رزرو به‌نام دیگری با patientId=1 است). |

---

# PHASE 2 — PERFORMANCE OPTIMIZATION

## 1) EF Query

| موضوع | وضعیت |
|--------|--------|
| **N+1** | 🟡 برای Schedules با Batch Loading و `AsNoTracking()` برطرف شده. برای Bio هنوز N بار `GetDoctorDetailsAsync` فراخوانی می‌شود (یک بار به ازای هر پزشک). |
| **Include** | ✅ `Include(WorkDays)` و `Include(TimeRanges)` برای Schedules. |
| **AsNoTracking** | ✅ در کوئری Schedules استفاده شده. |
| **Projection** | ✅ خروجی به `DoctorSearchResultDto` مپ می‌شود؛ entity کامل به View فرستاده نمی‌شود. |

## 2) Async

- ✅ `SelectDoctor` و سرویس‌ها به صورت async/await هستند.

## 3) Caching

- ✅ طبق طراحی، کش برای لیست پزشکان حذف شده (NoCache، داده Real-time). برای پروداکشن مناسب است.

## 4) Asset

- اسکریپت‌ها و استایل‌ها به صورت فایل جدا لود می‌شوند. در پروداکشن با Bundle/Minify می‌توان حجم را کاهش داد. لاگ‌های console در doctor-selection.js فقط در localhost فعال هستند.

---

# PHASE 3 — ARCHITECTURE

- **Controller:** سبک؛ اعتبارسنجی و فراخوانی سرویس و Factory. منطق کسب‌وکار در Service.
- **ViewModel:** فقط فیلدهای لازم (Doctors, Departments, SearchTerm, SelectedDepartmentId). نشت entity نداریم.
- **فلو رزرو:** وضعیت بین مراحل با URL (doctorId, date) و در Confirm/Reserve با POST و مدل منتقل می‌شود. ذخیره state در session برای رزرو استفاده نشده؛ پس از فعال شدن auth، patientId از کاربر جاری گرفته می‌شود و امن‌تر است.

---

# PHASE 4 — UX/UI برای پروداکشن

| مورد | وضعیت |
|------|--------|
| حذف بلوک دیباگ از UI | 🔴 باید در پروداکشن هرگز نمایش داده نشود (تنها به `IsDebuggingEnabled` وابسته نباشد یا بلوک حذف شود). |
| طراحی badge وضعیت | ✅ در دسترس / غیرفعال با کلاس و آیکون مشخص. |
| حالت لودینگ | ✅ `#loadingState` و `_showLoading` / `_hideLoading`. |
| حالت خطا | ✅ پیام خطا و Swal/toastr. |
| جلوگیری از دوبار کلیک | ✅ در time-selection دکمه با `data('processing')` قفل می‌شود. در doctor-selection با لینک/دکمه یک بار ناوبری. |
| دسترسی‌پذیری | 🟡 می‌توان برای کارت‌ها و دکمه‌ها aria-label و نقش‌های مناسب اضافه کرد. |

---

# PHASE 5 — PRODUCTION CHECKLIST

## 🔴 Critical (قبل از دیپلوی حتماً رفع شود)

- [x] **فعال‌سازی احراز هویت در Reserve:** ✅ اعمال شد. استفاده از `GetCurrentPatientIdAsync()`؛ در صورت null برگرداندن JSON با `requiresLogin: true`. کلاینت در `confirm-booking.js` با نمایش Swal و هدایت به `/Account/Login?returnUrl=...` واکنش نشان می‌دهد.
- [x] **محدود کردن CheckAuth در پروداکشن:** ✅ اعمال شد. در ابتدای `CheckAuth()` اگر `!HttpContext.IsDebuggingEnabled` باشد، `HttpNotFound()` برگردانده می‌شود.
- [x] **عدم نمایش تشخیص احراز هویت در پروداکشن:** ✅ اعمال شد. رندر `_AuthDiagnostic` از `SelectDoctor.cshtml` حذف شده است.
- [x] **حذف متن Debug URL از کارت پزشک:** ✅ اعمال شد. بلوک «🔍 Debug: URL» از `_DoctorCard.cshtml` حذف و برای دکمه `aria-label` اضافه شده است.
- [x] **جلوگیری از Over-posting در Reserve:** ✅ اعمال شد. پارامتر مدل با `[Bind(Include = "DoctorId, AppointmentDate, StartTime, EndTime, ServiceCategoryId, Description")]` محدود شده است.

## 🟠 Security

- [ ] اعمال مجدد `[PatientRoleAuthorization]` یا `[Authorize(Roles = "Patient")]` روی مراحل بعد از SelectDoctor (SelectDate, SelectTime, Confirm, Reserve) پس از رفع مشکل redirect.
- [ ] استفاده از `[Bind(Include = "DoctorId, AppointmentDate, StartTime, EndTime, ServiceCategoryId, Description")]` در Reserve برای جلوگیری از over-posting روی Price و سایر فیلدها.

## 🟡 Performance

- [ ] در نظر گرفتن یک متد Batch برای Bio در GetAvailableDoctorsAsync تا N فراخوانی به GetDoctorDetailsAsync حذف شود.

## 🔵 UX

- [ ] اضافه کردن aria-label برای کارت پزشک و دکمه «انتخاب پزشک».
- [ ] اطمینان از نمایش پیام خطای شبکه در جستجوی زنده.

## 🟢 Optional

- [ ] پایش نرخ خطا و زمان پاسخ برای GetAvailableDoctors و GetOverview.
- [ ] در صورت نیاز، ماسک کردن کد نظام پزشکی (مثلاً فقط ۴ رقم آخر).

---

# PHASE 6 — REFACTOR PLAN (اقدامات بحرانی)

## 1) فعال‌سازی احراز هویت در Reserve و حذف patientId ثابت

**ریسک:** هر کاربر (حتی مهمان) می‌تواند با رفتن به Confirm و ارسال POST به Reserve، نوبت به‌نام بیمار ۱ ثبت کند (جعل هویت و هرجومرج رزرو).

**قبل:**
```csharp
// var patientId = await GetCurrentPatientIdAsync();
var patientId = 1; // ⚠️ TEMPORARY
```

**بعد:**
```csharp
var patientId = await GetCurrentPatientIdAsync();
if (patientId == null)
{
    _logger.Warning("Reserve: کاربر لاگین نشده یا بیمار یافت نشد");
    return Json(new { success = false, message = "لطفاً ابتدا وارد سیستم شوید", requiresLogin = true });
}
```

**تأثیر:** فقط کاربران احراز هویت‌شده با نقش Patient و دارای رکورد بیمار می‌توانند رزرو انجام دهند.

---

## 2) غیرقابل دسترس کردن CheckAuth در پروداکشن

**ریسک:** افشای userId، patientId و لیست نقش‌ها به هر کسی که URL را بداند.

**راه‌حل پیشنهادی:** شرط کردن اجرای منطق تشخیصی به یک تنظیم اپ (مثلاً `AppSettings:EnableAuthDiagnostic`) که فقط در محیط توسعه true باشد؛ در غیر این صورت برگرداندن 404 یا پیام «در دسترس نیست».

---

## 3) حذف رندر تشخیصی و Debug URL در View برای پروداکشن

**ریسک:** در صورت دیپلوی اشتباه با `debug=true`، اطلاعات حساس و مسیرهای داخلی در UI دیده می‌شوند.

**راه‌حل:**  
- در SelectDoctor: به‌جای `@if (this.Context.IsDebuggingEnabled)` از یک مقدار از ViewBag استفاده شود که فقط در محیط توسعه از Controller ست شود (بر اساس تنظیم اپ).  
- در _DoctorCard: حذف کامل بلوک `if (this.Context.IsDebuggingEnabled)` که متن «🔍 Debug: URL = ...» را نمایش می‌دهد، یا همان ViewBag/تنظیم اپ.

**تأثیر:** حتی با debug=true در پروداکشن، بلوک‌های حساس نمایش داده نمی‌شوند.

---

## 4) محدود کردن Binding در Reserve (جلوگیری از Over-posting)

**ریسک:** در آینده اگر فیلدی به ViewModel اضافه شود و در سرویس استفاده شود، کاربر بتواند از طریق ارسال فرم آن را دستکاری کند.

**قبل:**  
`public async Task<ActionResult> Reserve(AppointmentBookingViewModel model, ...)`

**بعد:**  
```csharp
public async Task<ActionResult> Reserve(
    [Bind(Include = "DoctorId, AppointmentDate, StartTime, EndTime, ServiceCategoryId, Description")] 
    AppointmentBookingViewModel model, 
    string idempotencyKey = null)
```
و ادامه استفاده از همان مدل برای ساخت DTO (بدون استفاده از model.Price و model.DoctorName و غیره).

**تأثیر:** فقط فیلدهای مجاز از درخواست باند می‌شوند؛ Price و نام پزشک و غیره از سمت سرور تعیین می‌شوند.

---

در ادامه، اصلاحات بحرانی **۱، ۳ (حذف Debug از View)، و ۴** در کد اعمال می‌شوند. مورد **۲ (CheckAuth)** را می‌توان با افزودن یک کلید در `AppSettings` و چک کردن آن در Controller انجام داد (در این گزارش مسیر پیشنهادی توضیح داده شده است).
