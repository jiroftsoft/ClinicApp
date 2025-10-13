/**
 * مدیریت گزارش‌گیری تریاژ - Advanced Reporting Management
 * طراحی شده برای سیستم‌های پزشکی کلینیک شفا
 */

var TriageReports = {
    // تنظیمات
    config: {
        chartColors: {
            primary: '#007bff',
            success: '#28a745',
            warning: '#ffc107',
            danger: '#dc3545',
            info: '#17a2b8',
            secondary: '#6c757d'
        },
        exportFormats: ['Excel', 'PDF', 'CSV']
    },

    // نمودارها
    charts: {
        trendChart: null,
        levelDistribution: null,
        statusDistribution: null
    },

    /**
     * راه‌اندازی اولیه گزارش‌گیری
     */
    init: function() {
        console.log('TriageReports: Initializing...');
        
        // تنظیم رویدادها
        this.setupEvents();
        
        // تنظیم فیلترها
        this.setupFilters();
        
        // بارگذاری اولیه داده‌ها
        this.loadInitialData();
        
        console.log('TriageReports: Initialized successfully');
    },

    /**
     * تنظیم رویدادها
     */
    setupEvents: function() {
        var self = this;
        
        // دکمه تولید گزارش
        $(document).on('click', 'button[type="submit"]', function(e) {
            e.preventDefault();
            self.generateReport();
        });
        
        // دکمه خروجی Excel
        $(document).on('click', '[onclick*="TriageReports.exportExcel"]', function(e) {
            e.preventDefault();
            self.exportExcel();
        });
        
        // دکمه خروجی PDF
        $(document).on('click', '[onclick*="TriageReports.exportPdf"]', function(e) {
            e.preventDefault();
            self.exportPdf();
        });
        
        // دکمه بازنشانی فیلترها
        $(document).on('click', '[onclick*="TriageReports.resetFilters"]', function(e) {
            e.preventDefault();
            self.resetFilters();
        });
        
        // تغییر فیلترها
        $(document).on('change', 'select, input[type="date"]', function() {
            self.loadQuickStats();
        });
    },

    /**
     * تنظیم فیلترها
     */
    setupFilters: function() {
        // تنظیم تاریخ پیش‌فرض
        var today = new Date();
        var lastWeek = new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000);
        
        $('#StartDate').val(lastWeek.toISOString().split('T')[0]);
        $('#EndDate').val(today.toISOString().split('T')[0]);
    },

    /**
     * بارگذاری اولیه داده‌ها
     */
    loadInitialData: function() {
        this.loadQuickStats();
        this.loadTrendChart();
    },

    /**
     * بارگذاری آمار سریع
     */
    loadQuickStats: function() {
        var self = this;
        var startDate = $('#StartDate').val();
        var endDate = $('#EndDate').val();
        var departmentId = $('#DepartmentId').val();
        
        $.ajax({
            url: '/TriageReport/GetQuickStats',
            type: 'POST',
            data: {
                startDate: startDate,
                endDate: endDate,
                departmentId: departmentId
            },
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    self.updateQuickStats(response.data);
                } else {
                    console.error('TriageReports: Error loading quick stats:', response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageReports: Error loading quick stats:', error);
            }
        });
    },

    /**
     * بارگذاری نمودار روند
     */
    loadTrendChart: function() {
        var self = this;
        var startDate = $('#StartDate').val();
        var endDate = $('#EndDate').val();
        var departmentId = $('#DepartmentId').val();
        
        $.ajax({
            url: '/TriageReport/GetTrendChart',
            type: 'POST',
            data: {
                startDate: startDate,
                endDate: endDate,
                departmentId: departmentId
            },
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    self.createTrendChart(response.data);
                } else {
                    console.error('TriageReports: Error loading trend chart:', response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageReports: Error loading trend chart:', error);
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
     * ایجاد نمودار روند
     */
    createTrendChart: function(data) {
        var ctx = document.getElementById('trend-chart');
        if (!ctx) return;

        var self = this;
        
        if (self.charts.trendChart) {
            self.charts.trendChart.destroy();
        }

        self.charts.trendChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: data.Labels || [],
                datasets: [{
                    label: 'ارزیابی‌های روزانه',
                    data: data.Data || [],
                    borderColor: self.config.chartColors.primary,
                    backgroundColor: self.config.chartColors.primary + '20',
                    tension: 0.1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                },
                plugins: {
                    legend: {
                        display: true,
                        position: 'top'
                    }
                }
            }
        });
    },

    /**
     * تولید گزارش
     */
    generateReport: function() {
        var self = this;
        var form = $('form');
        
        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            dataType: 'json',
            beforeSend: function() {
                self.showLoading();
            },
            success: function(response) {
                if (response.success) {
                    self.displayReport(response.data);
                    toastr.success('گزارش با موفقیت تولید شد');
                } else {
                    toastr.error('خطا در تولید گزارش: ' + response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageReports: Error generating report:', error);
                toastr.error('خطا در تولید گزارش');
            },
            complete: function() {
                self.hideLoading();
            }
        });
    },

    /**
     * نمایش گزارش
     */
    displayReport: function(data) {
        // به‌روزرسانی جدول نتایج
        $('#report-table tbody').html('');
        
        if (data.ReportItems && data.ReportItems.length > 0) {
            data.ReportItems.forEach(function(item) {
                var row = self.createReportRow(item);
                $('#report-table tbody').append(row);
            });
        }
        
        // به‌روزرسانی خلاصه
        if (data.Summary) {
            self.updateSummary(data.Summary);
        }
        
        // ایجاد نمودارهای توزیع
        if (data.ReportItems) {
            self.createDistributionCharts(data.ReportItems);
        }
    },

    /**
     * ایجاد ردیف گزارش
     */
    createReportRow: function(item) {
        var levelBadge = self.getLevelBadge(item.Level);
        var statusBadge = self.getStatusBadge(item.Status);
        
        return '<tr>' +
            '<td><a href="/Triage/Details/' + item.AssessmentId + '" class="text-primary">#' + item.AssessmentId + '</a></td>' +
            '<td><strong>' + item.PatientFullName + '</strong><br><small class="text-muted">#' + item.PatientId + '</small></td>' +
            '<td>' + item.ChiefComplaint + '</td>' +
            '<td>' + levelBadge + '</td>' +
            '<td><span class="badge badge-info">' + item.Priority + '</span></td>' +
            '<td>' + statusBadge + '</td>' +
            '<td><small>' + self.formatDateTime(item.ArrivalAt) + '</small></td>' +
            '<td><small>' + self.formatDateTime(item.TriageStartAt) + '</small></td>' +
            '<td><small>' + (item.TriageEndAt ? self.formatDateTime(item.TriageEndAt) : '-') + '</small></td>' +
            '<td>' + (item.TotalTimeDisplay || '-') + '</td>' +
            '<td>' + (item.ReassessmentCount > 0 ? '<span class="badge badge-info">' + item.ReassessmentCount + '</span>' : '-') + '</td>' +
            '<td>' + (item.DepartmentName || '-') + '</td>' +
            '<td>' + (item.DoctorName || '-') + '</td>' +
            '<td>' + (item.HasIsolation ? '<span class="badge badge-warning"><i class="fas fa-shield-alt"></i> ' + item.Isolation + '</span>' : '-') + '</td>' +
            '</tr>';
    },

    /**
     * دریافت نشان سطح
     */
    getLevelBadge: function(level) {
        var badges = {
            'ESI1': '<span class="badge badge-danger">بحرانی</span>',
            'ESI2': '<span class="badge badge-warning">فوری</span>',
            'ESI3': '<span class="badge badge-info">عاجل</span>',
            'ESI4': '<span class="badge badge-success">کم‌عاجل</span>',
            'ESI5': '<span class="badge badge-secondary">غیرعاجل</span>'
        };
        return badges[level] || '<span class="badge badge-secondary">نامشخص</span>';
    },

    /**
     * دریافت نشان وضعیت
     */
    getStatusBadge: function(status) {
        var badges = {
            'Completed': '<span class="badge badge-success">تکمیل شده</span>',
            'Pending': '<span class="badge badge-warning">در انتظار</span>',
            'InProgress': '<span class="badge badge-info">در حال انجام</span>',
            'Cancelled': '<span class="badge badge-secondary">لغو شده</span>'
        };
        return badges[status] || '<span class="badge badge-secondary">نامشخص</span>';
    },

    /**
     * فرمت تاریخ و زمان
     */
    formatDateTime: function(dateTime) {
        if (!dateTime) return '-';
        var date = new Date(dateTime);
        return date.toLocaleDateString('fa-IR') + ' ' + date.toLocaleTimeString('fa-IR');
    },

    /**
     * به‌روزرسانی خلاصه
     */
    updateSummary: function(summary) {
        // به‌روزرسانی کارت‌های خلاصه
        $('.card-body h4').each(function() {
            var $this = $(this);
            var text = $this.text();
            
            if (text.includes('کل ارزیابی‌ها')) {
                $this.text(summary.TotalAssessments);
            } else if (text.includes('تکمیل شده')) {
                $this.text(summary.CompletedAssessments);
            } else if (text.includes('میانگین انتظار')) {
                $this.text(summary.AverageWaitTimeDisplay);
            } else if (text.includes('بحرانی')) {
                $this.text(summary.CriticalAssessments);
            }
        });
    },

    /**
     * ایجاد نمودارهای توزیع
     */
    createDistributionCharts: function(reportItems) {
        // نمودار توزیع سطوح
        this.createLevelDistributionChart(reportItems);
        
        // نمودار توزیع وضعیت
        this.createStatusDistributionChart(reportItems);
    },

    /**
     * ایجاد نمودار توزیع سطوح
     */
    createLevelDistributionChart: function(reportItems) {
        var ctx = document.getElementById('level-distribution-chart');
        if (!ctx) return;

        var self = this;
        var levelCounts = this.countByLevel(reportItems);
        
        if (self.charts.levelDistribution) {
            self.charts.levelDistribution.destroy();
        }

        self.charts.levelDistribution = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['بحرانی', 'فوری', 'عاجل', 'کم‌عاجل', 'غیرعاجل'],
                datasets: [{
                    data: [
                        levelCounts.ESI1 || 0,
                        levelCounts.ESI2 || 0,
                        levelCounts.ESI3 || 0,
                        levelCounts.ESI4 || 0,
                        levelCounts.ESI5 || 0
                    ],
                    backgroundColor: [
                        self.config.chartColors.danger,
                        self.config.chartColors.warning,
                        self.config.chartColors.info,
                        self.config.chartColors.success,
                        self.config.chartColors.secondary
                    ]
                }]
            },
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
    },

    /**
     * ایجاد نمودار توزیع وضعیت
     */
    createStatusDistributionChart: function(reportItems) {
        var ctx = document.getElementById('status-distribution-chart');
        if (!ctx) return;

        var self = this;
        var statusCounts = this.countByStatus(reportItems);
        
        if (self.charts.statusDistribution) {
            self.charts.statusDistribution.destroy();
        }

        self.charts.statusDistribution = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: ['تکمیل شده', 'در انتظار', 'در حال انجام', 'لغو شده'],
                datasets: [{
                    data: [
                        statusCounts.Completed || 0,
                        statusCounts.Pending || 0,
                        statusCounts.InProgress || 0,
                        statusCounts.Cancelled || 0
                    ],
                    backgroundColor: [
                        self.config.chartColors.success,
                        self.config.chartColors.warning,
                        self.config.chartColors.info,
                        self.config.chartColors.secondary
                    ]
                }]
            },
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
    },

    /**
     * شمارش بر اساس سطح
     */
    countByLevel: function(reportItems) {
        var counts = {};
        reportItems.forEach(function(item) {
            counts[item.Level] = (counts[item.Level] || 0) + 1;
        });
        return counts;
    },

    /**
     * شمارش بر اساس وضعیت
     */
    countByStatus: function(reportItems) {
        var counts = {};
        reportItems.forEach(function(item) {
            counts[item.Status] = (counts[item.Status] || 0) + 1;
        });
        return counts;
    },

    /**
     * خروجی Excel
     */
    exportExcel: function() {
        var self = this;
        
        $.ajax({
            url: '/TriageReport/ExportExcel',
            type: 'POST',
            data: $('form').serialize(),
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    window.open(response.downloadUrl, '_blank');
                    toastr.success('گزارش Excel با موفقیت صادر شد');
                } else {
                    toastr.error('خطا در صادر کردن Excel: ' + response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageReports: Error exporting Excel:', error);
                toastr.error('خطا در صادر کردن Excel');
            }
        });
    },

    /**
     * خروجی PDF
     */
    exportPdf: function() {
        var self = this;
        
        $.ajax({
            url: '/TriageReport/ExportPdf',
            type: 'POST',
            data: $('form').serialize(),
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    window.open(response.downloadUrl, '_blank');
                    toastr.success('گزارش PDF با موفقیت صادر شد');
                } else {
                    toastr.error('خطا در صادر کردن PDF: ' + response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageReports: Error exporting PDF:', error);
                toastr.error('خطا در صادر کردن PDF');
            }
        });
    },

    /**
     * بازنشانی فیلترها
     */
    resetFilters: function() {
        // بازنشانی تاریخ‌ها
        var today = new Date();
        var lastWeek = new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000);
        
        $('#StartDate').val(lastWeek.toISOString().split('T')[0]);
        $('#EndDate').val(today.toISOString().split('T')[0]);
        
        // بازنشانی سایر فیلترها
        $('#DepartmentId, #Level, #Status, #ReportType, #ExportFormat').val('');
        
        // بارگذاری مجدد آمار
        this.loadQuickStats();
    },

    /**
     * نمایش لودینگ
     */
    showLoading: function() {
        $('.card-body').append('<div class="overlay"><i class="fas fa-2x fa-sync fa-spin"></i></div>');
    },

    /**
     * مخفی کردن لودینگ
     */
    hideLoading: function() {
        $('.overlay').remove();
    },

    /**
     * مدیریت خطا
     */
    handleError: function(error, context) {
        console.error('TriageReports Error [' + context + ']:', error);
        toastr.error('خطا در ' + context + ': ' + error.message);
    }
};

// راه‌اندازی خودکار
$(document).ready(function() {
    if (typeof TriageReports !== 'undefined') {
        TriageReports.init();
    }
});
