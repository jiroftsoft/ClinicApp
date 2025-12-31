using System;
using System.Collections.Generic;
using System.Linq;
using ClinicApp.Extensions;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.ViewModels.Patient.MedicalRecord;
using ClinicApp.Helpers;

namespace ClinicApp.Factories.Patient
{
    /// <summary>
    /// Factory برای تبدیل Entity به ViewModel
    /// Single Responsibility: فقط تبدیل Entity → ViewModel
    /// ✅ Contract Compliance: "Entity → ViewModel ONLY via Factory Method"
    /// </summary>
    public static class MedicalRecordFactory
    {
        /// <summary>
        /// تبدیل MedicalHistory Entity به ViewModel
        /// ✅ Factory Method Pattern
        /// </summary>
        public static MedicalHistoryViewModel ToViewModel(MedicalHistory entity)
        {
            if (entity == null) return null;
            
            return new MedicalHistoryViewModel
            {
                MedicalHistoryId = entity.MedicalHistoryId,
                PatientId = entity.PatientId,
                Type = entity.Type,
                TypeText = GetMedicalHistoryTypeText(entity.Type),
                Title = entity.Title,
                Description = entity.Description,
                StartDate = entity.StartDate,
                StartDateShamsi = entity.StartDate?.ToPersianDate(),
                EndDate = entity.EndDate,
                EndDateShamsi = entity.EndDate?.ToPersianDate(),
                IsActive = entity.IsActive,
                Severity = entity.Severity,
                DoctorName = entity.DoctorName,
                MedicalCenter = entity.MedicalCenter,
                Attachments = entity.Attachments,
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                UpdatedAt = entity.UpdatedAt,
                UpdatedAtShamsi = entity.UpdatedAt?.ToPersianDateTime()
            };
        }
        
        /// <summary>
        /// تبدیل لیست MedicalHistory به ViewModel
        /// ✅ Factory Method Pattern
        /// </summary>
        public static List<MedicalHistoryViewModel> ToViewModelList(
            IEnumerable<MedicalHistory> entities)
        {
            return entities?.Select(ToViewModel).Where(vm => vm != null).ToList() 
                ?? new List<MedicalHistoryViewModel>();
        }
        
        /// <summary>
        /// تبدیل ViewModel به Entity (برای Create)
        /// ✅ Factory Method Pattern
        /// </summary>
        public static MedicalHistory ToEntity(MedicalHistoryCreateEditViewModel viewModel, 
            int patientId, string createdByUserId)
        {
            if (viewModel == null) return null;
            
            return new MedicalHistory
            {
                PatientId = patientId,
                Type = viewModel.Type,
                Title = viewModel.Title,
                Description = viewModel.Description,
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
                IsActive = viewModel.IsActive,
                Severity = viewModel.Severity,
                DoctorName = viewModel.DoctorName,
                MedicalCenter = viewModel.MedicalCenter,
                Attachments = viewModel.Attachments,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.Now
            };
        }
        
        /// <summary>
        /// به‌روزرسانی Entity از ViewModel (برای Update)
        /// ✅ Factory Method Pattern
        /// </summary>
        public static void UpdateEntity(MedicalHistory entity, 
            MedicalHistoryCreateEditViewModel viewModel, string updatedByUserId)
        {
            if (entity == null || viewModel == null) return;
            
            entity.Type = viewModel.Type;
            entity.Title = viewModel.Title;
            entity.Description = viewModel.Description;
            entity.StartDate = viewModel.StartDate;
            entity.EndDate = viewModel.EndDate;
            entity.IsActive = viewModel.IsActive;
            entity.Severity = viewModel.Severity;
            entity.DoctorName = viewModel.DoctorName;
            entity.MedicalCenter = viewModel.MedicalCenter;
            entity.Attachments = viewModel.Attachments;
            entity.UpdatedByUserId = updatedByUserId;
            entity.UpdatedAt = DateTime.Now;
        }
        
        /// <summary>
        /// تبدیل نوع تاریخچه پزشکی به متن فارسی
        /// </summary>
        private static string GetMedicalHistoryTypeText(Models.Enums.MedicalHistoryType type)
        {
            switch (type)
            {
                case Models.Enums.MedicalHistoryType.Disease:
                    return "بیماری";
                case Models.Enums.MedicalHistoryType.Surgery:
                    return "جراحی";
                case Models.Enums.MedicalHistoryType.Injury:
                    return "آسیب";
                case Models.Enums.MedicalHistoryType.Medication:
                    return "دارو";
                case Models.Enums.MedicalHistoryType.Allergy:
                    return "آلرژی";
                case Models.Enums.MedicalHistoryType.FamilyHistory:
                    return "سابقه خانوادگی";
                case Models.Enums.MedicalHistoryType.Other:
                    return "سایر";
                default:
                    return "نامشخص";
            }
        }
        
