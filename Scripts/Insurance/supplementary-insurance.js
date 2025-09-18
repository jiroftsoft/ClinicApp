/**
 * JavaScript برای مدیریت بیمه تکمیلی
 * طراحی شده برای سیستم‌های پزشکی کلینیک شفا
 * 
 * ویژگی‌های کلیدی:
 * 1. محاسبه بیمه تکمیلی
 * 2. مدیریت تعرفه‌های تکمیلی
 * 3. تنظیمات پیشرفته
 * 4. اعتبارسنجی فرم‌ها
 * 5. رابط کاربری تعاملی
 */

// Namespace برای بیمه تکمیلی
var SupplementaryInsurance = (function() {
    'use strict';
    
    // تنظیمات پیش‌فرض
    var config = {
        apiBaseUrl: '/Admin/Insurance/CombinedInsuranceCalculation/',
        toastrOptions: {
            positionClass: 'toast-top-left',
            timeOut: 5000,
            closeButton: true,
            progressBar: true
        },
        validationRules: {
            minCoveragePercent: 0,
            maxCoveragePercent: 100,
            minPayment: 0,
            maxPayment: 999999999999,
            minDeductible: 0
        }
    };
    
    // متدهای عمومی
    return {
        // محاسبه بیمه تکمیلی
        calculate: function(patientId, serviceId, serviceAmount, primaryCoverage, calculationDate) {
            return new Promise(function(resolve, reject) {
                var requestData = {
                    patientId: patientId,
                    serviceId: serviceId,
                    serviceAmount: serviceAmount,
                    primaryCoverage: primaryCoverage,
                    calculationDate: calculationDate || new Date().toISOString().split('T')[0]
                };
                
                $.ajax({
                    url: config.apiBaseUrl + 'CalculateSupplementary',
                    type: 'POST',
                    data: requestData,
                    beforeSend: function() {
                        SupplementaryInsurance.showLoading();
                    },
                    success: function(response) {
                        SupplementaryInsurance.hideLoading();
                        if (response.success) {
                            resolve(response.data);
                        } else {
                            reject(new Error(response.message || 'خطا در محاسبه بیمه تکمیلی'));
                        }
                    },
                    error: function(xhr, status, error) {
                        SupplementaryInsurance.hideLoading();
                        reject(new Error('خطا در ارتباط با سرور: ' + error));
                    }
                });
            });
        },
        
        // دریافت تعرفه‌های تکمیلی
        getTariffs: function(planId) {
            return new Promise(function(resolve, reject) {
                $.ajax({
                    url: config.apiBaseUrl + 'GetSupplementaryTariffs',
                    type: 'GET',
                    data: { planId: planId },
                    success: function(response) {
                        if (response.success) {
                            resolve(response.data);
                        } else {
                            reject(new Error(response.message || 'خطا در دریافت تعرفه‌ها'));
                        }
                    },
                    error: function(xhr, status, error) {
                        reject(new Error('خطا در ارتباط با سرور: ' + error));
                    }
                });
            });
        },
        
        // ذخیره تنظیمات بیمه تکمیلی
        saveSettings: function(settings) {
            return new Promise(function(resolve, reject) {
                $.ajax({
                    url: config.apiBaseUrl + 'UpdateSupplementarySettings',
                    type: 'POST',
                    data: settings,
                    success: function(response) {
                        if (response.success) {
                            resolve(response.data);
                        } else {
                            reject(new Error(response.message || 'خطا در ذخیره تنظیمات'));
                        }
                    },
                    error: function(xhr, status, error) {
                        reject(new Error('خطا در ارتباط با سرور: ' + error));
                    }
                });
            });
        },
        
        // اعتبارسنجی فرم محاسبه
        validateCalculationForm: function(formData) {
            var errors = [];
            
            if (!formData.patientId || formData.patientId <= 0) {
                errors.push('شناسه بیمار الزامی است');
            }
            
            if (!formData.serviceId || formData.serviceId <= 0) {
                errors.push('شناسه خدمت الزامی است');
            }
            
            if (!formData.serviceAmount || formData.serviceAmount <= 0) {
                errors.push('مبلغ خدمت باید بیشتر از صفر باشد');
            }
            
            if (formData.primaryCoverage < 0) {
                errors.push('پوشش بیمه اصلی نمی‌تواند منفی باشد');
            }
            
            if (formData.primaryCoverage > formData.serviceAmount) {
                errors.push('پوشش بیمه اصلی نمی‌تواند بیشتر از مبلغ خدمت باشد');
            }
            
            return {
                isValid: errors.length === 0,
                errors: errors
            };
        },
        
        // اعتبارسنجی فرم تنظیمات
        validateSettingsForm: function(formData) {
            var errors = [];
            
            if (!formData.planName || formData.planName.trim() === '') {
                errors.push('نام طرح بیمه الزامی است');
            }
            
            if (formData.coveragePercent < config.validationRules.minCoveragePercent || 
                formData.coveragePercent > config.validationRules.maxCoveragePercent) {
                errors.push('درصد پوشش باید بین 0 تا 100 باشد');
            }
            
            if (formData.maxPayment < config.validationRules.minPayment) {
                errors.push('حداکثر مبلغ پرداخت نمی‌تواند منفی باشد');
            }
            
            if (formData.deductible < config.validationRules.minDeductible) {
                errors.push('فرانشیز نمی‌تواند منفی باشد');
            }
            
            // اعتبارسنجی JSON
            if (formData.settingsJson && formData.settingsJson.trim() !== '') {
                try {
                    JSON.parse(formData.settingsJson);
                } catch (e) {
                    errors.push('فرمت JSON نامعتبر است');
                }
            }
            
            return {
                isValid: errors.length === 0,
                errors: errors
            };
        },
        
        // نمایش نتیجه محاسبه
        displayCalculationResult: function(result) {
            var resultHtml = `
                <div class="calculation-result">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="info-box bg-info">
                                <span class="info-box-icon">
                                    <i class="fas fa-money-bill-wave"></i>
                                </span>
                                <div class="info-box-content">
                                    <span class="info-box-text">مبلغ خدمت</span>
                                    <span class="info-box-number">${this.formatCurrency(result.serviceAmount)}</span>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="info-box bg-primary">
                                <span class="info-box-icon">
                                    <i class="fas fa-shield-alt"></i>
                                </span>
                                <div class="info-box-content">
                                    <span class="info-box-text">پوشش بیمه اصلی</span>
                                    <span class="info-box-number">${this.formatCurrency(result.primaryCoverage)}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <div class="info-box bg-success">
                                <span class="info-box-icon">
                                    <i class="fas fa-plus-circle"></i>
                                </span>
                                <div class="info-box-content">
                                    <span class="info-box-text">پوشش بیمه تکمیلی</span>
                                    <span class="info-box-number">${this.formatCurrency(result.supplementaryCoverage)}</span>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="info-box bg-warning">
                                <span class="info-box-icon">
                                    <i class="fas fa-user"></i>
                                </span>
                                <div class="info-box-content">
                                    <span class="info-box-text">سهم نهایی بیمار</span>
                                    <span class="info-box-number">${this.formatCurrency(result.finalPatientShare)}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <div class="info-box bg-success">
                                <span class="info-box-icon">
                                    <i class="fas fa-chart-pie"></i>
                                </span>
                                <div class="info-box-content">
                                    <span class="info-box-text">مجموع پوشش بیمه</span>
                                    <span class="info-box-number">${this.formatCurrency(result.totalCoverage)}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="alert alert-info mt-3">
                        <i class="fas fa-info-circle"></i>
                        <strong>توضیحات:</strong> ${result.notes || '-'}
                    </div>
                </div>
            `;
            
            $('#calculationResult').html(resultHtml).show();
        },
        
        // فرمت کردن مبلغ
        formatCurrency: function(amount) {
            return new Intl.NumberFormat('fa-IR').format(amount) + ' ریال';
        },
        
        // نمایش loading
        showLoading: function() {
            if ($('.loading-overlay').length === 0) {
                $('body').append('<div class="loading-overlay"><div class="spinner-border text-primary" role="status"><span class="sr-only">در حال بارگذاری...</span></div></div>');
            }
            $('.loading-overlay').show();
        },
        
        // مخفی کردن loading
        hideLoading: function() {
            $('.loading-overlay').hide();
        },
        
        // نمایش پیام موفقیت
        showSuccess: function(message) {
            toastr.success(message, 'موفقیت', config.toastrOptions);
        },
        
        // نمایش پیام خطا
        showError: function(message) {
            toastr.error(message, 'خطا', config.toastrOptions);
        },
        
        // نمایش پیام اطلاعات
        showInfo: function(message) {
            toastr.info(message, 'اطلاعات', config.toastrOptions);
        },
        
        // نمایش پیام هشدار
        showWarning: function(message) {
            toastr.warning(message, 'هشدار', config.toastrOptions);
        },
        
        // اعتبارسنجی JSON
        validateJson: function(jsonString) {
            try {
                JSON.parse(jsonString);
                return true;
            } catch (e) {
                return false;
            }
        },
        
        // تولید تنظیمات پیش‌فرض JSON
        generateDefaultSettings: function() {
            return JSON.stringify({
                minAmountForCoverage: 100000,
                maxUsagePerYear: 10,
                waitingPeriodDays: 30,
                coverageRules: {
                    emergency: 90,
                    routine: 80,
                    preventive: 100
                },
                exclusions: [],
                specialConditions: []
            }, null, 2);
        },
        
        // محاسبه درصد پوشش موثر
        calculateEffectiveCoverage: function(baseAmount, coveragePercent, maxPayment, deductible) {
            var amountAfterDeductible = Math.max(0, baseAmount - deductible);
            var calculatedCoverage = amountAfterDeductible * (coveragePercent / 100);
            var effectiveCoverage = Math.min(calculatedCoverage, maxPayment);
            
            return {
                baseAmount: baseAmount,
                amountAfterDeductible: amountAfterDeductible,
                calculatedCoverage: calculatedCoverage,
                effectiveCoverage: effectiveCoverage,
                patientShare: baseAmount - effectiveCoverage
            };
        },
        
        // اعتبارسنجی تعرفه
        validateTariff: function(tariff) {
            var errors = [];
            
            if (!tariff.serviceId || tariff.serviceId <= 0) {
                errors.push('شناسه خدمت الزامی است');
            }
            
            if (!tariff.planId || tariff.planId <= 0) {
                errors.push('شناسه طرح بیمه الزامی است');
            }
            
            if (tariff.coveragePercent < 0 || tariff.coveragePercent > 100) {
                errors.push('درصد پوشش باید بین 0 تا 100 باشد');
            }
            
            if (tariff.maxPayment < 0) {
                errors.push('حداکثر مبلغ پرداخت نمی‌تواند منفی باشد');
            }
            
            if (tariff.deductible < 0) {
                errors.push('فرانشیز نمی‌تواند منفی باشد');
            }
            
            return {
                isValid: errors.length === 0,
                errors: errors
            };
        }
    };
})();

