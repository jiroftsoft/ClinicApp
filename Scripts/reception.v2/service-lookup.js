(function(API, U){
  'use strict';

  /**
   * بارگذاری خدمات یک دپارتمان
   * اگر basePlanId یا suppPlanId ارسال شود، وضعیت تعیین‌ست برای هر خدمت برمی‌گردد و خدمات بدون تعیین‌ست با رنگ متفاوت نمایش داده می‌شوند.
   */
  function loadServices(deptId, basePlanId, suppPlanId) {
    if (!deptId || deptId === '' || deptId === '0') {
      console.warn('🏥 V2: Cannot load services - invalid department ID:', deptId);
      $("#ServiceId").empty().append('<option value="">ابتدا دپارتمان را انتخاب کنید</option>');
      return;
    }

    var params = { deptId: deptId };
    if (basePlanId) params.basePlanId = basePlanId;
    if (suppPlanId) params.suppPlanId = suppPlanId;
    var cacheKey = deptId + '_b' + (basePlanId || '') + '_s' + (suppPlanId || '');

    console.log('🏥 V2: Loading services for department:', deptId, 'basePlanId:', basePlanId, 'suppPlanId:', suppPlanId);

    currentServiceRequest = API.get("/services/by-department", params);

    currentServiceRequest
      .then(function(fullResponse) {
        currentServiceRequest = null;

        const response = API.ok(fullResponse);
        let services = [];
        if (Array.isArray(response)) {
          services = response;
        } else if (response.Services || response.services) {
          services = response.Services || response.services || [];
        } else if (response.Data && Array.isArray(response.Data)) {
          services = response.Data;
        }

        serviceCache.set(cacheKey, services);
        populateServiceSelect(services);
      })
      .catch(function(err) {
        currentServiceRequest = null;
        if (err && err.status === 'abort') return;
        console.error('🏥 V2: Services load error:', err);
        toastr.error('خطا در بارگذاری خدمات');
        $("#ServiceId").empty().append('<option value="">خطا در بارگذاری</option>');
      });
  }

  // ✅ Cache key شامل deptId و بیمه‌ها (وضعیت تعیین‌ست وابسته به بیمه است)
  const serviceCache = {
    data: {},
    get: function(cacheKey) {
      const cached = this.data[cacheKey];
      if (cached && Date.now() - cached.timestamp < 600000) return cached.data;
      return null;
    },
    set: function(cacheKey, data) {
      this.data[cacheKey] = { data: data, timestamp: Date.now() };
    }
  };
  
  let currentServiceRequest = null; // ✅ برای Cancel کردن Request های قبلی
  let serviceLoadDebounceTimer = null; // ✅ برای Debouncing

  // ✅ انتخاب خدمت با کد: لیست خدمات دپارتمان فعلی (برای جستجو با کد)
  let currentDepartmentServices = [];

  function loadServicesOptimized(deptId) {
    if (currentServiceRequest && currentServiceRequest.abort) {
      currentServiceRequest.abort();
      currentServiceRequest = null;
    }
    if (serviceLoadDebounceTimer) {
      clearTimeout(serviceLoadDebounceTimer);
      serviceLoadDebounceTimer = null;
    }

    var basePlanId = $("#BasePlanId").val() || null;
    var suppPlanId = $("#SuppPlanId").val() || null;
    var cacheKey = deptId + '_b' + (basePlanId || '') + '_s' + (suppPlanId || '');
    var cached = serviceCache.get(cacheKey);
    if (cached) {
      console.log('✅ V2: Using cached services:', cacheKey);
      populateServiceSelect(cached);
      return;
    }

    serviceLoadDebounceTimer = setTimeout(function() {
      serviceLoadDebounceTimer = null;
      loadServices(deptId, basePlanId, suppPlanId);
    }, 300);
  }

  // ✅ Populate service select با نمایش کد و تمایز خدمات بدون تعیین‌ست (رنگ متفاوت)
  function populateServiceSelect(services) {
    currentDepartmentServices = services && Array.isArray(services) ? services : [];
    const $serviceSelect = $("#ServiceId");
    $serviceSelect.empty().append('<option value="">انتخاب کنید</option>');

    if (currentDepartmentServices.length > 0) {
      currentDepartmentServices.forEach(function(service) {
        if (!service) return;
        const serviceId = service.serviceId || service.ServiceId;
        const serviceCode = (service.serviceCode || service.ServiceCode || service.code || service.Code || '').toString().trim() || '—';
        const serviceName = service.serviceName || service.ServiceName || service.name || service.Name || '';
        const price = service.price || service.Price || service.unitPriceIRR || service.UnitPriceIRR || 0;
        const hasTariffSet = service.hasTariffSet !== false;
        const tariffWarning = service.tariffWarning || service.TariffWarning || null;
        let text = serviceCode + ' - ' + serviceName + ' - ' + U.toIRR(price);
        if (!hasTariffSet) {
          text += ' \u26A0\uFE0F ' + (tariffWarning || 'تعیین ست نشده'); // ⚠️ emoji برای نمایش رنگی
        }
        const opt = $('<option></option>').val(serviceId).text(text);
        if (!hasTariffSet) {
          opt.addClass('service-no-tariff').attr('data-tariff-warning', tariffWarning || '');
        }
        $serviceSelect.append(opt);
      });
      console.log('✅ V2: Services filled:', currentDepartmentServices.length);
    } else {
      $serviceSelect.append('<option value="">خدمتی یافت نشد</option>');
    }
  }

  // ✅ انتخاب خدمت با کد: جستجو در لیست دپارتمان فعلی و set کردن dropdown
  function applyServiceByCode() {
    const codeInput = ($("#ServiceCodeSearch").val() || '').toString().trim();
    if (!codeInput) {
      toastr.warning('کد خدمت را وارد کنید');
      return;
    }
    const deptId = $("#DepartmentId").val();
    if (!deptId || deptId === '' || deptId === '0') {
      toastr.warning('ابتدا دپارتمان را انتخاب کنید');
      $("#ServiceCodeSearch").val('');
      return;
    }
    const codeLower = codeInput.toLowerCase();
    const found = currentDepartmentServices.find(function(s) {
      const sc = (s.serviceCode || s.ServiceCode || s.code || s.Code || '').toString().trim().toLowerCase();
      return sc === codeLower;
    });
    if (found) {
      const serviceId = found.serviceId || found.ServiceId;
      $("#ServiceId").val(serviceId).trigger('change');
      $("#ServiceCodeSearch").val('');
      $("#Quantity").focus();
      toastr.success('خدمت انتخاب شد');
    } else {
      toastr.warning('خدمتی با این کد در دپارتمان انتخاب‌شده یافت نشد.');
    }
  }
  
  // Load services when department changes - ✅ با بهینه‌سازی
  $("#DepartmentId").on('change', function() {
    const deptId = $(this).val();
    console.log('🏥 V2: Department changed, loading services for:', deptId);
    if (deptId) {
      loadServicesOptimized(deptId);
      $("#ServiceId").val('').trigger('change');
    } else {
      currentDepartmentServices = [];
      $("#ServiceId").empty().append('<option value="">ابتدا دپارتمان را انتخاب کنید</option>');
      $("#ServiceCodeSearch").val('');
    }
  });

  // ✅ با تغییر بیمه (پایه/تکمیلی)، لیست خدمات دوباره لود شود تا وضعیت تعیین‌ست با بیمه جدید به‌روز شود
  $("#BasePlanId, #SuppPlanId").on('change', function() {
    const deptId = $("#DepartmentId").val();
    if (deptId && deptId !== '' && deptId !== '0') {
      loadServicesOptimized(deptId);
    }
  });

  // ✅ انتخاب خدمت با کد: Enter در فیلد کد
  $("#ServiceCodeSearch").on('keypress', function(e) {
    if (e.which === 13) {
      e.preventDefault();
      applyServiceByCode();
    }
  });

  $("#BtnApplyServiceCode").on('click', function() {
    applyServiceByCode();
  });

  // ✅ Load eligible doctors when service changes + هوشمندسازی: اگر ReceptionId وجود دارد، Reprice
  $("#ServiceId").on('change', function() {
    const serviceId = parseInt($(this).val(), 10);
    const deptId = parseInt($("#DepartmentId").val(), 10);
    const clinicId = parseInt($("#ClinicId").val(), 10) || 1; // Default clinic ID = 1 (کلینیک شفا)
    
    if (!serviceId || !deptId) {
      console.log('🏥 V2: Service or Department not selected, skipping doctor filter');
      return;
    }
    
    console.log('🏥 V2: Service changed, loading eligible doctors...', { serviceId, deptId, clinicId });
    
    // Call the new endpoint to filter doctors by service
    if (window.loadDoctorsByService) {
      window.loadDoctorsByService({ serviceId, deptId, clinicId });
    } else {
      console.warn('🏥 V2: loadDoctorsByService function not found');
    }
    
    // ✅ هوشمندسازی: اگر ReceptionId وجود دارد و آیتم‌هایی در Reception هستند، Totals را رفرش کن
    const receptionId = $("#ReceptionId").val();
    if (receptionId && receptionId > 0) {
      console.log('🏥 V2: Service changed with active reception, refreshing totals...');
      // Totals بعداً هنگام افزودن/حذف آیتم به‌روزرسانی می‌شود
      // اینجا فقط برای اطمینان، اگر آیتم‌هایی وجود دارد، refresh می‌کنیم
      setTimeout(function() {
        if (window.insurancePanelModule && typeof window.insurancePanelModule.loadTotals === 'function') {
          window.insurancePanelModule.loadTotals(receptionId);
        }
      }, 500); // کمی تأخیر برای اطمینان از به‌روزرسانی
    }
  });

  $("#BtnAddItem").on("click", function(){
    const serviceId = $("#ServiceId").val();
    const quantity = U.parseFaInt($("#Quantity").val());
    
    if(!serviceId) {
      toastr.warning('لطفاً خدمت را انتخاب کنید');
      return;
    }
    
    if(quantity <= 0) {
      toastr.warning('تعداد باید بیشتر از صفر باشد');
      return;
    }
    
    // ✅ گام 2 - Draft Orchestrator: استفاده از ensureDraftOrSkip
    window.AutoDraftManager?.ensureDraftOrSkip({
      patientId: $("#Patient_PatientId").val(),
      clinicId: $("#ClinicId").val(),
      departmentId: $("#DepartmentId").val(),
      doctorId: $("#DoctorId").val(),
      receptionId: $("#ReceptionId").val()
    }).then(function(receptionId) {
      if (!receptionId || receptionId <= 0) {
        window.AutoDraftManager?.warnDraftMissing();
        return;
      }
      $("#ReceptionId").val(receptionId);
      proceedWithAddItem();
    }).catch(function(err) {
      console.error('🏥 V2: ensureDraftOrSkip error:', err);
      window.AutoDraftManager?.warnDraftMissing();
    });
  });
  
  /**
   * 🚨 PROFESSIONAL: تابع کمکی برای نمایش محاسبه بیمه real-time
   * @param {Object} insuranceCalc - اطلاعات محاسبه بیمه از InsuranceCalculation
   * @returns {Object} - اطلاعات فرمت شده برای نمایش در UI
   */
  function formatInsuranceCalculation(insuranceCalc) {
    if (!insuranceCalc) {
      return {
        primaryCoverage: 0,
        supplementaryCoverage: 0,
        totalCoverage: 0,
        patientShare: 0,
        coverageStatus: 'بدون پوشش',
        primaryCoverageStr: U.toIRR(0),
        supplementaryCoverageStr: U.toIRR(0),
        totalCoverageStr: U.toIRR(0),
        patientShareStr: U.toIRR(0),
        statusClass: 'text-danger',
        statusBadge: '<span class="badge bg-danger">بدون پوشش</span>'
      };
    }

    const primary = insuranceCalc.PrimaryCoverage || insuranceCalc.primaryCoverage || 0;
    const supplementary = insuranceCalc.SupplementaryCoverage || insuranceCalc.supplementaryCoverage || 0;
    const total = insuranceCalc.TotalInsuranceCoverage || insuranceCalc.totalInsuranceCoverage || 0;
    const patient = insuranceCalc.PatientShare || insuranceCalc.patientShare || 0;
    const status = insuranceCalc.CoverageStatus || insuranceCalc.coverageStatus || 'بدون پوشش';

    // تعیین کلاس و badge بر اساس وضعیت
    let statusClass = 'text-danger';
    let statusBadge = '<span class="badge bg-danger coverage-badge" role="button" style="cursor:pointer;">بدون پوشش</span>';
    
    if (status === 'پوشش کامل') {
      statusClass = 'text-success';
      statusBadge = '<span class="badge bg-success coverage-badge" role="button" style="cursor:pointer;">پوشش کامل</span>';
    } else if (status === 'پوشش ناقص') {
      statusClass = 'text-warning';
      statusBadge = '<span class="badge bg-warning text-dark coverage-badge" role="button" style="cursor:pointer;">پوشش ناقص</span>';
    }

    return {
      primaryCoverage: primary,
      supplementaryCoverage: supplementary,
      totalCoverage: total,
      patientShare: patient,
      coverageStatus: status,
      primaryCoverageStr: U.toIRR(primary),
      supplementaryCoverageStr: U.toIRR(supplementary),
      totalCoverageStr: U.toIRR(total),
      patientShareStr: U.toIRR(patient),
      statusClass: statusClass,
      statusBadge: statusBadge
    };
  }

  function proceedWithAddItem() {
    const serviceId = $("#ServiceId").val();
    const quantity = U.parseFaInt($("#Quantity").val());
    const receptionId = $("#ReceptionId").val();
    
    const payload = {
      receptionId: receptionId,
      serviceId: serviceId,
      quantity: quantity,
      year: (window.ReceptionBootstrap && window.ReceptionBootstrap.FinancialYear) || 1404
    };
    
    // 🚨 PROFESSIONAL: نمایش loading state
    const $btn = $("#BtnAddItem");
    const originalText = $btn.text();
    $btn.prop('disabled', true).text('در حال افزودن...');
    
    API.post("/item/add", payload)
      .then(function(fullResponse) {
        console.log('🏥 V2: Add item raw response:', fullResponse);
        
        // ✅ بررسی Success قبل از استخراج Data
        const success = fullResponse?.Success ?? fullResponse?.success ?? false;
        const message = fullResponse?.Message ?? fullResponse?.message ?? '';
        const code = fullResponse?.Code ?? fullResponse?.code ?? '';
        
        // ✅ اگر Success === false است، خطا را نمایش بده و ادامه نده
        if (success === false) {
          console.warn('🏥 V2: Add item failed - Success: false, Code:', code, 'Message:', message);
          
          // ✅ استفاده از handleErrorJson برای خطاهای خاص (ANTIFORGERY_MISSING, UNHANDLED, etc.)
          if (API.handleErrorJson && typeof API.handleErrorJson === 'function') {
            const errorHandled = API.handleErrorJson(fullResponse);
            if (errorHandled) {
              // خطا توسط handleErrorJson handle شد (مثلاً ANTIFORGERY_MISSING)
              $btn.prop('disabled', false).text(originalText);
              return;
            }
          }
          
          // ✅ نمایش پیغام خطا به کاربر (برای خطاهای معمولی)
          if (message) {
            // استفاده از SweetAlert2 اگر موجود است، وگرنه toastr
            if (window.Swal && typeof window.Swal.fire === 'function') {
              window.Swal.fire({
                icon: 'error',
                title: 'خطا در افزودن خدمت',
                html: message.replace(/\n/g, '<br>'),
                confirmButtonText: 'متوجه شدم',
                confirmButtonColor: '#d33'
              });
            } else {
              toastr.error(message, 'خطا در افزودن خدمت', {
                timeOut: 8000,
                extendedTimeOut: 5000
              });
            }
          } else {
            toastr.error('خطا در افزودن خدمت. لطفاً مجدداً تلاش کنید.', 'خطا', {
              timeOut: 5000
            });
          }
          
          // ✅ بازگرداندن دکمه به حالت عادی و خروج از تابع
          $btn.prop('disabled', false).text(originalText);
          return; // خروج از تابع - خدمت افزوده نشده است
        }
        
        // ✅ Extract data using API.ok (handles ServiceResult structure)
        const response = API.ok(fullResponse);
        console.log('🏥 V2: Item added response:', response);
        
        // ✅ پشتیبانی از ساختار جدید: { item, pricing, totals, insuranceCalculation }
        const itemData = response.item || response.Item || {};
        const pricingData = response.pricing || response.Pricing || null;
        const totalsData = response.totals || response.Totals || null;
        
        // 🚨 PROFESSIONAL: استخراج اطلاعات محاسبه بیمه real-time
        // بررسی در سطح response و Data
        let insuranceCalc = response.InsuranceCalculation || response.insuranceCalculation || null;
        
        // اگر در response نیست، از Data بررسی کن
        if (!insuranceCalc && response.Data) {
          insuranceCalc = response.Data.InsuranceCalculation || response.Data.insuranceCalculation || null;
        }
        
        // 🚨 PROFESSIONAL: اگر هنوز null است، از اولین item در Items بررسی کن
        if (!insuranceCalc && response.Items && response.Items.length > 0) {
          insuranceCalc = response.Items[0].InsuranceCalculation || response.Items[0].insuranceCalculation || null;
        }
        
        // 🚨 PROFESSIONAL: اگر هنوز null است، از items بررسی کن
        if (!insuranceCalc && response.items && response.items.length > 0) {
          insuranceCalc = response.items[0].InsuranceCalculation || response.items[0].insuranceCalculation || null;
        }
        
        console.log('🏥 V2: Insurance calculation from response:', insuranceCalc);
        console.log('🏥 V2: Full response structure:', JSON.stringify(response, null, 2));
        
        // ✅ اگر totals در پاسخ نیست، از Data.totals یا Data.Totals استفاده کن
        let totals = totalsData;
        if (!totals && response.Data) {
          totals = response.Data.totals || response.Data.Totals || 
                  (response.Data.ReceptionTotals || response.Data.receptionTotals) || null;
        }
        
        // ✅ نمایش پیغام موفقیت فقط اگر Success === true باشد
        toastr.success('خدمت افزوده شد');
        
        // ✅ Update items grid - استفاده از pricing و insuranceCalculation
        const $tb = $("#items-grid tbody");
        
        // 🚨 PROFESSIONAL FIX: اولویت با items است (چون InsuranceCalculation در آن است)
        // بررسی در چند سطح: response.items, response.Items, response.Data.items, response.Data.Items
        const items = response.items || response.Items || 
                     (response.Data && (response.Data.items || response.Data.Items)) || [];
        
        console.log('🏥 V2: Checking for items in response - items found:', items && items.length > 0, 'items count:', items ? items.length : 0);
        
        if (items && items.length > 0) {
          // 🚨 PROFESSIONAL: استفاده از items برای نمایش (چون InsuranceCalculation در آن است)
          // پیدا کردن آخرین آیتم (آیتم جدید)
          const newItem = items[items.length - 1];
          
          const serviceId = newItem.serviceId || newItem.ServiceId || $("#ServiceId").val();
          const code = newItem.code || newItem.Code || '';
          const name = newItem.name || newItem.Name || '';
          const qty = newItem.qty || newItem.Qty || 0;
          const unitPrice = newItem.unitPriceIRR || newItem.UnitPriceIRR || 0;
          const total = newItem.totalIRR || newItem.TotalIRR || 0;
          const receptionItemId = newItem.receptionItemId || newItem.ReceptionItemId ||
                                  itemData.ReceptionItemId || itemData.receptionItemId ||
                                  (pricingData ? (pricingData.ReceptionItemId || pricingData.receptionItemId) : '') || '';
          
          // 🚨 PROFESSIONAL: استخراج اطلاعات محاسبه بیمه از item (اولویت اصلی)
          const itemInsuranceCalc = newItem.InsuranceCalculation || newItem.insuranceCalculation || insuranceCalc || null;
          const itemInsuranceInfo = formatInsuranceCalculation(itemInsuranceCalc);
          
          console.log('🏥 V2: ===== ADD ITEM RESPONSE ANALYSIS =====');
          console.log('🏥 V2: Full response:', JSON.stringify(response, null, 2));
          console.log('🏥 V2: New item:', JSON.stringify(newItem, null, 2));
          console.log('🏥 V2: Item insurance calculation:', itemInsuranceCalc);
          console.log('🏥 V2: Formatted insurance info:', itemInsuranceInfo);
          console.log('🏥 V2: ======================================');
          
          // 🚨 PROFESSIONAL: محاسبه سهم‌ها از InsuranceCalculation (اولویت اصلی)
          const baseCovered = itemInsuranceInfo.totalCoverage > 0 ? itemInsuranceInfo.primaryCoverage : 0;
          const suppCovered = itemInsuranceInfo.totalCoverage > 0 ? itemInsuranceInfo.supplementaryCoverage : 0;
          const patientPayable = itemInsuranceInfo.totalCoverage > 0 ? itemInsuranceInfo.patientShare : total;
          
          var rowId = 'row-' + serviceId;
          
          // 🚨 PROFESSIONAL FIX: بررسی و حذف ردیف‌های تکراری قبل از افزودن
          const existingRows = $tb.find(`tr[data-service-id="${serviceId}"]`);
          if (existingRows.length > 0) {
            console.log('🏥 V2: Removing duplicate rows for ServiceId:', serviceId, 'Count:', existingRows.length);
            existingRows.remove();
          }
          
          // افزودن ردیف جدید
          $tb.append(`<tr id="${rowId}" data-service-id="${serviceId}" data-reception-item-id="${receptionItemId}" class="${itemInsuranceInfo.statusClass}">
            <td class="cell-code">${code}</td>
            <td class="cell-name">${name}</td>
            <td class="cell-qty">${qty}</td>
            <td class="cell-unit">${U.toIRR(unitPrice)}</td>
            <td class="cell-gross">${U.toIRR(total)}</td>
            <td class="cell-base">${U.toIRR(baseCovered)}</td>
            <td class="cell-supp">${U.toIRR(suppCovered)}</td>
            <td class="cell-patient">${U.toIRR(patientPayable)}</td>
            <td class="cell-coverage">${itemInsuranceInfo.statusBadge}</td>
            <td class="cell-actions"><button class="btn btn-link text-danger btn-sm remove-item" data-id="${serviceId}">حذف</button></td>
          </tr>`);
          
          // ذخیره اطلاعات بیمه
          const $row = $('#' + rowId);
          
          // ✅ ذخیره در jQuery data() (برای دسترسی سریع)
          $row.data('insurance', itemInsuranceCalc);
          if (pricingData) {
            $row.data('pricing', pricingData);
          }
          $row.data('item', newItem);
          
          // ✅ ذخیره در data-* attributes (برای پایداری و دسترسی از coverage-modal)
          // این روش پایدارتر است و حتی بعد از تغییر DOM هم کار می‌کند
          try {
            if (itemInsuranceCalc) {
              $row.attr('data-insurance-json', JSON.stringify(itemInsuranceCalc));
            }
            if (pricingData) {
              $row.attr('data-pricing-json', JSON.stringify(pricingData));
            }
            if (newItem) {
              $row.attr('data-item-json', JSON.stringify(newItem));
            }
            console.log('🏥 V2: Data saved to data-* attributes for row:', rowId);
          } catch (e) {
            console.warn('🏥 V2: Failed to save data to data-* attributes:', e);
          }
          
          if (receptionItemId) {
            $row.attr('data-reception-item-id', receptionItemId);
          }
          
          console.log('🏥 V2: ✅ Row added - ServiceId:', serviceId, 'CoverageStatus:', itemInsuranceInfo.coverageStatus, 
            'PrimaryCoverage:', baseCovered, 'SupplementaryCoverage:', suppCovered, 'PatientShare:', patientPayable);
        } else if (pricingData) {
          // Fallback: اگر items موجود نیست، از pricing استفاده کن
          const serviceId = itemData.ServiceId || itemData.serviceId || $("#ServiceId").val();
          const serviceCode = itemData.Code || itemData.code || '';
          const serviceName = itemData.Name || itemData.name || '';
          const qty = pricingData.Quantity || pricingData.quantity || itemData.Quantity || itemData.quantity || 1;
          const unitPrice = pricingData.UnitPriceIRR || pricingData.unitPriceIRR || 0;
          const gross = pricingData.GrossIRR || pricingData.grossIRR || 0;
          
          // 🚨 PROFESSIONAL FIX: تعریف insuranceInfo از insuranceCalc (که قبلاً استخراج شده)
          // اگر insuranceCalc موجود نیست، از pricing استفاده می‌کنیم
          const insuranceInfo = formatInsuranceCalculation(insuranceCalc);
          
          console.log('🏥 V2: Using pricing fallback - ServiceId:', serviceId, 'InsuranceCalc:', insuranceCalc, 'InsuranceInfo:', insuranceInfo);
          
          // 🚨 PROFESSIONAL: استفاده از اطلاعات بیمه real-time اگر موجود باشد، در غیر این صورت از pricing
          const baseCovered = insuranceInfo.totalCoverage > 0 ? insuranceInfo.primaryCoverage : 
                              (pricingData.BaseCoveredIRR || pricingData.baseCoveredIRR || 0);
          const suppCovered = insuranceInfo.totalCoverage > 0 ? insuranceInfo.supplementaryCoverage : 
                             (pricingData.SuppCoveredIRR || pricingData.suppCoveredIRR || 0);
          const patientPayable = insuranceInfo.totalCoverage > 0 ? insuranceInfo.patientShare : 
                                (pricingData.PatientPayableIRR || pricingData.patientPayableIRR || 0);
          
          // استفاده از Friendly strings اگر موجود باشند
          const unitPriceStr = pricingData.UnitPriceIRRStr || pricingData.unitPriceIRRStr || U.toIRR(unitPrice);
          const grossStr = pricingData.GrossIRRStr || pricingData.grossIRRStr || U.toIRR(gross);
          
          // 🚨 PROFESSIONAL: استفاده از اطلاعات بیمه real-time برای نمایش
          const baseStr = insuranceInfo.totalCoverage > 0 ? insuranceInfo.primaryCoverageStr : 
                         (pricingData.BaseCoveredIRRStr || pricingData.baseCoveredIRRStr || U.toIRR(baseCovered));
          const suppStr = insuranceInfo.totalCoverage > 0 ? insuranceInfo.supplementaryCoverageStr : 
                         (pricingData.SuppCoveredIRRStr || pricingData.suppCoveredIRRStr || U.toIRR(suppCovered));
          const patientStr = insuranceInfo.totalCoverage > 0 ? insuranceInfo.patientShareStr : 
                            (pricingData.PatientPayableIRRStr || pricingData.patientPayableIRRStr || U.toIRR(patientPayable));
          
          // 🚨 PROFESSIONAL FIX: بررسی و حذف ردیف‌های تکراری قبل از افزودن
          const existingRows = $tb.find(`tr[data-service-id="${serviceId}"]`);
          if (existingRows.length > 0) {
            console.log('🏥 V2: Removing duplicate rows for ServiceId:', serviceId, 'Count:', existingRows.length);
            existingRows.remove();
          }
          
          // ✅ ردیف با ستون‌های کامل: کد، نام، تعداد، فی، مبلغ کل، سهم پایه، سهم تکمیلی، سهم بیمار، وضعیت پوشش
          var rowId = 'row-' + (itemData.ReceptionItemId || itemData.receptionItemId || serviceId);
          $tb.append(`<tr id="${rowId}" data-service-id="${serviceId}" data-reception-item-id="${itemData.ReceptionItemId || itemData.receptionItemId || ''}" class="${insuranceInfo.statusClass}">
            <td class="cell-code">${serviceCode}</td>
            <td class="cell-name">${serviceName}</td>
            <td class="cell-qty">${qty}</td>
            <td class="cell-unit">${unitPriceStr}</td>
            <td class="cell-gross">${grossStr}</td>
            <td class="cell-base">${baseStr}</td>
            <td class="cell-supp">${suppStr}</td>
            <td class="cell-patient">${patientStr}</td>
            <td class="cell-coverage">${insuranceInfo.statusBadge}</td>
            <td class="cell-actions"><button class="btn btn-link text-danger btn-sm remove-item" data-id="${serviceId}">حذف</button></td>
          </tr>`);
          
          // ✅ ذخیره item، pricing و insuranceCalculation در data
          var $newRow = $('#' + rowId);
          
          // ✅ ذخیره در jQuery data() (برای دسترسی سریع)
          $newRow.data('item', itemData);
          $newRow.data('pricing', pricingData);
          $newRow.data('insurance', insuranceCalc);
          
          // ✅ ذخیره در data-* attributes (برای پایداری و دسترسی از coverage-modal)
          try {
            if (itemData) {
              $newRow.attr('data-item-json', JSON.stringify(itemData));
            }
            if (pricingData) {
              $newRow.attr('data-pricing-json', JSON.stringify(pricingData));
            }
            if (insuranceCalc) {
              $newRow.attr('data-insurance-json', JSON.stringify(insuranceCalc));
            }
            console.log('🏥 V2: Data saved to data-* attributes for row (fallback):', rowId);
          } catch (e) {
            console.warn('🏥 V2: Failed to save data to data-* attributes (fallback):', e);
          }
          
          console.log('🏥 V2: ✅ Row added (pricing fallback) - ServiceId:', serviceId, 'CoverageStatus:', insuranceInfo.coverageStatus, 
            'PrimaryCoverage:', baseCovered, 'SupplementaryCoverage:', suppCovered, 'PatientShare:', patientPayable);
          
          // استفاده از renderRowWithPricing اگر موجود است
          if (window.renderRowWithPricing && typeof window.renderRowWithPricing === 'function') {
            window.renderRowWithPricing(itemData, pricingData, insuranceCalc);
          } else if (window.ClinicApp && window.ClinicApp.ReceptionV2 && window.ClinicApp.ReceptionV2.PricingUI) {
            window.ClinicApp.ReceptionV2.PricingUI.renderRowWithPricing(itemData, pricingData, insuranceCalc);
          }
        } else {
          console.warn('🏥 V2: ⚠️ Neither items nor pricingData found in response');
        }
        
        // ✅ Update totals - استفاده از تابع updateTotalsUI اگر موجود باشد
        if (totals) {
          console.log('🏥 V2: Updating totals from AddItem response:', totals);
          if (window.insurancePanelModule && typeof window.insurancePanelModule.updateTotalsUI === 'function') {
            window.insurancePanelModule.updateTotalsUI(totals);
          } else {
            // Fallback: به‌روزرسانی مستقیم
            const gross = totals.GrossIRR || totals.grossIRR || totals.Gross || totals.gross || 0;
            const base = totals.BaseCoveredIRR || totals.baseCoveredIRR || totals.Base || totals.base || 0;
            const supp = totals.SuppCoveredIRR || totals.suppCoveredIRR || totals.Supplementary || totals.supplementary || 0;
            const patient = totals.PatientPayableIRR || totals.patientPayableIRR || totals.Patient || totals.patient || 0;
            
            const grossStr = totals.GrossIRRStr || totals.grossIRRStr || U.toIRR(gross);
            const baseStr = totals.BaseCoveredIRRStr || totals.baseCoveredIRRStr || U.toIRR(base);
            const suppStr = totals.SuppCoveredIRRStr || totals.suppCoveredIRRStr || U.toIRR(supp);
            const patientStr = totals.PatientPayableIRRStr || totals.patientPayableIRRStr || U.toIRR(patient);
            
            $("#Gross").text(grossStr).attr('data-value', gross);
            $("#InsurancePayable").text(baseStr).attr('data-value', base);
            $("#SuppPayable").text(suppStr).attr('data-value', supp);
            $("#PatientPayable").text(patientStr).attr('data-value', patient);
          }
        } else {
          console.warn('🏥 V2: Totals not found in AddItem response, attempting to fetch separately...');
          const receptionId = $("#ReceptionId").val();
          if (receptionId && window.insurancePanelModule && typeof window.insurancePanelModule.loadTotals === 'function') {
            window.insurancePanelModule.loadTotals(receptionId);
          }
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Add item error:', err);
        toastr.error('خطا در افزودن خدمت: ' + (err.message || 'خطای نامشخص'));
      })
      .always(function() {
        // 🚨 PROFESSIONAL FIX: استفاده از .always() به جای .finally() برای jQuery Deferred
        // 🚨 PROFESSIONAL: بازگرداندن دکمه به حالت عادی
        $btn.prop('disabled', false).text(originalText);
      });
  }

  $(document).on("click",".remove-item", function(){
    const serviceId = $(this).data("id");
    const payload = { 
      receptionId: $("#ReceptionId").val() || 0, 
      serviceId: serviceId 
    };
    
    API.post("/item/remove", payload)
      .then(function(fullResponse) {
        console.log('🏥 V2: Remove item raw response:', fullResponse);
        
        // Extract data using API.ok
        const response = API.ok(fullResponse);
        console.log('🏥 V2: Item removed:', response);
        
        toastr.success('خدمت حذف شد');
        
        // Update items grid - پشتیبانی از PascalCase و camelCase
        if (response && (response.items || response.Items)) {
          const items = response.items || response.Items || [];
          const $tb = $("#items-grid tbody").empty();
          items.forEach(function(it) {
            const code = it.code || it.Code || '';
            const name = it.name || it.Name || '';
            const qty = it.qty || it.Qty || 0;
            const unitPrice = it.unitPriceIRR || it.UnitPriceIRR || 0;
            const total = it.totalIRR || it.TotalIRR || 0;
            const serviceId = it.serviceId || it.ServiceId;
            const tariffWarning = it.tariffWarning || it.TariffWarning || null;
            
            // ✅ گام 3.2: افزودن نماد هشدار برای TariffWarning
            let warningIcon = '';
            if (tariffWarning) {
              warningIcon = ` <i class="fas fa-exclamation-triangle text-warning" 
                data-bs-toggle="tooltip" 
                data-bs-placement="top" 
                title="${tariffWarning}" 
                style="cursor: pointer;"></i>`;
            }
            
            $tb.append(`<tr>
              <td>${code}${warningIcon}</td><td>${name}</td><td>${qty}</td>
              <td>${U.toIRR(unitPrice)}</td><td>${U.toIRR(total)}</td>
              <td><button class="btn btn-link text-danger btn-sm remove-item" data-id="${serviceId}">حذف</button></td>
            </tr>`);
          });
          
          // ✅ گام 3.3: فعال‌سازی Bootstrap Tooltip
          if (window.bootstrap && typeof window.bootstrap.Tooltip !== 'undefined') {
            $('[data-bs-toggle="tooltip"]').each(function() {
              try {
                var existingTooltip = bootstrap.Tooltip.getInstance(this);
                if (existingTooltip) {
                  existingTooltip.dispose();
                }
                new bootstrap.Tooltip(this);
              } catch (err) {
                console.warn('🏥 V2: Error creating tooltip:', err);
              }
            });
          }
        }
        
        // ✅ Update totals - استفاده از تابع updateTotalsUI
        const totals = response.totals || response.Totals || null;
        if (totals) {
          console.log('🏥 V2: Updating totals from RemoveItem response:', totals);
          if (window.insurancePanelModule && typeof window.insurancePanelModule.updateTotalsUI === 'function') {
            window.insurancePanelModule.updateTotalsUI(totals);
          } else {
            // Fallback: به‌روزرسانی مستقیم
            const gross = totals.GrossIRR || totals.grossIRR || totals.Gross || totals.gross || 0;
            const base = totals.BaseCoveredIRR || totals.baseCoveredIRR || totals.Base || totals.base || 0;
            const supp = totals.SuppCoveredIRR || totals.suppCoveredIRR || totals.Supplementary || totals.supplementary || 0;
            const patient = totals.PatientPayableIRR || totals.patientPayableIRR || totals.Patient || totals.patient || 0;
            
            const grossStr = totals.GrossIRRStr || totals.grossIRRStr || U.toIRR(gross);
            const baseStr = totals.BaseCoveredIRRStr || totals.baseCoveredIRRStr || U.toIRR(base);
            const suppStr = totals.SuppCoveredIRRStr || totals.suppCoveredIRRStr || U.toIRR(supp);
            const patientStr = totals.PatientPayableIRRStr || totals.patientPayableIRRStr || U.toIRR(patient);
            
            $("#Gross").text(grossStr).attr('data-value', gross);
            $("#InsurancePayable").text(baseStr).attr('data-value', base);
            $("#SuppPayable").text(suppStr).attr('data-value', supp);
            $("#PatientPayable").text(patientStr).attr('data-value', patient);
          }
        } else {
          console.warn('🏥 V2: Totals not found in RemoveItem response, attempting to fetch separately...');
          const receptionId = $("#ReceptionId").val();
          if (receptionId && window.insurancePanelModule && typeof window.insurancePanelModule.loadTotals === 'function') {
            window.insurancePanelModule.loadTotals(receptionId);
          }
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Remove item error:', err);
        toastr.error('خطا در حذف خدمت');
      });
  });
  
  /**
   * ✅ تغییر خدمت/تعداد یک آیتم با پیش‌چک تعیین‌ست و Reprice
   * @param {number} itemId - ReceptionItemId
   * @param {number} serviceId - ServiceId جدید
   * @param {number} qty - Quantity جدید
   */
  async function changeItemService(itemId, serviceId, qty) {
    try {
      // ✅ Draft Orchestrator: اطمینان از وجود Draft
      if (!window.AutoDraftManager || typeof window.AutoDraftManager.ensureDraftOrSkip !== 'function') {
        console.error('🏥 V2: AutoDraftManager not available');
        toastr.error('سیستم پیش‌نویس در دسترس نیست. لطفاً صفحه را نوسازی کنید.');
        return;
      }

      const receptionId = parseInt($('#ReceptionId').val(), 10);
      const draft = await window.AutoDraftManager.ensureDraftOrSkip({
        patientId: $('#Patient_PatientId').val(),
        clinicId: $('#ClinicId').val(),
        departmentId: $('#DepartmentId').val(),
        doctorId: $('#DoctorId').val(),
        receptionId: receptionId
      });

      if (!draft || !draft.id) {
        console.warn('🏥 V2: Cannot change item service, draft creation failed or missing required fields');
        window.AutoDraftManager?.warnDraftMissing();
        return;
      }

      const payload = {
        receptionId: draft.id,
        receptionItemId: itemId,
        serviceId: serviceId,
        quantity: qty || 1,
        departmentId: parseInt($('#DepartmentId').val(), 10),
        doctorId: parseInt($('#DoctorId').val(), 10),
        financialYearId: window.ReceptionBootstrap?.FinancialYearId || 1, // TODO: از Bootstrap بگیر
        basePlanId: parseInt($('#BasePlanId').val(), 10) || null,
        supplementaryPlanId: parseInt($('#SuppPlanId').val(), 10) || null
      };

      console.log('🏥 V2: Changing item service:', payload);

      // ✅ Busy state برای ردیف
      const $row = $('#row-' + itemId);
      if ($row.length) {
        $row.addClass('table-warning');
      }

      const fullResponse = await API.post('/item/update-service', payload);
      
      // ✅ Busy state را بردار
      if ($row.length) {
        $row.removeClass('table-warning');
      }

      // بررسی پاسخ
      if (!fullResponse) {
        toastr.error('خطا در به‌روزرسانی آیتم');
        return;
      }

      const response = API.ok(fullResponse);
      const successValue = fullResponse?.Success ?? fullResponse?.success;
      const isSuccess = successValue === true || successValue === "true" || successValue === 1;

      // ✅ بررسی INSURANCE_SET_MISSING
      if (fullResponse?.Code === 'INSURANCE_SET_MISSING' || fullResponse?.code === 'INSURANCE_SET_MISSING') {
        const meta = fullResponse?.Metadata?.meta || fullResponse?.metadata?.meta || {};
        const createTariffUrl = meta.createTariffUrl || `/InsuranceTariff/Create?serviceId=${serviceId}&planId=${(payload.basePlanId || payload.supplementaryPlanId)}`;

        // ✅ Confirm Dialog
        const message = `
          برای این خدمت تعیین‌ست بیمه‌ای پیدا نشد.<br>
          می‌خواهید آیتم با «پرداخت کامل بیمار» ثبت شود؟
        `;

        // استفاده از SweetAlert2 اگر موجود است، وگرنه confirm ساده
        if (window.Swal && typeof window.Swal.fire === 'function') {
          const result = await window.Swal.fire({
            title: 'تعیین‌ست بیمه‌ای یافت نشد',
            html: message,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'ثبت با پرداخت کامل',
            cancelButtonText: 'انصراف',
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33'
          });

          if (result.isConfirmed) {
            // TODO: اگر مسیر سریع «پرداخت کامل بیمار» داری، از آن استفاده کن
            // فعلاً فقط پیام می‌دهیم
            toastr.info('این قابلیت در حال توسعه است. لطفاً تعیین‌ست بیمه‌ای را تعریف کنید.');
            // window.location.href = createTariffUrl; // اگر می‌خواهی لینک باز شود
          }
        } else {
          // Fallback: استفاده از confirm ساده
          const ok = confirm('برای این خدمت تعیین‌ست بیمه‌ای پیدا نشد.\nمی‌خواهید آیتم با «پرداخت کامل بیمار» ثبت شود؟');
          if (ok) {
            toastr.info('این قابلیت در حال توسعه است. لطفاً تعیین‌ست بیمه‌ای را تعریف کنید.');
          }
        }

        return;
      }

      if (!isSuccess) {
        const errorMsg = fullResponse?.Message || fullResponse?.message || 'خطا در به‌روزرسانی آیتم';
        console.warn('🏥 V2: UpdateItemService failed:', errorMsg, fullResponse);
        toastr.warning(errorMsg);
        return;
      }

      // ✅ موفق: به‌روزرسانی همان ردیف + Totals
      const pricing = response?.pricing || response?.Pricing || fullResponse?.Data?.pricing || fullResponse?.Data?.Pricing;
      const totals = response?.totals || response?.Totals || fullResponse?.Data?.totals || fullResponse?.Data?.Totals;

      if (pricing) {
        // به‌روزرسانی ردیف با pricing
        if (window.updateRowPricing && typeof window.updateRowPricing === 'function') {
          window.updateRowPricing(itemId, pricing);
        } else if (window.ClinicApp && window.ClinicApp.ReceptionV2 && window.ClinicApp.ReceptionV2.PricingUI) {
          window.ClinicApp.ReceptionV2.PricingUI.updateRowPricing(itemId, pricing);
        } else {
          // Fallback: استفاده از renderRowWithPricing
          const item = { ReceptionItemId: itemId, Id: itemId, receptionItemId: itemId, id: itemId };
          if (window.renderRowWithPricing && typeof window.renderRowWithPricing === 'function') {
            window.renderRowWithPricing(item, pricing);
          }
        }
      }

      if (totals) {
        // به‌روزرسانی Totals
        if (window.updateTotalsUI && typeof window.updateTotalsUI === 'function') {
          window.updateTotalsUI(totals);
        } else if (window.insurancePanelModule && typeof window.insurancePanelModule.updateTotalsUI === 'function') {
          window.insurancePanelModule.updateTotalsUI(totals);
        }
      }

      toastr.success('آیتم به‌روزرسانی و مجدد محاسبه شد.');
    } catch (err) {
      console.error('🏥 V2: Change item service error:', err);
      toastr.error('خطا در به‌روزرسانی خدمت آیتم');
    }
  }

  // Export برای استفاده در ماژول‌های دیگر
  window.serviceLookupModule = {
    loadServices: loadServices,
    changeItemService: changeItemService // ✅ جدید: برای تغییر خدمت
  };
  
  // Initialize - اگر دپارتمان از قبل انتخاب شده، خدمات را لود کن
  $(document).ready(function() {
    const deptId = $("#DepartmentId").val();
    if (deptId) {
      console.log('🏥 V2: Department already selected on init, loading services:', deptId);
      setTimeout(function() {
        loadServices(deptId);
      }, 500); // کمی تأخیر برای اطمینان از لود شدن dropdown
    }
  });
})(window.ReceptionAPI, window.RxUtils);
