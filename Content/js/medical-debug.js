/**
 * Medical Environment Debug Utilities
 * ابزارهای دیباگ برای محیط درمانی
 * 
 * ویژگی‌ها:
 * ✅ بررسی Anti-Forgery Token
 * ✅ تست AJAX calls
 * ✅ نمایش اطلاعات درخواست
 * ✅ ردیابی خطاها
 * ✅ مناسب برای محیط درمانی
 */

// تابع بررسی Anti-Forgery Token
function checkAntiForgeryToken() {
    console.log('🏥 MEDICAL DEBUG: بررسی Anti-Forgery Token...');
    
    var token = $('input[name="__RequestVerificationToken"]').val();
    if (token) {
        console.log('✅ MEDICAL DEBUG: Anti-Forgery Token یافت شد:', token.substring(0, 20) + '...');
        return true;
    } else {
        console.error('❌ MEDICAL DEBUG: Anti-Forgery Token یافت نشد!');
        return false;
    }
}

// تابع تست AJAX call
function testAjaxCall(url, data, callback) {
    console.log('🏥 MEDICAL DEBUG: تست AJAX call...');
    console.log('🏥 MEDICAL DEBUG: URL:', url);
    console.log('🏥 MEDICAL DEBUG: Data:', data);
    
    $.ajax({
        url: url,
        type: 'POST',
        dataType: 'json',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: data,
        beforeSend: function(xhr) {
            console.log('🏥 MEDICAL DEBUG: قبل از ارسال درخواست...');
            console.log('🏥 MEDICAL DEBUG: Headers:', xhr.getAllResponseHeaders());
        },
        success: function(result) {
            console.log('✅ MEDICAL DEBUG: درخواست موفق:', result);
            if (callback) callback(result);
        },
        error: function(xhr, status, error) {
            console.error('❌ MEDICAL DEBUG: خطا در درخواست:', {
                status: status,
                error: error,
                responseText: xhr.responseText,
                responseJSON: xhr.responseJSON
            });
            if (callback) callback(null, error);
        }
    });
}

// تابع نمایش اطلاعات درخواست
function logRequestInfo() {
    console.log('🏥 MEDICAL DEBUG: اطلاعات درخواست فعلی...');
    console.log('🏥 MEDICAL DEBUG: URL:', window.location.href);
    console.log('🏥 MEDICAL DEBUG: Method:', window.location.protocol);
    console.log('🏥 MEDICAL DEBUG: User Agent:', navigator.userAgent);
    console.log('🏥 MEDICAL DEBUG: jQuery Version:', $.fn.jquery);
    console.log('🏥 MEDICAL DEBUG: Bootstrap Version:', typeof bootstrap !== 'undefined' ? 'Available' : 'Not Available');
}

// تابع بررسی عملکرد toast
function testToastSystem() {
    console.log('🏥 MEDICAL DEBUG: تست سیستم toast...');
    
    if (typeof showMedicalToast === 'function') {
        console.log('✅ MEDICAL DEBUG: تابع showMedicalToast موجود است');
        showMedicalToast('تست دیباگ', 'این یک پیام تست است', 'info');
    } else {
        console.error('❌ MEDICAL DEBUG: تابع showMedicalToast موجود نیست!');
    }
    
    if (typeof showMedicalSuccess === 'function') {
        console.log('✅ MEDICAL DEBUG: تابع showMedicalSuccess موجود است');
    } else {
        console.error('❌ MEDICAL DEBUG: تابع showMedicalSuccess موجود نیست!');
    }
}

