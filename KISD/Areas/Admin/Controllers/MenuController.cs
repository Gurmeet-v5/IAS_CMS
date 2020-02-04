using KISD.Areas.Admin.Models;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using static KISD.Areas.Admin.Models.Common;
using ContentTypeAlias = KISD.Areas.Admin.Models.ContentType;
using ModuleTypeAlias = KISD.Areas.Admin.Models.Common.ModuleType;

namespace KISD.Areas.Admin.Controllers
{
    [Authorize]
    [SessionExpire]
    public class MenuController : Controller
    {
        #region Menu
        [Authorize]
        /// <summary>
        /// This action method will be called when the user visits the Menu listing page.        
        /// Set the page variables value according to the menu type id
        /// Code added to update the Status of the Menu if formcollection parameters contains values for hdncheckboxselected and 
        /// hdnvalue params.
        /// </summary>
        /// <param name="page">This parameters is used for page number.It shows the records in the list of that page. </param>
        /// <param name="pagesize">This parameter is used for showing the number of records per page</param>
        /// <param name="gridSortOptions">This parameter is used for sorting the list of records in ascending/descending order</param>
        /// <param name="mt">This parameter is used to get the type of menu and show records in the list of this type</param>
        /// <param name="fm">This parameter is used to get the Form collection of the control from view</param>
        public ActionResult MenuListing(int? page, int? pagesize, GridSortOptions gridSortOptions, string mt, string smt, string mtid, string smtid, FormCollection fm, string objresult)
        {
            if (string.IsNullOrEmpty(gridSortOptions.Column))
            {
                gridSortOptions.Direction = MvcContrib.Sorting.SortDirection.Descending;
            }
            var objContentService = new ContentService();
            using (var objContext = new db_KISDEntities())
            {
                //decrypt menu type id(mt)
                mt = !string.IsNullOrEmpty(Convert.ToString(mt)) ? EncryptDecrypt.Decrypt(mt) : "0";

                int menutypeId = Convert.ToInt32(mt);

                #region Check Tab is Accessible or Not
                int TabType = 0;
                if (menutypeId == 2) { TabType = Convert.ToInt32(ModuleTypeAlias.Home); }
                if (menutypeId == 14) { TabType = Convert.ToInt32(ModuleTypeAlias.AboutUs); }
                if (menutypeId == 20) { TabType = Convert.ToInt32(ModuleTypeAlias.ContactUs); }
                if (menutypeId == 19) { TabType = Convert.ToInt32(ModuleTypeAlias.DailyNews); }
                if (menutypeId == 8) { TabType = Convert.ToInt32(ModuleTypeAlias.Downloads); }
                if (menutypeId == 5) { TabType = Convert.ToInt32(ModuleTypeAlias.Syllabus); }
                if (menutypeId == 17) { TabType = Convert.ToInt32(ModuleTypeAlias.Video); }
                if (menutypeId == 25) { TabType = Convert.ToInt32(ModuleTypeAlias.FlyPages); }

                var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
                var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
                var HasTabAccess = GetAccessibleTabAccess(TabType, Convert.ToInt32(userId));
                if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                    || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
                {
                    if ((TabType == Convert.ToInt32(ModuleTypeAlias.Departments) || TabType == Convert.ToInt32(ModuleTypeAlias.FlyPages)) && RoleID == Convert.ToInt32(UserType.DepartmentUser))
                    {

                    }
                    else
                        return RedirectToAction("Index", "Home");
                }
                #endregion

                smt = !string.IsNullOrEmpty(Convert.ToString(smt)) ? EncryptDecrypt.Decrypt(smt) : "0";
                var contenttypetitle = objContext.ContentTypes.Find(menutypeId).ContentTypeNameTxt;
                ViewBag.PageTitle = contenttypetitle + " Page Listing";
                ViewBag.Title = contenttypetitle + " Page Listing";
                ViewBag.MenuTypeId = mt;
                //*******************Fill Values if Display order contains null values***************************
                var displayOrderList = objContext.Contents.Where(x => x.ContentTypeID == menutypeId && x.ParentID == null && x.IsDeletedInd == false).ToList();
                foreach (var item in displayOrderList)
                {
                    if (string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()))
                    {
                        var objContentData = objContext.Contents.Where(x => x.ContentID == item.ContentID && x.IsDeletedInd == false).FirstOrDefault();
                        var displayOrder1 = (displayOrderList.Max(x => x.DisplayOrderNbr)) == null ? 1 : displayOrderList.Max(x => x.DisplayOrderNbr).Value + 1;
                        objContentData.DisplayOrderNbr = displayOrder1;
                        objContext.SaveChanges();
                    }
                }
                //***********************************************************

                #region Ajax Call

                // Ajax call type 
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
                        objAjaxRequest.ajaxcall = null;//remove parameter value
                    }

