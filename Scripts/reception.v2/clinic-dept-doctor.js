(function(API){
  'use strict';

  // Flag برای جلوگیری از reset شدن departments
  let isInitialized = false;
  
  /**
   * بارگذاری داده‌های اولیه (کلینیک، دپارتمان، پزشک)
   * پشتیبانی از PascalCase و camelCase
   * @param {boolean} reloadDepartments - آیا departments را دوباره لود کند؟
   */
  function bootstrap(reloadDepartments){
    console.log('🏥 V2: Starting bootstrap...', { reloadDepartments: reloadDepartments });
    
    const clinicId = $("#ClinicId").val() || 1; // Default clinic ID = 1 (کلینیک شفا)
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
        
        // Fill clinics (فقط در اولین بار یا اگر reloadDepartments = true)
        if ((!isInitialized || reloadDepartments) && clinics && clinics.length > 0) {
          const $clinicSelect = $("#ClinicId");
          const currentClinicId = $clinicSelect.val() || 1; // Default = 1 (کلینیک شفا)
          $clinicSelect.empty().append('<option value="">انتخاب کنید</option>');
          clinics.forEach(function(clinic) {
            const clinicId = clinic.clinicId || clinic.ClinicId;
            const clinicName = clinic.name || clinic.Name || clinic.clinicName || clinic.ClinicName;
            $clinicSelect.append(`<option value="${clinicId}">${clinicName}</option>`);
          });
          // Set default clinic (کلینیک شفا)
          if (clinics.length === 1) {
            // اگر فقط یک کلینیک داریم، به صورت خودکار انتخاب کن و disable کن
            const singleClinicId = clinics[0].clinicId || clinics[0].ClinicId;
            $clinicSelect.val(singleClinicId).prop('disabled', true);
            console.log('🏥 V2: Single clinic auto-selected and disabled:', singleClinicId);
          } else if (currentClinicId) {
            $clinicSelect.val(currentClinicId);
          } else {
            // اگر clinicId وجود نداشت، اولین کلینیک را انتخاب کن
            const firstClinicId = clinics[0].clinicId || clinics[0].ClinicId;
            $clinicSelect.val(firstClinicId);
            console.log('🏥 V2: First clinic auto-selected:', firstClinicId);
          }
        }
        
        // Fill departments (فقط در اولین بار یا اگر reloadDepartments = true)
        if ((!isInitialized || reloadDepartments) && departments && departments.length > 0) {
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
        } else if (!isInitialized) {
          console.warn('🏥 V2: No departments found in bootstrap response');
        }
        
        // Fill doctors (همیشه - چون وابسته به deptId است)
        const selectedDeptId = $("#DepartmentId").val();
        if (selectedDeptId && doctors && doctors.length > 0) {
          const $doctorSelect = $("#DoctorId");
          const currentDoctorId = $doctorSelect.val();
          $doctorSelect.empty().append('<option value="">انتخاب کنید</option>');
          doctors.forEach(function(doctor) {
            const doctorId = doctor.doctorId || doctor.DoctorId;
            const doctorName = doctor.name || doctor.Name || doctor.doctorName || doctor.DoctorName || doctor.FullName || doctor.fullName;
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
          $doctorSelect.empty().append('<option value="">پزشکی در این دپارتمان یافت نشد</option>');
          console.warn('🏥 V2: No doctors found for department:', selectedDeptId);
        } else {
          // اگر دپارتمان انتخاب نشده
          const $doctorSelect = $("#DoctorId");
          $doctorSelect.empty().append('<option value="">ابتدا دپارتمان را انتخاب کنید</option>');
        }
        
                // Mark as initialized
                if (!isInitialized) {
                  isInitialized = true;
                  console.log('🏥 V2: Bootstrap initialized');
                  
                  // ✅ Trigger FinancialYear update for Summary Header
                  if (financialYear) {
                    $(document).trigger('rv2:stateChanged', {
                      financialYear: {
                        Year: financialYear,
                        YearTitle: 'سال مالی ' + financialYear
                      }
                    });
                  }
                }
      })
      .catch(function(err) {
        console.error('🏥 V2: Bootstrap error:', err);
        toastr.error('خطا در بارگذاری اطلاعات اولیه');
      });
  }
  
  /**
   * بارگذاری مجدد پزشکان هنگام تغییر دپارتمان
   * این تابع فقط پزشکان را لود می‌کند، departments را reset نمی‌کند
   */
  function loadDoctorsForDepartment(deptId) {
    if (!deptId) {
      $("#DoctorId").empty().append('<option value="">ابتدا دپارتمان را انتخاب کنید</option>');
      return;
    }
    
    console.log('🏥 V2: Loading doctors for department:', deptId);
    
    // بارگذاری مجدد bootstrap برای دریافت پزشکان دپارتمان جدید
    // اما departments را reset نکن (reloadDepartments = false)
    bootstrap(false);
  }
  
  // Load on page ready
  $(document).ready(function() {
    console.log('🏥 V2: Initializing clinic/dept/doctor module...');
    bootstrap();
  });
  
  // Reload doctors when department changes
  $("#DepartmentId").on('change', function() {
    const deptId = $(this).val();
    const deptName = $(this).find('option:selected').text();
    
    // ✅ Trigger state change event for Summary Header
    $(document).trigger('rv2:stateChanged', {
      department: {
        DepartmentId: deptId ? parseInt(deptId) : null,
        Name: deptName || '—'
      }
    });
    
    // Trigger FinancialYear update if available
    if (window.ReceptionBootstrap && window.ReceptionBootstrap.FinancialYear) {
      $(document).trigger('rv2:stateChanged', {
        financialYear: {
          Year: window.ReceptionBootstrap.FinancialYear,
          YearTitle: 'سال مالی ' + window.ReceptionBootstrap.FinancialYear
        }
      });
    }
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
  
  // Doctor change handler
  $("#DoctorId").on('change', function() {
    const doctorId = $(this).val();
    const doctorName = $(this).find('option:selected').text();
    
    // ✅ Trigger state change event for Summary Header
    $(document).trigger('rv2:stateChanged', {
      doctor: {
        DoctorId: doctorId ? parseInt(doctorId) : null,
        FullName: doctorName || '—',
        Name: doctorName || '—'
      }
    });
  });
  
  // Reload departments when clinic changes (اما چون یک کلینیک داریم، این event کمتر اتفاق می‌افتد)
  $("#ClinicId").on('change', function() {
    const clinicId = $(this).val();
    console.log('🏥 V2: Clinic changed:', clinicId);
    if (clinicId) {
      // وقتی کلینیک تغییر کرد، departments را هم reload کن
      bootstrap(true);
    }
  });
  
  // Export برای استفاده در ماژول‌های دیگر
  window.clinicDeptDoctorModule = {
    bootstrap: bootstrap,
    loadDoctorsForDepartment: loadDoctorsForDepartment
  };
})(window.ReceptionAPI);
