(function($){
  let dirty=false;
  $(document).on("input change", ".reception-pro :input", ()=> dirty=true);
  
  // 🏥 MEDICAL: حذف Draft ناقص هنگام خروج از فرم
  window.addEventListener("beforeunload", function(e) {
    // بررسی وجود Draft ناقص
    if (window.AutoDraftManager && window.AutoDraftManager.isDraftCreated()) {
      const receptionId = window.AutoDraftManager.getCurrentDraftId();
      const receptionIdFromDOM = $("#ReceptionId").val();
      const draftId = receptionId || receptionIdFromDOM;
      
      if (draftId && draftId > 0) {
        // بررسی اینکه آیا Draft دارای خدمت است یا نه
        // اگر Draft ناقص است (بدون خدمت)، آن را حذف کن
        // استفاده از sendBeacon برای اطمینان از ارسال درخواست قبل از بسته شدن صفحه
        const hasItems = $('.reception-item-row').length > 0 || 
                        $('.service-item').length > 0 ||
                        $('#ReceptionItemsList tbody tr').length > 0;
        
        if (!hasItems) {
          console.log('🏥 V2: Deleting incomplete draft on page unload:', draftId);
          
          // استفاده از sendBeacon برای ارسال درخواست قبل از بسته شدن صفحه
          const payload = JSON.stringify({ receptionId: draftId });
          const blob = new Blob([payload], { type: 'application/json' });
          
          // ارسال درخواست با sendBeacon (مطمئن‌تر از fetch در beforeunload)
          if (navigator.sendBeacon) {
            navigator.sendBeacon('/api/v1/reception/draft/delete-incomplete', blob);
            console.log('✅ V2: Incomplete draft deletion sent via sendBeacon');
          } else {
            // Fallback: استفاده از synchronous XMLHttpRequest (فقط در beforeunload مجاز است)
            const xhr = new XMLHttpRequest();
            xhr.open('POST', '/api/v1/reception/draft/delete-incomplete', false); // synchronous
            xhr.setRequestHeader('Content-Type', 'application/json');
            xhr.setRequestHeader('RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val() || '');
            xhr.send(payload);
          }
        }
      }
    }
    
    // نمایش هشدار برای فرم dirty
    if(dirty){ 
      e.preventDefault(); 
      e.returnValue=""; 
    }
  });
  
  // 🏥 MEDICAL: حذف Draft ناقص هنگام تغییر صفحه (navigation)
  $(window).on('pagehide', function() {
    if (window.AutoDraftManager && window.AutoDraftManager.isDraftCreated()) {
      const receptionId = window.AutoDraftManager.getCurrentDraftId();
      const receptionIdFromDOM = $("#ReceptionId").val();
      const draftId = receptionId || receptionIdFromDOM;
      
      if (draftId && draftId > 0) {
        const hasItems = $('.reception-item-row').length > 0 || 
                        $('.service-item').length > 0 ||
                        $('#ReceptionItemsList tbody tr').length > 0;
        
        if (!hasItems) {
          console.log('🏥 V2: Deleting incomplete draft on page hide:', draftId);
          // استفاده از sendBeacon
          const payload = JSON.stringify({ receptionId: draftId });
          const blob = new Blob([payload], { type: 'application/json' });
          
          if (navigator.sendBeacon) {
            navigator.sendBeacon('/api/v1/reception/draft/delete-incomplete', blob);
          }
        }
      }
    }
  });
  
  window.FormDirty = { clean:()=>dirty=false };
})(jQuery);
