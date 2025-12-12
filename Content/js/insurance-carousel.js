/**
 * Professional Insurance Logo Carousel Initialization
 * 
 * استفاده از Swiper.js برای کروسل لوگوهای بیمه
 * بهینه‌سازی شده برای محیط Production
 * حل مشکل Cache و لود شدن
 */

(function() {
    'use strict';

    // Wait for DOM and Swiper to be ready
    function initInsuranceCarousel() {
        // Check if carousel element exists first
        const carouselElement = document.getElementById('insuranceCarousel');
        if (!carouselElement) {
            console.warn('Insurance carousel element not found.');
            return null;
        }

        // Check if Swiper is available
        if (typeof Swiper === 'undefined') {
            console.warn('Swiper.js is not loaded. Retrying in 500ms...');
            setTimeout(initInsuranceCarousel, 500);
            return null;
        }

        // Check if already initialized
        if (carouselElement.swiper) {
            console.log('Insurance carousel already initialized.');
            return carouselElement.swiper;
        }

        // Initialize Swiper
        try {
            const insuranceCarousel = new Swiper('#insuranceCarousel', {
                // Direction
                direction: 'horizontal',
                loop: true,
                
                // RTL Support
                rtl: true, // Right-to-Left for Persian
                
                // Autoplay
                autoplay: {
                    delay: 4000, // 4 seconds (3-5 seconds as requested)
                    disableOnInteraction: false,
                    pauseOnMouseEnter: true,
                },
                
                // Speed
                speed: 800, // Smooth transition
                
                // Slides per view (Responsive)
                slidesPerView: 'auto',
                spaceBetween: 30,
                
                // Breakpoints
                breakpoints: {
                    // Mobile (up to 767px)
                    320: {
                        slidesPerView: 2,
                        spaceBetween: 20,
                    },
                    // Tablet (768px - 991px)
                    768: {
                        slidesPerView: 3,
                        spaceBetween: 25,
                    },
                    // Desktop (992px - 1199px)
                    992: {
                        slidesPerView: 4,
                        spaceBetween: 30,
                    },
                    // Large Desktop (1200px+)
                    1200: {
                        slidesPerView: 5,
                        spaceBetween: 35,
                    },
                    // Extra Large (1400px+)
                    1400: {
                        slidesPerView: 6,
                        spaceBetween: 40,
                    }
                },
                
                // Navigation (Hidden by default, can be enabled)
                navigation: {
                    nextEl: '.insurance-carousel-next',
                    prevEl: '.insurance-carousel-prev',
                },
                
                // Pagination (Hidden by default, can be enabled)
                pagination: {
                    el: '.insurance-carousel-pagination',
                    clickable: true,
                    dynamicBullets: true,
                },
                
                // Touch
                touchEventsTarget: 'container',
                touchRatio: 1,
                touchAngle: 45,
                grabCursor: true,
                
                // Keyboard
                keyboard: {
                    enabled: true,
                    onlyInViewport: true,
                },
                
                // Accessibility
                a11y: {
                    enabled: true,
                    prevSlideMessage: 'اسلاید قبلی',
                    nextSlideMessage: 'اسلاید بعدی',
                    firstSlideMessage: 'این اولین اسلاید است',
                    lastSlideMessage: 'این آخرین اسلاید است',
                },
                
                // Effect
                effect: 'slide', // Simple slide, no fancy effects
                
                // Prevent clicks during transition
                preventClicks: true,
                preventClicksPropagation: true,
                
                // Watch overflow
                watchOverflow: true,
                
                // Observer
                observer: true,
                observeParents: true,
                observeSlideChildren: true,
                
                // Performance
                freeMode: false,
                freeModeSticky: false,
                
                // Events
                on: {
                    init: function() {
                        console.log('✅ Insurance carousel initialized successfully');
                    },
                    slideChange: function() {
                        // Optional: Track slide changes
                    },
                    error: function(swiper, error) {
                        console.error('❌ Swiper error:', error);
                    }
                }
            });

            // Pause on hover (optional enhancement)
            const carouselWrapper = carouselElement.closest('.insurance-carousel-wrapper');
            if (carouselWrapper) {
                carouselWrapper.addEventListener('mouseenter', function() {
                    if (insuranceCarousel.autoplay && insuranceCarousel.autoplay.running) {
                        insuranceCarousel.autoplay.stop();
                    }
                });

                carouselWrapper.addEventListener('mouseleave', function() {
                    if (insuranceCarousel.autoplay && !insuranceCarousel.autoplay.running) {
                        insuranceCarousel.autoplay.start();
                    }
                });
            }

            // Expose carousel instance for external access if needed
            window.insuranceCarousel = insuranceCarousel;
            return insuranceCarousel;
        } catch (error) {
            console.error('❌ Error initializing insurance carousel:', error);
            return null;
        }
    }

    // Multiple initialization strategies to handle cache and loading issues
    function attemptInitialization() {
        // Strategy 1: If DOM and Swiper are ready, initialize immediately
        if (document.readyState !== 'loading' && typeof Swiper !== 'undefined') {
            const result = initInsuranceCarousel();
            if (result) return;
        }

        // Strategy 2: Wait for DOMContentLoaded
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                setTimeout(function() {
                    initInsuranceCarousel();
                }, 100);
            });
        }

        // Strategy 3: Wait for window load (all resources loaded)
        window.addEventListener('load', function() {
            setTimeout(function() {
                if (!window.insuranceCarousel) {
                    initInsuranceCarousel();
                }
            }, 200);
        });

        // Strategy 4: Polling fallback (check every 200ms for max 5 seconds)
        let attempts = 0;
        const maxAttempts = 25; // 25 * 200ms = 5 seconds
        const pollInterval = setInterval(function() {
            attempts++;
            if (typeof Swiper !== 'undefined' && document.getElementById('insuranceCarousel')) {
                const result = initInsuranceCarousel();
                if (result) {
                    clearInterval(pollInterval);
                }
            }
            if (attempts >= maxAttempts) {
                clearInterval(pollInterval);
                console.warn('⚠️ Insurance carousel initialization timeout');
            }
        }, 200);
    }

    // Start initialization
    attemptInitialization();

})();
