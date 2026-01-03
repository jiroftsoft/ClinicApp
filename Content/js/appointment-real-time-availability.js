/**
 * ✅ ULTIMATE: Real-time Slot Availability Checker
 * 
 * Features:
 * - AJAX polling every 30 seconds
 * - Visual indication of taken slots
 * - Auto-refresh when slots become unavailable
 * - Optimized to reduce server load
 * 
 * طبق: APPOINTMENT_BOOKING_ROADMAP.md - Phase 4
 */

(function ($) {
    'use strict';

    var config = {
        pollingInterval: 30000, // 30 seconds
        enablePolling: true,
        apiEndpoint: '/Patient/Appointment/Book/CheckSlotAvailability',
        slotsContainerSelector: '.time-slots-container',
        slotSelector: '.time-slot-btn'
    };

    var pollingTimer = null;
    var currentDoctorId = null;
    var currentDate = null;
    var isPolling = false;

    // ============================================
    // Initialize Real-time Availability Checker
    // ============================================

    function init(doctorId, date) {
        if (!config.enablePolling) return;

        currentDoctorId = doctorId;
        currentDate = date;

        console.log('✅ Real-time Availability Checker initialized', {
            doctorId: doctorId,
            date: date,
            interval: config.pollingInterval
        });

        // Start polling
        startPolling();

        // Stop polling when user leaves page
        $(window).on('beforeunload', function () {
            stopPolling();
        });

        // Pause polling when tab is not visible (to save resources)
        document.addEventListener('visibilitychange', function () {
            if (document.hidden) {
                pausePolling();
            } else {
                resumePolling();
            }
        });
    }

    // ============================================
    // Polling Control
    // ============================================

    function startPolling() {
        if (isPolling) return;

        isPolling = true;
        pollingTimer = setInterval(function () {
            checkAvailability();
        }, config.pollingInterval);

        console.log('✅ Polling started');
    }

    function stopPolling() {
        if (!isPolling) return;

        isPolling = false;
        if (pollingTimer) {
            clearInterval(pollingTimer);
            pollingTimer = null;
        }

        console.log('⏹️ Polling stopped');
    }

    function pausePolling() {
        if (!isPolling) return;

        if (pollingTimer) {
            clearInterval(pollingTimer);
            pollingTimer = null;
        }

        console.log('⏸️ Polling paused');
    }

    function resumePolling() {
        if (!isPolling) return;

        if (!pollingTimer) {
            pollingTimer = setInterval(function () {
                checkAvailability();
            }, config.pollingInterval);
        }

        console.log('▶️ Polling resumed');
    }

    // ============================================
    // Check Availability (AJAX)
    // ============================================

    function checkAvailability() {
        if (!currentDoctorId || !currentDate) {
            console.warn('⚠️ Missing doctorId or date, skipping availability check');
            return;
        }

        $.ajax({
            url: config.apiEndpoint,
            method: 'POST',
            data: {
                doctorId: currentDoctorId,
                date: currentDate,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success && response.data) {
                    updateSlots(response.data);
                } else {
                    console.warn('⚠️ Availability check failed:', response.message);
                }
            },
            error: function (xhr) {
                console.error('❌ Availability check error:', xhr.status, xhr.statusText);
                
                // If unauthorized (session expired), stop polling and show message
                if (xhr.status === 401 || xhr.status === 403) {
                    stopPolling();
                    showSessionExpiredMessage();
                }
            }
        });
    }

    // ============================================
    // Update Slots UI
    // ============================================

    function updateSlots(availableSlots) {
        var $slotsContainer = $(config.slotsContainerSelector);
        if ($slotsContainer.length === 0) return;

        var $slotButtons = $slotsContainer.find(config.slotSelector);
        var changesDetected = false;

        $slotButtons.each(function () {
            var $btn = $(this);
            var startTime = $btn.data('start-time');
            var endTime = $btn.data('end-time');

            // Check if this slot is still available
            var isAvailable = availableSlots.some(function (slot) {
                return slot.StartTime === startTime && slot.EndTime === endTime && slot.IsAvailable;
            });

            var wasAvailable = !$btn.hasClass('unavailable');

            if (wasAvailable && !isAvailable) {
                // Slot became unavailable
                $btn.addClass('unavailable')
                    .prop('disabled', true)
                    .attr('title', 'این زمان اکنون رزرو شده است');

                // Add visual indicator
                $btn.find('.slot-status').remove();
                $btn.prepend('<span class="slot-status"><i class="fas fa-times-circle"></i> رزرو شده</span>');

                changesDetected = true;
                console.log('🚫 Slot became unavailable:', startTime, '-', endTime);

                // Show notification
                showSlotTakenNotification(startTime, endTime);
            } else if (!wasAvailable && isAvailable) {
                // Slot became available (rare, but possible if someone cancels)
                $btn.removeClass('unavailable')
                    .prop('disabled', false)
                    .attr('title', 'این زمان در دسترس است');

                $btn.find('.slot-status').remove();

                changesDetected = true;
                console.log('✅ Slot became available:', startTime, '-', endTime);
            }
        });

        if (changesDetected) {
            console.log('🔄 Slots updated at', new Date().toLocaleTimeString('fa-IR'));
        }
    }

    // ============================================
    // Notifications
    // ============================================

    function showSlotTakenNotification(startTime, endTime) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'warning',
                title: 'اسلات رزرو شد',
                text: `زمان ${startTime} - ${endTime} توسط بیمار دیگری رزرو شد. لطفاً زمان دیگری انتخاب کنید.`,
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 5000,
                timerProgressBar: true
            });
        }
    }

    function showSessionExpiredMessage() {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'error',
                title: 'جلسه منقضی شد',
                text: 'جلسه شما منقضی شده است. لطفاً دوباره وارد شوید.',
                confirmButtonText: 'ورود',
                allowOutsideClick: false
            }).then(function (result) {
                if (result.isConfirmed) {
                    window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                }
            });
        } else {
            alert('جلسه شما منقضی شده است. لطفاً دوباره وارد شوید.');
            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        }
    }

    // ============================================
    // Export Public API
    // ============================================

    window.AppointmentRealTimeAvailability = {
        init: init,
        startPolling: startPolling,
        stopPolling: stopPolling,
        pausePolling: pausePolling,
        resumePolling: resumePolling,
        checkAvailability: checkAvailability,
        config: config
    };

    console.log('✅ Real-time Availability Checker module loaded');

})(jQuery);

