# راهنمای اجرایی: تبدیل جدول به DataTables (سطح Production HIS/Clinic)

این سند مرجع رسمی برای **تبدیل هر جدول به DataTables با تنظیمات حرفه‌ای** در پروژه ClinicApp (MVC5 + EF6 + Unity) است. هر زمان گفته شد «جدول X را به DataTable تغییر بده»، این راهنما و نمونهٔ UserManagement مبنای کار قرار گیرد.

---

## اهداف سه‌گانه (Production Medical Level)

| محور | هدف | تنظیمات کلیدی |
|------|-----|----------------|
| **۱. سرعت (Performance)** | عدم لود هزاران رکورد در مرورگر، رندر سبک | `serverSide: true`, `deferRender: true`, `processing: true`, `pageLength: 25`, `searchDelay: 500` |
| **۲. پایداری و مقیاس‌پذیری (Scalability)** | پردازش در سرور، کوئری بهینه | API سرور با `Skip`/`Take`، بدون `ToList()` قبل از صفحه‌بندی |
| **۳. تجربه کاربری منشی (UX Speed)** | بازگشت به همان صفحه/فیلتر، بدون رفرش کل صفحه | `stateSave: true`, `stateSaveParams`/`stateLoadParams`, `ajax.reload(null, false)` |

---

## چک‌لیست پیاده‌سازی (اجرایی)

### مرحله ۱: بک‌اند (C#)

#### ۱.۱ مدل درخواست (ViewModel)

در فایل ViewModels مربوط به همان ماژول (یا یک فایل مشترک) اضافه کن:

```csharp
/// <summary>
/// درخواست DataTables برای [نام ماژول] (سرور-ساید)
/// </summary>
public class [ModuleName]DataTablesRequest
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
    public DataTablesSearch Search { get; set; } = new DataTablesSearch();
    public List<DataTablesOrder> Order { get; set; } = new List<DataTablesOrder>();
    // فیلترهای اختصاصی صفحه (مثال)
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
```

- اگر همین کلاس‌ها در پروژه وجود دارند (مثلاً در `UserManagementViewModels.cs`) از همان استفاده کن و فقط یک کلاس Request جدید برای ماژول جدید بساز (با فیلدهای فیلتر همان صفحه).

#### ۱.۲ سرویس (Service)

- متدی اضافه کن که **فقط یک صفحه** از داده را برگرداند و **تعداد کل و تعداد فیلترشده** را هم بدهد.
- امضای پیشنهادی:

```csharp
Task<ServiceResult<(int RecordsTotal, int RecordsFiltered, List<TItemViewModel> Data)>>
    Get[Entity]ForDataTablesAsync([FilterType] filter, int start, int length);
```

- **مهم:** داخل سرویس/ریپازیتوری **قبل از Skip/Take** از دیتابیس فقط Count و همان صفحه را بگیر (بدون `ToList()` روی کل دیتاست). از متدهای موجود صفحه‌بندی‌دار استفاده کن (مثل `GetUsersAsync` با `pageNumber` و `pageSize`).

#### ۱.۳ کنترلر (Controller)

- یک اکشن `[HttpPost]` با نام مثلاً `Get[Entity]Data` یا `GetUsersData`:
  - پارامتر: مدل درخواست DataTables (با فیلترهای اختصاصی).
  - ساخت فیلتر از روی Request (ترکیب `FilterX` و در صورت تمایل `Search.Value`).
  - فراخوانی متد سرویس با `request.Start`, `request.Length`.
  - لاگ درخواست (Draw, Start, Length، خلاصه فیلترها **بدون داده حساس**).
  - خروجی JSON به فرمت ثابت DataTables:

```csharp
return Json(new
{
    draw = request.Draw,
    recordsTotal = recordsTotal,
    recordsFiltered = recordsFiltered,
    data = data  // لیست آبجکت‌های ردیف (با نام propertyهای camelCase برای JS)
});
```

- در صورت خطا:

```csharp
return Json(new
{
    draw = request?.Draw ?? 0,
    recordsTotal = 0,
    recordsFiltered = 0,
    data = new object[0],
    error = "پیام خطا"
});
```

- **امنیت:**  
  - `[ValidateAntiForgeryToken]` روی اکشن.  
  - در AJAX حتماً `__RequestVerificationToken` ارسال شود.  
  - `[Authorize]` / بررسی نقش مطابق ماژول.

- **ردیف‌های هایلایت (اختیاری):** در آبجکت هر ردیف می‌توانی فیلدهای کمکی مثل `hasDoctorRole` بگذاری تا در `rowCallback` سمت کلاینت از آن استفاده شود.

