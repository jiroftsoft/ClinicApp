/**
 * 🖼️ Gallery Lightbox - نسخه بهینه و مدرن
 * استفاده از data-image-url برای تصویر تمام‌اندازه، حالت بارگذاری، بدون console
 */
(function() {
    'use strict';

    const GalleryLightboxManager = {
        currentIndex: 0,
        items: [],
        lightbox: null,
        lightboxImage: null,
        lightboxImageWrap: null,
        lightboxLoading: null,
        lightboxTitle: null,
        lightboxCounter: null,
        lightboxClose: null,
        lightboxPrev: null,
        lightboxNext: null,

        init: function() {
            this.setupLightbox();
            this.bindEvents();
        },

        setupLightbox: function() {
            const lightboxHTML = `
                <div class="gallery-lightbox" id="galleryLightbox" role="dialog" aria-label="گالری تصاویر" aria-modal="true">
                    <button type="button" class="gallery-lightbox-close" aria-label="بستن">
                        <i class="fas fa-times" aria-hidden="true"></i>
                    </button>
                    <button type="button" class="gallery-lightbox-nav prev" aria-label="تصویر قبلی">
                        <i class="fas fa-chevron-right" aria-hidden="true"></i>
                    </button>
                    <button type="button" class="gallery-lightbox-nav next" aria-label="تصویر بعدی">
                        <i class="fas fa-chevron-left" aria-hidden="true"></i>
                    </button>
                    <div class="gallery-lightbox-content" id="galleryLightboxContent">
                        <div class="gallery-lightbox-image-wrap">
                            <div class="gallery-lightbox-loading" id="galleryLightboxLoading" aria-hidden="true"></div>
                            <img class="gallery-lightbox-image" src="" alt="" id="galleryLightboxImage">
                        </div>
                        <h3 class="gallery-lightbox-title" id="galleryLightboxTitle"></h3>
                        <p class="gallery-lightbox-counter" id="galleryLightboxCounter" aria-live="polite"></p>
                    </div>
                </div>
            `;
            document.body.insertAdjacentHTML('beforeend', lightboxHTML);

            this.lightbox = document.getElementById('galleryLightbox');
            this.lightboxImage = document.getElementById('galleryLightboxImage');
            this.lightboxImageWrap = this.lightbox && this.lightbox.querySelector('.gallery-lightbox-image-wrap');
            this.lightboxLoading = document.getElementById('galleryLightboxLoading');
            this.lightboxTitle = document.getElementById('galleryLightboxTitle');
            this.lightboxCounter = document.getElementById('galleryLightboxCounter');
            this.lightboxClose = this.lightbox && this.lightbox.querySelector('.gallery-lightbox-close');
            this.lightboxPrev = this.lightbox && this.lightbox.querySelector('.gallery-lightbox-nav.prev');
            this.lightboxNext = this.lightbox && this.lightbox.querySelector('.gallery-lightbox-nav.next');
        },

        bindEvents: function() {
            var self = this;
            var galleryItems = document.querySelectorAll('.gallery-item');
            galleryItems.forEach(function(item, index) {
                item.addEventListener('click', function() { self.openLightbox(index); });
                item.addEventListener('keydown', function(e) {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        self.openLightbox(index);
                    }
                });
            });

            if (this.lightboxClose) {
                this.lightboxClose.addEventListener('click', function() { self.closeLightbox(); });
            }
            if (this.lightboxPrev) {
                this.lightboxPrev.addEventListener('click', function(e) { e.stopPropagation(); self.showPrevious(); });
            }
            if (this.lightboxNext) {
                this.lightboxNext.addEventListener('click', function(e) { e.stopPropagation(); self.showNext(); });
            }

            if (this.lightbox) {
                this.lightbox.addEventListener('click', function(e) {
                    if (e.target === self.lightbox) self.closeLightbox();
                });
            }
            var content = document.getElementById('galleryLightboxContent');
            if (content) {
                content.addEventListener('click', function(e) { e.stopPropagation(); });
            }

            document.addEventListener('keydown', function(e) {
                if (!self.lightbox || !self.lightbox.classList.contains('active')) return;
                if (e.key === 'Escape') self.closeLightbox();
                else if (e.key === 'ArrowRight') self.showNext();
                else if (e.key === 'ArrowLeft') self.showPrevious();
            });
        },

        getImageUrl: function(item) {
            var fullUrl = item.getAttribute('data-image-url');
            if (fullUrl) return fullUrl;
            var img = item.querySelector('.gallery-image');
            return img ? img.src : '';
        },

        getImageTitle: function(item) {
            var title = item.getAttribute('data-title');
            if (title) return title;
            var el = item.querySelector('.gallery-title-text');
            return el ? el.textContent.trim() : '';
        },

        setImageWithLoading: function(imageUrl, alt, title) {
            var self = this;
            if (!this.lightboxImage || !this.lightboxLoading) return;

            this.lightboxImage.classList.remove('loaded');
            this.lightboxLoading.setAttribute('aria-hidden', 'false');
            this.lightboxLoading.style.display = 'flex';

            this.lightboxImage.alt = alt || '';
            if (this.lightboxTitle) this.lightboxTitle.textContent = title || '';

            if (!imageUrl) {
                this.lightboxLoading.style.display = 'none';
                this.lightboxImage.classList.add('loaded');
                return;
            }

            var img = new Image();
            img.onload = function() {
                self.lightboxImage.src = imageUrl;
                self.lightboxImage.classList.add('loaded');
                self.lightboxLoading.style.display = 'none';
                self.lightboxLoading.setAttribute('aria-hidden', 'true');
            };
            img.onerror = function() {
                self.lightboxImage.src = imageUrl;
                self.lightboxImage.classList.add('loaded');
                self.lightboxLoading.style.display = 'none';
                self.lightboxLoading.setAttribute('aria-hidden', 'true');
            };
            img.src = imageUrl;
        },

        openLightbox: function(index) {
            var galleryItems = document.querySelectorAll('.gallery-item');
            this.items = Array.prototype.slice.call(galleryItems);
            this.currentIndex = index;
            if (this.items.length === 0) return;

            var item = this.items[this.currentIndex];
            var imageUrl = this.getImageUrl(item);
            var title = this.getImageTitle(item);
            var imgEl = item.querySelector('.gallery-image');
            var alt = imgEl ? imgEl.alt : '';

            this.setImageWithLoading(imageUrl, alt, title);
            this.updateCounter();
            this.updateNavigation();

            this.lightbox.classList.add('active');
            document.body.style.overflow = 'hidden';
            if (this.lightboxClose) this.lightboxClose.focus();
        },

        closeLightbox: function() {
            if (!this.lightbox) return;
            this.lightbox.classList.remove('active');
            document.body.style.overflow = '';
            if (this.items[this.currentIndex]) {
                this.items[this.currentIndex].focus();
            }
        },

        showPrevious: function() {
            if (this.items.length === 0) return;
            this.currentIndex = (this.currentIndex - 1 + this.items.length) % this.items.length;
            this.updateLightboxImage();
            this.updateCounter();
            this.updateNavigation();
        },

        showNext: function() {
            if (this.items.length === 0) return;
            this.currentIndex = (this.currentIndex + 1) % this.items.length;
            this.updateLightboxImage();
            this.updateCounter();
            this.updateNavigation();
        },

        updateLightboxImage: function() {
            var item = this.items[this.currentIndex];
            var imageUrl = this.getImageUrl(item);
            var title = this.getImageTitle(item);
            var imgEl = item.querySelector('.gallery-image');
            var alt = imgEl ? imgEl.alt : '';
            this.setImageWithLoading(imageUrl, alt, title);
        },

        updateCounter: function() {
            if (!this.lightboxCounter || this.items.length === 0) return;
            if (this.items.length <= 1) {
                this.lightboxCounter.textContent = '';
                this.lightboxCounter.style.display = 'none';
            } else {
                var n = this.currentIndex + 1;
                var total = this.items.length;
                this.lightboxCounter.textContent = n + ' از ' + total;
                this.lightboxCounter.style.display = 'block';
            }
        },

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

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() { GalleryLightboxManager.init(); });
    } else {
        GalleryLightboxManager.init();
    }
})();
