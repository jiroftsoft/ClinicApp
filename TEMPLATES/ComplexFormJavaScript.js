/**
 * Complex Form JavaScript Template
 * استاندارد JavaScript برای فرم‌های پیچیده
 * 
 * @author AI Assistant
 * @version 1.0
 * @date 1404/06/23
 */

// Global Form Configuration
const FormConfig = {
    moduleName: '[Module]',
    ajaxTimeout: 30000,
    debounceDelay: 300,
    validationDelay: 500,
    cacheTimeout: 300000 // 5 minutes
};

// Global Cache for AJAX responses
const FormCache = new Map();

// Global Error Handler
window.addEventListener('error', function(e) {
    console.error('💥 Global Error:', {
        message: e.message,
        filename: e.filename,
        lineno: e.lineno,
        colno: e.colno,
        stack: e.error ? e.error.stack : 'No stack trace',
        timestamp: new Date().toISOString()
    });
});

// Global Unhandled Promise Rejection Handler
window.addEventListener('unhandledrejection', function(e) {
    console.error('💥 Unhandled Promise Rejection:', {
        reason: e.reason,
        timestamp: new Date().toISOString()
    });
});

/**
 * Main Form Initialization
 * مقداردهی اولیه فرم
 */
$(document).ready(function() {
    console.log('🏥 ' + FormConfig.moduleName + ' Form - Production Ready Version Loaded');
    console.log('📊 Form Configuration:', FormConfig);
    
    // Initialize all components
    initializeFormComponents();
    
    // Initialize event handlers
    initializeEventHandlers();
    
    // Initialize AJAX handlers
    initializeAjaxHandlers();
    
    // Initialize validation
    initializeValidation();
    
    // Initialize performance monitoring
    initializePerformanceMonitoring();
});

/**
 * Initialize Form Components
 * مقداردهی اولیه کامپوننت‌های فرم
 */
function initializeFormComponents() {
    console.log('🔧 Initializing form components...');
    
    // Initialize Persian DatePickers
    initializePersianDatePickers();
    
    // Initialize input groups
    initializeInputGroups();
    
    // Initialize tooltips
    initializeTooltips();
    
    // Initialize form cards
    initializeFormCards();
    
    console.log('✅ Form components initialized successfully');
}

/**
 * Initialize Persian DatePickers
 * مقداردهی اولیه انتخابگرهای تاریخ شمسی
 */
function initializePersianDatePickers() {
    console.log('📅 Initializing Persian DatePickers...');
    
    $('.persian-datepicker').persianDatepicker({
        format: 'YYYY/MM/DD',
        calendar: {
            persian: {
                locale: 'fa',
                showHint: true,
                leapYearMode: 'algorithmic'
            }
        },
        checkDate: function(unix) {
            return unix < Date.now();
        },
        autoClose: true,
        initialValue: false,
        position: 'auto',
        viewMode: 'day',
        inputDelay: FormConfig.validationDelay,
        navigator: {
            enabled: true,
            scroll: {
                enabled: true
            }
        },
        toolbox: {
            enabled: true,
            calendarSwitch: {
                enabled: false
            },
            todayButton: {
                enabled: true,
                text: 'امروز'
            },
            submitButton: {
                enabled: true,
                text: 'تأیید'
            },
            clearButton: {
                enabled: true,
                text: 'پاک کردن'
            }
        },
        onSelect: function(unix) {
            console.log('📅 Date selected:', {
                unix: unix,
                persian: $(this).val(),
                gregorian: new Date(unix).toISOString(),
                timestamp: new Date().toISOString()
            });
        }
    });
    
    console.log('✅ Persian DatePickers initialized');
}

/**
 * Initialize Input Groups
 * مقداردهی اولیه گروه‌های ورودی
 */
function initializeInputGroups() {
    console.log('🔧 Initializing input groups...');
    
    // Coverage percent input
    $('.coverage-percent-input').on('input change', debounce(function() {
        validateCoveragePercent($(this));
    }, FormConfig.debounceDelay));
    
    // Deductible input
    $('.deductible-input').on('input change', debounce(function() {
        validateDeductible($(this));
    }, FormConfig.debounceDelay));
    
    // Date inputs
    $('.persian-datepicker').on('change', debounce(function() {
        validatePersianDate($(this));
    }, FormConfig.debounceDelay));
    
    console.log('✅ Input groups initialized');
}

