/**
 * JavaScript برای مدیریت تعرفه‌های بیمه تکمیلی
 * طراحی شده برای سیستم‌های پزشکی کلینیک شفا
 * 
 * ویژگی‌های کلیدی:
 * 1. مدیریت CRUD تعرفه‌های تکمیلی
 * 2. اعتبارسنجی پیشرفته
 * 3. رابط کاربری تعاملی
 * 4. مدیریت تنظیمات JSON
 * 5. محاسبات خودکار
 */

// Namespace برای مدیریت تعرفه‌های بیمه تکمیلی
var SupplementaryTariffManager = (function() {
    'use strict';
    
    // تنظیمات پیش‌فرض
    var config = {
        apiBaseUrl: '/Admin/Insurance/CombinedInsuranceCalculation/',
        dataTableConfig: {
            language: {
                url: "//cdn.datatables.net/plug-ins/1.10.24/i18n/Persian.json"
            },
            responsive: true,
            pageLength: 25,
            order: [[0, "desc"]],
            columnDefs: [
                { "orderable": false, "targets": -1 } // آخرین ستون (عملیات)
            ]
        },
        validationRules: {
            minCoveragePercent: 0,
            maxCoveragePercent: 100,
            minPayment: 0,
            maxPayment: 999999999999,
            minDeductible: 0,
            maxDeductible: 999999999999
        }
    };
    
    // متغیرهای داخلی
    var currentPlanId = null;
    var tariffTable = null;
    
    // متدهای عمومی
    return {
        // مقداردهی اولیه
        init: function(planId) {
            currentPlanId = planId;
            this.initDataTable();
            this.bindEvents();
            this.loadTariffs();
        },
        
        // مقداردهی DataTable
        initDataTable: function() {
            if ($('#supplementaryTariffTable').length) {
                tariffTable = $('#supplementaryTariffTable').DataTable(config.dataTableConfig);
            }
        },
        
        // اتصال رویدادها
        bindEvents: function() {
            var self = this;
            
            // رویداد افزودن تعرفه جدید
            $(document).on('click', '.btn-add-tariff', function() {
                self.showAddTariffModal();
            });
            
            // رویداد ویرایش تعرفه
            $(document).on('click', '.btn-edit-tariff', function() {
                var tariffId = $(this).data('tariff-id');
                self.showEditTariffModal(tariffId);
            });
            
            // رویداد حذف تعرفه
            $(document).on('click', '.btn-delete-tariff', function() {
                var tariffId = $(this).data('tariff-id');
                self.deleteTariff(tariffId);
            });
            
            // رویداد مشاهده جزئیات
            $(document).on('click', '.btn-view-tariff', function() {
                var tariffId = $(this).data('tariff-id');
                self.showTariffDetails(tariffId);
            });
            
            // رویداد ذخیره فرم
            $(document).on('submit', '#tariffForm', function(e) {
                e.preventDefault();
                self.saveTariff();
            });
            
            // رویداد اعتبارسنجی JSON
            $(document).on('blur', '#settingsJson', function() {
                self.validateJson($(this));
            });
            
            // رویداد محاسبه خودکار
            $(document).on('input', '.coverage-input', function() {
                self.calculateEffectiveCoverage();
            });
        },
        
        // بارگذاری تعرفه‌ها
        loadTariffs: function() {
            var self = this;
            
            if (!currentPlanId) {
                this.showError('شناسه طرح بیمه مشخص نشده است');
                return;
            }
            
            $.ajax({
                url: config.apiBaseUrl + 'GetSupplementaryTariffs',
                type: 'GET',
                data: { planId: currentPlanId },
                beforeSend: function() {
                    self.showLoading();
                },
                success: function(response) {
                    self.hideLoading();
                    if (response.success) {
                        self.displayTariffs(response.data);
                    } else {
                        self.showError(response.message || 'خطا در بارگذاری تعرفه‌ها');
                    }
                },
                error: function(xhr, status, error) {
                    self.hideLoading();
                    self.showError('خطا در ارتباط با سرور: ' + error);
                }
            });
        },
        
        // نمایش تعرفه‌ها در جدول
        displayTariffs: function(tariffs) {
            if (tariffTable) {
                tariffTable.clear();
                
                tariffs.forEach(function(tariff) {
                    var row = [
                        '<span class="badge badge-info">' + tariff.tariffId + '</span>',
                        '<strong>' + tariff.serviceName + '</strong><br><small class="text-muted">شناسه: ' + tariff.serviceId + '</small>',
                        '<span class="badge badge-success">' + tariff.coveragePercent.toFixed(2) + '%</span>',
                        '<span class="text-warning font-weight-bold">' + self.formatCurrency(tariff.maxPayment) + '</span>',
                        '<span class="text-danger">' + self.formatCurrency(tariff.deductible) + '</span>',
                        tariff.useAdvancedSettings ? 
                            '<span class="badge badge-info"><i class="fas fa-check"></i> فعال</span>' : 
                            '<span class="badge badge-secondary"><i class="fas fa-times"></i> غیرفعال</span>',
                        '<span class="text-muted">' + self.formatDate(tariff.createdAt) + '</span>',
                        '<div class="btn-group" role="group">' +
                            '<button type="button" class="btn btn-sm btn-outline-primary btn-edit-tariff" data-tariff-id="' + tariff.tariffId + '" title="ویرایش"><i class="fas fa-edit"></i></button>' +
                            '<button type="button" class="btn btn-sm btn-outline-info btn-view-tariff" data-tariff-id="' + tariff.tariffId + '" title="مشاهده"><i class="fas fa-eye"></i></button>' +
                            '<button type="button" class="btn btn-sm btn-outline-danger btn-delete-tariff" data-tariff-id="' + tariff.tariffId + '" title="حذف"><i class="fas fa-trash"></i></button>' +
                        '</div>'
                    ];
                    
                    tariffTable.row.add(row);
                });
                
                tariffTable.draw();
            }
        },
        
        // نمایش مودال افزودن تعرفه
        showAddTariffModal: function() {
            var self = this;
            
            $.ajax({
                url: config.apiBaseUrl + 'CreateSupplementaryTariff',
                type: 'GET',
                data: { planId: currentPlanId },
                success: function(response) {
                    $('#tariffModalBody').html(response);
                    $('#tariffModal').modal('show');
                    self.initTariffForm();
                },
                error: function(xhr, status, error) {
                    self.showError('خطا در بارگذاری فرم: ' + error);
                }
            });
        },
        
        // نمایش مودال ویرایش تعرفه
        showEditTariffModal: function(tariffId) {
            var self = this;
            
            $.ajax({
                url: config.apiBaseUrl + 'EditSupplementaryTariff',
                type: 'GET',
                data: { id: tariffId },
                success: function(response) {
                    $('#tariffModalBody').html(response);
                    $('#tariffModal').modal('show');
                    self.initTariffForm();
                },
                error: function(xhr, status, error) {
                    self.showError('خطا در بارگذاری فرم ویرایش: ' + error);
                }
            });
        },
        
        // نمایش جزئیات تعرفه
        showTariffDetails: function(tariffId) {
            var self = this;
            
            $.ajax({
                url: config.apiBaseUrl + 'ViewSupplementaryTariffDetails',
                type: 'GET',
                data: { id: tariffId },
                success: function(response) {
                    $('#tariffDetailsModalBody').html(response);
                    $('#tariffDetailsModal').modal('show');
                },
                error: function(xhr, status, error) {
                    self.showError('خطا در بارگذاری جزئیات: ' + error);
                }
            });
        },
        
        // مقداردهی فرم تعرفه
        initTariffForm: function() {
            var self = this;
            
            // تنظیم تاریخ امروز
            $('#tariffForm input[type="date"]').val(new Date().toISOString().split('T')[0]);
            
            // اعتبارسنجی real-time
            $('#tariffForm input, #tariffForm textarea').on('blur', function() {
                self.validateField($(this));
            });
            
            // محاسبه خودکار
            $('.coverage-input').on('input', function() {
                self.calculateEffectiveCoverage();
            });
            
            // تنظیمات پیش‌فرض JSON
            if ($('#settingsJson').val().trim() === '') {
                $('#settingsJson').val(SupplementaryInsurance.generateDefaultSettings());
            }
        },
        
        // ذخیره تعرفه
        saveTariff: function() {
            var self = this;
            var formData = $('#tariffForm').serialize();
            
            // اعتبارسنجی فرم
            if (!this.validateForm()) {
                return;
            }
            
            $.ajax({
                url: config.apiBaseUrl + 'SaveSupplementaryTariff',
                type: 'POST',
                data: formData,
                beforeSend: function() {
                    self.showLoading();
                },
                success: function(response) {
                    self.hideLoading();
                    if (response.success) {
                        self.showSuccess('تعرفه با موفقیت ذخیره شد');
                        $('#tariffModal').modal('hide');
                        self.loadTariffs();
                    } else {
                        self.showError(response.message || 'خطا در ذخیره تعرفه');
                    }
                },
                error: function(xhr, status, error) {
                    self.hideLoading();
                    self.showError('خطا در ارتباط با سرور: ' + error);
                }
            });
        },
        
        // حذف تعرفه
        deleteTariff: function(tariffId) {
            var self = this;
            
            if (!confirm('آیا از حذف این تعرفه اطمینان دارید؟')) {
                return;
            }
            
            $.ajax({
                url: config.apiBaseUrl + 'DeleteSupplementaryTariff',
                type: 'POST',
                data: { 
                    id: tariffId,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                beforeSend: function() {
                    self.showLoading();
                },
                success: function(response) {
                    self.hideLoading();
                    if (response.success) {
                        self.showSuccess('تعرفه با موفقیت حذف شد');
                        self.loadTariffs();
                    } else {
                        self.showError(response.message || 'خطا در حذف تعرفه');
                    }
                },
                error: function(xhr, status, error) {
                    self.hideLoading();
                    self.showError('خطا در ارتباط با سرور: ' + error);
                }
            });
        },
        
        // اعتبارسنجی فرم
        validateForm: function() {
            var isValid = true;
            var errors = [];
            
            // اعتبارسنجی فیلدهای الزامی
            $('#tariffForm [required]').each(function() {
                if (!self.validateField($(this))) {
                    isValid = false;
                }
            });
            
            // اعتبارسنجی منطقی
            var coveragePercent = parseFloat($('#coveragePercent').val()) || 0;
            var maxPayment = parseFloat($('#maxPayment').val()) || 0;
            var deductible = parseFloat($('#deductible').val()) || 0;
            
            if (coveragePercent < config.validationRules.minCoveragePercent || 
                coveragePercent > config.validationRules.maxCoveragePercent) {
                errors.push('درصد پوشش باید بین 0 تا 100 باشد');
                isValid = false;
            }
            
            if (maxPayment < config.validationRules.minPayment) {
                errors.push('حداکثر مبلغ پرداخت نمی‌تواند منفی باشد');
                isValid = false;
            }
            
            if (deductible < config.validationRules.minDeductible) {
                errors.push('فرانشیز نمی‌تواند منفی باشد');
                isValid = false;
            }
            
            if (errors.length > 0) {
                this.showError(errors.join('<br>'));
            }
            
            return isValid;
        },
        
        // اعتبارسنجی فیلد
        validateField: function(field) {
            var value = field.val();
            var isValid = true;
            
            // حذف کلاس‌های قبلی
            field.removeClass('is-valid is-invalid');
            
            // اعتبارسنجی فیلدهای الزامی
            if (field.prop('required') && (!value || value.trim() === '')) {
                field.addClass('is-invalid');
                isValid = false;
            }
            
            // اعتبارسنجی نوع داده
            if (field.attr('type') === 'number') {
                var numValue = parseFloat(value);
                if (value && (isNaN(numValue) || numValue < 0)) {
                    field.addClass('is-invalid');
                    isValid = false;
                }
            }
            
            // اعتبارسنجی JSON
            if (field.attr('id') === 'settingsJson' && value.trim() !== '') {
                if (!SupplementaryInsurance.validateJson(value)) {
                    field.addClass('is-invalid');
                    isValid = false;
                }
            }
            
            if (isValid && value) {
                field.addClass('is-valid');
            }
            
            return isValid;
        },
        
        // اعتبارسنجی JSON
        validateJson: function(field) {
            var jsonText = field.val();
            if (jsonText.trim() !== '') {
                try {
                    JSON.parse(jsonText);
                    field.removeClass('is-invalid').addClass('is-valid');
                    return true;
                } catch (e) {
                    field.removeClass('is-valid').addClass('is-invalid');
                    this.showError('فرمت JSON نامعتبر است');
                    return false;
                }
            }
            return true;
        },
        
        // محاسبه پوشش موثر
        calculateEffectiveCoverage: function() {
            var baseAmount = parseFloat($('#baseAmount').val()) || 1000000;
            var coveragePercent = parseFloat($('#coveragePercent').val()) || 0;
            var maxPayment = parseFloat($('#maxPayment').val()) || 0;
            var deductible = parseFloat($('#deductible').val()) || 0;
            
            var calculation = SupplementaryInsurance.calculateEffectiveCoverage(
                baseAmount, coveragePercent, maxPayment, deductible
            );
            
            // نمایش نتایج
            $('#amountAfterDeductible').text(this.formatCurrency(calculation.amountAfterDeductible));
            $('#calculatedCoverage').text(this.formatCurrency(calculation.calculatedCoverage));
            $('#effectiveCoverage').text(this.formatCurrency(calculation.effectiveCoverage));
            $('#patientShare').text(this.formatCurrency(calculation.patientShare));
        },
        
        // فرمت کردن مبلغ
        formatCurrency: function(amount) {
            return new Intl.NumberFormat('fa-IR').format(amount) + ' ریال';
        },
        
        // فرمت کردن تاریخ
        formatDate: function(dateString) {
            var date = new Date(dateString);
            return date.toLocaleDateString('fa-IR');
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
            toastr.success(message, 'موفقیت', {
                positionClass: 'toast-top-left',
                timeOut: 5000
            });
        },
        
        // نمایش پیام خطا
        showError: function(message) {
            toastr.error(message, 'خطا', {
                positionClass: 'toast-top-left',
                timeOut: 5000
            });
        }
    };
})();

// مقداردهی اولیه هنگام بارگذاری صفحه
$(document).ready(function() {
    // بررسی وجود جدول تعرفه‌ها
    if ($('#supplementaryTariffTable').length) {
        var planId = $('#supplementaryTariffTable').data('plan-id');
        if (planId) {
            SupplementaryTariffManager.init(planId);
        }
    }
});

// Export برای استفاده در ماژول‌ها
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SupplementaryTariffManager;
}
