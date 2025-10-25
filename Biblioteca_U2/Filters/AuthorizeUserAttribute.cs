using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Biblioteca_U2.Filters
{
    // Filtro para autorización de usuarios autenticados
    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            if (session["UserId"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" }
                    });
            }

            base.OnActionExecuting(filterContext);
        }
    }

    // Filtro para autorización solo de administradores
    public class AuthorizeAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            if (session["UserId"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" }
                    });
            }
            else if (session["UserRole"]?.ToString() != "admin")
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Home" },
                        { "action", "Index" }
                    });

                filterContext.Controller.TempData["ErrorMessage"] = "No tienes permisos para acceder a esta sección.";
            }

            base.OnActionExecuting(filterContext);
        }
    }

    // Filtro para redirigir usuarios ya autenticados
    public class RedirectIfAuthenticatedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            if (session["UserId"] != null)
            {
                var userRole = session["UserRole"]?.ToString();

                if (userRole == "admin")
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Admin" },
                            { "action", "Dashboard" }
                        });
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Home" },
                            { "action", "Index" }
                        });
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}