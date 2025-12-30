/**
 * Login Modal Manager
 * Enterprise-grade modal management with SRP compliance
 * Handles modal initialization, backdrop, and body scroll lock
 */
(function ($) {
    'use strict';

    /**
     * LoginModalManager
     * Single Responsibility: Manage login modal state and behavior
     */
    var LoginModalManager = {
        /**
         * Initialize modal on page load
         */
        init: function () {
            this.setupModal();
            this.setupBackdrop();
            this.lockBodyScroll();
        },

        /**
         * Setup modal structure
         */
        setupModal: function () {
            var $modal = $('#loginModal');
            if ($modal.length) {
                // Ensure modal is visible
                $modal.addClass('show');
                $modal.attr('aria-hidden', 'false');
                $modal.attr('aria-modal', 'true');
                
                // Focus management for accessibility
                this.focusFirstInput();
            }
        },

        /**
         * Setup backdrop
         */
        setupBackdrop: function () {
            var $backdrop = $('.login-modal-backdrop');
            if ($backdrop.length) {
                $backdrop.addClass('show');
            }
        },

        /**
         * Lock body scroll when modal is open
         */
        lockBodyScroll: function () {
            $('body').addClass('login-modal-body');
            // Prevent scroll on body
            $('body').css({
                'overflow': 'hidden',
                'position': 'fixed',
                'width': '100%'
            });
        },

        /**
         * Focus first input for accessibility
         */
        focusFirstInput: function () {
            setTimeout(function () {
                var $firstInput = $('#NationalCode, #otp-inputs .otp-input:first');
                if ($firstInput.length) {
                    $firstInput.focus();
                }
            }, 300);
        }
    };

    /**
     * Initialize on DOM ready
     */
    $(document).ready(function () {
        LoginModalManager.init();
    });

    /**
     * Handle ESC key to close modal (if needed in future)
     */
    $(document).on('keydown', function (e) {
        // ESC key handling can be added here if modal should be closable
        // For now, login modal is not closable (user must complete login)
    });

})(jQuery);

