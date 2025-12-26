/**
 * 🏥 Reception Error Handler - مدیریت حرفه‌ای خطاها برای منشی
 * 
 * این ماژول تمام خطاهای API را به زبان ساده و کاربرپسند تبدیل می‌کند
 * برای منشی‌هایی که دانش فنی ندارند
 * 
 * @version 1.0.0
 * @date 1404/10/05
 */

(function(window, $) {
  'use strict';

  // =====================================================
  // 📋 ERROR CODE MAPPING - نگاشت کدهای خطا به پیام‌های کاربرپسند
  // =====================================================

  const ERROR_MESSAGES = {
    // خطاهای بیمار
    'NOT_FOUND': '❌ بیمار با این کد ملی یافت نشد.\n\n💡 لطفاً:\n• کد ملی را دوباره بررسی کنید\n• اگر بیمار جدید است، اطلاعات او را کامل کنید',
    'CREATE_FAILED': '❌ ثبت بیمار انجام نشد.\n\n💡 لطفاً:\n• همه فیلدهای الزامی را پر کنید\n• کد ملی و شماره موبایل را بررسی کنید',
    'PATIENT_DTO_NULL': '❌ اطلاعات بیمار دریافت نشد.\n\n💡 لطفاً دوباره تلاش کنید',
    'INVALID_PATIENT_ID': '❌ خطا در ثبت بیمار.\n\n💡 لطفاً:\n• دوباره تلاش کنید\n• اگر مشکل ادامه داشت با پشتیبانی تماس بگیرید',
    
    // خطاهای اعتبارسنجی
    'VALIDATION_ERROR': '❌ اطلاعات وارد شده نامعتبر است.\n\n💡 لطفاً موارد زیر را بررسی کنید:',
    
    // خطاهای کد ملی
    'INVALID_NATIONAL_CODE': '❌ کد ملی نامعتبر است.\n\n💡 لطفاً:\n• کد ملی را 10 رقمی وارد کنید\n• اعداد را درست تایپ کنید',
    'DUPLICATE_NATIONAL_CODE': '❌ بیمار با این کد ملی قبلاً ثبت شده.\n\n💡 از قسمت "جستجو" اطلاعات او را پیدا کنید',
    
    // خطاهای موبایل
    'INVALID_MOBILE': '❌ شماره موبایل نامعتبر است.\n\n💡 لطفاً:\n• شماره را با 09 شروع کنید\n• 11 رقم وارد کنید (مثال: 09123456789)',
    
    // خطاهای سرویس
    'SERVICE_NOT_FOUND': '❌ خدمت مورد نظر یافت نشد.\n\n💡 لطفاً:\n• خدمت دیگری انتخاب کنید\n• با پشتیبانی تماس بگیرید',
    'SERVICE_UNAVAILABLE': '❌ سرویس در دسترس نیست.\n\n💡 لطفاً:\n• چند لحظه صبر کنید\n• دوباره تلاش کنید',
    
    // خطاهای پذیرش
    'DRAFT_NOT_FOUND': '❌ پذیرش یافت نشد.\n\n💡 لطفاً:\n• پذیرش جدید ایجاد کنید\n• از لیست پذیرش‌ها مورد نظر را انتخاب کنید',
    
    // خطاهای بیمه
    'INSURANCE_ERROR': '❌ خطا در تنظیم بیمه.\n\n💡 لطفاً:\n• بیمه را دوباره انتخاب کنید\n• بدون بیمه ادامه دهید',
    
    // خطاهای پرداخت
    'PAYMENT_FAILED': '❌ پرداخت انجام نشد.\n\n💡 لطفاً:\n• دوباره تلاش کنید\n• از روش پرداخت دیگری استفاده کنید',
    'POS_NOT_READY': '❌ دستگاه کارتخوان آماده نیست.\n\n💡 لطفاً:\n• اتصال دستگاه را بررسی کنید\n• با پشتیبانی فنی تماس بگیرید',
    
    // خطاهای عمومی
    'UNHANDLED': '❌ خطای غیرمنتظره رخ داد.\n\n💡 لطفاً:\n• دوباره تلاش کنید\n• اگر مشکل ادامه داشت با پشتیبانی تماس بگیرید',
    'NETWORK_ERROR': '❌ خطا در ارتباط با سرور.\n\n💡 لطفاً:\n• اتصال اینترنت را بررسی کنید\n• دوباره تلاش کنید'
  };

  // =====================================================
  // 🔍 PARSE API RESPONSE - تجزیه پاسخ API
  // =====================================================

  /**
   * استخراج اطلاعات خطا از پاسخ API
   */
  function parseErrorResponse(response) {
    console.log('🔍 Parsing error response:', response);
    
    // بررسی Success
    if (response && response.Success === false) {
      return {
        code: response.Code || 'UNKNOWN',
        message: response.Message || 'خطای نامشخص',
        validationErrors: response.ValidationErrors || [],
        metadata: response.Metadata || {}
      };
    }
    
    // پاسخ خطای AJAX
    if (response && response.responseJSON) {
      return parseErrorResponse(response.responseJSON);
    }
    
    // خطای Network
    if (response && response.status === 0) {
      return {
        code: 'NETWORK_ERROR',
        message: 'خطا در ارتباط با سرور',
        validationErrors: [],
        metadata: {}
      };
    }
    
    return {
      code: 'UNKNOWN',
      message: response?.statusText || 'خطای نامشخص',
      validationErrors: [],
      metadata: {}
    };
  }

  // =====================================================
  // 💬 BUILD USER-FRIENDLY MESSAGE
  // =====================================================

  /**
   * ساخت پیام کاربرپسند برای منشی
   */
  function buildUserFriendlyMessage(errorInfo) {
    let message = '';
    let title = 'خطا';
    
    // 1. پیام اصلی از کد خطا
    if (errorInfo.code && ERROR_MESSAGES[errorInfo.code]) {
      message = ERROR_MESSAGES[errorInfo.code];
      title = 'توجه';
    } else if (errorInfo.message) {
      message = `❌ ${errorInfo.message}\n\n💡 لطفاً دوباره تلاش کنید`;
    } else {
      message = ERROR_MESSAGES['UNHANDLED'];
    }
    
    // 2. اضافه کردن ValidationErrors اگر وجود دارد
    if (errorInfo.validationErrors && errorInfo.validationErrors.length > 0) {
      message += '\n\n📋 موارد نیاز به بررسی:\n';
      errorInfo.validationErrors.forEach(err => {
        const field = translateFieldName(err.Field || err.field);
        const error = err.ErrorMessage || err.Message || err.message || 'نامعتبر است';
        message += `\n• ${field}: ${error}`;
      });
    }
    
    // 3. اضافه کردن Metadata errors
    if (errorInfo.metadata && errorInfo.metadata.InsuranceError) {
      message += `\n\n⚠️ بیمه: ${errorInfo.metadata.InsuranceError}`;
    }
    
    // 4. اگر هیچ جزئیاتی نداریم، راهنمایی عمومی بده
    if (errorInfo.validationErrors.length === 0 && !errorInfo.metadata.InsuranceError) {
      message += '\n\n📞 نیاز به کمک؟\nتماس با پشتیبانی: داخلی 100';
    }
    
    return {
      title: title,
      message: message
    };
  }

  /**
   * ترجمه نام فیلدها به فارسی
   */
  function translateFieldName(field) {
    const fieldNames = {
      'NationalCode': 'کد ملی',
      'FirstName': 'نام',
      'LastName': 'نام خانوادگی',
      'FatherName': 'نام پدر',
      'Mobile': 'موبایل',
      'PhoneNumber': 'تلفن',
      'BirthDateShamsi': 'تاریخ تولد',
      'Gender': 'جنسیت',
      'Address': 'آدرس',
      'Email': 'ایمیل',
      'ClinicId': 'کلینیک',
      'DepartmentId': 'بخش',
      'DoctorId': 'پزشک',
      'ServiceId': 'خدمت'
    };
    
    return fieldNames[field] || field || 'فیلد';
  }

  // =====================================================
  // 🎨 SHOW ERROR TO USER
  // =====================================================

  /**
   * نمایش خطا به منشی با Toastr
   */
  function showError(errorResponse, options) {
    const errorInfo = parseErrorResponse(errorResponse);
    const userMessage = buildUserFriendlyMessage(errorInfo);
    
    console.log('💬 Showing error to user:', userMessage);
    
    // استفاده از Toastr
    toastr.error(
      userMessage.message,
      userMessage.title,
      {
        timeOut: 0, // نمایش تا زمانی که کاربر ببندد
        extendedTimeOut: 0,
        closeButton: true,
        progressBar: false,
        positionClass: 'toast-top-center',
        escapeHtml: false, // برای نمایش خطوط جدید
        tapToDismiss: false, // فقط با دکمه بسته شود
        preventDuplicates: true,
        newestOnTop: true,
        // استایل بزرگتر برای خوانایی بهتر
        toastClass: 'toast toast-error toast-reception-error',
        iconClass: 'toast-error',
        messageClass: 'toast-message-large'
      }
    );
    
    // اگر options مشخص شده، از آن استفاده کن
    if (options && options.callback) {
      options.callback(errorInfo);
    }
  }

  /**
   * نمایش هشدار (برای خطاهای کم اهمیت)
   */
  function showWarning(message, title) {
    title = title || 'توجه';
    
    toastr.warning(
      message,
      title,
      {
        timeOut: 8000,
        closeButton: true,
        progressBar: true,
        positionClass: 'toast-top-center',
        escapeHtml: false
      }
    );
  }

  /**
   * نمایش اطلاعات
   */
  function showInfo(message, title) {
    title = title || 'اطلاعات';
    
    toastr.info(
      message,
      title,
      {
        timeOut: 5000,
        closeButton: true,
        progressBar: true,
        positionClass: 'toast-top-center',
        escapeHtml: false
      }
    );
  }

  // =====================================================
  // 🚀 PUBLIC API
  // =====================================================

  window.ReceptionErrorHandler = {
    showError: showError,
    showWarning: showWarning,
    showInfo: showInfo,
    parseErrorResponse: parseErrorResponse,
    buildUserFriendlyMessage: buildUserFriendlyMessage,
    translateFieldName: translateFieldName,
    ERROR_MESSAGES: ERROR_MESSAGES,
    version: '1.0.0'
  };

  console.log('🏥 V2: ReceptionErrorHandler loaded - Version:', window.ReceptionErrorHandler.version);

})(window, jQuery);

