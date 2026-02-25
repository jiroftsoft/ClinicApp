using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Shared;

namespace ClinicApp.ViewModels.UserManagement
{
    #region User Index ViewModel

    /// <summary>
    /// ViewModel برای صفحه لیست کاربران
    /// </summary>
    public class UserIndexViewModel
    {
        public List<UserListItemViewModel> Users { get; set; } = new List<UserListItemViewModel>();
        public UserSearchFilter Filter { get; set; } = new UserSearchFilter();
        public PaginationViewModel PagingInfo { get; set; } = new PaginationViewModel();
        public UserStatisticsViewModel Statistics { get; set; } = new UserStatisticsViewModel();
    }

    /// <summary>
    /// ViewModel برای هر آیتم در لیست کاربران
    /// </summary>
    public class UserListItemViewModel
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string NationalCode { get; set; }
        public string NationalCodeMasked => !string.IsNullOrEmpty(NationalCode) && NationalCode.Length >= 4
            ? $"{NationalCode.Substring(0, 2)}***{NationalCode.Substring(NationalCode.Length - 2)}"
            : NationalCode;
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberMasked => !string.IsNullOrEmpty(PhoneNumber) && PhoneNumber.Length >= 4
            ? $"{PhoneNumber.Substring(0, 2)}***{PhoneNumber.Substring(PhoneNumber.Length - 2)}"
            : PhoneNumber;
        public List<string> Roles { get; set; } = new List<string>();
        public string RolesDisplay => string.Join(", ", Roles);
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtShamsi { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string LastLoginDateShamsi { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedAtShamsi { get; set; }
    }

    /// <summary>
    /// فیلتر جستجوی کاربران
    /// </summary>
    public class UserSearchFilter
    {
        [Display(Name = "جستجو")]
        public string SearchTerm { get; set; }

        [Display(Name = "وضعیت")]
        public bool? IsActive { get; set; }

        [Display(Name = "نقش")]
        public string RoleName { get; set; }

        public List<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// درخواست DataTables برای لیست کاربران (سرور-ساید)
    /// </summary>
    public class UserManagementDataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public DataTablesSearch Search { get; set; } = new DataTablesSearch();
        public List<DataTablesOrder> Order { get; set; } = new List<DataTablesOrder>();
        public string FilterSearchTerm { get; set; }
        public bool? FilterIsActive { get; set; }
        public string FilterRoleName { get; set; }
    }

    public class DataTablesSearch
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class DataTablesOrder
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }

    /// <summary>
    /// خروجی DataTables برای یک ردیف کاربر
    /// </summary>
    public class UserManagementDataTablesRow
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string NationalCodeMasked { get; set; }
        public string Email { get; set; }
        public string PhoneNumberMasked { get; set; }
        public string RolesDisplay { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAtShamsi { get; set; }
        public string ActionsHtml { get; set; }
    }

    #endregion

    #region User Create/Edit ViewModel

    /// <summary>
    /// ViewModel برای فرم ایجاد و ویرایش کاربر
    /// </summary>
    public class UserCreateEditViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "نام")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [MaxLength(100, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [MaxLength(100, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string LastName { get; set; }

        [Display(Name = "کد ملی")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "{0} باید ۱۰ رقم باشد.")]
        public string NationalCode { get; set; }

        [Display(Name = "ایمیل")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [EmailAddress(ErrorMessage = "فرمت {0} معتبر نیست.")]
        [MaxLength(256, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string Email { get; set; }

        [Display(Name = "شماره تلفن")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [Phone(ErrorMessage = "فرمت {0} معتبر نیست.")]
        [MaxLength(20, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "جنسیت")]
        [Required(ErrorMessage = "انتخاب {0} الزامی است.")]
        public Gender Gender { get; set; }

        [Display(Name = "آدرس")]
        [MaxLength(500, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "نقش‌ها")]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        public List<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();

        public bool IsEdit => !string.IsNullOrEmpty(UserId);
    }

    #endregion

    #region User Details ViewModel

    /// <summary>
    /// ViewModel برای صفحه جزئیات کاربر
    /// </summary>
    public class UserDetailsViewModel
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string NationalCode { get; set; }
        public string NationalCodeMasked => !string.IsNullOrEmpty(NationalCode) && NationalCode.Length >= 4
            ? $"{NationalCode.Substring(0, 2)}***{NationalCode.Substring(NationalCode.Length - 2)}"
            : NationalCode;
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberMasked => !string.IsNullOrEmpty(PhoneNumber) && PhoneNumber.Length >= 4
            ? $"{PhoneNumber.Substring(0, 2)}***{PhoneNumber.Substring(PhoneNumber.Length - 2)}"
            : PhoneNumber;
        public Gender Gender { get; set; }
        public string GenderDisplay => Gender == Gender.Male ? "مرد" :
                                      Gender == Gender.Female ? "زن" : "نامشخص";
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public List<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();

        // Audit Trail
        public DateTime CreatedAt { get; set; }
        public string CreatedAtShamsi { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedAtShamsi { get; set; }
        public string UpdatedByUser { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedAtShamsi { get; set; }
        public string DeletedByUser { get; set; }

        // Activity
        public DateTime? LastLoginDate { get; set; }
        public string LastLoginDateShamsi { get; set; }
    }

    #endregion

    #region Role ViewModel

    /// <summary>
    /// ViewModel برای نقش
    /// </summary>
    public class RoleViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string DisplayName { get; set; }
        public bool IsSelected { get; set; }
    }

    #endregion

    #region Statistics ViewModel

    /// <summary>
    /// ViewModel برای آمار کاربران
    /// </summary>
    public class UserStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int DeletedUsers { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();
    }

    #endregion
}

