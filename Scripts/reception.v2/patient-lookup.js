(function(API){
  $("#BtnPatientLookup").on("click", function(){
    const nc = $("#NationalCode").val();
    if(!nc || nc.length < 10) {
      toastr.warning('کد ملی باید 10 رقم باشد');
      return;
    }
    
    API.post("/patient/lookup-or-create", { NationalCode: nc })
      .then(API.ok)
      .then(d => { 
        console.log('🏥 V2: Patient found/created:', d);
        toastr.success("بیمار پیدا/ایجاد شد");
        
        // Fill patient fields
        if(d.patient) {
          $("#Patient_NationalCode").val(d.patient.nationalCode || '');
          $("#Patient_FullName").val(d.patient.fullName || '');
          $("#Patient_Mobile").val(d.patient.mobile || '');
          $("#Patient_PatientId").val(d.patient.patientId || '');
          
          // Trigger auto-draft creation
          if (window.AutoDraftManager) {
            window.AutoDraftManager.createDraft().catch(err => {
              console.error('🏥 V2: Auto-draft creation error:', err);
            });
          }
        }
      })
      .catch(err => {
        console.error('🏥 V2: Patient lookup error:', err);
        toastr.error('خطا در جستجوی بیمار');
      });
  });
  
  // Auto-focus on NationalCode
  $("#NationalCode").on('keypress', function(e) {
    if(e.key === 'Enter') {
      $("#BtnPatientLookup").click();
    }
  });
})(window.ReceptionAPI);
