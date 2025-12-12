/**
 * 🖼️ Gallery Lightbox JavaScript
 * 
 * مدیریت Lightbox برای Gallery
 * استفاده از Design System و اصول SRP
 */

(function() {
    'use strict';

    /**
     * Gallery Lightbox Manager
     * مدیریت Lightbox با رعایت SRP
     */
    const GalleryLightboxManager = {
        currentIndex: 0,
        items: [],
        lightbox: null,
        lightboxImage: null,
        lightboxTitle: null,
        lightboxClose: null,
        lightboxPrev: null,
        lightboxNext: null,

        /**
         * Initialize Lightbox
         */
        init: function() {
            console.log('🖼️ Initializing Gallery Lightbox...');
            
            this.setupLightbox();
            this.bindEvents();
            
            console.log('✅ Gallery Lightbox initialized successfully');
        },

        /**
         * Setup Lightbox HTML
         */
        setupLightbox: function() {
            // Create lightbox HTML
            const lightboxHTML = `
                <div class="gallery-lightbox" id="galleryLightbox" role="dialog" aria-label="گالری تصاویر" aria-modal="true">
                    <button class="gallery-lightbox-close" aria-label="بستن">
                        <i class="fas fa-times" aria-hidden="true"></i>
                    </button>
                    <button class="gallery-lightbox-nav prev" aria-label="تصویر قبلی">
                        <i class="fas fa-chevron-right" aria-hidden="true"></i>
                    </button>
                    <button class="gallery-lightbox-nav next" aria-label="تصویر بعدی">
                        <i class="fas fa-chevron-left" aria-hidden="true"></i>
                    </button>
                    <div class="gallery-lightbox-content">
                        <img class="gallery-lightbox-image" src="" alt="" id="galleryLightboxImage">
                        <h3 class="gallery-lightbox-title" id="galleryLightboxTitle"></h3>
                    </div>
                </div>
            `;
            
            // Append to body
            document.body.insertAdjacentHTML('beforeend', lightboxHTML);
            
            // Get references
            this.lightbox = document.getElementById('galleryLightbox');
            this.lightboxImage = document.getElementById('galleryLightboxImage');
            this.lightboxTitle = document.getElementById('galleryLightboxTitle');
            this.lightboxClose = this.lightbox.querySelector('.gallery-lightbox-close');
            this.lightboxPrev = this.lightbox.querySelector('.gallery-lightbox-nav.prev');
            this.lightboxNext = this.lightbox.querySelector('.gallery-lightbox-nav.next');
        },

        /**
         * Bind Events
         */
        bindEvents: function() {
            // Get all gallery items
            const galleryItems = document.querySelectorAll('.gallery-item');
            
            galleryItems.forEach((item, index) => {
                item.addEventListener('click', () => {
                    this.openLightbox(index);
                });
                
                item.addEventListener('keydown', (e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        this.openLightbox(index);
                    }
                });
            });
            
            // Close button
            if (this.lightboxClose) {
                this.lightboxClose.addEventListener('click', () => {
                    this.closeLightbox();
                });
            }
            
            // Previous button
            if (this.lightboxPrev) {
                this.lightboxPrev.addEventListener('click', () => {
                    this.showPrevious();
                });
            }
            
            // Next button
            if (this.lightboxNext) {
                this.lightboxNext.addEventListener('click', () => {
                    this.showNext();
                });
            }
            
            // Close on background click
            if (this.lightbox) {
                this.lightbox.addEventListener('click', (e) => {
                    if (e.target === this.lightbox) {
                        this.closeLightbox();
                    }
                });
            }
            
            // Keyboard navigation
            document.addEventListener('keydown', (e) => {
                if (!this.lightbox || !this.lightbox.classList.contains('active')) {
                    return;
                }
                
                if (e.key === 'Escape') {
                    this.closeLightbox();
                } else if (e.key === 'ArrowRight') {
                    this.showNext();
                } else if (e.key === 'ArrowLeft') {
                    this.showPrevious();
                }
            });
        },

        /**
         * Open Lightbox
         */
        openLightbox: function(index) {
            const galleryItems = document.querySelectorAll('.gallery-item');
            this.items = Array.from(galleryItems);
            this.currentIndex = index;
            
            if (this.items.length === 0) return;
            
            // Get image data
            const item = this.items[this.currentIndex];
            const image = item.querySelector('.gallery-image');
            const title = item.querySelector('.gallery-title-text');
            
            if (!image) return;
            
            // Set lightbox content
            this.lightboxImage.src = image.src.replace(image.src.split('/').pop(), image.src.split('/').pop().replace('thumb_', ''));
            this.lightboxImage.alt = image.alt || '';
            this.lightboxTitle.textContent = title ? title.textContent : '';
            
            // Show lightbox
            this.lightbox.classList.add('active');
            document.body.style.overflow = 'hidden';
            
            // Focus management
            this.lightboxClose.focus();
            
            // Update navigation buttons
            this.updateNavigation();
        },

        /**
         * Close Lightbox
         */
        closeLightbox: function() {
            if (!this.lightbox) return;
            
            this.lightbox.classList.remove('active');
            document.body.style.overflow = '';
            
            // Return focus to the item that opened the lightbox
            if (this.items[this.currentIndex]) {
                this.items[this.currentIndex].focus();
            }
        },

        /**
         * Show Previous Image
         */
        showPrevious: function() {
            if (this.items.length === 0) return;
            
            this.currentIndex = (this.currentIndex - 1 + this.items.length) % this.items.length;
            this.updateLightboxImage();
            this.updateNavigation();
        },

        /**
         * Show Next Image
         */
        showNext: function() {
            if (this.items.length === 0) return;
            
            this.currentIndex = (this.currentIndex + 1) % this.items.length;
            this.updateLightboxImage();
            this.updateNavigation();
        },

        /**
         * Update Lightbox Image
         */
        updateLightboxImage: function() {
            const item = this.items[this.currentIndex];
            const image = item.querySelector('.gallery-image');
            const title = item.querySelector('.gallery-title-text');
            
            if (!image) return;
            
            // Fade out
            this.lightboxImage.style.opacity = '0';
            
            setTimeout(() => {
                this.lightboxImage.src = image.src.replace(image.src.split('/').pop(), image.src.split('/').pop().replace('thumb_', ''));
                this.lightboxImage.alt = image.alt || '';
                this.lightboxTitle.textContent = title ? title.textContent : '';
                
                // Fade in
                this.lightboxImage.style.opacity = '1';
            }, 150);
        },

        /**
         * Update Navigation Buttons
         */
        updateNavigation: function() {
            if (this.items.length <= 1) {
                if (this.lightboxPrev) this.lightboxPrev.style.display = 'none';
                if (this.lightboxNext) this.lightboxNext.style.display = 'none';
            } else {
                if (this.lightboxPrev) this.lightboxPrev.style.display = 'flex';
                if (this.lightboxNext) this.lightboxNext.style.display = 'flex';
            }
        }
    };

    /**
     * Initialize when DOM is ready
     */
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            GalleryLightboxManager.init();
        });
    } else {
        GalleryLightboxManager.init();
    }

})();

