/**
 * Performance Optimizer
 * Handles CSS/JS optimization, resource hints, and performance monitoring
 */
(function() {
    'use strict';

    /**
     * Add resource hints for better performance
     */
    function addResourceHints() {
        const head = document.head;
        
        // DNS Prefetch for external domains
        const externalDomains = [
            'https://fonts.googleapis.com',
            'https://fonts.gstatic.com',
            'https://cdnjs.cloudflare.com'
        ];
        
        externalDomains.forEach(function(domain) {
            const link = document.createElement('link');
            link.rel = 'dns-prefetch';
            link.href = domain;
            head.appendChild(link);
        });
    }

    /**
     * Defer non-critical CSS
     */
    function deferNonCriticalCSS() {
        const nonCriticalCSS = document.querySelectorAll('link[rel="stylesheet"][data-defer="true"]');
        nonCriticalCSS.forEach(function(link) {
            link.media = 'print';
            link.onload = function() {
                this.media = 'all';
            };
        });
    }

    /**
     * Monitor performance metrics
     */
    function monitorPerformance() {
        if ('PerformanceObserver' in window) {
            // Monitor Largest Contentful Paint (LCP)
            try {
                const observer = new PerformanceObserver(function(list) {
                    const entries = list.getEntries();
                    const lastEntry = entries[entries.length - 1];
                    console.log('LCP:', lastEntry.renderTime || lastEntry.loadTime);
                });
                observer.observe({ entryTypes: ['largest-contentful-paint'] });
            } catch (e) {
                // PerformanceObserver not fully supported
            }

            // Monitor First Input Delay (FID)
            try {
                const observer = new PerformanceObserver(function(list) {
                    const entries = list.getEntries();
                    entries.forEach(function(entry) {
                        console.log('FID:', entry.processingStart - entry.startTime);
                    });
                });
                observer.observe({ entryTypes: ['first-input'] });
            } catch (e) {
                // PerformanceObserver not fully supported
            }
        }
    }

    /**
     * Optimize animations for performance
     */
    function optimizeAnimations() {
        // Use will-change for animated elements
        const animatedElements = document.querySelectorAll('.animate-in, .animate-fade-in, .animate-slide-up');
        animatedElements.forEach(function(el) {
            if ('IntersectionObserver' in window) {
                const observer = new IntersectionObserver(function(entries) {
                    entries.forEach(function(entry) {
                        if (entry.isIntersecting) {
                            entry.target.style.willChange = 'transform, opacity';
                            observer.unobserve(entry.target);
                            
                            // Remove will-change after animation
                            setTimeout(function() {
                                entry.target.style.willChange = 'auto';
                            }, 1000);
                        }
                    });
                });
                observer.observe(el);
            }
        });
    }

    /**
     * Initialize all optimizations
     */
    function init() {
        // Add resource hints
        addResourceHints();
        
        // Defer non-critical CSS
        deferNonCriticalCSS();
        
        // Monitor performance (only in development)
        if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
            monitorPerformance();
        }
        
        // Optimize animations
        optimizeAnimations();
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();

