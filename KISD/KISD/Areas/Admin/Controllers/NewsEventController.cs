using KISD.Areas.Admin.Models;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using static KISD.Areas.Admin.Models.Common;
using NewsEventTypeAlias = KISD.Areas.Admin.Models.GalleryListingService.TypeMaster;


namespace KISD.Areas.Admin.Controllers
{
    public class NewsEventController : Controller
    {
        private NewsEventServices _service;
        db_KISDEntities objContext = new db_KISDEntities();
        /// <summary>
        /// Code to create instance Image Service class in constructor
        /// </summary>
        public NewsEventController()
        {
            _service = new NewsEventServices();
        }

        [Authorize]
        /// <summary>
        /// this method will show the Gallery listing with all type master.
        /// </summary>
        /// <param name="Page">this parameter is used to get page number to be shown.</param>
        /// <param name="PageSize">this parameter is used to get no of recorde to be shown.</param>
        /// <param name="gridSortOptions">this parameter is used to get grid sorting option.</param>
        /// <param name="glt">this parameter is used to get type id of the gallery listing i.e. 1,2 or 3</param>
        /// <param name="CategoryId">show the category id in case of image type=3 i.e Photo Gallery</param>
        /// <param name="formCollection">this parameter is used to get controls collection on the page.</param>
        /// <param name="ObjResult"></param>
        /// <returns>view to enter image details.</returns>
        public ActionResult Index(int? Page, int? PageSize, GridSortOptions gridSortOptions, string mt, string pid, FormCollection formCollection, string ObjResult, PagedViewModel<NewsEvent> model,string command)
        {
            var db_obj = new db_KISDEntities();

            #region Check Tab is Accessible or Not
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Masters), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin) || RoleID == Convert.ToInt32(UserType.DepartmentUser)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            //Check for valid ImageTypeID
            if (mt == null)
            {
                return RedirectToAction("Index", "Home");
            }
            //decrypt image type id(it)
            if (!string.IsNullOrEmpty(Convert.ToString(mt)))
            {
                mt = Convert.ToString(EncryptDecrypt.Decrypt(mt));
            }
            //decrypt image type id(it)
            if (!string.IsNullOrEmpty(Convert.ToString(pid)))
            {
                pid = Convert.ToString(EncryptDecrypt.Decrypt(pid));
            }
            else
            {
                pid = "0";
            }
            TempData["CroppedImage"] = null;
            var NewsEventType = mt != null ? Convert.ToInt64(mt) : Convert.ToInt64(NewsEventTypeAlias.News);
            ViewBag.TypeMasterID = NewsEventType;

