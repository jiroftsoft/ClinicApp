/**
 * Medical Sidebar JavaScript - Production Ready
 * 
 * ویژگی‌های کلیدی:
 * - Accessibility: Keyboard navigation, ARIA support
 * - Security: No sensitive data logging
 * - Performance: Optimized, minimal JavaScript
 * - UX: Smooth interactions, mobile support
 * - Responsive: Touch support, RTL support
 */

(function() {
    'use strict';

    // ============================================
    // CONFIGURATION
    // ============================================
    const CONFIG = {
        // Privacy (GDPR compliance)
        logUserInteractions: false,
        anonymizeData: true,
        
        // Performance
        lazyLoadImages: true,
        
        // Accessibility
        keyboardNavigation: true,
        announceChanges: true
    };

    // ============================================
    // INITIALIZATION
    // ============================================
    function init() {
        console.log('[Medical Sidebar] Initializing...');
        // Reception layout uses #receptionSidebar; other pages may use #medicalSidebar
        const sidebar = document.getElementById('receptionSidebar') || document.getElementById('medicalSidebar');
        if (!sidebar) {
            return;
        }

        // Setup accessibility
        setupAccessibility(sidebar);
        
        // Setup keyboard navigation
        if (CONFIG.keyboardNavigation) {
            setupKeyboardNavigation(sidebar);
        }
        
        // Setup lazy loading
        if (CONFIG.lazyLoadImages) {
            setupLazyLoading(sidebar);
        }
        
        // Setup smooth scrolling for internal links
        setupSmoothScrolling(sidebar);
        
        console.log('[Medical Sidebar] ✅ Initialized successfully');
    }

    // ============================================
    // ACCESSIBILITY
    // ============================================
    function setupAccessibility(sidebar) {
        // Ensure all interactive elements are keyboard accessible
        const interactiveElements = sidebar.querySelectorAll('a, button');
        interactiveElements.forEach(function(element) {
            if (!element.hasAttribute('tabindex')) {
                element.setAttribute('tabindex', '0');
            }
        });

        // Add ARIA live region for dynamic content
        if (CONFIG.announceChanges) {
            let liveRegion = document.getElementById('sidebar-live-region');
            if (!liveRegion) {
                liveRegion = document.createElement('div');
                liveRegion.id = 'sidebar-live-region';
                liveRegion.className = 'sr-only';
                liveRegion.setAttribute('role', 'status');
                liveRegion.setAttribute('aria-live', 'polite');
                liveRegion.setAttribute('aria-atomic', 'true');
                document.body.appendChild(liveRegion);
            }
        }
    }

    // ============================================
    // KEYBOARD NAVIGATION
    // ============================================
    function setupKeyboardNavigation(sidebar) {
        // Handle Enter/Space on interactive elements
        sidebar.addEventListener('keydown', function(e) {
            const target = e.target;
            
            // Enter or Space on links/buttons
            if ((e.key === 'Enter' || e.key === ' ') && 
                (target.tagName === 'A' || target.tagName === 'BUTTON')) {
                e.preventDefault();
                target.click();
            }
        });

        // Focus management for better keyboard navigation
        const focusableElements = sidebar.querySelectorAll(
            'a[href], button:not([disabled]), [tabindex]:not([tabindex="-1"])'
        );

        // Trap focus within sidebar when it's active (for mobile drawer)
        if (window.innerWidth <= 991) {
            const firstFocusable = focusableElements[0];
            const lastFocusable = focusableElements[focusableElements.length - 1];

            sidebar.addEventListener('keydown', function(e) {
                if (e.key === 'Tab') {
                    if (e.shiftKey) {
                        // Shift + Tab
                        if (document.activeElement === firstFocusable) {
                            e.preventDefault();
                            lastFocusable.focus();
                        }
                    } else {
                        // Tab
                        if (document.activeElement === lastFocusable) {
                            e.preventDefault();
                            firstFocusable.focus();
                        }
                    }
                }
            });
        }
    }

    // ============================================
    // LAZY LOADING
    // ============================================
    function setupLazyLoading(sidebar) {
        const images = sidebar.querySelectorAll('img[loading="lazy"]');
        
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver(function(entries, observer) {
                entries.forEach(function(entry) {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        if (img.dataset.src) {
                            img.src = img.dataset.src;
                            img.removeAttribute('data-src');
                        }
                        observer.unobserve(img);
                    }
                });
            }, {
                rootMargin: '50px'
            });

            images.forEach(function(img) {
                imageObserver.observe(img);
            });
        } else {
            // Fallback for browsers without IntersectionObserver
            images.forEach(function(img) {
                if (img.dataset.src) {
                    img.src = img.dataset.src;
                    img.removeAttribute('data-src');
                }
            });
        }
    }

    // ============================================
    // SMOOTH SCROLLING
    // ============================================
    function setupSmoothScrolling(sidebar) {
        sidebar.addEventListener('click', function(e) {
            const link = e.target.closest('a[href^="#"]');
            if (link) {
                const targetId = link.getAttribute('href').substring(1);
                const targetElement = document.getElementById(targetId);
                
                if (targetElement) {
                    e.preventDefault();
                    targetElement.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                    
                    // Update URL without scrolling
                    if (history.pushState) {
                        history.pushState(null, null, '#' + targetId);
                    }
                }
            }
        });
    }

    // ============================================
    // UTILITY FUNCTIONS
    // ============================================
    function announceToScreenReader(message) {
        if (!CONFIG.announceChanges) return;
        
        const liveRegion = document.getElementById('sidebar-live-region');
        if (liveRegion) {
            liveRegion.textContent = message;
            // Clear after announcement
            setTimeout(function() {
                liveRegion.textContent = '';
            }, 1000);
        }
    }

    // ============================================
    // STARTUP
    // ============================================
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Expose to global scope for debugging
    window.MedicalSidebar = {
        init: init,
        announce: announceToScreenReader
    };

})();

