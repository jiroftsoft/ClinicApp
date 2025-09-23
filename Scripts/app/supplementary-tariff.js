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
    
    // CSRF Token Setup for Medical Environment
    // تنظیم CSRF Token برای محیط درمانی
    var csrfToken = $('input[name="__RequestVerificationToken"]').val();
    
    // Configure AJAX defaults for CSRF protection
    // تنظیم پیش‌فرض‌های AJAX برای محافظت CSRF
    $.ajaxSetup({
        beforeSend: function(xhr, settings) {
            if (settings.type === 'POST' && csrfToken) {
                xhr.setRequestHeader('RequestVerificationToken', csrfToken);
            }
        }
    });
    
    // Helper Functions for Code Reusability
    // توابع کمکی برای کاهش تکرار کد
    var MedicalHelpers = {
        /**
         * Safe UI call with error handling
         * فراخوانی امن UI با مدیریت خطا
         */
        safeUICall: function(method, ...args) {
            try {
                if (typeof window.MedicalUI !== 'undefined' && typeof window.MedicalUI[method] === 'function') {
                    return window.MedicalUI[method](...args);
                } else {
                    console.warn('🏥 MEDICAL: MedicalUI.' + method + ' not available');
                    return false;
                }
            } catch (error) {
                console.error('🏥 MEDICAL: Error in safeUICall:', error);
                return false;
            }
        },

        /**
         * Safe API call with error handling
         * فراخوانی امن API با مدیریت خطا
         */
        safeAPICall: function(method, ...args) {
            try {
                if (typeof window.MedicalAPI !== 'undefined' && typeof window.MedicalAPI[method] === 'function') {
                    return window.MedicalAPI[method](...args);
                } else {
                    console.warn('🏥 MEDICAL: MedicalAPI.' + method + ' not available');
                    return Promise.reject(new Error('MedicalAPI.' + method + ' not available'));
                }
            } catch (error) {
                console.error('🏥 MEDICAL: Error in safeAPICall:', error);
                return Promise.reject(error);
            }
        },

        /**
         * Safe validation call with error handling
         * فراخوانی امن validation با مدیریت خطا
         */
        safeValidationCall: function(method, ...args) {
            try {
                if (typeof window.MedicalValidation !== 'undefined' && typeof window.MedicalValidation[method] === 'function') {
                    return window.MedicalValidation[method](...args);
                } else {
                    console.warn('🏥 MEDICAL: MedicalValidation.' + method + ' not available');
                    return { isValid: false, errorMessage: 'Validation not available' };
                }
            } catch (error) {
                console.error('🏥 MEDICAL: Error in safeValidationCall:', error);
                return { isValid: false, errorMessage: 'Validation error: ' + error.message };
            }
        },

        /**
         * Show error message safely
         * نمایش پیام خطا به صورت امن
         */
        showError: function(message) {
            this.safeUICall('showError', message);
        },

        /**
         * Show loading indicator safely
         * نمایش نشانگر بارگذاری به صورت امن
         */
        showLoading: function() {
            this.safeUICall('showLoading');
        },

        /**
         * Hide loading indicator safely
         * مخفی کردن نشانگر بارگذاری به صورت امن
         */
        hideLoading: function() {
            this.safeUICall('hideLoading');
        }
    };

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
            MedicalHelpers.safeUICall('init');
            
            // Load initial data
            this.loadInitialData();
            
            console.log('🏥 MEDICAL: Module initialized successfully');
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error initializing module:', error);
            MedicalHelpers.showError('خطا در راه‌اندازی سیستم: ' + error.message);
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
            MedicalHelpers.showError('خطا در بارگذاری داده‌ها: ' + error.message);
        }
    },

    /**
     * Load tariffs with pagination
     * بارگذاری تعرفه‌ها با صفحه‌بندی
     */
    loadTariffs: function(page = 1) {
        console.log('🏥 MEDICAL: Loading tariffs for page:', page);
        
        try {
            // Get current filters
            const searchTerm = $('#searchTerm').val() || '';
            const insurancePlanId = $('#insurancePlanId').val() || '';
            const departmentId = $('#departmentId').val() || '';
            const isActive = $('#isActive').val() || '';
            
            // Build query parameters
            const params = new URLSearchParams({
                searchTerm: searchTerm,
                insurancePlanId: insurancePlanId,
                departmentId: departmentId,
                isActive: isActive,
                page: page,
                pageSize: 10
            });
            
            // Make API call
            if (typeof window.MedicalAPI !== 'undefined') {
                window.MedicalAPI.getTariffs(params.toString())
                    .then(response => {
                        if (response.success) {
                            // Update tariffs display
                            if (typeof window.MedicalUI !== 'undefined') {
                                window.MedicalUI.updateTariffsDisplay(response.data);
                            }
                            
                            // Update pagination
                            this.updatePagination(response);
                            
                        } else {
                            console.error('🏥 MEDICAL: Error loading tariffs:', response.message);
                        }
                    })
                    .catch(error => {
                        console.error('🏥 MEDICAL: Error loading tariffs:', error);
                    });
            }
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error in loadTariffs:', error);
        }
    },

    /**
     * Update pagination
     * به‌روزرسانی صفحه‌بندی
     */
    updatePagination: function(response) {
        console.log('🏥 MEDICAL: Updating pagination', response);
        
        try {
            const paginationContainer = $('#pagination');
            if (!paginationContainer.length) {
                console.error('🏥 MEDICAL: Pagination container not found');
                return;
            }
            
            // Clear existing pagination
            paginationContainer.empty();
            
            const currentPage = response.currentPage || 1;
            const totalPages = response.totalPages || 1;
            const hasNextPage = response.hasNextPage || false;
            const hasPreviousPage = response.hasPreviousPage || false;
            
            // Previous button
            if (hasPreviousPage) {
                paginationContainer.append(`
                    <li class="page-item">
                        <a class="page-link" href="#" onclick="window.MedicalSupplementaryTariff.loadTariffs(${currentPage - 1}); return false;">
                            <i class="fas fa-chevron-right"></i>
                        </a>
                    </li>
                `);
            } else {
                paginationContainer.append(`
                    <li class="page-item disabled">
                        <span class="page-link">
                            <i class="fas fa-chevron-right"></i>
                        </span>
                    </li>
                `);
            }
            
            // Page numbers
            const startPage = Math.max(1, currentPage - 2);
            const endPage = Math.min(totalPages, currentPage + 2);
            
            for (let i = startPage; i <= endPage; i++) {
                const isActive = i === currentPage ? 'active' : '';
                paginationContainer.append(`
                    <li class="page-item ${isActive}">
                        <a class="page-link" href="#" onclick="window.MedicalSupplementaryTariff.loadTariffs(${i}); return false;">
                            ${i}
                        </a>
                    </li>
                `);
            }
            
            // Next button
            if (hasNextPage) {
                paginationContainer.append(`
                    <li class="page-item">
                        <a class="page-link" href="#" onclick="window.MedicalSupplementaryTariff.loadTariffs(${currentPage + 1}); return false;">
                            <i class="fas fa-chevron-left"></i>
                        </a>
                    </li>
                `);
            } else {
                paginationContainer.append(`
                    <li class="page-item disabled">
                        <span class="page-link">
                            <i class="fas fa-chevron-left"></i>
                        </span>
                    </li>
                `);
            }
            
            // Page info
            paginationContainer.append(`
                <li class="page-item disabled">
                    <span class="page-link">
                        صفحه ${currentPage} از ${totalPages}
                    </span>
                </li>
            `);
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error updating pagination:', error);
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
    },

    /**
     * Edit tariff
     * ویرایش تعرفه
     */
    editTariff: function(tariffId) {
        console.log('🏥 MEDICAL: Editing tariff', tariffId);
        
        try {
            if (!tariffId) {
                if (typeof window.MedicalUI !== 'undefined') {
                    window.MedicalUI.showError('شناسه تعرفه نامعتبر است');
                }
                return;
            }

            // Redirect to edit page
            window.location.href = '/Admin/SupplementaryTariff/Edit/' + tariffId;
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error in editTariff:', error);
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.showError('خطا در ویرایش تعرفه: ' + error.message);
            }
        }
    },

    /**
     * Delete tariff
     * حذف تعرفه
     */
    deleteTariff: function(tariffId) {
        console.log('🏥 MEDICAL: Deleting tariff', tariffId);
        
        try {
            if (!tariffId) {
                if (typeof window.MedicalUI !== 'undefined') {
                    window.MedicalUI.showError('شناسه تعرفه نامعتبر است');
                }
                return;
            }

            // Show confirmation dialog
            if (confirm('آیا از حذف این تعرفه اطمینان دارید؟')) {
                // Show loading
                if (typeof window.MedicalUI !== 'undefined') {
                    window.MedicalUI.showLoading();
                }

                // Make API call
                if (typeof window.MedicalAPI !== 'undefined') {
                    window.MedicalAPI.deleteTariff(tariffId)
                        .then(response => {
                            if (typeof window.MedicalUI !== 'undefined') {
                                window.MedicalUI.hideLoading();
                                
                                if (response.success) {
                                    window.MedicalUI.showSuccess('تعرفه با موفقیت حذف شد');
                                    // Reload data
                                    this.loadInitialData();
                                } else {
                                    window.MedicalUI.showError('خطا در حذف تعرفه: ' + response.message);
                                }
                            }
                        })
                        .catch(error => {
                            if (typeof window.MedicalUI !== 'undefined') {
                                window.MedicalUI.hideLoading();
                                window.MedicalUI.showError('خطا در حذف تعرفه: ' + error.message);
                            }
                        });
                }
            }
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error in deleteTariff:', error);
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.showError('خطا در حذف تعرفه: ' + error.message);
            }
        }
    },

    /**
     * Refresh data
     * بروزرسانی داده‌ها
     */
    refreshData: function() {
        console.log('🏥 MEDICAL: Refreshing data');
        
        try {
            // Show loading
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.showLoading();
            }
            
            // Get current filter values
            var formData = $('#filterForm').serialize();
            
            // Make AJAX request to refresh table
            $.ajax({
                url: '/Admin/SupplementaryTariff/GetTariffsTable',
                type: 'GET',
                data: formData,
                success: function(response) {
                    console.log('🏥 MEDICAL: Data refreshed successfully');
                    
                    // FIX: مطابق با AJAX_RESPONSE_CHECKLIST_CONTRACT - بررسی نوع پاسخ
                    let parsedResponse;
                    try {
                        if (typeof response === 'string') {
                            parsedResponse = JSON.parse(response);
                        } else {
                            parsedResponse = response;
                        }
                    } catch (e) {
                        console.error('🏥 MEDICAL: Error parsing response:', e);
                        if (typeof window.MedicalUI !== 'undefined') {
                            window.MedicalUI.showError('خطا در پردازش پاسخ سرور');
                        }
                        return;
                    }
                    
                    // بررسی موفقیت پاسخ
                    if (parsedResponse && parsedResponse.success) {
                        // Update table content
                        $('#tariffsTableContainer').html(parsedResponse.data || response);
                        
                        // Hide loading
                        if (typeof window.MedicalUI !== 'undefined') {
                            window.MedicalUI.hideLoading();
                            window.MedicalUI.showSuccess(parsedResponse.message || 'داده‌ها با موفقیت بروزرسانی شد');
                        }
                    } else {
                        // نمایش خطا در صورت عدم موفقیت
                        if (typeof window.MedicalUI !== 'undefined') {
                            window.MedicalUI.hideLoading();
                            window.MedicalUI.showError(parsedResponse.message || 'خطا در دریافت داده‌ها');
                        }
                    }
                },
                error: function(xhr, status, error) {
                    console.error('🏥 MEDICAL: Error refreshing data:', error);
                    
                    // Hide loading
                    if (typeof window.MedicalUI !== 'undefined') {
                        window.MedicalUI.hideLoading();
                        window.MedicalUI.showError('خطا در بروزرسانی داده‌ها: ' + error);
                    }
                }
            });
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error refreshing data:', error);
            if (typeof window.MedicalUI !== 'undefined') {
                window.MedicalUI.hideLoading();
                window.MedicalUI.showError('خطا در بروزرسانی داده‌ها: ' + error.message);
            }
        }
    }
};

    // Initialize when document is ready
    $(document).ready(function() {
        console.log('🏥 MEDICAL: Document ready, initializing Supplementary Tariff Management');
        
        try {
            // Initialize the module
            window.MedicalSupplementaryTariff.init();
            
            // Debug: Check if global functions are available
            console.log('🏥 MEDICAL: Global functions check:');
            console.log('- editTariff available:', typeof window.editTariff === 'function');
            console.log('- deleteTariff available:', typeof window.deleteTariff === 'function');
            console.log('- MedicalSupplementaryTariff available:', typeof window.MedicalSupplementaryTariff === 'object');
            console.log('- MedicalSupplementaryTariff.editTariff available:', typeof window.MedicalSupplementaryTariff.editTariff === 'function');
            
        } catch (error) {
            console.error('🏥 MEDICAL: Error in document ready:', error);
        }
    });

})();

