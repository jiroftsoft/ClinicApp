using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Extensions;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using FluentValidation;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل داده‌های داشبورد پزشک برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش داده‌های کلیدی برای داشبورد پزشک
    /// 2. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 3. رعایت استانداردهای پزشکی ایران در نمایش اطلاعات
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// </summary>
    public class DoctorDashboardViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام کامل پزشک
        /// </summary>
        public string DoctorName { get; set; }

        /// <summary>
        /// تعداد نوبت‌های امروز
        /// </summary>
        public int TodayAppointmentsCount { get; set; }

        /// <summary>
        /// تعداد نوبت‌های فردا
        /// </summary>
        public int TomorrowAppointmentsCount { get; set; }

        /// <summary>
        /// تعداد نوبت‌های این هفته
        /// </summary>
        public int ThisWeekAppointmentsCount { get; set; }

        /// <summary>
        /// تعداد نوبت‌های این ماه
        /// </summary>
        public int ThisMonthAppointmentsCount { get; set; }

        /// <summary>
        /// درآمد امروز
        /// </summary>
        public decimal TodayRevenue { get; set; }

        /// <summary>
        /// درآمد این هفته
        /// </summary>
        public decimal ThisWeekRevenue { get; set; }

        /// <summary>
        /// درآمد این ماه
        /// </summary>
        public decimal ThisMonthRevenue { get; set; }

        /// <summary>
        /// درآمد ماه گذشته
        /// </summary>
        public decimal LastMonthRevenue { get; set; }

        /// <summary>
        /// درصد تغییر درآمد نسبت به ماه گذشته
        /// </summary>
        public double RevenueChangePercentage { get; set; }

        /// <summary>
        /// تعداد بیماران جدید امروز
        /// </summary>
        public int NewPatientsToday { get; set; }

        /// <summary>
        /// تعداد بیماران جدید این هفته
        /// </summary>
        public int NewPatientsThisWeek { get; set; }

        /// <summary>
        /// لیست نوبت‌های امروز
        /// </summary>
        public List<TodayAppointmentViewModel> TodayAppointments { get; set; } = new List<TodayAppointmentViewModel>();

        /// <summary>
        /// لیست نوبت‌های فردا
        /// </summary>
        public List<TomorrowAppointmentViewModel> TomorrowAppointments { get; set; } = new List<TomorrowAppointmentViewModel>();

        /// <summary>
        /// آمار نوبت‌ها بر اساس دسته‌بندی خدمات
        /// </summary>
        public List<ServiceCategoryStatsViewModel> ServiceCategoryStats { get; set; } = new List<ServiceCategoryStatsViewModel>();

        /// <summary>
        /// آمار نوبت‌ها بر اساس روزهای هفته
        /// </summary>
        public List<DailyAppointmentStatsViewModel> DailyAppointmentStats { get; set; } = new List<DailyAppointmentStatsViewModel>();

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DoctorDashboardViewModel FromEntity(Doctor doctor)
        {
            if (doctor == null) return null;

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = monthStart.AddMonths(-1);

            var appointments = doctor.Appointments ?? new List<Appointment>();

            var todayAppointments = appointments
                .Where(a => a.AppointmentDate.Date == today)
                .Select(TodayAppointmentViewModel.FromEntity)
                .ToList();

            var tomorrowAppointments = appointments
                .Where(a => a.AppointmentDate.Date == tomorrow)
                .Select(TomorrowAppointmentViewModel.FromEntity)
                .ToList();

            var serviceCategoryStats = appointments
                .Where(a => a.AppointmentDate >= monthStart)
                .GroupBy(a => a.ServiceCategory?.Title ?? "نامشخص")
                .Select(g => ServiceCategoryStatsViewModel.FromEntity(g.Key, g.Count(), appointments.Count()))
                .ToList();

            var dailyStats = Enumerable.Range(0, 7)
                .Select(i => {
                    var day = weekStart.AddDays(i);
                    var count = appointments.Count(a => a.AppointmentDate.Date == day);
                    return DailyAppointmentStatsViewModel.FromEntity(day, count);
                })
                .ToList();

            return new DoctorDashboardViewModel
            {
                DoctorId = doctor.DoctorId,
                DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                TodayAppointmentsCount = todayAppointments.Count,
                TomorrowAppointmentsCount = tomorrowAppointments.Count,
                ThisWeekAppointmentsCount = appointments.Count(a => a.AppointmentDate >= weekStart && a.AppointmentDate < weekStart.AddDays(7)),
                ThisMonthAppointmentsCount = appointments.Count(a => a.AppointmentDate >= monthStart),
                TodayRevenue = appointments.Where(a => a.AppointmentDate.Date == today && a.Status == AppointmentStatus.Completed)
                    .Sum(a => a.Price),
                ThisWeekRevenue = appointments.Where(a => a.AppointmentDate >= weekStart && a.AppointmentDate < weekStart.AddDays(7) && a.Status == AppointmentStatus.Completed)
                    .Sum(a => a.Price),
                ThisMonthRevenue = appointments.Where(a => a.AppointmentDate >= monthStart && a.Status == AppointmentStatus.Completed)
                    .Sum(a => a.Price),
                LastMonthRevenue = appointments.Where(a => a.AppointmentDate >= lastMonthStart && a.AppointmentDate < monthStart && a.Status == AppointmentStatus.Completed)
                    .Sum(a => a.Price),
                RevenueChangePercentage = CalculateRevenueChangePercentage(
                    appointments.Where(a => a.AppointmentDate >= monthStart && a.Status == AppointmentStatus.Completed).Sum(a => a.Price),
                    appointments.Where(a => a.AppointmentDate >= lastMonthStart && a.AppointmentDate < monthStart && a.Status == AppointmentStatus.Completed).Sum(a => a.Price)
                ),
                NewPatientsToday = appointments.Where(a => a.AppointmentDate.Date == today && a.IsNewPatient).Count(),
                NewPatientsThisWeek = appointments.Where(a => a.AppointmentDate >= weekStart && a.AppointmentDate < weekStart.AddDays(7) && a.IsNewPatient).Count(),
                TodayAppointments = todayAppointments,
                TomorrowAppointments = tomorrowAppointments,
                ServiceCategoryStats = serviceCategoryStats,
                DailyAppointmentStats = dailyStats
            };
        }

        /// <summary>
        /// محاسبه درصد تغییر درآمد
        /// </summary>
        private static double CalculateRevenueChangePercentage(decimal currentMonth, decimal lastMonth)
        {
            if (lastMonth == 0) return currentMonth > 0 ? 100 : 0;
            return Math.Round((double)((currentMonth - lastMonth) / lastMonth * 100), 2);
        }
    }

    /// <summary>
    /// مدل نوبت امروز
    /// </summary>
    public class TodayAppointmentViewModel
    {
        /// <summary>
        /// شناسه نوبت
        /// </summary>
        public int AppointmentId { get; set; }

        /// <summary>
        /// زمان شروع نوبت
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// زمان شروع نوبت به فرمت شمسی
        /// </summary>
        public string StartTimeShamsi { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// شماره تلفن بیمار
        /// </summary>
        public string PatientPhoneNumber { get; set; }

        /// <summary>
        /// وضعیت نوبت
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static TodayAppointmentViewModel FromEntity(Appointment appointment)
        {
            if (appointment == null) return null;
            return new TodayAppointmentViewModel
            {
                AppointmentId = appointment.AppointmentId,
                StartTime = appointment.AppointmentDate,
                StartTimeShamsi = appointment.AppointmentDate.ToPersianDateTime(),
                PatientName = appointment.PatientName,
                PatientPhoneNumber = appointment.PatientPhone,
                Status = GetStatusDescription(appointment.Status)
            };
        }

        /// <summary>
        /// دریافت توضیحات وضعیت نوبت
        /// </summary>
        private static string GetStatusDescription(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Available => "در دسترس",
                AppointmentStatus.Scheduled => "رزرو شده",
                AppointmentStatus.Completed => "تکمیل شده",
                AppointmentStatus.Cancelled => "لغو شده",
                AppointmentStatus.NoShow => "عدم حضور",
                _ => "نامشخص"
            };
        }
    }

    /// <summary>
    /// مدل نوبت فردا
    /// </summary>
    public class TomorrowAppointmentViewModel
    {
        /// <summary>
        /// شناسه نوبت
        /// </summary>
        public int AppointmentId { get; set; }

        /// <summary>
        /// زمان شروع نوبت
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// زمان شروع نوبت به فرمت شمسی
        /// </summary>
        public string StartTimeShamsi { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// وضعیت نوبت
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static TomorrowAppointmentViewModel FromEntity(Appointment appointment)
        {
            if (appointment == null) return null;
            return new TomorrowAppointmentViewModel
            {
                AppointmentId = appointment.AppointmentId,
                StartTime = appointment.AppointmentDate,
                StartTimeShamsi = appointment.AppointmentDate.ToPersianDateTime(),
                PatientName = appointment.PatientName,
                Status = GetStatusDescription(appointment.Status)
            };
        }

        /// <summary>
        /// دریافت توضیحات وضعیت نوبت
        /// </summary>
        private static string GetStatusDescription(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Available => "در دسترس",
                AppointmentStatus.Scheduled => "رزرو شده",
                AppointmentStatus.Completed => "تکمیل شده",
                AppointmentStatus.Cancelled => "لغو شده",
                AppointmentStatus.NoShow => "عدم حضور",
                _ => "نامشخص"
            };
        }
    }

    /// <summary>
    /// مدل آمار دسته‌بندی خدمات
    /// </summary>
    public class ServiceCategoryStatsViewModel
    {
        /// <summary>
        /// نام دسته‌بندی خدمات
        /// </summary>
        public string ServiceCategoryName { get; set; }

        /// <summary>
        /// تعداد نوبت‌ها
        /// </summary>
        public int AppointmentCount { get; set; }

        /// <summary>
        /// درصد از کل
        /// </summary>
        public double Percentage { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static ServiceCategoryStatsViewModel FromEntity(string categoryName, int count, int total)
        {
            return new ServiceCategoryStatsViewModel
            {
                ServiceCategoryName = categoryName ?? "نامشخص",
                AppointmentCount = count,
                Percentage = total > 0 ? Math.Round((double)count / total * 100, 2) : 0
            };
        }
    }

    /// <summary>
    /// مدل آمار روزانه نوبت‌ها
    /// </summary>
    public class DailyAppointmentStatsViewModel
    {
        /// <summary>
        /// نام روز
        /// </summary>
        public string DayName { get; set; }

        /// <summary>
        /// تعداد نوبت‌ها
        /// </summary>
        public int AppointmentCount { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DailyAppointmentStatsViewModel FromEntity(DateTime date, int count)
        {
            return new DailyAppointmentStatsViewModel
            {
                DayName = GetPersianDayName(date.DayOfWeek),
                AppointmentCount = count
            };
        }

        /// <summary>
        /// دریافت نام روز به فارسی
        /// </summary>
        private static string GetPersianDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Saturday => "شنبه",
                DayOfWeek.Sunday => "یکشنبه",
                DayOfWeek.Monday => "دوشنبه",
                DayOfWeek.Tuesday => "سه‌شنبه",
                DayOfWeek.Wednesday => "چهارشنبه",
                DayOfWeek.Thursday => "پنج‌شنبه",
                DayOfWeek.Friday => "جمعه",
                _ => "نامشخص"
            };
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل داشبورد پزشک
    /// </summary>
    public class DoctorDashboardViewModelValidator : AbstractValidator<DoctorDashboardViewModel>
    {
        public DoctorDashboardViewModelValidator()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است.");

            RuleFor(x => x.DoctorName)
                .NotEmpty()
                .WithMessage("نام پزشک الزامی است.")
                .MaximumLength(200)
                .WithMessage("نام پزشک نمی‌تواند بیش از 200 کاراکتر باشد.");

            RuleFor(x => x.TodayAppointmentsCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد نوبت‌های امروز نمی‌تواند منفی باشد.");

            RuleFor(x => x.TomorrowAppointmentsCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد نوبت‌های فردا نمی‌تواند منفی باشد.");

            RuleFor(x => x.ThisWeekAppointmentsCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد نوبت‌های این هفته نمی‌تواند منفی باشد.");

            RuleFor(x => x.ThisMonthAppointmentsCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد نوبت‌های این ماه نمی‌تواند منفی باشد.");

            RuleFor(x => x.TodayRevenue)
                .GreaterThanOrEqualTo(0)
                .WithMessage("درآمد امروز نمی‌تواند منفی باشد.");

            RuleFor(x => x.ThisWeekRevenue)
                .GreaterThanOrEqualTo(0)
                .WithMessage("درآمد این هفته نمی‌تواند منفی باشد.");

            RuleFor(x => x.ThisMonthRevenue)
                .GreaterThanOrEqualTo(0)
                .WithMessage("درآمد این ماه نمی‌تواند منفی باشد.");

            RuleFor(x => x.LastMonthRevenue)
                .GreaterThanOrEqualTo(0)
                .WithMessage("درآمد ماه گذشته نمی‌تواند منفی باشد.");

            RuleFor(x => x.NewPatientsToday)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد بیماران جدید امروز نمی‌تواند منفی باشد.");

            RuleFor(x => x.NewPatientsThisWeek)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد بیماران جدید این هفته نمی‌تواند منفی باشد.");
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل نوبت امروز
    /// </summary>
    public class TodayAppointmentViewModelValidator : AbstractValidator<TodayAppointmentViewModel>
    {
        public TodayAppointmentViewModelValidator()
        {
            RuleFor(x => x.AppointmentId)
                .GreaterThan(0)
                .WithMessage("شناسه نوبت نامعتبر است.");

            RuleFor(x => x.PatientName)
                .NotEmpty()
                .WithMessage("نام بیمار الزامی است.")
                .MaximumLength(200)
                .WithMessage("نام بیمار نمی‌تواند بیش از 200 کاراکتر باشد.");

            RuleFor(x => x.PatientPhoneNumber)
                .Must(PersianNumberHelper.IsValidPhoneNumber)
                .When(x => !string.IsNullOrEmpty(x.PatientPhoneNumber))
                .WithMessage("شماره تلفن بیمار نامعتبر است.");

            RuleFor(x => x.Status)
                .NotEmpty()
                .WithMessage("وضعیت نوبت الزامی است.");
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل نوبت فردا
    /// </summary>
    public class TomorrowAppointmentViewModelValidator : AbstractValidator<TomorrowAppointmentViewModel>
    {
        public TomorrowAppointmentViewModelValidator()
        {
            RuleFor(x => x.AppointmentId)
                .GreaterThan(0)
                .WithMessage("شناسه نوبت نامعتبر است.");

            RuleFor(x => x.PatientName)
                .NotEmpty()
                .WithMessage("نام بیمار الزامی است.")
                .MaximumLength(200)
                .WithMessage("نام بیمار نمی‌تواند بیش از 200 کاراکتر باشد.");

            RuleFor(x => x.Status)
                .NotEmpty()
                .WithMessage("وضعیت نوبت الزامی است.");
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل آمار دسته‌بندی خدمات
    /// </summary>
    public class ServiceCategoryStatsViewModelValidator : AbstractValidator<ServiceCategoryStatsViewModel>
    {
        public ServiceCategoryStatsViewModelValidator()
        {
            RuleFor(x => x.ServiceCategoryName)
                .NotEmpty()
                .WithMessage("نام دسته‌بندی خدمات الزامی است.")
                .MaximumLength(200)
                .WithMessage("نام دسته‌بندی خدمات نمی‌تواند بیش از 200 کاراکتر باشد.");

            RuleFor(x => x.AppointmentCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد نوبت‌ها نمی‌تواند منفی باشد.");

            RuleFor(x => x.Percentage)
                .InclusiveBetween(0, 100)
                .WithMessage("درصد باید بین 0 تا 100 باشد.");
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل آمار روزانه نوبت‌ها
    /// </summary>
    public class DailyAppointmentStatsViewModelValidator : AbstractValidator<DailyAppointmentStatsViewModel>
    {
        public DailyAppointmentStatsViewModelValidator()
        {
            RuleFor(x => x.DayName)
                .NotEmpty()
                .WithMessage("نام روز الزامی است.")
                .MaximumLength(20)
                .WithMessage("نام روز نمی‌تواند بیش از 20 کاراکتر باشد.");

            RuleFor(x => x.AppointmentCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد نوبت‌ها نمی‌تواند منفی باشد.");
        }
    }
}