#### ۱.۴ دکمه‌های عملیات (ستون Actions)

- ستون عملیات را در سرور به صورت HTML آماده کن (مثلاً با متد `BuildActionsHtml`) و در آبجکت ردیف به عنوان مثلاً `actionsHtml` برگردان تا در DataTables با `render` یا مستقیم استفاده شود.
- از `data-*` برای شناسه ردیف و رویدادهای حذف/فعال/غیرفعال استفاده کن تا در `drawCallback` با یک اسکریپت مشترک (مثل `UserManagement.init()`) به دکمه‌ها وصل شوند.

---

### مرحله ۲: فرانت‌اند (View + JavaScript)

#### ۲.۱ ساختار HTML جدول

- جدول فقط **یک بار** با `<thead>` و ستون‌های صحیح تعریف شود؛ **بدون ردیف در `<tbody>`** (خالی بماند).

```html
@Html.AntiForgeryToken()
<div class="table-responsive">
    <table id="[tableId]" class="table [table-class] table-striped" style="width:100%">
        <thead>
            <tr>
                <th>ستون۱</th>
                <th>ستون۲</th>
                <th class="no-sort">عملیات</th>
            </tr>
        </thead>
        <tbody></tbody>
    </table>
</div>
```

- مسیر اسکریپت و استایل DataTables در این پروژه:
  - CSS: `~/Content/js/plugins/DataTables/css/dataTables.bootstrap4.min.css`
  - JS: `~/Content/js/plugins/DataTables/js/jquery.dataTables.min.js`, `dataTables.bootstrap4.min.js`

#### ۲.۲ CSS تولید (Loading overlay)

در همان View داخل `@section Styles`:

```css
.dataTables_wrapper .dataTables_processing {
    z-index: 10000;
    border: 1px solid var(--user-primary, #2c5aa0);
    border-radius: 8px;
    box-shadow: 0 2px 12px rgba(44, 90, 160, 0.2);
}
```

#### ۲.۳ کانفیگ استاندارد DataTables (Production HIS)

در `@section Scripts` از این قالب استفاده کن و فقط `url`, `columns`, و در صورت نیاز `rowCallback` و فیلترها را با ماژول خودت عوض کن:

```javascript
$(function() {
    var getDataUrl = '@Url.Action("Get[Entity]Data", "[Controller]", new { area = "Admin" })';

    var dataTable = $('#[tableId]').DataTable({
        processing: true,
        serverSide: true,
        deferRender: true,

        ajax: {
            url: getDataUrl,
            type: 'POST',
            data: function(d) {
                d.FilterSearchTerm = $('#filterSearchTerm').val();
                d.FilterIsActive = $('#filterIsActive').val() || null;
                d.FilterRoleName = $('#filterRoleName').val() || null;
                d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();
            }
        },

        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],

        searching: true,
        ordering: true,
        searchDelay: 500,

        order: [[0, 'desc']],  // ستون و جهت مرتب‌سازی پیش‌فرض

        stateSave: true,
        stateSaveParams: function(settings, data) {
            data.filterSearchTerm = $('#filterSearchTerm').val();
            data.filterIsActive = $('#filterIsActive').val();
            data.filterRoleName = $('#filterRoleName').val();
        },
        stateLoadParams: function(settings, data) {
            if (data.filterSearchTerm) $('#filterSearchTerm').val(data.filterSearchTerm);
            if (data.filterIsActive !== undefined) $('#filterIsActive').val(data.filterIsActive);
            if (data.filterRoleName) $('#filterRoleName').val(data.filterRoleName);
        },

        responsive: true,
        autoWidth: false,

        language: {
            url: '/Content/js/plugins/DataTables/js/fa.json',
            emptyTable: 'هیچ موردی یافت نشد',
            processing: 'در حال بارگذاری...',
            zeroRecords: 'نتیجه‌ای یافت نشد'
        },

        dom: "<'row'<'col-md-6'l><'col-md-6'f>>" +
             "<'row'<'col-md-12'tr>>" +
             "<'row'<'col-md-5'i><'col-md-7'p>>",

        columns: [
            { data: 'field1', render: function(v) { return v || ''; } },
            { data: 'field2' },
            { data: 'actionsHtml', orderable: false, searchable: false, className: 'text-nowrap', render: function(v) { return v || ''; } }
        ],

        rowCallback: function(row, data) {
            if (data && data.isActive === false) $(row).addClass('table-danger');
            if (data && data.hasDoctorRole === true) $(row).addClass('table-info');
        },

        drawCallback: function() {
            // اتصال دکمه‌های عملیات (حذف، فعال/غیرفعال و ...)
            if (typeof window.[ModuleNamespace] !== 'undefined') {
                window.[ModuleNamespace].init();
            }
        }
    });

    $('#searchForm').on('submit', function(e) {
        e.preventDefault();
        dataTable.ajax.reload(null, false);
    });

    $('.btn-reset-filters').on('click', function(e) {
        e.preventDefault();
        $('#filterSearchTerm').val('');
        $('#filterIsActive').val('');
        $('#filterRoleName').val('');
        dataTable.state.clear();
        dataTable.ajax.reload(null, false);
    });

    $(document).on('click', '.clear-search', function() {
        $('#filterSearchTerm').val('');
        dataTable.ajax.reload(null, false);
    });
});
```

