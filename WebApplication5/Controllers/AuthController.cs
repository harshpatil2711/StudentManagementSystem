using BusinessLayer.Helpers;
using BusinessLayer1.DAL;
using BusinessLayer1.Models;
using Serilog;
using System;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;

namespace WebApplication5.Controllers
{
    public class AuthController : Controller
    {
        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
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

                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                Session["UserSession"] = user;

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

        [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
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
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
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
