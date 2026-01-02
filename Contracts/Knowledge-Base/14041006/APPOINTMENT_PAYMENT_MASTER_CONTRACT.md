# 🎯 قرارداد اصلی: ماژول رزرو نوبت و پرداخت آنلاین (زرین‌پال)

**تاریخ:** 1404/10/06  
**وضعیت:** ✅ **آماده برای پیاده‌سازی**  
**اولویت:** 🚨 **CRITICAL - پردرآمدترین ماژول**  
**نکته کلیدی:** 📱 **90% کاربران از موبایل رزرو می‌کنند!**

---

## 📋 خلاصه قراردادها و نقش‌ها

### ✅ قراردادهای Critical بررسی شده:

#### 1. 🚨 قرارداد ماژول‌های مالی (CRITICAL-FINANCIAL-MODULE-CONTRACT.md)
- ✅ 10 قانون طلایی
- ✅ Checklist قبل از تغییر
- ✅ ممنوعیت‌های مطلق
- ✅ الزامات امنیتی
- ✅ Test Coverage: 95% Minimum
- ✅ Transaction Management الزامی
- ✅ Idempotency برای همه پرداخت‌ها
- ✅ Audit Trail کامل
- ✅ Soft Delete فقط (نه Hard Delete)

#### 2. 📋 قرارداد توسعه (03-Development-Contract-Quick-Guide.md)
- ✅ پالت رنگ استاندارد (`--medical-*`)
- ✅ Strongly-Typed Development
- ✅ Bulletproof Coding
- ✅ SRP Architecture
- ✅ Checklist نهایی
- ✅ Notification System (Toastr + SweetAlert2)
- ✅ Persian DatePicker
- ✅ Image Upload System
- ✅ CKEditor Integration
- ✅ Medical Form Design Standards

#### 3. 📋 راهنمای TODO (04-TODO-Implementation-Guide.md)
- ✅ 13 Phase پیاده‌سازی
- ✅ زمان‌بندی (12-17 روز)
- ✅ Template آماده
- ✅ Checklist هر Phase

#### 4. 🔧 قرارداد دیباگر (05-Debugging-Specialist-Contract.md)
- ✅ فرآیند 6 مرحله‌ای
- ✅ تحلیل علت ریشه‌ای (5 Whys)
- ✅ رفع اتمیک
- ✅ قانون: ممنوع رفع کورکورانه!

#### 5. 🛣️ MVC Routing Best Practices (08-MVC-Routing-Best-Practices.md)
- ✅ ترتیب Routes: خاص قبل از عمومی
- ✅ `UseNamespaceFallback = false`
- ✅ `area = ""` در View

---

## 👥 نقش‌های 7گانه (همزمان)

### 1️⃣ معمار نرم‌افزار ارشد (Senior Software Architect)
**مسئولیت‌ها:**
- ✅ Clean Architecture
- ✅ SOLID Principles
- ✅ Design Patterns (Repository, Service, Factory)
- ✅ Dependency Injection (Unity Container)
- ✅ Separation of Concerns

**قوانین:**
```
Controllers (Presentation)
    ↓
Services (Business Logic)
    ↓
Repositories (Data Access)
    ↓
Entities (Domain Models)
```

---

### 2️⃣ کد ریویوئر خبره (Expert Code Reviewer)
**مسئولیت‌ها:**
- ✅ Code Quality
- ✅ Clean Code
- ✅ Performance Optimization
- ✅ Code Smells Detection

**قوانین:**
- Single Responsibility Principle
- Open/Closed Principle
- Liskov Substitution Principle
- Interface Segregation Principle
- Dependency Inversion Principle

---

### 3️⃣ متخصص ASP.NET MVC
**مسئولیت‌ها:**
- ✅ MVC Pattern
- ✅ Routing
- ✅ ViewModels
- ✅ Model Binding

**قوانین:**
- Controller → فقط Routing و Orchestration
- Service → فقط Business Logic
- Repository → فقط Data Access
- Strongly-Typed ViewModels (نه ViewBag/ViewData)
- `GetViewPath()` در Admin Area

