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
            
            var medications = entity.Medications?
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Id)
                .Select(m => new MedicalHistoryMedicationItemDto
                {
                    Id = m.Id,
                    DrugName = m.DrugName,
                    Dosage = m.Dosage,
                    DosageUnit = m.DosageUnit,
                    Frequency = m.Frequency,
                    Route = m.Route,
                    StartDate = m.StartDate,
                    StartDateShamsi = m.StartDate?.ToPersianDate(),
                    EndDate = m.EndDate,
                    EndDateShamsi = m.EndDate?.ToPersianDate(),
                    Indication = m.Indication,
                    PrescribingDoctor = m.PrescribingDoctor,
                    IsActive = m.IsActive
                }).ToList() ?? new List<MedicalHistoryMedicationItemDto>();
            
            var labResults = entity.LabResults?
                .OrderBy(l => l.LabDate)
                .ThenBy(l => l.Id)
                .Select(l => new MedicalHistoryLabResultItemDto
                {
                    Id = l.Id,
                    LabName = l.LabName,
                    Value = l.Value,
                    Unit = l.Unit,
                    LabDate = l.LabDate,
                    LabDateShamsi = l.LabDate.ToPersianDate(),
                    ReferenceRange = l.ReferenceRange
                }).ToList() ?? new List<MedicalHistoryLabResultItemDto>();
            
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
                IsCritical = entity.IsCritical,
                Medications = medications,
                LabResults = labResults,
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
            
            var title = viewModel.Title?.Trim();
            if (viewModel.Type == Models.Enums.MedicalHistoryType.Medication && string.IsNullOrEmpty(title) && !string.IsNullOrWhiteSpace(viewModel.DrugName))
                title = viewModel.DrugName.Trim();
            
            var entity = new MedicalHistory
            {
                PatientId = patientId,
                Type = viewModel.Type,
                Title = title ?? string.Empty,
                Description = viewModel.Description,
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
                IsActive = viewModel.IsActive,
                Severity = viewModel.Severity,
                DoctorName = viewModel.DoctorName,
                MedicalCenter = viewModel.MedicalCenter,
                Attachments = viewModel.Attachments,
                IsCritical = viewModel.IsCritical,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.Now
            };
            
            if (viewModel.Type == Models.Enums.MedicalHistoryType.Medication)
            {
                var drugName = viewModel.DrugName?.Trim();
                if (string.IsNullOrWhiteSpace(drugName) && !string.IsNullOrWhiteSpace(title))
                    drugName = title;
                if (!string.IsNullOrWhiteSpace(drugName))
                {
                    entity.Medications.Add(new MedicalHistoryMedication
                    {
                        DrugName = drugName,
                        Dosage = viewModel.Dosage?.Trim(),
                    DosageUnit = viewModel.DosageUnit?.Trim(),
                    Frequency = viewModel.Frequency?.Trim(),
                    Route = viewModel.Route?.Trim(),
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    Indication = viewModel.Indication?.Trim(),
                    PrescribingDoctor = viewModel.PrescribingDoctor?.Trim(),
                    IsActive = viewModel.IsActive,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = createdByUserId
                });
                }
            }
            
            if (viewModel.Type == Models.Enums.MedicalHistoryType.Disease && 
                viewModel.MedicationsList != null && viewModel.MedicationsList.Any())
            {
                var order = 0;
                foreach (var m in viewModel.MedicationsList.Where(x => !string.IsNullOrWhiteSpace(x.DrugName)))
                {
                    entity.Medications.Add(new MedicalHistoryMedication
                    {
                        DrugName = (m.DrugName ?? "").Trim(),
                        Dosage = m.Dosage?.Trim(),
                        DosageUnit = m.DosageUnit?.Trim(),
                        Frequency = m.Frequency?.Trim(),
                        Route = m.Route?.Trim(),
                        StartDate = viewModel.StartDate,
                        EndDate = viewModel.EndDate,
                        Indication = m.Indication?.Trim(),
                        PrescribingDoctor = m.PrescribingDoctor?.Trim(),
                        IsActive = true,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = createdByUserId
                    });
                }
            }
            
            if (!string.IsNullOrWhiteSpace(viewModel.LabName?.Trim()))
            {
                entity.LabResults.Add(new MedicalHistoryLabResult
                {
                    LabName = viewModel.LabName.Trim(),
                    Value = viewModel.LabValue?.Trim(),
                    Unit = viewModel.LabUnit?.Trim(),
                    LabDate = viewModel.LabDate ?? entity.StartDate ?? DateTime.UtcNow.Date,
                    ReferenceRange = viewModel.LabReferenceRange?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = createdByUserId
                });
            }
            
            return entity;
        }
        
        /// <summary>
        /// به‌روزرسانی Entity از ViewModel (برای Update)
        /// ✅ Factory Method Pattern
        /// </summary>
        public static void UpdateEntity(MedicalHistory entity, 
            MedicalHistoryCreateEditViewModel viewModel, string updatedByUserId)
        {
            if (entity == null || viewModel == null) return;
            
            var title = viewModel.Title?.Trim();
            if (viewModel.Type == Models.Enums.MedicalHistoryType.Medication && string.IsNullOrEmpty(title) && !string.IsNullOrWhiteSpace(viewModel.DrugName))
                title = viewModel.DrugName.Trim();
            
            entity.Type = viewModel.Type;
            entity.Title = title ?? entity.Title;
            entity.Description = viewModel.Description;
            entity.StartDate = viewModel.StartDate;
            entity.EndDate = viewModel.EndDate;
            entity.IsActive = viewModel.IsActive;
            entity.Severity = viewModel.Severity;
            entity.DoctorName = viewModel.DoctorName;
            entity.MedicalCenter = viewModel.MedicalCenter;
            entity.Attachments = viewModel.Attachments;
            entity.IsCritical = viewModel.IsCritical;
            entity.UpdatedByUserId = updatedByUserId;
            entity.UpdatedAt = DateTime.Now;
        }
        
        /// <summary>
        /// همگام‌سازی داروهای مرتبط با تاریخچه (یک دارو برای نوع دارو، چند دارو برای نوع بیماری)
        /// </summary>
        public static void SyncMedicationFromViewModel(MedicalHistory entity,
            MedicalHistoryCreateEditViewModel viewModel, string userId)
        {
            if (entity == null || viewModel == null || entity.Medications == null) return;
            
            if (viewModel.Type == Models.Enums.MedicalHistoryType.Disease && 
                viewModel.MedicationsList != null && viewModel.MedicationsList.Any())
            {
                entity.Medications.Clear();
                var order = 0;
                foreach (var m in viewModel.MedicationsList.Where(x => !string.IsNullOrWhiteSpace(x.DrugName)))
                {
                    entity.Medications.Add(new MedicalHistoryMedication
                    {
                        MedicalHistoryId = entity.MedicalHistoryId,
                        DrugName = (m.DrugName ?? "").Trim(),
                        Dosage = m.Dosage?.Trim(),
                        DosageUnit = m.DosageUnit?.Trim(),
                        Frequency = m.Frequency?.Trim(),
                        Route = m.Route?.Trim(),
                        StartDate = viewModel.StartDate,
                        EndDate = viewModel.EndDate,
                        Indication = m.Indication?.Trim(),
                        PrescribingDoctor = m.PrescribingDoctor?.Trim(),
                        IsActive = true,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = userId
                    });
                }
                return;
            }
            
            if (viewModel.Type != Models.Enums.MedicalHistoryType.Medication)
            {
                entity.Medications?.Clear();
                return;
            }
            
            var drugName = viewModel.DrugName?.Trim();
            if (string.IsNullOrWhiteSpace(drugName) && !string.IsNullOrWhiteSpace(viewModel.Title))
                drugName = viewModel.Title.Trim();
            if (string.IsNullOrWhiteSpace(drugName))
            {
                entity.Medications?.Clear();
                return;
            }
            
            var med = entity.Medications?.FirstOrDefault();
            if (med == null)
            {
                entity.Medications.Add(new MedicalHistoryMedication
                {
                    MedicalHistoryId = entity.MedicalHistoryId,
                    DrugName = drugName,
                    Dosage = viewModel.Dosage?.Trim(),
                    DosageUnit = viewModel.DosageUnit?.Trim(),
                    Frequency = viewModel.Frequency?.Trim(),
                    Route = viewModel.Route?.Trim(),
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    Indication = viewModel.Indication?.Trim(),
                    PrescribingDoctor = viewModel.PrescribingDoctor?.Trim(),
                    IsActive = viewModel.IsActive,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = userId
                });
            }
            else
            {
                med.DrugName = drugName;
                med.Dosage = viewModel.Dosage?.Trim();
                med.DosageUnit = viewModel.DosageUnit?.Trim();
                med.Frequency = viewModel.Frequency?.Trim();
                med.Route = viewModel.Route?.Trim();
                med.StartDate = viewModel.StartDate;
                med.EndDate = viewModel.EndDate;
                med.Indication = viewModel.Indication?.Trim();
                med.PrescribingDoctor = viewModel.PrescribingDoctor?.Trim();
                med.IsActive = viewModel.IsActive;
            }
        }
        
        /// <summary>
        /// همگام‌سازی نتایج آزمایش (یک آزمایش در فرم ساده)
        /// </summary>
        public static void SyncLabFromViewModel(MedicalHistory entity,
            MedicalHistoryCreateEditViewModel viewModel, string userId)
        {
            if (entity == null || viewModel == null || entity.LabResults == null) return;
            
            if (string.IsNullOrWhiteSpace(viewModel.LabName?.Trim()))
            {
                entity.LabResults.Clear();
                return;
            }
            
            var lab = entity.LabResults.FirstOrDefault();
            var labDate = viewModel.LabDate ?? entity.StartDate ?? DateTime.UtcNow.Date;
            if (lab == null)
            {
                entity.LabResults.Add(new MedicalHistoryLabResult
                {
                    MedicalHistoryId = entity.MedicalHistoryId,
                    LabName = viewModel.LabName.Trim(),
                    Value = viewModel.LabValue?.Trim(),
                    Unit = viewModel.LabUnit?.Trim(),
                    LabDate = labDate,
                    ReferenceRange = viewModel.LabReferenceRange?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = userId
                });
            }
            else
            {
                lab.LabName = viewModel.LabName.Trim();
                lab.Value = viewModel.LabValue?.Trim();
                lab.Unit = viewModel.LabUnit?.Trim();
                lab.LabDate = labDate;
                lab.ReferenceRange = viewModel.LabReferenceRange?.Trim();
            }
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



