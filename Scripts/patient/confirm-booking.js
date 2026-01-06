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

            // ✅ CRITICAL FIX: بهبود Error Handling با Retry Logic و Timeout
            // ⚠️ Note: برای Reserve، Retry باید با احتیاط باشد (Idempotency)
            // ✅ CRITICAL FIX: Use dynamic URL from Razor instead of hardcoded
            const reserveUrl = window.appConfig?.appointmentBooking?.reserveUrl || '/Patient/AppointmentBooking/Reserve';
            
            // ✅ CRITICAL FIX: Ensure idempotency key is included in form data
            const idempotencyKey = $('#idempotencyKey').val();
            if (idempotencyKey) {
                formData += `&idempotencyKey=${encodeURIComponent(idempotencyKey)}`;
            }
            
            this.ajaxWithRetry({
                url: reserveUrl,
                type: 'POST',
                data: formData,
                timeout: 60000, // ✅ 60 ثانیه Timeout برای Reserve (ممکن است طول بکشد)
                maxRetries: 1, // ✅ فقط 1 بار Retry برای Reserve (به دلیل Idempotency)
                retryDelay: 2000, // ✅ 2 ثانیه تاخیر
                onSuccess: async (response) => {
                    // ✅ CRITICAL FIX: Wrap entire onSuccess in try-catch to catch unhandled exceptions
                    try {
                        hideLoading();
                        console.log('✅ [ConfirmBooking] Reserve response received:', response);
                        console.log('🔍 [ConfirmBooking] Response details - success:', response?.success, 'requiresPayment:', response?.requiresPayment, 'appointmentId:', response?.appointmentId);
                        
                        // ✅ CRITICAL FIX: بررسی دقیق‌تر response
                        if (response && response.success === true) {
                            console.log('✅ [ConfirmBooking] Reserve successful - AppointmentId:', response.appointmentId, 'RequiresPayment:', response.requiresPayment);
                            
                            // ✅ CRITICAL FIX: اگر نیاز به پرداخت دارد، پرداخت را انجام بده
                            if (response.requiresPayment === true && response.appointmentId) {
                                console.log('💰 [ConfirmBooking] Starting payment process for AppointmentId:', response.appointmentId);
                                try {
                                    await this.processPayment(response.appointmentId);
                                } catch (error) {
                                    console.error('❌ [ConfirmBooking] Error in processPayment:', error);
                                    // ✅ CRITICAL FIX: نمایش خطای پرداخت به صورت جداگانه
                                    // ✅ CRITICAL FIX: نوبت با موفقیت رزرو شده است، فقط پرداخت خطا دارد
                                    Swal.fire({
                                        title: 'خطا در پردازش پرداخت',
                                        text: 'نوبت شما با موفقیت رزرو شده است. لطفاً از بخش "نوبت‌های من" برای پرداخت اقدام کنید.',
                                        icon: 'warning',
                                        confirmButtonText: 'باشه',
                                        confirmButtonColor: '#2c5aa0'
                                    }).then(() => {
                                        window.location.href = '/Patient/Appointment/MyAppointments';
                                    });
                                }
                            } else {
                                console.log('✅ [ConfirmBooking] No payment required, showing success');
                                this.showSuccess(response);
                            }
                        } else {
                            // ✅ CRITICAL FIX: Display warnings separately from errors
                            let errorMessage = response?.message || 'خطا در رزرو نوبت';
                            console.error('❌ [ConfirmBooking] Reserve failed - Message:', errorMessage, 'Response:', response);
                            
                            // Display warnings separately if present
                            if (response?.warnings && response.warnings.length > 0) {
                                const warningsText = response.warnings.join('\n');
                                Swal.fire({
                                    title: 'هشدار',
                                    html: `<p>${warningsText}</p>`,
                                    icon: 'warning',
                                    confirmButtonText: 'باشه',
                                    confirmButtonColor: '#2c5aa0'
                                }).then(() => {
                                    // After user acknowledges warnings, show error
                                    this.showError(errorMessage);
                                });
                            } else {
                                this.showError(errorMessage);
                            }
                        }
                    } catch (error) {
                        // ✅ CRITICAL FIX: Catch any unhandled exceptions in onSuccess
                        hideLoading();
                        console.error('❌ [ConfirmBooking] Unhandled error in onSuccess:', error);
                        console.error('❌ [ConfirmBooking] Error stack:', error.stack);
                        console.error('❌ [ConfirmBooking] Response was:', response);
                        
                        // ✅ CRITICAL FIX: If response was successful but error occurred, show payment error
                        if (response && response.success === true && response.requiresPayment === true) {
                            Swal.fire({
                                title: 'خطا در پردازش پرداخت',
                                text: 'نوبت شما با موفقیت رزرو شده است. لطفاً از بخش "نوبت‌های من" برای پرداخت اقدام کنید.',
                                icon: 'warning',
                                confirmButtonText: 'باشه',
                                confirmButtonColor: '#2c5aa0'
                            }).then(() => {
                                window.location.href = '/Patient/Appointment/MyAppointments';
                            });
                        } else {
                            // ✅ CRITICAL FIX: Generic error for other cases
                            this.showError('خطا در پردازش درخواست. لطفاً دوباره تلاش کنید.');
                        }
                    }
                },
                onError: (xhr, status, error) => {
                    hideLoading();
                    let errorMessage = 'خطا در ارتباط با سرور';
                    
                    // ✅ تشخیص نوع خطا و نمایش پیام مناسب
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    } else if (status === 'timeout') {
                        errorMessage = 'زمان اتصال به سرور به پایان رسید. لطفاً اتصال اینترنت خود را بررسی کنید و دوباره تلاش کنید.';
                    } else if (status === 'error' && xhr.status === 0) {
                        errorMessage = 'خطا در اتصال به سرور. لطفاً اتصال اینترنت خود را بررسی کنید.';
                    } else if (xhr.status >= 500) {
                        errorMessage = 'خطای سرور. لطفاً چند لحظه صبر کنید و دوباره تلاش کنید.';
                    } else if (xhr.status === 400) {
                        errorMessage = 'اطلاعات ارسالی نامعتبر است. لطفاً صفحه را رفرش کنید و دوباره تلاش کنید.';
                    }
                    
                    this.showError(errorMessage);
                    console.error('❌ [ConfirmBooking] AJAX Error:', { status, error, xhr });
                }
            });
        },

        processPayment: async function (appointmentId) {
            console.log('💰 ConfirmBooking: processPayment called - AppointmentId:', appointmentId);
            
            // ✅ CRITICAL FIX: بررسی وجود AppointmentPayment Module
            if (window.AppointmentPayment && typeof window.AppointmentPayment.processPayment === 'function') {
                console.log('✅ ConfirmBooking: Using AppointmentPayment module');
                try {
                    // ✅ CRITICAL FIX: AppointmentPayment.processPayment یک Promise برنمی‌گرداند
                    // پس باید از fallback استفاده کنیم یا آن را wrap کنیم
                    // فعلاً از fallback استفاده می‌کنیم تا error handling بهتری داشته باشیم
                    // window.AppointmentPayment.processPayment(appointmentId);
                    // return;
                    console.warn('⚠️ ConfirmBooking: AppointmentPayment module found but using fallback for better error handling');
                } catch (error) {
                    console.error('❌ ConfirmBooking: Error in AppointmentPayment.processPayment:', error);
                    // Fallback to direct AJAX call
                }
            } else {
                console.warn('⚠️ ConfirmBooking: AppointmentPayment module not available, using fallback');
            }

            // ✅ Fallback: استفاده از کد قبلی
            showLoading();

            try {
                const token = $('input[name="__RequestVerificationToken"]').val();
                if (!token) {
                    console.error('❌ ConfirmBooking: AntiForgeryToken not found');
                    hideLoading();
                    // ✅ CRITICAL FIX: Throw exception برای catch block در submitBooking
                    throw new Error('خطا در دریافت توکن امنیتی. لطفاً صفحه را نوسازی کنید.');
                }

                console.log('💰 ConfirmBooking: Sending payment request - AppointmentId:', appointmentId);
                
                const response = await $.ajax({
                    url: '/Patient/AppointmentBooking/ProcessPayment',
                    type: 'POST',
                    data: {
                        appointmentId: appointmentId,
                        paymentMethod: 'online',
                        __RequestVerificationToken: token
                    },
                    dataType: 'json',
                    timeout: 30000
                });

                hideLoading();
                console.log('✅ ConfirmBooking: Payment response received:', response);
                console.log('🔍 [ConfirmBooking] Payment response details - success:', response?.success, 'paymentUrl:', response?.paymentUrl);

                if (response && response.success === true && response.paymentUrl) {
                    // هدایت به درگاه پرداخت
                    console.log('🔄 ConfirmBooking: Redirecting to payment gateway:', response.paymentUrl);
                    
                    if (typeof Swal !== 'undefined') {
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
                    }

                    // هدایت به درگاه پس از 1 ثانیه
                    setTimeout(() => {
                        window.location.href = response.paymentUrl;
                    }, 1000);
                } else {
                    // ✅ CRITICAL FIX: نمایش خطای پرداخت به صورت جداگانه (نه "خطا در رزرو نوبت")
                    const errorMessage = response?.message || 'خطا در ایجاد درخواست پرداخت';
                    console.error('❌ ConfirmBooking: Payment request failed - Message:', errorMessage, 'Response:', response);
                    
                    // ✅ CRITICAL FIX: Throw exception برای catch block در submitBooking
                    throw new Error(errorMessage);
                }
            } catch (error) {
                hideLoading();
                console.error('❌ ConfirmBooking: AJAX Error in processPayment:', error);
                
                let errorMessage = 'خطا در پردازش پرداخت';
                if (error.responseJSON && error.responseJSON.message) {
                    errorMessage = error.responseJSON.message;
                } else if (error.message) {
                    errorMessage = error.message;
                } else if (error.status === 0) {
                    errorMessage = 'خطا در اتصال به سرور. لطفاً اتصال اینترنت خود را بررسی کنید.';
                } else if (error.status >= 500) {
                    errorMessage = 'خطای سرور. لطفاً چند لحظه صبر کنید و دوباره تلاش کنید.';
                } else if (error.status === 400) {
                    errorMessage = 'اطلاعات ارسالی نامعتبر است. لطفاً صفحه را رفرش کنید و دوباره تلاش کنید.';
                }
                
                // ✅ CRITICAL FIX: Throw exception برای catch block در submitBooking
                throw new Error(errorMessage);
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

        /**
         * ✅ CRITICAL FIX: AJAX Helper با Retry Logic و Timeout Handling
         * طبق قراردادها: Bulletproof Error Handling
         */
        ajaxWithRetry: function (options) {
            const self = this;
            let retryCount = 0;
            const maxRetries = options.maxRetries || 3;
            const retryDelay = options.retryDelay || 1000;
            const timeout = options.timeout || 30000;

            function makeRequest() {
                $.ajax({
                    url: options.url,
                    type: options.type || 'GET',
                    data: options.data || {},
                    headers: options.headers || {},
                    timeout: timeout,
                    success: function (response) {
                        if (options.onSuccess) {
                            options.onSuccess(response);
                        }
                    },
                    error: function (xhr, status, error) {
                        // ✅ تشخیص نوع خطا
                        const isNetworkError = status === 'timeout' || 
                                             status === 'error' && xhr.status === 0 ||
                                             status === 'abort';
                        
                        const isServerError = xhr.status >= 500;
                        const isClientError = xhr.status >= 400 && xhr.status < 500;

                        // ✅ Retry Logic برای Network Errors و Server Errors (نه Client Errors)
                        if (retryCount < maxRetries && (isNetworkError || isServerError) && !isClientError) {
                            retryCount++;
                            console.warn(`⚠️ [ConfirmBooking] Retry attempt ${retryCount}/${maxRetries} for ${options.url}`);
                            
                            // ✅ Exponential Backoff
                            const delay = retryDelay * Math.pow(2, retryCount - 1);
                            
                            setTimeout(function () {
                                makeRequest();
                            }, delay);
                        } else {
                            // ✅ تمام تلاش‌ها انجام شد یا خطای Client Error
                            if (options.onError) {
                                options.onError(xhr, status, error);
                            } else {
                                self.showError('خطا در ارتباط با سرور. لطفاً دوباره تلاش کنید.');
                            }
                        }
                    }
                });
            }

            makeRequest();
        },

        showError: function (message) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'خطا',
                    text: message,
                    icon: 'error',
                    confirmButtonText: 'باشه',
                    confirmButtonColor: '#2c5aa0'
                });
            } else if (typeof toastr !== 'undefined') {
                toastr.error(message);
            } else {
                alert(message);
            }
        }
    };

    // Initialize on document ready
    $(document).ready(function () {
        ConfirmBooking.init();
    });

    // Export for global access
    window.ConfirmBooking = ConfirmBooking;

})(jQuery);

