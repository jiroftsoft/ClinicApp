// Persian Date Picker Helper - کلینیک شفا
// این فایل برای رعایت استانداردهای سیستم‌های پزشکی ایرانی طراحی شده است
(function ($) {
    "use strict";

    // تنظیمات پیش‌فرض تقویم شمسی برای سیستم پزشکی
    var defaultSettings = {
        format: 'YYYY/MM/DD',
        autoClose: true,
        initialValue: false,
        observer: true,
        altFormat: 'YYYY-MM-DD',
        calendarSettings: {
            persian: {
                locale: 'fa',
                leapYearMode: 'astronomical'
            }
        },
        theme: 'blue',
        digits: 'persian',
        toolbox: {
            todayBtn: {
                enabled: true,
                text: { fa: 'امروز' }
            },
            clearBtn: {
                enabled: true,
                text: { fa: 'پاک کردن' }
            },
            calendarSwitch: {
                enabled: true,
                format: 'YYYY'
            }
        },
        calendar: {
            persian: {
                monthNames: [
                    'فروردین', 'اردیبهشت', 'خرداد',
                    'تیر', 'مرداد', 'شهریور',
                    'مهر', 'آبان', 'آذر',
                    'دی', 'بهمن', 'اسفند'
                ],
                dayNames: [
                    'شنبه', 'یک‌شنبه', 'دوشنبه',
                    'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه'
                ],
                dayNamesShort: ['ش', 'ی', 'د', 'س', 'چ', 'پ', 'ج']
            }
        },
        textArray: {
            'fa': {
                'today': 'امروز',
                'yesterday': 'دیروز',
                'lastWeek': 'هفته گذشته',
                'lastMonth': 'ماه گذشته',
                'lastYear': 'سال گذشته',
                'clear': 'پاک کردن',
                'close': 'بستن'
            }
        }
    };

    // روش‌های کمکی برای تقویم شمسی
    $.fn.persianDatepicker = function (options) {
        var settings = $.extend({}, defaultSettings, options);

        return this.each(function () {
            var $this = $(this);
            var $altField = $this.data('alt-field') ? $($this.data('alt-field')) : null;

            // اگر فیلد مخفی وجود داشته باشد، آن را تنظیم کن
            if ($altField && $altField.length) {
                settings.altField = $altField;
            }

            // راه‌اندازی تقویم شمسی
            $this.pDatepicker(settings);

            // افزودن کلاس‌های استایل پزشکی
            $this.addClass('datepicker-input');
            $this.closest('.form-group').addClass('datepicker-container');

            // افزودن آیکون تقویم
            if (!$this.siblings('.datepicker-icon').length) {
                $this.before('<i class="fas fa-calendar-alt datepicker-icon"></i>');
            }
        });
    };

    // روش کمکی برای تبدیل تاریخ شمسی به میلادی
    $.fn.toGregorianDate = function (persianDate) {
        if (!persianDate) return null;

        try {
            // استفاده از کلاس PersianDateHelper برای تبدیل ایمن
            // در عمل، این تبدیل باید در سمت سرور انجام شود
            // اما برای UI، این یک تبدیل تقریبی است
            var parts = persianDate.split('/');
            if (parts.length !== 3) return null;

            var year = parseInt(parts[0]);
            var month = parseInt(parts[1]);
            var day = parseInt(parts[2]);

            // تبدیل به میلادی
            var gregorianDate = new Date(year + 621, month - 1, day);
            return gregorianDate.toISOString().split('T')[0];
        } catch (e) {
            console.error("Error converting Persian date:", e);
            return null;
        }
    };

    // راه‌اندازی خودکار تقویم‌ها با کلاس خاص
    $(function () {
        $('.persian-datepicker').each(function () {
            var $this = $(this);
            var options = $this.data('datepicker-options') || {};
            $this.persianDatepicker(options);
        });
    });

})(jQuery);