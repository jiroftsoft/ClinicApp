/**
 * Cashier Reports JavaScript
 * مدیریت تعاملات و عملیات گزارشات صندوق
 * 
 * طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md, DEVELOPMENT_CONTRACT.md
 * 
 * ویژگی‌های کلیدی:
 * - مدیریت فرم‌های فیلتر
 * - مدیریت Quick Actions
 * - مدیریت Export
 * - مدیریت AJAX Calls
 * - مدیریت Summary Cards
 */

var CashierReports = {
    // تنظیمات
    config: {
        apiBaseUrl: '/Payment/CashierReport',
        refreshInterval: 60000, // 60 ثانیه
        dateFormat: 'YYYY/MM/DD'
    },
    
    // Initialize
    init: function() {
        var self = this;
        
        console.log('✅ Initializing Cashier Reports...');
        
        // Setup event listeners
        this.setupEventListeners();
        
        // Setup Quick Actions
        this.setupQuickActions();
        
        // Load summary data if on Index page
        if ($('#summary-cards').length > 0) {
            this.loadSummaryData();
        }
        
        console.log('✅ Cashier Reports initialized successfully');
    },
    
    // Setup Event Listeners
    setupEventListeners: function() {
        var self = this;
        
        // Filter form submit
        $('#filter-form').on('submit', function(e) {
            // Validation will be handled by form
            console.log('📊 Filter form submitted');
        });
        
        // Report type radio buttons
        $('input[name="reportType"]').on('change', function() {
            var reportType = $(this).val();
            console.log('📊 Report type changed to:', reportType);
            self.handleReportTypeChange(reportType);
        });
        
        // Cashier dropdown change
        $('#CashierId').on('change', function() {
            var cashierId = $(this).val();
            console.log('📊 Cashier changed to:', cashierId);
        });
    },
    
    // Setup Quick Actions
    setupQuickActions: function() {
        var self = this;
        
        function getSelectedCashierId() {
            var $sel = $('#CashierId');
            return ($sel.length && $sel.val()) ? $sel.val() : null;
        }
        
        // Today Report (نیاز به انتخاب منشی)
        $('#btn-today-report').on('click', function() {
            var cashierId = getSelectedCashierId();
            if (!cashierId) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({ icon: 'warning', title: 'انتخاب منشی', text: 'لطفاً ابتدا یک منشی را انتخاب کنید.' });
                } else {
                    alert('لطفاً ابتدا یک منشی را انتخاب کنید.');
                }
                return;
            }
            var today = new Date();
            var todayStr = self.formatDateForUrl(today);
            window.location.href = self.config.apiBaseUrl + '/DailyReport?cashierId=' + encodeURIComponent(cashierId) + '&date=' + todayStr;
        });
        
        // This Week Report (نیاز به انتخاب منشی)
        $('#btn-this-week-report').on('click', function() {
            var cashierId = getSelectedCashierId();
            if (!cashierId) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({ icon: 'warning', title: 'انتخاب منشی', text: 'لطفاً ابتدا یک منشی را انتخاب کنید.' });
                } else {
                    alert('لطفاً ابتدا یک منشی را انتخاب کنید.');
                }
                return;
            }
            var today = new Date();
            var weekStart = new Date(today);
            weekStart.setDate(today.getDate() - today.getDay());
            var weekStartStr = self.formatDateForUrl(weekStart);
            var todayStr = self.formatDateForUrl(today);
            window.location.href = self.config.apiBaseUrl + '/RangeReport?cashierId=' + encodeURIComponent(cashierId) + '&fromDate=' + weekStartStr + '&toDate=' + todayStr;
        });
        
        // This Month Report (نیاز به انتخاب منشی)
        $('#btn-this-month-report').on('click', function() {
            var cashierId = getSelectedCashierId();
            if (!cashierId) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({ icon: 'warning', title: 'انتخاب منشی', text: 'لطفاً ابتدا یک منشی را انتخاب کنید.' });
                } else {
                    alert('لطفاً ابتدا یک منشی را انتخاب کنید.');
                }
                return;
            }
            var today = new Date();
            var monthStart = new Date(today.getFullYear(), today.getMonth(), 1);
            var monthStartStr = self.formatDateForUrl(monthStart);
            var todayStr = self.formatDateForUrl(today);
            window.location.href = self.config.apiBaseUrl + '/RangeReport?cashierId=' + encodeURIComponent(cashierId) + '&fromDate=' + monthStartStr + '&toDate=' + todayStr;
        });
        
        // All Cashiers Summary (بدون نیاز به منشی)
        $('#btn-all-cashiers-summary').on('click', function() {
            var today = new Date();
            var monthStart = new Date(today);
            monthStart.setDate(today.getDate() - 30); // Last 30 days
            
            var monthStartStr = self.formatDateForUrl(monthStart);
            var todayStr = self.formatDateForUrl(today);
            
            window.location.href = self.config.apiBaseUrl + '/AllCashiersSummary?fromDate=' + monthStartStr + '&toDate=' + todayStr;
        });
    },
    
    // Handle Report Type Change
    handleReportTypeChange: function(reportType) {
        // Show/hide relevant fields based on report type
        switch(parseInt(reportType)) {
            case 1: // Daily
                $('#StartDatePicker').closest('.col-md-3').show();
                $('#EndDatePicker').closest('.col-md-3').hide();
                break;
            case 2: // Monthly
                $('#StartDatePicker').closest('.col-md-3').hide();
                $('#EndDatePicker').closest('.col-md-3').hide();
                // TODO: Show Year/Month selectors
                break;
            case 3: // Range
                $('#StartDatePicker').closest('.col-md-3').show();
                $('#EndDatePicker').closest('.col-md-3').show();
                break;
            case 4: // All Cashiers
                $('#StartDatePicker').closest('.col-md-3').show();
                $('#EndDatePicker').closest('.col-md-3').show();
                break;
            case 5: // Compare
                $('#StartDatePicker').closest('.col-md-3').show();
                $('#EndDatePicker').closest('.col-md-3').show();
                break;
        }
    },
    
    // Load Summary Data
    loadSummaryData: function() {
        var self = this;
        
        // Get filter values
        var cashierId = $('#CashierId').val() || '';
        var startDate = $('#StartDatePicker').val() || '';
        var endDate = $('#EndDatePicker').val() || '';
        
        // If no dates, use default (last 7 days)
        if (!startDate || !endDate) {
            var today = new Date();
            var weekAgo = new Date(today);
            weekAgo.setDate(today.getDate() - 7);
            
            startDate = self.formatDateForUrl(weekAgo);
            endDate = self.formatDateForUrl(today);
        }
        
        // TODO: Make AJAX call to get summary data
        // For now, we'll leave it empty as the data should come from server
        console.log('📊 Loading summary data...', {
            cashierId: cashierId,
            startDate: startDate,
            endDate: endDate
        });
    },
    
    // Format Date for URL
    formatDateForUrl: function(date) {
        if (!date) return '';
        
        var year = date.getFullYear();
        var month = String(date.getMonth() + 1).padStart(2, '0');
        var day = String(date.getDate()).padStart(2, '0');
        
        return year + '-' + month + '-' + day;
    },
    
    // Format Currency
    formatCurrency: function(amount) {
        if (!amount && amount !== 0) return '-';
        
        return new Intl.NumberFormat('fa-IR', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(amount);
    },
    
    // Format Number
    formatNumber: function(number) {
        if (!number && number !== 0) return '-';
        
        return new Intl.NumberFormat('fa-IR', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(number);
    },
    
    // Format Percentage
    formatPercentage: function(value) {
        if (!value && value !== 0) return '-';
        
        return value.toFixed(2) + '%';
    },
    
    // Show Error
    showError: function(message) {
        // Use notification helper if available
        if (typeof NotificationHelper !== 'undefined') {
            NotificationHelper.showError(message);
        } else {
            alert('خطا: ' + message);
        }
    },
    
    // Show Success
    showSuccess: function(message) {
        // Use notification helper if available
        if (typeof NotificationHelper !== 'undefined') {
            NotificationHelper.showSuccess(message);
        } else {
            alert('موفق: ' + message);
        }
    },
    
    // Show Loading
    showLoading: function(show) {
        if (show) {
            if ($('.loading-overlay').length === 0) {
                $('body').append('<div class="loading-overlay"><div class="spinner-border text-primary" role="status"><span class="sr-only">در حال بارگذاری...</span></div></div>');
            }
            $('.loading-overlay').show();
        } else {
            $('.loading-overlay').hide();
        }
    },
    
    // Export to Excel
    exportToExcel: function(cashierId, fromDate, toDate, reportType) {
        var self = this;
        
        var url = self.config.apiBaseUrl + '/ExportToExcel' +
            '?cashierId=' + encodeURIComponent(cashierId || '') +
            '&fromDate=' + encodeURIComponent(fromDate || '') +
            '&toDate=' + encodeURIComponent(toDate || '') +
            '&reportType=' + encodeURIComponent(reportType || '');
        
        // Add anti-forgery token
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            url += '&__RequestVerificationToken=' + encodeURIComponent(token);
        }
        
        window.location.href = url;
    },
    
    // Export to PDF
    exportToPdf: function(cashierId, fromDate, toDate, reportType) {
        var self = this;
        
        var url = self.config.apiBaseUrl + '/ExportToPdf' +
            '?cashierId=' + encodeURIComponent(cashierId || '') +
            '&fromDate=' + encodeURIComponent(fromDate || '') +
            '&toDate=' + encodeURIComponent(toDate || '') +
            '&reportType=' + encodeURIComponent(reportType || '');
        
        // Add anti-forgery token
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            url += '&__RequestVerificationToken=' + encodeURIComponent(token);
        }
        
        window.location.href = url;
    },
    
    // Update Summary Cards
    updateSummaryCards: function(data) {
        if (!data) return;
        
        if (data.TotalTransactions !== undefined) {
            $('#total-transactions').text(this.formatNumber(data.TotalTransactions));
        }
        
        if (data.TotalAmount !== undefined) {
            $('#total-amount').text(this.formatCurrency(data.TotalAmount));
        }
        
        if (data.TotalCashiers !== undefined) {
            $('#total-cashiers').text(this.formatNumber(data.TotalCashiers));
        }
        
        if (data.SuccessRate !== undefined) {
            $('#success-rate').text(this.formatPercentage(data.SuccessRate));
        }
    }
};

// Initialize on document ready
$(document).ready(function() {
    if (typeof CashierReports !== 'undefined') {
        CashierReports.init();
    }
});

