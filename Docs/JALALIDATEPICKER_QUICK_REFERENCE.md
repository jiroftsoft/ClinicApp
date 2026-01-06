# ⚡ JalaliDatePicker Enterprise - Quick Reference

**نسخه:** 2.0.0  
**برای:** Cursor AI - Quick Migration

---

## 🔄 **Quick Migration Steps**

### **1. View (Razor) - Input Change**

```html
<!-- ❌ قبل -->
<input type="text" 
       class="form-control persian-date" 
       data-persian-datepicker="true" />

<!-- ✅ بعد -->
<input type="text" 
       class="form-control persian-date-input" 
       data-jdp
       data-jdp-theme="medical"
       data-jdp-size="medium"
       data-no-default-date="true" />
```

---

### **2. JavaScript - Remove Old Initialization**

```javascript
// ❌ حذف کنید
$('.persian-date').pDatepicker({
    initialValue: false,
    format: 'YYYY/MM/DD'
});

// ✅ نیازی به initialization دستی نیست!
// Component به صورت خودکار initialize می‌شود
```

---

### **3. JavaScript - Dynamic Inputs**

```javascript
// ✅ برای Inputهای داینامیک
function addNewDateInput() {
    var html = '<input type="text" data-jdp data-jdp-theme="medical" />';
    var $newItem = $(html);
    $('#container').append($newItem);
    
    if (typeof JalaliDatePickerEnterprise !== 'undefined') {
        JalaliDatePickerEnterprise.init($newItem[0], {
            theme: 'medical',
            size: 'medium',
            noDefaultDate: true
        });
    }
}
```

---

### **4. JavaScript - Event Handling**

```javascript
// ✅ استفاده از pDatepicker:select (backward compatible)
document.querySelectorAll('.persian-date-input').forEach(function(input) {
    input.addEventListener('pDatepicker:select', function(e) {
        var date = this.value;
        // ...
    });
});
```

---

### **5. Layout - Add Scripts (اگر لود نشده است)**

```html
<!-- ✅ JalaliDatePicker Enterprise -->
<link href="~/Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.css" rel="stylesheet" />
<link href="~/Content/css/jalali-datepicker-enterprise.css" rel="stylesheet" />
<script src="~/Content/js/plugins/PersianDateTimePicker/jalaali.js"></script>
<script src="~/Content/js/plugins/PersianDateTimePicker/jalalidatepicker.min.js"></script>
<script src="~/Content/js/jalali-datepicker-enterprise.js"></script>
```

---

## 📋 **Attributes Reference**

| Attribute | توضیح | مثال |
|-----------|-------|------|
| `data-jdp` | فعال‌سازی | `data-jdp` |
| `data-jdp-theme` | Theme | `data-jdp-theme="medical"` |
| `data-jdp-size` | Size | `data-jdp-size="medium"` |
| `data-no-default-date` | بدون تاریخ پیش‌فرض | `data-no-default-date="true"` |
| `data-jdp-init-date` | تاریخ اولیه | `data-jdp-init-date="1404/10/15"` |
| `data-jdp-min-date` | حداقل تاریخ | `data-jdp-min-date="1404/10/15"` |
| `data-jdp-max-date` | حداکثر تاریخ | `data-jdp-max-date="1404/12/29"` |

---

## 🔧 **API Reference**

### **Initialization**
```javascript
JalaliDatePickerEnterprise.init(inputElement, {
    theme: 'medical',
    size: 'medium',
    noDefaultDate: true
});
```

### **Get Today from Server**
```javascript
JalaliDatePickerEnterprise.getTodayFromServer().then(function(todayPersian) {
    console.log('Today:', todayPersian); // "1404/10/15"
});
```

### **Convert Date**
```javascript
var gregorianDate = JalaliDatePickerEnterprise.convertPersianToGregorian('1404/10/15');
```

### **Get Instance**
```javascript
var instance = JalaliDatePickerEnterprise.getInstance('#myInput');
```

---

## ✅ **Checklist**

- [ ] تغییر `data-persian-datepicker="true"` به `data-jdp`
- [ ] اضافه کردن `data-jdp-theme="medical"`
- [ ] اضافه کردن `data-jdp-size="medium"`
- [ ] حذف `$('.persian-date').pDatepicker({...})`
- [ ] تغییر Event Listeners
- [ ] اضافه کردن Initialization برای Inputهای داینامیک
- [ ] بررسی Layout (Scripts لود شده‌اند)

---

**📚 برای جزئیات بیشتر:** [راهنمای کامل به‌روزرسانی](./JALALIDATEPICKER_MIGRATION_GUIDE.md)