// Global function aliases for HTML onclick events - OUTSIDE IIFE
window.createNewPlan = function() {
    if (typeof window.MedicalSupplementaryTariff !== 'undefined' && 
        typeof window.MedicalSupplementaryTariff.createNewPlan === 'function') {
        window.MedicalSupplementaryTariff.createNewPlan();
    }
};

window.createNewTariff = function() {
    if (typeof window.MedicalSupplementaryTariff !== 'undefined' && 
        typeof window.MedicalSupplementaryTariff.createNewTariff === 'function') {
        window.MedicalSupplementaryTariff.createNewTariff();
    }
};

window.saveNewTariff = function() {
    if (typeof window.MedicalSupplementaryTariff !== 'undefined' && 
        typeof window.MedicalSupplementaryTariff.saveNewTariff === 'function') {
        window.MedicalSupplementaryTariff.saveNewTariff();
    }
};

window.applyFilters = function() {
    if (typeof window.MedicalSupplementaryTariff !== 'undefined' && 
        typeof window.MedicalSupplementaryTariff.applyFilters === 'function') {
        window.MedicalSupplementaryTariff.applyFilters();
    }
};

window.clearFilters = function() {
    if (typeof window.MedicalSupplementaryTariff !== 'undefined' && 
        typeof window.MedicalSupplementaryTariff.clearFilters === 'function') {
        window.MedicalSupplementaryTariff.clearFilters();
    }
};

