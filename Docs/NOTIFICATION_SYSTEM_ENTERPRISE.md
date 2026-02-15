# Enterprise Notification System — ClinicApp

## ۱) معماری (Architecture)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           TRIGGERS (فقط بعد از Commit)                        │
├─────────────────────────────────────────────────────────────────────────────┤
│  AppointmentBookingService.ReserveAppointmentAsync                           │
│       → transaction.Commit()                                                  │
│       → IAppointmentNotificationQueueService.EnqueueAppointmentBooking...     │
│                                                                               │
│  AppointmentBookingController.PaymentCallback                                 │
│       → transaction.Commit()                                                  │
│       → IAppointmentNotificationQueueService.EnqueuePaymentConfirmation...   │
└─────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  IAppointmentNotificationQueueService (NotificationService)                   │
│  - بارگذاری نوبت + پزشک + بیمار + کلینیک                                     │
│  - ساخت متن از قالب با متغیرهای {{PatientName}}, {{DoctorName}}, ...         │
│  - IdempotencyKey = A{appointmentId}_{NotificationType}_{Channel}           │
│  - در صورت نبود رکورد قبلی با همین کلید → Add to NotificationQueue          │
└─────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  NotificationQueue (جدول صف)                                                 │
│  - Status: Queued | Scheduled | Sending | Sent | Failed                       │
│  - ScheduledTime: null = فوری؛ مقدار = زمان ارسال یادآوری                    │
└─────────────────────────────────────────────────────────────────────────────┘
                                        │
          ┌─────────────────────────────┴─────────────────────────────┐
          ▼                                                             ▼
┌──────────────────────────────┐                    ┌──────────────────────────────────┐
│  Hangfire: ProcessQueue      │                    │  Hangfire: ScheduleReminders       │
│  (هر ۱ دقیقه)                 │                    │  (هر ۱۵ دقیقه)                    │
│  - GetPendingBatch           │                    │  - نوبت‌های 24h، 3h، 30min آینده   │
│  - Send SMS (IIdentityMsg)  │                    │  - EnqueueAppointmentReminderAsync │
│  - Update Sent/Failed        │                    │    برای هر نوع یادآوری             │
└──────────────────────────────┘                    └──────────────────────────────────┘
```

---

## ۲) طراحی دیتابیس (Database Schema)

### جدول NotificationQueue

| ستون | نوع | توضیح |
|------|-----|--------|
| Id | BIGINT IDENTITY | کلید اصلی |
| UserId | NVARCHAR(128) | کاربر مرتبط |
| PatientId | INT | بیمار |
| AppointmentId | INT | نوبت |
| NotificationType | INT | 1=Booking, 2=Payment, 3=Reminder24h, 4=Reminder3h, 5=Reminder30min |
| Title | NVARCHAR(200) | عنوان |
| Message | NVARCHAR(2000) | متن نهایی |
| Channel | INT | 1=Sms, 2=Email, 4=InApp |
| Status | INT | Queued=1, Sending=2, Sent=3, Failed=4, Scheduled=6 |
| RetryCount | INT | تعداد تلاش |
| MaxRetries | INT | حداکثر تلاش (پیش‌فرض 3) |
| ScheduledTime | DATETIME2 | زمان ارسال برنامه‌ریزی‌شده |
| SentTime | DATETIME2 | زمان ارسال واقعی |
| ErrorLog | NVARCHAR(2000) | لاگ خطا |
| IdempotencyKey | NVARCHAR(256) | کلید یکتا برای جلوگیری از ارسال تکراری |
| Recipient | NVARCHAR(100) | شماره/ایمیل گیرنده |
| Subject | NVARCHAR(500) | موضوع (ایمیل) |
| CreatedAt | DATETIME2 | زمان ایجاد |

ایندکس‌ها: Status, ScheduledTime, IdempotencyKey, (AppointmentId, NotificationType, Channel).

**به‌روزرسانی دیتابیس با مایگریشن (پیشنهادی):**
```powershell
Add-Migration AddNotificationQueueTable
Update-Database
```
اسکریپت دستی (در صورت نیاز): `Scripts/sql/Create_NotificationQueue_Table.sql`

---

## ۳) نمونه سرویس و صف

### Enqueue بعد از Commit (بدون ارسال مستقیم)

- `AppointmentBookingService`: بعد از `transaction.Commit()` فراخوانی `EnqueueAppointmentBookingConfirmationAsync`.
- `AppointmentBookingController.PaymentCallback`: بعد از `transaction.Commit()` فراخوانی `EnqueuePaymentConfirmationAsync`.

### پردازش صف (Background)

- `NotificationQueueProcessor.ProcessPendingAsync()`: آیتم‌های Queued با `ScheduledTime == null` یا `<= now` را می‌خواند و با `IIdentityMessageService` (SMS) ارسال می‌کند؛ وضعیت را Sent/Failed به‌روز می‌کند.
- `AppointmentReminderScheduler`: نوبت‌های در بازه 24h، 3h، 30min را پیدا می‌کند و برای هر کدام `EnqueueAppointmentReminderAsync` با نوع مناسب فراخوانی می‌کند (Idempotency جلوگیری از تکرار می‌کند).

---

## ۴) قالب‌های پیام (Production-Level)

### رزرو موفق

```
بیمار گرامی {{PatientName}}

