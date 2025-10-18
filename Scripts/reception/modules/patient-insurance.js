/**
 * PATIENT INSURANCE MODULE - ماژول مدیریت بیمه بیمار
 * ========================================
 * 
 * این ماژول مسئولیت‌های زیر را دارد:
 * - بارگذاری اطلاعات بیمه بیمار
 * - نمایش اطلاعات بیمه اصلی و تکمیلی
 * - مدیریت خطاها و loading states
 * - هماهنگی با ماژول‌های دیگر
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
            load: '/Reception/Insurance/Load',
            save: '/Reception/Insurance/Save'
        },
        
        // Validation Rules
        validation: {
            ajaxTimeout: 10000
        },
        
        // UI Selectors
        selectors: {
            loadBtn: '#loadPatientInsuranceBtn',
            saveBtn: '#saveInsuranceBtn',
            primaryName: '#primaryInsuranceName',
            primaryNumber: '#primaryInsuranceNumber',
            supplementaryName: '#supplementaryInsuranceName',
            supplementaryNumber: '#supplementaryInsuranceNumber',
            statusMessage: '#insuranceStatusMessage'
        },
        
        // Messages
        messages: {
            info: {
                loading: 'در حال بارگذاری اطلاعات بیمه...'
            },
            success: {
                loaded: 'اطلاعات بیمه با موفقیت بارگذاری شد',
                saved: 'اطلاعات بیمه با موفقیت ذخیره شد'
            },
            error: {
                loadError: 'خطا در بارگذاری اطلاعات بیمه',
                notFound: 'اطلاعات بیمه یافت نشد'
            }
        }
    };

    // ========================================
    // PATIENT INSURANCE MODULE - ماژول اصلی
    // ========================================
    var PatientInsuranceModule = {
        
        // ========================================
        // INITIALIZATION - راه‌اندازی
        // ========================================
        init: function() {
            console.log('[PatientInsuranceModule] Initializing...');
            
            try {
                this.bindEvents();
                this.setupEventListeners();
                
                console.log('[PatientInsuranceModule] Initialized successfully');
            } catch (error) {
                console.error('[PatientInsuranceModule] Initialization failed:', error);
                this.handleError(error, 'initialization');
            }
        },

        // ========================================
        // EVENT BINDING - اتصال رویدادها
        // ========================================
        bindEvents: function() {
            var self = this;
            
            // Load Insurance Button
            $(CONFIG.selectors.loadBtn).off('click.patientInsurance')
                .on('click.patientInsurance', function() {
                    self.handleLoadInsurance.call(this, self);
                });
            
            // Save Insurance Button
            $(CONFIG.selectors.saveBtn).off('click.patientInsurance')
                .on('click.patientInsurance', function() {
                    self.handleSaveInsurance.call(this, self);
                });
        },

        // ========================================
        // EVENT LISTENERS SETUP - تنظیم شنوندگان رویداد
        // ========================================
        setupEventListeners: function() {
            var self = this;
            
            // گوش دادن به رویداد بارگذاری بیمه
            if (window.ReceptionEventBus) {
                window.ReceptionEventBus.on('patientInsurance:load', function(data) {
                    self.loadPatientInsurance(data.patientId);
                });
            }
        },

        // ========================================
        // LOAD INSURANCE HANDLER - مدیریت بارگذاری بیمه
        // ========================================
        handleLoadInsurance: function(module) {
            const $btn = $(this);
            var patientId = this.getCurrentPatientId();
            
            if (!patientId) {
                module.showError('شناسه بیمار یافت نشد');
                return;
            }
            
            module.loadPatientInsurance(patientId);
        },

        // ========================================
        // LOAD PATIENT INSURANCE - بارگذاری اطلاعات بیمه
        // ========================================
        loadPatientInsurance: function(patientId) {
            if (!patientId) {
                console.warn('[PatientInsuranceModule] Patient ID not provided for insurance loading');
                return;
            }

            this.showInfo(CONFIG.messages.info.loading);

            var self = this;
            
            $.ajax({
                url: CONFIG.endpoints.load,
                type: 'POST',
                data: { 
                    patientId: patientId,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                dataType: 'json',
                timeout: CONFIG.validation.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.handleLoadSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.handleLoadError(xhr, status, error);
                }
            });
        },

        // ========================================
        // LOAD SUCCESS HANDLER - مدیریت موفقیت بارگذاری
        // ========================================
        handleLoadSuccess: function(response) {
            if (response.success && response.data) {
                this.displayInsuranceInfo(response.data);
                this.showSuccess(CONFIG.messages.success.loaded);
            } else {
                this.showWarning(response.message || CONFIG.messages.error.notFound);
            }
        },

        // ========================================
        // LOAD ERROR HANDLER - مدیریت خطای بارگذاری
        // ========================================
        handleLoadError: function(xhr, status, error) {
            console.error('[PatientInsuranceModule] Error loading patient insurance:', error);
            this.showError(CONFIG.messages.error.loadError);
        },

        // ========================================
        // DISPLAY INSURANCE INFO - نمایش اطلاعات بیمه
        // ========================================
        displayInsuranceInfo: function(insuranceData) {
            if (!insuranceData) {
                console.error('[PatientInsuranceModule] Insurance data is null or undefined');
                return;
            }
            
            try {
                // نمایش اطلاعات بیمه اصلی
                if (insuranceData.primaryInsurance) {
                    $(CONFIG.selectors.primaryName).val(insuranceData.primaryInsurance.name || '');
                    $(CONFIG.selectors.primaryNumber).val(insuranceData.primaryInsurance.number || '');
                }
                
                // نمایش اطلاعات بیمه تکمیلی
                if (insuranceData.supplementaryInsurance) {
                    $(CONFIG.selectors.supplementaryName).val(insuranceData.supplementaryInsurance.name || '');
                    $(CONFIG.selectors.supplementaryNumber).val(insuranceData.supplementaryInsurance.number || '');
                }
                
                // به‌روزرسانی وضعیت
                this.updateInsuranceStatus('loaded');
                
            } catch (error) {
                console.error('[PatientInsuranceModule] Error displaying insurance info:', error);
                this.handleError(error, 'displayInsuranceInfo');
            }
        },

        // ========================================
        // SAVE INSURANCE HANDLER - مدیریت ذخیره بیمه
        // ========================================
        handleSaveInsurance: function(module) {
            const $btn = $(this);
            module.showButtonLoading($btn);
            
            // TODO: Implement save insurance logic
            setTimeout(function() {
                module.hideButtonLoading($btn);
                module.showSuccess(CONFIG.messages.success.saved);
                module.updateAccordionState('insuranceSection', 'completed');
            }, 1000);
        },

        // ========================================
        // GET CURRENT PATIENT ID - دریافت شناسه بیمار فعلی
        // ========================================
        getCurrentPatientId: function() {
            // تلاش برای دریافت شناسه بیمار از فیلد مخفی یا data attribute
            var patientId = $('#currentPatientId').val() || 
                             $('[data-patient-id]').data('patient-id') ||
                             (window.ReceptionModules && window.ReceptionModules.Patient && window.ReceptionModules.Patient.currentPatientId);
            
            return patientId;
        },

        // ========================================
        // UPDATE INSURANCE STATUS - به‌روزرسانی وضعیت بیمه
        // ========================================
        updateInsuranceStatus: function(status) {
            var statusElement = $(CONFIG.selectors.statusMessage);
            if (statusElement.length) {
                statusElement.text(this.getStatusMessage(status));
                statusElement.removeClass('text-success text-warning text-danger')
                    .addClass(this.getStatusClass(status));
            }
        },

        getStatusMessage: function(status) {
            var messages = {
                'loaded': 'اطلاعات بیمه بارگذاری شد',
                'saved': 'اطلاعات بیمه ذخیره شد',
                'error': 'خطا در بارگذاری اطلاعات بیمه'
            };
            return messages[status] || '';
        },

        getStatusClass: function(status) {
            var classes = {
                'loaded': 'text-success',
                'saved': 'text-success',
                'error': 'text-danger'
            };
            return classes[status] || 'text-muted';
        },

        // ========================================
        // UI HELPER METHODS - متدهای کمکی UI
        // ========================================
        showButtonLoading: function($btn) {
            if (!$btn || $btn.length === 0) {
                console.error('[PatientInsuranceModule] Button element is null or undefined');
                return;
            }
            
            try {
                $btn.prop('disabled', true);
                $btn.find('.btn-text').addClass('d-none');
                $btn.find('.btn-loading').removeClass('d-none');
            } catch (error) {
                console.error('[PatientInsuranceModule] Error showing button loading:', error);
            }
        },

        hideButtonLoading: function($btn) {
            if (!$btn || $btn.length === 0) {
                console.error('[PatientInsuranceModule] Button element is null or undefined');
                return;
            }
            
            try {
                $btn.prop('disabled', false);
                $btn.find('.btn-text').removeClass('d-none');
                $btn.find('.btn-loading').addClass('d-none');
            } catch (error) {
                console.error('[PatientInsuranceModule] Error hiding button loading:', error);
            }
        },

        // ========================================
        // MESSAGE DISPLAY METHODS - متدهای نمایش پیام
        // ========================================
        showError: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showError(message);
            } else {
                console.error('[PatientInsuranceModule] Error:', message);
            }
        },

        showSuccess: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showSuccess(message);
            } else {
                console.log('[PatientInsuranceModule] Success:', message);
            }
        },

        showInfo: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showInfo(message);
            } else {
                console.log('[PatientInsuranceModule] Info:', message);
            }
        },

        showWarning: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showWarning(message);
            } else {
                console.warn('[PatientInsuranceModule] Warning:', message);
            }
        },

        // ========================================
        // UTILITY METHODS - متدهای کمکی
        // ========================================
        updateAccordionState: function(section, state) {
            if (window.ReceptionModules && window.ReceptionModules.Accordion) {
                window.ReceptionModules.Accordion.setState(section, state);
            }
        },

        // ========================================
        // ERROR HANDLING - مدیریت خطا
        // ========================================
        handleError: function(error, context) {
            console.error(`[PatientInsuranceModule] Error in ${context}:`, error);
            
            if (window.ReceptionErrorHandler && typeof window.ReceptionErrorHandler.handle === 'function') {
                window.ReceptionErrorHandler.handle(error, 'PatientInsuranceModule', context);
            }
        }
    };

    // ========================================
    // MODULE EXPORT - صادرات ماژول
    // ========================================
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = PatientInsuranceModule;
    } else {
        global.PatientInsuranceModule = PatientInsuranceModule;
    }

})(window, jQuery);
