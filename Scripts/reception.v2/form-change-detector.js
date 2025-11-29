(function($){
  let dirty=false;
  $(document).on("input change", ".reception-pro :input", ()=> dirty=true);
  
  // 🏥 MEDICAL: حذف Draft ناقص هنگام خروج از فرم - بهبود یافته
  window.addEventListener("beforeunload", function(e) {
    // بررسی وجود Draft ناقص
    if (window.AutoDraftManager && window.AutoDraftManager.isDraftCreated()) {
      const receptionId = window.AutoDraftManager.getCurrentDraftId();
      const receptionIdFromDOM = $("#ReceptionId").val();
      const draftId = receptionId || receptionIdFromDOM;
      
      if (draftId && draftId > 0) {
        // ✅ استفاده از تابع جدید برای بررسی Draft نهایی نشده
        // Draft باید حذف شود اگر هنوز نهایی نشده باشد (حتی اگر خدمت داشته باشد)
        if (window.AutoDraftManager.isDraftNotFinalized && window.AutoDraftManager.isDraftNotFinalized()) {
          console.log('🏥 V2: Deleting non-finalized draft on page unload:', draftId);
          
          // ✅ استفاده از متد بهبود یافته با sendBeacon
          if (window.AutoDraftManager.deleteIncompleteDraftWithBeacon) {
            window.AutoDraftManager.deleteIncompleteDraftWithBeacon(draftId);
          } else {
            // Fallback: استفاده از sendBeacon مستقیم
            const url = '/api/v1/reception/draft/delete-incomplete?receptionId=' + draftId;
            if (navigator.sendBeacon) {
              navigator.sendBeacon(url);
              console.log('✅ V2: Incomplete draft deletion sent via sendBeacon (fallback)');
            }
          }
        } else {
          console.log('🏥 V2: Draft has items, not deleting:', draftId);
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
        // ✅ استفاده از تابع جدید برای بررسی Draft نهایی نشده
        if (window.AutoDraftManager.isDraftNotFinalized && window.AutoDraftManager.isDraftNotFinalized()) {
          console.log('🏥 V2: Deleting non-finalized draft on page hide:', draftId);
          
          // ✅ استفاده از متد بهبود یافته با sendBeacon
          if (window.AutoDraftManager.deleteIncompleteDraftWithBeacon) {
            window.AutoDraftManager.deleteIncompleteDraftWithBeacon(draftId);
          } else {
            // Fallback
            const url = '/api/v1/reception/draft/delete-incomplete?receptionId=' + draftId;
            if (navigator.sendBeacon) {
              navigator.sendBeacon(url);
            }
          }
        }
      }
    }
  });

  // 🏥 MEDICAL: حذف Draft ناقص هنگام تغییر Tab (visibilitychange)
  document.addEventListener('visibilitychange', function() {
    // اگر صفحه hidden شد و Draft ناقص وجود دارد، حذف کن
    if (document.hidden && window.AutoDraftManager && window.AutoDraftManager.isDraftCreated()) {
      const receptionId = window.AutoDraftManager.getCurrentDraftId();
      const receptionIdFromDOM = $("#ReceptionId").val();
      const draftId = receptionId || receptionIdFromDOM;
      
      if (draftId && draftId > 0) {
        // فقط اگر Draft نهایی نشده است، حذف کن (نه فوری، با کمی تاخیر)
        setTimeout(function() {
          if (document.hidden && window.AutoDraftManager && window.AutoDraftManager.isDraftNotFinalized && window.AutoDraftManager.isDraftNotFinalized()) {
            console.log('🏥 V2: Deleting non-finalized draft on visibility change (hidden):', draftId);
            if (window.AutoDraftManager.deleteIncompleteDraft) {
              window.AutoDraftManager.deleteIncompleteDraft(draftId).catch(function(err) {
                console.warn('⚠️ V2: Error deleting draft on visibility change:', err);
              });
            }
          }
        }, 5000); // 5 ثانیه تاخیر - اگر کاربر برگشت، حذف نمی‌شود
      }
    }
  });
  
  window.FormDirty = { clean:()=>dirty=false };
})(jQuery);