            #region Ajax Call
            if (ObjResult != null)
            {
                AjaxRequest objAjaxRequest = JsonConvert.DeserializeObject<AjaxRequest>(ObjResult);//Convert json String to object Model
                if (objAjaxRequest.ajaxcall != null && !string.IsNullOrEmpty(objAjaxRequest.ajaxcall) && ObjResult != null && !string.IsNullOrEmpty(ObjResult))
                {
                    if (objAjaxRequest.ajaxcall == "paging")//Ajax Call type = paging i.e. Next|Previous|Back|Last
                    {
                        Session["pageNo"] = Page;// stores the page no for status
                    }
                    else if (objAjaxRequest.ajaxcall == "sorting")//Ajax Call type = sorting i.e. column sorting Asc or Desc
                    {
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                        Session["GridSortOption"] = gridSortOptions;
                        PageSize = (Session["PageSize"] != null ? Convert.ToInt32(Session["PageSize"].ToString()) : PageSize);
                    }
                    else if (objAjaxRequest.ajaxcall == "ddlPaging")//Ajax Call type = drop down paging i.e. drop down value 10, 25, 50, 100, ALL
                    {
                        Session["PageSize"] = (Request.QueryString["pagesize"] != null ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : PageSize);
                        Session["GridSortOption"] = gridSortOptions;
                        Session["pageNo"] = Page;
                    }
                    else if (objAjaxRequest.ajaxcall == "status")//Ajax Call type = status i.e. Active/Inactive
                    {
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    else if (objAjaxRequest.ajaxcall == "displayorder")//Ajax Call type = Display Order i.e. drop down values
                    {
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    model.strFromDate = objAjaxRequest.qs_FilterFromDate;
                    model.strToDate = objAjaxRequest.qs_FilterToDate;
                    model.status = objAjaxRequest.qs_FilterUserType;
                    model.Title = objAjaxRequest.qs_FilterUserName;
                    model.strDate = objAjaxRequest.qs_Date;
                    model.strCreateDate = objAjaxRequest.qs_CreateDate;
                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
                }

                //Ajax Call for update status for images
                if (objAjaxRequest.qs_checkboxselected != null && objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && ObjResult != null && !string.IsNullOrEmpty(ObjResult) && objAjaxRequest.qs_checkboxselected != null && objAjaxRequest.qs_value != null && objAjaxRequest.qs_Type != null && objAjaxRequest.qs_shownonhomevalue != null &&
                        !string.IsNullOrEmpty(objAjaxRequest.qs_checkboxselected)
                        && !string.IsNullOrEmpty(objAjaxRequest.qs_value)
                        && !string.IsNullOrEmpty(objAjaxRequest.qs_Type)
                        && !string.IsNullOrEmpty(objAjaxRequest.qs_shownonhomevalue) && objAjaxRequest.hfvalue.ToString().Trim().ToLower() != "displayOrder".Trim().ToLower())
                {
                    var NewsEventID = System.Convert.ToInt64(objAjaxRequest.hfid);
                    var NewsEvent = objContext.NewsEvents.Find(NewsEventID);
                    if (NewsEvent != null)
                    {
                        #region System Change Log
                        var oldresult = (from a in objContext.NewsEvents
                                         where a.NewsEventID == NewsEventID
                                         select a).ToList();
                        DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                        #endregion
                        #region Check For Show On Home
                        var isValid = true;
                        if (objAjaxRequest.qs_value == "0" && objAjaxRequest.qs_shownonhomevalue == "1")
                        {
                            if (objAjaxRequest.qs_Type == "status")
                                TempData["Message"] = "Show on Home "+ _service.GetNewsEventType(NewsEventType) + " cannot be set to Inactive.";
                            else
                                TempData["Message"] = "Inactive "+ _service.GetNewsEventType(NewsEventType) + " cannot be set to Show on Home.";
                            isValid = false;
                        }
                        #endregion
                        if (isValid)
                        {
                            if (objAjaxRequest.qs_Type == "status")
                            {
                                NewsEvent.StatusInd = objAjaxRequest.hfvalue == "1";
                                objContext.SaveChanges();
                                TempData["AlertMessage"] = "Status updated successfully.";
                            }
                            else
                            {

                                NewsEvent.ShowOnHomeInd = objAjaxRequest.hfvalue == "1";
                                var TypeMasterID= Convert.ToInt64(mt);
                                var showhomemax = objContext.NewsEvents.Where(m => m.TypeMasterID == TypeMasterID && (m.IsDeletedInd == null || m.IsDeletedInd == false) && m.StatusInd == true && m.ShowOnHomeInd == true).ToList();
                                if (showhomemax.Count >= 4 && NewsEvent.ShowOnHomeInd.Value && (TypeMasterID== Convert.ToInt32(NewsEventTypeAlias.News) || TypeMasterID == Convert.ToInt32(NewsEventTypeAlias.Events)))
                                {
                                    NewsEvent.ShowOnHomeInd = false;
                                    //TempData["Message"] = "Only 4 " + _service.GetNewsEventType(NewsEventType) + " can be shown at home.";
                                    TempData["Message"] = "More than four " + _service.GetNewsEventType(NewsEventType) + " cannot be set as Active on Home.";
                                }
                                else
                                {
                                    objContext.SaveChanges();
                                    TempData["AlertMessage"] = "Show on Home updated successfully.";
                                }
                            }
                            objAjaxRequest.qs_checkboxselected = null;
                            objAjaxRequest.qs_Type = null;
                            objAjaxRequest.qs_value = null;
                            objAjaxRequest.qs_shownonhomevalue = null;
                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "News Event";
                            objSCL.LogTypeTxt = NewsEvent.NewsEventID > 0 ? "Update" : "Add";
                            objSCL.NotesTxt = "New Event Details" + (NewsEvent.NewsEventID > 0 ? " updated for " : "  added for ") + NewsEvent.TitleTxt;
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();
                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in objContext.NewsEvents
                                             where x.NewsEventID == NewsEvent.NewsEventID
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
                            objAjaxRequest.hfid = null;//remove parameter value
                            objAjaxRequest.hfvalue = null;//remove parameter value
                            PageSize = ((Request.QueryString["pagesize"] != null && Request.QueryString["pagesize"].ToString() != "All") ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : PageSize);
                            Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                            gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                        }
                    }
                }
                else
                {
                    TempData["Message"] = string.Empty;
                }
                ObjResult = string.Empty;
            }
            #endregion Ajax Call

            ViewBag.Title = ViewBag.PageTitle = _service.GetNewsEventType(NewsEventType);

            //This section is used to retain the values of page , pagesize and gridsortoption on complete page post back(Edit, Dlete)
            if (!Request.IsAjaxRequest() && Session["Edit/Delete"] != null && !string.IsNullOrEmpty(Session["Edit/Delete"].ToString()))
            {
                PageSize = (Session["PageSize"] != null ? Convert.ToInt32(Session["PageSize"]) : Models.Common._pageSize);
                Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"]) : Models.Common._currentPage);
                gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                Session["Edit/Delete"] = null;
            }
            else if (!Request.IsAjaxRequest() && Session["Edit/Delete"] == null)
            {
                gridSortOptions.Column = "EventCreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column == "TitleTxt" || gridSortOptions.Column == "EventCreateDate" || gridSortOptions.Column == "EventDate")
            {

            }
            else
            {
                gridSortOptions.Column = "EventCreateDate";
            }
            //.. Code for get records as page view model
            var pagesize = PageSize.HasValue ? PageSize.Value : Models.Common._pageSize;
            var page = Page.HasValue ? Page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;
            var pagedViewModel = new PagedViewModel<NewsEventModel>
            {
                ViewData = ViewData,
                Query = _service.GetNewsEventView(Convert.ToInt64(mt), Convert.ToInt64(pid)).AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "EventCreateDate",
                Page = page,
                PageSize = pagesize,
            };
            if (!string.IsNullOrEmpty(model.Title) && (string.IsNullOrEmpty(command) || command != "Cancel"))
            {
                pagedViewModel.AddFilter("title", model.Title,
            a => a.TitleTxt.Contains(model.Title));
            }
            if(model.status != "All" && (string.IsNullOrEmpty(command) || command != "Cancel"))
            {
                pagedViewModel.AddFilter("status", model.status, a => a.StatusInd.ToString() == model.status);
            }
            if (!string.IsNullOrEmpty(model.strDate) && (string.IsNullOrEmpty(command) || command != "Cancel"))
            {
                model.dtDate = Convert.ToDateTime(model.strDate);
                pagedViewModel.AddFilter("date", model.dtDate, a => DbFunctions.TruncateTime(a.EventDate) == model.dtDate.Date);
            }
            if (!string.IsNullOrEmpty(model.strFromDate) && !string.IsNullOrEmpty(model.strToDate) && (string.IsNullOrEmpty(command) || command != "Cancel"))
            {
                model.FromDate = Convert.ToDateTime(model.strFromDate);
                model.ToDate = Convert.ToDateTime(model.strToDate);
                pagedViewModel.AddFilter("daterange", model.FromDate, a => (model.FromDate.Date >= DbFunctions.TruncateTime(a.DisplayStartDate.Value) && model.FromDate.Date <= DbFunctions.TruncateTime(a.DisplayEndDate.Value) || (model.ToDate.Date >= DbFunctions.TruncateTime(a.DisplayStartDate.Value) && model.ToDate.Date <= DbFunctions.TruncateTime(a.DisplayEndDate.Value))
                            || (DbFunctions.TruncateTime(a.DisplayStartDate) >= model.FromDate.Date && DbFunctions.TruncateTime(a.DisplayStartDate) <= model.ToDate.Date) || (DbFunctions.TruncateTime(a.DisplayEndDate) >= model.ToDate.Date && DbFunctions.TruncateTime(a.DisplayEndDate) <= model.ToDate.Date)));
            }
            if (!string.IsNullOrEmpty(model.strCreateDate) && (string.IsNullOrEmpty(command) || command != "Cancel"))
            {
                model.dtCreateDate = Convert.ToDateTime(model.strCreateDate);
                pagedViewModel.AddFilter("dateadded", model.strCreateDate, a => a.EventCreateDate == model.dtCreateDate);
            }
            pagedViewModel.Setup();
            
            
            
            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("NewsEventPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }

        [Authorize]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="glt">Newsevent type id</param>
        /// <param name="glid"> Listing id</param>
        /// <param name="NewsEventID"></param>
        /// <returns></returns>
        public ActionResult Create(string mt, string nid, string pid)
        {
            var objNewsEventModel = new NewsEventModel();

            #region Check Tab is Accessible or Not
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Masters), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin) || RoleID == Convert.ToInt32(UserType.DepartmentUser)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            //Check for valid ImageTypeID
            ViewBag.ListingTypeId = mt;
            ViewBag.Title = "";
            if ((Request.QueryString["mt"] == null) && (Request.QueryString["mt"] == null))
            {
                return RedirectToAction("Index", "Home");
            }

            //decrypt image type id(it)
            mt = !string.IsNullOrEmpty(Convert.ToString(mt)) ? EncryptDecrypt.Decrypt(mt) : "0";

            //decrypt image id(iid)
            nid = !string.IsNullOrEmpty(Convert.ToString(nid)) ? EncryptDecrypt.Decrypt(nid) : "0";

            //decrypt image type id(pid)
            pid = !string.IsNullOrEmpty(Convert.ToString(pid)) ? EncryptDecrypt.Decrypt(pid) : "0";

            int NewsEventID = Convert.ToInt32(nid);
            if (NewsEventID > 0 && objContext.NewsEvents.Where(x => x.NewsEventID == NewsEventID && x.IsDeletedInd == true).Any())
            {
                return RedirectToAction("Index", "Home");
            }
            Session["Edit/Delete"] = "Edit";
            ViewBag.PageTitle = (nid == "0" ? "Add " : "Edit ") + (_service.GetNewsEventType(Convert.ToInt64(mt))) +" Details"; 
            ViewBag.Submit = (nid == "0" ? "Save" : "Update");
            ViewBag.ListingTypeId = mt;
            ViewBag.Title = (nid == "0" ? "Add " : "Edit ") + (_service.GetNewsEventType(Convert.ToInt64(mt))) + " Details";
            ViewBag.ImageTypeTitle = _service.GetNewsEventType(Convert.ToInt64(mt));
            objNewsEventModel.EventCreateDate = DateTime.Now;
            ViewBag.Date = DateTime.Now.ToShortDateString();
            ViewBag.StartDateStr = "";
            ViewBag.EventDate = DateTime.Now.ToString("MM/dd/yyyy");
            ViewBag.EventEndDate = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt");
            ViewBag.EndDateStr = "";
            objNewsEventModel.TypeMasterID = Convert.ToInt64(mt);
            if(!string.IsNullOrEmpty(pid))
                objNewsEventModel.DepartmentID = Convert.ToInt64(pid);
            if (Convert.ToInt32(nid) > 0)
            {
                var NewsEvent = (from u in objContext.NewsEvents
                                 where u.NewsEventID == NewsEventID
                                 select u).FirstOrDefault();
                if (NewsEvent != null)
                {
                    objNewsEventModel.NewsEventID = NewsEvent.NewsEventID;
                    objNewsEventModel.TitleTxt = NewsEvent.TitleTxt;
                    objNewsEventModel.ImageURLTxt = NewsEvent.ImageURLTxt;
                    objNewsEventModel.IsExternalLinkInd = NewsEvent.IsExternalLinkInd;
                    objNewsEventModel.ExternalLinkTargetInd = NewsEvent.ExternalLinkTargetInd;
                    objNewsEventModel.AuthorNameTxt = NewsEvent.AuthorNameTxt;
                    objNewsEventModel.AbstractTxt = NewsEvent.AbstractTxt;
                    objNewsEventModel.DescriptionTxt = NewsEvent.DescriptionTxt;
                    objNewsEventModel.RightSectionTitleTxt = NewsEvent.RightSectionTitleTxt;
                    objNewsEventModel.RightSectionAbstractTxt = NewsEvent.RightSectionAbstractTxt;
                    objNewsEventModel.BannerImageID = NewsEvent.BannerImageID;
                    objNewsEventModel.AltBannerImageTxt = NewsEvent.AltBannerImageTxt;
                    objNewsEventModel.BannerImageAbstractTxt = NewsEvent.BannerImageAbstractTxt;
                    objNewsEventModel.EventDate = NewsEvent.EventDate;
                    objNewsEventModel.DisplayStartDate = NewsEvent.DisplayStartDate;
                    objNewsEventModel.DisplayEndDate = NewsEvent.DisplayEndDate;
                    objNewsEventModel.ShowOnHomeInd = NewsEvent.ShowOnHomeInd;
                    objNewsEventModel.PageMetaTitleTxt = NewsEvent.PageMetaTitleTxt;
                    objNewsEventModel.PageMetaDescriptionTxt = NewsEvent.PageMetaDescriptionTxt;
                    objNewsEventModel.StatusInd = NewsEvent.StatusInd;
                    objNewsEventModel.DepartmentID = NewsEvent.DepartmentID;
                    objNewsEventModel.EventCreateDate = Convert.ToDateTime(NewsEvent.EventCreateDate.Value.ToShortDateString());
                    objNewsEventModel.CreateDate = NewsEvent.CreateDate;
                    objNewsEventModel.CreateByID = NewsEvent.CreateByID;
                    objNewsEventModel.LastModifyDate = NewsEvent.LastModifyDate;
                    objNewsEventModel.LastModifyByID = NewsEvent.LastModifyByID;
                    objNewsEventModel.IsDeletedInd = NewsEvent.IsDeletedInd;
                    objNewsEventModel.TypeMasterID = NewsEvent.TypeMasterID;
                    objNewsEventModel.IsFacebookSharingInd = NewsEvent.IsFacebookSharingInd;
                    objNewsEventModel.IsTwitterSharingInd = NewsEvent.IsTwitterSharingInd;
                    objNewsEventModel.IsGooglePlusSharingInd = NewsEvent.IsGooglePlusSharingInd;
                    if (objNewsEventModel.IsExternalLinkInd == true)
                    {
                        objNewsEventModel.URLTxt = NewsEvent.PageURLTxt;
                    }
                    else
                    {
                        objNewsEventModel.PageURLTxt = NewsEvent.PageURLTxt;
                    }
                    objNewsEventModel.IsRecurringInd = NewsEvent.IsRecurringInd;
                    objNewsEventModel.RecurrenceTypeNbr = NewsEvent.RecurrenceTypeNbr;
                    objNewsEventModel.IsSundayInd = NewsEvent.IsSundayInd;
                    objNewsEventModel.IsMondayInd = NewsEvent.IsMondayInd;
                    objNewsEventModel.IsTuesdayInd = NewsEvent.IsTuesdayInd;
                    objNewsEventModel.IsWednesdayInd = NewsEvent.IsWednesdayInd;
                    objNewsEventModel.IsThursdayInd = NewsEvent.IsThursdayInd;
                    objNewsEventModel.IsFridayInd = NewsEvent.IsFridayInd;
                    objNewsEventModel.IsSaturdayInd = NewsEvent.IsSaturdayInd;
                    objNewsEventModel.MonthModeNbr = NewsEvent.MonthModeNbr;
                    objNewsEventModel.MonthdayNbr = NewsEvent.MonthdayNbr;
                    objNewsEventModel.MonthondayNbr = NewsEvent.MonthondayNbr;
                    objNewsEventModel.MonthonNbr = NewsEvent.MonthonNbr;
                    objNewsEventModel.EventsEndOnDate = NewsEvent.EventsEndOnDate;

                    ViewBag.StatusInd = GetStatusData(objNewsEventModel.StatusInd.Value ? "1" : "0");
                    ViewBag.Date = objNewsEventModel.EventCreateDate.Value.ToShortDateString();
                    ViewBag.ShowonHomeInd = Models.Common.GetShowonHomeDataBoolean(NewsEvent.ShowOnHomeInd != null && NewsEvent.ShowOnHomeInd.Value ? "True" : "False");
                    ViewBag.StartDateStr = NewsEvent.DisplayStartDate != null? NewsEvent.DisplayStartDate.Value.ToString("MM/dd/yyyy hh:mm tt"):null;
                    ViewBag.EndDateStr = NewsEvent.DisplayEndDate != null ? NewsEvent.DisplayEndDate.Value.ToString("MM/dd/yyyy hh:mm tt"):null;
                    ViewBag.MonthondayNbr = GetWeekData(NewsEvent.MonthondayNbr.HasValue ? NewsEvent.MonthondayNbr.Value.ToString() : "0");
                    ViewBag.MonthonNbr = GetmonthdayData(NewsEvent.MonthonNbr.HasValue ? NewsEvent.MonthonNbr.Value.ToString() : "1");
                    if (NewsEvent.EventDate.HasValue)
                    {
                        ViewBag.EventDate = NewsEvent.EventDate.Value.ToString("MM/dd/yyyy hh:mm tt");
                    }
                    if (NewsEvent.EventsEndOnDate.HasValue)
                    {
                        ViewBag.EventEndDate = NewsEvent.EventsEndOnDate.Value.ToString("MM/dd/yyyy hh:mm tt");
                    }
                }
            }
            else
            {
                objNewsEventModel.IsRecurringInd = false;
                ViewBag.StatusInd = GetStatusData(string.Empty);
                ViewBag.ShowonHomeInd = Models.Common.GetShowonHomeDataBoolean("False");
                ViewBag.MonthondayNbr = GetWeekData(string.Empty);
                ViewBag.MonthonNbr = GetmonthdayData(string.Empty);
            }
            var InnerImagesTitle = Models.Common.GetInnerImages();
            ViewBag.InnerImagesTitle = InnerImagesTitle;//get all the inner image titles
            return View(objNewsEventModel);
        }

        /// <summary>
        /// this method wil post the details of image filled by the admin.
        /// </summary>
        /// <param name="command">command name whether Save or Cancel.</param>
        /// <param name="fm">controls collection on the page.</param>
        /// <param name="model">object of Image model</param>
        /// <param name="CategoryID">show the category id in case of image type=3 i.e Photo Gallery</param>
        /// <returns>view with status message.</returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(string command, FormCollection fm, NewsEventModel model, int? CategoryID)
        {
            try
            {
                var width = 260;
                HttpPostedFileBase file = Request.Files.Count > 0 ? Request.Files[0] : null;
                ViewBag.Title = (model.NewsEventID == 0 ? "Add " : "Edit ") + _service.GetNewsEventType(model.TypeMasterID.Value) +" Details";
                ViewBag.StatusInd = GetStatusData(string.Empty);
                ViewBag.Submit = (model.NewsEventID == 0 ? "Save" : "Update");
                ViewBag.ListingTypeId = model.TypeMasterID;
                ViewBag.Date = model.EventCreateDate.Value.ToShortDateString().Trim();
                ViewBag.StartDateStr = model.DisplayStartDate != null?Convert.ToString(model.DisplayStartDate):"";
                ViewBag.EndDateStr = model.DisplayEndDate != null ? Convert.ToString(model.DisplayEndDate) : "";
                if (model.TypeMasterID == Convert.ToInt32(NewsEventTypeAlias.Events) || model.TypeMasterID == Convert.ToInt32(NewsEventTypeAlias.ManageEvents))
                {
                    ViewBag.EventDate = model.EventDate.Value.ToString("MM/dd/yyyy");
                    if(model.TypeMasterID == Convert.ToInt32(NewsEventTypeAlias.ManageEvents))
                        ViewBag.EventEndDate = model.EventsEndOnDate.Value.ToString("MM/dd/yyyy hh:mm tt");
                }
                model.IsRecurringInd = model.IsRecurringInd;
                var rvd = new RouteValueDictionary();
                rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
                rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
                rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "EventCreateDate");
                rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
                rvd.Add("mt", EncryptDecrypt.Encrypt(Convert.ToString(model.TypeMasterID)));
                rvd.Add("pid", EncryptDecrypt.Encrypt(Convert.ToString(model.DepartmentID)));
                ViewBag.PageTitle = (model.NewsEventID == 0 ? "Add " : "Edit ") + _service.GetNewsEventType(model.TypeMasterID.Value) + " Details";
                ViewBag.ImageTypeTitle = _service.GetNewsEventType(model.TypeMasterID.Value);
                ViewBag.ismorethanthree = "0";
                var InnerImagesTitle = Models.Common.GetInnerImages();
                ViewBag.InnerImagesTitle = InnerImagesTitle;//get all the inner image titles
                var RightSectionTitle = (from db in objContext.RightSections
                                         where db.StatusInd == true
                                         select new { db.TitleTxt, db.RightSectionID }).ToList().OrderBy(x => x.TitleTxt);

                ViewBag.RightSectionTitle = RightSectionTitle;
                
                #region System Change Log
                DataTable dtOld;
                var oldresult = (from a in objContext.NewsEvents
                                 where a.NewsEventID == model.NewsEventID
                                 select a).ToList();
                dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                #endregion

                if (string.IsNullOrEmpty(command))
                {
                    NewsEvent objNewsEvent;
                    if (model.NewsEventID > 0)
                    {
                        objNewsEvent = objContext.NewsEvents.Find(model.NewsEventID);
                    }
                    else
                    {
                        objNewsEvent = new NewsEvent();
                        objNewsEvent.IsDeletedInd = false;
                    }
                    objNewsEvent.TypeMasterID = model.TypeMasterID;
                    objNewsEvent.StatusInd = fm["StatusInd"].ToString() == "True";
                    objNewsEvent.DepartmentID = model.DepartmentID;
                    ViewBag.StatusInd = GetStatusData(fm["StatusInd"].ToString() == "True" ? "True" : "False");
                    ViewBag.ShowonHomeInd = GetShowonHomeData(fm["ShowonHomeInd"] == "True" ? "True" : "False");
                    ViewBag.MonthondayNbr = GetWeekData(model.MonthondayNbr.HasValue ? model.MonthondayNbr.Value.ToString() : "0");
                    ViewBag.MonthonNbr = GetmonthdayData(model.MonthonNbr.HasValue ? model.MonthonNbr.Value.ToString() : "1");
                    
                    if (fm["StatusInd"] == "False" && fm["ShowonHomeInd"] == "True")
                    {
                        TempData["Message"] = "Inactive " + _service.GetNewsEventType(model.TypeMasterID.Value) + " cannot be set to Show on Home.";
                        return View(model);
                    }
                    //Code Restrict user to active only two or less than two listing in footer.
                    var ShowonHomeInd = 0;
                    if (model.NewsEventID == 0)
                        ShowonHomeInd = objContext.NewsEvents.Where(x => x.ShowOnHomeInd == true && x.TypeMasterID == model.TypeMasterID && x.StatusInd == true && (x.IsDeletedInd == false || x.IsDeletedInd==null)).Count();
                    else
                        ShowonHomeInd = objContext.NewsEvents.Where(x => x.ShowOnHomeInd == true && x.TypeMasterID == model.TypeMasterID && x.StatusInd == true && x.NewsEventID != model.NewsEventID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                    ViewBag.ismorethanthree = ShowonHomeInd;
                    if (ShowonHomeInd > 4 && (model.ShowOnHomeInd == true || fm["ShowonHomeInd"] == "True") && model.NewsEventID==0 && (model.TypeMasterID == Convert.ToInt32(NewsEventTypeAlias.Events) || model.TypeMasterID == Convert.ToInt32(NewsEventTypeAlias.News)))
                    {
                        TempData["Message"] = "More than four "+ _service.GetNewsEventType(model.TypeMasterID.Value) + " cannot be set as Active on Home.";
                        return View(model);
                    }
                    else if(ShowonHomeInd >= 4 && (model.ShowOnHomeInd == true || fm["ShowonHomeInd"] == "True") && model.NewsEventID > 0 && (model.TypeMasterID == Convert.ToInt32(NewsEventTypeAlias.Events) || model.TypeMasterID == Convert.ToInt32(NewsEventTypeAlias.News)))
                    {
                        TempData["Message"] = "More than four " + _service.GetNewsEventType(model.TypeMasterID.Value) + " cannot be set as Active on Home.";
                        return View(model);
                    }
                    if (fm["StatusInd"] == "False" && model.ShowOnHomeInd == true)
                    {
                        TempData["Message"] = "Show on home " + _service.GetNewsEventType(model.TypeMasterID.Value) + " cannot be set as inactive.";
                        return View(model);
                    }
                    if (!string.IsNullOrEmpty(model.TitleTxt))
                    {
                        //check for Events
                        if (model.TypeMasterID == Convert.ToInt64(NewsEventTypeAlias.ManageEvents))
                        {
                            var chkListingTitle = objContext.NewsEvents.Where(x => x.TitleTxt == model.TitleTxt.Trim()
                                                                       && x.TypeMasterID == model.TypeMasterID
                                                                       && x.DepartmentID == model.DepartmentID
                                                                       && x.NewsEventID != model.NewsEventID).Any();
                            if (chkListingTitle)//check image title on adding new image details or updating existing 1
                            {
                                TempData["CroppedImage"] = null;
                                ModelState.AddModelError("TitleTxt", model.TitleTxt + " title already exists.");
                                return View(model);
                            }
                        }
                        else
                        {
                            var chkListingTitle = objContext.NewsEvents.Where(x => x.TitleTxt == model.TitleTxt.Trim()
                                                                        && x.TypeMasterID == model.TypeMasterID
                                                                        && x.NewsEventID != model.NewsEventID).Any();
                            if (chkListingTitle)//check image title on adding new image details or updating existing 1
                            {
                                TempData["CroppedImage"] = null;
                                ModelState.AddModelError("TitleTxt", model.TitleTxt + " title already exists.");
                                return View(model);
                            }
                        }
                    }
                    if (model != null && !string.IsNullOrEmpty(model.PageURLTxt))
                    {
                        var count = 0;
                        count = objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.NewsEvents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.NewsEventID != model.NewsEventID && x.IsDeletedInd == false).Count();
                        count += objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.ListingID != model.NewsEventID && x.IsDeletedInd == false).Count();
                        count += objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.ExceptionOpportunities.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.Departments.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.RightSections.Where(x => x.ExternalLinkURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                        if (model.PageURLTxt.Trim().ToLower() == "error404")// Check for duplicate url and error404 url
                        {
                            count = count + 1;
                        }
                        if (count > 0)
                        {
                            ViewBag.FocusPageUrl = true;// Set focus on Pageurl Field if same url exist
                            if (model.PageURLTxt.ToLower().Trim() == "error404")//if user types url 'error404' below validation msg should display
                            {
                                ModelState.AddModelError("PageURLTxt", model.PageURLTxt + " URL is not allowed.");
                            }
                            else
                            {
                                ModelState.AddModelError("PageURLTxt", model.PageURLTxt + " URL already exists.");
                            }
                            return View(model);
                        }
                    }
                    objNewsEvent.NewsEventID = model.NewsEventID;
                    objNewsEvent.TitleTxt = model.TitleTxt;
                    objNewsEvent.ImageURLTxt = model.ImageURLTxt;
                    objNewsEvent.IsExternalLinkInd = model.IsExternalLinkInd;
                    objNewsEvent.PageURLTxt = objNewsEvent.IsExternalLinkInd == true ? model.URLTxt : model.PageURLTxt;
                    objNewsEvent.ExternalLinkTargetInd = model.ExternalLinkTargetInd;
                    objNewsEvent.AuthorNameTxt = model.AuthorNameTxt;
                    objNewsEvent.AbstractTxt = model.AbstractTxt;
                    objNewsEvent.DescriptionTxt = model.DescriptionTxt;
                    objNewsEvent.RightSectionTitleTxt = model.RightSectionTitleTxt;
                    objNewsEvent.RightSectionAbstractTxt = model.RightSectionAbstractTxt;
                    objNewsEvent.BannerImageID = model.BannerImageID;
                    objNewsEvent.AltBannerImageTxt = model.AltBannerImageTxt;
                    objNewsEvent.BannerImageAbstractTxt = model.BannerImageAbstractTxt;
                    objNewsEvent.EventDate = model.EventDate;
                    objNewsEvent.DisplayStartDate = model.DisplayStartDate != null ? model.DisplayStartDate : null;
                    objNewsEvent.DisplayEndDate = model.DisplayEndDate != null ? model.DisplayEndDate : null;
                    objNewsEvent.ShowOnHomeInd = model.ShowOnHomeInd;
                    objNewsEvent.PageMetaTitleTxt = model.PageMetaTitleTxt;
                    objNewsEvent.PageMetaDescriptionTxt = model.PageMetaDescriptionTxt;
                    objNewsEvent.StatusInd = model.StatusInd;
                    objNewsEvent.DepartmentID = model.DepartmentID;
                    objNewsEvent.EventCreateDate = Convert.ToDateTime(model.EventCreateDate.Value.ToShortDateString());
                    objNewsEvent.CreateDate = model.NewsEventID > 0 ? objNewsEvent.CreateDate : DateTime.Now; ;
                    objNewsEvent.CreateByID = model.NewsEventID > 0 ? objNewsEvent.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    objNewsEvent.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    objNewsEvent.LastModifyDate = DateTime.Now;
                    objNewsEvent.IsDeletedInd = false;
                    objNewsEvent.TypeMasterID = model.TypeMasterID;
                    objNewsEvent.IsFacebookSharingInd = model.IsFacebookSharingInd;
                    objNewsEvent.IsTwitterSharingInd = model.IsTwitterSharingInd;
                    objNewsEvent.IsGooglePlusSharingInd = model.IsGooglePlusSharingInd;

                    objNewsEvent.IsRecurringInd = model.IsRecurringInd;
                    if (objNewsEvent.IsRecurringInd.HasValue && objNewsEvent.IsRecurringInd.Value)
                    {
                        objNewsEvent.RecurrenceTypeNbr = model.RecurrenceTypeNbr;
                        if (objNewsEvent.RecurrenceTypeNbr == 2)//For Weekly
                        {
                            objNewsEvent.IsSundayInd = model.NoIsSunday;
                            objNewsEvent.IsMondayInd = model.NoIsMonday;
                            objNewsEvent.IsTuesdayInd = model.NoIsTuesday;
                            objNewsEvent.IsWednesdayInd = model.NoIsWednesday;
                            objNewsEvent.IsThursdayInd = model.NoIsThursday;
                            objNewsEvent.IsFridayInd = model.NoIsFriday;
                            objNewsEvent.IsSaturdayInd = model.NoIsSaturday;
                        }
                        if (objNewsEvent.RecurrenceTypeNbr == 3)// For Monthly
                        {
                            objNewsEvent.MonthModeNbr = model.MonthModeNbr;
                            if (objNewsEvent.MonthModeNbr == 1)// For One Date
                            {
                                objNewsEvent.MonthdayNbr = model.MonthdayNbr;
                                objNewsEvent.MonthondayNbr = null;
                                objNewsEvent.MonthonNbr = null;
                            }
                            if (objNewsEvent.MonthModeNbr == 2)// For One Week Day
                            {
                                objNewsEvent.MonthdayNbr = model.MonthdayNbr;
                                objNewsEvent.MonthondayNbr = model.MonthondayNbr;
                                objNewsEvent.MonthonNbr = model.MonthonNbr;
                            }
                        }
                        objNewsEvent.EventsEndOnDate = model.EventsEndOnDate;
                    }

                    ViewBag.StatusInd = GetStatusData(fm["StatusInd"].ToString() == "True" ? "True" : "False");
                    ViewBag.Date = model.EventCreateDate.Value.ToShortDateString();
                    if (TempData["CroppedImage"] != null)
                    {
                        #region Cropped Image
                        var croppedfile = new System.IO.FileInfo(Server.MapPath(TempData["CroppedImage"].ToString()));
                        var fileName = croppedfile.Name;
                        croppedfile = null;
                        var sourcePath = Server.MapPath(TempData["CroppedImage"].ToString());
                        var targetPath = Request.PhysicalApplicationPath + "WebData\\";
                        System.IO.File.Copy(System.IO.Path.Combine(sourcePath.Replace(fileName, ""), fileName), System.IO.Path.Combine(targetPath + "images\\", fileName), true);
                        try
                        {
                            Models.Common.DeleteImage(Server.MapPath(TempData["CroppedImage"].ToString()));
                        }
                        catch
                        {
                        }
                        objNewsEvent.ImageURLTxt = "~/WebData/images/" + fileName;
                        var fileExtension = fileName.Substring(fileName.LastIndexOf("."), fileName.Length - fileName.LastIndexOf("."));
                        var strPath = Request.PhysicalApplicationPath + "WebData\\images\\" + fileName;
                        var myImage = Models.Common.CreateImageThumbnail(strPath, width);
                        myImage.Save(Request.PhysicalApplicationPath + "WebData\\thumbnails\\" + fileName,
                                       fileExtension.ToLower() == ".png" ?
                                       System.Drawing.Imaging.ImageFormat.Png :
                                       fileExtension.ToLower() == ".gif" ?
                                       System.Drawing.Imaging.ImageFormat.Gif :
                                       System.Drawing.Imaging.ImageFormat.Jpeg
                                       );
                        myImage.Dispose();
                        TempData["CroppedImage"] = null;
                        #endregion
                    }
                    else
                    {
                        objNewsEvent.ImageURLTxt = objNewsEvent.ImageURLTxt;
                    }
                    if (model.NewsEventID > 0)
                    {
                        TempData["AlertMessage"] = _service.GetNewsEventType(model.TypeMasterID.Value) + " details updated successfully.";
                        objContext.SaveChanges();
                    }
                    else
                    {
                        objContext.NewsEvents.Add(objNewsEvent);
                        objContext.SaveChanges();
                        TempData["AlertMessage"] = _service.GetNewsEventType(model.TypeMasterID.Value) + " details saved successfully.";
                    }
                    
                    #region System Change Log
                    SystemChangeLog objSCL = new SystemChangeLog();
                    long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                    objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                    objSCL.UsernameTxt = objuser.UserNameTxt;
                    objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                    objSCL.ModuleTxt = "News Event";
                    objSCL.LogTypeTxt = objNewsEvent.NewsEventID > 0 ? "Update" : "Add";
                    objSCL.NotesTxt = "New Event Details" + (objNewsEvent.NewsEventID > 0 ? " updated for " : "  added for ") + objNewsEvent.TitleTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();
                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var newResult = (from x in objContext.NewsEvents
                                     where x.NewsEventID == model.NewsEventID
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
                    return RedirectToAction("Index", "NewsEvent", rvd);
                }
                else
                {
                    return RedirectToAction("Index", "NewsEvent", rvd);
                }
            }
            catch (Exception ex)
            {
                return View(model);
            }
        }

        /// <summary>
        /// This method is used to delete the image from database.It also chacks wheather image is in use or not.
        /// If image is in use then it return to view and show message "image is in use, cannot be deleted." else it delte the image and return to thye view.
        /// </summary>
        /// <param name="mt">This parameter is used to get the imagetypeid</param>
        /// <param name="nid">This parameter is used to get the imageid</param>
        /// <param name="fm">this parameter is used to get the form control values from view </param>
        /// <param name="returnUrl"></param>
        /// <returns>This method return the Json result (url) that will be passed to the Ajax post method on client side.</returns>
        [HttpPost]
        public JsonResult Delete(string mt, string nid, int? CategoryID, FormCollection fm)
        {
            //decrypt image type id(it)
            mt = !string.IsNullOrEmpty(Convert.ToString(mt)) ? EncryptDecrypt.Decrypt(mt) : "0";
            //decrypt image id(iid)
            nid = !string.IsNullOrEmpty(Convert.ToString(nid)) ? EncryptDecrypt.Decrypt(nid) : "0";

            //.. Code for get the route value directory
            RouteValueDictionary rvd = new RouteValueDictionary();
            ViewBag.ListingTypeId = mt;
            ViewBag.Title = _service.GetNewsEventType(Convert.ToInt32(mt));
            var page = Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString();
            var pagesize = Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString();
            rvd.Add("pagesize", pagesize);
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "EventCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
            rvd.Add("mt", EncryptDecrypt.Encrypt(mt));
            TempData["pager"] = pagesize;
            Session["Edit/Delete"] = "Delete";
            try
            {
                // TODO: Add delete logic here
                //.. Check for image in use
                NewsEvent objListing = objContext.NewsEvents.Find(Convert.ToInt32(nid));
                int NewsEventID = Convert.ToInt32(nid);
                #region System Change Log
                var oldresult = (from a in objContext.NewsEvents
                                 where a.NewsEventID == NewsEventID
                                 select a).ToList();
                DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                #endregion
              
                if (objListing != null)
                {
                    objListing.IsDeletedInd = true;
                    objContext.SaveChanges();
                    
                    #region System Change Log
                    SystemChangeLog objSCL = new SystemChangeLog();
                    long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                    objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                    objSCL.UsernameTxt = objuser.UserNameTxt;
                    objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                    objSCL.ModuleTxt = "News Event";
                    objSCL.LogTypeTxt = "Delete";
                    objSCL.NotesTxt = (_service.GetNewsEventType(objListing.TypeMasterID.Value)) + " Details deleted for " + objListing.TitleTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();
                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var newResult = (from x in objContext.NewsEvents
                                     where x.NewsEventID == NewsEventID
                                     select x);
                    DataTable dtNew = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(newResult);
                    foreach (DataColumn col in dtNew.Columns)
                    {
                        SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                        objSCLD.ChangeLogID = objSCL.ChangeLogID;
                        objSCLD.FieldNameTxt = col.ColumnName.ToString();
                        objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                        objSCLD.NewValueTxt = "";// dtNew.Rows[0][col.ColumnName].ToString();
                        objContext.SystemChangeLogDetails.Add(objSCLD);
                        objContext.SaveChanges();
                    }
                    #endregion
                    try
                    {
                        Models.Common.DeleteImage(Server.MapPath(objListing.ImageURLTxt));
                    }
                    catch
                    {
                    }
                    TempData["AlertMessage"] = _service.GetNewsEventType(Convert.ToInt64(mt)) + " details deleted successfully.";
                }
                //.. Checks for no of records in current page if exists records then return same page number else decrease the page number
                int? CheckPage = 1;
                int ListingTypeID = Convert.ToInt32(mt);
                var count = objContext.NewsEvents.Where(x => x.TypeMasterID == ListingTypeID && x.IsDeletedInd == false).Count();
                if (Convert.ToInt32(page) > 1)
                    CheckPage = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
                rvd.Add("page", CheckPage);
                return Json(Url.Action("Index", "NewsEvent", rvd));
            }
            catch (Exception ex)
            {
                rvd.Add("page", page);
                return Json(Url.Action("Index", "NewsEvent", rvd));
            }
        }

        [HttpPost]
        public JsonResult CheckURL(string url)
        {
            var objContext = new db_KISDEntities();
            var count = 0;
            count += objContext.NewsEvents.Where(x => x.PageURLTxt.Contains(url)).Count();
            if (count > 0)
            {
                if (url != "")
                    url = url + count;
            }
            return Json(url);
        }

        private List<SelectListItem> GetStatusData(string value)
        {
            value = !string.IsNullOrEmpty(value) ? ((value == "1" || value == "True") ? "True" : "False") : value;
            var items = new List<SelectListItem>();
            var data = new SelectListItem();
            data.Text = "Active";
            data.Value = "True";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "InActive";
            data.Value = "False";
            items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return items;
        }

        private List<SelectListItem> GetShowonHomeData(string value)
        {
            var items = new List<SelectListItem>();
            var data = new SelectListItem();
            data.Text = "Yes";
            data.Value = "True";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "No";
            data.Value = "False";

            items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value).FirstOrDefault().Selected = true;

            }
            return items;
        }

