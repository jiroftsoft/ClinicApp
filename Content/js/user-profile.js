/**
 * User Profile Module - Component-Based AJAX Implementation
 * Single Responsibility: مدیریت پروفایل کاربر بدون رفرش صفحه
 * 
 * Mobile-First, Healthcare Formal UI
 * طبق: DEVELOPMENT_CONTRACT.md, AI_ASSISTANT_MASTER_CONTRACT.md
 */

(function($) {
    'use strict';

    // ✅ Module Configuration
    var ProfileModule = {
        config: {
            formSelector: '#profile-form',
            submitButtonSelector: '#profile-form button[type="submit"]',
            cancelButtonSelector: '#profile-form .btn-secondary',
            validationSummarySelector: '#profile-form .alert-danger',
            apiUrl: '/Account/Profile'
        },

        // ✅ Initialize Module
        init: function() {
            this.bindEvents();
            this.initValidation();
        },

        // ✅ Bind Events
        bindEvents: function() {
            var self = this;
            
            // Form submission via AJAX
            $(document).off('submit', this.config.formSelector)
                       .on('submit', this.config.formSelector, function(e) {
                e.preventDefault();
                self.handleFormSubmit($(this));
            });

            // Cancel button
            $(document).off('click', this.config.cancelButtonSelector)
                       .on('click', this.config.cancelButtonSelector, function(e) {
                e.preventDefault();
                self.handleCancel();
            });
        },

        // ✅ Initialize Client-Side Validation
        initValidation: function() {
            var $form = $(this.config.formSelector);
            
            // ✅ Use HTML5 validation as primary (works without jQuery Validation)
            // ✅ jQuery Validation will enhance if available
            if ($.fn.validate) {
                $form.validate({
                    rules: {
                        FirstName: {
                            required: true,
                            maxlength: 100
                        },
                        LastName: {
                            required: true,
                            maxlength: 100
                        },
                        Email: {
                            required: true,
                            email: true,
                            maxlength: 256
                        },
                        Gender: {
                            required: true
                        },
                        Address: {
                            maxlength: 500
                        }
                    },
                    messages: {
                        FirstName: {
                            required: 'وارد کردن نام الزامی است.',
                            maxlength: 'نام نمی‌تواند بیش از 100 کاراکتر باشد.'
                        },
                        LastName: {
                            required: 'وارد کردن نام خانوادگی الزامی است.',
                            maxlength: 'نام خانوادگی نمی‌تواند بیش از 100 کاراکتر باشد.'
                        },
                        Email: {
                            required: 'وارد کردن ایمیل الزامی است.',
                            email: 'فرمت ایمیل معتبر نیست.',
                            maxlength: 'ایمیل نمی‌تواند بیش از 256 کاراکتر باشد.'
                        },
                        Gender: {
                            required: 'انتخاب جنسیت الزامی است.'
                        },
                        Address: {
                            maxlength: 'آدرس نمی‌تواند بیش از 500 کاراکتر باشد.'
                        }
                    },
                    errorClass: 'is-invalid',
                    validClass: 'is-valid',
                    errorElement: 'div',
                    errorPlacement: function(error, element) {
                        error.addClass('text-danger small mt-1');
                        error.insertAfter(element);
                    },
                    highlight: function(element) {
                        $(element).addClass('is-invalid').removeClass('is-valid');
                    },
                    unhighlight: function(element) {
                        $(element).removeClass('is-invalid').addClass('is-valid');
                    }
                });
            } else {
                // ✅ Fallback: Use HTML5 validation
                $form[0].addEventListener('submit', function(e) {
                    if (!$form[0].checkValidity()) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                    $form[0].classList.add('was-validated');
                });
            }
        },

        // ✅ Handle Form Submit
        handleFormSubmit: function($form) {
            var self = this;
            var $submitButton = $form.find(this.config.submitButtonSelector);
            var originalButtonText = $submitButton.html();

            // ✅ Client-side validation (jQuery Validation or HTML5)
            var isValid = true;
            if ($.fn.validate) {
                isValid = $form.valid();
            } else {
                // ✅ HTML5 validation fallback
                isValid = $form[0].checkValidity();
                if (!isValid) {
                    $form[0].classList.add('was-validated');
                }
            }

            if (!isValid) {
                this.showValidationErrors($form);
                return false;
            }

            // ✅ Show loading state
            this.setButtonLoading($submitButton, true);

            // ✅ Clear previous errors
            this.clearErrors($form);

            // ✅ Submit via AJAX
            $.ajax({
                url: this.config.apiUrl,
                method: 'POST',
                dataType: 'json',
                data: $form.serialize(),
                success: function(response) {
                    self.handleSuccess(response, $form, $submitButton, originalButtonText);
                },
                error: function(xhr, status, error) {
                    self.handleError(xhr, status, error, $submitButton, originalButtonText);
                },
                complete: function() {
                    self.setButtonLoading($submitButton, false, originalButtonText);
                }
            });

            return false;
        },

        // ✅ Handle Success Response
        handleSuccess: function(response, $form, $submitButton, originalButtonText) {
            if (response && response.success) {
                // ✅ Show success message
                if (window.toastr) {
                    toastr.success(response.message || 'پروفایل با موفقیت به‌روزرسانی شد.', '', {
                        timeOut: 3000,
                        progressBar: true
                    });
                }

                // ✅ Update form with new data (if provided)
                if (response.data) {
                    this.updateFormData($form, response.data);
                }

                // ✅ Remove validation classes
                $form.find('.is-invalid').removeClass('is-invalid');
                $form.find('.is-valid').addClass('is-valid');
            } else {
                // ✅ Handle error response
                this.handleErrorResponse(response, $form);
            }
        },

        // ✅ Handle Error Response
        handleErrorResponse: function(response, $form) {
            var errorMessage = response.message || 'خطا در به‌روزرسانی پروفایل.';

            // ✅ Show error message
            if (window.toastr) {
                toastr.error(errorMessage, '', {
                    timeOut: 5000,
                    progressBar: true
                });
            }

            // ✅ Show validation errors if any
            if (response.validationErrors && response.validationErrors.length > 0) {
                this.showFieldErrors($form, response.validationErrors);
            } else {
                // ✅ Show general error in validation summary
                var $validationSummary = $form.find(this.config.validationSummarySelector);
                if ($validationSummary.length === 0) {
                    $validationSummary = $('<div class="alert alert-danger" role="alert"></div>');
                    $form.prepend($validationSummary);
                }
                $validationSummary.html('<i class="fas fa-exclamation-circle me-2"></i>' + errorMessage).show();
            }
        },

        // ✅ Handle AJAX Error
        handleError: function(xhr, status, error, $submitButton, originalButtonText) {
            console.error('Profile Update AJAX Error:', { xhr: xhr, status: status, error: error });

            var errorMessage = 'خطا در ارتباط با سرور. لطفاً دوباره تلاش کنید.';

            if (xhr.status === 400) {
                try {
                    var errorResponse = JSON.parse(xhr.responseText);
                    errorMessage = errorResponse.message || errorMessage;
                } catch (e) {
                    console.error('Failed to parse error response:', e);
                }
            } else if (xhr.status === 401) {
                errorMessage = 'لطفاً دوباره وارد شوید.';
            } else if (xhr.status === 500) {
                errorMessage = 'خطای سرور. لطفاً با پشتیبانی تماس بگیرید.';
            }

            if (window.toastr) {
                toastr.error(errorMessage, '', {
                    timeOut: 5000,
                    progressBar: true
                });
            }
        },

        // ✅ Update Form Data (after successful update)
        updateFormData: function($form, data) {
            if (!data) return;

            // ✅ Update form fields with new data
            if (data.FirstName) $form.find('[name="FirstName"]').val(data.FirstName);
            if (data.LastName) $form.find('[name="LastName"]').val(data.LastName);
            if (data.Email) $form.find('[name="Email"]').val(data.Email);
            if (data.Gender !== undefined) $form.find('[name="Gender"]').val(data.Gender);
            if (data.Address !== undefined) $form.find('[name="Address"]').val(data.Address);

            // ✅ Trigger change event for validation
            $form.find('input, select, textarea').trigger('change');
        },

        // ✅ Show Field Errors
        showFieldErrors: function($form, errors) {
            var self = this;
            
            errors.forEach(function(error) {
                var $field = $form.find('[name="' + error.field + '"]');
                if ($field.length > 0) {
                    $field.addClass('is-invalid').removeClass('is-valid');
                    
                    // ✅ Show error message
                    var $errorMsg = $field.siblings('.text-danger');
                    if ($errorMsg.length === 0) {
                        $errorMsg = $('<div class="text-danger small mt-1"></div>');
                        $field.after($errorMsg);
                    }
                    $errorMsg.text(error.message);
                }
            });
        },

        // ✅ Show Validation Errors
        showValidationErrors: function($form) {
            var $validationSummary = $form.find(this.config.validationSummarySelector);
            if ($validationSummary.length === 0) {
                $validationSummary = $('<div class="alert alert-danger" role="alert"></div>');
                $form.prepend($validationSummary);
            }
            $validationSummary.html('<i class="fas fa-exclamation-circle me-2"></i>لطفاً تمام فیلدهای الزامی را پر کنید.').show();
        },

        // ✅ Clear Errors
        clearErrors: function($form) {
            $form.find('.is-invalid').removeClass('is-invalid');
            $form.find('.text-danger').text('');
            $form.find(this.config.validationSummarySelector).hide();
        },

        // ✅ Set Button Loading State
        setButtonLoading: function($button, isLoading, originalText) {
            if (isLoading) {
                $button.data('original-text', $button.html());
                $button.prop('disabled', true);
                $button.html('<i class="fas fa-spinner fa-spin me-2"></i>در حال ذخیره...');
            } else {
                $button.prop('disabled', false);
                if (originalText) {
                    $button.html(originalText);
                } else {
                    $button.html($button.data('original-text') || '<i class="fas fa-save me-2"></i>ذخیره تغییرات');
                }
            }
        },

        // ✅ Handle Cancel
        handleCancel: function() {
            // ✅ Reset form to original values (if needed)
            var $form = $(this.config.formSelector);
            $form[0].reset();
            this.clearErrors($form);
            
            // ✅ Optional: Show info message
            if (window.toastr) {
                toastr.info('تغییرات لغو شد.', '', { timeOut: 2000 });
            }
        }
    };

    // ✅ Initialize on document ready
    $(document).ready(function() {
        ProfileModule.init();
    });

    // ✅ Expose globally for external access
    window.ProfileModule = ProfileModule;

})(jQuery);

