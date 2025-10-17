/**
 * RECEPTION TOASTR SERVICE - PROFESSIONAL MEDICAL ENVIRONMENT
 * ===========================================================
 * 
 * اصول طراحی:
 * 1. SRP - مدیریت پیام‌ها
 * 2. DRY - عدم تکرار کد
 * 3. Medical Environment - محیط درمانی
 * 4. Production Ready - آماده production
 * 5. RTL Support - پشتیبانی از راست‌چین
 */

// Global Namespace برای Toastr Service
window.ReceptionToastr = window.ReceptionToastr || {};

// ========================================
// TOASTR CONFIGURATION - تنظیمات حرفه‌ای
// ========================================
window.ReceptionToastr.config = {
    // تنظیمات اصلی Toastr
    options: {
        closeButton: true,              // نمایش دکمه بستن
        debug: false,                   // غیر فعال کردن debug
        newestOnTop: true,              // نمایش پیام جدید در بالای لیست
        progressBar: true,              // نمایش نوار پیشرفت
        positionClass: "toast-top-left",// موقعیت پیام (راست‌چین)
        preventDuplicates: true,        // جلوگیری از تکرار پیام
        onclick: null,                  // کلیک روی پیام
        showDuration: "300",            // مدت زمان نمایش
        hideDuration: "1000",           // مدت زمان مخفی شدن
        timeOut: "5000",                // مدت زمان نمایش خودکار
        extendedTimeOut: "1000",        // مدت زمان اضافی
        showEasing: "swing",            // انیمیشن نمایش
        hideEasing: "linear",           // انیمیشن مخفی شدن
        showMethod: "fadeIn",           // روش نمایش
        hideMethod: "fadeOut"           // روش مخفی شدن
    },
    
    // تنظیمات فونت برای محیط درمانی
    font: {
        family: "'IRANSans', 'Tahoma', 'Arial', sans-serif",
        size: "14px",
        weight: "500"
    },
    
    // تنظیمات رنگ برای محیط درمانی
    colors: {
        success: "#28a745",     // سبز موفقیت
        error: "#dc3545",       // قرمز خطا
        warning: "#ffc107",     // زرد هشدار
        info: "#17a2b8"         // آبی اطلاعات
    }
};

