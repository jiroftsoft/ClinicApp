/**
 * JavaScript Module برای تایید و پرداخت نوبت
 * ✅ ENTERPRISE-GRADE: بازنویسی کامل با Error Handling پیشرفته
 * رعایت SRP: فقط مدیریت تایید نهایی و پرداخت
 */
(function ($) {
    'use strict';

    const ConfirmBooking = {
        init: function () {
            this.initVisitTypeCards();
            this.bindEvents();
        },

        /**
         * خواندن doctorId از فرم (مطمئن‌ترین منبع) یا از data-section
         */
        getDoctorIdAndDate: function () {
            var $section = $('#visitTypeSection');
            var doctorId = parseInt($('#DoctorId').val(), 10);
            if (isNaN(doctorId) || doctorId <= 0) {
                var raw = $section.length ? $section.data('doctor-id') : null;
                doctorId = (raw != null && raw !== '') ? parseInt(raw, 10) : NaN;
            }
            if (isNaN(doctorId) || doctorId <= 0) return { doctorId: null, appointmentDate: null, baseUrl: null };
            var appointmentDate = $section.length ? $section.data('appointment-date') : null;
            var baseUrl = (window.appConfig && window.appConfig.appointmentBooking && window.appConfig.appointmentBooking.getAppointmentPriceUrl) || '/Patient/Api/DoctorSearch/GetAppointmentPrice';
            baseUrl = String(baseUrl).replace(/\/?$/, '');
            if (baseUrl.indexOf('/') !== 0 && baseUrl.indexOf('http') !== 0) baseUrl = '/' + baseUrl;
            return { doctorId: doctorId, appointmentDate: appointmentDate || '', baseUrl: baseUrl };
        },

        /**
         * فراخوانی API قیمت برای یک کارت و به‌روزرسانی متن قیمت
         * @param {jQuery} $card - کارت نوع ویزیت
         * @param {Function} [doneCallback] - بعد از بارگذاری قیمت (در صورت موفقیت)
         */
        fetchPriceForCard: function ($card, doneCallback) {
            var self = this;
            var ctx = this.getDoctorIdAndDate();
            if (!ctx.doctorId || !ctx.baseUrl) {
                if (doneCallback) doneCallback(false);
                return;
            }
            var categoryId = $card.data('service-category-id');
            var $priceEl = $card.find('.visit-type-price-value');
            if (!categoryId) {
                if (doneCallback) doneCallback(false);
                return;
            }
            var url = ctx.baseUrl + '?id=' + encodeURIComponent(ctx.doctorId) + '&serviceCategoryId=' + encodeURIComponent(categoryId);
            if (ctx.appointmentDate) url += '&appointmentDate=' + encodeURIComponent(ctx.appointmentDate);

            $.ajax({
                url: url,
                type: 'GET',
                dataType: 'json'
            }).done(function (res) {
                var ok = res && (res.success === true || res.Success === true);
                var price = res && (res.price != null ? res.price : (res.Price != null ? res.Price : null));
                if (ok && typeof price === 'number' && price >= 0) {
                    $priceEl.data('price', price).text(price.toLocaleString('fa-IR'));
                    if (doneCallback) doneCallback(true);
                } else {
                    $priceEl.text('—');
                    if (doneCallback) doneCallback(false);
                }
            }).fail(function () {
                $priceEl.text('—');
                if (doneCallback) doneCallback(false);
            });
        },

        /**
         * ✅ کارت‌های انتخاب نوع ویزیت: بارگذاری قیمت هر کارت و انتخاب نوع ویزیت
         */
        initVisitTypeCards: function () {
            var self = this;
            var $section = $('#visitTypeSection');
            if (!$section.length) return;

            var ctx = this.getDoctorIdAndDate();
            if (!ctx.doctorId) {
                $('.visit-type-card .visit-type-price-value').text('—');
                return;
            }

            $('.visit-type-card').each(function () {
                var $card = $(this);
                var categoryId = $card.data('service-category-id');
                var $priceEl = $card.find('.visit-type-price-value');

                $card.off('click keydown').on('click', function (e) {
                    e.preventDefault();
                    var price = parseFloat($priceEl.data('price'));
                    if ((!price && price !== 0) || price < 0) {
                        self.fetchPriceForCard($card, function (ok) {
                            if (ok) self.selectVisitTypeCard($card);
                            else self.selectVisitTypeCard($card);
                        });
                    } else {
                        self.selectVisitTypeCard($card);
                    }
                }).on('keydown', function (e) {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        $(this).click();
                    }
                });

                if (!categoryId) {
                    $priceEl.text('—');
                    return;
                }

                self.fetchPriceForCard($card, function (ok) {
                    if (!ok) return;
                    var currentCat = $('#ServiceCategoryId').val();
                    if (currentCat && String(categoryId) === String(currentCat)) {
                        self.selectVisitTypeCard($card);
                    }
                });
            });
        },

        selectVisitTypeCard: function ($card) {
            var categoryId = $card.data('service-category-id');
            var $priceEl = $card.find('.visit-type-price-value');
            var price = parseFloat($priceEl.data('price')) || 0;

            $('.visit-type-card').removeClass('selected').attr('aria-pressed', 'false');
            $card.addClass('selected').attr('aria-pressed', 'true');

            $('#ServiceCategoryId').val(categoryId || '');
            $('#Price').val(price);

            var $summary = $('#summaryPriceDisplay');
            if ($summary.length) {
                $summary.text(price > 0 ? price.toLocaleString('fa-IR') + ' تومان' : '— تومان');
            }

            var $btn = $('#confirmBookingBtn');
            if ($btn.length) {
                $btn.prop('disabled', !categoryId || price <= 0).attr('aria-disabled', !categoryId || price <= 0);
            }
        },

        bindEvents: function () {
            const self = this;
            $('.payment-method-card').on('click', this.handlePaymentMethodSelection.bind(this));
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

        /**
         * ✅ ENTERPRISE-GRADE: Submit Booking با Error Handling پیشرفته
         * Flow:
         * 1. Reserve Appointment (Status = Pending)
         * 2. Process Payment (Redirect to Gateway)
         * 3. Payment Callback (Status = Scheduled)
         */
        submitBooking: function (formData) {
            showLoading();

            const reserveUrl = window.appConfig?.appointmentBooking?.reserveUrl || '/Patient/Appointment/Book/Reserve';
            
            // ✅ CRITICAL: Ensure idempotency key is included
            const idempotencyKey = $('#idempotencyKey').val();
            if (idempotencyKey) {
                formData += `&idempotencyKey=${encodeURIComponent(idempotencyKey)}`;
            }
            
            // ✅ PRODUCTION: ارسال کوکی احراز هویت با درخواست (withCredentials) برای جلوگیری از requiresLogin کاذب
            this.ajaxWithRetry({
                url: reserveUrl,
                type: 'POST',
                data: formData,
                timeout: 60000,
                maxRetries: 1, // فقط 1 بار Retry (Idempotency)
                retryDelay: 2000,
                xhrFields: { withCredentials: true },
                onSuccess: async (response) => {
                    // ✅ CRITICAL: Wrap در try-catch برای catch کردن exceptions از processPayment
                    try {
                        hideLoading();
                        
                        // ✅ ENTERPRISE-GRADE: Logging کامل
                        console.log('✅ [ConfirmBooking] Reserve response:', response);
                        console.log('🔍 [ConfirmBooking] Details - success:', response?.success, 
                                   'requiresPayment:', response?.requiresPayment, 
                                   'appointmentId:', response?.appointmentId,
                                   'message:', response?.message);
                        
                        // ✅ CRITICAL: بررسی دقیق response
                        if (!response) {
                            console.error('❌ [ConfirmBooking] Response is null or undefined');
                            this.showError('خطا در دریافت پاسخ از سرور. لطفاً دوباره تلاش کنید.');
                            return;
                        }

                        // ✅ CRITICAL: بررسی success flag (با پشتیبانی از string و boolean)
                        // ⚠️ NOTE: بعضی سرورها success را به صورت string "true" برمی‌گردانند
                        const isSuccess = response.success === true || response.success === 'true' || response.success === 1;
                        
                        if (!isSuccess) {
                            // ❌ Reserve failed
                            console.error('❌ [ConfirmBooking] Reserve failed - Message:', response.message, 'Response:', response);

                            // ✅ PRODUCTION: در صورت نیاز به لاگین، هدایت به صفحه ورود با returnUrl
                            if (response.requiresLogin === true) {
                                const returnUrl = encodeURIComponent(window.location.href);
                                const loginUrl = '/Account/Login?returnUrl=' + returnUrl;
                                await Swal.fire({
                                    title: 'ورود به سیستم',
                                    text: response.message || 'لطفاً ابتدا وارد سیستم شوید.',
                                    icon: 'info',
                                    confirmButtonText: 'ورود',
                                    confirmButtonColor: '#2c5aa0'
                                }).then(function () {
                                    window.location.href = loginUrl;
                                });
                                return;
                            }

                            // ✅ نمایش warnings جداگانه (اگر وجود دارد)
                            if (response.warnings && response.warnings.length > 0) {
                                const warningsText = response.warnings.join('\n');
                                await Swal.fire({
                                    title: 'هشدار',
                                    html: `<p>${warningsText}</p>`,
                                    icon: 'warning',
                                    confirmButtonText: 'باشه',
                                    confirmButtonColor: '#2c5aa0'
                                });
                            }

                            // ✅ نمایش خطا
                            const errorMessage = response.message || 'خطا در رزرو نوبت';
                            this.showError(errorMessage);
                            return;
                        }

                        // ✅ Reserve successful
                        console.log('✅ [ConfirmBooking] Reserve successful - AppointmentId:', response.appointmentId, 
                                   'RequiresPayment:', response.requiresPayment);
                        
                        // ✅ CRITICAL: اگر نیاز به پرداخت دارد
                        if (response.requiresPayment === true && response.appointmentId) {
                            console.log('💰 [ConfirmBooking] Starting payment process - AppointmentId:', response.appointmentId);
                            
                            try {
                                // ✅ ENTERPRISE-GRADE: پرداخت را انجام بده
                                await this.processPayment(response.appointmentId);
                                
                                // ✅ اگر processPayment موفق بود، redirect انجام می‌شود
                                // پس این خط اجرا نمی‌شود
                                console.log('✅ [ConfirmBooking] Payment process completed successfully');
                                
                            } catch (paymentError) {
                                // ❌ خطا در پردازش پرداخت
                                const paymentMsg = paymentError && (paymentError.message || paymentError.responseJSON?.message) || 'نامشخص';
                                console.error('❌ [ConfirmBooking] Payment process failed:', paymentError);
                                console.error('❌ [ConfirmBooking] Payment error details:', {
                                    message: paymentError.message,
                                    stack: paymentError.stack,
                                    response: paymentError.responseJSON
                                });
                                
                                // ✅ CRITICAL: نوبت با موفقیت رزرو شده است (Status = Pending)
                                // فقط پرداخت خطا دارد — دلیل خطا را نمایش بده تا کاربر/توسعه‌دهنده بداند چرا به درگاه وصل نشد
                                await Swal.fire({
                                    title: 'خطا در پردازش پرداخت',
                                    html: `
                                        <p>نوبت شما با موفقیت ثبت شد و در انتظار پرداخت است.</p>
                                        <p class="mt-2"><strong>شناسه نوبت: ${response.appointmentId}</strong></p>
                                        <p class="mt-2 text-danger"><strong>دلیل:</strong> ${paymentMsg}</p>
                                        <p class="mt-2">لطفاً بعداً از بخش "نوبت‌های من" برای پرداخت اقدام کنید یا دلیل بالا را برطرف کنید.</p>
                                    `,
                                    icon: 'warning',
                                    confirmButtonText: 'باشه',
                                    confirmButtonColor: '#2c5aa0'
                                });
                                
                                // ✅ CRITICAL FIX: هدایت به صفحه SelectTime به جای MyAppointments
                                // چون MyAppointments نیاز به احراز هویت دارد
                                // اما SelectTime با AllowAnonymous است
                                const doctorId = $('input[name="DoctorId"]').val() || $('#doctorId').val() || '';
                                const appointmentDate = $('input[name="AppointmentDate"]').val() || $('#selectedDate').val() || '';
                                
                                console.log('🔍 [ConfirmBooking] Redirect info - DoctorId:', doctorId, 'AppointmentDate:', appointmentDate);
                                
                                if (doctorId && appointmentDate) {
                                    const formattedDate = appointmentDate.includes('/') ? appointmentDate : appointmentDate;
                                    const baseUrl = window.appConfig?.appointmentBooking?.selectTimeBaseUrl || '/Patient/Appointment/Book/SelectTime';
                                    window.location.href = baseUrl + '?doctorId=' + doctorId + '&date=' + encodeURIComponent(formattedDate);
                                } else {
                                    const selectDoctorUrl = window.appConfig?.appointmentBooking?.selectDoctorUrl || '/Patient/Appointment/Book/SelectDoctor';
                                    console.warn('⚠️ [ConfirmBooking] DoctorId or AppointmentDate not found, redirecting to SelectDoctor');
                                    window.location.href = selectDoctorUrl;
                                }
                            }
                            
                        } else {
                            // ✅ نیازی به پرداخت نیست
                            console.log('✅ [ConfirmBooking] No payment required');
                            this.showSuccess(response);
                        }
                        
                    } catch (error) {
                        // ❌ خطای غیرمنتظره در onSuccess
                        hideLoading();
                        console.error('❌ [ConfirmBooking] Unexpected error in onSuccess:', error);
                        console.error('❌ [ConfirmBooking] Error stack:', error.stack);
                        console.error('❌ [ConfirmBooking] Response was:', response);
                        
                        // ✅ CRITICAL: بررسی اینکه آیا Reserve موفق بود یا نه (با پشتیبانی از string و boolean)
                        const isSuccess = response && (response.success === true || response.success === 'true' || response.success === 1);
                        const requiresPayment = response && (response.requiresPayment === true || response.requiresPayment === 'true' || response.requiresPayment === 1);
                        
                        if (isSuccess && requiresPayment) {
                            const paymentMsg = error && error.message || (error && error.responseJSON && error.responseJSON.message) || 'نامشخص';
                            // ✅ Reserve موفق بود، فقط پرداخت خطا دارد
                            await Swal.fire({
                                title: 'خطا در پردازش پرداخت',
                                html: `
                                    <p>نوبت شما با موفقیت ثبت شد و در انتظار پرداخت است.</p>
                                    <p class="mt-2"><strong>شناسه نوبت: ${response.appointmentId}</strong></p>
                                    <p class="mt-2 text-danger"><strong>دلیل:</strong> ${paymentMsg}</p>
                                    <p class="mt-2">لطفاً بعداً از بخش "نوبت‌های من" برای پرداخت اقدام کنید یا دلیل بالا را برطرف کنید.</p>
                                `,
                                icon: 'warning',
                                confirmButtonText: 'باشه',
                                confirmButtonColor: '#2c5aa0'
                            });
                            
                            // ✅ CRITICAL FIX: هدایت به صفحه SelectTime به جای MyAppointments
                            // چون MyAppointments نیاز به احراز هویت دارد
                            // اما SelectTime با AllowAnonymous است
                            const doctorId = $('input[name="DoctorId"]').val() || $('#doctorId').val() || '';
                            const appointmentDate = $('input[name="AppointmentDate"]').val() || $('#selectedDate').val() || '';
                            
                            console.log('🔍 [ConfirmBooking] Redirect info (catch block) - DoctorId:', doctorId, 'AppointmentDate:', appointmentDate);
                            
                            if (doctorId && appointmentDate) {
                                const formattedDate = appointmentDate.includes('/') ? appointmentDate : appointmentDate;
                                const baseUrl = window.appConfig?.appointmentBooking?.selectTimeBaseUrl || '/Patient/Appointment/Book/SelectTime';
                                window.location.href = baseUrl + '?doctorId=' + doctorId + '&date=' + encodeURIComponent(formattedDate);
                            } else {
                                const selectDoctorUrl = window.appConfig?.appointmentBooking?.selectDoctorUrl || '/Patient/Appointment/Book/SelectDoctor';
                                console.warn('⚠️ [ConfirmBooking] DoctorId or AppointmentDate not found, redirecting to SelectDoctor');
                                window.location.href = selectDoctorUrl;
                            }
                        } else {
                            // ❌ Reserve ناموفق بود
                            this.showError('خطا در پردازش درخواست. لطفاً دوباره تلاش کنید.');
                        }
                    }
                },
                onError: (xhr, status, error) => {
                    hideLoading();
                    
                    // ✅ ENTERPRISE-GRADE: تشخیص نوع خطا
                    let errorMessage = 'خطا در ارتباط با سرور';
                    
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
                    
                    console.error('❌ [ConfirmBooking] AJAX Error:', { status, error, xhr, responseJSON: xhr.responseJSON });
                    this.showError(errorMessage);
                }
            });
        },

        /**
         * ✅ ENTERPRISE-GRADE: Process Payment با Error Handling پیشرفته
         * Flow:
         * 1. ارسال درخواست پرداخت به ProcessPayment action
         * 2. دریافت paymentUrl از درگاه
         * 3. هدایت به درگاه پرداخت
         */
        processPayment: async function (appointmentId) {
            console.log('💰 [ConfirmBooking] processPayment called - AppointmentId:', appointmentId);
            
            if (!appointmentId) {
                throw new Error('شناسه نوبت نامعتبر است');
            }
            
            showLoading();

            try {
                // ✅ CRITICAL: بررسی AntiForgeryToken
                const token = $('input[name="__RequestVerificationToken"]').val();
                if (!token) {
                    console.error('❌ [ConfirmBooking] AntiForgeryToken not found');
                    hideLoading();
                    throw new Error('خطا در دریافت توکن امنیتی. لطفاً صفحه را نوسازی کنید.');
                }

                console.log('💰 [ConfirmBooking] Sending payment request - AppointmentId:', appointmentId);
                
                // ✅ ENTERPRISE-GRADE: AJAX Call با Error Handling
                const processPaymentUrl = window.appConfig?.appointmentBooking?.processPaymentUrl || '/Patient/Appointment/Book/ProcessPayment';
                const response = await $.ajax({
                    url: processPaymentUrl,
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
                
                // ✅ ENTERPRISE-GRADE: Logging کامل
                console.log('✅ [ConfirmBooking] Payment response received:', response);
                console.log('🔍 [ConfirmBooking] Payment details - success:', response?.success, 
                           'paymentUrl:', response?.paymentUrl,
                           'message:', response?.message);

                // ✅ CRITICAL: بررسی response
                if (!response) {
                    throw new Error('خطا در دریافت پاسخ از سرور');
                }

                // ✅ CRITICAL: بررسی success flag (با پشتیبانی از string و boolean)
                const isSuccess = response.success === true || response.success === 'true' || response.success === 1;
                
                if (!isSuccess) {
                    // ❌ Payment request failed
                    const errorMessage = response.message || 'خطا در ایجاد درخواست پرداخت';
                    console.error('❌ [ConfirmBooking] Payment request failed - Message:', errorMessage, 'Response:', response);
                    console.error('❌ [ConfirmBooking] Success value:', response.success, 'Type:', typeof response.success);
                    throw new Error(errorMessage);
                }

                if (!response.paymentUrl) {
                    // ❌ PaymentUrl موجود نیست
                    console.error('❌ [ConfirmBooking] PaymentUrl is missing in response:', response);
                    throw new Error('خطا در دریافت آدرس درگاه پرداخت');
                }

                // ✅ Payment successful - Redirect to gateway
                console.log('🔄 [ConfirmBooking] Redirecting to payment gateway:', response.paymentUrl);
                
                // ✅ نمایش پیام هدایت
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

                // ✅ هدایت به درگاه پس از 1 ثانیه
                setTimeout(() => {
                    window.location.href = response.paymentUrl;
                }, 1000);
                
            } catch (error) {
                hideLoading();
                
                // ✅ ENTERPRISE-GRADE: Error Handling پیشرفته
                console.error('❌ [ConfirmBooking] Error in processPayment:', error);
                console.error('❌ [ConfirmBooking] Error details:', {
                    message: error.message,
                    status: error.status,
                    statusText: error.statusText,
                    responseJSON: error.responseJSON,
                    stack: error.stack
                });
                
                // ✅ تشخیص نوع خطا
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
                
                // ✅ CRITICAL: Throw exception برای catch block در submitBooking
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
                confirmButtonColor: '#2c5aa0'
            }).then(() => {
                const myAppointmentsUrl = window.appConfig?.appointmentBooking?.myAppointmentsUrl || '/Patient/Appointment/MyAppointments';
                window.location.href = myAppointmentsUrl;
            });
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
            } else {
                alert('خطا: ' + message);
            }
        },

        /**
         * ✅ ENTERPRISE-GRADE: AJAX با Retry Logic و JSON Parsing
         * CRITICAL FIX: اضافه کردن dataType: 'json' و parse کردن response اگر string باشد
         */
        ajaxWithRetry: function (options) {
            const self = this;
            let retryCount = 0;
            const maxRetries = options.maxRetries || 0;
            const retryDelay = options.retryDelay || 1000;

            function attempt() {
                $.ajax({
                    url: options.url,
                    type: options.type || 'POST',
                    data: options.data,
                    dataType: 'json', // ✅ CRITICAL FIX: Explicitly set dataType to JSON
                    timeout: options.timeout || 30000,
                    headers: options.headers || {},
                    xhrFields: options.xhrFields || {}, // ✅ PRODUCTION: ارسال کوکی (withCredentials) برای Reserve
                    success: function (response) {
                        // ✅ CRITICAL FIX: Ensure response is parsed correctly
                        // اگر response به صورت string برگردد، parse می‌کنیم
                        if (typeof response === 'string') {
                            try {
                                response = JSON.parse(response);
                                console.log('✅ [ConfirmBooking] Parsed JSON response:', response);
                            } catch (e) {
                                console.error('❌ [ConfirmBooking] Failed to parse JSON response:', e);
                                console.error('❌ [ConfirmBooking] Raw response:', response);
                            }
                        }
                        
                        // ✅ CRITICAL FIX: Log response برای debugging
                        console.log('✅ [ConfirmBooking] AJAX Success - Response type:', typeof response, 'Response:', response);
                        
                        if (options.onSuccess) {
                            options.onSuccess(response);
                        }
                    },
                    error: function (xhr, status, error) {
                        // ✅ Retry logic
                        if (retryCount < maxRetries && (status === 'timeout' || xhr.status >= 500)) {
                            retryCount++;
                            console.warn(`⚠️ [ConfirmBooking] Retry attempt ${retryCount}/${maxRetries} after ${retryDelay}ms`);
                            setTimeout(attempt, retryDelay);
                        } else {
                            if (options.onError) {
                                options.onError(xhr, status, error);
                            } else {
                                self.showError('خطا در ارتباط با سرور. لطفاً دوباره تلاش کنید.');
                            }
                        }
                    }
                });
            }

            attempt();
        }
    };

    // Initialize on document ready
    $(document).ready(function () {
        ConfirmBooking.init();
    });

    // Export for global access (if needed)
    window.ConfirmBooking = ConfirmBooking;

})(jQuery);
