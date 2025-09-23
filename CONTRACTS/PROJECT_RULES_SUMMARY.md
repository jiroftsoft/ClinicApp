# 📚 ClinicApp Contract Rulebook Summary

## 1. حاکمیت و فرآیندهای توسعه
- **AI_COMPLIANCE_CONTRACT.md**: 69 قانون الزام‌آور برای هوش مصنوعی و تیم؛ بهینه‌سازی ویو، پاسخ‌های DataTables، تغییرات اتمیک، منع تکرار، مستندسازی، توقف برای تایید، امنیت و کیفیت، خروجی شفاف، ممنوعیت اجرای خودکار و پایبندی به دامنه به‌علاوه 14 قانون ویژه کنترلرها، 9 قانون نمایش جزئیات، 17 قانون فرم‌های ایجاد/ویرایش و قواعد AJAX/تاریخ.
- **PREFLIGHT_CHECKLIST_CONTRACT.md**: چک‌لیست 9‌مرحله‌ای قبل از هر اقدام (مرور قراردادها، بازیابی دانش، تعیین دامنه، جستجوی تکرار، وابستگی‌ها، بررسی Schema، کنترل امنیت، ثبت شواهد و یادآوری تایید «I APPROVE APPLY»).
- **ATOMIC_EXECUTION_WORKFLOW_CONTRACT.md**: روند 9‌گامی با شناسه تغییر، جستجوی عدم تکرار، پیشنهاد کوچک، تحلیل ریسک و rollback، الزامات تست، مستندسازی شواهد و منع تغییرات غیراتمیک یا اعمال خودکار.
- **APP_PRINCIPLES_CONTRACT.md**: اصول معماری (ViewModel قوی، الگوی Factory، Anti-Forgery سراسری، ServiceResult Enhanced، پرهیز از dynamic/AutoMapper، الگوهای جلوگیری از خطا، Chart.js، استاندارد UI/UX پزشکی).
- **FORM_DEVELOPMENT_CHECKLIST.md**: نقشه کامل توسعه فرم‌های پیچیده از تحلیل و طراحی تا deployment؛ پوشش Controller/Service/Repository/ViewModel، چک‌های امنیت، تست، مستندسازی و مانیتورینگ.
- **PR_REVIEW_COMMENTS.md** & **SENIOR_REVIEWER_REPORT.md**: قالب بازبینی برای تفکیک ریسک‌ها (Critical تا Low)، پیشنهاد split PR، تحلیل وابستگی/Infrastructure/Migration و برنامه اقدام کوتاه‌، میان‌ و بلندمدت.
- **MANUAL_TESTING_CHECKLIST.md**: سناریوهای تست دستی برای بهینه‌سازی EF (راه‌اندازی، جستجوهای پرترافیک، حافظه، دسترسی همزمان، تست‌های امنیتی و مقایسه قبل/بعد).

## 2. استانداردهای بک‌اند و کارایی
- **ServiceResult_Enhanced_Contract.md** & **ServiceResult_Enhanced.md**: استفاده اجباری از ServiceResult/ServiceResult<T>، متدهای Factory، ValidationError با ErrorCode، AdvancedValidationResult و الگوهای پزشکی برای سرویس‌ها و کنترلرها.
- **DATATABLES_STANDARDS_CONTRACT.md**: JSON با حروف کوچک، Anti-Forgery در View/JS، مدیریت خطا، ستون‌بندی و نمونه کامل Controller/JS.
- **AJAX_RESPONSE_CHECKLIST.md** & **JQUERY_RESPONSE_PARSING_FIX.md**: کنترل `typeof response`, JSON.parse با try/catch، استفاده از `parsedResponse`, لاگ کامل، Error handler استاندارد و Helperهای مشترک.
- **DATE_CONVERSION_ERROR_FIX.md**: تفکیک HTML5 date از Persian DatePicker، اعتبارسنجی وجود element، کلاس و فرمت تاریخ، لاگ‌گذاری و جلوگیری از اجرای تبدیل روی فیلد نامعتبر.
- **EF_OPTIMIZATION_DIFFS.md** & **EF_PERFORMANCE_OPTIMIZATION_REPORT.md**: حذف N+1، Projection/Compiled Query، تنظیمات ApplicationDbContext، بهینه‌ساز EF و ایندکس‌های پیشنهادی، Unity/Global.asax wiring و بنچمارک/مانیتورینگ عملکرد.
- **SERVICE_FILTERS_FIX.md**: بازنویسی فیلتر خدمات با فرم جستجوی بهینه، ارسال امن، اعتبارسنجی ورودی، مدیریت صفحه‌بندی، آمار و الزامات محیط پزشکی.
- **NOTIFICATION_SYSTEM_CONTRACT.md**: چارچوب اعلان با jQuery safety، auto-hide 5ثانیه‌ای، مدیریت session، انیمیشن، Responsive/Dark Mode و امنیت XSS/CSRF.

