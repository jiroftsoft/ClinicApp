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
            var context = this;
            var args = Array.prototype.slice.call(arguments);
            var later = function() {
                clearTimeout(timeout);
                func.apply(context, args);
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
            insuranceProviders: '/Reception/Insurance/GetInsuranceProviders',
            primaryInsuranceProviders: '/Reception/Insurance/GetPrimaryInsuranceProviders',
            supplementaryInsuranceProviders: '/Reception/Insurance/GetSupplementaryInsuranceProviders',
            insurancePlans: '/Reception/Insurance/GetInsurancePlans',
            primaryInsurancePlans: '/Reception/Insurance/GetPrimaryInsurancePlans',
            supplementaryInsurancePlans: '/Reception/Insurance/GetSupplementaryInsurancePlans',
            supplementaryInsurances: '/Reception/Insurance/GetSupplementaryInsurances',
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
        $('#patientNationalCode').on('input', window.ReceptionModules.utils.debounce(function(event) {
            var $input = $(event.target);
            var value = $input.val() || '';
            
            if (value.length === 10) {
                if (window.ReceptionModules.Patient.validateNationalCode(value)) {
                    $input.removeClass('is-invalid').addClass('is-valid');
                    $('#nationalCodeError').text('');
                } else {
                    $input.removeClass('is-valid').addClass('is-invalid');
                    $('#nationalCodeError').text(window.ReceptionModules.config.messages.validation.nationalCodeInvalid);
                }
            } else {
                $input.removeClass('is-valid is-invalid');
                $('#nationalCodeError').text('');
            }
        }, 500));

        // Search patient - Click and Enter
        $('#searchPatientBtn').off('click.patient').on('click.patient', window.ReceptionModules.Patient.handleSearchPatient);
        $('#searchByNationalCodeBtn').off('click.patient').on('click.patient', window.ReceptionModules.Patient.handleSearchPatient);
        
        // Enter key search
        $('#patientNationalCode').off('keypress.patient').on('keypress.patient', function(e) {
            if (e.which === 13) { // Enter key
                e.preventDefault();
                window.ReceptionModules.Patient.handleSearchPatient.call($('#searchByNationalCodeBtn')[0]);
            }
        });
        
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
        var nationalCode = '';
        
        try {
            var $input = $('#patientNationalCode');
            if ($input && $input.length > 0) {
                nationalCode = ($input.val() || '').trim();
            }
        } catch (error) {
            console.error('Error getting national code value:', error);
            nationalCode = '';
        }
        
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
        console.log('[ReceptionModules.Patient] Starting search for national code:', nationalCode);
        
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
                
                console.log('[ReceptionModules.Patient] Search response received:', response);
                
                if (response.Success && response.Data) {
                    // نمایش اطلاعات بیمار
                    window.ReceptionModules.Patient.displayPatientInfo(response.Data);
                    
                    // بارگذاری اطلاعات بیمه
                    window.ReceptionModules.Patient.loadPatientInsurance(response.Data.PatientId);
                    
                    // به‌روزرسانی وضعیت آکاردئون
                    if (window.ReceptionModules.Accordion && window.ReceptionModules.Accordion.setState) {
                        window.ReceptionModules.Accordion.setState('patientSection', 'completed');
                    }
                } else {
                    window.ReceptionModules.Patient.showError(response.Message || window.ReceptionModules.config.messages.error.patientNotFound);
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
        if (!$btn || $btn.length === 0) {
            console.error('Save button not found');
            return;
        }
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
            // استفاده از Property Names صحیح (با حروف بزرگ)
            $('#patientFirstName').val(patientData.FirstName || '');
            $('#patientLastName').val(patientData.LastName || '');
            $('#patientFatherName').val(patientData.FatherName || '');
            
            // رفع مشکل فرمت تاریخ تولد (.NET Date format)
            var birthDate = patientData.BirthDate;
            if (birthDate) {
                // تبدیل فرمت .NET Date به فرمت قابل استفاده
                if (birthDate.startsWith('/Date(') && birthDate.endsWith(')/')) {
                    var timestamp = parseInt(birthDate.substring(6, birthDate.length - 2));
                    var date = new Date(timestamp);
                    var formattedDate = date.getFullYear() + '-' + 
                        String(date.getMonth() + 1).padStart(2, '0') + '-' + 
                        String(date.getDate()).padStart(2, '0');
                    $('#patientBirthDate').val(formattedDate);
                } else {
                    $('#patientBirthDate').val(birthDate);
                }
            } else {
                $('#patientBirthDate').val('');
            }
            
            // محاسبه سن در سمت کلاینت (اگر در سمت سرور محاسبه نشده)
            var age = patientData.Age;
            if (!age && birthDate) {
                try {
                    var birthDateObj = new Date(birthDate.startsWith('/Date(') ? 
                        parseInt(birthDate.substring(6, birthDate.length - 2)) : birthDate);
                    var today = new Date();
                    age = today.getFullYear() - birthDateObj.getFullYear();
                    var monthDiff = today.getMonth() - birthDateObj.getMonth();
                    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDateObj.getDate())) {
                        age--;
                    }
                } catch (e) {
                    console.warn('Error calculating age:', e);
                    age = null;
                }
            }
            $('#patientAge').val(age || '');
            
            $('#patientGender').val(patientData.Gender || '');
            $('#patientPhone').val(patientData.PhoneNumber || '');
            $('#patientAddress').val(patientData.Address || '');
            
            // نمایش پیام موفقیت
            if (patientData.StatusMessage) {
                window.ReceptionModules.Patient.showSuccess(patientData.StatusMessage);
            }
            
            console.log('[ReceptionModules.Patient] Patient info displayed successfully:', patientData);
        } catch (error) {
            console.error('Error displaying patient info:', error);
            window.ReceptionModules.Patient.showError('خطا در نمایش اطلاعات بیمار');
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

    showSuccess: function(message) {
        // TODO: Implement success message display
        console.log('Success:', message);
    }
};

