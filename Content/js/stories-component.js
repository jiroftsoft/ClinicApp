/**
 * Stories Component - مشابه دیجی‌کالا
 * طراحی شده برای نمایش استوری‌ها زیر منو
 * اصول: SRP, Modular, Production-Ready
 */

(function($) {
    'use strict';

    // ✅ Stories Module (SRP: Stories Management)
    var StoriesModule = {
        init: function() {
            this.attachEventHandlers();
            this.setupKeyboardNavigation();
        },

        attachEventHandlers: function() {
            var self = this;
            
            // ✅ کلیک روی Story
            $(document).off('click.story').on('click.story', '.story-item', function(e) {
                e.preventDefault();
                var $story = $(this);
                var storyId = $story.data('story-id');
                var videoUrl = $story.data('video-url');
                var videoType = $story.data('video-type');
                var linkUrl = $story.data('link-url');
                var buttonText = $story.data('button-text');

                console.log('✅ Story clicked:', { storyId, videoUrl, videoType, linkUrl });

                // اگر لینک وجود دارد و ویدیو نیست، به لینک هدایت می‌شود
                if (linkUrl && linkUrl.trim() !== '' && (!videoUrl || videoUrl.trim() === '')) {
                    window.location.href = linkUrl;
                    return;
                }

                // اگر ویدیو وجود دارد، Modal را نمایش می‌دهد
                if (videoUrl && videoUrl.trim() !== '') {
                    self.showVideoModal(videoUrl, videoType, storyId);
                } else if (linkUrl && linkUrl.trim() !== '') {
                    // اگر فقط لینک وجود دارد
                    window.location.href = linkUrl;
                }
            });

            // ✅ بستن Modal با دکمه X
            $(document).off('click.storyModalClose').on('click.storyModalClose', '#storyVideoModal .modal-close-btn', function(e) {
                e.preventDefault();
                e.stopPropagation();
                console.log('✅ Close button clicked - closing modal');
                self.closeModal();
            });

            // ✅ بستن Modal با کلیک روی backdrop
            $(document).off('click.storyModalBackdrop').on('click.storyModalBackdrop', '#storyVideoModal', function(e) {
                if ($(e.target).is('#storyVideoModal')) {
                    console.log('✅ Backdrop clicked - closing modal');
                    self.closeModal();
                }
            });

            // ✅ بستن Modal با کلید ESC
            $(document).off('keydown.storyModalEsc').on('keydown.storyModalEsc', function(e) {
                if (e.key === 'Escape' && $('#storyVideoModal').hasClass('show')) {
                    console.log('✅ ESC key pressed - closing modal');
                    self.closeModal();
                }
            });

            // ✅ بعد از بسته شدن Modal
            $('#storyVideoModal').on('hidden.bs.modal', function() {
                console.log('✅ Modal hidden event - clearing video');
                self.clearVideoPlayer();
            });
        },

        closeModal: function() {
            var $modal = $('#storyVideoModal');
            console.log('✅ Closing modal...');
            
            // ✅ پاک کردن ویدیو قبل از بستن
            this.clearVideoPlayer();
            
            // ✅ بستن Modal با Bootstrap
            $modal.modal('hide');
            
            // ✅ Fallback: بستن دستی اگر Bootstrap کار نکرد
            setTimeout(function() {
                if ($modal.hasClass('show')) {
                    console.log('⚠️ Bootstrap modal.hide() failed, using manual close');
                    $modal.removeClass('show');
                    $modal.hide();
                    $('.modal-backdrop').remove();
                    $('body').removeClass('modal-open');
                    $('body').css('padding-right', '');
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
            // ✅ ارسال درخواست AJAX برای افزایش تعداد بازدید
            var token = $('input[name="__RequestVerificationToken"]').val() || 
                       $('input[name="__RequestVerificationToken"]').val() ||
                       '';
            
            // ✅ ساخت URL به صورت دستی (چون این فایل JavaScript است و نمی‌تواند از Razor استفاده کند)
            var url = '/Admin/CMS/Story/IncrementViewCount';
            var token = $('input[name="__RequestVerificationToken"]').val() || 
                       $('input[name="__RequestVerificationToken"]').val() ||
                       '';
            
            $.ajax({
                url: url,
                type: 'POST',
                data: {
                    id: storyId,
                    __RequestVerificationToken: token
                },
                success: function(response) {
                    console.log('✅ View count incremented for story:', storyId);
                },
                error: function(xhr, status, error) {
                    console.warn('⚠️ Failed to increment view count:', error);
                }
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

    // ✅ Initialize on Document Ready
    $(document).ready(function() {
        if ($('.stories-section').length > 0) {
            StoriesModule.init();
            console.log('✅ Stories Module initialized');
            
            // ✅ تست Bootstrap Modal
            if (typeof $.fn.modal === 'undefined') {
                console.error('❌ Bootstrap Modal plugin not loaded!');
            } else {
                console.log('✅ Bootstrap Modal plugin loaded');
            }
            
            // ✅ تست وجود Modal
            if ($('#storyVideoModal').length === 0) {
                console.error('❌ Story Video Modal not found in DOM!');
            } else {
                console.log('✅ Story Video Modal found in DOM');
            }
            
            // ✅ تست وجود دکمه Close
            if ($('#storyVideoModal .modal-close-btn').length === 0) {
                console.error('❌ Modal close button not found!');
            } else {
                console.log('✅ Modal close button found');
            }
        }
    });

    // ✅ Export for global access (if needed)
    window.StoriesModule = StoriesModule;

})(jQuery);
