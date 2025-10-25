/**
 * ANTI-FORGERY AJAX INTEGRATION - کلینیک شفا
 * ===========================================
 * 
 * این اسکریپت مسئولیت‌های زیر را دارد:
 * - تزریق خودکار Anti-Forgery Token به درخواست‌های AJAX
 * - مدیریت خطاهای Anti-Forgery
 * - پشتیبانی از تمام jQuery AJAX methods
 * 
 * @author ClinicApp Development Team
 * @version 1.0.0
 * @since 2025-01-17
 */

(function($) {
    'use strict';

    // ========================================
    // CONFIGURATION - تنظیمات
    // ========================================
    var CONFIG = {
        tokenName: '__RequestVerificationToken',
        headerName: 'RequestVerificationToken',
        errorMessage: 'توکن امنیتی منقضی شده است. لطفا صفحه را بازخوانی کنید.',
        retryCount: 1,
        retryDelay: 1000
    };

    // ========================================
    // ANTI-FORGERY TOKEN MANAGEMENT - مدیریت توکن
    // ========================================
    var AntiForgeryManager = {
        
        // دریافت توکن از فرم
        getToken: function() {
            var token = $('input[name="' + CONFIG.tokenName + '"]').val();
            if (!token) {
                console.warn('[AntiForgery] Token not found in form');
            }
            return token;
        },

        // دریافت توکن از meta tag
        getTokenFromMeta: function() {
            var metaToken = $('meta[name="' + CONFIG.tokenName + '"]').attr('content');
            if (!metaToken) {
                console.warn('[AntiForgery] Token not found in meta tag');
            }
            return metaToken;
        },

        // دریافت توکن (اولویت: فرم، سپس meta)
        getValidToken: function() {
            return this.getToken() || this.getTokenFromMeta();
        },

        // بررسی وجود توکن
        hasToken: function() {
            return !!this.getValidToken();
        }
    };

    // ========================================
    // AJAX INTERCEPTOR - رهگیر AJAX
    // ========================================
    var AjaxInterceptor = {
        
        // تنظیم قبل از ارسال درخواست
        beforeSend: function(xhr, settings) {
            // فقط برای POST, PUT, DELETE, PATCH
            if (this.shouldAddToken(settings)) {
                var token = AntiForgeryManager.getValidToken();
                if (token) {
                    xhr.setRequestHeader(CONFIG.headerName, token);
                    console.debug('[AntiForgery] Token added to request:', settings.url);
                } else {
                    console.error('[AntiForgery] No token available for request:', settings.url);
                }
            }
        },

        // بررسی نیاز به اضافه کردن توکن
        shouldAddToken: function(settings) {
            var method = (settings.type || 'GET').toUpperCase();
            return ['POST', 'PUT', 'DELETE', 'PATCH'].includes(method);
        },

        // مدیریت خطاهای Anti-Forgery
        errorHandler: function(xhr, status, error) {
            if (xhr.status === 400 && xhr.responseText && 
                xhr.responseText.includes('AntiForgery')) {
                
                console.error('[AntiForgery] Token validation failed');
                this.handleTokenError();
                return true;
            }
            return false;
        },

        // مدیریت خطای توکن
        handleTokenError: function() {
            // نمایش پیام خطا
            if (window.toastr) {
                toastr.error(CONFIG.errorMessage, 'خطای امنیتی');
            } else {
                alert(CONFIG.errorMessage);
            }

            // بازخوانی صفحه پس از تاخیر
            setTimeout(function() {
                window.location.reload();
            }, 2000);
        }
    };

    // ========================================
    // JQUERY AJAX SETUP - تنظیم jQuery AJAX
    // ========================================
    $(document).ready(function() {
        
        // تنظیم global AJAX settings
        $.ajaxSetup({
            beforeSend: function(xhr, settings) {
                AjaxInterceptor.beforeSend.call(this, xhr, settings);
            },
            error: function(xhr, status, error) {
                if (!AjaxInterceptor.errorHandler.call(this, xhr, status, error)) {
                    // خطای عادی - به handler اصلی ارسال شود
                    return true;
                }
            }
        });

        // تنظیم برای $.post, $.get, $.ajax
        var originalPost = $.post;
        var originalGet = $.get;
        var originalAjax = $.ajax;

        // Override $.post
        $.post = function(url, data, success, dataType) {
            return originalAjax({
                url: url,
                type: 'POST',
                data: data,
                success: success,
                dataType: dataType
            });
        };

        // Override $.get
        $.get = function(url, data, success, dataType) {
            return originalAjax({
                url: url,
                type: 'GET',
                data: data,
                success: success,
                dataType: dataType
            });
        };

        console.log('[AntiForgery] AJAX integration initialized');
    });

    // ========================================
    // UTILITY FUNCTIONS - توابع کمکی
    // ========================================
    window.AntiForgeryAjax = {
        
        // بررسی وضعیت توکن
        checkTokenStatus: function() {
            return {
                hasToken: AntiForgeryManager.hasToken(),
                token: AntiForgeryManager.getValidToken()
            };
        },

        // دریافت توکن جدید
        refreshToken: function() {
            return $.get('/AntiForgery/RefreshToken').then(function(response) {
                if (response.success) {
                    // به‌روزرسانی توکن در فرم
                    $('input[name="' + CONFIG.tokenName + '"]').val(response.token);
                    console.log('[AntiForgery] Token refreshed');
                }
            });
        },

        // تنظیم توکن دستی
        setToken: function(token) {
            $('input[name="' + CONFIG.tokenName + '"]').val(token);
            $('meta[name="' + CONFIG.tokenName + '"]').attr('content', token);
        },

        // دریافت توکن فعلی
        getCurrentToken: function() {
            return AntiForgeryManager.getValidToken();
        }
    };

    // ========================================
    // FORM SUBMISSION HANDLER - مدیریت ارسال فرم
    // ========================================
    $(document).on('submit', 'form', function(e) {
        var $form = $(this);
        var method = $form.attr('method') || 'GET';
        
        // برای POST forms، بررسی وجود توکن
        if (method.toUpperCase() === 'POST') {
            if (!AntiForgeryManager.hasToken()) {
                console.error('[AntiForgery] No token found for form submission');
                e.preventDefault();
                
                if (window.toastr) {
                    toastr.error('توکن امنیتی یافت نشد. لطفا صفحه را بازخوانی کنید.', 'خطای امنیتی');
                }
                return false;
            }
        }
    });

    // ========================================
    // DEBUGGING - دیباگ
    // ========================================
    if (window.console && console.debug) {
        console.debug('[AntiForgery] Module loaded successfully');
        console.debug('[AntiForgery] Token status:', window.AntiForgeryAjax.checkTokenStatus());
    }

})(jQuery);
