# بررسی کامل فرایند ورود و ثبت‌نام مبتنی بر OTP

این سند جریان **ورود** و **ثبت‌نام** را در پروژه ClinicApp بر اساس **OTP (کد یکبار مصرف)** مرور می‌کند و نقاطی که هنوز به رمز عبور اشاره دارند را برای حرفه‌ای‌سازی صرفاً بر مبنای OTP مشخص می‌کند.

---

## ۱. معماری فعلی (پسوردلس / OTP)

- **ورود بیمار:** فقط با **کد ملی + OTP** (بدون رمز عبور).
- **ثبت‌نام بیمار:** **کد ملی → شماره موبایل → OTP → تکمیل پروفایل** (بدون رمز).
- **Identity:** با `PasswordlessPasswordValidator` کاربر بدون رمز ساخته می‌شود؛ قفل حساب و محدودیت تلاش برای OTP اعمال می‌شود.

---

## ۲. فرایند ورود (Login)

### مسیرها و کنترلر

| مرحله | اکشن | متد | توضیح |
|--------|------|------|--------|
| ۱ | `Account/Login` | GET | صفحه ورود (یا مودال از لایه اصلی). |
| ۲ | `Account/CheckUser` | POST | بررسی وجود کاربر با کد ملی؛ خروجی: `USER_EXISTS` یا `USER_IS_NEW`. |
| ۳ | `Account/SendLoginOtp` | POST | ارسال OTP به موبایل کاربر **موجود**؛ فقط وقتی کاربر از قبل در سیستم است. |
| ۴ | `Account/VerifyLoginOtp` | POST | بررسی OTP؛ در صورت موفقیت: SignIn و redirect. |

### سرویس‌ها

- **IAuthService.CheckUserExistsAsync(NationalCode)**  
  - کاربر با این کد ملی وجود دارد → `USER_EXISTS`.  
  - وجود ندارد → `USER_IS_NEW` (برای ثبت‌نام).
- **IAuthService.SendLoginOtpAsync(NationalCode)**  
  - اعتبارسنجی کد ملی و شماره موبایل، Rate limit، Lockout، تولید OTP، ذخیره در Session + DB (`OtpStates`, `OtpRequests`)، ارسال SMS.
- **IAuthService.VerifyLoginOtpAndSignInAsync(NationalCode, OtpCode)**  
  - اعتبارسنجی OTP (Session یا fallback از DB)، پاک‌سازی OTP، Reset failed count، اطمینان از رکورد Patient، SignIn و ثبت LoginHistory.

### ویوها

- **Views/Account/Login.cshtml**  
  - مراحل: `step-national-code` → در صورت کاربر جدید `step-register-phone`، در غیر این صورت بعد از SendLoginOtp → `step-otp`.  
  - فرم OTP با ۶ باکس و ارسال AJAX به `VerifyLoginOtp`؛ در موفقیت redirect با `response.redirectUrl`.
- **Views/Account/_LoginModal.cshtml**  
  - همان منطق برای مودال ورود در سایت؛ اسکریپت داخلی و در _Layout از `login-otp-manager.js` استفاده می‌شود.

### اسکریپت‌ها

- **Content/js/login-otp-manager.js**  
  - مدیریت ورودی OTP (پاست، فوکوس، به‌روزرسانی فیلد ترکیبی و غیره).

---

## ۳. فرایند ثبت‌نام (Registration)

### مسیرها و کنترلر

| مرحله | اکشن | متد | توضیح |
|--------|------|------|--------|
| ۱ | `Account/CheckUser` | POST | اگر `USER_IS_NEW` → نمایش مرحله شماره موبایل. |
| ۲ | `Account/SendRegistrationOtp` | POST | ارسال OTP به شماره موبایل **جدید** (چک تکراری نبودن شماره). |
| ۳ | `Account/VerifyRegistrationOtp` | POST | تأیید OTP؛ در موفقیت برگرداندن **URL توکن‌دار** برای تکمیل ثبت‌نام. |
| ۴ | `Account/CompleteRegistration` | GET | باز کردن صفحه تکمیل پروفایل با token (کد ملی + موبایل از توکن). |
| ۵ | `Account/CompleteRegistration` | POST | ثبت نهایی: `RegisterPatientAsync` سپس `SignInWithNationalCodeAsync`. |

### سرویس‌ها

- **IAuthService.SendRegistrationOtpAsync(NationalCode, PhoneNumber)**  
  - اعتبارسنجی موبایل، عدم تکراری بودن شماره، Rate limit، تولید و ذخیره OTP در Session و لاگ در `OtpRequests`، ارسال SMS.
- **IAuthService.VerifyRegistrationOtpAsync(NationalCode, PhoneNumber, OtpCode)**  
  - اعتبارسنجی OTP از Session؛ در موفقیت به‌روزرسانی `IsVerified` در لاگ.
- **IPatientService.RegisterPatientAsync(RegisterPatientViewModel, userIp)**  
  - ایجاد `ApplicationUser` (بدون رمز) و `Patient` در تراکنش؛ اختصاص نقش Patient.

### ویوها

- **Views/Account/CompleteRegistration.cshtml**  
  - فرم تکمیل پروفایل: نام، نام خانوادگی، تاریخ تولد، جنسیت، ایمیل، آدرس. **هیچ فیلد رمز عبور ندارد.**

### نکات امنیتی ثبت‌نام

