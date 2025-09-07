/**
 * فایل JavaScript اختصاصی برای صفحه مدیریت انتسابات پزشکان
 * Doctor Assignment Management - Index Page
 */

$(document).ready(function() {
    'use strict';

    // دریافت URL ها از data attributes
    const table = $('#assignmentsTable');
    const urls = {
        refresh: table.data('refresh-url'),
        filter: table.data('filter-url'),
        export: table.data('export-url'),
        getDoctorInfo: table.data('doctor-info-url'),
        getAssignments: table.data('assignments-url')
    };

    // متغیرهای سراسری
    let assignmentsTable;
    let autoRefreshInterval;

    /**
     * راه‌اندازی DataTable
     */
    function initializeDataTable() {
        assignmentsTable = $('#assignmentsTable').DataTable({
            language: {
                url: '/Content/plugins/DataTables/js/fa.json'
            },
            processing: true,
            serverSide: true,
            ajax: {
                url: urls.refresh,
                type: 'POST',
                data: function(d) {
                    // اضافه کردن فیلترها
                    d.departmentId = $('#departmentFilter').val();
                    d.serviceCategoryId = $('#serviceCategoryFilter').val();
                    d.dateFrom = $('#dateFromFilter').val();
                    d.dateTo = $('#dateToFilter').val();
                    d.doctorSearch = $('#doctorSearchFilter').val();
                    
                    // اضافه کردن AntiForgeryToken
                    d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();
                },
                error: function(xhr, error, thrown) {
                    console.error('DataTables AJAX Error:', error, thrown);
                    console.error('Response:', xhr.responseText);
                }
            },
            columns: [
                { data: 'doctorName', title: 'نام پزشک' },
                { data: 'departmentName', title: 'دپارتمان' },
                { data: 'serviceCategoryName', title: 'سرفصل خدماتی' },
                { data: 'assignmentDate', title: 'تاریخ انتساب' },
                { data: 'status', title: 'وضعیت' },
                { 
                    data: null, 
                    title: 'عملیات',
                    orderable: false,
                    render: function(data, type, row) {
                        return `
                            <div class="btn-group" role="group">
                                <button type="button" class="btn btn-sm btn-info" onclick="viewDetails(${row.doctorId})">
                                    <i class="fas fa-eye"></i> جزئیات
                                </button>
                                <button type="button" class="btn btn-sm btn-warning" onclick="editAssignment(${row.doctorId})">
                                    <i class="fas fa-edit"></i> ویرایش
                                </button>
                            </div>
                        `;
                    }
                }
            ],
            order: [[3, 'desc']],
            pageLength: 25,
            responsive: true,
            dom: 'Bfrtip',
            buttons: [
                {
                    extend: 'excel',
                    text: 'خروجی Excel',
                    className: 'btn btn-success btn-sm'
                },
                {
                    extend: 'pdf',
                    text: 'خروجی PDF',
                    className: 'btn btn-danger btn-sm'
                }
            ]
        });
    }

    /**
     * راه‌اندازی فیلترها
     */
    function initializeFilters() {
        // راه‌اندازی Select2
        $('#departmentFilter, #serviceCategoryFilter').select2({
            placeholder: 'انتخاب کنید...',
            allowClear: true,
            language: 'fa'
        });

        // راه‌اندازی DatePicker
        $('#dateFromFilter, #dateToFilter').persianDatepicker({
            format: 'YYYY/MM/DD',
            observer: true,
            timePicker: {
                enabled: false
            }
        });

        // رویداد تغییر فیلترها
        $('#departmentFilter, #serviceCategoryFilter, #dateFromFilter, #dateToFilter, #doctorSearchFilter')
            .on('change keyup', function() {
                assignmentsTable.ajax.reload();
            });
    }

    /**
     * راه‌اندازی رفرش خودکار
     */
    function initializeAutoRefresh() {
        ClinicApp.Utils.AutoRefresh.start('assignments', function() {
            assignmentsTable.ajax.reload(null, false); // false = نگه داشتن صفحه فعلی
        }, 30000); // هر 30 ثانیه
    }

    /**
     * راه‌اندازی دکمه‌های عملیات
     */
    function initializeActionButtons() {
        // دکمه رفرش دستی
        $('#refreshBtn').on('click', function() {
            ClinicApp.Utils.showLoading('در حال به‌روزرسانی...');
            assignmentsTable.ajax.reload(function() {
                ClinicApp.Utils.hideLoading();
                ClinicApp.Utils.showSuccess('اطلاعات با موفقیت به‌روزرسانی شد');
            });
        });

        // دکمه پاک کردن فیلترها
        $('#clearFiltersBtn').on('click', function() {
            $('#departmentFilter, #serviceCategoryFilter').val(null).trigger('change');
            $('#dateFromFilter, #dateToFilter, #doctorSearchFilter').val('');
            assignmentsTable.ajax.reload();
        });

        // دکمه انتساب جدید
        $('#newAssignmentBtn').on('click', function() {
            window.location.href = $(this).data('url');
        });
    }

    /**
     * راه‌اندازی AJAX Error Handler
     */
    function initializeAjaxErrorHandler() {
        $(document).ajaxError(function(event, xhr, settings, thrownError) {
            ClinicApp.Utils.hideLoading();
            
            if (xhr.status === 401) {
                ClinicApp.Utils.showError('دسترسی غیرمجاز. لطفاً مجدداً وارد شوید.');
                setTimeout(() => {
                    window.location.href = '/Account/Login';
                }, 2000);
            } else if (xhr.status === 403) {
                ClinicApp.Utils.showError('شما مجوز دسترسی به این بخش را ندارید.');
            } else if (xhr.status === 500) {
                ClinicApp.Utils.showError('خطای سرور. لطفاً با مدیر سیستم تماس بگیرید.');
            } else {
                ClinicApp.Utils.showError('خطا در ارتباط با سرور: ' + thrownError);
            }
        });
    }

    /**
     * تابع نمایش جزئیات
     */
    window.viewDetails = function(doctorId) {
        window.location.href = `/Admin/DoctorAssignment/Details/${doctorId}`;
    };

    /**
     * تابع ویرایش انتساب
     */
    window.editAssignment = function(doctorId) {
        window.location.href = `/Admin/DoctorAssignment/Edit/${doctorId}`;
    };

    /**
     * راه‌اندازی اولیه
     */
    function initialize() {
        try {
            initializeDataTable();
            initializeFilters();
            initializeActionButtons();
            initializeAjaxErrorHandler();
            initializeAutoRefresh();
            
            console.log('Doctor Assignment Index initialized successfully');
        } catch (error) {
            console.error('Error initializing Doctor Assignment Index:', error);
            ClinicApp.Utils.showError('خطا در راه‌اندازی صفحه');
        }
    }

    // شروع راه‌اندازی
    initialize();
});
