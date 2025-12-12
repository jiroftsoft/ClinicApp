/**
 * Image Optimization Utility
 * Handles lazy loading, responsive images, and error handling
 */
(function() {
    'use strict';

    /**
     * Initialize Intersection Observer for advanced lazy loading
     */
    function initLazyLoading() {
        if (!('IntersectionObserver' in window)) {
            // Fallback: Native lazy loading is already handled by browser
            return;
        }

        const imageObserver = new IntersectionObserver(function(entries, observer) {
            entries.forEach(function(entry) {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    
                    // Load image from data-src if available
                    if (img.dataset.src) {
                        img.src = img.dataset.src;
                        img.removeAttribute('data-src');
                    }
                    
                    // Load srcset if available
                    if (img.dataset.srcset) {
                        img.srcset = img.dataset.srcset;
                        img.removeAttribute('data-srcset');
                    }
                    
                    // Remove loading="lazy" after image is loaded
                    img.removeAttribute('loading');
                    
                    observer.unobserve(img);
                }
            });
        }, {
            rootMargin: '50px' // Start loading 50px before image enters viewport
        });

        // Observe all images with data-src attribute
        document.querySelectorAll('img[data-src]').forEach(function(img) {
            imageObserver.observe(img);
        });
    }

    /**
     * Handle image load errors with fallback
     */
    function handleImageErrors() {
        document.querySelectorAll('img').forEach(function(img) {
            if (!img.hasAttribute('data-error-handled')) {
                img.setAttribute('data-error-handled', 'true');
                
                img.addEventListener('error', function() {
                    // If image fails to load, try fallback
                    const fallbackSrc = this.dataset.fallback || '/Content/Images/default-image.jpg';
                    
                    // Prevent infinite loop
                    if (this.src !== fallbackSrc && !this.dataset.fallbackTried) {
                        this.dataset.fallbackTried = 'true';
                        this.src = fallbackSrc;
                    } else {
                        // Hide image and show placeholder if fallback also fails
                        this.style.display = 'none';
                        const placeholder = this.nextElementSibling;
                        if (placeholder && placeholder.classList.contains('default-image-placeholder')) {
                            placeholder.style.display = 'flex';
                        }
                    }
                });
            }
        });
    }

    /**
     * Preload critical images (above the fold)
     */
    function preloadCriticalImages() {
        const criticalImages = document.querySelectorAll('img[data-critical="true"]');
        criticalImages.forEach(function(img) {
            const link = document.createElement('link');
            link.rel = 'preload';
            link.as = 'image';
            link.href = img.src || img.dataset.src;
            if (img.srcset) {
                link.imagesrcset = img.srcset;
            }
            document.head.appendChild(link);
        });
    }

    /**
     * Initialize all optimizations
     */
    function init() {
        // Initialize lazy loading
        initLazyLoading();
        
        // Handle image errors
        handleImageErrors();
        
        // Preload critical images
        preloadCriticalImages();
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();

