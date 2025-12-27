# 🗺️ نقشه راه کامل: ماژول گزارشات صندوق (Cashier Reports Module)

**تاریخ ایجاد:** 1404/10/06  
**اولویت:** 🔴 CRITICAL  
**وضعیت:** 🚧 در حال پیاده‌سازی  
**طبق:** CRITICAL-FINANCIAL-MODULE-CONTRACT.md, DEVELOPMENT_CONTRACT.md

---

## 📋 فهرست مطالب

1. [معرفی و اهداف](#معرفی-و-اهداف)
2. [وضعیت فعلی](#وضعیت-فعلی)
3. [معماری پیشنهادی](#معماری-پیشنهادی)
4. [گام‌های پیاده‌سازی](#گام‌های-پیاده‌سازی)
5. [الزامات امنیتی](#الزامات-امنیتی)
6. [الزامات عملکردی](#الزامات-عملکردی)
7. [تعریف انجام (Definition of Done)](#تعریف-انجام)

---

## 🎯 معرفی و اهداف

### هدف اصلی
ایجاد یک ماژول قدرتمند و حرفه‌ای برای گزارش‌گیری از عملکرد منشی‌ها (Cashiers) با رعایت کامل اصول SRP، Best Practices، و قراردادهای Critical Financial Module.

### ویژگی‌های کلیدی
✅ **گزارش روزانه** - گزارش عملکرد یک منشی در یک روز خاص  
✅ **گزارش ماهانه** - گزارش عملکرد یک منشی در یک ماه خاص  
✅ **گزارش بازه زمانی** - گزارش عملکرد در بازه زمانی دلخواه  
✅ **خلاصه تمام منشی‌ها** - مقایسه عملکرد تمام منشی‌ها  
✅ **مقایسه منشی‌ها** - مقایسه عملکرد چند منشی با هم  
✅ **Export به Excel** - خروجی Excel برای گزارش‌ها  
✅ **Export به PDF** - خروجی PDF برای گزارش‌ها  
✅ **نمودارها و Charts** - نمایش بصری داده‌ها با Chart.js  
✅ **فیلتر و جستجو** - فیلتر پیشرفته بر اساس تاریخ، منشی، نوع تراکنش  
✅ **Real-time Updates** - به‌روزرسانی خودکار داده‌ها (اختیاری)

---

## 📊 وضعیت فعلی

### ✅ موجود (Completed)
- ✅ `ICashierReportService` - Interface کامل
- ✅ `CashierReportService` - Service پیاده‌سازی شده
- ✅ DTOs:
  - ✅ `CashierDailyReport`
  - ✅ `CashierMonthlyReport`
  - ✅ `CashierSummary`
  - ✅ `CashierPerformanceComparison`
  - ✅ `CashSessionSummary`
  - ✅ `DiscrepancySummary`
- ✅ Dependency Injection در `UnityConfig.cs`
- ✅ `CashierDashboardController` - برای Dashboard (موجود)

### ❌ نیاز به پیاده‌سازی (To Do)
- ❌ `CashierReportController` - Controller برای Reports
- ❌ ViewModels برای Reports
- ❌ Views برای Reports:
  - ❌ Index (صفحه اصلی گزارش‌ها)
  - ❌ DailyReport
  - ❌ MonthlyReport
  - ❌ RangeReport
  - ❌ AllCashiersSummary
  - ❌ CompareCashiers
- ❌ JavaScript برای Reports
- ❌ Charts (Chart.js) برای نمایش داده‌ها
- ❌ Export به Excel (EPPlus یا ClosedXML)
- ❌ Export به PDF (iTextSharp یا QuestPDF)
- ❌ Navigation Menu Integration

---

## 🏗️ معماری پیشنهادی

### 1. ساختار فایل‌ها

```
Controllers/
  Payment/
    CashierReportController.cs          ← جدید

ViewModels/
  Payment/
    CashierReportViewModels.cs         ← جدید

Views/
  Payment/
    CashierReport/
      Index.cshtml                      ← جدید
      DailyReport.cshtml                ← جدید
      MonthlyReport.cshtml              ← جدید
      RangeReport.cshtml                ← جدید
      AllCashiersSummary.cshtml         ← جدید
      CompareCashiers.cshtml            ← جدید
      _ReportFiltersPartial.cshtml      ← جدید
      _ReportChartsPartial.cshtml       ← جدید

Scripts/
  payment/
    cashier-reports.js                  ← جدید
    cashier-reports-charts.js           ← جدید

Content/
  css/
    cashier-reports.css                 ← جدید

Services/
  Payment/
    CashierReportService.cs             ✅ موجود

Interfaces/
  Payment/
    ICashierReportService.cs            ✅ موجود

Models/
  DTOs/
    Payment/
      CashierDailyReport.cs             ✅ موجود
      CashierMonthlyReport.cs           ✅ موجود
      CashierSummary.cs                 ✅ موجود
      CashierPerformanceComparison.cs  ✅ موجود
```

### 2. الگوی معماری (Clean Architecture)

```
┌─────────────────────────────────────┐
│   Presentation Layer (MVC)         │
│   - CashierReportController         │
│   - Views (Razor)                   │
│   - JavaScript                      │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Business Logic Layer              │
│   - ICashierReportService           │
│   - CashierReportService            │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Data Access Layer                 │
│   - ApplicationDbContext             │
│   - Entity Framework                 │
└─────────────────────────────────────┘
```

### 3. اصول SRP (Single Responsibility Principle)

#### Controller (CashierReportController)
**مسئولیت:** فقط HTTP handling و ViewModel mapping
- ✅ دریافت درخواست‌های HTTP
- ✅ فراخوانی Service
- ✅ تبدیل DTO به ViewModel
- ✅ Return View یا JSON

#### Service (CashierReportService)
**مسئولیت:** Business Logic و محاسبات
- ✅ محاسبه گزارش‌ها
- ✅ Query از Database
- ✅ Validation
- ✅ Error Handling
- ✅ Logging

#### ViewModels
**مسئولیت:** Data Transfer برای UI
- ✅ فقط فیلدهای مورد نیاز UI
- ✅ Data Annotations برای Validation
- ✅ Display Names فارسی

---

## 🚀 گام‌های پیاده‌سازی

### Phase 1: ViewModels و Controller (2-3 روز)

#### 1.1 ایجاد ViewModels
- ✅ `CashierReportIndexViewModel` - برای صفحه اصلی
- ✅ `CashierDailyReportViewModel` - برای گزارش روزانه
- ✅ `CashierMonthlyReportViewModel` - برای گزارش ماهانه
- ✅ `CashierRangeReportViewModel` - برای گزارش بازه زمانی
- ✅ `CashierAllCashiersSummaryViewModel` - برای خلاصه تمام منشی‌ها
- ✅ `CashierCompareCashiersViewModel` - برای مقایسه منشی‌ها
- ✅ `CashierReportFilterViewModel` - برای فیلترها

#### 1.2 ایجاد Controller
- ✅ `CashierReportController` با Actions:
  - ✅ `Index` - صفحه اصلی
  - ✅ `DailyReport` (GET/POST) - گزارش روزانه
  - ✅ `MonthlyReport` (GET/POST) - گزارش ماهانه
  - ✅ `RangeReport` (GET/POST) - گزارش بازه زمانی
  - ✅ `AllCashiersSummary` (GET/POST) - خلاصه تمام منشی‌ها
  - ✅ `CompareCashiers` (GET/POST) - مقایسه منشی‌ها
  - ✅ `ExportToExcel` - Export به Excel
  - ✅ `ExportToPdf` - Export به PDF
  - ✅ `GetCashiersList` (AJAX) - لیست منشی‌ها برای DropDown

### Phase 2: Views (3-4 روز)

#### 2.1 Index View
- ✅ Search Panel با فیلترها:
  - ✅ تاریخ شروع (Persian DatePicker)
  - ✅ تاریخ پایان (Persian DatePicker)
  - ✅ منشی (DropDown)
  - ✅ نوع گزارش (Radio Buttons)
- ✅ Quick Actions:
  - ✅ گزارش امروز
  - ✅ گزارش این ماه
  - ✅ گزارش هفته جاری
  - ✅ گزارش ماه جاری
- ✅ Summary Cards:
  - ✅ تعداد کل تراکنش‌ها
  - ✅ مبلغ کل
  - ✅ تعداد منشی‌ها
  - ✅ نرخ موفقیت

#### 2.2 DailyReport View
- ✅ Header با اطلاعات منشی و تاریخ
- ✅ Summary Cards:
  - ✅ تعداد جلسات
  - ✅ تعداد تراکنش‌ها
  - ✅ مبلغ کل
  - ✅ نرخ موفقیت
- ✅ جدول جلسات (Sessions)
- ✅ جدول تراکنش‌ها (Transactions)
- ✅ جدول اختلاف‌ها (Discrepancies)
- ✅ Charts:
  - ✅ نمودار تراکنش‌ها بر اساس روش پرداخت (Pie Chart)
  - ✅ نمودار تراکنش‌ها بر اساس وضعیت (Bar Chart)
  - ✅ نمودار تراکنش‌ها بر اساس زمان (Line Chart)

#### 2.3 MonthlyReport View
- ✅ Header با اطلاعات منشی و ماه
- ✅ Summary Cards
- ✅ جدول گزارش روزانه (Daily Reports)
- ✅ Charts:
  - ✅ نمودار تراکنش‌ها در طول ماه (Line Chart)
  - ✅ نمودار مبالغ در طول ماه (Area Chart)
  - ✅ نمودار نرخ موفقیت در طول ماه (Bar Chart)

#### 2.4 RangeReport View
- ✅ مشابه DailyReport با بازه زمانی

#### 2.5 AllCashiersSummary View
- ✅ جدول خلاصه تمام منشی‌ها
- ✅ Sortable Columns
- ✅ Pagination
- ✅ Charts:
  - ✅ نمودار مقایسه تعداد تراکنش‌ها (Bar Chart)
  - ✅ نمودار مقایسه مبالغ (Bar Chart)
  - ✅ نمودار نرخ موفقیت (Bar Chart)

#### 2.6 CompareCashiers View
- ✅ انتخاب چند منشی (Multi-Select)
- ✅ جدول مقایسه
- ✅ Charts:
  - ✅ نمودار مقایسه Side-by-Side (Bar Chart)
  - ✅ نمودار روند (Line Chart)

### Phase 3: JavaScript و Charts (2-3 روز)

#### 3.1 JavaScript (cashier-reports.js)
- ✅ AJAX Calls برای دریافت گزارش‌ها
- ✅ Form Validation
- ✅ Date Picker Integration
- ✅ Auto-refresh (اختیاری)
- ✅ Error Handling
- ✅ Loading States

#### 3.2 Charts (cashier-reports-charts.js)
- ✅ Chart.js Integration
- ✅ Pie Chart برای روش پرداخت
- ✅ Bar Chart برای وضعیت تراکنش‌ها
- ✅ Line Chart برای روند زمانی
- ✅ Area Chart برای مبالغ
- ✅ Responsive Charts
- ✅ RTL Support

### Phase 4: Export (2-3 روز)

#### 4.1 Excel Export
- ✅ نصب NuGet Package (EPPlus یا ClosedXML)
- ✅ پیاده‌سازی `ExportToExcelAsync` در Service
- ✅ ایجاد Excel با:
  - ✅ Header و Footer
  - ✅ Formatting
  - ✅ Charts (اختیاری)
  - ✅ Multiple Sheets (برای گزارش‌های پیچیده)

#### 4.2 PDF Export
- ✅ نصب NuGet Package (iTextSharp یا QuestPDF)
- ✅ پیاده‌سازی `ExportToPdfAsync` در Service
- ✅ ایجاد PDF با:
  - ✅ Header و Footer
  - ✅ Table Formatting
  - ✅ Charts (به صورت Image)
  - ✅ Page Numbers

### Phase 5: UI/UX Optimization (1-2 روز)

#### 5.1 Design Consistency
- ✅ استفاده از فونت Vazir
- ✅ استفاده از Card Components
- ✅ استفاده از Button Styles
- ✅ استفاده از Table Styles
- ✅ رنگ‌بندی استاندارد (Medical Colors)

#### 5.2 Responsive Design
- ✅ Mobile View
- ✅ Tablet View
- ✅ Desktop View
- ✅ Table Responsive

#### 5.3 Accessibility
- ✅ Alt Text برای Images
- ✅ ARIA Labels
- ✅ Keyboard Navigation
- ✅ Screen Reader Support

### Phase 6: Testing & Quality Assurance (2-3 روز)

#### 6.1 Unit Testing
- ✅ تست Controller Actions
- ✅ تست Service Methods
- ✅ تست ViewModels

#### 6.2 Integration Testing
- ✅ تست End-to-End Flows
- ✅ تست Export Functions
- ✅ تست Charts Rendering

#### 6.3 Security Testing
- ✅ Authorization Checks
- ✅ Input Validation
- ✅ SQL Injection Prevention
- ✅ XSS Prevention

#### 6.4 Performance Testing
- ✅ Page Load Time
- ✅ Database Query Performance
- ✅ Chart Rendering Performance
- ✅ Export Performance

---

## 🔒 الزامات امنیتی

### 1. Authorization
```csharp
[Authorize(Roles = AppRoles.Admin + "," + AppRoles.FinancialManager)]
public class CashierReportController : BaseController
{
    // فقط Admin و FinancialManager می‌توانند گزارش‌ها را ببینند
}
```

### 2. Input Validation
- ✅ Validation تاریخ‌ها (شروع < پایان)
- ✅ Validation شناسه منشی
- ✅ Validation بازه زمانی (حداکثر 1 سال)
- ✅ Sanitization ورودی‌ها

### 3. Logging
- ✅ Log تمام درخواست‌های گزارش
- ✅ Log Export Operations
- ✅ Log خطاها با جزئیات کامل

### 4. Audit Trail
- ✅ ثبت چه کسی چه گزارشی را مشاهده کرده
- ✅ ثبت Export Operations
- ✅ ثبت فیلترهای استفاده شده

---

## ⚡ الزامات عملکردی

### 1. Page Load Time
- ✅ Index: < 2 ثانیه
- ✅ DailyReport: < 3 ثانیه
- ✅ MonthlyReport: < 5 ثانیه
- ✅ RangeReport: < 5 ثانیه

### 2. Database Query Optimization
- ✅ استفاده از Indexes
- ✅ جلوگیری از N+1 Queries
- ✅ استفاده از `Include` برای Eager Loading
- ✅ Caching برای داده‌های ثابت

### 3. Chart Rendering
- ✅ Charts باید در < 1 ثانیه render شوند
- ✅ Responsive Charts
- ✅ Lazy Loading برای Charts بزرگ

### 4. Export Performance
- ✅ Excel Export: < 5 ثانیه برای 1000 ردیف
- ✅ PDF Export: < 10 ثانیه برای 1000 ردیف

---

## ✅ تعریف انجام (Definition of Done)

### الزامات تکمیل
- [ ] تمام ViewModels ایجاد شده‌اند
- [ ] تمام Controller Actions پیاده‌سازی شده‌اند
- [ ] تمام Views ایجاد شده‌اند
- [ ] JavaScript و Charts پیاده‌سازی شده‌اند
- [ ] Export به Excel پیاده‌سازی شده است
- [ ] Export به PDF پیاده‌سازی شده است
- [ ] UI/UX بهینه شده است
- [ ] Responsive Design پیاده‌سازی شده است
- [ ] Accessibility رعایت شده است
- [ ] تمام Tests پاس شده‌اند
- [ ] Code Review انجام شده است
- [ ] Documentation کامل است
- [ ] Navigation Menu به‌روزرسانی شده است
- [ ] طبق CRITICAL-FINANCIAL-MODULE-CONTRACT.md پیاده‌سازی شده است
- [ ] طبق DEVELOPMENT_CONTRACT.md پیاده‌سازی شده است

### Checklist نهایی
- [ ] تمام کدها طبق استانداردهای پروژه هستند
- [ ] تمام Security Requirements رعایت شده‌اند
- [ ] تمام Performance Requirements برآورده شده‌اند
- [ ] تمام UI/UX Requirements رعایت شده‌اند
- [ ] تمام Tests پاس شده‌اند
- [ ] Documentation کامل است
- [ ] آماده برای Production است

---

## 📚 مراجع

- `Docs/Knowledge-Base/CRITICAL-FINANCIAL-MODULE-CONTRACT.md`
- `Docs/DEVELOPMENT_CONTRACT.md`
- `Contracts/02-Architecture-Guidelines.md`
- `Services/Payment/CashierReportService.cs`
- `Interfaces/Payment/ICashierReportService.cs`
- `Models/DTOs/Payment/`

---

## 🎯 خلاصه

این ماژول یک سیستم گزارش‌گیری کامل و حرفه‌ای برای منشی‌ها است که:
- ✅ طبق اصول SRP طراحی شده است
- ✅ از Best Practices استفاده می‌کند
- ✅ برای محیط Production درمانی بهینه شده است
- ✅ امنیت و Performance را در اولویت قرار می‌دهد
- ✅ UI/UX حرفه‌ای و کاربرپسند دارد

**زمان تخمینی کل:** 12-18 روز کاری  
**اولویت:** 🔴 CRITICAL

---

**نویسنده:** ClinicApp Development Team  
**آخرین به‌روزرسانی:** 1404/10/06  
**وضعیت:** 🚧 در حال پیاده‌سازی

