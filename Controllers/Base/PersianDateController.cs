using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.ViewModels.Base;

namespace ClinicApp.Controllers.Base
{
    /// <summary>
    /// Base Controller برای عملیات تاریخ شمسی
    /// طبق اصول DRY و SRP طراحی شده است
    /// </summary>
    public abstract class PersianDateController : Controller
    {
        /// <summary>
        /// آماده‌سازی مدل برای فرم ایجاد
        /// </summary>
        /// <typeparam name="T">نوع ViewModel</typeparam>
        /// <param name="model">مدل</param>
        /// <returns>مدل آماده شده</returns>
        protected virtual T PrepareModelForCreate<T>(T model) where T : PersianDateViewModel
        {
            // در فرم ایجاد، TextBox ها باید خالی باشند
            // model.ConvertGregorianDatesToPersian(); // Comment شده
            return model;
        }

        /// <summary>
        /// آماده‌سازی مدل برای فرم ویرایش
        /// </summary>
        /// <typeparam name="T">نوع ViewModel</typeparam>
        /// <param name="model">مدل</param>
        /// <returns>مدل آماده شده</returns>
        protected virtual T PrepareModelForEdit<T>(T model) where T : PersianDateViewModel
        {
            // تبدیل تاریخ‌های میلادی به شمسی برای نمایش در فرم
            model.ConvertGregorianDatesToPersian();
            return model;
        }

        /// <summary>
        /// آماده‌سازی مدل برای POST
        /// </summary>
        /// <typeparam name="T">نوع ViewModel</typeparam>
        /// <param name="model">مدل</param>
        /// <returns>مدل آماده شده</returns>
        protected virtual T PrepareModelForPost<T>(T model) where T : PersianDateViewModel
        {
            // تبدیل تاریخ‌های شمسی به میلادی قبل از validation
            model.ConvertPersianDatesToGregorian();
            return model;
        }

        /// <summary>
        /// اعتبارسنجی مدل با تاریخ شمسی
        /// </summary>
        /// <typeparam name="T">نوع ViewModel</typeparam>
        /// <param name="model">مدل</param>
        /// <returns>true اگر معتبر باشد</returns>
        protected virtual bool ValidateModelWithPersianDates<T>(T model) where T : PersianDateViewModel
        {
            if (model is PersianDateViewModelWithValidation validationModel)
            {
                return validationModel.ValidatePersianDates();
            }
            return true;
        }

        /// <summary>
        /// اضافه کردن خطای validation برای تاریخ
        /// </summary>
        /// <param name="fieldName">نام فیلد</param>
        /// <param name="errorMessage">پیام خطا</param>
        protected virtual void AddDateValidationError(string fieldName, string errorMessage)
        {
            ModelState.AddModelError(fieldName, errorMessage);
        }

        /// <summary>
        /// اضافه کردن خطای validation برای مقایسه تاریخ
        /// </summary>
        /// <param name="fieldName">نام فیلد</param>
        /// <param name="errorMessage">پیام خطا</param>
        protected virtual void AddDateComparisonError(string fieldName, string errorMessage)
        {
            ModelState.AddModelError(fieldName, errorMessage);
        }
    }

    /// <summary>
    /// Base Controller برای عملیات CRUD با تاریخ شمسی
    /// </summary>
    /// <typeparam name="TViewModel">نوع ViewModel</typeparam>
    /// <typeparam name="TEntity">نوع Entity</typeparam>
    public abstract class PersianDateCrudController<TViewModel, TEntity> : PersianDateController
        where TViewModel : PersianDateViewModel
        where TEntity : class
    {
        /// <summary>
        /// ایجاد مدل جدید
        /// </summary>
        /// <returns>مدل جدید</returns>
        protected abstract TViewModel CreateNewModel();

        /// <summary>
        /// دریافت مدل بر اساس ID
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>مدل</returns>
        protected abstract Task<TViewModel> GetModelByIdAsync(int id);

        /// <summary>
        /// ذخیره مدل
        /// </summary>
        /// <param name="model">مدل</param>
        /// <returns>نتیجه عملیات</returns>
        protected abstract Task<bool> SaveModelAsync(TViewModel model);

        /// <summary>
        /// به‌روزرسانی مدل
        /// </summary>
        /// <param name="model">مدل</param>
        /// <returns>نتیجه عملیات</returns>
        protected abstract Task<bool> UpdateModelAsync(TViewModel model);

        /// <summary>
        /// GET: Create
        /// </summary>
        /// <returns>View</returns>
        public virtual async Task<ActionResult> Create()
        {
            var model = CreateNewModel();
            model = PrepareModelForCreate(model);
            return View(model);
        }

        /// <summary>
        /// GET: Edit
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>View</returns>
        public virtual async Task<ActionResult> Edit(int id)
        {
            var model = await GetModelByIdAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            model = PrepareModelForEdit(model);
            return View(model);
        }

        /// <summary>
        /// POST: Create
        /// </summary>
        /// <param name="model">مدل</param>
        /// <returns>View</returns>
        [HttpPost]
        public virtual async Task<ActionResult> Create(TViewModel model)
        {
            model = PrepareModelForPost(model);

            if (ModelState.IsValid && ValidateModelWithPersianDates(model))
            {
                var result = await SaveModelAsync(model);
                if (result)
                {
                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        /// <summary>
        /// POST: Edit
        /// </summary>
        /// <param name="model">مدل</param>
        /// <returns>View</returns>
        [HttpPost]
        public virtual async Task<ActionResult> Edit(TViewModel model)
        {
            model = PrepareModelForPost(model);

            if (ModelState.IsValid && ValidateModelWithPersianDates(model))
            {
                var result = await UpdateModelAsync(model);
                if (result)
                {
                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }
    }
}
