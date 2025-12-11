using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// نوع منبع ویدیو
    /// طراحی شده برای سیستم مدیریت ویدیو
    /// </summary>
    public enum VideoType : byte
    {
        /// <summary>
        /// ویدیو از YouTube
        /// </summary>
        [Display(Name = "یوتیوب")]
        YouTube = 0,

        /// <summary>
        /// ویدیو از Vimeo
        /// </summary>
        [Display(Name = "ویمئو")]
        Vimeo = 1,

        /// <summary>
        /// ویدیو از آپارات
        /// </summary>
        [Display(Name = "آپارات")]
        Aparat = 2,

        /// <summary>
        /// آپلود مستقیم ویدیو
        /// </summary>
        [Display(Name = "آپلود مستقیم")]
        DirectUpload = 3
    }
}

