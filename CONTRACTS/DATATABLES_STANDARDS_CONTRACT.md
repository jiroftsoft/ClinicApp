# قرارداد استانداردهای DataTables برای پروژه ClinicApp

## مقدمه
این قرارداد الزام‌آور برای تمامی پیاده‌سازی‌های DataTables در پروژه ClinicApp است. تمامی قوانین زیر باید بدون استثنا رعایت شوند.

---

## 1. قانون ساختار JSON Response (JSON Response Structure Rule)

### اصول اجباری:
- **همه پاسخ‌های AJAX برای DataTables باید ساختار JSON صحیح داشته باشند**
- **Property names باید با حروف کوچک باشند**
- **ساختار response باید مطابق استاندارد DataTables باشد**

### ساختار صحیح JSON Response:
```csharp
// ✅ صحیح - ساختار DataTables
return Json(new
{
    draw = request.Draw,                    // شماره درخواست
    recordsTotal = totalRecords,            // کل رکوردها
    recordsFiltered = filteredRecords,      // رکوردهای فیلتر شده
    data = dataTablesData                   // داده‌های جدول
});

// ❌ نادرست - استفاده از DataTablesResponse class
return Json(new DataTablesResponse
{
    Draw = request.Draw,                    // اشتباه: حروف بزرگ
    RecordsTotal = totalRecords,            // اشتباه: حروف بزرگ
    RecordsFiltered = filteredRecords,      // اشتباه: حروف بزرگ
    Data = dataTablesData                   // اشتباه: حروف بزرگ
});
```

### Property Names الزامی:
| Property | نوع | توضیح |
|----------|-----|-------|
| `draw` | int | شماره درخواست از DataTables |
| `recordsTotal` | int | کل تعداد رکوردها در دیتابیس |
| `recordsFiltered` | int | تعداد رکوردهای فیلتر شده |
| `data` | array | آرایه داده‌های جدول |

---

## 2. قانون مدیریت خطا (Error Handling Rule)

### اصول اجباری:
- **همه خطاها باید به درستی مدیریت شوند**
- **در صورت خطا، ساختار JSON باید حفظ شود**
- **خطاها باید لاگ شوند**

### پیاده‌سازی صحیح:
```csharp
// ✅ صحیح - مدیریت خطا
[HttpPost]
public async Task<JsonResult> GetDataTableData(DataTablesRequest request)
{
    try
    {
        // منطق اصلی
        var result = await _service.GetDataAsync(request);
        
        return Json(new
        {
            draw = request.Draw,
            recordsTotal = result.TotalRecords,
            recordsFiltered = result.FilteredRecords,
            data = result.Data
        });
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در DataTables");
        return Json(new
        {
            draw = request.Draw,
            recordsTotal = 0,
            recordsFiltered = 0,
            data = new List<object>(),
            error = $"خطا در دریافت داده‌ها: {ex.Message}"
        });
    }
}
```

### نکات مهم:
1. **Error Response**: در صورت خطا، `data` را به `List<object>()` خالی تنظیم کنید
2. **Logging**: همیشه خطاها را با `_logger.Error()` لاگ کنید
3. **User Message**: پیام خطا را به صورت کاربرپسند ارائه دهید

---

## 3. قانون AntiForgeryToken (AntiForgeryToken Rule)

### اصول اجباری:
- **برای POST requests از AntiForgeryToken استفاده کنید**
- **Token باید در View و JavaScript تنظیم شود**

### پیاده‌سازی در View:
```html
<!-- ✅ صحیح - در View -->
<div class="card-body">
    @Html.AntiForgeryToken()
    <!-- محتوای فرم -->
</div>
```

### پیاده‌سازی در JavaScript:
```javascript
// ✅ صحیح - در JavaScript
ajax: {
    url: urls.refresh,
    type: 'POST',
    data: function(d) {
        // اضافه کردن فیلترها
        d.departmentId = $('#departmentFilter').val();
        d.serviceCategoryId = $('#serviceCategoryFilter').val();
        
        // اضافه کردن AntiForgeryToken
        d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();
    }
}
```

