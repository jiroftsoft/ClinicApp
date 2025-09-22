/**
 * Medical UX Enhancer Module
 * ماژول بهبود تجربه کاربری محیط درمانی
 * 
 * @author ClinicApp Medical Team
 * @version 1.0.0
 * @description Enhanced user experience for medical environment
 */

(function() {
    'use strict';
    
    // Global MedicalUXEnhancer object
    window.MedicalUXEnhancer = {
        
        // Configuration
        config: {
            loadingDelay: 200, // 200ms delay before showing loading
            animationDuration: 300,
            autoSaveInterval: 30000, // 30 seconds
            formTimeout: 300000, // 5 minutes
            keyboardShortcuts: true
        },
        
        // State management
        state: {
            isLoading: false,
            autoSaveEnabled: false,
            formData: {},
            lastSaved: null
        },
        
        // Loading states
        loadingStates: new Map(),
        
        /**
         * Initialize UX Enhancer
         * راه‌اندازی بهبوددهنده UX
         */
        init: function() {
            console.log(window.MedicalConfig.logPrefix + ' MedicalUXEnhancer initialized');
            this.setupLoadingStates();
            this.setupFormEnhancements();
            this.setupKeyboardShortcuts();
            this.setupAutoSave();
            this.setupProgressIndicators();
            return true;
        },
        
        /**
         * Setup loading states for better UX
         * تنظیم وضعیت‌های بارگذاری برای UX بهتر
         */
        setupLoadingStates: function() {
            const self = this;
            
            // Form submission loading
            $(document).on('submit', '.medical-form', function(e) {
                const $form = $(this);
                const $submitBtn = $form.find('button[type="submit"], input[type="submit"]');
                
                if ($submitBtn.length) {
                    self.showButtonLoading($submitBtn);
                }
            });
            
            // AJAX loading states
            $(document).ajaxStart(function() {
                self.showGlobalLoading();
            });
            
            $(document).ajaxStop(function() {
                self.hideGlobalLoading();
            });
            
            // Button click loading
            $(document).on('click', '.btn-loading', function() {
                const $btn = $(this);
                self.showButtonLoading($btn);
            });
        },
        
        /**
         * Setup form enhancements
         * تنظیم بهبودهای فرم
         */
        setupFormEnhancements: function() {
            const self = this;
            
            // Auto-focus first input
            $('.medical-form').each(function() {
                const $form = $(this);
                const $firstInput = $form.find('input:not([type="hidden"]):not([readonly]):not([disabled]), select:not([disabled]), textarea:not([readonly]):not([disabled])').first();
                
                if ($firstInput.length) {
                    setTimeout(function() {
                        $firstInput.focus();
                    }, 100);
                }
            });
            
            // Form validation feedback
            $('.medical-form input, .medical-form select, .medical-form textarea').on('blur', function() {
                const $field = $(this);
                self.validateField($field);
            });
            
            // Form data tracking for auto-save
            $('.medical-form input, .medical-form select, .medical-form textarea').on('input change', function() {
                const $form = $(this).closest('.medical-form');
                if ($form.length) {
                    self.trackFormData($form);
                }
            });
        },
        
        /**
         * Setup keyboard shortcuts
         * تنظیم میانبرهای صفحه‌کلید
         */
        setupKeyboardShortcuts: function() {
            if (!this.config.keyboardShortcuts) return;
            
            const self = this;
            
            $(document).on('keydown', function(e) {
                // Ctrl+S for save
                if (e.ctrlKey && e.key === 's') {
                    e.preventDefault();
                    const $form = $('.medical-form:visible').first();
                    if ($form.length) {
                        $form.submit();
                    }
                }
                
                // Escape to close modals/messages
                if (e.key === 'Escape') {
                    $('.modal').modal('hide');
                    if (typeof window.MedicalMessageManager !== 'undefined') {
                        window.MedicalMessageManager.clearAllMessages();
                    }
                }
                
                // Enter in forms (except textarea)
                if (e.key === 'Enter' && !$(e.target).is('textarea')) {
                    const $form = $(e.target).closest('.medical-form');
                    if ($form.length && $form.find('button[type="submit"]').length) {
                        e.preventDefault();
                        $form.submit();
                    }
                }
            });
        },
        
        /**
         * Setup auto-save functionality
         * تنظیم قابلیت ذخیره خودکار
         */
        setupAutoSave: function() {
            const self = this;
            
            setInterval(function() {
                if (self.state.autoSaveEnabled && Object.keys(self.state.formData).length > 0) {
                    self.performAutoSave();
                }
            }, this.config.autoSaveInterval);
        },
        
        /**
         * Setup progress indicators
         * تنظیم نشانگرهای پیشرفت
         */
        setupProgressIndicators: function() {
            const self = this;
            
            // Form progress
            $('.medical-form').each(function() {
                const $form = $(this);
                const $fields = $form.find('input:not([type="hidden"]), select, textarea');
                const totalFields = $fields.length;
                
                if (totalFields > 0) {
                    self.createFormProgress($form, totalFields);
                }
            });
        },
        
        /**
         * Show button loading state
         * نمایش وضعیت بارگذاری دکمه
         */
        showButtonLoading: function($btn) {
            const originalText = $btn.text();
            const originalHtml = $btn.html();
            
            $btn.data('original-text', originalText);
            $btn.data('original-html', originalHtml);
            
            $btn.prop('disabled', true);
            $btn.html('<i class="fas fa-spinner fa-spin"></i> در حال پردازش...');
            
            // Auto-disable after timeout
            setTimeout(function() {
                self.hideButtonLoading($btn);
            }, this.config.formTimeout);
        },
        
        /**
         * Hide button loading state
         * مخفی کردن وضعیت بارگذاری دکمه
         */
        hideButtonLoading: function($btn) {
            const originalHtml = $btn.data('original-html');
            
            if (originalHtml) {
                $btn.html(originalHtml);
                $btn.prop('disabled', false);
            }
        },
        
        /**
         * Show global loading
         * نمایش بارگذاری عمومی
         */
        showGlobalLoading: function() {
            if (this.state.isLoading) return;
            
            this.state.isLoading = true;
            
            if ($('#global-loading-overlay').length === 0) {
                $('body').append(`
                    <div id="global-loading-overlay" class="global-loading-overlay">
                        <div class="global-loading-content">
                            <div class="global-loading-spinner">
                                <i class="fas fa-spinner fa-spin"></i>
                            </div>
                            <div class="global-loading-text">در حال پردازش...</div>
                        </div>
                    </div>
                `);
            }
            
            $('#global-loading-overlay').fadeIn(this.config.animationDuration);
        },
        
        /**
         * Hide global loading
         * مخفی کردن بارگذاری عمومی
         */
        hideGlobalLoading: function() {
            this.state.isLoading = false;
            $('#global-loading-overlay').fadeOut(this.config.animationDuration);
        },
        
        /**
         * Validate field
         * اعتبارسنجی فیلد
         */
        validateField: function($field) {
            const value = $field.val();
            const isRequired = $field.prop('required');
            const fieldName = $field.attr('name') || $field.attr('id');
            
            // Clear previous validation
            $field.removeClass('is-valid is-invalid');
            $field.siblings('.invalid-feedback, .valid-feedback').remove();
            
            // Required field validation
            if (isRequired && (!value || value.trim() === '')) {
                $field.addClass('is-invalid');
                $field.after('<div class="invalid-feedback">این فیلد الزامی است</div>');
                return false;
            }
            
            // Valid field
            if (value && value.trim() !== '') {
                $field.addClass('is-valid');
                $field.after('<div class="valid-feedback">✓ معتبر</div>');
            }
            
            return true;
        },
        
        /**
         * Track form data for auto-save
         * ردیابی داده‌های فرم برای ذخیره خودکار
         */
        trackFormData: function($form) {
            const formId = $form.attr('id') || 'form_' + Date.now();
            const formData = {};
            
            $form.find('input, select, textarea').each(function() {
                const $field = $(this);
                const name = $field.attr('name');
                const value = $field.val();
                
                if (name && value) {
                    formData[name] = value;
                }
            });
            
            this.state.formData[formId] = formData;
            this.state.lastSaved = Date.now();
        },
        
        /**
         * Perform auto-save
         * انجام ذخیره خودکار
         */
        performAutoSave: function() {
            // This would typically send data to server
            // For now, we'll just show a subtle indicator
            if (typeof window.MedicalMessageManager !== 'undefined') {
                window.MedicalMessageManager.info('داده‌ها به صورت خودکار ذخیره شدند', 2000);
            }
        },
        
        /**
         * Create form progress indicator
         * ایجاد نشانگر پیشرفت فرم
         */
        createFormProgress: function($form, totalFields) {
            const progressHtml = `
                <div class="form-progress-container">
                    <div class="form-progress-bar">
                        <div class="form-progress-fill" style="width: 0%"></div>
                    </div>
                    <div class="form-progress-text">0 از ${totalFields} فیلد تکمیل شده</div>
                </div>
            `;
            
            $form.prepend(progressHtml);
            
            // Update progress on field changes
            const self = this;
            $form.find('input, select, textarea').on('input change', function() {
                self.updateFormProgress($form, totalFields);
            });
        },
        
        /**
         * Update form progress
         * به‌روزرسانی پیشرفت فرم
         */
        updateFormProgress: function($form, totalFields) {
            const $fields = $form.find('input:not([type="hidden"]), select, textarea');
            let completedFields = 0;
            
            $fields.each(function() {
                const $field = $(this);
                const value = $field.val();
                
                if (value && value.trim() !== '') {
                    completedFields++;
                }
            });
            
            const percentage = Math.round((completedFields / totalFields) * 100);
            const $progressFill = $form.find('.form-progress-fill');
            const $progressText = $form.find('.form-progress-text');
            
            $progressFill.css('width', percentage + '%');
            $progressText.text(`${completedFields} از ${totalFields} فیلد تکمیل شده`);
            
            // Add completion class
            if (percentage === 100) {
                $form.addClass('form-completed');
            } else {
                $form.removeClass('form-completed');
            }
        },
        
        /**
         * Enable auto-save
         * فعال کردن ذخیره خودکار
         */
        enableAutoSave: function() {
            this.state.autoSaveEnabled = true;
            if (typeof window.MedicalMessageManager !== 'undefined') {
                window.MedicalMessageManager.info('ذخیره خودکار فعال شد', 2000);
            }
        },
        
        /**
         * Disable auto-save
         * غیرفعال کردن ذخیره خودکار
         */
        disableAutoSave: function() {
            this.state.autoSaveEnabled = false;
            if (typeof window.MedicalMessageManager !== 'undefined') {
                window.MedicalMessageManager.info('ذخیره خودکار غیرفعال شد', 2000);
            }
        },
        
        /**
         * Show form completion celebration
         * نمایش جشن تکمیل فرم
         */
        showFormCompletion: function() {
            if (typeof window.MedicalMessageManager !== 'undefined') {
                window.MedicalMessageManager.success('🎉 فرم با موفقیت تکمیل شد!', 3000);
            }
        }
    };
    
    // Auto-initialize when DOM is ready
    $(document).ready(function() {
        if (window.MedicalConfig && window.MedicalConfig.enableUXEnhancer !== false) {
            window.MedicalUXEnhancer.init();
        }
    });
    
})();
