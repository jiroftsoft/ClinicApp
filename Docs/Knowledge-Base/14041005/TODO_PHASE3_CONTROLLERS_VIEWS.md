# ✅ TODO List - Phase 3: Controllers & Views

**پروژه:** بهینه‌سازی صندوق و ردیابی منشی‌ها  
**فاز:** 3.1 - Controllers & Views  
**مدت زمان:** 3-4 روز  
**اولویت:** 🔴 CRITICAL  
**طبق:** CRITICAL-FINANCIAL-MODULE-CONTRACT.md, TODO_TEMPLATE.md

---

## 📋 Task 3.1.1: CashierDashboardController

**هدف:** ایجاد Controller برای Dashboard اصلی منشی‌ها

### **Checklist:**

- [ ] **3.1.1.1** ایجاد Controller Class
  ```csharp
  // Controllers/Payment/CashierDashboardController.cs
  [Authorize(Roles = AppRoles.Admin + "," + AppRoles.FinancialManager)]
  public class CashierDashboardController : BaseController
  {
      private readonly ICashierPerformanceService _performanceService;
      private readonly ICashierReportService _reportService;
      private readonly ICurrentUserService _currentUserService;
      private readonly ILogger _logger;
      
      public CashierDashboardController(
          ICashierPerformanceService performanceService,
          ICashierReportService reportService,
          ICurrentUserService currentUserService,
          ILogger logger) : base(logger)
      {
          _performanceService = performanceService;
          _reportService = reportService;
          _currentUserService = currentUserService;
          _logger = logger;
      }
  }
  ```

- [ ] **3.1.1.2** پیاده‌سازی Index Action
  ```csharp
  public async Task<ActionResult> Index()
  {
      try
      {
          var model = new CashierDashboardViewModel
          {
              SelectedDate = DateTime.Today,
              SelectedCashierId = _currentUserService.UserId // برای منشی‌ها
          };
          
          // دریافت آمار روزانه
          var dailyStats = await GetDailyStatsAsync(DateTime.Today);
          model.DailyStats = dailyStats;
          
          // دریافت Top Performers
          var topPerformers = await _performanceService.GetTopPerformersAsync(
              DateTime.Today.AddDays(-30), 
              DateTime.Today, 
              topN: 5);
          if (topPerformers.Success)
          {
              model.TopPerformers = topPerformers.Data;
          }
          
          return View(model);
      }
      catch (Exception ex)
      {
          _logger.Error(ex, "خطا در بارگذاری Dashboard منشی‌ها");
          NotificationHelper.SetError(TempData, "خطا در بارگذاری Dashboard");
          return RedirectToAction("Index", "Home");
      }
  }
  ```

- [ ] **3.1.1.3** پیاده‌سازی GetDailyStats Action (AJAX)
  ```csharp
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<JsonResult> GetDailyStats(DateTime date, string cashierId = null)
  {
      try
      {
          if (string.IsNullOrEmpty(cashierId))
          {
              cashierId = _currentUserService.UserId;
          }
          
          var metrics = await _performanceService.GetMetricsAsync(cashierId, date);
          if (!metrics.Success)
          {
              return Json(new { success = false, message = metrics.Message });
          }
          
          var stats = new CashierStatsViewModel
          {
              TotalTransactions = metrics.Data.TotalTransactions,
              TotalAmount = metrics.Data.TotalAmount,
              SuccessRate = metrics.Data.SuccessRate,
              AverageTransactionTime = metrics.Data.AverageTransactionTime,
              DiscrepancyCount = metrics.Data.DiscrepancyCount
          };
          
          return Json(new { success = true, data = stats });
      }
      catch (Exception ex)
      {
          _logger.Error(ex, "خطا در دریافت آمار روزانه");
          return Json(new { success = false, message = "خطا در دریافت آمار" });
      }
  }
  ```

- [ ] **3.1.1.4** پیاده‌سازی GetTopPerformers Action (AJAX)
  ```csharp
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<JsonResult> GetTopPerformers(DateTime fromDate, DateTime toDate, int topN = 10)
  {
      try
      {
          var result = await _performanceService.GetTopPerformersAsync(fromDate, toDate, topN);
          if (!result.Success)
          {
              return Json(new { success = false, message = result.Message });
          }
          
          return Json(new { success = true, data = result.Data });
      }
      catch (Exception ex)
      {
          _logger.Error(ex, "خطا در دریافت منشی‌های برتر");
          return Json(new { success = false, message = "خطا در دریافت داده" });
      }
  }
  ```

