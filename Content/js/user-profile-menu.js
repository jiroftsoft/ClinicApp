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

            // ✅ CRITICAL: Logout button handler MUST come FIRST
            // This ensures logout form submission is NOT intercepted by AJAX handlers
            $(document).off('click', '.user-menu-logout')
                       .on('click', '.user-menu-logout', function(e) {
                // ✅ EXPLICITLY allow form submission
                console.log('🚪 Logout button clicked - submitting form');
                
                // ✅ Get the form and submit it explicitly
                var $form = $(this).closest('form');
                if ($form.length > 0) {
                    console.log('✅ Found logout form, submitting...');
                    $form.submit();
                } else {
                    console.error('❌ Logout form not found!');
                }
                
                // ✅ CRITICAL: Stop event propagation to prevent other handlers
                e.stopPropagation();
                return false;
            });

            // ✅ Profile menu items - AJAX navigation (ONLY items with data-ajax="true" AND href)
            // This handler will NOT affect logout button because it's a <button>, not an <a>
            $(document).off('click', '.user-menu-item[data-ajax="true"][href]')
                       .on('click', '.user-menu-item[data-ajax="true"][href]', function(e) {
                e.preventDefault();
                var $this = $(this);
                var url = $this.attr('href');
                var menuText = $this.find('span').text() || $this.text();
                
                // ✅ CRITICAL: Validate URL before proceeding
                if (!url || url.trim() === '' || url === '#') {
                    console.error('🔴 AJAX link has invalid href:', url, 'Element:', this);
                    if (window.toastr) {
                        toastr.error('این گزینه در دسترس نیست.', '', { timeOut: 3000 });
                    }
                    return false;
                }
                
                // ✅ Skip "به زودی" items
                if ($this.find('.badge-new').length > 0) {
                    if (window.toastr) {
                        toastr.info('این بخش به زودی فعال خواهد شد.', '', { timeOut: 3000 });
                    }
                    return false;
                }

                console.log('🔗 Navigating to:', url, 'Title:', menuText);
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
                var $this = $(this);
                var url = $this.attr('href');
                var linkText = $this.text().trim() || $this.find('span').text().trim();
                
                // ✅ CRITICAL: Validate URL
                if (!url || url.trim() === '' || url === '#') {
                    console.error('🔴 AJAX link has invalid href:', url, 'Element:', this);
                    return false;
                }
                
                console.log('🔗 AJAX link clicked:', url, 'Title:', linkText);
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

            // ✅ CRITICAL: Validate URL before navigation
            if (!url || typeof url !== 'string' || url.trim() === '') {
                console.error('🔴 Invalid URL provided to navigateTo:', url);
                if (window.toastr) {
                    toastr.error('آدرس صفحه نامعتبر است.', '', { timeOut: 3000 });
                }
                return;
            }

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

            // ✅ CRITICAL: Validate URL parameter
            if (!url || typeof url !== 'string' || url.trim() === '') {
                console.error('🔴 Invalid URL provided to loadContent:', url);
                if (window.toastr) {
                    toastr.error('آدرس صفحه نامعتبر است.', '', { timeOut: 3000 });
                }
                return;
            }

            if ($container.length === 0) {
                // ✅ Fallback: redirect if container not found
                window.location.href = url;
                return;
            }

            // ✅ CRITICAL FIX: Add query parameter for bulletproof AJAX detection
            var ajaxUrl = url + (url.indexOf('?') > -1 ? '&' : '?') + 'ajax=1';
            
            // ✅ Check cache (use original URL as key)
            if (config.cacheEnabled && componentCache[url]) {
                var cached = componentCache[url];
                if (Date.now() - cached.timestamp < config.cacheTimeout) {
                    self.renderContent(cached.html, cached.title, pushState, url);
                    return;
                }
            }

            // ✅ Show loading
            if (showLoading) {
                $container.html(config.loadingOverlay);
            }

            // ✅ AJAX request with enhanced detection
            $.ajax({
                url: ajaxUrl,  // ✅ Use URL with ajax=1 parameter
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
                    
                    // ✅ Cache content (use original URL as key, not ajaxUrl)
                    if (config.cacheEnabled) {
                        componentCache[url] = {
                            html: html,
                            title: pageTitle,
                            timestamp: Date.now()
                        };
                    }

                    // ✅ Render content (pass original URL for history)
                    self.renderContent(html, pageTitle, pushState, url);

                    // ✅ Reinitialize components
                    self.reinitializeComponents($container);

                    // ✅ Scroll to top
                    $('html, body').animate({ scrollTop: 0 }, config.transitionDuration);

                    // ✅ Trigger custom event (use original URL)
                    $(window).trigger('contentLoaded', [url, html]);
                },
                error: function(xhr, status, error) {
                    console.error('🔴 AJAX Navigation Error:', {
                        url: url,
                        status: xhr.status,
                        statusText: xhr.statusText,
                        error: error,
                        responseText: xhr.responseText
                    });
                    
                    // ✅ CRITICAL: Try to parse JSON response for better error handling
                    var errorData = null;
                    try {
                        errorData = JSON.parse(xhr.responseText);
                    } catch (e) {
                        // Not JSON, ignore
                    }
                    
                    if (xhr.status === 401) {
                        // ✅ Unauthorized - redirect to login
                        console.warn('⚠️ Unauthorized access - redirecting to login');
                        
                        if (window.toastr) {
                            toastr.warning('لطفاً دوباره وارد شوید.', 'احراز هویت', { timeOut: 3000 });
                        }
                        
                        // ✅ Clear any stale auth state
                        if (window.sessionStorage) {
                            sessionStorage.removeItem('authToken');
                        }
                        
                        // ✅ Redirect to login after short delay
                        setTimeout(function() {
                            if (window.openLoginModal) {
                                window.openLoginModal(url);
                            } else {
                                window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(url);
                            }
                        }, 500);
                        
                    } else if (xhr.status === 404) {
                        // ✅ Not found
                        $container.html('<div class="alert alert-danger text-center p-5"><i class="fas fa-exclamation-triangle fa-3x mb-3"></i><h4>صفحه یافت نشد</h4><p>صفحه مورد نظر وجود ندارد.</p></div>');
                        
                        if (window.toastr) {
                            toastr.error('صفحه مورد نظر یافت نشد.', '', { timeOut: 5000 });
                        }
                        
                    } else if (xhr.status === 403) {
                        // ✅ Forbidden
                        $container.html('<div class="alert alert-danger text-center p-5"><i class="fas fa-ban fa-3x mb-3"></i><h4>دسترسی غیرمجاز</h4><p>شما مجوز دسترسی به این صفحه را ندارید.</p></div>');
                        
                        if (window.toastr) {
                            toastr.error('دسترسی غیرمجاز.', '', { timeOut: 5000 });
                        }
                        
                    } else {
                        // ✅ Other errors
                        var errorMessage = errorData && errorData.message ? errorData.message : 'خطا در بارگذاری صفحه';
                        
                        $container.html('<div class="alert alert-danger text-center p-5"><i class="fas fa-exclamation-circle fa-3x mb-3"></i><h4>خطا در بارگذاری</h4><p>' + errorMessage + '</p><button class="btn btn-primary mt-3" onclick="location.reload()">بازخوانی صفحه</button></div>');
                        
                        if (window.toastr) {
                            toastr.error(errorMessage, '', { timeOut: 5000 });
                        }
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
         * @param {string} originalUrl - Original URL (without ajax=1 parameter)
         */
        renderContent: function(html, title, pushState, originalUrl) {
            var $container = $(config.contentContainer);
            
            // ✅ CRITICAL FIX: Strip inline scripts BEFORE inserting HTML to prevent re-execution
            // Create a detached DOM element to parse and clean the HTML
            var $temp = $('<div>').html(html);
            var inlineScripts = $temp.find('script:not([src])');
            var scriptCount = inlineScripts.length;
            
            if (scriptCount > 0) {
                console.log('🧹 Stripping ' + scriptCount + ' inline script(s) before render');
                inlineScripts.remove();
            }
            
            // Get cleaned HTML
            var cleanedHtml = $temp.html();
            
            // ✅ Fade out current content
            $container.fadeOut(config.transitionDuration, function() {
                // ✅ Update content with cleaned HTML (no inline scripts)
                $container.html(cleanedHtml);
                
                // ✅ Update page title
                if (title) {
                    document.title = title + ' - کلینیک شفا جیرفت';
                }

                // ✅ Push to browser history (use original URL without ajax=1)
                if (pushState && window.history && window.history.pushState) {
                    var url = originalUrl || (window.location.pathname + window.location.search);
                    // Remove ajax=1 parameter if present
                    url = url.replace(/[?&]ajax=1/, '').replace(/\?$/, '');
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
            // ✅ Mark container as AJAX-loaded
            $container.attr('data-ajax-loaded', 'true');
            
            // ✅ Reinitialize UserProfileComponent if exists
            if (window.UserProfileComponent) {
                $container.find('[data-profile-component="true"]').each(function() {
                    var $component = $(this).closest('.row');
                    if ($component.length > 0) {
                        UserProfileComponent.init($component);
                    }
                });
            }

            // ✅ Reinitialize DataTables if exists (with error handling)
            try {
                if (typeof $.fn.DataTable !== 'undefined') {
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
                } else {
                    console.log('ℹ️ DataTables plugin not loaded - will initialize when plugin loads');
                }
            } catch (e) {
                console.error('Error initializing DataTables:', e);
            }

            // ✅ Reinitialize tooltips if exists
            if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
                $container.find('[data-bs-toggle="tooltip"]').each(function() {
                    try {
                        new bootstrap.Tooltip(this);
                    } catch (e) {
                        // Silently fail - tooltip not critical
                    }
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

            // ✅ CRITICAL FIX: Smart script handling to prevent redeclaration errors
            // Strategy: Only load external scripts; skip ALL inline scripts
            $container.find('script[src]').each(function() {
                var script = this;
                var src = script.src;
                
                // Check if script is already loaded in the main page
                if (!$('script[src="' + src + '"]').not(script).length) {
                    var newScript = document.createElement('script');
                    newScript.src = src;
                    newScript.async = false;  // Maintain execution order
                    console.log('📜 Loading external script:', src);
                    document.body.appendChild(newScript);
                } else {
                    console.log('⏭️ Script already loaded:', src);
                }
            });

            // ✅ Remove ALL inline scripts to prevent re-execution and redeclaration errors
            // Inline scripts should be moved to external files or executed only once in the layout
            var inlineScriptCount = $container.find('script:not([src])').length;
            if (inlineScriptCount > 0) {
                console.log('🧹 Removing ' + inlineScriptCount + ' inline script(s) to prevent redeclaration');
                $container.find('script:not([src])').remove();
            }

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

