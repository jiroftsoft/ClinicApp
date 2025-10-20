/**
 * REAL-TIME INSURANCE BINDING COORDINATOR - هماهنگ‌کننده ماژول‌های بیمه
 * ====================================================================
 * 
 * این ماژول مسئولیت‌های زیر را دارد:
 * - هماهنگی بین ماژول‌های تخصصی بیمه
 * - مدیریت رویدادهای cross-module
 * - اتصال خودکار اطلاعات بیمه پس از جستجوی بیمار
 * - بهینه‌سازی Performance برای Production
 * 
 * @author ClinicApp Development Team
 * @version 2.0.0
 * @since 2025-01-17
 */

(function(global, $) {
    'use strict';

    // Simple, GC-friendly debounce suitable for production UIs
    function debounce(func, wait) {
        var timeoutId;
        return function() {
            var context = this;
            var args = arguments;
            clearTimeout(timeoutId);
            timeoutId = setTimeout(function() {
                func.apply(context, args);
            }, wait);
        };
    }

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
        // INITIALIZATION - راه‌اندازی (Coordinator)
        // ========================================
        init: function() {
            console.log('[RealTimeInsuranceBinding] 🎯 Initializing Insurance Coordinator...');
            
            try {
                // 1. Initialize specialized modules
                this.initializeSpecializedModules();
                
                // 2. Setup cross-module communication
                this.setupCrossModuleCommunication();
                
                // 3. Initialize coordinator state
                this.initializeCoordinatorState();
                
                // 4. Setup performance monitoring
                this.setupPerformanceMonitoring();
                
                console.log('[RealTimeInsuranceBinding] ✅ Insurance Coordinator initialized successfully');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Coordinator initialization failed:', error);
                this.handleError(error, 'init');
            }
        },

        // ========================================
        // INITIALIZE SPECIALIZED MODULES - مقداردهی ماژول‌های تخصصی
        // ========================================
        initializeSpecializedModules: function() {
            console.log('[RealTimeInsuranceBinding] 🔧 Initializing specialized modules...');
            
            try {
                // Initialize ValidationEngine
                if (window.ValidationEngine) {
                    window.ValidationEngine.init();
                    console.log('[RealTimeInsuranceBinding] ✅ ValidationEngine initialized');
                }
                
                // Initialize SaveProcessor
                if (window.SaveProcessor) {
                    window.SaveProcessor.init();
                    console.log('[RealTimeInsuranceBinding] ✅ SaveProcessor initialized');
                }
                
                // Initialize FormChangeDetector
                if (window.FormChangeDetector) {
                    window.FormChangeDetector.init();
                    console.log('[RealTimeInsuranceBinding] ✅ FormChangeDetector initialized');
                }
                
                // Initialize EditModeManager
                if (window.EditModeManager) {
                    window.EditModeManager.init();
                    console.log('[RealTimeInsuranceBinding] ✅ EditModeManager initialized');
                }
                
                // Initialize InsuranceOrchestrator
                if (window.InsuranceOrchestrator) {
                    window.InsuranceOrchestrator.init();
                    console.log('[RealTimeInsuranceBinding] ✅ InsuranceOrchestrator initialized');
                }
                
                console.log('[RealTimeInsuranceBinding] ✅ All specialized modules initialized');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error initializing specialized modules:', error);
                throw error;
            }
        },

        // ========================================
        // SETUP CROSS-MODULE COMMUNICATION - تنظیم ارتباطات cross-module
        // ========================================
        setupCrossModuleCommunication: function() {
            console.log('[RealTimeInsuranceBinding] 🔗 Setting up cross-module communication...');
            
            try {
                // Setup EventBus integration
                if (window.ReceptionEventBus) {
                    this.setupEventBusIntegration();
                }
                
                // Setup legacy event handling
                this.setupLegacyEventHandling();
                
                console.log('[RealTimeInsuranceBinding] ✅ Cross-module communication setup completed');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error setting up cross-module communication:', error);
                throw error;
            }
        },

        // ========================================
        // SETUP EVENT BUS INTEGRATION - تنظیم یکپارچگی EventBus
        // ========================================
        setupEventBusIntegration: function() {
            console.log('[RealTimeInsuranceBinding] 📡 Setting up EventBus integration...');
            
            try {
                var self = this;
                
                // Patient search success
                window.ReceptionEventBus.on('patient:searchSuccess', function(data) {
                    console.log('[RealTimeInsuranceBinding] 👤 Patient search success:', data);
                    self.handlePatientSearchSuccess(data);
                });
                
                // Insurance load success
                window.ReceptionEventBus.on('insurance:loadSuccess', function(data) {
                    console.log('[RealTimeInsuranceBinding] 🏥 Insurance load success:', data);
                    self.handleInsuranceLoadSuccess(data);
                });
                
                // Insurance changed
                window.ReceptionEventBus.on('insurance:changed', function(data) {
                    console.log('[RealTimeInsuranceBinding] 🔄 Insurance changed:', data);
                    self.handleInsuranceChanged(data);
                });
                
                // Insurance saved
                window.ReceptionEventBus.on('insurance:saved', function(data) {
                    console.log('[RealTimeInsuranceBinding] ✅ Insurance saved:', data);
                    self.handleInsuranceSaved(data);
                });
                
                console.log('[RealTimeInsuranceBinding] ✅ EventBus integration setup completed');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error setting up EventBus integration:', error);
                throw error;
            }
        },

        // ========================================
        // SETUP LEGACY EVENT HANDLING - تنظیم مدیریت رویدادهای legacy
        // ========================================
        setupLegacyEventHandling: function() {
            console.log('[RealTimeInsuranceBinding] 🔄 Setting up legacy event handling...');
            
            try {
                var self = this;
                
                // Legacy patient search success
                $(document).on('patientSearchSuccess', function(e, data) {
                    console.log('[RealTimeInsuranceBinding] 👤 Legacy patient search success:', data);
                    self.handlePatientSearchSuccess(data);
                });
                
                // Legacy insurance load success
                $(document).on('insuranceLoadSuccess', function(e, data) {
                    console.log('[RealTimeInsuranceBinding] 🏥 Legacy insurance load success:', data);
                    self.handleInsuranceLoadSuccess(data);
                });
                
                console.log('[RealTimeInsuranceBinding] ✅ Legacy event handling setup completed');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error setting up legacy event handling:', error);
                throw error;
            }
        },

        // ========================================
        // INITIALIZE COORDINATOR STATE - مقداردهی وضعیت coordinator
        // ========================================
        initializeCoordinatorState: function() {
            console.log('[RealTimeInsuranceBinding] 🔧 Initializing coordinator state...');
            
            try {
                this._isInitialized = true;
                this._patientId = null;
                this._insuranceData = null;
                this._isLoading = false;
                this._isSaving = false;
                
                console.log('[RealTimeInsuranceBinding] ✅ Coordinator state initialized');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error initializing coordinator state:', error);
                throw error;
            }
        },

        // ========================================
        // EVENT BINDING - اتصال رویدادها (Memory-Safe)
        // ========================================
        bindEvents: function() {
            console.log('[RealTimeInsuranceBinding] 🔗 Binding events...');
            
            try {
                var self = this;
                
                // 1. PATIENT SEARCH SUCCESS EVENT - رویداد موفقیت جستجوی بیمار
                var patientSearchHandler = function(event, patientData) {
                    console.log('[RealTimeInsuranceBinding] Patient search success detected:', patientData);
                    self.handlePatientSearchSuccess(patientData);
                };
                $(document).on('patientSearchSuccess.realtimeInsurance', patientSearchHandler);
                this._eventListeners.push({
                    element: $(document),
                    event: 'patientSearchSuccess.realtimeInsurance',
                    handler: patientSearchHandler
                });
                
                // 2. PRIMARY PROVIDER CHANGE EVENT - رویداد تغییر ارائه‌دهنده پایه
                var primaryProviderHandler = function() {
                    console.log('[RealTimeInsuranceBinding] Primary provider changed');
                    if (self.suppressFormChange) { console.log('[RealTimeInsuranceBinding] Change suppressed'); return; }
                    self.handlePrimaryProviderChange();
                };
                $(CONFIG.selectors.primaryProvider).on('change.realtimeInsurance', primaryProviderHandler);
                this._eventListeners.push({
                    element: $(CONFIG.selectors.primaryProvider),
                    event: 'change.realtimeInsurance',
                    handler: primaryProviderHandler
                });
                
                // 3. SUPPLEMENTARY PROVIDER CHANGE EVENT - رویداد تغییر ارائه‌دهنده تکمیلی
                var supplementaryProviderHandler = function() {
                    console.log('[RealTimeInsuranceBinding] Supplementary provider changed');
                    if (self.suppressFormChange) { console.log('[RealTimeInsuranceBinding] Change suppressed'); return; }
                    self.handleSupplementaryProviderChange();
                };
                $(CONFIG.selectors.supplementaryProvider).on('change.realtimeInsurance', supplementaryProviderHandler);
                this._eventListeners.push({
                    element: $(CONFIG.selectors.supplementaryProvider),
                    event: 'change.realtimeInsurance',
                    handler: supplementaryProviderHandler
                });
                
                // 4. LOAD INSURANCE BUTTON EVENT - رویداد دکمه بارگذاری بیمه
                var loadInsuranceHandler = function() {
                    console.log('[RealTimeInsuranceBinding] Load insurance button clicked');
                    self.loadPatientInsurance();
                };
                $(CONFIG.selectors.loadInsuranceBtn).on('click.realtimeInsurance', loadInsuranceHandler);
                this._eventListeners.push({
                    element: $(CONFIG.selectors.loadInsuranceBtn),
                    event: 'click.realtimeInsurance',
                    handler: loadInsuranceHandler
                });
                
                // 5. SAVE INSURANCE BUTTON EVENT - رویداد دکمه ذخیره بیمه
                var saveInsuranceHandler = function() {
                    console.log('[RealTimeInsuranceBinding] Save insurance button clicked');
                    self.savePatientInsurance();
                };
                $(CONFIG.selectors.saveInsuranceBtn).on('click.realtimeInsurance', saveInsuranceHandler);
                this._eventListeners.push({
                    element: $(CONFIG.selectors.saveInsuranceBtn),
                    event: 'click.realtimeInsurance',
                    handler: saveInsuranceHandler
                });
                
                // 6. FORM CHANGE DETECTION EVENTS - رویدادهای تشخیص تغییر فرم
                this.bindFormChangeEvents();
                
                // 7. SELECT2 EVENTS (if available) - رویدادهای Select2
                this.bindSelect2Events();
                
                console.log('[RealTimeInsuranceBinding] ✅ Events bound successfully');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error binding events:', error);
                this.handleCriticalError(error, 'bindEvents');
            }
        },

        // ========================================
        // BIND FORM CHANGE EVENTS - اتصال رویدادهای تغییر فرم (Production-Optimized)
        // ========================================
        bindFormChangeEvents: function() {
            console.log('[RealTimeInsuranceBinding] 🔗 Binding form change events...');
            
            try {
                var self = this;
                
                // SPECIFIC FORM ELEMENTS - عناصر مشخص فرم
                var insuranceFields = [
                    CONFIG.selectors.primaryProvider,
                    CONFIG.selectors.primaryPlan,
                    CONFIG.selectors.primaryPolicyNumber,
                    CONFIG.selectors.primaryCardNumber,
                    CONFIG.selectors.supplementaryProvider,
                    CONFIG.selectors.supplementaryPlan,
                    CONFIG.selectors.supplementaryPolicyNumber,
                    CONFIG.selectors.supplementaryExpiry
                ];
                
                // Change events for specific form elements
                var formChangeHandler = function() {
                    console.log('[RealTimeInsuranceBinding] 🔄 Form field changed:', this.id);
                    if (self.suppressFormChange) { 
                        console.log('[RealTimeInsuranceBinding] ⚠️ Change suppressed'); 
                        return; 
                    }
                    self.handleInsuranceFormChange();
                };
                
                // Input events for real-time detection
                var formInputHandler = function() {
                    console.log('[RealTimeInsuranceBinding] 🔄 Form field input:', this.id);
                    if (self.suppressFormChange) { 
                        console.log('[RealTimeInsuranceBinding] ⚠️ Input suppressed'); 
                        return; 
                    }
                    self.handleInsuranceFormChange();
                };
                
                // Bind events to each field
                insuranceFields.forEach(function(selector) {
                    var $element = $(selector);
                    if ($element.length > 0) {
                        // Change event
                        $element.on('change.formEditMode', formChangeHandler);
                        this._eventListeners.push({
                            element: $element,
                            event: 'change.formEditMode',
                            handler: formChangeHandler
                        });
                        
                        // Input event for real-time detection
                        $element.on('input.formEditMode', formInputHandler);
                        this._eventListeners.push({
                            element: $element,
                            event: 'input.formEditMode',
                            handler: formInputHandler
                        });
                        
                        console.log('[RealTimeInsuranceBinding] ✅ Events bound to:', selector);
                    } else {
                        console.warn('[RealTimeInsuranceBinding] ⚠️ Element not found:', selector);
                    }
                }.bind(this));
                
                console.log('[RealTimeInsuranceBinding] ✅ Form change events bound successfully');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error binding form change events:', error);
                this.handleCriticalError(error, 'bindFormChangeEvents');
            }
        },

        // ========================================
        // BIND SELECT2 EVENTS - اتصال رویدادهای Select2
        // ========================================
        bindSelect2Events: function() {
            console.log('[RealTimeInsuranceBinding] 🔗 Binding Select2 events...');
            
            try {
                if (!$.fn.select2) {
                    console.log('[RealTimeInsuranceBinding] ℹ️ Select2 not available, skipping Select2 events');
                    return;
                }
                
                var self = this;
                
                // Supplementary Provider Select2 events
                var supplementaryProviderSelect2Handler = function() {
                    console.log('[RealTimeInsuranceBinding] Supplementary provider cleared via Select2');
                    if (self.suppressFormChange) { console.log('[RealTimeInsuranceBinding] Select2 clear suppressed'); return; }
                    self.handleInsuranceFormChange();
                };
                
                var supplementaryProvider = $(CONFIG.selectors.supplementaryProvider);
                supplementaryProvider.on('select2:clear.realtimeInsurance select2:unselect.realtimeInsurance', supplementaryProviderSelect2Handler);
                this._eventListeners.push({
                    element: supplementaryProvider,
                    event: 'select2:clear.realtimeInsurance select2:unselect.realtimeInsurance',
                    handler: supplementaryProviderSelect2Handler
                });
                
                // Supplementary Plan Select2 events
                var supplementaryPlanSelect2Handler = function() {
                    console.log('[RealTimeInsuranceBinding] Supplementary plan cleared via Select2');
                    if (self.suppressFormChange) { console.log('[RealTimeInsuranceBinding] Select2 clear suppressed'); return; }
                    self.handleInsuranceFormChange();
                };
                
                var supplementaryPlan = $(CONFIG.selectors.supplementaryPlan);
                supplementaryPlan.on('select2:clear.realtimeInsurance select2:unselect.realtimeInsurance', supplementaryPlanSelect2Handler);
                this._eventListeners.push({
                    element: supplementaryPlan,
                    event: 'select2:clear.realtimeInsurance select2:unselect.realtimeInsurance',
                    handler: supplementaryPlanSelect2Handler
                });
                
                console.log('[RealTimeInsuranceBinding] ✅ Select2 events bound');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error binding Select2 events:', error);
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
                        
                        // CRITICAL: Set original values for change detection
                        this.setOriginalValuesForChangeDetection();
                        
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
        // SET ORIGINAL VALUES FOR CHANGE DETECTION - تنظیم مقادیر اولیه برای تشخیص تغییرات (Production-Optimized)
        // ========================================
        setOriginalValuesForChangeDetection: function() {
            console.log('[RealTimeInsuranceBinding] 📝 Setting original values for change detection...');
            
            try {
                var self = this;
                
                // Wait for DOM to be fully updated and plans to be loaded
                setTimeout(function() {
                    if (window.FormChangeDetector) {
                        // Use the new method to update original values from current form
                        window.FormChangeDetector.updateOriginalValuesFromCurrentForm();
                        console.log('[RealTimeInsuranceBinding] ✅ Original values updated from current form');
                        
                        // Also set in coordinator for backup
                        if (window.RealTimeInsuranceBinding) {
                            var currentValues = window.FormChangeDetector.captureFormValues();
                            window.RealTimeInsuranceBinding._originalValues = currentValues;
                            console.log('[RealTimeInsuranceBinding] ✅ Original values backed up in coordinator');
                        }
                        
                        // Disable edit mode after setting original values
                        if (window.EditModeManager) {
                            window.EditModeManager.disableEditMode();
                            console.log('[RealTimeInsuranceBinding] ✅ Edit mode disabled after setting original values');
                        }
                    }
                }, 2000); // Increased timeout for plan loading
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error setting original values:', error);
            }
        },

        // ========================================
        // CAPTURE FORM VALUES - دریافت مقادیر فرم (Production-Optimized)
        // ========================================
        captureFormValues: function() {
            console.log('[RealTimeInsuranceBinding] 📊 Capturing form values...');
            
            try {
                var values = {
                    primaryProvider: $('#insuranceProvider').val() || '',
                    primaryPlan: $('#insurancePlan').val() || '',
                    primaryPolicyNumber: $('#policyNumber').val() || '',
                    primaryCardNumber: $('#cardNumber').val() || '',
                    supplementaryProvider: $('#supplementaryProvider').val() || '',
                    supplementaryPlan: $('#supplementaryPlan').val() || '',
                    supplementaryPolicyNumber: $('#supplementaryPolicyNumber').val() || '',
                    supplementaryExpiry: $('#supplementaryExpiry').val() || ''
                };
                
                console.log('[RealTimeInsuranceBinding] 📊 Form values captured:', values);
                return values;
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error capturing form values:', error);
                return {};
            }
        },

        // ========================================
        // HAS FORM CHANGED - بررسی تغییرات فرم (Production-Optimized)
        // ========================================
        hasFormChanged: function() {
            console.log('[RealTimeInsuranceBinding] 🔍 Coordinating form change check...');
            
            try {
                if (window.FormChangeDetector) {
                    return window.FormChangeDetector.hasFormChanged();
                }
                
                console.warn('[RealTimeInsuranceBinding] ⚠️ FormChangeDetector not available');
                return false;
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error coordinating form change check:', error);
                return false;
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
                
                // Prevent event storms while binding programmatically
                this.suppressFormChange = true;

                // Set provider
                if (primaryInsurance.ProviderId) {
                    var $provider = $(CONFIG.selectors.primaryProvider);
                    console.log('[RealTimeInsuranceBinding] Setting provider:', primaryInsurance.ProviderId);
                    if ($provider.length > 0) {
                        // بررسی وجود option در select
                        var optionExists = $provider.find('option[value="' + primaryInsurance.ProviderId + '"]').length > 0;
                        if (optionExists) {
                            $provider.val(primaryInsurance.ProviderId);
                            console.log('[RealTimeInsuranceBinding] ✅ Provider set:', primaryInsurance.ProviderId);
                            // Manually load plans to avoid relying on change handlers
                            this.loadPrimaryInsurancePlans(primaryInsurance.ProviderId);
                        } else {
                            console.warn('[RealTimeInsuranceBinding] Provider option not found in select, waiting for options to load...');
                            // انتظار برای بارگذاری options
                            var self = this;
                            setTimeout(function() {
                                var optionExists = $provider.find('option[value="' + primaryInsurance.ProviderId + '"]').length > 0;
                                if (optionExists) {
                                    $provider.val(primaryInsurance.ProviderId);
                                    console.log('[RealTimeInsuranceBinding] ✅ Provider set after delay:', primaryInsurance.ProviderId);
                                    self.loadPrimaryInsurancePlans(primaryInsurance.ProviderId);
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
                            $plan.val(primaryInsurance.PlanId);
                            console.log('[RealTimeInsuranceBinding] ✅ Plan set:', primaryInsurance.PlanId);
                        } else {
                            console.warn('[RealTimeInsuranceBinding] Plan option not found in select, waiting for options to load...');
                            // انتظار برای بارگذاری options
                            setTimeout(function() {
                                var optionExists = $plan.find('option[value="' + primaryInsurance.PlanId + '"]').length > 0;
                                if (optionExists) {
                                    $plan.val(primaryInsurance.PlanId);
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
                
                // Re-enable events and publish a consolidated debounced change event
                this.suppressFormChange = false;
                if (typeof this.handleInsuranceFormChangeDebounced === 'function') {
                    this.handleInsuranceFormChangeDebounced();
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
                
                // Prevent event storms while binding programmatically
                this.suppressFormChange = true;

                // Set provider
                if (supplementaryInsurance.ProviderId) {
                    var $provider = $(CONFIG.selectors.supplementaryProvider);
                    console.log('[RealTimeInsuranceBinding] Setting supplementary provider:', supplementaryInsurance.ProviderId);
                    if ($provider.length > 0) {
                        // بررسی وجود option در select
                        var optionExists = $provider.find('option[value="' + supplementaryInsurance.ProviderId + '"]').length > 0;
                        if (optionExists) {
                            $provider.val(supplementaryInsurance.ProviderId);
                            console.log('[RealTimeInsuranceBinding] ✅ Supplementary provider set:', supplementaryInsurance.ProviderId);
                            this.loadSupplementaryInsurancePlans(supplementaryInsurance.ProviderId);
                        } else {
                            console.warn('[RealTimeInsuranceBinding] Supplementary provider option not found in select, waiting for options to load...');
                            // انتظار برای بارگذاری options
                            var self = this;
                            setTimeout(function() {
                                var optionExists = $provider.find('option[value="' + supplementaryInsurance.ProviderId + '"]').length > 0;
                                if (optionExists) {
                                    $provider.val(supplementaryInsurance.ProviderId);
                                    console.log('[RealTimeInsuranceBinding] ✅ Supplementary provider set after delay:', supplementaryInsurance.ProviderId);
                                    self.loadSupplementaryInsurancePlans(supplementaryInsurance.ProviderId);
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
                            $plan.val(supplementaryInsurance.PlanId);
                            console.log('[RealTimeInsuranceBinding] ✅ Supplementary plan set:', supplementaryInsurance.PlanId);
                        } else {
                            console.warn('[RealTimeInsuranceBinding] Supplementary plan option not found in select, waiting for options to load...');
                            // انتظار برای بارگذاری options
                            setTimeout(function() {
                                var optionExists = $plan.find('option[value="' + supplementaryInsurance.PlanId + '"]').length > 0;
                                if (optionExists) {
                                    $plan.val(supplementaryInsurance.PlanId);
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
                
                // Re-enable events and publish a consolidated debounced change event
                this.suppressFormChange = false;
                if (typeof this.handleInsuranceFormChangeDebounced === 'function') {
                    this.handleInsuranceFormChangeDebounced();
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
                this.suppressFormChange = true;
                // پاک کردن provider
                $(CONFIG.selectors.supplementaryProvider).val('');
                // Trigger both native and plugin events
                $(CONFIG.selectors.supplementaryProvider).trigger('input');
                
                // پاک کردن plan
                $(CONFIG.selectors.supplementaryPlan)
                    .empty()
                    .append('<option value="">انتخاب طرح بیمه تکمیلی...</option>')
                    .trigger('input');
                
                // پاک کردن policy number
                $(CONFIG.selectors.supplementaryPolicyNumber).val('').trigger('input');
                
                // پاک کردن expiry date
                $(CONFIG.selectors.supplementaryExpiry).val('').trigger('input');
                
                // اطمینان از فعال شدن ویرایش پس از پاکسازی
                this.suppressFormChange = false;
                if (typeof this.handleInsuranceFormChangeDebounced === 'function') {
                    this.handleInsuranceFormChangeDebounced();
                } else {
                    this.handleInsuranceFormChange();
                }

                console.log('[RealTimeInsuranceBinding] ✅ Supplementary insurance fields cleared');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error clearing supplementary insurance fields:', error);
                this.handleError(error, 'clearSupplementaryInsuranceFields');
            }
        },

        // ========================================
        // INSURANCE FORM CHANGE HANDLER - مدیریت تغییر فرم بیمه (Production-Optimized)
        // ========================================
        handleInsuranceFormChange: function() {
            console.log('[RealTimeInsuranceBinding] 🔄 Insurance form changed');
            
            try {
                // 1. Check if form has changes
                var hasChanges = this.hasFormChanged();
                console.log('[RealTimeInsuranceBinding] 📊 Form has changes:', hasChanges);
                
                if (!hasChanges) {
                    console.log('[RealTimeInsuranceBinding] ❌ No changes detected, skipping update');
                    return;
                }
                
                // 2. Update form state
                this.updateFormState();
                
                // 3. Update save button state
                this.updateSaveButtonState();
                
                // 4. Publish standardized insurance:changed event for legacy modules
                this.publishInsuranceChanged();
                
                // 5. Show user feedback
                this.showChangeDetectedMessage();
                
                console.log('[RealTimeInsuranceBinding] ✅ Form change handled successfully');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error handling form change:', error);
                this.handleError(error, 'handleInsuranceFormChange');
            }
        },

        // ========================================
        // SHOW CHANGE DETECTED MESSAGE - نمایش پیام تشخیص تغییرات
        // ========================================
        showChangeDetectedMessage: function() {
            console.log('[RealTimeInsuranceBinding] 📢 Showing change detected message...');
            
            try {
                var message = 'تغییرات در فرم تشخیص داده شد. برای ذخیره تغییرات روی دکمه ذخیره کلیک کنید.';
                
                // Use Toastr if available
                if (window.ReceptionToastr && window.ReceptionToastr.helpers && window.ReceptionToastr.helpers.showInfo) {
                    window.ReceptionToastr.helpers.showInfo(message);
                } else {
                    console.log('[RealTimeInsuranceBinding] ℹ️ Info:', message);
                }
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error showing change detected message:', error);
            }
        },

        // ========================================
        // PUBLISH INSURANCE CHANGED EVENT - انتشار رویداد تغییر بیمه
        // ========================================
        publishInsuranceChanged: function() {
            try {
                // Collect current form data
                var formData = {
                    primaryProvider: $(CONFIG.selectors.primaryProvider).val(),
                    primaryPlan: $(CONFIG.selectors.primaryPlan).val(),
                    primaryPolicyNumber: $(CONFIG.selectors.primaryPolicyNumber).val(),
                    primaryCardNumber: $(CONFIG.selectors.primaryCardNumber).val(),
                    supplementaryProvider: $(CONFIG.selectors.supplementaryProvider).val(),
                    supplementaryPlan: $(CONFIG.selectors.supplementaryPlan).val(),
                    supplementaryPolicyNumber: $(CONFIG.selectors.supplementaryPolicyNumber).val(),
                    supplementaryExpiry: $(CONFIG.selectors.supplementaryExpiry).val()
                };

                // Publish via EventBus (preferred) or jQuery fallback
                if (window.ReceptionEventBus) {
                    window.ReceptionEventBus.emit('insurance:changed', formData);
                    console.log('[RealTimeInsuranceBinding] ✅ Insurance changed event published via EventBus');
                } else {
                    $(document).trigger('insuranceFormChanged', formData);
                    console.log('[RealTimeInsuranceBinding] ✅ Insurance changed event published via jQuery');
                }
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] Error publishing insurance changed event:', error);
            }
        },

        // ========================================
        // SAVE PATIENT INSURANCE - ذخیره اطلاعات بیمه بیمار (Production-Optimized)
        // ========================================
        savePatientInsurance: function() {
            var self = this;
            var startTime = performance.now();
            
            try {
                // 1. VALIDATION LAYER - لایه اعتبارسنجی
                var validationResult = this.performComprehensiveValidation();
                if (!validationResult.isValid) {
                    this.showValidationErrors(validationResult.errors);
                    return;
                }
                
                var patientId = validationResult.patientId;
                console.log('[RealTimeInsuranceBinding] 💾 Starting save process for patient:', patientId);

                // 2. CONCURRENCY PROTECTION - محافظت از تداخل
                if (this._isSaving) {
                    console.warn('[RealTimeInsuranceBinding] ⚠️ Save already in progress, queuing request');
                    this.queueSaveRequest();
                    return;
                }
                
                // 3. PERFORMANCE MONITORING - نظارت بر عملکرد
                this.startPerformanceMonitoring('save');
                
                // 4. UI STATE MANAGEMENT - مدیریت وضعیت UI
                this.setSaveInProgressState();
                
                // 5. DATA COLLECTION & SANITIZATION - جمع‌آوری و پاکسازی داده
                var insuranceData = this.collectAndSanitizeInsuranceData();
                
                // 6. REQUEST PREPARATION - آماده‌سازی درخواست
                var requestData = this.prepareSaveRequest(patientId, insuranceData);
                
                // 7. AJAX REQUEST WITH ENHANCED ERROR HANDLING - درخواست AJAX با مدیریت خطای پیشرفته
                this.executeSaveRequest(requestData, startTime);
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Critical error in save process:', error);
                this.handleCriticalError(error, 'savePatientInsurance');
            }
        },

        // ========================================
        // COMPREHENSIVE VALIDATION - اعتبارسنجی جامع
        // ========================================
        performComprehensiveValidation: function() {
            console.log('[RealTimeInsuranceBinding] 🔍 Performing comprehensive validation...');
            
            try {
                var errors = [];
                var patientId = $(CONFIG.selectors.patientId).val();
                
                // 1. Patient ID Validation
                if (!patientId || patientId <= 0) {
                    errors.push({
                        field: 'patientId',
                        message: 'شناسه بیمار نامعتبر است',
                        severity: 'error'
                    });
                }
                
                // 2. Form Data Validation
                var formValidation = this.validateInsuranceForm();
                if (!formValidation.isValid) {
                    formValidation.errors.forEach(function(error) {
                        errors.push({
                            field: 'form',
                            message: error,
                            severity: 'error'
                        });
                    });
                }
                
                // 3. Business Logic Validation
                var businessValidation = this.validateBusinessRules();
                if (!businessValidation.isValid) {
                    businessValidation.errors.forEach(function(error) {
                        errors.push({
                            field: 'business',
                            message: error,
                            severity: 'warning'
                        });
                    });
                }
                
                // 4. Network State Validation
                if (!navigator.onLine) {
                    errors.push({
                        field: 'network',
                        message: 'اتصال اینترنت قطع است',
                        severity: 'error'
                    });
                }
                
                console.log('[RealTimeInsuranceBinding] 📊 Validation result:', {
                    isValid: errors.filter(e => e.severity === 'error').length === 0,
                    errors: errors,
                    patientId: patientId
                });
                
                return {
                    isValid: errors.filter(e => e.severity === 'error').length === 0,
                    errors: errors,
                    patientId: patientId
                };
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Validation error:', error);
                return {
                    isValid: false,
                    errors: [{ field: 'system', message: 'خطا در اعتبارسنجی', severity: 'error' }],
                    patientId: null
                };
            }
        },

        // ========================================
        // BUSINESS RULES VALIDATION - اعتبارسنجی قوانین کسب‌وکار
        // ========================================
        validateBusinessRules: function() {
            console.log('[RealTimeInsuranceBinding] 🏥 Validating business rules...');
            
            try {
                var errors = [];
                var primaryProvider = $(CONFIG.selectors.primaryProvider).val();
                var supplementaryProvider = $(CONFIG.selectors.supplementaryProvider).val();
                
                // Rule 1: Primary insurance is mandatory
                if (!primaryProvider) {
                    errors.push('بیمه پایه الزامی است');
                }
                
                // Rule 2: Supplementary insurance cannot be same as primary
                if (primaryProvider && supplementaryProvider && primaryProvider === supplementaryProvider) {
                    errors.push('بیمه تکمیلی نمی‌تواند همان بیمه پایه باشد');
                }
                
                // Rule 3: Policy number format validation
                var primaryPolicyNumber = $(CONFIG.selectors.primaryPolicyNumber).val();
                if (primaryPolicyNumber && !this.isValidPolicyNumber(primaryPolicyNumber)) {
                    errors.push('شماره بیمه پایه نامعتبر است');
                }
                
                return {
                    isValid: errors.length === 0,
                    errors: errors
                };
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Business rules validation error:', error);
                return {
                    isValid: false,
                    errors: ['خطا در اعتبارسنجی قوانین کسب‌وکار']
                };
            }
        },

        // ========================================
        // POLICY NUMBER VALIDATION - اعتبارسنجی شماره بیمه
        // ========================================
        isValidPolicyNumber: function(policyNumber) {
            // Iranian insurance policy number format validation
            var iranianPolicyPattern = /^[0-9]{8,15}$/;
            return iranianPolicyPattern.test(policyNumber);
        },

        // ========================================
        // SAVE REQUEST QUEUE - صف درخواست‌های ذخیره
        // ========================================
        queueSaveRequest: function() {
            if (!this._saveQueue) {
                this._saveQueue = [];
            }
            
            this._saveQueue.push({
                timestamp: Date.now(),
                retryCount: 0,
                maxRetries: 3
            });
            
            console.log('[RealTimeInsuranceBinding] 📋 Save request queued. Queue length:', this._saveQueue.length);
            
            // Process queue after current save completes
            var queueTimeout = setTimeout(function() {
                if (self._saveQueue && self._saveQueue.length > 0 && !self._isSaving) {
                    self.processSaveQueue();
                }
            }, 1000);
            this._timeouts.push(queueTimeout);
        },

        // ========================================
        // PROCESS SAVE QUEUE - پردازش صف ذخیره
        // ========================================
        processSaveQueue: function() {
            if (!this._saveQueue || this._saveQueue.length === 0) {
                return;
            }
            
            var queuedRequest = this._saveQueue.shift();
            console.log('[RealTimeInsuranceBinding] 🔄 Processing queued save request');
            
            // Retry logic with exponential backoff
            if (queuedRequest.retryCount < queuedRequest.maxRetries) {
                queuedRequest.retryCount++;
                var retryTimeout = setTimeout(function() {
                    self.savePatientInsurance();
                }, Math.pow(2, queuedRequest.retryCount) * 1000);
                this._timeouts.push(retryTimeout);
            } else {
                console.error('[RealTimeInsuranceBinding] ❌ Max retries exceeded for queued save request');
                this.showError('خطا در ذخیره اطلاعات. لطفاً دوباره تلاش کنید.');
            }
        },

        // ========================================
        // PERFORMANCE MONITORING - نظارت بر عملکرد
        // ========================================
        startPerformanceMonitoring: function(operation) {
            this._performanceMetrics = this._performanceMetrics || {};
            this._performanceMetrics[operation] = {
                startTime: performance.now(),
                memoryBefore: performance.memory ? performance.memory.usedJSHeapSize : 0
            };
            
            console.log('[RealTimeInsuranceBinding] 📊 Performance monitoring started for:', operation);
        },

        endPerformanceMonitoring: function(operation) {
            if (!this._performanceMetrics || !this._performanceMetrics[operation]) {
                return;
            }
            
            var metrics = this._performanceMetrics[operation];
            var duration = performance.now() - metrics.startTime;
            var memoryAfter = performance.memory ? performance.memory.usedJSHeapSize : 0;
            var memoryDelta = memoryAfter - metrics.memoryBefore;
            
            console.log('[RealTimeInsuranceBinding] 📊 Performance metrics for', operation, ':', {
                duration: duration.toFixed(2) + 'ms',
                memoryDelta: (memoryDelta / 1024 / 1024).toFixed(2) + 'MB',
                timestamp: new Date().toISOString()
            });
            
            // Clean up metrics
            delete this._performanceMetrics[operation];
        },

        // ========================================
        // SET SAVE IN PROGRESS STATE - تنظیم وضعیت در حال ذخیره
        // ========================================
        setSaveInProgressState: function() {
            console.log('[RealTimeInsuranceBinding] 🔄 Setting save in progress state...');
            
            this._isSaving = true;
            this._saveStartTime = Date.now();
            
            // UI State Management
            $(CONFIG.selectors.saveInsuranceBtn)
                .prop('disabled', true)
                .addClass('disabled')
                .html('<i class="fas fa-spinner fa-spin"></i> در حال ذخیره...');
            
            // Progress Steps
            this.updateProgressSteps('saving');
            
            // Loading Indicator
            this.showLoadingIndicator();
            
            // Disable form inputs to prevent changes during save
            this.disableFormInputs();
            
            console.log('[RealTimeInsuranceBinding] ✅ Save in progress state set');
        },

        // ========================================
        // DISABLE FORM INPUTS - غیرفعال کردن ورودی‌های فرم
        // ========================================
        disableFormInputs: function() {
            $(CONFIG.selectors.insuranceSection + ' input, ' + CONFIG.selectors.insuranceSection + ' select')
                .prop('disabled', true)
                .addClass('form-disabled');
        },

        // ========================================
        // ENABLE FORM INPUTS - فعال کردن ورودی‌های فرم
        // ========================================
        enableFormInputs: function() {
            $(CONFIG.selectors.insuranceSection + ' input, ' + CONFIG.selectors.insuranceSection + ' select')
                .prop('disabled', false)
                .removeClass('form-disabled');
        },

        // ========================================
        // COLLECT AND SANITIZE INSURANCE DATA - جمع‌آوری و پاکسازی داده‌های بیمه
        // ========================================
        collectAndSanitizeInsuranceData: function() {
            console.log('[RealTimeInsuranceBinding] 🧹 Collecting and sanitizing insurance data...');
            
            try {
                var data = {
                    // Primary Insurance
                    PrimaryProviderId: this.sanitizeValue($(CONFIG.selectors.primaryProvider).val()),
                    PrimaryPlanId: this.sanitizeValue($(CONFIG.selectors.primaryPlan).val()),
                    PrimaryPolicyNumber: this.sanitizeValue($(CONFIG.selectors.primaryPolicyNumber).val()),
                    PrimaryCardNumber: this.sanitizeValue($(CONFIG.selectors.primaryCardNumber).val()),
                    
                    // Supplementary Insurance
                    SupplementaryProviderId: this.sanitizeValue($(CONFIG.selectors.supplementaryProvider).val()),
                    SupplementaryPlanId: this.sanitizeValue($(CONFIG.selectors.supplementaryPlan).val()),
                    SupplementaryPolicyNumber: this.sanitizeValue($(CONFIG.selectors.supplementaryPolicyNumber).val()),
                    SupplementaryExpiryDate: this.sanitizeValue($(CONFIG.selectors.supplementaryExpiry).val())
                };
                
                console.log('[RealTimeInsuranceBinding] 📊 Sanitized data:', data);
                return data;
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error collecting insurance data:', error);
                throw error;
            }
        },

        // ========================================
        // SANITIZE VALUE - پاکسازی مقدار
        // ========================================
        sanitizeValue: function(value) {
            if (!value || value === '') {
                return null;
            }
            
            // Trim whitespace
            value = value.toString().trim();
            
            // Remove potentially dangerous characters
            value = value.replace(/[<>\"'&]/g, '');
            
            return value;
        },

        // ========================================
        // PREPARE SAVE REQUEST - آماده‌سازی درخواست ذخیره
        // ========================================
        prepareSaveRequest: function(patientId, insuranceData) {
            console.log('[RealTimeInsuranceBinding] 📦 Preparing save request...');
            
            try {
                var requestData = $.extend(insuranceData, {
                    patientId: patientId,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                    timestamp: Date.now(),
                    userAgent: navigator.userAgent,
                    sessionId: this.getSessionId()
                });
                
                console.log('[RealTimeInsuranceBinding] 📦 Request prepared:', {
                    patientId: requestData.patientId,
                    hasToken: !!requestData.__RequestVerificationToken,
                    timestamp: requestData.timestamp
                });
                
                return requestData;
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error preparing save request:', error);
                throw error;
            }
        },

        // ========================================
        // GET SESSION ID - دریافت شناسه جلسه
        // ========================================
        getSessionId: function() {
            if (!this._sessionId) {
                this._sessionId = 'session_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
            }
            return this._sessionId;
        },

        // ========================================
        // EXECUTE SAVE REQUEST - اجرای درخواست ذخیره
        // ========================================
        executeSaveRequest: function(requestData, startTime) {
            console.log('[RealTimeInsuranceBinding] 🚀 Executing save request...');
            
            var self = this;
            var requestId = 'req_' + Date.now() + '_' + Math.random().toString(36).substr(2, 5);
            
            $.ajax({
                url: CONFIG.endpoints.saveInsurance,
                type: 'POST',
                data: requestData,
                timeout: CONFIG.performance.ajaxTimeout,
                cache: false,
                beforeSend: function(xhr) {
                    xhr.setRequestHeader('X-Request-ID', requestId);
                    xhr.setRequestHeader('X-Request-Time', startTime.toString());
                },
                success: function(response, textStatus, xhr) {
                    self.endPerformanceMonitoring('save');
                    self.handleInsuranceSaveSuccess(response, requestId);
                },
                error: function(xhr, status, error) {
                    self.endPerformanceMonitoring('save');
                    self.handleInsuranceSaveError(xhr, status, error, requestId);
                },
                complete: function(xhr, status) {
                    self.resetSaveState();
                }
            });
        },

        // ========================================
        // RESET SAVE STATE - بازنشانی وضعیت ذخیره
        // ========================================
        resetSaveState: function() {
            console.log('[RealTimeInsuranceBinding] 🔄 Resetting save state...');
            
            this._isSaving = false;
            this._saveStartTime = null;
            
            // Re-enable save button
            $(CONFIG.selectors.saveInsuranceBtn)
                .prop('disabled', false)
                .removeClass('disabled')
                .html('<i class="fas fa-save"></i> ذخیره اطلاعات بیمه');
            
            // Re-enable form inputs
            this.enableFormInputs();
            
            // Hide loading indicator
            this.hideLoadingIndicator();
            
            // Reset progress steps
            this.updateProgressSteps('default');
            
            console.log('[RealTimeInsuranceBinding] ✅ Save state reset');
        },

        // ========================================
        // SHOW VALIDATION ERRORS - نمایش خطاهای اعتبارسنجی
        // ========================================
        showValidationErrors: function(errors) {
            console.log('[RealTimeInsuranceBinding] ❌ Validation errors:', errors);
            
            var errorMessages = errors.map(function(error) {
                return error.message;
            });
            
            this.showError('خطاهای اعتبارسنجی: ' + errorMessages.join(', '));
        },

        // ========================================
        // HANDLE CRITICAL ERROR - مدیریت خطای بحرانی
        // ========================================
        handleCriticalError: function(error, context) {
            console.error('[RealTimeInsuranceBinding] 🚨 Critical error in', context, ':', error);
            
            // Reset all states
            this.resetSaveState();
            
            // Show user-friendly error
            this.showError('خطای بحرانی در سیستم. لطفاً صفحه را بازخوانی کنید.');
            
            // Log for debugging
            if (window.ReceptionEventBus) {
                window.ReceptionEventBus.emit('system:criticalError', {
                    context: context,
                    error: error.message || error,
                    timestamp: new Date().toISOString()
                });
            }
        },

        // ========================================
        // COLLECT INSURANCE DATA - جمع‌آوری اطلاعات بیمه (Legacy Support)
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
        // INSURANCE SAVE SUCCESS HANDLER - مدیریت موفقیت ذخیره بیمه (Enhanced)
        // ========================================
        handleInsuranceSaveSuccess: function(response, requestId) {
            console.log('[RealTimeInsuranceBinding] ✅ Insurance save success:', response);
            
            try {
                // 1. RESPONSE PARSING & VALIDATION - تجزیه و اعتبارسنجی پاسخ
                var parsed = this.parseSaveResponse(response);
                if (!parsed) {
                    this.showError('خطا در پردازش پاسخ سرور');
                    return;
                }

                // 2. SUCCESS VALIDATION - اعتبارسنجی موفقیت
                if (parsed.success === true) {
                    this.handleSuccessfulSave(parsed, requestId);
                } else {
                    this.handleFailedSave(parsed);
                }
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error handling insurance save success:', error);
                this.handleCriticalError(error, 'handleInsuranceSaveSuccess');
            }
        },

        // ========================================
        // PARSE SAVE RESPONSE - تجزیه پاسخ ذخیره
        // ========================================
        parseSaveResponse: function(response) {
            console.log('[RealTimeInsuranceBinding] 🔍 Parsing save response...');
            
            try {
                var parsed = response;
                
                // Handle string responses
                if (typeof response === 'string') {
                    try {
                        parsed = JSON.parse(response);
                        console.log('[RealTimeInsuranceBinding] ✅ Response parsed successfully');
                    } catch (e) {
                        console.error('[RealTimeInsuranceBinding] ❌ Failed to parse response:', e);
                        return null;
                    }
                }
                
                // Validate response structure
                if (!parsed || typeof parsed !== 'object') {
                    console.error('[RealTimeInsuranceBinding] ❌ Invalid response structure');
                    return null;
                }
                
                return parsed;
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error parsing response:', error);
                return null;
            }
        },

        // ========================================
        // HANDLE SUCCESSFUL SAVE - مدیریت ذخیره موفق
        // ========================================
        handleSuccessfulSave: function(parsed, requestId) {
            console.log('[RealTimeInsuranceBinding] 🎉 Handling successful save...');
            
            try {
                // 1. Update UI Status
                this.updateInsuranceStatus('saved');
                
                // 2. Show Success Message
                this.showSuccess(parsed.message || 'اطلاعات بیمه با موفقیت ذخیره شد');
                
                // 3. Update Form State
                this.updateFormAfterSuccessfulSave(parsed);
                
                // 4. Publish Success Event
                this.publishSaveSuccessEvent(parsed, requestId);
                
                // 5. Performance Logging
                this.logSavePerformance(requestId, true);
                
                console.log('[RealTimeInsuranceBinding] ✅ Successful save handled');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error handling successful save:', error);
                this.handleCriticalError(error, 'handleSuccessfulSave');
            }
        },

        // ========================================
        // UPDATE FORM AFTER SUCCESSFUL SAVE - به‌روزرسانی فرم پس از ذخیره موفق
        // ========================================
        updateFormAfterSuccessfulSave: function(parsed) {
            console.log('[RealTimeInsuranceBinding] 🔄 Updating form after successful save...');
            
            try {
                // 1. Update original form values
                this.originalFormValues = this.captureFormValues();
                
                // 2. Disable edit mode
                this.disableEditMode();
                
                // 3. Sync with server state
                this.syncWithServerState(parsed);
                
                // 4. Update save button state
                this.updateSaveButtonState();
                
                console.log('[RealTimeInsuranceBinding] ✅ Form updated after successful save');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error updating form after save:', error);
                // Fallback: reload insurance data
                this.loadPatientInsurance($(CONFIG.selectors.patientId).val());
            }
        },

        // ========================================
        // SYNC WITH SERVER STATE - همگام‌سازی با وضعیت سرور
        // ========================================
        syncWithServerState: function(parsed) {
            console.log('[RealTimeInsuranceBinding] 🔄 Syncing with server state...');
            
            try {
                if (parsed.data && parsed.data.Status) {
                    console.log('[RealTimeInsuranceBinding] 📊 Rebinding with authoritative server state');
                    this.bindInsuranceDataToForm(parsed.data.Status);
                } else {
                    console.log('[RealTimeInsuranceBinding] 📊 Reloading insurance data');
                    this.loadPatientInsurance($(CONFIG.selectors.patientId).val());
                }
            } catch (error) {
                console.warn('[RealTimeInsuranceBinding] ⚠️ Server sync failed, fallback to reload:', error);
                this.loadPatientInsurance($(CONFIG.selectors.patientId).val());
            }
        },

        // ========================================
        // PUBLISH SAVE SUCCESS EVENT - انتشار رویداد ذخیره موفق
        // ========================================
        publishSaveSuccessEvent: function(parsed, requestId) {
            console.log('[RealTimeInsuranceBinding] 📡 Publishing save success event...');
            
            try {
                var eventData = {
                    requestId: requestId,
                    patientId: $(CONFIG.selectors.patientId).val(),
                    timestamp: new Date().toISOString(),
                    data: parsed.data
                };
                
                // Publish via EventBus
                if (window.ReceptionEventBus) {
                    window.ReceptionEventBus.emit('insurance:saved', eventData);
                    console.log('[RealTimeInsuranceBinding] ✅ Event published via EventBus');
                }
                
                // Publish via jQuery (fallback)
                $(document).trigger('insuranceSaved', [eventData]);
                console.log('[RealTimeInsuranceBinding] ✅ Event published via jQuery');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error publishing save success event:', error);
            }
        },

        // ========================================
        // HANDLE FAILED SAVE - مدیریت ذخیره ناموفق
        // ========================================
        handleFailedSave: function(parsed) {
            console.log('[RealTimeInsuranceBinding] ❌ Handling failed save...');
            
            var errorMessage = (parsed && parsed.message) || 'خطا در ذخیره اطلاعات بیمه';
            this.showError(errorMessage);
        },

        // ========================================
        // LOG SAVE PERFORMANCE - ثبت عملکرد ذخیره
        // ========================================
        logSavePerformance: function(requestId, success) {
            console.log('[RealTimeInsuranceBinding] 📊 Logging save performance...');
            
            try {
                var metrics = {
                    requestId: requestId,
                    success: success,
                    timestamp: new Date().toISOString(),
                    duration: this._saveStartTime ? Date.now() - this._saveStartTime : 0,
                    memoryUsage: performance.memory ? performance.memory.usedJSHeapSize : 0
                };
                
                console.log('[RealTimeInsuranceBinding] 📊 Save performance metrics:', metrics);
                
                // Send to analytics if available
                if (window.ReceptionEventBus) {
                    window.ReceptionEventBus.emit('analytics:savePerformance', metrics);
                }
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error logging save performance:', error);
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
            console.log('[RealTimeInsuranceBinding] 🔍 Coordinating form validation...');
            
            try {
                if (window.ValidationEngine) {
                    return window.ValidationEngine.validateForm();
                }
                
                console.warn('[RealTimeInsuranceBinding] ⚠️ ValidationEngine not available');
                return {
                    isValid: false,
                    errors: ['ValidationEngine not available'],
                    warnings: [],
                    hasChanges: false,
                    canSave: false
                };
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error coordinating form validation:', error);
                return {
                    isValid: false,
                    errors: ['خطا در اعتبارسنجی فرم'],
                    warnings: [],
                    hasChanges: false,
                    canSave: false
                };
            }
        },

        // ========================================
        // UPDATE SAVE BUTTON STATE - به‌روزرسانی وضعیت دکمه ذخیره
        // ========================================
        updateSaveButtonState: function() {
            console.log('[RealTimeInsuranceBinding] 🔄 Coordinating save button state update...');
            
            try {
                if (window.EditModeManager) {
                    return window.EditModeManager.updateSaveButtonState();
                }
                
                if (window.SaveProcessor) {
                    return window.SaveProcessor.updateSaveButtonState();
                }
                
                console.warn('[RealTimeInsuranceBinding] ⚠️ No save button manager available');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error coordinating save button state update:', error);
                this.handleError(error, 'updateSaveButtonState');
            }
        },

        // ========================================
        // SHOW VALIDATION ERRORS - نمایش خطاهای اعتبارسنجی
        // ========================================
        showValidationErrors: function(errors) {
            console.log('[RealTimeInsuranceBinding] ❌ Validation errors:', errors);
            
            var errorMessage = errors.join(', ');
            
            if (window.ReceptionToastr && window.ReceptionToastr.helpers && window.ReceptionToastr.helpers.showError) {
                window.ReceptionToastr.helpers.showError('خطاهای اعتبارسنجی: ' + errorMessage);
            } else {
                console.error('[RealTimeInsuranceBinding] Validation errors: ' + errorMessage);
            }
        },

        // ========================================
        // SHOW VALIDATION WARNINGS - نمایش هشدارهای اعتبارسنجی
        // ========================================
        showValidationWarnings: function(warnings) {
            console.log('[RealTimeInsuranceBinding] ⚠️ Validation warnings:', warnings);
            
            var warningMessage = warnings.join(', ');
            
            if (window.ReceptionToastr && window.ReceptionToastr.helpers && window.ReceptionToastr.helpers.showWarning) {
                window.ReceptionToastr.helpers.showWarning('هشدارهای اعتبارسنجی: ' + warningMessage);
            } else {
                console.warn('[RealTimeInsuranceBinding] Validation warnings: ' + warningMessage);
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

        handleInsuranceSaveError: function(xhr, status, error, requestId) {
            console.error('[RealTimeInsuranceBinding] ❌ Insurance save error:', xhr, status, error);
            
            try {
                // 1. ERROR CLASSIFICATION - طبقه‌بندی خطا
                var errorInfo = this.classifySaveError(xhr, status, error);
                
                // 2. ERROR LOGGING - ثبت خطا
                this.logSaveError(errorInfo, requestId);
                
                // 3. USER NOTIFICATION - اطلاع‌رسانی کاربر
                this.notifyUserOfError(errorInfo);
                
                // 4. RETRY LOGIC - منطق تلاش مجدد
                this.handleSaveErrorRetry(errorInfo);
                
                // 5. PERFORMANCE LOGGING - ثبت عملکرد
                this.logSavePerformance(requestId, false);
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error handling save error:', error);
                this.showError('خطای غیرمنتظره در ذخیره اطلاعات');
            }
        },

        // ========================================
        // CLASSIFY SAVE ERROR - طبقه‌بندی خطای ذخیره
        // ========================================
        classifySaveError: function(xhr, status, error) {
            console.log('[RealTimeInsuranceBinding] 🔍 Classifying save error...');
            
            var errorInfo = {
                type: 'unknown',
                severity: 'error',
                message: 'خطا در ذخیره اطلاعات',
                retryable: false,
                timestamp: new Date().toISOString()
            };
            
            try {
                // Network errors
                if (status === 'timeout') {
                    errorInfo.type = 'timeout';
                    errorInfo.message = 'زمان انتظار به پایان رسید. لطفاً دوباره تلاش کنید.';
                    errorInfo.retryable = true;
                } else if (status === 'abort') {
                    errorInfo.type = 'abort';
                    errorInfo.message = 'درخواست لغو شد';
                    errorInfo.retryable = true;
                } else if (!navigator.onLine) {
                    errorInfo.type = 'offline';
                    errorInfo.message = 'اتصال اینترنت قطع است';
                    errorInfo.retryable = true;
                }
                // HTTP errors
                else if (xhr && xhr.status) {
                    if (xhr.status >= 500) {
                        errorInfo.type = 'server';
                        errorInfo.message = 'خطا در سرور. لطفاً بعداً تلاش کنید.';
                        errorInfo.retryable = true;
                    } else if (xhr.status === 401) {
                        errorInfo.type = 'unauthorized';
                        errorInfo.message = 'دسترسی غیرمجاز. لطفاً دوباره وارد شوید.';
                        errorInfo.retryable = false;
                    } else if (xhr.status === 403) {
                        errorInfo.type = 'forbidden';
                        errorInfo.message = 'دسترسی غیرمجاز';
                        errorInfo.retryable = false;
                    } else if (xhr.status === 404) {
                        errorInfo.type = 'notfound';
                        errorInfo.message = 'سرویس یافت نشد';
                        errorInfo.retryable = false;
                    } else if (xhr.status === 422) {
                        errorInfo.type = 'validation';
                        errorInfo.message = 'اطلاعات وارد شده نامعتبر است';
                        errorInfo.retryable = false;
                    }
                }
                
                // Parse server response for detailed error
                if (xhr && xhr.responseJSON && xhr.responseJSON.message) {
                    errorInfo.message = xhr.responseJSON.message;
                } else if (xhr && xhr.responseText) {
                    try {
                        var response = JSON.parse(xhr.responseText);
                        if (response.message) {
                            errorInfo.message = response.message;
                        }
                    } catch (e) {
                        // Use raw response text
                        errorInfo.message = 'خطا در ذخیره: ' + xhr.responseText;
                    }
                }
                
                console.log('[RealTimeInsuranceBinding] 📊 Error classified:', errorInfo);
                return errorInfo;
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error classifying save error:', error);
                return errorInfo;
            }
        },

        // ========================================
        // LOG SAVE ERROR - ثبت خطای ذخیره
        // ========================================
        logSaveError: function(errorInfo, requestId) {
            console.log('[RealTimeInsuranceBinding] 📝 Logging save error...');
            
            try {
                var errorLog = {
                    requestId: requestId,
                    errorType: errorInfo.type,
                    severity: errorInfo.severity,
                    message: errorInfo.message,
                    retryable: errorInfo.retryable,
                    timestamp: errorInfo.timestamp,
                    userAgent: navigator.userAgent,
                    url: window.location.href
                };
                
                console.error('[RealTimeInsuranceBinding] 📝 Save error logged:', errorLog);
                
                // Send to analytics if available
                if (window.ReceptionEventBus) {
                    window.ReceptionEventBus.emit('analytics:saveError', errorLog);
                }
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error logging save error:', error);
            }
        },

        // ========================================
        // NOTIFY USER OF ERROR - اطلاع‌رسانی کاربر از خطا
        // ========================================
        notifyUserOfError: function(errorInfo) {
            console.log('[RealTimeInsuranceBinding] 📢 Notifying user of error...');
            
            try {
                // Show error message
                this.showError(errorInfo.message);
                
                // Show retry option if applicable
                if (errorInfo.retryable) {
                    this.showRetryOption(errorInfo);
                }
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error notifying user:', error);
                this.showError('خطا در ذخیره اطلاعات');
            }
        },

        // ========================================
        // SHOW RETRY OPTION - نمایش گزینه تلاش مجدد
        // ========================================
        showRetryOption: function(errorInfo) {
            console.log('[RealTimeInsuranceBinding] 🔄 Showing retry option...');
            
            try {
                // Add retry button to UI
                var retryBtn = $('<button class="btn btn-warning btn-sm ms-2" id="retrySaveBtn">تلاش مجدد</button>');
                $(CONFIG.selectors.saveInsuranceBtn).after(retryBtn);
                
                // Bind retry event
                retryBtn.on('click', function() {
                    retryBtn.remove();
                    self.savePatientInsurance();
                });
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error showing retry option:', error);
            }
        },

        // ========================================
        // HANDLE SAVE ERROR RETRY - مدیریت تلاش مجدد خطای ذخیره
        // ========================================
        handleSaveErrorRetry: function(errorInfo) {
            console.log('[RealTimeInsuranceBinding] 🔄 Handling save error retry...');
            
            try {
                if (errorInfo.retryable && errorInfo.type === 'timeout') {
                    // Auto-retry for timeout errors
                    var autoRetryTimeout = setTimeout(function() {
                        console.log('[RealTimeInsuranceBinding] 🔄 Auto-retrying save after timeout');
                        self.savePatientInsurance();
                    }, 2000);
                    this._timeouts.push(autoRetryTimeout);
                }
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error handling save error retry:', error);
            }
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
        // INITIALIZATION - راه‌اندازی اولیه (Production-Optimized)
        // ========================================
        initializeState: function() {
            console.log('[RealTimeInsuranceBinding] 🚀 Initializing state...');
            
            try {
                // 1. CORE STATE FLAGS - پرچم‌های وضعیت اصلی
                this.suppressFormChange = false;
                this._isSaving = false;
                this.pendingPrimaryPlanId = null;
                this.pendingSupplementaryPlanId = null;
                
                // 2. PERFORMANCE METRICS - متریک‌های عملکرد
                this._performanceMetrics = {};
                this._saveQueue = [];
                this._sessionId = null;
                this._saveStartTime = null;
                
                // 3. MEMORY MANAGEMENT - مدیریت حافظه
                this._eventListeners = [];
                this._timeouts = [];
                this._intervals = [];
                
                // 4. UI STATE INITIALIZATION - راه‌اندازی وضعیت UI
                this.updateInsuranceStatus('unknown');
                this.updateSaveButtonState();
                this.initializeFormEditMode();
                
                // 5. MEMORY LEAK PREVENTION - جلوگیری از نشت حافظه
                this.setupMemoryManagement();
                
                console.log('[RealTimeInsuranceBinding] ✅ State initialized successfully');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error initializing state:', error);
                this.handleCriticalError(error, 'initializeState');
            }
        },

        // ========================================
        // SETUP MEMORY MANAGEMENT - راه‌اندازی مدیریت حافظه
        // ========================================
        setupMemoryManagement: function() {
            console.log('[RealTimeInsuranceBinding] 🧠 Setting up memory management...');
            
            try {
                // 1. CLEANUP ON PAGE UNLOAD - پاکسازی هنگام خروج از صفحه
                $(window).on('beforeunload.realtimeInsurance', function() {
                    self.cleanup();
                });
                
                // 2. PERIODIC MEMORY CLEANUP - پاکسازی دوره‌ای حافظه
                this._memoryCleanupInterval = setInterval(function() {
                    self.performMemoryCleanup();
                }, 30000); // Every 30 seconds
                this._intervals.push(this._memoryCleanupInterval);
                
                // 3. PERFORMANCE MONITORING - نظارت بر عملکرد
                if (performance.memory) {
                    this._memoryThreshold = 50 * 1024 * 1024; // 50MB threshold
                    this._memoryCheckInterval = setInterval(function() {
                        self.checkMemoryUsage();
                    }, 10000); // Every 10 seconds
                    this._intervals.push(this._memoryCheckInterval);
                }
                
                console.log('[RealTimeInsuranceBinding] ✅ Memory management setup complete');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error setting up memory management:', error);
            }
        },

        // ========================================
        // PERFORM MEMORY CLEANUP - انجام پاکسازی حافظه
        // ========================================
        performMemoryCleanup: function() {
            console.log('[RealTimeInsuranceBinding] 🧹 Performing memory cleanup...');
            
            try {
                // 1. Clear completed timeouts
                this._timeouts = this._timeouts.filter(function(timeoutId) {
                    return timeoutId !== null;
                });
                
                // 2. Clear old performance metrics
                var now = Date.now();
                for (var operation in this._performanceMetrics) {
                    if (this._performanceMetrics[operation].startTime < now - 300000) { // 5 minutes
                        delete this._performanceMetrics[operation];
                    }
                }
                
                // 3. Clear old queue items
                if (this._saveQueue) {
                    this._saveQueue = this._saveQueue.filter(function(item) {
                        return item.timestamp > now - 60000; // 1 minute
                    });
                }
                
                // 4. Force garbage collection if available
                if (window.gc) {
                    window.gc();
                }
                
                console.log('[RealTimeInsuranceBinding] ✅ Memory cleanup completed');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error performing memory cleanup:', error);
            }
        },

        // ========================================
        // CHECK MEMORY USAGE - بررسی استفاده از حافظه
        // ========================================
        checkMemoryUsage: function() {
            if (!performance.memory) return;
            
            var memoryUsage = performance.memory.usedJSHeapSize;
            var memoryLimit = performance.memory.jsHeapSizeLimit;
            var usagePercent = (memoryUsage / memoryLimit) * 100;
            
            if (usagePercent > 80) {
                console.warn('[RealTimeInsuranceBinding] ⚠️ High memory usage:', {
                    used: (memoryUsage / 1024 / 1024).toFixed(2) + 'MB',
                    limit: (memoryLimit / 1024 / 1024).toFixed(2) + 'MB',
                    percent: usagePercent.toFixed(2) + '%'
                });
                
                // Force cleanup
                this.performMemoryCleanup();
            }
        },

        // ========================================
        // CLEANUP - پاکسازی کامل
        // ========================================
        cleanup: function() {
            console.log('[RealTimeInsuranceBinding] 🧹 Performing complete cleanup...');
            
            try {
                // 1. Clear all timeouts
                this._timeouts.forEach(function(timeoutId) {
                    clearTimeout(timeoutId);
                });
                this._timeouts = [];
                
                // 2. Clear all intervals
                this._intervals.forEach(function(intervalId) {
                    clearInterval(intervalId);
                });
                this._intervals = [];
                
                // 3. Remove all event listeners
                this._eventListeners.forEach(function(listener) {
                    listener.element.off(listener.event, listener.handler);
                });
                this._eventListeners = [];
                
                // 4. Clear performance metrics
                this._performanceMetrics = {};
                
                // 5. Clear queues
                this._saveQueue = [];
                
                // 6. Reset state
                this._isSaving = false;
                this.suppressFormChange = false;
                
                console.log('[RealTimeInsuranceBinding] ✅ Cleanup completed');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error during cleanup:', error);
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
                console.log('[RealTimeInsuranceBinding] Form values captured:', this.originalFormValues);
                
                // اضافه کردن event listeners برای تشخیص تغییرات
                this.setupFormChangeDetection();
                
                // تاخیر کوتاه برای اطمینان از بارگذاری کامل فرم
                setTimeout(function() {
                    console.log('[RealTimeInsuranceBinding] Re-capturing form values after delay...');
                    this.originalFormValues = this.captureFormValues();
                    console.log('[RealTimeInsuranceBinding] Updated form values:', this.originalFormValues);
                }.bind(this), 500);
                
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
                // تابع امن برای دریافت مقدار
                function safeGetValue(selector) {
                    try {
                        var element = $(selector);
                        if (element.length === 0) {
                            console.warn('[RealTimeInsuranceBinding] Element not found:', selector);
                            return '';
                        }
                        var value = element.val();
                        return value ? String(value).trim() : '';
                    } catch (error) {
                        console.warn('[RealTimeInsuranceBinding] Error getting value for selector:', selector, error);
                        return '';
                    }
                }
                
                var formValues = {
                    primaryProvider: safeGetValue(CONFIG.selectors.primaryProvider),
                    primaryPlan: safeGetValue(CONFIG.selectors.primaryPlan),
                    primaryPolicyNumber: safeGetValue(CONFIG.selectors.primaryPolicyNumber),
                    primaryCardNumber: safeGetValue(CONFIG.selectors.primaryCardNumber),
                    supplementaryProvider: safeGetValue(CONFIG.selectors.supplementaryProvider),
                    supplementaryPlan: safeGetValue(CONFIG.selectors.supplementaryPlan),
                    supplementaryPolicyNumber: safeGetValue(CONFIG.selectors.supplementaryPolicyNumber),
                    supplementaryExpiry: safeGetValue(CONFIG.selectors.supplementaryExpiry)
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
                
                // Event listeners تفصیلی برای هر فیلد
                $(CONFIG.selectors.primaryProvider).on('change.formEditMode', function() {
                    console.log('[RealTimeInsuranceBinding] 🔄 Primary Provider changed to:', $(this).val());
                    self.handleFormChange();
                });
                
                $(CONFIG.selectors.primaryPlan).on('change.formEditMode', function() {
                    console.log('[RealTimeInsuranceBinding] 🔄 Primary Plan changed to:', $(this).val());
                    self.handleFormChange();
                });
                
                $(CONFIG.selectors.primaryPolicyNumber).on('input.formEditMode change.formEditMode', function() {
                    console.log('[RealTimeInsuranceBinding] 🔄 Primary Policy Number changed to:', $(this).val());
                    self.handleFormChange();
                });
                
                $(CONFIG.selectors.primaryCardNumber).on('input.formEditMode change.formEditMode', function() {
                    console.log('[RealTimeInsuranceBinding] 🔄 Primary Card Number changed to:', $(this).val());
                    self.handleFormChange();
                });
                
                $(CONFIG.selectors.supplementaryProvider).on('change.formEditMode', function() {
                    console.log('[RealTimeInsuranceBinding] 🔄 Supplementary Provider changed to:', $(this).val());
                    self.handleFormChange();
                });
                
                $(CONFIG.selectors.supplementaryPlan).on('change.formEditMode', function() {
                    console.log('[RealTimeInsuranceBinding] 🔄 Supplementary Plan changed to:', $(this).val());
                    self.handleFormChange();
                });
                
                $(CONFIG.selectors.supplementaryPolicyNumber).on('input.formEditMode change.formEditMode', function() {
                    console.log('[RealTimeInsuranceBinding] 🔄 Supplementary Policy Number changed to:', $(this).val());
                    self.handleFormChange();
                });
                
                $(CONFIG.selectors.supplementaryExpiry).on('change.formEditMode', function() {
                    console.log('[RealTimeInsuranceBinding] 🔄 Supplementary Expiry changed to:', $(this).val());
                    self.handleFormChange();
                });
                
                // Event listener عمومی برای همه فیلدهای بیمه
                $(CONFIG.selectors.insuranceSection + ' input, ' + CONFIG.selectors.insuranceSection + ' select').on('change.formEditMode input.formEditMode', function() {
                    console.log('[RealTimeInsuranceBinding] 🔄 General form change detected on:', $(this).attr('id') || $(this).attr('name'));
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
            console.log('[RealTimeInsuranceBinding] 🔄 Form change detected');
            
            try {
                // بررسی وجود originalFormValues
                if (!this.originalFormValues) {
                    console.warn('[RealTimeInsuranceBinding] ⚠️ Original form values not set, capturing current values');
                    this.originalFormValues = this.captureFormValues();
                    console.log('[RealTimeInsuranceBinding] 📝 Original values set:', this.originalFormValues);
                    return;
                }
                
                console.log('[RealTimeInsuranceBinding] 📊 Capturing current form values...');
                var currentValues = this.captureFormValues();
                console.log('[RealTimeInsuranceBinding] 📊 Current values:', currentValues);
                
                console.log('[RealTimeInsuranceBinding] 🔍 Comparing with original values...');
                console.log('[RealTimeInsuranceBinding] 📝 Original values:', this.originalFormValues);
                
                var hasChanges = this.detectFormChanges(this.originalFormValues, currentValues);
                console.log('[RealTimeInsuranceBinding] 🔍 Has changes result:', hasChanges);
                
                if (hasChanges) {
                    console.log('[RealTimeInsuranceBinding] ✅ Form has changes, enabling edit mode');
                    this.enableEditMode();
                } else {
                    console.log('[RealTimeInsuranceBinding] ❌ No form changes detected, disabling edit mode');
                    this.disableEditMode();
                }
                
                // اجرای دیباگ برای بررسی وضعیت
                console.log('[RealTimeInsuranceBinding] 🔍 Running debug analysis...');
                this.debugFormState();
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error handling form change:', error);
                this.handleError(error, 'handleFormChange');
            }
        },

        // ========================================
        // DETECT FORM CHANGES - تشخیص تغییرات فرم
        // ========================================
        detectFormChanges: function(originalValues, currentValues) {
            console.log('[RealTimeInsuranceBinding] 🔍 Detecting form changes...');
            console.log('[RealTimeInsuranceBinding] 📝 Original values:', originalValues);
            console.log('[RealTimeInsuranceBinding] 📊 Current values:', currentValues);
            
            try {
                var changes = [];
                
                // تابع مقایسه امن با لاگ‌های تفصیلی
                function safeCompare(original, current, fieldName) {
                    var orig = original || '';
                    var curr = current || '';
                    
                    // تبدیل به رشته برای مقایسه
                    orig = String(orig).trim();
                    curr = String(curr).trim();
                    
                    console.log('[RealTimeInsuranceBinding] 🔍 Comparing ' + fieldName + ':');
                    console.log('[RealTimeInsuranceBinding] 📝 Original: "' + orig + '" (type: ' + typeof orig + ')');
                    console.log('[RealTimeInsuranceBinding] 📊 Current: "' + curr + '" (type: ' + typeof curr + ')');
                    console.log('[RealTimeInsuranceBinding] 🔍 Are equal: ' + (orig === curr));
                    
                    if (orig !== curr) {
                        changes.push(fieldName);
                        console.log('[RealTimeInsuranceBinding] ✅ Change detected in ' + fieldName + ': "' + orig + '" -> "' + curr + '"');
                        return true;
                    } else {
                        console.log('[RealTimeInsuranceBinding] ❌ No change in ' + fieldName);
                        return false;
                    }
                }
                
                console.log('[RealTimeInsuranceBinding] 🔍 Checking Primary Insurance changes...');
                // بررسی تغییرات بیمه پایه
                safeCompare(originalValues.primaryProvider, currentValues.primaryProvider, 'Primary Provider');
                safeCompare(originalValues.primaryPlan, currentValues.primaryPlan, 'Primary Plan');
                safeCompare(originalValues.primaryPolicyNumber, currentValues.primaryPolicyNumber, 'Primary Policy Number');
                safeCompare(originalValues.primaryCardNumber, currentValues.primaryCardNumber, 'Primary Card Number');
                
                console.log('[RealTimeInsuranceBinding] 🔍 Checking Supplementary Insurance changes...');
                // بررسی تغییرات بیمه تکمیلی
                safeCompare(originalValues.supplementaryProvider, currentValues.supplementaryProvider, 'Supplementary Provider');
                safeCompare(originalValues.supplementaryPlan, currentValues.supplementaryPlan, 'Supplementary Plan');
                safeCompare(originalValues.supplementaryPolicyNumber, currentValues.supplementaryPolicyNumber, 'Supplementary Policy Number');
                safeCompare(originalValues.supplementaryExpiry, currentValues.supplementaryExpiry, 'Supplementary Expiry');
                
                console.log('[RealTimeInsuranceBinding] 📊 Changes detected:', changes);
                console.log('[RealTimeInsuranceBinding] 📊 Total changes:', changes.length);
                console.log('[RealTimeInsuranceBinding] 🔍 Has changes:', changes.length > 0);
                
                return changes.length > 0;
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error detecting form changes:', error);
                return false;
            }
        },

        // ========================================
        // ENABLE EDIT MODE - فعال کردن حالت ویرایش
        // ========================================
        enableEditMode: function() {
            console.log('[RealTimeInsuranceBinding] ✅ Enabling edit mode...');
            
            try {
                // بررسی وجود دکمه ذخیره
                var saveBtn = $(CONFIG.selectors.saveInsuranceBtn);
                console.log('[RealTimeInsuranceBinding] 🔍 Save button found:', saveBtn.length > 0);
                console.log('[RealTimeInsuranceBinding] 🔍 Save button current state:', saveBtn.prop('disabled'));
                
                // فعال کردن دکمه ذخیره و نمایش آن
                saveBtn.prop('disabled', false)
                       .removeClass('btn-secondary d-none')
                       .addClass('btn-success');
                console.log('[RealTimeInsuranceBinding] ✅ Save button enabled and shown');
                
                // نمایش پیام ویرایش
                this.showInfo('تغییرات در فرم تشخیص داده شد. برای ذخیره تغییرات روی دکمه ذخیره کلیک کنید.');
                console.log('[RealTimeInsuranceBinding] ✅ Info message shown');
                
                // اضافه کردن کلاس ویرایش به فرم
                $(CONFIG.selectors.insuranceSection).addClass('form-editing');
                console.log('[RealTimeInsuranceBinding] ✅ Form editing class added');
                
                // فعال کردن Progress Steps
                this.updateProgressSteps('editing');
                console.log('[RealTimeInsuranceBinding] ✅ Progress steps updated');
                
                // بررسی وضعیت نهایی
                console.log('[RealTimeInsuranceBinding] 🔍 Final save button state:', saveBtn.prop('disabled'));
                console.log('[RealTimeInsuranceBinding] 🔍 Final save button classes:', saveBtn.attr('class'));
                
                console.log('[RealTimeInsuranceBinding] ✅ Edit mode enabled successfully');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error enabling edit mode:', error);
                this.handleError(error, 'enableEditMode');
            }
        },

        // ========================================
        // DISABLE EDIT MODE - غیرفعال کردن حالت ویرایش
        // ========================================
        disableEditMode: function() {
            console.log('[RealTimeInsuranceBinding] ❌ Disabling edit mode...');
            
            try {
                // غیرفعال کردن دکمه ذخیره و مخفی کردن آن
                $(CONFIG.selectors.saveInsuranceBtn).prop('disabled', true)
                                                   .removeClass('btn-success')
                                                   .addClass('btn-secondary d-none');
                
                // حذف کلاس ویرایش از فرم
                $(CONFIG.selectors.insuranceSection).removeClass('form-editing');
                
                // بازنشانی Progress Steps
                this.updateProgressSteps('default');
                console.log('[RealTimeInsuranceBinding] ✅ Progress steps reset');
                
                console.log('[RealTimeInsuranceBinding] ✅ Edit mode disabled');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error disabling edit mode:', error);
                this.handleError(error, 'disableEditMode');
            }
        },

        // ========================================
        // UPDATE PROGRESS STEPS - بهینه‌سازی Progress Steps
        // ========================================
        updateProgressSteps: function(state) {
            console.log('[RealTimeInsuranceBinding] 🔄 Updating progress steps to:', state);
            
            try {
                // حذف کلاس‌های قبلی
                $('.step').removeClass('active completed');
                
                switch (state) {
                    case 'editing':
                        // فعال کردن مرحله ویرایش
                        $('#step3').addClass('active');
                        console.log('[RealTimeInsuranceBinding] ✅ Step 3 (ویرایش اطلاعات) activated');
                        break;
                        
                    case 'saving':
                        // فعال کردن مرحله ذخیره
                        $('#step3').addClass('completed');
                        $('#step4').addClass('active');
                        console.log('[RealTimeInsuranceBinding] ✅ Step 4 (ذخیره نهایی) activated');
                        break;
                        
                    case 'completed':
                        // تکمیل همه مراحل
                        $('.step').addClass('completed');
                        console.log('[RealTimeInsuranceBinding] ✅ All steps completed');
                        break;
                        
                    default:
                        // حالت پیش‌فرض
                        $('#step1').addClass('completed');
                        $('#step2').addClass('completed');
                        console.log('[RealTimeInsuranceBinding] ✅ Steps 1-2 completed');
                        break;
                }
                
                console.log('[RealTimeInsuranceBinding] ✅ Progress steps updated successfully');
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error updating progress steps:', error);
            }
        },

        // ========================================
        // DEBUG FORM STATE - دیباگ وضعیت فرم
        // ========================================
        debugFormState: function() {
            console.log('[RealTimeInsuranceBinding] 🔍 DEBUG: Form State Analysis');
            console.log('[RealTimeInsuranceBinding] ========================================');
            
            try {
                // بررسی وجود المان‌ها
                console.log('[RealTimeInsuranceBinding] 🔍 Element Existence Check:');
                console.log('[RealTimeInsuranceBinding] - Primary Provider:', $(CONFIG.selectors.primaryProvider).length);
                console.log('[RealTimeInsuranceBinding] - Primary Plan:', $(CONFIG.selectors.primaryPlan).length);
                console.log('[RealTimeInsuranceBinding] - Primary Policy Number:', $(CONFIG.selectors.primaryPolicyNumber).length);
                console.log('[RealTimeInsuranceBinding] - Primary Card Number:', $(CONFIG.selectors.primaryCardNumber).length);
                console.log('[RealTimeInsuranceBinding] - Supplementary Provider:', $(CONFIG.selectors.supplementaryProvider).length);
                console.log('[RealTimeInsuranceBinding] - Supplementary Plan:', $(CONFIG.selectors.supplementaryPlan).length);
                console.log('[RealTimeInsuranceBinding] - Supplementary Policy Number:', $(CONFIG.selectors.supplementaryPolicyNumber).length);
                console.log('[RealTimeInsuranceBinding] - Supplementary Expiry:', $(CONFIG.selectors.supplementaryExpiry).length);
                console.log('[RealTimeInsuranceBinding] - Save Button:', $(CONFIG.selectors.saveInsuranceBtn).length);
                
                // بررسی مقادیر فعلی
                console.log('[RealTimeInsuranceBinding] 🔍 Current Form Values:');
                var currentValues = this.captureFormValues();
                console.log('[RealTimeInsuranceBinding] Current values:', currentValues);
                
                // بررسی مقادیر اصلی
                console.log('[RealTimeInsuranceBinding] 🔍 Original Form Values:');
                console.log('[RealTimeInsuranceBinding] Original values:', this.originalFormValues);
                
                // بررسی وضعیت دکمه ذخیره
                var saveBtn = $(CONFIG.selectors.saveInsuranceBtn);
                console.log('[RealTimeInsuranceBinding] 🔍 Save Button State:');
                console.log('[RealTimeInsuranceBinding] - Exists:', saveBtn.length > 0);
                console.log('[RealTimeInsuranceBinding] - Disabled:', saveBtn.prop('disabled'));
                console.log('[RealTimeInsuranceBinding] - Classes:', saveBtn.attr('class'));
                
                // بررسی وضعیت فرم
                var insuranceSection = $(CONFIG.selectors.insuranceSection);
                console.log('[RealTimeInsuranceBinding] 🔍 Form Section State:');
                console.log('[RealTimeInsuranceBinding] - Exists:', insuranceSection.length > 0);
                console.log('[RealTimeInsuranceBinding] - Has editing class:', insuranceSection.hasClass('form-editing'));
                
                console.log('[RealTimeInsuranceBinding] ========================================');
                console.log('[RealTimeInsuranceBinding] 🔍 DEBUG: Form State Analysis Complete');
                
            } catch (error) {
                console.error('[RealTimeInsuranceBinding] ❌ Error in debug form state:', error);
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
