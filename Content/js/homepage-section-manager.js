/**
 * Homepage Section Manager
 * Manages loading, error, and retry states for homepage sections
 * 
 * Single Responsibility: مدیریت وضعیت‌های Section (Loading, Error, Retry)
 * طبق: DEVELOPMENT_CONTRACT.md, Error Handling
 */

(function() {
    'use strict';

    // ✅ HomePage - Global namespace
    window.HomePage = window.HomePage || {};

    /**
     * Reload a section
     * @param {string} sectionId - ID of the section to reload
     */
    HomePage.reloadSection = function(sectionId) {
        if (!sectionId) {
            console.error('Section ID is required');
            return;
        }

        var section = document.querySelector('[data-section-id="' + sectionId + '"]');
        if (!section) {
            console.error('Section not found:', sectionId);
            return;
        }

        // Show loading state
        showLoadingState(section);

        // TODO: Implement actual reload logic (AJAX call to reload section data)
        // For now, just remove error state and show content
        setTimeout(function() {
            hideErrorState(section);
            console.log('Section reloaded:', sectionId);
        }, 1000);
    };

    /**
     * Show loading state for a section
     */
    function showLoadingState(section) {
        if (!section) return;

        // Remove error/empty states
        var errorDiv = section.querySelector('.section-error');
        var emptyDiv = section.querySelector('.section-empty');
        if (errorDiv) errorDiv.remove();
        if (emptyDiv) emptyDiv.remove();

        // Add loading state
        var loadingDiv = document.createElement('div');
        loadingDiv.className = 'section-loading';
        loadingDiv.innerHTML = '<div class="spinner-border text-primary" role="status" aria-label="در حال بارگذاری">' +
            '<span class="sr-only">در حال بارگذاری...</span></div>' +
            '<p class="section-loading-text">در حال بارگذاری...</p>';
        
        section.appendChild(loadingDiv);
    }

    /**
     * Hide error state for a section
     */
    function hideErrorState(section) {
        if (!section) return;

        var errorDiv = section.querySelector('.section-error');
        var loadingDiv = section.querySelector('.section-loading');
        
        if (errorDiv) errorDiv.remove();
        if (loadingDiv) loadingDiv.remove();
    }

    /**
     * Show error state for a section
     * @param {string} sectionId - ID of the section
     * @param {string} errorMessage - Error message to display
     */
    HomePage.showSectionError = function(sectionId, errorMessage) {
        if (!sectionId) return;

        var section = document.querySelector('[data-section-id="' + sectionId + '"]');
        if (!section) return;

        // Remove existing states
        var loadingDiv = section.querySelector('.section-loading');
        var emptyDiv = section.querySelector('.section-empty');
        if (loadingDiv) loadingDiv.remove();
        if (emptyDiv) emptyDiv.remove();

        // Add error state
        var errorDiv = document.createElement('div');
        errorDiv.className = 'section-error';
        errorDiv.innerHTML = '<i class="fas fa-exclamation-triangle" aria-hidden="true"></i>' +
            '<p class="error-message">' + (errorMessage || 'خطا در بارگذاری') + '</p>' +
            '<button class="btn btn-sm btn-outline-danger mt-2" onclick="HomePage.reloadSection(\'' + sectionId + '\')" aria-label="تلاش مجدد">' +
            '<i class="fas fa-redo" aria-hidden="true"></i> تلاش مجدد</button>';
        
        section.appendChild(errorDiv);
    };

    /**
     * Show empty state for a section
     * @param {string} sectionId - ID of the section
     */
    HomePage.showSectionEmpty = function(sectionId) {
        if (!sectionId) return;

        var section = document.querySelector('[data-section-id="' + sectionId + '"]');
        if (!section) return;

        // Remove existing states
        var loadingDiv = section.querySelector('.section-loading');
        var errorDiv = section.querySelector('.section-error');
        if (loadingDiv) loadingDiv.remove();
        if (errorDiv) errorDiv.remove();

        // Add empty state
        var emptyDiv = document.createElement('div');
        emptyDiv.className = 'section-empty';
        emptyDiv.innerHTML = '<i class="fas fa-info-circle" aria-hidden="true"></i>' +
            '<p>محتوایی برای نمایش وجود ندارد.</p>';
        
        section.appendChild(emptyDiv);
    };

    console.log('HomePage Section Manager initialized');
})();

