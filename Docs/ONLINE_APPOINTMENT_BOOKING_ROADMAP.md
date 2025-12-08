# 🏥 نقشه راه ماژول رزرو نوبت آنلاین (Online Appointment Booking)

## 📋 خلاصه اجرایی

این سند نقشه راه کامل برای پیاده‌سازی ماژول رزرو نوبت آنلاین را ارائه می‌دهد. این ماژول به بیماران امکان می‌دهد:
- نوبت‌های گذشته خود را مشاهده کنند
- پزشک مورد نظر را جستجو و انتخاب کنند
- تاریخ و زمان دلخواه را از اسلات‌های موجود انتخاب کنند
- نوبت را رزرو و پرداخت کنند

---

## 🎯 اهداف اصلی

1. **کاربرپسندی (User-Friendly)**: رابط کاربری ساده، واضح و قابل استفاده برای همه سنین
2. **قابلیت اطمینان (Reliability)**: سیستم ضدگلوله با مدیریت خطا و لاگ کامل
3. **عملکرد (Performance)**: بارگذاری سریع و پاسخگویی بهینه
4. **امنیت (Security)**: احراز هویت و محافظت از داده‌های بیمار

---

## 🏗️ معماری کلی

```
Patient Portal (Frontend)
    ↓
Patient Appointment Controller
    ↓
Appointment Booking Service
    ↓
Appointment Repository + DoctorSchedule Repository
    ↓
Database (Appointments, DoctorSchedules, PaymentTransactions)
```

---

## 📦 فازهای پیاده‌سازی

### **فاز 1: زیرساخت و Backend (Infrastructure & Backend)**

#### 1.1. ایجاد Area برای Patient Portal
- [ ] ایجاد `Areas/Patient/`
- [ ] ایجاد `Areas/Patient/Controllers/`
- [ ] ایجاد `Areas/Patient/Views/`
- [ ] ایجاد `Areas/Patient/Views/Shared/_PatientLayout.cshtml`
- [ ] ثبت Area در `Global.asax.cs`

#### 1.2. ایجاد Service Layer
- [ ] ایجاد `IAppointmentBookingService` interface
- [ ] ایجاد `AppointmentBookingService` class
- [ ] متدهای اصلی:
  - [ ] `GetPatientAppointmentsAsync(int patientId, DateTime? startDate, DateTime? endDate)`
  - [ ] `GetAvailableDoctorsAsync(int? departmentId, string searchTerm)`
  - [ ] `GetAvailableTimeSlotsAsync(int doctorId, DateTime date)`
  - [ ] `ReserveAppointmentAsync(AppointmentBookingRequest request)`
  - [ ] `CancelAppointmentAsync(int appointmentId, int patientId)`
  - [ ] `GetAppointmentPriceAsync(int doctorId, int? serviceCategoryId)`

#### 1.3. ایجاد Repository Layer
- [ ] ایجاد `IAppointmentRepository` interface (اگر وجود ندارد)
- [ ] اضافه کردن متدهای لازم به Repository:
  - [ ] `GetPatientAppointmentsAsync(int patientId, DateTime? startDate, DateTime? endDate)`
  - [ ] `GetAppointmentByIdAsync(int appointmentId)`
  - [ ] `CreateAppointmentAsync(Appointment appointment)`
  - [ ] `UpdateAppointmentStatusAsync(int appointmentId, AppointmentStatus status)`
  - [ ] `CheckSlotAvailabilityAsync(int doctorId, DateTime appointmentDate, TimeSpan startTime, TimeSpan endTime)`

#### 1.4. ایجاد DTOs و ViewModels
- [ ] `PatientAppointmentListDto`
- [ ] `DoctorSearchResultDto`
- [ ] `AvailableTimeSlotDto`
- [ ] `AppointmentBookingRequestDto`
- [ ] `AppointmentBookingViewModel`
- [ ] `PatientAppointmentListViewModel`
- [ ] `DoctorSelectionViewModel`
- [ ] `TimeSlotSelectionViewModel`

---

### **فاز 2: API Endpoints**

