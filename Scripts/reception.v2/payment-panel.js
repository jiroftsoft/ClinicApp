(function(API, U){
  'use strict';
  
  function getReceptionItemRows() {
    let $rows = $('[data-reception-item-id]');
    console.log('🏥 V2: getReceptionItemRows - rows with data-reception-item-id:', $rows.length);
    if ($rows.length) {
      console.log('🏥 V2: getReceptionItemRows - returning rows with data-reception-item-id');
      return $rows;
    }

    // fallback: rows rendered without reception-item id (e.g., draft rows)
    $rows = $('#items-grid tbody tr[data-service-id]');
    console.log('🏥 V2: getReceptionItemRows - rows with data-service-id inside tbody:', $rows.length);
    if ($rows.length) {
      console.log('🏥 V2: getReceptionItemRows - returning rows with data-service-id inside tbody');
      return $rows;
    }

    // additional fallback: any row inside items-grid tbody
    $rows = $('#items-grid tbody tr');
    console.log('🏥 V2: getReceptionItemRows - rows inside items-grid tbody:', $rows.length);
    if ($rows.length) {
      console.log('🏥 V2: getReceptionItemRows - returning rows inside items-grid tbody');
      return $rows;
    }

    // ultimate fallback: any row with data-service-id anywhere in DOM
    $rows = $('tr[data-service-id]');
    console.log('🏥 V2: getReceptionItemRows - rows with data-service-id (global):', $rows.length);
    console.log('🏥 V2: getReceptionItemRows - final fallback returning rows with data-service-id (global)');
    return $rows;
  }

  // ✅ اطمینان از لود شدن DOM قبل از attach کردن event handlers
  $(document).ready(function() {
    console.log('🏥 V2: Payment Panel - DOM Ready, attaching event handlers...');
    
    // Payment method toggle
    $("#PayPOS, #PayCash").on('click', function() {
      $("#PayPOS, #PayCash").removeClass('active btn-primary').addClass('btn-outline-secondary');
      $(this).removeClass('btn-outline-secondary').addClass('active btn-primary');
    });

    /**
     * ✅ دکمه "ذخیره پذیرش"
     * پس از ذخیره، مودال پرداخت باز می‌شود
     */
    $("#BtnSaveReception").on("click", function(e){
      e.preventDefault();
      e.stopPropagation();
      
      console.log('🏥 V2: BtnSaveReception clicked');
      
      const receptionId = $("#ReceptionId").val();
      
      if(!receptionId || receptionId <= 0) {
        // Try to create auto-draft first
        if (window.AutoDraftManager && typeof window.AutoDraftManager.createDraft === 'function') {
          window.AutoDraftManager.createDraft().then(draftId => {
            if (draftId) {
              $("#ReceptionId").val(draftId);
              saveReceptionAndOpenPaymentModal(draftId);
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
      
      saveReceptionAndOpenPaymentModal(receptionId);
    });
    
    // ✅ بررسی وجود دکمه در DOM
    if ($("#BtnSaveReception").length === 0) {
      console.error('🏥 V2: ❌ BtnSaveReception not found in DOM!');
    } else {
      console.log('🏥 V2: ✅ BtnSaveReception found:', $("#BtnSaveReception")[0]);
    }
  });
  
  // ✅ Event Delegation برای اطمینان از کار کردن حتی اگر دکمه بعداً اضافه شود
  $(document).on('click', '#BtnSaveReception', function(e) {
    e.preventDefault();
    e.stopPropagation();
    
    console.log('🏥 V2: BtnSaveReception clicked (via delegation)');
    
    // ✅ علامت‌گذاری Draft به عنوان در حال نهایی شدن
    if (window.AutoDraftManager && window.AutoDraftManager.markDraftAsFinalizing) {
      window.AutoDraftManager.markDraftAsFinalizing();
    }
    
    const receptionId = $("#ReceptionId").val();
    
    if(!receptionId || receptionId <= 0) {
      // Try to create auto-draft first
      if (window.AutoDraftManager && typeof window.AutoDraftManager.createDraft === 'function') {
        window.AutoDraftManager.createDraft().then(draftId => {
          if (draftId) {
            $("#ReceptionId").val(draftId);
            saveReceptionAndOpenPaymentModal(draftId);
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
    
    saveReceptionAndOpenPaymentModal(receptionId);
  });
  
  /**
   * ✅ ذخیره پذیرش و باز کردن مودال پرداخت
   */
  function saveReceptionAndOpenPaymentModal(receptionId) {
    console.log('🏥 V2: Saving reception and opening payment modal:', receptionId);
    
    // ✅ بررسی وجود آیتم‌ها
    let $items = getReceptionItemRows();
    const countAllReception = $('[data-reception-item-id]').length;
    const countTbodyService = $('#items-grid tbody tr[data-service-id]').length;
    const countTbody = $('#items-grid tbody tr').length;
    const countGlobalService = $('tr[data-service-id]').length;
    console.log('🏥 V2: saveReceptionAndOpenPaymentModal - rows detected:', $items.length, {
      countAllReception,
      countTbodyService,
      countTbody,
      countGlobalService
    });
    if (!$items.length) {
      console.warn('🏥 V2: saveReceptionAndOpenPaymentModal - no item rows detected in DOM');
      toastr.warning('هیچ خدمتی به پذیرش افزوده نشده است. لطفاً ابتدا خدمت اضافه کنید.');
      return;
    }
    
    // ✅ ذخیره پذیرش (Update Draft)
    const formData = {
      receptionId: receptionId,
      patientId: $("#Patient_PatientId").val(),
      nationalCode: $("#Patient_NationalCode").val(),
      clinicId: $("#ClinicId").val(),
      departmentId: $("#DepartmentId").val(),
      doctorId: $("#DoctorId").val(),
      basePlanId: $("#BasePlanId").val() || null,
      supplementaryPlanId: $("#SuppPlanId").val() || null
    };
    
    API.post("/draft/update", formData)
      .then(function(response) {
        console.log('🏥 V2: Draft update response:', response);
        
        // ✅ بررسی موفقیت درخواست
        const success = response?.Success ?? response?.success;
        const isSuccess = success === true || success === "true" || success === 1;
        
        if (!isSuccess) {
          const errorMsg = response?.Message || response?.message || 'خطا در ذخیره پذیرش';
          throw new Error(errorMsg);
        }
        
        return API.ok(response);
      })
      .then(function() {
        console.log('🏥 V2: Reception saved successfully');
        toastr.success('پذیرش با موفقیت ذخیره شد', 'موفق', {
          timeOut: 3000
        });
        
        // ✅ خواندن مبلغ قابل پرداخت
        ensurePricingUpToDate(receptionId).then(function() {
          let amountIRR = U.parseFaInt($("#PatientPayable").attr("data-value")) || 0;
          
          if (amountIRR <= 0) {
            const patientText = $("#PatientPayable").text().trim();
            amountIRR = U.parseFaInt(patientText) || 0;
          }
          
          if (amountIRR <= 0) {
            amountIRR = U.parseFaInt($("#SumPatient").attr("data-value")) || 
                        U.parseFaInt($("#SumPatient").text()) || 0;
          }
          
          // ✅ بررسی نوع پرداخت
          const isPOS = $("#PayPOS").hasClass('active');
          
          if (isPOS && amountIRR > 0) {
            // ✅ باز کردن مودال پرداخت POS
            openPosPaymentModal(receptionId, amountIRR);
            
            // ✅ مخفی کردن دکمه "ذخیره پذیرش" و نمایش دکمه "پرداخت و نهایی‌سازی"
            $("#BtnSaveReception").addClass('d-none');
            $("#BtnFinalizePOS").removeClass('d-none');
          } else if (!isPOS) {
            // ✅ برای نقدی، مستقیماً Finalize انجام شود
            const payload = {
              receptionId: receptionId,
              amountIRR: amountIRR,
              idempotencyKey: U.guid(),
              cash: {
                cashSessionId: $("#CashSessionId").val() || null
              }
            };
            
            finalizeReception(payload, false);
          } else {
            // ✅ مبلغ صفر - بیمه 100% پوشش می‌دهد
            toastr.info('مبلغ قابل پرداخت صفر است زیرا بیمه 100% هزینه را پوشش می‌دهد. می‌توانید پذیرش را نهایی کنید.', 'اطلاع', {
              timeOut: 5000
            });
            
            const payload = {
              receptionId: receptionId,
              amountIRR: 0,
              idempotencyKey: U.guid(),
              cash: {
                cashSessionId: $("#CashSessionId").val() || null
              }
            };
            
            finalizeReception(payload, false);
          }
        });
      })
      .catch(function(err) {
        console.error('🏥 V2: Save reception error:', err);
        
        // ✅ بررسی ANTIFORGERY_MISSING
        if (err?.responseJSON && API.handleErrorJson && typeof API.handleErrorJson === 'function') {
          if (API.handleErrorJson(err.responseJSON)) {
            return; // خطا توسط handleErrorJson مدیریت شد
          }
        }
        
        const errorMsg = err?.responseJSON?.Message || 
                        err?.responseJSON?.message || 
                        err?.message || 
                        'خطا در ذخیره پذیرش';
        toastr.error(errorMsg, 'خطا', {
          timeOut: 7000,
          positionClass: 'toast-top-center',
          closeButton: true
        });
      });
  }

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
      const $items = getReceptionItemRows();
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
    const $items = getReceptionItemRows();
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
        // ✅ بررسی مبلغ قبل از باز کردن Modal
        if (amountIRR <= 0) {
          // اگر مبلغ صفر است اما بیمه 100% پوشش می‌دهد، اجازه بدهیم ادامه دهد
          const gross = U.parseFaInt($("#Gross").attr("data-value")) || 
                        U.parseFaInt($("#SumGross").attr("data-value")) || 0;
          const baseCovered = U.parseFaInt($("#InsurancePayable").attr("data-value")) || 
                              U.parseFaInt($("#SumBase").attr("data-value")) || 0;
          const suppCovered = U.parseFaInt($("#SuppPayable").attr("data-value")) || 
                             U.parseFaInt($("#SumSupp").attr("data-value")) || 0;
          
          if (gross > 0 && (baseCovered + suppCovered) >= gross) {
            // بیمه 100% پوشش می‌دهد - می‌توانیم بدون پرداخت نهایی کنیم
            payload.pos = null; // بدون پرداخت POS
            finalizeReception(payload, false); // به عنوان نقدی نهایی کن
            return;
          } else {
            toastr.warning('مبلغ قابل پرداخت صفر است. لطفاً ابتدا خدمت اضافه کنید یا بیمه را تنظیم کنید.');
            return;
          }
        }
        
        // ✅ باز کردن Modal پرداخت POS
        console.log('🏥 V2: Opening POS Payment Modal with amount:', amountIRR);
        openPosPaymentModal(receptionId, amountIRR, function(posData) {
          // ✅ پس از دریافت اطلاعات تراکنش از دستگاه، Finalize را انجام بده
          payload.pos = {
            rrn: posData.rrn,
            traceNo: posData.traceNo,
            terminalId: posData.terminalId,
            cardLast4: posData.cardLast4 || null
          };
          
          // ✅ ادامه با Finalize
          finalizeReception(payload, isPOS);
        });
        return; // منتظر بمان تا Modal اطلاعات را برگرداند
      } else {
        payload.cash = {
          cashSessionId: $("#CashSessionId").val() || null
        };
      }
      
      // ✅ گام 7: ارسال درخواست نهایی‌سازی
      finalizeReception(payload, isPOS);
    });
  }
  
  /**
   * ✅ نهایی‌سازی پس از پرداخت موفق
   */
  function finalizeAfterPayment(receptionId, amountIRR, posData) {
    console.log('🏥 V2: Finalizing after successful payment:', { receptionId, amountIRR, posData });
    
    const payload = {
      receptionId: receptionId,
      amountIRR: amountIRR,
      idempotencyKey: U.guid(),
      pos: {
        rrn: posData.rrn,
        traceNo: posData.traceNo,
        terminalId: posData.terminalId,
        cardLast4: posData.cardLast4 || null
      }
    };
    
    // ✅ بستن Modal قبل از Finalize
    $('#posPaymentModal').modal('hide');
    
    finalizeReception(payload, true);
  }
  
  /**
   * ✅ نهایی‌سازی پذیرش
   */
  function finalizeReception(payload, isPOS) {
    // ✅ علامت‌گذاری Draft به عنوان در حال نهایی شدن
    if (window.AutoDraftManager && window.AutoDraftManager.markDraftAsFinalizing) {
      window.AutoDraftManager.markDraftAsFinalizing();
    }
    
    const endpoint = isPOS ? "/finalize/pos" : "/finalize/cash";
    
    console.log('🏥 V2: Finalizing reception:', payload);
    
    API.post(endpoint, payload)
      .then(API.ok)
      .then(function(d){ 
        console.log('🏥 V2: Reception finalized:', d);
        toastr.success("پذیرش با موفقیت نهایی شد", 'موفق', {
          timeOut: 5000
        });
        
        // ✅ نمایش گزینه چاپ
        if(d.receipt && d.receipt.printedUrl) {
          setTimeout(function() {
            if (confirm('آیا می‌خواهید قبض پرداخت را چاپ کنید؟')) {
              window.open(d.receipt.printedUrl, '_blank');
            }
            
            // ✅ نمایش گزینه چاپ قبض بیمه تکمیلی (اگر وجود دارد)
            if (confirm('آیا می‌خواهید قبض بیمه تکمیلی را چاپ کنید؟')) {
              // TODO: اضافه کردن URL چاپ قبض بیمه تکمیلی
              // window.open(`/ReceptionV2/PrintInsurance/${payload.receptionId}`, '_blank');
            }
          }, 1000);
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
        }, 2000);
      })
      .catch(function(err) {
        console.error('🏥 V2: Finalize error:', err);
        
        // ✅ در صورت خطا، flag را بردار (Draft هنوز نهایی نشده)
        if (window.AutoDraftManager && window.AutoDraftManager.unmarkDraftAsFinalizing) {
          window.AutoDraftManager.unmarkDraftAsFinalizing();
        }
        
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
  }
  
  /**
   * ✅ باز کردن Modal پرداخت POS و ارتباط با دستگاه کارتخوان
   */
  function openPosPaymentModal(receptionId, amountIRR) {
    const $modal = $('#posPaymentModal');
    
    // ✅ Reset Modal state
    $('#posPaymentReady').removeClass('d-none');
    $('#posPaymentLoading').addClass('d-none');
    $('#posPaymentSuccess').addClass('d-none');
    $('#posPaymentError').addClass('d-none');
    $('#posPaymentStartBtn').removeClass('d-none');
    $('#posPaymentConfirmBtn').addClass('d-none');
    $('#posPaymentPrintBtn').addClass('d-none');
    $('#posPaymentCancelBtn').removeClass('d-none');
    
    // ✅ ذخیره ReceptionId و AmountIRR برای استفاده در callback
    $modal.data('receptionId', receptionId);
    $modal.data('amountIRR', amountIRR);
    
    // ✅ نمایش اطلاعات تراکنش
    console.log('🏥 V2: Opening POS Payment Modal - ReceptionId:', receptionId, 'AmountIRR:', amountIRR);
    if (amountIRR && amountIRR > 0) {
      $('#posAmount').text(U.toIRR(amountIRR) + ' ریال');
    } else {
      $('#posAmount').text('۰ ریال');
      console.warn('🏥 V2: AmountIRR is zero or invalid:', amountIRR);
    }
    
    // ✅ دریافت اطلاعات ترمینال پیش‌فرض
    $.ajax({
      url: '/api/v1/pos/terminals/default',
      method: 'GET',
      dataType: 'json',
      success: function(response) {
        console.log('🏥 V2: GetDefault Terminal Response:', response);
        if (response && response.Success && response.Data) {
          const terminal = response.Data;
          $('#posTerminalName').text(terminal.title || terminal.Title || 'دستگاه کارتخوان');
          $modal.data('terminal', terminal);
        } else {
          const errorMsg = response?.Message || response?.message || 'ترمینال POS پیش‌فرض یافت نشد. لطفاً ابتدا ترمینال را تنظیم کنید.';
          console.error('🏥 V2: Terminal not found:', errorMsg);
          showPosPaymentError(errorMsg);
        }
      },
      error: function(xhr, status, error) {
        console.error('🏥 V2: Error fetching terminal:', { xhr, status, error });
        const errorMsg = xhr?.responseJSON?.Message || 
                        xhr?.responseJSON?.message || 
                        'خطا در دریافت اطلاعات ترمینال';
        showPosPaymentError(errorMsg);
      }
    });
    
    // ✅ مدیریت دکمه "پرداخت با POS"
    $('#posPaymentStartBtn').off('click').on('click', function() {
      const terminal = $modal.data('terminal');
      if (!terminal) {
        toastr.error('اطلاعات ترمینال یافت نشد');
        return;
      }
      
      // ✅ شروع پردازش پرداخت
      $('#posPaymentReady').addClass('d-none');
      $('#posPaymentLoading').removeClass('d-none');
      $('#posPaymentStartBtn').addClass('d-none');
      
      processPosPayment(receptionId, amountIRR, terminal);
    });
    
    // ✅ نمایش Modal
    $modal.modal('show');
    
    // ✅ مدیریت بستن Modal
    $modal.off('hidden.bs.modal').on('hidden.bs.modal', function() {
      $('#posPaymentReady').removeClass('d-none');
      $('#posPaymentLoading').addClass('d-none');
      $('#posPaymentSuccess').addClass('d-none');
      $('#posPaymentError').addClass('d-none');
      $('#posPaymentStartBtn').removeClass('d-none');
      $('#posPaymentConfirmBtn').addClass('d-none');
      $('#posPaymentPrintBtn').addClass('d-none');
    });
  }
  
  /**
   * ✅ پردازش پرداخت POS از طریق دستگاه کارتخوان
   */
  function processPosPayment(receptionId, amountIRR, terminal) {
    $.ajax({
      url: '/api/v1/pos/process-payment',
      method: 'POST',
      contentType: 'application/json; charset=utf-8',
      data: JSON.stringify({
        ReceptionId: receptionId,
        AmountIRR: amountIRR,
        PosTerminalId: terminal.posTerminalId || terminal.PosTerminalId
      }),
      headers: {
        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
      },
      dataType: 'json',
      success: function(response) {
        if (response && response.Success && response.Data) {
          const posData = response.Data;
          
          // ✅ نمایش موفقیت
          $('#posPaymentLoading').addClass('d-none');
          $('#posPaymentSuccess').removeClass('d-none');
          $('#posPaymentConfirmBtn').removeClass('d-none');
          $('#posPaymentPrintBtn').removeClass('d-none');
          $('#posPaymentCancelBtn').addClass('d-none');
          
          // ✅ نمایش جزئیات تراکنش
          $('#posRRN').text(posData.rrn || '');
          $('#posTraceNo').text(posData.traceNo || '');
          $('#posTerminalId').text(posData.terminalId || terminal.terminalId || terminal.TerminalId || '');
          $('#posCardLast4').text(posData.cardLast4 || '');
          
          // ✅ ذخیره اطلاعات برای استفاده در Finalize
          window.posPaymentData = {
            rrn: posData.rrn,
            traceNo: posData.traceNo,
            terminalId: posData.terminalId || terminal.terminalId || terminal.TerminalId,
            cardLast4: posData.cardLast4
          };
          
          // ✅ مدیریت دکمه تأیید و نهایی‌سازی
          $('#posPaymentConfirmBtn').off('click').on('click', function() {
            finalizeAfterPayment(receptionId, amountIRR, window.posPaymentData);
          });
          
          // ✅ مدیریت دکمه چاپ
          $('#posPaymentPrintBtn').off('click').on('click', function() {
            // چاپ قبض پرداخت
            window.open(`/ReceptionV2/Print/${receptionId}`, '_blank');
          });
        } else {
          showPosPaymentError(response?.Message || response?.message || 'خطا در پردازش پرداخت');
        }
      },
      error: function(xhr) {
        const errorMsg = xhr?.responseJSON?.Message || 
                        xhr?.responseJSON?.message || 
                        'خطا در ارتباط با دستگاه کارتخوان';
        showPosPaymentError(errorMsg);
        
        // ✅ اگر پرداخت ناموفق بود، پذیرش به لیست پذیرش‌ها می‌رود (Status = Pending)
        // این کار در backend انجام می‌شود - فقط پیام نمایش می‌دهیم
        toastr.warning('پرداخت ناموفق بود. پذیرش در لیست پذیرش‌ها ذخیره شد و می‌توانید بعداً پرداخت کنید.', 'هشدار', {
          timeOut: 8000,
          positionClass: 'toast-top-center',
          closeButton: true
        });
        
        // ✅ هدایت به لیست پذیرش‌ها (اختیاری)
        setTimeout(function() {
          if (confirm('آیا می‌خواهید به لیست پذیرش‌ها بروید؟')) {
            window.location.href = '/Reception/Index';
          }
        }, 2000);
      }
    });
  }
  
  /**
   * ✅ نمایش خطا در Modal پرداخت POS
   */
  function showPosPaymentError(message) {
    const $modal = $('#posPaymentModal');
    $('#posPaymentReady').addClass('d-none');
    $('#posPaymentLoading').addClass('d-none');
    $('#posPaymentSuccess').addClass('d-none');
    $('#posPaymentError').removeClass('d-none');
    $('#posErrorMessage').text(message);
    $('#posPaymentStartBtn').addClass('d-none');
    $('#posPaymentConfirmBtn').addClass('d-none');
    $('#posPaymentPrintBtn').addClass('d-none');
    $('#posPaymentCancelBtn').removeClass('d-none');
  }
  
  /**
   * ✅ نهایی‌سازی پس از پرداخت موفق
   */
  function finalizeAfterPayment(receptionId, amountIRR, posData) {
    console.log('🏥 V2: Finalizing after successful payment:', { receptionId, amountIRR, posData });
    
    const payload = {
      receptionId: receptionId,
      amountIRR: amountIRR,
      idempotencyKey: U.guid(),
      pos: {
        rrn: posData.rrn,
        traceNo: posData.traceNo,
        terminalId: posData.terminalId,
        cardLast4: posData.cardLast4 || null
      }
    };
    
    // ✅ بستن Modal قبل از Finalize
    $('#posPaymentModal').modal('hide');
    
    finalizeReception(payload, true);
  }

  /**
   * 🏥 MEDICAL: پاک کردن فرم و آماده‌سازی برای پذیرش بیمار بعدی
   * این تابع Draft را حذف می‌کند و تمام فیلدهای فرم را پاک می‌کند
   */
  async function resetForm() {
    try {
      console.log('🏥 V2: ===== شروع پاک کردن فرم =====');
      
      // ✅ 1. حذف Draft (اگر وجود دارد) - بدون بررسی isDraftNotFinalized
      // چون کاربر صراحتاً روی دکمه "پاک کردن فرم" کلیک کرده است
      
      // ⚠️ مهم: باید ReceptionId را قبل از هر چیز دیگری بخوانیم
      // چون ممکن است در حین reset شدن، ReceptionId از DOM یا AutoDraftManager پاک شود
      
      let receptionId = 0;
      
      // ✅ اولویت 1: خواندن از AutoDraftManager (قبل از DOM)
      // چون AutoDraftManager منبع اصلی truth است
      if (window.AutoDraftManager) {
        try {
          console.log('🏥 V2: تلاش برای خواندن ReceptionId از AutoDraftManager...');
          console.log('🏥 V2: AutoDraftManager type:', typeof window.AutoDraftManager);
          console.log('🏥 V2: AutoDraftManager exists:', !!window.AutoDraftManager);
          
          // بررسی متدهای مختلف برای دریافت draft ID
          if (typeof window.AutoDraftManager.getCurrentDraftId === 'function') {
            const draftIdFromManager = window.AutoDraftManager.getCurrentDraftId();
            console.log('🏥 V2: getCurrentDraftId() result:', draftIdFromManager, 'Type:', typeof draftIdFromManager);
            if (draftIdFromManager != null && draftIdFromManager !== undefined && draftIdFromManager > 0) {
              receptionId = parseInt(draftIdFromManager, 10);
              console.log('✅ V2: ReceptionId از AutoDraftManager.getCurrentDraftId():', receptionId);
            } else {
              console.warn('⚠️ V2: getCurrentDraftId() مقدار نامعتبر برگرداند:', draftIdFromManager);
            }
          } else {
            console.warn('⚠️ V2: getCurrentDraftId function موجود نیست');
          }
          
          // ✅ اگر هنوز receptionId نداریم و isDraftCreated true است، از DOM بخوان
          if ((!receptionId || receptionId <= 0) && typeof window.AutoDraftManager.isDraftCreated === 'function') {
            const isCreated = window.AutoDraftManager.isDraftCreated();
            console.log('🏥 V2: isDraftCreated():', isCreated);
            if (isCreated) {
              // اگر Draft created است اما getCurrentDraftId null برگرداند، از DOM بخوان
              const receptionIdFromDOM = $("#ReceptionId").val();
              if (receptionIdFromDOM) {
                receptionId = parseInt(receptionIdFromDOM, 10);
                console.log('🏥 V2: Draft created است، ReceptionId از DOM:', receptionId);
              }
            }
          }
        } catch (err) {
          console.error('❌ V2: خطا در خواندن ReceptionId از AutoDraftManager:', err);
          console.error('❌ V2: Error stack:', err?.stack);
        }
      } else {
        console.warn('⚠️ V2: window.AutoDraftManager موجود نیست');
      }
      
      // ✅ اولویت 2: اگر از AutoDraftManager خوانده نشد، از DOM بخوان
      if (!receptionId || receptionId <= 0) {
        const receptionIdRaw = $("#ReceptionId").val();
        receptionId = receptionIdRaw ? parseInt(receptionIdRaw, 10) : 0;
        console.log('🏥 V2: ReceptionId از DOM - Raw:', receptionIdRaw, 'Parsed:', receptionId, 'Type:', typeof receptionId);
      }
      
      console.log('🏥 V2: ReceptionId نهایی برای حذف:', receptionId, 'Type:', typeof receptionId);
      
      if (receptionId && receptionId > 0) {
        console.log('🏥 V2: حذف Draft قبل از Reset فرم - ReceptionId:', receptionId);
        
        // بررسی اینکه آیا Draft در حال نهایی شدن است (اگر در حال نهایی شدن است، حذف نکن)
        // بررسی flag isDraftFinalizing از AutoDraftManager
        let isFinalizing = false;
        if (window.AutoDraftManager) {
          // بررسی flag داخلی (اگر در دسترس باشد)
          // یا بررسی از طریق بررسی وضعیت فعلی
          try {
            // اگر متد isDraftFinalizing وجود دارد، استفاده کن
            if (typeof window.AutoDraftManager.isDraftFinalizing === 'function') {
              isFinalizing = window.AutoDraftManager.isDraftFinalizing();
            }
          } catch (err) {
            console.warn('⚠️ V2: خطا در بررسی isDraftFinalizing:', err);
          }
        }
        
        if (isFinalizing) {
          console.log('⚠️ V2: Draft در حال نهایی شدن است، حذف نمی‌شود');
          toastr.warning('در حال نهایی‌سازی پذیرش است. لطفاً صبر کنید...', 'هشدار', {
            timeOut: 3000
          });
          return; // خروج از تابع
        }
        
        try {
          // حذف Draft با AJAX - بدون بررسی isDraftNotFinalized
          // چون کاربر صراحتاً روی دکمه "پاک کردن فرم" کلیک کرده است
          // Backend بررسی می‌کند که آیا Draft واقعاً Pending است یا نه
          console.log('🏥 V2: ===== شروع حذف Draft =====');
          console.log('🏥 V2: ReceptionId:', receptionId);
          console.log('🏥 V2: API object:', typeof API, API);
          console.log('🏥 V2: API.post:', typeof API?.post);
          
          // استفاده از API.post با error handling بهتر
          console.log('🏥 V2: فراخوانی API.post("/draft/delete-incomplete", { receptionId: ' + receptionId + ' })');
          console.log('🏥 V2: ReceptionId type:', typeof receptionId, 'Value:', receptionId);
          
          // ✅ اطمینان از اینکه receptionId یک عدد است
          const payload = { 
            receptionId: parseInt(receptionId, 10) || 0 
          };
          
          console.log('🏥 V2: Payload:', JSON.stringify(payload));
          
          const result = await API.post('/draft/delete-incomplete', payload)
            .catch(function(err) {
              console.error('❌ V2: ===== خطا در فراخوانی API =====');
              console.error('❌ V2: Error type:', typeof err);
              console.error('❌ V2: Error object:', err);
              console.error('❌ V2: Error message:', err?.message);
              console.error('❌ V2: Error stack:', err?.stack);
              console.error('❌ V2: Error responseJSON:', err?.responseJSON);
              console.error('❌ V2: Error status:', err?.status);
              console.error('❌ V2: Error statusText:', err?.statusText);
              console.error('❌ V2: Error responseText:', err?.responseText);
              console.error('❌ V2: ====================================');
              
              // Return یک object با Success = false برای ادامه پردازش
              return {
                Success: false,
                Message: err?.message || err?.responseJSON?.Message || err?.responseJSON?.message || 'خطا در ارتباط با سرور',
                Code: err?.responseJSON?.Code || err?.responseJSON?.code || 'API_ERROR',
                Error: err
              };
            });
          
          console.log('🏥 V2: ===== پاسخ API دریافت شد =====');
          console.log('🏥 V2: Result type:', typeof result);
          console.log('🏥 V2: Result:', result);
          console.log('🏥 V2: Result keys:', result ? Object.keys(result) : 'null/undefined');
          console.log('🏥 V2: Result.Success:', result?.Success);
          console.log('🏥 V2: Result.success:', result?.success);
          console.log('🏥 V2: Result.Message:', result?.Message);
          console.log('🏥 V2: Result.message:', result?.message);
          console.log('🏥 V2: Result.Code:', result?.Code);
          console.log('🏥 V2: Result.code:', result?.code);
          console.log('🏥 V2: Result.Data:', result?.Data);
          console.log('🏥 V2: Result.data:', result?.data);
          
          // بررسی Success از response اصلی (قبل از API.ok)
          const successValue = result?.Success ?? result?.success;
          const isSuccess = successValue === true || successValue === "true" || successValue === 1;
          
          console.log('🏥 V2: Success value:', successValue);
          console.log('🏥 V2: Is success:', isSuccess);
          
          if (isSuccess) {
            console.log('✅ V2: ===== Draft با موفقیت حذف شد =====');
            // Reset local state
            if (window.AutoDraftManager && window.AutoDraftManager.reset) {
              window.AutoDraftManager.reset();
              console.log('✅ V2: AutoDraftManager reset شد');
            }
          } else {
            console.warn('⚠️ V2: ===== حذف Draft ناموفق بود =====');
            
            // استفاده از API.ok برای extract کردن error message
            let okResult = null;
            if (API && typeof API.ok === 'function') {
              try {
                okResult = API.ok(result);
                console.log('🏥 V2: API.ok result:', okResult);
              } catch (err) {
                console.warn('⚠️ V2: خطا در API.ok:', err);
                okResult = result;
              }
            } else {
              okResult = result;
            }
            
            const errorMsg = okResult?.Message || okResult?.message || result?.Message || result?.message || 'خطای نامشخص';
            const errorCode = okResult?.Code || okResult?.code || result?.Code || result?.code || 'UNKNOWN';
            
            console.warn('⚠️ V2: Error Message:', errorMsg);
            console.warn('⚠️ V2: Error Code:', errorCode);
            console.warn('⚠️ V2: Full Response:', JSON.stringify(result, null, 2));
            console.warn('⚠️ V2: Ok Result:', JSON.stringify(okResult, null, 2));
            console.warn('⚠️ V2: ====================================');
            
            // نمایش پیام خطا به کاربر
            if (errorCode !== 'FINALIZED') {
              toastr.warning('خطا در حذف Draft: ' + errorMsg, 'هشدار', {
                timeOut: 5000,
                positionClass: 'toast-top-center'
              });
            }
            
            // اگر Draft نهایی شده است، فقط state را reset کن
            if (errorCode === 'FINALIZED') {
              console.log('ℹ️ V2: Draft نهایی شده است، فقط state را reset می‌کنیم');
              if (window.AutoDraftManager && window.AutoDraftManager.reset) {
                window.AutoDraftManager.reset();
              }
            }
            // ادامه می‌دهیم حتی اگر حذف Draft ناموفق باشد
          }
        } catch (err) {
          console.error('❌ V2: ===== Exception در حذف Draft =====');
          console.error('❌ V2: Error type:', typeof err);
          console.error('❌ V2: Error name:', err?.name);
          console.error('❌ V2: Error message:', err?.message);
          console.error('❌ V2: Error stack:', err?.stack);
          console.error('❌ V2: Full error object:', err);
          console.error('❌ V2: ====================================');
          
          toastr.error('خطا در حذف Draft: ' + (err?.message || 'خطای غیرمنتظره'), 'خطا', {
            timeOut: 5000,
            positionClass: 'toast-top-center'
          });
          // ادامه می‌دهیم حتی اگر حذف Draft ناموفق باشد
        }
      }

      // ✅ 2. Reset AutoDraftManager
      if (window.AutoDraftManager && window.AutoDraftManager.reset) {
        window.AutoDraftManager.reset();
        console.log('✅ V2: AutoDraftManager reset شد');
      }

      // ✅ 3. پاک کردن فیلدهای اطلاعات بیمار
      // پاک کردن فیلدهای readonly/disabled نیز
      $("#NationalCode").val('').prop('readonly', false);
      $("#Patient_NationalCode").val('').prop('readonly', false);
      $("#Patient_PatientId").val('');
      $("#firstName").val('').prop('readonly', false);
      $("#lastName").val('').prop('readonly', false);
      $("#fatherName").val('').prop('readonly', false);
      $("#mobile").val('').prop('readonly', false);
      $("#phone").val('').prop('readonly', false);
      $("#address").val('').prop('readonly', false);
      $("#gender").val('').prop('disabled', false);
      $("#birthSh").val('').prop('readonly', false);
      $("#ReceptionId").val('');
      
      // ✅ Reset Summary Header State (پاک کردن state داخلی)
      if (window.ClinicApp && window.ClinicApp.ReceptionV2 && window.ClinicApp.ReceptionV2.state) {
        window.ClinicApp.ReceptionV2.state = {
          patient: null,
          department: null,
          doctor: null,
          insurances: null,
          financialYear: null
        };
      }
      
      // ✅ Trigger event برای reset Summary Header
      $(document).trigger('rv2:stateChanged', { 
        patient: null,
        insurances: null,
        clinic: null,
        department: null,
        doctor: null
      });
      
      // ✅ Reset Insurance Status Checker (اگر وجود دارد)
      $(document).trigger('patient:selected', [null]);
      
      // ✅ پاک کردن Insurance Status Badge و تمام نمایش‌های وضعیت بیمه (اگر وجود دارد)
      // حذف container اصلی که توسط Insurance Status Checker ایجاد می‌شود
      $('#insurance-status-badge-container').remove();
      $('#insuranceStatusBadge, .insurance-status-badge, #insuranceStatusContainer, .insurance-status-container').remove();
      $('.insurance-status-display, .insurance-status-info, [data-insurance-status]').remove();
      
      // حذف تمام alert‌های مربوط به وضعیت بیمه
      $('.alert-success, .alert-danger, .alert-warning, .alert-info').filter(function() {
        return $(this).text().indexOf('وضعیت بیمه') !== -1 || 
               $(this).text().indexOf('بیمه') !== -1 ||
               $(this).find('strong').text().indexOf('وضعیت بیمه') !== -1;
      }).remove();
      
      // ✅ فراخوانی removeAlerts از Insurance Status Checker (اگر وجود دارد)
      if (window.ReceptionV2 && window.ReceptionV2.InsuranceStatusChecker && typeof window.ReceptionV2.InsuranceStatusChecker.removeAlerts === 'function') {
        try {
          window.ReceptionV2.InsuranceStatusChecker.removeAlerts();
          console.log('✅ V2: Insurance Status Checker alerts removed');
        } catch (err) {
          console.warn('⚠️ V2: خطا در removeAlerts:', err);
        }
      }
      
      // ✅ پاک کردن toastr notifications
      if (typeof toastr !== 'undefined' && typeof toastr.clear === 'function') {
        toastr.clear();
      }
      
      // ✅ Reset Readonly State
      // استفاده از setReadonly از patient-lookup module
      if (typeof setReadonly === 'function') {
        setReadonly(false);
      }
      
      console.log('✅ V2: فیلدهای اطلاعات بیمار پاک شد');

      // ✅ 4. پاک کردن فیلدهای بیمه
      $("#BasePlanId").val('').trigger('change');
      $("#SuppPlanId").val('').trigger('change');
      
      // ✅ Reset Insurance Panel (اگر وجود دارد)
      // استفاده از set با null برای پاک کردن بیمه‌ها
      if (window.insPanel && typeof window.insPanel.set === 'function') {
        try {
          window.insPanel.set({ 
            BasePlanId: null, 
            SupplementaryPlanId: null,
            basePlanId: null, 
            suppPlanId: null,
            BasePlanName: null,
            SupplementaryPlanName: null
          });
          console.log('✅ V2: Insurance Panel reset شد');
        } catch (err) {
          console.warn('⚠️ V2: خطا در reset Insurance Panel:', err);
        }
      }
      
      // ✅ پاک کردن نمایش بیمه در Summary Header
      $('[data-field="base-ins-name"]').text('بیمه پایه: —');
      $('[data-field="supp-ins-name"]').text('تکمیلی: —');
      
      console.log('✅ V2: فیلدهای بیمه پاک شد');

      // ✅ 5. پاک کردن فیلدهای دپارتمان و پزشک
      $("#DepartmentId").val('').trigger('change');
      $("#DoctorId").val('').trigger('change');
      console.log('✅ V2: فیلدهای دپارتمان و پزشک پاک شد');

      // ✅ 6. پاک کردن فیلدهای خدمت
      $("#ServiceId").val('').trigger('change');
      $("#Quantity").val(1);
      console.log('✅ V2: فیلدهای خدمت پاک شد');

      // ✅ 7. پاک کردن آیتم‌های جدول
      const $itemsGrid = $('#items-grid tbody, #ReceptionItemsList tbody');
      $itemsGrid.empty();
      console.log('✅ V2: آیتم‌های جدول پاک شد');

      // ✅ 8. Reset Totals
      $("#Gross").text('0').attr('data-value', '0');
      $("#InsurancePayable").text('0');
      $("#SuppPayable").text('0');
      $("#PatientPayable").text('0').attr('data-value', '0');
      console.log('✅ V2: Totals reset شد');

      // ✅ 9. Reset Payment Method
      $("#PayPOS").addClass('active btn-primary').removeClass('btn-outline-secondary');
      $("#PayCash").removeClass('active btn-primary').addClass('btn-outline-secondary');
      console.log('✅ V2: Payment method reset شد');

      // ✅ 10. Reset Form Dirty State
      if (window.FormDirty && window.FormDirty.clean) {
        window.FormDirty.clean();
        console.log('✅ V2: Form dirty state reset شد');
      }

      // ✅ 11. پاک کردن Summary Header (اگر وجود دارد)
      // پاک کردن تمام فیلدهای Summary Header
      $('[data-field="patient-fullname"]').text('—');
      $('[data-field="patient-gender"]').text('—');
      $('[data-field="patient-nc"]').text('—');
      $('[data-field="patient-age"]').text('—');
      $('[data-field="patient-address"]').text('—');
      $('[data-field="department-name"]').text('—');
      $('[data-field="doctor-name"]').text('—');
      $('[data-field="base-ins-name"]').text('بیمه پایه: —');
      $('[data-field="supp-ins-name"]').text('تکمیلی: —');
      $('[data-field="fy-name"]').text('—');
      
      // ✅ پاک کردن لینک ویرایش بیمار
      $('#rv2-edit-patient-link').addClass('d-none').attr('href', '#');
      
      // ✅ 12. پاک کردن Insurance Status Badge (اگر وجود دارد) - دوباره برای اطمینان
      $('#insuranceStatusBadge, .insurance-status-badge, #insuranceStatusContainer').remove();
      
      // ✅ 13. Reset Form Dirty State (دوباره برای اطمینان)
      if (window.FormDirty && window.FormDirty.clean) {
        window.FormDirty.clean();
      }
      
      // ✅ 14. Focus روی فیلد کد ملی برای آماده‌سازی پذیرش بیمار بعدی
      setTimeout(function() {
        $("#NationalCode").focus();
        console.log('✅ V2: Focus روی فیلد کد ملی');
      }, 100);

      // ✅ 15. نمایش پیام موفقیت
      toastr.success('فرم پاک شد و آماده پذیرش بیمار بعدی است', 'موفق', {
        timeOut: 3000,
        positionClass: 'toast-top-center'
      });

      console.log('✅ V2: فرم با موفقیت پاک شد و آماده پذیرش بیمار بعدی است');
    } catch (err) {
      console.error('❌ V2: خطا در reset فرم:', err);
      toastr.error('خطا در پاک کردن فرم', 'خطا', {
        timeOut: 5000,
        positionClass: 'toast-top-center'
      });
    }
  }

  // ✅ Event Handler برای دکمه Reset Form
  $(document).on('click', '#BtnResetForm', function(e) {
    e.preventDefault();
    e.stopPropagation();
    
    console.log('🏥 V2: BtnResetForm clicked');
    
    // نمایش تایید از کاربر
    if (confirm('آیا مطمئن هستید که می‌خواهید فرم را پاک کنید؟\n\nتمام اطلاعات وارد شده حذف می‌شود و فرم آماده پذیرش بیمار بعدی می‌شود.')) {
      resetForm();
    }
  });
})(window.ReceptionAPI, window.RxUtils);
