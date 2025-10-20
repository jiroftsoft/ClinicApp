/**
 * EditModeManager Module - مدیریت حالت ویرایش
 * Single Responsibility: فقط مدیریت حالت ویرایش فرم
 * 
 * @author ClinicApp Team
 * @version 1.0.0
 * @description ماژول تخصصی برای مدیریت حالت ویرایش فرم بیمه
 */

(function() {
    'use strict';

    // ========================================
    // EDIT MODE MANAGER MODULE
    // ========================================
    window.EditModeManager = {
        
        // Configuration
        config: {
            selectors: {
                saveButton: '#saveInsuranceBtn',
                editModeIndicator: '#editModeIndicator',
                formContainer: '#insuranceFormContainer'
            },
            classes: {
                editMode: 'edit-mode',
                disabled: 'disabled',
                active: 'active'
            },
            messages: {
                editModeEnabled: 'حالت ویرایش فعال شد',
                editModeDisabled: 'حالت ویرایش غیرفعال شد',
                changesDetected: 'تغییرات در فرم تشخیص داده شد. برای ذخیره تغییرات روی دکمه ذخیره کلیک کنید.'
            }
        },

        // State
        isEditMode: false,
        isInitialized: false,

        // ========================================
        // INITIALIZATION - مقداردهی اولیه
        // ========================================
        init: function() {
            console.log('[EditModeManager] Initializing...');
            
            try {
                this.setupEventListeners();
                this.initializeState();
                this.isInitialized = true;
                console.log('[EditModeManager] ✅ Initialized successfully');
            } catch (error) {
                console.error('[EditModeManager] ❌ Initialization failed:', error);
                throw error;
            }
        },

        // ========================================
        // SETUP EVENT LISTENERS - تنظیم event listeners
        // ========================================
        setupEventListeners: function() {
            console.log('[EditModeManager] Setting up event listeners...');
            
            try {
                // Save button click
                $(document).on('click', this.config.selectors.saveButton, function() {
                    console.log('[EditModeManager] Save button clicked');
                });
                
                console.log('[EditModeManager] Event listeners setup completed');
            } catch (error) {
                console.error('[EditModeManager] Error setting up event listeners:', error);
                throw error;
            }
        },

        // ========================================
        // INITIALIZE STATE - مقداردهی اولیه state
        // ========================================
        initializeState: function() {
            console.log('[EditModeManager] Initializing state...');
            
            try {
                this.isEditMode = false;
                this.updateUI();
                console.log('[EditModeManager] State initialized');
            } catch (error) {
                console.error('[EditModeManager] Error initializing state:', error);
                throw error;
            }
        },

        // ========================================
        // ENABLE EDIT MODE - فعال کردن حالت ویرایش (Production-Optimized)
        // ========================================
        enableEditMode: function() {
            console.log('[EditModeManager] ✅ Enabling edit mode...');
            
            try {
                if (this.isEditMode) {
                    console.log('[EditModeManager] Edit mode already enabled');
                    return;
                }
                
                this.isEditMode = true;
                this.updateUI();
                this.updateProgressSteps('editing');
                this.showEditModeMessage();
                console.log('[EditModeManager] ✅ Edit mode enabled');
            } catch (error) {
                console.error('[EditModeManager] ❌ Error enabling edit mode:', error);
                throw error;
            }
        },

        // ========================================
        // UPDATE PROGRESS STEPS - به‌روزرسانی مراحل پیشرفت
        // ========================================
        updateProgressSteps: function(step) {
            console.log('[EditModeManager] 🔄 Updating progress steps to:', step);
            
            try {
                // Reset all steps
                $('.step1, .step2, .step3, .step4').removeClass('active');
                
                switch (step) {
                    case 'editing':
                        $('.step3').addClass('active');
                        break;
                    case 'saving':
                        $('.step4').addClass('active');
                        break;
                    default:
                        $('.step1').addClass('active');
                        break;
                }
                
                console.log('[EditModeManager] ✅ Progress steps updated successfully');
            } catch (error) {
                console.error('[EditModeManager] ❌ Error updating progress steps:', error);
            }
        },

        // ========================================
        // DISABLE EDIT MODE - غیرفعال کردن حالت ویرایش (Production-Optimized)
        // ========================================
        disableEditMode: function() {
            console.log('[EditModeManager] ❌ Disabling edit mode...');
            
            try {
                if (!this.isEditMode) {
                    console.log('[EditModeManager] Edit mode already disabled');
                    return;
                }
                
                this.isEditMode = false;
                this.updateUI();
                this.updateProgressSteps('default');
                console.log('[EditModeManager] ✅ Edit mode disabled');
            } catch (error) {
                console.error('[EditModeManager] ❌ Error disabling edit mode:', error);
                throw error;
            }
        },

        // ========================================
        // UPDATE UI - به‌روزرسانی رابط کاربری (Production-Optimized)
        // ========================================
        updateUI: function() {
            console.log('[EditModeManager] 🔄 Updating UI...');
            
            try {
                var $saveButton = $(this.config.selectors.saveButton);
                var $editModeIndicator = $(this.config.selectors.editModeIndicator);
                var $formContainer = $(this.config.selectors.formContainer);
                
                if (this.isEditMode) {
                    // Enable save button and make it visible
                    $saveButton
                        .prop('disabled', false)
                        .removeClass('d-none btn-secondary')
                        .addClass('btn-success')
                        .removeClass(this.config.classes.disabled);
                    
                    // Show edit mode indicator
                    if ($editModeIndicator.length > 0) {
                        $editModeIndicator.addClass(this.config.classes.active).show();
                    }
                    
                    // Add edit mode class to form container
                    $formContainer.addClass(this.config.classes.editMode);
                    
                    console.log('[EditModeManager] ✅ Edit mode UI activated');
                } else {
                    // Disable save button and hide it
                    $saveButton
                        .prop('disabled', true)
                        .addClass('d-none btn-secondary')
                        .removeClass('btn-success')
                        .addClass(this.config.classes.disabled);
                    
                    // Hide edit mode indicator
                    if ($editModeIndicator.length > 0) {
                        $editModeIndicator.removeClass(this.config.classes.active).hide();
                    }
                    
                    // Remove edit mode class from form container
                    $formContainer.removeClass(this.config.classes.editMode);
                    
                    console.log('[EditModeManager] ❌ Edit mode UI deactivated');
                }
                
                console.log('[EditModeManager] UI updated');
            } catch (error) {
                console.error('[EditModeManager] Error updating UI:', error);
                throw error;
            }
        },

        // ========================================
        // UPDATE SAVE BUTTON STATE - به‌روزرسانی وضعیت دکمه ذخیره (Production-Optimized)
        // ========================================
        updateSaveButtonState: function() {
            console.log('[EditModeManager] 🔄 Updating save button state...');
            
            try {
                var validation = this.validateForm();
                var $saveBtn = $('#saveInsuranceBtn');
                
                console.log('[EditModeManager] 📊 Save button validation:', {
                    isValid: validation.isValid,
                    hasChanges: validation.hasChanges,
                    canSave: validation.canSave,
                    errors: validation.errors,
                    warnings: validation.warnings,
                    isEditMode: this.isEditMode
                });
                
                // Check if save button exists
                if ($saveBtn.length === 0) {
                    console.warn('[EditModeManager] ⚠️ Save button not found');
                    return;
                }
                
                // Determine button state - Enable if there are changes and edit mode is active
                var shouldEnable = validation.hasChanges && this.isEditMode;
                
                if (shouldEnable) {
                    // Enable and show save button
                    $saveBtn
                        .prop('disabled', false)
                        .removeClass('d-none btn-secondary')
                        .addClass('btn-success')
                        .html('<i class="fas fa-save"></i> ذخیره اطلاعات بیمه');
                    
                    console.log('[EditModeManager] ✅ Save button enabled and shown');
                } else {
                    // Disable and hide save button
                    $saveBtn
                        .prop('disabled', true)
                        .addClass('d-none btn-secondary')
                        .removeClass('btn-success')
                        .html('<i class="fas fa-save"></i> ذخیره اطلاعات بیمه');
                    
                    console.log('[EditModeManager] ❌ Save button disabled and hidden');
                }
                
                // Show validation messages if needed
                if (validation.errors.length > 0) {
                    this.showValidationErrors(validation.errors);
                }
                
                if (validation.warnings.length > 0) {
                    this.showValidationWarnings(validation.warnings);
                }
                
                console.log('[EditModeManager] ✅ Save button state updated');
                
            } catch (error) {
                console.error('[EditModeManager] ❌ Error updating save button state:', error);
                this.handleError(error);
            }
        },

        // ========================================
        // VALIDATE FORM - اعتبارسنجی فرم
        // ========================================
        validateForm: function() {
            console.log('[EditModeManager] 🔍 Validating form...');
            
            try {
                if (window.ValidationEngine) {
                    return window.ValidationEngine.validateForm();
                }
                
                return {
                    isValid: false,
                    errors: ['ValidationEngine not available'],
                    warnings: [],
                    hasChanges: false,
                    canSave: false
                };
                
            } catch (error) {
                console.error('[EditModeManager] ❌ Error validating form:', error);
                return {
                    isValid: false,
                    errors: ['خطا در اعتبارسنجی فرم'],
                    warnings: [],
                    hasChanges: false,
                    canSave: false
                };
            }
        },

        // ========================================
        // SHOW VALIDATION ERRORS - نمایش خطاهای اعتبارسنجی
        // ========================================
        showValidationErrors: function(errors) {
            console.log('[EditModeManager] ❌ Validation errors:', errors);
            
            var errorMessage = errors.join(', ');
            
            if (window.ReceptionToastr && window.ReceptionToastr.helpers && window.ReceptionToastr.helpers.showError) {
                window.ReceptionToastr.helpers.showError('خطاهای اعتبارسنجی: ' + errorMessage);
            } else {
                console.error('[EditModeManager] Validation errors: ' + errorMessage);
            }
        },

        // ========================================
        // SHOW VALIDATION WARNINGS - نمایش هشدارهای اعتبارسنجی
        // ========================================
        showValidationWarnings: function(warnings) {
            console.log('[EditModeManager] ⚠️ Validation warnings:', warnings);
            
            var warningMessage = warnings.join(', ');
            
            if (window.ReceptionToastr && window.ReceptionToastr.helpers && window.ReceptionToastr.helpers.showWarning) {
                window.ReceptionToastr.helpers.showWarning('هشدارهای اعتبارسنجی: ' + warningMessage);
            } else {
                console.warn('[EditModeManager] Validation warnings: ' + warningMessage);
            }
        },

        // ========================================
        // SHOW EDIT MODE MESSAGE - نمایش پیام حالت ویرایش
        // ========================================
        showEditModeMessage: function() {
            console.log('[EditModeManager] Showing edit mode message...');
            
            try {
                // Try to show toastr message
                if (typeof toastr !== 'undefined') {
                    toastr.info(this.config.messages.changesDetected);
                } else {
                    console.log('[EditModeManager] Info:', this.config.messages.changesDetected);
                }
            } catch (error) {
                console.error('[EditModeManager] Error showing edit mode message:', error);
                // Fallback to console
                console.log('[EditModeManager] Info:', this.config.messages.changesDetected);
            }
        },

        // ========================================
        // GET EDIT MODE STATUS - دریافت وضعیت حالت ویرایش
        // ========================================
        getEditModeStatus: function() {
            return {
                isEditMode: this.isEditMode,
                isInitialized: this.isInitialized
            };
        },

        // ========================================
        // HANDLE ERROR - مدیریت خطا
        // ========================================
        handleError: function(error, context) {
            console.error('[EditModeManager] 🚨 Error in', context, ':', error);
            this.showError('خطا در سیستم. لطفاً صفحه را بازخوانی کنید.');
        },

        // ========================================
        // SHOW ERROR - نمایش پیام خطا
        // ========================================
        showError: function(message) {
            if (window.ReceptionToastr && window.ReceptionToastr.helpers && window.ReceptionToastr.helpers.showError) {
                window.ReceptionToastr.helpers.showError(message);
            } else {
                console.error('[EditModeManager] ❌ Error:', message);
            }
        },

        // ========================================
        // RESET - بازنشانی
        // ========================================
        reset: function() {
            console.log('[EditModeManager] Resetting...');
            
            try {
                this.disableEditMode();
                this.isInitialized = false;
                console.log('[EditModeManager] Reset completed');
            } catch (error) {
                console.error('[EditModeManager] Error resetting:', error);
                throw error;
            }
        }
    };

    // Auto-initialize when DOM is ready
    $(document).ready(function() {
        try {
            window.EditModeManager.init();
        } catch (error) {
            console.error('[EditModeManager] Auto-initialization failed:', error);
        }
    });

})();