#### 2.1. Patient Appointment Controller
- [ ] `GET /Patient/Appointment/MyAppointments` - لیست نوبت‌های بیمار
- [ ] `GET /Patient/Appointment/Details/{id}` - جزئیات یک نوبت
- [ ] `POST /Patient/Appointment/Cancel/{id}` - لغو نوبت

#### 2.2. Appointment Booking Controller
- [ ] `GET /Patient/Appointment/Book` - صفحه رزرو نوبت
- [ ] `GET /Patient/Appointment/Book/SelectDoctor` - انتخاب پزشک
- [ ] `GET /Patient/Appointment/Book/SelectDate/{doctorId}` - انتخاب تاریخ
- [ ] `GET /Patient/Appointment/Book/SelectTime/{doctorId}/{date}` - انتخاب زمان
- [ ] `POST /Patient/Appointment/Book/Reserve` - رزرو نوبت
- [ ] `POST /Patient/Appointment/Book/ProcessPayment` - پردازش پرداخت

#### 2.3. API Endpoints (JSON)
- [ ] `GET /api/patient/appointments` - لیست نوبت‌های بیمار (JSON)
- [ ] `GET /api/patient/doctors` - لیست پزشکان قابل رزرو (JSON)
- [ ] `GET /api/patient/doctors/{id}/schedule` - برنامه کاری پزشک (JSON)
- [ ] `GET /api/patient/doctors/{id}/slots/{date}` - اسلات‌های در دسترس (JSON)
- [ ] `POST /api/patient/appointments/reserve` - رزرو نوبت (JSON)
- [ ] `POST /api/patient/appointments/{id}/cancel` - لغو نوبت (JSON)
- [ ] `POST /api/patient/appointments/{id}/payment` - پردازش پرداخت (JSON)

---

### **فاز 3: Frontend - نمایش نوبت‌های گذشته**

#### 3.1. صفحه اصلی نوبت‌های بیمار
- [ ] ایجاد `Areas/Patient/Views/Appointment/MyAppointments.cshtml`
- [ ] طراحی UI:
  - [ ] کارت‌های نوبت با اطلاعات کامل
  - [ ] فیلتر بر اساس تاریخ (گذشته، آینده، همه)
  - [ ] فیلتر بر اساس وضعیت (رزرو شده، لغو شده، تکمیل شده)
  - [ ] جستجو بر اساس نام پزشک
  - [ ] Pagination
- [ ] JavaScript:
  - [ ] `patient-appointments.js` - مدیریت لیست نوبت‌ها
  - [ ] فیلتر و جستجو
  - [ ] نمایش جزئیات نوبت در Modal
  - [ ] لغو نوبت با تایید

#### 3.2. جزئیات نوبت
- [ ] Modal یا صفحه جداگانه برای نمایش جزئیات
- [ ] اطلاعات نمایش داده شده:
  - [ ] نام پزشک و تخصص
  - [ ] تاریخ و زمان نوبت
  - [ ] وضعیت نوبت
  - [ ] مبلغ پرداخت شده
  - [ ] اطلاعات کلینیک
  - [ ] دکمه لغو نوبت (در صورت امکان)

---

### **فاز 4: Frontend - رزرو نوبت**

#### 4.1. صفحه انتخاب پزشک
- [ ] ایجاد `Areas/Patient/Views/Appointment/SelectDoctor.cshtml`
- [ ] طراحی UI:
  - [ ] جستجوی پزشک (نام، تخصص، کد نظام پزشکی)
  - [ ] فیلتر بر اساس بخش (Department)
  - [ ] کارت‌های پزشک با اطلاعات:
    - [ ] عکس پزشک
    - [ ] نام و نام خانوادگی
    - [ ] تخصص
    - [ ] کد نظام پزشکی
    - [ ] برنامه کاری هفتگی
    - [ ] دکمه "انتخاب این پزشک"
- [ ] JavaScript:
  - [ ] `doctor-selection.js` - جستجو و فیلتر
  - [ ] AJAX برای بارگذاری پزشکان
  - [ ] نمایش برنامه کاری در Tooltip یا Modal

