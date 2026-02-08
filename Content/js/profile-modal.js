/**
 * Profile Modal - صفحه اصلی
 * مودال پروفایل بهینه برای پروداکشن درمانی
 * بارگذاری AJAX از LoadProfileComponent، راه‌اندازی UserProfileComponent پس از لود
 */
(function($, window) {
    'use strict';

    var profileModalEl = null;
    var profileModalBackdrop = null;
    var profileModalBody = null;
    var profileModalCloseBtn = null;
    var contentLoaded = false;
    var loadProfileUrl = null;

    function getLoadProfileUrl() {
        if (loadProfileUrl) return loadProfileUrl;
        if (window.PROFILE_MODAL_LOAD_URL) {
            loadProfileUrl = window.PROFILE_MODAL_LOAD_URL;
            return loadProfileUrl;
        }
        var base = document.querySelector('script[data-profile-modal-url]');
        loadProfileUrl = base ? base.getAttribute('data-profile-modal-url') : null;
        if (!loadProfileUrl) {
            var a = document.createElement('a');
            a.href = '/Account/LoadProfileComponent';
            loadProfileUrl = a.pathname + '?showHeader=false&containerClass=col-12&cancelButtonText=بستن&formId=profile-form-modal&cancelUrl=';
        } else if (loadProfileUrl.indexOf('?') === -1) {
            loadProfileUrl += '?showHeader=false&containerClass=col-12&cancelButtonText=بستن&formId=profile-form-modal&cancelUrl=';
        }
        return loadProfileUrl;
    }

    function showLoading() {
        if (!profileModalBody) return;
        profileModalBody.innerHTML = '<div class="profile-modal-loading d-flex justify-content-center align-items-center py-5">' +
            '<div class="spinner-border text-primary" style="width: 3rem; height: 3rem;" role="status">' +
            '<span class="visually-hidden">در حال بارگذاری...</span></div></div>';
        contentLoaded = false;
    }

    function loadContent() {
        if (!profileModalBody) return;
        var url = getLoadProfileUrl();
        showLoading();
        $.ajax({
            url: url,
            method: 'GET',
            dataType: 'html',
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            cache: false,
            success: function(html) {
                if (!profileModalBody) return;
                profileModalBody.innerHTML = html;
                contentLoaded = true;
                var $container = $(profileModalBody).find('[data-profile-component="true"]').closest('.row');
                if ($container.length && window.UserProfileComponent) {
                    var $form = $container.find('form[id]');
                    var formId = $form.length ? $form.attr('id') : 'profile-form-modal';
                    var apiUrl = $form.data('api-url') || '/Account/Profile';
                    UserProfileComponent.init($container, { apiUrl: apiUrl, formId: formId });
                    $container.one('profileComponent:success', function() {
                        if (window.closeProfileModal) window.closeProfileModal();
                    });
                }
                $(profileModalBody).find('a.btn-secondary[href="#"], a.btn-secondary[href=""]').off('click.profileModal').on('click.profileModal', function(e) {
                    e.preventDefault();
                    if (window.closeProfileModal) window.closeProfileModal();
                });
            },
            error: function(xhr) {
                if (!profileModalBody) return;
                var msg = xhr.status === 401 ? 'لطفاً دوباره وارد شوید.' : 'خطا در بارگذاری پروفایل.';
                profileModalBody.innerHTML = '<div class="alert alert-danger mb-0">' + msg + '</div>';
                if (window.toastr) toastr.error(msg, '', { timeOut: 4000 });
            }
        });
    }

    function openModal() {
        if (!profileModalEl) return;
        profileModalEl.classList.add('show');
        profileModalEl.setAttribute('aria-hidden', 'false');
        if (profileModalBackdrop) {
            profileModalBackdrop.classList.add('show');
        }
        document.body.classList.add('profile-modal-open');
        loadContent();
        var firstFocus = profileModalEl.querySelector('#profileModalCloseBtn');
        if (firstFocus) setTimeout(function() { firstFocus.focus(); }, 100);
    }

    function closeModal() {
        if (!profileModalEl) return;
        profileModalEl.classList.remove('show');
        profileModalEl.setAttribute('aria-hidden', 'true');
        if (profileModalBackdrop) {
            profileModalBackdrop.classList.remove('show');
        }
        document.body.classList.remove('profile-modal-open');
        var trigger = document.querySelector('[data-open-profile-modal][aria-expanded="true"]') || document.getElementById('userProfileDropdown');
        if (trigger) setTimeout(function() { trigger.focus(); }, 50);
    }

    function init() {
        profileModalEl = document.getElementById('profileModal');
        profileModalBackdrop = document.getElementById('profileModalBackdrop');
        profileModalBody = document.getElementById('profileModalBody');
        profileModalCloseBtn = document.getElementById('profileModalCloseBtn');
        if (!profileModalEl || !profileModalBody) return;

        if (profileModalCloseBtn) {
            profileModalCloseBtn.addEventListener('click', closeModal);
        }
        if (profileModalBackdrop) {
            profileModalBackdrop.addEventListener('click', function(e) {
                if (e.target === profileModalBackdrop) closeModal();
            });
        }
        profileModalEl.addEventListener('click', function(e) {
            if (e.target === profileModalEl) closeModal();
        });
        profileModalEl.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') closeModal();
        });
        $(document).on('click', '[data-open-profile-modal="true"]', function(e) {
            e.preventDefault();
            e.stopPropagation();
            openModal();
            return false;
        });

        window.openProfileModal = openModal;
        window.closeProfileModal = closeModal;
    }

    $(document).ready(init);
})(jQuery, window);
