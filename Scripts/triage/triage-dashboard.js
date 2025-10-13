/**
 * مدیریت داشبورد تریاژ - Real-time Dashboard Management
 * طراحی شده برای سیستم‌های پزشکی کلینیک شفا
 */

var TriageDashboard = {
    // تنظیمات
    config: {
        refreshInterval: 30000, // 30 ثانیه
        chartColors: {
            primary: '#007bff',
            success: '#28a745',
            warning: '#ffc107',
            danger: '#dc3545',
            info: '#17a2b8',
            secondary: '#6c757d'
        }
    },

    // نمودارها
    charts: {
        levelDistribution: null,
        dailyTrend: null
    },

    /**
     * راه‌اندازی اولیه داشبورد
     */
    init: function() {
        console.log('TriageDashboard: Initializing...');
        
        // تنظیم به‌روزرسانی خودکار
        this.setupAutoRefresh();
        
        // تنظیم رویدادها
        this.setupEvents();
        
        // بارگذاری اولیه داده‌ها
        this.loadInitialData();
        
        console.log('TriageDashboard: Initialized successfully');
    },

    /**
     * تنظیم به‌روزرسانی خودکار
     */
    setupAutoRefresh: function() {
        var self = this;
        
        setInterval(function() {
            self.refreshStats();
        }, this.config.refreshInterval);
    },

    /**
     * تنظیم رویدادها
     */
    setupEvents: function() {
        var self = this;
        
        // دکمه به‌روزرسانی
        $(document).on('click', '[onclick*="TriageDashboard.refreshStats"]', function(e) {
            e.preventDefault();
            self.refreshStats();
        });
        
        // دکمه خروجی
        $(document).on('click', '[onclick*="TriageDashboard.exportDashboard"]', function(e) {
            e.preventDefault();
            self.exportDashboard();
        });
    },

    /**
     * بارگذاری اولیه داده‌ها
     */
    loadInitialData: function() {
        this.refreshStats();
        this.loadQuickStats();
    },

    /**
     * به‌روزرسانی آمار
     */
    refreshStats: function() {
        var self = this;
        
        $.ajax({
            url: '/TriageDashboard/GetRealTimeStats',
            type: 'POST',
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    self.updateStatsDisplay(response.data);
                    self.updateLastUpdateTime();
                } else {
                    console.error('TriageDashboard: Error refreshing stats:', response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageDashboard: AJAX error:', error);
            }
        });
    },

    /**
     * بارگذاری آمار سریع
     */
    loadQuickStats: function() {
        var self = this;
        
        $.ajax({
            url: '/TriageDashboard/GetQuickStats',
            type: 'POST',
            data: {
                startDate: new Date().toISOString().split('T')[0],
                endDate: new Date().toISOString().split('T')[0]
            },
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    self.updateQuickStats(response.data);
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageDashboard: Error loading quick stats:', error);
            }
        });
    },

    /**
     * به‌روزرسانی نمایش آمار
     */
    updateStatsDisplay: function(data) {
        // به‌روزرسانی کارت‌های آمار
        $('.info-box-number').each(function() {
            var $this = $(this);
            var text = $this.text();
            var newValue = data[this.id] || text;
            
            if (newValue !== text) {
                $this.fadeOut(200, function() {
                    $this.text(newValue).fadeIn(200);
                });
            }
        });
    },

    /**
     * به‌روزرسانی آمار سریع
     */
    updateQuickStats: function(data) {
        $('#total-assessments').text(data.TotalAssessments || 0);
        $('#completed-assessments').text(data.CompletedAssessments || 0);
        $('#average-wait-time').text(data.AverageWaitTime || 0);
        $('#critical-count').text(data.CriticalCount || 0);
    },

    /**
     * به‌روزرسانی زمان آخرین به‌روزرسانی
     */
    updateLastUpdateTime: function() {
        var now = new Date();
        var timeString = now.toLocaleTimeString('fa-IR');
        $('#last-update').text(timeString);
    },

    /**
     * ایجاد نمودار توزیع سطوح
     */
    createLevelDistributionChart: function() {
        var ctx = document.getElementById('level-distribution-chart');
        if (!ctx) return;

        var self = this;
        
        $.ajax({
            url: '/TriageDashboard/GetLevelDistribution',
            type: 'POST',
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    self.charts.levelDistribution = new Chart(ctx, {
                        type: 'doughnut',
                        data: response.data,
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {
                                legend: {
                                    position: 'bottom'
                                }
                            }
                        }
                    });
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageDashboard: Error loading level distribution:', error);
            }
        });
    },

    /**
     * ایجاد نمودار روند روزانه
     */
    createDailyTrendChart: function() {
        var ctx = document.getElementById('daily-trend-chart');
        if (!ctx) return;

        var self = this;
        
        $.ajax({
            url: '/TriageDashboard/GetDailyTrend',
            type: 'POST',
            data: {
                days: 7
            },
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    self.charts.dailyTrend = new Chart(ctx, {
                        type: 'line',
                        data: response.data,
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                y: {
                                    beginAtZero: true
                                }
                            }
                        }
                    });
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageDashboard: Error loading daily trend:', error);
            }
        });
    },

    /**
     * خروجی داشبورد
     */
    exportDashboard: function() {
        // ایجاد PDF از داشبورد
        var self = this;
        
        $.ajax({
            url: '/TriageDashboard/ExportDashboard',
            type: 'POST',
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    // دانلود فایل
                    window.open(response.downloadUrl, '_blank');
                    toastr.success('داشبورد با موفقیت صادر شد');
                } else {
                    toastr.error('خطا در صادر کردن داشبورد: ' + response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageDashboard: Error exporting dashboard:', error);
                toastr.error('خطا در صادر کردن داشبورد');
            }
        });
    },

    /**
     * نمایش هشدار
     */
    showAlert: function(message, type) {
        var alertClass = 'alert-' + (type || 'info');
        var alertHtml = '<div class="alert ' + alertClass + ' alert-dismissible fade show" role="alert">' +
                       '<i class="fas fa-info-circle"></i> ' + message +
                       '<button type="button" class="close" data-dismiss="alert">' +
                       '<span>&times;</span></button></div>';
        
        $('.card-body').prepend(alertHtml);
        
        // حذف خودکار بعد از 5 ثانیه
        setTimeout(function() {
            $('.alert').fadeOut();
        }, 5000);
    },

    /**
     * مدیریت خطا
     */
    handleError: function(error, context) {
        console.error('TriageDashboard Error [' + context + ']:', error);
        this.showAlert('خطا در ' + context + ': ' + error.message, 'danger');
    }
};

// راه‌اندازی خودکار
$(document).ready(function() {
    if (typeof TriageDashboard !== 'undefined') {
        TriageDashboard.init();
    }
});
