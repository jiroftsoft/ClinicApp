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
            if (typeof showLoading === 'function') showLoading();
            $.ajax({
                url: '/Patient/Api/PatientAppointment/GetAppointmentDetails',
                type: 'GET',
                data: { id: appointmentId },
                dataType: 'json'
            }).done(function (response) {
                if (typeof hideLoading === 'function') hideLoading();
                if (response && response.success && response.data) {
                    PatientAppointments.showAppointmentDetailsModal(response.data);
                    } else {
                    PatientAppointments.showError(response && response.message ? response.message : 'خطا در دریافت جزئیات نوبت');
                    }
            }).fail(function (xhr) {
                if (typeof hideLoading === 'function') hideLoading();
                var msg = (xhr.responseJSON && xhr.responseJSON.message) ? xhr.responseJSON.message : 'خطا در ارتباط با سرور';
                PatientAppointments.showError(msg);
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
                html: 'آیا می‌خواهید این نوبت را لغو کنید؟<br><small class="text-muted">در صورت پرداخت آنلاین، استرداد مبلغ از طریق درگاه انجام نمی‌شود؛ برای استرداد به واحد پذیرش مراجعه کنید.</small>',
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
                    if (typeof hideLoading === 'function') hideLoading();
                    if (response.success) {
                        Swal.fire({
                            title: 'موفق',
                            text: response.message || 'نوبت با موفقیت لغو شد',
                            icon: 'success',
                            confirmButtonText: 'باشه'
                        }).then(function () {
                            if (window.UnifiedDashboard && typeof window.UnifiedDashboard.reloadTab === 'function') {
                                window.UnifiedDashboard.reloadTab('appointments');
                            } else {
                            location.reload();
                            }
                        });
                    } else {
                        PatientAppointments.showError(response.message || 'خطا در لغو نوبت');
                    }
                },
                error: function () {
                    if (typeof hideLoading === 'function') hideLoading();
                    PatientAppointments.showError('خطا در ارتباط با سرور');
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
                        const code = response?.code || response?.securityDetails || '';
                        const correlationId = response?.correlationId || '';
                        console.error('❌ [PatientAppointments] Payment request failed - Message:', errorMessage, 'Code:', code, 'CorrelationId:', correlationId);
                        
                        let detailText = errorMessage;
                        if (code || correlationId) {
                            detailText += '\n\nدر صورت تکرار، این موارد را به پشتیبانی بدهید:';
                            if (code) detailText += '\nکد خطا: ' + code;
                            if (correlationId) detailText += '\nشناسه: ' + correlationId;
                        }
                        Swal.fire({
                            title: 'خطا در پردازش پرداخت',
                            text: detailText,
                            icon: 'warning',
                            confirmButtonText: 'باشه',
                            confirmButtonColor: '#2c5aa0'
                        });
                    }
                },
                error: (xhr, status, error) => {
                    hideLoading();
                    const resp = xhr.responseJSON || {};
                    const code = resp.code || resp.securityDetails || '';
                    const correlationId = resp.correlationId || '';
                    console.error('❌ [PatientAppointments] AJAX Error in processPayment:', { status, error, code, correlationId, xhr });
                    
                    let errorMessage = 'خطا در پردازش پرداخت';
                    if (resp.message) {
                        errorMessage = resp.message;
                    } else if (status === 'timeout') {
                        errorMessage = 'زمان اتصال به سرور به پایان رسید. لطفاً دوباره تلاش کنید.';
                    } else if (status === 'error' && xhr.status === 0) {
                        errorMessage = 'خطا در اتصال به سرور. لطفاً اتصال اینترنت خود را بررسی کنید.';
                    } else if (xhr.status >= 500) {
                        errorMessage = 'خطای سرور. لطفاً چند لحظه صبر کنید و دوباره تلاش کنید.';
                    } else if (xhr.status === 400) {
                        errorMessage = 'اطلاعات ارسالی نامعتبر است. لطفاً صفحه را رفرش کنید و دوباره تلاش کنید.';
                    }
                    if (code || correlationId) {
                        errorMessage += '\n\nدر صورت تکرار به پشتیبانی بدهید:';
                        if (code) errorMessage += ' کد: ' + code;
                        if (correlationId) errorMessage += ' شناسه: ' + correlationId;
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

        /** مقدار از API با PascalCase برمی‌گردد؛ هر دو حالت را پشتیبانی می‌کنیم */
        _get: function (data, key) {
            var v = data[key];
            if (v !== undefined && v !== null) return v;
            var camel = key.charAt(0).toLowerCase() + key.slice(1);
            return data[camel] !== undefined ? data[camel] : '';
        },

        /**
         * تبدیل مقدار تاریخ API به متن شمسی خوانا — پشتیبانی از /Date(ticks)/ و ISO و عدد
         * الهام از سایت‌های مطرح (دکترتو، دیجی‌کالا، با سلام)
         */
        _formatAppointmentDate: function (dateValue) {
            if (dateValue === undefined || dateValue === null || dateValue === '') return '—';
            var d = null;
            try {
                if (typeof dateValue === 'string') {
                    if (dateValue.indexOf('/Date(') === 0 && dateValue.indexOf(')/') !== -1) {
                        var tick = parseInt(dateValue.replace(/^\/Date\(/, '').replace(/\)\/$/, ''), 10);
                        if (!isNaN(tick)) d = new Date(tick);
                    } else if (dateValue.indexOf('T') !== -1 || /^\d{4}-\d{2}-\d{2}/.test(dateValue)) {
                        d = new Date(dateValue);
                    }
                } else if (typeof dateValue === 'number') {
                    d = new Date(dateValue);
                }
                if (d && !isNaN(d.getTime())) {
                    return d.toLocaleDateString('fa-IR', { year: 'numeric', month: 'long', day: 'numeric' });
                }
            } catch (e) { }
            return typeof dateValue === 'string' ? dateValue : '—';
        },

        /**
         * مودال AJAX تمیز با جزئیات کامل نوبت — بدون رفرش، حرفه‌ای، اندازه بهینه
         */
        showAppointmentDetailsModal: function (data) {
            var self = this;
            var g = function (key) { return self._get(data, key); };
            var doctorName = g('DoctorName') || '—';
            var doctorSpec = g('DoctorSpecialization') || '—';
            var medicalCouncil = g('MedicalCouncilCode');
            var appointmentDate = self._formatAppointmentDate(g('AppointmentDate'));
            var appointmentTime = g('AppointmentTime') || '—';
            var clinicName = g('ClinicName') || '—';
            var deptName = g('DepartmentName') || '—';
            var statusDisplay = g('StatusDisplay') || '—';
            var price = typeof data.Price !== 'undefined' ? data.Price : (data.price != null ? data.price : 0);
            var priceStr = (typeof price === 'number' ? price : parseFloat(price) || 0).toLocaleString('fa-IR');
            var duration = g('Duration');
            var description = g('Description');
            var requiresPayment = data.RequiresPayment === true || data.requiresPayment === true;
            var status = g('Status');
            var statusStr = (typeof status === 'string' ? status : (status != null ? String(status) : '')).toLowerCase();
            var canCancel = (statusStr.indexOf('scheduled') !== -1 || statusStr === 'scheduled') && !requiresPayment;
            var appointmentId = data.AppointmentId || data.appointmentId;

            var esc = function (s) {
                if (s == null) return '';
                s = String(s);
                return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
            };

            var html = '<div class="appointment-details-modal text-right" dir="rtl">' +
                '<div class="apt-modal-section">' +
                '<div class="apt-modal-row"><span class="apt-modal-label">پزشک</span><span class="apt-modal-value">' + esc(doctorName) + '</span></div>' +
                '<div class="apt-modal-row"><span class="apt-modal-label">تخصص</span><span class="apt-modal-value">' + esc(doctorSpec) + '</span></div>' +
                (medicalCouncil ? '<div class="apt-modal-row"><span class="apt-modal-label">کد نظام پزشکی</span><span class="apt-modal-value">' + esc(medicalCouncil) + '</span></div>' : '') +
                '<div class="apt-modal-row"><span class="apt-modal-label">تاریخ</span><span class="apt-modal-value">' + esc(appointmentDate) + '</span></div>' +
                '<div class="apt-modal-row"><span class="apt-modal-label">زمان</span><span class="apt-modal-value">' + esc(appointmentTime) + '</span></div>' +
                (duration ? '<div class="apt-modal-row"><span class="apt-modal-label">مدت ویزیت</span><span class="apt-modal-value">' + esc(duration) + ' دقیقه</span></div>' : '') +
                '<div class="apt-modal-row"><span class="apt-modal-label">کلینیک</span><span class="apt-modal-value">' + esc(clinicName) + '</span></div>' +
                '<div class="apt-modal-row"><span class="apt-modal-label">بخش</span><span class="apt-modal-value">' + esc(deptName) + '</span></div>' +
                '<div class="apt-modal-row"><span class="apt-modal-label">وضعیت</span><span class="apt-modal-value">' + esc(statusDisplay) + '</span></div>' +
                '<div class="apt-modal-row"><span class="apt-modal-label">مبلغ</span><span class="apt-modal-value">' + priceStr + ' تومان</span></div>' +
                (description ? '<div class="apt-modal-row apt-modal-notes"><span class="apt-modal-label">توضیحات</span><span class="apt-modal-value">' + esc(description) + '</span></div>' : '') +
                '</div></div>';

            var footer = '<div class="apt-modal-footer-actions">' +
                (requiresPayment ? '<button type="button" class="btn btn-warning btn-sm apt-modal-pay-btn payment-action-btn" data-appointment-id="' + appointmentId + '" data-price="' + (price || 0) + '"><i class="fas fa-credit-card ml-1"></i> پرداخت سریع</button>' : '') +
                (canCancel ? '<button type="button" class="btn btn-outline-danger btn-sm apt-modal-cancel-btn cancel-appointment-btn" data-appointment-id="' + appointmentId + '"><i class="fas fa-times ml-1"></i> لغو نوبت</button>' : '') +
                '<button type="button" class="btn btn-secondary btn-sm apt-modal-close-btn">بستن</button></div>';

            Swal.fire({
                title: '<i class="fas fa-calendar-check text-primary me-2"></i>جزئیات نوبت',
                html: html + footer,
                showConfirmButton: false,
                showCloseButton: true,
                width: 'min(92vw, 560px)',
                customClass: { container: 'apt-details-swal', popup: 'apt-details-swal-popup', htmlContainer: 'apt-details-swal-html' },
                didOpen: function (el) {
                    var $pop = $(el);
                    $pop.find('.apt-modal-close-btn').on('click', function () { Swal.close(); });
                    $pop.find('.apt-modal-pay-btn').on('click', function () {
                        Swal.close();
                        PatientAppointments.handleQuickPayment({ preventDefault: function () { }, currentTarget: this });
                    });
                    $pop.find('.apt-modal-cancel-btn').on('click', function () {
                        Swal.close();
                        PatientAppointments.handleCancelAppointment({ preventDefault: function () { }, currentTarget: this });
                    });
                }
            });
        },

        showAppointmentDetails: function (data) {
            this.showAppointmentDetailsModal(data);
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