نوبت شما با موفقیت ثبت شد ✅

👨‍⚕️ پزشک: {{DoctorName}}
📅 تاریخ: {{Date}}
⏰ ساعت: {{Time}}
🏥 مرکز: {{Clinic}}

کد پیگیری: {{TrackingCode}}

لطفاً 10 دقیقه قبل از زمان مراجعه حضور داشته باشید.
```

### یادآوری نوبت

```
یادآوری نوبت پزشکی ⏰

{{PatientName}} عزیز
شما در تاریخ {{Date}} ساعت {{Time}} با دکتر {{DoctorName}} نوبت دارید.

📍 {{Clinic}}

در صورت عدم امکان حضور، لطفاً اطلاع دهید.
```

### تأیید پرداخت

```
بیمار گرامی {{PatientName}}

پرداخت نوبت شما با موفقیت انجام شد ✅

👨‍⚕️ پزشک: {{DoctorName}}
📅 تاریخ نوبت: {{Date}}
⏰ ساعت: {{Time}}
🏥 مرکز: {{Clinic}}

کد پیگیری: {{TrackingCode}}

کلینیک شفا
```

متغیرهای پشتیبانی‌شده: `{{PatientName}}`, `{{DoctorName}}`, `{{Specialty}}`, `{{Date}}`, `{{Time}}`, `{{Clinic}}`, `{{ClinicAddress}}`, `{{TrackingCode}}`.

---

## ۵) زمان‌بندی یادآوری (استاندارد پزشکی)

| زمان ارسال | نوع |
|------------|-----|
| 24 ساعت قبل | AppointmentReminder24h |
| 3 ساعت قبل | AppointmentReminder3h |
| 30 دقیقه قبل | AppointmentReminder30min (اختیاری) |

یادآوری‌ها فقط برای نوبت‌های با `Status = Scheduled` و `PatientId != null` زمان‌بندی می‌شوند.

---

## ۶) Hangfire (راه‌اندازی شده)

### پکیج‌ها
- `Hangfire.Core` (1.8.22)
- `Hangfire.SqlServer` (1.8.22)

### پیکربندی (OWIN)
- **`App_Start/Startup.Hangfire.cs`**: اتصال به SQL Server با connection name `DefaultConnection`، استفاده از `HangfireUnityJobActivator` برای resolve کردن Jobها از Unity، دو صف `default` و `notifications`.
- **داشبورد**: مسیر `/hangfire`. در Development و درخواست local بدون لاگین؛ در غیر این صورت فقط نقش **Admin** (`AppRoles.Admin`). فیلتر: `Infrastructure/Hangfire/HangfireAuthorizationFilter.cs`.

### Recurring Jobها (فقط وقتی اپلیکیشن Host است)
| Job | Cron | TimeZone |
|-----|------|----------|
| `NotificationQueueProcessor.ProcessPendingAsync` | هر ۱ دقیقه (`*/1 * * * *`) | Local |
| `AppointmentReminderScheduler.Schedule24HourRemindersAsync` | هر ۱۵ دقیقه | Local |
| `AppointmentReminderScheduler.Schedule3HourRemindersAsync` | هر ۱۵ دقیقه | Local |
| `AppointmentReminderScheduler.Schedule30MinuteRemindersAsync` | هر ۱۵ دقیقه | Local |

### DI (Unity)
- `NotificationQueueProcessor`, `AppointmentReminderScheduler` با `HierarchicalLifetimeManager`.
- Jobها از طریق `HangfireUnityJobActivator` با همان کانتینر Unity resolve می‌شوند.

### فایل‌های مرتبط
- `Infrastructure/Hangfire/HangfireUnityJobActivator.cs` — فعال‌سازی Job با Unity
- `Infrastructure/Hangfire/HangfireAuthorizationFilter.cs` — محدودیت دسترسی داشبورد
- `App_Start/Startup.Hangfire.cs` — پیکربندی Storage، Dashboard، Server، Recurring Jobها

---

## ۷) چک‌لیست Production

- [ ] اجرای مایگریشن: `Add-Migration AddNotificationQueueTable` سپس `Update-Database`
- [ ] اطمینان از ثبت `IAppointmentNotificationQueueService` و `INotificationQueueRepository` در DI
- [ ] فعال‌سازی Background Job (Hangfire یا معادل) برای ProcessQueue و ScheduleReminders
- [ ] بررسی Idempotency: عدم ارسال تکراری برای همان AppointmentId + Type + Channel
- [ ] Retry: حداکثر ۳ بار برای هر آیتم؛ بعد از آن Status = Failed و ErrorLog پر شود
- [ ] لاگ: Serilog برای Enqueue، ارسال، و خطاها
- [ ] (اختیاری) Rate limiting در سرویس SMS برای جلوگیری از مسدودی
- [ ] (اختیاری) پنل ادمین برای مشاهده صف و قالب‌ها

---

## ۸) فایل‌های اضافه‌شده

| فایل | نقش |
|------|-----|
| `Models/Enums/NotificationType.cs` | نوع اعلان (Booking, Payment, Reminder24h, …) |
| `Models/Entities/Notification/NotificationQueueItem.cs` | موجودیت و Config صف |
| `Interfaces/Notification/INotificationQueueRepository.cs` | ریپوزیتوری صف |
| `Interfaces/Notification/INotificationService.cs` | IAppointmentNotificationQueueService — سرویس Enqueue |
| `Repositories/Notification/NotificationQueueRepository.cs` | پیاده‌سازی ریپوزیتوری صف |
| `Services/Notification/NotificationTemplateEngine.cs` | جایگزینی {{Var}} در قالب |
| `Services/Notification/NotificationService.cs` | Enqueue رزرو، پرداخت، یادآوری |
| `Services/Notification/NotificationQueueProcessor.cs` | ارسال از صف (SMS) |
| `Services/Notification/AppointmentReminderScheduler.cs` | زمان‌بندی یادآوری 24h/3h/30min |
| `Scripts/sql/Create_NotificationQueue_Table.sql` | ایجاد جدول NotificationQueue |
| `Infrastructure/Hangfire/HangfireUnityJobActivator.cs` | Job Activator برای resolve از Unity در Hangfire |
| `Infrastructure/Hangfire/HangfireAuthorizationFilter.cs` | محدودیت دسترسی داشبورد (Admin / Local) |
| `App_Start/Startup.Hangfire.cs` | پیکربندی Hangfire در OWIN، Dashboard، Recurring Jobها |

تغییرات در سرویس/کنترلر موجود:
- `AppointmentBookingService`: تزریق `IAppointmentNotificationQueueService`، بعد از Commit فراخوانی `EnqueueAppointmentBookingConfirmationAsync`.
- `AppointmentBookingController`: تزریق `IAppointmentNotificationQueueService`، بعد از Commit در PaymentCallback فراخوانی `EnqueuePaymentConfirmationAsync`.
