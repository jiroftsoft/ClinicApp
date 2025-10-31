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
          const errorMsg = responseObj?.Message || responseObj?.message || 'بیمار یافت نشد';
          console.warn('🏥 V2: Patient lookup failed:', errorMsg, responseObj);
          toastr.error(errorMsg);
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
