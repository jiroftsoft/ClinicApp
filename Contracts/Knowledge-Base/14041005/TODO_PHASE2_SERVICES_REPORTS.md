# ✅ TODO List - Phase 2: Services & Reports

**پروژه:** بهینه‌سازی صندوق و ردیابی منشی‌ها  
**فاز:** 2 - Services & Reports  
**مدت زمان:** 3-4 روز  
**اولویت:** 🔴 CRITICAL  
**طبق:** CRITICAL-FINANCIAL-MODULE-CONTRACT.md

---

## 📋 Task 2.1: CashierReportService

**هدف:** ایجاد Service برای گزارش‌گیری از عملکرد منشی‌ها

### **Checklist:**

- [ ] **2.1.1** ایجاد Interface
  ```csharp
  // Interfaces/Payment/ICashierReportService.cs
  public interface ICashierReportService
  {
      Task<ServiceResult<CashierDailyReport>> GetDailyReportAsync(string cashierId, DateTime date);
      Task<ServiceResult<CashierMonthlyReport>> GetMonthlyReportAsync(string cashierId, int year, int month);
      Task<ServiceResult<List<CashierSummary>>> GetAllCashiersSummaryAsync(DateTime fromDate, DateTime toDate);
      Task<ServiceResult<CashierPerformanceComparison>> CompareCashiersAsync(List<string> cashierIds, DateTime fromDate, DateTime toDate);
      Task<ServiceResult<byte[]>> ExportToExcelAsync(string cashierId, DateTime fromDate, DateTime toDate);
      Task<ServiceResult<byte[]>> ExportToPdfAsync(string cashierId, DateTime fromDate, DateTime toDate);
  }
  ```

- [ ] **2.1.2** ایجاد DTOs
  ```csharp
  // Models/DTOs/Payment/CashierDailyReport.cs
  public class CashierDailyReport
  {
      public string CashierId { get; set; }
      public string CashierName { get; set; }
      public DateTime Date { get; set; }
      
      // Sessions
      public int SessionsOpened { get; set; }
      public int SessionsClosed { get; set; }
      public List<CashSessionSummary> Sessions { get; set; }
      
      // Transactions
      public int TotalTransactions { get; set; }
      public int PosTransactions { get; set; }
      public int CashTransactions { get; set; }
      public decimal TotalAmount { get; set; }
      public decimal PosAmount { get; set; }
      public decimal CashAmount { get; set; }
      
      // Performance
      public decimal AverageTransactionTime { get; set; }
      public decimal SuccessRate { get; set; }
      
      // Discrepancies
      public int DiscrepancyCount { get; set; }
      public decimal TotalDiscrepancy { get; set; }
      public List<DiscrepancySummary> Discrepancies { get; set; }
  }
  ```

- [ ] **2.1.3** ایجاد Service Class
  ```csharp
  // Services/Payment/CashierReportService.cs
  public class CashierReportService : ICashierReportService
  {
      private readonly ApplicationDbContext _context;
      private readonly ILogger _logger;
      private readonly ICurrentUserService _currentUserService;
      
      // Implementation...
  }
  ```

- [ ] **2.1.4** پیاده‌سازی `GetDailyReportAsync`
  ```csharp
  public async Task<ServiceResult<CashierDailyReport>> GetDailyReportAsync(string cashierId, DateTime date)
  {
      try
      {
          _logger.Information("📊 Getting daily report for Cashier: {CashierId}, Date: {Date}", cashierId, date);
          
          var startOfDay = date.Date;
          var endOfDay = startOfDay.AddDays(1);
          
          // Get Sessions
          var sessions = await _context.CashSessions
              .Include(cs => cs.Transactions)
              .Where(cs => cs.UserId == cashierId &&
                           cs.OpenedAt >= startOfDay &&
                           cs.OpenedAt < endOfDay &&
                           !cs.IsDeleted)
              .ToListAsync();
          
          // Get Transactions
          var transactions = sessions.SelectMany(s => s.Transactions).ToList();
          
          // Calculate Metrics
          var report = new CashierDailyReport
          {
              CashierId = cashierId,
              Date = date,
              SessionsOpened = sessions.Count,
              SessionsClosed = sessions.Count(s => s.Status == CashSessionStatus.Closed),
              TotalTransactions = transactions.Count,
              PosTransactions = transactions.Count(t => t.Method == PaymentMethod.POS),
              CashTransactions = transactions.Count(t => t.Method == PaymentMethod.Cash),
              TotalAmount = transactions.Sum(t => t.Amount),
              PosAmount = transactions.Where(t => t.Method == PaymentMethod.POS).Sum(t => t.Amount),
              CashAmount = transactions.Where(t => t.Method == PaymentMethod.Cash).Sum(t => t.Amount),
              // ... more calculations
          };
          
          return ServiceResult<CashierDailyReport>.Successful(report);
      }
      catch (Exception ex)
      {
          _logger.Error(ex, "Error getting daily report");
          return ServiceResult<CashierDailyReport>.Failed("خطا در دریافت گزارش روزانه");
      }
  }
  ```

