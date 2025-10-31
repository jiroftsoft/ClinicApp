(function($, API, U) {
  'use strict';

  // References to form fields
  const $nc = $('#Patient_NationalCode');
  const $pid = $('#Patient_PatientId');
  const $fn = $('#firstName');
  const $ln = $('#lastName');
  const $fa = $('#fatherName');
  const $mb = $('#mobile');
  const $ph = $('#phone');
  const $ad = $('#address');
  const $gd = $('#gender');
  const $bd = $('#birthSh');

  // Edit buttons
  const $btnE = $('#btnEditIdentity');
  const $btnS = $('#btnSaveIdentity');
  const $btnC = $('#btnCancelIdentity');

  let cache = null; // برای انصراف

  /**
   * تنظیم حالت ReadOnly/Editable
   */
  function setReadonly(ro) {
    [$fn, $ln, $fa, $mb, $ph, $ad, $bd].forEach($x => $x.prop('readonly', ro));
    $gd.prop('disabled', ro);
    $btnE.toggleClass('d-none', !ro);
    $btnS.toggleClass('d-none', ro);
    $btnC.toggleClass('d-none', ro);
  }

  /**
   * پر کردن فیلدهای هویتی از DTO
   * پشتیبانی از camelCase و PascalCase
   */
  function fillIdentity(identity) {
    if (!identity) return;
    
    // پشتیبانی از camelCase و PascalCase
    const pid = identity.patientId || identity.PatientId;
    const fn = identity.firstName || identity.FirstName;
    const ln = identity.lastName || identity.LastName;
    const fa = identity.fatherName || identity.FatherName;
    const mb = identity.mobile || identity.Mobile;
    const ph = identity.phone || identity.Phone;
    const ad = identity.address || identity.Address;
    const gd = identity.gender || identity.Gender;
    const bd = identity.birthDateShamsi || identity.BirthDateShamsi;
    
    $pid.val(pid || '');
    $fn.val(fn || '');
    $ln.val(ln || '');
    $fa.val(fa || '');
    $mb.val(mb || '');
    $ph.val(ph || '');
    $ad.val(ad || '');
    $gd.val(gd || '');
    $bd.val(bd || '');
  }

  /**
   * جستجوی بیمار بر اساس کد ملی
   */
  function lookup() {
    const nc = ($nc.val() || '').trim();
    
    // اعتبارسنجی کد ملی
    if (!/^\d{10}$/.test(nc)) {
      console.warn('🏥 V2: کد ملی نامعتبر:', nc);
      toastr.warning('کد ملی باید 10 رقم باشد');
      return;
    }

    console.log('🏥 V2: جستجوی بیمار - کد ملی:', nc);

    API.post('/patient/lookup-or-create', { NationalCode: nc })
      .then(function(fullResponse) {
        // Log کامل response برای دیباگ
        console.log('🏥 V2: Full API response (raw):', fullResponse);
        console.log('🏥 V2: Response type:', typeof fullResponse);
        
        // اگر response به صورت string است، آن را parse کن
        let responseObj = fullResponse;
        if (typeof fullResponse === 'string') {
          try {
            responseObj = JSON.parse(fullResponse);
            console.log('🏥 V2: Response parsed from string:', responseObj);
          } catch (e) {
            console.error('🏥 V2: Failed to parse JSON response:', e);
            toastr.error('خطا در پردازش پاسخ سرور');
            return;
          }
        }
        
        console.log('🏥 V2: Response keys:', responseObj ? Object.keys(responseObj) : 'null/undefined');
        
        // چک Success - پشتیبانی از Success و success (camelCase/PascalCase)
        // همچنین چک می‌کنیم که آیا Success به صورت true (boolean) یا "true" (string) برگردانده شده
        const successValue = responseObj?.Success ?? responseObj?.success;
        const isSuccess = successValue === true || successValue === "true" || successValue === 1;
        
        console.log('🏥 V2: Success check:', {
          'responseObj.Success': responseObj?.Success,
          'responseObj.success': responseObj?.success,
          'successValue': successValue,
          'isSuccess': isSuccess,
          'typeof successValue': typeof successValue
        });
        
        if (!responseObj || !isSuccess) {
          const errorCode = responseObj?.Code || responseObj?.code;
          const errorMsg = responseObj?.Message || responseObj?.message || 'بیمار یافت نشد';
          
          console.warn('🏥 V2: Patient lookup failed:', errorCode, errorMsg, responseObj);
          
          // اگر NOT_FOUND است، Modal را باز کن
          if (errorCode === 'NOT_FOUND' || errorCode === 'NotFound') {
            console.log('🏥 V2: Patient not found, opening Fast Create Modal...');
            openFastCreateModal(nc);
          } else {
            toastr.error(errorMsg);
          }
          return;
        }

        // دریافت Data (API.ok() آن را extract می‌کند)
        const dto = API.ok(responseObj);
        console.log('🏥 V2: Patient lookup data (extracted):', dto);
        console.log('🏥 V2: Data type:', typeof dto);
        console.log('🏥 V2: Data keys:', dto ? Object.keys(dto) : 'null/undefined');

        // ذخیره کپی برای انصراف
        const identity = dto?.Identity || dto?.identity;
        console.log('🏥 V2: Identity extracted:', identity);
        
        if (identity) {
          cache = JSON.parse(JSON.stringify(identity));
          
          // پر کردن اطلاعات هویتی
          fillIdentity(identity);
          console.log('🏥 V2: Identity filled to form');
          
          // تنظیم بیمه‌ها
          const insurance = dto?.Insurance || dto?.insurance;
          console.log('🏥 V2: Insurance extracted:', insurance);
          
          if (window.insPanel && insurance) {
            window.insPanel.set(insurance);
            console.log('🏥 V2: Insurance set to panel');
          }

          setReadonly(true);
          toastr.success('اطلاعات بیمار بارگذاری شد');
        } else {
          console.warn('🏥 V2: Identity not found in response data. DTO:', dto);
          toastr.warning('اطلاعات هویتی یافت نشد');
        }

        // Trigger auto-draft creation
        if (window.AutoDraftManager) {
          window.AutoDraftManager.createDraft().catch(err => {
            console.error('🏥 V2: Auto-draft creation error:', err);
          });
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Patient lookup error:', err);
        toastr.error('خطا در جستجوی بیمار');
      });
  }

  /**
   * باز کردن Modal ثبت سریع بیمار
   * @param {string} nc - کد ملی (اختیاری)
   */
  function openFastCreateModal(nc) {
    console.log('🏥 V2: Opening Fast Create Modal, NationalCode:', nc || 'empty');
    
    // لود کردن لیست پلن‌های بیمه برای dropdowns در Modal
    if (window.insPanel && window.insPanel.loadPlans) {
      window.insPanel.loadPlans()
        .then(function(plansData) {
          console.log('🏥 V2: Insurance plans loaded for modal:', plansData);
          
          // Fill base insurance plans in modal
          const $fcBasePlan = $('#fc_basePlanId');
          $fcBasePlan.empty().append('<option value="">انتخاب کنید</option>');
          if (plansData.basePlans) {
            plansData.basePlans.forEach(function(plan) {
              const planId = plan.insurancePlanId || plan.insuranceId;
              const planName = plan.name || plan.insuranceName;
              const coverage = plan.coveragePercent || plan.coveragePercentage;
              $fcBasePlan.append(`<option value="${planId}">${planName} (${coverage}%)</option>`);
            });
          }
          
          // Fill supplementary insurance plans in modal
          const $fcSuppPlan = $('#fc_suppPlanId');
          $fcSuppPlan.empty().append('<option value="">انتخاب کنید</option>');
          if (plansData.supplementaryPlans) {
            plansData.supplementaryPlans.forEach(function(plan) {
              const planId = plan.insurancePlanId || plan.insuranceId;
              const planName = plan.name || plan.insuranceName;
              const coverage = plan.coveragePercent || plan.coveragePercentage;
              $fcSuppPlan.append(`<option value="${planId}">${planName} (${coverage}%)</option>`);
            });
          }
          
          // Set national code if provided
          if (nc) {
            $('#fc_nationalCode').val(nc);
          }
          
          // Initialize Persian DatePicker for birth date
          const $fcBirth = $('#fc_birth');
          if (typeof $.fn.persianDatepicker !== 'undefined' && !$fcBirth.data('persian-datepicker-initialized')) {
            $fcBirth.persianDatepicker({
              observer: true,
              format: 'YYYY/MM/DD',
              altField: '#fc_birth',
              altFormat: 'YYYY/MM/DD'
            });
            $fcBirth.data('persian-datepicker-initialized', true);
          }
          
          // Show modal
          const modal = new bootstrap.Modal(document.getElementById('patientFastCreateModal'));
          modal.show();
          
          // Focus on first name field after modal is shown
          $('#patientFastCreateModal').on('shown.bs.modal', function() {
            $('#fc_firstName').focus();
          });
        })
        .catch(function(err) {
          console.error('🏥 V2: Error loading insurance plans for modal:', err);
          toastr.warning('خطا در بارگذاری لیست بیمه‌ها');
          
          // Show modal anyway
          if (nc) {
            $('#fc_nationalCode').val(nc);
          }
          const modal = new bootstrap.Modal(document.getElementById('patientFastCreateModal'));
          modal.show();
        });
    } else {
      // اگر insPanel موجود نیست، Modal را بدون بیمه‌ها نشان بده
      if (nc) {
        $('#fc_nationalCode').val(nc);
      }
      const modal = new bootstrap.Modal(document.getElementById('patientFastCreateModal'));
      modal.show();
    }
  }

  /**
   * Submit فرم ثبت سریع بیمار
   */
  function submitFastCreate() {
    console.log('🏥 V2: Submitting Fast Create form...');
    
    // Validation
    const nc = ($('#fc_nationalCode').val() || '').trim();
    const fn = ($('#fc_firstName').val() || '').trim();
    const ln = ($('#fc_lastName').val() || '').trim();
    const mb = ($('#fc_mobile').val() || '').trim();
    
    if (!/^\d{10}$/.test(nc)) {
      toastr.error('کد ملی باید 10 رقم باشد');
      $('#fc_nationalCode').addClass('is-invalid').focus();
      return;
    }
    
    if (!fn) {
      toastr.error('نام الزامی است');
      $('#fc_firstName').addClass('is-invalid').focus();
      return;
    }
    
    if (!ln) {
      toastr.error('نام خانوادگی الزامی است');
      $('#fc_lastName').addClass('is-invalid').focus();
      return;
    }
    
    if (!mb || !/^09\d{9}$/.test(mb)) {
      toastr.error('شماره موبایل باید 11 رقم و با 09 شروع شود');
      $('#fc_mobile').addClass('is-invalid').focus();
      return;
    }
    
    // Remove invalid classes
    $('#patientFastCreateForm input, #patientFastCreateForm select').removeClass('is-invalid');
    
    // Prepare payload
    const payload = {
      NationalCode: nc,
      FirstName: fn,
      LastName: ln,
      Mobile: mb,
      Gender: $('#fc_gender').val() || null,
      BirthDateShamsi: $('#fc_birth').val() || null,
      Address: $('#fc_address').val() || null,
      BaseInsurancePlanId: $('#fc_basePlanId').val() ? parseInt($('#fc_basePlanId').val()) : null,
      SupplementaryInsurancePlanId: $('#fc_suppPlanId').val() ? parseInt($('#fc_suppPlanId').val()) : null
    };
    
    console.log('🏥 V2: Fast Create payload:', payload);
    
    // Disable save button
    const $btnSave = $('#btnFastCreateSave');
    $btnSave.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>در حال ثبت...');
    
    // Submit
    API.post('/patient/lookup-or-create', payload)
      .then(function(fullResponse) {
        console.log('🏥 V2: Fast Create response:', fullResponse);
        
        // Parse response if string
        let responseObj = fullResponse;
        if (typeof fullResponse === 'string') {
          try {
            responseObj = JSON.parse(fullResponse);
          } catch (e) {
            console.error('🏥 V2: Failed to parse JSON response:', e);
            toastr.error('خطا در پردازش پاسخ سرور');
            return;
          }
        }
        
        // Check success
        const successValue = responseObj?.Success ?? responseObj?.success;
        const isSuccess = successValue === true || successValue === "true" || successValue === 1;
        
        if (!responseObj || !isSuccess) {
          const errorMsg = responseObj?.Message || responseObj?.message || 'خطا در ثبت سریع بیمار';
          console.error('🏥 V2: Fast Create failed:', errorMsg, responseObj);
          toastr.error(errorMsg);
          return;
        }
        
        // Extract data
        const dto = API.ok(responseObj);
        const identity = dto?.Identity || dto?.identity;
        const insurance = dto?.Insurance || dto?.insurance;
        
        console.log('🏥 V2: Fast Create success - Identity:', identity, 'Insurance:', insurance);
        
        if (identity) {
          // Fill form with patient data
          fillIdentity(identity);
          cache = JSON.parse(JSON.stringify(identity));
          
          // Set insurances if provided
          if (window.insPanel && insurance) {
            window.insPanel.set(insurance);
          }
          
          // Set readonly
          setReadonly(true);
          
          // Hide modal
          const modal = bootstrap.Modal.getInstance(document.getElementById('patientFastCreateModal'));
          if (modal) {
            modal.hide();
          }
          
          toastr.success('بیمار با موفقیت ثبت شد');
          
          // Trigger auto-draft creation
          if (window.AutoDraftManager) {
            window.AutoDraftManager.createDraft().catch(err => {
              console.error('🏥 V2: Auto-draft creation error:', err);
            });
          }
          
          // اگر DraftId موجود است و بیمه‌ها تغییر کرده، Reprice کن
          const receptionId = $('#ReceptionId').val();
          if (receptionId && receptionId > 0 && insurance) {
            console.log('🏥 V2: Draft exists, triggering Reprice after insurance change...');
            // insurance-panel.js به صورت خودکار Reprice می‌کند وقتی set() فراخوانی می‌شود
          }
        } else {
          console.warn('🏥 V2: Fast Create success but Identity is missing');
          toastr.warning('بیمار ثبت شد اما اطلاعات هویتی یافت نشد');
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Fast Create error:', err);
        toastr.error('خطا در ثبت سریع بیمار');
      })
      .finally(function() {
        // Re-enable save button
        $btnSave.prop('disabled', false).html('<i class="fas fa-save me-2"></i>ثبت و ادامه پذیرش');
      });
  }

  /**
   * ذخیره اطلاعات ویرایش شده بیمار
   */
  function save() {
    const patientId = parseInt($pid.val());
    if (!patientId || patientId <= 0) {
      toastr.warning('ابتدا بیمار را پیدا کنید');
      return;
    }

    const payload = {
      patientId: patientId,
      firstName: $fn.val()?.trim(),
      lastName: $ln.val()?.trim(),
      fatherName: $fa.val()?.trim() || null,
      mobile: $mb.val()?.trim(),
      phone: $ph.val()?.trim() || null,
      address: $ad.val()?.trim() || null,
      gender: $gd.val() || null,
      birthDateShamsi: $bd.val()?.trim() || null
    };

    console.log('🏥 V2: ذخیره اطلاعات بیمار:', payload);

    API.post('/patient/update-basic', payload)
      .then(function(fullResponse) {
        // چک Success
        if (!fullResponse || (fullResponse.Success !== true && fullResponse.success !== true)) {
          toastr.error(fullResponse?.Message || fullResponse?.message || 'خطا در ذخیره');
          return;
        }

        const updated = API.ok(fullResponse);
        fillIdentity(updated.Identity || updated.identity || updated);
        cache = JSON.parse(JSON.stringify(updated.Identity || updated.identity || updated));
        setReadonly(true);
        toastr.success('اطلاعات به‌روزرسانی شد');
      })
      .catch(function(err) {
        console.error('🏥 V2: Update patient error:', err);
        toastr.error('خطا در به‌روزرسانی اطلاعات');
      });
  }

  /**
   * لغو ویرایش و بازگشت به مقادیر قبلی
   */
  function cancelEdit() {
    if (cache) {
      fillIdentity(cache);
    }
    setReadonly(true);
    toastr.info('ویرایش لغو شد');
  }

  // Event handlers
  $('#BtnPatientLookup').on('click', lookup);
  
  $nc.on('keypress', function(e) {
    if (e.key === 'Enter') {
      e.preventDefault();
      lookup();
    }
  });

  $nc.on('blur', function() {
    const nc = ($nc.val() || '').trim();
    if (/^\d{10}$/.test(nc) && !$pid.val()) {
      lookup();
    }
  });

  $btnE.on('click', function() {
    setReadonly(false);
    toastr.info('حالت ویرایش فعال شد');
  });

  $btnS.on('click', save);

  $btnC.on('click', cancelEdit);

  // Initialize - اگر PatientId وجود دارد، اطلاعات را لود کن
  $(document).ready(function() {
    const patientId = parseInt($pid.val());
    const nc = $nc.val()?.trim();
    
    if (patientId > 0) {
      // اگر PatientId وجود دارد، اطلاعات را از API بگیر
      API.post('/patient/lookup-or-create', { NationalCode: nc || '' })
        .then(function(fullResponse) {
          // چک Success
          if (fullResponse && (fullResponse.Success === true || fullResponse.success === true)) {
            const dto = API.ok(fullResponse);
            const identity = dto.Identity || dto.identity;
            const insurance = dto.Insurance || dto.insurance;
            
            if (identity) {
              fillIdentity(identity);
            }
            if (window.insPanel && insurance) {
              window.insPanel.set(insurance);
            }
            setReadonly(true);
          }
        })
        .catch(function(err) {
          console.warn('🏥 V2: Failed to load patient data on init:', err);
        });
    }
  });

})(jQuery, window.ReceptionAPI, window.RxUtils);
