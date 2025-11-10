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
   * ✅ رندر Badge پوشش (با هشدار تعرفه)
   */
  function renderCoverageBadge(itemId, coverage) {
    if (!coverage) return '';

    // ⚠️ بررسی وجود تعرفه
    var hasTariff = coverage.HasTariff !== false; // اگر undefined باشد، true فرض می‌کنیم
    var missingBaseTariff = coverage.MissingBaseTariff === true;
    var missingSuppTariff = coverage.MissingSuppTariff === true;
    
    // اگر تعرفه وجود ندارد، badge قرمز نمایش بده
    if (!hasTariff || missingBaseTariff || missingSuppTariff) {
      var warningMsg = [];
      if (missingBaseTariff) warningMsg.push('بیمه پایه');
      if (missingSuppTariff) warningMsg.push('بیمه تکمیلی');
      
      var tariffWarning = warningMsg.length > 0 
        ? 'تعرفه ' + warningMsg.join(' و ') + ' ثبت نشده'
        : 'تعرفه ثبت نشده';
      
      return (
        '<span class="badge bg-danger coverage-badge tariff-warning"' +
        '        data-itemid="' + itemId + '"' +
        '        title="⚠️ ' + tariffWarning + ' - لطفاً با واحد بیمه تماس بگیرید">' +
        '<i class="fas fa-exclamation-triangle me-1"></i>' + tariffWarning +
        '</span>'
      );
    }

    var s = covStateClass(coverage.State || 0);
    var segments = coverage.Segments || [];
    
    // ساخت tooltip از segments
    var tip = segments.map(function (seg) {
      return '• ' + (seg.Note || '') + ' — ' + formatIRR(seg.AmountIRR || 0);
    }).join('<br>');

    if (!tip) tip = s.label;

    // ✅ بررسی وجود Popper.js قبل از اضافه کردن data-bs-toggle
    var hasPopper = false;
    try {
      if (typeof window.Popper !== 'undefined' && typeof window.Popper.createPopper === 'function') {
        hasPopper = true;
      } else if (typeof Popper !== 'undefined' && typeof Popper.createPopper === 'function') {
        hasPopper = true;
      }
    } catch (e) {
      hasPopper = false;
    }

    // ✅ اگر Popper موجود نیست یا Bootstrap موجود نیست، فقط از title استفاده کنیم (native tooltip)
    var useNativeTooltip = !hasPopper || !window.bootstrap || typeof window.bootstrap.Tooltip === 'undefined';
    
    // Escape HTML برای title (native tooltip HTML را نمی‌پذیرد)
    var tipText = tip.replace(/<br>/g, ' | ').replace(/'/g, "&#39;").replace(/"/g, "&quot;");
    
    if (useNativeTooltip) {
      // ✅ Fallback: فقط title (native browser tooltip)
      return (
        '<span class="badge ' + s.badge + ' coverage-badge"' +
        '        data-itemid="' + itemId + '"' +
        '        title="' + tipText + '">' +
        s.label +
        '</span>'
      );
    } else {
      // ✅ Bootstrap 5 Tooltip با Popper
      return (
        '<span class="badge ' + s.badge + ' coverage-badge"' +
        '        data-itemid="' + itemId + '"' +
        '        data-bs-toggle="tooltip"' +
        '        data-bs-html="true"' +
        '        data-bs-original-title="' + (tip.replace(/'/g, "&#39;") || s.label) + '"' +
        '        title="' + tipText + '">' +
        s.label +
        '</span>'
      );
    }
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

      // ✅ init tooltip (پشتیبانی از Bootstrap 5 و 4 با fallback کامل برای Popper)
      try {
        // ✅ بررسی دقیق وجود Popper.js (چندین روش)
        var hasPopper = false;
        try {
          // روش 1: بررسی window.Popper.createPopper (Popper v2)
          if (typeof window.Popper !== 'undefined' && 
              window.Popper && 
              typeof window.Popper.createPopper === 'function') {
            // تست عملی: سعی کن یک popper ساده بسازیم
            try {
              var testElement = document.createElement('div');
              var testPopper = window.Popper.createPopper(testElement, testElement, {});
              if (testPopper) {
                testPopper.destroy();
                hasPopper = true;
              }
            } catch (testErr) {
              hasPopper = false;
            }
          }
          // روش 2: بررسی Popper از namespace مستقیم
          if (!hasPopper && typeof Popper !== 'undefined' && typeof Popper.createPopper === 'function') {
            try {
              var testElement = document.createElement('div');
              var testPopper = Popper.createPopper(testElement, testElement, {});
              if (testPopper) {
                testPopper.destroy();
                hasPopper = true;
              }
            } catch (testErr) {
              hasPopper = false;
            }
          }
        } catch (e) {
          hasPopper = false;
        }
        
        // ✅ اگر Bootstrap 5 موجود است اما Popper موجود نیست، از title attribute استفاده کنیم
        if (window.bootstrap && typeof window.bootstrap.Tooltip !== 'undefined') {
          if (hasPopper) {
            // Bootstrap 5 API (با Popper) - فقط با hover init شود
            $('[data-bs-toggle="tooltip"]', $row).each(function() {
              try {
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
                var $el = $(this);
                var title = $el.data('bs-original-title') || $el.attr('title');
                if (title) {
                  $el.attr('title', title);
                  $el.removeAttr('data-bs-toggle');
                }
              }
            });
          } else {
            // Popper موجود نیست - از title attribute استفاده کنیم (native browser tooltip)
            console.warn('🏥 V2: Popper.js not found, using native tooltips');
            $('[data-bs-toggle="tooltip"]', $row).each(function() {
              var $el = $(this);
              var title = $el.data('bs-original-title') || $el.attr('title') || $el.attr('data-bs-original-title');
              if (title) {
                $el.attr('title', title);
                $el.removeAttr('data-bs-toggle'); // حذف data-bs-toggle تا Bootstrap تلاش نکند tooltip بسازد
              }
            });
          }
        } else if ($.fn.tooltip && typeof $('[data-toggle="tooltip"]', $row).tooltip === 'function') {
          // Bootstrap 4 API (fallback)
          $('[data-toggle="tooltip"]', $row).tooltip({ trigger: 'hover' });
        } else {
          // Ultimate fallback: استفاده از title attribute (native browser tooltip)
          $('[data-bs-toggle="tooltip"], [data-toggle="tooltip"]', $row).each(function() {
            var $el = $(this);
            var title = $el.data('bs-original-title') || $el.data('original-title') || $el.attr('title') || $el.attr('data-bs-original-title');
            if (title && !$el.attr('title')) {
              $el.attr('title', title);
              $el.removeAttr('data-bs-toggle data-toggle'); // حذف تا Bootstrap تلاش نکند
            }
          });
        }
      } catch (err) {
        console.warn('🏥 V2: Error initializing tooltips:', err);
        // Ultimate fallback: استفاده از title attribute
        $('[data-bs-toggle="tooltip"], [data-toggle="tooltip"]', $row).each(function() {
          var $el = $(this);
          var title = $el.data('bs-original-title') || $el.data('original-title') || $el.attr('title');
          if (title) {
            $el.attr('title', title);
            $el.removeAttr('data-bs-toggle data-toggle');
          }
        });
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

  /**
   * ✅ به‌روزرسانی یک ردیف با pricing (برای Reprice)
   * @param {number} receptionItemId - شناسه ReceptionItem
   * @param {object} pricing - PricingBreakdownDto
   */
  function updateRowPricing(receptionItemId, pricing) {
    if (!receptionItemId || !pricing) {
      console.warn('🏥 V2: updateRowPricing called with invalid params:', receptionItemId, pricing);
      return;
    }

    // پیدا کردن item از pricing (اگر موجود باشد)
    var item = {
      ReceptionItemId: receptionItemId,
      Id: receptionItemId,
      receptionItemId: receptionItemId,
      id: receptionItemId
    };

    // استفاده از renderRowWithPricing موجود
    renderRowWithPricing(item, pricing);
  }

  // ✅ Export
  ns.ReceptionV2.PricingUI = {
    renderRowWithPricing: renderRowWithPricing,
    renderCoverageBadge: renderCoverageBadge,
    updateTotalsUI: updateTotalsUI,
    updateRowPricing: updateRowPricing, // ✅ جدید: برای Reprice
    covStateClass: covStateClass
  };

  // ✅ Global برای استفاده در ماژول‌های دیگر
  w.renderRowWithPricing = renderRowWithPricing;
  w.updateTotalsUI = updateTotalsUI;
  w.updateRowPricing = updateRowPricing; // ✅ جدید

  console.log('🏥 V2: PricingUI module initialized');

})(window, jQuery);

