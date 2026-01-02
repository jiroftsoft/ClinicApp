# 🏥 تحلیل عمیق: Audit Trail صندوق و ردیابی منشی‌ها

**تاریخ:** 1404/10/05  
**اولویت:** ⚠️ **CRITICAL**  
**طبق:** CRITICAL-FINANCIAL-MODULE-CONTRACT.md  
**نوع:** Deep Analysis & Systematic Review

---

## 📋 درخواست کاربر

```
"بریم برای بهینه سازی صندوق و مدیریت ان اینکه کارت توسط کدام منشی 
 در صندوق کشیده شده تمامی اطلاعات پرداخت ماژول قدرتمند"
```

---

## 🔍 Phase 1: تحلیل ساختار فعلی

### **1.1 Entity Analysis**

#### **PaymentTransaction** ✅

```csharp
public class PaymentTransaction : ISoftDelete, ITrackable
{
    // ✅ Audit Trail موجود است
    public string CreatedByUserId { get; set; }          // منشی که پرداخت را ثبت کرد
    public virtual ApplicationUser CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // ✅ CashSession موجود است
    public int CashSessionId { get; set; }               // جلسه صندوق
    public virtual CashSession CashSession { get; set; }
    
    // ✅ POS Terminal موجود است
    public int? PosTerminalId { get; set; }              // دستگاه POS
    public virtual PosTerminal PosTerminal { get; set; }
    
    // ✅ Transaction Details موجود است
    public string TransactionId { get; set; }            // شماره تراکنش
    public string ReferenceCode { get; set; }            // RRN
    public string TerminalId { get; set; }               // شناسه ترمینال
    public string CardLast4 { get; set; }                // 4 رقم آخر کارت
    
    // ✅ Soft Delete موجود است
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string DeletedByUserId { get; set; }
}
```

**نتیجه:** ✅ **ساختار کامل است!**

---

#### **CashSession** ✅

```csharp
public class CashSession : ISoftDelete, ITrackable
{
    // ✅ منشی صندوق‌دار مشخص است
    public string UserId { get; set; }                    // منشی صندوق‌دار
    public virtual ApplicationUser User { get; set; }
    
    // ✅ Audit Trail موجود است
    public string CreatedByUserId { get; set; }           // کسی که جلسه را باز کرد
    public DateTime CreatedAt { get; set; }
    public string UpdatedByUserId { get; set; }           // کسی که جلسه را بست
    public DateTime? UpdatedAt { get; set; }
    
    // ✅ Session Details موجود است
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CashBalance { get; set; }
    public decimal PosBalance { get; set; }
    
    // ✅ Transactions موجود است
    public virtual ICollection<PaymentTransaction> Transactions { get; set; }
}
```

**نتیجه:** ✅ **ساختار کامل است!**

---

### **1.2 Database Analysis**

**نتیجه Query:**
```sql
SELECT COUNT(*) as TotalPayments, 
       COUNT(DISTINCT CreatedByUserId) as UniqueUsers, 
       COUNT(CASE WHEN CreatedByUserId IS NULL THEN 1 END) as NullUsers 
FROM PaymentTransactions 
WHERE IsDeleted = 0
```

**نتیجه:** (0 rows) - **هیچ پرداختی در دیتابیس وجود ندارد!**

---

### **1.3 Code Analysis**

#### **✅ CreatedByUserId پر می‌شود:**

```csharp
// Services/Reception/ReceptionFacade.cs - خط 3218
var payment = new PaymentTransaction
{
    ReceptionId = request.ReceptionId,
    Amount = request.AmountIRR,
    Status = PaymentStatus.Success,
    Method = PaymentMethod.POS,
    CashSessionId = sessionResult.Data.CashSessionId,
    CreatedByUserId = _currentUserService?.UserId, // ✅ منشی ثبت‌کننده
    CreatedAt = DateTime.Now
};
```

**نتیجه:** ✅ **کد صحیح است!**

