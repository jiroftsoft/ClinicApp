/**
 * Patient Education - Index (فیلتر و صفحه‌بندی بدون رفرش)
 * بدون رفرش صفحه با AJAX و به‌روزرسانی URL (pushState)
 */
(function () {
    'use strict';

    var apiUrl = '';
    var currentCategory = null;
    var currentPage = 1;
    var isLoading = false;
    var loadRequestId = 0;

    function escapeHtml(str) {
        if (str == null || str === undefined) return '';
        var div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    function getFilterLinksContainer() {
        return document.querySelector('.patient-education-filter-list');
    }

    function getGridContainer() {
        return document.getElementById('materialsContainer');
    }

    function getPaginationContainer() {
        return document.querySelector('.patient-education-pagination-wrap');
    }

    function setLoading(show) {
        var grid = getGridContainer();
        if (!grid) return;
        if (show) {
            grid.setAttribute('aria-busy', 'true');
            grid.classList.add('patient-education-loading');
        } else {
            grid.removeAttribute('aria-busy');
            grid.classList.remove('patient-education-loading');
        }
    }

    function setActiveFilter(category) {
        var list = getFilterLinksContainer();
        if (!list) return;
        var links = list.querySelectorAll('a[data-category]');
        var value = category === null || category === '' ? '' : String(category);
        links.forEach(function (a) {
            var isActive = a.getAttribute('data-category') === value;
            a.classList.toggle('is-active', isActive);
            a.setAttribute('aria-current', isActive ? 'true' : null);
        });
    }

    function renderCards(items, detailsUrlBase) {
        if (!items || items.length === 0) {
            return '<div class="patient-education-empty">' +
                '<i class="fas fa-info-circle" aria-hidden="true"></i>' +
                '<h2>مطلبی یافت نشد</h2>' +
                '<p>در حال حاضر مطلب آموزشی در این دسته‌بندی موجود نیست.</p>' +
                '</div>';
        }
        var html = '';
        items.forEach(function (m) {
            var detailsUrl = m.detailsUrl || (detailsUrlBase + '/' + m.patientEducationMaterialId);
            var imgHtml = '';
            if (m.thumbnailUrl) {
                imgHtml = '<img src="' + escapeHtml(m.thumbnailUrl) + '" alt="' + escapeHtml(m.title) + '" loading="lazy" width="400" height="240" />';
            } else if (m.imageUrl) {
                imgHtml = '<img src="' + escapeHtml(m.imageUrl) + '" alt="' + escapeHtml(m.title) + '" loading="lazy" width="400" height="240" />';
            } else {
                imgHtml = '<div class="patient-education-card-placeholder"><i class="fas fa-book-medical" aria-hidden="true"></i></div>';
            }
            var fileSpan = m.fileUrl ? '<span title="فایل موجود"><i class="fas fa-file" aria-hidden="true"></i> فایل</span>' : '';
            html += '<article class="patient-education-card" aria-label="' + escapeHtml(m.title) + '">' +
                '<div class="patient-education-card-img-wrap">' + imgHtml + '</div>' +
                '<div class="patient-education-card-body">' +
                '<span class="patient-education-card-category">' + escapeHtml(m.categoryDisplay || '') + '</span>' +
                '<h2 class="patient-education-card-title">' +
                '<a href="' + escapeHtml(detailsUrl) + '" aria-label="مشاهده: ' + escapeHtml(m.title) + '">' + escapeHtml(m.title) + '</a>' +
                '</h2>' +
                '<p class="patient-education-card-description">' + escapeHtml(m.description || '') + '</p>' +
                '<div class="patient-education-card-footer">' +
                '<div class="patient-education-card-stats">' + fileSpan +
                '<span><i class="fas fa-eye" aria-hidden="true"></i> ' + (m.viewCount || 0) + '</span>' +
                '<span><i class="fas fa-download" aria-hidden="true"></i> ' + (m.downloadCount || 0) + '</span>' +
                '</div>' +
                '<a href="' + escapeHtml(detailsUrl) + '" class="patient-education-card-btn" aria-label="مشاهده مطلب: ' + escapeHtml(m.title) + '">' +
                '<i class="fas fa-arrow-left" aria-hidden="true"></i> مشاهده</a>' +
                '</div></div></article>';
        });
        return html;
    }

    function renderPagination(data) {
        if (!data || data.totalPages < 2) return '';
        var cat = currentCategory != null ? currentCategory : '';
        var page = data.pageNumber || 1;
        var totalPages = data.totalPages || 1;
        var hasPrev = data.hasPreviousPage;
        var hasNext = data.hasNextPage;
        var base = window.location.pathname + (window.location.pathname.indexOf('PatientEducation') >= 0 ? '' : '');
        function pageUrl(p) {
            var q = '?page=' + p;
            if (cat !== '') q += '&category=' + cat;
            return base + '/GetMaterialsJson' + q;
        }
        var startPage = Math.max(1, page - 2);
        var endPage = Math.min(totalPages, page + 2);
        var html = '<ul class="pagination justify-content-center">';
        if (hasPrev) {
            html += '<li class="page-item"><a class="page-link pe-ajax-page" href="#" data-page="' + (page - 1) + '" aria-label="صفحه قبلی"><i class="fas fa-chevron-right" aria-hidden="true"></i> قبلی</a></li>';
        } else {
            html += '<li class="page-item disabled"><span class="page-link"><i class="fas fa-chevron-right" aria-hidden="true"></i> قبلی</span></li>';
        }
        if (startPage > 1) {
            html += '<li class="page-item"><a class="page-link pe-ajax-page" href="#" data-page="1">1</a></li>';
            if (startPage > 2) html += '<li class="page-item disabled"><span class="page-link">…</span></li>';
        }
        for (var i = startPage; i <= endPage; i++) {
            if (i === page) {
                html += '<li class="page-item active"><span class="page-link" aria-current="page">' + i + '</span></li>';
            } else {
                html += '<li class="page-item"><a class="page-link pe-ajax-page" href="#" data-page="' + i + '">' + i + '</a></li>';
            }
        }
        if (endPage < totalPages) {
            if (endPage < totalPages - 1) html += '<li class="page-item disabled"><span class="page-link">…</span></li>';
            html += '<li class="page-item"><a class="page-link pe-ajax-page" href="#" data-page="' + totalPages + '">' + totalPages + '</a></li>';
        }
        if (hasNext) {
            html += '<li class="page-item"><a class="page-link pe-ajax-page" href="#" data-page="' + (page + 1) + '" aria-label="صفحه بعدی">بعدی <i class="fas fa-chevron-left" aria-hidden="true"></i></a></li>';
        } else {
            html += '<li class="page-item disabled"><span class="page-link">بعدی <i class="fas fa-chevron-left" aria-hidden="true"></i></span></li>';
        }
        html += '</ul>';
        return html;
    }

    function buildIndexUrl(cat, p) {
        var base = apiUrl ? apiUrl.replace(/\/GetMaterialsJson.*$/, '') : window.location.pathname.replace(/\/GetMaterialsJson.*$/, '').replace(/\/Index\/?$/, '');
        if (base.slice(-1) === '/') base = base.slice(0, -1);
        var q = [];
        if (cat !== null && cat !== '' && cat !== undefined) q.push('category=' + encodeURIComponent(cat));
        if (p > 1) q.push('page=' + p);
        return q.length ? base + '?' + q.join('&') : base;
    }

    function load(category, page) {
        if (isLoading) return;
        var grid = getGridContainer();
        if (!grid) return;

        currentCategory = category === '' || category === undefined ? null : category;
        currentPage = page || 1;
        loadRequestId += 1;
        var thisRequestId = loadRequestId;
        isLoading = true;
        setLoading(true);
        setActiveFilter(currentCategory);

        var url = apiUrl + (apiUrl.indexOf('?') >= 0 ? '&' : '?') + 'page=' + currentPage + (currentCategory != null ? '&category=' + currentCategory : '');

        fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (res) {
                if (!res.ok) return res.text().then(function () { throw new Error('HTTP ' + res.status); });
                return res.json();
            })
            .then(function (json) {
                if (thisRequestId !== loadRequestId) return;
                if (json && json.success === true) {
                    try {
                        var base = apiUrl ? apiUrl.replace(/\/GetMaterialsJson.*$/, '') : window.location.pathname.replace(/\/GetMaterialsJson.*$/, '');
                        if (base.slice(-1) === '/') base = base.slice(0, -1);
                        var detailsBase = base + '/Details/';
                        grid.innerHTML = renderCards(json.items || [], detailsBase);

                        var pagWrap = getPaginationContainer();
                        if (!pagWrap && json.totalPages > 1) {
                            pagWrap = document.createElement('nav');
                            pagWrap.className = 'patient-education-pagination-wrap';
                            pagWrap.setAttribute('aria-label', 'صفحه‌بندی مطالب');
                            var gridEl = getGridContainer();
                            if (gridEl && gridEl.parentNode) gridEl.parentNode.insertBefore(pagWrap, gridEl.nextSibling);
                        }
                        if (pagWrap) {
                            if (json.totalPages > 1) {
                                pagWrap.innerHTML = renderPagination(json);
                                pagWrap.style.display = '';
                                pagWrap.querySelectorAll('.pe-ajax-page').forEach(function (a) {
                                    a.addEventListener('click', onPageClick);
                                });
                            } else {
                                pagWrap.innerHTML = '';
                                pagWrap.style.display = 'none';
                            }
                        }

                        var newUrl = buildIndexUrl(currentCategory, currentPage);
                        if (window.history && window.history.pushState) {
                            window.history.pushState({ category: currentCategory, page: currentPage }, '', newUrl);
                        }
                        grid.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    } catch (err) {
                        grid.innerHTML = '<div class="patient-education-empty"><i class="fas fa-info-circle" aria-hidden="true"></i><h2>مطلبی یافت نشد</h2><p>در حال حاضر مطلب آموزشی در این دسته‌بندی موجود نیست.</p></div>';
                    }
                } else {
                    grid.innerHTML = '<div class="patient-education-empty"><i class="fas fa-info-circle" aria-hidden="true"></i><h2>مطلبی یافت نشد</h2><p>در حال حاضر مطلب آموزشی در این دسته‌بندی موجود نیست.</p></div>';
                }
            })
            .catch(function () {
                if (thisRequestId !== loadRequestId) return;
                grid.innerHTML = '<div class="patient-education-empty"><i class="fas fa-exclamation-triangle" aria-hidden="true"></i><h2>خطا در بارگذاری</h2><p>لطفاً دوباره تلاش کنید.</p></div>';
            })
            .then(function () {
                if (thisRequestId === loadRequestId) {
                    isLoading = false;
                    setLoading(false);
                }
            });
    }

    function onFilterClick(e) {
        var a = e.target.closest('a[data-category]');
        if (!a) return;
        e.preventDefault();
        var cat = a.getAttribute('data-category');
        load(cat === '' ? null : cat, 1);
    }

    function onPageClick(e) {
        var a = e.target.closest('a.pe-ajax-page');
        if (!a) return;
        e.preventDefault();
        var p = parseInt(a.getAttribute('data-page'), 10);
        if (!isNaN(p) && p >= 1) load(currentCategory, p);
    }

    function onPopState() {
        var params = new URLSearchParams(window.location.search);
        var cat = params.get('category');
        var page = parseInt(params.get('page'), 10) || 1;
        currentCategory = cat === null || cat === '' ? null : cat;
        currentPage = page;
        load(currentCategory, currentPage);
    }

    function init() {
        var grid = getGridContainer();
        if (!grid) return;

        var scriptEl = document.currentScript || document.querySelector('script[src*="patient-education-index"]');
        if (scriptEl && scriptEl.getAttribute('data-api-url')) {
            apiUrl = scriptEl.getAttribute('data-api-url');
        } else {
            var base = window.location.pathname.replace(/\/GetMaterialsJson.*$/, '').replace(/\/Index\/?$/, '');
            if (base.slice(-1) === '/') base = base.slice(0, -1);
            apiUrl = base + '/GetMaterialsJson';
        }

        var filterList = getFilterLinksContainer();
        if (filterList) {
            var activeFilter = filterList.querySelector('a.is-active');
            if (activeFilter) {
                var ac = activeFilter.getAttribute('href') || '';
                var mCat = ac.match(/category=(\d+)/);
                currentCategory = mCat ? mCat[1] : null;
            }
            filterList.querySelectorAll('a').forEach(function (a) {
                var href = a.getAttribute('href') || '';
                var cat = (href.match(/category=(\d+)/) || [])[1] || '';
                a.setAttribute('data-category', cat);
                a.addEventListener('click', onFilterClick);
            });
        }

        var pagWrap = getPaginationContainer();
        if (pagWrap) {
            var activePageEl = pagWrap.querySelector('.page-item.active .page-link');
            if (activePageEl) currentPage = parseInt(activePageEl.textContent, 10) || 1;
            pagWrap.querySelectorAll('a[href*="page="]').forEach(function (a) {
                var m = (a.getAttribute('href') || '').match(/page=(\d+)/);
                if (m) {
                    a.classList.add('pe-ajax-page');
                    a.setAttribute('data-page', m[1]);
                    a.addEventListener('click', onPageClick);
                }
            });
        }

        if (window.history && window.history.pushState) {
            window.addEventListener('popstate', onPopState);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
