/**
 * Medical UI Module
 * ماژول رابط کاربری محیط درمانی
 * 
 * @author ClinicApp Medical Team
 * @version 1.0.0
 * @description UI management for medical environment
 */

(function() {
    'use strict';
    
    // Global MedicalUI object
    window.MedicalUI = {
    
    /**
     * Initialize UI components
     * راه‌اندازی اجزای رابط کاربری
     */
    init: function() {
        console.log(MedicalConfig.logPrefix + ' Initializing UI components');
        
        try {
            // Initialize tooltips
            this.initTooltips();
            
            // Initialize modals
            this.initModals();
            
            // Initialize form validation
            this.initFormValidation();
            
            console.log(MedicalConfig.logPrefix + ' UI components initialized successfully');
            
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error initializing UI:', error);
            throw error;
        }
    },

    /**
     * Initialize tooltips
     * راه‌اندازی tooltip ها
     */
    initTooltips: function() {
        try {
            $('[data-toggle="tooltip"]').tooltip();
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error initializing tooltips:', error);
        }
    },

    /**
     * Initialize modals
     * راه‌اندازی مودال‌ها
     */
    initModals: function() {
        try {
            // Initialize create tariff modal
            $('#createTariffModal').on('show.bs.modal', function() {
                console.log(MedicalConfig.logPrefix + ' Create tariff modal opening');
            });
            
            // Initialize edit modal
            $('#editModal').on('show.bs.modal', function() {
                console.log(MedicalConfig.logPrefix + ' Edit modal opening');
            });
            
            // Initialize delete modal
            $('#deleteModal').on('show.bs.modal', function() {
                console.log(MedicalConfig.logPrefix + ' Delete modal opening');
            });
            
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error initializing modals:', error);
        }
    },

    /**
     * Initialize form validation
     * راه‌اندازی اعتبارسنجی فرم
     */
    initFormValidation: function() {
        try {
            // Real-time validation
            $('.form-control').on('blur', function() {
                MedicalUI.validateField($(this));
            });
            
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error initializing form validation:', error);
        }
    },

    /**
     * Show create tariff modal
     * نمایش مودال ایجاد تعرفه
     */
    showCreateTariffModal: function() {
        try {
            $('#createTariffModal').modal('show');
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error showing create tariff modal:', error);
            throw error;
        }
    },

    /**
     * Hide create tariff modal
     * مخفی کردن مودال ایجاد تعرفه
     */
    hideCreateTariffModal: function() {
        try {
            $('#createTariffModal').modal('hide');
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error hiding create tariff modal:', error);
        }
    },

    /**
     * Reset create tariff form
     * بازنشانی فرم ایجاد تعرفه
     */
    resetCreateTariffForm: function() {
        try {
            $('#createTariffForm')[0].reset();
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error resetting create tariff form:', error);
        }
    },

    /**
     * Get filter values
     * دریافت مقادیر فیلتر
     */
    getFilterValues: function() {
        try {
            return {
                searchTerm: $('#searchTerm').val() || '',
                insurancePlanId: $('#insurancePlanId').val() || '',
                departmentId: $('#departmentId').val() || '',
                isActive: $('#isActive').val() || ''
            };
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error getting filter values:', error);
            return {};
        }
    },

    /**
     * Clear filter inputs
     * پاک کردن ورودی‌های فیلتر
     */
    clearFilterInputs: function() {
        try {
            $('#searchTerm').val('');
            $('#insurancePlanId').val('');
            $('#departmentId').val('');
            $('#isActive').val('');
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error clearing filter inputs:', error);
        }
    },

    /**
     * Update statistics display
     * به‌روزرسانی نمایش آمار
     */
    updateStatisticsDisplay: function(stats) {
        console.log(MedicalConfig.logPrefix + ' Updating statistics display', stats);
        
        try {
            if (stats) {
                $('#totalServices').text(stats.TotalServices || 0);
                $('#totalTariffs').text(stats.TotalSupplementaryTariffs || 0);
                $('#activeTariffs').text(stats.ActiveSupplementaryTariffs || 0);
                $('#expiredTariffs').text(stats.ExpiredSupplementaryTariffs || 0);
                
                console.log(MedicalConfig.logPrefix + ' Statistics display updated');
            }
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error updating statistics display:', error);
        }
    },

    /**
     * Update tariffs display
     * به‌روزرسانی نمایش تعرفه‌ها
     */
    updateTariffsDisplay: function(tariffs) {
        console.log(MedicalConfig.logPrefix + ' Updating tariffs display', tariffs);
        
        try {
            const tableBody = $('#tariffsTableBody');
            if (!tableBody.length) {
                console.error(MedicalConfig.logPrefix + ' Table body not found');
                return;
            }

            if (!tariffs || tariffs.length === 0) {
                tableBody.html(`
                    <tr>
                        <td colspan="10" class="text-center text-muted">
                            <i class="fas fa-info-circle me-2"></i>
                            هیچ تعرفه‌ای یافت نشد
                        </td>
                    </tr>
                `);
                console.log(MedicalConfig.logPrefix + ' No tariffs to display');
                return;
            }

            // Clear existing content
            tableBody.empty();

            // Generate table rows
            tariffs.forEach(function(tariff) {
                const row = `
                    <tr data-tariff-id="${tariff.tariffId}">
                        <td>
                            <div class="fw-bold">${tariff.serviceName || 'نامشخص'}</div>
                            <small class="text-muted">${tariff.serviceCode || ''}</small>
                        </td>
                        <td>
                            <div class="fw-bold">${tariff.planName || 'نامشخص'}</div>
                        </td>
                        <td>
                            <span class="currency-format">${(tariff.tariffPrice || 0).toLocaleString()} تومان</span>
                        </td>
                        <td>
                            <span class="currency-format">${(tariff.patientShare || 0).toLocaleString()} تومان</span>
                        </td>
                        <td>
                            <span class="currency-format">${(tariff.insurerShare || 0).toLocaleString()} تومان</span>
                        </td>
                        <td>
                            <span class="fw-bold">${(tariff.supplementaryCoveragePercent || 0).toFixed(2)}%</span>
                        </td>
                        <td>
                            <span class="badge badge-info">${tariff.priority || 0}</span>
                        </td>
                        <td>
                            <span class="badge ${tariff.isActive ? 'badge-success' : 'badge-secondary'}">
                                ${tariff.isActive ? 'فعال' : 'غیرفعال'}
                            </span>
                        </td>
                        <td>
                            <span class="text-muted">${tariff.createdAt || 'نامشخص'}</span>
                        </td>
                        <td>
                            <div class="btn-group" role="group">
                                <button type="button" class="btn btn-sm btn-outline-primary" 
                                        onclick="editTariff(${tariff.tariffId})"
                                        title="ویرایش">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button type="button" class="btn btn-sm btn-outline-danger" 
                                        onclick="deleteTariff(${tariff.tariffId})"
                                        title="حذف">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                `;
                tableBody.append(row);
            });

            console.log(MedicalConfig.logPrefix + ' Tariffs display updated');
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error updating tariffs display:', error);
        }
    },

    /**
     * Update insurance plans filter
     * به‌روزرسانی فیلتر طرح‌های بیمه
     */
    updateInsurancePlansFilter: function(plans) {
        console.log(MedicalConfig.logPrefix + ' Updating insurance plans filter', plans);
        
        try {
            const select = $('#insurancePlanId');
            select.empty();
            select.append('<option value="">همه طرح‌ها</option>');
            
            if (plans && plans.length > 0) {
                plans.forEach(function(plan) {
                    select.append(`<option value="${plan.id}">${plan.name}</option>`);
                });
            }
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error updating insurance plans filter:', error);
        }
    },

    /**
     * Update departments filter
     * به‌روزرسانی فیلتر دپارتمان‌ها
     */
    updateDepartmentsFilter: function(departments) {
        console.log(MedicalConfig.logPrefix + ' Updating departments filter', departments);
        
        try {
            const select = $('#departmentId');
            select.empty();
            select.append('<option value="">همه دپارتمان‌ها</option>');
            
            if (departments && departments.length > 0) {
                departments.forEach(function(dept) {
                    select.append(`<option value="${dept.id}">${dept.name}</option>`);
                });
            }
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error updating departments filter:', error);
        }
    },

    /**
     * Show loading indicator
     * نمایش نشانگر بارگذاری
     */
    showLoading: function() {
        console.log(MedicalConfig.logPrefix + ' Showing loading indicator');
        
        try {
            // Remove existing loading overlay
            $('#loadingOverlay').remove();
            
            // Add loading overlay
            const loadingHtml = `
                <div id="loadingOverlay" class="loading-overlay">
                    <div class="loading-spinner">
                        <i class="fas fa-spinner fa-spin"></i>
                        <p>در حال بارگذاری...</p>
                    </div>
                </div>
            `;
            
            $('body').append(loadingHtml);
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error showing loading:', error);
        }
    },

    /**
     * Hide loading indicator
     * مخفی کردن نشانگر بارگذاری
     */
    hideLoading: function() {
        console.log(MedicalConfig.logPrefix + ' Hiding loading indicator');
        
        try {
            $('#loadingOverlay').remove();
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error hiding loading:', error);
        }
    },

    /**
     * Show success message
     * نمایش پیام موفقیت
     */
    showSuccess: function(message) {
        console.log(MedicalConfig.logPrefix + ' Success:', message);
        
        try {
            // Remove existing success messages
            $('.alert-medical-success').remove();
            
            // Show success in UI
            const successHtml = `
                <div class="alert alert-medical alert-medical-success">
                    <i class="fas fa-check-circle"></i>
                    <strong>موفق:</strong> ${message}
                </div>
            `;
            
            // Add success to page
            $('.container-fluid').prepend(successHtml);
            
            // Auto-hide after configured timeout
            setTimeout(() => {
                $('.alert-medical-success').fadeOut();
            }, MedicalConfig.successMessageTimeout);
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error showing success:', error);
        }
    },

    /**
     * Show error message
     * نمایش پیام خطا
     */
    showError: function(message) {
        console.error(MedicalConfig.logPrefix + ' Error:', message);
        
        try {
            // Remove existing error messages
            $('.alert-medical-danger').remove();
            
            // Show error in UI
            const errorHtml = `
                <div class="alert alert-medical alert-medical-danger">
                    <i class="fas fa-exclamation-triangle"></i>
                    <strong>خطا:</strong> ${message}
                </div>
            `;
            
            // Add error to page
            $('.container-fluid').prepend(errorHtml);
            
            // Auto-hide after configured timeout
            setTimeout(() => {
                $('.alert-medical-danger').fadeOut();
            }, MedicalConfig.errorMessageTimeout);
        } catch (error) {
            console.error(MedicalConfig.logPrefix + ' Error showing error message:', error);
        }
    },

    /**
     * Validate field
     * اعتبارسنجی فیلد
     */
    validateField: function(field) {
        try {
            const value = field.val();
            const fieldName = field.attr('name');
            
            // Remove existing validation classes
            field.removeClass('is-valid is-invalid');
            
            // Validate based on field type
            switch (fieldName) {
                case 'TariffPrice':
                case 'PatientShare':
                case 'InsurerShare':
                    if (value && (isNaN(value) || parseFloat(value) < 0)) {
                        field.addClass('is-invalid');
                        return false;
                    }
                    break;
                case 'CoveragePercent':
                    if (value && (isNaN(value) || parseFloat(value) < 0 || parseFloat(value) > 100)) {
                        field.addClass('is-invalid');
                        return false;
                    }
                    break;
            }
            
            if (value) {
                field.addClass('is-valid');
            }
            
            return true;
        } catch (error) {
            console.error(window.MedicalConfig.logPrefix + ' Error validating field:', error);
            return false;
        }
    }
    };
    
})();
