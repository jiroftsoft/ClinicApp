(function(API, U){
  // Load insurance plans on page load
  function loadInsurancePlans() {
    API.get("/insurance/plans")
      .then(API.ok)
      .then(plans => {
        console.log('🏥 V2: Insurance plans loaded:', plans);
        
        // Fill base insurance plans
        const $baseSelect = $("#BasePlanId");
        $baseSelect.empty().append('<option value="">انتخاب کنید</option>');
        if(plans.basePlans) {
          plans.basePlans.forEach(plan => {
            $baseSelect.append(`<option value="${plan.insuranceId}">${plan.insuranceName} (${plan.coveragePercentage}%)</option>`);
          });
        }
        
        // Fill supplementary insurance plans
        const $suppSelect = $("#SuppPlanId");
        $suppSelect.empty().append('<option value="">انتخاب کنید</option>');
        if(plans.supplementaryPlans) {
          plans.supplementaryPlans.forEach(plan => {
            $suppSelect.append(`<option value="${plan.insuranceId}">${plan.insuranceName} (${plan.coveragePercentage}%)</option>`);
          });
        }
      })
      .catch(err => {
        console.error('🏥 V2: Insurance plans load error:', err);
        toastr.error('خطا در بارگذاری بیمه‌ها');
      });
  }
  
  // Load on page ready
  $(document).ready(loadInsurancePlans);

  $("#BtnSetInsurances").on("click", function(){
    const receptionId = $("#ReceptionId").val();
    const basePlanId = $("#BasePlanId").val() || null;
    const supplementaryPlanId = $("#SuppPlanId").val() || null;
    
    if(!receptionId || receptionId <= 0) {
      // Try to create auto-draft first
      if (window.AutoDraftManager && !window.AutoDraftManager.isDraftCreated()) {
        window.AutoDraftManager.createDraft().then(draftId => {
          if (draftId) {
            $("#ReceptionId").val(draftId);
            proceedWithInsurance();
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
    
    proceedWithInsurance();
  });
  
  function proceedWithInsurance() {
    const receptionId = $("#ReceptionId").val();
    const basePlanId = $("#BasePlanId").val() || null;
    const supplementaryPlanId = $("#SuppPlanId").val() || null;
    
    const payload = {
      receptionId: receptionId,
      basePlanId: basePlanId,
      supplementaryPlanId: supplementaryPlanId
    };
    
    API.post("/insurances/set", payload)
      .then(API.ok)
      .then(d=>{
        console.log('🏥 V2: Insurances set:', d);
        toastr.success('بیمه‌ها تنظیم شدند');
        
        // Update totals
        if(d.totals) {
          $("#Gross").text(U.toIRR(d.totals.gross || 0));
          $("#InsurancePayable").text(U.toIRR(d.totals.base || 0));
          $("#SuppPayable").text(U.toIRR(d.totals.supplementary || 0));
          $("#PatientPayable").text(U.toIRR(d.totals.patient || 0)).attr("data-value", d.totals.patient || 0);
        }
      })
      .catch(err => {
        console.error('🏥 V2: Set insurances error:', err);
        toastr.error('خطا در تنظیم بیمه‌ها');
      });
  }
})(window.ReceptionAPI, window.RxUtils);
