/**
 * RECEPTION MODULES - PROFESSIONAL MEDICAL ENVIRONMENT
 * ===================================================
 * 
 * اصول طراحی:
 * 1. SRP - هر ماژول یک مسئولیت
 * 2. DRY - عدم تکرار کد
 * 3. Professional UX - تجربه کاربری حرفه‌ای
 * 4. Medical Environment - محیط درمانی
 * 5. Production Ready - آماده production
 */

// Global Namespace برای Reception Modules
window.ReceptionModules = window.ReceptionModules || {};

// ========================================
// UTILITY FUNCTIONS - توابع کمکی
// ========================================
window.ReceptionModules.utils = {
    debounce: function(func, wait) {
        var timeout;
        return function executedFunction() {
            var args = Array.prototype.slice.call(arguments);
            var later = function() {
                clearTimeout(timeout);
                func.apply(null, args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },
    
    throttle: function(func, limit) {
        var inThrottle;
        return function() {
            var args = arguments;
            var context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(function() { inThrottle = false; }, limit);
            }
        };
    },
    
    formatCurrency: function(amount) {
        return new Intl.NumberFormat('fa-IR').format(amount) + ' ریال';
    },
    
    validateNationalCode: function(code) {
        if (!/^\d{10}$/.test(code)) return false;
        
        var digits = code.split('').map(Number);
        var checkDigit = digits[9];
        var sum = digits.slice(0, 9).reduce(function(acc, digit, index) { return acc + digit * (10 - index); }, 0);
        var remainder = sum % 11;
        
        return (remainder < 2 && checkDigit === remainder) || (remainder >= 2 && checkDigit === 11 - remainder);
    },
    
    formatPhoneNumber: function(phone) {
        return phone.replace(/(\d{4})(\d{3})(\d{4})/, '$1-$2-$3');
    }
};

// ========================================
// CONFIGURATION MODULE - تنظیمات پویا
// ========================================
window.ReceptionModules.config = {
    // API Endpoints - از دیتابیس یا Configuration
    apiEndpoints: {
        patientSearch: '/Reception/Patient/SearchByNationalCode',
        patientSave: '/Reception/Patient/Save',
        departmentLoad: '/Reception/Department/Load',
        departmentSave: '/Reception/Department/Save',
        insuranceLoad: '/Reception/Insurance/Load',
        insuranceSave: '/Reception/Insurance/Save',
        serviceLoad: '/Reception/Service/Load',
        serviceCalculate: '/Reception/Service/Calculate',
        paymentRefresh: '/Reception/Payment/Refresh',
        paymentProcess: '/Reception/Payment/Process'
    },
    
    // Messages - از دیتابیس یا Resource Files
    messages: {
        validation: {
            nationalCodeInvalid: 'کد ملی نامعتبر است',
            nationalCodeRequired: 'کد ملی الزامی است',
            nationalCodeLength: 'کد ملی باید 10 رقم باشد',
            requiredField: 'این فیلد الزامی است'
        },
        success: {
            patientFound: 'بیمار با موفقیت یافت شد',
            patientSaved: 'اطلاعات بیمار با موفقیت ذخیره شد',
            departmentsLoaded: 'دپارتمان‌ها با موفقیت بارگذاری شدند',
            departmentSaved: 'انتخاب دپارتمان با موفقیت ذخیره شد',
            insuranceLoaded: 'اطلاعات بیمه با موفقیت بارگذاری شد',
            insuranceSaved: 'اطلاعات بیمه با موفقیت ذخیره شد',
            servicesLoaded: 'خدمات با موفقیت بارگذاری شدند',
            servicesCalculated: 'محاسبه خدمات با موفقیت انجام شد',
            paymentRefreshed: 'اطلاعات پرداخت با موفقیت به‌روزرسانی شد',
            paymentProcessed: 'پرداخت با موفقیت انجام شد'
        },
        error: {
            patientNotFound: 'بیمار یافت نشد',
            serverError: 'خطا در ارتباط با سرور',
            timeoutError: 'زمان اتصال به سرور تمام شد',
            networkError: 'خطا در اتصال به شبکه',
            insuranceLoadError: 'خطا در بارگذاری اطلاعات بیمه'
        },
        info: {
            patientSearching: 'در حال جستجوی بیمار...',
            insuranceLoading: 'در حال بارگذاری اطلاعات بیمه...'
        }
    },
    
    // Timeouts - قابل تنظیم
    timeouts: {
        ajax: 10000,
        autoRefresh: 30000
    }
};

// ========================================
// CORE MODULE - مدیریت اصلی
// ========================================
window.ReceptionModules.Core = {
    // Performance Tracking
    performance: {
        startTime: Date.now(),
        initCount: 0,
        errorCount: 0
    },

    // Error Handler مرکزی
    errorHandler: function(error, context) {
        console.error(`[ReceptionModules] Error in ${context}:`, error);
        this.performance.errorCount++;
        
        // در محیط Production، ارسال به logging service
        if (window.location.hostname !== 'localhost') {
            // TODO: ارسال به logging service
        }
    },

    // Utility Functions
    utils: {
        debounce: function(func, wait) {
            var timeout;
            return function executedFunction() {
                var args = Array.prototype.slice.call(arguments);
                var later = function() {
                    clearTimeout(timeout);
                    func.apply(null, args);
                };
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
            };
        },
        
        throttle: function(func, limit) {
            var inThrottle;
            return function() {
                var args = arguments;
                if (!inThrottle) {
                    func.apply(this, args);
                    inThrottle = true;
                    setTimeout(function() { inThrottle = false; }, limit);
                }
            };
        }
    }
};

// ========================================
// ACCORDION MODULE - مدیریت آکاردئون
// ========================================
window.ReceptionModules.Accordion = {
    // Initialize accordion functionality
    init: function() {
        console.log('[ReceptionModules.Accordion] Initializing...');
        
        // Accordion click handlers
        $('.accordion-section .section-header').off('click.accordion').on('click.accordion', function() {
            var $section = $(this).closest('.accordion-section');
            var $content = $section.find('.section-content');
            
            // Toggle active state
            if ($section.hasClass('active')) {
                $section.removeClass('active');
                $content.slideUp(300);
            } else {
                // Close other sections
                $('.accordion-section.active').removeClass('active');
                $('.accordion-section .section-content').slideUp(300);
                
                // Open current section
                $section.addClass('active');
                $content.slideDown(300);
            }
        });

        // Auto-open first section
        $('.accordion-section').first().addClass('active');
        $('.accordion-section').first().find('.section-content').show();
        
        console.log('[ReceptionModules.Accordion] Initialized successfully');
    },

    // Section state management
    setState: function(sectionId, state) {
        var $section = $('#' + sectionId);
        $section.removeClass('completed in-progress error');
        $section.addClass(state);
        
        // Update progress bar
        this.updateProgress();
    },

    // Update progress bar
    updateProgress: function() {
        var completedSections = $('.accordion-section.completed').length;
        var totalSections = $('.accordion-section').length;
        var percentage = Math.round((completedSections / totalSections) * 100);
        
        $('#completedSections').text(completedSections);
        $('#totalSections').text(totalSections);
        $('#progressFill').css('width', percentage + '%');
        $('#progressFill').attr('data-width', percentage);
    }
};

// ========================================
// PATIENT MODULE - مدیریت بیمار
// ========================================
window.ReceptionModules.Patient = {
    init: function() {
        console.log('[ReceptionModules.Patient] Initializing...');
        
        // National Code validation
        $('#patientNationalCode').on('input', window.ReceptionModules.utils.debounce(function() {
            var value = $(this).val() || '';
                if (value.length === 10) {
                    if (window.ReceptionModules.Patient.validateNationalCode(value)) {
                        $(this).removeClass('is-invalid').addClass('is-valid');
                        $('#nationalCodeError').text('');
                    } else {
                        $(this).removeClass('is-valid').addClass('is-invalid');
                        $('#nationalCodeError').text(window.ReceptionModules.config.messages.validation.nationalCodeInvalid);
                    }
                } else {
                    $(this).removeClass('is-valid is-invalid');
                    $('#nationalCodeError').text('');
                }
        }.bind(this), 500));

        // Search patient
        $('#searchPatientBtn').off('click.patient').on('click.patient', window.ReceptionModules.Patient.handleSearchPatient);
        
        // Enter key support for national code
        $('#patientNationalCode').off('keypress.patient').on('keypress.patient', function(e) {
            if (e.which === 13) { // Enter key
                window.ReceptionModules.Patient.handleSearchPatient.call($('#searchPatientBtn')[0]);
            }
        });
        
        // Save patient
        $('#savePatientBtn').off('click.patient').on('click.patient', window.ReceptionModules.Patient.handleSavePatient);
        
        console.log('[ReceptionModules.Patient] Initialized successfully');
    },

    validateNationalCode: function(nationalCode) {
        if (!nationalCode || nationalCode.length !== 10) return false;
        
        try {
            var digits = nationalCode.split('').map(Number);
            var checkDigit = digits[9];
            var sum = digits.slice(0, 9).reduce(function(acc, digit, index) { return acc + digit * (10 - index); }, 0);
            var remainder = sum % 11;
            
            return remainder < 2 ? checkDigit === remainder : checkDigit === 11 - remainder;
        } catch (error) {
            console.error('Error validating national code:', error);
            return false;
        }
    },

    handleSearchPatient: function() {
        var $btn = $(this);
        var nationalCode = ($('#patientNationalCode').val() || '').trim();
        
        // اعتبارسنجی اولیه
        if (!nationalCode) {
            window.ReceptionModules.Patient.showError(window.ReceptionModules.config.messages.validation.nationalCodeRequired);
            return;
        }
        
        if (nationalCode.length !== 10) {
            window.ReceptionModules.Patient.showError(window.ReceptionModules.config.messages.validation.nationalCodeLength);
            return;
        }
        
        if (!window.ReceptionModules.Patient.validateNationalCode(nationalCode)) {
            window.ReceptionModules.Patient.showError(window.ReceptionModules.config.messages.validation.nationalCodeInvalid);
            return;
        }
        
        // نمایش loading state
        window.ReceptionModules.Patient.showButtonLoading($btn);
        window.ReceptionModules.Patient.showInfo(window.ReceptionModules.config.messages.info.patientSearching);
        
        // درخواست AJAX
        $.ajax({
            url: window.ReceptionModules.config.apiEndpoints.patientSearch,
            type: 'POST',
            data: { 
                nationalCode: nationalCode,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            dataType: 'json',
            timeout: 15000,
            cache: false,
            success: function(response) {
                window.ReceptionModules.Patient.hideButtonLoading($btn);
                
                if (response.success && response.data) {
                    // نمایش اطلاعات بیمار
                    window.ReceptionModules.Patient.displayPatientInfo(response.data);
                    window.ReceptionModules.Patient.showSuccess(window.ReceptionModules.config.messages.success.patientFound);
                    
                    // بارگذاری اطلاعات بیمه
                    window.ReceptionModules.Patient.loadPatientInsurance(response.data.patientId);
                    
                    // به‌روزرسانی وضعیت آکاردئون
                    window.ReceptionModules.Accordion.setState('patientSection', 'completed');
                } else {
                    window.ReceptionModules.Patient.showError(response.message || window.ReceptionModules.config.messages.error.patientNotFound);
                }
            },
            error: function(xhr, status, error) {
                window.ReceptionModules.Patient.hideButtonLoading($btn);
                
                if (status === 'timeout') {
                    window.ReceptionModules.Patient.showError(window.ReceptionModules.config.messages.error.timeoutError);
                } else if (xhr.status === 0) {
                    window.ReceptionModules.Patient.showError(window.ReceptionModules.config.messages.error.networkError);
                } else {
                    window.ReceptionModules.Patient.showError(window.ReceptionModules.config.messages.error.serverError);
                }
            }
        });
    },

    handleSavePatient: function() {
        var $btn = $(this);
        window.ReceptionModules.Patient.showButtonLoading($btn);
        
        // TODO: Implement save patient logic
        setTimeout(function() {
            window.ReceptionModules.Patient.hideButtonLoading($btn);
            window.ReceptionModules.Patient.showSuccess(window.ReceptionModules.config.messages.success.patientSaved);
        }, 1000);
    },

    displayPatientInfo: function(patientData) {
        if (!patientData) {
            console.error('Patient data is null or undefined');
            return;
        }
        
        try {
            $('#patientFirstName').val(patientData.firstName || '');
            $('#patientLastName').val(patientData.lastName || '');
            $('#patientFatherName').val(patientData.fatherName || ''); // اضافه شده
            $('#patientBirthDate').val(patientData.birthDate || '');
            $('#patientAge').val(patientData.age || ''); // اضافه شده
            $('#patientGender').val(patientData.gender || '');
            $('#patientPhone').val(patientData.phoneNumber || '');
            $('#patientAddress').val(patientData.address || '');
        } catch (error) {
            console.error('Error displaying patient info:', error);
        }
    },

    loadPatientInsurance: function(patientId) {
        if (!patientId) {
            console.warn('Patient ID not provided for insurance loading');
            return;
        }

        window.ReceptionModules.Patient.showInfo(window.ReceptionModules.config.messages.info.insuranceLoading);

        $.ajax({
            url: window.ReceptionModules.config.apiEndpoints.insuranceLoad,
            type: 'POST',
            data: { 
                patientId: patientId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            dataType: 'json',
            timeout: 10000,
            cache: false,
            success: function(response) {
                if (response.success && response.data) {
                    window.ReceptionModules.Patient.displayInsuranceInfo(response.data);
                    window.ReceptionModules.Patient.showSuccess(window.ReceptionModules.config.messages.success.insuranceLoaded);
                } else {
                    window.ReceptionModules.Patient.showWarning(response.message || 'اطلاعات بیمه یافت نشد');
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading patient insurance:', error);
                window.ReceptionModules.Patient.showError(window.ReceptionModules.config.messages.error.insuranceLoadError);
            }
        });
    },

    displayInsuranceInfo: function(insuranceData) {
        if (!insuranceData) {
            console.error('Insurance data is null or undefined');
            return;
        }
        
        try {
            // نمایش اطلاعات بیمه در بخش مربوطه
            if (insuranceData.primaryInsurance) {
                $('#primaryInsuranceName').val(insuranceData.primaryInsurance.name || '');
                $('#primaryInsuranceNumber').val(insuranceData.primaryInsurance.number || '');
            }
            
            if (insuranceData.supplementaryInsurance) {
                $('#supplementaryInsuranceName').val(insuranceData.supplementaryInsurance.name || '');
                $('#supplementaryInsuranceNumber').val(insuranceData.supplementaryInsurance.number || '');
            }
        } catch (error) {
            console.error('Error displaying insurance info:', error);
        }
    },

    showButtonLoading: function($btn) {
        if (!$btn || $btn.length === 0) {
            console.error('Button element is null or undefined');
            return;
        }
        
        try {
            $btn.prop('disabled', true);
            $btn.find('.btn-text').addClass('d-none');
            $btn.find('.btn-loading').removeClass('d-none');
        } catch (error) {
            console.error('Error showing button loading:', error);
        }
    },

    hideButtonLoading: function($btn) {
        if (!$btn || $btn.length === 0) {
            console.error('Button element is null or undefined');
            return;
        }
        
        try {
            $btn.prop('disabled', false);
            $btn.find('.btn-text').removeClass('d-none');
            $btn.find('.btn-loading').addClass('d-none');
        } catch (error) {
            console.error('Error hiding button loading:', error);
        }
    },

    showError: function(message) {
        // نمایش خطا با Toastr
        if (window.ReceptionToastr) {
            window.ReceptionToastr.helpers.showError(message);
        } else {
            // Fallback به روش قدیمی
            $('#patientErrorContainer').removeClass('d-none');
            $('#patientErrorContainer .error-message').text(message);
        }
    },

    showSuccess: function(message) {
        // نمایش موفقیت با Toastr
        if (window.ReceptionToastr) {
            window.ReceptionToastr.helpers.showSuccess(message);
        } else {
            // Fallback به روش قدیمی
            $('#patientSuccessContainer').removeClass('d-none');
            $('#patientSuccessContainer .success-message').text(message);
        }
    },

    showWarning: function(message) {
        // نمایش هشدار با Toastr
        if (window.ReceptionToastr) {
            window.ReceptionToastr.helpers.showWarning(message);
        }
    },

    showInfo: function(message) {
        // نمایش اطلاعات با Toastr
        if (window.ReceptionToastr) {
            window.ReceptionToastr.helpers.showInfo(message);
        }
    }
};

// ========================================
// DEPARTMENT MODULE - مدیریت دپارتمان
// ========================================
window.ReceptionModules.Department = {
    init: function() {
        console.log('[ReceptionModules.Department] Initializing...');
        
        // Load departments
        $('#loadDepartmentsBtn').off('click.department').on('click.department', this.handleLoadDepartments.bind(this));
        
        // Save department
        $('#saveDepartmentBtn').off('click.department').on('click.department', this.handleSaveDepartment.bind(this));
        
        console.log('[ReceptionModules.Department] Initialized successfully');
    },

    handleLoadDepartments: function() {
        var $btn = $(this);
        this.showButtonLoading($btn);
        
        // TODO: Implement load departments logic
        setTimeout(function() {
            this.hideButtonLoading($btn);
            this.showSuccess(window.ReceptionModules.config.messages.success.departmentsLoaded);
        }, 1000);
    },

    handleSaveDepartment: function() {
        var $btn = $(this);
        this.showButtonLoading($btn);
        
        // TODO: Implement save department logic
        setTimeout(function() {
            this.hideButtonLoading($btn);
            this.showSuccess(window.ReceptionModules.config.messages.success.departmentSaved);
            window.ReceptionModules.Accordion.setState('departmentSection', 'completed');
        }, 1000);
    },

    showButtonLoading: function($btn) {
        $btn.prop('disabled', true);
        $btn.find('.btn-text').addClass('d-none');
        $btn.find('.btn-loading').removeClass('d-none');
    },

    hideButtonLoading: function($btn) {
        $btn.prop('disabled', false);
        $btn.find('.btn-text').removeClass('d-none');
        $btn.find('.btn-loading').addClass('d-none');
    },

    showSuccess: function(message) {
        // TODO: Implement success message display
        console.log('Success:', message);
    }
};

// ========================================
// INSURANCE MODULE - مدیریت بیمه
// ========================================
window.ReceptionModules.Insurance = {
    init: function() {
        console.log('[ReceptionModules.Insurance] Initializing...');
        
        // Load patient insurance
        $('#loadPatientInsuranceBtn').off('click.insurance').on('click.insurance', this.handleLoadInsurance.bind(this));
        
        // Save insurance
        $('#saveInsuranceBtn').off('click.insurance').on('click.insurance', this.handleSaveInsurance.bind(this));
        
        console.log('[ReceptionModules.Insurance] Initialized successfully');
    },

    handleLoadInsurance: function() {
        var $btn = $(this);
        this.showButtonLoading($btn);
        
        // TODO: Implement load insurance logic
        setTimeout(function() {
            this.hideButtonLoading($btn);
            this.showSuccess(window.ReceptionModules.config.messages.success.insuranceLoaded);
        }, 1000);
    },

    handleSaveInsurance: function() {
        var $btn = $(this);
        this.showButtonLoading($btn);
        
        // TODO: Implement save insurance logic
        setTimeout(function() {
            this.hideButtonLoading($btn);
            this.showSuccess(window.ReceptionModules.config.messages.success.insuranceSaved);
            window.ReceptionModules.Accordion.setState('insuranceSection', 'completed');
        }, 1000);
    },

    showButtonLoading: function($btn) {
        $btn.prop('disabled', true);
        $btn.find('.btn-text').addClass('d-none');
        $btn.find('.btn-loading').removeClass('d-none');
    },

    hideButtonLoading: function($btn) {
        $btn.prop('disabled', false);
        $btn.find('.btn-text').removeClass('d-none');
        $btn.find('.btn-loading').addClass('d-none');
    },

    showSuccess: function(message) {
        console.log('Success:', message);
    }
};

// ========================================
// SERVICE MODULE - مدیریت خدمات
// ========================================
window.ReceptionModules.Service = {
    init: function() {
        console.log('[ReceptionModules.Service] Initializing...');
        
        // Load services
        $('#loadServicesBtn').off('click.service').on('click.service', this.handleLoadServices.bind(this));
        
        // Calculate services
        $('#calculateServicesBtn').off('click.service').on('click.service', this.handleCalculateServices.bind(this));
        
        console.log('[ReceptionModules.Service] Initialized successfully');
    },

    handleLoadServices: function() {
        var $btn = $(this);
        this.showButtonLoading($btn);
        
        // TODO: Implement load services logic
        setTimeout(function() {
            this.hideButtonLoading($btn);
            this.showSuccess(window.ReceptionModules.config.messages.success.servicesLoaded);
        }, 1000);
    },

    handleCalculateServices: function() {
        var $btn = $(this);
        this.showButtonLoading($btn);
        
        // TODO: Implement calculate services logic
        setTimeout(function() {
            this.hideButtonLoading($btn);
            this.showSuccess(window.ReceptionModules.config.messages.success.servicesCalculated);
            window.ReceptionModules.Accordion.setState('serviceSection', 'completed');
        }, 1000);
    },

    showButtonLoading: function($btn) {
        $btn.prop('disabled', true);
        $btn.find('.btn-text').addClass('d-none');
        $btn.find('.btn-loading').removeClass('d-none');
    },

    hideButtonLoading: function($btn) {
        $btn.prop('disabled', false);
        $btn.find('.btn-text').removeClass('d-none');
        $btn.find('.btn-loading').addClass('d-none');
    },

    showSuccess: function(message) {
        console.log('Success:', message);
    }
};

// ========================================
// PAYMENT MODULE - مدیریت پرداخت
// ========================================
window.ReceptionModules.Payment = {
    init: function() {
        console.log('[ReceptionModules.Payment] Initializing...');
        
        // Refresh payment
        $('#refreshPaymentBtn').off('click.payment').on('click.payment', this.handleRefreshPayment.bind(this));
        
        // Process payment
        $('#processPaymentBtn').off('click.payment').on('click.payment', this.handleProcessPayment.bind(this));
        
        console.log('[ReceptionModules.Payment] Initialized successfully');
    },

    handleRefreshPayment: function() {
        var $btn = $(this);
        this.showButtonLoading($btn);
        
        // TODO: Implement refresh payment logic
        setTimeout(function() {
            this.hideButtonLoading($btn);
            this.showSuccess(window.ReceptionModules.config.messages.success.paymentRefreshed);
        }, 1000);
    },

    handleProcessPayment: function() {
        var $btn = $(this);
        this.showButtonLoading($btn);
        
        // TODO: Implement process payment logic
        setTimeout(function() {
            this.hideButtonLoading($btn);
            this.showSuccess(window.ReceptionModules.config.messages.success.paymentProcessed);
            window.ReceptionModules.Accordion.setState('paymentSection', 'completed');
        }, 1000);
    },

    showButtonLoading: function($btn) {
        $btn.prop('disabled', true);
        $btn.find('.btn-text').addClass('d-none');
        $btn.find('.btn-loading').removeClass('d-none');
    },

    hideButtonLoading: function($btn) {
        $btn.prop('disabled', false);
        $btn.find('.btn-text').removeClass('d-none');
        $btn.find('.btn-loading').addClass('d-none');
    },

    showSuccess: function(message) {
        console.log('Success:', message);
    }
};

// ========================================
// REAL-TIME MODULE - ویژگی‌های real-time
// ========================================
window.ReceptionModules.RealTime = {
    init: function() {
        console.log('[ReceptionModules.RealTime] Initializing...');
        
        // Disable caching for all AJAX requests
        $.ajaxSetup({
            cache: false,
            headers: {
                'Cache-Control': 'no-cache, no-store, must-revalidate',
                'Pragma': 'no-cache',
                'Expires': '0'
            }
        });

        // Enable real-time validation
        this.enableRealTimeValidation();
        
        // Enable auto-refresh
        this.enableAutoRefresh();
        
        console.log('[ReceptionModules.RealTime] Initialized successfully');
    },

    enableRealTimeValidation: function() {
        // Real-time validation for all inputs with data-validation attribute
        $('[data-validation]').on('input', function() {
            var $input = $(this);
            var validationType = $input.attr('data-validation');
            
            if (validationType === 'national-code') {
                var value = $input.val();
                if (value.length === 10) {
                    // TODO: Implement national code validation
                }
            }
        });
    },

    enableAutoRefresh: function() {
        // Auto-refresh patient data every 30 seconds
        setInterval(function() {
            if ($('#patientAccordionSection').is(':visible')) {
                console.log('[Realtime] Auto-refreshing patient data...');
            }
        }, 30000);
    }
};

// ========================================
// MAIN INITIALIZATION
// ========================================
window.ReceptionModules.initAll = function() {
    try {
        console.log('[ReceptionModules] Initializing all modules...');
        
        // Initialize all modules - ترتیب جدید برای منشی
        window.ReceptionModules.Accordion.init();
        window.ReceptionModules.Patient.init();
        window.ReceptionModules.Insurance.init();  // اولویت دوم - اطلاعات بیمه
        window.ReceptionModules.Department.init();
        window.ReceptionModules.Service.init();
        window.ReceptionModules.Payment.init();
        window.ReceptionModules.RealTime.init();
        
        window.ReceptionModules.Core.performance.initCount++;
        console.log('[ReceptionModules] All modules initialized successfully');
        
    } catch (error) {
        window.ReceptionModules.Core.errorHandler(error, 'initAll');
    }
};

// ========================================
// SAFE INITIALIZATION
// ========================================
function safeInitialize() {
    if (window.jQuery) {
        jQuery(function() { 
            window.ReceptionModules.initAll(); 
        });
    } else {
        document.addEventListener('DOMContentLoaded', function() {
            if (window.jQuery) {
                window.ReceptionModules.initAll();
            } else {
                console.warn('[ReceptionModules] jQuery not available, retrying...');
                setTimeout(function() {
                    if (window.jQuery) {
                        window.ReceptionModules.initAll();
                    }
                }, 100);
            }
        });
    }
}

// Start safe initialization
safeInitialize();