// ========================================
// TOASTR SERVICE - سرویس مدیریت پیام‌ها
// ========================================
window.ReceptionToastr.service = {
    
    /**
     * راه‌اندازی Toastr
     */
    init: function() {
        console.log('ReceptionToastr: Starting initialization...');
        
        // بررسی وجود Toastr
        if (typeof window.toastr !== 'undefined' && window.toastr) {
            console.log('ReceptionToastr: Toastr library found');
            
            // بررسی وجود config
            if (window.ReceptionToastr && window.ReceptionToastr.config) {
                console.log('ReceptionToastr: Config found');
                
                // تنظیم گزینه‌های Toastr
                try {
                    window.toastr.options = window.ReceptionToastr.config.options;
                    console.log('ReceptionToastr: Options set successfully');
                    
                    // تنظیم فونت
                    this.setupFont();
                    
                    // تنظیم RTL
                    this.setupRTL();
                    
                    console.log('ReceptionToastr: Initialized successfully');
                } catch (error) {
                    console.error('ReceptionToastr: Error setting options:', error);
                }
            } else {
                console.error('ReceptionToastr: Config not found');
            }
        } else {
            console.error('ReceptionToastr: Toastr library not found');
            console.log('Available on window:', Object.keys(window).filter(key => key.toLowerCase().includes('toast')));
        }
    },
    
    /**
     * تنظیم فونت برای محیط درمانی
     */
    setupFont: function() {
        var style = document.createElement('style');
        style.textContent = `
            .toast {
                font-family: ${window.ReceptionToastr.config.font.family} !important;
                font-size: ${window.ReceptionToastr.config.font.size} !important;
                font-weight: ${window.ReceptionToastr.config.font.weight} !important;
                direction: rtl !important;
                text-align: right !important;
            }
            
            .toast-title {
                font-weight: 600 !important;
                margin-bottom: 8px !important;
            }
            
            .toast-message {
                line-height: 1.5 !important;
                margin-top: 4px !important;
            }
            
            .toast-close-button {
                font-size: 18px !important;
                font-weight: bold !important;
                color: #666 !important;
            }
            
            .toast-close-button:hover {
                color: #333 !important;
            }
        `;
        document.head.appendChild(style);
    },
    
    /**
     * تنظیم RTL برای محیط فارسی
     */
    setupRTL: function() {
        var style = document.createElement('style');
        style.textContent = `
            .toast-top-left {
                left: 20px !important;
                right: auto !important;
            }
            
            .toast-top-right {
                right: 20px !important;
                left: auto !important;
            }
            
            .toast-bottom-left {
                left: 20px !important;
                right: auto !important;
            }
            
            .toast-bottom-right {
                right: 20px !important;
                left: auto !important;
            }
            
            .toast-top-center {
                left: 50% !important;
                right: auto !important;
                transform: translateX(-50%) !important;
            }
            
            .toast-bottom-center {
                left: 50% !important;
                right: auto !important;
                transform: translateX(-50%) !important;
            }
        `;
        document.head.appendChild(style);
    },
    
    /**
     * نمایش پیام موفقیت
     */
    success: function(message, title) {
        if (typeof window.toastr !== 'undefined' && window.toastr) {
            window.toastr.success(message, title || 'موفقیت');
        } else {
            console.log('SUCCESS:', message);
        }
    },
    
    /**
     * نمایش پیام خطا
     */
    error: function(message, title) {
        if (typeof window.toastr !== 'undefined' && window.toastr) {
            window.toastr.error(message, title || 'خطا');
        } else {
            console.error('ERROR:', message);
        }
    },
    
    /**
     * نمایش پیام هشدار
     */
    warning: function(message, title) {
        if (typeof window.toastr !== 'undefined' && window.toastr) {
            window.toastr.warning(message, title || 'هشدار');
        } else {
            console.warn('WARNING:', message);
        }
    },
    
    /**
     * نمایش پیام اطلاعاتی
     */
    info: function(message, title) {
        if (typeof window.toastr !== 'undefined' && window.toastr) {
            window.toastr.info(message, title || 'اطلاعات');
        } else {
            console.info('INFO:', message);
        }
    },
    
    /**
     * پاک کردن تمام پیام‌ها
     */
    clear: function() {
        if (typeof window.toastr !== 'undefined' && window.toastr) {
            window.toastr.clear();
        }
    },
    
    /**
     * نمایش پیام با تایپ مشخص
     */
    show: function(type, message, title) {
        switch (type.toLowerCase()) {
            case 'success':
                this.success(message, title);
                break;
            case 'error':
                this.error(message, title);
                break;
            case 'warning':
                this.warning(message, title);
                break;
            case 'info':
                this.info(message, title);
                break;
            default:
                this.info(message, title);
        }
    }
};

