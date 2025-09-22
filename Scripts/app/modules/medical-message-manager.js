/**
 * Medical Message Manager Module
 * ماژول مدیریت پیام‌های یکپارچه محیط درمانی
 * 
 * @author ClinicApp Medical Team
 * @version 1.0.0
 * @description Unified message management for medical environment
 */

(function() {
    'use strict';
    
    // Global MedicalMessageManager object
    window.MedicalMessageManager = {
        
        // Configuration
        config: {
            defaultDuration: 5000, // 5 seconds
            animationDuration: 300,
            maxMessages: 5,
            position: 'top-right',
            zIndex: 9999
        },
        
        // Message types
        messageTypes: {
            SUCCESS: 'success',
            ERROR: 'error',
            WARNING: 'warning',
            INFO: 'info',
            LOADING: 'loading'
        },
        
        // Message queue
        messageQueue: [],
        
        // Current messages
        currentMessages: [],
        
        /**
         * Initialize Message Manager
         * راه‌اندازی مدیر پیام‌ها
         */
        init: function() {
            console.log(window.MedicalConfig.logPrefix + ' MedicalMessageManager initialized');
            this.createMessageContainer();
            this.setupGlobalHandlers();
            return true;
        },
        
        /**
         * Create message container
         * ایجاد کانتینر پیام‌ها
         */
        createMessageContainer: function() {
            if ($('#medical-message-container').length === 0) {
                $('body').append(`
                    <div id="medical-message-container" class="medical-message-container">
                        <div class="medical-messages-wrapper"></div>
                    </div>
                `);
            }
        },
        
        /**
         * Setup global message handlers
         * تنظیم handler های جهانی پیام‌ها
         */
        setupGlobalHandlers: function() {
            const self = this;
            
            // Handle AJAX success/error messages
            $(document).ajaxSuccess(function(event, xhr, settings) {
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    self.showMessage(xhr.responseJSON.message, self.messageTypes.SUCCESS);
                }
            });
            
            $(document).ajaxError(function(event, xhr, settings) {
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    self.showMessage(xhr.responseJSON.message, self.messageTypes.ERROR);
                } else if (xhr.status === 500) {
                    self.showMessage('خطا در سرور. لطفاً دوباره تلاش کنید.', self.messageTypes.ERROR);
                } else if (xhr.status === 404) {
                    self.showMessage('منبع مورد نظر یافت نشد.', self.messageTypes.ERROR);
                }
            });
            
            // Handle form submission messages
            $(document).on('form-submit-success', function(e, data) {
                if (data.message) {
                    self.showMessage(data.message, self.messageTypes.SUCCESS);
                }
            });
            
            $(document).on('form-submit-error', function(e, data) {
                if (data.message) {
                    self.showMessage(data.message, self.messageTypes.ERROR);
                }
            });
        },
        
        /**
         * Show message
         * نمایش پیام
         */
        showMessage: function(message, type, duration, options) {
            const self = this;
            const messageId = 'msg_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
            
            // Default options
            const defaultOptions = {
                closable: true,
                autoClose: true,
                showIcon: true,
                showProgress: true,
                position: this.config.position,
                animation: 'slideInRight'
            };
            
            const finalOptions = Object.assign({}, defaultOptions, options || {});
            const finalDuration = duration || this.config.defaultDuration;
            
            // Create message element
            const messageElement = this.createMessageElement(messageId, message, type, finalOptions);
            
            // Add to container
            this.addMessageToContainer(messageElement);
            
            // Auto close if enabled
            if (finalOptions.autoClose && finalDuration > 0) {
                setTimeout(function() {
                    self.hideMessage(messageId);
                }, finalDuration);
            }
            
            // Add to current messages
            this.currentMessages.push({
                id: messageId,
                element: messageElement,
                type: type,
                timestamp: Date.now()
            });
            
            // Limit number of messages
            if (this.currentMessages.length > this.config.maxMessages) {
                const oldestMessage = this.currentMessages.shift();
                this.hideMessage(oldestMessage.id);
            }
            
            return messageId;
        },
        
        /**
         * Create message element
         * ایجاد عنصر پیام
         */
        createMessageElement: function(id, message, type, options) {
            const iconClass = this.getIconClass(type);
            const typeClass = `medical-message-${type}`;
            
            const messageHtml = `
                <div id="${id}" class="medical-message ${typeClass}" data-type="${type}">
                    <div class="medical-message-content">
                        ${options.showIcon ? `<div class="medical-message-icon"><i class="${iconClass}"></i></div>` : ''}
                        <div class="medical-message-text">${message}</div>
                        ${options.closable ? '<div class="medical-message-close"><i class="fas fa-times"></i></div>' : ''}
                    </div>
                    ${options.showProgress ? '<div class="medical-message-progress"></div>' : ''}
                </div>
            `;
            
            return $(messageHtml);
        },
        
        /**
         * Get icon class for message type
         * دریافت کلاس آیکون برای نوع پیام
         */
        getIconClass: function(type) {
            const iconMap = {
                [this.messageTypes.SUCCESS]: 'fas fa-check-circle',
                [this.messageTypes.ERROR]: 'fas fa-exclamation-circle',
                [this.messageTypes.WARNING]: 'fas fa-exclamation-triangle',
                [this.messageTypes.INFO]: 'fas fa-info-circle',
                [this.messageTypes.LOADING]: 'fas fa-spinner fa-spin'
            };
            
            return iconMap[type] || 'fas fa-info-circle';
        },
        
        /**
         * Add message to container
         * اضافه کردن پیام به کانتینر
         */
        addMessageToContainer: function(messageElement) {
            const $container = $('#medical-message-container .medical-messages-wrapper');
            
            // Add animation class
            messageElement.addClass('medical-message-enter');
            
            // Append to container
            $container.append(messageElement);
            
            // Trigger animation
            setTimeout(function() {
                messageElement.removeClass('medical-message-enter').addClass('medical-message-show');
            }, 10);
            
            // Setup close handler
            messageElement.find('.medical-message-close').on('click', function() {
                const messageId = messageElement.attr('id');
                window.MedicalMessageManager.hideMessage(messageId);
            });
        },
        
        /**
         * Hide message
         * مخفی کردن پیام
         */
        hideMessage: function(messageId) {
            const messageIndex = this.currentMessages.findIndex(msg => msg.id === messageId);
            if (messageIndex === -1) return;
            
            const message = this.currentMessages[messageIndex];
            const $messageElement = $(`#${messageId}`);
            
            if ($messageElement.length) {
                // Add exit animation
                $messageElement.removeClass('medical-message-show').addClass('medical-message-exit');
                
                // Remove after animation
                setTimeout(function() {
                    $messageElement.remove();
                }, this.config.animationDuration);
            }
            
            // Remove from current messages
            this.currentMessages.splice(messageIndex, 1);
        },
        
        /**
         * Clear all messages
         * پاک کردن همه پیام‌ها
         */
        clearAllMessages: function() {
            const self = this;
            this.currentMessages.forEach(function(message) {
                self.hideMessage(message.id);
            });
        },
        
        /**
         * Show success message
         * نمایش پیام موفقیت
         */
        success: function(message, duration, options) {
            return this.showMessage(message, this.messageTypes.SUCCESS, duration, options);
        },
        
        /**
         * Show error message
         * نمایش پیام خطا
         */
        error: function(message, duration, options) {
            return this.showMessage(message, this.messageTypes.ERROR, duration, options);
        },
        
        /**
         * Show warning message
         * نمایش پیام هشدار
         */
        warning: function(message, duration, options) {
            return this.showMessage(message, this.messageTypes.WARNING, duration, options);
        },
        
        /**
         * Show info message
         * نمایش پیام اطلاعات
         */
        info: function(message, duration, options) {
            return this.showMessage(message, this.messageTypes.INFO, duration, options);
        },
        
        /**
         * Show loading message
         * نمایش پیام بارگذاری
         */
        loading: function(message, options) {
            return this.showMessage(message, this.messageTypes.LOADING, 0, Object.assign({}, options, { autoClose: false }));
        },
        
        /**
         * Hide loading message
         * مخفی کردن پیام بارگذاری
         */
        hideLoading: function(messageId) {
            this.hideMessage(messageId);
        },
        
        /**
         * Show form validation errors
         * نمایش خطاهای اعتبارسنجی فرم
         */
        showValidationErrors: function(errors) {
            if (Array.isArray(errors)) {
                const errorList = errors.map(error => `<li>${error}</li>`).join('');
                this.error(`<ul class="mb-0">${errorList}</ul>`, 8000, { showProgress: false });
            } else if (typeof errors === 'string') {
                this.error(errors, 5000);
            }
        },
        
        /**
         * Show form success
         * نمایش موفقیت فرم
         */
        showFormSuccess: function(message) {
            this.success(message || 'عملیات با موفقیت انجام شد.', 3000);
        },
        
        /**
         * Show form error
         * نمایش خطای فرم
         */
        showFormError: function(message) {
            this.error(message || 'خطا در انجام عملیات. لطفاً دوباره تلاش کنید.', 5000);
        },
        
        /**
         * Show network error
         * نمایش خطای شبکه
         */
        showNetworkError: function() {
            this.error('خطا در ارتباط با سرور. لطفاً اتصال اینترنت خود را بررسی کنید.', 5000);
        },
        
        /**
         * Show permission error
         * نمایش خطای دسترسی
         */
        showPermissionError: function() {
            this.error('شما دسترسی لازم برای انجام این عملیات را ندارید.', 5000);
        },
        
        /**
         * Show session timeout
         * نمایش انقضای جلسه
         */
        showSessionTimeout: function() {
            this.warning('جلسه شما منقضی شده است. لطفاً مجدداً وارد شوید.', 8000, { 
                autoClose: false,
                closable: false 
            });
        }
    };
    
    // Auto-initialize when DOM is ready
    $(document).ready(function() {
        if (window.MedicalConfig && window.MedicalConfig.enableMessageManager !== false) {
            window.MedicalMessageManager.init();
        }
    });
    
})();
