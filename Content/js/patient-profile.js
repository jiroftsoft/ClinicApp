/**
 * 🏥 Patient Profile JavaScript Module
 * 
 * طبق قراردادها:
 * - AJAX-First Development
 * - Bulletproof Validation
 * - ServiceResult Pattern
 * - Error Handling & Logging
 * 
 * @version 1.0.0
 */

(function($) {
    'use strict';

    const PatientProfile = {
        // ✅ Configuration
        config: {
            apiUrl: '/Patient/Api/Profile',
            formId: '#profileForm',
            loadingClass: 'btn-loading',
            textClass: 'btn-text'
        },

        // ✅ Initialize
        init: function() {
            this.loadProfileData();
            this.bindEvents();
            this.initDatepicker();
        },

        // ✅ Load Profile Data via AJAX
        loadProfileData: function() {
            const self = this;
            
            console.log('📡 [PatientProfile] Loading profile data from:', self.config.apiUrl + '/GetProfile');
            
            $.ajax({
                url: self.config.apiUrl + '/GetProfile',
                method: 'GET',
                dataType: 'json',
                beforeSend: function() {
                    // Show loading state
                    $(self.config.formId + ' input, ' + self.config.formId + ' textarea').prop('disabled', true);
                    console.log('⏳ [PatientProfile] Request sent, waiting for response...');
                }
            })
            .done(function(response) {
                console.log('✅ [PatientProfile] Response received:', response);
                
                if (response && response.success && response.data) {
                    console.log('✅ [PatientProfile] Profile data:', response.data);
                    self.populateForm(response.data);
                    self.updateDisplay(response.data);
                } else {
                    const errorMsg = response?.message || 'خطا در بارگذاری اطلاعات پروفایل';
                    console.error('❌ [PatientProfile] Response error:', errorMsg, response);
                    PatientProfile.showError(errorMsg);
                }
            })
            .fail(function(xhr, status, error) {
                console.error('❌ [PatientProfile] AJAX failed:', {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    error: error,
                    responseText: xhr.responseText
                });
                
                let errorMessage = 'خطا در ارتباط با سرور';
                if (xhr.status === 401) {
                    errorMessage = 'لطفاً مجدداً وارد شوید';
                } else if (xhr.status === 403) {
                    errorMessage = 'شما دسترسی لازم را ندارید';
                } else if (xhr.status === 404) {
                    errorMessage = 'API endpoint یافت نشد';
                } else if (xhr.status === 500) {
                    errorMessage = 'خطای سرور';
                }
                
                PatientProfile.showError(errorMessage);
            })
            .always(function() {
                // Remove loading state
                $(self.config.formId + ' input, ' + self.config.formId + ' textarea').prop('disabled', false);
                console.log('✅ [PatientProfile] Request completed');
            });
        },

        // ✅ Populate Form Fields
        populateForm: function(profile) {
            console.log('📝 [PatientProfile] Populating form with data:', profile);
            
            $('#FirstName').val(profile.FirstName || '');
            $('#LastName').val(profile.LastName || '');
            $('#NationalCode').val(profile.NationalCode || '');
            $('#PhoneNumber').val(profile.PhoneNumber || '');
            $('#Email').val(profile.Email || '');
            $('#BirthDate').val(profile.BirthDate || '');
            $('#Address').val(profile.Address || '');
            
            // Set Gender - handle both string and enum values
            if (profile.Gender) {
                const genderValue = typeof profile.Gender === 'string' 
                    ? profile.Gender 
                    : profile.Gender.toString();
                
                console.log('👤 [PatientProfile] Setting gender:', genderValue);
                
                // Try to find and check the radio button
                const $genderRadio = $('input[name="Gender"][value="' + genderValue + '"]');
                if ($genderRadio.length > 0) {
                    $genderRadio.prop('checked', true);
                    console.log('✅ [PatientProfile] Gender radio button checked');
                } else {
                    console.warn('⚠️ [PatientProfile] Gender radio button not found for value:', genderValue);
                    // Try alternative values
                    if (genderValue === 'Male' || genderValue === '1') {
                        $('input[name="Gender"][value="Male"]').prop('checked', true);
                    } else if (genderValue === 'Female' || genderValue === '2') {
                        $('input[name="Gender"][value="Female"]').prop('checked', true);
                    }
                }
            } else {
                console.warn('⚠️ [PatientProfile] No gender value in profile data');
            }
            
            console.log('✅ [PatientProfile] Form populated successfully');
        },

        // ✅ Update Display (Sidebar)
        updateDisplay: function(profile) {
            const fullName = (profile.FirstName || '') + ' ' + (profile.LastName || '');
            $('#displayFullName').text(fullName.trim() || 'نام کاربر');
            $('#displayNationalCode').text('کد ملی: ' + (profile.NationalCode || '-'));
        },

        // ✅ Bind Events
        bindEvents: function() {
            const self = this;
            
            // Form Submit
            $(this.config.formId).on('submit', function(e) {
                e.preventDefault();
                self.handleSubmit();
            });

            // Real-time Validation
            $(this.config.formId + ' input, ' + this.config.formId + ' textarea').on('blur', function() {
                self.validateField($(this));
            });
        },

        // ✅ Handle Form Submit
        handleSubmit: function() {
            const self = this;
            const $form = $(this.config.formId);
            const $submitBtn = $('#btnSaveProfile');
            
            // Client-side Validation
            if (!this.validateForm()) {
                return false;
            }

            // ✅ Disable submit button (استفاده از self برای consistency)
            $submitBtn.prop('disabled', true);
            $submitBtn.find('.' + self.config.textClass).addClass('d-none');
            $submitBtn.find('.' + self.config.loadingClass).removeClass('d-none');

            // Collect form data
            const formData = {
                firstName: $('#FirstName').val().trim(),
                lastName: $('#LastName').val().trim(),
                phoneNumber: $('#PhoneNumber').val().trim(),
                email: $('#Email').val().trim(),
                birthDate: $('#BirthDate').val().trim(),
                gender: $('input[name="Gender"]:checked').val(),
                address: $('#Address').val().trim()
            };

            // Get AntiForgeryToken
            const token = $form.find('input[name="__RequestVerificationToken"]').val();

            // Submit via AJAX
            $.ajax({
                url: self.config.apiUrl + '/UpdateProfile',
                method: 'POST',
                dataType: 'json',
                data: {
                    ...formData,
                    __RequestVerificationToken: token
                },
                headers: {
                    'RequestVerificationToken': token
                }
            })
            .done(function(response) {
                if (response.success) {
                    PatientProfile.showSuccess(response.message || 'پروفایل با موفقیت به‌روزرسانی شد');
                    // Update display
                    self.updateDisplay(formData);
                } else {
                    PatientProfile.showError(response.message || 'خطا در به‌روزرسانی پروفایل');
                }
            })
            .fail(function(xhr, status, error) {
                console.error('❌ [PatientProfile] Update failed:', error);
                
                let errorMessage = 'خطا در ارتباط با سرور';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                } else if (xhr.status === 400) {
                    errorMessage = 'اطلاعات وارد شده معتبر نیست';
                } else if (xhr.status === 401) {
                    errorMessage = 'لطفاً مجدداً وارد شوید';
                } else if (xhr.status === 403) {
                    errorMessage = 'شما دسترسی لازم را ندارید';
                }
                
                PatientProfile.showError(errorMessage);
            })
            .always(function() {
                // ✅ Re-enable submit button (استفاده از self به جای this)
                $submitBtn.prop('disabled', false);
                $submitBtn.find('.' + self.config.textClass).removeClass('d-none');
                $submitBtn.find('.' + self.config.loadingClass).addClass('d-none');
            });
        },

        // ✅ Validate Form
        validateForm: function() {
            let isValid = true;
            const $form = $(this.config.formId);

            // Clear previous errors
            $form.find('.field-validation-error').text('').hide();

            // Validate First Name
            if (!$('#FirstName').val().trim()) {
                this.showFieldError('FirstName', 'نام الزامی است');
                isValid = false;
            }

            // Validate Last Name
            if (!$('#LastName').val().trim()) {
                this.showFieldError('LastName', 'نام خانوادگی الزامی است');
                isValid = false;
            }

            // Validate Phone Number
            const phoneNumber = $('#PhoneNumber').val().trim();
            if (!phoneNumber) {
                this.showFieldError('PhoneNumber', 'شماره تماس الزامی است');
                isValid = false;
            } else if (!/^\+98\d{10}$/.test(phoneNumber)) {
                this.showFieldError('PhoneNumber', 'فرمت شماره تماس صحیح نیست (مثال: +989123456789)');
                isValid = false;
            }

            // Validate Email (if provided)
            const email = $('#Email').val().trim();
            if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
                this.showFieldError('Email', 'فرمت ایمیل صحیح نیست');
                isValid = false;
            }

            // Validate Gender
            if (!$('input[name="Gender"]:checked').val()) {
                this.showFieldError('Gender', 'انتخاب جنسیت الزامی است');
                isValid = false;
            }

            return isValid;
        },

        // ✅ Validate Single Field
        validateField: function($field) {
            const fieldName = $field.attr('name');
            const fieldValue = $field.val().trim();

            // Clear previous error
            this.clearFieldError(fieldName);

            // Validate based on field name
            switch(fieldName) {
                case 'FirstName':
                case 'LastName':
                    if (!fieldValue) {
                        this.showFieldError(fieldName, 'این فیلد الزامی است');
                        return false;
                    }
                    break;
                case 'PhoneNumber':
                    if (!fieldValue) {
                        this.showFieldError(fieldName, 'شماره تماس الزامی است');
                        return false;
                    } else if (!/^\+98\d{10}$/.test(fieldValue)) {
                        this.showFieldError(fieldName, 'فرمت صحیح نیست (مثال: +989123456789)');
                        return false;
                    }
                    break;
                case 'Email':
                    if (fieldValue && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(fieldValue)) {
                        this.showFieldError(fieldName, 'فرمت ایمیل صحیح نیست');
                        return false;
                    }
                    break;
            }

            return true;
        },

        // ✅ Show Field Error
        showFieldError: function(fieldName, message) {
            const $errorSpan = $('[data-valmsg-for="' + fieldName + '"]');
            if ($errorSpan.length) {
                $errorSpan.text(message).show();
            } else {
                // Create error span if doesn't exist
                const $field = $('[name="' + fieldName + '"]');
                if ($field.length) {
                    $field.after('<span class="text-danger field-validation-error" data-valmsg-for="' + fieldName + '">' + message + '</span>');
                }
            }
            
            // Add error class to field
            $('[name="' + fieldName + '"]').addClass('is-invalid');
        },

        // ✅ Clear Field Error
        clearFieldError: function(fieldName) {
            $('[data-valmsg-for="' + fieldName + '"]').text('').hide();
            $('[name="' + fieldName + '"]').removeClass('is-invalid');
        },

        // ✅ Initialize Datepicker (طبق قراردادها - استفاده از PersianDatePickerComponent)
        initDatepicker: function() {
            console.log('📅 [PatientProfile] Initializing datepicker...');
            
            // ✅ BEST PRACTICE: استفاده از PersianDatePickerComponent
            if (typeof PersianDatePickerComponent !== 'undefined') {
                const $birthDateInput = $('#BirthDate');
                if ($birthDateInput.length > 0) {
                    console.log('✅ [PatientProfile] Using PersianDatePickerComponent');
                    // Component will auto-initialize inputs with data-persian-datepicker="true"
                    PersianDatePickerComponent.initializeAll();
                } else {
                    console.warn('⚠️ [PatientProfile] BirthDate input not found');
                }
            } else if (typeof $.fn.persianDatepicker !== 'undefined') {
                // Fallback: استفاده مستقیم از persianDatepicker
                console.log('✅ [PatientProfile] Using persianDatepicker directly');
                $('#BirthDate').each(function() {
                    const $input = $(this);
                    if (!$input.data('persian-datepicker-initialized')) {
                        $input.persianDatepicker({
                            format: 'YYYY/MM/DD',
                            initialValue: false,
                            autoClose: true,
                            observer: true,
                            calendar: {
                                persian: {
                                    locale: 'fa',
                                    leapYearMode: 'astronomical'
                                }
                            },
                            toolbox: {
                                todayBtn: { enabled: true, text: { fa: 'امروز' } },
                                clearBtn: { enabled: true, text: { fa: 'پاک کردن' } }
                            },
                            onSelect: function(unix) {
                                // Fix for Unix timestamp unit (seconds vs milliseconds)
                                const ts = unix < 2e10 ? unix * 1000 : unix;
                                const date = new Date(ts);
                                const persianDate = date.getFullYear() + '/' + 
                                    String(date.getMonth() + 1).padStart(2, '0') + '/' + 
                                    String(date.getDate()).padStart(2, '0');
                                $input.val(persianDate);
                                $input.trigger('change');
                            }
                        });
                        $input.data('persian-datepicker-initialized', true);
                        console.log('✅ [PatientProfile] Datepicker initialized');
                    }
                });
            } else {
                console.error('❌ [PatientProfile] Persian DatePicker not available');
                this.showError('خطا در بارگذاری تقویم شمسی');
            }
        },

        // ✅ Show Success Message (طبق notification-helper.js: success method)
        showSuccess: function(message) {
            if (window.NotificationHelper) {
                NotificationHelper.success(message);
            } else if (window.Notify) {
                Notify.success(message);
            } else if (window.toastr) {
                toastr.success(message);
            } else {
                alert(message);
            }
        },

        // ✅ Show Error Message (طبق notification-helper.js: error method)
        showError: function(message) {
            if (window.NotificationHelper) {
                NotificationHelper.error(message);
            } else if (window.Notify) {
                Notify.error(message);
            } else if (window.toastr) {
                toastr.error(message);
            } else {
                alert(message);
            }
        }
    };

    // ✅ Auto-initialize on DOM ready (for direct page load)
    $(document).ready(function() {
        if ($('#profileForm').length > 0) {
            console.log('✅ [PatientProfile] Auto-initializing on DOM ready...');
            PatientProfile.init();
        }
    });
    
    // ✅ Also initialize when profile tab is loaded via AJAX
    // This will be called by UnifiedDashboard.initializeTabContent('profile')
    // But we also listen for custom event as fallback
    $(document).on('profileTabLoaded', function() {
        if ($('#profileForm').length > 0 && typeof PatientProfile !== 'undefined') {
            console.log('✅ [PatientProfile] Initializing via profileTabLoaded event...');
            PatientProfile.init();
        }
    });

    // ✅ Expose to global scope
    window.PatientProfile = PatientProfile;

})(jQuery);

