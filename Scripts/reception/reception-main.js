/**
 * RECEPTION MAIN MODULE - ماژول اصلی پذیرش
 * ========================================
 * 
 * این ماژول مسئولیت‌های زیر را دارد:
 * - هماهنگی بین تمام ماژول‌ها
 * - مدیریت lifecycle کلی
 * - Performance monitoring
 * - Error handling مرکزی
 * 
 * @author ClinicApp Development Team
 * @version 1.0.0
 * @since 2025-01-17
 */

(function(global, $) {
    'use strict';

    // ========================================
    // MAIN MODULE CONFIGURATION - تنظیمات ماژول اصلی
    // ========================================
    var CONFIG = {
        // Module Dependencies
        dependencies: {
            'ReceptionEventBus': [],
            'ReceptionErrorHandler': ['ReceptionEventBus'],
            'PatientSearchModule': ['ReceptionEventBus', 'ReceptionErrorHandler'],
            'PatientInsuranceModule': ['ReceptionEventBus', 'ReceptionErrorHandler', 'PatientSearchModule'],
            'RealTimeInsuranceBinding': ['ReceptionEventBus', 'ReceptionErrorHandler', 'PatientSearchModule', 'PatientInsuranceModule'],
            'DepartmentSelectionModule': ['ReceptionEventBus', 'ReceptionErrorHandler'],
            'ServiceCalculationModule': ['ReceptionEventBus', 'ReceptionErrorHandler'],
            'PaymentProcessingModule': ['ReceptionEventBus', 'ReceptionErrorHandler']
        },
        
        // Module Load Order
        loadOrder: [
            'ReceptionEventBus',
            'ReceptionErrorHandler',
            'PatientSearchModule',
            'PatientInsuranceModule',
            'RealTimeInsuranceBinding',
            'DepartmentSelectionModule',
            'ServiceCalculationModule',
            'PaymentProcessingModule'
        ],
        
        // Performance Settings
        performance: {
            enableMonitoring: true,
            maxInitTime: 10000,
            enableMetrics: true,
            memoryThreshold: 0.8
        },
        
        // Error Handling
        errorHandling: {
            maxRetries: 3,
            retryDelay: 1000,
            enableFallback: true
        }
    };

    // ========================================
    // RECEPTION MAIN MODULE - ماژول اصلی
    // ========================================
    var ReceptionMainModule = {
        
        // ========================================
        // INITIALIZATION - راه‌اندازی
        // ========================================
        init: function() {
            console.log('[ReceptionMainModule] Initializing...');
            
            try {
                this.setupGlobalErrorHandlers();
                this.setupPerformanceMonitoring();
                this.initializeModules();
                this.setupEventListeners();
                
                console.log('[ReceptionMainModule] Initialized successfully');
            } catch (error) {
                console.error('[ReceptionMainModule] Initialization failed:', error);
                this.handleError(error, 'initialization');
            }
        },

        // ========================================
        // GLOBAL ERROR HANDLERS SETUP - تنظیم handlers خطای سراسری
        // ========================================
        setupGlobalErrorHandlers: function() {
            var self = this;
            
            // Global error handler
            window.addEventListener('error', function(event) {
                self.handleGlobalError(event);
            });
            
            // Unhandled promise rejection handler
            window.addEventListener('unhandledrejection', function(event) {
                self.handleUnhandledRejection(event);
            });
        },

        // ========================================
        // PERFORMANCE MONITORING SETUP - تنظیم monitoring عملکرد
        // ========================================
        setupPerformanceMonitoring: function() {
            if (!CONFIG.performance.enableMonitoring) return;
            
            var self = this;
            
            // Memory usage monitoring
            setInterval(function() {
                self.monitorMemoryUsage();
            }, 30000);
            
            // Performance metrics collection
            setInterval(function() {
                self.collectPerformanceMetrics();
            }, 60000);
        },

        // ========================================
        // MODULES INITIALIZATION - راه‌اندازی ماژول‌ها
        // ========================================
        initializeModules: function() {
            var self = this;
            
            CONFIG.loadOrder.forEach(function(moduleName) {
                try {
                    self.initializeModule(moduleName);
                } catch (error) {
                    console.error('[ReceptionMainModule] Failed to initialize ' + moduleName + ':', error);
                    self.handleError(error, 'initializeModule:' + moduleName);
                }
            });
        },

        // ========================================
        // MODULE INITIALIZATION - راه‌اندازی ماژول
        // ========================================
        initializeModule: function(moduleName) {
            try {
                var module = global[moduleName];
                
                if (!module) {
                    console.warn('[ReceptionMainModule] Module ' + moduleName + ' not found in global scope');
                    return false;
                }
                
                if (module.init && typeof module.init === 'function') {
                    module.init();
                    console.log('[ReceptionMainModule] ' + moduleName + ' initialized successfully');
                    return true;
                } else {
                    console.warn('[ReceptionMainModule] ' + moduleName + ' does not have init method');
                    return false;
                }
            } catch (error) {
                console.error('[ReceptionMainModule] Error initializing ' + moduleName + ':', error);
                this.handleError(error, 'initializeModule:' + moduleName);
                return false;
            }
        },

        // ========================================
        // EVENT LISTENERS SETUP - تنظیم شنوندگان رویداد
        // ========================================
        setupEventListeners: function() {
            var self = this;
            
            // گوش دادن به رویدادهای سیستم
            if (global.ReceptionEventBus) {
                global.ReceptionEventBus.on('system:error', function(error) {
                    self.handleSystemError(error);
                });
                
                global.ReceptionEventBus.on('system:performance', function(metrics) {
                    self.handlePerformanceMetrics(metrics);
                });
            }
        },

        // ========================================
        // GLOBAL ERROR HANDLER - مدیریت خطای سراسری
        // ========================================
        handleGlobalError: function(event) {
            console.error('[ReceptionMainModule] Global error:', event);
            
            if (global.ReceptionErrorHandler) {
                global.ReceptionErrorHandler.handle({
                    message: event.message,
                    filename: event.filename,
                    lineno: event.lineno,
                    colno: event.colno,
                    error: event.error
                }, 'global');
            }
        },

        // ========================================
        // UNHANDLED REJECTION HANDLER - مدیریت rejection های مدیریت نشده
        // ========================================
        handleUnhandledRejection: function(event) {
            console.error('[ReceptionMainModule] Unhandled promise rejection:', event);
            
            if (global.ReceptionErrorHandler) {
                global.ReceptionErrorHandler.handle({
                    message: 'Unhandled Promise Rejection',
                    error: event.reason
                }, 'unhandledRejection');
            }
        },

        // ========================================
        // SYSTEM ERROR HANDLER - مدیریت خطای سیستم
        // ========================================
        handleSystemError: function(error) {
            console.error('[ReceptionMainModule] System error:', error);
        },

        // ========================================
        // PERFORMANCE METRICS HANDLER - مدیریت metrics عملکرد
        // ========================================
        handlePerformanceMetrics: function(metrics) {
            console.log('[ReceptionMainModule] Performance metrics:', metrics);
        },

        // ========================================
        // MEMORY USAGE MONITORING - monitoring استفاده از حافظه
        // ========================================
        monitorMemoryUsage: function() {
            if (!performance.memory) return;
            
            var memoryInfo = {
                used: performance.memory.usedJSHeapSize,
                total: performance.memory.totalJSHeapSize,
                limit: performance.memory.jsHeapSizeLimit
            };
            
            // هشدار در صورت استفاده زیاد از حافظه
            if (memoryInfo.used / memoryInfo.limit > CONFIG.performance.memoryThreshold) {
                console.warn('[ReceptionMainModule] High memory usage detected:', memoryInfo);
                
                if (global.ReceptionEventBus) {
                    global.ReceptionEventBus.emit('system:memory', memoryInfo);
                }
            }
        },

        // ========================================
        // PERFORMANCE METRICS COLLECTION - جمع‌آوری آمار عملکرد
        // ========================================
        collectPerformanceMetrics: function() {
            if (!CONFIG.performance.enableMetrics) return;
            
            var metrics = {
                timestamp: Date.now(),
                memory: this.getMemoryInfo(),
                performance: this.getPerformanceInfo(),
                modules: this.getModuleStatus()
            };
            
            if (global.ReceptionEventBus) {
                global.ReceptionEventBus.emit('system:performance', metrics);
            }
        },

        // ========================================
        // MEMORY INFO GETTER - دریافت اطلاعات حافظه
        // ========================================
        getMemoryInfo: function() {
            if (!performance.memory) return null;
            
            return {
                used: performance.memory.usedJSHeapSize,
                total: performance.memory.totalJSHeapSize,
                limit: performance.memory.jsHeapSizeLimit,
                usage: performance.memory.usedJSHeapSize / performance.memory.jsHeapSizeLimit
            };
        },

        // ========================================
        // PERFORMANCE INFO GETTER - دریافت اطلاعات عملکرد
        // ========================================
        getPerformanceInfo: function() {
            var navigation = performance.getEntriesByType('navigation')[0];
            
            return {
                loadTime: navigation ? navigation.loadEventEnd - navigation.loadEventStart : 0,
                domContentLoaded: navigation ? navigation.domContentLoadedEventEnd - navigation.domContentLoadedEventStart : 0,
                firstPaint: this.getFirstPaint(),
                firstContentfulPaint: this.getFirstContentfulPaint()
            };
        },

        // ========================================
        // FIRST PAINT GETTER - دریافت First Paint
        // ========================================
        getFirstPaint: function() {
            var paintEntries = performance.getEntriesByType('paint');
            var firstPaint = paintEntries.find(function(entry) { return entry.name === 'first-paint'; });
            return firstPaint ? firstPaint.startTime : 0;
        },

        // ========================================
        // FIRST CONTENTFUL PAINT GETTER - دریافت First Contentful Paint
        // ========================================
        getFirstContentfulPaint: function() {
            var paintEntries = performance.getEntriesByType('paint');
            var firstContentfulPaint = paintEntries.find(function(entry) { return entry.name === 'first-contentful-paint'; });
            return firstContentfulPaint ? firstContentfulPaint.startTime : 0;
        },

        // ========================================
        // MODULE STATUS GETTER - دریافت وضعیت ماژول‌ها
        // ========================================
        getModuleStatus: function() {
            var status = {};
            
            CONFIG.loadOrder.forEach(function(moduleName) {
                var module = global[moduleName];
                status[moduleName] = {
                    loaded: !!module,
                    hasInit: !!(module && module.init),
                    initialized: !!(module && module.initialized)
                };
            });
            
            return status;
        },

        // ========================================
        // ERROR HANDLING - مدیریت خطا
        // ========================================
        handleError: function(error, context) {
            console.error('[ReceptionMainModule] Error in ' + context + ':', error);
            
            // Fallback error handling if ReceptionErrorHandler is not available
            try {
                if (global.ReceptionErrorHandler && typeof global.ReceptionErrorHandler.handle === 'function') {
                    global.ReceptionErrorHandler.handle(error, 'ReceptionMainModule', context);
                } else {
                    this.fallbackErrorHandling(error, context);
                }
            } catch (handlerError) {
                console.error('[ReceptionMainModule] Error in error handler:', handlerError);
                this.fallbackErrorHandling(error, context);
            }
        },

        // ========================================
        // FALLBACK ERROR HANDLING - مدیریت خطای جایگزین
        // ========================================
        fallbackErrorHandling: function(error, context) {
            console.warn('[ReceptionMainModule] Using fallback error handling for context: ' + context);
            
            // Basic error logging
            console.error('[ReceptionMainModule] Fallback Error Details:', {
                message: error.message || 'Unknown error',
                stack: error.stack || 'No stack trace',
                context: context,
                timestamp: new Date().toISOString()
            });
            
            // Try to show user-friendly message if possible
            if (window.ReceptionToastr && window.ReceptionToastr.helpers) {
                try {
                    window.ReceptionToastr.helpers.showError('خطا در سیستم رخ داد. لطفا صفحه را بازخوانی کنید.');
                } catch (toastError) {
                    console.error('[ReceptionMainModule] Toast notification failed:', toastError);
                }
            }
        },

        // ========================================
        // MODULE MANAGEMENT - مدیریت ماژول‌ها
        // ========================================
        getModule: function(moduleName) {
            return global[moduleName];
        },

        isModuleLoaded: function(moduleName) {
            return !!global[moduleName];
        },

        getLoadedModules: function() {
            return CONFIG.loadOrder.filter(function(moduleName) { return this.isModuleLoaded(moduleName); }.bind(this));
        },

        // ========================================
        // HEALTH CHECK - بررسی سلامت سیستم
        // ========================================
        healthCheck: function() {
            var health = {
                status: 'healthy',
                issues: [],
                recommendations: [],
                modules: this.getModuleStatus(),
                performance: this.getPerformanceInfo(),
                memory: this.getMemoryInfo()
            };
            
            // بررسی ماژول‌های بارگذاری نشده
            var loadedModules = this.getLoadedModules();
            var missingModules = CONFIG.loadOrder.filter(function(moduleName) { return !loadedModules.includes(moduleName); });
            
            if (missingModules.length > 0) {
                health.status = 'degraded';
                health.issues.push(missingModules.length + ' modules not loaded: ' + missingModules.join(', '));
                health.recommendations.push('Check module dependencies and loading order');
            }
            
            // بررسی عملکرد حافظه
            if (health.memory && health.memory.usage > CONFIG.performance.memoryThreshold) {
                health.status = 'unhealthy';
                health.issues.push(`High memory usage: ${(health.memory.usage * 100).toFixed(1)}%`);
                health.recommendations.push('Optimize memory usage and check for memory leaks');
            }
            
            // بررسی عملکرد
            if (health.performance.loadTime > 5000) {
                health.issues.push(`Slow load time: ${health.performance.loadTime.toFixed(0)}ms`);
                health.recommendations.push('Optimize page loading performance');
            }
            
            return health;
        },

        // ========================================
        // DEBUGGING METHODS - متدهای debugging
        // ========================================
        getDebugInfo: function() {
            return {
                config: CONFIG,
                modules: this.getModuleStatus(),
                performance: this.getPerformanceInfo(),
                memory: this.getMemoryInfo(),
                health: this.healthCheck()
            };
        },

        // ========================================
        // CLEANUP - پاکسازی
        // ========================================
        destroy: function() {
            // پاکسازی event listeners
            window.removeEventListener('error', this.handleGlobalError);
            window.removeEventListener('unhandledrejection', this.handleUnhandledRejection);
            
            // پاکسازی ماژول‌ها
            CONFIG.loadOrder.forEach(function(moduleName) {
                var module = global[moduleName];
                if (module && module.destroy && typeof module.destroy === 'function') {
                    module.destroy();
                }
            });
            
            console.log('[ReceptionMainModule] Destroyed');
        }
    };

    // ========================================
    // AUTO INITIALIZATION - راه‌اندازی خودکار
    // ========================================
    $(document).ready(function() {
        // تاخیر برای اطمینان از بارگذاری کامل
        setTimeout(function() {
            ReceptionMainModule.init();
        }, 100);
    });

    // ========================================
    // GLOBAL EXPORT - صادرات سراسری
    // ========================================
    global.ReceptionMainModule = ReceptionMainModule;

    // ========================================
    // MODULE EXPORT - صادرات ماژول
    // ========================================
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = ReceptionMainModule;
    }

})(window, jQuery);
