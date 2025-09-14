/**
 * Patient Insurance Form - Central JavaScript File
 * Handles form validation, date conversion, and UI interactions
 */

// محافظت jQuery - همان pattern موجود در layout
(function () {
    'use strict';

    function ensureJQuery(callback) {
        if (typeof jQuery !== 'undefined' && typeof $.fn !== 'undefined') {
            callback();
        } else {
            setTimeout(function () {
                ensureJQuery(callback);
            }, 50);
        }
    }

    function initializePatientInsuranceForm() {
        ensureJQuery(function () {
            $(document).ready(function() {
                // Initialize Persian DatePicker
                initializePersianDatePicker();
                
                // Initialize form validation
                initializeFormValidation();
                
                // Initialize event handlers
                initializeEventHandlers();
            });
        });
    }

    // شروع initialization
    initializePatientInsuranceForm();
})();

/**
 * Initialize Persian DatePicker for all date fields
 */
function initializePersianDatePicker() {
    $('.persian-datepicker').persianDatepicker({
        format: 'YYYY/MM/DD',
        altField: '.observer-example-alt',
        altFormat: 'YYYY/MM/DD',
        observer: true,
        timePicker: {
            enabled: false
        }
    });
}

/**
 * Initialize form validation
 */
function initializeFormValidation() {
    'use strict';
    window.addEventListener('load', function() {
        var forms = document.getElementsByClassName('needs-validation');
        var validation = Array.prototype.filter.call(forms, function(form) {
            form.addEventListener('submit', function(event) {
                if (form.checkValidity() === false) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                form.classList.add('was-validated');
            }, false);
        });
    }, false);
}

/**
 * Initialize event handlers
 */
function initializeEventHandlers() {
    // Date conversion and validation
    $('.persian-datepicker').on('change', handleDateChange);
    
    // Patient selection change
    $('#PatientId').on('change', handlePatientSelection);
    
    // Insurance plan selection change
    $('#InsurancePlanId').on('change', handleInsurancePlanSelection);
    
    // Primary insurance toggle
    $('#IsPrimary').on('change', handlePrimaryInsuranceToggle);
}

/**
 * Convert Gregorian date to Persian date using persianDate library
 * طبق مستندات PERSIAN_DATEPICKER_CONTRACT.md
 */
function convertGregorianToPersian(gregorianDate) {
    try {
        console.log('🗓️ Converting Gregorian date to Persian:', gregorianDate);
        
        // بررسی وجود persianDate library
        if (typeof persianDate !== 'undefined') {
            try {
                var date = new Date(gregorianDate);
                if (isNaN(date.getTime())) {
                    console.error('❌ Invalid Gregorian date:', gregorianDate);
                    return convertGregorianToPersianFallback(gregorianDate);
                }
                
                // استفاده از persianDate library
                var persianDateObj = persianDate(date);
                var persianDateStr = persianDateObj.format('YYYY/MM/DD');
                
                console.log('✅ Persian date created using library:', persianDateStr);
                return persianDateStr;
            } catch (libraryError) {
                console.warn('⚠️ persianDate library error, using fallback:', libraryError);
                return convertGregorianToPersianFallback(gregorianDate);
            }
        } else {
            console.warn('⚠️ persianDate library not loaded, using fallback');
            return convertGregorianToPersianFallback(gregorianDate);
        }
        
    } catch (error) {
        console.error('❌ Error in convertGregorianToPersian:', error);
        return convertGregorianToPersianFallback(gregorianDate);
    }
}

/**
 * Fallback function for Gregorian to Persian conversion
 */
function convertGregorianToPersianFallback(gregorianDate) {
    try {
        console.log('🔄 Using fallback conversion for:', gregorianDate);
        
        var date = new Date(gregorianDate);
        if (isNaN(date.getTime())) {
            console.error('❌ Invalid date in fallback:', gregorianDate);
            return null;
        }
        
        // تبدیل میلادی به شمسی (تقریبی)
        var year = date.getFullYear();
        var month = date.getMonth() + 1;
        var day = date.getDate();
        
        console.log('📅 Gregorian date parts:', { year: year, month: month, day: day });
        
        // محاسبه سال شمسی (تقریبی)
        var persianYear = year - 621;
        
        // محاسبه ماه شمسی (تقریبی)
        var persianMonth = month;
        var persianDay = day;
        
        // تنظیم ماه و روز برای تقویم شمسی (تقریبی)
        if (month > 10) {
            persianYear += 1;
            persianMonth = month - 10;
        } else {
            persianMonth = month + 2;
        }
        
        // فرمت تاریخ شمسی
        var persianDateStr = persianYear + '/' + 
                           (persianMonth < 10 ? '0' + persianMonth : persianMonth) + '/' + 
                           (persianDay < 10 ? '0' + persianDay : persianDay);
        
        console.log('✅ Fallback conversion result:', persianDateStr);
        return persianDateStr;
        
    } catch (error) {
        console.error('❌ Error in fallback conversion:', error);
        return null;
    }
}

