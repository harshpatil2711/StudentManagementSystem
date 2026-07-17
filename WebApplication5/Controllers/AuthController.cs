using BusinessLayer.Helpers;
using BusinessLayer1.DAL;
using BusinessLayer1.Helpers;
using BusinessLayer1.Models;
using Serilog;
using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    public class AuthController : Controller
    {
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                AuthDAL dal = new AuthDAL();
                string storedHash;
                UserSession user = dal.AuthenticateUser(model.Username, out storedHash);

                if (user == null || !PasswordHelper.VerifyPassword(model.Password, storedHash))
                {
                    ViewBag.Error = "Invalid username or password";
                    return View(model);
                }

                string accessToken = JwtHelper.GenerateAccessToken(user);
                string refreshToken = JwtHelper.GenerateRefreshToken();

                dal.SaveRefreshToken(user.UserId, JwtHelper.HashRefreshToken(refreshToken),
                    Request.UserAgent, Request.UserHostAddress);

                Response.Cookies.Add(new HttpCookie("jwt_token", accessToken)
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddMinutes(15)
                });
                Response.Cookies.Add(new HttpCookie("refresh_token", refreshToken)
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddDays(7)
                });

                dal.UpdateLastLogin(user.UserId);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Login error for user {Username}", model.Username);
                ViewBag.Error = "An error occurred. Please try again.";
            }

            return View(model);
        }

        public ActionResult SignUp()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewBag.RoleList = GetRoles();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(SignUpViewModel model)
        {
            ViewBag.RoleList = GetRoles();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                string passwordHash = PasswordHelper.HashPassword(model.Password);

                AuthDAL dal = new AuthDAL();
                string message = dal.RegisterUser(model, passwordHash);

                if (message.StartsWith("Error:"))
                {
                    ViewBag.Error = message;
                    return View(model);
                }

                TempData["Success"] = "Account created successfully. Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "SignUp error for user {Username}", model.Username);
                ViewBag.Error = "An error occurred. Please try again.";
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            AuthDAL dal = new AuthDAL();

            var claimsPrincipal = User as ClaimsPrincipal;
            var userIdClaim = claimsPrincipal?.Identity?.IsAuthenticated == true
                ? claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                : null;

            if (userIdClaim != null)
            {
                dal.RevokeAllUserTokens(Convert.ToInt32(userIdClaim));
            }
            else if (Request.Cookies["refresh_token"] != null)
            {
                string tokenHash = JwtHelper.HashRefreshToken(Request.Cookies["refresh_token"].Value);
                var stored = dal.GetRefreshToken(tokenHash);
                if (stored != null)
                    dal.RevokeAllUserTokens(stored.UserId);
            }

            if (Request.Cookies["jwt_token"] != null)
                Response.Cookies["jwt_token"].Expires = DateTime.Now.AddDays(-1);
            if (Request.Cookies["refresh_token"] != null)
                Response.Cookies["refresh_token"].Expires = DateTime.Now.AddDays(-1);

            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        private SelectList GetRoles()
        {
            var dal = new AuthDAL();
            var roles = dal.GetRoles();
            return new SelectList(roles, "Key", "Value");
        }
    }
}
