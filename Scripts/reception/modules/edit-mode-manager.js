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
        // ENABLE EDIT MODE - فعال کردن حالت ویرایش
        // ========================================
        enableEditMode: function() {
            console.log('[EditModeManager] Enabling edit mode...');
            
            try {
                if (this.isEditMode) {
                    console.log('[EditModeManager] Edit mode already enabled');
                    return;
                }
                
                this.isEditMode = true;
                this.updateUI();
                this.showEditModeMessage();
                console.log('[EditModeManager] ✅ Edit mode enabled');
            } catch (error) {
                console.error('[EditModeManager] Error enabling edit mode:', error);
                throw error;
            }
        },

        // ========================================
        // DISABLE EDIT MODE - غیرفعال کردن حالت ویرایش
        // ========================================
        disableEditMode: function() {
            console.log('[EditModeManager] Disabling edit mode...');
            
            try {
                if (!this.isEditMode) {
                    console.log('[EditModeManager] Edit mode already disabled');
                    return;
                }
                
                this.isEditMode = false;
                this.updateUI();
                console.log('[EditModeManager] ✅ Edit mode disabled');
            } catch (error) {
                console.error('[EditModeManager] Error disabling edit mode:', error);
                throw error;
            }
        },

        // ========================================
        // UPDATE UI - به‌روزرسانی رابط کاربری
        // ========================================
        updateUI: function() {
            console.log('[EditModeManager] Updating UI...');
            
            try {
                var $saveButton = $(this.config.selectors.saveButton);
                var $editModeIndicator = $(this.config.selectors.editModeIndicator);
                var $formContainer = $(this.config.selectors.formContainer);
                
                if (this.isEditMode) {
                    // Enable save button
                    $saveButton.prop('disabled', false).removeClass(this.config.classes.disabled);
                    
                    // Show edit mode indicator
                    if ($editModeIndicator.length > 0) {
                        $editModeIndicator.addClass(this.config.classes.active).show();
                    }
                    
                    // Add edit mode class to form container
                    $formContainer.addClass(this.config.classes.editMode);
                } else {
                    // Disable save button
                    $saveButton.prop('disabled', true).addClass(this.config.classes.disabled);
                    
                    // Hide edit mode indicator
                    if ($editModeIndicator.length > 0) {
                        $editModeIndicator.removeClass(this.config.classes.active).hide();
                    }
                    
                    // Remove edit mode class from form container
                    $formContainer.removeClass(this.config.classes.editMode);
                }
                
                console.log('[EditModeManager] UI updated');
            } catch (error) {
                console.error('[EditModeManager] Error updating UI:', error);
                throw error;
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
