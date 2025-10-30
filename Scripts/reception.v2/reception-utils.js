(function(w){
  w.RxUtils = {
    toIRR: n => (n||0).toLocaleString("fa-IR"),
    parseFaInt: s => parseInt(String(s||"").replace(/[^\d]/g,""),10)||0,
    guid: () => (crypto && crypto.randomUUID) ? crypto.randomUUID() : (Date.now()+"-xxxx").replace(/x/g,()=>Math.floor(Math.random()*10)),
  };
})(window);
