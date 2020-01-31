using KISD.Areas.Admin.Models;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using static KISD.Areas.Admin.Models.Common;
using EmailTypeAlias = KISD.Areas.Admin.Models.EmailService.EmailType;

namespace KISD.Areas.Admin.Controllers
{
    public class EmailController : Controller
    {
        #region Initialize service

        /// <summary>
        /// Initialize the object for the SAWTST Database. 
        /// </summary>
        db_KISDEntities objContext = new db_KISDEntities();
        private EmailService _service;
        public EmailController()
        {
            _service = new EmailService();
        }

        #endregion

        #region Crud Operation for emails

        /// <summary>
        /// Get the Email List from the database on the basis of the Email Type passed by calling the GetEmailType method of the EmailModel Class.
        /// PagedViewModel method bind the email list into gridview.
        /// </summary>
        /// <param name="emailtype">Defines type of email From/to email</param>
        /// <param name="gridSortOptions">Sort the grid basis on the options defined</param>
        /// <param name="page">Current Page</param>        
        /// <param name="pagesize">Pagesize for records</param>
        /// <returns>Email Type of PagedViewModel Data.</returns>
        [Authorize]
        [SessionExpire]
        public ActionResult Emails(int? et, GridSortOptions gridSortOptions, int? page, int? pagesize, string eid, string objresult)
        {
            #region Check Tab is Accessible or Not
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Email), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            //Check for valid email type ID
            if (!et.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            //Decrypt email ID
            if (!string.IsNullOrEmpty(eid))
            {
                eid = Convert.ToString(EncryptDecrypt.Decrypt(eid));
            }
            else { eid = "0"; }

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
                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
                }
                else
                {
                    TempData["Message"] = string.Empty;
                }
                objresult = string.Empty;

            }
            #endregion Ajax Call

            //This section is used to retain the values of page , pagesize and gridsortoption on complete page post back(Edit, Dlete)
            if (!Request.IsAjaxRequest() && Session["Edit/Delete"] != null && !string.IsNullOrEmpty(Session["Edit/Delete"].ToString()))
            {
                pagesize = (Session["PageSize"] != null ? Convert.ToInt32(Session["PageSize"]) : (pagesize != null ? pagesize : Models.Common._pageSize));
                page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"]) : (page != null ? page : Models.Common._currentPage));
                gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                Session["Edit/Delete"] = null;
            }
            else if (!Request.IsAjaxRequest() && Session["Edit/Delete"] == null)
            {
                gridSortOptions.Column = "EmailTxt";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }

            if (gridSortOptions.Column != null && !gridSortOptions.Column.Contains("EmailTxt"))
            {
                gridSortOptions.Column = "EmailTxt";
            }

            var emailType = et.HasValue ? et.Value : Convert.ToInt32(EmailTypeAlias.From_Email);
            var pageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
            var Page = page.HasValue ? page.Value : _currentPage;
            TempData["pager"] = pagesize;
            ViewBag.Edit = null;//Check for postback or not
            if (page != null && pagesize != null)
            {
                ViewBag.Edit = true;
            }

            ViewBag.Title = _service.GetEmailType(emailType) + " Listing";
            var pagedViewModel = new PagedViewModel<EmailModel>
            {
                ViewData = ViewData,
                Query = _service.GetEmails(emailType),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "EmailTxt",
                Page = Page,
                PageSize = pageSize,
            }.Setup();

            if (et == 0)
            {
                return RedirectToAction("Index", "Email");
            }

            var emailTypeName = _service.GetEmailType(emailType);
            ViewBag.Title = (!string.IsNullOrEmpty(eid) && eid != "0" ? "Edit " : "Add ") + emailTypeName;
            ViewBag.Title = (!string.IsNullOrEmpty(eid) && eid != "0" ? "Edit " : "Add ") + emailTypeName;
            ViewBag.Submit = !string.IsNullOrEmpty(eid) && eid != "0" ? "Update" : "Save";

            pagedViewModel.EmailTypeID = emailType;

