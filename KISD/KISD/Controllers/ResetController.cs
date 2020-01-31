using KISD.Common;
using KISD.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace KISD.Controllers
{
    public class ResetController : Controller
    {
        public ActionResult ResetPassword(string u)
        {
            if (!string.IsNullOrEmpty(u))
            {
                var UserID = Convert.ToInt64(EncryptDecrypt.Decrypt(u));
                ResetPasswordModel obj = new ResetPasswordModel();
                obj.UserID = UserID;
                return View(obj);
            }

            return RedirectToAction("Login", "Account", new { Area = "Admin" });
        }

        /// <summary>
        /// Update Password
        /// Else Return error messages.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model, string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather
                    // than return false in certain failure scenarios.
                    bool changePasswordSucceeded = false;
                    try
                    {
                        if (model.NewPassword == model.ConfirmPassword) // && Regex.Match(model.NewPassword, @"^.*(?=.{8,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$").Success
                        {
                            try
                            {
                                var password = model.NewPassword;
                                var md5HashNew = GetMd5Hash(model.NewPassword);
                                var objContext = new db_KISDEntities();
                                long UserID = model.UserID;
                                var obj = objContext.Users.Where(x => x.UserID == UserID).FirstOrDefault();
                                obj.PasswordTxt = md5HashNew;
                                objContext.SaveChanges();

                                #region email
                                var myMailUtilityBAL = new MailUtilityBAL();
                                var FromEmail = ConfigurationManager.AppSettings["FromEmail"].ToString();
                                var body = "";
                                var query = objContext.Users.Where(x => x.UserID == UserID && x.IsDeletedInd == false).FirstOrDefault();
                                if (query != null && query.EmailTxt != null)
                                {
                                    #region Email Body
                                    body = "<table style=\"font-family:Arial;font-size:12px\" border=\"0\" align=\"left\" cellpadding=\"4\" cellspacing=\"0\" >";
                                    body += "<tr><td colspan='2' > Dear " + query.FirstNameTxt + " " + query.LastNameTxt + @",<br/><br/>Your password has been updated successfully.Please see below your login details:</td></tr>";
                                    body += "<tr><td colspan='2' > <br/>Email: " + query.EmailTxt + @"</td></tr>";
                                    body += "<tr><td colspan='2' > User Name: " + query.UserNameTxt + @"</td></tr>";
                                    body += "<tr><td colspan='2' > Password: " + password + @"</td></tr>";
                                    body = body + "<tr><td colspan='2'><br/><b>Regards,<br/>Killeen ISD Team.</b></td></tr></table>";
                                    #endregion

                                    try
                                    {
                                        if (myMailUtilityBAL.SendEmail(FromEmail, query.EmailTxt, body, ("Killeen ISD - Password Reset Confirmation")).ToLower().Trim() == "ok")
                                        {
                                            changePasswordSucceeded = true;
                                        }
                                    }
                                    catch
                                    {
                                        changePasswordSucceeded = false;
                                    }
                                }
                                else
                                {
                                    changePasswordSucceeded = false;
                                }
                                #endregion

                                changePasswordSucceeded = true;
                                return RedirectToAction("ResetConfirmation", "Reset");
                            }
                            catch
                            {
                                changePasswordSucceeded = false;
                            }

                        }
                        //else if (!Regex.Match(model.NewPassword, @"^.*(?=.{8,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$").Success)
                        //{
                        //    TempData["AlertMessage"] = "Password must be 8 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.";
                        //    return View(model);
                        //}
                        else
                        {
                            TempData["AlertMessage"] = "Confirm New Password should be same as New Password.";
                            return View(model);
                        }
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
            else
            {
                RouteValueDictionary rvd = new RouteValueDictionary();
                rvd.Add("cus", model.UserID);
                model.NewPassword = "";
                model.ConfirmPassword = "";
                return RedirectToAction("ResetPassword", "Reset", rvd);
            }
        }

        public ActionResult ResetConfirmation()
        {

            return View();
        }

        /// <summary>
        /// Represents the abstract class from which all implementations of the MD5 hash algorithm inherit.
        /// </summary>
        /// <param name="value">The string value to decrypted in password.</param>
        /// <returns>The hash size for the MD5 algorithm is 128 bits.</returns>
        public static string GetMd5Hash(string value)
        {
            var md5Hasher = MD5.Create();
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            var sBuilder = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}
