/**
 * Supplementary Tariff Management JavaScript
 * مدیریت تعرفه‌های بیمه تکمیلی
 * 
 * @author ClinicApp Team
 * @version 1.0.0
 * @description JavaScript functions for supplementary tariff management
 */

// Global variables
let supplementaryTariffData = {};
let currentView = 'index'; // index, details, edit, delete
let isFormDirty = false;

/**
 * Initialize supplementary tariff management
 * راه‌اندازی مدیریت تعرفه‌های بیمه تکمیلی
 */
function initializeSupplementaryTariff() {
    console.log('🏥 MEDICAL: Initializing Supplementary Tariff Management');
    
    // Initialize based on current view
    switch (currentView) {
        case 'index':
            initializeIndexView();
            break;
        case 'details':
            initializeDetailsView();
            break;
        case 'edit':
            initializeEditView();
            break;
        case 'delete':
            initializeDeleteView();
            break;
    }
    
    // Initialize common components
    initializeCommonComponents();
}

/**
 * Initialize index view
 * راه‌اندازی نمای فهرست
 */
function initializeIndexView() {
    console.log('🏥 MEDICAL: Initializing Index View');
    
    // Initialize filters
    initializeFilters();
    
    // Initialize search
    initializeSearch();
    
    // Initialize pagination
    initializePagination();
    
    // Initialize view toggle
    initializeViewToggle();
    
    // Initialize export functionality
    initializeExport();
}

/**
 * Initialize details view
 * راه‌اندازی نمای جزئیات
 */
function initializeDetailsView() {
    console.log('🏥 MEDICAL: Initializing Details View');
    
    // Initialize calculation preview
    initializeCalculationPreview();
    
    // Initialize action buttons
    initializeActionButtons();
}

/**
 * Initialize edit view
 * راه‌اندازی نمای ویرایش
 */
function initializeEditView() {
    console.log('🏥 MEDICAL: Initializing Edit View');
    
    // Initialize form validation
    initializeFormValidation();
    
    // Initialize real-time calculation
    initializeRealTimeCalculation();
    
    // Initialize form dirty tracking
    initializeFormDirtyTracking();
    
    // Initialize date pickers
    initializeDatePickers();
    
    // Initialize currency formatting
    initializeCurrencyFormatting();
}

/**
 * Initialize delete view
 * راه‌اندازی نمای حذف
 */
function initializeDeleteView() {
    console.log('🏥 MEDICAL: Initializing Delete View');
    
    // Initialize confirmation dialogs
    initializeConfirmationDialogs();
}

/**
 * Initialize common components
 * راه‌اندازی اجزای مشترک
 */
function initializeCommonComponents() {
    // Initialize tooltips
    $('[data-toggle="tooltip"]').tooltip();
    
    // Initialize popovers
    $('[data-toggle="popover"]').popover();
    
    // Initialize loading indicators
    initializeLoadingIndicators();
    
    // Initialize error handling
    initializeErrorHandling();
}

/**
 * Initialize filters
 * راه‌اندازی فیلترها
 */
function initializeFilters() {
    console.log('🏥 MEDICAL: Initializing Filters');
    
    // Auto-submit on filter change
    $('#serviceId, #insurancePlanId, #status, #priority').on('change', function() {
        console.log('🏥 MEDICAL: Filter changed, submitting form');
        $('#filterForm').submit();
    });
    
    // Date range validation
    $('#startDate, #endDate').on('change', function() {
        validateDateRange();
    });
    
    // Amount range validation
    $('#minAmount, #maxAmount').on('input', function() {
        validateAmountRange();
    });
}

/**
 * Initialize search
 * راه‌اندازی جستجو
 */
function initializeSearch() {
    console.log('🏥 MEDICAL: Initializing Search');
    
    // Debounced search
    let searchTimeout;
    $('#searchTerm').on('input', function() {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function() {
            console.log('🏥 MEDICAL: Performing search');
            $('#filterForm').submit();
        }, 500);
    });
}

/**
 * Initialize pagination
 * راه‌اندازی صفحه‌بندی
 */
function initializePagination() {
    console.log('🏥 MEDICAL: Initializing Pagination');
    
    // Handle pagination clicks
    $('.pagination a').on('click', function(e) {
        e.preventDefault();
        const page = $(this).data('page');
        if (page) {
            $('#page').val(page);
            $('#filterForm').submit();
        }
    });
}

