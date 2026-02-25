(function($, API, U) {
  'use strict';

  // Fallback when toastr failed to load (e.g. 404) - use NotificationHelper or alert
  if (typeof window.toastr === 'undefined') {
    window.toastr = {
      error: function(msg, title, opts) {
        if (window.NotificationHelper && window.NotificationHelper.error) {
          window.NotificationHelper.error(msg, title || 'خطا', opts);
        } else {
          alert((title || 'خطا') + ': ' + (msg || 'خطایی رخ داده است'));
        }
      },
      success: function(msg, title, opts) {
        if (window.NotificationHelper && window.NotificationHelper.success) {
          window.NotificationHelper.success(msg, title || 'موفقیت', opts);
        } else {
          alert((title || 'موفقیت') + ': ' + (msg || ''));
        }
      },
      warning: function(msg, title, opts) {
        if (window.NotificationHelper && window.NotificationHelper.warning) {
          window.NotificationHelper.warning(msg, title || 'هشدار', opts);
        } else {
          alert((title || 'هشدار') + ': ' + (msg || ''));
        }
      },
      info: function(msg, title, opts) {
        if (window.NotificationHelper && window.NotificationHelper.info) {
          window.NotificationHelper.info(msg, title || 'اطلاعات', opts);
        } else {
          alert((title ? title + ': ' : '') + (msg || ''));
        }
      },
      options: {}
    };
  }

  /**
   * اعتبارسنجی کد ملی ایرانی (الگوریتم استاندارد + دفاع در عمق)
   * استفاده از ReceptionValidator در صورت وجود؛ وگرنه پیاده‌سازی محلی یکسان
   * @param {string} code - کد ملی 10 رقمی
   * @returns {{ isValid: boolean, message: string }}
   */
  function validateIranianNationalCode(code) {
    if (window.ReceptionValidator && typeof window.ReceptionValidator.validateNationalCode === 'function') {
      return window.ReceptionValidator.validateNationalCode(code);
    }
    var c = String(code || '').trim().replace(/[\u06F0-\u06F9]/g, function(d) { return String.fromCharCode(d.charCodeAt(0) - 0x06F0 + 0x0030); }).replace(/[\u0660-\u0669]/g, function(d) { return String.fromCharCode(d.charCodeAt(0) - 0x0660 + 0x0030); });
    if (!c) return { isValid: false, message: 'کد ملی الزامی است' };
    if (c.length !== 10 || !/^\d{10}$/.test(c)) return { isValid: false, message: 'کد ملی باید 10 رقم باشد' };
    if (/^(\d)\1{9}$/.test(c)) return { isValid: false, message: 'کد ملی نامعتبر است' };
    var sum = 0; for (var i = 0; i < 9; i++) sum += parseInt(c[i], 10) * (10 - i);
    var r = sum % 11, last = parseInt(c[9], 10);
    var ok = (r < 2 && last === r) || (r >= 2 && last === 11 - r);
    return { isValid: ok, message: ok ? '' : 'کد ملی نامعتبر است (رقم کنترل اشتباه)' };
  }

  /** نرمال‌سازی کد ملی (فارسی/عربی → انگلیسی) قبل از ارسال به API - HIS Production */
  function normalizeNationalCodeToEnglish(code) {
    if (!code) return (code || '').trim();
    var s = String(code).trim()
      .replace(/[\u06F0-\u06F9]/g, function(d) { return String.fromCharCode(d.charCodeAt(0) - 0x06F0 + 0x0030); })
      .replace(/[\u0660-\u0669]/g, function(d) { return String.fromCharCode(d.charCodeAt(0) - 0x0660 + 0x0030); });
    return s;
  }

  /** نمایش خطای کد ملی نامعتبر: SweetAlert2 در اولویت، در غیر این صورت toastr */
  function showNationalCodeError(message) {
    var msg = message || 'کد ملی وارد شده معتبر نیست. لطفاً کد ملی صحیح را وارد کنید.';
    if (typeof Swal !== 'undefined' && typeof Swal.fire === 'function') {
      Swal.fire({
        icon: 'error',
        title: 'کد ملی نامعتبر',
        text: msg,
        confirmButtonText: 'متوجه شدم'
      });
    } else {
      toastr.error(msg, 'کد ملی نامعتبر', { timeOut: 5000 });
    }
  }

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
    var mbRaw = identity.mobile || identity.Mobile;
    var mb = (window.RxUtils && window.RxUtils.normalizeMobileForDisplay)
      ? window.RxUtils.normalizeMobileForDisplay(mbRaw)
      : (mbRaw || '');
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
   * ✅ اعتبارسنجی کامل کد ملی ایرانی قبل از فراخوانی API؛ جلوگیری از باز شدن مودال برای کد نامعتبر
   */
  function lookup() {
    const nc = ($nc.val() || '').trim();
    
    if (nc.length !== 10 || !/^\d{10}$/.test(nc)) {
      console.warn('🏥 V2: کد ملی ناقص یا غیرعددی:', nc);
      toastr.warning('کد ملی باید 10 رقم باشد');
      return $.Deferred().reject('Invalid national code').promise();
    }

    var ncResult = validateIranianNationalCode(nc);
    if (!ncResult.isValid) {
      console.warn('🏥 V2: کد ملی نامعتبر (الگوریتم ایرانی):', nc, ncResult.message);
      showNationalCodeError(ncResult.message);
      $nc.addClass('is-invalid').focus();
      return $.Deferred().reject('Invalid national code').promise();
    }

    var ncForApi = normalizeNationalCodeToEnglish(nc);
    console.log('🏥 V2: جستجوی بیمار - کد ملی:', nc, '(نرمال:', ncForApi + ')');

    // ✅ Performance: بررسی Cache اول (با کلید نرمال)
    const cached = patientLookupCache.get(ncForApi);
    if (cached) {
      console.log('✅ V2: Using cached patient data for national code:', nc);
      // Process cached data
      const dto = cached;
      const identity = dto?.Identity || dto?.identity;
      const insurance = dto?.Insurance || dto?.insurance;
      
      if (identity) {
        fillIdentity(identity);
        if (window.insPanel && insurance) {
          window.insPanel.set(insurance);
        }
        setReadonly(true);
        toastr.success('اطلاعات بیمار از Cache بارگذاری شد');
      }
      
      return $.Deferred().resolve(cached).promise();
    }

    // ✅ Cancel previous request
    if (currentLookupRequest && currentLookupRequest.abort) {
      console.log('🔄 V2: Canceling previous patient lookup request...');
      currentLookupRequest.abort();
      currentLookupRequest = null;
    }

    // ✅ HIS Production: ارسال کد ملی نرمال (انگلیسی) به API
    currentLookupRequest = API.post('/patient/lookup-or-create', { NationalCode: ncForApi });
    
    currentLookupRequest.done(function(fullResponse) {
        currentLookupRequest = null; // ✅ Clear request reference
        
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
        const successValue = responseObj?.Success ?? responseObj?.success;
        const isSuccess = successValue === true || successValue === "true" || successValue === 1;
        // اگر پاسخ حاوی Data.Identity است، حتی با Success=false آن را موفق در نظر بگیر (مقاوم در برابر تفاوت سریالایز/بایند)
        const dataPayload = responseObj?.Data ?? responseObj?.data;
        const hasIdentity = dataPayload && (dataPayload.Identity || dataPayload.identity);
        const treatAsSuccess = isSuccess || !!hasIdentity;
        
        console.log('🏥 V2: Success check:', {
          'responseObj.Success': responseObj?.Success,
          'successValue': successValue,
          'isSuccess': isSuccess,
          'hasIdentity': !!hasIdentity,
          'treatAsSuccess': treatAsSuccess
        });
        
        if (!responseObj || !treatAsSuccess) {
          const errorCode = responseObj?.Code || responseObj?.code;
          const errorMsg = responseObj?.Message || responseObj?.message || 'بیمار یافت نشد';
          
          console.warn('🏥 V2: Patient lookup failed:', errorCode, errorMsg, responseObj);
          
          // ✅ فقط وقتی API صریحاً «بیمار یافت نشد» (NOT_FOUND) برگرداند مودال افزودن باز می‌شود؛ نه برای خطای اعتبارسنجی یا شبکه
          if (errorCode === 'NOT_FOUND' || errorCode === 'NotFound') {
            var ncCheck = validateIranianNationalCode(nc);
            if (!ncCheck.isValid) {
              showNationalCodeError(ncCheck.message);
              $nc.addClass('is-invalid').focus();
              return;
            }
            console.log('🏥 V2: Patient not found, opening Fast Create Modal...');
            openFastCreateModal(nc);
          } else {
            // نمایش خطا با Error Handler
            if (window.ReceptionErrorHandler) {
              window.ReceptionErrorHandler.showError(responseObj);
            } else {
              toastr.error(errorMsg);
            }
          }
          return;
        }

        // دریافت Data: در صورت موفقیت از API.ok؛ اگر فقط Identity داشتیم از dataPayload
        const dto = (treatAsSuccess && hasIdentity ? dataPayload : null) || API.ok(responseObj);
        console.log('🏥 V2: Patient lookup data (extracted):', dto);
        console.log('🏥 V2: Data type:', typeof dto);
        console.log('🏥 V2: Data keys:', dto ? Object.keys(dto) : 'null/undefined');

        // ✅ Performance: Cache response (با کلید نرمال)
        patientLookupCache.set(ncForApi, dto);
        console.log('✅ V2: Patient lookup data cached for national code:', ncForApi);

        // ذخیره کپی برای انصراف
        const identity = dto?.Identity || dto?.identity;
        console.log('🏥 V2: Identity extracted:', identity);
        
          if (identity) {
            cancelCache = JSON.parse(JSON.stringify(identity)); // فقط برای انصراف از ویرایش
            
            // پر کردن اطلاعات هویتی
            fillIdentity(identity);
            console.log('🏥 V2: Identity filled to form');
            
            // ✅ Trigger state change event for Summary Header
            var mobileForState = (window.RxUtils && window.RxUtils.normalizeMobileForDisplay)
                ? window.RxUtils.normalizeMobileForDisplay(identity.Mobile || identity.mobile)
                : (identity.Mobile || identity.mobile || '');
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
                Mobile: mobileForState
            };
            
            console.log('🏥 V2 Patient-Lookup: ✅ Triggering rv2:stateChanged with patientData:', patientData);
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
    
    currentLookupRequest.fail(function(err) {
      currentLookupRequest = null; // ✅ Clear request reference
      
      // ✅ Ignore aborted requests
      if (err && err.status === 'abort') {
        console.log('ℹ️ V2: Patient lookup request was aborted');
        return;
      }
      
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
    
    return currentLookupRequest; // Return promise برای استفاده در performLookup
  }

  /**
   * مقداردهی DatePicker تاریخ تولد در مودال ثبت سریع با JalaliDatePickerEnterprise
   * الگوی استاندارد: maxDate = امروز، minDate = گذشته دور، بدون تاریخ پیش‌فرض
   * چون اینپوت مودال data-jdp ندارد، کتابخانهٔ پایه روی focus باز نمی‌کند؛ با focus/click دستی show فراخوانی می‌شود.
   */
  function initFastCreateModalBirthDatePicker() {
    var fcBirth = document.getElementById('fc_birth');
    if (!fcBirth) return;
    if (typeof window.JalaliDatePickerEnterprise === 'undefined' || typeof window.jalaliDatepicker === 'undefined') return;

    var JDP = window.JalaliDatePickerEnterprise;

    function bindShowPicker() {
      if (fcBirth.dataset.jdpShowBound === 'true') return;
      function showPicker() {
        if (typeof window.jalaliDatepicker !== 'undefined') {
          window.jalaliDatepicker.show(fcBirth);
        }
      }
      fcBirth.addEventListener('focus', showPicker);
      fcBirth.addEventListener('click', showPicker);
      fcBirth.dataset.jdpShowBound = 'true';
    }

    if (fcBirth.dataset && fcBirth.dataset.jdpInitialized === 'true') {
      bindShowPicker();
      return;
    }

    JDP.getTodayFromServer()
      .then(function(todayStr) {
        var todayObj = JDP.parsePersianDate(todayStr);
        if (!todayObj) return;
        JDP.init(fcBirth, {
          maxDate: todayObj,
          minDate: { year: 1280, month: 1, day: 1 },
          theme: 'medical',
          size: 'medium',
          noDefaultDate: true
        });
        setTimeout(bindShowPicker, 150);
      })
      .catch(function() {
        JDP.init(fcBirth, {
          theme: 'medical',
          size: 'medium',
          noDefaultDate: true
        });
        setTimeout(bindShowPicker, 150);
      });
  }

  /**
   * باز کردن Modal ثبت سریع بیمار
   * @param {string} nc - کد ملی (اختیاری)؛ در صورت ارسال، اعتبارسنجی می‌شود و در صورت نامعتبر بودن مودال باز نمی‌شود
   */
  function openFastCreateModal(nc) {
    if (nc) {
      var ncResult = validateIranianNationalCode(nc);
      if (!ncResult.isValid) {
        showNationalCodeError(ncResult.message);
        $nc.addClass('is-invalid').focus();
        return;
      }
    }
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

          // ✅ تاریخ تولد: JalaliDatePicker Enterprise (الگوی جدید)
          initFastCreateModalBirthDatePicker();

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

          if (nc) {
            $('#fc_nationalCode').val(nc);
          }
          initFastCreateModalBirthDatePicker();

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
      initFastCreateModalBirthDatePicker();

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
    
    // Validation - اعتبارسنجی کامل کد ملی ایرانی
    const nc = ($('#fc_nationalCode').val() || '').trim();
    const fn = ($('#fc_firstName').val() || '').trim();
    const ln = ($('#fc_lastName').val() || '').trim();
    const mb = ($('#fc_mobile').val() || '').trim();

    var ncResult = validateIranianNationalCode(nc);
    if (!ncResult.isValid) {
      showNationalCodeError(ncResult.message);
      $('#fc_nationalCode').addClass('is-invalid').focus();
      return;
    }
    $('#fc_nationalCode').removeClass('is-invalid');
    
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
      
      // نمایش خطا با Error Handler
      if (window.ReceptionErrorHandler) {
        window.ReceptionErrorHandler.showError(err);
      } else {
        toastr.error('خطا در ثبت سریع بیمار');
      }
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
      
      // نمایش خطا با Error Handler
      if (window.ReceptionErrorHandler) {
        window.ReceptionErrorHandler.showError(err);
      } else {
        toastr.error('خطا در به‌روزرسانی اطلاعات');
      }
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
  // ✅ Performance: Cache برای Lookup Results (5 دقیقه)
  
  // ✅ Cache برای Patient Lookup (5 دقیقه)
  const patientLookupCache = {
    data: {},
    get: function(nationalCode) {
      const cached = this.data[nationalCode];
      if (cached && Date.now() - cached.timestamp < 300000) { // 5 minutes
        return cached.data;
      }
      return null;
    },
    set: function(nationalCode, data) {
      this.data[nationalCode] = {
        data: data,
        timestamp: Date.now()
      };
    },
    clear: function() {
      this.data = {};
    }
  };
  
  let lookupTimeout = null;
  let isLookingUp = false; // Flag برای جلوگیری از درخواست‌های همزمان
  let currentLookupRequest = null; // ✅ برای Cancel کردن Request های قبلی
  
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
    
    // اگر کد ملی کامل نیست یا معتبر نیست (الگوریتم ایرانی)، skip کن
    if (!/^\d{10}$/.test(nc)) return;
    var ncResult = validateIranianNationalCode(nc);
    if (!ncResult.isValid) {
      showNationalCodeError(ncResult.message);
      $nc.addClass('is-invalid').focus();
      return;
    }
    $nc.removeClass('is-invalid');

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
    if (!/^\d{10}$/.test(nc)) return;
    var ncResult = validateIranianNationalCode(nc);
    if (!ncResult.isValid) {
      showNationalCodeError(ncResult.message);
      $nc.addClass('is-invalid').focus();
      return;
    }
    $nc.removeClass('is-invalid');

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
    const ncNormalized = normalizeNationalCodeToEnglish(nc);
    if (/^\d{10}$/.test(ncNormalized) && !$pid.val() && !isLookingUp) {
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

    // پاک‌سازی فرم ثبت سریع (دکمه «پاک‌سازی فرم» داخل مودال)
    $('#btnFastCreateReset').on('click', function() {
      var $form = $('#patientFastCreateForm');
      if ($form.length) {
        $form.trigger('reset');
        $form.find('input, select').removeClass('is-invalid');
      }
      $('#fc_insurancePanel').removeClass('show');
      var fcBirth = document.getElementById('fc_birth');
      if (fcBirth && fcBirth.value) fcBirth.value = '';
    });

    $('#patientFastCreateModal').on('hidden.bs.modal', function() {
      console.log('🏥 V2: Fast Create Modal hidden - Starting cleanup...');
      
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
      
      // ✅ BULLETPROOF FIX: پاکسازی کامل Modal Backdrop با استفاده از چند لایه امنیتی
      function cleanupModalBackdrop() {
        console.log('🏥 V2: Cleaning up modal backdrop...');
        
        // روش 1: حذف کلاس modal-open از body
        $('body').removeClass('modal-open');
        
        // روش 2: حذف تمام backdrop ها
        $('.modal-backdrop').remove();
        
        // روش 3: اطمینان از پاک شدن overflow hidden
        $('body').css({
          'overflow': '',
          'padding-right': ''
        });
        
        // روش 4: پاکسازی inline styles از body (با حفظ سایر styles)
        const bodyStyle = $('body').attr('style');
        if (bodyStyle) {
          // حذف فقط overflow و padding-right
          const newStyle = bodyStyle
            .replace(/overflow\s*:\s*[^;]+;?/gi, '')
            .replace(/padding-right\s*:\s*[^;]+;?/gi, '')
            .trim();
          
          if (newStyle) {
            $('body').attr('style', newStyle);
          } else {
            $('body').removeAttr('style');
          }
        }
        
        // روش 5: dispose instance modal
        const modalElement = document.getElementById('patientFastCreateModal');
        if (modalElement) {
          const modalInstance = bootstrap.Modal.getInstance(modalElement);
          if (modalInstance) {
            modalInstance.dispose();
          }
        }
        
        console.log('🏥 V2: ✅ Modal backdrop cleanup completed');
        console.log('🏥 V2: ✅ Body classes:', $('body').attr('class'));
        console.log('🏥 V2: ✅ Body style:', $('body').attr('style') || 'none');
        console.log('🏥 V2: ✅ Remaining backdrops:', $('.modal-backdrop').length);
      }
      
      // اجرای فوری
      cleanupModalBackdrop();
      
      // اجرای با تاخیر (fallback) - در صورتی که Bootstrap هنوز cleanup نکرده باشد
      setTimeout(function() {
        const remainingBackdrops = $('.modal-backdrop').length;
        if (remainingBackdrops > 0 || $('body').hasClass('modal-open')) {
          console.warn('🏥 V2: ⚠️ Backdrop still exists after 100ms, forcing cleanup...');
          cleanupModalBackdrop();
        }
      }, 100);
      
      // اجرای با تاخیر بیشتر (double-check)
      setTimeout(function() {
        const remainingBackdrops = $('.modal-backdrop').length;
        if (remainingBackdrops > 0 || $('body').hasClass('modal-open')) {
          console.error('🏥 V2: ❌ Backdrop STILL exists after 300ms, forcing aggressive cleanup...');
          cleanupModalBackdrop();
          
          // اگر هنوز باقی مانده، با force حذف کن
          $('.modal-backdrop').each(function() {
            $(this).remove();
          });
          $('body').removeClass('modal-open').removeAttr('style');
        }
      }, 300);
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
        // نمایش خطا به منشی
        if (window.ReceptionErrorHandler) {
          window.ReceptionErrorHandler.showError(err);
        }
      });
    }
    
    // ✅ فعال‌سازی Real-time Validation
    if (window.ReceptionValidator && typeof window.ReceptionValidator.initializeRealtimeValidation === 'function') {
      window.ReceptionValidator.initializeRealtimeValidation();
      console.log('✅ V2: Real-time Validation activated');
    } else {
      console.warn('⚠️ V2: ReceptionValidator not found - Real-time validation disabled');
    }
  });

})(jQuery, window.ReceptionAPI, window.RxUtils);
