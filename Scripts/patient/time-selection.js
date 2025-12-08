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
        },

        bindEvents: function () {
            // انتخاب اسلات
            $(document).on('click', '.select-slot-btn', this.handleSelectSlot.bind(this));
            
            // پاک کردن انتخاب
            $('#clearSelectionBtn').on('click', this.handleClearSelection.bind(this));
            
            // ادامه به تایید
            $('#continueToConfirmBtn').on('click', this.handleContinue.bind(this));
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

            // نمایش اطلاعات انتخاب شده
            this.showSelectedSlotInfo();
            
            // فعال کردن دکمه ادامه
            $('#continueToConfirmBtn').prop('disabled', false);
        },

        handleClearSelection: function () {
            $('.time-slot-card').removeClass('selected');
            this.selectedSlot = null;
            $('#selectedSlotInfo').removeClass('show');
            $('#continueToConfirmBtn').prop('disabled', true);
        },

        handleContinue: function () {
            if (!this.selectedSlot) {
                this.showError('لطفاً زمان را انتخاب کنید');
                return;
            }

            // بررسی مجدد دسترسی‌پذیری
            this.checkSlotAvailability();
        },

        checkSlotAvailability: function () {
            showLoading();

            $.ajax({
                url: '/Patient/Api/DoctorSearch/CheckSlotAvailability',
                type: 'POST',
                data: {
                    doctorId: this.doctorId,
                    appointmentDate: this.selectedDate,
                    startTime: this.selectedSlot.startTime,
                    endTime: this.selectedSlot.endTime
                },
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: (response) => {
                    hideLoading();
                    if (response.success && response.isAvailable) {
                        this.proceedToConfirm();
                    } else {
                        this.showError('این زمان دیگر در دسترس نیست. لطفاً زمان دیگری انتخاب کنید');
                        this.updateSlotAvailability();
                    }
                },
                error: () => {
                    hideLoading();
                    this.showError('خطا در بررسی دسترسی‌پذیری');
                }
            });
        },

        proceedToConfirm: function () {
            const params = new URLSearchParams({
                doctorId: this.doctorId,
                appointmentDate: this.selectedDate,
                startTime: this.selectedSlot.startTime,
                endTime: this.selectedSlot.endTime
            });

            window.location.href = `/Patient/AppointmentBooking/ConfirmBooking?${params.toString()}`;
        },

        showSelectedSlotInfo: function () {
            $('#selectedTimeDisplay').text(this.selectedSlot.displayTime);
            $('#selectedSlotInfo').addClass('show');
            $('#selectedStartTime').val(this.selectedSlot.startTime);
            $('#selectedEndTime').val(this.selectedSlot.endTime);
        },

        startRealTimeUpdates: function () {
            // به‌روزرسانی Real-time هر 30 ثانیه
            this.updateInterval = setInterval(() => {
                this.updateSlotAvailability();
            }, 30000);
        },

        updateSlotAvailability: function () {
            $.ajax({
                url: '/Patient/Api/DoctorSearch/GetAvailableTimeSlots',
                type: 'GET',
                data: {
                    id: this.doctorId,
                    date: this.selectedDate
                },
                success: (response) => {
                    if (response.success && response.data) {
                        this.updateSlotsUI(response.data);
                    }
                },
                error: () => {
                    // Silent fail برای Real-time updates
                    console.error('خطا در به‌روزرسانی اسلات‌ها');
                }
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

        showError: function (message) {
            Swal.fire({
                title: 'خطا',
                text: message,
                icon: 'error',
                confirmButtonText: 'باشه'
            });
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