/**
 * Initialize view toggle
 * راه‌اندازی تغییر نمای
 */
function initializeViewToggle() {
    console.log('🏥 MEDICAL: Initializing View Toggle');
    
    // Toggle between card and table view
    $('#toggleView').on('click', function() {
        const currentView = $(this).data('view');
        const newView = currentView === 'card' ? 'table' : 'card';
        
        console.log('🏥 MEDICAL: Switching view from', currentView, 'to', newView);
        
        // Update view
        $(this).data('view', newView);
        
        // Toggle visibility
        if (newView === 'card') {
            $('.supplementary-tariff-table').hide();
            $('.supplementary-tariff-cards').show();
            $(this).html('<i class="fas fa-table me-2"></i>نمای جدول');
        } else {
            $('.supplementary-tariff-cards').hide();
            $('.supplementary-tariff-table').show();
            $(this).html('<i class="fas fa-th me-2"></i>نمای کارت');
        }
    });
}

/**
 * Initialize export functionality
 * راه‌اندازی قابلیت خروجی
 */
function initializeExport() {
    console.log('🏥 MEDICAL: Initializing Export');
    
    $('#exportBtn').on('click', function() {
        console.log('🏥 MEDICAL: Exporting data');
        
        // Add export parameter
        $('<input>').attr({
            type: 'hidden',
            name: 'export',
            value: 'true'
        }).appendTo('#filterForm');
        
        $('#filterForm').submit();
    });
}

/**
 * Initialize calculation preview
 * راه‌اندازی پیش‌نمایش محاسبات
 */
function initializeCalculationPreview() {
    console.log('🏥 MEDICAL: Initializing Calculation Preview');
    
    // Update calculation preview
    updateCalculationPreview();
}

/**
 * Initialize action buttons
 * راه‌اندازی دکمه‌های عملیات
 */
function initializeActionButtons() {
    console.log('🏥 MEDICAL: Initializing Action Buttons');
    
    // Edit button
    $('#editBtn').on('click', function() {
        const tariffId = $(this).data('tariff-id');
        window.location.href = '/Admin/SupplementaryTariff/Edit/' + tariffId;
    });
    
    // Delete button
    $('#deleteBtn').on('click', function() {
        const tariffId = $(this).data('tariff-id');
        if (confirm('آیا مطمئن هستید که می‌خواهید این تعرفه را حذف کنید؟')) {
            window.location.href = '/Admin/SupplementaryTariff/Delete/' + tariffId;
        }
    });
}

/**
 * Initialize form validation
 * راه‌اندازی اعتبارسنجی فرم
 */
function initializeFormValidation() {
    console.log('🏥 MEDICAL: Initializing Form Validation');
    
    // Real-time validation
    $('.form-control').on('blur', function() {
        validateField($(this));
    });
    
    // Form submission validation
    $('#editForm').on('submit', function(e) {
        if (!validateForm()) {
            e.preventDefault();
            return false;
        }
    });
}

/**
 * Initialize real-time calculation
 * راه‌اندازی محاسبه زمان واقعی
 */
function initializeRealTimeCalculation() {
    console.log('🏥 MEDICAL: Initializing Real-time Calculation');
    
    // Update calculation on input change
    $('.currency-input, .percentage-input').on('input', function() {
        updateCalculationPreview();
    });
}

/**
 * Initialize form dirty tracking
 * راه‌اندازی ردیابی تغییرات فرم
 */
function initializeFormDirtyTracking() {
    console.log('🏥 MEDICAL: Initializing Form Dirty Tracking');
    
    // Track form changes
    $('.form-control').on('input', function() {
        isFormDirty = true;
    });
    
    // Warn before leaving if form is dirty
    $(window).on('beforeunload', function() {
        if (isFormDirty) {
            return 'تغییرات ذخیره نشده است. آیا مطمئن هستید که می‌خواهید صفحه را ترک کنید؟';
        }
    });
    
    // Reset dirty flag on form submission
    $('#editForm').on('submit', function() {
        isFormDirty = false;
    });
}