/**
 * Convert Persian date to Gregorian date using persianDate library
 * طبق مستندات PERSIAN_DATEPICKER_CONTRACT.md
 */
function convertPersianToGregorian(persianDate) {
    try {
        console.log('🗓️ Converting Persian date to Gregorian:', persianDate);
        
        // بررسی وجود persianDate library
        if (typeof persianDate === 'undefined') {
            console.warn('⚠️ persianDate library not loaded, using fallback');
            return convertPersianToGregorianFallback(persianDate);
        }
        
        // تبدیل اعداد فارسی به انگلیسی
        var englishDate = persianDate
            .replace(/۰/g, '0')
            .replace(/۱/g, '1')
            .replace(/۲/g, '2')
            .replace(/۳/g, '3')
            .replace(/۴/g, '4')
            .replace(/۵/g, '5')
            .replace(/۶/g, '6')
            .replace(/۷/g, '7')
            .replace(/۸/g, '8')
            .replace(/۹/g, '9');
        
        // استفاده از persianDate library
        var persianDateObj = new persianDate(englishDate.split('/'));
        var gregorianDate = persianDateObj.toDate();
        
        // بررسی معتبر بودن تاریخ
        if (isNaN(gregorianDate.getTime())) {
            console.error('❌ Invalid Gregorian date created');
            return null;
        }
        
        console.log('✅ Gregorian date created:', gregorianDate);
        return gregorianDate;
        
    } catch (error) {
        console.error('❌ Error in convertPersianToGregorian:', error);
        return convertPersianToGregorianFallback(persianDate);
    }
}

/**
 * Fallback function for Persian to Gregorian conversion
 */
function convertPersianToGregorianFallback(persianDate) {
    try {
        // تبدیل اعداد فارسی به انگلیسی
        var englishDate = persianDate
            .replace(/۰/g, '0')
            .replace(/۱/g, '1')
            .replace(/۲/g, '2')
            .replace(/۳/g, '3')
            .replace(/۴/g, '4')
            .replace(/۵/g, '5')
            .replace(/۶/g, '6')
            .replace(/۷/g, '7')
            .replace(/۸/g, '8')
            .replace(/۹/g, '9');
        
        // تجزیه تاریخ
        var parts = englishDate.split('/');
        if (parts.length !== 3) {
            console.error('❌ Invalid date format:', persianDate);
            return null;
        }
        
        var year = parseInt(parts[0]);
        var month = parseInt(parts[1]);
        var day = parseInt(parts[2]);
        
        // اعتبارسنجی
        if (isNaN(year) || isNaN(month) || isNaN(day)) {
            console.error('❌ Invalid date numbers');
            return null;
        }
        
        if (year < 1300 || year > 1500) {
            console.error('❌ Invalid year range:', year);
            return null;
        }
        
        if (month < 1 || month > 12) {
            console.error('❌ Invalid month range:', month);
            return null;
        }
        
        if (day < 1 || day > 31) {
            console.error('❌ Invalid day range:', day);
            return null;
        }
        
        // تبدیل شمسی به میلادی (تقریبی)
        var gregorianYear = year + 621;
        var gregorianMonth = month;
        var gregorianDay = day;
        
        // تنظیم ماه و روز برای تقویم میلادی
        if (month > 10) {
            gregorianYear += 1;
            gregorianMonth = month - 10;
        } else {
            gregorianMonth = month + 2;
        }
        
        // ایجاد تاریخ میلادی
        var gregorianDate = new Date(gregorianYear, gregorianMonth - 1, gregorianDay);
        
        // بررسی معتبر بودن تاریخ
        if (isNaN(gregorianDate.getTime())) {
            console.error('❌ Invalid Gregorian date created');
            return null;
        }
        
        return gregorianDate;
        
    } catch (error) {
        console.error('❌ Error in fallback conversion:', error);
        return null;
    }
}

/**
 * Handle date field changes - convert Persian to Gregorian and validate
 */
