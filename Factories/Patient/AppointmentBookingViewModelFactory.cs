using System;
using System.Collections.Generic;
using ClinicApp.Models.DTOs.Appointment;
using ClinicApp.ViewModels.ClinicAdmin;
using ClinicApp.ViewModels.Patient;

namespace ClinicApp.Factories.Patient
{
    /// <summary>
    /// ✅ Factory Pattern برای ViewModels ماژول رزرو نوبت
    /// طبق: DEVELOPMENT_CONTRACT.md - Strongly-Typed Development
    /// 
    /// مسئولیت: تبدیل DTOs به ViewModels
    /// اصول: Single Responsibility, Separation of Concerns
    /// </summary>
    public static class AppointmentBookingViewModelFactory
    {
        /// <summary>
        /// ایجاد ViewModel برای صفحه انتخاب پزشک
        /// </summary>
        public static DoctorSelectionViewModel CreateDoctorSelectionViewModel(
            List<DoctorSearchResultDto> doctors,
            List<DepartmentInfo> departments,
            int? selectedDepartmentId = null,
            string searchTerm = null)
        {
            if (doctors == null)
                doctors = new List<DoctorSearchResultDto>();
            
            if (departments == null)
                departments = new List<DepartmentInfo>();

            return new DoctorSelectionViewModel
            {
                Doctors = doctors,
                Departments = departments,
                SelectedDepartmentId = selectedDepartmentId,
                SearchTerm = searchTerm
            };
        }

        /// <summary>
        /// ایجاد ViewModel برای صفحه انتخاب تاریخ (ساده، سازگار با قبل)
        /// </summary>
        public static DateSelectionViewModel CreateDateSelectionViewModel(
            int doctorId,
            string doctorName,
            string doctorSpecialization)
        {
            return new DateSelectionViewModel
            {
                DoctorId = doctorId,
                DoctorName = doctorName ?? "نامشخص",
                DoctorSpecialization = doctorSpecialization ?? "نامشخص"
            };
        }

        /// <summary>
        /// ایجاد ViewModel برای صفحه انتخاب تاریخ با اطلاعات کامل پزشک (Context-Aware UX)
        /// </summary>
        public static DateSelectionViewModel CreateDateSelectionViewModelFromDoctor(DoctorSearchResultDto doctor)
        {
            if (doctor == null)
                return new DateSelectionViewModel { DoctorName = "نامشخص", DoctorSpecialization = "نامشخص" };
            var first = doctor.AvailableDates != null && doctor.AvailableDates.Count > 0 ? doctor.AvailableDates[0] : null;
            var firstText = first != null
                ? $"{first.DayName} {first.PersianDate} — {first.TimeRange}"
                : null;
            return new DateSelectionViewModel
            {
                DoctorId = doctor.DoctorId,
                DoctorName = doctor.FullName ?? "نامشخص",
                DoctorSpecialization = doctor.Specialization ?? "نامشخص",
                MedicalCouncilCode = doctor.MedicalCouncilCode ?? "",
                FirstAvailableSlotText = firstText,
                HasOnlineConsultation = doctor.HasOnlineConsultation,
                AvailableDates = doctor.AvailableDates ?? new List<AvailableDateInfo>()
            };
        }

        /// <summary>
        /// ایجاد ViewModel برای صفحه انتخاب زمان
        /// </summary>
        public static TimeSlotSelectionViewModel CreateTimeSlotSelectionViewModel(
            int doctorId,
            string doctorName,
            DateTime selectedDate,
            List<AvailableTimeSlotDto> availableSlots,
            int appointmentDuration)
        {
            if (availableSlots == null)
                availableSlots = new List<AvailableTimeSlotDto>();

            return new TimeSlotSelectionViewModel
            {
                DoctorId = doctorId,
                DoctorName = doctorName ?? "نامشخص",
                SelectedDate = selectedDate,
                AvailableSlots = availableSlots,
                AppointmentDuration = appointmentDuration
            };
        }

        /// <summary>
        /// ایجاد ViewModel برای صفحه تایید رزرو
        /// </summary>
        public static AppointmentBookingViewModel CreateAppointmentBookingViewModel(
            int doctorId,
            string doctorName,
            string doctorSpecialization,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime,
            decimal price,
            int? serviceCategoryId = null,
            string description = null)
        {
            return new AppointmentBookingViewModel
            {
                DoctorId = doctorId,
                DoctorName = doctorName ?? "نامشخص",
                DoctorSpecialization = doctorSpecialization ?? "نامشخص",
                AppointmentDate = appointmentDate,
                StartTime = startTime,
                EndTime = endTime,
                Price = price,
                ServiceCategoryId = serviceCategoryId,
                Description = description
            };
        }
    }
}