            if (eid != "0" && !string.IsNullOrEmpty(eid))
            {
                int iid = Convert.ToInt32(eid);
                var emaildata = objContext.Emails.Find(iid);
                if (emaildata != null)
                {
                    pagedViewModel.EmailTypeID = emaildata.EmailTypeID;
                    pagedViewModel.EmailTxt = emaildata.EmailTxt;
                    pagedViewModel.EmailID = emaildata.EmailID;
                    pagedViewModel.CreateByID = emaildata.CreateByID;
                    pagedViewModel.CreateDate = emaildata.CreateDate;
                    pagedViewModel.LastModifyByID = emaildata.LastModifyByID;
                    pagedViewModel.LastModifyDate = emaildata.LastModifyDate;
                    pagedViewModel.IsDeletedInd = emaildata.IsDeletedInd;
                    ViewBag.Edit = true;
                }
            }
            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("EmailPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }

        /// <summary>
        /// Only Authorize user will have rights to perform changes into email data.
        /// Add new or Update the existing email into the database as per request requested by the user.
        /// If model.EmailID is Zero then system will consider as new email and add into the database.
        /// Checks for the uniqueness of the email into the database. 
        /// If it already exists then display an error message else add or update into the database.
        /// </summary>
        /// <param name="model">Intialized EmailModel Object from Email Create view</param>
        /// <param name="command">Defines Submit or Cancel </param>
        /// <returns>Construct a redirect url to a specific action/controller and use the route table to generate the correct URL.</returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        public ActionResult Emails(int? et, EmailModel model, string command, GridSortOptions gridSortOptions, int? page, int? pagesize)
        {
            Session["Edit/Delete"] = "Edit";
            var emailType = et.HasValue ? et.Value : Convert.ToInt32(EmailTypeAlias.From_Email);
            var pageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
            var Page = page.HasValue ? page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;
            ViewBag.Edit = null;//Check for postback or not
            var grdsrtOpt = new GridSortOptions();
            grdsrtOpt.Column = gridSortOptions != null ? gridSortOptions.Column : "EmailTxt";
            grdsrtOpt.Direction = gridSortOptions != null ? gridSortOptions.Direction : MvcContrib.Sorting.SortDirection.Ascending;

            ViewBag.Title = _service.GetEmailType(emailType) + " Listing";
            var pagedViewModel = new PagedViewModel<EmailModel>
            {
                ViewData = ViewData,
                Query = _service.GetEmails(emailType),
                GridSortOptions = grdsrtOpt,
                DefaultSortColumn = "EmailTxt",
                Page = Page,
                PageSize = pageSize,
            }.Setup();

            ViewBag.Submit = model.EmailID == 0 ? "Save" : "Update";
            var isNew = (model.EmailID == 0);
            var emailTypeName = _service.GetEmailType(model.EmailTypeID);
            ViewBag.Title = (isNew ? "Add " : "Edit ") + emailTypeName;
            var emailTypeID = model.EmailTypeID == 0 ? Convert.ToInt32(Request.QueryString["emailtype"]) : model.EmailTypeID;
            var rvd = new RouteValueDictionary();
            DataTable dtOld;
            if (string.IsNullOrEmpty(command))
            {
                if (objContext.Emails.Where(x => x.EmailTxt.Trim().ToLower() == model.EmailTxt.Trim().ToLower() && x.EmailID != model.EmailID && x.EmailTypeID == model.EmailTypeID && x.IsDeletedInd == false).FirstOrDefault() != null)
                {
                    ModelState.AddModelError("EmailTxt", model.EmailTxt + " already exists.");
                    ViewBag.Edit = true;//Check for postback or not
                    return View(pagedViewModel);
                }
                #region System Change Log
                var oldresult = (from a in objContext.Emails
                                 where a.EmailID == model.EmailID
                                 select a).ToList();
                dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                #endregion
                var email = objContext.Emails.Where(x => x.EmailID == model.EmailID).FirstOrDefault();
                if (isNew)
                    email = new Email();
                email.EmailTxt = model.EmailTxt;
                email.EmailTypeID = model.EmailTypeID == 0 ? Convert.ToInt32(Request.QueryString["emailtype"]) : model.EmailTypeID;
                email.CreateDate = isNew ? DateTime.Now : email.CreateDate;
                email.CreateByID = isNew ? Convert.ToInt64(Membership.GetUser().ProviderUserKey) : email.CreateByID;
                email.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                email.LastModifyDate = DateTime.Now;
                email.IsDeletedInd = isNew ? false : email.IsDeletedInd;
                if (isNew)
                    objContext.Emails.Add(email);
                objContext.SaveChanges();
                #region System Change Log
                SystemChangeLog objSCL = new SystemChangeLog();
                long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                objSCL.UsernameTxt = objuser.UserNameTxt;
                objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                objSCL.ModuleTxt = "Email";
                objSCL.LogTypeTxt = isNew ? "Add" : "Update";
                objSCL.NotesTxt = isNew ? " Email details Added" : "Email Details Updated";
                objSCL.LogDateTime = DateTime.Now;
                objContext.SystemChangeLogs.Add(objSCL);
                objContext.SaveChanges();
                objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                var newResult = (from x in objContext.Emails
                                 where x.EmailID == email.EmailID
                                 select x);
                DataTable dtNew = LINQResultToDataTable(newResult);
                foreach (DataColumn col in dtNew.Columns)
                {
                    if (model.EmailID > 0)
                    {
                        if (dtOld.Rows[0][col.ColumnName].ToString() != dtNew.Rows[0][col.ColumnName].ToString())
                        {
                            SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                            objSCLD.ChangeLogID = objSCL.ChangeLogID;
                            objSCLD.FieldNameTxt = col.ColumnName.ToString();
                            objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                            objSCLD.NewValueTxt = dtNew.Rows[0][col.ColumnName].ToString();
                            objContext.SystemChangeLogDetails.Add(objSCLD);
                            objContext.SaveChanges();
                        }
                    }
                    else
                    {
                        SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                        objSCLD.ChangeLogID = objSCL.ChangeLogID;
                        objSCLD.FieldNameTxt = col.ColumnName.ToString();
                        objSCLD.OldValueTxt = "";
                        objSCLD.NewValueTxt = dtNew.Rows[0][col.ColumnName].ToString();
                        objContext.SystemChangeLogDetails.Add(objSCLD);
                        objContext.SaveChanges();
                    }
                }
                #endregion
                TempData["AlertMessage"] = emailTypeName + " details " + (model.EmailID == 0 ? "saved" : "updated") + " successfully.";
            }
            else
            {
                rvd.Add("pagesize", null);
                rvd.Add("page", null);
                rvd.Add("emailtype", emailTypeID);
                rvd.Add("eid", EncryptDecrypt.Encrypt("0"));
                pagedViewModel.EmailTxt = "";
                return View(pagedViewModel);
            }
            var Column = Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "EmailTxt";
            var Direction = Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Ascending";
            return RedirectToAction("Emails", "Email", new { et = emailTypeID, page = page, pagesize = pagesize, Column = Column, Direction = Direction });
        }

        /// <summary>
        /// Only Authorize user will have rights to perform changes into email data.
        /// Checks whether email is not in use. 
        /// If it is in use then system will display message saying as "already in use."
        /// Else delete the corresponding email and bind the grid.
        /// </summary>
        /// <param name="id">Email Primary Key.</param>
        /// <param name="et">Defines type of email From/to email.</param>
        /// <returns>Construct a redirect url to a specific action/controller in JSOn Format and use the route table to generate the correct URL.</returns>
        [HttpPost]
        [Authorize]
        public JsonResult Delete(string eid, int et)
        {
            Session["Edit/Delete"] = "Edit";
            if (!string.IsNullOrEmpty(eid))
            {
                int id = 0;
                //Decrypt email ID
                if (!string.IsNullOrEmpty(eid))
                {
                    id = Convert.ToInt32(EncryptDecrypt.Decrypt(eid));
                }
                #region System Change Log
                var oldresult = (from a in objContext.Emails
                                 where a.EmailID == id
                                 select a).ToList();
                DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                #endregion
                var checkexists = objContext.FormEmails.Where(x => x.EmailID == id).FirstOrDefault();
                if (checkexists == null)
                {
                    try
                    {
                        var obj = objContext.Emails.Where(x => x.EmailID == id).FirstOrDefault();
                        if(obj!=null)
                        {
                            obj.IsDeletedInd = true;
                            objContext.SaveChanges();
                        }
                        
                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = "Email";
                        objSCL.LogTypeTxt = "Delete";
                        objSCL.NotesTxt = "Email details deleted";
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();
                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.Emails
                                         where x.EmailID == id
                                         select x);
                        DataTable dtNew = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(newResult);
                        foreach (DataColumn col in dtNew.Columns)
                        {
                            // if(objSCL)
                            if (dtOld.Rows[0][col.ColumnName].ToString() != dtNew.Rows[0][col.ColumnName].ToString())
                            {
                                SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                                objSCLD.ChangeLogID = objSCL.ChangeLogID;
                                objSCLD.FieldNameTxt = col.ColumnName.ToString();
                                objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                                objSCLD.NewValueTxt = dtNew.Rows[0][col.ColumnName].ToString();
                                objContext.SystemChangeLogDetails.Add(objSCLD);
                                objContext.SaveChanges();
                            }
                        }
                        #endregion
                    }
                    catch
                    {

                    }
                    TempData["AlertMessage"] = (et == 1 ? "From " : "To ") + "Email details deleted successfully.";
                }
                else
                {
                    TempData["Message"] = "Email is in use, cannot be deleted.";
                }
            }
            var count = objContext.Emails.Where(x => x.EmailTypeID == et).Count();
            var Column = Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "EmailTxt";
            var Direction = Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Ascending";
            var page = Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : "1";
            var pagesize = Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : "10";
            return Json(Url.Action("Emails", "Email", new { et = et, page = Convert.ToInt32(page), pagesize = Convert.ToInt32(pagesize), Column = Column, Direction = Direction, eid = EncryptDecrypt.Encrypt("0") }));
        }
        #endregion
    }
}