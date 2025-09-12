using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Security.Claims;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Notification;

namespace ClinicApp.Models.Core;

/// <summary>
/// مدل کاربران سیستم - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 2. مدیریت صحیح فیلدهای ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
/// 3. پشتیبانی از سیستم احراز هویت ASP.NET Identity
/// 4. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
/// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
/// </summary>
public class ApplicationUser : IdentityUser, ISoftDelete, ITrackable
{
    /// <summary>
    /// نام کاربر
    /// </summary>
    [Required(ErrorMessage = "نام الزامی است.")]
    [MaxLength(100, ErrorMessage = "نام نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string FirstName { get; set; }

    /// <summary>
    /// نام خانوادگی کاربر
    /// </summary>
    [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
    [MaxLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string LastName { get; set; }
    [Required]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد.")]
    [Index(IsUnique = true)]
    public string NationalCode { get; set; }

    // ✅ اضافه شده: شماره تلفن همراه
    [Required(ErrorMessage = "شماره تلفن همراه الزامی است.")]
    [StringLength(20, ErrorMessage = "شماره تلفن همراه نمی‌تواند بیش از 20 کاراکتر باشد.")]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// نام کامل کاربر
    /// این ویژگی برای نمایش در UI بسیار مفید است
    /// </summary>
    [NotMapped]
    public string FullName
    {
        get
        {
            return $"{FirstName} {LastName}".Trim();
        }
    }

    /// <summary>
    /// آیا کاربر فعال است؟
    /// این فیلد برای غیرفعال کردن حساب‌های کاربری قدیمی یا مسدود شده استفاده می‌شود
    /// </summary>
    [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
    public bool IsActive { get; set; } = true;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن کاربر
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف کاربر
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که کاربر را حذف کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر حذف کننده ضروری است
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد کاربر
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که کاربر را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش کاربر
    /// این اطلاعات برای ردیابی تغییرات در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که آخرین ویرایش را انجام داده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ویرایش کننده ضروری است
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// لیست بیماران مرتبط با این کاربر
    /// این لیست برای نمایش تمام بیمارانی که با این حساب کاربری مرتبط هستند استفاده می‌شود
    /// </summary>
    public virtual ICollection<Patient> Patients { get; set; } = new HashSet<Patient>();
    public virtual ICollection<NotificationHistory> NotificationHistories { get; set; }
    /// <summary>
    /// تاریخ آخرین ورود بیمار به سیستم
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// جنسیت بیمار
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    [Required(ErrorMessage = "جنسیت الزامی است.")]
    public Gender Gender { get; set; }

    /// <summary>
    /// آدرس بیمار
    /// </summary>
    [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Address { get; set; }

    #endregion

    /// <summary>
    /// تولید هویت ادعا مبتنی بر کاربر
    /// این متد برای افزودن ادعاهای سفارشی به هویت کاربر استفاده می‌شود
    /// </summary>
    public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
    {
        var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

        // افزودن ادعاهای سفارشی
        userIdentity.AddClaim(new Claim("FullName", this.FullName ?? ""));
        userIdentity.AddClaim(new Claim("UserId", this.Id));
        userIdentity.AddClaim(new Claim("IsActive", this.IsActive.ToString()));

        return userIdentity;
    }
}


