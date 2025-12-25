/**
 * NOTIFICATION HELPER - Reception Module
 * =======================================
 * 
 * سیستم مدیریت Notifications برای فرم پذیرش
 * استفاده از Toastr برای پیام‌های عادی و SweetAlert2 برای تأییدیه‌ها
 * 
 * ویژگی‌ها:
 * ✅ سبک و سریع
 * ✅ RTL Support کامل
 * ✅ Medical Environment Optimized
 * ✅ Zero Cache Compatible
 * ✅ Production Ready
 * 
 * @author ClinicApp Development Team
 * @version 1.0.0
 * @since 2025-12-25
 */

(function(global) {
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
            confirmButtonColor: '#00796b', // Medical Primary
            cancelButtonColor: '#e53935',  // Medical Danger
            allowOutsideClick: false,
            allowEscapeKey: true
        }
    };

    // ========================================
    // TOASTR INITIALIZATION - راه‌اندازی Toastr
    // ========================================
    function initializeToastr() {
        if (typeof toastr === 'undefined') {
            console.warn('[NotificationHelper] Toastr is not loaded. Falling back to alert().');
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
            console.warn('[NotificationHelper] SweetAlert2 is not loaded. Falling back to confirm().');
            return false;
        }
        return true;
    }

    // ========================================
    // NOTIFICATION HELPER - سرویس اعلان
    // ========================================
    const NotificationHelper = {
        /**
         * نمایش پیام موفقیت
         * @param {string} message - متن پیام
         * @param {string} [title='موفقیت'] - عنوان پیام
         * @param {object} [options] - تنظیمات اضافی
         */
        success: function(message, title, options) {
            if (!initializeToastr()) {
                alert((title || 'موفقیت') + ': ' + (message || 'عملیات با موفقیت انجام شد'));
                return;
            }
            
            const opts = Object.assign({}, CONFIG.toastr, options || {});
            toastr.options = opts;
            toastr.success(message || 'عملیات با موفقیت انجام شد', title || 'موفقیت');
        },

        /**
         * نمایش پیام خطا
         * @param {string} message - متن پیام
         * @param {string} [title='خطا'] - عنوان پیام
         * @param {object} [options] - تنظیمات اضافی
         */
        error: function(message, title, options) {
            if (!initializeToastr()) {
                alert((title || 'خطا') + ': ' + (message || 'خطایی رخ داده است'));
                return;
            }
            
            const opts = Object.assign({}, CONFIG.toastr, options || {});
            toastr.options = opts;
            toastr.error(message || 'خطایی رخ داده است', title || 'خطا');
        },

        /**
         * نمایش پیام هشدار
         * @param {string} message - متن پیام
         * @param {string} [title='هشدار'] - عنوان پیام
         * @param {object} [options] - تنظیمات اضافی
         */
        warning: function(message, title, options) {
            if (!initializeToastr()) {
                alert((title || 'هشدار') + ': ' + (message || 'هشدار'));
                return;
            }
            
            const opts = Object.assign({}, CONFIG.toastr, options || {});
            toastr.options = opts;
            toastr.warning(message || 'هشدار', title || 'هشدار');
        },

        /**
         * نمایش پیام اطلاعات
         * @param {string} message - متن پیام
         * @param {string} [title='اطلاعات'] - عنوان پیام
         * @param {object} [options] - تنظیمات اضافی
         */
        info: function(message, title, options) {
            if (!initializeToastr()) {
                alert((title || 'اطلاعات') + ': ' + (message || 'اطلاعات'));
                return;
            }
            
            const opts = Object.assign({}, CONFIG.toastr, options || {});
            toastr.options = opts;
            toastr.info(message || 'اطلاعات', title || 'اطلاعات');
        },

        /**
         * نمایش پیام تأیید با SweetAlert2
         * @param {string} message - متن پیام
         * @param {string} [title='تأیید'] - عنوان پیام
         * @param {function} [onConfirm] - callback در صورت تأیید
         * @param {function} [onCancel] - callback در صورت انصراف
         * @param {object} [options] - تنظیمات اضافی
         */
        confirm: function(message, title, onConfirm, onCancel, options) {
            if (!initializeSweetAlert()) {
                if (confirm(message || title || 'آیا مطمئن هستید؟')) {
                    if (onConfirm) onConfirm();
                } else {
                    if (onCancel) onCancel();
                }
                return;
            }

            const opts = Object.assign({}, {
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
            }, options || {});

            Swal.fire(opts).then((result) => {
                if (result.isConfirmed) {
                    if (onConfirm) onConfirm(result);
                } else if (result.isDismissed) {
                    if (onCancel) onCancel(result);
                }
            });
        },

        /**
         * نمایش پیام خطای بحرانی با SweetAlert2
         * @param {string} message - متن پیام
         * @param {string} [title='خطای بحرانی'] - عنوان پیام
         * @param {object} [options] - تنظیمات اضافی
         */
        criticalError: function(message, title, options) {
            if (!initializeSweetAlert()) {
                alert((title || 'خطای بحرانی') + ': ' + (message || 'خطای مهمی رخ داده است'));
                return;
            }

            const opts = Object.assign({}, {
                title: title || 'خطای بحرانی',
                text: message || 'خطای مهمی رخ داده است. لطفاً صفحه را رفرش کنید.',
                icon: 'error',
                confirmButtonText: CONFIG.sweetAlert.confirmButtonText,
                confirmButtonColor: CONFIG.sweetAlert.confirmButtonColor,
                allowOutsideClick: false,
                allowEscapeKey: false
            }, options || {});

            Swal.fire(opts);
        },

        /**
         * نمایش پیام موفقیت با SweetAlert2
         * @param {string} message - متن پیام
         * @param {string} [title='موفقیت'] - عنوان پیام
         * @param {function} [callback] - callback بعد از بستن
         * @param {object} [options] - تنظیمات اضافی
         */
        successAlert: function(message, title, callback, options) {
            if (!initializeSweetAlert()) {
                alert((title || 'موفقیت') + ': ' + (message || 'عملیات با موفقیت انجام شد'));
                if (callback) callback();
                return;
            }

            const opts = Object.assign({}, {
                title: title || 'موفقیت',
                text: message || 'عملیات با موفقیت انجام شد',
                icon: 'success',
                confirmButtonText: CONFIG.sweetAlert.confirmButtonText,
                confirmButtonColor: CONFIG.sweetAlert.confirmButtonColor
            }, options || {});

            Swal.fire(opts).then((result) => {
                if (callback) callback(result);
            });
        },

        /**
         * نمایش Loading Spinner
         * @param {string} [message='در حال پردازش...'] - متن پیام
         */
        showLoading: function(message) {
            if (!initializeSweetAlert()) {
                console.log('[NotificationHelper] Loading: ' + (message || 'در حال پردازش...'));
                return;
            }

            Swal.fire({
                title: message || 'در حال پردازش...',
                allowOutsideClick: false,
                allowEscapeKey: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });
        },

        /**
         * بستن Loading Spinner
         */
        hideLoading: function() {
            if (initializeSweetAlert() && Swal.isVisible()) {
                Swal.close();
            }
        },

        /**
         * پاک کردن تمام Toastr های فعال
         */
        clearAll: function() {
            if (initializeToastr()) {
                toastr.clear();
            }
        },

        /**
         * پاک کردن آخرین Toastr
         */
        clearLast: function() {
            if (initializeToastr()) {
                toastr.remove();
            }
        }
    };

    // ========================================
    // TEMPDATA HANDLER - مدیریت TempData از سرور
    // ========================================
    /**
     * نمایش پیام‌های TempData از سرور
     * باید در View با Razor صدا زده شود
     */
    function handleServerNotifications() {
        // این تابع در View با Razor پر می‌شود
        // مثال:
        // @if (TempData["Success"] != null) {
        //     <script>
        //         NotificationHelper.success('@TempData["Success"]');
        //     </script>
        // }
    }

    // ========================================
    // AUTO-INITIALIZATION
    // ========================================
    function initialize() {
        initializeToastr();
        console.log('[NotificationHelper] Initialized successfully');
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }

    // ========================================
    // EXPORT - صادرات
    // ========================================
    // Global namespace
    global.NotificationHelper = NotificationHelper;

    // Alias for shorter access
    global.Notify = NotificationHelper;

    // Export for module systems (if available)
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = NotificationHelper;
    }

})(window);

