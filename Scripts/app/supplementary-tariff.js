/**
 * Supplementary Tariff Management - Main Module
 * مدیریت تعرفه‌های بیمه تکمیلی - ماژول اصلی
 * 
 * @author ClinicApp Medical Team
 * @version 3.0.0 - Medical Production Ready
 * @description Main module for supplementary tariff management
 */

/**
 * Medical Supplementary Tariff Management Module
 * ماژول مدیریت تعرفه‌های بیمه تکمیلی درمانی
 * 
 * @author ClinicApp Medical Team
 * @version 1.0.0
 * @description Main module for supplementary tariff management
 */

(function() {
    'use strict';
    
    // Main Medical Supplementary Tariff Module
    window.MedicalSupplementaryTariff = {
    
    /**
     * Initialize the module
     * راه‌اندازی ماژول
     */
    init: function() {
        console.log('🏥 MEDICAL: Initializing Supplementary Tariff Management');
        
        try {
            // Initialize UI components
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.init();
            }
            
            // Load initial data
            this.loadInitialData();
            
            console.log('🏥 MEDICAL: Module initialized successfully');
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error initializing module:', error);
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.showError('خطا در راه‌اندازی سیستم: ' + error.message);
            }
        }
    },

    /**
     * Load initial data
     * بارگذاری داده‌های اولیه
     */
    loadInitialData: function() {
        console.log('🏥 MEDICAL: Loading initial data');
        
        try {
            // Load statistics
            if (typeof window.MedicalAPI !== 'undefined') {
                window.MedicalAPI.loadStatistics();
                
                // Load tariffs
                window.MedicalAPI.loadTariffs();
                
                // Load filter options
                window.MedicalAPI.loadFilterOptions();
            }
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error loading initial data:', error);
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.showError('خطا در بارگذاری داده‌ها: ' + error.message);
            }
        }
    },

    /**
     * Create new insurance plan
     * ایجاد طرح بیمه جدید
     */
    createNewPlan: function() {
        console.log('🏥 MEDICAL: Creating new insurance plan');
        
        try {
            // Validate environment
            if (!window.location) {
                throw new Error('Browser environment not available');
            }
            
            // Redirect to insurance plan creation page
            window.location.href = '/Admin/InsurancePlan/Create';
            
            console.log('🏥 MEDICAL: Redirected to insurance plan creation');
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error creating new plan:', error);
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.showError('خطا در ایجاد طرح بیمه جدید: ' + error.message);
            }
        }
    },

    /**
     * Create new tariff
     * ایجاد تعرفه جدید
     */
    createNewTariff: function() {
        console.log('🏥 MEDICAL: Creating new tariff');
        
        try {
            // Redirect to create tariff page
            window.location.href = '/Admin/SupplementaryTariff/Create';
            
            console.log('🏥 MEDICAL: Redirected to create tariff page');
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error redirecting to create tariff page:', error);
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.showError('خطا در انتقال به صفحه ایجاد تعرفه: ' + error.message);
            }
        }
    },

    /**
     * Save new tariff
     * ذخیره تعرفه جدید
     */
    saveNewTariff: function() {
        console.log('🏥 MEDICAL: Saving new tariff');
        
        try {
            // Validate form data
            if (typeof window.MedicalValidation !== 'undefined') {
                const validationResult = window.MedicalValidation.validateCreateTariffForm();
                if (!validationResult.isValid) {
                    if (typeof window.MedicalUI !== 'undefined') {
                        window.MedicalUI.showError(validationResult.errorMessage);
                    }
                    return;
                }
                
                // Show loading
                if (typeof window.MedicalUI !== 'undefined') {
                    window.MedicalUI.showLoading();
                }
                
                // Make API call
                if (typeof window.MedicalAPI !== 'undefined') {
                    window.MedicalAPI.createTariff(validationResult.data)
                        .then(response => {
                            if (typeof window.MedicalUI !== 'undefined') {
                                window.MedicalUI.hideLoading();
                                
                                if (response.success) {
                                    window.MedicalUI.showSuccess('تعرفه جدید با موفقیت ایجاد شد');
                                    window.MedicalUI.hideCreateTariffModal();
                                    window.MedicalUI.resetCreateTariffForm();
                                    this.loadInitialData();
                                } else {
                                    window.MedicalUI.showError('خطا در ایجاد تعرفه جدید: ' + response.message);
                                }
                            }
                        })
                        .catch(error => {
                            if (typeof window.MedicalUI !== 'undefined') {
                                window.MedicalUI.hideLoading();
                                window.MedicalUI.showError('خطا در ایجاد تعرفه جدید: ' + error.message);
                            }
                        });
                }
            }
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error in saveNewTariff:', error);
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.hideLoading();
                window.MedicalUI.showError('خطا در ذخیره تعرفه: ' + error.message);
            }
        }
    },

    /**
     * Apply filters
     * اعمال فیلترها
     */
    applyFilters: function() {
        console.log('🏥 MEDICAL: Applying filters');
        
        try {
            // Get filter values
            if (typeof window.MedicalUI !== 'undefined') {
                const filterData = window.MedicalUI.getFilterValues();
                
                console.log('🏥 MEDICAL: Filter values:', filterData);
                
                // Show loading
                window.MedicalUI.showLoading();
                
                // Make API call
                if (typeof window.MedicalAPI !== 'undefined') {
                    window.MedicalAPI.getTariffs(filterData)
                        .then(response => {
                            if (typeof window.MedicalUI !== 'undefined') {
                                window.MedicalUI.hideLoading();
                                
                                if (response.success && response.data) {
                                    window.MedicalUI.updateTariffsDisplay(response.data);
                                    window.MedicalUI.showSuccess('فیلترها با موفقیت اعمال شد');
                                } else {
                                    window.MedicalUI.showError('خطا در اعمال فیلترها: ' + response.message);
                                }
                            }
                        })
                        .catch(error => {
                            if (typeof window.MedicalUI !== 'undefined') {
                                window.MedicalUI.hideLoading();
                                window.MedicalUI.showError('خطا در اعمال فیلترها: ' + error.message);
                            }
                        });
                }
            }
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error in applyFilters:', error);
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.hideLoading();
                window.MedicalUI.showError('خطا در اعمال فیلترها: ' + error.message);
            }
        }
    },

    /**
     * Clear filters
     * پاک کردن فیلترها
     */
    clearFilters: function() {
        console.log('🏥 MEDICAL: Clearing filters');
        
        try {
            // Clear all filter inputs
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.clearFilterInputs();
                
                // Reload data without filters
                if (typeof window.MedicalAPI !== 'undefined') {
                    window.MedicalAPI.loadTariffs();
                }
                
                window.MedicalUI.showSuccess('فیلترها پاک شدند');
            }
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error in clearFilters:', error);
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.showError('خطا در پاک کردن فیلترها: ' + error.message);
            }
        }
    }
};

    // Global function aliases for HTML onclick events
    window.createNewPlan = window.MedicalSupplementaryTariff.createNewPlan.bind(window.MedicalSupplementaryTariff);
    window.createNewTariff = window.MedicalSupplementaryTariff.createNewTariff.bind(window.MedicalSupplementaryTariff);
    window.saveNewTariff = window.MedicalSupplementaryTariff.saveNewTariff.bind(window.MedicalSupplementaryTariff);
    window.applyFilters = window.MedicalSupplementaryTariff.applyFilters.bind(window.MedicalSupplementaryTariff);
    window.clearFilters = window.MedicalSupplementaryTariff.clearFilters.bind(window.MedicalSupplementaryTariff);

    // Initialize when document is ready
    $(document).ready(function() {
        console.log('🏥 MEDICAL: Document ready, initializing Supplementary Tariff Management');
        
        try {
            // Initialize the module
            window.MedicalSupplementaryTariff.init();
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error in document ready:', error);
        }
    });

})();