---

### 4️⃣ متخصص امنیت (Security Expert)
**مسئولیت‌ها:**
- ✅ OWASP Top 10
- ✅ Authorization
- ✅ Validation
- ✅ SQL Injection Prevention
- ✅ CSRF Protection

**قوانین:**
- `[ValidateAntiForgeryToken]` برای POST Actions
- Input Validation کامل
- SQL Injection Prevention (EF Core)
- XSS Protection
- Rate Limiting
- Mask کردن داده‌های حساس در Logs

---

### 5️⃣ متخصص سیستم‌های پزشکی (Medical Systems Specialist)
**مسئولیت‌ها:**
- ✅ HIPAA Compliance
- ✅ Data Privacy
- ✅ Audit Trail
- ✅ Soft Delete

**قوانین:**
- Audit Trail کامل (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy)
- Soft Delete (نه Hard Delete)
- Logging کامل تمام عملیات
- Data Privacy و Mask کردن اطلاعات حساس

---

### 6️⃣ متخصص UX (UX Expert)
**مسئولیت‌ها:**
- ✅ User Flow
- ✅ Error Handling
- ✅ Performance از دید کاربر
- ✅ **📱 Mobile-First Design (90% کاربران موبایل!)**

**قوانین:**
- Toastr برای Notifications (نه Alert Bootstrap)
- SweetAlert2 برای Confirmations (نه confirm())
- Persian DatePicker (نه datetime-local)
- Real-time Validation
- **📱 Responsive Design (Mobile-First)**
- **📱 Touch-Friendly UI (دکمه‌های بزرگ، فاصله مناسب)**
- **📱 Performance Optimization (Lazy Loading, Code Splitting)**

---

### 7️⃣ متخصص پایگاه داده (Database Expert)
**مسئولیت‌ها:**
- ✅ Entity Design
- ✅ N+1 Problem
- ✅ Indexing
- ✅ Transaction Management

**قوانین:**
- `decimal(18,0)` برای مبالغ مالی (IRR)
- `[Timestamp]` برای RowVersion (Concurrency)
- Transaction Management برای عملیات مالی
- Indexing برای Query های رایج

---

## 📱 الزامات موبایل (90% کاربران!)

### ✅ Mobile-First Design

#### 1. Responsive Breakpoints:
```css
/* Mobile First Approach */
/* Base: Mobile (< 576px) */
.container {
    padding: 1rem;
    width: 100%;
}

/* Tablet (≥ 576px) */
@media (min-width: 576px) {
    .container {
        padding: 1.5rem;
    }
}

/* Desktop (≥ 992px) */
@media (min-width: 992px) {
    .container {
        padding: 2rem;
        max-width: 1200px;
    }
}
```

#### 2. Touch-Friendly UI:
```css
/* ✅ دکمه‌های بزرگ برای موبایل */
.btn-mobile {
    min-height: 44px; /* حداقل اندازه برای Touch */
    min-width: 44px;
    padding: 0.75rem 1.5rem;
    font-size: 16px; /* جلوگیری از Zoom در iOS */
}

/* ✅ فاصله مناسب بین دکمه‌ها */
.btn-group-mobile {
    gap: 1rem;
}

/* ✅ Input های بزرگ */
.form-control-mobile {
    min-height: 44px;
    font-size: 16px; /* جلوگیری از Zoom */
    padding: 0.75rem;
}
```

#### 3. Performance Optimization:
```javascript
// ✅ Lazy Loading برای تصاویر
<img data-src="image.jpg" class="lazy-load" />

// ✅ Code Splitting
const PaymentModule = lazy(() => import('./PaymentModule'));

// ✅ Debounce برای Search
const debouncedSearch = debounce(handleSearch, 300);

// ✅ Virtual Scrolling برای لیست‌های طولانی
<VirtualList items={appointments} />
```

