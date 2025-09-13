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

    // Initialize Persian DatePicker
    function initializePersianDatePicker() {
        if (typeof persianDatepicker !== 'undefined') {
            $('.persian-datepicker').persianDatepicker({
                format: 'YYYY/MM/DD',
                altField: '.gregorian-date',
                altFormat: 'YYYY-MM-DD',
                observer: true,
                timePicker: {
                    enabled: false
                },
                autoClose: true,
                initialValue: false,
                position: 'auto',
                viewMode: 'day',
                minDate: null,
                maxDate: null,
                checkDate: function(unix) {
                    return true;
                },
                checkMonth: function(month) {
                    return true;
                },
                checkYear: function(year) {
                    return true;
                },
                navigator: {
                    enabled: true,
                    scroll: {
                        enabled: true
                    },
                    text: {
                        btnNextText: 'بعدی',
                        btnPrevText: 'قبلی'
                    }
                },
                toolbox: {
                    enabled: true,
                    calendarSwitch: {
                        enabled: true,
                        format: 'MMMM'
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
                calendar: {
                    persian: {
                        locale: 'fa',
                        showHint: true,
                        leapYearMode: 'algorithmic'
                    },
                    gregorian: {
                        locale: 'en',
                        showHint: true
                    }
                },
                timePicker: {
                    enabled: false
                },
                dayPicker: {
                    enabled: true,
                    titleFormat: 'YYYY MMMM'
                },
                monthPicker: {
                    enabled: true,
                    titleFormat: 'YYYY'
                },
                yearPicker: {
                    enabled: true,
                    titleFormat: 'YYYY'
                },
                responsive: true,
                zIndex: 9999
            });
        } else {
            console.warn('Persian DatePicker library not loaded');
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

    // Date validation helper
    function validateDateRange() {
        var validFrom = $('#ValidFromShamsi').val();
        var validTo = $('#ValidToShamsi').val();
        
        if (validFrom && validTo) {
            try {
                if (typeof persianDate !== 'undefined') {
                    var fromDate = new persianDate(validFrom.split('/')).toDate();
                    var toDate = new persianDate(validTo.split('/')).toDate();
                    
                    if (fromDate >= toDate) {
                        $('#ValidToShamsi').addClass('is-invalid');
                        return false;
                    } else {
                        $('#ValidToShamsi').removeClass('is-invalid');
                        return true;
                    }
                }
            } catch (e) {
                console.log('Date conversion error:', e);
                return false;
            }
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

    // Initialize date validation on change
    function initializeDateValidation() {
        $('.persian-datepicker').on('change', function() {
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
