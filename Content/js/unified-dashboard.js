/**
 * Unified Dashboard - Enterprise-Grade Tab-Based SPA
 * 
 * ✅ Features:
 * - Tab-based navigation (SPA experience)
 * - AJAX content loading
 * - URL history management (pushState)
 * - Smart caching
 * - Bulletproof error handling
 * - Form validation integration
 * 
 * Architecture: مثل AWS Console, Azure Portal
 */

(function($, window) {
    'use strict';

    // ✅ Configuration
    var config = {
        tabs: {
            overview: {
                name: 'overview',
                title: 'خانه',
                url: null, // Already loaded
                requiresAuth: true,
                cacheable: false
            },
            profile: {
                name: 'profile',
                title: 'پروفایل من',
                url: '/Patient/Dashboard/ProfileTab',
                requiresAuth: true,
                cacheable: true
            },
            appointments: {
                name: 'appointments',
                title: 'نوبت‌ها',
                url: '/Patient/Dashboard/AppointmentsTab',
                requiresAuth: true,
                cacheable: false
            },
            'medical-record': {
                name: 'medical-record',
                title: 'پرونده پزشکی',
                url: '/Patient/Dashboard/MedicalRecordTab',
                requiresAuth: true,
                cacheable: false
            },
            settings: {
                name: 'settings',
                title: 'تنظیمات',
                url: '/Patient/Dashboard/SettingsTab',
                requiresAuth: true,
                cacheable: true
            }
        },
        cache: {},
        currentTab: 'overview',
        historyEnabled: true
    };

    // ✅ Unified Dashboard Module
    var UnifiedDashboard = {
        
        /**
         * Initialize
         */
        init: function() {
            console.log('🚀 UnifiedDashboard: Initializing...');
            
            this.bindEvents();
            this.handleInitialTab();
            this.setupHistoryListener();
            
            console.log('✅ UnifiedDashboard: Initialized successfully');
        },

        /**
         * Bind Events
         */
        bindEvents: function() {
            var self = this;
            
            // Tab click events
            $('[data-bs-toggle="tab"]').on('shown.bs.tab', function(e) {
                var tabName = $(e.target).data('tab-name');
                console.log('📌 Tab switched to:', tabName);
                
                self.onTabShow(tabName);
            });
            
            // Quick appointment button
            $('#btnQuickAppointment').on('click', function() {
                window.location.href = '/Patient/Appointment/Book/SelectDoctor';
            });
        },

        /**
         * Handle Initial Tab (from URL hash)
         */
        handleInitialTab: function() {
            var hash = window.location.hash.replace('#', '');
            
            if (hash && config.tabs[hash]) {
                console.log('🔗 Loading tab from URL hash:', hash);
                this.switchTab(hash);
            } else {
                console.log('🏠 Loading default tab: overview');
                config.currentTab = 'overview';
            }
        },

        /**
         * Setup History Listener (Browser back/forward)
         */
        setupHistoryListener: function() {
            var self = this;
            
            if (!config.historyEnabled) return;
            
            window.addEventListener('popstate', function(event) {
                if (event.state && event.state.tab) {
                    console.log('⬅️ History: Back/Forward to tab:', event.state.tab);
                    self.switchTab(event.state.tab, true); // true = skip pushState
                }
            });
        },

        /**
         * Switch Tab
         */
        switchTab: function(tabName, skipHistory) {
            console.log('🔄 Switching to tab:', tabName);
            
            if (!config.tabs[tabName]) {
                console.error('❌ Invalid tab:', tabName);
                return;
            }
            
            // Update current tab
            config.currentTab = tabName;
            
            // Update URL (pushState)
            if (!skipHistory && config.historyEnabled) {
                var newUrl = window.location.pathname + '#' + tabName;
                window.history.pushState({ tab: tabName }, '', newUrl);
                console.log('🔗 URL updated:', newUrl);
            }
            
            // Activate tab (if not already active)
            var $tabButton = $('#tab-' + tabName);
            if (!$tabButton.hasClass('active')) {
                $tabButton.tab('show'); // This will trigger 'shown.bs.tab' event
            }
        },

        /**
         * On Tab Show (after tab is visible)
         */
        onTabShow: function(tabName) {
            var self = this;
            var tabConfig = config.tabs[tabName];
            
            if (!tabConfig) return;
            
            // Overview tab is already loaded
            if (tabName === 'overview') {
                return;
            }
            
            // Check cache
            if (tabConfig.cacheable && config.cache[tabName]) {
                console.log('💾 Loading from cache:', tabName);
                self.renderTabContent(tabName, config.cache[tabName]);
                return;
            }
            
            // Load content via AJAX
            if (tabConfig.url) {
                self.loadTabContent(tabName);
            }
        },

        /**
         * Load Tab Content (AJAX)
         */
        loadTabContent: function(tabName) {
            var self = this;
            var tabConfig = config.tabs[tabName];
            var $tabPane = $('#content-' + tabName);
            var $loading = $tabPane.find('.tab-loading');
            var $contentArea = $tabPane.find('.tab-content-area');
            
            console.log('📡 AJAX: Loading tab content:', tabName, tabConfig.url);
            
            // Show loading
            $loading.show();
            $contentArea.hide().html('');
            
            $.ajax({
                url: tabConfig.url,
                method: 'GET',
                dataType: 'html',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                },
                timeout: 30000
            })
            .done(function(html) {
                console.log('✅ AJAX Success:', tabName, html.length + ' bytes');
                
                // Cache if allowed
                if (tabConfig.cacheable) {
                    config.cache[tabName] = html;
                }
                
                // Render
                self.renderTabContent(tabName, html);
            })
            .fail(function(xhr, status, error) {
                console.error('❌ AJAX Error:', tabName, { status: xhr.status, error: error });
                
                // Show error
                var errorHtml = '<div class="alert alert-danger mt-3">' +
                    '<i class="fas fa-exclamation-triangle ml-2"></i>' +
                    '<strong>خطا در بارگذاری</strong><br>' +
                    '<small>' + (xhr.status === 401 ? 'لطفاً مجدداً وارد شوید' : 'لطفاً دوباره تلاش کنید') + '</small>' +
                    '<br><button class="btn btn-sm btn-outline-danger mt-2" onclick="UnifiedDashboard.reloadTab(\'' + tabName + '\')">تلاش مجدد</button>' +
                    '</div>';
                
                $loading.hide();
                $contentArea.html(errorHtml).fadeIn(300);
                
                // Redirect to login on 401
                if (xhr.status === 401) {
                    setTimeout(function() {
                        window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.href);
                    }, 2000);
                }
            });
        },

        /**
         * Render Tab Content
         */
        renderTabContent: function(tabName, html) {
            var $tabPane = $('#content-' + tabName);
            var $loading = $tabPane.find('.tab-loading');
            var $contentArea = $tabPane.find('.tab-content-area');
            
            console.log('🎨 Rendering tab content:', tabName);
            
            $loading.hide();
            $contentArea.html(html).fadeIn(300);
            
            // Initialize any forms/validation in the loaded content
            this.initializeTabContent(tabName);
        },

        /**
         * Initialize Tab Content (after loading)
         */
        initializeTabContent: function(tabName) {
            var $tabPane = $('#content-' + tabName);
            
            console.log('🔧 Initializing tab content:', tabName);
            
            // jQuery Validation
            $tabPane.find('form').each(function() {
                if ($.validator && $.validator.unobtrusive) {
                    $.validator.unobtrusive.parse(this);
                }
            });
            
            // AJAX Forms
            $tabPane.find('form[data-ajax="true"]').each(function() {
                $(this).on('submit', function(e) {
                    e.preventDefault();
                    UnifiedDashboard.handleFormSubmit(this);
                });
            });
            
            // Dispatch custom event
            $(document).trigger('unifiedDashboard:contentLoaded', { tab: tabName });
        },

        /**
         * Handle Form Submit (AJAX)
         */
        handleFormSubmit: function(form) {
            var $form = $(form);
            
            console.log('📤 Form submit:', $form.attr('action'));
            
            // Validate
            if ($form.valid && !$form.valid()) {
                console.warn('⚠️ Form validation failed');
                return false;
            }
            
            // Disable submit button
            var $submitBtn = $form.find('button[type="submit"]');
            var originalText = $submitBtn.html();
            $submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin ml-1"></i> در حال ارسال...');
            
            $.ajax({
                url: $form.attr('action'),
                method: $form.attr('method') || 'POST',
                data: $form.serialize(),
                dataType: 'json'
            })
            .done(function(response) {
                console.log('✅ Form submit success:', response);
                
                if (response.success) {
                    // Show success message
                    if (window.NotificationHelper) {
                        NotificationHelper.showSuccess(response.message || 'عملیات با موفقیت انجام شد');
                    }
                    
                    // Clear cache for this tab
                    var tabName = config.currentTab;
                    delete config.cache[tabName];
                    
                    // Reload tab if needed
                    if (response.reload) {
                        UnifiedDashboard.reloadTab(tabName);
                    }
                } else {
                    // Show error
                    if (window.NotificationHelper) {
                        NotificationHelper.showError(response.message || 'خطا در ثبت اطلاعات');
                    }
                }
            })
            .fail(function(xhr) {
                console.error('❌ Form submit error:', xhr.status);
                
                if (window.NotificationHelper) {
                    NotificationHelper.showError('خطا در ارتباط با سرور');
                }
            })
            .always(function() {
                // Re-enable submit button
                $submitBtn.prop('disabled', false).html(originalText);
            });
            
            return false;
        },

        /**
         * Reload Tab (clear cache & reload)
         */
        reloadTab: function(tabName) {
            console.log('🔄 Reloading tab:', tabName);
            
            // Clear cache
            delete config.cache[tabName];
            
            // Reload
            this.loadTabContent(tabName);
        },

        /**
         * Get Current Tab
         */
        getCurrentTab: function() {
            return config.currentTab;
        }
    };

    // ✅ Expose to global scope
    window.UnifiedDashboard = UnifiedDashboard;

})(jQuery, window);

