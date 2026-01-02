# ✅ TODO List - Phase 1: Database & Entities

**پروژه:** بهینه‌سازی صندوق و ردیابی منشی‌ها  
**فاز:** 1 - Database & Entities  
**مدت زمان:** 2-3 روز  
**اولویت:** 🔴 CRITICAL  
**طبق:** CRITICAL-FINANCIAL-MODULE-CONTRACT.md

---

## 📋 Task 1.1: CashSessionAuditLog Entity

**هدف:** ایجاد Entity برای لاگ تمام تغییرات جلسات صندوق

### **Checklist:**

- [ ] **1.1.1** ایجاد `Models/Entities/Payment/CashSessionAuditLog.cs`
  ```csharp
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

- [ ] **1.1.2** ایجاد Configuration Class
  ```csharp
  public class CashSessionAuditLogConfig : EntityTypeConfiguration<CashSessionAuditLog>
  {
      public CashSessionAuditLogConfig()
      {
          ToTable("CashSessionAuditLogs");
          HasKey(x => x.Id);
          
          Property(x => x.Action).IsRequired().HasMaxLength(50);
          Property(x => x.Reason).IsOptional().HasMaxLength(500);
          Property(x => x.IpAddress).IsOptional().HasMaxLength(50);
          Property(x => x.UserAgent).IsOptional().HasMaxLength(500);
          
          // Indexes
          HasIndex(x => x.CashSessionId);
          HasIndex(x => x.PerformedByUserId);
          HasIndex(x => x.PerformedAt);
          HasIndex(x => new { x.CashSessionId, x.PerformedAt });
      }
  }
  ```

- [ ] **1.1.3** اضافه کردن به `ApplicationDbContext`
  ```csharp
  public DbSet<CashSessionAuditLog> CashSessionAuditLogs { get; set; }
  ```

- [ ] **1.1.4** ایجاد Migration
  ```bash
  Add-Migration AddCashSessionAuditLog
  ```

- [ ] **1.1.5** بررسی Migration Code

- [ ] **1.1.6** تست Migration (Up)
  ```bash
  Update-Database
  ```

- [ ] **1.1.7** تست Migration (Down)
  ```bash
  Update-Database -TargetMigration: [PreviousMigration]
  Update-Database
  ```

- [ ] **1.1.8** بررسی Indexes در SQL Server Management Studio

- [ ] **1.1.9** تست Insert/Select در Database

---

## 📋 Task 1.2: CashierPerformanceMetrics Entity

**هدف:** ایجاد Entity برای ذخیره متریک‌های عملکرد روزانه منشی‌ها

### **Checklist:**

- [ ] **1.2.1** ایجاد `Models/Entities/Payment/CashierPerformanceMetrics.cs`
  ```csharp
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
      
      public DateTime CreatedAt { get; set; }
      public DateTime? UpdatedAt { get; set; }
      
      public virtual ApplicationUser Cashier { get; set; }
  }
  ```

- [ ] **1.2.2** ایجاد Configuration Class

- [ ] **1.2.3** اضافه کردن به `ApplicationDbContext`

- [ ] **1.2.4** ایجاد Migration

- [ ] **1.2.5** بررسی Migration Code

- [ ] **1.2.6** تست Migration

- [ ] **1.2.7** بررسی Indexes

- [ ] **1.2.8** تست Insert/Select

---

## 📋 Task 1.3: PaymentDiscrepancy Entity

**هدف:** ایجاد Entity برای ثبت اختلاف‌های مالی

### **Checklist:**

- [ ] **1.3.1** ایجاد Enums
  ```csharp
  // Models/Enums/DiscrepancyType.cs
  public enum DiscrepancyType
  {
      [Display(Name = "کسری")]
      Shortage = 1,
      
      [Display(Name = "مازاد")]
      Overage = 2,
      
      [Display(Name = "عدم تطابق")]
      Mismatch = 3
  }
  
  // Models/Enums/DiscrepancyStatus.cs
  public enum DiscrepancyStatus
  {
      [Display(Name = "در انتظار")]
      Pending = 1,
      
      [Display(Name = "حل شده")]
      Resolved = 2,
      
      [Display(Name = "ارجاع شده")]
      Escalated = 3
  }
  ```

- [ ] **1.3.2** ایجاد `Models/Entities/Payment/PaymentDiscrepancy.cs`
  ```csharp
  public class PaymentDiscrepancy
  {
      public int Id { get; set; }
      public int CashSessionId { get; set; }
      public int? PaymentTransactionId { get; set; }
      
      public DiscrepancyType Type { get; set; }
      public decimal ExpectedAmount { get; set; }
      public decimal ActualAmount { get; set; }
      public decimal Difference { get; set; }
      
      public string Reason { get; set; }
      public string Resolution { get; set; }
      public DiscrepancyStatus Status { get; set; }
      
      public string ReportedByUserId { get; set; }
      public DateTime ReportedAt { get; set; }
      public string ResolvedByUserId { get; set; }
      public DateTime? ResolvedAt { get; set; }
      
      public virtual CashSession CashSession { get; set; }
      public virtual PaymentTransaction PaymentTransaction { get; set; }
      public virtual ApplicationUser ReportedByUser { get; set; }
      public virtual ApplicationUser ResolvedByUser { get; set; }
  }
  ```

- [ ] **1.3.3** ایجاد Configuration Class

- [ ] **1.3.4** اضافه کردن به `ApplicationDbContext`

- [ ] **1.3.5** ایجاد Migration

- [ ] **1.3.6** بررسی Migration Code

- [ ] **1.3.7** تست Migration

- [ ] **1.3.8** بررسی Indexes

- [ ] **1.3.9** تست Insert/Select

---

## 📋 Task 1.4: Database Optimization

**هدف:** بهینه‌سازی Indexes و Performance

### **Checklist:**

- [ ] **1.4.1** بررسی Execution Plans برای Queries رایج

- [ ] **1.4.2** اضافه کردن Composite Indexes
  ```sql
  -- PaymentTransactions
  CREATE INDEX IX_PaymentTransaction_CashSessionId_CreatedByUserId_CreatedAt
  ON PaymentTransactions (CashSessionId, CreatedByUserId, CreatedAt)
  INCLUDE (Amount, Method, Status);
  
  -- CashSessions
  CREATE INDEX IX_CashSession_UserId_OpenedAt_Status
  ON CashSessions (UserId, OpenedAt, Status)
  INCLUDE (CashBalance, PosBalance);
  ```

- [ ] **1.4.3** تست Performance با Sample Data

- [ ] **1.4.4** بررسی Index Fragmentation

- [ ] **1.4.5** Rebuild Indexes

---

## 📋 Task 1.5: Data Seeding (Optional)

**هدف:** ایجاد Sample Data برای تست

### **Checklist:**

- [ ] **1.5.1** ایجاد Sample CashSessions

- [ ] **1.5.2** ایجاد Sample PaymentTransactions

- [ ] **1.5.3** ایجاد Sample AuditLogs

- [ ] **1.5.4** ایجاد Sample Discrepancies

- [ ] **1.5.5** تست با Sample Data

---

## 🎯 Definition of Done

```
✅ تمام Entities ایجاد شده‌اند
✅ تمام Configurations ایجاد شده‌اند
✅ تمام Migrations اجرا شده‌اند
✅ تمام Indexes ایجاد شده‌اند
✅ Database Schema بررسی شده است
✅ Sample Data ایجاد شده است (Optional)
✅ Performance تست شده است
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
**فاز:** 1 - Database & Entities  
**طبق:** CRITICAL-FINANCIAL-MODULE-CONTRACT.md

