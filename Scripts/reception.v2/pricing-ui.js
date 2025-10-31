// Scripts/reception.v2/pricing-ui.js
// ✅ ماژول UI برای نمایش pricing و coverage (badge + highlight + tooltip)
(function (w, $) {
  'use strict';

  var ns = w.ClinicApp = w.ClinicApp || {};
  ns.ReceptionV2 = ns.ReceptionV2 || {};

  // ✅ استفاده از formatIRR از RxUtils (اگر موجود است)
  var formatIRR = window.RxUtils && typeof window.RxUtils.toIRR === 'function'
    ? window.RxUtils.toIRR
    : function (n) {
        try {
          return (Number(n) || 0).toLocaleString('fa-IR') + ' ریال';
        } catch (e) {
          return String(n || '—');
        }
      };

  /**
   * ✅ تعیین کلاس و برچسب CoverageState
   */
  function covStateClass(state) {
    switch (state) {
      case 2: // Full
        return { badge: 'cov-good', row: 'row-good', label: 'پوشش کامل' };
      case 1: // Partial
        return { badge: 'cov-warn', row: 'row-warn', label: 'پوشش ناقص' };
      default: // None
        return { badge: 'cov-bad', row: 'row-bad', label: 'بدون پوشش' };
    }
  }

  /**
   * ✅ رندر Badge پوشش
   */
  function renderCoverageBadge(itemId, coverage) {
    if (!coverage) return '';

    var s = covStateClass(coverage.State || 0);
    var segments = coverage.Segments || [];
    
    // ساخت tooltip از segments
    var tip = segments.map(function (seg) {
      return '• ' + (seg.Note || '') + ' — ' + formatIRR(seg.AmountIRR || 0);
    }).join('<br>');

    if (!tip) tip = s.label;

    return (
      '<span class="badge ' + s.badge + ' coverage-badge"' +
      '        data-itemid="' + itemId + '"' +
      '        data-bs-toggle="tooltip"' +
      '        data-bs-html="true"' +
      '        title="' + (tip.replace(/'/g, "&#39;") || s.label) + '">' +
      s.label +
      '</span>'
    );
  }

  /**
   * ✅ رندر ردیف با pricing کامل (اعداد + badge + highlight)
   */
  function renderRowWithPricing(item, pricing) {
    if (!item || !pricing) return;

    var rowId = 'row-' + (item.ReceptionItemId || item.Id || item.receptionItemId || item.id);
    var $row = $('#' + rowId);
    
    // اگر ردیف وجود ندارد، سعی کن با data-reception-item-id پیدا کن
    if (!$row.length) {
      var receptionItemId = item.ReceptionItemId || item.receptionItemId;
      if (receptionItemId) {
        $row = $('[data-reception-item-id="' + receptionItemId + '"]');
      }
    }

    if (!$row.length) {
      console.warn('🏥 V2: Row not found for item:', item);
      return;
    }

    // ✅ به‌روزرسانی اعداد (پشتیبانی از PascalCase و camelCase)
    var unitPrice = pricing.UnitPriceIRR || pricing.unitPriceIRR || 0;
    var gross = pricing.GrossIRR || pricing.grossIRR || 0;
    var base = pricing.BaseCoveredIRR || pricing.baseCoveredIRR || 0;
    var supp = pricing.SuppCoveredIRR || pricing.suppCoveredIRR || 0;
    var patient = pricing.PatientPayableIRR || pricing.patientPayableIRR || 0;

    var unitPriceStr = pricing.UnitPriceIRRStr || pricing.unitPriceIRRStr || formatIRR(unitPrice);
    var grossStr = pricing.GrossIRRStr || pricing.grossIRRStr || formatIRR(gross);
    var baseStr = pricing.BaseCoveredIRRStr || pricing.baseCoveredIRRStr || formatIRR(base);
    var suppStr = pricing.SuppCoveredIRRStr || pricing.suppCoveredIRRStr || formatIRR(supp);
    var patientStr = pricing.PatientPayableIRRStr || pricing.patientPayableIRRStr || formatIRR(patient);

    // به‌روزرسانی سلول‌ها (اگر وجود دارند)
    $row.find('.cell-unit, td:nth-child(4)').text(unitPriceStr);
    $row.find('.cell-gross, td:nth-child(5)').text(grossStr);
    $row.find('.cell-base, td:nth-child(6)').text(baseStr);
    $row.find('.cell-supp, td:nth-child(7)').text(suppStr);
    $row.find('.cell-patient, td:nth-child(8)').text(patientStr);

    // ✅ پوشش + highlight
    if (pricing.Coverage || pricing.coverage) {
      var coverage = pricing.Coverage || pricing.coverage;
      var s = covStateClass(coverage.State || 0);

      // حذف کلاس‌های قبلی و اضافه کردن جدید
      $row.removeClass('row-good row-warn row-bad').addClass(s.row);

      // رندر badge در ستون پوشش (اگر وجود دارد)
      var badgeHtml = renderCoverageBadge(item.ReceptionItemId || item.receptionItemId || item.Id || item.id, coverage);
      var $coverageCell = $row.find('.cell-coverage, td:last-child').first();
      
      // اگر ستون coverage وجود دارد، badge را اضافه کن
      if ($coverageCell.length && $coverageCell.find('.coverage-badge').length === 0) {
        // badge را قبل از دکمه حذف اضافه کن
        var $removeBtn = $coverageCell.find('.remove-item');
        if ($removeBtn.length) {
          $removeBtn.before(badgeHtml + ' ');
        } else {
          $coverageCell.prepend(badgeHtml + ' ');
        }
      }

      // ✅ ذخیره item و pricing در data برای modal
      $row.data('item', item);
      $row.data('pricing', pricing);

      // ✅ init tooltip (پشتیبانی از Bootstrap 5 و 4 با fallback برای Popper)
      try {
        // بررسی وجود Popper.js (Bootstrap 5 نیاز دارد)
        var hasPopper = typeof window.Popper !== 'undefined' || 
                       (window.bootstrap && window.bootstrap.Tooltip && 
                        typeof window.bootstrap.Tooltip.prototype !== 'undefined');
        
        if (window.bootstrap && typeof window.bootstrap.Tooltip !== 'undefined' && hasPopper) {
          // Bootstrap 5 API (با Popper)
          $('[data-bs-toggle="tooltip"]', $row).each(function() {
            try {
              // بررسی اینکه آیا tooltip قبلاً ایجاد شده یا نه
              var existingTooltip = bootstrap.Tooltip.getInstance(this);
              if (existingTooltip) {
                existingTooltip.dispose();
              }
              new bootstrap.Tooltip(this, { 
                trigger: 'hover',
                html: true,
                fallbackPlacements: ['top', 'bottom', 'left', 'right']
              });
            } catch (err) {
              // اگر Popper خطا داد، از title استفاده کنیم
              console.warn('🏥 V2: Error creating Bootstrap 5 tooltip (Popper issue):', err.message);
              // Fallback: استفاده از title attribute
              var $el = $(this);
              if (!$el.attr('title') && $el.data('bs-original-title')) {
                $el.attr('title', $el.data('bs-original-title'));
              }
            }
          });
        } else if ($.fn.tooltip && typeof $('[data-toggle="tooltip"]', $row).tooltip === 'function') {
          // Bootstrap 4 API (fallback)
          $('[data-toggle="tooltip"]', $row).tooltip({ trigger: 'hover' });
        } else {
          // Ultimate fallback: استفاده از title attribute و CSS
          $('[data-bs-toggle="tooltip"], [data-toggle="tooltip"]', $row).each(function() {
            var $el = $(this);
            var title = $el.data('bs-original-title') || $el.data('original-title') || $el.attr('title');
            if (title && !$el.attr('title')) {
              $el.attr('title', title);
            }
          });
        }
      } catch (err) {
        console.warn('🏥 V2: Error initializing tooltips:', err);
        // Silent fail - tooltips optional
      }
    }
  }

  /**
   * ✅ به‌روزرسانی Totals در UI (قبلاً در insurance-panel.js موجود است، اینجا هم export می‌کنیم)
   */
  function updateTotalsUI(totals) {
    if (!totals) return;

    var gross = totals.GrossIRR || totals.grossIRR || totals.Gross || totals.gross || 0;
    var base = totals.BaseCoveredIRR || totals.baseCoveredIRR || totals.Base || totals.base || 0;
    var supp = totals.SuppCoveredIRR || totals.suppCoveredIRR || totals.Supplementary || totals.supplementary || 0;
    var patient = totals.PatientPayableIRR || totals.patientPayableIRR || totals.Patient || totals.patient || 0;

    var grossStr = totals.GrossIRRStr || totals.grossIRRStr || formatIRR(gross);
    var baseStr = totals.BaseCoveredIRRStr || totals.baseCoveredIRRStr || formatIRR(base);
    var suppStr = totals.SuppCoveredIRRStr || totals.suppCoveredIRRStr || formatIRR(supp);
    var patientStr = totals.PatientPayableIRRStr || totals.patientPayableIRRStr || formatIRR(patient);

    $('#Gross, #SumGross').text(grossStr).attr('data-value', gross);
    $('#InsurancePayable, #SumBase').text(baseStr).attr('data-value', base);
    $('#SuppPayable, #SumSupp').text(suppStr).attr('data-value', supp);
    $('#PatientPayable, #SumPatient').text(patientStr).attr('data-value', patient);
  }

  // ✅ Export
  ns.ReceptionV2.PricingUI = {
    renderRowWithPricing: renderRowWithPricing,
    renderCoverageBadge: renderCoverageBadge,
    updateTotalsUI: updateTotalsUI,
    covStateClass: covStateClass
  };

  // ✅ Global برای استفاده در ماژول‌های دیگر
  w.renderRowWithPricing = renderRowWithPricing;
  w.updateTotalsUI = updateTotalsUI;

  console.log('🏥 V2: PricingUI module initialized');

})(window, jQuery);

