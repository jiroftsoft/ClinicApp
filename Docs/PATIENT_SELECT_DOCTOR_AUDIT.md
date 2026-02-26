# گزارش بررسی و بهینه‌سازی صفحه انتخاب پزشک — Patient/Appointment/Book/SelectDoctor

**مسیر:** `/Patient/Appointment/Book/SelectDoctor`  
**کنترلر:** `Areas/Patient/Controllers/AppointmentBookingController.cs` — `SelectDoctor(int? departmentId, string searchTerm)`  
**ویو:** `Areas/Patient/Views/AppointmentBooking/SelectDoctor.cshtml`  
**کارت پزشک:** `Areas/Patient/Views/Shared/_DoctorCard.cshtml`

---

## ۱. منطق و امنیت (بررسی شده)

| مورد | وضعیت |
|------|--------|
| جریان رزرو | Patient → SelectDoctor → SelectDate(doctorId) → SelectTime → Confirm → Payment ✅ |
| فقط پزشکان فعال | از `DoctorCrudService.GetDoctorsAsync` با فیلتر؛ لیست از سرویس معتبر می‌آید ✅ |
| اعتبار departmentId | در کنترلر: اگر `<= 0` نادیده گرفته می‌شود ✅ |
| اعتبار searchTerm | Trim + حداکثر ۱۰۰ کاراکتر ✅ |
| DoctorId در SelectDate | در SelectDate اعتبار `doctorId > 0`؛ در غیر این صورت Redirect به SelectDoctor ✅ |
| TempData برای خطا | NotificationHelper.SetError + نمایش در View با toastr ✅ |
| AllowAnonymous | برای مشاهده لیست پزشک؛ برای رزرو در مراحل بعد چک می‌شود ✅ |
| دستکاری DoctorId | در SelectDate از سرور با doctorId در URL؛ باید در مراحل بعد بررسی شود که پزشک واقعاً در سیستم باشد (در SelectDate/SelectTime معمولاً چک می‌شود) |

---

## ۲. داده و عملکرد

| مورد | وضعیت |
|------|--------|
| منبع داده کارت | DTO از `GetAvailableDoctorsAsync`: FullName, Specialization, MedicalCouncilCode, HasActiveSchedule, ScheduleInfo, AvailableDates, ProfileImageUrl ✅ |
| N+1 | برنامه‌ها (Schedules) به صورت Batch لود می‌شوند؛ جزئیات پزشک (Bio) در یک حلقه با `GetDoctorDetailsAsync` برای هر پزشک فراخوانی می‌شود — در صورت طولانی بودن لیست می‌توان در فاز بعد فقط برای لیست، Bio را حذف یا Batch کرد ✅/⚠️ |
| تصویر پروفایل | در کارت از `ProfileImageUrl` استفاده می‌شود؛ در صورت نبود، آیکون نمایش داده می‌شود ✅ |

---

## ۳. اصلاحات اعمال‌شده (UI/UX و دسترسی‌پذیری)

### ۳.۱ فرم جستجو (SelectDoctor.cshtml)
- **Label و `for`:** برای فیلدهای «جستجوی پزشک» و «بخش» از `for="searchInput"` و `for="departmentFilter"` استفاده شد.
- **نوع input:** به `type="search"` برای جستجو تغییر داده شد.
- **Autocomplete:** `autocomplete="off"` برای جستجو (جستجوی پزشک نه autofill آدرس).
- **Accessibility:** `role="search"` و `aria-label` برای فرم؛ `aria-label` برای input و select و دکمه جستجو.
- **ریسپانسیو:** کلاس‌های `col-12 col-md-4` برای فیلدها و دکمه در موبایل.

### ۳.۲ حالت خالی و لیست
- **Empty state:** متن واضح‌تر: «با فیلترهای فعلی پزشکی وجود ندارد. بخش یا عبارت جستجو را تغییر دهید.»
- **Semantic:** `role="list"` و `role="listitem"` برای کانتینر و کارت‌ها؛ `role="status"` و `aria-live="polite"` برای حالت خالی و loading.

### ۳.۳ کارت پزشک (_DoctorCard.cshtml)
- **ساختار:** از `<div>` به `<article>` با `role="listitem"` برای معنای بهتر.
- **تصویر پروفایل:** در صورت وجود `ProfileImageUrl` تصویر در آواتار دایره‌ای با `loading="lazy"` و `alt=""` (تزئینی) نمایش داده می‌شود؛ در غیر این صورت آیکون.
- **بج «اولین نوبت خالی»:** وقتی `AvailableDates` دارای آیتم باشد، بج سبز «اولین نوبت خالی» نمایش داده می‌شود.
- **موبایل:** با `order-1`/`order-2` در موبایل اول دکمه/بج‌ها و بعد اطلاعات پزشک نمایش داده می‌شود؛ `col-12 col-md-4` و `text-md-end` برای تراز در دسکتاپ و چیدمان در موبایل.
- **دکمه CTA:** متن به «انتخاب نوبت» تغییر کرد؛ `aria-label` مناسب برای لینک انتخاب پزشک و دکمه غیرفعال.

### ۳.۴ CSS (appointment-booking-views.css)
- **کارت:** border، border-radius، سایه در hover برای ظاهر مدرن‌تر.
- **آواتار تصویری:** `.doctor-avatar-img` با `object-fit: cover` و `border-radius: 50%`.
- **دکمه انتخاب:** `min-height: 44px` (و در موبایل 48px)، font-weight و border-radius یکسان.
- **موبایل:** padding کمتر برای کارت، اندازه آواتار کوچکتر، دکمه و دکمه جستجو با حداقل 48px ارتفاع و `font-size: 1rem`.

---

## ۴. پیشنهادات فاز بعد (اختیاری)

1. **عملکرد:** حذف یا Batch کردن فراخوانی `GetDoctorDetailsAsync` در حلقه برای لیست انتخاب پزشک (در صورت نیاز به Bio فقط در صفحه جزئیات).
2. **امنیت:** در SelectDate/SelectTime اطمینان از اینکه پزشک مربوط به doctorId واقعاً فعال و دارای نوبت است (احتمالاً در سرویس رزرو انجام می‌شود).
3. **تبدیل:** در صورت نیاز، دکمه «ادامه» یا «انتخاب نوبت» sticky در پایین صفحه در موبایل (فعلاً هر کارت خود دکمه دارد).
4. **جستجوی زنده:** در صورت استفاده از AJAX برای جستجو، نمایش `#loadingState` هنگام درخواست و پنهان کردن پس از دریافت نتیجه.

---

## ۵. اولویت اعمال‌شده

```
منطق و امنیت > دسترسی‌پذیری و برچسب‌ها > موبایل و CTA > ظاهر و سایه/بج
```

تمام تغییرات بدون بازنویسی گسترده و با حفظ رفتار فعلی انجام شده است.