// CSS برای loading overlay
var supplementaryInsuranceCSS = `
    <style>
    .loading-overlay {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(0, 0, 0, 0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 9999;
    }
    
    .loading-overlay .spinner-border {
        width: 3rem;
        height: 3rem;
    }
    
    .calculation-result {
        animation: fadeIn 0.5s ease-in;
    }
    
    @keyframes fadeIn {
        from { opacity: 0; transform: translateY(20px); }
        to { opacity: 1; transform: translateY(0); }
    }
    
    .info-box {
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        transition: transform 0.2s ease;
    }
    
    .info-box:hover {
        transform: translateY(-2px);
    }
    
    .supplementary-form .form-control:focus {
        border-color: #007bff;
        box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
    }
    
    .supplementary-form .form-label {
        font-weight: 600;
        color: #495057;
    }
    
    .supplementary-form .invalid-feedback {
        display: block;
    }
    
    .supplementary-form .is-valid {
        border-color: #28a745;
    }
    
    .supplementary-form .is-invalid {
        border-color: #dc3545;
    }
    </style>
`;

// اضافه کردن CSS به صفحه
if (typeof document !== 'undefined') {
    $('head').append(supplementaryInsuranceCSS);
}

// Export برای استفاده در ماژول‌ها
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SupplementaryInsurance;
}
