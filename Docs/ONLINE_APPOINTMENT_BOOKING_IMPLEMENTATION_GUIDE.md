# 🏥 راهنمای اجرای ماژول رزرو نوبت آنلاین

## 📌 مرور کلی

این راهنما مراحل عملی پیاده‌سازی ماژول رزرو نوبت آنلاین را به صورت گام به گام ارائه می‌دهد.

---

## 🎯 هدف

ایجاد یک سیستم کامل و کاربرپسند برای رزرو نوبت آنلاین که به بیماران امکان می‌دهد:
1. نوبت‌های گذشته خود را مشاهده کنند
2. پزشک مورد نظر را جستجو و انتخاب کنند
3. تاریخ و زمان دلخواه را از اسلات‌های موجود انتخاب کنند
4. نوبت را رزرو و پرداخت کنند

---

## 📋 مراحل اجرا

### **مرحله 1: ایجاد زیرساخت (Infrastructure)**

#### 1.1. ایجاد Patient Area

```bash
# ساختار دایرکتوری:
Areas/
  Patient/
    Controllers/
      AppointmentController.cs
      AppointmentBookingController.cs
    Views/
      Appointment/
        MyAppointments.cshtml
        SelectDoctor.cshtml
        SelectDate.cshtml
        SelectTime.cshtml
        ConfirmBooking.cshtml
      Shared/
        _PatientLayout.cshtml
        _PatientNavbar.cshtml
```

**فایل‌های مورد نیاز:**
- `Areas/Patient/PatientAreaRegistration.cs`
- `Areas/Patient/Views/_ViewStart.cshtml`
- `Areas/Patient/Views/Web.config`

#### 1.2. ثبت Area در Global.asax.cs

```csharp
AreaRegistration.RegisterAllAreas();
```

---

### **مرحله 2: ایجاد Service Layer**

#### 2.1. Interface

**فایل:** `Interfaces/IAppointmentBookingService.cs`

```csharp
public interface IAppointmentBookingService
{
    Task<ServiceResult<List<PatientAppointmentDto>>> GetPatientAppointmentsAsync(
        int patientId, 
        DateTime? startDate = null, 
        DateTime? endDate = null);
    
    Task<ServiceResult<List<DoctorSearchResultDto>>> GetAvailableDoctorsAsync(
        int? departmentId = null, 
        string searchTerm = null);
    
    Task<ServiceResult<List<AvailableTimeSlotDto>>> GetAvailableTimeSlotsAsync(
        int doctorId, 
        DateTime date);
    
    Task<ServiceResult<Appointment>> ReserveAppointmentAsync(
        AppointmentBookingRequest request);
    
    Task<ServiceResult> CancelAppointmentAsync(int appointmentId, int patientId);
    
    Task<ServiceResult<decimal>> GetAppointmentPriceAsync(
        int doctorId, 
        int? serviceCategoryId = null);
}
```

#### 2.2. Implementation

**فایل:** `Services/Appointment/AppointmentBookingService.cs`

**وابستگی‌ها:**
- `IAppointmentRepository`
- `IDoctorScheduleRepository`
- `IDoctorCrudService`
- `IPaymentService`
- `ICurrentUserService`
- `ILogger`

---

### **مرحله 3: ایجاد Repository Methods**

#### 3.1. اضافه کردن متدها به `IAppointmentRepository`

```csharp
Task<List<Appointment>> GetPatientAppointmentsAsync(
    int patientId, 
    DateTime? startDate, 
    DateTime? endDate);

Task<Appointment> GetAppointmentByIdAsync(int appointmentId);

Task<Appointment> CreateAppointmentAsync(Appointment appointment);

Task<bool> CheckSlotAvailabilityAsync(
    int doctorId, 
    DateTime appointmentDate, 
    TimeSpan startTime, 
    TimeSpan endTime);
```

#### 3.2. پیاده‌سازی در `AppointmentRepository`

---

### **مرحله 4: ایجاد DTOs و ViewModels**

#### 4.1. DTOs