function handleDateChange() {
    var startDateShamsi = $('#StartDateShamsi').val();
    var endDateShamsi = $('#EndDateShamsi').val();
    
    // Convert Persian dates to Gregorian
    if (startDateShamsi) {
        try {
            var startDateGregorian = convertPersianToGregorian(startDateShamsi);
            if (startDateGregorian) {
                $('#StartDate').val(startDateGregorian.toISOString().split('T')[0]);
                console.log('✅ Start date converted to Gregorian:', startDateGregorian.toISOString().split('T')[0]);
            } else {
                showDateError('StartDateShamsi', 'تاریخ شروع نامعتبر است');
            }
        } catch (e) {
            console.error('Error converting start date:', e);
            showDateError('StartDateShamsi', 'تاریخ شروع نامعتبر است');
        }
    }
    
    if (endDateShamsi) {
        try {
            var endDateGregorian = convertPersianToGregorian(endDateShamsi);
            if (endDateGregorian) {
                $('#EndDate').val(endDateGregorian.toISOString().split('T')[0]);
                console.log('✅ End date converted to Gregorian:', endDateGregorian.toISOString().split('T')[0]);
            } else {
                showDateError('EndDateShamsi', 'تاریخ پایان نامعتبر است');
            }
        } catch (e) {
            console.error('Error converting end date:', e);
            showDateError('EndDateShamsi', 'تاریخ پایان نامعتبر است');
        }
    }
    
    // Date validation
    if (startDateShamsi && endDateShamsi) {
        validateDateRange(startDateShamsi, endDateShamsi);
    }
}

/**
 * Validate date range
 */
function validateDateRange(startDateShamsi, endDateShamsi) {
    try {
        var fromDate = convertPersianToGregorian(startDateShamsi);
        var toDate = convertPersianToGregorian(endDateShamsi);
        
        if (fromDate && toDate) {
            if (fromDate >= toDate) {
                var startFieldName = getFriendlyFieldName('StartDateShamsi');
                var endFieldName = getFriendlyFieldName('EndDateShamsi');
                showDateError('EndDateShamsi', endFieldName + ' باید بعد از ' + startFieldName + ' باشد');
            } else {
                clearDateError('EndDateShamsi');
            }
        } else {
            console.warn('⚠️ تاریخ‌ها قابل تبدیل نیستند برای اعتبارسنجی');
        }
    } catch (e) {
        console.error('Error validating dates:', e);
        showDateError('EndDateShamsi', 'خطا در اعتبارسنجی تاریخ‌ها');
    }
}

/**
 * Get friendly field name from label or DisplayName
 * دریافت نام دوستانه فیلد از label یا DisplayName
 */
function getFriendlyFieldName(inputName) {
    var label = $("label[for='" + inputName + "']");
    if (label.length) {
        var labelText = label.text().trim();
        // حذف علامت * از انتهای label
        return labelText.replace(/\*$/, '');
    }
    return inputName; // fallback به نام انگلیسی
}

/**
 * Show date error
 */
function showDateError(fieldId, message) {
    $('#' + fieldId).addClass('is-invalid');
    $('#' + fieldId).next('.invalid-feedback').remove();
    
    // استفاده از نام دوستانه فیلد در پیام خطا
    var friendlyFieldName = getFriendlyFieldName(fieldId);
    var friendlyMessage = message.replace('تاریخ', friendlyFieldName);
    
    $('#' + fieldId).after('<div class="invalid-feedback">' + friendlyMessage + '</div>');
}

/**
 * Clear date error
 */
function clearDateError(fieldId) {
    $('#' + fieldId).removeClass('is-invalid');
    $('#' + fieldId).next('.invalid-feedback').remove();
}

/**
 * Handle patient selection change
 */
function handlePatientSelection() {
    var patientId = $(this).val();
    if (patientId) {
        // Load patient's existing insurances and show warnings
        loadPatientInsurances(patientId);
    } else {
        hidePatientInsurances();
    }
}

/**
 * Load patient's existing insurances
 */
function loadPatientInsurances(patientId) {
    // You can implement AJAX call to get patient's existing insurances
    // For now, we'll show a placeholder
    var infoHtml = '<div class="alert alert-info mt-2" id="patientInsurances">' +
        '<i class="fas fa-info-circle"></i> ' +
        '<strong>توجه:</strong> در صورت وجود بیمه‌های قبلی، از تداخل تاریخ‌ها اطمینان حاصل کنید.' +
        '</div>';
    $('#PatientId').closest('.form-group').find('#patientInsurances').remove();
    $('#PatientId').closest('.form-group').append(infoHtml);
}

