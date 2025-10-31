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
    
    // ✅ لاگ دقیق‌تر برای دیباگ
    console.log('🏥 V2: Bootstrap params - clinicId:', clinicId, 'deptId:', deptId, 'deptId type:', typeof deptId);
    
    API.get("/bootstrap", { clinicId: clinicId, deptId: deptId })
      .then(function(fullResponse) {
        console.log('🏥 V2: Bootstrap raw response:', fullResponse);
        console.log('🏥 V2: Bootstrap response type:', typeof fullResponse);
        console.log('🏥 V2: Bootstrap response keys:', fullResponse ? Object.keys(fullResponse) : 'null/undefined');
        
        // 🔍 چک Success قبل از extract
        const successValue = fullResponse?.Success ?? fullResponse?.success;
        const isSuccess = successValue === true || successValue === "true" || successValue === 1;
        console.log('🏥 V2: Bootstrap success check - successValue:', successValue, 'isSuccess:', isSuccess);
        
        if (!fullResponse || !isSuccess) {
          const errorMsg = fullResponse?.Message || fullResponse?.message || 'خطا در بارگذاری اطلاعات اولیه';
          console.error('🏥 V2: Bootstrap failed:', errorMsg, fullResponse);
          toastr.error(errorMsg);
          return;
        }
        
        // Extract data using API.ok (handles ServiceResult structure)
        const response = API.ok(fullResponse);
        console.log('🏥 V2: Bootstrap extracted data:', response);
        console.log('🏥 V2: Bootstrap extracted data type:', typeof response);
        console.log('🏥 V2: Bootstrap extracted data keys:', response ? Object.keys(response) : 'null/undefined');
        
        // 🔍 لاگ دقیق‌تر: بررسی ساختار response
        if (response) {
          console.log('🔍 V2: Response.Departments:', response.Departments, 'type:', typeof response.Departments, 'length:', response.Departments?.length);
          console.log('🔍 V2: Response.departments:', response.departments, 'type:', typeof response.departments, 'length:', response.departments?.length);
          console.log('🔍 V2: Response.Doctors:', response.Doctors, 'type:', typeof response.Doctors, 'length:', response.Doctors?.length);
          console.log('🔍 V2: Response.doctors:', response.doctors, 'type:', typeof response.doctors, 'length:', response.doctors?.length);
          console.log('🔍 V2: Response.Clinics:', response.Clinics, 'type:', typeof response.Clinics, 'length:', response.Clinics?.length);
          console.log('🔍 V2: Response.clinics:', response.clinics, 'type:', typeof response.clinics, 'length:', response.clinics?.length);
        }
        
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
      $("#DoctorId").prop('disabled', true);
      return;
    }
    
    console.log('🏥 V2: Loading doctors for department:', deptId);
    
    const clinicId = $("#ClinicId").val() || 1; // Default clinic ID = 1 (کلینیک شفا)
    
    // ✅ استفاده از endpoint مستقل به‌جای bootstrap
    API.get("/doctors/by-department", { deptId: deptId, clinicId: clinicId })
      .then(function(fullResponse) {
        console.log('🏥 V2: Doctors raw response:', fullResponse);
        
        // 🔍 چک Success قبل از extract
        const successValue = fullResponse?.Success ?? fullResponse?.success;
        const isSuccess = successValue === true || successValue === "true" || successValue === 1;
        
        if (!fullResponse || !isSuccess) {
          const errorMsg = fullResponse?.Message || fullResponse?.message || 'خطا در بارگذاری پزشکان';
          console.error('🏥 V2: Doctors load failed:', errorMsg, fullResponse);
          toastr.error(errorMsg);
          
          const $doctorSelect = $("#DoctorId");
          $doctorSelect.empty().append('<option value="">خطا در بارگذاری پزشکان</option>');
          $doctorSelect.prop('disabled', true);
          return;
        }
        
        // Extract data using API.ok (handles ServiceResult structure)
        // ✅ GetDoctorsByDepartment مستقیماً ServiceResult<List<DoctorDto>> برمی‌گرداند
        // پس API.ok() مستقیماً List<DoctorDto> را برمی‌گرداند (نه یک object با property Data)
        const response = API.ok(fullResponse);
        console.log('🏥 V2: Doctors extracted data:', response);
        console.log('🔍 V2: Response type:', typeof response, 'isArray:', Array.isArray(response));
        console.log('🔍 V2: Response keys:', response && typeof response === 'object' && !Array.isArray(response) ? Object.keys(response) : 'N/A (array)');
        
        // پشتیبانی از PascalCase و camelCase
        // ✅ اگر response مستقیماً Array باشد (List<DoctorDto>):
        let doctors = [];
        if (Array.isArray(response)) {
          // API.ok() مستقیماً Array را برگردانده است
          doctors = response;
          console.log('✅ V2: Response is Array directly, count:', doctors.length);
        } else if (response && typeof response === 'object') {
          // اگر response یک object است، property Data را چک کن
          doctors = response.Data || response.data || response.Doctors || response.doctors || [];
          console.log('✅ V2: Response is Object, Data count:', doctors.length);
        } else {
          console.warn('⚠️ V2: Unexpected response type:', typeof response);
          doctors = [];
        }
        
        console.log('🏥 V2: Doctors parsed - Count:', doctors.length);
        
        const $doctorSelect = $("#DoctorId");
        $doctorSelect.empty().append('<option value="">انتخاب کنید</option>');
        
        if (doctors.length === 0) {
          $doctorSelect.append('<option value="">پزشکی در این دپارتمان یافت نشد</option>');
          $doctorSelect.prop('disabled', true);
          console.warn('🏥 V2: No doctors found for department:', deptId);
        } else {
          doctors.forEach(function(doctor) {
            const doctorId = doctor.doctorId || doctor.DoctorId;
            const firstName = doctor.firstName || doctor.FirstName || '';
            const lastName = doctor.lastName || doctor.LastName || '';
            const specialization = doctor.specialization || doctor.Specialization || '';
            const fullName = (firstName + ' ' + lastName).trim();
            const displayName = specialization ? `${fullName} — ${specialization}` : fullName;
            
            $doctorSelect.append(`<option value="${doctorId}">${displayName}</option>`);
          });
          $doctorSelect.prop('disabled', false);
          console.log('🏥 V2: Doctors filled:', doctors.length);
          
          // ✅ Trigger state change event for Summary Header
          if (doctors.length > 0) {
            const selectedDoctor = doctors[0];
            $(document).trigger('rv2:stateChanged', {
              doctor: {
                DoctorId: selectedDoctor.doctorId || selectedDoctor.DoctorId,
                Name: (selectedDoctor.firstName || selectedDoctor.FirstName || '') + ' ' + (selectedDoctor.lastName || selectedDoctor.LastName || '')
              }
            });
          }
        }
      })
      .catch(function(error) {
        console.error('🏥 V2: Doctors load error:', error);
        toastr.error('خطا در بارگذاری پزشکان');
        
        const $doctorSelect = $("#DoctorId");
        $doctorSelect.empty().append('<option value="">خطا در بارگذاری پزشکان</option>');
        $doctorSelect.prop('disabled', true);
      });
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
