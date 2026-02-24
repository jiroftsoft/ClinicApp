namespace ClinicApp.Models.DTOs.Appointment
{
    /// <summary>
    /// DTO برای نمایش دسته‌بندی خدمت در لیست کشویی (انتخاب نوع ویزیت توسط بیمار)
    /// </summary>
    public class ServiceCategoryLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
