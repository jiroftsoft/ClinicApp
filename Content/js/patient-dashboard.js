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

    // ✅ PatientDashboard - Enterprise-Grade Module
    var PatientDashboard = {
        
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
            
            // ✅ Load Quick Stats first
            this.loadSection('quickStats').then(function() {
                // ✅ Then load other sections in parallel
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
                if (response && response.success && response.data) {
                    self.renderSection($container, section.partial, response.data);
                } else {
                    self.showError($container, response?.message || 'خطا در بارگذاری');
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
                // Check for appointments/receptions arrays
                if (data.Appointments && data.Appointments.length === 0) {
                    isEmpty = true;
                } else if (data.Receptions && data.Receptions.length === 0) {
                    isEmpty = true;
                }
            }

            if (isEmpty) {
                this.showEmpty($container);
                return;
            }

            // ✅ Render partial directly from data (client-side rendering)
            // Note: For server-side rendering, we would need a RenderPartial action
            // For now, we'll render the HTML directly using templates
            
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
                    '<a href="/Patient/Appointment/Details/' + apt.AppointmentId + '" ' +
                    'class="btn btn-sm btn-outline-primary" data-ajax="true">' +
                    'مشاهده جزئیات <i class="fas fa-chevron-left"></i>' +
                    '</a>' +
                    '</div>' +
                    '</div>';
            }
            html += '</div>';
            
            if (sectionData.HasMore) {
                html += '<div class="text-center mt-3">' +
                    '<a href="/Patient/Appointment/MyAppointments" ' +
                    'class="btn btn-sm btn-outline-primary" data-ajax="true">' +
                    'مشاهده همه نوبت‌ها <i class="fas fa-chevron-left"></i>' +
                    '</a>' +
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
                    '<a href="#" class="btn btn-sm btn-outline-primary">' +
                    'مشاهده همه پذیرش‌ها <i class="fas fa-chevron-left"></i>' +
                    '</a>' +
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
         * ✅ Render Section (Updated)
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
                // Check for appointments/receptions arrays
                if (data.Appointments && data.Appointments.length === 0) {
                    isEmpty = true;
                } else if (data.Receptions && data.Receptions.length === 0) {
                    isEmpty = true;
                }
            }

            if (isEmpty) {
                this.showEmpty($container);
                return;
            }

            // ✅ Render HTML directly (client-side)
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
         * ✅ Reload Section
         */
        reloadSection: function(sectionName) {
            this.loadSection(sectionName);
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

