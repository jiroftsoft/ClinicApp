/**
 * Payment Management JavaScript
 * مدیریت تعاملات و عملیات مدیریت پرداخت‌ها
 * 
 * طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md, DEVELOPMENT_CONTRACT.md
 * 
 * ویژگی‌های کلیدی:
 * - مدیریت Search & Filter (با Debounce)
 * - مدیریت Retry/Cancel/Refund (AJAX)
 * - Real-time Updates
 * - API محور بدون رفرش صفحه
 * - کاملاً واکنش‌گرا
 */

var PaymentManagement = {
    // تنظیمات
    config: {
        apiBaseUrl: '/Admin/PaymentManagement',
        debounceTime: 500, // ms
        currentPage: 1
    },

    // Initialize
    init: function() {
        var self = this;

        console.log('✅ Initializing Payment Management...');

        // Setup event listeners
        this.setupEventListeners();

        console.log('✅ Payment Management initialized successfully');
    },

    // Initialize Details Page
    initDetails: function() {
        var self = this;

        console.log('✅ Initializing Payment Details...');

        // Setup action buttons
        this.setupActionButtons();

        console.log('✅ Payment Details initialized');
    },

    // Setup Event Listeners
    setupEventListeners: function() {
        var self = this;

        // ✅ Search Input (با Debounce)
        var searchTimeout;
        $('#searchForm input[name="filter.SearchTerm"]').on('input', function() {
            clearTimeout(searchTimeout);
            var searchTerm = $(this).val();
            searchTimeout = setTimeout(function() {
                if (searchTerm.length >= 3 || searchTerm.length === 0) {
                    self.loadPayments();
                }
            }, self.config.debounceTime);
        });

        // ✅ Filter Changes
        $('#searchForm select').on('change', function() {
            self.loadPayments();
        });

        // ✅ Form Submit
        $('#searchForm').on('submit', function(e) {
            e.preventDefault();
            self.loadPayments();
        });
    },

    // Setup Action Buttons (Details Page)
    setupActionButtons: function() {
        var self = this;

        // ✅ Retry Button
        $(document).off('click.paymentManagement', '.btn-retry').on('click.paymentManagement', '.btn-retry', function(e) {
            e.preventDefault();
            e.stopPropagation();
            var paymentId = $(this).data('payment-id');
            self.handleRetry(paymentId);
        });

        // ✅ Cancel Button
        $(document).off('click.paymentManagement', '.btn-cancel').on('click.paymentManagement', '.btn-cancel', function(e) {
            e.preventDefault();
            e.stopPropagation();
            var paymentId = $(this).data('payment-id');
            self.handleCancel(paymentId);
        });

        // ✅ Refund Button
        $(document).off('click.paymentManagement', '.btn-refund').on('click.paymentManagement', '.btn-refund', function(e) {
            e.preventDefault();
            e.stopPropagation();
            var paymentId = $(this).data('payment-id');
            var amount = $(this).data('amount');
            self.handleRefund(paymentId, amount);
        });
    },

    // Load Payments (Form Submit - SSR)
    // استفاده از submit نیتیو تا رویداد submit جی‌کوئری trigger نشود و از حلقهٔ بی‌نهایت (preventDefault + loadPayments + submit) جلوگیری شود
    loadPayments: function() {
        var form = document.getElementById('searchForm');
        if (form) form.submit();
    },

    // Handle Retry
    handleRetry: function(paymentId) {
        var self = this;

        if (!paymentId) {
            Swal.fire({
                icon: 'error',
                title: 'خطا',
                text: 'شناسه پرداخت نامعتبر است'
            });
            return;
        }

        Swal.fire({
            title: 'آیا مطمئن هستید؟',
            text: 'آیا می‌خواهید این پرداخت را دوباره تلاش کنید؟',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'بله، Retry کن',
            cancelButtonText: 'لغو',
            confirmButtonColor: '#ffc107'
        }).then((result) => {
            if (result.isConfirmed) {
                self.executeRetry(paymentId);
            }
        });
    },

    // Execute Retry
    executeRetry: function(paymentId) {
        var self = this;

        self.showLoading();

        $.ajax({
            url: self.config.apiBaseUrl + '/RetryPayment',
            type: 'POST',
            data: {
                onlinePaymentId: paymentId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                self.hideLoading();
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'موفق',
                        text: response.message || 'پرداخت با موفقیت Retry شد'
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'خطا',
                        text: response.message || 'خطا در Retry پرداخت'
                    });
                }
            },
            error: function(xhr, status, error) {
                self.hideLoading();
                var errorMessage = 'خطا در ارتباط با سرور';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                Swal.fire({
                    icon: 'error',
                    title: 'خطا',
                    text: errorMessage
                });
            }
        });
    },

    // Handle Cancel
    handleCancel: function(paymentId) {
        var self = this;

        if (!paymentId) {
            Swal.fire({
                icon: 'error',
                title: 'خطا',
                text: 'شناسه پرداخت نامعتبر است'
            });
            return;
        }

        Swal.fire({
            title: 'لغو پرداخت',
            text: 'لطفاً دلیل لغو را وارد کنید:',
            input: 'text',
            inputPlaceholder: 'دلیل لغو...',
            showCancelButton: true,
            confirmButtonText: 'لغو پرداخت',
            cancelButtonText: 'انصراف',
            confirmButtonColor: '#6c757d',
            inputValidator: (value) => {
                if (!value) {
                    return 'لطفاً دلیل لغو را وارد کنید';
                }
            }
        }).then((result) => {
            if (result.isConfirmed && result.value) {
                self.executeCancel(paymentId, result.value);
            }
        });
    },

    // Execute Cancel
    executeCancel: function(paymentId, reason) {
        var self = this;

        self.showLoading();

        $.ajax({
            url: self.config.apiBaseUrl + '/CancelPayment',
            type: 'POST',
            data: {
                onlinePaymentId: paymentId,
                reason: reason,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                self.hideLoading();
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'موفق',
                        text: response.message || 'پرداخت با موفقیت Cancel شد'
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'خطا',
                        text: response.message || 'خطا در Cancel پرداخت'
                    });
                }
            },
            error: function(xhr, status, error) {
                self.hideLoading();
                var errorMessage = 'خطا در ارتباط با سرور';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                Swal.fire({
                    icon: 'error',
                    title: 'خطا',
                    text: errorMessage
                });
            }
        });
    },

    // Handle Refund
    handleRefund: function(paymentId, amount) {
        var self = this;

        if (!paymentId) {
            Swal.fire({
                icon: 'error',
                title: 'خطا',
                text: 'شناسه پرداخت نامعتبر است'
            });
            return;
        }

        Swal.fire({
            title: 'برگشت وجه',
            html: `
                <p>مبلغ پرداخت: <strong>${amount.toLocaleString()} ریال</strong></p>
                <p>لطفاً مبلغ برگشت و دلیل را وارد کنید:</p>
                <input id="refundAmount" class="swal2-input" type="number" placeholder="مبلغ برگشت (ریال)" value="${amount}" min="0" max="${amount}">
                <input id="refundReason" class="swal2-input" type="text" placeholder="دلیل برگشت...">
            `,
            showCancelButton: true,
            confirmButtonText: 'برگشت وجه',
            cancelButtonText: 'انصراف',
            confirmButtonColor: '#dc3545',
            preConfirm: () => {
                const refundAmount = parseFloat(document.getElementById('refundAmount').value);
                const refundReason = document.getElementById('refundReason').value;

                if (!refundAmount || refundAmount <= 0) {
                    Swal.showValidationMessage('مبلغ برگشت باید بیشتر از صفر باشد');
                    return false;
                }

                if (refundAmount > amount) {
                    Swal.showValidationMessage('مبلغ برگشت نمی‌تواند بیشتر از مبلغ پرداخت باشد');
                    return false;
                }

                if (!refundReason || refundReason.trim() === '') {
                    Swal.showValidationMessage('لطفاً دلیل برگشت را وارد کنید');
                    return false;
                }

                return {
                    amount: refundAmount,
                    reason: refundReason
                };
            }
        }).then((result) => {
            if (result.isConfirmed && result.value) {
                self.executeRefund(paymentId, result.value.amount, result.value.reason);
            }
        });
    },

    // Execute Refund
    executeRefund: function(paymentId, refundAmount, reason) {
        var self = this;

        self.showLoading();

        $.ajax({
            url: self.config.apiBaseUrl + '/RefundPayment',
            type: 'POST',
            data: {
                onlinePaymentId: paymentId,
                refundAmount: refundAmount,
                reason: reason,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                self.hideLoading();
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'موفق',
                        text: response.message || 'پرداخت با موفقیت Refund شد'
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'خطا',
                        text: response.message || 'خطا در Refund پرداخت'
                    });
                }
            },
            error: function(xhr, status, error) {
                self.hideLoading();
                var errorMessage = 'خطا در ارتباط با سرور';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                Swal.fire({
                    icon: 'error',
                    title: 'خطا',
                    text: errorMessage
                });
            }
        });
    },

    // Update Statistics
    updateStatistics: function(statistics) {
        // TODO: Update statistics cards if needed
    },

    // Show Loading
    showLoading: function() {
        $('#loadingOverlay').addClass('active');
    },

    // Hide Loading
    hideLoading: function() {
        $('#loadingOverlay').removeClass('active');
    }
};

