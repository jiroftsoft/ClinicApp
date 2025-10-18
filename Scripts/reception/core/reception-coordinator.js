/**
 * RECEPTION COORDINATOR - هماهنگ‌کننده اصلی
 * ========================================
 * 
 * این سیستم مسئولیت‌های زیر را دارد:
 * - مدیریت ماژول‌ها
 * - هماهنگی بین ماژول‌ها
 * - مدیریت وابستگی‌ها
 * - Performance monitoring
 * - Health check
 * 
 * @author ClinicApp Development Team
 * @version 1.0.0
 * @since 2025-01-17
 */

(function(global, $) {
    'use strict';

    // ========================================
    // COORDINATOR CONFIGURATION - تنظیمات Coordinator
    // ========================================
    var CONFIG = {
        modules: {
            patientSearch: 'patient-search',
            patientInsurance: 'patient-insurance',
            departmentSelection: 'department-selection',
            serviceCalculation: 'service-calculation',
            paymentProcessing: 'payment-processing'
        },
        performance: {
            enableMonitoring: true,
            maxMemoryUsage: 50 * 1024 * 1024, // 50MB
            healthCheckInterval: 30000 // 30 seconds
        },
        dependencies: {
            jquery: 'jQuery',
            toastr: 'ReceptionToastr',
            eventBus: 'ReceptionEventBus',
            errorHandler: 'ReceptionErrorHandler'
        }
    };

    // ========================================
    // RECEPTION COORDINATOR CLASS - کلاس Reception Coordinator
    // ========================================
    function ReceptionCoordinator() {
        this.modules = {};
        this.dependencies = {};
        this.isInitialized = false;
        this.performanceMetrics = {
            memoryUsage: 0,
            moduleLoadTimes: {},
            healthStatus: 'unknown'
        };
        this.healthCheckInterval = null;
    }

    // ========================================
    // INITIALIZATION - مقداردهی اولیه
    // ========================================
    ReceptionCoordinator.prototype.init = function() {
        if (this.isInitialized) {
            console.warn('ReceptionCoordinator already initialized');
            return;
        }

        try {
            this.checkDependencies();
            this.initializeModules();
            this.setupPerformanceMonitoring();
            this.startHealthCheck();
            this.isInitialized = true;
            console.log('ReceptionCoordinator initialized successfully');
        } catch (error) {
            console.error('Failed to initialize ReceptionCoordinator:', error);
            throw error;
        }
    };

    // ========================================
    // DEPENDENCY CHECKING - بررسی وابستگی‌ها
    // ========================================
    ReceptionCoordinator.prototype.checkDependencies = function() {
        var missingDependencies = [];
        
        for (var dep in CONFIG.dependencies) {
            var globalName = CONFIG.dependencies[dep];
            if (typeof global[globalName] === 'undefined') {
                missingDependencies.push(globalName);
            } else {
                this.dependencies[dep] = global[globalName];
            }
        }

        if (missingDependencies.length > 0) {
            throw new Error('Missing dependencies: ' + missingDependencies.join(', '));
        }
    };

    // ========================================
    // MODULE INITIALIZATION - مقداردهی ماژول‌ها
    // ========================================
    ReceptionCoordinator.prototype.initializeModules = function() {
        var self = this;
        
        // Initialize Event Bus
        if (this.dependencies.eventBus) {
            this.modules.eventBus = new this.dependencies.eventBus();
        }

        // Initialize Error Handler
        if (this.dependencies.errorHandler) {
            this.modules.errorHandler = new this.dependencies.errorHandler();
            this.modules.errorHandler.init();
        }

        // Initialize feature modules
        this.loadModule('patientSearch');
        this.loadModule('patientInsurance');
        this.loadModule('departmentSelection');
        this.loadModule('serviceCalculation');
        this.loadModule('paymentProcessing');
    };

    // ========================================
    // MODULE LOADING - بارگذاری ماژول
    // ========================================
    ReceptionCoordinator.prototype.loadModule = function(moduleName) {
        var startTime = performance.now();
        
        try {
            var moduleConfig = CONFIG.modules[moduleName];
            if (!moduleConfig) {
                throw new Error('Unknown module: ' + moduleName);
            }

            // Check if module is available globally
            var globalModuleName = 'Reception' + this.capitalizeFirst(moduleName);
            if (typeof global[globalModuleName] !== 'undefined') {
                this.modules[moduleName] = global[globalModuleName];
                this.modules[moduleName].init();
            } else {
                console.warn('Module not found: ' + globalModuleName);
            }

            var loadTime = performance.now() - startTime;
            this.performanceMetrics.moduleLoadTimes[moduleName] = loadTime;
            
            console.log('Module loaded:', moduleName, 'in', loadTime.toFixed(2), 'ms');
        } catch (error) {
            console.error('Failed to load module:', moduleName, error);
            throw error;
        }
    };

    // ========================================
    // PERFORMANCE MONITORING SETUP - تنظیم نظارت بر عملکرد
    // ========================================
    ReceptionCoordinator.prototype.setupPerformanceMonitoring = function() {
        if (!CONFIG.performance.enableMonitoring) return;

        var self = this;
        
        // Memory usage monitoring
        setInterval(function() {
            if (typeof performance !== 'undefined' && performance.memory) {
                self.performanceMetrics.memoryUsage = performance.memory.usedJSHeapSize;
                
                if (self.performanceMetrics.memoryUsage > CONFIG.performance.maxMemoryUsage) {
                    console.warn('High memory usage detected:', self.performanceMetrics.memoryUsage);
                }
            }
        }, 10000); // Check every 10 seconds
    };

    // ========================================
    // HEALTH CHECK - بررسی سلامت سیستم
    // ========================================
    ReceptionCoordinator.prototype.startHealthCheck = function() {
        var self = this;
        
        this.healthCheckInterval = setInterval(function() {
            self.performHealthCheck();
        }, CONFIG.performance.healthCheckInterval);
    };

    ReceptionCoordinator.prototype.performHealthCheck = function() {
        var healthStatus = 'healthy';
        var issues = [];

        // Check memory usage
        if (this.performanceMetrics.memoryUsage > CONFIG.performance.maxMemoryUsage) {
            healthStatus = 'warning';
            issues.push('High memory usage');
        }

        // Check module status
        for (var moduleName in this.modules) {
            if (this.modules[moduleName] && typeof this.modules[moduleName].isHealthy === 'function') {
                if (!this.modules[moduleName].isHealthy()) {
                    healthStatus = 'unhealthy';
                    issues.push('Module unhealthy: ' + moduleName);
                }
            }
        }

        this.performanceMetrics.healthStatus = healthStatus;
        
        if (healthStatus !== 'healthy') {
            console.warn('Health check issues:', issues);
        }
    };

    // ========================================
    // MODULE MANAGEMENT - مدیریت ماژول‌ها
    // ========================================
    ReceptionCoordinator.prototype.getModule = function(moduleName) {
        return this.modules[moduleName];
    };

    ReceptionCoordinator.prototype.getAllModules = function() {
        return this.modules;
    };

    ReceptionCoordinator.prototype.isModuleLoaded = function(moduleName) {
        return this.modules.hasOwnProperty(moduleName) && this.modules[moduleName] !== null;
    };

    // ========================================
    // EVENT COORDINATION - هماهنگی رویدادها
    // ========================================
    ReceptionCoordinator.prototype.emitEvent = function(eventName, data, options) {
        if (this.modules.eventBus) {
            return this.modules.eventBus.emit(eventName, data, options);
        } else {
            console.warn('EventBus not available');
        }
    };

    ReceptionCoordinator.prototype.onEvent = function(eventName, callback, options) {
        if (this.modules.eventBus) {
            return this.modules.eventBus.on(eventName, callback, options);
        } else {
            console.warn('EventBus not available');
        }
    };

    // ========================================
    // ERROR HANDLING - مدیریت خطا
    // ========================================
    ReceptionCoordinator.prototype.handleError = function(error, context, options) {
        if (this.modules.errorHandler) {
            this.modules.errorHandler.handle(error, context, options);
        } else {
            console.error('ErrorHandler not available:', error);
        }
    };

    // ========================================
    // PERFORMANCE METRICS - آمار عملکرد
    // ========================================
    ReceptionCoordinator.prototype.getPerformanceMetrics = function() {
        return {
            memoryUsage: this.performanceMetrics.memoryUsage,
            moduleLoadTimes: this.performanceMetrics.moduleLoadTimes,
            healthStatus: this.performanceMetrics.healthStatus,
            moduleCount: Object.keys(this.modules).length
        };
    };

    // ========================================
    // UTILITY METHODS - متدهای کمکی
    // ========================================
    ReceptionCoordinator.prototype.capitalizeFirst = function(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    };

    ReceptionCoordinator.prototype.isHealthy = function() {
        return this.performanceMetrics.healthStatus === 'healthy';
    };

    // ========================================
    // CLEANUP - پاکسازی
    // ========================================
    ReceptionCoordinator.prototype.destroy = function() {
        if (this.healthCheckInterval) {
            clearInterval(this.healthCheckInterval);
            this.healthCheckInterval = null;
        }

        for (var moduleName in this.modules) {
            if (this.modules[moduleName] && typeof this.modules[moduleName].destroy === 'function') {
                this.modules[moduleName].destroy();
            }
        }

        this.modules = {};
        this.dependencies = {};
        this.isInitialized = false;
    };

    // ========================================
    // EXPORT - صادرات
    // ========================================
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = ReceptionCoordinator;
    } else if (typeof global !== 'undefined') {
        global.ReceptionCoordinator = ReceptionCoordinator;
    }

})(typeof window !== 'undefined' ? window : this, typeof jQuery !== 'undefined' ? jQuery : null);