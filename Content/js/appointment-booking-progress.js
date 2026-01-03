/**
 * ✅ ULTIMATE: Appointment Booking Progress Indicator
 * 
 * Features:
 * - Visual step indicator (1/4, 2/4, 3/4, 4/4)
 * - Breadcrumb navigation
 * - Progress bar animation
 * - Responsive design
 * - RTL support
 * 
 * Usage:
 * 1. Include this JS file in appointment booking pages
 * 2. Add data-booking-step attribute to body/main element
 * 3. Script automatically renders progress indicator
 * 
 * Example:
 * <body data-booking-step="1" data-booking-total="4">
 * 
 * طبق: APPOINTMENT_BOOKING_ROADMAP.md - Phase 2.1
 */

(function () {
    'use strict';

    // تنظیمات پیش‌فرض
    const config = {
        steps: [
            { id: 1, name: 'انتخاب پزشک', url: '/Patient/Appointment/Book/SelectDoctor', icon: 'fa-user-md' },
            { id: 2, name: 'انتخاب تاریخ', url: null, icon: 'fa-calendar-alt' },  // URL dynamic است
            { id: 3, name: 'انتخاب زمان', url: null, icon: 'fa-clock' },       // URL dynamic است
            { id: 4, name: 'تایید و پرداخت', url: null, icon: 'fa-check-circle' } // URL dynamic است
        ],
        containerSelector: '.appointment-progress-container',
        autoInit: true
    };

    // کلاس اصلی Progress Indicator
    class AppointmentProgress {
        constructor(currentStep, totalSteps) {
            this.currentStep = parseInt(currentStep) || 1;
            this.totalSteps = parseInt(totalSteps) || 4;
            this.container = null;
            this.init();
        }

        init() {
            this.createContainer();
            this.render();
            this.attachEvents();
        }

        createContainer() {
            // پیدا کردن container موجود یا ایجاد جدید
            this.container = document.querySelector(config.containerSelector);

            if (!this.container) {
                // ایجاد container در ابتدای محتوای صفحه
                this.container = document.createElement('div');
                this.container.className = 'appointment-progress-container';

                const mainContent = document.querySelector('.page-content') 
                    || document.querySelector('main') 
                    || document.body;

                if (mainContent.firstChild) {
                    mainContent.insertBefore(this.container, mainContent.firstChild);
                } else {
                    mainContent.appendChild(this.container);
                }
            }
        }

        render() {
            const progressPercent = ((this.currentStep - 1) / (this.totalSteps - 1)) * 100;

            const html = `
                <!-- Progress Bar -->
                <div class="appointment-progress-bar">
                    <div class="progress-track">
                        <div class="progress-fill" style="width: ${progressPercent}%"></div>
                    </div>
                </div>

                <!-- Breadcrumb Steps -->
                <div class="appointment-steps-breadcrumb">
                    ${this.renderSteps()}
                </div>

                <!-- Current Step Info -->
                <div class="current-step-info">
                    <span class="step-number">مرحله ${this.currentStep} از ${this.totalSteps}</span>
                    <span class="step-name">${config.steps[this.currentStep - 1].name}</span>
                </div>
            `;

            this.container.innerHTML = html;

            // انیمیشن برای progress bar
            setTimeout(() => {
                const progressFill = this.container.querySelector('.progress-fill');
                if (progressFill) {
                    progressFill.style.transition = 'width 0.6s ease-in-out';
                }
            }, 100);
        }

        renderSteps() {
            return config.steps.map((step, index) => {
                const stepNumber = index + 1;
                const isCompleted = stepNumber < this.currentStep;
                const isCurrent = stepNumber === this.currentStep;
                const isPending = stepNumber > this.currentStep;

                let statusClass = '';
                let statusIcon = '';

                if (isCompleted) {
                    statusClass = 'completed';
                    statusIcon = '<i class="fas fa-check"></i>';
                } else if (isCurrent) {
                    statusClass = 'current';
                    statusIcon = `<i class="fas ${step.icon}"></i>`;
                } else {
                    statusClass = 'pending';
                    statusIcon = `<i class="fas ${step.icon}"></i>`;
                }

                // اگر مرحله completed است و URL دارد، قابل کلیک است
                const isClickable = isCompleted && step.url;

                return `
                    <div class="breadcrumb-step ${statusClass} ${isClickable ? 'clickable' : ''}" 
                         data-step="${stepNumber}"
                         ${isClickable ? `data-url="${step.url}"` : ''}>
                        <div class="step-icon">
                            ${statusIcon}
                        </div>
                        <div class="step-label">
                            <span class="step-number-text">مرحله ${stepNumber}</span>
                            <span class="step-name-text">${step.name}</span>
                        </div>
                    </div>
                    ${stepNumber < this.totalSteps ? '<div class="step-connector"></div>' : ''}
                `;
            }).join('');
        }

        attachEvents() {
            // اضافه کردن رویداد کلیک برای مراحل completed
            const clickableSteps = this.container.querySelectorAll('.breadcrumb-step.clickable');

            clickableSteps.forEach(step => {
                step.addEventListener('click', (e) => {
                    const url = step.dataset.url;
                    if (url) {
                        window.location.href = url;
                    }
                });

                // اضافه کردن cursor pointer
                step.style.cursor = 'pointer';
            });
        }

        // متد برای به‌روزرسانی مرحله فعلی (برای استفاده در AJAX navigation)
        updateStep(newStep) {
            this.currentStep = parseInt(newStep);
            this.render();
            this.attachEvents();
        }
    }

    // ✅ Auto-initialization
    function autoInit() {
        // چک کردن data attribute روی body/main
        const body = document.body;
        const currentStep = body.dataset.bookingStep 
            || body.dataset.appointmentStep 
            || body.getAttribute('data-step');

        if (currentStep) {
            const totalSteps = body.dataset.bookingTotal || 4;
            window.appointmentProgress = new AppointmentProgress(currentStep, totalSteps);
        }
    }

    // اجرای خودکار پس از بارگذاری DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', autoInit);
    } else {
        autoInit();
    }

    // Export برای استفاده manual
    window.AppointmentProgress = AppointmentProgress;

})();

