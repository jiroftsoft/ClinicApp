/**
 * User Profile Component - Enterprise-Grade Reusable Component
 * 
 * ✅ Enterprise-Grade Features:
 * - Fully Modular & Configurable
 * - Multiple Instance Support
 * - Component Lifecycle Management
 * - Event-Driven Architecture
 * - Error Recovery & Retry Logic
 * - Performance Optimized
 * 
 * ✅ Usage Examples:
 * 
 * 1. Auto-initialize on page load:
 *    <div data-profile-component="true"></div>
 * 
 * 2. Manual initialization:
 *    UserProfileComponent.init('#my-profile-container', {
 *        apiUrl: '/Account/Profile',
 *        formId: 'my-profile-form'
 *    });
 * 
 * 3. Load via AJAX:
 *    UserProfileComponent.load('#container', {
 *        containerClass: 'col-12',
 *        showHeader: false
 *    });
 * 
 * Single Responsibility: مدیریت کامپوننت پروفایل کاربر به صورت reusable
 * طبق: DEVELOPMENT_CONTRACT.md, AI_ASSISTANT_MASTER_CONTRACT.md
 */

(function($, window) {
    'use strict';

    // ✅ Component Registry (برای مدیریت چند instance)
    var componentRegistry = {};

    // ✅ Default Configuration
    var defaultConfig = {
        formSelector: '#profile-form',
        submitButtonSelector: 'button[type="submit"]',
        cancelButtonSelector: '.btn-secondary',
        validationSummarySelector: '.alert-danger',
        apiUrl: '/Account/Profile',
        containerSelector: null,
        autoInit: true,
        enableValidation: true,
        enableToastr: true,
        retryAttempts: 3,
        retryDelay: 1000
    };

    // ✅ UserProfileComponent - Enterprise-Grade Module
    var UserProfileComponent = {
        
        /**
         * ✅ Initialize Component
         * @param {string|jQuery} container - Container selector or jQuery object
         * @param {object} options - Configuration options
         * @returns {object} Component instance
         */
        init: function(container, options) {
            var self = this;
            var $container = $(container);
            
            if ($container.length === 0) {
                console.error('UserProfileComponent: Container not found', container);
                return null;
            }

            // ✅ Merge configuration
            var config = $.extend({}, defaultConfig, options || {});
            config.containerSelector = $container.selector || container;

            // ✅ Find form within container
            var $form = $container.find(config.formSelector);
            if ($form.length === 0) {
                console.warn('UserProfileComponent: Form not found in container', config.formSelector);
                return null;
            }

            // ✅ Override API URL from form data attribute if available
            var formApiUrl = $form.data('api-url');
            if (formApiUrl) {
                config.apiUrl = formApiUrl;
            }

            // ✅ Create component instance
            var instanceId = this.generateInstanceId();
            var instance = {
                id: instanceId,
                container: $container,
                form: $form,
                config: config,
                initialized: false,
                validation: null
            };

            // ✅ Initialize instance
            this.initializeInstance(instance);

            // ✅ Register instance
            componentRegistry[instanceId] = instance;

            // ✅ Store instance ID on container
            $container.data('profile-component-id', instanceId);

            return instance;
        },

        /**
         * ✅ Initialize Component Instance
         * @param {object} instance - Component instance
         */
        initializeInstance: function(instance) {
            var self = this;
            var $form = instance.form;
            var config = instance.config;

            // ✅ Bind events
            this.bindEvents(instance);

            // ✅ Initialize validation
            if (config.enableValidation) {
                this.initValidation(instance);
            }

            instance.initialized = true;

            // ✅ Trigger custom event
            $form.trigger('profileComponent:initialized', [instance]);
        },

        /**
         * ✅ Bind Events
         * @param {object} instance - Component instance
         */
        bindEvents: function(instance) {
            var self = this;
            var $form = instance.form;
            var config = instance.config;

            // ✅ Form submission
            $form.off('submit.profileComponent')
                 .on('submit.profileComponent', function(e) {
                e.preventDefault();
                self.handleFormSubmit(instance);
            });

            // ✅ Cancel button
            var $cancelButton = $form.find(config.cancelButtonSelector);
            if ($cancelButton.length > 0) {
                $cancelButton.off('click.profileComponent')
                             .on('click.profileComponent', function(e) {
                    e.preventDefault();
                    self.handleCancel(instance);
                });
            }

            // ✅ Field change events (for real-time validation)
            $form.find('input, select, textarea')
                 .off('change.profileComponent blur.profileComponent')
                 .on('change.profileComponent blur.profileComponent', function() {
                self.clearFieldError($(this));
            });
        },

        /**
         * ✅ Initialize Validation
         * @param {object} instance - Component instance
         */
        initValidation: function(instance) {
            var $form = instance.form;
            var config = instance.config;

            // ✅ jQuery Validation
            if ($.fn.validate) {
                instance.validation = $form.validate({
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
                // ✅ HTML5 validation fallback
                $form[0].addEventListener('submit', function(e) {
                    if (!$form[0].checkValidity()) {
                        e.preventDefault();
                        e.stopPropagation();
                    }
                    $form[0].classList.add('was-validated');
                });
            }
        },

        /**
         * ✅ Handle Form Submit
         * @param {object} instance - Component instance
         */
        handleFormSubmit: function(instance) {
            var self = this;
            var $form = instance.form;
            var config = instance.config;
            var $submitButton = $form.find(config.submitButtonSelector);
            var originalButtonText = $submitButton.html();

            // ✅ Validation
            var isValid = true;
            if (instance.validation) {
                isValid = $form.valid();
            } else {
                isValid = $form[0].checkValidity();
                if (!isValid) {
                    $form[0].classList.add('was-validated');
                }
            }

            if (!isValid) {
                this.showValidationErrors(instance);
                $form.trigger('profileComponent:validationFailed', [instance]);
                return false;
            }

            // ✅ Show loading
            this.setButtonLoading($submitButton, true);

            // ✅ Clear errors
            this.clearErrors(instance);

            // ✅ Submit with retry logic
            this.submitWithRetry(instance, $submitButton, originalButtonText, 0);

            return false;
        },

        /**
         * ✅ Submit with Retry Logic (Enterprise-Grade)
         * @param {object} instance - Component instance
         * @param {jQuery} $submitButton - Submit button
         * @param {string} originalButtonText - Original button text
         * @param {number} attempt - Current attempt number
         */
        submitWithRetry: function(instance, $submitButton, originalButtonText, attempt) {
            var self = this;
            var $form = instance.form;
            var config = instance.config;

            $.ajax({
                url: config.apiUrl,
                method: 'POST',
                dataType: 'json',
                data: $form.serialize(),
                timeout: 30000, // ✅ 30 seconds timeout
                success: function(response) {
                    self.handleSuccess(response, instance, $submitButton, originalButtonText);
                },
                error: function(xhr, status, error) {
                    // ✅ Retry logic for network errors
                    if (attempt < config.retryAttempts && (status === 'timeout' || status === 'error')) {
                        setTimeout(function() {
                            self.submitWithRetry(instance, $submitButton, originalButtonText, attempt + 1);
                        }, config.retryDelay * (attempt + 1));
                    } else {
                        self.handleError(xhr, status, error, instance, $submitButton, originalButtonText);
                    }
                },
                complete: function() {
                    self.setButtonLoading($submitButton, false, originalButtonText);
                }
            });
        },

        /**
         * ✅ Handle Success Response
         * @param {object} response - Server response
         * @param {object} instance - Component instance
         * @param {jQuery} $submitButton - Submit button
         * @param {string} originalButtonText - Original button text
         */
        handleSuccess: function(response, instance, $submitButton, originalButtonText) {
            var $form = instance.form;
            var config = instance.config;

            if (response && response.success) {
                // ✅ Show success message
                if (config.enableToastr && window.toastr) {
                    toastr.success(response.message || 'پروفایل با موفقیت به‌روزرسانی شد.', '', {
                        timeOut: 3000,
                        progressBar: true
                    });
                }

                // ✅ Update form data
                if (response.data) {
                    this.updateFormData($form, response.data);
                }

                // ✅ Clear validation classes
                $form.find('.is-invalid').removeClass('is-invalid');
                $form.find('.is-valid').addClass('is-valid');

                // ✅ Trigger custom event
                $form.trigger('profileComponent:updateSuccess', [response, instance]);
            } else {
                this.handleErrorResponse(response, instance);
            }
        },

        /**
         * ✅ Handle Error Response
         * @param {object} response - Server response
         * @param {object} instance - Component instance
         */
        handleErrorResponse: function(response, instance) {
            var $form = instance.form;
            var config = instance.config;
            var errorMessage = response.message || 'خطا در به‌روزرسانی پروفایل.';

            // ✅ Show error message
            if (config.enableToastr && window.toastr) {
                toastr.error(errorMessage, '', {
                    timeOut: 5000,
                    progressBar: true
                });
            }

            // ✅ Show validation errors
            if (response.validationErrors && response.validationErrors.length > 0) {
                this.showFieldErrors($form, response.validationErrors);
            } else {
                var $validationSummary = $form.find(config.validationSummarySelector);
                if ($validationSummary.length === 0) {
                    $validationSummary = $('<div class="alert alert-danger" role="alert"></div>');
                    $form.prepend($validationSummary);
                }
                $validationSummary.html('<i class="fas fa-exclamation-circle me-2"></i>' + errorMessage).show();
            }

            // ✅ Trigger custom event
            $form.trigger('profileComponent:updateError', [response, instance]);
        },

        /**
         * ✅ Handle AJAX Error
         * @param {object} xhr - XMLHttpRequest
         * @param {string} status - Status
         * @param {string} error - Error message
         * @param {object} instance - Component instance
         * @param {jQuery} $submitButton - Submit button
         * @param {string} originalButtonText - Original button text
         */
        handleError: function(xhr, status, error, instance, $submitButton, originalButtonText) {
            console.error('UserProfileComponent: AJAX Error', { xhr: xhr, status: status, error: error });

            var config = instance.config;
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

            if (config.enableToastr && window.toastr) {
                toastr.error(errorMessage, '', {
                    timeOut: 5000,
                    progressBar: true
                });
            }

            instance.form.trigger('profileComponent:ajaxError', [xhr, status, error, instance]);
        },

        /**
         * ✅ Update Form Data
         * @param {jQuery} $form - Form element
         * @param {object} data - Data to update
         */
        updateFormData: function($form, data) {
            if (!data) return;

            if (data.FirstName) $form.find('[name="FirstName"]').val(data.FirstName);
            if (data.LastName) $form.find('[name="LastName"]').val(data.LastName);
            if (data.Email) $form.find('[name="Email"]').val(data.Email);
            if (data.Gender !== undefined) $form.find('[name="Gender"]').val(data.Gender);
            if (data.Address !== undefined) $form.find('[name="Address"]').val(data.Address);

            $form.find('input, select, textarea').trigger('change');
        },

        /**
         * ✅ Show Field Errors
         * @param {jQuery} $form - Form element
         * @param {array} errors - Error array
         */
        showFieldErrors: function($form, errors) {
            var self = this;
            errors.forEach(function(error) {
                var $field = $form.find('[name="' + error.field + '"]');
                if ($field.length > 0) {
                    $field.addClass('is-invalid').removeClass('is-valid');
                    var $errorMsg = $field.siblings('.text-danger');
                    if ($errorMsg.length === 0) {
                        $errorMsg = $('<div class="text-danger small mt-1"></div>');
                        $field.after($errorMsg);
                    }
                    $errorMsg.text(error.message);
                }
            });
        },

        /**
         * ✅ Show Validation Errors
         * @param {object} instance - Component instance
         */
        showValidationErrors: function(instance) {
            var $form = instance.form;
            var config = instance.config;
            var $validationSummary = $form.find(config.validationSummarySelector);
            if ($validationSummary.length === 0) {
                $validationSummary = $('<div class="alert alert-danger" role="alert"></div>');
                $form.prepend($validationSummary);
            }
            $validationSummary.html('<i class="fas fa-exclamation-circle me-2"></i>لطفاً تمام فیلدهای الزامی را پر کنید.').show();
        },

        /**
         * ✅ Clear Errors
         * @param {object} instance - Component instance
         */
        clearErrors: function(instance) {
            var $form = instance.form;
            var config = instance.config;
            $form.find('.is-invalid').removeClass('is-invalid');
            $form.find('.text-danger').text('');
            $form.find(config.validationSummarySelector).hide();
        },

        /**
         * ✅ Clear Field Error
         * @param {jQuery} $field - Field element
         */
        clearFieldError: function($field) {
            $field.removeClass('is-invalid');
            $field.siblings('.text-danger').text('');
        },

        /**
         * ✅ Set Button Loading State
         * @param {jQuery} $button - Button element
         * @param {boolean} isLoading - Loading state
         * @param {string} originalText - Original text
         */
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

        /**
         * ✅ Handle Cancel
         * @param {object} instance - Component instance
         */
        handleCancel: function(instance) {
            var $form = instance.form;
            var config = instance.config;
            $form[0].reset();
            this.clearErrors(instance);
            
            if (config.enableToastr && window.toastr) {
                toastr.info('تغییرات لغو شد.', '', { timeOut: 2000 });
            }

            $form.trigger('profileComponent:cancel', [instance]);
        },

        /**
         * ✅ Load Component via AJAX
         * @param {string|jQuery} container - Container selector
         * @param {object} options - Load options
         * @returns {Promise} Promise
         */
        load: function(container, options) {
            var self = this;
            var $container = $(container);
            var deferred = $.Deferred();

            if ($container.length === 0) {
                deferred.reject('Container not found');
                return deferred.promise();
            }

            // ✅ Show loading
            $container.html('<div class="text-center p-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">در حال بارگذاری...</span></div></div>');

            // ✅ Build URL with options
            var url = '/Account/LoadProfileComponent';
            var params = $.param(options || {});
            if (params) {
                url += '?' + params;
            }

            // ✅ Load via AJAX
            $.ajax({
                url: url,
                method: 'GET',
                cache: false,
                success: function(html) {
                    $container.html(html);
                    
                    // ✅ Initialize component
                    var instance = self.init($container, options);
                    if (instance) {
                        deferred.resolve(instance);
                    } else {
                        deferred.reject('Failed to initialize component');
                    }
                },
                error: function(xhr, status, error) {
                    $container.html('<div class="alert alert-danger">خطا در بارگذاری کامپوننت پروفایل.</div>');
                    deferred.reject(error);
                }
            });

            return deferred.promise();
        },

        /**
         * ✅ Get Component Instance
         * @param {string|jQuery} container - Container selector
         * @returns {object} Component instance
         */
        getInstance: function(container) {
            var $container = $(container);
            var instanceId = $container.data('profile-component-id');
            return instanceId ? componentRegistry[instanceId] : null;
        },

        /**
         * ✅ Destroy Component Instance
         * @param {string|jQuery} container - Container selector
         */
        destroy: function(container) {
            var $container = $(container);
            var instanceId = $container.data('profile-component-id');
            
            if (instanceId && componentRegistry[instanceId]) {
                var instance = componentRegistry[instanceId];
                
                // ✅ Unbind events
                instance.form.off('.profileComponent');
                instance.form.find('input, select, textarea').off('.profileComponent');
                
                // ✅ Remove from registry
                delete componentRegistry[instanceId];
                $container.removeData('profile-component-id');
            }
        },

        /**
         * ✅ Generate Instance ID
         * @returns {string} Instance ID
         */
        generateInstanceId: function() {
            return 'profile-component-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9);
        }
    };

    // ✅ Auto-initialize on document ready
    $(document).ready(function() {
        // ✅ Find all components with data attribute
        $('[data-profile-component="true"]').each(function() {
            var $container = $(this).closest('[data-profile-component="true"]');
            if ($container.length > 0) {
                UserProfileComponent.init($container);
            }
        });
    });

    // ✅ Expose globally
    window.UserProfileComponent = UserProfileComponent;

})(jQuery, window);

