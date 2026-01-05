/**
 * ✅ ULTIMATE: Date Selection Module for Appointment Booking
 * 
 * Features:
 * - Persian DatePicker integration
 * - Date validation (past dates, future dates)
 * - Gregorian/Persian date conversion
 * - Real-time UI feedback
 * - Error handling
 * 
 * طبق: APPOINTMENT_BOOKING_VIEWS_REVIEW.md - Issue #2
 * 
 * Usage:
 * 1. Include this JS file in SelectDate.cshtml
 * 2. Ensure PersianDatePicker is loaded
 * 3. Module auto-initializes on document ready
 */

(function ($) {
    'use strict';

    // ✅ Date Selection Module - Bulletproof & Production-Ready
    const DateSelectionModule = {
        doctorId: null,
        selectedDatePersian: null,
        selectedDateGregorian: null,
        datePickerInstance: null,
        isProcessingSelection: false, // ✅ Flag برای جلوگیری از duplicate validation
        lastSelectionTime: 0, // ✅ Track selection time for auto-recovery

        init: function () {
            this.doctorId = parseInt($('#doctorId').val(), 10);
            if (!this.doctorId || isNaN(this.doctorId)) {
                this.showError('شناسه پزشک نامعتبر است');
                return;
            }

            // ✅ Wait for PersianDatePicker to initialize
            this.waitForDatePicker();
            this.bindEvents();
        },

        waitForDatePicker: function () {
            const self = this;
            let attempts = 0;
            const maxAttempts = 50; // 5 seconds max

            const checkInterval = setInterval(function () {
                attempts++;
                const $datePicker = $('#appointmentDatePicker');
                
                if ($datePicker.length && $datePicker.data('pDatepicker-initialized')) {
                    clearInterval(checkInterval);
                    self.datePickerInstance = $datePicker.data('pDatepicker');
                    self.initDatePickerHandlers();
                    console.log('✅ DatePicker initialized successfully');
                } else if (attempts >= maxAttempts) {
                    clearInterval(checkInterval);
                    console.error('❌ DatePicker failed to initialize after 5 seconds');
                    self.showError('خطا در بارگذاری تقویم. لطفاً صفحه را رفرش کنید.');
                }
            }, 100);
        },

        initDatePickerHandlers: function () {
            const self = this;
            const $datePicker = $('#appointmentDatePicker');

            // ✅ Method 1: Listen for pDatepicker:select event (Primary)
            $datePicker.on('pDatepicker:select', function (e) {
                console.log('📅 pDatepicker:select event fired', e);
                self.handleDateSelection(e);
            });

            // ✅ Method 2: Listen for change event (Fallback)
            $datePicker.on('change', function () {
                // ✅ CRITICAL FIX: جلوگیری از duplicate validation
                if (self.isProcessingSelection) {
                    console.log('📅 Selection already processing, skipping change event');
                    return;
                }
                
                const persianDate = $(this).val();
                if (persianDate && persianDate.trim() !== '') {
                    console.log('📅 Change event fired', persianDate);
                    self.isProcessingSelection = true;
                    self.handleDateSelectionFromPersian(persianDate);
                    // ✅ Flag در handleDateSelectionFromPersian reset می‌شود
                }
            });

            // ✅ Method 3: Direct access to datePicker instance (Ultimate Fallback)
            if (self.datePickerInstance && self.datePickerInstance.config) {
                const originalOnSelect = self.datePickerInstance.config.onSelect;
                if (originalOnSelect) {
                    self.datePickerInstance.config.onSelect = function(unix) {
                        // Call original handler
                        if (typeof originalOnSelect === 'function') {
                            originalOnSelect.call(this, unix);
                        }
                        // Our custom handler
                        console.log('📅 onSelect callback fired', unix);
                        self.handleDateSelectionFromUnix(unix);
                    };
                }
            }
        },

        bindEvents: function () {
            const self = this;
            $('#continueToTimeBtn').on('click', function () {
                self.handleContinue();
            });
        },

        handleDateSelection: function (event) {
            try {
                // ✅ CRITICAL FIX: جلوگیری از duplicate validation
                // اما اگر بیش از 2 ثانیه گذشته باشد، flag را reset می‌کنیم (auto-recovery)
                if (this.isProcessingSelection) {
                    const timeSinceLastSelection = Date.now() - (this.lastSelectionTime || 0);
                    if (timeSinceLastSelection > 2000) {
                        console.log('📅 Auto-resetting flag after timeout');
                        this.isProcessingSelection = false;
                    } else {
                        console.log('📅 Selection already processing, skipping duplicate event');
                        return;
                    }
                }
                
                // ✅ CRITICAL FIX: اولویت با input value (دقیق‌ترین و timezone-independent)
                // Input value همیشه Persian date string است که دقیق‌تر از unix timestamp است
                // استفاده از setTimeout برای اطمینان از اینکه input value set شده است
                const self = this;
                this.isProcessingSelection = true;
                this.lastSelectionTime = Date.now(); // ✅ Track selection time
                
                setTimeout(function() {
                    const $datePicker = $('#appointmentDatePicker');
                    if ($datePicker.length > 0) {
                        const persianDate = $datePicker.val();
                        if (persianDate && persianDate.trim() !== '') {
                            console.log('📅 Using input value (primary):', persianDate);
                            self.handleDateSelectionFromPersian(persianDate);
                            self.isProcessingSelection = false;
                            return;
                        }
                    }
                    
                    // ✅ Fallback: استفاده از event data
                    let eventData = null;
                    if (event && event.originalEvent) {
                        eventData = event.originalEvent;
                    } else if (event && event.data) {
                        eventData = event.data;
                    } else if (event && (event.unix || event.selected)) {
                        eventData = event;
                    }
                    
                    // ✅ Fallback 1: استفاده از selected object (jy, jm, jd)
                    if (eventData && eventData.selected) {
                        const selected = eventData.selected;
                        if (selected && selected.jy && selected.jm && selected.jd) {
                            const persianDate = `${selected.jy}/${String(selected.jm).padStart(2, '0')}/${String(selected.jd).padStart(2, '0')}`;
                            console.log('📅 Using selected object:', persianDate);
                            self.handleDateSelectionFromPersian(persianDate);
                            self.isProcessingSelection = false;
                            return;
                        }
                    }
                    
                    // ✅ Fallback 2: استفاده از unix timestamp (آخرین گزینه)
                    if (eventData && eventData.unix) {
                        const unixTimestamp = eventData.unix;
                        console.log('📅 Using unix timestamp from event (fallback):', unixTimestamp);
                        self.handleDateSelectionFromUnix(unixTimestamp);
                        self.isProcessingSelection = false;
                        return;
                    }
                    
                    console.warn('⚠️ No valid date found in event:', event);
                    self.isProcessingSelection = false;
                }, 50); // ✅ Small delay to ensure input value is set
            } catch (ex) {
                console.error('❌ Error in handleDateSelection:', ex);
                this.isProcessingSelection = false;
                this.showError('خطا در انتخاب تاریخ. لطفاً دوباره تلاش کنید.');
            }
        },

        handleDateSelectionFromUnix: function (unixTimestamp) {
            try {
                if (!unixTimestamp) {
                    this.isProcessingSelection = false; // ✅ CRITICAL: Reset flag if no timestamp
                    return;
                }

                // ✅ CRITICAL FIX: اولویت با Persian date string (timezone-independent)
                // Unix timestamp ممکن است از local timezone DatePicker بیاید
                if (this.datePickerInstance) {
                    const persianDate = this.datePickerInstance.getFormattedDate('YYYY/MM/DD');
                    if (persianDate) {
                        console.log('📅 Using Persian date from DatePicker:', persianDate);
                        // ✅ استفاده از Persian date برای تبدیل (دقیق‌تر از unix timestamp)
                        this.handleDateSelectionFromPersian(persianDate);
                        return;
                    }
                }

                // ✅ Fallback: استفاده از unix timestamp (اگر Persian date در دسترس نبود)
                const timestamp = unixTimestamp < 2e10 ? unixTimestamp * 1000 : unixTimestamp;
                // ✅ CRITICAL FIX: ساخت Date از UTC برای timezone-independent
                // Unix timestamp معمولاً UTC است، اما برای اطمینان از UTC استفاده می‌کنیم
                const date = new Date(timestamp);
                
                if (date && date instanceof Date && !isNaN(date.getTime())) {
                    // ✅ استفاده از UTC methods برای date-only (timezone-independent)
                    const year = date.getUTCFullYear();
                    const month = date.getUTCMonth() + 1;
                    const day = date.getUTCDate();
                    const dateUTC = new Date(Date.UTC(year, month - 1, day));
                    
                    this.selectedDateGregorian = dateUTC;
                    $('#selectedDateGregorian').val(this.formatDateForInput(dateUTC));
                    
                    // ✅ Get Persian date from datePicker instance
                    if (this.datePickerInstance) {
                        const persianDate = this.datePickerInstance.getFormattedDate('YYYY/MM/DD');
                        if (persianDate) {
                            this.selectedDatePersian = persianDate;
                            this.updateUI(persianDate);
                        }
                    }
                    
                    this.checkDateAvailability(dateUTC);
                } else {
                    this.isProcessingSelection = false; // ✅ CRITICAL: Reset flag if invalid date
                    console.error('❌ Invalid date from unix timestamp:', unixTimestamp);
                }
            } catch (ex) {
                this.isProcessingSelection = false; // ✅ CRITICAL: Reset flag on error
                console.error('❌ Error in handleDateSelectionFromUnix:', ex);
                this.showError('خطا در انتخاب تاریخ. لطفاً دوباره تلاش کنید.');
            }
        },

        handleDateSelectionFromPersian: function (persianDate) {
            try {
                if (!persianDate || persianDate.trim() === '') {
                    this.isProcessingSelection = false;
                    return;
                }

                // ✅ CRITICAL FIX: تبدیل اعداد فارسی به انگلیسی قبل از parse
                const englishDate = this.convertPersianToEnglishNumbers(persianDate.trim());
                
                // ✅ Convert Persian date to Gregorian
                const gregorianDate = this.convertPersianToGregorian(englishDate);
                if (gregorianDate) {
                    this.selectedDatePersian = persianDate; // نگه داشتن فرمت فارسی برای نمایش
                    this.selectedDateGregorian = gregorianDate;
                    $('#selectedDateGregorian').val(this.formatDateForInput(gregorianDate));
                    this.updateUI(persianDate);
                    this.checkDateAvailability(gregorianDate);
                    // ✅ Flag در checkDateAvailability reset می‌شود (بعد از async)
                } else {
                    this.isProcessingSelection = false;
                    console.error('❌ Failed to convert Persian date:', persianDate, 'English:', englishDate);
                    this.showError('تاریخ انتخاب شده نامعتبر است. لطفاً دوباره تلاش کنید.');
                }
            } catch (ex) {
                this.isProcessingSelection = false;
                console.error('❌ Error in handleDateSelectionFromPersian:', ex);
                this.showError('خطا در انتخاب تاریخ. لطفاً دوباره تلاش کنید.');
            }
        },

        updateUI: function (persianDate) {
            // ✅ Show feedback
            $('#selectedDateDisplay').text('تاریخ انتخاب شده: ' + persianDate);
            $('#dateSelectedFeedback').addClass('show');
        },

        convertPersianToGregorian: function (persianDate) {
            try {
                // ✅ CRITICAL FIX: اطمینان از اینکه تاریخ با اعداد انگلیسی است
                // (اگر هنوز فارسی است، تبدیل می‌کنیم)
                const englishDate = this.convertPersianToEnglishNumbers(persianDate);
                
                // ✅ Use jalaali library (loaded with PersianDatePicker)
                if (typeof jalaali !== 'undefined' && jalaali.toGregorian) {
                    const parts = englishDate.split('/');
                    if (parts.length === 3) {
                        const year = parseInt(parts[0], 10);
                        const month = parseInt(parts[1], 10);
                        const day = parseInt(parts[2], 10);
                        
                        if (!isNaN(year) && !isNaN(month) && !isNaN(day) && 
                            year > 0 && month >= 1 && month <= 12 && day >= 1 && day <= 31) {
                            const gregorian = jalaali.toGregorian(year, month, day);
                            // ✅ CRITICAL FIX: استفاده از UTC برای timezone-independent date
                            return new Date(Date.UTC(gregorian.gy, gregorian.gm - 1, gregorian.gd));
                        }
                    }
                }
                return null;
            } catch (ex) {
                console.error('❌ Error converting Persian to Gregorian:', ex);
                return null;
            }
        },

        /**
         * Convert Persian/Arabic Numbers to English
         * تبدیل اعداد فارسی/عربی به انگلیسی
         */
        convertPersianToEnglishNumbers: function(str) {
            if (!str) return str;
            
            const persianNumbers = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
            const arabicNumbers = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
            const englishNumbers = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
            
            let result = str.toString();
            for (let i = 0; i < 10; i++) {
                result = result.replace(new RegExp(persianNumbers[i], 'g'), englishNumbers[i]);
                result = result.replace(new RegExp(arabicNumbers[i], 'g'), englishNumbers[i]);
            }
            
            return result;
        },

        checkDateAvailability: function (date) {
            if (!date || !(date instanceof Date) || isNaN(date.getTime())) {
                $('#continueToTimeBtn').prop('disabled', true);
                this.isProcessingSelection = false; // ✅ CRITICAL: Reset flag if invalid date
                return;
            }

            // ✅ CRITICAL FIX: استفاده از server today (Iran timezone) به جای client Date
            // دریافت تاریخ امروز از server برای اطمینان از صحت
            const self = this;
            if (window.PersianDatePickerComponent && window.PersianDatePickerComponent.getTodayFromServer) {
                window.PersianDatePickerComponent.getTodayFromServer().then(function(todayPersian) {
                    // ✅ CRITICAL FIX: مقایسه date strings (timezone-independent)
                    // تبدیل todayPersian (1404/10/15) به Gregorian string برای مقایسه
                    const todayGregorian = self.convertPersianToGregorian(todayPersian);
                    if (todayGregorian) {
                        // ✅ استفاده از date string (YYYY-MM-DD) برای مقایسه timezone-independent
                        const todayString = self.formatDateForInput(todayGregorian); // "2025-12-26"
                        const selectedString = self.formatDateForInput(date); // "2026-01-05"
                        
                        console.log('🔍 Date comparison - Today (Iran):', todayString, 'Selected:', selectedString);

                        // ✅ Check if date is in the past (string comparison is timezone-independent)
                        if (selectedString < todayString) {
                            $('#continueToTimeBtn').prop('disabled', true);
                            $('#dateSelectedFeedback').removeClass('show');
                            self.showError('نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید');
                            console.warn('⚠️ Date rejected as past:', selectedString, '<', todayString);
                            self.isProcessingSelection = false; // ✅ CRITICAL: Reset flag before return
                            return;
                        }

                        // ✅ Date is valid - enable button
                        $('#continueToTimeBtn').prop('disabled', false);
                        console.log('✅ Date selected and validated (Iran timezone):', selectedString, '>=', todayString);
                        self.isProcessingSelection = false; // ✅ Reset flag after validation
                    } else {
                        // Fallback: استفاده از client date (اگر server unavailable)
                        console.warn('⚠️ Failed to convert todayPersian, using fallback');
                        self.checkDateAvailabilityFallback(date);
                    }
                }).catch(function(error) {
                    // Fallback: استفاده از client date
                    console.warn('⚠️ getTodayFromServer failed, using fallback:', error);
                    self.checkDateAvailabilityFallback(date);
                });
            } else {
                // Fallback: استفاده از client date
                console.warn('⚠️ PersianDatePickerComponent not available, using fallback');
                this.checkDateAvailabilityFallback(date);
            }
        },

        checkDateAvailabilityFallback: function (date) {
            // ✅ Fallback: محاسبه ایران‌محور در client
            const now = new Date();
            const utcMs = now.getTime() + (now.getTimezoneOffset() * 60000);
            const iranMs = utcMs + (210 * 60000); // +03:30
            const iranDate = new Date(iranMs);
            
            // ✅ استفاده از UTC برای date-only (timezone-independent)
            const today = new Date(Date.UTC(iranDate.getUTCFullYear(), iranDate.getUTCMonth(), iranDate.getUTCDate()));
            const todayString = this.formatDateForInput(today);
            
            const selectedString = this.formatDateForInput(date);
            
            console.log('🔍 Date comparison (fallback) - Today (Iran):', todayString, 'Selected:', selectedString);

            if (selectedString < todayString) {
                $('#continueToTimeBtn').prop('disabled', true);
                $('#dateSelectedFeedback').removeClass('show');
                this.showError('نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید');
                console.warn('⚠️ Date rejected as past (fallback):', selectedString, '<', todayString);
                this.isProcessingSelection = false; // ✅ CRITICAL: Reset flag before return
                return;
            }

            $('#continueToTimeBtn').prop('disabled', false);
            console.log('✅ Date selected and validated (fallback):', selectedString, '>=', todayString);
            this.isProcessingSelection = false; // ✅ Reset flag after validation
        },

        handleContinue: function () {
            if (!this.selectedDateGregorian) {
                this.showError('لطفاً تاریخ را انتخاب کنید');
                return;
            }

            const dateStr = this.formatDateForInput(this.selectedDateGregorian);
            // ✅ CRITICAL: استفاده از config برای URL (باید بعداً به app-config.js منتقل شود)
            const baseUrl = window.appConfig?.appointmentBooking?.selectTimeUrl || '/Patient/Appointment/Book/SelectTime';
            const url = baseUrl + '?doctorId=' + 
                        encodeURIComponent(this.doctorId) + 
                        '&date=' + encodeURIComponent(dateStr);
            
            // ✅ Show loading state
            const $btn = $('#continueToTimeBtn');
            $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>در حال انتقال...');
            
            window.location.href = url;
        },

        formatDateForInput: function (date) {
            if (!date || !(date instanceof Date) || isNaN(date.getTime())) {
                return '';
            }
            // ✅ CRITICAL FIX: استفاده از UTC برای timezone-independent date-only
            // این برای مقایسه تاریخ‌ها مهم است (مستقل از timezone کاربر)
            const year = date.getUTCFullYear();
            const month = String(date.getUTCMonth() + 1).padStart(2, '0');
            const day = String(date.getUTCDate()).padStart(2, '0');
            return `${year}-${month}-${day}`;
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

    // ✅ Initialize on document ready
    $(document).ready(function () {
        // ✅ Wait for jQuery and PersianDatePicker to be ready
        if (typeof window.whenJQ === 'function') {
            window.whenJQ(function () {
                setTimeout(function () {
                    DateSelectionModule.init();
                }, 500); // ✅ Small delay to ensure datePicker is ready
            });
        } else {
            // Fallback: Wait for jQuery
            var jqCheck = setInterval(function () {
                if (typeof jQuery !== 'undefined' && jQuery.fn.pDatepicker) {
                    clearInterval(jqCheck);
                    setTimeout(function () {
                        DateSelectionModule.init();
                    }, 500);
                }
            }, 100);

            // Timeout after 5 seconds
            setTimeout(function () {
                clearInterval(jqCheck);
                if (typeof jQuery !== 'undefined') {
                    DateSelectionModule.init();
                }
            }, 5000);
        }
    });

    // ✅ Export for global access
    window.DateSelectionModule = DateSelectionModule;

})(jQuery);
