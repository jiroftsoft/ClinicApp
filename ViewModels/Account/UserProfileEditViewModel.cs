using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Account
{
    /// <summary>
    /// ViewModel for user self-profile editing
    /// Excludes: NationalCode (immutable), Roles, IsActive (admin-only), PhoneNumber (requires OTP)
    /// 
    /// Single Responsibility: Display and edit user's own profile information
    /// </summary>
    public class UserProfileEditViewModel
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
        public string NationalCode { get; set; } // ✅ Read-only, for display only

        [Display(Name = "ایمیل")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [EmailAddress(ErrorMessage = "فرمت {0} معتبر نیست.")]
        [MaxLength(256, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string Email { get; set; }

        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; } // ✅ Read-only (requires OTP verification to change)

        [Display(Name = "جنسیت")]
        [Required(ErrorMessage = "انتخاب {0} الزامی است.")]
        public Gender Gender { get; set; }

        [Display(Name = "آدرس")]
        [MaxLength(500, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        /// <summary>
        /// ✅ Factory Method: Create ViewModel from Entity
        /// </summary>
        public static UserProfileEditViewModel FromEntity(ApplicationUser user)
        {
            if (user == null) return null;

            return new UserProfileEditViewModel
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                NationalCode = user.NationalCode,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                Address = user.Address
            };
        }
    }
}

