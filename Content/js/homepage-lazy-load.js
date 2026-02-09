/**
 * Homepage Lazy Loading
 * Implements lazy loading for below-the-fold sections using IntersectionObserver
 * CRITICAL FIX: سکشن‌های داخل viewport بلافاصله نمایش داده می‌شوند (رفع باگ عدم نمایش تا هارد رفرش)
 */

(function() {
    'use strict';

    var config = {
        threshold: 0.1,
        rootMargin: '200px',
        /** فاصله از بالای viewport (px) - سکشن‌هایی که بالاتر از این هستند بلافاصله لود می‌شوند */
        viewportRevealOffset: 400
    };

    function init() {
        if (!('IntersectionObserver' in window)) {
            loadAllSections();
            return;
        }

        var sections = document.querySelectorAll('.homepage-main-content > section[data-lazy-load="true"]');
        if (sections.length === 0) return;

        var observer = new IntersectionObserver(function(entries) {
            entries.forEach(function(entry) {
                if (entry.isIntersecting) {
                    loadSection(entry.target);
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: config.threshold,
            rootMargin: config.rootMargin
        });

        sections.forEach(function(section) {
            observer.observe(section);
        });

        // CRITICAL: سکشن‌های داخل یا نزدیک viewport را بلافاصله بعد از اولین فریم نمایش بده
        // (IntersectionObserver در برخی موارد برای حالت اولیه در بار اول فراخوانی نمی‌شود)
        requestAnimationFrame(function() {
            requestAnimationFrame(function() {
                var vh = window.innerHeight;
                var offset = config.viewportRevealOffset;
                sections.forEach(function(section) {
                    if (section.classList.contains('lazy-loaded')) return;
                    var rect = section.getBoundingClientRect();
                    if (rect.top <= vh + offset) {
                        loadSection(section);
                        observer.unobserve(section);
                    }
                });
            });
        });
    }

    /**
     * Load a section (currently sections are already rendered, this is for future AJAX loading)
     * For now, we just add a class to trigger animations
     */
    function loadSection(section) {
        if (!section) return;

        // Add loaded class for animations
        section.classList.add('lazy-loaded');
        var sectionType = section.getAttribute('data-section-type');
        if (sectionType) initializeSection(section, sectionType);
    }

    /**
     * Initialize section-specific functionality
     */
    function initializeSection(section, sectionType) {
        // Future: Add section-specific initialization logic here
        // For example: Initialize carousels, galleries, etc.
        
        switch (sectionType) {
            case 'gallery':
                // Initialize gallery carousel if needed
                break;
            case 'testimonials':
                // Initialize testimonials carousel if needed
                break;
            default:
                break;
        }
    }

    /**
     * Fallback: Load all sections immediately
     */
    function loadAllSections() {
        var sections = document.querySelectorAll('.homepage-main-content > section[data-lazy-load="true"]');
        sections.forEach(function(section) {
            loadSection(section);
        });
    }

    // ✅ Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();

