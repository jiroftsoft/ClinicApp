/**
 * Admin Notification Service - Production Ready
 * ==============================================
 * 
 * سیستم مدیریت پیام‌های کاربرپسند برای Admin Panel
 * استفاده از Toastr برای پیام‌های عادی و SweetAlert برای پیام‌های مهم
 * 
 * اصول طراحی:
 * 1. SRP - مدیریت پیام‌ها
 * 2. DRY - عدم تکرار کد
 * 3. Production Ready - آماده production
 * 4. RTL Support - پشتیبانی از راست‌چین
 * 5. Strongly-Typed - استفاده از ViewModels
 */

(function() {
    'use strict';

    // ========================================
    // CONFIGURATION - تنظیمات
    // ========================================
    const CONFIG = {
        toastr: {
            closeButton: true,
            debug: false,
            newestOnTop: true,
            progressBar: true,
            positionClass: "toast-top-left", // برای RTL
            preventDuplicates: true,
            onclick: null,
            showDuration: "300",
            hideDuration: "1000",
            timeOut: "5000",
            extendedTimeOut: "1000",
            showEasing: "swing",
            hideEasing: "linear",
            showMethod: "fadeIn",
            hideMethod: "fadeOut",
            rtl: true
        },
        sweetAlert: {
            confirmButtonText: 'تأیید',
            cancelButtonText: 'انصراف',
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            allowOutsideClick: false,
            allowEscapeKey: true
        }
    };

    // ========================================
    // TOASTR INITIALIZATION - راه‌اندازی Toastr
    // ========================================
    function initializeToastr() {
        if (typeof toastr === 'undefined') {
            console.warn('Toastr is not loaded. Please include toastr library.');
            return false;
        }

        // تنظیمات پیش‌فرض Toastr
        toastr.options = CONFIG.toastr;
        return true;
    }

    // ========================================
    // SWEETALERT INITIALIZATION - راه‌اندازی SweetAlert
    // ========================================
    function initializeSweetAlert() {
        if (typeof Swal === 'undefined') {
            console.warn('SweetAlert2 is not loaded. Please include SweetAlert2 library.');
            return false;
        }
        return true;
    }

    // ========================================
    // NOTIFICATION SERVICE - سرویس اعلان
    // ========================================
    const NotificationService = {
        /**
         * نمایش پیام موفقیت
         */
        success: function(message, title) {
            if (!initializeToastr()) return;
            toastr.success(message || 'عملیات با موفقیت انجام شد', title || 'موفقیت');
        },

        /**
         * نمایش پیام خطا
         */
        error: function(message, title) {
            if (!initializeToastr()) return;
            toastr.error(message || 'خطایی رخ داده است', title || 'خطا');
        },

        /**
         * نمایش پیام هشدار
         */
        warning: function(message, title) {
            if (!initializeToastr()) return;
            toastr.warning(message || 'هشدار', title || 'هشدار');
        },

        /**
         * نمایش پیام اطلاعات
         */
        info: function(message, title) {
            if (!initializeToastr()) return;
            toastr.info(message || 'اطلاعات', title || 'اطلاعات');
        },

        /**
         * نمایش پیام تأیید با SweetAlert
         */
        confirm: function(message, title, onConfirm, onCancel) {
            if (!initializeSweetAlert()) {
                if (confirm(message || title || 'آیا مطمئن هستید؟')) {
                    if (onConfirm) onConfirm();
                } else {
                    if (onCancel) onCancel();
                }
                return;
            }

            Swal.fire({
                title: title || 'تأیید',
                text: message || 'آیا مطمئن هستید؟',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: CONFIG.sweetAlert.confirmButtonText,
                cancelButtonText: CONFIG.sweetAlert.cancelButtonText,
                confirmButtonColor: CONFIG.sweetAlert.confirmButtonColor,
                cancelButtonColor: CONFIG.sweetAlert.cancelButtonColor,
                allowOutsideClick: CONFIG.sweetAlert.allowOutsideClick,
                allowEscapeKey: CONFIG.sweetAlert.allowEscapeKey
            }).then((result) => {
                if (result.isConfirmed) {
                    if (onConfirm) onConfirm();
                } else {
                    if (onCancel) onCancel();
                }
            });
        },

        /**
         * نمایش پیام خطای بحرانی با SweetAlert
         */
        criticalError: function(message, title) {
            if (!initializeSweetAlert()) {
                alert(message || title || 'خطای بحرانی');
                return;
            }

            Swal.fire({
                title: title || 'خطای بحرانی',
                text: message || 'خطای مهمی رخ داده است. لطفاً صفحه را refresh کنید.',
                icon: 'error',
                confirmButtonText: CONFIG.sweetAlert.confirmButtonText,
                confirmButtonColor: CONFIG.sweetAlert.confirmButtonColor,
                allowOutsideClick: false,
                allowEscapeKey: false
            });
        },

        /**
         * نمایش پیام موفقیت با SweetAlert
         */
        successAlert: function(message, title) {
            if (!initializeSweetAlert()) {
                alert(message || title || 'موفقیت');
                return;
            }

            Swal.fire({
                title: title || 'موفقیت',
                text: message || 'عملیات با موفقیت انجام شد',
                icon: 'success',
                confirmButtonText: CONFIG.sweetAlert.confirmButtonText,
                confirmButtonColor: CONFIG.sweetAlert.confirmButtonColor
            });
        }
    };

    // ========================================
    // TEMPDATA HANDLER - مدیریت TempData از سرور
    // ========================================
    function handleTempDataNotifications() {
        // این تابع باید در View صدا زده شود
        // با استفاده از Razor syntax برای خواندن TempData
    }

    // ========================================
    // EXPORT - صادرات
    // ========================================
    // Global namespace
    window.AdminNotification = NotificationService;

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeToastr);
    } else {
        initializeToastr();
    }

})();

