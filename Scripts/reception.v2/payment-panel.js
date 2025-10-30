(function(API, U){
  // Payment method toggle
  $("#PayPOS, #PayCash").on('click', function() {
    $("#PayPOS, #PayCash").removeClass('active btn-primary').addClass('btn-outline-secondary');
    $(this).removeClass('btn-outline-secondary').addClass('active btn-primary');
  });

  $("#BtnFinalizePOS").on("click", function(){
    const receptionId = $("#ReceptionId").val();
    const amountIRR = U.parseFaInt($("#PatientPayable").attr("data-value"));
    
    if(!receptionId || receptionId <= 0) {
      // Try to create auto-draft first
      if (window.AutoDraftManager && !window.AutoDraftManager.isDraftCreated()) {
        window.AutoDraftManager.createDraft().then(draftId => {
          if (draftId) {
            $("#ReceptionId").val(draftId);
            proceedWithFinalize();
          } else {
            toastr.warning('لطفاً ابتدا پذیرش را ایجاد کنید');
          }
        }).catch(err => {
          console.error('🏥 V2: Auto-draft creation error:', err);
          toastr.warning('لطفاً ابتدا پذیرش را ایجاد کنید');
        });
        return;
      } else {
        toastr.warning('لطفاً ابتدا پذیرش را ایجاد کنید');
        return;
      }
    }
    
    proceedWithFinalize();
  });
  
  function proceedWithFinalize() {
    const receptionId = $("#ReceptionId").val();
    const amountIRR = U.parseFaInt($("#PatientPayable").attr("data-value"));
    
    if(amountIRR <= 0) {
      toastr.warning('مبلغ قابل پرداخت باید بیشتر از صفر باشد');
      return;
    }
    
    const isPOS = $("#PayPOS").hasClass('active');
    const payload = {
      receptionId: receptionId,
      amountIRR: amountIRR,
      idempotencyKey: U.guid()
    };
    
    if(isPOS) {
      payload.pos = { 
        rrn: $("#RRN").val(), 
        traceNo: $("#TraceNo").val(), 
        terminalId: $("#TerminalId").val(), 
        cardLast4: $("#CardLast4").val() 
      };
    } else {
      payload.cash = {
        cashSessionId: $("#CashSessionId").val() || null
      };
    }
    
    const endpoint = isPOS ? "/Api/ReceptionApi/FinalizeWithPos" : "/Api/ReceptionApi/FinalizeWithCash";
    
    API.post(endpoint, payload)
      .then(API.ok)
      .then(d=>{ 
        console.log('🏥 V2: Reception finalized:', d);
        toastr.success("پذیرش نهایی شد");
        
        if(d.receipt && d.receipt.printedUrl) {
          window.open(d.receipt.printedUrl, '_blank');
        }
        
        // Reset form and auto-draft system
        if (window.FormDirty) {
          window.FormDirty.clean();
        }
        if (window.AutoDraftManager) {
          window.AutoDraftManager.reset();
        }
        location.reload();
      })
      .catch(err => {
        console.error('🏥 V2: Finalize error:', err);
        toastr.error('خطا در نهایی‌سازی پذیرش');
      });
  }
})(window.ReceptionAPI, window.RxUtils);
