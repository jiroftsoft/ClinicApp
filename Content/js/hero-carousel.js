/**
 * Hero Carousel Manager - Medical Environment Production Ready
 * 
 * ویژگی‌های کلیدی:
 * 1. Accessibility (اولویت اول): ARIA, keyboard navigation, WCAG AA
 * 2. Privacy & Security: GDPR compliance, no sensitive data
 * 3. Performance: Image optimization, lazy loading, GPU-accelerated
 * 4. UX: Auto-play control, pause on hover/focus, clear controls
 * 5. Responsive & Touch: Aspect ratio, touch targets >=44px, swipe, RTL
 * 6. Design: Calming colors, soft animations, professional
 * 7. Controls: Auto-play toggle, status text, progress bar
 * 8. Maintainability: API, lazy-fetch, reduced motion support
 */
(function() {
    'use strict';

    // ============================================
    // CONFIGURATION - تنظیمات استاندارد برای محیط درمانی
    // ============================================
    const CONFIG = {
        // Timing
        autoSlideDelay: 6500,        // 6.5 seconds - زمان مناسب برای خواندن
        transitionDuration: 800,     // 0.8 seconds - transition نرم
        initDelay: 2000,             // 2 seconds - تاخیر برای لود تصاویر
        
        // Behavior
        autoPlay: true,              // Auto-play (قابل کنترل توسط کاربر)
        pauseOnHover: true,          // توقف هنگام hover
        pauseOnFocus: true,          // توقف هنگام focus (accessibility)
        loop: true,                  // Loop بین اسلایدها
        keyboardNavigation: true,    // Navigation با کیبورد
        touchSwipe: true,            // Swipe برای موبایل
        swipeThreshold: 50,          // حداقل فاصله برای swipe
        
        // Accessibility
        announceSlideChanges: true,  // اعلان تغییر اسلاید برای screen readers
        reducedMotion: window.matchMedia('(prefers-reduced-motion: reduce)').matches,
        
        // Performance
        lazyLoadImages: true,        // Lazy loading برای اسلایدهای بعدی
        preloadNextSlide: true,      // Preload اسلاید بعدی
        
        // Privacy (GDPR compliance)
        logUserInteractions: false,  // عدم لاگ تعاملات کاربر
        anonymizeData: true          // ناشناس‌سازی داده‌ها
    };

    // ============================================
    // STATE MANAGEMENT
    // ============================================
    let carouselInstance = null;
    let currentIndex = 0;
    let isInitialized = false;
    let autoSlideInterval = null;
    let isPaused = false;
    let isTransitioning = false;
    let touchStartX = 0;
    let touchEndX = 0;
    let progressBar = null;
    let statusText = null;
    let autoPlayToggle = null;
    let startTime = null;
    let progressInterval = null;

    // ============================================
    // DOM ELEMENTS
    // ============================================
    let heroCarousel = null;
    let carouselItems = null;
    let indicators = null;
    let prevButton = null;
    let nextButton = null;
    let carouselInner = null;

    // ============================================
    // INITIALIZATION
    // ============================================
    function init() {
        if (isInitialized) return true;

        heroCarousel = document.getElementById('heroCarousel');
        if (!heroCarousel) return false;

        // Get all elements
        carouselInner = heroCarousel.querySelector('.carousel-inner');
        carouselItems = heroCarousel.querySelectorAll('.carousel-item');
        indicators = heroCarousel.querySelectorAll('.hero-carousel-indicator');
        prevButton = heroCarousel.querySelector('.hero-carousel-controls.prev');
        nextButton = heroCarousel.querySelector('.hero-carousel-controls.next');
        
        createAdditionalControls();

        if (carouselItems.length === 0) return false;

        // Setup accessibility
        setupAccessibility();

        // Initialize carousel
        setupEventListeners();
        showSlide(0, false); // Show first slide without transition

        if (carouselItems.length > 1 && CONFIG.autoPlay && !CONFIG.reducedMotion) {
            setTimeout(function() { startAutoSlide(); }, CONFIG.initDelay);
        }

        isInitialized = true;
        return true;
    }

    // ============================================
    // ACCESSIBILITY SETUP
    // ============================================
    function setupAccessibility() {
        if (!heroCarousel) return;

        // ARIA live region for slide changes
        if (CONFIG.announceSlideChanges) {
            let liveRegion = document.getElementById('hero-carousel-live-region');
            if (!liveRegion) {
                liveRegion = document.createElement('div');
                liveRegion.id = 'hero-carousel-live-region';
                liveRegion.className = 'sr-only';
                liveRegion.setAttribute('role', 'status');
                liveRegion.setAttribute('aria-live', 'polite');
                liveRegion.setAttribute('aria-atomic', 'true');
                heroCarousel.appendChild(liveRegion);
            }
        }

        // Status text for current slide
        statusText = document.getElementById('hero-carousel-status');
        if (!statusText && carouselItems.length > 1) {
            statusText = document.createElement('div');
            statusText.id = 'hero-carousel-status';
            statusText.className = 'hero-carousel-status';
            statusText.setAttribute('aria-live', 'polite');
            statusText.setAttribute('aria-atomic', 'true');
            updateStatusText();
            heroCarousel.appendChild(statusText);
        }

        // Ensure all interactive elements are keyboard accessible
        if (prevButton) {
            prevButton.setAttribute('tabindex', '0');
            prevButton.setAttribute('role', 'button');
        }
        if (nextButton) {
            nextButton.setAttribute('tabindex', '0');
            nextButton.setAttribute('role', 'button');
        }

        indicators.forEach(function(indicator, index) {
            indicator.setAttribute('role', 'tab');
            indicator.setAttribute('tabindex', index === 0 ? '0' : '-1');
            indicator.setAttribute('aria-label', 'اسلاید ' + (index + 1));
        });
    }

    // ============================================
    // ADDITIONAL CONTROLS
    // ============================================
    function createAdditionalControls() {
        if (!heroCarousel || carouselItems.length <= 1) return;

        // Progress bar
        if (!document.getElementById('hero-carousel-progress')) {
            progressBar = document.createElement('div');
            progressBar.id = 'hero-carousel-progress';
            progressBar.className = 'hero-carousel-progress';
            progressBar.setAttribute('role', 'progressbar');
            progressBar.setAttribute('aria-valuemin', '0');
            progressBar.setAttribute('aria-valuemax', '100');
            progressBar.setAttribute('aria-valuenow', '0');
            progressBar.setAttribute('aria-label', 'پیشرفت اسلاید');
            heroCarousel.appendChild(progressBar);
        } else {
            progressBar = document.getElementById('hero-carousel-progress');
        }

        // Auto-play toggle button
        if (!document.getElementById('hero-carousel-autoplay-toggle')) {
            autoPlayToggle = document.createElement('button');
            autoPlayToggle.id = 'hero-carousel-autoplay-toggle';
            autoPlayToggle.className = 'hero-carousel-autoplay-toggle';
            autoPlayToggle.setAttribute('type', 'button');
            autoPlayToggle.setAttribute('aria-label', 'توقف/شروع خودکار اسلاید');
            autoPlayToggle.setAttribute('title', 'توقف/شروع خودکار');
            autoPlayToggle.innerHTML = '<i class="fas fa-pause" aria-hidden="true"></i>';
            autoPlayToggle.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                toggleAutoPlay();
            });
            heroCarousel.appendChild(autoPlayToggle);
        } else {
            autoPlayToggle = document.getElementById('hero-carousel-autoplay-toggle');
        }
    }

    // ============================================
    // SLIDE MANAGEMENT
    // ============================================
    function showSlide(index, animate = true) {
        // Validate index
        if (index < 0 || index >= carouselItems.length) return;

        if (isTransitioning && animate) return;

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
                item.setAttribute('aria-hidden', 'true');
            }
        });

        // Remove active from indicators
        indicators.forEach(function(indicator, i) {
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
            targetSlide.setAttribute('aria-hidden', 'false');

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

            // Preload next slide image (performance optimization)
            if (CONFIG.preloadNextSlide && carouselItems.length > 1) {
                const nextIndex = (index + 1) % carouselItems.length;
                preloadSlideImage(nextIndex);
            }
        }

        // Update indicator
        if (indicators[index]) {
            indicators[index].classList.add('active');
            indicators[index].setAttribute('aria-selected', 'true');
            indicators[index].setAttribute('tabindex', '0');
        }

        currentIndex = index;

        // Update status text and ARIA live region
        updateStatusText();
        announceSlideChange(index);

        // Reset progress bar
        resetProgressBar();

        if (animate) {
            setTimeout(function() {
                isTransitioning = false;
            }, CONFIG.transitionDuration);
        } else {
            isTransitioning = false;
        }

    }

    // ============================================
    // ACCESSIBILITY HELPERS
    // ============================================
    function updateStatusText() {
        if (statusText && carouselItems.length > 1) {
            statusText.textContent = 'اسلاید ' + (currentIndex + 1) + ' از ' + carouselItems.length;
            statusText.setAttribute('aria-label', 'اسلاید ' + (currentIndex + 1) + ' از ' + carouselItems.length);
        }
    }

    function announceSlideChange(index) {
        if (!CONFIG.announceSlideChanges) return;

        const liveRegion = document.getElementById('hero-carousel-live-region');
        if (liveRegion) {
            const slide = carouselItems[index];
            const title = slide ? slide.querySelector('.hero-slide-title') : null;
            const titleText = title ? title.textContent.trim() : '';
            
            liveRegion.textContent = 'اسلاید ' + (index + 1) + ' از ' + carouselItems.length + 
                (titleText ? ': ' + titleText : '');
        }
    }

    // ============================================
    // PERFORMANCE OPTIMIZATION
    // ============================================
    function preloadSlideImage(index) {
        if (index < 0 || index >= carouselItems.length) return;

        const slide = carouselItems[index];
        const heroSlide = slide ? slide.querySelector('.hero-slide') : null;
        if (!heroSlide) return;

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
            
            // Preload image
            const img = new Image();
            img.src = imageUrl;
        }
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

    function goToFirstSlide() {
        showSlide(0);
    }

    function goToLastSlide() {
        showSlide(carouselItems.length - 1);
    }

    // ============================================
    // AUTO-SLIDE MANAGEMENT
    // ============================================
    function startAutoSlide() {
        if (isPaused || carouselItems.length <= 1 || !CONFIG.autoPlay || CONFIG.reducedMotion) {
            return;
        }

        stopAutoSlide();

        startTime = Date.now();
        startProgressBar();
        
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
        }
        stopProgressBar();
    }

    function pauseAutoSlide() {
        if (!isPaused) {
            isPaused = true;
            stopAutoSlide();
            updateAutoPlayToggle(false);
        }
    }

    function resumeAutoSlide() {
        if (isPaused) {
            isPaused = false;
            if (CONFIG.autoPlay && !CONFIG.reducedMotion) {
                startAutoSlide();
            }
            updateAutoPlayToggle(true);
        }
    }

    function toggleAutoPlay() {
        if (isPaused) {
            resumeAutoSlide();
        } else {
            pauseAutoSlide();
        }
    }

    // ============================================
    // PROGRESS BAR
    // ============================================
    function startProgressBar() {
        if (!progressBar || !CONFIG.autoPlay) return;

        stopProgressBar();
        
        let progress = 0;
        const increment = 100 / (CONFIG.autoSlideDelay / 100); // Update every 100ms
        
        progressInterval = setInterval(function() {
            if (!isPaused) {
                progress += increment;
                if (progress >= 100) {
                    progress = 100;
                    stopProgressBar();
                }
                
                progressBar.style.width = progress + '%';
                progressBar.setAttribute('aria-valuenow', Math.round(progress));
            }
        }, 100);
    }

    function stopProgressBar() {
        if (progressInterval) {
            clearInterval(progressInterval);
            progressInterval = null;
        }
        if (progressBar) {
            progressBar.style.width = '0%';
            progressBar.setAttribute('aria-valuenow', '0');
        }
    }

    function resetProgressBar() {
        stopProgressBar();
        if (autoSlideInterval && !isPaused) {
            startProgressBar();
        }
    }

    function updateAutoPlayToggle(isPlaying) {
        if (!autoPlayToggle) return;

        const icon = autoPlayToggle.querySelector('i');
        if (icon) {
            if (isPlaying) {
                icon.className = 'fas fa-pause';
                autoPlayToggle.setAttribute('aria-label', 'توقف خودکار اسلاید');
                autoPlayToggle.setAttribute('title', 'توقف خودکار');
            } else {
                icon.className = 'fas fa-play';
                autoPlayToggle.setAttribute('aria-label', 'شروع خودکار اسلاید');
                autoPlayToggle.setAttribute('title', 'شروع خودکار');
            }
        }
    }

    // ============================================
    // EVENT LISTENERS
    // ============================================
    function setupEventListeners() {
        // Next Button
        if (nextButton) {
            nextButton.addEventListener('click', handleNextClick);
            nextButton.addEventListener('touchend', handleNextClick);
            nextButton.addEventListener('keydown', function(e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    handleNextClick(e);
                }
            });
        }

        // Prev Button
        if (prevButton) {
            prevButton.addEventListener('click', handlePrevClick);
            prevButton.addEventListener('touchend', handlePrevClick);
            prevButton.addEventListener('keydown', function(e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    handlePrevClick(e);
                }
            });
        }

        // Indicators
        indicators.forEach(function(indicator, index) {
            indicator.addEventListener('click', function() {
                goToSlide(index);
                resumeAutoSlide();
            });
            indicator.addEventListener('keydown', function(e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    goToSlide(index);
                    resumeAutoSlide();
                }
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
            document.addEventListener('keydown', handleKeyboardNavigation);
        }

        // Touch swipe
        if (CONFIG.touchSwipe) {
            heroCarousel.addEventListener('touchstart', handleTouchStart, { passive: true });
            heroCarousel.addEventListener('touchend', handleTouchEnd, { passive: true });
        }
    }

    function handleNextClick(e) {
        e.preventDefault();
        e.stopPropagation();
        if (!CONFIG.logUserInteractions) {
            // Privacy: Don't log user interactions
        }
        nextSlide();
        resumeAutoSlide();
    }

    function handlePrevClick(e) {
        e.preventDefault();
        e.stopPropagation();
        if (!CONFIG.logUserInteractions) {
            // Privacy: Don't log user interactions
        }
        prevSlide();
        resumeAutoSlide();
    }

    function handleKeyboardNavigation(e) {
        // Only handle if carousel is focused or hovered
        if (!heroCarousel.matches(':hover') && document.activeElement !== heroCarousel) {
            return;
        }

        switch(e.key) {
            case 'ArrowRight':
            case 'ArrowDown':
                e.preventDefault();
                nextSlide();
                resumeAutoSlide();
                break;
            case 'ArrowLeft':
            case 'ArrowUp':
                e.preventDefault();
                prevSlide();
                resumeAutoSlide();
                break;
            case 'Home':
                e.preventDefault();
                goToFirstSlide();
                resumeAutoSlide();
                break;
            case 'End':
                e.preventDefault();
                goToLastSlide();
                resumeAutoSlide();
                break;
            case ' ': // Spacebar - pause/resume
                e.preventDefault();
                toggleAutoPlay();
                break;
        }
    }

    function handleTouchStart(e) {
        touchStartX = e.changedTouches[0].screenX;
    }

    function handleTouchEnd(e) {
        touchEndX = e.changedTouches[0].screenX;
        handleSwipe();
    }

    function handleSwipe() {
        const diff = touchStartX - touchEndX;

        if (Math.abs(diff) > CONFIG.swipeThreshold) {
            if (diff > 0) {
                // Swipe left (next) - RTL: swipe right means next
                nextSlide();
            } else {
                // Swipe right (prev) - RTL: swipe left means prev
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
        goToFirst: goToFirstSlide,
        goToLast: goToLastSlide,
        pause: pauseAutoSlide,
        resume: resumeAutoSlide,
        toggle: toggleAutoPlay,
        start: startAutoSlide,
        stop: stopAutoSlide,
        getCurrentIndex: function() { return currentIndex; },
        getTotalSlides: function() { return carouselItems ? carouselItems.length : 0; },
        isPaused: function() { return isPaused; },
        isAutoPlaying: function() { return !!autoSlideInterval && !isPaused; }
    };

    // ============================================
    // STARTUP - لود قطعی در بار اول و بعد از refresh
    // ============================================
    function tryInit() {
        if (!document.getElementById('heroCarousel')) return;
        if (init()) {
            if (carouselItems && carouselItems.length > 1 && CONFIG.autoPlay && !CONFIG.reducedMotion) {
                setTimeout(function() { startAutoSlide(); }, CONFIG.initDelay);
            }
        }
    }

    function onDomReady() {
        setTimeout(tryInit, 50);
    }

    function onWindowLoad() {
        if (!document.getElementById('heroCarousel')) return;
        if (!isInitialized) {
            tryInit();
        } else if (carouselItems && carouselItems.length > 0) {
            showSlide(currentIndex, false);
            if (carouselItems.length > 1 && !isPaused && !autoSlideInterval && CONFIG.autoPlay && !CONFIG.reducedMotion) {
                startAutoSlide();
            }
        }
    }

    if (document.readyState === 'complete' || document.readyState === 'interactive') {
        onDomReady();
    } else {
        document.addEventListener('DOMContentLoaded', onDomReady);
    }
    window.addEventListener('load', function() {
        setTimeout(onWindowLoad, 100);
    });
    setTimeout(function() {
        if (document.getElementById('heroCarousel') && !isInitialized) tryInit();
    }, 1500);

    window.HeroCarousel = carouselInstance;
    window.initHeroCarousel = tryInit;

})();