**فایل:** `Models/DTOs/Appointment/PatientAppointmentDto.cs`
```csharp
public class PatientAppointmentDto
{
    public int AppointmentId { get; set; }
    public string DoctorName { get; set; }
    public string DoctorSpecialization { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string AppointmentTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public decimal Price { get; set; }
    public string ClinicName { get; set; }
}
```

**فایل:** `Models/DTOs/Appointment/DoctorSearchResultDto.cs`
```csharp
public class DoctorSearchResultDto
{
    public int DoctorId { get; set; }
    public string FullName { get; set; }
    public string Specialization { get; set; }
    public string MedicalCouncilCode { get; set; }
    public string DepartmentName { get; set; }
    public bool HasActiveSchedule { get; set; }
}
```

**فایل:** `Models/DTOs/Appointment/AvailableTimeSlotDto.cs`
```csharp
public class AvailableTimeSlotDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public string DisplayTime { get; set; } // "07:30 قبل از ظهر"
}
```

**فایل:** `Models/DTOs/Appointment/AppointmentBookingRequestDto.cs`
```csharp
public class AppointmentBookingRequestDto
{
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int? ServiceCategoryId { get; set; }
    public string Description { get; set; }
}
```

#### 4.2. ViewModels

**فایل:** `ViewModels/Patient/PatientAppointmentListViewModel.cs`
```csharp
public class PatientAppointmentListViewModel
{
    public List<PatientAppointmentDto> Appointments { get; set; }
    public DateTime? StartDateFilter { get; set; }
    public DateTime? EndDateFilter { get; set; }
    public AppointmentStatus? StatusFilter { get; set; }
    public string SearchTerm { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
```

**فایل:** `ViewModels/Patient/DoctorSelectionViewModel.cs`
```csharp
public class DoctorSelectionViewModel
{
    public List<DoctorSearchResultDto> Doctors { get; set; }
    public int? SelectedDepartmentId { get; set; }
    public string SearchTerm { get; set; }
    public List<DepartmentDto> Departments { get; set; }
}
```

**فایل:** `ViewModels/Patient/TimeSlotSelectionViewModel.cs`
```csharp
public class TimeSlotSelectionViewModel
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; }
    public DateTime SelectedDate { get; set; }
    public List<AvailableTimeSlotDto> AvailableSlots { get; set; }
    public int AppointmentDuration { get; set; }
}
```

**فایل:** `ViewModels/Patient/AppointmentBookingViewModel.cs`
```csharp
public class AppointmentBookingViewModel
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; }
    public string DoctorSpecialization { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal Price { get; set; }
    public int? ServiceCategoryId { get; set; }
    public string Description { get; set; }
}
```

---

### **مرحله 5: ایجاد Controllers**

#### 5.1. Patient Appointment Controller

**فایل:** `Areas/Patient/Controllers/AppointmentController.cs`

```csharp
[Authorize]
public class AppointmentController : Controller
{
    private readonly IAppointmentBookingService _bookingService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    // GET: /Patient/Appointment/MyAppointments
    public async Task<ActionResult> MyAppointments(
        DateTime? startDate, 
        DateTime? endDate, 
        AppointmentStatus? status,
        string searchTerm,
        int page = 1)
    {
        // Implementation
    }

    // GET: /Patient/Appointment/Details/{id}
    public async Task<ActionResult> Details(int id)
    {
        // Implementation
    }

    // POST: /Patient/Appointment/Cancel/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Cancel(int id)
    {
        // Implementation
    }
}
```

#### 5.2. Appointment Booking Controller

**فایل:** `Areas/Patient/Controllers/AppointmentBookingController.cs`

