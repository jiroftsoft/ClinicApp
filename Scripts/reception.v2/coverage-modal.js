// Scripts/reception.v2/coverage-modal.js
(function (w, $) {
  'use strict';

  var ns = w.ClinicApp = w.ClinicApp || {};
  ns.ReceptionV2 = ns.ReceptionV2 || {};
  
  var $modal;
  // Use the same API wrapper as other modules (from reception-api.js)
  var API = w.ReceptionAPI || {
    get: function(path, data) {
      return $.ajax({
        url: '/api/v1/reception' + path,
        type: 'GET',
        data: data || {},
        dataType: 'json',
        headers: {
          'X-Requested-With': 'XMLHttpRequest',
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        }
      });
    },
    ok: function(response) {
      if (response && response.Data !== undefined) return response.Data;
      if (response && response.data !== undefined) return response.data;
      return response;
    }
  };

  /**
   * فرمت مبلغ IRR
   */
  function formatIrr(amount) {
    if (amount == null || isNaN(amount)) return '—';
    return Math.round(amount).toLocaleString('fa-IR') + ' ریال';
  }

  /**
   * رندر Key-Value pairs در تب‌ها
   */
  function renderKeyValues($host, data) {
    if (!data || Object.keys(data).length === 0) {
      $host.html('<div class="text-center text-muted py-3">اطلاعاتی برای نمایش وجود ندارد</div>');
      return;
    }

    var html = '<div class="row g-2">';
    var pairs = [
      ['نام بیمه', data.PlanName],
      ['فرانشیز', data.FranchisePercent != null ? data.FranchisePercent + '%' : '—'],
      ['درصد پوشش', data.CoveragePercent != null ? data.CoveragePercent + '%' : '—'],
      ['سقف هر خدمت', data.CeilingPerServiceStr || '—'],
      ['سقف هر ویزیت', data.CeilingPerVisitStr || '—'],
      ['سقف ماهانه', data.CeilingMonthlyStr || '—'],
      ['باقی‌مانده سقف', data.RemainingCeilingStr || '—']
    ];

    pairs.forEach(function (p) {
      html += '<div class="col-6 col-md-4"><div class="border rounded p-2 small bg-light">' +
              '<span class="text-muted d-block mb-1">' + p[0] + '</span>' +
              '<div class="fw-bold">' + (p[1] || '—') + '</div></div></div>';
    });
    html += '</div>';
    $host.html(html);
  }

  /**
   * باز کردن Modal و بارگذاری داده‌ها
   */
  function open() {
    if (!$('#rv2-coverage-modal').length) {
      console.warn('🏥 V2: Coverage modal not found in DOM');
      return;
    }

    $modal = $modal || new bootstrap.Modal(document.getElementById('rv2-coverage-modal'), {});
    $modal.show();

    var s = ns.ReceptionV2.state || {};
    var req = {
      patientId: s?.patient?.PatientId || 0,
      basePlanId: s?.insurances?.BasePlanId || null,
      supplementaryPlanId: s?.insurances?.SupplementaryPlanId || null
    };

    // نمایش Loading
    $('#cov-base, #cov-supp, #cov-eff').html('<div class="text-center text-muted py-3"><i class="fas fa-spinner fa-spin me-2"></i>در حال بارگذاری...</div>');

    // فراخوانی API
    API.get('/insurance/coverage', req)
      .then(function(res) {
        var response = API.ok ? API.ok(res) : res;
        
        if (!response || (response.Success !== true && response.Success !== undefined)) {
          var errorMsg = response?.Message || response?.message || 'خطا در دریافت پوشش';
          $('#cov-base, #cov-supp, #cov-eff').html('<div class="text-danger py-3">' + errorMsg + '</div>');
          return;
        }

        var data = response.Data || response.data || response;
        renderKeyValues($('#cov-base'), data.Base);
        renderKeyValues($('#cov-supp'), data.Supplementary);
        
        // رندر جمع مؤثر
        if (data.Effective) {
          var eff = data.Effective;
          var effHtml = '<div class="row g-2">' +
            '<div class="col-12"><div class="border rounded p-3 bg-info bg-opacity-10">' +
            '<div class="fw-bold mb-2">پوشش مؤثر نهایی</div>' +
            '<div class="row">' +
            '<div class="col-6"><span class="text-muted">درصد پوشش مؤثر:</span> <strong>' + (eff.EffectiveCoveragePercent || 0) + '%</strong></div>' +
            '<div class="col-6"><span class="text-muted">سهم بیمار:</span> <strong>' + (eff.PatientSharePercent || 0) + '%</strong></div>' +
            '</div>';
          
          if (eff.Notes) {
            effHtml += '<div class="mt-2 small text-muted"><i class="fas fa-info-circle me-1"></i>' + eff.Notes + '</div>';
          }
          effHtml += '</div></div></div>';
          $('#cov-eff').html(effHtml);
        } else {
          renderKeyValues($('#cov-eff'), null);
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Coverage API error:', err);
        $('#cov-base, #cov-supp, #cov-eff').html('<div class="text-danger py-3">خطا در دریافت اطلاعات پوشش</div>');
      });
  }

  /**
   * رویداد باز کردن Modal
   */
  $(document).on('rv2:coverage:open', open);

  /**
   * Price Preview - محاسبه قیمت خدمت
   */
  $(document).on('click', '#cov-preview-btn', function () {
    var code = $('#cov-service-code').val()?.trim();
    if (!code) {
      toastr.warning('لطفاً کد یا نام خدمت را وارد کنید');
      return;
    }

    var s = ns.ReceptionV2.state || {};
    var req = {
      patientId: s?.patient?.PatientId || null,
      departmentId: s?.department?.DepartmentId || null,
      doctorId: s?.doctor?.DoctorId || null,
      basePlanId: s?.insurances?.BasePlanId || null,
      supplementaryPlanId: s?.insurances?.SupplementaryPlanId || null,
      serviceCodeOrName: code
    };

    $('#cov-preview-result').html('<i class="fas fa-spinner fa-spin me-1"></i>در حال محاسبه...');

    API.get('/item/price/preview', req)
      .then(function(res) {
        var response = API.ok ? API.ok(res) : res;
        
        if (!response || (response.Success !== true && response.Success !== undefined)) {
          var errorMsg = response?.Message || response?.message || 'خطا در محاسبه';
          $('#cov-preview-result').html('<span class="text-danger">' + errorMsg + '</span>');
          return;
        }

        var d = response.Data || response.data || response;
        var priceStr = d.PriceStr || formatIrr(d.Price);
        var patientShareStr = d.PatientShareStr || formatIrr(d.PatientShare);
        var effPct = d.EffectiveCoveragePercent || 0;

        $('#cov-preview-result').html(
          '<div class="small">' +
          '<div><strong>قیمت:</strong> ' + priceStr + '</div>' +
          '<div><strong>پوشش مؤثر:</strong> ' + effPct + '%</div>' +
          '<div><strong>سهم بیمار:</strong> ' + patientShareStr + '</div>' +
          '</div>'
        );
      })
      .catch(function(err) {
        console.error('🏥 V2: Price preview error:', err);
        $('#cov-preview-result').html('<span class="text-danger">خطا در ارتباط</span>');
      });
  });

  // Enter key برای Price Preview
  $(document).on('keypress', '#cov-service-code', function(e) {
    if (e.which === 13) {
      $('#cov-preview-btn').click();
    }
  });

  /**
   * ✅ باز کردن Modal جزئیات پوشش برای یک آیتم خاص (Item Coverage Modal)
   */
  function openItemCoverageModal(item, pricing) {
    if (!$('#rv2-coverage-modal').length) {
      console.warn('🏥 V2: Coverage modal not found in DOM');
      return;
    }

    var c = pricing?.Coverage || pricing?.coverage;
    if (!c) {
      toastr.warning('اطلاعات پوشش برای این آیتم موجود نیست');
      return;
    }

    $modal = $modal || new bootstrap.Modal(document.getElementById('rv2-coverage-modal'), {});
    
    // تغییر تب به "جزئیات آیتم" (یا تب جدید)
    var serviceName = item?.ServiceName || item?.serviceName || item?.Name || item?.name || '';
    var serviceCode = item?.ServiceCode || item?.serviceCode || item?.Code || item?.code || '';
    var qty = item?.Quantity || item?.quantity || 1;

    // ساخت HTML برای segments
    var segs = (c.Segments || []).map(function(s) {
      var payerText = s.Payer === 'BASE' ? 'بیمه پایه' : 
                      s.Payer === 'SUPP' ? 'بیمه تکمیلی' : 'بیمار';
      var reasonText = humanizeReason(s.Reason || 0);
      return (
        '<tr>' +
        '<td>' + payerText + '</td>' +
        '<td>' + formatIrr(s.AmountIRR || 0) + '</td>' +
        '<td>' + reasonText + '<div class="text-muted small mt-1">' + (s.Note || '') + '</div></td>' +
        '</tr>'
      );
    }).join('');

    // ساخت HTML برای caps/franchise
    var caps = '<ul class="mb-2">';
    if (c.BaseCapRemainingIRR != null) {
      caps += '<li>باقیمانده سقف پایه: <b>' + formatIrr(c.BaseCapRemainingIRR) + '</b></li>';
    }
    if (c.SuppCapRemainingIRR != null) {
      caps += '<li>باقیمانده سقف تکمیلی: <b>' + formatIrr(c.SuppCapRemainingIRR) + '</b></li>';
    }
    if (c.FranchiseIRR != null && c.FranchiseIRR > 0) {
      caps += '<li>فرانشیز: <b>' + formatIrr(c.FranchiseIRR) + '</b></li>';
    }
    caps += '</ul>';

    // ساخت HTML برای warnings
    var warns = (c.Warnings || []).map(function(w) {
      return '<div class="alert alert-warning py-1 my-1 small">' + w + '</div>';
    }).join('');

    var html = (
      '<div class="mb-2"><b>خدمت:</b> ' + serviceName + ' (' + serviceCode + ') × ' + qty + '</div>' +
      caps +
      warns +
      '<div class="table-responsive mt-3">' +
      '<table class="table table-sm table-bordered">' +
      '<thead><tr><th>پرداخت‌کننده</th><th>مبلغ (ریال)</th><th>دلیل</th></tr></thead>' +
      '<tbody>' + (segs || '<tr><td colspan="3" class="text-center text-muted">اطلاعاتی موجود نیست</td></tr>') + '</tbody>' +
      '</table>' +
      '</div>'
    );

    // نمایش در تب مؤثر (یا تب جدید)
    $('#cov-eff').html(html);
    
    // فعال کردن تب مؤثر
    $('#rv2-coverage-modal .nav-link[href="#tab-eff"]').tab('show');
    
    $modal.show();
  }

  /**
   * ✅ Humanize CoverageReasonCode
   */
  function humanizeReason(reasonCode) {
    switch (reasonCode) {
      case 1: return 'پوشش توسط بیمه پایه';
      case 2: return 'پوشش توسط بیمه تکمیلی';
      case 3: return 'سقف بیمه پایه پر شد';
      case 4: return 'سقف بیمه تکمیلی پر شد';
      case 5: return 'فرانشیز اعمال شد';
      case 6: return 'خارج از شمول پوشش';
      case 7: return 'پلن بیمه منقضی';
      case 8: return 'خدمت مستثنی';
      case 9: return 'تعرفه/تعین‌ست ناقص';
      case 10: return 'پزشک مجاز برای خدمت/دپارتمان نیست';
      default: return '—';
    }
  }

  // ✅ Click handler روی badge
  $(document).on('click', '.coverage-badge', function() {
    var $row = $(this).closest('tr');
    var item = $row.data('item');
    var pricing = $row.data('pricing');
    
    if (!item || !pricing) {
      console.warn('🏥 V2: Item or pricing data not found on row');
      return;
    }
    
    openItemCoverageModal(item, pricing);
  });

  // Export
  ns.ReceptionV2.CoverageModal = {
    open: open,
    openItemCoverageModal: openItemCoverageModal
  };

  console.log('🏥 V2: Coverage Modal initialized');

})(window, jQuery);

