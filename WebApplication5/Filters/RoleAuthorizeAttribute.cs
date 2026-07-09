using BusinessLayer.Helpers;
using BusinessLayer1.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication5.Filters
{
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly UserRole[] _allowedRoles;

        public RoleAuthorizeAttribute(params UserRole[] roles)
        {
            _allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = httpContext.Session?["UserSession"] as UserSession;
            if (user == null)
                return false;

            if (_allowedRoles == null || _allowedRoles.Length == 0)
                return true;

            var roleString = user.RoleName.Replace(" ", "");
            UserRole userRole;
            return Enum.TryParse<UserRole>(roleString, out userRole)
                && _allowedRoles.Contains(userRole);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                filterContext.Result = new RedirectResult("~/Auth/AccessDenied");
            else
                filterContext.Result = new RedirectResult("~/Auth/Login");
        }
    }
}
