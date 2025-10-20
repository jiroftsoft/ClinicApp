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
        // SAVE INSURANCE - ذخیره اطلاعات بیمه (Production-Optimized)
        // ========================================
        saveInsurance: function(patientId, formData) {
            console.log('[SaveProcessor] 💾 Starting save process for patient:', patientId);
            
            try {
                // 1. CONCURRENCY PROTECTION - محافظت از تداخل
                if (this.isSaving) {
                    console.warn('[SaveProcessor] ⚠️ Save already in progress, queuing request');
                    this.queueSaveRequest(patientId, formData);
                    return Promise.reject('Save operation queued');
                }
                
                // 2. PERFORMANCE MONITORING - نظارت بر عملکرد
                this.startPerformanceMonitoring('save');
                
                // 3. UI STATE MANAGEMENT - مدیریت وضعیت UI
                this.setSaveInProgressState();
                
                // 4. DATA SANITIZATION - پاکسازی داده
                var sanitizedData = this.sanitizeFormData(formData);
                
                // 5. REQUEST PREPARATION - آماده‌سازی درخواست
                var requestData = this.prepareRequestData(patientId, sanitizedData);
                
                // 6. AJAX REQUEST - درخواست AJAX
                var request = this.sendSaveRequest(requestData);
                
                this.currentRequest = request;
                return request;
                
            } catch (error) {
                console.error('[SaveProcessor] ❌ Critical error in save process:', error);
                this.handleCriticalError(error, 'saveInsurance');
                return Promise.reject(error);
            }
        },

        // ========================================
        // UPDATE SAVE BUTTON STATE - به‌روزرسانی وضعیت دکمه ذخیره (Production-Optimized)
        // ========================================
        updateSaveButtonState: function() {
            console.log('[SaveProcessor] 🔄 Updating save button state...');
            
            try {
                var validation = this.validateForm();
                var $saveBtn = $('#saveInsuranceBtn');
                
                console.log('[SaveProcessor] 📊 Save button validation:', {
                    isValid: validation.isValid,
                    hasChanges: validation.hasChanges,
                    canSave: validation.canSave,
                    errors: validation.errors,
                    warnings: validation.warnings
                });
                
                // Check if save button exists
                if ($saveBtn.length === 0) {
                    console.warn('[SaveProcessor] ⚠️ Save button not found');
                    return;
                }
                
                // Determine button state
                var shouldEnable = validation.canSave; // isValid && hasChanges
                
                if (shouldEnable) {
                    // Enable and show save button
                    $saveBtn
                        .prop('disabled', false)
                        .removeClass('d-none btn-secondary')
                        .addClass('btn-success')
                        .html('<i class="fas fa-save"></i> ذخیره اطلاعات بیمه');
                    
                    console.log('[SaveProcessor] ✅ Save button enabled and shown');
                } else {
                    // Disable and hide save button
                    $saveBtn
                        .prop('disabled', true)
                        .addClass('d-none btn-secondary')
                        .removeClass('btn-success')
                        .html('<i class="fas fa-save"></i> ذخیره اطلاعات بیمه');
                    
                    console.log('[SaveProcessor] ❌ Save button disabled and hidden');
                }
                
                // Show validation messages if needed
                if (validation.errors.length > 0) {
                    this.showValidationErrors(validation.errors);
                }
                
                if (validation.warnings.length > 0) {
                    this.showValidationWarnings(validation.warnings);
                }
                
                console.log('[SaveProcessor] ✅ Save button state updated');
                
            } catch (error) {
                console.error('[SaveProcessor] ❌ Error updating save button state:', error);
                this.handleCriticalError(error);
            }
        },

        // ========================================
        // VALIDATE FORM - اعتبارسنجی فرم
        // ========================================
        validateForm: function() {
            console.log('[SaveProcessor] 🔍 Validating form...');
            
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
                console.error('[SaveProcessor] ❌ Error validating form:', error);
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
            console.log('[SaveProcessor] ❌ Validation errors:', errors);
            
            var errorMessage = errors.join(', ');
            
            if (window.ReceptionToastr && window.ReceptionToastr.helpers && window.ReceptionToastr.helpers.showError) {
                window.ReceptionToastr.helpers.showError('خطاهای اعتبارسنجی: ' + errorMessage);
            } else {
                console.error('[SaveProcessor] Validation errors: ' + errorMessage);
            }
        },

        // ========================================
        // SHOW VALIDATION WARNINGS - نمایش هشدارهای اعتبارسنجی
        // ========================================
        showValidationWarnings: function(warnings) {
            console.log('[SaveProcessor] ⚠️ Validation warnings:', warnings);
            
            var warningMessage = warnings.join(', ');
            
            if (window.ReceptionToastr && window.ReceptionToastr.helpers && window.ReceptionToastr.helpers.showWarning) {
                window.ReceptionToastr.helpers.showWarning('هشدارهای اعتبارسنجی: ' + warningMessage);
            } else {
                console.warn('[SaveProcessor] Validation warnings: ' + warningMessage);
            }
        },

        // ========================================
        // SANITIZE FORM DATA - پاکسازی داده‌های فرم
        // ========================================
        sanitizeFormData: function(formData) {
            console.log('[SaveProcessor] 🧹 Sanitizing form data...');
            
            try {
                var sanitized = {};
                
                for (var key in formData) {
                    if (formData.hasOwnProperty(key)) {
                        sanitized[key] = this.sanitizeValue(formData[key]);
                    }
                }
                
                console.log('[SaveProcessor] 📊 Sanitized data:', sanitized);
                return sanitized;
                
            } catch (error) {
                console.error('[SaveProcessor] ❌ Error sanitizing data:', error);
                throw error;
            }
        },

        // ========================================
        // SANITIZE VALUE - پاکسازی مقدار
        // ========================================
        sanitizeValue: function(value) {
            if (!value || value === '') {
                return null;
            }
            
            // Trim whitespace
            value = value.toString().trim();
            
            // Remove potentially dangerous characters
            value = value.replace(/[<>\"'&]/g, '');
            
            return value;
        },

        // ========================================
        // SET SAVE IN PROGRESS STATE - تنظیم وضعیت در حال ذخیره
        // ========================================
        setSaveInProgressState: function() {
            console.log('[SaveProcessor] 🔄 Setting save in progress state...');
            
            this.isSaving = true;
            this._saveStartTime = Date.now();
            
            // UI State Management
            $('#saveInsuranceBtn')
                .prop('disabled', true)
                .addClass('disabled')
                .html('<i class="fas fa-spinner fa-spin"></i> در حال ذخیره...');
            
            console.log('[SaveProcessor] ✅ Save in progress state set');
        },

        // ========================================
        // PERFORMANCE MONITORING - نظارت بر عملکرد
        // ========================================
        startPerformanceMonitoring: function(operation) {
            this._performanceMetrics = this._performanceMetrics || {};
            this._performanceMetrics[operation] = {
                startTime: performance.now(),
                memoryBefore: performance.memory ? performance.memory.usedJSHeapSize : 0
            };
            
            console.log('[SaveProcessor] 📊 Performance monitoring started for:', operation);
        },

        endPerformanceMonitoring: function(operation) {
            if (!this._performanceMetrics || !this._performanceMetrics[operation]) {
                return;
            }
            
            var metrics = this._performanceMetrics[operation];
            var duration = performance.now() - metrics.startTime;
            var memoryAfter = performance.memory ? performance.memory.usedJSHeapSize : 0;
            var memoryDelta = memoryAfter - metrics.memoryBefore;
            
            console.log('[SaveProcessor] 📊 Performance metrics for', operation, ':', {
                duration: duration.toFixed(2) + 'ms',
                memoryDelta: (memoryDelta / 1024 / 1024).toFixed(2) + 'MB',
                timestamp: new Date().toISOString()
            });
            
            // Clean up metrics
            delete this._performanceMetrics[operation];
        },

        // ========================================
        // QUEUE SAVE REQUEST - صف درخواست‌های ذخیره
        // ========================================
        queueSaveRequest: function(patientId, formData) {
            if (!this._saveQueue) {
                this._saveQueue = [];
            }
            
            this._saveQueue.push({
                patientId: patientId,
                formData: formData,
                timestamp: Date.now(),
                retryCount: 0,
                maxRetries: 3
            });
            
            console.log('[SaveProcessor] 📋 Save request queued. Queue length:', this._saveQueue.length);
        },

        // ========================================
        // HANDLE CRITICAL ERROR - مدیریت خطای بحرانی
        // ========================================
        handleCriticalError: function(error, context) {
            console.error('[SaveProcessor] 🚨 Critical error in', context, ':', error);
            
            // Reset all states
            this.isSaving = false;
            this.currentRequest = null;
            
            // Show user-friendly error
            this.showError('خطای بحرانی در سیستم. لطفاً صفحه را بازخوانی کنید.');
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
        // SEND SAVE REQUEST - ارسال درخواست ذخیره (Production-Optimized)
        // ========================================
        sendSaveRequest: function(requestData) {
            console.log('[SaveProcessor] 🚀 Executing save request...');
            
            try {
                var self = this;
                var requestId = 'req_' + Date.now() + '_' + Math.random().toString(36).substr(2, 5);
                
                return $.ajax({
                    url: this.config.endpoints.save,
                    type: 'POST',
                    data: requestData,
                    timeout: this.config.timeout,
                    cache: false,
                    beforeSend: function(xhr) {
                        xhr.setRequestHeader('X-Request-ID', requestId);
                        xhr.setRequestHeader('X-Request-Time', Date.now().toString());
                    },
                    success: function(response, textStatus, xhr) {
                        self.endPerformanceMonitoring('save');
                        self.handleSaveSuccess(response, requestId);
                    },
                    error: function(xhr, status, error) {
                        self.endPerformanceMonitoring('save');
                        self.handleSaveError(xhr, status, error, requestId);
                    },
                    complete: function(xhr, status) {
                        console.log('[SaveProcessor] Save request completed');
                        self.isSaving = false;
                        self.currentRequest = null;
                    }
                });
            } catch (error) {
                console.error('[SaveProcessor] ❌ Error sending save request:', error);
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
