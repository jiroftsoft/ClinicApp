/**
 * Advanced Change Detector - تشخیص‌دهنده پیشرفته تغییرات (ES5)
 * ==============================================================
 * 
 * این ماژول از تکنولوژی‌های ES5 استفاده می‌کند:
 * - Object-based Change Detection
 * - Event Delegation Pattern
 * - Observer Pattern
 * - Debouncing
 * 
 * @author ClinicApp Development Team
 * @version 3.0.0
 * @since 2025-01-20
 */

(function(global, $) {
    'use strict';

    // ========================================
    // ADVANCED CHANGE DETECTOR CONSTRUCTOR
    // ========================================
    function AdvancedChangeDetector() {
        this.originalValues = {};
        this.currentValues = {};
        this.changeObservers = [];
        this.debounceTimers = {};
        this.isInitialized = false;
        this.isCapturing = false;
        
        // Form field selectors
        this.selectors = {
            primaryProvider: '#insuranceProvider',
            primaryPlan: '#insurancePlan',
            primaryPolicyNumber: '#policyNumber',
            primaryCardNumber: '#cardNumber',
            supplementaryProvider: '#supplementaryProvider',
            supplementaryPlan: '#supplementaryPlan',
            supplementaryPolicyNumber: '#supplementaryPolicyNumber',
            supplementaryExpiry: '#supplementaryExpiry'
        };
        
        this.init();
    }

    // ========================================
    // INITIALIZATION - راه‌اندازی
    // ========================================
    AdvancedChangeDetector.prototype.init = function() {
        console.log('[AdvancedChangeDetector] 🚀 Initializing advanced change detector...');
        
        try {
            this.setupEventDelegation();
            this.setupAbortController();
            
            this.isInitialized = true;
            console.log('[AdvancedChangeDetector] ✅ Advanced change detector initialized successfully');
            
        } catch (error) {
            console.error('[AdvancedChangeDetector] ❌ Initialization failed:', error);
            this.handleError(error);
        }
    };

    // ========================================
    // SETUP EVENT DELEGATION - تنظیم Event Delegation
    // ========================================
    AdvancedChangeDetector.prototype.setupEventDelegation = function() {
        console.log('[AdvancedChangeDetector] 🔗 Setting up event delegation...');
        
        var self = this;
        var selectors = Object.values(this.selectors).join(',');
        
        // Single event listener for all form changes
        $(document).on('change.advancedChangeDetector input.advancedChangeDetector', 
            selectors,
            function(event) {
                self.createDebouncedHandler(self.handleFormChange.bind(self), 100)(event);
            }
        );
        
        console.log('[AdvancedChangeDetector] ✅ Event delegation setup completed');
    };

    // ========================================
    // SETUP ABORT CONTROLLER - تنظیم AbortController
    // ========================================
    AdvancedChangeDetector.prototype.setupAbortController = function() {
        console.log('[AdvancedChangeDetector] 🛑 Setting up abort controller...');
        
        // Simple abort mechanism for ES5
        this.abortController = {
            aborted: false,
            abort: function() {
                this.aborted = true;
            }
        };
        
        console.log('[AdvancedChangeDetector] ✅ Abort controller setup completed');
    };

    // ========================================
    // DEBOUNCED HANDLER - مدیریت debounced
    // ========================================
    AdvancedChangeDetector.prototype.createDebouncedHandler = function(handler, delay) {
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
    // CAPTURE INITIAL VALUES - ثبت مقادیر اولیه
    // ========================================
    AdvancedChangeDetector.prototype.captureInitialValues = function() {
        console.log('[AdvancedChangeDetector] 📝 Capturing initial values...');
        
        this.isCapturing = true;
        
        try {
            var self = this;
            Object.keys(this.selectors).forEach(function(key) {
                var selector = self.selectors[key];
                var element = $(selector);
                if (element.length > 0) {
                    var value = element.val() || '';
                    self.originalValues[key] = value;
                    self.currentValues[key] = value;
                    console.log('[AdvancedChangeDetector] 📝 ' + key + ': ' + value);
                }
            });
            
            console.log('[AdvancedChangeDetector] ✅ Initial values captured successfully');
            
        } catch (error) {
            console.error('[AdvancedChangeDetector] ❌ Error capturing initial values:', error);
        } finally {
            this.isCapturing = false;
        }
    };

    // ========================================
    // UPDATE ORIGINAL VALUES - به‌روزرسانی مقادیر اولیه
    // ========================================
    AdvancedChangeDetector.prototype.updateOriginalValues = function() {
        console.log('[AdvancedChangeDetector] 📝 Updating original values...');
        
        try {
            var self = this;
            Object.keys(this.selectors).forEach(function(key) {
                var selector = self.selectors[key];
                var element = $(selector);
                if (element.length > 0) {
                    var value = element.val() || '';
                    self.originalValues[key] = value;
                    self.currentValues[key] = value;
                }
            });
            
            console.log('[AdvancedChangeDetector] ✅ Original values updated successfully');
            
        } catch (error) {
            console.error('[AdvancedChangeDetector] ❌ Error updating original values:', error);
        }
    };

    // ========================================
    // HANDLE FORM CHANGE - مدیریت تغییر فرم
    // ========================================
    AdvancedChangeDetector.prototype.handleFormChange = function(event) {
        if (this.isCapturing) {
            console.log('[AdvancedChangeDetector] ⚠️ Currently capturing, skipping change');
            return;
        }
        
        var elementId = event.target.id;
        var fieldKey = this.getElementKey(elementId);
        
        if (!fieldKey) {
            console.warn('[AdvancedChangeDetector] ⚠️ Unknown field:', elementId);
            return;
        }
        
        console.log('[AdvancedChangeDetector] 🔄 Form change detected: ' + fieldKey);
        
        try {
            // Update current value
            var newValue = $(event.target).val() || '';
            this.currentValues[fieldKey] = newValue;
            
            // Check for changes
            var hasChanges = this.detectChanges();
            
            // Notify observers
            this.notifyChangeObservers(hasChanges, fieldKey, newValue);
            
        } catch (error) {
            console.error('[AdvancedChangeDetector] ❌ Error handling form change:', error);
        }
    };

    // ========================================
    // GET ELEMENT KEY - دریافت کلید عنصر
    // ========================================
    AdvancedChangeDetector.prototype.getElementKey = function(elementId) {
        var keyMap = {
            'insuranceProvider': 'primaryProvider',
            'insurancePlan': 'primaryPlan',
            'policyNumber': 'primaryPolicyNumber',
            'cardNumber': 'primaryCardNumber',
            'supplementaryProvider': 'supplementaryProvider',
            'supplementaryPlan': 'supplementaryPlan',
            'supplementaryPolicyNumber': 'supplementaryPolicyNumber',
            'supplementaryExpiry': 'supplementaryExpiry'
        };
        
        return keyMap[elementId] || null;
    };

    // ========================================
    // DETECT CHANGES - تشخیص تغییرات
    // ========================================
    AdvancedChangeDetector.prototype.detectChanges = function() {
        if (Object.keys(this.originalValues).length === 0 || Object.keys(this.currentValues).length === 0) {
            return false;
        }
        
        var changes = [];
        var self = this;
        
        Object.keys(this.currentValues).forEach(function(key) {
            var originalValue = self.originalValues[key] || '';
            var currentValue = self.currentValues[key] || '';
            
            if (originalValue !== currentValue) {
                changes.push({
                    field: key,
                    original: originalValue,
                    current: currentValue
                });
            }
        });
        
        console.log('[AdvancedChangeDetector] 📊 Changes detected:', changes.length);
        return changes.length > 0;
    };

    // ========================================
    // GET CHANGES - دریافت تغییرات
    // ========================================
    AdvancedChangeDetector.prototype.getChanges = function() {
        var changes = [];
        var self = this;
        
        Object.keys(this.currentValues).forEach(function(key) {
            var originalValue = self.originalValues[key] || '';
            var currentValue = self.currentValues[key] || '';
            
            if (originalValue !== currentValue) {
                changes.push({
                    field: key,
                    original: originalValue,
                    current: currentValue
                });
            }
        });
        
        return changes;
    };

    // ========================================
    // OBSERVER PATTERN - الگوی ناظر
    // ========================================
    AdvancedChangeDetector.prototype.observeChanges = function(callback) {
        if (typeof callback === 'function') {
            this.changeObservers.push(callback);
        }
    };

    AdvancedChangeDetector.prototype.unobserveChanges = function(callback) {
        var index = this.changeObservers.indexOf(callback);
        if (index > -1) {
            this.changeObservers.splice(index, 1);
        }
    };

    // ========================================
    // NOTIFY CHANGE OBSERVERS - اطلاع‌رسانی به ناظران
    // ========================================
    AdvancedChangeDetector.prototype.notifyChangeObservers = function(hasChanges, fieldKey, newValue) {
        var self = this;
        this.changeObservers.forEach(function(callback) {
            try {
                callback({
                    hasChanges: hasChanges,
                    fieldKey: fieldKey,
                    newValue: newValue,
                    changes: self.getChanges(),
                    timestamp: Date.now()
                });
            } catch (error) {
                console.error('[AdvancedChangeDetector] ❌ Observer callback error:', error);
            }
        });
    };

    // ========================================
    // RESET CHANGES - بازنشانی تغییرات
    // ========================================
    AdvancedChangeDetector.prototype.resetChanges = function() {
        console.log('[AdvancedChangeDetector] 🔄 Resetting changes...');
        
        try {
            // Update original values to current values
            var self = this;
            Object.keys(this.currentValues).forEach(function(key) {
                self.originalValues[key] = self.currentValues[key];
            });
            
            console.log('[AdvancedChangeDetector] ✅ Changes reset successfully');
            
        } catch (error) {
            console.error('[AdvancedChangeDetector] ❌ Error resetting changes:', error);
        }
    };

    // ========================================
    // GET CURRENT VALUES - دریافت مقادیر فعلی
    // ========================================
    AdvancedChangeDetector.prototype.getCurrentValues = function() {
        return this.currentValues;
    };

    // ========================================
    // GET ORIGINAL VALUES - دریافت مقادیر اولیه
    // ========================================
    AdvancedChangeDetector.prototype.getOriginalValues = function() {
        return this.originalValues;
    };

    // ========================================
    // VALIDATE CHANGES - اعتبارسنجی تغییرات
    // ========================================
    AdvancedChangeDetector.prototype.validateChanges = function() {
        var changes = this.getChanges();
        var validationErrors = [];
        var self = this;
        
        changes.forEach(function(change) {
            // Validate required fields
            if (change.field === 'primaryProvider' && !change.current) {
                validationErrors.push('سازمان بیمه پایه الزامی است');
            }
            
            if (change.field === 'primaryPlan' && !change.current && self.currentValues.primaryProvider) {
                validationErrors.push('طرح بیمه پایه الزامی است');
            }
            
            if (change.field === 'supplementaryPlan' && !change.current && self.currentValues.supplementaryProvider) {
                validationErrors.push('طرح بیمه تکمیلی الزامی است');
            }
        });
        
        return {
            isValid: validationErrors.length === 0,
            errors: validationErrors,
            changes: changes
        };
    };

    // ========================================
    // HANDLE ERROR - مدیریت خطا
    // ========================================
    AdvancedChangeDetector.prototype.handleError = function(error) {
        console.error('[AdvancedChangeDetector] 🚨 Error:', error);
        
        // Reset state
        this.isCapturing = false;
        
        // Notify observers of error
        var self = this;
        this.changeObservers.forEach(function(callback) {
            try {
                callback({
                    hasChanges: false,
                    error: error.message || 'Unknown error',
                    timestamp: Date.now()
                });
            } catch (callbackError) {
                console.error('[AdvancedChangeDetector] ❌ Error in error callback:', callbackError);
            }
        });
    };

    // ========================================
    // CLEANUP - پاکسازی
    // ========================================
    AdvancedChangeDetector.prototype.cleanup = function() {
        console.log('[AdvancedChangeDetector] 🧹 Cleaning up...');
        
        try {
            // Clear timers
            var self = this;
            Object.keys(this.debounceTimers).forEach(function(timerId) {
                clearTimeout(self.debounceTimers[timerId]);
            });
            this.debounceTimers = {};
            
            // Abort ongoing operations
            if (this.abortController) {
                this.abortController.abort();
            }
            
            // Remove event listeners
            $(document).off('.advancedChangeDetector');
            
            // Clear objects
            this.originalValues = {};
            this.currentValues = {};
            this.changeObservers = [];
            
            console.log('[AdvancedChangeDetector] ✅ Cleanup completed');
            
        } catch (error) {
            console.error('[AdvancedChangeDetector] ❌ Error during cleanup:', error);
        }
    };

    // ========================================
    // GET STATUS - دریافت وضعیت
    // ========================================
    AdvancedChangeDetector.prototype.getStatus = function() {
        return {
            isInitialized: this.isInitialized,
            isCapturing: this.isCapturing,
            originalValuesCount: Object.keys(this.originalValues).length,
            currentValuesCount: Object.keys(this.currentValues).length,
            observersCount: this.changeObservers.length,
            hasChanges: this.detectChanges()
        };
    };

    // ========================================
    // GLOBAL INSTANCE - نمونه سراسری
    // ========================================
    var detectorInstance = null;

    // Initialize when DOM is ready
    $(document).ready(function() {
        try {
            detectorInstance = new AdvancedChangeDetector();
            window.AdvancedChangeDetector = detectorInstance;
            console.log('[AdvancedChangeDetector] 🌟 Global instance created');
        } catch (error) {
            console.error('[AdvancedChangeDetector] ❌ Failed to create global instance:', error);
        }
    });

    // Cleanup on page unload
    $(window).on('beforeunload', function() {
        if (detectorInstance) {
            detectorInstance.cleanup();
        }
    });

})(window, jQuery);