## 3. استانداردهای UI/UX و فرم‌ها
- **ADMIN_LAYOUT_USAGE_GUIDE.md**: ساختار باندل‌های core و conditional در `_AdminLayout`, کنترل ViewBag برای DataTables/Select2/DatePicker/Validation و مثال‌های صفحه.
- **VIEW_OPTIMIZATION_CONTRACT.md**: اصول بارگذاری شرطی، هماهنگی با BundleConfig، سناریوهای صفحه (ساده، جدول، فرم، کامل)، چک‌لیست و مزایا.
- **DESIGN_PRINCIPLES_CONTRACT.md**: پالت رنگی رسمی، تایپوگرافی استاندارد، الزامات Anti-Forgery، استفاده از ServiceResult، چک‌لیست فرم‌های درمانی و ممنوعیت‌های طراحی.
- **DETAILS_DISPLAY_STANDARDS.md**: ساختار کارت، رنگ‌بندی برای دسته‌بندی اطلاعات، بخش‌بندی منطقی، الزامات Responsive/Accessibility و نمونه HTML/CSS.
- **FormStandards.md** & **COMPLEX_FORM_STANDARDS_CONTRACT.md**: معماری فرم‌های پیشرفته (ساختار فایل، ViewModelهای دو‌لایه، Persian DatePicker، اعتبارسنجی کلاینت/سرور، امنیت، بهینه‌سازی عملکرد و چک‌لیست جامع).
- **MEDICAL_UX_UI_STANDARDS.md**: CSS variables پزشکی، دکمه‌های دارای آیکون و Tooltip، دسترس‌پذیری، Responsive, Print & Dark Mode.
- **CREATE_FORM_UX_UI_OPTIMIZATION_REPORT.md** & **INDEX_UX_UI_OPTIMIZATION_REPORT.md**: تفکیک CSS/JS بیرونی، Validation Summary، تایپوگرافی، رنگ‌بندی، دکمه‌ها، جستجو، Accessibility, Responsive و Dark Mode برای فرم‌ها و صفحات Index.
- **INDEX_PAGE_USAGE_GUIDE.md** & **VIEWS_UPDATE_REPORT.md**: راهنمای تنظیم باندل و به‌روزرسانی View های DoctorSchedule (Index/Edit) همراه با اقدامات بعدی.
- **NOTIFICATION_SYSTEM_CONTRACT.md** & **TOAST_UNDEFINED_FIX.md**: سیستم اعلان استاندارد و رفع خطای پیام `undefined`، اضافه شدن Logging و سازگاری Bootstrap5.

## 4. مدیریت تاریخ شمسی و ماژول Persian DatePicker
- **PERSIAN_DATEPICKER_CONTRACT.md**: الگوی ViewModel/Controller/View، مدیریت Validation، Anti-Forgery و جلوگیری از خطای «The field must be a date».
- **PERSIAN_DATEPICKER_STANDARD_CONTRACT.md**: پیکربندی مرکزی، Bundleها و مزایای مهاجرت از مقداردهی دستی.
- **PERSIAN_DATEPICKER_MODULE_USAGE.md**: روش‌های استفاده (HtmlHelper، Extension، Partial)، تنظیمات پیشرفته و چک‌لیست قبل/بعد از پیاده‌سازی.
- **PERSIAN_DATEPICKER_LIBRARY_ERROR_FIX.md**: خطای عدم دسترسی به کتابخانه، تابع fallback دستی، Error handling و Logging.
- **DATE_CONVERSION_ERROR_FIX.md**: (ارجاع دوباره) پشتیبانی از تبدیل و نظارت بر نوع فیلد.

## 5. قوانین دامنه پزشکی و محافظت از داده
- **CLINIC_DELETION_PROTECTION.md / _DEBUG / _FIX**: سیستم تحلیل وابستگی، جلوگیری از حذف تصادفی، نمایش modal هشدار، logging جامع، تست و دستورالعمل دیباگ.
- **SERVICE_DELETION_FIX.md**, **SERVICE_DELETION_DEBUG.md** & **FINAL_FIX_SUMMARY.md**: رفع حذف خدمت، مدیریت کاربر سیستم، لاگ‌گیری، جلوگیری از پیام‌های نامشخص، بررسی Anti-Forgery و دسترس‌پذیری.
- **CLINIC_DETAILS_OPTIMIZATION.md**: غنی‌سازی View جزئیات کلینیک با آمار فعال/کل، تاریخچه، logging و پیشنهادات آینده.
- **SERVICE_FILTERS_FIX.md**: (ارجاع) بهینه‌سازی فیلتر خدمات در محیط درمانی.
- **RECEPTION_CREATE_FORM_FIXES.md**: مجموعه خطاهای فرم پذیرش (DatePicker، بارگذاری دسته‌بندی/خدمت، سن، ViewModel) و راه‌حل‌های الزام‌آور.
- **MEDICAL_FORMS_CONTRACT.md**: اصول سادگی، گروه‌بندی، رنگ‌ها، دکمه‌ها، اعتبارسنجی، دسترس‌پذیری، state management، Auto Save و caching داده.
- **MEDICAL_TROUBLESHOOTING.md**: راهنمای عیب‌یابی شامل Scripts کنسول، بررسی log، Anti-Forgery و چک‌لیست گزارش مشکل.
- **TOAST_UNDEFINED_FIX.md**: بخش از رفع اشکال خدمات (نمایش پیام‌های واضح، حذف فونت‌های ناقص، اصلاح accessibility).

## 6. منابع تضمین کیفیت و دانش
- **MANUAL_TESTING_CHECKLIST.md**: (ارجاع) سناریوهای تست دستی قبل از Merge.
- **PR_REVIEW_COMMENTS.md** & **SENIOR_REVIEWER_REPORT.md**: (ارجاع) ساختار بازبینی و توصیه‌ها.
- **PROJECT_ANALYSIS.md** & **PROJECT_COMPREHENSIVE_ANALYSIS.md**: نمای کلی معماری، داده، امنیت، UI، سرویس‌ها، عملکرد و نقشه راه بهبود.
- **CHANGELOG.md**: تاریخچه تغییرات نسخه‌ها.
- **README.md (CONTRACTS)**: مقدمه و مسیرهای مرجع برای شروع.

---
**Created:** 2025-09-22 23:30:40
