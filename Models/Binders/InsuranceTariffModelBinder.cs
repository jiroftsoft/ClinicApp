using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ClinicApp.ViewModels.Insurance.InsuranceTariff;
using Serilog;

namespace ClinicApp.Models.Binders
{
    /// <summary>
    /// Model Binder سفارشی برای InsuranceTariffCreateEditViewModel
    /// برای مدیریت صحیح مقادیر "همه خدمات" و "همه سرفصل‌ها"
    /// </summary>
    public class InsuranceTariffModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            try
            {
                // Logging کامل ValueProvider
                var allValues = new Dictionary<string, string>();
                // در .NET Framework از ControllerContext استفاده می‌کنیم
                if (controllerContext?.HttpContext?.Request?.Form != null)
                {
                    foreach (string key in controllerContext.HttpContext.Request.Form.AllKeys)
                    {
                        allValues[key] = controllerContext.HttpContext.Request.Form[key] ?? "null";
                    }
                }
                
                   Log.Information("🏥 MEDICAL: Model Binder شروع شد - تمام مقادیر Form: {@AllValues}", allValues);
                   
                   // 🔍 CONSOLE LOGGING - Model Binder
                   System.Console.WriteLine("🔍 ===== MODEL BINDER DEBUG START =====");
                   System.Console.WriteLine($"🔍 Model Binder شروع شد - تمام مقادیر Form:");
                   foreach (var kvp in allValues)
                   {
                       System.Console.WriteLine($"🔍   {kvp.Key}: '{kvp.Value}'");
                   }
                
                // فراخوانی Model Binder پیش‌فرض ابتدا
                var model = base.BindModel(controllerContext, bindingContext) as InsuranceTariffCreateEditViewModel;
                
                if (model == null)
                {
                    Log.Warning("🏥 MEDICAL: Model Binder پیش‌فرض null برگرداند - ایجاد مدل جدید");
                    model = new InsuranceTariffCreateEditViewModel();
                }
                
                // پردازش مقادیر Form به صورت دستی
                var form = controllerContext.HttpContext.Request.Form;
                
                // پردازش DepartmentId
                if (form["DepartmentId"] != null && !string.IsNullOrEmpty(form["DepartmentId"]))
                {
                    if (int.TryParse(form["DepartmentId"], out int deptId))
                    {
                        model.DepartmentId = deptId;
                        Log.Information("🏥 MEDICAL: DepartmentId تنظیم شد: {DepartmentId}", deptId);
                    }
                }
                
                // پردازش InsuranceProviderId
                if (form["InsuranceProviderId"] != null && !string.IsNullOrEmpty(form["InsuranceProviderId"]))
                {
                    if (int.TryParse(form["InsuranceProviderId"], out int providerId))
                    {
                        model.InsuranceProviderId = providerId;
                        Log.Information("🏥 MEDICAL: InsuranceProviderId تنظیم شد: {InsuranceProviderId}", providerId);
                    }
                }
                
                // پردازش InsurancePlanId
                if (form["InsurancePlanId"] != null && !string.IsNullOrEmpty(form["InsurancePlanId"]))
                {
                    if (int.TryParse(form["InsurancePlanId"], out int planId))
                    {
                        model.InsurancePlanId = planId;
                        Log.Information("🏥 MEDICAL: InsurancePlanId تنظیم شد: {InsurancePlanId}", planId);
                    }
                }
                
                // پردازش ServiceId
                if (form["ServiceId"] != null && !string.IsNullOrEmpty(form["ServiceId"]))
                {
                    if (int.TryParse(form["ServiceId"], out int serviceId))
                    {
                        model.ServiceId = serviceId;
                        Log.Information("🏥 MEDICAL: ServiceId تنظیم شد: {ServiceId}", serviceId);
                    }
                }
                
                // پردازش ServiceCategoryId
                if (form["ServiceCategoryId"] != null && !string.IsNullOrEmpty(form["ServiceCategoryId"]))
                {
                    if (int.TryParse(form["ServiceCategoryId"], out int categoryId))
                    {
                        model.ServiceCategoryId = categoryId;
                        Log.Information("🏥 MEDICAL: ServiceCategoryId تنظیم شد: {ServiceCategoryId}", categoryId);
                    }
                }
                
                // پردازش TariffPrice
                if (form["TariffPrice"] != null && !string.IsNullOrEmpty(form["TariffPrice"]))
                {
                    if (decimal.TryParse(form["TariffPrice"], out decimal price))
                    {
                        model.TariffPrice = price;
                        Log.Information("🏥 MEDICAL: TariffPrice تنظیم شد: {TariffPrice}", price);
                    }
                }
                
