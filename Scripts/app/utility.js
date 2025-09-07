/**
 * فایل توابع عمومی و کمکی برای سیستم مدیریت کلینیک
 * ClinicApp - Utility Functions
 */

// توابع نمایش پیام‌ها
window.ClinicApp = window.ClinicApp || {};

ClinicApp.Utils = {
    /**
     * نمایش پیام موفقیت
     */
    showSuccess: function(message, title = 'موفقیت') {
        Swal.fire({
            icon: 'success',
            title: title,
            text: message,
            confirmButtonText: 'تأیید',
            confirmButtonColor: '#28a745'
        });
    },

    /**
     * نمایش پیام خطا
     */
    showError: function(message, title = 'خطا') {
        Swal.fire({
            icon: 'error',
            title: title,
            text: message,
            confirmButtonText: 'تأیید',
            confirmButtonColor: '#dc3545'
        });
    },

    /**
     * نمایش پیام هشدار
     */
    showWarning: function(message, title = 'هشدار') {
        Swal.fire({
            icon: 'warning',
            title: title,
            text: message,
            confirmButtonText: 'تأیید',
            confirmButtonColor: '#ffc107'
        });
    },

    /**
     * نمایش پیام تأیید
     */
    showConfirm: function(message, title = 'تأیید', confirmText = 'بله', cancelText = 'خیر') {
        return Swal.fire({
            title: title,
            text: message,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: confirmText,
            cancelButtonText: cancelText,
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#dc3545'
        });
    },

    /**
     * نمایش Loading
     */
    showLoading: function(title = 'در حال پردازش...') {
        Swal.fire({
            title: title,
            allowOutsideClick: false,
            allowEscapeKey: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    },

    /**
     * بستن Loading
     */
    hideLoading: function() {
        Swal.close();
    },

    /**
     * فرمت کردن تاریخ شمسی
     */
    formatPersianDate: function(date) {
        if (!date) return '';
        // پیاده‌سازی فرمت تاریخ شمسی
        return date;
    },

    /**
     * اعتبارسنجی فرم
     */
    validateForm: function(formId) {
        const form = document.getElementById(formId);
        if (!form) return false;
        
        return form.checkValidity();
    },

    /**
     * دریافت داده از data-* attributes
     */
    getDataAttribute: function(element, attribute) {
        return element.getAttribute('data-' + attribute);
    },

    /**
     * تنظیم data-* attribute
     */
    setDataAttribute: function(element, attribute, value) {
        element.setAttribute('data-' + attribute, value);
    },

    /**
     * مدیریت رفرش خودکار با Page Visibility API
     */
    AutoRefresh: {
        intervals: {},
        
        start: function(key, callback, interval = 30000) {
            this.stop(key);
            
            this.intervals[key] = setInterval(() => {
                if (!document.hidden) {
                    callback();
                }
            }, interval);
        },
        
        stop: function(key) {
            if (this.intervals[key]) {
                clearInterval(this.intervals[key]);
                delete this.intervals[key];
            }
        },
        
        stopAll: function() {
            Object.keys(this.intervals).forEach(key => {
                this.stop(key);
            });
        }
    },

    /**
     * مدیریت Draft (پیش‌نویس) فرم‌ها
     */
    DraftManager: {
        save: function(formId, data) {
            const key = 'draft_' + formId;
            localStorage.setItem(key, JSON.stringify({
                data: data,
                timestamp: new Date().toISOString()
            }));
        },
        
        load: function(formId) {
            const key = 'draft_' + formId;
            const draft = localStorage.getItem(key);
            if (draft) {
                return JSON.parse(draft);
            }
            return null;
        },
        
        clear: function(formId) {
            const key = 'draft_' + formId;
            localStorage.removeItem(key);
        },
        
        isExpired: function(draft, maxAge = 24 * 60 * 60 * 1000) { // 24 ساعت
            if (!draft || !draft.timestamp) return true;
            const age = new Date() - new Date(draft.timestamp);
            return age > maxAge;
        }
    }
};

// مدیریت رفرش خودکار هنگام تغییر تب
document.addEventListener('visibilitychange', function() {
    if (document.hidden) {
        // توقف رفرش‌های خودکار هنگام مخفی بودن تب
        ClinicApp.Utils.AutoRefresh.stopAll();
    }
});

// مدیریت رفرش خودکار هنگام بسته شدن صفحه
window.addEventListener('beforeunload', function() {
    ClinicApp.Utils.AutoRefresh.stopAll();
});
