(function(API, U){
  // Load services when department changes
  $("#DepartmentId").on('change', function() {
    const deptId = $(this).val();
    if(deptId) {
      loadServices(deptId);
    }
  });
  
  function loadServices(deptId) {
    API.get("/Api/ReceptionApi/GetServicesForDepartment", { deptId: deptId })
      .then(API.ok)
      .then(services => {
        console.log('🏥 V2: Services loaded:', services);
        const $serviceSelect = $("#ServiceId");
        $serviceSelect.empty().append('<option value="">انتخاب کنید</option>');
        services.forEach(service => {
          $serviceSelect.append(`<option value="${service.serviceId}">${service.serviceName} - ${U.toIRR(service.price)}</option>`);
        });
      })
      .catch(err => {
        console.error('🏥 V2: Services load error:', err);
        toastr.error('خطا در بارگذاری خدمات');
      });
  }

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
        window.AutoDraftManager.createDraft().then(draftId => {
          if (draftId) {
            $("#ReceptionId").val(draftId);
            proceedWithAddItem();
          } else {
            toastr.warning('لطفاً ابتدا پذیرش را ایجاد کنید');
          }
        }).catch(err => {
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
    
    API.post("/Api/ReceptionApi/AddItem", payload)
      .then(API.ok)
      .then(d=>{
        console.log('🏥 V2: Item added:', d);
        toastr.success('خدمت افزوده شد');
        
        // Update items grid
        const $tb = $("#items-grid tbody").empty();
        if(d.items && d.items.length > 0) {
          d.items.forEach(it => $tb.append(`<tr>
            <td>${it.code || ''}</td><td>${it.name || ''}</td><td>${it.qty || 0}</td>
            <td>${U.toIRR(it.unitPriceIRR || 0)}</td><td>${U.toIRR(it.totalIRR || 0)}</td>
            <td><button class="btn btn-link text-danger btn-sm remove-item" data-id="${it.serviceId}">حذف</button></td>
          </tr>`));
        }
        
        // Update totals
        if(d.totals) {
          $("#Gross").text(U.toIRR(d.totals.gross || 0));
          $("#InsurancePayable").text(U.toIRR(d.totals.base || 0));
          $("#SuppPayable").text(U.toIRR(d.totals.supplementary || 0));
          $("#PatientPayable").text(U.toIRR(d.totals.patient || 0)).attr("data-value", d.totals.patient || 0);
        }
      })
      .catch(err => {
        console.error('🏥 V2: Add item error:', err);
        toastr.error('خطا در افزودن خدمت');
      });
  });

  $(document).on("click",".remove-item", function(){
    const serviceId = $(this).data("id");
    const payload = { 
      receptionId: $("#ReceptionId").val() || 0, 
      serviceId: serviceId 
    };
    
    API.post("/Api/ReceptionApi/RemoveItem", payload)
      .then(API.ok)
      .then(() => {
        toastr.success('خدمت حذف شد');
        location.reload(); // Simple refresh for now
      })
      .catch(err => {
        console.error('🏥 V2: Remove item error:', err);
        toastr.error('خطا در حذف خدمت');
      });
  });
})(window.ReceptionAPI, window.RxUtils);