### نکات مهم:
1. **Token Location**: Token باید در همان container فرم قرار گیرد
2. **JavaScript Access**: از `$('input[name="__RequestVerificationToken"]').val()` استفاده کنید
3. **POST Only**: فقط برای POST requests لازم است

---

## 4. قانون Column Configuration (Column Configuration Rule)

### اصول اجباری:
- **Column names باید با property names مطابقت داشته باشند**
- **از computed properties استفاده کنید**
- **Render functions برای عملیات پیچیده**

### پیاده‌سازی صحیح:
```javascript
// ✅ صحیح - Column Configuration
columns: [
    { data: 'doctorName', title: 'نام پزشک' },
    { data: 'departmentName', title: 'دپارتمان' },
    { data: 'serviceCategoryName', title: 'سرفصل خدماتی' },
    { data: 'assignmentDate', title: 'تاریخ انتساب' },
    { data: 'status', title: 'وضعیت' },
    { 
        data: null, 
        title: 'عملیات',
        orderable: false,
        render: function(data, type, row) {
            return `
                <div class="btn-group" role="group">
                    <button type="button" class="btn btn-sm btn-info" onclick="viewDetails(${row.doctorId})">
                        <i class="fas fa-eye"></i> جزئیات
                    </button>
                </div>
            `;
        }
    }
]
```

### نکات مهم:
1. **Property Mapping**: `data` property باید با ViewModel property مطابقت داشته باشد
2. **Computed Properties**: از computed properties در ViewModel استفاده کنید
3. **Render Functions**: برای HTML پیچیده از render functions استفاده کنید

---

## 5. قانون ViewModel Design (ViewModel Design Rule)

### اصول اجباری:
- **ViewModel باید computed properties داشته باشد**
- **از strongly-typed models استفاده کنید**
- **Property names باید با JavaScript مطابقت داشته باشند**

### پیاده‌سازی صحیح:
```csharp
// ✅ صحیح - ViewModel با computed properties
public class DoctorAssignmentListItem
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; }
    public List<DepartmentAssignment> Departments { get; set; } = new List<DepartmentAssignment>();
    public List<ServiceCategoryAssignment> ServiceCategories { get; set; } = new List<ServiceCategoryAssignment>();
    public string Status { get; set; }
    public string AssignmentDate { get; set; }
    public DateTime? CreatedDate { get; set; }

    // Computed properties for DataTables
    public string DepartmentName => Departments?.FirstOrDefault()?.Name ?? "نامشخص";
    public string ServiceCategoryName => ServiceCategories?.FirstOrDefault()?.Name ?? "نامشخص";
    public string StatusBadge => GetStatusBadge(Status);
    public string FormattedDate => CreatedDate?.ToString("yyyy/MM/dd") ?? "نامشخص";
}
```

---

## 6. خطاهای رایج و راه‌حل‌ها

### خطاهای JavaScript:
| خطا | علت | راه‌حل |
|-----|-----|-------|
| `Cannot read properties of undefined (reading 'length')` | ساختار JSON نادرست | استفاده از property names با حروف کوچک |
| `DataTables warning: Ajax error` | مشکل در AntiForgeryToken یا URL | بررسی Token و URL ها |
| `TypeError: Cannot read properties of undefined` | Property names با حروف بزرگ | تغییر به حروف کوچک |

### خطاهای Server-side:
| خطا | علت | راه‌حل |
|-----|-----|-------|
| `CS0117: 'DataTablesResponse' does not contain a definition` | استفاده از class نادرست | استفاده از anonymous object |
| `CS1061: does not contain a definition` | Property missing در ViewModel | اضافه کردن property مورد نیاز |

---

## 7. چک‌لیست پیاده‌سازی