---

## 📊 Phase 2: Gap Analysis (شکاف‌ها)

### **2.1 موارد موجود ✅**

```
✅ PaymentTransaction.CreatedByUserId
✅ PaymentTransaction.CreatedByUser (Navigation)
✅ CashSession.UserId (منشی صندوق‌دار)
✅ CashSession.User (Navigation)
✅ PosTerminal.TerminalId
✅ TransactionId, ReferenceCode, CardLast4
✅ Soft Delete
✅ ITrackable (CreatedAt, UpdatedAt, DeletedAt)
✅ Indexes برای Performance
```

---

### **2.2 موارد ناقص ⚠️**

```
⚠️ گزارش جامع منشی‌ها (Cashier Report)
⚠️ Dashboard برای مدیریت صندوق
⚠️ گزارش تراکنش‌های هر منشی
⚠️ مقایسه عملکرد منشی‌ها
⚠️ Audit Log برای تغییرات CashSession
⚠️ Alert برای تفاوت موجودی
⚠️ Export به Excel/PDF
⚠️ Real-time Monitoring
```

---

### **2.3 موارد پیشنهادی 💡**

```
💡 CashierPerformanceReport
💡 CashSessionAuditLog
💡 PaymentTransactionAuditLog
💡 CashierDashboard
💡 Real-time Alerts
💡 Reconciliation Report
💡 Discrepancy Tracking
💡 Shift Handover Report
```

---

## 🎯 Phase 3: نقشه راه بهینه‌سازی

### **Roadmap: 3 فاز اصلی**

```
Phase 1: Database & Entities (2-3 روز)
├── 1.1 CashSessionAuditLog Entity
├── 1.2 CashierPerformanceMetrics Entity
├── 1.3 PaymentDiscrepancy Entity
└── 1.4 Migration & Indexes

Phase 2: Services & Reports (3-4 روز)
├── 2.1 CashierReportService
├── 2.2 CashSessionAuditService
├── 2.3 PaymentReconciliationService
└── 2.4 CashierPerformanceService

Phase 3: UI & Dashboard (2-3 روز)
├── 3.1 Cashier Dashboard
├── 3.2 Transaction Reports
├── 3.3 Performance Charts
└── 3.4 Export Functionality
```

**مدت زمان کل:** 7-10 روز

---

## 📝 Phase 4: TODO Lists

### **TODO List 1: Database & Entities (فاز 1)**

#### **Task 1.1: CashSessionAuditLog Entity**

```csharp
/// <summary>
/// لاگ تغییرات جلسات صندوق
/// </summary>
public class CashSessionAuditLog
{
    public int Id { get; set; }
    public int CashSessionId { get; set; }
    public string Action { get; set; } // Open, Close, Adjust, Cancel
    public string OldValue { get; set; } // JSON
    public string NewValue { get; set; } // JSON
    public string Reason { get; set; }
    public string PerformedByUserId { get; set; }
    public DateTime PerformedAt { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    
    public virtual CashSession CashSession { get; set; }
    public virtual ApplicationUser PerformedByUser { get; set; }
}
```

**Checklist:**
- [ ] ایجاد Entity
- [ ] ایجاد Configuration
- [ ] اضافه کردن به DbContext
- [ ] ایجاد Migration
- [ ] اضافه کردن Indexes
- [ ] تست در Database

---

#### **Task 1.2: CashierPerformanceMetrics Entity**

