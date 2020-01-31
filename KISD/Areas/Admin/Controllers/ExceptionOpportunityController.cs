using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using KISD.Areas.Admin.Models;
using System.Data;
using System.Web.Security;
using ExceptionOppTypeAlias = KISD.Areas.Admin.Models.GalleryListingService.TypeMaster;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using static KISD.Areas.Admin.Models.Common;

namespace KISD.Areas.Admin.Controllers
{
    public class ExceptionOpportunityController : Controller
    {
        private ExceptionOpportunityService _service;
        db_KISDEntities objContext = new db_KISDEntities();
        /// <summary>
        /// Code to create instance Exceptional Opportunity Service class in constructor
        /// </summary>
        public ExceptionOpportunityController()
        {
            _service = new ExceptionOpportunityService();
        }
        [SessionExpire]
        [Authorize]
        /// <summary>
        /// this method will show the Exceptional Opportunity listing.
        /// </summary>
        /// <param name="Page">this parameter is used to get page number to be shown.</param>
        /// <param name="PageSize">this parameter is used to get no of recorde to be shown.</param>
        /// <param name="gridSortOptions">this parameter is used to get grid sorting option.</param>
        /// <param name="formCollection">this parameter is used to get controls collection on the page.</param>
        /// <param name="ObjResult"></param>
        /// <returns>view to enter Exceptional Opportunity details.</returns>
        public ActionResult Index(int? Page, int? PageSize, GridSortOptions gridSortOptions, FormCollection formCollection, string ObjResult)
        {
            var db_obj = new db_KISDEntities();
            TempData["CroppedImage"] = null;

            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.NewToKISD), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }

            #endregion

