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
            console.log('[RealTimeInsuranceBinding] Binding events...');
            
            try {
                var self = this;
                
                // Patient Search Success Event
                $(document).on('patientSearchSuccess', function(event, patientData) {
                    console.log('[RealTimeInsuranceBinding] Patient search success detected:', patientData);
                    self.handlePatientSearchSuccess(patientData);
                });
                
                // Insurance Provider Change Events
                $(CONFIG.selectors.primaryProvider).on('change.insuranceBinding', function() {
                    console.log('[RealTimeInsuranceBinding] Primary provider changed');
                    if (self.suppressFormChange) { console.log('[RealTimeInsuranceBinding] Change suppressed'); return; }
                    self.handlePrimaryProviderChange();
                });
                
                $(CONFIG.selectors.supplementaryProvider).on('change.insuranceBinding', function() {
                    console.log('[RealTimeInsuranceBinding] Supplementary provider changed');
                    if (self.suppressFormChange) { console.log('[RealTimeInsuranceBinding] Change suppressed'); return; }
                    self.handleSupplementaryProviderChange();
                });
                
                // Manual Load Insurance Button
                $(CONFIG.selectors.loadInsuranceBtn).on('click.insuranceBinding', function() {
                    console.log('[RealTimeInsuranceBinding] Load insurance button clicked');
                    self.loadPatientInsurance();
                });
                
                // Save Insurance Button
                $(CONFIG.selectors.saveInsuranceBtn).on('click.insuranceBinding', function() {
                    console.log('[RealTimeInsuranceBinding] Save insurance button clicked');
                    self.savePatientInsurance();
                });
                
                // Form Change Detection
                $(CONFIG.selectors.insuranceSection + ' input, ' + CONFIG.selectors.insuranceSection + ' select').on('change.insuranceBinding', function() {
                    console.log('[RealTimeInsuranceBinding] Insurance form changed');
                    if (self.suppressFormChange) { console.log('[RealTimeInsuranceBinding] Change suppressed'); return; }
                    self.handleInsuranceFormChange();
                });

                // Input events to catch clears/typing (real-time UX)
                $(CONFIG.selectors.insuranceSection + ' input').on('input.insuranceBinding', function() {
                    console.log('[RealTimeInsuranceBinding] Insurance form input changed');
                    if (self.suppressFormChange) { console.log('[RealTimeInsuranceBinding] Input suppressed'); return; }
                    self.handleInsuranceFormChange();
                });

                // Select2 clear/unselect safety (if Select2 is used)
                if ($.fn.select2) {
                    $(CONFIG.selectors.supplementaryProvider).on('select2:clear.select2Insurance select2:unselect.select2Insurance', function() {
                        console.log('[RealTimeInsuranceBinding] Supplementary provider cleared via Select2');
                        if (self.suppressFormChange) { console.log('[RealTimeInsuranceBinding] Select2 clear suppressed'); return; }
                        self.handleInsuranceFormChange();
                    });
                    $(CONFIG.selectors.supplementaryPlan).on('select2:clear.select2Insurance select2:unselect.select2Insurance', function() {
                        console.log('[RealTimeInsuranceBinding] Supplementary plan cleared via Select2');
                        if (self.suppressFormChange) { console.log('[RealTimeInsuranceBinding] Select2 clear suppressed'); return; }
                        self.handleInsuranceFormChange();
                    });
                }
                
                console.log('[RealTimeInsuranceBinding] ✅ Events bound successfully');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error binding events:', error);
                this.handleError(error, 'bindEvents');
            }
        },

        // ========================================
        // PATIENT SEARCH SUCCESS HANDLER - مدیریت موفقیت جستجوی بیمار
        // ========================================
        handlePatientSearchSuccess: function(patientData) {
            console.log('[RealTimeInsuranceBinding] Handling patient search success:', patientData);
            
            try {
                if (!patientData || !patientData.PatientId) {
                    console.warn('[RealTimeInsuranceBinding] Invalid patient data received');
                    this.showError('اطلاعات بیمار نامعتبر است');
                    return;
                }
                
                console.log('[RealTimeInsuranceBinding] Patient ID:', patientData.PatientId);
                
                // Update patient ID
                $(CONFIG.selectors.patientId).val(patientData.PatientId);
                
                // Auto-load insurance data
                this.loadPatientInsurance(patientData.PatientId);
                
                console.log('[RealTimeInsuranceBinding] ✅ Patient search success handled');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error handling patient search success:', error);
                this.handleError(error, 'handlePatientSearchSuccess');
            }
        },

        // ========================================
        // LOAD PATIENT INSURANCE - بارگذاری اطلاعات بیمه بیمار
        // ========================================
        loadPatientInsurance: function(patientId) {
            console.log('[RealTimeInsuranceBinding] Loading insurance for patient:', patientId);
            
            try {
                var self = this;
                
                if (!patientId) {
                    patientId = $(CONFIG.selectors.patientId).val();
                }
                
                if (!patientId || patientId <= 0) {
                    console.warn('[RealTimeInsuranceBinding] Invalid patient ID for insurance loading');
                    this.showError(CONFIG.messages.error.loadError);
                    return;
                }
                
                console.log('[RealTimeInsuranceBinding] Patient ID validated:', patientId);
                
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
                
                console.log('[RealTimeInsuranceBinding] ✅ Insurance load request sent');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error loading patient insurance:', error);
                this.handleError(error, 'loadPatientInsurance');
            }
        },

        // ========================================
        // INSURANCE LOAD SUCCESS HANDLER - مدیریت موفقیت بارگذاری بیمه
        // ========================================
        handleInsuranceLoadSuccess: function(response) {
            console.log('[RealTimeInsuranceBinding] Insurance load success:', response);
            
            try {
                // Parse response if it's a string
                var parsedResponse = response;
                if (typeof response === 'string') {
                    try {
                        parsedResponse = JSON.parse(response);
                        console.log('[RealTimeInsuranceBinding] Response parsed successfully');
                    } catch (parseError) {
                        console.error('[RealTimeInsuranceBinding] Failed to parse response:', parseError);
                        this.showError('خطا در پردازش پاسخ سرور');
                        return;
                    }
                }
                
                // بررسی دقیق‌تر پاسخ
                console.log('[RealTimeInsuranceBinding] Response type:', typeof parsedResponse);
                console.log('[RealTimeInsuranceBinding] Response success:', parsedResponse.success);
                console.log('[RealTimeInsuranceBinding] Response success type:', typeof parsedResponse.success);
                console.log('[RealTimeInsuranceBinding] Response success === true:', parsedResponse.success === true);
                console.log('[RealTimeInsuranceBinding] Response success == true:', parsedResponse.success == true);
                
                if (parsedResponse && parsedResponse.success === true) {
                    console.log('[RealTimeInsuranceBinding] ✅ Response is successful');
                    console.log('[RealTimeInsuranceBinding] Response data:', parsedResponse.data);
                    
                    if (parsedResponse.data) {
                        console.log('[RealTimeInsuranceBinding] ✅ Data exists, binding to form');
                        this.bindInsuranceDataToForm(parsedResponse.data);
                        this.updateInsuranceStatus('loaded');
                        this.showSuccess(CONFIG.messages.success.loaded);
                        
                        // Trigger insurance loaded event
                        $(document).trigger('insuranceLoaded', [parsedResponse.data]);
                    } else {
                        console.warn('[RealTimeInsuranceBinding] No insurance data received');
                        this.showError('اطلاعات بیمه یافت نشد');
                    }
                } else {
                    console.error('[RealTimeInsuranceBinding] Response not successful:', parsedResponse);
                    this.showError(parsedResponse.message || CONFIG.messages.error.loadError);
                }
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error in handleInsuranceLoadSuccess:', error);
                this.handleError(error, 'handleInsuranceLoadSuccess');
            }
        },

        // ========================================
        // BIND INSURANCE DATA TO FORM - اتصال اطلاعات بیمه به فرم
        // ========================================
        bindInsuranceDataToForm: function(insuranceData) {
            console.log('[RealTimeInsuranceBinding] Binding insurance data to form:', insuranceData);
            
            try {
                // بررسی وجود insuranceData
                if (!insuranceData) {
                    console.error('[RealTimeInsuranceBinding] No insurance data provided');
                    this.showError('اطلاعات بیمه موجود نیست');
                    return;
                }
                
                console.log('[RealTimeInsuranceBinding] Insurance data type:', typeof insuranceData);
                console.log('[RealTimeInsuranceBinding] Insurance data keys:', Object.keys(insuranceData));
                console.log('[RealTimeInsuranceBinding] Has PrimaryInsurance:', !!insuranceData.PrimaryInsurance);
                console.log('[RealTimeInsuranceBinding] Has SupplementaryInsurance:', !!insuranceData.SupplementaryInsurance);
                
                // بررسی وجود Elements قبل از بایند کردن
                var $primaryProvider = $(CONFIG.selectors.primaryProvider);
                var $primaryPlan = $(CONFIG.selectors.primaryPlan);
                var $primaryPolicyNumber = $(CONFIG.selectors.primaryPolicyNumber);
                var $primaryCardNumber = $(CONFIG.selectors.primaryCardNumber);
                
                console.log('[RealTimeInsuranceBinding] Checking DOM elements...');
                console.log('[RealTimeInsuranceBinding] Primary provider found:', $primaryProvider.length);
                console.log('[RealTimeInsuranceBinding] Primary plan found:', $primaryPlan.length);
                console.log('[RealTimeInsuranceBinding] Primary policy number found:', $primaryPolicyNumber.length);
                console.log('[RealTimeInsuranceBinding] Primary card number found:', $primaryCardNumber.length);
                
                if ($primaryProvider.length === 0) {
                    console.warn('[RealTimeInsuranceBinding] Primary provider element not found, retrying in 500ms...');
                    setTimeout(() => this.bindInsuranceDataToForm(insuranceData), 500);
                    return;
                }
                
                // Bind primary insurance data
                if (insuranceData.PrimaryInsurance) {
                    console.log('[RealTimeInsuranceBinding] Binding primary insurance:', insuranceData.PrimaryInsurance);
                    this.bindPrimaryInsuranceData(insuranceData.PrimaryInsurance);
                } else {
                    console.log('[RealTimeInsuranceBinding] No primary insurance data');
                }
                
                // Bind supplementary insurance data
                if (insuranceData.SupplementaryInsurance) {
                    console.log('[RealTimeInsuranceBinding] ✅ Supplementary insurance found:', insuranceData.SupplementaryInsurance);
                    this.bindSupplementaryInsuranceData(insuranceData.SupplementaryInsurance);
                } else {
                    console.log('[RealTimeInsuranceBinding] ℹ️ No supplementary insurance data - patient has only primary insurance');
                    // پاک کردن فیلدهای بیمه تکمیلی
                    this.clearSupplementaryInsuranceFields();
                }
                
                // Update form state
                this.updateFormState(insuranceData);
                
                // به‌روزرسانی مقادیر اولیه فرم برای تشخیص تغییرات
                this.originalFormValues = this.captureFormValues();
                
                console.log('[RealTimeInsuranceBinding] ✅ Insurance data bound successfully');
                
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
            
            try {
                // بررسی وجود primaryInsurance
                if (!primaryInsurance) {
                    console.warn('[RealTimeInsuranceBinding] No primary insurance data provided');
                    return;
                }
                
                console.log('[RealTimeInsuranceBinding] Primary insurance type:', typeof primaryInsurance);
                console.log('[RealTimeInsuranceBinding] Primary insurance keys:', Object.keys(primaryInsurance));
                console.log('[RealTimeInsuranceBinding] Primary insurance details:', {
                    ProviderId: primaryInsurance.ProviderId,
                    PlanId: primaryInsurance.PlanId,
                    PolicyNumber: primaryInsurance.PolicyNumber,
                    CardNumber: primaryInsurance.CardNumber
                });
                
                // Set provider
                if (primaryInsurance.ProviderId) {
                    var $provider = $(CONFIG.selectors.primaryProvider);
                    console.log('[RealTimeInsuranceBinding] Setting provider:', primaryInsurance.ProviderId);
                    if ($provider.length > 0) {
                        // بررسی وجود option در select
                        var optionExists = $provider.find('option[value="' + primaryInsurance.ProviderId + '"]').length > 0;
                        if (optionExists) {
                            $provider.val(primaryInsurance.ProviderId).trigger('change');
                            console.log('[RealTimeInsuranceBinding] ✅ Provider set:', primaryInsurance.ProviderId);
                        } else {
                            console.warn('[RealTimeInsuranceBinding] Provider option not found in select, waiting for options to load...');
                            // انتظار برای بارگذاری options
                            setTimeout(() => {
                                var optionExists = $provider.find('option[value="' + primaryInsurance.ProviderId + '"]').length > 0;
                                if (optionExists) {
                                    $provider.val(primaryInsurance.ProviderId).trigger('change');
                                    console.log('[RealTimeInsuranceBinding] ✅ Provider set after delay:', primaryInsurance.ProviderId);
                                } else {
                                    console.warn('[RealTimeInsuranceBinding] Provider option still not found after delay');
                                }
                            }, 1000);
                        }
                    } else {
                        console.warn('[RealTimeInsuranceBinding] Primary provider element not found');
                    }
                }
                
                // Set plan
                if (primaryInsurance.PlanId) {
                    var $plan = $(CONFIG.selectors.primaryPlan);
                    console.log('[RealTimeInsuranceBinding] Setting plan:', primaryInsurance.PlanId);
                    if ($plan.length > 0) {
                        // بررسی وجود option در select
                        var optionExists = $plan.find('option[value="' + primaryInsurance.PlanId + '"]').length > 0;
                        if (optionExists) {
                            $plan.val(primaryInsurance.PlanId).trigger('change');
                            console.log('[RealTimeInsuranceBinding] ✅ Plan set:', primaryInsurance.PlanId);
                        } else {
                            console.warn('[RealTimeInsuranceBinding] Plan option not found in select, waiting for options to load...');
                            // انتظار برای بارگذاری options
                            setTimeout(() => {
                                var optionExists = $plan.find('option[value="' + primaryInsurance.PlanId + '"]').length > 0;
                                if (optionExists) {
                                    $plan.val(primaryInsurance.PlanId).trigger('change');
                                    console.log('[RealTimeInsuranceBinding] ✅ Plan set after delay:', primaryInsurance.PlanId);
                                } else {
                                    console.warn('[RealTimeInsuranceBinding] Plan option still not found after delay');
                                }
                            }, 1500);
                        }
                    } else {
                        console.warn('[RealTimeInsuranceBinding] Primary plan element not found');
                    }
                }
                
                // Set policy number
                if (primaryInsurance.PolicyNumber) {
                    var $policyNumber = $(CONFIG.selectors.primaryPolicyNumber);
                    console.log('[RealTimeInsuranceBinding] Setting policy number:', primaryInsurance.PolicyNumber);
                    if ($policyNumber.length > 0) {
                        $policyNumber.val(primaryInsurance.PolicyNumber);
                        console.log('[RealTimeInsuranceBinding] ✅ Policy number set:', primaryInsurance.PolicyNumber);
                    } else {
                        console.warn('[RealTimeInsuranceBinding] Primary policy number element not found');
                    }
                }
                
                // Set card number
                if (primaryInsurance.CardNumber) {
                    var $cardNumber = $(CONFIG.selectors.primaryCardNumber);
                    console.log('[RealTimeInsuranceBinding] Setting card number:', primaryInsurance.CardNumber);
                    if ($cardNumber.length > 0) {
                        $cardNumber.val(primaryInsurance.CardNumber);
                        console.log('[RealTimeInsuranceBinding] ✅ Card number set:', primaryInsurance.CardNumber);
                    } else {
                        console.warn('[RealTimeInsuranceBinding] Primary card number element not found');
                    }
                }
                
                console.log('[RealTimeInsuranceBinding] ✅ Primary insurance data bound successfully');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error binding primary insurance data:', error);
                throw error;
            }
        },

        // ========================================
        // BIND SUPPLEMENTARY INSURANCE DATA - اتصال اطلاعات بیمه تکمیلی
        // ========================================
        bindSupplementaryInsuranceData: function(supplementaryInsurance) {
            console.log('[RealTimeInsuranceBinding] Binding supplementary insurance data:', supplementaryInsurance);
            
            try {
                // بررسی وجود supplementaryInsurance
                if (!supplementaryInsurance) {
                    console.warn('[RealTimeInsuranceBinding] No supplementary insurance data provided');
                    return;
                }
                
                console.log('[RealTimeInsuranceBinding] Supplementary insurance type:', typeof supplementaryInsurance);
                console.log('[RealTimeInsuranceBinding] Supplementary insurance keys:', Object.keys(supplementaryInsurance));
                console.log('[RealTimeInsuranceBinding] Supplementary insurance details:', {
                    ProviderId: supplementaryInsurance.ProviderId,
                    PlanId: supplementaryInsurance.PlanId,
                    PolicyNumber: supplementaryInsurance.PolicyNumber,
                    ExpiryDate: supplementaryInsurance.ExpiryDate
                });
                
                // Set provider
                if (supplementaryInsurance.ProviderId) {
                    var $provider = $(CONFIG.selectors.supplementaryProvider);
                    console.log('[RealTimeInsuranceBinding] Setting supplementary provider:', supplementaryInsurance.ProviderId);
                    if ($provider.length > 0) {
                        // بررسی وجود option در select
                        var optionExists = $provider.find('option[value="' + supplementaryInsurance.ProviderId + '"]').length > 0;
                        if (optionExists) {
                            $provider.val(supplementaryInsurance.ProviderId).trigger('change');
                            console.log('[RealTimeInsuranceBinding] ✅ Supplementary provider set:', supplementaryInsurance.ProviderId);
                        } else {
                            console.warn('[RealTimeInsuranceBinding] Supplementary provider option not found in select, waiting for options to load...');
                            // انتظار برای بارگذاری options
                            setTimeout(() => {
                                var optionExists = $provider.find('option[value="' + supplementaryInsurance.ProviderId + '"]').length > 0;
                                if (optionExists) {
                                    $provider.val(supplementaryInsurance.ProviderId).trigger('change');
                                    console.log('[RealTimeInsuranceBinding] ✅ Supplementary provider set after delay:', supplementaryInsurance.ProviderId);
                                } else {
                                    console.warn('[RealTimeInsuranceBinding] Supplementary provider option still not found after delay');
                                }
                            }, 1000);
                        }
                    } else {
                        console.warn('[RealTimeInsuranceBinding] Supplementary provider element not found');
                    }
                }
                
                // Set plan
                if (supplementaryInsurance.PlanId) {
                    var $plan = $(CONFIG.selectors.supplementaryPlan);
                    console.log('[RealTimeInsuranceBinding] Setting supplementary plan:', supplementaryInsurance.PlanId);
                    if ($plan.length > 0) {
                        // بررسی وجود option در select
                        var optionExists = $plan.find('option[value="' + supplementaryInsurance.PlanId + '"]').length > 0;
                        if (optionExists) {
                            $plan.val(supplementaryInsurance.PlanId).trigger('change');
                            console.log('[RealTimeInsuranceBinding] ✅ Supplementary plan set:', supplementaryInsurance.PlanId);
                        } else {
                            console.warn('[RealTimeInsuranceBinding] Supplementary plan option not found in select, waiting for options to load...');
                            // انتظار برای بارگذاری options
                            setTimeout(() => {
                                var optionExists = $plan.find('option[value="' + supplementaryInsurance.PlanId + '"]').length > 0;
                                if (optionExists) {
                                    $plan.val(supplementaryInsurance.PlanId).trigger('change');
                                    console.log('[RealTimeInsuranceBinding] ✅ Supplementary plan set after delay:', supplementaryInsurance.PlanId);
                                } else {
                                    console.warn('[RealTimeInsuranceBinding] Supplementary plan option still not found after delay');
                                }
                            }, 1500);
                        }
                    } else {
                        console.warn('[RealTimeInsuranceBinding] Supplementary plan element not found');
                    }
                }
                
                // Set policy number
                if (supplementaryInsurance.PolicyNumber) {
                    var $policyNumber = $(CONFIG.selectors.supplementaryPolicyNumber);
                    console.log('[RealTimeInsuranceBinding] Setting supplementary policy number:', supplementaryInsurance.PolicyNumber);
                    if ($policyNumber.length > 0) {
                        $policyNumber.val(supplementaryInsurance.PolicyNumber);
                        console.log('[RealTimeInsuranceBinding] ✅ Supplementary policy number set:', supplementaryInsurance.PolicyNumber);
                    } else {
                        console.warn('[RealTimeInsuranceBinding] Supplementary policy number element not found');
                    }
                }
                
                // Set expiry date
                if (supplementaryInsurance.ExpiryDate) {
                    var $expiry = $(CONFIG.selectors.supplementaryExpiry);
                    console.log('[RealTimeInsuranceBinding] Setting supplementary expiry date:', supplementaryInsurance.ExpiryDate);
                    if ($expiry.length > 0) {
                        $expiry.val(supplementaryInsurance.ExpiryDate);
                        console.log('[RealTimeInsuranceBinding] ✅ Supplementary expiry date set:', supplementaryInsurance.ExpiryDate);
                    } else {
                        console.warn('[RealTimeInsuranceBinding] Supplementary expiry element not found');
                    }
                }
                
                console.log('[RealTimeInsuranceBinding] ✅ Supplementary insurance data bound successfully');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error binding supplementary insurance data:', error);
                throw error;
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
        // CLEAR SUPPLEMENTARY INSURANCE FIELDS - پاک کردن فیلدهای بیمه تکمیلی
        // ========================================
        clearSupplementaryInsuranceFields: function() {
            console.log('[RealTimeInsuranceBinding] Clearing supplementary insurance fields...');
            
            try {
                // پاک کردن provider
                $(CONFIG.selectors.supplementaryProvider).val('');
                // Trigger both native and plugin events
                $(CONFIG.selectors.supplementaryProvider).trigger('change').trigger('input');
                
                // پاک کردن plan
                $(CONFIG.selectors.supplementaryPlan)
                    .empty()
                    .append('<option value="">انتخاب طرح بیمه تکمیلی...</option>')
                    .trigger('change').trigger('input');
                
                // پاک کردن policy number
                $(CONFIG.selectors.supplementaryPolicyNumber).val('').trigger('input').trigger('change');
                
                // پاک کردن expiry date
                $(CONFIG.selectors.supplementaryExpiry).val('').trigger('input').trigger('change');
                
                // اطمینان از فعال شدن ویرایش پس از پاکسازی
                this.handleInsuranceFormChange();

                console.log('[RealTimeInsuranceBinding] ✅ Supplementary insurance fields cleared');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error clearing supplementary insurance fields:', error);
                this.handleError(error, 'clearSupplementaryInsuranceFields');
            }
        },

        // ========================================
        // INSURANCE FORM CHANGE HANDLER - مدیریت تغییر فرم بیمه
        // ========================================
        handleInsuranceFormChange: function() {
            console.log('[RealTimeInsuranceBinding] Insurance form changed');
            
            try {
                // Update form state
                this.updateFormState();
                
                // Trigger form change event
                $(document).trigger('insuranceFormChanged');
                
                console.log('[RealTimeInsuranceBinding] ✅ Form change handled');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error handling form change:', error);
                this.handleError(error, 'handleInsuranceFormChange');
            }
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

            // Prevent double-submit in high-traffic environments
            if (this._isSaving) {
                console.warn('[RealTimeInsuranceBinding] Save already in progress, ignoring duplicate submit');
                return;
            }
            
            // اعتبارسنجی فرم
            var validation = this.validateInsuranceForm();
            if (!validation.isValid) {
                this.showError('لطفاً فرم را کامل کنید: ' + validation.errors.join(', '));
                return;
            }
            
            // Show loading indicator
            this.showLoadingIndicator();
            this._isSaving = true;
            $(CONFIG.selectors.saveInsuranceBtn).prop('disabled', true).addClass('disabled');
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
                    self._isSaving = false;
                    $(CONFIG.selectors.saveInsuranceBtn).prop('disabled', false).removeClass('disabled');
                    self.handleInsuranceSaveSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideLoadingIndicator();
                    self._isSaving = false;
                    $(CONFIG.selectors.saveInsuranceBtn).prop('disabled', false).removeClass('disabled');
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
            
            try {
                // Normalize/parse response if needed
                var parsed = response;
                if (typeof response === 'string') {
                    try {
                        parsed = JSON.parse(response);
                        console.log('[RealTimeInsuranceBinding] Save response parsed successfully');
                    } catch (e) {
                        console.error('[RealTimeInsuranceBinding] Failed to parse save response:', e);
                        this.showError(CONFIG.messages.error.saveError);
                        return;
                    }
                }

                if (parsed && parsed.success === true) {
                    this.updateInsuranceStatus('saved');
                    this.showSuccess(parsed.message || CONFIG.messages.success.saved);

                    // به‌روزرسانی مقادیر اولیه فرم پس از ذخیره موفق
                    this.originalFormValues = this.captureFormValues();

                    // غیرفعال کردن حالت ویرایش
                    this.disableEditMode();

                    // همگام‌سازی UI با وضعیت قطعی سرور
                    try {
                        if (parsed.data && parsed.data.Status) {
                            console.log('[RealTimeInsuranceBinding] Rebinding insurance with authoritative server state after save');
                            this.bindInsuranceDataToForm(parsed.data.Status);
                        } else {
                            console.log('[RealTimeInsuranceBinding] Reloading insurance after save (no Status payload)');
                            this.loadPatientInsurance($(CONFIG.selectors.patientId).val());
                        }
                        this.updateSaveButtonState();
                    } catch (syncError) {
                        console.warn('[RealTimeInsuranceBinding] Post-save UI sync failed, fallback to reload:', syncError);
                        this.loadPatientInsurance($(CONFIG.selectors.patientId).val());
                    }

                    // Trigger insurance saved event
                    $(document).trigger('insuranceSaved', [parsed.data]);

                    console.log('[RealTimeInsuranceBinding] ✅ Insurance saved successfully');
                } else {
                    this.showError((parsed && parsed.message) || CONFIG.messages.error.saveError);
                }
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error handling insurance save success:', error);
                this.handleError(error, 'handleInsuranceSaveSuccess');
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
            console.log('[RealTimeInsuranceBinding] Updating form state...');
            
            try {
                // Update form validation state
                this.validateInsuranceForm();
                
                // Update save button state
                this.updateSaveButtonState();
                
                // نمایش وضعیت بیمه
                if (insuranceData) {
                    if (insuranceData.PrimaryInsurance) {
                        console.log('[RealTimeInsuranceBinding] ✅ Primary insurance available:', {
                            ProviderId: insuranceData.PrimaryInsurance.ProviderId,
                            PlanId: insuranceData.PrimaryInsurance.PlanId,
                            PolicyNumber: insuranceData.PrimaryInsurance.PolicyNumber
                        });
                    }
                    if (insuranceData.SupplementaryInsurance) {
                        console.log('[RealTimeInsuranceBinding] ✅ Supplementary insurance available:', {
                            ProviderId: insuranceData.SupplementaryInsurance.ProviderId,
                            PlanId: insuranceData.SupplementaryInsurance.PlanId,
                            PolicyNumber: insuranceData.SupplementaryInsurance.PolicyNumber
                        });
                    } else {
                        console.log('[RealTimeInsuranceBinding] ℹ️ No supplementary insurance - patient has only primary insurance');
                    }
                }
                
                console.log('[RealTimeInsuranceBinding] ✅ Form state updated successfully');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error updating form state:', error);
                this.handleError(error, 'updateFormState');
            }
        },

        // ========================================
        // VALIDATE INSURANCE FORM - اعتبارسنجی فرم بیمه
        // ========================================
        validateInsuranceForm: function() {
            console.log('[RealTimeInsuranceBinding] Validating insurance form...');
            
            try {
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
                
                console.log('[RealTimeInsuranceBinding] Form validation result:', {
                    isValid: isValid,
                    errors: errors,
                    primaryProvider: primaryProvider,
                    primaryPlan: primaryPlan,
                    supplementaryProvider: supplementaryProvider,
                    supplementaryPlan: supplementaryPlan
                });
                
                return {
                    isValid: isValid,
                    errors: errors
                };
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error validating insurance form:', error);
                return {
                    isValid: false,
                    errors: ['خطا در اعتبارسنجی فرم']
                };
            }
        },

        // ========================================
        // UPDATE SAVE BUTTON STATE - به‌روزرسانی وضعیت دکمه ذخیره
        // ========================================
        updateSaveButtonState: function() {
            console.log('[RealTimeInsuranceBinding] Updating save button state...');
            
            try {
                var validation = this.validateInsuranceForm();
                var $saveBtn = $(CONFIG.selectors.saveInsuranceBtn);
                
                console.log('[RealTimeInsuranceBinding] Save button validation:', {
                    isValid: validation.isValid,
                    errors: validation.errors
                });
                
                if (validation.isValid) {
                    $saveBtn.prop('disabled', false).removeClass('btn-secondary').addClass('btn-success');
                    console.log('[RealTimeInsuranceBinding] ✅ Save button enabled');
                } else {
                    $saveBtn.prop('disabled', true).removeClass('btn-success').addClass('btn-secondary');
                    console.log('[RealTimeInsuranceBinding] ❌ Save button disabled');
                }
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error updating save button state:', error);
                this.handleError(error, 'updateSaveButtonState');
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
            
            // نمایش خطای دقیق‌تر
            var errorMessage = CONFIG.messages.error.networkError;
            if (error && error.message) {
                errorMessage = error.message;
            } else if (error && typeof error === 'string') {
                errorMessage = error;
            }
            
            this.showError(errorMessage);
        },

        handleInsuranceLoadError: function(xhr, status, error) {
            console.error('[RealTimeInsuranceBinding] Insurance load error:', xhr, status, error);
            
            // نمایش خطای دقیق‌تر
            var errorMessage = CONFIG.messages.error.loadError;
            if (xhr && xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            } else if (xhr && xhr.responseText) {
                errorMessage = 'خطا در ارتباط با سرور: ' + xhr.responseText;
            } else if (error) {
                errorMessage = 'خطا در بارگذاری: ' + error;
            }
            
            this.showError(errorMessage);
        },

        handleInsuranceSaveError: function(xhr, status, error) {
            console.error('[RealTimeInsuranceBinding] Insurance save error:', xhr, status, error);
            
            // نمایش خطای دقیق‌تر
            var errorMessage = CONFIG.messages.error.saveError;
            if (xhr && xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            } else if (xhr && xhr.responseText) {
                errorMessage = 'خطا در ذخیره: ' + xhr.responseText;
            } else if (error) {
                errorMessage = 'خطا در ذخیره: ' + error;
            }
            
            this.showError(errorMessage);
        },

        // ========================================
        // MESSAGE DISPLAY - نمایش پیام‌ها
        // ========================================
        showSuccess: function(message) {
            console.log('[RealTimeInsuranceBinding] Success:', message);
            
            try {
                if (window.ReceptionToastr && window.ReceptionToastr.helpers && window.ReceptionToastr.helpers.showSuccess) {
                    window.ReceptionToastr.helpers.showSuccess(message);
                } else if (window.ReceptionToastr && window.ReceptionToastr.success) {
                    window.ReceptionToastr.success(message);
                } else {
                    // Fallback: نمایش موفقیت در کنسول
                    console.log('[RealTimeInsuranceBinding] Toastr not available, success logged to console');
                }
            } catch (toastError) {
                console.error('[RealTimeInsuranceBinding] Toast notification failed:', toastError);
            }
        },

        showError: function(message) {
            console.error('[RealTimeInsuranceBinding] Error:', message);
            
            try {
                if (window.ReceptionToastr && window.ReceptionToastr.helpers && window.ReceptionToastr.helpers.showError) {
                    window.ReceptionToastr.helpers.showError(message);
                } else if (window.ReceptionToastr && window.ReceptionToastr.error) {
                    window.ReceptionToastr.error(message);
                } else {
                    // Fallback: نمایش خطا در کنسول
                    console.error('[RealTimeInsuranceBinding] Toastr not available, error logged to console');
                }
            } catch (toastError) {
                console.error('[RealTimeInsuranceBinding] Toast notification failed:', toastError);
            }
        },

        showInfo: function(message) {
            console.log('[RealTimeInsuranceBinding] Info:', message);
            
            if (window.ReceptionToastr && window.ReceptionToastr.info) {
                window.ReceptionToastr.info(message);
            } else {
                // Fallback: نمایش اطلاعات در کنسول
                console.log('[RealTimeInsuranceBinding] Toastr not available, info logged to console');
            }
        },

        // ========================================
        // INITIALIZATION - راه‌اندازی اولیه
        // ========================================
        initializeState: function() {
            console.log('[RealTimeInsuranceBinding] Initializing state...');
            
            try {
                // Flags for robust real-time UX
                this.suppressFormChange = false;
                this._isSaving = false;
                this.pendingPrimaryPlanId = null;
                this.pendingSupplementaryPlanId = null;
                this.updateInsuranceStatus('unknown');
                this.updateSaveButtonState();
                this.initializeFormEditMode();
                
                console.log('[RealTimeInsuranceBinding] ✅ State initialized successfully');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error initializing state:', error);
                this.handleError(error, 'initializeState');
            }
        },

        // ========================================
        // FORM EDIT MODE INITIALIZATION - راه‌اندازی حالت ویرایش فرم
        // ========================================
        initializeFormEditMode: function() {
            console.log('[RealTimeInsuranceBinding] Initializing form edit mode...');
            
            try {
                // ذخیره مقادیر اولیه فرم
                this.originalFormValues = this.captureFormValues();
                
                // اضافه کردن event listeners برای تشخیص تغییرات
                this.setupFormChangeDetection();
                
                console.log('[RealTimeInsuranceBinding] ✅ Form edit mode initialized successfully');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error initializing form edit mode:', error);
                this.handleError(error, 'initializeFormEditMode');
            }
        },

        // ========================================
        // CAPTURE FORM VALUES - ذخیره مقادیر فرم
        // ========================================
        captureFormValues: function() {
            console.log('[RealTimeInsuranceBinding] Capturing form values...');
            
            try {
                var formValues = {
                    primaryProvider: $(CONFIG.selectors.primaryProvider).val(),
                    primaryPlan: $(CONFIG.selectors.primaryPlan).val(),
                    primaryPolicyNumber: $(CONFIG.selectors.primaryPolicyNumber).val(),
                    primaryCardNumber: $(CONFIG.selectors.primaryCardNumber).val(),
                    supplementaryProvider: $(CONFIG.selectors.supplementaryProvider).val(),
                    supplementaryPlan: $(CONFIG.selectors.supplementaryPlan).val(),
                    supplementaryPolicyNumber: $(CONFIG.selectors.supplementaryPolicyNumber).val(),
                    supplementaryExpiry: $(CONFIG.selectors.supplementaryExpiry).val()
                };
                
                console.log('[RealTimeInsuranceBinding] Form values captured:', formValues);
                return formValues;
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error capturing form values:', error);
                return {};
            }
        },

        // ========================================
        // SETUP FORM CHANGE DETECTION - راه‌اندازی تشخیص تغییرات فرم
        // ========================================
        setupFormChangeDetection: function() {
            console.log('[RealTimeInsuranceBinding] Setting up form change detection...');
            
            try {
                var self = this;
                
                // Event listener برای تشخیص تغییرات
                $(CONFIG.selectors.insuranceSection + ' input, ' + CONFIG.selectors.insuranceSection + ' select').on('change.formEditMode', function() {
                    self.handleFormChange();
                });
                
                console.log('[RealTimeInsuranceBinding] ✅ Form change detection setup successfully');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error setting up form change detection:', error);
                this.handleError(error, 'setupFormChangeDetection');
            }
        },

        // ========================================
        // HANDLE FORM CHANGE - مدیریت تغییرات فرم
        // ========================================
        handleFormChange: function() {
            console.log('[RealTimeInsuranceBinding] Form change detected');
            
            try {
                var currentValues = this.captureFormValues();
                var hasChanges = this.detectFormChanges(this.originalFormValues, currentValues);
                
                if (hasChanges) {
                    console.log('[RealTimeInsuranceBinding] Form has changes, enabling edit mode');
                    this.enableEditMode();
                } else {
                    console.log('[RealTimeInsuranceBinding] No form changes detected');
                    this.disableEditMode();
                }
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error handling form change:', error);
                this.handleError(error, 'handleFormChange');
            }
        },

        // ========================================
        // DETECT FORM CHANGES - تشخیص تغییرات فرم
        // ========================================
        detectFormChanges: function(originalValues, currentValues) {
            console.log('[RealTimeInsuranceBinding] Detecting form changes...');
            
            try {
                var changes = [];
                
                // بررسی تغییرات بیمه پایه
                if (originalValues.primaryProvider !== currentValues.primaryProvider) {
                    changes.push('Primary Provider');
                }
                if (originalValues.primaryPlan !== currentValues.primaryPlan) {
                    changes.push('Primary Plan');
                }
                if (originalValues.primaryPolicyNumber !== currentValues.primaryPolicyNumber) {
                    changes.push('Primary Policy Number');
                }
                if (originalValues.primaryCardNumber !== currentValues.primaryCardNumber) {
                    changes.push('Primary Card Number');
                }
                
                // بررسی تغییرات بیمه تکمیلی
                if (originalValues.supplementaryProvider !== currentValues.supplementaryProvider) {
                    changes.push('Supplementary Provider');
                }
                if (originalValues.supplementaryPlan !== currentValues.supplementaryPlan) {
                    changes.push('Supplementary Plan');
                }
                if (originalValues.supplementaryPolicyNumber !== currentValues.supplementaryPolicyNumber) {
                    changes.push('Supplementary Policy Number');
                }
                if (originalValues.supplementaryExpiry !== currentValues.supplementaryExpiry) {
                    changes.push('Supplementary Expiry');
                }
                
                console.log('[RealTimeInsuranceBinding] Changes detected:', changes);
                return changes.length > 0;
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error detecting form changes:', error);
                return false;
            }
        },

        // ========================================
        // ENABLE EDIT MODE - فعال کردن حالت ویرایش
        // ========================================
        enableEditMode: function() {
            console.log('[RealTimeInsuranceBinding] Enabling edit mode...');
            
            try {
                // فعال کردن دکمه ذخیره
                $(CONFIG.selectors.saveInsuranceBtn).prop('disabled', false).removeClass('btn-secondary').addClass('btn-success');
                
                // نمایش پیام ویرایش
                this.showInfo('تغییرات در فرم تشخیص داده شد. برای ذخیره تغییرات روی دکمه ذخیره کلیک کنید.');
                
                // اضافه کردن کلاس ویرایش به فرم
                $(CONFIG.selectors.insuranceSection).addClass('form-editing');
                
                console.log('[RealTimeInsuranceBinding] ✅ Edit mode enabled');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error enabling edit mode:', error);
                this.handleError(error, 'enableEditMode');
            }
        },

        // ========================================
        // DISABLE EDIT MODE - غیرفعال کردن حالت ویرایش
        // ========================================
        disableEditMode: function() {
            console.log('[RealTimeInsuranceBinding] Disabling edit mode...');
            
            try {
                // غیرفعال کردن دکمه ذخیره
                $(CONFIG.selectors.saveInsuranceBtn).prop('disabled', true).removeClass('btn-success').addClass('btn-secondary');
                
                // حذف کلاس ویرایش از فرم
                $(CONFIG.selectors.insuranceSection).removeClass('form-editing');
                
                console.log('[RealTimeInsuranceBinding] ✅ Edit mode disabled');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error disabling edit mode:', error);
                this.handleError(error, 'disableEditMode');
            }
        },

        // ========================================
        // PERFORMANCE MONITORING - نظارت بر عملکرد
        // ========================================
        setupPerformanceMonitoring: function() {
            console.log('[RealTimeInsuranceBinding] Setting up performance monitoring...');
            
            try {
                // Monitor AJAX performance
                var self = this;
                
                $(document).ajaxStart(function() {
                    self.performanceStartTime = Date.now();
                    console.log('[RealTimeInsuranceBinding] AJAX request started');
                });
                
                $(document).ajaxComplete(function() {
                    if (self.performanceStartTime) {
                        var duration = Date.now() - self.performanceStartTime;
                        console.log('[RealTimeInsuranceBinding] AJAX request completed in', duration, 'ms');
                    }
                });
                
                console.log('[RealTimeInsuranceBinding] ✅ Performance monitoring setup successfully');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error setting up performance monitoring:', error);
                this.handleError(error, 'setupPerformanceMonitoring');
            }
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