#### 4. Mobile-Specific Features:
```javascript
// ✅ تشخیص موبایل
const isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);

// ✅ بهینه‌سازی برای موبایل
if (isMobile) {
    // کاهش Animation
    // کاهش تعداد Request ها
    // استفاده از LocalStorage برای Cache
}
```

---

## 🎨 استانداردهای UI/UX

### ✅ رنگ‌بندی:
```css
:root {
    --medical-primary: #2c5aa0;      /* آبی درمانی */
    --medical-secondary: #6c757d;    /* خاکستری */
    --medical-success: #28a745;      /* سبز */
    --medical-danger: #dc3545;       /* قرمز */
    --medical-warning: #ffc107;      /* زرد */
    --medical-info: #17a2b8;         /* آبی روشن */
}
```

### ✅ فونت‌ها:
```css
.medical-form {
    font-family: 'IRANSansX', 'Vazirmatn', 'Dana', 'Shabnam', sans-serif;
    font-size: 16px; /* حداقل برای موبایل */
    line-height: 1.6;
}
```

### ✅ Notifications:
```csharp
// Backend
NotificationHelper.SetSuccess(TempData, "نوبت با موفقیت رزرو شد");
NotificationHelper.SetError(TempData, "خطا در رزرو نوبت");

// Frontend
Swal.fire({
    title: 'آیا از انجام این عملیات اطمینان دارید؟',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'بله',
    cancelButtonText: 'خیر'
});
```

---

## 💰 استانداردهای مالی (CRITICAL!)

### ✅ Data Types:
```csharp
// ✅ درست
public decimal Amount { get; set; }  // decimal برای مبالغ
public decimal Price { get; set; }

// ❌ اشتباه
public float Amount { get; set; }   // ❌ Rounding Error!
public double Price { get; set; }   // ❌ Rounding Error!
```

### ✅ Transaction Management:
```csharp
using (var transaction = _context.Database.BeginTransaction())
{
    try
    {
        // 1. ایجاد OnlinePayment
        _context.OnlinePayments.Add(onlinePayment);
        await _context.SaveChangesAsync();
        
        // 2. به‌روزرسانی Appointment
        appointment.Status = AppointmentStatus.Paid;
        appointment.PaymentTransactionId = paymentTransactionId;
        await _context.SaveChangesAsync();
        
        // 3. Verification
        var saved = await _context.OnlinePayments
            .FirstOrDefaultAsync(p => p.OnlinePaymentId == onlinePayment.OnlinePaymentId);
        
        if (saved == null)
        {
            throw new Exception("Payment was not saved!");
        }
        
        transaction.Commit();
        _logger.Information("✅ PAYMENT: Committed - OnlinePaymentId: {Id}", onlinePayment.OnlinePaymentId);
    }
    catch (Exception ex)
    {
        transaction.Rollback();
        _logger.Error(ex, "❌ PAYMENT: Rollback - OnlinePaymentId: {Id}", onlinePayment.OnlinePaymentId);
        throw;
    }
}
```

### ✅ Idempotency:
```csharp
// ✅ ALWAYS: چک کردن پرداخت تکراری
var idempotencyKey = Guid.NewGuid().ToString();
var existing = await _context.OnlinePayments
    .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);

if (existing != null)
{
    _logger.Warning("⚠️ DUPLICATE: پرداخت قبلاً ثبت شده - IdempotencyKey: {Key}", idempotencyKey);
    return existing; // برگرداندن همان پرداخت قبلی
}
```

### ✅ Logging:
```csharp
_logger.Information("💰 PAYMENT: شروع ثبت پرداخت - AppointmentId: {AppointmentId}, Amount: {Amount}, Gateway: {Gateway}", 
    appointmentId, amount, gatewayType);

// ... عملیات ...

_logger.Information("✅ PAYMENT: ثبت موفق - OnlinePaymentId: {OnlinePaymentId}", onlinePayment.OnlinePaymentId);
```

---

## 📊 الزامات گزارش‌گیری (مدیریت و کنترل)

### ✅ گزارش‌های الزامی:

