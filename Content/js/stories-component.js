/**
 * Stories Component — الگو: دیجی‌کالا
 * اسکرول افقی، کلیک برای ویدیو/لینک، مودال ویدیو
 */

(function($) {
    'use strict';

    var StoriesModule = {
        init: function() {
            this.attachEventHandlers();
            this.setupKeyboardNavigation();
            this.setupScrollKeys();
        },

        attachEventHandlers: function() {
            var self = this;

            $(document).off('click.story').on('click.story', '.story-item', function(e) {
                e.preventDefault();
                var $story = $(this);
                var storyId = $story.data('story-id');
                var videoUrl = ($story.data('video-url') || '').trim();
                var videoType = ($story.data('video-type') || '').trim();
                var linkUrl = ($story.data('link-url') || '').trim();

                if (linkUrl && !videoUrl) {
                    window.location.href = linkUrl;
                    return;
                }
                if (videoUrl) {
                    self.showVideoModal(videoUrl, videoType, storyId);
                } else if (linkUrl) {
                    window.location.href = linkUrl;
                }
            });

            $(document).off('click.storyModalClose').on('click.storyModalClose', '#storyVideoModal .modal-close-btn', function(e) {
                e.preventDefault();
                e.stopPropagation();
                self.closeModal();
            });

            $(document).off('click.storyModalBackdrop').on('click.storyModalBackdrop', '#storyVideoModal', function(e) {
                if ($(e.target).is('#storyVideoModal')) {
                    self.closeModal();
                }
            });

            $(document).off('keydown.storyModalEsc').on('keydown.storyModalEsc', function(e) {
                if (e.key === 'Escape' && $('#storyVideoModal').hasClass('show')) {
                    self.closeModal();
                }
            });

            $('#storyVideoModal').on('hidden.bs.modal', function() {
                self.clearVideoPlayer();
            });
        },

        setupScrollKeys: function() {
            var $wrapper = $('#storiesScrollWrapper');
            if (!$wrapper.length) return;
            $(document).off('keydown.storyScroll').on('keydown.storyScroll', '.story-item', function(e) {
                if (e.key !== 'ArrowLeft' && e.key !== 'ArrowRight') return;
                e.preventDefault();
                var step = $wrapper[0].offsetWidth * 0.6;
                var isRtl = document.documentElement.getAttribute('dir') === 'rtl';
                var dir = (e.key === 'ArrowRight') ? -step : step;
                if (isRtl) dir = -dir;
                $wrapper[0].scrollBy({ left: dir, behavior: 'smooth' });
            });
        },

        closeModal: function() {
            var $modal = $('#storyVideoModal');
            this.clearVideoPlayer();
            $modal.modal('hide');
            setTimeout(function() {
                if ($modal.hasClass('show')) {
                    $modal.removeClass('show').hide();
                    $('.modal-backdrop').remove();
                    $('body').removeClass('modal-open').css('padding-right', '');
                }
            }, 300);
        },

        setupKeyboardNavigation: function() {
            // ✅ پشتیبانی از کیبورد برای دسترسی‌پذیری
            $(document).off('keydown.story').on('keydown.story', '.story-item', function(e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    $(this).trigger('click');
                }
            });
        },

        showVideoModal: function(videoUrl, videoType, storyId) {
            var $modal = $('#storyVideoModal');
            var $player = $('#storyVideoPlayer');
            
            // ✅ پاک کردن محتوای قبلی
            $player.empty();

            // ✅ ساخت Player بر اساس نوع ویدیو
            var playerHtml = '';
            
            if (videoType === 'YouTube') {
                playerHtml = '<iframe class="embed-responsive-item" ' +
                    'src="https://www.youtube.com/embed/' + this.escapeHtml(videoUrl) + '?autoplay=1&rel=0" ' +
                    'allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" ' +
                    'allowfullscreen></iframe>';
            } else if (videoType === 'Vimeo') {
                playerHtml = '<iframe class="embed-responsive-item" ' +
                    'src="https://player.vimeo.com/video/' + this.escapeHtml(videoUrl) + '?autoplay=1" ' +
                    'allow="autoplay; fullscreen; picture-in-picture" ' +
                    'allowfullscreen></iframe>';
            } else if (videoType === 'Aparat') {
                // برای آپارات، URL کامل را استفاده می‌کنیم
                playerHtml = '<iframe class="embed-responsive-item" ' +
                    'src="' + this.escapeHtml(videoUrl) + '" ' +
                    'allowfullscreen></iframe>';
            } else {
                // DirectUpload - استفاده از <video> tag
                playerHtml = '<video controls autoplay class="w-100 h-100" style="object-fit: contain;">' +
                    '<source src="' + this.escapeHtml(videoUrl) + '" type="video/mp4">' +
                    'مرورگر شما از پخش ویدیو پشتیبانی نمی‌کند.' +
                    '</video>';
            }

            $player.html(playerHtml);

            // ✅ نمایش Modal
            $modal.modal('show');

            // ✅ افزایش تعداد بازدید (AJAX)
            if (storyId) {
                this.incrementViewCount(storyId);
            }
        },

        clearVideoPlayer: function() {
            var $player = $('#storyVideoPlayer');
            $player.empty();
        },

        incrementViewCount: function(storyId) {
            var token = $('input[name="__RequestVerificationToken"]').val() || '';
            var url = '/Admin/CMS/Story/IncrementViewCount';
            $.ajax({
                url: url,
                type: 'POST',
                data: { id: storyId, __RequestVerificationToken: token },
                success: function() {},
                error: function() {}
            });
        },

        escapeHtml: function(text) {
            var map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text.replace(/[&<>"']/g, function(m) { return map[m]; });
        }
    };

    $(document).ready(function() {
        if ($('.stories-section').length > 0 && $('#storyVideoModal').length > 0) {
            StoriesModule.init();
        }
    });

    // ✅ Export for global access (if needed)
    window.StoriesModule = StoriesModule;

})(jQuery);
