using System;

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
    public object Value { get; set; }

    /// <summary>
    /// Text برای SelectList (نام آیتم)
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Constructor پیش‌فرض
    /// </summary>
    public LookupItemViewModel()
    {
    }

    /// <summary>
    /// Constructor با پارامترهای اصلی
    /// </summary>
    public LookupItemViewModel(int id, string name, string title = null, string code = null, string description = null)
    {
        Id = id;
        Name = name;
        Title = title;
        Code = code;
        Description = description;
        Value = id;
        Text = name;
    }

    /// <summary>
    /// Constructor با Value سفارشی
    /// </summary>
    public LookupItemViewModel(object value, string text, string name = null, string title = null, string code = null, string description = null)
    {
        Value = value;
        Text = text;
        Name = name ?? text;
        Title = title;
        Code = code;
        Description = description;
        Id = value is int intValue ? intValue : 0;
    }

    /// <summary>
    /// Factory Method برای ایجاد از Entity
    /// </summary>
    public static LookupItemViewModel FromEntity<T>(T entity, Func<T, int> idSelector, Func<T, string> nameSelector, Func<T, string> titleSelector = null, Func<T, string> codeSelector = null, Func<T, string> descriptionSelector = null) where T : class
    {
        if (entity == null) return null;
        
        return new LookupItemViewModel
        {
            Id = idSelector(entity),
            Name = nameSelector(entity),
            Title = titleSelector?.Invoke(entity),
            Code = codeSelector?.Invoke(entity),
            Description = descriptionSelector?.Invoke(entity),
            Value = idSelector(entity),
            Text = nameSelector(entity)
        };
    }

    /// <summary>
    /// Factory Method برای ایجاد با Value سفارشی
    /// </summary>
    public static LookupItemViewModel Create(object value, string text, string name = null, string title = null, string code = null, string description = null)
    {
        return new LookupItemViewModel(value, text, name, title, code, description);
    }
}