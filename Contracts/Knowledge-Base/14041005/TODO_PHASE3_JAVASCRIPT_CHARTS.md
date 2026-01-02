# ✅ TODO List - Phase 3: JavaScript & Charts

**پروژه:** بهینه‌سازی صندوق و ردیابی منشی‌ها  
**فاز:** 3.2 - JavaScript & Charts  
**مدت زمان:** 2-3 روز  
**اولویت:** 🔴 CRITICAL  
**طبق:** CRITICAL-FINANCIAL-MODULE-CONTRACT.md, TODO_TEMPLATE.md

---

## 📋 Task 3.2.1: Cashier Dashboard JavaScript

**هدف:** ایجاد JavaScript برای Dashboard اصلی

### **Checklist:**

- [ ] **3.2.1.1** ایجاد Scripts/cashier/cashier-dashboard.js
  ```javascript
  /**
   * Cashier Dashboard JavaScript
   * مدیریت Dashboard منشی‌ها
   */
  var CashierDashboard = {
      // تنظیمات
      config: {
          apiBaseUrl: '/Payment/CashierDashboard',
          refreshInterval: 30000, // 30 ثانیه
          chartColors: {
              primary: '#667eea',
              success: '#48bb78',
              warning: '#ed8936',
              danger: '#f56565',
              info: '#4299e1'
          }
      },
      
      // Charts
      charts: {
          transactionsChart: null,
          performanceChart: null,
          discrepancyChart: null
      },
      
      // Initialize
      init: function() {
          this.loadDailyStats();
          this.loadTopPerformers();
          this.setupAutoRefresh();
          this.setupEventListeners();
      },
      
      // Load Daily Stats
      loadDailyStats: function() {
          var self = this;
          var date = $('#selectedDate').val() || new Date().toISOString().split('T')[0];
          
          $.ajax({
              url: self.config.apiBaseUrl + '/GetDailyStats',
              type: 'POST',
              data: {
                  date: date,
                  __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
              },
              success: function(response) {
                  if (response.success) {
                      self.updateStatsCards(response.data);
                      self.updateCharts(response.data);
                  } else {
                      console.error('خطا در دریافت آمار:', response.message);
                  }
              },
              error: function(xhr, status, error) {
                  console.error('خطا در AJAX:', error);
              }
          });
      },
      
      // Update Stats Cards
      updateStatsCards: function(data) {
          $('#total-transactions').text(data.TotalTransactions || 0);
          $('#total-amount').text(this.formatCurrency(data.TotalAmount || 0));
          $('#success-rate').text((data.SuccessRate || 0).toFixed(2) + '%');
          $('#avg-transaction-time').text((data.AverageTransactionTime || 0).toFixed(2) + ' ثانیه');
          $('#discrepancy-count').text(data.DiscrepancyCount || 0);
      },
      
      // Format Currency
      formatCurrency: function(amount) {
          return new Intl.NumberFormat('fa-IR', {
              style: 'currency',
              currency: 'IRR',
              minimumFractionDigits: 0
          }).format(amount);
      },
      
      // Setup Auto Refresh
      setupAutoRefresh: function() {
          var self = this;
          setInterval(function() {
              self.loadDailyStats();
          }, self.config.refreshInterval);
      },
      
      // Setup Event Listeners
      setupEventListeners: function() {
          var self = this;
          
          // Date Change
          $('#selectedDate').on('change', function() {
              self.loadDailyStats();
          });
          
          // Refresh Button
          $('#refresh-btn').on('click', function() {
              self.loadDailyStats();
          });
      }
  };
  
  // Initialize on document ready
  $(document).ready(function() {
      CashierDashboard.init();
  });
  ```

- [ ] **3.2.1.2** پیاده‌سازی updateStatsCards
- [ ] **3.2.1.3** پیاده‌سازی setupAutoRefresh
- [ ] **3.2.1.4** پیاده‌سازی setupEventListeners
- [ ] **3.2.1.5** Error Handling
- [ ] **3.2.1.6** Loading States
- [ ] **3.2.1.7** تست کامل

---

## 📋 Task 3.2.2: Cashier Charts JavaScript

**هدف:** ایجاد Charts با Chart.js

### **Checklist:**

