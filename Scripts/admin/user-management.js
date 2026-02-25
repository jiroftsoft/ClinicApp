/**
 * User Management JavaScript
 * مدیریت تعاملات و عملیات مدیریت کاربران — بهینه‌سازی برای پروداکشن درمانی
 * 
 * طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md, DEVELOPMENT_CONTRACT.md
 * 
 * ویژگی‌های کلیدی:
 * - مدیریت Search & Filter
 * - مدیریت Activate/Deactivate
 * - مدیریت Delete (Soft Delete)
 * - مدیریت Role Assignment
 * - Real-time Validation
 * - AJAX Calls
 * - بدون لوگ حساس در پروداکشن (فقط در حالت DEBUG)
 */

var UserManagement = (function() {
    var DEBUG = typeof window.USER_MANAGEMENT_DEBUG !== 'undefined' && window.USER_MANAGEMENT_DEBUG;

    function log() {
        if (DEBUG && typeof console !== 'undefined' && console.log) {
            console.log.apply(console, arguments);
        }
    }

    return {
    config: {
        apiBaseUrl: '/Admin/UserManagement',
        debounceTime: 300
    },

    init: function() {
        var self = this;
        log('Initializing User Management...');
        this.setupEventListeners();
        log('User Management initialized');
    },

    initRestore: function() {
        log('Initializing Restore functionality...');
        // ✅ Restore Button
        $(document).off('click.userManagementRestore', '.btn-restore-user').on('click.userManagementRestore', '.btn-restore-user', function(e) {
            e.preventDefault();
            e.stopPropagation();
            var userId = $(this).data('user-id');
            var userName = $(this).data('user-name') || 'کاربر';
            self.handleRestore(userId, userName);
        });
        log('Restore functionality initialized');
    },

    // Setup Event Listeners
    setupEventListeners: function() {
        var self = this;

        // ✅ Delete Button (با SweetAlert2)
        $(document).off('click.userManagement', '.btn-delete').on('click.userManagement', '.btn-delete', function(e) {
            e.preventDefault();
            e.stopPropagation();
            var userId = $(this).data('user-id');
            var userName = $(this).data('user-name') || 'کاربر';
            self.handleDelete(userId, userName);
        });

        // ✅ Activate Button
        $(document).off('click.userManagement', '.btn-activate').on('click.userManagement', '.btn-activate', function(e) {
            e.preventDefault();
            e.stopPropagation();
            var userId = $(this).data('user-id');
            self.handleActivate(userId);
        });

        // ✅ Deactivate Button
        $(document).off('click.userManagement', '.btn-deactivate').on('click.userManagement', '.btn-deactivate', function(e) {
            e.preventDefault();
            e.stopPropagation();
            var userId = $(this).data('user-id');
            self.handleDeactivate(userId);
        });

        // ✅ Search Input (با Debounce)
        var searchTimeout;
        $('#searchForm input[name="filter.SearchTerm"]').on('input', function() {
            clearTimeout(searchTimeout);
            var searchTerm = $(this).val();
            searchTimeout = setTimeout(function() {
                // Auto-submit form after debounce
                if (searchTerm.length >= 3 || searchTerm.length === 0) {
                    $('#searchForm').submit();
                }
            }, self.config.debounceTime);
        });
    },

    // Handle Delete
    handleDelete: function(userId, userName) {
        var self = this;

        if (!userId) {
            if (DEBUG) console.error('UserManagement: UserId برای حذف موجود نیست');
            toastr.error('شناسه کاربر برای حذف موجود نیست', 'خطا');
            return;
        }

        // ✅ SweetAlert2 Confirmation
        Swal.fire({
            title: 'آیا مطمئن هستید؟',
            html: `<p>آیا می‌خواهید کاربر <strong>${userName}</strong> را حذف کنید؟</p><p class="text-danger"><small>⚠️ این عملیات قابل بازگشت است (Soft Delete)</small></p>`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'بله، حذف کن',
            cancelButtonText: 'انصراف',
            reverseButtons: true
        }).then(function(result) {
            if (result.isConfirmed) {
                self.performDelete(userId);
            }
        });
    },

    // Perform Delete
    performDelete: function(userId) {
        var self = this;

        log('حذف کاربر در حال انجام');

        // ✅ Show loading
        Swal.fire({
            title: 'در حال حذف...',
            text: 'لطفاً صبر کنید',
            allowOutsideClick: false,
            didOpen: function() {
                Swal.showLoading();
            }
        });

        // ✅ AJAX Call
        var form = $('#deleteForm_' + userId);
        var token = form.length > 0 ? form.find('input[name="__RequestVerificationToken"]').val() : $('input[name="__RequestVerificationToken"]').first().val();
        
        $.ajax({
            url: self.config.apiBaseUrl + '/Delete',
            type: 'POST',
            data: {
                id: userId,
                __RequestVerificationToken: token
            },
            success: function(response) {
                Swal.close();
                toastr.success('کاربر با موفقیت حذف شد', 'موفقیت');
                
                // ✅ Reload page after delay
                setTimeout(function() {
                    window.location.reload();
                }, 1500);
            },
            error: function(xhr, status, error) {
                Swal.close();
                var errorMessage = 'خطا در حذف کاربر';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                toastr.error(errorMessage, 'خطا');
                if (DEBUG) console.error('UserManagement: خطا در حذف کاربر', error);
            }
        });
    },

    // Handle Activate
    handleActivate: function(userId) {
        var self = this;

        if (!userId) {
            if (DEBUG) console.error('UserManagement: UserId برای فعال‌سازی موجود نیست');
            toastr.error('شناسه کاربر برای فعال‌سازی موجود نیست', 'خطا');
            return;
        }

        var token = $('input[name="__RequestVerificationToken"]').first().val();
        if (!token) {
            if (DEBUG) console.error('UserManagement: AntiForgeryToken یافت نشد');
            toastr.error('خطا در دریافت توکن امنیتی', 'خطا');
            return;
        }
        
        $.ajax({
            url: self.config.apiBaseUrl + '/Activate',
            type: 'POST',
            data: {
                id: userId,
                __RequestVerificationToken: token
            },
            dataType: 'json',
            success: function(response) {
                if (response && response.success === true) {
                    var message = response.message || 'کاربر با موفقیت فعال شد';
                    toastr.success(message, 'موفقیت');
                    setTimeout(function() {
                        window.location.reload();
                    }, 1500);
                } else {
                    var errorMessage = (response && response.message) ? response.message : 'خطا در فعال‌سازی کاربر';
                    toastr.error(errorMessage, 'خطا');
                }
            },
            error: function(xhr, status, error) {
                var errorMessage = 'خطا در فعال‌سازی کاربر';
                try {
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    } else if (xhr.responseText) {
                        var parsed = JSON.parse(xhr.responseText);
                        if (parsed && parsed.message) {
                            errorMessage = parsed.message;
                        }
                    }
                } catch (e) { /* نادیده در پروداکشن */ }
                toastr.error(errorMessage, 'خطا');
            }
        });
    },

    // Handle Deactivate
    handleDeactivate: function(userId) {
        var self = this;

        if (!userId) {
            if (DEBUG) console.error('UserManagement: UserId برای غیرفعال‌سازی موجود نیست');
            toastr.error('شناسه کاربر برای غیرفعال‌سازی موجود نیست', 'خطا');
            return;
        }

        // ✅ SweetAlert2 Confirmation
        Swal.fire({
            title: 'آیا مطمئن هستید؟',
            text: 'آیا می‌خواهید این کاربر را غیرفعال کنید؟',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#ffc107',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'بله، غیرفعال کن',
            cancelButtonText: 'انصراف',
            reverseButtons: true
        }).then(function(result) {
            if (result.isConfirmed) {
                self.performDeactivate(userId);
            }
        });
    },

    // Perform Deactivate
    performDeactivate: function(userId) {
        var self = this;

        var token = $('input[name="__RequestVerificationToken"]').first().val();
        if (!token) {
            if (DEBUG) console.error('UserManagement: AntiForgeryToken یافت نشد');
            toastr.error('خطا در دریافت توکن امنیتی', 'خطا');
            return;
        }
        
        $.ajax({
            url: self.config.apiBaseUrl + '/Deactivate',
            type: 'POST',
            data: {
                id: userId,
                __RequestVerificationToken: token
            },
            dataType: 'json',
            success: function(response) {
                if (response && response.success === true) {
                    var message = response.message || 'کاربر با موفقیت غیرفعال شد';
                    toastr.success(message, 'موفقیت');
                    setTimeout(function() {
                        window.location.reload();
                    }, 1500);
                } else {
                    var errorMessage = (response && response.message) ? response.message : 'خطا در غیرفعال‌سازی کاربر';
                    toastr.error(errorMessage, 'خطا');
                }
            },
            error: function(xhr, status, error) {
                var errorMessage = 'خطا در غیرفعال‌سازی کاربر';
                try {
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    } else if (xhr.responseText) {
                        var parsed = JSON.parse(xhr.responseText);
                        if (parsed && parsed.message) {
                            errorMessage = parsed.message;
                        }
                    }
                } catch (e) { /* نادیده در پروداکشن */ }
                toastr.error(errorMessage, 'خطا');
            }
        });
    },

    handleRestore: function(userId, userName) {
        var self = this;

        // ✅ Confirmation با SweetAlert2
        Swal.fire({
            title: 'بازگردانی کاربر',
            html: `<p>آیا از بازگردانی کاربر <strong>${userName}</strong> اطمینان دارید؟</p>
                   <p class="text-muted" style="font-size: 0.9rem; margin-top: 0.5rem;">
                       <i class="fas fa-info-circle"></i> کاربر به لیست کاربران فعال بازمی‌گردد.
                   </p>`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            confirmButtonText: '<i class="fas fa-check"></i> بله، بازگردانی',
            cancelButtonText: '<i class="fas fa-times"></i> انصراف',
            reverseButtons: true,
            focusCancel: true
        }).then(function(result) {
            if (result.isConfirmed) {
                self.executeRestore(userId, userName);
            }
        });
    },

    executeRestore: function(userId, userName) {
        var self = this;

        // ✅ نمایش Loading
        Swal.fire({
            title: 'در حال بازگردانی...',
            html: 'لطفاً صبر کنید',
            allowOutsideClick: false,
            allowEscapeKey: false,
            showConfirmButton: false,
            didOpen: function() {
                Swal.showLoading();
            }
        });

        // ✅ دریافت AntiForgeryToken
        var token = $('input[name="__RequestVerificationToken"]').first().val();
        if (!token) {
            if (DEBUG) console.error('UserManagement: AntiForgeryToken یافت نشد');
            Swal.fire({
                title: 'خطا',
                text: 'خطا در دریافت توکن امنیتی. لطفاً صفحه را نوسازی کنید.',
                icon: 'error',
                confirmButtonText: 'باشه',
                confirmButtonColor: '#dc3545'
            });
            return;
        }

        // ✅ AJAX Call
        $.ajax({
            url: self.config.apiBaseUrl + '/Restore',
            type: 'POST',
            data: {
                id: userId,
                __RequestVerificationToken: token
            },
            dataType: 'json',
            success: function(response) {
                if (response && response.success === true) {
                    var message = response.message || 'کاربر با موفقیت بازگردانی شد';

                    Swal.fire({
                        title: 'موفقیت',
                        text: message,
                        icon: 'success',
                        confirmButtonText: 'باشه',
                        confirmButtonColor: '#28a745'
                    }).then(function() {
                        // Reload page
                        window.location.reload();
                    });
                } else {
                    var errorMessage = response ? (response.message || 'خطا در بازگردانی کاربر') : 'پاسخ نامعتبر از سرور';

                    Swal.fire({
                        title: 'خطا',
                        text: errorMessage,
                        icon: 'error',
                        confirmButtonText: 'باشه',
                        confirmButtonColor: '#dc3545'
                    });
                }
            },
            error: function(xhr, status, error) {
                var errorMessage = 'خطا در ارتباط با سرور';
                try {
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    } else if (xhr.responseText) {
                        var parsed = JSON.parse(xhr.responseText);
                        if (parsed && parsed.message) {
                            errorMessage = parsed.message;
                        }
                    }
                } catch (e) { /* نادیده در پروداکشن */ }

                Swal.fire({
                    title: 'خطا',
                    text: errorMessage,
                    icon: 'error',
                    confirmButtonText: 'باشه',
                    confirmButtonColor: '#dc3545'
                });
            }
        });
    }
    };
})();

window.UserManagement = UserManagement;