#### 4.2. صفحه انتخاب تاریخ
- [ ] ایجاد `Areas/Patient/Views/Appointment/SelectDate.cshtml`
- [ ] طراحی UI:
  - [ ] تقویم فارسی (Persian DatePicker)
  - [ ] نمایش تاریخ‌های در دسترس با رنگ متفاوت
  - [ ] غیرفعال کردن تاریخ‌های گذشته
  - [ ] نمایش تعداد اسلات‌های خالی در هر تاریخ
  - [ ] نمایش تعطیلات رسمی
- [ ] JavaScript:
  - [ ] `date-selection.js` - مدیریت تقویم
  - [ ] AJAX برای دریافت تاریخ‌های در دسترس
  - [ ] اعتبارسنجی انتخاب تاریخ

#### 4.3. صفحه انتخاب زمان
- [ ] ایجاد `Areas/Patient/Views/Appointment/SelectTime.cshtml`
- [ ] طراحی UI:
  - [ ] نمایش اسلات‌های زمانی به صورت Grid
  - [ ] رنگ‌بندی:
    - [ ] سبز: در دسترس
    - [ ] قرمز: رزرو شده
    - [ ] خاکستری: غیرفعال
  - [ ] نمایش زمان به صورت فارسی (قبل از ظهر / بعد از ظهر)
  - [ ] نمایش مدت زمان هر نوبت
  - [ ] دکمه "رزرو این زمان"
- [ ] JavaScript:
  - [ ] `time-selection.js` - مدیریت اسلات‌ها
  - [ ] AJAX برای دریافت اسلات‌های در دسترس
  - [ ] Real-time update اسلات‌ها (برای جلوگیری از double booking)
  - [ ] اعتبارسنجی انتخاب زمان

#### 4.4. صفحه تایید و پرداخت
- [ ] ایجاد `Areas/Patient/Views/Appointment/ConfirmBooking.cshtml`
- [ ] طراحی UI:
  - [ ] خلاصه اطلاعات نوبت:
    - [ ] نام پزشک
    - [ ] تاریخ و زمان
    - [ ] مبلغ نوبت
    - [ ] اطلاعات بیمار
  - [ ] فرم پرداخت:
    - [ ] انتخاب روش پرداخت (آنلاین، نقدی در کلینیک)
    - [ ] درگاه پرداخت آنلاین (در صورت نیاز)
  - [ ] دکمه "تایید و پرداخت"
- [ ] JavaScript:
  - [ ] `booking-confirmation.js` - مدیریت تایید
  - [ ] پردازش پرداخت
  - [ ] نمایش نتیجه رزرو

---

### **فاز 5: Business Logic و Validation**

#### 5.1. اعتبارسنجی رزرو
- [ ] بررسی وجود پزشک
- [ ] بررسی برنامه کاری پزشک
- [ ] بررسی دسترسی‌پذیری اسلات
- [ ] بررسی عدم تداخل با نوبت‌های دیگر
- [ ] بررسی تاریخ (نباید در گذشته باشد)
- [ ] بررسی حداقل زمان رزرو (مثلاً 2 ساعت قبل)

#### 5.2. محاسبه قیمت
- [ ] دریافت قیمت پایه از تنظیمات پزشک
- [ ] اعمال تخفیف (در صورت وجود)
- [ ] محاسبه مالیات (در صورت نیاز)
- [ ] محاسبه نهایی

#### 5.3. مدیریت پرداخت
- [ ] ایجاد `PaymentTransaction`
- [ ] اتصال به درگاه پرداخت (در صورت نیاز)
- [ ] به‌روزرسانی وضعیت نوبت پس از پرداخت موفق
- [ ] مدیریت بازگشت از درگاه پرداخت

#### 5.4. مدیریت نوبت
- [ ] ایجاد `Appointment` با `IsOnlineBooking = true`
- [ ] تنظیم `Status = AppointmentStatus.Scheduled`
- [ ] ثبت `CreatedByUserId` (شناسه بیمار)
- [ ] ارسال پیامک/ایمیل تایید (در صورت نیاز)

---

### **فاز 6: UI/UX Optimization**

