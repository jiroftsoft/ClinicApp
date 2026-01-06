/**
 * JavaScript Module برای مدیریت نوبت‌های بیمار
 * رعایت SRP: فقط مدیریت لیست نوبت‌ها و عملیات مربوطه
 */
(function ($) {
    'use strict';

    const PatientAppointments = {
        init: function () {
            this.bindEvents();
            this.loadAppointments();
        },

        bindEvents: function () {
            // مشاهده جزئیات نوبت
            $(document).on('click', '.view-details-btn', this.handleViewDetails.bind(this));

            // لغو نوبت
            $(document).on('click', '.cancel-appointment-btn', this.handleCancelAppointment.bind(this));

            // ✅ ENTERPRISE-GRADE: پرداخت سریع نوبت
            $(document).on('click', '.payment-action-btn', this.handleQuickPayment.bind(this));

            // فیلتر
            $('#filterForm').on('submit', this.handleFilter.bind(this));
        },

        loadAppointments: function () {
            // در صورت نیاز، می‌توان از API استفاده کرد
            // فعلاً از Server-Side Rendering استفاده می‌کنیم
        },

        handleViewDetails: function (e) {
            e.preventDefault();
            const appointmentId = $(e.currentTarget).data('appointment-id');
            
            if (!appointmentId) {
                this.showError('شناسه نوبت نامعتبر است');
                return;
            }

            showLoading();
            
            $.ajax({
                url: '/Patient/Api/PatientAppointment/GetAppointmentDetails',
                type: 'GET',
                data: { id: appointmentId },
                success: (response) => {
                    hideLoading();
                    if (response.success && response.data) {
                        this.showAppointmentDetails(response.data);
                    } else {
                        this.showError(response.message || 'خطا در دریافت جزئیات نوبت');
                    }
                },
                error: () => {
                    hideLoading();
                    this.showError('خطا در ارتباط با سرور');
                }
            });
        },

        handleCancelAppointment: function (e) {
            e.preventDefault();
            const appointmentId = $(e.currentTarget).data('appointment-id');
            
            if (!appointmentId) {
                this.showError('شناسه نوبت نامعتبر است');
                return;
            }

            Swal.fire({
                title: 'آیا مطمئن هستید؟',
                text: 'آیا می‌خواهید این نوبت را لغو کنید؟',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'بله، لغو کن',
                cancelButtonText: 'خیر',
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d'
            }).then((result) => {
                if (result.isConfirmed) {
                    this.cancelAppointment(appointmentId);
                }
            });
        },

        cancelAppointment: function (appointmentId) {
            showLoading();

            $.ajax({
                url: '/Patient/Api/PatientAppointment/CancelAppointment',
                type: 'POST',
                data: { id: appointmentId },
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: (response) => {
                    hideLoading();
                    if (response.success) {
                        Swal.fire({
                            title: 'موفق',
                            text: response.message || 'نوبت با موفقیت لغو شد',
                            icon: 'success',
                            confirmButtonText: 'باشه'
                        }).then(() => {
                            location.reload();
                        });
                    } else {
                        this.showError(response.message || 'خطا در لغو نوبت');
                    }
                },
                error: () => {
                    hideLoading();
                    this.showError('خطا در ارتباط با سرور');
                }
            });
        },

        handleQuickPayment: function (e) {
            e.preventDefault();
            const appointmentId = $(e.currentTarget).data('appointment-id');
            const price = $(e.currentTarget).data('price');
            
            if (!appointmentId) {
                this.showError('شناسه نوبت نامعتبر است');
                return;
            }

            // ✅ ENTERPRISE-GRADE: تایید پرداخت با نمایش مبلغ
            Swal.fire({
                title: 'تایید پرداخت',
                html: `
                    <p class="mb-3">آیا می‌خواهید برای این نوبت پرداخت کنید؟</p>
                    <div class="alert alert-info">
                        <strong>مبلغ قابل پرداخت:</strong> 
                        <span class="h5 text-medical-primary">${price ? price.toLocaleString('fa-IR') : 'نامشخص'} تومان</span>
                    </div>
                `,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'بله، پرداخت کن',
                cancelButtonText: 'خیر',
                confirmButtonColor: '#ffc107',
                cancelButtonColor: '#6c757d'
            }).then((result) => {
                if (result.isConfirmed) {
                    this.processPayment(appointmentId);
                }
            });
        },

        processPayment: function (appointmentId) {
            showLoading();

            const token = $('input[name="__RequestVerificationToken"]').val();
            if (!token) {
                hideLoading();
                this.showError('خطا در دریافت توکن امنیتی. لطفاً صفحه را نوسازی کنید.');
                return;
            }

            $.ajax({
                url: '/Patient/AppointmentBooking/ProcessPayment',
                type: 'POST',
                data: {
                    appointmentId: appointmentId,
                    paymentMethod: 'online',
                    __RequestVerificationToken: token
                },
                dataType: 'json',
                timeout: 30000,
                success: (response) => {
                    hideLoading();
                    console.log('✅ [PatientAppointments] Payment response received:', response);
                    
                    if (response && response.success === true && response.paymentUrl) {
                        // هدایت به درگاه پرداخت
                        console.log('🔄 [PatientAppointments] Redirecting to payment gateway:', response.paymentUrl);
                        
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
                        const errorMessage = response?.message || 'خطا در ایجاد درخواست پرداخت';
                        console.error('❌ [PatientAppointments] Payment request failed - Message:', errorMessage);
                        
                        Swal.fire({
                            title: 'خطا در پردازش پرداخت',
                            text: errorMessage,
                            icon: 'warning',
                            confirmButtonText: 'باشه',
                            confirmButtonColor: '#2c5aa0'
                        });
                    }
                },
                error: (xhr, status, error) => {
                    hideLoading();
                    console.error('❌ [PatientAppointments] AJAX Error in processPayment:', { status, error, xhr });
                    
                    let errorMessage = 'خطا در پردازش پرداخت';
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    } else if (status === 'timeout') {
                        errorMessage = 'زمان اتصال به سرور به پایان رسید. لطفاً دوباره تلاش کنید.';
                    } else if (status === 'error' && xhr.status === 0) {
                        errorMessage = 'خطا در اتصال به سرور. لطفاً اتصال اینترنت خود را بررسی کنید.';
                    } else if (xhr.status >= 500) {
                        errorMessage = 'خطای سرور. لطفاً چند لحظه صبر کنید و دوباره تلاش کنید.';
                    } else if (xhr.status === 400) {
                        errorMessage = 'اطلاعات ارسالی نامعتبر است. لطفاً صفحه را رفرش کنید و دوباره تلاش کنید.';
                    }
                    
                    Swal.fire({
                        title: 'خطا در پردازش پرداخت',
                        text: errorMessage,
                        icon: 'error',
                        confirmButtonText: 'باشه',
                        confirmButtonColor: '#2c5aa0'
                    });
                }
            });
        },

        handleFilter: function (e) {
            // فیلتر به صورت Server-Side انجام می‌شود
            // این متد فقط برای جلوگیری از submit پیش‌فرض است
        },

        showAppointmentDetails: function (data) {
            const detailsHtml = `
                <div class="appointment-details">
                    <h5 class="mb-3">جزئیات نوبت</h5>
                    <div class="row mb-2">
                        <div class="col-4"><strong>پزشک:</strong></div>
                        <div class="col-8">${data.doctorName}</div>
                    </div>
                    <div class="row mb-2">
                        <div class="col-4"><strong>تخصص:</strong></div>
                        <div class="col-8">${data.doctorSpecialization}</div>
                    </div>
                    <div class="row mb-2">
                        <div class="col-4"><strong>تاریخ:</strong></div>
                        <div class="col-8">${data.appointmentDate}</div>
                    </div>
                    <div class="row mb-2">
                        <div class="col-4"><strong>زمان:</strong></div>
                        <div class="col-8">${data.appointmentTime}</div>
                    </div>
                    <div class="row mb-2">
                        <div class="col-4"><strong>وضعیت:</strong></div>
                        <div class="col-8">${data.statusDisplay}</div>
                    </div>
                    <div class="row mb-2">
                        <div class="col-4"><strong>مبلغ:</strong></div>
                        <div class="col-8">${data.price.toLocaleString('fa-IR')} تومان</div>
                    </div>
                </div>
            `;

            Swal.fire({
                title: 'جزئیات نوبت',
                html: detailsHtml,
                icon: 'info',
                confirmButtonText: 'بستن',
                width: '600px'
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
        PatientAppointments.init();
    });

    // Export for global access if needed
    window.PatientAppointments = PatientAppointments;

})(jQuery);

