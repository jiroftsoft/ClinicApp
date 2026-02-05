# بررسی کامل ماژول ایونت‌های تبلیغاتی (PromotionalEvent)

## ۱. نمای کلی

ماژول **PromotionalEvent** برای مدیریت **تخفیف‌های دوره‌ای و ویژه** (مثل عید نوروز، تخفیف ویژه پزشک) طراحی شده است. تخفیف روی **قیمت پایه نوبت** (حق ویزیت از `DoctorSchedule.ConsultationFee`) اعمال می‌شود و در رزرو آنلاین و نمایش قیمت استفاده می‌شود.

---

## ۲. معماری و لایه‌ها

| لایه | فایل‌ها | وضعیت |
|------|---------|--------|
| **Entity** | `Models/Entities/PromotionalEvent/PromotionalEvent.cs` | ✅ ISoftDelete, ITrackable, PromotionalEventConfig, decimal(18,0) برای DiscountValue |
| **Enum** | `Models/Enums/DiscountType.cs` | ✅ Percentage, FixedAmount |
| **DTO** | `Models/DTOs/PromotionalEvent/DiscountResult.cs` | ✅ TotalDiscount, PromotionalEventId, PromotionalEventTitle |
| **Repository** | `Interfaces/PromotionalEvent/IPromotionalEventRepository.cs`, `Repositories/PromotionalEvent/PromotionalEventRepository.cs` | ✅ CRUD, GetActiveEventsAsync, GetEventsByDoctorAsync, SearchAsync, IncrementUsedSlotsAsync |
| **Service** | `Interfaces/PromotionalEvent/IPromotionalEventService.cs`, `Services/PromotionalEvent/PromotionalEventService.cs` | ✅ Create/Update/Delete, GetById/GetAll/GetActive, CalculateDiscountAsync, CalculateDiscountWithDetailsAsync, IncrementUsedSlotsAsync |
| **Controller** | `Areas/Admin/Controllers/PromotionalEventController.cs` | ✅ Index, Details, Create, Edit, Delete؛ استفاده از ViewModel و NotificationHelper |
| **ViewModels** | `ViewModels/PromotionalEventVM/PromotionalEventViewModels.cs` | ✅ Index, CreateEdit, Details, Factory, تبدیل Entity ↔ ViewModel |
| **Validator** | `ViewModels/PromotionalEventVM/PromotionalEventViewModelValidator.cs` | ✅ FluentValidation برای CreateEdit |
| **Views** | `Areas/Admin/Views/PromotionalEvent/` (Index, Create, Edit, Details, Delete) | ✅ Strongly-Typed، پالت medical |
| **DI** | `App_Start/UnityConfig.cs` | ✅ IPromotionalEventRepository, IPromotionalEventService ثبت شده |
| **DbContext** | `Models/IdentityModels.cs` | ✅ DbSet<PromotionalEvent> PromotionalEvents |
| **Migration** | `Migrations/202601081512167_AddPromotionalEventModule.cs` | ✅ جدول PromotionalEvents، ستون‌های Appointments (PromotionalEventId, DiscountAmount) |

---

## ۳. Entity و دیتابیس

### ۳.۱ فیلدهای اصلی

| فیلد | نوع | توضیح |
|------|-----|--------|
| EventId | int | PK, Identity |
| Title | string(200) | عنوان ایونت (الزامی) |
| Description | string(1000) | توضیحات |
| StartDate, EndDate | DateTime | بازه اعتبار ایونت |
| DiscountType | DiscountType (byte) | درصدی یا مبلغ ثابت |
| DiscountValue | decimal(18,0) | مقدار تخفیف (درصد یا ریال) — طبق قرارداد مالی |
| TotalSlots | int? | تعداد کل نوبت‌های قابل استفاده (null = نامحدود) |
| UsedSlots | int | تعداد استفاده شده |
| IsDoctorSpecific | bool | محدود به پزشکان خاص |
| DoctorIds | string (JSON) | آرایه شناسه پزشکان، مثلاً "[1,2,3]" |
| IsActive | bool | فعال/غیرفعال |

