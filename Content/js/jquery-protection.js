/**
 * jQuery Protection Helper - کلینیک شفا
 * محافظت از jQuery و اطمینان از بارگذاری کامل آن
 */

(function() {
    'use strict';

    // Global jQuery Protection
    window.ensureJQuery = function(callback) {
        if (typeof jQuery !== 'undefined' && typeof $ !== 'undefined') {
            callback();
        } else {
            setTimeout(function() {
                window.ensureJQuery(callback);
            }, 100);
        }
    };

    // Global jQuery Ready Protection
    window.jQueryReady = function(callback) {
        window.ensureJQuery(function() {
            $(document).ready(callback);
        });
    };

    // Global DatePicker Initialization
    window.initializeDatepickers = function() {
        window.ensureJQuery(function() {
            if (typeof $.fn.datepicker !== 'undefined') {
                $('.datepicker').each(function() {
                    var $this = $(this);
                    if (!$this.data('datepicker-initialized')) {
                        $this.datepicker({
                            format: 'yyyy/mm/dd',
                            autoclose: true,
                            todayHighlight: true,
                            rtl: true
                        });
                        $this.data('datepicker-initialized', true);
                    }
                });
            } else {
                console.warn('DatePicker plugin not loaded');
            }
        });
    };

    // Global Persian DatePicker Initialization
    window.initializePersianDatepickers = function() {
        window.ensureJQuery(function() {
            if (typeof $.fn.persianDatepicker !== 'undefined') {
                $('.persian-date').each(function() {
                    var $this = $(this);
                    if (!$this.data('persian-datepicker-initialized')) {
                        $this.persianDatepicker({
                            format: 'YYYY/MM/DD',
                            initialValue: false,
                            autoClose: true,
                            observer: true,
                            calendar: {
                                persian: {
                                    locale: 'fa',
                                    leapYearMode: 'astronomical'
                                }
                            },
                            toolbox: {
                                todayBtn: { enabled: true, text: { fa: 'امروز' } },
                                clearBtn: { enabled: true, text: { fa: 'پاک کردن' } }
                            },
                            onSelect: function(unix) {
                                var date = new Date(unix);
                                var persianDate = date.getFullYear() + '/' +
                                    String(date.getMonth() + 1).padStart(2, '0') + '/' +
                                    String(date.getDate()).padStart(2, '0');
                                $this.val(persianDate);
                                
                                // Trigger change event
                                $this.trigger('change');
                            }
                        });
                        $this.data('persian-datepicker-initialized', true);
                    }
                });
            } else {
                console.warn('Persian DatePicker plugin not loaded');
            }
        });
    };

    // Global DataTable Initialization
    window.initializeDataTables = function() {
        window.ensureJQuery(function() {
            if (typeof $.fn.DataTable !== 'undefined') {
                $('.datatable').each(function() {
                    var $this = $(this);
                    if (!$this.data('datatable-initialized')) {
                        $this.DataTable({
                            language: {
                                url: '/Content/plugins/datatables/js/Persian.json'
                            },
                            responsive: true,
                            pageLength: 25,
                            order: [[0, 'desc']]
                        });
                        $this.data('datatable-initialized', true);
                    }
                });
            } else {
                console.warn('DataTable plugin not loaded');
            }
        });
    };

    // Global Select2 Initialization
    window.initializeSelect2 = function() {
        window.ensureJQuery(function() {
            if (typeof $.fn.select2 !== 'undefined') {
                $('.select2').each(function() {
                    var $this = $(this);
                    if (!$this.data('select2-initialized')) {
                        $this.select2({
                            dir: 'rtl',
                            language: 'fa',
                            placeholder: 'انتخاب کنید...',
                            allowClear: true
                        });
                        $this.data('select2-initialized', true);
                    }
                });
            } else {
                console.warn('Select2 plugin not loaded');
            }
        });
    };

    // Global SweetAlert2 Configuration
    window.configureSweetAlert = function() {
        window.ensureJQuery(function() {
            if (typeof Swal !== 'undefined') {
                // Configure SweetAlert2 for RTL
                Swal.mixin({
                    confirmButtonText: 'تایید',
                    cancelButtonText: 'انصراف',
                    reverseButtons: true
                });
            }
        });
    };

    // Auto-initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            window.initializeDatepickers();
            window.initializePersianDatepickers();
            window.initializeDataTables();
            window.initializeSelect2();
            window.configureSweetAlert();
        });
    } else {
        window.initializeDatepickers();
        window.initializePersianDatepickers();
        window.initializeDataTables();
        window.initializeSelect2();
        window.configureSweetAlert();
    }

    // Re-initialize for dynamic content
    window.ensureJQuery(function() {
        $(document).on('DOMNodeInserted', function(e) {
            var $target = $(e.target);
            var hasDatepicker = $target.hasClass('datepicker') || $target.find('.datepicker').length > 0;
            var hasPersianDate = $target.hasClass('persian-date') || $target.find('.persian-date').length > 0;
            var hasDataTable = $target.hasClass('datatable') || $target.find('.datatable').length > 0;
            var hasSelect2 = $target.hasClass('select2') || $target.find('.select2').length > 0;

            if (hasDatepicker || hasPersianDate || hasDataTable || hasSelect2) {
                setTimeout(function() {
                    if (hasDatepicker) window.initializeDatepickers();
                    if (hasPersianDate) window.initializePersianDatepickers();
                    if (hasDataTable) window.initializeDataTables();
                    if (hasSelect2) window.initializeSelect2();
                }, 100);
            }
        });
    });

    console.log('jQuery Protection Helper loaded successfully');

})();