```csharp
[Authorize]
public class AppointmentBookingController : Controller
{
    private readonly IAppointmentBookingService _bookingService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    // GET: /Patient/Appointment/Book
    public ActionResult Book()
    {
        return RedirectToAction("SelectDoctor");
    }

    // GET: /Patient/Appointment/Book/SelectDoctor
    public async Task<ActionResult> SelectDoctor(int? departmentId, string searchTerm)
    {
        // Implementation
    }

    // GET: /Patient/Appointment/Book/SelectDate/{doctorId}
    public ActionResult SelectDate(int doctorId)
    {
        // Implementation
    }

    // GET: /Patient/Appointment/Book/SelectTime/{doctorId}/{date}
    public async Task<ActionResult> SelectTime(int doctorId, DateTime date)
    {
        // Implementation
    }

    // POST: /Patient/Appointment/Book/Reserve
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Reserve(AppointmentBookingViewModel model)
    {
        // Implementation
    }

    // POST: /Patient/Appointment/Book/ProcessPayment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ProcessPayment(int appointmentId, PaymentMethod method)
    {
        // Implementation
    }
}
```

---

### **مرحله 6: ایجاد API Endpoints**

#### 6.1. Patient Appointment API Controller

**فایل:** `Areas/Patient/Controllers/Api/PatientAppointmentApiController.cs`

```csharp
[Authorize]
[Route("api/patient/appointments")]
public class PatientAppointmentApiController : Controller
{
    // GET: /api/patient/appointments
    [HttpGet]
    public async Task<JsonResult> GetAppointments(
        DateTime? startDate, 
        DateTime? endDate, 
        AppointmentStatus? status)
    {
        // Implementation
    }

    // GET: /api/patient/appointments/{id}
    [HttpGet("{id}")]
    public async Task<JsonResult> GetAppointment(int id)
    {
        // Implementation
    }

    // POST: /api/patient/appointments/{id}/cancel
    [HttpPost("{id}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> CancelAppointment(int id)
    {
        // Implementation
    }
}
```

#### 6.2. Appointment Booking API Controller

**فایل:** `Areas/Patient/Controllers/Api/AppointmentBookingApiController.cs`

```csharp
[Authorize]
[Route("api/patient")]
public class AppointmentBookingApiController : Controller
{
    // GET: /api/patient/doctors
    [HttpGet("doctors")]
    public async Task<JsonResult> GetDoctors(int? departmentId, string searchTerm)
    {
        // Implementation
    }

    // GET: /api/patient/doctors/{id}/schedule
    [HttpGet("doctors/{id}/schedule")]
    public async Task<JsonResult> GetDoctorSchedule(int id)
    {
        // Implementation
    }

    // GET: /api/patient/doctors/{id}/slots/{date}
    [HttpGet("doctors/{id}/slots/{date}")]
    public async Task<JsonResult> GetAvailableSlots(int id, DateTime date)
    {
        // Implementation
    }

    // POST: /api/patient/appointments/reserve
    [HttpPost("appointments/reserve")]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> ReserveAppointment(AppointmentBookingRequestDto request)
    {
        // Implementation
    }

    // POST: /api/patient/appointments/{id}/payment
    [HttpPost("appointments/{id}/payment")]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> ProcessPayment(int id, PaymentMethod method)
    {
        // Implementation
    }
}
```

---

### **مرحله 7: ایجاد Views**

#### 7.1. Layout

**فایل:** `Areas/Patient/Views/Shared/_PatientLayout.cshtml`

- استفاده از فونت‌های محلی (Vazirmatn)
- پالت رنگ درمانی
- Navbar برای Patient Portal
- Footer

#### 7.2. My Appointments

**فایل:** `Areas/Patient/Views/Appointment/MyAppointments.cshtml`

**ویژگی‌ها:**
- کارت‌های نوبت با اطلاعات کامل
- فیلتر بر اساس تاریخ و وضعیت
- جستجو بر اساس نام پزشک
- Pagination
- Modal برای جزئیات نوبت

#### 7.3. Select Doctor

**فایل:** `Areas/Patient/Views/Appointment/SelectDoctor.cshtml`

**ویژگی‌ها:**
- جستجوی پزشک
- فیلتر بر اساس بخش
- کارت‌های پزشک با اطلاعات کامل
- نمایش برنامه کاری

#### 7.4. Select Date

**فایل:** `Areas/Patient/Views/Appointment/SelectDate.cshtml`