// تابع بررسی عملکرد حذف خدمت
function testServiceDelete(serviceId) {
    console.log('🏥 MEDICAL DEBUG: تست حذف خدمت...');
    
    if (!checkAntiForgeryToken()) {
        console.error('❌ MEDICAL DEBUG: Anti-Forgery Token موجود نیست!');
        return;
    }
    
    var token = $('input[name="__RequestVerificationToken"]').val();
    var data = {
        id: serviceId,
        __RequestVerificationToken: token
    };
    
    console.log('🏥 MEDICAL DEBUG: ارسال درخواست حذف خدمت...');
    console.log('🏥 MEDICAL DEBUG: ServiceId:', serviceId);
    console.log('🏥 MEDICAL DEBUG: Token:', token ? token.substring(0, 20) + '...' : 'NULL');
    
    $.ajax({
        url: '/Admin/Service/Delete',
        type: 'POST',
        dataType: 'json',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: data,
        beforeSend: function(xhr) {
            console.log('🏥 MEDICAL DEBUG: قبل از ارسال درخواست...');
            console.log('🏥 MEDICAL DEBUG: Headers:', xhr.getAllResponseHeaders());
        },
        success: function(result) {
            console.log('✅ MEDICAL DEBUG: درخواست موفق:', result);
            if (result.success) {
                console.log('✅ MEDICAL DEBUG: خدمت با موفقیت حذف شد');
            } else {
                console.error('❌ MEDICAL DEBUG: حذف ناموفق:', result.message);
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ MEDICAL DEBUG: خطای AJAX:', {
                status: status,
                error: error,
                responseText: xhr.responseText,
                responseJSON: xhr.responseJSON,
                statusText: xhr.statusText
            });
            
            // Try to parse response text for more details
            try {
                var response = JSON.parse(xhr.responseText);
                console.error('❌ MEDICAL DEBUG: Response parsed:', response);
            } catch (e) {
                console.error('❌ MEDICAL DEBUG: Could not parse response as JSON');
            }
        }
    });
}

// تابع بررسی عملکرد حذف دسته‌بندی
function testCategoryDelete(categoryId) {
    console.log('🏥 MEDICAL DEBUG: تست حذف دسته‌بندی...');
    
    if (!checkAntiForgeryToken()) {
        console.error('❌ MEDICAL DEBUG: Anti-Forgery Token موجود نیست!');
        return;
    }
    
    var token = $('input[name="__RequestVerificationToken"]').val();
    var data = {
        id: categoryId,
        __RequestVerificationToken: token
    };
    
    testAjaxCall('/Admin/Service/DeleteCategory', data, function(result, error) {
        if (result) {
            console.log('✅ MEDICAL DEBUG: حذف دسته‌بندی موفق:', result);
        } else {
            console.error('❌ MEDICAL DEBUG: حذف دسته‌بندی ناموفق:', error);
        }
    });
}

// تابع بررسی کامل سیستم
function runMedicalDebug() {
    console.log('🏥 MEDICAL DEBUG: شروع بررسی کامل سیستم...');
    console.log('='.repeat(50));
    
    logRequestInfo();
    console.log('-'.repeat(30));
    
    checkAntiForgeryToken();
    console.log('-'.repeat(30));
    
    testToastSystem();
    console.log('-'.repeat(30));
    
    console.log('🏥 MEDICAL DEBUG: بررسی کامل سیستم پایان یافت.');
    console.log('='.repeat(50));
}

// تابع نمایش اطلاعات خطا
function logErrorDetails(error, context) {
    console.error('🏥 MEDICAL DEBUG: جزئیات خطا در', context);
    console.error('🏥 MEDICAL DEBUG: نوع خطا:', error.constructor.name);
    console.error('🏥 MEDICAL DEBUG: پیام خطا:', error.message);
    console.error('🏥 MEDICAL DEBUG: Stack trace:', error.stack);
    
    if (error.responseJSON) {
        console.error('🏥 MEDICAL DEBUG: Response JSON:', error.responseJSON);
    }
    
    if (error.responseText) {
        console.error('🏥 MEDICAL DEBUG: Response Text:', error.responseText);
    }
}

// تابع بررسی عملکرد در زمان load صفحه
$(document).ready(function() {
    console.log('🏥 MEDICAL DEBUG: صفحه بارگذاری شد');
    
    // اضافه کردن event listener برای خطاهای AJAX
    $(document).ajaxError(function(event, xhr, settings, error) {
        console.error('🏥 MEDICAL DEBUG: خطای AJAX رخ داد:', {
            url: settings.url,
            type: settings.type,
            status: xhr.status,
            error: error
        });
    });
    
    // اضافه کردن event listener برای موفقیت AJAX
    $(document).ajaxSuccess(function(event, xhr, settings) {
        console.log('🏥 MEDICAL DEBUG: درخواست AJAX موفق:', {
            url: settings.url,
            type: settings.type,
            status: xhr.status
        });
    });
});

// تابع تست سریع
function quickTest() {
    console.log('🏥 MEDICAL DEBUG: تست سریع...');
    runMedicalDebug();
    testToastSystem();
    clearFontCache();
}

/**
 * Clear browser cache for fonts and CSS
 * پاک کردن کش مرورگر برای فونت‌ها و CSS
 */
