# خلاصه ماژول‌ها و هلپرهای تاریخ، زمان و تایم‌زون

این سند فهرست و نقش اجزای تخصصی پروژه برای **تاریخ، زمان، تایم‌زون و پرووایدر زمان** را خلاصه می‌کند.

---

## ۱. پرووایدر زمان (Time Provider)

### `Infrastructure/ITimeProvider.cs`

| مورد | توضیح |
|------|--------|
| **اینترفیس** | `ITimeProvider` |
| **پیاده‌سازی پیش‌فرض** | `DefaultTimeProvider` |
| **ثبت DI** | `UnityConfig` → `DefaultTimeProvider` برای `ITimeProvider` |

**اعضای مهم:**
- `UtcNow` — زمان فعلی UTC (برای ذخیره در دیتابیس)
- `Now` — زمان محلی سیستم
- `GetIranToday()` — **تاریخ امروز** در تایم‌زون ایران (فقط Date)
- `GetIranNow()` — **زمان فعلی** در تایم‌زون ایران
- `GetIranTodayPersian()` — همان تاریخ امروز به **رشته شمسی**
- `ToIranTime(DateTime utcTime)` — تبدیل UTC → زمان ایران
- `FromIranTime(DateTime iranTime)` — تبدیل زمان ایران → UTC (برای ذخیره/مقایسه)
- `FormatForIran(DateTime utcTime)` — فرمت نمایش برای کاربر ایرانی

**نکته:** تبدیل‌ها با `TimeZoneInfo` و زمان‌زون `"Iran Standard Time"` انجام می‌شوند. برای `FromIranTime` فقط وقتی `DateTime.Kind == Unspecified` (مثلاً از دیتابیس) درست کار می‌کند؛ در غیر این صورت مقدار به Unspecified تنظیم و سپس تبدیل می‌شود.

**استفاده در پروژه:** سرویس‌های رزرو نوبت، مشاوره آنلاین، اعلان‌ها و هر جایی که به «الان ایران» یا «امروز ایران» نیاز است.

---

## ۲. هلپرهای تاریخ شمسی (Persian Date)

### `Helpers/PersianDateHelper.cs`

| متد / قابلیت | کاربرد |
|---------------|--------|
| `ToPersianDate(DateTime)` | میلادی → رشته شمسی `yyyy/MM/dd` |
| `ToPersianDateTime(DateTime, includeSeconds)` | میلادی → رشته شمسی با زمان |
| `ToGregorianDate(string)` | رشته شمسی → `DateTime` میلادی (خروجی `Kind.Unspecified`) |
| `ToGregorianDateTime(string)` | رشته «تاریخ + زمان» شمسی → `DateTime` |
| `ParsePersianDate(string)` | رشته شمسی → `DateTime?` (در صورت خطا null) |
| محدوده و اعتبارسنجی | بررسی سال/ماه/روز شمسی و محدوده تقویم |

**نکته:** در `ToPersianDate` برای مقدار UTC از `TimeZoneInfo` برای تبدیل به زمان ایران استفاده می‌شود؛ برای `Unspecified` فعلاً به عنوان Local در نظر گرفته می‌شود (فقط برای نمایش تاریخ شمسی).

---

## ۳. اکستنشن‌های تاریخ و زمان

### `Extensions/DateTimeExtensions.cs`

- `ToPersianDate(this DateTime)` / `ToPersianDateTime(...)` — فراخوانی `PersianDateHelper`
- `ToPersianDate(this DateTime?)` / `ToPersianDateTime(this DateTime?, ...)` — نسخه nullable
- `ToDateTime(this string persianDate)` — شمسی → میلادی (از `PersianDateHelper.ToGregorianDate`)
- `ToDateTimeNullable` / `ToDateTimeFromPersian` / `ToDateTimeFromPersianNullable` — همان منطق با nullable یا alias
- `StartOfDay` / `EndOfDay` — ابتدا و انتهای روز
- `StartOfWeek` / `EndOfWeek` — هفته (قابل تنظیم با `DayOfWeek.Saturday`)
- `StartOfMonth` / `EndOfMonth` — ابتدا و انتهای ماه
- `ToRelativeTime(this DateTime)` — زمان نسبی («۲ ساعت پیش» و …)
- `IsBetween`, `IsWeekend`, `IsWorkday` — مقایسه و نوع روز

### `Extensions/PersianDateExtensions.cs`

