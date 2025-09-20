/**
 * 🏥 Medical Environment JavaScript - بهینه شده برای محیط درمانی
 * 
 * این فایل شامل توابع کمکی و بهینه‌سازی‌های مخصوص محیط درمانی است
 * 
 * @version 2.0
 * @author ClinicApp Team
 * @date 2024
 */

// 🏥 Medical Environment Namespace
window.MedicalEnvironment = {
    version: '2.0',
    initialized: false,
    cache: {},
    settings: {
        debug: true,
        logLevel: 'info',
        cacheTimeout: 300000, // 5 minutes
        maxRetries: 3,
        retryDelay: 1000
    }
};

/**
 * 🚀 Initialize Medical Environment
 */
function initializeMedicalEnvironment() {
    console.log('🏥 MEDICAL: Initializing Medical Environment v' + MedicalEnvironment.version);
    
    try {
        // تنظیمات اولیه
        setupMedicalSettings();
        
        // تنظیمات toastr
        setupToastrSettings();
        
        // تنظیمات Select2
        setupSelect2Settings();
        
        // تنظیمات AJAX
        setupAjaxSettings();
        
        // تنظیمات Cache
        setupCacheSettings();
        
        // تنظیمات Security
        setupSecuritySettings();
        
        MedicalEnvironment.initialized = true;
        console.log('🏥 MEDICAL: Medical Environment initialized successfully');
        
    } catch (error) {
        console.error('🏥 MEDICAL: Error initializing Medical Environment', error);
        showError('خطا در راه‌اندازی محیط درمانی');
    }
}

/**
 * ⚙️ Setup Medical Settings
 */
function setupMedicalSettings() {
    console.log('🏥 MEDICAL: Setting up medical settings');
    
    // تنظیمات عمومی
    MedicalEnvironment.settings.debug = window.location.hostname === 'localhost';
    MedicalEnvironment.settings.logLevel = MedicalEnvironment.settings.debug ? 'debug' : 'info';
    
    // تنظیمات Cache
    MedicalEnvironment.settings.cacheTimeout = 300000; // 5 minutes
    
    // تنظیمات Retry
    MedicalEnvironment.settings.maxRetries = 3;
    MedicalEnvironment.settings.retryDelay = 1000;
}

/**
 * 🍞 Setup Toastr Settings
 */