/**
 * Initialize Tooltips
 * مقداردهی اولیه راهنمای ابزار
 */
function initializeTooltips() {
    console.log('💡 Initializing tooltips...');
    
    $('[data-toggle="tooltip"]').tooltip({
        placement: 'top',
        trigger: 'hover focus'
    });
    
    console.log('✅ Tooltips initialized');
}

/**
 * Initialize Form Cards
 * مقداردهی اولیه کارت‌های فرم
 */
function initializeFormCards() {
    console.log('🎴 Initializing form cards...');
    
    $('.form-card').each(function() {
        var $card = $(this);
        var $header = $card.find('.form-card-header');
        var $body = $card.find('.form-card-body');
        
        // Add click to expand/collapse functionality
        $header.on('click', function() {
            $body.slideToggle();
            $card.toggleClass('collapsed');
        });
        
        // Add hover effects
        $card.hover(
            function() {
                $(this).addClass('hovered');
            },
            function() {
                $(this).removeClass('hovered');
            }
        );
    });
    
    console.log('✅ Form cards initialized');
}

/**
 * Initialize Event Handlers
 * مقداردهی اولیه مدیریتگران رویداد
 */
function initializeEventHandlers() {
    console.log('🎯 Initializing event handlers...');
    
    // Form field focus effects
    $('.form-control').on('focus', function() {
        $(this).closest('.form-group').addClass('focused');
        console.log('🎯 Field focused:', $(this).attr('name') || $(this).attr('id'));
    }).on('blur', function() {
        $(this).closest('.form-group').removeClass('focused');
        console.log('🎯 Field blurred:', $(this).attr('name') || $(this).attr('id'));
    });
    
    // Form submission
    $('form').on('submit', function(e) {
        handleFormSubmission(e);
    });
    
    // Reset button
    $('.btn-reset').on('click', function(e) {
        e.preventDefault();
        resetForm();
    });
    
    // Keyboard shortcuts
    $(document).on('keydown', function(e) {
        handleKeyboardShortcuts(e);
    });
    
    console.log('✅ Event handlers initialized');
}

/**
 * Initialize AJAX Handlers
 * مقداردهی اولیه مدیریتگران AJAX
 */
function initializeAjaxHandlers() {
    console.log('🔄 Initializing AJAX handlers...');
    
    // Global AJAX error handler
    $(document).ajaxError(function(event, xhr, settings, error) {
        console.error('❌ AJAX Error:', {
            url: settings.url,
            method: settings.type,
            status: xhr.status,
            error: error,
            response: xhr.responseText,
            timestamp: new Date().toISOString()
        });
        handleAjaxError(xhr, settings, error);
    });
    
    // Global AJAX success handler
    $(document).ajaxSuccess(function(event, xhr, settings) {
        console.log('✅ AJAX Success:', {
            url: settings.url,
            method: settings.type,
            status: xhr.status,
            responseTime: new Date().toISOString()
        });
    });
    
    // Global AJAX start handler
    $(document).ajaxStart(function() {
        console.log('🔄 AJAX Request Started');
        showLoadingIndicator();
    });
    
    // Global AJAX complete handler
    $(document).ajaxComplete(function() {
        console.log('✅ AJAX Request Completed');
        hideLoadingIndicator();
    });
    
    console.log('✅ AJAX handlers initialized');
}

/**
 * Initialize Validation
 * مقداردهی اولیه اعتبارسنجی
 */
function initializeValidation() {
    console.log('✅ Initializing validation...');
    
    // Real-time validation
    $('.form-control').on('input change', debounce(function() {
        validateField($(this));
    }, FormConfig.debounceDelay));
    
    // Form validation on submit
    $('form').on('submit', function(e) {
        if (!validateForm()) {
            e.preventDefault();
            return false;
        }
    });
    
    console.log('✅ Validation initialized');
}

