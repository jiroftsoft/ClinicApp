/**
 * Patient Dashboard - Enterprise-Grade AJAX Module
 * 
 * ✅ Features:
 * - Fully AJAX-based section loading (no page refresh)
 * - Component-based architecture
 * - Error handling & retry logic
 * - Loading states management
 * - Empty states handling
 * 
 * Single Responsibility: مدیریت بارگذاری و نمایش sections داشبورد بیمار
 * طبق: CLINICAPP_PATIENT_DASHBOARD_BEAST_ROADMAP_PROMPT.md
 */

(function($, window) {
    'use strict';

    // ✅ Configuration
    var config = {
        apiBaseUrl: '/Patient/Api/PatientDashboard',
        /** یک درخواست GetOverview به‌جای چهار درخواست جدا (فاز ۳.۳) */
        useOverview: true,
        overviewUrl: '/GetOverview',
        sections: {
            quickStats: {
                url: '/GetQuickStats',
                container: '#quickStatsContainer',
                partial: '_DashboardQuickStats'
            },
            recentAppointments: {
                url: '/GetRecentAppointments',
                container: '[data-dashboard-section="recentAppointments"]',
                partial: '_DashboardAppointmentsList'
            },
            upcomingAppointments: {
                url: '/GetUpcomingAppointments',
                container: '[data-dashboard-section="upcomingAppointments"]',
                partial: '_DashboardAppointmentsList'
            },
            recentReceptions: {
                url: '/GetRecentReceptions',
                container: '[data-dashboard-section="recentReceptions"]',
                partial: '_DashboardReceptionsList'
            }
        },
        defaultPageSize: 5,
        retryAttempts: 3,
        retryDelay: 2000
    };

    /** تعداد تلاش مجدد خودکار برای Overview (خطای بار اول رفع شود بدون نمایش به کاربر) */
    var _overviewRetryCount = 0;

    // ✅ PatientDashboard - Enterprise-Grade Module
    var PatientDashboard = {
        
        /**
         * ✅ Initialize Module
         * تأخیر کوتاه قبل از اولین درخواست تا از race با session/auth جلوگیری شود.
         */
        init: function() {
            var self = this;
            this.bindEvents();
            setTimeout(function() {
                self.loadAllSections();
            }, 200);
        },

        /**
         * ✅ Load All Sections — در صورت useOverview یک درخواست، وگرنه چهار درخواست جدا
         */
        loadAllSections: function() {
            var self = this;
            if (config.useOverview) {
                this.loadOverview();
                return;
            }
            this.loadSection('quickStats').then(function() {
                Promise.all([
                    self.loadSection('recentAppointments'),
                    self.loadSection('upcomingAppointments'),
                    self.loadSection('recentReceptions')
                ]).catch(function(error) {
                    console.error('Error loading dashboard sections:', error);
                });
            });
        },

        /**
         * ✅ یک درخواست GetOverview و پر کردن تمام sections (فاز ۳.۳)
         */
        loadOverview: function() {
            var self = this;
            var sections = config.sections;
            var $containers = [
                $(sections.quickStats.container),
                $(sections.recentAppointments.container),
                $(sections.upcomingAppointments.container),
                $(sections.recentReceptions.container)
            ];
            $containers.forEach(function($c) {
                if ($c.length) self.showLoading($c);
            });
            var url = config.apiBaseUrl + config.overviewUrl;
            $.ajax({
                url: url,
                method: 'GET',
                dataType: 'json',
                headers: { 'X-Requested-With': 'XMLHttpRequest', 'X-AJAX-Request': 'true' },
                cache: false,
                timeout: 30000
            }).then(function(response) {
                if (!response || !response.success || !response.data) {
                    var errMsg = response && response.message ? response.message : 'خطا در بارگذاری';
                    $containers.forEach(function($c) {
                        if ($c.length) {
                            self.hideLoading($c);
                            self.showError($c, errMsg);
                        }
                    });
                    _overviewRetryCount = 0;
                    return;
                }
                var d = response.data;
                var sectionErrors = d.SectionErrors || d.sectionErrors || {};
                var hasSectionErrors = sectionErrors && Object.keys(sectionErrors).length > 0;

                // ✅ تلاش مجدد خودکار یک بار در صورت خطای سکشن (رفع خطای بار اول بدون نمایش به کاربر)
                if (hasSectionErrors && _overviewRetryCount < 1) {
                    _overviewRetryCount++;
                    setTimeout(function() { self.loadOverview(); }, 500);
                    return;
                }
                if (!hasSectionErrors) _overviewRetryCount = 0;

                var $qs = $(sections.quickStats.container), $ra = $(sections.recentAppointments.container), $ua = $(sections.upcomingAppointments.container), $rr = $(sections.recentReceptions.container);
                // QuickStats — پشتیبانی از خطا و نمایش در همان ساختار کارت‌ها
                if ($qs.length) {
                    if (sectionErrors.QuickStats) {
                        self.hideLoading($qs);
                        self.showError($qs, sectionErrors.QuickStats);
                    } else {
                        self.renderSection($qs, sections.quickStats.partial, d.QuickStats || d.quickStats);
                    }
                }
                // RecentAppointments
                if ($ra.length) {
                    if (sectionErrors.RecentAppointments) { self.hideLoading($ra); self.showError($ra, sectionErrors.RecentAppointments); }
                    else self.renderSection($ra, sections.recentAppointments.partial, d.RecentAppointments || d.recentAppointments);
                }
                // UpcomingAppointments
                if ($ua.length) {
                    if (sectionErrors.UpcomingAppointments) { self.hideLoading($ua); self.showError($ua, sectionErrors.UpcomingAppointments); }
                    else self.renderSection($ua, sections.upcomingAppointments.partial, d.UpcomingAppointments || d.upcomingAppointments);
                }
                // RecentReceptions
                if ($rr.length) {
                    if (sectionErrors.RecentReceptions) { self.hideLoading($rr); self.showError($rr, sectionErrors.RecentReceptions); }
                    else self.renderSection($rr, sections.recentReceptions.partial, d.RecentReceptions || d.recentReceptions);
                }
            }).catch(function(xhr) {
                var msg = 'خطا در بارگذاری. لطفاً دوباره تلاش کنید.';
                if (xhr.status === 401) {
                    if (window.openLoginModal) window.openLoginModal(window.location.href);
                    else window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.href);
                    return;
                }
                _overviewRetryCount = 0;
                $containers.forEach(function($c) {
                    if ($c.length) {
                        self.hideLoading($c);
                        self.showError($c, msg);
                    }
                });
            });
        },

        /**
         * ✅ Load Section via AJAX
         * @param {string} sectionName - Name of the section
         * @param {object} params - Additional parameters (e.g., pageNumber, pageSize)
         */
        loadSection: function(sectionName, params) {
            var self = this;
            var section = config.sections[sectionName];
            
            if (!section) {
                console.error('Unknown section:', sectionName);
                return Promise.reject('Unknown section');
            }

            var $container = $(section.container);
            if ($container.length === 0) {
                console.warn('Container not found for section:', sectionName);
                return Promise.reject('Container not found');
            }

            // ✅ Show loading state
            this.showLoading($container);

            // ✅ Build URL
            var url = config.apiBaseUrl + section.url;
            if (params) {
                var queryString = $.param(params);
                if (queryString) {
                    url += '?' + queryString;
                }
            }

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
                console.log('✅ Dashboard AJAX Success:', sectionName, response);
                
                if (response && response.success) {
                    self.renderSection($container, section.partial, response.data);
                } else {
                    console.error('❌ Dashboard AJAX Failed:', sectionName, response);
                    self.showError($container, response?.message || 'خطا در بارگذاری');
                }
            }).catch(function(xhr, status, error) {
                console.error('❌ Dashboard AJAX Error:', sectionName, { 
                    status: xhr.status, 
                    statusText: xhr.statusText, 
                    responseText: xhr.responseText,
                    error: error 
                });
                
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
            
            if (!data) {
                isEmpty = true;
            } else if (Array.isArray(data)) {
                isEmpty = data.length === 0;
            } else if (typeof data === 'object') {
                // QuickStats always shows (even with 0 values)
                if (partialName === '_DashboardQuickStats') {
                    isEmpty = false;
                }
                // Check for appointments/receptions arrays
                else if (data.Appointments !== undefined) {
                    isEmpty = !data.Appointments || data.Appointments.length === 0;
                } else if (data.Receptions !== undefined) {
                    isEmpty = !data.Receptions || data.Receptions.length === 0;
                }
            }

            if (isEmpty) {
                this.showEmpty($container);
                return;
            }

            // ✅ Render HTML
            var html = self.renderPartialFromData(partialName, data);
            if (html) {
                var $content = $container.find('.dashboard-section-content');
                $content.html(html).fadeIn(300);
                self.hideEmpty($container);
                self.hideError($container);
            } else {
                self.showError($container, 'خطا در نمایش محتوا');
            }
        },

        /**
         * ✅ Render Partial from Data (Client-Side)
         * @param {string} partialName - Name of the partial
         * @param {object} data - Data to render
         */
        renderPartialFromData: function(partialName, data) {
            // ✅ For Quick Stats
            if (partialName === '_DashboardQuickStats' && data) {
                return this.renderQuickStats(data);
            }
            
            // ✅ For Appointments List
            if (partialName === '_DashboardAppointmentsList' && data && data.Appointments) {
                return this.renderAppointmentsList(data);
            }
            
            // ✅ For Receptions List
            if (partialName === '_DashboardReceptionsList' && data && data.Receptions) {
                return this.renderReceptionsList(data);
            }
            
            return null;
        },

        /**
         * ✅ Render Quick Stats HTML
         */
        renderQuickStats: function(stats) {
            return '<div class="row g-3">' +
                '<div class="col-6 col-md-3">' +
                '<div class="stat-card stat-card-primary">' +
                '<div class="stat-card-icon"><i class="fas fa-calendar-check"></i></div>' +
                '<div class="stat-card-content">' +
                '<div class="stat-card-value">' + (stats.TotalAppointments || 0) + '</div>' +
                '<div class="stat-card-label">کل نوبت‌ها</div>' +
                '</div></div></div>' +
                '<div class="col-6 col-md-3">' +
                '<div class="stat-card stat-card-success">' +
                '<div class="stat-card-icon"><i class="fas fa-calendar-alt"></i></div>' +
                '<div class="stat-card-content">' +
                '<div class="stat-card-value">' + (stats.UpcomingAppointments || 0) + '</div>' +
                '<div class="stat-card-label">نوبت‌های آینده</div>' +
                '</div></div></div>' +
                '<div class="col-6 col-md-3">' +
                '<div class="stat-card stat-card-info">' +
                '<div class="stat-card-icon"><i class="fas fa-check-circle"></i></div>' +
                '<div class="stat-card-content">' +
                '<div class="stat-card-value">' + (stats.CompletedAppointments || 0) + '</div>' +
                '<div class="stat-card-label">تکمیل شده</div>' +
                '</div></div></div>' +
                '<div class="col-6 col-md-3">' +
                '<div class="stat-card stat-card-warning">' +
                '<div class="stat-card-icon"><i class="fas fa-file-medical"></i></div>' +
                '<div class="stat-card-content">' +
                '<div class="stat-card-value">' + (stats.TotalReceptions || 0) + '</div>' +
                '<div class="stat-card-label">پذیرش‌ها</div>' +
                '</div></div></div>' +
                '</div>';
        },

        /**
         * ✅ Render Appointments List HTML
         */
        renderAppointmentsList: function(sectionData) {
            var appointments = sectionData.Appointments || [];
            if (appointments.length === 0) {
                return '';
            }
            
            var html = '<div class="appointments-list">';
            for (var i = 0; i < appointments.length; i++) {
                var apt = appointments[i];
                var statusClass = this.getStatusBadgeClass(apt.Status);
                html += '<div class="appointment-item">' +
                    '<div class="appointment-item-header">' +
                    '<div class="appointment-doctor">' +
                    '<i class="fas fa-user-md ml-2"></i>' +
                    '<strong>' + this.escapeHtml(apt.DoctorName || '') + '</strong>' +
                    '</div>' +
                    '<span class="appointment-status badge badge-' + statusClass + '">' +
                    this.escapeHtml(apt.StatusText || '') +
                    '</span>' +
                    '</div>' +
                    '<div class="appointment-item-body">' +
                    '<div class="appointment-date">' +
                    '<i class="fas fa-calendar ml-1"></i>' +
                    '<span>' + this.escapeHtml(apt.AppointmentDateShamsi || '') + '</span>' +
                    '</div>' +
                    '<div class="appointment-time">' +
                    '<i class="fas fa-clock ml-1"></i>' +
                    '<span>' + this.escapeHtml(apt.AppointmentTime || '') + '</span>' +
                    '</div>';
                
                if (apt.Price && apt.Price > 0) {
                    html += '<div class="appointment-price">' +
                        '<i class="fas fa-money-bill-wave ml-1"></i>' +
                        '<span>' + apt.Price.toLocaleString('fa-IR') + ' ریال</span>' +
                        '</div>';
                }
                
                html += '</div>' +
                    '<div class="appointment-item-footer">' +
                    '<button type="button" class="btn btn-sm btn-outline-primary dashboard-appointment-details-btn" ' +
                    'data-appointment-id="' + apt.AppointmentId + '">' +
                    'مشاهده جزئیات <i class="fas fa-chevron-left"></i>' +
                    '</button>' +
                    '</div>' +
                    '</div>';
            }
            html += '</div>';
            
            if (sectionData.HasMore) {
                html += '<div class="text-center mt-3">' +
                    '<button type="button" class="btn btn-sm btn-outline-primary btn-dashboard-view-all-appointments">' +
                    'مشاهده همه نوبت‌ها <i class="fas fa-chevron-left"></i>' +
                    '</button>' +
                    '</div>';
            }
            
            return html;
        },

        /**
         * ✅ Render Receptions List HTML
         */
        renderReceptionsList: function(sectionData) {
            var receptions = sectionData.Receptions || [];
            if (receptions.length === 0) {
                return '';
            }
            
            var html = '<div class="receptions-list">';
            for (var i = 0; i < receptions.length; i++) {
                var rec = receptions[i];
                var statusClass = this.getStatusBadgeClass(rec.Status);
                html += '<div class="reception-item">' +
                    '<div class="reception-item-header">' +
                    '<div class="reception-doctor">' +
                    '<i class="fas fa-user-md ml-2"></i>' +
                    '<strong>' + this.escapeHtml(rec.DoctorName || '') + '</strong>' +
                    '</div>' +
                    '<span class="reception-status badge badge-' + statusClass + '">' +
                    this.escapeHtml(rec.StatusText || '') +
                    '</span>' +
                    '</div>' +
                    '<div class="reception-item-body">' +
                    '<div class="reception-date">' +
                    '<i class="fas fa-calendar ml-1"></i>' +
                    '<span>' + this.escapeHtml(rec.ReceptionDateShamsi || '') + '</span>' +
                    '</div>';
                
                if (rec.TotalAmount && rec.TotalAmount > 0) {
                    html += '<div class="reception-amount">' +
                        '<i class="fas fa-money-bill-wave ml-1"></i>' +
                        '<span>' + rec.TotalAmount.toLocaleString('fa-IR') + ' ریال</span>' +
                        '</div>';
                }
                
                html += '</div></div>';
            }
            html += '</div>';
            
            if (sectionData.HasMore) {
                html += '<div class="text-center mt-3">' +
                    '<button type="button" class="btn btn-sm btn-outline-primary btn-dashboard-view-all-receptions">' +
                    'مشاهده همه پذیرش‌ها <i class="fas fa-chevron-left"></i>' +
                    '</button>' +
                    '</div>';
            }
            
            return html;
        },

        /**
         * ✅ Get Status Badge Class
         */
        getStatusBadgeClass: function(status) {
            if (!status) return 'secondary';
            var s = status.toUpperCase();
            if (s.includes('PENDING') || s.includes('در انتظار')) return 'warning';
            if (s.includes('CONFIRMED') || s.includes('تایید شده') || s.includes('INPROGRESS') || s.includes('در حال انجام')) return 'info';
            if (s.includes('COMPLETED') || s.includes('تکمیل شده')) return 'success';
            if (s.includes('CANCELLED') || s.includes('لغو شده')) return 'danger';
            return 'secondary';
        },

        /**
         * ✅ Escape HTML
         */
        escapeHtml: function(text) {
            if (!text) return '';
            var map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text.replace(/[&<>"']/g, function(m) { return map[m]; });
        },

        /**
         * ✅ Show Loading State
         */
        showLoading: function($container) {
            $container.find('.dashboard-section-loading').fadeIn(200);
            $container.find('.dashboard-section-content').hide();
            $container.find('.dashboard-section-empty').hide();
            $container.find('.dashboard-section-error').hide();
        },

        /**
         * ✅ Hide Loading State
         */
        hideLoading: function($container) {
            $container.find('.dashboard-section-loading').fadeOut(200);
        },

        /**
         * ✅ Show Empty State
         */
        showEmpty: function($container) {
            $container.find('.dashboard-section-empty').fadeIn(300);
            $container.find('.dashboard-section-content').hide();
            $container.find('.dashboard-section-error').hide();
        },

        /**
         * ✅ Hide Empty State
         */
        hideEmpty: function($container) {
            $container.find('.dashboard-section-empty').hide();
        },

        /**
         * ✅ Show Error State
         */
        showError: function($container, message) {
            var $errorContainer = $container.find('.dashboard-section-error');
            $errorContainer.find('.error-message').text(message || 'خطا در بارگذاری');
            $errorContainer.fadeIn(300);
            $container.find('.dashboard-section-content').hide();
            $container.find('.dashboard-section-empty').hide();
        },

        /**
         * ✅ Hide Error State
         */
        hideError: function($container) {
            $container.find('.dashboard-section-error').hide();
        },

        /**
         * ✅ Reload Section — در حالت useOverview کل Overview یک‌جا دوباره لود می‌شود (یک درخواست، تجربه یکپارچه).
         */
        reloadSection: function(sectionName) {
            if (config.useOverview) {
                _overviewRetryCount = 0;
                this.loadOverview();
            } else {
                this.loadSection(sectionName);
            }
        },

        /**
         * ✅ Bind Events
         */
        bindEvents: function() {
            var self = this;

            // ✅ Retry buttons
            $(document).on('click', '[onclick*="PatientDashboard.reloadSection"]', function(e) {
                e.preventDefault();
                var sectionName = $(this).closest('[data-dashboard-section]').attr('data-dashboard-section');
                if (sectionName) {
                    self.reloadSection(sectionName);
                }
            });

            // ✅ مشاهده جزئیات نوبت — فقط API، بدون رفرش، مودال
            $(document).on('click', '.dashboard-appointment-details-btn', function(e) {
                e.preventDefault();
                var id = $(this).data('appointment-id');
                if (!id) return;
                self.showAppointmentDetailsModal(id);
            });

            // ✅ مشاهده همه نوبت‌ها — تعویض تب
            $(document).on('click', '.btn-dashboard-view-all-appointments', function(e) {
                e.preventDefault();
                if (window.UnifiedDashboard && typeof window.UnifiedDashboard.switchTab === 'function') {
                    window.UnifiedDashboard.switchTab('appointments');
                } else {
                    window.location.href = '/Patient/Appointment/MyAppointments';
                }
            });

            // ✅ مشاهده همه پذیرش‌ها — تعویض به تب پرونده پزشکی
            $(document).on('click', '.btn-dashboard-view-all-receptions', function(e) {
                e.preventDefault();
                if (window.UnifiedDashboard && typeof window.UnifiedDashboard.switchTab === 'function') {
                    window.UnifiedDashboard.switchTab('medical-record');
                }
            });
        },

        /**
         * ✅ نمایش جزئیات نوبت در مودال — یکپارچه با تب نوبت‌ها: مدرن، جذاب، تاریخ شمسی، دکمه‌های اقدام
         * در صورت وجود PatientAppointments از همان مودال حرفه‌ای استفاده می‌شود.
         */
        showAppointmentDetailsModal: function(appointmentId) {
            var self = this;
            var $btn = $('.dashboard-appointment-details-btn[data-appointment-id="' + appointmentId + '"]');
            if ($btn.length) $btn.prop('disabled', true);
            $.ajax({
                url: '/Patient/Api/PatientAppointment/GetAppointmentDetails',
                method: 'GET',
                data: { id: appointmentId },
                dataType: 'json',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                cache: false
            }).then(function(response) {
                if ($btn.length) $btn.prop('disabled', false);
                if (!response || !response.success) {
                    self.showModalError(response && response.message ? response.message : 'خطا در دریافت جزئیات نوبت');
                    return;
                }
                if (window.PatientAppointments && typeof window.PatientAppointments.showAppointmentDetailsModal === 'function') {
                    window.PatientAppointments.showAppointmentDetailsModal(response.data);
                    return;
                }
                self._showAppointmentDetailsFallback(response.data);
            }).catch(function(xhr) {
                if ($btn.length) $btn.prop('disabled', false);
                var msg = (xhr.responseJSON && xhr.responseJSON.message) ? xhr.responseJSON.message : 'خطا در ارتباط با سرور';
                self.showModalError(msg);
            });
        },

        /** فرمت تاریخ API به شمسی خوانا — پشتیبانی از /Date(ticks)/ و ISO */
        _formatDateForModal: function(dateValue) {
            if (dateValue === undefined || dateValue === null || dateValue === '') return '—';
            var d = null;
            try {
                if (typeof dateValue === 'string') {
                    if (dateValue.indexOf('/Date(') === 0 && dateValue.indexOf(')/') !== -1) {
                        var tick = parseInt(dateValue.replace(/^\/Date\(/, '').replace(/\)\/$/, ''), 10);
                        if (!isNaN(tick)) d = new Date(tick);
                    } else if (dateValue.indexOf('T') !== -1 || /^\d{4}-\d{2}-\d{2}/.test(dateValue)) {
                        d = new Date(dateValue);
                    }
                } else if (typeof dateValue === 'number') {
                    d = new Date(dateValue);
                }
                if (d && !isNaN(d.getTime())) {
                    return d.toLocaleDateString('fa-IR', { year: 'numeric', month: 'long', day: 'numeric' });
                }
            } catch (e) { }
            return typeof dateValue === 'string' ? dateValue : '—';
        },

        /** مودال ساده (فقط وقتی PatientAppointments لود نشده) با تاریخ شمسی */
        _showAppointmentDetailsFallback: function(d) {
            var self = this;
            var g = function(k) { return d[k] !== undefined && d[k] !== null ? d[k] : (d[k.charAt(0).toLowerCase() + k.slice(1)] || '—'); };
            var dateStr = self._formatDateForModal(g('AppointmentDate'));
            var html = '<div class="appointment-details-modal text-right" dir="rtl">' +
                '<div class="apt-modal-section">' +
                '<div class="apt-modal-row"><span class="apt-modal-label">پزشک</span><span class="apt-modal-value">' + self.escapeHtml(g('DoctorName')) + '</span></div>' +
                '<div class="apt-modal-row"><span class="apt-modal-label">تخصص</span><span class="apt-modal-value">' + self.escapeHtml(g('DoctorSpecialization')) + '</span></div>' +
                '<div class="apt-modal-row"><span class="apt-modal-label">تاریخ</span><span class="apt-modal-value">' + self.escapeHtml(dateStr) + '</span></div>' +
                '<div class="apt-modal-row"><span class="apt-modal-label">زمان</span><span class="apt-modal-value">' + self.escapeHtml(g('AppointmentTime')) + '</span></div>' +
                '<div class="apt-modal-row"><span class="apt-modal-label">وضعیت</span><span class="apt-modal-value">' + self.escapeHtml(g('StatusDisplay')) + '</span></div>' +
                '<div class="apt-modal-row"><span class="apt-modal-label">مبلغ</span><span class="apt-modal-value">' + (d.Price != null ? Number(d.Price).toLocaleString('fa-IR') : '—') + ' تومان</span></div>' +
                '</div></div>';
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: '<i class="fas fa-calendar-check text-primary me-2"></i>جزئیات نوبت',
                    html: html,
                    showConfirmButton: true,
                    confirmButtonText: 'بستن',
                    showCloseButton: true,
                    width: 'min(92vw, 560px)',
                    customClass: { container: 'apt-details-swal', popup: 'apt-details-swal-popup', htmlContainer: 'apt-details-swal-html' }
                });
            } else {
                alert('پزشک: ' + g('DoctorName') + '\nتاریخ: ' + dateStr + '\nزمان: ' + g('AppointmentTime'));
            }
        },

        showModalError: function(message) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({ title: 'خطا', text: message || 'خطا در بارگذاری', icon: 'error', confirmButtonText: 'باشه' });
            } else {
                alert(message || 'خطا در بارگذاری');
            }
        }
    };

    // ✅ Initialize on document ready
    $(document).ready(function() {
        if ($('#patientDashboard').length > 0) {
            PatientDashboard.init();
        }
    });

    // ✅ Expose globally
    window.PatientDashboard = PatientDashboard;

})(jQuery, window);

