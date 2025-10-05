/**
 * 🏥 Patient Insurance Management Library
 * کتابخانه مدیریت بیمه بیماران - قابل استفاده مجدد در تمامی ماژول‌ها
 */

(function ($) {
    'use strict';

    // تنظیمات پیش‌فرض
    const DEFAULT_CONFIG = {
        baseUrl: '/Admin/Insurance/PatientInsuranceManagement',
        toastrOptions: {
            closeButton: true,
            progressBar: true,
            timeOut: 5000,
            extendedTimeOut: 2000,
            positionClass: 'toast-top-right'
        },
        dateFormat: 'yyyy/MM/dd'
    };

    /**
     * کلاس اصلی مدیریت بیمه بیماران
     */
    class PatientInsuranceManager {
        constructor(config = {}) {
            this.config = $.extend({}, DEFAULT_CONFIG, config);
            this.init();
        }

        /**
         * مقداردهی اولیه
         */
        init() {
            this.setupEventHandlers();
            this.setupToastr();
        }

        /**
         * تنظیم Event Handlers
         */
        setupEventHandlers() {
            // Event delegation برای دکمه‌های مدیریت بیمه
            $(document).on('click', '[data-insurance-action]', (e) => {
                e.preventDefault();
                this.handleInsuranceAction($(e.currentTarget));
            });

            // Event delegation برای انتخاب بیمه
            $(document).on('change', '[data-insurance-plan]', (e) => {
                this.handleInsurancePlanChange($(e.currentTarget));
            });
        }

        /**
         * تنظیم Toastr
         */
        setupToastr() {
            if (typeof toastr !== 'undefined') {
                toastr.options = this.config.toastrOptions;
            }
        }

        /**
         * مدیریت عملیات بیمه
         */
        handleInsuranceAction($button) {
            const action = $button.data('insurance-action');
            const patientId = $button.data('patient-id');
            const insuranceType = $button.data('insurance-type');

            switch (action) {
                case 'select-primary':
                    this.selectPrimaryInsurance(patientId);
                    break;
                case 'select-supplementary':
                    this.selectSupplementaryInsurance(patientId);
                    break;
                case 'change-primary':
                    this.changePrimaryInsurance(patientId);
                    break;
                case 'change-supplementary':
                    this.changeSupplementaryInsurance(patientId);
                    break;
                case 'deactivate-primary':
                    this.deactivatePrimaryInsurance(patientId);
                    break;
                case 'deactivate-supplementary':
                    this.deactivateSupplementaryInsurance(patientId);
                    break;
                case 'get-status':
                    this.getPatientInsuranceStatus(patientId);
                    break;
                default:
                    console.warn('عملیات بیمه نامشخص:', action);
            }
        }

        /**
         * انتخاب بیمه پایه
         */
        async selectPrimaryInsurance(patientId) {
            try {
                this.showLoading('در حال انتخاب بیمه پایه...');

                const formData = this.getInsuranceFormData(patientId, 'primary');
                if (!formData) {
                    this.hideLoading();
                    return;
                }

                const response = await this.makeAjaxCall('SelectPrimaryInsurance', formData);
                this.handleInsuranceResponse(response, 'بیمه پایه');
            } catch (error) {
                this.handleError('خطا در انتخاب بیمه پایه', error);
            }
        }

        /**
         * انتخاب بیمه تکمیلی
         */
        async selectSupplementaryInsurance(patientId) {
            try {
                this.showLoading('در حال انتخاب بیمه تکمیلی...');

                const formData = this.getInsuranceFormData(patientId, 'supplementary');
                if (!formData) {
                    this.hideLoading();
                    return;
                }

                const response = await this.makeAjaxCall('SelectSupplementaryInsurance', formData);
                this.handleInsuranceResponse(response, 'بیمه تکمیلی');
            } catch (error) {
                this.handleError('خطا در انتخاب بیمه تکمیلی', error);
            }
        }

        /**
         * تغییر بیمه پایه
         */
        async changePrimaryInsurance(patientId) {
            try {
                this.showLoading('در حال تغییر بیمه پایه...');

                const formData = this.getInsuranceFormData(patientId, 'primary');
                if (!formData) {
                    this.hideLoading();
                    return;
                }

                const response = await this.makeAjaxCall('ChangePrimaryInsurance', formData);
                this.handleInsuranceResponse(response, 'بیمه پایه');
            } catch (error) {
                this.handleError('خطا در تغییر بیمه پایه', error);
            }
        }

        /**
         * تغییر بیمه تکمیلی
         */
        async changeSupplementaryInsurance(patientId) {
            try {
                this.showLoading('در حال تغییر بیمه تکمیلی...');

                const formData = this.getInsuranceFormData(patientId, 'supplementary');
                if (!formData) {
                    this.hideLoading();
                    return;
                }

                const response = await this.makeAjaxCall('ChangeSupplementaryInsurance', formData);
                this.handleInsuranceResponse(response, 'بیمه تکمیلی');
            } catch (error) {
                this.handleError('خطا در تغییر بیمه تکمیلی', error);
            }
        }

        /**
         * غیرفعال کردن بیمه پایه
         */
        async deactivatePrimaryInsurance(patientId) {
            try {
                this.showLoading('در حال غیرفعال کردن بیمه پایه...');

                const response = await this.makeAjaxCall('DeactivatePrimaryInsurance', { patientId });
                this.handleInsuranceResponse(response, 'بیمه پایه');
            } catch (error) {
                this.handleError('خطا در غیرفعال کردن بیمه پایه', error);
            }
        }

        /**
         * غیرفعال کردن بیمه تکمیلی
         */
        async deactivateSupplementaryInsurance(patientId) {
            try {
                this.showLoading('در حال غیرفعال کردن بیمه تکمیلی...');

                const response = await this.makeAjaxCall('DeactivateSupplementaryInsurance', { patientId });
                this.handleInsuranceResponse(response, 'بیمه تکمیلی');
            } catch (error) {
                this.handleError('خطا در غیرفعال کردن بیمه تکمیلی', error);
            }
        }

        /**
         * دریافت وضعیت بیمه بیمار
         */
        async getPatientInsuranceStatus(patientId) {
            try {
                this.showLoading('در حال دریافت وضعیت بیمه...');

                const response = await this.makeAjaxCall('GetPatientInsuranceStatus', { patientId });
                this.handleStatusResponse(response);
            } catch (error) {
                this.handleError('خطا در دریافت وضعیت بیمه', error);
            }
        }

        /**
         * دریافت داده‌های فرم بیمه
         */
        getInsuranceFormData(patientId, insuranceType) {
            const prefix = insuranceType === 'primary' ? 'Primary' : 'Supplementary';
            const planIdField = `#${prefix}InsurancePlanId`;
            const policyNumberField = `#${prefix}PolicyNumber`;
            const startDateField = `#${prefix}StartDate`;
            const endDateField = `#${prefix}EndDate`;

            const planId = $(planIdField).val();
            const policyNumber = $(policyNumberField).val();
            const startDate = $(startDateField).val();
            const endDate = $(endDateField).val();

            // اعتبارسنجی
            if (!planId) {
                this.showError('لطفاً طرح بیمه را انتخاب کنید');
                return null;
            }

            if (!policyNumber) {
                this.showError('لطفاً شماره بیمه‌نامه را وارد کنید');
                return null;
            }

            if (!startDate) {
                this.showError('لطفاً تاریخ شروع را وارد کنید');
                return null;
            }

            return {
                patientId: patientId,
                insurancePlanId: parseInt(planId),
                policyNumber: policyNumber,
                startDate: new Date(startDate),
                endDate: endDate ? new Date(endDate) : null
            };
        }

        /**
         * فراخوانی AJAX
         */
        async makeAjaxCall(action, data) {
            const url = `${this.config.baseUrl}/${action}`;
            
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: url,
                    type: 'POST',
                    data: data,
                    dataType: 'json',
                    success: (response) => {
                        resolve(response);
                    },
                    error: (xhr, status, error) => {
                        reject(new Error(`AJAX Error: ${error}`));
                    }
                });
            });
        }

        /**
         * مدیریت پاسخ بیمه
         */
        handleInsuranceResponse(response, insuranceType) {
            this.hideLoading();

            if (response.success) {
                this.showSuccess(response.message);
                this.updateInsuranceDisplay(response.data);
                this.triggerInsuranceChangeEvent(response.data);
            } else {
                this.showError(response.message);
                if (response.errors && response.errors.length > 0) {
                    response.errors.forEach(error => this.showError(error));
                }
            }
        }

        /**
         * مدیریت پاسخ وضعیت
         */
        handleStatusResponse(response) {
            this.hideLoading();

            if (response.success) {
                this.updateInsuranceStatusDisplay(response.data);
                this.triggerStatusUpdateEvent(response.data);
            } else {
                this.showError(response.message);
            }
        }

        /**
         * به‌روزرسانی نمایش بیمه
         */
        updateInsuranceDisplay(data) {
            // به‌روزرسانی نمایش بیمه در UI
            const insuranceType = data.insuranceType.toLowerCase();
            const container = $(`#${insuranceType}-insurance-display`);
            
            if (container.length) {
                container.html(`
                    <div class="insurance-info-card">
                        <h6>${data.insuranceName}</h6>
                        <p><strong>شماره بیمه:</strong> ${data.policyNumber}</p>
                        <p><strong>تاریخ شروع:</strong> ${data.startDate}</p>
                        <p><strong>تاریخ پایان:</strong> ${data.endDate || 'نامحدود'}</p>
                        <span class="badge badge-success">فعال</span>
                    </div>
                `);
            }
        }

        /**
         * به‌روزرسانی نمایش وضعیت بیمه
         */
        updateInsuranceStatusDisplay(data) {
            // به‌روزرسانی نمایش وضعیت در UI
            const statusContainer = $('#insurance-status-display');
            
            if (statusContainer.length) {
                let html = '<div class="insurance-status-summary">';
                
                if (data.hasPrimaryInsurance) {
                    html += `
                        <div class="insurance-status-item primary">
                            <i class="fas fa-shield-alt"></i>
                            <span>بیمه پایه: ${data.primaryInsurance.name}</span>
                            <span class="badge badge-success">فعال</span>
                        </div>
                    `;
                } else {
                    html += `
                        <div class="insurance-status-item primary inactive">
                            <i class="fas fa-shield-alt"></i>
                            <span>بیمه پایه: ندارد</span>
                            <span class="badge badge-warning">غیرفعال</span>
                        </div>
                    `;
                }

                if (data.hasSupplementaryInsurance) {
                    html += `
                        <div class="insurance-status-item supplementary">
                            <i class="fas fa-plus-circle"></i>
                            <span>بیمه تکمیلی: ${data.supplementaryInsurance.name}</span>
                            <span class="badge badge-success">فعال</span>
                        </div>
                    `;
                } else {
                    html += `
                        <div class="insurance-status-item supplementary inactive">
                            <i class="fas fa-plus-circle"></i>
                            <span>بیمه تکمیلی: ندارد</span>
                            <span class="badge badge-secondary">ندارد</span>
                        </div>
                    `;
                }

                html += '</div>';
                statusContainer.html(html);
            }
        }

        /**
         * مدیریت تغییر طرح بیمه
         */
        handleInsurancePlanChange($select) {
            const planId = $select.val();
            const insuranceType = $select.data('insurance-type');
            
            if (planId) {
                // بارگیری اطلاعات طرح بیمه
                this.loadInsurancePlanInfo(planId, insuranceType);
            }
        }

        /**
         * بارگیری اطلاعات طرح بیمه
         */
        async loadInsurancePlanInfo(planId, insuranceType) {
            try {
                // اینجا می‌توانید اطلاعات طرح بیمه را بارگیری کنید
                console.log(`بارگیری اطلاعات طرح بیمه: ${planId} (${insuranceType})`);
            } catch (error) {
                console.error('خطا در بارگیری اطلاعات طرح بیمه:', error);
            }
        }

        /**
         * نمایش پیام موفقیت
         */
        showSuccess(message) {
            if (typeof toastr !== 'undefined') {
                toastr.success(message);
            } else {
                alert('✅ ' + message);
            }
        }

        /**
         * نمایش پیام خطا
         */
        showError(message) {
            if (typeof toastr !== 'undefined') {
                toastr.error(message);
            } else {
                alert('❌ ' + message);
            }
        }

        /**
         * نمایش Loading
         */
        showLoading(message) {
            if (typeof toastr !== 'undefined') {
                toastr.info(message);
            }
        }

        /**
         * مخفی کردن Loading
         */
        hideLoading() {
            // پیاده‌سازی مخفی کردن loading
        }

        /**
         * مدیریت خطا
         */
        handleError(message, error) {
            console.error(message, error);
            this.hideLoading();
            this.showError(message);
        }

        /**
         * Trigger Event برای تغییر بیمه
         */
        triggerInsuranceChangeEvent(data) {
            $(document).trigger('insuranceChanged', [data]);
        }

        /**
         * Trigger Event برای به‌روزرسانی وضعیت
         */
        triggerStatusUpdateEvent(data) {
            $(document).trigger('insuranceStatusUpdated', [data]);
        }
    }

    // ایجاد instance پیش‌فرض
    window.PatientInsuranceManager = PatientInsuranceManager;

    // Auto-initialize اگر jQuery موجود باشد
    $(document).ready(function() {
        if (typeof window.patientInsuranceManager === 'undefined') {
            window.patientInsuranceManager = new PatientInsuranceManager();
        }
    });

})(jQuery);
