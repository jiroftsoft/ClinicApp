/**
 * Reception List Module - لیست پذیرش‌ها (مراجعات قبلی)
 * 
 * ویژگی‌ها:
 * - نمایش لیست پذیرش‌ها با فیلتر و صفحه‌بندی
 * - امکان پرداخت مجدد با POS
 * - چاپ قبض پرداخت و بیمه تکمیلی
 * - مدیریت بدهی‌ها
 * - بهینه‌سازی برای محیط درمانی (سریع، چابک، حرفه‌ای)
 */
(function(API) {
    'use strict';

    // ✅ اطمینان از لود شدن DOM
    $(document).ready(function() {
        console.log('🏥 Reception List: Initializing module...');

        const config = window.ReceptionListConfig || {};
        // استفاده از ReceptionAPI اگر موجود باشد، در غیر این صورت از API محلی
        const API = window.ReceptionAPI || window.API || {};
        let currentPage = 1;
        const pageSize = 20;
        let currentFilters = {};
        let isLoading = false;

        /**
         * دریافت توکن Anti-Forgery
         */
        function getAntiForgeryToken() {
            // بررسی چند منبع برای توکن
            let token = $('input[name="__RequestVerificationToken"]').val();
            if (!token) {
                const $meta = $('meta[name="__RequestVerificationToken"]');
                if ($meta.length) {
                    token = $meta.attr('content');
                }
            }
            if (!token) {
                console.warn('⚠️ Reception List: Anti-forgery token not found');
            }
            return token || '';
        }

        /**
         * فرمت مبلغ به ریال
         */
        function formatIRR(amount) {
            if (!amount && amount !== 0) return '—';
            return Number(amount).toLocaleString('fa-IR') + ' ریال';
        }

        /**
         * فرمت تاریخ شمسی
         */
        function formatDateShamsi(dateStr) {
            return dateStr || '—';
        }

        /**
         * دریافت کلاس وضعیت
         */
        function getStatusBadgeClass(status) {
            switch(status) {
                case 0: return 'bg-warning'; // در انتظار
                case 1: return 'bg-success'; // تکمیل شده
                case 2: return 'bg-danger';   // لغو شده
                case 3: return 'bg-info';    // در حال انجام
                case 4: return 'bg-warning'; // نیاز به پرداخت بیشتر
                default: return 'bg-secondary';
            }
        }

        /**
         * دریافت متن وضعیت
         */
        function getStatusText(status) {
            switch(status) {
                case 0: return 'در انتظار';
                case 1: return 'تکمیل شده';
                case 2: return 'لغو شده';
                case 3: return 'در حال انجام';
                case 4: return 'نیاز به پرداخت بیشتر';
                default: return 'نامشخص';
            }
        }

        /**
         * بارگذاری لیست پذیرش‌ها - بهینه‌سازی شده برای محیط درمانی
         */
        function loadReceptionList(page = 1) {
            // جلوگیری از درخواست‌های تکراری
            if (isLoading) {
                console.warn('⚠️ Reception List: Request already in progress');
                return;
            }

            currentPage = page;
            isLoading = true;
            
            const filters = {
                NationalCode: $('#filterNationalCode').val()?.trim() || null,
                PatientName: $('#filterPatientName').val()?.trim() || null,
                DateFrom: $('#filterDateFrom').val()?.trim() || null,
                DateTo: $('#filterDateTo').val()?.trim() || null,
                Status: $('#filterStatus').val() ? parseInt($('#filterStatus').val()) : null,
                ReceptionNo: $('#filterReceptionNo').val()?.trim() || null, // 🏥 MEDICAL: فیلتر شماره پذیرش
                ElectronicReceptionNumber: $('#filterElectronicReceptionNumber').val()?.trim() || null // 🏥 MEDICAL: فیلتر شماره الکترونیکی
            };

            // حذف null values برای بهینه‌سازی
            Object.keys(filters).forEach(key => {
                if (filters[key] === null || filters[key] === '') {
                    delete filters[key];
                }
            });

            currentFilters = filters;

            const $container = $('#receptionListContainer');
            const $btnSearch = $('#btnSearch');
            
            // نمایش loading state
            $btnSearch.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-1"></i>در حال جستجو...');
            $container.html(`
                <div class="text-center py-5">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">در حال بارگذاری...</span>
                    </div>
                    <p class="mt-2 text-muted">در حال بارگذاری لیست پذیرش‌ها...</p>
                </div>
            `);

            const payload = {
                filters: filters,
                page: page,
                pageSize: pageSize
            };

            const apiUrl = config.apiUrl || '/ReceptionV2/ReceptionList/GetReceptionList';
            console.log('🏥 Reception List: Loading page', page, 'with filters:', filters);
            console.log('🏥 Reception List: API URL:', apiUrl);

            // استفاده از AJAX مستقیم با error handling بهتر
            $.ajax({
                url: apiUrl,
                method: 'POST',
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken(),
                    'X-RequestVerificationToken': getAntiForgeryToken(),
                    'Content-Type': 'application/json; charset=utf-8',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                data: JSON.stringify(payload),
                dataType: 'json',
                cache: false,
                timeout: 30000 // 30 ثانیه timeout برای محیط درمانی
            })
            .done(function(fullResponse) {
                isLoading = false;
                $btnSearch.prop('disabled', false).html('<i class="fas fa-search me-1"></i>جستجو');
                
                console.log('🏥 Reception List: Raw response received', fullResponse);
                console.log('🏥 Reception List: Response type:', typeof fullResponse);
                console.log('🏥 Reception List: Response keys:', fullResponse ? Object.keys(fullResponse) : 'null/undefined');
                
                // اگر response به صورت string است، آن را parse کن
                let responseObj = fullResponse;
                if (typeof fullResponse === 'string') {
                    try {
                        responseObj = JSON.parse(fullResponse);
                        console.log('🏥 Reception List: Response parsed from string', responseObj);
                    } catch (e) {
                        console.error('❌ Reception List: Failed to parse JSON response', e);
                        $container.html(`
                            <div class="alert alert-danger">
                                <i class="fas fa-times-circle me-2"></i>
                                خطا در پردازش پاسخ سرور. لطفاً صفحه را نوسازی کنید.
                            </div>
                        `);
                        toastr.error('خطا در پردازش پاسخ سرور');
                        return;
                    }
                }
                
                // بررسی Success - پشتیبانی از Success و success (camelCase/PascalCase)
                const successValue = responseObj?.Success ?? responseObj?.success;
                const isSuccess = successValue === true || successValue === "true" || successValue === 1;
                
                console.log('🏥 Reception List: Success check - successValue:', successValue, 'isSuccess:', isSuccess);
                console.log('🏥 Reception List: Has Data?', !!responseObj?.Data, 'Data type:', typeof responseObj?.Data);
                console.log('🏥 Reception List: Data value:', responseObj?.Data);
                console.log('🏥 Reception List: Message:', responseObj?.Message || responseObj?.message);
                console.log('🏥 Reception List: Code:', responseObj?.Code || responseObj?.code);
                
                // بررسی دقیق‌تر: اگر Success false است اما Data وجود دارد، ممکن است مشکل از ساختار response باشد
                if (!responseObj || (!isSuccess && !responseObj.Data)) {
                    const errorMsg = responseObj?.Message || responseObj?.message || 'خطا در دریافت لیست پذیرش‌ها';
                    console.error('❌ Reception List: API returned error', {
                        response: responseObj,
                        successValue: successValue,
                        isSuccess: isSuccess,
                        hasData: !!responseObj?.Data
                    });
                    
                    $container.html(`
                        <div class="alert alert-warning">
                            <i class="fas fa-exclamation-triangle me-2"></i>
                            ${errorMsg}
                        </div>
                    `);
                    
                    // استفاده از handleErrorJson اگر موجود باشد
                    if (API && API.handleErrorJson && typeof API.handleErrorJson === 'function') {
                        API.handleErrorJson(responseObj);
                    } else if (window.ReceptionAPI && window.ReceptionAPI.handleErrorJson) {
                        window.ReceptionAPI.handleErrorJson(responseObj);
                    }
                    return;
                }
                
                // Extract data using API.ok (handles ServiceResult structure)
                let data = responseObj.Data || responseObj.data;
                
                // اگر API.ok موجود است، از آن استفاده کن
                if (API && API.ok && typeof API.ok === 'function') {
                    data = API.ok(responseObj);
                } else if (window.ReceptionAPI && window.ReceptionAPI.ok) {
                    data = window.ReceptionAPI.ok(responseObj);
                }
                
                console.log('🏥 Reception List: Extracted data', data);
                console.log('🏥 Reception List: Data type:', typeof data);
                console.log('🏥 Reception List: Data keys:', data ? Object.keys(data) : 'null/undefined');
                
                // بررسی اینکه data معتبر است
                if (!data) {
                    console.error('❌ Reception List: Data is null or undefined', responseObj);
                    $container.html(`
                        <div class="alert alert-warning">
                            <i class="fas fa-exclamation-triangle me-2"></i>
                            داده‌ای دریافت نشد. لطفاً دوباره تلاش کنید.
                        </div>
                    `);
                    return;
                }
                
                // رندر لیست
                renderReceptionList(data);
                renderPagination(data);
                
                // نمایش تعداد نتایج
                const totalCount = data.TotalCount || data.totalCount || 0;
                if (totalCount > 0) {
                    toastr.success(`${totalCount} پذیرش یافت شد`, '', { timeOut: 2000 });
                } else {
                    console.log('🏥 Reception List: No items found');
                }
            })
            .fail(function(xhr, status, error) {
                isLoading = false;
                $btnSearch.prop('disabled', false).html('<i class="fas fa-search me-1"></i>جستجو');
                
                console.error('❌ Reception List: Load failed', {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    error: error,
                    responseText: xhr.responseText?.substring(0, 500)
                });
                
                let errorMessage = 'خطا در ارتباط با سرور. لطفاً دوباره تلاش کنید.';
                
                if (xhr.status === 404) {
                    errorMessage = 'آدرس درخواست یافت نشد. لطفاً صفحه را نوسازی کنید.';
                } else if (xhr.status === 500) {
                    errorMessage = 'خطای سرور رخ داد. لطفاً با پشتیبانی تماس بگیرید.';
                } else if (xhr.status === 0 || status === 'timeout') {
                    errorMessage = 'اتصال به سرور برقرار نشد. لطفاً اتصال اینترنت خود را بررسی کنید.';
                }
                
                // تلاش برای parse کردن response JSON
                try {
                    const jsonResponse = JSON.parse(xhr.responseText);
                    if (jsonResponse && API.handleErrorJson && typeof API.handleErrorJson === 'function') {
                        API.handleErrorJson(jsonResponse);
                        return;
                    }
                } catch (e) {
                    // Ignore parse errors
                }
                
                $container.html(`
                    <div class="alert alert-danger">
                        <i class="fas fa-times-circle me-2"></i>
                        ${errorMessage}
                        <br><small class="text-muted">کد خطا: ${xhr.status || 'نامشخص'}</small>
                    </div>
                `);
                
                toastr.error(errorMessage, 'خطا', { timeOut: 5000 });
            });
        }

        /**
         * رندر لیست پذیرش‌ها - بهینه‌سازی شده برای محیط درمانی
         */
        function renderReceptionList(data) {
            const $container = $('#receptionListContainer');
            
            // پشتیبانی از هر دو حالت: Items و items (PascalCase و camelCase)
            const items = data.Items || data.items || [];
            const totalCount = data.TotalCount || data.totalCount || 0;
            
            console.log('🏥 Reception List: Rendering list', {
                itemsCount: items.length,
                totalCount: totalCount,
                dataKeys: Object.keys(data)
            });
            
            if (!items || items.length === 0) {
                $container.html(`
                    <div class="alert alert-info">
                        <i class="fas fa-info-circle me-2"></i>
                        پذیرشی یافت نشد.
                    </div>
                `);
                return;
            }

            let html = `
                <div class="table-responsive">
                    <table class="table table-hover table-striped">
                        <thead class="table-dark">
                            <tr>
                                <th>شماره رسید</th>
                                <th>شماره پذیرش</th>
                                <th>شماره الکترونیکی</th>
                                <th>بیمار</th>
                                <th>کد ملی</th>
                                <th>پزشک</th>
                                <th>دپارتمان</th>
                                <th>تاریخ پذیرش</th>
                                <th>وضعیت</th>
                                <th>مبلغ کل</th>
                                <th>سهم بیمه پایه</th>
                                <th>سهم بیمه تکمیلی</th>
                                <th>سهم بیمار</th>
                                <th>پرداخت شده</th>
                                <th>باقی‌مانده</th>
                                <th>عملیات</th>
                            </tr>
                        </thead>
                        <tbody>
            `;

            items.forEach(function(item) {
                const hasDebt = item.RemainingAmount > 0;
                const statusBadge = getStatusBadgeClass(item.Status);
                
                // 🏥 MEDICAL: استخراج ReceptionNo و ElectronicReceptionNumber
                const receptionNo = item.ReceptionNo || item.ReceiptNo || '—';
                const electronicNumber = item.ElectronicReceptionNumber || '—';
                
                html += `
                    <tr data-reception-id="${item.ReceptionId}" class="${hasDebt ? 'table-warning' : ''}">
                        <td>${item.ReceiptNo || '—'}</td>
                        <td>
                            <span class="badge bg-primary" title="شماره پذیرش رسمی">${receptionNo}</span>
                        </td>
                        <td>
                            <small class="text-muted" title="شماره الکترونیکی - برای گروه‌بندی پذیرش‌های بیمار">${electronicNumber}</small>
                        </td>
                        <td>${item.PatientName || '—'}</td>
                        <td>${item.PatientNationalCode || '—'}</td>
                        <td>${item.DoctorName || '—'}</td>
                        <td>${item.DepartmentName || '—'}</td>
                        <td>${formatDateShamsi(item.ReceptionDateShamsi)}</td>
                        <td>
                            <span class="badge ${statusBadge}">${item.StatusText || getStatusText(item.Status)}</span>
                        </td>
                        <td><strong>${formatIRR(item.TotalAmount)}</strong></td>
                        <td class="text-primary">${formatIRR(item.BaseInsuranceShare || item.baseInsuranceShare || 0)}</td>
                        <td class="text-success">${formatIRR(item.SupplementaryInsuranceShare || item.supplementaryInsuranceShare || 0)}</td>
                        <td class="text-warning">${formatIRR(item.PatientShareAmount || item.patientShareAmount || 0)}</td>
                        <td>${formatIRR(item.PaidAmount)}</td>
                        <td>
                            ${hasDebt ? `<strong class="text-danger">${formatIRR(item.RemainingAmount)}</strong>` : formatIRR(item.RemainingAmount)}
                        </td>
                        <td>
                            <div class="btn-group btn-group-sm" role="group">
                                ${hasDebt ? `
                                    <button type="button" class="btn btn-warning btn-pay-debt" 
                                            data-reception-id="${item.ReceptionId}" 
                                            data-amount="${item.RemainingAmount}"
                                            title="پرداخت بدهی">
                                        <i class="fas fa-credit-card"></i>
                                    </button>
                                ` : ''}
                                ${(item.Status === 1 || item.StatusText === 'تکمیل شده' || item.StatusText === 'Completed' || item.PaidAmount > 0 || (item.TotalAmount > 0 && item.RemainingAmount === 0)) ? `
                                    <button type="button" class="btn btn-info btn-print-receipt" 
                                            data-reception-id="${item.ReceptionId}"
                                            title="چاپ قبض پذیرش">
                                        <i class="fas fa-receipt"></i>
                                    </button>
                                ` : ''}
                                ${item.SupplementaryPlanId && (item.Status === 1 || item.StatusText === 'تکمیل شده' || item.StatusText === 'Completed' || item.PaidAmount > 0 || (item.TotalAmount > 0 && item.RemainingAmount === 0)) ? `
                                    <button type="button" class="btn btn-success btn-print-insurance" 
                                            data-reception-id="${item.ReceptionId}"
                                            title="چاپ قبض بیمه تکمیلی">
                                        <i class="fas fa-file-invoice"></i>
                                    </button>
                                ` : ''}
                                ${item.Status === 0 || item.Status === 'Pending' ? `
                                    <button type="button" class="btn btn-primary btn-edit-reception" 
                                            data-reception-id="${item.ReceptionId}"
                                            title="ویرایش پذیرش">
                                        <i class="fas fa-edit"></i>
                                    </button>
                                ` : ''}
                                ${(item.Status === 0 || item.Status === 'Pending' || item.Status === 1 || item.Status === 'Completed') && item.Status !== 2 && item.Status !== 'Cancelled' ? `
                                    <button type="button" class="btn btn-danger btn-cancel-reception" 
                                            data-reception-id="${item.ReceptionId}"
                                            data-paid-amount="${item.PaidAmount || 0}"
                                            title="لغو پذیرش">
                                        <i class="fas fa-ban"></i>
                                    </button>
                                ` : ''}
                                <button type="button" class="btn btn-secondary btn-view-details" 
                                        data-reception-id="${item.ReceptionId}"
                                        title="مشاهده جزئیات">
                                    <i class="fas fa-eye"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                `;
            });

            html += `
                        </tbody>
                    </table>
                </div>
                <div class="mt-2 text-muted small">
                    <i class="fas fa-info-circle me-1"></i>
                    نمایش ${items.length} از ${totalCount} پذیرش
                </div>
            `;

            $container.html(html);

            // Attach event handlers
            attachEventHandlers();
        }

        /**
         * رندر صفحه‌بندی
         */
        function renderPagination(data) {
            const $pagination = $('#receptionListPagination ul');
            $pagination.empty();

            // پشتیبانی از هر دو حالت: TotalPages و totalPages
            const totalPages = data.TotalPages || data.totalPages || 0;
            const current = data.CurrentPage || data.currentPage || 1;
            
            if (!totalPages || totalPages <= 1) {
                return;
            }

            // Previous button
            $pagination.append(`
                <li class="page-item ${current === 1 ? 'disabled' : ''}">
                    <a class="page-link" href="#" data-page="${current - 1}">قبلی</a>
                </li>
            `);

            // Page numbers
            for (let i = 1; i <= totalPages; i++) {
                if (i === 1 || i === totalPages || (i >= current - 2 && i <= current + 2)) {
                    $pagination.append(`
                        <li class="page-item ${i === current ? 'active' : ''}">
                            <a class="page-link" href="#" data-page="${i}">${i}</a>
                        </li>
                    `);
                } else if (i === current - 3 || i === current + 3) {
                    $pagination.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
                }
            }

            // Next button
            $pagination.append(`
                <li class="page-item ${current === totalPages ? 'disabled' : ''}">
                    <a class="page-link" href="#" data-page="${current + 1}">بعدی</a>
                </li>
            `);
        }

        /**
         * Attach event handlers
         */
        function attachEventHandlers() {
            // پرداخت بدهی
            $('.btn-pay-debt').off('click').on('click', function() {
                const receptionId = $(this).data('reception-id');
                const amount = $(this).data('amount');
                handlePayDebt(receptionId, amount);
            });

            // چاپ قبض پذیرش
            $('.btn-print-receipt').off('click').on('click', function() {
                const receptionId = $(this).data('reception-id');
                handlePrintReceipt(receptionId);
            });

            // 🏥 MEDICAL: چاپ قبض بیمه تکمیلی
            $('.btn-print-insurance').off('click').on('click', function() {
                const receptionId = $(this).data('reception-id');
                handlePrintInsurance(receptionId);
            });

            // ویرایش پذیرش
            $('.btn-edit-reception').off('click').on('click', function() {
                const receptionId = $(this).data('reception-id');
                window.location.href = `/ReceptionV2/reception/edit/${receptionId}`;
            });

            // لغو پذیرش
            $('.btn-cancel-reception').off('click').on('click', function() {
                const receptionId = $(this).data('reception-id');
                const paidAmount = parseFloat($(this).data('paid-amount')) || 0;
                handleCancelReception(receptionId, paidAmount);
            });

            // مشاهده جزئیات
            $('.btn-view-details').off('click').on('click', function() {
                const receptionId = $(this).data('reception-id');
                handleViewDetails(receptionId);
            });
        }

        /**
         * پرداخت بدهی با POS
         */
        function handlePayDebt(receptionId, amount) {
            console.log('🏥 Reception List: Pay debt', { receptionId, amount });

            if (!amount || amount <= 0) {
                toastr.warning('مبلغ قابل پرداخت باید بیشتر از صفر باشد');
                return;
            }

            // باز کردن مودال پرداخت POS
            if (window.openPosPaymentModal && typeof window.openPosPaymentModal === 'function') {
                window.openPosPaymentModal(receptionId, amount);
            } else {
                // Fallback: استفاده از مودال موجود
                const $modal = $('#posPaymentModal');
                if ($modal.length) {
                    // Reset modal state
                    $('#posPaymentReady').removeClass('d-none');
                    $('#posPaymentLoading').addClass('d-none');
                    $('#posPaymentSuccess').addClass('d-none');
                    $('#posPaymentError').addClass('d-none');
                    $('#posPaymentStartBtn').removeClass('d-none');
                    $('#posPaymentConfirmBtn').addClass('d-none');
                    $('#posPaymentPrintBtn').addClass('d-none');
                    $('#posPaymentCancelBtn').removeClass('d-none');

                    // Set amount
                    $('#posPaymentAmount').text(formatIRR(amount));
                    $('#posPaymentReceptionId').text(receptionId);

                    // Store data
                    $modal.data('receptionId', receptionId);
                    $modal.data('amountIRR', amount);

                    // Show modal
                    const modal = new bootstrap.Modal($modal[0]);
                    modal.show();

                    // Attach start payment handler
                    $('#posPaymentStartBtn').off('click').on('click', function() {
                        processPosPayment(receptionId, amount);
                    });
                } else {
                    toastr.error('مودال پرداخت یافت نشد');
                }
            }
        }

        /**
         * پردازش پرداخت POS
         */
        function processPosPayment(receptionId, amount) {
            console.log('🏥 Reception List: Processing POS payment', { receptionId, amount });

            // Show loading
            $('#posPaymentReady').addClass('d-none');
            $('#posPaymentLoading').removeClass('d-none');
            $('#posPaymentStartBtn').addClass('d-none');
            $('#posPaymentCancelBtn').addClass('d-none');

            // Get default terminal
            $.get('/api/v1/pos/terminals/default')
                .done(function(response) {
                    // بررسی ساختار ServiceResult
                    const successValue = response?.Success ?? response?.success;
                    const isSuccess = successValue === true || successValue === "true" || successValue === 1;
                    
                    // استخراج Data از ServiceResult
                    let terminal = response?.Data || response?.data;
                    if (API && API.ok && typeof API.ok === 'function') {
                        terminal = API.ok(response);
                    }
                    
                    if (!isSuccess || !terminal) {
                        const errorMsg = response?.Message || response?.message || 'ترمینال POS پیش‌فرض یافت نشد';
                        throw new Error(errorMsg);
                    }
                    
                    // استفاده از posTerminalId یا Id
                    const terminalId = terminal.posTerminalId || terminal.PosTerminalId || terminal.Id || terminal.id;
                    if (!terminalId) {
                        throw new Error('شناسه ترمینال یافت نشد');
                    }

                    console.log('🏥 Reception List: Terminal found', { terminal, terminalId });

                    // Process payment
                    const token = getAntiForgeryToken();
                    return $.ajax({
                        url: config.paymentUrl || '/api/v1/pos/process-payment',
                        method: 'POST',
                        headers: {
                            'RequestVerificationToken': token,
                            'Content-Type': 'application/json; charset=utf-8'
                        },
                        data: JSON.stringify({
                            ReceptionId: receptionId,
                            AmountIRR: amount,
                            TerminalId: terminalId
                        }),
                        dataType: 'json'
                    });
                })
                .done(function(response) {
                    if (response && response.Success) {
                        // Show success
                        $('#posPaymentLoading').addClass('d-none');
                        $('#posPaymentSuccess').removeClass('d-none');
                        $('#posPaymentConfirmBtn').removeClass('d-none');
                        $('#posPaymentPrintBtn').removeClass('d-none');

                        // Store payment data
                        const $modal = $('#posPaymentModal');
                        $modal.data('posPaymentData', response.Data);

                        // Attach confirm handler
                        $('#posPaymentConfirmBtn').off('click').on('click', function() {
                            finalizeAfterPayment(receptionId, amount, response.Data);
                        });

                        // Attach print handler
                        $('#posPaymentPrintBtn').off('click').on('click', function() {
                            handlePrintReceipt(receptionId);
                        });

                        toastr.success('پرداخت با موفقیت انجام شد');
                    } else {
                        throw new Error(response?.Message || 'خطا در پردازش پرداخت');
                    }
                })
                .fail(function(xhr, status, error) {
                    console.error('❌ Reception List: POS payment failed', { xhr, status, error });
                    
                    // استخراج پیام خطا
                    let errorMessage = 'خطا در پردازش پرداخت';
                    try {
                        if (xhr.responseJSON) {
                            const jsonResponse = xhr.responseJSON;
                            errorMessage = jsonResponse.Message || jsonResponse.message || errorMessage;
                            
                            // اگر ServiceResult است، از Data استفاده کن
                            if (jsonResponse.Success === false && jsonResponse.Data) {
                                errorMessage = jsonResponse.Data.Message || errorMessage;
                            }
                        } else if (xhr.responseText) {
                            try {
                                const parsed = JSON.parse(xhr.responseText);
                                errorMessage = parsed.Message || parsed.message || errorMessage;
                            } catch (e) {
                                // Ignore parse errors
                            }
                        }
                    } catch (e) {
                        console.warn('⚠️ Reception List: Error parsing error response', e);
                    }
                    
                    // نمایش خطا
                    $('#posPaymentLoading').addClass('d-none');
                    $('#posPaymentError').removeClass('d-none');
                    $('#posPaymentErrorMsg').text(errorMessage);
                    $('#posPaymentCancelBtn').removeClass('d-none');
                    toastr.error(errorMessage);
                });
        }

        /**
         * نهایی‌سازی پس از پرداخت
         */
        function finalizeAfterPayment(receptionId, amount, posData) {
            console.log('🏥 Reception List: Finalizing after payment', { receptionId, amount, posData });

            const token = getAntiForgeryToken();
            const payload = {
                ReceptionId: receptionId,
                AmountIRR: amount,
                PaymentMethod: 'POS',
                PosTransactionId: posData?.TransactionId || posData?.ReferenceCode,
                PosReferenceCode: posData?.ReferenceCode,
                PosData: posData
            };

            $.ajax({
                url: config.finalizeUrl || '/api/v1/reception/finalize/pos',
                method: 'POST',
                headers: {
                    'RequestVerificationToken': token,
                    'Content-Type': 'application/json; charset=utf-8'
                },
                data: JSON.stringify(payload),
                dataType: 'json'
            })
            .done(function(response) {
                if (response && response.Success) {
                    // Close modal
                    const $modal = $('#posPaymentModal');
                    const modalInstance = bootstrap.Modal.getInstance($modal[0]);
                    if (modalInstance) {
                        modalInstance.hide();
                    }

                    toastr.success('پذیرش با موفقیت نهایی شد');

                    // Reload list
                    setTimeout(function() {
                        loadReceptionList(currentPage);
                    }, 1000);
                } else {
                    toastr.error(response?.Message || 'خطا در نهایی‌سازی پذیرش');
                }
            })
            .fail(function(xhr, status, error) {
                console.error('❌ Reception List: Finalize failed', error);
                toastr.error('خطا در نهایی‌سازی پذیرش');
            });
        }

        /**
         * چاپ قبض پذیرش
         */
        function handlePrintReceipt(receptionId) {
            const url = '/ReceptionV2/reception/print/' + receptionId;
            console.log('🏥 Reception List: Printing receipt for reception:', receptionId);
            window.open(url, '_blank');
        }

        /**
         * 🏥 MEDICAL: چاپ قبض بیمه تکمیلی
         */
        function handlePrintInsurance(receptionId) {
            const url = '/ReceptionV2/reception/print-insurance/' + receptionId;
            console.log('🏥 Reception List: Printing insurance receipt for reception:', receptionId);
            window.open(url, '_blank');
        }

        /**
         * مشاهده جزئیات
         */
        function handleViewDetails(receptionId) {
            // TODO: Implement view details
            toastr.info('مشاهده جزئیات - در حال توسعه');
        }

        /**
         * لغو پذیرش
         */
        function handleCancelReception(receptionId, paidAmount) {
            console.log('🚫 Reception List: Cancel reception', { receptionId, paidAmount });

            // نمایش مودال لغو
            showCancelModal(receptionId, paidAmount);
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

            console.log('🚫 Reception List: Sending cancel request:', request);

            $.ajax({
                url: `${baseUrl}/cancel`,
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                contentType: 'application/json',
                data: JSON.stringify(request),
                success: function(response) {
                    console.log('🚫 Reception List: Cancel response:', response);

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

                    // رفرش لیست
                    setTimeout(function() {
                        loadReceptionList(currentPage);
                    }, 1000);
                },
                error: function(xhr, status, error) {
                    console.error('❌ Reception List: Error canceling reception:', error);
                    toastr.error('خطا در لغو پذیرش', 'خطا');
                    $('#btnConfirmCancel').prop('disabled', false).html('<i class="fas fa-ban me-1"></i>لغو پذیرش');
                }
            });
        }

        // Event handlers - بهینه‌سازی شده برای محیط درمانی
        $('#btnSearch').on('click', function(e) {
            e.preventDefault();
            if (!isLoading) {
                loadReceptionList(1);
            }
        });

        // 🏥 MEDICAL: Reset filters - شامل فیلترهای جدید
        $('#btnReset').on('click', function(e) {
            e.preventDefault();
            $('#filterNationalCode').val('');
            $('#filterPatientName').val('');
            $('#filterDateFrom').val('');
            $('#filterDateTo').val('');
            $('#filterStatus').val('');
            $('#filterReceptionNo').val(''); // 🏥 MEDICAL: پاک کردن فیلتر شماره پذیرش
            $('#filterElectronicReceptionNumber').val(''); // 🏥 MEDICAL: پاک کردن فیلتر شماره الکترونیکی
            if (!isLoading) {
                loadReceptionList(1);
            }
        });

        // Pagination
        $(document).on('click', '#receptionListPagination .page-link', function(e) {
            e.preventDefault();
            const page = $(this).data('page');
            if (page && page !== currentPage && !isLoading) {
                loadReceptionList(page);
            }
        });

        // Enter key in filters
        $('#receptionListFilters input').on('keypress', function(e) {
            if (e.which === 13 && !isLoading) {
                e.preventDefault();
                loadReceptionList(1);
            }
        });

        // Initial load با تاخیر کوتاه برای اطمینان از لود شدن کامل
        setTimeout(function() {
            console.log('🏥 Reception List: Starting initial load...');
            loadReceptionList(1);
        }, 100);

        console.log('✅ Reception List: Module initialized');
    });

})(window.ReceptionAPI || window.API || {});

