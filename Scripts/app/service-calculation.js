/**
 * Service Calculation JavaScript Library
 * کتابخانه محاسبات خدمات برای محیط درمانی
 * 
 * ویژگی‌های کلیدی:
 * 1. محاسبات داینامیک قیمت خدمات
 * 2. دریافت ضرایب از سرور
 * 3. نمایش جزئیات محاسبات
 * 4. مدیریت خطاها
 * 5. UI/UX بهینه
 */

(function ($) {
    'use strict';

    // Namespace برای Service Calculation
    window.ServiceCalculation = {
        
        // تنظیمات پیش‌فرض
        config: {
            baseUrl: '/Reception',
            timeout: 30000,
            retryAttempts: 3,
            animationDuration: 300
        },

        // Cache برای ضرایب
        cache: {
            factors: {},
            services: {},
            lastUpdate: null
        },

        /**
         * محاسبه قیمت خدمت با ServiceComponents
         */
        calculateServicePrice: function (serviceId, calculationDate, callback) {
            var self = this;
            var requestData = {
                serviceId: serviceId,
                calculationDate: calculationDate || new Date().toISOString()
            };

            $.ajax({
                url: self.config.baseUrl + '/CalculateServicePriceWithComponents',
                type: 'POST',
                data: requestData,
                timeout: self.config.timeout,
                success: function (response) {
                    if (response.success) {
                        // Cache کردن نتیجه
                        self.cache.services[serviceId] = {
                            data: response,
                            timestamp: Date.now()
                        };
                        
                        if (callback) callback(null, response);
                    } else {
                        if (callback) callback(new Error(response.message), null);
                    }
                },
                error: function (xhr, status, error) {
                    var errorMessage = 'خطا در محاسبه قیمت خدمت: ' + error;
                    if (callback) callback(new Error(errorMessage), null);
                }
            });
        },

        /**
         * دریافت جزئیات محاسبه خدمت
         */
        getServiceCalculationDetails: function (serviceId, calculationDate, callback) {
            var self = this;
            var requestData = {
                serviceId: serviceId,
                calculationDate: calculationDate || new Date().toISOString()
            };

            $.ajax({
                url: self.config.baseUrl + '/GetServiceCalculationDetails',
                type: 'POST',
                data: requestData,
                timeout: self.config.timeout,
                success: function (response) {
                    if (response.success) {
                        if (callback) callback(null, response);
                    } else {
                        if (callback) callback(new Error(response.message), null);
                    }
                },
                error: function (xhr, status, error) {
                    var errorMessage = 'خطا در دریافت جزئیات محاسبه: ' + error;
                    if (callback) callback(new Error(errorMessage), null);
                }
            });
        },

        /**
         * بررسی وضعیت اجزای خدمت
         */
        getServiceComponentsStatus: function (serviceId, callback) {
            var self = this;
            
            $.ajax({
                url: self.config.baseUrl + '/GetServiceComponentsStatus',
                type: 'GET',
                data: { serviceId: serviceId },
                timeout: self.config.timeout,
                success: function (response) {
                    if (response.success) {
                        if (callback) callback(null, response);
                    } else {
                        if (callback) callback(new Error(response.message), null);
                    }
                },
                error: function (xhr, status, error) {
                    var errorMessage = 'خطا در بررسی وضعیت اجزای خدمت: ' + error;
                    if (callback) callback(new Error(errorMessage), null);
                }
            });
        },

        /**
         * نمایش جزئیات محاسبه در UI
         */
        displayCalculationDetails: function (containerId, calculationData) {
            var container = $('#' + containerId);
            if (!container.length) return;

            var html = '<div class="calculation-details">';
            html += '<h6><i class="fas fa-calculator"></i> جزئیات محاسبه</h6>';
            html += '<div class="row">';
            
            // اطلاعات خدمت
            html += '<div class="col-md-6">';
            html += '<div class="info-item">';
            html += '<label>خدمت:</label>';
            html += '<span>' + calculationData.serviceTitle + '</span>';
            html += '</div>';
            html += '<div class="info-item">';
            html += '<label>کد خدمت:</label>';
            html += '<span><code>' + calculationData.serviceCode + '</code></span>';
            html += '</div>';
            html += '</div>';

            // جزئیات محاسبه
            html += '<div class="col-md-6">';
            html += '<div class="info-item">';
            html += '<label>قیمت کل:</label>';
            html += '<span class="price-total">' + this.formatPrice(calculationData.calculatedPrice) + '</span>';
            html += '</div>';
            html += '<div class="info-item">';
            html += '<label>تاریخ محاسبه:</label>';
            html += '<span>' + this.formatDate(calculationData.calculationDate) + '</span>';
            html += '</div>';
            html += '</div>';

            html += '</div>';

            // اجزای خدمت
            if (calculationData.components && calculationData.components.length > 0) {
                html += '<div class="components-section">';
                html += '<h6><i class="fas fa-cogs"></i> اجزای خدمت</h6>';
                html += '<div class="table-responsive">';
                html += '<table class="table table-sm">';
                html += '<thead><tr><th>نوع جزء</th><th>ضریب</th></tr></thead>';
                html += '<tbody>';
                
                calculationData.components.forEach(function (component) {
                    html += '<tr>';
                    html += '<td>' + component.ComponentTypeName + '</td>';
                    html += '<td>' + component.Coefficient + '</td>';
                    html += '</tr>';
                });
                
                html += '</tbody></table>';
                html += '</div>';
                html += '</div>';
            }

            html += '</div>';

            container.html(html).fadeIn(this.config.animationDuration);
        },

        /**
         * نمایش وضعیت اجزای خدمت
         */
        displayComponentsStatus: function (containerId, statusData) {
            var container = $('#' + containerId);
            if (!container.length) return;

            var statusClass = statusData.isComplete ? 'success' : 'warning';
            var statusIcon = statusData.isComplete ? 'check-circle' : 'exclamation-triangle';
            var statusText = statusData.isComplete ? 'کامل' : 'ناقص';

            var html = '<div class="components-status">';
            html += '<div class="status-indicator ' + statusClass + '">';
            html += '<i class="fas fa-' + statusIcon + '"></i>';
            html += '<span>' + statusText + '</span>';
            html += '</div>';

            html += '<div class="status-details">';
            html += '<div class="row">';
            html += '<div class="col-md-6">';
            html += '<div class="status-item">';
            html += '<label>جزء فنی:</label>';
            html += '<span class="' + (statusData.hasTechnicalComponent ? 'text-success' : 'text-danger') + '">';
            html += statusData.hasTechnicalComponent ? 'موجود' : 'مفقود';
            html += '</span>';
            html += '</div>';
            html += '</div>';
            html += '<div class="col-md-6">';
            html += '<div class="status-item">';
            html += '<label>جزء حرفه‌ای:</label>';
            html += '<span class="' + (statusData.hasProfessionalComponent ? 'text-success' : 'text-danger') + '">';
            html += statusData.hasProfessionalComponent ? 'موجود' : 'مفقود';
            html += '</span>';
            html += '</div>';
            html += '</div>';
            html += '</div>';

            if (statusData.technicalCoefficient > 0 || statusData.professionalCoefficient > 0) {
                html += '<div class="coefficients">';
                html += '<h6>ضرایب:</h6>';
                html += '<div class="row">';
                html += '<div class="col-md-6">';
                html += '<span>فنی: ' + statusData.technicalCoefficient + '</span>';
                html += '</div>';
                html += '<div class="col-md-6">';
                html += '<span>حرفه‌ای: ' + statusData.professionalCoefficient + '</span>';
                html += '</div>';
                html += '</div>';
                html += '</div>';
            }

            html += '</div>';
            html += '</div>';

            container.html(html).fadeIn(this.config.animationDuration);
        },

        /**
         * نمایش خطا در UI
         */
        displayError: function (containerId, errorMessage) {
            var container = $('#' + containerId);
            if (!container.length) return;

            var html = '<div class="alert alert-danger">';
            html += '<i class="fas fa-exclamation-triangle"></i>';
            html += '<strong>خطا:</strong> ' + errorMessage;
            html += '</div>';

            container.html(html).fadeIn(this.config.animationDuration);
        },

        /**
         * نمایش Loading
         */
        displayLoading: function (containerId) {
            var container = $('#' + containerId);
            if (!container.length) return;

            var html = '<div class="text-center">';
            html += '<div class="spinner-border text-primary" role="status">';
            html += '<span class="sr-only">در حال بارگذاری...</span>';
            html += '</div>';
            html += '<p class="mt-2">در حال محاسبه...</p>';
            html += '</div>';

            container.html(html).fadeIn(this.config.animationDuration);
        },

        /**
         * فرمت کردن قیمت
         */
        formatPrice: function (price) {
            return new Intl.NumberFormat('fa-IR', {
                style: 'currency',
                currency: 'IRR',
                minimumFractionDigits: 0,
                maximumFractionDigits: 0
            }).format(price);
        },

        /**
         * فرمت کردن تاریخ
         */
        formatDate: function (dateString) {
            var date = new Date(dateString);
            return date.toLocaleDateString('fa-IR');
        },

        /**
         * پاک کردن Cache
         */
        clearCache: function () {
            this.cache = {
                factors: {},
                services: {},
                lastUpdate: null
            };
        },

        /**
         * بررسی اعتبار Cache
         */
        isCacheValid: function (key, maxAge) {
            maxAge = maxAge || 300000; // 5 دقیقه پیش‌فرض
            var cached = this.cache[key];
            return cached && (Date.now() - cached.timestamp) < maxAge;
        }
    };

    // jQuery Plugin برای استفاده آسان
    $.fn.serviceCalculation = function (options) {
        var settings = $.extend({}, ServiceCalculation.config, options);
        
        return this.each(function () {
            var $this = $(this);
            var serviceId = $this.data('service-id');
            
            if (serviceId) {
                ServiceCalculation.displayLoading($this.attr('id'));
                
                ServiceCalculation.calculateServicePrice(serviceId, null, function (error, result) {
                    if (error) {
                        ServiceCalculation.displayError($this.attr('id'), error.message);
                    } else {
                        ServiceCalculation.displayCalculationDetails($this.attr('id'), result);
                    }
                });
            }
        });
    };

})(jQuery);

// Auto-initialize برای عناصر با کلاس service-calculation
$(document).ready(function () {
    $('.service-calculation').serviceCalculation();
});
