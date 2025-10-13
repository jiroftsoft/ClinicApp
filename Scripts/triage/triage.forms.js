/**
 * تریاژ فرم - اعتبارسنجی و مدیریت فرم‌های تریاژ
 * Production-Ready, RTL Support, Real-time Validation
 */

(function($) {
    'use strict';

    // Namespace برای مدیریت فرم‌ها
    window.TriageForms = {
        // تنظیمات
        config: {
            validationDelay: 300,
            showErrors: true,
            showSuccess: true
        },

        // مقداردهی اولیه
        init: function() {
            this.bindEvents();
            this.setupValidation();
        },

        // اتصال رویدادها
        bindEvents: function() {
            var self = this;

            // اعتبارسنجی Real-time
            $(document).on('input blur', '.triage-form input, .triage-form select, .triage-form textarea', function() {
                self.validateField($(this));
            });

            // محاسبه GCS Total
            $(document).on('input', '#GcsE, #GcsV, #GcsM', function() {
                self.calculateGcsTotal();
            });

            // بررسی هشدارها
            $(document).on('input', '#SBP, #DBP, #HR, #RR, #TempC, #SpO2', function() {
                self.checkAlerts();
            });

            // ارسال فرم
            $(document).on('submit', '.triage-form', function(e) {
                if (!self.validateForm($(this))) {
                    e.preventDefault();
                    return false;
                }
            });

            // دکمه ذخیره
            $(document).on('click', '.btn-save-triage', function(e) {
                e.preventDefault();
                self.saveTriage();
            });
        },

        // تنظیم اعتبارسنجی
        setupValidation: function() {
            // اضافه کردن کلاس‌های اعتبارسنجی
            $('.triage-form input[required], .triage-form select[required], .triage-form textarea[required]')
                .addClass('required-field');

            // تنظیم پیغام‌های خطا
            this.setupErrorMessages();
        },

        // تنظیم پیغام‌های خطا
        setupErrorMessages: function() {
            var messages = {
                required: 'این فیلد الزامی است',
                range: 'مقدار باید در محدوده مجاز باشد',
                stringLength: 'تعداد کاراکترها باید در محدوده مجاز باشد',
                number: 'لطفاً عدد معتبر وارد کنید',
                email: 'لطفاً ایمیل معتبر وارد کنید',
                url: 'لطفاً آدرس معتبر وارد کنید'
            };

            // تنظیم پیغام‌های پیش‌فرض jQuery Validation
            if ($.validator) {
                $.extend($.validator.messages, messages);
            }
        },

        // اعتبارسنجی فیلد
        validateField: function(field) {
            var self = this;
            var value = field.val();
            var isValid = true;
            var errorMessage = '';

            // بررسی فیلدهای الزامی
            if (field.hasClass('required-field') && (!value || value.trim() === '')) {
                isValid = false;
                errorMessage = 'این فیلد الزامی است';
            }

            // بررسی محدوده عددی
            if (isValid && field.attr('type') === 'number' && value) {
                var min = parseFloat(field.attr('min'));
                var max = parseFloat(field.attr('max'));
                var numValue = parseFloat(value);

                if (!isNaN(min) && numValue < min) {
                    isValid = false;
                    errorMessage = `مقدار باید حداقل ${min} باشد`;
                } else if (!isNaN(max) && numValue > max) {
                    isValid = false;
                    errorMessage = `مقدار باید حداکثر ${max} باشد`;
                }
            }

            // بررسی طول رشته
            if (isValid && field.attr('maxlength') && value) {
                var maxLength = parseInt(field.attr('maxlength'));
                if (value.length > maxLength) {
                    isValid = false;
                    errorMessage = `تعداد کاراکترها نباید بیش از ${maxLength} باشد`;
                }
            }

            // نمایش نتیجه
            this.showFieldValidation(field, isValid, errorMessage);

            return isValid;
        },

        // نمایش نتیجه اعتبارسنجی فیلد
        showFieldValidation: function(field, isValid, errorMessage) {
            var self = this;
            var container = field.closest('.form-group');
            var feedback = container.find('.validation-feedback');

            // حذف کلاس‌های قبلی
            field.removeClass('is-valid is-invalid');
            feedback.remove();

            if (isValid) {
                if (this.config.showSuccess) {
                    field.addClass('is-valid');
                }
            } else {
                if (this.config.showErrors) {
                    field.addClass('is-invalid');
                    container.append(`<div class="validation-feedback invalid-feedback">${errorMessage}</div>`);
                }
            }
        },

        // اعتبارسنجی فرم کامل
        validateForm: function(form) {
            var self = this;
            var isValid = true;
            var firstInvalidField = null;

            // اعتبارسنجی همه فیلدها
            form.find('input, select, textarea').each(function() {
                var field = $(this);
                if (!self.validateField(field)) {
                    isValid = false;
                    if (!firstInvalidField) {
                        firstInvalidField = field;
                    }
                }
            });

            // اعتبارسنجی‌های خاص تریاژ
            if (isValid) {
                isValid = self.validateTriageSpecific(form);
            }

            // نمایش نتیجه
            if (!isValid) {
                this.showFormValidation(form, false);
                if (firstInvalidField) {
                    firstInvalidField.focus();
                }
            } else {
                this.showFormValidation(form, true);
            }

            return isValid;
        },

        // اعتبارسنجی‌های خاص تریاژ
        validateTriageSpecific: function(form) {
            var isValid = true;

            // بررسی GCS
            var gcsE = parseInt(form.find('#GcsE').val()) || 0;
            var gcsV = parseInt(form.find('#GcsV').val()) || 0;
            var gcsM = parseInt(form.find('#GcsM').val()) || 0;

            if (gcsE > 0 && gcsV > 0 && gcsM > 0) {
                var gcsTotal = gcsE + gcsV + gcsM;
                if (gcsTotal < 3 || gcsTotal > 15) {
                    isValid = false;
                    this.showError('مجموع GCS باید بین 3 تا 15 باشد');
                }
            }

            // بررسی فشار خون
            var sbp = parseInt(form.find('#SBP').val()) || 0;
            var dbp = parseInt(form.find('#DBP').val()) || 0;

            if (sbp > 0 && dbp > 0 && sbp <= dbp) {
                isValid = false;
                this.showError('فشار خون سیستولیک باید بیشتر از دیاستولیک باشد');
            }

            // بررسی دمای بدن
            var temp = parseFloat(form.find('#TempC').val()) || 0;
            if (temp > 0 && (temp < 30 || temp > 45)) {
                isValid = false;
                this.showError('دمای بدن باید بین 30 تا 45 درجه سانتی‌گراد باشد');
            }

            return isValid;
        },

        // نمایش نتیجه اعتبارسنجی فرم
        showFormValidation: function(form, isValid) {
            var alertContainer = form.find('.form-validation-alert');
            alertContainer.remove();

            if (!isValid) {
                form.prepend('<div class="form-validation-alert alert alert-danger">لطفاً خطاهای فرم را برطرف کنید</div>');
            } else {
                form.prepend('<div class="form-validation-alert alert alert-success">فرم معتبر است</div>');
            }
        },

        // محاسبه GCS Total
        calculateGcsTotal: function() {
            var gcsE = parseInt($('#GcsE').val()) || 0;
            var gcsV = parseInt($('#GcsV').val()) || 0;
            var gcsM = parseInt($('#GcsM').val()) || 0;
            var total = gcsE + gcsV + gcsM;

            $('#gcsTotal').text(total);

            // بررسی محدوده GCS
            if (total > 0 && (total < 3 || total > 15)) {
                $('#gcsTotal').addClass('text-danger');
            } else {
                $('#gcsTotal').removeClass('text-danger');
            }
        },

        // بررسی هشدارها
        checkAlerts: function() {
            var self = this;
            var alerts = [];

            // بررسی SpO2
            var spO2 = parseInt($('#SpO2').val()) || 0;
            if (spO2 > 0 && spO2 < 90) {
                alerts.push('اشباع اکسیژن پایین است');
            }

            // بررسی ضربان قلب
            var hr = parseInt($('#HR').val()) || 0;
            if (hr > 0 && (hr > 120 || hr < 50)) {
                alerts.push('ضربان قلب غیرطبیعی است');
            }

            // بررسی فشار خون
            var sbp = parseInt($('#SBP').val()) || 0;
            if (sbp > 0 && sbp < 90) {
                alerts.push('فشار خون پایین است');
            }

            // بررسی دمای بدن
            var temp = parseFloat($('#TempC').val()) || 0;
            if (temp > 0 && (temp > 39 || temp < 35)) {
                alerts.push('دمای بدن غیرطبیعی است');
            }

            // بررسی میزان تنفس
            var rr = parseInt($('#RR').val()) || 0;
            if (rr > 0 && (rr > 30 || rr < 10)) {
                alerts.push('میزان تنفس غیرطبیعی است');
            }

            // بررسی GCS
            var gcsTotal = parseInt($('#gcsTotal').text()) || 0;
            if (gcsTotal > 0 && gcsTotal < 8) {
                alerts.push('GCS پایین است');
            }

            // نمایش هشدارها
            this.showAlerts(alerts);
        },

        // نمایش هشدارها
        showAlerts: function(alerts) {
            var alertContainer = $('#triageAlerts');
            alertContainer.empty();

            if (alerts.length === 0) {
                alertContainer.append('<div class="alert alert-success">✅ همه علائم حیاتی در محدوده طبیعی</div>');
                return;
            }

            alerts.forEach(function(alert) {
                alertContainer.append(`<div class="alert alert-danger">⚠️ ${alert}</div>`);
            });
        },

        // ذخیره تریاژ
        saveTriage: function() {
            var self = this;
            var form = $('.triage-form');

            if (!this.validateForm(form)) {
                return;
            }

            // نمایش لودینگ
            TriageAjax.showLoading();

            // ارسال فرم
            form.submit();
        },

        // Helper Methods
        showError: function(message) {
            TriageAjax.showError(message);
        },

        showSuccess: function(message) {
            TriageAjax.showSuccess(message);
        },

        showWarning: function(message) {
            TriageAjax.showWarning(message);
        },

        showInfo: function(message) {
            TriageAjax.showInfo(message);
        }
    };

    // مقداردهی اولیه خودکار
    $(document).ready(function() {
        TriageForms.init();
    });

})(jQuery);
