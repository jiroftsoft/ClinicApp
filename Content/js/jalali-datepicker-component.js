/**
 * JalaliDatePicker Component
 * کامپوننت اصولی و قابل استفاده مجدد برای JalaliDatePicker
 * 
 * اصول طراحی:
 * - Component-Based: کامپوننت محور و قابل استفاده مجدد
 * - Server-Side Today: دریافت تاریخ امروز از سرور برای اطمینان از صحت
 * - Medical Form Standards: طبق استانداردهای فرم‌های درمانی سطح سازمانی
 * - Bulletproof: مقاوم و ضد گلوله
 * - No Dependencies: بدون وابستگی به jQuery (JalaliDatePicker خودش بدون jQuery است)
 * 
 * @version 1.0.0
 * @author ClinicApp Team
 * @date 1404/10/15
 * 
 * Migration از Persian DatePicker (babakhani) به JalaliDatePicker (majidh1)
 * 
 * مراجع:
 * - https://github.com/majidh1/JalaliDatePicker
 * - https://majidh1.github.io/JalaliDatePicker/
 */

(function(window) {
    'use strict';

    /**
     * JalaliDatePicker Component
     * کامپوننت اصلی برای مدیریت JalaliDatePicker
     */
    var JalaliDatePickerComponent = {
        
        /**
         * Configuration
         * تنظیمات پیش‌فرض
         */
        config: {
            selector: 'input[data-jdp]',
            hiddenInputSuffix: '_Hidden',
            apiEndpoint: '/api/persian-date/today', // ✅ استفاده از RoutePrefix + Route attribute
            logPrefix: '📅 [JalaliDatePicker]',
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
                if (JalaliDatePickerComponent.config.enableLogging) {
                    console.log(JalaliDatePickerComponent.config.logPrefix, message, data || '');
                }
            },
            error: function(message, error, data) {
                if (JalaliDatePickerComponent.config.enableLogging) {
                    console.error(JalaliDatePickerComponent.config.logPrefix, '❌', message, error, data || '');
                }
            },
            warn: function(message, data) {
                if (JalaliDatePickerComponent.config.enableLogging) {
                    console.warn(JalaliDatePickerComponent.config.logPrefix, '⚠️', message, data || '');
                }
            },
            success: function(message, data) {
                if (JalaliDatePickerComponent.config.enableLogging) {
                    console.log(JalaliDatePickerComponent.config.logPrefix, '✅', message, data || '');
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
                var xhr = new XMLHttpRequest();
                xhr.open('GET', self.config.apiEndpoint, true);
                xhr.setRequestHeader('Content-Type', 'application/json');
                xhr.onreadystatechange = function() {
                    if (xhr.readyState === 4) {
                        if (xhr.status === 200) {
                            try {
                                var response = JSON.parse(xhr.responseText);
                                if (response && response.persianDate) {
                                    self.cache.today = response.persianDate;
                                    self.cache.timestamp = Date.now();
                                    self.logger.success('تاریخ امروز از سرور دریافت شد:', response.persianDate);
                                    resolve(response.persianDate);
                                } else {
                                    self.logger.warn('پاسخ سرور نامعتبر:', response);
                                    reject(new Error('پاسخ سرور نامعتبر'));
                                }
                            } catch (e) {
                                self.logger.error('خطا در parse کردن پاسخ سرور:', e);
                                reject(e);
                            }
                        } else {
                            self.logger.error('خطا در دریافت تاریخ از سرور:', null, {
                                status: xhr.status,
                                statusText: xhr.statusText
                            });
                            reject(new Error('خطا در دریافت تاریخ از سرور: ' + xhr.status));
                        }
                    }
                };
                xhr.onerror = function() {
                    self.logger.error('خطا در ارتباط با سرور:', null);
                    reject(new Error('خطا در ارتباط با سرور'));
                };
                xhr.send();
            });
        },

        /**
         * تبدیل تاریخ شمسی به میلادی
         * 
         * @param {string} persianDate - تاریخ شمسی (مثلاً "1404/10/15")
         * @returns {Date|null} - Date object یا null
         */
        convertPersianToGregorian: function(persianDate) {
            if (!persianDate || typeof persianDate !== 'string') {
                return null;
            }

            try {
                // ✅ تبدیل اعداد فارسی به انگلیسی
                var englishDate = this.convertPersianToEnglishNumbers(persianDate.trim());
                
                // ✅ Parse کردن تاریخ
                var parts = englishDate.split('/');
                if (parts.length !== 3) {
                    this.logger.warn('فرمت تاریخ نامعتبر:', persianDate);
                    return null;
                }

                var year = parseInt(parts[0], 10);
                var month = parseInt(parts[1], 10);
                var day = parseInt(parts[2], 10);

                if (isNaN(year) || isNaN(month) || isNaN(day)) {
                    this.logger.warn('تاریخ parse نشد:', persianDate);
                    return null;
                }

                // ✅ استفاده از jalaali library (که در jalalidatepicker.min.js موجود است)
                // jalaliDatepicker از jalaali استفاده می‌کند
                if (typeof jalaali !== 'undefined' && typeof jalaali.toGregorian === 'function') {
                    try {
                        var gregorian = jalaali.toGregorian(year, month, day);
                        // ✅ استفاده از UTC برای timezone-independent date
                        var date = new Date(Date.UTC(gregorian.gy, gregorian.gm - 1, gregorian.gd));
                        
                        // ✅ بررسی معتبر بودن تاریخ
                        if (isNaN(date.getTime())) {
                            this.logger.error('تاریخ میلادی نامعتبر:', gregorian);
                            return null;
                        }
                        
                        this.logger.log('تاریخ تبدیل شد:', {
                            persian: persianDate,
                            gregorian: gregorian.gy + '/' + gregorian.gm + '/' + gregorian.gd
                        });
                        
                        return date;
                    } catch (e) {
                        this.logger.error('خطا در تبدیل با jalaali:', e);
                        return null;
                    }
                } else {
                    // ✅ Fallback: استفاده از API سرور
                    this.logger.warn('jalaali library در دسترس نیست، استفاده از fallback');
                    // TODO: می‌توانیم از API سرور استفاده کنیم
                    return null;
                }
            } catch (e) {
                this.logger.error('خطا در تبدیل تاریخ شمسی به میلادی:', e);
                return null;
            }
        },

        /**
         * تبدیل اعداد فارسی به انگلیسی
         * 
         * @param {string} text - متن با اعداد فارسی
         * @returns {string} - متن با اعداد انگلیسی
         */
        convertPersianToEnglishNumbers: function(text) {
            if (!text || typeof text !== 'string') {
                return text;
            }

            var persianDigits = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
            var englishDigits = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
            
            var result = text;
            for (var i = 0; i < persianDigits.length; i++) {
                result = result.replace(new RegExp(persianDigits[i], 'g'), englishDigits[i]);
            }
            
            return result;
        },

        /**
         * Parse کردن تاریخ شمسی به object
         * 
         * @param {string} persianDate - تاریخ شمسی (مثلاً "1404/10/15")
         * @returns {Object|null} - { year, month, day } یا null
         */
        parsePersianDate: function(persianDate) {
            if (!persianDate || typeof persianDate !== 'string') {
                return null;
            }

            try {
                var normalized = this.convertPersianToEnglishNumbers(persianDate.trim());
                var parts = normalized.split('/');
                if (parts.length !== 3) {
                    return null;
                }

                var year = parseInt(parts[0]);
                var month = parseInt(parts[1]);
                var day = parseInt(parts[2]);

                if (isNaN(year) || isNaN(month) || isNaN(day)) {
                    return null;
                }

                return { year: year, month: month, day: day };
            } catch (e) {
                this.logger.error('خطا در parse کردن تاریخ شمسی:', e);
                return null;
            }
        },

        /**
         * ایجاد یا دریافت hidden input برای فرم POST
         * 
         * @param {HTMLElement} form - فرم والد
         * @param {string} fieldName - نام فیلد
         * @returns {HTMLElement|null} - hidden input element یا null
         */
        getOrCreateHiddenInput: function(form, fieldName) {
            if (!form || !fieldName) {
                return null;
            }

            var hiddenInputName = fieldName + this.config.hiddenInputSuffix;
            var hiddenInput = form.querySelector('input[name="' + hiddenInputName + '"]');
            
            if (!hiddenInput) {
                hiddenInput = document.createElement('input');
                hiddenInput.type = 'hidden';
                hiddenInput.name = hiddenInputName;
                form.appendChild(hiddenInput);
                this.logger.log('Hidden input ایجاد شد:', hiddenInputName);
            }

            return hiddenInput;
        },

        /**
         * Initialize Single DatePicker
         * Initialize کردن یک DatePicker
         * 
         * @param {HTMLElement} input - input element
         */
        initializeDatePicker: function(input) {
            var self = this;
            
            // بررسی اینکه قبلاً initialize شده یا نه
            if (input.dataset.jdpInitialized === 'true') {
                this.logger.log('DatePicker قبلاً initialize شده:', input.name);
                return;
            }

            var fieldName = input.name;
            var form = input.closest('form');
            
            // ✅ برای فرم GET، نیازی به hidden input نیست
            var isGetForm = form && form.method && form.method.toLowerCase() === 'get';
            
            if (!form) {
                this.logger.warn('فرم والد یافت نشد برای:', fieldName);
                isGetForm = true;
            }

            // ایجاد hidden input فقط برای فرم POST
            var hiddenInput = null;
            if (!isGetForm) {
                hiddenInput = this.getOrCreateHiddenInput(form, fieldName);
            }

            // ✅ خواندن مقدار فعلی input (ممکن است از View set شده باشد)
            var currentValue = input.value;
            this.logger.log('مقدار فعلی input:', {
                field: fieldName,
                currentValue: currentValue,
                hasValue: currentValue && currentValue.trim() !== ''
            });
            
            // اگر input مقدار دارد (مثلاً در Edit form یا از View)، تبدیل کن
            if (currentValue && currentValue.trim() !== '' && hiddenInput) {
                var gregorianDate = this.convertPersianToGregorian(currentValue);
                if (gregorianDate) {
                    hiddenInput.value = gregorianDate;
                    this.logger.success('تاریخ موجود تبدیل شد:', {
                        field: fieldName,
                        persian: currentValue,
                        gregorian: gregorianDate
                    });
                }
            }

            // ✅ CRITICAL FIX: بررسی data-no-default-date attribute
            var noDefaultDate = input.dataset.noDefaultDate === 'true';
            
            // ✅ CRITICAL FIX: همیشه تاریخ امروز از سرور را دریافت کن
            var todayPromise = this.getTodayFromServer();
            
            todayPromise.then(function(todayPersianDate) {
                // ✅ Parse کردن تاریخ امروز
                var todayDateObj = self.parsePersianDate(todayPersianDate);
                
                // ✅ تنظیم minDate از تاریخ سرور
                var minDate = todayDateObj || null;
                
                // ✅ Parse کردن currentValue
                var initDate = null;
                if (currentValue && currentValue.trim() !== '') {
                    initDate = self.parsePersianDate(currentValue);
                } else if (!noDefaultDate && todayDateObj) {
                    // فقط اگر noDefaultDate false باشد
                    initDate = todayDateObj;
                }

                // ✅ تنظیم data attributes برای JalaliDatePicker
                if (minDate) {
                    input.setAttribute('data-jdp-min-date', 
                        minDate.year + '/' + 
                        String(minDate.month).padStart(2, '0') + '/' + 
                        String(minDate.day).padStart(2, '0'));
                }

                // ✅ تنظیم initDate در data attribute (اگر نیاز باشد)
                if (initDate && !noDefaultDate) {
                    input.setAttribute('data-jdp-init-date',
                        initDate.year + '/' + 
                        String(initDate.month).padStart(2, '0') + '/' + 
                        String(initDate.day).padStart(2, '0'));
                }

                // ✅ Event listener برای تغییرات
                // JalaliDatePicker از 'jdp:change' event استفاده می‌کند
                // جلوگیری از duplicate events با استفاده از flag
                var isHandlingChange = false;
                
                input.addEventListener('jdp:change', function(event) {
                    if (!isHandlingChange) {
                        isHandlingChange = true;
                        self.handleDateChange(input, hiddenInput, fieldName);
                        setTimeout(function() {
                            isHandlingChange = false;
                        }, 50);
                    }
                });
                
                // ✅ change event را listen نمی‌کنیم چون jdp:change کافی است
                // و باعث duplicate events می‌شود

                // ✅ Mark as initialized
                input.dataset.jdpInitialized = 'true';
                
                self.logger.success('DatePicker initialize شد:', fieldName);
            }).catch(function(error) {
                self.logger.error('خطا در initialize DatePicker:', error, {
                    field: fieldName
                });
            });
        },

        /**
         * Handle Date Change
         * مدیریت تغییر تاریخ
         * 
         * @param {HTMLElement} input - input element
         * @param {HTMLElement} hiddenInput - hidden input element
         * @param {string} fieldName - نام فیلد
         */
        handleDateChange: function(input, hiddenInput, fieldName) {
            var self = this;
            var persianDate = input.value;
            
            if (persianDate && persianDate.trim() !== '') {
                // ✅ تبدیل به میلادی برای hidden input
                if (hiddenInput) {
                    var gregorianDate = this.convertPersianToGregorian(persianDate);
                    if (gregorianDate) {
                        // ✅ Format کردن تاریخ برای hidden input (YYYY-MM-DD)
                        var year = gregorianDate.getFullYear();
                        var month = String(gregorianDate.getMonth() + 1).padStart(2, '0');
                        var day = String(gregorianDate.getDate()).padStart(2, '0');
                        var formattedDate = year + '-' + month + '-' + day;
                        
                        hiddenInput.value = formattedDate;
                        this.logger.log('تاریخ تبدیل شد:', {
                            field: fieldName,
                            persian: persianDate,
                            gregorian: formattedDate
                        });
                    } else {
                        this.logger.warn('تاریخ تبدیل نشد:', persianDate);
                    }
                }

                // ✅ Trigger custom event برای date-selection.js (فقط یک بار)
                // جلوگیری از duplicate events با استفاده از flag
                if (!input.dataset.eventTriggered) {
                    input.dataset.eventTriggered = 'true';
                    
                    var event = new CustomEvent('pDatepicker:select', {
                        detail: {
                            unix: null, // JalaliDatePicker unix timestamp ندارد
                            selected: {
                                persianDate: persianDate
                            }
                        }
                    });
                    input.dispatchEvent(event);
                    
                    // ✅ Reset flag بعد از 100ms
                    setTimeout(function() {
                        input.dataset.eventTriggered = '';
                    }, 100);
                }
            } else {
                // ✅ Clear hidden input
                if (hiddenInput) {
                    hiddenInput.value = '';
                }
            }
        },

        /**
         * Initialize All DatePickers
         * Initialize کردن تمام DatePicker ها در صفحه
         */
        initializeAll: function() {
            var self = this;
            
            this.logger.log('شروع initialize تمام DatePicker ها...');
            
            var inputs = document.querySelectorAll(this.config.selector);
            var count = 0;
            
            inputs.forEach(function(input) {
                try {
                    self.initializeDatePicker(input);
                    count++;
                } catch (e) {
                    self.logger.error('خطا در initialize DatePicker:', e, {
                        field: input.name || input.id
                    });
                }
            });
            
            this.logger.success('تمام DatePicker ها initialize شدند:', count + ' مورد');
        }
    };

    // ✅ Export به window
    window.JalaliDatePickerComponent = JalaliDatePickerComponent;

    // ✅ Auto-initialize وقتی DOM ready است
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            // ✅ Wait for JalaliDatePicker to be loaded
            if (typeof jalaliDatepicker !== 'undefined') {
                // ✅ Initialize JalaliDatePicker globally
                jalaliDatepicker.startWatch({
                    date: true,
                    time: false,
                    showTodayBtn: true,
                    showEmptyBtn: true,
                    hideAfterChange: true,
                    autoShow: true,
                    autoHide: true
                });
                
                // ✅ Initialize our component
                JalaliDatePickerComponent.initializeAll();
            } else {
                console.warn('⚠️ JalaliDatePicker library not loaded yet');
            }
        });
    } else {
        // DOM already ready
        if (typeof jalaliDatepicker !== 'undefined') {
            jalaliDatepicker.startWatch({
                date: true,
                time: false,
                showTodayBtn: true,
                showEmptyBtn: true,
                hideAfterChange: true,
                autoShow: true,
                autoHide: true
            });
            
            JalaliDatePickerComponent.initializeAll();
        }
    }

})(window);

