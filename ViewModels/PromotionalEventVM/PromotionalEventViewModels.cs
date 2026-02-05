using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using ClinicApp.Models.Entities.PromotionalEvent;
using ClinicApp.Models.Enums;
using Newtonsoft.Json;
using MvcSelectListItem = System.Web.Mvc.SelectListItem; // ✅ برای رفع ambiguity

namespace ClinicApp.ViewModels.PromotionalEventVM
{
    #region Index ViewModels

    /// <summary>
    /// ViewModel برای نمایش لیست ایونت‌های تبلیغاتی
    /// </summary>
    public class PromotionalEventIndexViewModel
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DiscountType DiscountType { get; set; }
        public string DiscountTypeDisplay { get; set; }
        public decimal DiscountValue { get; set; }
        public int? TotalSlots { get; set; }
        public int UsedSlots { get; set; }
        public string SlotsDisplay => TotalSlots.HasValue ? $"{UsedSlots}/{TotalSlots}" : $"{UsedSlots}/∞";
        public bool IsDoctorSpecific { get; set; }
        public string DoctorNames { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
    }

    /// <summary>
    /// ViewModel برای صفحه Index
    /// </summary>
    public class PromotionalEventIndexPageViewModel
    {
        public List<PromotionalEventIndexViewModel> Events { get; set; } = new List<PromotionalEventIndexViewModel>();
        public PromotionalEventSearchViewModel SearchModel { get; set; } = new PromotionalEventSearchViewModel();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
    }

    /// <summary>
    /// ViewModel برای جستجو
    /// </summary>
    public class PromotionalEventSearchViewModel
    {
        public string SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش عمومی ایونت تبلیغاتی در صفحه اصلی و صفحات بیمار
    /// </summary>
    public class PromotionalEventPublicViewModel
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        /// <summary>تاریخ شروع شمسی برای نمایش</summary>
        public string StartDateDisplay { get; set; }
        /// <summary>تاریخ پایان شمسی برای نمایش</summary>
        public string EndDateDisplay { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        /// <summary>متن نمایشی تخفیف (مثلاً "۲۰٪ تخفیف" یا "۱۰۰,۰۰۰ ریال تخفیف")</summary>
        public string DiscountDisplayText { get; set; }
        /// <summary>آدرس دکمه CTA (رزرو نوبت)</summary>
        public string CtaUrl { get; set; }
        public int? TotalSlots { get; set; }
        public int UsedSlots { get; set; }
        /// <summary>ظرفیت باقی‌مانده برای نمایش (اختیاری)</summary>
        public int? RemainingSlots { get; set; }
    }

    #endregion

    #region Create & Edit ViewModels