```csharp
/// <summary>
/// متریک‌های عملکرد منشی‌ها
/// </summary>
public class CashierPerformanceMetrics
{
    public int Id { get; set; }
    public string CashierId { get; set; }
    public DateTime Date { get; set; }
    
    // Transaction Metrics
    public int TotalTransactions { get; set; }
    public int PosTransactions { get; set; }
    public int CashTransactions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PosAmount { get; set; }
    public decimal CashAmount { get; set; }
    
    // Performance Metrics
    public decimal AverageTransactionTime { get; set; } // seconds
    public int SuccessfulTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public decimal SuccessRate { get; set; } // percentage
    
    // Discrepancy Metrics
    public int DiscrepancyCount { get; set; }
    public decimal TotalDiscrepancy { get; set; }
    
    // Session Metrics
    public int SessionsOpened { get; set; }
    public int SessionsClosed { get; set; }
    public TimeSpan AverageSessionDuration { get; set; }
    
    public virtual ApplicationUser Cashier { get; set; }
}
```

**Checklist:**
- [ ] ایجاد Entity
- [ ] ایجاد Configuration
- [ ] اضافه کردن به DbContext
- [ ] ایجاد Migration
- [ ] اضافه کردن Indexes
- [ ] تست در Database

---

#### **Task 1.3: PaymentDiscrepancy Entity**

```csharp
/// <summary>
/// اختلاف‌های مالی
/// </summary>
public class PaymentDiscrepancy
{
    public int Id { get; set; }
    public int CashSessionId { get; set; }
    public int? PaymentTransactionId { get; set; }
    
    public DiscrepancyType Type { get; set; } // Shortage, Overage, Mismatch
    public decimal ExpectedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Difference { get; set; }
    
    public string Reason { get; set; }
    public string Resolution { get; set; }
    public DiscrepancyStatus Status { get; set; } // Pending, Resolved, Escalated
    
    public string ReportedByUserId { get; set; }
    public DateTime ReportedAt { get; set; }
    public string ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    
    public virtual CashSession CashSession { get; set; }
    public virtual PaymentTransaction PaymentTransaction { get; set; }
    public virtual ApplicationUser ReportedByUser { get; set; }
    public virtual ApplicationUser ResolvedByUser { get; set; }
}

public enum DiscrepancyType
{
    Shortage = 1,   // کسری
    Overage = 2,    // مازاد
    Mismatch = 3    // عدم تطابق
}

public enum DiscrepancyStatus
{
    Pending = 1,
    Resolved = 2,
    Escalated = 3
}
```

**Checklist:**
- [ ] ایجاد Entity
- [ ] ایجاد Enums
- [ ] ایجاد Configuration
- [ ] اضافه کردن به DbContext
- [ ] ایجاد Migration
- [ ] اضافه کردن Indexes
- [ ] تست در Database

---

#### **Task 1.4: Database Migration**

**Checklist:**
- [ ] Generate Migration
- [ ] Review Migration Code
- [ ] Test Migration (Up)
- [ ] Test Migration (Down)
- [ ] Backup Database
- [ ] Apply Migration to Production

---

### **TODO List 2: Services & Reports (فاز 2)**

#### **Task 2.1: CashierReportService**

```csharp
public interface ICashierReportService
{
    Task<ServiceResult<CashierDailyReport>> GetDailyReportAsync(string cashierId, DateTime date);
    Task<ServiceResult<CashierMonthlyReport>> GetMonthlyReportAsync(string cashierId, int year, int month);
    Task<ServiceResult<List<CashierSummary>>> GetAllCashiersSummaryAsync(DateTime fromDate, DateTime toDate);
    Task<ServiceResult<CashierPerformanceComparison>> CompareCashiersAsync(List<string> cashierIds, DateTime fromDate, DateTime toDate);
}
```

**Checklist:**
- [ ] ایجاد Interface
- [ ] ایجاد Service Class
- [ ] پیاده‌سازی GetDailyReportAsync
- [ ] پیاده‌سازی GetMonthlyReportAsync
- [ ] پیاده‌سازی GetAllCashiersSummaryAsync
- [ ] پیاده‌سازی CompareCashiersAsync
- [ ] Unit Tests
- [ ] Integration Tests
- [ ] ثبت در Unity Container

---

#### **Task 2.2: CashSessionAuditService**

