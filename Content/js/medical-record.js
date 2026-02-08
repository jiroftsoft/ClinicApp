/**
 * Medical Record Component Manager
 * Single Responsibility: مدیریت AJAX Loading برای Components
 * Pattern: مشابه patient-dashboard.js
 * ✅ Enterprise-Grade: Component-Based, AJAX-First, Error Handling
 */
(function($, window) {
    'use strict';
    
    // ✅ Configuration
    var config = {
        apiBaseUrl: '/Patient/Api/MedicalRecord',
        sections: {
            medicalHistory: {
                url: '/GetMedicalHistories',
                container: '[data-medical-record-section="medicalHistory"]',
                partial: '_MedicalHistorySection'
            },
            appointments: {
                url: '/GetAppointments',
                container: '[data-medical-record-section="appointments"]',
                partial: '_AppointmentsSection'
            },
            receptions: {
                url: '/GetReceptions',
                container: '[data-medical-record-section="receptions"]',
                partial: '_ReceptionsSection'
            },
            triage: {
                url: '/GetTriageAssessments',
                container: '[data-medical-record-section="triage"]',
                partial: '_TriageSection'
            }
        },
        retryAttempts: 3,
        retryDelay: 2000
    };
    
    /** ✅ URL از سرور (data-* روی #medicalRecordShell) برای جلوگیری از 404 / virtual path */
    function getApiBaseUrl() {
        var $shell = $('#medicalRecordShell');
        if ($shell.length && $shell.data('api-base')) return $shell.data('api-base');
        return config.apiBaseUrl;
    }
    function getCreateMedicalHistoryUrl() {
        var $shell = $('#medicalRecordShell');
        if ($shell.length && $shell.data('api-create')) return $shell.data('api-create');
        return config.apiBaseUrl + '/CreateMedicalHistory';
    }
    function getUpdateMedicalHistoryUrl() {
        var $shell = $('#medicalRecordShell');
        if ($shell.length && $shell.data('api-update')) return $shell.data('api-update');
        return config.apiBaseUrl + '/UpdateMedicalHistory';
    }
    
    // ✅ MedicalRecord - Enterprise-Grade Module
    var MedicalRecord = {
        
        /**
         * ✅ Initialize Module
         */
        init: function() {
            this.loadAllSections();
            this.bindEvents();
        },
        
        /**
         * ✅ Load All Sections
         */
        loadAllSections: function() {
            var self = this;
            
            // ✅ Load all sections in parallel
            Promise.all([
                this.loadSection('medicalHistory'),
                this.loadSection('appointments'),
                this.loadSection('receptions'),
                this.loadSection('triage')
            ]).catch(function(error) {
                console.error('Error loading medical record sections:', error);
            });
        },
        
        /**
         * ✅ Load Section via AJAX
         * @param {string} sectionName - Name of the section
         */
        loadSection: function(sectionName) {
            var self = this;
            var section = config.sections[sectionName];
            
            if (!section) {
                console.error('Unknown section:', sectionName);
                return Promise.reject('Unknown section');
            }
            
            // ✅ همیشه بخش را داخل Shell جستجو کن تا در داشبورد به‌درستی به‌روز شود
            var $shell = $('#medicalRecordShell');
            var $container = $shell.length ? $shell.find(section.container).first() : $(section.container);
            if ($container.length === 0) {
                console.warn('Container not found for section:', sectionName);
                return Promise.reject('Container not found');
            }
            
            // ✅ Show loading state
            this.showLoading($container);
            
            // ✅ Build URL (ترجیح از data-api-base سرور)
            var url = getApiBaseUrl() + section.url;
            
            // ✅ AJAX request
            return $.ajax({
                url: url,
                method: 'GET',
                dataType: 'json',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'X-AJAX-Request': 'true'
                },
                cache: false,
                timeout: 30000
            }).then(function(response) {
                var ok = response && (response.success === true || response.Success === true);
                var data = response && (response.data !== undefined ? response.data : response.Data);
                if (ok) {
                    var list = Array.isArray(data) ? data : (data && (data.items || data.Items) ? (data.items || data.Items) : []);
                    if (!Array.isArray(list)) list = [];
                    if (sectionName === 'medicalHistory') {
                        if (list.length > 0) {
                            console.log('✅ Medical history loaded:', list.length, 'item(s)');
                        } else {
                            console.warn('⚠️ GetMedicalHistories returned success but 0 items. Check Patient.ApplicationUserId for PatientId 7127.');
                        }
                    }
                    self.renderSection($container, section.partial, list);
                } else {
                    self.showError($container, (response && (response.message || response.Message)) || 'خطا در بارگذاری');
                }
            }).catch(function(xhr, status, error) {
                console.error('AJAX Error for section:', sectionName, { xhr: xhr, status: status, error: error });
                
                if (xhr.status === 401) {
                    // ✅ Unauthorized - redirect to login
                    if (window.openLoginModal) {
                        window.openLoginModal(window.location.href);
                    } else {
                        window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.href);
                    }
                } else {
                    self.showError($container, 'خطا در بارگذاری. لطفاً دوباره تلاش کنید.');
                }
            });
        },
        
        /**
         * ✅ Render Section
         * @param {jQuery} $container - Container element
         * @param {string} partialName - Name of the partial view
         * @param {object} data - Data to render
         */
        renderSection: function($container, partialName, data) {
            var self = this;
            
            // ✅ Hide loading
            this.hideLoading($container);
            
            // ✅ Check if data is empty
            var isEmpty = false;
            if (Array.isArray(data)) {
                isEmpty = data.length === 0;
            } else if (data && typeof data === 'object') {
                isEmpty = Object.keys(data).length === 0;
            }
            
            var $cardBody = $container.find('.medical-record-card-body');
            if ($cardBody.length === 0) {
                $cardBody = $container;
            }
            
            if (isEmpty) {
                // ✅ Show empty state
                $cardBody.find('.medical-record-section-content').hide();
                $cardBody.find('.medical-record-section-empty').show();
                $cardBody.find('.medical-record-section-error').hide();
            } else {
                // ✅ Render content via AJAX (load partial view)
                var renderUrl = '/Patient/MedicalRecord/RenderPartial?partialName=' + encodeURIComponent(partialName);
                var token = $('input[name="__RequestVerificationToken"]').first().val();
                var ajaxHeaders = {
                    'X-Requested-With': 'XMLHttpRequest',
                    'X-AJAX-Request': 'true'
                };
                if (token) ajaxHeaders['RequestVerificationToken'] = token;
                
                $.ajax({
                    url: renderUrl,
                    method: 'POST',
                    data: JSON.stringify(data),
                    contentType: 'application/json',
                    dataType: 'html',
                    headers: ajaxHeaders,
                    success: function(html) {
                        $cardBody.find('.medical-record-section-content').html(html).show();
                        $cardBody.find('.medical-record-section-empty').hide();
                        $cardBody.find('.medical-record-section-error').hide();
                        
                        // ✅ Reinitialize components
                        self.reinitializeComponents($cardBody);
                    },
                    error: function(xhr) {
                        console.error('Error rendering partial:', partialName, xhr);
                        self.showError($container, 'خطا در نمایش محتوا');
                    }
                });
            }
        },
        
        /**
         * ✅ Show Loading State
         */
        showLoading: function($container) {
            var $cardBody = $container.find('.medical-record-card-body');
            if ($cardBody.length === 0) {
                $cardBody = $container;
            }
            
            $cardBody.find('.medical-record-section-loading').show();
            $cardBody.find('.medical-record-section-content').hide();
            $cardBody.find('.medical-record-section-empty').hide();
            $cardBody.find('.medical-record-section-error').hide();
        },
        
        /**
         * ✅ Hide Loading State
         */
        hideLoading: function($container) {
            var $cardBody = $container.find('.medical-record-card-body');
            if ($cardBody.length === 0) {
                $cardBody = $container;
            }
            
            $cardBody.find('.medical-record-section-loading').hide();
        },
        
        /**
         * ✅ Show Error State
         */
        showError: function($container, message) {
            var self = this;
            var $cardBody = $container.find('.medical-record-card-body');
            if ($cardBody.length === 0) {
                $cardBody = $container;
            }
            
            this.hideLoading($container);
            
            var $errorDiv = $cardBody.find('.medical-record-section-error');
            $errorDiv.find('.error-message').text(message || 'خطا در بارگذاری');
            $errorDiv.show();
            $cardBody.find('.medical-record-section-content').hide();
            $cardBody.find('.medical-record-section-empty').hide();
        },
        
        /**
         * ✅ Reinitialize Components
         */
        reinitializeComponents: function($container) {
            // ✅ Reinitialize tooltips if exists
            if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
                $container.find('[data-bs-toggle="tooltip"]').each(function() {
                    new bootstrap.Tooltip(this);
                });
            }
            
            // ✅ Reinitialize modals if exists
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                $container.find('[data-toggle="modal"]').each(function() {
                    // Modal will be initialized on click
                });
            }
        },
        
        /**
         * ✅ Bind Events
         */
        bindEvents: function() {
            var self = this;
            
            // ✅ Refresh button
            $(document).on('click', '.refresh-medical-record', function() {
                self.loadAllSections();
            });
            
            // ✅ Reload section button
            $(document).on('click', '.reload-section', function() {
                var sectionName = $(this).data('section');
                if (sectionName) {
                    self.loadSection(sectionName);
                }
            });
            
            // ✅ Add medical history button
            $(document).on('click', '.add-medical-history', function() {
                self.openMedicalHistoryModal();
            });
            
            // ✅ Edit medical history
            $(document).on('click', '.edit-medical-history', function() {
                var medicalHistoryId = $(this).data('medical-history-id');
                if (medicalHistoryId) {
                    self.loadMedicalHistoryForEdit(medicalHistoryId);
                }
            });
            
            // ✅ Delete medical history
            $(document).on('click', '.delete-medical-history', function() {
                var medicalHistoryId = $(this).data('medical-history-id');
                if (medicalHistoryId) {
                    self.deleteMedicalHistory(medicalHistoryId);
                }
            });
            
            // ✅ Medical history form submit
            $(document).on('submit', '#medicalHistoryForm', function(e) {
                e.preventDefault();
                self.saveMedicalHistory();
            });
            
            // ✅ File input change
            $(document).on('change', '#Attachments', function() {
                self.previewAttachments(this.files);
            });
        },
        
        /**
         * ✅ Open Medical History Modal (Create)
         */
        openMedicalHistoryModal: function() {
            var $modal = $('#medicalHistoryModal');
            if ($modal.length === 0) {
                console.error('Medical history modal not found');
                return;
            }
            
            // Reset form
            var form = $('#medicalHistoryForm')[0];
            if (form) form.reset();
            $('#MedicalHistoryId').val('');
            $('#Title').val('');
            $('#modalTitle').text('افزودن تاریخچه پزشکی');
            $('#attachmentsPreview').empty();
            $('#startDatePicker').val('');
            $('#endDatePicker').val('');
            
            // ✅ DatePicker شمسی: اجرای مجدد startWatch تا تقویم روی کلیک باز شود (به‌خصوص وقتی مودال با AJAX لود شده)
            if (typeof JalaliDatePickerEnterprise !== 'undefined' && JalaliDatePickerEnterprise.startWatchAgain) {
                setTimeout(function() {
                    JalaliDatePickerEnterprise.startWatchAgain();
                }, 100);
            }
            
            // Show modal (Bootstrap 5)
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                var modalInstance = bootstrap.Modal.getOrCreateInstance($modal[0]);
                modalInstance.show();
            } else if ($modal.modal) {
                $modal.modal('show');
            }
        },
        
        /**
         * ✅ Load Medical History for Edit
         */
        loadMedicalHistoryForEdit: function(medicalHistoryId) {
            var self = this;
            
            $.ajax({
                url: getApiBaseUrl() + '/GetMedicalHistory',
                method: 'GET',
                data: { id: medicalHistoryId },
                dataType: 'json',
                success: function(response) {
                    if (response && response.success && response.data) {
                        self.populateMedicalHistoryForm(response.data);
                        $('#modalTitle').text('ویرایش تاریخچه پزشکی');
                        
                        var $modal = $('#medicalHistoryModal');
                        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                            var modalInstance = bootstrap.Modal.getOrCreateInstance($modal[0]);
                            modalInstance.show();
                        } else if ($modal.modal) {
                            $modal.modal('show');
                        }
                    } else {
                        Swal.fire({
                            title: 'خطا',
                            text: response?.message || 'خطا در دریافت اطلاعات',
                            icon: 'error',
                            confirmButtonText: 'باشه'
                        });
                    }
                },
                error: function(xhr) {
                    console.error('Error loading medical history:', xhr);
                    Swal.fire({
                        title: 'خطا',
                        text: 'خطا در دریافت اطلاعات',
                        icon: 'error',
                        confirmButtonText: 'باشه'
                    });
                }
            });
        },
        
        /**
         * ✅ Populate Medical History Form
         */
        populateMedicalHistoryForm: function(data) {
            $('#MedicalHistoryId').val(data.MedicalHistoryId);
            $('#Type').val(data.Type);
            $('#Title').val(data.Title);
            $('#Description').val(data.Description);
            // ✅ طبق قرارداد: DatePicker شمسی - مقدار از API به صورت شمسی (StartDateShamsi / EndDateShamsi)
            $('#startDatePicker').val(data.StartDateShamsi || '');
            $('#endDatePicker').val(data.EndDateShamsi || '');
            $('#Severity').val(data.Severity);
            $('#IsActive').prop('checked', data.IsActive);
            $('#DoctorName').val(data.DoctorName);
            $('#MedicalCenter').val(data.MedicalCenter);
        },
        
        /**
         * ✅ Save Medical History (Create/Update)
         */
        saveMedicalHistory: function() {
            var self = this;
            var $form = $('#medicalHistoryForm');
            var $btn = $('#saveMedicalHistoryBtn');
            var medicalHistoryId = $('#MedicalHistoryId').val();
            var isEdit = medicalHistoryId && medicalHistoryId !== '';
            
            // Disable button
            $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin ml-1"></i> در حال ذخیره...');
            
            // Get form data (FormData شامل __RequestVerificationToken داخل همین فرم است)
            var formData = new FormData($form[0]);
            // ✅ اگر توکن داخل فرم نبود (مثلاً لود AJAX)، از همین فرم بگیر تا با کوکی سرور جور باشد
            var token = $form.find('input[name="__RequestVerificationToken"]').val();
            if (token && !formData.has('__RequestVerificationToken')) {
                formData.append('__RequestVerificationToken', token);
            }
            
            // ✅ URL از سرور (data-api-create / data-api-update) برای جلوگیری از 404
            var url = isEdit ? getUpdateMedicalHistoryUrl() : getCreateMedicalHistoryUrl();
            
            $.ajax({
                url: url,
                method: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                dataType: 'json',
                success: function(response) {
                    if (response && response.success) {
                        Swal.fire({
                            title: 'موفق',
                            text: response.message || 'با موفقیت ذخیره شد',
                            icon: 'success',
                            confirmButtonText: 'باشه'
                        }).then(function() {
                            // Close modal
                            var $modal = $('#medicalHistoryModal');
                            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                                var modalInstance = bootstrap.Modal.getInstance($modal[0]);
                                if (modalInstance) modalInstance.hide();
                            } else if ($modal.modal) {
                                $modal.modal('hide');
                            }
                            
                            // Reload medical history section
                            self.loadSection('medicalHistory');
                        });
                    } else {
                        Swal.fire({
                            title: 'خطا',
                            text: response?.message || 'خطا در ذخیره',
                            icon: 'error',
                            confirmButtonText: 'باشه'
                        });
                    }
                },
                error: function(xhr) {
                    console.error('Error saving medical history:', xhr);
                    var errorMessage = 'خطا در ذخیره';
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    }
                    Swal.fire({
                        title: 'خطا',
                        text: errorMessage,
                        icon: 'error',
                        confirmButtonText: 'باشه'
                    });
                },
                complete: function() {
                    $btn.prop('disabled', false).html('<i class="fas fa-save ml-1"></i> ذخیره');
                }
            });
        },
        
        /**
         * ✅ Delete Medical History
         */
        deleteMedicalHistory: function(medicalHistoryId) {
            var self = this;
            
            Swal.fire({
                title: 'آیا مطمئن هستید؟',
                text: 'این عمل قابل بازگشت نیست',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'بله، حذف کن',
                cancelButtonText: 'انصراف'
            }).then(function(result) {
                if (result.isConfirmed) {
                    $.ajax({
                        url: getApiBaseUrl() + '/DeleteMedicalHistory',
                        method: 'POST',
                        data: { id: medicalHistoryId },
                        dataType: 'json',
                        success: function(response) {
                            if (response && response.success) {
                                Swal.fire({
                                    title: 'موفق',
                                    text: response.message || 'با موفقیت حذف شد',
                                    icon: 'success',
                                    confirmButtonText: 'باشه'
                                }).then(function() {
                                    // Reload medical history section
                                    self.loadSection('medicalHistory');
                                });
                            } else {
                                Swal.fire({
                                    title: 'خطا',
                                    text: response?.message || 'خطا در حذف',
                                    icon: 'error',
                                    confirmButtonText: 'باشه'
                                });
                            }
                        },
                        error: function(xhr) {
                            console.error('Error deleting medical history:', xhr);
                            Swal.fire({
                                title: 'خطا',
                                text: 'خطا در حذف',
                                icon: 'error',
                                confirmButtonText: 'باشه'
                            });
                        }
                    });
                }
            });
        },
        
        /**
         * ✅ Preview Attachments
         */
        previewAttachments: function(files) {
            var $preview = $('#attachmentsPreview');
            $preview.empty();
            
            if (!files || files.length === 0) return;
            
            var maxFiles = 5;
            var maxSize = 5 * 1024 * 1024; // 5MB
            
            if (files.length > maxFiles) {
                Swal.fire({
                    title: 'خطا',
                    text: 'حداکثر ' + maxFiles + ' فایل می‌توانید انتخاب کنید',
                    icon: 'error',
                    confirmButtonText: 'باشه'
                });
                return;
            }
            
            for (var i = 0; i < files.length; i++) {
                var file = files[i];
                
                if (file.size > maxSize) {
                    Swal.fire({
                        title: 'خطا',
                        text: 'فایل ' + file.name + ' بیش از 5 مگابایت است',
                        icon: 'error',
                        confirmButtonText: 'باشه'
                    });
                    continue;
                }
                
                var $item = $('<div class="attachment-preview-item mb-2"></div>');
                $item.html(
                    '<i class="fas fa-file ml-2"></i>' +
                    '<span>' + file.name + '</span>' +
                    '<small class="text-muted mr-2">(' + (file.size / 1024 / 1024).toFixed(2) + ' MB)</small>'
                );
                $preview.append($item);
            }
        }
    };
    
    // ✅ Initialize on document ready
    $(document).ready(function() {
        if ($('#medicalRecordContainer').length > 0 || $('.medical-record-shell').length > 0) {
            MedicalRecord.init();
        }
    });
    
    // ✅ Expose globally
    window.MedicalRecord = MedicalRecord;
})(jQuery, window);

