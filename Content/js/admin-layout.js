/**
 * Admin Layout JavaScript - کلینیک شفا
 * طراحی شده برای محیط‌های درمانی با رعایت استانداردهای پزشکی
 */

(function() {
    'use strict';

    // Configuration
    const CONFIG = {
        themeKey: 'admin-theme',
        defaultTheme: 'light',
        animationDuration: 250,
        toastDuration: 5000,
        scrollThreshold: 50,
        breakpoints: {
            mobile: 768,
            tablet: 1024,
            desktop: 1200
        }
    };

    // State Management
    const state = {
        theme: localStorage.getItem(CONFIG.themeKey) || CONFIG.defaultTheme,
        isScrolled: false,
        isMobileMenuOpen: false,
        notifications: []
    };

    // DOM Elements Cache
    const elements = {
        html: document.documentElement,
        body: document.body,
        header: document.querySelector('.header'),
        sidebar: document.querySelector('.sidebar'),
        themeToggle: document.querySelector('[data-theme-toggle]'),
        mobileMenuToggle: document.querySelector('[data-mobile-menu-toggle]'),
        searchForm: document.querySelector('[data-search-form]'),
        notificationsContainer: document.querySelector('[data-notifications]')
    };

    // Utility Functions
    const utils = {
        /**
         * Debounce function for performance optimization
         */
        debounce: function(func, wait) {
            let timeout;
            return function executedFunction(...args) {
                const later = () => {
                    clearTimeout(timeout);
                    func(...args);
                };
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
            };
        },

        /**
         * Throttle function for scroll events
         */
        throttle: function(func, limit) {
            let inThrottle;
            return function() {
                const args = arguments;
                const context = this;
                if (!inThrottle) {
                    func.apply(context, args);
                    inThrottle = true;
                    setTimeout(() => inThrottle = false, limit);
                }
            };
        },

        /**
         * Check if element is in viewport
         */
        isInViewport: function(element) {
            const rect = element.getBoundingClientRect();
            return (
                rect.top >= 0 &&
                rect.left >= 0 &&
                rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
                rect.right <= (window.innerWidth || document.documentElement.clientWidth)
            );
        },

        /**
         * Get device type
         */
        getDeviceType: function() {
            const width = window.innerWidth;
            if (width < CONFIG.breakpoints.mobile) return 'mobile';
            if (width < CONFIG.breakpoints.tablet) return 'tablet';
            return 'desktop';
        },

        /**
         * Format number with Persian digits
         */
        formatNumber: function(num) {
            const persianDigits = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
            return num.toString().replace(/\d/g, x => persianDigits[x]);
        },

        /**
         * Format date to Persian
         */
        formatDate: function(date) {
            const options = {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                weekday: 'long'
            };
            return new Intl.DateTimeFormat('fa-IR', options).format(date);
        }
    };

    // Theme Management
    const themeManager = {
        /**
         * Initialize theme
         */
        init: function() {
            this.applyTheme(state.theme);
            this.bindEvents();
        },

        /**
         * Apply theme to document
         */
        applyTheme: function(theme) {
            elements.html.setAttribute('data-theme', theme);
            state.theme = theme;
            localStorage.setItem(CONFIG.themeKey, theme);
            
            // Dispatch custom event
            document.dispatchEvent(new CustomEvent('themeChanged', {
                detail: { theme: theme }
            }));
        },

        /**
         * Toggle theme
         */
        toggle: function() {
            const newTheme = state.theme === 'light' ? 'dark' : 'light';
            this.applyTheme(newTheme);
        },

        /**
         * Bind theme events
         */
        bindEvents: function() {
            if (elements.themeToggle) {
                elements.themeToggle.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.toggle();
                });
            }

            // Listen for system theme changes
            if (window.matchMedia) {
                const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
                mediaQuery.addListener((e) => {
                    if (!localStorage.getItem(CONFIG.themeKey)) {
                        this.applyTheme(e.matches ? 'dark' : 'light');
                    }
                });
            }
        }
    };

    // Scroll Management
    const scrollManager = {
        /**
         * Initialize scroll management
         */
        init: function() {
            this.bindEvents();
            this.checkScroll();
        },

        /**
         * Check scroll position
         */
        checkScroll: function() {
            const scrolled = window.scrollY > CONFIG.scrollThreshold;
            if (scrolled !== state.isScrolled) {
                state.isScrolled = scrolled;
                this.updateHeader();
            }
        },

        /**
         * Update header appearance
         */
        updateHeader: function() {
            if (elements.header) {
                if (state.isScrolled) {
                    elements.header.classList.add('scrolled');
                } else {
                    elements.header.classList.remove('scrolled');
                }
            }
        },

        /**
         * Bind scroll events
         */
        bindEvents: function() {
            window.addEventListener('scroll', utils.throttle(() => {
                this.checkScroll();
            }, 16));
        }
    };

    // Navigation Management
    const navigationManager = {
        /**
         * Initialize navigation
         */
        init: function() {
            this.setActiveNavItem();
            this.bindEvents();
        },

        /**
         * Set active navigation item
         */
        setActiveNavItem: function() {
            const currentPath = window.location.pathname;
            const navItems = document.querySelectorAll('.nav-item');
            
            navItems.forEach(item => {
                const href = item.getAttribute('href');
                if (href && currentPath.includes(href.split('/').pop())) {
                    item.classList.add('active');
                } else {
                    item.classList.remove('active');
                }
            });
        },

        /**
         * Handle mobile menu
         */
        toggleMobileMenu: function() {
            state.isMobileMenuOpen = !state.isMobileMenuOpen;
            if (elements.sidebar) {
                elements.sidebar.classList.toggle('mobile-open', state.isMobileMenuOpen);
            }
            document.body.classList.toggle('mobile-menu-open', state.isMobileMenuOpen);
        },

        /**
         * Bind navigation events
         */
        bindEvents: function() {
            if (elements.mobileMenuToggle) {
                elements.mobileMenuToggle.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.toggleMobileMenu();
                });
            }

            // Close mobile menu on outside click
            document.addEventListener('click', (e) => {
                if (state.isMobileMenuOpen && 
                    !elements.sidebar?.contains(e.target) && 
                    !elements.mobileMenuToggle?.contains(e.target)) {
                    this.toggleMobileMenu();
                }
            });

            // Handle window resize
            window.addEventListener('resize', utils.debounce(() => {
                if (utils.getDeviceType() !== 'mobile' && state.isMobileMenuOpen) {
                    this.toggleMobileMenu();
                }
            }, 250));
        }
    };

    // Search Management
    const searchManager = {
        /**
         * Initialize search functionality
         */
        init: function() {
            this.bindEvents();
        },

        /**
         * Handle search form submission
         */
        handleSearch: function(e) {
            e.preventDefault();
            const formData = new FormData(e.target);
            const query = formData.get('q');
            
            if (query && query.trim().length > 0) {
                // Add loading state
                const submitButton = e.target.querySelector('[type="submit"]');
                if (submitButton) {
                    submitButton.disabled = true;
                    submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> جستجو...';
                }

                // Simulate search (replace with actual search logic)
                setTimeout(() => {
                    if (submitButton) {
                        submitButton.disabled = false;
                        submitButton.innerHTML = '<i class="fas fa-search"></i>';
                    }
                    notificationManager.show('جستجو انجام شد', 'info');
                }, 1000);
            }
        },

        /**
         * Bind search events
         */
        bindEvents: function() {
            if (elements.searchForm) {
                elements.searchForm.addEventListener('submit', (e) => {
                    this.handleSearch(e);
                });
            }
        }
    };

    // Notification Management
    const notificationManager = {
        /**
         * Show notification
         */
        show: function(message, type = 'info', duration = CONFIG.toastDuration) {
            const notification = this.createNotification(message, type);
            this.addNotification(notification);
            
            // Auto remove
            setTimeout(() => {
                this.removeNotification(notification);
            }, duration);

            return notification;
        },

        /**
         * Create notification element
         */
        createNotification: function(message, type) {
            const notification = document.createElement('div');
            notification.className = `notification notification-${type}`;
            notification.innerHTML = `
                <div class="notification-content">
                    <i class="fas fa-${this.getIcon(type)}"></i>
                    <span>${message}</span>
                </div>
                <button class="notification-close" aria-label="بستن">
                    <i class="fas fa-times"></i>
                </button>
            `;

            // Add close functionality
            const closeBtn = notification.querySelector('.notification-close');
            closeBtn.addEventListener('click', () => {
                this.removeNotification(notification);
            });

            return notification;
        },

        /**
         * Get icon for notification type
         */
        getIcon: function(type) {
            const icons = {
                success: 'check-circle',
                error: 'exclamation-circle',
                warning: 'exclamation-triangle',
                info: 'info-circle'
            };
            return icons[type] || 'info-circle';
        },

        /**
         * Add notification to container
         */
        addNotification: function(notification) {
            if (!elements.notificationsContainer) {
                this.createContainer();
            }
            
            elements.notificationsContainer.appendChild(notification);
            state.notifications.push(notification);

            // Trigger animation
            setTimeout(() => {
                notification.classList.add('show');
            }, 10);
        },

        /**
         * Remove notification
         */
        removeNotification: function(notification) {
            notification.classList.remove('show');
            notification.classList.add('hiding');
            
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.parentNode.removeChild(notification);
                }
                const index = state.notifications.indexOf(notification);
                if (index > -1) {
                    state.notifications.splice(index, 1);
                }
            }, 300);
        },

        /**
         * Create notifications container
         */
        createContainer: function() {
            const container = document.createElement('div');
            container.className = 'notifications-container';
            container.setAttribute('data-notifications', '');
            document.body.appendChild(container);
            elements.notificationsContainer = container;
        },

        /**
         * Clear all notifications
         */
        clearAll: function() {
            state.notifications.forEach(notification => {
                this.removeNotification(notification);
            });
        }
    };

    // Performance Monitoring
    const performanceManager = {
        /**
         * Initialize performance monitoring
         */
        init: function() {
            this.measurePageLoad();
            this.measureUserInteractions();
        },

        /**
         * Measure page load performance
         */
        measurePageLoad: function() {
            if ('performance' in window) {
                window.addEventListener('load', () => {
                    const perfData = performance.getEntriesByType('navigation')[0];
                    const loadTime = perfData.loadEventEnd - perfData.loadEventStart;
                    
                    // Log performance data
                    console.log('Page Load Time:', loadTime + 'ms');
                    
                    // Send to analytics if needed
                    if (loadTime > 3000) {
                        console.warn('Slow page load detected');
                    }
                });
            }
        },

        /**
         * Measure user interactions
         */
        measureUserInteractions: function() {
            let interactionCount = 0;
            const interactionEvents = ['click', 'input', 'scroll', 'keydown'];
            
            interactionEvents.forEach(eventType => {
                document.addEventListener(eventType, utils.throttle(() => {
                    interactionCount++;
                    
                    // Log interaction metrics
                    if (interactionCount % 10 === 0) {
                        console.log('User Interactions:', interactionCount);
                    }
                }, 1000));
            });
        }
    };

    // Accessibility Manager
    const accessibilityManager = {
        /**
         * Initialize accessibility features
         */
        init: function() {
            this.setupKeyboardNavigation();
            this.setupFocusManagement();
            this.setupScreenReaderSupport();
        },

        /**
         * Setup keyboard navigation
         */
        setupKeyboardNavigation: function() {
            document.addEventListener('keydown', (e) => {
                // Escape key to close modals/menus
                if (e.key === 'Escape') {
                    if (state.isMobileMenuOpen) {
                        navigationManager.toggleMobileMenu();
                    }
                    notificationManager.clearAll();
                }

                // Ctrl/Cmd + K for search
                if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                    e.preventDefault();
                    const searchInput = document.querySelector('[data-search-input]');
                    if (searchInput) {
                        searchInput.focus();
                    }
                }
            });
        },

        /**
         * Setup focus management
         */
        setupFocusManagement: function() {
            // Trap focus in modals
            const modals = document.querySelectorAll('[data-modal]');
            modals.forEach(modal => {
                const focusableElements = modal.querySelectorAll(
                    'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
                );
                
                const firstElement = focusableElements[0];
                const lastElement = focusableElements[focusableElements.length - 1];

                modal.addEventListener('keydown', (e) => {
                    if (e.key === 'Tab') {
                        if (e.shiftKey) {
                            if (document.activeElement === firstElement) {
                                e.preventDefault();
                                lastElement.focus();
                            }
                        } else {
                            if (document.activeElement === lastElement) {
                                e.preventDefault();
                                firstElement.focus();
                            }
                        }
                    }
                });
            });
        },

        /**
         * Setup screen reader support
         */
        setupScreenReaderSupport: function() {
            // Add ARIA labels
            const buttons = document.querySelectorAll('button:not([aria-label])');
            buttons.forEach(button => {
                if (button.textContent.trim()) {
                    button.setAttribute('aria-label', button.textContent.trim());
                }
            });

            // Add skip links
            const skipLink = document.createElement('a');
            skipLink.href = '#main-content';
            skipLink.textContent = 'پرش به محتوای اصلی';
            skipLink.className = 'skip-link sr-only';
            skipLink.setAttribute('aria-label', 'پرش به محتوای اصلی');
            document.body.insertBefore(skipLink, document.body.firstChild);
        }
    };

    // Internationalization
    const i18n = {
        /**
         * Get localized text
         */
        t: function(key, params = {}) {
            const translations = {
                'fa': {
                    'search.placeholder': 'جستجو...',
                    'search.button': 'جستجو',
                    'theme.light': 'روشن',
                    'theme.dark': 'تاریک',
                    'notification.close': 'بستن',
                    'menu.toggle': 'منو',
                    'loading': 'در حال بارگذاری...',
                    'error.general': 'خطایی رخ داده است',
                    'success.saved': 'با موفقیت ذخیره شد',
                    'confirm.delete': 'آیا از حذف اطمینان دارید؟'
                }
            };

            let text = translations.fa[key] || key;
            
            // Replace parameters
            Object.keys(params).forEach(param => {
                text = text.replace(`{${param}}`, params[param]);
            });

            return text;
        },

        /**
         * Format number with locale
         */
        formatNumber: function(num, locale = 'fa-IR') {
            return new Intl.NumberFormat(locale).format(num);
        },

        /**
         * Format date with locale
         */
        formatDate: function(date, options = {}, locale = 'fa-IR') {
            const defaultOptions = {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            };
            return new Intl.DateTimeFormat(locale, { ...defaultOptions, ...options }).format(date);
        }
    };

    // Error Handling
    const errorHandler = {
        /**
         * Initialize error handling
         */
        init: function() {
            this.setupGlobalErrorHandling();
            this.setupAjaxErrorHandling();
        },

        /**
         * Setup global error handling
         */
        setupGlobalErrorHandling: function() {
            window.addEventListener('error', (e) => {
                console.error('Global Error:', e.error);
                this.handleError(e.error);
            });

            window.addEventListener('unhandledrejection', (e) => {
                console.error('Unhandled Promise Rejection:', e.reason);
                this.handleError(e.reason);
            });
        },

        /**
         * Setup AJAX error handling
         */
        setupAjaxErrorHandling: function() {
            // Intercept fetch requests
            const originalFetch = window.fetch;
            window.fetch = function(...args) {
                return originalFetch.apply(this, args)
                    .then(response => {
                        if (!response.ok) {
                            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                        }
                        return response;
                    })
                    .catch(error => {
                        errorHandler.handleError(error);
                        throw error;
                    });
            };
        },

        /**
         * Handle error
         */
        handleError: function(error) {
            console.error('Error handled:', error);
            
            // Show user-friendly error message
            const message = error.message || i18n.t('error.general');
            notificationManager.show(message, 'error');
            
            // Send to error tracking service (if available)
            if (window.gtag) {
                window.gtag('event', 'exception', {
                    description: error.message,
                    fatal: false
                });
            }
        }
    };

    // Public API
    window.AdminLayout = {
        theme: themeManager,
        navigation: navigationManager,
        search: searchManager,
        notifications: notificationManager,
        performance: performanceManager,
        accessibility: accessibilityManager,
        i18n: i18n,
        utils: utils,
        state: state
    };

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    function init() {
        // Initialize all managers
        themeManager.init();
        scrollManager.init();
        navigationManager.init();
        searchManager.init();
        performanceManager.init();
        accessibilityManager.init();
        errorHandler.init();

        // Show welcome notification
        setTimeout(() => {
            notificationManager.show('خوش آمدید به پنل مدیریت کلینیک شفا', 'success');
        }, 1000);

        console.log('Admin Layout initialized successfully');
    }

})();