// Global functions for edit and delete operations - OUTSIDE IIFE
window.editTariff = function(tariffId) {
    console.log('🏥 MEDICAL: Global editTariff called with ID:', tariffId);
    try {
        if (typeof window.MedicalSupplementaryTariff !== 'undefined' && 
            typeof window.MedicalSupplementaryTariff.editTariff === 'function') {
            window.MedicalSupplementaryTariff.editTariff(tariffId);
        } else {
            console.error('🏥 MEDICAL: MedicalSupplementaryTariff.editTariff not available');
            // Fallback: direct redirect
            window.location.href = '/Admin/SupplementaryTariff/Edit/' + tariffId;
        }
    } catch (error) {
        console.error('🏥 MEDICAL: Error in global editTariff:', error);
        // Fallback: direct redirect
        window.location.href = '/Admin/SupplementaryTariff/Edit/' + tariffId;
    }
};

window.deleteTariff = function(tariffId) {
    console.log('🏥 MEDICAL: Global deleteTariff called with ID:', tariffId);
    try {
        if (typeof window.MedicalSupplementaryTariff !== 'undefined' && 
            typeof window.MedicalSupplementaryTariff.deleteTariff === 'function') {
            window.MedicalSupplementaryTariff.deleteTariff(tariffId);
        } else {
            console.error('🏥 MEDICAL: MedicalSupplementaryTariff.deleteTariff not available');
            // Fallback: show error
            alert('خطا در حذف تعرفه. لطفاً صفحه را مجدداً بارگذاری کنید.');
        }
    } catch (error) {
        console.error('🏥 MEDICAL: Error in global deleteTariff:', error);
        alert('خطا در حذف تعرفه. لطفاً صفحه را مجدداً بارگذاری کنید.');
    }
};

