# 📋 TODO List: ماژول گزارشات صندوق (Cashier Reports Module)

**پروژه:** ماژول گزارشات صندوق  
**فاز:** کامل  
**مدت زمان:** 12-18 روز کاری  
**اولویت:** 🔴 CRITICAL  
**طبق:** CRITICAL-FINANCIAL-MODULE-CONTRACT.md, DEVELOPMENT_CONTRACT.md

---

## 📋 Phase 1: ViewModels و Controller (2-3 روز)

### Task 1.1: ایجاد ViewModels

**هدف:** ایجاد ViewModels برای تمام صفحات گزارش

#### Checklist:

- [ ] **1.1.1** ایجاد `CashierReportIndexViewModel`
  ```csharp
  // ViewModels/Payment/CashierReportViewModels.cs
  public class CashierReportIndexViewModel
  {
      public CashierReportFilterViewModel Filter { get; set; }
      public List<SelectListItem> Cashiers { get; set; }
      public ReportType SelectedReportType { get; set; }
  }
  ```

- [ ] **1.1.2** ایجاد `CashierReportFilterViewModel`
  ```csharp
  public class CashierReportFilterViewModel
  {
      [Display(Name = "از تاریخ")]
      public DateTime? StartDate { get; set; }
      
      [Display(Name = "تا تاریخ")]
      public DateTime? EndDate { get; set; }
      
      [Display(Name = "منشی")]
      public string CashierId { get; set; }
      
      [Display(Name = "نوع گزارش")]
      public ReportType ReportType { get; set; }
  }
  ```

- [ ] **1.1.3** ایجاد `CashierDailyReportViewModel`
  ```csharp
  public class CashierDailyReportViewModel
  {
      public CashierDailyReport Report { get; set; }
      public CashierReportFilterViewModel Filter { get; set; }
  }
  ```

- [ ] **1.1.4** ایجاد `CashierMonthlyReportViewModel`
  ```csharp
  public class CashierMonthlyReportViewModel
  {
      public CashierMonthlyReport Report { get; set; }
      public CashierReportFilterViewModel Filter { get; set; }
  }
  ```

- [ ] **1.1.5** ایجاد `CashierRangeReportViewModel`
  ```csharp
  public class CashierRangeReportViewModel
  {
      public CashierDailyReport Report { get; set; }
      public CashierReportFilterViewModel Filter { get; set; }
  }
  ```

- [ ] **1.1.6** ایجاد `CashierAllCashiersSummaryViewModel`
  ```csharp
  public class CashierAllCashiersSummaryViewModel
  {
      public List<CashierSummary> Summaries { get; set; }
      public CashierReportFilterViewModel Filter { get; set; }
      public PagedResult<CashierSummary> PagedResult { get; set; }
  }
  ```

- [ ] **1.1.7** ایجاد `CashierCompareCashiersViewModel`
  ```csharp
  public class CashierCompareCashiersViewModel
  {
      public CashierPerformanceComparison Comparison { get; set; }
      public CashierReportFilterViewModel Filter { get; set; }
      public List<SelectListItem> AvailableCashiers { get; set; }
      public List<string> SelectedCashierIds { get; set; }
  }
  ```

- [ ] **1.1.8** ایجاد Enum `ReportType`
  ```csharp
  public enum ReportType
  {
      [Display(Name = "روزانه")]
      Daily = 1,
      
      [Display(Name = "ماهانه")]
      Monthly = 2,
      
      [Display(Name = "بازه زمانی")]
      Range = 3,
      
      [Display(Name = "همه منشی‌ها")]
      AllCashiers = 4,
      
      [Display(Name = "مقایسه")]
      Compare = 5
  }
  ```

---

### Task 1.2: ایجاد Controller

**هدف:** ایجاد `CashierReportController` با تمام Actions

#### Checklist:

- [ ] **1.2.1** ایجاد Controller Class
  ```csharp
  // Controllers/Payment/CashierReportController.cs
  [Authorize(Roles = AppRoles.Admin + "," + AppRoles.FinancialManager)]
  public class CashierReportController : BaseController
  {
      private readonly ICashierReportService _reportService;
      private readonly ICurrentUserService _currentUserService;
      private readonly ILogger _logger;
      
      public CashierReportController(
          ICashierReportService reportService,
          ICurrentUserService currentUserService,
          ILogger logger) : base(currentUserService, logger)
      {
          _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
          _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
      }
  }
  ```

