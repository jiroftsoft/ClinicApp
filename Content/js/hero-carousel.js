/**
 * Hero Carousel Manager - Production Ready
 * الهام گرفته از بهترین کتابخانه‌های Carousel (Swiper, Owl Carousel)
 * بهینه‌سازی شده برای محیط درمانی
 */
(function() {
    'use strict';

    // ============================================
    // CONFIGURATION - تنظیمات استاندارد برای محیط درمانی
    // ============================================
    const CONFIG = {
        autoSlideDelay: 6500,        // 6.5 seconds - زمان مناسب برای خواندن محتوا
        transitionDuration: 800,     // 0.8 seconds - transition نرم
        initDelay: 2000,             // 2 seconds - تاخیر برای لود کامل تصاویر
        easing: 'cubic-bezier(0.4, 0, 0.2, 1)', // Easing حرفه‌ای
        pauseOnHover: true,          // توقف هنگام hover
        pauseOnFocus: true,          // توقف هنگام focus (accessibility)
        loop: true,                   // Loop بین اسلایدها
        keyboardNavigation: true,     // Navigation با کیبورد
        touchSwipe: true             // Swipe برای موبایل
    };

    // ============================================
    // STATE MANAGEMENT
    // ============================================
    let carouselInstance = null;
    let currentIndex = 0;
    let autoSlideInterval = null;
    let isPaused = false;
    let isTransitioning = false;
    let touchStartX = 0;
    let touchEndX = 0;

    // ============================================
    // DOM ELEMENTS
    // ============================================
    let heroCarousel = null;
    let carouselItems = null;
    let indicators = null;
    let prevButton = null;
    let nextButton = null;

    // ============================================
    // INITIALIZATION
    // ============================================
    function init() {
        console.log('[Hero Carousel] ========================================');
        console.log('[Hero Carousel] 🏥 Medical Carousel - Production Ready');
        console.log('[Hero Carousel] ========================================');

        // Find carousel element
        heroCarousel = document.getElementById('heroCarousel');
        if (!heroCarousel) {
            console.warn('[Hero Carousel] ⚠️ Carousel element not found');
            return false;
        }

        console.log('[Hero Carousel] ✅ Carousel element found');

        // Get all elements
        carouselItems = heroCarousel.querySelectorAll('.carousel-item');
        indicators = heroCarousel.querySelectorAll('.hero-carousel-indicator');
        prevButton = heroCarousel.querySelector('.hero-carousel-controls.prev');
        nextButton = heroCarousel.querySelector('.hero-carousel-controls.next');

        console.log('[Hero Carousel] Elements found:', {
            slides: carouselItems.length,
            indicators: indicators.length,
            prevButton: prevButton ? '✅' : '❌',
            nextButton: nextButton ? '✅' : '❌'
        });

        if (carouselItems.length === 0) {
            console.warn('[Hero Carousel] ⚠️ No slides found');
            return false;
        }

        // Initialize carousel
        setupEventListeners();
        showSlide(0, false); // Show first slide without transition

        // Start auto-slide after delay
        if (carouselItems.length > 1) {
            setTimeout(function() {
                startAutoSlide();
                console.log('[Hero Carousel] ✅ Auto-slide started');
            }, CONFIG.initDelay);
        }

        console.log('[Hero Carousel] ✅ Initialization complete');
        console.log('[Hero Carousel] ========================================');

        return true;
    }

    // ============================================
    // SLIDE MANAGEMENT
    // ============================================
    function showSlide(index, animate = true) {
        // Validate index
        if (index < 0 || index >= carouselItems.length) {
            console.warn('[Hero Carousel] ⚠️ Invalid slide index:', index);
            return;
        }

        // Prevent concurrent transitions
        if (isTransitioning) {
            console.log('[Hero Carousel] ⚠️ Transition in progress, skipping...');
            return;
        }

        if (animate) {
            isTransitioning = true;
        }

        // Remove active class from all items
        carouselItems.forEach(function(item, i) {
            item.classList.remove('active');
            if (i !== index) {
                item.style.opacity = '0';
                item.style.visibility = 'hidden';
                item.style.display = 'none';
                item.style.marginRight = '-100%';
            }
        });

        // Remove active from indicators
        indicators.forEach(function(indicator) {
            indicator.classList.remove('active');
            indicator.setAttribute('aria-selected', 'false');
            indicator.setAttribute('tabindex', '-1');
        });

        // Show target slide
        const targetSlide = carouselItems[index];
        if (targetSlide) {
            targetSlide.classList.add('active');
            targetSlide.style.opacity = '1';
            targetSlide.style.visibility = 'visible';
            targetSlide.style.display = 'flex';
            targetSlide.style.marginRight = '0';
            targetSlide.style.transform = 'translateX(0)';

            // Ensure background image is visible
            const heroSlide = targetSlide.querySelector('.hero-slide');
            if (heroSlide) {
                let imageUrl = heroSlide.getAttribute('data-image-url');
                
                if (!imageUrl || imageUrl === '') {
                    const bgImage = heroSlide.style.backgroundImage;
                    if (bgImage && bgImage !== 'none') {
                        const match = bgImage.match(/url\(['"]?([^'"]+)['"]?\)/);
                        if (match && match[1]) {
                            imageUrl = match[1];
                        }
                    }
                }

                if (imageUrl && imageUrl !== '') {
                    imageUrl = imageUrl.replace(/^['"]|['"]$/g, '');
                    heroSlide.style.backgroundImage = 'url(\'' + imageUrl + '\')';
                    heroSlide.style.backgroundSize = 'cover';
                    heroSlide.style.backgroundPosition = 'center';
                    heroSlide.style.backgroundRepeat = 'no-repeat';
                    heroSlide.style.backgroundColor = 'transparent';
                }
            }

            // Show content
            const content = targetSlide.querySelector('.hero-slide-content');
            if (content) {
                content.style.opacity = '1';
                content.style.visibility = 'visible';
            }
        }

        // Update indicator
        if (indicators[index]) {
            indicators[index].classList.add('active');
            indicators[index].setAttribute('aria-selected', 'true');
            indicators[index].setAttribute('tabindex', '0');
        }

        currentIndex = index;

        if (animate) {
            setTimeout(function() {
                isTransitioning = false;
            }, CONFIG.transitionDuration);
        } else {
            isTransitioning = false;
        }

        console.log('[Hero Carousel] 📍 Slide changed to:', index + 1, 'of', carouselItems.length);
    }

    // ============================================
    // NAVIGATION
    // ============================================
    function nextSlide() {
        if (carouselItems.length === 0) return;
        
        const nextIndex = CONFIG.loop 
            ? (currentIndex + 1) % carouselItems.length
            : Math.min(currentIndex + 1, carouselItems.length - 1);
        
        showSlide(nextIndex);
    }

    function prevSlide() {
        if (carouselItems.length === 0) return;
        
        const prevIndex = CONFIG.loop
            ? (currentIndex - 1 + carouselItems.length) % carouselItems.length
            : Math.max(currentIndex - 1, 0);
        
        showSlide(prevIndex);
    }

    function goToSlide(index) {
        showSlide(index);
    }

    // ============================================
    // AUTO-SLIDE MANAGEMENT
    // ============================================
    function startAutoSlide() {
        if (isPaused || carouselItems.length <= 1) {
            return;
        }

        stopAutoSlide();

        console.log('[Hero Carousel] ▶️ Starting auto-slide (', CONFIG.autoSlideDelay, 'ms)');
        
        autoSlideInterval = setInterval(function() {
            if (!isPaused && !isTransitioning) {
                nextSlide();
            }
        }, CONFIG.autoSlideDelay);
    }

    function stopAutoSlide() {
        if (autoSlideInterval) {
            clearInterval(autoSlideInterval);
            autoSlideInterval = null;
            console.log('[Hero Carousel] ⏹️ Auto-slide stopped');
        }
    }

    function pauseAutoSlide() {
        if (!isPaused) {
            isPaused = true;
            stopAutoSlide();
            console.log('[Hero Carousel] ⏸️ Auto-slide paused');
        }
    }

    function resumeAutoSlide() {
        if (isPaused) {
            isPaused = false;
            startAutoSlide();
            console.log('[Hero Carousel] ▶️ Auto-slide resumed');
        }
    }

    // ============================================
    // EVENT LISTENERS
    // ============================================
    function setupEventListeners() {
        // Next Button
        if (nextButton) {
            nextButton.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                console.log('[Hero Carousel] 🔄 Next button clicked');
                nextSlide();
                resumeAutoSlide();
            });

            nextButton.addEventListener('touchend', function(e) {
                e.preventDefault();
                e.stopPropagation();
                nextSlide();
                resumeAutoSlide();
            });
        }

        // Prev Button
        if (prevButton) {
            prevButton.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                console.log('[Hero Carousel] 🔄 Prev button clicked');
                prevSlide();
                resumeAutoSlide();
            });

            prevButton.addEventListener('touchend', function(e) {
                e.preventDefault();
                e.stopPropagation();
                prevSlide();
                resumeAutoSlide();
            });
        }

        // Indicators
        indicators.forEach(function(indicator, index) {
            indicator.addEventListener('click', function() {
                console.log('[Hero Carousel] 🔄 Indicator', index + 1, 'clicked');
                goToSlide(index);
                resumeAutoSlide();
            });
        });

        // Pause on hover
        if (CONFIG.pauseOnHover) {
            heroCarousel.addEventListener('mouseenter', pauseAutoSlide);
            heroCarousel.addEventListener('mouseleave', resumeAutoSlide);
        }

        // Pause on focus (accessibility)
        if (CONFIG.pauseOnFocus) {
            heroCarousel.addEventListener('focusin', pauseAutoSlide);
            heroCarousel.addEventListener('focusout', resumeAutoSlide);
        }

        // Keyboard navigation
        if (CONFIG.keyboardNavigation) {
            document.addEventListener('keydown', function(e) {
                if (heroCarousel.matches(':hover') || document.activeElement === heroCarousel) {
                    if (e.key === 'ArrowRight' || e.key === 'ArrowDown') {
                        e.preventDefault();
                        nextSlide();
                        resumeAutoSlide();
                    } else if (e.key === 'ArrowLeft' || e.key === 'ArrowUp') {
                        e.preventDefault();
                        prevSlide();
                        resumeAutoSlide();
                    }
                }
            });
        }

        // Touch swipe
        if (CONFIG.touchSwipe) {
            heroCarousel.addEventListener('touchstart', function(e) {
                touchStartX = e.changedTouches[0].screenX;
            }, { passive: true });

            heroCarousel.addEventListener('touchend', function(e) {
                touchEndX = e.changedTouches[0].screenX;
                handleSwipe();
            }, { passive: true });
        }
    }

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
            resumeAutoSlide();
        }
    }

    // ============================================
    // PUBLIC API
    // ============================================
    carouselInstance = {
        next: nextSlide,
        prev: prevSlide,
        goTo: goToSlide,
        pause: pauseAutoSlide,
        resume: resumeAutoSlide,
        start: startAutoSlide,
        stop: stopAutoSlide,
        getCurrentIndex: function() { return currentIndex; },
        getTotalSlides: function() { return carouselItems ? carouselItems.length : 0; }
    };

    // ============================================
    // STARTUP - Multiple initialization strategies
    // ============================================
    function startup() {
        console.log('[Hero Carousel] 🚀 Starting initialization...');
        console.log('[Hero Carousel] Document readyState:', document.readyState);

        // Strategy 1: If DOM is already ready, initialize immediately
        if (document.readyState === 'complete' || document.readyState === 'interactive') {
            console.log('[Hero Carousel] DOM already ready, initializing...');
            setTimeout(function() {
                if (init()) {
                    // If initialization successful, start auto-slide after delay
                    if (carouselItems && carouselItems.length > 1) {
                        setTimeout(function() {
                            startAutoSlide();
                        }, CONFIG.initDelay);
                    }
                }
            }, 100);
        } else {
            // Strategy 2: Wait for DOMContentLoaded
            document.addEventListener('DOMContentLoaded', function() {
                console.log('[Hero Carousel] DOMContentLoaded fired');
                setTimeout(function() {
                    if (init()) {
                        if (carouselItems && carouselItems.length > 1) {
                            setTimeout(function() {
                                startAutoSlide();
                            }, CONFIG.initDelay);
                        }
                    }
                }, 100);
            });
        }

        // Strategy 3: Also initialize on window load (after all resources load)
        window.addEventListener('load', function() {
            console.log('[Hero Carousel] Window load fired');
            if (carouselItems && carouselItems.length > 0) {
                setTimeout(function() {
                    showSlide(currentIndex, false);
                    if (carouselItems.length > 1 && !isPaused && !autoSlideInterval) {
                        startAutoSlide();
                    }
                }, 500);
            }
        });

        // Strategy 4: Fallback - try to initialize after a delay
        setTimeout(function() {
            const carousel = document.getElementById('heroCarousel');
            if (carousel && (!carouselInstance || !carouselItems)) {
                console.log('[Hero Carousel] Fallback initialization triggered');
                if (init()) {
                    if (carouselItems && carouselItems.length > 1) {
                        setTimeout(function() {
                            startAutoSlide();
                        }, CONFIG.initDelay);
                    }
                }
            }
        }, 1000);
    }

    // Expose to global scope BEFORE initialization (for immediate access)
    window.HeroCarousel = carouselInstance;
    window.initHeroCarousel = function() {
        console.log('[Hero Carousel] Manual initialization triggered');
        if (init()) {
            if (carouselItems && carouselItems.length > 1) {
                setTimeout(function() {
                    startAutoSlide();
                }, CONFIG.initDelay);
            }
        }
    };

    // Start initialization immediately
    console.log('[Hero Carousel] Script loaded, starting initialization...');
    startup();

})();
