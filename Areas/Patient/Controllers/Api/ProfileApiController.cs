using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Areas.Patient.Controllers.Base;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
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
            ICurrentUserService currentUserService,
            ApplicationDbContext context)
            : base(logger, currentUserService, context)
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
                // ✅ BEST PRACTICE: Convert Gender enum to string for JSON serialization
                var profileDto = new
                {
                    PatientId = patient.PatientId,
                    FirstName = patient.FirstName ?? string.Empty,
                    LastName = patient.LastName ?? string.Empty,
                    NationalCode = patient.NationalCode ?? string.Empty,
                    PhoneNumber = patient.PhoneNumber ?? string.Empty,
                    Email = patient.Email ?? string.Empty,
                    BirthDate = patient.BirthDate?.ToString("yyyy/MM/dd") ?? string.Empty,
                    Gender = patient.Gender.ToString(), // ✅ Convert enum to string
                    Address = patient.Address ?? string.Empty
                };

                _logger.Information("✅ GetProfile: Profile loaded successfully - PatientId: {PatientId}, Gender: {Gender}", 
                    patient.PatientId, profileDto.Gender);

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
        /// منطق در IPatientService.UpdatePatientProfileFromFormAsync متمرکز شده است.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateProfile(string firstName, string lastName, string phoneNumber,
            string email, string birthDate, string gender, string address)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    _logger.Warning("❌ UpdateProfile: Patient not found - UserId: {UserId}", User.Identity.GetUserId());
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }

                _logger.Information("📝 UpdateProfile: Updating profile - PatientId: {PatientId}", patientId.Value);
                var result = await _patientService.UpdatePatientProfileFromFormAsync(patientId.Value, firstName, lastName, phoneNumber, email, birthDate, gender, address);

                if (!result.Success)
                {
                    _logger.Warning("⚠️ UpdateProfile: Update failed - PatientId: {PatientId}, Message: {Message}", patientId.Value, result.Message);
                    return ErrorJsonResult(result.Message ?? "خطا در به‌روزرسانی پروفایل");
                }

                _logger.Information("✅ UpdateProfile: Profile updated successfully - PatientId: {PatientId}", patientId.Value);
                return Json(new { success = true, message = "پروفایل با موفقیت به‌روزرسانی شد", reload = false }, JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Exception in UpdateProfile");
                return ErrorJsonResult("خطا در به‌روزرسانی پروفایل");
            }
        }
    }
}


