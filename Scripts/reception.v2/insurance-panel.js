(function($, API, U) {
  'use strict';

  // References to form fields
  // نکته: در view فقط BasePlanId و SuppPlanId وجود دارند، نه baseInsurance و suppInsurance
  const $basePlan = $('#BasePlanId');
  const $suppPlan = $('#SuppPlanId');
  const $btnRemoveSupp = $('#btnRemoveSupp');
  const $btnSetInsurances = $('#BtnSetInsurances');

  // Cache برای ذخیره وضعیت قبلی بیمه‌ها (مثل patient-lookup)
  let cache = {
    basePlanId: null,
    basePlanName: null,
    suppPlanId: null,
    suppPlanName: null
  };

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
   * ذخیره وضعیت فعلی بیمه‌ها در cache
   */
  function saveToCache() {
    const basePlanId = $basePlan.val();
    const suppPlanId = $suppPlan.val();
    
    cache = {
      basePlanId: (basePlanId && basePlanId !== '') ? parseInt(basePlanId) : null,
      basePlanName: getInsuranceName($basePlan, basePlanId),
      suppPlanId: (suppPlanId && suppPlanId !== '') ? parseInt(suppPlanId) : null,
      suppPlanName: getInsuranceName($suppPlan, suppPlanId)
    };
    
    console.log('🏥 V2: Insurance state cached:', cache);
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
            $basePlan.val(basePlanIdToSet).trigger('change');
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
            $suppPlan.val(suppPlanIdToSet).trigger('change');
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

        // ذخیره وضعیت فعلی در cache
        saveToCache();
        
        // به‌روزرسانی نمایش وضعیت در UI
        updateInsuranceStatus();

        // اگر پذیرش وجود دارد، بیمه‌ها را ذخیره کن
        const receptionId = $('#ReceptionId').val();
        if (receptionId && receptionId > 0) {
          console.log('🏥 V2: Reception ID exists, persisting insurances');
          persist();
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
    // ✅ Bugfix: بررسی وجود AutoDraftManager و ensureDraftOrSkip
    if (!window.AutoDraftManager) {
      console.error('🏥 V2: AutoDraftManager not available');
      toastr.error('سیستم پیش‌نویس در دسترس نیست. لطفاً صفحه را نوسازی کنید.');
      return Promise.resolve();
    }
    
    if (typeof window.AutoDraftManager.ensureDraftOrSkip !== 'function') {
      console.error('🏥 V2: ensureDraftOrSkip is not a function', window.AutoDraftManager);
      toastr.error('خطا در سیستم پیش‌نویس. لطفاً صفحه را نوسازی کنید.');
      return Promise.resolve();
    }
    
    // ✅ استفاده از ensureDraftOrSkip برای اطمینان از وجود Draft
    let receptionId;
    try {
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
        return Promise.resolve();
      }
    } catch (err) {
      console.error('🏥 V2: ensureDraftOrSkip error:', err);
      toastr.error('خطا در ایجاد پیش‌نویس. لطفاً مجدداً تلاش کنید.');
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

    return API.post('/insurances/set', payload)
      .then(function(fullResponse) {
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
          console.warn('🏥 V2: SetInsurances failed:', errorMsg, responseObj);
          toastr.warning(errorMsg);
          return;
        }

        // دریافت Data
        const response = API.ok(responseObj);
        console.log('🏥 V2: Insurances persisted successfully:', response);
        
        // ✅ به‌روزرسانی Totals از پاسخ API (اگر موجود باشد)
        if (response && response.totals) {
          console.log('🏥 V2: Totals received in SetInsurances response:', response.totals);
          updateTotalsUI(response.totals);
        } else if (response && response.Data && response.Data.totals) {
          console.log('🏥 V2: Totals received in SetInsurances response.Data:', response.Data.totals);
          updateTotalsUI(response.Data.totals);
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
        
        // تشخیص تغییرات
        let changes = [];
        if (cache.basePlanId !== currentBasePlanId) {
          if (cache.basePlanId && currentBasePlanId) {
            changes.push(`بیمه پایه: "${cache.basePlanName}" → "${currentBasePlanName}"`);
          } else if (currentBasePlanId) {
            changes.push(`بیمه پایه: "${currentBasePlanName}" انتخاب شد`);
          } else {
            changes.push('بیمه پایه حذف شد');
          }
        }
        
        if (cache.suppPlanId !== currentSuppPlanId) {
          if (cache.suppPlanId && currentSuppPlanId) {
            changes.push(`بیمه تکمیلی: "${cache.suppPlanName}" → "${currentSuppPlanName}"`);
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
        
        // به‌روزرسانی cache با مقادیر فعلی
        saveToCache();
        
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
      })
      .catch(function(err) {
        console.error('🏥 V2: Persist insurances error:', err);
        toastr.error('خطا در ذخیره بیمه‌ها');
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

  // Event handlers
  $basePlan.on('change', function() {
    console.log('🏥 V2: Base plan changed');
    
    // به‌روزرسانی نمایش وضعیت در UI (قبل از persist)
    updateInsuranceStatus();
    
    // persist() اجرا می‌شود و در آن cache به‌روزرسانی می‌شود و پیغام نمایش داده می‌شود
    persist();
  });
  
  $suppPlan.on('change', function() {
    const selectedValue = $suppPlan.val();
    console.log('🏥 V2: Supplementary plan changed, selected value:', selectedValue);
    
    // نمایش/مخفی کردن دکمه حذف
    toggleRemoveButton();
    
    // به‌روزرسانی نمایش وضعیت در UI (قبل از persist)
    updateInsuranceStatus();
    
    // persist() اجرا می‌شود و در آن cache به‌روزرسانی می‌شود و پیغام نمایش داده می‌شود
    if (!selectedValue || selectedValue === '' || selectedValue === null) {
      console.log('🏥 V2: Supplementary plan cleared → Patient has NO supplementary insurance');
    } else {
      console.log('🏥 V2: Supplementary plan selected:', selectedValue, '- Persisting...');
    }
    
    persist();
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
              persist();
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
      
      persist();
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
    
    // ✅ به‌روزرسانی UI
    $('#Gross').text(grossStr).attr('data-value', gross);
    $('#InsurancePayable').text(baseStr).attr('data-value', base);
    $('#SuppPayable').text(suppStr).attr('data-value', supp);
    $('#PatientPayable').text(patientStr).attr('data-value', patient);
    
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
        // پس از لود شدن بیمه‌ها، وضعیت فعلی را در cache ذخیره کن
        saveToCache();
        console.log('🏥 V2: Initial insurance state cached:', cache);
        
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
