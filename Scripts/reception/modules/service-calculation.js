/**
 * SERVICE CALCULATION MODULE - ماژول محاسبه خدمات
 * ========================================
 * 
 * این ماژول مسئولیت‌های زیر را دارد:
 * - بارگذاری لیست خدمات
 * - انتخاب خدمات
 * - محاسبه هزینه خدمات
 * - مدیریت خطاها و loading states
 * 
 * @author ClinicApp Development Team
 * @version 1.0.0
 * @since 2025-01-17
 */

(function(global, $) {
    'use strict';

    // ========================================
    // MODULE CONFIGURATION - تنظیمات ماژول
    // ========================================
    var CONFIG = {
        // API Endpoints
        endpoints: {
            loadServices: '/Reception/Service/Load',
            calculateServices: '/Reception/Service/Calculate',
            saveServices: '/Reception/Service/Save'
        },
        
        // Validation Rules
        validation: {
            ajaxTimeout: 15000,
            maxRetries: 3,
            retryDelay: 1000
        },
        
        // UI Selectors
        selectors: {
            serviceSelect: '#serviceSelect',
            serviceList: '#serviceList',
            calculateBtn: '#calculateServicesBtn',
            saveBtn: '#saveServicesBtn',
            totalAmount: '#totalAmount',
            statusMessage: '#serviceStatusMessage',
            loadingIndicator: '#serviceLoadingIndicator'
        },
        
        // Messages
        messages: {
            info: {
                loadingServices: 'در حال بارگذاری خدمات...',
                calculating: 'در حال محاسبه هزینه‌ها...'
            },
            success: {
                servicesLoaded: 'خدمات با موفقیت بارگذاری شدند',
                calculationCompleted: 'محاسبه با موفقیت انجام شد',
                servicesSaved: 'خدمات با موفقیت ذخیره شدند'
            },
            error: {
                loadError: 'خطا در بارگذاری خدمات',
                calculationError: 'خطا در محاسبه هزینه‌ها',
                saveError: 'خطا در ذخیره خدمات',
                networkError: 'خطا در اتصال به شبکه'
            }
        }
    };

    // ========================================
    // SERVICE CALCULATION MODULE - ماژول اصلی
    // ========================================
    var ServiceCalculationModule = {
        
        // ========================================
        // INITIALIZATION - راه‌اندازی
        // ========================================
        init: function() {
            console.log('[ServiceCalculationModule] Initializing...');
            
            try {
                this.bindEvents();
                this.setupEventListeners();
                this.initializeSelect2();
                this.initializeServiceList();
                
                console.log('[ServiceCalculationModule] Initialized successfully');
            } catch (error) {
                console.error('[ServiceCalculationModule] Initialization failed:', error);
                this.handleError(error, 'initialization');
            }
        },

        // ========================================
        // EVENT BINDING - اتصال رویدادها
        // ========================================
        bindEvents: function() {
            var self = this;
            
            // Load Services Button
            $(CONFIG.selectors.calculateBtn).off('click.serviceCalculation')
                .on('click.serviceCalculation', function() {
                    self.handleCalculateServices.call(this, self);
                });
            
            // Service Selection Change
            $(CONFIG.selectors.serviceSelect).off('change.serviceCalculation')
                .on('change.serviceCalculation', function() {
                    self.handleServiceSelection.call(this, self);
                });
            
            // Save Services Button
            $(CONFIG.selectors.saveBtn).off('click.serviceCalculation')
                .on('click.serviceCalculation', function() {
                    self.handleSaveServices.call(this, self);
                });
        },

        // ========================================
        // EVENT LISTENERS SETUP - تنظیم شنوندگان رویداد
        // ========================================
        setupEventListeners: function() {
            var self = this;
            
            // گوش دادن به رویدادهای سیستم
            if (window.ReceptionEventBus) {
                window.ReceptionEventBus.on('department:selected', function(data) {
                    self.handleDepartmentSelected(data);
                });
                
                window.ReceptionEventBus.on('system:error', function(error) {
                    self.handleSystemError(error);
                });
            }
        },

        // ========================================
        // SELECT2 INITIALIZATION - راه‌اندازی Select2
        // ========================================
        initializeSelect2: function() {
            try {
                // Initialize Service Select2
                $(CONFIG.selectors.serviceSelect).select2({
                    placeholder: 'انتخاب خدمت',
                    allowClear: true,
                    language: 'fa',
                    dir: 'rtl',
                    width: '100%'
                });
                
            } catch (error) {
                console.error('[ServiceCalculationModule] Select2 initialization failed:', error);
                this.handleError(error, 'select2Initialization');
            }
        },

        // ========================================
        // SERVICE LIST INITIALIZATION - راه‌اندازی لیست خدمات
        // ========================================
        initializeServiceList: function() {
            try {
                const $serviceList = $(CONFIG.selectors.serviceList);
                $serviceList.empty();
                $serviceList.append('<div class="text-center text-muted">هیچ خدمتی انتخاب نشده است</div>');
                
            } catch (error) {
                console.error('[ServiceCalculationModule] Service list initialization failed:', error);
                this.handleError(error, 'serviceListInitialization');
            }
        },

        // ========================================
        // DEPARTMENT SELECTED HANDLER - مدیریت انتخاب دپارتمان
        // ========================================
        handleDepartmentSelected: function(data) {
            console.log('[ServiceCalculationModule] Department selected, loading services');
            this.loadServices();
        },

        // ========================================
        // LOAD SERVICES - بارگذاری خدمات
        // ========================================
        loadServices: function() {
            this.showInfo(CONFIG.messages.info.loadingServices);
            this.showLoadingIndicator(true);

            var self = this;
            
            $.ajax({
                url: CONFIG.endpoints.loadServices,
                type: 'POST',
                data: { 
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                dataType: 'json',
                timeout: CONFIG.validation.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.hideLoadingIndicator();
                    self.handleLoadServicesSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideLoadingIndicator();
                    self.handleLoadServicesError(xhr, status, error);
                }
            });
        },

        // ========================================
        // LOAD SERVICES SUCCESS HANDLER - مدیریت موفقیت بارگذاری خدمات
        // ========================================
        handleLoadServicesSuccess: function(response) {
            if (response.success && response.data) {
                this.populateServices(response.data);
                this.showSuccess(CONFIG.messages.success.servicesLoaded);
            } else {
                this.showError(response.message || CONFIG.messages.error.loadError);
            }
        },

        // ========================================
        // LOAD SERVICES ERROR HANDLER - مدیریت خطای بارگذاری خدمات
        // ========================================
        handleLoadServicesError: function(xhr, status, error) {
            console.error('[ServiceCalculationModule] Error loading services:', error);
            this.showError(CONFIG.messages.error.loadError);
        },

        // ========================================
        // POPULATE SERVICES - پر کردن لیست خدمات
        // ========================================
        populateServices: function(services) {
            if (!services || !Array.isArray(services)) {
                console.error('[ServiceCalculationModule] Invalid services data');
                return;
            }
            
            try {
                const $select = $(CONFIG.selectors.serviceSelect);
                $select.empty();
                
                // اضافه کردن گزینه پیش‌فرض
                $select.append('<option value="">انتخاب خدمت</option>');
                
                // اضافه کردن خدمات
                services.forEach(function(service) {
                    $select.append(`<option value="${service.id}" data-price="${service.price}">${service.name} - ${service.price} ریال</option>`);
                });
                
                // به‌روزرسانی Select2
                $select.trigger('change');
                
            } catch (error) {
                console.error('[ServiceCalculationModule] Error populating services:', error);
                this.handleError(error, 'populateServices');
            }
        },

        // ========================================
        // SERVICE SELECTION HANDLER - مدیریت انتخاب خدمت
        // ========================================
        handleServiceSelection: function(module) {
            var serviceId = $(this).val();
            var serviceName = $(this).find('option:selected').text();
            var servicePrice = $(this).find('option:selected').data('price');
            
            if (serviceId && serviceName && servicePrice) {
                module.addServiceToList(serviceId, serviceName, servicePrice);
                $(this).val('').trigger('change');
            }
        },

        // ========================================
        // ADD SERVICE TO LIST - اضافه کردن خدمت به لیست
        // ========================================
        addServiceToList: function(serviceId, serviceName, servicePrice) {
            try {
                const $serviceList = $(CONFIG.selectors.serviceList);
                
                // حذف پیام پیش‌فرض
                $serviceList.find('.text-center.text-muted').remove();
                
                // ایجاد آیتم جدید
                var serviceItem = `
                    <div class="service-item d-flex justify-content-between align-items-center mb-2 p-2 border rounded" data-service-id="${serviceId}">
                        <div>
                            <span class="service-name">${serviceName}</span>
                            <span class="service-price text-success">${this.formatCurrency(servicePrice)}</span>
                        </div>
                        <button type="button" class="btn btn-sm btn-outline-danger remove-service">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                `;
                
                $serviceList.append(serviceItem);
                
                // اتصال رویداد حذف
                $serviceList.find('.remove-service').off('click').on('click', function() {
                    $(this).closest('.service-item').remove();
                    module.updateTotalAmount();
                });
                
                // به‌روزرسانی مجموع
                this.updateTotalAmount();
                
            } catch (error) {
                console.error('[ServiceCalculationModule] Error adding service to list:', error);
                this.handleError(error, 'addServiceToList');
            }
        },

        // ========================================
        // UPDATE TOTAL AMOUNT - به‌روزرسانی مجموع
        // ========================================
        updateTotalAmount: function() {
            try {
                var $serviceList = $(CONFIG.selectors.serviceList);
                var $totalAmount = $(CONFIG.selectors.totalAmount);
                
                var total = 0;
                $serviceList.find('.service-item').each(function() {
                    var priceText = $(this).find('.service-price').text();
                    var price = parseFloat(priceText.replace(/[^\d]/g, '')) || 0;
                    total += price;
                });
                
                $totalAmount.text(this.formatCurrency(total));
                
                // نمایش یا مخفی کردن دکمه‌ها
                if (total > 0) {
                    $(CONFIG.selectors.calculateBtn).prop('disabled', false);
                    $(CONFIG.selectors.saveBtn).prop('disabled', false);
                } else {
                    $(CONFIG.selectors.calculateBtn).prop('disabled', true);
                    $(CONFIG.selectors.saveBtn).prop('disabled', true);
                }
                
            } catch (error) {
                console.error('[ServiceCalculationModule] Error updating total amount:', error);
                this.handleError(error, 'updateTotalAmount');
            }
        },

        // ========================================
        // CALCULATE SERVICES HANDLER - مدیریت محاسبه خدمات
        // ========================================
        handleCalculateServices: function(module) {
            var $btn = $(this);
            var selectedServices = module.getSelectedServices();
            
            if (selectedServices.length === 0) {
                module.showError('لطفا حداقل یک خدمت انتخاب کنید');
                return;
            }
            
            module.calculateServices(selectedServices, $btn);
        },

        // ========================================
        // GET SELECTED SERVICES - دریافت خدمات انتخاب شده
        // ========================================
        getSelectedServices: function() {
            var services = [];
            $(CONFIG.selectors.serviceList).find('.service-item').each(function() {
                var serviceId = $(this).data('service-id');
                var serviceName = $(this).find('.service-name').text();
                var servicePrice = $(this).find('.service-price').text();
                
                services.push({
                    id: serviceId,
                    name: serviceName,
                    price: servicePrice
                });
            });
            
            return services;
        },

        // ========================================
        // CALCULATE SERVICES - محاسبه خدمات
        // ========================================
        calculateServices: function(services, $btn) {
            this.showButtonLoading($btn);
            this.showInfo(CONFIG.messages.info.calculating);
            
            var self = this;
            
            $.ajax({
                url: CONFIG.endpoints.calculateServices,
                type: 'POST',
                data: { 
                    services: services,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                dataType: 'json',
                timeout: CONFIG.validation.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.hideButtonLoading($btn);
                    self.handleCalculateServicesSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideButtonLoading($btn);
                    self.handleCalculateServicesError(xhr, status, error);
                }
            });
        },

        // ========================================
        // CALCULATE SERVICES SUCCESS HANDLER - مدیریت موفقیت محاسبه خدمات
        // ========================================
        handleCalculateServicesSuccess: function(response) {
            if (response.success && response.data) {
                this.displayCalculationResults(response.data);
                this.showSuccess(CONFIG.messages.success.calculationCompleted);
                this.updateAccordionState('serviceSection', 'completed');
            } else {
                this.showError(response.message || CONFIG.messages.error.calculationError);
            }
        },

        // ========================================
        // CALCULATE SERVICES ERROR HANDLER - مدیریت خطای محاسبه خدمات
        // ========================================
        handleCalculateServicesError: function(xhr, status, error) {
            console.error('[ServiceCalculationModule] Error calculating services:', error);
            this.showError(CONFIG.messages.error.calculationError);
        },

        // ========================================
        // DISPLAY CALCULATION RESULTS - نمایش نتایج محاسبه
        // ========================================
        displayCalculationResults: function(results) {
            try {
                // نمایش نتایج محاسبه
                if (results.totalAmount) {
                    $(CONFIG.selectors.totalAmount).text(this.formatCurrency(results.totalAmount));
                }
                
                // نمایش جزئیات محاسبه
                if (results.details) {
                    this.displayCalculationDetails(results.details);
                }
                
            } catch (error) {
                console.error('[ServiceCalculationModule] Error displaying calculation results:', error);
                this.handleError(error, 'displayCalculationResults');
            }
        },

        // ========================================
        // DISPLAY CALCULATION DETAILS - نمایش جزئیات محاسبه
        // ========================================
        displayCalculationDetails: function(details) {
            // TODO: Implement calculation details display
            console.log('[ServiceCalculationModule] Calculation details:', details);
        },

        // ========================================
        // SAVE SERVICES HANDLER - مدیریت ذخیره خدمات
        // ========================================
        handleSaveServices: function(module) {
            var $btn = $(this);
            var selectedServices = module.getSelectedServices();
            
            if (selectedServices.length === 0) {
                module.showError('لطفا حداقل یک خدمت انتخاب کنید');
                return;
            }
            
            module.saveServices(selectedServices, $btn);
        },

        // ========================================
        // SAVE SERVICES - ذخیره خدمات
        // ========================================
        saveServices: function(services, $btn) {
            this.showButtonLoading($btn);
            
            var self = this;
            
            $.ajax({
                url: CONFIG.endpoints.saveServices,
                type: 'POST',
                data: { 
                    services: services,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                dataType: 'json',
                timeout: CONFIG.validation.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.hideButtonLoading($btn);
                    self.handleSaveServicesSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideButtonLoading($btn);
                    self.handleSaveServicesError(xhr, status, error);
                }
            });
        },

        // ========================================
        // SAVE SERVICES SUCCESS HANDLER - مدیریت موفقیت ذخیره خدمات
        // ========================================
        handleSaveServicesSuccess: function(response) {
            if (response.success) {
                this.showSuccess(CONFIG.messages.success.servicesSaved);
                this.updateAccordionState('serviceSection', 'completed');
            } else {
                this.showError(response.message || CONFIG.messages.error.saveError);
            }
        },

        // ========================================
        // SAVE SERVICES ERROR HANDLER - مدیریت خطای ذخیره خدمات
        // ========================================
        handleSaveServicesError: function(xhr, status, error) {
            console.error('[ServiceCalculationModule] Error saving services:', error);
            this.showError(CONFIG.messages.error.saveError);
        },

        // ========================================
        // SYSTEM ERROR HANDLER - مدیریت خطای سیستم
        // ========================================
        handleSystemError: function(error) {
            console.error('[ServiceCalculationModule] System error:', error);
        },

        // ========================================
        // UI HELPER METHODS - متدهای کمکی UI
        // ========================================
        showButtonLoading: function($btn) {
            if (!$btn || $btn.length === 0) {
                console.error('[ServiceCalculationModule] Button element is null or undefined');
                return;
            }
            
            try {
                $btn.prop('disabled', true);
                $btn.find('.btn-text').addClass('d-none');
                $btn.find('.btn-loading').removeClass('d-none');
            } catch (error) {
                console.error('[ServiceCalculationModule] Error showing button loading:', error);
            }
        },

        hideButtonLoading: function($btn) {
            if (!$btn || $btn.length === 0) {
                console.error('[ServiceCalculationModule] Button element is null or undefined');
                return;
            }
            
            try {
                $btn.prop('disabled', false);
                $btn.find('.btn-text').removeClass('d-none');
                $btn.find('.btn-loading').addClass('d-none');
            } catch (error) {
                console.error('[ServiceCalculationModule] Error hiding button loading:', error);
            }
        },

        showLoadingIndicator: function(show) {
            const $indicator = $(CONFIG.selectors.loadingIndicator);
            if (show) {
                $indicator.removeClass('d-none');
            } else {
                $indicator.addClass('d-none');
            }
        },

        hideLoadingIndicator: function() {
            this.showLoadingIndicator(false);
        },

        // ========================================
        // MESSAGE DISPLAY METHODS - متدهای نمایش پیام
        // ========================================
        showError: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showError(message);
            } else {
                console.error('[ServiceCalculationModule] Error:', message);
            }
        },

        showSuccess: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showSuccess(message);
            } else {
                console.log('[ServiceCalculationModule] Success:', message);
            }
        },

        showInfo: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showInfo(message);
            } else {
                console.log('[ServiceCalculationModule] Info:', message);
            }
        },

        // ========================================
        // UTILITY METHODS - متدهای کمکی
        // ========================================
        formatCurrency: function(amount) {
            return new Intl.NumberFormat('fa-IR').format(amount) + ' ریال';
        },

        updateAccordionState: function(section, state) {
            if (window.ReceptionModules && window.ReceptionModules.Accordion) {
                window.ReceptionModules.Accordion.setState(section, state);
            }
        },

        // ========================================
        // ERROR HANDLING - مدیریت خطا
        // ========================================
        handleError: function(error, context) {
            console.error(`[ServiceCalculationModule] Error in ${context}:`, error);
            
            if (window.ReceptionErrorHandler && typeof window.ReceptionErrorHandler.handle === 'function') {
                window.ReceptionErrorHandler.handle(error, 'ServiceCalculationModule', context);
            }
        }
    };

    // ========================================
    // MODULE EXPORT - صادرات ماژول
    // ========================================
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = ServiceCalculationModule;
    } else {
        global.ServiceCalculationModule = ServiceCalculationModule;
    }

})(window, jQuery);
