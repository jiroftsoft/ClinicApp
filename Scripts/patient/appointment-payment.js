/**
 * ✅ Appointment Payment Flow - Mobile-Optimized
 * طراحی شده برای 90% کاربران موبایل
 * 
 * ویژگی‌های کلیدی:
 * 1. Mobile-First Design
 * 2. Loading States
 * 3. Error Handling
 * 4. Auto-redirect to Gateway
 * 5. Idempotency Support
 * 
 * طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
 */

(function ($) {
    'use strict';

    var AppointmentPayment = {
        config: {
            apiBaseUrl: '/Patient/AppointmentBooking',
            processing: false,
            lastPaymentRequest: null
        },

        /**
         * ✅ Initialize Payment Flow
         */
        init: function () {
            var self = this;

            console.log('✅ AppointmentPayment: Initialized');

            // ✅ Event Handlers
            $(document).on('click', '.btn-process-payment', function (e) {
                e.preventDefault();
                var appointmentId = $(this).data('appointment-id');
                if (appointmentId) {
                    self.processPayment(appointmentId);
                }
            });

            // ✅ Auto-process payment if data attribute exists
            var autoProcessAppointmentId = $('[data-auto-process-payment]').data('auto-process-payment');
            if (autoProcessAppointmentId && !self.config.processing) {
                console.log('✅ AppointmentPayment: Auto-processing payment for AppointmentId:', autoProcessAppointmentId);
                setTimeout(function () {
                    self.processPayment(autoProcessAppointmentId);
                }, 500); // Delay for page load
            }
        },

        /**
         * ✅ Process Payment Request
         */
        processPayment: function (appointmentId) {
            var self = this;

            // ✅ Prevent duplicate requests (Idempotency)
            if (self.config.processing) {
                console.warn('⚠️ AppointmentPayment: Payment already processing');
                return;
            }

            if (self.config.lastPaymentRequest === appointmentId) {
                console.warn('⚠️ AppointmentPayment: Duplicate payment request prevented');
                return;
            }

            self.config.processing = true;
            self.config.lastPaymentRequest = appointmentId;

            console.log('💰 AppointmentPayment: Processing payment - AppointmentId:', appointmentId);

            // ✅ Show Loading State
            self.showLoading();

            // ✅ Get AntiForgeryToken
            var token = $('input[name="__RequestVerificationToken"]').val();
            if (!token) {
                console.error('❌ AppointmentPayment: AntiForgeryToken not found');
                self.hideLoading();
                self.showError('خطا در دریافت توکن امنیتی. لطفاً صفحه را نوسازی کنید.');
                self.config.processing = false;
                return;
            }

            // ✅ AJAX Request
            $.ajax({
                url: self.config.apiBaseUrl + '/ProcessPayment',
                type: 'POST',
                data: {
                    appointmentId: appointmentId,
                    paymentMethod: 'online',
                    __RequestVerificationToken: token
                },
                dataType: 'json',
                timeout: 30000, // 30 seconds
                success: function (response) {
                    self.config.processing = false;

                    if (response && response.success && response.paymentUrl) {
                        console.log('✅ AppointmentPayment: Payment request successful - PaymentUrl:', response.paymentUrl);

                        // ✅ Auto-redirect to Gateway
                        self.redirectToGateway(response.paymentUrl);
                    } else {
                        console.error('❌ AppointmentPayment: Payment request failed - Message:', response.message);
                        self.hideLoading();
                        self.showError(response.message || 'خطا در ایجاد درخواست پرداخت');
                    }
                },
                error: function (xhr, status, error) {
                    self.config.processing = false;
                    console.error('❌ AppointmentPayment: AJAX Error - Status:', status, 'Error:', error);

                    var errorMessage = 'خطا در ارتباط با سرور';
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    } else if (status === 'timeout') {
                        errorMessage = 'زمان درخواست به پایان رسید. لطفاً دوباره تلاش کنید.';
                    }

                    self.hideLoading();
                    self.showError(errorMessage);
                }
            });
        },

        /**
         * ✅ Redirect to Payment Gateway
         */
        redirectToGateway: function (paymentUrl) {
            var self = this;

            console.log('🔄 AppointmentPayment: Redirecting to gateway - URL:', paymentUrl);

            // ✅ Show redirect message
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'در حال هدایت به درگاه پرداخت...',
                    text: 'لطفاً صبر کنید',
                    icon: 'info',
                    allowOutsideClick: false,
                    allowEscapeKey: false,
                    showConfirmButton: false,
                    didOpen: function () {
                        Swal.showLoading();
                    }
                });
            }

            // ✅ Redirect after short delay (for mobile UX)
            setTimeout(function () {
                window.location.href = paymentUrl;
            }, 1000);
        },

        /**
         * ✅ Show Loading State
         */
        showLoading: function () {
            // ✅ Disable buttons
            $('.btn-process-payment').prop('disabled', true).addClass('loading');

            // ✅ Show loading overlay (if exists)
            if ($('.payment-loading-overlay').length === 0) {
                $('body').append(`
                    <div class="payment-loading-overlay" style="
                        position: fixed;
                        top: 0;
                        left: 0;
                        width: 100%;
                        height: 100%;
                        background: rgba(0, 0, 0, 0.5);
                        z-index: 9999;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                    ">
                        <div style="
                            background: white;
                            padding: 2rem;
                            border-radius: 8px;
                            text-align: center;
                        ">
                            <i class="fas fa-spinner fa-spin" style="font-size: 2rem; color: var(--medical-primary);"></i>
                            <p style="margin-top: 1rem; color: var(--medical-text);">در حال پردازش...</p>
                        </div>
                    </div>
                `);
            }
        },

        /**
         * ✅ Hide Loading State
         */
        hideLoading: function () {
            // ✅ Enable buttons
            $('.btn-process-payment').prop('disabled', false).removeClass('loading');

            // ✅ Hide loading overlay
            $('.payment-loading-overlay').remove();
        },

        /**
         * ✅ Show Error Message
         */
        showError: function (message) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'خطا',
                    text: message,
                    icon: 'error',
                    confirmButtonText: 'باشه',
                    confirmButtonColor: '#dc3545'
                });
            } else {
                // ✅ استفاده از SweetAlert2 به جای alert()
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        title: 'خطا',
                        text: message,
                        icon: 'error',
                        confirmButtonText: 'باشه',
                        confirmButtonColor: '#dc3545'
                    });
                } else {
                    // Fallback فقط در صورت عدم وجود SweetAlert2
                    console.error('❌ Payment Error:', message);
                }
            }
        }
    };

    // ✅ Initialize on DOM Ready
    $(document).ready(function () {
        AppointmentPayment.init();
    });

    // ✅ Expose globally for manual calls
    window.AppointmentPayment = AppointmentPayment;

})(jQuery);

