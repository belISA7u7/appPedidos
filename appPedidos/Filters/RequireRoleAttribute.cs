using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace appPedidos.Filters
{
    public class RequireRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;
        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userRole = session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
            {
                // No logueado
                context.Result = new RedirectToActionResult("Login", "Users", null);
            }
            else if (!_roles.Contains(userRole))
            {
                // Logueado, pero sin permiso
                context.Result = new ViewResult { ViewName = "NoAccess" };
            }
            base.OnActionExecuting(context);
        }
    }
}