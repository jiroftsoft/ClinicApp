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
   */
  function fillIdentity(identity) {
    if (!identity) return;
    
    $pid.val(identity.patientId || '');
    $fn.val(identity.firstName || '');
    $ln.val(identity.lastName || '');
    $fa.val(identity.fatherName || '');
    $mb.val(identity.mobile || '');
    $ph.val(identity.phone || '');
    $ad.val(identity.address || '');
    $gd.val(identity.gender || '');
    $bd.val(identity.birthDateShamsi || '');
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
      .then(API.ok)
      .then(function(response) {
        if (!response || !response.success) {
          toastr.error(response?.message || 'بیمار یافت نشد');
          return;
        }

        const dto = response.data || response;
        console.log('🏥 V2: Patient lookup response:', dto);

        // ذخیره کپی برای انصراف
        cache = JSON.parse(JSON.stringify(dto.identity || dto.Identity));

        // پر کردن اطلاعات هویتی
        fillIdentity(dto.identity || dto.Identity);

        // تنظیم بیمه‌ها
        if (window.insPanel && (dto.insurance || dto.Insurance)) {
          window.insPanel.set(dto.insurance || dto.Insurance);
        }

        setReadonly(true);
        toastr.success('اطلاعات بیمار بارگذاری شد');

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
      .then(API.ok)
      .then(function(response) {
        if (!response || !response.success) {
          toastr.error(response?.message || 'خطا در ذخیره');
          return;
        }

        const updated = response.data || response;
        fillIdentity(updated);
        cache = JSON.parse(JSON.stringify(updated));
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
        .then(API.ok)
        .then(function(response) {
          if (response && response.success && response.data) {
            const dto = response.data;
            fillIdentity(dto.identity || dto.Identity);
            if (window.insPanel && (dto.insurance || dto.Insurance)) {
              window.insPanel.set(dto.insurance || dto.Insurance);
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