/**
 * Initialize Performance Monitoring
 * مقداردهی اولیه مانیتورینگ عملکرد
 */
function initializePerformanceMonitoring() {
    console.log('📊 Initializing performance monitoring...');
    
    // Monitor form load time
    var loadTime = performance.now();
    console.log('⏱️ Form load time:', loadTime + 'ms');
    
    // Monitor memory usage
    if (performance.memory) {
        console.log('💾 Memory usage:', {
            used: Math.round(performance.memory.usedJSHeapSize / 1048576) + 'MB',
            total: Math.round(performance.memory.totalJSHeapSize / 1048576) + 'MB',
            limit: Math.round(performance.memory.jsHeapSizeLimit / 1048576) + 'MB'
        });
    }
    
    console.log('✅ Performance monitoring initialized');
}

/**
 * Validation Functions
 * توابع اعتبارسنجی
 */

/**
 * Validate Coverage Percent
 * اعتبارسنجی درصد پوشش
 */
function validateCoveragePercent($input) {
    var value = parseFloat($input.val());
    var $formGroup = $input.closest('.form-group');
    
    console.log('📊 Coverage Percent Changed:', value);
    
    $input.removeClass('is-valid is-invalid');
    $formGroup.find('.validation-feedback').remove();
    
    if (isNaN(value) || value < 0 || value > 100) {
        $input.addClass('is-invalid');
        $formGroup.append('<div class="validation-feedback text-danger"><i class="fas fa-exclamation-triangle"></i> درصد پوشش باید بین 0 تا 100 باشد</div>');
        console.warn('⚠️ Invalid coverage percent:', value);
        return false;
    } else {
        $input.addClass('is-valid');
        $formGroup.append('<div class="validation-feedback text-success"><i class="fas fa-check-circle"></i> درصد پوشش معتبر است</div>');
        console.log('✅ Valid coverage percent:', value);
        
        // Calculate patient share
        var patientShare = 100 - value;
        console.log('💰 Patient Share:', patientShare + '%');
        return true;
    }
}

/**
 * Validate Deductible
 * اعتبارسنجی فرانشیز
 */
function validateDeductible($input) {
    var value = parseFloat($input.val());
    var $formGroup = $input.closest('.form-group');
    
    console.log('💰 Deductible Changed:', value);
    
    $input.removeClass('is-valid is-invalid');
    $formGroup.find('.validation-feedback').remove();
    
    if (isNaN(value) || value < 0) {
        $input.addClass('is-invalid');
        $formGroup.append('<div class="validation-feedback text-danger"><i class="fas fa-exclamation-triangle"></i> مبلغ فرانشیز باید بزرگتر یا مساوی صفر باشد</div>');
        console.warn('⚠️ Invalid deductible:', value);
        return false;
    } else {
        $input.addClass('is-valid');
        $formGroup.append('<div class="validation-feedback text-success"><i class="fas fa-check-circle"></i> مبلغ فرانشیز معتبر است</div>');
        console.log('✅ Valid deductible:', value);
        
        // Format number with thousand separators
        if (value > 0) {
            var formattedValue = value.toLocaleString('fa-IR');
            console.log('💵 Formatted Deductible:', formattedValue + ' تومان');
        }
        return true;
    }
}

/**
 * Validate Persian Date
 * اعتبارسنجی تاریخ شمسی
 */