- `ToFaDate(this DateTime, format)` / `ToFaDateTime` / `ToFaTime` — تبدیل به رشته شمسی با فرمت
- نسخه‌های nullable برای تاریخ
- `FromFaDate(this string)` / `TryParseFaDate` — رشته شمسی → `DateTime` / `DateTime?`
- اکستنشن‌های `HtmlHelper` برای **Persian DatePicker** در Viewها

### `Extensions/ControllerDateExtensions.cs`

- `ParsePersianDateSafe(this Controller, dateString, logger)` — پارس امن تاریخ شمسی با پشتیبانی از:
  - شمسی `YYYY/MM/DD`
  - timestamp (ثانیه/میلی‌ثانیه)
  - fallback به `PersianDateHelper.ToGregorianDate`
  - در صورت خطا یا تاریخ گذشته → `DateTime.Today`

---

## ۴. هلپر فرمت زمان

### `Helpers/TimeFormatHelper.cs`

- `FormatTimeToPersian(TimeSpan)` — تبدیل به متن فارسی با «قبل از ظهر» / «بعد از ظهر»
- `FormatTimeRangeToPersian(TimeSpan start, TimeSpan end)` — بازه زمانی به فارسی

استفاده معمول: نمایش زمان نوبت در UI بیمار (مثلاً صفحه تأیید رزرو).

---

## ۵. مدل بایندر و کنترلر پایه

### `Models/Binders/TimeSpanModelBinder.cs`

- بایند کردن مقدار ورودی (مثلاً `input type="time"` با `HH:mm`) به `TimeSpan` یا `TimeSpan?`
- پشتیبانی از فرمت‌های مختلف زمان برای Model Binding

### `Controllers/Base/PersianDateController.cs` و `Views/Base`

- کنترلر/ویوهای پایه مرتبط با تاریخ شمسی در صورت وجود.

### `Filters/PersianDateAttribute.cs`

- فیلتر/اعتبارسنجی مربوط به پارامترهای تاریخ شمسی در درخواست.

---

## ۶. کامپوننت DatePicker (شمسی)

### `Helpers/PersianDatePickerHelper.cs`

- متدهای `PersianDatePickerFor`, `PersianDatePickerWithComparison`, `PersianDatePickerWithOptions` و مشابه برای رندر Persian DatePicker در فرم‌ها.

### مستندات مرتبط (پوشه `Docs` و `Docs/Jalili`)

- `JALALIDATEPICKER_*.md` — راهنمای مهاجرت و استفاده از Jalali DatePicker
- `PERSIAN_DATEPICKER_*.md` — راهنمای کامپوننت Persian DatePicker
- `JALALIDATEPICKER_LEGACY_LOCATIONS.md` — مکان‌های قدیمی و مهاجرت

---

## ۷. استراتژی تایم‌زون و مستندات

### `Docs/ENTERPRISE_DATE_TIMEZONE_STRATEGY.md`

- قانون کلی: **UTC در دیتابیس، تبدیل به timezone محلی (ایران) فقط برای نمایش**
- لایه‌های Database، Business Logic (با `ITimeProvider`) و Presentation
- استفاده از `ITimeProvider` در سرویس‌ها به‌جای `DateTime.Now`/`UtcNow` مستقیم

### `Docs/APPOINTMENT_DATE_TIMEZONE_*.md`

- اصلاحات و ممیزی‌های مربوط به تاریخ و تایم‌زون در ماژول نوبت‌دهی.

---

## ۸. جریان پیشنهادی استفاده

| نیاز | پیشنهاد |
|------|----------|
| «الان» یا «امروز» در منطق کسب‌وکار | `ITimeProvider.GetIranNow()` / `GetIranToday()` / `GetIranTodayPersian()` |
| ذخیره یا مقایسه زمان در سرور | UTC و در صورت نیاز `FromIranTime` برای تبدیل زمان ایران → UTC |
| نمایش به کاربر | `ToIranTime` سپس `PersianDateHelper.ToPersianDate` یا `TimeFormatHelper` برای زمان |
| پارس تاریخ از کاربر (شمسی) | `PersianDateHelper.ParsePersianDate` / `ToGregorianDate` یا `ControllerDateExtensions.ParsePersianDateSafe` |
| نمایش تاریخ در View | `DateTimeExtensions.ToPersianDate` یا `PersianDateExtensions.ToFaDate` |
| بایند TimeSpan از فرم | `TimeSpanModelBinder` (ثبت در `Global.asax` / تنظیمات Model Binder) |

با این ماژول‌ها و هلپرها، تمام نیازهای تاریخ، زمان، تایم‌زون و پرووایدر در یک نقطه متمرکز و قابل تست پوشش داده می‌شوند.