### قبل از شروع:
- [ ] ViewModel با computed properties طراحی شده
- [ ] Controller action با try-catch پیاده‌سازی شده
- [ ] AntiForgeryToken در View تنظیم شده
- [ ] JavaScript با data-* attributes تنظیم شده

### بعد از پیاده‌سازی:
- [ ] JSON response با حروف کوچک تست شده
- [ ] Error handling تست شده
- [ ] AntiForgeryToken تست شده
- [ ] Column mapping تست شده
- [ ] Performance تست شده

---

## 8. مراجع و منابع

### فایل‌های مرجع:
- [DoctorAssignmentController.cs](../Areas/Admin/Controllers/DoctorAssignmentController.cs)
- [doctor-assignment-index.js](../Scripts/app/doctor-assignment-index.js)
- [DoctorAssignmentIndexViewModel.cs](../ViewModels/DoctorManagementVM/DoctorAssignmentIndexViewModel.cs)

### مستندات خارجی:
- [DataTables Server-side Processing](https://datatables.net/manual/server-side)
- [DataTables AJAX Data](https://datatables.net/reference/option/ajax.data)
- [DataTables Column Configuration](https://datatables.net/reference/option/columns)

---

## 9. مثال کامل

### Controller:
```csharp
[HttpPost]
public async Task<JsonResult> GetAssignmentsData(DataTablesRequest request)
{
    try
    {
        var result = await _doctorAssignmentService.GetAssignmentsForDataTableAsync(
            request.Start, request.Length, request.Search?.Value,
            request.Columns[1]?.Search?.Value, // departmentId
            request.Columns[2]?.Search?.Value, // serviceCategoryId
            "", "");

        if (!result.Success)
        {
            return Json(new
            {
                draw = request.Draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new List<object>()
            });
        }

        var dataTablesData = result.Data.Data.Cast<DoctorAssignmentListItem>().Select(assignment => new
        {
            doctorName = assignment.DoctorName ?? "نامشخص",
            departmentName = assignment.DepartmentName,
            serviceCategoryName = assignment.ServiceCategoryName,
            assignmentDate = assignment.AssignmentDate ?? "نامشخص",
            status = assignment.StatusBadge,
            doctorId = assignment.DoctorId
        }).ToList();

        return Json(new
        {
            draw = request.Draw,
            recordsTotal = result.Data.RecordsTotal,
            recordsFiltered = result.Data.RecordsFiltered,
            data = dataTablesData
        });
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "خطا در دریافت داده‌های انتسابات برای DataTables");
        return Json(new
        {
            draw = request.Draw,
            recordsTotal = 0,
            recordsFiltered = 0,
            data = new List<object>(),
            error = $"خطا در دریافت داده‌ها: {ex.Message}"
        });
    }
}
```

### JavaScript:
```javascript
$('#assignmentsTable').DataTable({
    language: {
        url: '/Content/plugins/DataTables/Persian.json'
    },
    processing: true,
    serverSide: true,
    ajax: {
        url: urls.refresh,
        type: 'POST',
        data: function(d) {
            d.departmentId = $('#departmentFilter').val();
            d.serviceCategoryId = $('#serviceCategoryFilter').val();
            d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();
        },
        error: function(xhr, error, thrown) {
            console.error('DataTables AJAX Error:', error, thrown);
        }
    },
    columns: [
        { data: 'doctorName', title: 'نام پزشک' },
        { data: 'departmentName', title: 'دپارتمان' },
        { data: 'serviceCategoryName', title: 'سرفصل خدماتی' },
        { data: 'assignmentDate', title: 'تاریخ انتساب' },
        { data: 'status', title: 'وضعیت' },
        { 
            data: null, 
            title: 'عملیات',
            orderable: false,
            render: function(data, type, row) {
                return `<button onclick="viewDetails(${row.doctorId})">جزئیات</button>`;
            }
        }
    ]
});
```

---

**تاریخ آخرین بروزرسانی**: 2024-01-15  
**نسخه**: 1.0  
**وضعیت**: فعال و الزامی