                // پردازش InsurerShare
                if (form["InsurerShare"] != null && !string.IsNullOrEmpty(form["InsurerShare"]))
                {
                    if (decimal.TryParse(form["InsurerShare"], out decimal insurerShare))
                    {
                        model.InsurerShare = insurerShare;
                        Log.Information("🏥 MEDICAL: InsurerShare تنظیم شد: {InsurerShare}", insurerShare);
                    }
                }
                
                // پردازش PatientShare
                if (form["PatientShare"] != null && !string.IsNullOrEmpty(form["PatientShare"]))
                {
                    if (decimal.TryParse(form["PatientShare"], out decimal patientShare))
                    {
                        model.PatientShare = patientShare;
                        Log.Information("🏥 MEDICAL: PatientShare تنظیم شد: {PatientShare}", patientShare);
                    }
                }
                
                // پردازش IsAllServices
                if (form["IsAllServices"] != null)
                {
                    model.IsAllServices = form["IsAllServices"] == "true";
                    Log.Information("🏥 MEDICAL: IsAllServices تنظیم شد: {IsAllServices}", model.IsAllServices);
                }
                
                // پردازش IsAllServiceCategories
                if (form["IsAllServiceCategories"] != null)
                {
                    model.IsAllServiceCategories = form["IsAllServiceCategories"] == "true";
                    Log.Information("🏥 MEDICAL: IsAllServiceCategories تنظیم شد: {IsAllServiceCategories}", model.IsAllServiceCategories);
                }
                
                // اگر "همه خدمات" انتخاب شده، ServiceId را به null تنظیم کن
                if (model.IsAllServices)
                {
                    model.ServiceId = null;
                    Log.Information("🏥 MEDICAL: همه خدمات انتخاب شده - ServiceId به null تنظیم شد");
                }
                
                // اگر "همه سرفصل‌ها" انتخاب شده، ServiceCategoryId را به null تنظیم کن
                if (model.IsAllServiceCategories)
                {
                    model.ServiceCategoryId = null;
                    Log.Information("🏥 MEDICAL: همه سرفصل‌ها انتخاب شده - ServiceCategoryId به null تنظیم شد");
                }
                
                   Log.Information("🏥 MEDICAL: Model Binder تکمیل شد - DepartmentId: {DepartmentId}, InsuranceProviderId: {InsuranceProviderId}, InsurancePlanId: {InsurancePlanId}, ServiceId: {ServiceId}, ServiceCategoryId: {ServiceCategoryId}, TariffPrice: {TariffPrice}, InsurerShare: {InsurerShare}, PatientShare: {PatientShare}, IsAllServices: {IsAllServices}, IsAllServiceCategories: {IsAllServiceCategories}",
                       model.DepartmentId, model.InsuranceProviderId, model.InsurancePlanId, model.ServiceId, model.ServiceCategoryId, model.TariffPrice, model.InsurerShare, model.PatientShare, model.IsAllServices, model.IsAllServiceCategories);
                   
                   // 🔍 CONSOLE LOGGING - Model Binder Final Values
                   System.Console.WriteLine("🔍 Model Binder مقادیر نهایی:");
                   System.Console.WriteLine($"🔍   DepartmentId: {model.DepartmentId}");
                   System.Console.WriteLine($"🔍   ServiceCategoryId: {model.ServiceCategoryId}");
                   System.Console.WriteLine($"🔍   ServiceId: {model.ServiceId}");
                   System.Console.WriteLine($"🔍   InsuranceProviderId: {model.InsuranceProviderId}");
                   System.Console.WriteLine($"🔍   InsurancePlanId: {model.InsurancePlanId}");
                   System.Console.WriteLine($"🔍   TariffPrice: {model.TariffPrice}");
                   System.Console.WriteLine($"🔍   PatientShare: {model.PatientShare}");
                   System.Console.WriteLine($"🔍   InsurerShare: {model.InsurerShare}");
                   System.Console.WriteLine($"🔍   IsAllServices: {model.IsAllServices}");
                   System.Console.WriteLine($"🔍   IsAllServiceCategories: {model.IsAllServiceCategories}");
                   System.Console.WriteLine("🔍 ===== MODEL BINDER DEBUG END =====");
                   
                   return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "🏥 MEDICAL: خطا در Model Binder");
                return base.BindModel(controllerContext, bindingContext);
            }
        }
    }
}
