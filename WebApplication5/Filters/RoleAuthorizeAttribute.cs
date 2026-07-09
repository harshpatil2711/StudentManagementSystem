using BusinessLayer1.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplication5.Filters
{
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly UserRole[] _allowedRoles;

        public RoleAuthorizeAttribute(params UserRole[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var session = httpContext.Session?["UserSession"] as UserSession;
            if (session == null)
                return false;

            if (_allowedRoles == null || _allowedRoles.Length == 0)
                return true;

            return _allowedRoles.Any(r => (int)r == session.RoleId);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session?["UserSession"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller", "Auth" },
                    { "action", "Login" }
                });
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller", "Auth" },
                    { "action", "AccessDenied" }
                });
            }
        }
    }
}