/**
 * Initialize date pickers
 * راه‌اندازی انتخابگر تاریخ
 */
function initializeDatePickers() {
    console.log('🏥 MEDICAL: Initializing Date Pickers');
    
    $('.persian-datepicker').persianDatepicker({
        format: 'YYYY/MM/DD',
        autoClose: true,
        initialValue: false,
        observer: true
    });
}

/**
 * Initialize currency formatting
 * راه‌اندازی فرمت‌بندی ارز
 */
function initializeCurrencyFormatting() {
    console.log('🏥 MEDICAL: Initializing Currency Formatting');
    
    $('.currency-input').on('input', function() {
        let value = $(this).val().replace(/[^\d]/g, '');
        if (value) {
            $(this).val(parseInt(value).toLocaleString());
        }
    });
}

/**
 * Initialize confirmation dialogs
 * راه‌اندازی دیالوگ‌های تأیید
 */
function initializeConfirmationDialogs() {
    console.log('🏥 MEDICAL: Initializing Confirmation Dialogs');
    
    // Delete confirmation
    $('#deleteForm').on('submit', function(e) {
        if (!confirm('آیا مطمئن هستید که می‌خواهید این تعرفه را حذف کنید؟\n\nاین عمل قابل بازگشت نیست.')) {
            e.preventDefault();
            return false;
        }
    });
}

/**
 * Initialize loading indicators
 * راه‌اندازی نشانگرهای بارگذاری
 */
function initializeLoadingIndicators() {
    console.log('🏥 MEDICAL: Initializing Loading Indicators');
    
    // Show loading on form submission
    $('form').on('submit', function() {
        showLoading();
    });
    
    // Hide loading on page load
    $(window).on('load', function() {
        hideLoading();
    });
}

/**
 * Initialize error handling
 * راه‌اندازی مدیریت خطا
 */
function initializeErrorHandling() {
    console.log('🏥 MEDICAL: Initializing Error Handling');
    
    // Global error handler
    window.onerror = function(message, source, lineno, colno, error) {
        console.error('🏥 MEDICAL: Global error:', message, source, lineno, colno, error);
        showError('خطای غیرمنتظره رخ داده است. لطفاً صفحه را بازخوانی کنید.');
    };
    
    // AJAX error handler
    $(document).ajaxError(function(event, xhr, settings, thrownError) {
        console.error('🏥 MEDICAL: AJAX error:', xhr.status, thrownError);
        showError('خطا در ارتباط با سرور. لطفاً دوباره تلاش کنید.');
    });
}

/**
 * Validate date range
 * اعتبارسنجی محدوده تاریخ
 */
function validateDateRange() {
    const startDate = $('#startDate').val();
    const endDate = $('#endDate').val();
    
    if (startDate && endDate) {
        const start = new Date(startDate);
        const end = new Date(endDate);
        
        if (start > end) {
            showError('تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.');
            $('#endDate').focus();
            return false;
        }
    }
    
    return true;
}

/**
 * Validate amount range
 * اعتبارسنجی محدوده مبلغ
 */
function validateAmountRange() {
    const minAmount = parseFloat($('#minAmount').val()) || 0;
    const maxAmount = parseFloat($('#maxAmount').val()) || 0;
    
    if (minAmount > 0 && maxAmount > 0 && minAmount > maxAmount) {
        showError('حداقل مبلغ نمی‌تواند بیشتر از حداکثر مبلغ باشد.');
        $('#maxAmount').focus();
        return false;
    }
    
    return true;
}

/**
 * Validate field
 * اعتبارسنجی فیلد
 */
function validateField(field) {
    const value = field.val();
    const fieldName = field.attr('name');
    
    // Remove existing validation classes
    field.removeClass('is-valid is-invalid');
    
    // Validate based on field type
    switch (fieldName) {
        case 'TariffPrice':
        case 'PatientShare':
        case 'InsurerShare':
        case 'SupplementaryMaxPayment':
            if (value && (isNaN(value) || parseFloat(value) < 0)) {
                field.addClass('is-invalid');
                return false;
            }
            break;
        case 'SupplementaryCoveragePercent':
            if (value && (isNaN(value) || parseFloat(value) < 0 || parseFloat(value) > 100)) {
                field.addClass('is-invalid');
                return false;
            }
            break;
    }
    
    if (value) {
        field.addClass('is-valid');
    }
    
    return true;
}

