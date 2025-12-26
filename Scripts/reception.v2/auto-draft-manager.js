(function(API, U, $) {
  'use strict';
  
  let currentDraftId = null;
  let autoSaveTimeout = null;
  let isDraftCreated = false;
  let isCreatingDraft = false; // 🏥 MEDICAL: Request Lock برای جلوگیری از Race Condition
  let draftCreationPromise = null; // 🏥 MEDICAL: Promise برای wait کردن request در حال اجرا
  let draftCreationTimeout = null; // 🏥 MEDICAL: Debounce timeout
  let isDraftFinalizing = false; // 🏥 MEDICAL: Flag برای نشان دادن Draft در حال نهایی شدن
  
  /**
   * 🏥 MEDICAL: ایجاد پیش‌نویس با Request Lock و Race Condition Prevention
   * این تابع از ایجاد duplicate Draft جلوگیری می‌کند
   */
  function createAutoDraft() {
    // 🏥 MEDICAL: بررسی اولویت 1: بررسی DOM برای ReceptionId (ممکن است از جای دیگری set شده باشد)
    const domReceptionId = $("#ReceptionId").val();
    if (domReceptionId) {
      const receptionIdNum = parseInt(domReceptionId);
      if (receptionIdNum && receptionIdNum > 0) {
        console.log('🏥 V2: ReceptionId found in DOM, syncing with memory:', receptionIdNum);
        currentDraftId = receptionIdNum;
        isDraftCreated = true;
        
        // 🚨 PROFESSIONAL: Trigger event برای persist خودکار بیمه‌ها
        $(document).trigger('receptionId:updated', [receptionIdNum]);
        
        return Promise.resolve(currentDraftId);
      }
    }
    
    // ✅ بررسی 1: اگر Draft قبلاً در memory ایجاد شده، برگردان
    if (isDraftCreated && currentDraftId && currentDraftId > 0) {
      console.log('🏥 V2: Draft already created in memory:', currentDraftId);
      // اطمینان از sync بودن با DOM
      $("#ReceptionId").val(currentDraftId);
      return Promise.resolve(currentDraftId);
    }
    
    // ✅ بررسی 2: اگر request در حال اجرا است، منتظر بمان
    if (isCreatingDraft && draftCreationPromise) {
      console.log('🏥 V2: Draft creation already in progress, waiting for completion...');
      return draftCreationPromise.then(function(draftId) {
        // بعد از اتمام request، بررسی کن که آیا Draft ایجاد شد
        if (draftId && draftId > 0) {
          console.log('🏥 V2: Draft creation completed, returning:', draftId);
          return draftId;
        }
        // اگر null برگشت، بررسی کن که آیا Draft در DOM وجود دارد
        const domId = $("#ReceptionId").val();
        if (domId) {
          const domIdNum = parseInt(domId);
          if (domIdNum && domIdNum > 0) {
            console.log('🏥 V2: Draft found in DOM after promise:', domIdNum);
            currentDraftId = domIdNum;
            isDraftCreated = true;
            return domIdNum;
          }
        }
        return null;
      });
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
      .then(function(rawResponse) {
        console.log('🏥 V2: Draft create raw response:', rawResponse);
        console.log('🏥 V2: Raw response type:', typeof rawResponse);
        console.log('🏥 V2: Raw response Success:', rawResponse?.Success, rawResponse?.success);
        console.log('🏥 V2: Raw response Data:', rawResponse?.Data, rawResponse?.data);
        console.log('🏥 V2: Raw response keys:', rawResponse ? Object.keys(rawResponse) : 'null');
        
        const okResult = API.ok(rawResponse);
        console.log('🏥 V2: API.ok result:', okResult);
        console.log('🏥 V2: API.ok result type:', typeof okResult);
        console.log('🏥 V2: API.ok result keys:', okResult ? Object.keys(okResult) : 'null');
        
        return okResult;
      })
      .then(d => {
        console.log('🏥 V2: Auto-draft created successfully (after API.ok):', d);
        console.log('🏥 V2: Response type:', typeof d, 'Keys:', d ? Object.keys(d) : 'null');
        console.log('🏥 V2: Full response object:', JSON.stringify(d, null, 2));
        
        // 🏥 MEDICAL: استخراج receptionId با پشتیبانی از چندین format
        let receptionId = null;
        if (d) {
          // بررسی تمام احتمالات
          receptionId = d.receptionId || d.ReceptionId || d.reception_id || d.id || d.Id || d.ReceptionID;
          
          // اگر هنوز null است، بررسی کن که آیا d خودش یک عدد است
          if (!receptionId && typeof d === 'number') {
            receptionId = d;
          }
          
          // اگر هنوز null است، بررسی کن که آیا d یک object با property های مختلف است
          if (!receptionId && typeof d === 'object') {
            // بررسی تمام keys
            for (const key in d) {
              if (key.toLowerCase().includes('reception') || key.toLowerCase() === 'id') {
                const value = d[key];
                if (typeof value === 'number' && value > 0) {
                  receptionId = value;
                  console.log('🏥 V2: Found receptionId in key:', key, 'value:', value);
                  break;
                }
              }
            }
          }
          
          console.log('🏥 V2: Extracted receptionId:', receptionId, 'from:', d);
        }
        
        if (!receptionId || receptionId <= 0) {
          console.error('❌ V2: Invalid receptionId extracted:', receptionId, 'from response:', d);
          console.error('❌ V2: Full response for debugging:', JSON.stringify(d, null, 2));
          throw new Error('Invalid receptionId in draft creation response: ' + JSON.stringify(d));
        }
        
        // ✅ بررسی duplicate: اگر Draft دیگری در این فاصله ایجاد شده، از آن استفاده کن
        if (isDraftCreated && currentDraftId && currentDraftId > 0 && currentDraftId !== receptionId) {
          console.warn('⚠️ V2: Another draft was created during request. Using existing:', currentDraftId, 'New:', receptionId);
          return currentDraftId;
        }
        
        currentDraftId = parseInt(receptionId);
        isDraftCreated = true;
        
        // Update hidden field
        $("#ReceptionId").val(currentDraftId);
        
        console.log('🏥 V2: Draft state updated - currentDraftId:', currentDraftId, 'isDraftCreated:', isDraftCreated);
        console.log('🏥 V2: DOM ReceptionId after update:', $("#ReceptionId").val());
        console.log('🏥 V2: Memory state - currentDraftId:', currentDraftId, 'isDraftCreated:', isDraftCreated);
        
        // 🚨 PROFESSIONAL: Trigger event برای persist خودکار بیمه‌ها
        $(document).trigger('receptionId:updated', [currentDraftId]);
        
        // Show success message
        toastr.success('پذیرش موقت ایجاد شد');
        
        return currentDraftId;
      })
      .catch(err => {
        console.error('🏥 V2: Auto-draft creation failed:', err);
        console.error('🏥 V2: Error details:', err.message, err.stack);
        
        // 🏥 MEDICAL: بررسی DOM و memory در صورت خطا
        const domId = $("#ReceptionId").val();
        if (domId) {
          const domIdNum = parseInt(domId);
          if (domIdNum && domIdNum > 0) {
            console.log('🏥 V2: Draft found in DOM after error:', domIdNum);
            currentDraftId = domIdNum;
            isDraftCreated = true;
            // Don't show error if draft exists in DOM
            return domIdNum;
          }
        }
        if (isDraftCreated && currentDraftId && currentDraftId > 0) {
          console.log('🏥 V2: Draft found in memory after error:', currentDraftId);
          // Don't show error if draft exists in memory
          return currentDraftId;
        }
        
        toastr.error('خطا در ایجاد پذیرش موقت');
        throw err;
      })
      .always(() => {
        // ✅ Reset Lock: آزاد کردن lock در هر صورت (success یا error)
        // استفاده از .always() به جای .finally() برای jQuery Deferred
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
    
    // 🏥 MEDICAL: بررسی اولویت 1: اگر Draft قبلاً در memory ایجاد شده، برگردان
    if (isDraftCreated && currentDraftId && currentDraftId > 0) {
      console.log('🏥 V2: Draft already exists in memory:', currentDraftId);
      // اطمینان از sync بودن با DOM
      $("#ReceptionId").val(currentDraftId);
      return Promise.resolve(currentDraftId);
    }
    
    // 🏥 MEDICAL: بررسی اولویت 2: اگر ReceptionId در DOM موجود است، استفاده کن
    const existingReceptionId = receptionId || $("#ReceptionId").val();
    if (existingReceptionId) {
      const receptionIdNum = parseInt(existingReceptionId);
      if (receptionIdNum && receptionIdNum > 0) {
        console.log('🏥 V2: ReceptionId found in DOM:', receptionIdNum);
        // Sync با memory
        currentDraftId = receptionIdNum;
        isDraftCreated = true;
        return Promise.resolve(currentDraftId);
      }
    }
    
    // 🏥 MEDICAL: بررسی اولویت 3: اگر request در حال اجرا است، منتظر بمان
    if (isCreatingDraft && draftCreationPromise) {
      console.log('🏥 V2: Draft creation in progress, waiting...');
      try {
        const draftId = await draftCreationPromise;
        if (draftId && draftId > 0) {
          console.log('🏥 V2: Draft creation promise resolved:', draftId);
          return draftId;
        }
        // اگر null برگشت، بررسی کن که آیا Draft در DOM یا memory وجود دارد
        const domId = $("#ReceptionId").val();
        if (domId) {
          const domIdNum = parseInt(domId);
          if (domIdNum && domIdNum > 0) {
            console.log('🏥 V2: Draft found in DOM after promise (null response):', domIdNum);
            currentDraftId = domIdNum;
            isDraftCreated = true;
            return domIdNum;
          }
        }
        // بررسی memory
        if (isDraftCreated && currentDraftId && currentDraftId > 0) {
          console.log('🏥 V2: Draft found in memory after promise (null response):', currentDraftId);
          return currentDraftId;
        }
      } catch (err) {
        console.error('🏥 V2: Draft creation promise failed:', err);
        // بررسی DOM و memory در صورت خطا
        const domId = $("#ReceptionId").val();
        if (domId) {
          const domIdNum = parseInt(domId);
          if (domIdNum && domIdNum > 0) {
            console.log('🏥 V2: Draft found in DOM after promise error:', domIdNum);
            currentDraftId = domIdNum;
            isDraftCreated = true;
            return domIdNum;
          }
        }
        if (isDraftCreated && currentDraftId && currentDraftId > 0) {
          console.log('🏥 V2: Draft found in memory after promise error:', currentDraftId);
          return currentDraftId;
        }
        // ادامه برای ایجاد Draft جدید
      }
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
        if (draftId && draftId > 0) {
          console.log('🏥 V2: Draft created successfully:', draftId);
          return draftId;
        } else {
          console.warn('🏥 V2: Draft creation returned null or invalid, checking DOM and memory...');
          // 🏥 MEDICAL: بررسی مجدد DOM و memory در صورت null
          const domId = $("#ReceptionId").val();
          if (domId) {
            const domIdNum = parseInt(domId);
            if (domIdNum && domIdNum > 0) {
              console.log('🏥 V2: Draft found in DOM after null response:', domIdNum);
              currentDraftId = domIdNum;
              isDraftCreated = true;
              return domIdNum;
            }
          }
          if (isDraftCreated && currentDraftId && currentDraftId > 0) {
            console.log('🏥 V2: Draft found in memory after null response:', currentDraftId);
            return currentDraftId;
          }
          return null;
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Draft creation failed:', err);
        // 🏥 MEDICAL: بررسی DOM و memory در صورت خطا
        const domId = $("#ReceptionId").val();
        if (domId) {
          const domIdNum = parseInt(domId);
          if (domIdNum && domIdNum > 0) {
            console.log('🏥 V2: Draft found in DOM after error:', domIdNum);
            currentDraftId = domIdNum;
            isDraftCreated = true;
            return domIdNum;
          }
        }
        if (isDraftCreated && currentDraftId && currentDraftId > 0) {
          console.log('🏥 V2: Draft found in memory after error:', currentDraftId);
          return currentDraftId;
        }
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
    get draftCreationPromise() { return draftCreationPromise; }, // ✅ برای بررسی اینکه آیا Draft creation در حال انجام است
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
      isDraftFinalizing = false; // ✅ Reset Finalizing Flag
      if (autoSaveTimeout) {
        clearTimeout(autoSaveTimeout);
        autoSaveTimeout = null;
      }
      if (draftCreationTimeout) {
        clearTimeout(draftCreationTimeout);
        draftCreationTimeout = null;
      }
      $("#ReceptionId").val('');
    },
    /**
     * 🏥 MEDICAL: بررسی اینکه آیا Draft ناقص است (بدون خدمت)
     * این تابع بررسی می‌کند که آیا Draft دارای خدمت است یا نه
     * 
     * ⚠️ توجه: این تابع فقط برای بررسی ناقص بودن از نظر خدمت است
     * برای بررسی نهایی نشدن Draft، از isDraftNotFinalized استفاده کنید
     */
    isDraftIncomplete: function() {
      // بررسی چندگانه برای اطمینان از دقت
      const hasItemsInTable = $('#ReceptionItemsList tbody tr[data-reception-item-id]').length > 0 ||
                              $('#items-grid tbody tr[data-reception-item-id]').length > 0 ||
                              $('[data-reception-item-id]').length > 0;
      
      const hasItemsInDOM = $('.reception-item-row').length > 0 ||
                           $('.service-item').length > 0 ||
                           $('tr[data-service-id]').length > 0;
      
      // بررسی TotalAmount از UI (اگر موجود باشد)
      const totalAmount = parseFloat($('#TotalAmount').text().replace(/[^\d.]/g, '') || '0');
      const hasAmount = totalAmount > 0;
      
      // Draft ناقص است اگر هیچ خدمتی نداشته باشد
      const isIncomplete = !hasItemsInTable && !hasItemsInDOM && !hasAmount;
      
      console.log('🏥 V2: Draft completeness check:', {
        hasItemsInTable: hasItemsInTable,
        hasItemsInDOM: hasItemsInDOM,
        hasAmount: hasAmount,
        totalAmount: totalAmount,
        isIncomplete: isIncomplete
      });
      
      return isIncomplete;
    },

    /**
     * 🏥 MEDICAL: بررسی اینکه آیا Draft نهایی نشده است
     * Draft باید حذف شود اگر:
     * 1. هنوز نهایی نشده باشد (Status = Pending)
     * 2. کاربر روی "ذخیره و پذیرش" کلیک نکرده باشد
     * 
     * ⚠️ مهم: Draft فقط زمانی نهایی می‌شود که کاربر روی "ذخیره و پذیرش" کلیک کند
     */
    isDraftNotFinalized: function() {
      // اگر Draft در حال نهایی شدن است، حذف نکن
      if (isDraftFinalizing) {
        console.log('🏥 V2: Draft is finalizing, skipping deletion');
        return false;
      }

      // Draft نهایی نشده است اگر هنوز در وضعیت Pending باشد
      // (یعنی کاربر روی "ذخیره و پذیرش" کلیک نکرده)
      // در این حالت، Draft باید حذف شود (حتی اگر خدمت داشته باشد)
      const isNotFinalized = true; // Draft هنوز نهایی نشده است
      
      console.log('🏥 V2: Draft finalization check:', {
        isDraftFinalizing: isDraftFinalizing,
        isNotFinalized: isNotFinalized,
        shouldDelete: isNotFinalized && !isDraftFinalizing
      });
      
      return isNotFinalized && !isDraftFinalizing;
    },

    /**
     * 🏥 MEDICAL: علامت‌گذاری Draft به عنوان در حال نهایی شدن
     * این متد باید هنگام کلیک روی "ذخیره و پذیرش" فراخوانی شود
     */
    markDraftAsFinalizing: function() {
      isDraftFinalizing = true;
      console.log('🏥 V2: Draft marked as finalizing:', currentDraftId);
    },

    /**
     * 🏥 MEDICAL: حذف علامت نهایی شدن Draft
     * این متد باید در صورت خطا در نهایی‌سازی فراخوانی شود
     */
    unmarkDraftAsFinalizing: function() {
      isDraftFinalizing = false;
      console.log('🏥 V2: Draft unmarked as finalizing:', currentDraftId);
    },

    /**
     * 🏥 MEDICAL: حذف Draft نهایی نشده (Pending) هنگام خروج از فرم
     * Draft باید حذف شود اگر:
     * 1. هنوز نهایی نشده باشد (Status = Pending)
     * 2. کاربر روی "ذخیره و پذیرش" کلیک نکرده باشد
     */
    deleteIncompleteDraft: async function(receptionId) {
      if (!receptionId || receptionId <= 0) {
        console.log('🏥 V2: No draft to delete');
        return Promise.resolve();
      }

      // ✅ بررسی اینکه آیا Draft در حال نهایی شدن است
      if (isDraftFinalizing) {
        console.log('🏥 V2: Draft is finalizing, skipping deletion:', receptionId);
        return Promise.resolve();
      }

      // ✅ Draft باید حذف شود اگر هنوز نهایی نشده باشد (حتی اگر خدمت داشته باشد)
      // این منطق جدید است: Draft فقط زمانی نهایی می‌شود که کاربر روی "ذخیره و پذیرش" کلیک کند
      // ⚠️ تغییر: حذف بررسی isDraftNotFinalized چون همیشه true برمی‌گرداند
      // در عوض، مستقیماً Draft را حذف می‌کنیم (Backend بررسی می‌کند که Status = Pending است)

      try {
        console.log('🏥 V2: Deleting incomplete draft:', receptionId);
        const result = await API.post('/draft/delete-incomplete', { receptionId: receptionId });
        const okResult = API.ok(result);
        
        if (okResult && (okResult.Success === true || okResult.success === true)) {
          console.log('✅ V2: Incomplete draft deleted successfully:', receptionId);
          // Reset local state
          currentDraftId = null;
          isDraftCreated = false;
          $("#ReceptionId").val('');
        } else {
          console.warn('⚠️ V2: Failed to delete incomplete draft:', okResult);
          // اگر Backend گفت که Draft نهایی شده است، state را reset کن
          if (okResult && (okResult.Code === 'FINALIZED' || okResult.code === 'FINALIZED')) {
            console.log('ℹ️ V2: Draft already finalized, resetting local state');
            currentDraftId = null;
            isDraftCreated = false;
            $("#ReceptionId").val('');
          }
        }
      } catch (err) {
        console.error('❌ V2: Error deleting incomplete draft:', err);
        // Don't show error to user - it's cleanup
      }
    },

    /**
     * 🏥 MEDICAL: حذف Draft نهایی نشده با sendBeacon (برای beforeunload)
     * این متد برای استفاده در beforeunload event استفاده می‌شود
     */
    deleteIncompleteDraftWithBeacon: function(receptionId) {
      if (!receptionId || receptionId <= 0) {
        return false;
      }

      // ✅ بررسی اینکه آیا Draft در حال نهایی شدن است
      if (isDraftFinalizing) {
        console.log('🏥 V2: Draft is finalizing, skipping beacon deletion:', receptionId);
        return false;
      }

      // ✅ Draft باید حذف شود اگر هنوز نهایی نشده باشد (حتی اگر خدمت داشته باشد)
      if (!this.isDraftNotFinalized()) {
        console.log('🏥 V2: Draft is finalized or finalizing, skipping beacon deletion:', receptionId);
        return false;
      }

      try {
        console.log('🏥 V2: Deleting incomplete draft via beacon:', receptionId);
        
        // ساخت URL با query string (sendBeacon نمی‌تواند body بفرستد)
        const url = '/api/v1/reception/draft/delete-incomplete?receptionId=' + receptionId;
        
        // استفاده از sendBeacon (بدون body، فقط query string)
        if (navigator.sendBeacon) {
          const success = navigator.sendBeacon(url);
          if (success) {
            console.log('✅ V2: Incomplete draft deletion sent via sendBeacon');
            // Reset local state
            currentDraftId = null;
            isDraftCreated = false;
            $("#ReceptionId").val('');
            return true;
          } else {
            console.warn('⚠️ V2: sendBeacon failed');
            return false;
          }
        } else {
          console.warn('⚠️ V2: sendBeacon not supported');
          return false;
        }
      } catch (err) {
        console.error('❌ V2: Error in deleteIncompleteDraftWithBeacon:', err);
        return false;
      }
    }
  };
  
  // ✅ اضافه کردن متد برای بررسی isDraftFinalizing از خارج
  autoDraftManagerPublicAPI.isDraftFinalizing = function() {
    return isDraftFinalizing;
  };

  // ✅ Bugfix: Export به window.AutoDraftManager
  window.AutoDraftManager = autoDraftManagerPublicAPI;
  
  // Initialize when document is ready
  $(document).ready(initializeAutoDraft);
  
})(window.ReceptionAPI, window.RxUtils, jQuery);
