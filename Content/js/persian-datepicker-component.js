/**
 * Persian DatePicker Component
 * کامپوننت اصولی و قابل استفاده مجدد برای Persian DatePicker
 * 
 * اصول طراحی:
 * - Component-Based: کامپوننت محور و قابل استفاده مجدد
 * - Server-Side Today: دریافت تاریخ امروز از سرور برای اطمینان از صحت
 * - Medical Form Standards: طبق استانداردهای فرم‌های درمانی سطح سازمانی
 * - Bulletproof: مقاوم و ضد گلوله
 * - Tested: تست شده و قابل اعتماد
 * 
 * @version 2.0.0
 * @author ClinicApp Team
 * @date 1404/10/04
 */

(function(window, $) {
    'use strict';

    /**
     * Persian DatePicker Component
     * کامپوننت اصلی برای مدیریت Persian DatePicker
     */
    var PersianDatePickerComponent = {
        
        /**
         * Configuration
         * تنظیمات پیش‌فرض
         */
        config: {
            selector: 'input[data-persian-datepicker="true"]',
            hiddenInputSuffix: '_Hidden',
            apiEndpoint: '/api/persian-date/today', // ✅ استفاده از RoutePrefix + Route attribute
            logPrefix: '📅 [PersianDatePicker]',
            enableLogging: true,
            cacheTodayFor: 60000, // 1 دقیقه (میلی‌ثانیه)
            retryDelay: 100, // تأخیر برای retry
            maxRetries: 3
        },

        /**
         * Cache برای تاریخ امروز
         */
        cache: {
            today: null,
            timestamp: null
        },

        /**
         * Logger
         * سیستم لاگ‌گذاری
         */
        logger: {
            log: function(message, data) {
                if (PersianDatePickerComponent.config.enableLogging) {
                    console.log(PersianDatePickerComponent.config.logPrefix, message, data || '');
                }
            },
            error: function(message, error, data) {
                if (PersianDatePickerComponent.config.enableLogging) {
                    console.error(PersianDatePickerComponent.config.logPrefix, '❌', message, error, data || '');
                }
            },
            warn: function(message, data) {
                if (PersianDatePickerComponent.config.enableLogging) {
                    console.warn(PersianDatePickerComponent.config.logPrefix, '⚠️', message, data || '');
                }
            },
            success: function(message, data) {
                if (PersianDatePickerComponent.config.enableLogging) {
                    console.log(PersianDatePickerComponent.config.logPrefix, '✅', message, data || '');
                }
            }
        },

        /**
         * دریافت تاریخ امروز شمسی از سرور
         * این متد تاریخ امروز را از سرور می‌گیرد تا از صحت آن اطمینان حاصل شود
         * 
         * @returns {Promise<string>} - Promise که تاریخ امروز شمسی را برمی‌گرداند
         */
        getTodayFromServer: function() {
            var self = this;
            
            // ✅ بررسی Cache
            if (self.cache.today && self.cache.timestamp) {
                var now = Date.now();
                var cacheAge = now - self.cache.timestamp;
                if (cacheAge < self.config.cacheTodayFor) {
                    self.logger.log('استفاده از Cache برای تاریخ امروز:', self.cache.today);
                    return Promise.resolve(self.cache.today);
                }
            }

            // ✅ دریافت از سرور
            return new Promise(function(resolve, reject) {
                $.ajax({
                    url: self.config.apiEndpoint,
                    method: 'GET',
                    dataType: 'json',
                    cache: false,
                    timeout: 5000,
                    success: function(response) {
                        if (response && response.success && response.persianDate) {
                            // ✅ ذخیره در Cache
                            self.cache.today = response.persianDate;
                            self.cache.timestamp = Date.now();
                            
                            self.logger.success('تاریخ امروز از سرور دریافت شد:', response.persianDate);
                            resolve(response.persianDate);
                        } else {
                            self.logger.error('خطا در دریافت تاریخ امروز از سرور:', null, response);
                            // ✅ Fallback: محاسبه در client-side
                            var fallbackToday = self.calculateTodayClientSide();
                            resolve(fallbackToday);
                        }
                    },
                    error: function(xhr, status, error) {
                        self.logger.error('خطا در درخواست تاریخ امروز:', error, {
                            status: status,
                            xhr: xhr
                        });
                        // ✅ Fallback: محاسبه در client-side
                        var fallbackToday = self.calculateTodayClientSide();
                        resolve(fallbackToday);
                    }
                });
            });
        },

        /**
         * محاسبه تاریخ امروز شمسی در client-side (Fallback)
         * این متد فقط در صورت عدم دسترسی به سرور استفاده می‌شود
         * 
         * @returns {string} - تاریخ امروز شمسی
         */
        calculateTodayClientSide: function() {
            try {
                // ✅ استفاده از jalaali برای تبدیل
                if (typeof jalaali !== 'undefined' && jalaali.toJalaali) {
                    var today = new Date();
                    var jalaaliDate = jalaali.toJalaali(today.getFullYear(), today.getMonth() + 1, today.getDate());
                    
                    var year = String(jalaaliDate.jy).padStart(4, '0');
                    var month = String(jalaaliDate.jm).padStart(2, '0');
                    var day = String(jalaaliDate.jd).padStart(2, '0');
                    
                    var result = year + '/' + month + '/' + day;
                    this.logger.log('تاریخ امروز در client-side محاسبه شد:', result);
                    return result;
                } else {
                    this.logger.warn('jalaali library یافت نشد، استفاده از تاریخ میلادی');
                    var today = new Date();
                    var year = today.getFullYear();
                    var month = String(today.getMonth() + 1).padStart(2, '0');
                    var day = String(today.getDate()).padStart(2, '0');
                    return year + '/' + month + '/' + day;
                }
            } catch (error) {
                this.logger.error('خطا در محاسبه تاریخ امروز در client-side:', error);
                // ✅ آخرین Fallback: تاریخ میلادی
                var today = new Date();
                var year = today.getFullYear();
                var month = String(today.getMonth() + 1).padStart(2, '0');
                var day = String(today.getDate()).padStart(2, '0');
                return year + '/' + month + '/' + day;
            }
        },

        /**
         * Convert Persian/Arabic Numbers to English
         * تبدیل اعداد فارسی/عربی به انگلیسی
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
         * تبدیل تاریخ شمسی به میلادی
         */
        convertPersianToGregorian: function(persianDate) {
            try {
                if (!persianDate || persianDate.trim() === '') {
                    return null;
                }

                var normalizedDate = this.convertPersianToEnglishNumbers(persianDate.trim());
                var parts = normalizedDate.split('/');
                
                if (parts.length !== 3) {
                    this.logger.warn('فرمت تاریخ شمسی نامعتبر:', persianDate);
                    return null;
                }

                var persianYear = parseInt(parts[0], 10);
                var persianMonth = parseInt(parts[1], 10);
                var persianDay = parseInt(parts[2], 10);

                if (isNaN(persianYear) || isNaN(persianMonth) || isNaN(persianDay)) {
                    this.logger.warn('اعداد تاریخ شمسی نامعتبر:', {
                        original: persianDate,
                        normalized: normalizedDate
                    });
                    return null;
                }

                if (typeof jalaali !== 'undefined' && jalaali.toGregorian) {
                    var gregorian = jalaali.toGregorian(persianYear, persianMonth, persianDay);
                    
                    if (!gregorian || !gregorian.gy || !gregorian.gm || !gregorian.gd) {
                        this.logger.error('خطا در تبدیل تاریخ با jalaali:', {
                            persian: persianDate
                        });
                        return null;
                    }
                    
                    var year = gregorian.gy;
                    var month = String(gregorian.gm).padStart(2, '0');
                    var day = String(gregorian.gd).padStart(2, '0');
                    
                    var dateISO = year + '-' + month + '-' + day + 'T00:00:00';
                    
                    this.logger.success('تبدیل موفق:', {
                        persian: persianDate,
                        gregorian: dateISO
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
         * ایجاد یا دریافت hidden input
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
            
            // ✅ برای فرم GET، نیازی به hidden input نیست
            var isGetForm = $form.length > 0 && $form.attr('method') && $form.attr('method').toLowerCase() === 'get';
            
            if ($form.length === 0) {
                this.logger.warn('فرم والد یافت نشد برای:', fieldName);
                // برای input های خارج از form، hidden input ایجاد نمی‌کنیم
                isGetForm = true;
            }

            // ایجاد hidden input فقط برای فرم POST
            var $hiddenInput = null;
            if (!isGetForm) {
                $hiddenInput = this.getOrCreateHiddenInput($form, fieldName);
            }

            // ✅ خواندن مقدار فعلی input (ممکن است از View set شده باشد)
            var currentValue = $input.val();
            this.logger.log('مقدار فعلی input:', {
                field: fieldName,
                currentValue: currentValue,
                hasValue: currentValue && currentValue.trim() !== ''
            });
            
            // اگر input مقدار دارد (مثلاً در Edit form یا از View)، تبدیل کن
            if (currentValue && currentValue.trim() !== '' && $hiddenInput) {
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

            // ✅ دریافت تاریخ امروز از سرور (فقط برای fallback - اگر currentValue خالی باشد)
            // ⚠️ مهم: این فقط برای initialize اولیه است، نه برای reset کردن تاریخ انتخاب شده
            this.getTodayFromServer().then(function(todayPersianDate) {
                // ✅ دوباره چک کردن currentValue (ممکن است در این فاصله set شده باشد)
                var finalCurrentValue = $input.val();
                if (!finalCurrentValue || finalCurrentValue.trim() === '') {
                    finalCurrentValue = currentValue; // استفاده از مقدار اولیه
                }
                
                self.logger.log('مقدار نهایی برای initialValue:', {
                    field: fieldName,
                    finalCurrentValue: finalCurrentValue,
                    todayPersianDate: todayPersianDate,
                    willUseCurrentValue: finalCurrentValue && finalCurrentValue.trim() !== ''
                });
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
                        },
                        todayBtn: {
                            enabled: true,
                            text: { fa: 'امروز' }
                        },
                        clearBtn: {
                            enabled: true,
                            text: { fa: 'پاک کردن' }
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
                    // ✅ تنظیم initialValue: اول از finalCurrentValue استفاده می‌کنیم (تاریخ انتخاب شده از View)
                    // فقط اگر finalCurrentValue خالی باشد، از todayPersianDate استفاده می‌کنیم
                    // ⚠️ مهم: این تضمین می‌کند که تاریخ انتخاب شده از View حفظ می‌شود
                    initialValue: (finalCurrentValue && finalCurrentValue.trim() !== '') 
                        ? self.convertPersianToEnglishNumbers(finalCurrentValue.trim())
                        : (todayPersianDate || false),
                    initialValueType: 'persian',
                    onSelect: function(unix) {
                        // ✅ برای فرم GET، فقط تاریخ شمسی را در input نگه می‌داریم
                        if ($hiddenInput) {
                            self.handleDateSelect($input, $hiddenInput, fieldName, unix);
                        } else {
                            // ✅ برای فرم GET، استفاده مستقیم از تاریخ انتخاب شده از datePicker instance
                            // ⚠️ مهم: نباید از $input.val() استفاده کنیم چون ممکن است هنوز به‌روز نشده باشد
                            var persianDateStr = null;
                            
                            try {
                                // ✅ استفاده از datePicker instance برای دریافت تاریخ انتخاب شده
                                var datePickerInstance = $input.data('pDatepicker');
                                if (datePickerInstance) {
                                    // روش 1: استفاده از selected object (jy, jm, jd) - بهترین روش
                                    var selected = datePickerInstance.selected;
                                    if (selected && 
                                        typeof selected.jy === 'number' && 
                                        typeof selected.jm === 'number' && 
                                        typeof selected.jd === 'number' &&
                                        selected.jy > 0 && 
                                        selected.jm >= 1 && selected.jm <= 12 && 
                                        selected.jd >= 1 && selected.jd <= 31) {
                                        
                                        var year = String(selected.jy).padStart(4, '0');
                                        var month = String(selected.jm).padStart(2, '0');
                                        var day = String(selected.jd).padStart(2, '0');
                                        persianDateStr = year + '/' + month + '/' + day;
                                        
                                        if (persianDateStr.match(/^\d{4}\/\d{2}\/\d{2}$/)) {
                                            self.logger.success('تاریخ انتخاب شد (فرم GET) - از selected object:', {
                                                field: fieldName,
                                                persian: persianDateStr
                                            });
                                        }
                                    }
                                    
                                    // روش 2: استفاده از getFormattedDate (fallback)
                                    if (!persianDateStr) {
                                        var formattedDate = datePickerInstance.getFormattedDate('YYYY/MM/DD');
                                        if (formattedDate && 
                                            typeof formattedDate === 'string' && 
                                            formattedDate.includes('/') && 
                                            formattedDate.match(/^\d{4}\/\d{2}\/\d{2}$/)) {
                                            persianDateStr = formattedDate;
                                            self.logger.success('تاریخ انتخاب شد (فرم GET) - از getFormattedDate:', {
                                                field: fieldName,
                                                persian: persianDateStr
                                            });
                                        }
                                    }
                                }
                                
                                // روش 3: استفاده از input value (آخرین fallback)
                                if (!persianDateStr) {
                                    persianDateStr = $input.val();
                                    if (persianDateStr) {
                                        persianDateStr = self.convertPersianToEnglishNumbers(persianDateStr);
                                        if (persianDateStr.match(/^\d{4}\/\d{2}\/\d{2}$/)) {
                                            self.logger.success('تاریخ انتخاب شد (فرم GET) - از input value:', {
                                                field: fieldName,
                                                persian: persianDateStr
                                            });
                                        } else {
                                            persianDateStr = null;
                                        }
                                    }
                                }
                            } catch (error) {
                                self.logger.error('خطا در دریافت تاریخ از datePicker:', error, {
                                    field: fieldName
                                });
                                // Fallback: استفاده از input value
                                persianDateStr = $input.val();
                            }
                            
                            // ✅ اگر تاریخ معتبر پیدا نشد، لاگ warning
                            if (!persianDateStr || !persianDateStr.match(/^\d{4}\/\d{2}\/\d{2}$/)) {
                                self.logger.warn('تاریخ معتبر پیدا نشد در onSelect:', {
                                    field: fieldName,
                                    inputValue: $input.val()
                                });
                            }
                        }
                    }
                };

                $input.pDatepicker(datePickerConfig);

                // Mark as initialized
                $input.data('pDatepicker-initialized', true);
                self.logger.success('DatePicker initialize شد:', fieldName);
            }).catch(function(error) {
                self.logger.error('خطا در دریافت تاریخ امروز:', error);
                // ✅ Fallback: initialize بدون تاریخ امروز
                var datePickerConfig = {
                    calendarType: 'persian',
                    format: 'YYYY/MM/DD',
                    autoClose: true,
                    observer: true,
                    timePicker: { enabled: false },
                    toolbox: {
                        calendarSwitch: { enabled: false },
                        todayBtn: { enabled: true, text: { fa: 'امروز' } },
                        clearBtn: { enabled: true, text: { fa: 'پاک کردن' } }
                    },
                    navigator: { enabled: true },
                    onlyTimePicker: false,
                    onlySelectOnDate: true,
                    calendar: {
                        persian: {
                            enabled: true,
                            locale: 'fa'
                        }
                    },
                    initialValue: currentValue && currentValue.trim() !== '' 
                        ? self.convertPersianToEnglishNumbers(currentValue.trim())
                        : false,
                    initialValueType: 'persian',
                    onSelect: function(unix) {
                        // ✅ برای فرم GET، فقط تاریخ شمسی را در input نگه می‌داریم
                        if ($hiddenInput) {
                            self.handleDateSelect($input, $hiddenInput, fieldName, unix);
                        } else {
                            // ✅ برای فرم GET، استفاده مستقیم از تاریخ انتخاب شده از datePicker instance
                            // ⚠️ مهم: نباید از $input.val() استفاده کنیم چون ممکن است هنوز به‌روز نشده باشد
                            var persianDateStr = null;
                            
                            try {
                                // ✅ استفاده از datePicker instance برای دریافت تاریخ انتخاب شده
                                var datePickerInstance = $input.data('pDatepicker');
                                if (datePickerInstance) {
                                    // روش 1: استفاده از selected object (jy, jm, jd) - بهترین روش
                                    var selected = datePickerInstance.selected;
                                    if (selected && 
                                        typeof selected.jy === 'number' && 
                                        typeof selected.jm === 'number' && 
                                        typeof selected.jd === 'number' &&
                                        selected.jy > 0 && 
                                        selected.jm >= 1 && selected.jm <= 12 && 
                                        selected.jd >= 1 && selected.jd <= 31) {
                                        
                                        var year = String(selected.jy).padStart(4, '0');
                                        var month = String(selected.jm).padStart(2, '0');
                                        var day = String(selected.jd).padStart(2, '0');
                                        persianDateStr = year + '/' + month + '/' + day;
                                        
                                        if (persianDateStr.match(/^\d{4}\/\d{2}\/\d{2}$/)) {
                                            self.logger.success('تاریخ انتخاب شد (فرم GET) - از selected object:', {
                                                field: fieldName,
                                                persian: persianDateStr
                                            });
                                        }
                                    }
                                    
                                    // روش 2: استفاده از getFormattedDate (fallback)
                                    if (!persianDateStr) {
                                        var formattedDate = datePickerInstance.getFormattedDate('YYYY/MM/DD');
                                        if (formattedDate && 
                                            typeof formattedDate === 'string' && 
                                            formattedDate.includes('/') && 
                                            formattedDate.match(/^\d{4}\/\d{2}\/\d{2}$/)) {
                                            persianDateStr = formattedDate;
                                            self.logger.success('تاریخ انتخاب شد (فرم GET) - از getFormattedDate:', {
                                                field: fieldName,
                                                persian: persianDateStr
                                            });
                                        }
                                    }
                                }
                                
                                // روش 3: استفاده از input value (آخرین fallback)
                                if (!persianDateStr) {
                                    persianDateStr = $input.val();
                                    if (persianDateStr) {
                                        persianDateStr = self.convertPersianToEnglishNumbers(persianDateStr);
                                        if (persianDateStr.match(/^\d{4}\/\d{2}\/\d{2}$/)) {
                                            self.logger.success('تاریخ انتخاب شد (فرم GET) - از input value:', {
                                                field: fieldName,
                                                persian: persianDateStr
                                            });
                                        } else {
                                            persianDateStr = null;
                                        }
                                    }
                                }
                            } catch (error) {
                                self.logger.error('خطا در دریافت تاریخ از datePicker:', error, {
                                    field: fieldName
                                });
                                // Fallback: استفاده از input value
                                persianDateStr = $input.val();
                            }
                            
                            // ✅ اگر تاریخ معتبر پیدا نشد، لاگ warning
                            if (!persianDateStr || !persianDateStr.match(/^\d{4}\/\d{2}\/\d{2}$/)) {
                                self.logger.warn('تاریخ معتبر پیدا نشد در onSelect:', {
                                    field: fieldName,
                                    inputValue: $input.val()
                                });
                            }
                        }
                    }
                };

                $input.pDatepicker(datePickerConfig);
                $input.data('pDatepicker-initialized', true);
            });
        },

        /**
         * Handle Date Select
         * مدیریت انتخاب تاریخ
         */
        handleDateSelect: function($input, $hiddenInput, fieldName, unix) {
            try {
                var persianDateStr = $input.val();
                
                if (!persianDateStr || persianDateStr.trim() === '') {
                    if ($hiddenInput) {
                        $hiddenInput.val('');
                    }
                    this.logger.warn('تاریخ خالی انتخاب شد:', fieldName);
                    return;
                }

                // ✅ فقط برای فرم POST، hidden input را تنظیم می‌کنیم
                if ($hiddenInput) {
                    var gregorianDate = this.convertPersianToGregorian(persianDateStr);
                    
                    if (gregorianDate) {
                        $hiddenInput.val(gregorianDate);
                        this.logger.success('تاریخ انتخاب و تبدیل شد:', {
                            field: fieldName,
                            persian: persianDateStr,
                            gregorian: gregorianDate
                        });
                    } else {
                        this.logger.error('خطا در تبدیل تاریخ', null, {
                            field: fieldName,
                            persian: persianDateStr
                        });
                        $hiddenInput.val('');
                    }
                } else {
                    // برای فرم GET، فقط لاگ می‌کنیم
                    this.logger.success('تاریخ انتخاب شد (فرم GET):', {
                        field: fieldName,
                        persian: persianDateStr
                    });
                }
            } catch (error) {
                this.logger.error('خطا در handleDateSelect', error, fieldName);
                if ($hiddenInput) {
                    $hiddenInput.val('');
                }
            }
        },

        /**
         * Prepare Form for Submit
         * آماده‌سازی فرم قبل از submit
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
                
                if (!persianDate || persianDate.trim() === '') {
                    $hiddenInput.val('');
                    self.logger.log('تاریخ خالی:', fieldName);
                } else {
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
                    }
                }
                
                $input.prop('disabled', true);
            });

            return !hasError;
        },

        /**
         * Initialize All DatePickers
         * Initialize کردن تمام DatePicker ها
         */
        initializeAll: function() {
            var self = this;
            
            if (typeof jQuery === 'undefined') {
                this.logger.error('jQuery یافت نشد');
                return false;
            }

            if (typeof $.fn.pDatepicker === 'undefined') {
                this.logger.warn('pDatepicker یافت نشد، تلاش مجدد...');
                setTimeout(function() {
                    self.initializeAll();
                }, this.config.retryDelay);
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
                PersianDatePickerComponent.initializeAll();
            });
        } else {
            PersianDatePickerComponent.initializeAll();
        }
    }

    // Export to global scope
    window.PersianDatePickerComponent = PersianDatePickerComponent;

    // Auto initialize
    autoInitialize();

})(window, jQuery);

