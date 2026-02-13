/**
 * JavaScript Module برای انتخاب پزشک
 * رعایت SRP: فقط مدیریت جستجو و انتخاب پزشک
 */
(function ($) {
    'use strict';

    // ✅ FIX Issue 7: Conditional Console Logging (طبق SELECT_DOCTOR_MODULE_REVIEW.md)
    const DEBUG = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';
    const log = DEBUG ? console.log.bind(console) : function() {};
    const warn = DEBUG ? console.warn.bind(console) : function() {};
    const error = console.error.bind(console); // ✅ Error همیشه لاگ می‌شود

    const DoctorSelection = {
        init: function () {
            log('🔵 [DoctorSelection] Initializing...');
            
            // ✅ Check if jQuery is loaded
            if (typeof jQuery === 'undefined') {
                error('❌ [DoctorSelection] jQuery is not loaded!');
                return;
            }
            log('✅ [DoctorSelection] jQuery loaded');

            this.bindEvents();
            this.setupSearchDebounce();
            
            // ✅ Verify buttons exist
            const $buttons = $('.select-doctor-btn');
            log(`🔵 [DoctorSelection] Found ${$buttons.length} select-doctor buttons`);
            
            if ($buttons.length === 0) {
                warn('⚠️ [DoctorSelection] No select-doctor buttons found! Buttons may be rendered after script loads.');
            }
        },

        bindEvents: function () {
            log('🔵 [DoctorSelection] Binding events...');
            
            // ✅ BEST PRACTICE: فقط برای <button> tags handler بزن (طبق قراردادها)
            // برای <a> tags، navigation طبیعی را اجازه بده (بدون interference)
            $(document).off('click', '.select-doctor-btn[type="button"]');
            $(document).on('click', '.select-doctor-btn[type="button"]', this.handleSelectDoctor.bind(this));
            
            // ✅ برای <a> tags: فقط لاگ کن و validate (navigation طبیعی)
            // ✅ CRITICAL FIX: حذف handler برای <a> tags تا navigation طبیعی انجام شود
            // مشکل: handler ممکن است navigation را intercept کند
            // راه‌حل: فقط برای <button> tags handler بزنیم
            // $(document).on('click', 'a.select-doctor-btn', ...) // ❌ حذف شد - باعث interference می‌شد
            
            log('✅ [DoctorSelection] Event handlers bound');

            // جستجوی Real-time
            $('#searchInput').on('input', this.handleSearchInput.bind(this));
        },

        setupSearchDebounce: function () {
            let searchTimeout;
            this.searchTimeout = searchTimeout;
        },

        handleSelectDoctor: function (e) {
            log('🔵 [DoctorSelection] handleSelectDoctor called');
            log('🔵 [DoctorSelection] Event:', e);
            log('🔵 [DoctorSelection] Current Target:', e.currentTarget);
            log('🔵 [DoctorSelection] Tag Name:', e.currentTarget.tagName);
            
            const $btn = $(e.currentTarget);
            const doctorId = $btn.data('doctor-id') || $btn.attr('data-doctor-id');
            
            log('🔵 [DoctorSelection] DoctorId from data attribute:', doctorId);
            log('🔵 [DoctorSelection] Button disabled?', $btn.prop('disabled'));
            log('🔵 [DoctorSelection] Button classes:', $btn.attr('class'));
            log('🔵 [DoctorSelection] Button href (if <a>):', $btn.attr('href'));
            
            if (!doctorId) {
                error('❌ [DoctorSelection] DoctorId is missing!');
                error('❌ [DoctorSelection] Button HTML:', $btn[0].outerHTML);
                e.preventDefault();
                e.stopPropagation();
                this.showError('شناسه پزشک نامعتبر است');
                return false;
            }

            // بررسی دسترسی‌پذیری
            if ($btn.prop('disabled')) {
                warn('⚠️ [DoctorSelection] Button is disabled');
                e.preventDefault();
                e.stopPropagation();
                this.showError('این پزشک در حال حاضر در دسترس نیست');
                return false;
            }

            // ✅ If it's an <a> tag with href, verify URL and navigate
            if ($btn.is('a') && $btn.attr('href')) {
                const href = $btn.attr('href');
                log('🔵 [DoctorSelection] <a> tag detected with href:', href);
                log('🔵 [DoctorSelection] data-href:', $btn.data('href'));
                
                // ✅ Verify href is valid
                if (!href || href === '#' || href === '') {
                    error('❌ [DoctorSelection] Invalid href:', href);
                    e.preventDefault();
                    e.stopPropagation();
                    this.showError('لینک نامعتبر است. لطفاً صفحه را refresh کنید.');
                    return false;
                }
                
                // ✅ Verify href contains doctorId
                if (!href.includes(doctorId.toString())) {
                    warn('⚠️ [DoctorSelection] href does not contain doctorId:', href, 'doctorId:', doctorId);
                }
                
                // ✅ Verify href is not pointing to Home
                if (href.includes('/Home') || href === '/' || href === '/Patient/') {
                    error('❌ [DoctorSelection] href is pointing to Home! This is wrong!', href);
                    e.preventDefault();
                    e.stopPropagation();
                    this.showError('لینک نامعتبر است. لطفاً صفحه را refresh کنید.');
                    return false;
                }
                
                log('🔵 [DoctorSelection] Allowing natural navigation to:', href);
                // ✅ Don't prevent default - let the link work naturally
                // But log for debugging
                log('🔵 [DoctorSelection] Navigation will proceed to:', href);
                return true; // Allow default behavior
            }

            // ✅ For <button> tags, prevent default and navigate manually
            e.preventDefault();
            e.stopPropagation();
            
            // ✅ هدایت به صفحه انتخاب تاریخ (طبق route: Patient/Appointment/Book/SelectDate/{doctorId})
            const targetUrl = `/Patient/Appointment/Book/SelectDate/${doctorId}`;
            log('🔵 [DoctorSelection] Redirecting to:', targetUrl);
            log('🔵 [DoctorSelection] Full URL:', window.location.origin + targetUrl);
            
            // ✅ Use window.location.assign instead of href for better debugging
            window.location.assign(targetUrl);
            return false;
        },

        handleSearchInput: function (e) {
            const searchTerm = $(e.target).val();
            
            // Debounce برای جلوگیری از درخواست‌های زیاد
            clearTimeout(this.searchTimeout);
            
            this.searchTimeout = setTimeout(() => {
                if (searchTerm.length >= 2 || searchTerm.length === 0) {
                    this.performSearch(searchTerm);
                }
            }, 500);
        },

        performSearch: function (searchTerm) {
            const departmentId = $('#departmentFilter').val();
            
            this._showLoading();
            
            // ✅ CRITICAL FIX: بهبود Error Handling با Retry Logic و Timeout
            this.ajaxWithRetry({
                url: '/Patient/Api/DoctorSearch/GetAvailableDoctors',
                type: 'GET',
                data: {
                    departmentId: departmentId || null,
                    searchTerm: searchTerm || null
                },
                timeout: 30000, // ✅ 30 ثانیه Timeout
                maxRetries: 3, // ✅ حداکثر 3 بار تلاش
                retryDelay: 1000, // ✅ 1 ثانیه تاخیر
                onSuccess: (response) => {
                    this._hideLoading();
                    if (response.success && response.data) {
                        this.renderDoctors(response.data);
                    } else {
                        this.showError(response.message || 'خطا در دریافت لیست پزشکان');
                    }
                },
                onError: (xhr, status, error) => {
                    this._hideLoading();
                    let errorMessage = 'خطا در ارتباط با سرور';
                    
                    // ✅ تشخیص نوع خطا و نمایش پیام مناسب
                    if (status === 'timeout') {
                        errorMessage = 'زمان اتصال به سرور به پایان رسید. لطفاً اتصال اینترنت خود را بررسی کنید و دوباره تلاش کنید.';
                    } else if (status === 'error' && xhr.status === 0) {
                        errorMessage = 'خطا در اتصال به سرور. لطفاً اتصال اینترنت خود را بررسی کنید.';
                    } else if (xhr.status >= 500) {
                        errorMessage = 'خطای سرور. لطفاً چند لحظه صبر کنید و دوباره تلاش کنید.';
                    }
                    
                    this.showError(errorMessage);
                    console.error('❌ [DoctorSelection] AJAX Error:', { status, error, xhr });
                }
            });
        },

        renderDoctors: function (doctors) {
            const $container = $('#doctorsContainer');
            $container.empty();

            if (doctors.length === 0) {
                $container.html(`
                    <div class="empty-state">
                        <i class="fas fa-user-md"></i>
                        <h4>پزشکی یافت نشد</h4>
                        <p class="text-muted">لطفاً فیلترهای جستجو را تغییر دهید</p>
                    </div>
                `);
                return;
            }

            // TODO: استفاده از Partial View یا Template Engine
            doctors.forEach(doctor => {
                const doctorCard = this.createDoctorCard(doctor);
                $container.append(doctorCard);
            });
        },

        createDoctorCard: function (doctor) {
            // ✅ Support both PascalCase (API default) and camelCase
            const doctorId = doctor.doctorId ?? doctor.DoctorId ?? 0;
            const fullName = (doctor.fullName ?? doctor.FullName ?? '').toString();
            const specialization = (doctor.specialization ?? doctor.Specialization ?? 'نامشخص').toString();
            const scheduleInfo = (doctor.scheduleInfo ?? doctor.ScheduleInfo ?? '').toString();
            const hasActiveSchedule = doctor.hasActiveSchedule ?? doctor.HasActiveSchedule ?? false;
            // ✅ Escape for safe HTML (prevent XSS)
            const escape = (s) => {
                const div = document.createElement('div');
                div.textContent = s;
                return div.innerHTML;
            };

            const availabilityBadge = hasActiveSchedule
                ? `<span class="badge bg-success mb-2"><i class="fas fa-check-circle me-1"></i> در دسترس</span>`
                : `<span class="badge bg-secondary mb-2"><i class="fas fa-times-circle me-1"></i> غیرفعال</span>`;

            const selectBtn = hasActiveSchedule
                ? `<button type="button" class="btn btn-primary w-100 select-doctor-btn" data-doctor-id="${doctorId}">
                    <i class="fas fa-calendar-plus me-1"></i> انتخاب پزشک
                   </button>`
                : `<button type="button" class="btn btn-secondary w-100" disabled>
                    <i class="fas fa-times-circle me-1"></i> غیرفعال
                   </button>`;

            return $(`
                <div class="doctor-card card mb-3" data-doctor-id="${doctorId}">
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-8">
                                <div class="d-flex align-items-center mb-2">
                                    <div class="doctor-avatar me-3">
                                        <i class="fas fa-user-md text-white fs-1"></i>
                                    </div>
                                    <div>
                                        <h5 class="card-title mb-1">${escape(fullName)}</h5>
                                        <p class="text-muted mb-1">
                                            <i class="fas fa-stethoscope me-1"></i>
                                            ${escape(specialization)}
                                        </p>
                                    </div>
                                </div>
                                <p class="text-info mb-0">
                                    <i class="fas fa-calendar-check me-1"></i>
                                    ${escape(scheduleInfo)}
                                </p>
                            </div>
                            <div class="col-md-4 text-end d-flex flex-column justify-content-between">
                                <div>${availabilityBadge}</div>
                                ${selectBtn}
                            </div>
                        </div>
                    </div>
                </div>
            `);
        },

        /**
         * ✅ CRITICAL FIX: AJAX Helper با Retry Logic و Timeout Handling
         * طبق قراردادها: Bulletproof Error Handling
         */
        ajaxWithRetry: function (options) {
            const self = this;
            let retryCount = 0;
            const maxRetries = options.maxRetries || 3;
            const retryDelay = options.retryDelay || 1000;
            const timeout = options.timeout || 30000;

            function makeRequest() {
                $.ajax({
                    url: options.url,
                    type: options.type || 'GET',
                    data: options.data || {},
                    headers: options.headers || {},
                    timeout: timeout,
                    success: function (response) {
                        if (options.onSuccess) {
                            options.onSuccess(response);
                        }
                    },
                    error: function (xhr, status, error) {
                        // ✅ تشخیص نوع خطا
                        const isNetworkError = status === 'timeout' || 
                                             status === 'error' && xhr.status === 0 ||
                                             status === 'abort';
                        
                        const isServerError = xhr.status >= 500;

                        // ✅ Retry Logic برای Network Errors و Server Errors
                        if (retryCount < maxRetries && (isNetworkError || isServerError)) {
                            retryCount++;
                            console.warn(`⚠️ [DoctorSelection] Retry attempt ${retryCount}/${maxRetries} for ${options.url}`);
                            
                            // ✅ Exponential Backoff
                            const delay = retryDelay * Math.pow(2, retryCount - 1);
                            
                            setTimeout(function () {
                                makeRequest();
                            }, delay);
                        } else {
                            // ✅ تمام تلاش‌ها انجام شد یا خطای Client Error
                            if (options.onError) {
                                options.onError(xhr, status, error);
                            } else {
                                self.showError('خطا در ارتباط با سرور. لطفاً دوباره تلاش کنید.');
                            }
                        }
                    }
                });
            }

            makeRequest();
        },

        /** ✅ Fallback: استفاده از #loadingState اگر showLoading/hideLoading سراسری نباشند */
        _showLoading: function () {
            if (typeof showLoading === 'function') {
                showLoading();
            } else {
                var $el = $('#loadingState');
                if ($el.length) { $el.show(); }
            }
        },
        _hideLoading: function () {
            if (typeof hideLoading === 'function') {
                hideLoading();
            } else {
                var $el = $('#loadingState');
                if ($el.length) { $el.hide(); }
            }
        },

        showError: function (message) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'خطا',
                    text: message,
                    icon: 'error',
                    confirmButtonText: 'باشه',
                    confirmButtonColor: '#2c5aa0'
                });
            } else if (typeof toastr !== 'undefined') {
                toastr.error(message);
            } else {
                alert(message);
            }
        }
    };

    // Initialize on document ready
    $(document).ready(function () {
        log('🔵 [DoctorSelection] Document ready, initializing...');
        log('🔵 [DoctorSelection] jQuery version:', $.fn.jquery);
        
        DoctorSelection.init();
        
        // ✅ Re-check buttons after a short delay (in case they're rendered dynamically)
        setTimeout(function () {
            const $buttons = $('.select-doctor-btn');
            log(`🔵 [DoctorSelection] After delay: Found ${$buttons.length} select-doctor buttons`);
            
            $buttons.each(function (index) {
                const $btn = $(this);
                const doctorId = $btn.data('doctor-id');
                log(`🔵 [DoctorSelection] Button ${index + 1}: doctorId=${doctorId}, disabled=${$btn.prop('disabled')}`);
            });
        }, 500);
    });

    // Export for global access
    window.DoctorSelection = DoctorSelection;
    
    log('✅ [DoctorSelection] Module loaded');

})(jQuery);