function validatePersianDate($input) {
    var inputId = $input.attr('id');
    var value = $input.val();
    var $formGroup = $input.closest('.form-group');
    
    console.log('📅 Date Field Changed:', {
        fieldId: inputId,
        value: value,
        timestamp: new Date().toISOString()
    });
    
    $input.removeClass('is-valid is-invalid');
    $formGroup.find('.validation-feedback').remove();
    
    if (!value || value.trim() === '') {
        if (inputId === 'ValidFromShamsi') {
            $input.addClass('is-invalid');
            $formGroup.append('<div class="validation-feedback text-danger"><i class="fas fa-exclamation-triangle"></i> تاریخ شروع اعتبار الزامی است</div>');
            console.warn('⚠️ ValidFrom is required');
            return false;
        }
        return true;
    }
    
    if (!isValidPersianDate(value)) {
        $input.addClass('is-invalid');
        $formGroup.append('<div class="validation-feedback text-danger"><i class="fas fa-exclamation-triangle"></i> فرمت تاریخ نامعتبر است (مثال: 1404/06/23)</div>');
        console.warn('⚠️ Invalid Persian date format:', value);
        return false;
    }
    
    // Check date range validation
    var validFrom = $('#ValidFromShamsi').val();
    var validTo = $('#ValidToShamsi').val();
    
    if (validFrom && validTo) {
        var fromDate = convertPersianToGregorian(validFrom);
        var toDate = convertPersianToGregorian(validTo);
        
        console.log('📊 Date Range Validation:', {
            validFrom: validFrom,
            validTo: validTo,
            fromDate: fromDate,
            toDate: toDate
        });
        
        if (fromDate >= toDate) {
            $('#ValidToShamsi').addClass('is-invalid');
            $('#ValidToShamsi').removeClass('is-valid');
            $('#ValidToShamsi').closest('.form-group').find('.validation-feedback').remove();
            $('#ValidToShamsi').closest('.form-group').append('<div class="validation-feedback text-danger"><i class="fas fa-exclamation-triangle"></i> تاریخ پایان اعتبار نمی‌تواند قبل از تاریخ شروع اعتبار باشد</div>');
            console.warn('⚠️ Invalid date range: End date is before start date');
            return false;
        } else {
            $('#ValidToShamsi').removeClass('is-invalid');
            $('#ValidToShamsi').addClass('is-valid');
            $('#ValidToShamsi').closest('.form-group').find('.validation-feedback').remove();
            $('#ValidToShamsi').closest('.form-group').append('<div class="validation-feedback text-success"><i class="fas fa-check-circle"></i> بازه زمانی معتبر است</div>');
            console.log('✅ Valid date range');
            return true;
        }
    } else {
        $input.addClass('is-valid');
        $formGroup.append('<div class="validation-feedback text-success"><i class="fas fa-check-circle"></i> تاریخ معتبر است</div>');
        console.log('✅ Valid date:', value);
        return true;
    }
}

/**
 * Validate Field
 * اعتبارسنجی فیلد
 */
function validateField($field) {
    var fieldName = $field.attr('name') || $field.attr('id');
    var value = $field.val();
    var isRequired = $field.prop('required');
    
    console.log('🔍 Validating field:', {
        field: fieldName,
        value: value,
        isRequired: isRequired,
        timestamp: new Date().toISOString()
    });
    
    $field.removeClass('is-valid is-invalid');
    
    if (isRequired && (!value || value.trim() === '')) {
        $field.addClass('is-invalid');
        return false;
    }
    
    if (value && value.trim() !== '') {
        $field.addClass('is-valid');
    }
    
    return true;
}

/**
 * Validate Form
 * اعتبارسنجی فرم
 */
function validateForm() {
    console.log('🔍 Validating entire form...');
    
    var isValid = true;
    var errors = [];
    
    // Validate required fields
    $('.form-control[required]').each(function() {
        if (!validateField($(this))) {
            isValid = false;
            var fieldName = $(this).attr('name') || $(this).attr('id');
            errors.push(fieldName + ' الزامی است');
        }
    });
    
    // Validate date range
    var validFrom = $('#ValidFromShamsi').val();
    var validTo = $('#ValidToShamsi').val();
    
    if (validFrom && validTo) {
        var fromDate = convertPersianToGregorian(validFrom);
        var toDate = convertPersianToGregorian(validTo);
        
        if (fromDate >= toDate) {
            isValid = false;
            errors.push('تاریخ پایان اعتبار نمی‌تواند قبل از تاریخ شروع اعتبار باشد');
        }
    }
    
    if (!isValid) {
        console.error('❌ Form validation failed:', errors);
        showError('لطفاً خطاهای زیر را برطرف کنید:\n' + errors.join('\n'));
    } else {
        console.log('✅ Form validation passed');
    }
    
    return isValid;
}

/**
 * Persian Date Helper Functions
 * توابع کمکی تاریخ شمسی
 */

