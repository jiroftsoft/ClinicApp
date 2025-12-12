/**
 * Hero Carousel Manager
 * Handles hero carousel interactions and animations
 */
(function() {
    'use strict';

    const heroCarousel = document.getElementById('heroCarousel');
    if (!heroCarousel) {
        console.warn('[Hero Carousel] Carousel element not found');
        return;
    }
    
    console.log('[Hero Carousel] Carousel element found');

    const carouselItems = heroCarousel.querySelectorAll('.carousel-item');
    const indicators = heroCarousel.querySelectorAll('.hero-carousel-indicator');
    const prevButton = heroCarousel.querySelector('.hero-carousel-controls.prev');
    const nextButton = heroCarousel.querySelector('.hero-carousel-controls.next');
    
    console.log('[Hero Carousel] Found', carouselItems.length, 'carousel items');
    
    // Debug: Log all image URLs and verify they exist
    carouselItems.forEach(function(item, index) {
        const slide = item.querySelector('.hero-slide');
        if (slide) {
            const imageUrl = slide.getAttribute('data-image-url') || 'not set';
            const inlineBg = slide.style.backgroundImage || 'not set';
            console.log('[Hero Carousel] Slide', index, ':', {
                'data-image-url': imageUrl,
                'inline-background': inlineBg,
                'has-active-class': item.classList.contains('active')
            });
            
            // Try to verify image exists
            if (imageUrl && imageUrl !== 'not set' && !imageUrl.includes('clinic-hero.jpg')) {
                const img = new Image();
                img.onload = function() {
                    console.log('[Hero Carousel] ✓ Slide', index, 'image verified:', imageUrl);
                };
                img.onerror = function() {
                    console.error('[Hero Carousel] ✗ Slide', index, 'image NOT FOUND:', imageUrl);
                };
                img.src = imageUrl;
            }
        }
    });

    let currentIndex = 0;
    let autoSlideInterval = null;
    const autoSlideDelay = 5000; // 5 seconds

    /**
     * Show specific slide
     */
    function showSlide(index) {
        // Remove active class from all items and indicators
        carouselItems.forEach(item => {
            item.classList.remove('active');
            item.style.opacity = '0';
            item.style.visibility = 'hidden';
            item.style.display = 'none';
            item.style.marginRight = '-100%';
        });
        indicators.forEach(indicator => indicator.classList.remove('active'));

        // Add active class to current item and indicator
        if (carouselItems[index]) {
            const activeItem = carouselItems[index];
            activeItem.classList.add('active');
            
            // Force visibility with important styles
            activeItem.style.opacity = '1';
            activeItem.style.visibility = 'visible';
            activeItem.style.display = 'flex';
            activeItem.style.marginRight = '0';
            
            // Ensure background image is loaded and displayed
            const heroSlide = activeItem.querySelector('.hero-slide');
            if (heroSlide) {
                // Get image URL from data attribute or inline style
                let imageUrl = heroSlide.getAttribute('data-image-url');
                
                // If not in data attribute, try to extract from inline style
                if (!imageUrl || imageUrl === '') {
                    const bgImage = heroSlide.style.backgroundImage;
                    if (bgImage && bgImage !== 'none') {
                        const match = bgImage.match(/url\(['"]?([^'"]+)['"]?\)/);
                        if (match && match[1]) {
                            imageUrl = match[1];
                        }
                    }
                }
                
                // Fallback to default if no URL found
                if (!imageUrl || imageUrl === '') {
                    imageUrl = '/Content/Images/clinic-hero.jpg';
                }
                
                // Clean up URL (remove quotes if present)
                imageUrl = imageUrl.replace(/^['"]|['"]$/g, '');
                
                // Preload image to ensure it's ready
                const img = new Image();
                img.onload = function() {
                    // Set background image with proper URL encoding
                    heroSlide.style.backgroundImage = 'url("' + imageUrl + '")';
                    heroSlide.style.backgroundSize = 'cover';
                    heroSlide.style.backgroundPosition = 'center';
                    heroSlide.style.backgroundRepeat = 'no-repeat';
                    heroSlide.style.opacity = '1';
                    heroSlide.style.visibility = 'visible';
                    heroSlide.style.display = 'flex';
                    
                    // Set CSS variable for fallback
                    heroSlide.style.setProperty('--hero-slide-image', 'url("' + imageUrl + '")');
                    
                    console.log('[Hero Carousel] Image loaded successfully:', imageUrl);
                };
                img.onerror = function() {
                    // Fallback to default image
                    const fallbackUrl = '/Content/Images/clinic-hero.jpg';
                    heroSlide.style.backgroundImage = 'url("' + fallbackUrl + '")';
                    heroSlide.style.backgroundSize = 'cover';
                    heroSlide.style.backgroundPosition = 'center';
                    heroSlide.style.backgroundRepeat = 'no-repeat';
                    heroSlide.style.opacity = '1';
                    heroSlide.style.visibility = 'visible';
                    heroSlide.style.display = 'flex';
                    
                    console.warn('[Hero Carousel] Image failed to load, using fallback:', imageUrl);
                };
                
                // Set background image immediately (browser will use cached version if available)
                heroSlide.style.backgroundImage = 'url("' + imageUrl + '")';
                heroSlide.style.backgroundSize = 'cover';
                heroSlide.style.backgroundPosition = 'center';
                heroSlide.style.backgroundRepeat = 'no-repeat';
                heroSlide.style.opacity = '1';
                heroSlide.style.visibility = 'visible';
                heroSlide.style.display = 'flex';
                
                // Start loading image for verification
                img.src = imageUrl;
            }
            
            // Force content visibility
            const content = activeItem.querySelector('.hero-slide-content');
            if (content) {
                content.style.opacity = '1';
                content.style.visibility = 'visible';
            }
            
            // Force title, description, buttons visibility
            const title = activeItem.querySelector('.hero-slide-title');
            const description = activeItem.querySelector('.hero-slide-description');
            const buttons = activeItem.querySelector('.hero-slide-buttons');
            
            if (title) {
                title.style.opacity = '1';
                title.style.visibility = 'visible';
            }
            if (description) {
                description.style.opacity = '1';
                description.style.visibility = 'visible';
            }
            if (buttons) {
                buttons.style.opacity = '1';
                buttons.style.visibility = 'visible';
            }
        }
        if (indicators[index]) {
            indicators[index].classList.add('active');
        }

        currentIndex = index;
    }

    /**
     * Go to next slide
     */
    function nextSlide() {
        const nextIndex = (currentIndex + 1) % carouselItems.length;
        showSlide(nextIndex);
    }

    /**
     * Go to previous slide
     */
    function prevSlide() {
        const prevIndex = (currentIndex - 1 + carouselItems.length) % carouselItems.length;
        showSlide(prevIndex);
    }

    /**
     * Start auto slide
     */
    function startAutoSlide() {
        stopAutoSlide();
        autoSlideInterval = setInterval(nextSlide, autoSlideDelay);
    }

    /**
     * Stop auto slide
     */
    function stopAutoSlide() {
        if (autoSlideInterval) {
            clearInterval(autoSlideInterval);
            autoSlideInterval = null;
        }
    }

    // Event Listeners
    if (nextButton) {
        nextButton.addEventListener('click', function(e) {
            e.preventDefault();
            nextSlide();
            startAutoSlide(); // Restart auto slide after manual navigation
        });
    }

    if (prevButton) {
        prevButton.addEventListener('click', function(e) {
            e.preventDefault();
            prevSlide();
            startAutoSlide(); // Restart auto slide after manual navigation
        });
    }

    // Indicator clicks
    indicators.forEach((indicator, index) => {
        indicator.addEventListener('click', function() {
            showSlide(index);
            startAutoSlide(); // Restart auto slide after manual navigation
        });
    });

    // Pause on hover
    heroCarousel.addEventListener('mouseenter', stopAutoSlide);
    heroCarousel.addEventListener('mouseleave', startAutoSlide);

    // Keyboard navigation
    document.addEventListener('keydown', function(e) {
        if (heroCarousel.matches(':hover') || document.activeElement === heroCarousel) {
            if (e.key === 'ArrowRight') {
                e.preventDefault();
                nextSlide();
                startAutoSlide();
            } else if (e.key === 'ArrowLeft') {
                e.preventDefault();
                prevSlide();
                startAutoSlide();
            }
        }
    });

    // Touch swipe support
    let touchStartX = 0;
    let touchEndX = 0;

    heroCarousel.addEventListener('touchstart', function(e) {
        touchStartX = e.changedTouches[0].screenX;
    });

    heroCarousel.addEventListener('touchend', function(e) {
        touchEndX = e.changedTouches[0].screenX;
        handleSwipe();
    });

    function handleSwipe() {
        const swipeThreshold = 50;
        const diff = touchStartX - touchEndX;

        if (Math.abs(diff) > swipeThreshold) {
            if (diff > 0) {
                // Swipe left (next)
                nextSlide();
            } else {
                // Swipe right (prev)
                prevSlide();
            }
            startAutoSlide();
        }
    }

    // Initialize
    function initializeCarousel() {
        if (carouselItems.length > 0) {
            console.log('[Hero Carousel] ========================================');
            console.log('[Hero Carousel] Initializing with', carouselItems.length, 'slides');
            console.log('[Hero Carousel] ========================================');
            
            // Show first slide immediately
            showSlide(0);
            
            // Start auto slide if multiple slides
            if (carouselItems.length > 1) {
                startAutoSlide();
            }
            
            // Force re-render after a short delay to ensure images load
            setTimeout(function() {
                showSlide(0);
                console.log('[Hero Carousel] ========================================');
                console.log('[Hero Carousel] Initialization complete');
                console.log('[Hero Carousel] Current slide:', currentIndex);
                console.log('[Hero Carousel] ========================================');
            }, 300);
        } else {
            console.warn('[Hero Carousel] ⚠️ No slides found');
        }
    }
    
    // Wait for DOM to be fully ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(initializeCarousel, 100);
        });
    } else {
        setTimeout(initializeCarousel, 100);
    }
    
    // Also initialize on window load (after all images are loaded)
    window.addEventListener('load', function() {
        if (carouselItems.length > 0) {
            setTimeout(function() {
                showSlide(currentIndex);
            }, 200);
        }
    });
})();

