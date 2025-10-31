(function($, API, U) {
  'use strict';

  // References to form fields
  const $baseIns = $('#baseInsurance');
  const $basePlan = $('#BasePlanId');
  const $suppIns = $('#suppInsurance');
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
    if (!dto) return;
    
    console.log('🏥 V2: Setting insurances from DTO:', dto);

    // تنظیم بیمه پایه
    if (dto.BaseInsuranceId) {
      $baseIns.val(dto.BaseInsuranceId);
    }
    
    // تنظیم پلن پایه (اولویت: BasePlanId، سپس SuggestedBasePlanId)
    if (dto.BasePlanId) {
      $basePlan.val(dto.BasePlanId);
    } else if (dto.SuggestedBasePlanId) {
      $basePlan.val(dto.SuggestedBasePlanId);
    }

    // تنظیم بیمه تکمیلی
    if (dto.SupplementaryInsuranceId) {
      $suppIns.val(dto.SupplementaryInsuranceId);
    }
    
    // تنظیم پلن تکمیلی (اولویت: SupplementaryPlanId، سپس SuggestedSupplementaryPlanId)
    if (dto.SupplementaryPlanId) {
      $suppPlan.val(dto.SupplementaryPlanId);
    } else if (dto.SuggestedSupplementaryPlanId) {
      $suppPlan.val(dto.SuggestedSupplementaryPlanId);
    }

    // اگر پذیرش وجود دارد، بیمه‌ها را ذخیره کن
    const receptionId = $('#ReceptionId').val();
    if (receptionId && receptionId > 0) {
      persist();
    }
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
      .then(API.ok)
      .then(function(response) {
        if (!response || !response.success) {
          toastr.warning(response?.message || 'خطا در ثبت بیمه');
          return;
        }

        console.log('🏥 V2: Insurances persisted successfully');
        
        // Update totals if provided
        if (response.data && response.data.totals) {
          const totals = response.data.totals;
          $('#Gross').text(U.toIRR(totals.gross || 0));
          $('#InsurancePayable').text(U.toIRR(totals.base || 0));
          $('#SuppPayable').text(U.toIRR(totals.supplementary || 0));
          $('#PatientPayable').text(U.toIRR(totals.patient || 0)).attr('data-value', totals.patient || 0);
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
    $suppIns.val('');
    $suppPlan.val('');
    
    const receptionId = $('#ReceptionId').val();
    if (receptionId && receptionId > 0) {
      persist();
    }
    
    toastr.info('بیمه تکمیلی حذف شد');
  }

  // Event handlers
  $baseIns.on('change', function() {
    // وقتی بیمه پایه تغییر کرد، پلن‌های آن را لود کن
    loadPlans();
    persist();
  });

  $basePlan.on('change', persist);
  $suppIns.on('change', function() {
    loadPlans();
    persist();
  });
  $suppPlan.on('change', persist);

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
