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
                // Si no está logueado, redirige a Login
                context.Result = new RedirectToActionResult("Login", "Users", null);
            }
            else if (!_roles.Contains(userRole))
            {
                // Si está logueado pero no tiene el rol, muestra acceso denegado
                context.Result = new ViewResult { ViewName = "NoAccess" };
                // Alternativamente: context.Result = new ForbidResult();
            }
            base.OnActionExecuting(context);
        }
    }
}