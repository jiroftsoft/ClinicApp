using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ClinicApp.ViewModels;

/// <summary>
/// ویو مدل لیست بیمه‌ها - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. نمایش اطلاعات ضروری برای لیست بیمه‌ها
/// 2. پشتیبانی از محیط‌های پزشکی ایرانی
/// 3. نمایش سهم‌های بیمه و بیمار به صورت درصد
/// 4. پشتیبانی از وضعیت فعال/غیرفعال بودن بیمه
/// </summary>
public class InsuranceViewModel
{
    public int InsuranceId { get; set; }

    [Display(Name = "نام بیمه")]
    public string Name { get; set; }

    [Display(Name = "سهم پیش‌فرض بیمار")]
    public decimal DefaultPatientShare { get; set; }

    [Display(Name = "سهم پیش‌فرض بیمه")]
    public decimal DefaultInsurerShare { get; set; }

    [Display(Name = "تعداد تعرفه‌ها")]
    public int TariffCount { get; set; }

    [Display(Name = "وضعیت")]
    public string StatusDisplay => IsActive ? "فعال" : "غیرفعال";
    public bool IsActive { get; set; }
    public String Description { get; set; }
}
/// <summary>
/// ویو مدل جزئیات بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. نمایش کامل اطلاعات بیمه
/// 2. پشتیبانی از محیط‌های پزشکی ایرانی
/// 3. نمایش اطلاعات ردیابی (کاربر ایجاد کننده، ویرایش کننده)
/// 4. نمایش تعداد تعرفه‌های مرتبط
/// </summary>
public class InsuranceDetailsViewModel
{
    public int InsuranceId { get; set; }

    [Display(Name = "نام بیمه")]
    public string Name { get; set; }

    [Display(Name = "توضیحات")]
    public string Description { get; set; }

    [Display(Name = "سهم پیش‌فرض بیمار")]
    public decimal DefaultPatientShare { get; set; }

    [Display(Name = "سهم پیش‌فرض بیمه")]
    public decimal DefaultInsurerShare { get; set; }

    [Display(Name = "وضعیت")]
    public bool IsActive { get; set; }

    [Display(Name = "تاریخ ایجاد")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "ایجاد شده توسط")]
    public string CreatedByUser { get; set; }

    [Display(Name = "تاریخ آخرین ویرایش")]
    public DateTime? UpdatedAt { get; set; }

    [Display(Name = "ویرایش شده توسط")]
    public string UpdatedByUser { get; set; }

    [Display(Name = "تعداد تعرفه‌ها")]
    public int TariffCount { get; set; }

    // نمایش به شمسی برای محیط‌های پزشکی ایرانی
    [Display(Name = "تاریخ ایجاد")]
    public string CreatedAtShamsi { get; set; }

    [Display(Name = "تاریخ آخرین ویرایش")]
    public string UpdatedAtShamsi { get; set; }
}
/// <summary>
/// ویو مدل ایجاد بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. اعتبارسنجی‌های کامل برای ایجاد بیمه
/// 2. پشتیبانی از محیط‌های پزشکی ایرانی
/// 3. اعتبارسنجی سهم‌های بیمه و بیمار (باید در مجموع 100% باشند)
/// 4. پشتیبانی از وضعیت فعال/غیرفعال بیمه
/// </summary>
public class CreateInsuranceViewModel
{
    [Display(Name = "نام بیمه")]
    [Required(ErrorMessage = "نام بیمه الزامی است.")]
    [MaxLength(250, ErrorMessage = "نام بیمه نمی‌تواند بیش از 250 کاراکتر باشد.")]
    public string Name { get; set; }

