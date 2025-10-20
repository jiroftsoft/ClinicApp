/**
 * SaveProcessor Module - پردازش ذخیره
 * Single Responsibility: فقط پردازش عملیات ذخیره
 * 
 * @author ClinicApp Team
 * @version 1.0.0
 * @description ماژول تخصصی برای پردازش ذخیره اطلاعات بیمه
 */

(function() {
    'use strict';

    // ========================================
    // SAVE PROCESSOR MODULE
    // ========================================
    window.SaveProcessor = {
        
        // Configuration
        config: {
            endpoints: {
                save: '/Reception/Insurance/Save'
            },
            messages: {
                saving: 'در حال ذخیره...',
                success: 'اطلاعات با موفقیت ذخیره شد',
                error: 'خطا در ذخیره اطلاعات',
                validationError: 'لطفاً فرم را کامل کنید'
            },
            timeout: 30000 // 30 seconds
        },

        // State
        isSaving: false,
        currentRequest: null,
        isInitialized: false,

        // ========================================
        // INITIALIZATION - مقداردهی اولیه
        // ========================================
        init: function() {
            console.log('[SaveProcessor] Initializing...');
            
            try {
                this.isInitialized = true;
                console.log('[SaveProcessor] ✅ Initialized successfully');
            } catch (error) {
                console.error('[SaveProcessor] ❌ Initialization failed:', error);
                throw error;
            }
        },

        // ========================================
        // SAVE INSURANCE - ذخیره اطلاعات بیمه
        // ========================================
        saveInsurance: function(patientId, formData) {
            console.log('[SaveProcessor] Saving insurance for patient:', patientId);
            
            try {
                if (this.isSaving) {
                    console.warn('[SaveProcessor] Save operation already in progress');
                    return Promise.reject('Save operation already in progress');
                }
                
                this.isSaving = true;
                this.showSavingMessage();
                
                var requestData = this.prepareRequestData(patientId, formData);
                var request = this.sendSaveRequest(requestData);
                
                this.currentRequest = request;
                
                return request;
            } catch (error) {
                console.error('[SaveProcessor] Error saving insurance:', error);
                this.isSaving = false;
                throw error;
            }
        },

        // ========================================
        // PREPARE REQUEST DATA - آماده‌سازی داده‌های درخواست
        // ========================================
        prepareRequestData: function(patientId, formData) {
            console.log('[SaveProcessor] Preparing request data...');
            
            try {
                var requestData = {
                    PatientId: patientId,
                    PrimaryInsuranceProviderId: formData.primaryProvider || null,
                    PrimaryInsurancePlanId: formData.primaryPlan || null,
                    PrimaryPolicyNumber: formData.primaryPolicyNumber || null,
                    PrimaryCardNumber: formData.primaryCardNumber || null,
                    SupplementaryInsuranceProviderId: formData.supplementaryProvider || null,
                    SupplementaryInsurancePlanId: formData.supplementaryPlan || null,
                    SupplementaryPolicyNumber: formData.supplementaryPolicyNumber || null,
                    SupplementaryExpiryDate: formData.supplementaryExpiry || null,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                };
                
                console.log('[SaveProcessor] Request data prepared:', requestData);
                return requestData;
            } catch (error) {
                console.error('[SaveProcessor] Error preparing request data:', error);
                throw error;
            }
        },

        // ========================================
        // SEND SAVE REQUEST - ارسال درخواست ذخیره
        // ========================================
        sendSaveRequest: function(requestData) {
            console.log('[SaveProcessor] Sending save request...');
            
            try {
                var self = this;
                
                return $.ajax({
                    url: this.config.endpoints.save,
                    type: 'POST',
                    data: requestData,
                    timeout: this.config.timeout,
                    success: function(response) {
                        console.log('[SaveProcessor] Save request successful:', response);
                        self.handleSaveSuccess(response);
                    },
                    error: function(xhr, status, error) {
                        console.error('[SaveProcessor] Save request failed:', xhr, status, error);
                        self.handleSaveError(xhr, status, error);
                    },
                    complete: function() {
                        console.log('[SaveProcessor] Save request completed');
                        self.isSaving = false;
                        self.currentRequest = null;
                    }
                });
            } catch (error) {
                console.error('[SaveProcessor] Error sending save request:', error);
                this.isSaving = false;
                throw error;
            }
        },

        // ========================================
        // HANDLE SAVE SUCCESS - مدیریت موفقیت ذخیره
        // ========================================
        handleSaveSuccess: function(response) {
            console.log('[SaveProcessor] Handling save success...');
            
            try {
                var parsed = response;
                if (typeof response === 'string') {
                    parsed = JSON.parse(response);
                }
                
                if (parsed.success === true) {
                    this.showSuccessMessage(parsed.message || this.config.messages.success);
                    this.triggerSaveSuccessEvent(parsed);
                } else {
                    this.showErrorMessage(parsed.message || this.config.messages.error);
                    this.triggerSaveErrorEvent(parsed);
                }
            } catch (error) {
                console.error('[SaveProcessor] Error handling save success:', error);
                this.showErrorMessage(this.config.messages.error);
                this.triggerSaveErrorEvent({ message: this.config.messages.error });
            }
        },

        // ========================================
        // HANDLE SAVE ERROR - مدیریت خطای ذخیره
        // ========================================
        handleSaveError: function(xhr, status, error) {
            console.log('[SaveProcessor] Handling save error...');
            
            try {
                var errorMessage = this.config.messages.error;
                
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                } else if (xhr.responseText) {
                    errorMessage = xhr.responseText;
                }
                
                this.showErrorMessage(errorMessage);
                this.triggerSaveErrorEvent({ message: errorMessage });
            } catch (err) {
                console.error('[SaveProcessor] Error handling save error:', err);
                this.showErrorMessage(this.config.messages.error);
                this.triggerSaveErrorEvent({ message: this.config.messages.error });
            }
        },

        // ========================================
        // SHOW SAVING MESSAGE - نمایش پیام در حال ذخیره
        // ========================================
        showSavingMessage: function() {
            console.log('[SaveProcessor] Showing saving message...');
            
            try {
                if (typeof toastr !== 'undefined') {
                    toastr.info(this.config.messages.saving);
                } else {
                    console.log('[SaveProcessor] Info:', this.config.messages.saving);
                }
            } catch (error) {
                console.error('[SaveProcessor] Error showing saving message:', error);
            }
        },

        // ========================================
        // SHOW SUCCESS MESSAGE - نمایش پیام موفقیت
        // ========================================
        showSuccessMessage: function(message) {
            console.log('[SaveProcessor] Showing success message:', message);
            
            try {
                if (typeof toastr !== 'undefined') {
                    toastr.success(message);
                } else {
                    console.log('[SaveProcessor] Success:', message);
                }
            } catch (error) {
                console.error('[SaveProcessor] Error showing success message:', error);
            }
        },

        // ========================================
        // SHOW ERROR MESSAGE - نمایش پیام خطا
        // ========================================
        showErrorMessage: function(message) {
            console.log('[SaveProcessor] Showing error message:', message);
            
            try {
                if (typeof toastr !== 'undefined') {
                    toastr.error(message);
                } else {
                    console.error('[SaveProcessor] Error:', message);
                }
            } catch (error) {
                console.error('[SaveProcessor] Error showing error message:', error);
            }
        },

        // ========================================
        // TRIGGER SAVE SUCCESS EVENT - فعال کردن event موفقیت ذخیره
        // ========================================
        triggerSaveSuccessEvent: function(data) {
            console.log('[SaveProcessor] Triggering save success event...');
            
            try {
                $(document).trigger('insuranceSaveSuccess', [data]);
            } catch (error) {
                console.error('[SaveProcessor] Error triggering save success event:', error);
            }
        },

        // ========================================
        // TRIGGER SAVE ERROR EVENT - فعال کردن event خطای ذخیره
        // ========================================
        triggerSaveErrorEvent: function(data) {
            console.log('[SaveProcessor] Triggering save error event...');
            
            try {
                $(document).trigger('insuranceSaveError', [data]);
            } catch (error) {
                console.error('[SaveProcessor] Error triggering save error event:', error);
            }
        },

        // ========================================
        // CANCEL SAVE - لغو ذخیره
        // ========================================
        cancelSave: function() {
            console.log('[SaveProcessor] Canceling save...');
            
            try {
                if (this.currentRequest) {
                    this.currentRequest.abort();
                    this.currentRequest = null;
                }
                
                this.isSaving = false;
                console.log('[SaveProcessor] Save canceled');
            } catch (error) {
                console.error('[SaveProcessor] Error canceling save:', error);
            }
        },

        // ========================================
        // GET SAVE STATUS - دریافت وضعیت ذخیره
        // ========================================
        getSaveStatus: function() {
            return {
                isSaving: this.isSaving,
                isInitialized: this.isInitialized
            };
        },

        // ========================================
        // RESET - بازنشانی
        // ========================================
        reset: function() {
            console.log('[SaveProcessor] Resetting...');
            
            try {
                this.cancelSave();
                this.isInitialized = false;
                console.log('[SaveProcessor] Reset completed');
            } catch (error) {
                console.error('[SaveProcessor] Error resetting:', error);
                throw error;
            }
        }
    };

    // Auto-initialize when DOM is ready
    $(document).ready(function() {
        try {
            window.SaveProcessor.init();
        } catch (error) {
            console.error('[SaveProcessor] Auto-initialization failed:', error);
        }
    });

})();
