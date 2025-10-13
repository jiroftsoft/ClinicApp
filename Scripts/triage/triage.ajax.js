/**
 * تریاژ AJAX - عملیات AJAX برای ماژول تریاژ
 * Production-Ready, RTL Support, Error Handling
 */

(function($) {
    'use strict';

    // Namespace برای تریاژ
    window.TriageAjax = {
        // تنظیمات پیش‌فرض
        config: {
            timeout: 30000,
            retryCount: 3,
            retryDelay: 1000
        },

        // عملیات AJAX اصلی
        ajax: function(options) {
            var defaults = {
                timeout: this.config.timeout,
                retryCount: this.config.retryCount,
                retryDelay: this.config.retryDelay,
                beforeSend: function() {
                    // نمایش لودینگ
                    TriageAjax.showLoading();
                },
                complete: function() {
                    // مخفی کردن لودینگ
                    TriageAjax.hideLoading();
                },
                error: function(xhr, status, error) {
                    // مدیریت خطا
                    TriageAjax.handleError(xhr, status, error);
                }
            };

            var settings = $.extend({}, defaults, options);
            return $.ajax(settings);
        },

        // دریافت خلاصه ارزیابی
        getSummary: function(assessmentId, callback) {
            this.ajax({
                url: '/Triage/GetSummary',
                type: 'POST',
                data: { assessmentId: assessmentId },
                success: function(response) {
                    if (callback) callback(response);
                }
            });
        },

        // دریافت علائم حیاتی
        getVitalSigns: function(assessmentId, callback) {
            this.ajax({
                url: '/Triage/GetVitalSigns',
                type: 'POST',
                data: { assessmentId: assessmentId },
                success: function(response) {
                    if (callback) callback(response);
                }
            });
        },

        // دریافت پایش‌های مجدد
        getReassessments: function(assessmentId, callback) {
            this.ajax({
                url: '/TriageReassessment/GetReassessments',
                type: 'POST',
                data: { assessmentId: assessmentId },
                success: function(response) {
                    if (callback) callback(response);
                }
            });
        },

        // ایجاد پایش مجدد
        createReassessment: function(data, callback) {
            this.ajax({
                url: '/TriageReassessment/CreateReassessment',
                type: 'POST',
                data: data,
                success: function(response) {
                    if (callback) callback(response);
                }
            });
        },

        // دریافت لیست صف
        getQueueList: function(departmentId, callback) {
            this.ajax({
                url: '/TriageQueue/GetQueueList',
                type: 'POST',
                data: { departmentId: departmentId },
                success: function(response) {
                    if (callback) callback(response);
                }
            });
        },

        // دریافت آمار صف
        getQueueStats: function(departmentId, callback) {
            this.ajax({
                url: '/TriageQueue/GetQueueStats',
                type: 'POST',
                data: { departmentId: departmentId },
                success: function(response) {
                    if (callback) callback(response);
                }
            });
        },

        // فراخوانی بیمار بعدی
        callNext: function(departmentId, callback) {
            this.ajax({
                url: '/TriageQueue/CallNextAjax',
                type: 'POST',
                data: { departmentId: departmentId },
                success: function(response) {
                    if (callback) callback(response);
                }
            });
        },

        // تکمیل صف
        completeQueue: function(queueId, callback) {
            this.ajax({
                url: '/TriageQueue/CompleteAjax',
                type: 'POST',
                data: { queueId: queueId },
                success: function(response) {
                    if (callback) callback(response);
                }
            });
        },

        // مرتب‌سازی صف
        reorderQueue: function(departmentId, callback) {
            this.ajax({
                url: '/TriageQueue/ReorderAjax',
                type: 'POST',
                data: { departmentId: departmentId },
                success: function(response) {
                    if (callback) callback(response);
                }
            });
        },

        // دریافت آمار سریع
        getQuickStats: function(startDate, endDate, departmentId, callback) {
            this.ajax({
                url: '/TriageReport/GetQuickStats',
                type: 'POST',
                data: { 
                    startDate: startDate, 
                    endDate: endDate, 
                    departmentId: departmentId 
                },
                success: function(response) {
                    if (callback) callback(response);
                }
            });
        },

        // دریافت نمودار روند
        getTrendChart: function(startDate, endDate, departmentId, callback) {
            this.ajax({
                url: '/TriageReport/GetTrendChart',
                type: 'POST',
                data: { 
                    startDate: startDate, 
                    endDate: endDate, 
                    departmentId: departmentId 
                },
                success: function(response) {
                    if (callback) callback(response);
                }
            });
        },

        // نمایش لودینگ
        showLoading: function() {
            if ($('#triageLoading').length === 0) {
                $('body').append('<div id="triageLoading" class="triage-loading"><i class="fas fa-spinner fa-spin"></i></div>');
            }
            $('#triageLoading').show();
        },

        // مخفی کردن لودینگ
        hideLoading: function() {
            $('#triageLoading').hide();
        },

        // مدیریت خطا
        handleError: function(xhr, status, error) {
            var message = 'خطا در ارتباط با سرور';
            
            if (xhr.status === 0) {
                message = 'خطا در اتصال به اینترنت';
            } else if (xhr.status === 404) {
                message = 'صفحه مورد نظر یافت نشد';
            } else if (xhr.status === 500) {
                message = 'خطای داخلی سرور';
            } else if (xhr.status === 403) {
                message = 'دسترسی غیرمجاز';
            }

            this.showError(message);
        },

        // نمایش پیغام خطا
        showError: function(message) {
            this.hideLoading();
            
            if (typeof toastr !== 'undefined') {
                toastr.error(message, 'خطا');
            } else {
                alert(message);
            }
        },

        // نمایش پیغام موفقیت
        showSuccess: function(message) {
            if (typeof toastr !== 'undefined') {
                toastr.success(message, 'موفقیت');
            } else {
                alert(message);
            }
        },

        // نمایش پیغام هشدار
        showWarning: function(message) {
            if (typeof toastr !== 'undefined') {
                toastr.warning(message, 'هشدار');
            } else {
                alert(message);
            }
        },

        // نمایش پیغام اطلاعات
        showInfo: function(message) {
            if (typeof toastr !== 'undefined') {
                toastr.info(message, 'اطلاعات');
            } else {
                alert(message);
            }
        }
    };

    // CSS برای لودینگ
    if (!$('#triageLoadingStyles').length) {
        $('head').append(`
            <style id="triageLoadingStyles">
                .triage-loading {
                    position: fixed;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    background: rgba(0, 0, 0, 0.5);
                    z-index: 9999;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    color: white;
                    font-size: 24px;
                }
                .triage-loading i {
                    margin-left: 10px;
                }
            </style>
        `);
    }

})(jQuery);
