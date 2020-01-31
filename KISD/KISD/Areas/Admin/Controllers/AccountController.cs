using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using KISD.Common.Infrastructure;
using KISD.Common.Interfaces;
using KISD.Areas.Admin.Models;
using KISD.Common.Services;
using KISD;
using System.Linq;
using System.Configuration;
using KISD.Common;
using System.Web;
using System.Security.Principal;

namespace KISD.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        #region Initialized form/Membership provider
        public IFormsAuthenticationService FormService { get; set; }

        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormService == null) { FormService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }
            base.Initialize(requestContext);
        }
        #endregion
        public ActionResult Index()
        {
            return View();
        }

        #region Login
        public ActionResult Login()
        {
            AccountModel accountModel = new AccountModel();

            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                // Get the forms authentication ticket.
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                var identity = new GenericIdentity(authTicket.Name, "Forms");

                var IsPersistent = ((FormsIdentity)(HttpContext.User.Identity)).Ticket.IsPersistent;
                if (IsPersistent)
                {
                    // Get the custom user data encrypted in the ticket.
                    string userData = ((FormsIdentity)(HttpContext.User.Identity)).Ticket.UserData;

                    accountModel.IsCheckedRememberMe = accountModel.RememberMe = IsPersistent;
                    // accountModel.Password = userData;
                    accountModel.UserNameTxt = identity.Name;
                    ViewBag.WebName = userData;
                }
            }

            return View(accountModel);
        }

        /// <summary>
        /// check the entered credential to login as Admin
        /// </summary>
        /// <param name="accountModel">object of account model</param>
        /// <param name="ReturnUrl">if any error occured then return to this url</param>
        /// <param name="command">name of the command(login or cancel)</param>
        /// <returns>if successfully logged in then admin dashboard else login view with error</returns>
        [HttpPost]
        public ActionResult Login(AccountModel accountModel, string ReturnUrl, string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                if (ModelState.IsValid)
                {
                    var password = accountModel.IsCheckedRememberMe ? EncryptDecrypt.Decrypt(accountModel.Password) : accountModel.Password;
                    password = string.IsNullOrEmpty(password) ? accountModel.Password : password;

                    if (MembershipService.ValidateUser(accountModel.UserNameTxt, password))
                    {
                        FormService.SignIn(accountModel.UserNameTxt, accountModel.RememberMe);
                        FormsAuthentication.SetAuthCookie(accountModel.UserNameTxt, accountModel.RememberMe);

                        var authTicket = new FormsAuthenticationTicket(1, accountModel.UserNameTxt, DateTime.Now, DateTime.Now.AddDays(30), accountModel.RememberMe, accountModel.RememberMe ? EncryptDecrypt.Encrypt(password) : "", "/");
                        //encrypt the ticket and add it to a cookie
                        HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(authTicket));
                        Response.Cookies.Add(cookie);

                        #region System Access Log
                        db_KISDEntities _context = new db_KISDEntities();
                        SystemAccessLog objSystemAccessLog;

                        #region Check for user already logged in or not

                        objSystemAccessLog = _context.SystemAccessLogs.Where(x => x.UserNameTxt == accountModel.UserNameTxt).OrderByDescending(x => x.SystemAccessLogID).FirstOrDefault();
                        if (objSystemAccessLog != null)
                        {
                            objSystemAccessLog.LogoutDateTime = objSystemAccessLog.LogoutDateTime > System.DateTime.Now ? System.DateTime.Now : objSystemAccessLog.LogoutDateTime;
                            _context.Entry(objSystemAccessLog).State = System.Data.Entity.EntityState.Modified;
                            _context.SaveChanges();
                        }
                        #endregion
                        User objUser = _context.Users.Where(x => x.UserNameTxt == accountModel.UserNameTxt).FirstOrDefault();
                        objSystemAccessLog = new SystemAccessLog();
                        objSystemAccessLog.UserNameTxt = objUser.UserNameTxt;
                        objSystemAccessLog.NameTxt = objUser.FirstNameTxt + " " + objUser.LastNameTxt;
                        objSystemAccessLog.LoginDateTime = System.DateTime.Now;
                        objSystemAccessLog.LogoutDateTime = Convert.ToDateTime(System.DateTime.Today.ToShortDateString() + " 23:59:00");
                        objSystemAccessLog.UserRoleID = _context.UserRoles.Where(x => x.UserID == objUser.UserID).FirstOrDefault().RoleID;
                        _context.SystemAccessLogs.Add(objSystemAccessLog);
                        _context.SaveChanges();

                        #endregion

                        if (!string.IsNullOrEmpty(ReturnUrl) && ReturnUrl.Length > 1 && ReturnUrl.StartsWith("/")
                            && !ReturnUrl.StartsWith("//") && !ReturnUrl.StartsWith("/\\"))
                        {
                            return Redirect(ReturnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    ModelState.AddModelError("", "Login failed. Please check Username/Password and try again.");
                }
                return View(accountModel);
            }
            else
            {
                if (accountModel != null)
                {
                    accountModel.Password = string.Empty;
                    accountModel.UserNameTxt = string.Empty;
                    accountModel.RememberMe = false;
                }
                ModelState.Clear();
                return View(accountModel);
            }
        }
        #endregion

        #region Logoff
        /// <summary>
        /// this method is called when admin lohout his/her account
        /// </summary>
        /// <returns>redirect to login view</returns>
        public ActionResult LogOff()
        {
            db_KISDEntities _contex = new db_KISDEntities();

            SystemAccessLog objSystemAccessLog = new SystemAccessLog();
            var usename = Membership.GetUser().UserName;
            objSystemAccessLog = _contex.SystemAccessLogs.Where(x => x.UserNameTxt == usename).OrderByDescending(x => x.SystemAccessLogID).FirstOrDefault();
            objSystemAccessLog.LogoutDateTime = System.DateTime.Now;
            _contex.Entry(objSystemAccessLog).State = System.Data.Entity.EntityState.Modified;
            _contex.SaveChanges();
            FormsAuthentication.SignOut();

            // Clear authentication cookie.
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie);

            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region Change Password
        /// <summary>
        /// to change the password
        /// </summary>
        /// <param name="PasswordModel">password model data with old and new passwords</param>
        /// <param name="command">it is submit request or cancel request</param>
        /// <returns>view with status whether passwoed is changed or any error occured.</returns>
        [SessionExpire]
        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel PasswordModel, string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                if (ModelState.IsValid)
                {
                    bool PasswordChangedSucceeded = false;
                    try
                    {
                        if (PasswordModel.NewPassword == PasswordModel.ConfirmNewPassword && Regex.Match(PasswordModel.NewPassword, @"^.*(?=.{6,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$").Success)
                        {
                            var md5HashOld = CustomMembershipProvider.GetMd5Hash(PasswordModel.OldPassword);
                            var md5HashNew = CustomMembershipProvider.GetMd5Hash(PasswordModel.NewPassword);
                            MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                            PasswordChangedSucceeded = currentUser.ChangePassword(md5HashOld, md5HashNew);
                        }
                        else if (!Regex.Match(PasswordModel.NewPassword, @"^.*(?=.{6,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$").Success)
                        {
                            TempData["AlertMessage"] = "Password must be 6 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.";
                            return View(PasswordModel);
                        }
                        else
                        {
                            TempData["AlertMessage"] = "Confirm Password does not match with new Password.";
                            return View(PasswordModel);
                        }
                    }
                    catch (Exception)
                    {
                        PasswordChangedSucceeded = false;
                    }
                    if (PasswordChangedSucceeded)
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
                //when something went wrong then return view with model
                return View(PasswordModel);
            }
            else
            {
                ModelState.Clear();
                return RedirectToAction("Index", "Home");
            }
        }

        [SessionExpire]
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }
        #endregion

        #region Forgot Password
        public ActionResult ForgotPassword(string Message)
        {
            AccountModel accountModel = new AccountModel();
            if (!string.IsNullOrEmpty(Message))
            {
                accountModel.Email = accountModel.Message = string.Empty;
                ViewBag.Message = Message;
                return View();
            }
            return View(accountModel);
        }

        [HttpPost]
        public ActionResult ForgotPassword(AccountModel model, FormCollection fc)
        {
            db_KISDEntities _Context = new db_KISDEntities();

            #region Send Email
            if (!string.IsNullOrEmpty(model.Email))
            {
                bool IsMailSent = false;
                string AppPath = ConfigurationManager.AppSettings["AppPath"].ToString();
                string body = string.Empty;
                var query = _Context.Users.Where(x => x.EmailTxt.Trim().ToLower() == model.Email.Trim().ToLower() && x.IsDeletedInd == false).FirstOrDefault();
                if (query != null)
                {
                    string FullLink = AppPath + "/Reset/ResetPassword?u=" + EncryptDecrypt.Encrypt(query.UserID.ToString());
                    var myMailUtilityBAL = new MailUtilityBAL();
                    var FromEmail = ConfigurationManager.AppSettings["FromEmail"].ToString();

                    #region Email Body
                    body = "<table style=\"font-family:Arial;font-size:12px\" border=\"0\" align=\"left\" cellpadding=\"4\" cellspacing=\"0\" >";
                    body += "<tr><td colspan='2' > Dear " + query.FirstNameTxt + " " + query.LastNameTxt + @",<br/><br/>Please click on the <a href='" + FullLink + "'>link</a> to reset your Password.</td></tr>";
                    body = body + "<tr><td colspan='2'><br/>Regards,<br/>Killeen ISD Team.</td></tr></table>";
                    #endregion

                    #region Email                    
                    try
                    {
                        if (myMailUtilityBAL.SendEmail(FromEmail, query.EmailTxt, body, ("Killeen ISD - Password Reset Request")).ToLower().Trim() == "ok")
                        {
                            IsMailSent = true;
                        }
                    }
                    catch (Exception ce)
                    {
                        IsMailSent = false;
                    }

                    #endregion
                }
                if (IsMailSent)
                    model.Message = "Reset Password link is sent to the email. Please click on the link and reset the Password.";
            }
            #endregion

            return RedirectToAction("ForgotPassword", model);
        }
        #endregion
    }
}