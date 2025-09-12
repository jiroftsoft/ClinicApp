/**
 * AJAX Response Parser Helper
 * 
 * این فایل برای حل مشکل jQuery Response Parsing ایجاد شده است.
 * طبق قرارداد JQUERY_RESPONSE_PARSING_FIX.md، همیشه باید از این helper استفاده شود.
 * 
 * @author AI Assistant
 * @date 1404/06/21
 * @version 1.0
 */

/**
 * تجزیه Response دریافتی از AJAX
 * @param {any} response - Response دریافتی از jQuery AJAX
 * @returns {Object} - Response تجزیه شده
 * @throws {Error} - در صورت خطا در تجزیه
 */
function parseAjaxResponse(response) {
    console.log('🔍 Parsing AJAX Response:', response);
    console.log('🔍 Response Type:', typeof response);
    
    // اگر response قبلاً object است، مستقیماً برگردان
    if (typeof response === 'object' && response !== null) {
        console.log('✅ Response is already an object');
        return response;
    }
    
    // اگر response string است، آن را تجزیه کن
    if (typeof response === 'string') {
        try {
            var parsedResponse = JSON.parse(response);
            console.log('✅ Response parsed successfully:', parsedResponse);
            return parsedResponse;
        } catch (e) {
            console.error('❌ JSON Parse Error:', e);
            throw new Error('خطا در تجزیه پاسخ سرور: ' + e.message);
        }
    }
    
    // اگر response نوع نامشخصی دارد
    console.warn('⚠️ Unknown response type:', typeof response);
    return response;
}

/**
 * بررسی موفقیت Response
 * @param {Object} response - Response تجزیه شده
 * @returns {boolean} - آیا response موفق است یا نه
 */
function isResponseSuccessful(response) {
    return response && response.success === true;
}

/**
 * دریافت داده‌های Response
 * @param {Object} response - Response تجزیه شده
 * @returns {any} - داده‌های Response
 */
function getResponseData(response) {
    return response && response.data ? response.data : null;
}

/**
 * دریافت پیام Response
 * @param {Object} response - Response تجزیه شده
 * @returns {string} - پیام Response
 */
function getResponseMessage(response) {
    return response && response.message ? response.message : 'پیام نامشخص';
}

/**
 * Template برای AJAX Success Handler
 * @param {Function} onSuccess - تابع اجرا در صورت موفقیت
 * @param {Function} onError - تابع اجرا در صورت خطا
 * @returns {Function} - Success Handler
 */
function createAjaxSuccessHandler(onSuccess, onError) {
    return function (response) {
        try {
            var parsedResponse = parseAjaxResponse(response);
            
            if (isResponseSuccessful(parsedResponse)) {
                var data = getResponseData(parsedResponse);
                if (onSuccess) {
                    onSuccess(parsedResponse, data);
                }
            } else {
                var message = getResponseMessage(parsedResponse);
                if (onError) {
                    onError(parsedResponse, message);
                } else {
                    showError('خطا در درخواست: ' + message);
                }
            }
        } catch (error) {
            console.error('❌ Error in AJAX Success Handler:', error);
            if (onError) {
                onError(null, error.message);
            } else {
                showError('خطا در پردازش پاسخ: ' + error.message);
            }
        }
    };
}

/**
 * Template برای AJAX Error Handler
 * @param {Function} onError - تابع اجرا در صورت خطا
 * @returns {Function} - Error Handler
 */
function createAjaxErrorHandler(onError) {
    return function (xhr, status, error) {
        console.error('❌ AJAX Error:', error);
        console.error('❌ XHR Status:', xhr.status);
        console.error('❌ XHR Response:', xhr.responseText);
        
        var errorMessage = 'خطا در ارتباط با سرور';
        if (xhr.responseJSON && xhr.responseJSON.message) {
            errorMessage = xhr.responseJSON.message;
        } else if (error) {
            errorMessage = error;
        }
        
        if (onError) {
            onError(xhr, status, error, errorMessage);
        } else {
            showError(errorMessage);
        }
    };
}

/**
 * مثال استفاده:
 * 
 * $.ajax({
 *     url: '/api/endpoint',
 *     type: 'GET',
 *     success: createAjaxSuccessHandler(
 *         function(parsedResponse, data) {
 *             // موفقیت
 *             console.log('Success:', data);
 *         },
 *         function(parsedResponse, message) {
 *             // خطا
 *             showError(message);
 *         }
 *     ),
 *     error: createAjaxErrorHandler(
 *         function(xhr, status, error, errorMessage) {
 *             // خطای AJAX
 *             showError(errorMessage);
 *         }
 *     )
 * });
 */