```csharp
public interface ICashSessionAuditService
{
    Task LogActionAsync(int cashSessionId, string action, object oldValue, object newValue, string reason);
    Task<ServiceResult<List<CashSessionAuditLog>>> GetAuditLogsAsync(int cashSessionId);
    Task<ServiceResult<List<CashSessionAuditLog>>> GetUserAuditLogsAsync(string userId, DateTime fromDate, DateTime toDate);
}
```

**Checklist:**
- [ ] ایجاد Interface
- [ ] ایجاد Service Class
- [ ] پیاده‌سازی LogActionAsync
- [ ] پیاده‌سازی GetAuditLogsAsync
- [ ] پیاده‌سازی GetUserAuditLogsAsync
- [ ] Integration با CashSession Actions
- [ ] Unit Tests
- [ ] ثبت در Unity Container

---

#### **Task 2.3: PaymentReconciliationService**

```csharp
public interface IPaymentReconciliationService
{
    Task<ServiceResult<ReconciliationReport>> ReconcileSessionAsync(int cashSessionId);
    Task<ServiceResult<DiscrepancyReport>> DetectDiscrepanciesAsync(int cashSessionId);
    Task<ServiceResult<bool>> ResolveDiscrepancyAsync(int discrepancyId, string resolution);
}
```

**Checklist:**
- [ ] ایجاد Interface
- [ ] ایجاد Service Class
- [ ] پیاده‌سازی ReconcileSessionAsync
- [ ] پیاده‌سازی DetectDiscrepanciesAsync
- [ ] پیاده‌سازی ResolveDiscrepancyAsync
- [ ] Unit Tests
- [ ] Integration Tests
- [ ] ثبت در Unity Container

---

#### **Task 2.4: CashierPerformanceService**

```csharp
public interface ICashierPerformanceService
{
    Task CalculateDailyMetricsAsync(string cashierId, DateTime date);
    Task<ServiceResult<CashierPerformanceMetrics>> GetMetricsAsync(string cashierId, DateTime date);
    Task<ServiceResult<List<CashierRanking>>> GetTopPerformersAsync(DateTime fromDate, DateTime toDate, int topN);
}
```

**Checklist:**
- [ ] ایجاد Interface
- [ ] ایجاد Service Class
- [ ] پیاده‌سازی CalculateDailyMetricsAsync
- [ ] پیاده‌سازی GetMetricsAsync
- [ ] پیاده‌سازی GetTopPerformersAsync
- [ ] Scheduled Job برای محاسبه روزانه
- [ ] Unit Tests
- [ ] ثبت در Unity Container

---

## 🎯 اولویت‌بندی

### **Priority 1: CRITICAL (فوری)** 🔴

```
1. CashSessionAuditLog Entity
2. CashierReportService (Daily Report)
3. Transaction Report by Cashier
```

**دلیل:** ردیابی منشی‌ها و Audit Trail مالی

---

### **Priority 2: HIGH (مهم)** 🟡

```
4. PaymentDiscrepancy Entity
5. PaymentReconciliationService
6. Cashier Dashboard
```

**دلیل:** تطبیق و رفع اختلاف‌های مالی

---

### **Priority 3: MEDIUM (متوسط)** 🟢

```
7. CashierPerformanceMetrics Entity
8. CashierPerformanceService
9. Performance Charts
10. Export Functionality
```

**دلیل:** بهبود عملکرد و گزارش‌گیری

---

## 📚 مراجع

- `Models/Entities/Payment/PaymentTransaction.cs`
- `Models/Entities/Payment/CashSession.cs`
- `Services/Reception/ReceptionFacade.cs`
- `Docs/CRITICAL-FINANCIAL-MODULE-CONTRACT.md`
- `Docs/Knowledge-Base/`

---

**تهیه‌کننده:** AI Assistant  
**تاریخ:** 1404/10/05  
**نوع:** Deep Analysis & Roadmap  
**طبق:** CRITICAL-FINANCIAL-MODULE-CONTRACT.md

