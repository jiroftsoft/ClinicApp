using AutoMapper;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;

namespace ClinicApp
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // نقشه برای لیست بیماران
            CreateMap<Patient, PatientIndexViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName));

            // نقشه برای نمایش جزئیات (با تبدیل تاریخ به رشته)
            CreateMap<Patient, PatientDetailsViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate.HasValue ? src.BirthDate.Value.ToShortDateString() : "-"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToShortDateString()));

            // نقشه برای فرم ایجاد و ویرایش (دو طرفه)
            CreateMap<Patient, PatientCreateEditViewModel>().ReverseMap();

            // نقشه برای ثبت‌نام بیمار جدید
            CreateMap<RegisterPatientViewModel, Patient>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationUserId, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<Clinic, ClinicIndexViewModel>()
                .ForMember(dest => dest.DepartmentCount, opt => opt.MapFrom(src => src.Departments.Count))
                .ForMember(dest => dest.DoctorCount, opt => opt.MapFrom(src => src.Doctors.Count))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted));

            // For converting a Clinic entity to the Details ViewModel
            CreateMap<Clinic, ClinicDetailsViewModel>();

            // For two-way mapping between the Create/Edit ViewModel and the Clinic entity
            CreateMap<ClinicCreateEditViewModel, Clinic>(); // The specific map that was missing
            CreateMap<Clinic, ClinicCreateEditViewModel>(); // The reverse map for the Edit GET action
            //===================================================
            ConfigureDepartmentMappings();
        }
        /// <summary>
        /// Configures all mappings related to the Department entity.
        /// </summary>
        private void ConfigureDepartmentMappings()
        {
            // تبدیل از موجودیت دپارتمان به مدل ویو جزئیات
            CreateMap<Department, DepartmentDetailsViewModel>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src =>
                    GetUserNameFromUser(src.CreatedByUser)))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src =>
                    GetUserNameFromUser(src.UpdatedByUser)))
                .ForMember(dest => dest.DeletedBy, opt => opt.MapFrom(src =>
                    GetUserNameFromUser(src.DeletedByUser)))
                .ForMember(dest => dest.DoctorCount, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceCount, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceCategories, opt => opt.Ignore())
                .ReverseMap();

            // تبدیل از موجودیت دپارتمان به مدل ویو لیست
            CreateMap<Department, DepartmentIndexViewModel>()
                .ForMember(dest => dest.DoctorCount, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => ToPersianDateString(src.CreatedAt)))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src =>
                    GetUserNameFromUser(src.CreatedByUser)))
                .ForMember(dest => dest.ClinicName, opt => opt.MapFrom(src => src.Clinic.Name));
            // تبدیل از موجودیت به ViewModel (برای نمایش فرم‌ها)
            CreateMap<Department, DepartmentCreateEditViewModel>()
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ClinicId, opt => opt.MapFrom(src => src.ClinicId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                // افزودن اطلاعات اضافی برای سیستم‌های پزشکی
                .ForMember(dest => dest.ClinicName, opt => opt.MapFrom(src => src.Clinic.Name))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedByUser != null ?
                    $"{src.CreatedByUser.FirstName} {src.CreatedByUser.LastName}" : "سیستم"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedByUser != null ?
                    $"{src.UpdatedByUser.FirstName} {src.UpdatedByUser.LastName}" : ""))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.DeletedBy, opt => opt.MapFrom(src => src.DeletedByUser != null ?
                    $"{src.DeletedByUser.FirstName} {src.DeletedByUser.LastName}" : ""))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
                .ForMember(dest => dest.DoctorCount, opt => opt.MapFrom(src =>
                    src.Doctors != null ? src.Doctors.Count(d => !d.IsDeleted) : 0))
                .ForMember(dest => dest.ServiceCount, opt => opt.MapFrom(src =>
                    src.ServiceCategories != null ? src.ServiceCategories.Sum(sc => sc.Services.Count(s => !s.IsDeleted)) : 0))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted && src.IsActive))
                .AfterMap((src, dest) =>
                {
                    // تبدیل تاریخ میلادی به شمسی برای محیط‌های پزشکی ایرانی
                    dest.CreatedAtShamsi = src.CreatedAt.ToPersianDateTime();
                    if (src.UpdatedAt.HasValue)
                        dest.UpdatedAtShamsi = src.UpdatedAt.Value.ToPersianDateTime();
                    if (src.DeletedAt.HasValue)
                        dest.DeletedAtShamsi = src.DeletedAt.Value.ToPersianDateTime();
                });
            // تبدیل از ViewModel به موجودیت (برای پردازش درخواست‌ها)
            CreateMap<DepartmentCreateEditViewModel, Department>()
                .ForMember(dest => dest.DepartmentId, opt => opt.Condition(src => src.DepartmentId > 0))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ClinicId, opt => opt.MapFrom(src => src.ClinicId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                // مدیریت صحیح فیلدهای ردیابی برای سیستم‌های پزشکی
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Clinic, opt => opt.Ignore())
                .ForMember(dest => dest.Doctors, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceCategories, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedByUser, opt => opt.Ignore())
                .AfterMap((src, dest, context) =>
                {
                    // راه‌حل سازگار با Unity DI
                    if (context.Items.TryGetValue("CurrentUser", out var currentUserObj) &&
                        currentUserObj is ICurrentUserService currentUserService)
                    {
                        // تنظیم فیلدهای ردیابی بر اساس عملیات (ایجاد یا ویرایش)
                        if (dest.DepartmentId == 0) // عملیات ایجاد
                        {
                            dest.CreatedAt = currentUserService.UtcNow;
                            dest.CreatedByUserId = currentUserService.UserId;
                            dest.IsActive = true;
                        }
                        else // عملیات ویرایش
                        {
                            dest.UpdatedAt = currentUserService.UtcNow;
                            dest.UpdatedByUserId = currentUserService.UserId;
                        }
                    }
                    else
                    {
                        // حالت پیش‌فرض در صورت عدم دسترسی به سرویس کاربر فعلی
                        var currentUserId = "System";
                        var currentDateTime = DateTime.UtcNow;

                        if (dest.DepartmentId == 0)
                        {
                            dest.CreatedAt = currentDateTime;
                            dest.CreatedByUserId = currentUserId;
                            dest.IsActive = true;
                        }
                        else
                        {
                            dest.UpdatedAt = currentDateTime;
                            dest.UpdatedByUserId = currentUserId;
                        }
                    }

                    // رعایت استانداردهای پزشکی: حذف فیزیکی مجاز نیست
                    dest.IsDeleted = false;
                    dest.DeletedAt = null;
                    dest.DeletedByUserId = null;
                });
        }

            // --- Department Mappings ---
            //CreateMap<Department, DepartmentIndexViewModel>()
            //    .ForMember(dest => dest.ClinicName, opt => opt.MapFrom(src => src.Clinic.Name))
            //    .ForMember(dest => dest.ServiceCount, opt => opt.MapFrom(src => src.ServiceCategories.SelectMany(sc => sc.Services).Count(s => !s.IsDeleted)))
            //    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsDeleted));
            //CreateMap<DepartmentCreateEditViewModel, Department>().ReverseMap();
        


        
        private string ToPersianDateString(DateTime? date)
        {
            if (!date.HasValue) return "-";
            var pc = new PersianCalendar();
            return $"{pc.GetYear(date.Value)}/{pc.GetMonth(date.Value):D2}/{pc.GetDayOfMonth(date.Value):D2}";
        }

        private string ToPersianDateString(DateTime date)
        {
            var pc = new PersianCalendar();
            return $"{pc.GetYear(date)}/{pc.GetMonth(date):D2}/{pc.GetDayOfMonth(date):D2}";
        }
        /// <summary>
        /// تبدیل اعداد فارسی به انگلیسی
        /// </summary>
        private string ToEnglishNumbers(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            var sb = new System.Text.StringBuilder();
            foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    // تبدیل اعداد فارسی/عربی به انگلیسی
                    if (c >= '۰' && c <= '۹')
                        sb.Append((char)(c - '۰' + '0'));
                    else if (c >= '٠' && c <= '٩')
                        sb.Append((char)(c - '٠' + '0'));
                    else
                        sb.Append(c);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// دریافت نام کاربر از شیء ApplicationUser
        /// این متد برای تبدیل شیء کاربر به نام کاربر در سیستم‌های پزشکی استفاده می‌شود
        /// </summary>
        /// <param name="user">شیء کاربر</param>
        /// <returns>نام کامل کاربر یا متن پیش‌فرض در صورت عدم وجود</returns>
        private string GetUserNameFromUser(ApplicationUser user)
        {
            if (user == null)
                return "نام کاربر حذف شده یا نامشخص";

            return $"{user.FirstName} {user.LastName}".Trim();
        }
    }
}