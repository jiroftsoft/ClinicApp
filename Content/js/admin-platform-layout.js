/**
 * Admin Platform Layout — رفتار سایدبار، تم، منوی موبایل
 * طبق Docs/ADMIN_PLATFORM_ARCHITECTURE.md
 */
(function () {
    'use strict';

    var COOKIE_THEME = 'AdminPlatformTheme';
    var THEME_LIGHT = 'light';
    var THEME_MEDICAL = 'medical-blue';

    function getTheme() {
        try {
            var match = document.cookie.match(new RegExp('(?:^|;\\s*)' + COOKIE_THEME + '=([^;]*)'));
            return (match ? match[1] : null) || THEME_LIGHT;
        } catch (e) { return THEME_LIGHT; }
    }

    function setTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        try {
            var maxAge = 365 * 24 * 60 * 60;
            document.cookie = COOKIE_THEME + '=' + theme + ';path=/;max-age=' + maxAge + ';SameSite=Lax';
        } catch (e) {}
    }

    function cycleTheme() {
        var current = getTheme();
        var next = current === THEME_LIGHT ? THEME_MEDICAL : THEME_LIGHT;
        setTheme(next);
    }

    function initSidebarToggle() {
        var toggle = document.querySelector('[data-sidebar-toggle]');
        var sidebar = document.querySelector('.admin-platform-sidebar');
        if (toggle && sidebar) {
            toggle.addEventListener('click', function () {
                sidebar.classList.toggle('is-open');
            });
        }
    }

    function initThemeToggle() {
        var btn = document.querySelector('[data-theme-toggle]');
        if (btn) btn.addEventListener('click', cycleTheme);
    }

    function init() {
        setTheme(getTheme());
        initSidebarToggle();
        initThemeToggle();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
