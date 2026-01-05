/**
 * JalaliDatePicker Enterprise Component
 * =====================================
 * 
 * ✅ Enterprise-Grade Features:
 * - Fully Modular & Configurable
 * - Multiple Instance Support
 * - Component Lifecycle Management
 * - Event-Driven Architecture
 * - Error Recovery & Retry Logic
 * - Performance Optimized
 * - Production-Ready
 * - Bulletproof & Best Practices
 * 
 * ✅ Usage Examples:
 * 
 * 1. Auto-initialize on page load:
 *    <input data-jdp data-jdp-config='{"theme": "medical", "size": "large"}' />
 * 
 * 2. Manual initialization:
 *    JalaliDatePickerEnterprise.init('#myDateInput', {
 *        theme: 'medical',
 *        size: 'large',
 *        minDate: { year: 1404, month: 10, day: 15 },
 *        onSelect: function(date) { console.log('Selected:', date); }
 *    });
 * 
 * 3. Programmatic control:
 *    var picker = JalaliDatePickerEnterprise.getInstance('#myDateInput');
 *    picker.setDate('1404/10/15');
 *    picker.show();
 *    picker.hide();
 * 
 * @version 2.0.0
 * @author ClinicApp Team
 * @date 1404/10/15
 * 
 * مراجع:
 * - https://github.com/majidh1/JalaliDatePicker
 * - https://majidh1.github.io/JalaliDatePicker/
 * - https://codepen.io/collection/wajWMo
 */