// Initialize Real-time Validation when DOM is ready
// راه‌اندازی اعتبارسنجی لحظه‌ای هنگام آماده بودن DOM
$(document).ready(function() {
    console.log('🏥 MEDICAL: Initializing Real-time Validation...');
    
    // Load Real-time Validation module
    if (typeof window.MedicalRealTimeValidation !== 'undefined') {
        window.MedicalRealTimeValidation.init();
        console.log('🏥 MEDICAL: Real-time Validation initialized successfully');
    } else {
        console.warn('🏥 MEDICAL: MedicalRealTimeValidation module not loaded');
    }
    
    // Load Message Manager module
    if (typeof window.MedicalMessageManager !== 'undefined') {
        window.MedicalMessageManager.init();
        console.log('🏥 MEDICAL: Message Manager initialized successfully');
    } else {
        console.warn('🏥 MEDICAL: MedicalMessageManager module not loaded');
    }
    
    // Load UX Enhancer module
    if (typeof window.MedicalUXEnhancer !== 'undefined') {
        window.MedicalUXEnhancer.init();
        console.log('🏥 MEDICAL: UX Enhancer initialized successfully');
    } else {
        console.warn('🏥 MEDICAL: MedicalUXEnhancer module not loaded');
    }
    
    // Setup form-specific real-time validation
    setupFormRealTimeValidation();
});

