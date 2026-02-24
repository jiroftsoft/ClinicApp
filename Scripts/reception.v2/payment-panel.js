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

  // ============================================
  // POS Payment Client & UI Instances (Global)
  // ============================================
  var posPaymentClient = null;
  var posPaymentUI = null;
  var posPaymentLockManager = null; // ✅ NEW: Lock Manager
  var currentReceptionId = null;
  var currentAmountIRR = null;

  // ✅ اطمینان از لود شدن DOM قبل از attach کردن event handlers
  $(document).ready(function() {
    console.log('🏥 V2: Payment Panel - DOM Ready, attaching event handlers...');
    
    // ============================================
    // Initialize POS Payment Lock Manager
    // ============================================
    if (typeof PosPaymentLockManager !== 'undefined') {
      posPaymentLockManager = new PosPaymentLockManager();
      console.log('✅ V2: PosPaymentLockManager initialized');
    } else {
      console.warn('⚠️ V2: PosPaymentLockManager not found - Lock management disabled');
    }
    
    // ============================================
    // Initialize POS Payment Modules
    // ============================================
    try {
      // تنظیمات SignalR URL - از global variable استفاده می‌کنیم که در view تنظیم می‌شود
      var signalRUrl = window.SamanKishSignalRUrl || 'http://localhost:5000/signalr';
      
      // Initialize PosPaymentClient
      if (typeof PosPaymentClient !== 'undefined') {
        posPaymentClient = new PosPaymentClient({
          signalRUrl: signalRUrl,
          
          onConnecting: function() {
            console.log('🏥 V2: POS Payment - Connecting...');
            if (posPaymentUI) {
              posPaymentUI.showLoading('در حال اتصال...', 'در حال اتصال به دستگاه کارتخوان', 'لطفاً صبر کنید');
            }
          },
          
          onConnected: function() {
            console.log('✅ V2: POS Payment - Connected to SignalR Hub');
          },
          
          onCardSwiped: function(data) {
            console.log('🔔 V2: POS Payment - Card swiped:', data);
            if (posPaymentUI) {
              posPaymentUI.showLoading('کارت کشیده شد', 'لطفاً رمز کارت را وارد کنید', '');
            }
          },
          
          onSuccess: function(response) {
            console.log('═══════════════════════════════════════════════════════════');
            console.log('✅ FRONTEND: POS Payment Success Callback');
            console.log('═══════════════════════════════════════════════════════════');
            console.log('📊 Payment Response:', JSON.stringify(response, null, 2));
            console.log('📋 Current ReceptionId:', currentReceptionId);
            console.log('💰 Current AmountIRR:', currentAmountIRR);
            console.log('⏰ Timestamp:', new Date().toISOString());
            
            // ✅ Unlock payment (success)
            if (posPaymentLockManager) {
              posPaymentLockManager.unlock();
              console.log('🔓 Payment unlocked (success)');
            }
            
            // ذخیره اطلاعات برای Finalize (قبل از نمایش Modal)
            window.posPaymentData = {
              rrn: response.rrn,
              traceNo: response.traceNo,
              terminalId: response.terminalId,
              cardLast4: response.cardLast4
            };
            console.log('💾 POS Payment Data saved:', JSON.stringify(window.posPaymentData, null, 2));
            
            // ✅ CRITICAL: منطق دامین - وقتی پرداخت موفق است، پذیرش باید به صورت خودکار نهایی شود
            // دکمه "تأیید و نهایی‌سازی" فقط برای چاپ قبض است (اختیاری)
            console.log('🏥 FRONTEND: پرداخت موفق - شروع نهایی‌سازی خودکار پذیرش...');
            
            // ✅ نهایی‌سازی خودکار پس از پرداخت موفق
            if (currentReceptionId && currentAmountIRR && window.posPaymentData) {
              console.log('✅ FRONTEND: تمام اطلاعات لازم برای Finalize موجود است');
              console.log('⏳ FRONTEND: تاخیر 500ms قبل از شروع Finalize...');
              // تاخیر کوتاه برای نمایش پیام موفقیت به کاربر
              setTimeout(function() {
                console.log('🚀 FRONTEND: اجرای نهایی‌سازی خودکار - ReceptionId:', currentReceptionId);
                finalizeAfterPayment(currentReceptionId, currentAmountIRR, window.posPaymentData);
              }, 500); // 500ms delay برای UX بهتر
            } else {
              console.error('❌ FRONTEND: اطلاعات لازم برای Finalize موجود نیست:', {
                receptionId: currentReceptionId,
                amountIRR: currentAmountIRR,
                posData: window.posPaymentData
              });
            }
            
            // نمایش موفقیت در Modal (بعد از شروع Finalize)
            if (posPaymentUI) {
              console.log('🎨 FRONTEND: نمایش Modal موفقیت...');
              posPaymentUI.showSuccess({
                rrn: response.rrn,
                traceNo: response.traceNo,
                terminalId: response.terminalId,
                cardLast4: response.cardLast4,
                amount: currentAmountIRR,
                txnDate: new Date().toLocaleDateString('fa-IR')
              });
            }
            console.log('═══════════════════════════════════════════════════════════');
          },
          
          onCancel: function(response) {
            console.log('⚠️ V2: POS Payment - Canceled:', response);
            
            // ✅ Unlock payment via Lock Manager
            if (posPaymentLockManager) {
              posPaymentLockManager.unlock();
            }
            
            if (posPaymentUI) {
              posPaymentUI.showCanceled();
            }
          },
          
          onError: function(error) {
            console.error('❌ V2: POS Payment - Error:', error);
            
            // ✅ Unlock payment (error)
            if (posPaymentLockManager) {
              posPaymentLockManager.unlock();
              console.log('🔓 Payment unlocked (error)');
            }
            
            if (posPaymentUI) {
              posPaymentUI.showError(error.message || 'خطا در پرداخت', error.code);
            }
          }
        });
        
        console.log('✅ V2: PosPaymentClient initialized');
      } else {
        console.warn('⚠️ V2: PosPaymentClient not found - make sure pos-payment-client.js is loaded');
      }
      
      // Initialize PosPaymentUI
      if (typeof PosPaymentUI !== 'undefined') {
        posPaymentUI = new PosPaymentUI({
          modalId: 'posPaymentModal',
          
          onStart: function() {
            // این callback در payment-panel.js مدیریت می‌شود
            console.log('🏥 V2: POS Payment - Start button clicked');
          },
          
          onConfirm: function() {
            console.log('✅ V2: POS Payment - Confirm clicked');
            console.log('🔍 V2: Checking finalization state:', {
              _receptionFinalized: window._receptionFinalized,
              _finalizingReceptionId: window._finalizingReceptionId,
              currentReceptionId: currentReceptionId,
              posPaymentData: window.posPaymentData ? 'exists' : 'null'
            });
            
            // ✅ CRITICAL: بررسی اینکه آیا Finalize قبلاً انجام شده است یا نه
            // اگر انجام شده باشد، فقط Modal را ببند (بدون Finalize مجدد و بدون Popup)
            if (window._receptionFinalized === true) {
              console.log('✅ V2: Finalize قبلاً انجام شده است - فقط Modal را می‌بندیم (بدون Popup)');
              closePosPaymentModal();
              return;
            }
            
            // ✅ بررسی اینکه آیا Finalize در حال انجام است یا نه
            if (window._finalizingReceptionId !== null && window._finalizingReceptionId === currentReceptionId) {
              console.log('⏳ V2: Finalize در حال انجام است - منتظر می‌مانیم');
              toastr.info('در حال نهایی‌سازی پذیرش... لطفاً صبر کنید.', 'در حال پردازش', {
                timeOut: 3000,
                positionClass: 'toast-top-center'
              });
              return;
            }
            
            // ✅ CRITICAL: بررسی از DOM - اگر ReceptionId در DOM موجود نیست یا Status Finalized است، Finalize انجام نشود
            var receptionIdFromDOM = $("#ReceptionId").val();
            var receptionStatusFromDOM = $("#ReceptionStatus").val() || $("#receptionStatus").val();
            
            if (receptionIdFromDOM && parseInt(receptionIdFromDOM, 10) > 0) {
              // اگر Status Finalized است، فقط Modal را ببند
              if (receptionStatusFromDOM === 'Finalized' || receptionStatusFromDOM === 'finalized') {
                console.log('✅ V2: Reception قبلاً نهایی شده است (از DOM) - فقط Modal را می‌بندیم');
                window._receptionFinalized = true; // ✅ Set flag برای جلوگیری از Finalize مجدد
                closePosPaymentModal();
                return;
              }
            }
            
            // ✅ Fallback: اگر Finalize انجام نشده باشد، انجام می‌دهیم
            if (currentReceptionId && currentAmountIRR && window.posPaymentData) {
              console.log('🏥 V2: Finalize انجام نشده - انجام Finalize...');
              finalizeAfterPayment(currentReceptionId, currentAmountIRR, window.posPaymentData);
            } else {
              console.log('⚠️ V2: اطلاعات لازم برای Finalize موجود نیست - فقط Modal را می‌بندیم');
              closePosPaymentModal();
            }
          },
          
          onPrint: function() {
            // ✅ DISABLED: استفاده از Print Manager به جای این callback
            // این callback باعث duplicate call می‌شود
            // چاپ از طریق event handler دکمه انجام می‌شود
            console.log('🖨️ V2: POS Payment - Print clicked (handled by button event)');
            // Do nothing - handled by button click event
          },
          
          onRetry: function() {
            console.log('🔄 V2: POS Payment - Retry clicked');
            if (currentReceptionId && currentAmountIRR) {
              // Retry payment
              openPosPaymentModal(currentReceptionId, currentAmountIRR);
            }
          },
          
          onCancel: function() {
            console.log('❌ V2: POS Payment - Cancel clicked');
            
            // ✅ CRITICAL: اگر پرداخت موفق بوده اما Finalize انجام نشده، انجام می‌دهیم
            // منطق دامین: اگر پرداخت موفق است، پذیرش باید نهایی شود حتی اگر کاربر پنجره را ببندد
            if (window.posPaymentData && currentReceptionId && currentAmountIRR) {
              console.log('⚠️ V2: Modal بسته شد اما پرداخت موفق بوده - انجام Finalize خودکار...');
              // تاخیر کوتاه برای اطمینان از بسته شدن Modal
              setTimeout(function() {
                if (window._finalizingReceptionId !== currentReceptionId) {
                  finalizeAfterPayment(currentReceptionId, currentAmountIRR, window.posPaymentData);
                }
              }, 300);
            }
            
            // ✅ Cancel payment via client
            if (posPaymentClient) {
              posPaymentClient.cancelPayment('USER_CANCELLED');
            }
            
            // ✅ Unlock via Lock Manager
            if (posPaymentLockManager) {
              posPaymentLockManager.unlock();
            }
            
            posPaymentUI.close();
            // ❌ Reset payment data را حذف کردیم - برای Fallback Finalize نیاز داریم
            // window.posPaymentData = null;
          }
        });
        
        console.log('✅ V2: PosPaymentUI initialized');
        
        // ✅ CRITICAL: Fallback mechanism - اگر Modal بسته شد و Finalize انجام نشده، انجام می‌دهیم
        // منطق دامین: اگر پرداخت موفق است، پذیرش باید نهایی شود حتی اگر کاربر پنجره را ببندد
        // ✅ جلوگیری از Event Handler تکراری
        var modalElement = document.getElementById('posPaymentModal');
        if (modalElement) {
          // ✅ Cleanup Event Handlers قبلی (جلوگیری از چند بار attach)
          $(modalElement).off('hidden.bs.modal');
          
          // ✅ Bootstrap 5 event (یک بار attach با once)
          if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            var modalCloseHandler = function() {
              console.log('🔔 V2: POS Payment Modal closed (Bootstrap 5)');
              
              // ✅ حذف Event Handler بعد از یک بار استفاده (جلوگیری از چند بار اجرا)
              modalElement.removeEventListener('hidden.bs.modal', modalCloseHandler);
              
              // اگر پرداخت موفق بوده اما Finalize انجام نشده، انجام می‌دهیم
              if (window.posPaymentData && currentReceptionId && currentAmountIRR) {
                // تاخیر برای اطمینان از بسته شدن کامل Modal
                setTimeout(function() {
                  // ✅ بررسی اینکه آیا Finalize قبلاً انجام شده است یا نه
                  if (window._finalizingReceptionId !== null && window._finalizingReceptionId === currentReceptionId) {
                    console.log('✅ V2: Finalize قبلاً انجام شده است - ReceptionId:', window._finalizingReceptionId);
                    return;
                  }
                  
                  // ✅ بررسی اینکه آیا Reception قبلاً نهایی شده است یا نه (از DOM)
                  const receptionIdFromDOM = $("#ReceptionId").val();
                  if (receptionIdFromDOM && parseInt(receptionIdFromDOM, 10) > 0) {
                    // برای جلوگیری از Finalize تکراری، فقط اگر flag وجود نداشته باشد، Finalize می‌کنیم
                    if (!window._finalizingReceptionId) {
                      console.log('⚠️ V2: Modal بسته شد اما Finalize انجام نشده - انجام Finalize خودکار...');
                      finalizeAfterPayment(currentReceptionId, currentAmountIRR, window.posPaymentData);
                    } else {
                      console.log('✅ V2: Finalize در حال انجام است - منتظر می‌مانیم');
                    }
                  } else {
                    console.log('ℹ️ V2: ReceptionId در DOM موجود نیست - Finalize انجام نمی‌شود');
                  }
                }, 500);
              } else {
                console.log('ℹ️ V2: پرداخت موفق نبوده یا داده‌های لازم موجود نیست');
              }
            };
            
            modalElement.addEventListener('hidden.bs.modal', modalCloseHandler, { once: true });
          }
          
          // ✅ Bootstrap 4 fallback (یک بار attach با one)
          $(modalElement).one('hidden.bs.modal', function() {
            console.log('🔔 V2: POS Payment Modal closed (Bootstrap 4)');
            
            // اگر پرداخت موفق بوده اما Finalize انجام نشده، انجام می‌دهیم
            if (window.posPaymentData && currentReceptionId && currentAmountIRR) {
              // تاخیر برای اطمینان از بسته شدن کامل Modal
              setTimeout(function() {
                // ✅ بررسی اینکه آیا Finalize قبلاً انجام شده است یا نه
                if (window._finalizingReceptionId !== null && window._finalizingReceptionId === currentReceptionId) {
                  console.log('✅ V2: Finalize قبلاً انجام شده است - ReceptionId:', window._finalizingReceptionId);
                  return;
                }
                
                // ✅ بررسی اینکه آیا Reception قبلاً نهایی شده است یا نه (از DOM)
                const receptionIdFromDOM = $("#ReceptionId").val();
                if (receptionIdFromDOM && parseInt(receptionIdFromDOM, 10) > 0) {
                  // برای جلوگیری از Finalize تکراری، فقط اگر flag وجود نداشته باشد، Finalize می‌کنیم
                  if (!window._finalizingReceptionId) {
                    console.log('⚠️ V2: Modal بسته شد اما Finalize انجام نشده - انجام Finalize خودکار...');
                    finalizeAfterPayment(currentReceptionId, currentAmountIRR, window.posPaymentData);
                  } else {
                    console.log('✅ V2: Finalize در حال انجام است - منتظر می‌مانیم');
                  }
                } else {
                  console.log('ℹ️ V2: ReceptionId در DOM موجود نیست - Finalize انجام نمی‌شود');
                }
              }, 500);
            } else {
              console.log('ℹ️ V2: پرداخت موفق نبوده یا داده‌های لازم موجود نیست');
            }
          });
        }
      } else {
        console.warn('⚠️ V2: PosPaymentUI not found - make sure pos-payment-ui.js is loaded');
      }
    } catch (ex) {
      console.error('❌ V2: Error initializing POS Payment modules:', ex);
    }
    
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
            // ✅ باز کردن مودال پرداخت POS (استفاده از ماژول جدید)
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
            
            const receptionIdForFinalize = $("#ReceptionId").val() ? parseInt($("#ReceptionId").val(), 10) : null;
            finalizeReception(payload, false, receptionIdForFinalize);
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
            
            const receptionIdForFinalize = $("#ReceptionId").val() ? parseInt($("#ReceptionId").val(), 10) : null;
            finalizeReception(payload, false, receptionIdForFinalize);
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
            const receptionIdForFinalize = $("#ReceptionId").val() ? parseInt($("#ReceptionId").val(), 10) : null;
            finalizeReception(payload, false, receptionIdForFinalize); // به عنوان نقدی نهایی کن
            return;
          } else {
            toastr.warning('مبلغ قابل پرداخت صفر است. لطفاً ابتدا خدمت اضافه کنید یا بیمه را تنظیم کنید.');
            return;
          }
        }
        
        // ✅ باز کردن Modal پرداخت POS (استفاده از ماژول جدید)
        // اطلاعات تراکنش از طریق PosPaymentClient callbacks دریافت می‌شود
        // Finalize در onConfirm callback از PosPaymentUI انجام می‌شود
        console.log('🏥 V2: Opening POS Payment Modal with amount:', amountIRR);
        openPosPaymentModal(receptionId, amountIRR);
        return; // منتظر بمان تا پرداخت انجام شود و کاربر Confirm کند
      } else {
        payload.cash = {
          cashSessionId: $("#CashSessionId").val() || null
        };
      }
      
      // ✅ گام 7: ارسال درخواست نهایی‌سازی
      const receptionIdForFinalize = $("#ReceptionId").val() ? parseInt($("#ReceptionId").val(), 10) : null;
      finalizeReception(payload, isPOS, receptionIdForFinalize);
    });
  }
  
  /**
   * ✅ نهایی‌سازی پس از پرداخت موفق
   * منطق دامین: وقتی پرداخت موفق است، پذیرش باید به صورت خودکار نهایی شود
   */
  function finalizeAfterPayment(receptionId, amountIRR, posData) {
    console.log('═══════════════════════════════════════════════════════════');
    console.log('🏥 FRONTEND: Finalize After Payment - شروع');
    console.log('═══════════════════════════════════════════════════════════');
    console.log('📋 ReceptionId:', receptionId);
    console.log('💰 AmountIRR:', amountIRR);
    console.log('💳 POS Data:', JSON.stringify(posData, null, 2));
    console.log('⏰ Timestamp:', new Date().toISOString());
    
    // ✅ جلوگیری از Finalize تکراری
    if (window._finalizingReceptionId === receptionId) {
      console.warn('⚠️ FRONTEND: Finalize در حال انجام است برای ReceptionId:', receptionId);
      console.log('═══════════════════════════════════════════════════════════');
      return;
    }
    
    // ✅ علامت‌گذاری برای جلوگیری از Finalize تکراری
    window._finalizingReceptionId = receptionId;
    console.log('🔒 FRONTEND: Finalize flag set - ReceptionId:', receptionId);
    
    // ✅ CRITICAL: Model Binding - باید با Controllers.Api.FinalizePosRequest مطابقت داشته باشد
    // Backend انتظار دارد: ReceptionId (PascalCase), Amount (نه AmountIRR!), PosPayment (نه pos!)
    // توجه: PosPaymentDto نیاز به Amount ندارد (فقط RRN, TraceNo, TerminalId, CardLast4)
    const idempotencyKey = U.guid();
    console.log('🔑 FRONTEND: IdempotencyKey generated:', idempotencyKey);
    
    const payload = {
      ReceptionId: receptionId,  // ✅ PascalCase برای Model Binding
      Amount: amountIRR,          // ✅ Amount (نه amountIRR!)
      IdempotencyKey: idempotencyKey,
      PosPayment: {                // ✅ PosPayment (نه pos!)
        Amount: amountIRR,         // ✅ Amount در PosPaymentDto (از کد Backend خط 1688)
        RRN: posData.rrn,          // ✅ PascalCase
        TraceNo: posData.traceNo,  // ✅ PascalCase
        TerminalId: posData.terminalId,  // ✅ PascalCase
        CardLast4: posData.cardLast4 || null  // ✅ PascalCase
      }
    };
    
    console.log('📦 FRONTEND: Finalize Payload (برای Model Binding):');
    console.log(JSON.stringify(payload, null, 2));
    
    // ✅ CRITICAL: Modal را باز نگه می‌داریم تا منشی بتواند قبض را چاپ بگیرد
    // Modal فقط زمانی بسته می‌شود که کاربر دکمه "بستن" را بزند
    console.log('✅ FRONTEND: Modal باز نگه داشته می‌شود برای چاپ قبض');
    
    console.log('🚀 FRONTEND: فراخوانی finalizeReception...');
    finalizeReception(payload, true, receptionId); // ✅ اضافه کردن receptionId برای چاپ
    console.log('═══════════════════════════════════════════════════════════');
  }
  
  /**
   * ✅ نهایی‌سازی پذیرش
   * @param {Object} payload - Payload برای Finalize
   * @param {boolean} isPOS - آیا پرداخت POS است یا نقدی
   * @param {number} receptionId - شناسه پذیرش (برای چاپ قبض)
   */
  function finalizeReception(payload, isPOS, receptionId) {
    console.log('═══════════════════════════════════════════════════════════');
    console.log('🏥 FRONTEND: Finalize Reception - شروع');
    console.log('═══════════════════════════════════════════════════════════');
    console.log('📋 Payload:', JSON.stringify(payload, null, 2));
    console.log('💳 IsPOS:', isPOS);
    console.log('📋 ReceptionId:', receptionId);
    console.log('⏰ Timestamp:', new Date().toISOString());
    
    // ✅ Fallback: اگر receptionId پاس نشده، از payload بگیر
    if (!receptionId && payload && payload.ReceptionId) {
      receptionId = parseInt(payload.ReceptionId, 10);
      console.log('✅ FRONTEND: ReceptionId از Payload استخراج شد:', receptionId);
    }
    
    // ✅ علامت‌گذاری Draft به عنوان در حال نهایی شدن
    if (window.AutoDraftManager && window.AutoDraftManager.markDraftAsFinalizing) {
      window.AutoDraftManager.markDraftAsFinalizing();
      console.log('✅ FRONTEND: Draft marked as finalizing');
    }
    
    const endpoint = isPOS ? "/finalize/pos" : "/finalize/cash";
    const fullUrl = '/api/v1/reception' + endpoint;
    console.log('🌐 FRONTEND: Endpoint:', endpoint);
    console.log('🔗 FRONTEND: Full URL:', fullUrl);
    
    console.log('📤 FRONTEND: ارسال درخواست Finalize به Backend...');
    const requestStartTime = Date.now();
    
    API.post(endpoint, payload)
      .then(function(response) {
        const requestDuration = Date.now() - requestStartTime;
        console.log('═══════════════════════════════════════════════════════════');
        console.log('📥 FRONTEND: Response received from Backend');
        console.log('═══════════════════════════════════════════════════════════');
        console.log('⏱️ Request Duration:', requestDuration + 'ms');
        console.log('📊 Response:', JSON.stringify(response, null, 2));
        console.log('⏰ Timestamp:', new Date().toISOString());
        
        // ✅ CRITICAL: بررسی Success از Response قبل از Extract
        const isSuccess = response && (response.Success === true || response.success === true);
        if (!isSuccess) {
          const errorMsg = response?.Message || response?.message || 'خطا در نهایی‌سازی پذیرش';
          const errorCode = response?.Code || response?.code || 'GENERAL_ERROR';
          console.error('❌ FRONTEND: Finalize failed - Message:', errorMsg, 'Code:', errorCode);
          
        // ✅ پاک کردن flag در صورت خطا
        window._finalizingReceptionId = null;
        window._receptionFinalized = false; // ✅ Reset flag در صورت خطا
        
        // ✅ در صورت خطا، flag را بردار (Draft هنوز نهایی نشده)
        if (window.AutoDraftManager && window.AutoDraftManager.unmarkDraftAsFinalizing) {
          window.AutoDraftManager.unmarkDraftAsFinalizing();
          console.log('🔄 FRONTEND: Draft unmarked as finalizing (error)');
        }
          
          toastr.error(errorMsg, 'خطا', {
            timeOut: 7000,
            positionClass: 'toast-top-center',
            closeButton: true
          });
          
          // ✅ اگر خطا بود، Promise را reject کن
          return Promise.reject({
            message: errorMsg,
            code: errorCode,
            response: response
          });
        }
        
        // ✅ Extract data از Response (فقط اگر Success بود)
        return API.ok(response);
      })
      .then(function(d){ 
        console.log('═══════════════════════════════════════════════════════════');
        console.log('✅ FRONTEND: Reception Finalized Successfully');
        console.log('═══════════════════════════════════════════════════════════');
        console.log('📊 Finalize Result:', JSON.stringify(d, null, 2));
        console.log('⏰ Timestamp:', new Date().toISOString());
        
        // ✅ پاک کردن flag برای جلوگیری از Finalize تکراری
        window._finalizingReceptionId = null;
        // ✅ CRITICAL: ذخیره flag برای بررسی در onConfirm (جلوگیری از Finalize تکراری)
        window._receptionFinalized = true;
        console.log('🔓 FRONTEND: Finalize flag cleared');
        console.log('✅ FRONTEND: Reception Finalized flag set - ReceptionId:', receptionId);
        
        // ✅ CRITICAL: Unmark Draft as Finalizing - چون Finalize موفق شد
        if (window.AutoDraftManager && typeof window.AutoDraftManager.unmarkDraftAsFinalizing === 'function') {
          window.AutoDraftManager.unmarkDraftAsFinalizing();
          console.log('✅ FRONTEND: Draft unmarked as finalizing (success)');
        }
        
        toastr.success("پذیرش با موفقیت نهایی شد", 'موفق', {
          timeOut: 5000
        });
        
        // ✅ CRITICAL: اگر پرداخت POS است، Modal را باز نگه می‌داریم و دکمه‌های چاپ را فعال می‌کنیم
        if (isPOS && receptionId && posPaymentUI) {
          console.log('🖨️ FRONTEND: فعال کردن دکمه‌های چاپ در Modal - ReceptionId:', receptionId);
          
          // ذخیره ReceptionId برای استفاده در دکمه‌های چاپ
          window._currentReceptionIdForPrint = receptionId;
          
          // ✅ بررسی وجود بیمه تکمیلی از Response یا Form
          var hasSupplementaryInsurance = false;
          if (d && (d.SupplementaryPlanId || d.SupplementaryPlanName)) {
            hasSupplementaryInsurance = true;
            console.log('✅ FRONTEND: بیمه تکمیلی یافت شد - PlanId:', d.SupplementaryPlanId, 'PlanName:', d.SupplementaryPlanName);
          } else {
            // Fallback: بررسی از Form
            var suppPlanId = $('#SupplementaryPlanId').val() || $('#supplementaryPlanId').val();
            if (suppPlanId && parseInt(suppPlanId) > 0) {
              hasSupplementaryInsurance = true;
              console.log('✅ FRONTEND: بیمه تکمیلی از Form یافت شد - PlanId:', suppPlanId);
            }
          }
          
          // ✅ Cleanup Event Handlers قبل از attach جدید (جلوگیری از چند بار attach)
          $('#posPaymentPrintBtn').off('click.print');
          $('#posPaymentPrintInsuranceBtn').off('click.print');
          $('#posPaymentConfirmBtn').off('click');
          
          // ✅ فعال کردن دکمه چاپ قبض پرداخت با Print Manager
          $('#posPaymentPrintBtn').on('click.print', function(e) {
            e.preventDefault();
            e.stopPropagation();
            console.log('🖨️ FRONTEND: چاپ قبض پرداخت - ReceptionId:', receptionId);
            
            // ✅ استفاده از Print Manager برای چاپ حرفه‌ای
            if (window.PrintManager && typeof window.PrintManager.print === 'function') {
              var printUrl = `/ReceptionV2/PrintReceipt/${receptionId}?type=payment&printer=thermal`;
              window.PrintManager.print(printUrl)
                .then(function() {
                  console.log('✅ FRONTEND: چاپ قبض پرداخت با موفقیت به صف اضافه شد');
                })
                .catch(function(err) {
                  console.error('❌ FRONTEND: خطا در چاپ:', err);
                  toastr.error(err.message || 'خطا در چاپ قبض پرداخت', 'خطا', {
                    timeOut: 5000,
                    closeButton: true
                  });
                });
            } else {
              // Fallback: استفاده از تابع قدیمی
              console.warn('⚠️ FRONTEND: PrintManager not available, using fallback');
              printPaymentReceipt(receptionId);
            }
          });
          
          // ✅ اضافه کردن دکمه چاپ قبض بیمه تکمیلی (اگر وجود ندارد)
          if (!$('#posPaymentPrintInsuranceBtn').length) {
            var $printInsuranceBtn = $('<button>', {
              type: 'button',
              class: 'btn btn-warning d-none',
              id: 'posPaymentPrintInsuranceBtn',
              html: '<i class="fas fa-file-invoice me-2"></i>چاپ قبض بیمه تکمیلی'
            });
            $('#posPaymentPrintBtn').after($printInsuranceBtn);
          }
          
          // ✅ فعال کردن دکمه چاپ قبض بیمه تکمیلی با Print Manager
          $('#posPaymentPrintInsuranceBtn').on('click.print', function(e) {
            e.preventDefault();
            e.stopPropagation();
            console.log('🖨️ FRONTEND: چاپ قبض بیمه تکمیلی - ReceptionId:', receptionId);
            
            // ✅ استفاده از Print Manager برای چاپ حرفه‌ای
            if (window.PrintManager && typeof window.PrintManager.print === 'function') {
              var printUrl = `/ReceptionV2/PrintInsurance/${receptionId}`;
              window.PrintManager.print(printUrl)
                .then(function() {
                  console.log('✅ FRONTEND: چاپ قبض بیمه تکمیلی با موفقیت به صف اضافه شد');
                })
                .catch(function(err) {
                  console.error('❌ FRONTEND: خطا در چاپ:', err);
                  toastr.error(err.message || 'خطا در چاپ قبض بیمه تکمیلی', 'خطا', {
                    timeOut: 5000,
                    closeButton: true
                  });
                });
            } else {
              // Fallback: استفاده از تابع قدیمی
              console.warn('⚠️ FRONTEND: PrintManager not available, using fallback');
              printInsuranceReceipt(receptionId);
            }
          });
          
          // ✅ نمایش دکمه‌های چاپ
          $('#posPaymentPrintBtn').removeClass('d-none');
          if (hasSupplementaryInsurance) {
            $('#posPaymentPrintInsuranceBtn').removeClass('d-none');
            console.log('✅ FRONTEND: دکمه چاپ بیمه تکمیلی فعال شد');
          } else {
            $('#posPaymentPrintInsuranceBtn').addClass('d-none');
            console.log('ℹ️ FRONTEND: بیمه تکمیلی وجود ندارد - دکمه چاپ بیمه تکمیلی مخفی شد');
          }
          
          // ✅ تغییر متن و استایل دکمه "تأیید" به "بستن" (Finalize خودکار انجام شده است)
          $('#posPaymentConfirmBtn')
            .html('<i class="fas fa-times me-2"></i>بستن')
            .removeClass('btn-success')
            .addClass('btn-secondary')
            .removeClass('d-none')
            .on('click', function() {
              console.log('🚪 FRONTEND: بستن Modal توسط کاربر');
              closePosPaymentModal();
            });
          
          // ✅ مخفی کردن دکمه‌های غیرضروری
          $('#posPaymentStartBtn').addClass('d-none');
          $('#posPaymentRetryBtn').addClass('d-none');
          
          // ✅ جلوگیری از بستن Modal با ESC یا Backdrop بعد از موفقیت
          var modalElement = document.getElementById('posPaymentModal');
          if (modalElement) {
            // جلوگیری از بستن با ESC
            $(modalElement).off('keydown.dismiss.bs.modal');
            // جلوگیری از بستن با Backdrop
            $(modalElement).data('bs.modal', null);
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
              var modalInstance = bootstrap.Modal.getInstance(modalElement);
              if (modalInstance) {
                modalInstance._config.keyboard = false;
                modalInstance._config.backdrop = 'static';
              }
            }
            console.log('🔒 FRONTEND: Modal قفل شد - فقط با دکمه "بستن" قابل بستن است');
          }
          
          // ❌ برای پرداخت POS، reload نمی‌کنیم - Modal باز می‌ماند
          console.log('✅ FRONTEND: Modal باز نگه داشته می‌شود برای چاپ قبض');
        } else {
          // ✅ برای پرداخت نقدی، نمایش گزینه چاپ به صورت معمول
          if(d.receipt && d.receipt.printedUrl) {
            setTimeout(function() {
              if (confirm('آیا می‌خواهید قبض پرداخت را چاپ کنید؟')) {
                window.open(d.receipt.printedUrl, '_blank');
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
        }
      })
      .catch(function(err) {
        console.error('═══════════════════════════════════════════════════════════');
        console.error('❌ FRONTEND: Finalize Error');
        console.error('═══════════════════════════════════════════════════════════');
        console.error('📊 Error Object:', err);
        console.error('📋 Error Message:', err?.message);
        console.error('📋 Error Status:', err?.status);
        console.error('📋 Error StatusText:', err?.statusText);
        console.error('📋 Response JSON:', err?.responseJSON);
        console.error('📋 Response Text:', err?.responseText);
        console.error('⏰ Timestamp:', new Date().toISOString());
        
        // ✅ پاک کردن flag در صورت خطا
        window._finalizingReceptionId = null;
        window._receptionFinalized = false; // ✅ Reset flag در صورت خطا
        console.log('🔓 FRONTEND: Finalize flag cleared (error)');
        
        // ✅ در صورت خطا، flag را بردار (Draft هنوز نهایی نشده)
        if (window.AutoDraftManager && window.AutoDraftManager.unmarkDraftAsFinalizing) {
          window.AutoDraftManager.unmarkDraftAsFinalizing();
          console.log('🔄 FRONTEND: Draft unmarked as finalizing (error)');
        }
        
        // ✅ نمایش پیام خطای دقیق‌تر
        const errorMsg = err?.responseJSON?.Message || 
                        err?.responseJSON?.message || 
                        err?.message || 
                        'خطا در نهایی‌سازی پذیرش';
        
        console.error('💬 Error Message to User:', errorMsg);
        console.error('═══════════════════════════════════════════════════════════');
        
        toastr.error(errorMsg, 'خطا', {
          timeOut: 7000,
          positionClass: 'toast-top-center',
          closeButton: true
        });
      });
  }
  
  /**
   * ✅ باز کردن Modal پرداخت POS و ارتباط با دستگاه کارتخوان
   * استفاده از ماژول جدید PosPaymentClient و PosPaymentUI
   */
  function openPosPaymentModal(receptionId, amountIRR) {
    console.log('🏥 V2: Opening POS Payment Modal - ReceptionId:', receptionId, 'AmountIRR:', amountIRR);
    
    // ✅ ذخیره ReceptionId و Amount برای استفاده در Callbacks
    currentReceptionId = receptionId;
    currentAmountIRR = amountIRR;
    
    // ✅ Reset flag برای پذیرش جدید (قبل از باز کردن Modal)
    window._receptionFinalized = false;
    console.log('🔄 FRONTEND: Reception Finalized flag reset for new payment');
    
    // ✅ بررسی Lock Manager - جلوگیری از پرداخت همزمان
    if (posPaymentLockManager && posPaymentLockManager.isLocked()) {
      console.warn('⚠️ V2: Payment already in progress (locked)');
      toastr.warning('یک پرداخت در حال انجام است. لطفاً صبر کنید یا آن را لغو کنید.', 'توجه', {
        timeOut: 5000,
        closeButton: true,
        positionClass: 'toast-top-center'
      });
      return;
    }
    
    // ذخیره برای استفاده در callbacks
    currentReceptionId = receptionId;
    currentAmountIRR = amountIRR;
    
    // ✅ بررسی اینکه ماژول‌ها initialize شده‌اند
    if (!posPaymentClient || !posPaymentUI) {
      console.error('❌ V2: POS Payment modules not initialized');
      toastr.error('ماژول پرداخت POS آماده نیست. لطفاً صفحه را نوسازی کنید.', 'خطا', {
        timeOut: 5000
      });
      return;
    }
    
    // ✅ دریافت اطلاعات ترمینال پیش‌فرض (GET request)
    API.get('/pos/terminals/default')
      .then(function(response) {
        console.log('🏥 V2: GetDefault Terminal Response:', response);
        
        if (response && response.Success && response.Data) {
          const terminal = API.ok(response);
          const terminalName = terminal.title || terminal.Title || 'دستگاه کارتخوان';
          const terminalId = terminal.terminalId || terminal.TerminalId;
          const ipAddress = terminal.ipAddress || terminal.IpAddress;
          
          // ✅ نمایش Modal با اطلاعات ترمینال
          posPaymentUI.setPaymentInfo(amountIRR, terminalName);
          posPaymentUI.open();
          
          // ✅ تنظیم callback برای دکمه "پرداخت با POS"
          // این callback در PosPaymentUI.onStart مدیریت می‌شود
          // اما ما باید processPayment را فراخوانی کنیم
          $('#posPaymentStartBtn').off('click').on('click', function() {
            console.log('🏥 V2: POS Payment Start button clicked');
            
            // ✅ Lock payment
            if (posPaymentLockManager) {
              posPaymentLockManager.lock();
              console.log('🔒 Payment locked');
            }
            
            // نمایش Loading
            posPaymentUI.showLoading('در حال ارسال مبلغ...', 'در حال ارسال مبلغ به دستگاه POS', 'لطفاً کارت را وارد کنید');
            
            // شروع پرداخت با PosPaymentClient
            if (posPaymentClient && terminalId && ipAddress) {
              posPaymentClient.processPayment(terminalId, amountIRR, ipAddress);
            } else {
              console.error('❌ V2: Missing terminal info:', { terminalId, ipAddress });
              posPaymentUI.showError('اطلاعات ترمینال ناقص است', 'INVALID_TERMINAL');
              
              // ✅ Unlock on error
              if (posPaymentLockManager) {
                posPaymentLockManager.unlock();
              }
            }
          });
        } else {
          const errorMsg = response?.Message || response?.message || 'ترمینال POS پیش‌فرض یافت نشد. لطفاً ابتدا ترمینال را تنظیم کنید.';
          console.error('🏥 V2: Terminal not found:', errorMsg);
          toastr.error(errorMsg, 'خطا', {
            timeOut: 5000
          });
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Error fetching terminal:', err);
        const errorMsg = err?.responseJSON?.Message || 
                        err?.responseJSON?.message || 
                        'خطا در دریافت اطلاعات ترمینال';
        toastr.error(errorMsg, 'خطا', {
          timeOut: 5000
        });
      });
  }
  
  /**
   * ✅ پردازش پرداخت POS از طریق دستگاه کارتخوان
   * این تابع دیگر استفاده نمی‌شود - منطق به PosPaymentClient منتقل شده است
   * اما برای backward compatibility نگه داشته شده است
   * @deprecated استفاده از PosPaymentClient.processPayment() به جای این تابع
   */
  function processPosPayment(receptionId, amountIRR, terminal) {
    console.warn('⚠️ V2: processPosPayment is deprecated - use PosPaymentClient.processPayment() instead');
    
    // Fallback: اگر PosPaymentClient موجود نبود، از AJAX استفاده کن
    if (!posPaymentClient) {
      console.warn('⚠️ V2: PosPaymentClient not available, falling back to AJAX');
      // TODO: می‌توانیم AJAX قدیمی را اینجا نگه داریم یا خطا بدهیم
      toastr.error('ماژول پرداخت POS آماده نیست. لطفاً صفحه را نوسازی کنید.', 'خطا');
      return;
    }
    
    // استفاده از PosPaymentClient
    const terminalId = terminal.terminalId || terminal.TerminalId;
    const ipAddress = terminal.ipAddress || terminal.IpAddress;
    
    if (terminalId && ipAddress) {
      posPaymentClient.processPayment(terminalId, amountIRR, ipAddress);
    } else {
      console.error('❌ V2: Missing terminal info:', terminal);
      if (posPaymentUI) {
        posPaymentUI.showError('اطلاعات ترمینال ناقص است', 'INVALID_TERMINAL');
      }
    }
  }
  
  /**
   * 🏥 MEDICAL: پاک کردن فرم و آماده‌سازی برای پذیرش بیمار بعدی
   * @param {boolean} [skipDeleteDraft=false] - اگر true باشد (دکمه «ادامه»)، Draft حذف نمی‌شود و بیمار به لیست مراجعه‌کنندگان می‌رود تا بعداً تسویه کند
   */
  async function resetForm(skipDeleteDraft) {
    try {
      console.log('🏥 V2: ===== شروع پاک کردن فرم =====', 'skipDeleteDraft:', !!skipDeleteDraft);
      
      // ✅ وقتی «ادامه (پذیرش بعدی)» زده شده، Draft را حذف نکن تا بیمار در لیست مراجعه‌کنندگان بماند
      if (skipDeleteDraft) {
        console.log('✅ V2: ادامه بدون تسویه - Draft حذف نمی‌شود، بیمار در لیست مراجعه‌کنندگان می‌ماند');
      }
      
      // ✅ 1. حذف Draft (اگر وجود دارد و skipDeleteDraft نباشد) - بدون بررسی isDraftNotFinalized
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
      
      if (!skipDeleteDraft && receptionId && receptionId > 0) {
        console.log('🏥 V2: حذف Draft قبل از Reset فرم - ReceptionId:', receptionId);
        
        // ✅ CRITICAL: بررسی اینکه آیا Reception قبلاً finalized شده است یا نه
        // اگر finalized شده است، Draft را حذف نکن (چون دیگر Draft نیست، Reception است)
        let isFinalized = false;
        if (window._receptionFinalized === true) {
          isFinalized = true;
          console.log('✅ V2: Reception قبلاً finalized شده است - Draft حذف نمی‌شود');
        }
        
        // ✅ بررسی اینکه آیا Draft در حال نهایی شدن است (اگر در حال نهایی شدن است، حذف نکن)
        // بررسی flag isDraftFinalizing از AutoDraftManager
        let isFinalizing = false;
        if (window.AutoDraftManager && !isFinalized) {
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
        
        // ✅ اگر finalized شده است، فقط Reset کنیم (بدون حذف Draft)
        if (isFinalized) {
          console.log('✅ V2: Reception finalized شده است - فقط Reset می‌کنیم (بدون حذف Draft)');
          // Unmark Draft as Finalizing برای پذیرش بعدی
          if (window.AutoDraftManager && typeof window.AutoDraftManager.unmarkDraftAsFinalizing === 'function') {
            window.AutoDraftManager.unmarkDraftAsFinalizing();
            console.log('✅ V2: Draft unmarked as finalizing (برای پذیرش بعدی)');
          }
          // ادامه می‌دهیم بدون حذف Draft
        } else if (isFinalizing) {
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

      // ✅ 9.1 بازگرداندن دکمه‌های پرداخت به حالت اول — بعد از ریست فقط «ذخیره پذیرش» نمایش داده شود، نه «پرداخت و نهایی‌سازی»
      $("#BtnSaveReception").removeClass('d-none');
      $("#BtnFinalizePOS").addClass('d-none');

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
        $("#Patient_NationalCode").focus();
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

  // ✅ Event Handler برای دکمه «ادامه (پذیرش بعدی)» — بیمار فعلی به لیست مراجعه‌کنندگان می‌رود، فرم ریست می‌شود
  $(document).on('click', '#BtnContinueWithoutPayment', function(e) {
    e.preventDefault();
    e.stopPropagation();
    console.log('🏥 V2: BtnContinueWithoutPayment (ادامه) clicked');
    if (typeof Swal === 'undefined' || typeof Swal.fire !== 'function') {
      if (confirm('بیمار فعلی به لیست مراجعه‌کنندگان اضافه می‌شود و می‌تواند بعداً تسویه کند. ادامه می‌دهید؟')) {
        resetForm(true).then(function() {
          if (typeof toastr !== 'undefined') { toastr.success('بیمار به لیست مراجعه‌کنندگان اضافه شد.', 'ادامه', { timeOut: 4000 }); }
        });
      }
      return;
    }
    Swal.fire({
      icon: 'question',
      title: 'ادامه (پذیرش بعدی)',
      html: '<p class="mb-2">بیمار فعلی به <strong>لیست مراجعه‌کنندگان</strong> اضافه می‌شود و می‌تواند بعداً تسویه کند.</p><p class="mb-0 text-muted">فرم برای پذیرش بیمار بعدی خالی می‌شود.</p>',
      showCancelButton: true,
      confirmButtonText: 'بله، ادامه',
      cancelButtonText: 'انصراف',
      confirmButtonColor: '#0d6efd',
      cancelButtonColor: '#6c757d',
      reverseButtons: true,
      focusCancel: false
    }).then(function(result) {
      if (result.isConfirmed) {
        resetForm(true).then(function() {
          if (typeof toastr !== 'undefined') {
            toastr.success('بیمار به لیست مراجعه‌کنندگان اضافه شد. می‌تواند از لیست پذیرش‌ها بعداً تسویه کند.', 'ادامه', { timeOut: 4000 });
          }
        });
      }
    });
  });

  // ✅ Event Handler برای دکمه Reset Form (پاک کردن فرم و حذف Draft — فقط وقتی پذیرش را می‌خواهید لغو کنید)
  $(document).on('click', '#BtnResetForm', function(e) {
    e.preventDefault();
    e.stopPropagation();
    console.log('🏥 V2: BtnResetForm clicked');
    if (typeof Swal === 'undefined' || typeof Swal.fire !== 'function') {
      if (confirm('آیا مطمئن هستید که می‌خواهید فرم را پاک کنید؟ پذیرش فعلی حذف می‌شود.')) {
        resetForm(false);
      }
      return;
    }
    Swal.fire({
      icon: 'warning',
      title: 'پاک کردن فرم',
      html: '<p class="mb-2">پذیرش فعلی <strong>حذف</strong> می‌شود و فرم برای پذیرش بیمار بعدی خالی می‌شود.</p><p class="mb-0 small text-muted">برای نگه داشتن بیمار در لیست و تسویه بعداً، از دکمه «ادامه (پذیرش بعدی)» استفاده کنید.</p>',
      showCancelButton: true,
      confirmButtonText: 'بله، پاک کن',
      cancelButtonText: 'انصراف',
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d',
      reverseButtons: true
    }).then(function(result) {
      if (result.isConfirmed) {
        resetForm(false);
      }
    });
  });
  
  /**
   * ✅ چاپ قبض پرداخت برای فیش پرینتر - Production-Grade
   * فرمت مناسب برای دستگاه‌های فیش پرینتر مثل SRP-330II (58mm)
   * با استفاده از Print Manager برای مدیریت حرفه‌ای
   * 
   * ⚠️ Fallback: اگر Print Manager موجود نباشد، از روش قدیمی استفاده می‌کند
   */
  function printPaymentReceipt(receptionId) {
    if (!receptionId) {
      console.error('❌ FRONTEND: ReceptionId برای چاپ قبض موجود نیست');
      toastr.error('شناسه پذیرش برای چاپ قبض موجود نیست', 'خطا', {
        timeOut: 5000,
        positionClass: 'toast-top-center',
        closeButton: true
      });
      return;
    }
    
    console.log('🖨️ FRONTEND: چاپ قبض پرداخت - ReceptionId:', receptionId);
    
    // ✅ استفاده از Print Manager (بهترین روش)
    if (window.PrintManager && typeof window.PrintManager.print === 'function') {
      var printUrl = `/ReceptionV2/PrintReceipt/${receptionId}?type=payment&printer=thermal`;
      console.log('🔗 FRONTEND: Print URL:', printUrl);
      
      window.PrintManager.print(printUrl)
        .then(function() {
          console.log('✅ FRONTEND: چاپ قبض پرداخت با موفقیت به صف اضافه شد');
        })
        .catch(function(err) {
          console.error('❌ FRONTEND: خطا در چاپ:', err);
          toastr.error(err.message || 'خطا در چاپ قبض پرداخت', 'خطا', {
            timeOut: 7000,
            positionClass: 'toast-top-center',
            closeButton: true
          });
        });
      return;
    }
    
    // ✅ Fallback: روش قدیمی (اگر Print Manager موجود نباشد)
    console.warn('⚠️ FRONTEND: PrintManager not available, using fallback method');
    var printUrl = `/ReceptionV2/PrintReceipt/${receptionId}?type=payment&printer=thermal`;
    
    try {
      var printWindow = window.open(printUrl, '_blank', 'width=400,height=600,menubar=no,toolbar=no,location=no,status=no,scrollbars=yes');
      
      if (printWindow) {
        var checkLoad = setInterval(function() {
          try {
            if (printWindow.document && printWindow.document.readyState === 'complete') {
              clearInterval(checkLoad);
              setTimeout(function() {
                try {
                  printWindow.focus();
                  printWindow.print();
                  console.log('✅ FRONTEND: چاپ قبض پرداخت شروع شد (fallback)');
                  setTimeout(function() {
                    try {
                      if (printWindow && !printWindow.closed) {
                        printWindow.close();
                      }
                    } catch (closeErr) {
                      console.warn('⚠️ FRONTEND: Cannot close print window:', closeErr);
                    }
                  }, 1000);
                } catch (printErr) {
                  console.error('❌ FRONTEND: خطا در چاپ:', printErr);
                  toastr.warning('لطفاً از منوی مرورگر برای چاپ استفاده کنید.', 'توجه', {
                    timeOut: 5000
                  });
                }
              }, 500);
            }
          } catch (err) {
            console.warn('⚠️ FRONTEND: Cannot check window state:', err);
          }
        }, 100);
        
        setTimeout(function() {
          clearInterval(checkLoad);
          try {
            if (printWindow && !printWindow.closed) {
              printWindow.focus();
              printWindow.print();
              console.log('✅ FRONTEND: چاپ قبض پرداخت شروع شد (timeout fallback)');
            }
          } catch (err) {
            console.warn('⚠️ FRONTEND: Cannot print after timeout:', err);
          }
        }, 5000);
        
        printWindow.onerror = function() {
          console.error('❌ FRONTEND: خطا در باز کردن پنجره چاپ');
          clearInterval(checkLoad);
          toastr.error('نمی‌توان پنجره چاپ را باز کرد. لطفاً popup blocker را غیرفعال کنید.', 'خطا', {
            timeOut: 7000,
            positionClass: 'toast-top-center',
            closeButton: true
          });
        };
      } else {
        console.error('❌ FRONTEND: window.open returned null - Popup blocker فعال است');
        toastr.error('نمی‌توان پنجره چاپ را باز کرد. لطفاً popup blocker را غیرفعال کنید.', 'خطا', {
          timeOut: 7000,
          positionClass: 'toast-top-center',
          closeButton: true
        });
      }
    } catch (ex) {
      console.error('❌ FRONTEND: Exception در printPaymentReceipt:', ex);
      toastr.error('خطا در چاپ قبض پرداخت: ' + (ex.message || 'خطای نامشخص'), 'خطا');
    }
  }
  
  /**
   * ✅ چاپ قبض بیمه تکمیلی برای فیش پرینتر - Production-Grade
   * با استفاده از Print Manager برای مدیریت حرفه‌ای
   * 
   * ⚠️ Fallback: اگر Print Manager موجود نباشد، از روش قدیمی استفاده می‌کند
   */
  function printInsuranceReceipt(receptionId) {
    if (!receptionId) {
      console.error('❌ FRONTEND: ReceptionId برای چاپ قبض بیمه تکمیلی موجود نیست');
      toastr.error('شناسه پذیرش برای چاپ قبض بیمه تکمیلی موجود نیست', 'خطا');
      return;
    }
    
    console.log('🖨️ FRONTEND: چاپ قبض بیمه تکمیلی - ReceptionId:', receptionId);
    
    // ✅ استفاده از Print Manager (بهترین روش)
    if (window.PrintManager && typeof window.PrintManager.print === 'function') {
      var printUrl = `/ReceptionV2/PrintInsurance/${receptionId}`;
      console.log('🔗 FRONTEND: Print URL:', printUrl);
      
      window.PrintManager.print(printUrl)
        .then(function() {
          console.log('✅ FRONTEND: چاپ قبض بیمه تکمیلی با موفقیت به صف اضافه شد');
        })
        .catch(function(err) {
          console.error('❌ FRONTEND: خطا در چاپ:', err);
          toastr.error(err.message || 'خطا در چاپ قبض بیمه تکمیلی', 'خطا', {
            timeOut: 5000,
            closeButton: true
          });
        });
      return;
    }
    
    // ✅ Fallback: روش قدیمی (اگر Print Manager موجود نباشد)
    console.warn('⚠️ FRONTEND: PrintManager not available, using fallback method');
    var printUrl = `/ReceptionV2/PrintInsurance/${receptionId}`;
    
    try {
      var printWindow = window.open(printUrl, '_blank', 'width=400,height=600,menubar=no,toolbar=no,location=no,status=no,scrollbars=yes');
      
      if (printWindow) {
        var checkLoad = setInterval(function() {
          try {
            if (printWindow.document && printWindow.document.readyState === 'complete') {
              clearInterval(checkLoad);
              setTimeout(function() {
                try {
                  printWindow.focus();
                  printWindow.print();
                  console.log('✅ FRONTEND: چاپ قبض بیمه تکمیلی شروع شد (fallback)');
                  setTimeout(function() {
                    try {
                      if (printWindow && !printWindow.closed) {
                        printWindow.close();
                      }
                    } catch (closeErr) {
                      console.warn('⚠️ FRONTEND: Cannot close print window:', closeErr);
                    }
                  }, 1000);
                } catch (printErr) {
                  console.error('❌ FRONTEND: خطا در چاپ:', printErr);
                  toastr.warning('لطفاً از منوی مرورگر برای چاپ استفاده کنید.', 'توجه', {
                    timeOut: 5000
                  });
                }
              }, 500);
            }
          } catch (err) {
            console.warn('⚠️ FRONTEND: Cannot check window state:', err);
          }
        }, 100);
        
        setTimeout(function() {
          clearInterval(checkLoad);
          try {
            if (printWindow && !printWindow.closed) {
              printWindow.focus();
              printWindow.print();
              console.log('✅ FRONTEND: چاپ قبض بیمه تکمیلی شروع شد (timeout fallback)');
            }
          } catch (err) {
            console.warn('⚠️ FRONTEND: Cannot print after timeout:', err);
          }
        }, 5000);
        
        printWindow.onerror = function() {
          console.error('❌ FRONTEND: خطا در باز کردن پنجره چاپ');
          clearInterval(checkLoad);
          toastr.error('نمی‌توان پنجره چاپ را باز کرد. لطفاً popup blocker را غیرفعال کنید.', 'خطا', {
            timeOut: 7000,
            positionClass: 'toast-top-center',
            closeButton: true
          });
        };
      } else {
        console.error('❌ FRONTEND: window.open returned null - Popup blocker فعال است');
        toastr.error('نمی‌توان پنجره چاپ را باز کرد. لطفاً popup blocker را غیرفعال کنید.', 'خطا', {
          timeOut: 7000,
          positionClass: 'toast-top-center',
          closeButton: true
        });
      }
    } catch (ex) {
      console.error('❌ FRONTEND: Exception در printInsuranceReceipt:', ex);
      toastr.error('خطا در چاپ قبض بیمه تکمیلی: ' + (ex.message || 'خطای نامشخص'), 'خطا');
    }
  }
  
  /**
   * ✅ بستن Modal پرداخت POS - Production-Grade
   * با Cleanup کامل و Reset فرم
   */
  function closePosPaymentModal() {
    console.log('🚪 FRONTEND: بستن Modal پرداخت POS');
    
    var modalElement = document.getElementById('posPaymentModal');
    if (modalElement) {
      // ✅ Bootstrap 5 API
      if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
        var modal = bootstrap.Modal.getInstance(modalElement);
        if (modal) {
          modal.hide();
          console.log('✅ FRONTEND: Modal بسته شد (Bootstrap 5)');
        }
      }
      // ✅ Fallback: Bootstrap 4 API (jQuery)
      else if ($ && $.fn.modal) {
        $(modalElement).modal('hide');
        console.log('✅ FRONTEND: Modal بسته شد (Bootstrap 4)');
      }
    }
    
    // ✅ Cleanup کامل: پاک کردن داده‌های پرداخت
    window.posPaymentData = null;
    window._currentReceptionIdForPrint = null;
    window._finalizingReceptionId = null;
    window._receptionFinalized = false; // ✅ Reset flag برای پذیرش بعدی
    currentReceptionId = null;
    currentAmountIRR = null;
    
    // ✅ Cleanup Event Handlers (با namespace برای جلوگیری از conflict)
    $('#posPaymentPrintBtn').off('click.print');
    $('#posPaymentPrintInsuranceBtn').off('click.print');
    $('#posPaymentConfirmBtn').off('click');
    
    // ✅ Reset Modal State
    if (posPaymentUI && typeof posPaymentUI.showReady === 'function') {
      posPaymentUI.showReady();
    }
    
    // ✅ CRITICAL: Reset فرم بعد از بستن Modal
    console.log('🔄 FRONTEND: Reset فرم بعد از بستن Modal...');
    
    // ✅ CRITICAL: اگر Reception finalized شده است، Unmark Draft as Finalizing
    // این باید قبل از resetForm() انجام شود
    if (window._receptionFinalized === true) {
      console.log('✅ FRONTEND: Reception finalized شده است - Unmarking Draft as Finalizing...');
      if (window.AutoDraftManager && typeof window.AutoDraftManager.unmarkDraftAsFinalizing === 'function') {
        window.AutoDraftManager.unmarkDraftAsFinalizing();
        console.log('✅ FRONTEND: Draft unmarked as finalizing (برای پذیرش بعدی)');
      }
    }
    
    if (typeof resetForm === 'function') {
      resetForm().then(function() {
        console.log('✅ FRONTEND: فرم با موفقیت Reset شد');
      }).catch(function(err) {
        console.error('❌ FRONTEND: خطا در Reset فرم:', err);
        // Fallback: Reset دستی
        if (window.FormDirty && window.FormDirty.clean) {
          window.FormDirty.clean();
        }
        if (window.AutoDraftManager && window.AutoDraftManager.reset) {
          window.AutoDraftManager.reset();
        }
        // Fallback: Reload صفحه
        setTimeout(function() {
          console.log('🔄 FRONTEND: Reload صفحه (Fallback)...');
          location.reload();
        }, 1000);
      });
    } else {
      // Fallback: Reset دستی
      console.log('⚠️ FRONTEND: resetForm function موجود نیست - استفاده از Fallback');
      
      // Reset form and auto-draft system
      if (window.FormDirty && window.FormDirty.clean) {
        window.FormDirty.clean();
      }
      if (window.AutoDraftManager && window.AutoDraftManager.reset) {
        window.AutoDraftManager.reset();
      }
      
      // ✅ Reload صفحه برای نمایش پذیرش جدید (بعد از تاخیر کوتاه)
      setTimeout(function() {
        console.log('🔄 FRONTEND: Reload صفحه...');
        location.reload();
      }, 500);
    }
  }
  
  // Export functions to global scope
  window.printPaymentReceipt = printPaymentReceipt;
  window.printInsuranceReceipt = printInsuranceReceipt;
  window.closePosPaymentModal = closePosPaymentModal;
  
})(window.ReceptionAPI, window.RxUtils);
