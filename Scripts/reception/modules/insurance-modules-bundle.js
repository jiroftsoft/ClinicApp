/**
 * Insurance Modules Bundle - بسته ماژول‌های بیمه
 * Production-Ready Modular Architecture
 * 
 * @author ClinicApp Team
 * @version 1.0.0
 * @description بسته کامل ماژول‌های تخصصی بیمه با رعایت SRP
 */

(function() {
    'use strict';

    // ========================================
    // INSURANCE MODULES BUNDLE
    // ========================================
    window.InsuranceModulesBundle = {
        
        // Configuration
        config: {
            modules: [
                'FormChangeDetector',
                'EditModeManager', 
                'ValidationEngine',
                'SaveProcessor',
                'InsuranceOrchestrator'
            ],
            dependencies: {
                'jQuery': typeof $ !== 'undefined',
                'Toastr': typeof toastr !== 'undefined'
            }
        },

        // State
        isInitialized: false,
        initializedModules: [],

        // ========================================
        // INITIALIZATION - مقداردهی اولیه
        // ========================================
        init: function() {
            console.log('[InsuranceModulesBundle] Initializing...');
            
            try {
                this.checkDependencies();
                this.initializeModules();
                this.isInitialized = true;
                console.log('[InsuranceModulesBundle] ✅ All modules initialized successfully');
            } catch (error) {
                console.error('[InsuranceModulesBundle] ❌ Initialization failed:', error);
                throw error;
            }
        },

        // ========================================
        // CHECK DEPENDENCIES - بررسی وابستگی‌ها
        // ========================================
        checkDependencies: function() {
            console.log('[InsuranceModulesBundle] Checking dependencies...');
            
            try {
                var missingDeps = [];
                
                for (var dep in this.config.dependencies) {
                    if (!this.config.dependencies[dep]) {
                        missingDeps.push(dep);
                    }
                }
                
                if (missingDeps.length > 0) {
                    throw new Error('Missing dependencies: ' + missingDeps.join(', '));
                }
                
                console.log('[InsuranceModulesBundle] All dependencies available');
            } catch (error) {
                console.error('[InsuranceModulesBundle] Dependency check failed:', error);
                throw error;
            }
        },

        // ========================================
        // INITIALIZE MODULES - مقداردهی ماژول‌ها
        // ========================================
        initializeModules: function() {
            console.log('[InsuranceModulesBundle] Initializing modules...');
            
            try {
                for (var i = 0; i < this.config.modules.length; i++) {
                    var moduleName = this.config.modules[i];
                    this.initializeModule(moduleName);
                }
                
                console.log('[InsuranceModulesBundle] All modules initialized:', this.initializedModules);
            } catch (error) {
                console.error('[InsuranceModulesBundle] Error initializing modules:', error);
                throw error;
            }
        },

        // ========================================
        // INITIALIZE MODULE - مقداردهی یک ماژول
        // ========================================
        initializeModule: function(moduleName) {
            console.log('[InsuranceModulesBundle] Initializing module:', moduleName);
            
            try {
                if (window[moduleName] && typeof window[moduleName].init === 'function') {
                    window[moduleName].init();
                    this.initializedModules.push(moduleName);
                    console.log('[InsuranceModulesBundle] ✅ Module initialized:', moduleName);
                } else {
                    throw new Error('Module not found or invalid: ' + moduleName);
                }
            } catch (error) {
                console.error('[InsuranceModulesBundle] Error initializing module ' + moduleName + ':', error);
                throw error;
            }
        },

        // ========================================
        // GET MODULE STATUS - دریافت وضعیت ماژول‌ها
        // ========================================
        getModuleStatus: function() {
            var status = {
                bundleInitialized: this.isInitialized,
                initializedModules: this.initializedModules,
                totalModules: this.config.modules.length,
                dependencies: this.config.dependencies
            };
            
            // Add individual module status
            for (var i = 0; i < this.config.modules.length; i++) {
                var moduleName = this.config.modules[i];
                if (window[moduleName] && typeof window[moduleName].getStatus === 'function') {
                    status[moduleName] = window[moduleName].getStatus();
                }
            }
            
            return status;
        },

        // ========================================
        // RESET ALL MODULES - بازنشانی همه ماژول‌ها
        // ========================================
        resetAllModules: function() {
            console.log('[InsuranceModulesBundle] Resetting all modules...');
            
            try {
                for (var i = 0; i < this.initializedModules.length; i++) {
                    var moduleName = this.initializedModules[i];
                    if (window[moduleName] && typeof window[moduleName].reset === 'function') {
                        window[moduleName].reset();
                    }
                }
                
                this.initializedModules = [];
                this.isInitialized = false;
                console.log('[InsuranceModulesBundle] All modules reset');
            } catch (error) {
                console.error('[InsuranceModulesBundle] Error resetting modules:', error);
                throw error;
            }
        },

        // ========================================
        // GET HEALTH STATUS - دریافت وضعیت سلامت
        // ========================================
        getHealthStatus: function() {
            var health = {
                status: 'healthy',
                issues: [],
                recommendations: []
            };
            
            try {
                // Check bundle status
                if (!this.isInitialized) {
                    health.status = 'unhealthy';
                    health.issues.push('Bundle not initialized');
                }
                
                // Check module status
                for (var i = 0; i < this.config.modules.length; i++) {
                    var moduleName = this.config.modules[i];
                    if (this.initializedModules.indexOf(moduleName) === -1) {
                        health.status = 'unhealthy';
                        health.issues.push('Module not initialized: ' + moduleName);
                    }
                }
                
                // Check dependencies
                for (var dep in this.config.dependencies) {
                    if (!this.config.dependencies[dep]) {
                        health.status = 'unhealthy';
                        health.issues.push('Missing dependency: ' + dep);
                    }
                }
                
                if (health.issues.length === 0) {
                    health.recommendations.push('All systems operational');
                }
                
            } catch (error) {
                health.status = 'error';
                health.issues.push('Health check failed: ' + error.message);
            }
            
            return health;
        }
    };

    // Auto-initialize when DOM is ready
    $(document).ready(function() {
        try {
            window.InsuranceModulesBundle.init();
        } catch (error) {
            console.error('[InsuranceModulesBundle] Auto-initialization failed:', error);
        }
    });

})();
