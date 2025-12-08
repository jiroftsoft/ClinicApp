/**
 * JavaScript Module برای انتخاب پزشک
 * رعایت SRP: فقط مدیریت جستجو و انتخاب پزشک
 */
(function ($) {
    'use strict';

    const DoctorSelection = {
        init: function () {
            this.bindEvents();
            this.setupSearchDebounce();
        },

        bindEvents: function () {
            // انتخاب پزشک
            $(document).on('click', '.select-doctor-btn', this.handleSelectDoctor.bind(this));

            // جستجوی Real-time
            $('#searchInput').on('input', this.handleSearchInput.bind(this));
        },

        setupSearchDebounce: function () {
            let searchTimeout;
            this.searchTimeout = searchTimeout;
        },

        handleSelectDoctor: function (e) {
            e.preventDefault();
            const doctorId = $(e.currentTarget).data('doctor-id');
            
            if (!doctorId) {
                this.showError('شناسه پزشک نامعتبر است');
                return;
            }

            // بررسی دسترسی‌پذیری
            const $btn = $(e.currentTarget);
            if ($btn.prop('disabled')) {
                this.showError('این پزشک در حال حاضر در دسترس نیست');
                return;
            }

            // هدایت به صفحه انتخاب تاریخ
            window.location.href = `/Patient/AppointmentBooking/SelectDate?doctorId=${doctorId}`;
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
        DoctorSelection.init();
    });

    // Export for global access
    window.DoctorSelection = DoctorSelection;

})(jQuery);

