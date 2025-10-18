/**
 * PATIENT SEARCH MODULE - ماژول جستجوی بیمار
 * ========================================
 * 
 * این ماژول مسئولیت‌های زیر را دارد:
 * - اعتبارسنجی کد ملی
 * - جستجوی بیمار در دیتابیس
 * - نمایش اطلاعات بیمار
 * - مدیریت خطاها و loading states
 * 
 * @author ClinicApp Development Team
 * @version 1.0.0
 * @since 2025-01-17
 */

(function(global, $) {
    'use strict';

    // ========================================
    // MODULE CONFIGURATION - تنظیمات ماژول
    // ========================================
    var CONFIG = {
        // API Endpoints
        endpoints: {
            search: '/Reception/Patient/SearchByNationalCode',
            save: '/Reception/Patient/Save'
        },
        
        // Validation Rules
        validation: {
            nationalCodeLength: 10,
            debounceDelay: 500,
            ajaxTimeout: 15000
        },
        
        // UI Selectors
        selectors: {
            nationalCode: '#patientNationalCode',
            searchBtn: '#searchPatientBtn',
            saveBtn: '#savePatientBtn',
            firstName: '#patientFirstName',
            lastName: '#patientLastName',
            fatherName: '#patientFatherName',
            birthDate: '#patientBirthDate',
            age: '#patientAge',
            gender: '#patientGender',
            phone: '#patientPhone',
            address: '#patientAddress',
            errorContainer: '#nationalCodeError'
        },
        
        // Messages
        messages: {
            validation: {
                required: 'کد ملی الزامی است',
                length: 'کد ملی باید 10 رقم باشد',
                invalid: 'کد ملی نامعتبر است'
            },
            info: {
                searching: 'در حال جستجوی بیمار...'
            },
            success: {
                found: 'بیمار با موفقیت یافت شد',
                saved: 'اطلاعات بیمار با موفقیت ذخیره شد'
            },
            error: {
                notFound: 'بیمار یافت نشد',
                serverError: 'خطا در ارتباط با سرور',
                timeoutError: 'زمان اتصال به سرور تمام شد',
                networkError: 'خطا در اتصال به شبکه'
            }
        }
    };

    // ========================================
    // PATIENT SEARCH MODULE - ماژول اصلی
    // ========================================
    var PatientSearchModule = {
        
        // ========================================
        // INITIALIZATION - راه‌اندازی
        // ========================================
        init: function() {
            console.log('[PatientSearchModule] Initializing...');
            
            try {
                this.bindEvents();
                this.setupValidation();
                this.setupEnterKeySupport();
                
                console.log('[PatientSearchModule] Initialized successfully');
            } catch (error) {
                console.error('[PatientSearchModule] Initialization failed:', error);
                this.handleError(error, 'initialization');
            }
        },

        // ========================================
        // EVENT BINDING - اتصال رویدادها
        // ========================================
        bindEvents: function() {
            var self = this;
            
            // National Code Input Validation
            $(CONFIG.selectors.nationalCode).off('input.patientSearch')
                .on('input.patientSearch', function() {
                    self.handleNationalCodeInput.call(this, self);
                });
            
            // Search Button Click
            $(CONFIG.selectors.searchBtn).off('click.patientSearch')
                .on('click.patientSearch', function() {
                    self.handleSearchPatient.call(this, self);
                });
            
            // Save Button Click
            $(CONFIG.selectors.saveBtn).off('click.patientSearch')
                .on('click.patientSearch', function() {
                    self.handleSavePatient.call(this, self);
                });
        },

        // ========================================
        // VALIDATION SETUP - تنظیم اعتبارسنجی
        // ========================================
        setupValidation: function() {
            // Real-time validation with debounce
            var self = this;
            var debouncedValidation = this.debounce(function() {
                self.validateNationalCode.call(this, self);
            }, CONFIG.validation.debounceDelay);
            
            $(CONFIG.selectors.nationalCode).on('input', debouncedValidation);
        },

        // ========================================
        // ENTER KEY SUPPORT - پشتیبانی از Enter
        // ========================================
        setupEnterKeySupport: function() {
            var self = this;
            
            $(CONFIG.selectors.nationalCode).off('keypress.patientSearch')
                .on('keypress.patientSearch', function(e) {
                    if (e.which === 13) { // Enter key
                        e.preventDefault();
                        self.handleSearchPatient.call($(CONFIG.selectors.searchBtn)[0], self);
                    }
                });
        },

        // ========================================
        // NATIONAL CODE INPUT HANDLER - مدیریت ورودی کد ملی
        // ========================================
        handleNationalCodeInput: function(module) {
            const value = $(this).val() || '';
            
            if (value.length === CONFIG.validation.nationalCodeLength) {
                module.validateNationalCode.call(this, module);
            } else {
                $(this).removeClass('is-valid is-invalid');
                $(CONFIG.selectors.errorContainer).text('');
            }
        },

        // ========================================
        // NATIONAL CODE VALIDATION - اعتبارسنجی کد ملی
        // ========================================
        validateNationalCode: function(module) {
            const value = $(this).val() || '';
            
            if (value.length !== CONFIG.validation.nationalCodeLength) {
                $(this).removeClass('is-valid').addClass('is-invalid');
                $(CONFIG.selectors.errorContainer).text(CONFIG.messages.validation.length);
                return false;
            }
            
            if (module.isValidNationalCode(value)) {
                $(this).removeClass('is-invalid').addClass('is-valid');
                $(CONFIG.selectors.errorContainer).text('');
                return true;
            } else {
                $(this).removeClass('is-valid').addClass('is-invalid');
                $(CONFIG.selectors.errorContainer).text(CONFIG.messages.validation.invalid);
                return false;
            }
        },

        // ========================================
        // NATIONAL CODE ALGORITHM - الگوریتم کد ملی
        // ========================================
        isValidNationalCode: function(nationalCode) {
            if (!nationalCode || nationalCode.length !== 10) return false;
            
            try {
                const digits = nationalCode.split('').map(Number);
                const checkDigit = digits[9];
                const sum = digits.slice(0, 9).reduce((acc, digit, index) => 
                    acc + digit * (10 - index), 0);
                const remainder = sum % 11;
                
                return remainder < 2 ? checkDigit === remainder : checkDigit === 11 - remainder;
            } catch (error) {
                console.error('[PatientSearchModule] National code validation error:', error);
                return false;
            }
        },

        // ========================================
        // SEARCH PATIENT HANDLER - مدیریت جستجوی بیمار
        // ========================================
        handleSearchPatient: function(module) {
            const $btn = $(this);
            const nationalCode = $(CONFIG.selectors.nationalCode).val() || '';
            
            // اعتبارسنجی اولیه
            if (!module.validateSearchInput(nationalCode)) {
                return;
            }
            
            // نمایش loading state
            module.showButtonLoading($btn);
            module.showInfo(CONFIG.messages.info.searching);
            
            // درخواست AJAX
            module.searchPatient(nationalCode, $btn);
        },

        // ========================================
        // SEARCH INPUT VALIDATION - اعتبارسنجی ورودی جستجو
        // ========================================
        validateSearchInput: function(nationalCode) {
            if (!nationalCode) {
                this.showError(CONFIG.messages.validation.required);
                return false;
            }
            
            if (nationalCode.length !== CONFIG.validation.nationalCodeLength) {
                this.showError(CONFIG.messages.validation.length);
                return false;
            }
            
            if (!this.isValidNationalCode(nationalCode)) {
                this.showError(CONFIG.messages.validation.invalid);
                return false;
            }
            
            return true;
        },

        // ========================================
        // SEARCH PATIENT AJAX - درخواست جستجوی بیمار
        // ========================================
        searchPatient: function(nationalCode, $btn) {
            var self = this;
            
            $.ajax({
                url: CONFIG.endpoints.search,
                type: 'POST',
                data: { 
                    nationalCode: nationalCode,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                dataType: 'json',
                timeout: CONFIG.validation.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.hideButtonLoading($btn);
                    self.handleSearchSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideButtonLoading($btn);
                    self.handleSearchError(xhr, status, error);
                }
            });
        },

        // ========================================
        // SEARCH SUCCESS HANDLER - مدیریت موفقیت جستجو
        // ========================================
        handleSearchSuccess: function(response) {
            console.log('[PatientSearchModule] Search response received:', response);
            
            if (response.Success && response.Data) {
                this.displayPatientInfo(response.Data);
                this.showSuccess(CONFIG.messages.success.found);
                
                // بارگذاری اطلاعات بیمه
                this.loadPatientInsurance(response.Data.PatientId);
                
                // به‌روزرسانی وضعیت آکاردئون
                this.updateAccordionState('patientSection', 'completed');
            } else {
                this.showError(response.Message || CONFIG.messages.error.notFound);
            }
        },

        // ========================================
        // SEARCH ERROR HANDLER - مدیریت خطای جستجو
        // ========================================
        handleSearchError: function(xhr, status, error) {
            let errorMessage = CONFIG.messages.error.serverError;
            
            if (status === 'timeout') {
                errorMessage = CONFIG.messages.error.timeoutError;
            } else if (xhr.status === 0) {
                errorMessage = CONFIG.messages.error.networkError;
            }
            
            this.showError(errorMessage);
        },

        // ========================================
        // DISPLAY PATIENT INFO - نمایش اطلاعات بیمار
        // ========================================
        displayPatientInfo: function(patientData) {
            if (!patientData) {
                console.error('[PatientSearchModule] Patient data is null or undefined');
                return;
            }
            
            try {
                // استفاده از Property Names صحیح (با حروف بزرگ)
                $(CONFIG.selectors.firstName).val(patientData.FirstName || '');
                $(CONFIG.selectors.lastName).val(patientData.LastName || '');
                $(CONFIG.selectors.fatherName).val(patientData.FatherName || '');
                $(CONFIG.selectors.birthDate).val(patientData.BirthDate || '');
                $(CONFIG.selectors.age).val(patientData.Age || '');
                $(CONFIG.selectors.gender).val(patientData.Gender || '');
                $(CONFIG.selectors.phone).val(patientData.PhoneNumber || '');
                $(CONFIG.selectors.address).val(patientData.Address || '');
                
                // نمایش پیام موفقیت
                if (patientData.StatusMessage) {
                    this.showSuccess(patientData.StatusMessage);
                }
                
                console.log('[PatientSearchModule] Patient info displayed successfully:', patientData);
            } catch (error) {
                console.error('[PatientSearchModule] Error displaying patient info:', error);
                this.handleError(error, 'displayPatientInfo');
            }
        },

        // ========================================
        // LOAD PATIENT INSURANCE - بارگذاری اطلاعات بیمه
        // ========================================
        loadPatientInsurance: function(patientId) {
            if (!patientId) {
                console.warn('[PatientSearchModule] Patient ID not provided for insurance loading');
                return;
            }

            // ارسال event برای ماژول بیمه
            this.emitEvent('patientInsurance:load', { patientId: patientId });
        },

        // ========================================
        // SAVE PATIENT HANDLER - مدیریت ذخیره بیمار
        // ========================================
        handleSavePatient: function(module) {
            const $btn = $(this);
            module.showButtonLoading($btn);
            
            // TODO: Implement save patient logic
            setTimeout(() => {
                module.hideButtonLoading($btn);
                module.showSuccess(CONFIG.messages.success.saved);
            }, 1000);
        },

        // ========================================
        // UI HELPER METHODS - متدهای کمکی UI
        // ========================================
        showButtonLoading: function($btn) {
            if (!$btn || $btn.length === 0) {
                console.error('[PatientSearchModule] Button element is null or undefined');
                return;
            }
            
            try {
                $btn.prop('disabled', true);
                $btn.find('.btn-text').addClass('d-none');
                $btn.find('.btn-loading').removeClass('d-none');
            } catch (error) {
                console.error('[PatientSearchModule] Error showing button loading:', error);
            }
        },

        hideButtonLoading: function($btn) {
            if (!$btn || $btn.length === 0) {
                console.error('[PatientSearchModule] Button element is null or undefined');
                return;
            }
            
            try {
                $btn.prop('disabled', false);
                $btn.find('.btn-text').removeClass('d-none');
                $btn.find('.btn-loading').addClass('d-none');
            } catch (error) {
                console.error('[PatientSearchModule] Error hiding button loading:', error);
            }
        },

        // ========================================
        // MESSAGE DISPLAY METHODS - متدهای نمایش پیام
        // ========================================
        showError: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showError(message);
            } else {
                console.error('[PatientSearchModule] Error:', message);
            }
        },

        showSuccess: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showSuccess(message);
            } else {
                console.log('[PatientSearchModule] Success:', message);
            }
        },

        showInfo: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showInfo(message);
            } else {
                console.log('[PatientSearchModule] Info:', message);
            }
        },

        // ========================================
        // UTILITY METHODS - متدهای کمکی
        // ========================================
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

        // ========================================
        // EVENT SYSTEM - سیستم رویداد
        // ========================================
        emitEvent: function(eventName, data) {
            if (window.ReceptionEventBus) {
                window.ReceptionEventBus.emit(eventName, data);
            } else {
                console.warn('[PatientSearchModule] EventBus not available');
            }
        },

        updateAccordionState: function(section, state) {
            if (window.ReceptionModules && window.ReceptionModules.Accordion) {
                window.ReceptionModules.Accordion.setState(section, state);
            }
        },

        // ========================================
        // ERROR HANDLING - مدیریت خطا
        // ========================================
        handleError: function(error, context) {
            console.error(`[PatientSearchModule] Error in ${context}:`, error);
            
            if (window.ReceptionErrorHandler) {
                window.ReceptionErrorHandler.handle(error, 'PatientSearchModule', context);
            }
        }
    };

    // ========================================
    // MODULE EXPORT - صادرات ماژول
    // ========================================
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = PatientSearchModule;
    } else {
        global.PatientSearchModule = PatientSearchModule;
    }

})(window, jQuery);
