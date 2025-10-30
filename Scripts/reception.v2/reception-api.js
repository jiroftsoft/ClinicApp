(function($, win){
  var KEY = 'rx_api_base_v2';
  var baseV1 = '/api/v1/reception';
  var legacy = '/Api/ReceptionApi';
  var cached = null;
  try { cached = win.sessionStorage.getItem(KEY); } catch(e){}
  var base = cached || baseV1;

  function memo(okBase){
    if(okBase !== base){ base = okBase; try{ win.sessionStorage.setItem(KEY, okBase); }catch(e){} }
  }

  function anti(){ var t = $('input[name="__RequestVerificationToken"]').first().val(); return t || ''; }
  function qnc(q){ var o = q||{}; o._ts = Date.now(); return o; }
  function ok(res){ return (res && (res.Success === true || res.success === true)) ? (res.Data ?? res.data ?? res) : res; }

  // Map RESTful v1 paths to legacy MVC action names
  function toLegacy(path){
    var map = {
      '/bootstrap':                 '/Bootstrap',
      '/patient/lookup-or-create':  '/PatientLookup',
      '/draft/create':              '/CreateDraft',
      '/services/by-department':    '/GetServicesForDepartment',
      '/item/add':                  '/AddItem',
      '/item/remove':               '/RemoveItem',
      '/insurance/plans':           '/GetInsurancePlans',
      '/insurances/set':            '/SetInsurances',
      '/finalize/pos':              '/FinalizeWithPos',
      '/finalize/cash':             '/FinalizeWithCash'
    };
    return map[path] || path;
  }

  function ajaxWithFallback(method, path, data){
    var first = {
      url: baseV1 + path + (method === 'GET' ? ('?' + $.param(qnc(data||{}))) : ('?_ts=' + Date.now())),
      type: method,
      data: method === 'GET' ? undefined : JSON.stringify(data||{}),
      contentType: method === 'GET' ? undefined : 'application/json; charset=utf-8',
      cache: false,
      headers: { 'RequestVerificationToken': anti() }
    };
    var second = {
      url: legacy + toLegacy(path),
      type: method === 'GET' ? 'GET' : 'POST',
      data: method === 'GET' ? (data||{}) : (data||{}),
      cache: false,
      headers: { 'RequestVerificationToken': anti() }
    };

    return $.ajax(first)
      .then(function(d){ memo(baseV1); return d; })
      .catch(function(){ return $.ajax(second).then(function(d){ memo(legacy); return d; }); });
  }

  function get(path, query){
    return ajaxWithFallback('GET', path, query);
  }

  function post(path, body){
    return ajaxWithFallback('POST', path, body);
  }

  win.ReceptionAPI = { get:get, post:post, ok:ok, _base:function(){return base;} };
})(jQuery, window);
