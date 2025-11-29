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

  // ❌ هیچ cache داده - فقط برای انصراف از ویرایش (UI state)
  let cancelCache = null; // برای انصراف از ویرایش (نه cache داده‌ها)

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

  function showFastCreateValidationErrors(responseObj) {
    const validationErrors = responseObj?.ValidationErrors || responseObj?.validationErrors || [];
    const fieldNameMap = {
      'NationalCode': 'کد ملی',
      'FirstName': 'نام',
      'LastName': 'نام خانوادگی',
      'FatherName': 'نام پدر',
      'Mobile': 'موبایل',
      'BirthDateShamsi': 'تاریخ تولد',
      'Address': 'آدرس',
      'PhoneNumber': 'موبایل',
      'UserName': 'کد ملی / حساب کاربری',
      'Identity': 'حساب کاربری',
      'Email': 'ایمیل'
    };
    const fieldSelectorMap = {
      'NationalCode': '#fc_nationalCode',
      'FirstName': '#fc_firstName',
      'LastName': '#fc_lastName',
      'FatherName': '#fc_fatherName',
      'Mobile': '#fc_mobile',
      'BirthDateShamsi': '#fc_birth',
      'Address': '#fc_address',
      'PhoneNumber': '#fc_mobile',
      'UserName': '#fc_nationalCode',
      'Identity': '#fc_nationalCode',
      'Email': '#fc_email'
    };

    let handled = false;

    if (Array.isArray(validationErrors) && validationErrors.length > 0) {
      handled = true;
      validationErrors.forEach(function(err) {
        const field = err.Field || err.field || '';
        const message = err.ErrorMessage || err.Message || err.message || err.errorMessage || '';
        const fieldName = fieldNameMap[field] || field || 'فیلد';
        const displayMessage = message || 'مقدار نامعتبر است';
        const selector = fieldSelectorMap[field];

        if (selector) {
          $(selector).addClass('is-invalid');
        }

        toastr.error(`${fieldName}: ${displayMessage}`, 'خطای اعتبارسنجی', {
          timeOut: 5000,
          positionClass: 'toast-top-center',
          closeButton: true
        });
      });
    }

    return handled;
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
   * ✅ Realtime - هیچ cache استفاده نمی‌شود
   */
  function lookup() {
    const nc = ($nc.val() || '').trim();
    
    // اعتبارسنجی کد ملی
    if (!/^\d{10}$/.test(nc)) {
      console.warn('🏥 V2: کد ملی نامعتبر:', nc);
      toastr.warning('کد ملی باید 10 رقم باشد');
      return $.Deferred().reject('Invalid national code').promise();
    }

    console.log('🏥 V2: جستجوی بیمار - کد ملی:', nc);

    // ✅ استفاده از jQuery Deferred API برای سازگاری بهتر
    // ❌ هیچ cache - همیشه realtime query
    const lookupRequest = API.post('/patient/lookup-or-create', { NationalCode: nc });
    
    lookupRequest.done(function(fullResponse) {
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
            cancelCache = JSON.parse(JSON.stringify(identity)); // فقط برای انصراف از ویرایش
            
            // پر کردن اطلاعات هویتی
            fillIdentity(identity);
            console.log('🏥 V2: Identity filled to form');
            
            // ✅ Trigger state change event for Summary Header
            const patientData = {
              PatientId: identity.PatientId || identity.patientId,
              NationalCode: identity.NationalCode || identity.nationalCode,
              FirstName: identity.FirstName || identity.firstName,
              LastName: identity.LastName || identity.lastName,
              Gender: identity.Gender || identity.gender,
              GenderTitle: identity.Gender === 'Male' || identity.gender === 'Male' ? 'مرد' : (identity.Gender === 'Female' || identity.gender === 'Female' ? 'زن' : ''),
              BirthDate: identity.BirthDate || identity.birthDate,
              BirthDateIso: identity.BirthDate || identity.birthDate,
              BirthDateShamsi: identity.BirthDateShamsi || identity.birthDateShamsi,
              Address: identity.Address || identity.address,
              Mobile: identity.Mobile || identity.mobile
            };
            
            $(document).trigger('rv2:stateChanged', { patient: patientData });
            
            // ✅ Trigger event for Insurance Status Checker (کامپوننت قابل استفاده مجدد)
            $(document).trigger('patient:selected', [patientData]);
            
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
      });
    
    lookupRequest.fail(function(err) {
      console.error('🏥 V2: Patient lookup error:', err);
      
      // بررسی response JSON برای خطاهای خاص
      try {
        if (err.responseJSON) {
          if (API && API.handleErrorJson && typeof API.handleErrorJson === 'function') {
            if (API.handleErrorJson(err.responseJSON)) {
              return; // خطا handle شد
            }
          }
        }
      } catch (e) {
        // Ignore
      }
      
      toastr.error('خطا در جستجوی بیمار');
    });
    
    return lookupRequest; // Return promise برای استفاده در performLookup
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
          const modalElement = document.getElementById('patientFastCreateModal');
          if (!modalElement) {
            console.error('🏥 V2: ❌ patientFastCreateModal not found in DOM!');
            toastr.error('مودال ثبت سریع بیمار یافت نشد');
            return;
          }
          
          const modal = new bootstrap.Modal(modalElement);
          
          // ✅ اطمینان از ثبت event handler بعد از نمایش modal
          $('#patientFastCreateModal').one('shown.bs.modal', function() {
            console.log('🏥 V2: Fast Create Modal shown, ensuring event handlers...');
            
            // بررسی وجود دکمه
            const $btn = $('#btnFastCreateSave');
            if ($btn.length === 0) {
              console.error('🏥 V2: ❌ btnFastCreateSave not found in DOM after modal shown!');
            } else {
              console.log('🏥 V2: ✅ btnFastCreateSave found:', $btn[0]);
              
              // ثبت event handler
              $btn.off('click').on('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                console.log('🏥 V2: Fast Create Save button clicked (from openFastCreateModal)', this);
                submitFastCreate();
                return false;
              });
            }
            
            // Focus on first name field
            $('#fc_firstName').focus();
          });
          
          modal.show();
        })
        .catch(function(err) {
          console.error('🏥 V2: Error loading insurance plans for modal:', err);
          toastr.warning('خطا در بارگذاری لیست بیمه‌ها');
          
          // Show modal anyway
          if (nc) {
            $('#fc_nationalCode').val(nc);
          }
          const modalElement = document.getElementById('patientFastCreateModal');
          if (modalElement) {
            const modal = new bootstrap.Modal(modalElement);
            
            // ✅ اطمینان از ثبت event handler
            $('#patientFastCreateModal').one('shown.bs.modal', function() {
              console.log('🏥 V2: Fast Create Modal shown (fallback), ensuring event handlers...');
              const $btn = $('#btnFastCreateSave');
              if ($btn.length > 0) {
                $btn.off('click').on('click', function(e) {
                  e.preventDefault();
                  e.stopPropagation();
                  console.log('🏥 V2: Fast Create Save button clicked (fallback)', this);
                  submitFastCreate();
                  return false;
                });
              }
              $('#fc_firstName').focus();
            });
            
            modal.show();
          }
        });
    } else {
      // اگر insPanel موجود نیست، Modal را بدون بیمه‌ها نشان بده
      if (nc) {
        $('#fc_nationalCode').val(nc);
      }
      const modalElement = document.getElementById('patientFastCreateModal');
      if (modalElement) {
        const modal = new bootstrap.Modal(modalElement);
        
        // ✅ اطمینان از ثبت event handler
        $('#patientFastCreateModal').one('shown.bs.modal', function() {
          console.log('🏥 V2: Fast Create Modal shown (no insPanel), ensuring event handlers...');
          const $btn = $('#btnFastCreateSave');
          if ($btn.length > 0) {
            $btn.off('click').on('click', function(e) {
              e.preventDefault();
              e.stopPropagation();
              console.log('🏥 V2: Fast Create Save button clicked (no insPanel)', this);
              submitFastCreate();
              return false;
            });
          }
          $('#fc_firstName').focus();
        });
        
        modal.show();
      }
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
    
    // ✅ گام ۴: Prepare payload با کلیدهای دقیق هم‌نام با DTO
    const payload = {
      NationalCode: nc,
      FirstName: fn,
      LastName: ln,
      FatherName: ($('#fc_fatherName').val() || '').trim() || null,
      Mobile: mb,
      Gender: $('#fc_gender').val() || null, // "Male"/"Female"
      BirthDateShamsi: ($('#fc_birth').val() || '').trim() || null, // "yyyy/MM/dd" شمسی
      Address: ($('#fc_address').val() || '').trim() || null,
      BaseInsurancePlanId: $('#fc_basePlanId').val() ? parseInt($('#fc_basePlanId').val(), 10) : null,
      SupplementaryInsurancePlanId: $('#fc_suppPlanId').val() ? parseInt($('#fc_suppPlanId').val(), 10) : null
    };
    
    console.log('🏥 V2: Fast Create payload:', payload);
    
    // Disable save button
    const $btnSave = $('#btnFastCreateSave');
    $btnSave.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>در حال ثبت...');
    
    // ✅ Submit - استفاده از jQuery Deferred API (.done/.fail/.always) به جای Promise API (.then/.catch/.finally)
    const request = API.post('/patient/lookup-or-create', payload);
    
    request.done(function(fullResponse) {
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
      const errorCode = responseObj?.Code || responseObj?.code;

      if (!responseObj || !isSuccess) {
        console.warn('🏥 V2: Fast Create failed - code:', errorCode, 'response:', responseObj);

        if (showFastCreateValidationErrors(responseObj)) {
          return;
        }

        const errorMsg = responseObj?.Message || responseObj?.message || 'خطا در ثبت سریع بیمار';
        toastr.error(errorMsg);
        return;
      }
      
      // ✅ گام ۶: Extract data و handleLookupOrCreateResponse
      const dto = API.ok(responseObj);
      const identity = dto?.Identity || dto?.identity;
      const insurance = dto?.Insurance || dto?.insurance;
      
      // ✅ بررسی خطای بیمه از Metadata
      const metadata = responseObj?.Metadata || responseObj?.metadata || {};
      const insuranceError = metadata?.InsuranceError || metadata?.insuranceError || 
                            (typeof metadata === 'object' && metadata !== null ? (metadata.InsuranceError || metadata.insuranceError) : null);
      
      console.log('🏥 V2: Fast Create success - Identity:', identity, 'Insurance:', insurance, 'InsuranceError:', insuranceError);
      
      if (identity) {
        // Fill form with patient data
        fillIdentity(identity);
        cancelCache = JSON.parse(JSON.stringify(identity)); // فقط برای انصراف از ویرایش
        
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
        
        // ✅ نمایش پیام مناسب با توجه به خطای بیمه
        if (insuranceError) {
          toastr.warning('بیمار ثبت شد اما تنظیم بیمه با خطا مواجه شد: ' + insuranceError, 'هشدار', {
            timeOut: 7000,
            positionClass: 'toast-top-center',
            closeButton: true
          });
        } else {
          toastr.success('بیمار با موفقیت ثبت/بازیابی شد.');
        }
        
        // ✅ Trigger auto-draft creation (الان patientId داریم)
        if (window.AutoDraftManager) {
          window.AutoDraftManager.createDraft().catch(err => {
            console.error('🏥 V2: Auto-draft creation error:', err);
          });
        }
        
        // ✅ اگر DraftId موجود است و بیمه‌ها تغییر کرده، Reprice کن
        const receptionId = $('#ReceptionId').val();
        if (receptionId && receptionId > 0 && insurance) {
          console.log('🏥 V2: Draft exists, triggering Reprice after insurance change...');
          // insurance-panel.js به صورت خودکار Reprice می‌کند وقتی set() فراخوانی می‌شود
        }
      } else {
        console.warn('🏥 V2: Fast Create success but Identity is missing');
        toastr.warning('بیمار ثبت شد اما اطلاعات هویتی یافت نشد');
      }
    });
    
    request.fail(function(err) {
      console.error('🏥 V2: Fast Create error:', err);
      toastr.error('خطا در ثبت سریع بیمار');
    });
    
    request.always(function() {
      // ✅ Re-enable save button - استفاده از .always() برای jQuery Deferred
      // .always() معادل .finally() در jQuery Deferred است و همیشه اجرا می‌شود
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

    // ✅ استفاده از jQuery Deferred API برای سازگاری بهتر
    const updateRequest = API.post('/patient/update-basic', payload);
    
    updateRequest.done(function(fullResponse) {
      // چک Success
      if (!fullResponse || (fullResponse.Success !== true && fullResponse.success !== true)) {
        toastr.error(fullResponse?.Message || fullResponse?.message || 'خطا در ذخیره');
        return;
      }

      const updated = API.ok(fullResponse);
      fillIdentity(updated.Identity || updated.identity || updated);
        cancelCache = JSON.parse(JSON.stringify(updated.Identity || updated.identity || updated)); // فقط برای انصراف از ویرایش
      setReadonly(true);
      toastr.success('اطلاعات به‌روزرسانی شد');
    });
    
    updateRequest.fail(function(err) {
      console.error('🏥 V2: Update patient error:', err);
      toastr.error('خطا در به‌روزرسانی اطلاعات');
    });
  }

  /**
   * لغو ویرایش و بازگشت به مقادیر قبلی
   */
  function cancelEdit() {
    if (cancelCache) {
      fillIdentity(cancelCache);
    }
    setReadonly(true);
    toastr.info('ویرایش لغو شد');
  }

  // ✅ رویکرد حرفه‌ای: Auto-lookup با debounce + Enter key + Blur fallback
  // ❌ هیچ cache - همه چیز realtime برای محیط درمانی
  
  let lookupTimeout = null;
  let isLookingUp = false; // Flag برای جلوگیری از درخواست‌های همزمان
  
  /**
   * Lookup با debounce و loading state
   */
  function triggerLookup() {
    const nc = ($nc.val() || '').trim();
    
    // ✅ اگر فیلد readonly است (edit mode)، lookup نکن
    if ($nc.prop('readonly')) {
      console.log('🏥 V2: National code field is readonly (edit mode), skipping lookup');
      return;
    }
    
    // اگر در حال lookup هستیم، skip کن
    if (isLookingUp) {
      console.warn('🏥 V2: Lookup already in progress, skipping...');
      return;
    }
    
    // Clear timeout قبلی
    if (lookupTimeout) {
      clearTimeout(lookupTimeout);
      lookupTimeout = null;
    }
    
    // اگر کد ملی کامل نیست، skip کن
    if (!/^\d{10}$/.test(nc)) {
      return;
    }
    
    // اگر قبلاً lookup شده (PatientId وجود دارد) و کد ملی تغییر نکرده، skip کن
    if ($pid.val() && $nc.data('last-looked-up') === nc) {
      console.log('🏥 V2: Patient already loaded for this national code, skipping lookup');
      return;
    }
    
    // Debounce: 500ms delay
    lookupTimeout = setTimeout(function() {
      lookupTimeout = null;
      performLookup();
    }, 500);
  }
  
  /**
   * انجام lookup با loading state
   */
  function performLookup() {
    // ✅ اگر فیلد readonly است (edit mode)، lookup نکن
    if ($nc.prop('readonly')) {
      console.log('🏥 V2: National code field is readonly (edit mode), skipping lookup');
      return;
    }
    
    if (isLookingUp) {
      return;
    }
    
    const nc = ($nc.val() || '').trim();
    if (!/^\d{10}$/.test(nc)) {
      return;
    }
    
    isLookingUp = true;
    
    // نمایش loading state
    const $lookupBtn = $('#BtnPatientLookup');
    const originalHtml = $lookupBtn.html();
    $lookupBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i>در حال جستجو...');
    $nc.prop('readonly', true).addClass('is-loading');
    
    // ذخیره کد ملی برای جلوگیری از lookup تکراری
    $nc.data('last-looked-up', nc);
    
    // انجام lookup
    lookup()
      .always(function() {
        // بازگرداندن UI state
        isLookingUp = false;
        $lookupBtn.prop('disabled', false).html(originalHtml);
        $nc.prop('readonly', false).removeClass('is-loading');
      });
  }
  
  // Event handlers
  $('#BtnPatientLookup').on('click', function(e) {
    e.preventDefault();
    if (lookupTimeout) {
      clearTimeout(lookupTimeout);
      lookupTimeout = null;
    }
    performLookup();
  });
  
  // ✅ Auto-lookup با تایپ (debounce 500ms)
  $nc.on('input', function() {
    // Clear data برای lookup مجدد
    $nc.removeData('last-looked-up');
    triggerLookup();
  });
  
  // ✅ Enter key برای lookup فوری
  $nc.on('keypress', function(e) {
    if (e.key === 'Enter' || e.which === 13) {
      e.preventDefault();
      if (lookupTimeout) {
        clearTimeout(lookupTimeout);
        lookupTimeout = null;
      }
      performLookup();
    }
  });

  // ✅ Blur fallback برای سازگاری (فقط اگر lookup نشده باشد)
  $nc.on('blur', function() {
    // ✅ اگر فیلد readonly است (edit mode)، lookup نکن
    if ($nc.prop('readonly')) {
      return;
    }
    
    const nc = ($nc.val() || '').trim();
    if (/^\d{10}$/.test(nc) && !$pid.val() && !isLookingUp) {
      // اگر timeout وجود دارد، آن را cancel کن و lookup کن
      if (lookupTimeout) {
        clearTimeout(lookupTimeout);
        lookupTimeout = null;
      }
      performLookup();
    }
  });

  $btnE.on('click', function() {
    setReadonly(false);
    toastr.info('حالت ویرایش فعال شد');
  });

  $btnS.on('click', save);

  $btnC.on('click', cancelEdit);

  // ✅ جلوگیری از submit فرم در صورت Enter
  $(document).on('submit', '#patientFastCreateForm', function(e) {
    e.preventDefault();
    e.stopPropagation();
    console.log('🏥 V2: Fast Create Form submit prevented');
    return false;
  });

  // ✅ Event handler for Fast Create Save button - چند لایه برای اطمینان
  // روش ۱: Event delegation روی document با data-action (برای عناصر پویا)
  $(document).on('click', '[data-action="submit-fast-create"], #btnFastCreateSave', function(e) {
    e.preventDefault();
    e.stopPropagation();
    e.stopImmediatePropagation();
    console.log('🏥 V2: Fast Create Save button clicked (via delegation)', e.target);
    submitFastCreate();
    return false;
  });
  
  // روش ۲: مستقیماً روی دکمه (اگر در DOM موجود باشد)
  $(document).ready(function() {
    $('#btnFastCreateSave').on('click', function(e) {
      e.preventDefault();
      e.stopPropagation();
      console.log('🏥 V2: Fast Create Save button clicked (direct)', this);
      submitFastCreate();
      return false;
    });
    
    // روش ۳: بعد از نمایش modal (برای Bootstrap modal events)
    $('#patientFastCreateModal').on('shown.bs.modal', function() {
      console.log('🏥 V2: Fast Create Modal shown, re-attaching event handlers');
      var $btn = $('#btnFastCreateSave');
      $btn.off('click').on('click', function(e) {
        e.preventDefault();
        e.stopPropagation();
        console.log('🏥 V2: Fast Create Save button clicked (after modal shown)', this);
        submitFastCreate();
        return false;
      });
      
      // تست: بررسی وجود دکمه
      if ($btn.length === 0) {
        console.error('🏥 V2: ❌ btnFastCreateSave not found in DOM!');
      } else {
        console.log('🏥 V2: ✅ btnFastCreateSave found:', $btn[0]);
      }
    });

    $('#patientFastCreateModal').on('hidden.bs.modal', function() {
      console.log('🏥 V2: Fast Create Modal hidden');
      const $btn = $('#btnFastCreateSave');
      $btn.prop('disabled', false)
          .html('<i class="fas fa-save me-2"></i>ثبت و ادامه پذیرش');

      const $form = $('#patientFastCreateForm');
      if ($form.length > 0) {
        $form.trigger('reset');
        $form.find('input, select').removeClass('is-invalid');
      }

      if (!($('#Patient_PatientId').val() && parseInt($('#Patient_PatientId').val(), 10) > 0)) {
        setReadonly(false);
        $fn.focus();
      }

      $('#fc_insurancePanel').removeClass('show');
      $('body').removeClass('modal-open');
      $('.modal-backdrop').remove();
    });
  });
  
  // Export برای تست در console
  window.submitFastCreate = submitFastCreate;

  // Initialize - اگر PatientId وجود دارد، اطلاعات را لود کن
  $(document).ready(function() {
    const patientId = parseInt($pid.val());
    const nc = $nc.val()?.trim();
    
    if (patientId > 0) {
      // اگر PatientId وجود دارد، اطلاعات را از API بگیر
      // ✅ استفاده از jQuery Deferred API برای سازگاری بهتر
      const initRequest = API.post('/patient/lookup-or-create', { NationalCode: nc || '' });
      
      initRequest.done(function(fullResponse) {
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
      });
      
      initRequest.fail(function(err) {
        console.warn('🏥 V2: Failed to load patient data on init:', err);
      });
    }
  });

})(jQuery, window.ReceptionAPI, window.RxUtils);
