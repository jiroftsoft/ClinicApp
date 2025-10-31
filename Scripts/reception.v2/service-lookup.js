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

  // Load eligible doctors when service changes
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
        
        // Update totals - پشتیبانی از PascalCase و camelCase
        if (response && (response.totals || response.Totals)) {
          const totals = response.totals || response.Totals || {};
          $("#Gross").text(U.toIRR(totals.gross || totals.Gross || 0));
          $("#InsurancePayable").text(U.toIRR(totals.base || totals.Base || 0));
          $("#SuppPayable").text(U.toIRR(totals.supplementary || totals.Supplementary || 0));
          $("#PatientPayable").text(U.toIRR(totals.patient || totals.Patient || 0)).attr("data-value", totals.patient || totals.Patient || 0);
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Remove item error:', err);
        toastr.error('خطا در حذف خدمت');
      });
  });
  
  // Export برای استفاده در ماژول‌های دیگر
  window.serviceLookupModule = {
    loadServices: loadServices
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
