/**
 * User Management JavaScript
 * مدیریت تعاملات و عملیات مدیریت کاربران
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
 */

var UserManagement = {
    // تنظیمات
    config: {
        apiBaseUrl: '/Admin/UserManagement',
        debounceTime: 300 // ms
    },

    // Initialize
    init: function() {
        var self = this;

        console.log('✅ Initializing User Management...');

        // Setup event listeners
        this.setupEventListeners();

        console.log('✅ User Management initialized successfully');
    },

    // Initialize Restore functionality
    initRestore: function() {
        var self = this;

        console.log('✅ Initializing Restore functionality...');

        // ✅ Restore Button
        $(document).off('click.userManagementRestore', '.btn-restore-user').on('click.userManagementRestore', '.btn-restore-user', function(e) {
            e.preventDefault();
            e.stopPropagation();
            var userId = $(this).data('user-id');
            var userName = $(this).data('user-name') || 'کاربر';
            self.handleRestore(userId, userName);
        });

        console.log('✅ Restore functionality initialized');
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
            console.error('❌ UserManagement: UserId برای حذف موجود نیست');
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

        console.log('🗑️ UserManagement: حذف کاربر - UserId:', userId);

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
                console.error('❌ UserManagement: خطا در حذف کاربر:', error);
            }
        });
    },

    // Handle Activate
    handleActivate: function(userId) {
        var self = this;

        if (!userId) {
            console.error('❌ UserManagement: UserId برای فعال‌سازی موجود نیست');
            toastr.error('شناسه کاربر برای فعال‌سازی موجود نیست', 'خطا');
            return;
        }

        console.log('✅ UserManagement: فعال‌سازی کاربر - UserId:', userId);

        // ✅ AJAX Call
        var token = $('input[name="__RequestVerificationToken"]').first().val();
        if (!token) {
            console.error('❌ UserManagement: AntiForgeryToken یافت نشد');
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
                console.log('✅ UserManagement: Response received:', response);
                
                // ✅ بررسی دقیق response
                if (response && response.success === true) {
                    var message = response.message || 'کاربر با موفقیت فعال شد';
                    console.log('✅ UserManagement: فعال‌سازی موفق - Message:', message);
                    toastr.success(message, 'موفقیت');
                    setTimeout(function() {
                        window.location.reload();
                    }, 1500);
                } else {
                    var errorMessage = (response && response.message) ? response.message : 'خطا در فعال‌سازی کاربر';
                    console.error('❌ UserManagement: فعال‌سازی ناموفق - Response:', response);
                    toastr.error(errorMessage, 'خطا');
                }
            },
            error: function(xhr, status, error) {
                console.error('❌ UserManagement: AJAX Error - Status:', status, 'Error:', error);
                console.error('❌ UserManagement: Response Text:', xhr.responseText);
                
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
                } catch (e) {
                    console.error('❌ UserManagement: خطا در parse کردن response:', e);
                }
                
                toastr.error(errorMessage, 'خطا');
            }
        });
    },

    // Handle Deactivate
    handleDeactivate: function(userId) {
        var self = this;

        if (!userId) {
            console.error('❌ UserManagement: UserId برای غیرفعال‌سازی موجود نیست');
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

        console.log('⚠️ UserManagement: غیرفعال‌سازی کاربر - UserId:', userId);

        // ✅ AJAX Call
        var token = $('input[name="__RequestVerificationToken"]').first().val();
        if (!token) {
            console.error('❌ UserManagement: AntiForgeryToken یافت نشد');
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
                console.log('✅ UserManagement: Response received:', response);
                
                // ✅ بررسی دقیق response
                if (response && response.success === true) {
                    var message = response.message || 'کاربر با موفقیت غیرفعال شد';
                    console.log('✅ UserManagement: غیرفعال‌سازی موفق - Message:', message);
                    toastr.success(message, 'موفقیت');
                    setTimeout(function() {
                        window.location.reload();
                    }, 1500);
                } else {
                    var errorMessage = (response && response.message) ? response.message : 'خطا در غیرفعال‌سازی کاربر';
                    console.error('❌ UserManagement: غیرفعال‌سازی ناموفق - Response:', response);
                    toastr.error(errorMessage, 'خطا');
                }
            },
            error: function(xhr, status, error) {
                console.error('❌ UserManagement: AJAX Error - Status:', status, 'Error:', error);
                console.error('❌ UserManagement: Response Text:', xhr.responseText);
                
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
                } catch (e) {
                    console.error('❌ UserManagement: خطا در parse کردن response:', e);
                }
                
                toastr.error(errorMessage, 'خطا');
            }
        });
    },

    // ✅ Handle Restore
    handleRestore: function(userId, userName) {
        var self = this;

        console.log('🔄 UserManagement: درخواست بازگردانی کاربر - UserId:', userId, 'UserName:', userName);

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
            } else {
                console.log('❌ UserManagement: بازگردانی لغو شد');
            }
        });
    },

    // ✅ Execute Restore (AJAX)
    executeRestore: function(userId, userName) {
        var self = this;

        console.log('🔄 UserManagement: اجرای بازگردانی - UserId:', userId);

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
            console.error('❌ UserManagement: AntiForgeryToken یافت نشد');
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
                console.log('📥 UserManagement: پاسخ بازگردانی دریافت شد:', response);

                if (response && response.success === true) {
                    var message = response.message || 'کاربر با موفقیت بازگردانی شد';
                    console.log('✅ UserManagement: بازگردانی موفق - Message:', message);

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
                    console.error('❌ UserManagement: خطا در بازگردانی کاربر - Message:', errorMessage, 'Response:', response);
                    
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
                console.error('❌ UserManagement: خطا در AJAX بازگردانی - Status:', status, 'Error:', error, 'Response:', xhr.responseText);

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
                } catch (e) {
                    console.error('❌ UserManagement: خطا در parse کردن response:', e);
                }

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

// ✅ Export to window
window.UserManagement = UserManagement;

