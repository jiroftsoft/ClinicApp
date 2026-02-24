# 📍 محل‌های استفاده از DatePicker قدیمی (Legacy)

**هدف:** هر وقت گفته شد «datepicker قدیمی» یا «بروز کن به نسخه جدید»، این فایل مرجع است برای پیدا کردن و به‌روزرسانی به **JalaliDatePicker Enterprise**.

**الگوی جدید (Enterprise):**
- در View: `data-jdp` و در صورت نیاز `data-jdp-theme="medical"` روی input (بدون کلاس `persian-datepicker`).
- در JS: استفاده از `JalaliDatePickerEnterprise.initializeAll()` یا `JalaliDatePickerEnterprise.startWatchAgain()` بعد از لود محتوا؛ برای مودال/AJAX حتماً `startWatchAgain()` بعد از باز شدن مودال.

---

## ✅ انجام‌شده (به Enterprise مهاجرت شده)

| محل | توضیح |
|-----|--------|
| `Areas/Admin/Views/InsurancePlan/Edit.cshtml` | ValidFromShamsi, ValidToShamsi → `data-jdp` |
| `Areas/Admin/Views/InsurancePlan/Create.cshtml` | همان |
| `Scripts/app/insurance-plan-form.js` | حذف `.persianDatepicker()`؛ استفاده از `JalaliDatePickerEnterprise.startWatchAgain()` و اعتبارسنجی با `convertPersianToGregorian` |
| `Areas/Admin/Views/PatientInsurance/Edit.cshtml` | فراخوانی `startWatchAgain()` بعد از ست کردن تاریخ‌ها؛ اعتبارسنجی بازه با `convertPersianToGregorian` |
| `Areas/Admin/Views/PatientInsurance/_PatientInsuranceForm.cshtml` | تاریخ شروع/پایان اعتبار → `data-jdp` و `data-jdp-theme="medical"` (استفاده در Edit و هر جایی که این پارشال لود شود) |
| `Areas/Admin/Views/PatientInsurance/Create.cshtml` | مرحله ۳: جزئیات بیمه — inputها به `data-jdp`؛ حذف لینک CSS قدیمی؛ `initializeEnhancedFallbackDateInputs` → `startWatchAgain()`؛ اعتبارسنجی با `convertPersianToGregorian` و رویداد `jdp:change` |

---

## ❌ هنوز قدیمی (برای بروزرسانی بعدی)

### Admin

| فایل | نوع استفاده | اقدام پیشنهادی |
|------|-------------|-----------------|
| `Areas/Admin/Views/DoctorSchedule/Edit.cshtml` | لینک CSS قدیمی persian-datepicker | حذف لینک؛ استفاده از data-jdp در inputها |
| `Areas/Admin/Views/DoctorSchedule/Schedule.cshtml` | CSS + JS قدیمی | همان |
| `Areas/Admin/Views/DoctorSchedule/Index.cshtml` | CSS + JS + `$('.persian-datepicker').persianDatepicker(...)` | جایگزینی با data-jdp و حذف init قدیمی |
| `Areas/Admin/Views/Security/LoginHistory/Index.cshtml` | لینک CSS قدیمی | حذف؛ data-jdp |
| `Areas/Admin/Views/DoctorTimeSlot/Index.cshtml` | لینک CSS قدیمی | همان |
| `Areas/Admin/Views/PaymentManagement/Index.cshtml` | کلاس persian-datepicker + persianDatepicker() | data-jdp و حذف init |
| `Areas/Admin/Views/DoctorAssignment/Index.cshtml` | کلاس persian-datepicker روی inputها | data-jdp |
| `Areas/Admin/Views/DoctorAssignment/Edit.cshtml` | persian-datepicker + persianDatepicker() | data-jdp و startWatchAgain یا حذف init |
| `Areas/Admin/Views/DoctorAssignment/_AssignmentFilters.cshtml` | کلاس persian-datepicker | data-jdp |
| `Areas/Admin/Views/DoctorServiceCategory/Index.cshtml` | persian-datepicker-initialized + init دستی | data-jdp و startWatchAgain بعد از لود |
| `Areas/Admin/Views/DoctorServiceCategory/Edit.cshtml` | input با کلاس persian-datepicker | data-jdp |
| `Areas/Admin/Views/DoctorServiceCategory/ServiceCategoryPermissions.cshtml` | CSS + JS قدیمی | حذف و data-jdp |
| `Areas/Admin/Views/CMS/Story/Edit.cshtml` | لینک CSS قدیمی | حذف؛ data-jdp |
| `Areas/Admin/Views/CMS/Story/Create.cshtml` | لینک CSS قدیمی | همان |
| `Areas/Admin/Views/CMS/Announcement/Edit.cshtml` | لینک CSS قدیمی | همان |
| `Areas/Admin/Views/CMS/Announcement/Create.cshtml` | لینک CSS قدیمی | همان |
| `Areas/Admin/Views/EmergencyBooking/Create.cshtml` | bundle persian-datepicker | حذف باندل؛ data-jdp |
| `Areas/Admin/Views/EmergencyBooking/Index.cshtml` | همان | همان |
| `Areas/Admin/Views/PatientInsurance/SupplementaryInsurances.cshtml` | persian-datepicker + persianDatepicker() | data-jdp و حذف init |
| `Areas/Admin/Views/SupplementaryTariff/_SupplementaryTariffFilters.cshtml` | persian-datepicker + persianDatepicker() | data-jdp و حذف init |
| `Areas/Admin/Views/CombinedInsuranceCalculation/Index.cshtml` | input + CSS + JS قدیمی | data-jdp و حذف اسکریپت قدیمی |
| `Areas/Admin/Views/InsuranceCalculation/Calculate.cshtml` | persian-datepicker + persianDatepicker() | data-jdp و حذف init |
| `Areas/Admin/Views/AppointmentAvailability/*.cshtml` (ReleaseSlot, ViewSlotDetails, ReserveSlot, GenerateMonthlySlots, CheckSlotAvailability, GenerateWeeklySlots) | `@Scripts.Render("~/bundles/persian-datepicker")` | حذف باندل؛ استفاده از data-jdp و لود Enterprise از Layout |

