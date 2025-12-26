# 🎯 نقشه راه بازطراحی شده - Phase 3: UI & Dashboard

**تاریخ:** 1404/10/05  
**وضعیت:** 🔄 **بازطراحی شده**  
**اولویت:** 🔴 **CRITICAL**  
**طبق:** CRITICAL-FINANCIAL-MODULE-CONTRACT.md

---

## 📋 **خلاصه اجرایی**

Phase 3 شامل **4 ماژول اصلی** است که باید به صورت **سیستماتیک و دقیق** پیاده‌سازی شوند:

1. **Cashier Dashboard** - داشبورد اصلی منشی‌ها
2. **Transaction Reports UI** - رابط کاربری گزارش‌های تراکنش
3. **Performance Charts** - نمودارهای عملکرد
4. **Export Functionality** - قابلیت Export (Excel/PDF)

---

## 🎯 **اهداف Phase 3**

### **1. Cashier Dashboard**
- نمایش Real-time Metrics
- نمایش Transaction Summary
- نمایش Performance Charts
- نمایش Top Performers
- نمایش Discrepancies

### **2. Transaction Reports UI**
- گزارش روزانه
- گزارش ماهانه
- گزارش بازه زمانی
- فیلتر بر اساس منشی
- فیلتر بر اساس تاریخ

### **3. Performance Charts**
- نمودار روند تراکنش‌ها
- نمودار مقایسه منشی‌ها
- نمودار نرخ موفقیت
- نمودار اختلاف‌ها

### **4. Export Functionality**
- Export به Excel
- Export به PDF
- Export با فیلترها

---

## 📊 **ساختار Phase 3**

### **Module 3.1: Cashier Dashboard** 🟢
```
├── Controller: CashierDashboardController
│   ├── Index (Dashboard اصلی)
│   ├── GetDailyStats (AJAX)
│   ├── GetTopPerformers (AJAX)
│   └── GetCashierRanking (AJAX)
│
├── ViewModels
│   ├── CashierDashboardViewModel
│   └── CashierStatsViewModel
│
├── Views
│   ├── CashierDashboard/Index.cshtml
│   └── CashierDashboard/_StatsPartial.cshtml
│
└── JavaScript
    └── cashier-dashboard.js
```

### **Module 3.2: Transaction Reports UI** 🟡
```
├── Controller: CashierReportController
│   ├── DailyReport
│   ├── MonthlyReport
│   ├── RangeReport
│   ├── AllCashiersSummary
│   └── CompareCashiers
│
├── ViewModels
│   ├── CashierReportIndexViewModel
│   ├── CashierDailyReportViewModel
│   ├── CashierMonthlyReportViewModel
│   └── CashierComparisonViewModel
│
├── Views
│   ├── CashierReport/Index.cshtml
│   ├── CashierReport/DailyReport.cshtml
│   ├── CashierReport/MonthlyReport.cshtml
│   ├── CashierReport/RangeReport.cshtml
│   ├── CashierReport/AllCashiers.cshtml
│   └── CashierReport/Compare.cshtml
│
└── JavaScript
    └── cashier-reports.js
```

### **Module 3.3: Performance Charts** 🔵
```
├── JavaScript Libraries
│   ├── Chart.js (موجود در پروژه)
│   └── Custom Chart Helpers
│
├── Chart Types
│   ├── Line Chart (روند تراکنش‌ها)
│   ├── Bar Chart (مقایسه منشی‌ها)
│   ├── Doughnut Chart (نرخ موفقیت)
│   └── Area Chart (اختلاف‌ها)
│
└── JavaScript
    └── cashier-charts.js
```

### **Module 3.4: Export Functionality** 🟠
```
├── Controller Actions
│   ├── ExportToExcel
│   └── ExportToPdf
│
├── Libraries
│   ├── EPPlus یا ClosedXML (Excel)
│   └── iTextSharp یا QuestPDF (PDF)
│
└── Export Templates
    ├── Excel Template
    └── PDF Template
```

---

## 🗓️ **زمان‌بندی**

### **Week 1: Controller & Views (3-4 روز)**
- Day 1-2: CashierDashboardController + Views
- Day 3-4: CashierReportController + Views