- [ ] **3.2.2.1** ایجاد Scripts/cashier/cashier-charts.js
  ```javascript
  /**
   * Cashier Charts JavaScript
   * مدیریت Charts با Chart.js
   */
  var CashierCharts = {
      // Charts
      charts: {},
      
      // Initialize
      init: function() {
          this.createTransactionsChart();
          this.createPerformanceChart();
          this.createDiscrepancyChart();
      },
      
      // Create Transactions Chart (Line)
      createTransactionsChart: function(data) {
          var ctx = document.getElementById('transactionsChart');
          if (!ctx) return;
          
          var self = this;
          
          // Destroy existing chart
          if (self.charts.transactionsChart) {
              self.charts.transactionsChart.destroy();
          }
          
          self.charts.transactionsChart = new Chart(ctx, {
              type: 'line',
              data: {
                  labels: data.Labels || [],
                  datasets: [{
                      label: 'تعداد تراکنش‌ها',
                      data: data.Transactions || [],
                      borderColor: '#667eea',
                      backgroundColor: 'rgba(102, 126, 234, 0.1)',
                      tension: 0.4,
                      fill: true
                  }]
              },
              options: {
                  responsive: true,
                  maintainAspectRatio: false,
                  plugins: {
                      legend: {
                          display: true,
                          position: 'top'
                      },
                      tooltip: {
                          callbacks: {
                              label: function(context) {
                                  return 'تعداد: ' + context.parsed.y;
                              }
                          }
                      }
                  },
                  scales: {
                      y: {
                          beginAtZero: true,
                          ticks: {
                              precision: 0
                          }
                      }
                  }
              }
          });
      },
      
      // Create Performance Chart (Bar)
      createPerformanceChart: function(data) {
          var ctx = document.getElementById('performanceChart');
          if (!ctx) return;
          
          var self = this;
          
          // Destroy existing chart
          if (self.charts.performanceChart) {
              self.charts.performanceChart.destroy();
          }
          
          self.charts.performanceChart = new Chart(ctx, {
              type: 'bar',
              data: {
                  labels: data.Labels || [],
                  datasets: [{
                      label: 'مبلغ کل (ریال)',
                      data: data.Amounts || [],
                      backgroundColor: '#48bb78',
                      borderColor: '#38a169',
                      borderWidth: 1
                  }]
              },
              options: {
                  responsive: true,
                  maintainAspectRatio: false,
                  plugins: {
                      legend: {
                          display: true,
                          position: 'top'
                      },
                      tooltip: {
                          callbacks: {
                              label: function(context) {
                                  return 'مبلغ: ' + new Intl.NumberFormat('fa-IR').format(context.parsed.y) + ' ریال';
                              }
                          }
                      }
                  },
                  scales: {
                      y: {
                          beginAtZero: true,
                          ticks: {
                              callback: function(value) {
                                  return new Intl.NumberFormat('fa-IR').format(value);
                              }
                          }
                      }
                  }
              }
          });
      },
      
      // Create Discrepancy Chart (Doughnut)
      createDiscrepancyChart: function(data) {
          var ctx = document.getElementById('discrepancyChart');
          if (!ctx) return;
          
          var self = this;
          
          // Destroy existing chart
          if (self.charts.discrepancyChart) {
              self.charts.discrepancyChart.destroy();
          }
          
          self.charts.discrepancyChart = new Chart(ctx, {
              type: 'doughnut',
              data: {
                  labels: ['بدون اختلاف', 'با اختلاف'],
                  datasets: [{
                      data: [
                          data.TotalTransactions - data.DiscrepancyCount,
                          data.DiscrepancyCount
                      ],
                      backgroundColor: [
                          '#48bb78',
                          '#f56565'
                      ],
                      borderWidth: 1
                  }]
              },
              options: {
                  responsive: true,
                  maintainAspectRatio: false,
                  plugins: {
                      legend: {
                          display: true,
                          position: 'bottom'
                      },
                      tooltip: {
                          callbacks: {
                              label: function(context) {
                                  var label = context.label || '';
                                  var value = context.parsed || 0;
                                  var total = context.dataset.data.reduce((a, b) => a + b, 0);
                                  var percentage = ((value / total) * 100).toFixed(2);
                                  return label + ': ' + value + ' (' + percentage + '%)';
                              }
                          }
                      }
                  }
              }
          });
      },
      
      // Update Charts
      updateCharts: function(data) {
          this.createTransactionsChart(data);
          this.createPerformanceChart(data);
          this.createDiscrepancyChart(data);
      },
      
      // Destroy All Charts
      destroyAll: function() {
          var self = this;
          Object.keys(self.charts).forEach(function(key) {
              if (self.charts[key]) {
                  self.charts[key].destroy();
              }
          });
      }
  };
  
  // Initialize on document ready
  $(document).ready(function() {
      CashierCharts.init();
  });
  ```

