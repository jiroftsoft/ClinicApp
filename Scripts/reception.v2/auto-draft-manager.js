(function(API, U, $) {
  'use strict';
  
  let currentDraftId = null;
  let autoSaveTimeout = null;
  let isDraftCreated = false;
  let isCreatingDraft = false; // 🏥 MEDICAL: Request Lock برای جلوگیری از Race Condition
  let draftCreationPromise = null; // 🏥 MEDICAL: Promise برای wait کردن request در حال اجرا
  let draftCreationTimeout = null; // 🏥 MEDICAL: Debounce timeout
  
  /**
   * 🏥 MEDICAL: ایجاد پیش‌نویس با Request Lock و Race Condition Prevention
   * این تابع از ایجاد duplicate Draft جلوگیری می‌کند
   */
  function createAutoDraft() {
    // ✅ بررسی 1: اگر Draft قبلاً ایجاد شده، برگردان
    if (isDraftCreated && currentDraftId) {
      console.log('🏥 V2: Draft already created:', currentDraftId);
      return Promise.resolve(currentDraftId);
    }
    
    // ✅ بررسی 2: اگر request در حال اجرا است، منتظر بمان
    if (isCreatingDraft && draftCreationPromise) {
      console.log('🏥 V2: Draft creation already in progress, waiting for completion...');
      return draftCreationPromise;
    }
    
    // ✅ بررسی 3: بررسی فیلدهای الزامی
    const patientId = $("#Patient_PatientId").val();
    const nationalCode = $("#Patient_NationalCode").val();
    const clinicId = $("#ClinicId").val();
    const departmentId = $("#DepartmentId").val();
    const doctorId = $("#DoctorId").val();
    
    // Require minimal data to avoid server 400/500: patient + clinic + department + doctor
    if ((!patientId && !nationalCode) || !clinicId || !departmentId || !doctorId) {
      console.log('🏥 V2: Missing required fields for draft (patient/clinic/department/doctor). Skipping.');
      return Promise.resolve(null);
    }
    
    // ✅ Set Lock: جلوگیری از duplicate request
    isCreatingDraft = true;
    console.log('🏥 V2: Creating auto-draft with lock...');
    
    const payload = {
      patientId: patientId || null,
      nationalCode: nationalCode || null,
      clinicId: clinicId,
      departmentId: departmentId,
      doctorId: doctorId,
      financialYear: (window.ReceptionBootstrap && window.ReceptionBootstrap.FinancialYear) || 1404
    };
    
    // ✅ ایجاد Promise و ذخیره برای wait کردن
    draftCreationPromise = API.post("/draft/create", payload)
      .then(API.ok)
      .then(d => {
        console.log('🏥 V2: Auto-draft created successfully:', d);
        
        // ✅ بررسی duplicate: اگر Draft دیگری در این فاصله ایجاد شده، از آن استفاده کن
        if (isDraftCreated && currentDraftId && currentDraftId !== d.receptionId) {
          console.warn('⚠️ V2: Another draft was created during request. Using existing:', currentDraftId, 'New:', d.receptionId);
          return currentDraftId;
        }
        
        currentDraftId = d.receptionId;
        isDraftCreated = true;
        
        // Update hidden field
        $("#ReceptionId").val(currentDraftId);
        
        // Show success message
        toastr.success('پذیرش موقت ایجاد شد');
        
        return currentDraftId;
      })
      .catch(err => {
        console.error('🏥 V2: Auto-draft creation failed:', err);
        toastr.error('خطا در ایجاد پذیرش موقت');
        throw err;
      })
      .finally(() => {
        // ✅ Reset Lock: آزاد کردن lock در هر صورت (success یا error)
        isCreatingDraft = false;
        draftCreationPromise = null;
      });
    
    return draftCreationPromise;
  }
  
  /**
   * 🏥 MEDICAL: ایجاد پیش‌نویس با Debouncing برای بهبود UX
   * این تابع از ایجاد multiple request در زمان کوتاه جلوگیری می‌کند
   */
  function scheduleDraftCreation() {
    // ✅ Clear existing timeout
    if (draftCreationTimeout) {
      clearTimeout(draftCreationTimeout);
      draftCreationTimeout = null;
    }
    
    // ✅ اگر Draft قبلاً ایجاد شده، نیازی به ایجاد نیست
    if (isDraftCreated && currentDraftId) {
      return;
    }
    
    // ✅ Set new timeout با debounce 500ms
    draftCreationTimeout = setTimeout(() => {
      draftCreationTimeout = null;
      createAutoDraft().catch(err => {
        console.error('🏥 V2: Scheduled draft creation error:', err);
        // Don't show error to user - it's auto-creation
      });
    }, 500); // 500ms debounce برای محیط درمانی
  }

  /**
   * ✅ گام 2 - Draft Orchestrator: ensureDraftOrSkip
   * بررسی وجود ReceptionId، اگر نبود و شرایط کامل بود، Auto-draft ساخته می‌شود
   * @param {Object} state - وضعیت فعلی فرم (patientId, clinicId, departmentId, doctorId)
   * @returns {Promise<number|null>} ReceptionId یا null اگر شرایط کامل نیست
   */
  async function ensureDraftOrSkip(state) {
    const { patientId, clinicId, departmentId, doctorId, receptionId } = state || {};
    
    // اگر ReceptionId موجود است، برگردان
    const existingReceptionId = receptionId || $("#ReceptionId").val();
    if (existingReceptionId && existingReceptionId > 0) {
      console.log('🏥 V2: ReceptionId already exists:', existingReceptionId);
      currentDraftId = parseInt(existingReceptionId);
      isDraftCreated = true;
      return Promise.resolve(currentDraftId);
    }
    
    // اگر ReceptionId موجود نیست، بررسی شرایط
    const resolvedPatientId = patientId || $("#Patient_PatientId").val();
    const resolvedNationalCode = $("#Patient_NationalCode").val();
    const resolvedClinicId = clinicId || $("#ClinicId").val();
    const resolvedDepartmentId = departmentId || $("#DepartmentId").val();
    const resolvedDoctorId = doctorId || $("#DoctorId").val();
    
    // بررسی وجود فیلدهای الزامی
    if ((!resolvedPatientId && !resolvedNationalCode) || !resolvedClinicId || !resolvedDepartmentId || !resolvedDoctorId) {
      console.log('🏥 V2: Missing required fields for draft (patient/clinic/department/doctor). Skipping.');
      return Promise.resolve(null);
    }
    
    // اگر شرایط کامل است، draft ایجاد کن
    console.log('🏥 V2: Required fields complete, creating draft...');
    return createAutoDraft()
      .then(function(draftId) {
        if (draftId) {
          console.log('🏥 V2: Draft created successfully:', draftId);
          return draftId;
        } else {
          console.warn('🏥 V2: Draft creation returned null');
          return null;
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Draft creation failed:', err);
        return null;
      });
  }

  /**
   * هشدار در صورت ناقص بودن فیلدهای الزامی
   */
  function warnDraftMissing() {
    const patientId = $("#Patient_PatientId").val();
    const nationalCode = $("#Patient_NationalCode").val();
    const clinicId = $("#ClinicId").val();
    const departmentId = $("#DepartmentId").val();
    const doctorId = $("#DoctorId").val();
    
    const missingFields = [];
    if (!patientId && !nationalCode) missingFields.push('بیمار');
    if (!clinicId) missingFields.push('کلینیک');
    if (!departmentId) missingFields.push('دپارتمان');
    if (!doctorId) missingFields.push('پزشک');
    
    if (missingFields.length > 0) {
      toastr.warning('فیلدهای الزامی برای پیش‌نویس ناقص است: ' + missingFields.join(', '));
    } else {
      toastr.warning('برای ثبت این اطلاعات، ابتدا پذیرش را ایجاد کنید');
    }
    
    console.warn('🏥 V2: Draft missing - Missing fields:', missingFields);
  }
  
  // Auto-save functionality with debouncing
  function autoSave() {
    if (!currentDraftId || !isDraftCreated) return;
    
    // Clear existing timeout
    if (autoSaveTimeout) {
      clearTimeout(autoSaveTimeout);
    }
    
    // Set new timeout for auto-save
    autoSaveTimeout = setTimeout(() => {
      console.log('🏥 V2: Auto-saving draft...');
      
      // Collect current form data
      const formData = {
        receptionId: currentDraftId,
        patientId: $("#Patient_PatientId").val(),
        nationalCode: $("#Patient_NationalCode").val(),
        fullName: $("#Patient_FullName").val(),
        mobile: $("#Patient_Mobile").val(),
        clinicId: $("#ClinicId").val(),
        departmentId: $("#DepartmentId").val(),
        doctorId: $("#DoctorId").val(),
        basePlanId: $("#BasePlanId").val() || null,
        supplementaryPlanId: $("#SuppPlanId").val() || null
      };
      
      // Auto-save draft data
      API.post("/draft/update", formData)
        .then(API.ok)
        .then(() => {
          console.log('🏥 V2: Draft auto-saved successfully');
          // Update form dirty state
          if (window.FormDirty) {
            window.FormDirty.clean();
          }
        })
        .catch(err => {
          console.error('🏥 V2: Auto-save failed:', err);
          // Don't show error to user for auto-save failures
        });
    }, 2000); // 2 second debounce
  }
  
  // Initialize auto-draft system
  function initializeAutoDraft() {
    console.log('🏥 V2: Initializing auto-draft system...');
    
    // Create hidden field for reception ID if it doesn't exist
    if (!$("#ReceptionId").length) {
      $('<input type="hidden" id="ReceptionId" name="ReceptionId" />').appendTo('body');
    }
    
    // Auto-create draft when patient data is entered - با Debouncing
    $(document).on('blur', '#Patient_NationalCode, #Patient_FullName, #Patient_Mobile', function() {
      const nationalCode = $("#Patient_NationalCode").val();
      const fullName = $("#Patient_FullName").val();
      const mobile = $("#Patient_Mobile").val();
      
      if ((nationalCode && nationalCode.length >= 10) || fullName || mobile) {
        // ✅ استفاده از Debounced version
        scheduleDraftCreation();
      }
    });
    
    // Auto-save on form changes
    $(document).on('input change', '.reception-pro :input', function() {
      if (isDraftCreated) {
        autoSave();
      }
    });
    
    // Auto-create draft when clinic/department is selected - با Debouncing
    $(document).on('change', '#ClinicId, #DepartmentId, #DoctorId', function() {
      if (isDraftCreated) {
        autoSave();
      } else {
        // Try to create draft if we have patient data
        const nationalCode = $("#Patient_NationalCode").val();
        const fullName = $("#Patient_FullName").val();
        const mobile = $("#Patient_Mobile").val();
        
        if ((nationalCode && nationalCode.length >= 10) || fullName || mobile) {
          // ✅ استفاده از Debounced version
          scheduleDraftCreation();
        }
      }
    });
    
    // Auto-create draft when insurance is selected - با Debouncing
    $(document).on('change', '#BasePlanId, #SuppPlanId', function() {
      if (isDraftCreated) {
        autoSave();
      } else {
        // Try to create draft if we have patient data
        const nationalCode = $("#Patient_NationalCode").val();
        const fullName = $("#Patient_FullName").val();
        const mobile = $("#Patient_Mobile").val();
        
        if ((nationalCode && nationalCode.length >= 10) || fullName || mobile) {
          // ✅ استفاده از Debounced version
          scheduleDraftCreation();
        }
      }
    });
    
    console.log('🏥 V2: Auto-draft system initialized');
  }
  
  // Public API - ✅ Bugfix: اطمینان از دسترسی صحیح به async function
  // ایجاد object قبل از تعریف Public API برای اطمینان از دسترسی
  const autoDraftManagerPublicAPI = {
    createDraft: createAutoDraft,
    ensureDraftOrSkip: async function(state) {
      // ✅ Bugfix: Wrapper برای اطمینان از دسترسی صحیح به async function
      try {
        return await ensureDraftOrSkip(state);
      } catch (err) {
        console.error('🏥 V2: ensureDraftOrSkip error:', err);
        return null;
      }
    },
    warnDraftMissing: warnDraftMissing,
    getCurrentDraftId: () => currentDraftId,
    isDraftCreated: () => isDraftCreated,
    forceSave: () => {
      if (autoSaveTimeout) {
        clearTimeout(autoSaveTimeout);
        autoSaveTimeout = null;
      }
      autoSave();
    },
    reset: () => {
      currentDraftId = null;
      isDraftCreated = false;
      isCreatingDraft = false; // ✅ Reset Lock
      draftCreationPromise = null; // ✅ Reset Promise
      if (autoSaveTimeout) {
        clearTimeout(autoSaveTimeout);
        autoSaveTimeout = null;
      }
      if (draftCreationTimeout) {
        clearTimeout(draftCreationTimeout);
        draftCreationTimeout = null;
      }
      $("#ReceptionId").val('');
    }
  };
  
  // ✅ Bugfix: Export به window.AutoDraftManager
  window.AutoDraftManager = autoDraftManagerPublicAPI;
  
  // Initialize when document is ready
  $(document).ready(initializeAutoDraft);
  
})(window.ReceptionAPI, window.RxUtils, jQuery);