(function(window) {
    'use strict';

    /**
     * JalaliDatePicker Enterprise Component
     * کامپوننت Enterprise-Grade برای JalaliDatePicker
     */
    var JalaliDatePickerEnterprise = {
        
        /**
         * Configuration
         * تنظیمات پیش‌فرض
         */
        config: {
            selector: 'input[data-jdp]',
            hiddenInputSuffix: '_Hidden',
            apiEndpoint: '/api/persian-date/today',
            logPrefix: '📅 [JalaliDatePicker]',
            enableLogging: false, // ✅ Production: false, Development: true
            cacheTodayFor: 60000, // 1 دقیقه
            retryDelay: 100,
            maxRetries: 30,
            
            // ✅ Default JalaliDatePicker Options
            defaultOptions: {
                date: true,
                time: false,
                showTodayBtn: true,
                showEmptyBtn: true,
                showCloseBtn: true,
                hideAfterChange: true,
                autoShow: true,
                autoHide: true,
                autoReadOnlyInput: true,
                useDropDownYears: true,
                persianDigits: false,
                separatorChars: {
                    date: '/',
                    between: ' ',
                    time: ':'
                }
            },
            
            // ✅ Theme Configuration
            themes: {
                medical: {
                    zIndex: 10000,
                    container: 'body',
                    showTodayBtn: true,
                    showEmptyBtn: true
                },
                minimal: {
                    zIndex: 10000,
                    container: 'body',
                    showTodayBtn: false,
                    showEmptyBtn: false
                },
                compact: {
                    zIndex: 10000,
                    container: 'body',
                    showTodayBtn: true,
                    showEmptyBtn: true,
                    useDropDownYears: false
                }
            },
            
            // ✅ Size Configuration
            sizes: {
                small: {
                    inputClass: 'form-control form-control-sm',
                    topSpace: 5,
                    bottomSpace: 5
                },
                medium: {
                    inputClass: 'form-control',
                    topSpace: 0,
                    bottomSpace: 0
                },
                large: {
                    inputClass: 'form-control form-control-lg',
                    topSpace: 10,
                    bottomSpace: 10
                }
            }
        },

        /**
         * Cache برای تاریخ امروز
         */
        cache: {
            today: null,
            timestamp: null
        },

        /**
         * Instances Registry
         * ثبت تمام instance های ایجاد شده
         */
        instances: new Map(),

        /**
         * Logger
         * سیستم لاگ‌گذاری (Production: silent, Development: verbose)
         */
        logger: {
            log: function(message, data) {
                if (JalaliDatePickerEnterprise.config.enableLogging) {
                    console.log(JalaliDatePickerEnterprise.config.logPrefix, message, data || '');
                }
            },
            error: function(message, error, data) {
                // ✅ Production: فقط خطاهای critical را log می‌کنیم
                if (JalaliDatePickerEnterprise.config.enableLogging || (error && error.critical)) {
                    console.error(JalaliDatePickerEnterprise.config.logPrefix, '❌', message, error, data || '');
                }
            },
            warn: function(message, data) {
                if (JalaliDatePickerEnterprise.config.enableLogging) {
                    console.warn(JalaliDatePickerEnterprise.config.logPrefix, '⚠️', message, data || '');
                }
            },
            success: function(message, data) {
                if (JalaliDatePickerEnterprise.config.enableLogging) {
                    console.log(JalaliDatePickerEnterprise.config.logPrefix, '✅', message, data || '');
                }
            }
        },

        /**
         * دریافت تاریخ امروز شمسی از سرور
         * 
         * @returns {Promise<string>}
         */
        getTodayFromServer: function() {
            var self = this;
            
            // ✅ بررسی Cache
            if (self.cache.today && self.cache.timestamp) {
                var now = Date.now();
                var cacheAge = now - self.cache.timestamp;
                if (cacheAge < self.config.cacheTodayFor) {
                    return Promise.resolve(self.cache.today);
                }
            }

            // ✅ دریافت از سرور با retry logic
            return new Promise(function(resolve, reject) {
                var attempts = 0;
                
                function fetchToday() {
                    attempts++;
                    var xhr = new XMLHttpRequest();
                    xhr.open('GET', self.config.apiEndpoint, true);
                    xhr.setRequestHeader('Content-Type', 'application/json');
                    xhr.timeout = 5000;
                    
                    xhr.onreadystatechange = function() {
                        if (xhr.readyState === 4) {
                            if (xhr.status === 200) {
                                try {
                                    var response = JSON.parse(xhr.responseText);
                                    if (response && response.persianDate) {
                                        self.cache.today = response.persianDate;
                                        self.cache.timestamp = Date.now();
                                        resolve(response.persianDate);
                                    } else {
                                        throw new Error('Invalid response format');
                                    }
                                } catch (e) {
                                    if (attempts < self.config.maxRetries) {
                                        setTimeout(fetchToday, self.config.retryDelay);
                                    } else {
                                        reject(e);
                                    }
                                }
                            } else {
                                if (attempts < self.config.maxRetries) {
                                    setTimeout(fetchToday, self.config.retryDelay);
                                } else {
                                    reject(new Error('Server error: ' + xhr.status));
                                }
                            }
                        }
                    };
                    
                    xhr.onerror = function() {
                        if (attempts < self.config.maxRetries) {
                            setTimeout(fetchToday, self.config.retryDelay);
                        } else {
                            reject(new Error('Network error'));
                        }
                    };
                    
                    xhr.ontimeout = function() {
                        if (attempts < self.config.maxRetries) {
                            setTimeout(fetchToday, self.config.retryDelay);
                        } else {
                            reject(new Error('Request timeout'));
                        }
                    };
                    
                    xhr.send();
                }
                
                fetchToday();
            });
        },

        /**
         * تبدیل اعداد فارسی به انگلیسی
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
         * Parse کردن تاریخ شمسی
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
                var year = parseInt(parts[0], 10);
                var month = parseInt(parts[1], 10);
                var day = parseInt(parts[2], 10);
                if (isNaN(year) || isNaN(month) || isNaN(day)) {
                    return null;
                }
                return { year: year, month: month, day: day };
            } catch (e) {
                return null;
            }
        },

        /**
         * تبدیل تاریخ شمسی به میلادی
         */
        convertPersianToGregorian: function(persianDate) {
            if (!persianDate || typeof persianDate !== 'string') {
                return null;
            }
            try {
                var englishDate = this.convertPersianToEnglishNumbers(persianDate.trim());
                var parts = englishDate.split('/');
                if (parts.length !== 3) {
                    return null;
                }
                var year = parseInt(parts[0], 10);
                var month = parseInt(parts[1], 10);
                var day = parseInt(parts[2], 10);
                if (isNaN(year) || isNaN(month) || isNaN(day)) {
                    return null;
                }
                
                if (typeof jalaali !== 'undefined' && typeof jalaali.toGregorian === 'function') {
                    var gregorian = jalaali.toGregorian(year, month, day);
                    var date = new Date(Date.UTC(gregorian.gy, gregorian.gm - 1, gregorian.gd));
                    if (isNaN(date.getTime())) {
                        return null;
                    }
                    return date;
                }
                return null;
            } catch (e) {
                return null;
            }
        },

        /**
         * Merge Configuration
         * ادغام تنظیمات با defaults
         */
        mergeConfig: function(userConfig, defaults) {
            var result = {};
            for (var key in defaults) {
                if (defaults.hasOwnProperty(key)) {
                    if (typeof defaults[key] === 'object' && defaults[key] !== null && !Array.isArray(defaults[key])) {
                        result[key] = this.mergeConfig(userConfig[key] || {}, defaults[key]);
                    } else {
                        result[key] = userConfig && userConfig.hasOwnProperty(key) ? userConfig[key] : defaults[key];
                    }
                }
            }
            if (userConfig) {
                for (var key in userConfig) {
                    if (userConfig.hasOwnProperty(key) && !defaults.hasOwnProperty(key)) {
                        result[key] = userConfig[key];
                    }
                }
            }
            return result;
        },

        /**
         * Parse Data Attributes
         * خواندن تنظیمات از data attributes
         */
        parseDataAttributes: function(input) {
            var config = {};
            
            // ✅ Theme
            if (input.dataset.jdpTheme) {
                config.theme = input.dataset.jdpTheme;
            }
            
            // ✅ Size
            if (input.dataset.jdpSize) {
                config.size = input.dataset.jdpSize;
            }
            
            // ✅ Min Date
            if (input.dataset.jdpMinDate) {
                var minDate = this.parsePersianDate(input.dataset.jdpMinDate);
                if (minDate) {
                    config.minDate = minDate;
                }
            }
            
            // ✅ Max Date
            if (input.dataset.jdpMaxDate) {
                var maxDate = this.parsePersianDate(input.dataset.jdpMaxDate);
                if (maxDate) {
                    config.maxDate = maxDate;
                }
            }
            
            // ✅ Init Date
            if (input.dataset.jdpInitDate) {
                var initDate = this.parsePersianDate(input.dataset.jdpInitDate);
                if (initDate) {
                    config.initDate = initDate;
                }
            }
            
            // ✅ No Default Date
            if (input.dataset.noDefaultDate === 'true') {
                config.noDefaultDate = true;
            }
            
            // ✅ Custom Config (JSON)
            if (input.dataset.jdpConfig) {
                try {
                    var customConfig = JSON.parse(input.dataset.jdpConfig);
                    config = this.mergeConfig(customConfig, config);
                } catch (e) {
                    this.logger.warn('Invalid JSON in data-jdp-config:', e);
                }
            }
            
            return config;
        },

        /**
         * Initialize Single DatePicker
         * 
         * @param {HTMLElement|string} input - input element or selector
         * @param {Object} userConfig - user configuration
         * @returns {Object} - instance object
         */
        init: function(input, userConfig) {
            var self = this;
            
            // ✅ Get element
            if (typeof input === 'string') {
                input = document.querySelector(input);
            }
            
            if (!input || !(input instanceof HTMLElement)) {
                this.logger.error('Invalid input element', null, { input: input });
                return null;
            }
            
            // ✅ Check if already initialized
            var instanceId = input.id || input.name || 'jdp_' + Date.now();
            if (self.instances.has(instanceId)) {
                this.logger.warn('DatePicker already initialized:', instanceId);
                return self.instances.get(instanceId);
            }
            
            // ✅ Parse data attributes
            var dataConfig = self.parseDataAttributes(input);
            
            // ✅ Merge configurations
            var config = self.mergeConfig(userConfig || {}, dataConfig);
            config = self.mergeConfig(config, {
                theme: 'medical',
                size: 'medium',
                noDefaultDate: false
            });
            
            // ✅ Apply theme
            var themeConfig = self.config.themes[config.theme] || self.config.themes.medical;
            
            // ✅ Apply size
            var sizeConfig = self.config.sizes[config.size] || self.config.sizes.medium;
            if (sizeConfig.inputClass) {
                input.className = input.className.replace(/form-control(-\w+)?/g, '').trim();
                input.className += ' ' + sizeConfig.inputClass;
            }
            
            // ✅ Get form and hidden input
            var form = input.closest('form');
            var isGetForm = form && form.method && form.method.toLowerCase() === 'get';
            var hiddenInput = null;
            if (!isGetForm && form) {
                var fieldName = input.name;
                if (fieldName) {
                    var hiddenInputName = fieldName + self.config.hiddenInputSuffix;
                    hiddenInput = form.querySelector('input[name="' + hiddenInputName + '"]');
                    if (!hiddenInput) {
                        hiddenInput = document.createElement('input');
                        hiddenInput.type = 'hidden';
                        hiddenInput.name = hiddenInputName;
                        form.appendChild(hiddenInput);
                    }
                }
            }
            
            // ✅ Get today's date from server
            var todayPromise = self.getTodayFromServer();
            
            // ✅ Create instance object
            var instance = {
                id: instanceId,
                input: input,
                hiddenInput: hiddenInput,
                config: config,
                isInitialized: false,
                
                // ✅ Methods
                setDate: function(date) {
                    if (typeof date === 'string') {
                        input.value = date;
                    } else if (date && date.year) {
                        input.value = date.year + '/' + 
                                     String(date.month).padStart(2, '0') + '/' + 
                                     String(date.day).padStart(2, '0');
                    }
                    self._triggerEvent(input, 'jdp:change');
                },
                
                getDate: function() {
                    return input.value ? self.parsePersianDate(input.value) : null;
                },
                
                show: function() {
                    if (typeof jalaliDatepicker !== 'undefined') {
                        jalaliDatepicker.show(input);
                    }
                },
                
                hide: function() {
                    if (typeof jalaliDatepicker !== 'undefined') {
                        jalaliDatepicker.hide();
                    }
                },
                
                destroy: function() {
                    self.instances.delete(instanceId);
                    input.removeEventListener('jdp:change', instance._onChangeHandler);
                    input.removeEventListener('change', instance._onChangeHandler);
                    input.dataset.jdpInitialized = '';
                }
            };
            
            // ✅ Initialize with today's date
            todayPromise.then(function(todayPersianDate) {
                var todayDateObj = self.parsePersianDate(todayPersianDate);
                
                // ✅ Set minDate
                if (todayDateObj && !config.minDate) {
                    config.minDate = todayDateObj;
                }
                
                // ✅ Set initDate
                if (!config.initDate) {
                    var currentValue = input.value;
                    if (currentValue && currentValue.trim() !== '') {
                        config.initDate = self.parsePersianDate(currentValue);
                    } else if (!config.noDefaultDate && todayDateObj) {
                        config.initDate = todayDateObj;
                    }
                }
                
                // ✅ Set data attributes for JalaliDatePicker
                if (config.minDate) {
                    input.setAttribute('data-jdp-min-date', 
                        config.minDate.year + '/' + 
                        String(config.minDate.month).padStart(2, '0') + '/' + 
                        String(config.minDate.day).padStart(2, '0'));
                }
                
                if (config.maxDate) {
                    input.setAttribute('data-jdp-max-date',
                        config.maxDate.year + '/' + 
                        String(config.maxDate.month).padStart(2, '0') + '/' + 
                        String(config.maxDate.day).padStart(2, '0'));
                }
                
                if (config.initDate && !config.noDefaultDate) {
                    input.setAttribute('data-jdp-init-date',
                        config.initDate.year + '/' + 
                        String(config.initDate.month).padStart(2, '0') + '/' + 
                        String(config.initDate.day).padStart(2, '0'));
                }
                
                // ✅ Merge JalaliDatePicker options
                var jdpOptions = self.mergeConfig(config.jdpOptions || {}, self.config.defaultOptions);
                jdpOptions = self.mergeConfig(themeConfig, jdpOptions);
                if (sizeConfig.topSpace !== undefined) {
                    jdpOptions.topSpace = sizeConfig.topSpace;
                }
                if (sizeConfig.bottomSpace !== undefined) {
                    jdpOptions.bottomSpace = sizeConfig.bottomSpace;
                }
                
                // ✅ Event handlers
                var isHandlingChange = false;
                instance._onChangeHandler = function(event) {
                    if (!isHandlingChange) {
                        isHandlingChange = true;
                        self._handleDateChange(input, hiddenInput, instance, config);
                        setTimeout(function() {
                            isHandlingChange = false;
                        }, 50);
                    }
                };
                
                input.addEventListener('jdp:change', instance._onChangeHandler);
                
                // ✅ Custom onSelect callback
                if (config.onSelect && typeof config.onSelect === 'function') {
                    input.addEventListener('pDatepicker:select', function(e) {
                        config.onSelect.call(instance, e.detail.selected.persianDate, instance.getDate());
                    });
                }
                
                // ✅ Mark as initialized
                input.dataset.jdpInitialized = 'true';
                instance.isInitialized = true;
                self.instances.set(instanceId, instance);
                
                self.logger.success('DatePicker initialized:', instanceId);
            }).catch(function(error) {
                self.logger.error('Failed to initialize DatePicker:', error, { instanceId: instanceId });
            });
            
            return instance;
        },

        /**
         * Handle Date Change
         */
        _handleDateChange: function(input, hiddenInput, instance, config) {
            var self = this;
            var persianDate = input.value;
            
            if (persianDate && persianDate.trim() !== '') {
                // ✅ Convert to Gregorian for hidden input
                if (hiddenInput) {
                    var gregorianDate = self.convertPersianToGregorian(persianDate);
                    if (gregorianDate) {
                        var year = gregorianDate.getFullYear();
                        var month = String(gregorianDate.getMonth() + 1).padStart(2, '0');
                        var day = String(gregorianDate.getDate()).padStart(2, '0');
                        hiddenInput.value = year + '-' + month + '-' + day;
                    }
                }
                
                // ✅ Trigger custom event (فقط یک بار)
                // ⚠️ CRITICAL: جلوگیری از duplicate events
                if (!input.dataset.eventTriggered) {
                    input.dataset.eventTriggered = 'true';
                    self._triggerEvent(input, 'pDatepicker:select', {
                        persianDate: persianDate,
                        gregorianDate: hiddenInput ? hiddenInput.value : null
                    });
                    // ✅ Reset flag بعد از 200ms (بیشتر از timeout در date-selection.js)
                    setTimeout(function() {
                        input.dataset.eventTriggered = '';
                    }, 200);
                }
            } else {
                if (hiddenInput) {
                    hiddenInput.value = '';
                }
            }
        },

        /**
         * Trigger Custom Event
         */
        _triggerEvent: function(element, eventName, detail) {
            var event = new CustomEvent(eventName, {
                detail: detail || {},
                bubbles: true,
                cancelable: true
            });
            element.dispatchEvent(event);
        },

        /**
         * Get Instance
         */
        getInstance: function(input) {
            if (typeof input === 'string') {
                input = document.querySelector(input);
            }
            if (!input) {
                return null;
            }
            var instanceId = input.id || input.name;
            return this.instances.get(instanceId) || null;
        },

        /**
         * Initialize All
         */
        initializeAll: function() {
            var self = this;
            var inputs = document.querySelectorAll(self.config.selector);
            var count = 0;
            
            inputs.forEach(function(input) {
                if (!input.dataset.jdpInitialized) {
                    try {
                        self.init(input);
                        count++;
                    } catch (e) {
                        self.logger.error('Failed to initialize DatePicker:', e, {
                            field: input.name || input.id
                        });
                    }
                }
            });
            
            if (count > 0) {
                self.logger.success('Initialized DatePickers:', count + ' instances');
            }
        }
    };

    // ✅ Export to window
    window.JalaliDatePickerEnterprise = JalaliDatePickerEnterprise;

    // ✅ Auto-initialize when DOM is ready
    // ✅ با retry logic برای اطمینان از لود شدن jalaliDatepicker
    var enterpriseInitialized = false;
    
    function initializeEnterprise() {
        if (typeof jalaliDatepicker !== 'undefined') {
            try {
                // ✅ فقط یک بار startWatch را فراخوانی می‌کنیم
                if (!enterpriseInitialized) {
                    jalaliDatepicker.startWatch(JalaliDatePickerEnterprise.config.defaultOptions);
                    enterpriseInitialized = true;
                }
                JalaliDatePickerEnterprise.initializeAll();
            } catch (e) {
                JalaliDatePickerEnterprise.logger.error('Failed to initialize Enterprise Component:', e);
                // ✅ Retry after 100ms (حداکثر 10 بار)
                var retryCount = initializeEnterprise.retryCount || 0;
                if (retryCount < 10) {
                    initializeEnterprise.retryCount = retryCount + 1;
                    setTimeout(initializeEnterprise, 100);
                }
            }
        } else {
            // ✅ jalaliDatepicker هنوز لود نشده - retry (حداکثر 10 بار)
            var retryCount = initializeEnterprise.retryCount || 0;
            if (retryCount < 10) {
                initializeEnterprise.retryCount = retryCount + 1;
                setTimeout(initializeEnterprise, 100);
            } else {
                JalaliDatePickerEnterprise.logger.error('jalaliDatepicker failed to load after 10 retries');
            }
        }
    }
    
    // ✅ Reset retry counter
    initializeEnterprise.retryCount = 0;
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            initializeEnterprise();
        });
    } else {
        // ✅ DOM already ready - start initialization
        initializeEnterprise();
    }

})(window);

