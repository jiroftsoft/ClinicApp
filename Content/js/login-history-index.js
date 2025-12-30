/**
 * Login History Index Page Script
 * Single Responsibility: مدیریت DataTables و فیلترها
 * 
 * طبق: LOGIN_SECURITY_AUDIT_ROADMAP.md
 */
(function ($) {
    'use strict';

    $(document).ready(function () {
        // Initialize DataTables
        var table = $('#loginHistoryTable').DataTable({
            language: {
                url: '/Content/plugins/datatables/lang/Persian.json'
            },
            order: [[0, 'desc']], // Sort by ID descending
            pageLength: 25,
            responsive: true,
            columnDefs: [
                { orderable: false, targets: [10] } // Disable sorting on Actions column
            ]
        });

        // Re-initialize DataTables after AJAX content load (if needed)
        $(document).on('loginHistoryTableRedraw', function () {
            table.draw();
        });
    });

})(jQuery);