// ========================================
// RECEPTION MESSAGES - پیام‌های ماژول پذیرش
// ========================================
window.ReceptionToastr.messages = {
    
    // پیام‌های اعتبارسنجی
    validation: {
        nationalCodeRequired: 'کد ملی الزامی است',
        nationalCodeInvalid: 'کد ملی وارد شده معتبر نیست',
        nationalCodeLength: 'کد ملی باید 10 رقم باشد',
        firstNameRequired: 'نام بیمار الزامی است',
        lastNameRequired: 'نام خانوادگی بیمار الزامی است',
        phoneNumberInvalid: 'شماره تلفن وارد شده معتبر نیست',
        birthDateInvalid: 'تاریخ تولد وارد شده معتبر نیست'
    },
    
    // پیام‌های موفقیت
    success: {
        patientFound: 'بیمار با موفقیت یافت شد',
        patientSaved: 'اطلاعات بیمار با موفقیت ذخیره شد',
        patientUpdated: 'اطلاعات بیمار با موفقیت به‌روزرسانی شد',
        insuranceLoaded: 'اطلاعات بیمه با موفقیت بارگذاری شد',
        insuranceSaved: 'اطلاعات بیمه با موفقیت ذخیره شد',
        departmentLoaded: 'دپارتمان‌ها با موفقیت بارگذاری شدند',
        departmentSaved: 'انتخاب دپارتمان با موفقیت ذخیره شد',
        serviceLoaded: 'خدمات با موفقیت بارگذاری شدند',
        serviceCalculated: 'محاسبه خدمات با موفقیت انجام شد',
        paymentProcessed: 'پرداخت با موفقیت انجام شد'
    },
    
    // پیام‌های خطا
    error: {
        patientNotFound: 'بیمار با این کد ملی یافت نشد',
        patientSaveError: 'خطا در ذخیره اطلاعات بیمار',
        insuranceLoadError: 'خطا در بارگذاری اطلاعات بیمه',
        departmentLoadError: 'خطا در بارگذاری دپارتمان‌ها',
        serviceLoadError: 'خطا در بارگذاری خدمات',
        serviceCalculateError: 'خطا در محاسبه خدمات',
        paymentProcessError: 'خطا در پردازش پرداخت',
        networkError: 'خطا در ارتباط با سرور',
        serverError: 'خطا در سرور',
        timeoutError: 'زمان درخواست به پایان رسید',
        validationError: 'خطا در اعتبارسنجی اطلاعات'
    },
    
    // پیام‌های هشدار
    warning: {
        patientDuplicate: 'بیمار با این کد ملی قبلاً ثبت شده است',
        insuranceExpired: 'بیمه بیمار منقضی شده است',
        insuranceInvalid: 'بیمه بیمار معتبر نیست',
        departmentUnavailable: 'دپارتمان انتخاب شده در دسترس نیست',
        serviceUnavailable: 'خدمت انتخاب شده در دسترس نیست'
    },
    
    // پیام‌های اطلاعاتی
    info: {
        patientSearching: 'در حال جستجوی بیمار...',
        patientSaving: 'در حال ذخیره اطلاعات بیمار...',
        insuranceLoading: 'در حال بارگذاری اطلاعات بیمه...',
        departmentLoading: 'در حال بارگذاری دپارتمان‌ها...',
        serviceLoading: 'در حال بارگذاری خدمات...',
        serviceCalculating: 'در حال محاسبه خدمات...',
        paymentProcessing: 'در حال پردازش پرداخت...'
    }
};

// ========================================
// RECEPTION TOASTR HELPERS - توابع کمکی
// ========================================
window.ReceptionToastr.helpers = {
    
    /**
     * نمایش پیام موفقیت با فرمت استاندارد
     */
    showSuccess: function(message, title) {
        window.ReceptionToastr.service.success(message, title);
    },
    
    /**
     * نمایش پیام خطا با فرمت استاندارد
     */
    showError: function(message, title) {
        window.ReceptionToastr.service.error(message, title);
    },
    
    /**
     * نمایش پیام هشدار با فرمت استاندارد
     */
    showWarning: function(message, title) {
        window.ReceptionToastr.service.warning(message, title);
    },
    
    /**
     * نمایش پیام اطلاعاتی با فرمت استاندارد
     */
    showInfo: function(message, title) {
        window.ReceptionToastr.service.info(message, title);
    },
    
    /**
     * نمایش پیام با تایپ مشخص
     */
    showMessage: function(type, message, title) {
        window.ReceptionToastr.service.show(type, message, title);
    },
    
    /**
     * نمایش پیام از منابع
     */
    showFromResources: function(type, key, title) {
        var message = window.ReceptionToastr.messages[type][key];
        if (message) {
            window.ReceptionToastr.service.show(type, message, title);
        } else {
            console.error('ReceptionToastr: Message not found for key:', key);
        }
    }
};

// ========================================
// AUTO INITIALIZATION - راه‌اندازی خودکار
// ========================================
$(document).ready(function() {
    // تاخیر برای اطمینان از بارگذاری Toastr
    setTimeout(function() {
        // بررسی وجود jQuery و Toastr
        if (typeof window.jQuery !== 'undefined' && typeof window.toastr !== 'undefined') {
            // راه‌اندازی Toastr Service
            window.ReceptionToastr.service.init();
            
            console.log('ReceptionToastr: Service ready for medical environment');
        } else {
            console.error('ReceptionToastr: jQuery or Toastr not available');
            console.log('jQuery available:', typeof window.jQuery !== 'undefined');
            console.log('Toastr available:', typeof window.toastr !== 'undefined');
        }
    }, 500);
});

// Export برای استفاده در ماژول‌های دیگر
if (typeof module !== 'undefined' && module.exports) {
    module.exports = window.ReceptionToastr;
}
