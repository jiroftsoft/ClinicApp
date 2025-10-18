/**
 * RECEPTION ERROR HANDLER - سیستم مدیریت خطا
 * ========================================
 * 
 * این سیستم مسئولیت‌های زیر را دارد:
 * - مدیریت خطاهای سراسری
 * - دسته‌بندی خطاها
 * - نمایش پیام‌های کاربرپسند
 * - ارسال خطاها به سرور
 * - Performance monitoring
 * 
 * @author ClinicApp Development Team
 * @version 1.0.0
 * @since 2025-01-17
 */

(function(global) {
    'use strict';

    // ========================================
    // ERROR HANDLER CONFIGURATION - تنظیمات Error Handler
    // ========================================
    var CONFIG = {
        settings: {
            enableServerLogging: true,
            enableUserNotifications: true,
            enablePerformanceMonitoring: true,
            maxRetries: 3,
            retryDelay: 1000
        },
        severity: {
            LOW: 'low',
            MEDIUM: 'medium',
            HIGH: 'high',
            CRITICAL: 'critical'
        },
        categories: {
            VALIDATION: 'validation',
            NETWORK: 'network',
            BUSINESS: 'business',
            SYSTEM: 'system'
        },
        messages: {
            validation: {
                nationalCodeInvalid: 'کد ملی نامعتبر است',
                requiredField: 'این فیلد الزامی است',
                invalidFormat: 'فرمت ورودی نامعتبر است'
            },
            network: {
                connectionError: 'خطا در اتصال به سرور',
                timeoutError: 'زمان اتصال به سرور تمام شد',
                serverError: 'خطا در سرور'
            },
            business: {
                patientNotFound: 'بیمار یافت نشد',
                duplicateEntry: 'رکورد تکراری است',
                insufficientData: 'اطلاعات ناکافی است'
            },
            system: {
                unexpectedError: 'خطای غیرمنتظره رخ داد',
                configurationError: 'خطا در تنظیمات سیستم',
                databaseError: 'خطا در پایگاه داده'
            }
        }
    };

    // ========================================
    // ERROR HANDLER CLASS - کلاس Error Handler
    // ========================================
    function ErrorHandler() {
        this.isInitialized = false;
        this.errorHistory = [];
        this.retryCounts = {};
        this.performanceObserver = null;
    }

    // ========================================
    // INITIALIZATION - مقداردهی اولیه
    // ========================================
    ErrorHandler.prototype.init = function() {
        if (this.isInitialized) {
            console.warn('ErrorHandler already initialized');
            return;
        }

        try {
            this.setupGlobalErrorHandlers();
            this.setupUnhandledRejectionHandler();
            this.setupPerformanceObserver();
            this.isInitialized = true;
            console.log('ErrorHandler initialized successfully');
        } catch (error) {
            console.error('Failed to initialize ErrorHandler:', error);
        }
    };

    // ========================================
    // GLOBAL ERROR HANDLERS SETUP - تنظیم handlers سراسری
    // ========================================
    ErrorHandler.prototype.setupGlobalErrorHandlers = function() {
        var self = this;
        
        // Global error handler
        window.addEventListener('error', function(event) {
            self.handle({
                message: event.message,
                filename: event.filename,
                lineno: event.lineno,
                colno: event.colno,
                error: event.error
            }, 'global', { source: 'window.error' });
        });

        // Unhandled promise rejection handler
        window.addEventListener('unhandledrejection', function(event) {
            self.handle({
                message: event.reason ? event.reason.message : 'Unhandled promise rejection',
                error: event.reason
            }, 'promise', { source: 'unhandledrejection' });
        });
    };

    // ========================================
    // UNHANDLED REJECTION HANDLER - مدیریت rejection های مدیریت نشده
    // ========================================
    ErrorHandler.prototype.setupUnhandledRejectionHandler = function() {
        var self = this;
        
        if (typeof Promise !== 'undefined' && Promise.reject) {
            var originalReject = Promise.reject;
            Promise.reject = function(reason) {
                self.handle({
                    message: reason ? reason.message : 'Promise rejected',
                    error: reason
                }, 'promise', { source: 'Promise.reject' });
                return originalReject.call(this, reason);
            };
        }
    };

    // ========================================
    // PERFORMANCE OBSERVER SETUP - تنظیم Performance Observer
    // ========================================
    ErrorHandler.prototype.setupPerformanceObserver = function() {
        if (!CONFIG.settings.enablePerformanceMonitoring) return;
        
        var self = this;
        
        if (typeof PerformanceObserver !== 'undefined') {
            this.performanceObserver = new PerformanceObserver(function(list) {
                var entries = list.getEntries();
                for (var i = 0; i < entries.length; i++) {
                    var entry = entries[i];
                    if (entry.entryType === 'measure' && entry.duration > 5000) {
                        self.handle({
                            message: 'Performance issue detected',
                            duration: entry.duration,
                            name: entry.name
                        }, 'performance', { source: 'PerformanceObserver' });
                    }
                }
            });
            
            this.performanceObserver.observe({ entryTypes: ['measure'] });
        }
    };

    // ========================================
    // ERROR HANDLING - مدیریت خطا
    // ========================================
    ErrorHandler.prototype.handle = function(error, context, options) {
        context = context || 'unknown';
        options = options || {};
        
        if (!this.isInitialized) {
            console.error('ErrorHandler not initialized');
            return;
        }

        try {
            var errorObject = this.createErrorObject(error, context, options);
            var category = this.categorizeError(error);
            var severity = this.determineSeverity(error);

            this.recordError(errorObject);
            this.handleBySeverity(errorObject, severity);

            if (CONFIG.settings.enableServerLogging) {
                this.sendToServer(errorObject, options);
            }

            if (CONFIG.settings.enableUserNotifications) {
                this.notifyUser(errorObject, severity);
            }
        } catch (handlingError) {
            console.error('Error in error handler:', handlingError);
        }
    };

    // ========================================
    // ERROR OBJECT CREATION - ایجاد شیء خطا
    // ========================================
    ErrorHandler.prototype.createErrorObject = function(error, context, options) {
        return {
            id: this.generateErrorId(),
            message: this.extractMessage(error),
            stack: this.extractStack(error),
            context: context,
            timestamp: Date.now(),
            userAgent: navigator.userAgent,
            url: window.location.href,
            userId: this.getCurrentUserId(),
            sessionId: this.getSessionId(),
            retryCount: this.getRetryCount(context),
            options: options
        };
    };

    // ========================================
    // ERROR CATEGORIZATION - دسته‌بندی خطا
    // ========================================
    ErrorHandler.prototype.categorizeError = function(error) {
        var message = this.extractMessage(error).toLowerCase();
        
        if (message.includes('validation') || message.includes('invalid') || message.includes('required')) {
            return CONFIG.categories.VALIDATION;
        }
        
        if (message.includes('network') || message.includes('fetch') || message.includes('ajax') || message.includes('timeout')) {
            return CONFIG.categories.NETWORK;
        }
        
        if (message.includes('business') || message.includes('patient') || message.includes('reception')) {
            return CONFIG.categories.BUSINESS;
        }
        
        return CONFIG.categories.SYSTEM;
    };

    // ========================================
    // SEVERITY DETERMINATION - تعیین شدت خطا
    // ========================================
    ErrorHandler.prototype.determineSeverity = function(error) {
        var message = this.extractMessage(error).toLowerCase();
        
        if (message.includes('critical') || message.includes('fatal') || message.includes('crash')) {
            return CONFIG.severity.CRITICAL;
        }
        
        if (message.includes('error') || message.includes('failed') || message.includes('exception')) {
            return CONFIG.severity.HIGH;
        }
        
        if (message.includes('warning') || message.includes('deprecated')) {
            return CONFIG.severity.MEDIUM;
        }
        
        return CONFIG.severity.LOW;
    };

    // ========================================
    // SEVERITY-BASED HANDLING - مدیریت بر اساس شدت
    // ========================================
    ErrorHandler.prototype.handleBySeverity = function(errorObject, severity) {
        switch (severity) {
            case CONFIG.severity.CRITICAL:
                this.handleCriticalError(errorObject);
                break;
            case CONFIG.severity.HIGH:
                this.handleHighSeverityError(errorObject);
                break;
            case CONFIG.severity.MEDIUM:
                this.handleMediumSeverityError(errorObject);
                break;
            case CONFIG.severity.LOW:
                this.handleLowSeverityError(errorObject);
                break;
        }
    };

    // ========================================
    // CRITICAL ERROR HANDLING - مدیریت خطاهای بحرانی
    // ========================================
    ErrorHandler.prototype.handleCriticalError = function(errorObject) {
        console.error('CRITICAL ERROR:', errorObject);
        this.showCriticalErrorModal(errorObject);
    };

    // ========================================
    // HIGH SEVERITY ERROR HANDLING - مدیریت خطاهای با شدت بالا
    // ========================================
    ErrorHandler.prototype.handleHighSeverityError = function(errorObject) {
        console.error('HIGH SEVERITY ERROR:', errorObject);
        this.showErrorToast(errorObject, 'خطای مهم رخ داد');
    };

    // ========================================
    // MEDIUM SEVERITY ERROR HANDLING - مدیریت خطاهای با شدت متوسط
    // ========================================
    ErrorHandler.prototype.handleMediumSeverityError = function(errorObject) {
        console.warn('MEDIUM SEVERITY ERROR:', errorObject);
        setTimeout(function() {
            console.log('Medium severity error handled');
        }, 1000);
    };

    // ========================================
    // LOW SEVERITY ERROR HANDLING - مدیریت خطاهای با شدت پایین
    // ========================================
    ErrorHandler.prototype.handleLowSeverityError = function(errorObject) {
        console.info('LOW SEVERITY ERROR:', errorObject);
    };

    // ========================================
    // USER NOTIFICATION - اطلاع‌رسانی به کاربر
    // ========================================
    ErrorHandler.prototype.notifyUser = function(errorObject, severity) {
        if (severity === CONFIG.severity.CRITICAL) {
            this.showCriticalErrorModal(errorObject);
        } else if (severity === CONFIG.severity.HIGH) {
            this.showErrorToast(errorObject, this.getUserFriendlyMessage(errorObject));
        }
    };

    // ========================================
    // ERROR TOAST DISPLAY - نمایش toast خطا
    // ========================================
    ErrorHandler.prototype.showErrorToast = function(errorObject, message) {
        if (window.ReceptionToastr) {
            window.ReceptionToastr.service.error(message, 'خطا', {
                timeOut: 5000,
                closeButton: true
            });
        } else {
            console.error('Toastr not available:', message);
        }
    };

    // ========================================
    // CRITICAL ERROR MODAL - نمایش modal خطای بحرانی
    // ========================================
    ErrorHandler.prototype.showCriticalErrorModal = function(errorObject) {
        if (window.Swal) {
            window.Swal.fire({
                title: 'خطای بحرانی',
                text: 'خطای مهمی رخ داده است. لطفاً صفحه را refresh کنید.',
                icon: 'error',
                confirmButtonText: 'تأیید'
            });
        }
    };

    // ========================================
    // SERVER LOGGING - ارسال به سرور
    // ========================================
    ErrorHandler.prototype.sendToServer = function(errorObject, options) {
        options = options || {};
        
        if (typeof fetch !== 'undefined') {
            fetch('/api/errors/log', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(errorObject)
            }).catch(function(error) {
                console.error('Failed to send error to server:', error);
            });
        }
    };

    // ========================================
    // UTILITY METHODS - متدهای کمکی
    // ========================================
    ErrorHandler.prototype.extractMessage = function(error) {
        if (typeof error === 'string') return error;
        if (error && error.message) return error.message;
        return 'Unknown error';
    };

    ErrorHandler.prototype.extractStack = function(error) {
        if (error && error.stack) return error.stack;
        return '';
    };

    ErrorHandler.prototype.generateErrorId = function() {
        return 'error_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    };

    ErrorHandler.prototype.getCurrentUserId = function() {
        return window.currentUserId || 'anonymous';
    };

    ErrorHandler.prototype.getSessionId = function() {
        return window.sessionId || 'unknown';
    };

    ErrorHandler.prototype.getRetryCount = function(context) {
        return this.retryCounts[context] || 0;
    };

    ErrorHandler.prototype.getUserFriendlyMessage = function(errorObject) {
        var category = this.categorizeError(errorObject);
        var messages = CONFIG.messages[category];
        return messages[Object.keys(messages)[0]] || 'خطای غیرمنتظره رخ داد';
    };

    // ========================================
    // ERROR HISTORY MANAGEMENT - مدیریت تاریخچه خطاها
    // ========================================
    ErrorHandler.prototype.recordError = function(errorObject) {
        this.errorHistory.push(errorObject);
        
        if (this.errorHistory.length > 100) {
            this.errorHistory.shift();
        }
    };

    ErrorHandler.prototype.getErrorHistory = function() {
        return this.errorHistory;
    };

    ErrorHandler.prototype.getErrorStats = function() {
        var stats = {
            total: this.errorHistory.length,
            byCategory: {},
            bySeverity: {}
        };

        for (var i = 0; i < this.errorHistory.length; i++) {
            var error = this.errorHistory[i];
            var category = this.categorizeError(error);
            var severity = this.determineSeverity(error);
            
            stats.byCategory[category] = (stats.byCategory[category] || 0) + 1;
            stats.bySeverity[severity] = (stats.bySeverity[severity] || 0) + 1;
        }

        return stats;
    };

    // ========================================
    // CLEANUP - پاکسازی
    // ========================================
    ErrorHandler.prototype.destroy = function() {
        if (this.performanceObserver) {
            this.performanceObserver.disconnect();
            this.performanceObserver = null;
        }
        
        this.errorHistory = [];
        this.retryCounts = {};
        this.isInitialized = false;
    };

    // ========================================
    // EXPORT - صادرات
    // ========================================
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = ErrorHandler;
    } else if (typeof global !== 'undefined') {
        // Create a singleton instance
        global.ReceptionErrorHandler = new ErrorHandler();
    }

})(typeof window !== 'undefined' ? window : this);