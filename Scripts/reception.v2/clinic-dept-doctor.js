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
        
        // ✅ Extract data using API.ok (handles ServiceResult structure)
        // ✅ GetDoctorsByDepartment اکنون { doctors: [...] } برمی‌گرداند
        const response = API.ok(fullResponse);
        console.log('🏥 V2: Doctors extracted data:', response);
        console.log('🔍 V2: Response type:', typeof response, 'isArray:', Array.isArray(response));
        
        // ✅ پشتیبانی از پاسخ جدید { doctors: [...] }
        let doctors = [];
        if (response && typeof response === 'object') {
          // ✅ پاسخ جدید: { doctors: [...] }
          doctors = response.doctors || response.Doctors || 
                    (Array.isArray(response) ? response : []);
          console.log('✅ V2: Doctors extracted from response, count:', doctors.length);
        } else {
          console.warn('⚠️ V2: Unexpected response type:', typeof response);
          doctors = [];
        }
        
        console.log('🏥 V2: Doctors parsed - Count:', doctors.length);
        
        fillDoctorOptions(doctors);
        
        if (doctors.length === 0) {
          toastr.warning('برای این دپارتمان، پزشک فعالی تعریف نشده است.');
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
  
  /**
   * ✅ پر کردن Dropdown پزشکان (بهینه‌سازی شده)
   * پشتیبانی از DoctorOptionDto (FullName, Title, DepartmentName)
   */
  function fillDoctorOptions(doctors) {
    const $doctorSelect = $("#DoctorId");
    $doctorSelect.empty().append('<option value="">— انتخاب پزشک —</option>');
    
    if (!doctors || doctors.length === 0) {
      $doctorSelect.append('<option value="">پزشکی در این دپارتمان یافت نشد</option>');
      $doctorSelect.prop('disabled', true);
      return;
    }
    
    doctors.forEach(function(doctor) {
      const doctorId = doctor.doctorId || doctor.DoctorId;
      const fullName = doctor.fullName || doctor.FullName || 
                       `${doctor.firstName || doctor.FirstName || ''} ${doctor.lastName || doctor.LastName || ''}`.trim();
      const title = doctor.title || doctor.Title || doctor.specialization || doctor.Specialization || '';
      const displayName = title ? `${fullName} — ${title}` : fullName;
      
      $doctorSelect.append(`<option value="${doctorId}">${displayName}</option>`);
    });
    
    $doctorSelect.prop('disabled', false);
    console.log('🏥 V2: Doctors filled:', doctors.length);
  }
  
  /**
   * بارگذاری پزشکان مجاز برای یک خدمت در دپارتمان
   * این تابع فقط پزشکان را لود می‌کند که برای خدمت انتخاب شده مجاز هستند
   */
  window.loadDoctorsByService = function(options) {
    const { serviceId, deptId, clinicId } = options || {};
    
    if (!serviceId || !deptId) {
      console.warn('🏥 V2: Cannot load doctors by service - missing serviceId or deptId');
      $("#DoctorId").empty().append('<option value="">ابتدا خدمت و دپارتمان را انتخاب کنید</option>');
      $("#DoctorId").prop('disabled', true);
      return Promise.resolve();
    }
    
    console.log('🏥 V2: Loading eligible doctors for service:', { serviceId, deptId, clinicId });
    
    const effectiveClinicId = clinicId || 1; // Default clinic ID = 1 (کلینیک شفا)
    
    // ✅ استفاده از endpoint مستقل برای فیلتر پزشکان بر اساس خدمت
    return API.get("/doctors/by-service", { 
      deptId: deptId, 
      serviceId: serviceId, 
      clinicId: effectiveClinicId 
    })
      .then(function(fullResponse) {
        console.log('🏥 V2: Eligible doctors raw response:', fullResponse);
        
        // 🔍 چک Success قبل از extract
        const successValue = fullResponse?.Success ?? fullResponse?.success;
        const isSuccess = successValue === true || successValue === "true" || successValue === 1;
        
        if (!fullResponse || !isSuccess) {
          const errorMsg = fullResponse?.Message || fullResponse?.message || 'خطا در بارگذاری پزشکان مجاز';
          console.error('🏥 V2: Eligible doctors load failed:', errorMsg, fullResponse);
          toastr.warning(errorMsg);
          
          const $doctorSelect = $("#DoctorId");
          $doctorSelect.empty().append('<option value="">خطا در بارگذاری پزشکان مجاز</option>');
          $doctorSelect.prop('disabled', true);
          return;
        }
        
        // ✅ Extract data using API.ok (handles ServiceResult structure)
        // ✅ GetDoctorsByService اکنون { doctors: [...] } برمی‌گرداند
        const response = API.ok(fullResponse);
        console.log('🏥 V2: Eligible doctors extracted data:', response);
        console.log('🔍 V2: Response type:', typeof response, 'isArray:', Array.isArray(response));
        
        // ✅ پشتیبانی از پاسخ جدید { doctors: [...] }
        let doctors = [];
        if (response && typeof response === 'object') {
          // ✅ پاسخ جدید: { doctors: [...] }
          doctors = response.doctors || response.Doctors || 
                    (Array.isArray(response) ? response : []);
          console.log('✅ V2: Eligible doctors extracted from response, count:', doctors.length);
        } else {
          console.warn('⚠️ V2: Unexpected response type:', typeof response);
          doctors = [];
        }
        
        console.log('🏥 V2: Eligible doctors parsed - Count:', doctors.length);
        
        // ✅ استفاده از fillDoctorOptions برای یکنواختی
        const previouslySelectedDoctorId = $("#DoctorId").val(); // حفظ انتخاب قبلی اگر ممکن باشد
        
        if (doctors.length === 0) {
          fillDoctorOptions([]);
          console.warn('🏥 V2: No eligible doctors found for service:', serviceId);
          
          // ✅ اگر قبلاً پزشکی انتخاب شده بود و حالا غیرمجاز است، هشدار بده
          if (previouslySelectedDoctorId) {
            toastr.warning('پزشک انتخاب شده برای این خدمت مجاز نیست. لطفاً پزشک دیگری انتخاب کنید.');
          } else {
            toastr.info('هیچ پزشکی برای این خدمت در این دپارتمان مجاز/در دسترس نیست.');
          }
        } else {
          // ✅ پر کردن dropdown با استفاده از fillDoctorOptions
          fillDoctorOptions(doctors);
          
          // ✅ اگر پزشک قبلی انتخاب شده بود و حالا در لیست مجاز است، انتخاب را حفظ کن
          if (previouslySelectedDoctorId) {
            const selectedDoctorFound = doctors.some(function(doctor) {
              const doctorId = doctor.doctorId || doctor.DoctorId;
              return previouslySelectedDoctorId == doctorId || previouslySelectedDoctorId == doctorId.toString();
            });
            
            if (selectedDoctorFound) {
              $("#DoctorId").val(previouslySelectedDoctorId);
              console.log('✅ V2: Previously selected doctor is still eligible, preserving selection');
            } else {
              // اگر پزشک قبلی دیگر مجاز نیست، هشدار بده و انتخاب را پاک کن
              toastr.warning('پزشک انتخاب شده برای این خدمت مجاز نیست. لطفاً پزشک دیگری انتخاب کنید.');
              $("#DoctorId").val('').trigger('change');
            }
          }
          
          // ✅ Trigger state change event for Summary Header
          const selectedDoctorId = $("#DoctorId").val();
          if (selectedDoctorId && doctors.length > 0) {
            const selectedDoctor = doctors.find(function(d) {
              const doctorId = d.doctorId || d.DoctorId;
              return selectedDoctorId == doctorId || selectedDoctorId == doctorId.toString();
            });
            
            if (selectedDoctor) {
              const fullName = selectedDoctor.fullName || selectedDoctor.FullName || 
                               `${selectedDoctor.firstName || selectedDoctor.FirstName || ''} ${selectedDoctor.lastName || selectedDoctor.LastName || ''}`.trim();
              $(document).trigger('rv2:stateChanged', {
                doctor: {
                  DoctorId: selectedDoctor.doctorId || selectedDoctor.DoctorId,
                  FullName: fullName,
                  Name: fullName
                }
              });
            }
          }
        }
      })
      .catch(function(error) {
        console.error('🏥 V2: Eligible doctors load error:', error);
        toastr.error('خطا در بارگذاری پزشکان مجاز برای خدمت');
        
        const $doctorSelect = $("#DoctorId");
        $doctorSelect.empty().append('<option value="">خطا در بارگذاری پزشکان مجاز</option>');
        $doctorSelect.prop('disabled', true);
      });
  };

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
    
    // ✅ گام 5: Draft را فقط وقتی بساز که چهارتا کلید آماده باشد
    tryCreateDraftIfAllReady();
  });
  
  /**
   * ✅ بررسی و ایجاد Draft اگر تمام فیلدهای الزامی پر شده باشند
   * فیلدهای الزامی: patient + clinic + department + doctor
   */
  function tryCreateDraftIfAllReady() {
    const patientId = $("#Patient_PatientId").val();
    const nationalCode = $("#Patient_NationalCode").val();
    const clinicId = $("#ClinicId").val();
    const departmentId = $("#DepartmentId").val();
    const doctorId = $("#DoctorId").val();
    const existingReceptionId = $("#ReceptionId").val();
    
    // اگر ReceptionId موجود است، نیازی به ایجاد نیست
    if (existingReceptionId && existingReceptionId > 0) {
      console.log('🏥 V2: ReceptionId already exists, skipping draft creation');
      return;
    }
    
    // بررسی وجود فیلدهای الزامی
    if ((!patientId && !nationalCode) || !clinicId || !departmentId || !doctorId) {
      console.log('🏥 V2: Missing required fields for draft creation');
      return;
    }
    
    // ✅ تمام فیلدهای الزامی پر شده‌اند، Draft ایجاد کن
    console.log('🏥 V2: All required fields ready, creating draft...');
    if (window.AutoDraftManager && typeof window.AutoDraftManager.ensureDraftOrSkip === 'function') {
      window.AutoDraftManager.ensureDraftOrSkip({
        patientId: patientId,
        clinicId: clinicId,
        departmentId: departmentId,
        doctorId: doctorId,
        receptionId: existingReceptionId
      }).then(function(receptionId) {
        if (receptionId && receptionId > 0) {
          console.log('🏥 V2: Draft created successfully:', receptionId);
          $("#ReceptionId").val(receptionId);
        }
      }).catch(function(err) {
        console.error('🏥 V2: Draft creation failed:', err);
      });
    } else {
      console.warn('🏥 V2: AutoDraftManager not available');
    }
  }
  
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
    loadDoctorsForDepartment: loadDoctorsForDepartment,
    fillDoctorOptions: fillDoctorOptions,
    tryCreateDraftIfAllReady: tryCreateDraftIfAllReady
  };
})(window.ReceptionAPI);
