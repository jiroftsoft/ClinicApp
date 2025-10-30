(function($){
  let dirty=false;
  $(document).on("input change", ".reception-pro :input", ()=> dirty=true);
  window.addEventListener("beforeunload", e=>{ if(dirty){ e.preventDefault(); e.returnValue=""; } });
  window.FormDirty = { clean:()=>dirty=false };
})(jQuery);
