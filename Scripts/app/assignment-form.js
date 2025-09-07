/**
 * فایل JavaScript اختصاصی برای فرم‌های انتساب پزشکان
 * Assignment Form Management
 */

$(document).ready(function() {
    'use strict';

    // دریافت URL ها از data attributes
    const form = $('#assignmentForm');
    const urls = {
        searchDoctor: form.data('search-doctor-url'),
        getDepartments: form.data('get-departments-url'),
        getServiceCategories: form.data('get-service-categories-url'),
        preview: form.data('preview-url'),
        submit: form.attr('action')
    };

    // متغیرهای سراسری
    let selectedDepartments = [];
    let selectedServiceCategories = [];

    /**
     * راه‌اندازی اولیه
     */
    function initialize() {
        initializeEventHandlers();
        loadDepartments();
        loadDraft();
        initializeFormValidation();
    }

    /**
     * راه‌اندازی رویدادها
     */
    function initializeEventHandlers() {
        // جستجوی پزشک
        $('#searchDoctorBtn').on('click', searchDoctor);
        $('#DoctorNationalCode').on('keypress', function(e) {
            if (e.which === 13) {
                e.preventDefault();
                searchDoctor();
            }
        });

        // پیش‌نمایش
        $('#previewBtn').on('click', showPreview);

        // تأیید و ارسال
        $('#confirmSubmitBtn').on('click', submitForm);

        // ذخیره پیش‌نویس
        $('#saveDraftBtn').on('click', saveDraft);

        // تغییر دپارتمان
        $(document).on('change', '.department-checkbox', function() {
            const departmentId = $(this).val();
            if ($(this).is(':checked')) {
                selectedDepartments.push(departmentId);
                loadServiceCategoriesForDepartment(departmentId);
            } else {
                selectedDepartments = selectedDepartments.filter(id => id !== departmentId);
                removeServiceCategoriesForDepartment(departmentId);
            }
        });

        // تغییر سرفصل خدماتی
        $(document).on('change', '.service-category-checkbox', function() {
            const serviceCategoryId = $(this).val();
            if ($(this).is(':checked')) {
                selectedServiceCategories.push(serviceCategoryId);
            } else {
                selectedServiceCategories = selectedServiceCategories.filter(id => id !== serviceCategoryId);
            }
        });

        // ذخیره خودکار پیش‌نویس
        $('#Notes').on('input', debounce(saveDraft, 2000));
    }

    /**
     * جستجوی پزشک
     */
    function searchDoctor() {
        const nationalCode = $('#DoctorNationalCode').val().trim();
        
        if (!nationalCode) {
            ClinicApp.Utils.showError('لطفاً کد ملی پزشک را وارد کنید');
            return;
        }

        if (!isValidNationalCode(nationalCode)) {
            ClinicApp.Utils.showError('کد ملی وارد شده معتبر نیست');
            return;
        }

        ClinicApp.Utils.showLoading('در حال جستجوی پزشک...');

        $.ajax({
            url: urls.searchDoctor,
            type: 'GET',
            data: { nationalCode: nationalCode },
            success: function(response) {
                ClinicApp.Utils.hideLoading();
                
                if (response.success) {
                    $('#DoctorName').val(response.data.name);
                    $('#DoctorId').val(response.data.id);
                    loadDoctorAssignments(response.data.id);
                    ClinicApp.Utils.showSuccess('اطلاعات پزشک با موفقیت یافت شد');
                } else {
                    ClinicApp.Utils.showError(response.message || 'پزشک یافت نشد');
                }
            },
            error: function() {
                ClinicApp.Utils.hideLoading();
                ClinicApp.Utils.showError('خطا در جستجوی پزشک');
            }
        });
    }

    /**
     * بارگذاری دپارتمان‌ها
     */
    function loadDepartments() {
        $.ajax({
            url: urls.getDepartments,
            type: 'GET',
            success: function(response) {
                if (response.success) {
                    renderDepartments(response.data);
                }
            },
            error: function() {
                ClinicApp.Utils.showError('خطا در بارگذاری دپارتمان‌ها');
            }
        });
    }

    /**
     * رندر دپارتمان‌ها
     */
    function renderDepartments(departments) {
        const container = $('#departmentsContainer');
        container.empty();

        departments.forEach(function(dept) {
            const checkbox = $(`
                <div class="form-check form-check-inline">
                    <input class="form-check-input department-checkbox" 
                           type="checkbox" 
                           value="${dept.id}" 
                           id="dept_${dept.id}">
                    <label class="form-check-label" for="dept_${dept.id}">
                        ${dept.name}
                    </label>
                </div>
            `);
            container.append(checkbox);
        });
    }

    /**
     * بارگذاری سرفصل‌های خدماتی برای دپارتمان
     */
    function loadServiceCategoriesForDepartment(departmentId) {
        $.ajax({
            url: urls.getServiceCategories,
            type: 'GET',
            data: { departmentId: departmentId },
            success: function(response) {
                if (response.success) {
                    renderServiceCategories(response.data, departmentId);
                }
            },
            error: function() {
                ClinicApp.Utils.showError('خطا در بارگذاری سرفصل‌های خدماتی');
            }
        });
    }

    /**
     * رندر سرفصل‌های خدماتی
     */
    function renderServiceCategories(categories, departmentId) {
        const container = $('#serviceCategoriesContainer');
        
        // ایجاد بخش جدید برای این دپارتمان
        const section = $(`
            <div class="service-category-section" data-department-id="${departmentId}">
                <h6 class="text-primary">سرفصل‌های خدماتی دپارتمان ${departmentId}:</h6>
                <div class="service-categories-list"></div>
            </div>
        `);
        
        container.append(section);
        
        const listContainer = section.find('.service-categories-list');
        
        categories.forEach(function(category) {
            const checkbox = $(`
                <div class="form-check form-check-inline">
                    <input class="form-check-input service-category-checkbox" 
                           type="checkbox" 
                           value="${category.id}" 
                           id="cat_${category.id}"
                           data-department-id="${departmentId}">
                    <label class="form-check-label" for="cat_${category.id}">
                        ${category.name}
                    </label>
                </div>
            `);
            listContainer.append(checkbox);
        });
    }

    /**
     * حذف سرفصل‌های خدماتی دپارتمان
     */
    function removeServiceCategoriesForDepartment(departmentId) {
        $(`.service-category-section[data-department-id="${departmentId}"]`).remove();
        selectedServiceCategories = selectedServiceCategories.filter(id => {
            return $(`#cat_${id}`).data('department-id') !== departmentId;
        });
    }

    /**
     * نمایش پیش‌نمایش
     */
    function showPreview() {
        if (!validateForm()) {
            return;
        }

        const formData = getFormData();
        
        ClinicApp.Utils.showLoading('در حال آماده‌سازی پیش‌نمایش...');

        $.ajax({
            url: urls.preview,
            type: 'POST',
            data: formData,
            success: function(response) {
                ClinicApp.Utils.hideLoading();
                
                if (response.success) {
                    $('#previewContent').html(response.html);
                    $('#previewModal').modal('show');
                } else {
                    ClinicApp.Utils.showError(response.message || 'خطا در آماده‌سازی پیش‌نمایش');
                }
            },
            error: function() {
                ClinicApp.Utils.hideLoading();
                ClinicApp.Utils.showError('خطا در آماده‌سازی پیش‌نمایش');
            }
        });
    }

    /**
     * ارسال فرم
     */
    function submitForm() {
        if (!validateForm()) {
            return;
        }

        const formData = getFormData();
        
        ClinicApp.Utils.showLoading('در حال ارسال...');

        $.ajax({
            url: urls.submit,
            type: 'POST',
            data: formData,
            success: function(response) {
                ClinicApp.Utils.hideLoading();
                
                if (response.success) {
                    ClinicApp.Utils.DraftManager.clear('assignmentForm');
                    ClinicApp.Utils.showSuccess('انتساب با موفقیت ثبت شد');
                    setTimeout(() => {
                        window.location.href = '/Admin/DoctorAssignment/Index';
                    }, 2000);
                } else {
                    ClinicApp.Utils.showError(response.message || 'خطا در ثبت انتساب');
                }
            },
            error: function() {
                ClinicApp.Utils.hideLoading();
                ClinicApp.Utils.showError('خطا در ارسال فرم');
            }
        });
    }

    /**
     * ذخیره پیش‌نویس
     */
    function saveDraft() {
        const formData = getFormData();
        ClinicApp.Utils.DraftManager.save('assignmentForm', formData);
    }

    /**
     * بارگذاری پیش‌نویس
     */
    function loadDraft() {
        const draft = ClinicApp.Utils.DraftManager.load('assignmentForm');
        
        if (draft && !ClinicApp.Utils.DraftManager.isExpired(draft)) {
            ClinicApp.Utils.showConfirm(
                'پیش‌نویس ذخیره شده‌ای یافت شد. آیا می‌خواهید آن را بارگذاری کنید؟',
                'بارگذاری پیش‌نویس',
                'بله',
                'خیر'
            ).then((result) => {
                if (result.isConfirmed) {
                    loadFormData(draft.data);
                }
            });
        }
    }

    /**
     * دریافت داده‌های فرم
     */
    function getFormData() {
        const formData = form.serializeArray();
        
        // اضافه کردن دپارتمان‌های انتخاب شده
        selectedDepartments.forEach(function(deptId) {
            formData.push({ name: 'SelectedDepartments', value: deptId });
        });
        
        // اضافه کردن سرفصل‌های انتخاب شده
        selectedServiceCategories.forEach(function(catId) {
            formData.push({ name: 'SelectedServiceCategories', value: catId });
        });
        
        return formData;
    }

    /**
     * بارگذاری داده‌ها در فرم
     */
    function loadFormData(data) {
        // پیاده‌سازی بارگذاری داده‌ها
        // این بخش باید بر اساس ساختار داده‌ها پیاده‌سازی شود
    }

    /**
     * اعتبارسنجی فرم
     */
    function validateForm() {
        if (!ClinicApp.Utils.validateForm('assignmentForm')) {
            ClinicApp.Utils.showError('لطفاً تمام فیلدهای اجباری را پر کنید');
            return false;
        }

        if (selectedDepartments.length === 0) {
            ClinicApp.Utils.showError('حداقل یک دپارتمان را انتخاب کنید');
            return false;
        }

        if (selectedServiceCategories.length === 0) {
            ClinicApp.Utils.showError('حداقل یک سرفصل خدماتی را انتخاب کنید');
            return false;
        }

        return true;
    }

    /**
     * راه‌اندازی اعتبارسنجی فرم
     */
    function initializeFormValidation() {
        // اعتبارسنجی کد ملی
        $('#DoctorNationalCode').on('blur', function() {
            const nationalCode = $(this).val().trim();
            if (nationalCode && !isValidNationalCode(nationalCode)) {
                $(this).addClass('is-invalid');
                ClinicApp.Utils.showError('کد ملی وارد شده معتبر نیست');
            } else {
                $(this).removeClass('is-invalid');
            }
        });
    }

    /**
     * اعتبارسنجی کد ملی
     */
    function isValidNationalCode(nationalCode) {
        if (!/^\d{10}$/.test(nationalCode)) {
            return false;
        }
        
        const check = parseInt(nationalCode[9]);
        const sum = nationalCode.split('').slice(0, 9).reduce((acc, x, i) => acc + parseInt(x) * (10 - i), 0);
        const remainder = sum % 11;
        
        return (remainder < 2 && check === remainder) || (remainder >= 2 && check === 11 - remainder);
    }

    /**
     * تابع debounce
     */
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // شروع راه‌اندازی
    initialize();
});