/**
 * Setup form-specific real-time validation
 * تنظیم اعتبارسنجی لحظه‌ای مخصوص فرم
 */
function setupFormRealTimeValidation() {
    // Enhanced calculation preview updates
    $(document).on('input', '#TariffPrice, #PatientShare, #InsurerShare, #SupplementaryCoveragePercent', function() {
        invokeCalculationPreview();
    });
    
    // Form submission validation
    $(document).on('submit', '#createTariffForm, #editTariffForm', function(e) {
        if (!validateFormSubmission($(this))) {
            e.preventDefault();
            return false;
        }
    });
}

/**
 * Update calculation preview in real-time
 * به‌روزرسانی پیش‌نمایش محاسبات به صورت لحظه‌ای
 */
function defaultUpdateCalculationPreview() {
    try {
        const tariffPrice = parseFloat($('#TariffPrice').val() || 0);
        const patientShare = parseFloat($('#PatientShare').val() || 0);
        const insurerShare = parseFloat($('#InsurerShare').val() || 0);
        const coveragePercent = parseFloat($('#SupplementaryCoveragePercent').val() || 0);
        
        if (tariffPrice > 0) {
            // دریافت اطلاعات بیمه پایه از ViewBag
            const primaryPlans = window.primaryInsurancePlans || [];
            const selectedPrimaryPlanId = $('#primaryInsurancePlanId').val();
            const selectedPrimaryPlan = primaryPlans.find(plan => plan.InsurancePlanId == selectedPrimaryPlanId);
            
            // دریافت درصد پوشش بیمه پایه (داینامیک)
            const primaryCoveragePercent = selectedPrimaryPlan ? 
                (selectedPrimaryPlan.CoveragePercent || 70) : 70;
            
            // دریافت فرانشیز بیمه پایه (داینامیک)
            const primaryDeductible = selectedPrimaryPlan ? 
                (selectedPrimaryPlan.Deductible || 0) : 0;
            
            // محاسبه مبلغ قابل پوشش (بعد از کسر فرانشیز)
            const coverableAmount = Math.max(0, tariffPrice - primaryDeductible);
            
            // محاسبه بیمه پایه (درصد داینامیک)
            const primaryInsuranceCoverage = coverableAmount * (primaryCoveragePercent / 100);
            
            // محاسبه مبلغ باقی‌مانده بعد از بیمه پایه
            const remainingAfterPrimary = Math.max(0, coverableAmount - primaryInsuranceCoverage);
            
            // محاسبه بیمه تکمیلی (درصد داینامیک از مبلغ باقی‌مانده)
            const supplementaryCoverage = remainingAfterPrimary * (coveragePercent / 100);
            
            // سهم نهایی بیمار = فرانشیز + (مبلغ باقی‌مانده - بیمه تکمیلی)
            const finalPatientShare = primaryDeductible + Math.max(0, remainingAfterPrimary - supplementaryCoverage);
            
            // Update preview with formatted numbers
            $('#previewServiceAmount').text(formatCurrency(tariffPrice));
            $('#previewPrimaryCoverage').text(formatCurrency(primaryInsuranceCoverage));
            $('#previewRemainingAmount').text(formatCurrency(remainingAfterPrimary));
            $('#previewSupplementaryPercent').text(coveragePercent + '%');
            $('#previewFinalPatientShare').text(formatCurrency(finalPatientShare));
            
            // به‌روزرسانی فیلدهای فرم
            $('#InsurerShare').val(primaryInsuranceCoverage.toFixed(2));
            $('#PatientShare').val(finalPatientShare.toFixed(2));
            
            // Log for debugging
            console.log('🏥 MEDICAL: محاسبات داینامیک - PrimaryCoveragePercent:', primaryCoveragePercent, 
                'PrimaryDeductible:', primaryDeductible, 'CoverableAmount:', coverableAmount,
                'PrimaryCoverage:', primaryInsuranceCoverage, 'Remaining:', remainingAfterPrimary,
                'SupplementaryCoverage:', supplementaryCoverage, 'FinalPatientShare:', finalPatientShare);
            
            // Validate consistency
            validateCalculationConsistency(tariffPrice, finalPatientShare, primaryInsuranceCoverage, primaryCoveragePercent);
        }
    } catch (error) {
        console.error('🏥 MEDICAL: Error updating calculation preview:', error);
    }
}
function invokeCalculationPreview() {
    const previewFn = typeof window.updateCalculationPreview === 'function'
        ? window.updateCalculationPreview
        : defaultUpdateCalculationPreview;

    previewFn();
}