- [ ] **2.1.5** پیاده‌سازی `GetMonthlyReportAsync`

- [ ] **2.1.6** پیاده‌سازی `GetAllCashiersSummaryAsync`

- [ ] **2.1.7** پیاده‌سازی `CompareCashiersAsync`

- [ ] **2.1.8** پیاده‌سازی `ExportToExcelAsync` (با EPPlus)

- [ ] **2.1.9** پیاده‌سازی `ExportToPdfAsync` (با iTextSharp)

- [ ] **2.1.10** Unit Tests

- [ ] **2.1.11** Integration Tests

- [ ] **2.1.12** ثبت در Unity Container
  ```csharp
  container.RegisterType<ICashierReportService, CashierReportService>();
  ```

---

## 📋 Task 2.2: CashSessionAuditService

**هدف:** ایجاد Service برای لاگ تمام تغییرات جلسات صندوق

### **Checklist:**

- [ ] **2.2.1** ایجاد Interface
  ```csharp
  public interface ICashSessionAuditService
  {
      Task LogActionAsync(int cashSessionId, string action, object oldValue, object newValue, string reason);
      Task<ServiceResult<List<CashSessionAuditLog>>> GetAuditLogsAsync(int cashSessionId);
      Task<ServiceResult<List<CashSessionAuditLog>>> GetUserAuditLogsAsync(string userId, DateTime fromDate, DateTime toDate);
      Task<ServiceResult<AuditSummary>> GetAuditSummaryAsync(int cashSessionId);
  }
  ```

- [ ] **2.2.2** ایجاد Service Class

- [ ] **2.2.3** پیاده‌سازی `LogActionAsync`
  ```csharp
  public async Task LogActionAsync(int cashSessionId, string action, object oldValue, object newValue, string reason)
  {
      try
      {
          var log = new CashSessionAuditLog
          {
              CashSessionId = cashSessionId,
              Action = action,
              OldValue = JsonConvert.SerializeObject(oldValue),
              NewValue = JsonConvert.SerializeObject(newValue),
              Reason = reason,
              PerformedByUserId = _currentUserService.UserId,
              PerformedAt = DateTime.Now,
              IpAddress = _currentUserService.IpAddress,
              UserAgent = _currentUserService.UserAgent
          };
          
          _context.CashSessionAuditLogs.Add(log);
          await _context.SaveChangesAsync();
          
          _logger.Information("✅ Audit log created: {Action} for CashSession: {CashSessionId}", action, cashSessionId);
      }
      catch (Exception ex)
      {
          _logger.Error(ex, "Error creating audit log");
          throw;
      }
  }
  ```

- [ ] **2.2.4** پیاده‌سازی `GetAuditLogsAsync`

- [ ] **2.2.5** پیاده‌سازی `GetUserAuditLogsAsync`

- [ ] **2.2.6** پیاده‌سازی `GetAuditSummaryAsync`

- [ ] **2.2.7** Integration با `PosManagementService`
  ```csharp
  // در PosManagementService.OpenCashSessionAsync
  await _auditService.LogActionAsync(session.CashSessionId, "Open", null, session, "Session opened");
  
  // در PosManagementService.EndSessionAsync
  await _auditService.LogActionAsync(sessionId, "Close", oldSession, newSession, "Session closed");
  ```

- [ ] **2.2.8** Unit Tests

- [ ] **2.2.9** ثبت در Unity Container

---

## 📋 Task 2.3: PaymentReconciliationService

**هدف:** ایجاد Service برای تطبیق و رفع اختلاف‌های مالی

### **Checklist:**

