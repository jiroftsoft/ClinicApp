/**
 * ✅ ULTIMATE: Client-side Validation for Appointment Booking
 * 
 * Features:
 * - jQuery Validation integration
 * - Real-time validation feedback
 * - RTL support
 * - Custom validation rules
 * - Accessible error messages
 * 
 * طبق: APPOINTMENT_BOOKING_ROADMAP.md - Phase 4
 */

(function ($) {
    'use strict';

    // ============================================
    // Custom Validation Methods
    // ============================================

    // تاریخ نباید در گذشته باشد
    $.validator.addMethod('futureDate', function (value, element) {
        if (!value) return true; // اگر خالی است، با required check می‌شود

        var selectedDate = new Date(value);
        var today = new Date();
        today.setHours(0, 0, 0, 0); // Reset time to midnight

        return selectedDate >= today;
    }, 'تاریخ انتخاب شده نمی‌تواند در گذشته باشد');

    // تاریخ نباید بیش از 90 روز در آینده باشد
    $.validator.addMethod('maxFutureDate', function (value, element) {
        if (!value) return true;

        var selectedDate = new Date(value);
        var maxDate = new Date();
        maxDate.setDate(maxDate.getDate() + 90);

        return selectedDate <= maxDate;
    }, 'نمی‌توانید برای بیش از 90 روز آینده نوبت رزرو کنید');

    // زمان شروع باید قبل از زمان پایان باشد
    $.validator.addMethod('timeBeforeEnd', function (value, element, params) {
        if (!value) return true;

        var endTimeSelector = params;
        var endTimeValue = $(endTimeSelector).val();

        if (!endTimeValue) return true;

        var startTime = parseTimeSpan(value);
        var endTime = parseTimeSpan(endTimeValue);

        return startTime < endTime;
    }, 'زمان شروع باید قبل از زمان پایان باشد');

    // حداکثر طول توضیحات: 500 کاراکتر
    $.validator.addMethod('maxLength500', function (value, element) {
        if (!value) return true;
        return value.length <= 500;
    }, 'توضیحات نباید بیش از 500 کاراکتر باشد');

    // Helper function: Parse TimeSpan string (HH:mm:ss or HH:mm)
    function parseTimeSpan(timeString) {
        if (!timeString) return 0;

        var parts = timeString.split(':');
        var hours = parseInt(parts[0]) || 0;
        var minutes = parseInt(parts[1]) || 0;
        var seconds = parseInt(parts[2]) || 0;

        return hours * 3600 + minutes * 60 + seconds;
    }

    // ============================================
    // Validation Configuration
    // ============================================

    var validationDefaults = {
        errorClass: 'is-invalid',
        validClass: 'is-valid',
        errorElement: 'div',
        errorPlacement: function (error, element) {
            error.addClass('invalid-feedback');
            error.insertAfter(element);
        },
        highlight: function (element, errorClass, validClass) {
            $(element).addClass(errorClass).removeClass(validClass);
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function (element, errorClass, validClass) {
            $(element).removeClass(errorClass).addClass(validClass);
            $(element).closest('.form-group').removeClass('has-error');
        },
        // RTL support
        onfocusout: function (element) {
            $(element).valid();
        },
        onkeyup: function (element) {
            if ($(element).hasClass('is-invalid')) {
                $(element).valid();
            }
        }
    };

    // ============================================
    // SelectDoctor Form Validation
    // ============================================

    function initSelectDoctorValidation() {
        var $form = $('#selectDoctorForm');
        if ($form.length === 0) return;

        $form.validate($.extend({}, validationDefaults, {
            rules: {
                departmentId: {
                    required: false,
                    min: 1
                },
                searchTerm: {
                    minlength: 2,
                    maxlength: 100
                }
            },
            messages: {
                departmentId: {
                    min: 'لطفاً یک دپارتمان انتخاب کنید'
                },
                searchTerm: {
                    minlength: 'حداقل 2 کاراکتر وارد کنید',
                    maxlength: 'حداکثر 100 کاراکتر مجاز است'
                }
            }
        }));
    }

    // ============================================
    // SelectDate Form Validation
    // ============================================

    function initSelectDateValidation() {
        var $form = $('#selectDateForm');
        if ($form.length === 0) return;

        $form.validate($.extend({}, validationDefaults, {
            rules: {
                selectedDate: {
                    required: true,
                    futureDate: true,
                    maxFutureDate: true
                }
            },
            messages: {
                selectedDate: {
                    required: 'لطفاً تاریخ را انتخاب کنید'
                }
            }
        }));
    }

    // ============================================
    // SelectTime Form Validation
    // ============================================

    function initSelectTimeValidation() {
        var $form = $('#selectTimeForm');
        if ($form.length === 0) return;

        $form.validate($.extend({}, validationDefaults, {
            rules: {
                startTime: {
                    required: true,
                    timeBeforeEnd: '#endTime'
                },
                endTime: {
                    required: true
                }
            },
            messages: {
                startTime: {
                    required: 'لطفاً زمان شروع را انتخاب کنید'
                },
                endTime: {
                    required: 'لطفاً زمان پایان را انتخاب کنید'
                }
            }
        }));
    }

    // ============================================
    // ConfirmBooking Form Validation
    // ============================================

    function initConfirmBookingValidation() {
        var $form = $('#confirmBookingForm');
        if ($form.length === 0) return;

        $form.validate($.extend({}, validationDefaults, {
            rules: {
                doctorId: {
                    required: true,
                    min: 1
                },
                appointmentDate: {
                    required: true,
                    futureDate: true,
                    maxFutureDate: true
                },
                startTime: {
                    required: true,
                    timeBeforeEnd: '#endTime'
                },
                endTime: {
                    required: true
                },
                description: {
                    maxLength500: true
                },
                serviceCategoryId: {
                    min: 1
                }
            },
            messages: {
                doctorId: {
                    required: 'شناسه پزشک الزامی است',
                    min: 'شناسه پزشک نامعتبر است'
                },
                appointmentDate: {
                    required: 'تاریخ نوبت الزامی است'
                },
                startTime: {
                    required: 'زمان شروع الزامی است'
                },
                endTime: {
                    required: 'زمان پایان الزامی است'
                },
                serviceCategoryId: {
                    min: 'لطفاً نوع خدمت را انتخاب کنید'
                }
            },
            submitHandler: function (form) {
                // اضافه کردن loading state به دکمه Submit
                var $submitBtn = $(form).find('button[type="submit"]');
                var originalText = $submitBtn.html();

                $submitBtn.prop('disabled', true)
                    .html('<i class="fas fa-spinner fa-spin ml-2"></i> در حال پردازش...');

                // Submit form via AJAX
                $.ajax({
                    url: $(form).attr('action'),
                    method: 'POST',
                    data: $(form).serialize(),
                    success: function (response) {
                        if (response.success) {
                            // Redirect to payment or success page
                            if (response.paymentUrl) {
                                window.location.href = response.paymentUrl;
                            } else if (response.redirectUrl) {
                                window.location.href = response.redirectUrl;
                            } else {
                                showNotification('success', response.message || 'نوبت با موفقیت رزرو شد');
                            }
                        } else {
                            showNotification('error', response.message || 'خطا در رزرو نوبت');
                            $submitBtn.prop('disabled', false).html(originalText);
                        }
                    },
                    error: function (xhr) {
                        var errorMsg = 'خطا در ارتباط با سرور';
                        if (xhr.responseJSON && xhr.responseJSON.message) {
                            errorMsg = xhr.responseJSON.message;
                        }
                        showNotification('error', errorMsg);
                        $submitBtn.prop('disabled', false).html(originalText);
                    }
                });

                return false; // Prevent normal form submission
            }
        }));
    }

    // ============================================
    // Helper: Show Notification (uses SweetAlert2 if available)
    // ============================================

    function showNotification(type, message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: type === 'success' ? 'success' : 'error',
                title: type === 'success' ? 'موفق' : 'خطا',
                text: message,
                confirmButtonText: 'بستن',
                timer: type === 'success' ? 3000 : null,
                timerProgressBar: true
            });
        } else {
            alert(message);
        }
    }

    // ============================================
    // Auto-initialization
    // ============================================

    $(document).ready(function () {
        // تنظیمات پیش‌فرض jQuery Validation برای RTL
        $.validator.setDefaults({
            errorClass: 'is-invalid',
            validClass: 'is-valid'
        });

        // Initialize validation for all forms
        initSelectDoctorValidation();
        initSelectDateValidation();
        initSelectTimeValidation();
        initConfirmBookingValidation();

        console.log('✅ Appointment Booking Validation initialized');
    });

    // Export for manual usage
    window.AppointmentBookingValidation = {
        initSelectDoctorValidation: initSelectDoctorValidation,
        initSelectDateValidation: initSelectDateValidation,
        initSelectTimeValidation: initSelectTimeValidation,
        initConfirmBookingValidation: initConfirmBookingValidation
    };

})(jQuery);

