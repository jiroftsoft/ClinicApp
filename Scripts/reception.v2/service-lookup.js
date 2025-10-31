(function(API, U){
  'use strict';

  /**
   * بارگذاری خدمات یک دپارتمان
   * پشتیبانی از PascalCase و camelCase
   */
  function loadServices(deptId) {
    if (!deptId || deptId === '' || deptId === '0') {
      console.warn('🏥 V2: Cannot load services - invalid department ID:', deptId);
      $("#ServiceId").empty().append('<option value="">ابتدا دپارتمان را انتخاب کنید</option>');
      return;
    }
    
    console.log('🏥 V2: Loading services for department:', deptId);
    
    API.get("/services/by-department", { deptId: deptId })
      .then(function(fullResponse) {
        console.log('🏥 V2: Services raw response:', fullResponse);
        
        // Extract data using API.ok (handles ServiceResult structure)
        const response = API.ok(fullResponse);
        console.log('🏥 V2: Services extracted data:', response);
        
        // پشتیبانی از PascalCase و camelCase
        // services ممکن است یک array باشد یا یک object با property Services
        let services = [];
        if (Array.isArray(response)) {
          services = response;
        } else if (response.Services || response.services) {
          services = response.Services || response.services || [];
        } else if (response.Data && Array.isArray(response.Data)) {
          services = response.Data;
        }
        
        console.log('🏥 V2: Services parsed:', services.length);
        
        const $serviceSelect = $("#ServiceId");
        $serviceSelect.empty().append('<option value="">انتخاب کنید</option>');
        
        if (services && services.length > 0) {
          services.forEach(function(service) {
            const serviceId = service.serviceId || service.ServiceId;
            const serviceName = service.serviceName || service.ServiceName || service.name || service.Name || '';
            const price = service.price || service.Price || service.unitPriceIRR || service.UnitPriceIRR || 0;
            $serviceSelect.append(`<option value="${serviceId}">${serviceName} - ${U.toIRR(price)}</option>`);
          });
          console.log('🏥 V2: Services filled:', services.length);
        } else {
          console.warn('🏥 V2: No services found for department:', deptId);
          $serviceSelect.append('<option value="">خدمتی یافت نشد</option>');
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Services load error:', err);
        toastr.error('خطا در بارگذاری خدمات');
        $("#ServiceId").empty().append('<option value="">خطا در بارگذاری</option>');
      });
  }
  
  // Load services when department changes
  $("#DepartmentId").on('change', function() {
    const deptId = $(this).val();
    console.log('🏥 V2: Department changed, loading services for:', deptId);
    if (deptId) {
      loadServices(deptId);
      // Reset service selection when department changes
      $("#ServiceId").val('').trigger('change');
    } else {
      $("#ServiceId").empty().append('<option value="">ابتدا دپارتمان را انتخاب کنید</option>');
    }
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
    
    API.post("/item/add", payload)
      .then(function(fullResponse) {
        console.log('🏥 V2: Add item raw response:', fullResponse);
        
        // Extract data using API.ok (handles ServiceResult structure)
        const response = API.ok(fullResponse);
        console.log('🏥 V2: Item added response:', response);
        
        // ✅ پشتیبانی از ساختار جدید: { item, pricing, totals }
        const itemData = response.item || response.Item || {};
        const pricingData = response.pricing || response.Pricing || null;
        const totalsData = response.totals || response.Totals || null;
        
        // ✅ اگر totals در پاسخ نیست، از Data.totals یا Data.Totals استفاده کن
        let totals = totalsData;
        if (!totals && response.Data) {
          totals = response.Data.totals || response.Data.Totals || null;
        }
        
        toastr.success('خدمت افزوده شد');
        
        // ✅ Update items grid - استفاده از pricing اگر موجود باشد
        const $tb = $("#items-grid tbody");
        
        // اگر pricing موجود است، ردیف جدید را با اطلاعات کامل pricing اضافه کن
        if (pricingData) {
          const serviceId = itemData.ServiceId || itemData.serviceId || $("#ServiceId").val();
          const serviceCode = itemData.Code || itemData.code || '';
          const serviceName = itemData.Name || itemData.name || '';
          const qty = pricingData.Quantity || pricingData.quantity || itemData.Quantity || itemData.quantity || 1;
          const unitPrice = pricingData.UnitPriceIRR || pricingData.unitPriceIRR || 0;
          const gross = pricingData.GrossIRR || pricingData.grossIRR || 0;
          const baseCovered = pricingData.BaseCoveredIRR || pricingData.baseCoveredIRR || 0;
          const suppCovered = pricingData.SuppCoveredIRR || pricingData.suppCoveredIRR || 0;
          const patientPayable = pricingData.PatientPayableIRR || pricingData.patientPayableIRR || 0;
          
          // استفاده از Friendly strings اگر موجود باشند
          const unitPriceStr = pricingData.UnitPriceIRRStr || pricingData.unitPriceIRRStr || U.toIRR(unitPrice);
          const grossStr = pricingData.GrossIRRStr || pricingData.grossIRRStr || U.toIRR(gross);
          const baseStr = pricingData.BaseCoveredIRRStr || pricingData.baseCoveredIRRStr || U.toIRR(baseCovered);
          const suppStr = pricingData.SuppCoveredIRRStr || pricingData.suppCoveredIRRStr || U.toIRR(suppCovered);
          const patientStr = pricingData.PatientPayableIRRStr || pricingData.patientPayableIRRStr || U.toIRR(patientPayable);
          
          // ✅ ردیف با ستون‌های کامل: کد، نام، تعداد، فی، مبلغ کل، سهم پایه، سهم تکمیلی، سهم بیمار
          var rowId = 'row-' + (itemData.ReceptionItemId || itemData.receptionItemId || serviceId);
          $tb.append(`<tr id="${rowId}" data-service-id="${serviceId}" data-reception-item-id="${itemData.ReceptionItemId || itemData.receptionItemId || ''}">
            <td class="cell-code">${serviceCode}</td>
            <td class="cell-name">${serviceName}</td>
            <td class="cell-qty">${qty}</td>
            <td class="cell-unit">${unitPriceStr}</td>
            <td class="cell-gross">${grossStr}</td>
            <td class="cell-base">${baseStr}</td>
            <td class="cell-supp">${suppStr}</td>
            <td class="cell-patient">${patientStr}</td>
            <td class="cell-coverage"><button class="btn btn-link text-danger btn-sm remove-item" data-id="${serviceId}">حذف</button></td>
          </tr>`);
          
          // ✅ ذخیره item و pricing در data و فراخوانی renderRowWithPricing برای badge + highlight
          var $newRow = $('#' + rowId);
          $newRow.data('item', itemData);
          $newRow.data('pricing', pricingData);
          
          // استفاده از renderRowWithPricing اگر موجود است
          if (window.renderRowWithPricing && typeof window.renderRowWithPricing === 'function') {
            window.renderRowWithPricing(itemData, pricingData);
          } else if (window.ClinicApp && window.ClinicApp.ReceptionV2 && window.ClinicApp.ReceptionV2.PricingUI) {
            window.ClinicApp.ReceptionV2.PricingUI.renderRowWithPricing(itemData, pricingData);
          }
        } else {
          // Fallback: اگر pricing موجود نیست، از items قدیمی استفاده کن
          const items = response.items || response.Items || [];
          $tb.empty();
          
          if (items && items.length > 0) {
            items.forEach(function(it) {
              const code = it.code || it.Code || '';
              const name = it.name || it.Name || '';
              const qty = it.qty || it.Qty || 0;
              const unitPrice = it.unitPriceIRR || it.UnitPriceIRR || 0;
              const total = it.totalIRR || it.TotalIRR || 0;
              const serviceId = it.serviceId || it.ServiceId;
              
              $tb.append(`<tr>
                <td>${code}</td><td>${name}</td><td>${qty}</td>
                <td>${U.toIRR(unitPrice)}</td><td>${U.toIRR(total)}</td>
                <td><button class="btn btn-link text-danger btn-sm remove-item" data-id="${serviceId}">حذف</button></td>
              </tr>`);
            });
          }
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
        toastr.error('خطا در افزودن خدمت');
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
            
            $tb.append(`<tr>
              <td>${code}</td><td>${name}</td><td>${qty}</td>
              <td>${U.toIRR(unitPrice)}</td><td>${U.toIRR(total)}</td>
              <td><button class="btn btn-link text-danger btn-sm remove-item" data-id="${serviceId}">حذف</button></td>
            </tr>`);
          });
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