        private List<SelectListItem> GetWeekData(string value)
        {
            value = !string.IsNullOrEmpty(value) ? ((value == "5th") ? "5" : (value == "2nd") ? "2" : (value == "3rd") ? "3" : (value == "4th") ? "4" : "1") : value;
            var items = new List<SelectListItem>();
            var data = new SelectListItem();
            data.Text = "1st";
            data.Value = "1";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "2nd";
            data.Value = "2";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "3rd";
            data.Value = "3";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "4th";
            data.Value = "4";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "5th";
            data.Value = "5";
            items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return items;
        }

        private List<SelectListItem> GetmonthdayData(string value)
        {
            //value = !string.IsNullOrEmpty(value) ? ((value == "2") ? "Mon" : (value == "3") ? "Tue" : (value == "4") ? "Wed" : (value == "5") ? "Thu" : (value == "6") ? "Fri": (value == "5") ? "Sat":"Sun") : value;
            var items = new List<SelectListItem>();
            var data = new SelectListItem();
            data.Text = "Sun";
            data.Value = "1";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "Mon";
            data.Value = "2";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "Tue";
            data.Value = "3";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "Wed";
            data.Value = "4";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "Thu";
            data.Value = "5";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "Fri";
            data.Value = "6";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "Sat";
            data.Value = "7";
            items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return items;
        }
        
