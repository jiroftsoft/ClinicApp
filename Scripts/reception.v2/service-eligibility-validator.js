/**
 * 🏥 Service Eligibility Validator - اعتبارسنجی صلاحیت خدمت
 * 
 * این ماژول قبل از اضافه کردن خدمت به پذیرش، بررسی می‌کند که:
 * ✅ سن بیمار مناسب است (AgeMin/AgeMax)
 * ✅ جنسیت بیمار مناسب است (GenderLimit)
 * ✅ پیام‌های واضح به کاربر
 * 
 * @version 1.0.0
 * @date 1404/10/05
 */

(function(window, $) {
  'use strict';

  // =====================================================
  // 🔧 AGE CALCULATION - محاسبه سن
  // =====================================================

  /**
   * محاسبه سن دقیق از تاریخ تولد شمسی
   * @param {string} birthDateShamsi - تاریخ تولد شمسی (1370/01/01)
   * @returns {number|null} - سن بیمار یا null
   */
  function calculateAge(birthDateShamsi) {
    if (!birthDateShamsi) return null;
    
    try {
      // تبدیل تاریخ شمسی به میلادی (فرض: تابع کمکی موجود است)
      const birthDate = convertPersianToGregorian(birthDateShamsi);
      if (!birthDate) return null;
      
      const today = new Date();
      let age = today.getFullYear() - birthDate.getFullYear();
      const monthDiff = today.getMonth() - birthDate.getMonth();
      
      if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
        age--;
      }
      
      return age >= 0 ? age : null;
    } catch (e) {
      console.error('خطا در محاسبه سن:', e);
      return null;
    }
  }

  /**
   * تبدیل تاریخ شمسی به میلادی
   * این تابع باید با PersianDateHelper سمت سرور هماهنگ باشد
   */
  function convertPersianToGregorian(persianDate) {
    // TODO: پیاده‌سازی کامل تبدیل تاریخ
    // فعلاً از API سرور استفاده می‌کنیم
    return null;
  }

  // =====================================================
  // ✅ SERVICE ELIGIBILITY VALIDATION
  // =====================================================

  /**
   * بررسی صلاحیت بیمار برای دریافت خدمت
   * @param {Object} service - اطلاعات خدمت { ServiceId, AgeMin, AgeMax, GenderLimit, Title }
   * @param {Object} patient - اطلاعات بیمار { Age, Gender, BirthDateShamsi }
   * @returns {Object} { isEligible: boolean, message: string, reason: string }
   */
  function validateServiceEligibility(service, patient) {
    console.log('🔍 Validating Service Eligibility:', { service, patient });
    
    // بررسی محدودیت سنی - حداقل سن
    if (service.AgeMin !== null && service.AgeMin !== undefined) {
      if (patient.Age === null || patient.Age === undefined) {
        return {
          isEligible: false,
          message: `این خدمت نیاز به تاریخ تولد دارد. حداقل سن: ${service.AgeMin} سال`,
          reason: 'AGE_REQUIRED'
        };
      }
      
      if (patient.Age < service.AgeMin) {
        return {
          isEligible: false,
          message: `حداقل سن برای "${service.Title}" ${service.AgeMin} سال است. سن بیمار: ${patient.Age} سال`,
          reason: 'AGE_TOO_LOW'
        };
      }
    }
    
    // بررسی محدودیت سنی - حداکثر سن
    if (service.AgeMax !== null && service.AgeMax !== undefined) {
      if (patient.Age === null || patient.Age === undefined) {
        return {
          isEligible: false,
          message: `این خدمت نیاز به تاریخ تولد دارد. حداکثر سن: ${service.AgeMax} سال`,
          reason: 'AGE_REQUIRED'
        };
      }
      
      if (patient.Age > service.AgeMax) {
        return {
          isEligible: false,
          message: `حداکثر سن برای "${service.Title}" ${service.AgeMax} سال است. سن بیمار: ${patient.Age} سال`,
          reason: 'AGE_TOO_HIGH'
        };
      }
    }
    
    // بررسی محدودیت جنسیتی
    if (service.GenderLimit) {
      const serviceGender = normalizeGender(service.GenderLimit);
      const patientGender = normalizeGender(patient.Gender);
      
      if (!patientGender) {
        return {
          isEligible: false,
          message: `این خدمت نیاز به مشخص بودن جنسیت دارد`,
          reason: 'GENDER_REQUIRED'
        };
      }
      
      if (serviceGender && patientGender !== serviceGender) {
        const genderName = serviceGender === 'Male' ? 'مردان' : 'زنان';
        return {
          isEligible: false,
          message: `خدمت "${service.Title}" فقط برای ${genderName} قابل ارائه است`,
          reason: 'GENDER_MISMATCH'
        };
      }
    }
    
    // همه چیز OK
    return {
      isEligible: true,
      message: '',
      reason: ''
    };
  }

  /**
   * نرمال‌سازی جنسیت
   */
  function normalizeGender(gender) {
    if (!gender) return null;
    
    const g = String(gender).toLowerCase().trim();
    
    if (g === 'male' || g === 'مرد' || g === 'm' || g === '1') {
      return 'Male';
    }
    if (g === 'female' || g === 'زن' || g === 'f' || g === '2') {
      return 'Female';
    }
    
    return null;
  }

  // =====================================================
  // 🎨 UI FEEDBACK
  // =====================================================

  /**
   * نمایش خطای عدم صلاحیت
   */
  function showEligibilityError(result) {
    toastr.warning(
      result.message,
      'محدودیت خدمت',
      {
        timeOut: 8000,
        closeButton: true,
        progressBar: true,
        positionClass: 'toast-top-center'
      }
    );
  }

  /**
   * بررسی خودکار قبل از اضافه کردن خدمت
   * این تابع باید قبل از فراخوانی API برای AddItem فراخوانی شود
   */
  function checkBeforeAddService(serviceId) {
    console.log('🔍 Checking eligibility before adding service:', serviceId);
    
    // دریافت اطلاعات بیمار
    const patientAge = parseInt($('#Patient_Age').val()) || null;
    const patientGender = $('#Patient_Gender').val() || null;
    const patientBirthDate = $('#Patient_BirthDateShamsi').val() || null;
    
    // اگر سن محاسبه نشده، از تاریخ تولد محاسبه کن
    let age = patientAge;
    if (!age && patientBirthDate) {
      age = calculateAge(patientBirthDate);
    }
    
    const patient = {
      Age: age,
      Gender: patientGender,
      BirthDateShamsi: patientBirthDate
    };
    
    // دریافت اطلاعات خدمت از سرور
    // این باید از Cache یا API فراخوانی شود
    return $.ajax({
      url: '/api/v1/reception/service/eligibility',
      method: 'POST',
      data: JSON.stringify({
        ServiceId: serviceId,
        PatientAge: age,
        PatientGender: patientGender
      }),
      contentType: 'application/json',
      headers: {
        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
      }
    }).then(function(response) {
      if (response && response.Success) {
        const result = response.Data;
        
        if (!result.IsEligible) {
          showEligibilityError({
            message: result.Message || 'بیمار واجد شرایط دریافت این خدمت نیست'
          });
          return false;
        }
        
        return true;
      } else {
        // اگر API خطا داد، اجازه اضافه کردن بده (Fail-safe)
        console.warn('⚠️ Eligibility check failed, allowing service add');
        return true;
      }
    }).catch(function(err) {
      console.error('خطا در بررسی صلاحیت:', err);
      // Fail-safe: اجازه اضافه کردن
      return true;
    });
  }

  // =====================================================
  // 🚀 PUBLIC API
  // =====================================================

  window.ServiceEligibilityValidator = {
    calculateAge: calculateAge,
    validateServiceEligibility: validateServiceEligibility,
    checkBeforeAddService: checkBeforeAddService,
    showEligibilityError: showEligibilityError,
    normalizeGender: normalizeGender,
    version: '1.0.0'
  };

  console.log('🏥 V2: ServiceEligibilityValidator loaded - Version:', window.ServiceEligibilityValidator.version);

})(window, jQuery);

