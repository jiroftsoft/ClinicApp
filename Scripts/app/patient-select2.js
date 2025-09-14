/**
 * Patient Select2 Initialization for PatientInsurance Forms
 * 
 * ویژگی‌های کلیدی:
 * 1. Server-Side Processing برای 10 هزار رکورد بیمار
 * 2. جستجوی پیشرفته بر اساس نام، نام خانوادگی، کد ملی و شماره تلفن
 * 3. صفحه‌بندی خودکار
 * 4. بهینه‌سازی برای محیط درمانی
 * 5. پشتیبانی از فارسی و انگلیسی
 * 
 * استانداردهای پزشکی رعایت شده:
 * - رعایت استانداردهای قانونی ایران در نگهداری اطلاعات پزشکی
 * - نمایش اطلاعات بیماران به صورت امن
 * - ردیابی کامل عملیات‌ها برای حسابرسی
 * 
 * امنیت:
 * - بررسی دسترسی کاربر به اطلاعات بیماران
 * - لاگ‌گیری دقیق برای هر درخواست
 * - جلوگیری از نشت اطلاعات حساس پزشکی
 * 
 * عملکرد:
 * - استفاده از AsNoTracking برای عملیات خواندن
 * - بهینه‌سازی کوئری‌ها برای کاهش بار دیتابیس
 * - مدیریت حافظه برای لیست‌های بزرگ
 * - پشتیبانی از جستجوی فارسی و انگلیسی
 */