        #region Appointment Factory Methods
        
        /// <summary>
        /// تبدیل Appointment Entity به ViewModel
        /// ✅ Factory Method Pattern
        /// </summary>
        public static MedicalRecordAppointmentViewModel ToViewModel(
            Models.Entities.Appointment.Appointment entity, 
            string doctorName = null, 
            string doctorSpecialization = null,
            string serviceCategory = null)
        {
            if (entity == null) return null;
            
            return new MedicalRecordAppointmentViewModel
            {
                AppointmentId = entity.AppointmentId,
                DoctorId = entity.DoctorId,
                DoctorName = doctorName ?? "نامشخص",
                DoctorSpecialization = doctorSpecialization,
                AppointmentDate = entity.AppointmentDate,
                AppointmentDateShamsi = entity.AppointmentDate.ToPersianDate(),
                AppointmentTime = entity.AppointmentDate.ToString("HH:mm"),
                Status = entity.Status,
                StatusText = GetAppointmentStatusText(entity.Status),
                Price = entity.Price,
                Description = entity.Description,
                IsNewPatient = entity.IsNewPatient,
                ServiceCategory = serviceCategory,
                Duration = entity.Duration
            };
        }
        
        /// <summary>
        /// تبدیل لیست Appointment به ViewModel
        /// ✅ Factory Method Pattern
        /// </summary>
        public static List<MedicalRecordAppointmentViewModel> ToAppointmentViewModelList(
            IEnumerable<Models.Entities.Appointment.Appointment> entities)
        {
            return entities?.Select(e => ToViewModel(e)).Where(vm => vm != null).ToList() 
                ?? new List<MedicalRecordAppointmentViewModel>();
        }
        
        /// <summary>
        /// تبدیل وضعیت نوبت به متن فارسی
        /// </summary>
        private static string GetAppointmentStatusText(Models.Enums.AppointmentStatus status)
        {
            switch (status)
            {
                case Models.Enums.AppointmentStatus.Available:
                    return "در دسترس";
                case Models.Enums.AppointmentStatus.Scheduled:
                    return "ثبت شده";
                case Models.Enums.AppointmentStatus.Pending:
                    return "در انتظار";
                case Models.Enums.AppointmentStatus.Completed:
                    return "انجام شده";
                case Models.Enums.AppointmentStatus.Cancelled:
                    return "لغو شده";
                case Models.Enums.AppointmentStatus.NoShow:
                    return "عدم حضور";
                default:
                    return "نامشخص";
            }
        }
        
        #endregion
        
        #region Reception Factory Methods
        
        /// <summary>
        /// تبدیل Reception Entity به ViewModel
        /// ✅ Factory Method Pattern
        /// </summary>
        public static MedicalRecordReceptionViewModel ToViewModel(
            Models.Entities.Reception.Reception entity,
            string doctorName = null,
            string departmentName = null,
            string clinicName = null)
        {
            if (entity == null) return null;
            
            return new MedicalRecordReceptionViewModel
            {
                ReceptionId = entity.ReceptionId,
                ReceptionNumber = entity.ReceptionNumber,
                DoctorId = entity.DoctorId,
                DoctorName = doctorName ?? "نامشخص",
                DepartmentName = departmentName,
                ClinicName = clinicName,
                ReceptionDate = entity.ReceptionDate,
                ReceptionDateShamsi = entity.ReceptionDate.ToPersianDate(),
                ReceptionTime = entity.ReceptionDate.ToString("HH:mm"),
                Status = entity.Status,
                StatusText = GetReceptionStatusText(entity.Status),
                TotalAmount = entity.TotalAmount,
                PatientShare = entity.PatientCoPay,
                InsurerShare = entity.InsurerShareAmount,
                Notes = entity.Notes,
                IsEmergency = entity.IsEmergency
            };
        }
        
        /// <summary>
        /// تبدیل لیست Reception به ViewModel
        /// ✅ Factory Method Pattern
        /// </summary>
        public static List<MedicalRecordReceptionViewModel> ToReceptionViewModelList(
            IEnumerable<Models.Entities.Reception.Reception> entities)
        {
            return entities?.Select(e => ToViewModel(e)).Where(vm => vm != null).ToList() 
                ?? new List<MedicalRecordReceptionViewModel>();
        }
        
