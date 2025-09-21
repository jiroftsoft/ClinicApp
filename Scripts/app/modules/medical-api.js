/**
 * Medical API Module
 * ماژول API محیط درمانی
 * 
 * @author ClinicApp Medical Team
 * @version 1.0.0
 * @description API management for medical environment
 */

(function() {
    'use strict';
    
    // Global MedicalAPI object
    window.MedicalAPI = {
    
    /**
     * Initialize MedicalAPI
     * راه‌اندازی MedicalAPI
     */
    init: function() {
        console.log(window.MedicalConfig.logPrefix + ' MedicalAPI initialized successfully');
        return true;
    },
    
    /**
     * Load statistics
     * بارگذاری آمار
     */
    loadStatistics: function() {
        console.log(MedicalConfig.logPrefix + ' Loading Statistics');
        
        try {
            // Check if stats are already available in ViewBag
            if (typeof ViewBag !== 'undefined' && ViewBag.Stats) {
                console.log(MedicalConfig.logPrefix + ' Using ViewBag stats');
                MedicalUI.updateStatisticsDisplay(ViewBag.Stats);
                return Promise.resolve(ViewBag.Stats);
            }
            
            // Make AJAX call
            return this.makeRequest('GetStats', 'GET')
                .then(response => {
                    console.log(MedicalConfig.logPrefix + ' Statistics loaded successfully', response);
                    if (response.success && response.data) {
                        MedicalUI.updateStatisticsDisplay(response.data);
                    } else {
                        MedicalUI.showError('خطا در بارگذاری آمار: ' + response.message);
                    }
                    return response;
                })
                .catch(error => {
                    console.error(MedicalConfig.logPrefix + ' Error loading statistics:', error);
                    MedicalUI.showError('خطا در بارگذاری آمار: ' + error);
                    throw error;
                });
            
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error in loadStatistics:', error);
            MedicalUI.showError('خطا در بارگذاری آمار: ' + error.message);
            return Promise.reject(error);
        }
    },

    /**
     * Load tariffs
     * بارگذاری تعرفه‌ها
     */
    loadTariffs: function(filterData = {}) {
        console.log(MedicalConfig.logPrefix + ' Loading Tariffs');
        
        try {
            return this.makeRequest('GetTariffs', 'GET', filterData)
                .then(response => {
                    console.log(MedicalConfig.logPrefix + ' Tariffs loaded successfully', response);
                    if (response.success && response.data) {
                        MedicalUI.updateTariffsDisplay(response.data);
                    } else {
                        MedicalUI.showError('خطا در بارگذاری تعرفه‌ها: ' + response.message);
                    }
                    return response;
                })
                .catch(error => {
                    console.error(MedicalConfig.logPrefix + ' Error loading tariffs:', error);
                    MedicalUI.showError('خطا در بارگذاری تعرفه‌ها: ' + error);
                    throw error;
                });
            
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error in loadTariffs:', error);
            MedicalUI.showError('خطا در بارگذاری تعرفه‌ها: ' + error.message);
            return Promise.reject(error);
        }
    },

    /**
     * Get tariffs with filters
     * دریافت تعرفه‌ها با فیلتر
     */
    getTariffs: function(filterData) {
        console.log(MedicalConfig.logPrefix + ' Getting Tariffs with filters', filterData);
        
        try {
            return this.makeRequest('GetTariffs', 'GET', filterData)
                .then(response => {
                    console.log(MedicalConfig.logPrefix + ' Filtered tariffs loaded successfully', response);
                    return response;
                })
                .catch(error => {
                    console.error(MedicalConfig.logPrefix + ' Error getting filtered tariffs:', error);
                    throw error;
                });
            
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error in getTariffs:', error);
            return Promise.reject(error);
        }
    },

    /**
     * Create tariff
     * ایجاد تعرفه
     */
    createTariff: function(tariffData) {
        console.log(MedicalConfig.logPrefix + ' Creating Tariff', tariffData);
        
        try {
            return this.makeRequest('CreateTariff', 'POST', tariffData)
                .then(response => {
                    console.log(MedicalConfig.logPrefix + ' Tariff created successfully', response);
                    return response;
                })
                .catch(error => {
                    console.error(MedicalConfig.logPrefix + ' Error creating tariff:', error);
                    throw error;
                });
            
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error in createTariff:', error);
            return Promise.reject(error);
        }
    },

    /**
     * Update tariff
     * به‌روزرسانی تعرفه
     */
    updateTariff: function(tariffData) {
        console.log(MedicalConfig.logPrefix + ' Updating Tariff', tariffData);
        
        try {
            return this.makeRequest('EditTariff', 'POST', tariffData)
                .then(response => {
                    console.log(MedicalConfig.logPrefix + ' Tariff updated successfully', response);
                    return response;
                })
                .catch(error => {
                    console.error(MedicalConfig.logPrefix + ' Error updating tariff:', error);
                    throw error;
                });
            
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error in updateTariff:', error);
            return Promise.reject(error);
        }
    },

    /**
     * Delete tariff
     * حذف تعرفه
     */
    deleteTariff: function(tariffId) {
        console.log(MedicalConfig.logPrefix + ' Deleting Tariff', tariffId);
        
        try {
            return this.makeRequest('DeleteTariff', 'POST', { id: tariffId })
                .then(response => {
                    console.log(MedicalConfig.logPrefix + ' Tariff deleted successfully', response);
                    return response;
                })
                .catch(error => {
                    console.error(MedicalConfig.logPrefix + ' Error deleting tariff:', error);
                    throw error;
                });
            
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error in deleteTariff:', error);
            return Promise.reject(error);
        }
    },

    /**
     * Load filter options
     * بارگذاری گزینه‌های فیلتر
     */
    loadFilterOptions: function() {
        console.log(MedicalConfig.logPrefix + ' Loading Filter Options');
        
        try {
            // Load insurance plans
            this.makeRequest('GetInsurancePlans', 'GET')
                .then(response => {
                    if (response.success && response.data) {
                        MedicalUI.updateInsurancePlansFilter(response.data);
                    }
                })
                .catch(error => {
                    console.error(MedicalConfig.logPrefix + ' Error loading insurance plans:', error);
                });
            
            // Load departments
            this.makeRequest('GetDepartments', 'GET')
                .then(response => {
                    if (response.success && response.data) {
                        MedicalUI.updateDepartmentsFilter(response.data);
                    }
                })
                .catch(error => {
                    console.error(MedicalConfig.logPrefix + ' Error loading departments:', error);
                });
            
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error in loadFilterOptions:', error);
            MedicalUI.showError('خطا در بارگذاری گزینه‌های فیلتر: ' + error.message);
        }
    },

    /**
     * Make HTTP request
     * انجام درخواست HTTP
     */
    makeRequest: function(endpoint, method, data = {}) {
        console.log(MedicalConfig.logPrefix + ' Making request:', endpoint, method, data);
        
        try {
            const requestOptions = {
                url: MedicalConfig.apiBaseUrl + endpoint,
                type: method,
                dataType: 'json',
                timeout: MedicalConfig.requestTimeout
            };
            
            if (method === 'POST' && Object.keys(data).length > 0) {
                requestOptions.data = data;
            } else if (method === 'GET' && Object.keys(data).length > 0) {
                requestOptions.data = data;
            }
            
            return $.ajax(requestOptions)
                .then(response => {
                    console.log(MedicalConfig.logPrefix + ' Request successful:', response);
                    return response;
                })
                .catch(error => {
                    console.error(MedicalConfig.logPrefix + ' Request failed:', error);
                    throw error;
                });
            
        } catch (error) {
            console.error(window.MedicalConfig.logPrefix + ' Error in makeRequest:', error);
            return Promise.reject(error);
        }
    }
    };
    
})();