            //*******************Fill Values if Display order contains null values***************************
            var displayOrderList = objContext.ExceptionOpportunities.Where(x => x.IsDeletedInd == false).ToList();
            foreach (var item in displayOrderList)
            {
                if (string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()))
                {
                    var objExceptionOppData = objContext.ExceptionOpportunities.Where(x => x.ExOpportunityID == item.ExOpportunityID).FirstOrDefault();
                    var NewdisplayOrder = (displayOrderList.Max(x => x.DisplayOrderNbr)) == null ? 1 : displayOrderList.Max(x => x.DisplayOrderNbr).Value + 1;
                    objExceptionOppData.DisplayOrderNbr = NewdisplayOrder;
                    objContext.SaveChanges();
                }
            }
            //***********************************************************



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
                    else if (objAjaxRequest.ajaxcall == "showonhome")//Ajax Call type = showonhome 
                    {
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    else if (objAjaxRequest.ajaxcall == "displayorder")//Ajax Call type = Display Order i.e. drop down values
                    {
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }

                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
                }

                //Ajax Call for update status for Exceptional Opportunity
                if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && ObjResult != null && !string.IsNullOrEmpty(ObjResult) && objAjaxRequest.qs_Type.ToString().Trim().ToLower() == "showonhome")
                {
                    var ExcepOppID = System.Convert.ToInt64(objAjaxRequest.hfid);
                    var objExcepOpp = objContext.ExceptionOpportunities.Find(ExcepOppID);
                    if (objExcepOpp != null)
                    {
                        #region System Change Log
                        var oldresult = (from a in objContext.ExceptionOpportunities
                                         where a.ExOpportunityID == ExcepOppID
                                         select a).ToList();
                        DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                        #endregion
                        if (objExcepOpp.StatusInd == false)
                        {
                            TempData["Message"] = "Inactive Exceptional Opportunity cannot be set to show on home.";
                        }
                        else
                        {
                            objExcepOpp.ShowOnHomeInd = objAjaxRequest.hfvalue == "1";
                            objContext.SaveChanges();
                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Exceptional Opportunity";
                            objSCL.LogTypeTxt = objExcepOpp.ExOpportunityID > 0 ? "Update" : "Add";
                            objSCL.NotesTxt = " Details updated show on home for " + objExcepOpp.TitleTxt;
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();

                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in objContext.ExceptionOpportunities
                                             where x.ExOpportunityID == objExcepOpp.ExOpportunityID
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
                            TempData["AlertMessage"] = "Show on home updated successfully.";

                        }

                    }
                    objAjaxRequest.hfid = null;//remove parameter value
                    objAjaxRequest.hfvalue = null;//remove parameter value
                    objAjaxRequest.qs_Type = null;//remove parameter value
                    PageSize = ((Request.QueryString["pagesize"] != null && Request.QueryString["pagesize"].ToString() != "All") ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : Models.Common._pageSize);
                    Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Models.Common._currentPage);
                    gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                }
                else if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && ObjResult != null && !string.IsNullOrEmpty(ObjResult))
                {
                    var ExOpportunityID = System.Convert.ToInt64(objAjaxRequest.hfid);
                    var ExceptionOpportunity = objContext.ExceptionOpportunities.Find(ExOpportunityID);
                    if (ExceptionOpportunity != null)
                    {
                        #region System Change Log
                        var oldresult = (from a in objContext.ExceptionOpportunities
                                         where a.ExOpportunityID == ExOpportunityID
                                         select a).ToList();
                        DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                        #endregion
                        ExceptionOpportunity.StatusInd = objAjaxRequest.hfvalue == "1";

                        if (objAjaxRequest.qs_Type == "displayorder")
                        {
                            if (ExceptionOpportunityService.ChangeExceptionOpportunityDisplayOrder(ExceptionOpportunity.DisplayOrderNbr.Value, Convert.ToInt64(objAjaxRequest.qs_value)))
                            {
                                TempData["AlertMessage"] = "Display Order has been changed successfully.";
                            }
                        }
                        else
                        {
                            if (ExceptionOpportunity.ShowOnHomeInd == true)
                            {
                                TempData["Message"] = "Show on Home Exceptional Opportunity cannot be set to Inactive.";
                            }
                            else
                            {
                                objContext.SaveChanges();
                                #region System Change Log
                                SystemChangeLog objSCL = new SystemChangeLog();
                                long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                                User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                                objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                                objSCL.UsernameTxt = objuser.UserNameTxt;
                                objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                                objSCL.ModuleTxt = "Exceptional Opportunity";
                                objSCL.LogTypeTxt = ExceptionOpportunity.ExOpportunityID > 0 ? "Update" : "Add";
                                objSCL.NotesTxt = " Details updated status for " + ExceptionOpportunity.TitleTxt;
                                objSCL.LogDateTime = DateTime.Now;
                                objContext.SystemChangeLogs.Add(objSCL);
                                objContext.SaveChanges();

                                objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                                var newResult = (from x in objContext.ExceptionOpportunities
                                                 where x.ExOpportunityID == ExceptionOpportunity.ExOpportunityID
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
                                TempData["AlertMessage"] = "Status updated successfully.";
                            }

                        }
                        objAjaxRequest.hfid = null;//remove parameter value
                        objAjaxRequest.hfvalue = null;//remove parameter value

                        PageSize = ((Request.QueryString["pagesize"] != null && Request.QueryString["pagesize"].ToString() != "All") ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : Models.Common._pageSize);
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Models.Common._currentPage);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                }
                else
                {
                    TempData["Message"] = string.Empty;
                }
                ObjResult = string.Empty;
            }
            #endregion Ajax Call

            ViewBag.Title = ViewBag.PageTitle = "Exceptional Opportunities Listing";

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
                gridSortOptions.Column = "ExOpportunityCreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column == "TitleTxt" || gridSortOptions.Column == "ExOpportunityCreateDate" || gridSortOptions.Column == "CategoryName")
            {

            }
            else
            {
                gridSortOptions.Column = "ExOpportunityCreateDate";
            }
            //.. Code for get records as page view model
            var pagesize = PageSize.HasValue ? PageSize.Value : Models.Common._pageSize;
            var page = Page.HasValue ? Page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;
            var pagedViewModel = new PagedViewModel<ExceptionOpportunityModel>
            {
                ViewData = ViewData,
                Query = _service.GetExceptionOpportunitys().AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "ExOpportunityCreateDate",
                Page = page,
                PageSize = pagesize,
            }.Setup();
            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("ExceptionOpportunityPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }
        /// <summary>
        /// Get data for content page
        /// </summary>
        /// <param name="ct">Content Type</param>
        /// <returns></returns>
        /// 
        [Authorize]
        [SessionExpire]
        public ActionResult Create(string iid)
        {
            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.NewToKISD), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            var objExceptionOppModel = new ExceptionOpportunityModel();
            //decrypt ExceptionOpportunity id(iid)
            iid = !string.IsNullOrEmpty(Convert.ToString(iid)) ? EncryptDecrypt.Decrypt(iid) : "0";
            Int64 ExOpportunityID = Convert.ToInt64(iid);
            if (ExOpportunityID > 0 && objContext.ExceptionOpportunities.Where(x => x.ExOpportunityID == ExOpportunityID && x.IsDeletedInd == true).Any())
            {
                return RedirectToAction("Index", "Home");
            }
            Session["Edit/Delete"] = "Edit";
            ViewBag.Title = ViewBag.PageTitle = (iid == "0" ? "Add " : "Edit ") + "Exceptional Opportunity";
            ViewBag.Submit = (iid == "0" ? "Save" : "Update");
            ViewBag.ExceptionOpportunityTypeTitle = "Exceptional Opportunities";
            objExceptionOppModel.strCreateDate = DateTime.Now.ToShortDateString();
            Int64 TypeMasterId = Convert.ToInt64(ExceptionOppTypeAlias.ExceptionOpportunity);
            TempData["CroppedImage"] = null;
            using (objContext = new db_KISDEntities())
            {

                var objExceptionOpp = objContext.ExceptionOpportunities.Where(x => x.ExOpportunityID == ExOpportunityID && (x.IsDeletedInd == false || x.IsDeletedInd==null)).FirstOrDefault();

                if (objExceptionOpp != null)
                {
                    objExceptionOppModel.ExOpportunityID = objExceptionOpp.ExOpportunityID;
                    objExceptionOppModel.ExternalLinkInd = objExceptionOpp.ExternalLinkInd.Value;
                    objExceptionOppModel.ExternalLinkTargetInd = objExceptionOpp.ExternalLinkTargetInd;
                    objExceptionOppModel.TitleTxt = objExceptionOpp.TitleTxt;
                    if (objExceptionOpp.ExternalLinkInd == true)
                    {
                        objExceptionOppModel.URLTxt = objExceptionOpp.URLTxt;
                    }
                    else
                    {
                        objExceptionOppModel.PageURLTxt = objExceptionOpp.URLTxt;
                    }
                    objExceptionOppModel.BannerImageID = objExceptionOpp.BannerImageID;
                    objExceptionOppModel.AltBannerImageTxt = objExceptionOpp.AltBannerImageTxt;
                    objExceptionOppModel.BannerImageAbstractTxt = objExceptionOpp.BannerImageAbstractTxt;

                    objExceptionOppModel.DescriptionTxt = objExceptionOpp.DescriptionTxt;
                    objExceptionOppModel.StatusInd = objExceptionOpp.StatusInd.Value;
                    objExceptionOppModel.ShowOnHomeInd = objExceptionOpp.ShowOnHomeInd.Value;
                    objExceptionOppModel.PageMetaTitleTxt = objExceptionOpp.PageMetaTitleTxt;
                    objExceptionOppModel.RightSectionTitleTxt = objExceptionOpp.RightSectionTitleTxt;
                    objExceptionOppModel.RightSectionAbstractTxt = objExceptionOpp.RightSectionAbstractTxt;
                    objExceptionOppModel.PageMetaDescriptionTxt = objExceptionOpp.PageMetaDescriptionTxt;
                    objExceptionOppModel.CreateByID = objExceptionOpp.CreateByID;
                    objExceptionOppModel.CreateDate = objExceptionOpp.CreateDate;
                    objExceptionOppModel.LastModifyByID = objExceptionOpp.LastModifyByID;
                    objExceptionOppModel.LastModifyDate = objExceptionOpp.LastModifyDate;
                    objExceptionOppModel.strCreateDate = objExceptionOpp.ExOpportunityCreateDate.HasValue ? objExceptionOpp.ExOpportunityCreateDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                    ViewBag.IsActiveInd = GetStatusData(objExceptionOppModel.StatusInd == true ? "1" : "0");
                    ViewBag.SchoolCategoryID = GetSchoolCategories(objExceptionOpp.SchoolCategoryID.ToString());
                    ViewBag.ShowOnHomeInd = GetShowonHomeDataBoolean(objExceptionOppModel.ShowOnHomeInd ? "True" : "False");
                    ViewBag.Submit = "Update";
                }
                else
                {
                    ViewBag.Submit = "Save";
                    objExceptionOppModel.strCreateDate = DateTime.Now.ToShortDateString();
                    ViewBag.IsActiveInd = GetStatusData(string.Empty);
                    ViewBag.SchoolCategoryID = GetSchoolCategories("");
                    ViewBag.ShowOnHomeInd = GetShowonHomeDataBoolean(string.Empty);
                }

                var InnerImagesTitle = Models.Common.GetInnerImages();
                ViewBag.InnerImagesTitle = InnerImagesTitle;//get all the inner image titles

                ViewBag.Title = "Exceptional Opportunities";
                return View(objExceptionOppModel);
            }
        }
        ///<summary>
        ///Add Update the content.
        /// </summary>
        /// <param name="model">Intialized ContentModel model object from view</param>        
        /// <param name="command">Defines Submit or Cancel </param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [Authorize]
        [SessionExpire]
        public ActionResult Create(ExceptionOpportunityModel model, string command, FormCollection fm)
        {
            try
            {
                using (var objContext = new db_KISDEntities())
                {
                    var rvd = new RouteValueDictionary();
                    rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
                    rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
                    rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "ExOpportunityCreateDate");
                    rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
                    var file = Request.Files.Count > 0 ? Request.Files[0] : null;
                    if (string.IsNullOrEmpty(command))
                    {
                        var InnerImagesTitle = Models.Common.GetInnerImages();
                        ViewBag.InnerImagesTitle = InnerImagesTitle;//get all the inner image titles

                        var IsSave = false;
                        ViewBag.Title = ViewBag.PageTitle = (model.ExOpportunityID == 0 ? "Add " : "Edit ") + "Exceptional Opportunity";

                        var objExceptionOpp = objContext.ExceptionOpportunities.Where(x => x.ExOpportunityID == model.ExOpportunityID).FirstOrDefault();
                        if (objExceptionOpp == null)
                        {
                            IsSave = true;
                            objExceptionOpp = new ExceptionOpportunity();
                            objExceptionOpp.IsDeletedInd = false;
                        }

                        model.StatusInd = fm["IsActiveInd"] == "1" ? true : false;
                        ViewBag.ShowOnHomeInd = GetShowonHomeDataBoolean(model.ShowOnHomeInd ? "True" : "False");
                        ViewBag.SchoolCategoryID = GetSchoolCategories(model.SchoolCategoryID.ToString());
                        ViewBag.IsActiveInd = GetStatusData(model.StatusInd == true ? "1" : "0");
                        Int64 TypeMasterId = Convert.ToInt64(ExceptionOppTypeAlias.ExceptionOpportunity);
                        #region System Change Log
                        DataTable dtOld;
                        var oldResult = (from a in objContext.ExceptionOpportunities
                                         where a.ExOpportunityID == model.ExOpportunityID
                                         select a).ToList();
                        dtOld = Models.Common.LINQResultToDataTable(oldResult);
                        #endregion

                        ViewBag.Submit = IsSave ? "Save" : "Update";
                        if (model != null && !string.IsNullOrEmpty(model.TitleTxt))
                        {
                          var isexist = objContext.ExceptionOpportunities.Where(x => x.TitleTxt.ToLower().Trim() == model.TitleTxt.ToLower().Trim()
                                    && x.SchoolCategoryID == model.SchoolCategoryID && x.ExOpportunityID != model.ExOpportunityID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            if (isexist > 0)
                            {
                             ModelState.AddModelError("TitleTxt", model.TitleTxt + " already exists."); 
                             return View(model);
                            }
                        }
                        if (model != null && !string.IsNullOrEmpty(model.PageURLTxt))
                        {
                            var count = 0;
                            if (!model.ExternalLinkInd)
                            {
                                count = objContext.ExceptionOpportunities.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.ExOpportunityID != model.ExOpportunityID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                                count += objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                                count += objContext.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                                count += objContext.Departments.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                                count += objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                                count += objContext.NewsEvents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                                count += objContext.RightSections.Where(x => x.ExternalLinkURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            }

                            if (model.PageURLTxt.Trim().ToLower() == "error404")// Check for duplicate url and error404 url
                            {
                                count = count + 1;
                            }

                            if (count > 0)
                            {
                                ViewBag.FocusPageUrl = true;// Set focus on Pageurl Field if same url exist
                               // ViewBag.InnerImages = new SelectList(objContext.Images.Where(x => x.ImageTypeID == 2 && x.StatusInd == true).ToList(), "ImageID", "TitleTxt");
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

                        if (!model.StatusInd && model.ShowOnHomeInd)
                        {
                            TempData["Message"] = "Exceptional Opportunity cannot be set to show on home.";
                            return View(model);
                        }
                        objExceptionOpp.ExOpportunityID = model.ExOpportunityID;
                        objExceptionOpp.ParentID = (int?)null;
                        objExceptionOpp.ExternalLinkInd = model.ExternalLinkInd;
                        objExceptionOpp.ExternalLinkTargetInd = model.ExternalLinkTargetInd;
                        objExceptionOpp.TitleTxt = model.TitleTxt;
                        objExceptionOpp.URLTxt = objExceptionOpp.ExternalLinkInd == true ? model.URLTxt : model.PageURLTxt;

                        
                        objExceptionOpp.DescriptionTxt = string.IsNullOrEmpty(model.DescriptionTxt) ? string.Empty : model.DescriptionTxt;
                        objExceptionOpp.StatusInd = fm["IsActiveInd"] == "1" ? true : false;
                        objExceptionOpp.SchoolCategoryID = model.SchoolCategoryID;
                        objExceptionOpp.BannerImageID = model.BannerImageID;
                        objExceptionOpp.BannerImageAbstractTxt = model.BannerImageAbstractTxt;
                        objExceptionOpp.AltBannerImageTxt = model.AltBannerImageTxt;
                        objExceptionOpp.IsDeletedInd = false;

                        //objExceptionOpp.SchoolCategoryID = Convert.ToInt64(fm["SchoolCategoryID"]);
                        objExceptionOpp.ShowOnHomeInd = model.ShowOnHomeInd;
                        objExceptionOpp.ExOpportunityCreateDate = Convert.ToDateTime(model.strCreateDate);
                        objExceptionOpp.PageMetaTitleTxt = string.IsNullOrEmpty(model.PageMetaTitleTxt) ? string.Empty : model.PageMetaTitleTxt;
                        objExceptionOpp.PageMetaDescriptionTxt = string.IsNullOrEmpty(model.PageMetaDescriptionTxt) ? string.Empty : model.PageMetaDescriptionTxt;
                        
                        objExceptionOpp.RightSectionAbstractTxt = model.RightSectionAbstractTxt;
                        objExceptionOpp.RightSectionTitleTxt = model.RightSectionTitleTxt;
                        objExceptionOpp.CreateDate = model.ExOpportunityID > 0 ? objExceptionOpp.CreateDate : DateTime.Now; ;
                        objExceptionOpp.CreateByID = model.ExOpportunityID > 0 ? objExceptionOpp.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        objExceptionOpp.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        objExceptionOpp.LastModifyDate = DateTime.Now;

                        if (IsSave)
                        {
                            objContext.ExceptionOpportunities.Add(objExceptionOpp);
                        }
                        objContext.SaveChanges();

                        TempData["AlertMessage"] = model.ExOpportunityID == 0 ? "Exceptional Opportunity details saved successfully." : "Exceptional Opportunity details updated successfully.";

                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = "Exceptional Opportunity";
                        objSCL.LogTypeTxt = model.ExOpportunityID > 0 ? "Update" : "Add";
                        objSCL.NotesTxt = "Exceptional Opportunity Details" + (model.ExOpportunityID > 0 ? " updated for " : "  added for ") + model.TitleTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();
                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.ExceptionOpportunities
                                         where x.ExOpportunityID == objExceptionOpp.ExOpportunityID
                                         select x);
                        DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                        foreach (DataColumn col in dtNew.Columns)
                        {
                            if (model.ExOpportunityID > 0)
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

                        return RedirectToAction("Index", "ExceptionOpportunity", rvd);
                    }
                    else
                    {
                        return RedirectToAction("Index", "ExceptionOpportunity", rvd);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = " Some error occured. Please try again later.";
                return RedirectToAction("Index", "Home");
            }
        }
        /// <summary>
        /// This method is used to delete the ExceptionOpportunity from database.It also chacks wheather ExceptionOpportunity is in use or not.
        /// If ExceptionOpportunity is in use then it return to view and show message "ExceptionOpportunity is in use, cannot be deleted." else it delte the ExceptionOpportunity and return to thye view.
        /// </summary>
        /// <param name="TypeMasterID">This parameter is used to get the TypeMasterID</param>
        /// <param name="ExceptionOpportunityID">This parameter is used to get the ExceptionOpportunityid</param>
        /// <param name="fm">this parameter is used to get the form control values from view </param>
        /// <returns>This method return the Json result (url) that will be passed to the Ajax post method on client side.</returns>
        [HttpPost]
        [Authorize]
        public JsonResult Delete(string iid, FormCollection fm)
        {
            //decrypt ExceptionOpportunity id(iid)
            iid = !string.IsNullOrEmpty(Convert.ToString(iid)) ? EncryptDecrypt.Decrypt(iid) : "0";

            //.. Code for get the route value directory
            RouteValueDictionary rvd = new RouteValueDictionary();
            ViewBag.Title = "Exceptional Opportunity";
            var page = Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString();
            var pagesize = Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString();
            rvd.Add("pagesize", pagesize);
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "ExOpportunityCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
            TempData["pager"] = pagesize;
            Session["Edit/Delete"] = "Delete";
            try
            {
                // TODO: Add delete logic here
                //.. Check for Exceptional Opportunity  in use
                ExceptionOpportunity objExOpportunity = objContext.ExceptionOpportunities.Find(Convert.ToInt32(iid));
                int OpportunityID = Convert.ToInt32(iid);
                #region System Change Log
                var oldresult = (from a in objContext.ExceptionOpportunities
                                 where a.ExOpportunityID == OpportunityID
                                 select a).ToList();
                DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                #endregion
                if (objExOpportunity != null)
                {
                    // objExOpportunity.IsDeletedInd = true;

                    //****************Display Order ************************

                    try
                    {
                        var objExOpportunityService = new ExceptionOpportunityService();
                        objExOpportunityService.ChangeDeletedDisplayOrder(objExOpportunity.DisplayOrderNbr.Value, objExOpportunity.ExOpportunityID);
                    }
                    catch { }

                    //***************************************************
                    // objContext.SaveChanges();
                    #region System Change Log
                    SystemChangeLog objSCL = new SystemChangeLog();
                    long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                    objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                    objSCL.UsernameTxt = objuser.UserNameTxt;
                    objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                    objSCL.ModuleTxt = "Exceptional Opportunity";
                    objSCL.LogTypeTxt = "Delete";
                    objSCL.NotesTxt = " Details deleted for " + objExOpportunity.TitleTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();
                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var objContextnew = new db_KISDEntities();
                    var newResult = (from x in objContextnew.ExceptionOpportunities
                                     where x.ExOpportunityID == OpportunityID
                                     select x);
                    DataTable dtNew = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(newResult);
                    foreach (DataColumn col in dtNew.Columns)
                    {
                        SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                        objSCLD.ChangeLogID = objSCL.ChangeLogID;
                        objSCLD.FieldNameTxt = col.ColumnName.ToString();
                        objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                        objSCLD.NewValueTxt = col.ColumnName == "IsDeletedInd" ? dtNew.Rows[0][col.ColumnName].ToString() : "";
                        objContext.SystemChangeLogDetails.Add(objSCLD);
                        objContext.SaveChanges();
                    }
                    #endregion                   
                    TempData["AlertMessage"] = "Exceptional Opportunity details deleted successfully.";
                }
                //.. Checks for no of records in current page if exists records then return same page number else decrease the page number
                int? CheckPage = 1;
                var count = objContext.ExceptionOpportunities.Where(x => x.IsDeletedInd == false).Count();
                if (Convert.ToInt32(page) > 1)
                    CheckPage = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
                rvd.Add("page", CheckPage);
                return Json(Url.Action("Index", "ExceptionOpportunity", rvd));
            }
            catch
            {
                rvd.Add("page", page);
                return Json(Url.Action("Index", "ExceptionOpportunity", rvd));
            }
        }
        #region Private Methods

        /// <summary>
        /// This method is used to bind the Status Data
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetStatusData(string value)
        {
            List<SelectListItem> lstStatus = new List<SelectListItem>();
            SelectListItem lstStatusData = new SelectListItem();
            lstStatusData.Text = "Active";
            lstStatusData.Value = "1";
            lstStatus.Add(lstStatusData);
            lstStatusData = new SelectListItem();
            lstStatusData.Text = "InActive";
            lstStatusData.Value = "0";
            lstStatus.Add(lstStatusData);
            if (!string.IsNullOrEmpty(value))
            {
                lstStatus.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return lstStatus;
        }
        /// <summary>
        /// This method is used to bind the ShowOnHome data
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> GetShowonHomeDataBoolean(string value)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            SelectListItem data = new SelectListItem();
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
        [HttpPost]
        public JsonResult CheckURL(string url)
        {
            var objContext = new db_KISDEntities();
            var count = 0;

            //count = objContext.Events.Where(x => x.URLTxt.Contains(url)).Count();
            //count = objContext.News.Where(x => x.URLTxt.Contains(url)).Count();
            count += objContext.ExceptionOpportunities.Where(x => x.URLTxt.Contains(url)).Count();
            if (count > 0)
            {
                if (url != "")
                    url = url + count;
            }
            return Json(url);
        }

        private List<SelectListItem> GetSchoolCategories(string schoolCategoryID)
        {
            List<SelectListItem> lstSelectListItem = new List<SelectListItem>();
            var TypeMasterID = Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory);
            var data = objContext.Schools.Where(x => x.TypeMasterID == TypeMasterID && x.StatusInd == true && x.IsDeletedInd == false).ToList();

            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    SelectListItem lstData = new SelectListItem();
                    lstData.Text = data[i].NameTxt;
                    lstData.Value = Convert.ToString(data[i].SchoolID);
                    lstSelectListItem.Add(lstData);
                }

                if (!string.IsNullOrEmpty(schoolCategoryID) && lstSelectListItem.Count > 0)
                {
                    lstSelectListItem.Where(x => x.Value == schoolCategoryID).FirstOrDefault().Selected = true;
                }
            }
            return lstSelectListItem;
        }
        #endregion
    }
}