#### 6.1. طراحی Responsive
- [ ] Mobile-first approach
- [ ] Breakpoints:
  - [ ] Mobile (< 768px)
  - [ ] Tablet (768px - 1024px)
  - [ ] Desktop (> 1024px)

#### 6.2. فونت‌ها و رنگ‌ها
- [ ] استفاده از فونت‌های محلی (Vazirmatn)
- [ ] پالت رنگ درمانی (آبی، سبز، سفید)
- [ ] کنتراست مناسب برای خوانایی

#### 6.3. انیمیشن‌ها و Transitions
- [ ] Loading states
- [ ] Smooth transitions
- [ ] Skeleton loaders
- [ ] Success/Error animations

#### 6.4. Accessibility
- [ ] ARIA labels
- [ ] Keyboard navigation
- [ ] Screen reader support
- [ ] Focus management

---

### **فاز 7: Testing و Optimization**

#### 7.1. Unit Tests
- [ ] تست Service methods
- [ ] تست Repository methods
- [ ] تست Validation logic

#### 7.2. Integration Tests
- [ ] تست جریان کامل رزرو نوبت
- [ ] تست پرداخت
- [ ] تست لغو نوبت

#### 7.3. Performance Testing
- [ ] تست بارگذاری صفحه
- [ ] تست AJAX calls
- [ ] تست Database queries
- [ ] Optimization queries با Indexes

#### 7.4. Security Testing
- [ ] تست CSRF protection
- [ ] تست Authorization
- [ ] تست Input validation
- [ ] تست SQL Injection prevention

---

## 🔒 ملاحظات امنیتی

1. **احراز هویت**: فقط بیماران لاگین شده می‌توانند نوبت رزرو کنند
2. **Authorization**: بیمار فقط می‌تواند نوبت‌های خودش را مشاهده/لغو کند
3. **CSRF Protection**: تمام POST requests باید `ValidateAntiForgeryToken` داشته باشند
4. **Input Validation**: تمام ورودی‌ها باید validate شوند
5. **Rate Limiting**: محدود کردن تعداد درخواست‌های رزرو در یک بازه زمانی

---

## 📊 معیارهای موفقیت

1. **کاربرپسندی**: 
   - زمان رزرو نوبت < 3 دقیقه
   - نرخ رضایت کاربر > 90%

2. **عملکرد**:
   - زمان بارگذاری صفحه < 2 ثانیه
   - زمان دریافت اسلات‌ها < 1 ثانیه

3. **قابلیت اطمینان**:
   - نرخ موفقیت رزرو > 99%
   - Zero double booking

4. **امنیت**:
   - Zero security vulnerabilities
   - تمام تست‌های امنیتی passed

---

## 📝 یادداشت‌های پیاده‌سازی

### استفاده از کدهای موجود:
- ✅ `Appointment` entity موجود است
- ✅ `DoctorSchedule` module کامل شده است
- ✅ `AppointmentAvailabilityService` موجود است
- ✅ Payment system موجود است
- ✅ Authentication system موجود است

### نیاز به ایجاد:
- ❌ Patient Portal Area
- ❌ `AppointmentBookingService`
- ❌ Patient-facing Views
- ❌ JavaScript modules برای Patient Portal

---

## 🚀 اولویت‌بندی

### **اولویت بالا (Critical)**
1. ایجاد Area و Controller
2. ایجاد Service و Repository methods
3. صفحه نمایش نوبت‌های گذشته
4. صفحه انتخاب پزشک
5. صفحه انتخاب تاریخ و زمان
6. رزرو نوبت و پرداخت

### **اولویت متوسط (Important)**
1. UI/UX Optimization
2. Real-time slot updates
3. پیامک/ایمیل تایید
4. مدیریت لغو نوبت

### **اولویت پایین (Nice to Have)**
1. پیشنهاد پزشکان مشابه
2. تاریخچه نوبت‌های قبلی
3. امتیازدهی به پزشک
4. یادآوری نوبت

---

**تاریخ ایجاد**: 2025-01-08  
**آخرین به‌روزرسانی**: 2025-01-08  
**وضعیت**: در حال برنامه‌ریزی

