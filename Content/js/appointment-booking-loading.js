/**
 * ✅ ULTIMATE: Loading States Manager for Appointment Booking
 * 
 * Features:
 * - Show/hide loading overlays
 * - Skeleton screen management
 * - Button loading states
 * - AJAX request tracking
 * 
 * طبق: APPOINTMENT_BOOKING_ROADMAP.md - Phase 4
 */

(function ($) {
    'use strict';

    // ============================================
    // Loading Overlay
    // ============================================

    var $loadingOverlay = null;

    function createLoadingOverlay() {
        if ($loadingOverlay) return;

        $loadingOverlay = $(`
            <div class="loading-overlay">
                <div class="loading-spinner">
                    <div class="spinner"></div>
                    <div class="loading-text">در حال بارگذاری...</div>
                </div>
            </div>
        `);

        $('body').append($loadingOverlay);
    }

    function showLoadingOverlay(message) {
        createLoadingOverlay();
        
        if (message) {
            $loadingOverlay.find('.loading-text').text(message);
        }
        
        $loadingOverlay.addClass('active');
    }

    function hideLoadingOverlay() {
        if ($loadingOverlay) {
            $loadingOverlay.removeClass('active');
        }
    }

    // ============================================
    // Button Loading State
    // ============================================

    function setButtonLoading($button, loading, originalText) {
        if (loading) {
            // Save original text if not provided
            if (!$button.data('original-text')) {
                $button.data('original-text', $button.html());
            }

            $button.prop('disabled', true)
                .addClass('btn-loading')
                .html('<i class="fas fa-spinner fa-spin ml-2"></i> در حال پردازش...');
        } else {
            var text = originalText || $button.data('original-text') || $button.html();
            $button.prop('disabled', false)
                .removeClass('btn-loading')
                .html(text);
        }
    }

    // ============================================
    // Skeleton Screens
    // ============================================

    // Doctor List Skeleton
    function showDoctorListSkeleton(count) {
        count = count || 8; // Default: 8 skeleton cards

        var html = '<div class="doctors-skeleton-container">';
        
        for (var i = 0; i < count; i++) {
            html += `
                <div class="doctor-skeleton-card">
                    <div class="skeleton skeleton-avatar"></div>
                    <div class="skeleton skeleton-name"></div>
                    <div class="skeleton skeleton-specialty"></div>
                    <div class="skeleton skeleton-info"></div>
                    <div class="skeleton skeleton-info"></div>
                    <div class="skeleton skeleton-button"></div>
                </div>
            `;
        }

        html += '</div>';
        return html;
    }

    // Time Slots Skeleton
    function showTimeSlotsSkeletonHtml(count) {
        count = count || 12; // Default: 12 skeleton slots

        var html = '<div class="timeslots-skeleton-container">';
        
        for (var i = 0; i < count; i++) {
            html += '<div class="skeleton timeslot-skeleton"></div>';
        }

        html += '</div>';
        return html;
    }

    // Hide Skeleton and Show Content
    function hideSkeleton($container) {
        $container.find('.skeleton, .doctors-skeleton-container, .timeslots-skeleton-container').remove();
    }

    function showContent($container, html) {
        hideSkeleton($container);
        $container.html(html).addClass('fade-in');
    }

    // ============================================
    // AJAX Request Tracker (Global)
    // ============================================

    var activeRequests = 0;

    function trackAjaxStart() {
        activeRequests++;
        if (activeRequests === 1) {
            // Show global loading indicator (optional)
            // You can add a small loading bar at the top of the page
        }
    }

    function trackAjaxEnd() {
        activeRequests--;
        if (activeRequests === 0) {
            // Hide global loading indicator
        }
    }

    // Auto-track all AJAX requests
    $(document).ajaxStart(function () {
        trackAjaxStart();
    });

    $(document).ajaxStop(function () {
        trackAjaxEnd();
    });

    // ============================================
    // Export Public API
    // ============================================

    window.AppointmentBookingLoading = {
        // Loading Overlay
        showLoadingOverlay: showLoadingOverlay,
        hideLoadingOverlay: hideLoadingOverlay,

        // Button Loading
        setButtonLoading: setButtonLoading,

        // Skeleton Screens
        showDoctorListSkeleton: showDoctorListSkeleton,
        showTimeSlotsSkeletonHtml: showTimeSlotsSkeletonHtml,
        hideSkeleton: hideSkeleton,
        showContent: showContent,

        // AJAX Tracking
        trackAjaxStart: trackAjaxStart,
        trackAjaxEnd: trackAjaxEnd
    };

    console.log('✅ Appointment Booking Loading States initialized');

})(jQuery);

