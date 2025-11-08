/**
 * Reception Edit Module - ماژول ویرایش پذیرش
 * 
 * ویژگی‌ها:
 * - بارگذاری اطلاعات پذیرش برای ویرایش
 * - اعمال محدودیت‌های ویرایش بر اساس وضعیت
 * - ذخیره تغییرات
 * - هماهنگی با modules موجود
 */
(function(API, $) {
    'use strict';

    // ✅ اطمینان از لود شدن DOM
    $(document).ready(function() {
        console.log('🏥 Reception Edit: Initializing module...');

        // بررسی اینکه آیا در حالت ویرایش هستیم
        if (!window.ReceptionEditMode || !window.ReceptionId) {
            console.log('🏥 Reception Edit: Not in edit mode, skipping initialization');
            return;
        }

        const receptionId = window.ReceptionId;
        console.log('🏥 Reception Edit: Edit mode enabled for ReceptionId:', receptionId);

        // بارگذاری اطلاعات پذیرش
        loadReceptionForEdit(receptionId);

        // Event handler برای ذخیره تغییرات
        $('#BtnSaveChanges').on('click', function(e) {
            e.preventDefault();
            saveChanges(receptionId);
        });

        // Event handler برای لغو پذیرش
        $('#BtnCancelReception').on('click', function(e) {
            e.preventDefault();
            handleCancelReception(receptionId);
        });
    });

    /**
     * بارگذاری اطلاعات پذیرش برای ویرایش
     */
    function loadReceptionForEdit(receptionId) {
        console.log('🏥 Reception Edit: Loading reception data - ReceptionId:', receptionId);

        const API = window.ReceptionAPI || window.API || {};

        // ✅ استفاده از ReceptionAPI.get() برای سازگاری با سایر ماژول‌ها
        API.get(`/edit/${receptionId}`)
            .then(function(fullResponse) {
                console.log('🏥 Reception Edit: Raw response:', fullResponse);

                // 🔍 چک Success - پشتیبانی از Success و success (camelCase/PascalCase)
                const successValue = fullResponse?.Success ?? fullResponse?.success;
                const isSuccess = successValue === true || successValue === "true" || successValue === 1;
                
                console.log('🏥 Reception Edit: Success check - successValue:', successValue, 'isSuccess:', isSuccess);

                // بررسی دقیق‌تر: اگر Success false است اما Data وجود دارد، ممکن است مشکل از ساختار response باشد
                if (!fullResponse || (!isSuccess && !fullResponse.Data)) {
                    const errorMsg = fullResponse?.Message || fullResponse?.message || 'خطا در بارگذاری اطلاعات پذیرش';
                    console.error('❌ Reception Edit: API returned error', {
                        response: fullResponse,
                        successValue: successValue,
                        isSuccess: isSuccess,
                        hasData: !!fullResponse?.Data
                    });
                    
                    toastr.error(errorMsg, 'خطا');
                    
                    // استفاده از handleErrorJson اگر موجود باشد
                    if (API && API.handleErrorJson && typeof API.handleErrorJson === 'function') {
                        API.handleErrorJson(fullResponse);
                    }
                    return;
                }

                // ✅ Extract data using API.ok (handles ServiceResult structure)
                let data = fullResponse.Data || fullResponse.data;
                if (API && API.ok && typeof API.ok === 'function') {
                    data = API.ok(fullResponse);
                }

                if (!data) {
                    console.error('❌ Reception Edit: No data extracted from response');
                    toastr.error('داده‌ای دریافت نشد', 'خطا');
                    return;
                }

                console.log('🏥 Reception Edit: Reception data loaded:', data);

                // پر کردن فرم
                populateForm(data);

                // اعمال محدودیت‌های ویرایش
                if (data.Permissions) {
                    applyEditPermissions(data.Permissions);
                }

                // به‌روزرسانی وضعیت
                if (data.Status !== undefined) {
                    updateStatusBadge(data.Status);
                }

                toastr.success('اطلاعات پذیرش بارگذاری شد', 'موفق');
            })
            .fail(function(jqXHR, textStatus, errorThrown) {
                console.error('❌ Reception Edit: Error loading reception:', {
                    status: jqXHR?.status,
                    statusText: jqXHR?.statusText,
                    error: errorThrown,
                    responseText: jqXHR?.responseText
                });
                
                // بررسی response JSON برای خطاهای خاص
                try {
                    if (jqXHR.responseJSON) {
                        if (API && API.handleErrorJson && typeof API.handleErrorJson === 'function') {
                            if (API.handleErrorJson(jqXHR.responseJSON)) {
                                return; // خطا handle شد
                            }
                        }
                    }
                } catch (e) {
                    // Ignore
                }
                
                toastr.error('خطا در بارگذاری اطلاعات پذیرش', 'خطا');
            });
    }

    /**
     * پر کردن فرم با داده‌های پذیرش
     */
    function populateForm(data) {
        console.log('🏥 Reception Edit: Populating form with data:', data);

        // اطلاعات بیمار (readonly) - کامل
        // ✅ ابتدا readonly را set کن، سپس value - برای جلوگیری از patient lookup در edit mode
        $('#Patient_NationalCode').prop('readonly', true).val(data.PatientNationalCode || '');
        $('#firstName').val(data.PatientFirstName || '');
        $('#lastName').val(data.PatientLastName || '');
        $('#fatherName').val(data.PatientFatherName || '');
        $('#gender').val(data.PatientGender || '');
        $('#birthSh').val(data.PatientBirthDateShamsi || '');
        $('#mobile').val(data.PatientMobile || '');
        $('#phone').val(data.PatientPhone || '');
        $('#address').val(data.PatientAddress || '');

        // اطلاعات کلینیک و دپارتمان
        if (data.ClinicId) {
            $('#ClinicId').val(data.ClinicId);
        }
        
        // ✅ ابتدا department را set کن و trigger کن تا doctors load شوند
        if (data.DepartmentId) {
            $('#DepartmentId').val(data.DepartmentId).trigger('change');
            
            // سپس با delay کوتاه، doctor را set کن (منتظر load شدن doctors)
            if (data.DoctorId) {
                setTimeout(function() {
                    $('#DoctorId').val(data.DoctorId);
                    console.log('🏥 Reception Edit: Doctor set to:', data.DoctorId);
                }, 300); // 300ms delay برای load شدن doctors
            }
        } else if (data.DoctorId) {
            // اگر department نداریم اما doctor داریم، مستقیماً set کن
            $('#DoctorId').val(data.DoctorId);
        }

        // تاریخ پذیرش
        if (data.ReceptionDateShamsi) {
            $('#ReceptionDate').val(data.ReceptionDateShamsi);
        }

        // بیمه‌ها - ✅ استفاده از delay برای اطمینان از load شدن options
        // insurance-panel.js در initialization خودش loadPlans() را call می‌کند
        // پس با یک delay کوتاه، اطمینان می‌دهیم که options load شده‌اند
        setTimeout(function() {
            console.log('🏥 Reception Edit: Setting insurance values');
            
            // حالا که option‌ها از insurance-panel initialization load شده‌اند، مقادیر را set کن
            if (data.BasePlanId) {
                const $basePlan = $('#BasePlanId');
                const basePlanOption = $basePlan.find('option[value="' + data.BasePlanId + '"]');
                console.log('🏥 Reception Edit: Base plan - ID:', data.BasePlanId, 'Option exists:', basePlanOption.length > 0);
                $basePlan.val(data.BasePlanId);
            }
            if (data.SupplementaryPlanId) {
                const $suppPlan = $('#SuppPlanId');
                const suppPlanOption = $suppPlan.find('option[value="' + data.SupplementaryPlanId + '"]');
                console.log('🏥 Reception Edit: Supplementary plan - ID:', data.SupplementaryPlanId, 'Option exists:', suppPlanOption.length > 0);
                
                // Log تمام options موجود برای debug
                const allOptions = [];
                $suppPlan.find('option').each(function() {
                    allOptions.push({ value: $(this).val(), text: $(this).text() });
                });
                console.log('🏥 Reception Edit: All supplementary plan options:', allOptions);
                
                $suppPlan.val(data.SupplementaryPlanId);
                
                // چک کنیم که آیا value واقعاً set شده است
                const actualValue = $suppPlan.val();
                console.log('🏥 Reception Edit: Supplementary plan set to:', data.SupplementaryPlanId, 'Actual value after set:', actualValue);
            }
            
            // به‌روزرسانی نمایش insurance status و toggle remove button
            if (window.insPanel && typeof window.insPanel.updateInsuranceStatus === 'function') {
                window.insPanel.updateInsuranceStatus();
            }
        }, 400); // 400ms delay برای اطمینان از load شدن insurance options

        // یادداشت‌ها
        if (data.Notes) {
            $('#Notes').val(data.Notes);
        }

        // خدمات
        if (data.Items && data.Items.length > 0) {
            populateItems(data.Items);
        }

        // مبالغ
        updateTotals(data);
    }

    /**
     * پر کردن لیست خدمات
     */
    function populateItems(items) {
        console.log('🏥 Reception Edit: Populating items:', items);

        // پاک کردن لیست موجود - استفاده از selector های مختلف برای سازگاری
        const $table = $('#items-grid tbody');
        if ($table.length === 0) {
            console.warn('⚠️ Reception Edit: Items table not found');
            return;
        }

        $table.empty();

        items.forEach(function(item) {
            // محاسبه مبالغ
            const unitPrice = item.UnitPrice || 0;
            const totalPrice = item.TotalPrice || (unitPrice * item.Quantity);
            const patientShare = item.PatientShareAmount || 0;
            const insurerShare = item.InsurerShareAmount || 0;
            
            // ساخت ردیف مطابق با ساختار موجود
            const rowId = 'row-' + (item.ReceptionItemId || item.ServiceId);
            const row = `
                <tr id="${rowId}" 
                    data-service-id="${item.ServiceId}" 
                    data-reception-item-id="${item.ReceptionItemId || ''}"
                    class="reception-item-row">
                    <td class="cell-code">${item.ServiceCode || '-'}</td>
                    <td class="cell-name">${item.ServiceName || '-'}</td>
                    <td class="cell-qty">
                        <input type="number" class="form-control form-control-sm item-quantity" 
                               value="${item.Quantity || 1}" min="1" 
                               data-item-id="${item.ReceptionItemId}"
                               data-service-id="${item.ServiceId}" />
                    </td>
                    <td class="cell-unit">${formatIRR(unitPrice)}</td>
                    <td class="cell-gross">${formatIRR(totalPrice)}</td>
                    <td class="cell-base">${formatIRR(insurerShare)}</td>
                    <td class="cell-supp">—</td>
                    <td class="cell-patient">${formatIRR(patientShare)}</td>
                    <td class="cell-coverage">
                        <button class="btn btn-link text-danger btn-sm remove-item" 
                                data-id="${item.ServiceId}"
                                data-item-id="${item.ReceptionItemId}">
                            حذف
                        </button>
                    </td>
                </tr>
            `;
            $table.append(row);
        });

        // Event handlers برای تغییرات
        $('.item-quantity').off('change').on('change', function() {
            const itemId = $(this).data('item-id');
            const serviceId = $(this).data('service-id');
            const quantity = parseInt($(this).val()) || 1;
            console.log('🏥 Reception Edit: Quantity changed - ItemId:', itemId, 'ServiceId:', serviceId, 'Quantity:', quantity);
            
            if (quantity < 1) {
                $(this).val(1);
                toastr.warning('تعداد باید حداقل 1 باشد');
                return;
            }
            
            // TODO: بازمحاسبه قیمت از طریق API
            // برای حالا فقط نمایش می‌دهیم
        });

        $('.remove-item').off('click').on('click', function() {
            const itemId = $(this).data('item-id');
            const serviceId = $(this).data('id');
            if (confirm('آیا از حذف این خدمت اطمینان دارید؟')) {
                $(this).closest('tr').fadeOut(300, function() {
                    $(this).remove();
                    // به‌روزرسانی مجموع‌ها
                    updateTotalsFromItems();
                });
            }
        });
    }

    /**
     * به‌روزرسانی مجموع‌ها
     * ✅ با selector های صحیح مطابق با View
     */
    function updateTotals(data) {
        console.log('🏥 Reception Edit: Updating totals:', data);

        // ✅ Selector های صحیح مطابق با _Totals.cshtml
        $('#Gross').text(formatIRR(data.TotalAmount || 0));
        $('#InsurancePayable').text(formatIRR(data.InsurerShareAmount || 0));
        $('#PatientPayable').text(formatIRR(data.PatientCoPay || 0));
        
        // این دو در _Totals.cshtml نیستند، احتمالاً در _Payment.cshtml هستند
        if ($('#PaidAmount').length) {
            $('#PaidAmount').text(formatIRR(data.PaidAmount || 0));
        }
        if ($('#RemainingAmount').length) {
            $('#RemainingAmount').text(formatIRR(data.RemainingAmount || 0));
        }
    }

    /**
     * اعمال محدودیت‌های ویرایش
     */
    function applyEditPermissions(permissions) {
        console.log('🏥 Reception Edit: Applying edit permissions:', permissions);

        if (!permissions) {
            console.warn('⚠️ Reception Edit: No permissions provided');
            return;
        }

        // غیرفعال کردن فیلدهای غیرقابل ویرایش
        if (!permissions.CanEditPatient) {
            $('#Patient_NationalCode, #Patient_FullName, #Patient_Mobile').prop('readonly', true).addClass('bg-light');
        }

        if (!permissions.CanEditDoctor) {
            $('#DoctorId').prop('disabled', true).addClass('bg-light');
        }

        if (!permissions.CanEditDepartment) {
            $('#DepartmentId, #ClinicId').prop('disabled', true).addClass('bg-light');
        }

        if (!permissions.CanEditServices) {
            $('.btn-add-service, .btn-remove-item').prop('disabled', true);
            $('.item-quantity').prop('readonly', true).addClass('bg-light');
        }

        if (!permissions.CanEditInsurances) {
            $('#BasePlanId, #SupplementaryPlanId').prop('disabled', true).addClass('bg-light');
        }

        if (!permissions.CanEditDate) {
            $('#ReceptionDate').prop('readonly', true).addClass('bg-light');
        }

        if (!permissions.CanEditNotes) {
            $('#Notes').prop('readonly', true).addClass('bg-light');
        }

        // نمایش هشدار در صورت نیاز به تایید
        if (permissions.RequiresApproval) {
            toastr.warning('این تغییرات نیاز به تایید مدیر دارد', 'هشدار');
        }
    }

    /**
     * به‌روزرسانی نشان وضعیت
     */
    function updateStatusBadge(status) {
        const $badge = $('#ReceptionStatusBadge');
        let badgeClass = 'bg-secondary';
        let badgeText = 'نامشخص';

        switch (status) {
            case 0: // Pending
                badgeClass = 'bg-warning';
                badgeText = 'در انتظار';
                break;
            case 1: // Completed
                badgeClass = 'bg-success';
                badgeText = 'تکمیل شده';
                break;
            case 2: // Cancelled
                badgeClass = 'bg-danger';
                badgeText = 'لغو شده';
                break;
        }

        $badge.removeClass('bg-secondary bg-warning bg-success bg-danger')
               .addClass(badgeClass)
               .text(badgeText);
    }

    /**
     * ذخیره تغییرات
     */
    function saveChanges(receptionId) {
        console.log('🏥 Reception Edit: Saving changes - ReceptionId:', receptionId);

        // ساخت درخواست
        const request = {
            ReceptionId: receptionId,
            DoctorId: $('#DoctorId').val() ? parseInt($('#DoctorId').val()) : null,
            DepartmentId: $('#DepartmentId').val() ? parseInt($('#DepartmentId').val()) : null,
            ClinicId: $('#ClinicId').val() ? parseInt($('#ClinicId').val()) : null,
            ReceptionDateShamsi: $('#ReceptionDate').val() || null,
            BasePlanId: $('#BasePlanId').val() ? parseInt($('#BasePlanId').val()) : null,
            SupplementaryPlanId: $('#SupplementaryPlanId').val() ? parseInt($('#SupplementaryPlanId').val()) : null,
            Notes: $('#Notes').val() || null,
            Items: collectItemsChanges(),
            RecalculatePrices: true
        };

        console.log('🏥 Reception Edit: Update request:', request);

        const API = window.ReceptionAPI || window.API || {};

        // غیرفعال کردن دکمه ذخیره
        const $btn = $('#BtnSaveChanges');
        $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i> در حال ذخیره...');

        API.post('/update', request)
            .then(function(fullResponse) {
                console.log('🏥 Reception Edit: Update response:', fullResponse);

                // 🔍 چک Success
                const successValue = fullResponse?.Success ?? fullResponse?.success;
                const isSuccess = successValue === true || successValue === "true" || successValue === 1;

                if (!fullResponse || !isSuccess) {
                    const errorMsg = fullResponse?.Message || fullResponse?.message || 'خطا در ذخیره تغییرات';
                    toastr.error(errorMsg, 'خطا');
                    $btn.prop('disabled', false).html('<i class="fas fa-save me-1"></i> ذخیره تغییرات');
                    
                    // استفاده از handleErrorJson اگر موجود باشد
                    if (API && API.handleErrorJson && typeof API.handleErrorJson === 'function') {
                        API.handleErrorJson(fullResponse);
                    }
                    return;
                }

                toastr.success('تغییرات با موفقیت ذخیره شد', 'موفق');

                // Extract data using API.ok
                let responseData = fullResponse.Data || fullResponse.data;
                if (API && API.ok && typeof API.ok === 'function') {
                    responseData = API.ok(fullResponse);
                }

                // به‌روزرسانی نمایش
                if (responseData) {
                    if (responseData.Items) {
                        populateItems(responseData.Items);
                    }
                    if (responseData.Totals) {
                        updateTotalsFromResponse(responseData.Totals);
                    }
                }

                // بازگشت به لیست پس از 2 ثانیه
                setTimeout(function() {
                    window.location.href = '/ReceptionV2/ReceptionList';
                }, 2000);
            })
            .fail(function(jqXHR, textStatus, errorThrown) {
                console.error('❌ Reception Edit: Error saving changes:', {
                    status: jqXHR?.status,
                    statusText: jqXHR?.statusText,
                    error: errorThrown,
                    responseText: jqXHR?.responseText
                });
                
                // بررسی response JSON برای خطاهای خاص
                try {
                    if (jqXHR.responseJSON) {
                        if (API && API.handleErrorJson && typeof API.handleErrorJson === 'function') {
                            if (API.handleErrorJson(jqXHR.responseJSON)) {
                                $btn.prop('disabled', false).html('<i class="fas fa-save me-1"></i> ذخیره تغییرات');
                                return; // خطا handle شد
                            }
                        }
                    }
                } catch (e) {
                    // Ignore
                }
                
                toastr.error('خطا در ذخیره تغییرات', 'خطا');
                $btn.prop('disabled', false).html('<i class="fas fa-save me-1"></i> ذخیره تغییرات');
            });
    }

    /**
     * جمع‌آوری تغییرات آیتم‌ها
     */
    function collectItemsChanges() {
        const items = [];

        // استفاده از selector های مختلف برای سازگاری
        $('#items-grid tbody tr, [data-reception-item-id]').each(function() {
            const $row = $(this);
            const itemId = $row.data('reception-item-id') || $row.data('item-id');
            const serviceId = $row.data('service-id');
            const quantity = parseInt($row.find('.item-quantity').val()) || 1;

            if (serviceId) {
                items.push({
                    ReceptionItemId: itemId || null,
                    ServiceId: serviceId,
                    Quantity: quantity,
                    IsDeleted: false
                });
            }
        });

        console.log('🏥 Reception Edit: Collected items changes:', items);
        return items;
    }

    /**
     * به‌روزرسانی مجموع‌ها از آیتم‌های موجود در جدول
     */
    function updateTotalsFromItems() {
        let totalAmount = 0;
        let totalPatientShare = 0;
        let totalInsurerShare = 0;

        $('#items-grid tbody tr, [data-reception-item-id]').each(function() {
            const $row = $(this);
            const quantity = parseInt($row.find('.item-quantity').val()) || 1;
            const unitPriceText = $row.find('.cell-unit').text().replace(/[^\d]/g, '');
            const unitPrice = parseFloat(unitPriceText) || 0;
            
            totalAmount += unitPrice * quantity;
            
            const patientShareText = $row.find('.cell-patient').text().replace(/[^\d]/g, '');
            const patientShare = parseFloat(patientShareText) || 0;
            totalPatientShare += patientShare * quantity;
            
            const insurerShareText = $row.find('.cell-base').text().replace(/[^\d]/g, '');
            const insurerShare = parseFloat(insurerShareText) || 0;
            totalInsurerShare += insurerShare * quantity;
        });

        // به‌روزرسانی نمایش با selector های صحیح
        $('#Gross').text(formatIRR(totalAmount));
        $('#PatientPayable').text(formatIRR(totalPatientShare));
        $('#InsurancePayable').text(formatIRR(totalInsurerShare));
    }

    /**
     * به‌روزرسانی مجموع‌ها از پاسخ
     */
    function updateTotalsFromResponse(totals) {
        if (!totals) return;

        // ✅ Selector های صحیح مطابق با View
        $('#Gross').text(formatIRR(totals.GrossAmount || 0));
        $('#InsurancePayable').text(formatIRR(totals.BaseInsurancePayable || 0));
        $('#PatientPayable').text(formatIRR(totals.PatientPayable || 0));
    }

    /**
     * فرمت مبلغ به ریال
     */
    function formatIRR(amount) {
        if (!amount && amount !== 0) return '—';
        return Number(amount).toLocaleString('fa-IR') + ' ریال';
    }

    /**
     * لغو پذیرش
     */
    function handleCancelReception(receptionId) {
        console.log('🚫 Reception Edit: Cancel reception - ReceptionId:', receptionId);

        // دریافت اطلاعات پذیرش برای نمایش PaidAmount
        const API = window.ReceptionAPI || window.API || {};

        API.get(`/edit/${receptionId}`)
            .then(function(fullResponse) {
                // 🔍 چک Success
                const successValue = fullResponse?.Success ?? fullResponse?.success;
                const isSuccess = successValue === true || successValue === "true" || successValue === 1;

                if (!fullResponse || !isSuccess) {
                    console.warn('⚠️ Reception Edit: Failed to load reception for cancel, using PaidAmount = 0');
                    showCancelModal(receptionId, 0);
                    return;
                }

                // Extract data using API.ok
                let data = fullResponse.Data || fullResponse.data;
                if (API && API.ok && typeof API.ok === 'function') {
                    data = API.ok(fullResponse);
                }

                const paidAmount = data?.PaidAmount || 0;
                showCancelModal(receptionId, paidAmount);
            })
            .fail(function() {
                // اگر خطا رخ داد، با PaidAmount صفر ادامه بده
                console.warn('⚠️ Reception Edit: Error loading reception for cancel, using PaidAmount = 0');
                showCancelModal(receptionId, 0);
            });
    }

    /**
     * نمایش مودال لغو پذیرش
     */
    function showCancelModal(receptionId, paidAmount) {
        const hasPayment = paidAmount > 0;
        const paymentWarning = hasPayment 
            ? `<div class="alert alert-warning">
                <i class="fas fa-exclamation-triangle me-2"></i>
                <strong>هشدار:</strong> این پذیرش دارای پرداخت به مبلغ <strong>${formatIRR(paidAmount)}</strong> است.
                با لغو این پذیرش، مبلغ پرداخت شده باید برگشت داده شود.
            </div>`
            : '';

        const modalHtml = `
            <div class="modal fade" id="cancelReceptionModal" tabindex="-1" aria-labelledby="cancelReceptionModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header bg-danger text-white">
                            <h5 class="modal-title" id="cancelReceptionModalLabel">
                                <i class="fas fa-ban me-2"></i>لغو پذیرش
                            </h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            ${paymentWarning}
                            <div class="mb-3">
                                <label for="cancelReason" class="form-label">
                                    دلیل لغو <span class="text-danger">*</span>
                                </label>
                                <textarea class="form-control" id="cancelReason" rows="4" 
                                          placeholder="لطفاً دلیل لغو پذیرش را به صورت کامل وارد کنید (حداقل 10 کاراکتر)"
                                          required></textarea>
                                <small class="form-text text-muted">حداقل 10 کاراکتر الزامی است</small>
                            </div>
                            ${hasPayment ? `
                                <div class="form-check mb-3">
                                    <input class="form-check-input" type="checkbox" id="processRefund" checked>
                                    <label class="form-check-label" for="processRefund">
                                        برگشت وجه پرداخت شده
                                    </label>
                                </div>
                            ` : ''}
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">انصراف</button>
                            <button type="button" class="btn btn-danger" id="btnConfirmCancel">
                                <i class="fas fa-ban me-1"></i>لغو پذیرش
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // حذف مودال قبلی اگر وجود دارد
        $('#cancelReceptionModal').remove();

        // اضافه کردن مودال به DOM
        $('body').append(modalHtml);

        // نمایش مودال
        const modal = new bootstrap.Modal(document.getElementById('cancelReceptionModal'));
        modal.show();

        // Event handler برای تایید لغو
        $('#btnConfirmCancel').off('click').on('click', function() {
            const reason = $('#cancelReason').val().trim();
            const processRefund = $('#processRefund').is(':checked');

            if (!reason || reason.length < 10) {
                toastr.error('لطفاً دلیل لغو را به صورت کامل وارد کنید (حداقل 10 کاراکتر)', 'خطا');
                $('#cancelReason').focus();
                return;
            }

            if (hasPayment && !processRefund) {
                toastr.error('برای لغو پذیرش با پرداخت، باید برگشت وجه انجام شود', 'خطا');
                return;
            }

            // غیرفعال کردن دکمه
            $(this).prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i>در حال لغو...');

            // ارسال درخواست لغو
            cancelReceptionRequest(receptionId, reason, processRefund, modal);
        });

        // پاک کردن مودال هنگام بسته شدن
        $('#cancelReceptionModal').on('hidden.bs.modal', function() {
            $(this).remove();
        });
    }

    /**
     * ارسال درخواست لغو پذیرش
     */
    function cancelReceptionRequest(receptionId, reason, processRefund, modal) {
        const API = window.ReceptionAPI || window.API || {};

        const request = {
            ReceptionId: receptionId,
            Reason: reason,
            ProcessRefund: processRefund,
            RefundReason: processRefund ? reason : null
        };

        console.log('🚫 Reception Edit: Sending cancel request:', request);

        API.post('/cancel', request)
            .then(function(fullResponse) {
                console.log('🚫 Reception Edit: Cancel response:', fullResponse);

                // 🔍 چک Success
                const successValue = fullResponse?.Success ?? fullResponse?.success;
                const isSuccess = successValue === true || successValue === "true" || successValue === 1;

                if (!fullResponse || !isSuccess) {
                    const errorMsg = fullResponse?.Message || fullResponse?.message || 'خطا در لغو پذیرش';
                    toastr.error(errorMsg, 'خطا');
                    $('#btnConfirmCancel').prop('disabled', false).html('<i class="fas fa-ban me-1"></i>لغو پذیرش');
                    
                    // استفاده از handleErrorJson اگر موجود باشد
                    if (API && API.handleErrorJson && typeof API.handleErrorJson === 'function') {
                        API.handleErrorJson(fullResponse);
                    }
                    return;
                }

                // بستن مودال
                modal.hide();

                // Extract data using API.ok
                let responseData = fullResponse.Data || fullResponse.data;
                if (API && API.ok && typeof API.ok === 'function') {
                    responseData = API.ok(fullResponse);
                }

                // نمایش پیام موفقیت
                const message = responseData?.Message || 'پذیرش با موفقیت لغو شد';
                toastr.success(message, 'موفق');

                // بازگشت به لیست پس از 2 ثانیه
                setTimeout(function() {
                    window.location.href = '/ReceptionV2/ReceptionList';
                }, 2000);
            })
            .fail(function(jqXHR, textStatus, errorThrown) {
                console.error('❌ Reception Edit: Error canceling reception:', {
                    status: jqXHR?.status,
                    statusText: jqXHR?.statusText,
                    error: errorThrown,
                    responseText: jqXHR?.responseText
                });
                
                // بررسی response JSON برای خطاهای خاص
                try {
                    if (jqXHR.responseJSON) {
                        if (API && API.handleErrorJson && typeof API.handleErrorJson === 'function') {
                            if (API.handleErrorJson(jqXHR.responseJSON)) {
                                $('#btnConfirmCancel').prop('disabled', false).html('<i class="fas fa-ban me-1"></i>لغو پذیرش');
                                return; // خطا handle شد
                            }
                        }
                    }
                } catch (e) {
                    // Ignore
                }
                
                toastr.error('خطا در لغو پذیرش', 'خطا');
                $('#btnConfirmCancel').prop('disabled', false).html('<i class="fas fa-ban me-1"></i>لغو پذیرش');
            });
    }

})(window.API || window.ReceptionAPI || {}, jQuery);