/**
 * Hide patient insurances info
 */
function hidePatientInsurances() {
    $('#patientInsurances').remove();
}

/**
 * Handle insurance plan selection change
 */
function handleInsurancePlanSelection() {
    var planId = $(this).val();
    if (planId) {
        showPlanDetails(planId);
    } else {
        hidePlanDetails();
    }
}

/**
 * Show plan details
 */
function showPlanDetails(planId) {
    var planInfo = $('#InsurancePlanId option:selected').text();
    if (planInfo && planInfo !== 'انتخاب طرح بیمه') {
        var infoHtml = '<div class="alert alert-info mt-2" id="planInfo">' +
            '<i class="fas fa-info-circle"></i> ' +
            '<strong>طرح انتخاب شده:</strong> ' + planInfo +
            '</div>';
        $('#InsurancePlanId').closest('.form-group').find('#planInfo').remove();
        $('#InsurancePlanId').closest('.form-group').append(infoHtml);
    }
}

/**
 * Hide plan details
 */
function hidePlanDetails() {
    $('#planInfo').remove();
}

/**
 * Handle primary insurance toggle
 */
function handlePrimaryInsuranceToggle() {
    var isPrimary = $(this).is(':checked');
    if (isPrimary) {
        showPrimaryInsuranceWarning();
    } else {
        hidePrimaryInsuranceWarning();
    }
}

/**
 * Show primary insurance warning
 */
function showPrimaryInsuranceWarning() {
    var warningHtml = '<div class="alert alert-warning mt-2" id="primaryWarning">' +
        '<i class="fas fa-exclamation-triangle"></i> ' +
        '<strong>توجه:</strong> هر بیمار فقط می‌تواند یک بیمه اصلی داشته باشد. در صورت وجود بیمه اصلی قبلی، این عملیات ممکن است با خطا مواجه شود.' +
        '</div>';
    $('#IsPrimary').closest('.form-group').find('#primaryWarning').remove();
    $('#IsPrimary').closest('.form-group').append(warningHtml);
}

/**
 * Hide primary insurance warning
 */
function hidePrimaryInsuranceWarning() {
    $('#primaryWarning').remove();
}

/**
 * Initialize form with existing data (for Edit mode)
 */
function initializeFormWithData() {
    console.log('🔄 Initializing form with existing data...');
    
    // Convert existing Gregorian dates to Persian for display
    var startDate = $('#StartDate').val();
    var endDate = $('#EndDate').val();
    
    console.log('📅 StartDate hidden field value:', startDate);
    console.log('📅 EndDate hidden field value:', endDate);
    
    // Debug: Check if StartDateShamsi already has a value
    var existingStartDateShamsi = $('#StartDateShamsi').val();
    var existingEndDateShamsi = $('#EndDateShamsi').val();
    
    console.log('📅 Existing StartDateShamsi value:', existingStartDateShamsi);
    console.log('📅 Existing EndDateShamsi value:', existingEndDateShamsi);
    
    if (startDate) {
        try {
            console.log('🔄 Converting start date to Persian...');
            var startDateShamsi = convertGregorianToPersian(startDate);
            if (startDateShamsi) {
                // فقط در صورت خالی بودن فیلد، مقدار را تنظیم کنیم
                if (!$('#StartDateShamsi').val()) {
                    $('#StartDateShamsi').val(startDateShamsi);
                    console.log('✅ Start date converted to Persian:', startDateShamsi);
                } else {
                    console.log('ℹ️ StartDateShamsi already has value, keeping existing:', $('#StartDateShamsi').val());
                }
            } else {
                console.warn('⚠️ Start date conversion returned null');
            }
        } catch (e) {
            console.error('❌ Error converting start date to Persian:', e);
        }
    } else {
        console.warn('⚠️ No start date found in hidden field');
    }
    
    if (endDate) {
        try {
            console.log('🔄 Converting end date to Persian...');
            var endDateShamsi = convertGregorianToPersian(endDate);
            if (endDateShamsi) {
                // فقط در صورت خالی بودن فیلد، مقدار را تنظیم کنیم
                if (!$('#EndDateShamsi').val()) {
                    $('#EndDateShamsi').val(endDateShamsi);
                    console.log('✅ End date converted to Persian:', endDateShamsi);
                } else {
                    console.log('ℹ️ EndDateShamsi already has value, keeping existing:', $('#EndDateShamsi').val());
                }
            } else {
                console.warn('⚠️ End date conversion returned null');
            }
        } catch (e) {
            console.error('❌ Error converting end date to Persian:', e);
        }
    } else {
        console.warn('⚠️ No end date found in hidden field');
    }
    
    // Initialize other form elements
    var patientId = $('#PatientId').val();
    if (patientId) {
        loadPatientInsurances(patientId);
        
        // حفظ انتخاب بیمار در Select2
        console.log('🔄 Setting patient selection in Select2:', patientId);
        if (typeof PatientSelect2 !== 'undefined' && PatientSelect2.setValue) {
            PatientSelect2.setValue('#PatientId', {
                id: patientId,
                text: 'بیمار انتخاب شده'
            });
        }
    }
    
    var planId = $('#InsurancePlanId').val();
    if (planId) {
        showPlanDetails(planId);
    }
    
    var isPrimary = $('#IsPrimary').is(':checked');
    if (isPrimary) {
        showPrimaryInsuranceWarning();
    }
}

