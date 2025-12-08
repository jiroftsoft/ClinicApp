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