function clearFontCache() {
    console.log('🏥 MEDICAL DEBUG: Clearing font cache...');
    
    // Force reload of CSS files
    const links = document.querySelectorAll('link[rel="stylesheet"]');
    links.forEach(link => {
        const url = new URL(link.href);
        url.searchParams.set('v', Date.now());
        link.href = url.toString();
    });
    
    // Force reload of font files
    const styleSheets = document.styleSheets;
    for (let i = 0; i < styleSheets.length; i++) {
        try {
            const rules = styleSheets[i].cssRules || styleSheets[i].rules;
            for (let j = 0; j < rules.length; j++) {
                if (rules[j].type === CSSRule.FONT_FACE_RULE) {
                    const src = rules[j].style.getPropertyValue('src');
                    if (src.includes('Vazirmatn')) {
                        console.log('🏥 MEDICAL DEBUG: Found Vazirmatn font rule:', src);
                    }
                }
            }
        } catch (e) {
            // Cross-origin stylesheets will throw an error
        }
    }
    
    console.log('🏥 MEDICAL DEBUG: Font cache cleared');
}

/**
 * Enhanced debugging for service deletion issues
 * دیباگ پیشرفته برای مشکلات حذف خدمت
 */
function debugServiceDeletion(serviceId) {
    console.log('🏥 MEDICAL DEBUG: شروع دیباگ حذف خدمت...');
    console.log('='.repeat(60));
    
    // Check current page state
    console.log('🏥 MEDICAL DEBUG: Current URL:', window.location.href);
    console.log('🏥 MEDICAL DEBUG: Service ID to delete:', serviceId);
    
    // Check if service exists in DOM
    var serviceRow = $(`tr[data-service-id="${serviceId}"]`);
    var serviceCard = $(`.service-card[data-service-id="${serviceId}"]`);
    
    console.log('🏥 MEDICAL DEBUG: Service row found:', serviceRow.length > 0);
    console.log('🏥 MEDICAL DEBUG: Service card found:', serviceCard.length > 0);
    
    // Check Anti-Forgery Token
    var token = $('input[name="__RequestVerificationToken"]').val();
    console.log('🏥 MEDICAL DEBUG: Anti-Forgery Token exists:', !!token);
    console.log('🏥 MEDICAL DEBUG: Token length:', token ? token.length : 0);
    
    // Check jQuery and AJAX availability
    console.log('🏥 MEDICAL DEBUG: jQuery available:', typeof $ !== 'undefined');
    console.log('🏥 MEDICAL DEBUG: $.ajax available:', typeof $.ajax !== 'undefined');
    
    // Test the actual deletion
    console.log('🏥 MEDICAL DEBUG: Testing service deletion...');
    testServiceDelete(serviceId);
    
    console.log('='.repeat(60));
    console.log('🏥 MEDICAL DEBUG: دیباگ حذف خدمت پایان یافت');
}

/**
 * Check server logs for detailed error information
 * بررسی لاگ‌های سرور برای اطلاعات دقیق خطا
 */
function checkServerLogs() {
    console.log('🏥 MEDICAL DEBUG: بررسی لاگ‌های سرور...');
    
    // This would typically be done server-side, but we can provide guidance
    console.log('🏥 MEDICAL DEBUG: برای بررسی لاگ‌های سرور:');
    console.log('🏥 MEDICAL DEBUG: 1. فایل‌های لاگ را در پوشه Logs بررسی کنید');
    console.log('🏥 MEDICAL DEBUG: 2. Event Viewer را بررسی کنید');
    console.log('🏥 MEDICAL DEBUG: 3. خطاهای Entity Framework را بررسی کنید');
    console.log('🏥 MEDICAL DEBUG: 4. خطاهای پایگاه داده را بررسی کنید');
    
    // Provide common error patterns to look for
    console.log('🏥 MEDICAL DEBUG: الگوهای خطای رایج:');
    console.log('🏥 MEDICAL DEBUG: - SqlException: خطای پایگاه داده');
    console.log('🏥 MEDICAL DEBUG: - InvalidOperationException: خطای عملیاتی');
    console.log('🏥 MEDICAL DEBUG: - DbUpdateException: خطای به‌روزرسانی دیتابیس');
    console.log('🏥 MEDICAL DEBUG: - ValidationException: خطای اعتبارسنجی');
}
