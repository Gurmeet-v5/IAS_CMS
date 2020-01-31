using System;
using KISD.Areas.BlogAdmin.Infrastructure;
using KISD.Areas.BlogAdmin.Interfaces;
using KISD.Areas.BlogAdmin.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using KISD.Areas.BlogAdmin.Models;
using System.Web.Security;
using System.Text.RegularExpressions;

namespace KISD.Areas.BlogAdmin.Controllers
{
    public class AccountController : Controller
    {
        #region Intialize Form or MemberShip Service
        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }
        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }
        #endregion

        #region Login
        /// <summary>
        /// Initialize the login view.
        /// </summary>
        /// <param name="ReturnUrl">return url.</param>
        /// <param name="isChanged">Check weather password is changed.</param>
        /// <returns>View for the user.</returns>
        public ActionResult Login(string ReturnUrl, string isChanged)
        {
            if (string.IsNullOrEmpty(ReturnUrl) && Request.UrlReferrer != null && string.IsNullOrEmpty(isChanged))
                ReturnUrl = Request.UrlReferrer.PathAndQuery.ToString();
            if (!string.IsNullOrEmpty(isChanged))
            {
                if (isChanged == "1")
                {
                    TempData["AlertMessage"] = "Password changed successfully. Please login again.";
                }
                else if (isChanged == "2")
                {
                    TempData["AlertMessage"] = "Account Inactivated successfully.";
                }
            }

            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                ViewBag.ReturnUrl = ReturnUrl;
            }
            Session.Abandon();
            FormsAuthentication.SignOut();
            if (this.User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            ModelState.Clear();
            return View();
        }

        /// <summary>
        /// On click of login we check for credentilas of user that are valid or not.
        /// </summary>
        /// <param name="model"> Get detail of class object</param>
        /// <param name="returnUrl"> if we have some appended url  when person is not logged in.then  after checking credentials redirect to that url.</param>
        /// <param name="command"> define that is cancel command or login command </param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Login(BlogAccountModel model, string ReturnUrl, string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                if (ModelState.IsValid)
                {
                    if (MembershipService.ValidateUser(model.UserNameTxt,
                        model.Password))
                    {
                        FormsService.SignIn(model.UserNameTxt, model.RememberMe);
                        if (!String.IsNullOrEmpty(ReturnUrl) && ReturnUrl.Length > 1 && ReturnUrl.StartsWith("/")
                            && !ReturnUrl.StartsWith("//") && !ReturnUrl.StartsWith("/\\"))
                        {
                            return Redirect(ReturnUrl);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    ModelState.AddModelError("", "Login failed. Please check username/password and try again.");
                }
                // If we got this far, something failed, redisplay form
                return View(model);
            }
            else
            {
                if (model != null)
                {
                    model.Password = string.Empty;
                    model.UserNameTxt = string.Empty;
                    model.RememberMe = false;
                }
                ModelState.Clear();
                return View(model);
            }
        }
        #endregion

        #region Logout
        /// <summary>
        /// This method is called when user click on logout button in admin section
        /// </summary>
        /// <param name="ReturnUrl">This parameter is used to get the return url</param>
        /// <returns></returns>
        public ActionResult LogOff(string ReturnUrl)
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            var ReturnUrlVal = "";
            if (string.IsNullOrEmpty(ReturnUrl) && Request.UrlReferrer != null)
                ReturnUrlVal = Request.UrlReferrer.PathAndQuery.ToString();

            return RedirectToAction("Login", "Account", new { ReturnUrl = ReturnUrlVal });
        }
        #endregion

        #region Change Password
        /// <summary>
        /// Get View for change password
        /// </summary>
        /// <returns></returns>
        [SessionExpire]
        [Authorize]
        public ActionResult ChangePassword()
        {
            TempData["AlertMessage"] = "";
            return View();
        }
        /// <summary>
        /// Update Password
        /// Else Return error messages.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [SessionExpire]
        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model, string command)
        {
            TempData["AlertMessage"] = "";
            if (string.IsNullOrEmpty(command))
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather
                    // than return false in certain failure scenarios.
                    bool changePasswordSucceeded = false;
                    try
                    {
                        if (model.NewPassword == model.ConfirmPassword && Regex.Match(model.NewPassword, @"^.*(?=.{8,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$").Success)
                        {
                            var userContext = new Contexts.UsersContexts();
                            var md5HashOld = model.OldPassword;
                            var md5HashNew = model.NewPassword;
                            changePasswordSucceeded = userContext.ChangePassword(md5HashOld, md5HashNew, User.Identity.Name);
                        }
                        else if (!Regex.Match(model.NewPassword, @"^.*(?=.{8,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$").Success)
                        {
                            TempData["AlertMessage"] = "Password must be 8 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.";
                            return View(model);
                        }
                        else
                        {
                            TempData["AlertMessage"] = "Confirm New Password should be same as New Password.";
                            return View(model);
                        }
                    }
                    catch (Exception ex)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        FormsAuthentication.SignOut();
                        Session.Abandon();
                        return RedirectToAction("Login", "Account", new { isChanged = "1" });
                    }
                    else
                    {
                        ModelState.AddModelError("OldPassword", "The current password is incorrect.");
                    }
                }
                // If we got this far, something failed, redisplay form
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        #endregion
    }
}