                    //Ajax CAll for update status for images
                    if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && objresult != null && !string.IsNullOrEmpty(objresult))
                    {
                        var contentID = Convert.ToInt64(objAjaxRequest.hfid);
                        var objMenuContent = objContext.Contents.Find(contentID);
                        if (objMenuContent != null)
                        {
                            #region System Change Log
                            var oldresult = (from a in objContext.Contents
                                             where a.ContentID == contentID
                                             select a).ToList();
                            DataTable dtOld = Models.Common.LINQResultToDataTable(oldresult);
                            #endregion

                            objMenuContent.StatusInd = objAjaxRequest.hfvalue == "1" ? true : false;
                            objContext.SaveChanges();
                            TempData["AlertMessage"] = "Status updated successfully.";

                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Menu";
                            objSCL.LogTypeTxt = objMenuContent.ContentID > 0 ? "Update" : "Add";
                            objSCL.NotesTxt = "Menu Details" + (objMenuContent.ContentID > 0 ? " updated for " : "  added for ") + objMenuContent.MenuTitleTxt;
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();

                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in objContext.Contents
                                             where x.ContentID == objMenuContent.ContentID
                                             select x);
                            DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                            foreach (DataColumn col in dtNew.Columns)
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
                            #endregion
                        }
                        objAjaxRequest.hfid = null;//remove parameter value
                        objAjaxRequest.hfvalue = null;//remove parameter value
                        pagesize = (Request.QueryString["pagesize"] != null ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : pagesize);
                        page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    //*******DisplayOrder Dorder*************** 
                    else if (objAjaxRequest.qs_checkboxselected != null && objAjaxRequest.qs_value != null && !string.IsNullOrEmpty(objAjaxRequest.qs_checkboxselected)
                         && !string.IsNullOrEmpty(objAjaxRequest.qs_value) && objAjaxRequest.qs_Type.Trim().ToLower() == "displayorder".Trim().ToLower())
                    {
                        var contentID = Convert.ToInt64(objAjaxRequest.qs_checkboxselected);
                        var objMenuContent = objContext.Contents.Find(contentID);
                        if (objMenuContent != null)
                        {
                            try
                            {
                                //var displayOrder = string.IsNullOrEmpty(objMenuContent.DisplayOrderNum.ToString()) ? objHAI.Contents.Where(x => x.ContentTypeID == menutypeId).Max(x => x.DisplayOrderNum).Value : objMenuContent.DisplayOrderNum.Value ; 
                                if (objContentService.ChangeImageDisplayOrder(objMenuContent.DisplayOrderNbr.Value, Convert.ToInt32(objAjaxRequest.qs_value), Convert.ToInt64(objMenuContent.ContentID), menutypeId))
                                {
                                    TempData["AlertMessage"] = "Display Order has been changed successfully.";
                                }
                            }
                            catch
                            {
                                TempData["AlertMessage"] = "Some Error Occured while changing Display Order, Please try again later.";
                            }
                            objAjaxRequest.qs_checkboxselected = null;//remove parameter value
                            objAjaxRequest.qs_value = null;//remove parameter value
                            objAjaxRequest.qs_Type = null;//remove parameter value

                            pagesize = (Request.QueryString["pagesize"] != null ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : pagesize);
                            page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                            gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                        }
                    }
                    else
                    {
                        TempData["AlertMessage"] = string.Empty;
                    }
                    objresult = string.Empty;
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
                    gridSortOptions.Column = "ContentCreateDate";
                    Session["PageSize"] = null;
                    Session["pageNo"] = null;
                    Session["GridSortOption"] = null;
                }
                //.. Code for get records as Page View Model
                var pageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
                var Page = page.HasValue ? page.Value : Models.Common._currentPage;
                TempData["pager"] = pageSize;

                if (gridSortOptions.Column != null)
                {
                    if (gridSortOptions.Column == "URLTxt"
                        || gridSortOptions.Column == "MenuTitleTxt" || gridSortOptions.Column == "PageTitleTxt"
                        || gridSortOptions.Column == "ContentCreateDate" || gridSortOptions.Column == "DisplayOrderNbr")
                    {

                    }
                    else
                    {
                        gridSortOptions.Column = "ContentCreateDate";
                    }
                }
                var pagedViewModel = new PagedViewModel<ContentModel>
                {
                    ViewData = ViewData,
                    Query = objContentService.GetMenus(menutypeId, userId).AsQueryable(),
                    GridSortOptions = gridSortOptions,
                    DefaultSortColumn = "ContentCreateDate",
                    Page = Page,
                    PageSize = pageSize,
                }
               .Setup();
                if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
                {
                    return View("MenuPartial", pagedViewModel);// ("partial view name ")
                }
                else
                {
                    return View(pagedViewModel);
                }
            }
        }

        [Authorize]
        /// <summary>
        /// This method will be called when admin add  or edits the Menu page.
        /// </summary>
        /// <param name="contenttype"></param>
        /// <returns></returns>
        public ActionResult Create(string mt, string cid, string mtid, string smtid)
        {
            //decrypt menu type id(mt)
            mt = !string.IsNullOrEmpty(Convert.ToString(mt)) ? EncryptDecrypt.Decrypt(mt) : "0";
            //decrypt menu type id(mt)
            cid = !string.IsNullOrEmpty(Convert.ToString(cid)) ? EncryptDecrypt.Decrypt(cid) : "0";

            var objContentModel = new ContentModel();
            Session["Edit/Delete"] = "Edit";
            ViewBag.FocusPageUrl = false;// Set focus on Pageurl Field if same url exist
            int MenuTypeId = Convert.ToInt32(mt);
            int ContentId = Convert.ToInt32(cid);
            using (var objContext = new db_KISDEntities())
            {
                #region Check Tab is Accessible or Not
                int TabType = 0;
                if (MenuTypeId == 2) { TabType = Convert.ToInt32(ModuleTypeAlias.Home); }
                if (MenuTypeId == 14) { TabType = Convert.ToInt32(ModuleTypeAlias.AboutUs); }
                if (MenuTypeId == 20) { TabType = Convert.ToInt32(ModuleTypeAlias.ContactUs); }
                if (MenuTypeId == 19) { TabType = Convert.ToInt32(ModuleTypeAlias.DailyNews); }
                if (MenuTypeId == 8) { TabType = Convert.ToInt32(ModuleTypeAlias.Downloads); }
                if (MenuTypeId == 5) { TabType = Convert.ToInt32(ModuleTypeAlias.Syllabus); }
                if (MenuTypeId == 17) { TabType = Convert.ToInt32(ModuleTypeAlias.Video); }
                if (MenuTypeId == 25) { TabType = Convert.ToInt32(ModuleTypeAlias.FlyPages); }

                var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
                var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
                var HasTabAccess = GetAccessibleTabAccess(TabType, Convert.ToInt32(userId));
                if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                    || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
                {
                    if ((TabType == Convert.ToInt32(ModuleTypeAlias.Departments) || TabType == Convert.ToInt32(ModuleTypeAlias.FlyPages)) && RoleID == Convert.ToInt32(UserType.DepartmentUser))
                    {

                    }
                    else
                        return RedirectToAction("Index", "Home");
                }
                #endregion

                ViewBag.Menu = MenuTypeId;
                objContentModel.ContentTypeID = MenuTypeId;
                var objContentData = objContext.Contents.Where(x => x.ContentID == ContentId).FirstOrDefault();
                var contentTypeTitle = objContext.ContentTypes.Find(MenuTypeId).ContentTypeNameTxt;
                ViewBag.BreadCrumTtile = contentTypeTitle + " Page Listing";
                ViewBag.InnerImages = new SelectList(Models.Common.GetInnerImages(), "ImageID", "TitleTxt");
                objContentModel.strCreateDate = DateTime.Today.ToShortDateString();

                objContentModel.RightSections = GetAllRightSections();
                if (objContentData != null)
                {
                    objContentModel.ContentID = objContentData.ContentID;
                    objContentModel.ContentTypeID = objContentData.ContentTypeID;
                    objContentModel.IsExternalLinkInd = objContentData.IsExternalLinkInd;
                    objContentModel.ExternalLinkTxt = objContentData.ExternalLinkTxt;
                    objContentModel.ExternalLinkTargetInd = objContentData.ExternalLinkTargetInd;
                    objContentModel.PageTitleTxt = objContentData.PageTitleTxt;
                    objContentModel.MenuTitleTxt = objContentData.MenuTitleTxt;
                    objContentModel.PageURLTxt = objContentData.PageURLTxt;
                    objContentModel.BannerImageID = objContentData.BannerImageID;
                    objContentModel.AbstractTxt = objContentData.AbstractTxt;
                    objContentModel.DescriptionTxt = objContentData.DescriptionTxt;
                    objContentModel.strCreateDate = objContentData.ContentCreateDate.Value.ToShortDateString();
                    objContentModel.StatusInd = objContentData.StatusInd;
                    objContentModel.DisplayOrderNbr = objContentData.DisplayOrderNbr.Value;//Display Order Num
                    objContentModel.PageMetaTitleTxt = objContentData.PageMetaTitleTxt;
                    objContentModel.PageMetaDescriptionTxt = objContentData.PageMetaDescriptionTxt;
                    objContentModel.AltBannerImageTxt = objContentData.AltBannerImageTxt;
                    objContentModel.BannerImageAbstractTxt = objContentData.BannerImageAbstractTxt;
                    objContentModel.RightSectionTitleTxt = objContentData.RightSectionTitleTxt;
                    objContentModel.RightSectionAbstractTxt = objContentData.RightSectionAbstractTxt;
                    objContentModel.ContentTypeTitle = "Edit " + contentTypeTitle + " Page Details";
                    objContentModel.IsGooglePlusSharingInd = objContentData.IsGooglePlusSharingInd.HasValue ? objContentData.IsGooglePlusSharingInd.Value : false;
                    objContentModel.IsFacebookSharingInd = objContentData.IsFacebookSharingInd.HasValue ? objContentData.IsFacebookSharingInd.Value : false;
                    objContentModel.IsTwitterSharingInd = objContentData.IsTwitterSharingInd.HasValue ? objContentData.IsTwitterSharingInd.Value : false;
                    ViewBag.IsActiveInd = GetStatusData(objContentModel.StatusInd == true ? "1" : "0");
                    ViewBag.Submit = "Update";
                }
                else
                {
                    ViewBag.Submit = "Save";
                    objContentModel.ContentTypeID = MenuTypeId;
                    objContentModel.ContentTypeTitle = "Add " + contentTypeTitle + " Page Details";
                    objContentModel.ContentCreateDate = DateTime.Now;
                    objContentModel.IsGooglePlusSharingInd = false;
                    objContentModel.IsFacebookSharingInd = false;
                    objContentModel.IsTwitterSharingInd = false;
                    ViewBag.IsActiveInd = GetStatusData(string.Empty);
                }
                ViewBag.Title = (objContentData != null ? " Edit " : " Add ") + contentTypeTitle + (objContentModel.ContentTypeID == Convert.ToInt32(ContentTypeAlias.Fly) ? " Page" : "");
                return View(objContentModel);
            }
        }

        /// <summary>
        /// Save update the Menu pages              
        /// </summary>
        /// <param name="model">Intialized ContentModel model object from view</param>        
        /// <param name="command">Defines Submit or Cancel </param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(ContentModel model, string command, FormCollection fm)
        {
            var mt = EncryptDecrypt.Encrypt(model.ContentTypeID.ToString());
            var smt = Request.QueryString["smt"] ?? EncryptDecrypt.Encrypt("0");
            var mtid = Request.QueryString["mtid"] ?? EncryptDecrypt.Encrypt("0");
            var smtid = Request.QueryString["smtid"] ?? EncryptDecrypt.Encrypt("0");
            var decodedSMT = EncryptDecrypt.Decrypt(smt);
            var rvd = new RouteValueDictionary();
            rvd.Add("page", Request.QueryString["page"] ?? "1");
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "ContentCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            rvd.Add("mt", mt);
            rvd.Add("smt", smt);
            rvd.Add("mtid", mtid);
            rvd.Add("smtid", smtid);
            rvd.Add("cid", EncryptDecrypt.Encrypt(model.ContentID.ToString()));
            rvd.Add("DisplayOrderNum", model.DisplayOrderNbr);//Display Order Num
            ViewBag.FocusPageUrl = false;// Set focus on Pageurl Field if same url exist
            using (var objContext = new db_KISDEntities())
            {
                if (string.IsNullOrEmpty(command))
                {
                    try
                    {
                        #region System Change Log
                        DataTable dtOld;
                        var oldResult = (from a in objContext.Contents
                                         where a.ContentID == model.ContentID
                                         select a).ToList();
                        dtOld = Models.Common.LINQResultToDataTable(oldResult);
                        #endregion

                        model.RightSections = GetAllRightSections();
                        model.PageTitleTxt = model.PageTitleTxt;
                        ViewBag.Menu = model.ContentTypeID;
                        ViewBag.Submit = model.ContentID == 0 ? "Save" : "Update";
                        var contentType = model.ContentTypeID;
                        var contentTypeTitle = objContext.ContentTypes.Find(contentType).ContentTypeNameTxt;
                        ViewBag.Title = (model != null ? " Edit " : " Add ") + contentTypeTitle + (model.ContentTypeID == Convert.ToInt32(ContentTypeAlias.Fly) ? " Page" : "");
                        ViewBag.BreadCrumTtile = contentTypeTitle + " Page Listing";
                        var objContentData = objContext.Contents.Where(x => x.ContentID == model.ContentID).FirstOrDefault();

                        if (model != null && !string.IsNullOrEmpty(model.PageURLTxt) && model.IsExternalLinkInd == false)
                        {
                            var count = 0;
                            count = objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.ContentID != model.ContentID && x.IsDeletedInd == false).Count();
                            count += objContext.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.Departments.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.ExceptionOpportunities.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.NewsEvents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.RightSections.Where(x => x.ExternalLinkURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            if (model.PageURLTxt.Trim().ToLower() == "error404")
                            {
                                count = count + 1;
                            }
                            if (count > 0)
                            {
                                ViewBag.InnerImages = new SelectList(Models.Common.GetInnerImages(), "ImageID", "TitleTxt");

                                if (model.PageURLTxt.ToLower().Trim() == "error404")//if user types url 'error404' below validation msg should display
                                {
                                    ModelState.AddModelError("PageURLTxt", model.PageURLTxt + " URL is not allowed.");
                                }
                                else
                                {
                                    ModelState.AddModelError("PageURLTxt", model.PageURLTxt + " URL already exists.");
                                }

                                ViewBag.FocusPageUrl = true;// Set focus on Pageurl Field if same url exist
                                ViewBag.IsActiveInd = GetStatusData(fm["IsActiveInd"].ToString() == "1" ? "1" : "0");
                                return View(model);
                            }
                        }
                        var IsNew = objContentData == null;
                        if (objContentData == null)
                        {
                            objContentData = new Content();
                        }
                        var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();

                        objContentData.IsExternalLinkInd = model.IsExternalLinkInd;
                        objContentData.ParentID = null;
                        objContentData.ExternalLinkTxt = model.IsExternalLinkInd ? (model.ExternalLinkTxt ?? string.Empty) : string.Empty;
                        objContentData.AbstractTxt = model.AbstractTxt ?? string.Empty;
                        objContentData.PageTitleTxt = model.IsExternalLinkInd ? string.Empty : model.PageTitleTxt ?? string.Empty;
                        objContentData.PageURLTxt = model.IsExternalLinkInd ? string.Empty : (string.IsNullOrEmpty(model.PageURLTxt) ? string.Empty : model.PageURLTxt);
                        objContentData.BannerImageID = model.IsExternalLinkInd ? null : model.BannerImageID;
                        objContentData.DescriptionTxt = model.IsExternalLinkInd ? string.Empty : model.DescriptionTxt ?? string.Empty;
                        objContentData.PageMetaTitleTxt = model.IsExternalLinkInd ? string.Empty : model.PageMetaTitleTxt ?? string.Empty;
                        objContentData.PageMetaDescriptionTxt = model.IsExternalLinkInd ? string.Empty : model.PageMetaDescriptionTxt ?? string.Empty;
                        objContentData.ExternalLinkTargetInd = model.IsExternalLinkInd ? (model.ExternalLinkTargetInd) : false;
                        objContentData.AltBannerImageTxt = model.IsExternalLinkInd ? string.Empty : model.AltBannerImageTxt ?? string.Empty;
                        objContentData.BannerImageAbstractTxt = model.IsExternalLinkInd ? string.Empty : model.BannerImageAbstractTxt;
                        objContentData.RightSectionTitleTxt = model.IsExternalLinkInd ? string.Empty : model.RightSectionTitleTxt ?? string.Empty;
                        objContentData.RightSectionAbstractTxt = model.IsExternalLinkInd ? string.Empty : model.RightSectionAbstractTxt ?? string.Empty;
                        objContentData.IsDeletedInd = false;
                        objContentData.CreateByID = userId;
                        objContentData.CreateDate = DateTime.Now;
                        objContentData.LastModifyByID = userId;
                        objContentData.LastModifyDate = DateTime.Now;
                        objContentData.IsFacebookSharingInd = model.IsFacebookSharingInd;
                        objContentData.IsGooglePlusSharingInd = model.IsGooglePlusSharingInd;
                        objContentData.IsTwitterSharingInd = model.IsTwitterSharingInd;
                        DateTime dt_to = Convert.ToDateTime(model.strCreateDate, System.Globalization.CultureInfo.InvariantCulture);
                        objContentData.ContentCreateDate = dt_to;
                        objContentData.StatusInd = fm["IsActiveInd"] == "1" ? true : false;

                        //*************Display Order ************************
                        if (model.ContentID == 0)
                        {
                            var MenuCount = objContext.Contents.Where(x => x.ContentTypeID == model.ContentTypeID && x.ParentID == null && x.IsDeletedInd == false).ToList();
                            objContentData.DisplayOrderNbr = MenuCount.Any() ? objContext.Contents.Where(x => x.ContentTypeID == model.ContentTypeID && x.ParentID == null && x.IsDeletedInd == false).Max(x => x.DisplayOrderNbr) + 1 : 1;
                            objContext.Contents.Add(objContentData);
                        }
                        //**************************************************

                        objContentData.MenuTitleTxt = model.MenuTitleTxt ?? string.Empty;
                        objContentData.ContentTypeID = contentType;

                        if (IsNew)
                        {
                            objContext.Contents.Add(objContentData);
                        }
                        TempData["AlertMessage"] = contentTypeTitle + " Page details" + (IsNew ? " saved" : " updated") + " successfully.";
                        objContext.SaveChanges();

                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = "Menu";
                        objSCL.LogTypeTxt = model.ContentID > 0 ? "Update" : "Add";
                        objSCL.NotesTxt = (GetContentType(model.ContentTypeID)) + " Details" + (model.ContentID > 0 ? " updated for " : "  added for ") + model.PageTitleTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();
                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.Contents
                                         where x.ContentID == objContentData.ContentID
                                         select x);
                        DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                        foreach (DataColumn col in dtNew.Columns)
                        {
                            if (model.ContentID > 0)
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
                    }
                    catch (Exception ex)
                    {
                        TempData["AlertMessage"] = "Some error occured, Please try after some time.";
                    }
                    return RedirectToAction("MenuListing", rvd);
                }
                else
                {
                    return RedirectToAction("MenuListing", rvd);
                }
            }
        }

        /// <summary>
        /// Delete the Menu 
        /// </summary>
        /// <param name="contentID">contentID  </param>
        /// <param name="MenuTypeId">Type of content</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(string cid, string mt, string mtid, string smtid)
        {
            //decrypt menu type id(mt)
            mt = !string.IsNullOrEmpty(Convert.ToString(mt)) ? EncryptDecrypt.Decrypt(mt) : "0";
            //decrypt content id(mt)
            cid = !string.IsNullOrEmpty(Convert.ToString(cid)) ? EncryptDecrypt.Decrypt(cid) : "0";
            mtid = !string.IsNullOrEmpty(Convert.ToString(mtid)) ? EncryptDecrypt.Decrypt(mtid) : "0";
            smtid = !string.IsNullOrEmpty(Convert.ToString(smtid)) ? EncryptDecrypt.Decrypt(smtid) : "0";
            int MenuTypeId = Convert.ToInt32(mt);
            int contentID = Convert.ToInt32(cid);

            var rvd = new RouteValueDictionary();
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "ContentCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            rvd.Add("mt", Request.QueryString["mt"]);
            rvd.Add("smt", Request.QueryString["smt"]);
            rvd.Add("cid", EncryptDecrypt.Decrypt(cid));
            rvd.Add("mtid", Request.QueryString["mtid"]);
            rvd.Add("smtid", Request.QueryString["smtid"]);
            Session["Edit/Delete"] = "Delete";
            if (contentID > 0)
            {
                try
                {
                    using (var objContext = new db_KISDEntities())
                    {
                        //******************Check Sub Menu Available *********************************
                        var contentlist_count = objContext.Contents.Where(x => x.ParentID == contentID && x.IsDeletedInd == false).ToList();

                        if (contentlist_count.Any())
                        {
                            try
                            {
                                TempData["Message"] = "Menu can not be deleted as it contains Sub Menu details.";
                                return Json(Url.Action("MenuListing", "Menu", rvd));
                            }
                            catch { }
                        }
                        /*****************************************************************************/

                        var obj = objContext.Contents.Where(x => x.ContentID == contentID || x.ParentID == contentID).FirstOrDefault();
                        if (obj != null)
                        {
                            #region System Change Log
                            DataTable dtOld;
                            var oldResult = (from a in objContext.Contents
                                             where a.ContentID == contentID
                                             select a).ToList();
                            dtOld = Models.Common.LINQResultToDataTable(oldResult);
                            #endregion

                            //****************Display Order and delete ************************
                            var objContentMenu = objContext.Contents.Where(x => x.ContentID == contentID).FirstOrDefault();
                            if (objContentMenu != null)
                            {
                                try
                                {
                                    var objModelService = new ContentService();
                                    objModelService.ChangeDeletedDisplayOrder(objContentMenu.DisplayOrderNbr.Value, contentID, MenuTypeId);
                                }
                                catch { }
                            }
                            //***************************************************
                            TempData["AlertMessage"] = objContentMenu.MenuTitleTxt + (objContentMenu.ContentTypeID != Convert.ToInt32(ContentTypeAlias.Fly) ? " menu details" : objContentMenu.PageTitleTxt + " fly page") + " deleted successfully.";

                            #region Delete Selected Right Section for the Menu
                            var TypeMasterID = Convert.ToInt64(mtid);
                            var rightSections = objContext.RightSections.Where(x => x.ListingID == contentID && x.TypeMasterID == TypeMasterID).ToList();
                            if (rightSections != null && rightSections.Count() > 0)
                            {
                                foreach (var section in rightSections)
                                {
                                    section.IsDeletedInd = true;
                                }
                                objContext.SaveChanges();
                            }
                            #endregion

                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Menu";
                            objSCL.LogTypeTxt = "Delete";
                            objSCL.NotesTxt = obj.MenuTitleTxt + " Details deleted.";
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();
                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var objContextNew = new db_KISDEntities();
                            var newResult = (from x in objContextNew.Contents
                                             where x.ContentID == contentID
                                             select x);
                            DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                            foreach (DataColumn col in dtNew.Columns)
                            {
                                // if(objSCL)
                                if (dtOld.Rows[0][col.ColumnName].ToString() != dtNew.Rows[0][col.ColumnName].ToString())
                                {
                                    SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                                    objSCLD.ChangeLogID = objSCL.ChangeLogID;
                                    objSCLD.FieldNameTxt = col.ColumnName.ToString();
                                    objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                                    objSCLD.NewValueTxt = col.ColumnName == "IsDeletedInd" ? dtNew.Rows[0][col.ColumnName].ToString() : "";
                                    objContext.SystemChangeLogDetails.Add(objSCLD);
                                    objContext.SaveChanges();
                                }
                            }
                            #endregion
                        }
                        //foreach (var value in obj)
                        //{
                        //    //objContext.Contents.Remove(value);
                        //}

                        
                    }
                }
                catch
                {
                    TempData["AlertMessage"] = "Some error occured while deleting the Menu, Please try again later.";
                }
            }
            int? Page = 1;
            var count = 1;
            using (var objContext = new db_KISDEntities())
            {
                count = objContext.Contents.Where(x => x.ContentTypeID == MenuTypeId).Count();
            }
            var page = Request.QueryString["page"] ?? "1";
            var pagesize = Request.QueryString["pagesize"] ?? "10";
            if (Convert.ToInt32(page) > 1)
                Page = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
            rvd.Add("page", Page);
            return Json(Url.Action("MenuListing", "Menu", rvd));
        }
        #endregion        

        #region Sub Menu
        [Authorize]
        /// <summary>
        /// This action method will be called when the user visits the Menu listing page.        
        /// Set the page variables value according to the menu type id
        /// Code added to update the Status of the Menu if formcollection parameters contains values for hdncheckboxselected and 
        /// hdnvalue params.
        /// </summary>
        /// <param name="page">This parameters is used for page number.It shows the records in the list of that page. </param>
        /// <param name="pagesize">This parameter is used for showing the number of records per page</param>
        /// <param name="gridSortOptions">This parameter is used for sorting the list of records in ascending/descending order</param>
        /// <param name="menutype">This parameter is used to get the type of menu and show records in the list of this type</param>
        /// <param name="fm">This parameter is used to get the Form collection of the control from view</param>
        public ActionResult SubMenuListing(int? page, int? pagesize, GridSortOptions gridSortOptions, string smt, string pid, string mtid, string smtid, FormCollection fm, string objresult)
        {
            if (string.IsNullOrEmpty(gridSortOptions.Column))
            {
                gridSortOptions.Direction = MvcContrib.Sorting.SortDirection.Descending;
            }
            var objContentService = new ContentService();

            //decrypt sub menu type id(mt)
            smt = !string.IsNullOrEmpty(Convert.ToString(smt)) ? EncryptDecrypt.Decrypt(smt) : "0";

            //decrypt parent id(mt)
            pid = !string.IsNullOrEmpty(Convert.ToString(pid)) ? EncryptDecrypt.Decrypt(pid) : "0";

            int SubMenuTypeID = Convert.ToInt32(smt);
            int ParentId = Convert.ToInt32(pid);

            using (var objContext = new db_KISDEntities())
            {
                #region Check Tab is Accessible or Not
                int TabType = 0;
                if (SubMenuTypeID == 3) { TabType = Convert.ToInt32(ModuleTypeAlias.Home); }
                if (SubMenuTypeID == 15) { TabType = Convert.ToInt32(ModuleTypeAlias.AboutUs); }
                if (SubMenuTypeID == 21) { TabType = Convert.ToInt32(ModuleTypeAlias.ContactUs); }
                if (SubMenuTypeID == 20) { TabType = Convert.ToInt32(ModuleTypeAlias.DailyNews); }
                if (SubMenuTypeID == 9) { TabType = Convert.ToInt32(ModuleTypeAlias.Downloads); }
                if (SubMenuTypeID == 6) { TabType = Convert.ToInt32(ModuleTypeAlias.Syllabus); }
                if (SubMenuTypeID == 18) { TabType = Convert.ToInt32(ModuleTypeAlias.Video); }
                if (SubMenuTypeID == 25) { TabType = Convert.ToInt32(ModuleTypeAlias.FlyPages); }

                var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
                var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
                var HasTabAccess = GetAccessibleTabAccess(TabType, Convert.ToInt32(userId));
                if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                    || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
                {
                    if (TabType == Convert.ToInt32(ModuleTypeAlias.Departments) && RoleID == Convert.ToInt32(UserType.DepartmentUser))
                    {

                    }
                    else
                        return RedirectToAction("Index", "Home");
                }
                #endregion

                ViewBag.InnerImages = new SelectList(Models.Common.GetInnerImages(), "ImageID", "TitleTxt");

                var contenttypetitle = objContext.ContentTypes.Find(SubMenuTypeID).ContentTypeNameTxt.Replace("_", " ");
                var parentcontentTypeTitle = objContext.Contents.Find(ParentId).MenuTitleTxt;
                ViewBag.Title = contenttypetitle.Replace("Sub", "") + " Page Listing";
                ViewBag.PageTitle = parentcontentTypeTitle;
                ViewBag.MenuTypeId = SubMenuTypeID;

                //****************************Fill Values if Display order contains null values*******************************************
                var displayOrderList = objContext.Contents.Where(x => x.ContentTypeID == SubMenuTypeID && x.ParentID == ParentId && x.IsDeletedInd == false).ToList();
                foreach (var item in displayOrderList)
                {
                    if (string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()))
                    {
                        var objContentData = objContext.Contents.Where(x => x.ContentID == item.ContentID && x.IsDeletedInd == false).FirstOrDefault();
                        var displayOrder1 = (displayOrderList.Max(x => x.DisplayOrderNbr)) == null ? 1 : displayOrderList.Max(x => x.DisplayOrderNbr).Value + 1;
                        objContentData.DisplayOrderNbr = displayOrder1;
                        objContext.SaveChanges();
                    }
                }
                //***********************************************************************

                #region Ajax Call
                // Ajax call type 
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
                        objAjaxRequest.ajaxcall = null; ;//remove parameter value
                    }

                    //Ajax Call for update status for images
                    if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && objresult != null && !string.IsNullOrEmpty(objresult))
                    {
                        var contentID = Convert.ToInt64(objAjaxRequest.hfid);
                        var objMenuContent = objContext.Contents.Find(contentID);
                        if (objMenuContent != null)
                        {
                            #region System Change Log
                            var oldresult = (from a in objContext.Contents
                                             where a.ContentID == contentID
                                             select a).ToList();
                            DataTable dtOld = Models.Common.LINQResultToDataTable(oldresult);
                            #endregion

                            objMenuContent.StatusInd = objAjaxRequest.hfvalue == "1" ? true : false;
                            objContext.SaveChanges();
                            TempData["AlertMessage"] = "Status updated successfully.";

                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Menu";
                            objSCL.LogTypeTxt = objMenuContent.ContentID > 0 ? "Update" : "Add";
                            objSCL.NotesTxt = "Menu Details" + (objMenuContent.ContentID > 0 ? " updated for " : "  added for ") + objMenuContent.MenuTitleTxt;
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();

                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in objContext.Contents
                                             where x.ContentID == objMenuContent.ContentID
                                             select x);
                            DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                            foreach (DataColumn col in dtNew.Columns)
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
                            #endregion
                        }

                        objAjaxRequest.hfid = null;//remove parameter value
                        objAjaxRequest.hfvalue = null;//remove parameter value
                        pagesize = (Request.QueryString["pagesize"] != null ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : pagesize);
                        page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    //*******DisplayOrder Sub Menu*************************** 
                    else if (objAjaxRequest.qs_checkboxselected != null && objAjaxRequest.qs_value != null && !string.IsNullOrEmpty(objAjaxRequest.qs_checkboxselected)
                    && !string.IsNullOrEmpty(objAjaxRequest.qs_value) && objAjaxRequest.qs_Type.Trim().ToLower() == "displayorder".Trim().ToLower())
                    {
                        var contentID = Convert.ToInt64(objAjaxRequest.qs_checkboxselected);
                        var objMenuContent = objContext.Contents.Find(contentID);
                        if (objMenuContent != null)
                        {
                            try
                            {
                                if (objContentService.ChangeImageDisplayOrderSubMenu(objMenuContent.DisplayOrderNbr.Value, Convert.ToInt32(objAjaxRequest.qs_value), Convert.ToInt32(objMenuContent.ContentID), SubMenuTypeID, ParentId))
                                {
                                    TempData["AlertMessage"] = "Display Order has been changed successfully.";
                                }
                            }
                            catch
                            {
                                TempData["AlertMessage"] = "Some Error Occured while changing Display Order, Please try again later.";
                            }
                            objAjaxRequest.qs_checkboxselected = null;//remove parameter value
                            objAjaxRequest.qs_Type = null;//remove parameter value
                            objAjaxRequest.qs_value = null;//remove parameter value

                            pagesize = (Request.QueryString["pagesize"] != null ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : pagesize);
                            page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                            gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                        }
                    }
                    //***************************************************************************************
                    else
                    {
                        TempData["AlertMessage"] = string.Empty;
                    }
                    objresult = string.Empty;
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
                    gridSortOptions.Column = "ContentCreateDate";
                    Session["PageSize"] = null;
                    Session["pageNo"] = null;
                    Session["GridSortOption"] = null;
                }
                //.. Code for get records as Page View Model
                var pageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
                var Page = page.HasValue ? page.Value : Models.Common._currentPage;
                TempData["pager"] = pageSize;
                if (gridSortOptions.Column != null)
                {
                    if (
                        gridSortOptions.Column == "MenuTitleTxt" || gridSortOptions.Column == "PageTitleTxt" ||
                        gridSortOptions.Column == "ContentCreateDate" || gridSortOptions.Column == "DisplayOrderNbr")
                    {
                    }
                    else
                    {
                        gridSortOptions.Column = "ContentCreateDate";
                    }
                }
                var pagedViewModel = new PagedViewModel<ContentModel>
                {
                    ViewData = ViewData,
                    Query = objContentService.GetSubMenus(SubMenuTypeID, ParentId).AsQueryable(),
                    GridSortOptions = gridSortOptions,
                    DefaultSortColumn = "ContentCreateDate",
                    Page = Page,
                    PageSize = pageSize,
                }
               .Setup();
                if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
                {
                    return View("SubMenuPartial", pagedViewModel);// ("partial view name ")
                }
                else
                {
                    return View(pagedViewModel);
                }
            }
        }

        [Authorize]
        /// <summary>
        /// This method will be called when admin add  or edits the Menu page.
        /// </summary>
        /// <param name="contenttype"></param>
        /// <returns></returns>
        public ActionResult CreateSubMenu(string smt, string pid, string cid, string mtid, string smtid)
        {
            var objContentModel = new ContentModel();
            Session["Edit/Delete"] = "Edit";
            ViewBag.FocusPageUrl = false;// Set focus on Pageurl Field if same url exist

            //decrypt sub menu type id(mt)
            smt = !string.IsNullOrEmpty(Convert.ToString(smt)) ? EncryptDecrypt.Decrypt(smt) : "0";
            //decrypt parent id(mt)
            pid = !string.IsNullOrEmpty(Convert.ToString(pid)) ? EncryptDecrypt.Decrypt(pid) : "0";
            //decrypt content id(mt)
            cid = !string.IsNullOrEmpty(Convert.ToString(cid)) ? EncryptDecrypt.Decrypt(cid) : "0";
            //decrypt sub menu type id(mt)
            mtid = !string.IsNullOrEmpty(Convert.ToString(mtid)) ? EncryptDecrypt.Decrypt(mtid) : "0";
            int SubMenuTypeID = Convert.ToInt32(smt);
            long ParentId = Convert.ToInt64(pid);
            long ContentID = Convert.ToInt64(cid);
           
            using (var objContext = new db_KISDEntities())
            {
                var objMenuTypeID = objContext.Contents.Where(x => x.ContentID == ParentId).FirstOrDefault();

                long MenuTypeID = objMenuTypeID.ContentTypeID;

                #region Check Tab is Accessible or Not
                int TabType = 0;
                if (SubMenuTypeID == 3) { TabType = Convert.ToInt32(ModuleTypeAlias.Home); }
                if (SubMenuTypeID == 15) { TabType = Convert.ToInt32(ModuleTypeAlias.AboutUs); }
                if (SubMenuTypeID == 21) { TabType = Convert.ToInt32(ModuleTypeAlias.ContactUs); }
                if (SubMenuTypeID == 20) { TabType = Convert.ToInt32(ModuleTypeAlias.DailyNews); }
                if (SubMenuTypeID == 9) { TabType = Convert.ToInt32(ModuleTypeAlias.Downloads); }
                if (SubMenuTypeID == 6) { TabType = Convert.ToInt32(ModuleTypeAlias.Syllabus); }
                if (SubMenuTypeID == 18) { TabType = Convert.ToInt32(ModuleTypeAlias.Video); }
                if (SubMenuTypeID == 25) { TabType = Convert.ToInt32(ModuleTypeAlias.FlyPages); }

                var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
                var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
                var HasTabAccess = GetAccessibleTabAccess(TabType, Convert.ToInt32(userId));
                if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                    || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
                {
                    if (TabType == Convert.ToInt32(ModuleTypeAlias.Departments) && RoleID == Convert.ToInt32(UserType.DepartmentUser))
                    {

                    }
                    else
                        return RedirectToAction("Index", "Home");
                }
                #endregion

                ViewBag.InnerImages = new SelectList(Models.Common.GetInnerImages(), "ImageID", "TitleTxt");
                ViewBag.Menu = SubMenuTypeID;
                objContentModel.ContentTypeID = SubMenuTypeID;
                var objContentData = objContext.Contents.Where(x => x.ContentID == ContentID).FirstOrDefault();
                var contenttypetitle = objContext.ContentTypes.Find(MenuTypeID).ContentTypeNameTxt;
                ViewBag.PageTitle = contenttypetitle + " Page Listing";
                ViewBag.BreadCrumTtile = contenttypetitle+ " Page Listing";

                var parentcontentTypeTitle = objContext.Contents.Find(ParentId).MenuTitleTxt;
                ViewBag.ParentcontentTypeTitle = parentcontentTypeTitle;

                objContentModel.strCreateDate = DateTime.Today.ToString("MM/dd/yyyy");
                objContentModel.ParentID = ParentId;

                objContentModel.RightSections = GetAllRightSections();

                if (objContentData != null)
                {
                    objContentModel.ContentID = objContentData.ContentID;
                    objContentModel.ParentID = objContentData.ParentID;
                    objContentModel.ContentID = objContentData.ContentID;
                    objContentModel.ContentTypeID = objContentData.ContentTypeID;
                    objContentModel.IsExternalLinkInd = objContentData.IsExternalLinkInd;
                    objContentModel.ExternalLinkTxt = objContentData.ExternalLinkTxt;
                    objContentModel.ExternalLinkTargetInd = objContentData.ExternalLinkTargetInd;
                    objContentModel.PageTitleTxt = objContentData.PageTitleTxt;
                    objContentModel.MenuTitleTxt = objContentData.MenuTitleTxt;
                    objContentModel.PageURLTxt = objContentData.PageURLTxt;
                    objContentModel.BannerImageID = objContentData.BannerImageID;
                    objContentModel.AbstractTxt = objContentData.AbstractTxt;
                    objContentModel.DescriptionTxt = objContentData.DescriptionTxt;
                    objContentModel.DisplayOrderNbr = objContentData.DisplayOrderNbr.Value;//Display Order Num
                    objContentModel.strCreateDate = objContentData.ContentCreateDate.Value.ToString("MM/dd/yyyy");
                    objContentModel.StatusInd = objContentData.StatusInd;
                    objContentModel.PageMetaTitleTxt = objContentData.PageMetaTitleTxt;
                    objContentModel.PageMetaDescriptionTxt = objContentData.PageMetaDescriptionTxt;
                    objContentModel.AltBannerImageTxt = objContentData.AltBannerImageTxt;
                    objContentModel.BannerImageAbstractTxt = objContentData.BannerImageAbstractTxt;
                    objContentModel.RightSectionTitleTxt = objContentData.RightSectionTitleTxt;
                    objContentModel.RightSectionAbstractTxt = objContentData.RightSectionAbstractTxt;
                    objContentModel.ContentTypeTitle = "Edit" +contenttypetitle+ " Sub Menu Page Details";
                    objContentModel.IsGooglePlusSharingInd = objContentData.IsGooglePlusSharingInd.HasValue ? objContentData.IsGooglePlusSharingInd.Value : false;
                    objContentModel.IsFacebookSharingInd = objContentData.IsFacebookSharingInd.HasValue ? objContentData.IsFacebookSharingInd.Value : false;
                    objContentModel.IsTwitterSharingInd = objContentData.IsTwitterSharingInd.HasValue ? objContentData.IsTwitterSharingInd.Value : false;


                    ViewBag.IsActiveInd = GetStatusData(objContentModel.StatusInd == true ? "1" : "0");
                    ViewBag.Submit = "Update";
                }
                else
                {
                    ViewBag.Submit = "Save";
                    objContentModel.ContentTypeID = SubMenuTypeID;
                    objContentModel.ContentTypeTitle = "Add " + contenttypetitle + " Sub Menu Page Details";
                    objContentModel.ContentCreateDate = DateTime.Now;
                    objContentModel.IsGooglePlusSharingInd = false;
                    objContentModel.IsFacebookSharingInd = false;
                    objContentModel.IsTwitterSharingInd = false;
                    ViewBag.IsActiveInd = GetStatusData(string.Empty);
                }
                ViewBag.Title = (objContentData != null ? " Edit " : " Add ") + contenttypetitle + " Sub Menu Page Details";
                return View(objContentModel);
            }
        }

        /// <summary>
        /// Save update the Menu pages              
        /// </summary>
        /// <param name="model">Intialized ContentModel model object from view</param>        
        /// <param name="command">Defines Submit or Cancel </param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateSubmenu(ContentModel model, string command, FormCollection fm)
        {
            var rvd = new RouteValueDictionary();
            var mtid = Request.QueryString["mtid"] ?? EncryptDecrypt.Encrypt("0");
            var smtid = Request.QueryString["smtid"] ?? EncryptDecrypt.Encrypt("0");
            rvd.Add("page", Request.QueryString["page"] ?? "1");
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "ContentCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            rvd.Add("mt", Request.QueryString["mt"]);
            rvd.Add("smt", Request.QueryString["smt"]);
            rvd.Add("mtid", mtid);
            rvd.Add("smtid", smtid);
            rvd.Add("pid", EncryptDecrypt.Encrypt(model.ParentID.ToString()));
            rvd.Add("cid", EncryptDecrypt.Encrypt(model.ContentID.ToString()));
            rvd.Add("mpage", Request.QueryString["mpage"] ?? "1");
            rvd.Add("mpagesize", Request.QueryString["mpagesize"] ?? "10");
            rvd.Add("mColumn", Request.QueryString["mColumn"] ?? "ContentCreateDate");
            rvd.Add("mDirection", Request.QueryString["mDirection"] ?? "Descending");
            rvd.Add("DisplayOrderNum", model.DisplayOrderNbr);//Display Order Num
            ViewBag.FocusPageUrl = false;// Set focus on Pageurl Field if same url exist
            using (var objContext = new db_KISDEntities())
            {
                if (string.IsNullOrEmpty(command))
                {
                    try
                    {
                        #region System Change Log
                        DataTable dtOld;
                        var oldResult = (from a in objContext.Contents
                                         where a.ContentID == model.ContentID
                                         select a).ToList();
                        dtOld = Models.Common.LINQResultToDataTable(oldResult);
                        #endregion

                        var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
                        model.RightSections = GetAllRightSections();

                        ViewBag.InnerImages = new SelectList(GetInnerImages(), "ImageID", "TitleTxt");
                        ViewBag.Menu = model.ContentTypeID;
                        ViewBag.Submit = model.ContentID == 0 ? "Save" : "Update";
                        var contentType = model.ContentTypeID;
                        var contentTypeTitle = objContext.ContentTypes.Find(contentType).ContentTypeNameTxt;
                        var parentcontentTypeTitle = objContext.Contents.Find(model.ParentID).MenuTitleTxt;
                        ViewBag.Title = (model.ContentID > 0 ? "Edit " : "Add ") + contentTypeTitle;
                        ViewBag.ContentTypeTitle = contentTypeTitle;
                        ViewBag.ParentcontentTypeTitle = parentcontentTypeTitle;
                        ViewBag.BreadCrumTtile = contentTypeTitle + " Page Listing";
                        var objContentData = objContext.Contents.Where(x => x.ContentID == model.ContentID).FirstOrDefault();
                        if (model != null && !string.IsNullOrEmpty(model.PageURLTxt) && model.IsExternalLinkInd == false)
                        {
                            var count = 0;
                            count = objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.ContentID != model.ContentID && x.IsDeletedInd == false).Count();
                            count += objContext.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.Departments.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.ExceptionOpportunities.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.NewsEvents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.RightSections.Where(x => x.ExternalLinkURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            if (model.PageURLTxt.Trim().ToLower() == "error404")
                            {
                                count = count + 1;
                            }
                            if (count > 0)
                            {
                                if (model.PageURLTxt.ToLower().Trim() == "error404")//if user types url 'error404' below validation msg should display
                                {
                                    ModelState.AddModelError("PageURLTxt", model.PageURLTxt + " URL is not allowed.");
                                }
                                else
                                {
                                    ModelState.AddModelError("PageURLTxt", model.PageURLTxt + " URL already exists.");
                                }

                                ViewBag.FocusPageUrl = true;// Set focus on Pageurl Field if same url exist
                                ViewBag.IsActiveInd = GetStatusData(fm["IsActiveInd"].ToString() == "1" ? "1" : "0");
                                return View(model);
                            }
                        }
                        var IsNew = objContentData == null;
                        if (objContentData == null)
                        {
                            objContentData = new Content();
                        }

                        objContentData.IsExternalLinkInd = model.IsExternalLinkInd;
                        objContentData.ParentID = model.ParentID;
                        objContentData.ExternalLinkTxt = model.IsExternalLinkInd ? (model.ExternalLinkTxt ?? string.Empty) : string.Empty;
                        objContentData.AbstractTxt = model.AbstractTxt ?? string.Empty;
                        objContentData.PageTitleTxt = model.IsExternalLinkInd ? string.Empty : model.PageTitleTxt ?? string.Empty;
                        objContentData.PageURLTxt = model.IsExternalLinkInd ? string.Empty : (string.IsNullOrEmpty(model.PageURLTxt) ? string.Empty : model.PageURLTxt);
                        objContentData.BannerImageID = model.IsExternalLinkInd ? null : model.BannerImageID;
                        objContentData.DescriptionTxt = model.IsExternalLinkInd ? string.Empty : model.DescriptionTxt ?? string.Empty;
                        objContentData.PageMetaTitleTxt = model.IsExternalLinkInd ? string.Empty : model.PageMetaTitleTxt ?? string.Empty;
                        objContentData.PageMetaDescriptionTxt = model.IsExternalLinkInd ? string.Empty : model.PageMetaDescriptionTxt ?? string.Empty;
                        objContentData.ExternalLinkTargetInd = model.IsExternalLinkInd ? (model.ExternalLinkTargetInd) : false;
                        objContentData.AltBannerImageTxt = model.IsExternalLinkInd ? string.Empty : model.AltBannerImageTxt ?? string.Empty;
                        objContentData.BannerImageAbstractTxt = model.IsExternalLinkInd ? string.Empty : model.BannerImageAbstractTxt ?? string.Empty;
                        objContentData.RightSectionTitleTxt = model.IsExternalLinkInd ? string.Empty : model.RightSectionTitleTxt ?? string.Empty;
                        objContentData.RightSectionAbstractTxt = model.IsExternalLinkInd ? string.Empty : model.RightSectionAbstractTxt ?? string.Empty;
                        objContentData.IsDeletedInd = false;
                        objContentData.IsFacebookSharingInd = model.IsFacebookSharingInd;
                        objContentData.IsGooglePlusSharingInd = model.IsGooglePlusSharingInd;
                        objContentData.IsTwitterSharingInd = model.IsTwitterSharingInd;

                        DateTime dt_to = Convert.ToDateTime(model.strCreateDate, System.Globalization.CultureInfo.InvariantCulture);
                        objContentData.ContentCreateDate = dt_to;

                        //objContentData.CreateDate = Convert.ToDateTime(model.strCreateDate);
                        objContentData.StatusInd = fm["IsActiveInd"] == "1" ? true : false;
                        objContentData.MenuTitleTxt = model.MenuTitleTxt ?? string.Empty;
                        objContentData.ContentTypeID = contentType;

                        objContentData.CreateDate = model.ContentID > 0 ? objContentData.CreateDate : DateTime.Now; ;
                        objContentData.CreateByID = model.ContentID > 0 ? objContentData.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        objContentData.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        objContentData.LastModifyDate = DateTime.Now;

                        //*************Display Order ************************
                        if (model.ContentID == 0)
                        {
                            objContentData.ContentCreateDate = DateTime.Now;
                            var MenuCount = objContext.Contents.Where(x => x.ContentTypeID == model.ContentTypeID && x.ParentID == model.ParentID && (x.IsDeletedInd == false || x.IsDeletedInd==null)).ToList();
                            objContentData.DisplayOrderNbr = MenuCount.Any() ? objContext.Contents.Where(x => x.ContentTypeID == model.ContentTypeID && x.ParentID == model.ParentID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Max(x => x.DisplayOrderNbr) + 1 : 1;
                            objContext.Contents.Add(objContentData);
                        }
                        //**************************************************
                        if (IsNew)
                        {
                            objContext.Contents.Add(objContentData);
                        }


                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = "Menu";
                        objSCL.LogTypeTxt = model.ContentID > 0 ? "Update" : "Add";
                        objSCL.NotesTxt = (GetContentType(model.ContentTypeID)) + " Details" + (model.ContentID > 0 ? " updated for " : "  added for ") + model.PageTitleTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();
                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.Contents
                                         where x.ContentID == objContentData.ContentID
                                         select x);
                        DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                        foreach (DataColumn col in dtNew.Columns)
                        {
                            if (model.ContentID > 0)
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

                        TempData["AlertMessage"] = parentcontentTypeTitle + " sub menu page details" + (IsNew ? " saved successfully." : " updated successfully.");
                        objContext.SaveChanges();
                    }
                    catch
                    {
                        TempData["AlertMessage"] = "Some error occured, Please try after some time.";
                    }
                    return RedirectToAction("SubMenuListing", rvd);
                }
                else
                {
                    return RedirectToAction("SubMenuListing", rvd);
                }
            }
        }

        /// <summary>
        /// Delete the Menu 
        /// </summary>
        /// <param name="contentID">contentID  </param>
        /// <param name="MenuTypeId">Type of content</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteSubmenu(string cid, string smt, string mtid, string smtid)
        {
            {
                //decrypt sub menu type id(mt)
                smt = !string.IsNullOrEmpty(Convert.ToString(smt)) ? EncryptDecrypt.Decrypt(smt) : "0";

                //decrypt parent id(mt)
                cid = !string.IsNullOrEmpty(Convert.ToString(cid)) ? EncryptDecrypt.Decrypt(cid) : "0";
                mtid = !string.IsNullOrEmpty(Convert.ToString(mtid)) ? EncryptDecrypt.Decrypt(mtid) : "0";
                smtid = !string.IsNullOrEmpty(Convert.ToString(smtid)) ? EncryptDecrypt.Decrypt(smtid) : "0";
                int SubMenuTypeID = Convert.ToInt32(smt);
                int ContentID = Convert.ToInt32(cid);

                var rvd = new RouteValueDictionary();
                rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
                rvd.Add("Column", Request.QueryString["Column"] ?? "ContentCreateDate");
                rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
                rvd.Add("mt", Request.QueryString["mt"]);
                rvd.Add("smt", Request.QueryString["smt"]);
                rvd.Add("mtid", Request.QueryString["mtid"]);
                rvd.Add("smtid", Request.QueryString["smtid"]);
                rvd.Add("cid", EncryptDecrypt.Encrypt(ContentID.ToString()));
                rvd.Add("mpagesize", Request.QueryString["mpagesize"] ?? "10");
                rvd.Add("mColumn", Request.QueryString["mColumn"] ?? "ContentCreateDate");
                rvd.Add("mDirection", Request.QueryString["mDirection"] ?? "Descending");
                rvd.Add("pid", Request.QueryString["pid"] ?? EncryptDecrypt.Encrypt("0"));
                Session["Edit/Delete"] = "Delete";
                if (ContentID > 0)
                {
                    try
                    {
                        using (var _objdbContext = new db_KISDEntities())
                        {

                            var obj = _objdbContext.Contents.Where(x => x.ContentID == ContentID).FirstOrDefault();
                            //_objdbContext.Contents.Remove(obj);
                            if (obj != null)
                            {
                                #region System Change Log
                                DataTable dtOld;
                                var oldResult = (from a in _objdbContext.Contents
                                                 where a.ContentID == ContentID
                                                 select a).ToList();
                                dtOld = Models.Common.LINQResultToDataTable(oldResult);
                                #endregion

                                //****************Display Order ************************
                                var objContentMenu = _objdbContext.Contents.Where(x => x.ContentID == ContentID).FirstOrDefault();
                                if (objContentMenu != null)
                                {
                                    try
                                    {
                                        var objModelService = new ContentService();
                                        var parentid = Request.QueryString["ParentId"] ?? "0";
                                        objModelService.ChangeDeletedDisplayOrderSubMenu(objContentMenu.DisplayOrderNbr.Value, objContentMenu.ContentID, SubMenuTypeID, Convert.ToInt32(parentid));
                                    }
                                    catch { }
                                }
                                //***************************************************
                                //_objdbContext.SaveChanges();
                                TempData["AlertMessage"] = objContentMenu.MenuTitleTxt + " sub menu details deleted successfully.";

                                #region Delete Selected Right Section for the Menu
                                var TypeMasterID = Convert.ToInt64(smtid);
                                var rightSections = _objdbContext.RightSections.Where(x => x.ListingID == ContentID && x.TypeMasterID == TypeMasterID).ToList();
                                if (rightSections != null && rightSections.Count() > 0)
                                {
                                    foreach (var section in rightSections)
                                    {
                                        section.IsDeletedInd = true;
                                    }
                                    _objdbContext.SaveChanges();
                                }
                                #endregion

                                #region System Change Log
                                SystemChangeLog objSCL = new SystemChangeLog();
                                long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                                User objuser = _objdbContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                                objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                                objSCL.UsernameTxt = objuser.UserNameTxt;
                                objSCL.UserRoleID = (short)_objdbContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                                objSCL.ModuleTxt = "Sub Menu";
                                objSCL.LogTypeTxt = "Delete";
                                objSCL.NotesTxt = obj.MenuTitleTxt + " Details deleted.";
                                objSCL.LogDateTime = DateTime.Now;
                                _objdbContext.SystemChangeLogs.Add(objSCL);
                                _objdbContext.SaveChanges();
                                objSCL = _objdbContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                                var newResult = (from x in _objdbContext.Contents
                                                 where x.ContentID == ContentID
                                                 select x);
                                DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
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
                                        _objdbContext.SystemChangeLogDetails.Add(objSCLD);
                                        _objdbContext.SaveChanges();
                                    }
                                }
                                #endregion
                            }

                
                        }
                    }
                    catch
                    {
                        TempData["AlertMessage"] = "Some error occured while deleting the Sub Menu, Please try again later.";
                    }
                }
                int? Page = 1;
                var count = 1;
                using (var _objdbContext = new db_KISDEntities())
                {
                    count = _objdbContext.Contents.Where(x => x.ContentTypeID == SubMenuTypeID).Count();
                }
                var page = Request.QueryString["page"] ?? "1";
                var pagesize = Request.QueryString["pagesize"] ?? "10";
                if (Convert.ToInt32(page) > 1)
                    Page = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
                rvd.Add("page", Page);
                return Json(Url.Action("SubMenuListing", "Menu", rvd));
            }
        }
        #endregion
 

        #region Private method of the page
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
        /// to get all inner images titles
        /// </summary>
        /// <param name="ImgId"></param>
        /// <returns></returns>
        public ActionResult GetDataForInnerImages(int ImgId)
        {
            db_KISDEntities db_Context = new db_KISDEntities();
            var data = (from db in db_Context.Images
                        where db.ImageID == ImgId
                        select new { db.AltImageTxt, db.AbstractTxt }).FirstOrDefault();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CheckURL(string url)
        {
            var objContext = new db_KISDEntities();
            var count = 0;

            //count = objContext.Events.Where(x => x.PageURLTxt.Contains(url)).Count();
            //count = objContext.News.Where(x => x.PageURLTxt.Contains(url)).Count();
            count += objContext.Contents.Where(x => x.PageURLTxt.Contains(url)).Count();
            if (count > 0)
            {
                if (url != "")
                    url = url + count;
            }
            return Json(url);
        }

        private string GetContentType(long ContentTypeID)
        {
            string s = string.Empty;
            using (var objContext = new db_KISDEntities())
            {
                s = objContext.ContentTypes.Where(x => x.ContentTypeID == ContentTypeID).Select(x => x.ContentTypeNameTxt).FirstOrDefault();
            }
            return s;
        }

        private SelectList GetAllRightSections()
        {
            var objContext = new db_KISDEntities();
            var list = objContext.RightSections.Where(x => x.StatusInd == true).OrderBy(x => x.TitleTxt).Select(x => new RightSectionModel()
            {
                RightSectionID = x.RightSectionID,
                TitleTxt = x.TitleTxt
            }).ToList();
            var obj = new RightSectionModel();
            obj.TitleTxt = "--- Select Right Section ---";
            obj.RightSectionID = 0;
            list.Insert(0, obj);
            var objselectlist = new SelectList(list, "RightSectionID", "TitleTxt");
            return objselectlist;
        }
        #endregion
    }
}