- [ ] **3.2.2.2** پیاده‌سازی createTransactionsChart (Line Chart)
- [ ] **3.2.2.3** پیاده‌سازی createPerformanceChart (Bar Chart)
- [ ] **3.2.2.4** پیاده‌سازی createDiscrepancyChart (Doughnut Chart)
- [ ] **3.2.2.5** پیاده‌سازی createSuccessRateChart (Area Chart)
- [ ] **3.2.2.6** پیاده‌سازی updateCharts
- [ ] **3.2.2.7** پیاده‌سازی destroyAll
- [ ] **3.2.2.8** RTL Support
- [ ] **3.2.2.9** Responsive Design
- [ ] **3.2.2.10** تست کامل

---

## 📋 Task 3.2.3: Cashier Reports JavaScript

**هدف:** ایجاد JavaScript برای Reports

### **Checklist:**

- [ ] **3.2.3.1** ایجاد Scripts/cashier/cashier-reports.js
  ```javascript
  /**
   * Cashier Reports JavaScript
   * مدیریت Reports و فیلترها
   */
  var CashierReports = {
      // تنظیمات
      config: {
          apiBaseUrl: '/Payment/CashierReport'
      },
      
      // Initialize
      init: function() {
          this.setupDatePickers();
          this.setupEventListeners();
          this.loadInitialData();
      },
      
      // Setup Persian Date Pickers
      setupDatePickers: function() {
          // Start Date
          $('#startDate').persianDatepicker({
              format: 'YYYY/MM/DD',
              autoClose: true,
              initialValue: false,
              persianDigit: true
          });
          
          // End Date
          $('#endDate').persianDatepicker({
              format: 'YYYY/MM/DD',
              autoClose: true,
              initialValue: false,
              persianDigit: true
          });
      },
      
      // Setup Event Listeners
      setupEventListeners: function() {
          var self = this;
          
          // Filter Form Submit
          $('#filterForm').on('submit', function(e) {
              e.preventDefault();
              self.loadReport();
          });
          
          // Export Buttons
          $('#exportExcelBtn').on('click', function() {
              self.exportToExcel();
          });
          
          $('#exportPdfBtn').on('click', function() {
              self.exportToPdf();
          });
      },
      
      // Load Report
      loadReport: function() {
          var self = this;
          var reportType = $('#reportType').val();
          var cashierId = $('#cashierId').val();
          var startDate = $('#startDate').val();
          var endDate = $('#endDate').val();
          
          // Show Loading
          $('.loading-overlay').show();
          
          $.ajax({
              url: self.config.apiBaseUrl + '/' + reportType,
              type: 'POST',
              data: {
                  cashierId: cashierId,
                  startDate: startDate,
                  endDate: endDate,
                  __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
              },
              success: function(response) {
                  if (response.success) {
                      self.renderReport(response.data);
                  } else {
                      self.showError(response.message);
                  }
              },
              error: function(xhr, status, error) {
                  self.showError('خطا در دریافت گزارش');
              },
              complete: function() {
                  $('.loading-overlay').hide();
              }
          });
      },
      
      // Render Report
      renderReport: function(data) {
          // Render based on report type
          var reportType = $('#reportType').val();
          
          switch(reportType) {
              case 'DailyReport':
                  this.renderDailyReport(data);
                  break;
              case 'MonthlyReport':
                  this.renderMonthlyReport(data);
                  break;
              case 'RangeReport':
                  this.renderRangeReport(data);
                  break;
              default:
                  console.error('نوع گزارش نامعتبر');
          }
      },
      
      // Export to Excel
      exportToExcel: function() {
          var self = this;
          var cashierId = $('#cashierId').val();
          var startDate = $('#startDate').val();
          var endDate = $('#endDate').val();
          
          window.location.href = self.config.apiBaseUrl + '/ExportToExcel?cashierId=' + cashierId + 
              '&fromDate=' + startDate + '&toDate=' + endDate;
      },
      
      // Export to PDF
      exportToPdf: function() {
          var self = this;
          var cashierId = $('#cashierId').val();
          var startDate = $('#startDate').val();
          var endDate = $('#endDate').val();
          
          window.location.href = self.config.apiBaseUrl + '/ExportToPdf?cashierId=' + cashierId + 
              '&fromDate=' + startDate + '&toDate=' + endDate;
      }
  };
  
  // Initialize on document ready
  $(document).ready(function() {
      CashierReports.init();
  });
  ```