/**
 * Validate form
 * اعتبارسنجی فرم
 */
function validateForm() {
    console.log('🏥 MEDICAL: Validating form');
    
    let isValid = true;
    
    // Validate all required fields
    $('.form-control[required]').each(function() {
        if (!validateField($(this))) {
            isValid = false;
        }
    });
    
    // Validate date range
    if (!validateDateRange()) {
        isValid = false;
    }
    
    // Validate amount range
    if (!validateAmountRange()) {
        isValid = false;
    }
    
    return isValid;
}

/**
 * Update calculation preview
 * به‌روزرسانی پیش‌نمایش محاسبات
 */
function updateCalculationPreview() {
    console.log('🏥 MEDICAL: Updating calculation preview');
    
    const serviceAmount = parseFloat($('#TariffPrice').val().replace(/,/g, '')) || 0;
    const primaryCoverage = parseFloat($('#InsurerShare').val().replace(/,/g, '')) || 0;
    const supplementaryPercent = parseFloat($('#SupplementaryCoveragePercent').val()) || 0;
    
    const remainingAmount = Math.max(0, serviceAmount - primaryCoverage);
    const supplementaryCoverage = (remainingAmount * supplementaryPercent) / 100;
    const finalPatientShare = Math.max(0, remainingAmount - supplementaryCoverage);
    
    // Update preview elements
    $('#previewServiceAmount').text(serviceAmount.toLocaleString() + ' تومان');
    $('#previewPrimaryCoverage').text(primaryCoverage.toLocaleString() + ' تومان');
    $('#previewRemainingAmount').text(remainingAmount.toLocaleString() + ' تومان');
    $('#previewSupplementaryPercent').text(supplementaryPercent.toFixed(2) + '%');
    $('#previewFinalPatientShare').text(finalPatientShare.toLocaleString() + ' تومان');
}

/**
 * Clear filters
 * پاک کردن فیلترها
 */
function clearFilters() {
    console.log('🏥 MEDICAL: Clearing filters');
    
    $('#filterForm')[0].reset();
    $('#filterForm').submit();
}

/**
 * Show loading
 * نمایش بارگذاری
 */
function showLoading() {
    $('.loading-overlay').show();
}

/**
 * Hide loading
 * مخفی کردن بارگذاری
 */
function hideLoading() {
    $('.loading-overlay').hide();
}

/**
 * Show error
 * نمایش خطا
 */
function showError(message) {
    console.error('🏥 MEDICAL: Error:', message);
    
    // Show toastr notification
    if (typeof toastr !== 'undefined') {
        toastr.error(message);
    } else {
        alert(message);
    }
}

/**
 * Show success
 * نمایش موفقیت
 */
function showSuccess(message) {
    console.log('🏥 MEDICAL: Success:', message);
    
    // Show toastr notification
    if (typeof toastr !== 'undefined') {
        toastr.success(message);
    } else {
        alert(message);
    }
}

/**
 * Reset form
 * بازنشانی فرم
 */
function resetForm() {
    if (confirm('آیا مطمئن هستید که می‌خواهید فرم را بازنشانی کنید؟')) {
        $('#editForm')[0].reset();
        updateCalculationPreview();
        isFormDirty = false;
    }
}

/**
 * Confirm delete
 * تأیید حذف
 */
function confirmDelete() {
    if (confirm('آیا مطمئن هستید که می‌خواهید این تعرفه بیمه تکمیلی را حذف کنید؟\n\nاین عمل قابل بازگشت نیست و تمام محاسبات مرتبط متوقف خواهد شد.')) {
        if (confirm('آیا واقعاً مطمئن هستید؟\n\nاین تعرفه برای همیشه حذف خواهد شد.')) {
            $('#deleteForm').submit();
        }
    }
}

/**
 * Initialize supplementary tariff page
 * راه‌اندازی صفحه تعرفه‌های بیمه تکمیلی
 */
function initializeSupplementaryTariffPage() {
    console.log('🏥 MEDICAL: Initializing Supplementary Tariff Page');
    
    // Set current view
    currentView = 'index';
    
    // Initialize the page
    initializeSupplementaryTariff();
    
    // Initialize specific page components
    initializePageComponents();
}

