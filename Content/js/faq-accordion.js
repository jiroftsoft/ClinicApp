/**
 * FAQ Accordion Manager
 * Handles FAQ accordion interactions and view count tracking
 */
(function() {
    'use strict';

    const faqAccordion = document.getElementById('faqAccordion');
    if (!faqAccordion) {
        return;
    }

    // Handle accordion toggle
    faqAccordion.addEventListener('show.bs.collapse', function(e) {
        const chevron = e.target.previousElementSibling?.querySelector('.faq-chevron');
        if (chevron) {
            chevron.style.transform = 'rotate(180deg)';
        }
        
        // Increment view count
        const button = e.target.previousElementSibling?.querySelector('.btn-link');
        if (button && button.dataset.faqId) {
            incrementFAQView(button.dataset.faqId);
        }
    });

    faqAccordion.addEventListener('hide.bs.collapse', function(e) {
        const chevron = e.target.previousElementSibling?.querySelector('.faq-chevron');
        if (chevron) {
            chevron.style.transform = 'rotate(0deg)';
        }
    });

    /**
     * Increment FAQ view count via AJAX
     */
    function incrementFAQView(faqId) {
        if (!faqId) return;

        // Use fetch API instead of jQuery
        fetch('/FAQ/IncrementView', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: 'id=' + encodeURIComponent(faqId)
        }).catch(function(error) {
            // Silently fail - view count increment is not critical
            console.warn('Failed to increment FAQ view count:', error);
        });
    }

    // Keyboard navigation support
    faqAccordion.addEventListener('keydown', function(e) {
        if (e.key === 'Enter' || e.key === ' ') {
            const button = e.target.closest('.btn-link');
            if (button) {
                e.preventDefault();
                button.click();
            }
        }
    });
})();