    /// <summary>
    /// ViewModel برای ایجاد و ویرایش ایونت تبلیغاتی
    /// </summary>
    public class PromotionalEventCreateEditViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "عنوان ایونت الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان ایونت نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "عنوان ایونت")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "تاریخ شروع الزامی است.")]
        [Display(Name = "تاریخ شروع")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "تاریخ پایان الزامی است.")]
        [Display(Name = "تاریخ پایان")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "نوع تخفیف الزامی است.")]
        [Display(Name = "نوع تخفیف")]
        public DiscountType DiscountType { get; set; }

        [Required(ErrorMessage = "مقدار تخفیف الزامی است.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "مقدار تخفیف باید بیشتر از صفر باشد.")]
        [Display(Name = "مقدار تخفیف")]
        public decimal DiscountValue { get; set; }

        [Display(Name = "تعداد کل نوبت‌ها (اختیاری - برای نامحدود خالی بگذارید)")]
        [Range(1, int.MaxValue, ErrorMessage = "تعداد کل نوبت‌ها باید بیشتر از صفر باشد.")]
        public int? TotalSlots { get; set; }

        [Display(Name = "فقط برای پزشکان خاص")]
        public bool IsDoctorSpecific { get; set; }

        [Display(Name = "انتخاب پزشکان")]
        public List<int> SelectedDoctorIds { get; set; } = new List<int>();

        [Display(Name = "لیست پزشکان موجود")]
        public List<MvcSelectListItem> AvailableDoctors { get; set; } = new List<MvcSelectListItem>();

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }

    #endregion

    #region Details ViewModel

    /// <summary>
    /// ViewModel برای نمایش جزئیات ایونت تبلیغاتی
    /// </summary>
    public class PromotionalEventDetailsViewModel
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DiscountType DiscountType { get; set; }
        public string DiscountTypeDisplay { get; set; }
        public decimal DiscountValue { get; set; }
        public int? TotalSlots { get; set; }
        public int UsedSlots { get; set; }
        public string SlotsDisplay => TotalSlots.HasValue ? $"{UsedSlots}/{TotalSlots}" : $"{UsedSlots}/∞";
        public int RemainingSlots => TotalSlots.HasValue ? (TotalSlots.Value - UsedSlots) : int.MaxValue;
        public bool IsDoctorSpecific { get; set; }
        public List<int> DoctorIds { get; set; } = new List<int>();
        public List<string> DoctorNames { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserName { get; set; }
        public List<PromotionalEventAppointmentViewModel> Appointments { get; set; } = new List<PromotionalEventAppointmentViewModel>();
    }

    /// <summary>
    /// ViewModel برای نمایش نوبت‌های استفاده شده از ایونت
    /// </summary>
    public class PromotionalEventAppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int? PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region Send SMS ViewModels

    /// <summary>
    /// ViewModel برای صفحه تأیید ارسال پیامک (GET SendSms) و فرم ارسال — Strongly-Typed
    /// </summary>
    public class PromotionalEventSendSmsViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public int PatientsWithPhoneCount { get; set; }
        public int NewsletterSubscribersCount { get; set; }
        public int BothCount { get; set; }
        public string WarningMessage { get; set; }
        /// <summary>نوع مخاطب (برای فرم ارسال)</summary>
        public ClinicApp.Models.Enums.PromotionalEventAudience Audience { get; set; } = ClinicApp.Models.Enums.PromotionalEventAudience.PatientsWithPhone;
        /// <summary>متن سفارشی پیامک (اختیاری — حداکثر ۱۶۰ کاراکتر)</summary>
        [MaxLength(160, ErrorMessage = "متن پیامک حداکثر ۱۶۰ کاراکتر است.")]
        public string CustomMessage { get; set; }
    }

    /// <summary>
    /// ViewModel برای فرم ارسال پیامک (POST SendSms)
    /// </summary>
    public class PromotionalEventSendSmsPostViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "انتخاب مخاطب الزامی است.")]
        [Display(Name = "نوع مخاطب")]
        public ClinicApp.Models.Enums.PromotionalEventAudience Audience { get; set; }

        [MaxLength(160, ErrorMessage = "متن پیامک حداکثر ۱۶۰ کاراکتر است.")]
        [Display(Name = "متن سفارشی پیامک")]
        public string CustomMessage { get; set; }
    }

    #endregion

    #region Factory

    /// <summary>
    /// Factory برای ایجاد ViewModels
    /// </summary>
    public static class PromotionalEventViewModelFactory
    {
        /// <summary>
        /// ایجاد ViewModel خالی برای Create
        /// </summary>
        public static PromotionalEventCreateEditViewModel CreateEmpty()
        {
            return new PromotionalEventCreateEditViewModel
            {
                EventId = 0,
                Title = string.Empty,
                Description = string.Empty,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(30),
                DiscountType = DiscountType.Percentage,
                DiscountValue = 0,
                TotalSlots = null,
                IsDoctorSpecific = false,
                SelectedDoctorIds = new List<int>(),
                AvailableDoctors = new List<MvcSelectListItem>(),
                IsActive = true
            };
        }

        /// <summary>
        /// تبدیل Entity به CreateEditViewModel
        /// </summary>
        public static PromotionalEventCreateEditViewModel FromEntity(PromotionalEvent entity)
        {
            if (entity == null)
                return CreateEmpty();

            var selectedDoctorIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(entity.DoctorIds))
            {
                try
                {
                    selectedDoctorIds = JsonConvert.DeserializeObject<List<int>>(entity.DoctorIds) ?? new List<int>();
                }
                catch
                {
                    selectedDoctorIds = new List<int>();
                }
            }

            return new PromotionalEventCreateEditViewModel
            {
                EventId = entity.EventId,
                Title = entity.Title,
                Description = entity.Description,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                DiscountType = entity.DiscountType,
                DiscountValue = entity.DiscountValue,
                TotalSlots = entity.TotalSlots,
                IsDoctorSpecific = entity.IsDoctorSpecific,
                SelectedDoctorIds = selectedDoctorIds,
                AvailableDoctors = new List<MvcSelectListItem>(), // باید در Controller پر شود
                IsActive = entity.IsActive
            };
        }

        /// <summary>
        /// تبدیل CreateEditViewModel به Entity
        /// </summary>
        public static PromotionalEvent ToEntity(PromotionalEventCreateEditViewModel viewModel)
        {
            if (viewModel == null)
                return null;

            var entity = new PromotionalEvent
            {
                EventId = viewModel.EventId,
                Title = viewModel.Title?.Trim() ?? string.Empty,
                Description = viewModel.Description?.Trim(),
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
                DiscountType = viewModel.DiscountType,
                DiscountValue = viewModel.DiscountValue,
                TotalSlots = viewModel.TotalSlots,
                IsDoctorSpecific = viewModel.IsDoctorSpecific,
                IsActive = viewModel.IsActive
            };

            // تبدیل SelectedDoctorIds به JSON
            if (viewModel.IsDoctorSpecific && viewModel.SelectedDoctorIds != null && viewModel.SelectedDoctorIds.Any())
            {
                entity.DoctorIds = JsonConvert.SerializeObject(viewModel.SelectedDoctorIds);
            }
            else
            {
                entity.DoctorIds = null;
            }

            return entity;
        }

        /// <summary>
        /// تبدیل Entity به IndexViewModel
        /// </summary>
        public static PromotionalEventIndexViewModel ToIndexViewModel(PromotionalEvent entity)
        {
            if (entity == null)
                return null;

            return new PromotionalEventIndexViewModel
            {
                EventId = entity.EventId,
                Title = entity.Title,
                Description = entity.Description,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                DiscountType = entity.DiscountType,
                DiscountTypeDisplay = GetDiscountTypeDisplay(entity.DiscountType),
                DiscountValue = entity.DiscountValue,
                TotalSlots = entity.TotalSlots,
                UsedSlots = entity.UsedSlots,
                IsDoctorSpecific = entity.IsDoctorSpecific,
                DoctorNames = string.Empty, // باید در Controller پر شود
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedByUserName = entity.CreatedByUser?.UserName ?? "سیستم"
            };
        }

        /// <summary>
        /// تبدیل Entity به DetailsViewModel
        /// </summary>
        public static PromotionalEventDetailsViewModel ToDetailsViewModel(PromotionalEvent entity)
        {
            if (entity == null)
                return null;

            var doctorIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(entity.DoctorIds))
            {
                try
                {
                    doctorIds = JsonConvert.DeserializeObject<List<int>>(entity.DoctorIds) ?? new List<int>();
                }
                catch
                {
                    doctorIds = new List<int>();
                }
            }

            var appointments = entity.Appointments?.Select(a => new PromotionalEventAppointmentViewModel
            {
                AppointmentId = a.AppointmentId,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor?.FullName ?? "نامشخص",
                PatientId = a.PatientId,
                PatientName = a.Patient?.FullName ?? a.PatientName ?? "نامشخص",
                AppointmentDate = a.AppointmentDate,
                Price = a.Price,
                DiscountAmount = a.DiscountAmount,
                CreatedAt = a.CreatedAt
            }).ToList() ?? new List<PromotionalEventAppointmentViewModel>();

            return new PromotionalEventDetailsViewModel
            {
                EventId = entity.EventId,
                Title = entity.Title,
                Description = entity.Description,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                DiscountType = entity.DiscountType,
                DiscountTypeDisplay = GetDiscountTypeDisplay(entity.DiscountType),
                DiscountValue = entity.DiscountValue,
                TotalSlots = entity.TotalSlots,
                UsedSlots = entity.UsedSlots,
                IsDoctorSpecific = entity.IsDoctorSpecific,
                DoctorIds = doctorIds,
                DoctorNames = new List<string>(), // باید در Controller پر شود
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedByUserName = entity.CreatedByUser?.UserName ?? "سیستم",
                UpdatedAt = entity.UpdatedAt,
                UpdatedByUserName = entity.UpdatedByUser?.UserName,
                Appointments = appointments
            };
        }

        /// <summary>
        /// دریافت نمایش فارسی نوع تخفیف
        /// </summary>
        private static string GetDiscountTypeDisplay(DiscountType discountType)
        {
            return discountType switch
            {
                DiscountType.Percentage => "درصدی",
                DiscountType.FixedAmount => "مبلغ ثابت",
                _ => discountType.ToString()
            };
        }
    }

    #endregion
}