- [ ] **3.2.3.2** پیاده‌سازی setupDatePickers
- [ ] **3.2.3.3** پیاده‌سازی setupEventListeners
- [ ] **3.2.3.4** پیاده‌سازی loadReport
- [ ] **3.2.3.5** پیاده‌سازی renderReport
- [ ] **3.2.3.6** پیاده‌سازی renderDailyReport
- [ ] **3.2.3.7** پیاده‌سازی renderMonthlyReport
- [ ] **3.2.3.8** پیاده‌سازی renderRangeReport
- [ ] **3.2.3.9** پیاده‌سازی exportToExcel
- [ ] **3.2.3.10** پیاده‌سازی exportToPdf
- [ ] **3.2.3.11** Error Handling
- [ ] **3.2.3.12** Loading States
- [ ] **3.2.3.13** تست کامل

---

## 📋 Task 3.2.4: Top Performers JavaScript

**هدف:** ایجاد JavaScript برای نمایش Top Performers

### **Checklist:**

- [ ] **3.2.4.1** پیاده‌سازی loadTopPerformers
- [ ] **3.2.4.2** پیاده‌سازی renderTopPerformers
- [ ] **3.2.4.3** پیاده‌سازی createTopPerformersChart
- [ ] **3.2.4.4** Responsive Design
- [ ] **3.2.4.5** تست کامل

---

## 📋 Task 3.2.5: Chart.js Configuration

**هدف:** تنظیم Chart.js برای پروژه

### **Checklist:**

- [ ] **3.2.5.1** بررسی وجود Chart.js در پروژه
- [ ] **3.2.5.2** اضافه کردن Chart.js به Bundle (در صورت نیاز)
- [ ] **3.2.5.3** تنظیم RTL برای Charts
- [ ] **3.2.5.4** تنظیم Persian Number Format
- [ ] **3.2.5.5** تنظیم Default Options
- [ ] **3.2.5.6** تست کامل

---

## 📋 Task 3.2.6: CSS Styling

**هدف:** استایل‌دهی Dashboard و Reports

### **Checklist:**

- [ ] **3.2.6.1** ایجاد Content/cashier/cashier-dashboard.css
- [ ] **3.2.6.2** استایل Stats Cards
- [ ] **3.2.6.3** استایل Charts Container
- [ ] **3.2.6.4** استایل Reports Table
- [ ] **3.2.6.5** Responsive Design
- [ ] **3.2.6.6** RTL Support
- [ ] **3.2.6.7** Loading States
- [ ] **3.2.6.8** Error States

---

## 📋 Task 3.2.7: Integration & Testing

**هدف:** یکپارچه‌سازی و تست

### **Checklist:**

- [ ] **3.2.7.1** یکپارچه‌سازی با Controllers
- [ ] **3.2.7.2** یکپارچه‌سازی با Views
- [ ] **3.2.7.3** تست تمام Functions
- [ ] **3.2.7.4** تست Error Handling
- [ ] **3.2.7.5** تست Loading States
- [ ] **3.2.7.6** تست Responsive Design
- [ ] **3.2.7.7** تست Performance
- [ ] **3.2.7.8** تست Cross-Browser

---

## ✅ **Definition of Done**

- [ ] تمام JavaScript Files ایجاد شده‌اند
- [ ] تمام Charts کار می‌کنند
- [ ] Persian DatePicker استفاده شده است
- [ ] Error Handling کامل است
- [ ] Loading States کامل است
- [ ] Responsive Design است
- [ ] RTL Support است
- [ ] Chart.js تنظیم شده است
- [ ] CSS Styling کامل است
- [ ] تست شده است

---

## 📚 **مراجع**

- `Scripts/triage/triage-reports.js` - الگوی JavaScript
- `Areas/Admin/Views/DoctorHistory/DoctorReport.cshtml` - الگوی Chart.js
- `Docs/Knowledge-Base/` - Knowledge-Base

---

**تهیه‌کننده:** AI Assistant  
**تاریخ:** 1404/10/05  
**وضعیت:** ⏳ **در انتظار شروع**