- [ ] **1.2.2** پیاده‌سازی `Index` Action
  ```csharp
  [HttpGet]
  public async Task<ActionResult> Index()
  {
      try
      {
          var model = new CashierReportIndexViewModel
          {
              Filter = new CashierReportFilterViewModel
              {
                  StartDate = DateTime.Today.AddDays(-7),
                  EndDate = DateTime.Today,
                  ReportType = ReportType.Daily
              },
              Cashiers = await GetCashiersListAsync(),
              SelectedReportType = ReportType.Daily
          };
          
          return View(model);
      }
      catch (Exception ex)
      {
          _logger.Error(ex, "❌ خطا در Index CashierReport");
          NotificationHelper.SetError(TempData, "خطا در بارگذاری صفحه");
          return View(new CashierReportIndexViewModel());
      }
  }
  ```

- [ ] **1.2.3** پیاده‌سازی `DailyReport` (GET) Action
  ```csharp
  [HttpGet]
  public async Task<ActionResult> DailyReport(string cashierId, DateTime? date)
  {
      try
      {
          if (string.IsNullOrWhiteSpace(cashierId))
          {
              NotificationHelper.SetWarning(TempData, "لطفاً منشی را انتخاب کنید");
              return RedirectToAction("Index");
          }
          
          var reportDate = date ?? DateTime.Today;
          
          var result = await _reportService.GetDailyReportAsync(cashierId, reportDate);
          if (!result.Success)
          {
              NotificationHelper.SetError(TempData, result.Message);
              return RedirectToAction("Index");
          }
          
          var model = new CashierDailyReportViewModel
          {
              Report = result.Data,
              Filter = new CashierReportFilterViewModel
              {
                  CashierId = cashierId,
                  StartDate = reportDate,
                  EndDate = reportDate
              }
          };
          
          return View(model);
      }
      catch (Exception ex)
      {
          _logger.Error(ex, "❌ خطا در DailyReport");
          NotificationHelper.SetError(TempData, "خطا در دریافت گزارش روزانه");
          return RedirectToAction("Index");
      }
  }
  ```

- [ ] **1.2.4** پیاده‌سازی `DailyReport` (POST) Action
  ```csharp
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<ActionResult> DailyReport(CashierReportFilterViewModel filter)
  {
      try
      {
          // Parse تاریخ از hidden input
          var date = this.ParseDateFromHiddenInput("StartDate", _logger) ?? DateTime.Today;
          
          if (string.IsNullOrWhiteSpace(filter.CashierId))
          {
              NotificationHelper.SetWarning(TempData, "لطفاً منشی را انتخاب کنید");
              return RedirectToAction("Index");
          }
          
          return RedirectToAction("DailyReport", new { cashierId = filter.CashierId, date = date });
      }
      catch (Exception ex)
      {
          _logger.Error(ex, "❌ خطا در POST DailyReport");
          NotificationHelper.SetError(TempData, "خطا در دریافت گزارش");
          return RedirectToAction("Index");
      }
  }
  ```

- [ ] **1.2.5** پیاده‌سازی `MonthlyReport` (GET) Action
- [ ] **1.2.6** پیاده‌سازی `MonthlyReport` (POST) Action
- [ ] **1.2.7** پیاده‌سازی `RangeReport` (GET) Action
- [ ] **1.2.8** پیاده‌سازی `RangeReport` (POST) Action
- [ ] **1.2.9** پیاده‌سازی `AllCashiersSummary` (GET) Action
- [ ] **1.2.10** پیاده‌سازی `AllCashiersSummary` (POST) Action
- [ ] **1.2.11** پیاده‌سازی `CompareCashiers` (GET) Action
- [ ] **1.2.12** پیاده‌سازی `CompareCashiers` (POST) Action
- [ ] **1.2.13** پیاده‌سازی `ExportToExcel` Action
  ```csharp
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<ActionResult> ExportToExcel(string cashierId, DateTime fromDate, DateTime toDate)
  {
      try
      {
          var result = await _reportService.ExportToExcelAsync(cashierId, fromDate, toDate);
          if (!result.Success)
          {
              NotificationHelper.SetError(TempData, result.Message);
              return RedirectToAction("Index");
          }
          
          var fileName = $"CashierReport_{cashierId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";
          return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
      }
      catch (Exception ex)
      {
          _logger.Error(ex, "❌ خطا در ExportToExcel");
          NotificationHelper.SetError(TempData, "خطا در Export به Excel");
          return RedirectToAction("Index");
      }
  }
  ```

