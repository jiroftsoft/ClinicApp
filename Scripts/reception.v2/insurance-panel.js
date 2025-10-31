(function($, API, U) {
  'use strict';

  // References to form fields
  // نکته: در view فقط BasePlanId و SuppPlanId وجود دارند، نه baseInsurance و suppInsurance
  const $basePlan = $('#BasePlanId');
  const $suppPlan = $('#SuppPlanId');
  const $btnRemoveSupp = $('#btnRemoveSupp');
  const $btnSetInsurances = $('#BtnSetInsurances');

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
          }
        } else {
          console.log('🏥 V2: No supplementary plan to set');
        }

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
   */
  function persist() {
    const receptionId = $('#ReceptionId').val();
    if (!receptionId || receptionId <= 0) {
      console.warn('🏥 V2: Cannot persist insurances, no reception ID');
      return Promise.resolve();
    }

    const payload = {
      receptionId: parseInt(receptionId),
      basePlanId: parseInt($basePlan.val()) || null,
      supplementaryPlanId: parseInt($suppPlan.val()) || null
    };

    console.log('🏥 V2: Persisting insurances:', payload);

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
   */
  function removeSupplementary() {
    $suppPlan.val('');
    
    const receptionId = $('#ReceptionId').val();
    if (receptionId && receptionId > 0) {
      persist();
    }
    
    toastr.info('بیمه تکمیلی حذف شد');
  }

  // Event handlers
  $basePlan.on('change', function() {
    console.log('🏥 V2: Base plan changed, persisting');
    persist();
  });
  
  $suppPlan.on('change', function() {
    console.log('🏥 V2: Supplementary plan changed, persisting');
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
    loadPlans().catch(function(err) {
      console.warn('🏥 V2: Failed to load insurance plans on init:', err);
    });
  });

})(jQuery, window.ReceptionAPI, window.RxUtils);