- [ ] **3.1.1.5** پیاده‌سازی GetCashierRanking Action (AJAX)
  ```csharp
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<JsonResult> GetCashierRanking(string cashierId, DateTime fromDate, DateTime toDate)
  {
      try
      {
          if (string.IsNullOrEmpty(cashierId))
          {
              cashierId = _currentUserService.UserId;
          }
          
          var result = await _performanceService.GetCashierRankingAsync(cashierId, fromDate, toDate);
          if (!result.Success)
          {
              return Json(new { success = false, message = result.Message });
          }
          
          return Json(new { success = true, data = result.Data });
      }
      catch (Exception ex)
      {
          _logger.Error(ex, "خطا در دریافت رتبه منشی");
          return Json(new { success = false, message = "خطا در دریافت داده" });
      }
  }
  ```

- [ ] **3.1.1.6** ثبت Controller در RouteConfig (در صورت نیاز)
- [ ] **3.1.1.7** تست تمام Actions
- [ ] **3.1.1.8** Error Handling کامل
- [ ] **3.1.1.9** Logging کامل

---

## 📋 Task 3.1.2: CashierDashboardViewModel

**هدف:** ایجاد ViewModel برای Dashboard

### **Checklist:**

- [ ] **3.1.2.1** ایجاد CashierDashboardViewModel
  ```csharp
  // ViewModels/Payment/CashierDashboardViewModel.cs
  public class CashierDashboardViewModel
  {
      public DateTime SelectedDate { get; set; }
      public string SelectedCashierId { get; set; }
      public CashierStatsViewModel DailyStats { get; set; }
      public List<CashierRanking> TopPerformers { get; set; }
      public CashierRanking CurrentCashierRanking { get; set; }
      public List<SelectListItem> Cashiers { get; set; }
  }
  ```

- [ ] **3.1.2.2** ایجاد CashierStatsViewModel
  ```csharp
  public class CashierStatsViewModel
  {
      public int TotalTransactions { get; set; }
      public decimal TotalAmount { get; set; }
      public decimal SuccessRate { get; set; }
      public decimal AverageTransactionTime { get; set; }
      public int DiscrepancyCount { get; set; }
      public int SessionsOpened { get; set; }
      public int SessionsClosed { get; set; }
  }
  ```

- [ ] **3.1.2.3** Data Annotations
- [ ] **3.1.2.4** Validation Attributes

---

## 📋 Task 3.1.3: CashierDashboard Views

**هدف:** ایجاد Views برای Dashboard

### **Checklist:**

- [ ] **3.1.3.1** ایجاد Views/CashierDashboard/Index.cshtml
  ```html
  @model ClinicApp.ViewModels.Payment.CashierDashboardViewModel
  
  @{
      ViewBag.Title = "داشبورد منشی‌ها";
      Layout = "~/Views/Shared/_Layout.cshtml";
  }
  
  <div class="container-fluid">
      <div class="row">
          <div class="col-12">
              <h2>داشبورد منشی‌ها</h2>
          </div>
      </div>
      
      <!-- Stats Cards -->
      <div class="row" id="stats-cards">
          @Html.Partial("_StatsPartial", Model.DailyStats)
      </div>
      
      <!-- Charts -->
      <div class="row">
          <div class="col-md-6">
              <canvas id="transactionsChart"></canvas>
          </div>
          <div class="col-md-6">
              <canvas id="performanceChart"></canvas>
          </div>
      </div>
      
      <!-- Top Performers -->
      <div class="row">
          <div class="col-12">
              <div id="top-performers">
                  @Html.Partial("_TopPerformersPartial", Model.TopPerformers)
              </div>
          </div>
      </div>
  </div>
  
  @section Scripts {
      <script src="~/Scripts/cashier/cashier-dashboard.js"></script>
  }
  ```

- [ ] **3.1.3.2** ایجاد Views/CashierDashboard/_StatsPartial.cshtml
- [ ] **3.1.3.3** ایجاد Views/CashierDashboard/_TopPerformersPartial.cshtml
- [ ] **3.1.3.4** Responsive Design
- [ ] **3.1.3.5** RTL Support
- [ ] **3.1.3.6** Loading States
- [ ] **3.1.3.7** Error States

---

## 📋 Task 3.1.4: CashierReportController

**هدف:** ایجاد Controller برای گزارش‌های تراکنش

### **Checklist:**

- [ ] **3.1.4.1** ایجاد Controller Class
  ```csharp
  [Authorize(Roles = AppRoles.Admin + "," + AppRoles.FinancialManager)]
  public class CashierReportController : BaseController
  {
      private readonly ICashierReportService _reportService;
      private readonly ICurrentUserService _currentUserService;
      private readonly ILogger _logger;
  }
  ```