### **Week 2: JavaScript & Charts (2-3 روز)**
- Day 1-2: JavaScript برای Dashboard و Reports
- Day 2-3: Charts و Visualization

### **Week 3: Export & Polish (1-2 روز)**
- Day 1: Export به Excel
- Day 2: Export به PDF + Testing

**کل زمان:** 6-9 روز

---

## 🎯 **اولویت‌بندی**

### **Priority 1: CRITICAL** 🔴
```
1. CashierDashboardController (Index)
2. CashierReportController (DailyReport, MonthlyReport)
3. Basic Charts (Line, Bar)
```

### **Priority 2: HIGH** 🟡
```
4. Transaction Reports UI (RangeReport, AllCashiers)
5. Advanced Charts (Doughnut, Area)
6. Export to Excel
```

### **Priority 3: MEDIUM** 🟢
```
7. Compare Cashiers UI
8. Export to PDF
9. Real-time Updates (SignalR - اختیاری)
```

---

## 📝 **الگوهای موجود در پروژه**

### **1. TriageDashboardController** ✅
- الگوی خوب برای Dashboard
- استفاده از ViewModels
- AJAX Actions
- Real-time Stats

### **2. Chart.js** ✅
- موجود در پروژه
- استفاده در DoctorReporting
- استفاده در TriageReports

### **3. Persian DatePicker** ✅
- Helper موجود
- استفاده در تمام Forms

---

## 🔧 **تکنولوژی‌ها**

### **Backend:**
- ASP.NET MVC 5
- Entity Framework 6
- Unity Container (DI)
- Serilog (Logging)

### **Frontend:**
- jQuery
- Chart.js
- Bootstrap 4
- Persian DatePicker

### **Export:**
- EPPlus یا ClosedXML (Excel)
- iTextSharp یا QuestPDF (PDF)

---

## ✅ **Definition of Done**

### **Module 3.1: Cashier Dashboard**
- [ ] Controller ایجاد شده
- [ ] ViewModels ایجاد شده
- [ ] Views ایجاد شده
- [ ] JavaScript ایجاد شده
- [ ] Real-time Stats کار می‌کند
- [ ] Charts نمایش داده می‌شوند
- [ ] Responsive است
- [ ] تست شده

### **Module 3.2: Transaction Reports UI**
- [ ] Controller ایجاد شده
- [ ] ViewModels ایجاد شده
- [ ] Views ایجاد شده
- [ ] JavaScript ایجاد شده
- [ ] فیلترها کار می‌کنند
- [ ] Persian DatePicker کار می‌کند
- [ ] Responsive است
- [ ] تست شده

### **Module 3.3: Performance Charts**
- [ ] Chart.js تنظیم شده
- [ ] Charts ایجاد شده
- [ ] Data Binding کار می‌کند
- [ ] Responsive است
- [ ] تست شده

### **Module 3.4: Export Functionality**
- [ ] Export به Excel کار می‌کند
- [ ] Export به PDF کار می‌کند
- [ ] فیلترها اعمال می‌شوند
- [ ] فرمت صحیح است
- [ ] تست شده

---

## 🎓 **یادگیری‌ها از Phase 1 & 2**

### **1. ServiceResult Pattern** ✅
- تمام Services از ServiceResult استفاده می‌کنند
- Error Handling یکپارچه
- Logging کامل

### **2. ViewModels** ✅
- Strongly-Typed
- Data Annotations
- Validation

### **3. Persian DatePicker** ✅
- الزامی برای تمام Forms
- ParseDateFromHiddenInput در Controller

---

## 📚 **مراجع**

- `Controllers/Triage/TriageDashboardController.cs` - الگوی Dashboard
- `Services/Payment/CashierReportService.cs` - Service موجود
- `Services/Payment/CashierPerformanceService.cs` - Service موجود
- `Docs/TODO_TEMPLATE.md` - Template TODO
- `Docs/CRITICAL-FINANCIAL-MODULE-CONTRACT.md` - قرارداد Critical

---

**تهیه‌کننده:** AI Assistant  
**تاریخ:** 1404/10/05  
**وضعیت:** ✅ **بازطراحی شده و آماده**

