/**
 * REAL-TIME INSURANCE BINDING MODULE - ماژول اتصال Real-Time بیمه
 * ================================================================
 * 
 * این ماژول مسئولیت‌های زیر را دارد:
 * - اتصال خودکار اطلاعات بیمه پس از جستجوی بیمار
 * - تشخیص تغییرات بیمه و به‌روزرسانی Real-Time
 * - مدیریت UX حرفه‌ای برای تغییر بیمه
 * - بهینه‌سازی Performance برای Production
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
            loadInsurance: '/Reception/Insurance/Load',
            saveInsurance: '/Reception/Insurance/Save',
            validateInsurance: '/Reception/Insurance/QuickValidatePatientInsurance',
            changeInsurance: '/Reception/Insurance/ChangePatientInsurance'
        },
        
        // Performance Settings
        performance: {
            debounceDelay: 300,
            ajaxTimeout: 10000,
            cacheTimeout: 30000,
            maxRetries: 3
        },
        
        // UI Selectors
        selectors: {
            // Patient Section
            patientId: '#patientId',
            patientNationalCode: '#patientNationalCode',
            
            // Insurance Section
            insuranceSection: '#insuranceAccordionSection',
            loadInsuranceBtn: '#loadPatientInsuranceBtn',
            saveInsuranceBtn: '#saveInsuranceBtn',
            
            // Primary Insurance
            primaryProvider: '#insuranceProvider',
            primaryPlan: '#insurancePlan',
            primaryPolicyNumber: '#policyNumber',
            primaryCardNumber: '#cardNumber',
            
            // Supplementary Insurance
            supplementaryProvider: '#supplementaryProvider',
            supplementaryPlan: '#supplementaryPlan',
            supplementaryPolicyNumber: '#supplementaryPolicyNumber',
            supplementaryExpiry: '#supplementaryExpiry',
            
            // Status Indicators
            insuranceStatus: '#insuranceStatus',
            loadingIndicator: '#insuranceLoadingIndicator'
        },
        
        // Messages
        messages: {
            success: {
                loaded: 'اطلاعات بیمه با موفقیت بارگذاری شد',
                saved: 'اطلاعات بیمه با موفقیت ذخیره شد',
                updated: 'اطلاعات بیمه با موفقیت به‌روزرسانی شد'
            },
            error: {
                loadError: 'خطا در بارگذاری اطلاعات بیمه',
                saveError: 'خطا در ذخیره اطلاعات بیمه',
                validationError: 'خطا در اعتبارسنجی بیمه',
                networkError: 'خطا در ارتباط با سرور'
            },
            info: {
                loading: 'در حال بارگذاری اطلاعات بیمه...',
                saving: 'در حال ذخیره اطلاعات بیمه...',
                validating: 'در حال اعتبارسنجی بیمه...'
            }
        },
        
        // Animation Settings
        animation: {
            fadeInDuration: 300,
            fadeOutDuration: 200,
            slideDownDuration: 400,
            slideUpDuration: 300
        }
    };

    // ========================================
    // REAL-TIME INSURANCE BINDING MODULE - ماژول اصلی
    // ========================================
    var RealTimeInsuranceBinding = {
        
        // ========================================
        // INITIALIZATION - راه‌اندازی
        // ========================================
        init: function() {
            console.log('[RealTimeInsuranceBinding] Initializing Real-Time Insurance Binding Module...');
            
            try {
                this.bindEvents();
                this.initializeState();
                this.setupPerformanceMonitoring();
                
                console.log('[RealTimeInsuranceBinding] ✅ Module initialized successfully');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Initialization failed:', error);
                this.handleError(error, 'init');
            }
        },

        // ========================================
        // EVENT BINDING - اتصال رویدادها
        // ========================================
        bindEvents: function() {
            var self = this;
            
            // Patient Search Success Event
            $(document).on('patientSearchSuccess', function(event, patientData) {
                console.log('[RealTimeInsuranceBinding] Patient search success detected:', patientData);
                self.handlePatientSearchSuccess(patientData);
            });
            
            // Insurance Provider Change Events
            $(CONFIG.selectors.primaryProvider).on('change.insuranceBinding', function() {
                self.handlePrimaryProviderChange();
            });
            
            $(CONFIG.selectors.supplementaryProvider).on('change.insuranceBinding', function() {
                self.handleSupplementaryProviderChange();
            });
            
            // Manual Load Insurance Button
            $(CONFIG.selectors.loadInsuranceBtn).on('click.insuranceBinding', function() {
                self.loadPatientInsurance();
            });
            
            // Save Insurance Button
            $(CONFIG.selectors.saveInsuranceBtn).on('click.insuranceBinding', function() {
                self.savePatientInsurance();
            });
            
            // Form Change Detection
            $(CONFIG.selectors.insuranceSection + ' input, ' + CONFIG.selectors.insuranceSection + ' select').on('change.insuranceBinding', function() {
                self.handleInsuranceFormChange();
            });
        },

        // ========================================
        // PATIENT SEARCH SUCCESS HANDLER - مدیریت موفقیت جستجوی بیمار
        // ========================================
        handlePatientSearchSuccess: function(patientData) {
            console.log('[RealTimeInsuranceBinding] Handling patient search success:', patientData);
            
            if (!patientData || !patientData.PatientId) {
                console.warn('[RealTimeInsuranceBinding] Invalid patient data received');
                return;
            }
            
            // Update patient ID
            $(CONFIG.selectors.patientId).val(patientData.PatientId);
            
            // Auto-load insurance data
            this.loadPatientInsurance(patientData.PatientId);
        },

        // ========================================
        // LOAD PATIENT INSURANCE - بارگذاری اطلاعات بیمه بیمار
        // ========================================
        loadPatientInsurance: function(patientId) {
            var self = this;
            
            if (!patientId) {
                patientId = $(CONFIG.selectors.patientId).val();
            }
            
            if (!patientId || patientId <= 0) {
                console.warn('[RealTimeInsuranceBinding] Invalid patient ID for insurance loading');
                this.showError(CONFIG.messages.error.loadError);
                return;
            }
            
            console.log('[RealTimeInsuranceBinding] Loading insurance for patient:', patientId);
            
            // Show loading indicator
            this.showLoadingIndicator();
            
            $.ajax({
                url: CONFIG.endpoints.loadInsurance,
                type: 'POST',
                data: {
                    patientId: patientId,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                timeout: CONFIG.performance.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.hideLoadingIndicator();
                    self.handleInsuranceLoadSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideLoadingIndicator();
                    self.handleInsuranceLoadError(xhr, status, error);
                }
            });
        },

        // ========================================
        // INSURANCE LOAD SUCCESS HANDLER - مدیریت موفقیت بارگذاری بیمه
        // ========================================
        handleInsuranceLoadSuccess: function(response) {
            console.log('[RealTimeInsuranceBinding] Insurance load success:', response);
            
            if (response.success && response.data) {
                this.bindInsuranceDataToForm(response.data);
                this.updateInsuranceStatus('loaded');
                this.showSuccess(CONFIG.messages.success.loaded);
                
                // Trigger insurance loaded event
                $(document).trigger('insuranceLoaded', [response.data]);
            } else {
                this.showError(response.message || CONFIG.messages.error.loadError);
            }
        },

        // ========================================
        // BIND INSURANCE DATA TO FORM - اتصال اطلاعات بیمه به فرم
        // ========================================
        bindInsuranceDataToForm: function(insuranceData) {
            console.log('[RealTimeInsuranceBinding] Binding insurance data to form:', insuranceData);
            
            try {
                // Bind primary insurance data
                if (insuranceData.PrimaryInsurance) {
                    this.bindPrimaryInsuranceData(insuranceData.PrimaryInsurance);
                }
                
                // Bind supplementary insurance data
                if (insuranceData.SupplementaryInsurance) {
                    this.bindSupplementaryInsuranceData(insuranceData.SupplementaryInsurance);
                }
                
                // Update form state
                this.updateFormState(insuranceData);
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error binding insurance data:', error);
                this.handleError(error, 'bindInsuranceDataToForm');
            }
        },

        // ========================================
        // BIND PRIMARY INSURANCE DATA - اتصال اطلاعات بیمه پایه
        // ========================================
        bindPrimaryInsuranceData: function(primaryInsurance) {
            console.log('[RealTimeInsuranceBinding] Binding primary insurance data:', primaryInsurance);
            
            // Set provider
            if (primaryInsurance.ProviderId) {
                $(CONFIG.selectors.primaryProvider).val(primaryInsurance.ProviderId);
            }
            
            // Set plan
            if (primaryInsurance.PlanId) {
                $(CONFIG.selectors.primaryPlan).val(primaryInsurance.PlanId);
            }
            
            // Set policy number
            if (primaryInsurance.PolicyNumber) {
                $(CONFIG.selectors.primaryPolicyNumber).val(primaryInsurance.PolicyNumber);
            }
            
            // Set card number
            if (primaryInsurance.CardNumber) {
                $(CONFIG.selectors.primaryCardNumber).val(primaryInsurance.CardNumber);
            }
        },

        // ========================================
        // BIND SUPPLEMENTARY INSURANCE DATA - اتصال اطلاعات بیمه تکمیلی
        // ========================================
        bindSupplementaryInsuranceData: function(supplementaryInsurance) {
            console.log('[RealTimeInsuranceBinding] Binding supplementary insurance data:', supplementaryInsurance);
            
            // Set provider
            if (supplementaryInsurance.ProviderId) {
                $(CONFIG.selectors.supplementaryProvider).val(supplementaryInsurance.ProviderId);
            }
            
            // Set plan
            if (supplementaryInsurance.PlanId) {
                $(CONFIG.selectors.supplementaryPlan).val(supplementaryInsurance.PlanId);
            }
            
            // Set policy number
            if (supplementaryInsurance.PolicyNumber) {
                $(CONFIG.selectors.supplementaryPolicyNumber).val(supplementaryInsurance.PolicyNumber);
            }
            
            // Set expiry date
            if (supplementaryInsurance.ExpiryDate) {
                $(CONFIG.selectors.supplementaryExpiry).val(supplementaryInsurance.ExpiryDate);
            }
        },

        // ========================================
        // PRIMARY PROVIDER CHANGE HANDLER - مدیریت تغییر ارائه‌دهنده بیمه پایه
        // ========================================
        handlePrimaryProviderChange: function() {
            var providerId = $(CONFIG.selectors.primaryProvider).val();
            console.log('[RealTimeInsuranceBinding] Primary provider changed to:', providerId);
            
            if (providerId) {
                this.loadPrimaryInsurancePlans(providerId);
            } else {
                this.clearPrimaryInsurancePlans();
            }
        },

        // ========================================
        // SUPPLEMENTARY PROVIDER CHANGE HANDLER - مدیریت تغییر ارائه‌دهنده بیمه تکمیلی
        // ========================================
        handleSupplementaryProviderChange: function() {
            var providerId = $(CONFIG.selectors.supplementaryProvider).val();
            console.log('[RealTimeInsuranceBinding] Supplementary provider changed to:', providerId);
            
            if (providerId) {
                this.loadSupplementaryInsurancePlans(providerId);
            } else {
                this.clearSupplementaryInsurancePlans();
            }
        },

        // ========================================
        // LOAD PRIMARY INSURANCE PLANS - بارگذاری طرح‌های بیمه پایه
        // ========================================
        loadPrimaryInsurancePlans: function(providerId) {
            console.log('[RealTimeInsuranceBinding] Loading primary insurance plans for provider:', providerId);
            
            // Use existing ReceptionModules.Insurance method
            if (window.ReceptionModules && window.ReceptionModules.Insurance && window.ReceptionModules.Insurance.loadPrimaryInsurancePlans) {
                window.ReceptionModules.Insurance.loadPrimaryInsurancePlans(providerId);
            }
        },

        // ========================================
        // LOAD SUPPLEMENTARY INSURANCE PLANS - بارگذاری طرح‌های بیمه تکمیلی
        // ========================================
        loadSupplementaryInsurancePlans: function(providerId) {
            console.log('[RealTimeInsuranceBinding] Loading supplementary insurance plans for provider:', providerId);
            
            // Use existing ReceptionModules.Insurance method
            if (window.ReceptionModules && window.ReceptionModules.Insurance && window.ReceptionModules.Insurance.loadSupplementaryInsurancePlans) {
                window.ReceptionModules.Insurance.loadSupplementaryInsurancePlans(providerId);
            }
        },

        // ========================================
        // CLEAR PRIMARY INSURANCE PLANS - پاک کردن طرح‌های بیمه پایه
        // ========================================
        clearPrimaryInsurancePlans: function() {
            $(CONFIG.selectors.primaryPlan).empty().append('<option value="">انتخاب طرح بیمه پایه...</option>');
        },

        // ========================================
        // CLEAR SUPPLEMENTARY INSURANCE PLANS - پاک کردن طرح‌های بیمه تکمیلی
        // ========================================
        clearSupplementaryInsurancePlans: function() {
            $(CONFIG.selectors.supplementaryPlan).empty().append('<option value="">انتخاب طرح بیمه تکمیلی...</option>');
        },

        // ========================================
        // INSURANCE FORM CHANGE HANDLER - مدیریت تغییر فرم بیمه
        // ========================================
        handleInsuranceFormChange: function() {
            console.log('[RealTimeInsuranceBinding] Insurance form changed');
            
            // Update form state
            this.updateFormState();
            
            // Trigger form change event
            $(document).trigger('insuranceFormChanged');
        },

        // ========================================
        // SAVE PATIENT INSURANCE - ذخیره اطلاعات بیمه بیمار
        // ========================================
        savePatientInsurance: function() {
            var self = this;
            
            var patientId = $(CONFIG.selectors.patientId).val();
            if (!patientId || patientId <= 0) {
                this.showError('شناسه بیمار نامعتبر است');
                return;
            }
            
            console.log('[RealTimeInsuranceBinding] Saving insurance for patient:', patientId);
            
            // اعتبارسنجی فرم
            var validation = this.validateInsuranceForm();
            if (!validation.isValid) {
                this.showError('لطفاً فرم را کامل کنید: ' + validation.errors.join(', '));
                return;
            }
            
            // Show loading indicator
            this.showLoadingIndicator();
            this.updateProgressSteps(3); // مرحله 3: ویرایش اطلاعات
            
            // Collect insurance data
            var insuranceData = this.collectInsuranceData();
            
            $.ajax({
                url: CONFIG.endpoints.saveInsurance,
                type: 'POST',
                data: $.extend(insuranceData, {
                    patientId: patientId,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                }),
                timeout: CONFIG.performance.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.hideLoadingIndicator();
                    self.handleInsuranceSaveSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideLoadingIndicator();
                    self.handleInsuranceSaveError(xhr, status, error);
                }
            });
        },

        // ========================================
        // COLLECT INSURANCE DATA - جمع‌آوری اطلاعات بیمه
        // ========================================
        collectInsuranceData: function() {
            return {
                // Primary Insurance
                PrimaryProviderId: $(CONFIG.selectors.primaryProvider).val(),
                PrimaryPlanId: $(CONFIG.selectors.primaryPlan).val(),
                PrimaryPolicyNumber: $(CONFIG.selectors.primaryPolicyNumber).val(),
                PrimaryCardNumber: $(CONFIG.selectors.primaryCardNumber).val(),
                
                // Supplementary Insurance
                SupplementaryProviderId: $(CONFIG.selectors.supplementaryProvider).val(),
                SupplementaryPlanId: $(CONFIG.selectors.supplementaryPlan).val(),
                SupplementaryPolicyNumber: $(CONFIG.selectors.supplementaryPolicyNumber).val(),
                SupplementaryExpiryDate: $(CONFIG.selectors.supplementaryExpiry).val()
            };
        },

        // ========================================
        // INSURANCE SAVE SUCCESS HANDLER - مدیریت موفقیت ذخیره بیمه
        // ========================================
        handleInsuranceSaveSuccess: function(response) {
            console.log('[RealTimeInsuranceBinding] Insurance save success:', response);
            
            if (response.success) {
                this.updateInsuranceStatus('saved');
                this.showSuccess(CONFIG.messages.success.saved);
                
                // Trigger insurance saved event
                $(document).trigger('insuranceSaved', [response.data]);
            } else {
                this.showError(response.message || CONFIG.messages.error.saveError);
            }
        },

        // ========================================
        // UPDATE INSURANCE STATUS - به‌روزرسانی وضعیت بیمه
        // ========================================
        updateInsuranceStatus: function(status) {
            var statusText = '';
            var statusClass = '';
            
            switch (status) {
                case 'loaded':
                    statusText = 'بارگذاری شده';
                    statusClass = 'text-success';
                    this.updateProgressSteps(2); // مرحله 2: بارگذاری بیمه
                    break;
                case 'saved':
                    statusText = 'ذخیره شده';
                    statusClass = 'text-success';
                    this.updateProgressSteps(4); // مرحله 4: ذخیره نهایی
                    break;
                case 'error':
                    statusText = 'خطا';
                    statusClass = 'text-danger';
                    break;
                default:
                    statusText = 'آماده';
                    statusClass = 'text-muted';
                    this.updateProgressSteps(1); // مرحله 1: جستجوی بیمار
            }
            
            $(CONFIG.selectors.insuranceStatus).html('<span class="' + statusClass + '">' + statusText + '</span>');
        },

        // ========================================
        // UPDATE PROGRESS STEPS - به‌روزرسانی مراحل پیشرفت
        // ========================================
        updateProgressSteps: function(currentStep) {
            $('.insurance-progress-steps .step').each(function(index) {
                var stepNumber = index + 1;
                var $step = $(this);
                
                $step.removeClass('completed current');
                
                if (stepNumber < currentStep) {
                    $step.addClass('completed');
                } else if (stepNumber === currentStep) {
                    $step.addClass('current');
                }
            });
        },

        // ========================================
        // UPDATE FORM STATE - به‌روزرسانی وضعیت فرم
        // ========================================
        updateFormState: function(insuranceData) {
            // Update form validation state
            this.validateInsuranceForm();
            
            // Update save button state
            this.updateSaveButtonState();
        },

        // ========================================
        // VALIDATE INSURANCE FORM - اعتبارسنجی فرم بیمه
        // ========================================
        validateInsuranceForm: function() {
            var isValid = true;
            var errors = [];
            
            // Validate primary insurance
            var primaryProvider = $(CONFIG.selectors.primaryProvider).val();
            var primaryPlan = $(CONFIG.selectors.primaryPlan).val();
            
            if (primaryProvider && !primaryPlan) {
                isValid = false;
                errors.push('لطفاً طرح بیمه پایه را انتخاب کنید');
            }
            
            // Validate supplementary insurance
            var supplementaryProvider = $(CONFIG.selectors.supplementaryProvider).val();
            var supplementaryPlan = $(CONFIG.selectors.supplementaryPlan).val();
            
            if (supplementaryProvider && !supplementaryPlan) {
                isValid = false;
                errors.push('لطفاً طرح بیمه تکمیلی را انتخاب کنید');
            }
            
            return {
                isValid: isValid,
                errors: errors
            };
        },

        // ========================================
        // UPDATE SAVE BUTTON STATE - به‌روزرسانی وضعیت دکمه ذخیره
        // ========================================
        updateSaveButtonState: function() {
            var validation = this.validateInsuranceForm();
            var $saveBtn = $(CONFIG.selectors.saveInsuranceBtn);
            
            if (validation.isValid) {
                $saveBtn.prop('disabled', false).removeClass('btn-secondary').addClass('btn-success');
            } else {
                $saveBtn.prop('disabled', true).removeClass('btn-success').addClass('btn-secondary');
            }
        },

        // ========================================
        // LOADING INDICATOR MANAGEMENT - مدیریت نشانگر بارگذاری
        // ========================================
        showLoadingIndicator: function() {
            $(CONFIG.selectors.loadingIndicator).show();
            $(CONFIG.selectors.loadInsuranceBtn).prop('disabled', true);
        },

        hideLoadingIndicator: function() {
            $(CONFIG.selectors.loadingIndicator).hide();
            $(CONFIG.selectors.loadInsuranceBtn).prop('disabled', false);
        },

        // ========================================
        // ERROR HANDLING - مدیریت خطا
        // ========================================
        handleError: function(error, context) {
            console.error('[RealTimeInsuranceBinding] Error in ' + context + ':', error);
            this.showError(CONFIG.messages.error.networkError);
        },

        handleInsuranceLoadError: function(xhr, status, error) {
            console.error('[RealTimeInsuranceBinding] Insurance load error:', xhr, status, error);
            this.showError(CONFIG.messages.error.loadError);
        },

        handleInsuranceSaveError: function(xhr, status, error) {
            console.error('[RealTimeInsuranceBinding] Insurance save error:', xhr, status, error);
            this.showError(CONFIG.messages.error.saveError);
        },

        // ========================================
        // MESSAGE DISPLAY - نمایش پیام‌ها
        // ========================================
        showSuccess: function(message) {
            if (window.ReceptionToastr && window.ReceptionToastr.success) {
                window.ReceptionToastr.success(message);
            } else {
                console.log('[RealTimeInsuranceBinding] Success:', message);
            }
        },

        showError: function(message) {
            if (window.ReceptionToastr && window.ReceptionToastr.error) {
                window.ReceptionToastr.error(message);
            } else {
                console.error('[RealTimeInsuranceBinding] Error:', message);
            }
        },

        showInfo: function(message) {
            if (window.ReceptionToastr && window.ReceptionToastr.info) {
                window.ReceptionToastr.info(message);
            } else {
                console.log('[RealTimeInsuranceBinding] Info:', message);
            }
        },

        // ========================================
        // INITIALIZATION - راه‌اندازی اولیه
        // ========================================
        initializeState: function() {
            this.updateInsuranceStatus('unknown');
            this.updateSaveButtonState();
        },

        // ========================================
        // PERFORMANCE MONITORING - نظارت بر عملکرد
        // ========================================
        setupPerformanceMonitoring: function() {
            // Monitor AJAX performance
            var self = this;
            
            $(document).ajaxStart(function() {
                self.performanceStartTime = Date.now();
            });
            
            $(document).ajaxComplete(function() {
                if (self.performanceStartTime) {
                    var duration = Date.now() - self.performanceStartTime;
                    console.log('[RealTimeInsuranceBinding] AJAX request completed in', duration, 'ms');
                }
            });
        }
    };

    // ========================================
    // MODULE EXPORT - صادرات ماژول
    // ========================================
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = RealTimeInsuranceBinding;
    } else {
        global.RealTimeInsuranceBinding = RealTimeInsuranceBinding;
    }

})(window, jQuery);
