(function(API){
  'use strict';

  /**
   * بارگذاری داده‌های اولیه (کلینیک، دپارتمان، پزشک)
   * پشتیبانی از PascalCase و camelCase
   */
  function bootstrap(){
    console.log('🏥 V2: Starting bootstrap...');
    
    const clinicId = $("#ClinicId").val();
    const deptId = $("#DepartmentId").val();
    
    API.get("/bootstrap", { clinicId: clinicId, deptId: deptId })
      .then(function(fullResponse) {
        console.log('🏥 V2: Bootstrap raw response:', fullResponse);
        
        // Extract data using API.ok (handles ServiceResult structure)
        const response = API.ok(fullResponse);
        console.log('🏥 V2: Bootstrap extracted data:', response);
        
        // پشتیبانی از PascalCase و camelCase
        const departments = response.Departments || response.departments || [];
        const doctors = response.Doctors || response.doctors || [];
        const clinics = response.Clinics || response.clinics || [];
        const financialYear = response.FinancialYear || response.financialYear;
        
        console.log('🏥 V2: Bootstrap parsed - Departments:', departments.length, 'Doctors:', doctors.length, 'Clinics:', clinics.length);
        
        // ذخیره FinancialYear در window برای استفاده در auto-draft-manager
        if (financialYear) {
          window.ReceptionBootstrap = window.ReceptionBootstrap || {};
          window.ReceptionBootstrap.FinancialYear = financialYear;
        }
        
        // Fill clinics (اگر وجود دارد)
        if (clinics && clinics.length > 0) {
          const $clinicSelect = $("#ClinicId");
          const currentClinicId = $clinicSelect.val();
          $clinicSelect.empty().append('<option value="">انتخاب کنید</option>');
          clinics.forEach(function(clinic) {
            const clinicId = clinic.clinicId || clinic.ClinicId;
            const clinicName = clinic.name || clinic.Name || clinic.clinicName || clinic.ClinicName;
            $clinicSelect.append(`<option value="${clinicId}">${clinicName}</option>`);
          });
          // اگر قبلاً انتخاب شده بود، مقدار را برگردان
          if (currentClinicId) {
            $clinicSelect.val(currentClinicId);
          }
        }
        
        // Fill departments
        if (departments && departments.length > 0) {
          const $deptSelect = $("#DepartmentId");
          const currentDeptId = $deptSelect.val();
          $deptSelect.empty().append('<option value="">انتخاب کنید</option>');
          departments.forEach(function(dept) {
            const deptId = dept.departmentId || dept.DepartmentId;
            const deptName = dept.name || dept.Name || dept.departmentName || dept.DepartmentName;
            $deptSelect.append(`<option value="${deptId}">${deptName}</option>`);
          });
          // اگر قبلاً انتخاب شده بود، مقدار را برگردان
          if (currentDeptId) {
            $deptSelect.val(currentDeptId);
          }
          console.log('🏥 V2: Departments filled:', departments.length);
        } else {
          console.warn('🏥 V2: No departments found in bootstrap response');
        }
        
        // Fill doctors (فقط اگر deptId انتخاب شده باشد)
        const selectedDeptId = $("#DepartmentId").val();
        if (selectedDeptId && doctors && doctors.length > 0) {
          const $doctorSelect = $("#DoctorId");
          const currentDoctorId = $doctorSelect.val();
          $doctorSelect.empty().append('<option value="">انتخاب کنید</option>');
          doctors.forEach(function(doctor) {
            const doctorId = doctor.doctorId || doctor.DoctorId;
            const doctorName = doctor.name || doctor.Name || doctor.doctorName || doctor.DoctorName;
            $doctorSelect.append(`<option value="${doctorId}">${doctorName}</option>`);
          });
          // اگر قبلاً انتخاب شده بود، مقدار را برگردان
          if (currentDoctorId) {
            $doctorSelect.val(currentDoctorId);
          }
          console.log('🏥 V2: Doctors filled:', doctors.length);
        } else if (selectedDeptId) {
          // اگر دپارتمان انتخاب شده اما پزشکی نیست
          const $doctorSelect = $("#DoctorId");
          $doctorSelect.empty().append('<option value="">ابتدا دپارتمان را انتخاب کنید</option>');
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Bootstrap error:', err);
        toastr.error('خطا در بارگذاری اطلاعات اولیه');
      });
  }
  
  /**
   * بارگذاری مجدد پزشکان هنگام تغییر دپارتمان
   */
  function loadDoctorsForDepartment(deptId) {
    if (!deptId) {
      $("#DoctorId").empty().append('<option value="">ابتدا دپارتمان را انتخاب کنید</option>');
      return;
    }
    
    console.log('🏥 V2: Loading doctors for department:', deptId);
    
    // بارگذاری مجدد bootstrap برای دریافت پزشکان دپارتمان جدید
    bootstrap();
  }
  
  // Load on page ready
  $(document).ready(function() {
    console.log('🏥 V2: Initializing clinic/dept/doctor module...');
    bootstrap();
  });
  
  // Reload doctors when department changes
  $("#DepartmentId").on('change', function() {
    const deptId = $(this).val();
    console.log('🏥 V2: Department changed:', deptId);
    loadDoctorsForDepartment(deptId);
    
    // اگر دپارتمان تغییر کرد، خدمت‌ها را هم لود کن
    if (window.serviceLookupModule && typeof window.serviceLookupModule.loadServices === 'function') {
      window.serviceLookupModule.loadServices(deptId);
    } else if (deptId) {
      // Fallback: اگر service-lookup module export نشده، مستقیماً لود کن
      console.log('🏥 V2: Triggering services load for department:', deptId);
      // service-lookup.js خودش change event را handle می‌کند
    }
  });
  
  // Reload departments when clinic changes
  $("#ClinicId").on('change', function() {
    const clinicId = $(this).val();
    console.log('🏥 V2: Clinic changed:', clinicId);
    if (clinicId) {
      bootstrap();
    }
  });
  
  // Export برای استفاده در ماژول‌های دیگر
  window.clinicDeptDoctorModule = {
    bootstrap: bootstrap,
    loadDoctorsForDepartment: loadDoctorsForDepartment
  };
})(window.ReceptionAPI);
