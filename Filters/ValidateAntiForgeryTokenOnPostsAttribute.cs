using System.Web.Mvc;

namespace ClinicApp.Filters
{
    /// <summary>
    /// فیلتر سفارشی برای اعمال Anti-Forgery Token فقط روی درخواست‌های POST, PUT, DELETE
    /// این فیلتر مشکل تداخل با GET requests را حل می‌کند
    /// </summary>
    public class ValidateAntiForgeryTokenOnPostsAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            
            // فقط روی POST، PUT، DELETE اعمال شود
            if (request.HttpMethod == "POST" || 
                request.HttpMethod == "PUT" || 
                request.HttpMethod == "DELETE")
            {
                // بررسی Anti-Forgery Token
                var validator = new ValidateAntiForgeryTokenAttribute();
                validator.OnAuthorization(filterContext);
            }
        }
    }
}