        /// <summary>
        /// تبدیل وضعیت پذیرش به متن فارسی
        /// </summary>
        private static string GetReceptionStatusText(Models.Enums.ReceptionStatus status)
        {
            switch (status)
            {
                case Models.Enums.ReceptionStatus.Pending:
                    return "در انتظار";
                case Models.Enums.ReceptionStatus.InProgress:
                    return "در حال انجام";
                case Models.Enums.ReceptionStatus.Completed:
                    return "تکمیل شده";
                case Models.Enums.ReceptionStatus.Cancelled:
                    return "لغو شده";
                default:
                    return "نامشخص";
            }
        }
        
        #endregion
        
        #region Triage Factory Methods
        
        /// <summary>
        /// تبدیل TriageAssessment Entity به ViewModel
        /// ✅ Factory Method Pattern
        /// </summary>
        public static MedicalRecordTriageViewModel ToViewModel(
            Models.Entities.Triage.TriageAssessment entity,
            Models.Entities.Triage.TriageVitalSigns vitalSigns = null,
            string assessorName = null)
        {
            if (entity == null) return null;
            
            return new MedicalRecordTriageViewModel
            {
                TriageAssessmentId = entity.TriageAssessmentId,
                AssessmentNumber = entity.AssessmentNumber,
                AssessorName = assessorName ?? "نامشخص",
                Level = entity.Level,
                LevelText = GetTriageLevelText(entity.Level),
                EsiScore = entity.EsiScore,
                News2Score = entity.News2Score,
                PewsScore = entity.PewsScore,
                ChiefComplaint = entity.ChiefComplaint,
                ArrivalAt = entity.ArrivalAt,
                ArrivalAtShamsi = entity.ArrivalAt.ToPersianDateTime(),
                TriageStartAt = entity.TriageStartAt,
                TriageStartAtShamsi = entity.TriageStartAt.ToPersianDateTime(),
                TriageEndAt = entity.TriageEndAt,
                TriageEndAtShamsi = entity.TriageEndAt?.ToPersianDateTime(),
                VitalSigns = vitalSigns != null ? ToVitalSignsViewModel(vitalSigns) : null
            };
        }
        
        /// <summary>
        /// تبدیل TriageVitalSigns Entity به ViewModel
        /// ✅ Factory Method Pattern
        /// </summary>
        public static MedicalRecordVitalSignsViewModel ToVitalSignsViewModel(
            Models.Entities.Triage.TriageVitalSigns entity)
        {
            if (entity == null) return null;
            
            return new MedicalRecordVitalSignsViewModel
            {
                SystolicBP = entity.SystolicBP,
                DiastolicBP = entity.DiastolicBP,
                HeartRate = entity.HeartRate,
                Temperature = entity.Temperature,
                RespiratoryRate = entity.RespiratoryRate,
                OxygenSaturation = entity.OxygenSaturation,
                PainLevel = entity.PainLevel,
                Weight = entity.Weight,
                Height = entity.Height,
                BMI = entity.BMI,
                GcsTotal = entity.GcsTotal
            };
        }
        
        /// <summary>
        /// تبدیل لیست TriageAssessment به ViewModel
        /// ✅ Factory Method Pattern
        /// </summary>
        public static List<MedicalRecordTriageViewModel> ToTriageViewModelList(
            IEnumerable<Models.Entities.Triage.TriageAssessment> entities)
        {
            return entities?.Select(e => ToViewModel(e)).Where(vm => vm != null).ToList() 
                ?? new List<MedicalRecordTriageViewModel>();
        }
        
        /// <summary>
        /// تبدیل سطح تریاژ به متن فارسی
        /// </summary>
        private static string GetTriageLevelText(Models.Enums.TriageLevel level)
        {
            switch (level)
            {
                case Models.Enums.TriageLevel.ESI1:
                    return "ESI 1 - نیاز به مراقبت فوری";
                case Models.Enums.TriageLevel.ESI2:
                    return "ESI 2 - نیاز به مراقبت فوری";
                case Models.Enums.TriageLevel.ESI3:
                    return "ESI 3 - نیاز به مراقبت فوری";
                case Models.Enums.TriageLevel.ESI4:
                    return "ESI 4 - نیاز به مراقبت فوری";
                case Models.Enums.TriageLevel.ESI5:
                    return "ESI 5 - نیاز به مراقبت فوری";
                default:
                    return "نامشخص";
            }
        }
        
        #endregion
    }
}



