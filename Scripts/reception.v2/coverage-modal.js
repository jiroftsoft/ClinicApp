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
   * رندر Key-Value pairs در تب‌ها - بهبود یافته برای محیط درمانی
   */
  function renderKeyValues($host, data, isBase) {
    // اگر بیمه انتخاب نشده
    if (!data || Object.keys(data).length === 0) {
      $host.html(
        '<div class="coverage-empty-state">' +
        '<i class="fas fa-info-circle"></i>' +
        '<p class="mb-0">اطلاعات بیمه‌ای برای نمایش وجود ندارد</p>' +
        '<small>لطفاً بیمه بیمار را انتخاب کنید</small>' +
        '</div>'
      );
      return;
    }
    
    // ✅ بررسی تعرفه برای خدمت خاص
    var hasTariff = data.HasTariff !== false; // اگر undefined باشد، true فرض می‌کنیم
    var insuranceType = isBase ? 'بیمه پایه' : 'بیمه تکمیلی';

    var html = '';
    
    // ⚠️ هشدار تعرفه نشده
    if (!hasTariff) {
      html += '<div class="coverage-info-card" style="border-right-color: #ef4444; background: linear-gradient(135deg, #fee2e2 0%, #ffffff 100%);">' +
              '<div style="display: flex; align-items: center; gap: 0.75rem;">' +
              '<i class="fas fa-exclamation-triangle" style="font-size: 2rem; color: #ef4444;"></i>' +
              '<div>' +
              '<span class="coverage-label" style="color: #991b1b;"><i class="fas fa-alert-triangle me-1"></i>هشدار تعرفه</span>' +
              '<div class="coverage-value" style="color: #dc2626; font-size: 1rem; font-weight: 700;">این خدمت برای ' + insuranceType + ' تعرفه‌گذاری نشده است</div>' +
              '<small style="color: #7f1d1d; display: block; margin-top: 0.5rem;">لطفاً قبل از ادامه، تعرفه این خدمت را در سیستم ثبت کنید یا با واحد بیمه تماس بگیرید.</small>' +
              '</div>' +
              '</div>' +
              '</div>';
    }
    
    // نام بیمه - Card اصلی
    if (data.PlanName) {
      html += '<div class="coverage-info-card" style="border-right-color: #2c5aa0; background: linear-gradient(135deg, #dbeafe 0%, #ffffff 100%);">' +
              '<span class="coverage-label"><i class="fas fa-hospital me-1"></i>نام بیمه</span>' +
              '<div class="coverage-value highlight">' + data.PlanName + '</div>' +
              '</div>';
    }
    
    // Grid برای سایر اطلاعات
    html += '<div class="row g-3">';
    
    // فرانشیز
    if (data.FranchisePercent != null) {
      html += '<div class="col-md-6">' +
              '<div class="coverage-info-card" style="border-right-color: #f59e0b;">' +
              '<span class="coverage-label"><i class="fas fa-percentage me-1"></i>فرانشیز</span>' +
              '<div class="coverage-value">' + data.FranchisePercent + '%</div>' +
              '</div></div>';
    }
    
    // درصد پوشش
    if (data.CoveragePercent != null) {
      html += '<div class="col-md-6">' +
              '<div class="coverage-info-card" style="border-right-color: #10b981;">' +
              '<span class="coverage-label"><i class="fas fa-shield-alt me-1"></i>درصد پوشش</span>' +
              '<div class="coverage-value" style="color: #10b981;">' + data.CoveragePercent + '%</div>' +
              '</div></div>';
    }
    
    // سقف هر خدمت
    if (data.CeilingPerServiceStr && data.CeilingPerServiceStr !== '—') {
      html += '<div class="col-md-6">' +
              '<div class="coverage-info-card">' +
              '<span class="coverage-label"><i class="fas fa-hand-holding-medical me-1"></i>سقف هر خدمت</span>' +
              '<div class="coverage-value">' + data.CeilingPerServiceStr + '</div>' +
              '</div></div>';
    }
    
    // سقف هر ویزیت
    if (data.CeilingPerVisitStr && data.CeilingPerVisitStr !== '—') {
      html += '<div class="col-md-6">' +
              '<div class="coverage-info-card">' +
              '<span class="coverage-label"><i class="fas fa-user-md me-1"></i>سقف هر ویزیت</span>' +
              '<div class="coverage-value">' + data.CeilingPerVisitStr + '</div>' +
              '</div></div>';
    }
    
    // سقف ماهانه
    if (data.CeilingMonthlyStr && data.CeilingMonthlyStr !== '—') {
      html += '<div class="col-md-6">' +
              '<div class="coverage-info-card">' +
              '<span class="coverage-label"><i class="fas fa-calendar-alt me-1"></i>سقف ماهانه</span>' +
              '<div class="coverage-value">' + data.CeilingMonthlyStr + '</div>' +
              '</div></div>';
    }
    
    // باقی‌مانده سقف
    if (data.RemainingCeilingStr && data.RemainingCeilingStr !== '—') {
      var remainingColor = '#ef4444'; // قرمز برای باقی‌مانده کم
      if (data.RemainingCeilingPercent && data.RemainingCeilingPercent > 50) {
        remainingColor = '#10b981'; // سبز برای باقی‌مانده زیاد
      } else if (data.RemainingCeilingPercent && data.RemainingCeilingPercent > 20) {
        remainingColor = '#f59e0b'; // نارنجی برای باقی‌مانده متوسط
      }
      
      html += '<div class="col-md-6">' +
              '<div class="coverage-info-card" style="border-right-color: ' + remainingColor + ';">' +
              '<span class="coverage-label"><i class="fas fa-chart-line me-1"></i>باقی‌مانده سقف</span>' +
              '<div class="coverage-value" style="color: ' + remainingColor + ';">' + data.RemainingCeilingStr + '</div>' +
              '</div></div>';
    }
    
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
    $('#cov-base, #cov-supp, #cov-eff').html(
      '<div class="coverage-loading">' +
      '<i class="fas fa-spinner fa-spin"></i>' +
      '<p class="mt-2">در حال بارگذاری اطلاعات بیمه...</p>' +
      '</div>'
    );

    // فراخوانی API
    API.get('/insurance/coverage', req)
      .then(function(res) {
        var response = API.ok ? API.ok(res) : res;
        
        if (!response || (response.Success !== true && response.Success !== undefined)) {
          var errorMsg = response?.Message || response?.message || 'خطا در دریافت پوشش';
          $('#cov-base, #cov-supp, #cov-eff').html(
            '<div class="coverage-empty-state">' +
            '<i class="fas fa-exclamation-triangle" style="color: #ef4444;"></i>' +
            '<p class="mb-0" style="color: #ef4444;">' + errorMsg + '</p>' +
            '</div>'
          );
          return;
        }

        var data = response.Data || response.data || response;
        renderKeyValues($('#cov-base'), data.Base, true);  // true = isBase
        renderKeyValues($('#cov-supp'), data.Supplementary, false);  // false = not base
        
        // رندر جمع مؤثر
        if (data.Effective) {
          var eff = data.Effective;
          var coveragePercent = eff.EffectiveCoveragePercent || 0;
          var patientPercent = eff.PatientSharePercent || 0;
          
          // تعیین رنگ بر اساس درصد پوشش
          var coverageColor = '#ef4444'; // قرمز برای کم
          if (coveragePercent >= 70) {
            coverageColor = '#10b981'; // سبز برای زیاد
          } else if (coveragePercent >= 40) {
            coverageColor = '#f59e0b'; // نارنجی برای متوسط
          }
          
          var effHtml = '<div class="coverage-info-card" style="border-right-color: ' + coverageColor + '; background: linear-gradient(135deg, #e0f2fe 0%, #ffffff 100%);">' +
            '<span class="coverage-label"><i class="fas fa-chart-pie me-1"></i>پوشش مؤثر نهایی</span>' +
            '<div class="row g-3 mt-2">' +
            '<div class="col-md-6">' +
            '<div class="coverage-info-card" style="border-right-color: ' + coverageColor + ';">' +
            '<span class="coverage-label">درصد پوشش مؤثر</span>' +
            '<div class="coverage-value" style="color: ' + coverageColor + '; font-size: 2rem;">' + coveragePercent + '%</div>' +
            '</div></div>' +
            '<div class="col-md-6">' +
            '<div class="coverage-info-card" style="border-right-color: #ef4444;">' +
            '<span class="coverage-label">سهم بیمار</span>' +
            '<div class="coverage-value" style="color: #ef4444; font-size: 2rem;">' + patientPercent + '%</div>' +
            '</div></div>' +
            '</div>';
          
          if (eff.Notes) {
            effHtml += '<div class="mt-3 p-3 rounded" style="background-color: #fef3c7; border-right: 4px solid #f59e0b;">' +
                      '<i class="fas fa-info-circle me-2" style="color: #f59e0b;"></i>' +
                      '<strong style="color: #92400e;">توجه:</strong> ' +
                      '<span style="color: #78350f;">' + eff.Notes + '</span>' +
                      '</div>';
          }
          effHtml += '</div>';
          $('#cov-eff').html(effHtml);
        } else {
          renderKeyValues($('#cov-eff'), null);
        }
      })
      .catch(function(err) {
        console.error('🏥 V2: Coverage API error:', err);
        $('#cov-base, #cov-supp, #cov-eff').html(
          '<div class="coverage-empty-state">' +
          '<i class="fas fa-exclamation-triangle" style="color: #ef4444;"></i>' +
          '<p class="mb-0" style="color: #ef4444;">خطا در دریافت اطلاعات پوشش</p>' +
          '<small>لطفاً مجدداً تلاش کنید</small>' +
          '</div>'
        );
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
        
        // تعیین رنگ بر اساس درصد پوشش
        var coverageColor = '#ef4444';
        if (effPct >= 70) {
          coverageColor = '#10b981';
        } else if (effPct >= 40) {
          coverageColor = '#f59e0b';
        }

        $('#cov-preview-result').html(
          '<div style="width: 100%;">' +
          '<div style="margin-bottom: 0.5rem;"><strong style="color: #78350f;">قیمت:</strong> <span style="color: #1f2937; font-weight: 700;">' + priceStr + '</span></div>' +
          '<div style="margin-bottom: 0.5rem;"><strong style="color: #78350f;">پوشش:</strong> <span style="color: ' + coverageColor + '; font-weight: 700; font-size: 1.1rem;">' + effPct + '%</span></div>' +
          '<div><strong style="color: #78350f;">سهم بیمار:</strong> <span style="color: #ef4444; font-weight: 700;">' + patientShareStr + '</span></div>' +
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
  /**
   * ✅ باز کردن modal جزئیات پوشش برای یک آیتم
   * @param {Object} item - اطلاعات آیتم
   * @param {Object} pricing - اطلاعات pricing (اختیاری)
   * @param {Object} insurance - اطلاعات InsuranceCalculation (اختیاری)
   */
  function openItemCoverageModal(item, pricing, insurance) {
    if (!$('#rv2-coverage-modal').length) {
      console.warn('🏥 V2: Coverage modal not found in DOM');
      return;
    }

    // ✅ استخراج Coverage از منابع مختلف (اولویت: pricing > insurance > ساخت از item)
    // طبق الگوی موجود در پروژه - پشتیبانی از PascalCase و camelCase
    var c = null;
    
    // ✅ اولویت 1: از pricing (اگر CoverageDetailsDto در pricing موجود باشد)
    if (pricing) {
      c = pricing.Coverage || pricing.coverage;
    }
    
    // ✅ اولویت 2: از insurance (InsuranceCalculation) - ساخت CoverageDetailsDto
    if (!c && insurance) {
      c = buildCoverageFromInsurance(insurance, item, pricing);
    }
    
    // ✅ اولویت 3: از item - استخراج یا ساخت CoverageDetailsDto
    if (!c && item) {
      c = buildCoverageFromItem(item, pricing);
    }
    
    // ✅ بررسی نهایی - اگر Coverage پیدا نشد، خطا نمایش بده
    if (!c) {
      console.warn('🏥 V2: Coverage information not found in pricing, insurance, or item');
      console.warn('🏥 V2: Item:', item);
      console.warn('🏥 V2: Pricing:', pricing);
      console.warn('🏥 V2: Insurance:', insurance);
      toastr.warning('اطلاعات پوشش برای این آیتم موجود نیست. لطفاً دوباره محاسبه کنید.');
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
    
    // ✅ فعال کردن تب مؤثر (پشتیبانی از Bootstrap 5 و 4)
    try {
      var $tabLink = $('#rv2-coverage-modal .nav-link[href="#tab-eff"]');
      
      if ($tabLink.length > 0) {
        // Bootstrap 5 API
        if (window.bootstrap && typeof window.bootstrap.Tab !== 'undefined') {
          var tabElement = $tabLink[0];
          var tab = new bootstrap.Tab(tabElement);
          tab.show();
        }
        // Bootstrap 4 API (fallback)
        else if ($.fn.tab && typeof $tabLink.tab === 'function') {
          $tabLink.tab('show');
        }
        // Fallback: دستی فعال کردن تب
        else {
          // حذف active از همه تب‌ها
          $('#rv2-coverage-modal .nav-link').removeClass('active');
          $('#rv2-coverage-modal .tab-pane').removeClass('show active');
          
          // فعال کردن تب مؤثر
          $tabLink.addClass('active');
          $('#tab-eff').addClass('show active');
          
          console.log('🏥 V2: Tab activated manually (fallback)');
        }
      } else {
        console.warn('🏥 V2: Tab link not found for #tab-eff');
      }
    } catch (err) {
      console.error('🏥 V2: Error activating tab:', err);
      // Silent fail - modal still opens
    }
    
    $modal.show();
  }

  /**
   * ✅ ساخت Coverage از InsuranceCalculation
   * طبق الگوی ReceptionPricingService.BuildCoverageDetails
   * 
   * @param {Object} insurance - InsuranceCalculation (PrimaryCoverage, SupplementaryCoverage, etc. به صورت decimal)
   * @param {Object} item - اطلاعات آیتم (برای استخراج gross و snapshotJson)
   * @param {Object} pricing - اطلاعات pricing (اختیاری)
   * @returns {Object|null} CoverageDetailsDto structure
   */
  function buildCoverageFromInsurance(insurance, item, pricing) {
    if (!insurance) return null;
    
    try {
      // ✅ استخراج مقادیر با پشتیبانی از PascalCase و camelCase
      var primaryCoverage = insurance.PrimaryCoverage || insurance.primaryCoverage || 0;
      var supplementaryCoverage = insurance.SupplementaryCoverage || insurance.supplementaryCoverage || 0;
      var totalCoverage = insurance.TotalInsuranceCoverage || insurance.totalInsuranceCoverage || 0;
      var patientShare = insurance.PatientShare || insurance.patientShare || 0;
      var coverageStatus = insurance.CoverageStatus || insurance.coverageStatus || 'بدون پوشش';
      
      // ✅ تبدیل decimal به long (AmountIRR) - طبق الگوی C#
      // در JavaScript، decimal به صورت number است و باید به integer تبدیل شود
      var baseCoveredIRR = Math.round(primaryCoverage);
      var suppCoveredIRR = Math.round(supplementaryCoverage);
      var patientPayableIRR = Math.round(patientShare);
      
      // ✅ تعیین State - طبق CoverageState enum (None=0, Partial=1, Full=2)
      var state = 0; // None
      if (coverageStatus === 'پوشش کامل' || (patientPayableIRR === 0 && (baseCoveredIRR > 0 || suppCoveredIRR > 0))) {
        state = 2; // Full
      } else if (coverageStatus === 'پوشش ناقص' || (baseCoveredIRR > 0 || suppCoveredIRR > 0)) {
        state = 1; // Partial
      }
      
      // ✅ ساخت Segments - طبق الگوی ReceptionPricingService.BuildCoverageDetails
      var segments = [];
      
      // ✅ Segment بیمه پایه - طبق CoverageReasonCode.BaseCovered (1)
      if (baseCoveredIRR > 0) {
        segments.push({
          Payer: 'BASE',
          AmountIRR: baseCoveredIRR,
          Reason: 1, // BaseCovered (CoverageReasonCode.BaseCovered)
          Note: 'پوشش توسط بیمه پایه'
        });
      }
      
      // ✅ Segment بیمه تکمیلی - طبق CoverageReasonCode.SuppCovered (2)
      if (suppCoveredIRR > 0) {
        segments.push({
          Payer: 'SUPP',
          AmountIRR: suppCoveredIRR,
          Reason: 2, // SuppCovered (CoverageReasonCode.SuppCovered)
          Note: 'پوشش توسط بیمه تکمیلی'
        });
      }
      
      // ✅ Segment بیمار - طبق CoverageReasonCode.FranchiseApplied (5)
      if (patientPayableIRR > 0) {
        segments.push({
          Payer: 'PATIENT',
          AmountIRR: patientPayableIRR,
          Reason: 5, // FranchiseApplied (CoverageReasonCode.FranchiseApplied)
          Note: 'سهم بیمار'
        });
      }
      
      // ✅ ساخت CoverageDetailsDto - طبق CoverageDetailsDto structure
      return {
        State: state,
        Segments: segments,
        BaseCapRemainingIRR: null, // TODO: می‌توان از snapshotJson استخراج کرد
        SuppCapRemainingIRR: null, // TODO: می‌توان از snapshotJson استخراج کرد
        FranchiseIRR: null, // TODO: می‌توان از snapshotJson استخراج کرد
        Warnings: []
      };
    } catch (err) {
      console.error('🏥 V2: Error building coverage from insurance:', err);
      console.error('🏥 V2: Insurance data:', insurance);
      return null;
    }
  }
  
  /**
   * ✅ ساخت Coverage از item (fallback)
   * طبق الگوی موجود در پروژه - پشتیبانی از PascalCase و camelCase
   * 
   * @param {Object} item - اطلاعات آیتم (ممکن است InsuranceCalculation یا Coverage داشته باشد)
   * @param {Object} pricing - اطلاعات pricing (اختیاری)
   * @returns {Object|null} CoverageDetailsDto structure
   */
  function buildCoverageFromItem(item, pricing) {
    if (!item) return null;
    
    try {
      // ✅ اولویت 1: اگر item دارای InsuranceCalculation است، از آن استفاده کن
      var insurance = item.InsuranceCalculation || item.insuranceCalculation;
      if (insurance) {
        return buildCoverageFromInsurance(insurance, item, pricing);
      }
      
      // ✅ اولویت 2: اگر item دارای Coverage است، از آن استفاده کن
      var coverage = item.Coverage || item.coverage;
      if (coverage) {
        return coverage;
      }
      
      return null;
    } catch (err) {
      console.error('🏥 V2: Error building coverage from item:', err);
      console.error('🏥 V2: Item data:', item);
      return null;
    }
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
    var insurance = $row.data('insurance'); // ✅ اضافه شده: InsuranceCalculation
    
    if (!item) {
      console.warn('🏥 V2: Item data not found on row');
      toastr.warning('اطلاعات آیتم یافت نشد');
      return;
    }
    
    // ✅ اگر pricing وجود ندارد، از insurance یا item استفاده کن
    if (!pricing) {
      pricing = {};
    }
    
    // ✅ پاس دادن insurance به openItemCoverageModal
    openItemCoverageModal(item, pricing, insurance);
  });

  // Export
  ns.ReceptionV2.CoverageModal = {
    open: open,
    openItemCoverageModal: openItemCoverageModal,
    buildCoverageFromInsurance: buildCoverageFromInsurance,
    buildCoverageFromItem: buildCoverageFromItem
  };

  console.log('🏥 V2: Coverage Modal initialized');

})(window, jQuery);

