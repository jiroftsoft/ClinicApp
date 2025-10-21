/**
 * Advanced Insurance System - سیستم پیشرفته بیمه (ES5)
 * ====================================================
 * 
 * این فایل اصلی سیستم پیشرفته بیمه را هماهنگ می‌کند:
 * - Advanced Insurance Coordinator
 * - Advanced Change Detector
 * - Advanced State Manager
 * - Modern Event System
 * - Performance Monitoring
 * 
 * @author ClinicApp Development Team
 * @version 3.0.0
 * @since 2025-01-20
 */

(function(global, $) {
    'use strict';

    // ========================================
    // ADVANCED INSURANCE SYSTEM CONSTRUCTOR
    // ========================================
    function AdvancedInsuranceSystem() {
        this.modules = {};
        this.eventBus = null;
        this.performanceMonitor = null;
        this.isInitialized = false;
        this.isProcessing = false;
        
        this.init();
    }

    // ========================================
    // INITIALIZATION - راه‌اندازی
    // ========================================
    AdvancedInsuranceSystem.prototype.init = function() {
        console.log('[AdvancedInsuranceSystem] 🚀 Initializing advanced insurance system...');
        
        try {
            // Initialize performance monitoring
            this.setupPerformanceMonitoring();
            
            // Initialize modules
            this.initializeModules();
            
            // Setup event coordination
            this.setupEventCoordination();
            
            // Setup error handling
            this.setupErrorHandling();
            
            this.isInitialized = true;
            console.log('[AdvancedInsuranceSystem] ✅ Advanced insurance system initialized successfully');
            
        } catch (error) {
            console.error('[AdvancedInsuranceSystem] ❌ Initialization failed:', error);
            this.handleCriticalError(error);
        }
    };

    // ========================================
    // INITIALIZE MODULES - راه‌اندازی ماژول‌ها
    // ========================================
    AdvancedInsuranceSystem.prototype.initializeModules = function() {
        console.log('[AdvancedInsuranceSystem] 🔧 Initializing modules...');
        
        try {
            // Initialize Advanced Change Detector
            if (window.AdvancedChangeDetector) {
                this.modules.changeDetector = window.AdvancedChangeDetector;
                console.log('[AdvancedInsuranceSystem] ✅ Change Detector initialized');
            }
            
            // Initialize Advanced State Manager
            if (window.AdvancedStateManager) {
                this.modules.stateManager = window.AdvancedStateManager;
                console.log('[AdvancedInsuranceSystem] ✅ State Manager initialized');
            }
            
            // Initialize Advanced Insurance Coordinator
            if (window.AdvancedInsuranceCoordinator) {
                this.modules.coordinator = window.AdvancedInsuranceCoordinator;
                console.log('[AdvancedInsuranceSystem] ✅ Insurance Coordinator initialized');
            }
            
            console.log('[AdvancedInsuranceSystem] ✅ All modules initialized');
            
        } catch (error) {
            console.error('[AdvancedInsuranceSystem] ❌ Module initialization failed:', error);
            throw error;
        }
    };

    // ========================================
    // SETUP EVENT COORDINATION - تنظیم هماهنگی رویدادها
    // ========================================
    AdvancedInsuranceSystem.prototype.setupEventCoordination = function() {
        console.log('[AdvancedInsuranceSystem] 🔗 Setting up event coordination...');
        
        var self = this;
        
        // Patient search events
        $(document).on('patientSearchSuccess.advancedInsuranceSystem', function(event, data) {
            self.handlePatientSearchSuccess(data);
        });
        
        // Form change events
        $(document).on('formChange.advancedInsuranceSystem', function(event, data) {
            self.handleFormChange(data);
        });
        
        // Save events
        $(document).on('saveRequest.advancedInsuranceSystem', function(event, data) {
            self.handleSaveRequest(data);
        });
        
        console.log('[AdvancedInsuranceSystem] ✅ Event coordination setup completed');
    };

    // ========================================
    // SETUP PERFORMANCE MONITORING - تنظیم نظارت بر عملکرد
    // ========================================
    AdvancedInsuranceSystem.prototype.setupPerformanceMonitoring = function() {
        console.log('[AdvancedInsuranceSystem] 📊 Setting up performance monitoring...');
        
        this.performanceMonitor = {
            startTime: Date.now(),
            memoryUsage: [],
            eventCount: 0,
            errorCount: 0,
            
            recordEvent: function() {
                this.eventCount++;
            },
            
            recordError: function() {
                this.errorCount++;
            },
            
            recordMemoryUsage: function() {
                if (performance.memory) {
                    this.memoryUsage.push({
                        timestamp: Date.now(),
                        used: performance.memory.usedJSHeapSize,
                        total: performance.memory.totalJSHeapSize
                    });
                }
            },
            
            getReport: function() {
                return {
                    uptime: Date.now() - this.startTime,
                    eventCount: this.eventCount,
                    errorCount: this.errorCount,
                    memoryUsage: this.memoryUsage,
                    averageMemoryUsage: this.calculateAverageMemoryUsage()
                };
            },
            
            calculateAverageMemoryUsage: function() {
                if (this.memoryUsage.length === 0) {
                    return 0;
                }
                
                var total = this.memoryUsage.reduce(function(sum, record) {
                    return sum + record.used;
                }, 0);
                
                return total / this.memoryUsage.length;
            }
        };
        
        // Record memory usage every 30 seconds
        var self = this;
        setInterval(function() {
            self.performanceMonitor.recordMemoryUsage();
        }, 30000);
        
        console.log('[AdvancedInsuranceSystem] ✅ Performance monitoring setup completed');
    };

    // ========================================
    // SETUP ERROR HANDLING - تنظیم مدیریت خطا
    // ========================================
    AdvancedInsuranceSystem.prototype.setupErrorHandling = function() {
        console.log('[AdvancedInsuranceSystem] 🛡️ Setting up error handling...');
        
        var self = this;
        
        // Global error handler
        window.addEventListener('error', function(event) {
            self.handleError(event.error);
        });
        
        // Unhandled promise rejection handler
        window.addEventListener('unhandledrejection', function(event) {
            self.handleError(event.reason);
        });
        
        console.log('[AdvancedInsuranceSystem] ✅ Error handling setup completed');
    };

    // ========================================
    // HANDLE PATIENT SEARCH SUCCESS - مدیریت موفقیت جستجوی بیمار
    // ========================================
    AdvancedInsuranceSystem.prototype.handlePatientSearchSuccess = function(data) {
        console.log('[AdvancedInsuranceSystem] 👤 Handling patient search success...');
        
        try {
            this.performanceMonitor.recordEvent();
            
            // Notify coordinator
            var coordinator = this.modules.coordinator;
            if (coordinator) {
                coordinator.handlePatientSearchSuccess(data);
            }
            
            // Update state manager
            var stateManager = this.modules.stateManager;
            if (stateManager) {
                stateManager.transitionTo('LOADING', data);
            }
            
            console.log('[AdvancedInsuranceSystem] ✅ Patient search success handled');
            
        } catch (error) {
            console.error('[AdvancedInsuranceSystem] ❌ Error handling patient search success:', error);
            this.handleError(error);
        }
    };

    // ========================================
    // HANDLE FORM CHANGE - مدیریت تغییر فرم
    // ========================================
    AdvancedInsuranceSystem.prototype.handleFormChange = function(data) {
        console.log('[AdvancedInsuranceSystem] 🔄 Handling form change...');
        
        try {
            this.performanceMonitor.recordEvent();
            
            // Notify change detector
            var changeDetector = this.modules.changeDetector;
            if (changeDetector) {
                changeDetector.handleFormChange(data);
            }
            
            // Update state manager
            var stateManager = this.modules.stateManager;
            if (stateManager) {
                if (data.hasChanges) {
                    stateManager.transitionTo('EDITING', data);
                } else {
                    stateManager.transitionTo('IDLE', data);
                }
            }
            
            console.log('[AdvancedInsuranceSystem] ✅ Form change handled');
            
        } catch (error) {
            console.error('[AdvancedInsuranceSystem] ❌ Error handling form change:', error);
            this.handleError(error);
        }
    };

    // ========================================
    // HANDLE SAVE REQUEST - مدیریت درخواست ذخیره
    // ========================================
    AdvancedInsuranceSystem.prototype.handleSaveRequest = function(data) {
        console.log('[AdvancedInsuranceSystem] 💾 Handling save request...');
        
        var self = this;
        
        try {
            this.performanceMonitor.recordEvent();
            this.isProcessing = true;
            
            // Update state manager
            var stateManager = this.modules.stateManager;
            if (stateManager) {
                stateManager.transitionTo('SAVING', data);
            }
            
            // Validate data
            var validationResult = this.validateSaveData(data);
            if (!validationResult.isValid) {
                throw new Error(validationResult.errors.join(', '));
            }
            
            // Save data
            this.saveData(data).then(function(result) {
                if (result.success) {
                    // Update state manager
                    if (stateManager) {
                        stateManager.transitionTo('SUCCESS', result);
                    }
                    
                    // Reset change detector
                    var changeDetector = self.modules.changeDetector;
                    if (changeDetector) {
                        changeDetector.resetChanges();
                    }
                    
                    console.log('[AdvancedInsuranceSystem] ✅ Save request handled successfully');
                } else {
                    throw new Error(result.message || 'Save failed');
                }
            }).catch(function(error) {
                console.error('[AdvancedInsuranceSystem] ❌ Save error:', error);
                self.handleError(error);
                
                // Update state manager
                if (stateManager) {
                    stateManager.transitionTo('ERROR', { error: error.message });
                }
            });
            
        } catch (error) {
            console.error('[AdvancedInsuranceSystem] ❌ Error handling save request:', error);
            this.handleError(error);
            
            // Update state manager
            var stateManager = this.modules.stateManager;
            if (stateManager) {
                stateManager.transitionTo('ERROR', { error: error.message });
            }
        } finally {
            this.isProcessing = false;
        }
    };

    // ========================================
    // VALIDATE SAVE DATA - اعتبارسنجی داده‌های ذخیره
    // ========================================
    AdvancedInsuranceSystem.prototype.validateSaveData = function(data) {
        console.log('[AdvancedInsuranceSystem] 🔍 Validating save data...');
        
        var errors = [];
        
        // Required field validation
        if (!data.PatientId) {
            errors.push('شناسه بیمار الزامی است');
        }
        
        if (!data.PrimaryInsuranceProviderId) {
            errors.push('سازمان بیمه پایه الزامی است');
        }
        
        if (data.PrimaryInsuranceProviderId && !data.PrimaryInsurancePlanId) {
            errors.push('طرح بیمه پایه الزامی است');
        }
        
        if (data.SupplementaryInsuranceProviderId && !data.SupplementaryInsurancePlanId) {
            errors.push('طرح بیمه تکمیلی الزامی است');
        }
        
        return {
            isValid: errors.length === 0,
            errors: errors
        };
    };

    // ========================================
    // SAVE DATA - ذخیره داده
    // ========================================
    AdvancedInsuranceSystem.prototype.saveData = function(data) {
        console.log('[AdvancedInsuranceSystem] 💾 Saving data...');
        
        var self = this;
        return new Promise(function(resolve, reject) {
            $.ajax({
                url: '/Reception/Insurance/Save',
                type: 'POST',
                data: data,
                timeout: 30000,
                success: function(response) {
                    resolve(response);
                },
                error: function(xhr, status, error) {
                    reject({ message: 'خطا در ارتباط با سرور' });
                }
            });
        });
    };

    // ========================================
    // HANDLE ERROR - مدیریت خطا
    // ========================================
    AdvancedInsuranceSystem.prototype.handleError = function(error) {
        console.error('[AdvancedInsuranceSystem] 🚨 Error:', error);
        
        this.performanceMonitor.recordError();
        
        // Update state manager
        var stateManager = this.modules.stateManager;
        if (stateManager) {
            stateManager.transitionTo('ERROR', { error: error.message || 'Unknown error' });
        }
        
        // Show user-friendly error
        if (typeof toastr !== 'undefined') {
            toastr.error('خطا در سیستم. لطفاً صفحه را بازخوانی کنید.');
        }
    };

    // ========================================
    // HANDLE CRITICAL ERROR - مدیریت خطای بحرانی
    // ========================================
    AdvancedInsuranceSystem.prototype.handleCriticalError = function(error) {
        console.error('[AdvancedInsuranceSystem] 🚨 Critical error:', error);
        
        // Reset all modules
        var self = this;
        Object.keys(this.modules).forEach(function(moduleName) {
            try {
                var module = self.modules[moduleName];
                if (module && module.cleanup) {
                    module.cleanup();
                }
            } catch (cleanupError) {
                console.error('[AdvancedInsuranceSystem] ❌ Cleanup error for ' + moduleName + ':', cleanupError);
            }
        });
        
        // Show critical error message
        if (typeof toastr !== 'undefined') {
            toastr.error('خطای بحرانی در سیستم. لطفاً صفحه را بازخوانی کنید.');
        }
    };

    // ========================================
    // GET MODULE - دریافت ماژول
    // ========================================
    AdvancedInsuranceSystem.prototype.getModule = function(name) {
        return this.modules[name];
    };

    // ========================================
    // GET PERFORMANCE REPORT - دریافت گزارش عملکرد
    // ========================================
    AdvancedInsuranceSystem.prototype.getPerformanceReport = function() {
        return this.performanceMonitor.getReport();
    };

    // ========================================
    // GET STATUS - دریافت وضعیت
    // ========================================
    AdvancedInsuranceSystem.prototype.getStatus = function() {
        return {
            isInitialized: this.isInitialized,
            isProcessing: this.isProcessing,
            modulesCount: Object.keys(this.modules).length,
            performanceReport: this.getPerformanceReport()
        };
    };

    // ========================================
    // CLEANUP - پاکسازی
    // ========================================
    AdvancedInsuranceSystem.prototype.cleanup = function() {
        console.log('[AdvancedInsuranceSystem] 🧹 Cleaning up...');
        
        try {
            // Cleanup modules
            var self = this;
            Object.keys(this.modules).forEach(function(moduleName) {
                try {
                    var module = self.modules[moduleName];
                    if (module && module.cleanup) {
                        module.cleanup();
                    }
                } catch (error) {
                    console.error('[AdvancedInsuranceSystem] ❌ Cleanup error for ' + moduleName + ':', error);
                }
            });
            
            // Clear modules
            this.modules = {};
            
            // Remove event listeners
            $(document).off('.advancedInsuranceSystem');
            
            // Reset state
            this.isInitialized = false;
            this.isProcessing = false;
            
            console.log('[AdvancedInsuranceSystem] ✅ Cleanup completed');
            
        } catch (error) {
            console.error('[AdvancedInsuranceSystem] ❌ Error during cleanup:', error);
        }
    };

    // ========================================
    // GLOBAL INSTANCE - نمونه سراسری
    // ========================================
    var systemInstance = null;

    // Initialize when DOM is ready
    $(document).ready(function() {
        try {
            systemInstance = new AdvancedInsuranceSystem();
            window.AdvancedInsuranceSystem = systemInstance;
            console.log('[AdvancedInsuranceSystem] 🌟 Global instance created');
        } catch (error) {
            console.error('[AdvancedInsuranceSystem] ❌ Failed to create global instance:', error);
        }
    });

    // Cleanup on page unload
    $(window).on('beforeunload', function() {
        if (systemInstance) {
            systemInstance.cleanup();
        }
    });

})(window, jQuery);
