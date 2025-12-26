(function($, API, U) {
  'use strict';

  // References to form fields
  // نکته: در view فقط BasePlanId و SuppPlanId وجود دارند، نه baseInsurance و suppInsurance
  const $basePlan = $('#BasePlanId');
  const $suppPlan = $('#SuppPlanId');
  const $btnRemoveSupp = $('#btnRemoveSupp');
  const $btnSetInsurances = $('#BtnSetInsurances');

  // ❌ هیچ cache - همه چیز realtime برای محیط درمانی
  // فقط برای مقایسه تغییرات استفاده می‌شود (نه برای cache کردن داده‌ها)
  let lastState = {
    basePlanId: null,
    basePlanName: null,
    suppPlanId: null,
    suppPlanName: null
  };
  
  // ✅ CRITICAL: Lock Manager برای جلوگیری از درخواست‌های همزمان SetInsurances
  let isPersisting = false;
  let persistLock = false;
  let persistQueue = null;
  let persistDebounceTimeout = null;
  
  // 🚨 PROFESSIONAL: لیسنر برای تغییر ReceptionId (برای persist خودکار بیمه‌ها)
  // ✅ با Debounce و Lock برای جلوگیری از Concurrency Error
  $(document).on('receptionId:updated', function(e, receptionId) {
    console.log('🏥 V2: ReceptionId updated event received:', receptionId);
    if (receptionId && receptionId > 0) {
      // اگر بیمه‌ها قبلاً تنظیم شده‌اند، آن‌ها را persist کن
      const basePlanId = $('#BasePlanId').val();
      const suppPlanId = $('#SuppPlanId').val();
      if (basePlanId || suppPlanId) {
        console.log('🏥 V2: Auto-persisting insurances after ReceptionId update - BasePlanId:', basePlanId, 'SuppPlanId:', suppPlanId);
        
        // ✅ CRITICAL: Debounce برای جلوگیری از درخواست‌های همزمان
        // Clear previous timeout
        if (persistDebounceTimeout) {
          clearTimeout(persistDebounceTimeout);
          persistDebounceTimeout = null;
        }
        
        // ✅ تاخیر 300ms برای اطمینان از کامل شدن Draft creation
        persistDebounceTimeout = setTimeout(function() {
          persistDebounceTimeout = null;
          
          // ✅ بررسی Lock - اگر در حال persist است، صبر کن
          if (isPersisting || persistLock) {
            console.log('⏳ V2: SetInsurances در حال انجام است - در صف قرار می‌گیرد');
            persistQueue = function() {
              persist().catch(function(err) {
                console.warn('🏥 V2: Error auto-persisting insurances:', err);
              });
            };
            return;
          }
          
          // ✅ اجرای persist
          persist().catch(function(err) {
            console.warn('🏥 V2: Error auto-persisting insurances:', err);
          });
        }, 300); // 300ms delay برای اطمینان از کامل شدن Draft creation
      }
    }
  });
  
  // ✅ Race condition prevention
  let repriceTimeout = null;
  let isRepricing = false;

  /**
   * بارگذاری لیست پلن‌های بیمه
   */
  function loadPlans() {
    return API.get('/insurance/plans')
      .then(API.ok)
      .then(function(res) {
        console.log('🏥 V2: Insurance plans loaded:', res);
        
        // Fill base insurance plans
        $basePlan.empty().append('<option value="">انتخاب کنید</option>');
        if (res.basePlans) {
          res.basePlans.forEach(function(plan) {
            $basePlan.append(`<option value="${plan.insurancePlanId || plan.insuranceId}">${plan.name || plan.insuranceName} (${plan.coveragePercent || plan.coveragePercentage}%)</option>`);
          });
        }
        
        // Fill supplementary insurance plans
        $suppPlan.empty().append('<option value="">انتخاب کنید</option>');
        if (res.supplementaryPlans) {
          res.supplementaryPlans.forEach(function(plan) {
            $suppPlan.append(`<option value="${plan.insurancePlanId || plan.insuranceId}">${plan.name || plan.insuranceName} (${plan.coveragePercent || plan.coveragePercentage}%)</option>`);
          });
        }
        
        return res;
      })
      .catch(function(err) {
        console.error('🏥 V2: Insurance plans load error:', err);
        toastr.error('خطا در بارگذاری بیمه‌ها');
        throw err;
      });
  }

  /**
   * دریافت نام بیمه از dropdown بر اساس value
   */
  function getInsuranceName($select, planId) {
    if (!planId || planId === '' || planId === null) return null;
    const $option = $select.find(`option[value="${planId}"]`);
    return $option.length > 0 ? $option.text().trim() : null;
  }

  /**
   * ذخیره وضعیت فعلی بیمه‌ها (برای مقایسه تغییرات - نه cache)
   * ❌ هیچ cache - همه چیز realtime
   */
  function saveLastState() {
    const basePlanId = $basePlan.val();
    const suppPlanId = $suppPlan.val();
    
    lastState = {
      basePlanId: (basePlanId && basePlanId !== '') ? parseInt(basePlanId) : null,
      basePlanName: getInsuranceName($basePlan, basePlanId),
      suppPlanId: (suppPlanId && suppPlanId !== '') ? parseInt(suppPlanId) : null,
      suppPlanName: getInsuranceName($suppPlan, suppPlanId)
    };
    
    console.log('🏥 V2: Insurance last state saved:', lastState);
  }

  /**
   * تنظیم بیمه‌ها از DTO (از patient-lookup)
   * @param {Object} dto - InsuranceSelectionDto
   */
  function set(dto) {
    if (!dto) {
      console.warn('🏥 V2: Insurance DTO is null/undefined');
      return;
    }
    
    console.log('🏥 V2: Setting insurances from DTO:', dto);

    // ابتدا لیست پلن‌ها را لود کن، سپس مقدار را set کن
    loadPlans()
      .then(function(plansData) {
        console.log('🏥 V2: Insurance plans loaded, now setting values');
        
        // تنظیم پلن پایه (اولویت: BasePlanId، سپس SuggestedBasePlanId)
        let basePlanIdToSet = null;
        if (dto.BasePlanId) {
          basePlanIdToSet = dto.BasePlanId;
        } else if (dto.SuggestedBasePlanId) {
          basePlanIdToSet = dto.SuggestedBasePlanId;
        }
        
        if (basePlanIdToSet) {
          console.log('🏥 V2: Setting base plan ID:', basePlanIdToSet);
          // چک کن که آیا option با این value وجود دارد
          const basePlanExists = $basePlan.find(`option[value="${basePlanIdToSet}"]`).length > 0;
          if (basePlanExists) {
            // ✅ بدون trigger('change') برای جلوگیری از race condition - reprice در انتهای set() انجام می‌شود
            $basePlan.val(basePlanIdToSet);
            console.log('🏥 V2: Base plan set successfully');
          } else {
            console.warn('🏥 V2: Base plan ID not found in dropdown:', basePlanIdToSet);
            // حتی اگر در dropdown نیست، مقدار را set کن (شاید بعداً لود شود)
            $basePlan.val(basePlanIdToSet);
          }
        }
        
        // تنظیم پلن تکمیلی (اولویت: SupplementaryPlanId، سپس SuggestedSupplementaryPlanId)
        let suppPlanIdToSet = null;
        if (dto.SupplementaryPlanId) {
          suppPlanIdToSet = dto.SupplementaryPlanId;
        } else if (dto.SuggestedSupplementaryPlanId) {
          suppPlanIdToSet = dto.SuggestedSupplementaryPlanId;
        }
        
        if (suppPlanIdToSet) {
          console.log('🏥 V2: Setting supplementary plan ID:', suppPlanIdToSet);
          // چک کن که آیا option با این value وجود دارد
          const suppPlanExists = $suppPlan.find(`option[value="${suppPlanIdToSet}"]`).length > 0;
          if (suppPlanExists) {
            // ✅ بدون trigger('change') برای جلوگیری از race condition - reprice در انتهای set() انجام می‌شود
            $suppPlan.val(suppPlanIdToSet);
            toggleRemoveButton(); // Update button visibility
            console.log('🏥 V2: Supplementary plan set successfully');
          } else {
            console.warn('🏥 V2: Supplementary plan ID not found in dropdown:', suppPlanIdToSet);
            // حتی اگر در dropdown نیست، مقدار را set کن (شاید بعداً لود شود)
            $suppPlan.val(suppPlanIdToSet);
            toggleRemoveButton(); // Update button visibility
          }
        } else {
          console.log('🏥 V2: No supplementary plan to set');
          $suppPlan.val(''); // Clear if no value
          toggleRemoveButton(); // Hide button
        }

        // ذخیره وضعیت فعلی (برای مقایسه - نه cache)
        saveLastState();
        
        // به‌روزرسانی نمایش وضعیت در UI
        updateInsuranceStatus();

        // اگر پذیرش وجود دارد، بیمه‌ها را ذخیره کن با debounce
        const receptionId = $('#ReceptionId').val();
        if (receptionId && receptionId > 0) {
          console.log('🏥 V2: Reception ID exists, triggering persist with debounce');
          triggerReprice(); // استفاده از triggerReprice به جای persist مستقیم برای جلوگیری از race condition
        } else {
          console.log('🏥 V2: No reception ID yet, skipping persist');
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Error loading insurance plans for set operation:', err);
        toastr.warning('خطا در بارگذاری لیست بیمه‌ها');
      });
  }

  /**
   * ذخیره بیمه‌ها در سرور
   * ✅ گام 2 - Draft Orchestrator: استفاده از ensureDraftOrSkip
   * اگر ReceptionId وجود ندارد، ابتدا draft ایجاد می‌کند
   */
  async function persist() {
    // ✅ CRITICAL: Lock Manager - جلوگیری از درخواست‌های همزمان
    if (isPersisting || persistLock) {
      console.warn('⏳ V2: SetInsurances در حال انجام است - درخواست جدید در صف قرار می‌گیرد');
      // Queue this request
      persistQueue = function() {
        persist().catch(function(err) {
          console.warn('🏥 V2: Error auto-persisting insurances (queued):', err);
        });
      };
      return Promise.resolve();
    }
    
    // ✅ Set Lock
    isPersisting = true;
    persistLock = true;
    
    // ✅ Bugfix: بررسی وجود AutoDraftManager و ensureDraftOrSkip
    if (!window.AutoDraftManager) {
      console.error('🏥 V2: AutoDraftManager not available');
      toastr.error('سیستم پیش‌نویس در دسترس نیست. لطفاً صفحه را نوسازی کنید.');
      isPersisting = false;
      persistLock = false;
      return Promise.resolve();
    }
    
    if (typeof window.AutoDraftManager.ensureDraftOrSkip !== 'function') {
      console.error('🏥 V2: ensureDraftOrSkip is not a function', window.AutoDraftManager);
      toastr.error('خطا در سیستم پیش‌نویس. لطفاً صفحه را نوسازی کنید.');
      isPersisting = false;
      persistLock = false;
      return Promise.resolve();
    }
    
    // ✅ استفاده از async/await برای اطمینان از وجود Draft
    let receptionId;
    try {
      // ✅ بررسی اینکه آیا Draft creation در حال انجام است یا نه
      // بررسی از طریق draftCreationPromise (اگر موجود باشد)
      if (window.AutoDraftManager && window.AutoDraftManager.draftCreationPromise) {
        console.log('⏳ V2: Draft creation در حال انجام است - منتظر می‌مانیم...');
        try {
          // منتظر بمان تا Draft creation تمام شود
          await window.AutoDraftManager.draftCreationPromise;
          console.log('✅ V2: Draft creation تمام شد - ادامه persist...');
        } catch (err) {
          console.warn('⚠️ V2: Draft creation با خطا مواجه شد:', err);
          // ادامه می‌دهیم حتی اگر Draft creation با خطا مواجه شد
        }
      }
      
      receptionId = await window.AutoDraftManager.ensureDraftOrSkip({
        patientId: $('#Patient_PatientId').val(),
        clinicId: $('#ClinicId').val(),
        departmentId: $('#DepartmentId').val(),
        doctorId: $('#DoctorId').val(),
        receptionId: $('#ReceptionId').val()
      });
      
      if (!receptionId || receptionId <= 0) {
        console.warn('🏥 V2: Cannot persist insurances, draft creation failed or missing required fields');
        window.AutoDraftManager?.warnDraftMissing();
        isPersisting = false;
        persistLock = false;
        return Promise.resolve();
      }
    } catch (err) {
      console.error('🏥 V2: ensureDraftOrSkip error:', err);
      toastr.error('خطا در ایجاد پیش‌نویس. لطفاً مجدداً تلاش کنید.');
      isPersisting = false;
      persistLock = false;
      return Promise.resolve();
    }

    // دریافت مقادیر
    const basePlanValue = $basePlan.val();
    const suppPlanValue = $suppPlan.val();
    
    // تبدیل به integer یا null (اگر خالی باشد)
    const basePlanId = (basePlanValue && basePlanValue !== '' && basePlanValue !== null) 
      ? parseInt(basePlanValue) 
      : null;
    
    const supplementaryPlanId = (suppPlanValue && suppPlanValue !== '' && suppPlanValue !== null) 
      ? parseInt(suppPlanValue) 
      : null;

    const payload = {
      receptionId: parseInt(receptionId),
      basePlanId: basePlanId,
      supplementaryPlanId: supplementaryPlanId // اگر null باشد، یعنی بیمار بیمه تکمیلی ندارد
    };

    console.log('🏥 V2: Persisting insurances:', payload);
    console.log('🏥 V2: SupplementaryPlanId:', supplementaryPlanId === null ? 'NULL (No supplementary insurance)' : supplementaryPlanId);

    // ✅ استفاده از setInsurancesAndReprice برای Token-based race safety
    const repricePromise = window.ReceptionAPI && typeof window.ReceptionAPI.setInsurancesAndReprice === 'function'
      ? window.ReceptionAPI.setInsurancesAndReprice(payload)
      : API.post('/insurances/set', payload);

    return repricePromise
      .then(function(fullResponse) {
        // اگر token outdated بود، null برمی‌گرداند
        if (fullResponse === null) {
          console.warn('🏥 V2: Reprice response ignored (outdated)');
          return;
        }

        // Log کامل response برای دیباگ
        console.log('🏥 V2: Full SetInsurances API response:', fullResponse);
        
        // اگر response به صورت string است، آن را parse کن
        let responseObj = fullResponse;
        if (typeof fullResponse === 'string') {
          try {
            responseObj = JSON.parse(fullResponse);
            console.log('🏥 V2: SetInsurances response parsed from string:', responseObj);
          } catch (e) {
            console.error('🏥 V2: Failed to parse JSON response:', e);
            toastr.error('خطا در پردازش پاسخ سرور');
            return;
          }
        }
        
        // چک Success
        const successValue = responseObj?.Success ?? responseObj?.success;
        const isSuccess = successValue === true || successValue === "true" || successValue === 1;
        
        if (!responseObj || !isSuccess) {
          const errorMsg = responseObj?.Message || responseObj?.message || 'خطا در ثبت بیمه';
          const errorCode = responseObj?.Code || responseObj?.code || '';
          console.warn('🏥 V2: SetInsurances failed:', errorMsg, responseObj);
          
          // ✅ بهبود error handling برای Concurrency و سایر خطاها
          if (errorCode === 'CONCURRENCY_ERROR' || errorMsg.indexOf('جای دیگری تغییر کرده') > -1) {
            // خطای Concurrency - پیشنهاد refresh
            if (window.Swal && typeof window.Swal.fire === 'function') {
              window.Swal.fire({
                icon: 'warning',
                title: 'تغییر همزمان',
                html: '<div class="text-right">' +
                      '<p>' + errorMsg + '</p>' +
                      '<p class="mt-3"><strong>توصیه:</strong> صفحه را نوسازی کنید و مجدداً تلاش کنید.</p>' +
                      '</div>',
                confirmButtonText: 'نوسازی صفحه',
                showCancelButton: true,
                cancelButtonText: 'بستن',
                allowOutsideClick: false
              }).then(function(result) {
                if (result.isConfirmed) {
                  window.location.reload();
                }
              });
            } else {
              if (confirm(errorMsg + '\n\nآیا می‌خواهید صفحه را نوسازی کنید؟')) {
                window.location.reload();
              }
            }
          } else {
            // سایر خطاها
            toastr.warning(errorMsg, 'خطا در ثبت بیمه', {
              timeOut: 5000,
              closeButton: true
            });
          }
          return;
        }

        // دریافت Data
        const response = API.ok(responseObj);
        console.log('🏥 V2: Insurances persisted successfully:', response);
        
        // ✅ به‌روزرسانی state.insurances برای Coverage Modal
        const basePlanName = getInsuranceName($basePlan, basePlanId);
        const suppPlanName = getInsuranceName($suppPlan, supplementaryPlanId);
        
        if (window.ClinicApp && window.ClinicApp.ReceptionV2 && window.ClinicApp.ReceptionV2.state) {
          window.ClinicApp.ReceptionV2.state.insurances = {
            BasePlanId: basePlanId,
            BasePlanName: basePlanName,
            SupplementaryPlanId: supplementaryPlanId,
            SupplementaryPlanName: suppPlanName
          };
          console.log('🏥 V2: state.insurances updated:', window.ClinicApp.ReceptionV2.state.insurances);
          
          // Trigger state change event
          $(document).trigger('rv2:stateChanged', [{ insurances: window.ClinicApp.ReceptionV2.state.insurances }]);
        }
        
        // ✅ گام 3.3: به‌روزرسانی همه ردیف‌ها با pricings
        const pricings = response.pricings || response.Pricings || responseObj?.Data?.pricings || responseObj?.Data?.Pricings || [];
        if (pricings && Array.isArray(pricings) && pricings.length > 0) {
          console.log('🏥 V2: Updating all rows with pricings:', pricings.length);
          pricings.forEach(function(p) {
            const itemId = p.ReceptionItemId || p.receptionItemId;
            if (itemId) {
              updateRowPricing(itemId, p);
            }
          });
        }
        
        // ✅ به‌روزرسانی Totals از پاسخ API (اگر موجود باشد)
        const totals = response.totals || response.Totals || responseObj?.Data?.totals || responseObj?.Data?.Totals;
        if (totals) {
          console.log('🏥 V2: Totals received in SetInsurances response:', totals);
          updateTotalsUI(totals);
        } else if (receptionId) {
          // Fallback: اگر totals در پاسخ نیست، از API جداگانه دریافت کن
          console.log('🏥 V2: Totals not in response, fetching separately...');
          loadTotals(receptionId).catch(function(err) {
            console.warn('🏥 V2: Error loading totals after SetInsurances:', err);
          });
        }
        
        // 🎯 نمایش پیغام موفقیت با جزئیات برای منشی
        const currentBasePlanId = parseInt($basePlan.val()) || null;
        const currentSuppPlanId = parseInt($suppPlan.val()) || null;
        const currentBasePlanName = getInsuranceName($basePlan, currentBasePlanId);
        const currentSuppPlanName = getInsuranceName($suppPlan, currentSuppPlanId);
        
        // تشخیص تغییرات (مقایسه با lastState - نه cache)
        let changes = [];
        if (lastState.basePlanId !== currentBasePlanId) {
          if (lastState.basePlanId && currentBasePlanId) {
            changes.push(`بیمه پایه: "${lastState.basePlanName}" → "${currentBasePlanName}"`);
          } else if (currentBasePlanId) {
            changes.push(`بیمه پایه: "${currentBasePlanName}" انتخاب شد`);
          } else {
            changes.push('بیمه پایه حذف شد');
          }
        }
        
        if (lastState.suppPlanId !== currentSuppPlanId) {
          if (lastState.suppPlanId && currentSuppPlanId) {
            changes.push(`بیمه تکمیلی: "${lastState.suppPlanName}" → "${currentSuppPlanName}"`);
          } else if (currentSuppPlanId) {
            changes.push(`بیمه تکمیلی: "${currentSuppPlanName}" انتخاب شد`);
          } else {
            changes.push('بیمه تکمیلی حذف شد. بیمار بیمه تکمیلی ندارد.');
          }
        }
        
        // نمایش پیغام موفقیت
        if (changes.length > 0) {
          // اگر تغییرات وجود داشت، پیغام تفصیلی نمایش بده
          let message = '✅ بیمه‌ها با موفقیت به‌روزرسانی شد:\n\n';
          changes.forEach(function(change) {
            message += '• ' + change + '\n';
          });
          message += '\n💡 وضعیت فعلی:\n';
          message += '• بیمه پایه: ' + (currentBasePlanName || '—') + '\n';
          message += '• بیمه تکمیلی: ' + (currentSuppPlanName || 'ندارد');
          
          toastr.success(message, 'بیمه‌ها به‌روزرسانی شد', {
            timeOut: 8000, // 8 ثانیه نمایش بده
            extendedTimeOut: 5000,
            closeButton: true,
            progressBar: true
          });
        } else {
          // اگر تغییری نبود، فقط پیغام ساده
          toastr.success('بیمه‌ها ذخیره شدند. تغییر جدیدی انجام نشد.');
        }
        
        // به‌روزرسانی lastState با مقادیر فعلی (برای مقایسه - نه cache)
        saveLastState();
        
        // به‌روزرسانی نمایش وضعیت در UI
        updateInsuranceStatus();
        
        // ✅ Trigger state change event for Summary Header (با totals اگر موجود باشد)
        $(document).trigger('rv2:stateChanged', {
          insurances: {
            BasePlanId: currentBasePlanId,
            BasePlanName: currentBasePlanName,
            SupplementaryPlanId: currentSuppPlanId,
            SupplementaryPlanName: currentSuppPlanName
          }
        });
        
        // ✅ Totals قبلاً با updateTotalsUI به‌روزرسانی شده است - نیازی به کد duplicate نیست
        
        // ✅ Release Lock و بررسی Queue
        isPersisting = false;
        persistLock = false;
        
        // ✅ اگر درخواستی در صف است، آن را اجرا کن
        if (persistQueue && typeof persistQueue === 'function') {
          console.log('🔄 V2: اجرای درخواست SetInsurances از صف...');
          const queuedRequest = persistQueue;
          persistQueue = null;
          // تاخیر کوتاه قبل از اجرای درخواست بعدی
          setTimeout(function() {
            queuedRequest();
          }, 200);
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Persist insurances error:', err);
        
        // ✅ Release Lock حتی در صورت خطا
        isPersisting = false;
        persistLock = false;
        
        // ✅ بررسی Queue حتی در صورت خطا
        if (persistQueue && typeof persistQueue === 'function') {
          console.log('🔄 V2: اجرای درخواست SetInsurances از صف (بعد از خطا)...');
          const queuedRequest = persistQueue;
          persistQueue = null;
          // تاخیر بیشتر در صورت خطا
          setTimeout(function() {
            queuedRequest();
          }, 500);
        }
        
        // ✅ بررسی Concurrency Error
        const errorMsg = err?.responseJSON?.Message || err?.responseJSON?.message || err?.message || 'خطا در ذخیره بیمه‌ها';
        if (errorMsg.indexOf('جای دیگری تغییر کرده') > -1 || err?.responseJSON?.Code === 'CONCURRENCY_ERROR') {
          // خطای Concurrency - فقط warning نمایش بده (نه error)
          toastr.warning('⚠️ ' + errorMsg, 'تغییر همزمان', {
            timeOut: 5000,
            closeButton: true
          });
        } else {
          toastr.error('خطا در ذخیره بیمه‌ها');
        }
      });
  }

  /**
   * حذف بیمه تکمیلی
   * این تابع زمانی صدا زده می‌شود که کاربر روی دکمه ❌ کلیک می‌کند
   * باعث می‌شود که فیلد بیمه تکمیلی خالی شود → بیمار بیمه تکمیلی ندارد
   */
  function removeSupplementary() {
    console.log('🏥 V2: Removing supplementary insurance → Patient will have NO supplementary insurance');
    
    // پاک کردن dropdown (انتخاب "انتخاب کنید")
    $suppPlan.val('').trigger('change'); // trigger change برای persist خودکار
    
    // مخفی کردن دکمه (چون حالا بیمه تکمیلی نداریم)
    toggleRemoveButton();
    
    // پیام موفقیت - مشخص کردن که بیمار بیمه تکمیلی ندارد
    toastr.info('بیمه تکمیلی حذف شد. بیمار بیمه تکمیلی ندارد.');
  }

  /**
   * نمایش/مخفی کردن دکمه حذف بیمه تکمیلی
   */
  function toggleRemoveButton() {
    const hasValue = $suppPlan.val() && $suppPlan.val() !== '';
    if ($btnRemoveSupp.length) {
      if (hasValue) {
        $btnRemoveSupp.show();
      } else {
        $btnRemoveSupp.hide();
      }
    }
  }

  /**
   * به‌روزرسانی نمایش وضعیت فعلی بیمه‌ها در UI (برای منشی)
   */
  function updateInsuranceStatus() {
    const basePlanId = $basePlan.val();
    const suppPlanId = $suppPlan.val();
    
    const basePlanName = getInsuranceName($basePlan, basePlanId);
    const suppPlanName = getInsuranceName($suppPlan, suppPlanId);
    
    // به‌روزرسانی badge‌های وضعیت
    const $baseBadge = $('#current-base-insurance');
    const $suppBadge = $('#current-supp-insurance');
    
    if ($baseBadge.length) {
      if (basePlanName) {
        $baseBadge.text('پایه: ' + basePlanName).removeClass('bg-secondary').addClass('bg-info');
      } else {
        $baseBadge.text('پایه: —').removeClass('bg-info').addClass('bg-secondary');
      }
    }
    
    if ($suppBadge.length) {
      if (suppPlanName) {
        $suppBadge.text('تکمیلی: ' + suppPlanName).removeClass('bg-secondary').addClass('bg-success');
      } else {
        $suppBadge.text('تکمیلی: ندارد').removeClass('bg-success').addClass('bg-secondary');
      }
    }
    
    console.log('🏥 V2: Insurance status updated in UI:', {
      base: basePlanName || '—',
      supplementary: suppPlanName || 'ندارد'
    });
  }

  /**
   * Trigger Reprice با debounce و race condition prevention
   * ✅ رویکرد حرفه‌ای: debounce 500ms + isRepricing flag
   * ❌ هیچ cache - همیشه realtime
   */
  function triggerReprice() {
    // اگر در حال reprice هستیم، skip کن
    if (isRepricing) {
      console.warn('🏥 V2: Reprice already in progress, skipping...');
      return;
    }
    
    // Clear timeout قبلی
    if (repriceTimeout) {
      clearTimeout(repriceTimeout);
      repriceTimeout = null;
    }
    
    // Debounce: 500ms delay
    repriceTimeout = setTimeout(function() {
      repriceTimeout = null;
      performReprice();
    }, 500);
  }
  
  /**
   * انجام Reprice با loading state
   */
  async function performReprice() {
    if (isRepricing) {
      return;
    }
    
    isRepricing = true;
    
    try {
      // به‌روزرسانی نمایش وضعیت در UI (قبل از persist)
      updateInsuranceStatus();
      
      // انجام persist (که خودش reprice می‌کند)
      await persist();
    } catch (err) {
      console.error('🏥 V2: Reprice error:', err);
      toastr.error('خطا در بازمحاسبه قیمت‌ها');
    } finally {
      isRepricing = false;
    }
  }
  
  // ✅ Event handlers - هوشمندسازی: تغییر بیمه → Reprice با debounce
  $basePlan.on('change', function() {
    console.log('🏥 V2: Base plan changed - Triggering smart recalculation...');
    triggerReprice();
  });
  
  $suppPlan.on('change', function() {
    const selectedValue = $suppPlan.val();
    console.log('🏥 V2: Supplementary plan changed, selected value:', selectedValue, '- Triggering smart recalculation...');
    
    // نمایش/مخفی کردن دکمه حذف
    toggleRemoveButton();
    
    if (!selectedValue || selectedValue === '' || selectedValue === null) {
      console.log('🏥 V2: Supplementary plan cleared → Patient has NO supplementary insurance');
    } else {
      console.log('🏥 V2: Supplementary plan selected:', selectedValue, '- Persisting...');
    }
    
    triggerReprice();
  });

  if ($btnRemoveSupp.length) {
    $btnRemoveSupp.on('click', removeSupplementary);
  }

  // Manual set button (if exists)
  if ($btnSetInsurances.length) {
    $btnSetInsurances.on('click', function() {
      const receptionId = $('#ReceptionId').val();
      if (!receptionId || receptionId <= 0) {
        // Try to create auto-draft first
        if (window.AutoDraftManager && !window.AutoDraftManager.isDraftCreated()) {
          window.AutoDraftManager.createDraft().then(function(draftId) {
            if (draftId) {
              $('#ReceptionId').val(draftId);
              triggerReprice(); // با debounce برای جلوگیری از race condition
            } else {
              toastr.warning('لطفاً ابتدا پذیرش را ایجاد کنید');
            }
          }).catch(function(err) {
            console.error('🏥 V2: Auto-draft creation error:', err);
            toastr.warning('لطفاً ابتدا پذیرش را ایجاد کنید');
          });
          return;
        } else {
          toastr.warning('لطفاً ابتدا پذیرش را ایجاد کنید');
          return;
        }
      }
      
      triggerReprice(); // با debounce برای جلوگیری از race condition
    });
  }

  /**
   * ✅ به‌روزرسانی Totals در UI
   * پشتیبانی از ReceptionTotalsDto (GrossIRR, BaseCoveredIRR, SuppCoveredIRR, PatientPayableIRR)
   */
  function updateTotalsUI(totals) {
    if (!totals) {
      console.warn('🏥 V2: updateTotalsUI called with null/undefined totals');
      return;
    }
    
    console.log('🏥 V2: Updating totals UI:', totals);
    
    // ✅ پشتیبانی از PascalCase و camelCase + Friendly strings
    const gross = totals.GrossIRR || totals.grossIRR || totals.Gross || totals.gross || 0;
    const base = totals.BaseCoveredIRR || totals.baseCoveredIRR || totals.Base || totals.base || 0;
    const supp = totals.SuppCoveredIRR || totals.suppCoveredIRR || totals.Supplementary || totals.supplementary || 0;
    const patient = totals.PatientPayableIRR || totals.patientPayableIRR || totals.Patient || totals.patient || 0;
    
    // ✅ استفاده از Friendly strings اگر موجود باشند
    const grossStr = totals.GrossIRRStr || totals.grossIRRStr || (gross ? U.toIRR(gross) : '۰');
    const baseStr = totals.BaseCoveredIRRStr || totals.baseCoveredIRRStr || (base ? U.toIRR(base) : '۰');
    const suppStr = totals.SuppCoveredIRRStr || totals.suppCoveredIRRStr || (supp ? U.toIRR(supp) : '۰');
    const patientStr = totals.PatientPayableIRRStr || totals.patientPayableIRRStr || (patient ? U.toIRR(patient) : '۰');
    
    // ✅ به‌روزرسانی UI - پشتیبانی از هر دو مجموعه selector
    $('#Gross, #SumGross').text(grossStr).attr('data-value', gross);
    $('#InsurancePayable, #SumBase').text(baseStr).attr('data-value', base);
    $('#SuppPayable, #SumSupp').text(suppStr).attr('data-value', supp);
    $('#PatientPayable, #SumPatient').text(patientStr).attr('data-value', patient);
    
    console.log('✅ V2: Totals UI updated - Gross:', grossStr, 'Base:', baseStr, 'Supp:', suppStr, 'Patient:', patientStr);
  }
  
  /**
   * ✅ دریافت Totals از API (fallback)
   */
  async function loadTotals(receptionId) {
    if (!receptionId || receptionId <= 0) {
      console.warn('🏥 V2: Cannot load totals - invalid receptionId:', receptionId);
      return Promise.resolve();
    }
    
    try {
      const fullResponse = await API.get('/totals', { receptionId: receptionId });
      console.log('🏥 V2: LoadTotals raw response:', fullResponse);
      
      const successValue = fullResponse?.Success ?? fullResponse?.success;
      const isSuccess = successValue === true || successValue === "true" || successValue === 1;
      
      if (!fullResponse || !isSuccess) {
        const errorMsg = fullResponse?.Message || fullResponse?.message || 'خطا در دریافت جمع‌ها';
        console.warn('🏥 V2: LoadTotals failed:', errorMsg);
        return Promise.resolve();
      }
      
      const response = API.ok(fullResponse);
      const totals = response.totals || response.Totals || response;
      
      if (totals) {
        updateTotalsUI(totals);
      }
    } catch (err) {
      console.error('🏥 V2: Error loading totals:', err);
      // Silent fail - don't show error to user as it's a background operation
    }
  }
  
  // Export برای patient-lookup.js
  window.insPanel = {
    set: set,
    persist: persist,
    loadPlans: loadPlans
  };
  
  // ✅ Export برای استفاده در ماژول‌های دیگر
  window.insurancePanelModule = {
    loadPlans: loadPlans,
    persist: persist,
    updateInsuranceStatus: updateInsuranceStatus,
    updateTotalsUI: updateTotalsUI,
    loadTotals: loadTotals
  };

  // Initialization: لود لیست‌ها
  $(document).ready(function() {
    // بارگذاری لیست بیمه‌ها
    loadPlans()
      .then(function() {
        // پس از لود شدن بیمه‌ها، وضعیت فعلی را ذخیره کن (برای مقایسه - نه cache)
        saveLastState();
        console.log('🏥 V2: Initial insurance state saved:', lastState);
        
        // به‌روزرسانی نمایش وضعیت در UI
        updateInsuranceStatus();
      })
      .catch(function(err) {
        console.warn('🏥 V2: Failed to load insurance plans on init:', err);
      });
    
    // نمایش/مخفی کردن دکمه حذف بیمه تکمیلی بر اساس مقدار فعلی
    toggleRemoveButton();
    
    // به‌روزرسانی نمایش وضعیت در UI (اگر بیمه‌ها از قبل انتخاب شده باشند)
    setTimeout(function() {
      updateInsuranceStatus();
    }, 500); // کمی تأخیر برای اطمینان از لود شدن dropdown‌ها
  });

})(jQuery, window.ReceptionAPI, window.RxUtils);
