/**
 * FormChangeDetector Module - تشخیص تغییرات فرم
 * Single Responsibility: فقط تشخیص تغییرات فرم
 * 
 * @author ClinicApp Team
 * @version 1.0.0
 * @description ماژول تخصصی برای تشخیص تغییرات فرم بیمه
 */

(function() {
    'use strict';

    // ========================================
    // FORM CHANGE DETECTOR MODULE
    // ========================================
    window.FormChangeDetector = {
        
        // Configuration
        config: {
            selectors: {
                primaryProvider: '#insuranceProvider',
                primaryPlan: '#insurancePlan',
                primaryPolicyNumber: '#policyNumber',
                primaryCardNumber: '#cardNumber',
                supplementaryProvider: '#supplementaryProvider',
                supplementaryPlan: '#supplementaryPlan',
                supplementaryPolicyNumber: '#supplementaryPolicyNumber',
                supplementaryExpiry: '#supplementaryExpiry'
            }
        },

        // State
        originalValues: null,
        currentValues: null,

        // ========================================
        // INITIALIZATION - مقداردهی اولیه
        // ========================================
        init: function() {
            console.log('[FormChangeDetector] Initializing...');
            
            try {
                this.captureInitialValues();
                console.log('[FormChangeDetector] ✅ Initialized successfully');
            } catch (error) {
                console.error('[FormChangeDetector] ❌ Initialization failed:', error);
                throw error;
            }
        },

        // ========================================
        // CAPTURE INITIAL VALUES - ثبت مقادیر اولیه (Production-Optimized)
        // ========================================
        captureInitialValues: function() {
            console.log('[FormChangeDetector] 📝 Capturing initial form values...');
            
            try {
                // Wait for DOM elements to be available
                var self = this;
                setTimeout(function() {
                    self.originalValues = self.captureFormValues();
                    console.log('[FormChangeDetector] ✅ Initial values captured:', self.originalValues);
                    
                    // Emit event for other modules
                    if (window.ReceptionEventBus) {
                        window.ReceptionEventBus.emit('form:initialValuesCaptured', self.originalValues);
                    }
                }, 100);
                
            } catch (error) {
                console.error('[FormChangeDetector] ❌ Error capturing initial values:', error);
                throw error;
            }
        },

        // ========================================
        // SET ORIGINAL VALUES - تنظیم مقادیر اولیه
        // ========================================
        setOriginalValues: function(values) {
            console.log('[FormChangeDetector] 📝 Setting original values...');
            
            try {
                this.originalValues = values;
                console.log('[FormChangeDetector] ✅ Original values set:', this.originalValues);
                
                // Emit event for other modules
                if (window.ReceptionEventBus) {
                    window.ReceptionEventBus.emit('form:originalValuesSet', this.originalValues);
                }
                
            } catch (error) {
                console.error('[FormChangeDetector] ❌ Error setting original values:', error);
                throw error;
            }
        },

        // ========================================
        // UPDATE ORIGINAL VALUES FROM CURRENT FORM - به‌روزرسانی مقادیر اولیه از فرم فعلی
        // ========================================
        updateOriginalValuesFromCurrentForm: function() {
            console.log('[FormChangeDetector] 📝 Updating original values from current form...');
            
            try {
                this.originalValues = this.captureFormValues();
                console.log('[FormChangeDetector] ✅ Original values updated from current form:', this.originalValues);
                
                // Emit event for other modules
                if (window.ReceptionEventBus) {
                    window.ReceptionEventBus.emit('form:originalValuesUpdated', this.originalValues);
                }
                
            } catch (error) {
                console.error('[FormChangeDetector] ❌ Error updating original values from current form:', error);
                throw error;
            }
        },

        // ========================================
        // CAPTURE FORM VALUES - ثبت مقادیر فرم
        // ========================================
        captureFormValues: function() {
            console.log('[FormChangeDetector] Capturing form values...');
            
            try {
                var values = {
                    primaryProvider: $(this.config.selectors.primaryProvider).val() || '',
                    primaryPlan: $(this.config.selectors.primaryPlan).val() || '',
                    primaryPolicyNumber: $(this.config.selectors.primaryPolicyNumber).val() || '',
                    primaryCardNumber: $(this.config.selectors.primaryCardNumber).val() || '',
                    supplementaryProvider: $(this.config.selectors.supplementaryProvider).val() || '',
                    supplementaryPlan: $(this.config.selectors.supplementaryPlan).val() || '',
                    supplementaryPolicyNumber: $(this.config.selectors.supplementaryPolicyNumber).val() || '',
                    supplementaryExpiry: $(this.config.selectors.supplementaryExpiry).val() || ''
                };
                
                console.log('[FormChangeDetector] Form values captured:', values);
                return values;
            } catch (error) {
                console.error('[FormChangeDetector] Error capturing form values:', error);
                throw error;
            }
        },

        // ========================================
        // DETECT CHANGES - تشخیص تغییرات (Production-Optimized)
        // ========================================
        detectChanges: function() {
            console.log('[FormChangeDetector] 🔍 Detecting form changes...');
            
            try {
                if (!this.originalValues) {
                    console.warn('[FormChangeDetector] ⚠️ Original values not set, capturing current values');
                    this.originalValues = this.captureFormValues();
                    return {
                        hasChanges: false,
                        changes: [],
                        originalValues: this.originalValues,
                        currentValues: this.originalValues
                    };
                }
                
                this.currentValues = this.captureFormValues();
                var changes = this.compareValues(this.originalValues, this.currentValues);
                
                console.log('[FormChangeDetector] 📊 Changes detected:', changes);
                return {
                    hasChanges: changes.length > 0,
                    changes: changes,
                    originalValues: this.originalValues,
                    currentValues: this.currentValues
                };
            } catch (error) {
                console.error('[FormChangeDetector] ❌ Error detecting changes:', error);
                throw error;
            }
        },

        // ========================================
        // HAS FORM CHANGED - بررسی تغییرات فرم (Production-Optimized)
        // ========================================
        hasFormChanged: function() {
            console.log('[FormChangeDetector] 🔍 Checking if form has changed...');
            
            try {
                if (!this.originalValues) {
                    console.warn('[FormChangeDetector] ⚠️ No original values set');
                    return false;
                }
                
                var currentValues = this.captureFormValues();
                var hasChanged = false;
                var changes = [];
                
                // Compare each field
                Object.keys(this.originalValues).forEach(function(key) {
                    var original = (this.originalValues[key] || '').toString().trim();
                    var current = (currentValues[key] || '').toString().trim();
                    
                    if (original !== current) {
                        hasChanged = true;
                        changes.push({
                            field: key,
                            original: original,
                            current: current
                        });
                    }
                }.bind(this));
                
                console.log('[FormChangeDetector] 📊 Form change check:', {
                    hasChanged: hasChanged,
                    changesCount: changes.length,
                    changes: changes
                });
                
                return hasChanged;
                
            } catch (error) {
                console.error('[FormChangeDetector] ❌ Error checking form changes:', error);
                return false;
            }
        },

        // ========================================
        // SAFE COMPARE - مقایسه امن مقادیر
        // ========================================
        safeCompare: function(original, current) {
            // Convert to strings and trim
            var origStr = (original || '').toString().trim();
            var currStr = (current || '').toString().trim();
            
            return origStr === currStr;
        },

        // ========================================
        // SAFE GET VALUE - دریافت امن مقدار
        // ========================================
        safeGetValue: function(selector) {
            try {
                var element = $(selector);
                if (element.length === 0) {
                    return '';
                }
                
                var value = element.val();
                return value ? value.toString().trim() : '';
            } catch (error) {
                console.warn('[FormChangeDetector] Error getting value for selector:', selector, error);
                return '';
            }
        },

        // ========================================
        // COMPARE VALUES - مقایسه مقادیر (Production-Optimized)
        // ========================================
        compareValues: function(original, current) {
            console.log('[FormChangeDetector] 🔍 Comparing values...');
            
            try {
                var changes = [];
                
                // بررسی تغییرات بیمه پایه
                if (!this.safeCompare(original.primaryProvider, current.primaryProvider)) {
                    changes.push({
                        field: 'primaryProvider',
                        original: original.primaryProvider,
                        current: current.primaryProvider,
                        type: 'Primary Provider'
                    });
                }
                if (!this.safeCompare(original.primaryPlan, current.primaryPlan)) {
                    changes.push({
                        field: 'primaryPlan',
                        original: original.primaryPlan,
                        current: current.primaryPlan,
                        type: 'Primary Plan'
                    });
                }
                if (!this.safeCompare(original.primaryPolicyNumber, current.primaryPolicyNumber)) {
                    changes.push({
                        field: 'primaryPolicyNumber',
                        original: original.primaryPolicyNumber,
                        current: current.primaryPolicyNumber,
                        type: 'Primary Policy Number'
                    });
                }
                if (!this.safeCompare(original.primaryCardNumber, current.primaryCardNumber)) {
                    changes.push({
                        field: 'primaryCardNumber',
                        original: original.primaryCardNumber,
                        current: current.primaryCardNumber,
                        type: 'Primary Card Number'
                    });
                }
                
                // بررسی تغییرات بیمه تکمیلی
                if (!this.safeCompare(original.supplementaryProvider, current.supplementaryProvider)) {
                    changes.push({
                        field: 'supplementaryProvider',
                        original: original.supplementaryProvider,
                        current: current.supplementaryProvider,
                        type: 'Supplementary Provider'
                    });
                }
                if (!this.safeCompare(original.supplementaryPlan, current.supplementaryPlan)) {
                    changes.push({
                        field: 'supplementaryPlan',
                        original: original.supplementaryPlan,
                        current: current.supplementaryPlan,
                        type: 'Supplementary Plan'
                    });
                }
                if (!this.safeCompare(original.supplementaryPolicyNumber, current.supplementaryPolicyNumber)) {
                    changes.push({
                        field: 'supplementaryPolicyNumber',
                        original: original.supplementaryPolicyNumber,
                        current: current.supplementaryPolicyNumber,
                        type: 'Supplementary Policy Number'
                    });
                }
                if (!this.safeCompare(original.supplementaryExpiry, current.supplementaryExpiry)) {
                    changes.push({
                        field: 'supplementaryExpiry',
                        original: original.supplementaryExpiry,
                        current: current.supplementaryExpiry,
                        type: 'Supplementary Expiry'
                    });
                }
                
                console.log('[FormChangeDetector] 📊 Comparison result:', changes);
                return changes;
            } catch (error) {
                console.error('[FormChangeDetector] ❌ Error comparing values:', error);
                throw error;
            }
        },

        // ========================================
        // UPDATE ORIGINAL VALUES - به‌روزرسانی مقادیر اولیه
        // ========================================
        updateOriginalValues: function() {
            console.log('[FormChangeDetector] Updating original values...');
            
            try {
                this.originalValues = this.captureFormValues();
                console.log('[FormChangeDetector] Original values updated:', this.originalValues);
            } catch (error) {
                console.error('[FormChangeDetector] Error updating original values:', error);
                throw error;
            }
        },

        // ========================================
        // RESET - بازنشانی
        // ========================================
        reset: function() {
            console.log('[FormChangeDetector] Resetting...');
            
            try {
                this.originalValues = null;
                this.currentValues = null;
                console.log('[FormChangeDetector] Reset completed');
            } catch (error) {
                console.error('[FormChangeDetector] Error resetting:', error);
                throw error;
            }
        }
    };

    // Auto-initialize when DOM is ready
    $(document).ready(function() {
        try {
            window.FormChangeDetector.init();
        } catch (error) {
            console.error('[FormChangeDetector] Auto-initialization failed:', error);
        }
    });

})();
