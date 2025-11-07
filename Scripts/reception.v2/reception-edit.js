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
        const baseUrl = '/api/v1/reception';

        $.ajax({
            url: `${baseUrl}/edit/${receptionId}`,
            method: 'GET',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function(response) {
                console.log('🏥 Reception Edit: Raw response:', response);

                // بررسی موفقیت
                if (!response || !response.Success) {
                    const errorMsg = response?.Message || 'خطا در بارگذاری اطلاعات پذیرش';
                    toastr.error(errorMsg, 'خطا');
                    return;
                }

                // استخراج داده‌ها
                let data = response.Data || response.data;
                if (API && API.ok && typeof API.ok === 'function') {
                    data = API.ok(response);
                }

                if (!data) {
                    toastr.error('داده‌ای دریافت نشد', 'خطا');
                    return;
                }

                console.log('🏥 Reception Edit: Reception data loaded:', data);

                // پر کردن فرم
                populateForm(data);

                // اعمال محدودیت‌های ویرایش
                applyEditPermissions(data.Permissions);

                // به‌روزرسانی وضعیت
                updateStatusBadge(data.Status);

                toastr.success('اطلاعات پذیرش بارگذاری شد', 'موفق');
            },
            error: function(xhr, status, error) {
                console.error('❌ Reception Edit: Error loading reception:', error);
                toastr.error('خطا در بارگذاری اطلاعات پذیرش', 'خطا');
            }
        });
    }

    /**
     * پر کردن فرم با داده‌های پذیرش
     */
    function populateForm(data) {
        console.log('🏥 Reception Edit: Populating form with data:', data);

        // اطلاعات بیمار (readonly)
        $('#Patient_NationalCode').val(data.PatientNationalCode || '').prop('readonly', true);
        $('#Patient_FullName').val(data.PatientFullName || '').prop('readonly', true);
        $('#Patient_Mobile').val(data.PatientMobile || '').prop('readonly', true);

        // اطلاعات پزشک و دپارتمان
        if (data.DoctorId) {
            $('#DoctorId').val(data.DoctorId);
        }
        if (data.DepartmentId) {
            $('#DepartmentId').val(data.DepartmentId);
        }
        if (data.ClinicId) {
            $('#ClinicId').val(data.ClinicId);
        }

        // تاریخ پذیرش
        if (data.ReceptionDateShamsi) {
            $('#ReceptionDate').val(data.ReceptionDateShamsi);
        }

        // بیمه‌ها
        if (data.BasePlanId) {
            $('#BasePlanId').val(data.BasePlanId);
        }
        if (data.SupplementaryPlanId) {
            $('#SupplementaryPlanId').val(data.SupplementaryPlanId);
        }

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
     */
    function updateTotals(data) {
        console.log('🏥 Reception Edit: Updating totals:', data);

        $('#TotalAmount').text(formatIRR(data.TotalAmount || 0));
        $('#InsurerShareAmount').text(formatIRR(data.InsurerShareAmount || 0));
        $('#PatientCoPay').text(formatIRR(data.PatientCoPay || 0));
        $('#PaidAmount').text(formatIRR(data.PaidAmount || 0));
        $('#RemainingAmount').text(formatIRR(data.RemainingAmount || 0));
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
        const baseUrl = '/api/v1/reception';

        // غیرفعال کردن دکمه ذخیره
        const $btn = $('#BtnSaveChanges');
        $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i> در حال ذخیره...');

        $.ajax({
            url: `${baseUrl}/update`,
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            contentType: 'application/json',
            data: JSON.stringify(request),
            success: function(response) {
                console.log('🏥 Reception Edit: Update response:', response);

                if (!response || !response.Success) {
                    const errorMsg = response?.Message || 'خطا در ذخیره تغییرات';
                    toastr.error(errorMsg, 'خطا');
                    $btn.prop('disabled', false).html('<i class="fas fa-save me-1"></i> ذخیره تغییرات');
                    return;
                }

                toastr.success('تغییرات با موفقیت ذخیره شد', 'موفق');

                // به‌روزرسانی نمایش
                if (response.Data) {
                    if (response.Data.Items) {
                        populateItems(response.Data.Items);
                    }
                    if (response.Data.Totals) {
                        updateTotalsFromResponse(response.Data.Totals);
                    }
                }

                // بازگشت به لیست پس از 2 ثانیه
                setTimeout(function() {
                    window.location.href = '/ReceptionV2/ReceptionList';
                }, 2000);
            },
            error: function(xhr, status, error) {
                console.error('❌ Reception Edit: Error saving changes:', error);
                toastr.error('خطا در ذخیره تغییرات', 'خطا');
                $btn.prop('disabled', false).html('<i class="fas fa-save me-1"></i> ذخیره تغییرات');
            }
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

        // به‌روزرسانی نمایش
        $('#TotalAmount').text(formatIRR(totalAmount));
        $('#PatientCoPay').text(formatIRR(totalPatientShare));
        $('#InsurerShareAmount').text(formatIRR(totalInsurerShare));
    }

    /**
     * به‌روزرسانی مجموع‌ها از پاسخ
     */
    function updateTotalsFromResponse(totals) {
        if (!totals) return;

        $('#TotalAmount').text(formatIRR(totals.GrossAmount || 0));
        $('#InsurerShareAmount').text(formatIRR(totals.BaseInsurancePayable || 0));
        $('#PatientCoPay').text(formatIRR(totals.PatientPayable || 0));
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
        const baseUrl = '/api/v1/reception';

        $.ajax({
            url: `${baseUrl}/edit/${receptionId}`,
            method: 'GET',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function(response) {
                if (!response || !response.Success) {
                    toastr.error('خطا در دریافت اطلاعات پذیرش', 'خطا');
                    return;
                }

                let data = response.Data || response.data;
                if (API && API.ok && typeof API.ok === 'function') {
                    data = API.ok(response);
                }

                const paidAmount = data.PaidAmount || 0;
                showCancelModal(receptionId, paidAmount);
            },
            error: function() {
                // اگر خطا رخ داد، با PaidAmount صفر ادامه بده
                showCancelModal(receptionId, 0);
            }
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
        const baseUrl = '/api/v1/reception';

        const request = {
            ReceptionId: receptionId,
            Reason: reason,
            ProcessRefund: processRefund,
            RefundReason: processRefund ? reason : null
        };

        console.log('🚫 Reception Edit: Sending cancel request:', request);

        $.ajax({
            url: `${baseUrl}/cancel`,
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            contentType: 'application/json',
            data: JSON.stringify(request),
            success: function(response) {
                console.log('🚫 Reception Edit: Cancel response:', response);

                if (!response || !response.Success) {
                    const errorMsg = response?.Message || 'خطا در لغو پذیرش';
                    toastr.error(errorMsg, 'خطا');
                    $('#btnConfirmCancel').prop('disabled', false).html('<i class="fas fa-ban me-1"></i>لغو پذیرش');
                    return;
                }

                // بستن مودال
                modal.hide();

                // نمایش پیام موفقیت
                const message = response.Data?.Message || 'پذیرش با موفقیت لغو شد';
                toastr.success(message, 'موفق');

                // بازگشت به لیست پس از 2 ثانیه
                setTimeout(function() {
                    window.location.href = '/ReceptionV2/ReceptionList';
                }, 2000);
            },
            error: function(xhr, status, error) {
                console.error('❌ Reception Edit: Error canceling reception:', error);
                toastr.error('خطا در لغو پذیرش', 'خطا');
                $('#btnConfirmCancel').prop('disabled', false).html('<i class="fas fa-ban me-1"></i>لغو پذیرش');
            }
        });
    }

})(window.API || window.ReceptionAPI || {}, jQuery);

