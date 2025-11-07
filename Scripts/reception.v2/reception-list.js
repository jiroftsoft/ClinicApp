/**
 * Reception List Module - لیست پذیرش‌ها (مراجعات قبلی)
 * 
 * ویژگی‌ها:
 * - نمایش لیست پذیرش‌ها با فیلتر و صفحه‌بندی
 * - امکان پرداخت مجدد با POS
 * - چاپ قبض پرداخت و بیمه تکمیلی
 * - مدیریت بدهی‌ها
 */
(function(API) {
    'use strict';

    // ✅ اطمینان از لود شدن DOM
    $(document).ready(function() {
        console.log('🏥 Reception List: Initializing module...');

        const config = window.ReceptionListConfig || {};
        let currentPage = 1;
        const pageSize = 20;
        let currentFilters = {};

        /**
         * دریافت توکن Anti-Forgery
         */
        function getAntiForgeryToken() {
            const token = $('input[name="__RequestVerificationToken"]').val();
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
         * بارگذاری لیست پذیرش‌ها
         */
        function loadReceptionList(page = 1) {
            currentPage = page;
            
            const filters = {
                NationalCode: $('#filterNationalCode').val() || null,
                PatientName: $('#filterPatientName').val() || null,
                DateFrom: $('#filterDateFrom').val() || null,
                DateTo: $('#filterDateTo').val() || null,
                Status: $('#filterStatus').val() ? parseInt($('#filterStatus').val()) : null
            };

            currentFilters = filters;

            const $container = $('#receptionListContainer');
            $container.html(`
                <div class="text-center py-5">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">در حال بارگذاری...</span>
                    </div>
                    <p class="mt-2 text-muted">در حال بارگذاری لیست پذیرش‌ها...</p>
                </div>
            `);

            const token = getAntiForgeryToken();
            const payload = {
                filters: filters,
                page: page,
                pageSize: pageSize
            };

            $.ajax({
                url: config.apiUrl || '/Reception/ReceptionList/GetReceptionList',
                method: 'POST',
                headers: {
                    'RequestVerificationToken': token,
                    'Content-Type': 'application/json; charset=utf-8'
                },
                data: JSON.stringify(payload),
                dataType: 'json'
            })
            .done(function(response) {
                if (response && response.Success && response.Data) {
                    renderReceptionList(response.Data);
                    renderPagination(response.Data);
                } else {
                    $container.html(`
                        <div class="alert alert-warning">
                            <i class="fas fa-exclamation-triangle me-2"></i>
                            ${response?.Message || 'خطا در دریافت لیست پذیرش‌ها'}
                        </div>
                    `);
                }
            })
            .fail(function(xhr, status, error) {
                console.error('❌ Reception List: Load failed', error);
                $container.html(`
                    <div class="alert alert-danger">
                        <i class="fas fa-times-circle me-2"></i>
                        خطا در ارتباط با سرور. لطفاً دوباره تلاش کنید.
                    </div>
                `);
            });
        }

        /**
         * رندر لیست پذیرش‌ها
         */
        function renderReceptionList(data) {
            const $container = $('#receptionListContainer');
            
            if (!data.Items || data.Items.length === 0) {
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
                                <th>بیمار</th>
                                <th>کد ملی</th>
                                <th>پزشک</th>
                                <th>دپارتمان</th>
                                <th>تاریخ پذیرش</th>
                                <th>وضعیت</th>
                                <th>مبلغ کل</th>
                                <th>پرداخت شده</th>
                                <th>باقی‌مانده</th>
                                <th>عملیات</th>
                            </tr>
                        </thead>
                        <tbody>
            `;

            data.Items.forEach(function(item) {
                const hasDebt = item.RemainingAmount > 0;
                const statusBadge = getStatusBadgeClass(item.Status);
                
                html += `
                    <tr data-reception-id="${item.ReceptionId}" class="${hasDebt ? 'table-warning' : ''}">
                        <td>${item.ReceiptNo || '—'}</td>
                        <td>${item.PatientName || '—'}</td>
                        <td>${item.PatientNationalCode || '—'}</td>
                        <td>${item.DoctorName || '—'}</td>
                        <td>${item.DepartmentName || '—'}</td>
                        <td>${formatDateShamsi(item.ReceptionDateShamsi)}</td>
                        <td>
                            <span class="badge ${statusBadge}">${item.StatusText || getStatusText(item.Status)}</span>
                        </td>
                        <td>${formatIRR(item.TotalAmount)}</td>
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
                                ${item.PaidAmount > 0 ? `
                                    <button type="button" class="btn btn-info btn-print-receipt" 
                                            data-reception-id="${item.ReceptionId}"
                                            title="چاپ قبض پرداخت">
                                        <i class="fas fa-print"></i>
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
                    نمایش ${data.Items.length} از ${data.TotalCount} پذیرش
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

            if (!data.TotalPages || data.TotalPages <= 1) {
                return;
            }

            const totalPages = data.TotalPages;
            const current = data.CurrentPage;

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

            // چاپ قبض
            $('.btn-print-receipt').off('click').on('click', function() {
                const receptionId = $(this).data('reception-id');
                handlePrintReceipt(receptionId);
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
                .done(function(terminal) {
                    if (!terminal || !terminal.Id) {
                        throw new Error('ترمینال POS پیش‌فرض یافت نشد');
                    }

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
                            TerminalId: terminal.Id
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
                    console.error('❌ Reception List: POS payment failed', error);
                    $('#posPaymentLoading').addClass('d-none');
                    $('#posPaymentError').removeClass('d-none');
                    $('#posPaymentErrorMsg').text(xhr.responseJSON?.Message || 'خطا در پردازش پرداخت');
                    $('#posPaymentCancelBtn').removeClass('d-none');
                    toastr.error('خطا در پردازش پرداخت');
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
         * چاپ قبض
         */
        function handlePrintReceipt(receptionId) {
            const url = (config.printReceiptUrl || '/Reception/PrintReceipt') + '?receptionId=' + receptionId;
            window.open(url, '_blank');
        }

        /**
         * مشاهده جزئیات
         */
        function handleViewDetails(receptionId) {
            // TODO: Implement view details
            toastr.info('مشاهده جزئیات - در حال توسعه');
        }

        // Event handlers
        $('#btnSearch').on('click', function() {
            loadReceptionList(1);
        });

        $('#btnReset').on('click', function() {
            $('#filterNationalCode').val('');
            $('#filterPatientName').val('');
            $('#filterDateFrom').val('');
            $('#filterDateTo').val('');
            $('#filterStatus').val('');
            loadReceptionList(1);
        });

        // Pagination
        $(document).on('click', '#receptionListPagination .page-link', function(e) {
            e.preventDefault();
            const page = $(this).data('page');
            if (page && page !== currentPage) {
                loadReceptionList(page);
            }
        });

        // Enter key in filters
        $('#receptionListFilters input').on('keypress', function(e) {
            if (e.which === 13) {
                $('#btnSearch').click();
            }
        });

        // Initial load
        loadReceptionList(1);

        console.log('✅ Reception List: Module initialized');
    });

})(window.ReceptionAPI || window.API || {});