- [ ] **1.2.14** پیاده‌سازی `ExportToPdf` Action
- [ ] **1.2.15** پیاده‌سازی `GetCashiersList` (AJAX) Action
  ```csharp
  [HttpGet]
  public async Task<JsonResult> GetCashiersList()
  {
      try
      {
          var cashiers = await GetCashiersListAsync();
          return Json(cashiers, JsonRequestBehavior.AllowGet);
      }
      catch (Exception ex)
      {
          _logger.Error(ex, "❌ خطا در GetCashiersList");
          return Json(new { success = false, message = "خطا در دریافت لیست منشی‌ها" }, JsonRequestBehavior.AllowGet);
      }
  }
  
  private async Task<List<SelectListItem>> GetCashiersListAsync()
  {
      // دریافت لیست منشی‌ها از Database
      var cashiers = await _context.Users
          .Where(u => u.Roles.Any(r => r.RoleId == "Receptionist") && !u.IsDeleted)
          .Select(u => new SelectListItem
          {
              Value = u.Id,
              Text = u.UserName ?? u.Email ?? "نامشخص"
          })
          .ToListAsync();
      
      return cashiers;
  }
  ```

---

## 📋 Phase 2: Views (3-4 روز)

### Task 2.1: Index View

**هدف:** ایجاد صفحه اصلی گزارش‌ها

#### Checklist:

- [ ] **2.1.1** ایجاد `Index.cshtml`
- [ ] **2.1.2** پیاده‌سازی Search Panel با فیلترها:
  - [ ] تاریخ شروع (Persian DatePicker)
  - [ ] تاریخ پایان (Persian DatePicker)
  - [ ] منشی (DropDown)
  - [ ] نوع گزارش (Radio Buttons)
- [ ] **2.1.3** پیاده‌سازی Quick Actions:
  - [ ] دکمه "گزارش امروز"
  - [ ] دکمه "گزارش این ماه"
  - [ ] دکمه "گزارش هفته جاری"
  - [ ] دکمه "گزارش ماه جاری"
- [ ] **2.1.4** پیاده‌سازی Summary Cards:
  - [ ] Card تعداد کل تراکنش‌ها
  - [ ] Card مبلغ کل
  - [ ] Card تعداد منشی‌ها
  - [ ] Card نرخ موفقیت
- [ ] **2.1.5** اضافه کردن `_PersianDatePicker` برای تاریخ‌ها
- [ ] **2.1.6** اضافه کردن `_PersianDatePickerScript` به Scripts
- [ ] **2.1.7** حذف Alert های Bootstrap
- [ ] **2.1.8** استفاده از Toastr برای Notifications

---

### Task 2.2: DailyReport View

**هدف:** ایجاد صفحه گزارش روزانه

#### Checklist:

- [ ] **2.2.1** ایجاد `DailyReport.cshtml`
- [ ] **2.2.2** پیاده‌سازی Header:
  - [ ] نام منشی
  - [ ] تاریخ گزارش
  - [ ] دکمه Export (Excel, PDF)
- [ ] **2.2.3** پیاده‌سازی Summary Cards:
  - [ ] Card تعداد جلسات
  - [ ] Card تعداد تراکنش‌ها
  - [ ] Card مبلغ کل
  - [ ] Card نرخ موفقیت
- [ ] **2.2.4** پیاده‌سازی جدول جلسات (Sessions)
- [ ] **2.2.5** پیاده‌سازی جدول تراکنش‌ها (Transactions)
- [ ] **2.2.6** پیاده‌سازی جدول اختلاف‌ها (Discrepancies)
- [ ] **2.2.7** اضافه کردن Partial View برای Charts:
  - [ ] `_ReportChartsPartial.cshtml`
