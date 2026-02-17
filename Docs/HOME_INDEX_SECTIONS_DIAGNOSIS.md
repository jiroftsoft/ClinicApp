# تشخیص علت «سکشن‌های صفحه هوم تا یک بار رفرش دیده نمی‌شوند»

## اصول

- **بدون حدس:** هیچ تغییری در کد بر اساس فرض انجام نشده است.
- **علت قطعی** باید از طریق لاگ یا تکرار خطا مشخص شود.

## سناریوهای محتمل

| سناریو | توضیح | وضعیت در کد |
|--------|--------|---------------|
| **A) Exception در اولین درخواست** | یکی از تسک‌های `GetHomePageDataAsync` (مثلاً PromotionalEvents، Stories، …) در بار اول exception می‌دهد → کنترلر در catch مدل خالی برمی‌گرداند → کاربر صفحه بدون سکشن می‌بیند. رفرش = درخواست دوم موفق. | با لاگ Serilog در `HomeController.Index` catch قابل تشخیص است. |
| **B) کش مرورگر/پروکسی** | پاسخ قدیمی یا ناقص از کش برگردانده می‌شود. | فیلتر سراسری `NoCacheFilter` (در `FilterConfig` و `Global.asax`) روی همه اکشن‌ها از جمله `Home/Index` اعمال می‌شود؛ هدرهای no-cache از قبل ست هستند. |

## کارهای انجام‌شده (بر اساس تحلیل)

1. **حذف کد تکراری**  
   متد `SetHomePageNoCacheHeaders` در `HomeController` حذف شد؛ چون `NoCacheFilter` به‌صورت سراسری همین هدرها را ست می‌کند، تکرار آن لازم نبود.

2. **عدم پنهان‌کردن خطا**  
   در `HomePageService.GetHomePageDataAsync` به طراحی قبلی (یک `Task.WhenAll` و در صورت خطا rethrow) برگشتیم تا خطا پنهان نشود و علت واقعی در لاگ دیده شود.

3. **لاگ برای تشخیص علت**  
   در بلوک catch مربوط به `Home/Index` یک لاگ Serilog اضافه شد:
   - متن: `"Home/Index: خطا در GetHomePageDataAsync — برای رفع قطعی باید از همین لاگ علت مشخص شود"`.
   - با همین لاگ می‌توان stack trace و InnerException را دید و فهمید کدام سکشن/سرویس/وابستگی خطا داده است.

## گام بعدی (وقتی دوباره اتفاق افتاد)

1. در لاگ‌ها (مثلاً Serilog) جستجو کنید: **"Home/Index"** یا **"GetHomePageDataAsync"**.
2. Exception و InnerException و stack trace را بررسی کنید تا مشخص شود کدام بخش (مثلاً یک repository، سرویس یا DI) خطا می‌دهد.
3. رفع علت در همان نقطه (مثلاً ثبت در DI، اصلاح کوئری، یا رفع شرط رقابتی) انجام شود؛ نه با پنهان کردن خطا در یک بخش و خالی گذاشتن آن سکشن.

## فایل‌های مرتبط

- `Controllers/HomeController.cs` — اکشن `Index` و catch با Serilog.
- `Services/HomePageService.cs` — `GetHomePageDataAsync`.
- `Filters/NoCacheFilter.cs` — فیلتر سراسری no-cache.
- `App_Start/FilterConfig.cs` — ثبت `NoCacheFilter`.
