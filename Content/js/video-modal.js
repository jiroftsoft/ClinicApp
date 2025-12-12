/**
 * 🎥 Video Modal JavaScript
 * 
 * مدیریت Video Modal برای Video Section
 * استفاده از Design System و اصول SRP
 */

(function() {
    'use strict';

    /**
     * Video Modal Manager
     * مدیریت Video Modal با رعایت SRP
     */
    const VideoModalManager = {
        modal: null,
        iframe: null,
        closeButton: null,
        currentVideoId: null,

        /**
         * Initialize Video Modal
         */
        init: function() {
            console.log('🎥 Initializing Video Modal...');
            
            this.setupModal();
            this.bindEvents();
            
            console.log('✅ Video Modal initialized successfully');
        },

        /**
         * Setup Modal HTML
         */
        setupModal: function() {
            // Create modal HTML if it doesn't exist
            if (document.getElementById('videoModal')) {
                this.modal = document.getElementById('videoModal');
                this.iframe = document.getElementById('videoModalIframe');
                this.closeButton = this.modal.querySelector('.video-modal-close');
            } else {
                const modalHTML = `
                    <div class="video-modal" id="videoModal" role="dialog" aria-label="پخش ویدیو" aria-modal="true">
                        <div class="video-modal-content">
                            <div class="video-modal-header">
                                <button class="video-modal-close" aria-label="بستن">
                                    <i class="fas fa-times" aria-hidden="true"></i>
                                </button>
                            </div>
                            <div class="video-modal-body">
                                <iframe class="video-modal-iframe" 
                                        id="videoModalIframe" 
                                        allowfullscreen 
                                        allow="autoplay; encrypted-media"
                                        frameborder="0">
                                </iframe>
                            </div>
                        </div>
                    </div>
                `;
                
                document.body.insertAdjacentHTML('beforeend', modalHTML);
                
                this.modal = document.getElementById('videoModal');
                this.iframe = document.getElementById('videoModalIframe');
                this.closeButton = this.modal.querySelector('.video-modal-close');
            }
        },

        /**
         * Bind Events
         */
        bindEvents: function() {
            // Get all video cards
            const videoCards = document.querySelectorAll('.video-thumbnail-wrapper');
            
            videoCards.forEach((card) => {
                card.addEventListener('click', () => {
                    const embedUrl = card.getAttribute('data-embed-url');
                    const videoId = card.getAttribute('data-video-id');
                    
                    if (embedUrl) {
                        this.openModal(embedUrl, videoId);
                    }
                });
                
                card.addEventListener('keydown', (e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        const embedUrl = card.getAttribute('data-embed-url');
                        const videoId = card.getAttribute('data-video-id');
                        if (embedUrl) {
                            this.openModal(embedUrl, videoId);
                        }
                    }
                });
            });
            
            // Close button
            if (this.closeButton) {
                this.closeButton.addEventListener('click', () => {
                    this.closeModal();
                });
            }
            
            // Close on background click
            if (this.modal) {
                this.modal.addEventListener('click', (e) => {
                    if (e.target === this.modal) {
                        this.closeModal();
                    }
                });
            }
            
            // Keyboard navigation
            document.addEventListener('keydown', (e) => {
                if (!this.modal || !this.modal.classList.contains('active')) {
                    return;
                }
                
                if (e.key === 'Escape') {
                    this.closeModal();
                }
            });
        },

        /**
         * Open Modal
         */
        openModal: function(embedUrl, videoId) {
            if (!this.modal || !this.iframe) return;
            
            this.currentVideoId = videoId;
            
            // Set iframe source
            this.iframe.src = embedUrl;
            
            // Show modal
            this.modal.classList.add('active');
            document.body.style.overflow = 'hidden';
            
            // Focus management
            if (this.closeButton) {
                this.closeButton.focus();
            }
            
            // Increment view count
            if (videoId) {
                this.incrementViewCount(videoId);
            }
        },

        /**
         * Close Modal
         */
        closeModal: function() {
            if (!this.modal || !this.iframe) return;
            
            // Stop video by clearing iframe src
            this.iframe.src = '';
            
            // Hide modal
            this.modal.classList.remove('active');
            document.body.style.overflow = '';
            
            this.currentVideoId = null;
        },

        /**
         * Increment View Count
         */
        incrementViewCount: function(videoId) {
            // Use fetch API for modern browsers
            if (typeof fetch !== 'undefined') {
                fetch('/api/Video/IncrementViewCount', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ videoId: parseInt(videoId) })
                })
                .then(response => {
                    if (!response.ok) {
                        console.warn('Failed to increment view count');
                    }
                })
                .catch(error => {
                    console.error('Error incrementing view count:', error);
                });
            } else {
                // Fallback for older browsers
                const xhr = new XMLHttpRequest();
                xhr.open('POST', '/api/Video/IncrementViewCount', true);
                xhr.setRequestHeader('Content-Type', 'application/json');
                xhr.send(JSON.stringify({ videoId: parseInt(videoId) }));
            }
        }
    };

    /**
     * Initialize when DOM is ready
     */
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            VideoModalManager.init();
        });
    } else {
        VideoModalManager.init();
    }

})();