### Patient / Views (غیر Area)

| فایل | نوع استفاده | اقدام پیشنهادی |
|------|-------------|-----------------|
| `Views/Patient/Edit.cshtml` | persian-datepicker + pDatepicker() | data-jdp و حذف init (یا استفاده از Layout Patient که Enterprise دارد) |
| `Views/Patient/Create.cshtml` | همان | همان |
| `Views/Account/CompleteRegistration.cshtml` | `$('#birthdate-picker').pDatepicker(...)` | data-jdp روی input و startWatchAgain اگر داخل مودال است |

### Patient Area

| فایل | نوع استفاده | اقدام پیشنهادی |
|------|-------------|-----------------|
| `Areas/Patient/Views/Appointment/Available.cshtml` | لینک CSS قدیمی + `$dateInput.data('pDatepicker')` در چند جا | حذف لینک؛ یکسان‌سازی با Enterprise و eventهای pDatepicker:select/jdp:change |

### اسکریپت‌های مشترک

| فایل | نوع استفاده | اقدام پیشنهادی |
|------|-------------|-----------------|
| `Content/js/patient-profile.js` | persian-datepicker-initialized + $.fn.persianDatepicker | استفاده از data-jdp و JalaliDatePickerEnterprise.startWatchAgain() (صفحه پروفایل/تب) |
| `Scripts/app/patient-insurance-form.js` | persian-datepicker + persianDatepicker() و on change | data-jdp و حذف init؛ استفاده از jdp:change |
| `Views/Shared/_Layout.cshtml` | persian-datepicker-initialized + init دستی | در صورت استفاده از این Layout برای صفحات با تاریخ، جایگزینی با data-jdp و Enterprise |
| `Content/js/jquery-protection.js` | persian-datepicker-initialized و datepicker-initialized | در صورت لزوم، فقط برای المنت‌هایی که data-jdp ندارند یا حذف و یکسان‌سازی با Enterprise |

---

## 📌 نکات

1. **Layoutهای Admin و Patient Pro** قبلاً JalaliDatePicker Enterprise را لود و `initializeAll()` / `startWatchAgain()` را صدا می‌زنند؛ کافی است در View فقط `data-jdp` بگذارید و init قدیمی را حذف کنید.
2. **مودال / AJAX:** بعد از باز شدن مودال یا لود محتوا، `JalaliDatePickerEnterprise.startWatchAgain()` را فراخوانی کنید (راهنما: `Contracts/Knowledge-Base/AI/Master/01-Helpers-DateTime.md` بخش ۹).
3. **رویداد تغییر تاریخ:** به‌جای `change` می‌توان از `jdp:change` یا `pDatepicker:select` استفاده کرد تا با Enterprise سازگار باشد.
4. **تبدیل و اعتبارسنجی:** برای مقایسه تاریخ شمسی از `JalaliDatePickerEnterprise.convertPersianToGregorian()` استفاده کنید.

---

**آخرین به‌روزرسانی:** پس از مهاجرت InsurancePlan (Edit/Create) و insurance-plan-form.js به Enterprise.