#### 1. گزارش پرداخت‌های Appointment:
```csharp
public class AppointmentPaymentReportViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SuccessfulAmount { get; set; }
    public decimal FailedAmount { get; set; }
    public decimal SuccessRate { get; set; }
    public List<AppointmentPaymentDetailViewModel> Details { get; set; }
}
```

#### 2. گزارش درآمد روزانه/هفتگی/ماهانه:
```csharp
public class RevenueReportViewModel
{
    public DateTime Date { get; set; }
    public int AppointmentCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRevenue { get; set; }
    public Dictionary<string, decimal> RevenueByDoctor { get; set; }
    public Dictionary<string, decimal> RevenueByService { get; set; }
}
```

#### 3. گزارش وضعیت پرداخت‌ها:
```csharp
public class PaymentStatusReportViewModel
{
    public int PendingPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public int CanceledPayments { get; set; }
    public List<OnlinePayment> RecentPayments { get; set; }
}
```

#### 4. گزارش خطاها و مشکلات:
```csharp
public class PaymentErrorReportViewModel
{
    public int TotalErrors { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; }
    public List<PaymentErrorDetailViewModel> ErrorDetails { get; set; }
}
```

### ✅ کنترل و مدیریت:

#### 1. Dashboard برای مدیر:
```csharp
public class PaymentDashboardViewModel
{
    // آمار کلی
    public int TodayAppointments { get; set; }
    public decimal TodayRevenue { get; set; }
    public int PendingPayments { get; set; }
    
    // نمودارها
    public List<ChartDataPoint> RevenueChart { get; set; }
    public List<ChartDataPoint> PaymentStatusChart { get; set; }
    
    // لیست پرداخت‌های اخیر
    public List<OnlinePayment> RecentPayments { get; set; }
    
    // هشدارها
    public List<PaymentAlertViewModel> Alerts { get; set; }
}
```

#### 2. Actions برای مدیر:
- ✅ مشاهده تمام پرداخت‌ها
- ✅ Retry پرداخت‌های ناموفق
- ✅ Cancel پرداخت‌های Pending
- ✅ Refund پرداخت‌های موفق
- ✅ Export گزارش‌ها به Excel
- ✅ فیلتر و جستجو پیشرفته

---

## 🎯 معماری پیشنهادی

### ✅ ساختار فولدرها:

```
Services/Payment/
├── Gateway/
│   ├── PaymentGatewayService.cs ✅
│   └── Drivers/
│       ├── IGatewayDriver.cs ⚠️ (نیاز به ایجاد)
│       ├── ZarinPalDriver.cs ⚠️ (نیاز به ایجاد)
│       ├── PayPingDriver.cs (آینده)
│       └── IDPayDriver.cs (آینده)
├── Web/
│   └── WebPaymentService.cs ✅ (نیاز به تکمیل)
└── Appointment/
    ├── AppointmentPaymentService.cs ⚠️ (نیاز به ایجاد)
    └── AppointmentPaymentOrchestrator.cs ⚠️ (نیاز به ایجاد)

Areas/Patient/
├── Controllers/
│   └── AppointmentBookingController.cs ✅ (نیاز به تکمیل)
└── Views/
    └── AppointmentBooking/
        ├── Payment.cshtml ⚠️ (نیاز به ایجاد)
        ├── PaymentSuccess.cshtml ⚠️ (نیاز به ایجاد)
        └── PaymentError.cshtml ⚠️ (نیاز به ایجاد)

Areas/Admin/
├── Controllers/
│   └── Payment/
│       └── AppointmentPaymentController.cs ⚠️ (نیاز به ایجاد)
└── Views/
    └── Payment/
        └── AppointmentPayments/
            ├── Index.cshtml ⚠️ (نیاز به ایجاد)
            ├── Details.cshtml ⚠️ (نیاز به ایجاد)
            └── Dashboard.cshtml ⚠️ (نیاز به ایجاد)
```

---

## 📱 Mobile-First Implementation Checklist