**ویژگی‌ها:**
- تقویم فارسی
- نمایش تاریخ‌های در دسترس
- غیرفعال کردن تاریخ‌های گذشته
- نمایش تعطیلات

#### 7.5. Select Time

**فایل:** `Areas/Patient/Views/Appointment/SelectTime.cshtml`

**ویژگی‌ها:**
- Grid نمایش اسلات‌ها
- رنگ‌بندی (سبز/قرمز/خاکستری)
- نمایش زمان به فارسی
- Real-time update

#### 7.6. Confirm Booking

**فایل:** `Areas/Patient/Views/Appointment/ConfirmBooking.cshtml`

**ویژگی‌ها:**
- خلاصه اطلاعات نوبت
- فرم پرداخت
- دکمه تایید

---

### **مرحله 8: ایجاد JavaScript Modules**

#### 8.1. Patient Appointments

**فایل:** `Scripts/patient/appointment/patient-appointments.js`

**عملکردها:**
- بارگذاری لیست نوبت‌ها
- فیلتر و جستجو
- نمایش جزئیات در Modal
- لغو نوبت

#### 8.2. Doctor Selection

**فایل:** `Scripts/patient/appointment/doctor-selection.js`

**عملکردها:**
- جستجوی پزشک
- فیلتر بر اساس بخش
- AJAX برای بارگذاری پزشکان
- نمایش برنامه کاری

#### 8.3. Date Selection

**فایل:** `Scripts/patient/appointment/date-selection.js`

**عملکردها:**
- مدیریت تقویم فارسی
- دریافت تاریخ‌های در دسترس
- اعتبارسنجی

#### 8.4. Time Selection

**فایل:** `Scripts/patient/appointment/time-selection.js`

**عملکردها:**
- دریافت اسلات‌های در دسترس
- Real-time update
- اعتبارسنجی
- جلوگیری از double booking

#### 8.5. Booking Confirmation

**فایل:** `Scripts/patient/appointment/booking-confirmation.js`

**عملکردها:**
- مدیریت تایید
- پردازش پرداخت
- نمایش نتیجه

---

### **مرحله 9: Business Logic**

#### 9.1. اعتبارسنجی رزرو

**فایل:** `Services/Appointment/AppointmentBookingValidationService.cs`

**بررسی‌ها:**
- وجود پزشک
- برنامه کاری پزشک
- دسترسی‌پذیری اسلات
- عدم تداخل
- تاریخ معتبر
- حداقل زمان رزرو

#### 9.2. محاسبه قیمت

**فایل:** `Services/Appointment/AppointmentPricingService.cs`

**محاسبات:**
- قیمت پایه
- تخفیف
- مالیات
- نهایی

#### 9.3. مدیریت پرداخت

**استفاده از:**
- `IPaymentService`
- `PosPaymentOrchestrator`
- ایجاد `PaymentTransaction`
- به‌روزرسانی وضعیت نوبت

---

### **مرحله 10: UI/UX Optimization**

#### 10.1. Responsive Design
- Mobile-first
- Breakpoints
- Touch-friendly

#### 10.2. فونت‌ها و رنگ‌ها
- Vazirmatn
- پالت درمانی
- کنتراست

#### 10.3. انیمیشن‌ها
- Loading states
- Transitions
- Success/Error animations

---

## 🔒 ملاحظات امنیتی

1. **Authorization**: فقط بیماران لاگین شده
2. **CSRF Protection**: `ValidateAntiForgeryToken` در تمام POST requests
3. **Input Validation**: اعتبارسنجی تمام ورودی‌ها
4. **Rate Limiting**: محدود کردن درخواست‌ها
5. **SQL Injection Prevention**: استفاده از Parameterized Queries

---

## 📊 معیارهای موفقیت

1. **کاربرپسندی**: زمان رزرو < 3 دقیقه
2. **عملکرد**: زمان بارگذاری < 2 ثانیه
3. **قابلیت اطمینان**: نرخ موفقیت > 99%
4. **امنیت**: Zero vulnerabilities

---

**تاریخ ایجاد**: 2025-01-08  
**وضعیت**: آماده برای اجرا

