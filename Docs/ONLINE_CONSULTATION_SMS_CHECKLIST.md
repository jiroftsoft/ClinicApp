# چک‌لیست ارسال پیام (SMS) مشاوره آنلاین — بیمار و پزشک

## مسیر ارسال (Flow)

```
پرداخت موفق نوبت مشاوره آنلاین
    → AppointmentBookingController (PaymentCallback)
    → transaction.Commit()
    → EnqueueOnlineConsultationRequestToDoctorAsync(appointmentId)
    → EnqueueOnlineConsultationRequestToPatientAsync(appointmentId)
    → NotificationQueue (وضعیت: Queued)
    → Hangfire هر ۱ دقیقه: ProcessNotificationQueue()
    → NotificationQueueProcessor.ProcessPendingAsync()
    → IIdentityMessageService (AsanakSmsService).SendAsync()
    → آسانک REST API
    → وضعیت صف: Sent یا Failed
```

## پیش‌نیازها (چک‌لیست عملیاتی)

| مورد | توضیح | محل تنظیم |
|------|--------|-----------|
| **ماژول مشاوره آنلاین** | فعال باشد | `Jitsi:EnableOnlineConsultation` / `OnlineConsultationServiceCategoryId` در AppSettings |
| **Hangfire** | سرویس و Recurring Job در حال اجرا | `/hangfire` — Job با شناسه `notification-queue-processor` هر دقیقه |
| **آسانک فعال** | ارسال SMS روشن باشد | Web.config: `Asanak:Enabled` = `true` |
| **اعتبار آسانک** | Username و Password معتبر | `Asanak:Username`, `Asanak:Password` |
| **شماره فرستنده** | شماره خط پنل آسانک | `Asanak:SourceNumber` |
| **لینک در پیام** | برای «ورود به اتاق» لازم است | `PaymentBaseUrl` (مثال: `https://clinic.example.com`) |
| **شماره پزشک** | در پروفایل پزشک پر باشد | جدول Doctors — ستون PhoneNumber |
| **شماره بیمار** | در پروفایل بیمار پر باشد | جدول Patients — ستون PhoneNumber |

## عیب‌یابی

- **پیام به پزشک/بیمار نرسید**
  1. داشبورد Hangfire: Jobs → Recurring → `notification-queue-processor` (آخرین اجرا، موفق/ناموفق).
  2. جدول `NotificationQueue`: فیلتر بر اساس `AppointmentId` و `NotificationType` (۶ = پزشک، ۷ = بیمار). وضعیت `Queued`/`Sending` یعنی هنوز پردازش نشده یا در حال ارسال؛ `Failed` با `ErrorLog` دلیل (مثلاً «شماره گیرنده خالی»، «SMS غیرفعال است»، «تنظیمات آسانک ناقص»).
  3. لاگ سرور: جستجو با `Type: OnlineConsultationRequestToDoctor` یا `OnlineConsultationRequestToPatient` و `Recipient` (در لاگ ماسک شده است).
  4. پروفایل پزشک/بیمار: مطمئن شوید `PhoneNumber` ذخیره شده و با فرمت معتبر (مثلاً 09xxxxxxxxx یا +989xxxxxxxxx).

- **پیام در صف می‌ماند (همیشه Queued)**
  - Hangfire را بررسی کنید؛ اگر سرور یا AppPool ریستارت شده باشد، Recurring Job دوباره ثبت می‌شود با راه‌اندازی اپلیکیشن. اطمینان از اجرای مداوم Hangfire Server.

## متن پیام‌ها (نمایشی)

- **پزشک:**  
  `کلینیک شفا | مشاوره آنلاین از [نام بیمار]. لینک ورود به اتاق: [URL]`
- **بیمار:**  
  `کلینیک شفا | نوبت ویزیت آنلاین با دکتر [نام پزشک] ثبت شد. لینک ورود به اتاق: [URL]`

متن‌ها در `NotificationService` (EnqueueOnlineConsultationRequestToDoctorAsync / EnqueueOnlineConsultationRequestToPatientAsync) تولید می‌شوند و با نام کلینیک و لینک مستقیم ورود به اتاق، حس اعتماد و وضوح را تقویت می‌کنند.