- [ ] **3.1.4.2** پیاده‌سازی Index Action
  ```csharp
  public ActionResult Index()
  {
      var model = new CashierReportIndexViewModel
      {
          StartDate = DateTime.Today.AddDays(-7),
          EndDate = DateTime.Today,
          ReportType = ReportType.Daily
      };
      
      return View(model);
  }
  ```

- [ ] **3.1.4.3** پیاده‌سازی DailyReport Action
  ```csharp
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<ActionResult> DailyReport(string cashierId, DateTime date)
  {
      // Parse تاریخ از hidden input
      date = this.ParseDateFromHiddenInput("Date", _logger) ?? DateTime.Today;
      
      var result = await _reportService.GetDailyReportAsync(cashierId, date);
      if (!result.Success)
      {
          NotificationHelper.SetError(TempData, result.Message);
          return RedirectToAction("Index");
      }
      
      var model = new CashierDailyReportViewModel
      {
          Report = result.Data
      };
      
      return View(model);
  }
  ```

- [ ] **3.1.4.4** پیاده‌سازی MonthlyReport Action
- [ ] **3.1.4.5** پیاده‌سازی RangeReport Action
- [ ] **3.1.4.6** پیاده‌سازی AllCashiersSummary Action
- [ ] **3.1.4.7** پیاده‌سازی CompareCashiers Action
- [ ] **3.1.4.8** پیاده‌سازی ExportToExcel Action
- [ ] **3.1.4.9** پیاده‌سازی ExportToPdf Action

---

## 📋 Task 3.1.5: CashierReport ViewModels

**هدف:** ایجاد ViewModels برای Reports

### **Checklist:**

- [ ] **3.1.5.1** ایجاد CashierReportIndexViewModel
- [ ] **3.1.5.2** ایجاد CashierDailyReportViewModel
- [ ] **3.1.5.3** ایجاد CashierMonthlyReportViewModel
- [ ] **3.1.5.4** ایجاد CashierRangeReportViewModel
- [ ] **3.1.5.5** ایجاد CashierComparisonViewModel
- [ ] **3.1.5.6** Data Annotations
- [ ] **3.1.5.7** Validation Attributes

---

## 📋 Task 3.1.6: CashierReport Views

**هدف:** ایجاد Views برای Reports

### **Checklist:**

- [ ] **3.1.6.1** ایجاد Views/CashierReport/Index.cshtml
- [ ] **3.1.6.2** ایجاد Views/CashierReport/DailyReport.cshtml
- [ ] **3.1.6.3** ایجاد Views/CashierReport/MonthlyReport.cshtml
- [ ] **3.1.6.4** ایجاد Views/CashierReport/RangeReport.cshtml
- [ ] **3.1.6.5** ایجاد Views/CashierReport/AllCashiers.cshtml
- [ ] **3.1.6.6** ایجاد Views/CashierReport/Compare.cshtml
- [ ] **3.1.6.7** استفاده از Persian DatePicker
- [ ] **3.1.6.8** Responsive Design
- [ ] **3.1.6.9** RTL Support

---

## 📋 Task 3.1.7: Navigation & Routing

**هدف:** اضافه کردن به Navigation و Routing

### **Checklist:**

- [ ] **3.1.7.1** اضافه کردن به Menu (در صورت نیاز)
- [ ] **3.1.7.2** اضافه کردن به RouteConfig (در صورت نیاز)
- [ ] **3.1.7.3** اضافه کردن Authorization
- [ ] **3.1.7.4** تست Navigation

---

## 📋 Task 3.1.8: Testing & Validation

**هدف:** تست و اعتبارسنجی

### **Checklist:**

- [ ] **3.1.8.1** تست تمام Actions
- [ ] **3.1.8.2** تست Error Handling
- [ ] **3.1.8.3** تست Validation
- [ ] **3.1.8.4** تست Authorization
- [ ] **3.1.8.5** تست Persian DatePicker
- [ ] **3.1.8.6** تست Responsive Design
- [ ] **3.1.8.7** تست Performance

---

## ✅ **Definition of Done**

- [ ] تمام Controllers ایجاد شده‌اند
- [ ] تمام ViewModels ایجاد شده‌اند
- [ ] تمام Views ایجاد شده‌اند
- [ ] Persian DatePicker استفاده شده است
- [ ] Error Handling کامل است
- [ ] Logging کامل است
- [ ] Authorization کامل است
- [ ] Responsive Design است
- [ ] RTL Support است
- [ ] Build موفق است (0 Error, 0 Warning)
- [ ] تست شده است

---

**تهیه‌کننده:** AI Assistant  
**تاریخ:** 1404/10/05  
**وضعیت:** ⏳ **در انتظار شروع**

