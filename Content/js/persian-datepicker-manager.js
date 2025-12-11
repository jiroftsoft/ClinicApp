/**
 * Persian DatePicker Manager
 * ماژول حرفه‌ای برای مدیریت Persian DatePicker
 * 
 * اصول طراحی:
 * - SRP: هر تابع یک مسئولیت دارد
 * - DRY: بدون تکرار کد
 * - Logging: لاگ‌گذاری کامل برای دیباگ
 * - Modular: قابل استفاده مجدد در تمام پروژه
 * 
 * @version 1.0.0
 * @author ClinicApp Team
 */

(function(window, $) {
    'use strict';

    /**
     * Persian DatePicker Manager Module
     * ماژول اصلی برای مدیریت Persian DatePicker
     */
    var PersianDatePickerManager = {
        
        /**
         * Configuration
         * تنظیمات پیش‌فرض
         */
        config: {
            selector: 'input[data-persian-datepicker="true"]',
            hiddenInputSuffix: '_Hidden',
            logPrefix: '📅 [PersianDatePicker]',
            enableLogging: true
        },

        /**
         * Logger
         * سیستم لاگ‌گذاری
         */
        logger: {
            log: function(message, data) {
                if (PersianDatePickerManager.config.enableLogging) {
                    console.log(PersianDatePickerManager.config.logPrefix, message, data || '');
                }
            },
            error: function(message, error, data) {
                if (PersianDatePickerManager.config.enableLogging) {
                    console.error(PersianDatePickerManager.config.logPrefix, '❌', message, error, data || '');
                }
            },
            warn: function(message, data) {
                if (PersianDatePickerManager.config.enableLogging) {
                    console.warn(PersianDatePickerManager.config.logPrefix, '⚠️', message, data || '');
                }
            },
            success: function(message, data) {
                if (PersianDatePickerManager.config.enableLogging) {
                    console.log(PersianDatePickerManager.config.logPrefix, '✅', message, data || '');
                }
            }
        },

        /**
         * Convert Persian/Arabic Numbers to English
         * تبدیل اعداد فارسی/عربی به انگلیسی
         * 
         * @param {string} str - رشته حاوی اعداد فارسی/عربی
         * @returns {string} - رشته با اعداد انگلیسی
         */
        convertPersianToEnglishNumbers: function(str) {
            if (!str) return str;
            
            var persianNumbers = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
            var arabicNumbers = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
            var englishNumbers = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
            
            var result = str;
            for (var i = 0; i < 10; i++) {
                result = result.replace(new RegExp(persianNumbers[i], 'g'), englishNumbers[i]);
                result = result.replace(new RegExp(arabicNumbers[i], 'g'), englishNumbers[i]);
            }
            
            return result;
        },

        /**
         * Convert Persian Date to Gregorian
         * تبدیل تاریخ شمسی به میلادی با استفاده از jalaali
         * 
         * @param {string} persianDate - تاریخ شمسی (مثلاً "1404/09/19" یا "۱۴۰۴/۰۹/۱۹")
         * @returns {string|null} - تاریخ میلادی ISO format یا null
         */
        convertPersianToGregorian: function(persianDate) {
            try {
                if (!persianDate || persianDate.trim() === '') {
                    return null;
                }

                // تبدیل اعداد فارسی/عربی به انگلیسی
                var normalizedDate = this.convertPersianToEnglishNumbers(persianDate.trim());
                
                var parts = normalizedDate.split('/');
                if (parts.length !== 3) {
                    this.logger.warn('فرمت تاریخ شمسی نامعتبر:', persianDate);
                    return null;
                }

                var persianYear = parseInt(parts[0], 10);
                var persianMonth = parseInt(parts[1], 10);
                var persianDay = parseInt(parts[2], 10);

                // بررسی اعتبار اعداد
                if (isNaN(persianYear) || isNaN(persianMonth) || isNaN(persianDay)) {
                    this.logger.warn('اعداد تاریخ شمسی نامعتبر:', {
                        original: persianDate,
                        normalized: normalizedDate,
                        year: persianYear,
                        month: persianMonth,
                        day: persianDay
                    });
                    return null;
                }

                // استفاده از jalaali برای تبدیل
                if (typeof jalaali !== 'undefined' && jalaali.toGregorian) {
                    var gregorian = jalaali.toGregorian(persianYear, persianMonth, persianDay);
                    
                    // بررسی اعتبار نتیجه
                    if (!gregorian || !gregorian.gy || !gregorian.gm || !gregorian.gd) {
                        this.logger.error('خطا در تبدیل تاریخ با jalaali:', {
                            persian: persianDate,
                            normalized: normalizedDate,
                            year: persianYear,
                            month: persianMonth,
                            day: persianDay
                        });
                        return null;
                    }
                    
                    var year = gregorian.gy;
                    var month = String(gregorian.gm).padStart(2, '0');
                    var day = String(gregorian.gd).padStart(2, '0');
                    
                    var dateISO = year + '-' + month + '-' + day + 'T00:00:00';
                    
                    this.logger.success('تبدیل موفق:', {
                        persian: persianDate,
                        normalized: normalizedDate,
                        gregorian: dateISO,
                        year: persianYear,
                        month: persianMonth,
                        day: persianDay
                    });
                    
                    return dateISO;
                } else {
                    this.logger.error('jalaali library یافت نشد');
                    return null;
                }
            } catch (error) {
                this.logger.error('خطا در تبدیل تاریخ شمسی به میلادی', error, persianDate);
                return null;
            }
        },

        /**
         * Create or Get Hidden Input
         * ایجاد یا دریافت hidden input برای ذخیره تاریخ میلادی
         * 
         * @param {jQuery} $form - فرم والد
         * @param {string} fieldName - نام فیلد
         * @returns {jQuery} - hidden input element
         */
        getOrCreateHiddenInput: function($form, fieldName) {
            var hiddenName = fieldName + this.config.hiddenInputSuffix;
            var $hiddenInput = $form.find('input[type="hidden"][name="' + hiddenName + '"]');
            
            if ($hiddenInput.length === 0) {
                $hiddenInput = $('<input>').attr({
                    type: 'hidden',
                    name: hiddenName,
                    value: ''
                });
                $form.append($hiddenInput);
                this.logger.log('Hidden input ایجاد شد:', hiddenName);
            }
            
            return $hiddenInput;
        },

        /**
         * Initialize Single DatePicker
         * Initialize کردن یک DatePicker
         * 
         * @param {jQuery} $input - input element
         */
        initializeDatePicker: function($input) {
            var self = this;
            
            // بررسی اینکه قبلاً initialize شده یا نه
            if ($input.data('pDatepicker-initialized')) {
                this.logger.log('DatePicker قبلاً initialize شده:', $input.attr('name'));
                return;
            }

            var fieldName = $input.attr('name');
            var $form = $input.closest('form');
            
            if ($form.length === 0) {
                this.logger.warn('فرم والد یافت نشد برای:', fieldName);
                return;
            }

            // ایجاد hidden input
            var $hiddenInput = this.getOrCreateHiddenInput($form, fieldName);

            // اگر input مقدار دارد (مثلاً در Edit form)، تبدیل کن
            var currentValue = $input.val();
            if (currentValue && currentValue.trim() !== '') {
                var gregorianDate = this.convertPersianToGregorian(currentValue);
                if (gregorianDate) {
                    $hiddenInput.val(gregorianDate);
                    this.logger.success('تاریخ موجود تبدیل شد:', {
                        field: fieldName,
                        persian: currentValue,
                        gregorian: gregorianDate
                    });
                }
            }

            // Initialize pDatepicker
            var datePickerConfig = {
                calendarType: 'persian',
                format: 'YYYY/MM/DD',
                autoClose: true,
                observer: true,
                timePicker: {
                    enabled: false
                },
                toolbox: {
                    calendarSwitch: {
                        enabled: false
                    }
                },
                navigator: {
                    enabled: true
                },
                onlyTimePicker: false,
                onlySelectOnDate: true,
                calendar: {
                    persian: {
                        enabled: true,
                        locale: 'fa'
                    }
                },
                onSelect: function(unix) {
                    self.handleDateSelect($input, $hiddenInput, fieldName, unix);
                }
            };

            // تنظیم initialValue اگر مقدار وجود دارد
            if (currentValue && currentValue.trim() !== '') {
                // تبدیل اعداد فارسی به انگلیسی برای datePicker
                var normalizedValue = this.convertPersianToEnglishNumbers(currentValue.trim());
                datePickerConfig.initialValue = normalizedValue;
                datePickerConfig.initialValueType = 'persian';
                
                this.logger.log('مقدار اولیه تنظیم شد:', {
                    field: fieldName,
                    original: currentValue,
                    normalized: normalizedValue
                });
            } else {
                datePickerConfig.initialValue = false;
            }

            $input.pDatepicker(datePickerConfig);

            // Mark as initialized
            $input.data('pDatepicker-initialized', true);
            this.logger.success('DatePicker initialize شد:', fieldName);
        },

        /**
         * Handle Date Select
         * مدیریت انتخاب تاریخ
         * 
         * @param {jQuery} $input - input element
         * @param {jQuery} $hiddenInput - hidden input element
         * @param {string} fieldName - نام فیلد
         * @param {number} unix - unix timestamp
         */
        handleDateSelect: function($input, $hiddenInput, fieldName, unix) {
            try {
                var persianDateStr = $input.val();
                
                if (!persianDateStr || persianDateStr.trim() === '') {
                    $hiddenInput.val('');
                    this.logger.warn('تاریخ خالی انتخاب شد:', fieldName);
                    return;
                }

                var gregorianDate = this.convertPersianToGregorian(persianDateStr);
                
                if (gregorianDate) {
                    $hiddenInput.val(gregorianDate);
                    this.logger.success('تاریخ انتخاب و تبدیل شد:', {
                        field: fieldName,
                        persian: persianDateStr,
                        gregorian: gregorianDate,
                        timestamp: unix
                    });
                } else {
                    this.logger.error('خطا در تبدیل تاریخ', null, {
                        field: fieldName,
                        persian: persianDateStr
                    });
                    $hiddenInput.val('');
                }
            } catch (error) {
                this.logger.error('خطا در handleDateSelect', error, fieldName);
                $hiddenInput.val('');
            }
        },

        /**
         * Prepare Form for Submit
         * آماده‌سازی فرم قبل از submit
         * 
         * @param {jQuery} $form - فرم
         */
        prepareFormForSubmit: function($form) {
            var self = this;
            var hasError = false;

            this.logger.log('آماده‌سازی فرم برای submit...');

            $form.find(this.config.selector).each(function() {
                var $input = $(this);
                var fieldName = $input.attr('name');
                var $hiddenInput = self.getOrCreateHiddenInput($form, fieldName);
                
                var persianDate = $input.val();
                
                // اگر input خالی است
                if (!persianDate || persianDate.trim() === '') {
                    $hiddenInput.val('');
                    self.logger.log('تاریخ خالی:', fieldName);
                } else {
                    // اگر hidden input خالی است، تبدیل کن
                    if (!$hiddenInput.val() || $hiddenInput.val().trim() === '') {
                        var gregorianDate = self.convertPersianToGregorian(persianDate);
                        if (gregorianDate) {
                            $hiddenInput.val(gregorianDate);
                            self.logger.success('تاریخ در submit تبدیل شد:', {
                                field: fieldName,
                                persian: persianDate,
                                gregorian: gregorianDate
                            });
                        } else {
                            self.logger.error('خطا در تبدیل تاریخ در submit', null, {
                                field: fieldName,
                                persian: persianDate
                            });
                            hasError = true;
                        }
                    } else {
                        self.logger.log('Hidden input از قبل مقدار دارد:', {
                            field: fieldName,
                            value: $hiddenInput.val()
                        });
                    }
                }
                
                // غیرفعال کردن input اصلی
                $input.prop('disabled', true);
            });

            if (hasError) {
                this.logger.error('خطا در آماده‌سازی فرم');
            } else {
                this.logger.success('فرم آماده submit است');
            }

            return !hasError;
        },

        /**
         * Initialize All DatePickers
         * Initialize کردن تمام DatePicker ها
         */
        initializeAll: function() {
            var self = this;
            
            // بررسی jQuery و pDatepicker
            if (typeof jQuery === 'undefined') {
                this.logger.error('jQuery یافت نشد');
                return false;
            }

            if (typeof $.fn.pDatepicker === 'undefined') {
                this.logger.warn('pDatepicker یافت نشد، تلاش مجدد...');
                setTimeout(function() {
                    self.initializeAll();
                }, 100);
                return false;
            }

            this.logger.log('شروع initialize تمام DatePicker ها...');

            var count = 0;
            $(this.config.selector).each(function() {
                self.initializeDatePicker($(this));
                count++;
            });

            this.logger.success('تمام DatePicker ها initialize شدند:', count + ' مورد');

            // تنظیم event handler برای submit
            $('form').on('submit', function(e) {
                var $form = $(this);
                if (!self.prepareFormForSubmit($form)) {
                    e.preventDefault();
                    alert('خطا در تبدیل تاریخ. لطفاً دوباره تلاش کنید.');
                    return false;
                }
            });

            return true;
        }
    };

    /**
     * Auto Initialize
     * Initialize خودکار هنگام لود صفحه
     */
    function autoInitialize() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                PersianDatePickerManager.initializeAll();
            });
        } else {
            PersianDatePickerManager.initializeAll();
        }
    }

    // Export to global scope
    window.PersianDatePickerManager = PersianDatePickerManager;

    // Auto initialize
    autoInitialize();

})(window, jQuery);

