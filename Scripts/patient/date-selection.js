/**
 * JavaScript Module برای انتخاب تاریخ
 * رعایت SRP: فقط مدیریت تقویم و انتخاب تاریخ
 */
(function ($) {
    'use strict';

    const DateSelection = {
        doctorId: null,
        selectedDate: null,
        availableDates: [],

        init: function () {
            this.doctorId = $('#doctorId').val();
            if (!this.doctorId) {
                this.showError('شناسه پزشک نامعتبر است');
                return;
            }

            this.initPersianDatePicker();
            this.loadAvailableDates();
            this.bindEvents();
        },

        initPersianDatePicker: function () {
            const self = this;
            
            $('#persianDatePicker').persianDatepicker({
                observer: true,
                format: 'YYYY/MM/DD',
                altField: '#selectedDate',
                altFormat: 'YYYY-MM-DD',
                calendarType: 'persian',
                timePicker: {
                    enabled: false
                },
                onSelect: function (unixDate) {
                    const selectedDate = new Date(unixDate);
                    self.handleDateSelection(selectedDate);
                }
            });
        },

        bindEvents: function () {
            $('#continueToTimeBtn').on('click', this.handleContinue.bind(this));
        },

        loadAvailableDates: function () {
            // TODO: دریافت تاریخ‌های در دسترس از API
            // فعلاً از تقویم استفاده می‌کنیم
            this.renderAvailableDates();
        },

        handleDateSelection: function (date) {
            this.selectedDate = date;
            $('#selectedDate').val(this.formatDateForInput(date));
            
            // بررسی دسترسی‌پذیری
            this.checkDateAvailability(date);
        },

        checkDateAvailability: function (date) {
            if (!date || date < new Date()) {
                $('#continueToTimeBtn').prop('disabled', true);
                this.showError('نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید');
                return;
            }

            // TODO: بررسی از طریق API
            $('#continueToTimeBtn').prop('disabled', false);
        },

        handleContinue: function () {
            if (!this.selectedDate) {
                this.showError('لطفاً تاریخ را انتخاب کنید');
                return;
            }

            const dateStr = this.formatDateForInput(this.selectedDate);
            window.location.href = `/Patient/AppointmentBooking/SelectTime?doctorId=${this.doctorId}&date=${dateStr}`;
        },

        renderAvailableDates: function () {
            // TODO: Render available dates from API
            // فعلاً فقط تقویم نمایش داده می‌شود
        },

        formatDateForInput: function (date) {
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            return `${year}-${month}-${day}`;
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
        DateSelection.init();
    });

    // Export for global access
    window.DateSelection = DateSelection;

})(jQuery);

