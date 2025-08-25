/**
 * Medical Environment Toast Notifications
 * سیستم نمایش پیام‌های محیط درمانی
 * 
 * ویژگی‌ها:
 * ✅ نمایش پیام‌های موفقیت، خطا و هشدار
 * ✅ پشتیبانی از Bootstrap 5
 * ✅ مدیریت خودکار container
 * ✅ حذف خودکار بعد از نمایش
 * ✅ پشتیبانی از RTL
 * ✅ مناسب برای محیط درمانی
 */

// تابع نمایش پیام برای محیط درمانی
function showMedicalToast(title, message, type = 'info') {
    console.log('🏥 MEDICAL: Showing toast:', {title, message, type});
    
    // اطمینان از وجود پیام
    const displayMessage = message || 'پیام نامشخص';
    const displayTitle = title || 'اطلاعیه';
    
    // ایجاد toast container اگر وجود نداشته باشد
    if (!$('#medicalToastContainer').length) {
        $('body').append('<div id="medicalToastContainer" class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 9999; direction: rtl;"></div>');
    }
    
    // تعیین کلاس بر اساس نوع
    let toastClass, iconClass;
    switch (type) {
        case 'success':
            toastClass = 'bg-success';
            iconClass = 'fas fa-check-circle';
            break;
        case 'warning':
            toastClass = 'bg-warning';
            iconClass = 'fas fa-exclamation-triangle';
            break;
        case 'error':
            toastClass = 'bg-danger';
            iconClass = 'fas fa-times-circle';
            break;
        case 'info':
        default:
            toastClass = 'bg-info';
            iconClass = 'fas fa-info-circle';
            break;
    }
    
    // ایجاد toast
    const toast = `
        <div class="toast ${toastClass} text-white border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header ${toastClass} text-white border-0">
                <i class="${iconClass} me-2"></i>
                <strong class="me-auto">${displayTitle}</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${displayMessage}
            </div>
        </div>
    `;
    
    // اضافه کردن toast به container
    $('#medicalToastContainer').append(toast);
    
    // نمایش toast
    const toastElement = $('#medicalToastContainer .toast').last();
    const bsToast = new bootstrap.Toast(toastElement, {
        autohide: true,
        delay: type === 'success' ? 3000 : type === 'warning' ? 4000 : 5000
    });
    bsToast.show();
    
    // حذف toast بعد از نمایش
    toastElement.on('hidden.bs.toast', function() {
        $(this).remove();
    });
}

// تابع نمایش پیام موفقیت
function showMedicalSuccess(message, title = '✅ موفقیت') {
    showMedicalToast(title, message, 'success');
}

// تابع نمایش پیام خطا
function showMedicalError(message, title = '❌ خطا') {
    showMedicalToast(title, message, 'error');
}

// تابع نمایش پیام هشدار
function showMedicalWarning(message, title = '⚠️ هشدار') {
    showMedicalToast(title, message, 'warning');
}

// تابع نمایش پیام اطلاعات
function showMedicalInfo(message, title = 'ℹ️ اطلاعات') {
    showMedicalToast(title, message, 'info');
}

// تابع پاک کردن تمام toast ها
function clearMedicalToasts() {
    $('#medicalToastContainer').empty();
}

// تابع بررسی وجود toast container
function hasMedicalToastContainer() {
    return $('#medicalToastContainer').length > 0;
}
