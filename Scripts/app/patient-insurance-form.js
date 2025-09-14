/**
 * Patient Insurance Form - Central JavaScript File
 * Handles form validation, date conversion, and UI interactions
 */

$(document).ready(function() {
    // Initialize Persian DatePicker
    initializePersianDatePicker();
    
    // Initialize form validation
    initializeFormValidation();
    
    // Initialize event handlers
    initializeEventHandlers();
});

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
 * Handle date field changes - convert Persian to Gregorian and validate
 */
function handleDateChange() {
    var startDateShamsi = $('#StartDateShamsi').val();
    var endDateShamsi = $('#EndDateShamsi').val();
    
    // Convert Persian dates to Gregorian
    if (startDateShamsi) {
        try {
            var startDateGregorian = persianDate(startDateShamsi).toDate();
            $('#StartDate').val(startDateGregorian.toISOString().split('T')[0]);
        } catch (e) {
            console.error('Error converting start date:', e);
            showDateError('StartDateShamsi', 'تاریخ شروع نامعتبر است');
        }
    }
    
    if (endDateShamsi) {
        try {
            var endDateGregorian = persianDate(endDateShamsi).toDate();
            $('#EndDate').val(endDateGregorian.toISOString().split('T')[0]);
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
        var fromDate = persianDate(startDateShamsi).toDate();
        var toDate = persianDate(endDateShamsi).toDate();
        
        if (fromDate >= toDate) {
            showDateError('EndDateShamsi', 'تاریخ پایان باید بعد از تاریخ شروع باشد');
        } else {
            clearDateError('EndDateShamsi');
        }
    } catch (e) {
        console.error('Error validating dates:', e);
        showDateError('EndDateShamsi', 'خطا در اعتبارسنجی تاریخ‌ها');
    }
}

/**
 * Show date error
 */
function showDateError(fieldId, message) {
    $('#' + fieldId).addClass('is-invalid');
    $('#' + fieldId).next('.invalid-feedback').remove();
    $('#' + fieldId).after('<div class="invalid-feedback">' + message + '</div>');
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
    // Convert existing Gregorian dates to Persian for display
    var startDate = $('#StartDate').val();
    var endDate = $('#EndDate').val();
    
    if (startDate) {
        try {
            var startDateShamsi = persianDate(new Date(startDate)).format('YYYY/MM/DD');
            $('#StartDateShamsi').val(startDateShamsi);
        } catch (e) {
            console.error('Error converting start date to Persian:', e);
        }
    }
    
    if (endDate) {
        try {
            var endDateShamsi = persianDate(new Date(endDate)).format('YYYY/MM/DD');
            $('#EndDateShamsi').val(endDateShamsi);
        } catch (e) {
            console.error('Error converting end date to Persian:', e);
        }
    }
    
    // Initialize other form elements
    var patientId = $('#PatientId').val();
    if (patientId) {
        loadPatientInsurances(patientId);
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
            isValid = false;
        } else {
            field.removeClass('is-invalid');
        }
    });
    
    // Validate date range
    var startDateShamsi = $('#StartDateShamsi').val();
    var endDateShamsi = $('#EndDateShamsi').val();
    
    if (startDateShamsi && endDateShamsi) {
        try {
            var fromDate = persianDate(startDateShamsi).toDate();
            var toDate = persianDate(endDateShamsi).toDate();
            
            if (fromDate >= toDate) {
                showDateError('EndDateShamsi', 'تاریخ پایان باید بعد از تاریخ شروع باشد');
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
    showErrorMessage: showErrorMessage
};