- **توضیح کوتاه:**
  - `l`: length (تعداد در صفحه)، `f`: جستجوی پیش‌فرض DataTables، `t`: جدول، `i`: اطلاعات، `p`: صفحه‌بندی.
  - `ajax.reload(null, false)`: رفرش بدون رفرش کل صفحه و بدون ریست صفحه (مناسب UX منشی).
  - برای «بازنشانی فیلترها» قبل از reload یک بار `state.clear()` صدا بزن تا state ذخیره‌شده فیلترها هم پاک شود.

---

## خلاصه تنظیمات ثابت (Production)

این موارد در همه جداول DataTables این پروژه یکسان باشند:

| گزینه | مقدار | دلیل |
|--------|------|------|
| `serverSide` | `true` | پردازش در سرور؛ ضروری برای دیتاست بزرگ |
| `deferRender` | `true` | رندر سریع‌تر |
| `processing` | `true` | نمایش حالت بارگذاری |
| `pageLength` | `25` | تعادل سرعت و دید |
| `lengthMenu` | `[10, 25, 50, 100]` | انتخاب استاندارد |
| `searchDelay` | `500` | کاهش تعداد درخواست هنگام تایپ |
| `stateSave` | `true` | حفظ وضعیت برای منشی |
| `responsive` | `true` | مانیتورهای مختلف |
| `autoWidth` | `false` | جلوگیری از lag |
| `dom` | چیدمان بالا | یکسان‌سازی ظاهر |

---

## امنیت (محیط درمانی)

- **Anti-Forgery:** در هر درخواست POST جدول، `__RequestVerificationToken` در تابع `data` ارسال شود و اکشن با `[ValidateAntiForgeryToken]` محافظت شود.
- **Authorization:** اکشن DataTables مثل بقیه اکشن‌های ادمین با `[Authorize(Roles = ...)]` محدود شود.
- **لاگ:** برای هر درخواست DataTables حداقل Draw, Start, Length و خلاصه فیلترها (بدون مقادیر حساس مثل کد ملی کامل) در لاگ ثبت شود.

---

## اشتباهات رایج (اجتناب)

- استفاده از **Client-Side** برای دیتای بزرگ.
- **Include/ThenInclude** زیاد در EF که یک کوئری سنگین بسازد؛ فقط فیلدهای لازم را بیاور.
- **ToList()** قبل از `Skip`/`Take` روی کل دیتاست.
- رندر **HTML خیلی سنگین** در ستون‌ها؛ ترجیحاً ستون عملیات را در سرور با یک متد ساده مثل `BuildActionsHtml` بساز.
- **pageLength** پیش‌فرض ۱۰۰ یا بیشتر.
- فراموش کردن **stateSaveParams** / **stateLoadParams** وقتی فیلترهای سفارشی (مثل جستجو، وضعیت، نقش) داریم.

---

## نمونهٔ مرجع در پروژه

- **کنترلر:** `Areas/Admin/Controllers/UserManagementController.cs` → منطقه `DataTables API`، متد `GetUsersData`.
- **سرویس:** `Services/UserManagement/UserManagementService.cs` → `GetUsersForDataTablesAsync`.
- **ViewModel:** `ViewModels/UserManagement/UserManagementViewModels.cs` → `UserManagementDataTablesRequest`, `DataTablesSearch`, `DataTablesOrder`.
- **View:** `Areas/Admin/Views/UserManagement/Index.cshtml` → جدول `#usersTable`، اسکریپت DataTables و فرم فیلتر.

هر زمان گفته شد «این جدول را به DataTable تغییر بده»، با این سند و نمونهٔ UserManagement به‌صورت حرفه‌ای و یکسان پیاده‌سازی شود.
