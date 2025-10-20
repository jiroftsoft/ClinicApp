/**
 * ValidationEngine Module - موتور اعتبارسنجی
 * Single Responsibility: فقط اعتبارسنجی فرم
 * 
 * @author ClinicApp Team
 * @version 1.0.0
 * @description ماژول تخصصی برای اعتبارسنجی فرم بیمه
 */

(function() {
    'use strict';

    // ========================================
    // VALIDATION ENGINE MODULE
    // ========================================
    window.ValidationEngine = {
        
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
            },
            rules: {
                required: {
                    primaryProvider: true,
                    primaryPlan: true,
                    primaryPolicyNumber: false,
                    primaryCardNumber: false,
                    supplementaryProvider: false,
                    supplementaryPlan: false,
                    supplementaryPolicyNumber: false,
                    supplementaryExpiry: false
                },
                minLength: {
                    primaryPolicyNumber: 3,
                    primaryCardNumber: 3,
                    supplementaryPolicyNumber: 3
                },
                maxLength: {
                    primaryPolicyNumber: 50,
                    primaryCardNumber: 50,
                    supplementaryPolicyNumber: 50
                }
            },
            messages: {
                required: 'این فیلد الزامی است',
                minLength: 'حداقل {0} کاراکتر وارد کنید',
                maxLength: 'حداکثر {0} کاراکتر وارد کنید',
                invalidFormat: 'فرمت وارد شده صحیح نیست'
            }
        },

        // State
        validationResults: null,
        isInitialized: false,

        // ========================================
        // INITIALIZATION - مقداردهی اولیه
        // ========================================
        init: function() {
            console.log('[ValidationEngine] Initializing...');
            
            try {
                this.isInitialized = true;
                console.log('[ValidationEngine] ✅ Initialized successfully');
            } catch (error) {
                console.error('[ValidationEngine] ❌ Initialization failed:', error);
                throw error;
            }
        },

        // ========================================
        // VALIDATE FORM - اعتبارسنجی فرم
        // ========================================
        validateForm: function() {
            console.log('[ValidationEngine] Validating form...');
            
            try {
                var errors = [];
                var values = this.getFormValues();
                
                // اعتبارسنجی فیلدهای الزامی
                errors = errors.concat(this.validateRequiredFields(values));
                
                // اعتبارسنجی طول فیلدها
                errors = errors.concat(this.validateFieldLengths(values));
                
                // اعتبارسنجی فرمت فیلدها
                errors = errors.concat(this.validateFieldFormats(values));
                
                var result = {
                    isValid: errors.length === 0,
                    errors: errors,
                    values: values
                };
                
                this.validationResults = result;
                console.log('[ValidationEngine] Validation completed:', result);
                
                return result;
            } catch (error) {
                console.error('[ValidationEngine] Error validating form:', error);
                throw error;
            }
        },

        // ========================================
        // GET FORM VALUES - دریافت مقادیر فرم
        // ========================================
        getFormValues: function() {
            console.log('[ValidationEngine] Getting form values...');
            
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
                
                console.log('[ValidationEngine] Form values retrieved:', values);
                return values;
            } catch (error) {
                console.error('[ValidationEngine] Error getting form values:', error);
                throw error;
            }
        },

        // ========================================
        // VALIDATE REQUIRED FIELDS - اعتبارسنجی فیلدهای الزامی
        // ========================================
        validateRequiredFields: function(values) {
            console.log('[ValidationEngine] Validating required fields...');
            
            try {
                var errors = [];
                
                for (var field in this.config.rules.required) {
                    if (this.config.rules.required[field] && (!values[field] || values[field].trim() === '')) {
                        errors.push({
                            field: field,
                            message: this.config.messages.required,
                            type: 'required'
                        });
                    }
                }
                
                console.log('[ValidationEngine] Required field validation completed:', errors);
                return errors;
            } catch (error) {
                console.error('[ValidationEngine] Error validating required fields:', error);
                throw error;
            }
        },

        // ========================================
        // VALIDATE FIELD LENGTHS - اعتبارسنجی طول فیلدها
        // ========================================
        validateFieldLengths: function(values) {
            console.log('[ValidationEngine] Validating field lengths...');
            
            try {
                var errors = [];
                
                // اعتبارسنجی حداقل طول
                for (var field in this.config.rules.minLength) {
                    if (values[field] && values[field].length < this.config.rules.minLength[field]) {
                        errors.push({
                            field: field,
                            message: this.config.messages.minLength.replace('{0}', this.config.rules.minLength[field]),
                            type: 'minLength'
                        });
                    }
                }
                
                // اعتبارسنجی حداکثر طول
                for (var field in this.config.rules.maxLength) {
                    if (values[field] && values[field].length > this.config.rules.maxLength[field]) {
                        errors.push({
                            field: field,
                            message: this.config.messages.maxLength.replace('{0}', this.config.rules.maxLength[field]),
                            type: 'maxLength'
                        });
                    }
                }
                
                console.log('[ValidationEngine] Field length validation completed:', errors);
                return errors;
            } catch (error) {
                console.error('[ValidationEngine] Error validating field lengths:', error);
                throw error;
            }
        },

        // ========================================
        // VALIDATE FIELD FORMATS - اعتبارسنجی فرمت فیلدها
        // ========================================
        validateFieldFormats: function(values) {
            console.log('[ValidationEngine] Validating field formats...');
            
            try {
                var errors = [];
                
                // اعتبارسنجی فرمت شماره بیمه
                if (values.primaryPolicyNumber && !this.isValidPolicyNumber(values.primaryPolicyNumber)) {
                    errors.push({
                        field: 'primaryPolicyNumber',
                        message: this.config.messages.invalidFormat,
                        type: 'format'
                    });
                }
                
                // اعتبارسنجی فرمت شماره کارت
                if (values.primaryCardNumber && !this.isValidCardNumber(values.primaryCardNumber)) {
                    errors.push({
                        field: 'primaryCardNumber',
                        message: this.config.messages.invalidFormat,
                        type: 'format'
                    });
                }
                
                console.log('[ValidationEngine] Field format validation completed:', errors);
                return errors;
            } catch (error) {
                console.error('[ValidationEngine] Error validating field formats:', error);
                throw error;
            }
        },

        // ========================================
        // IS VALID POLICY NUMBER - بررسی صحت شماره بیمه
        // ========================================
        isValidPolicyNumber: function(policyNumber) {
            // شماره بیمه باید فقط شامل اعداد و حروف باشد
            return /^[a-zA-Z0-9]+$/.test(policyNumber);
        },

        // ========================================
        // IS VALID CARD NUMBER - بررسی صحت شماره کارت
        // ========================================
        isValidCardNumber: function(cardNumber) {
            // شماره کارت باید فقط شامل اعداد باشد
            return /^[0-9]+$/.test(cardNumber);
        },

        // ========================================
        // GET VALIDATION RESULTS - دریافت نتایج اعتبارسنجی
        // ========================================
        getValidationResults: function() {
            return this.validationResults;
        },

        // ========================================
        // RESET - بازنشانی
        // ========================================
        reset: function() {
            console.log('[ValidationEngine] Resetting...');
            
            try {
                this.validationResults = null;
                console.log('[ValidationEngine] Reset completed');
            } catch (error) {
                console.error('[ValidationEngine] Error resetting:', error);
                throw error;
            }
        }
    };

    // Auto-initialize when DOM is ready
    $(document).ready(function() {
        try {
            window.ValidationEngine.init();
        } catch (error) {
            console.error('[ValidationEngine] Auto-initialization failed:', error);
        }
    });

})();
