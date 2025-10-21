/**
 * Advanced State Manager - مدیریت‌کننده پیشرفته حالت (ES5)
 * ===========================================================
 * 
 * این ماژول از تکنولوژی‌های ES5 استفاده می‌کند:
 * - State Machine Pattern
 * - Observer Pattern
 * - Command Pattern
 * - Strategy Pattern
 * 
 * @author ClinicApp Development Team
 * @version 3.0.0
 * @since 2025-01-20
 */

(function(global, $) {
    'use strict';

    // ========================================
    // STATE MACHINE DEFINITION
    // ========================================
    var STATES = {
        IDLE: 'idle',
        LOADING: 'loading',
        EDITING: 'editing',
        SAVING: 'saving',
        ERROR: 'error',
        SUCCESS: 'success'
    };

    var TRANSITIONS = {
        'idle': ['loading', 'editing', 'error'],
        'loading': ['idle', 'editing', 'error'],
        'editing': ['saving', 'idle', 'error'],
        'saving': ['success', 'error', 'editing'],
        'error': ['idle', 'editing'],
        'success': ['idle', 'editing']
    };

    // ========================================
    // ADVANCED STATE MANAGER CONSTRUCTOR
    // ========================================
    function AdvancedStateManager() {
        this.currentState = STATES.IDLE;
        this.previousState = null;
        this.stateHistory = [];
        this.observers = [];
        this.commands = {};
        this.strategies = {};
        this.isInitialized = false;
        
        this.init();
    }

    // ========================================
    // INITIALIZATION - راه‌اندازی
    // ========================================
    AdvancedStateManager.prototype.init = function() {
        console.log('[AdvancedStateManager] 🚀 Initializing advanced state manager...');
        
        try {
            this.setupStateMachine();
            this.setupObservers();
            this.setupCommands();
            this.setupStrategies();
            
            this.isInitialized = true;
            console.log('[AdvancedStateManager] ✅ Advanced state manager initialized successfully');
            
        } catch (error) {
            console.error('[AdvancedStateManager] ❌ Initialization failed:', error);
            this.handleError(error);
        }
    };

    // ========================================
    // SETUP STATE MACHINE - تنظیم ماشین حالت
    // ========================================
    AdvancedStateManager.prototype.setupStateMachine = function() {
        console.log('[AdvancedStateManager] 🔄 Setting up state machine...');
        
        // Add state change logging
        var self = this;
        this.observeStateChange(function(newState, oldState) {
            console.log('[AdvancedStateManager] 🔄 State transition: ' + oldState + ' → ' + newState);
            self.stateHistory.push({
                from: oldState,
                to: newState,
                timestamp: Date.now()
            });
        });
        
        console.log('[AdvancedStateManager] ✅ State machine setup completed');
    };

    // ========================================
    // SETUP OBSERVERS - تنظیم ناظران
    // ========================================
    AdvancedStateManager.prototype.setupObservers = function() {
        console.log('[AdvancedStateManager] 👁️ Setting up observers...');
        
        var self = this;
        
        // State change observers
        this.observeStateChange(STATES.EDITING, function() {
            self.executeCommand('enableEditMode');
        });
        
        this.observeStateChange(STATES.IDLE, function() {
            self.executeCommand('disableEditMode');
        });
        
        this.observeStateChange(STATES.SAVING, function() {
            self.executeCommand('showSavingIndicator');
        });
        
        this.observeStateChange(STATES.SUCCESS, function() {
            self.executeCommand('showSuccessMessage');
        });
        
        this.observeStateChange(STATES.ERROR, function() {
            self.executeCommand('showErrorMessage');
        });
        
        console.log('[AdvancedStateManager] ✅ Observers setup completed');
    };

    // ========================================
    // SETUP COMMANDS - تنظیم دستورات
    // ========================================
    AdvancedStateManager.prototype.setupCommands = function() {
        console.log('[AdvancedStateManager] 📋 Setting up commands...');
        
        var self = this;
        
        // Edit mode commands
        this.commands.enableEditMode = function() {
            self.enableEditMode();
        };
        
        this.commands.disableEditMode = function() {
            self.disableEditMode();
        };
        
        // UI commands
        this.commands.showSavingIndicator = function() {
            self.showSavingIndicator();
        };
        
        this.commands.showSuccessMessage = function() {
            self.showSuccessMessage();
        };
        
        this.commands.showErrorMessage = function() {
            self.showErrorMessage();
        };
        
        // Form commands
        this.commands.updateFormState = function(data) {
            self.updateFormState(data);
        };
        
        this.commands.resetForm = function() {
            self.resetForm();
        };
        
        console.log('[AdvancedStateManager] ✅ Commands setup completed');
    };

    // ========================================
    // SETUP STRATEGIES - تنظیم استراتژی‌ها
    // ========================================
    AdvancedStateManager.prototype.setupStrategies = function() {
        console.log('[AdvancedStateManager] 🎯 Setting up strategies...');
        
        var self = this;
        
        // Validation strategies
        this.strategies.validateForm = function(data) {
            return self.validateForm(data);
        };
        
        this.strategies.validateChanges = function(changes) {
            return self.validateChanges(changes);
        };
        
        // Save strategies
        this.strategies.saveData = function(data) {
            return self.saveData(data);
        };
        
        // Load strategies
        this.strategies.loadData = function(patientId) {
            return self.loadData(patientId);
        };
        
        console.log('[AdvancedStateManager] ✅ Strategies setup completed');
    };

    // ========================================
    // STATE TRANSITION - انتقال حالت
    // ========================================
    AdvancedStateManager.prototype.transitionTo = function(newState, data) {
        console.log('[AdvancedStateManager] 🔄 Attempting transition to: ' + newState);
        
        if (!this.canTransitionTo(newState)) {
            console.warn('[AdvancedStateManager] ⚠️ Invalid transition: ' + this.currentState + ' → ' + newState);
            return false;
        }
        
        var oldState = this.currentState;
        this.previousState = oldState;
        this.currentState = newState;
        
        // Notify observers
        this.notifyStateChange(newState, oldState, data);
        
        return true;
    };

    // ========================================
    // CAN TRANSITION TO - بررسی امکان انتقال
    // ========================================
    AdvancedStateManager.prototype.canTransitionTo = function(newState) {
        var allowedTransitions = TRANSITIONS[this.currentState] || [];
        return allowedTransitions.indexOf(newState) !== -1;
    };

    // ========================================
    // OBSERVER PATTERN - الگوی ناظر
    // ========================================
    AdvancedStateManager.prototype.observeStateChange = function(callback) {
        if (typeof callback === 'function') {
            // General state change observer
            this.observeStateChange = callback;
        } else {
            // Specific state observer
            var state = callback;
            var handler = arguments[1];
            if (typeof handler === 'function') {
                this.observers.push({
                    state: state,
                    handler: handler
                });
            }
        }
    };

    // ========================================
    // NOTIFY STATE CHANGE - اطلاع‌رسانی تغییر حالت
    // ========================================
    AdvancedStateManager.prototype.notifyStateChange = function(newState, oldState, data) {
        // Notify general observer
        if (typeof this.observeStateChange === 'function') {
            this.observeStateChange(newState, oldState, data);
        }
        
        // Notify specific state observers
        var self = this;
        this.observers.forEach(function(observer) {
            if (observer.state === newState) {
                try {
                    observer.handler(data);
                } catch (error) {
                    console.error('[AdvancedStateManager] ❌ Observer error:', error);
                }
            }
        });
    };

    // ========================================
    // EXECUTE COMMAND - اجرای دستور
    // ========================================
    AdvancedStateManager.prototype.executeCommand = function(commandName, data) {
        console.log('[AdvancedStateManager] 📋 Executing command: ' + commandName);
        
        var command = this.commands[commandName];
        if (command) {
            try {
                command(data);
                console.log('[AdvancedStateManager] ✅ Command executed: ' + commandName);
            } catch (error) {
                console.error('[AdvancedStateManager] ❌ Command error: ' + commandName, error);
            }
        } else {
            console.warn('[AdvancedStateManager] ⚠️ Unknown command: ' + commandName);
        }
    };

    // ========================================
    // EXECUTE STRATEGY - اجرای استراتژی
    // ========================================
    AdvancedStateManager.prototype.executeStrategy = function(strategyName, data) {
        console.log('[AdvancedStateManager] 🎯 Executing strategy: ' + strategyName);
        
        var strategy = this.strategies[strategyName];
        if (strategy) {
            try {
                var result = strategy(data);
                console.log('[AdvancedStateManager] ✅ Strategy executed: ' + strategyName);
                return result;
            } catch (error) {
                console.error('[AdvancedStateManager] ❌ Strategy error: ' + strategyName, error);
                return null;
            }
        } else {
            console.warn('[AdvancedStateManager] ⚠️ Unknown strategy: ' + strategyName);
            return null;
        }
    };

    // ========================================
    // ENABLE EDIT MODE - فعال کردن حالت ویرایش
    // ========================================
    AdvancedStateManager.prototype.enableEditMode = function() {
        console.log('[AdvancedStateManager] ✅ Enabling edit mode...');
        
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
    AdvancedStateManager.prototype.disableEditMode = function() {
        console.log('[AdvancedStateManager] ❌ Disabling edit mode...');
        
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
    AdvancedStateManager.prototype.updateProgressSteps = function(state) {
        // Reset all steps
        $('.step').removeClass('active');
        
        switch (state) {
            case 'editing':
                $('.step3').addClass('active');
                break;
            case 'saving':
                $('.step4').addClass('active');
                break;
            case 'success':
                $('.step4').addClass('active');
                break;
            default:
                $('.step1, .step2').addClass('active');
                break;
        }
    };

    // ========================================
    // SHOW SAVING INDICATOR - نمایش نشانگر ذخیره
    // ========================================
    AdvancedStateManager.prototype.showSavingIndicator = function() {
        console.log('[AdvancedStateManager] 💾 Showing saving indicator...');
        
        var $saveBtn = $('#saveInsuranceBtn');
        if ($saveBtn.length > 0) {
            $saveBtn
                .prop('disabled', true)
                .html('<i class="fas fa-spinner fa-spin"></i> در حال ذخیره...');
        }
    };

    // ========================================
    // SHOW SUCCESS MESSAGE - نمایش پیام موفقیت
    // ========================================
    AdvancedStateManager.prototype.showSuccessMessage = function() {
        console.log('[AdvancedStateManager] ✅ Showing success message...');
        
        if (typeof toastr !== 'undefined') {
            toastr.success('اطلاعات بیمه با موفقیت ذخیره شد');
        }
    };

    // ========================================
    // SHOW ERROR MESSAGE - نمایش پیام خطا
    // ========================================
    AdvancedStateManager.prototype.showErrorMessage = function() {
        console.log('[AdvancedStateManager] ❌ Showing error message...');
        
        if (typeof toastr !== 'undefined') {
            toastr.error('خطا در ذخیره اطلاعات');
        }
    };

    // ========================================
    // SHOW CHANGE NOTIFICATION - نمایش اعلان تغییر
    // ========================================
    AdvancedStateManager.prototype.showChangeNotification = function() {
        if (typeof toastr !== 'undefined') {
            toastr.info('تغییرات در فرم تشخیص داده شد. برای ذخیره تغییرات روی دکمه ذخیره کلیک کنید.');
        }
    };

    // ========================================
    // VALIDATE FORM - اعتبارسنجی فرم
    // ========================================
    AdvancedStateManager.prototype.validateForm = function(data) {
        console.log('[AdvancedStateManager] 🔍 Validating form...');
        
        var errors = [];
        
        // Required field validation
        if (!data.primaryProvider) {
            errors.push('سازمان بیمه پایه الزامی است');
        }
        
        if (data.primaryProvider && !data.primaryPlan) {
            errors.push('طرح بیمه پایه الزامی است');
        }
        
        if (data.supplementaryProvider && !data.supplementaryPlan) {
            errors.push('طرح بیمه تکمیلی الزامی است');
        }
        
        return {
            isValid: errors.length === 0,
            errors: errors
        };
    };

    // ========================================
    // VALIDATE CHANGES - اعتبارسنجی تغییرات
    // ========================================
    AdvancedStateManager.prototype.validateChanges = function(changes) {
        console.log('[AdvancedStateManager] 🔍 Validating changes...');
        
        var errors = [];
        var self = this;
        
        changes.forEach(function(change) {
            if (change.field === 'primaryProvider' && !change.current) {
                errors.push('سازمان بیمه پایه الزامی است');
            }
        });
        
        return {
            isValid: errors.length === 0,
            errors: errors
        };
    };

    // ========================================
    // SAVE DATA - ذخیره داده
    // ========================================
    AdvancedStateManager.prototype.saveData = function(data) {
        console.log('[AdvancedStateManager] 💾 Saving data...');
        
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
                    reject(error);
                }
            });
        });
    };

    // ========================================
    // LOAD DATA - بارگذاری داده
    // ========================================
    AdvancedStateManager.prototype.loadData = function(patientId) {
        console.log('[AdvancedStateManager] 📥 Loading data...');
        
        var self = this;
        return new Promise(function(resolve, reject) {
            $.ajax({
                url: '/Reception/Insurance/Load',
                type: 'POST',
                data: { patientId: patientId },
                timeout: 30000,
                success: function(response) {
                    resolve(response);
                },
                error: function(xhr, status, error) {
                    reject(error);
                }
            });
        });
    };

    // ========================================
    // UPDATE FORM STATE - به‌روزرسانی وضعیت فرم
    // ========================================
    AdvancedStateManager.prototype.updateFormState = function(data) {
        console.log('[AdvancedStateManager] 🔄 Updating form state...');
        
        // Update form fields
        if (data.primaryProvider) {
            $('#insuranceProvider').val(data.primaryProvider);
        }
        if (data.primaryPlan) {
            $('#insurancePlan').val(data.primaryPlan);
        }
        if (data.primaryPolicyNumber) {
            $('#policyNumber').val(data.primaryPolicyNumber);
        }
        if (data.primaryCardNumber) {
            $('#cardNumber').val(data.primaryCardNumber);
        }
        if (data.supplementaryProvider) {
            $('#supplementaryProvider').val(data.supplementaryProvider);
        }
        if (data.supplementaryPlan) {
            $('#supplementaryPlan').val(data.supplementaryPlan);
        }
        if (data.supplementaryPolicyNumber) {
            $('#supplementaryPolicyNumber').val(data.supplementaryPolicyNumber);
        }
        if (data.supplementaryExpiry) {
            $('#supplementaryExpiry').val(data.supplementaryExpiry);
        }
    };

    // ========================================
    // RESET FORM - بازنشانی فرم
    // ========================================
    AdvancedStateManager.prototype.resetForm = function() {
        console.log('[AdvancedStateManager] 🔄 Resetting form...');
        
        // Clear form fields
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
        
        Object.values(selectors).forEach(function(selector) {
            $(selector).val('');
        });
    };

    // ========================================
    // HANDLE ERROR - مدیریت خطا
    // ========================================
    AdvancedStateManager.prototype.handleError = function(error) {
        console.error('[AdvancedStateManager] 🚨 Error:', error);
        
        // Transition to error state
        this.transitionTo(STATES.ERROR, { error: error.message || 'Unknown error' });
    };

    // ========================================
    // GET CURRENT STATE - دریافت حالت فعلی
    // ========================================
    AdvancedStateManager.prototype.getCurrentState = function() {
        return this.currentState;
    };

    // ========================================
    // GET STATE HISTORY - دریافت تاریخچه حالت
    // ========================================
    AdvancedStateManager.prototype.getStateHistory = function() {
        return this.stateHistory;
    };

    // ========================================
    // GET STATUS - دریافت وضعیت
    // ========================================
    AdvancedStateManager.prototype.getStatus = function() {
        return {
            currentState: this.currentState,
            previousState: this.previousState,
            isInitialized: this.isInitialized,
            historyLength: this.stateHistory.length,
            observersCount: this.observers.length,
            commandsCount: Object.keys(this.commands).length,
            strategiesCount: Object.keys(this.strategies).length
        };
    };

    // ========================================
    // CLEANUP - پاکسازی
    // ========================================
    AdvancedStateManager.prototype.cleanup = function() {
        console.log('[AdvancedStateManager] 🧹 Cleaning up...');
        
        try {
            // Clear observers
            this.observers = [];
            
            // Clear commands
            this.commands = {};
            
            // Clear strategies
            this.strategies = {};
            
            // Reset state
            this.currentState = STATES.IDLE;
            this.previousState = null;
            this.stateHistory = [];
            
            console.log('[AdvancedStateManager] ✅ Cleanup completed');
            
        } catch (error) {
            console.error('[AdvancedStateManager] ❌ Error during cleanup:', error);
        }
    };

    // ========================================
    // GLOBAL INSTANCE - نمونه سراسری
    // ========================================
    var stateManagerInstance = null;

    // Initialize when DOM is ready
    $(document).ready(function() {
        try {
            stateManagerInstance = new AdvancedStateManager();
            window.AdvancedStateManager = stateManagerInstance;
            console.log('[AdvancedStateManager] 🌟 Global instance created');
        } catch (error) {
            console.error('[AdvancedStateManager] ❌ Failed to create global instance:', error);
        }
    });

    // Cleanup on page unload
    $(window).on('beforeunload', function() {
        if (stateManagerInstance) {
            stateManagerInstance.cleanup();
        }
    });

})(window, jQuery);