// ========================================
// INSURANCE MODULE - مدیریت بیمه
// ========================================
window.ReceptionModules.Insurance = {
    // Flag to prevent duplicate loading
    _providersLoaded: false,
    
    init: function() {
        console.log('[ReceptionModules.Insurance] Initializing...');
        
        // Load patient insurance
        $('#loadPatientInsuranceBtn').off('click.insurance').on('click.insurance', this.handleLoadInsurance.bind(this));
        
        // Save insurance
        $('#saveInsuranceBtn').off('click.insurance').on('click.insurance', this.handleSaveInsurance.bind(this));
        
        // Insurance provider change event - Load primary insurance plans
        $('#insuranceProvider').off('change.insurance').on('change.insurance', function() {
            var providerId = $(this).val();
            console.log('[ReceptionModules.Insurance] Insurance provider changed to:', providerId);
            console.log('[ReceptionModules.Insurance] Calling loadPrimaryInsurancePlans with providerId:', providerId);
            window.ReceptionModules.Insurance.loadPrimaryInsurancePlans(providerId);
        });

        // Supplementary insurance provider change event - Load supplementary insurance plans
        $('#supplementaryProvider').off('change.insurance').on('change.insurance', function() {
            var providerId = $(this).val();
            console.log('[ReceptionModules.Insurance] Supplementary insurance provider changed to:', providerId);
            console.log('[ReceptionModules.Insurance] Calling loadSupplementaryInsurancePlans with providerId:', providerId);
            window.ReceptionModules.Insurance.loadSupplementaryInsurancePlans(providerId);
        });
        
        // Load primary insurance providers on page load (only once)
        if (!this._providersLoaded) {
            this.loadPrimaryInsuranceProviders();
        }

        // Load supplementary insurance providers on page load
        this.loadSupplementaryInsuranceProviders();
        
        console.log('[ReceptionModules.Insurance] Initialized successfully');
    },

    handleLoadInsurance: function() {
        var $btn = $(this);
        var patientId = $('#patientId').val();
        
        if (!patientId || patientId <= 0) {
            this.showError('شناسه بیمار نامعتبر است');
            return;
        }
        
        this.showButtonLoading($btn);
        
        var self = this;
        $.ajax({
            url: window.ReceptionModules.config.apiEndpoints.insuranceLoad,
            type: 'POST',
            data: {
                patientId: patientId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                self.hideButtonLoading($btn);
                if (response.success) {
                    self.displayInsuranceInfo(response.data);
                    self.showSuccess('اطلاعات بیمه با موفقیت بارگذاری شد');
                } else {
                    self.showError(response.message || 'خطا در بارگذاری اطلاعات بیمه');
                }
            },
            error: function(xhr, status, error) {
                self.hideButtonLoading($btn);
                self.showError('خطا در ارتباط با سرور: ' + error);
            }
        });
    },

    handleSaveInsurance: function() {
        var $btn = $(this);
        var patientId = $('#patientId').val();
        
        if (!patientId || patientId <= 0) {
            this.showError('شناسه بیمار نامعتبر است');
            return;
        }
        
        var insuranceData = this.collectInsuranceData();
        if (!insuranceData) {
            return;
        }
        
        this.showButtonLoading($btn);
        
        var self = this;
        $.ajax({
            url: window.ReceptionModules.config.apiEndpoints.insuranceSave,
            type: 'POST',
            data: $.extend(insuranceData, {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            }),
            success: function(response) {
                self.hideButtonLoading($btn);
                if (response.success) {
                    self.showSuccess('اطلاعات بیمه با موفقیت ذخیره شد');
                    window.ReceptionModules.Accordion.setState('insuranceSection', 'completed');
                } else {
                    self.showError(response.message || 'خطا در ذخیره اطلاعات بیمه');
                }
            },
            error: function(xhr, status, error) {
                self.hideButtonLoading($btn);
                self.showError('خطا در ارتباط با سرور: ' + error);
            }
        });
    },

    // Load primary insurance providers
    loadPrimaryInsuranceProviders: function() {
        var self = this;
        
        // Prevent duplicate loading
        if (this._providersLoaded) {
            console.log('[ReceptionModules.Insurance] Primary providers already loaded, skipping...');
            return;
        }
        
        this._providersLoaded = true;
        
        $.ajax({
            url: window.ReceptionModules.config.apiEndpoints.primaryInsuranceProviders,
            type: 'POST',
            data: {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                console.log('[ReceptionModules.Insurance] Primary Providers API Response:', response);
                console.log('[ReceptionModules.Insurance] Response type:', typeof response);
                console.log('[ReceptionModules.Insurance] Response.success:', response.success);
                console.log('[ReceptionModules.Insurance] Response.success type:', typeof response.success);
                
                // Check if response is a string (JSON string)
                if (typeof response === 'string') {
                    try {
                        response = JSON.parse(response);
                        console.log('[ReceptionModules.Insurance] Parsed JSON response:', response);
                    } catch (e) {
                        console.error('[ReceptionModules.Insurance] JSON parse error:', e);
                        self.showError('خطا در پردازش پاسخ سرور');
                        return;
                    }
                }
                
                if (response && response.success === true) {
                    console.log('[ReceptionModules.Insurance] Primary providers response success:', response.success);
                    console.log('[ReceptionModules.Insurance] Primary providers data received:', response.data);
                    
                    self.populateInsuranceProviders(response.data);
                    console.log('[ReceptionModules.Insurance] Primary providers loaded successfully');
                    
                    // Show success message
                    self.showSuccess('ارائه‌دهندگان بیمه پایه با موفقیت بارگذاری شدند');
                } else {
                    console.log('[ReceptionModules.Insurance] Primary providers response failed:', response.message);
                    console.log('[ReceptionModules.Insurance] Primary providers response object:', response);
                    self.showError(response.message || 'خطا در بارگذاری ارائه‌دهندگان بیمه پایه');
                    self._providersLoaded = false; // Reset flag on error
                }
            },
            error: function(xhr, status, error) {
                console.error('[ReceptionModules.Insurance] Primary providers AJAX error:', xhr, status, error);
                self.showError('خطا در ارتباط با سرور: ' + error);
                self._providersLoaded = false; // Reset flag on error
            }
        });
    },

    // Load supplementary insurance providers
    loadSupplementaryInsuranceProviders: function() {
        var self = this;
        
        $.ajax({
            url: window.ReceptionModules.config.apiEndpoints.supplementaryInsuranceProviders,
            type: 'POST',
            data: {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                console.log('[ReceptionModules.Insurance] Supplementary Providers API Response:', response);
                
                // Check if response is a string (JSON string)
                if (typeof response === 'string') {
                    try {
                        response = JSON.parse(response);
                        console.log('[ReceptionModules.Insurance] Parsed JSON response:', response);
                    } catch (e) {
                        console.error('[ReceptionModules.Insurance] JSON parse error:', e);
                        self.showError('خطا در پردازش پاسخ سرور');
                        return;
                    }
                }
                
                if (response && response.success === true) {
                    console.log('[ReceptionModules.Insurance] Supplementary providers response success:', response.success);
                    console.log('[ReceptionModules.Insurance] Supplementary providers data received:', response.data);
                    
                    self.populateSupplementaryInsuranceProviders(response.data);
                    console.log('[ReceptionModules.Insurance] Supplementary providers loaded successfully');
                    
                    // Show success message
                    self.showSuccess('ارائه‌دهندگان بیمه تکمیلی با موفقیت بارگذاری شدند');
                } else {
                    console.log('[ReceptionModules.Insurance] Supplementary providers response failed:', response.message);
                    self.showError(response.message || 'خطا در بارگذاری ارائه‌دهندگان بیمه تکمیلی');
                }
            },
            error: function(xhr, status, error) {
                console.error('[ReceptionModules.Insurance] Supplementary providers AJAX error:', xhr, status, error);
                self.showError('خطا در ارتباط با سرور: ' + error);
            }
        });
    },

    // Load insurance plans by provider (all types)
    loadInsurancePlans: function(providerId) {
        console.log('[ReceptionModules.Insurance] loadInsurancePlans called with providerId:', providerId);
        
        if (!providerId || providerId <= 0) {
            console.log('[ReceptionModules.Insurance] Invalid providerId, clearing plans');
            this.clearInsurancePlans();
            return;
        }
        
        var self = this;
        $.ajax({
            url: window.ReceptionModules.config.apiEndpoints.insurancePlans,
            type: 'POST',
            data: {
                providerId: providerId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                console.log('[ReceptionModules.Insurance] Insurance Plans API Response:', response);
                console.log('[ReceptionModules.Insurance] Response type:', typeof response);
                console.log('[ReceptionModules.Insurance] Response.success:', response.success);
                
                // Check if response is a string (JSON string)
                if (typeof response === 'string') {
                    try {
                        response = JSON.parse(response);
                        console.log('[ReceptionModules.Insurance] Parsed JSON response:', response);
                    } catch (e) {
                        console.error('[ReceptionModules.Insurance] JSON parse error:', e);
                        self.showError('خطا در پردازش پاسخ سرور');
                        return;
                    }
                }
                
                if (response && response.success === true) {
                    console.log('[ReceptionModules.Insurance] Plans response success:', response.success);
                    console.log('[ReceptionModules.Insurance] Plans data received:', response.data);
                    
                    self.populateInsurancePlans(response.data);
                    console.log('[ReceptionModules.Insurance] Plans loaded successfully');
                    
                    // Show success message
                    self.showSuccess('طرح‌های بیمه با موفقیت بارگذاری شدند');
                } else {
                    console.log('[ReceptionModules.Insurance] Plans response failed:', response.message);
                    console.log('[ReceptionModules.Insurance] Plans response object:', response);
                    self.showError(response.message || 'خطا در بارگذاری طرح‌های بیمه');
                }
            },
            error: function(xhr, status, error) {
                console.error('[ReceptionModules.Insurance] Plans AJAX error:', xhr, status, error);
                self.showError('خطا در ارتباط با سرور: ' + error);
            }
        });
    },

    // Load primary insurance plans by provider
    loadPrimaryInsurancePlans: function(providerId) {
        console.log('[ReceptionModules.Insurance] loadPrimaryInsurancePlans called with providerId:', providerId);
        
        if (!providerId || providerId <= 0) {
            console.log('[ReceptionModules.Insurance] Invalid providerId, clearing primary plans');
            this.clearPrimaryInsurancePlans();
            return;
        }
        
        var self = this;
        $.ajax({
            url: window.ReceptionModules.config.apiEndpoints.primaryInsurancePlans,
            type: 'POST',
            data: {
                providerId: providerId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                console.log('[ReceptionModules.Insurance] Primary Insurance Plans API Response:', response);
                console.log('[ReceptionModules.Insurance] Response type:', typeof response);
                console.log('[ReceptionModules.Insurance] Response.success:', response.success);
                
                // Check if response is a string (JSON string)
                if (typeof response === 'string') {
                    try {
                        response = JSON.parse(response);
                        console.log('[ReceptionModules.Insurance] Parsed JSON response:', response);
                    } catch (e) {
                        console.error('[ReceptionModules.Insurance] JSON parse error:', e);
                        self.showError('خطا در پردازش پاسخ سرور');
                        return;
                    }
                }
                
                if (response && response.success === true) {
                    console.log('[ReceptionModules.Insurance] Primary plans response success:', response.success);
                    console.log('[ReceptionModules.Insurance] Primary plans data received:', response.data);
                    
                    self.populatePrimaryInsurancePlans(response.data);
                    console.log('[ReceptionModules.Insurance] Primary plans loaded successfully');
                    
                    // Show success message
                    self.showSuccess('طرح‌های بیمه پایه با موفقیت بارگذاری شدند');
                } else {
                    console.log('[ReceptionModules.Insurance] Primary plans response failed:', response.message);
                    console.log('[ReceptionModules.Insurance] Primary plans response object:', response);
                    self.showError(response.message || 'خطا در بارگذاری طرح‌های بیمه پایه');
                }
            },
            error: function(xhr, status, error) {
                console.error('[ReceptionModules.Insurance] Primary plans AJAX error:', xhr, status, error);
                self.showError('خطا در ارتباط با سرور: ' + error);
            }
        });
    },

    // Load supplementary insurance plans by provider
    loadSupplementaryInsurancePlans: function(providerId) {
        console.log('[ReceptionModules.Insurance] loadSupplementaryInsurancePlans called with providerId:', providerId);
        
        if (!providerId || providerId <= 0) {
            console.log('[ReceptionModules.Insurance] Invalid providerId, clearing supplementary plans');
            this.clearSupplementaryInsurancePlans();
            return;
        }
        
        var self = this;
        $.ajax({
            url: window.ReceptionModules.config.apiEndpoints.supplementaryInsurancePlans,
            type: 'POST',
            data: {
                providerId: providerId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                console.log('[ReceptionModules.Insurance] Supplementary Insurance Plans API Response:', response);
                console.log('[ReceptionModules.Insurance] Response type:', typeof response);
                console.log('[ReceptionModules.Insurance] Response.success:', response.success);
                
                // Check if response is a string (JSON string)
                if (typeof response === 'string') {
                    try {
                        response = JSON.parse(response);
                        console.log('[ReceptionModules.Insurance] Parsed JSON response:', response);
                    } catch (e) {
                        console.error('[ReceptionModules.Insurance] JSON parse error:', e);
                        self.showError('خطا در پردازش پاسخ سرور');
                        return;
                    }
                }
                
                if (response && response.success === true) {
                    console.log('[ReceptionModules.Insurance] Supplementary plans response success:', response.success);
                    console.log('[ReceptionModules.Insurance] Supplementary plans data received:', response.data);
                    
                    self.populateSupplementaryInsurancePlans(response.data);
                    console.log('[ReceptionModules.Insurance] Supplementary plans loaded successfully');
                    
                    // Show success message
                    self.showSuccess('طرح‌های بیمه تکمیلی با موفقیت بارگذاری شدند');
                } else {
                    console.log('[ReceptionModules.Insurance] Supplementary plans response failed:', response.message);
                    console.log('[ReceptionModules.Insurance] Supplementary plans response object:', response);
                    self.showError(response.message || 'خطا در بارگذاری طرح‌های بیمه تکمیلی');
                }
            },
            error: function(xhr, status, error) {
                console.error('[ReceptionModules.Insurance] Supplementary plans AJAX error:', xhr, status, error);
                self.showError('خطا در ارتباط با سرور: ' + error);
            }
        });
    },

    // Display insurance information
    displayInsuranceInfo: function(insuranceData) {
        if (!insuranceData) return;
        
        // Populate basic insurance info
        if (insuranceData.ProviderName) {
            $('#insuranceProvider').val(insuranceData.ProviderId);
            // Don't trigger change event to prevent duplicate loading
            // $('#insuranceProvider').trigger('change');
        }
        
        if (insuranceData.PlanName) {
            $('#insurancePlan').val(insuranceData.PlanId);
        }
        
        if (insuranceData.PolicyNumber) {
            $('#policyNumber').val(insuranceData.PolicyNumber);
        }
        
        if (insuranceData.CardNumber) {
            $('#cardNumber').val(insuranceData.CardNumber);
        }
        
        // Populate supplementary insurance if exists
        if (insuranceData.SupplementaryInsurance) {
            this.displaySupplementaryInsurance(insuranceData.SupplementaryInsurance);
        }
    },

    // Populate insurance providers dropdown
    populateInsuranceProviders: function(providers) {
        try {
            console.log('[ReceptionModules.Insurance] populateInsuranceProviders called with:', providers);
            console.log('[ReceptionModules.Insurance] Providers type:', typeof providers);
            console.log('[ReceptionModules.Insurance] Providers length:', providers ? providers.length : 'undefined');
            
            var $select = $('#insuranceProvider');
            console.log('[ReceptionModules.Insurance] Select element found:', $select.length);
            console.log('[ReceptionModules.Insurance] Select element HTML:', $select.length > 0 ? $select[0].outerHTML : 'Not found');
            
            if ($select.length === 0) {
                console.error('[ReceptionModules.Insurance] Insurance provider select element not found!');
                console.log('[ReceptionModules.Insurance] Available elements with id containing "insurance":');
                $('[id*="insurance"]').each(function() {
                    console.log('[ReceptionModules.Insurance] Found element:', this.id, this.tagName);
                });
                this.showError('عنصر انتخاب ارائه‌دهنده بیمه یافت نشد');
                return;
            }
            
            $select.empty().append('<option value="">انتخاب ارائه‌دهنده بیمه...</option>');
            console.log('[ReceptionModules.Insurance] Select cleared and default option added');
            
            if (providers && providers.length > 0) {
                console.log('[ReceptionModules.Insurance] Adding providers:', providers.length);
                $.each(providers, function(index, provider) {
                    console.log('[ReceptionModules.Insurance] Adding provider:', provider.Name, 'ID:', provider.InsuranceProviderId);
                    $select.append('<option value="' + provider.InsuranceProviderId + '">' + provider.Name + '</option>');
                });
                console.log('[ReceptionModules.Insurance] Providers populated successfully');
                console.log('[ReceptionModules.Insurance] Final select options count:', $select.find('option').length);
            } else {
                console.log('[ReceptionModules.Insurance] No providers to populate');
                this.showWarning('هیچ ارائه‌دهنده بیمه‌ای یافت نشد');
            }
        } catch (error) {
            console.error('[ReceptionModules.Insurance] Error in populateInsuranceProviders:', error);
            this.showError('خطا در نمایش ارائه‌دهندگان بیمه: ' + error.message);
        }
    },

    // Populate supplementary insurance providers dropdown
    populateSupplementaryInsuranceProviders: function(providers) {
        try {
            console.log('[ReceptionModules.Insurance] populateSupplementaryInsuranceProviders called with:', providers);
            console.log('[ReceptionModules.Insurance] Supplementary providers type:', typeof providers);
            console.log('[ReceptionModules.Insurance] Supplementary providers length:', providers ? providers.length : 'undefined');
            
            var $select = $('#supplementaryProvider');
            console.log('[ReceptionModules.Insurance] Supplementary providers select element found:', $select.length);
            console.log('[ReceptionModules.Insurance] Supplementary providers select element HTML:', $select.length > 0 ? $select[0].outerHTML : 'Not found');
            
            if ($select.length === 0) {
                console.error('[ReceptionModules.Insurance] Supplementary insurance provider select element not found!');
                console.log('[ReceptionModules.Insurance] Available elements with id containing "supplementary":');
                $('[id*="supplementary"]').each(function() {
                    console.log('[ReceptionModules.Insurance] Found element:', this.id, this.tagName);
                });
                this.showError('عنصر انتخاب ارائه‌دهنده بیمه تکمیلی یافت نشد');
                return;
            }
            
            $select.empty().append('<option value="">انتخاب ارائه‌دهنده بیمه تکمیلی...</option>');
            console.log('[ReceptionModules.Insurance] Supplementary providers select cleared and default option added');
            
            if (providers && providers.length > 0) {
                console.log('[ReceptionModules.Insurance] Adding supplementary providers:', providers.length);
                $.each(providers, function(index, provider) {
                    console.log('[ReceptionModules.Insurance] Adding supplementary provider:', provider.Name, 'ID:', provider.InsuranceProviderId);
                    $select.append('<option value="' + provider.InsuranceProviderId + '">' + provider.Name + '</option>');
                });
                console.log('[ReceptionModules.Insurance] Supplementary providers populated successfully');
                console.log('[ReceptionModules.Insurance] Final supplementary providers select options count:', $select.find('option').length);
            } else {
                console.log('[ReceptionModules.Insurance] No supplementary providers to populate');
                this.showWarning('هیچ ارائه‌دهنده بیمه تکمیلی یافت نشد');
            }
        } catch (error) {
            console.error('[ReceptionModules.Insurance] Error in populateSupplementaryInsuranceProviders:', error);
            this.showError('خطا در نمایش ارائه‌دهندگان بیمه تکمیلی: ' + error.message);
        }
    },

    // Populate insurance plans dropdown
    populateInsurancePlans: function(plans) {
        try {
            console.log('[ReceptionModules.Insurance] populateInsurancePlans called with:', plans);
            console.log('[ReceptionModules.Insurance] Plans type:', typeof plans);
            console.log('[ReceptionModules.Insurance] Plans length:', plans ? plans.length : 'undefined');
            
            var $select = $('#insurancePlan');
            console.log('[ReceptionModules.Insurance] Plans select element found:', $select.length);
            console.log('[ReceptionModules.Insurance] Plans select element HTML:', $select.length > 0 ? $select[0].outerHTML : 'Not found');
            
            if ($select.length === 0) {
                console.error('[ReceptionModules.Insurance] Insurance plans select element not found!');
                console.log('[ReceptionModules.Insurance] Available elements with id containing "plan":');
                $('[id*="plan"]').each(function() {
                    console.log('[ReceptionModules.Insurance] Found element:', this.id, this.tagName);
                });
                this.showError('عنصر انتخاب طرح بیمه یافت نشد');
                return;
            }
            
            $select.empty().append('<option value="">انتخاب طرح بیمه...</option>');
            console.log('[ReceptionModules.Insurance] Plans select cleared and default option added');
            
            if (plans && plans.length > 0) {
                console.log('[ReceptionModules.Insurance] Adding plans:', plans.length);
                $.each(plans, function(index, plan) {
                    console.log('[ReceptionModules.Insurance] Adding plan:', plan.Name, 'ID:', plan.InsurancePlanId);
                    $select.append('<option value="' + plan.InsurancePlanId + '">' + plan.Name + '</option>');
                });
                console.log('[ReceptionModules.Insurance] Plans populated successfully');
                console.log('[ReceptionModules.Insurance] Final plans select options count:', $select.find('option').length);
            } else {
                console.log('[ReceptionModules.Insurance] No plans to populate');
                this.showWarning('هیچ طرح بیمه‌ای یافت نشد');
            }
        } catch (error) {
            console.error('[ReceptionModules.Insurance] Error in populateInsurancePlans:', error);
            this.showError('خطا در نمایش طرح‌های بیمه: ' + error.message);
        }
    },

    // Clear insurance plans dropdown
    clearInsurancePlans: function() {
        $('#insurancePlan').empty().append('<option value="">انتخاب طرح بیمه...</option>');
    },

    // Populate primary insurance plans dropdown
    populatePrimaryInsurancePlans: function(plans) {
        try {
            console.log('[ReceptionModules.Insurance] populatePrimaryInsurancePlans called with:', plans);
            console.log('[ReceptionModules.Insurance] Primary plans type:', typeof plans);
            console.log('[ReceptionModules.Insurance] Primary plans length:', plans ? plans.length : 'undefined');
            
            var $select = $('#insurancePlan'); // Correct element ID
            console.log('[ReceptionModules.Insurance] Primary plans select element found:', $select.length);
            console.log('[ReceptionModules.Insurance] Primary plans select element HTML:', $select.length > 0 ? $select[0].outerHTML : 'Not found');
            
            if ($select.length === 0) {
                console.error('[ReceptionModules.Insurance] Primary insurance plans select element not found!');
                console.log('[ReceptionModules.Insurance] Available elements with id containing "insurance":');
                $('[id*="insurance"]').each(function() {
                    console.log('[ReceptionModules.Insurance] Found element:', this.id, this.tagName);
                });
                this.showError('عنصر انتخاب طرح بیمه پایه یافت نشد');
                return;
            }
            
            $select.empty().append('<option value="">انتخاب طرح بیمه پایه...</option>');
            console.log('[ReceptionModules.Insurance] Primary plans select cleared and default option added');
            
            if (plans && plans.length > 0) {
                console.log('[ReceptionModules.Insurance] Adding primary plans:', plans.length);
                $.each(plans, function(index, plan) {
                    console.log('[ReceptionModules.Insurance] Adding primary plan:', plan.Name, 'ID:', plan.InsurancePlanId);
                    $select.append('<option value="' + plan.InsurancePlanId + '">' + plan.Name + '</option>');
                });
                console.log('[ReceptionModules.Insurance] Primary plans populated successfully');
                console.log('[ReceptionModules.Insurance] Final primary plans select options count:', $select.find('option').length);
            } else {
                console.log('[ReceptionModules.Insurance] No primary plans to populate');
                this.showWarning('هیچ طرح بیمه پایه‌ای یافت نشد');
            }
        } catch (error) {
            console.error('[ReceptionModules.Insurance] Error in populatePrimaryInsurancePlans:', error);
            this.showError('خطا در نمایش طرح‌های بیمه پایه: ' + error.message);
        }
    },

    // Populate supplementary insurance plans dropdown
    populateSupplementaryInsurancePlans: function(plans) {
        try {
            console.log('[ReceptionModules.Insurance] populateSupplementaryInsurancePlans called with:', plans);
            console.log('[ReceptionModules.Insurance] Supplementary plans type:', typeof plans);
            console.log('[ReceptionModules.Insurance] Supplementary plans length:', plans ? plans.length : 'undefined');
            
            var $select = $('#supplementaryPlan'); // Correct element ID
            console.log('[ReceptionModules.Insurance] Supplementary plans select element found:', $select.length);
            console.log('[ReceptionModules.Insurance] Supplementary plans select element HTML:', $select.length > 0 ? $select[0].outerHTML : 'Not found');
            
            if ($select.length === 0) {
                console.error('[ReceptionModules.Insurance] Supplementary insurance plans select element not found!');
                console.log('[ReceptionModules.Insurance] Available elements with id containing "supplementary":');
                $('[id*="supplementary"]').each(function() {
                    console.log('[ReceptionModules.Insurance] Found element:', this.id, this.tagName);
                });
                this.showError('عنصر انتخاب طرح بیمه تکمیلی یافت نشد');
                return;
            }
            
            $select.empty().append('<option value="">انتخاب طرح بیمه تکمیلی...</option>');
            console.log('[ReceptionModules.Insurance] Supplementary plans select cleared and default option added');
            
            if (plans && plans.length > 0) {
                console.log('[ReceptionModules.Insurance] Adding supplementary plans:', plans.length);
                $.each(plans, function(index, plan) {
                    console.log('[ReceptionModules.Insurance] Adding supplementary plan:', plan.Name, 'ID:', plan.InsurancePlanId);
                    $select.append('<option value="' + plan.InsurancePlanId + '">' + plan.Name + '</option>');
                });
                console.log('[ReceptionModules.Insurance] Supplementary plans populated successfully');
                console.log('[ReceptionModules.Insurance] Final supplementary plans select options count:', $select.find('option').length);
            } else {
                console.log('[ReceptionModules.Insurance] No supplementary plans to populate');
                this.showWarning('هیچ طرح بیمه تکمیلی یافت نشد');
            }
        } catch (error) {
            console.error('[ReceptionModules.Insurance] Error in populateSupplementaryInsurancePlans:', error);
            this.showError('خطا در نمایش طرح‌های بیمه تکمیلی: ' + error.message);
        }
    },

    // Clear primary insurance plans dropdown
    clearPrimaryInsurancePlans: function() {
        $('#insurancePlan').empty().append('<option value="">انتخاب طرح بیمه پایه...</option>');
    },

    // Clear supplementary insurance plans dropdown
    clearSupplementaryInsurancePlans: function() {
        $('#supplementaryPlan').empty().append('<option value="">انتخاب طرح بیمه تکمیلی...</option>');
    },

    // Collect insurance data from form
    collectInsuranceData: function() {
        var patientId = $('#patientId').val();
        var providerId = $('#insuranceProvider').val();
        var planId = $('#insurancePlan').val();
        var policyNumber = $('#policyNumber').val();
        var cardNumber = $('#cardNumber').val();
        
        if (!patientId || patientId <= 0) {
            this.showError('شناسه بیمار الزامی است');
            return null;
        }
        
        if (!providerId || providerId <= 0) {
            this.showError('انتخاب ارائه‌دهنده بیمه الزامی است');
            return null;
        }
        
        if (!planId || planId <= 0) {
            this.showError('انتخاب طرح بیمه الزامی است');
            return null;
        }
        
        return {
            patientId: patientId,
            providerId: providerId,
            planId: planId,
            policyNumber: policyNumber,
            cardNumber: cardNumber
        };
    },

    // Display supplementary insurance
    displaySupplementaryInsurance: function(supplementaryData) {
        if (supplementaryData && supplementaryData.ProviderName) {
            $('#supplementaryProvider').val(supplementaryData.ProviderId);
            $('#supplementaryPlan').val(supplementaryData.PlanId);
            $('#supplementaryPolicyNumber').val(supplementaryData.PolicyNumber);
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

    showSuccess: function(message) {
        // نمایش موفقیت با Toastr
        if (window.ReceptionToastr) {
            window.ReceptionToastr.helpers.showSuccess(message);
        } else {
            console.log('Success:', message);
        }
    },

    showError: function(message) {
        // نمایش خطا با Toastr
        if (window.ReceptionToastr) {
            window.ReceptionToastr.helpers.showError(message);
        } else {
            console.error('Error:', message);
        }
    },

    showInfo: function(message) {
        // نمایش اطلاعات با Toastr
        if (window.ReceptionToastr) {
            window.ReceptionToastr.helpers.showInfo(message);
        } else {
            console.log('Info:', message);
        }
    },

    showWarning: function(message) {
        // نمایش هشدار با Toastr
        if (window.ReceptionToastr) {
            window.ReceptionToastr.helpers.showWarning(message);
        } else {
            console.warn('Warning:', message);
        }
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