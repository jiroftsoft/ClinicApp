/**
 * PAYMENT PROCESSING MODULE - ماژول پردازش پرداخت
 * ========================================
 * 
 * این ماژول مسئولیت‌های زیر را دارد:
 * - محاسبه مبلغ نهایی
 * - انتخاب روش پرداخت
 * - پردازش پرداخت
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
            refreshPayment: '/Reception/Payment/Refresh',
            processPayment: '/Reception/Payment/Process',
            savePayment: '/Reception/Payment/Save'
        },
        
        // Validation Rules
        validation: {
            ajaxTimeout: 20000,
            maxRetries: 3,
            retryDelay: 1000
        },
        
        // UI Selectors
        selectors: {
            paymentMethod: '#paymentMethod',
            totalAmount: '#totalAmount',
            discountAmount: '#discountAmount',
            finalAmount: '#finalAmount',
            refreshBtn: '#refreshPaymentBtn',
            processBtn: '#processPaymentBtn',
            saveBtn: '#savePaymentBtn',
            statusMessage: '#paymentStatusMessage',
            loadingIndicator: '#paymentLoadingIndicator'
        },
        
        // Payment Methods
        paymentMethods: {
            CASH: 'cash',
            CARD: 'card',
            INSURANCE: 'insurance',
            MIXED: 'mixed'
        },
        
        // Messages
        messages: {
            info: {
                refreshing: 'در حال به‌روزرسانی اطلاعات پرداخت...',
                processing: 'در حال پردازش پرداخت...'
            },
            success: {
                paymentRefreshed: 'اطلاعات پرداخت با موفقیت به‌روزرسانی شد',
                paymentProcessed: 'پرداخت با موفقیت انجام شد',
                paymentSaved: 'اطلاعات پرداخت با موفقیت ذخیره شد'
            },
            error: {
                refreshError: 'خطا در به‌روزرسانی اطلاعات پرداخت',
                processError: 'خطا در پردازش پرداخت',
                saveError: 'خطا در ذخیره اطلاعات پرداخت',
                networkError: 'خطا در اتصال به شبکه'
            }
        }
    };

    // ========================================
    // PAYMENT PROCESSING MODULE - ماژول اصلی
    // ========================================
    var PaymentProcessingModule = {
        
        // ========================================
        // INITIALIZATION - راه‌اندازی
        // ========================================
        init: function() {
            console.log('[PaymentProcessingModule] Initializing...');
            
            try {
                this.bindEvents();
                this.setupEventListeners();
                this.initializePaymentMethods();
                this.initializeAmounts();
                
                console.log('[PaymentProcessingModule] Initialized successfully');
            } catch (error) {
                console.error('[PaymentProcessingModule] Initialization failed:', error);
                this.handleError(error, 'initialization');
            }
        },

        // ========================================
        // EVENT BINDING - اتصال رویدادها
        // ========================================
        bindEvents: function() {
            var self = this;
            
            // Refresh Payment Button
            $(CONFIG.selectors.refreshBtn).off('click.paymentProcessing')
                .on('click.paymentProcessing', function() {
                    self.handleRefreshPayment.call(this, self);
                });
            
            // Process Payment Button
            $(CONFIG.selectors.processBtn).off('click.paymentProcessing')
                .on('click.paymentProcessing', function() {
                    self.handleProcessPayment.call(this, self);
                });
            
            // Save Payment Button
            $(CONFIG.selectors.saveBtn).off('click.paymentProcessing')
                .on('click.paymentProcessing', function() {
                    self.handleSavePayment.call(this, self);
                });
            
            // Payment Method Change
            $(CONFIG.selectors.paymentMethod).off('change.paymentProcessing')
                .on('change.paymentProcessing', function() {
                    self.handlePaymentMethodChange.call(this, self);
                });
        },

        // ========================================
        // EVENT LISTENERS SETUP - تنظیم شنوندگان رویداد
        // ========================================
        setupEventListeners: function() {
            var self = this;
            
            // گوش دادن به رویدادهای سیستم
            if (window.ReceptionEventBus) {
                window.ReceptionEventBus.on('services:calculated', function(data) {
                    self.handleServicesCalculated(data);
                });
                
                window.ReceptionEventBus.on('system:error', function(error) {
                    self.handleSystemError(error);
                });
            }
        },

        // ========================================
        // PAYMENT METHODS INITIALIZATION - راه‌اندازی روش‌های پرداخت
        // ========================================
        initializePaymentMethods: function() {
            try {
                const $paymentMethod = $(CONFIG.selectors.paymentMethod);
                
                // اضافه کردن روش‌های پرداخت
                $paymentMethod.empty();
                $paymentMethod.append('<option value="">انتخاب روش پرداخت</option>');
                $paymentMethod.append(`<option value="${CONFIG.paymentMethods.CASH}">نقدی</option>`);
                $paymentMethod.append(`<option value="${CONFIG.paymentMethods.CARD}">کارت</option>`);
                $paymentMethod.append(`<option value="${CONFIG.paymentMethods.INSURANCE}">بیمه</option>`);
                $paymentMethod.append(`<option value="${CONFIG.paymentMethods.MIXED}">ترکیبی</option>`);
                
            } catch (error) {
                console.error('[PaymentProcessingModule] Payment methods initialization failed:', error);
                this.handleError(error, 'paymentMethodsInitialization');
            }
        },

        // ========================================
        // AMOUNTS INITIALIZATION - راه‌اندازی مبالغ
        // ========================================
        initializeAmounts: function() {
            try {
                $(CONFIG.selectors.totalAmount).text('0 ریال');
                $(CONFIG.selectors.discountAmount).text('0 ریال');
                $(CONFIG.selectors.finalAmount).text('0 ریال');
                
            } catch (error) {
                console.error('[PaymentProcessingModule] Amounts initialization failed:', error);
                this.handleError(error, 'amountsInitialization');
            }
        },

        // ========================================
        // SERVICES CALCULATED HANDLER - مدیریت محاسبه خدمات
        // ========================================
        handleServicesCalculated: function(data) {
            console.log('[PaymentProcessingModule] Services calculated, updating payment amounts');
            this.updatePaymentAmounts(data);
        },

        // ========================================
        // UPDATE PAYMENT AMOUNTS - به‌روزرسانی مبالغ پرداخت
        // ========================================
        updatePaymentAmounts: function(data) {
            try {
                if (data.totalAmount) {
                    $(CONFIG.selectors.totalAmount).text(this.formatCurrency(data.totalAmount));
                }
                
                if (data.discountAmount) {
                    $(CONFIG.selectors.discountAmount).text(this.formatCurrency(data.discountAmount));
                }
                
                if (data.finalAmount) {
                    $(CONFIG.selectors.finalAmount).text(this.formatCurrency(data.finalAmount));
                }
                
                // فعال کردن دکمه‌ها
                $(CONFIG.selectors.refreshBtn).prop('disabled', false);
                $(CONFIG.selectors.processBtn).prop('disabled', false);
                
            } catch (error) {
                console.error('[PaymentProcessingModule] Error updating payment amounts:', error);
                this.handleError(error, 'updatePaymentAmounts');
            }
        },

        // ========================================
        // PAYMENT METHOD CHANGE HANDLER - مدیریت تغییر روش پرداخت
        // ========================================
        handlePaymentMethodChange: function(module) {
            var paymentMethod = $(this).val();
            module.updatePaymentMethodUI(paymentMethod);
        },

        // ========================================
        // UPDATE PAYMENT METHOD UI - به‌روزرسانی UI روش پرداخت
        // ========================================
        updatePaymentMethodUI: function(paymentMethod) {
            try {
                // مخفی کردن همه بخش‌های اضافی
                $('.payment-method-section').addClass('d-none');
                
                // نمایش بخش مربوط به روش پرداخت انتخاب شده
                switch (paymentMethod) {
                    case CONFIG.paymentMethods.CASH:
                        $('#cashPaymentSection').removeClass('d-none');
                        break;
                    case CONFIG.paymentMethods.CARD:
                        $('#cardPaymentSection').removeClass('d-none');
                        break;
                    case CONFIG.paymentMethods.INSURANCE:
                        $('#insurancePaymentSection').removeClass('d-none');
                        break;
                    case CONFIG.paymentMethods.MIXED:
                        $('#mixedPaymentSection').removeClass('d-none');
                        break;
                }
                
            } catch (error) {
                console.error('[PaymentProcessingModule] Error updating payment method UI:', error);
                this.handleError(error, 'updatePaymentMethodUI');
            }
        },

        // ========================================
        // REFRESH PAYMENT HANDLER - مدیریت به‌روزرسانی پرداخت
        // ========================================
        handleRefreshPayment: function(module) {
            const $btn = $(this);
            module.refreshPayment($btn);
        },

        // ========================================
        // REFRESH PAYMENT - به‌روزرسانی پرداخت
        // ========================================
        refreshPayment: function($btn) {
            this.showButtonLoading($btn);
            this.showInfo(CONFIG.messages.info.refreshing);
            this.showLoadingIndicator(true);

            var self = this;
            
            $.ajax({
                url: CONFIG.endpoints.refreshPayment,
                type: 'POST',
                data: { 
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                dataType: 'json',
                timeout: CONFIG.validation.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.hideButtonLoading($btn);
                    self.hideLoadingIndicator();
                    self.handleRefreshPaymentSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideButtonLoading($btn);
                    self.hideLoadingIndicator();
                    self.handleRefreshPaymentError(xhr, status, error);
                }
            });
        },

        // ========================================
        // REFRESH PAYMENT SUCCESS HANDLER - مدیریت موفقیت به‌روزرسانی پرداخت
        // ========================================
        handleRefreshPaymentSuccess: function(response) {
            if (response.success && response.data) {
                this.updatePaymentAmounts(response.data);
                this.showSuccess(CONFIG.messages.success.paymentRefreshed);
            } else {
                this.showError(response.message || CONFIG.messages.error.refreshError);
            }
        },

        // ========================================
        // REFRESH PAYMENT ERROR HANDLER - مدیریت خطای به‌روزرسانی پرداخت
        // ========================================
        handleRefreshPaymentError: function(xhr, status, error) {
            console.error('[PaymentProcessingModule] Error refreshing payment:', error);
            this.showError(CONFIG.messages.error.refreshError);
        },

        // ========================================
        // PROCESS PAYMENT HANDLER - مدیریت پردازش پرداخت
        // ========================================
        handleProcessPayment: function(module) {
            var $btn = $(this);
            var paymentMethod = $(CONFIG.selectors.paymentMethod).val();
            
            if (!paymentMethod) {
                module.showError('لطفا روش پرداخت را انتخاب کنید');
                return;
            }
            
            module.processPayment(paymentMethod, $btn);
        },

        // ========================================
        // PROCESS PAYMENT - پردازش پرداخت
        // ========================================
        processPayment: function(paymentMethod, $btn) {
            this.showButtonLoading($btn);
            this.showInfo(CONFIG.messages.info.processing);
            this.showLoadingIndicator(true);
            
            var self = this;
            
            $.ajax({
                url: CONFIG.endpoints.processPayment,
                type: 'POST',
                data: { 
                    paymentMethod: paymentMethod,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                dataType: 'json',
                timeout: CONFIG.validation.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.hideButtonLoading($btn);
                    self.hideLoadingIndicator();
                    self.handleProcessPaymentSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideButtonLoading($btn);
                    self.hideLoadingIndicator();
                    self.handleProcessPaymentError(xhr, status, error);
                }
            });
        },

        // ========================================
        // PROCESS PAYMENT SUCCESS HANDLER - مدیریت موفقیت پردازش پرداخت
        // ========================================
        handleProcessPaymentSuccess: function(response) {
            if (response.success && response.data) {
                this.displayPaymentResults(response.data);
                this.showSuccess(CONFIG.messages.success.paymentProcessed);
                this.updateAccordionState('paymentSection', 'completed');
            } else {
                this.showError(response.message || CONFIG.messages.error.processError);
            }
        },

        // ========================================
        // PROCESS PAYMENT ERROR HANDLER - مدیریت خطای پردازش پرداخت
        // ========================================
        handleProcessPaymentError: function(xhr, status, error) {
            console.error('[PaymentProcessingModule] Error processing payment:', error);
            this.showError(CONFIG.messages.error.processError);
        },

        // ========================================
        // DISPLAY PAYMENT RESULTS - نمایش نتایج پرداخت
        // ========================================
        displayPaymentResults: function(results) {
            try {
                // نمایش نتایج پرداخت
                if (results.transactionId) {
                    $('#transactionId').text(results.transactionId);
                }
                
                if (results.paymentStatus) {
                    $('#paymentStatus').text(results.paymentStatus);
                }
                
                if (results.paymentDate) {
                    $('#paymentDate').text(results.paymentDate);
                }
                
            } catch (error) {
                console.error('[PaymentProcessingModule] Error displaying payment results:', error);
                this.handleError(error, 'displayPaymentResults');
            }
        },

        // ========================================
        // SAVE PAYMENT HANDLER - مدیریت ذخیره پرداخت
        // ========================================
        handleSavePayment: function(module) {
            var $btn = $(this);
            var paymentData = module.getPaymentData();
            
            if (!paymentData.paymentMethod) {
                module.showError('لطفا روش پرداخت را انتخاب کنید');
                return;
            }
            
            module.savePayment(paymentData, $btn);
        },

        // ========================================
        // GET PAYMENT DATA - دریافت اطلاعات پرداخت
        // ========================================
        getPaymentData: function() {
            return {
                paymentMethod: $(CONFIG.selectors.paymentMethod).val(),
                totalAmount: this.parseAmount($(CONFIG.selectors.totalAmount).text()),
                discountAmount: this.parseAmount($(CONFIG.selectors.discountAmount).text()),
                finalAmount: this.parseAmount($(CONFIG.selectors.finalAmount).text())
            };
        },

        // ========================================
        // PARSE AMOUNT - تجزیه مبلغ
        // ========================================
        parseAmount: function(amountText) {
            return parseFloat(amountText.replace(/[^\d]/g, '')) || 0;
        },

        // ========================================
        // SAVE PAYMENT - ذخیره پرداخت
        // ========================================
        savePayment: function(paymentData, $btn) {
            this.showButtonLoading($btn);
            
            var self = this;
            
            $.ajax({
                url: CONFIG.endpoints.savePayment,
                type: 'POST',
                data: { 
                    paymentData: paymentData,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                dataType: 'json',
                timeout: CONFIG.validation.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.hideButtonLoading($btn);
                    self.handleSavePaymentSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideButtonLoading($btn);
                    self.handleSavePaymentError(xhr, status, error);
                }
            });
        },

        // ========================================
        // SAVE PAYMENT SUCCESS HANDLER - مدیریت موفقیت ذخیره پرداخت
        // ========================================
        handleSavePaymentSuccess: function(response) {
            if (response.success) {
                this.showSuccess(CONFIG.messages.success.paymentSaved);
                this.updateAccordionState('paymentSection', 'completed');
            } else {
                this.showError(response.message || CONFIG.messages.error.saveError);
            }
        },

        // ========================================
        // SAVE PAYMENT ERROR HANDLER - مدیریت خطای ذخیره پرداخت
        // ========================================
        handleSavePaymentError: function(xhr, status, error) {
            console.error('[PaymentProcessingModule] Error saving payment:', error);
            this.showError(CONFIG.messages.error.saveError);
        },

        // ========================================
        // SYSTEM ERROR HANDLER - مدیریت خطای سیستم
        // ========================================
        handleSystemError: function(error) {
            console.error('[PaymentProcessingModule] System error:', error);
        },

        // ========================================
        // UI HELPER METHODS - متدهای کمکی UI
        // ========================================
        showButtonLoading: function($btn) {
            if (!$btn || $btn.length === 0) {
                console.error('[PaymentProcessingModule] Button element is null or undefined');
                return;
            }
            
            try {
                $btn.prop('disabled', true);
                $btn.find('.btn-text').addClass('d-none');
                $btn.find('.btn-loading').removeClass('d-none');
            } catch (error) {
                console.error('[PaymentProcessingModule] Error showing button loading:', error);
            }
        },

        hideButtonLoading: function($btn) {
            if (!$btn || $btn.length === 0) {
                console.error('[PaymentProcessingModule] Button element is null or undefined');
                return;
            }
            
            try {
                $btn.prop('disabled', false);
                $btn.find('.btn-text').removeClass('d-none');
                $btn.find('.btn-loading').addClass('d-none');
            } catch (error) {
                console.error('[PaymentProcessingModule] Error hiding button loading:', error);
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
                console.error('[PaymentProcessingModule] Error:', message);
            }
        },

        showSuccess: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showSuccess(message);
            } else {
                console.log('[PaymentProcessingModule] Success:', message);
            }
        },

        showInfo: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showInfo(message);
            } else {
                console.log('[PaymentProcessingModule] Info:', message);
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
            console.error(`[PaymentProcessingModule] Error in ${context}:`, error);
            
            if (window.ReceptionErrorHandler && typeof window.ReceptionErrorHandler.handle === 'function') {
                window.ReceptionErrorHandler.handle(error, 'PaymentProcessingModule', context);
            }
        }
    };

    // ========================================
    // MODULE EXPORT - صادرات ماژول
    // ========================================
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = PaymentProcessingModule;
    } else {
        global.PaymentProcessingModule = PaymentProcessingModule;
    }

})(window, jQuery);