### ✅ Design:
- [ ] Mobile-First CSS (Base: Mobile, سپس Desktop)
- [ ] Touch-Friendly UI (دکمه‌های حداقل 44x44px)
- [ ] Font Size حداقل 16px (جلوگیری از Zoom در iOS)
- [ ] Responsive Images (Lazy Loading)
- [ ] Mobile Menu (Hamburger Menu)

### ✅ Performance:
- [ ] Code Splitting
- [ ] Lazy Loading
- [ ] Debounce برای Search
- [ ] Virtual Scrolling برای لیست‌های طولانی
- [ ] Cache Strategy (LocalStorage برای موبایل)

### ✅ UX:
- [ ] Loading States
- [ ] Progress Indicators
- [ ] Error Messages (User-Friendly)
- [ ] Offline Support (در صورت امکان)
- [ ] Push Notifications (برای موبایل)

### ✅ Testing:
- [ ] تست در Chrome DevTools (Mobile Emulation)
- [ ] تست در دستگاه واقعی (iOS + Android)
- [ ] تست Performance (Lighthouse Mobile)
- [ ] تست Touch Events
- [ ] تست در شبکه‌های ضعیف (3G)

---

## 📊 Reporting Requirements Checklist

### ✅ Reports:
- [ ] گزارش پرداخت‌های Appointment
- [ ] گزارش درآمد (روزانه/هفتگی/ماهانه)
- [ ] گزارش وضعیت پرداخت‌ها
- [ ] گزارش خطاها و مشکلات
- [ ] گزارش عملکرد درگاه‌ها

### ✅ Dashboard:
- [ ] آمار کلی (Today, Week, Month)
- [ ] نمودار درآمد
- [ ] نمودار وضعیت پرداخت‌ها
- [ ] لیست پرداخت‌های اخیر
- [ ] هشدارها و اعلان‌ها

### ✅ Management:
- [ ] مشاهده تمام پرداخت‌ها
- [ ] Retry پرداخت‌های ناموفق
- [ ] Cancel پرداخت‌های Pending
- [ ] Refund پرداخت‌های موفق
- [ ] Export به Excel
- [ ] فیلتر و جستجو پیشرفته

---

## ✅ Checklist نهایی قبل از Commit

### 📱 Mobile:
- [ ] Mobile-First Design پیاده‌سازی شده
- [ ] Touch-Friendly UI (دکمه‌های 44x44px)
- [ ] Font Size حداقل 16px
- [ ] Responsive در تمام Breakpoints
- [ ] Performance Optimization (Lazy Loading, Code Splitting)
- [ ] تست در دستگاه واقعی

### 💰 Financial:
- [ ] Transaction Management
- [ ] Idempotency
- [ ] Verification بعد از Save
- [ ] Logging کامل
- [ ] Audit Trail
- [ ] Soft Delete

### 🎨 UI/UX:
- [ ] رنگ‌های استاندارد `--medical-*`
- [ ] فونت Vazir یا IRANSansX
- [ ] Toastr + SweetAlert2
- [ ] Persian DatePicker
- [ ] Real-time Validation

### 📊 Reporting:
- [ ] گزارش‌های الزامی پیاده‌سازی شده
- [ ] Dashboard برای مدیر
- [ ] Actions برای مدیریت
- [ ] Export به Excel

### 🔒 Security:
- [ ] `[ValidateAntiForgeryToken]`
- [ ] Input Validation
- [ ] SQL Injection Prevention
- [ ] XSS Protection
- [ ] Rate Limiting

---

## 🚀 آماده برای پیاده‌سازی

**تمام قراردادها بررسی شد ✅**  
**تمام نقش‌ها حفظ شد ✅**  
**تمام استانداردها یاد گرفته شد ✅**  
**تمام Helpers شناسایی شد ✅**  
**تمام Hard Stop Rules حفظ شد ✅**  
**📱 Mobile-First Design آماده ✅**  
**📊 Reporting Requirements مشخص شد ✅**

---

**آماده برای شروع پیاده‌سازی گام به گام!** 🚀

**منتظر دستور شما هستم...** ⏳

