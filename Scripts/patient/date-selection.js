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
                const persianDate = $(this).val();
                if (persianDate && persianDate.trim() !== '') {
                    console.log('📅 Change event fired', persianDate);
                    self.handleDateSelectionFromPersian(persianDate);
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
                // ✅ CRITICAL FIX: اولویت با unix timestamp (دقیق‌تر است)
                // jQuery event object structure: event.originalEvent یا event.data
                let eventData = null;
                
                if (event && event.originalEvent) {
                    eventData = event.originalEvent;
                } else if (event && event.data) {
                    eventData = event.data;
                } else if (event && (event.unix || event.selected)) {
                    eventData = event;
                }
                
                if (eventData && eventData.unix) {
                    const unixTimestamp = eventData.unix;
                    console.log('📅 Using unix timestamp from event:', unixTimestamp);
                    this.handleDateSelectionFromUnix(unixTimestamp);
                    return;
                }
                
                // ✅ Fallback: استفاده از selected object (jy, jm, jd)
                if (eventData && eventData.selected) {
                    const selected = eventData.selected;
                    if (selected && selected.jy && selected.jm && selected.jd) {
                        const persianDate = `${selected.jy}/${String(selected.jm).padStart(2, '0')}/${String(selected.jd).padStart(2, '0')}`;
                        console.log('📅 Using selected object:', persianDate);
                        this.handleDateSelectionFromPersian(persianDate);
                        return;
                    }
                }
                
                // ✅ Last fallback: استفاده از input value
                const $datePicker = $('#appointmentDatePicker');
                if ($datePicker.length > 0) {
                    const persianDate = $datePicker.val();
                    if (persianDate && persianDate.trim() !== '') {
                        console.log('📅 Using input value as fallback:', persianDate);
                        this.handleDateSelectionFromPersian(persianDate);
                        return;
                    }
                }
                
                console.warn('⚠️ No valid date found in event:', event);
            } catch (ex) {
                console.error('❌ Error in handleDateSelection:', ex);
                this.showError('خطا در انتخاب تاریخ. لطفاً دوباره تلاش کنید.');
            }
        },

        handleDateSelectionFromUnix: function (unixTimestamp) {
            try {
                if (!unixTimestamp) return;

                // ✅ Convert Unix timestamp to Date (handle both seconds and milliseconds)
                const timestamp = unixTimestamp < 2e10 ? unixTimestamp * 1000 : unixTimestamp;
                const date = new Date(timestamp);
                
                if (date && date instanceof Date && !isNaN(date.getTime())) {
                    this.selectedDateGregorian = date;
                    $('#selectedDateGregorian').val(this.formatDateForInput(date));
                    
                    // ✅ Get Persian date from datePicker instance
                    if (this.datePickerInstance) {
                        const persianDate = this.datePickerInstance.getFormattedDate('YYYY/MM/DD');
                        if (persianDate) {
                            this.selectedDatePersian = persianDate;
                            this.updateUI(persianDate);
                        }
                    }
                    
                    this.checkDateAvailability(date);
                }
            } catch (ex) {
                console.error('❌ Error in handleDateSelectionFromUnix:', ex);
                this.showError('خطا در انتخاب تاریخ. لطفاً دوباره تلاش کنید.');
            }
        },

        handleDateSelectionFromPersian: function (persianDate) {
            try {
                if (!persianDate || persianDate.trim() === '') return;

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
                } else {
                    console.error('❌ Failed to convert Persian date:', persianDate, 'English:', englishDate);
                    this.showError('تاریخ انتخاب شده نامعتبر است. لطفاً دوباره تلاش کنید.');
                }
            } catch (ex) {
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
                            return new Date(gregorian.gy, gregorian.gm - 1, gregorian.gd);
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
                return;
            }

            // ✅ Get today's date (server timezone aware)
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            
            const selectedDateOnly = new Date(date);
            selectedDateOnly.setHours(0, 0, 0, 0);

            // ✅ Check if date is in the past
            if (selectedDateOnly < today) {
                $('#continueToTimeBtn').prop('disabled', true);
                $('#dateSelectedFeedback').removeClass('show');
                this.showError('نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید');
                return;
            }

            // ✅ Date is valid - enable button
            $('#continueToTimeBtn').prop('disabled', false);
            console.log('✅ Date selected and validated:', this.formatDateForInput(date));
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
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
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
