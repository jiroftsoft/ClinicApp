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
   * اگر ReceptionId وجود ندارد، ابتدا draft ایجاد می‌کند
   */
  function persist() {
    const receptionId = $('#ReceptionId').val();
    if (!receptionId || receptionId <= 0) {
      console.log('🏥 V2: No reception ID, attempting to create draft first...');
      
      // سعی کن draft ایجاد کن
      if (window.AutoDraftManager && !window.AutoDraftManager.isDraftCreated()) {
        return window.AutoDraftManager.createDraft()
          .then(function(draftId) {
            if (draftId) {
              console.log('🏥 V2: Draft created successfully:', draftId);
              $('#ReceptionId').val(draftId);
              // حالا که draft ایجاد شد، persist را دوباره صدا بزن
              return persist();
            } else {
              console.warn('🏥 V2: Draft creation returned null - missing required fields (patient/clinic/department/doctor)');
              toastr.warning('لطفاً ابتدا بیمار، کلینیک، دپارتمان و پزشک را انتخاب کنید');
              return Promise.resolve();
            }
          })
          .catch(function(err) {
            console.error('🏥 V2: Auto-draft creation error:', err);
            toastr.warning('برای ثبت بیمه، ابتدا پذیرش را ایجاد کنید (بیمار، کلینیک، دپارتمان، پزشک)');
            return Promise.resolve();
          });
      } else {
        console.warn('🏥 V2: Cannot persist insurances, no reception ID and AutoDraftManager unavailable');
        toastr.warning('برای ثبت بیمه، ابتدا پذیرش را ایجاد کنید (بیمار، کلینیک، دپارتمان، پزشک)');
        return Promise.resolve();
      }
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
        
        // Update totals if provided
        if (response && (response.totals || (response.Data && response.Data.totals))) {
          const totals = response.totals || response.Data?.totals;
          if (totals) {
            $('#Gross').text(U.toIRR(totals.gross || 0));
            $('#InsurancePayable').text(U.toIRR(totals.base || 0));
            $('#SuppPayable').text(U.toIRR(totals.supplementary || 0));
            $('#PatientPayable').text(U.toIRR(totals.patient || 0)).attr('data-value', totals.patient || 0);
            console.log('🏥 V2: Totals updated:', totals);
          }
        } else {
          console.log('🏥 V2: No totals in response, skipping totals update');
        }
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

  // Export برای patient-lookup.js
  window.insPanel = {
    set: set,
    persist: persist,
    loadPlans: loadPlans
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