    [Display(Name = "توضیحات")]
    [MaxLength(1000, ErrorMessage = "توضیحات بیمه نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Description { get; set; }

    [Display(Name = "سهم پیش‌فرض بیمار")]
    [Required(ErrorMessage = "سهم پیش‌فرض بیمار الزامی است.")]
    [Range(0, 100, ErrorMessage = "سهم بیمار باید بین 0 تا 100 درصد باشد.")]
    public decimal DefaultPatientShare { get; set; }

    [Display(Name = "سهم پیش‌فرض بیمه")]
    [Required(ErrorMessage = "سهم پیش‌فرض بیمه الزامی است.")]
    [Range(0, 100, ErrorMessage = "سهم بیمه باید بین 0 تا 100 درصد باشد.")]
    public decimal DefaultInsurerShare { get; set; }

    [Display(Name = "فعال باشد؟")]
    public bool IsActive { get; set; } = true;

    // اعتبارسنجی سمت سرور - بررسی مجموع سهم‌ها
    public bool ValidateShares()
    {
        return Math.Abs(DefaultPatientShare + DefaultInsurerShare - 100) < 0.01m;
    }
}
/// <summary>
/// ویو مدل ویرایش بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. شامل شناسه بیمه برای ویرایش
/// 2. اعتبارسنجی‌های کامل برای ویرایش بیمه
/// 3. پشتیبانی از محیط‌های پزشکی ایرانی
/// 4. اعتبارسنجی سهم‌های بیمه و بیمار (باید در مجموع 100% باشند)
/// 5. پشتیبانی از وضعیت فعال/غیرفعال بیمه
/// </summary>
public class EditInsuranceViewModel
{
    public int InsuranceId { get; set; }

    [Display(Name = "نام بیمه")]
    [Required(ErrorMessage = "نام بیمه الزامی است.")]
    [MaxLength(250, ErrorMessage = "نام بیمه نمی‌تواند بیش از 250 کاراکتر باشد.")]
    public string Name { get; set; }

    [Display(Name = "توضیحات")]
    [MaxLength(1000, ErrorMessage = "توضیحات بیمه نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Description { get; set; }

    [Display(Name = "سهم پیش‌فرض بیمار")]
    [Required(ErrorMessage = "سهم پیش‌فرض بیمار الزامی است.")]
    [Range(0, 100, ErrorMessage = "سهم بیمار باید بین 0 تا 100 درصد باشد.")]
    public decimal DefaultPatientShare { get; set; }

    [Display(Name = "سهم پیش‌فرض بیمه")]
    [Required(ErrorMessage = "سهم پیش‌فرض بیمه الزامی است.")]
    [Range(0, 100, ErrorMessage = "سهم بیمه باید بین 0 تا 100 درصد باشد.")]
    public decimal DefaultInsurerShare { get; set; }

    [Display(Name = "فعال باشد؟")]
    public bool IsActive { get; set; }

    // اعتبارسنجی سمت سرور - بررسی مجموع سهم‌ها
    public bool ValidateShares()
    {
        return Math.Abs(DefaultPatientShare + DefaultInsurerShare - 100) < 0.01m;
    }
}
/// <summary>
/// ویو مدل تعرفه بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. نمایش اطلاعات تعرفه بیمه
/// 2. پشتیبانی از محیط‌های پزشکی ایرانی
/// 3. نمایش اطلاعات ردیابی (کاربر ایجاد کننده)
/// 4. نمایش قیمت تعرفه‌ای و سهم‌های بیمه و بیمار
/// </summary>
public class InsuranceTariffViewModel
{
    public int InsuranceTariffId { get; set; }

    [Display(Name = "نام خدمت")]
    public string ServiceTitle { get; set; }

    [Display(Name = "قیمت تعرفه‌ای")]
    public decimal? TariffPrice { get; set; }

    [Display(Name = "سهم بیمار")]
    public decimal? PatientShare { get; set; }

    [Display(Name = "سهم بیمه")]
    public decimal? InsurerShare { get; set; }

    [Display(Name = "تاریخ ایجاد")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "ایجاد شده توسط")]
    public string CreatedByUser { get; set; }

    // نمایش به شمسی برای محیط‌های پزشکی ایرانی
    [Display(Name = "تاریخ ایجاد")]
    public string CreatedAtShamsi { get; set; }

    public int InsuranceId { get; set; }
    public int ServiceId { get; set; }
    public string UpdatedAtShamsi { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedByUser { get; set; }
}
/// <summary>
/// ویو مدل ایجاد تعرفه بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. شامل شناسه بیمه و خدمت
/// 2. اعتبارسنجی‌های کامل برای ایجاد تعرفه
/// 3. پشتیبانی از محیط‌های پزشکی ایرانی
/// 4. اعتبارسنجی سهم‌های بیمه و بیمار (باید در مجموع 100% باشند)
/// </summary>
public class CreateInsuranceTariffViewModel
{
    [Required(ErrorMessage = "بیمه الزامی است.")]
    public int InsuranceId { get; set; }

    [Required(ErrorMessage = "خدمت الزامی است.")]
    public int ServiceId { get; set; }

    [Display(Name = "قیمت تعرفه‌ای")]
    [DataType(DataType.Currency)]
    public decimal? TariffPrice { get; set; }

    [Display(Name = "سهم بیمار")]
    [Range(0, 100, ErrorMessage = "سهم بیمار باید بین 0 تا 100 درصد باشد.")]
    public decimal? PatientShare { get; set; }

    [Display(Name = "سهم بیمه")]
    [Range(0, 100, ErrorMessage = "سهم بیمه باید بین 0 تا 100 درصد باشد.")]
    public decimal? InsurerShare { get; set; }

    // اعتبارسنجی سمت سرور - بررسی مجموع سهم‌ها
    public bool ValidateShares()
    {
        if (!PatientShare.HasValue || !InsurerShare.HasValue)
            return true; // اگر هر دو تعریف نشده باشند، اعتبارسنجی انجام نمی‌شود

        return Math.Abs(PatientShare.Value + InsurerShare.Value - 100) < 0.01m;
    }
}
/// <summary>
/// ویو مدل ویرایش تعرفه بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. شامل شناسه تعرفه بیمه
/// 2. اعتبارسنجی‌های کامل برای ویرایش تعرفه
/// 3. پشتیبانی از محیط‌های پزشکی ایرانی
/// 4. اعتبارسنجی سهم‌های بیمه و بیمار (باید در مجموع 100% باشند)
/// </summary>
public class EditInsuranceTariffViewModel
{
    public int InsuranceTariffId { get; set; }

    [Required(ErrorMessage = "بیمه الزامی است.")]
    public int InsuranceId { get; set; }

    [Required(ErrorMessage = "خدمت الزامی است.")]
    public int ServiceId { get; set; }

    [Display(Name = "قیمت تعرفه‌ای")]
    [DataType(DataType.Currency)]
    public decimal? TariffPrice { get; set; }

    [Display(Name = "سهم بیمار")]
    [Range(0, 100, ErrorMessage = "سهم بیمار باید بین 0 تا 100 درصد باشد.")]
    public decimal? PatientShare { get; set; }

    [Display(Name = "سهم بیمه")]
    [Range(0, 100, ErrorMessage = "سهم بیمه باید بین 0 تا 100 درصد باشد.")]
    public decimal? InsurerShare { get; set; }

    public string ServiceTitle { get; set; }

    // اعتبارسنجی سمت سرور - بررسی مجموع سهم‌ها
    public bool ValidateShares()
    {
        if (!PatientShare.HasValue || !InsurerShare.HasValue)
            return true; // اگر هر دو تعریف نشده باشند، اعتبارسنجی انجام نمی‌شود

        return Math.Abs(PatientShare.Value + InsurerShare.Value - 100) < 0.01m;
    }

}
/// <summary>
/// ویو مدل مدیریت تعرفه‌های بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت تعرفه‌های بیمه برای خدمات مختلف
/// 2. نمایش لیست تعرفه‌های موجود
/// 3. نمایش لیست خدمات قابل افزودن
/// 4. پشتیبانی از محیط‌های پزشکی ایرانی
/// 5. عدم استفاده از AutoMapper برای کنترل کامل بر روی داده‌ها
/// </summary>
public class InsuranceTariffsViewModel
{
    public int InsuranceId { get; set; }
    public string InsuranceName { get; set; }
    public List<InsuranceTariffViewModel> Tariffs { get; set; }
    public List<SelectListItem> AvailableServices { get; set; }
}