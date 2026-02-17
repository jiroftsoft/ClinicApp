(function(w){
  /**
   * نرمال‌سازی شماره موبایل برای نمایش در فرم (همیشه 09xxxxxxxxx)
   * ورودی می‌تواند +989137699527 یا 00989137699527 یا 09137699527 باشد.
   */
  function normalizeMobileForDisplay(val) {
    if (val == null || val === '') return '';
    var s = String(val).trim().replace(/[\s\-]/g, '');
    if (!s) return '';
    if (s.startsWith('+989')) return '0' + s.slice(4);
    if (s.startsWith('00989')) return '0' + s.slice(4);
    if (s.startsWith('989')) return '0' + s.slice(3);
    if (s.startsWith('98') && s.length >= 11) return '0' + s.slice(2);
    return s;
  }

  w.RxUtils = {
    toIRR: n => (n||0).toLocaleString("fa-IR"),
    parseFaInt: s => parseInt(String(s||"").replace(/[^\d]/g,""),10)||0,
    guid: () => (crypto && crypto.randomUUID) ? crypto.randomUUID() : (Date.now()+"-xxxx").replace(/x/g,()=>Math.floor(Math.random()*10)),
    normalizeMobileForDisplay: normalizeMobileForDisplay,
  };
})(window);
