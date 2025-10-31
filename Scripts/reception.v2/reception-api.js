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
    .done(res => d.resolve(res))
    .fail(jq => {
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
        .done(res => d.resolve(res))
        .fail(err => d.reject(err));
      } else {
        d.reject(jq);
      }
    });

    return d.promise();
  }

  function ok(res) {
    return (res && (res.Success === true || res.success === true)) ? (res.Data ?? res.data ?? res) : res;
  }

  // Public API
  w.ReceptionAPI = {
    get: (path, params) => ajaxWithFallback('GET', path + (params ? ('?' + $.param(params)) : ''), null),
    post: (path, body) => ajaxWithFallback('POST', path, body),
    ok: ok
  };
})(window, jQuery);
