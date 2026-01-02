using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Areas.Patient.Controllers.Base;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using Microsoft.AspNet.Identity;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers.Api
{
    /// <summary>
    /// API Controller برای پروفایل بیمار
    /// Single Responsibility: ارائه API endpoints برای مدیریت پروفایل
    /// 
    /// ✅ Enterprise-Grade: ServiceResult Enhanced, Authorization, AJAX-First
    /// </summary>
    [Authorize]
    public class ProfileApiController : BasePatientController
    {
        private readonly IPatientService _patientService;

        public ProfileApiController(
            IPatientService patientService,
            ILogger logger,
            ICurrentUserService currentUserService)
            : base(logger, currentUserService)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
        }

        /// <summary>
        /// دریافت اطلاعات پروفایل بیمار
        /// GET: /Patient/Api/Profile/GetProfile
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetProfile()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var patientId = await GetCurrentPatientIdAsync();
                
                if (patientId == null)
                {
                    _logger.Warning("❌ GetProfile: Patient not found - UserId: {UserId}", userId);
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }

                _logger.Information("📄 GetProfile: Loading profile - PatientId: {PatientId}", patientId.Value);

                // Get patient details from service
                var result = await _patientService.GetPatientDetailsAsync(patientId.Value);
                
                if (!result.Success || result.Data == null)
                {
                    _logger.Warning("❌ GetProfile: Patient details not found - PatientId: {PatientId}, Message: {Message}", 
                        patientId.Value, result.Message);
                    return ErrorJsonResult(result.Message ?? "اطلاعات بیمار یافت نشد");
                }

                var patient = result.Data;

                // Map to DTO
                var profileDto = new
                {
                    PatientId = patient.PatientId,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    NationalCode = patient.NationalCode,
                    PhoneNumber = patient.PhoneNumber,
                    Email = patient.Email,
                    BirthDate = patient.BirthDate?.ToString("yyyy/MM/dd"),
                    Gender = patient.Gender,
                    Address = patient.Address
                };

                return SuccessJsonResult(profileDto, "پروفایل با موفقیت بارگذاری شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Exception in GetProfile");
                return ErrorJsonResult("خطا در بارگذاری پروفایل");
            }
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات پروفایل بیمار
        /// POST: /Patient/Api/Profile/UpdateProfile
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateProfile(string firstName, string lastName, string phoneNumber, 
            string email, string birthDate, string gender, string address)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var patientId = await GetCurrentPatientIdAsync();
                
                if (patientId == null)
                {
                    _logger.Warning("❌ UpdateProfile: Patient not found - UserId: {UserId}", userId);
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }

                _logger.Information("📝 UpdateProfile: Updating profile - PatientId: {PatientId}", patientId.Value);

                // Validate
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    return ErrorJsonResult("نام و نام خانوادگی الزامی است");
                }

                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    return ErrorJsonResult("شماره تماس الزامی است");
                }

                // Get patient for edit
                var getResult = await _patientService.GetPatientForEditAsync(patientId.Value);
                
                if (!getResult.Success || getResult.Data == null)
                {
                    _logger.Warning("❌ UpdateProfile: Patient not found - PatientId: {PatientId}", patientId.Value);
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }

                var model = getResult.Data;

                // Update properties
                model.FirstName = firstName.Trim();
                model.LastName = lastName.Trim();
                model.PhoneNumber = phoneNumber.Trim();
                model.Email = !string.IsNullOrWhiteSpace(email) ? email.Trim() : null;
                
                // Parse Gender enum
                if (!string.IsNullOrWhiteSpace(gender))
                {
                    if (Enum.TryParse<ClinicApp.Models.Enums.Gender>(gender, true, out var genderEnum))
                    {
                        model.Gender = genderEnum;
                    }
                }
                
                model.Address = !string.IsNullOrWhiteSpace(address) ? address.Trim() : null;

                // Parse birth date
                if (!string.IsNullOrWhiteSpace(birthDate))
                {
                    if (DateTime.TryParse(birthDate, out DateTime parsedDate))
                    {
                        model.BirthDate = parsedDate;
                    }
                }

                // Update via service
                var updateResult = await _patientService.UpdatePatientAsync(model);
                
                if (!updateResult.Success)
                {
                    _logger.Warning("⚠️ UpdateProfile: Update failed - PatientId: {PatientId}, Message: {Message}", 
                        patientId.Value, updateResult.Message);
                    return ErrorJsonResult(updateResult.Message ?? "خطا در به‌روزرسانی پروفایل");
                }

                _logger.Information("✅ UpdateProfile: Profile updated successfully - PatientId: {PatientId}", patientId.Value);

                return Json(new
                {
                    success = true,
                    message = "پروفایل با موفقیت به‌روزرسانی شد",
                    reload = false
                }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Exception in UpdateProfile");
                return ErrorJsonResult("خطا در به‌روزرسانی پروفایل");
            }
        }
    }
}


