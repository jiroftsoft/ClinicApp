/**
 * Medical Validation Module
 * ماژول اعتبارسنجی محیط درمانی
 * 
 * @author ClinicApp Medical Team
 * @version 1.0.0
 * @description Validation for medical environment
 */

(function() {
    'use strict';
    
    // Global MedicalValidation object
    window.MedicalValidation = {
    
    /**
     * Initialize MedicalValidation
     * راه‌اندازی MedicalValidation
     */
    init: function() {
        console.log(window.MedicalConfig.logPrefix + ' MedicalValidation initialized successfully');
        return true;
    },
    
    /**
     * Validate create tariff form
     * اعتبارسنجی فرم ایجاد تعرفه
     */
    validateCreateTariffForm: function() {
        console.log(window.MedicalConfig.logPrefix + ' Validating create tariff form');
        
        try {
            const serviceId = $('#createServiceId').val();
            const insurancePlanId = $('#createInsurancePlanId').val();
            const tariffPrice = $('#createTariffPrice').val();
            const patientShare = $('#createPatientShare').val();
            const insurerShare = $('#createInsurerShare').val();
            const coveragePercent = $('#createCoveragePercent').val();

            // Validate required fields
            if (!serviceId || !insurancePlanId || !tariffPrice || !patientShare || !insurerShare || !coveragePercent) {
                return {
                    isValid: false,
                    errorMessage: 'لطفاً تمام فیلدهای مورد نیاز را پر کنید'
                };
            }

            // Validate numeric values
            const validationResult = this.validateNumericFields({
                tariffPrice: tariffPrice,
                patientShare: patientShare,
                insurerShare: insurerShare,
                coveragePercent: coveragePercent
            });

            if (!validationResult.isValid) {
                return validationResult;
            }

            return {
                isValid: true,
                data: {
                    serviceId: parseInt(serviceId),
                    insurancePlanId: parseInt(insurancePlanId),
                    tariffPrice: parseFloat(tariffPrice),
                    patientShare: parseFloat(patientShare),
                    insurerShare: parseFloat(insurerShare),
                    coveragePercent: parseFloat(coveragePercent)
                }
            };

        } catch (error) {
            console.error(window.MedicalConfig.logPrefix + ' Error validating create tariff form:', error);
            return {
                isValid: false,
                errorMessage: 'خطا در اعتبارسنجی فرم: ' + error.message
            };
        }
    },

    /**
     * Validate numeric fields
     * اعتبارسنجی فیلدهای عددی
     */
    validateNumericFields: function(fields) {
        console.log(window.MedicalConfig.logPrefix + ' Validating numeric fields', fields);
        
        try {
            // Validate tariff price
            if (isNaN(fields.tariffPrice) || parseFloat(fields.tariffPrice) < window.MedicalConfig.validationRules.minTariffPrice) {
                return {
                    isValid: false,
                    errorMessage: 'قیمت تعرفه باید عدد مثبت باشد'
                };
            }

            if (parseFloat(fields.tariffPrice) > window.MedicalConfig.validationRules.maxTariffPrice) {
                return {
                    isValid: false,
                    errorMessage: 'قیمت تعرفه بیش از حد مجاز است'
                };
            }

            // Validate patient share
            if (isNaN(fields.patientShare) || parseFloat(fields.patientShare) < window.MedicalConfig.validationRules.minPatientShare) {
                return {
                    isValid: false,
                    errorMessage: 'سهم بیمار باید عدد مثبت باشد'
                };
            }

            if (parseFloat(fields.patientShare) > window.MedicalConfig.validationRules.maxPatientShare) {
                return {
                    isValid: false,
                    errorMessage: 'سهم بیمار بیش از حد مجاز است'
                };
            }

            // Validate insurer share
            if (isNaN(fields.insurerShare) || parseFloat(fields.insurerShare) < window.MedicalConfig.validationRules.minInsurerShare) {
                return {
                    isValid: false,
                    errorMessage: 'سهم بیمه باید عدد مثبت باشد'
                };
            }

            if (parseFloat(fields.insurerShare) > window.MedicalConfig.validationRules.maxInsurerShare) {
                return {
                    isValid: false,
                    errorMessage: 'سهم بیمه بیش از حد مجاز است'
                };
            }

            // Validate coverage percent
            if (isNaN(fields.coveragePercent) || parseFloat(fields.coveragePercent) < window.MedicalConfig.validationRules.minCoveragePercent) {
                return {
                    isValid: false,
                    errorMessage: 'درصد پوشش باید عدد مثبت باشد'
                };
            }

            if (parseFloat(fields.coveragePercent) > window.MedicalConfig.validationRules.maxCoveragePercent) {
                return {
                    isValid: false,
                    errorMessage: 'درصد پوشش نمی‌تواند بیش از 100 باشد'
                };
            }

            return {
                isValid: true
            };

        } catch (error) {
            console.error(window.MedicalConfig.logPrefix + ' Error validating numeric fields:', error);
            return {
                isValid: false,
                errorMessage: 'خطا در اعتبارسنجی فیلدهای عددی: ' + error.message
            };
        }
    },

    /**
     * Validate field
     * اعتبارسنجی فیلد
     */
    validateField: function(field) {
        console.log(window.MedicalConfig.logPrefix + ' Validating field', field);
        
        try {
            const value = field.val();
            const fieldName = field.attr('name');
            
            // Remove existing validation classes
            field.removeClass('is-valid is-invalid');
            
            // Validate based on field type
            switch (fieldName) {
                case 'TariffPrice':
                    return this.validateTariffPrice(value, field);
                case 'PatientShare':
                    return this.validatePatientShare(value, field);
                case 'InsurerShare':
                    return this.validateInsurerShare(value, field);
                case 'CoveragePercent':
                    return this.validateCoveragePercent(value, field);
                default:
                    return true;
            }
            
        } catch (error) {
            console.error(window.MedicalConfig.logPrefix + ' Error validating field:', error);
            return false;
        }
    },

    /**
     * Validate tariff price
     * اعتبارسنجی قیمت تعرفه
     */
    validateTariffPrice: function(value, field) {
        try {
            if (value && (isNaN(value) || parseFloat(value) < window.MedicalConfig.validationRules.minTariffPrice)) {
                field.addClass('is-invalid');
                return false;
            }
            
            if (value && parseFloat(value) > window.MedicalConfig.validationRules.maxTariffPrice) {
                field.addClass('is-invalid');
                return false;
            }
            
            if (value) {
                field.addClass('is-valid');
            }
            
            return true;
        } catch (error) {
            console.error(window.MedicalConfig.logPrefix + ' Error validating tariff price:', error);
            return false;
        }
    },

    /**
     * Validate patient share
     * اعتبارسنجی سهم بیمار
     */
    validatePatientShare: function(value, field) {
        try {
            if (value && (isNaN(value) || parseFloat(value) < window.MedicalConfig.validationRules.minPatientShare)) {
                field.addClass('is-invalid');
                return false;
            }
            
            if (value && parseFloat(value) > window.MedicalConfig.validationRules.maxPatientShare) {
                field.addClass('is-invalid');
                return false;
            }
            
            if (value) {
                field.addClass('is-valid');
            }
            
            return true;
        } catch (error) {
            console.error(window.MedicalConfig.logPrefix + ' Error validating patient share:', error);
            return false;
        }
    },

    /**
     * Validate insurer share
     * اعتبارسنجی سهم بیمه
     */
    validateInsurerShare: function(value, field) {
        try {
            if (value && (isNaN(value) || parseFloat(value) < window.MedicalConfig.validationRules.minInsurerShare)) {
                field.addClass('is-invalid');
                return false;
            }
            
            if (value && parseFloat(value) > window.MedicalConfig.validationRules.maxInsurerShare) {
                field.addClass('is-invalid');
                return false;
            }
            
            if (value) {
                field.addClass('is-valid');
            }
            
            return true;
        } catch (error) {
            console.error(window.MedicalConfig.logPrefix + ' Error validating insurer share:', error);
            return false;
        }
    },

    /**
     * Validate coverage percent
     * اعتبارسنجی درصد پوشش
     */
    validateCoveragePercent: function(value, field) {
        try {
            if (value && (isNaN(value) || parseFloat(value) < window.MedicalConfig.validationRules.minCoveragePercent)) {
                field.addClass('is-invalid');
                return false;
            }
            
            if (value && parseFloat(value) > window.MedicalConfig.validationRules.maxCoveragePercent) {
                field.addClass('is-invalid');
                return false;
            }
            
            if (value) {
                field.addClass('is-valid');
            }
            
            return true;
        } catch (error) {
            console.error(window.MedicalConfig.logPrefix + ' Error validating coverage percent:', error);
            return false;
        }
    }
    };
    
})();
