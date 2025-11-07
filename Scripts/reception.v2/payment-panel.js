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
  
  /**
   * ✅ بررسی و به‌روزرسانی Totals قبل از Finalize
   * این تابع اطمینان می‌دهد که pricing به‌روز است قبل از نهایی‌سازی
   */
  function ensurePricingUpToDate(receptionId) {
    return new Promise(function(resolve, reject) {
      // بررسی اینکه آیا ReceptionId معتبر است
      if (!receptionId || receptionId <= 0) {
        resolve(false);
        return;
      }
      
      // بررسی اینکه آیا آیتم‌هایی در پذیرش وجود دارد
      const $items = $('[data-reception-item-id]');
      if (!$items.length) {
        console.warn('🏥 V2: No items found in reception');
        resolve(false);
        return;
      }
      
      // ✅ Trigger Reprice برای اطمینان از به‌روز بودن pricing
      console.log('🏥 V2: Ensuring pricing is up-to-date before finalize...');
      
      API.post('/pricing/reprice-all', { receptionId: receptionId })
        .then(function(response) {
          const success = response?.Success ?? response?.success;
          const isSuccess = success === true || success === "true" || success === 1;
          
          if (isSuccess && response.Data) {
            const data = API.ok(response);
            const totals = data.totals || data.Totals || data;
            
            // ✅ به‌روزرسانی UI با totals جدید
            if (window.insurancePanelModule && typeof window.insurancePanelModule.updateTotalsUI === 'function') {
              window.insurancePanelModule.updateTotalsUI(totals);
            } else if (window.updateTotalsUI) {
              window.updateTotalsUI(totals);
            }
            
            console.log('✅ V2: Pricing refreshed before finalize - PatientPayable:', totals?.PatientPayableIRR || totals?.patientPayableIRR || totals?.Patient || totals?.patient);
            resolve(true);
          } else {
            console.warn('🏥 V2: Reprice response indicates failure, but continuing...');
            resolve(false); // Continue anyway
          }
        })
        .catch(function(err) {
          console.warn('🏥 V2: Error refreshing pricing before finalize:', err);
          resolve(false); // Continue anyway - don't block finalize
        });
    });
  }
  
  function proceedWithFinalize() {
    const receptionId = $("#ReceptionId").val();
    
    // ✅ گام 1: بررسی ReceptionId
    if (!receptionId || receptionId <= 0) {
      toastr.error('شناسه پذیرش نامعتبر است');
      return;
    }
    
    // ✅ گام 2: بررسی وجود آیتم‌ها
    const $items = $('[data-reception-item-id]');
    if (!$items.length) {
      toastr.warning('هیچ خدمتی به پذیرش افزوده نشده است. لطفاً ابتدا خدمت اضافه کنید.');
      return;
    }
    
    // ✅ گام 3: اطمینان از به‌روز بودن pricing (با async wait)
    ensurePricingUpToDate(receptionId).then(function() {
      // ✅ گام 4: خواندن مبلغ قابل پرداخت (بعد از refresh) - چند منبع برای اطمینان
      let amountIRR = U.parseFaInt($("#PatientPayable").attr("data-value")) || 0;
      
      // ✅ Fallback: اگر data-value موجود نبود، از متن element بخوان
      if (amountIRR <= 0) {
        const patientText = $("#PatientPayable").text().trim();
        amountIRR = U.parseFaInt(patientText) || 0;
      }
      
      // ✅ Fallback دوم: از SumPatient بخوان
      if (amountIRR <= 0) {
        amountIRR = U.parseFaInt($("#SumPatient").attr("data-value")) || 
                    U.parseFaInt($("#SumPatient").text()) || 0;
      }
      
      console.log('🏥 V2: PatientPayable read from UI:', {
        'PatientPayable.data-value': $("#PatientPayable").attr("data-value"),
        'PatientPayable.text': $("#PatientPayable").text(),
        'SumPatient.data-value': $("#SumPatient").attr("data-value"),
        'SumPatient.text': $("#SumPatient").text(),
        'Parsed amountIRR': amountIRR
      });
      
      // ✅ گام 5: اعتبارسنجی مبلغ قابل پرداخت
      if (amountIRR <= 0) {
        // ✅ تشخیص علت: بررسی اینکه آیا بیمه 100% پوشش می‌دهد
        const gross = U.parseFaInt($("#Gross").attr("data-value")) || 
                      U.parseFaInt($("#SumGross").attr("data-value")) || 0;
        const baseCovered = U.parseFaInt($("#InsurancePayable").attr("data-value")) || 
                            U.parseFaInt($("#SumBase").attr("data-value")) || 0;
        const suppCovered = U.parseFaInt($("#SuppPayable").attr("data-value")) || 
                           U.parseFaInt($("#SumSupp").attr("data-value")) || 0;
        
        console.log('🏥 V2: Totals check - Gross:', gross, 'Base:', baseCovered, 'Supp:', suppCovered, 'Patient:', amountIRR);
        
        if (gross > 0 && (baseCovered + suppCovered) >= gross) {
          toastr.info('مبلغ قابل پرداخت صفر است زیرا بیمه 100% هزینه را پوشش می‌دهد. می‌توانید پذیرش را نهایی کنید.', 'اطلاع', {
            timeOut: 8000,
            positionClass: 'toast-top-center',
            closeButton: true
          });
          // ✅ در این حالت، اجازه نهایی‌سازی با مبلغ صفر را بدهیم
          // اما backend باید این را validate کند
        } else if (gross <= 0) {
          toastr.error('مبلغ کل پذیرش صفر است. لطفاً خدمت‌های معتبر اضافه کنید.', 'خطا', {
            timeOut: 6000
          });
          return;
        } else {
          // ✅ پیام دقیق‌تر با جزئیات
          let errorMsg = 'مبلغ قابل پرداخت باید بیشتر از صفر باشد.';
          if (gross > 0 && baseCovered === 0 && suppCovered === 0) {
            errorMsg += ' لطفاً بیمه‌ها را انتخاب کنید.';
          } else if (gross > 0 && (baseCovered + suppCovered) < gross) {
            errorMsg += ` مبلغ کل: ${U.toIRR(gross)}، پوشش بیمه: ${U.toIRR(baseCovered + suppCovered)}، سهم بیمار: ${U.toIRR(gross - baseCovered - suppCovered)}`;
          }
          
          toastr.warning(errorMsg, 'هشدار', {
            timeOut: 7000,
            positionClass: 'toast-top-center',
            closeButton: true
          });
          return;
        }
      }
      
      // ✅ گام 6: آماده‌سازی payload
      const isPOS = $("#PayPOS").hasClass('active');
      const payload = {
        receptionId: receptionId,
        amountIRR: amountIRR,
        idempotencyKey: U.guid()
      };
      
      if(isPOS) {
        // ✅ اعتبارسنجی فیلدهای POS
        const rrn = $("#RRN").val();
        const traceNo = $("#TraceNo").val();
        const terminalId = $("#TerminalId").val();
        const cardLast4 = $("#CardLast4").val();
        
        if (!rrn || !traceNo || !terminalId) {
          toastr.warning('لطفاً اطلاعات تراکنش POS را کامل کنید (RRN، TraceNo، TerminalId)');
          return;
        }
        
        payload.pos = { 
          rrn: rrn, 
          traceNo: traceNo, 
          terminalId: terminalId, 
          cardLast4: cardLast4 || null
        };
      } else {
        payload.cash = {
          cashSessionId: $("#CashSessionId").val() || null
        };
      }
      
      // ✅ گام 7: ارسال درخواست نهایی‌سازی
      const endpoint = isPOS ? "/finalize/pos" : "/finalize/cash";
      
      console.log('🏥 V2: Finalizing reception:', payload);
      
      API.post(endpoint, payload)
        .then(API.ok)
        .then(function(d){ 
          console.log('🏥 V2: Reception finalized:', d);
          toastr.success("پذیرش با موفقیت نهایی شد", 'موفق', {
            timeOut: 5000
          });
          
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
          
          // ✅ کمی تاخیر قبل از reload برای نمایش پیام موفقیت
          setTimeout(function() {
            location.reload();
          }, 1500);
        })
        .catch(function(err) {
          console.error('🏥 V2: Finalize error:', err);
          
          // ✅ نمایش پیام خطای دقیق‌تر
          const errorMsg = err?.responseJSON?.Message || 
                          err?.responseJSON?.message || 
                          err?.message || 
                          'خطا در نهایی‌سازی پذیرش';
          
          toastr.error(errorMsg, 'خطا', {
            timeOut: 7000,
            positionClass: 'toast-top-center',
            closeButton: true
          });
        });
    });
  }
})(window.ReceptionAPI, window.RxUtils);