/**
 * Check if Persian Date is Valid
 * بررسی معتبر بودن تاریخ شمسی
 */
function isValidPersianDate(persianDate) {
    if (!persianDate || persianDate.trim() === '') {
        return false;
    }
    
    var englishDate = persianDate.replace(/[۰-۹]/g, function(d) {
        return String.fromCharCode(d.charCodeAt(0) - '۰'.charCodeAt(0) + '0'.charCodeAt(0));
    });
    
    var persianDatePattern = /^[12][0-9]{3}\/(0[1-9]|1[0-2])\/(0[1-9]|[12][0-9]|3[01])$/;
    return persianDatePattern.test(englishDate);
}

/**
 * Convert Persian Date to Gregorian
 * تبدیل تاریخ شمسی به میلادی
 */
function convertPersianToGregorian(persianDate) {
    if (!persianDate) return null;
    
    try {
        console.log('🔄 Converting Persian date to Gregorian:', persianDate);
        
        var englishDate = persianDate.replace(/[۰-۹]/g, function(d) {
            return String.fromCharCode(d.charCodeAt(0) - '۰'.charCodeAt(0) + '0'.charCodeAt(0));
        });
        
        var parts = englishDate.split('/');
        if (parts.length === 3) {
            var year = parseInt(parts[0]);
            var month = parseInt(parts[1]);
            var day = parseInt(parts[2]);
            
            var gregorianYear = year + 621;
            var result = new Date(gregorianYear, month - 1, day);
            console.log('✅ Converted date:', result);
            return result;
        }
    } catch (e) {
        console.error('❌ Error converting Persian date:', e);
    }
    
    return null;
}

/**
 * Form Handling Functions
 * توابع مدیریت فرم
 */

/**
 * Handle Form Submission
 * مدیریت ارسال فرم
 */
function handleFormSubmission(e) {
    console.log('📤 Form submission started');
    
    var formData = collectFormData();
    console.log('📊 Form data collected:', formData);
    
    if (!validateForm()) {
        e.preventDefault();
        console.error('❌ Form validation failed, preventing submission');
        return false;
    }
    
    console.log('✅ Form validation passed, submitting...');
    
    // Show loading state
    showFormLoadingState();
    
    return true;
}

/**
 * Collect Form Data
 * جمع‌آوری داده‌های فرم
 */
function collectFormData() {
    var formData = {};
    
    $('.form-control').each(function() {
        var $field = $(this);
        var fieldName = $field.attr('name') || $field.attr('id');
        var fieldValue = $field.val();
        
        if (fieldName) {
            formData[fieldName] = fieldValue;
        }
    });
    
    return formData;
}

/**
 * Reset Form
 * بازنشانی فرم
 */
function resetForm() {
    console.log('🔄 Resetting form...');
    
    if (confirm('آیا مطمئن هستید که می‌خواهید فرم را بازنشانی کنید؟')) {
        $('form')[0].reset();
        $('.form-control').removeClass('is-valid is-invalid');
        $('.validation-feedback').remove();
        console.log('✅ Form reset successfully');
    }
}

/**
 * Show Form Loading State
 * نمایش حالت بارگذاری فرم
 */
function showFormLoadingState() {
    $('.btn-submit').addClass('btn-loading').prop('disabled', true);
    $('.form-actions .btn').prop('disabled', true);
}

/**
 * Hide Form Loading State
 * مخفی کردن حالت بارگذاری فرم
 */
function hideFormLoadingState() {
    $('.btn-submit').removeClass('btn-loading').prop('disabled', false);
    $('.form-actions .btn').prop('disabled', false);
}

/**
 * Keyboard Shortcuts
 * میانبرهای صفحه‌کلید
 */
function handleKeyboardShortcuts(e) {
    // Ctrl + R: Reset form
    if (e.ctrlKey && e.key === 'r') {
        e.preventDefault();
        resetForm();
    }
    
    // Ctrl + S: Submit form
    if (e.ctrlKey && e.key === 's') {
        e.preventDefault();
        $('form').submit();
    }
    
    // Escape: Clear validation messages
    if (e.key === 'Escape') {
        $('.validation-feedback').fadeOut();
    }
}

