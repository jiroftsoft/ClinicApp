/**
 * ============================================
 * POS Payment Lock Manager (Production-Ready)
 * ============================================
 * 
 * مسئولیت: مدیریت Lock State و جلوگیری از Stuck Payments
 * 
 * ویژگی‌ها:
 * ✅ Global Timeout
 * ✅ Auto-Cleanup
 * ✅ Force Unlock
 * ✅ Heartbeat Check
 * ✅ Recovery on Page Load
 * ✅ localStorage Persistence
 * ✅ Multi-Tab Support
 * 
 * @author ClinicApp Development Team
 * @version 1.0.0
 * @date 1404/10/05
 */

(function(window, $) {
    'use strict';
    
    // ============================================
    // Constants
    // ============================================
    var STORAGE_KEY = 'pos_payment_state';
    var MAX_PAYMENT_DURATION = 120000; // 2 minutes
    var HEARTBEAT_INTERVAL = 30000; // 30 seconds
    var STUCK_THRESHOLD = 300000; // 5 minutes
    
    /**
     * POS Payment Lock Manager Constructor
     */
    function PosPaymentLockManager() {
        this.heartbeatIntervalId = null;
        this.init();
    }
    
    /**
     * Initialize Lock Manager
     */
    PosPaymentLockManager.prototype.init = function() {
        console.log('🔧 PosPaymentLockManager: Initializing...');
        
        // Check for stuck payment on init
        this.checkForStuckPayment();
        
        // Start heartbeat
        this.startHeartbeat();
        
        // Setup event listeners
        this.setupEventListeners();
        
        console.log('✅ PosPaymentLockManager: Initialized');
    };
    
    /**
     * Check for stuck payment from previous session
     */
    PosPaymentLockManager.prototype.checkForStuckPayment = function() {
        var state = this.getState();
        if (!state) return;
        
        var now = Date.now();
        var elapsed = now - state.startTime;
        
        console.log('🔍 Checking for stuck payment:', {
            elapsed: elapsed,
            startTime: new Date(state.startTime),
            now: new Date(now),
            threshold: STUCK_THRESHOLD
        });
        
        if (elapsed > STUCK_THRESHOLD) {
            console.warn('⚠️ STUCK PAYMENT DETECTED:', {
                elapsed: elapsed,
                startTime: new Date(state.startTime),
                now: new Date(now)
            });
            
            this.forceUnlock('STUCK_PAYMENT_DETECTED');
            
            // Show notification to user
            if (window.toastr) {
                toastr.warning('پرداخت قبلی به پایان نرسیده بود و حذف شد.', 'بازیابی خودکار', {
                    timeOut: 5000,
                    closeButton: true,
                    positionClass: 'toast-top-center'
                });
            }
        }
    };
    
    /**
     * Start Heartbeat Check
     */
    PosPaymentLockManager.prototype.startHeartbeat = function() {
        var self = this;
        
        if (this.heartbeatIntervalId) {
            clearInterval(this.heartbeatIntervalId);
        }
        
        this.heartbeatIntervalId = setInterval(function() {
            self.checkHeartbeat();
        }, HEARTBEAT_INTERVAL);
        
        console.log('💓 Heartbeat started (interval: ' + HEARTBEAT_INTERVAL + 'ms)');
    };
    
    /**
     * Check Heartbeat - Auto cleanup if payment stuck
     */
    PosPaymentLockManager.prototype.checkHeartbeat = function() {
        var state = this.getState();
        if (!state) return;
        
        var now = Date.now();
        var elapsed = now - state.startTime;
        
        if (elapsed > MAX_PAYMENT_DURATION) {
            console.error('🔴 HEARTBEAT: Payment exceeded max duration', {
                elapsed: elapsed,
                maxDuration: MAX_PAYMENT_DURATION,
                startTime: new Date(state.startTime)
            });
            
            this.forceUnlock('HEARTBEAT_TIMEOUT');
            
            // Notify user
            if (window.toastr) {
                toastr.error('زمان مجاز برای پرداخت به پایان رسید. لطفاً مجدداً تلاش کنید.', 'خطا', {
                    timeOut: 7000,
                    closeButton: true,
                    positionClass: 'toast-top-center'
                });
            }
            
            // Close modal if open
            $('#posPaymentModal').modal('hide');
        }
    };
    
    /**
     * Setup Event Listeners
     */
    PosPaymentLockManager.prototype.setupEventListeners = function() {
        var self = this;
        
        // Modal close
        $(document).on('hidden.bs.modal', '#posPaymentModal', function() {
            console.log('🔔 POS Modal closed');
            
            // Give a short delay to allow normal completion
            setTimeout(function() {
                var state = self.getState();
                if (state) {
                    console.warn('⚠️ Modal closed but payment still locked - Force unlocking');
                    self.forceUnlock('MODAL_CLOSED');
                }
            }, 1000);
        });
        
        // Page unload
        $(window).on('beforeunload', function() {
            var state = self.getState();
            if (state) {
                console.log('⚠️ Page unloading with active payment');
                localStorage.setItem('pos_payment_interrupted', Date.now());
                
                // Force unlock (best effort)
                self.forceUnlock('PAGE_UNLOAD');
            }
        });
        
        // Visibility change (tab hidden)
        $(document).on('visibilitychange', function() {
            if (document.hidden) {
                var state = self.getState();
                if (state) {
                    console.log('👁️ Tab hidden with active payment');
                }
            }
        });
    };
    
    /**
     * Lock Payment
     */
    PosPaymentLockManager.prototype.lock = function() {
        console.log('🔒 Locking payment');
        var state = {
            isLocked: true,
            startTime: Date.now(),
            lockTime: Date.now(),
            userAgent: navigator.userAgent
        };
        localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
        return state;
    };
    
    /**
     * Unlock Payment
     */
    PosPaymentLockManager.prototype.unlock = function() {
        console.log('🔓 Unlocking payment');
        localStorage.removeItem(STORAGE_KEY);
    };
    
    /**
     * Force Unlock - Emergency cleanup
     */
    PosPaymentLockManager.prototype.forceUnlock = function(reason) {
        console.log('🔓 FORCE UNLOCK - Reason:', reason);
        
        // Clear localStorage
        this.unlock();
        
        // Reset client state
        if (window.posPaymentClient) {
            if (typeof window.posPaymentClient._forceCleanup === 'function') {
                window.posPaymentClient._forceCleanup(reason, 'پرداخت به دلیل ' + reason + ' لغو شد');
            } else {
                // Fallback
                console.warn('⚠️ _forceCleanup not available, using fallback');
                window.posPaymentClient.isProcessing = false;
                window.posPaymentClient.currentPayment = null;
                window.posPaymentClient.transactionResponseReceived = false;
            }
        }
    };
    
    /**
     * Get Current State
     */
    PosPaymentLockManager.prototype.getState = function() {
        var stateStr = localStorage.getItem(STORAGE_KEY);
        if (!stateStr) return null;
        
        try {
            return JSON.parse(stateStr);
        } catch (ex) {
            console.error('❌ Failed to parse payment state:', ex);
            // Clear corrupted state
            this.unlock();
            return null;
        }
    };
    
    /**
     * Check if Locked
     */
    PosPaymentLockManager.prototype.isLocked = function() {
        var state = this.getState();
        if (!state) return false;
        
        // Check if lock is too old (auto-expire after 5 minutes)
        var now = Date.now();
        var elapsed = now - state.lockTime;
        if (elapsed > STUCK_THRESHOLD) {
            console.warn('⚠️ Lock expired, auto-unlocking');
            this.forceUnlock('LOCK_EXPIRED');
            return false;
        }
        
        return state.isLocked;
    };
    
    /**
     * Destroy Lock Manager
     */
    PosPaymentLockManager.prototype.destroy = function() {
        console.log('🗑️ Destroying PosPaymentLockManager');
        
        // Clear heartbeat
        if (this.heartbeatIntervalId) {
            clearInterval(this.heartbeatIntervalId);
            this.heartbeatIntervalId = null;
        }
        
        // Unlock
        this.unlock();
    };
    
    // ============================================
    // Export to Global
    // ============================================
    window.PosPaymentLockManager = PosPaymentLockManager;
    
    console.log('✅ PosPaymentLockManager loaded');
    
})(window, jQuery);

