/**
 * Persian DatePicker Helper Functions
 * بهبود یافته برای ClinicApp با محافظت jQuery
 */

(function($) {
    'use strict';

    // محافظت jQuery - اطمینان از بارگذاری کامل jQuery
    function ensureJQuery(callback) {
        if (typeof $ !== 'undefined' && typeof $.fn !== 'undefined') {
            callback();
        } else {
            setTimeout(function() {
                ensureJQuery(callback);
            }, 50);
        }
    }

    // راه‌اندازی Persian DatePicker با محافظت کامل
    function initializePersianDatePicker() {
        ensureJQuery(function() {
            if (typeof $.fn.persianDatepicker === 'undefined') {
                console.warn('Persian DatePicker plugin not loaded');
                return;
            }

            // راه‌اندازی خودکار برای کلاس‌های مشخص
            $('.persian-date').each(function() {
                var $this = $(this);
                if (!$this.data('persian-datepicker-initialized')) {
                    $this.persianDatepicker({
                        format: 'YYYY/MM/DD',
                        initialValue: false,
                        autoClose: true,
                        observer: true,
                        calendar: {
                            persian: {
                                locale: 'fa',
                                leapYearMode: 'astronomical'
                            }
                        },
                        toolbox: {
                            todayBtn: { enabled: true, text: { fa: 'امروز' } },
                            clearBtn: { enabled: true, text: { fa: 'پاک کردن' } }
                        },
                        onSelect: function(unix) {
                            var date = new Date(unix);
                            var persianDate = date.getFullYear() + '/' + 
                                String(date.getMonth() + 1).padStart(2, '0') + '/' + 
                                String(date.getDate()).padStart(2, '0');
                            $this.val(persianDate);
                        }
                    });
                    $this.data('persian-datepicker-initialized', true);
                }
            });

            // راه‌اندازی برای کلاس‌های دیگر
            $('.datepicker').each(function() {
                var $this = $(this);
                if (!$this.data('persian-datepicker-initialized')) {
                    $this.persianDatepicker({
                        format: 'YYYY/MM/DD',
                        initialValue: false,
                        autoClose: true,
                        observer: true,
                        calendar: {
                            persian: {
                                locale: 'fa',
                                leapYearMode: 'astronomical'
                            }
                        }
                    });
                    $this.data('persian-datepicker-initialized', true);
                }
            });
        });
    }

    // راه‌اندازی خودکار
    $(document).ready(function() {
        initializePersianDatePicker();
    });

    // راه‌اندازی مجدد برای محتوای AJAX
    $(document).on('DOMNodeInserted', function(e) {
        if ($(e.target).hasClass('persian-date') || $(e.target).hasClass('datepicker')) {
            setTimeout(function() {
                initializePersianDatePicker();
            }, 100);
        }
    });

    // متدهای کمکی برای استفاده در صفحات
    $.fn.initPersianDatePicker = function(options) {
        return this.each(function() {
            var $this = $(this);
            if (typeof $.fn.persianDatepicker !== 'undefined') {
                $this.persianDatepicker(options || {
                    format: 'YYYY/MM/DD',
                    initialValue: false,
                    autoClose: true,
                    observer: true
                });
            }
        });
    };

    // متد کمکی برای تبدیل تاریخ شمسی به میلادی
    $.fn.toGregorianDate = function (persianDate) {
        if (!persianDate) return null;

        try {
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

    // متد کمکی برای تبدیل تاریخ میلادی به شمسی
    $.fn.toPersianDate = function (gregorianDate) {
        if (!gregorianDate) return null;

        try {
            var date = new Date(gregorianDate);
            var year = date.getFullYear() - 621;
            var month = date.getMonth() + 1;
            var day = date.getDate();

            return year + '/' + 
                String(month).padStart(2, '0') + '/' + 
                String(day).padStart(2, '0');
        } catch (e) {
            console.error("Error converting Gregorian date:", e);
            return null;
        }
    };

    // راه‌اندازی خودکار تقویم‌ها با کلاس خاص
    $(function () {
        $('.persian-datepicker').each(function () {
            var $this = $(this);
            var options = $this.data('datepicker-options') || {};
            $this.initPersianDatePicker(options);
        });
    });

})(jQuery);