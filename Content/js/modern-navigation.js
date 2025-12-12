/**
 * 🧭 Modern Navigation JavaScript
 * 
 * مدیریت Navigation و MegaMenu
 * استفاده از Design System و اصول SRP
 */

(function() {
    'use strict';

    /**
     * Navigation Manager
     * مدیریت Navigation با رعایت SRP
     */
    const NavigationManager = {
        /**
         * Initialize Navigation
         */
        init: function() {
            console.log('🧭 Initializing Modern Navigation...');
            
            this.setupNavbar();
            this.setupMobileMenu();
            this.setupMegaMenu();
            this.setupScrollBehavior();
            this.setupActiveState();
            this.setupKeyboardNavigation();
            
            console.log('✅ Modern Navigation initialized successfully');
        },

        /**
         * Setup Navbar
         */
        setupNavbar: function() {
            const navbar = document.querySelector('.modern-navbar');
            if (!navbar) return;

            // Add scrolled class on scroll
            let lastScroll = 0;
            window.addEventListener('scroll', () => {
                const currentScroll = window.pageYOffset;
                
                if (currentScroll > 50) {
                    navbar.classList.add('scrolled');
                } else {
                    navbar.classList.remove('scrolled');
                }
                
                lastScroll = currentScroll;
            }, { passive: true });
        },

        /**
         * Setup Mobile Menu
         */
        setupMobileMenu: function() {
            const toggler = document.querySelector('.navbar-toggler-modern');
            const nav = document.querySelector('.navbar-nav-modern');
            
            if (!toggler || !nav) return;

            toggler.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                
                toggler.classList.toggle('active');
                nav.classList.toggle('active');
                
                // Close on outside click
                if (nav.classList.contains('active')) {
                    document.addEventListener('click', function closeMenu(e) {
                        if (!nav.contains(e.target) && !toggler.contains(e.target)) {
                            toggler.classList.remove('active');
                            nav.classList.remove('active');
                            document.removeEventListener('click', closeMenu);
                        }
                    });
                }
            });

            // Close menu on link click (mobile)
            const navLinks = nav.querySelectorAll('.nav-link-modern');
            navLinks.forEach(link => {
                link.addEventListener('click', () => {
                    if (window.innerWidth < 992) {
                        toggler.classList.remove('active');
                        nav.classList.remove('active');
                    }
                });
            });
        },

        /**
         * Setup MegaMenu
         */
        setupMegaMenu: function() {
            const dropdowns = document.querySelectorAll('.dropdown-modern');
            
            dropdowns.forEach(dropdown => {
                const toggle = dropdown.querySelector('.dropdown-toggle-modern');
                const menu = dropdown.querySelector('.megamenu-modern');
                
                if (!toggle || !menu) return;

                // Desktop: Hover to open
                if (window.innerWidth >= 992) {
                    dropdown.addEventListener('mouseenter', () => {
                        dropdown.classList.add('show');
                    });

                    dropdown.addEventListener('mouseleave', () => {
                        dropdown.classList.remove('show');
                    });
                }

                // Mobile: Click to toggle
                if (window.innerWidth < 992) {
                    toggle.addEventListener('click', (e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        
                        // Close other dropdowns
                        dropdowns.forEach(other => {
                            if (other !== dropdown) {
                                other.classList.remove('show');
                            }
                        });
                        
                        dropdown.classList.toggle('show');
                    });
                }

                // Close on outside click
                document.addEventListener('click', (e) => {
                    if (!dropdown.contains(e.target)) {
                        dropdown.classList.remove('show');
                    }
                });
            });
        },

        /**
         * Setup Scroll Behavior
         */
        setupScrollBehavior: function() {
            // Smooth scroll for anchor links
            const anchorLinks = document.querySelectorAll('a[href^="#"]');
            
            anchorLinks.forEach(link => {
                link.addEventListener('click', (e) => {
                    const href = link.getAttribute('href');
                    if (href === '#' || href === '') return;
                    
                    const target = document.querySelector(href);
                    if (target) {
                        e.preventDefault();
                        target.scrollIntoView({
                            behavior: 'smooth',
                            block: 'start'
                        });
                    }
                });
            });
        },

        /**
         * Setup Active State
         */
        setupActiveState: function() {
            const currentPath = window.location.pathname;
            const navLinks = document.querySelectorAll('.nav-link-modern');
            
            navLinks.forEach(link => {
                const href = link.getAttribute('href');
                if (href && currentPath.includes(href.split('/').pop())) {
                    link.classList.add('active');
                } else {
                    link.classList.remove('active');
                }
            });
        },

        /**
         * Setup Keyboard Navigation
         */
        setupKeyboardNavigation: function() {
            const nav = document.querySelector('.navbar-nav-modern');
            if (!nav) return;

            const focusableElements = nav.querySelectorAll('a, button, [tabindex]:not([tabindex="-1"])');
            const firstElement = focusableElements[0];
            const lastElement = focusableElements[focusableElements.length - 1];

            nav.addEventListener('keydown', (e) => {
                // Tab: Move to next element
                if (e.key === 'Tab') {
                    if (e.shiftKey) {
                        // Shift + Tab: Move to previous element
                        if (document.activeElement === firstElement) {
                            e.preventDefault();
                            lastElement.focus();
                        }
                    } else {
                        // Tab: Move to next element
                        if (document.activeElement === lastElement) {
                            e.preventDefault();
                            firstElement.focus();
                        }
                    }
                }

                // Escape: Close mobile menu
                if (e.key === 'Escape') {
                    const toggler = document.querySelector('.navbar-toggler-modern');
                    const nav = document.querySelector('.navbar-nav-modern');
                    if (toggler && nav) {
                        toggler.classList.remove('active');
                        nav.classList.remove('active');
                    }
                }
            });
        }
    };

    /**
     * Initialize when DOM is ready
     */
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            NavigationManager.init();
        });
    } else {
        NavigationManager.init();
    }

    /**
     * Handle window resize
     */
    let resizeTimer;
    window.addEventListener('resize', () => {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(() => {
            // Reinitialize mobile menu on resize
            NavigationManager.setupMobileMenu();
            NavigationManager.setupMegaMenu();
        }, 250);
    });

})();

