using System.Linq;
using System.Web.Mvc;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// Controller برای redirect کردن URL های اشتباه به URL های صحیح
    /// </summary>
    public class RedirectController : Controller
    {
        /// <summary>
        /// Redirect کردن URL های View به Controller Action
        /// مثال: /Areas/Admin/Views/CMS/ClinicWorkingHours/Index.cshtml -> /Admin/CMS/ClinicWorkingHours
        /// </summary>
        [HttpGet]
        public ActionResult ViewToController(string area, string path)
        {
            if (string.IsNullOrEmpty(area) || string.IsNullOrEmpty(path))
            {
                return RedirectToAction("Index", "Home");
            }

            // Parse کردن path: CMS/ClinicWorkingHours/Index.cshtml
            var pathParts = path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToList();
            
            if (pathParts.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            // حذف پسوند .cshtml از آخرین بخش
            var lastPart = pathParts.Last();
            if (lastPart.EndsWith(".cshtml", System.StringComparison.OrdinalIgnoreCase))
            {
                lastPart = lastPart.Replace(".cshtml", "");
                pathParts[pathParts.Count - 1] = lastPart;
            }

            // برای Admin Area
            if (area.Equals("Admin", System.StringComparison.OrdinalIgnoreCase))
            {
                // اگر path با CMS شروع شود
                if (pathParts.Count > 0 && pathParts[0].Equals("CMS", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (pathParts.Count >= 2)
                    {
                        var controllerName = pathParts[1];
                        var actionName = pathParts.Count > 2 ? pathParts[2] : "Index";
                        
                        // اگر action برابر Index باشد، آن را در URL قرار نده
                        var redirectUrl = $"/Admin/CMS/{controllerName}";
                        if (!actionName.Equals("Index", System.StringComparison.OrdinalIgnoreCase))
                        {
                            redirectUrl += $"/{actionName}";
                        }
                        
                        return Redirect(redirectUrl);
                    }
                }
                else
                {
                    // برای سایر Controllers در Admin (بدون CMS)
                    if (pathParts.Count >= 1)
                    {
                        var controllerName = pathParts[0];
                        var actionName = pathParts.Count > 1 ? pathParts[1] : "Index";
                        
                        var redirectUrl = $"/Admin/{controllerName}";
                        if (!actionName.Equals("Index", System.StringComparison.OrdinalIgnoreCase))
                        {
                            redirectUrl += $"/{actionName}";
                        }
                        
                        return Redirect(redirectUrl);
                    }
                }
            }
            
            // برای Patient Area
            if (area.Equals("Patient", System.StringComparison.OrdinalIgnoreCase))
            {
                if (pathParts.Count >= 1)
                {
                    var controllerName = pathParts[0];
                    var actionName = pathParts.Count > 1 ? pathParts[1] : "Index";
                    
                    var redirectUrl = $"/Patient/{controllerName}";
                    if (!actionName.Equals("Index", System.StringComparison.OrdinalIgnoreCase))
                    {
                        redirectUrl += $"/{actionName}";
                    }
                    
                    return Redirect(redirectUrl);
                }
            }
            
            // اگر نتوانستیم redirect کنیم، به صفحه اصلی برویم
            return RedirectToAction("Index", "Home");
        }
    }
}

