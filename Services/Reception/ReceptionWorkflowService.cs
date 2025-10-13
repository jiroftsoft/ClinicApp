using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس مدیریت فرآیندهای پذیرش - قلب سیستم درمانی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل workflow پذیرش
    /// 2. State Machine برای وضعیت‌های پذیرش
    /// 3. مدیریت انتقال بین مراحل
    /// 4. اعتبارسنجی مراحل
    /// 5. لاگ‌گیری کامل فرآیند
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت workflow
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    public class ReceptionWorkflowService
    {
        #region Fields and Constructor

        private readonly IReceptionService _receptionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionWorkflowService(
            IReceptionService receptionService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Workflow Management

        /// <summary>
        /// شروع فرآیند پذیرش
        /// </summary>
        public async Task<ServiceResult<ReceptionWorkflowResult>> StartReceptionWorkflowAsync(ReceptionWorkflowRequest request)
        {
            try
            {
                _logger.Information("🚀 شروع فرآیند پذیرش: بیمار {PatientId}, کاربر: {UserName}", 
                    request.PatientId, _currentUserService.UserName);

                // اعتبارسنجی اولیه
                if (request.CreateViewModel == null)
                {
                    return ServiceResult<ReceptionWorkflowResult>.Failed("مدل پذیرش نامعتبر است");
                }

                // ایجاد پذیرش
                var createResult = await _receptionService.CreateReceptionAsync(request.CreateViewModel);
                if (!createResult.Success)
                {
                    return ServiceResult<ReceptionWorkflowResult>.Failed(createResult.Message);
                }

                var workflowResult = new ReceptionWorkflowResult
                {
                    ReceptionId = createResult.Data.ReceptionId,
                    CurrentStep = "PatientVerification",
                    Status = "InProgress",
                    StartedAt = DateTime.Now,
                    Steps = GetWorkflowSteps()
                };

                _logger.Information("✅ فرآیند پذیرش شروع شد: {ReceptionId}, مرحله: {CurrentStep}", 
                    workflowResult.ReceptionId, workflowResult.CurrentStep);

                return ServiceResult<ReceptionWorkflowResult>.Successful(workflowResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در شروع فرآیند پذیرش: بیمار {PatientId}", request.PatientId);
                return ServiceResult<ReceptionWorkflowResult>.Failed("خطا در شروع فرآیند پذیرش");
            }
        }

        /// <summary>
        /// پردازش مرحله پذیرش
        /// </summary>
        public async Task<ServiceResult<ReceptionWorkflowResult>> ProcessReceptionStepAsync(int receptionId, string stepName, object stepData)
        {
            try
            {
                _logger.Information("🔄 پردازش مرحله پذیرش: {ReceptionId}, مرحله: {StepName}, کاربر: {UserName}", 
                    receptionId, stepName, _currentUserService.UserName);

                // دریافت وضعیت فعلی پذیرش
                var receptionResult = await _receptionService.GetReceptionByIdAsync(receptionId);
                if (!receptionResult.Success)
                {
                    return ServiceResult<ReceptionWorkflowResult>.Failed("پذیرش یافت نشد");
                }

                // پردازش مرحله بر اساس نام
                var stepResult = await ProcessStepByNameAsync(receptionId, stepName, stepData);
                if (!stepResult.Success)
                {
                    return ServiceResult<ReceptionWorkflowResult>.Failed(stepResult.Message);
                }

                var workflowResult = new ReceptionWorkflowResult
                {
                    ReceptionId = receptionId,
                    CurrentStep = GetNextStep(stepName),
                    Status = GetWorkflowStatus(receptionId),
                    ProcessedAt = DateTime.Now,
                    Steps = GetWorkflowSteps()
                };

                _logger.Information("✅ مرحله پردازش شد: {ReceptionId}, مرحله بعدی: {NextStep}", 
                    receptionId, workflowResult.CurrentStep);

                return ServiceResult<ReceptionWorkflowResult>.Successful(workflowResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در پردازش مرحله: {ReceptionId}, مرحله: {StepName}", receptionId, stepName);
                return ServiceResult<ReceptionWorkflowResult>.Failed("خطا در پردازش مرحله");
            }
        }

        /// <summary>
        /// تکمیل فرآیند پذیرش
        /// </summary>
        public async Task<ServiceResult<ReceptionWorkflowResult>> CompleteReceptionWorkflowAsync(int receptionId)
        {
            try
            {
                _logger.Information("🏁 تکمیل فرآیند پذیرش: {ReceptionId}, کاربر: {UserName}", 
                    receptionId, _currentUserService.UserName);

                // به‌روزرسانی وضعیت پذیرش
                var updateResult = await _receptionService.UpdateReceptionAsync(new ReceptionEditViewModel 
                { 
                    ReceptionId = receptionId,
                    Status = ReceptionStatus.Completed
                });
                if (!updateResult.Success)
                {
                    return ServiceResult<ReceptionWorkflowResult>.Failed(updateResult.Message);
                }

                var workflowResult = new ReceptionWorkflowResult
                {
                    ReceptionId = receptionId,
                    CurrentStep = "Completed",
                    Status = "Completed",
                    CompletedAt = DateTime.Now,
                    Steps = GetWorkflowSteps()
                };

                _logger.Information("🎉 فرآیند پذیرش تکمیل شد: {ReceptionId}", receptionId);

                return ServiceResult<ReceptionWorkflowResult>.Successful(workflowResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تکمیل فرآیند پذیرش: {ReceptionId}", receptionId);
                return ServiceResult<ReceptionWorkflowResult>.Failed("خطا در تکمیل فرآیند پذیرش");
            }
        }

        /// <summary>
        /// لغو فرآیند پذیرش
        /// </summary>
        public async Task<ServiceResult<ReceptionWorkflowResult>> CancelReceptionWorkflowAsync(int receptionId, string reason)
        {
            try
            {
                _logger.Information("🚫 لغو فرآیند پذیرش: {ReceptionId}, دلیل: {Reason}, کاربر: {UserName}", 
                    receptionId, reason, _currentUserService.UserName);

                // به‌روزرسانی وضعیت پذیرش
                var updateResult = await _receptionService.UpdateReceptionAsync(new ReceptionEditViewModel 
                { 
                    ReceptionId = receptionId,
                    Status = ReceptionStatus.Cancelled
                });
                if (!updateResult.Success)
                {
                    return ServiceResult<ReceptionWorkflowResult>.Failed(updateResult.Message);
                }

                var workflowResult = new ReceptionWorkflowResult
                {
                    ReceptionId = receptionId,
                    CurrentStep = "Cancelled",
                    Status = "Cancelled",
                    CancelledAt = DateTime.Now,
                    CancellationReason = reason,
                    Steps = GetWorkflowSteps()
                };

                _logger.Information("❌ فرآیند پذیرش لغو شد: {ReceptionId}, دلیل: {Reason}", receptionId, reason);

                return ServiceResult<ReceptionWorkflowResult>.Successful(workflowResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در لغو فرآیند پذیرش: {ReceptionId}", receptionId);
                return ServiceResult<ReceptionWorkflowResult>.Failed("خطا در لغو فرآیند پذیرش");
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<ServiceResult<object>> ProcessStepByNameAsync(int receptionId, string stepName, object stepData)
        {
            return stepName.ToLower() switch
            {
                "patientverification" => await ProcessPatientVerificationStepAsync(receptionId, stepData),
                "insurancevalidation" => await ProcessInsuranceValidationStepAsync(receptionId, stepData),
                "serviceSelection" => await ProcessServiceSelectionStepAsync(receptionId, stepData),
                "doctorAssignment" => await ProcessDoctorAssignmentStepAsync(receptionId, stepData),
                "paymentProcessing" => await ProcessPaymentProcessingStepAsync(receptionId, stepData),
                _ => ServiceResult<object>.Failed("مرحله نامعتبر")
            };
        }

        private async Task<ServiceResult<object>> ProcessPatientVerificationStepAsync(int receptionId, object stepData)
        {
            // TODO: پیاده‌سازی اعتبارسنجی بیمار
            return ServiceResult<object>.Successful(new { Step = "PatientVerification", Status = "Completed" });
        }

        private async Task<ServiceResult<object>> ProcessInsuranceValidationStepAsync(int receptionId, object stepData)
        {
            // TODO: پیاده‌سازی اعتبارسنجی بیمه
            return ServiceResult<object>.Successful(new { Step = "InsuranceValidation", Status = "Completed" });
        }

        private async Task<ServiceResult<object>> ProcessServiceSelectionStepAsync(int receptionId, object stepData)
        {
            // TODO: پیاده‌سازی انتخاب خدمات
            return ServiceResult<object>.Successful(new { Step = "ServiceSelection", Status = "Completed" });
        }

        private async Task<ServiceResult<object>> ProcessDoctorAssignmentStepAsync(int receptionId, object stepData)
        {
            // TODO: پیاده‌سازی انتساب پزشک
            return ServiceResult<object>.Successful(new { Step = "DoctorAssignment", Status = "Completed" });
        }

        private async Task<ServiceResult<object>> ProcessPaymentProcessingStepAsync(int receptionId, object stepData)
        {
            // TODO: پیاده‌سازی پردازش پرداخت
            return ServiceResult<object>.Successful(new { Step = "PaymentProcessing", Status = "Completed" });
        }

        private string GetNextStep(string currentStep)
        {
            return currentStep.ToLower() switch
            {
                "patientverification" => "InsuranceValidation",
                "insurancevalidation" => "ServiceSelection",
                "serviceselection" => "DoctorAssignment",
                "doctorassignment" => "PaymentProcessing",
                "paymentprocessing" => "Completed",
                _ => "Unknown"
            };
        }

        private string GetWorkflowStatus(int receptionId)
        {
            // TODO: پیاده‌سازی دریافت وضعیت واقعی
            return "InProgress";
        }

        private List<WorkflowStep> GetWorkflowSteps()
        {
            return new List<WorkflowStep>
            {
                new WorkflowStep { Name = "PatientVerification", DisplayName = "اعتبارسنجی بیمار", Order = 1 },
                new WorkflowStep { Name = "InsuranceValidation", DisplayName = "اعتبارسنجی بیمه", Order = 2 },
                new WorkflowStep { Name = "ServiceSelection", DisplayName = "انتخاب خدمات", Order = 3 },
                new WorkflowStep { Name = "DoctorAssignment", DisplayName = "انتساب پزشک", Order = 4 },
                new WorkflowStep { Name = "PaymentProcessing", DisplayName = "پردازش پرداخت", Order = 5 },
                new WorkflowStep { Name = "Completed", DisplayName = "تکمیل", Order = 6 }
            };
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// درخواست شروع فرآیند پذیرش
    /// </summary>
    public class ReceptionWorkflowRequest
    {
        public int PatientId { get; set; }
        public ReceptionCreateViewModel CreateViewModel { get; set; }
        public string WorkflowType { get; set; } = "Standard";
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// نتیجه فرآیند پذیرش
    /// </summary>
    public class ReceptionWorkflowResult
    {
        public int ReceptionId { get; set; }
        public string CurrentStep { get; set; }
        public string Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; }
        public List<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
        public Dictionary<string, object> StepData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// مرحله فرآیند
    /// </summary>
    public class WorkflowStep
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int Order { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? ProcessedAt { get; set; }
    }

    #endregion
}
