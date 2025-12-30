/**
 * Homepage Lazy Loading
 * Implements lazy loading for below-the-fold sections using IntersectionObserver
 * 
 * Single Responsibility: مدیریت Lazy Loading برای بخش‌های صفحه اصلی
 * طبق: DEVELOPMENT_CONTRACT.md, Performance Optimization
 */

(function() {
    'use strict';

    // ✅ Configuration
    var config = {
        // Sections that should load immediately (above-the-fold)
        criticalSections: [
            'main-menu-quick-actions',
            'hero-section',
            'value-proposition-section',
            'quick-appointment-section'
        ],
        // Threshold for IntersectionObserver (10% visible)
        threshold: 0.1,
        // Root margin for earlier loading (200px before section is visible)
        rootMargin: '200px'
    };

    /**
     * Initialize lazy loading
     */
    function init() {
        if (!('IntersectionObserver' in window)) {
            // Fallback: Load all sections if IntersectionObserver is not supported
            console.warn('IntersectionObserver not supported - loading all sections');
            loadAllSections();
            return;
        }

        // Find all sections that should be lazy-loaded
        var sections = document.querySelectorAll('.homepage-main-content > section[data-lazy-load="true"]');
        
        if (sections.length === 0) {
            console.log('No lazy-load sections found');
            return;
        }

        // Create IntersectionObserver
        var observer = new IntersectionObserver(function(entries) {
            entries.forEach(function(entry) {
                if (entry.isIntersecting) {
                    // Section is visible, load it
                    loadSection(entry.target);
                    // Stop observing this section
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: config.threshold,
            rootMargin: config.rootMargin
        });

        // Observe all lazy-load sections
        sections.forEach(function(section) {
            observer.observe(section);
        });

        console.log('Lazy loading initialized for ' + sections.length + ' sections');
    }

    /**
     * Load a section (currently sections are already rendered, this is for future AJAX loading)
     * For now, we just add a class to trigger animations
     */
    function loadSection(section) {
        if (!section) return;

        // Add loaded class for animations
        section.classList.add('lazy-loaded');
        
        // Trigger any section-specific initialization
        var sectionType = section.getAttribute('data-section-type');
        if (sectionType) {
            initializeSection(section, sectionType);
        }

        console.log('Section loaded:', section.className);
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

