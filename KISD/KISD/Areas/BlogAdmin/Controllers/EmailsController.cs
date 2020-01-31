using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using KISD.Areas.BlogAdmin.Models;

namespace KISD.Areas.BlogAdmin.Controllers
{
    public class EmailsController : Controller
    {
        //
        // GET: /Blog/Emails/

        /// <summary>
        /// Method to get display the the Added Rentals in Grid.
        /// Code to update the status and show on home property by clicking the checkboxes added for the status and show on home.
        /// </summary>
        /// <param name="gridSortOptions">It will pass sorting parameters</param>
        /// <param name="page">Defines the page on which Email is currently viewing records</param>
        /// <param name="pagesize">Number of records to be displayed on he page.</param>
        /// <param name="type">Rentals or Featured Homes for Sale</param>
        /// <param name="fm">Form hidden field values</param>
        /// <returns></returns>
        [Authorize]
        [SessionExpire]
        public ActionResult BlogEmailListing(int? emailtype, GridSortOptions gridSortOptions, int? pagetype, int? page, int? pagesize, FormCollection fm, string objresult)
        {
            var _objContext = new Contexts.EmailsContexts();
            ViewBag.Title = _objContext.GetEmailType(emailtype.Value) + " Listing";
            var _EmailsModel = new EmailsModel();
            #region Ajax Call
            if (objresult != null)
            {
                AjaxRequest objAjaxRequest = JsonConvert.DeserializeObject<AjaxRequest>(objresult);//Convert json String to object Model
                if (objAjaxRequest.ajaxcall != null && !string.IsNullOrEmpty(objAjaxRequest.ajaxcall) && objresult != null && !string.IsNullOrEmpty(objresult))
                {
                    if (objAjaxRequest.ajaxcall == "paging")//Ajax Call type = paging i.e. Next|Previous|Back|Last
                    {
                        Session["pageNo"] = page;// stores the page no for status
                    }
                    else if (objAjaxRequest.ajaxcall == "sorting")//Ajax Call type = sorting i.e. column sorting Asc or Desc
                    {
                        page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                        Session["GridSortOption"] = gridSortOptions;
                        pagesize = (Session["PageSize"] != null ? Convert.ToInt32(Session["PageSize"].ToString()) : pagesize);
                    }
                    else if (objAjaxRequest.ajaxcall == "ddlPaging")//Ajax Call type = drop down paging i.e. drop down value 10, 25, 50, 100, ALL
                    {
                        Session["PageSize"] = (Request.QueryString["pagesize"] != null ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : pagesize);
                        Session["GridSortOption"] = gridSortOptions;
                        Session["pageNo"] = page;
                    }
                    else if (objAjaxRequest.ajaxcall == "status")//Ajax Call type = status i.e. Active/Inactive
                    {
                        page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    else if (objAjaxRequest.ajaxcall == "displayorder")//Ajax Call type = Display Order i.e. drop down values
                    {
                        page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
                }
            }
            #endregion Ajax Call
            //This section is used to retain the values of page , pagesize and gridsortoption on complete page post back(Edit, Dlete)
            if (!Request.IsAjaxRequest() && Session["Edit/Delete"] != null && !string.IsNullOrEmpty(Session["Edit/Delete"].ToString()))
            {
                pagesize = (Session["PageSize"] != null ? Convert.ToInt32(Session["PageSize"]) : Models.Common._pageSize);
                page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"]) : Models.Common._currentPage);
                gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                Session["Edit/Delete"] = null;
            }
            else if (!Request.IsAjaxRequest() && Session["Edit/Delete"] == null)
            {
                //gridSortOptions.Column = "CreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column != null && gridSortOptions.Column == "EmailTxt")
            {
            }
            else
            {
                gridSortOptions.Column = "EmailTxt";
            }
            var pageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
            var Page = page.HasValue ? page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;
            var pagedViewModel = new PagedViewModel<EmailsModel>
            {
                ViewData = ViewData,
                Query = _objContext.GetEmail(emailtype).AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "EmailTxt",
                Page = Page,
                PageSize = pageSize,
            }.Setup();

            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("BlogEmailListingPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }

        [Authorize]
        [SessionExpire]
        public ActionResult CreateEmail(int? Emailid, int emailtype)
        {
            var _EmailContext = new Contexts.EmailsContexts();
            var _EmailModel = new EmailsModel();
            var emailTypeName = _EmailContext.GetEmailType(emailtype);
            ViewBag.Title = (Emailid.HasValue ? "Edit " : "Add ") + emailTypeName;
            ViewBag.Submit = Emailid.HasValue && Emailid.Value > 0 ? "Update" : "Save";

            if (Emailid.HasValue && Emailid.Value > 0)
            {
                if (_EmailModel != null)
                {
                    _EmailModel = _EmailContext.GetEmails().Where(x => x.EmailID == Emailid).FirstOrDefault();
                }
            }
            return View(_EmailModel);
        }

        /// <summary>
        /// Method to save/ update the added rentals into the database.
        /// </summary>
        /// <param name="_Emailmodel">EmailModel that will contain the values saved in the Email View</param>
        /// <param name="command">This will contain value on click of Cancel button</param>
        /// <param name="fm">It contains the form collection (hidden field etc) values.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateEmail(EmailsModel _Emailmodel, string command, FormCollection fm, int emailtype)
        {
            Session["Edit/Delete"] = "Edit";
            var EmailContext = new Contexts.EmailsContexts();
            var emailTypeName = EmailContext.GetEmailType(emailtype);
            ViewBag.Title = (_Emailmodel.EmailID > 0 ? "Edit " : "Add ") + emailTypeName;
            ViewBag.Submit = _Emailmodel.EmailID > 0 ? "Update" : "Save";
            if (string.IsNullOrEmpty(command))
            {
                try
                {
                    if (ViewBag.Submit == "Save")
                    {
                        if (EmailContext.GetEmails().Where(x => x.EmailTxt.ToLower().Trim() == _Emailmodel.EmailTxt.ToLower().Trim() && x.EmailTypeID == emailtype).Count() > 0)
                        {
                            ModelState.AddModelError("EmailTxt", _Emailmodel.EmailTxt + " Email already exists.");
                            return View(_Emailmodel);
                        }
                        _Emailmodel.EmailTypeID = emailtype;
                        EmailContext.AddEmail(_Emailmodel);
                        TempData["AlertMessage"] = emailTypeName + " details saved successfully.";
                    }
                    else
                    {
                        if (EmailContext.GetEmails().Where(x => x.EmailTxt.ToLower().Trim() == _Emailmodel.EmailTxt.ToLower().Trim() && x.EmailTypeID == emailtype && x.EmailID != _Emailmodel.EmailID).Count() > 0)
                        {
                            ModelState.AddModelError("EmailTxt", _Emailmodel.EmailTxt + " Email already exists.");
                            return View(_Emailmodel);
                        }
                        EmailContext.EditEmails(_Emailmodel);
                        TempData["AlertMessage"] = emailTypeName + " details updated successfully.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["AlertMessage"] = "Some error occured, Please try after some time. " + ex.Message.ToString();
                }
            }
            var rvd = new RouteValueDictionary();
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "FirstName");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Ascending");
            rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
            rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
            rvd.Add("emailtype", _Emailmodel.EmailTypeID == 0 ? emailtype : _Emailmodel.EmailTypeID);
            return RedirectToAction("BlogEmailListing", "Emails", rvd);
        }

        /// <summary>
        /// Method to delete the email based on property id.
        /// </summary>
        /// <param name="EmailId"></param>
        /// <param name="emailtype"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        public JsonResult Delete(int? EmailId, int? emailtype)
        {
            Session["Edit/Delete"] = "Delete";
            var EmailContext = new Contexts.EmailsContexts();
            var SettingContexts = new Contexts.SettingContexts();
            var objSettingContexts = SettingContexts.GetSettings().FirstOrDefault();
            var rvd = new RouteValueDictionary();
            int? Page = 1;
            var count = 1;
            count = EmailContext.GetEmails().Count();
            var page = Request.QueryString["page"] ?? Models.Common._currentPage.ToString();
            var pagesize = Request.QueryString["pagesize"] ?? Models.Common._pageSize.ToString();
            if (Convert.ToInt32(page) > 1)
            {
                Page = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
            }
            rvd.Add("page", Page);
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "FirstName");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Ascending");
            rvd.Add("pagesize", pagesize);
            rvd.Add("emailtype", emailtype);
            if (EmailId.HasValue)
            {
                try
                {
                    if (EmailContext != null)
                    {
                        var email_coun = 0;
                        if (emailtype == 1)// Sending email
                        {
                            email_coun = SettingContexts.GetSettings().Where(x => x.SendingEmail == Convert.ToString(EmailId)).Count();
                        }
                        else if (emailtype == 2)// Receiving email
                        {
                            if (!string.IsNullOrEmpty(objSettingContexts.ReceivingEmail))
                            {
                                if (objSettingContexts.ReceivingEmail.Contains(","))
                                {
                                    var recevingemail_array = objSettingContexts.ReceivingEmail.Split(',');
                                    email_coun = recevingemail_array.Contains(Convert.ToString(EmailId)) ? 1 : 0;
                                }
                                else
                                {
                                    var recevingemail_array = new string[] { objSettingContexts.ReceivingEmail };
                                    email_coun = recevingemail_array.Contains(Convert.ToString(EmailId)) ? 1 : 0;
                                }
                            }
                        }
                        if (email_coun == 0)
                        {
                            EmailContext.DeleteEmails(EmailId);
                            TempData["AlertMessage"] = "Email details deleted successfully.";
                        }
                        else
                        {
                            TempData["Message"] = "Email is in use, can not be deleted.";
                            return Json(Url.Action("BlogEmailListing", "Emails", rvd));
                        }
                    }
                }
                catch
                {
                    TempData["AlertMessage"] = "Some error occured while deleting the Email, Please try again later.";
                }
            }
            return Json(Url.Action("BlogEmailListing", "Emails", rvd));
        }
    }
}
