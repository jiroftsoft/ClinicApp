(function(w,$){
  function anti(){ return $('input[name="__RequestVerificationToken"]').val() || ""; }
  function ok(res){ if(!res || res.success !== true){ toastr.error(res?.message||"خطا"); throw new Error(res?.message||"fail"); } return res.data; }
  function get(url, params){ params = params || {}; params._ts = Date.now(); return $.ajax({ url, method:"GET", data: params, cache:false }); }
  function post(url, data){ return $.ajax({ url, method:"POST", headers:{ "RequestVerificationToken": anti() }, contentType:"application/json; charset=utf-8", data: JSON.stringify(data||{}) }); }
  w.ReceptionAPI = { get, post, ok };
})(window, jQuery);
