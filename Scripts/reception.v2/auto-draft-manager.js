(function(API, U, $) {
  'use strict';
  
  let currentDraftId = null;
  let autoSaveTimeout = null;
  let isDraftCreated = false;
  
  // Auto-draft creation when user starts entering data
  function createAutoDraft() {
    if (isDraftCreated) return Promise.resolve(currentDraftId);
    
    const patientId = $("#Patient_PatientId").val();
    const nationalCode = $("#Patient_NationalCode").val();
    
    if (!patientId && !nationalCode) {
      console.log('🏥 V2: No patient data yet, skipping auto-draft creation');
      return Promise.resolve(null);
    }
    
    console.log('🏥 V2: Creating auto-draft...');
    
    const payload = {
      patientId: patientId || null,
      nationalCode: nationalCode || null,
      clinicId: $("#ClinicId").val() || 1, // Default clinic
      departmentId: $("#DepartmentId").val() || null,
      doctorId: $("#DoctorId").val() || null,
      financialYear: (window.ReceptionBootstrap && window.ReceptionBootstrap.FinancialYear) || 1404
    };
    
    return API.post("/draft/create", payload)
      .then(API.ok)
      .then(d => {
        console.log('🏥 V2: Auto-draft created:', d);
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
      });
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
    
    // Auto-create draft when patient data is entered
    $(document).on('blur', '#Patient_NationalCode, #Patient_FullName, #Patient_Mobile', function() {
      const nationalCode = $("#Patient_NationalCode").val();
      const fullName = $("#Patient_FullName").val();
      const mobile = $("#Patient_Mobile").val();
      
      if ((nationalCode && nationalCode.length >= 10) || fullName || mobile) {
        createAutoDraft().catch(err => {
          console.error('🏥 V2: Auto-draft creation error:', err);
        });
      }
    });
    
    // Auto-save on form changes
    $(document).on('input change', '.reception-pro :input', function() {
      if (isDraftCreated) {
        autoSave();
      }
    });
    
    // Auto-create draft when clinic/department is selected
    $(document).on('change', '#ClinicId, #DepartmentId, #DoctorId', function() {
      if (isDraftCreated) {
        autoSave();
      } else {
        // Try to create draft if we have patient data
        const nationalCode = $("#Patient_NationalCode").val();
        const fullName = $("#Patient_FullName").val();
        const mobile = $("#Patient_Mobile").val();
        
        if ((nationalCode && nationalCode.length >= 10) || fullName || mobile) {
          createAutoDraft().catch(err => {
            console.error('🏥 V2: Auto-draft creation error:', err);
          });
        }
      }
    });
    
    // Auto-create draft when insurance is selected
    $(document).on('change', '#BasePlanId, #SuppPlanId', function() {
      if (isDraftCreated) {
        autoSave();
      } else {
        // Try to create draft if we have patient data
        const nationalCode = $("#Patient_NationalCode").val();
        const fullName = $("#Patient_FullName").val();
        const mobile = $("#Patient_Mobile").val();
        
        if ((nationalCode && nationalCode.length >= 10) || fullName || mobile) {
          createAutoDraft().catch(err => {
            console.error('🏥 V2: Auto-draft creation error:', err);
          });
        }
      }
    });
    
    console.log('🏥 V2: Auto-draft system initialized');
  }
  
  // Public API
  window.AutoDraftManager = {
    createDraft: createAutoDraft,
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
      if (autoSaveTimeout) {
        clearTimeout(autoSaveTimeout);
        autoSaveTimeout = null;
      }
      $("#ReceptionId").val('');
    }
  };
  
  // Initialize when document is ready
  $(document).ready(initializeAutoDraft);
  
})(window.ReceptionAPI, window.RxUtils, jQuery);
