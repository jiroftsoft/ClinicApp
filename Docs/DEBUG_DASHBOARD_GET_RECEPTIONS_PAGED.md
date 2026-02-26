# دیباگ خطای GET_RECEPTIONS_PAGED_ERROR و داشبورد بیمار

## ✅ رفع خطای «DbContext has been disposed»

برای جلوگیری از خطای **System.InvalidOperationException: The operation cannot be completed because the DbContext has been disposed**، تمام متدهای داشبورد که فقط خواندنی هستند از **context اختصاصی** (ایجاد و dispose داخل همان متد) استفاده می‌کنند و به DbContext درخواست وابسته نیستند:

| محل | متد | توضیح |
|-----|------|--------|
| `PatientService` | `GetPatientReceptionsPagedAsync` | `using (var ctx = new ApplicationDbContext())` |
| `PatientService` | `GetPatientAppointmentsPagedAsync` | همان الگو |
| `PatientService` | `GetPatientUpcomingAppointmentsPagedAsync` | همان الگو |
| `PatientService` | `GetPatientReceptionCountAsync` | همان الگو |
| `AppointmentRepository` | `GetPatientAppointmentCountsAsync` | همان الگو |

---

## محل قرار دادن Breakpoint (گام‌به‌گام)

### ۱. نقطهٔ اول — ورود به متد پذیرش با صفحه‌بندی
- **فایل:** `Services/PatientService.cs`
- **متد:** `GetPatientReceptionsPagedAsync`
- **خط:** ابتدای متد (بلافاصله بعد از `try {`)
- **هدف:** اطمینان از رسیدن درخواست و مقدار `patientId`

### ۲. نقطهٔ دوم — قبل از اجرای کوئری
- **فایل:** `Services/PatientService.cs`
- **متد:** `GetPatientReceptionsPagedAsync`
- **خط:** `var query = _context.Receptions...` (شروع کوئری)
- **هدف:** چک کردن اینکه `_context` null نیست و وضعیت آن درست است

### ۳. نقطهٔ سوم — بعد از CountAsync، قبل از ToListAsync
- **خط:** `int totalItems = await query.CountAsync();` و بلافاصله بعد از آن
- **هدف:** تشخیص اینکه خطا در `CountAsync` رخ می‌دهد یا در `ToListAsync`

### ۴. نقطهٔ چهارم — قبل از Select (ساخت ViewModel)
- **خط:** `var viewModels = receptions.Select(r => new PatientReceptionViewModel { ...`
- **هدف:** اگر اجرا به اینجا رسید، خطا داخل خود `Select` (مثلاً تبدیل تاریخ) است

### ۵. نقطهٔ پنجم — داخل catch
- **خط:** اولین خط داخل `catch (Exception ex)` در `GetPatientReceptionsPagedAsync`
- **هدف:** دیدن نوع و متن استثنا (`ex.GetType().Name`, `ex.Message`, `ex.InnerException`)

---

## لاگ دقیق استثنا

بعد از اعمال تغییرات، در لاگ Serilog به‌ازای هر بار خطا باید خطی شبیه زیر دیده شود:

```
GET_RECEPTIONS_PAGED_ERROR | Type: ... | Message: ... | Inner: ... | StackTrace: ...
```

با این خط می‌توانید **دقیقاً** نوع استثنا و پیام آن را ببینید و علت را پیدا کنید.

---

## مسیرهای فراخوانی

- **داشبورد بیمار (تب خانه):** `PatientDashboardService.GetOverviewAsync` → `GetRecentReceptionsAsync` → `PatientService.GetPatientReceptionsPagedAsync`
- **پرونده پزشکی (تب پذیرش‌ها):** `MedicalRecordApiController.GetReceptions` → سرویس پرونده پزشکی → احتمالاً همان `GetPatientReceptionsPagedAsync` یا متد مشابه

هر دو مسیر در صورت خطا اکنون لاگ تفصیلی می‌نویسند.
