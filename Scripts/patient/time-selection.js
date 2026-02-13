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
            // ✅ ENTERPRISE-GRADE: کلیک روی کارت (نه فقط دکمه) باعث انتخاب می‌شود
            $(document).on('click', '.time-slot-premium.slot-available, .time-slot-card-minimal.slot-available, .time-slot-card.available', this.handleCardClick.bind(this));
            
            // انتخاب اسلات از طریق دکمه (برای سازگاری)
            $(document).on('click', '.btn-slot-premium, .btn-slot-select, .select-slot-btn', this.handleSelectSlot.bind(this));
            
            // پاک کردن انتخاب
            $('#clearSelectionBtn').on('click', this.handleClearSelection.bind(this));
            
            // ادامه به تایید (Desktop + Mobile)
            $('#continueToConfirmBtn, #continueToConfirmBtnMobile').on('click', this.handleContinue.bind(this));
        },

        handleCardClick: function (e) {
            e.preventDefault();
            e.stopPropagation();
            
            // ✅ جلوگیری از انتخاب اگر روی دکمه کلیک شده است
            if ($(e.target).closest('.btn-slot-premium, .btn-slot-select, .select-slot-btn').length > 0) {
                return;
            }
            
            const $card = $(e.currentTarget);
            this.selectSlot($card);
        },

        handleSelectSlot: function (e) {
            e.preventDefault();
            e.stopPropagation();
            const $card = $(e.currentTarget).closest('.time-slot-premium, .time-slot-card-minimal, .time-slot-card');
            this.selectSlot($card);
        },

        selectSlot: function ($card) {
            if (!$card.hasClass('slot-available') && !$card.hasClass('available')) {
                return;
            }

            // ✅ ENTERPRISE-GRADE: حذف انتخاب قبلی با انیمیشن
            $('.time-slot-premium, .time-slot-card-minimal, .time-slot-card').removeClass('selected');
            
            // ✅ ENTERPRISE-GRADE: انتخاب جدید با انیمیشن
            $card.addClass('selected');
            
            // ✅ انیمیشن انتخاب
            $card.css('transform', 'scale(0.98)');
            setTimeout(() => {
                $card.css('transform', '');
            }, 150);
            
            const startTime = $card.data('start-time');
            const endTime = $card.data('end-time');
            
            // ✅ استخراج زمان نمایش از المان‌های مختلف
            let displayTime = $card.find('.slot-time-display').text() || 
                             $card.find('.slot-time-main').text() || 
                             $card.find('.slot-time strong').text() || 
                             $card.find('.slot-time-text').text() ||
                             startTime;
            
            this.selectedSlot = {
                startTime: startTime,
                endTime: endTime,
                displayTime: displayTime.trim()
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
            $('.time-slot-premium, .time-slot-card-minimal, .time-slot-card').removeClass('selected');
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
            this._showLoading();

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
                    this._hideLoading();
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
                    this._hideLoading();
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
            if (!this.selectedSlot) {
                $('#selectedSlotInfo').removeClass('show');
                $('#stickySelectedTime').hide();
                return;
            }

            const timeText = this.selectedSlot.displayTime || 
                           `${this.selectedSlot.startTime} - ${this.selectedSlot.endTime}`;
            
            // ✅ ENTERPRISE-GRADE: نمایش زمان انتخاب شده با انیمیشن
            $('#selectedTimeDisplay').text(timeText).hide().fadeIn(300);
            $('#stickyTimeDisplay').text(timeText).hide().fadeIn(300);
            
            // ✅ انیمیشن نمایش بخش انتخاب شده
            const $info = $('#selectedSlotInfo');
            if (!$info.hasClass('show')) {
                $info.addClass('show').hide().slideDown(300);
            }
            
            $('#stickySelectedTime').show().hide().fadeIn(300);
            
            // ✅ CRITICAL FIX: Set hidden fields for form submission
            $('#selectedStartTime').val(this.selectedSlot.startTime);
            $('#selectedEndTime').val(this.selectedSlot.endTime);
            
            // ✅ CRITICAL FIX: Update sticky bottom bar for mobile
            $('#continueToConfirmBtnMobile').prop('disabled', false);
        },

        startRealTimeUpdates: function () {
            // ✅ CRITICAL FIX: به‌روزرسانی Real-time هر 15 ثانیه (بهینه‌سازی تعادل Performance/Accuracy)
            this.updateInterval = setInterval(() => {
                this.updateSlotAvailability();
            }, 15000); // 15 seconds - Better balance between performance and data accuracy
        },

        _showLoading: function () {
            if (typeof showLoading === 'function') showLoading();
            else $('#loadingState').length && $('#loadingState').show();
        },
        _hideLoading: function () {
            if (typeof hideLoading === 'function') hideLoading();
            else $('#loadingState').length && $('#loadingState').hide();
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

        /** ✅ نرمال‌سازی زمان به فرمت hh:mm برای تطابق با data-start-time در کارت (سرور با @"hh\:mm" رندر می‌کند) */
        _normalizeTime: function (t) {
            if (t == null) return '';
            const s = String(t).trim();
            if (s.length >= 5) return s.substring(0, 5); // "09:30" or "09:30:00" -> "09:30"
            return s;
        },

        updateSlotsUI: function (slots) {
            const self = this;
            (slots || []).forEach(function (slot) {
                const startTime = self._normalizeTime(slot.startTime || slot.StartTime);
                const isAvailable = slot.isAvailable !== undefined ? slot.isAvailable : slot.IsAvailable;
                if (!startTime) return;
                const $card = $(`.time-slot-premium[data-start-time="${startTime}"], .time-slot-card-minimal[data-start-time="${startTime}"], .time-slot-card[data-start-time="${startTime}"]`);
                if ($card.length) {
                    if (!isAvailable) {
                        $card.removeClass('slot-available available').addClass('slot-booked unavailable');
                        const $btn = $card.find('.btn-slot-premium, .btn-slot-select, .select-slot-btn');
                        if ($btn.length) {
                            $btn.prop('disabled', true)
                                .removeClass('btn-medical-primary btn-primary')
                                .addClass('btn-secondary')
                                .html('رزرو شده');
                        }
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
                    const $card = $(`.time-slot-premium[data-start-time="${slot.startTime}"], .time-slot-card-minimal[data-start-time="${slot.startTime}"], .time-slot-card[data-start-time="${slot.startTime}"]`);
                    if ($card.length && ($card.hasClass('slot-available') || $card.hasClass('available'))) {
                        // ✅ Restore selection
                        $('.time-slot-premium, .time-slot-card-minimal, .time-slot-card').removeClass('selected');
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

