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

  console.log('🏥 V2: Coverage Modal initialized');

})(window, jQuery);