- [ ] **2.2.8** پیاده‌سازی Charts:
  - [ ] Pie Chart برای روش پرداخت
  - [ ] Bar Chart برای وضعیت تراکنش‌ها
  - [ ] Line Chart برای تراکنش‌ها بر اساس زمان

---

### Task 2.3: MonthlyReport View

**هدف:** ایجاد صفحه گزارش ماهانه

#### Checklist:

- [ ] **2.3.1** ایجاد `MonthlyReport.cshtml`
- [ ] **2.3.2** پیاده‌سازی Header
- [ ] **2.3.3** پیاده‌سازی Summary Cards
- [ ] **2.3.4** پیاده‌سازی جدول گزارش روزانه
- [ ] **2.3.5** پیاده‌سازی Charts:
  - [ ] Line Chart برای تراکنش‌ها در طول ماه
  - [ ] Area Chart برای مبالغ در طول ماه
  - [ ] Bar Chart برای نرخ موفقیت در طول ماه

---

### Task 2.4: RangeReport View

**هدف:** ایجاد صفحه گزارش بازه زمانی

#### Checklist:

- [ ] **2.4.1** ایجاد `RangeReport.cshtml`
- [ ] **2.4.2** مشابه DailyReport با بازه زمانی

---

### Task 2.5: AllCashiersSummary View

**هدف:** ایجاد صفحه خلاصه تمام منشی‌ها

#### Checklist:

- [ ] **2.5.1** ایجاد `AllCashiersSummary.cshtml`
- [ ] **2.5.2** پیاده‌سازی جدول خلاصه منشی‌ها:
  - [ ] Sortable Columns
  - [ ] Pagination
  - [ ] Search
- [ ] **2.5.3** پیاده‌سازی Charts:
  - [ ] Bar Chart برای مقایسه تعداد تراکنش‌ها
  - [ ] Bar Chart برای مقایسه مبالغ
  - [ ] Bar Chart برای نرخ موفقیت

---

### Task 2.6: CompareCashiers View

**هدف:** ایجاد صفحه مقایسه منشی‌ها

#### Checklist:

- [ ] **2.6.1** ایجاد `CompareCashiers.cshtml`
- [ ] **2.6.2** پیاده‌سازی Multi-Select برای انتخاب منشی‌ها
- [ ] **2.6.3** پیاده‌سازی جدول مقایسه
- [ ] **2.6.4** پیاده‌سازی Charts:
  - [ ] Bar Chart Side-by-Side
  - [ ] Line Chart برای روند

---

### Task 2.7: Partial Views

**هدف:** ایجاد Partial Views برای استفاده مجدد

#### Checklist:

- [ ] **2.7.1** ایجاد `_ReportFiltersPartial.cshtml`
- [ ] **2.7.2** ایجاد `_ReportChartsPartial.cshtml`
- [ ] **2.7.3** ایجاد `_ReportSummaryCardsPartial.cshtml`

---

## 📋 Phase 3: JavaScript و Charts (2-3 روز)

### Task 3.1: JavaScript (cashier-reports.js)

**هدف:** ایجاد JavaScript برای مدیریت گزارش‌ها

#### Checklist:

- [ ] **3.1.1** ایجاد `Scripts/payment/cashier-reports.js`
- [ ] **3.1.2** پیاده‌سازی AJAX Calls:
  - [ ] `loadDailyReport(cashierId, date)`
  - [ ] `loadMonthlyReport(cashierId, year, month)`
  - [ ] `loadRangeReport(cashierId, fromDate, toDate)`
  - [ ] `loadAllCashiersSummary(fromDate, toDate)`
  - [ ] `loadCompareCashiers(cashierIds, fromDate, toDate)`
- [ ] **3.1.3** پیاده‌سازی Form Validation
- [ ] **3.1.4** پیاده‌سازی Date Picker Integration
- [ ] **3.1.5** پیاده‌سازی Quick Actions:
  - [ ] `loadTodayReport()`
  - [ ] `loadThisMonthReport()`
  - [ ] `loadThisWeekReport()`
  - [ ] `loadCurrentMonthReport()`
