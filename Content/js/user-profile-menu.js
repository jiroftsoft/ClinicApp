/**
 * User Profile Menu - Enterprise-Grade AJAX Navigation System
 * 
 * ✅ Features:
 * - Fully AJAX-based navigation (no page refresh)
 * - API-First architecture
 * - Component loader for dynamic content
 * - Single Page Application (SPA) experience
 * - Lightning-fast navigation
 * - Component caching
 * 
 * Single Responsibility: مدیریت منوی پروفایل و navigation بدون رفرش صفحه
 * طبق: DEVELOPMENT_CONTRACT.md, AI_ASSISTANT_MASTER_CONTRACT.md
 */

(function($, window) {
    'use strict';

    // ✅ Configuration
    var config = {
        contentContainer: '#mainContent',
        loadingOverlay: '<div class="ajax-loading-overlay"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">در حال بارگذاری...</span></div></div>',
        cacheEnabled: true,
        cacheTimeout: 5 * 60 * 1000, // 5 minutes
        transitionDuration: 300
    };

    // ✅ Component Cache
    var componentCache = {};

    // ✅ UserProfileMenu - Enterprise-Grade Module
    var UserProfileMenu = {
        
        /**
         * ✅ Initialize Module
         */
        init: function() {
            this.bindEvents();
            this.initDropdown();
            this.setupAjaxNavigation();
        },

        /**
         * ✅ Bind Events
         */
        bindEvents: function() {
            var self = this;

            // ✅ Profile menu items - AJAX navigation
            $(document).off('click', '.user-menu-item[href]')
                       .on('click', '.user-menu-item[href]', function(e) {
                e.preventDefault();
                var url = $(this).attr('href');
                var menuText = $(this).find('span').text();
                
                // ✅ Skip "به زودی" items
                if ($(this).find('.badge-new').length > 0) {
                    if (window.toastr) {
                        toastr.info('این بخش به زودی فعال خواهد شد.', '', { timeOut: 3000 });
                    }
                    return false;
                }

                self.navigateTo(url, menuText);
            });

            // ✅ Close dropdown on outside click
            $(document).on('click', function(e) {
                if (!$(e.target).closest('.user-profile-menu').length) {
                    $('.user-profile-dropdown').removeClass('show');
                    $('.user-profile-toggle').attr('aria-expanded', 'false');
                }
            });
        },

        /**
         * ✅ Initialize Dropdown
         */
        initDropdown: function() {
            // ✅ Bootstrap dropdown initialization
            var dropdownElementList = [].slice.call(document.querySelectorAll('.user-profile-toggle'));
            dropdownElementList.map(function(dropdownToggleEl) {
                return new bootstrap.Dropdown(dropdownToggleEl);
            });
        },

        /**
         * ✅ Setup AJAX Navigation System
         */
        setupAjaxNavigation: function() {
            var self = this;

            // ✅ Intercept all internal links
            $(document).off('click', 'a[data-ajax="true"]')
                       .on('click', 'a[data-ajax="true"]', function(e) {
                e.preventDefault();
                var url = $(this).attr('href');
                var linkText = $(this).text().trim() || $(this).find('span').text().trim();
                self.navigateTo(url, linkText);
            });

            // ✅ Handle browser back/forward
            window.addEventListener('popstate', function(e) {
                if (e.state && e.state.url) {
                    self.loadContent(e.state.url, false, false);
                }
            });
        },

        /**
         * ✅ Navigate to URL (AJAX)
         * @param {string} url - URL to navigate
         * @param {string} title - Page title
         * @param {boolean} pushState - Push to browser history
         */
        navigateTo: function(url, title, pushState) {
            if (pushState === undefined) pushState = true;

            // ✅ Close dropdown
            $('.user-profile-dropdown').removeClass('show');
            $('.user-profile-toggle').attr('aria-expanded', 'false');

            // ✅ Load content
            this.loadContent(url, pushState, true, title);
        },

        /**
         * ✅ Load Content via AJAX
         * @param {string} url - URL to load
         * @param {boolean} pushState - Push to browser history
         * @param {boolean} showLoading - Show loading overlay
         * @param {string} title - Page title
         */
        loadContent: function(url, pushState, showLoading, title) {
            var self = this;
            var $container = $(config.contentContainer);

            if ($container.length === 0) {
                // ✅ Fallback: redirect if container not found
                window.location.href = url;
                return;
            }

            // ✅ Check cache
            if (config.cacheEnabled && componentCache[url]) {
                var cached = componentCache[url];
                if (Date.now() - cached.timestamp < config.cacheTimeout) {
                    self.renderContent(cached.html, cached.title, pushState);
                    return;
                }
            }

            // ✅ Show loading
            if (showLoading) {
                $container.html(config.loadingOverlay);
            }

            // ✅ AJAX request
            $.ajax({
                url: url,
                method: 'GET',
                dataType: 'html',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'X-AJAX-Request': 'true'
                },
                cache: false,
                timeout: 30000,
                success: function(html, textStatus, xhr) {
                    // ✅ Extract title from response
                    var pageTitle = title || self.extractTitle(html) || 'کلینیک شفا';
                    
                    // ✅ Cache content
                    if (config.cacheEnabled) {
                        componentCache[url] = {
                            html: html,
                            title: pageTitle,
                            timestamp: Date.now()
                        };
                    }

                    // ✅ Render content
                    self.renderContent(html, pageTitle, pushState);

                    // ✅ Reinitialize components
                    self.reinitializeComponents($container);

                    // ✅ Scroll to top
                    $('html, body').animate({ scrollTop: 0 }, config.transitionDuration);

                    // ✅ Trigger custom event
                    $(window).trigger('contentLoaded', [url, html]);
                },
                error: function(xhr, status, error) {
                    console.error('AJAX Navigation Error:', { url: url, status: status, error: error });
                    
                    if (xhr.status === 401) {
                        // ✅ Unauthorized - redirect to login
                        if (window.openLoginModal) {
                            window.openLoginModal(url);
                        } else {
                            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(url);
                        }
                    } else if (xhr.status === 404) {
                        // ✅ Not found
                        $container.html('<div class="alert alert-danger text-center p-5"><i class="fas fa-exclamation-triangle fa-3x mb-3"></i><h4>صفحه یافت نشد</h4><p>صفحه مورد نظر وجود ندارد.</p></div>');
                    } else {
                        // ✅ Other errors
                        $container.html('<div class="alert alert-danger text-center p-5"><i class="fas fa-exclamation-circle fa-3x mb-3"></i><h4>خطا در بارگذاری</h4><p>لطفاً دوباره تلاش کنید.</p><button class="btn btn-primary mt-3" onclick="location.reload()">بازخوانی صفحه</button></div>');
                    }

                    if (window.toastr) {
                        toastr.error('خطا در بارگذاری صفحه. لطفاً دوباره تلاش کنید.', '', { timeOut: 5000 });
                    }
                },
                complete: function() {
                    // ✅ Hide loading if still showing
                    $container.find('.ajax-loading-overlay').fadeOut(200, function() {
                        $(this).remove();
                    });
                }
            });
        },

        /**
         * ✅ Render Content
         * @param {string} html - HTML content
         * @param {string} title - Page title
         * @param {boolean} pushState - Push to browser history
         */
        renderContent: function(html, title, pushState) {
            var $container = $(config.contentContainer);
            
            // ✅ Fade out current content
            $container.fadeOut(config.transitionDuration, function() {
                // ✅ Update content
                $container.html(html);
                
                // ✅ Update page title
                if (title) {
                    document.title = title + ' - کلینیک شفا جیرفت';
                }

                // ✅ Push to browser history
                if (pushState && window.history && window.history.pushState) {
                    var url = window.location.pathname + window.location.search;
                    window.history.pushState({ url: url, title: title }, title, url);
                }

                // ✅ Fade in new content
                $container.fadeIn(config.transitionDuration);
            });
        },

        /**
         * ✅ Extract Title from HTML
         * @param {string} html - HTML content
         * @returns {string} Page title
         */
        extractTitle: function(html) {
            var match = html.match(/<title[^>]*>([^<]+)<\/title>/i);
            if (match && match[1]) {
                return match[1].replace(/\s*-\s*کلینیک شفا.*$/, '').trim();
            }
            return null;
        },

        /**
         * ✅ Reinitialize Components
         * @param {jQuery} $container - Container element
         */
        reinitializeComponents: function($container) {
            // ✅ Reinitialize UserProfileComponent if exists
            if (window.UserProfileComponent) {
                $container.find('[data-profile-component="true"]').each(function() {
                    var $component = $(this).closest('.row');
                    if ($component.length > 0) {
                        UserProfileComponent.init($component);
                    }
                });
            }

            // ✅ Reinitialize DataTables if exists
            if ($.fn.DataTable) {
                $container.find('table[data-datatable="true"]').each(function() {
                    if (!$.fn.DataTable.isDataTable(this)) {
                        $(this).DataTable({
                            language: {
                                url: '/Content/js/datatables-persian.json'
                            },
                            responsive: true
                        });
                    }
                });
            }

            // ✅ Reinitialize tooltips if exists
            if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
                $container.find('[data-bs-toggle="tooltip"]').each(function() {
                    new bootstrap.Tooltip(this);
                });
            }

            // ✅ Reinitialize modals if exists
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                $container.find('[data-bs-toggle="modal"]').each(function() {
                    // Modal will initialize on click
                });
            }

            // ✅ Reinitialize forms with AJAX
            $container.find('form[data-ajax="true"]').each(function() {
                var $form = $(this);
                $form.off('submit').on('submit', function(e) {
                    e.preventDefault();
                    // Form submission handled by respective modules
                });
            });

            // ✅ Reinitialize AJAX links
            $container.find('a[data-ajax="true"]').each(function() {
                // Already handled by setupAjaxNavigation
            });

            // ✅ Execute inline scripts
            $container.find('script').each(function() {
                var script = this;
                if (script.src) {
                    // External script - load if not already loaded
                    if (!$('script[src="' + script.src + '"]').length) {
                        var newScript = document.createElement('script');
                        newScript.src = script.src;
                        document.body.appendChild(newScript);
                    }
                } else {
                    // Inline script - execute
                    try {
                        eval(script.textContent || script.innerHTML);
                    } catch (e) {
                        console.error('Error executing inline script:', e);
                    }
                }
            });

            // ✅ Trigger custom event
            $(window).trigger('componentsReinitialized', [$container]);
        },

        /**
         * ✅ Clear Cache
         * @param {string} url - Specific URL to clear (optional)
         */
        clearCache: function(url) {
            if (url) {
                delete componentCache[url];
            } else {
                componentCache = {};
            }
        },

        /**
         * ✅ Preload Content
         * @param {string} url - URL to preload
         */
        preload: function(url) {
            if (!config.cacheEnabled) return;
            if (componentCache[url]) return; // Already cached

            $.ajax({
                url: url,
                method: 'GET',
                dataType: 'html',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'X-AJAX-Request': 'true'
                },
                cache: false,
                success: function(html) {
                    var title = UserProfileMenu.extractTitle(html) || 'کلینیک شفا';
                    componentCache[url] = {
                        html: html,
                        title: title,
                        timestamp: Date.now()
                    };
                }
            });
        }
    };

    // ✅ Initialize on document ready
    $(document).ready(function() {
        UserProfileMenu.init();
    });

    // ✅ Expose globally
    window.UserProfileMenu = UserProfileMenu;

})(jQuery, window);

