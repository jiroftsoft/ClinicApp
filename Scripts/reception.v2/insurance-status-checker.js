/**
 * کامپوننت قابل استفاده مجدد برای بررسی وضعیت بیمه
 * 
 * این کامپوننت برای نمایش هشدارهای واضح به منشی‌ها در فرم پذیرش طراحی شده است
 * 
 * ویژگی‌های کلیدی:
 * 1. بررسی خودکار وضعیت بیمه هنگام انتخاب بیمار
 * 2. نمایش هشدارهای واضح برای منشی‌ها
 * 3. Modal برای هشدارهای بحرانی
 * 4. قابل استفاده مجدد در ماژول‌های مختلف
 */

(function (window, $) {
    'use strict';

    // Namespace
    var ns = window.ReceptionV2 || {};
    ns.InsuranceStatusChecker = ns.InsuranceStatusChecker || {};

    var API_BASE = '/api/v1/reception';

    /**
     * بررسی وضعیت بیمه بیمار
     * @param {number} patientId - شناسه بیمار
     * @param {Date} checkDate - تاریخ بررسی (اختیاری)
     * @param {function} onSuccess - callback برای موفقیت
     * @param {function} onError - callback برای خطا
     */
    function checkInsuranceStatus(patientId, checkDate, onSuccess, onError) {
        if (!patientId || patientId <= 0) {
            console.warn('🏥 InsuranceStatusChecker: PatientId نامعتبر');
            if (onError) onError({ message: 'شناسه بیمار نامعتبر است' });
            return;
        }

        console.log('🔍 InsuranceStatusChecker: بررسی وضعیت بیمه. PatientId:', patientId);

        var payload = {
            patientId: patientId,
            checkDate: checkDate ? checkDate.toISOString() : null
        };

        // دریافت anti-forgery token
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (!token) {
            console.error('🏥 InsuranceStatusChecker: Anti-forgery token یافت نشد');
            if (onError) onError({ message: 'توکن امنیتی یافت نشد. صفحه را نوسازی کنید.' });
            return;
        }

        // ✅ استفاده از reception-api.js برای consistency و fallback
        if (window.ReceptionAPI && typeof window.ReceptionAPI.post === 'function') {
            // استفاده از API wrapper که fallback دارد
            window.ReceptionAPI.post('/insurance/check-status', payload)
                .then(function (fullResponse) {
                    console.log('✅ InsuranceStatusChecker: پاسخ دریافت شد:', fullResponse);

                    // ✅ استفاده از API.ok برای extract کردن Data
                    var response = window.ReceptionAPI && typeof window.ReceptionAPI.ok === 'function'
                        ? window.ReceptionAPI.ok(fullResponse)
                        : fullResponse;

                    // بررسی Success
                    var successValue = fullResponse?.Success ?? fullResponse?.success;
                    var isSuccess = successValue === true || successValue === "true" || successValue === 1;

                    if (isSuccess && response) {
                        // response می‌تواند مستقیم InsuranceStatusCheckResult باشد یا در Data باشد
                        var status = response.Data || response.data || response;
                        
                        if (status && (status.PatientId || status.patientId)) {
                            displayInsuranceStatus(status);
                            
                            if (onSuccess) {
                                onSuccess(status);
                            }
                        } else {
                            console.warn('⚠️ InsuranceStatusChecker: ساختار پاسخ نامعتبر:', response);
                            if (onError) {
                                onError(fullResponse || { message: 'ساختار پاسخ نامعتبر است' });
                            }
                        }
                    } else {
                        // خطا از سرور
                        var errorMsg = fullResponse?.Message || fullResponse?.message || 'خطا در بررسی وضعیت بیمه';
                        var errorCode = fullResponse?.Code || fullResponse?.code || 'UNKNOWN_ERROR';
                        
                        console.warn('⚠️ InsuranceStatusChecker: خطا از سرور:', errorMsg, errorCode, fullResponse);
                        
                        // بررسی خطاهای خاص
                        if (errorCode === 'ANTIFORGERY_MISSING') {
                            if (window.ReceptionAPI && typeof window.ReceptionAPI.handleErrorJson === 'function') {
                                window.ReceptionAPI.handleErrorJson(fullResponse);
                            }
                        }
                        
                        if (onError) {
                            onError(fullResponse || { message: errorMsg, code: errorCode });
                        }
                    }
                })
                .catch(function (err) {
                    console.error('❌ InsuranceStatusChecker: خطا در بررسی وضعیت بیمه:', err);
                    if (onError) {
                        onError(err.responseJSON || { message: 'خطا در بررسی وضعیت بیمه' });
                    }
                });
            return;
        }

        // Fallback به AJAX مستقیم
        $.ajax({
            url: API_BASE + '/insurance/check-status',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            headers: {
                'RequestVerificationToken': token,
                'X-RequestVerificationToken': token,
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (fullResponse) {
                console.log('✅ InsuranceStatusChecker: پاسخ دریافت شد:', fullResponse);

                // بررسی Success
                var successValue = fullResponse?.Success ?? fullResponse?.success;
                var isSuccess = successValue === true || successValue === "true" || successValue === 1;

                if (isSuccess && fullResponse) {
                    // response می‌تواند مستقیم InsuranceStatusCheckResult باشد یا در Data باشد
                    var status = fullResponse.Data || fullResponse.data || fullResponse;
                    
                    if (status && (status.PatientId || status.patientId)) {
                        displayInsuranceStatus(status);
                        
                        if (onSuccess) {
                            onSuccess(status);
                        }
                    } else {
                        console.warn('⚠️ InsuranceStatusChecker: ساختار پاسخ نامعتبر:', fullResponse);
                        if (onError) {
                            onError(fullResponse || { message: 'ساختار پاسخ نامعتبر است' });
                        }
                    }
                } else {
                    // خطا از سرور
                    var errorMsg = fullResponse?.Message || fullResponse?.message || 'خطا در بررسی وضعیت بیمه';
                    var errorCode = fullResponse?.Code || fullResponse?.code || 'UNKNOWN_ERROR';
                    
                    console.warn('⚠️ InsuranceStatusChecker: خطا از سرور:', errorMsg, errorCode, fullResponse);
                    
                    // بررسی خطاهای خاص
                    if (errorCode === 'ANTIFORGERY_MISSING') {
                        if (window.toastr) {
                            toastr.error('توکن امنیتی منقضی شده است. لطفاً صفحه را نوسازی کنید.', 'خطای امنیتی', {
                                timeOut: 5000,
                                extendedTimeOut: 3000
                            });
                        }
                        // پیشنهاد Refresh
                        setTimeout(function() {
                            if (confirm('آیا می‌خواهید صفحه را نوسازی کنید؟')) {
                                window.location.reload();
                            }
                        }, 1000);
                    }
                    
                    if (onError) {
                        onError(fullResponse || { message: errorMsg, code: errorCode });
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error('❌ InsuranceStatusChecker: خطا در بررسی وضعیت بیمه:', {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    error: error,
                    responseText: xhr.responseText
                });

                if (onError) {
                    var errorMessage = 'خطا در بررسی وضعیت بیمه';
                    if (xhr.status === 0) {
                        errorMessage = 'اتصال به سرور برقرار نیست';
                    } else if (xhr.status === 500) {
                        errorMessage = 'خطای داخلی سرور';
                    }
                    onError({ message: errorMessage, xhr: xhr });
                }
            }
        });
    }

    /**
     * نمایش وضعیت بیمه به کاربر
     * @param {object} status - نتیجه بررسی وضعیت بیمه
     */
    function displayInsuranceStatus(status) {
        if (!status) {
            console.warn('🏥 InsuranceStatusChecker: وضعیت بیمه null است');
            return;
        }

        console.log('🏥 InsuranceStatusChecker: نمایش وضعیت بیمه:', status);

        // حذف هشدارهای قبلی
        removeInsuranceStatusAlerts();

        // بررسی هشدارهای بحرانی (باید به صورت Modal نمایش داده شوند)
        var criticalAlerts = status.Alerts.filter(function (alert) {
            return alert.Severity === 2 && alert.ShowAsModal; // Critical severity
        });

        if (criticalAlerts.length > 0) {
            // نمایش Modal برای هشدارهای بحرانی
            showCriticalInsuranceAlert(criticalAlerts[0], status);
        }

        // نمایش هشدارهای Warning
        var warningAlerts = status.Alerts.filter(function (alert) {
            return alert.Severity === 1; // Warning severity
        });

        if (warningAlerts.length > 0) {
            showWarningInsuranceAlert(warningAlerts[0], status);
        }

        // نمایش هشدارهای Info
        var infoAlerts = status.Alerts.filter(function (alert) {
            return alert.Severity === 0; // Info severity
        });

        if (infoAlerts.length > 0) {
            showInfoInsuranceAlert(infoAlerts[0], status);
        }

        // نمایش Badge وضعیت در UI
        updateInsuranceStatusBadge(status);
    }

    /**
     * تبدیل تاریخ میلادی به شمسی (ساده)
     */
    function formatDateToPersian(dateStr) {
        if (!dateStr) return '—';
        try {
            var date = new Date(dateStr);
            if (isNaN(date.getTime())) return '—';
            
            // استفاده از کتابخانه persian-date اگر موجود باشد
            if (typeof persianDate !== 'undefined' && persianDate.toPersian) {
                var pDate = persianDate.toPersian(date);
                return pDate.format('YYYY/MM/DD');
            }
            
            // Fallback: نمایش تاریخ میلادی به صورت ساده
            return date.toLocaleDateString('fa-IR');
        } catch (e) {
            return '—';
        }
    }

    /**
     * ساخت HTML برای نمایش اطلاعات بیمه (برای منشی)
     */
    function buildInsuranceDetailsHtml(insurance, title) {
        if (!insurance || !insurance.Exists) {
            return '<div class="mb-3"><strong>' + title + ':</strong> <span class="text-muted">ثبت نشده</span></div>';
        }

        var html = '<div class="mb-3 border rounded p-3 bg-light">';
        html += '<strong class="d-block mb-2">' + title + '</strong>';
        
        // نام بیمه
        if (insurance.InsuranceName) {
            html += '<div class="mb-2"><i class="fas fa-shield-alt me-2 text-primary"></i><strong>نام بیمه:</strong> ' + insurance.InsuranceName + '</div>';
        }
        
        // شماره بیمه
        if (insurance.PolicyNumber) {
            html += '<div class="mb-2"><i class="fas fa-id-card me-2 text-info"></i><strong>شماره بیمه:</strong> ' + insurance.PolicyNumber + '</div>';
        }
        
        // تاریخ شروع
        if (insurance.StartDate) {
            var startDatePersian = formatDateToPersian(insurance.StartDate);
            html += '<div class="mb-2"><i class="fas fa-calendar-check me-2 text-success"></i><strong>تاریخ شروع:</strong> ' + startDatePersian + '</div>';
        }
        
        // تاریخ پایان
        if (insurance.EndDate) {
            var endDatePersian = formatDateToPersian(insurance.EndDate);
            var endDateClass = insurance.IsExpired ? 'text-danger' : (insurance.DaysRemaining && insurance.DaysRemaining <= 30 ? 'text-warning' : 'text-success');
            html += '<div class="mb-2"><i class="fas fa-calendar-times me-2 ' + endDateClass + '"></i><strong>تاریخ پایان:</strong> <span class="' + endDateClass + '">' + endDatePersian + '</span></div>';
        }
        
        // تعداد روزهای باقیمانده
        if (insurance.DaysRemaining !== null && insurance.DaysRemaining !== undefined) {
            var daysClass = insurance.IsExpired ? 'text-danger' : (insurance.DaysRemaining <= 30 ? 'text-warning' : 'text-success');
            var daysText = insurance.IsExpired 
                ? '<span class="' + daysClass + '"><strong>منقضی شده</strong></span>'
                : '<span class="' + daysClass + '"><strong>' + insurance.DaysRemaining + ' روز</strong> باقی مانده</span>';
            html += '<div class="mb-2"><i class="fas fa-clock me-2 ' + daysClass + '"></i><strong>وضعیت:</strong> ' + daysText + '</div>';
        }
        
        // وضعیت فعال/غیرفعال
        var activeClass = insurance.IsActive ? 'text-success' : 'text-danger';
        var activeIcon = insurance.IsActive ? 'fa-check-circle' : 'fa-times-circle';
        var activeText = insurance.IsActive ? 'فعال' : 'غیرفعال';
        html += '<div class="mb-2"><i class="fas ' + activeIcon + ' me-2 ' + activeClass + '"></i><strong>وضعیت:</strong> <span class="' + activeClass + '">' + activeText + '</span></div>';
        
        html += '</div>';
        return html;
    }

    /**
     * نمایش هشدار بحرانی (Modal) - با اطلاعات کامل برای منشی
     */
    function showCriticalInsuranceAlert(alert, status) {
        if (!window.Swal || typeof window.Swal.fire !== 'function') {
            // Fallback به alert ساده
            alert(alert.Title + '\n\n' + alert.Message);
            return;
        }

        // ساخت HTML کامل با اطلاعات بیمه
        var html = '<div class="text-right">';
        html += '<p class="mb-3 alert alert-danger"><strong>' + alert.Message + '</strong></p>';
        
        // اطلاعات بیمه پایه
        if (status.PrimaryInsurance) {
            html += buildInsuranceDetailsHtml(status.PrimaryInsurance, '📋 بیمه پایه');
        }
        
        // اطلاعات بیمه تکمیلی
        if (status.SupplementaryInsurance) {
            html += buildInsuranceDetailsHtml(status.SupplementaryInsurance, '📋 بیمه تکمیلی');
        }
        
        // توصیه‌ها
        if (status.Recommendations && status.Recommendations.length > 0) {
            html += '<div class="alert alert-info mt-3"><strong>💡 توصیه‌ها:</strong><ul class="mb-0 mt-2 text-right">';
            status.Recommendations.forEach(function (rec) {
                html += '<li class="text-right">' + rec + '</li>';
            });
            html += '</ul></div>';
        }
        
        // پیام تفصیلی
        if (status.DetailedMessage) {
            html += '<div class="alert alert-secondary mt-3"><small>' + status.DetailedMessage + '</small></div>';
        }
        
        html += '</div>';

        window.Swal.fire({
            icon: 'error',
            title: alert.Title,
            html: html,
            confirmButtonText: 'متوجه شدم',
            confirmButtonColor: '#dc3545',
            allowOutsideClick: false,
            allowEscapeKey: false,
            showCancelButton: false,
            width: '600px'
        }).then(function () {
            // اگر پذیرش را متوقف کند، می‌توانیم عملیات خاصی انجام دهیم
            if (alert.BlockReception) {
                console.warn('🏥 InsuranceStatusChecker: پذیرش متوقف شد به دلیل: ' + alert.Title);
                // می‌توانیم یک event trigger کنیم
                $(document).trigger('insurance:reception-blocked', [status]);
            }
        });
    }

    /**
     * نمایش هشدار Warning - با اطلاعات کامل برای منشی
     */
    function showWarningInsuranceAlert(alert, status) {
        // اگر بیمه در حال انقضا است، Modal نمایش بده
        if (status.HasExpiryWarning || (status.DaysUntilExpiry !== null && status.DaysUntilExpiry <= 30)) {
            if (window.Swal && typeof window.Swal.fire === 'function') {
                var html = '<div class="text-right">';
                html += '<p class="mb-3 alert alert-warning"><strong>' + alert.Message + '</strong></p>';
                
                // اطلاعات بیمه پایه
                if (status.PrimaryInsurance) {
                    html += buildInsuranceDetailsHtml(status.PrimaryInsurance, '📋 بیمه پایه');
                }
                
                // اطلاعات بیمه تکمیلی
                if (status.SupplementaryInsurance) {
                    html += buildInsuranceDetailsHtml(status.SupplementaryInsurance, '📋 بیمه تکمیلی');
                }
                
                // توصیه‌ها
                if (status.Recommendations && status.Recommendations.length > 0) {
                    html += '<div class="alert alert-info mt-3"><strong>💡 توصیه:</strong> ' + status.Recommendations[0] + '</div>';
                }
                
                html += '</div>';

                window.Swal.fire({
                    icon: 'warning',
                    title: alert.Title,
                    html: html,
                    confirmButtonText: 'متوجه شدم',
                    confirmButtonColor: '#ffc107',
                    allowOutsideClick: true,
                    width: '600px'
                });
                return;
            }
        }

        // نمایش Toast برای هشدارهای ساده‌تر
        if (window.toastr) {
            var toastMessage = alert.Message;
            // اضافه کردن اطلاعات مفید به Toast
            if (status.DaysUntilExpiry !== null && status.DaysUntilExpiry > 0) {
                toastMessage += ' (' + status.DaysUntilExpiry + ' روز باقی مانده)';
            }
            
            toastr.warning(
                toastMessage,
                alert.Title,
                {
                    timeOut: 10000, // 10 ثانیه
                    extendedTimeOut: 5000,
                    closeButton: true,
                    progressBar: true
                }
            );
        } else {
            console.warn('🏥 InsuranceStatusChecker: ' + alert.Title + ' - ' + alert.Message);
        }

        // نمایش Badge در UI
        showInsuranceStatusBadge(alert, 'warning');
    }

    /**
     * نمایش هشدار Info
     */
    function showInfoInsuranceAlert(alert, status) {
        // نمایش Toast
        if (window.toastr) {
            toastr.info(
                alert.Message,
                alert.Title,
                {
                    timeOut: 5000,
                    closeButton: true
                }
            );
        } else {
            console.info('🏥 InsuranceStatusChecker: ' + alert.Title + ' - ' + alert.Message);
        }

        // نمایش Badge در UI
        showInsuranceStatusBadge(alert, 'info');
    }

    /**
     * نمایش Badge وضعیت بیمه در UI - با اطلاعات کامل برای منشی
     */
    function updateInsuranceStatusBadge(status) {
        // پیدا کردن محل نمایش Badge (در فرم پذیرش)
        var $badgeContainer = $('#insurance-status-badge-container');
        if ($badgeContainer.length === 0) {
            // اگر container وجود ندارد، ایجاد می‌کنیم
            $badgeContainer = $('<div id="insurance-status-badge-container" class="mb-3"></div>');
            $('#insurance-panel .card-header').after($badgeContainer);
        }

        $badgeContainer.empty();

        var badgeClass = 'success';
        var badgeIcon = 'fas fa-check-circle';
        var badgeText = 'وضعیت بیمه معتبر است';
        var statusType = status.Status || 0;

        if (statusType === 1 || status.IsExpired) { // Expired
            badgeClass = 'danger';
            badgeIcon = 'fas fa-exclamation-triangle';
            badgeText = '⚠️ بیمه منقضی شده است';
        } else if (statusType === 2 || (status.DaysUntilExpiry !== null && status.DaysUntilExpiry <= 30 && status.DaysUntilExpiry > 0)) { // ExpiringSoon
            badgeClass = 'warning';
            badgeIcon = 'fas fa-exclamation-circle';
            badgeText = '⚠️ بیمه در حال انقضا است';
        } else if (statusType === 3) { // MissingPrimaryInsurance
            badgeClass = 'danger';
            badgeIcon = 'fas fa-times-circle';
            badgeText = '❌ بیمه پایه وجود ندارد';
        } else if (statusType === 4) { // Inactive
            badgeClass = 'danger';
            badgeIcon = 'fas fa-ban';
            badgeText = '❌ بیمه غیرفعال است';
        }

        // ساخت HTML با اطلاعات کامل
        var html = '<div class="alert alert-' + badgeClass + ' alert-dismissible fade show" role="alert">';
        html += '<div class="d-flex align-items-start">';
        html += '<i class="' + badgeIcon + ' me-2 mt-1"></i>';
        html += '<div class="flex-grow-1">';
        html += '<strong>' + badgeText + '</strong>';
        
        // اطلاعات بیمه پایه
        if (status.PrimaryInsurance && status.PrimaryInsurance.Exists) {
            var primary = status.PrimaryInsurance;
            html += '<div class="mt-2 small">';
            html += '<strong>📋 بیمه پایه:</strong> ' + (primary.InsuranceName || '—');
            if (primary.EndDate) {
                var endDatePersian = formatDateToPersian(primary.EndDate);
                var daysInfo = '';
                if (primary.DaysRemaining !== null && primary.DaysRemaining !== undefined) {
                    if (primary.IsExpired) {
                        daysInfo = ' <span class="text-danger">(منقضی شده)</span>';
                    } else {
                        daysInfo = ' <span class="text-warning">(' + primary.DaysRemaining + ' روز باقی مانده)</span>';
                    }
                }
                html += ' | <strong>پایان:</strong> ' + endDatePersian + daysInfo;
            }
            html += '</div>';
        }
        
        // اطلاعات بیمه تکمیلی
        if (status.SupplementaryInsurance && status.SupplementaryInsurance.Exists) {
            var supp = status.SupplementaryInsurance;
            html += '<div class="mt-1 small">';
            html += '<strong>📋 بیمه تکمیلی:</strong> ' + (supp.InsuranceName || '—');
            if (supp.EndDate) {
                var endDatePersian = formatDateToPersian(supp.EndDate);
                var daysInfo = '';
                if (supp.DaysRemaining !== null && supp.DaysRemaining !== undefined) {
                    if (supp.IsExpired) {
                        daysInfo = ' <span class="text-danger">(منقضی شده)</span>';
                    } else {
                        daysInfo = ' <span class="text-warning">(' + supp.DaysRemaining + ' روز باقی مانده)</span>';
                    }
                }
                html += ' | <strong>پایان:</strong> ' + endDatePersian + daysInfo;
            }
            html += '</div>';
        }
        
        // پیام اصلی
        if (status.MainMessage) {
            html += '<div class="mt-2"><small>' + status.MainMessage + '</small></div>';
        }
        
        html += '</div>';
        html += '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>';
        html += '</div>';
        html += '</div>';

        $badgeContainer.html(html);

        // اگر وضعیت بحرانی است، یک کلاس اضافه می‌کنیم
        if (!status.CanProceedWithReception) {
            $badgeContainer.addClass('insurance-blocked');
            $(document).trigger('insurance:reception-blocked', [status]);
        } else {
            $badgeContainer.removeClass('insurance-blocked');
        }
    }

    /**
     * نمایش Badge برای یک Alert خاص
     */
    function showInsuranceStatusBadge(alert, severity) {
        // این متد می‌تواند برای نمایش Badge‌های اضافی استفاده شود
        console.log('🏥 InsuranceStatusChecker: نمایش Badge - ' + alert.Title);
    }

    /**
     * حذف هشدارهای قبلی
     */
    function removeInsuranceStatusAlerts() {
        $('#insurance-status-badge-container').empty();
        // حذف Toast‌های قبلی (اختیاری)
        if (window.toastr) {
            toastr.clear();
        }
    }

    /**
     * بررسی خودکار وضعیت بیمه هنگام انتخاب بیمار
     */
    function autoCheckOnPatientSelect(patientId) {
        if (!patientId || patientId <= 0) {
            return;
        }

        console.log('🔍 InsuranceStatusChecker: بررسی خودکار وضعیت بیمه. PatientId:', patientId);

        checkInsuranceStatus(patientId, null, function (status) {
            console.log('✅ InsuranceStatusChecker: بررسی خودکار تکمیل شد', status);
        }, function (error) {
            console.error('❌ InsuranceStatusChecker: خطا در بررسی خودکار', error);
        });
    }

    // Public API
    ns.InsuranceStatusChecker = {
        check: checkInsuranceStatus,
        autoCheck: autoCheckOnPatientSelect,
        display: displayInsuranceStatus,
        removeAlerts: removeInsuranceStatusAlerts
    };

    // Export to global
    window.ReceptionV2 = ns;

    // Auto-initialize if patient is already selected
    $(document).ready(function () {
        // بررسی اینکه آیا بیمار قبلاً انتخاب شده است
        var patientId = $('#Patient_PatientId').val() || $('#PatientId').val();
        if (patientId && parseInt(patientId) > 0) {
            console.log('🏥 InsuranceStatusChecker: بیمار قبلاً انتخاب شده است. بررسی خودکار...');
            autoCheckOnPatientSelect(parseInt(patientId));
        }

        // گوش دادن به تغییرات PatientId
        $(document).on('patient:selected', function (e, patientData) {
            if (patientData && patientData.PatientId) {
                console.log('🏥 InsuranceStatusChecker: بیمار جدید انتخاب شد. بررسی خودکار...');
                autoCheckOnPatientSelect(patientData.PatientId);
            }
        });
    });

})(window, jQuery);

