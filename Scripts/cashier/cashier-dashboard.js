/**
 * Cashier Dashboard JavaScript
 * مدیریت Dashboard منشی‌ها
 * 
 * طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
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
        performanceChart: null
    },
    
    // Initialize
    init: function() {
        var self = this;
        
        console.log('✅ Initializing Cashier Dashboard...');
        
        // Load initial data
        this.loadDailyStats();
        this.loadTopPerformers();
        
        // Setup auto refresh
        this.setupAutoRefresh();
        
        // Setup event listeners
        this.setupEventListeners();
        
        console.log('✅ Cashier Dashboard initialized successfully');
    },
    
    // Load Daily Stats
    loadDailyStats: function() {
        var self = this;
        var date = new Date().toISOString().split('T')[0]; // Today
        
        // Show loading
        $('.loading-overlay').show();
        
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
                    // Update charts if they exist
                    self.updateCharts(response.data);
                } else {
                    console.error('❌ خطا در دریافت آمار:', response.message);
                    self.showError(response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('❌ خطا در AJAX:', error);
                self.showError('خطا در دریافت آمار');
            },
            complete: function() {
                $('.loading-overlay').hide();
            }
        });
    },
    
    // Update Stats Cards
    updateStatsCards: function(data) {
        $('#total-transactions').text(data.TotalTransactions || 0);
        $('#total-amount').text(this.formatCurrency(data.TotalAmount || 0));
        $('#success-rate').text((data.SuccessRate || 0).toFixed(2) + '%');
        $('#avg-transaction-time').text((data.AverageTransactionTime || 0).toFixed(2));
        $('#discrepancy-count').text(data.DiscrepancyCount || 0);
        $('#sessions-opened').text(data.SessionsOpened || 0);
        $('#sessions-closed').text(data.SessionsClosed || 0);
    },
    
    // Format Currency
    formatCurrency: function(amount) {
        return new Intl.NumberFormat('fa-IR', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(amount);
    },
    
    // Load Top Performers
    loadTopPerformers: function() {
        var self = this;
        var fromDate = new Date();
        fromDate.setDate(fromDate.getDate() - 30);
        var toDate = new Date();
        
        $.ajax({
            url: self.config.apiBaseUrl + '/GetTopPerformers',
            type: 'POST',
            data: {
                fromDate: fromDate.toISOString().split('T')[0],
                toDate: toDate.toISOString().split('T')[0],
                topN: 5,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    // Top Performers are already loaded in the view
                    console.log('✅ Top Performers loaded');
                } else {
                    console.warn('⚠️ Failed to load Top Performers:', response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('❌ خطا در دریافت Top Performers:', error);
            }
        });
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
        
        // Refresh Button
        $('#refresh-btn').on('click', function() {
            self.loadDailyStats();
            self.loadTopPerformers();
        });
    },
    
    // Update Charts
    updateCharts: function(data) {
        // This will be implemented when we have chart data
        // For now, we'll create placeholder charts
        this.createTransactionsChart(data);
        this.createPerformanceChart(data);
    },
    
    // Create Transactions Chart
    createTransactionsChart: function(data) {
        var ctx = document.getElementById('transactionsChart');
        if (!ctx) return;
        
        var self = this;
        
        // Destroy existing chart if it exists
        if (self.charts.transactionsChart) {
            self.charts.transactionsChart.destroy();
        }
        
        // For now, create a simple chart with placeholder data
        // This should be replaced with actual transaction history data
        self.charts.transactionsChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['شنبه', 'یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه'],
                datasets: [{
                    label: 'تعداد تراکنش‌ها',
                    data: [0, 0, 0, 0, 0, 0, data.TotalTransactions || 0],
                    borderColor: self.config.chartColors.primary,
                    backgroundColor: self.config.chartColors.primary + '20',
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
                        position: 'top',
                        rtl: true
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    },
    
    // Create Performance Chart
    createPerformanceChart: function(data) {
        var ctx = document.getElementById('performanceChart');
        if (!ctx) return;
        
        var self = this;
        
        // Destroy existing chart if it exists
        if (self.charts.performanceChart) {
            self.charts.performanceChart.destroy();
        }
        
        // Create a bar chart for performance metrics
        self.charts.performanceChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['نرخ موفقیت', 'زمان میانگین', 'اختلاف‌ها'],
                datasets: [{
                    label: 'عملکرد',
                    data: [
                        data.SuccessRate || 0,
                        data.AverageTransactionTime || 0,
                        data.DiscrepancyCount || 0
                    ],
                    backgroundColor: [
                        self.config.chartColors.success,
                        self.config.chartColors.info,
                        self.config.chartColors.danger
                    ],
                    borderColor: [
                        self.config.chartColors.success,
                        self.config.chartColors.info,
                        self.config.chartColors.danger
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    },
    
    // Show Error
    showError: function(message) {
        // Use toastr if available, otherwise console
        if (typeof toastr !== 'undefined') {
            toastr.error(message, 'خطا', {
                timeOut: 5000,
                positionClass: 'toast-top-left'
            });
        } else {
            console.error('❌ Error:', message);
        }
    }
};

// Initialize on document ready
$(document).ready(function() {
    if (typeof Chart !== 'undefined') {
        CashierDashboard.init();
    } else {
        console.error('❌ Chart.js is not loaded');
    }
});