- توکن تکمیل ثبت‌نام با `DpapiDataProtectionProvider("ClinicApp")` و انقضای ۱۵ دقیقه.
- پارامترهای حساس در لاگ ماسک می‌شوند.

---

## ۴. موارد باقی‌مانده مرتبط با «رمز عبور»

برای حرفه‌ای شدن **صرفاً بر مبنای OTP**، این بخش‌ها باید با خط‌مشی OTP هم‌خوان شوند:

### ۴.۱ تنظیمات بیمار (Patient Settings)

- **Areas/Patient/Views/Settings/Index.cshtml** و **Views/Dashboard/_SettingsTab.cshtml**  
  - لینک «تغییر رمز» به `Manage/ChangePassword`.  
  - **پیشنهاد:** برای کاربران Patient که رمز ندارند (`!HasPassword`) این لینک را نشان ندهید یا با متن «ورود شما با کد یکبار مصرف انجام می‌شود؛ نیازی به رمز عبور نیست» و بدون لینک تغییر رمز نمایش دهید.

### ۴.۲ Manage (حساب کاربری عمومی)

- **Controllers/ManageController.cs**  
  - `ChangePassword`, `SetPassword`, `HasPassword()`.  
- **Views/Manage/Index.cshtml**  
  - نمایش «Change your password» یا «Create» (SetPassword) بر اساس `HasPassword`.  
- **پیشنهاد:** برای نقش Patient اگر `!HasPassword` است، به‌جای فرم رمز، یک توضیح کوتاه OTP-only نمایش داده شود و لینک به تنظیمات اعلان‌ها/امنیت (مثلاً همان تب تنظیمات داشبورد) در نظر گرفته شود.

### ۴.۳ صفحات قدیمی Account (غیرفعال یا فقط پشتیبانی)

- **Views/Account/Register.cshtml**  
  - فرم قدیمی با فیلد Password؛ مسیر ورود اصلی بیمار از طریق **Login** (کد ملی + OTP یا ثبت‌نام با OTP) است.  
- **Views/Account/ForgotPassword.cshtml**, **ResetPassword.cshtml**, **ResetPasswordConfirmation.cshtml**, **ForgotPasswordConfirmation.cshtml**  
  - برای جریان OTP بیمار استفاده نمی‌شوند.  
- **پیشنهاد:** اگر هیچ لینک عمومی به Register/ForgotPassword در UI بیمار وجود ندارد، می‌توان آن‌ها را برای منطقه Patient مخفی نگه داشت یا در یک فاز بعدی به «بازیابی دسترسی با OTP» تبدیل کرد.

### ۴.۴ سرویس‌های داخلی

- **LegacyPatientWelcomeService**  
  - از `SetPassword` و لینک فعال‌سازی با Reset Password Token استفاده می‌کند. اگر خط‌مشی فقط OTP است، این سناریو یا حذف یا با «اولین ورود با OTP» جایگزین شود.

### ۴.۵ تنظیمات و پیکربندی

- **App_Start/IdentityConfig.cs**  
  - قبلاً `PasswordlessPasswordValidator` تنظیم شده؛ مناسب OTP است.
- **Helpers/AppSettings.cs** و **ManageController**  
  - تنظیمات مربوط به Password (مثل تاریخ انقضا) برای ادمین/کاربران با رمز قابل نگه‌داری است؛ برای بیمار OTP-only در UI بیمار نمایش داده نشود.

---

## ۵. خلاصه جریان برای توسعه‌دهنده

```
ورود:
  Login (GET) → CheckUser (POST) [USER_EXISTS]
    → SendLoginOtp (POST) → step-otp
    → VerifyLoginOtp (POST) → SignIn → redirect

ثبت‌نام:
  Login (GET) → CheckUser (POST) [USER_IS_NEW]
    → step-register-phone → SendRegistrationOtp (POST) → step-otp
    → VerifyRegistrationOtp (POST) → redirect to CompleteRegistration?token=...
    → CompleteRegistration (GET) → فرم پروفایل
    → CompleteRegistration (POST) → RegisterPatientAsync → SignIn → redirect
```

همهٔ مراحل ورود و ثبت‌نام بیمار **بدون رمز عبور** و فقط با **OTP** انجام می‌شوند.

---

## ۶. تغییرات اعمال‌شده (هم‌خوان با OTP)

- **Areas/Patient/Views/Settings/Index.cshtml**  
  - در تب **حساب**: به‌جای لینک «تغییر رمز عبور»، متن «ورود با کد یکبار مصرف (OTP)» و توضیح «ورود شما با ارسال کد تأیید به موبایل انجام می‌شود؛ نیازی به رمز عبور نیست» با badge «فعال» نمایش داده می‌شود.  
  - در تب **امنیت**: همین توضیح OTP جایگزین لینک تغییر رمز شده و نکته امنیتی به «کد تأیید ارسال‌شده به موبایل را با دیگران به اشتراک نگذارید» تغییر کرده است.

برای سایر نقش‌ها (مثلاً ادمین) در صورت استفاده از رمز عبور، صفحات Manage و لینک تغییر رمز در آن‌ها بدون تغییر باقی می‌مانند. در صورت نیاز می‌توان LegacyPatientWelcomeService را با خط‌مشی OTP هم‌خوان کرد یا لینک‌های ForgotPassword/Register را در UI بیمار مخفی نگه داشت.
