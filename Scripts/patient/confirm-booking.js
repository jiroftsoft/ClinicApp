/**
 * JavaScript Module برای تایید و پرداخت نوبت
 * رعایت SRP: فقط مدیریت تایید نهایی و پرداخت
 */
(function ($) {
    'use strict';

    const ConfirmBooking = {
        init: function () {
            this.bindEvents();
        },

        bindEvents: function () {
            // انتخاب روش پرداخت
            $('.payment-method-card').on('click', this.handlePaymentMethodSelection.bind(this));
            
            // تایید و پرداخت
            $('#bookingForm').on('submit', this.handleBookingSubmit.bind(this));
        },

        handlePaymentMethodSelection: function (e) {
            $('.payment-method-card').removeClass('selected');
            $(e.currentTarget).addClass('selected');
        },

        handleBookingSubmit: function (e) {
            e.preventDefault();
            
            const form = $(e.target);
            const formData = form.serialize();

            Swal.fire({
                title: 'آیا مطمئن هستید؟',
                text: 'آیا می‌خواهید این نوبت را رزرو کنید؟',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'بله، رزرو کن',
                cancelButtonText: 'خیر',
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d'
            }).then((result) => {
                if (result.isConfirmed) {
                    this.submitBooking(formData);
                }
            });
        },

        submitBooking: function (formData) {
            showLoading();

            $.ajax({
                url: '/Patient/AppointmentBooking/Reserve',
                type: 'POST',
                data: formData,
                success: async (response) => {
                    hideLoading();
                    if (response.success) {
                        // اگر نیاز به پرداخت دارد، پرداخت را انجام بده
                        if (response.requiresPayment && response.appointmentId) {
                            await this.processPayment(response.appointmentId);
                        } else {
                            this.showSuccess(response);
                        }
                    } else {
                        this.showError(response.message || 'خطا در رزرو نوبت');
                    }
                },
                error: (xhr) => {
                    hideLoading();
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        this.showError(xhr.responseJSON.message);
                    } else {
                        this.showError('خطا در ارتباط با سرور');
                    }
                }
            });
        },

        processPayment: async function (appointmentId) {
            showLoading();

            try {
                const response = await $.ajax({
                    url: '/Patient/AppointmentBooking/ProcessPayment',
                    type: 'POST',
                    data: {
                        appointmentId: appointmentId,
                        paymentMethod: 'online',
                        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                    }
                });

                hideLoading();

                if (response.success && response.paymentUrl) {
                    // هدایت به درگاه پرداخت
                    Swal.fire({
                        title: 'در حال هدایت به درگاه پرداخت',
                        text: 'لطفاً صبر کنید...',
                        icon: 'info',
                        allowOutsideClick: false,
                        allowEscapeKey: false,
                        showConfirmButton: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    // هدایت به درگاه پس از 1 ثانیه
                    setTimeout(() => {
                        window.location.href = response.paymentUrl;
                    }, 1000);
                } else {
                    this.showError(response.message || 'خطا در ایجاد درخواست پرداخت');
                }
            } catch (error) {
                hideLoading();
                if (error.responseJSON && error.responseJSON.message) {
                    this.showError(error.responseJSON.message);
                } else {
                    this.showError('خطا در پردازش پرداخت');
                }
            }
        },

        showSuccess: function (response) {
            Swal.fire({
                title: 'موفق',
                text: response.message || 'نوبت با موفقیت رزرو شد',
                icon: 'success',
                confirmButtonText: 'باشه',
                allowOutsideClick: false,
                allowEscapeKey: false
            }).then(() => {
                // هدایت به صفحه نوبت‌های من
                window.location.href = '/Patient/Appointment/MyAppointments';
            });
        },

        showError: function (message) {
            Swal.fire({
                title: 'خطا',
                text: message,
                icon: 'error',
                confirmButtonText: 'باشه'
            });
        }
    };

    // Initialize on document ready
    $(document).ready(function () {
        ConfirmBooking.init();
    });

    // Export for global access
    window.ConfirmBooking = ConfirmBooking;

})(jQuery);

