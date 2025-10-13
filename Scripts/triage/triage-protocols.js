/**
 * مدیریت پروتکل‌های تریاژ - Protocol Management
 * طراحی شده برای سیستم‌های پزشکی کلینیک شفا
 */

var TriageProtocol = {
    // تنظیمات
    config: {
        protocolTypes: {
            1: 'عمومی',
            2: 'قلبی',
            3: 'تنفسی',
            4: 'عصبی',
            5: 'تروما',
            6: 'اطفال'
        },
        priorityLevels: {
            1: 'خیلی بحرانی',
            2: 'فوری',
            3: 'اوژانسی',
            4: "غیر اورژانسی",
            5: "کم فوریت"
        }
    },

    /**
     * راه‌اندازی اولیه مدیریت پروتکل‌ها
     */
    init: function() {
        console.log('TriageProtocol: Initializing...');
        
        // تنظیم رویدادها
        this.setupEvents();
        
        // تنظیم فیلترها
        this.setupFilters();
        
        // بارگذاری اولیه داده‌ها
        this.loadInitialData();
        
        console.log('TriageProtocol: Initialized successfully');
    },

    /**
     * تنظیم رویدادها
     */
    setupEvents: function() {
        var self = this;
        
        // دکمه به‌روزرسانی
        $(document).on('click', '[onclick*="TriageProtocol.refreshProtocols"]', function(e) {
            e.preventDefault();
            self.refreshProtocols();
        });
        
        // دکمه تغییر وضعیت
        $(document).on('click', '[onclick*="TriageProtocol.toggleStatus"]', function(e) {
            e.preventDefault();
            var protocolId = $(this).data('protocol-id');
            var isActive = $(this).data('is-active');
            self.toggleStatus(protocolId, isActive);
        });
        
        // دکمه حذف
        $(document).on('click', '[onclick*="TriageProtocol.deleteProtocol"]', function(e) {
            e.preventDefault();
            var protocolId = $(this).data('protocol-id');
            self.deleteProtocol(protocolId);
        });
        
        // تغییر فیلترها
        $(document).on('change', '#protocol-type-filter, #status-filter', function() {
            self.filterProtocols();
        });
        
        // جستجو
        $(document).on('keyup', '#search-term', function() {
            clearTimeout(self.searchTimeout);
            self.searchTimeout = setTimeout(function() {
                self.searchProtocols();
            }, 500);
        });
    },

    /**
     * تنظیم فیلترها
     */
    setupFilters: function() {
        // تنظیم پیش‌فرض فیلترها
        $('#protocol-type-filter').val('');
        $('#status-filter').val('');
        $('#search-term').val('');
    },

    /**
     * بارگذاری اولیه داده‌ها
     */
    loadInitialData: function() {
        this.loadProtocols();
    },

    /**
     * بارگذاری پروتکل‌ها
     */
    loadProtocols: function() {
        var self = this;
        
        $.ajax({
            url: '/TriageProtocol/GetActiveProtocols',
            type: 'POST',
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    self.displayProtocols(response.data);
                } else {
                    console.error('TriageProtocol: Error loading protocols:', response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageProtocol: Error loading protocols:', error);
            }
        });
    },

    /**
     * نمایش پروتکل‌ها
     */
    displayProtocols: function(protocols) {
        var tbody = $('#protocols-table tbody');
        tbody.empty();
        
        if (protocols && protocols.length > 0) {
            protocols.forEach(function(protocol) {
                var row = self.createProtocolRow(protocol);
                tbody.append(row);
            });
        } else {
            tbody.append('<tr><td colspan="7" class="text-center text-muted"><i class="fas fa-inbox fa-2x mb-2"></i><br>هیچ پروتکلی یافت نشد</td></tr>');
        }
    },

    /**
     * ایجاد ردیف پروتکل
     */
    createProtocolRow: function(protocol) {
        var protocolTypeBadge = this.getProtocolTypeBadge(protocol.ProtocolType);
        var priorityBadge = this.getPriorityBadge(protocol.Priority);
        var statusBadge = this.getStatusBadge(protocol.IsActive);
        
        return '<tr data-protocol-id="' + protocol.TriageProtocolId + '" data-protocol-type="' + protocol.ProtocolType + '" data-is-active="' + protocol.IsActive.toString().toLowerCase() + '">' +
            '<td><strong>' + protocol.Name + '</strong>' +
            (protocol.Description ? '<br><small class="text-muted">' + protocol.Description + '</small>' : '') +
            '</td>' +
            '<td>' + protocolTypeBadge + '</td>' +
            '<td>' + priorityBadge + '</td>' +
            '<td>' + statusBadge + '</td>' +
            '<td><small>' + this.formatDateTime(protocol.CreatedAt) + '</small></td>' +
            '<td><small>' + (protocol.UpdatedAt ? this.formatDateTime(protocol.UpdatedAt) : '-') + '</small></td>' +
            '<td>' + this.createActionButtons(protocol) + '</td>' +
            '</tr>';
    },

    /**
     * دریافت نشان نوع پروتکل
     */
    getProtocolTypeBadge: function(protocolType) {
        var badges = {
            1: '<span class="badge badge-primary">عمومی</span>',
            2: '<span class="badge badge-danger">قلبی</span>',
            3: '<span class="badge badge-info">تنفسی</span>',
            4: '<span class="badge badge-warning">عصبی</span>',
            5: '<span class="badge badge-secondary">تروما</span>',
            6: '<span class="badge badge-success">اطفال</span>'
        };
        return badges[protocolType] || '<span class="badge badge-secondary">نامشخص</span>';
    },

    /**
     * دریافت نشان اولویت
     */
    getPriorityBadge: function(priority) {
        var badgeClass = priority <= 3 ? 'danger' : priority <= 6 ? 'warning' : 'info';
        return '<span class="badge badge-' + badgeClass + '">' + priority + '</span>';
    },

    /**
     * دریافت نشان وضعیت
     */
    getStatusBadge: function(isActive) {
        return isActive ? 
            '<span class="badge badge-success">فعال</span>' : 
            '<span class="badge badge-secondary">غیرفعال</span>';
    },

    /**
     * ایجاد دکمه‌های عملیات
     */
    createActionButtons: function(protocol) {
        var buttons = '<div class="btn-group btn-group-sm" role="group">';
        
        // دکمه مشاهده جزئیات
        buttons += '<a href="/TriageProtocol/Details/' + protocol.TriageProtocolId + '" class="btn btn-info btn-sm" title="مشاهده جزئیات"><i class="fas fa-eye"></i></a>';
        
        // دکمه ویرایش
        buttons += '<a href="/TriageProtocol/Edit/' + protocol.TriageProtocolId + '" class="btn btn-warning btn-sm" title="ویرایش"><i class="fas fa-edit"></i></a>';
        
        // دکمه تغییر وضعیت
        var statusButton = protocol.IsActive ? 
            '<button type="button" class="btn btn-secondary btn-sm" onclick="TriageProtocol.toggleStatus(' + protocol.TriageProtocolId + ', true)" title="غیرفعال کردن"><i class="fas fa-pause"></i></button>' :
            '<button type="button" class="btn btn-success btn-sm" onclick="TriageProtocol.toggleStatus(' + protocol.TriageProtocolId + ', false)" title="فعال کردن"><i class="fas fa-play"></i></button>';
        buttons += statusButton;
        
        // دکمه حذف
        buttons += '<button type="button" class="btn btn-danger btn-sm" onclick="TriageProtocol.deleteProtocol(' + protocol.TriageProtocolId + ')" title="حذف"><i class="fas fa-trash"></i></button>';
        
        buttons += '</div>';
        return buttons;
    },

    /**
     * فرمت تاریخ و زمان
     */
    formatDateTime: function(dateTime) {
        if (!dateTime) return '-';
        var date = new Date(dateTime);
        return date.toLocaleDateString('fa-IR') + ' ' + date.toLocaleTimeString('fa-IR');
    },

    /**
     * فیلتر کردن پروتکل‌ها
     */
    filterProtocols: function() {
        var protocolType = $('#protocol-type-filter').val();
        var status = $('#status-filter').val();
        var searchTerm = $('#search-term').val().toLowerCase();
        
        $('#protocols-table tbody tr').each(function() {
            var $row = $(this);
            var show = true;
            
            // فیلتر نوع پروتکل
            if (protocolType && $row.data('protocol-type') != protocolType) {
                show = false;
            }
            
            // فیلتر وضعیت
            if (status && $row.data('is-active') != (status === 'true')) {
                show = false;
            }
            
            // فیلتر جستجو
            if (searchTerm) {
                var text = $row.text().toLowerCase();
                if (!text.includes(searchTerm)) {
                    show = false;
                }
            }
            
            $row.toggle(show);
        });
    },

    /**
     * جستجوی پروتکل‌ها
     */
    searchProtocols: function() {
        this.filterProtocols();
    },

    /**
     * به‌روزرسانی پروتکل‌ها
     */
    refreshProtocols: function() {
        this.loadProtocols();
        toastr.success('لیست پروتکل‌ها به‌روزرسانی شد');
    },

    /**
     * تغییر وضعیت پروتکل
     */
    toggleStatus: function(protocolId, isActive) {
        var self = this;
        var newStatus = !isActive;
        var action = newStatus ? 'فعال' : 'غیرفعال';
        
        if (confirm('آیا مطمئن هستید که می‌خواهید این پروتکل را ' + action + ' کنید؟')) {
            $.ajax({
                url: '/TriageProtocol/ToggleStatus',
                type: 'POST',
                data: {
                    protocolId: protocolId,
                    isActive: newStatus
                },
                dataType: 'json',
                success: function(response) {
                    if (response.success) {
                        toastr.success('وضعیت پروتکل با موفقیت تغییر کرد');
                        self.refreshProtocols();
                    } else {
                        toastr.error('خطا در تغییر وضعیت: ' + response.message);
                    }
                },
                error: function(xhr, status, error) {
                    console.error('TriageProtocol: Error toggling status:', error);
                    toastr.error('خطا در تغییر وضعیت پروتکل');
                }
            });
        }
    },

    /**
     * حذف پروتکل
     */
    deleteProtocol: function(protocolId) {
        var self = this;
        
        if (confirm('آیا مطمئن هستید که می‌خواهید این پروتکل را حذف کنید؟ این عمل قابل بازگشت نیست.')) {
            $.ajax({
                url: '/TriageProtocol/Delete',
                type: 'POST',
                data: {
                    id: protocolId
                },
                dataType: 'json',
                success: function(response) {
                    if (response.success) {
                        toastr.success('پروتکل با موفقیت حذف شد');
                        self.refreshProtocols();
                    } else {
                        toastr.error('خطا در حذف پروتکل: ' + response.message);
                    }
                },
                error: function(xhr, status, error) {
                    console.error('TriageProtocol: Error deleting protocol:', error);
                    toastr.error('خطا در حذف پروتکل');
                }
            });
        }
    },

    /**
     * اعمال پروتکل به ارزیابی
     */
    applyProtocolToAssessment: function(assessmentId, protocolId) {
        var self = this;
        
        $.ajax({
            url: '/TriageProtocol/ApplyProtocolToAssessment',
            type: 'POST',
            data: {
                assessmentId: assessmentId,
                protocolId: protocolId
            },
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    toastr.success('پروتکل با موفقیت اعمال شد');
                } else {
                    toastr.error('خطا در اعمال پروتکل: ' + response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageProtocol: Error applying protocol:', error);
                toastr.error('خطا در اعمال پروتکل');
            }
        });
    },

    /**
     * دریافت پیشنهادات پروتکل
     */
    getProtocolSuggestions: function(assessmentId) {
        var self = this;
        
        $.ajax({
            url: '/TriageProtocol/GetProtocolSuggestions',
            type: 'POST',
            data: {
                assessmentId: assessmentId
            },
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    self.displayProtocolSuggestions(response.data);
                } else {
                    console.error('TriageProtocol: Error getting suggestions:', response.message);
                }
            },
            error: function(xhr, status, error) {
                console.error('TriageProtocol: Error getting suggestions:', error);
            }
        });
    },

    /**
     * نمایش پیشنهادات پروتکل
     */
    displayProtocolSuggestions: function(suggestions) {
        var modal = $('#protocol-suggestions-modal');
        if (modal.length === 0) {
            modal = this.createProtocolSuggestionsModal();
        }
        
        var suggestionsList = modal.find('#suggestions-list');
        suggestionsList.empty();
        
        if (suggestions && suggestions.length > 0) {
            suggestions.forEach(function(suggestion) {
                var item = self.createProtocolSuggestionItem(suggestion);
                suggestionsList.append(item);
            });
        } else {
            suggestionsList.append('<li class="list-group-item text-center text-muted">هیچ پیشنهادی یافت نشد</li>');
        }
        
        modal.modal('show');
    },

    /**
     * ایجاد مودال پیشنهادات پروتکل
     */
    createProtocolSuggestionsModal: function() {
        var modalHtml = '<div class="modal fade" id="protocol-suggestions-modal" tabindex="-1" role="dialog">' +
            '<div class="modal-dialog modal-lg" role="document">' +
            '<div class="modal-content">' +
            '<div class="modal-header">' +
            '<h5 class="modal-title">پیشنهادات پروتکل</h5>' +
            '<button type="button" class="close" data-dismiss="modal">' +
            '<span>&times;</span>' +
            '</button>' +
            '</div>' +
            '<div class="modal-body">' +
            '<ul class="list-group" id="suggestions-list"></ul>' +
            '</div>' +
            '<div class="modal-footer">' +
            '<button type="button" class="btn btn-secondary" data-dismiss="modal">بستن</button>' +
            '</div>' +
            '</div>' +
            '</div>' +
            '</div>';
        
        $('body').append(modalHtml);
        return $('#protocol-suggestions-modal');
    },

    /**
     * ایجاد آیتم پیشنهاد پروتکل
     */
    createProtocolSuggestionItem: function(suggestion) {
        var protocolTypeBadge = this.getProtocolTypeBadge(suggestion.ProtocolType);
        var priorityBadge = this.getPriorityBadge(suggestion.Priority);
        
        return '<li class="list-group-item">' +
            '<div class="d-flex w-100 justify-content-between">' +
            '<h6 class="mb-1">' + suggestion.Name + '</h6>' +
            '<small>' + protocolTypeBadge + ' ' + priorityBadge + '</small>' +
            '</div>' +
            '<p class="mb-1">' + suggestion.Description + '</p>' +
            '<small>' + suggestion.Criteria + '</small>' +
            '<div class="mt-2">' +
            '<button type="button" class="btn btn-primary btn-sm" onclick="TriageProtocol.applyProtocolToAssessment(' + suggestion.AssessmentId + ', ' + suggestion.TriageProtocolId + ')">' +
            '<i class="fas fa-check"></i> اعمال' +
            '</button>' +
            '</div>' +
            '</li>';
    },

    /**
     * مدیریت خطا
     */
    handleError: function(error, context) {
        console.error('TriageProtocol Error [' + context + ']:', error);
        toastr.error('خطا در ' + context + ': ' + error.message);
    }
};

// راه‌اندازی خودکار
$(document).ready(function() {
    if (typeof TriageProtocol !== 'undefined') {
        TriageProtocol.init();
    }
});
