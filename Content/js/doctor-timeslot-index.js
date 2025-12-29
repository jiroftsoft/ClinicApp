/**
 * فایل JavaScript اختصاصی برای صفحه مدیریت اسلات‌های زمانی
 * Doctor TimeSlot Management - Index Page
 * 
 * ویژگی‌ها:
 * - DataTables برای بهبود UX
 * - SweetAlert2 برای تأیید عملیات
 * - Event Handlers برای عملیات CRUD
 */

$(document).ready(function() {
    'use strict';

    // ✅ Initialize DataTables
    var $table = $('#timeSlotsTable');
    if ($table.length > 0) {
        var $tbody = $table.find('tbody');
        var $rows = $tbody.find('tr');
        
        // ✅ فقط اگر ردیف‌های معتبر (بدون colspan) وجود داشته باشد
        var hasValidRows = $rows.length > 0 && $rows.first().find('td').length === 8;
        
        if (hasValidRows) {
            var table = $table.DataTable({
                responsive: true,
                language: {
                    url: '/Content/plugins/DataTables/js/fa.json',
                    emptyTable: 'هیچ اسلاتی یافت نشد',
                    loadingRecords: 'در حال بارگذاری...',
                    processing: 'در حال پردازش...',
                    zeroRecords: 'هیچ رکوردی یافت نشد',
                    search: 'جستجو:',
                    lengthMenu: 'نمایش _MENU_ رکورد در هر صفحه',
                    info: 'نمایش _START_ تا _END_ از _TOTAL_ رکورد',
                    infoEmpty: 'نمایش 0 تا 0 از 0 رکورد',
                    infoFiltered: '(فیلتر شده از _MAX_ رکورد کل)',
                    paginate: {
                        first: 'اول',
                        previous: 'قبلی',
                        next: 'بعدی',
                        last: 'آخر'
                    }
                },
                pageLength: 25,
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "همه"]],
                order: [[0, 'desc']], // مرتب‌سازی بر اساس شناسه (جدیدترین اول)
                columnDefs: [
                    { 
                        orderable: false, 
                        targets: [7] // ستون عملیات قابل مرتب‌سازی نیست
                    },
                    {
                        type: 'num',
                        targets: [0, 4] // ستون‌های عددی
                    }
                ],
                // ✅ بهبود Performance
                deferRender: true,
                processing: true,
                stateSave: false, // ❌ غیرفعال برای محیط درمانی (اطلاعات باید همیشه به‌روز باشند)
                // ✅ بهبود UX
                dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rt<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
                drawCallback: function(settings) {
                    // Re-initialize event handlers after DataTables redraw
                    initializeEventHandlers();
                }
            });
        } else {
            console.log('DataTable not initialized: No valid data rows found');
        }
    }
    
    // ✅ Initialize Event Handlers
    function initializeEventHandlers() {
        // SweetAlert2 برای تأیید آزاد کردن اسلات
        $(document).off('click', '.btn-release').on('click', '.btn-release', function(e) {
            e.preventDefault();
            var form = $(this).closest('form');
            var timeSlotId = $(this).data('id');
            
            Swal.fire({
                title: 'آیا از انجام این عملیات اطمینان دارید؟',
                text: 'این اسلات آزاد خواهد شد و قابل رزرو مجدد می‌شود',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#ffc107',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'بله، آزاد کن',
                cancelButtonText: 'خیر، انصراف',
                reverseButtons: true
            }).then(function(result) {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        });
        
        // SweetAlert2 برای تأیید حذف اسلات
        $(document).off('click', '.btn-delete').on('click', '.btn-delete', function(e) {
            e.preventDefault();
            var form = $(this).closest('form');
            var timeSlotId = $(this).data('id');
            
            Swal.fire({
                title: 'آیا از انجام این عملیات اطمینان دارید؟',
                text: 'این اسلات حذف خواهد شد و قابل بازگشت نیست',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'بله، حذف کن',
                cancelButtonText: 'خیر، انصراف',
                reverseButtons: true
            }).then(function(result) {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        });
    }
    
    // Initialize event handlers on page load
    initializeEventHandlers();
});

