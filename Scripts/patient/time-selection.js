/**
 * JavaScript Module برای انتخاب زمان
 * رعایت SRP: فقط مدیریت انتخاب اسلات زمانی و Real-time updates
 */
(function ($) {
    'use strict';

    const TimeSelection = {
        doctorId: null,
        selectedDate: null,
        selectedSlot: null,
        updateInterval: null,

        init: function () {
            this.doctorId = $('#doctorId').val();
            this.selectedDate = $('#selectedDate').val();
            
            if (!this.doctorId || !this.selectedDate) {
                this.showError('اطلاعات ناقص است');
                return;
            }

            this.bindEvents();
            this.startRealTimeUpdates();
            this.restoreSelection(); // ✅ CRITICAL FIX: Restore selection from sessionStorage
        },

        bindEvents: function () {
            // انتخاب اسلات
            $(document).on('click', '.select-slot-btn', this.handleSelectSlot.bind(this));
            
            // پاک کردن انتخاب
            $('#clearSelectionBtn').on('click', this.handleClearSelection.bind(this));
            
            // ادامه به تایید (Desktop + Mobile)
            $('#continueToConfirmBtn, #continueToConfirmBtnMobile').on('click', this.handleContinue.bind(this));
        },

        handleSelectSlot: function (e) {
            e.preventDefault();
            const $card = $(e.currentTarget).closest('.time-slot-card');
            
            if (!$card.hasClass('available')) {
                return;
            }

            // حذف انتخاب قبلی
            $('.time-slot-card').removeClass('selected');
            
            // انتخاب جدید
            $card.addClass('selected');
            
            const startTime = $card.data('start-time');
            const endTime = $card.data('end-time');
            
            this.selectedSlot = {
                startTime: startTime,
                endTime: endTime,
                displayTime: $card.find('.slot-time strong').text()
            };

            // ✅ CRITICAL FIX: Save to sessionStorage for state management
            try {
                sessionStorage.setItem(
                    `timeSelection_${this.doctorId}_${this.selectedDate}`,
                    JSON.stringify(this.selectedSlot)
                );
            } catch (e) {
                console.warn('⚠️ [TimeSelection] Failed to save selection to sessionStorage:', e);
            }

            // نمایش اطلاعات انتخاب شده
            this.showSelectedSlotInfo();
            
            // ✅ CRITICAL FIX: فعال کردن دکمه ادامه (Desktop + Mobile)
            $('#continueToConfirmBtn, #continueToConfirmBtnMobile').prop('disabled', false);
        },

        handleClearSelection: function () {
            $('.time-slot-card').removeClass('selected');
            this.selectedSlot = null;
            $('#selectedSlotInfo').removeClass('show');
            $('#continueToConfirmBtn, #continueToConfirmBtnMobile').prop('disabled', true);
            
            // ✅ CRITICAL FIX: Clear sticky bottom bar
            $('#stickySelectedTime').hide();
            
            // ✅ CRITICAL FIX: Clear from sessionStorage
            try {
                sessionStorage.removeItem(`timeSelection_${this.doctorId}_${this.selectedDate}`);
            } catch (e) {
                console.warn('⚠️ [TimeSelection] Failed to clear selection from sessionStorage:', e);
            }
        },

        handleContinue: function () {
            if (!this.selectedSlot) {
                this.showError('لطفاً زمان را انتخاب کنید');
                return;
            }

            // ✅ CRITICAL FIX: Prevent double-click/duplicate requests
            const $btn = $('#continueToConfirmBtn, #continueToConfirmBtnMobile');
            if ($btn.prop('disabled') || $btn.data('processing')) {
                return; // Already processing
            }
            
            $btn.prop('disabled', true).data('processing', true);

            // بررسی مجدد دسترسی‌پذیری
            this.checkSlotAvailability();
        },

        checkSlotAvailability: function () {
            showLoading();

            // ✅ CRITICAL FIX: استفاده از Route system به جای Hardcode URL
            const checkUrl = window.appConfig?.appointmentBooking?.checkSlotAvailabilityUrl || '/Patient/Api/DoctorSearch/CheckSlotAvailability';
            
            // ✅ CRITICAL FIX: بهبود Error Handling با Retry Logic و Timeout
            // ✅ Note: CSRF Token حذف شد - این یک Read Operation است و AllowAnonymous است
            this.ajaxWithRetry({
                url: checkUrl,
                type: 'POST',
                data: {
                    doctorId: this.doctorId,
                    appointmentDate: this.selectedDate,
                    startTime: this.selectedSlot.startTime,
                    endTime: this.selectedSlot.endTime
                },
                // ✅ CRITICAL FIX: حذف CSRF Token Header - ValidateAntiForgeryToken حذف شد
                // این یک Read Operation است و برای Anonymous users مشکل ایجاد می‌کرد
                timeout: 30000, // ✅ 30 ثانیه Timeout
                maxRetries: 3, // ✅ حداکثر 3 بار تلاش
                retryDelay: 1000, // ✅ 1 ثانیه تاخیر بین تلاش‌ها
                onSuccess: (response) => {
                    hideLoading();
                    // ✅ CRITICAL FIX: Re-enable button after request
                    $('#continueToConfirmBtn, #continueToConfirmBtnMobile').prop('disabled', false).data('processing', false);
                    
                    // ✅ CRITICAL FIX: Debug logging
                    console.log('🔍 [TimeSelection] CheckSlotAvailability response:', response);
                    console.log('🔍 [TimeSelection] response.success:', response?.success, 'response.isAvailable:', response?.isAvailable);
                    
                    // ✅ CRITICAL FIX: Handle both response formats (direct or nested)
                    const isAvailable = response?.isAvailable === true || response?.data?.isAvailable === true;
                    const isSuccess = response?.success === true;
                    
                    if (isSuccess && isAvailable) {
                        this.proceedToConfirm();
                    } else {
                        console.warn('⚠️ [TimeSelection] Slot not available - success:', isSuccess, 'isAvailable:', isAvailable);
                        this.showError('این زمان دیگر در دسترس نیست. لطفاً زمان دیگری انتخاب کنید');
                        this.updateSlotAvailability();
                    }
                },
                onError: (xhr, status, error) => {
                    hideLoading();
                    // ✅ CRITICAL FIX: Re-enable button on error
                    $('#continueToConfirmBtn, #continueToConfirmBtnMobile').prop('disabled', false).data('processing', false);
                    
                    let errorMessage = 'خطا در بررسی دسترسی‌پذیری';
                    
                    // ✅ تشخیص نوع خطا و نمایش پیام مناسب
                    if (status === 'timeout') {
                        errorMessage = 'زمان اتصال به سرور به پایان رسید. لطفاً اتصال اینترنت خود را بررسی کنید و دوباره تلاش کنید.';
                    } else if (status === 'error' && xhr.status === 0) {
                        errorMessage = 'خطا در اتصال به سرور. لطفاً اتصال اینترنت خود را بررسی کنید.';
                    } else if (xhr.status >= 500) {
                        errorMessage = 'خطای سرور. لطفاً چند لحظه صبر کنید و دوباره تلاش کنید.';
                    } else if (xhr.status === 404) {
                        errorMessage = 'صفحه مورد نظر یافت نشد. لطفاً صفحه را رفرش کنید.';
                    }
                    
                    this.showError(errorMessage);
                    console.error('❌ [TimeSelection] AJAX Error:', { status, error, xhr });
                }
            });
        },

        proceedToConfirm: function () {
            // ✅ CRITICAL FIX: استفاده از Route system به جای Hardcode URL
            const confirmUrl = window.appConfig?.appointmentBooking?.confirmBookingUrl || '/Patient/AppointmentBooking/ConfirmBooking';
            
            // ✅ CRITICAL FIX: Log برای دیباگ
            console.log('🔍 [TimeSelection] proceedToConfirm - doctorId:', this.doctorId, 'selectedDate:', this.selectedDate, 'startTime:', this.selectedSlot.startTime, 'endTime:', this.selectedSlot.endTime);
            
            const params = new URLSearchParams({
                doctorId: this.doctorId,
                appointmentDate: this.selectedDate, // ✅ Format: yyyy-MM-dd (Gregorian)
                startTime: this.selectedSlot.startTime, // ✅ Format: hh:mm
                endTime: this.selectedSlot.endTime // ✅ Format: hh:mm
            });

            const fullUrl = `${confirmUrl}?${params.toString()}`;
            console.log('🔍 [TimeSelection] Navigating to:', fullUrl);
            window.location.href = fullUrl;
        },

        showSelectedSlotInfo: function () {
            $('#selectedTimeDisplay').text(this.selectedSlot.displayTime);
            $('#selectedSlotInfo').addClass('show');
            $('#selectedStartTime').val(this.selectedSlot.startTime);
            $('#selectedEndTime').val(this.selectedSlot.endTime);
            
            // ✅ CRITICAL FIX: Update sticky bottom bar for mobile
            $('#stickyTimeDisplay').text(this.selectedSlot.displayTime);
            $('#stickySelectedTime').show();
            $('#continueToConfirmBtnMobile').prop('disabled', false);
        },

        startRealTimeUpdates: function () {
            // ✅ CRITICAL FIX: به‌روزرسانی Real-time هر 15 ثانیه (بهینه‌سازی تعادل Performance/Accuracy)
            this.updateInterval = setInterval(() => {
                this.updateSlotAvailability();
            }, 15000); // 15 seconds - Better balance between performance and data accuracy
        },

        updateSlotAvailability: function () {
            // ✅ CRITICAL FIX: استفاده از Route system به جای Hardcode URL
            const slotsUrl = window.appConfig?.appointmentBooking?.getAvailableSlotsUrl || '/Patient/Api/DoctorSearch/GetAvailableTimeSlots';
            
            // ✅ CRITICAL FIX: بهبود Error Handling برای Real-time Updates
            this.ajaxWithRetry({
                url: slotsUrl,
                type: 'GET',
                data: {
                    id: this.doctorId,
                    date: this.selectedDate
                },
                timeout: 20000, // ✅ 20 ثانیه Timeout برای Real-time updates
                maxRetries: 2, // ✅ کمتر Retry برای Real-time (Silent fail)
                retryDelay: 2000, // ✅ 2 ثانیه تاخیر
                onSuccess: (response) => {
                    if (response.success && response.data) {
                        this.updateSlotsUI(response.data);
                    }
                },
                onError: (xhr, status, error) => {
                    // ✅ Silent fail برای Real-time updates (طبق طراحی)
                    // اما Log برای Debugging
                    console.error('❌ [TimeSelection] Real-time update failed:', { status, error });
                    // ✅ اگر خطا Network است، ممکن است نیاز به توقف Real-time updates باشد
                    if (status === 'timeout' || (status === 'error' && xhr.status === 0)) {
                        console.warn('⚠️ [TimeSelection] Network error detected. Consider stopping real-time updates.');
                    }
                },
                silentFail: true // ✅ Silent fail برای Real-time updates
            });
        },

        updateSlotsUI: function (slots) {
            slots.forEach(slot => {
                const $card = $(`.time-slot-card[data-start-time="${slot.startTime}"]`);
                if ($card.length) {
                    if (!slot.isAvailable) {
                        $card.removeClass('available').addClass('unavailable');
                        $card.find('.select-slot-btn').prop('disabled', true)
                            .removeClass('btn-primary').addClass('btn-secondary')
                            .html('<i class="fas fa-times-circle me-1"></i> غیرقابل رزرو');
                    }
                }
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
            const silentFail = options.silentFail || false;

            function makeRequest() {
                $.ajax({
                    url: options.url,
                    type: options.type || 'GET',
                    data: options.data || {},
                    headers: options.headers || {},
                    dataType: 'json', // ✅ CRITICAL FIX: Explicitly set dataType to JSON
                    timeout: timeout,
                    success: function (response) {
                        // ✅ CRITICAL FIX: Ensure response is parsed correctly
                        if (typeof response === 'string') {
                            try {
                                response = JSON.parse(response);
                            } catch (e) {
                                console.error('❌ [TimeSelection] Failed to parse JSON response:', e);
                            }
                        }
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

                        // ✅ Retry Logic برای Network Errors و Server Errors
                        if (retryCount < maxRetries && (isNetworkError || isServerError)) {
                            retryCount++;
                            console.warn(`⚠️ [TimeSelection] Retry attempt ${retryCount}/${maxRetries} for ${options.url}`);
                            
                            // ✅ Exponential Backoff
                            const delay = retryDelay * Math.pow(2, retryCount - 1);
                            
                            setTimeout(function () {
                                makeRequest();
                            }, delay);
                        } else {
                            // ✅ تمام تلاش‌ها انجام شد یا خطای Client Error
                            if (options.onError) {
                                options.onError(xhr, status, error);
                            } else if (!silentFail) {
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
        },

        /**
         * ✅ CRITICAL FIX: Restore selection from sessionStorage
         * برای حفظ state در صورت refresh/back
         */
        restoreSelection: function () {
            try {
                const saved = sessionStorage.getItem(`timeSelection_${this.doctorId}_${this.selectedDate}`);
                if (saved) {
                    const slot = JSON.parse(saved);
                    const $card = $(`.time-slot-card[data-start-time="${slot.startTime}"]`);
                    if ($card.length && $card.hasClass('available')) {
                        // ✅ Restore selection
                        $('.time-slot-card').removeClass('selected');
                        $card.addClass('selected');
                        this.selectedSlot = slot;
                        this.showSelectedSlotInfo();
                        $('#continueToConfirmBtn, #continueToConfirmBtnMobile').prop('disabled', false);
                        console.log('✅ [TimeSelection] Selection restored from sessionStorage');
                    } else {
                        // ✅ Slot no longer available - clear from storage
                        sessionStorage.removeItem(`timeSelection_${this.doctorId}_${this.selectedDate}`);
                    }
                }
            } catch (e) {
                console.warn('⚠️ [TimeSelection] Failed to restore selection from sessionStorage:', e);
            }
        },

        destroy: function () {
            if (this.updateInterval) {
                clearInterval(this.updateInterval);
            }
        }
    };

    // Initialize on document ready
    $(document).ready(function () {
        TimeSelection.init();
    });

    // Cleanup on page unload
    $(window).on('beforeunload', function () {
        TimeSelection.destroy();
    });

    // Export for global access
    window.TimeSelection = TimeSelection;

})(jQuery);

