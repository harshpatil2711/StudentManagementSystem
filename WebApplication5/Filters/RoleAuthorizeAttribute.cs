using BusinessLayer1.DAL;
using BusinessLayer1.Helpers;
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

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.HttpContext;

            string token = httpContext.Request.Headers["Authorization"]
                ?.StartsWith("Bearer ") == true
                ? httpContext.Request.Headers["Authorization"].Substring(7)
                : httpContext.Request.Cookies["jwt_token"]?.Value;

            if (token != null)
            {
                try
                {
                    var principal = JwtHelper.ValidateToken(token);
                    httpContext.User = principal;
                }
                catch
                {
                    TryRefreshToken(httpContext);
                }
            }
            else
            {
                var refreshCookie = httpContext.Request.Cookies["refresh_token"];
                if (refreshCookie != null)
                {
                    TryRefreshToken(httpContext);
                }
            }

            base.OnAuthorization(filterContext);
        }

        private void TryRefreshToken(HttpContextBase httpContext)
        {
            var refreshCookie = httpContext.Request.Cookies["refresh_token"];
            if (refreshCookie == null) return;

            try
            {
                var dal = new AuthDAL();
                string tokenHash = JwtHelper.HashRefreshToken(refreshCookie.Value);
                var storedToken = dal.GetRefreshToken(tokenHash);

                if (storedToken != null)
                {
                    dal.RevokeRefreshToken(storedToken.Id);

                    var user = dal.GetUserById(storedToken.UserId);
                    if (user != null)
                    {
                        string newAccessToken = JwtHelper.GenerateAccessToken(user);
                        string newRefreshToken = JwtHelper.GenerateRefreshToken();

                        dal.SaveRefreshToken(user.UserId, JwtHelper.HashRefreshToken(newRefreshToken),
                            httpContext.Request.UserAgent, httpContext.Request.UserHostAddress);

                        SetCookie(httpContext.Response, "jwt_token", newAccessToken, 15);
                        SetCookie(httpContext.Response, "refresh_token", newRefreshToken, 10080);

                        var principal = JwtHelper.ValidateToken(newAccessToken);
                        httpContext.User = principal;
                    }
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error in refresh token rotation");
            }
        }

        private void SetCookie(HttpResponseBase response, string name, string value, int expiryMinutes)
        {
            response.Cookies.Add(new HttpCookie(name, value)
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddMinutes(expiryMinutes)
            });
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            if (_allowedRoles == null || _allowedRoles.Length == 0)
                return true;

            return _allowedRoles.Any(r => httpContext.User.IsInRole(r.ToString()));
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
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
