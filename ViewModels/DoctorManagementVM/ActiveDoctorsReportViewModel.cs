using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using ClinicApp.Extensions;
using FluentValidation;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل گزارش پزشکان فعال برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. گزارش جامع از پزشکان فعال در سیستم
    /// 2. آمار و اطلاعات تفصیلی عملکرد پزشکان
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 4. رعایت استانداردهای پزشکی ایران در گزارش‌گیری
    /// 5. پشتیبانی از فیلترهای پیشرفته و جستجوی هوشمند
    /// </summary>
    public class ActiveDoctorsReportViewModel
    {
        /// <summary>
        /// آمار کلی گزارش
        /// </summary>
        public ReportStatisticsViewModel Statistics { get; set; } = new ReportStatisticsViewModel();

        /// <summary>
        /// لیست پزشکان فعال
        /// </summary>
        public List<DoctorReportItemViewModel> Doctors { get; set; } = new List<DoctorReportItemViewModel>();

        /// <summary>
        /// فیلترهای گزارش
        /// </summary>
        public ReportFilterViewModel Filters { get; set; } = new ReportFilterViewModel();

        /// <summary>
        /// تاریخ تولید گزارش
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// نام کاربر تولید کننده گزارش
        /// </summary>
        public string GeneratedBy { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static ActiveDoctorsReportViewModel FromEntity(IEnumerable<Doctor> doctors, ReportFilterViewModel filters = null)
        {
            if (doctors == null) return null;
            
            var doctorList = doctors.ToList();
            var report = new ActiveDoctorsReportViewModel
            {
                GeneratedAt = DateTime.Now,
                GeneratedBy = "سیستم",
                Filters = filters ?? new ReportFilterViewModel(),
                Doctors = doctorList.Select(DoctorReportItemViewModel.FromEntity).ToList()
            };

            // محاسبه آمار
            report.Statistics = ReportStatisticsViewModel.FromEntity(doctorList);

            return report;
        }
    }

    /// <summary>
    /// مدل آیتم پزشک در گزارش
    /// </summary>
    public class DoctorReportItemViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام کامل پزشک
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        public string Specialization { get; set; }

        /// <summary>
        /// شماره تلفن پزشک
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// تعداد دپارتمان‌های فعال
        /// </summary>
        public int ActiveDepartmentCount { get; set; }

        /// <summary>
        /// تعداد سرفصل‌های خدماتی فعال
        /// </summary>
        public int ActiveServiceCategoryCount { get; set; }

        /// <summary>
        /// تعداد نوبت‌های امروز
        /// </summary>
        public int TodayAppointments { get; set; }

        /// <summary>
        /// تعداد نوبت‌های فردا
        /// </summary>
        public int TomorrowAppointments { get; set; }

        /// <summary>
        /// تعداد کل نوبت‌های این ماه
        /// </summary>
        public int ThisMonthAppointments { get; set; }

        /// <summary>
        /// تعداد نوبت‌های تکمیل شده این ماه
        /// </summary>
        public int ThisMonthCompleted { get; set; }

        /// <summary>
        /// درصد تکمیل نوبت‌ها
        /// </summary>
        public decimal CompletionRate { get; set; }

        /// <summary>
        /// تاریخ شروع فعالیت
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ شروع فعالیت به شمسی
        /// </summary>
        public string StartDateShamsi { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DoctorReportItemViewModel FromEntity(Doctor doctor)
        {
            if (doctor == null) return null;
            
            var startDate = doctor.DoctorDepartments?
                .Where(dd => dd.IsActive && dd.StartDate.HasValue)
                .Min(dd => dd.StartDate);

            var todayAppointments = doctor.Appointments?
                .Count(a => a.AppointmentDate.Date == DateTime.Today) ?? 0;

            var tomorrowAppointments = doctor.Appointments?
                .Count(a => a.AppointmentDate.Date == DateTime.Today.AddDays(1)) ?? 0;

            var thisMonthAppointments = doctor.Appointments?
                .Count(a => a.AppointmentDate.Month == DateTime.Now.Month && 
                           a.AppointmentDate.Year == DateTime.Now.Year) ?? 0;

            var thisMonthCompleted = doctor.Appointments?
                .Count(a => a.AppointmentDate.Month == DateTime.Now.Month && 
                           a.AppointmentDate.Year == DateTime.Now.Year && 
                           a.Status == AppointmentStatus.Completed) ?? 0;

            var completionRate = thisMonthAppointments > 0 
                ? Math.Round((decimal)thisMonthCompleted / thisMonthAppointments * 100, 2) 
                : 0;

            return new DoctorReportItemViewModel
            {
                DoctorId = doctor.DoctorId,
                FullName = $"{doctor.FirstName} {doctor.LastName}",
                Specialization = doctor.Specialization,
                PhoneNumber = doctor.PhoneNumber,
                ActiveDepartmentCount = doctor.DoctorDepartments?.Count(dd => 
                    dd.Department != null && 
                    !dd.Department.IsDeleted && 
                    dd.IsActive) ?? 0,
                ActiveServiceCategoryCount = doctor.DoctorServiceCategories?.Count(dsc => 
                    dsc.ServiceCategory != null && 
                    !dsc.ServiceCategory.IsDeleted && 
                    dsc.IsActive) ?? 0,
                TodayAppointments = todayAppointments,
                TomorrowAppointments = tomorrowAppointments,
                ThisMonthAppointments = thisMonthAppointments,
                ThisMonthCompleted = thisMonthCompleted,
                CompletionRate = completionRate,
                StartDate = startDate,
                StartDateShamsi = startDate?.ToPersianDateTime(),
                IsActive = doctor.IsActive
            };
        }
    }

    /// <summary>
    /// مدل آمار گزارش
    /// </summary>
    public class ReportStatisticsViewModel
    {
        /// <summary>
        /// تعداد کل پزشکان فعال
        /// </summary>
        public int TotalActiveDoctors { get; set; }

        /// <summary>
        /// تعداد پزشکان دارای نوبت امروز
        /// </summary>
        public int DoctorsWithTodayAppointments { get; set; }

        /// <summary>
        /// تعداد پزشکان دارای نوبت فردا
        /// </summary>
        public int DoctorsWithTomorrowAppointments { get; set; }

        /// <summary>
        /// میانگین نوبت‌های امروز به ازای هر پزشک
        /// </summary>
        public decimal AverageTodayAppointments { get; set; }

        /// <summary>
        /// میانگین نوبت‌های فردا به ازای هر پزشک
        /// </summary>
        public decimal AverageTomorrowAppointments { get; set; }

        /// <summary>
        /// میانگین درصد تکمیل نوبت‌ها
        /// </summary>
        public decimal AverageCompletionRate { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static ReportStatisticsViewModel FromEntity(IEnumerable<Doctor> doctors)
        {
            if (doctors == null || !doctors.Any()) 
                return new ReportStatisticsViewModel();

            var doctorList = doctors.ToList();
            var totalActiveDoctors = doctorList.Count(d => d.IsActive);
            
            var doctorsWithTodayAppointments = doctorList.Count(d => 
                d.Appointments?.Any(a => a.AppointmentDate.Date == DateTime.Today) == true);

            var doctorsWithTomorrowAppointments = doctorList.Count(d => 
                d.Appointments?.Any(a => a.AppointmentDate.Date == DateTime.Today.AddDays(1)) == true);

            var totalTodayAppointments = doctorList.Sum(d => 
                d.Appointments?.Count(a => a.AppointmentDate.Date == DateTime.Today) ?? 0);

            var totalTomorrowAppointments = doctorList.Sum(d => 
                d.Appointments?.Count(a => a.AppointmentDate.Date == DateTime.Today.AddDays(1)) ?? 0);

            var averageTodayAppointments = totalActiveDoctors > 0 
                ? Math.Round((decimal)totalTodayAppointments / totalActiveDoctors, 2) 
                : 0;

            var averageTomorrowAppointments = totalActiveDoctors > 0 
                ? Math.Round((decimal)totalTomorrowAppointments / totalActiveDoctors, 2) 
                : 0;

            var completionRates = doctorList.Select(d =>
            {
                var thisMonthAppointments = d.Appointments?.Count(a => 
                    a.AppointmentDate.Month == DateTime.Now.Month && 
                    a.AppointmentDate.Year == DateTime.Now.Year) ?? 0;

                var thisMonthCompleted = d.Appointments?.Count(a => 
                    a.AppointmentDate.Month == DateTime.Now.Month && 
                    a.AppointmentDate.Year == DateTime.Now.Year && 
                    a.Status == AppointmentStatus.Completed) ?? 0;

                return thisMonthAppointments > 0 
                    ? (decimal)thisMonthCompleted / thisMonthAppointments * 100 
                    : 0;
            }).ToList();

            var averageCompletionRate = completionRates.Any() 
                ? Math.Round(completionRates.Average(), 2) 
                : 0;

            return new ReportStatisticsViewModel
            {
                TotalActiveDoctors = totalActiveDoctors,
                DoctorsWithTodayAppointments = doctorsWithTodayAppointments,
                DoctorsWithTomorrowAppointments = doctorsWithTomorrowAppointments,
                AverageTodayAppointments = averageTodayAppointments,
                AverageTomorrowAppointments = averageTomorrowAppointments,
                AverageCompletionRate = averageCompletionRate
            };
        }
    }

    /// <summary>
    /// مدل فیلترهای گزارش
    /// </summary>
    public class ReportFilterViewModel
    {
        /// <summary>
        /// شناسه کلینیک
        /// </summary>
        public int? ClinicId { get; set; }

        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// شناسه سرفصل خدماتی
        /// </summary>
        public int? ServiceCategoryId { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// عبارت جستجو
        /// </summary>
        public string SearchTerm { get; set; }
    }

    /// <summary>
    /// ولیدیتور برای مدل گزارش پزشکان فعال
    /// </summary>
    public class ActiveDoctorsReportViewModelValidator : AbstractValidator<ActiveDoctorsReportViewModel>
    {
        public ActiveDoctorsReportViewModelValidator()
        {
            RuleFor(x => x.Statistics)
                .NotNull()
                .WithMessage("آمار گزارش الزامی است.");

            RuleFor(x => x.Doctors)
                .NotNull()
                .WithMessage("لیست پزشکان الزامی است.");

            RuleFor(x => x.GeneratedAt)
                .NotEmpty()
                .WithMessage("تاریخ تولید گزارش الزامی است.");

            RuleFor(x => x.GeneratedBy)
                .NotEmpty()
                .WithMessage("نام تولید کننده گزارش الزامی است.")
                .MaximumLength(100)
                .WithMessage("نام تولید کننده گزارش نمی‌تواند بیش از 100 کاراکتر باشد.");
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل آیتم پزشک در گزارش
    /// </summary>
    public class DoctorReportItemViewModelValidator : AbstractValidator<DoctorReportItemViewModel>
    {
        public DoctorReportItemViewModelValidator()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است.");

            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage("نام کامل پزشک الزامی است.")
                .MaximumLength(200)
                .WithMessage("نام کامل پزشک نمی‌تواند بیش از 200 کاراکتر باشد.");

            RuleFor(x => x.Specialization)
                .MaximumLength(250)
                .WithMessage("تخصص پزشک نمی‌تواند بیش از 250 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.Specialization));

            RuleFor(x => x.PhoneNumber)
                .Must(PersianNumberHelper.IsValidPhoneNumber)
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("شماره تلفن پزشک نامعتبر است.");

            RuleFor(x => x.ActiveDepartmentCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد دپارتمان‌های فعال نمی‌تواند منفی باشد.");

            RuleFor(x => x.ActiveServiceCategoryCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد سرفصل‌های خدماتی فعال نمی‌تواند منفی باشد.");

            RuleFor(x => x.TodayAppointments)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد نوبت‌های امروز نمی‌تواند منفی باشد.");

            RuleFor(x => x.TomorrowAppointments)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد نوبت‌های فردا نمی‌تواند منفی باشد.");

            RuleFor(x => x.ThisMonthAppointments)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد کل نوبت‌های این ماه نمی‌تواند منفی باشد.");

            RuleFor(x => x.ThisMonthCompleted)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد نوبت‌های تکمیل شده این ماه نمی‌تواند منفی باشد.");

            RuleFor(x => x.CompletionRate)
                .InclusiveBetween(0, 100)
                .WithMessage("درصد تکمیل نوبت‌ها باید بین 0 تا 100 باشد.");
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل آمار گزارش
    /// </summary>
    public class ReportStatisticsViewModelValidator : AbstractValidator<ReportStatisticsViewModel>
    {
        public ReportStatisticsViewModelValidator()
        {
            RuleFor(x => x.TotalActiveDoctors)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد کل پزشکان فعال نمی‌تواند منفی باشد.");

            RuleFor(x => x.DoctorsWithTodayAppointments)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد پزشکان دارای نوبت امروز نمی‌تواند منفی باشد.");

            RuleFor(x => x.DoctorsWithTomorrowAppointments)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد پزشکان دارای نوبت فردا نمی‌تواند منفی باشد.");

            RuleFor(x => x.AverageTodayAppointments)
                .GreaterThanOrEqualTo(0)
                .WithMessage("میانگین نوبت‌های امروز نمی‌تواند منفی باشد.");

            RuleFor(x => x.AverageTomorrowAppointments)
                .GreaterThanOrEqualTo(0)
                .WithMessage("میانگین نوبت‌های فردا نمی‌تواند منفی باشد.");

            RuleFor(x => x.AverageCompletionRate)
                .InclusiveBetween(0, 100)
                .WithMessage("میانگین درصد تکمیل نوبت‌ها باید بین 0 تا 100 باشد.");
        }
    }
}
