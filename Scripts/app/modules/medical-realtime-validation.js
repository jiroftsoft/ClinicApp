/**
 * Medical Real-time Validation Module
 * ماژول اعتبارسنجی لحظه‌ای محیط درمانی
 * 
 * @author ClinicApp Medical Team
 * @version 1.0.0
 * @description Real-time validation for medical environment forms
 */

(function() {
    'use strict';
    
    // Global MedicalRealTimeValidation object
    window.MedicalRealTimeValidation = {
        
        // Configuration
        config: {
            debounceDelay: 300, // 300ms delay for input validation
            validationTimeout: 5000, // 5 seconds timeout for server validation
            errorClass: 'is-invalid',
            successClass: 'is-valid',
            warningClass: 'is-warning'
        },
        
        // Validation rules cache
        validationRules: {},
        
        // Debounce timers
        debounceTimers: {},
        
        /**
         * Initialize Real-time Validation
         * راه‌اندازی اعتبارسنجی لحظه‌ای
         */
        init: function() {
            console.log(window.MedicalConfig.logPrefix + ' MedicalRealTimeValidation initialized');
            this.setupEventListeners();
            this.loadValidationRules();
            return true;
        },
        
        /**
         * Setup event listeners for real-time validation
         * تنظیم event listener ها برای اعتبارسنجی لحظه‌ای
         */
        setupEventListeners: function() {
            const self = this;
            
            // Input field validation
            $(document).on('input blur', '.medical-form input, .medical-form select, .medical-form textarea', function() {
                const fieldName = $(this).attr('name') || $(this).attr('id');
                if (fieldName) {
                    self.validateField($(this), fieldName);
                }
            });
            
            // Form submission validation
            $(document).on('submit', '.medical-form', function(e) {
                if (!self.validateForm($(this))) {
                    e.preventDefault();
                    return false;
                }
            });
            
            // Real-time calculation updates
            $(document).on('input', '#createTariffPrice, #createPatientShare, #createInsurerShare, #createCoveragePercent', function() {
                self.updateCalculations();
            });
        },
        
        /**
         * Load validation rules from server or configuration
         * بارگذاری قوانین اعتبارسنجی از سرور یا تنظیمات
         */
        loadValidationRules: function() {
            this.validationRules = {
                // Tariff Price validation
                'TariffPrice': {
                    required: true,
                    min: 0,
                    max: 999999999,
                    pattern: /^\d+(\.\d{1,2})?$/,
                    message: 'قیمت تعرفه باید عدد مثبت باشد (حداکثر 2 رقم اعشار)'
                },
                
                // Patient Share validation
                'PatientShare': {
                    required: true,
                    min: 0,
                    max: 999999999,
                    pattern: /^\d+(\.\d{1,2})?$/,
                    message: 'سهم بیمار باید عدد مثبت باشد (حداکثر 2 رقم اعشار)'
                },
                
                // Insurer Share validation
                'InsurerShare': {
                    required: true,
                    min: 0,
                    max: 999999999,
                    pattern: /^\d+(\.\d{1,2})?$/,
                    message: 'سهم بیمه باید عدد مثبت باشد (حداکثر 2 رقم اعشار)'
                },
                
                // Coverage Percent validation
                'SupplementaryCoveragePercent': {
                    required: true,
                    min: 0,
                    max: 100,
                    pattern: /^\d+(\.\d{1,2})?$/,
                    message: 'درصد پوشش باید بین 0 تا 100 باشد'
                },
                
                // Priority validation
                'Priority': {
                    required: false,
                    min: 1,
                    max: 10,
                    pattern: /^\d+$/,
                    message: 'اولویت باید عددی بین 1 تا 10 باشد'
                },
                
                // Service selection validation
                'ServiceId': {
                    required: true,
                    message: 'لطفاً خدمت را انتخاب کنید'
                },
                
                // Insurance Plan validation
                'InsurancePlanId': {
                    required: true,
                    message: 'لطفاً طرح بیمه را انتخاب کنید'
                }
            };
        },
        
        /**
         * Validate individual field
         * اعتبارسنجی فیلد منفرد
         */
        validateField: function($field, fieldName) {
            const self = this;
            const fieldValue = $field.val();
            const rules = this.validationRules[fieldName];
            
            if (!rules) {
                return true; // No rules defined, consider valid
            }
            
            // Clear previous validation state
            this.clearFieldValidation($field);
            
            // Debounce validation for better performance
            if (this.debounceTimers[fieldName]) {
                clearTimeout(this.debounceTimers[fieldName]);
            }
            
            this.debounceTimers[fieldName] = setTimeout(function() {
                const validationResult = self.performFieldValidation(fieldValue, rules, fieldName);
                self.applyFieldValidation($field, validationResult);
                
                // Update calculations if this is a calculation field
                if (['TariffPrice', 'PatientShare', 'InsurerShare', 'SupplementaryCoveragePercent'].includes(fieldName)) {
                    self.updateCalculations();
                }
            }, this.config.debounceDelay);
            
            return true;
        },
        
        /**
         * Perform field validation based on rules
         * انجام اعتبارسنجی فیلد بر اساس قوانین
         */
        performFieldValidation: function(value, rules, fieldName) {
            const result = {
                isValid: true,
                message: '',
                level: 'success'
            };
            
            // Required field validation
            if (rules.required && (!value || value.trim() === '')) {
                result.isValid = false;
                result.message = rules.message || 'این فیلد الزامی است';
                result.level = 'error';
                return result;
            }
            
            // Skip other validations if field is empty and not required
            if (!value || value.trim() === '') {
                return result;
            }
            
            // Pattern validation
            if (rules.pattern && !rules.pattern.test(value)) {
                result.isValid = false;
                result.message = rules.message || 'فرمت ورودی نامعتبر است';
                result.level = 'error';
                return result;
            }
            
            // Numeric range validation
            if (rules.min !== undefined || rules.max !== undefined) {
                const numericValue = parseFloat(value);
                if (isNaN(numericValue)) {
                    result.isValid = false;
                    result.message = 'مقدار باید عدد باشد';
                    result.level = 'error';
                    return result;
                }
                
                if (rules.min !== undefined && numericValue < rules.min) {
                    result.isValid = false;
                    result.message = `مقدار باید حداقل ${rules.min} باشد`;
                    result.level = 'error';
                    return result;
                }
                
                if (rules.max !== undefined && numericValue > rules.max) {
                    result.isValid = false;
                    result.message = `مقدار باید حداکثر ${rules.max} باشد`;
                    result.level = 'error';
                    return result;
                }
            }
            
            // Business logic validation
            if (fieldName === 'PatientShare' || fieldName === 'InsurerShare') {
                const tariffPrice = parseFloat($('#createTariffPrice').val() || 0);
                const patientShare = parseFloat($('#createPatientShare').val() || 0);
                const insurerShare = parseFloat($('#createInsurerShare').val() || 0);
                
                if (tariffPrice > 0 && (patientShare + insurerShare) > tariffPrice) {
                    result.isValid = false;
                    result.message = 'مجموع سهم بیمار و بیمه نمی‌تواند بیشتر از قیمت تعرفه باشد';
                    result.level = 'error';
                    return result;
                }
            }
            
            return result;
        },
        
        /**
         * Apply validation result to field
         * اعمال نتیجه اعتبارسنجی به فیلد
         */
        applyFieldValidation: function($field, validationResult) {
            const $formGroup = $field.closest('.form-group, .mb-3');
            const $feedback = $formGroup.find('.invalid-feedback, .valid-feedback');
            
            // Remove existing validation classes
            $field.removeClass(this.config.errorClass + ' ' + this.config.successClass + ' ' + this.config.warningClass);
            
            if (validationResult.isValid) {
                $field.addClass(this.config.successClass);
                if ($feedback.length) {
                    $feedback.removeClass('invalid-feedback').addClass('valid-feedback').text('✓ معتبر');
                }
            } else {
                $field.addClass(this.config.errorClass);
                if ($feedback.length) {
                    $feedback.removeClass('valid-feedback').addClass('invalid-feedback').text(validationResult.message);
                } else {
                    // Create feedback element if it doesn't exist
                    $formGroup.append(`<div class="invalid-feedback">${validationResult.message}</div>`);
                }
            }
        },
        
        /**
         * Clear field validation state
         * پاک کردن وضعیت اعتبارسنجی فیلد
         */
        clearFieldValidation: function($field) {
            const $formGroup = $field.closest('.form-group, .mb-3');
            $field.removeClass(this.config.errorClass + ' ' + this.config.successClass + ' ' + this.config.warningClass);
            $formGroup.find('.invalid-feedback, .valid-feedback').remove();
        },
        
        /**
         * Validate entire form
         * اعتبارسنجی کل فرم
         */
        validateForm: function($form) {
            let isFormValid = true;
            const self = this;
            
            // Validate all fields in the form
            $form.find('input, select, textarea').each(function() {
                const fieldName = $(this).attr('name') || $(this).attr('id');
                if (fieldName && self.validationRules[fieldName]) {
                    const validationResult = self.performFieldValidation($(this).val(), self.validationRules[fieldName], fieldName);
                    self.applyFieldValidation($(this), validationResult);
                    
                    if (!validationResult.isValid) {
                        isFormValid = false;
                    }
                }
            });
            
            // Additional form-level validations
            if (isFormValid) {
                isFormValid = this.validateFormBusinessRules($form);
            }
            
            return isFormValid;
        },
        
        /**
         * Validate form business rules
         * اعتبارسنجی قوانین کسب‌وکار فرم
         */
        validateFormBusinessRules: function($form) {
            const tariffPrice = parseFloat($form.find('#createTariffPrice').val() || 0);
            const patientShare = parseFloat($form.find('#createPatientShare').val() || 0);
            const insurerShare = parseFloat($form.find('#createInsurerShare').val() || 0);
            
            // Check if sum of shares equals tariff price
            if (tariffPrice > 0 && Math.abs((patientShare + insurerShare) - tariffPrice) > 0.01) {
                this.showFormError('مجموع سهم بیمار و بیمه باید برابر با قیمت تعرفه باشد');
                return false;
            }
            
            return true;
        },
        
        /**
         * Update calculations in real-time
         * به‌روزرسانی محاسبات به صورت لحظه‌ای
         */
        updateCalculations: function() {
            try {
                const tariffPrice = parseFloat($('#createTariffPrice').val() || 0);
                const patientShare = parseFloat($('#createPatientShare').val() || 0);
                const insurerShare = parseFloat($('#createInsurerShare').val() || 0);
                const coveragePercent = parseFloat($('#createCoveragePercent').val() || 0);
                
                if (tariffPrice > 0) {
                    // Update calculation preview
                    const remainingAmount = tariffPrice - insurerShare;
                    const finalPatientShare = Math.max(0, remainingAmount - (remainingAmount * coveragePercent / 100));
                    
                    // Update preview elements
                    $('.calculation-preview .total-service-price').text(this.formatCurrency(tariffPrice));
                    $('.calculation-preview .primary-insurance-share').text(this.formatCurrency(insurerShare));
                    $('.calculation-preview .remaining-amount').text(this.formatCurrency(remainingAmount));
                    $('.calculation-preview .supplementary-coverage').text(coveragePercent + '%');
                    $('.calculation-preview .final-patient-share').text(this.formatCurrency(finalPatientShare));
                    
                    // Validate calculation consistency
                    this.validateCalculationConsistency(tariffPrice, patientShare, insurerShare, coveragePercent);
                }
            } catch (error) {
                console.error('Error updating calculations:', error);
            }
        },
        
        /**
         * Validate calculation consistency
         * اعتبارسنجی سازگاری محاسبات
         */
        validateCalculationConsistency: function(tariffPrice, patientShare, insurerShare, coveragePercent) {
            const calculatedPatientShare = tariffPrice - insurerShare;
            const difference = Math.abs(patientShare - calculatedPatientShare);
            
            if (difference > 0.01) {
                this.showCalculationWarning('محاسبات با مقادیر وارد شده سازگار نیستند');
            } else {
                this.hideCalculationWarning();
            }
        },
        
        /**
         * Show form error message
         * نمایش پیام خطای فرم
         */
        showFormError: function(message) {
            const $errorContainer = $('.form-error-container');
            if ($errorContainer.length === 0) {
                $('.medical-form').prepend('<div class="form-error-container alert alert-danger" style="display: none;"></div>');
            }
            
            $('.form-error-container')
                .html(`<i class="fas fa-exclamation-triangle"></i> ${message}`)
                .slideDown();
        },
        
        /**
         * Show calculation warning
         * نمایش هشدار محاسبات
         */
        showCalculationWarning: function(message) {
            $('.calculation-warning')
                .html(`<i class="fas fa-exclamation-triangle"></i> ${message}`)
                .show();
        },
        
        /**
         * Hide calculation warning
         * مخفی کردن هشدار محاسبات
         */
        hideCalculationWarning: function() {
            $('.calculation-warning').hide();
        },
        
        /**
         * Format currency for display
         * فرمت کردن ارز برای نمایش
         */
        formatCurrency: function(amount) {
            return new Intl.NumberFormat('fa-IR').format(amount) + ' تومان';
        },
        
        /**
         * Reset form validation
         * بازنشانی اعتبارسنجی فرم
         */
        resetFormValidation: function($form) {
            const self = this;
            $form.find('input, select, textarea').each(function() {
                self.clearFieldValidation($(this));
            });
            $('.form-error-container, .calculation-warning').hide();
        }
    };
    
    // Auto-initialize when DOM is ready
    $(document).ready(function() {
        if (window.MedicalConfig && window.MedicalConfig.enableRealTimeValidation !== false) {
            window.MedicalRealTimeValidation.init();
        }
    });
    
})();
