(function($, win){
  var KEY = 'rx_api_base_v2';
  var bases = ['/api/v1/reception', '/Api/ReceptionApi'];
  var cached = null;
  try { cached = win.sessionStorage.getItem(KEY); } catch(e){}
  var base = cached || bases[0];

  function memo(okBase){
    if(okBase !== base){ base = okBase; try{ win.sessionStorage.setItem(KEY, okBase); }catch(e){} }
  }

  function anti(){ var t = $('input[name="__RequestVerificationToken"]').first().val(); return t || ''; }
  function qnc(q){ var o = q||{}; o._ts = Date.now(); return o; }
  function ok(res){ return (res && (res.Success === true || res.success === true)) ? (res.Data ?? res.data ?? res) : res; }

  function ajaxWithFallback(opts){
    var first = $.extend(true, {}, opts, { url: bases[0] + opts.url });
    var second = $.extend(true, {}, opts, { url: bases[1] + opts.url });
    return $.ajax(first)
      .then(function(d){ memo(bases[0]); return d; })
      .catch(function(err){ if(err && err.status === 404){ return $.ajax(second).then(function(d){ memo(bases[1]); return d; }); } throw err; });
  }

  function get(path, query){
    return ajaxWithFallback({ url: path + '?' + $.param(qnc(query||{})), type:'GET', cache:false, headers:{ 'RequestVerificationToken': anti() } });
  }

  function post(path, body){
    return ajaxWithFallback({ url: path + '?_ts=' + Date.now(), type:'POST', data: JSON.stringify(body||{}), contentType:'application/json; charset=utf-8', cache:false, headers:{ 'RequestVerificationToken': anti() } });
  }

  win.ReceptionAPI = { get:get, post:post, ok:ok, _base:function(){return base;} };
})(jQuery, window);
