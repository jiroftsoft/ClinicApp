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

        // 🏥 MEDICAL: پاکسازی Draft‌های Pending کاربر فعلی هنگام بارگذاری صفحه
        // این مهم است چون ممکن است کاربر Draft ایجاد کرده و بدون کلیک روی "ذخیره و پذیرش" به این صفحه آمده باشد
        function cleanupPendingDrafts() {
            try {
                console.log('🏥 Reception List: شروع پاکسازی Draft‌های Pending...');
                
                // استفاده از API برای حذف Draft‌های Pending کاربر فعلی
                // ✅ API.post خودش baseUrl (/api/v1/reception) را اضافه می‌کند، پس فقط path نسبی می‌دهیم
                const cleanupPath = '/draft/cleanup-pending';
                const cleanupUrlFull = '/api/v1/reception/draft/cleanup-pending'; // برای fallback
                
                // استفاده از AJAX برای حذف Draft‌های Pending
                if (API && API.post) {
                    API.post(cleanupPath, {})
                        .then(function(response) {
                            if (response && response.Success) {
                                const count = response.Data || 0;
                                if (count > 0) {
                                    console.log('✅ Reception List: ' + count + ' Draft Pending حذف شد');
                                } else {
                                    console.log('ℹ️ Reception List: هیچ Draft Pending یافت نشد');
                                }
                            }
                        })
                        .catch(function(err) {
                            // خطا را فقط در حالت development نمایش بده (نه در production)
                            if (err && err.status !== 404) {
                                console.warn('⚠️ Reception List: خطا در پاکسازی Draft‌های Pending:', err);
                            }
                        });
                } else {
                    // Fallback: استفاده از jQuery AJAX مستقیم
                    $.ajax({
                        url: cleanupUrlFull,
                        type: 'POST',
                        headers: {
                            'RequestVerificationToken': getAntiForgeryToken()
                        },
                        success: function(response) {
                            if (response && response.Success) {
                                const count = response.Data || 0;
                                if (count > 0) {
                                    console.log('✅ Reception List: ' + count + ' Draft Pending حذف شد');
                                }
                            }
                        },
                        error: function(err) {
                            // خطا را فقط در حالت development نمایش بده (نه در production)
                            if (err && err.status !== 404) {
                                console.warn('⚠️ Reception List: خطا در پاکسازی Draft‌های Pending:', err);
                            }
                        }
                    });
                }
            } catch (err) {
                console.warn('⚠️ Reception List: خطا در cleanupPendingDrafts:', err);
            }
        }

        // اجرای cleanup هنگام بارگذاری صفحه
        cleanupPendingDrafts();

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
                    <table class="table table-hover table-striped table-bordered reception-list-table">
                        <thead class="bg-primary text-white" style="background-color: #2c5aa0 !important;">
                            <tr>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">شماره رسید</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">شماره پذیرش</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">شماره الکترونیکی</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">بیمار</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">کد ملی</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">پزشک</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">دپارتمان</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">تاریخ پذیرش</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">وضعیت</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">مبلغ کل</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">سهم بیمه پایه</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">سهم بیمه تکمیلی</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">سهم بیمار</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">پرداخت شده</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">باقی‌مانده</th>
                                <th class="text-center align-middle" style="font-weight: 600; font-size: 14px; padding: 12px 8px;">عملیات</th>
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

        // ============================================
        // POS Payment Client & UI Instances (Global)
        // ============================================
        var posPaymentClient = null;
        var posPaymentUI = null;

        /**
         * Initialize POS Payment Modules
         */
        function initializePosPaymentModules() {
            try {
                // تنظیمات SignalR URL - از global variable استفاده می‌کنیم که در view تنظیم می‌شود
                var signalRUrl = window.SamanKishSignalRUrl || 'http://localhost:5000/signalr';
                
                // Initialize PosPaymentClient
                if (typeof PosPaymentClient !== 'undefined') {
                    posPaymentClient = new PosPaymentClient({
                        signalRUrl: signalRUrl,
                        
                        onConnecting: function() {
                            console.log('🏥 Reception List: POS Payment - Connecting...');
                            if (posPaymentUI) {
                                posPaymentUI.showLoading('در حال اتصال...', 'در حال اتصال به دستگاه کارتخوان', 'لطفاً صبر کنید');
                            }
                        },
                        
                        onConnected: function() {
                            console.log('✅ Reception List: POS Payment - Connected to SignalR Hub');
                        },
                        
                        onCardSwiped: function(data) {
                            console.log('🔔 Reception List: POS Payment - Card swiped:', data);
                            if (posPaymentUI) {
                                posPaymentUI.showLoading('کارت کشیده شد', 'لطفاً رمز کارت را وارد کنید', '');
                            }
                        },
                        
                        onSuccess: function(response) {
                            console.log('✅ Reception List: POS Payment - Success:', response);
                            
                            // نمایش موفقیت در Modal
                            if (posPaymentUI) {
                                posPaymentUI.showSuccess({
                                    rrn: response.rrn,
                                    traceNo: response.traceNo,
                                    terminalId: response.terminalId,
                                    cardLast4: response.cardLast4,
                                    amount: window.currentPaymentAmount,
                                    txnDate: new Date().toLocaleDateString('fa-IR')
                                });
                            }
                            
                            // ذخیره اطلاعات برای Finalize
                            window.posPaymentData = {
                                rrn: response.rrn,
                                traceNo: response.traceNo,
                                terminalId: response.terminalId,
                                cardLast4: response.cardLast4
                            };
                        },
                        
                        onCancel: function(response) {
                            console.log('⚠️ Reception List: POS Payment - Canceled:', response);
                            if (posPaymentUI) {
                                posPaymentUI.showCanceled();
                            }
                            // Reset payment data
                            window.posPaymentData = null;
                        },
                        
                        onError: function(error) {
                            console.error('❌ Reception List: POS Payment - Error:', error);
                            if (posPaymentUI) {
                                posPaymentUI.showError(error.message || 'خطا در پرداخت', error.code);
                            }
                        }
                    });
                    
                    console.log('✅ Reception List: PosPaymentClient initialized');
                } else {
                    console.warn('⚠️ Reception List: PosPaymentClient not found - make sure pos-payment-client.js is loaded');
                }
                
                // Initialize PosPaymentUI
                if (typeof PosPaymentUI !== 'undefined') {
                    posPaymentUI = new PosPaymentUI({
                        modalId: 'posPaymentModal',
                        onStart: function() {
                            // این callback در openPosPaymentModal تنظیم می‌شود
                            console.log('🏥 Reception List: POS Payment Start button clicked');
                        },
                        onConfirm: function() {
                            const receptionId = window.currentPaymentReceptionId;
                            const amount = window.currentPaymentAmount;
                            const posData = window.posPaymentData;
                            
                            if (receptionId && amount && posData) {
                                finalizeAfterPayment(receptionId, amount, posData);
                            } else {
                                toastr.error('اطلاعات پرداخت ناقص است');
                            }
                        },
                        onPrint: function() {
                            const receptionId = window.currentPaymentReceptionId;
                            if (receptionId) {
                                handlePrintReceipt(receptionId);
                            }
                        },
                        onCancel: function() {
                            // Reset payment data
                            window.posPaymentData = null;
                            window.currentPaymentReceptionId = null;
                            window.currentPaymentAmount = null;
                        },
                        onRetry: function() {
                            const receptionId = window.currentPaymentReceptionId;
                            const amount = window.currentPaymentAmount;
                            if (receptionId && amount) {
                                openPosPaymentModal(receptionId, amount);
                            }
                        }
                    });
                    
                    console.log('✅ Reception List: PosPaymentUI initialized');
                } else {
                    console.warn('⚠️ Reception List: PosPaymentUI not found - make sure pos-payment-ui.js is loaded');
                }
            } catch (err) {
                console.error('❌ Reception List: Error initializing POS Payment modules:', err);
            }
        }

        /**
         * باز کردن مودال پرداخت POS
         */
        function openPosPaymentModal(receptionId, amount) {
            console.log('🏥 Reception List: Opening POS Payment Modal', { receptionId, amount });

            if (!amount || amount <= 0) {
                toastr.warning('مبلغ قابل پرداخت باید بیشتر از صفر باشد');
                return;
            }

            // ذخیره اطلاعات برای استفاده در callbacks
            window.currentPaymentReceptionId = receptionId;
            window.currentPaymentAmount = amount;
            window.posPaymentData = null;

            // دریافت ترمینال پیش‌فرض
            const API = window.ReceptionAPI || window.API || {};
            const terminalPromise = API.get ? API.get('/pos/terminals/default') : $.get('/api/v1/pos/terminals/default');
            
            terminalPromise
                .then(function(response) {
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
                    
                    const terminalName = terminal.title || terminal.Title || terminal.name || terminal.Name || terminal.terminalName || terminal.TerminalName || 'دستگاه کارتخوان';
                    const terminalId = terminal.terminalId || terminal.TerminalId;
                    const ipAddress = terminal.ipAddress || terminal.IpAddress;
                    
                    // ✅ Debug: Log terminal info
                    console.log('🏥 Reception List: Terminal info extracted:', {
                        terminal: terminal,
                        terminalId: terminalId,
                        terminalName: terminalName,
                        ipAddress: ipAddress,
                        amount: amount
                    });
                    
                    // ✅ Validation: بررسی اینکه terminalId و ipAddress موجود هستند
                    if (!terminalId) {
                        console.error('❌ Reception List: terminalId is missing', terminal);
                        toastr.error('شناسه ترمینال یافت نشد. لطفاً ترمینال را تنظیم کنید.', 'خطا');
                        return;
                    }
                    
                    if (!ipAddress) {
                        console.error('❌ Reception List: ipAddress is missing', terminal);
                        toastr.error('آدرس IP ترمینال یافت نشد. لطفاً ترمینال را تنظیم کنید.', 'خطا');
                        return;
                    }
                    
                    // نمایش Modal
                    if (posPaymentUI) {
                        posPaymentUI.setPaymentInfo(amount, terminalName);
                        posPaymentUI.open();
                        
                        // ✅ تنظیم callback برای دکمه "پرداخت با POS"
                        // این callback در PosPaymentUI.onStart مدیریت می‌شود
                        // اما ما باید processPayment را فراخوانی کنیم
                        $('#posPaymentStartBtn').off('click').on('click', function() {
                            console.log('🏥 Reception List: POS Payment Start button clicked');
                            console.log('🏥 Reception List: Calling processPayment with:', {
                                terminalId: terminalId,
                                amount: amount,
                                ipAddress: ipAddress
                            });
                            
                            // نمایش Loading
                            if (posPaymentUI) {
                                posPaymentUI.showLoading('در حال ارسال مبلغ...', 'در حال ارسال مبلغ به دستگاه POS', 'لطفاً کارت را وارد کنید');
                            }
                            
                            // شروع پرداخت با PosPaymentClient
                            if (posPaymentClient && terminalId && ipAddress) {
                                // ✅ اطمینان از اینکه terminalId string است (نه number)
                                const terminalIdStr = String(terminalId);
                                posPaymentClient.processPayment(terminalIdStr, amount, ipAddress);
                            } else {
                                console.error('❌ Reception List: Missing terminal info:', { 
                                    posPaymentClient: !!posPaymentClient,
                                    terminalId: terminalId, 
                                    ipAddress: ipAddress 
                                });
                                if (posPaymentUI) {
                                    posPaymentUI.showError('اطلاعات ترمینال ناقص است', 'INVALID_TERMINAL');
                                }
                            }
                        });
                    } else {
                        // Fallback: استفاده از Modal قدیمی
                        const $modal = $('#posPaymentModal');
                        if ($modal.length) {
                            $('#posAmount').text(formatIRR(amount));
                            $('#posTerminalName').text(terminalName);
                            const modal = new bootstrap.Modal($modal[0]);
                            modal.show();
                            
                            // Fallback: استفاده از روش قدیمی
                            $('#posPaymentStartBtn').off('click').on('click', function() {
                                processPosPayment(receptionId, amount);
                            });
                        } else {
                            toastr.error('مودال پرداخت یافت نشد');
                            return;
                        }
                    }
                })
                .catch(function(error) {
                    console.error('❌ Reception List: Error getting terminal:', error);
                    toastr.error(error.message || 'خطا در دریافت اطلاعات ترمینال');
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

            // استفاده از تابع openPosPaymentModal
            openPosPaymentModal(receptionId, amount);
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
         * مشاهده جزئیات پذیرش - پیاده‌سازی حرفه‌ای و کاربردی
         */
        function handleViewDetails(receptionId) {
            console.log('🔍 Reception List: نمایش جزئیات پذیرش - ReceptionId:', receptionId);

            if (!receptionId || receptionId <= 0) {
                toastr.error('شناسه پذیرش نامعتبر است', 'خطا');
                return;
            }

            // نمایش Modal و Reset کردن محتوا
            const $modal = $('#receptionDetailsModal');
            const $loading = $('#receptionDetailsLoading');
            const $content = $('#receptionDetailsContent');

            $loading.show();
            $content.hide();
            $modal.modal('show');

            // بارگذاری اطلاعات
            loadReceptionDetails(receptionId);
        }

        /**
         * بارگذاری جزئیات پذیرش از API
         */
        function loadReceptionDetails(receptionId) {
            const API = window.ReceptionAPI || window.API || {};
            const $loading = $('#receptionDetailsLoading');
            const $content = $('#receptionDetailsContent');

            API.get(`/details/${receptionId}`)
                .then(function(response) {
                    console.log('✅ Reception List: جزئیات دریافت شد', response);

                    // بررسی موفقیت
                    const successValue = response?.Success ?? response?.success;
                    const isSuccess = successValue === true || successValue === "true" || successValue === 1;

                    if (!response || !isSuccess) {
                        const errorMsg = response?.Message || response?.message || 'خطا در دریافت جزئیات پذیرش';
                        toastr.error(errorMsg, 'خطا');
                        $loading.hide();
                        return;
                    }

                    // Extract data
                    let data = response.Data || response.data;
                    if (API && API.ok && typeof API.ok === 'function') {
                        data = API.ok(response);
                    }

                    // نمایش اطلاعات
                    displayReceptionDetails(data);
                    $loading.hide();
                    $content.show();
                })
                .fail(function(err) {
                    console.error('❌ Reception List: خطا در دریافت جزئیات', err);
                    toastr.error('خطا در دریافت جزئیات پذیرش', 'خطا');
                    $loading.hide();
                });
        }

        /**
         * نمایش جزئیات پذیرش در Modal
         */
        function displayReceptionDetails(data) {
            if (!data) {
                toastr.error('اطلاعات پذیرش یافت نشد', 'خطا');
                return;
            }

            console.log('📋 Reception List: نمایش جزئیات', data);

            // Helper: فرمت مبلغ
            const formatIRR = (amount) => {
                if (amount == null || amount === undefined) return '0';
                return new Intl.NumberFormat('fa-IR').format(Math.round(amount)) + ' ریال';
            };

            // Helper: فرمت تاریخ
            const formatDate = (date) => {
                if (!date) return '-';
                return date;
            };

            // Helper: Badge وضعیت
            const getStatusBadge = (status, statusText) => {
                const badges = {
                    'Pending': 'badge bg-warning',
                    'Completed': 'badge bg-success',
                    'Cancelled': 'badge bg-danger'
                };
                const badgeClass = badges[status] || 'badge bg-secondary';
                return `<span class="${badgeClass}">${statusText || status}</span>`;
            };

            // Helper: Badge روش پرداخت
            const getMethodBadge = (method, methodText) => {
                const badges = {
                    'Cash': 'badge bg-success',
                    'POS': 'badge bg-primary',
                    'Online': 'badge bg-info',
                    'Debt': 'badge bg-warning'
                };
                const badgeClass = badges[method] || 'badge bg-secondary';
                return `<span class="${badgeClass}">${methodText || method}</span>`;
            };

            // Helper: Badge وضعیت پرداخت
            const getPaymentStatusBadge = (status, statusText) => {
                const badges = {
                    'Pending': 'badge bg-warning',
                    'Success': 'badge bg-success',
                    'Failed': 'badge bg-danger',
                    'Canceled': 'badge bg-secondary'
                };
                const badgeClass = badges[status] || 'badge bg-secondary';
                return `<span class="${badgeClass}">${statusText || status}</span>`;
            };

            // 1. اطلاعات اصلی پذیرش
            $('#detailsReceptionNo').text(data.ReceptionNo || '-');
            $('#detailsElectronicNumber').text(data.ElectronicReceptionNumber || '-');
            $('#detailsReceptionDate').text(formatDate(data.ReceptionDateShamsi));
            $('#detailsStatus').html(getStatusBadge(data.Status, data.StatusText));
            $('#detailsType').text(data.TypeText || data.Type || '-');
            $('#detailsPriority').text(data.PriorityText || data.Priority || '-');

            // 2. اطلاعات بیمار
            $('#detailsPatientName').text(data.PatientFullName || '-');
            $('#detailsPatientNationalCode').text(data.PatientNationalCode || '-');
            $('#detailsPatientPhone').text(data.PatientPhoneNumber || '-');
            $('#detailsPatientGender').text(data.PatientGender || '-');
            $('#detailsPatientBirthDate').text(formatDate(data.PatientBirthDateShamsi));

            // 3. اطلاعات پزشک و دپارتمان
            $('#detailsDoctorName').text(data.DoctorFullName || '-');
            $('#detailsDoctorSpecialization').text(data.DoctorSpecialization || '-');
            $('#detailsDoctorDegree').text(data.DoctorDegree || '-');
            $('#detailsDepartmentName').text(data.DepartmentName || '-');
            $('#detailsClinicName').text(data.ClinicName || '-');

            // 4. تب خدمات (با نمایش محاسبات بیمه پایه و تکمیلی)
            const $servicesList = $('#detailsServicesList');
            if (data.Items && data.Items.length > 0) {
                $('#servicesCount').text(data.Items.length);
                let servicesHtml = '';
                data.Items.forEach(item => {
                    // ✅ استخراج سهم بیمه پایه و تکمیلی از DTO
                    const primaryPays = item.PrimaryPays || 0;
                    const supplementaryPays = item.SupplementaryPays || 0;
                    const totalInsurerShare = item.InsurerShareAmount || 0;
                    
                    servicesHtml += `
                        <tr>
                            <td>${item.ServiceCode || '-'}</td>
                            <td>${item.ServiceName || '-'}</td>
                            <td>${item.Quantity || 0}</td>
                            <td>${formatIRR(item.UnitPrice)}</td>
                            <td>${formatIRR(item.TotalPrice || (item.UnitPrice * item.Quantity))}</td>
                            <td class="text-primary"><strong>${formatIRR(item.PatientShareAmount)}</strong></td>
                            <td class="text-info">${formatIRR(primaryPays)}</td>
                            <td class="text-info">${formatIRR(supplementaryPays)}</td>
                            <td class="text-info"><strong>${formatIRR(totalInsurerShare)}</strong></td>
                        </tr>
                    `;
                });
                $servicesList.html(servicesHtml);
            } else {
                $('#servicesCount').text('0');
                $servicesList.html('<tr><td colspan="9" class="text-center text-muted">هیچ خدمتی ثبت نشده است</td></tr>');
            }

            // 5. تب اطلاعات مالی
            $('#detailsTotalAmount').text(formatIRR(data.TotalAmount));
            $('#detailsGross').text(formatIRR(data.Gross));
            $('#detailsPatientShare').text(formatIRR(data.PatientCoPay));
            $('#detailsBasePay').text(formatIRR(data.BasePay));
            $('#detailsSuppPay').text(formatIRR(data.SuppPay));
            $('#detailsInsurerShare').text(formatIRR(data.InsurerShareAmount));
            $('#detailsPaidAmount').text(formatIRR(data.PaidAmount));
            $('#detailsRemainingAmount').text(formatIRR(data.RemainingAmount));

            // Progress Bar
            const totalAmount = data.PatientCoPay || 1;
            const paidAmount = data.PaidAmount || 0;
            const progressPercent = Math.min(100, Math.round((paidAmount / totalAmount) * 100));
            const $progressBar = $('#detailsPaymentProgress');
            $progressBar.css('width', `${progressPercent}%`).text(`${progressPercent}%`);

            if (progressPercent >= 100) {
                $progressBar.removeClass('bg-warning bg-danger').addClass('bg-success');
            } else if (progressPercent >= 50) {
                $progressBar.removeClass('bg-success bg-danger').addClass('bg-warning');
            } else {
                $progressBar.removeClass('bg-success bg-warning').addClass('bg-danger');
            }

            // وضعیت بدهی
            const $debtStatus = $('#detailsDebtStatus');
            if (data.HasDebt && data.RemainingAmount > 0) {
                $debtStatus.html(`
                    <div class="alert alert-warning mb-0">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        <strong>بدهی:</strong> مبلغ <strong>${formatIRR(data.RemainingAmount)}</strong> باقی‌مانده است.
                    </div>
                `);
            } else {
                $debtStatus.html(`
                    <div class="alert alert-success mb-0">
                        <i class="fas fa-check-circle me-2"></i>
                        <strong>پرداخت کامل:</strong> تمام مبلغ پرداخت شده است.
                    </div>
                `);
            }

            // 6. تب تراکنش‌های پرداخت
            const $paymentsList = $('#detailsPaymentsList');
            if (data.Transactions && data.Transactions.length > 0) {
                $('#paymentsCount').text(data.Transactions.length);
                let paymentsHtml = '';
                data.Transactions.forEach(transaction => {
                    const amountClass = transaction.Amount < 0 ? 'text-danger' : 'text-success';
                    const amountSign = transaction.Amount < 0 ? '-' : '+';
                    paymentsHtml += `
                        <tr>
                            <td>${formatDate(transaction.CreatedAtShamsi)}</td>
                            <td class="${amountClass}">${amountSign}${formatIRR(Math.abs(transaction.Amount))}</td>
                            <td>${getMethodBadge(transaction.Method, transaction.MethodText)}</td>
                            <td>${getPaymentStatusBadge(transaction.Status, transaction.StatusText)}</td>
                            <td>${transaction.TransactionId || '-'}</td>
                            <td>${transaction.ReferenceCode || '-'}</td>
                            <td>${transaction.Description || '-'}</td>
                        </tr>
                    `;
                });
                $paymentsList.html(paymentsHtml);
            } else {
                $('#paymentsCount').text('0');
                $paymentsList.html('<tr><td colspan="7" class="text-center text-muted">هیچ تراکنش پرداختی ثبت نشده است</td></tr>');
            }

            // 7. تب بیمه (با محاسبات دقیق از آیتم‌ها)
            // ✅ محاسبه مجموع سهم بیمه پایه و تکمیلی از آیتم‌ها
            let totalPrimaryPays = 0;
            let totalSupplementaryPays = 0;
            if (data.Items && data.Items.length > 0) {
                data.Items.forEach(item => {
                    totalPrimaryPays += (item.PrimaryPays || 0);
                    totalSupplementaryPays += (item.SupplementaryPays || 0);
                });
            }

            const $baseInsurance = $('#detailsBaseInsurance');
            if (data.BasePlanId && data.BasePlanName) {
                $baseInsurance.html(`
                    <div class="row g-2">
                        <div class="col-6"><strong>نام بیمه:</strong></div>
                        <div class="col-6"><strong>${data.BasePlanName}</strong></div>
                        <div class="col-6"><strong>شناسه:</strong></div>
                        <div class="col-6">${data.BasePlanId}</div>
                        <div class="col-12"><hr class="my-2"></div>
                        <div class="col-6"><strong>سهم بیمه (از Reception):</strong></div>
                        <div class="col-6 text-info">${formatIRR(data.BasePay)}</div>
                        <div class="col-6"><strong>سهم بیمه (از آیتم‌ها):</strong></div>
                        <div class="col-6 text-info">${formatIRR(totalPrimaryPays)}</div>
                        <div class="col-12">
                            <div class="alert alert-info mb-0 mt-2">
                                <i class="fas fa-info-circle me-2"></i>
                                <small>محاسبه شده از ${data.Items?.length || 0} خدمت</small>
                            </div>
                        </div>
                    </div>
                `);
            } else {
                $baseInsurance.html('<p class="text-muted mb-0">بیمه پایه ثبت نشده است</p>');
            }

            const $suppInsurance = $('#detailsSupplementaryInsurance');
            if (data.SupplementaryPlanId && data.SupplementaryPlanName) {
                $suppInsurance.html(`
                    <div class="row g-2">
                        <div class="col-6"><strong>نام بیمه:</strong></div>
                        <div class="col-6"><strong>${data.SupplementaryPlanName}</strong></div>
                        <div class="col-6"><strong>شناسه:</strong></div>
                        <div class="col-6">${data.SupplementaryPlanId}</div>
                        <div class="col-12"><hr class="my-2"></div>
                        <div class="col-6"><strong>سهم بیمه (از Reception):</strong></div>
                        <div class="col-6 text-info">${formatIRR(data.SuppPay)}</div>
                        <div class="col-6"><strong>سهم بیمه (از آیتم‌ها):</strong></div>
                        <div class="col-6 text-info">${formatIRR(totalSupplementaryPays)}</div>
                        <div class="col-12">
                            <div class="alert alert-info mb-0 mt-2">
                                <i class="fas fa-info-circle me-2"></i>
                                <small>محاسبه شده از ${data.Items?.length || 0} خدمت</small>
                            </div>
                        </div>
                    </div>
                `);
            } else {
                $suppInsurance.html('<p class="text-muted mb-0">بیمه تکمیلی ثبت نشده است</p>');
            }

            // 8. تب تاریخچه
            $('#detailsCreatedAt').text(formatDate(data.CreatedAtShamsi));
            $('#detailsCreatedBy').text(data.CreatedBy || '-');
            $('#detailsUpdatedAt').text(formatDate(data.UpdatedAtShamsi) || '-');
            $('#detailsUpdatedBy').text(data.UpdatedBy || '-');
            $('#detailsNotes').text(data.Notes || 'یادداشتی ثبت نشده است');

            // 9. Event Handler برای دکمه چاپ
            $('#btnPrintReceptionDetails').off('click').on('click', function() {
                window.open(`/ReceptionV2/Print/${data.ReceptionId}`, '_blank');
            });
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

            const request = {
                ReceptionId: receptionId,
                Reason: reason,
                ProcessRefund: processRefund,
                RefundReason: processRefund ? reason : null
            };

            console.log('🚫 Reception List: Sending cancel request:', request);

            API.post('/cancel', request)
                .then(function(fullResponse) {
                    console.log('🚫 Reception List: Cancel response:', fullResponse);

                    // 🔍 چک Success - منطق دقیق‌تر برای انواع مختلف Success
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

                    // رفرش لیست
                    setTimeout(function() {
                        loadReceptionList(currentPage);
                    }, 1000);
                })
                .fail(function(jqXHR, textStatus, errorThrown) {
                    console.error('❌ Reception List: Error canceling reception:', {
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

        // Initialize POS Payment Modules
        initializePosPaymentModules();

        // Initial load با تاخیر کوتاه برای اطمینان از لود شدن کامل
        setTimeout(function() {
            console.log('🏥 Reception List: Starting initial load...');
            loadReceptionList(1);
        }, 100);

        console.log('✅ Reception List: Module initialized');
    });

})(window.ReceptionAPI || window.API || {});

