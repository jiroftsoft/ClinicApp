/**
 * Medical Configuration Module
 * ماژول تنظیمات محیط درمانی
 * 
 * @author ClinicApp Medical Team
 * @version 1.0.0
 * @description Configuration settings for medical environment
 */

(function() {
    'use strict';
    
    // Global MedicalConfig object
    window.MedicalConfig = {
    // API Configuration
    apiBaseUrl: '/Admin/SupplementaryTariff/',
    
    // Cache Configuration
    cacheTimeout: 300000, // 5 minutes
    
    // Request Configuration
    maxRetries: 3,
    retryDelay: 1000,
    requestTimeout: 30000,
    
    // UI Configuration
    logPrefix: '🏥 MEDICAL:',
    successMessageTimeout: 3000,
    errorMessageTimeout: 5000,
    
    // Medical Standards
    medicalStandards: {
        requireValidation: true,
        enableLogging: true,
        enableErrorHandling: true,
        enablePerformanceMonitoring: true
    },
    
    // Validation Rules
    validationRules: {
        minTariffPrice: 0,
        maxTariffPrice: 999999999,
        minPatientShare: 0,
        maxPatientShare: 999999999,
        minInsurerShare: 0,
        maxInsurerShare: 999999999,
        minCoveragePercent: 0,
        maxCoveragePercent: 100
    },
    
    /**
     * Initialize MedicalConfig
     * راه‌اندازی MedicalConfig
     */
    init: function() {
        console.log(this.logPrefix + ' MedicalConfig initialized successfully');
        return true;
    },
    
    /**
     * Get configuration value
     * دریافت مقدار تنظیمات
     */
    get: function(key) {
        return this[key];
    },
    
    /**
     * Set configuration value
     * تنظیم مقدار تنظیمات
     */
    set: function(key, value) {
        this[key] = value;
        return this;
    }
    };
    
})();
