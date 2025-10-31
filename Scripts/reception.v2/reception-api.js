(function (w, $) {
  const baseV1 = '/api/v1/reception';
  const legacyBase = '/Api/ReceptionApi';

  function token() {
    return $('input[name="__RequestVerificationToken"]').val() || '';
  }

  function headers(method) {
    const h = {};
    if (method.toUpperCase() !== 'GET') {
      const t = token();
      if (t) {
        // MVC 5 accepts token in header as RequestVerificationToken
        h['RequestVerificationToken'] = t;
        // Also add X-RequestVerificationToken as fallback
        h['X-RequestVerificationToken'] = t;
      }
    }
    h['X-Requested-With'] = 'XMLHttpRequest';
    return h;
  }

  function stamp(url) {
    return url + (url.indexOf('?') > -1 ? '&' : '?') + '_ts=' + Date.now();
  }

  function shouldFallback(jqXHR) {
    const s = jqXHR && jqXHR.status;
    const body = (jqXHR && jqXHR.responseText || '').toLowerCase();
    return s === 404 || s === 500 || s === 0 ||
           body.indexOf('resource cannot be found') > -1 ||
           body.indexOf('was not found') > -1 ||
           body.indexOf('not found') > -1;
  }

  function toLegacyPath(path) {
    // map v1 paths -> legacy actions
    if (/^\/?bootstrap/i.test(path)) return 'Bootstrap';
    if (/^\/?draft\/create/i.test(path)) return 'CreateDraft';
    if (/^\/?patient\/lookup-or-create/i.test(path)) return 'PatientLookup';
    if (/^\/?services\/by-department/i.test(path)) {
      const q = path.indexOf('?') > -1 ? path.substring(path.indexOf('?')) : '';
      return 'GetServicesForDepartment' + q;
    }
    if (/^\/?insurance\/plans/i.test(path)) {
      const q = path.indexOf('?') > -1 ? path.substring(path.indexOf('?')) : '';
      return 'GetInsurancePlans' + q;
    }
    if (/^\/?doctors\/by-service/i.test(path)) {
      const q = path.indexOf('?') > -1 ? path.substring(path.indexOf('?')) : '';
      return 'GetDoctorsByService' + q;
    }
    if (/^\/?item\/add/i.test(path)) return 'AddItem';
    if (/^\/?item\/remove/i.test(path)) return 'RemoveItem';
    if (/^\/?draft\/update/i.test(path)) return 'DraftUpdate';
    if (/^\/?insurances\/set/i.test(path)) return 'SetInsurances';
    if (/^\/?finalize\/pos/i.test(path)) return 'FinalizeWithPos';
    if (/^\/?finalize\/cash/i.test(path)) return 'FinalizeWithCash';
    if (/^\/?health/i.test(path)) return 'Health';
    // Remove leading slash if present
    return path.replace(/^\//, '');
  }

  /**
   * ✅ گام 8 - قلاب کوچک در Frontend برای تجربه بهتر کاربر
   * اگر پاسخ JSON با Code === "ANTIFORGERY_MISSING" یا "UNHANDLED" بود، پیام کاربرپسند بده
   */
  function handleErrorJson(res) {
    if (!res) return false;

    // بررسی ANTIFORGERY_MISSING
    if (res.Code === 'ANTIFORGERY_MISSING' || res.code === 'ANTIFORGERY_MISSING') {
      console.warn('🏥 V2: CSRF token missing/expired', res);
      toastr.error('توکن امنیتی منقضی شده است. لطفاً صفحه را نوسازی کنید.', 'خطای امنیتی', {
        timeOut: 5000,
        extendedTimeOut: 3000
      });
      
      // پیشنهاد Refresh (اختیاری)
      if (confirm('آیا می‌خواهید صفحه را نوسازی کنید؟')) {
        window.location.reload();
      }
      
      return true; // خطا مصرف شد
    }

    // بررسی UNHANDLED
    if (res.Code === 'UNHANDLED' || res.code === 'UNHANDLED') {
      // در Dev، Metadata شامل Exception/StackTrace است؛ برای Console کافی است
      if (res.Metadata && (res.Metadata.Exception || res.Metadata.StackTrace)) {
        console.error('[DEV] Unhandled error:', res.Metadata);
      } else {
        console.error('[DEV] Unhandled error:', res);
      }
      
      toastr.error('خطای غیرمنتظره رخ داد. لطفاً مجدداً تلاش کنید.', 'خطا', {
        timeOut: 5000
      });
      
      return true; // خطا مصرف شد
    }

    return false; // خطا مصرف نشد
  }

  function ajaxWithFallback(method, path, data) {
    const d = $.Deferred();
    const cleanPath = path.replace(/^\//, ''); // Remove leading slash

    $.ajax({
      url: stamp(baseV1 + '/' + cleanPath),
      type: method,
      data: method === 'GET' ? undefined : JSON.stringify(data || {}),
      contentType: method === 'GET' ? undefined : 'application/json; charset=utf-8',
      dataType: 'json', // اجباری: jQuery باید response را به JSON parse کند
      cache: false,
      headers: headers(method)
    })
    .done(res => {
      // ✅ گام 8: بررسی خطاهای خاص (ANTIFORGERY_MISSING, UNHANDLED)
      if (!handleErrorJson(res)) {
        d.resolve(res);
      } else {
        // اگر خطا مصرف شد، reject نکنیم چون UI خودش handle کرده
        d.resolve(res);
      }
    })
    .fail(jq => {
      // ✅ گام 8: بررسی response JSON در صورت خطای HTTP
      try {
        if (jq.responseJSON) {
          if (handleErrorJson(jq.responseJSON)) {
            // خطا مصرف شد، reject نکنیم
            d.resolve(jq.responseJSON);
            return;
          }
        }
      } catch (e) {
        // Ignore
      }

      if (shouldFallback(jq)) {
        var legacyPath = toLegacyPath(cleanPath);
        $.ajax({
          url: stamp(legacyBase + '/' + legacyPath),
          type: method === 'GET' ? 'GET' : 'POST',
          data: method === 'GET' ? (data || {}) : (data || {}),
          dataType: 'json', // اجباری: jQuery باید response را به JSON parse کند
          cache: false,
          headers: headers(method)
        })
        .done(res => {
          // ✅ گام 8: بررسی خطاهای خاص در fallback
          if (!handleErrorJson(res)) {
            d.resolve(res);
          } else {
            d.resolve(res);
          }
        })
        .fail(err => {
          // ✅ گام 8: بررسی response JSON در خطای fallback
          try {
            if (err.responseJSON && handleErrorJson(err.responseJSON)) {
              d.resolve(err.responseJSON);
              return;
            }
          } catch (e) {
            // Ignore
          }
          d.reject(err);
        });
      } else {
        d.reject(jq);
      }
    });

    return d.promise();
  }

  function ok(res) {
    return (res && (res.Success === true || res.success === true)) ? (res.Data ?? res.data ?? res) : res;
  }

  // ✅ Race-safe: Token برای Reprice (ضد درخواست‌های کهنه)
  let currentPricingToken = 0;

  /**
   * ✅ setInsurancesAndReprice: با Token برای دور انداختن پاسخ‌های کهنه
   */
  async function setInsurancesAndReprice(payload) {
    const token = ++currentPricingToken;
    const res = await ajaxWithFallback('POST', '/insurances/set', payload);
    
    // اگر token تغییر کرده، پاسخ کهنه را دور بینداز
    if (token !== currentPricingToken) {
      console.warn('🏥 V2: Reprice response ignored (outdated token)', token, currentPricingToken);
      return null;
    }
    
    return res;
  }

  // Public API
  w.ReceptionAPI = {
    get: (path, params) => ajaxWithFallback('GET', path + (params ? ('?' + $.param(params)) : ''), null),
    post: (path, body) => ajaxWithFallback('POST', path, body),
    ok: ok,
    setInsurancesAndReprice: setInsurancesAndReprice // ✅ جدید: برای Reprice با Token
  };
})(window, jQuery);
