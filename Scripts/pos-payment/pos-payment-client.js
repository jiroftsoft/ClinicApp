/**
 * ============================================
 * POS Payment Client Module (Production-Ready)
 * ============================================
 * 
 * ماژول حرفه‌ای و قابل استفاده مجدد برای پرداخت POS
 * 
 * ویژگی‌ها:
 * ✅ Client-Side SignalR Communication
 * ✅ Event-Driven Architecture
 * ✅ Error Handling & Retry Logic
 * ✅ Connection State Management
 * ✅ Comprehensive Logging
 * ✅ User-Friendly Messages
 * 
 * استفاده:
 * ```javascript
 * var posClient = new PosPaymentClient({
 *     signalRUrl: 'http://localhost:5000/signalr',
 *     onSuccess: function(response) { ... },
 *     onError: function(error) { ... },
 *     onCancel: function() { ... }
 * });
 * 
 * posClient.processPayment(terminalId, amount);
 * ```
 * 
 * @author ClinicApp Development Team
 * @version 1.0.0
 * @date 1404/09/11
 */

(function(window, $) {
    'use strict';

    /**
     * POS Payment Client Class
     * 
     * مسئولیت: مدیریت ارتباط Client-Side با SignalR Hub برای پرداخت POS
     * 
     * اصول طراحی:
     * - Single Responsibility: فقط مدیریت SignalR Connection و Payment Flow
     * - Dependency Injection: Configuration از خارج تزریق می‌شود
     * - Event-Driven: استفاده از Callbacks برای Events
     * - Error Handling: مدیریت کامل خطاها
     */
    function PosPaymentClient(config) {
        // ============================================
        // Configuration & Dependencies
        // ============================================
        this.config = $.extend({
            signalRUrl: 'http://localhost:5000/signalr',
            hubName: 'SSP1126HUB',
            connectionTimeout: 30000, // 30 seconds
            paymentTimeout: 120000, // 2 minutes
            transactionTimeout: 60000, // ✅ 60 seconds for transaction response (لغو یا timeout)
            initialDelay: 1000, // 1 second delay after Initial
            maxRetries: 3,
            retryDelay: 2000, // 2 seconds
            enableLogging: true,
            enableConsoleLog: true
        }, config || {});

        // ============================================
        // Event Handlers (Callbacks)
        // ============================================
        this.onSuccess = config.onSuccess || function() {};
        this.onError = config.onError || function() {};
        this.onCancel = config.onCancel || function() {};
        this.onCardSwiped = config.onCardSwiped || function() {};
        this.onConnecting = config.onConnecting || function() {};
        this.onConnected = config.onConnected || function() {};
        this.onDisconnected = config.onDisconnected || function() {};

        // ============================================
        // Internal State
        // ============================================
        this.posHub = null;
        this.isConnected = false;
        this.isProcessing = false;
        this.serverMessage = '';
        this.currentPayment = null;
        this.retryCount = 0;
        this.hubsScriptLoaded = false;
        this.transactionResponseReceived = false; // ✅ Flag برای بررسی دریافت پاسخ

        // ============================================
        // Initialize
        // ============================================
        // ✅ Defer initialization until SignalR is loaded
        // Use a polling mechanism to wait for SignalR library
        this._waitForSignalRAndInitialize();
    }

    /**
     * Wait for SignalR to be loaded, then initialize
     * 
     * مسئولیت: منتظر ماندن تا SignalR لود شود، سپس initialization
     */
    PosPaymentClient.prototype._waitForSignalRAndInitialize = function() {
        var self = this;
        var maxAttempts = 50; // 5 seconds total (50 * 100ms)
        var attemptCount = 0;

        function checkSignalR() {
            attemptCount++;

            // ✅ Check if SignalR is available (both $.signalR and $.connection)
            var hasSignalR = typeof $.signalR !== 'undefined' || typeof $.connection !== 'undefined';

            if (hasSignalR) {
                self._log('info', '✅ SignalR library detected after ' + attemptCount + ' attempts');
                self._initialize();
                return;
            }

            if (attemptCount >= maxAttempts) {
                self._log('error', '❌ SignalR library not loaded after ' + maxAttempts + ' attempts. Please ensure jquery.signalR-2.4.2.min.js is included in the page.');
                self.onError({
                    code: 'SIGNALR_NOT_LOADED',
                    message: 'کتابخانه SignalR بارگذاری نشده است. لطفاً صفحه را refresh کنید.'
                });
                return;
            }

            // ✅ Retry after 100ms
            setTimeout(checkSignalR, 100);
        }

        // Start checking
        this._log('info', '🚀 Waiting for SignalR library to load...');
        checkSignalR();
    }

    /**
     * Initialize SignalR Connection
     * 
     * مسئولیت: بارگذاری SignalR Hubs و ایجاد اتصال
     * 
     * ⚠️ این متد فقط زمانی فراخوانی می‌شود که SignalR library لود شده باشد
     */
    PosPaymentClient.prototype._initialize = function() {
        var self = this;
        
        this._log('info', '🚀 Initializing POS Payment Client...');
        
        // ✅ At this point, SignalR should be loaded (checked in _waitForSignalRAndInitialize)
        // But we double-check to be safe
        
        // ✅ Ensure $.signalR exists (required by signalr/hubs)
        // In SignalR 2.x, both $.signalR and $.connection should point to the same function
        // But sometimes $.signalR might not be set immediately
        if (typeof $.signalR === 'undefined' && typeof $.connection !== 'undefined') {
            // ✅ Create $.signalR alias if it doesn't exist (for compatibility)
            // According to SignalR 2.x source: $.connection = $.signalR = signalR;
            $.signalR = $.connection;
            this._log('info', '✅ Created $.signalR alias from $.connection');
        }
        
        // ✅ Final verification: Ensure $.signalR exists
        if (typeof $.signalR === 'undefined' && typeof $.connection === 'undefined') {
            this._log('error', '❌ SignalR library disappeared after initial check. This should not happen.');
            this.onError({
                code: 'SIGNALR_INVALID',
                message: 'کتابخانه SignalR به درستی بارگذاری نشده است. لطفاً صفحه را refresh کنید.'
            });
            return;
        }
        
        // ✅ Verify $.signalR is a function or object (as required by signalr/hubs)
        var signalRValue = $.signalR || $.connection;
        if (typeof signalRValue !== 'function' && typeof signalRValue !== 'object') {
            this._log('error', '❌ $.signalR is not a function or object. SignalR may not be properly loaded.');
            this.onError({
                code: 'SIGNALR_INVALID',
                message: 'کتابخانه SignalR به درستی بارگذاری نشده است. لطفاً صفحه را refresh کنید.'
            });
            return;
        }

        // Load SignalR Hubs dynamically
        this._loadSignalRHubs();
    };

    /**
     * Load SignalR Hubs Script
     * 
     * مسئولیت: بارگذاری dynamic hubs script برای رفع مشکل CSP
     * 
     * ⚠️ نکته مهم: signalr/hubs script نیاز به $.signalR دارد
     * بنابراین باید قبل از لود شدن hubs، مطمئن شویم که $.signalR موجود است
     */
    PosPaymentClient.prototype._loadSignalRHubs = function() {
        var self = this;
        var hubsUrl = this.config.signalRUrl + '/hubs';

        // ✅ CRITICAL: signalr/hubs script requires $.signalR to be a FUNCTION
        // It checks: if (typeof ($.signalR) !== "function")
        // So we MUST ensure $.signalR is a function before loading hubs

        // Step 1: Ensure $.signalR exists
        if (typeof $.signalR === 'undefined') {
            if (typeof $.connection !== 'undefined') {
                // ✅ Create alias from $.connection
                $.signalR = $.connection;
                this._log('info', '✅ Created $.signalR alias from $.connection');
            } else {
                this._log('error', '❌ $.signalR and $.connection both undefined. SignalR not loaded.');
                this.onError({
                    code: 'SIGNALR_NOT_LOADED',
                    message: 'کتابخانه SignalR بارگذاری نشده است. لطفاً صفحه را refresh کنید.'
                });
                return;
            }
        }

        // Step 2: CRITICAL - Verify $.signalR is a FUNCTION (not just object)
        // signalr/hubs script explicitly checks: typeof ($.signalR) !== "function"
        var signalRType = typeof $.signalR;
        this._log('info', '🔍 $.signalR type: ' + signalRType);
        
        // ✅ DEBUG: Log SignalR structure to understand how it's organized
        // Note: In SignalR 2.4.2, $.hubConnection is usually defined directly by the library
        // It's not necessarily a property of $.signalR.hubConnection
        // $.signalR.hubConnection being undefined is NORMAL and expected behavior
        if (typeof $.signalR === 'function') {
            this._log('info', '🔍 $.signalR is a function');
            // Check if $.signalR.hubConnection exists (it might not in SignalR 2.4.2 - this is normal)
            if (typeof $.signalR.hubConnection !== 'undefined') {
                this._log('info', '🔍 $.signalR.hubConnection type: ' + typeof $.signalR.hubConnection);
            }
            // Note: $.signalR.hubConnection being undefined is NORMAL in SignalR 2.4.2
            // $.hubConnection is usually defined directly by the library, not as a property
        }
        
        // ✅ DEBUG: Check $.connection structure
        if (typeof $.connection !== 'undefined') {
            this._log('info', '🔍 $.connection type: ' + typeof $.connection);
            if (typeof $.connection.hub !== 'undefined') {
                this._log('info', '🔍 $.connection.hub type: ' + typeof $.connection.hub);
            }
            // Check if $.hubConnection is already defined (it might be set by SignalR library)
            if (typeof $.hubConnection !== 'undefined') {
                this._log('info', '🔍 $.hubConnection already exists (type: ' + typeof $.hubConnection + ')');
            }
        }
        
        if (signalRType !== 'function') {
            // ✅ Try to get the actual signalR function from $.connection
            if (typeof $.connection === 'function') {
                $.signalR = $.connection;
                this._log('info', '✅ Set $.signalR to $.connection (function)');
            } else if (typeof $.connection !== 'undefined' && typeof $.connection.hub !== 'undefined') {
                // ✅ $.connection might be an object with hub property, but we need the function itself
                // In SignalR 2.x, $.connection should be the signalR function
                // Let's check if we can find the actual function
                var signalRFunction = window.signalR || $.connection;
                if (typeof signalRFunction === 'function') {
                    $.signalR = signalRFunction;
                    this._log('info', '✅ Set $.signalR from window.signalR or $.connection');
                } else {
                    this._log('error', '❌ $.signalR is not a function. Type: ' + signalRType);
                    this.onError({
                        code: 'SIGNALR_INVALID',
                        message: 'کتابخانه SignalR به درستی بارگذاری نشده است. $.signalR باید یک function باشد. لطفاً صفحه را refresh کنید.'
                    });
                    return;
                }
            } else {
                this._log('error', '❌ $.signalR is not a function. Type: ' + signalRType + ', $.connection type: ' + typeof $.connection);
                this.onError({
                    code: 'SIGNALR_INVALID',
                    message: 'کتابخانه SignalR به درستی بارگذاری نشده است. $.signalR باید یک function باشد. لطفاً صفحه را refresh کنید.'
                });
                return;
            }
        }

        // Step 3: Final verification - $.signalR MUST be a function
        if (typeof $.signalR !== 'function') {
            this._log('error', '❌ $.signalR is still not a function after fix attempt. Type: ' + typeof $.signalR);
            this.onError({
                code: 'SIGNALR_INVALID',
                message: 'کتابخانه SignalR به درستی بارگذاری نشده است. لطفاً صفحه را refresh کنید.'
            });
            return;
        }

        this._log('info', '📡 Loading SignalR hubs from: ' + hubsUrl);
        this._log('info', '✅ $.signalR verified as function: ' + (typeof $.signalR === 'function'));

        // ✅ CRITICAL: Store $.signalR reference before loading hubs script
        // The hubs script executes immediately and checks $.signalR at runtime
        // We need to ensure $.signalR is available when the script executes
        var signalRRef = $.signalR;
        var connectionRef = $.connection;
        
        // ✅ Ensure $.signalR is set globally before script execution
        // This is critical because the hubs script checks $.signalR at runtime
        if (typeof signalRRef === 'function') {
            // Double-check that it's still a function
            $.signalR = signalRRef;
            this._log('info', '✅ $.signalR reference stored and verified');
        }

        // ✅ CRITICAL: Ensure $.signalR is a function BEFORE loading hubs script
        // The hubs script checks: if (typeof ($.signalR) !== "function")
        // We must ensure $.signalR is available and is a function when the script executes
        // This is a timing issue: bundles may load scripts asynchronously
        
        // Double-check that $.signalR is still a function right before loading
        if (typeof $.signalR !== 'function') {
            this._log('warn', '⚠️ $.signalR is not a function right before loading hubs, attempting to restore...');
            if (typeof signalRRef === 'function') {
                $.signalR = signalRRef;
                this._log('info', '✅ $.signalR restored from stored reference before loading hubs');
            } else if (typeof connectionRef !== 'undefined' && typeof connectionRef === 'function') {
                $.signalR = connectionRef;
                this._log('info', '✅ $.signalR restored from $.connection before loading hubs');
            } else {
                // Last resort: try to get it from window
                if (typeof window.signalR === 'function') {
                    $.signalR = window.signalR;
                    this._log('info', '✅ $.signalR restored from window.signalR before loading hubs');
                } else {
                    this._log('error', '❌ Cannot restore $.signalR before loading hubs');
                    this.onError({
                        code: 'SIGNALR_NOT_READY',
                        message: 'کتابخانه SignalR آماده نیست. لطفاً صفحه را refresh کنید.'
                    });
                    return;
                }
            }
        }
        
        // ✅ Final verification: $.signalR MUST be a function
        if (typeof $.signalR !== 'function') {
            this._log('error', '❌ $.signalR is still not a function after restore attempt');
            this.onError({
                code: 'SIGNALR_INVALID',
                message: 'کتابخانه SignalR به درستی بارگذاری نشده است. لطفاً صفحه را refresh کنید.'
            });
            return;
        }
        
        // ✅ CRITICAL: Use Object.defineProperty to ensure $.signalR is always a function
        // This prevents the hubs script from seeing $.signalR as undefined or non-function
        // We need to ensure $.signalR is available in the global scope when the script executes
        var signalRFunctionRef = $.signalR;
        
        // ✅ Ensure $.signalR is set on window.jQuery as well (for cross-scope access)
        if (typeof window.jQuery !== 'undefined' && typeof signalRFunctionRef === 'function') {
            if (typeof window.jQuery.signalR === 'undefined' || typeof window.jQuery.signalR !== 'function') {
                window.jQuery.signalR = signalRFunctionRef;
                this._log('info', '✅ $.signalR set on window.jQuery for cross-scope access');
            }
        }
        
        // ✅ Ensure $.signalR is set on window as well (fallback)
        if (typeof window !== 'undefined' && typeof signalRFunctionRef === 'function') {
            if (typeof window.signalR === 'undefined' || typeof window.signalR !== 'function') {
                window.signalR = signalRFunctionRef;
                this._log('info', '✅ $.signalR set on window for cross-scope access');
            }
        }
        
        // ✅ Final check: Ensure $.signalR is still a function right before creating script element
        if (typeof $.signalR !== 'function') {
            this._log('warn', '⚠️ $.signalR is not a function right before creating script element, restoring...');
            if (typeof signalRFunctionRef === 'function') {
                $.signalR = signalRFunctionRef;
            } else if (typeof window.jQuery !== 'undefined' && typeof window.jQuery.signalR === 'function') {
                $.signalR = window.jQuery.signalR;
            } else if (typeof window.signalR === 'function') {
                $.signalR = window.signalR;
            } else {
                this._log('error', '❌ Cannot restore $.signalR before creating script element');
                this.onError({
                    code: 'SIGNALR_NOT_READY',
                    message: 'کتابخانه SignalR آماده نیست. لطفاً صفحه را refresh کنید.'
                });
                return;
            }
        }
        
        var hubsScript = document.createElement('script');
        hubsScript.src = hubsUrl;
        
        // ✅ Set a flag to ensure $.signalR is available when script executes
        // This is a workaround for the timing issue
        hubsScript.setAttribute('data-signalr-ready', 'true');
        
        // ✅ CRITICAL: Ensure $.hubConnection is available before loading hubs script
        // hubs script uses: $.hubConnection.prototype.createHubProxies
        // In SignalR 2.x, $.hubConnection should be available on $.signalR or $.connection
        // From hubs script analysis: signalR.hub = $.hubConnection("/signalr", { useDefaultPath: false });
        // So $.hubConnection must be a constructor function
        
        // ✅ CRITICAL: Ensure $.hubConnection is available before loading hubs script
        // hubs script uses: $.hubConnection.prototype.createHubProxies
        // In SignalR 2.4.2, $.hubConnection is usually defined directly by the library
        // It might be on $.connection or $.signalR, or it might be set globally
        
        // ✅ DEBUG: Log current state (only if needed)
        if (typeof $.hubConnection === 'undefined') {
            this._log('warn', '⚠️ $.hubConnection is undefined, attempting to find it...');
            
            // Strategy 1: Try to get it from $.signalR.hubConnection (might not exist in SignalR 2.4.2)
            if (typeof $.signalR !== 'undefined' && typeof $.signalR.hubConnection !== 'undefined') {
                $.hubConnection = $.signalR.hubConnection;
                this._log('info', '✅ $.hubConnection set from $.signalR.hubConnection (type: ' + typeof $.signalR.hubConnection + ')');
            }
            
            // Strategy 2: Try to get it from $.connection.hub (in SignalR 2.4.2, this is usually an object, not a function)
            // But we can check if it has the createHubProxies method
            if (typeof $.hubConnection === 'undefined' && typeof $.connection !== 'undefined' && typeof $.connection.hub !== 'undefined') {
                // In SignalR 2.4.2, $.connection.hub is usually an object (hub connection instance)
                // But $.hubConnection should be the constructor function
                // Let's check if $.connection.hub.constructor might be it
                if (typeof $.connection.hub.constructor !== 'undefined' && typeof $.connection.hub.constructor.prototype !== 'undefined' && typeof $.connection.hub.constructor.prototype.createHubProxies !== 'undefined') {
                    $.hubConnection = $.connection.hub.constructor;
                    this._log('info', '✅ $.hubConnection set from $.connection.hub.constructor');
                } else if (typeof $.connection.hub === 'function') {
                    $.hubConnection = $.connection.hub;
                    this._log('info', '✅ $.hubConnection set from $.connection.hub (type: function)');
                }
            }
            
            // Strategy 3: Try window.jQuery.hubConnection
            if (typeof $.hubConnection === 'undefined' && typeof window.jQuery !== 'undefined' && typeof window.jQuery.hubConnection !== 'undefined') {
                $.hubConnection = window.jQuery.hubConnection;
                this._log('info', '✅ $.hubConnection set from window.jQuery.hubConnection');
            }
            
            // Strategy 4: Last resort - check if $.connection itself might be hubConnection
            if (typeof $.hubConnection === 'undefined' && typeof $.connection === 'function') {
                if ($.connection.prototype && typeof $.connection.prototype.createHubProxies !== 'undefined') {
                    $.hubConnection = $.connection;
                    this._log('info', '✅ $.hubConnection set from $.connection (has createHubProxies)');
                }
            }
            
            // Final check
            if (typeof $.hubConnection === 'undefined') {
                this._log('error', '❌ Cannot find $.hubConnection anywhere. Hubs script will fail.');
                this._log('error', '🔍 Available: $.signalR=' + typeof $.signalR + ', $.connection=' + typeof $.connection);
            } else {
                this._log('info', '✅ $.hubConnection found and set (type: ' + typeof $.hubConnection + ')');
            }
        } else {
            // ✅ $.hubConnection already exists - this is the normal case in SignalR 2.4.2
            // No need to log this as it's expected behavior
        }
        
        // ✅ CRITICAL: Also set on window.jQuery for cross-scope access
        if (typeof $.hubConnection !== 'undefined' && typeof window.jQuery !== 'undefined') {
            if (typeof window.jQuery.hubConnection === 'undefined') {
                window.jQuery.hubConnection = $.hubConnection;
                this._log('info', '✅ $.hubConnection set on window.jQuery for cross-scope access');
            }
        }
        
        // ✅ CRITICAL: Set up a global check function that hubs script can use
        // This ensures $.signalR and $.hubConnection are available when the script executes
        var originalSignalR = $.signalR;
        var originalHubConnection = $.hubConnection;
        var checkAndSetSignalR = function() {
            if (typeof $.signalR !== 'function') {
                if (typeof originalSignalR === 'function') {
                    $.signalR = originalSignalR;
                } else if (typeof window.jQuery !== 'undefined' && typeof window.jQuery.signalR === 'function') {
                    $.signalR = window.jQuery.signalR;
                } else if (typeof window.signalR === 'function') {
                    $.signalR = window.signalR;
                }
            }
            // Also ensure $.hubConnection is available
            if (typeof $.hubConnection === 'undefined') {
                if (typeof originalHubConnection !== 'undefined') {
                    $.hubConnection = originalHubConnection;
                } else if (typeof $.signalR !== 'undefined' && typeof $.signalR.hubConnection !== 'undefined') {
                    $.hubConnection = $.signalR.hubConnection;
                } else if (typeof $.connection !== 'undefined' && typeof $.connection.hub !== 'undefined' && typeof $.connection.hub === 'function') {
                    $.hubConnection = $.connection.hub;
                } else if (typeof window.jQuery !== 'undefined' && typeof window.jQuery.hubConnection !== 'undefined') {
                    $.hubConnection = window.jQuery.hubConnection;
                }
            }
        };
        
        // ✅ Execute check before script loads (in case script executes synchronously)
        checkAndSetSignalR();
        
        hubsScript.onerror = function(error) {
            self._log('error', '❌ Failed to load SignalR hubs from: ' + hubsUrl);
            
            // ✅ تشخیص نوع خطا برای پیام بهتر
            var errorMessage = 'عدم امکان بارگذاری SignalR Hubs.';
            var errorDetails = '';
            
            // بررسی نوع خطا از console یا error object
            if (error && error.message) {
                errorDetails = error.message;
            }
            
            // ✅ تشخیص خطا: ERR_CONNECTION_REFUSED معمولاً در console log می‌شود
            // اگر error object موجود نباشد یا message نداشته باشد، احتمالاً connection refused است
            var isConnectionRefused = false;
            
            if (errorDetails) {
                isConnectionRefused = errorDetails.indexOf('CONNECTION_REFUSED') !== -1 || 
                                     errorDetails.indexOf('ERR_CONNECTION_REFUSED') !== -1 ||
                                     errorDetails.indexOf('Failed to load resource') !== -1;
            } else {
                // اگر error object موجود نیست، احتمالاً ERR_CONNECTION_REFUSED است
                // چون script نمی‌تواند لود شود
                isConnectionRefused = true;
            }
            
            // ✅ پیام خطای دقیق‌تر بر اساس نوع خطا
            if (isConnectionRefused) {
                errorMessage = 'سرویس SSP1126 در حال اجرا نیست یا به درستی listen نمی‌کند.\n\n' +
                             '🔧 راه‌حل:\n' +
                             '1. بررسی وضعیت Service:\n' +
                             '   PowerShell: Get-Service -Name "SSP1126Service1"\n' +
                             '2. اگر Service متوقف است، راه‌اندازی کنید:\n' +
                             '   PowerShell: Start-Service -Name "SSP1126Service1"\n' +
                             '3. بررسی کنید که Service روی Port 5000 listen می‌کند:\n' +
                             '   PowerShell: netstat -ano | findstr :5000 | findstr LISTENING\n' +
                             '   اگر خروجی خالی است، Service listen نمی‌کند\n' +
                             '4. بررسی URL:\n' +
                             '   - Web.config: ' + hubsUrl + '\n' +
                             '   - Service Config: HostUrl = 192.168.1.103\n' +
                             '   - اگر Service روی IP دیگری است، URL را تغییر دهید\n' +
                             '5. Restart Service:\n' +
                             '   PowerShell: Restart-Service -Name "SSP1126Service1"\n' +
                             '6. Log های Service را بررسی کنید: C:\\Log\\';
            } else if (errorDetails && (errorDetails.indexOf('404') !== -1 || errorDetails.indexOf('Not Found') !== -1)) {
                errorMessage = 'آدرس SignalR Hubs یافت نشد.\n\n' +
                             '🔧 راه‌حل:\n' +
                             '1. URL را بررسی کنید: ' + hubsUrl + '\n' +
                             '2. Service را restart کنید:\n' +
                             '   PowerShell: Restart-Service -Name "SSP1126Service1"';
            } else if (errorDetails && (errorDetails.indexOf('timeout') !== -1 || errorDetails.indexOf('TIMEOUT') !== -1)) {
                errorMessage = 'زمان انتظار برای بارگذاری SignalR Hubs به پایان رسید.\n\n' +
                             '🔧 راه‌حل:\n' +
                             '1. Service را بررسی کنید\n' +
                             '2. شبکه و فایروال را بررسی کنید\n' +
                             '3. Service را restart کنید';
            } else {
                // خطای عمومی
                errorMessage = 'عدم امکان بارگذاری SignalR Hubs از: ' + hubsUrl + '\n\n' +
                             '🔧 راه‌حل:\n' +
                             '1. Windows Service "SSP1126Service1" را بررسی کنید\n' +
                             '2. Service را restart کنید:\n' +
                             '   PowerShell: Restart-Service -Name "SSP1126Service1"\n' +
                             '3. Log های Service را بررسی کنید: C:\\Log\\';
            }
            
            self.onError({
                code: 'HUBS_LOAD_FAILED',
                message: errorMessage,
                details: errorDetails || 'Connection refused or service not running',
                url: hubsUrl,
                troubleshooting: {
                    serviceName: 'SSP1126Service1',
                    port: 5000,
                    logPath: 'C:\\Log\\',
                    configFile: 'SSP1126SignalRWindowsService.exe.config'
                }
            });
        };

        hubsScript.onload = function() {
            // ✅ Double-check $.signalR after script loads
            // Execute check function to ensure $.signalR is set
            checkAndSetSignalR();
            
            if (typeof $.signalR !== 'function') {
                self._log('warn', '⚠️ $.signalR is not a function after hubs loaded, attempting to restore...');
                if (typeof signalRRef === 'function') {
                    $.signalR = signalRRef;
                    self._log('info', '✅ $.signalR restored from stored reference');
                } else if (typeof connectionRef !== 'undefined' && typeof connectionRef === 'function') {
                    $.signalR = connectionRef;
                    self._log('info', '✅ $.signalR restored from $.connection');
                } else if (typeof window.jQuery !== 'undefined' && typeof window.jQuery.signalR === 'function') {
                    $.signalR = window.jQuery.signalR;
                    self._log('info', '✅ $.signalR restored from window.jQuery.signalR');
                } else if (typeof window.signalR === 'function') {
                    $.signalR = window.signalR;
                    self._log('info', '✅ $.signalR restored from window.signalR');
                }
            }
            
            // ✅ CRITICAL: hubs script Hub ها را به signalR اضافه می‌کند، نه $.connection
            // از خروجی hubs script: $.extend(signalR, signalR.hub.createHubProxies())
            // پس باید Hub ها را از $.signalR به $.connection کپی کنیم
            if (typeof $.signalR === 'object' && $.signalR[self.config.hubName]) {
                // Hub در $.signalR است، آن را به $.connection اضافه می‌کنیم
                if (typeof $.connection === 'undefined') {
                    $.connection = {};
                }
                if (!$.connection[self.config.hubName]) {
                    $.connection[self.config.hubName] = $.signalR[self.config.hubName];
                    self._log('info', '✅ Hub "' + self.config.hubName + '" copied from $.signalR to $.connection');
                }
            }
            
            // ✅ Wait a bit for hubs to fully initialize
            setTimeout(function() {
                self._log('info', '✅ SignalR hubs loaded successfully');
                self.hubsScriptLoaded = true;
                self._connectToHub();
            }, 100);
        };

        document.head.appendChild(hubsScript);
    };

    /**
     * Connect to SignalR Hub
     * 
     * مسئولیت: ایجاد اتصال به SignalR Hub و ثبت Event Handlers
     */
    PosPaymentClient.prototype._connectToHub = function() {
        var self = this;

        if (!this.hubsScriptLoaded) {
            this._log('warn', '⚠️ Hubs script not loaded yet, waiting...');
            setTimeout(function() {
                self._connectToHub();
            }, 500);
            return;
        }

        try {
            // ✅ Verify $.connection and $.connection.hub exist
            if (typeof $.connection === 'undefined') {
                this._log('error', '❌ $.connection is undefined');
                this.onError({
                    code: 'CONNECTION_UNDEFINED',
                    message: 'اتصال SignalR برقرار نشده است. لطفاً صفحه را refresh کنید.'
                });
                return;
            }

            if (typeof $.connection.hub === 'undefined') {
                this._log('error', '❌ $.connection.hub is undefined');
                this.onError({
                    code: 'HUB_UNDEFINED',
                    message: 'SignalR Hub در دسترس نیست. لطفاً صفحه را refresh کنید.'
                });
                return;
            }

            // Set SignalR Hub URL
            $.connection.hub.url = this.config.signalRUrl;
            this._log('info', '✅ SignalR Hub URL set to: ' + this.config.signalRUrl);
            
            // ✅ Debug: Log available hubs
            // در SignalR 2.x، Hub ها ممکن است در $.connection یا $.signalR باشند
            // hubs script Hub ها را به signalR اضافه می‌کند: $.extend(signalR, signalR.hub.createHubProxies())
            // پس باید هم $.connection و هم $.signalR را بررسی کنیم
            
            var hubFound = false;
            var hubSource = null;
            
            // بررسی $.connection
            if (typeof $.connection !== 'undefined') {
                var availableHubs = Object.keys($.connection).filter(function(key) {
                    return key !== 'hub' && key !== 'signalR' && typeof $.connection[key] === 'object';
                });
                this._log('info', '🔍 Available hubs in $.connection: ' + (availableHubs.length > 0 ? availableHubs.join(', ') : 'none'));
                
                if ($.connection[this.config.hubName]) {
                    this.posHub = $.connection[this.config.hubName];
                    hubFound = true;
                    hubSource = '$.connection';
                }
            }
            
            // بررسی $.signalR (hubs script Hub ها را به signalR اضافه می‌کند)
            if (!hubFound && typeof $.signalR !== 'undefined' && typeof $.signalR === 'object') {
                var signalRHubs = Object.keys($.signalR || {}).filter(function(key) {
                    return key !== 'hub' && key !== 'connection' && typeof $.signalR[key] === 'object' && $.signalR[key].hubName;
                });
                this._log('info', '🔍 Available hubs in $.signalR: ' + (signalRHubs.length > 0 ? signalRHubs.join(', ') : 'none'));
                
                if ($.signalR[this.config.hubName]) {
                    this.posHub = $.signalR[this.config.hubName];
                    hubFound = true;
                    hubSource = '$.signalR';
                }
            }
            
            // بررسی window.signalR (fallback)
            if (!hubFound && typeof window.signalR !== 'undefined' && typeof window.signalR === 'object') {
                if (window.signalR[this.config.hubName]) {
                    this.posHub = window.signalR[this.config.hubName];
                    hubFound = true;
                    hubSource = 'window.signalR';
                }
            }
            
            this._log('info', '🔍 Looking for hub: ' + this.config.hubName);
            
            if (!hubFound) {
                this._log('error', '❌ Hub "' + this.config.hubName + '" not found in $.connection, $.signalR, or window.signalR');
                this._log('error', '🔍 $.connection keys: ' + Object.keys($.connection || {}).join(', '));
                if (typeof $.signalR === 'object') {
                    this._log('error', '🔍 $.signalR keys: ' + Object.keys($.signalR || {}).join(', '));
                }
                this.onError({
                    code: 'HUB_NOT_FOUND',
                    message: 'Hub "' + this.config.hubName + '" یافت نشد. آیا سرویس SSP1126 در حال اجراست؟\n\n' +
                             'لطفاً:\n' +
                             '1. Service را بررسی کنید: Get-Service -Name "SSP1126Service1"\n' +
                             '2. Port 5000 را بررسی کنید: netstat -ano | findstr :5000 | findstr LISTENING\n' +
                             '3. Service را restart کنید: Restart-Service -Name "SSP1126Service1"'
                });
                return;
            }
            
            this._log('info', '✅ Hub "' + this.config.hubName + '" found in ' + hubSource);
            
            this._log('info', '✅ Hub "' + this.config.hubName + '" found and connected');

            // Register Event Handlers
            this._registerEventHandlers();

            // Start Connection
            this._startConnection();
        } catch (ex) {
            this._log('error', '❌ Error connecting to Hub: ' + ex.message);
            this.onError({
                code: 'CONNECTION_ERROR',
                message: 'خطا در اتصال به Hub: ' + ex.message
            });
        }
    };

    /**
     * Register SignalR Event Handlers
     * 
     * مسئولیت: ثبت Callbacks برای Events از SignalR Hub
     */
    PosPaymentClient.prototype._registerEventHandlers = function() {
        var self = this;

        // GetSystemResponse: پاسخ اولیه از Initial
        this.posHub.client.GetSystemResponse = function(message) {
            self._log('info', '🔔 GetSystemResponse received: ' + message);
            self.serverMessage = message;
        };

        // GetCardSwiped: کارت کشیده شد
        this.posHub.client.GetCardSwiped = function(terminalId, cardNumberHash, cardNumberMask, purchaseTypes, encryptedNationalCode) {
            self._log('info', '🔔 GetCardSwiped received', {
                terminalId: terminalId,
                cardMask: cardNumberMask
            });
            
            self.onCardSwiped({
                terminalId: terminalId,
                cardNumberHash: cardNumberHash,
                cardNumberMask: cardNumberMask,
                purchaseTypes: purchaseTypes,
                encryptedNationalCode: encryptedNationalCode
            });
        };

        // GetTransactionResponse: پاسخ نهایی تراکنش
        this.posHub.client.GetTransactionResponse = function(terminalId, responseCode, serialId, rrn, responseDescription, txnDate, amount, cardNumberMask) {
            self._log('info', '🔔🔔🔔 GetTransactionResponse CALLBACK INVOKED!!!', {
                terminalId: terminalId,
                responseCode: responseCode,
                serialId: serialId,
                rrn: rrn,
                responseDescription: responseDescription,
                txnDate: txnDate,
                amount: amount,
                cardNumberMask: cardNumberMask
            });

            // ✅ Clear timeout if response received
            if (self.currentPayment && self.currentPayment.transactionTimeoutId) {
                clearTimeout(self.currentPayment.transactionTimeoutId);
                self.currentPayment.transactionTimeoutId = null;
            }

            // ✅ Mark response as received
            self.transactionResponseReceived = true;

            self._handleTransactionResponse({
                terminalId: terminalId,
                responseCode: responseCode,
                serialId: serialId,
                rrn: rrn,
                responseDescription: responseDescription,
                txnDate: txnDate,
                amount: amount,
                cardNumberMask: cardNumberMask
            });
        };
    };

    /**
     * Start SignalR Connection
     * 
     * مسئولیت: شروع اتصال به SignalR Hub
     */
    PosPaymentClient.prototype._startConnection = function() {
        var self = this;

        this.onConnecting();

        $.connection.hub.start()
            .done(function() {
                self._log('info', '✅ Connected to POS SignalR Hub');
                self.isConnected = true;
                self.onConnected();
            })
            .fail(function(error) {
                self._log('error', '❌ Failed to connect to POS SignalR Hub: ' + error);
                self.isConnected = false;
                self.onError({
                    code: 'CONNECTION_FAILED',
                    message: 'عدم ارتباط با سرویس پوز. آیا سرویس SSP1126 در حال اجراست؟',
                    error: error
                });
            });

        // Connection State Management
        $.connection.hub.connectionSlow(function() {
            self._log('warn', '⚠️ Connection slow');
        });

        $.connection.hub.reconnecting(function() {
            self._log('warn', '⚠️ Reconnecting...');
        });

        $.connection.hub.reconnected(function() {
            self._log('info', '✅ Reconnected');
            self.isConnected = true;
            self.onConnected();
        });

        $.connection.hub.disconnected(function() {
            self._log('error', '❌ Disconnected');
            self.isConnected = false;
            self.onDisconnected();
        });
    };

    /**
     * Process Payment
     * 
     * مسئولیت: پردازش پرداخت POS با Retry Logic
     * 
     * @param {number} terminalId - Terminal ID
     * @param {number} amount - Amount in Rials
     * @param {string} ipAddress - Terminal IP Address
     */
    PosPaymentClient.prototype.processPayment = function(terminalId, amount, ipAddress) {
        var self = this;

        // Validation
        if (this.isProcessing) {
            this._log('warn', '⚠️ Payment already in progress');
            this.onError({
                code: 'PAYMENT_IN_PROGRESS',
                message: 'یک پرداخت در حال انجام است. لطفاً صبر کنید.'
            });
            return;
        }

        if (!this.isConnected) {
            this._log('error', '❌ Not connected to SignalR Hub');
            // Try to reconnect
            this._attemptReconnect(function() {
                if (self.isConnected) {
                    self.processPayment(terminalId, amount, ipAddress);
                } else {
                    self.onError({
                        code: 'NOT_CONNECTED',
                        message: 'اتصال به SignalR Hub برقرار نیست. لطفاً دوباره تلاش کنید.'
                    });
                }
            });
            return;
        }

        if (!terminalId || !amount || amount <= 0) {
            this._log('error', '❌ Invalid parameters');
            this.onError({
                code: 'INVALID_PARAMETERS',
                message: 'پارامترهای ورودی نامعتبر است'
            });
            return;
        }

        // Store current payment info
        this.currentPayment = {
            terminalId: terminalId,
            amount: amount,
            ipAddress: ipAddress,
            startTime: new Date(),
            retryCount: 0,
            transactionTimeoutId: null // ✅ برای مدیریت timeout
        };

        // ✅ Reset transaction response flag
        this.transactionResponseReceived = false;

        this.isProcessing = true;
        this.retryCount = 0;
        this.serverMessage = '';

        this._log('info', '🚀 Starting payment process', {
            terminalId: terminalId,
            amount: amount,
            ipAddress: ipAddress
        });

        // Step 1: Initial with Retry Logic
        this._initializeTerminalWithRetry(terminalId, ipAddress);
    };

    /**
     * Initialize Terminal with Retry Logic
     * 
     * مسئولیت: مقداردهی اولیه ترمینال با Retry Logic
     */
    PosPaymentClient.prototype._initializeTerminalWithRetry = function(terminalId, ipAddress) {
        var self = this;

        if (this.currentPayment.retryCount >= this.config.maxRetries) {
            this._log('error', '❌ Max retries reached');
            this.isProcessing = false;
            this.onError({
                code: 'MAX_RETRIES_REACHED',
                message: 'حداکثر تعداد تلاش‌ها به پایان رسید. لطفاً دوباره تلاش کنید.'
            });
            return;
        }

        this._log('info', '🔧 Initializing terminal (Attempt: ' + (this.currentPayment.retryCount + 1) + '/' + this.config.maxRetries + ')');

        // Set timeout for initialization
        var initTimeout = setTimeout(function() {
            if (self.isProcessing && !self.serverMessage) {
                self._log('warn', '⚠️ Initial timeout, retrying...');
                self.currentPayment.retryCount++;
                setTimeout(function() {
                    self._initializeTerminalWithRetry(terminalId, ipAddress);
                }, self.config.retryDelay);
            }
        }, this.config.connectionTimeout);

        // Initialize
        this._initializeTerminal(terminalId, ipAddress, function() {
            clearTimeout(initTimeout);
        });
    };

    /**
     * Attempt Reconnect
     * 
     * مسئولیت: تلاش برای اتصال مجدد
     */
    PosPaymentClient.prototype._attemptReconnect = function(callback) {
        var self = this;
        this._log('info', '🔄 Attempting to reconnect...');
        
        this._connectToHub();
        
        // Wait for connection
        var checkInterval = setInterval(function() {
            if (self.isConnected) {
                clearInterval(checkInterval);
                if (callback) callback();
            }
        }, 500);

        // Timeout after 10 seconds
        setTimeout(function() {
            clearInterval(checkInterval);
            if (!self.isConnected && callback) {
                callback();
            }
        }, 10000);
    };

    /**
     * Initialize Terminal (Step 1)
     * 
     * مسئولیت: فراخوانی متد Initial برای آماده‌سازی ترمینال
     * 
     * @param {Function} onComplete - Callback when initialization completes
     */
    PosPaymentClient.prototype._initializeTerminal = function(terminalId, ipAddress, onComplete) {
        var self = this;

        try {
            this._log('info', '🔧 Initializing terminal...');

            // Check connection before invoking
            if (!this.isConnected || !this.posHub) {
                this._log('error', '❌ Not connected to Hub');
                if (onComplete) onComplete();
                return;
            }

            // پارامترها: MediaType (1=Network), IP, COM (null), AccountType (0=Single), Language (0=Farsi), Additional ("0")
            this.posHub.server.Initial(
                1, // MediaType: Network
                ipAddress, // IP
                null, // COM Port (null for Network)
                0, // AccountType: Single
                0, // Language: Farsi
                '0' // Additional
            );

            this._log('info', '✅ Initial invoked, waiting for GetSystemResponse...');

            // Wait for GetSystemResponse
            var responseReceived = false;
            var checkInterval = setInterval(function() {
                if (self.serverMessage !== '') {
                    responseReceived = true;
                    clearInterval(checkInterval);
                    if (onComplete) onComplete();

                    if (self.serverMessage === '0') {
                        self._log('info', '✅ Initial successful, sending amount...');
                        // Step 2: SendAmount1Step
                        self._sendAmount(terminalId, self.currentPayment.amount);
                    } else {
                        self._log('error', '❌ Initial failed - Response: ' + self.serverMessage);
                        
                        // Retry if retry count not exceeded
                        if (self.currentPayment.retryCount < self.config.maxRetries) {
                            self.currentPayment.retryCount++;
                            self._log('info', '🔄 Retrying initialization... (Attempt: ' + (self.currentPayment.retryCount + 1) + ')');
                            setTimeout(function() {
                                self.serverMessage = ''; // Reset
                                self._initializeTerminalWithRetry(terminalId, ipAddress);
                            }, self.config.retryDelay);
                        } else {
                            self.isProcessing = false;
                            self.onError({
                                code: 'INITIAL_FAILED',
                                message: 'خطا در اتصال به دستگاه POS. کد خطا: ' + self.serverMessage
                            });
                        }
                    }
                }
            }, 100);

            // Timeout
            setTimeout(function() {
                if (!responseReceived) {
                    clearInterval(checkInterval);
                    if (onComplete) onComplete();
                    
                    if (self.currentPayment.retryCount < self.config.maxRetries) {
                        self.currentPayment.retryCount++;
                        self._log('warn', '⚠️ Initial timeout, retrying... (Attempt: ' + (self.currentPayment.retryCount + 1) + ')');
                        self.serverMessage = ''; // Reset
                        setTimeout(function() {
                            self._initializeTerminalWithRetry(terminalId, ipAddress);
                        }, self.config.retryDelay);
                    } else {
                        self.isProcessing = false;
                        self.onError({
                            code: 'INITIAL_TIMEOUT',
                            message: 'زمان انتظار برای مقداردهی اولیه به پایان رسید'
                        });
                    }
                }
            }, this.config.connectionTimeout);
        } catch (ex) {
            this._log('error', '❌ Error in Initial: ' + ex.message);
            if (onComplete) onComplete();
            
            if (this.currentPayment.retryCount < this.config.maxRetries) {
                this.currentPayment.retryCount++;
                setTimeout(function() {
                    self._initializeTerminalWithRetry(terminalId, ipAddress);
                }, this.config.retryDelay);
            } else {
                this.isProcessing = false;
                this.onError({
                    code: 'INITIAL_ERROR',
                    message: 'خطا در مقداردهی اولیه: ' + ex.message
                });
            }
        }
    };

    /**
     * Send Amount (Step 2)
     * 
     * مسئولیت: ارسال مبلغ به ترمینال با Timeout Handling
     */
    PosPaymentClient.prototype._sendAmount = function(terminalId, amount) {
        var self = this;

        try {
            this._log('info', '💰 Sending amount: ' + amount + ' Rials');

            // Reset transaction response flag
            this.transactionResponseReceived = false;

            // پارامترها: Amount, Amounts (null), Additional (null), Reference (null), PurchaseID (null), TerminalID
            this.posHub.server.SendAmount1Step(
                amount.toString(), // Main Amount
                null, // Amounts (null for Single Account)
                null, // Additional Data
                null, // Reference Data
                null, // PurchaseID
                terminalId // TerminalID
            );

            this._log('info', '✅ SendAmount1Step invoked, waiting for GetTransactionResponse...');

            // ✅ Timeout Handling: اگر در زمان معین پاسخ دریافت نشد
            var transactionTimeout = setTimeout(function() {
                if (!self.transactionResponseReceived && self.isProcessing) {
                    self._log('error', '❌ Transaction timeout - No response received from POS device');
                    self.isProcessing = false;
                    self.onError({
                        code: 'TRANSACTION_TIMEOUT',
                        message: 'زمان انتظار برای پاسخ تراکنش به پایان رسید.\n\n' +
                                'لطفاً:\n' +
                                '• کارت را روی دستگاه بکشید\n' +
                                '• یا دکمه لغو را روی دستگاه بزنید\n' +
                                '• یا دوباره تلاش کنید'
                    });
                    self.currentPayment = null;
                }
            }, this.config.transactionTimeout); // از config استفاده می‌کند (60 seconds default)

            // Store timeout ID for cleanup
            this.currentPayment.transactionTimeoutId = transactionTimeout;
        } catch (ex) {
            this._log('error', '❌ Error in SendAmount1Step: ' + ex.message);
            this.isProcessing = false;
            this.onError({
                code: 'SEND_AMOUNT_ERROR',
                message: 'خطا در ارسال مبلغ: ' + ex.message
            });
        }
    };

    /**
     * Handle Transaction Response
     * 
     * مسئولیت: پردازش پاسخ نهایی تراکنش از POS
     */
    PosPaymentClient.prototype._handleTransactionResponse = function(response) {
        var self = this;

        // ✅ Prevent duplicate handling
        if (!this.isProcessing) {
            this._log('warn', '⚠️ Transaction response received but payment is not in progress');
            return;
        }

        this.isProcessing = false;

        // ✅ Clear timeout if exists
        if (this.currentPayment && this.currentPayment.transactionTimeoutId) {
            clearTimeout(this.currentPayment.transactionTimeoutId);
            this.currentPayment.transactionTimeoutId = null;
        }

        var isSuccess = response.responseCode === '0' || response.responseCode === '00';
        var isCancel = response.responseCode === '98';

        // Parse response
        var paymentResponse = {
            success: isSuccess,
            canceled: isCancel,
            terminalId: response.terminalId,
            responseCode: response.responseCode,
            rrn: response.rrn || '',
            traceNo: response.serialId || '',
            cardLast4: response.cardNumberMask ? response.cardNumberMask.slice(-4) : '',
            amount: response.amount ? parseFloat(response.amount) : (this.currentPayment ? this.currentPayment.amount : 0),
            message: this._getResponseMessage(response.responseCode, response.responseDescription),
            txnDate: response.txnDate,
            cardNumberMask: response.cardNumberMask
        };

        // Calculate duration
        if (this.currentPayment && this.currentPayment.startTime) {
            paymentResponse.durationMs = new Date() - this.currentPayment.startTime;
        }

        this._log('info', isSuccess ? '✅ Payment successful' : (isCancel ? '⚠️ Payment canceled' : '❌ Payment failed'), paymentResponse);

        // Store payment response for cleanup
        var currentPayment = this.currentPayment;

        // Clear current payment BEFORE calling callbacks (to prevent duplicate handling)
        this.currentPayment = null;
        this.transactionResponseReceived = false;

        // Call appropriate callback
        try {
            if (isSuccess) {
                this.onSuccess(paymentResponse);
            } else if (isCancel) {
                this.onCancel(paymentResponse);
            } else {
                this.onError({
                    code: 'PAYMENT_FAILED',
                    message: paymentResponse.message,
                    responseCode: response.responseCode,
                    response: paymentResponse
                });
            }
        } catch (ex) {
            this._log('error', '❌ Error in callback handler: ' + ex.message);
        }
    };

    /**
     * Get Response Message
     * 
     * مسئولیت: تبدیل Response Code به پیام قابل فهم
     */
    PosPaymentClient.prototype._getResponseMessage = function(responseCode, responseDescription) {
        if (responseDescription && responseDescription.trim()) {
            return responseDescription.trim();
        }

        var messages = {
            '0': 'تراکنش با موفقیت انجام شد',
            '00': 'تراکنش با موفقیت انجام شد',
            '98': 'عملیات توسط کاربر لغو شد',
            '55': 'رمز کارت نامعتبر است',
            '51': 'موجودی حساب کافی نیست',
            '54': 'کارت منقضی شده است',
            '61': 'مبلغ تراکنش بیش از حد مجاز است',
            '75': 'تعداد تلاش برای وارد کردن رمز بیش از حد مجاز است'
        };

        return messages[responseCode] || ('خطا: ' + responseCode);
    };

    /**
     * Disconnect
     * 
     * مسئولیت: قطع اتصال از SignalR Hub
     */
    PosPaymentClient.prototype.disconnect = function() {
        if (this.posHub && $.connection.hub) {
            $.connection.hub.stop();
            this.isConnected = false;
            this._log('info', '🔌 Disconnected from SignalR Hub');
        }
    };

    /**
     * Logging Helper
     * 
     * مسئولیت: لاگ کردن پیام‌ها
     */
    PosPaymentClient.prototype._log = function(level, message, data) {
        if (!this.config.enableLogging) {
            return;
        }

        var logMessage = '[PosPaymentClient] ' + message;
        
        if (data) {
            logMessage += ' ' + JSON.stringify(data);
        }

        if (this.config.enableConsoleLog) {
            switch (level) {
                case 'error':
                    console.error(logMessage);
                    break;
                case 'warn':
                    console.warn(logMessage);
                    break;
                case 'info':
                default:
                    console.log(logMessage);
                    break;
            }
        }
    };

    /**
     * Force Cleanup - Emergency method to reset all state
     * 
     * @param {string} reason - Reason for cleanup
     * @param {string} message - User-friendly message
     */
    PosPaymentClient.prototype._forceCleanup = function(reason, message) {
        this._log('warn', '🧹 Force cleanup - Reason: ' + reason);
        
        // Clear all timeouts
        if (this.globalTimeoutId) {
            clearTimeout(this.globalTimeoutId);
            this.globalTimeoutId = null;
        }
        
        if (this.currentPayment && this.currentPayment.transactionTimeoutId) {
            clearTimeout(this.currentPayment.transactionTimeoutId);
            this.currentPayment.transactionTimeoutId = null;
        }
        
        // Reset state
        this.isProcessing = false;
        this.transactionResponseReceived = false;
        this.serverMessage = '';
        this.currentPayment = null;
        this.retryCount = 0;
        
        // Notify UI
        if (this.onCancel) {
            this.onCancel({
                reason: reason,
                message: message || 'پرداخت لغو شد'
            });
        }
        
        this._log('info', '✅ Force cleanup completed');
    };

    /**
     * Cancel Payment - User-initiated cancellation
     * 
     * @param {string} reason - Reason for cancellation
     */
    PosPaymentClient.prototype.cancelPayment = function(reason) {
        this._log('info', '🛑 Cancelling payment - Reason: ' + reason);
        
        if (!this.isProcessing) {
            this._log('warn', '⚠️ No payment in progress to cancel');
            return;
        }
        
        // Try to notify server (best effort)
        try {
            if (this.isConnected && this.posHub && this.posHub.server && typeof this.posHub.server.cancelTransaction === 'function') {
                var self = this;
                this.posHub.server.cancelTransaction()
                    .done(function() {
                        self._log('info', '✅ Server notified of cancellation');
                    })
                    .fail(function(err) {
                        self._log('warn', '⚠️ Failed to notify server: ' + (err ? err.message || err : 'Unknown error'));
                    });
            }
        } catch (ex) {
            this._log('error', '❌ Error notifying server: ' + ex.message);
        }
        
        // Force cleanup regardless
        this._forceCleanup(reason, 'پرداخت لغو شد');
    };

    // ============================================
    // Export to Global Scope
    // ============================================
    window.PosPaymentClient = PosPaymentClient;

})(window, jQuery);