/**
 * AJAX Functions
 * توابع AJAX
 */

/**
 * Perform AJAX Request with Caching
 * انجام درخواست AJAX با کش
 */
function performAjaxRequest(url, data, options = {}) {
    var cacheKey = url + JSON.stringify(data);
    var cachedData = FormCache.get(cacheKey);
    
    if (cachedData && (Date.now() - cachedData.timestamp) < FormConfig.cacheTimeout) {
        console.log('📦 Using cached data for:', url);
        return Promise.resolve(cachedData.data);
    }
    
    console.log('🔄 Fetching fresh data for:', url);
    
    var defaultOptions = {
        url: url,
        type: 'GET',
        dataType: 'json',
        timeout: FormConfig.ajaxTimeout,
        data: data
    };
    
    var requestOptions = Object.assign({}, defaultOptions, options);
    
    return $.ajax(requestOptions)
        .done(function(response) {
            console.log('✅ AJAX Success:', {
                url: url,
                data: response,
                timestamp: new Date().toISOString()
            });
            
            // Cache the response
            FormCache.set(cacheKey, {
                data: response,
                timestamp: Date.now()
            });
        })
        .fail(function(xhr, status, error) {
            console.error('❌ AJAX Failed:', {
                url: url,
                status: status,
                error: error,
                response: xhr.responseText,
                timestamp: new Date().toISOString()
            });
            throw error;
        });
}

/**
 * Handle AJAX Error
 * مدیریت خطای AJAX
 */
function handleAjaxError(xhr, settings, error) {
    var errorMessage = 'خطا در ارتباط با سرور';
    
    try {
        var response = JSON.parse(xhr.responseText);
        if (response.message) {
            errorMessage = response.message;
        }
    } catch (e) {
        // Use default error message
    }
    
    showError(errorMessage);
}

/**
 * Utility Functions
 * توابع کمکی
 */

/**
 * Debounce Function
 * تابع کاهش فراخوانی
 */
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * Show Loading Indicator
 * نمایش نشانگر بارگذاری
 */
function showLoadingIndicator() {
    if (!$('.loading-indicator').length) {
        $('body').append('<div class="loading-indicator"><i class="fas fa-spinner fa-spin"></i> در حال بارگذاری...</div>');
    }
}

/**
 * Hide Loading Indicator
 * مخفی کردن نشانگر بارگذاری
 */
function hideLoadingIndicator() {
    $('.loading-indicator').remove();
}

/**
 * Show Error Message
 * نمایش پیام خطا
 */
function showError(message) {
    console.error('💥 Error:', message);
    
    // You can implement your preferred error display method here
    // For example: toastr.error(message);
    alert('خطا: ' + message);
}

/**
 * Show Success Message
 * نمایش پیام موفقیت
 */
function showSuccess(message) {
    console.log('✅ Success:', message);
    
    // You can implement your preferred success display method here
    // For example: toastr.success(message);
    alert('موفقیت: ' + message);
}

/**
 * Show Warning Message
 * نمایش پیام هشدار
 */
function showWarning(message) {
    console.warn('⚠️ Warning:', message);
    
    // You can implement your preferred warning display method here
    // For example: toastr.warning(message);
    alert('هشدار: ' + message);
}

/**
 * Format Number with Thousand Separators
 * فرمت کردن عدد با جداکننده هزارگان
 */
function formatNumber(number) {
    return number.toLocaleString('fa-IR');
}

/**
 * Clear Form Cache
 * پاک کردن کش فرم
 */
function clearFormCache() {
    FormCache.clear();
    console.log('🗑️ Form cache cleared');
}

/**
 * Export Functions for External Use
 * صادر کردن توابع برای استفاده خارجی
 */
window.FormUtils = {
    validateForm: validateForm,
    resetForm: resetForm,
    showError: showError,
    showSuccess: showSuccess,
    showWarning: showWarning,
    formatNumber: formatNumber,
    clearFormCache: clearFormCache,
    performAjaxRequest: performAjaxRequest
};
