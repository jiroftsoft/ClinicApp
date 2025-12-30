/**
 * Login OTP Manager
 * Single Responsibility: Manage OTP input behavior and validation
 * Bulletproof implementation for all devices and scenarios
 */
(function () {
    'use strict';
    
    // ✅ Bulletproof: Wait for jQuery before executing
    function initOTPManager() {
        // Check if jQuery is available
        var $ = (typeof jQuery !== 'undefined') ? jQuery : (typeof window.jQuery !== 'undefined' ? window.jQuery : null);
        
        if (!$) {
            // jQuery not available - wait for it
            if (typeof window.whenJQ !== 'undefined') {
                window.whenJQ(function() {
                    initOTPManager();
                });
                return;
            } else {
                // Fallback: check every 50ms for max 5 seconds
                var attempts = 0;
                var maxAttempts = 100; // 5 seconds
                var checkInterval = setInterval(function() {
                    attempts++;
                    if (typeof jQuery !== 'undefined' || typeof window.jQuery !== 'undefined') {
                        clearInterval(checkInterval);
                        initOTPManager();
                    } else if (attempts >= maxAttempts) {
                        clearInterval(checkInterval);
                        console.error('❌ OTPManager: jQuery failed to load after 5 seconds');
                    }
                }, 50);
                return;
            }
        }
        
        // ✅ jQuery is available - proceed with initialization
        
        /**
         * OTPManager
         * Handles all OTP input interactions
         */
        var OTPManager = {
            config: {
                otpLength: 6,
                inputSelector: '#otp-inputs .otp-input',
                combinedFieldSelector: '#combined-otp-code',
                formSelector: '#form-verify-otp'
            },

            /**
             * Initialize OTP inputs
             */
            init: function () {
                this.setupInputHandlers();
                this.setupPasteHandler();
                this.setupNavigationHandlers();
            },

            /**
             * Setup input handlers for digit entry
             */
            setupInputHandlers: function () {
                var self = this;
                var $inputs = $(this.config.inputSelector);

                // Remove existing handlers to prevent duplicates
                $inputs.off('input keydown paste keyup focus');

                // Input handler: Only digits, auto-focus next
                $inputs.on('input', function () {
                    var $this = $(this);
                    var value = $this.val();

                    // Only allow digits - bulletproof
                    value = value.replace(/\D/g, '');
                    if (value.length > 1) {
                        value = value.charAt(0);
                    }
                    $this.val(value);

                    // Auto-focus next if digit entered
                    if (value.length === 1) {
                        var index = $inputs.index($this);
                        if (index < $inputs.length - 1) {
                            setTimeout(function () {
                                $inputs.eq(index + 1).focus();
                            }, 50);
                        } else {
                            // Last input filled
                            self.updateCombinedOtp();
                        }
                    } else if (value.length === 0) {
                        self.updateCombinedOtp();
                    }
                });

                // Focus handler: Select text on focus
                $inputs.on('focus', function () {
                    var $this = $(this);
                    if ($this.val()) {
                        $this.select();
                    }
                });

                // Keyup handler: Update combined OTP
                $inputs.on('keyup', function () {
                    self.updateCombinedOtp();
                });
            },

            /**
             * Setup paste handler for clipboard paste
             */
            setupPasteHandler: function () {
                var self = this;
                var $inputs = $(this.config.inputSelector);

                $inputs.on('paste', function (e) {
                    e.preventDefault();
                    var pastedData = (e.originalEvent.clipboardData || window.clipboardData).getData('text');
                    var digits = pastedData.replace(/\D/g, '').substring(0, self.config.otpLength);

                    if (digits.length > 0) {
                        // Clear all inputs first
                        $inputs.val('');

                        // Fill inputs with pasted digits
                        digits.split('').forEach(function (digit, index) {
                            if (index < $inputs.length) {
                                $inputs.eq(index).val(digit);
                            }
                        });

                        // Focus last filled input
                        var lastIndex = Math.min(digits.length - 1, $inputs.length - 1);
                        $inputs.eq(lastIndex).focus();

                        // Update combined OTP
                        self.updateCombinedOtp();
                    }
                });
            },

            /**
             * Setup navigation handlers (arrow keys, backspace, delete)
             */
            setupNavigationHandlers: function () {
                var self = this;
                var $inputs = $(this.config.inputSelector);

                $inputs.on('keydown', function (e) {
                    var $this = $(this);
                    var index = $inputs.index($this);
                    var value = $this.val();

                    // Arrow keys navigation
                    if (e.key === 'ArrowLeft' && index > 0) {
                        e.preventDefault();
                        $inputs.eq(index - 1).focus();
                        return;
                    }
                    if (e.key === 'ArrowRight' && index < $inputs.length - 1) {
                        e.preventDefault();
                        $inputs.eq(index + 1).focus();
                        return;
                    }

                    // Backspace: clear current and go to previous
                    if (e.key === 'Backspace') {
                        if (value === '' && index > 0) {
                            e.preventDefault();
                            $inputs.eq(index - 1).val('').focus();
                            self.updateCombinedOtp();
                        } else if (value !== '') {
                            $this.val('');
                            self.updateCombinedOtp();
                        }
                        return;
                    }

                    // Delete key: clear current
                    if (e.key === 'Delete') {
                        $this.val('');
                        self.updateCombinedOtp();
                        return;
                    }

                    // Only allow digits (0-9) and navigation keys
                    if (!/^\d$/.test(e.key) &&
                        !['Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'Tab', 'Enter'].includes(e.key)) {
                        e.preventDefault();
                        return;
                    }
                });
            },

            /**
             * Update combined OTP field
             */
            updateCombinedOtp: function () {
                var otp = $(this.config.inputSelector).map(function () {
                    return $(this).val();
                }).get().join('').replace(/\D/g, '');

                $(this.config.combinedFieldSelector).val(otp);

                // Auto-submit when complete
                if (otp.length === this.config.otpLength) {
                    var self = this;
                    setTimeout(function () {
                        $(self.config.formSelector).submit();
                    }, 100);
                }
            },

            /**
             * Clear all OTP inputs
             */
            clear: function () {
                $(this.config.inputSelector).val('');
                $(this.config.combinedFieldSelector).val('');
            },

            /**
             * Focus first input
             */
            focusFirst: function () {
                var $firstInput = $(this.config.inputSelector).first();
                if ($firstInput.length) {
                    setTimeout(function () {
                        $firstInput.focus();
                    }, 100);
                }
            }
        };

        // Export for global access
        window.OTPManager = OTPManager;

        // Auto-initialize when DOM is ready
        $(document).ready(function () {
            // Initialize when OTP inputs are available
            if ($(OTPManager.config.inputSelector).length > 0) {
                OTPManager.init();
            }
        });

        // Re-initialize on dynamic content load
        $(document).on('loginModalContentLoaded', function () {
            if ($(OTPManager.config.inputSelector).length > 0) {
                OTPManager.init();
            }
        });
    }
    
    // ✅ Start initialization - Wait for jQuery if needed
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initOTPManager);
    } else {
        // DOM already loaded
        initOTPManager();
    }

})();
