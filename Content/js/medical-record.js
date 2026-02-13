/**
 * Medical Record Component Manager
 * Single Responsibility: مدیریت AJAX Loading برای Components
 * Pattern: مشابه patient-dashboard.js
 * ✅ Enterprise-Grade: Component-Based, AJAX-First, Error Handling
 */
(function($, window) {
    'use strict';
    
    // ✅ Configuration
    var config = {
        apiBaseUrl: '/Patient/Api/MedicalRecord',
        sections: {
            medicalHistory: {
                url: '/GetMedicalHistories',
                container: '[data-medical-record-section="medicalHistory"]',
                partial: '_MedicalHistorySection',
                listSelector: '.medical-history-list',
                itemSelector: '.medical-history-item'
            },
            appointments: {
                url: '/GetAppointments',
                container: '[data-medical-record-section="appointments"]',
                partial: '_AppointmentsSection',
                listSelector: '.appointments-list',
                itemSelector: '.appointment-item'
            },
            receptions: {
                url: '/GetReceptions',
                container: '[data-medical-record-section="receptions"]',
                partial: '_ReceptionsSection',
                listSelector: '.receptions-list',
                itemSelector: '.reception-item'
            },
            triage: {
                url: '/GetTriageAssessments',
                container: '[data-medical-record-section="triage"]',
                partial: '_TriageSection',
                listSelector: '.triage-list',
                itemSelector: '.triage-item'
            }
        },
        retryAttempts: 3,
        retryDelay: 2000
    };
    
    /** ✅ URL از سرور (data-* روی #medicalRecordShell) برای جلوگیری از 404 / virtual path */
    function getApiBaseUrl() {
        var $shell = $('#medicalRecordShell');
        if ($shell.length && $shell.data('api-base')) return $shell.data('api-base');
        return config.apiBaseUrl;
    }
    function getCreateMedicalHistoryUrl() {
        var $shell = $('#medicalRecordShell');
        if ($shell.length && $shell.data('api-create')) return $shell.data('api-create');
        return config.apiBaseUrl + '/CreateMedicalHistory';
    }
    function getUpdateMedicalHistoryUrl() {
        var $shell = $('#medicalRecordShell');
        if ($shell.length && $shell.data('api-update')) return $shell.data('api-update');
        return config.apiBaseUrl + '/UpdateMedicalHistory';
    }
    
    // ✅ MedicalRecord - Enterprise-Grade Module
    var MedicalRecord = {
        
        /**
         * ✅ Initialize Module
         */
        init: function() {
            this.applyFilterVisibility();
            this.loadAllSections();
            this.bindEvents();
        },

        /**
         * ✅ خواندن وضعیت نوار فیلتر (فاز ۲.۲)
         * @returns {{ fromDate: string|null, toDate: string|null, search: string, sectionType: string }}
         */
        getFilterState: function() {
            var preset = $('#medicalRecordFilterDateRange').val() || 'all';
            var search = ($('#medicalRecordFilterSearch').val() || '').trim();
            var sectionType = $('#medicalRecordFilterType').val() || 'all';
            var now = new Date();
            var fromDate = null;
            var toDate = null;
            if (preset === '3m') {
                var from = new Date(now);
                from.setMonth(from.getMonth() - 3);
                fromDate = from.toISOString().slice(0, 10);
                toDate = now.toISOString().slice(0, 10);
            } else if (preset === '6m') {
                var from6 = new Date(now);
                from6.setMonth(from6.getMonth() - 6);
                fromDate = from6.toISOString().slice(0, 10);
                toDate = now.toISOString().slice(0, 10);
            } else if (preset === '1y') {
                fromDate = now.getFullYear() + '-01-01';
                toDate = now.toISOString().slice(0, 10);
            }
            return { fromDate: fromDate, toDate: toDate, search: search, sectionType: sectionType };
        },

        /**
         * ✅ به‌روزرسانی کارت خلاصه پرونده (فاز ۲.۱)
         */
        updateSummary: function(sectionName, total) {
            var $el = $('#medicalRecordShell').find('[data-summary="' + sectionName + '"]');
            if ($el.length) {
                $el.text(total != null ? total : '–');
            }
        },

        /**
         * ✅ نمایش/مخفی کردن کارت‌های سکشن بر اساس فیلتر نوع
         */
        applyFilterVisibility: function() {
            var sectionType = ($('#medicalRecordFilterType').val() || 'all');
            var $sections = $('#medicalRecordShell .medical-record-sections');
            ['medicalHistory', 'appointments', 'receptions', 'triage'].forEach(function(key) {
                var $card = $sections.find('[data-medical-record-section="' + key + '"]').closest('.col-12');
                if ($card.length) {
                    $card.toggle(sectionType === 'all' || sectionType === key);
                }
            });
        },

        /**
         * ✅ Load All Sections (با فیلتر یکپارچه)
         */
        loadAllSections: function() {
            var self = this;
            var filter = this.getFilterState();
            this.applyFilterVisibility();

            var sectionsToLoad = filter.sectionType === 'all'
                ? ['medicalHistory', 'appointments', 'receptions', 'triage']
                : [filter.sectionType];

            var params = { pageNumber: 1 };
            if (filter.fromDate) params.fromDate = filter.fromDate;
            if (filter.toDate) params.toDate = filter.toDate;
            if (filter.search) params.search = filter.search;

            var promises = sectionsToLoad.map(function(sectionName) {
                return self.loadSection(sectionName, params);
            });
            Promise.all(promises).catch(function(error) {
                console.error('Error loading medical record sections:', error);
            });
        },

        /**
         * ✅ آیا نمای فعلی تایم‌لاین است؟ (فاز ۲.۳)
         */
        isTimelineView: function() {
            return $('#medicalRecordViewTimeline').hasClass('active');
        },

        /**
         * ✅ سوییچ بین نمای سکشن‌ها و تایم‌لاین (فاز ۲.۳)
         */
        setViewMode: function(mode) {
            var isTimeline = (mode === 'timeline');
            $('#medicalRecordViewSections').toggleClass('active', !isTimeline).attr('aria-pressed', !isTimeline);
            $('#medicalRecordViewTimeline').toggleClass('active', isTimeline).attr('aria-pressed', isTimeline);
            $('#medicalRecordSections').toggle(!isTimeline);
            var $timeline = $('#medicalRecordTimelineView');
            $timeline.toggle(isTimeline);
            if (isTimeline) {
                this.loadTimelineView();
            }
        },

        /**
         * ✅ بارگذاری و رندر نمای تایم‌لاین (فاز ۲.۳)
         */
        loadTimelineView: function() {
            var self = this;
            var $container = $('#medicalRecordTimelineView');
            var $loading = $container.find('.medical-record-timeline-loading');
            var $empty = $container.find('.medical-record-timeline-empty');
            var $content = $container.find('.medical-record-timeline-content');
            $loading.show();
            $empty.hide();
            $content.empty();

            var filter = this.getFilterState();
            var params = { pageNumber: 1, pageSize: 30 };
            if (filter.fromDate) params.fromDate = filter.fromDate;
            if (filter.toDate) params.toDate = filter.toDate;
            if (filter.search) params.search = filter.search;

            var base = getApiBaseUrl();
            var sectionKeys = ['medicalHistory', 'appointments', 'receptions', 'triage'];
            var urls = sectionKeys.map(function(key) {
                return base + config.sections[key].url + '?' + $.param(params);
            });

            // هر درخواست جداگانه؛ اگر یکی خطا داد بقیه همچنان نمایش داده می‌شوند (مثل نمای سکشن)
            var promises = urls.map(function(url, idx) {
                var typeKey = sectionKeys[idx];
                return $.ajax({
                    url: url,
                    method: 'GET',
                    dataType: 'json',
                    headers: { 'X-Requested-With': 'XMLHttpRequest', 'X-AJAX-Request': 'true' }
                }).then(function(response) {
                    var ok = response && (response.success === true || response.Success === true);
                    var data = response && (response.data !== undefined ? response.data : response.Data);
                    if (!ok || !data) return [];
                    var list = Array.isArray(data) ? data : (data.items || data.Items || []);
                    if (!Array.isArray(list)) list = [];
                    return list.map(function(item) {
                        return self.normalizeTimelineItem(item, typeKey);
                    }).filter(Boolean);
                }).catch(function(xhr, status, err) {
                    console.warn('Timeline: failed to load ' + typeKey, status, err);
                    return [];
                });
            });

            $.when.apply($, promises).then(function() {
                var arrays = Array.prototype.slice.call(arguments);
                var all = [];
                arrays.forEach(function(arr) {
                    if (Array.isArray(arr)) all = all.concat(arr);
                });
                $loading.hide();
                if (all.length === 0) {
                    $empty.show();
                    return;
                }
                all.sort(function(a, b) {
                    return (new Date(b.sortKey)).getTime() - (new Date(a.sortKey)).getTime();
                });
                var groups = {};
                all.forEach(function(entry) {
                    var key = entry.groupKey || (entry.dateShamsi ? entry.dateShamsi.replace(/\D/g, '').substring(0, 6) : (entry.sortKey || '').replace(/\D/g, '').substring(0, 6));
                    if (!key) key = 'other';
                    if (!groups[key]) groups[key] = [];
                    groups[key].push(entry);
                });
                var sortedKeys = Object.keys(groups).sort(function(a, b) {
                    var firstA = groups[a][0];
                    var firstB = groups[b][0];
                    return (new Date(firstB.sortKey)).getTime() - (new Date(firstA.sortKey)).getTime();
                });
                var typeLabels = { medicalHistory: 'تاریخچه پزشکی', appointments: 'نوبت', receptions: 'پذیرش', triage: 'تریاژ' };
                var typeIcons = { medicalHistory: 'fa-history', appointments: 'fa-calendar-check', receptions: 'fa-file-medical-alt', triage: 'fa-stethoscope' };
                var groupTitleDisplay = self.formatTimelineGroupTitle(sortedKeys);
                sortedKeys.forEach(function(key) {
                    var $group = $('<div class="medical-record-timeline-group tl-group"></div>');
                    var titleText = groupTitleDisplay[key] || key;
                    $group.append('<h6 class="medical-record-timeline-group-title tl-group__title"><i class="fas fa-calendar-alt ml-1"></i>' + titleText + '</h6>');
                    groups[key].forEach(function(entry) {
                        var $item = $('<div class="medical-record-timeline-item tl-item ' + entry.type + '" data-type="' + entry.type + '"></div>');
                        var label = typeLabels[entry.type] || entry.type;
                        var icon = typeIcons[entry.type] || 'fa-circle';
                        var $card = $('<div class="medical-record-timeline-card tl-card"></div>');
                        var $top = $('<div class="tl-card__top"></div>');
                        $top.append($('<span class="tl-card__badge"><i class="fas ' + icon + ' ml-1"></i>' + label + '</span>'));
                        if (entry.dateShamsi) $top.append($('<span class="tl-card__date"></span>').text(entry.dateShamsi));
                        $card.append($top);
                        $card.append($('<div class="timeline-title tl-card__title"></div>').text(entry.title || '—'));
                        if (entry.subtitle) $card.append($('<div class="timeline-subtitle tl-card__subtitle"></div>').text(entry.subtitle));
                        if (entry.metaLine) $card.append($('<div class="tl-card__meta"></div>').text(entry.metaLine));
                        $item.append($card);
                        $group.append($item);
                    });
                    $content.append($group);
                });
            }).fail(function() {
                $loading.hide();
                $empty.show();
                $content.empty();
            });
        },

        /**
         * ✅ فرمت عنوان گروه تایم‌لاین برای نمایش (مثلاً 1403/08)
         */
        formatTimelineGroupTitle: function(keys) {
            var out = {};
            keys.forEach(function(k) {
                if (!k) { out[k] = k; return; }
                var s = String(k).replace(/\D/g, '');
                if (s.length >= 6) out[k] = s.substring(0, 4) + '/' + s.substring(4, 6);
                else out[k] = k;
            });
            return out;
        },

        /**
         * ✅ نرمال‌سازی یک آیتم برای تایم‌لاین (فاز ۲.۳) — با فیلدهای اضافه برای کارت خوانا
         */
        normalizeTimelineItem: function(item, typeKey) {
            var sortKey = null;
            var dateShamsi = '';
            var title = '';
            var subtitle = '';
            var metaLine = '';
            if (typeKey === 'medicalHistory') {
                sortKey = item.StartDate || item.startDate || item.CreatedAt || item.createdAt;
                if (!sortKey) return null;
                dateShamsi = item.StartDateShamsi || item.startDateShamsi || item.CreatedAtShamsi || item.createdAtShamsi || '';
                title = item.Title || item.title || 'تاریخچه پزشکی';
                subtitle = item.TypeText || item.typeText || dateShamsi;
                var parts = [];
                var meds = item.Medications || item.medications;
                if (meds && meds.length) {
                    var m = meds[0];
                    var mn = m.DrugName || m.drugName || '';
                    var md = m.Dosage || m.dosage;
                    var mu = m.DosageUnit || m.dosageUnit;
                    var mf = m.Frequency || m.frequency;
                    if (md) mn += ' ' + md + (mu ? ' ' + mu : '');
                    if (mf) mn += (mn ? '، ' : '') + mf;
                    if (mn) parts.push(mn);
                }
                var labs = item.LabResults || item.labResults;
                if (labs && labs.length) {
                    var lb = labs[0];
                    var lbText = (lb.LabName || lb.labName || '') + (lb.Value || lb.value ? ' ' + (lb.Value || lb.value) + (lb.Unit || lb.unit ? ' ' + (lb.Unit || lb.unit) : '') : '');
                    if (lbText.trim()) parts.push(lbText.trim());
                }
                var docName = item.DoctorName || item.doctorName;
                if (docName) parts.push('دکتر ' + docName);
                if (item.MedicalCenter || item.medicalCenter) parts.push(item.MedicalCenter || item.medicalCenter);
                if (item.Severity || item.severity) parts.push('شدت: ' + (item.Severity || item.severity));
                if (parts.length) metaLine = parts.join(' · ');
            } else if (typeKey === 'appointments') {
                sortKey = item.AppointmentDate || item.appointmentDate;
                dateShamsi = item.AppointmentDateShamsi || item.appointmentDateShamsi || '';
                title = item.DoctorName || item.doctorName || 'نوبت';
                subtitle = dateShamsi;
                if (item.AppointmentTime || item.appointmentTime) metaLine = 'ساعت ' + (item.AppointmentTime || item.appointmentTime);
            } else if (typeKey === 'receptions') {
                sortKey = item.ReceptionDate || item.receptionDate;
                dateShamsi = item.ReceptionDateShamsi || item.receptionDateShamsi || '';
                title = item.ReceptionNumber || item.receptionNumber || 'پذیرش';
                subtitle = dateShamsi;
                if (item.DoctorName || item.doctorName) metaLine = item.DoctorName || item.doctorName;
            } else if (typeKey === 'triage') {
                sortKey = item.ArrivalAt || item.arrivalAt;
                dateShamsi = item.ArrivalAtShamsi || item.arrivalAtShamsi || '';
                title = item.ChiefComplaint || item.chiefComplaint || 'ارزیابی تریاژ';
                subtitle = dateShamsi;
                if (item.LevelText || item.levelText) metaLine = item.LevelText || item.levelText;
            }
            if (!sortKey) return null;
            var d = (typeof sortKey === 'string') ? sortKey : (sortKey.toISOString ? sortKey.toISOString() : (sortKey.toString && sortKey.toString().match(/^\d{4}-\d{2}-\d{2}/) ? sortKey.toString() : ''));
            if (!d) return null;
            var groupKey = dateShamsi ? dateShamsi.replace(/\D/g, '').substring(0, 6) : d.slice(0, 7).replace(/-/g, '').substring(0, 6);
            return { sortKey: d, type: typeKey, dateShamsi: dateShamsi, title: title, subtitle: subtitle, metaLine: metaLine, groupKey: groupKey };
        },
        
        /**
         * ✅ Load Section via AJAX
         * @param {string} sectionName - Name of the section
         */
        loadSection: function(sectionName) {
            var self = this;
            var section = config.sections[sectionName];
            
            if (!section) {
                console.error('Unknown section:', sectionName);
                return Promise.reject('Unknown section');
            }
            
            // ✅ همیشه بخش را داخل Shell جستجو کن تا در داشبورد به‌درستی به‌روز شود
            var $shell = $('#medicalRecordShell');
            var $container = $shell.length ? $shell.find(section.container).first() : $(section.container);
            if ($container.length === 0) {
                console.warn('Container not found for section:', sectionName);
                return Promise.reject('Container not found');
            }
            
            // ✅ Show loading state
            this.showLoading($container);
            
            // ✅ Build URL با پارامترهای اختیاری (صفحه‌بندی، فیلتر، جستجو)
            var url = getApiBaseUrl() + section.url;
            var params = arguments[1] || {};
            if (Object.keys(params).length > 0) {
                var qs = $.param(params);
                if (qs) url += (url.indexOf('?') >= 0 ? '&' : '?') + qs;
            }
            
            // ✅ AJAX request
            return $.ajax({
                url: url,
                method: 'GET',
                dataType: 'json',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'X-AJAX-Request': 'true'
                },
                cache: false,
                timeout: 30000
            }).then(function(response) {
                var ok = response && (response.success === true || response.Success === true);
                var data = response && (response.data !== undefined ? response.data : response.Data);
                if (ok) {
                    var list = Array.isArray(data) ? data : (data && (data.items || data.Items) ? (data.items || data.Items) : []);
                    if (!Array.isArray(list)) list = [];
                    // پشتیبانی از پاسخ صفحه‌بندی‌شده برای همه سکشن‌ها (تاریخچه، نوبت، پذیرش، تریاژ)
                    var pagedMeta = (data && (data.TotalItems != null || data.totalItems != null)) ? {
                        totalItems: data.TotalItems != null ? data.TotalItems : data.totalItems,
                        pageNumber: (data.PageNumber != null ? data.PageNumber : data.pageNumber) || 1,
                        pageSize: (data.PageSize != null ? data.PageSize : data.pageSize) || (sectionName === 'medicalHistory' ? 20 : 10)
                    } : null;
                    if (sectionName === 'medicalHistory') {
                        if (list.length > 0) {
                            console.log('✅ Medical history loaded:', list.length, 'item(s)', pagedMeta ? '(total: ' + pagedMeta.totalItems + ')' : '');
                        }
                    }
                    if (pagedMeta && pagedMeta.totalItems != null) {
                        self.updateSummary(sectionName, pagedMeta.totalItems);
                    }
                    self.renderSection($container, section.partial, list, pagedMeta, sectionName);
                } else {
                    self.showError($container, (response && (response.message || response.Message)) || 'خطا در بارگذاری');
                }
            }).catch(function(xhr, status, error) {
                console.error('AJAX Error for section:', sectionName, { xhr: xhr, status: status, error: error });
                
                if (xhr.status === 401) {
                    // ✅ Unauthorized - redirect to login
                    if (window.openLoginModal) {
                        window.openLoginModal(window.location.href);
                    } else {
                        window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.href);
                    }
                } else {
                    self.showError($container, 'خطا در بارگذاری. لطفاً دوباره تلاش کنید.');
                }
            });
        },
        
        /**
         * ✅ Render Section
         * @param {jQuery} $container - Container element
         * @param {string} partialName - Name of the partial view
         * @param {object} data - Data to render (list or object)
         * @param {object} pagedMeta - Optional { totalItems, pageNumber, pageSize }
         * @param {string} sectionName - Optional section key (medicalHistory, appointments, receptions, triage)
         */
        renderSection: function($container, partialName, data, pagedMeta, sectionName) {
            var self = this;
            
            this.hideLoading($container);
            
            var isEmpty = false;
            if (Array.isArray(data)) {
                isEmpty = data.length === 0;
            } else if (data && typeof data === 'object') {
                isEmpty = Object.keys(data).length === 0;
            }
            
            var $cardBody = $container.find('.medical-record-card-body');
            if ($cardBody.length === 0) {
                $cardBody = $container;
            }
            
            if (isEmpty) {
                $cardBody.find('.medical-record-section-content').hide();
                $cardBody.find('.medical-record-section-empty').show();
                $cardBody.find('.medical-record-section-error').hide();
                $cardBody.find('.medical-record-section-paging').remove();
            } else {
                var renderUrl = '/Patient/MedicalRecord/RenderPartial?partialName=' + encodeURIComponent(partialName);
                var token = $('input[name="__RequestVerificationToken"]').first().val();
                var ajaxHeaders = {
                    'X-Requested-With': 'XMLHttpRequest',
                    'X-AJAX-Request': 'true'
                };
                if (token) ajaxHeaders['RequestVerificationToken'] = token;
                
                $.ajax({
                    url: renderUrl,
                    method: 'POST',
                    data: JSON.stringify(data),
                    contentType: 'application/json',
                    dataType: 'html',
                    headers: ajaxHeaders,
                    success: function(html) {
                        $cardBody.find('.medical-record-section-content').html(html).show();
                        $cardBody.find('.medical-record-section-empty').hide();
                        $cardBody.find('.medical-record-section-error').hide();
                        
                        if (partialName === '_MedicalHistorySection' && pagedMeta && pagedMeta.totalItems > 0) {
                            self.renderMedicalHistoryPaging($cardBody, $container, pagedMeta);
                        } else if (pagedMeta && pagedMeta.totalItems > 0 && sectionName && sectionName !== 'medicalHistory') {
                            self.renderGenericSectionPaging(sectionName, $cardBody, $container, pagedMeta);
                        }
                        
                        self.reinitializeComponents($cardBody);
                    },
                    error: function(xhr) {
                        console.error('Error rendering partial:', partialName, xhr);
                        self.showError($container, 'خطا در نمایش محتوا');
                    }
                });
            }
        },

        /**
         * ✅ نوار صفحه‌بندی تاریخچه پزشکی و دکمه «مشاهده بیشتر»
         */
        renderMedicalHistoryPaging: function($cardBody, $container, pagedMeta) {
            var self = this;
            $cardBody.find('.medical-record-section-paging').remove();
            var total = pagedMeta.totalItems;
            var page = pagedMeta.pageNumber || 1;
            var pageSize = pagedMeta.pageSize || 20;
            var from = (page - 1) * pageSize + 1;
            var to = Math.min(page * pageSize, total);
            var hasMore = page * pageSize < total;
            var $footer = $('<div class="medical-record-section-paging border-top pt-3 mt-3"></div>');
            $footer.append('<span class="text-muted small">نمایش ' + from + '–' + to + ' از ' + total + '</span>');
            if (hasMore) {
                var $btn = $('<button type="button" class="btn btn-outline-primary btn-sm mr-2 medical-history-load-more">مشاهده بیشتر</button>');
                $btn.data('next-page', page + 1);
                $footer.append($btn);
            }
            $cardBody.find('.medical-record-section-content').append($footer);

            $cardBody.off('click.medicalHistoryLoadMore').on('click.medicalHistoryLoadMore', '.medical-history-load-more', function() {
                var nextPage = $(this).data('next-page');
                if (!nextPage) return;
                var $btn = $(this).prop('disabled', true).html('<i class="fas fa-spinner fa-spin ml-1"></i> در حال بارگذاری...');
                var section = config.sections.medicalHistory;
                var filter = self.getFilterState();
                var qs = { pageNumber: nextPage, pageSize: pageSize };
                if (filter.fromDate) qs.fromDate = filter.fromDate;
                if (filter.toDate) qs.toDate = filter.toDate;
                if (filter.search) qs.search = filter.search;
                var url = getApiBaseUrl() + section.url + '?' + $.param(qs);
                $.getJSON(url).then(function(response) {
                    var ok = response && (response.success === true || response.Success === true);
                    var data = response && (response.data !== undefined ? response.data : response.Data);
                    if (!ok || !data) return;
                    var list = data.Items || data.items || [];
                    if (list.length === 0) return;
                    var renderUrl = '/Patient/MedicalRecord/RenderPartial?partialName=_MedicalHistorySection';
                    var token = $('input[name="__RequestVerificationToken"]').first().val();
                    $.ajax({
                        url: renderUrl,
                        method: 'POST',
                        data: JSON.stringify(list),
                        contentType: 'application/json',
                        dataType: 'html',
                        headers: { 'X-Requested-With': 'XMLHttpRequest', 'RequestVerificationToken': token || '' }
                    }).then(function(html) {
                        var $list = $cardBody.find('.medical-history-list');
                        var $frag = $(html);
                        $frag.find('.medical-history-item').appendTo($list);
                        $cardBody.find('.medical-record-section-paging').remove();
                        var newMeta = { totalItems: total, pageNumber: nextPage, pageSize: pageSize };
                        self.renderMedicalHistoryPaging($cardBody, $container, newMeta);
                        self.reinitializeComponents($cardBody);
                    });
                }).always(function() {
                    $btn.prop('disabled', false).text('مشاهده بیشتر');
                });
            });
        },

        /**
         * ✅ نوار صفحه‌بندی و «مشاهده بیشتر» برای نوبت، پذیرش، تریاژ (فاز ۲.۴)
         */
        renderGenericSectionPaging: function(sectionName, $cardBody, $container, pagedMeta) {
            var self = this;
            var section = config.sections[sectionName];
            if (!section || !section.listSelector || !section.itemSelector) return;

            $cardBody.find('.medical-record-section-paging').remove();
            var total = pagedMeta.totalItems;
            var page = pagedMeta.pageNumber || 1;
            var pageSize = pagedMeta.pageSize || 10;
            var from = (page - 1) * pageSize + 1;
            var to = Math.min(page * pageSize, total);
            var hasMore = page * pageSize < total;
            var $footer = $('<div class="medical-record-section-paging border-top pt-3 mt-3"></div>');
            $footer.append('<span class="text-muted small">نمایش ' + from + '–' + to + ' از ' + total + '</span>');
            if (hasMore) {
                var $btn = $('<button type="button" class="btn btn-outline-primary btn-sm mr-2 medical-record-load-more">مشاهده بیشتر</button>');
                $btn.data('section', sectionName).data('next-page', page + 1).data('page-size', pageSize);
                $footer.append($btn);
            }
            $cardBody.find('.medical-record-section-content').append($footer);

            $cardBody.off('click.mrLoadMore').on('click.mrLoadMore', '.medical-record-load-more', function() {
                var sec = $(this).data('section');
                var nextPage = $(this).data('next-page');
                if (!sec || !nextPage) return;
                var $btn = $(this).prop('disabled', true).html('<i class="fas fa-spinner fa-spin ml-1"></i> در حال بارگذاری...');
                var cfg = config.sections[sec];
                var filter = self.getFilterState();
                var params = { pageNumber: nextPage, pageSize: $(this).data('page-size') || 10 };
                if (filter.fromDate) params.fromDate = filter.fromDate;
                if (filter.toDate) params.toDate = filter.toDate;
                if (filter.search) params.search = filter.search;
                var url = getApiBaseUrl() + cfg.url + '?' + $.param(params);
                $.getJSON(url).then(function(response) {
                    var ok = response && (response.success === true || response.Success === true);
                    var data = response && (response.data !== undefined ? response.data : response.Data);
                    if (!ok || !data) return;
                    var list = data.Items || data.items || [];
                    if (list.length === 0) return;
                    var renderUrl = '/Patient/MedicalRecord/RenderPartial?partialName=' + encodeURIComponent(cfg.partial);
                    var token = $('input[name="__RequestVerificationToken"]').first().val();
                    $.ajax({
                        url: renderUrl,
                        method: 'POST',
                        data: JSON.stringify(list),
                        contentType: 'application/json',
                        dataType: 'html',
                        headers: { 'X-Requested-With': 'XMLHttpRequest', 'RequestVerificationToken': token || '' }
                    }).then(function(html) {
                        var $list = $cardBody.find(cfg.listSelector);
                        var $frag = $(html);
                        $frag.find(cfg.itemSelector).appendTo($list);
                        $cardBody.find('.medical-record-section-paging').remove();
                        var newMeta = { totalItems: total, pageNumber: nextPage, pageSize: params.pageSize };
                        self.renderGenericSectionPaging(sec, $cardBody, $container, newMeta);
                        self.reinitializeComponents($cardBody);
                    });
                }).always(function() {
                    $btn.prop('disabled', false).text('مشاهده بیشتر');
                });
            });
        },
        
        /**
         * ✅ Show Loading State
         */
        showLoading: function($container) {
            var $cardBody = $container.find('.medical-record-card-body');
            if ($cardBody.length === 0) {
                $cardBody = $container;
            }
            
            $cardBody.find('.medical-record-section-loading').show();
            $cardBody.find('.medical-record-section-content').hide();
            $cardBody.find('.medical-record-section-empty').hide();
            $cardBody.find('.medical-record-section-error').hide();
        },
        
        /**
         * ✅ Hide Loading State
         */
        hideLoading: function($container) {
            var $cardBody = $container.find('.medical-record-card-body');
            if ($cardBody.length === 0) {
                $cardBody = $container;
            }
            
            $cardBody.find('.medical-record-section-loading').hide();
        },
        
        /**
         * ✅ Show Error State
         */
        showError: function($container, message) {
            var self = this;
            var $cardBody = $container.find('.medical-record-card-body');
            if ($cardBody.length === 0) {
                $cardBody = $container;
            }
            
            this.hideLoading($container);
            
            var $errorDiv = $cardBody.find('.medical-record-section-error');
            $errorDiv.find('.error-message').text(message || 'خطا در بارگذاری');
            $errorDiv.show();
            $cardBody.find('.medical-record-section-content').hide();
            $cardBody.find('.medical-record-section-empty').hide();
        },
        
        /**
         * ✅ Reinitialize Components
         */
        reinitializeComponents: function($container) {
            // ✅ Reinitialize tooltips if exists
            if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
                $container.find('[data-bs-toggle="tooltip"]').each(function() {
                    new bootstrap.Tooltip(this);
                });
            }
            
            // ✅ Reinitialize modals if exists
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                $container.find('[data-toggle="modal"]').each(function() {
                    // Modal will be initialized on click
                });
            }
        },
        
        /**
         * ✅ Bind Events
         */
        bindEvents: function() {
            var self = this;
            
            // ✅ اعمال فیلتر (فاز ۲.۲) — در نمای تایم‌لاین، تایم‌لاین را دوباره لود کن
            $(document).on('click', '#medicalRecordFilterApply', function() {
                if (self.isTimelineView()) {
                    self.loadTimelineView();
                } else {
                    self.loadAllSections();
                }
            });

            // ✅ سوییچ نما: سکشن‌ها / تایم‌لاین (فاز ۲.۳)
            $(document).on('click', '#medicalRecordViewSections', function() {
                self.setViewMode('sections');
            });
            $(document).on('click', '#medicalRecordViewTimeline', function() {
                self.setViewMode('timeline');
            });

            // ✅ نسخه قابل چاپ (فاز ۲.۵)
            $(document).on('click', '#medicalRecordPrintBtn', function(e) {
                e.preventDefault();
                window.print();
            });

            // ✅ دسترسی سریع: Ctrl+Shift+F فوکوس روی جستجو (فاز ۲.۵)
            $(document).on('keydown', function(e) {
                if ((e.ctrlKey || e.metaKey) && e.shiftKey && (e.key === 'F' || e.key === 'f')) {
                    var $search = $('#medicalRecordFilterSearch');
                    if ($search.length && $('#medicalRecordShell').length) {
                        e.preventDefault();
                        $search.focus();
                    }
                }
            });

            // ✅ Refresh button — در نمای تایم‌لاین، تایم‌لاین را لود کن
            $(document).on('click', '.refresh-medical-record', function() {
                if (self.isTimelineView()) {
                    self.loadTimelineView();
                } else {
                    self.loadAllSections();
                }
            });
            
            // ✅ Reload section button
            $(document).on('click', '.reload-section', function() {
                var sectionName = $(this).data('section');
                if (sectionName) {
                    self.loadSection(sectionName);
                }
            });
            
            // ✅ Add medical history button
            $(document).on('click', '.add-medical-history', function() {
                self.openMedicalHistoryModal();
            });
            
            // ✅ Edit medical history
            $(document).on('click', '.edit-medical-history', function() {
                var medicalHistoryId = $(this).data('medical-history-id');
                if (medicalHistoryId) {
                    self.loadMedicalHistoryForEdit(medicalHistoryId);
                }
            });
            
            // ✅ Delete medical history
            $(document).on('click', '.delete-medical-history', function() {
                var medicalHistoryId = $(this).data('medical-history-id');
                if (medicalHistoryId) {
                    self.deleteMedicalHistory(medicalHistoryId);
                }
            });
            
            // ✅ Medical history form submit
            $(document).on('submit', '#medicalHistoryForm', function(e) {
                e.preventDefault();
                self.saveMedicalHistory();
            });
            
            // ✅ File input change
            $(document).on('change', '#Attachments', function() {
                self.previewAttachments(this.files);
            });
            // ✅ نوع تاریخچه: نمایش/مخفی بلوک دارو، آزمایش، آلرژی و الزام عنوان
            $(document).on('change', '#Type', function() {
                self.toggleTypeBlocks();
            });
            $(document).on('click', '#btnAddDiseaseMedication', function() {
                self.addDiseaseMedicationRow();
            });
            $(document).on('click', '.remove-disease-medication', function() {
                $(this).closest('.disease-medication-row').remove();
            });
        },
        
        /**
         * نمایش یا مخفی کردن بلوک‌های وابسته به نوع (دارو، آزمایش، آلرژی) و تنظیم الزام عنوان
         */
        toggleMedicationBlock: function() {
            var typeVal = $('#Type').val();
            var isMedication = (typeVal === '3');
            var $block = $('#medicalHistoryMedicationBlock');
            var $title = $('#Title');
            var $titleStar = $('#titleRequiredStar');
            if ($block.length) {
                if (isMedication) {
                    $block.removeClass('d-none').show();
                    $title.removeAttr('required');
                    if ($titleStar.length) $titleStar.text('').addClass('text-muted').attr('title', 'برای نوع دارو عنوان یا نام دارو کافی است');
                } else {
                    $block.addClass('d-none').hide();
                    $title.attr('required', 'required');
                    if ($titleStar.length) { $titleStar.text('*').removeClass('text-muted').removeAttr('title'); }
                    $('#DrugName, #Dosage, #DosageUnit, #Frequency, #Route, #Indication, #PrescribingDoctor').val('');
                }
            }
        },
        toggleTypeBlocks: function() {
            var typeVal = $('#Type').val();
            this.toggleMedicationBlock();
            var $labBlock = $('#medicalHistoryLabBlock');
            var $allergyBlock = $('#medicalHistoryAllergyBlock');
            var $diseaseMedsBlock = $('#medicalHistoryDiseaseMedicationsBlock');
            if ($labBlock.length) {
                if (typeVal === '0') $labBlock.removeClass('d-none').show();
                else { $labBlock.addClass('d-none').hide(); $('#LabName, #LabValue, #LabUnit, #labDatePicker, #LabReferenceRange').val(''); }
            }
            if ($allergyBlock.length) {
                if (typeVal === '4') $allergyBlock.removeClass('d-none').show();
                else { $allergyBlock.addClass('d-none').hide(); $('#IsCritical').prop('checked', false); }
            }
            if ($diseaseMedsBlock.length) {
                if (typeVal === '0') $diseaseMedsBlock.removeClass('d-none').show();
                else { $diseaseMedsBlock.addClass('d-none').hide(); $('#diseaseMedicationsContainer').empty(); $('#MedicationsListJson').val(''); }
            }
        },
        addDiseaseMedicationRow: function(data) {
            var template = document.getElementById('diseaseMedicationRowTemplate');
            if (!template) return;
            var clone = template.content.cloneNode(true);
            if (data) {
                $(clone).find('.medication-name').val(data.DrugName || data.drugName || '');
                $(clone).find('.medication-dosage').val(data.Dosage || data.dosage || '');
                $(clone).find('.medication-unit').val(data.DosageUnit || data.dosageUnit || '');
                $(clone).find('.medication-frequency').val(data.Frequency || data.frequency || '');
            }
            $('#diseaseMedicationsContainer').append(clone);
        },
        collectDiseaseMedications: function() {
            var list = [];
            $('#diseaseMedicationsContainer .disease-medication-row').each(function() {
                var name = $(this).find('.medication-name').val();
                if (!name || !name.trim()) return;
                list.push({
                    DrugName: name.trim(),
                    Dosage: $(this).find('.medication-dosage').val() || '',
                    DosageUnit: $(this).find('.medication-unit').val() || '',
                    Frequency: $(this).find('.medication-frequency').val() || ''
                });
            });
            return list;
        },
        
        /**
         * ✅ Open Medical History Modal (Create)
         */
        openMedicalHistoryModal: function() {
            var $modal = $('#medicalHistoryModal');
            if ($modal.length === 0) {
                console.error('Medical history modal not found');
                return;
            }
            
            // Reset form
            var form = $('#medicalHistoryForm')[0];
            if (form) form.reset();
            $('#MedicalHistoryId').val('');
            $('#Title').val('');
            $('#modalTitle').text('افزودن تاریخچه پزشکی');
            $('#attachmentsPreview').empty();
            $('#startDatePicker').val('');
            $('#endDatePicker').val('');
            $('#DrugName, #Dosage, #DosageUnit, #Frequency, #Route, #Indication, #PrescribingDoctor').val('');
            $('#LabName, #LabValue, #LabUnit, #labDatePicker, #LabReferenceRange').val('');
            $('#IsCritical').prop('checked', false);
            $('#diseaseMedicationsContainer').empty();
            $('#MedicationsListJson').val('');
            this.toggleTypeBlocks();
            
            // ✅ DatePicker شمسی: اجرای مجدد startWatch تا تقویم روی کلیک باز شود (به‌خصوص وقتی مودال با AJAX لود شده)
            if (typeof JalaliDatePickerEnterprise !== 'undefined' && JalaliDatePickerEnterprise.startWatchAgain) {
                setTimeout(function() {
                    JalaliDatePickerEnterprise.startWatchAgain();
                }, 100);
            }
            
            // Show modal (Bootstrap 5)
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                var modalInstance = bootstrap.Modal.getOrCreateInstance($modal[0]);
                modalInstance.show();
            } else if ($modal.modal) {
                $modal.modal('show');
            }
        },
        
        /**
         * ✅ Load Medical History for Edit
         */
        loadMedicalHistoryForEdit: function(medicalHistoryId) {
            var self = this;
            
            $.ajax({
                url: getApiBaseUrl() + '/GetMedicalHistory',
                method: 'GET',
                data: { id: medicalHistoryId },
                dataType: 'json',
                success: function(response) {
                    if (response && response.success && response.data) {
                        self.populateMedicalHistoryForm(response.data);
                        $('#modalTitle').text('ویرایش تاریخچه پزشکی');
                        
                        var $modal = $('#medicalHistoryModal');
                        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                            var modalInstance = bootstrap.Modal.getOrCreateInstance($modal[0]);
                            modalInstance.show();
                        } else if ($modal.modal) {
                            $modal.modal('show');
                        }
                    } else {
                        Swal.fire({
                            title: 'خطا',
                            text: response?.message || 'خطا در دریافت اطلاعات',
                            icon: 'error',
                            confirmButtonText: 'باشه'
                        });
                    }
                },
                error: function(xhr) {
                    console.error('Error loading medical history:', xhr);
                    Swal.fire({
                        title: 'خطا',
                        text: 'خطا در دریافت اطلاعات',
                        icon: 'error',
                        confirmButtonText: 'باشه'
                    });
                }
            });
        },
        
        /**
         * ✅ Populate Medical History Form
         */
        populateMedicalHistoryForm: function(data) {
            var self = this;
            $('#MedicalHistoryId').val(data.MedicalHistoryId);
            $('#Type').val(data.Type);
            $('#Title').val(data.Title || '');
            $('#Description').val(data.Description || '');
            $('#startDatePicker').val(data.StartDateShamsi || '');
            $('#endDatePicker').val(data.EndDateShamsi || '');
            $('#Severity').val(data.Severity || '');
            $('#IsActive').prop('checked', data.IsActive !== false);
            $('#DoctorName').val(data.DoctorName || '');
            $('#MedicalCenter').val(data.MedicalCenter || '');
            var meds = data.Medications || data.medications || [];
            var m = Array.isArray(meds) && meds.length ? meds[0] : null;
            if (m) {
                $('#DrugName').val(m.DrugName || m.drugName || '');
                $('#Dosage').val(m.Dosage || m.dosage || '');
                $('#DosageUnit').val(m.DosageUnit || m.dosageUnit || '');
                $('#Frequency').val(m.Frequency || m.frequency || '');
                $('#Route').val(m.Route || m.route || '');
                $('#Indication').val(m.Indication || m.indication || '');
                $('#PrescribingDoctor').val(m.PrescribingDoctor || m.prescribingDoctor || '');
            } else {
                $('#DrugName, #Dosage, #DosageUnit, #Frequency, #Route, #Indication, #PrescribingDoctor').val('');
            }
            var labs = data.LabResults || data.labResults || [];
            var lab0 = Array.isArray(labs) && labs.length ? labs[0] : null;
            if (lab0) {
                $('#LabName').val(lab0.LabName || lab0.labName || '');
                $('#LabValue').val(lab0.Value || lab0.value || '');
                $('#LabUnit').val(lab0.Unit || lab0.unit || '');
                $('#labDatePicker').val(lab0.LabDateShamsi || lab0.labDateShamsi || '');
                $('#LabReferenceRange').val(lab0.ReferenceRange || lab0.referenceRange || '');
            } else {
                $('#LabName, #LabValue, #LabUnit, #labDatePicker, #LabReferenceRange').val('');
            }
            $('#IsCritical').prop('checked', !!(data.IsCritical || data.isCritical));
            var diseaseMeds = data.MedicationsList || data.medicationsList || data.Medications || data.medications || [];
            if (Array.isArray(diseaseMeds) && diseaseMeds.length) {
                $('#diseaseMedicationsContainer').empty();
                diseaseMeds.forEach(function(m) { self.addDiseaseMedicationRow(m); });
            }
            this.toggleTypeBlocks();
        },
        
        /**
         * ✅ Save Medical History (Create/Update)
         */
        saveMedicalHistory: function() {
            var self = this;
            var $form = $('#medicalHistoryForm');
            var $btn = $('#saveMedicalHistoryBtn');
            var medicalHistoryId = $('#MedicalHistoryId').val();
            var isEdit = medicalHistoryId && medicalHistoryId !== '';
            var typeVal = $('#Type').val();
            var isMedication = (typeVal === '3');
            var titleVal = ($('#Title').val() || '').trim();
            var drugNameVal = ($('#DrugName').val() || '').trim();
            if (isMedication && !titleVal && !drugNameVal) {
                Swal.fire({ title: 'خطا', text: 'برای نوع دارو، عنوان یا نام دارو الزامی است.', icon: 'error', confirmButtonText: 'باشه' });
                return;
            }
            
            // Disable button
            $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin ml-1"></i> در حال ذخیره...');
            
            // Get form data (FormData شامل __RequestVerificationToken داخل همین فرم است)
            var formData = new FormData($form[0]);
            // ✅ اگر توکن داخل فرم نبود (مثلاً لود AJAX)، از همین فرم بگیر تا با کوکی سرور جور باشد
            var token = $form.find('input[name="__RequestVerificationToken"]').val();
            if (token && !formData.has('__RequestVerificationToken')) {
                formData.append('__RequestVerificationToken', token);
            }
            // ✅ ارسال صریح فیلدهای دارو برای ذخیره و ویرایش (در صورت مخفی بودن بلوک هم مقدار فعلی ارسال می‌شود)
            formData.set('DrugName', $form.find('#DrugName').val() || '');
            formData.set('Dosage', $form.find('#Dosage').val() || '');
            formData.set('DosageUnit', $form.find('#DosageUnit').val() || '');
            formData.set('Frequency', $form.find('#Frequency').val() || '');
            formData.set('Route', $form.find('#Route').val() || '');
            formData.set('Indication', $form.find('#Indication').val() || '');
            formData.set('PrescribingDoctor', $form.find('#PrescribingDoctor').val() || '');
            var diseaseMedsList = this.collectDiseaseMedications();
            formData.set('MedicationsListJson', JSON.stringify(diseaseMedsList));
            formData.set('LabName', $form.find('#LabName').val() || '');
            formData.set('LabValue', $form.find('#LabValue').val() || '');
            formData.set('LabUnit', $form.find('#LabUnit').val() || '');
            formData.set('LabDate', $form.find('#labDatePicker').val() || '');
            formData.set('LabReferenceRange', $form.find('#LabReferenceRange').val() || '');
            formData.set('IsCritical', $form.find('#IsCritical').prop('checked') ? 'true' : '');
            
            // ✅ URL از سرور (data-api-create / data-api-update) برای جلوگیری از 404
            var url = isEdit ? getUpdateMedicalHistoryUrl() : getCreateMedicalHistoryUrl();
            
            $.ajax({
                url: url,
                method: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                dataType: 'json',
                success: function(response) {
                    if (response && response.success) {
                        Swal.fire({
                            title: 'موفق',
                            text: response.message || 'با موفقیت ذخیره شد',
                            icon: 'success',
                            confirmButtonText: 'باشه'
                        }).then(function() {
                            // Close modal
                            var $modal = $('#medicalHistoryModal');
                            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                                var modalInstance = bootstrap.Modal.getInstance($modal[0]);
                                if (modalInstance) modalInstance.hide();
                            } else if ($modal.modal) {
                                $modal.modal('hide');
                            }
                            
                            // Reload medical history section
                            self.loadSection('medicalHistory');
                        });
                    } else {
                        Swal.fire({
                            title: 'خطا',
                            text: response?.message || 'خطا در ذخیره',
                            icon: 'error',
                            confirmButtonText: 'باشه'
                        });
                    }
                },
                error: function(xhr) {
                    console.error('Error saving medical history:', xhr);
                    var errorMessage = 'خطا در ذخیره';
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    }
                    Swal.fire({
                        title: 'خطا',
                        text: errorMessage,
                        icon: 'error',
                        confirmButtonText: 'باشه'
                    });
                },
                complete: function() {
                    $btn.prop('disabled', false).html('<i class="fas fa-save ml-1"></i> ذخیره');
                }
            });
        },
        
        /**
         * ✅ Delete Medical History
         */
        deleteMedicalHistory: function(medicalHistoryId) {
            var self = this;
            
            Swal.fire({
                title: 'آیا مطمئن هستید؟',
                text: 'این عمل قابل بازگشت نیست',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'بله، حذف کن',
                cancelButtonText: 'انصراف'
            }).then(function(result) {
                if (result.isConfirmed) {
                    var token = $('input[name="__RequestVerificationToken"]').first().val();
                    $.ajax({
                        url: getApiBaseUrl() + '/DeleteMedicalHistory',
                        method: 'POST',
                        data: { id: medicalHistoryId, __RequestVerificationToken: token },
                        headers: { 'X-Requested-With': 'XMLHttpRequest', 'RequestVerificationToken': token || '' },
                        dataType: 'json',
                        success: function(response) {
                            if (response && response.success) {
                                Swal.fire({
                                    title: 'موفق',
                                    text: response.message || 'با موفقیت حذف شد',
                                    icon: 'success',
                                    confirmButtonText: 'باشه'
                                }).then(function() {
                                    // Reload medical history section
                                    self.loadSection('medicalHistory');
                                });
                            } else {
                                Swal.fire({
                                    title: 'خطا',
                                    text: response?.message || 'خطا در حذف',
                                    icon: 'error',
                                    confirmButtonText: 'باشه'
                                });
                            }
                        },
                        error: function(xhr) {
                            console.error('Error deleting medical history:', xhr);
                            Swal.fire({
                                title: 'خطا',
                                text: 'خطا در حذف',
                                icon: 'error',
                                confirmButtonText: 'باشه'
                            });
                        }
                    });
                }
            });
        },
        
        /**
         * ✅ Preview Attachments
         */
        previewAttachments: function(files) {
            var $preview = $('#attachmentsPreview');
            $preview.empty();
            
            if (!files || files.length === 0) return;
            
            var maxFiles = 5;
            var maxSize = 5 * 1024 * 1024; // 5MB
            
            if (files.length > maxFiles) {
                Swal.fire({
                    title: 'خطا',
                    text: 'حداکثر ' + maxFiles + ' فایل می‌توانید انتخاب کنید',
                    icon: 'error',
                    confirmButtonText: 'باشه'
                });
                return;
            }
            
            for (var i = 0; i < files.length; i++) {
                var file = files[i];
                
                if (file.size > maxSize) {
                    Swal.fire({
                        title: 'خطا',
                        text: 'فایل ' + file.name + ' بیش از 5 مگابایت است',
                        icon: 'error',
                        confirmButtonText: 'باشه'
                    });
                    continue;
                }
                
                var $item = $('<div class="attachment-preview-item mb-2"></div>');
                $item.html(
                    '<i class="fas fa-file ml-2"></i>' +
                    '<span>' + file.name + '</span>' +
                    '<small class="text-muted mr-2">(' + (file.size / 1024 / 1024).toFixed(2) + ' MB)</small>'
                );
                $preview.append($item);
            }
        }
    };
    
    // ✅ Initialize on document ready
    $(document).ready(function() {
        if ($('#medicalRecordContainer').length > 0 || $('.medical-record-shell').length > 0) {
            MedicalRecord.init();
        }
    });
    
    // ✅ Expose globally
    window.MedicalRecord = MedicalRecord;
})(jQuery, window);