/**
 * Validate form before submission
 */
function validateForm() {
    var isValid = true;
    
    // Check required fields
    var requiredFields = ['PatientId', 'InsurancePlanId', 'PolicyNumber', 'StartDateShamsi', 'EndDateShamsi'];
    
    requiredFields.forEach(function(fieldId) {
        var field = $('#' + fieldId);
        if (!field.val()) {
            field.addClass('is-invalid');
            
            // نمایش پیام خطا با نام دوستانه فیلد
            var friendlyFieldName = getFriendlyFieldName(fieldId);
            var errorMessage = friendlyFieldName + ' الزامی است.';
            
            field.next('.invalid-feedback').remove();
            field.after('<div class="invalid-feedback">' + errorMessage + '</div>');
            
            isValid = false;
        } else {
            field.removeClass('is-invalid');
            field.next('.invalid-feedback').remove();
        }
    });
    
    // Validate date range
    var startDateShamsi = $('#StartDateShamsi').val();
    var endDateShamsi = $('#EndDateShamsi').val();
    
    if (startDateShamsi && endDateShamsi) {
        try {
            var fromDate = convertPersianToGregorian(startDateShamsi);
            var toDate = convertPersianToGregorian(endDateShamsi);
            
            if (fromDate && toDate) {
                if (fromDate >= toDate) {
                    var startFieldName = getFriendlyFieldName('StartDateShamsi');
                    var endFieldName = getFriendlyFieldName('EndDateShamsi');
                    showDateError('EndDateShamsi', endFieldName + ' باید بعد از ' + startFieldName + ' باشد');
                    isValid = false;
                }
            } else {
                showDateError('EndDateShamsi', 'تاریخ‌ها نامعتبر هستند');
                isValid = false;
            }
        } catch (e) {
            showDateError('EndDateShamsi', 'خطا در اعتبارسنجی تاریخ‌ها');
            isValid = false;
        }
    }
    
    return isValid;
}

/**
 * Show loading state
 */
function showLoading(element) {
    $(element).addClass('loading');
}

/**
 * Hide loading state
 */
function hideLoading(element) {
    $(element).removeClass('loading');
}

/**
 * Show success message
 */
function showSuccessMessage(message) {
    var alertHtml = '<div class="alert alert-success alert-dismissible fade show" role="alert">' +
        '<i class="fas fa-check-circle"></i> ' + message +
        '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
        '</div>';
    $('.container-fluid').prepend(alertHtml);
    
    // Auto-hide after 5 seconds
    setTimeout(function() {
        $('.alert-success').fadeOut();
    }, 5000);
}

/**
 * Show error message
 */
function showErrorMessage(message) {
    var alertHtml = '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
        '<i class="fas fa-exclamation-circle"></i> ' + message +
        '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
        '</div>';
    $('.container-fluid').prepend(alertHtml);
    
    // Auto-hide after 7 seconds
    setTimeout(function() {
        $('.alert-danger').fadeOut();
    }, 7000);
}

// Export functions for global access
window.PatientInsuranceForm = {
    initializeFormWithData: initializeFormWithData,
    validateForm: validateForm,
    showLoading: showLoading,
    hideLoading: hideLoading,
    showSuccessMessage: showSuccessMessage,
    showErrorMessage: showErrorMessage,
    getFriendlyFieldName: getFriendlyFieldName
};
