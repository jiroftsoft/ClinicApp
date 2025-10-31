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
    } else {
      $("#ServiceId").empty().append('<option value="">ابتدا دپارتمان را انتخاب کنید</option>');
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
    
    const receptionId = $("#ReceptionId").val();
    if(!receptionId || receptionId <= 0) {
      // Try to create auto-draft first
      if (window.AutoDraftManager && !window.AutoDraftManager.isDraftCreated()) {
        window.AutoDraftManager.createDraft().then(function(draftId) {
          if (draftId) {
            $("#ReceptionId").val(draftId);
            proceedWithAddItem();
          } else {
            toastr.warning('لطفاً ابتدا پذیرش را ایجاد کنید');
          }
        }).catch(function(err) {
          console.error('🏥 V2: Auto-draft creation error:', err);
          toastr.warning('لطفاً ابتدا پذیرش را ایجاد کنید');
        });
        return;
      } else {
        toastr.warning('لطفاً ابتدا پذیرش را ایجاد کنید');
        return;
      }
    }
    
    proceedWithAddItem();
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
        
        toastr.success('خدمت افزوده شد');
        
        // Update items grid - پشتیبانی از PascalCase و camelCase
        const $tb = $("#items-grid tbody").empty();
        const items = response.items || response.Items || [];
        
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
        
        // Update totals - پشتیبانی از PascalCase و camelCase
        const totals = response.totals || response.Totals || {};
        if (totals) {
          $("#Gross").text(U.toIRR(totals.gross || totals.Gross || 0));
          $("#InsurancePayable").text(U.toIRR(totals.base || totals.Base || 0));
          $("#SuppPayable").text(U.toIRR(totals.supplementary || totals.Supplementary || 0));
          $("#PatientPayable").text(U.toIRR(totals.patient || totals.Patient || 0)).attr("data-value", totals.patient || totals.Patient || 0);
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