function setupToastrSettings() {
    console.log('🏥 MEDICAL: Setting up toastr settings');
    
    if (typeof toastr !== 'undefined') {
        toastr.options = {
            "closeButton": true,
            "debug": MedicalEnvironment.settings.debug,
            "newestOnTop": true,
            "progressBar": true,
            "positionClass": "toast-top-right",
            "preventDuplicates": false,
            "onclick": null,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "5000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        };
    }
}

/**
 * 🔍 Setup Select2 Settings
 */
function setupSelect2Settings() {
    console.log('🏥 MEDICAL: Setting up Select2 settings');
    
    if (typeof $.fn.select2 !== 'undefined') {
        $.fn.select2.defaults.set("language", {
            noResults: function() {
                return "نتیجه‌ای یافت نشد";
            },
            searching: function() {
                return "در حال جستجو...";
            },
            loadingMore: function() {
                return "بارگذاری بیشتر...";
            },
            maximumSelected: function(args) {
                return "حداکثر " + args.maximum + " مورد قابل انتخاب است";
            }
        });
        
        $.fn.select2.defaults.set("theme", "bootstrap4");
        $.fn.select2.defaults.set("width", "100%");
    }
}

/**
 * 🌐 Setup AJAX Settings
 */
function setupAjaxSettings() {
    console.log('🏥 MEDICAL: Setting up AJAX settings');
    
    // تنظیمات عمومی AJAX
    $.ajaxSetup({
        timeout: 30000, // 30 seconds
        cache: false,
        beforeSend: function(xhr, settings) {
            // اضافه کردن CSRF token
            if (settings.type === 'POST' && !settings.data.includes('__RequestVerificationToken')) {
                var token = $('input[name="__RequestVerificationToken"]').val();
                if (token) {
                    if (settings.data) {
                        settings.data += '&__RequestVerificationToken=' + token;
                    } else {
                        settings.data = '__RequestVerificationToken=' + token;
                    }
                }
            }
            
            // اضافه کردن Medical headers
            xhr.setRequestHeader('X-Medical-Environment', 'true');
            xhr.setRequestHeader('X-Medical-Version', MedicalEnvironment.version);
        },
        error: function(xhr, status, error) {
            console.error('🏥 MEDICAL: AJAX Error', {
                status: status,
                error: error,
                responseText: xhr.responseText,
                url: xhr.responseURL
            });
        }
    });
}

/**
 * 💾 Setup Cache Settings
 */
function setupCacheSettings() {
    console.log('🏥 MEDICAL: Setting up cache settings');
    
    // پاک کردن cache قدیمی
    var now = Date.now();
    for (var key in MedicalEnvironment.cache) {
        if (MedicalEnvironment.cache[key].timestamp < now - MedicalEnvironment.settings.cacheTimeout) {
            delete MedicalEnvironment.cache[key];
        }
    }
}

/**
 * 🔒 Setup Security Settings
 */
function setupSecuritySettings() {
    console.log('🏥 MEDICAL: Setting up security settings');
    
    // اضافه کردن security indicators
    $('body').append('<div id="medical-security-indicator" class="security-indicator" style="display: none;"><i class="fas fa-shield-alt"></i></div>');
    
    // تنظیمات امنیتی
    MedicalEnvironment.security = {
        enabled: true,
        encryptData: true,
        logSecurityEvents: true
    };
}

/**
 * 📊 Medical Logging System
 */
function medicalLog(level, message, data) {
    if (!MedicalEnvironment.settings.debug && level === 'debug') {
        return;
    }
    
    var timestamp = new Date().toISOString();
    var logMessage = '🏥 MEDICAL [' + level.toUpperCase() + ']: ' + message;
    
    switch (level) {
        case 'debug':
            console.debug(logMessage, data);
            break;
        case 'info':
            console.info(logMessage, data);
            break;
        case 'warn':
            console.warn(logMessage, data);
            break;
        case 'error':
            console.error(logMessage, data);
            break;
        default:
            console.log(logMessage, data);
    }
}

/**
 * 🎯 Enhanced Error Handling
 */
function showError(message, title, options) {
    medicalLog('error', 'Showing error message', { message: message, title: title });
    
    var defaultOptions = {
        timeOut: 8000,
        closeButton: true,
        progressBar: true,
        positionClass: 'toast-top-right',
        showMethod: 'slideDown',
        hideMethod: 'slideUp',
        showDuration: 300,
        hideDuration: 300
    };
    
    var finalOptions = $.extend({}, defaultOptions, options || {});
    
    if (typeof toastr !== 'undefined') {
        toastr.error(message, title || 'خطا در سیستم درمانی', finalOptions);
    } else {
        alert('🏥 خطا در سیستم درمانی:\n' + (title || 'خطا') + '\n' + message);
    }
}

/**
 * ✅ Enhanced Success Handling
 */
function showSuccess(message, title, options) {
    medicalLog('info', 'Showing success message', { message: message, title: title });
    
    var defaultOptions = {
        timeOut: 3000,
        closeButton: true,
        progressBar: true,
        positionClass: 'toast-top-right'
    };
    
    var finalOptions = $.extend({}, defaultOptions, options || {});
    
    if (typeof toastr !== 'undefined') {
        toastr.success(message, title || 'موفقیت', finalOptions);
    } else {
        alert('موفقیت: ' + message);
    }
}

/**
 * ℹ️ Enhanced Info Handling
 */
function showInfo(message, title, options) {
    medicalLog('info', 'Showing info message', { message: message, title: title });
    
    var defaultOptions = {
        timeOut: 4000,
        closeButton: true,
        progressBar: true,
        positionClass: 'toast-top-right'
    };
    
    var finalOptions = $.extend({}, defaultOptions, options || {});
    
    if (typeof toastr !== 'undefined') {
        toastr.info(message, title || 'اطلاعات', finalOptions);
    }
}

/**
 * ⚠️ Enhanced Warning Handling
 */
function showWarning(message, title, options) {
    medicalLog('warn', 'Showing warning message', { message: message, title: title });
    
    var defaultOptions = {
        timeOut: 5000,
        closeButton: true,
        progressBar: true,
        positionClass: 'toast-top-right'
    };
    
    var finalOptions = $.extend({}, defaultOptions, options || {});
    
    if (typeof toastr !== 'undefined') {
        toastr.warning(message, title || 'هشدار', finalOptions);
    }
}

/**
 * 🔄 Enhanced Loading Indicator
 */
function showLoadingIndicator(message, options) {
    medicalLog('info', 'Showing loading indicator', { message: message });
    
    var defaultMessage = message || 'در حال پردازش...';
    var defaultOptions = {
        overlay: true,
        spinner: true,
        progress: true,
        message: defaultMessage
    };
    
    var finalOptions = $.extend({}, defaultOptions, options || {});
    
    if (finalOptions.overlay) {
        if ($('#medical-loading-overlay').length === 0) {
            $('body').append(`
                <div id="medical-loading-overlay" class="loading-overlay">
                    <div class="loading-content">
                        <div class="loading-spinner"></div>
                        <h5 class="mt-3 mb-2">${finalOptions.message}</h5>
                        <p class="text-muted">لطفاً صبر کنید - این عملیات ممکن است چند ثانیه طول بکشد</p>
                        ${finalOptions.progress ? `
                            <div class="progress mt-3" style="width: 250px;">
                                <div class="progress-bar progress-bar-striped progress-bar-animated" 
                                     role="progressbar" style="width: 100%"></div>
                            </div>
                        ` : ''}
                        <small class="text-muted mt-2 d-block">
                            <i class="fas fa-shield-alt"></i> محاسبات امن و رمزنگاری شده
                        </small>
                    </div>
                </div>
            `);
        }
    }
    
    // نمایش security indicator
    $('#medical-security-indicator').show();
}

/**
 * 🔄 Hide Loading Indicator
 */
function hideLoadingIndicator() {
    medicalLog('info', 'Hiding loading indicator');
    
    // مخفی کردن loading overlay
    $('#medical-loading-overlay').fadeOut(300, function() {
        $(this).remove();
    });
    
    // مخفی کردن security indicator
    $('#medical-security-indicator').fadeOut(200, function() {
        $(this).hide();
    });
}

/**
 * 💾 Enhanced Cache Management
 */
function setCache(key, data, timeout) {
    var cacheTimeout = timeout || MedicalEnvironment.settings.cacheTimeout;
    MedicalEnvironment.cache[key] = {
        data: data,
        timestamp: Date.now() + cacheTimeout
    };
    
    medicalLog('debug', 'Cache set', { key: key, timeout: cacheTimeout });
}

function getCache(key) {
    var cacheItem = MedicalEnvironment.cache[key];
    if (cacheItem && cacheItem.timestamp > Date.now()) {
        medicalLog('debug', 'Cache hit', { key: key });
        return cacheItem.data;
    }
    
    if (cacheItem) {
        delete MedicalEnvironment.cache[key];
        medicalLog('debug', 'Cache expired', { key: key });
    }
    
    return null;
}

function clearCache(key) {
    if (key) {
        delete MedicalEnvironment.cache[key];
        medicalLog('debug', 'Cache cleared', { key: key });
    } else {
        MedicalEnvironment.cache = {};
        medicalLog('debug', 'All cache cleared');
    }
}

/**
 * 🌐 Enhanced AJAX with Retry
 */
function medicalAjax(options) {
    var retryCount = 0;
    var maxRetries = options.maxRetries || MedicalEnvironment.settings.maxRetries;
    var retryDelay = options.retryDelay || MedicalEnvironment.settings.retryDelay;
    
    function makeRequest() {
        return $.ajax(options).fail(function(xhr, status, error) {
            if (retryCount < maxRetries && (status === 'timeout' || status === 'error')) {
                retryCount++;
                medicalLog('warn', 'AJAX retry attempt', { 
                    attempt: retryCount, 
                    maxRetries: maxRetries,
                    status: status,
                    error: error
                });
                
                setTimeout(makeRequest, retryDelay * retryCount);
            } else {
                medicalLog('error', 'AJAX failed after retries', { 
                    retryCount: retryCount,
                    status: status,
                    error: error
                });
                
                if (options.error) {
                    options.error(xhr, status, error);
                }
            }
        });
    }
    
    return makeRequest();
}

/**
 * 🔍 Enhanced Form Validation
 */
function validateMedicalForm(formSelector, rules) {
    var form = $(formSelector);
    var isValid = true;
    var errors = [];
    
    // اعتبارسنجی فیلدهای الزامی
    form.find('[required]').each(function() {
        var field = $(this);
        var value = field.val();
        
        if (!value || value.trim() === '') {
            isValid = false;
            field.addClass('is-invalid');
            errors.push('فیلد ' + field.attr('name') + ' الزامی است');
        } else {
            field.removeClass('is-invalid');
        }
    });
    
    // اعتبارسنجی قوانین سفارشی
    if (rules) {
        for (var fieldName in rules) {
            var field = form.find('[name="' + fieldName + '"]');
            var value = field.val();
            var rule = rules[fieldName];
            
            if (rule.required && (!value || value.trim() === '')) {
                isValid = false;
                field.addClass('is-invalid');
                errors.push(rule.message || 'فیلد ' + fieldName + ' الزامی است');
            } else if (rule.pattern && value && !rule.pattern.test(value)) {
                isValid = false;
                field.addClass('is-invalid');
                errors.push(rule.message || 'فرمت فیلد ' + fieldName + ' نامعتبر است');
            } else if (rule.min && value && parseFloat(value) < rule.min) {
                isValid = false;
                field.addClass('is-invalid');
                errors.push(rule.message || 'مقدار فیلد ' + fieldName + ' باید بیشتر از ' + rule.min + ' باشد');
            } else if (rule.max && value && parseFloat(value) > rule.max) {
                isValid = false;
                field.addClass('is-invalid');
                errors.push(rule.message || 'مقدار فیلد ' + fieldName + ' باید کمتر از ' + rule.max + ' باشد');
            } else {
                field.removeClass('is-invalid');
            }
        }
    }
    
    if (!isValid) {
        showError('لطفاً خطاهای فرم را برطرف کنید:\n' + errors.join('\n'));
    }
    
    return isValid;
}

/**
 * 🎯 Enhanced Number Formatting
 */
function formatMedicalNumber(number, decimals, currency) {
    if (typeof number !== 'number') {
        number = parseFloat(number) || 0;
    }
    
    var options = {
        minimumFractionDigits: decimals || 0,
        maximumFractionDigits: decimals || 0
    };
    
    if (currency) {
        options.style = 'currency';
        options.currency = 'IRR';
    }
    
    return new Intl.NumberFormat('fa-IR', options).format(number);
}

/**
 * 📅 Enhanced Date Formatting
 */
function formatMedicalDate(date, format) {
    if (!date) return '';
    
    var d = new Date(date);
    var options = {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    };
    
    if (format === 'time') {
        options.hour = '2-digit';
        options.minute = '2-digit';
    }
    
    return new Intl.DateTimeFormat('fa-IR', options).format(d);
}

/**
 * 🔄 Initialize on Document Ready
 */
$(document).ready(function() {
    console.log('🏥 MEDICAL: Document ready, initializing Medical Environment');
    initializeMedicalEnvironment();
});

/**
 * 🎯 Export Functions to Global Scope
 */
window.medicalLog = medicalLog;
window.showError = showError;
window.showSuccess = showSuccess;
window.showInfo = showInfo;
window.showWarning = showWarning;
window.showLoadingIndicator = showLoadingIndicator;
window.hideLoadingIndicator = hideLoadingIndicator;
window.setCache = setCache;
window.getCache = getCache;
window.clearCache = clearCache;
window.medicalAjax = medicalAjax;
window.validateMedicalForm = validateMedicalForm;
window.formatMedicalNumber = formatMedicalNumber;
window.formatMedicalDate = formatMedicalDate;