if (typeof window.updateCalculationPreview !== 'function') {
    window.updateCalculationPreview = defaultUpdateCalculationPreview;
}


/**
 * Validate calculation consistency
 * اعتبارسنجی سازگاری محاسبات
 */
function validateCalculationConsistency(tariffPrice, patientShare, primaryInsuranceCoverage, primaryCoveragePercent) {
    // محاسبه صحیح با درصد داینامیک
    const expectedPrimaryCoverage = tariffPrice * (primaryCoveragePercent / 100);
    const remainingAfterPrimary = tariffPrice - expectedPrimaryCoverage;
    const expectedPatientShare = remainingAfterPrimary; // 100% بیمه تکمیلی = 0 سهم بیمار
    
    const primaryDifference = Math.abs(primaryInsuranceCoverage - expectedPrimaryCoverage);
    const patientDifference = Math.abs(patientShare - expectedPatientShare);
    
    if (primaryDifference > 0.01 || patientDifference > 0.01) {
        showCalculationWarning(`⚠️ محاسبات بیمه پایه (${primaryCoveragePercent}%) و تکمیلی سازگار نیستند`);
    } else {
        hideCalculationWarning();
    }
}

/**
 * Show calculation warning
 * نمایش هشدار محاسبات
 */
function showCalculationWarning(message) {
    let $warning = $('.calculation-warning');
    if ($warning.length === 0) {
        $('.calculation-preview').append('<div class="calculation-warning alert alert-warning" style="display: none;"></div>');
        $warning = $('.calculation-warning');
    }
    $warning.html(message).slideDown();
}

/**
 * Hide calculation warning
 * مخفی کردن هشدار محاسبات
 */
function hideCalculationWarning() {
    $('.calculation-warning').slideUp();
}