        #region Import Department Events
        [Authorize]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="glt">Newsevent type id</param>
        /// <param name="glid"> Listing id</param>
        /// <param name="NewsEventID"></param>
        /// <returns></returns>
        public ActionResult ImportEvents(string mt, string nid, string pid)
        {
            var objNewsEventModel = new NewsEventModel();

            #region Check Tab is Accessible or Not
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Masters), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin) || RoleID == Convert.ToInt32(UserType.DepartmentUser)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            //Check for valid ImageTypeID
            ViewBag.ListingTypeId = mt;
            ViewBag.Title = "";
            if ((Request.QueryString["mt"] == null) && (Request.QueryString["mt"] == null))
            {
                return RedirectToAction("Index", "Home");
            }

            //decrypt image type id(it)
            mt = !string.IsNullOrEmpty(Convert.ToString(mt)) ? EncryptDecrypt.Decrypt(mt) : "0";

            //decrypt image id(iid)
            nid = !string.IsNullOrEmpty(Convert.ToString(nid)) ? EncryptDecrypt.Decrypt(nid) : "0";

            //decrypt image type id(pid)
            pid = !string.IsNullOrEmpty(Convert.ToString(pid)) ? EncryptDecrypt.Decrypt(pid) : "0";

            int NewsEventID = Convert.ToInt32(nid);
            if (NewsEventID > 0 && objContext.NewsEvents.Where(x => x.NewsEventID == NewsEventID && x.IsDeletedInd == true).Any())
            {
                return RedirectToAction("Index", "Home");
            }
            Session["Edit/Delete"] = "Edit";
            ViewBag.PageTitle = (nid == "0" ? "Import " : "Edit ") + (_service.GetNewsEventType(Convert.ToInt64(mt)));
            ViewBag.Submit = (nid == "0" ? "Save" : "Update");
            ViewBag.ListingTypeId = mt;
            ViewBag.Title = (nid == "0" ? "Add " : "Edit ") + (_service.GetNewsEventType(Convert.ToInt64(mt)));
            ViewBag.ImageTypeTitle = _service.GetNewsEventType(Convert.ToInt64(mt));
            objNewsEventModel.EventCreateDate = DateTime.Now;
            objNewsEventModel.dsError = new DataSet();
            ViewBag.Date = DateTime.Now.ToShortDateString();
            objNewsEventModel.TypeMasterID = Convert.ToInt64(mt);
            if (!string.IsNullOrEmpty(pid))
                objNewsEventModel.DepartmentID = Convert.ToInt64(pid);
            var InnerImagesTitle = Models.Common.GetInnerImages();
            return View(objNewsEventModel);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ImportEvents(string command, FormCollection fm, NewsEventModel model)
        {
            try
            {
                var width = 260;
                HttpPostedFileBase file = Request.Files.Count > 0 ? Request.Files[0] : null;
                ViewBag.Title = "Import " + _service.GetNewsEventType(model.TypeMasterID.Value);
                ViewBag.Submit = "Save";
                ViewBag.ListingTypeId = model.TypeMasterID;
                ViewBag.Date = model.EventCreateDate.Value.ToShortDateString().Trim();
                var rvd = new RouteValueDictionary();
                rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
                rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
                rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "EventCreateDate");
                rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
                rvd.Add("mt", EncryptDecrypt.Encrypt(Convert.ToString(model.TypeMasterID)));
                rvd.Add("pid", EncryptDecrypt.Encrypt(Convert.ToString(model.DepartmentID)));
                ViewBag.PageTitle = "Import " + _service.GetNewsEventType(model.TypeMasterID.Value) + " Details";
                ViewBag.ImageTypeTitle = _service.GetNewsEventType(model.TypeMasterID.Value);
                model.dsError = new DataSet();
                #region System Change Log
                DataTable dtOld;
                var oldresult = (from a in objContext.NewsEvents
                                 where a.NewsEventID == model.NewsEventID
                                 select a).ToList();
                dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                #endregion
                if (string.IsNullOrEmpty(command))
                {
                    NewsEvent objNewsEvent;
                    DataSet dsError = new DataSet();
                    DataTable dtErrorlog = new DataTable();
                    DataTable dtErrorMsg = new DataTable();
                    dtErrorMsg.Columns.Add("ErrMsgID", typeof(int));
                    dtErrorMsg.Columns.Add("ErrMsgTxt", typeof(string));
                    dtErrorMsg.Columns.Add("ErrMsgType", typeof(string));
                    dtErrorMsg.TableName = "ErrorMsg";
                    dtErrorlog.TableName = "ErrorLog";
                    objNewsEvent = new NewsEvent();
                    objNewsEvent.IsDeletedInd = false;
                    objNewsEvent.TypeMasterID = model.TypeMasterID;
                    objNewsEvent.DepartmentID = model.DepartmentID;
                    if (file != null && file.ContentLength > 0)
                    {
                        bool IsValidExcel = false;
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/WebData/TempDocument"), fileName);
                        Models.Common.CreateFolder();
                        file.SaveAs(path);
                        var fileformatpath = Server.MapPath("~/FileFormat/DepartmentEvents.csv");
                        var dtFileFormat = ConvertExcelToDataTable(fileformatpath);
                        var dtUploadedExcel = ConvertExcelToDataTable(path);
                        if (dtFileFormat.Columns.Count == dtUploadedExcel.Columns.Count)
                        {
                            IsValidExcel = MatchColumn(dtFileFormat, dtUploadedExcel);
                            if (IsValidExcel)
                            {
                                if (dtUploadedExcel.Rows.Count > 0)
                                {
                                    dtErrorlog = dtUploadedExcel.Clone();
                                    dtErrorlog.TableName = "ErrorLog";
                                    var charlimiterr = 0;
                                    var statuserr = 0;
                                    var recurrerr = 0;
                                    var recurrtypeerr = 0;
                                    var weeklychk = 0;
                                    var monthmodeerr = 0;
                                    var monthdayerr = 0;
                                    var monthonerr = 0;
                                    var monthondayerr = 0;
                                    var edformaterr = 0;
                                    var dsdformaterr = 0;
                                    var dedformaterr = 0;
                                    var eedformaterr = 0;
                                    var emptytitlecount = dtUploadedExcel.AsEnumerable().Where(x => string.IsNullOrEmpty(Convert.ToString(x.Field<dynamic>("EventTitle")))).Count();
                                    var sametitlcount = dtUploadedExcel.AsEnumerable().Where(x => !string.IsNullOrEmpty(Convert.ToString(x.Field<dynamic>("EventTitle")))).GroupBy(r => r[0].ToString().ToLower()).Where(g => g.Count() > 1).Select(grp => new { Count = grp.Count() });
                                    var cnt = 0;
                                    if (emptytitlecount > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "There " + (emptytitlecount == 1 ? "is " : " are ") + emptytitlecount + " " + (emptytitlecount == 1 ? "record" : "records") + ", which do not have Event Title. Please update excel and try again.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (sametitlcount != null && sametitlcount.Count() > 0)
                                    {
                                        cnt = sametitlcount.Sum(x => x.Count);
                                    }
                                    if (cnt > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "There " + (cnt == 1 ? "is " : "are ") + cnt + " " + (cnt == 1 ? "record" : "records") + ", which have duplicate Event Title. Please update excel and try again.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    for (int i = 0; i < dtUploadedExcel.Rows.Count; i++)
                                    {
                                        var EventTitle = dtUploadedExcel.Rows[i][0].ToString();
                                        var EventDate = dtUploadedExcel.Rows[i][1].ToString();
                                        var DispalyStartDate = dtUploadedExcel.Rows[i][2].ToString();
                                        var DisplayEndDate = dtUploadedExcel.Rows[i][3].ToString();
                                        var Status = dtUploadedExcel.Rows[i][4].ToString();
                                        var IsRecurring = dtUploadedExcel.Rows[i][5].ToString();
                                        var RecurringNbr = dtUploadedExcel.Rows[i][6].ToString();
                                        var Issunday = dtUploadedExcel.Rows[i][7].ToString();
                                        var Ismonday = dtUploadedExcel.Rows[i][8].ToString();
                                        var Istuesday = dtUploadedExcel.Rows[i][9].ToString();
                                        var Iswednesday = dtUploadedExcel.Rows[i][10].ToString();
                                        var Isthursday = dtUploadedExcel.Rows[i][11].ToString();
                                        var Isfriday = dtUploadedExcel.Rows[i][12].ToString();
                                        var Issaturday = dtUploadedExcel.Rows[i][13].ToString();
                                        var monthmodenbr = dtUploadedExcel.Rows[i][14].ToString();
                                        var monthday = dtUploadedExcel.Rows[i][15].ToString();
                                        var monthon = dtUploadedExcel.Rows[i][16].ToString();
                                        var monthonday = dtUploadedExcel.Rows[i][17].ToString();
                                        var EndOnDate = dtUploadedExcel.Rows[i][18].ToString();
                                        var excelGrades = string.Empty;
                                        if ((string.IsNullOrEmpty(EventTitle) || string.IsNullOrEmpty(EventDate)  || string.IsNullOrEmpty(DispalyStartDate) || string.IsNullOrEmpty(DisplayEndDate)) ||
                                        string.IsNullOrEmpty(Status) || string.IsNullOrEmpty(IsRecurring))
                                        {
                                            DataRow[] row = dtErrorlog.Select("EventTitle = '" + EventTitle + "'");
                                            if (row.Length == 0)
                                                dtErrorlog.ImportRow(dtUploadedExcel.Rows[i]);
                                            if(IsRecurring=="1" && string.IsNullOrEmpty(RecurringNbr))
                                            {
                                                row = dtErrorlog.Select("EventTitle = '" + EventTitle + "'");
                                                if (row.Length == 0)
                                                    dtErrorlog.ImportRow(dtUploadedExcel.Rows[i]);
                                                if(RecurringNbr == "Weekly" && string.IsNullOrEmpty(Issunday) && string.IsNullOrEmpty(Ismonday) && string.IsNullOrEmpty(Istuesday) && string.IsNullOrEmpty(Iswednesday)
                                                    && string.IsNullOrEmpty(Isthursday) && string.IsNullOrEmpty(Isfriday) && string.IsNullOrEmpty(Issaturday))
                                                {
                                                    row = dtErrorlog.Select("EventTitle = '" + EventTitle + "'");
                                                    if (row.Length == 0)
                                                        dtErrorlog.ImportRow(dtUploadedExcel.Rows[i]);
                                                }
                                                if(RecurringNbr == "Weekly" && string.IsNullOrEmpty(monthmodenbr))
                                                {
                                                    row = dtErrorlog.Select("EventTitle = '" + EventTitle + "'");
                                                    if (row.Length == 0)
                                                        dtErrorlog.ImportRow(dtUploadedExcel.Rows[i]);
                                                    if(monthmodenbr=="One Day" && string.IsNullOrEmpty(monthday))
                                                    {
                                                        row = dtErrorlog.Select("EventTitle = '" + EventTitle + "'");
                                                        if (row.Length == 0)
                                                            dtErrorlog.ImportRow(dtUploadedExcel.Rows[i]);
                                                    }
                                                    if (monthmodenbr == "One Week Day" && (string.IsNullOrEmpty(monthon)|| string.IsNullOrEmpty(monthonday)))
                                                    {
                                                        row = dtErrorlog.Select("EventTitle = '" + EventTitle + "'");
                                                        if (row.Length == 0)
                                                            dtErrorlog.ImportRow(dtUploadedExcel.Rows[i]);
                                                    }
                                                }
                                            }
                                            
                                        }
                                        
                                        if (!string.IsNullOrEmpty(EventTitle) || !string.IsNullOrEmpty(Status))
                                        {
                                            if (EventTitle.Length > 100)
                                            {
                                                charlimiterr++;
                                            }
                                            var chkListingTitle = objContext.NewsEvents.Where(x => x.TitleTxt == EventTitle.Trim()
                                                                    && x.TypeMasterID == model.TypeMasterID
                                                                    && x.NewsEventID != model.NewsEventID).Any();
                                            if (chkListingTitle)//check newsevent title on adding new newsevent details  
                                            {
                                                DataRow dr = dtErrorMsg.NewRow();
                                                dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                                dr["ErrMsgTxt"] = EventTitle + " title already exists.";
                                                dtErrorMsg.Rows.Add(dr);
                                            }
                                        }
                                        if (dtUploadedExcel.Rows[i]["Status"].ToString() != "1" || dtUploadedExcel.Rows[i]["Status"].ToString() != "0")
                                        {
                                            if (dtUploadedExcel.Rows[i]["Status"].ToString() != "1" && dtUploadedExcel.Rows[i]["Status"].ToString() != "0")
                                            {
                                                statuserr++;
                                            }
                                        }
                                        if (dtUploadedExcel.Rows[i]["IsRecurringEvent"].ToString() != "1" || dtUploadedExcel.Rows[i]["IsRecurringEvent"].ToString() != "0")
                                        {
                                            if (dtUploadedExcel.Rows[i]["IsRecurringEvent"].ToString() != "1" && dtUploadedExcel.Rows[i]["IsRecurringEvent"].ToString() != "0")
                                            {
                                                recurrerr++;
                                            }
                                        }
                                        if (dtUploadedExcel.Rows[i]["RecurrenceType"].ToString() != "Daily" || dtUploadedExcel.Rows[i]["RecurrenceType"].ToString() != "Weekly" || dtUploadedExcel.Rows[i]["RecurrenceType"].ToString() != "Monthly" || dtUploadedExcel.Rows[i]["RecurrenceType"].ToString() != "Yearly")
                                        {
                                            if (dtUploadedExcel.Rows[i]["RecurrenceType"].ToString() != "Daily" && dtUploadedExcel.Rows[i]["RecurrenceType"].ToString() != "Weekly" && dtUploadedExcel.Rows[i]["RecurrenceType"].ToString() != "Monthly" && dtUploadedExcel.Rows[i]["RecurrenceType"].ToString() != "Yearly")
                                            {
                                                recurrtypeerr++;
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(RecurringNbr) && RecurringNbr == "Weekly")
                                        {
                                            if (dtUploadedExcel.Rows[i]["IsSunday"].ToString() != "1" || dtUploadedExcel.Rows[i]["IsSunday"].ToString() != "0")
                                            {
                                                if (dtUploadedExcel.Rows[i]["IsSunday"].ToString() != "1" && dtUploadedExcel.Rows[i]["IsSunday"].ToString() != "0")
                                                {
                                                    weeklychk++;
                                                }
                                            }
                                            if (dtUploadedExcel.Rows[i]["IsMonday"].ToString() != "1" || dtUploadedExcel.Rows[i]["IsMonday"].ToString() != "0")
                                            {
                                                if (dtUploadedExcel.Rows[i]["IsMonday"].ToString() != "1" && dtUploadedExcel.Rows[i]["IsMonday"].ToString() != "0")
                                                {
                                                    weeklychk++;
                                                }
                                            }
                                            if (dtUploadedExcel.Rows[i]["IsTuesday"].ToString() != "1" || dtUploadedExcel.Rows[i]["IsTuesday"].ToString() != "0")
                                            {
                                                if (dtUploadedExcel.Rows[i]["IsTuesday"].ToString() != "1" && dtUploadedExcel.Rows[i]["IsTuesday"].ToString() != "0")
                                                {
                                                    weeklychk++;
                                                }
                                            }
                                            if (dtUploadedExcel.Rows[i]["IsWednesday"].ToString() != "1" || dtUploadedExcel.Rows[i]["IsWednesday"].ToString() != "0")
                                            {
                                                if (dtUploadedExcel.Rows[i]["IsWednesday"].ToString() != "1" && dtUploadedExcel.Rows[i]["IsWednesday"].ToString() != "0")
                                                {
                                                    weeklychk++;
                                                }
                                            }
                                            if (dtUploadedExcel.Rows[i]["IsThursday"].ToString() != "1" || dtUploadedExcel.Rows[i]["IsThursday"].ToString() != "0")
                                            {
                                                if (dtUploadedExcel.Rows[i]["IsThursday"].ToString() != "1" && dtUploadedExcel.Rows[i]["IsThursday"].ToString() != "0")
                                                {
                                                    weeklychk++;
                                                }
                                            }
                                            if (dtUploadedExcel.Rows[i]["IsFriday"].ToString() != "1" || dtUploadedExcel.Rows[i]["IsFriday"].ToString() != "0")
                                            {
                                                if (dtUploadedExcel.Rows[i]["IsFriday"].ToString() != "1" && dtUploadedExcel.Rows[i]["IsFriday"].ToString() != "0")
                                                {
                                                    weeklychk++;
                                                }
                                            }
                                            if (dtUploadedExcel.Rows[i]["IsSaturday"].ToString() != "1" || dtUploadedExcel.Rows[i]["IsSaturday"].ToString() != "0")
                                            {
                                                if (dtUploadedExcel.Rows[i]["IsSaturday"].ToString() != "1" && dtUploadedExcel.Rows[i]["IsSaturday"].ToString() != "0")
                                                {
                                                    weeklychk++;
                                                }
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(RecurringNbr) && RecurringNbr == "Weekly")
                                        {
                                            if (dtUploadedExcel.Rows[i]["MonthMode"].ToString() != "One Date" || dtUploadedExcel.Rows[i]["MonthMode"].ToString() != "One Week Day")
                                            {
                                                if (dtUploadedExcel.Rows[i]["MonthMode"].ToString() != "One Date" && dtUploadedExcel.Rows[i]["MonthMode"].ToString() != "One Week Day")
                                                {
                                                    monthmodeerr++;
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(dtUploadedExcel.Rows[i]["MonthDay"].ToString()) && Convert.ToInt32(dtUploadedExcel.Rows[i]["MonthDay"].ToString()) > 31)
                                            {
                                                monthdayerr++;
                                            }
                                            if (!string.IsNullOrEmpty(dtUploadedExcel.Rows[i]["MonthOn"].ToString()) && Convert.ToInt32(dtUploadedExcel.Rows[i]["MonthOn"].ToString()) > 5)
                                            {
                                                monthonerr++;
                                            }
                                            if (dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Sun" || dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Mon"
                                                || dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Tue" || dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Wed"
                                                || dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Thu" || dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Fri"
                                                || dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Sat")
                                            {
                                                if (dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Sun" && dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Mon"
                                                && dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Tue" && dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Wed"
                                                && dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Thu" && dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Fri"
                                                && dtUploadedExcel.Rows[i]["MonthOnDay"].ToString() != "Sat")
                                                {
                                                    monthondayerr++;
                                                }
                                            }
                                        }
                                        DateTime dtout;
                                        if (DateTime.TryParseExact(dtUploadedExcel.Rows[i]["EventDate"].ToString(), "MM/dd/yyyy h:mm:ss tt", null, DateTimeStyles.None, out dtout) == false)
                                            edformaterr++;
                                        if (DateTime.TryParseExact(dtUploadedExcel.Rows[i]["DisplayStartDate"].ToString(), "MM/dd/yyyy h:mm:ss tt", null, DateTimeStyles.None, out dtout) == false)
                                            dsdformaterr++;
                                        if (DateTime.TryParseExact(dtUploadedExcel.Rows[i]["DisplayEndDate"].ToString(), "MM/dd/yyyy h:mm:ss tt", null, DateTimeStyles.None, out dtout) == false)
                                            dedformaterr++;
                                        if (DateTime.TryParseExact(dtUploadedExcel.Rows[i]["EndsOn"].ToString(), "MM/dd/yyyy h:mm:ss tt", null, DateTimeStyles.None, out dtout) == false)
                                            eedformaterr++;

                                    }
                                    if (charlimiterr > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = " Please check the character limit for Event Title.<ul class='dserror'><li>Event Title should not have more than 100 characters.</li></ul>";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (statuserr > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "Status values should either be '1' or '0' only.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (recurrerr > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "Is Recurring Event values should either be '1' or '0' only.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (recurrtypeerr > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "Recurrence Type values should either be 'Daily' or 'Weekly' or 'Monthly' or 'Yearly' only.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (weeklychk > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "IsSunday, IsMonday, IsTuesday, IsWednesday, IsThursday, IsFriday, IsSaturaday values should either be '1' or '0' only.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (monthmodeerr > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "MonthMode values should either be 'One Date' or 'One Week Day' only.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (monthdayerr > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "MonthDay must be less than or equal to 31.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (monthonerr > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "MonthOn must be less than or equal to 5.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (monthondayerr > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "MonthOnDay values should either be 'Sun' or 'Mon' or 'Tue' or 'Wed' or 'Thu' or 'Fri' or 'Sat' only.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (edformaterr > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "Event Date is invalid.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (dsdformaterr > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "Dispaly Start Date is invalid.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (dedformaterr > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "Dispaly End Date is invalid.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (eedformaterr > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "Event End Date is invalid.";
                                        dtErrorMsg.Rows.Add(dr);
                                    }
                                    if (dtErrorlog.Rows.Count > 0)
                                    {
                                        DataRow dr = dtErrorMsg.NewRow();
                                        dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                        dr["ErrMsgTxt"] = "One or more columns has left blank in excel. Missing column entries are showing below:";
                                        dtErrorMsg.Rows.Add(dr);
                                        dsError.Tables.Add(dtErrorlog);
                                    }
                                    if (dtErrorMsg.Rows.Count > 0)
                                    {
                                        dsError.Tables.Add(dtErrorMsg);
                                    }
                                    if (dsError.Tables.Count > 0)
                                    {
                                        model.dsError = dsError;
                                        DeleteFile(path);
                                        return View(model);
                                    }
                                    else
                                    {
                                        foreach (DataRow dr in dtUploadedExcel.Rows)
                                        {
                                            objNewsEvent.TitleTxt = dr["EventTitle"].ToString();
                                            objNewsEvent.ImageURLTxt = null;
                                            objNewsEvent.IsExternalLinkInd = null;
                                            objNewsEvent.PageURLTxt = null;
                                            objNewsEvent.ExternalLinkTargetInd = null;
                                            objNewsEvent.AuthorNameTxt = null;
                                            objNewsEvent.AbstractTxt = null;
                                            objNewsEvent.DescriptionTxt = null;
                                            objNewsEvent.RightSectionTitleTxt = null;
                                            objNewsEvent.RightSectionAbstractTxt = null;
                                            objNewsEvent.BannerImageID = null;
                                            objNewsEvent.AltBannerImageTxt = null;
                                            objNewsEvent.BannerImageAbstractTxt = null;
                                            objNewsEvent.EventDate = Convert.ToDateTime(dr["EventDate"].ToString());
                                            objNewsEvent.DisplayStartDate = Convert.ToDateTime(dr["DisplayStartDate"].ToString());
                                            objNewsEvent.DisplayEndDate = Convert.ToDateTime(dr["DisplayEndDate"].ToString());
                                            objNewsEvent.ShowOnHomeInd = null;
                                            objNewsEvent.PageMetaTitleTxt = null;
                                            objNewsEvent.PageMetaDescriptionTxt = null;
                                            objNewsEvent.StatusInd = Convert.ToBoolean(dr["Status"].ToString() == "Active" ? 1 : 0);
                                            objNewsEvent.DepartmentID = model.DepartmentID;
                                            objNewsEvent.EventCreateDate = Convert.ToDateTime(model.EventCreateDate.Value.ToShortDateString());
                                            objNewsEvent.CreateDate = model.NewsEventID > 0 ? objNewsEvent.CreateDate : DateTime.Now; ;
                                            objNewsEvent.CreateByID = model.NewsEventID > 0 ? objNewsEvent.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                                            objNewsEvent.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                                            objNewsEvent.LastModifyDate = DateTime.Now;
                                            objNewsEvent.IsDeletedInd = false;
                                            objNewsEvent.TypeMasterID = model.TypeMasterID;
                                            objNewsEvent.IsFacebookSharingInd = null;
                                            objNewsEvent.IsTwitterSharingInd = null;
                                            objNewsEvent.IsGooglePlusSharingInd = null;

                                            objNewsEvent.IsRecurringInd = Convert.ToBoolean(dr["IsRecurringEvent"].ToString() == "Yes" ? 1 : 0);
                                            if (objNewsEvent.IsRecurringInd.HasValue && objNewsEvent.IsRecurringInd.Value)
                                            {
                                                objNewsEvent.RecurrenceTypeNbr = Convert.ToInt32(dr["RecurrenceType"].ToString() == "Daily" ? 1 : dr["RecurrenceType"].ToString() == "Weekly" ? 2 : dr["RecurrenceType"].ToString() == "Monthly" ? 3 : 4);
                                                if (objNewsEvent.RecurrenceTypeNbr == 2)//For Weekly
                                                {
                                                    objNewsEvent.IsSundayInd = model.NoIsSunday;
                                                    objNewsEvent.IsMondayInd = model.NoIsMonday;
                                                    objNewsEvent.IsTuesdayInd = model.NoIsTuesday;
                                                    objNewsEvent.IsWednesdayInd = model.NoIsWednesday;
                                                    objNewsEvent.IsThursdayInd = model.NoIsThursday;
                                                    objNewsEvent.IsFridayInd = model.NoIsFriday;
                                                    objNewsEvent.IsSaturdayInd = model.NoIsSaturday;
                                                }
                                                if (objNewsEvent.RecurrenceTypeNbr == 3)// For Monthly
                                                {
                                                    objNewsEvent.MonthModeNbr = model.MonthModeNbr;
                                                    if (objNewsEvent.MonthModeNbr == 1)// For One Date
                                                    {
                                                        objNewsEvent.MonthdayNbr = model.MonthdayNbr;
                                                    }
                                                    if (objNewsEvent.MonthModeNbr == 2)// For One Week Day
                                                    {
                                                        objNewsEvent.MonthondayNbr = model.MonthondayNbr;
                                                        objNewsEvent.MonthonNbr = model.MonthonNbr;
                                                    }
                                                }
                                                objNewsEvent.EventsEndOnDate = Convert.ToDateTime(dr["EndsOn"].ToString());
                                            }
                                            objNewsEvent.ImageURLTxt = null;
                                            objContext.NewsEvents.Add(objNewsEvent);
                                            objContext.SaveChanges();
                                        }
                                        #region System Change Log
                                        SystemChangeLog objSCL = new SystemChangeLog();
                                        long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                                        User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                                        objSCL.UsernameTxt = objuser.UserNameTxt;
                                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                                        objSCL.ModuleTxt = "News Event";
                                        objSCL.LogTypeTxt = objNewsEvent.NewsEventID > 0 ? "Update" : "Add";
                                        objSCL.NotesTxt = "New Event Details" + (objNewsEvent.NewsEventID > 0 ? " updated for " : "  added for ") + objNewsEvent.TitleTxt;
                                        objSCL.LogDateTime = DateTime.Now;
                                        objContext.SystemChangeLogs.Add(objSCL);
                                        objContext.SaveChanges();
                                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                                        var newResult = (from x in objContext.NewsEvents
                                                         where x.NewsEventID == model.NewsEventID
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
                                        TempData["AlertMessage"] = _service.GetNewsEventType(model.TypeMasterID.Value) + " details saved successfully.";
                                    }
                                }
                                else
                                {
                                    DataRow dr = dtErrorMsg.NewRow();
                                    dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                    dr["ErrMsgTxt"] = "There are no rows in the excel. Please add data in the excel.";
                                    dtErrorMsg.Rows.Add(dr);
                                    dsError.Tables.Add(dtErrorMsg);
                                    model.dsError = dsError;
                                    DeleteFile(path);
                                    return View(model);
                                }
                            }
                            else
                            {
                                DataRow dr = dtErrorMsg.NewRow();
                                dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                                dr["ErrMsgTxt"] = "Column name or order is not matching with required excel format. Please download sample format for reference.";
                                dtErrorMsg.Rows.Add(dr);
                                dsError.Tables.Add(dtErrorMsg);
                                model.dsError = dsError;
                                DeleteFile(path);
                                return View(model);
                            }
                        }
                        else
                        {
                            //TempData["AlertMessage"] = "Invalid excel format.";
                            DataRow dr = dtErrorMsg.NewRow();
                            dr["ErrMsgID"] = dtErrorMsg.Rows.Count + 1;
                            dr["ErrMsgTxt"] = "Invalid excel format.";
                            dtErrorMsg.Rows.Add(dr);
                            dsError.Tables.Add(dtErrorMsg);
                            model.dsError = dsError;
                            DeleteFile(path);
                            return View(model);
                        }
                    }
                    return RedirectToAction("Index", "NewsEvent", rvd);
                }
                else
                {
                    return RedirectToAction("Index", "NewsEvent", rvd);
                }
            }
            catch (Exception ex)
            {
                return View(model);
            }
        }
        public static DataTable ConvertExcelToDataTable(string FileName)
        {
            DataTable dtResult = null;
            int totalSheet = 0; //No of sheets on excel file  
            using (OleDbConnection objConn = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties='Excel 12.0;HDR=YES;IMEX=1;';"))
            {
                objConn.Open();
                OleDbCommand cmd = new OleDbCommand();
                OleDbDataAdapter oleda = new OleDbDataAdapter();
                DataSet ds = new DataSet();
                DataTable dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string sheetName = string.Empty;
                if (dt != null)
                {
                    var tempDataTable = (from dataRow in dt.AsEnumerable()
                                         where !dataRow["TABLE_NAME"].ToString().Contains("FilterDatabase")
                                         select dataRow).CopyToDataTable();
                    dt = tempDataTable;
                    totalSheet = dt.Rows.Count;
                    sheetName = dt.Rows[0]["TABLE_NAME"].ToString();
                }
                cmd.Connection = objConn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM [" + sheetName + "]";
                oleda = new OleDbDataAdapter(cmd);
                oleda.Fill(ds, "excelData");
                dtResult = ds.Tables["excelData"];
                objConn.Close();
                return dtResult; //Returning Dattable  
            }
        }
        public static bool MatchColumn(DataTable dt1, DataTable dt2)
        {
            bool IsValidExcel = false;
            DataColumnCollection dcc = dt1.Columns;
            string str = "";
            foreach (var column in dt2.Columns)
            {
                if (!dcc.Contains(column.ToString()) || dcc.IndexOf(column.ToString()) != dt2.Columns.IndexOf(column.ToString()))
                {
                    str = column.ToString() + ",";
                }
            }
            if (string.IsNullOrEmpty(str))
                return !IsValidExcel;
            else
                return IsValidExcel;
        }
        public static void DeleteFile(string filename)
        {
            if ((System.IO.File.Exists(filename)))
            {
                System.IO.File.Delete(filename);
            }
        }
        #endregion
    }
}