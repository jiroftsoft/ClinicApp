/**
 * ============================================
 * POS Payment UI Helper Module (Production-Ready)
 * ============================================
 * 
 * ماژول مدیریت UI برای پرداخت POS
 * 
 * مسئولیت: مدیریت Modal و UI Updates
 * 
 * استفاده:
 * ```javascript
 * var posUI = new PosPaymentUI({
 *     modalId: 'posPaymentModal',
 *     onConfirm: function() { ... },
 *     onPrint: function() { ... },
 *     onRetry: function() { ... }
 * });
 * ```
 * 
 * @author ClinicApp Development Team
 * @version 1.0.0
 * @date 1404/09/11
 */

(function(window, $) {
    'use strict';

    /**
     * POS Payment UI Class
     * 
     * مسئولیت: مدیریت UI و Modal برای پرداخت POS
     */
    function PosPaymentUI(config) {
        this.config = $.extend({
            modalId: 'posPaymentModal',
            enableLogging: true
        }, config || {});

        this.onStart = config.onStart || function() {};
        this.onConfirm = config.onConfirm || function() {};
        this.onPrint = config.onPrint || function() {};
        this.onRetry = config.onRetry || function() {};
        this.onCancel = config.onCancel || function() {};

        this._initialize();
    }

    /**
     * Initialize UI
     */
    PosPaymentUI.prototype._initialize = function() {
        var self = this;
        var modalElement = document.getElementById(this.config.modalId);
        var $modal = $(modalElement);

        if (!modalElement || $modal.length === 0) {
            this._log('error', 'Modal not found: ' + this.config.modalId);
            return;
        }

        // Event Handlers
        $modal.find('#posPaymentStartBtn').on('click', function() {
            self.onStart();
        });

        $modal.find('#posPaymentConfirmBtn').on('click', function() {
            self.onConfirm();
        });

        $modal.find('#posPaymentPrintBtn').on('click', function() {
            self.onPrint();
        });

        $modal.find('#posPaymentRetryBtn').on('click', function() {
            self.onRetry();
        });

        $modal.find('#posPaymentCancelBtn').on('click', function() {
            self.onCancel();
        });

        // Reset on modal close (پشتیبانی از Bootstrap 5 و 4)
        // ✅ Bootstrap 5 event (native)
        modalElement.addEventListener('hidden.bs.modal', function() {
            self.showReady();
        });
        // ✅ Bootstrap 4 fallback (jQuery)
        $modal.on('hidden.bs.modal', function() {
            self.showReady();
        });
    };

    /**
     * Show Ready State
     */
    PosPaymentUI.prototype.showReady = function() {
        if (window.PosPaymentModal) {
            window.PosPaymentModal.showReady();
        } else {
            this._showState('ready');
        }
    };

    /**
     * Show Loading State
     */
    PosPaymentUI.prototype.showLoading = function(title, message, hint) {
        if (window.PosPaymentModal) {
            window.PosPaymentModal.showLoading(title, message, hint);
        } else {
            this._showState('loading', { title: title, message: message, hint: hint });
        }
    };

    /**
     * Show Success State
     */
    PosPaymentUI.prototype.showSuccess = function(data) {
        if (window.PosPaymentModal) {
            window.PosPaymentModal.showSuccess(data);
        } else {
            this._showState('success', data);
        }
    };

    /**
     * Show Error State
     */
    PosPaymentUI.prototype.showError = function(message, errorCode) {
        if (window.PosPaymentModal) {
            window.PosPaymentModal.showError(message, errorCode);
        } else {
            this._showState('error', { message: message, errorCode: errorCode });
        }
    };

    /**
     * Show Canceled State
     */
    PosPaymentUI.prototype.showCanceled = function() {
        if (window.PosPaymentModal) {
            window.PosPaymentModal.showCanceled();
        } else {
            this._showState('canceled');
        }
    };

    /**
     * Set Payment Info
     */
    PosPaymentUI.prototype.setPaymentInfo = function(amount, terminalName) {
        if (window.PosPaymentModal) {
            window.PosPaymentModal.setPaymentInfo(amount, terminalName);
        } else {
            var $modal = $('#' + this.config.modalId);
            $modal.find('#posAmount').text(amount ? parseFloat(amount).toLocaleString('fa-IR') + ' ریال' : 'نامشخص');
            $modal.find('#posTerminalName').text(terminalName || 'نامشخص');
        }
    };

    /**
     * Open Modal
     * ✅ پشتیبانی از Bootstrap 5 API
     */
    PosPaymentUI.prototype.open = function() {
        var modalElement = document.getElementById(this.config.modalId);
        if (modalElement) {
            // ✅ Bootstrap 5 API
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                // دریافت یا ایجاد Modal instance
                this.modalInstance = bootstrap.Modal.getInstance(modalElement);
                if (!this.modalInstance) {
                    this.modalInstance = new bootstrap.Modal(modalElement, {
                        backdrop: 'static',
                        keyboard: false
                    });
                }
                this.modalInstance.show();
                this.showReady();
            }
            // ✅ Fallback: Bootstrap 4 API (jQuery)
            else if ($ && $.fn.modal) {
                $(modalElement).modal('show');
                this.showReady();
            }
            // ✅ Fallback: Manual show
            else {
                $(modalElement).addClass('show').css('display', 'block');
                $('body').addClass('modal-open');
                this.showReady();
            }
        }
    };

    /**
     * Close Modal
     * ✅ پشتیبانی از Bootstrap 5 API
     */
    PosPaymentUI.prototype.close = function() {
        var modalElement = document.getElementById(this.config.modalId);
        if (modalElement) {
            // ✅ Bootstrap 5 API
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                // استفاده از instance ذخیره شده یا دریافت از DOM
                if (this.modalInstance) {
                    this.modalInstance.hide();
                } else {
                    var modal = bootstrap.Modal.getInstance(modalElement);
                    if (modal) {
                        modal.hide();
                    } else {
                        // اگر instance وجود ندارد، manual hide
                        $(modalElement).removeClass('show').css('display', 'none');
                        $('body').removeClass('modal-open');
                        $('.modal-backdrop').remove();
                    }
                }
            }
            // ✅ Fallback: Bootstrap 4 API (jQuery)
            else if ($ && $.fn.modal) {
                $(modalElement).modal('hide');
            }
            // ✅ Fallback: Manual hide
            else {
                $(modalElement).removeClass('show').css('display', 'none');
                $('body').removeClass('modal-open');
                $('.modal-backdrop').remove();
            }
        }
    };

    /**
     * Show State (Fallback if PosPaymentModal not available)
     */
    PosPaymentUI.prototype._showState = function(state, data) {
        var $modal = $('#' + this.config.modalId);
        if ($modal.length === 0) return;

        // Hide all states
        $modal.find('.pos-payment-state').addClass('d-none');

        // Show specific state
        switch(state) {
            case 'ready':
                $modal.find('#posPaymentReady').removeClass('d-none');
                break;
            case 'loading':
                $modal.find('#posPaymentLoading').removeClass('d-none');
                if (data) {
                    if (data.title) $modal.find('#posPaymentLoadingTitle').text(data.title);
                    if (data.message) $modal.find('#posPaymentLoadingMessage').text(data.message);
                    if (data.hint) $modal.find('#posPaymentLoadingHint').text(data.hint);
                }
                break;
            case 'success':
                $modal.find('#posPaymentSuccess').removeClass('d-none');
                if (data) {
                    if (data.rrn) $modal.find('#posRRN').text(data.rrn);
                    if (data.traceNo) $modal.find('#posTraceNo').text(data.traceNo);
                    if (data.terminalId) $modal.find('#posTerminalId').text(data.terminalId);
                    if (data.cardLast4) $modal.find('#posCardLast4').text('****' + data.cardLast4);
                    if (data.amount) $modal.find('#posSuccessAmount').text(parseFloat(data.amount).toLocaleString('fa-IR') + ' ریال');
                    if (data.txnDate) $modal.find('#posTxnDate').text(data.txnDate);
                }
                break;
            case 'error':
                $modal.find('#posPaymentError').removeClass('d-none');
                if (data) {
                    if (data.message) $modal.find('#posErrorMessage').text(data.message);
                    if (data.errorCode) $modal.find('#posErrorCode').text('کد خطا: ' + data.errorCode);
                }
                break;
            case 'canceled':
                $modal.find('#posPaymentCanceled').removeClass('d-none');
                break;
        }
    };

    /**
     * Logging Helper
     */
    PosPaymentUI.prototype._log = function(level, message) {
        if (!this.config.enableLogging) return;
        var logMessage = '[PosPaymentUI] ' + message;
        if (this.config.enableConsoleLog !== false) {
            console.log(logMessage);
        }
    };

    /**
     * On Start Handler
     */
    PosPaymentUI.prototype.onStart = function() {
        // Override in config
        if (this.config.onStart) {
            this.config.onStart();
        }
    };

    // Export to Global Scope
    window.PosPaymentUI = PosPaymentUI;

})(window, jQuery);

