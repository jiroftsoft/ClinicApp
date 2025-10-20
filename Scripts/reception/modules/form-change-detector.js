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
        // CAPTURE INITIAL VALUES - ثبت مقادیر اولیه
        // ========================================
        captureInitialValues: function() {
            console.log('[FormChangeDetector] Capturing initial form values...');
            
            try {
                this.originalValues = this.captureFormValues();
                console.log('[FormChangeDetector] Initial values captured:', this.originalValues);
            } catch (error) {
                console.error('[FormChangeDetector] Error capturing initial values:', error);
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
        // DETECT CHANGES - تشخیص تغییرات
        // ========================================
        detectChanges: function() {
            console.log('[FormChangeDetector] Detecting form changes...');
            
            try {
                this.currentValues = this.captureFormValues();
                var changes = this.compareValues(this.originalValues, this.currentValues);
                
                console.log('[FormChangeDetector] Changes detected:', changes);
                return {
                    hasChanges: changes.length > 0,
                    changes: changes,
                    originalValues: this.originalValues,
                    currentValues: this.currentValues
                };
            } catch (error) {
                console.error('[FormChangeDetector] Error detecting changes:', error);
                throw error;
            }
        },

        // ========================================
        // COMPARE VALUES - مقایسه مقادیر
        // ========================================
        compareValues: function(original, current) {
            console.log('[FormChangeDetector] Comparing values...');
            
            try {
                var changes = [];
                
                // بررسی تغییرات بیمه پایه
                if (original.primaryProvider !== current.primaryProvider) {
                    changes.push('Primary Provider');
                }
                if (original.primaryPlan !== current.primaryPlan) {
                    changes.push('Primary Plan');
                }
                if (original.primaryPolicyNumber !== current.primaryPolicyNumber) {
                    changes.push('Primary Policy Number');
                }
                if (original.primaryCardNumber !== current.primaryCardNumber) {
                    changes.push('Primary Card Number');
                }
                
                // بررسی تغییرات بیمه تکمیلی
                if (original.supplementaryProvider !== current.supplementaryProvider) {
                    changes.push('Supplementary Provider');
                }
                if (original.supplementaryPlan !== current.supplementaryPlan) {
                    changes.push('Supplementary Plan');
                }
                if (original.supplementaryPolicyNumber !== current.supplementaryPolicyNumber) {
                    changes.push('Supplementary Policy Number');
                }
                if (original.supplementaryExpiry !== current.supplementaryExpiry) {
                    changes.push('Supplementary Expiry');
                }
                
                console.log('[FormChangeDetector] Comparison result:', changes);
                return changes;
            } catch (error) {
                console.error('[FormChangeDetector] Error comparing values:', error);
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