/**
 * Format currency for display
 * فرمت کردن ارز برای نمایش
 */
function formatCurrency(amount) {
    return new Intl.NumberFormat('fa-IR').format(amount) + ' تومان';
}

/**
 * Validate form submission
 * اعتبارسنجی ارسال فرم
 */
function validateFormSubmission($form) {
    let isValid = true;
    const errors = [];
    
    // Check required fields
    $form.find('[required]').each(function() {
        if (!$(this).val() || $(this).val().trim() === '') {
            isValid = false;
            const fieldName = $(this).attr('name') || $(this).attr('id');
            errors.push(`فیلد ${fieldName} الزامی است`);
        }
    });
    
    // Check numeric fields
    $form.find('input[type="number"]').each(function() {
        const value = parseFloat($(this).val());
        const min = parseFloat($(this).attr('min'));
        const max = parseFloat($(this).attr('max'));
        
        if (!isNaN(value)) {
            if (!isNaN(min) && value < min) {
                isValid = false;
                errors.push(`مقدار ${$(this).attr('name')} باید حداقل ${min} باشد`);
            }
            if (!isNaN(max) && value > max) {
                isValid = false;
                errors.push(`مقدار ${$(this).attr('name')} باید حداکثر ${max} باشد`);
            }
        }
    });
    
    // Show errors if any
    if (!isValid) {
        showFormErrors(errors);
    }
    
    return isValid;
}

/**
 * Show form errors
 * نمایش خطاهای فرم
 */
function showFormErrors(errors) {
    // Use Message Manager if available
    if (typeof window.MedicalMessageManager !== 'undefined') {
        window.MedicalMessageManager.showValidationErrors(errors);
        return;
    }
    
    // Fallback to traditional method
    const errorHtml = errors.map(error => `<li>${error}</li>`).join('');
    const $errorContainer = $('.form-error-container');
    
    if ($errorContainer.length === 0) {
        $('.medical-form').prepend('<div class="form-error-container alert alert-danger" style="display: none;"></div>');
    }
    
    $('.form-error-container')
        .html(`<ul class="mb-0">${errorHtml}</ul>`)
        .slideDown();
}

/**
 * Show success message
 * نمایش پیام موفقیت
 */
function showSuccessMessage(message) {
    if (typeof window.MedicalMessageManager !== 'undefined') {
        window.MedicalMessageManager.showFormSuccess(message);
    } else {
        // Fallback to alert
        alert(message || 'عملیات با موفقیت انجام شد.');
    }
}

/**
 * Show error message
 * نمایش پیام خطا
 */
function showErrorMessage(message) {
    if (typeof window.MedicalMessageManager !== 'undefined') {
        window.MedicalMessageManager.showFormError(message);
    } else {
        // Fallback to alert
        alert(message || 'خطا در انجام عملیات.');
    }
}

/**
 * Show warning message
 * نمایش پیام هشدار
 */
function showWarningMessage(message) {
    if (typeof window.MedicalMessageManager !== 'undefined') {
        window.MedicalMessageManager.warning(message);
    } else {
        // Fallback to alert
        alert(message || 'هشدار!');
    }
}

/**
 * Show info message
 * نمایش پیام اطلاعات
 */
function showInfoMessage(message) {
    if (typeof window.MedicalMessageManager !== 'undefined') {
        window.MedicalMessageManager.info(message);
    } else {
        // Fallback to alert
        alert(message || 'اطلاعات');
    }
}

/**
 * Show loading message
 * نمایش پیام بارگذاری
 */
function showLoadingMessage(message) {
    if (typeof window.MedicalMessageManager !== 'undefined') {
        return window.MedicalMessageManager.loading(message || 'در حال پردازش...');
    }
    return null;
}

/**
 * Hide loading message
 * مخفی کردن پیام بارگذاری
 */
function hideLoadingMessage(messageId) {
    if (typeof window.MedicalMessageManager !== 'undefined' && messageId) {
        window.MedicalMessageManager.hideLoading(messageId);
    }
}
