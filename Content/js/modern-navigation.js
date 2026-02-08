/**
 * 🧭 Modern Navigation JavaScript
 *
 * منوی اصلی: ضد گلوله برای موبایل، تبلت و لپ‌تاپ
 * قفل اسکرول، backdrop، focus trap، بدون وابستگی به jQuery
 */

(function() {
    'use strict';

    const MOBILE_BREAKPOINT = 992;

    const NavigationManager = {
        toggler: null,
        nav: null,
        backdrop: null,

        init: function() {
            this.toggler = document.querySelector('.navbar-toggler-modern');
            this.nav = document.querySelector('.navbar-nav-modern');
            this.backdrop = document.getElementById('navbarBackdrop');

            if (!this.toggler || !this.nav) return;

            this.setupNavbar();
            this.setupMobileMenu();
            this.setupMegaMenu();
            this.setupScrollBehavior();
            this.setupActiveState();
            this.setupKeyboardNavigation();
            this.setupResize();
        },

        isMobile: function() {
            return window.innerWidth < MOBILE_BREAKPOINT;
        },

        /** بستن منوی موبایل و بازگردانی وضعیت */
        closeMobileMenu: function() {
            if (!this.toggler || !this.nav) return;
            this.toggler.classList.remove('active');
            this.toggler.setAttribute('aria-expanded', 'false');
            this.nav.classList.remove('active');
            document.documentElement.classList.remove('nav-mobile-open');
            document.body.classList.remove('nav-mobile-open');
            if (this.backdrop) {
                this.backdrop.setAttribute('aria-hidden', 'true');
            }
            this.toggler.focus();
        },

        /** باز کردن منوی موبایل و قفل اسکرول */
        openMobileMenu: function() {
            this.toggler.classList.add('active');
            this.toggler.setAttribute('aria-expanded', 'true');
            this.nav.classList.add('active');
            document.documentElement.classList.add('nav-mobile-open');
            document.body.classList.add('nav-mobile-open');
            if (this.backdrop) {
                this.backdrop.setAttribute('aria-hidden', 'false');
            }
            var firstFocusable = this.nav.querySelector('a[href], button');
            if (firstFocusable) {
                setTimeout(function() { firstFocusable.focus(); }, 100);
            }
        },

        setupNavbar: function() {
            var navbar = document.querySelector('.modern-navbar');
            if (!navbar) return;
            window.addEventListener('scroll', function() {
                navbar.classList.toggle('scrolled', window.pageYOffset > 50);
            }, { passive: true });
        },

        setupMobileMenu: function() {
            var self = this;

            this.toggler.setAttribute('aria-expanded', 'false');
            this.toggler.setAttribute('aria-controls', 'navbarNavModern');

            this.nav.setAttribute('id', 'navbarNavModern');

            this.toggler.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                if (self.nav.classList.contains('active')) {
                    self.closeMobileMenu();
                } else {
                    self.openMobileMenu();
                }
            });

            if (this.backdrop) {
                this.backdrop.addEventListener('click', function() {
                    if (self.isMobile()) self.closeMobileMenu();
                });
            }

            document.addEventListener('click', function(e) {
                if (!self.isMobile() || !self.nav.classList.contains('active')) return;
                if (!self.nav.contains(e.target) && !self.toggler.contains(e.target) && e.target !== self.backdrop) {
                    self.closeMobileMenu();
                }
            });

            var navLinks = this.nav.querySelectorAll('.nav-link-modern[href^="/"], .nav-link-modern[href^="http"], .megamenu-link-modern');
            for (var i = 0; i < navLinks.length; i++) {
                navLinks[i].addEventListener('click', function() {
                    if (self.isMobile()) self.closeMobileMenu();
                });
            }
        },

        setupMegaMenu: function() {
            var dropdowns = document.querySelectorAll('.dropdown-modern');
            var self = this;

            for (var i = 0; i < dropdowns.length; i++) {
                (function(dropdown) {
                    var toggle = dropdown.querySelector('.dropdown-toggle-modern');
                    var menu = dropdown.querySelector('.megamenu-modern');
                    if (!toggle || !menu) return;

                    dropdown.addEventListener('mouseenter', function() {
                        if (window.innerWidth >= MOBILE_BREAKPOINT) dropdown.classList.add('show');
                    });
                    dropdown.addEventListener('mouseleave', function() {
                        if (window.innerWidth >= MOBILE_BREAKPOINT) dropdown.classList.remove('show');
                    });

                    toggle.addEventListener('click', function(e) {
                        if (window.innerWidth >= MOBILE_BREAKPOINT) return;
                        e.preventDefault();
                        e.stopPropagation();
                        for (var j = 0; j < dropdowns.length; j++) {
                            if (dropdowns[j] !== dropdown) dropdowns[j].classList.remove('show');
                        }
                        dropdown.classList.toggle('show');
                    });

                    document.addEventListener('click', function(e) {
                        if (!dropdown.contains(e.target)) dropdown.classList.remove('show');
                    });
                })(dropdowns[i]);
            }
        },

        setupScrollBehavior: function() {
            var anchorLinks = document.querySelectorAll('a[href^="#"]');
            for (var i = 0; i < anchorLinks.length; i++) {
                anchorLinks[i].addEventListener('click', function(e) {
                    var href = this.getAttribute('href');
                    if (href === '#' || href === '') return;
                    var target = document.querySelector(href);
                    if (target) {
                        e.preventDefault();
                        target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    }
                });
            }
        },

        setupActiveState: function() {
            var currentPath = window.location.pathname;
            var navLinks = document.querySelectorAll('.nav-link-modern');
            for (var i = 0; i < navLinks.length; i++) {
                var href = navLinks[i].getAttribute('href');
                var isActive = href && href !== '#' && (currentPath === href || (href.length > 1 && currentPath.indexOf(href) === 0));
                if (isActive) {
                    navLinks[i].classList.add('active');
                } else {
                    navLinks[i].classList.remove('active');
                }
            }
        },

        setupKeyboardNavigation: function() {
            var self = this;
            var nav = this.nav;
            if (!nav) return;

            nav.addEventListener('keydown', function(e) {
                if (e.key === 'Escape') {
                    if (self.isMobile() && nav.classList.contains('active')) {
                        self.closeMobileMenu();
                    }
                    return;
                }

                if (e.key !== 'Tab' || !self.isMobile() || !nav.classList.contains('active')) return;

                var focusable = nav.querySelectorAll('a[href], button:not([disabled]), [tabindex="0"]');
                var list = [];
                for (var i = 0; i < focusable.length; i++) {
                    if (focusable[i].offsetParent !== null) list.push(focusable[i]);
                }
                var first = list[0];
                var last = list[list.length - 1];
                if (!first) return;

                if (e.shiftKey) {
                    if (document.activeElement === first) {
                        e.preventDefault();
                        last.focus();
                    }
                } else {
                    if (document.activeElement === last) {
                        e.preventDefault();
                        first.focus();
                    }
                }
            });
        },

        setupResize: function() {
            var self = this;
            var resizeTimer;
            window.addEventListener('resize', function() {
                clearTimeout(resizeTimer);
                resizeTimer = setTimeout(function() {
                    if (window.innerWidth >= MOBILE_BREAKPOINT) {
                        self.closeMobileMenu();
                    }
                }, 150);
            });
        }
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            NavigationManager.init();
        });
    } else {
        NavigationManager.init();
    }
})();

