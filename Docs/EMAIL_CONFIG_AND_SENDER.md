# تنظیمات ایمیل و آدرس فرستنده

## از چه ادرسی ایمیل ارسال می‌شود؟

**آدرس فرستنده (From)** از **Web.config** و کلید زیر خوانده می‌شود:

```xml
<add key="Email:FromAddress" value="noreply@yourclinic.com" />
```

- در محیط واقعی باید مقدار را به آدرس رسمی کلینیک/دامنه (مثلاً `newsletter@yourclinic.com` یا `noreply@yourclinic.com`) تغییر دهید.
- این آدرس در همه ارسال‌های ایمیل (خبرنامه، تایید اشتراک، OTP و غیره) به عنوان **From** استفاده می‌شود.
- اگر خالی باشد یا فرمت نامعتبر داشته باشد، سرویس ارسال خطا می‌دهد و ایمیل ارسال نمی‌شود.

---

## کلیدهای تنظیمات ایمیل (Web.config)

| کلید | الزامی | توضیح |
|------|--------|--------|
| `Email:FromAddress` | بله | آدرس فرستنده (From) — باید ایمیل معتبر باشد. |
| `Email:SmtpServer` | بله | آدرس سرور SMTP (مثلاً smtp.gmail.com). |
| `Email:Port` | بله | پورت (معمولاً 587 برای TLS). |
| `Email:Username` | بستگی به سرور | نام کاربری SMTP. |
| `Email:Password` | بستگی به سرور | رمز SMTP. |
| `Email:Enabled` | خیر | فعال/غیرفعال کردن ارسال (پیش‌فرض: false). در production باید true شود. |
| `Email:EnableSsl` | خیر | استفاده از SSL/TLS (پیش‌فرض: true). |
| `Email:MaxRetries` | خیر | تعداد تلاش مجدد (پیش‌فرض: 3). |
| `Email:TimeoutMs` | خیر | زمان‌ timeout هر تلاش به میلی‌ثانیه (پیش‌فرض: 15000). |
| `Email:RetryBaseDelayMs` | خیر | پایه تأخیر بین تلاش‌ها (پیش‌فرض: 400). |

---

## مسیر اجرایی ارسال ایمیل

1. **NewsletterEmailService** (یا سایر استفاده‌کنندگان) پیام را با `IdentityMessage` (مقصد، موضوع، متن) آماده می‌کنند.
2. **EmailService** (در `App_Start/IdentityConfig.cs`):
   - اگر `Email:Enabled` برابر false باشد، ارسال انجام نمی‌شود و بدون خطا برمی‌گردد.
   - `Email:FromAddress` و تنظیمات SMTP را می‌خواند؛ در صورت خالی یا نامعتبر بودن **Exception** پرتاب می‌کند.
   - با Retry و Backoff ارسال را انجام می‌دهد؛ در صورت شکست نهایی **Exception** پرتاب می‌کند.
3. caller با `try/catch` این خطا را می‌گیرد و در صورت نیاز `ServiceResult.Failed` یا پیام مناسب به کاربر برمی‌گرداند.

---

## نکات امنیتی و عملیاتی

- رمز SMTP را در Web.config در محیط production با رمزنگاری یا متغیر محیطی/راز ذخیره کنید.
- آدرس From را حتماً روی دامنه‌ای که در SMTP مجاز است قرار دهید تا از اسپم و bounce کم شود.
