namespace ClinicApp.ViewModels;

/// <summary>
/// ویومدل پایه برای آیتم‌های لیست در کامبو باکس‌ها
/// این مدل برای نمایش آیتم‌های لیست در کامبو باکس‌ها و سلکت‌ها استفاده می‌شود
/// </summary>
public class LookupItemViewModel
{
    /// <summary>
    /// شناسه آیتم
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// متن نمایشی آیتم
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// عنوان اصلی آیتم
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// کد آیتم
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// توضیحات اضافی آیتم
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Value برای SelectList (شناسه آیتم)
    /// </summary>
    public int Value => Id;

    /// <summary>
    /// Text برای SelectList (نام آیتم)
    /// </summary>
    public string Text => Name;
}