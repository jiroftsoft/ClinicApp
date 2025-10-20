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
        // SETUP EVENT LISTENERS - تنظیم event listeners (Production-Optimized)
        // ========================================
        setupEventListeners: function() {
            console.log('[InsuranceOrchestrator] 🔗 Setting up event listeners...');
            
            try {
                var self = this;
                
                // Form change events with debouncing - SPECIFIC SELECTORS
                var formSelectors = '#insuranceProvider, #insurancePlan, #policyNumber, #cardNumber, #supplementaryProvider, #supplementaryPlan, #supplementaryPolicyNumber, #supplementaryExpiry';
                
                $(document).on('change.insuranceOrchestrator', formSelectors, function() {
                    console.log('[InsuranceOrchestrator] 🔄 Form field changed:', this.id);
                    self.handleFormChange();
                });
                
                $(document).on('input.insuranceOrchestrator', formSelectors, this.debounce(function() {
                    console.log('[InsuranceOrchestrator] 🔄 Form field input:', this.id);
                    self.handleFormChange();
                }, 300));
                
                // Save button click
                $(document).on('click.insuranceOrchestrator', this.config.selectors.saveButton, function(e) {
                    e.preventDefault();
                    console.log('[InsuranceOrchestrator] 💾 Save button clicked');
                    self.handleSaveClick();
                });
                
                // Patient search success
                $(document).on('patientSearchSuccess.insuranceOrchestrator', function(e, data) {
                    console.log('[InsuranceOrchestrator] 👤 Patient search success:', data);
                    self.handlePatientSearchSuccess(data);
                });
                
                // Insurance load success
                $(document).on('insuranceLoadSuccess.insuranceOrchestrator', function(e, data) {
                    console.log('[InsuranceOrchestrator] 🏥 Insurance load success:', data);
                    self.handleInsuranceLoadSuccess(data);
                });
                
                // EventBus integration
                if (window.ReceptionEventBus) {
                    window.ReceptionEventBus.on('insurance:changed', function(data) {
                        console.log('[InsuranceOrchestrator] 📡 Insurance changed event received:', data);
                        self.handleInsuranceChanged(data);
                    });
                }
                
                console.log('[InsuranceOrchestrator] ✅ Event listeners setup completed');
            } catch (error) {
                console.error('[InsuranceOrchestrator] ❌ Error setting up event listeners:', error);
                throw error;
            }
        },

        // ========================================
        // DEBOUNCE FUNCTION - تابع debounce
        // ========================================
        debounce: function(func, wait) {
            var timeout;
            return function() {
                var context = this;
                var args = arguments;
                clearTimeout(timeout);
                timeout = setTimeout(function() {
                    func.apply(context, args);
                }, wait);
            };
        },

        // ========================================
        // HANDLE FORM CHANGE - مدیریت تغییر فرم (Production-Optimized)
        // ========================================
        handleFormChange: function() {
            console.log('[InsuranceOrchestrator] 🔄 Handling form change...');
            
            try {
                // 1. Detect changes using FormChangeDetector
                var changeResult = window.FormChangeDetector.detectChanges();
                
                console.log('[InsuranceOrchestrator] 📊 Change detection result:', {
                    hasChanges: changeResult.hasChanges,
                    changesCount: changeResult.changes ? changeResult.changes.length : 0,
                    changes: changeResult.changes
                });
                
                if (changeResult.hasChanges) {
                    console.log('[InsuranceOrchestrator] ✅ Changes detected, enabling edit mode');
                    
                    // 2. Enable edit mode using EditModeManager
                    if (window.EditModeManager) {
                        window.EditModeManager.enableEditMode();
                        console.log('[InsuranceOrchestrator] ✅ Edit mode enabled');
                    }
                    
                    // 3. Validate form using ValidationEngine
                    if (window.ValidationEngine) {
                        var validationResult = window.ValidationEngine.validateForm();
                        console.log('[InsuranceOrchestrator] 📊 Validation result:', validationResult);
                        
                        // 4. Update save button state
                        this.updateSaveButtonState(validationResult);
                    }
                    
                    // 5. Show user feedback
                    this.showChangeDetectedMessage(changeResult.changes);
                    
                } else {
                    console.log('[InsuranceOrchestrator] ❌ No changes detected, disabling edit mode');
                    
                    // Disable edit mode
                    if (window.EditModeManager) {
                        window.EditModeManager.disableEditMode();
                        console.log('[InsuranceOrchestrator] ❌ Edit mode disabled');
                    }
                }
                
            } catch (error) {
                console.error('[InsuranceOrchestrator] ❌ Error handling form change:', error);
                this.handleError(error, 'handleFormChange');
            }
        },

        // ========================================
        // FORCE FORM CHANGE DETECTION - اجبار تشخیص تغییرات فرم
        // ========================================
        forceFormChangeDetection: function() {
            console.log('[InsuranceOrchestrator] 🔄 Forcing form change detection...');
            
            try {
                // Force FormChangeDetector to re-capture current values
                if (window.FormChangeDetector) {
                    window.FormChangeDetector.updateOriginalValuesFromCurrentForm();
                    console.log('[InsuranceOrchestrator] ✅ Original values updated from current form');
                }
                
                // Trigger form change detection
                this.handleFormChange();
                
            } catch (error) {
                console.error('[InsuranceOrchestrator] ❌ Error forcing form change detection:', error);
                this.handleError(error, 'forceFormChangeDetection');
            }
        },

        // ========================================
        // SHOW CHANGE DETECTED MESSAGE - نمایش پیام تشخیص تغییرات
        // ========================================
        showChangeDetectedMessage: function(changes) {
            console.log('[InsuranceOrchestrator] 📢 Showing change detected message...');
            
            try {
                var changeCount = changes ? changes.length : 0;
                var message = 'تغییرات در فرم تشخیص داده شد. برای ذخیره تغییرات روی دکمه ذخیره کلیک کنید.';
                
                // Use Toastr if available
                if (window.ReceptionToastr && window.ReceptionToastr.helpers && window.ReceptionToastr.helpers.showInfo) {
                    window.ReceptionToastr.helpers.showInfo(message);
                } else {
                    console.log('[InsuranceOrchestrator] ℹ️ Info:', message);
                }
                
            } catch (error) {
                console.error('[InsuranceOrchestrator] ❌ Error showing change detected message:', error);
            }
        },

        // ========================================
        // UPDATE SAVE BUTTON STATE - به‌روزرسانی وضعیت دکمه ذخیره
        // ========================================
        updateSaveButtonState: function(validationResult) {
            console.log('[InsuranceOrchestrator] 🔄 Updating save button state...');
            
            try {
                var $saveButton = $(this.config.selectors.saveButton);
                
                if ($saveButton.length === 0) {
                    console.warn('[InsuranceOrchestrator] ⚠️ Save button not found');
                    return;
                }
                
                // Enable save button if there are changes and edit mode is enabled
                var shouldEnable = validationResult.hasChanges && window.EditModeManager && window.EditModeManager.isEditMode;
                
                if (shouldEnable) {
                    $saveButton
                        .prop('disabled', false)
                        .removeClass('d-none btn-secondary')
                        .addClass('btn-success')
                        .html('<i class="fas fa-save"></i> ذخیره اطلاعات بیمه');
                    console.log('[InsuranceOrchestrator] ✅ Save button enabled and shown');
                } else {
                    $saveButton
                        .prop('disabled', true)
                        .addClass('d-none btn-secondary')
                        .removeClass('btn-success')
                        .html('<i class="fas fa-save"></i> ذخیره اطلاعات بیمه');
                    console.log('[InsuranceOrchestrator] ❌ Save button disabled and hidden');
                }
                
            } catch (error) {
                console.error('[InsuranceOrchestrator] ❌ Error updating save button state:', error);
            }
        },

        // ========================================
        // HANDLE INSURANCE CHANGED - مدیریت تغییر بیمه
        // ========================================
        handleInsuranceChanged: function(data) {
            console.log('[InsuranceOrchestrator] 📡 Handling insurance changed event...');
            
            try {
                // Update form state based on changed data
                this.updateFormState(data);
                
            } catch (error) {
                console.error('[InsuranceOrchestrator] ❌ Error handling insurance changed:', error);
            }
        },

        // ========================================
        // UPDATE FORM STATE - به‌روزرسانی وضعیت فرم
        // ========================================
        updateFormState: function(data) {
            console.log('[InsuranceOrchestrator] 🔄 Updating form state...');
            
            try {
                // Update form fields based on data
                if (data.primaryProvider) {
                    $('#insuranceProvider').val(data.primaryProvider).trigger('change');
                }
                if (data.primaryPlan) {
                    $('#insurancePlan').val(data.primaryPlan).trigger('change');
                }
                // ... other fields
                
            } catch (error) {
                console.error('[InsuranceOrchestrator] ❌ Error updating form state:', error);
            }
        },

        // ========================================
        // HANDLE ERROR - مدیریت خطا
        // ========================================
        handleError: function(error, context) {
            console.error('[InsuranceOrchestrator] 🚨 Error in', context, ':', error);
            this.showError('خطا در سیستم. لطفاً صفحه را بازخوانی کنید.');
        },

        // ========================================
        // SHOW ERROR - نمایش پیام خطا
        // ========================================
        showError: function(message) {
            if (window.ReceptionToastr && window.ReceptionToastr.helpers && window.ReceptionToastr.helpers.showError) {
                window.ReceptionToastr.helpers.showError(message);
            } else {
                console.error('[InsuranceOrchestrator] ❌ Error:', message);
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
                // Wait for form to be fully populated before setting original values
                var self = this;
                setTimeout(function() {
                    // به‌روزرسانی مقادیر اولیه در FormChangeDetector
                    if (window.FormChangeDetector) {
                        window.FormChangeDetector.updateOriginalValuesFromCurrentForm();
                        console.log('[InsuranceOrchestrator] ✅ Original values updated from current form');
                    }
                    
                    // غیرفعال کردن حالت ویرایش
                    if (window.EditModeManager) {
                        window.EditModeManager.disableEditMode();
                        console.log('[InsuranceOrchestrator] ✅ Edit mode disabled');
                    }
                    
                    console.log('[InsuranceOrchestrator] ✅ Insurance load success handled');
                }, 1500); // Wait for form to be fully populated
                
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