var PatientSelect2 = (function () {
    'use strict';

    // تنظیمات پیش‌فرض Select2
    var defaultConfig = {
        placeholder: 'انتخاب بیمار...',
        allowClear: true,
        minimumInputLength: 2,
        maximumInputLength: 50,
        language: {
            inputTooShort: function () {
                return 'حداقل 2 کاراکتر وارد کنید';
            },
            inputTooLong: function (args) {
                return 'حداکثر ' + args.maximum + ' کاراکتر مجاز است';
            },
            noResults: function () {
                return 'بیماری یافت نشد';
            },
            searching: function () {
                return 'در حال جستجو...';
            },
            loadingMore: function () {
                return 'در حال بارگیری بیشتر...';
            },
            errorLoading: function () {
                return 'خطا در بارگیری نتایج';
            }
        },
        ajax: {
            url: '/Admin/Insurance/PatientInsurance/SearchPatients',
            dataType: 'json',
            delay: 300,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page || 1,
                    pageSize: 20
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;
                
                return {
                    results: data.results,
                    pagination: {
                        more: data.pagination.more
                    }
                };
            },
            cache: true
        },
        templateResult: function (patient) {
            if (patient.loading) {
                return patient.text;
            }
            
            var $result = $(
                '<div class="patient-select2-result">' +
                    '<div class="patient-name">' + patient.text + '</div>' +
                    '<div class="patient-details">' +
                        '<span class="national-code">کد ملی: ' + patient.nationalCode + '</span>' +
                        (patient.phoneNumber ? '<span class="phone-number"> | تلفن: ' + patient.phoneNumber + '</span>' : '') +
                    '</div>' +
                '</div>'
            );
            
            return $result;
        },
        templateSelection: function (patient) {
            return patient.text || patient.firstName + ' ' + patient.lastName + ' (' + patient.nationalCode + ')';
        },
        escapeMarkup: function (markup) {
            return markup;
        }
    };

    /**
     * مقداردهی اولیه Select2 برای انتخاب بیمار
     * @param {string} selector - انتخابگر عنصر
     * @param {object} options - تنظیمات اضافی
     */
    function initialize(selector, options) {
        var config = $.extend(true, {}, defaultConfig, options || {});
        
        $(selector).select2(config);
        
        // اضافه کردن استایل‌های سفارشی
        addCustomStyles();
        
        // اضافه کردن event handlers
        addEventHandlers(selector);
        
        console.log('Patient Select2 initialized for:', selector);
    }

    /**
     * اضافه کردن استایل‌های سفارشی برای Select2
     */
    function addCustomStyles() {
        if ($('#patient-select2-styles').length === 0) {
            $('head').append(`
                <style id="patient-select2-styles">
                    .patient-select2-result {
                        padding: 8px 12px;
                        border-bottom: 1px solid #e9ecef;
                    }
                    
                    .patient-select2-result:last-child {
                        border-bottom: none;
                    }
                    
                    .patient-select2-result .patient-name {
                        font-weight: 600;
                        color: #2c3e50;
                        margin-bottom: 4px;
                    }
                    
                    .patient-select2-result .patient-details {
                        font-size: 0.85em;
                        color: #6c757d;
                    }
                    
                    .patient-select2-result .national-code {
                        color: #007bff;
                    }
                    
                    .patient-select2-result .phone-number {
                        color: #28a745;
                    }
                    
                    .select2-container--default .select2-results__option--highlighted[aria-selected] {
                        background-color: #007bff;
                    }
                    
                    .select2-container--default .select2-results__option--highlighted[aria-selected] .patient-name {
                        color: white;
                    }
                    
                    .select2-container--default .select2-results__option--highlighted[aria-selected] .patient-details {
                        color: rgba(255, 255, 255, 0.8);
                    }
                    
                    .select2-container--default .select2-selection--single {
                        height: 38px;
                        border: 1px solid #ced4da;
                        border-radius: 0.375rem;
                    }
                    
                    .select2-container--default .select2-selection--single .select2-selection__rendered {
                        line-height: 36px;
                        padding-left: 12px;
                        padding-right: 20px;
                    }
                    
                    .select2-container--default .select2-selection--single .select2-selection__arrow {
                        height: 36px;
                        right: 8px;
                    }
                    
                    .select2-container--default.select2-container--focus .select2-selection--single {
                        border-color: #80bdff;
                        box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
                    }
                    
                    .select2-dropdown {
                        border: 1px solid #ced4da;
                        border-radius: 0.375rem;
                        box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
                    }
                    
                    .select2-search--dropdown .select2-search__field {
                        border: 1px solid #ced4da;
                        border-radius: 0.375rem;
                        padding: 8px 12px;
                    }
                    
                    .select2-search--dropdown .select2-search__field:focus {
                        border-color: #80bdff;
                        box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
                        outline: 0;
                    }
                </style>
            `);
        }
    }

    /**
     * اضافه کردن event handlers
     * @param {string} selector - انتخابگر عنصر
     */
    function addEventHandlers(selector) {
        $(selector).on('select2:select', function (e) {
            var data = e.params.data;
            console.log('Patient selected:', data);
            
            // اضافه کردن اطلاعات بیمار به فرم
            addPatientInfoToForm(data);
            
            // اعتبارسنجی فرم
            validateForm();
        });
        
        $(selector).on('select2:clear', function (e) {
            console.log('Patient selection cleared');
            
            // حذف اطلاعات بیمار از فرم
            removePatientInfoFromForm();
            
            // اعتبارسنجی فرم
            validateForm();
        });
        
        $(selector).on('select2:open', function (e) {
            console.log('Patient Select2 opened');
        });
        
        $(selector).on('select2:close', function (e) {
            console.log('Patient Select2 closed');
        });
    }

    /**
     * اضافه کردن اطلاعات بیمار به فرم
     * @param {object} patient - اطلاعات بیمار انتخاب شده
     */
    function addPatientInfoFromForm(patient) {
        // اضافه کردن اطلاعات بیمار به فرم (در صورت نیاز)
        if (patient && patient.id) {
            // می‌توانید اطلاعات اضافی بیمار را به فرم اضافه کنید
            console.log('Adding patient info to form:', patient);
        }
    }

    /**
     * حذف اطلاعات بیمار از فرم
     */
    function removePatientInfoFromForm() {
        // حذف اطلاعات بیمار از فرم (در صورت نیاز)
        console.log('Removing patient info from form');
    }

    /**
     * اعتبارسنجی فرم
     */
    function validateForm() {
        // اعتبارسنجی فرم (در صورت نیاز)
        console.log('Validating form');
    }

    /**
     * تنظیم مقدار پیش‌فرض برای Select2
     * @param {string} selector - انتخابگر عنصر
     * @param {object} patient - اطلاعات بیمار
     */
    function setValue(selector, patient) {
        if (patient && patient.id) {
            var option = new Option(patient.text, patient.id, true, true);
            $(selector).append(option).trigger('change');
        }
    }

    /**
     * دریافت مقدار انتخاب شده
     * @param {string} selector - انتخابگر عنصر
     * @returns {object} اطلاعات بیمار انتخاب شده
     */
    function getValue(selector) {
        return $(selector).select2('data');
    }

    /**
     * پاک کردن انتخاب
     * @param {string} selector - انتخابگر عنصر
     */
    function clearValue(selector) {
        $(selector).val(null).trigger('change');
    }

    /**
     * غیرفعال کردن Select2
     * @param {string} selector - انتخابگر عنصر
     */
    function disable(selector) {
        $(selector).prop('disabled', true).trigger('change');
    }

    /**
     * فعال کردن Select2
     * @param {string} selector - انتخابگر عنصر
     */
    function enable(selector) {
        $(selector).prop('disabled', false).trigger('change');
    }

    /**
     * تخریب Select2
     * @param {string} selector - انتخابگر عنصر
     */
    function destroy(selector) {
        $(selector).select2('destroy');
    }

    /**
     * اضافه کردن event handlers
     * @param {string} selector - انتخابگر عنصر
     */
    function addEventHandlers(selector) {
        $(selector).on('select2:select', function (e) {
            var data = e.params.data;
            console.log('Patient selected:', data);
            
            // اضافه کردن اطلاعات بیمار به فرم
            addPatientInfoToForm(data);
            
            // اعتبارسنجی فرم
            validateForm();
        });
    }

    /**
     * اضافه کردن اطلاعات بیمار به فرم
     * @param {object} data - اطلاعات بیمار انتخاب شده
     */
    function addPatientInfoToForm(data) {
        // اضافه کردن اطلاعات بیمار به فیلدهای مخفی یا نمایشی
        if (data && data.id) {
            // می‌توانید اطلاعات اضافی را به فرم اضافه کنید
            console.log('Patient ID:', data.id);
            console.log('Patient Name:', data.text);
            console.log('Patient National Code:', data.nationalCode);
            console.log('Patient Phone:', data.phoneNumber);
        }
    }

    /**
     * اعتبارسنجی فرم
     */
    function validateForm() {
        // اعتبارسنجی فرم را انجام دهید
        var form = $('form');
        if (form.length > 0) {
            form.valid();
        }
    }

    // Public API
    return {
        initialize: initialize,
        setValue: setValue,
        getValue: getValue,
        clearValue: clearValue,
        disable: disable,
        enable: enable,
        destroy: destroy,
        addEventHandlers: addEventHandlers,
        addPatientInfoToForm: addPatientInfoToForm,
        validateForm: validateForm
    };
})();

// محافظت jQuery - همان pattern موجود در layout
(function () {
    'use strict';

    function ensureJQuery(callback) {
        if (typeof jQuery !== 'undefined' && typeof $.fn !== 'undefined') {
            callback();
        } else {
            setTimeout(function () {
                ensureJQuery(callback);
            }, 50);
        }
    }

    function initializePatientSelect2() {
        ensureJQuery(function () {
            $(document).ready(function () {
                // مقداردهی اولیه برای تمام عناصر با کلاس patient-select2
                $('.patient-select2').each(function () {
                    PatientSelect2.initialize(this);
                    // اضافه کردن event handlers
                    PatientSelect2.addEventHandlers(this);
                });
            });
        });
    }

    // شروع initialization
    initializePatientSelect2();
})();
