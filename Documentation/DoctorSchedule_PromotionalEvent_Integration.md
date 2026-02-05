# ارتباط ماژول برنامه کاری پزشک (DoctorSchedule) و ایونت‌های تبلیغاتی (PromotionalEvent)

## خلاصه

**بله.** اگر ایونتی مثل «عید نوروز» ایجاد کنید، تخفیف روی **نوبت‌های همان برنامه کاری** (حق ویزیت از `DoctorSchedule.ConsultationFee`) اعمال می‌شود؛ مشروط به اینکه تاریخ نوبت در بازه ایونت و در صورت تنظیم، پزشک در لیست ایونت باشد.

---

## ۱. جریان محاسبه قیمت نوبت

```
رزرو نوبت / نمایش قیمت
    ↓
AppointmentPricingService.CalculatePriceAsync(doctorId, serviceCategoryId, patientId, appointmentDate?)
    ↓
۱) GetBasePriceAsync(doctorId, serviceCategoryId)
    → DoctorScheduleRepository.GetDoctorScheduleAsync(doctorId)
    → اگر ConsultationFee > 0 → قیمت پایه = ConsultationFee (ریال)
    → وگرنه → قیمت پایه = DEFAULT_CONSULTATION_FEE (۵۰۰,۰۰۰ ریال)

۲) CalculateDiscountWithDetailsAsync(doctorId, patientId, basePrice, appointmentDate?)
    → PromotionalEventService.CalculateDiscountWithDetailsAsync(doctorId, basePrice, appointmentDate?)
    → GetEventsByDoctorAsync(doctorId, appointmentDateTime)
        → ایونت‌های فعال در بازه زمانی (StartDate ≤ تاریخ ≤ EndDate)
        → در صورت IsDoctorSpecific، پزشک در DoctorIds
        → TotalSlots == null یا UsedSlots < TotalSlots
    → محاسبه تخفیف (درصدی یا مبلغ ثابت) روی basePrice
    → برگرداندن TotalDiscount و PromotionalEventId

۳) قیمت نهایی = basePrice - discount (+ مالیات در صورت وجود)
```

**قیمت پایه نوبت** از **همان برنامه کاری پزشک** (`DoctorSchedule.ConsultationFee`) است و **تخفیف ایونت** روی همین مبلغ اعمال می‌شود.

---

## ۲. ایونت تبلیغاتی (PromotionalEvent)

- **بازه زمانی:** `StartDate` و `EndDate` — ایونت فقط وقتی اعمال می‌شود که **تاریخ نوبت** در این بازه باشد.
- **محدودیت پزشک:** اگر `IsDoctorSpecific = true` باشد، فقط برای پزشکانی که شناسه‌شان در `DoctorIds` (JSON) است اعمال می‌شود.
- **تخفیف:** درصدی (`DiscountType.Percentage`) یا مبلغ ثابت (`DiscountType.FixedAmount`) روی **همان قیمت پایه (حق ویزیت)**.
- **محدودیت تعداد:** `TotalSlots` و `UsedSlots` — در صورت پر شدن، ایونت دیگر اعمال نمی‌شود.

پس برای «عید نوروز» کافی است ایونتی با بازه تاریخ نوروز و در صورت نیاز با لیست پزشکان تعریف شود؛ تخفیف روی نوبت‌های همان پزشکان و در همان بازه اعمال می‌شود.

---

## ۳. محل‌های استفاده

| محل | استفاده از appointmentDate | وضعیت تخفیف ایونت |
|-----|----------------------------|---------------------|
| **رزرو نوبت (CreateAppointmentAsync)** | ✅ `appointmentDateTime` به `CalculatePriceAsync` پاس داده می‌شود | تخفیف بر اساس **تاریخ نوبت** اعمال می‌شود. |
| **نمایش قیمت (GetAppointmentPriceAsync)** | ✅ اختیاری؛ در صورت پاس دادن تاریخ، همان تاریخ استفاده می‌شود | اگر تاریخ نوبت پاس داده شود، تخفیف برای **همان روز** در پیش‌نمایش درست است. |
| **API GetAppointmentPrice** | ✅ پارامتر اختیاری `appointmentDate` اضافه شده | فرانت‌اند می‌تواند برای روز انتخاب‌شده قیمت (با تخفیف) را بگیرد. |

---

## ۴. نکته مهم برای نمایش قیمت قبل از رزرو

- **قبل از اصلاح:** `GetAppointmentPriceAsync(doctorId, serviceCategoryId)` بدون تاریخ بود؛ در محاسبه تخفیف از `DateTime.Now` استفاده می‌شد. بنابراین برای نوبت در تاریخ دیگری (مثلاً عید) ممکن بود تخفیف در پیش‌نمایش دیده نشود.
- **بعد از اصلاح:** امضای سرویس و API با پارامتر اختیاری `DateTime? appointmentDate = null` به‌روز شده است. در صورت ارسال تاریخ نوبت، تخفیف ایونت برای **همان تاریخ** محاسبه و نمایش داده می‌شود. در صورت عدم ارسال، رفتار مانند قبل (بر اساس امروز) باقی است.

---

## ۵. پذیرش (Reception) و ایونت تبلیغاتی

- ماژول **پذیرش** برای **خدمات** (Service) قیمت و بیمه را محاسبه می‌کند (مثلاً از `ServiceCalculationEngine` / تعرفه).
- **قیمت نوبت (حق ویزیت)** و **تخفیف ایونت** در جریان **رزرو نوبت آنلاین** از طریق `AppointmentPricingService` اعمال می‌شوند و در `Appointment.Price` و `Appointment.PromotionalEventId` ذخیره می‌شوند.
- اگر در پذیرش از همان نوبت/قیمت نوبت استفاده شود، مقدار از همان نوبت (قبلاً محاسبه‌شده با تخفیف) می‌آید؛ ارتباط مستقیم Reception با ایونت تبلیغاتی در کد فعلی فقط از طریق نوبتِ از قبل محاسبه‌شده است.

---

## ۶. جمع‌بندی

- **ارتباط ماژول‌ها:** برنامه کاری پزشک (`DoctorSchedule.ConsultationFee`) منبع **قیمت پایه نوبت** است؛ ایونت تبلیغاتی روی همین مبلغ و بر اساس **تاریخ نوبت** و **پزشک** تخفیف اعمال می‌کند.
- **ایونت عید نوروز:** با تعریف ایونت در بازه نوروز، تخفیف روی نوبت‌های همان بازه اعمال می‌شود.
- **بهبود انجام‌شده:** پشتیبانی از پارامتر اختیاری `appointmentDate` در `GetAppointmentPriceAsync` و API برای نمایش صحیح تخفیف در پیش‌نمایش قیمت برای روز انتخاب‌شده.
