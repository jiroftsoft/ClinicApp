/**
 * Insurance Plan Index - AJAX Functionality
 * طبق قرارداد - بهبود تجربه کاربری با AJAX
 */

$(document).ready(function () {
    // Initialize AJAX functionality
    initializeAjaxSearch();
    initializeAjaxPagination();
});

/**
 * Initialize AJAX search functionality
 */
function initializeAjaxSearch() {
    // Handle form submission with AJAX
    $('#searchForm').on('submit', function (e) {
        e.preventDefault();
        performAjaxSearch();
    });

    // Handle filter changes
    $('#searchForm select').on('change', function () {
        performAjaxSearch();
    });

    // Handle search input with debounce
    let searchTimeout;
    $('#searchForm input[name="searchTerm"]').on('input', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            performAjaxSearch();
        }, 500); // 500ms debounce
    });
}

/**
 * Initialize AJAX pagination functionality
 */
function initializeAjaxPagination() {
    // Handle pagination links
    $(document).on('click', '.pagination a', function (e) {
        e.preventDefault();
        const url = $(this).attr('href');
        performAjaxSearch(url);
    });
}

/**
 * Perform AJAX search
 * @param {string} url - Optional URL for pagination
 */
function performAjaxSearch(url) {
    const form = $('#searchForm');
    const formData = form.serialize();
    
    // Use provided URL or construct from form
    const searchUrl = url || form.attr('action');
    
    // Show loading state
    showLoadingState();
    
    // Perform AJAX request
    $.ajax({
        url: searchUrl,
        type: 'GET',
        data: formData,
        success: function (response) {
            // Update URL without page reload
            if (url) {
                window.history.pushState({}, '', url);
            }
            
            // Update content
            updateContent(response);
            
            // Hide loading state
            hideLoadingState();
        },
        error: function (xhr, status, error) {
            console.error('AJAX search error:', error);
            hideLoadingState();
            
            // Show error message
            showErrorMessage('خطا در بارگذاری اطلاعات. لطفاً دوباره تلاش کنید.');
        }
    });
}

/**
 * Update page content with AJAX response
 * @param {string} response - HTML response
 */
function updateContent(response) {
    // Create a temporary container to parse the response
    const $temp = $('<div>').html(response);
    
    // Update insurance plans list
    const newList = $temp.find('#insurance-plans-list').html();
    if (newList) {
        $('#insurance-plans-list').html(newList);
    }
    
    // Update pagination
    const newPagination = $temp.find('.pagination').parent().html();
    if (newPagination) {
        $('.pagination').parent().html(newPagination);
    }
    
    // Update results summary
    const newSummary = $temp.find('.alert-info').html();
    if (newSummary) {
        $('.alert-info').html(newSummary);
    }
    
    // Update page title if needed
    const newTitle = $temp.find('h2').text();
    if (newTitle) {
        $('h2').text(newTitle);
    }
}

/**
 * Show loading state
 */
function showLoadingState() {
    const $container = $('#insurance-plans-list').parent();
    $container.addClass('loading-overlay');
    
    // Add loading spinner if not exists
    if (!$container.find('.loading-spinner').length) {
        $container.append(`
            <div class="loading-spinner">
                <div class="spinner-border text-primary" role="status">
                    <span class="sr-only">در حال بارگذاری...</span>
                </div>
            </div>
        `);
    }
}

/**
 * Hide loading state
 */
function hideLoadingState() {
    const $container = $('#insurance-plans-list').parent();
    $container.removeClass('loading-overlay');
    $container.find('.loading-spinner').remove();
}

/**
 * Show error message
 * @param {string} message - Error message
 */
function showErrorMessage(message) {
    // Remove existing error messages
    $('.alert-danger').remove();
    
    // Add new error message
    const errorHtml = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <i class="fas fa-exclamation-triangle"></i>
            ${message}
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>
    `;
    
    $('.search-filters').after(errorHtml);
    
    // Auto-hide after 5 seconds
    setTimeout(function () {
        $('.alert-danger').fadeOut();
    }, 5000);
}

/**
 * Handle browser back/forward buttons
 */
window.addEventListener('popstate', function (event) {
    if (event.state) {
        performAjaxSearch(window.location.href);
    }
});

/**
 * Initialize tooltips and other Bootstrap components
 */
function initializeBootstrapComponents() {
    // Initialize tooltips
    $('[data-toggle="tooltip"]').tooltip();
    
    // Initialize popovers
    $('[data-toggle="popover"]').popover();
}

// Initialize Bootstrap components when content is updated
$(document).on('DOMNodeInserted', function () {
    initializeBootstrapComponents();
});