/**
 * Initialize page-specific components
 * راه‌اندازی اجزای مخصوص صفحه
 */
function initializePageComponents() {
    console.log('🏥 MEDICAL: Initializing Page Components');
    
    // Initialize statistics
    initializeStatistics();
    
    // Initialize filters
    initializeFilters();
    
    // Initialize search
    initializeSearch();
    
    // Initialize view toggle
    initializeViewToggle();
    
    // Initialize modals
    initializeModals();
    
    // Initialize action buttons
    initializeActionButtons();
}

/**
 * Initialize statistics
 * راه‌اندازی آمار
 */
function initializeStatistics() {
    console.log('🏥 MEDICAL: Initializing Statistics');
    
    // Load statistics on page load
    loadStatistics();
}

/**
 * Initialize modals
 * راه‌اندازی مودال‌ها
 */
function initializeModals() {
    console.log('🏥 MEDICAL: Initializing Modals');
    
    // Initialize create modal
    $('#createModal').on('show.bs.modal', function() {
        console.log('🏥 MEDICAL: Create modal opening');
        loadCreateModalData();
    });
    
    // Initialize edit modal
    $('#editModal').on('show.bs.modal', function() {
        console.log('🏥 MEDICAL: Edit modal opening');
        loadEditModalData();
    });
    
    // Initialize delete modal
    $('#deleteModal').on('show.bs.modal', function() {
        console.log('🏥 MEDICAL: Delete modal opening');
        loadDeleteModalData();
    });
}

/**
 * Load statistics
 * بارگذاری آمار
 */
function loadStatistics() {
    console.log('🏥 MEDICAL: Loading Statistics');
    
    // This would typically make an AJAX call to get statistics
    // For now, we'll just log that it's being called
    console.log('🏥 MEDICAL: Statistics loaded');
}

/**
 * Load tariffs
 * بارگذاری تعرفه‌ها
 */
function loadTariffs() {
    console.log('🏥 MEDICAL: Loading Tariffs');
    
    // This would typically make an AJAX call to get tariffs
    // For now, we'll just log that it's being called
    console.log('🏥 MEDICAL: Tariffs loaded');
}

/**
 * Load filter options
 * بارگذاری گزینه‌های فیلتر
 */
function loadFilterOptions() {
    console.log('🏥 MEDICAL: Loading Filter Options');
    
    // This would typically make an AJAX call to get filter options
    // For now, we'll just log that it's being called
    console.log('🏥 MEDICAL: Filter options loaded');
}

/**
 * Load create modal data
 * بارگذاری داده‌های مودال ایجاد
 */
function loadCreateModalData() {
    console.log('🏥 MEDICAL: Loading Create Modal Data');
    
    // This would typically make an AJAX call to get data for create modal
    // For now, we'll just log that it's being called
    console.log('🏥 MEDICAL: Create modal data loaded');
}

/**
 * Load edit modal data
 * بارگذاری داده‌های مودال ویرایش
 */
function loadEditModalData() {
    console.log('🏥 MEDICAL: Loading Edit Modal Data');
    
    // This would typically make an AJAX call to get data for edit modal
    // For now, we'll just log that it's being called
    console.log('🏥 MEDICAL: Edit modal data loaded');
}

/**
 * Load delete modal data
 * بارگذاری داده‌های مودال حذف
 */
function loadDeleteModalData() {
    console.log('🏥 MEDICAL: Loading Delete Modal Data');
    
    // This would typically make an AJAX call to get data for delete modal
    // For now, we'll just log that it's being called
    console.log('🏥 MEDICAL: Delete modal data loaded');
}

/**
 * Delete tariff
 * حذف تعرفه
 */
function deleteTariff() {
    console.log('🏥 MEDICAL: Deleting Tariff');
    
    // This would typically make an AJAX call to delete the tariff
    // For now, we'll just log that it's being called
    console.log('🏥 MEDICAL: Tariff deleted');
    
    // Close modal
    $('#deleteModal').modal('hide');
}


// Initialize when document is ready
$(document).ready(function() {
    console.log('🏥 MEDICAL: Document ready, initializing Supplementary Tariff Management');
    initializeSupplementaryTariff();
});
