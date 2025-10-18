/**
 * DEPARTMENT SELECTION MODULE - ماژول انتخاب دپارتمان
 * ========================================
 * 
 * این ماژول مسئولیت‌های زیر را دارد:
 * - بارگذاری لیست دپارتمان‌ها
 * - انتخاب دپارتمان
 * - بارگذاری پزشکان بر اساس دپارتمان
 * - مدیریت خطاها و loading states
 * 
 * @author ClinicApp Development Team
 * @version 1.0.0
 * @since 2025-01-17
 */

(function(global, $) {
    'use strict';

    // ========================================
    // MODULE CONFIGURATION - تنظیمات ماژول
    // ========================================
    var CONFIG = {
        // API Endpoints
        endpoints: {
            loadDepartments: '/Reception/Department/Load',
            loadDoctors: '/Reception/Department/LoadDoctors',
            saveSelection: '/Reception/Department/Save'
        },
        
        // Validation Rules
        validation: {
            ajaxTimeout: 10000,
            maxRetries: 3,
            retryDelay: 1000
        },
        
        // UI Selectors
        selectors: {
            departmentSelect: '#departmentSelect',
            doctorSelect: '#doctorSelect',
            loadBtn: '#loadDepartmentsBtn',
            saveBtn: '#saveDepartmentBtn',
            statusMessage: '#departmentStatusMessage',
            loadingIndicator: '#departmentLoadingIndicator'
        },
        
        // Messages
        messages: {
            info: {
                loadingDepartments: 'در حال بارگذاری دپارتمان‌ها...',
                loadingDoctors: 'در حال بارگذاری پزشکان...'
            },
            success: {
                departmentsLoaded: 'دپارتمان‌ها با موفقیت بارگذاری شدند',
                doctorsLoaded: 'پزشکان با موفقیت بارگذاری شدند',
                selectionSaved: 'انتخاب دپارتمان با موفقیت ذخیره شد'
            },
            error: {
                loadError: 'خطا در بارگذاری دپارتمان‌ها',
                doctorsLoadError: 'خطا در بارگذاری پزشکان',
                saveError: 'خطا در ذخیره انتخاب دپارتمان',
                networkError: 'خطا در اتصال به شبکه'
            }
        }
    };

    // ========================================
    // DEPARTMENT SELECTION MODULE - ماژول اصلی
    // ========================================
    var DepartmentSelectionModule = {
        
        // ========================================
        // INITIALIZATION - راه‌اندازی
        // ========================================
        init: function() {
            console.log('[DepartmentSelectionModule] Initializing...');
            
            try {
                this.bindEvents();
                this.setupEventListeners();
                this.initializeSelect2();
                
                console.log('[DepartmentSelectionModule] Initialized successfully');
            } catch (error) {
                console.error('[DepartmentSelectionModule] Initialization failed:', error);
                this.handleError(error, 'initialization');
            }
        },

        // ========================================
        // EVENT BINDING - اتصال رویدادها
        // ========================================
        bindEvents: function() {
            var self = this;
            
            // Load Departments Button
            $(CONFIG.selectors.loadBtn).off('click.departmentSelection')
                .on('click.departmentSelection', function() {
                    self.handleLoadDepartments.call(this, self);
                });
            
            // Department Selection Change
            $(CONFIG.selectors.departmentSelect).off('change.departmentSelection')
                .on('change.departmentSelection', function() {
                    self.handleDepartmentChange.call(this, self);
                });
            
            // Save Selection Button
            $(CONFIG.selectors.saveBtn).off('click.departmentSelection')
                .on('click.departmentSelection', function() {
                    self.handleSaveSelection.call(this, self);
                });
        },

        // ========================================
        // EVENT LISTENERS SETUP - تنظیم شنوندگان رویداد
        // ========================================
        setupEventListeners: function() {
            var self = this;
            
            // گوش دادن به رویدادهای سیستم
            if (window.ReceptionEventBus) {
                window.ReceptionEventBus.on('patient:found', function(data) {
                    self.handlePatientFound(data);
                });
                
                window.ReceptionEventBus.on('system:error', function(error) {
                    self.handleSystemError(error);
                });
            }
        },

        // ========================================
        // SELECT2 INITIALIZATION - راه‌اندازی Select2
        // ========================================
        initializeSelect2: function() {
            try {
                // Initialize Department Select2
                $(CONFIG.selectors.departmentSelect).select2({
                    placeholder: 'انتخاب دپارتمان',
                    allowClear: true,
                    language: 'fa',
                    dir: 'rtl',
                    width: '100%'
                });
                
                // Initialize Doctor Select2
                $(CONFIG.selectors.doctorSelect).select2({
                    placeholder: 'انتخاب پزشک',
                    allowClear: true,
                    language: 'fa',
                    dir: 'rtl',
                    width: '100%',
                    disabled: true
                });
                
            } catch (error) {
                console.error('[DepartmentSelectionModule] Select2 initialization failed:', error);
                this.handleError(error, 'select2Initialization');
            }
        },

        // ========================================
        // LOAD DEPARTMENTS HANDLER - مدیریت بارگذاری دپارتمان‌ها
        // ========================================
        handleLoadDepartments: function(module) {
            const $btn = $(this);
            module.showButtonLoading($btn);
            module.loadDepartments();
        },

        // ========================================
        // LOAD DEPARTMENTS - بارگذاری دپارتمان‌ها
        // ========================================
        loadDepartments: function() {
            this.showInfo(CONFIG.messages.info.loadingDepartments);
            this.showLoadingIndicator(true);

            var self = this;
            
            $.ajax({
                url: CONFIG.endpoints.loadDepartments,
                type: 'POST',
                data: { 
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                dataType: 'json',
                timeout: CONFIG.validation.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.hideLoadingIndicator();
                    self.handleLoadDepartmentsSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideLoadingIndicator();
                    self.handleLoadDepartmentsError(xhr, status, error);
                }
            });
        },

        // ========================================
        // LOAD DEPARTMENTS SUCCESS HANDLER - مدیریت موفقیت بارگذاری دپارتمان‌ها
        // ========================================
        handleLoadDepartmentsSuccess: function(response) {
            if (response.success && response.data) {
                this.populateDepartments(response.data);
                this.showSuccess(CONFIG.messages.success.departmentsLoaded);
                this.updateAccordionState('departmentSection', 'completed');
            } else {
                this.showError(response.message || CONFIG.messages.error.loadError);
            }
        },

        // ========================================
        // LOAD DEPARTMENTS ERROR HANDLER - مدیریت خطای بارگذاری دپارتمان‌ها
        // ========================================
        handleLoadDepartmentsError: function(xhr, status, error) {
            console.error('[DepartmentSelectionModule] Error loading departments:', error);
            this.showError(CONFIG.messages.error.loadError);
        },

        // ========================================
        // POPULATE DEPARTMENTS - پر کردن لیست دپارتمان‌ها
        // ========================================
        populateDepartments: function(departments) {
            if (!departments || !Array.isArray(departments)) {
                console.error('[DepartmentSelectionModule] Invalid departments data');
                return;
            }
            
            try {
                const $select = $(CONFIG.selectors.departmentSelect);
                $select.empty();
                
                // اضافه کردن گزینه پیش‌فرض
                $select.append('<option value="">انتخاب دپارتمان</option>');
                
                // اضافه کردن دپارتمان‌ها
                departments.forEach(function(department) {
                    $select.append(`<option value="${department.id}">${department.name}</option>`);
                });
                
                // به‌روزرسانی Select2
                $select.trigger('change');
                
            } catch (error) {
                console.error('[DepartmentSelectionModule] Error populating departments:', error);
                this.handleError(error, 'populateDepartments');
            }
        },

        // ========================================
        // DEPARTMENT CHANGE HANDLER - مدیریت تغییر دپارتمان
        // ========================================
        handleDepartmentChange: function(module) {
            var departmentId = $(this).val();
            
            if (departmentId) {
                module.loadDoctors(departmentId);
            } else {
                module.clearDoctors();
            }
        },

        // ========================================
        // LOAD DOCTORS - بارگذاری پزشکان
        // ========================================
        loadDoctors: function(departmentId) {
            if (!departmentId) {
                console.warn('[DepartmentSelectionModule] Department ID not provided');
                return;
            }

            this.showInfo(CONFIG.messages.info.loadingDoctors);
            this.showLoadingIndicator(true);

            var self = this;
            
            $.ajax({
                url: CONFIG.endpoints.loadDoctors,
                type: 'POST',
                data: { 
                    departmentId: departmentId,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                dataType: 'json',
                timeout: CONFIG.validation.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.hideLoadingIndicator();
                    self.handleLoadDoctorsSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideLoadingIndicator();
                    self.handleLoadDoctorsError(xhr, status, error);
                }
            });
        },

        // ========================================
        // LOAD DOCTORS SUCCESS HANDLER - مدیریت موفقیت بارگذاری پزشکان
        // ========================================
        handleLoadDoctorsSuccess: function(response) {
            if (response.success && response.data) {
                this.populateDoctors(response.data);
                this.showSuccess(CONFIG.messages.success.doctorsLoaded);
            } else {
                this.showError(response.message || CONFIG.messages.error.doctorsLoadError);
            }
        },

        // ========================================
        // LOAD DOCTORS ERROR HANDLER - مدیریت خطای بارگذاری پزشکان
        // ========================================
        handleLoadDoctorsError: function(xhr, status, error) {
            console.error('[DepartmentSelectionModule] Error loading doctors:', error);
            this.showError(CONFIG.messages.error.doctorsLoadError);
        },

        // ========================================
        // POPULATE DOCTORS - پر کردن لیست پزشکان
        // ========================================
        populateDoctors: function(doctors) {
            if (!doctors || !Array.isArray(doctors)) {
                console.error('[DepartmentSelectionModule] Invalid doctors data');
                return;
            }
            
            try {
                const $select = $(CONFIG.selectors.doctorSelect);
                $select.empty();
                
                // اضافه کردن گزینه پیش‌فرض
                $select.append('<option value="">انتخاب پزشک</option>');
                
                // اضافه کردن پزشکان
                doctors.forEach(function(doctor) {
                    $select.append(`<option value="${doctor.id}">${doctor.name}</option>`);
                });
                
                // فعال کردن Select2
                $select.prop('disabled', false);
                $select.trigger('change');
                
            } catch (error) {
                console.error('[DepartmentSelectionModule] Error populating doctors:', error);
                this.handleError(error, 'populateDoctors');
            }
        },

        // ========================================
        // CLEAR DOCTORS - پاک کردن لیست پزشکان
        // ========================================
        clearDoctors: function() {
            const $select = $(CONFIG.selectors.doctorSelect);
            $select.empty();
            $select.append('<option value="">انتخاب پزشک</option>');
            $select.prop('disabled', true);
            $select.trigger('change');
        },

        // ========================================
        // SAVE SELECTION HANDLER - مدیریت ذخیره انتخاب
        // ========================================
        handleSaveSelection: function(module) {
            const $btn = $(this);
            var departmentId = $(CONFIG.selectors.departmentSelect).val();
            var doctorId = $(CONFIG.selectors.doctorSelect).val();
            
            if (!departmentId) {
                module.showError('لطفا دپارتمان را انتخاب کنید');
                return;
            }
            
            module.saveSelection(departmentId, doctorId, $btn);
        },

        // ========================================
        // SAVE SELECTION - ذخیره انتخاب
        // ========================================
        saveSelection: function(departmentId, doctorId, $btn) {
            this.showButtonLoading($btn);
            
            var self = this;
            
            $.ajax({
                url: CONFIG.endpoints.saveSelection,
                type: 'POST',
                data: { 
                    departmentId: departmentId,
                    doctorId: doctorId,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                dataType: 'json',
                timeout: CONFIG.validation.ajaxTimeout,
                cache: false,
                success: function(response) {
                    self.hideButtonLoading($btn);
                    self.handleSaveSelectionSuccess(response);
                },
                error: function(xhr, status, error) {
                    self.hideButtonLoading($btn);
                    self.handleSaveSelectionError(xhr, status, error);
                }
            });
        },

        // ========================================
        // SAVE SELECTION SUCCESS HANDLER - مدیریت موفقیت ذخیره انتخاب
        // ========================================
        handleSaveSelectionSuccess: function(response) {
            if (response.success) {
                this.showSuccess(CONFIG.messages.success.selectionSaved);
                this.updateAccordionState('departmentSection', 'completed');
            } else {
                this.showError(response.message || CONFIG.messages.error.saveError);
            }
        },

        // ========================================
        // SAVE SELECTION ERROR HANDLER - مدیریت خطای ذخیره انتخاب
        // ========================================
        handleSaveSelectionError: function(xhr, status, error) {
            console.error('[DepartmentSelectionModule] Error saving selection:', error);
            this.showError(CONFIG.messages.error.saveError);
        },

        // ========================================
        // PATIENT FOUND HANDLER - مدیریت یافتن بیمار
        // ========================================
        handlePatientFound: function(data) {
            console.log('[DepartmentSelectionModule] Patient found, enabling department selection');
            // فعال کردن انتخاب دپارتمان پس از یافتن بیمار
            $(CONFIG.selectors.loadBtn).prop('disabled', false);
        },

        // ========================================
        // SYSTEM ERROR HANDLER - مدیریت خطای سیستم
        // ========================================
        handleSystemError: function(error) {
            console.error('[DepartmentSelectionModule] System error:', error);
        },

        // ========================================
        // UI HELPER METHODS - متدهای کمکی UI
        // ========================================
        showButtonLoading: function($btn) {
            if (!$btn || $btn.length === 0) {
                console.error('[DepartmentSelectionModule] Button element is null or undefined');
                return;
            }
            
            try {
                $btn.prop('disabled', true);
                $btn.find('.btn-text').addClass('d-none');
                $btn.find('.btn-loading').removeClass('d-none');
            } catch (error) {
                console.error('[DepartmentSelectionModule] Error showing button loading:', error);
            }
        },

        hideButtonLoading: function($btn) {
            if (!$btn || $btn.length === 0) {
                console.error('[DepartmentSelectionModule] Button element is null or undefined');
                return;
            }
            
            try {
                $btn.prop('disabled', false);
                $btn.find('.btn-text').removeClass('d-none');
                $btn.find('.btn-loading').addClass('d-none');
            } catch (error) {
                console.error('[DepartmentSelectionModule] Error hiding button loading:', error);
            }
        },

        showLoadingIndicator: function(show) {
            const $indicator = $(CONFIG.selectors.loadingIndicator);
            if (show) {
                $indicator.removeClass('d-none');
            } else {
                $indicator.addClass('d-none');
            }
        },

        hideLoadingIndicator: function() {
            this.showLoadingIndicator(false);
        },

        // ========================================
        // MESSAGE DISPLAY METHODS - متدهای نمایش پیام
        // ========================================
        showError: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showError(message);
            } else {
                console.error('[DepartmentSelectionModule] Error:', message);
            }
        },

        showSuccess: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showSuccess(message);
            } else {
                console.log('[DepartmentSelectionModule] Success:', message);
            }
        },

        showInfo: function(message) {
            if (window.ReceptionToastr) {
                window.ReceptionToastr.helpers.showInfo(message);
            } else {
                console.log('[DepartmentSelectionModule] Info:', message);
            }
        },

        // ========================================
        // UTILITY METHODS - متدهای کمکی
        // ========================================
        updateAccordionState: function(section, state) {
            if (window.ReceptionModules && window.ReceptionModules.Accordion) {
                window.ReceptionModules.Accordion.setState(section, state);
            }
        },

        // ========================================
        // ERROR HANDLING - مدیریت خطا
        // ========================================
        handleError: function(error, context) {
            console.error(`[DepartmentSelectionModule] Error in ${context}:`, error);
            
            if (window.ReceptionErrorHandler && typeof window.ReceptionErrorHandler.handle === 'function') {
                window.ReceptionErrorHandler.handle(error, 'DepartmentSelectionModule', context);
            }
        }
    };

    // ========================================
    // MODULE EXPORT - صادرات ماژول
    // ========================================
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = DepartmentSelectionModule;
    } else {
        global.DepartmentSelectionModule = DepartmentSelectionModule;
    }

})(window, jQuery);
