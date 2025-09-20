/**
 * Combined Insurance Calculation Module
 * مدیریت محاسبه بیمه ترکیبی برای محیط درمانی
 * 
 * @version 2.0
 * @author Medical System Team
 * @description ماژول محاسبه بیمه ترکیبی با بهینه‌سازی برای 7000 بیمار
 */

// Namespace برای جلوگیری از تداخل
window.CombinedInsuranceCalculation = (function() {
    'use strict';

    // تنظیمات ماژول
    const CONFIG = {
        ajaxTimeout: 10000,
        searchDelay: 800,
        minInputLength: 3,
        maxPageSize: 200,
        nationalCodePattern: /^\d{10}$/
    };

    // Cache برای بهبود عملکرد
    const cache = {
        patients: new Map(),
        departments: new Map(),
        services: new Map()
    };

    /**
     * راه‌اندازی اولیه ماژول
     */
    function initialize() {
        console.log('🏥 MEDICAL: راه‌اندازی ماژول محاسبه بیمه ترکیبی v2.1');
        
        // اضافه کردن global error handler
        window.addEventListener('error', function(event) {
            if (event.filename && event.filename.includes('combined-insurance-calculation.js')) {
                console.error('🏥 MEDICAL: Global Error در combined-insurance-calculation.js:', event.error);
                showError('خطای سیستمی در محاسبه بیمه ترکیبی');
            }
        });
        
        // تنظیمات اولیه
        setupPageSettings();
        
        // بارگیری همزمان داده‌ها
        loadInitialData();
        
        // راه‌اندازی event handlers
        setupEventHandlers();
        
        console.log('🏥 MEDICAL: ماژول محاسبه بیمه ترکیبی راه‌اندازی شد');
    }

    /**
     * تنظیمات اولیه صفحه
     */
    function setupPageSettings() {
        // تنظیمات toastr
        if (typeof toastr !== 'undefined') {
            toastr.options = {
                "closeButton": true,
                "debug": false,
                "newestOnTop": true,
                "progressBar": true,
                "positionClass": "toast-top-right",
                "preventDuplicates": false,
                "onclick": null,
                "showDuration": "300",
                "hideDuration": "1000",
                "timeOut": "5000",
                "extendedTimeOut": "1000",
                "showEasing": "swing",
                "hideEasing": "linear",
                "showMethod": "fadeIn",
                "hideMethod": "fadeOut"
            };
        }

        // تنظیمات Select2
        if (typeof $.fn.select2 !== 'undefined') {
            $.fn.select2.defaults.set("language", {
                noResults: function() {
                    return 'نتیجه‌ای یافت نشد';
                },
                searching: function() {
                    return 'در حال جستجو...';
                },
                inputTooShort: function() {
                    return 'حداقل ' + CONFIG.minInputLength + ' کاراکتر وارد کنید';
                }
            });
        }

        // تنظیمات AJAX
        $.ajaxSetup({
            timeout: CONFIG.ajaxTimeout,
            cache: false
        });
    }

    /**
     * بارگیری داده‌های اولیه
     */
    function loadInitialData() {
        console.log('🏥 MEDICAL: بارگیری داده‌های اولیه');
        
        Promise.all([
            loadPatients(),
            loadDepartments()
        ]).then(function() {
            console.log('🏥 MEDICAL: تمام داده‌ها بارگذاری شد');
            showSuccess('داده‌ها با موفقیت بارگذاری شد');
        }).catch(function(error) {
            console.error('🏥 MEDICAL: خطا در بارگذاری داده‌ها', error);
            showError('خطا در بارگذاری داده‌های اولیه');
        });
    }

    /**
     * راه‌اندازی event handlers
     */
    function setupEventHandlers() {
        // مدیریت تغییر دپارتمان
        $('#departmentId').on('change', function() {
            var departmentId = $(this).val();
            if (departmentId) {
                $('#serviceId').prop('disabled', false).removeClass('is-disabled');
                loadServicesByDepartment(departmentId);
            } else {
                $('#serviceId').prop('disabled', true).addClass('is-disabled')
                    .empty().append('<option value="">ابتدا دپارتمان را انتخاب کنید...</option>');
            }
        });

        // مدیریت تغییر خدمت
        $('#serviceId').on('change', function() {
            var selectedServiceId = $(this).val();
            if (selectedServiceId) {
                loadServicePrice(selectedServiceId);
            }
        });

        // مدیریت دکمه محاسبه
        $('#calculateBtn').on('click', function(e) {
            e.preventDefault();
            calculateCombinedInsurance();
        });

        // مدیریت دکمه پاک کردن
        $('#clearBtn').on('click', function(e) {
            e.preventDefault();
            clearForm();
        });

        // اعتبارسنجی مبلغ خدمت
        $('#serviceAmount').on('input', validateServiceAmount);
    }

    /**
     * بارگیری لیست بیماران
     */
    function loadPatients() {
        console.log('🏥 MEDICAL: راه‌اندازی Select2 بیمار با جستجوی کد ملی...');
        
        var patientSelect = $('#patientId');
        
        return new Promise(function(resolve, reject) {
            try {
                patientSelect.select2({
                    placeholder: 'جستجو با کد ملی (حداقل 3 رقم)...',
                    allowClear: true,
                    minimumInputLength: 3, // حداقل 3 رقم برای کد ملی
                    ajax: {
                        url: '/Admin/Insurance/CombinedInsuranceCalculation/GetPatients',
                        dataType: 'json',
                        delay: CONFIG.searchDelay,
                        timeout: CONFIG.ajaxTimeout,
                        data: function (params) {
                            // فقط جستجوی کد ملی - حرفه‌ای برای محیط درمانی
                            var isNationalCode = CONFIG.nationalCodePattern.test(params.term);
                            var isPartialNationalCode = params.term && params.term.length >= 3 && params.term.length < 10 && /^\d+$/.test(params.term);
                            
                            // تعیین pageSize بر اساس نوع جستجو
                            var pageSize = 5; // حداکثر 5 نتیجه برای محیط درمانی
                            if (isNationalCode) {
                                pageSize = 1; // کد ملی کامل = فقط یک نتیجه
                            } else if (isPartialNationalCode) {
                                pageSize = 5; // کد ملی جزئی = حداکثر 5 نتیجه
                            } else {
                                pageSize = 0; // اگر کد ملی نیست = هیچ نتیجه‌ای
                            }
                            
                            return {
                                searchTerm: params.term || '',
                                searchType: 'nationalCode', // همیشه کد ملی
                                page: params.page || 1,
                                pageSize: pageSize
                            };
                        },
                        processResults: function (data, params) {
                            console.log('🏥 MEDICAL: نتایج جستجوی بیماران', data);
                            
                            // محدودیت اضافی برای جلوگیری از بارگذاری بیش از حد
                            var results = data.results || [];
                            var hasMore = false;
                            
                            // اگر بیش از 5 نتیجه داریم، صفحه‌بندی را متوقف کن
                            if (results.length >= 5) {
                                hasMore = false;
                            } else {
                                hasMore = data.pagination ? data.pagination.more : false;
                            }
                            
                            return {
                                results: results,
                                pagination: {
                                    more: hasMore
                                }
                            };
                        },
                        cache: true,
                        beforeSend: function(xhr, settings) {
                            if (settings.url.indexOf('?') > -1) {
                                settings.url += '&_t=' + new Date().getTime();
                            } else {
                                settings.url += '?_t=' + new Date().getTime();
                            }
                        }
                    },
                language: {
                    noResults: function() {
                        return 'بیماری با این کد ملی یافت نشد';
                    },
                    searching: function() {
                        return 'در حال جستجوی کد ملی...';
                    },
                    inputTooShort: function() {
                        return 'حداقل 3 رقم کد ملی وارد کنید';
                    }
                },
                    templateResult: function(patient) {
                        if (patient.loading) {
                            return patient.text;
                        }
                        
                        return $(
                            '<div class="patient-option">' +
                                '<div class="patient-name"><strong>' + patient.fullName + '</strong></div>' +
                                '<div class="patient-details">' +
                                    '<small class="text-muted">کد ملی: ' + patient.nationalCode + '</small>' +
                                    (patient.phoneNumber ? '<br><small class="text-muted">تلفن: ' + patient.phoneNumber + '</small>' : '') +
                                '</div>' +
                            '</div>'
                        );
                    },
                    templateSelection: function(patient) {
                        if (patient && patient.fullName && patient.nationalCode) {
                            return patient.fullName + ' (' + patient.nationalCode + ')';
                        }
                        return patient.text || 'انتخاب بیمار';
                    }
                });
                
                resolve();
            } catch (error) {
                console.error('🏥 MEDICAL: خطا در راه‌اندازی Select2 بیمار', error);
                reject(error);
            }
        });
    }

    /**
     * بارگیری لیست دپارتمان‌ها
     */
    function loadDepartments() {
        console.log('🏥 MEDICAL: بارگیری لیست دپارتمان‌ها...');
        
        return new Promise(function(resolve, reject) {
            $.ajax({
                url: '/Admin/Insurance/CombinedInsuranceCalculation/GetDepartments',
                type: 'GET',
                dataType: 'json',
                success: function(response) {
                    if (response.results && response.results.length > 0) {
                        console.log('🏥 MEDICAL: لیست دپارتمان‌ها دریافت شد', response.results);
                        
                        var departmentSelect = $('#departmentId');
                        departmentSelect.empty();
                        departmentSelect.append('<option value="">انتخاب دپارتمان...</option>');
                        
                        response.results.forEach(function(department) {
                            departmentSelect.append('<option value="' + department.id + '">' + department.text + '</option>');
                        });
                        
                        resolve();
                    } else {
                        console.warn('🏥 MEDICAL: هیچ دپارتمانی یافت نشد');
                        showWarning('هیچ دپارتمانی یافت نشد');
                        resolve();
                    }
                },
                error: function(xhr, status, error) {
                    console.error('🏥 MEDICAL: خطای AJAX در دریافت دپارتمان‌ها', error);
                    showError('خطا در ارتباط با سرور برای دریافت دپارتمان‌ها');
                    reject(error);
                }
            });
        });
    }

    /**
     * بارگیری خدمات بر اساس دپارتمان
     */
    function loadServicesByDepartment(departmentId) {
        console.log('🏥 MEDICAL: بارگیری لیست خدمات برای دپارتمان', departmentId);
        
        var serviceSelect = $('#serviceId');
        
        serviceSelect.select2({
            placeholder: 'جستجو با کد خدمت (حداقل 3 رقم)...',
            allowClear: true,
            minimumInputLength: 3, // حداقل 3 رقم برای جستجوی کد خدمت
            ajax: {
                url: '/Admin/Insurance/CombinedInsuranceCalculation/GetServices',
                dataType: 'json',
                delay: 500, // افزایش delay برای جستجوی بهتر
                data: function (params) {
                    console.log('🏥 MEDICAL: درخواست جستجوی خدمات با کد:', params.term);
                    
                    return {
                        departmentId: departmentId,
                        searchTerm: params.term || '',
                        page: params.page || 1,
                        pageSize: 10 // کاهش pageSize برای جستجوی دقیق‌تر
                    };
                },
                processResults: function (data, params) {
                    console.log('🏥 MEDICAL: نتایج جستجوی خدمات', data.results);
                    
                    // محدودیت نتایج برای جستجوی دقیق
                    var results = data.results || [];
                    var hasMore = false;
                    
                    // اگر بیش از 10 نتیجه داریم، صفحه‌بندی را متوقف کن
                    if (results.length >= 10) {
                        hasMore = false;
                    } else {
                        hasMore = data.pagination ? data.pagination.more : false;
                    }
                    
                    return {
                        results: results,
                        pagination: {
                            more: hasMore
                        }
                    };
                },
                cache: true
            },
            language: {
                noResults: function() {
                    return 'خدمتی با این کد یافت نشد';
                },
                searching: function() {
                    return 'در حال جستجوی کد خدمت...';
                },
                inputTooShort: function() {
                    return 'حداقل 3 رقم کد خدمت وارد کنید';
                }
            },
            templateResult: function(service) {
                if (service.loading) {
                    return service.text;
                }
                
                return $(
                    '<div class="service-option">' +
                        '<div class="service-title"><strong>' + service.title + '</strong></div>' +
                        '<div class="service-details">' +
                            '<small class="text-muted">کد: ' + service.serviceCode + '</small>' +
                            (service.basePrice ? '<br><small class="text-success">قیمت: ' + service.basePrice.toLocaleString() + ' تومان</small>' : '') +
                        '</div>' +
                    '</div>'
                );
            },
            templateSelection: function(service) {
                if (service && service.title) {
                    return service.title + (service.serviceCode ? ' (' + service.serviceCode + ')' : '');
                }
                return service.text || 'انتخاب خدمت';
            }
        });
        
        // اضافه کردن event handler برای تغییر خدمت
        serviceSelect.on('change', function() {
            var selectedServiceId = $(this).val();
            console.log('🏥 MEDICAL: خدمت انتخاب شد:', selectedServiceId);
            
            if (selectedServiceId) {
                loadServicePrice(selectedServiceId);
            } else {
                $('#serviceAmount').val('').removeClass('is-valid is-invalid');
            }
        });
    }

    /**
     * بارگیری قیمت خدمت
     */
    function loadServicePrice(serviceId) {
        console.log('🏥 MEDICAL: دریافت قیمت خدمت', serviceId);
        
        if (!serviceId) {
            console.warn('🏥 MEDICAL: ServiceId نامعتبر');
            return;
        }
        
        // نمایش loading state
        $('#serviceAmount').addClass('loading-state').prop('disabled', true);
        
        $.ajax({
            url: '/Admin/Insurance/CombinedInsuranceCalculation/GetServicePrice',
            type: 'GET',
            data: { serviceId: serviceId },
            dataType: 'json',
            timeout: 10000,
            success: function(response) {
                console.log('🏥 MEDICAL: پاسخ دریافت قیمت خدمت:', response);
                
                $('#serviceAmount').removeClass('loading-state').prop('disabled', false);
                
                if (response.success && response.price && response.price > 0) {
                    $('#serviceAmount').val(response.price).addClass('is-valid').removeClass('is-invalid');
                    console.log('🏥 MEDICAL: مبلغ خدمت از سرور دریافت شد:', response.price);
                    
                    var sourceText = response.source === 'stored' ? 'ذخیره شده' : 'محاسبه شده';
                    showSuccess('مبلغ خدمت به صورت خودکار تنظیم شد: ' + response.price.toLocaleString() + ' تومان (' + sourceText + ')');
                } else {
                    console.warn('🏥 MEDICAL: قیمت خدمت از سرور دریافت نشد - Response:', response);
                    $('#serviceAmount').addClass('is-invalid').removeClass('is-valid');
                    showWarning('قیمت خدمت در سیستم تعریف نشده است - لطفاً مبلغ را دستی وارد کنید');
                }
            },
            error: function(xhr, status, error) {
                console.error('🏥 MEDICAL: خطا در دریافت قیمت خدمت', {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    responseText: xhr.responseText,
                    error: error
                });
                
                $('#serviceAmount').removeClass('loading-state').prop('disabled', false).addClass('is-invalid');
                showWarning('خطا در دریافت قیمت خدمت - لطفاً مبلغ را دستی وارد کنید');
            }
        });
    }

    /**
     * محاسبه بیمه ترکیبی
     */
    function calculateCombinedInsurance() {
        console.log('🏥 MEDICAL: شروع محاسبه بیمه ترکیبی');
        
        // اعتبارسنجی ورودی‌ها
        var patientId = $('#patientId').val();
        var serviceId = $('#serviceId').val();
        var serviceAmount = $('#serviceAmount').val();
        var calculationDate = $('#calculationDate').val();
        
        if (!patientId || !serviceId || !serviceAmount) {
            showError('لطفاً تمام فیلدها را پر کنید');
            return;
        }
        
        if (parseFloat(serviceAmount) <= 0) {
            showError('مبلغ خدمت باید بیشتر از صفر باشد');
            return;
        }
        
        // نمایش loading indicator
        showLoadingIndicator();
        
        // ارسال درخواست محاسبه
        $.ajax({
            url: '/Admin/Insurance/CombinedInsuranceCalculation/CalculateCombinedInsurance',
            type: 'POST',
            data: {
                patientId: patientId,
                serviceId: serviceId,
                serviceAmount: serviceAmount,
                calculationDate: calculationDate
            },
            dataType: 'json',
            success: function(response) {
                hideLoadingIndicator();
                
                console.log('🏥 MEDICAL: پاسخ سرور دریافت شد', response);
                
                if (response && response.success) {
                    console.log('🏥 MEDICAL: محاسبه با موفقیت انجام شد', response.data);
                    
                    try {
                        displayCalculationResults(response.data);
                        showSuccess('محاسبه بیمه ترکیبی با موفقیت انجام شد');
                    } catch (error) {
                        console.error('🏥 MEDICAL: خطا در نمایش نتایج، استفاده از fallback', error);
                        displayCalculationResultsFallback(response.data);
                    }
                } else {
                    console.error('🏥 MEDICAL: خطا در محاسبه', response ? response.message : 'پاسخ نامعتبر');
                    showError('خطا در محاسبه: ' + (response ? response.message : 'پاسخ نامعتبر'));
                }
            },
            error: function(xhr, status, error) {
                hideLoadingIndicator();
                console.error('🏥 MEDICAL: خطای AJAX در محاسبه', error);
                
                var errorMessage = 'خطا در محاسبه بیمه ترکیبی';
                if (xhr.status === 400) {
                    errorMessage = 'درخواست نامعتبر - لطفاً ورودی‌ها را بررسی کنید';
                } else if (xhr.status === 500) {
                    errorMessage = 'خطای سرور - لطفاً بعداً تلاش کنید';
                }
                
                showError(errorMessage);
            }
        });
    }

    /**
     * اعتبارسنجی داده‌های محاسبه برای محیط درمانی
     * این تابع برای اطمینان از صحت داده‌ها در محیط درمانی طراحی شده است
     */
    function validateMedicalCalculationData(data) {
        if (!data) return false;
        
        // بررسی وجود فیلدهای ضروری
        const serviceAmount = data.serviceAmount || data.ServiceAmount;
        const finalPatientShare = data.finalPatientShare || data.FinalPatientShare;
        
        if (typeof serviceAmount === 'undefined' || typeof finalPatientShare === 'undefined') {
            console.error('🏥 MEDICAL: فیلدهای ضروری محاسبه موجود نیست');
            return false;
        }
        
        // بررسی صحت مقادیر عددی
        if (isNaN(serviceAmount) || isNaN(finalPatientShare)) {
            console.error('🏥 MEDICAL: مقادیر عددی نامعتبر');
            return false;
        }
        
        // بررسی منطقی بودن مقادیر
        if (serviceAmount < 0 || finalPatientShare < 0) {
            console.error('🏥 MEDICAL: مقادیر منفی غیرقابل قبول در محیط درمانی');
            return false;
        }
        
        // بررسی منطقی بودن سهم بیمار
        if (finalPatientShare > serviceAmount) {
            console.warn('🏥 MEDICAL: سهم بیمار بیشتر از مبلغ خدمت است - بررسی نیاز است');
        }
        
        console.log('🏥 MEDICAL: اعتبارسنجی داده‌ها موفق');
        return true;
    }

    /**
     * نرمال‌سازی داده‌های محاسبه برای پشتیبانی از case sensitivity
     * این تابع برای محیط درمانی طراحی شده است تا از خطاهای case sensitivity جلوگیری کند
     */
    function normalizeCalculationData(data) {
        if (!data) return {};
        
        return {
            // فیلدهای اصلی
            serviceAmount: data.serviceAmount || data.ServiceAmount || 0,
            finalPatientShare: data.finalPatientShare || data.FinalPatientShare || 0,
            
            // فیلدهای بیمه اصلی
            primaryCoverage: data.primaryCoverage || data.PrimaryCoverage || 0,
            primaryCoveragePercent: data.primaryCoveragePercent || data.PrimaryCoveragePercent || 0,
            
            // فیلدهای بیمه تکمیلی
            supplementaryCoverage: data.supplementaryCoverage || data.SupplementaryCoverage || 0,
            supplementaryCoveragePercent: data.supplementaryCoveragePercent || data.SupplementaryCoveragePercent || 0,
            
            // فیلدهای محاسبه
            patientSharePercent: data.patientSharePercent || data.PatientSharePercent || 0,
            totalInsuranceCoverage: data.totalInsuranceCoverage || data.TotalInsuranceCoverage || 0,
            totalCoveragePercent: data.totalCoveragePercent || data.TotalCoveragePercent || 0,
            supplementarySavings: data.supplementarySavings || data.SupplementarySavings || 0,
            
            // فیلدهای وضعیت
            coverageStatus: data.coverageStatus || data.CoverageStatus || 'نامشخص',
            coverageStatusColor: data.coverageStatusColor || data.CoverageStatusColor || 'secondary',
            calculationType: data.calculationType || data.CalculationType || 'ترکیبی',
            calculationDate: data.calculationDate || data.CalculationDate,
            
            // فیلدهای اضافی
            notes: data.notes || data.Notes || '',
            description: data.description || data.Description || 'محاسبه بیمه ترکیبی انجام شد',
            hasSupplementaryInsurance: data.hasSupplementaryInsurance || data.HasSupplementaryInsurance || false,
            
            // فیلدهای فرمت شده
            formattedServiceAmount: data.formattedServiceAmount || data.FormattedServiceAmount || '',
            formattedPrimaryCoverage: data.formattedPrimaryCoverage || data.FormattedPrimaryCoverage || '',
            formattedSupplementaryCoverage: data.formattedSupplementaryCoverage || data.FormattedSupplementaryCoverage || '',
            formattedFinalPatientShare: data.formattedFinalPatientShare || data.FormattedFinalPatientShare || '',
            formattedTotalInsuranceCoverage: data.formattedTotalInsuranceCoverage || data.FormattedTotalInsuranceCoverage || ''
        };
    }

    /**
     * نمایش نتایج محاسبه - نسخه بهبود یافته v2.1
     * تاریخ: 2024-12-20
     * بهبود: اضافه شدن validation کامل و error handling
     */
    function displayCalculationResults(data) {
        console.log('🏥 MEDICAL: displayCalculationResults v2.1 شروع شد', data);
        console.log('🏥 MEDICAL: ساختار کامل داده‌ها:', JSON.stringify(data, null, 2));
        
        // Medical Grade Validation - بررسی صحت داده‌ها برای محیط درمانی
        if (!validateMedicalCalculationData(data)) {
            console.error('🏥 MEDICAL: داده‌های محاسبه برای محیط درمانی نامعتبر است');
            showError('داده‌های محاسبه برای محیط درمانی نامعتبر است');
            return;
        }
        
        // بررسی وجود داده
        if (!data) {
            console.error('🏥 MEDICAL: داده‌های محاسبه موجود نیست');
            showError('داده‌های محاسبه موجود نیست');
            return;
        }

        // بررسی وجود properties مورد نیاز - با پشتیبانی از case sensitivity
        const serviceAmount = data.serviceAmount || data.ServiceAmount;
        const finalPatientShare = data.finalPatientShare || data.FinalPatientShare;
        
        if (typeof serviceAmount === 'undefined' || 
            typeof finalPatientShare === 'undefined') {
            console.error('🏥 MEDICAL: ساختار داده‌های محاسبه ناقص است', data);
            showError('ساختار داده‌های محاسبه ناقص است');
            return;
        }

        // نرمال‌سازی داده‌ها برای پشتیبانی از case sensitivity
        const normalizedData = normalizeCalculationData(data);
        
        // بررسی فیلدهای اختیاری
        const hasPrimaryCoverage = typeof normalizedData.primaryCoverage !== 'undefined';
        const hasSupplementaryCoverage = typeof normalizedData.supplementaryCoverage !== 'undefined';
        const hasPrimaryCoveragePercent = typeof normalizedData.primaryCoveragePercent !== 'undefined';
        const hasSupplementaryCoveragePercent = typeof normalizedData.supplementaryCoveragePercent !== 'undefined';
        const hasPatientSharePercent = typeof normalizedData.patientSharePercent !== 'undefined';

        console.log('🏥 MEDICAL: بررسی فیلدهای موجود:', {
            serviceAmount: typeof data.serviceAmount !== 'undefined',
            finalPatientShare: typeof data.finalPatientShare !== 'undefined',
            primaryCoverage: hasPrimaryCoverage,
            supplementaryCoverage: hasSupplementaryCoverage,
            primaryCoveragePercent: hasPrimaryCoveragePercent,
            supplementaryCoveragePercent: hasSupplementaryCoveragePercent,
            patientSharePercent: hasPatientSharePercent
        });

        try {
            // نمایش مبلغ کل خدمت
            $('#totalServiceAmount').text(normalizedData.serviceAmount.toLocaleString() + ' تومان');
            
            // نمایش پوشش بیمه اصلی
            $('#primaryCoverage').text(normalizedData.primaryCoverage.toLocaleString() + ' تومان');
            $('#primaryCoveragePercent').text(normalizedData.primaryCoveragePercent.toFixed(1) + '%');
            
            // نمایش پوشش بیمه تکمیلی
            $('#supplementaryCoverage').text(normalizedData.supplementaryCoverage.toLocaleString() + ' تومان');
            $('#supplementaryCoveragePercent').text(normalizedData.supplementaryCoveragePercent.toFixed(1) + '%');
            
            // نمایش سهم نهایی بیمار
            $('#finalPatientShare').text(normalizedData.finalPatientShare.toLocaleString() + ' تومان');
            $('#patientSharePercent').text(normalizedData.patientSharePercent.toFixed(1) + '%');
        
        // ساخت جزئیات محاسبه با داده‌های نرمال‌شده
        var details = `
            <div class="row">
                <div class="col-md-6">
                    <strong>وضعیت پوشش:</strong> <span class="badge badge-${normalizedData.coverageStatusColor}">${normalizedData.coverageStatus}</span>
                </div>
                <div class="col-md-6">
                    <strong>کل پوشش بیمه:</strong> ${normalizedData.totalInsuranceCoverage.toLocaleString()} تومان (${normalizedData.totalCoveragePercent.toFixed(1)}%)
                </div>
            </div>
            <div class="row mt-2">
                <div class="col-md-6">
                    <strong>صرفه‌جویی از بیمه تکمیلی:</strong> ${normalizedData.supplementarySavings.toLocaleString()} تومان
                </div>
                <div class="col-md-6">
                    <strong>تاریخ محاسبه:</strong> ${formatCalculationDate(normalizedData.calculationDate)}
                </div>
            </div>
            <div class="row mt-2">
                <div class="col-md-12">
                    <strong>توضیحات:</strong> ${normalizedData.description}
                </div>
            </div>
        `;
        
        if (data.notes) {
            details += `<div class="row mt-2"><div class="col-12"><strong>توضیحات:</strong> ${data.notes}</div></div>`;
        }
        
        $('#calculationDetails').html(details);
        $('#calculationResults').show();
        
        console.log('🏥 MEDICAL: نتایج محاسبه نمایش داده شد');
        console.log('🏥 MEDICAL: فیلدهای موجود در داده‌ها:', Object.keys(data));
        
        } catch (error) {
            console.error('🏥 MEDICAL: خطا در نمایش نتایج محاسبه', error);
            showError('خطا در نمایش نتایج محاسبه: ' + error.message);
        }
    }

    /**
     * Fallback function برای نمایش نتایج (در صورت خطا)
     */
    function displayCalculationResultsFallback(data) {
        console.log('🏥 MEDICAL: استفاده از fallback function', data);
        
        try {
            // نرمال‌سازی داده‌ها در fallback
            const normalizedData = normalizeCalculationData(data);
            
            // نمایش ساده نتایج با fallback کامل
            $('#totalServiceAmount').text(normalizedData.serviceAmount.toLocaleString() + ' تومان');
            $('#primaryCoverage').text(normalizedData.primaryCoverage.toLocaleString() + ' تومان');
            $('#primaryCoveragePercent').text(normalizedData.primaryCoveragePercent.toFixed(1) + '%');
            $('#supplementaryCoverage').text(normalizedData.supplementaryCoverage.toLocaleString() + ' تومان');
            $('#supplementaryCoveragePercent').text(normalizedData.supplementaryCoveragePercent.toFixed(1) + '%');
            $('#finalPatientShare').text(normalizedData.finalPatientShare.toLocaleString() + ' تومان');
            $('#patientSharePercent').text(normalizedData.patientSharePercent.toFixed(1) + '%');
            
            // نمایش جزئیات ساده
            var simpleDetails = `
                <div class="row">
                    <div class="col-md-12">
                        <strong>وضعیت:</strong> <span class="badge badge-warning">Fallback Mode</span>
                    </div>
                </div>
                <div class="row mt-2">
                    <div class="col-md-12">
                        <strong>توضیحات:</strong> محاسبه انجام شد (حالت fallback)
                    </div>
                </div>
            `;
            
            $('#calculationDetails').html(simpleDetails);
            $('#calculationResults').show();
            showSuccess('محاسبه انجام شد (حالت fallback)');
        } catch (error) {
            console.error('🏥 MEDICAL: خطا در fallback function', error);
            showError('خطا در نمایش نتایج');
        }
    }

    /**
     * نمایش loading indicator
     */
    function showLoadingIndicator() {
        console.log('🏥 MEDICAL: نمایش loading indicator');
        
        if ($('#loadingOverlay').length === 0) {
            $('body').append(`
                <div id="loadingOverlay" class="loading-overlay">
                    <div class="loading-content">
                        <div class="loading-spinner"></div>
                        <h5 class="mt-3 mb-2">در حال محاسبه بیمه ترکیبی...</h5>
                        <p class="text-muted">لطفاً صبر کنید - این عملیات ممکن است چند ثانیه طول بکشد</p>
                        <div class="progress mt-3" style="width: 250px;">
                            <div class="progress-bar progress-bar-striped progress-bar-animated" 
                                 role="progressbar" style="width: 100%"></div>
                        </div>
                        <small class="text-muted mt-2 d-block">
                            <i class="fas fa-shield-alt"></i> محاسبات امن و رمزنگاری شده
                        </small>
                    </div>
                </div>
            `);
        }
        
        $('#calculateBtn').prop('disabled', true).html('<span class="loading-spinner"></span> در حال محاسبه...');
        
        if (typeof toastr !== 'undefined') {
            toastr.info('محاسبه بیمه ترکیبی در حال انجام است...', 'در حال پردازش', {
                timeOut: 3000
            });
        }
    }

    /**
     * مخفی کردن loading indicator
     */
    function hideLoadingIndicator() {
        console.log('🏥 MEDICAL: مخفی کردن loading indicator');
        
        $('#loadingOverlay').fadeOut(300, function() {
            $(this).remove();
        });
        
        $('.security-indicator').fadeOut(200, function() {
            $(this).remove();
        });
        
        $('#calculateBtn').prop('disabled', false).html('<i class="fas fa-calculator"></i> محاسبه بیمه ترکیبی');
    }

    /**
     * پاک کردن فرم
     */
    function clearForm() {
        console.log('🏥 MEDICAL: پاک کردن فرم');
        
        $('#calculationForm')[0].reset();
        $('#calculationResults').hide();
        
        $('#patientId').val(null).trigger('change');
        $('#departmentId').val(null).trigger('change');
        
        $('#serviceId').prop('disabled', true).addClass('is-disabled')
            .empty().append('<option value="">ابتدا دپارتمان را انتخاب کنید...</option>');
        
        $('#calculationDate').val('');
        
        $('.form-control').removeClass('is-valid is-invalid');
        
        if (typeof toastr !== 'undefined') {
            toastr.success('فرم با موفقیت پاک شد', 'پاک کردن فرم');
        }
    }

    /**
     * اعتبارسنجی مبلغ خدمت
     */
    function validateServiceAmount() {
        var amount = parseFloat($('#serviceAmount').val());
        var serviceAmountInput = $('#serviceAmount');
        
        serviceAmountInput.removeClass('is-valid is-invalid');
        
        if (!amount || isNaN(amount)) {
            // اگر مقدار وجود ندارد، قرمز نکن - فقط border عادی
            return false;
        } else if (amount <= 0) {
            serviceAmountInput.addClass('is-invalid');
            showError('مبلغ خدمت باید بیشتر از صفر باشد');
            return false;
        } else if (amount > 0) {
            serviceAmountInput.addClass('is-valid');
            return true;
        }
        
        return false;
    }

    /**
     * نمایش پیام موفقیت
     */
    function showSuccess(message) {
        console.log('🏥 MEDICAL: Success -', message);
        if (typeof toastr !== 'undefined') {
            toastr.success(message, 'موفقیت', {
                timeOut: 5000,
                closeButton: true,
                progressBar: true,
                positionClass: 'toast-top-right'
            });
        }
    }

    /**
     * نمایش پیام خطا
     */
    function showError(message) {
        console.error('🏥 MEDICAL: Error -', message);
        if (typeof toastr !== 'undefined') {
            toastr.error(message, 'خطا', {
                timeOut: 8000,
                closeButton: true,
                progressBar: true,
                positionClass: 'toast-top-right'
            });
        }
    }

    /**
     * نمایش پیام هشدار
     */
    function showWarning(message) {
        console.warn('🏥 MEDICAL: Warning -', message);
        if (typeof toastr !== 'undefined') {
            toastr.warning(message, 'هشدار', {
                timeOut: 5000,
                closeButton: true,
                progressBar: true,
                positionClass: 'toast-top-right'
            });
        }
    }

    /**
     * نمایش پیام اطلاعاتی
     */
    function showInfo(message) {
        console.log('🏥 MEDICAL: Info -', message);
        if (typeof toastr !== 'undefined') {
            toastr.info(message, 'اطلاعات', {
                timeOut: 4000,
                closeButton: true,
                progressBar: true,
                positionClass: 'toast-top-right'
            });
        }
    }

    /**
     * فرمت کردن تاریخ محاسبه - رفع مشکل Invalid Date
     */
    function formatCalculationDate(dateValue) {
        try {
            if (!dateValue) {
                return 'نامشخص';
            }

            // بررسی اینکه آیا تاریخ معتبر است یا نه
            let date;
            if (typeof dateValue === 'string') {
                // اگر رشته است، سعی در تبدیل به Date
                date = new Date(dateValue);
            } else if (dateValue instanceof Date) {
                date = dateValue;
            } else {
                return 'نامشخص';
            }

            // بررسی معتبر بودن تاریخ
            if (isNaN(date.getTime())) {
                console.warn('🏥 MEDICAL: تاریخ نامعتبر:', dateValue);
                return 'نامشخص';
            }

            // تبدیل به تاریخ شمسی
            return date.toLocaleDateString('fa-IR', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });
        } catch (error) {
            console.error('🏥 MEDICAL: خطا در فرمت کردن تاریخ:', error, 'DateValue:', dateValue);
            return 'نامشخص';
        }
    }

    // Public API
    return {
        initialize: initialize,
        calculateCombinedInsurance: calculateCombinedInsurance,
        clearForm: clearForm,
        showSuccess: showSuccess,
        showError: showError,
        showWarning: showWarning,
        showInfo: showInfo
    };

})();

// راه‌اندازی خودکار هنگام بارگذاری صفحه
$(document).ready(function() {
    console.log('🏥 MEDICAL: صفحه محاسبه بیمه ترکیبی بارگذاری شد');
    console.log('🏥 MEDICAL: نسخه JavaScript: v2.1 - تاریخ: 2024-12-20');
    
    // راه‌اندازی ماژول
    if (typeof CombinedInsuranceCalculation !== 'undefined') {
        CombinedInsuranceCalculation.initialize();
    } else {
        console.error('🏥 MEDICAL: ماژول CombinedInsuranceCalculation یافت نشد');
    }
});
