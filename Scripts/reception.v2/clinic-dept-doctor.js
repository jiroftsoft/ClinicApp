(function(API){
  function bootstrap(){
    API.get("/bootstrap",{ clinicId: $("#ClinicId").val(), deptId: $("#DepartmentId").val() })
      .then(API.ok)
      .then(d=>{
        console.log('🏥 V2: Bootstrap data loaded:', d);
        
        // Fill departments
        if(d.departments) {
          const $deptSelect = $("#DepartmentId");
          $deptSelect.empty().append('<option value="">انتخاب کنید</option>');
          d.departments.forEach(dept => {
            $deptSelect.append(`<option value="${dept.departmentId}">${dept.name}</option>`);
          });
        }
        
        // Fill doctors
        if(d.doctors) {
          const $doctorSelect = $("#DoctorId");
          $doctorSelect.empty().append('<option value="">انتخاب کنید</option>');
          d.doctors.forEach(doctor => {
            $doctorSelect.append(`<option value="${doctor.doctorId}">${doctor.name}</option>`);
          });
        }
      })
      .catch(err => {
        console.error('🏥 V2: Bootstrap error:', err);
        toastr.error('خطا در بارگذاری اطلاعات اولیه');
      });
  }
  
  // Load on page ready
  $(document).ready(bootstrap);
  
  // Reload when department changes
  $("#DepartmentId").on('change', function() {
    const deptId = $(this).val();
    if(deptId) {
      bootstrap();
    }
  });
})(window.ReceptionAPI);