- [ ] **3.1.6** پیاده‌سازی Error Handling
- [ ] **3.1.7** پیاده‌سازی Loading States
- [ ] **3.1.8** پیاده‌سازی Auto-refresh (اختیاری)

---

### Task 3.2: Charts (cashier-reports-charts.js)

**هدف:** ایجاد JavaScript برای Charts

#### Checklist:

- [ ] **3.2.1** ایجاد `Scripts/payment/cashier-reports-charts.js`
- [ ] **3.2.2** پیاده‌سازی Chart.js Integration
- [ ] **3.2.3** پیاده‌سازی Pie Chart برای روش پرداخت
- [ ] **3.2.4** پیاده‌سازی Bar Chart برای وضعیت تراکنش‌ها
- [ ] **3.2.5** پیاده‌سازی Line Chart برای روند زمانی
- [ ] **3.2.6** پیاده‌سازی Area Chart برای مبالغ
- [ ] **3.2.7** پیاده‌سازی Responsive Charts
- [ ] **3.2.8** پیاده‌سازی RTL Support
- [ ] **3.2.9** پیاده‌سازی Chart Destroy و Recreate

---

## 📋 Phase 4: Export (2-3 روز)

### Task 4.1: Excel Export

**هدف:** پیاده‌سازی Export به Excel

#### Checklist:

- [ ] **4.1.1** نصب NuGet Package (EPPlus یا ClosedXML)
- [ ] **4.1.2** پیاده‌سازی `ExportToExcelAsync` در `CashierReportService`
- [ ] **4.1.3** ایجاد Excel با Header و Footer
- [ ] **4.1.4** Formatting Excel:
  - [ ] Bold Headers
  - [ ] Number Formatting
  - [ ] Date Formatting
  - [ ] Currency Formatting
- [ ] **4.1.5** ایجاد Multiple Sheets (برای گزارش‌های پیچیده)
- [ ] **4.1.6** تست Export برای DailyReport
- [ ] **4.1.7** تست Export برای MonthlyReport
- [ ] **4.1.8** تست Export برای RangeReport
- [ ] **4.1.9** تست Export برای AllCashiersSummary

---

### Task 4.2: PDF Export

**هدف:** پیاده‌سازی Export به PDF

#### Checklist:

- [ ] **4.2.1** نصب NuGet Package (iTextSharp یا QuestPDF)
- [ ] **4.2.2** پیاده‌سازی `ExportToPdfAsync` در `CashierReportService`
- [ ] **4.2.3** ایجاد PDF با Header و Footer
- [ ] **4.2.4** Table Formatting در PDF
- [ ] **4.2.5** Charts به صورت Image در PDF
- [ ] **4.2.6** Page Numbers
- [ ] **4.2.7** تست Export برای DailyReport
- [ ] **4.2.8** تست Export برای MonthlyReport
- [ ] **4.2.9** تست Export برای RangeReport

---

## 📋 Phase 5: UI/UX Optimization (1-2 روز)

### Task 5.1: Design Consistency

**هدف:** بهینه‌سازی UI/UX طبق استانداردهای پروژه

#### Checklist:

- [ ] **5.1.1** استفاده از فونت Vazir
- [ ] **5.1.2** استفاده از Card Components
- [ ] **5.1.3** استفاده از Button Styles
- [ ] **5.1.4** استفاده از Table Styles
- [ ] **5.1.5** رنگ‌بندی استاندارد (Medical Colors)
- [ ] **5.1.6** حذف Gradient های رنگی
- [ ] **5.1.7** Border-radius مناسب (6px-12px)

---

### Task 5.2: Responsive Design

**هدف:** سازگاری با تمام دستگاه‌ها

#### Checklist:

- [ ] **5.2.1** تست Mobile View
- [ ] **5.2.2** تست Tablet View
- [ ] **5.2.3** تست Desktop View
- [ ] **5.2.4** بهینه‌سازی Table Responsive
- [ ] **5.2.5** بهینه‌سازی Charts Responsive

---

### Task 5.3: Accessibility

**هدف:** رعایت استانداردهای دسترسی‌پذیری

#### Checklist:

