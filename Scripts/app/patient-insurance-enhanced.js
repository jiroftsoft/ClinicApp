/**
 * 🏥 Medical Environment - Enhanced Patient Insurance Management
 * یکپارچه‌سازی با سرویس‌های جدید اعتبارسنجی بیمه
 */

var PatientInsuranceEnhanced = (function() {
    'use strict';

    // 🏥 Medical Environment Configuration
    var config = {
        selectors: {
            patientId: '#PatientIdSelect',
            primaryInsuranceProviderId: '#InsuranceProviderId',
            primaryInsurancePlanId: '#InsurancePlanId',
            supplementaryInsuranceProviderId: '#SupplementaryInsuranceProviderId',
            supplementaryInsurancePlanId: '#SupplementaryInsurancePlanId',
            policyNumber: '#PolicyNumber',
            supplementaryPolicyNumber: '#SupplementaryPolicyNumber',
            startDate: '#StartDateShamsi',
            endDate: '#EndDateShamsi',
            isPrimary: '#IsPrimary',
            isActive: '#IsActive',
            selectedPatientInfo: '#selectedPatientInfo',
            validateButton: '#validatePatientInsuranceBtn',
            createDefaultButton: '#createDefaultFreeInsuranceBtn'
        },
        urls: {
            searchPatients: '/Admin/Insurance/PatientInsurance/SearchPatients',
            getPatientInsuranceStatus: '/Admin/Insurance/PatientInsurance/GetPatientInsuranceStatus',
            validatePatientInsurance: '/Admin/Insurance/PatientInsurance/ValidatePatientInsurance',
            createDefaultFreeInsurance: '/Admin/Insurance/PatientInsurance/CreateDefaultFreeInsurance',
            getPrimaryInsuranceProviders: '/Admin/Insurance/PatientInsurance/GetPrimaryInsuranceProviders',
            getSupplementaryInsuranceProviders: '/Admin/Insurance/PatientInsurance/GetSupplementaryInsuranceProviders',
            getInsurancePlansByProvider: '/Admin/Insurance/PatientInsurance/GetInsurancePlansByProvider'
        }
    };

    // 🏥 Medical Environment: Initialize Enhanced Patient Insurance
    function initialize() {
        console.log('🏥 Medical Environment: Initializing Enhanced Patient Insurance');
        
        initializePatientSelection();
        initializeInsuranceSelection();
        initializeFormValidation();
        initializeActionButtons();
        initializeRealTimeValidation();
    }

    // 🏥 Medical Environment: Advanced Patient Selection
    function initializePatientSelection() {
        console.log('🏥 Medical Environment: Initializing Patient Selection');
        console.log('🏥 Medical Environment: PatientIdSelect element exists:', $(config.selectors.patientId).length > 0);
        console.log('🏥 Medical Environment: PatientIdSelect element:', $(config.selectors.patientId)[0]);
        console.log('🏥 Medical Environment: PatientId hidden element exists:', $('#PatientId').length > 0);
        console.log('🏥 Medical Environment: PatientId hidden element:', $('#PatientId')[0]);
        
        $(config.selectors.patientId).select2({
            placeholder: 'جستجو در بیماران: نام، نام خانوادگی، کد ملی یا شماره تلفن...',
            allowClear: true,
            minimumInputLength: 2,
            language: {
                inputTooShort: function() {
                    return 'حداقل ۲ کاراکتر وارد کنید';
                },
                noResults: function() {
                    return 'هیچ بیمار یافت نشد';
                },
                searching: function() {
                    return 'در حال جستجو...';
                }
            },
            ajax: {
                url: config.urls.searchPatients,
                dataType: 'json',
                delay: 300,
                data: function(params) {
                    return {
                        q: params.term,
                        page: params.page || 1,
                        pageSize: 20
                    };
                },
                processResults: function(data, params) {
                    params.page = params.page || 1;
                    
                    var patients = [];
                    if (data.results && Array.isArray(data.results)) {
                        patients = data.results;
                    } else if (Array.isArray(data)) {
                        patients = data;
                    }
                    
                    return {
                        results: patients.map(function(patient) {
                            var displayName = extractNameFromText(patient.text);
                            return {
                                id: patient.id,
                                text: patient.text || (displayName + ' (' + patient.nationalCode + ')'),
                                patient: {
                                    id: patient.id,
                                    name: displayName,
                                    text: patient.text,
                                    nationalCode: patient.nationalCode,
                                    phone: patient.phoneNumber,
                                    phoneNumber: patient.phoneNumber,
                                    firstName: patient.firstName,
                                    lastName: patient.lastName,
                                    fatherName: patient.fatherName || 'نامشخص',
                                    birthDate: patient.birthDate,
                                    birthDateShamsi: patient.birthDateShamsi,
                                    gender: patient.gender,
                                    address: patient.address
                                }
                            };
                        })
                    };
                },
                cache: true
            },
            templateResult: function(patient) {
                if (patient.loading) {
                    return patient.text;
                }
                
                var $container = $('<div class="patient-option">');
                $container.append('<div class="patient-name">' + patient.text + '</div>');
                if (patient.patient && patient.patient.nationalCode) {
                    $container.append('<div class="patient-details">کد ملی: ' + patient.patient.nationalCode + '</div>');
                }
                return $container;
            },
            templateSelection: function(patient) {
                return patient.text || patient.patient?.name || 'انتخاب بیمار';
            }
        });

        // 🏥 Medical Environment: Patient Selection Events
        $(config.selectors.patientId).on('select2:select', function(e) {
            var data = e.params.data;
            var patient = data.patient;
            if (patient) {
                // Update hidden field for form submission
                $('#PatientId').val(patient.id);
                console.log('🏥 Medical Environment: Updated PatientId hidden field:', $('#PatientId').val());
                
                // Force sync all hidden fields
                syncAllHiddenFields();
                
                showSelectedPatientInfo(patient);
                
                // Auto-fill policy numbers with national code
                console.log('🏥 Medical Environment: Auto-filling policy numbers with national code:', patient.nationalCode);
                console.log('🏥 Medical Environment: PolicyNumber element exists:', $(config.selectors.policyNumber).length > 0);
                console.log('🏥 Medical Environment: PolicyNumber element:', $(config.selectors.policyNumber)[0]);
                console.log('🏥 Medical Environment: SupplementaryPolicyNumber element exists:', $(config.selectors.supplementaryPolicyNumber).length > 0);
                console.log('🏥 Medical Environment: SupplementaryPolicyNumber element:', $(config.selectors.supplementaryPolicyNumber)[0]);
                
                $(config.selectors.policyNumber).val(patient.nationalCode);
                $(config.selectors.supplementaryPolicyNumber).val(patient.nationalCode);
                
                // Verify policy numbers were set
                console.log('🏥 Medical Environment: PolicyNumber value after setting:', $(config.selectors.policyNumber).val());
                console.log('🏥 Medical Environment: SupplementaryPolicyNumber value after setting:', $(config.selectors.supplementaryPolicyNumber).val());
                
                // Force UI update
                $(config.selectors.policyNumber).trigger('input');
                $(config.selectors.supplementaryPolicyNumber).trigger('input');
                
                // Visual feedback
                $(config.selectors.policyNumber).addClass('bg-success text-white').delay(2000).queue(function() {
                    $(this).removeClass('bg-success text-white').dequeue();
                });
                $(config.selectors.supplementaryPolicyNumber).addClass('bg-success text-white').delay(2000).queue(function() {
                    $(this).removeClass('bg-success text-white').dequeue();
                });
                
                checkPatientInsuranceStatus();
            }
        });

        $(config.selectors.patientId).on('select2:clear', function(e) {
            // Reset hidden field
            $('#PatientId').val('0');
            console.log('🏥 Medical Environment: Reset PatientId hidden field');
            
            $(config.selectors.selectedPatientInfo).hide();
            clearPolicyNumbers();
        });
    }

    // 🏥 Medical Environment: Insurance Selection
    function initializeInsuranceSelection() {
        console.log('🏥 Medical Environment: Initializing Insurance Selection...');
        
        // Guard against double initialization
        if ($(config.selectors.primaryInsuranceProviderId).hasClass('select2-hidden-accessible')) {
            console.log('🏥 Medical Environment: Select2 already initialized, skipping...');
            return;
        }
        
        // 🚨 FIX: بارگذاری اولیه بیمه‌گذاران فقط در مرحله ۲
        // setTimeout(function() {
        //     loadPrimaryInsuranceProviders();
        // }, 100);
        
        $('.insurance-tab').on('click', function() {
            var type = $(this).data('type');
            console.log('🏥 Medical Environment: Switching to tab:', type);
            
            // حفظ انتخاب‌های قبلی
            var currentPrimaryProvider = $(config.selectors.primaryInsuranceProviderId).val();
            var currentPrimaryPlan = $(config.selectors.primaryInsurancePlanId).val();
            var currentSupplementaryProvider = $(config.selectors.supplementaryInsuranceProviderId).val();
            var currentSupplementaryPlan = $(config.selectors.supplementaryInsurancePlanId).val();
            
            console.log('🏥 Medical Environment: Preserving selections - Primary Provider:', currentPrimaryProvider, 'Primary Plan:', currentPrimaryPlan);
            console.log('🏥 Medical Environment: Preserving selections - Supplementary Provider:', currentSupplementaryProvider, 'Supplementary Plan:', currentSupplementaryPlan);
            
            $('.insurance-tab').removeClass('active');
            $(this).addClass('active');
            $('.insurance-content').removeClass('active');
            $('#' + type + 'InsuranceContent').addClass('active');
            
        // 🚨 FIX: تنظیم نوع بیمه و بارگذاری بیمه‌گذاران فقط در مرحله ۲
        if (type === 'primary') {
            $(config.selectors.isPrimary).prop('checked', true);
            console.log('🏥 Medical Environment: Primary insurance tab selected - loading providers...');
            // همیشه بارگذاری کن برای مرحله ۲
            loadPrimaryInsuranceProviders().then(function() {
                // بازگردانی انتخاب‌های قبلی
                if (currentPrimaryProvider && currentPrimaryProvider !== '0') {
                    console.log('🏥 Medical Environment: Restoring primary provider selection:', currentPrimaryProvider);
                    $(config.selectors.primaryInsuranceProviderId).val(currentPrimaryProvider).trigger('change');
                } else {
                    console.log('🏥 Medical Environment: No previous primary provider selection, keeping default');
                }
            });
        } else {
            $(config.selectors.isPrimary).prop('checked', false);
            console.log('🏥 Medical Environment: Supplementary insurance tab selected - loading providers...');
            // همیشه بارگذاری کن برای مرحله ۲
            loadSupplementaryInsuranceProviders().then(function() {
                // بازگردانی انتخاب‌های قبلی
                if (currentSupplementaryProvider) {
                    console.log('🏥 Medical Environment: Restoring supplementary provider selection:', currentSupplementaryProvider);
                    $(config.selectors.supplementaryInsuranceProviderId).val(currentSupplementaryProvider).trigger('change');
                }
            });
        }
        });

        // 🏥 Medical Environment: Primary Insurance Provider Selection
        $(config.selectors.primaryInsuranceProviderId).on('change', function() {
            var providerId = $(this).val();
            var $planSelect = $(config.selectors.primaryInsurancePlanId);
            
            console.log('🏥 Medical Environment: Primary provider changed:', providerId);
            
            // Force sync hidden fields
            syncAllHiddenFields();
            
            if (providerId && providerId !== '0') {
                loadInsurancePlans(providerId, $planSelect, 'primary');
            } else {
                $planSelect.empty().append('<option value="">ابتدا بیمه‌گذار را انتخاب کنید</option>').prop('disabled', true);
            }
        });

        // 🏥 Medical Environment: Supplementary Insurance Provider Selection
        $(config.selectors.supplementaryInsuranceProviderId).on('change', function() {
            var providerId = $(this).val();
            var $planSelect = $(config.selectors.supplementaryInsurancePlanId);
            
            if (providerId) {
                loadInsurancePlans(providerId, $planSelect, 'supplementary');
            } else {
                $planSelect.empty().append('<option value="">ابتدا بیمه‌گذار تکمیلی را انتخاب کنید</option>').prop('disabled', true);
            }
        });
    }

    // 🏥 Medical Environment: Sync All Hidden Fields
    function syncAllHiddenFields() {
        console.log('🏥 Medical Environment: === SYNCING ALL HIDDEN FIELDS ===');
        
        // Sync PatientId
        var patientId = $('#PatientIdSelect').val();
        if (patientId) {
            $('#PatientId').val(patientId);
            console.log('🏥 Medical Environment: ✅ PatientId synced:', patientId);
        } else {
            // Force sync from Select2
            var selectedData = $('#PatientIdSelect').select2('data');
            if (selectedData && selectedData.length > 0 && selectedData[0].patient) {
                $('#PatientId').val(selectedData[0].patient.id);
                console.log('🏥 Medical Environment: ✅ PatientId synced from Select2:', selectedData[0].patient.id);
            }
        }
        
        // Sync InsuranceProviderId
        var insuranceProviderId = $(config.selectors.primaryInsuranceProviderId).val();
        if (insuranceProviderId) {
            $('#InsuranceProviderId').val(insuranceProviderId);
            console.log('🏥 Medical Environment: ✅ InsuranceProviderId synced:', insuranceProviderId);
        }
        
        // Sync InsurancePlanId
        var insurancePlanId = $(config.selectors.primaryInsurancePlanId).val();
        if (insurancePlanId) {
            $('#InsurancePlanId').val(insurancePlanId);
            console.log('🏥 Medical Environment: ✅ InsurancePlanId synced:', insurancePlanId);
        }
        
        // Sync SupplementaryInsuranceProviderId
        var supplementaryProviderId = $(config.selectors.supplementaryInsuranceProviderId).val();
        if (supplementaryProviderId) {
            $('#SupplementaryInsuranceProviderId').val(supplementaryProviderId);
            console.log('🏥 Medical Environment: ✅ SupplementaryInsuranceProviderId synced:', supplementaryProviderId);
        }
        
        // Sync SupplementaryInsurancePlanId
        var supplementaryPlanId = $(config.selectors.supplementaryInsurancePlanId).val();
        if (supplementaryPlanId) {
            $('#SupplementaryInsurancePlanId').val(supplementaryPlanId);
            console.log('🏥 Medical Environment: ✅ SupplementaryInsurancePlanId synced:', supplementaryPlanId);
        }
        
        console.log('🏥 Medical Environment: === FINAL HIDDEN FIELD VALUES ===');
        console.log('🏥 Medical Environment: PatientId:', $('#PatientId').val());
        console.log('🏥 Medical Environment: InsuranceProviderId:', $('#InsuranceProviderId').val());
        console.log('🏥 Medical Environment: InsurancePlanId:', $('#InsurancePlanId').val());
        console.log('🏥 Medical Environment: SupplementaryInsuranceProviderId:', $('#SupplementaryInsuranceProviderId').val());
        console.log('🏥 Medical Environment: SupplementaryInsurancePlanId:', $('#SupplementaryInsurancePlanId').val());
    }

    // 🏥 Medical Environment: Load Primary Insurance Providers
    function loadPrimaryInsuranceProviders() {
        console.log('🏥 Medical Environment: Loading primary insurance providers...');
        
        return $.ajax({
            url: config.urls.getPrimaryInsuranceProviders,
            type: 'GET',
            success: function(response) {
                console.log('🏥 Medical Environment: AJAX Response:', response);
                console.log('🏥 Medical Environment: Response type:', typeof response);
                
                // اگر response string است، آن را parse کنیم
                var parsedResponse = response;
                if (typeof response === 'string') {
                    try {
                        parsedResponse = JSON.parse(response);
                        console.log('🏥 Medical Environment: Parsed response:', parsedResponse);
                    } catch (e) {
                        console.error('🏥 Medical Environment: JSON parse error:', e);
                        showErrorMessage('خطا در پردازش پاسخ سرور');
                        return;
                    }
                }
                
                console.log('🏥 Medical Environment: Response keys:', Object.keys(parsedResponse));
                console.log('🏥 Medical Environment: Success property:', parsedResponse.Success);
                console.log('🏥 Medical Environment: Data property:', parsedResponse.Data);
                
                // بررسی ساختار پاسخ - ممکن است Success یا success باشد
                var isSuccess = parsedResponse.Success || parsedResponse.success;
                var data = parsedResponse.Data || parsedResponse.data;
                
                console.log('🏥 Medical Environment: Success:', isSuccess);
                console.log('🏥 Medical Environment: Data:', data);
                
                if (isSuccess) {
                    var $providerSelect = $(config.selectors.primaryInsuranceProviderId);
                    console.log('🏥 Medical Environment: Provider select element:', $providerSelect);
                    console.log('🏥 Medical Environment: Provider data:', data);
                    
                    // Destroy existing Select2 if present
                    if ($providerSelect.hasClass('select2-hidden-accessible')) {
                        $providerSelect.select2('destroy');
                    }
                    
                    $providerSelect.empty();
                    $providerSelect.append('<option value="">انتخاب بیمه‌گذار پایه</option>');
                    
                    if (data && Array.isArray(data)) {
                        data.forEach(function(provider) {
                            console.log('🏥 Medical Environment: Adding provider:', provider);
                            $providerSelect.append('<option value="' + provider.id + '">' + provider.name + '</option>');
                        });
                    }
                    
                    // Initialize Select2 with proper configuration
                    $providerSelect.select2({
                        placeholder: 'انتخاب بیمه‌گذار پایه',
                        allowClear: true,
                        language: 'fa',
                        dir: 'rtl'
                    });
                    
                    // Enable control if disabled
                    $providerSelect.prop('disabled', false);
                    
                    console.log('🏥 Medical Environment: Primary insurance providers loaded. Count:', data ? data.length : 0);
                } else {
                    console.error('🏥 Medical Environment: Failed to load providers:', response.Message || response.message);
                    showErrorMessage('خطا در دریافت بیمه‌گذاران پایه: ' + (response.Message || response.message || 'نامشخص'));
                }
            },
            error: function(xhr, status, error) {
                console.error('🏥 Medical Environment: AJAX Error:', {xhr: xhr, status: status, error: error});
                showErrorMessage('خطا در دریافت بیمه‌گذاران پایه');
            }
        });
    }

    // 🏥 Medical Environment: Load Supplementary Insurance Providers
    function loadSupplementaryInsuranceProviders() {
        console.log('🏥 Medical Environment: Loading supplementary insurance providers...');
        
        return $.ajax({
            url: config.urls.getSupplementaryInsuranceProviders,
            type: 'GET',
            success: function(response) {
                console.log('🏥 Medical Environment: Supplementary AJAX Response:', response);
                console.log('🏥 Medical Environment: Supplementary Response type:', typeof response);
                
                // اگر response string است، آن را parse کنیم
                var parsedResponse = response;
                if (typeof response === 'string') {
                    try {
                        parsedResponse = JSON.parse(response);
                        console.log('🏥 Medical Environment: Parsed supplementary response:', parsedResponse);
                    } catch (e) {
                        console.error('🏥 Medical Environment: Supplementary JSON parse error:', e);
                        showErrorMessage('خطا در پردازش پاسخ سرور');
                        return;
                    }
                }
                
                // بررسی ساختار پاسخ - ممکن است Success یا success باشد
                var isSuccess = parsedResponse.Success || parsedResponse.success;
                var data = parsedResponse.Data || parsedResponse.data;
                
                console.log('🏥 Medical Environment: Supplementary Success:', isSuccess);
                console.log('🏥 Medical Environment: Supplementary Data:', data);
                
                if (isSuccess) {
                    var $providerSelect = $(config.selectors.supplementaryInsuranceProviderId);
                    console.log('🏥 Medical Environment: Supplementary provider select element:', $providerSelect);
                    
                    $providerSelect.empty();
                    $providerSelect.append('<option value="">انتخاب بیمه‌گذار تکمیلی (اختیاری)</option>');
                    
                    if (data && Array.isArray(data)) {
                        data.forEach(function(provider) {
                            console.log('🏥 Medical Environment: Adding supplementary provider:', provider);
                            $providerSelect.append('<option value="' + provider.id + '">' + provider.name + '</option>');
                        });
                    }
                    
                    // Initialize Select2 for supplementary providers
                    $providerSelect.select2({
                        placeholder: 'انتخاب بیمه‌گذار تکمیلی',
                        allowClear: true,
                        language: 'fa',
                        dir: 'rtl'
                    });
                    
                    // Enable control if disabled
                    $providerSelect.prop('disabled', false);
                    
                    console.log('🏥 Medical Environment: Supplementary insurance providers loaded. Count:', data ? data.length : 0);
                } else {
                    console.error('🏥 Medical Environment: Failed to load supplementary providers:', response.Message || response.message);
                    showErrorMessage('خطا در دریافت بیمه‌گذاران تکمیلی: ' + (response.Message || response.message || 'نامشخص'));
                }
            },
            error: function(xhr, status, error) {
                console.error('🏥 Medical Environment: Supplementary AJAX Error:', {xhr: xhr, status: status, error: error});
                showErrorMessage('خطا در دریافت بیمه‌گذاران تکمیلی');
            }
        });
    }

    // 🏥 Medical Environment: Load Insurance Plans
    function loadInsurancePlans(providerId, $planSelect, type) {
        console.log('🏥 Medical Environment: Loading insurance plans for provider:', providerId, 'type:', type);
        
        // 🚨 FIX: بررسی providerId معتبر
        if (!providerId || providerId === '0' || providerId === 0) {
            console.log('🏥 Medical Environment: Invalid providerId, skipping plans loading');
            $planSelect.empty().append('<option value="">ابتدا بیمه‌گذار را انتخاب کنید</option>').prop('disabled', true);
            return;
        }
        
        $.ajax({
            url: config.urls.getInsurancePlansByProvider,
            type: 'GET',
            data: { providerId: providerId, type: type },
            success: function(response) {
                console.log('🏥 Medical Environment: Plans AJAX Response:', response);
                console.log('🏥 Medical Environment: Plans Response type:', typeof response);
                
                // اگر response string است، آن را parse کنیم
                var parsedResponse = response;
                if (typeof response === 'string') {
                    try {
                        parsedResponse = JSON.parse(response);
                        console.log('🏥 Medical Environment: Parsed plans response:', parsedResponse);
                    } catch (e) {
                        console.error('🏥 Medical Environment: Plans JSON parse error:', e);
                        showErrorMessage('خطا در پردازش پاسخ سرور');
                        return;
                    }
                }
                
                // بررسی ساختار پاسخ - ممکن است Success یا success باشد
                var isSuccess = parsedResponse.Success || parsedResponse.success;
                var data = parsedResponse.Data || parsedResponse.data;
                
                console.log('🏥 Medical Environment: Plans Success:', isSuccess);
                console.log('🏥 Medical Environment: Plans Data:', data);
                
                if (isSuccess) {
                    $planSelect.empty();
                    $planSelect.append('<option value="">انتخاب طرح بیمه</option>');
                    
                    if (data && Array.isArray(data)) {
                        data.forEach(function(plan) {
                            console.log('🏥 Medical Environment: Adding plan:', plan);
                            $planSelect.append('<option value="' + plan.id + '">' + plan.name + '</option>');
                        });
                    }
                    
                    $planSelect.prop('disabled', false);
                    
                    // اگر فقط یک طرح وجود دارد، خودکار انتخاب شود
                    if (data && data.length === 1) {
                        $planSelect.val(data[0].id).trigger('change');
                        console.log('🏥 Medical Environment: Auto-selected single plan:', data[0].name);
                    }
                    
                    console.log('🏥 Medical Environment: Insurance plans loaded for type:', type, 'Count:', data ? data.length : 0);
                } else {
                    console.error('🏥 Medical Environment: Failed to load plans:', response.Message || response.message);
                    showErrorMessage('خطا در دریافت طرح‌های بیمه: ' + (response.Message || response.message || 'نامشخص'));
                }
            },
            error: function(xhr, status, error) {
                console.error('🏥 Medical Environment: Plans AJAX Error:', {xhr: xhr, status: status, error: error});
                showErrorMessage('خطا در دریافت طرح‌های بیمه');
            }
        });
    }

    // 🏥 Medical Environment: Form Validation
    function initializeFormValidation() {
        // 🏥 Medical Environment: Wait for DOM to be ready
        $(document).ready(function() {
            // 🏥 Medical Environment: Check if elements exist before binding events
            if ($('.medical-form-control').length === 0) {
                console.log('🏥 Medical Environment: No medical-form-control elements found, skipping validation initialization');
                return;
            }
            
            $('.medical-form-control').on('blur', function() {
                // 🏥 Medical Environment: Safety check for event handler
                var $this = $(this);
                if ($this && $this.length > 0) {
                    validateField($this);
                } else {
                    console.error('🏥 Medical Environment: Invalid element in blur event');
                }
            });
        });
        
        function validateField($field) {
            // 🏥 Medical Environment: Null check for field
            if (!$field || $field.length === 0) {
                console.error('🏥 Medical Environment: Invalid field passed to validateField');
                return false;
            }
            
            var value = $field.val() ? $field.val().trim() : '';
            var isValid = true;
            var fieldName = $field.attr('name') || $field.attr('id') || 'unknown';
            
            // بیمه تکمیلی اختیاری است
            if (fieldName && fieldName !== 'unknown' && fieldName.includes('Supplementary') && !value) {
                $field.removeClass('is-invalid is-valid');
                return true; // اختیاری است
            }
            
            if ($field && $field.prop && $field.prop('required') && !value) {
                isValid = false;
            }
            
            if (isValid) {
                if ($field && $field.removeClass && $field.addClass) {
                    $field.removeClass('is-invalid').addClass('is-valid');
                }
            } else {
                if ($field && $field.removeClass && $field.addClass) {
                    $field.removeClass('is-valid').addClass('is-invalid');
                }
            }
            
            return isValid;
        }
    }

    // 🏥 Medical Environment: Real-time Validation
    function initializeRealTimeValidation() {
        // اعتبارسنجی شماره بیمه‌نامه
        $(config.selectors.policyNumber).on('input', function() {
            var policyNumber = $(this).val();
            if (policyNumber && !validatePolicyNumberFormat(policyNumber)) {
                showMedicalValidationMessage($(this), 'فرمت شماره بیمه‌نامه نامعتبر است');
            } else {
                hideMedicalValidationMessage($(this));
            }
        });

        // اعتبارسنجی تاریخ‌ها
        $(config.selectors.startDate).on('change', function() {
            validateDateRange();
        });

        $(config.selectors.endDate).on('change', function() {
            validateDateRange();
        });
    }

    // 🏥 Medical Environment: Action Buttons
    function initializeActionButtons() {
        // دکمه اعتبارسنجی بیمه بیمار
        $(config.selectors.validateButton).on('click', function() {
            validatePatientInsuranceWithNewService();
        });

        // دکمه ایجاد بیمه رایگان پیش‌فرض
        $(config.selectors.createDefaultButton).on('click', function() {
            var patientId = $(config.selectors.patientId).val();
            if (patientId) {
                createDefaultFreeInsurance(patientId);
            } else {
                showErrorMessage('ابتدا بیمار را انتخاب کنید');
            }
        });
    }

    // 🏥 Helper functions
    function extractNameFromText(text) {
        if (!text) return '';
        var parts = text.split(' ');
        if (parts.length >= 2) {
            return parts[0] + ' ' + parts[1];
        }
        return parts[0] || '';
    }

    function calculateAge(birthDate) {
        if (!birthDate) return '';
        var today = new Date();
        var birth = new Date(birthDate);
        var age = today.getFullYear() - birth.getFullYear();
        var monthDiff = today.getMonth() - birth.getMonth();
        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
            age--;
        }
        return age + ' سال';
    }

    function formatPersianDate(dateString) {
        if (!dateString) return '';
        // تبدیل تاریخ میلادی به شمسی
        return dateString;
    }

    function showSuccessMessage(message) {
        var alertHtml = `
            <div class="alert alert-success alert-dismissible fade show" role="alert">
                <i class="fas fa-check-circle"></i> ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>`;
        $('.medical-form-container').prepend(alertHtml);
        
        setTimeout(function() {
            $('.alert-success').fadeOut();
        }, 5000);
    }

    function showErrorMessage(message) {
        var alertHtml = `
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                <i class="fas fa-exclamation-circle"></i> ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>`;
        $('.medical-form-container').prepend(alertHtml);
        
        setTimeout(function() {
            $('.alert-danger').fadeOut();
        }, 5000);
    }

    function showInfoMessage(message) {
        var alertHtml = `
            <div class="alert alert-info alert-dismissible fade show" role="alert">
                <i class="fas fa-info-circle"></i> ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>`;
        $('.medical-form-container').prepend(alertHtml);
        
        setTimeout(function() {
            $('.alert-info').fadeOut();
        }, 5000);
    }

    function showWarningMessage(message) {
        var alertHtml = `
            <div class="alert alert-warning alert-dismissible fade show" role="alert">
                <i class="fas fa-exclamation-triangle"></i> ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>`;
        $('.medical-form-container').prepend(alertHtml);
        
        setTimeout(function() {
            $('.alert-warning').fadeOut();
        }, 5000);
    }

    function clearPolicyNumbers() {
        $(config.selectors.policyNumber).val('');
        $(config.selectors.supplementaryPolicyNumber).val('');
    }

    function showSelectedPatientInfo(patient) {
        var $info = $(config.selectors.selectedPatientInfo);
        $('#selectedPatientName').text(patient.name);
        $('#selectedPatientFullName').text(patient.firstName + ' ' + patient.lastName);
        $('#selectedPatientNationalCode').text(patient.nationalCode);
        $('#selectedPatientFatherName').text(patient.fatherName);
        $('#selectedPatientPhone').text(patient.phoneNumber);
        $('#selectedPatientBirthDate').text(patient.birthDateShamsi + ' (' + calculateAge(patient.birthDate) + ')');
        $('#selectedPatientGender').text(patient.gender === 'Male' ? 'مرد' : 'زن');
        
        if (patient.address) {
            $('#selectedPatientAddress').text(patient.address);
            $('#additionalPatientInfo').show();
        } else {
            $('#additionalPatientInfo').hide();
        }
        
        $info.show();
    }

    function validatePolicyNumberFormat(policyNumber) {
        // اعتبارسنجی فرمت شماره بیمه‌نامه
        var pattern = /^[A-Za-z0-9\-_]+$/;
        return pattern.test(policyNumber);
    }

    function validateDateRange() {
        var startDate = $(config.selectors.startDate).val();
        var endDate = $(config.selectors.endDate).val();
        
        if (startDate && endDate) {
            // اعتبارسنجی محدوده تاریخ
            var start = new Date(startDate);
            var end = new Date(endDate);
            
            if (start >= end) {
                showMedicalValidationMessage($(config.selectors.endDate), 'تاریخ پایان باید بعد از تاریخ شروع باشد');
            } else {
                hideMedicalValidationMessage($(config.selectors.endDate));
            }
        }
    }

    function validateInsurancePlan(planId) {
        if (!planId) return false;
        // اعتبارسنجی طرح بیمه
        return true;
    }

    function showMedicalValidationMessage($field, message) {
        $field.addClass('is-invalid');
        var $feedback = $field.siblings('.invalid-feedback');
        if ($feedback.length === 0) {
            $feedback = $('<div class="invalid-feedback"></div>');
            $field.after($feedback);
        }
        $feedback.text(message);
    }

    function hideMedicalValidationMessage($field) {
        $field.removeClass('is-invalid');
        $field.siblings('.invalid-feedback').remove();
    }

    function getMedicalTooltip(field) {
        var tooltips = {
            'patient-selection': 'برای جستجوی بیمار، نام، نام خانوادگی، کد ملی یا شماره تلفن را وارد کنید',
            'insurance-provider': 'ابتدا بیمه‌گذار را انتخاب کنید',
            'insurance-plan': 'طرح بیمه بر اساس بیمه‌گذار انتخاب شده نمایش داده می‌شود',
            'policy-number': 'شماره بیمه‌نامه معمولاً کد ملی بیمار است',
            'start-date': 'تاریخ شروع اعتبار بیمه',
            'end-date': 'تاریخ پایان اعتبار بیمه (اختیاری)'
        };
        return tooltips[field] || '';
    }

    // 🏥 Check Patient Insurance Status with New Validation Service
    function checkPatientInsuranceStatus() {
        var patientId = $(config.selectors.patientId).val();
        if (!patientId) return;

        $.ajax({
            url: config.urls.getPatientInsuranceStatus,
            type: 'GET',
            data: { patientId: patientId },
            success: function(response) {
                if (response.success) {
                    updateInsuranceStatusDisplay(response.data);
                } else {
                    showInsuranceStatusError(response.message || 'خطا در دریافت وضعیت بیمه');
                }
            },
            error: function(xhr, status, error) {
                console.error('خطا در دریافت وضعیت بیمه:', error);
                showInsuranceStatusError('خطا در دریافت وضعیت بیمه');
            }
        });
    }

    function validatePatientInsuranceWithNewService() {
        var patientId = $(config.selectors.patientId).val();
        if (!patientId) {
            showErrorMessage('ابتدا بیمار را انتخاب کنید');
            return;
        }

        $.ajax({
            url: config.urls.validatePatientInsurance,
            type: 'GET',
            data: { patientId: patientId },
            success: function(response) {
                if (response.success) {
                    showValidationResult(response.data);
                } else {
                    showErrorMessage(response.message || 'خطا در اعتبارسنجی بیمه');
                }
            },
            error: function(xhr, status, error) {
                console.error('خطا در اعتبارسنجی بیمه:', error);
                showErrorMessage('خطا در اعتبارسنجی بیمه');
            }
        });
    }

    function showValidationResult(validationResult) {
        var $modal = $('#validationResultModal');
        if ($modal.length === 0) {
            $modal = $('<div class="modal fade" id="validationResultModal" tabindex="-1"></div>');
            $('body').append($modal);
        }

        var modalHtml = `
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">
                            <i class="fas fa-shield-alt"></i> نتیجه اعتبارسنجی بیمه
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="validation-result">
                            <div class="row">
                                <div class="col-md-6">
                                    <div class="validation-status ${validationResult.isValid ? 'valid' : 'invalid'}">
                                        <i class="fas ${validationResult.isValid ? 'fa-check-circle' : 'fa-times-circle'}"></i>
                                        <span>${validationResult.isValid ? 'معتبر' : 'نامعتبر'}</span>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="validation-date">
                                        <i class="fas fa-calendar"></i>
                                        <span>تاریخ اعتبارسنجی: ${validationResult.validationDate}</span>
                                    </div>
                                </div>
                            </div>
                            
                            ${validationResult.issues && validationResult.issues.length > 0 ? `
                                <div class="validation-issues mt-3">
                                    <h6><i class="fas fa-exclamation-triangle"></i> مسائل شناسایی شده:</h6>
                                    <ul class="list-group">
                                        ${validationResult.issues.map(issue => `
                                            <li class="list-group-item ${issue.severity === 'Critical' ? 'list-group-item-danger' : issue.severity === 'Warning' ? 'list-group-item-warning' : 'list-group-item-info'}">
                                                <i class="fas fa-${issue.severity === 'Critical' ? 'times-circle' : issue.severity === 'Warning' ? 'exclamation-triangle' : 'info-circle'}"></i>
                                                <strong>${issue.type}:</strong> ${issue.message}
                                                ${issue.recommendation ? `<br><small class="text-muted">توصیه: ${issue.recommendation}</small>` : ''}
                                            </li>
                                        `).join('')}
                                    </ul>
                                </div>
                            ` : ''}
                            
                            ${validationResult.recommendations && validationResult.recommendations.length > 0 ? `
                                <div class="validation-recommendations mt-3">
                                    <h6><i class="fas fa-lightbulb"></i> توصیه‌ها:</h6>
                                    <ul class="list-group">
                                        ${validationResult.recommendations.map(rec => `
                                            <li class="list-group-item list-group-item-info">
                                                <i class="fas fa-arrow-left"></i> ${rec}
                                            </li>
                                        `).join('')}
                                    </ul>
                                </div>
                            ` : ''}
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">بستن</button>
                    </div>
                </div>
            </div>
        `;

        $modal.html(modalHtml);
        $modal.modal('show');
    }

    function updateInsuranceStatusDisplay(statusData) {
        // به‌روزرسانی نمایش وضعیت بیمه
        console.log('🏥 Medical Environment: Insurance status updated', statusData);
    }

    function showInsuranceStatusError(message) {
        showErrorMessage(message);
    }

    function initializeDefaultFreeInsuranceButton() {
        $(config.selectors.createDefaultButton).on('click', function() {
            var patientId = $(config.selectors.patientId).val();
            if (patientId) {
                createDefaultFreeInsurance(patientId);
            } else {
                showErrorMessage('ابتدا بیمار را انتخاب کنید');
            }
        });
    }

    function createDefaultFreeInsurance(patientId) {
        $.ajax({
            url: config.urls.createDefaultFreeInsurance,
            type: 'POST',
            data: { patientId: patientId },
            success: function(response) {
                if (response.success) {
                    showSuccessMessage('بیمه رایگان پیش‌فرض با موفقیت ایجاد شد');
                } else {
                    showErrorMessage(response.message || 'خطا در ایجاد بیمه رایگان');
                }
            },
            error: function(xhr, status, error) {
                console.error('خطا در ایجاد بیمه رایگان:', error);
                showErrorMessage('خطا در ایجاد بیمه رایگان');
            }
        });
    }

    function initializePatientInsuranceValidationButton() {
        $(config.selectors.validateButton).on('click', function() {
            validatePatientInsuranceWithNewService();
        });
    }

    // 🚨 CRITICAL FIX: Sync hidden fields function
    function syncHiddenFields() {
        console.log('🏥 Medical Environment: === SYNCING HIDDEN FIELDS ===');
        
        // Sync PatientId
        var patientId = $(config.selectors.patientId).val();
        if (patientId && patientId !== '' && patientId !== '0') {
            $('#PatientId').val(patientId);
            console.log('🏥 Medical Environment: ✅ PatientId synced:', patientId);
        }
        
        // Sync InsuranceProviderId
        var primaryProviderId = $(config.selectors.primaryInsuranceProviderId).val();
        var supplementaryProviderId = $(config.selectors.supplementaryInsuranceProviderId).val();
        var finalProviderId = primaryProviderId || supplementaryProviderId;
        
        if (finalProviderId && finalProviderId !== '' && finalProviderId !== '0') {
            $('#InsuranceProviderId').val(finalProviderId);
            console.log('🏥 Medical Environment: ✅ InsuranceProviderId synced:', finalProviderId);
        }
        
        // Sync InsurancePlanId
        var primaryPlanId = $(config.selectors.primaryInsurancePlanId).val();
        var supplementaryPlanId = $(config.selectors.supplementaryInsurancePlanId).val();
        var finalPlanId = primaryPlanId || supplementaryPlanId;
        
        if (finalPlanId && finalPlanId !== '' && finalPlanId !== '0') {
            $('#InsurancePlanId').val(finalPlanId);
            console.log('🏥 Medical Environment: ✅ InsurancePlanId synced:', finalPlanId);
        }
        
        // Sync Supplementary fields
        if (supplementaryProviderId && supplementaryProviderId !== '' && supplementaryProviderId !== '0') {
            $('#SupplementaryInsuranceProviderId').val(supplementaryProviderId);
            console.log('🏥 Medical Environment: ✅ SupplementaryInsuranceProviderId synced:', supplementaryProviderId);
        }
        
        if (supplementaryPlanId && supplementaryPlanId !== '' && supplementaryPlanId !== '0') {
            $('#SupplementaryInsurancePlanId').val(supplementaryPlanId);
            console.log('🏥 Medical Environment: ✅ SupplementaryInsurancePlanId synced:', supplementaryPlanId);
        }
        
        // Final validation
        console.log('🏥 Medical Environment: === FINAL HIDDEN FIELD VALUES ===');
        console.log('🏥 Medical Environment: PatientId:', $('#PatientId').val());
        console.log('🏥 Medical Environment: InsuranceProviderId:', $('#InsuranceProviderId').val());
        console.log('🏥 Medical Environment: InsurancePlanId:', $('#InsurancePlanId').val());
        console.log('🏥 Medical Environment: SupplementaryInsuranceProviderId:', $('#SupplementaryInsuranceProviderId').val());
        console.log('🏥 Medical Environment: SupplementaryInsurancePlanId:', $('#SupplementaryInsurancePlanId').val());
    }

    // Public API
    return {
        initialize: initialize,
        syncHiddenFields: syncHiddenFields,
        initializeCreateForm: function() {
            initializePatientSelection();
            initializeInsuranceSelection();
            initializeFormValidation();
            initializeRealTimeValidation();
            initializeDefaultFreeInsuranceButton();
            initializePatientInsuranceValidationButton();
        },
        initializeEditForm: function() {
            initializeFormValidation();
            initializeRealTimeValidation();
            // PatientSelect2.setValue and PatientInsuranceForm.initializeFormWithData will be called from the Edit.cshtml directly
        },
        initializeInsuranceSelection: initializeInsuranceSelection,
        initializePatientSelection: initializePatientSelection,
        initializeFormValidation: initializeFormValidation,
        initializeRealTimeValidation: initializeRealTimeValidation,
        syncAllHiddenFields: syncAllHiddenFields
    };
})();

// 🏥 Medical Environment: Auto-initialize when DOM is ready
$(document).ready(function() {
    if (typeof PatientInsuranceEnhanced !== 'undefined') {
        PatientInsuranceEnhanced.initialize();
    }
});