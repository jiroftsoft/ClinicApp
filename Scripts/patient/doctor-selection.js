/**
 * JavaScript Module برای انتخاب پزشک
 * رعایت SRP: فقط مدیریت جستجو و انتخاب پزشک
 */
(function ($) {
    'use strict';

    const DoctorSelection = {
        init: function () {
            console.log('🔵 [DoctorSelection] Initializing...');
            
            // ✅ Check if jQuery is loaded
            if (typeof jQuery === 'undefined') {
                console.error('❌ [DoctorSelection] jQuery is not loaded!');
                return;
            }
            console.log('✅ [DoctorSelection] jQuery loaded');

            this.bindEvents();
            this.setupSearchDebounce();
            
            // ✅ Verify buttons exist
            const $buttons = $('.select-doctor-btn');
            console.log(`🔵 [DoctorSelection] Found ${$buttons.length} select-doctor buttons`);
            
            if ($buttons.length === 0) {
                console.warn('⚠️ [DoctorSelection] No select-doctor buttons found! Buttons may be rendered after script loads.');
            }
        },

        bindEvents: function () {
            console.log('🔵 [DoctorSelection] Binding events...');
            
            // ✅ BEST PRACTICE: فقط برای <button> tags handler بزن (طبق قراردادها)
            // برای <a> tags، navigation طبیعی را اجازه بده (بدون interference)
            $(document).off('click', '.select-doctor-btn[type="button"]');
            $(document).on('click', '.select-doctor-btn[type="button"]', this.handleSelectDoctor.bind(this));
            
            // ✅ برای <a> tags: فقط لاگ کن (navigation طبیعی)
            $(document).on('click', 'a.select-doctor-btn', function(e) {
                const href = $(this).attr('href');
                const doctorId = $(this).data('doctor-id');
                console.log('🔵 [DoctorSelection] <a> tag clicked - href:', href, 'doctorId:', doctorId);
                
                // ✅ Validation: بررسی اینکه href به Home اشاره نمی‌کند
                if (href && (href === '/' || href === '/Patient/' || href.includes('/Home'))) {
                    console.error('❌ [DoctorSelection] Invalid href detected:', href);
                    e.preventDefault();
                    e.stopPropagation();
                    alert('لینک نامعتبر است. لطفاً صفحه را refresh کنید.');
                    return false;
                }
                
                console.log('✅ [DoctorSelection] Allowing natural navigation to:', href);
                // Don't prevent default - let it navigate naturally
                return true;
            });
            
            console.log('✅ [DoctorSelection] Event handlers bound');

            // جستجوی Real-time
            $('#searchInput').on('input', this.handleSearchInput.bind(this));
        },

        setupSearchDebounce: function () {
            let searchTimeout;
            this.searchTimeout = searchTimeout;
        },

        handleSelectDoctor: function (e) {
            console.log('🔵 [DoctorSelection] handleSelectDoctor called');
            console.log('🔵 [DoctorSelection] Event:', e);
            console.log('🔵 [DoctorSelection] Current Target:', e.currentTarget);
            console.log('🔵 [DoctorSelection] Tag Name:', e.currentTarget.tagName);
            
            const $btn = $(e.currentTarget);
            const doctorId = $btn.data('doctor-id') || $btn.attr('data-doctor-id');
            
            console.log('🔵 [DoctorSelection] DoctorId from data attribute:', doctorId);
            console.log('🔵 [DoctorSelection] Button disabled?', $btn.prop('disabled'));
            console.log('🔵 [DoctorSelection] Button classes:', $btn.attr('class'));
            console.log('🔵 [DoctorSelection] Button href (if <a>):', $btn.attr('href'));
            
            if (!doctorId) {
                console.error('❌ [DoctorSelection] DoctorId is missing!');
                console.error('❌ [DoctorSelection] Button HTML:', $btn[0].outerHTML);
                e.preventDefault();
                e.stopPropagation();
                this.showError('شناسه پزشک نامعتبر است');
                return false;
            }

            // بررسی دسترسی‌پذیری
            if ($btn.prop('disabled')) {
                console.warn('⚠️ [DoctorSelection] Button is disabled');
                e.preventDefault();
                e.stopPropagation();
                this.showError('این پزشک در حال حاضر در دسترس نیست');
                return false;
            }

            // ✅ If it's an <a> tag with href, verify URL and navigate
            if ($btn.is('a') && $btn.attr('href')) {
                const href = $btn.attr('href');
                console.log('🔵 [DoctorSelection] <a> tag detected with href:', href);
                console.log('🔵 [DoctorSelection] data-href:', $btn.data('href'));
                
                // ✅ Verify href is valid
                if (!href || href === '#' || href === '') {
                    console.error('❌ [DoctorSelection] Invalid href:', href);
                    e.preventDefault();
                    e.stopPropagation();
                    this.showError('لینک نامعتبر است. لطفاً صفحه را refresh کنید.');
                    return false;
                }
                
                // ✅ Verify href contains doctorId
                if (!href.includes(doctorId.toString())) {
                    console.warn('⚠️ [DoctorSelection] href does not contain doctorId:', href, 'doctorId:', doctorId);
                }
                
                // ✅ Verify href is not pointing to Home
                if (href.includes('/Home') || href === '/' || href === '/Patient/') {
                    console.error('❌ [DoctorSelection] href is pointing to Home! This is wrong!', href);
                    e.preventDefault();
                    e.stopPropagation();
                    this.showError('لینک نامعتبر است. لطفاً صفحه را refresh کنید.');
                    return false;
                }
                
                console.log('🔵 [DoctorSelection] Allowing natural navigation to:', href);
                // ✅ Don't prevent default - let the link work naturally
                // But log for debugging
                console.log('🔵 [DoctorSelection] Navigation will proceed to:', href);
                return true; // Allow default behavior
            }

            // ✅ For <button> tags, prevent default and navigate manually
            e.preventDefault();
            e.stopPropagation();
            
            // ✅ هدایت به صفحه انتخاب تاریخ (طبق route: Patient/Appointment/Book/SelectDate/{doctorId})
            const targetUrl = `/Patient/Appointment/Book/SelectDate/${doctorId}`;
            console.log('🔵 [DoctorSelection] Redirecting to:', targetUrl);
            console.log('🔵 [DoctorSelection] Full URL:', window.location.origin + targetUrl);
            
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
            
            showLoading();
            
            $.ajax({
                url: '/Patient/Api/DoctorSearch/GetAvailableDoctors',
                type: 'GET',
                data: {
                    departmentId: departmentId || null,
                    searchTerm: searchTerm || null
                },
                success: (response) => {
                    hideLoading();
                    if (response.success && response.data) {
                        this.renderDoctors(response.data);
                    } else {
                        this.showError(response.message || 'خطا در دریافت لیست پزشکان');
                    }
                },
                error: () => {
                    hideLoading();
                    this.showError('خطا در ارتباط با سرور');
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
            const availabilityBadge = doctor.hasActiveSchedule
                ? `<span class="badge bg-success mb-2"><i class="fas fa-check-circle me-1"></i> در دسترس</span>`
                : `<span class="badge bg-secondary mb-2"><i class="fas fa-times-circle me-1"></i> غیرفعال</span>`;

            const selectBtn = doctor.hasActiveSchedule
                ? `<button type="button" class="btn btn-primary w-100 select-doctor-btn" data-doctor-id="${doctor.doctorId}">
                    <i class="fas fa-calendar-plus me-1"></i> انتخاب پزشک
                   </button>`
                : `<button type="button" class="btn btn-secondary w-100" disabled>
                    <i class="fas fa-times-circle me-1"></i> غیرفعال
                   </button>`;

            return $(`
                <div class="doctor-card card mb-3" data-doctor-id="${doctor.doctorId}">
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-8">
                                <div class="d-flex align-items-center mb-2">
                                    <div class="doctor-avatar me-3">
                                        <i class="fas fa-user-md text-white fs-1"></i>
                                    </div>
                                    <div>
                                        <h5 class="card-title mb-1">${doctor.fullName}</h5>
                                        <p class="text-muted mb-1">
                                            <i class="fas fa-stethoscope me-1"></i>
                                            ${doctor.specialization}
                                        </p>
                                    </div>
                                </div>
                                <p class="text-info mb-0">
                                    <i class="fas fa-calendar-check me-1"></i>
                                    ${doctor.scheduleInfo}
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

        showError: function (message) {
            Swal.fire({
                title: 'خطا',
                text: message,
                icon: 'error',
                confirmButtonText: 'باشه'
            });
        }
    };

    // Initialize on document ready
    $(document).ready(function () {
        console.log('🔵 [DoctorSelection] Document ready, initializing...');
        console.log('🔵 [DoctorSelection] jQuery version:', $.fn.jquery);
        
        DoctorSelection.init();
        
        // ✅ Re-check buttons after a short delay (in case they're rendered dynamically)
        setTimeout(function () {
            const $buttons = $('.select-doctor-btn');
            console.log(`🔵 [DoctorSelection] After delay: Found ${$buttons.length} select-doctor buttons`);
            
            $buttons.each(function (index) {
                const $btn = $(this);
                const doctorId = $btn.data('doctor-id');
                console.log(`🔵 [DoctorSelection] Button ${index + 1}: doctorId=${doctorId}, disabled=${$btn.prop('disabled')}`);
            });
        }, 500);
    });

    // Export for global access
    window.DoctorSelection = DoctorSelection;
    
    console.log('✅ [DoctorSelection] Module loaded');

})(jQuery);