### ۳.۲ Soft Delete و Trackable

- **ISoftDelete:** IsDeleted, DeletedAt, DeletedByUserId  
- **ITrackable:** CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId  

### ۳.۳ رابطه با Appointment

- **PromotionalEvent** HasMany **Appointments** (WithOptional, FK: PromotionalEventId).  
- در **Appointment**: PromotionalEventId (nullable), DiscountAmount (decimal(18,0)).

---

## ۴. سرویس و ریپوزیتوری

### ۴.۱ منطق تخفیف (CalculateDiscountWithDetailsAsync)

1. **ورودی:** doctorId, basePrice, appointmentDate (اختیاری؛ null = امروز).
2. **فیلتر ایونت‌ها:** `GetEventsByDoctorAsync(doctorId, appointmentDateTime)`:
   - ایونت‌های فعال: IsActive, !IsDeleted.
   - بازه زمانی: `StartDate <= now && EndDate >= now`.
   - تعداد: `TotalSlots == null || UsedSlots < TotalSlots`.
   - پزشک: اگر IsDoctorSpecific باشد، doctorId باید در DoctorIds باشد؛ وگرنه ایونت برای همه پزشکان است.
3. **محاسبه:** برای هر ایونت:
   - درصدی: `discount = basePrice * (DiscountValue / 100)`.
   - مبلغ ثابت: `discount = DiscountValue`.
   - سقف: `discount = Min(discount, basePrice)`.
4. **جمع:** totalDiscount مجموع تخفیف همه ایونت‌های واجد شرایط؛ سپس `totalDiscount = Min(totalDiscount, basePrice)`.
5. **خروجی:** DiscountResult با TotalDiscount و PromotionalEventId (ایونتی که بیشترین تخفیف را داده).

### ۴.۲ نکته: چند ایونت هم‌زمان

اگر **چند ایونت** برای یک نوبت اعمال شوند، مجموع تخفیف‌ها به عنوان **totalDiscount** برگردانده می‌شود، اما فقط **یک** PromotionalEventId (ایونت با بیشترین تخفیف) در Appointment ذخیره می‌شود و فقط برای **همان یک ایونت** در رزرو، `IncrementUsedSlotsAsync` صدا زده می‌شود. ایونت‌های دیگر که در جمع تخفیف نقش داشتند، UsedSlotsشان افزایش نمی‌یابد. در صورت نیاز به «یک نوبت = یک ایونت» یا «شمارش استفاده برای همه ایونت‌های اعمال‌شده»، باید منطق کسب‌وکار و ذخیره‌سازی اصلاح شود.

---

## ۵. یکپارچگی با نوبت و قیمت

| محل | استفاده |
|-----|---------|
| **AppointmentPricingService** | GetBasePriceAsync از DoctorSchedule.ConsultationFee؛ CalculateDiscountWithDetailsAsync از PromotionalEventService؛ خروجی: BasePrice, DiscountAmount, FinalPrice, PromotionalEventId. |
| **AppointmentBookingService** | در رزرو: CalculatePriceAsync با appointmentDate؛ ذخیره Price, DiscountAmount, PromotionalEventId در Appointment؛ بعد از Commit موفق، IncrementUsedSlotsAsync(PromotionalEventId). در GetAppointmentPriceAsync: پاس اختیاری appointmentDate برای نمایش صحیح تخفیف. |
| **Appointment (Entity)** | PromotionalEventId (nullable), DiscountAmount، رابطه با PromotionalEvent. |

جریان کامل در سند **DoctorSchedule_PromotionalEvent_Integration.md** توضیح داده شده است.

---

## ۶. کنترلر و ویوها

