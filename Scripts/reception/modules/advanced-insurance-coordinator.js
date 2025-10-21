/**
 * Advanced Insurance Coordinator - هماهنگ‌کننده پیشرفته بیمه (ES5)
 * =================================================================
 * 
 * این ماژول از تکنولوژی‌های ES5 استفاده می‌کند:
 * - Event Delegation Pattern
 * - Observer Pattern
 * - State Machine Pattern
 * - Performance Monitoring
 * 
 * @author ClinicApp Development Team
 * @version 3.0.0
 * @since 2025-01-20
 */

(function(global, $) {
    'use strict';

    // ========================================
    // ADVANCED INSURANCE COORDINATOR CONSTRUCTOR
    // ========================================
    function AdvancedInsuranceCoordinator() {
        this.state = {
            isInitialized: false,
            isEditMode: false,
            isProcessing: false,
            patientId: null,
            originalValues: null,
            currentValues: null
        };

        this.observers = [];
        this.abortController = null;
        this.debounceTimers = {};
        this.eventQueue = [];
        this.processingQueue = false;

        // Modern event delegation
        this.eventDelegator = this.createEventDelegator();
        
        // State machine for form states
        this.stateMachine = this.createStateMachine();
        
        // Object-based change detection
        this.changeDetector = this.createChangeDetector();
        
        this.init();
    }

    // ========================================
    // INITIALIZATION - راه‌اندازی پیشرفته
    // ========================================
    AdvancedInsuranceCoordinator.prototype.init = function() {
        console.log('[AdvancedInsuranceCoordinator] 🚀 Initializing advanced coordinator...');
        
        try {
            this.setupModernEventHandling();
            this.setupStateMachine();
            this.setupChangeDetection();
            this.setupPerformanceMonitoring();
            
            this.state.isInitialized = true;
            console.log('[AdvancedInsuranceCoordinator] ✅ Advanced coordinator initialized successfully');
            
        } catch (error) {
            console.error('[AdvancedInsuranceCoordinator] ❌ Initialization failed:', error);
            this.handleCriticalError(error);
        }
    };

    // ========================================
    // MODERN EVENT DELEGATION - مدیریت رویدادهای مدرن
    // ========================================
    AdvancedInsuranceCoordinator.prototype.createEventDelegator = function() {
        var eventMap = {};
        
        return {
            on: function(selector, event, handler, options) {
                var key = selector + ':' + event;
                if (!eventMap[key]) {
                    eventMap[key] = [];
                }
                eventMap[key].push({ handler: handler, options: options || {} });
            },
            
            off: function(selector, event) {
                var key = selector + ':' + event;
                delete eventMap[key];
            },
            
            trigger: function(selector, event, data) {
                var key = selector + ':' + event;
                var handlers = eventMap[key] || [];
                handlers.forEach(function(item) {
                    try {
                        item.handler(data);
                    } catch (error) {
                        console.error('[EventDelegator] Handler error:', error);
                    }
                });
            }
        };
    };

    // ========================================
    // SETUP MODERN EVENT HANDLING - تنظیم مدیریت رویدادهای مدرن
    // ========================================
    AdvancedInsuranceCoordinator.prototype.setupModernEventHandling = function() {
        console.log('[AdvancedInsuranceCoordinator] 🔗 Setting up modern event handling...');
        
        var self = this;
        
        // Single event listener with delegation
        $(document).on('change.advancedInsurance input.advancedInsurance', 
            '#insuranceProvider, #insurancePlan, #policyNumber, #cardNumber, #supplementaryProvider, #supplementaryPlan, #supplementaryPolicyNumber, #supplementaryExpiry',
            function(event) {
                self.createDebouncedHandler(self.handleFormChange.bind(self), 300)(event);
            }
        );

        // Save button with single handler
        $(document).on('click.advancedInsurance', '#saveInsuranceBtn', function(event) {
            self.handleSaveClick(event);
        });
        
        // Patient search events
        $(document).on('patientSearchSuccess.advancedInsurance', function(event, data) {
            self.handlePatientSearchSuccess(data);
        });
        
        console.log('[AdvancedInsuranceCoordinator] ✅ Modern event handling setup completed');
    };

    // ========================================
    // DEBOUNCED HANDLER - مدیریت debounced
    // ========================================
    AdvancedInsuranceCoordinator.prototype.createDebouncedHandler = function(handler, delay) {
        var self = this;
        return function(event) {
            var elementId = event.target.id;
            
            // Cancel previous timer
            if (self.debounceTimers[elementId]) {
                clearTimeout(self.debounceTimers[elementId]);
            }
            
            // Set new timer
            var timer = setTimeout(function() {
                delete self.debounceTimers[elementId];
                handler(event);
            }, delay);
            
            self.debounceTimers[elementId] = timer;
        };
    };

    // ========================================
    // STATE MACHINE - ماشین حالت
    // ========================================
    AdvancedInsuranceCoordinator.prototype.createStateMachine = function() {
        var states = {
            IDLE: 'idle',
            LOADING: 'loading',
            EDITING: 'editing',
            SAVING: 'saving',
            ERROR: 'error'
        };

        var transitions = {
            'idle': ['loading', 'editing'],
            'loading': ['idle', 'error'],
            'editing': ['saving', 'idle'],
            'saving': ['idle', 'error'],
            'error': ['idle']
        };

        return {
            currentState: states.IDLE,
            states: states,
            transitions: transitions,
            
            canTransition: function(toState) {
                var allowedTransitions = transitions[this.currentState] || [];
                return allowedTransitions.indexOf(toState) !== -1;
            },
            
            transition: function(toState) {
                if (this.canTransition(toState)) {
                    var previousState = this.currentState;
                    this.currentState = toState;
                    console.log('[StateMachine] ' + previousState + ' → ' + toState);
                    return true;
                }
                console.warn('[StateMachine] Invalid transition: ' + this.currentState + ' → ' + toState);
                return false;
            }
        };
    };

    // ========================================
    // SETUP STATE MACHINE - تنظیم ماشین حالت
    // ========================================
    AdvancedInsuranceCoordinator.prototype.setupStateMachine = function() {
        console.log('[AdvancedInsuranceCoordinator] 🔄 Setting up state machine...');
        
        var self = this;
        
        // State change observers
        this.observeStateChange('EDITING', function() {
            self.enableEditMode();
        });
        
        this.observeStateChange('IDLE', function() {
            self.disableEditMode();
        });
        
        console.log('[AdvancedInsuranceCoordinator] ✅ State machine setup completed');
    };

    // ========================================
    // OBSERVER PATTERN - الگوی ناظر
    // ========================================
    AdvancedInsuranceCoordinator.prototype.observeStateChange = function(state, callback) {
        this.observers.push({
            state: state,
            callback: callback
        });
    };

    AdvancedInsuranceCoordinator.prototype.notifyStateChange = function(state) {
        var self = this;
        this.observers.forEach(function(observer) {
            if (observer.state === state) {
                try {
                    observer.callback();
                } catch (error) {
                    console.error('[Observer] Callback error:', error);
                }
            }
        });
    };

    // ========================================
    // OBJECT-BASED CHANGE DETECTION - تشخیص تغییرات با Object
    // ========================================
    AdvancedInsuranceCoordinator.prototype.createChangeDetector = function() {
        var originalValues = {};
        var currentValues = {};
        
        return {
            set: function(property, value) {
                var oldValue = currentValues[property];
                currentValues[property] = value;
                
                // Check if value actually changed
                if (oldValue !== value && originalValues[property] !== undefined) {
                    this.handleValueChange(property, oldValue, value);
                }
                
                return true;
            },
            
            handleValueChange: function(property, oldValue, newValue) {
                console.log('[AdvancedInsuranceCoordinator] 🔄 Value changed: ' + property + ' (' + oldValue + ' → ' + newValue + ')');
                
                // Update state machine
                if (this.stateMachine.currentState === this.stateMachine.states.IDLE) {
                    this.stateMachine.transition(this.stateMachine.states.EDITING);
                }
            }
        };
    };

    // ========================================
    // SETUP CHANGE DETECTION - تنظیم تشخیص تغییرات
    // ========================================
    AdvancedInsuranceCoordinator.prototype.setupChangeDetection = function() {
        console.log('[AdvancedInsuranceCoordinator] 🔍 Setting up change detection...');
        
        var self = this;
        
        // Capture initial values after form is loaded
        setTimeout(function() {
            self.captureInitialValues();
        }, 1000);
        
        console.log('[AdvancedInsuranceCoordinator] ✅ Change detection setup completed');
    };

    // ========================================
    // CAPTURE INITIAL VALUES - ثبت مقادیر اولیه
    // ========================================
    AdvancedInsuranceCoordinator.prototype.captureInitialValues = function() {
        console.log('[AdvancedInsuranceCoordinator] 📝 Capturing initial values...');
        
        var selectors = {
            primaryProvider: '#insuranceProvider',
            primaryPlan: '#insurancePlan',
            primaryPolicyNumber: '#policyNumber',
            primaryCardNumber: '#cardNumber',
            supplementaryProvider: '#supplementaryProvider',
            supplementaryPlan: '#supplementaryPlan',
            supplementaryPolicyNumber: '#supplementaryPolicyNumber',
            supplementaryExpiry: '#supplementaryExpiry'
        };

        var values = {};
        var self = this;
        
        Object.keys(selectors).forEach(function(key) {
            var selector = selectors[key];
            var element = $(selector);
            values[key] = element.length > 0 ? (element.val() || '') : '';
        });

        this.state.originalValues = values;
        this.state.currentValues = values;
        
        console.log('[AdvancedInsuranceCoordinator] ✅ Initial values captured:', this.state.originalValues);
    };

    // ========================================
    // HANDLE FORM CHANGE - مدیریت تغییر فرم
    // ========================================
    AdvancedInsuranceCoordinator.prototype.handleFormChange = function(event) {
        if (this.state.isProcessing) {
            console.log('[AdvancedInsuranceCoordinator] ⚠️ Already processing, skipping change');
            return;
        }

        console.log('[AdvancedInsuranceCoordinator] 🔄 Handling form change:', event.target.id);
        
        try {
            this.state.isProcessing = true;
            
            // Update current values
            this.updateCurrentValues();
            
            // Check for changes
            var hasChanges = this.detectChanges();
            
            if (hasChanges) {
                this.stateMachine.transition(this.stateMachine.states.EDITING);
            } else {
                this.stateMachine.transition(this.stateMachine.states.IDLE);
            }
            
        } catch (error) {
            console.error('[AdvancedInsuranceCoordinator] ❌ Error handling form change:', error);
            this.stateMachine.transition(this.stateMachine.states.ERROR);
        } finally {
            this.state.isProcessing = false;
        }
    };

    // ========================================
    // UPDATE CURRENT VALUES - به‌روزرسانی مقادیر فعلی
    // ========================================
    AdvancedInsuranceCoordinator.prototype.updateCurrentValues = function() {
        var selectors = {
            primaryProvider: '#insuranceProvider',
            primaryPlan: '#insurancePlan',
            primaryPolicyNumber: '#policyNumber',
            primaryCardNumber: '#cardNumber',
            supplementaryProvider: '#supplementaryProvider',
            supplementaryPlan: '#supplementaryPlan',
            supplementaryPolicyNumber: '#supplementaryPolicyNumber',
            supplementaryExpiry: '#supplementaryExpiry'
        };

        var self = this;
        Object.keys(selectors).forEach(function(key) {
            var selector = selectors[key];
            var element = $(selector);
            if (element.length > 0) {
                self.state.currentValues[key] = element.val() || '';
            }
        });
    };

    // ========================================
    // DETECT CHANGES - تشخیص تغییرات
    // ========================================
    AdvancedInsuranceCoordinator.prototype.detectChanges = function() {
        if (!this.state.originalValues || !this.state.currentValues) {
            return false;
        }

        var changes = [];
        var self = this;
        
        Object.keys(this.state.currentValues).forEach(function(key) {
            var original = self.state.originalValues[key] || '';
            var current = self.state.currentValues[key] || '';
            
            if (original !== current) {
                changes.push({
                    field: key,
                    original: original,
                    current: current
                });
            }
        });

        console.log('[AdvancedInsuranceCoordinator] 📊 Changes detected:', changes.length);
        return changes.length > 0;
    };

    // ========================================
    // ENABLE EDIT MODE - فعال کردن حالت ویرایش
    // ========================================
    AdvancedInsuranceCoordinator.prototype.enableEditMode = function() {
        console.log('[AdvancedInsuranceCoordinator] ✅ Enabling edit mode...');
        
        this.state.isEditMode = true;
        
        // Show save button
        var $saveBtn = $('#saveInsuranceBtn');
        if ($saveBtn.length > 0) {
            $saveBtn
                .prop('disabled', false)
                .removeClass('d-none btn-secondary')
                .addClass('btn-success')
                .html('<i class="fas fa-save"></i> ذخیره اطلاعات بیمه');
        }
        
        // Update progress steps
        this.updateProgressSteps('editing');
        
        // Show notification
        this.showChangeNotification();
    };

    // ========================================
    // DISABLE EDIT MODE - غیرفعال کردن حالت ویرایش
    // ========================================
    AdvancedInsuranceCoordinator.prototype.disableEditMode = function() {
        console.log('[AdvancedInsuranceCoordinator] ❌ Disabling edit mode...');
        
        this.state.isEditMode = false;
        
        // Hide save button
        var $saveBtn = $('#saveInsuranceBtn');
        if ($saveBtn.length > 0) {
            $saveBtn
                .prop('disabled', true)
                .addClass('d-none btn-secondary')
                .removeClass('btn-success');
        }
        
        // Update progress steps
        this.updateProgressSteps('idle');
    };

    // ========================================
    // UPDATE PROGRESS STEPS - به‌روزرسانی مراحل پیشرفت
    // ========================================
    AdvancedInsuranceCoordinator.prototype.updateProgressSteps = function(state) {
        // Reset all steps
        $('.step').removeClass('active');
        
        switch (state) {
            case 'editing':
                $('.step3').addClass('active');
                break;
            case 'saving':
                $('.step4').addClass('active');
                break;
            default:
                $('.step1, .step2').addClass('active');
                break;
        }
    };

    // ========================================
    // SHOW CHANGE NOTIFICATION - نمایش اعلان تغییر
    // ========================================
    AdvancedInsuranceCoordinator.prototype.showChangeNotification = function() {
        if (typeof toastr !== 'undefined') {
            toastr.info('تغییرات در فرم تشخیص داده شد. برای ذخیره تغییرات روی دکمه ذخیره کلیک کنید.');
        }
    };

    // ========================================
    // HANDLE SAVE CLICK - مدیریت کلیک ذخیره
    // ========================================
    AdvancedInsuranceCoordinator.prototype.handleSaveClick = function(event) {
        event.preventDefault();
        console.log('[AdvancedInsuranceCoordinator] 💾 Save button clicked');
        
        if (this.stateMachine.transition(this.stateMachine.states.SAVING)) {
            this.performSave();
        }
    };

    // ========================================
    // PERFORM SAVE - انجام ذخیره
    // ========================================
    AdvancedInsuranceCoordinator.prototype.performSave = function() {
        console.log('[AdvancedInsuranceCoordinator] 💾 Performing save...');
        
        var self = this;
        
        try {
            // Validate form
            var validationResult = this.validateForm();
            if (!validationResult.isValid) {
                this.showValidationErrors(validationResult.errors);
                this.stateMachine.transition(this.stateMachine.states.EDITING);
                return;
            }
            
            // Prepare save data
            var saveData = this.prepareSaveData();
            
            // Perform save
            this.saveInsuranceData(saveData).then(function(result) {
                if (result.success) {
                    self.handleSaveSuccess(result);
                } else {
                    self.handleSaveError(result);
                }
            }).catch(function(error) {
                console.error('[AdvancedInsuranceCoordinator] ❌ Save error:', error);
                self.handleSaveError({ message: 'خطا در ذخیره اطلاعات' });
            });
            
        } catch (error) {
            console.error('[AdvancedInsuranceCoordinator] ❌ Save error:', error);
            this.handleSaveError({ message: 'خطا در ذخیره اطلاعات' });
        }
    };

    // ========================================
    // VALIDATE FORM - اعتبارسنجی فرم
    // ========================================
    AdvancedInsuranceCoordinator.prototype.validateForm = function() {
        var errors = [];
        var values = this.state.currentValues;
        
        // Required field validation
        if (!values.primaryProvider) {
            errors.push('سازمان بیمه پایه الزامی است');
        }
        
        if (values.primaryProvider && !values.primaryPlan) {
            errors.push('طرح بیمه پایه الزامی است');
        }
        
        if (values.supplementaryProvider && !values.supplementaryPlan) {
            errors.push('طرح بیمه تکمیلی الزامی است');
        }
        
        return {
            isValid: errors.length === 0,
            errors: errors
        };
    };

    // ========================================
    // PREPARE SAVE DATA - آماده‌سازی داده‌های ذخیره
    // ========================================
    AdvancedInsuranceCoordinator.prototype.prepareSaveData = function() {
        return {
            PatientId: this.state.patientId,
            PrimaryInsuranceProviderId: this.state.currentValues.primaryProvider,
            PrimaryInsurancePlanId: this.state.currentValues.primaryPlan,
            PrimaryPolicyNumber: this.state.currentValues.primaryPolicyNumber,
            PrimaryCardNumber: this.state.currentValues.primaryCardNumber,
            SupplementaryInsuranceProviderId: this.state.currentValues.supplementaryProvider,
            SupplementaryInsurancePlanId: this.state.currentValues.supplementaryPlan,
            SupplementaryPolicyNumber: this.state.currentValues.supplementaryPolicyNumber,
            SupplementaryExpiryDate: this.state.currentValues.supplementaryExpiry,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        };
    };

    // ========================================
    // SAVE INSURANCE DATA - ذخیره اطلاعات بیمه
    // ========================================
    AdvancedInsuranceCoordinator.prototype.saveInsuranceData = function(data) {
        var self = this;
        return new Promise(function(resolve, reject) {
            $.ajax({
                url: '/Reception/Insurance/Save',
                type: 'POST',
                data: data,
                timeout: 30000,
                success: function(response) {
                    resolve(response);
                },
                error: function(xhr, status, error) {
                    reject({ message: 'خطا در ارتباط با سرور' });
                }
            });
        });
    };

    // ========================================
    // HANDLE SAVE SUCCESS - مدیریت موفقیت ذخیره
    // ========================================
    AdvancedInsuranceCoordinator.prototype.handleSaveSuccess = function(result) {
        console.log('[AdvancedInsuranceCoordinator] ✅ Save successful');
        
        // Update original values
        this.state.originalValues = Object.assign({}, this.state.currentValues);
        
        // Transition to idle state
        this.stateMachine.transition(this.stateMachine.states.IDLE);
        
        // Show success message
        if (typeof toastr !== 'undefined') {
            toastr.success('اطلاعات بیمه با موفقیت ذخیره شد');
        }
    };

    // ========================================
    // HANDLE SAVE ERROR - مدیریت خطای ذخیره
    // ========================================
    AdvancedInsuranceCoordinator.prototype.handleSaveError = function(result) {
        console.error('[AdvancedInsuranceCoordinator] ❌ Save failed:', result);
        
        // Transition back to editing state
        this.stateMachine.transition(this.stateMachine.states.EDITING);
        
        // Show error message
        if (typeof toastr !== 'undefined') {
            toastr.error(result.message || 'خطا در ذخیره اطلاعات');
        }
    };

    // ========================================
    // HANDLE PATIENT SEARCH SUCCESS - مدیریت موفقیت جستجوی بیمار
    // ========================================
    AdvancedInsuranceCoordinator.prototype.handlePatientSearchSuccess = function(data) {
        console.log('[AdvancedInsuranceCoordinator] 👤 Patient search success:', data);
        
        if (data && data.PatientId) {
            this.state.patientId = data.PatientId;
            
            // Load insurance data
            this.loadPatientInsurance();
        }
    };

    // ========================================
    // LOAD PATIENT INSURANCE - بارگذاری اطلاعات بیمه بیمار
    // ========================================
    AdvancedInsuranceCoordinator.prototype.loadPatientInsurance = function() {
        if (!this.state.patientId) {
            console.warn('[AdvancedInsuranceCoordinator] ⚠️ No patient ID available');
            return;
        }
        
        console.log('[AdvancedInsuranceCoordinator] 🏥 Loading patient insurance...');
        
        var self = this;
        $.ajax({
            url: '/Reception/Insurance/Load',
            type: 'POST',
            data: { patientId: this.state.patientId },
            success: function(response) {
                self.handleInsuranceLoadSuccess(response);
            },
            error: function(xhr, status, error) {
                console.error('[AdvancedInsuranceCoordinator] ❌ Insurance load error:', error);
            }
        });
    };

    // ========================================
    // HANDLE INSURANCE LOAD SUCCESS - مدیریت موفقیت بارگذاری بیمه
    // ========================================
    AdvancedInsuranceCoordinator.prototype.handleInsuranceLoadSuccess = function(response) {
        console.log('[AdvancedInsuranceCoordinator] 🏥 Insurance load success:', response);
        
        if (response.success && response.data) {
            this.bindInsuranceData(response.data);
            
            // Update original values after binding
            var self = this;
            setTimeout(function() {
                self.captureInitialValues();
            }, 2000);
        }
    };

    // ========================================
    // BIND INSURANCE DATA - اتصال اطلاعات بیمه
    // ========================================
    AdvancedInsuranceCoordinator.prototype.bindInsuranceData = function(data) {
        console.log('[AdvancedInsuranceCoordinator] 🔗 Binding insurance data...');
        
        // Bind primary insurance
        if (data.PrimaryInsurance) {
            this.bindPrimaryInsurance(data.PrimaryInsurance);
        }
        
        // Bind supplementary insurance
        if (data.SupplementaryInsurance) {
            this.bindSupplementaryInsurance(data.SupplementaryInsurance);
        }
    };

    // ========================================
    // BIND PRIMARY INSURANCE - اتصال بیمه پایه
    // ========================================
    AdvancedInsuranceCoordinator.prototype.bindPrimaryInsurance = function(insurance) {
        if (insurance.ProviderId) {
            $('#insuranceProvider').val(insurance.ProviderId);
        }
        if (insurance.PlanId) {
            $('#insurancePlan').val(insurance.PlanId);
        }
        if (insurance.PolicyNumber) {
            $('#policyNumber').val(insurance.PolicyNumber);
        }
        if (insurance.CardNumber) {
            $('#cardNumber').val(insurance.CardNumber);
        }
    };

    // ========================================
    // BIND SUPPLEMENTARY INSURANCE - اتصال بیمه تکمیلی
    // ========================================
    AdvancedInsuranceCoordinator.prototype.bindSupplementaryInsurance = function(insurance) {
        if (insurance.ProviderId) {
            $('#supplementaryProvider').val(insurance.ProviderId);
        }
        if (insurance.PlanId) {
            $('#supplementaryPlan').val(insurance.PlanId);
        }
        if (insurance.PolicyNumber) {
            $('#supplementaryPolicyNumber').val(insurance.PolicyNumber);
        }
    };

    // ========================================
    // SETUP PERFORMANCE MONITORING - تنظیم نظارت بر عملکرد
    // ========================================
    AdvancedInsuranceCoordinator.prototype.setupPerformanceMonitoring = function() {
        console.log('[AdvancedInsuranceCoordinator] 📊 Setting up performance monitoring...');
        
        var self = this;
        
        // Monitor memory usage
        if (performance.memory) {
            setInterval(function() {
                var memory = performance.memory;
                if (memory.usedJSHeapSize > 50 * 1024 * 1024) { // 50MB
                    console.warn('[AdvancedInsuranceCoordinator] ⚠️ High memory usage:', memory.usedJSHeapSize);
                }
            }, 30000);
        }
        
        console.log('[AdvancedInsuranceCoordinator] ✅ Performance monitoring setup completed');
    };

    // ========================================
    // SHOW VALIDATION ERRORS - نمایش خطاهای اعتبارسنجی
    // ========================================
    AdvancedInsuranceCoordinator.prototype.showValidationErrors = function(errors) {
        var errorMessages = errors.map(function(error) {
            // Handle both string and object errors
            if (typeof error === 'string') {
                return error;
            } else if (error && error.message) {
                return error.message;
            } else if (error && typeof error === 'object') {
                return JSON.stringify(error);
            } else {
                return String(error);
            }
        });
        
        var message = errorMessages.join('\n');
        if (typeof toastr !== 'undefined') {
            toastr.error(message);
        } else {
            console.error('[AdvancedInsuranceCoordinator] Validation errors:', message);
        }
    };

    // ========================================
    // HANDLE CRITICAL ERROR - مدیریت خطای بحرانی
    // ========================================
    AdvancedInsuranceCoordinator.prototype.handleCriticalError = function(error) {
        console.error('[AdvancedInsuranceCoordinator] 🚨 Critical error:', error);
        
        // Reset state
        this.state.isProcessing = false;
        this.stateMachine.transition(this.stateMachine.states.ERROR);
        
        // Show user-friendly error
        if (typeof toastr !== 'undefined') {
            toastr.error('خطای بحرانی در سیستم. لطفاً صفحه را بازخوانی کنید.');
        }
    };

    // ========================================
    // CLEANUP - پاکسازی
    // ========================================
    AdvancedInsuranceCoordinator.prototype.cleanup = function() {
        console.log('[AdvancedInsuranceCoordinator] 🧹 Cleaning up...');
        
        // Clear timers
        var self = this;
        Object.keys(this.debounceTimers).forEach(function(timerId) {
            clearTimeout(self.debounceTimers[timerId]);
        });
        this.debounceTimers = {};
        
        // Abort ongoing requests
        if (this.abortController) {
            this.abortController.abort();
        }
        
        // Remove event listeners
        $(document).off('.advancedInsurance');
        
        console.log('[AdvancedInsuranceCoordinator] ✅ Cleanup completed');
    };

    // ========================================
    // GLOBAL INSTANCE - نمونه سراسری
    // ========================================
    var coordinatorInstance = null;

    // Initialize when DOM is ready
    $(document).ready(function() {
        try {
            coordinatorInstance = new AdvancedInsuranceCoordinator();
            window.AdvancedInsuranceCoordinator = coordinatorInstance;
            console.log('[AdvancedInsuranceCoordinator] 🌟 Global instance created');
        } catch (error) {
            console.error('[AdvancedInsuranceCoordinator] ❌ Failed to create global instance:', error);
        }
    });

    // Cleanup on page unload
    $(window).on('beforeunload', function() {
        if (coordinatorInstance) {
            coordinatorInstance.cleanup();
        }
    });

})(window, jQuery);