- [ ] **2.3.1** ایجاد Interface
  ```csharp
  public interface IPaymentReconciliationService
  {
      Task<ServiceResult<ReconciliationReport>> ReconcileSessionAsync(int cashSessionId);
      Task<ServiceResult<DiscrepancyReport>> DetectDiscrepanciesAsync(int cashSessionId);
      Task<ServiceResult<bool>> ResolveDiscrepancyAsync(int discrepancyId, string resolution);
      Task<ServiceResult<List<PaymentDiscrepancy>>> GetUnresolvedDiscrepanciesAsync();
  }
  ```

- [ ] **2.3.2** ایجاد DTOs
  ```csharp
  public class ReconciliationReport
  {
      public int CashSessionId { get; set; }
      public decimal ExpectedCashBalance { get; set; }
      public decimal ActualCashBalance { get; set; }
      public decimal CashDifference { get; set; }
      public decimal ExpectedPosBalance { get; set; }
      public decimal ActualPosBalance { get; set; }
      public decimal PosDifference { get; set; }
      public bool IsReconciled { get; set; }
      public List<DiscrepancyDetail> Discrepancies { get; set; }
  }
  ```

- [ ] **2.3.3** ایجاد Service Class

- [ ] **2.3.4** پیاده‌سازی `ReconcileSessionAsync`

- [ ] **2.3.5** پیاده‌سازی `DetectDiscrepanciesAsync`

- [ ] **2.3.6** پیاده‌سازی `ResolveDiscrepancyAsync`

- [ ] **2.3.7** پیاده‌سازی `GetUnresolvedDiscrepanciesAsync`

- [ ] **2.3.8** Unit Tests

- [ ] **2.3.9** Integration Tests

- [ ] **2.3.10** ثبت در Unity Container

---

## 📋 Task 2.4: CashierPerformanceService

**هدف:** ایجاد Service برای محاسبه و ذخیره متریک‌های عملکرد

### **Checklist:**

- [ ] **2.4.1** ایجاد Interface

- [ ] **2.4.2** ایجاد Service Class

- [ ] **2.4.3** پیاده‌سازی `CalculateDailyMetricsAsync`

- [ ] **2.4.4** پیاده‌سازی `GetMetricsAsync`

- [ ] **2.4.5** پیاده‌سازی `GetTopPerformersAsync`

- [ ] **2.4.6** ایجاد Scheduled Job (با Hangfire)
  ```csharp
  RecurringJob.AddOrUpdate(
      "calculate-daily-cashier-metrics",
      () => _performanceService.CalculateAllCashiersDailyMetricsAsync(DateTime.Today),
      Cron.Daily(1) // هر روز ساعت 1 صبح
  );
  ```

- [ ] **2.4.7** Unit Tests

- [ ] **2.4.8** ثبت در Unity Container

---

## 📋 Task 2.5: Repository Layer (Optional)

**هدف:** ایجاد Repository برای دسترسی به داده‌ها

### **Checklist:**

- [ ] **2.5.1** ایجاد `ICashSessionAuditLogRepository`

- [ ] **2.5.2** ایجاد `CashSessionAuditLogRepository`

- [ ] **2.5.3** ایجاد `IPaymentDiscrepancyRepository`

- [ ] **2.5.4** ایجاد `PaymentDiscrepancyRepository`

- [ ] **2.5.5** ایجاد `ICashierPerformanceMetricsRepository`

- [ ] **2.5.6** ایجاد `CashierPerformanceMetricsRepository`

- [ ] **2.5.7** Unit Tests

- [ ] **2.5.8** ثبت در Unity Container

---

## 🎯 Definition of Done

```
✅ تمام Interfaces ایجاد شده‌اند
✅ تمام Services پیاده‌سازی شده‌اند
✅ تمام DTOs ایجاد شده‌اند
✅ Integration با سرویس‌های موجود انجام شده است
✅ Unit Tests نوشته شده‌اند
✅ Integration Tests نوشته شده‌اند
✅ Scheduled Jobs پیکربندی شده‌اند
✅ تمام Services در Unity Container ثبت شده‌اند
✅ Build موفق است (0 Error, 0 Warning)
✅ مستندات به‌روز شده است
```

---

## 📊 Progress Tracking

**شروع:** [تاریخ]  
**پایان:** [تاریخ]  
**مدت زمان واقعی:** [X روز]  
**وضعیت:** ⏳ در حال انجام

---

**تهیه‌کننده:** AI Assistant  
**تاریخ:** 1404/10/05  
**فاز:** 2 - Services & Reports  
**طبق:** CRITICAL-FINANCIAL-MODULE-CONTRACT.md

