/**
 * تریاژ صف - مدیریت صف تریاژ
 * Production-Ready, RTL Support, Real-time Updates
 */

(function($) {
    'use strict';

    // Namespace برای مدیریت صف
    window.TriageQueue = {
        // تنظیمات
        config: {
            refreshInterval: 30000, // 30 ثانیه
            autoRefresh: true,
            soundEnabled: true
        },

        // متغیرهای داخلی
        _refreshTimer: null,
        _currentDepartmentId: null,

        // مقداردهی اولیه
        init: function(departmentId) {
            this._currentDepartmentId = departmentId;
            this.bindEvents();
            this.startAutoRefresh();
        },

        // اتصال رویدادها
        bindEvents: function() {
            var self = this;

            // دکمه فراخوانی بیمار بعدی
            $(document).on('click', '.btn-call-next', function(e) {
                e.preventDefault();
                self.callNext();
            });

            // دکمه تکمیل صف
            $(document).on('click', '.btn-complete-queue', function(e) {
                e.preventDefault();
                var queueId = $(this).data('queue-id');
                self.completeQueue(queueId);
            });

            // دکمه مرتب‌سازی صف
            $(document).on('click', '.btn-reorder-queue', function(e) {
                e.preventDefault();
                self.reorderQueue();
            });

            // دکمه تازه‌سازی
            $(document).on('click', '.btn-refresh-queue', function(e) {
                e.preventDefault();
                self.refreshQueue();
            });

            // فیلترها
            $(document).on('change', '#queue-filters select', function() {
                self.applyFilters();
            });

            // جستجو
            $(document).on('input', '#queue-search', function() {
                self.applySearch();
            });
        },

        // شروع به‌روزرسانی خودکار
        startAutoRefresh: function() {
            if (this.config.autoRefresh) {
                this._refreshTimer = setInterval(() => {
                    this.refreshQueue();
                }, this.config.refreshInterval);
            }
        },

        // توقف به‌روزرسانی خودکار
        stopAutoRefresh: function() {
            if (this._refreshTimer) {
                clearInterval(this._refreshTimer);
                this._refreshTimer = null;
            }
        },

        // تازه‌سازی صف
        refreshQueue: function() {
            var self = this;
            
            TriageAjax.getQueueList(this._currentDepartmentId, function(response) {
                if (response.success) {
                    self.updateQueueTable(response.data);
                    self.updateQueueStats();
                } else {
                    TriageAjax.showError(response.message);
                }
            });
        },

        // به‌روزرسانی جدول صف
        updateQueueTable: function(queueItems) {
            var tbody = $('#queueTable tbody');
            tbody.empty();

            if (queueItems.length === 0) {
                tbody.append('<tr><td colspan="8" class="text-center">هیچ بیمار در صف نیست</td></tr>');
                return;
            }

            queueItems.forEach(function(item) {
                var row = self.createQueueRow(item);
                tbody.append(row);
            });

            // اعمال فیلترها
            this.applyFilters();
        },

        // ایجاد ردیف صف
        createQueueRow: function(item) {
            var urgencyClass = item.isUrgent ? 'urgent' : 'normal';
            var overdueClass = item.isOverdue ? 'overdue' : 'ontime';
            var statusClass = this.getStatusClass(item.status);

            return `
                <tr class="${urgencyClass} ${overdueClass}">
                    <td>${item.queueId}</td>
                    <td>${item.patientFullName}</td>
                    <td>${item.chiefComplaint}</td>
                    <td>
                        <span class="badge ${this.getLevelBadgeClass(item.level)}">
                            ${item.levelDisplayName}
                        </span>
                    </td>
                    <td>${item.priority}</td>
                    <td>
                        <span class="badge ${statusClass}">
                            ${item.statusDisplayName}
                        </span>
                    </td>
                    <td>${item.waitTimeDisplay}</td>
                    <td>
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-info btn-sm" title="جزئیات" onclick="TriageQueue.showDetails(${item.queueId})">
                                <i class="fas fa-eye"></i>
                            </button>
                            ${item.status === 'Waiting' ? `
                                <button class="btn btn-warning btn-sm btn-call-next" title="فراخوانی" data-queue-id="${item.queueId}">
                                    <i class="fas fa-bell"></i>
                                </button>
                            ` : ''}
                            ${item.status === 'Called' ? `
                                <button class="btn btn-success btn-sm btn-complete-queue" title="تکمیل" data-queue-id="${item.queueId}">
                                    <i class="fas fa-check"></i>
                                </button>
                            ` : ''}
                        </div>
                    </td>
                </tr>
            `;
        },

        // به‌روزرسانی آمار صف
        updateQueueStats: function() {
            var self = this;
            
            TriageAjax.getQueueStats(this._currentDepartmentId, function(response) {
                if (response.success) {
                    self.updateStatsDisplay(response.data);
                }
            });
        },

        // به‌روزرسانی نمایش آمار
        updateStatsDisplay: function(stats) {
            $('#totalWaiting').text(stats.totalWaiting);
            $('#criticalWaiting').text(stats.criticalWaiting);
            $('#highPriorityWaiting').text(stats.highPriorityWaiting);
            $('#overdueCount').text(stats.overdueCount);
            $('#completedToday').text(stats.completedToday);
            $('#averageWaitTime').text(stats.averageWaitTimeMinutes.toFixed(1) + ' دقیقه');

            // به‌روزرسانی وضعیت هشدارها
            this.updateAlerts(stats);
        },

        // به‌روزرسانی هشدارها
        updateAlerts: function(stats) {
            var alerts = [];

            if (stats.criticalWaiting > 0) {
                alerts.push(`⚠️ ${stats.criticalWaiting} بیمار بحرانی در انتظار`);
            }

            if (stats.overdueCount > 0) {
                alerts.push(`🚨 ${stats.overdueCount} بیمار در انتظار بیش از حد مجاز`);
            }

            if (stats.averageWaitTimeMinutes > 60) {
                alerts.push(`⏰ میانگین زمان انتظار ${stats.averageWaitTimeMinutes.toFixed(1)} دقیقه`);
            }

            this.showAlerts(alerts);
        },

        // نمایش هشدارها
        showAlerts: function(alerts) {
            var alertContainer = $('#queueAlerts');
            alertContainer.empty();

            if (alerts.length === 0) {
                alertContainer.append('<div class="alert alert-success">✅ همه بیماران در زمان مناسب</div>');
                return;
            }

            alerts.forEach(function(alert) {
                alertContainer.append(`<div class="alert alert-warning">${alert}</div>`);
            });

            // پخش صدا برای هشدارهای مهم
            if (this.config.soundEnabled && alerts.length > 0) {
                this.playAlertSound();
            }
        },

        // پخش صدا هشدار
        playAlertSound: function() {
            try {
                var audio = new Audio('/Content/sounds/alert.wav');
                audio.play().catch(function() {
                    // صدا پخش نشد
                });
            } catch (e) {
                // مرورگر از صدا پشتیبانی نمی‌کند
            }
        },

        // فراخوانی بیمار بعدی
        callNext: function() {
            var self = this;
            
            TriageAjax.callNext(this._currentDepartmentId, function(response) {
                if (response.success) {
                    TriageAjax.showSuccess('بیمار فراخوانی شد');
                    self.refreshQueue();
                } else {
                    TriageAjax.showError(response.message);
                }
            });
        },

        // تکمیل صف
        completeQueue: function(queueId) {
            var self = this;
            
            TriageAjax.completeQueue(queueId, function(response) {
                if (response.success) {
                    TriageAjax.showSuccess('صف تریاژ تکمیل شد');
                    self.refreshQueue();
                } else {
                    TriageAjax.showError(response.message);
                }
            });
        },

        // مرتب‌سازی صف
        reorderQueue: function() {
            var self = this;
            
            TriageAjax.reorderQueue(this._currentDepartmentId, function(response) {
                if (response.success) {
                    TriageAjax.showSuccess('صف بر اساس اولویت مرتب شد');
                    self.refreshQueue();
                } else {
                    TriageAjax.showError(response.message);
                }
            });
        },

        // اعمال فیلترها
        applyFilters: function() {
            var status = $('#filter-status').val();
            var priority = $('#filter-priority').val();
            var level = $('#filter-level').val();

            $('#queueTable tbody tr').each(function() {
                var row = $(this);
                var show = true;

                if (status && status !== 'All') {
                    var rowStatus = row.find('.badge').text().trim();
                    if (rowStatus !== status) show = false;
                }

                if (priority && priority !== 'All') {
                    var rowPriority = row.find('td:eq(4)').text().trim();
                    if (rowPriority !== priority) show = false;
                }

                if (level && level !== 'All') {
                    var rowLevel = row.find('.badge').text().trim();
                    if (rowLevel.indexOf(level) === -1) show = false;
                }

                row.toggle(show);
            });
        },

        // اعمال جستجو
        applySearch: function() {
            var searchTerm = $('#queue-search').val().toLowerCase();

            $('#queueTable tbody tr').each(function() {
                var row = $(this);
                var text = row.text().toLowerCase();
                row.toggle(text.indexOf(searchTerm) !== -1);
            });
        },

        // نمایش جزئیات
        showDetails: function(queueId) {
            // TODO: پیاده‌سازی نمایش جزئیات
            TriageAjax.showInfo('جزئیات صف #' + queueId);
        },

        // Helper Methods
        getStatusClass: function(status) {
            switch (status) {
                case 'Waiting': return 'badge-warning';
                case 'Called': return 'badge-info';
                case 'Completed': return 'badge-success';
                default: return 'badge-light';
            }
        },

        getLevelBadgeClass: function(level) {
            switch (level) {
                case 'ESI1': return 'badge-danger';
                case 'ESI2': return 'badge-warning';
                case 'ESI3': return 'badge-info';
                case 'ESI4': return 'badge-success';
                case 'ESI5': return 'badge-secondary';
                default: return 'badge-light';
            }
        }
    };

    // مقداردهی اولیه خودکار
    $(document).ready(function() {
        if ($('#queueTable').length > 0) {
            var departmentId = $('#queueTable').data('department-id');
            TriageQueue.init(departmentId);
        }
    });

})(jQuery);
