# بررسی یکپارچگی زرین‌پال با مستندات رسمی

تاریخ بررسی: بر اساس مستندات سایت زرین‌پال (docs.apiDocs، auth، guide، paymentGateway).

---

## تفاوت دو نوع مستندات زرین‌پال

| موضوع | مستندات «API» (لینک‌های شما) | مستندات «درگاه پرداخت» (استفاده‌شده در پروژه) |
|--------|--------------------------------|------------------------------------------------|
| **آدرس** | [apiDocs](https://www.zarinpal.com/docs/apiDocs/) ، [auth](https://www.zarinpal.com/docs/apiDocs/auth.html) ، [guide](https://www.zarinpal.com/docs/apiDocs/guide.html) | [درگاه پرداخت](https://www.zarinpal.com/docs/paymentGateway/) |
| **هدف** | مدیریت حساب، تیکت، کارت، تسویه، و... از طریق پنل/برنامه | دریافت پرداخت آنلاین روی سایت (درخواست پرداخت → ریدایرکت → تأیید) |
| **پروتکل** | OAuth 2.0 + GraphQL | REST (JSON) |
| **Base URL** | `https://next.zarinpal.com/api/` (OAuth) و `https://next.zarinpal.com/api/v4/graphql/` (GraphQL) | Sandbox: `https://sandbox.zarinpal.com/...` ، Production: `https://payment.zarinpal.com/...` (طبق راهنمای اتصال) |
| **احراز هویت** | `client_id` + `client_secret` و سپس Access Token (و Refresh Token) | فقط `merchant_id` در بدنهٔ درخواست |

- **پروژهٔ ClinicApp** فقط برای **دریافت پرداخت روی سایت** از زرین‌پال استفاده می‌کند؛ بنابراین با **درگاه پرداخت (REST v4)** کار می‌کند، نه با API جدید OAuth/GraphQL.
- لینک‌های apiDocs/auth/guide برای زمانی است که بخواهید از **API جدید** برای امور پنل (مثلاً تیکت، تسویه، کارت) استفاده کنید؛ این موضوع جدا از درگاه پرداخت است.

---

## تطبیق کد با مستندات «درگاه پرداخت»

### ۱. آدرس‌ها و محیط (Sandbox / Production)

منبع: [راهنمای اتصال به درگاه](https://www.zarinpal.com/docs/paymentGateway/connectToGateway.html) و [سرویس تست (sandbox)](https://www.zarinpal.com/docs/paymentGateway/sandBox.html).

| مورد | مستندات درگاه | پیاده‌سازی (`ZarinPalHelper` و `ZarinPalDriver`) |
|------|----------------|--------------------------------------------------|
| درخواست پرداخت (Request) | `https://payment.zarinpal.com/pg/v4/payment/request.json` | ✅ Sandbox: `sandbox.zarinpal.com/...` ، Production: `payment.zarinpal.com/pg/v4/payment/request.json` |
| تأیید پرداخت (Verify) | `https://payment.zarinpal.com/pg/v4/payment/verify.json` | ✅ همان مسیر (sandbox / payment) |
| **انتقال به صفحه پرداخت (StartPay)** | `Location: https://payment.zarinpal.com/pg/StartPay/` + authority | ✅ `PaymentUrl = _startPayUrl + authority` → کاربر به همین آدرس ریدایرکت می‌شود |
| استعلام وضعیت (Inquiry) | `.../payment/inquiry.json` — [مستندات](https://www.zarinpal.com/docs/paymentGateway/otherMethods/Inquiry.html) | ✅ `CheckPaymentStatusAsync` از `_inquiryUrl` (inquiry.json) استفاده می‌کند؛ **از این متد برای تأیید تراکنش استفاده نشود**، فقط وضعیت اعلام می‌شود |

### ۲. درخواست پرداخت (Request)

- **پارامترهای اجباری درگاه:** `merchant_id`، مبلغ، `callback_url`، `description` (طبق واژگان و روند درگاه).
- **کد:** در `ZarinPalDriver.RequestPaymentAsync` از `merchant_id`, `amount`, `callback_url`, `description` و اختیاری `mobile`, `email`, `metadata` استفاده شده است. ✅ با مستندات درگاه همخوان است.

### ۳. تأیید پرداخت (Verify)

- **مستندات:** پس از بازگشت کاربر به `callback_url`، با ارسال `authority` و `amount` (مبلغ همان تراکنش) باید Verify فراخوانی شود.
- **کد:** در `VerifyPaymentAsync` مقدارهای `merchant_id`, `amount`, `authority` ارسال و پاسخ با `data.code == 100` و فیلدهای `ref_id`, `card_pan`, `card_hash`, `fee` پردازش می‌شود. ✅ منطبق با روند تأیید درگاه.

### ۴. بازگشت به وب‌سایت پذیرنده و Verify

- **مستندات ([بازگشت به وب‌سایت پذیرنده](https://www.zarinpal.com/docs/paymentGateway/connectToGateway.html#%D8%A8%D8%A7%D8%B2%DA%AF%D8%B4%D8%AA-%D8%A8%D9%87-%D9%88%D8%A8%E2%80%8C%D8%B3%D8%A7%DB%8C%D8%AA-%D9%BE%D8%B0%DB%8C%D8%B1%D9%86%D8%AF%D9%87)):** یک `Status` به صورت QueryString به سایت پذیرنده ارسال می‌شود با مقادیر ثابت **OK** یا **NOK**. اگر `Status=NOK` باشد یعنی تراکنش ناموفق یا لغو شده؛ در این حالت **متد verify فراخوانی نشود**.
- **کد:** در `WebPaymentService.ProcessGatewayCallbackAsync` برای زرین‌پال، اگر `callbackData.Status != "OK"` باشد، بدون فراخوانی Verify یک `PaymentCallbackResult` ناموفق برمی‌گردد. ✅ فقط در صورت `Status=OK` متد verify فراخوانی می‌شود.

### ۵. سرویس تست (Sandbox)

- **مستندات:** [سرویس تست (sandbox)](https://www.zarinpal.com/docs/paymentGateway/sandBox.html) — آدرس‌ها از `payment.zarinpal.com` به `sandbox.zarinpal.com` تغییر داده می‌شوند.
- **نکات:** تمام authorityهای دریافتی از سندباکس با حرف **S** شروع می‌شوند؛ برای مرچنت آیدی در سندباکس یک UUID دلخواه کافی است.

### ۶. پاسخ‌ها و خطاها

- **ساختار پاسخ:** پاسخ‌های درگاه به صورت `data` و در صورت خطا `errors` هستند.
- **کد:** مدل‌های `ZarinPalRequestResponse`, `ZarinPalVerifyResponse`, `ZarinPalInquiryResponse` و غیره هر کدام `data` و `errors` دارند. ✅ با مستندات سازگار است.
- **لیست خطاها:** [لیست خطاها](https://www.zarinpal.com/docs/paymentGateway/errorList.html) — `GetZarinPalErrorMessage` با تمام کدهای مستند (public، PaymentRequest، PaymentVerify، PaymentReverse) و توضیح فارسی به‌روز شده است.

### ۷. کدهای وضعیت

- **کد 100:** موفق (در Request منجر به `authority` و در Verify منجر به `ref_id` و جزئیات تراکنش).
- **کد 101:** تراکنش وریفای شده است (در Verify یعنی قبلاً verify شده؛ در Inquiry وضعیت VERIFIED).
- **استعلام وضعیت (Inquiry):** وضعیت‌های `data.status`: VERIFIED، PAID، IN_BANK، FAILED، REVERSED — از این متد **به هیچ عنوان برای تأیید و وریفای کردن تراکنش** استفاده نشود؛ فقط برای اطلاع از وضعیت.

---

## اطمینان از اتصال به «صفحه پرداخت»

طبق [راهنمای اتصال](https://www.zarinpal.com/docs/paymentGateway/connectToGateway.html):

1. **درخواست پرداخت (POST request.json)** → در صورت موفقیت، پاسخ شامل `data.code = 100` و `data.authority` است.
2. **انتقال خریدار به صفحه پرداخت:** باید کاربر را به آدرس `https://payment.zarinpal.com/pg/StartPay/` + `authority` ریدایرکت کرد.
3. پس از پرداخت، زرین‌پال کاربر را به `callback_url` با پارامترهای `Authority` و `Status=OK/NOK` برمی‌گرداند.
4. **Verify (POST verify.json)** فقط وقتی `Status=OK` است با `merchant_id`, `amount`, `authority` فراخوانی شود.

در کد پروژه:
- پس از Request موفق، `PaymentUrl = _startPayUrl + authority` ساخته می‌شود (`ZarinPalDriver` خط ~۲۷۹).
- `_startPayUrl` در Production برابر `https://payment.zarinpal.com/pg/StartPay/` است (هم‌خوان با مستندات).
- بنابراین ریدایرکت به **همان صفحهٔ پرداخت رسمی زرین‌پال** انجام می‌شود.

آدرس‌های Production قبلاً در کد به صورت `api.zarinpal.com` / `www.zarinpal.com` بودند؛ طبق متن رسمی راهنمای اتصال، دامنهٔ صحیح **payment.zarinpal.com** است و در به‌روزرسانی اخیر اصلاح شده است.

---

## جمع‌بندی

- پیاده‌سازی (**ZarinPalHelper** + **ZarinPalDriver**) بر اساس **درگاه پرداخت زرین‌پال (REST v4)** و دقیقاً مطابق [راهنمای اتصال](https://www.zarinpal.com/docs/paymentGateway/connectToGateway.html) است؛ آدرس‌های Production با مستندات یکسان شده‌اند (`payment.zarinpal.com`).
- اتصال به **صفحه پرداخت** از طریق ریدایرکت به `https://payment.zarinpal.com/pg/StartPay/{authority}` انجام می‌شود.
- مستندات [apiDocs](https://www.zarinpal.com/docs/apiDocs/) و [auth](https://www.zarinpal.com/docs/apiDocs/auth.html) و [guide](https://www.zarinpal.com/docs/apiDocs/guide.html) مربوط به **API جدید (OAuth + GraphQL)** هستند و برای همین سناریوی «پرداخت روی سایت» استفاده نمی‌شوند.

---

## مراجع

- [راهنمای اتصال به درگاه (بازگشت به وب‌سایت پذیرنده، Verify)](https://www.zarinpal.com/docs/paymentGateway/connectToGateway.html)
- [سرویس تست (sandbox)](https://www.zarinpal.com/docs/paymentGateway/sandBox.html)
- [لیست خطاها](https://www.zarinpal.com/docs/paymentGateway/errorList.html)
- [استعلام وضعیت پرداخت (Inquiry)](https://www.zarinpal.com/docs/paymentGateway/otherMethods/Inquiry.html)
- [درگاه پرداخت | مستندات زرین‌پال](https://www.zarinpal.com/docs/paymentGateway/)
- [معرفی API (OAuth/GraphQL) | مستندات زرین‌پال](https://www.zarinpal.com/docs/apiDocs/)
