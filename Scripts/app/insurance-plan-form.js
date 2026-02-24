/**
 * Insurance Plan Form JavaScript - طبق قرارداد
 * اصول UX/UI مخصوص محیط درمانی - سادگی، وضوح، خوانایی
 */

(function() {
    'use strict';

    // Ensure jQuery is loaded
    function ensureJQuery(callback) {
        if (typeof $ !== 'undefined') {
            callback();
        } else {
            setTimeout(function() {
                ensureJQuery(callback);
            }, 100);
        }
    }

    // Initialize form when DOM is ready
    function initializeForm() {
        ensureJQuery(function() {
            console.log('Insurance Plan Form initialized');
            
            // Initialize Persian DatePicker
            initializePersianDatePicker();
            
            // Initialize form validation
            initializeFormValidation();
            
            // Initialize input formatting
            initializeInputFormatting();
        });
    }

    // ✅ استفاده از JalaliDatePicker Enterprise (طبق JALALIDATEPICKER_ENTERPRISE_GUIDE.md)
    function initializePersianDatePicker() {
        if (typeof JalaliDatePickerEnterprise !== 'undefined') {
            JalaliDatePickerEnterprise.startWatchAgain();
        } else if (typeof jalaliDatepicker !== 'undefined') {
            setTimeout(function() {
                if (typeof JalaliDatePickerEnterprise !== 'undefined') {
                    JalaliDatePickerEnterprise.startWatchAgain();
                }
            }, 300);
        }
    }

    // Initialize form validation
    function initializeFormValidation() {
        // Bootstrap validation
        (function() {
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
        })();
    }

    // Initialize input formatting
    function initializeInputFormatting() {
        // Coverage percent formatting
        $('#CoveragePercent').on('input', function() {
            var value = parseFloat($(this).val());
            
            // Allow empty values and zero
            if ($(this).val() === '' || $(this).val() === '0') {
                return;
            }
            
            if (isNaN(value)) {
                return; // Don't clear, let user continue typing
            }
            
            // Ensure value is between 0 and 100
            if (value < 0) {
                $(this).val('0');
            } else if (value > 100) {
                $(this).val('100');
            }
        });

        // Deductible formatting
        $('#Deductible').on('input', function() {
            var value = parseFloat($(this).val());
            
            // Allow empty values and zero
            if ($(this).val() === '' || $(this).val() === '0') {
                return;
            }
            
            if (isNaN(value)) {
                return; // Don't clear, let user continue typing
            }
            
            // Ensure value is not negative
            if (value < 0) {
                $(this).val('0');
            }
        });

        // Number formatting for display (only on blur, not on input)
        $('#CoveragePercent').on('blur', function() {
            var value = parseFloat($(this).val());
            if (!isNaN(value) && value >= 0) {
                // Format as integer if it's a whole number, otherwise keep 2 decimal places
                if (value % 1 === 0) {
                    $(this).val(value.toString());
                } else {
                    $(this).val(value.toFixed(2));
                }
            }
        });

        // No formatting for deductible - keep it simple
        // Remove any existing formatting on focus for editing
        $('#CoveragePercent, #Deductible').on('focus', function() {
            var value = $(this).val().replace(/,/g, '');
            $(this).val(value);
        });
    }

    // Date validation helper (سازگار با JalaliDatePicker Enterprise و فرمت YYYY/MM/DD)
    function validateDateRange() {
        var validFrom = ($('#ValidFromShamsi').val() || '').trim();
        var validTo = ($('#ValidToShamsi').val() || '').trim();
        
        if (!validFrom || !validTo) return true;
        
        try {
            // استفاده از JalaliDatePickerEnterprise برای تبدیل و مقایسه
            if (typeof JalaliDatePickerEnterprise !== 'undefined' && JalaliDatePickerEnterprise.convertPersianToGregorian) {
                var gFrom = JalaliDatePickerEnterprise.convertPersianToGregorian(validFrom);
                var gTo = JalaliDatePickerEnterprise.convertPersianToGregorian(validTo);
                if (gFrom && gTo) {
                    var fromDate = new Date(gFrom);
                    var toDate = new Date(gTo);
                    if (fromDate >= toDate) {
                        $('#ValidToShamsi').addClass('is-invalid');
                        return false;
                    }
                    $('#ValidToShamsi').removeClass('is-invalid');
                    return true;
                }
            }
            // Fallback: مقایسه رشته‌ای برای فرمت YYYY/MM/DD
            var fromParts = validFrom.split('/').map(function(x) { return parseInt(x, 10) || 0; });
            var toParts = validTo.split('/').map(function(x) { return parseInt(x, 10) || 0; });
            if (fromParts.length >= 3 && toParts.length >= 3) {
                var cmp = fromParts[0] !== toParts[0] ? fromParts[0] - toParts[0] : (fromParts[1] !== toParts[1] ? fromParts[1] - toParts[1] : fromParts[2] - toParts[2]);
                if (cmp >= 0) {
                    $('#ValidToShamsi').addClass('is-invalid');
                    return false;
                }
                $('#ValidToShamsi').removeClass('is-invalid');
                return true;
            }
        } catch (e) {
            console.warn('Date range validation error:', e);
        }
        return true;
    }

    // Form submission handler
    function handleFormSubmission() {
        $('form').on('submit', function(e) {
            // Validate date range
            if (!validateDateRange()) {
                e.preventDefault();
                alert('تاریخ پایان باید بعد از تاریخ شروع باشد.');
                return false;
            }
            
            // Additional validations can be added here
            return true;
        });
    }

    // Initialize date validation on change (پشتیبانی از data-jdp و inputهای تاریخ طرح بیمه)
    function initializeDateValidation() {
        $('#ValidFromShamsi, #ValidToShamsi').on('change jdp:change', function() {
            validateDateRange();
        });
    }

    // Public API
    window.InsurancePlanForm = {
        init: initializeForm,
        validateDateRange: validateDateRange,
        handleFormSubmission: handleFormSubmission
    };

    // Auto-initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeForm);
    } else {
        initializeForm();
    }

})();
