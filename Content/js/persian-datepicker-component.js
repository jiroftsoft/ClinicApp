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
            retryDelay: 100, // تأخیر برای retry (میلی‌ثانیه)
            maxRetries: 30 // ✅ CRITICAL FIX: افزایش تعداد تلاش‌ها (30 * 100ms = 3 ثانیه)
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
                // ✅ CRITICAL FIX: محاسبه تاریخ «ایران» مستقل از timezone کاربر
                // محاسبه UTC + offset ایران (+03:30)
                var now = new Date();
                var utcMs = now.getTime() + (now.getTimezoneOffset() * 60000);
                var iranMs = utcMs + (210 * 60000); // +03:30 = 3.5 * 60 * 1000 = 210 minutes
                var iranDate = new Date(iranMs);
                
                // ✅ استفاده از jalaali برای تبدیل
                if (typeof jalaali !== 'undefined' && jalaali.toJalaali) {
                    var jalaaliDate = jalaali.toJalaali(
                        iranDate.getUTCFullYear(), 
                        iranDate.getUTCMonth() + 1, 
                        iranDate.getUTCDate()
                    );
                    
                    var year = String(jalaaliDate.jy).padStart(4, '0');
                    var month = String(jalaaliDate.jm).padStart(2, '0');
                    var day = String(jalaaliDate.jd).padStart(2, '0');
                    
                    var result = year + '/' + month + '/' + day;
                    this.logger.log('تاریخ امروز ایران در client-side محاسبه شد:', result);
                    return result;
                } else {
                    this.logger.warn('jalaali library یافت نشد، استفاده از تاریخ میلادی ایران');
                    var year = iranDate.getUTCFullYear();
                    var month = String(iranDate.getUTCMonth() + 1).padStart(2, '0');
                    var day = String(iranDate.getUTCDate()).padStart(2, '0');
                    return year + '/' + month + '/' + day;
                }
            } catch (error) {
                this.logger.error('خطا در محاسبه تاریخ امروز در client-side:', error);
                // ✅ آخرین Fallback: تاریخ میلادی ایران
                var now = new Date();
                var utcMs = now.getTime() + (now.getTimezoneOffset() * 60000);
                var iranMs = utcMs + (210 * 60000);
                var iranDate = new Date(iranMs);
                var year = iranDate.getUTCFullYear();
                var month = String(iranDate.getUTCMonth() + 1).padStart(2, '0');
                var day = String(iranDate.getUTCDate()).padStart(2, '0');
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

            // ✅ CRITICAL FIX: بررسی data-no-default-date attribute
            // اگر این attribute وجود داشته باشد، نباید تاریخ امروز را به عنوان پیش‌فرض نمایش دهیم
            var noDefaultDate = $input.attr('data-no-default-date') === 'true';
            
            // ✅ CRITICAL FIX: همیشه تاریخ امروز از سرور را دریافت کن (حتی اگر noDefaultDate true باشد)
            // دلیل: برای override کردن highlight اشتباه DatePicker (16 به جای 15)
            // اما اگر noDefaultDate true باشد، از آن به عنوان initialValue استفاده نمی‌کنیم
            var todayPromise = this.getTodayFromServer();
            
            todayPromise.then(function(todayPersianDate) {
                // ✅ دوباره چک کردن currentValue (ممکن است در این فاصله set شده باشد)
                // ⚠️ CRITICAL: اگر noDefaultDate true باشد، نباید از input value استفاده کنیم
                // چون DatePicker ممکن است تاریخ اشتباه (16) را set کرده باشد
                var finalCurrentValue = currentValue; // استفاده از مقدار اولیه (قبل از initialize)
                if (!finalCurrentValue || finalCurrentValue.trim() === '') {
                    // فقط اگر مقدار اولیه خالی باشد و noDefaultDate false باشد، از input value استفاده کن
                    var inputVal = $input.val();
                    if (inputVal && inputVal.trim() !== '' && !noDefaultDate) {
                        finalCurrentValue = inputVal;
                    }
                }
                
                // ✅ Log فقط برای debug (کاهش لاگ‌ها)
                if (PersianDatePickerComponent.config.enableLogging && (!noDefaultDate || finalCurrentValue)) {
                    self.logger.log('مقدار نهایی برای initialValue:', {
                        field: fieldName,
                        finalCurrentValue: finalCurrentValue,
                        todayPersianDate: todayPersianDate,
                        noDefaultDate: noDefaultDate,
                        willUseCurrentValue: finalCurrentValue && finalCurrentValue.trim() !== ''
                    });
                }
                
                // ✅ CRITICAL FIX: اگر noDefaultDate true باشد، initialValue باید false باشد
                // این تضمین می‌کند که تاریخ پیش‌فرض (15) نمایش داده نشود
                var initialValueToUse = false;
                if (finalCurrentValue && finalCurrentValue.trim() !== '') {
                    // اگر مقدار از View آمده، استفاده کن
                    initialValueToUse = self.convertPersianToEnglishNumbers(finalCurrentValue.trim());
                } else if (!noDefaultDate && todayPersianDate) {
                    // فقط اگر noDefaultDate false باشد و todayPersianDate موجود باشد
                    initialValueToUse = todayPersianDate;
                }
                
                // ✅ CRITICAL FIX: محاسبه minDate از تاریخ سرور (برای جلوگیری از انتخاب تاریخ‌های گذشته)
                var minDate = null;
                if (todayPersianDate) {
                    var gregorianTodayStr = self.convertPersianToGregorian(todayPersianDate);
                    if (gregorianTodayStr) {
                        // تبدیل ISO string به Date object
                        minDate = new Date(gregorianTodayStr);
                        if (isNaN(minDate.getTime())) {
                            minDate = null;
                        }
                    }
                }
                
                // Initialize pDatepicker
                var datePickerConfig = {
                    calendarType: 'persian',
                    format: 'YYYY/MM/DD',
                    autoClose: true,
                    // ✅ CRITICAL FIX: اگر noDefaultDate true باشد، observer باید false باشد
                    // چون observer باعث می‌شود DatePicker خودش input را parse کند و تاریخ اشتباه (16) را set کند
                    observer: !noDefaultDate, // false اگر noDefaultDate true باشد
                    // ✅ CRITICAL FIX: استفاده از minDate برای جلوگیری از انتخاب تاریخ‌های گذشته
                    minDate: minDate,
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
                    }
                };
                
                // ✅ CRITICAL FIX: طبق مستندات Persian DatePicker (https://babakhani.github.io/PersianWebToolkit/doc/datepicker/options/)
                // برای جلوگیری از نمایش تاریخ پیش‌فرض، باید initialValue: false باشد
                // ⚠️ مهم: طبق مستندات، initialValue: false به معنای "بدون مقدار اولیه" است
                var finalInitialValue = false;
                if (initialValueToUse) {
                    // ✅ اگر مقدار از View آمده، استفاده کن
                    finalInitialValue = initialValueToUse;
                } else if (!noDefaultDate && todayPersianDate) {
                    // ✅ فقط اگر noDefaultDate false باشد و todayPersianDate موجود باشد
                    // استفاده از تاریخ سرور برای highlight (نه client-side calculation)
                    finalInitialValue = todayPersianDate;
                }
                // ✅ اگر noDefaultDate true باشد، finalInitialValue باید false بماند
                
                // ✅ Set finalInitialValue در datePickerConfig (طبق مستندات)
                datePickerConfig.initialValue = finalInitialValue;
                datePickerConfig.initialValueType = finalInitialValue ? 'persian' : undefined;
                
                // ✅ CRITICAL: Log فقط برای debug (کاهش لاگ‌ها)
                if (PersianDatePickerComponent.config.enableLogging && (!noDefaultDate || finalInitialValue)) {
                    self.logger.log('🔍 [DEBUG] finalInitialValue set شد:', {
                        field: fieldName,
                        finalInitialValue: finalInitialValue,
                        initialValueToUse: initialValueToUse,
                        todayPersianDate: todayPersianDate,
                        noDefaultDate: noDefaultDate,
                        willUseServerDate: !noDefaultDate && todayPersianDate
                    });
                }
                
                // ✅ BEST PRACTICE: استفاده از onShow callback (event-driven approach)
                // این callback زمانی فراخوانی می‌شود که DatePicker باز می‌شود
                var originalOnShow = datePickerConfig.onShow;
                datePickerConfig.onShow = function() {
                    // ✅ Call original onShow
                    if (typeof originalOnShow === 'function') {
                        originalOnShow.call(this);
                    }
                    
                    // ✅ CRITICAL FIX: اگر noDefaultDate true باشد، باید highlight را clear کنیم
                    // طبق مستندات API: استفاده از setDate(null) و getState() برای clear کردن
                    if (noDefaultDate && !initialValueToUse) {
                        // ✅ Clear کردن فوری (بدون setTimeout)
                        try {
                            var datePickerInstance = $input.data('pDatepicker');
                            if (datePickerInstance) {
                                // ✅ استفاده از API: setDate(null) برای clear کردن (طبق مستندات)
                                if (typeof datePickerInstance.setDate === 'function') {
                                    datePickerInstance.setDate(null);
                                }
                                
                                // ✅ Clear کردن input value
                                $input.val('');
                            }
                        } catch (error) {
                            self.logger.warn('خطا در onShow clear highlight (فوری):', error);
                        }
                        
                        // ✅ Clear کردن با تاخیر (برای اطمینان کامل)
                        setTimeout(function() {
                            try {
                                var datePickerInstance = $input.data('pDatepicker');
                                if (datePickerInstance) {
                                    // ✅ استفاده از API: setDate(null) برای clear کردن (طبق مستندات)
                                    if (typeof datePickerInstance.setDate === 'function') {
                                        datePickerInstance.setDate(null);
                                    }
                                    
                                    // ✅ Clear کردن input value
                                    $input.val('');
                                    
                                    // ✅ استفاده از getState() برای بررسی state (طبق مستندات API)
                                    if (typeof datePickerInstance.getState === 'function') {
                                        var state = datePickerInstance.getState();
                                        if (state && state.selected && state.selected.unixDate) {
                                            // ✅ اگر هنوز تاریخ set شده است، دوباره clear کن
                                            datePickerInstance.setDate(null);
                                            $input.val('');
                                        }
                                    }
                                    
                                    // ✅ حذف class های highlight از تقویم (برای اطمینان کامل)
                                    // طبق مستندات، باید از container استفاده کنیم
                                    var $calendar = datePickerInstance.$container || $(datePickerInstance.container || '.pdp-container').last();
                                    if ($calendar.length > 0) {
                                        // ✅ حذف class های selected از تمام روزها
                                        $calendar.find('td[data-unix], .pdp-day-selected, .selected')
                                            .removeClass('pdp-day-selected selected')
                                            .removeAttr('data-selected');
                                        
                                        // ✅ حذف class های highlight
                                        $calendar.find('.pdp-day-today, .today, .pdp-selected, .pdp-today')
                                            .removeClass('pdp-day-today today pdp-selected pdp-today');
                                        
                                        // ✅ حذف attribute های selected
                                        $calendar.find('[data-selected="true"]')
                                            .attr('data-selected', 'false');
                                    }
                                }
                            } catch (error) {
                                self.logger.warn('خطا در onShow clear highlight:', error);
                            }
                        }, 100);
                    } else if (todayPersianDate && !noDefaultDate) {
                        // ✅ Override highlight با تاریخ سرور (فقط اگر noDefaultDate false باشد)
                        setTimeout(function() {
                            try {
                                var datePickerInstance = $input.data('pDatepicker');
                                if (datePickerInstance) {
                                    var gregorianDateStr = self.convertPersianToGregorian(todayPersianDate);
                                    if (gregorianDateStr) {
                                        var gregorianDate = new Date(gregorianDateStr);
                                        if (!isNaN(gregorianDate.getTime())) {
                                            // ✅ فقط highlight می‌کنیم، اما input value را set نمی‌کنیم
                                            // چون اگر initialValue false باشد، نباید input value set شود
                                            if (typeof datePickerInstance.setDate === 'function') {
                                                datePickerInstance.setDate(gregorianDate);
                                            }
                                        }
                                    }
                                }
                            } catch (error) {
                                self.logger.warn('خطا در onShow override highlight:', error);
                            }
                        }, 100);
                    }
                };
                
                // ✅ CRITICAL: تعریف flags در scope بالاتر برای دسترسی در onSelect و onSet
                var isUserSelection = false; // ✅ Flag برای تشخیص انتخاب user (در onSelect set می‌شود)
                var isInitializing = true; // ✅ Flag برای تشخیص initialize شدن
                var initializationCompleteTime = null; // ✅ زمان تکمیل initialization
                var allowSelection = false; // ✅ Flag برای اجازه دادن به انتخاب (بعد از initialize کامل)
                
                // ✅ اضافه کردن onSelect callback (طبق مستندات: https://babakhani.github.io/PersianWebToolkit/doc/datepicker/options/)
                datePickerConfig.onSelect = function(unix) {
                    // ✅ CRITICAL FIX: طبق مستندات، onSelect زمانی فراخوانی می‌شود که user تاریخ را انتخاب کند
                    // اما DatePicker در initialization خودش onSelect را فراخوانی می‌کند
                    // باید این را ignore کنیم تا تاریخ اشتباه (16) set نشود
                    
                    // ✅ CRITICAL: اگر noDefaultDate true باشد، باید تمام انتخاب‌های خودکار را ignore کنیم
                    if (noDefaultDate && !initialValueToUse) {
                        // ✅ بررسی اینکه آیا این انتخاب خودکار است یا نه
                        var now = Date.now();
                        var timeSinceInit = initializationCompleteTime ? (now - initializationCompleteTime) : 0;
                        var isAutoSelection = !allowSelection || isInitializing || timeSinceInit < 2000;
                        
                        if (isAutoSelection && !isUserSelection) {
                            // ✅ این یک انتخاب خودکار است - ignore کن
                            self.logger.log('⚠️ انتخاب خودکار ignore شد:', {
                                field: fieldName,
                                unix: unix,
                                allowSelection: allowSelection,
                                isInitializing: isInitializing,
                                timeSinceInit: timeSinceInit
                            });
                            
                            // ✅ Clear کردن فوری
                            $input.val('');
                            var datePickerInstance = $input.data('pDatepicker');
                            if (datePickerInstance) {
                                // ✅ استفاده از API: setDate(null) برای clear کردن (طبق مستندات)
                                if (typeof datePickerInstance.setDate === 'function') {
                                    datePickerInstance.setDate(null);
                                }
                                
                                // ✅ استفاده از getState() برای بررسی state (طبق مستندات API)
                                if (typeof datePickerInstance.getState === 'function') {
                                    var state = datePickerInstance.getState();
                                    if (state && state.selected && state.selected.unixDate) {
                                        // ✅ اگر هنوز تاریخ set شده است، دوباره clear کن
                                        datePickerInstance.setDate(null);
                                        $input.val('');
                                    }
                                }
                                
                                // ✅ حذف class های highlight از تقویم
                                var $calendar = datePickerInstance.$container || $(datePickerInstance.container || '.pdp-container').last();
                                if ($calendar.length > 0) {
                                    $calendar.find('td[data-unix], .pdp-day-selected, .selected')
                                        .removeClass('pdp-day-selected selected')
                                        .removeAttr('data-selected');
                                }
                            }
                            return; // ✅ جلوگیری از ادامه execution
                        }
                    }
                    
                    // ✅ CRITICAL FIX: Set flag برای تشخیص انتخاب user
                    isUserSelection = true;
                    isInitializing = false; // ✅ Initialize تمام شده است
                    
                    // ✅ CRITICAL FIX: Trigger custom event برای date-selection.js
                    // این event باید trigger شود تا date-selection.js بتواند تاریخ را پردازش کند
                    var eventData = {
                        unix: unix,
                        selected: null
                    };
                    
                    try {
                        // ✅ دریافت selected object از datePicker instance
                        var datePickerInstance = $input.data('pDatepicker');
                        if (datePickerInstance && datePickerInstance.selected) {
                            eventData.selected = datePickerInstance.selected;
                        }
                    } catch (e) {
                        self.logger.warn('خطا در دریافت selected object:', e);
                    }
                    
                    // ✅ CRITICAL FIX: Trigger custom event برای date-selection.js
                    // استفاده از jQuery.Event برای pass کردن data به درستی
                    var customEvent = $.Event('pDatepicker:select');
                    customEvent.unix = unix;
                    customEvent.selected = eventData.selected;
                    $input.trigger(customEvent, eventData);
                    
                    // ✅ Trigger change event برای fallback handlers
                    setTimeout(function() {
                        $input.trigger('change');
                    }, 50);
                    
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
                };

                // ✅ BEST PRACTICE: استفاده از onSet callback برای جلوگیری از set شدن خودکار تاریخ
                // طبق مستندات: onSet زمانی فراخوانی می‌شود که تاریخ از طریق API (نه user selection) set شود
                // ⚠️ توجه: isUserSelection و isInitializing در بالا تعریف شده‌اند
                var originalOnSet = datePickerConfig.onSet;
                datePickerConfig.onSet = function(unix) {
                    // ✅ CRITICAL FIX: اگر noDefaultDate true باشد و این set شدن خودکار است، ignore کن
                    // onSet معمولاً در initialization فراخوانی می‌شود
                    if (noDefaultDate && !initialValueToUse && (!allowSelection || isInitializing || !isUserSelection)) {
                        // ✅ این یک set شدن خودکار است - ignore کن
                        self.logger.log('⚠️ onSet خودکار ignore شد:', {
                            field: fieldName,
                            unix: unix,
                            allowSelection: allowSelection,
                            isInitializing: isInitializing,
                            isUserSelection: isUserSelection
                        });
                        
                        // ✅ Clear کردن فوری
                        $input.val('');
                        var datePickerInstance = $input.data('pDatepicker');
                        if (datePickerInstance && typeof datePickerInstance.setDate === 'function') {
                            datePickerInstance.setDate(null);
                        }
                        return; // ✅ جلوگیری از ادامه execution
                    }
                    
                    // ✅ Reset flag
                    isUserSelection = false;
                    
                    // ✅ Call original onSet اگر وجود دارد
                    if (typeof originalOnSet === 'function') {
                        originalOnSet.call(this, unix);
                    }
                };
                
                // ✅ BEST PRACTICE: استفاده از readonly attribute برای جلوگیری از manual input
                // این تضمین می‌کند که user نمی‌تواند تاریخ را manually وارد کند
                // ⚠️ مهم: فقط اگر observer false باشد، readonly را set کن
                // چون اگر observer true باشد، user باید بتواند تایپ کند
                if (noDefaultDate && !initialValueToUse && !datePickerConfig.observer) {
                    $input.attr('readonly', 'readonly');
                }
                
                // ✅ Initialize DatePicker (طبق مستندات: https://babakhani.github.io/PersianWebToolkit/doc/datepicker/api/)
                $input.pDatepicker(datePickerConfig);
                
                // ✅ Set initialization complete time (برای تشخیص انتخاب خودکار)
                initializationCompleteTime = Date.now();
                
                // ✅ CRITICAL FIX: بعد از initialize کامل، allowSelection را true کن
                // این تضمین می‌کند که فقط انتخاب‌های واقعی user پردازش شوند
                // ⚠️ مهم: باید تاخیر بیشتری بگذاریم تا مطمئن شویم که تمام انتخاب‌های خودکار ignore شده‌اند
                setTimeout(function() {
                    allowSelection = true;
                    isInitializing = false;
                    self.logger.log('✅ Initialize کامل شد - انتخاب‌ها فعال شدند:', {
                        field: fieldName,
                        noDefaultDate: noDefaultDate
                    });
                }, 2000); // ✅ 2 ثانیه تاخیر برای اطمینان از initialize کامل و ignore کردن تمام انتخاب‌های خودکار
                
                // ✅ CRITICAL FIX: طبق مستندات API (https://babakhani.github.io/PersianWebToolkit/doc/datepicker/api/)
                // استفاده از setDate(null) برای clear کردن تاریخ بلافاصله بعد از initialize
                // این تضمین می‌کند که اگر DatePicker خودش تاریخ را set کرد، بلافاصله clear شود
                if (noDefaultDate && !initialValueToUse) {
                    // ✅ Clear بلافاصله بعد از initialize (طبق API: setDate(unix))
                    setTimeout(function() {
                        try {
                            var datePickerInstance = $input.data('pDatepicker');
                            if (datePickerInstance) {
                                // ✅ استفاده از API: setDate(null) برای clear کردن (طبق مستندات)
                                if (typeof datePickerInstance.setDate === 'function') {
                                    datePickerInstance.setDate(null);
                                }
                                
                                // ✅ Clear input value
                                $input.val('');
                                
                                // ✅ استفاده از getState() برای بررسی state (طبق مستندات API)
                                if (typeof datePickerInstance.getState === 'function') {
                                    var state = datePickerInstance.getState();
                                    if (state && state.selected && state.selected.unixDate) {
                                        // ✅ اگر هنوز تاریخ set شده است، دوباره clear کن
                                        datePickerInstance.setDate(null);
                                        $input.val('');
                                    }
                                }
                            }
                        } catch (e) {
                            self.logger.warn('خطا در clear کردن تاریخ بعد از initialize:', e);
                        }
                    }, 0);
                    
                    // ✅ چندین بار تلاش برای clear کردن (برای اطمینان کامل - ضد گلوله)
                    // این برای handle کردن case هایی است که DatePicker دوباره تاریخ را set می‌کند
                    var clearAttempts = [50, 100, 200, 300, 500, 1000, 1500];
                    clearAttempts.forEach(function(delay) {
                        setTimeout(function() {
                            try {
                                var datePickerInstance = $input.data('pDatepicker');
                                if (datePickerInstance) {
                                    var currentVal = $input.val();
                                    // ✅ اگر تاریخ set شده است، clear کن
                                    if (currentVal && currentVal.trim() !== '') {
                                        // ✅ استفاده از API: setDate(null) (طبق مستندات)
                                        if (typeof datePickerInstance.setDate === 'function') {
                                            datePickerInstance.setDate(null);
                                        }
                                        $input.val('');
                                        
                                        // ✅ بررسی state با getState() (طبق مستندات API)
                                        if (typeof datePickerInstance.getState === 'function') {
                                            var state = datePickerInstance.getState();
                                            if (state && state.selected && state.selected.unixDate) {
                                                datePickerInstance.setDate(null);
                                                $input.val('');
                                            }
                                        }
                                    }
                                    
                                    // ✅ حذف class های highlight از تقویم (برای اطمینان کامل)
                                    var $calendar = datePickerInstance.$container || $(datePickerInstance.container || '.pdp-container').last();
                                    if ($calendar.length > 0) {
                                        // ✅ حذف class های selected از تمام روزها
                                        $calendar.find('td[data-unix], .pdp-day-selected, .selected')
                                            .removeClass('pdp-day-selected selected')
                                            .removeAttr('data-selected');
                                        
                                        // ✅ حذف class های highlight
                                        $calendar.find('.pdp-day-today, .today, .pdp-selected, .pdp-today')
                                            .removeClass('pdp-day-today today pdp-selected pdp-today');
                                        
                                        // ✅ حذف attribute های selected
                                        $calendar.find('[data-selected="true"]')
                                            .attr('data-selected', 'false');
                                    }
                                }
                            } catch (clearError) {
                                // Silent fail
                            }
                        }, delay);
                    });
                    
                    // ✅ CRITICAL FIX: استفاده از event listener برای detect کردن تغییرات خودکار در input value
                    // این یک لایه محافظ اضافی است برای جلوگیری از set شدن خودکار تاریخ
                    // طبق مستندات، باید از event listener استفاده کنیم نه MutationObserver
                    var inputChangeHandler = function() {
                        if (!allowSelection && noDefaultDate && !initialValueToUse) {
                            var currentVal = $input.val();
                            if (currentVal && currentVal.trim() !== '') {
                                // ✅ تاریخ خودکار set شده است - clear کن
                                self.logger.log('⚠️ تاریخ خودکار detect شد و clear شد:', {
                                    field: fieldName,
                                    value: currentVal
                                });
                                $input.val('');
                                var datePickerInstance = $input.data('pDatepicker');
                                if (datePickerInstance && typeof datePickerInstance.setDate === 'function') {
                                    datePickerInstance.setDate(null);
                                }
                            }
                        }
                    };
                    
                    // ✅ Listen کردن به تغییرات در input value
                    $input.on('input change', inputChangeHandler);
                    
                    // ✅ بعد از 1.5 ثانیه، event listener را remove کن
                    setTimeout(function() {
                        $input.off('input change', inputChangeHandler);
                    }, 1500);
                }
                
                // ✅ CRITICAL: Log برای debug - بررسی اینکه initialValue درست set شده
                setTimeout(function() {
                    var datePickerInstance = $input.data('pDatepicker');
                    if (datePickerInstance) {
                        self.logger.log('🔍 [DEBUG] DatePicker initialize شد - بررسی initialValue:', {
                            field: fieldName,
                            finalInitialValue: finalInitialValue,
                            datePickerConfigInitialValue: datePickerConfig.initialValue,
                            datePickerConfigInitialValueType: datePickerConfig.initialValueType,
                            inputValue: $input.val(),
                            todayPersianDate: todayPersianDate,
                            noDefaultDate: noDefaultDate
                        });
                    }
                }, 50);

                // ✅ NOTE: onShow callback قبلاً در خط 457 تعریف شده است

                // ✅ NOTE: Clear کردن تاریخ بعد از initialize در خط 720 انجام شده است
                // این بخش برای clear کردن highlight در تقویم است (بعد از render شدن کامل)
                if (noDefaultDate && !initialValueToUse) {
                    // ✅ Clear کردن highlight در تقویم (بعد از render شدن کامل)
                    setTimeout(function() {
                        try {
                            var datePickerInstance = $input.data('pDatepicker');
                            if (datePickerInstance) {
                                // ✅ استفاده از getState() برای بررسی state (طبق مستندات API)
                                if (typeof datePickerInstance.getState === 'function') {
                                    var state = datePickerInstance.getState();
                                    if (state && state.selected && state.selected.unixDate) {
                                        // ✅ اگر هنوز تاریخ set شده است، clear کن
                                        datePickerInstance.setDate(null);
                                        $input.val('');
                                    }
                                }
                                
                                    // ✅ حذف class های highlight از تقویم (برای اطمینان کامل)
                                    // طبق مستندات، باید از container استفاده کنیم
                                    var $calendar = datePickerInstance.$container || $(datePickerInstance.container || '.pdp-container').last();
                                    if ($calendar.length > 0) {
                                        // ✅ حذف class های selected از تمام روزها
                                        $calendar.find('td[data-unix], .pdp-day-selected, .selected')
                                            .removeClass('pdp-day-selected selected')
                                            .removeAttr('data-selected');
                                        
                                        // ✅ حذف class های highlight
                                        $calendar.find('.pdp-day-today, .today, .pdp-selected, .pdp-today')
                                            .removeClass('pdp-day-today today pdp-selected pdp-today');
                                        
                                        // ✅ حذف attribute های selected
                                        $calendar.find('[data-selected="true"]')
                                            .attr('data-selected', 'false');
                                    }
                            }
                        } catch (clearError) {
                            self.logger.warn('خطا در clear کردن highlight:', clearError);
                        }
                    }, 200); // ✅ تاخیر برای اطمینان از render شدن کامل تقویم
                }

                // ✅ CRITICAL FIX: Override دکمه "امروز" برای استفاده از تاریخ سرور
                // این تضمین می‌کند که وقتی کاربر روی "امروز" کلیک می‌کند، تاریخ از سرور استفاده شود
                setTimeout(function() {
                    try {
                        var datePickerInstance = $input.data('pDatepicker');
                        if (datePickerInstance && datePickerInstance.$container) {
                            // ✅ پیدا کردن دکمه "امروز" در DatePicker
                            var $todayBtn = datePickerInstance.$container.find('.pdp-today-btn, .pdp-toolbox-today, [data-today-btn], button:contains("امروز")');
                            
                            if ($todayBtn.length === 0) {
                                // ✅ Fallback: جستجو در کل container
                                $todayBtn = $(datePickerInstance.$container).find('*').filter(function() {
                                    var $this = $(this);
                                    return $this.text().trim() === 'امروز' || 
                                           $this.attr('data-today') === 'true' ||
                                           $this.hasClass('pdp-today');
                                });
                            }
                            
                            if ($todayBtn.length > 0) {
                                // ✅ Override click handler
                                $todayBtn.off('click.persianDatePickerOverride').on('click.persianDatePickerOverride', function(e) {
                                    e.preventDefault();
                                    e.stopPropagation();
                                    
                                    self.logger.log('دکمه "امروز" کلیک شد - دریافت تاریخ از سرور');
                                    
                                    // ✅ دریافت تاریخ امروز از سرور
                                    self.getTodayFromServer().then(function(todayPersianDate) {
                                        if (todayPersianDate) {
                                            // ✅ تبدیل تاریخ شمسی به میلادی برای setDate
                                            var gregorianDateStr = self.convertPersianToGregorian(todayPersianDate);
                                            if (gregorianDateStr) {
                                                // ✅ CRITICAL: تبدیل string به Date object
                                                // convertPersianToGregorian یک ISO string برمی‌گرداند (YYYY-MM-DDTHH:mm:ss)
                                                var dateObj = new Date(gregorianDateStr);
                                                
                                                // ✅ بررسی صحت Date object
                                                if (isNaN(dateObj.getTime())) {
                                                    self.logger.error('خطا در ساخت Date object از:', gregorianDateStr);
                                                    return;
                                                }
                                                
                                                // ✅ Set date در DatePicker instance
                                                if (typeof datePickerInstance.setDate === 'function') {
                                                    datePickerInstance.setDate(dateObj);
                                                } else if (typeof datePickerInstance.set === 'function') {
                                                    datePickerInstance.set('date', dateObj);
                                                } else if (typeof datePickerInstance.update === 'function') {
                                                    // ✅ Fallback: استفاده از update
                                                    datePickerInstance.update(dateObj);
                                                }
                                                
                                                // ✅ Set value در input
                                                $input.val(todayPersianDate);
                                                
                                                // ✅ Trigger events برای date-selection.js
                                                setTimeout(function() {
                                                    $input.trigger('change');
                                                    
                                                    // ✅ Trigger custom event
                                                    var customEvent = $.Event('pDatepicker:select');
                                                    customEvent.unix = dateObj.getTime();
                                                    
                                                    // ✅ Parse Persian date برای selected object
                                                    var dateParts = todayPersianDate.split('/');
                                                    if (dateParts.length === 3) {
                                                        customEvent.selected = {
                                                            jy: parseInt(dateParts[0], 10),
                                                            jm: parseInt(dateParts[1], 10),
                                                            jd: parseInt(dateParts[2], 10)
                                                        };
                                                    }
                                                    
                                                    $input.trigger(customEvent);
                                                }, 50);
                                                
                                                self.logger.success('✅ دکمه "امروز" - تاریخ از سرور set شد:', {
                                                    persian: todayPersianDate,
                                                    gregorian: dateObj.toISOString()
                                                });
                                            } else {
                                                self.logger.error('خطا در تبدیل تاریخ شمسی به میلادی:', todayPersianDate);
                                            }
                                        } else {
                                            self.logger.error('خطا در دریافت تاریخ امروز از سرور');
                                        }
                                    }).catch(function(error) {
                                        self.logger.error('خطا در دریافت تاریخ امروز از سرور:', error);
                                    });
                                });
                                
                                self.logger.success('✅ دکمه "امروز" override شد برای استفاده از تاریخ سرور');
                            } else {
                                self.logger.warn('⚠️ دکمه "امروز" پیدا نشد برای override');
                            }
                        }
                    } catch (overrideError) {
                        self.logger.warn('خطا در override دکمه "امروز":', overrideError);
                    }
                }, 200); // ✅ تاخیر برای اطمینان از render شدن کامل DatePicker
                
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
                // ✅ CRITICAL FIX: محدود کردن تعداد تلاش‌ها برای جلوگیری از loop بی‌نهایت
                if (!self._retryCount) {
                    self._retryCount = 0;
                }
                
                if (self._retryCount < self.config.maxRetries * 10) { // 30 تلاش = 3 ثانیه
                    self._retryCount++;
                    this.logger.warn('pDatepicker یافت نشد، تلاش مجدد... (' + self._retryCount + '/' + (self.config.maxRetries * 10) + ')');
                    setTimeout(function() {
                        self.initializeAll();
                    }, this.config.retryDelay);
                } else {
                    this.logger.error('pDatepicker پس از ' + (self.config.maxRetries * 10) + ' تلاش یافت نشد. لطفاً مطمئن شوید که فایل‌های persian-datepicker لود شده‌اند.');
                }
                return false;
            }
            
            // ✅ Reset retry count on success
            self._retryCount = 0;

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

