// Scripts/reception.v2/summary-header.js
(function (w, $) {
  'use strict';

  w.ClinicApp = w.ClinicApp || {};
  var ns = w.ClinicApp.ReceptionV2 = w.ClinicApp.ReceptionV2 || {};

  ns.state = ns.state || {
    patient: null,         // { PatientId, NationalCode, FirstName, LastName, Gender, BirthDate, Address, Mobile, BirthDateShamsi }
    department: null,      // { DepartmentId, Name }
    doctor: null,          // { DoctorId, FullName }
    insurances: null,      // { BasePlanId, BasePlanName, SupplementaryPlanId, SupplementaryPlanName }
    financialYear: null    // { Year, YearTitle }
  };

  /**
   * محاسبه سن از تاریخ تولد ISO
   */
  function calcAgeStr(birthDateIso) {
    if (!birthDateIso) return "—";
    try {
      var bd = new Date(birthDateIso);
      if (isNaN(bd.getTime())) return "—";
      var diff = Date.now() - bd.getTime();
      var ageDate = new Date(diff);
      var years = Math.abs(ageDate.getUTCFullYear() - 1970);
      return years + " سال";
    } catch (e) {
      return "—";
    }
  }

  /**
   * فرمت جنسیت برای نمایش
   */
  function formatGender(gender) {
    if (!gender) return "—";
    var g = String(gender).toLowerCase();
    if (g === 'male' || g === 'm' || g === '1') return 'مرد';
    if (g === 'female' || g === 'f' || g === '2') return 'زن';
    return gender;
  }

  /**
   * پر کردن بخش هویت
   */
  function fillIdentity(pat) {
    if (!pat) {
      pat = {};
    }

    $('#id-nationalCode').val(pat.NationalCode || '');
    $('#id-firstName').val(pat.FirstName || '');
    $('#id-lastName').val(pat.LastName || '');
    $('#id-mobile').val(pat.Mobile || '');
    $('#id-gender').val(pat.GenderTitle || formatGender(pat.Gender) || '');
    $('#id-birthdate-sh').val(pat.BirthDateShamsi || '');
    $('#id-address').val(pat.Address || '');

    // لینک ویرایش بیمار
    if (pat.PatientId) {
      var editUrl = '/Patient/Edit/' + pat.PatientId;
      $('#rv2-edit-patient-link').attr('href', editUrl).removeClass('d-none');
    } else {
      $('#rv2-edit-patient-link').addClass('d-none').attr('href', '#');
    }
  }

  /**
   * رندر خلاصه در هدر
   */
  function renderSummary() {
    var s = ns.state;
    var pat = s.patient || {};
    var ins = s.insurances || {};
    var fy = s.financialYear || {};
    var dept = s.department || {};
    var doc = s.doctor || {};

    // هدر خلاصه - بیمار
    var fullName = (pat.FirstName && pat.LastName) ? (pat.FirstName + ' ' + pat.LastName) : '—';
    $('[data-field="patient-fullname"]').text(fullName);
    $('[data-field="patient-gender"]').text(pat.GenderTitle || formatGender(pat.Gender) || '—');
    $('[data-field="patient-nc"]').text(pat.NationalCode || '—');
    $('[data-field="patient-age"]').text(calcAgeStr(pat.BirthDateIso || pat.BirthDate));
    $('[data-field="patient-address"]').text(pat.Address || '—');

    // هدر خلاصه - دپارتمان و پزشک
    $('[data-field="department-name"]').text(dept.Name || '—');
    $('[data-field="doctor-name"]').text(doc.FullName || doc.Name || '—');

    // هدر خلاصه - بیمه‌ها
    var basePlanName = ins.BasePlanName || '—';
    var suppPlanName = ins.SupplementaryPlanName || '—';
    $('[data-field="base-ins-name"]').text('بیمه پایه: ' + basePlanName);
    $('[data-field="supp-ins-name"]').text('تکمیلی: ' + suppPlanName);

    // هدر خلاصه - سال مالی
    var fyText = fy.YearTitle || (fy.Year ? 'سال مالی: ' + fy.Year : '—');
    $('[data-field="fy-name"]').text(fyText);

    // بخش هویت
    fillIdentity(pat);
  }

  /**
   * رویداد کلیک: باز کردن پرونده بیمار
   */
  $(document).on('click', '[data-action="open-patient"]', function () {
    var s = ns.state;
    if (!s?.patient?.PatientId) {
      toastr.warning('ابتدا بیمار را انتخاب کنید');
      return;
    }
    var url = '/Patient/Edit/' + s.patient.PatientId;
    window.open(url, '_blank');
  });

  /**
   * رویداد کلیک: رفتن به انتخاب دپارتمان
   */
  $(document).on('click', '[data-action="goto-dept"]', function () {
    var el = document.querySelector('#DepartmentId');
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'center' });
      setTimeout(function() {
        el.focus();
        if (el.tagName === 'SELECT') {
          $(el).select2('open');
        }
      }, 300);
    } else {
      toastr.info('بخش انتخاب دپارتمان یافت نشد');
    }
  });

  /**
   * رویداد کلیک: رفتن به انتخاب پزشک
   */
  $(document).on('click', '[data-action="goto-doctor"]', function () {
    var el = document.querySelector('#DoctorId');
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'center' });
      setTimeout(function() {
        el.focus();
        if (el.tagName === 'SELECT') {
          $(el).select2('open');
        }
      }, 300);
    } else {
      toastr.info('بخش انتخاب پزشک یافت نشد');
    }
  });

  /**
   * رویداد کلیک: باز کردن Modal جزئیات پوشش
   */
  $(document).on('click', '[data-action="open-coverage"]', function () {
    $(document).trigger('rv2:coverage:open');
  });

  /**
   * رویداد یکپارچه: هر ماژول بعد از تغییر state باید این را تریگر کند
   */
  $(document).on('rv2:stateChanged', function (e, newState) {
    if (newState) {
      ns.state = $.extend(true, {}, ns.state, newState);
    }
    renderSummary();
  });

  // در شروع صفحه هم یک‌بار رندر خالی داشته باشیم
  $(document).ready(function () {
    renderSummary();
    console.log('🏥 V2: Summary Header initialized');
  });

})(window, jQuery);