- **Index:** جستجو (SearchTerm, IsActive, FromDate, ToDate)، لیست با کارت/جدول، نمایش نام پزشکان برای ایونت‌های DoctorSpecific.
- **Create/Edit:** فرم با تاریخ شمسی (تبدیل از hidden)، انتخاب چند پزشک (AvailableDoctors, SelectedDoctorIds)، اعتبارسنجی سمت سرور و FluentValidation.
- **Details:** نمایش ایونت + لیست نوبت‌های استفاده‌شده (Appointments).
- **Delete:** حذف نرم (Soft Delete).

**امنیت:** روی کنترلر `[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]` فعال است (فاز تحویل).

---

## ۷. اعتبارسنجی

- **سرویس (Create/Update):** StartDate < EndDate، DiscountValue > 0، درصد ≤ 100، در صورت IsDoctorSpecific وجود DoctorIds، TotalSlots > 0 در صورت مقداردهی؛ در Update عدم کاهش TotalSlots به کمتر از UsedSlots.
- **FluentValidation (CreateEditViewModel):** Title الزامی و حداکثر 200 کاراکتر، EndDate > StartDate، DiscountValue > 0، در صورت درصدی ≤ 100، در صورت IsDoctorSpecific حداقل یک پزشک.

---

## ۸. مقایسه تاریخ (بازه ایونت)

در **GetActiveEventsAsync** شرط `StartDate <= now && EndDate >= now` با `now = appointmentDate ?? DateTime.Now` استفاده می‌شود. اگر **EndDate** به صورت «آخر روز» (مثلاً 23:59:59) ذخیره نشود و فقط «تاریخ» (مثلاً 00:00:00) باشد، نوبت‌های همان روز پایان ممکن است از فیلتر خارج شوند. توصیه: در ایجاد/ویرایش ایونت، EndDate را به انتهای روز تنظیم کنید یا در کوئری فقط بخش تاریخ را مقایسه کنید.

---

## ۹. چک‌لیست وضعیت ماژول

| مورد | وضعیت |
|------|--------|
| Entity و Config با decimal(18,0) و قرارداد مالی | ✅ |
| CRUD کامل (Create, Update, Delete نرم) | ✅ |
| GetById, GetAll, GetActive, GetEventsByDoctor, Search | ✅ |
| محاسبه تخفیف با تاریخ نوبت و محدودیت پزشک/تعداد | ✅ |
| ذخیره PromotionalEventId و DiscountAmount در Appointment | ✅ |
| IncrementUsedSlots بعد از رزرو موفق | ✅ |
| ViewModels و Factory و Validator | ✅ |
| ثبت در DI و DbSet | ✅ |
| ارتباط با DoctorSchedule و قیمت نوبت | ✅ (مستند در DoctorSchedule_PromotionalEvent_Integration.md) |
| Authorize روی کنترلر | ✅ فعال (Admin, Receptionist) |
| چند ایونت و شمارش UsedSlots | ⚠️ فقط یک ایونت در Appointment ذخیره و Increment می‌شود |
| EndDate برای «آخر روز» | ⚠️ در صورت نیاز در UI/سرویس لحاظ شود |

---

## ۱۰. جمع‌بندی

ماژول **PromotionalEvent** از نظر ساختار (Entity، Repository، Service، Controller، ViewModels، Validator، DI، Migration و یکپارچگی با نوبت و قیمت) **کامل و قابل استفاده** است. تخفیف روی **نوبت‌ها (حق ویزیت)** اعمال می‌شود و با ارسال **تاریخ نوبت** در محاسبه قیمت، تخفیف ایونت (مثلاً عید نوروز) به‌درستی اعمال و نمایش داده می‌شود. برای تحویل، **Authorize** و **لینک در منوی Admin** اعمال شده‌اند. در صورت نیاز، رفتار «چند ایونت» و EndDate به صورت آخر روز در فاز بعد قابل توسعه است. چک‌لیست تحویل در **PROMOTIONAL_EVENT_DELIVERY_CHECKLIST.md** آمده است.