- [ ] **5.3.1** اضافه کردن Alt Text برای Images
- [ ] **5.3.2** اضافه کردن ARIA Labels
- [ ] **5.3.3** تست Keyboard Navigation
- [ ] **5.3.4** تست Screen Reader

---

## 📋 Phase 6: Testing & Quality Assurance (2-3 روز)

### Task 6.1: Unit Testing

**هدف:** تست Unit برای Controller و Service

#### Checklist:

- [ ] **6.1.1** تست Controller Actions
- [ ] **6.1.2** تست Service Methods
- [ ] **6.1.3** تست ViewModels
- [ ] **6.1.4** تست Validation

---

### Task 6.2: Integration Testing

**هدف:** تست End-to-End

#### Checklist:

- [ ] **6.2.1** تست End-to-End Flows
- [ ] **6.2.2** تست Export Functions
- [ ] **6.2.3** تست Charts Rendering
- [ ] **6.2.4** تست AJAX Calls

---

### Task 6.3: Security Testing

**هدف:** تست امنیت

#### Checklist:

- [ ] **6.3.1** تست Authorization Checks
- [ ] **6.3.2** تست Input Validation
- [ ] **6.3.3** تست SQL Injection Prevention
- [ ] **6.3.4** تست XSS Prevention

---

### Task 6.4: Performance Testing

**هدف:** تست عملکرد

#### Checklist:

- [ ] **6.4.1** تست Page Load Time
- [ ] **6.4.2** تست Database Query Performance
- [ ] **6.4.3** تست Chart Rendering Performance
- [ ] **6.4.4** تست Export Performance

---

## 📋 Phase 7: Navigation & Integration (1 روز)

### Task 7.1: Navigation Menu

**هدف:** اضافه کردن به منوی Admin

#### Checklist:

- [ ] **7.1.1** اضافه کردن "گزارشات صندوق" به `_AdminModulesMenu.cshtml`
- [ ] **7.1.2** اضافه کردن زیرمنوها:
  - [ ] گزارش روزانه
  - [ ] گزارش ماهانه
  - [ ] گزارش بازه زمانی
  - [ ] خلاصه تمام منشی‌ها
  - [ ] مقایسه منشی‌ها

---

## 📋 Phase 8: Documentation (1 روز)

### Task 8.1: Documentation

**هدف:** مستندسازی کامل

#### Checklist:

- [ ] **8.1.1** به‌روزرسانی `CASHIER_REPORTS_ROADMAP.md`
- [ ] **8.1.2** ایجاد User Guide
- [ ] **8.1.3** ایجاد API Documentation
- [ ] **8.1.4** به‌روزرسانی README

---

## ✅ Checklist نهایی

### قبل از Production:

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

---

## 📝 Notes

### نکات مهم:

1. **الزامی:** تمام تغییرات باید طبق `CRITICAL-FINANCIAL-MODULE-CONTRACT.md` انجام شوند
2. **الزامی:** تمام کدها باید طبق `DEVELOPMENT_CONTRACT.md` نوشته شوند
3. **الزامی:** تمام گزارش‌ها باید Log شوند
4. **الزامی:** تمام Export Operations باید Audit شوند
5. **توصیه:** از Caching برای داده‌های ثابت استفاده شود
6. **توصیه:** از Pagination برای جداول بزرگ استفاده شود

### زمان‌بندی پیشنهادی:

- Phase 1: 2-3 روز
- Phase 2: 3-4 روز
- Phase 3: 2-3 روز
- Phase 4: 2-3 روز
- Phase 5: 1-2 روز
- Phase 6: 2-3 روز
- Phase 7: 1 روز
- Phase 8: 1 روز

**کل زمان:** 14-20 روز کاری

---

## ✅ Sign-off

- [ ] تمام مراحل تکمیل شده‌اند
- [ ] تمام Checklist ها بررسی شده‌اند
- [ ] Code Review انجام شده است
- [ ] آماده برای Production است

**تاریخ تکمیل:** ___________  
**تایید کننده:** ___________

---

**نویسنده:** ClinicApp Development Team  
**آخرین به‌روزرسانی:** 1404/10/06  
**وضعیت:** 🚧 در حال پیاده‌سازی

