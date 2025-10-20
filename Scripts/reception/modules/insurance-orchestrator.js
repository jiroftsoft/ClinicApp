/**
 * InsuranceOrchestrator Module - هماهنگ‌کننده بیمه
 * Single Responsibility: فقط هماهنگی بین ماژول‌های تخصصی
 * 
 * @author ClinicApp Team
 * @version 1.0.0
 * @description ماژول اصلی برای هماهنگی ماژول‌های تخصصی بیمه
 */

(function() {
    'use strict';

    // ========================================
    // INSURANCE ORCHESTRATOR MODULE
    // ========================================
    window.InsuranceOrchestrator = {
        
        // Configuration
        config: {
            selectors: {
                form: '#insuranceForm',
                saveButton: '#saveInsuranceBtn'
            },
            patientId: null,
            isInitialized: false
        },

        // ========================================
        // INITIALIZATION - مقداردهی اولیه
        // ========================================
        init: function() {
            console.log('[InsuranceOrchestrator] Initializing...');
            
            try {
                this.setupEventListeners();
                this.isInitialized = true;
                console.log('[InsuranceOrchestrator] ✅ Initialized successfully');
            } catch (error) {
                console.error('[InsuranceOrchestrator] ❌ Initialization failed:', error);
                throw error;
            }
        },

        // ========================================
        // SETUP EVENT LISTENERS - تنظیم event listeners
        // ========================================
        setupEventListeners: function() {
            console.log('[InsuranceOrchestrator] Setting up event listeners...');
            
            try {
                var self = this;
                
                // Form change events
                $(document).on('change input', this.config.selectors.form + ' input, ' + this.config.selectors.form + ' select', function() {
                    self.handleFormChange();
                });
                
                // Save button click
                $(document).on('click', this.config.selectors.saveButton, function(e) {
                    e.preventDefault();
                    self.handleSaveClick();
                });
                
                // Patient search success
                $(document).on('patientSearchSuccess', function(e, data) {
                    self.handlePatientSearchSuccess(data);
                });
                
                // Insurance load success
                $(document).on('insuranceLoadSuccess', function(e, data) {
                    self.handleInsuranceLoadSuccess(data);
                });
                
                console.log('[InsuranceOrchestrator] Event listeners setup completed');
            } catch (error) {
                console.error('[InsuranceOrchestrator] Error setting up event listeners:', error);
                throw error;
            }
        },

        // ========================================
        // HANDLE FORM CHANGE - مدیریت تغییر فرم
        // ========================================
        handleFormChange: function() {
            console.log('[InsuranceOrchestrator] Handling form change...');
            
            try {
                // تشخیص تغییرات با استفاده از FormChangeDetector
                var changeResult = window.FormChangeDetector.detectChanges();
                
                if (changeResult.hasChanges) {
                    console.log('[InsuranceOrchestrator] Changes detected, enabling edit mode');
                    window.EditModeManager.enableEditMode();
                } else {
                    console.log('[InsuranceOrchestrator] No changes detected, disabling edit mode');
                    window.EditModeManager.disableEditMode();
                }
            } catch (error) {
                console.error('[InsuranceOrchestrator] Error handling form change:', error);
            }
        },

        // ========================================
        // HANDLE SAVE CLICK - مدیریت کلیک ذخیره
        // ========================================
        handleSaveClick: function() {
            console.log('[InsuranceOrchestrator] Handling save click...');
            
            try {
                // اعتبارسنجی فرم با استفاده از ValidationEngine
                var validationResult = window.ValidationEngine.validateForm();
                
                if (!validationResult.isValid) {
                    console.log('[InsuranceOrchestrator] Form validation failed:', validationResult.errors);
                    this.showValidationErrors(validationResult.errors);
                    return;
                }
                
                // ذخیره با استفاده از SaveProcessor
                if (this.config.patientId) {
                    window.SaveProcessor.saveInsurance(this.config.patientId, validationResult.values)
                        .then(function(response) {
                            console.log('[InsuranceOrchestrator] Save completed successfully');
                        })
                        .catch(function(error) {
                            console.error('[InsuranceOrchestrator] Save failed:', error);
                        });
                } else {
                    console.error('[InsuranceOrchestrator] No patient ID available for save');
                }
            } catch (error) {
                console.error('[InsuranceOrchestrator] Error handling save click:', error);
            }
        },

        // ========================================
        // HANDLE PATIENT SEARCH SUCCESS - مدیریت موفقیت جستجوی بیمار
        // ========================================
        handlePatientSearchSuccess: function(data) {
            console.log('[InsuranceOrchestrator] Handling patient search success...');
            
            try {
                if (data && data.PatientId) {
                    this.config.patientId = data.PatientId;
                    console.log('[InsuranceOrchestrator] Patient ID set:', this.config.patientId);
                }
            } catch (error) {
                console.error('[InsuranceOrchestrator] Error handling patient search success:', error);
            }
        },

        // ========================================
        // HANDLE INSURANCE LOAD SUCCESS - مدیریت موفقیت بارگذاری بیمه
        // ========================================
        handleInsuranceLoadSuccess: function(data) {
            console.log('[InsuranceOrchestrator] Handling insurance load success...');
            
            try {
                // به‌روزرسانی مقادیر اولیه در FormChangeDetector
                window.FormChangeDetector.updateOriginalValues();
                
                // غیرفعال کردن حالت ویرایش
                window.EditModeManager.disableEditMode();
                
                console.log('[InsuranceOrchestrator] Insurance load success handled');
            } catch (error) {
                console.error('[InsuranceOrchestrator] Error handling insurance load success:', error);
            }
        },

        // ========================================
        // SHOW VALIDATION ERRORS - نمایش خطاهای اعتبارسنجی
        // ========================================
        showValidationErrors: function(errors) {
            console.log('[InsuranceOrchestrator] Showing validation errors...');
            
            try {
                var errorMessages = errors.map(function(error) {
                    return error.message;
                });
                
                var message = 'لطفاً فرم را کامل کنید:\n' + errorMessages.join('\n');
                
                if (typeof toastr !== 'undefined') {
                    toastr.error(message);
                } else {
                    console.error('[InsuranceOrchestrator] Validation errors:', message);
                }
            } catch (error) {
                console.error('[InsuranceOrchestrator] Error showing validation errors:', error);
            }
        },

        // ========================================
        // SET PATIENT ID - تنظیم شناسه بیمار
        // ========================================
        setPatientId: function(patientId) {
            console.log('[InsuranceOrchestrator] Setting patient ID:', patientId);
            
            try {
                this.config.patientId = patientId;
            } catch (error) {
                console.error('[InsuranceOrchestrator] Error setting patient ID:', error);
            }
        },

        // ========================================
        // GET STATUS - دریافت وضعیت
        // ========================================
        getStatus: function() {
            return {
                isInitialized: this.isInitialized,
                patientId: this.config.patientId,
                formChangeDetector: window.FormChangeDetector ? 'available' : 'not available',
                editModeManager: window.EditModeManager ? 'available' : 'not available',
                validationEngine: window.ValidationEngine ? 'available' : 'not available',
                saveProcessor: window.SaveProcessor ? 'available' : 'not available'
            };
        },

        // ========================================
        // RESET - بازنشانی
        // ========================================
        reset: function() {
            console.log('[InsuranceOrchestrator] Resetting...');
            
            try {
                this.config.patientId = null;
                this.isInitialized = false;
                console.log('[InsuranceOrchestrator] Reset completed');
            } catch (error) {
                console.error('[InsuranceOrchestrator] Error resetting:', error);
                throw error;
            }
        }
    };

    // Auto-initialize when DOM is ready
    $(document).ready(function() {
        try {
            window.InsuranceOrchestrator.init();
        } catch (error) {
            console.error('[InsuranceOrchestrator] Auto-initialization failed:', error);
        }
    });

})();
