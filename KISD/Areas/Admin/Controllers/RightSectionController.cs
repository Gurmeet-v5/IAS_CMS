using KISD.Areas.Admin.Models;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ContentTypeAlias = KISD.Areas.Admin.Models.ContentType;
using ModuleTypeAlias = KISD.Areas.Admin.Models.Common.ModuleType;
using System.Web.Security;
using static KISD.Areas.Admin.Models.Common;

namespace KISD.Areas.Admin.Controllers
{
    public class RightSectionController : Controller
    {
        #region Right Section
        [Authorize]
        /// <summary>
        /// This action method will be called when the user visits the Right Section listing page.        
        /// Code added to update the Status of the Menu if formcollection parameters contains values for        hdncheckboxselected and 
        /// hdnvalue params.
        /// </summary>
        /// <param name="page">This parameters is used for page number.It shows the records in the list of that page. </param>
        /// <param name="pagesize">This parameter is used for showing the number of records per page</param>
        /// <param name="gridSortOptions">This parameter is used for sorting the list of records in ascending/descending order</param>
        /// <param name="fm">This parameter is used to get the Form collection of the control from view</param>
        /// <param name="ct">Type of Content</param>
        /// <param name="mtid">Type of Master</param>
        public ActionResult RightSectionListing(string ct, string mtid, int? page, int? pagesize, GridSortOptions gridSortOptions, FormCollection fm, string objresult)
        {
            db_KISDEntities objContext = new db_KISDEntities();
            if (string.IsNullOrEmpty(gridSortOptions.Column))
            {
                gridSortOptions.Direction = MvcContrib.Sorting.SortDirection.Descending;
            }
            //decrypt content type id(ct)
            ct = !string.IsNullOrEmpty(Convert.ToString(ct)) ? EncryptDecrypt.Decrypt(ct) : "0";
            long contentID = !string.IsNullOrEmpty(ct) ? Convert.ToInt64(ct) : 0;
            mtid = !string.IsNullOrEmpty(Convert.ToString(mtid)) ? EncryptDecrypt.Decrypt(mtid) : "0";
            Int64 TypeMasterID = !string.IsNullOrEmpty(mtid) ? Convert.ToInt32(mtid) : 0;

            var objRightSectionService = new RightSectionService();
            using (objContext = new db_KISDEntities())
            {
                
                if(contentID <= 0 || TypeMasterID <= 0)
                {
                    TempData["AlertMessage"] = "Please add content page first.";
                    return RedirectToAction("Index", "Home");
                }
                #region Check Tab is Accessible or Not
                var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
                var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
                var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Images), Convert.ToInt32(userId));
                if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                    || RoleID == Convert.ToInt32(UserType.Admin) || RoleID == Convert.ToInt32(UserType.DepartmentUser)))//if tab not accessible then redirect to home
                {
                    return RedirectToAction("Index", "Home");
                }
                #endregion
                var TypeMasterName = objContext.TypeMasters.Where(x => x.TypeMasterID == TypeMasterID).Select(x => x.TypeMasterName).FirstOrDefault();
                ViewBag.PageTitle = TypeMasterName+" - Right Section Listing";
                ViewBag.Title = TypeMasterName + " - Right Section Listing";
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
                        var RightSectionID = Convert.ToInt64(objAjaxRequest.hfid);
                        var objRightSection = objContext.RightSections.Find(RightSectionID);
                        #region System Change Log
                        var oldresult = (from a in objContext.RightSections
                                         where a.RightSectionID == RightSectionID
                                         select a).ToList();
                        DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                        #endregion
                        if (objRightSection != null)
                        {
                            objRightSection.StatusInd = objAjaxRequest.hfvalue == "1" ? true : false;
                            objContext.SaveChanges();
                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Right Section";
                            objSCL.LogTypeTxt = objRightSection.RightSectionID > 0 ? "Update" : "Add";
                            objSCL.NotesTxt = "Right Section Details" + (objRightSection.RightSectionID > 0 ? " updated for " : "  added for ") + objRightSection.TitleTxt;
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();

                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in objContext.RightSections
                                             where x.RightSectionID == RightSectionID
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
                        objAjaxRequest.hfid = null;//remove parameter value
                        objAjaxRequest.hfvalue = null;//remove parameter value
                        pagesize = (Request.QueryString["pagesize"] != null ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : pagesize);
                        page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
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
                    gridSortOptions.Column = "RightSectionCreateDate";
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
                    if (gridSortOptions.Column == "ExternalLinkURLTxt"
                        || gridSortOptions.Column == "TitleTxt" || gridSortOptions.Column == "PageTitleTxt"
                        || gridSortOptions.Column == "RightSectionCreateDate")
                    {

                    }
                    else
                    {
                        gridSortOptions.Column = "RightSectionCreateDate";
                    }
                }
                var pagedViewModel = new PagedViewModel<RightSectionModel>
                {
                    ViewData = ViewData,
                    Query = objRightSectionService.GetRightSectionView(contentID, TypeMasterID).AsQueryable(),
                    GridSortOptions = gridSortOptions,
                    DefaultSortColumn = "RightSectionCreateDate",
                    Page = Page,
                    PageSize = pageSize,
                }
               .Setup();
                if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
                {
                    return View("RightSectionPartial", pagedViewModel);// ("partial view name ")
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
        /// <param name="rsid">RightSectionID</param>
        /// <param name="ct">Type of Content</param>
        /// <param name="mtid">Type of Master</param>
        /// <returns></returns>
        public ActionResult Create(string rsid, string ct, string mtid)
        {
            rsid = !string.IsNullOrEmpty(Convert.ToString(rsid)) ? EncryptDecrypt.Decrypt(rsid) : "0";
            // decrypt content type id(ct)
            ct = !string.IsNullOrEmpty(Convert.ToString(ct)) ? EncryptDecrypt.Decrypt(ct) : "0";
            Int64 ContentTypeID = !string.IsNullOrEmpty(ct) ? Convert.ToInt64(ct) : 0;
            mtid = !string.IsNullOrEmpty(Convert.ToString(mtid)) ? EncryptDecrypt.Decrypt(mtid) : "0";
            Int64 TypeMasterID = !string.IsNullOrEmpty(mtid) ? Convert.ToInt64(mtid) : 0;
            if (TypeMasterID <= 0)
            {
                return RedirectToAction("Index", "Home");
            }
            var objRightSectionModel = new RightSectionModel();
            Session["Edit/Delete"] = "Edit";
            ViewBag.FocusPageUrl = false;// Set focus on Pageurl Field if same url exist
            int RightSectionId = Convert.ToInt32(rsid);
            using (var objContext = new db_KISDEntities())
            {
                #region Check Tab is Accessible or Not
                var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
                var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
                var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Images), Convert.ToInt32(userId));
                if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                    || RoleID == Convert.ToInt32(UserType.Admin) || RoleID == Convert.ToInt32(UserType.DepartmentUser)))//if tab not accessible then redirect to home
                {
                    return RedirectToAction("Index", "Home");
                }
                #endregion

                var TypeMasterName = objContext.TypeMasters.Where(x => x.TypeMasterID == TypeMasterID).Select(x => x.TypeMasterName).FirstOrDefault();
                var objRightSectionData = objContext.RightSections.Where(x => x.RightSectionID == RightSectionId && (x.IsDeletedInd == false || x.IsDeletedInd == null)).FirstOrDefault();
                ViewBag.BreadCrumTtile = TypeMasterName + " - Right Section Listing";
                ViewBag.InnerImages = new SelectList(Models.Common.GetInnerImages(), "ImageID", "TitleTxt");
                objRightSectionModel.strRightSectionCreateDate = System.DateTime.Today.ToShortDateString();
                ViewBag.Title = (objRightSectionData != null ? " Edit " : " Add ") + " Right Section Details";
                if (objRightSectionData != null)
                {

                    if (objRightSectionData.IsParentTitleInd == false && objRightSectionData.IsExternalLinkInd == true)
                    {
                        objRightSectionModel.ExternalLinkURLTxt = objRightSectionData.ExternalLinkURLTxt;
                        objRightSectionModel.ExternalLinkTargetInd = objRightSectionData.ExternalLinkTargetInd;
                    }
                    else if (objRightSectionData.IsParentTitleInd == false && objRightSectionData.IsExternalLinkInd == false)
                    {
                        objRightSectionModel.PageURLTxt = objRightSectionData.ExternalLinkURLTxt;
                        objRightSectionModel.BannerImageID = objRightSectionData.BannerImageID;
                        objRightSectionModel.AltBannerImageTxt = objRightSectionData.AltBannerImageTxt;//Display Order Num
                        objRightSectionModel.BannerImageAbstractTxt = objRightSectionData.BannerImageAbstractTxt;

                        objRightSectionModel.RightSectionTitleTxt = objRightSectionData.RightSectionTitleTxt;
                        objRightSectionModel.RightSectionAbstractTxt = objRightSectionData.RightSectionAbstractTxt;
                        objRightSectionModel.DescriptionTxt = objRightSectionData.DescriptionTxt;
                        objRightSectionModel.PageTitleTxt = objRightSectionData.PageTitleTxt;
                        objRightSectionModel.PageMetaTitleTxt = objRightSectionData.PageMetaTitleTxt;
                        objRightSectionModel.PageMetaDescriptionTxt = objRightSectionData.PageMetaDescriptionTxt;
                        objRightSectionModel.IsGooglePlusSharingInd = objRightSectionData.IsGooglePlusSharingInd.HasValue ? objRightSectionData.IsGooglePlusSharingInd.Value : false;
                        objRightSectionModel.IsFacebookSharingInd = objRightSectionData.IsFacebookSharingInd.HasValue ? objRightSectionData.IsFacebookSharingInd.Value : false;
                        objRightSectionModel.IsTwitterSharingInd = objRightSectionData.IsTwitterSharingInd.HasValue ? objRightSectionData.IsTwitterSharingInd.Value : false;
                    }
                    objRightSectionModel.RightSectionID = objRightSectionData.RightSectionID;
                    objRightSectionModel.IsParentTitleInd = objRightSectionData.IsParentTitleInd;
                    objRightSectionModel.TitleTxt = objRightSectionData.TitleTxt;
                    objRightSectionModel.IsExternalLinkInd = objRightSectionData.IsExternalLinkInd;
                    objRightSectionModel.StatusInd = objRightSectionData.StatusInd;
                    objRightSectionModel.ParentID = objRightSectionData.ParentID;
                    objRightSectionModel.RightSectionCreateDate = objRightSectionData.RightSectionCreateDate;
                    objRightSectionModel.strRightSectionCreateDate = objRightSectionData.RightSectionCreateDate.HasValue ? objRightSectionData.RightSectionCreateDate.Value.ToShortDateString() : "";
                    objRightSectionModel.CreatedByID = objRightSectionData.CreatedByID;
                    objRightSectionModel.LastModifyDate = objRightSectionData.LastModifyDate;
                    objRightSectionModel.LastModifyByID = objRightSectionData.LastModifyByID;
                    objRightSectionModel.IsDeletedInd = objRightSectionData.IsDeletedInd;
                    objRightSectionModel.TableNameTxt = objRightSectionData.TableNameTxt;
                    objRightSectionModel.ListingID = objRightSectionData.ListingID;
                    objRightSectionModel.TypeMasterID = objRightSectionData.TypeMasterID;
                    ViewBag.StatusInd = GetStatusData(objRightSectionModel.StatusInd == true ? "1" : "0");
                    ViewBag.Submit = "Update";
                }
                else
                {
                    ViewBag.Submit = "Save";
                    objRightSectionModel.IsParentTitleInd = true;
                    objRightSectionModel.IsExternalLinkInd = false;
                    objRightSectionModel.RightSectionCreateDate = DateTime.Now;
                    objRightSectionModel.ListingID = ContentTypeID;
                    objRightSectionModel.TypeMasterID = TypeMasterID;
                    objRightSectionModel.IsGooglePlusSharingInd = false;
                    objRightSectionModel.IsFacebookSharingInd = false;
                    objRightSectionModel.IsTwitterSharingInd = false;
                    ViewBag.StatusInd = GetStatusData(string.Empty);
                }


                return View(objRightSectionModel);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        /// <summary>
        /// This method will be called when admin add  or edits the Menu page.
        /// </summary>
        /// <param name="RightSectiontype"></param>
        /// <returns></returns>
        public ActionResult Create(RightSectionModel model, string command, FormCollection fm)
        {
            var rsid = EncryptDecrypt.Encrypt(model.RightSectionID.ToString());
            var decodedRSID = EncryptDecrypt.Decrypt(rsid);
            var ct = EncryptDecrypt.Encrypt(model.ListingID.ToString());
            var ContentTypeID = EncryptDecrypt.Decrypt(ct);
            var mtid = EncryptDecrypt.Encrypt(model.TypeMasterID.ToString());
            var TypeMasterID = Convert.ToInt64(EncryptDecrypt.Decrypt(mtid));
            if (TypeMasterID <= 0)
            {
                return RedirectToAction("Index", "Home");
            }
            var rvd = new RouteValueDictionary();
            rvd.Add("page", Request.QueryString["page"] ?? "1");
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "RightSectionCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            rvd.Add("rsid", rsid);
            rvd.Add("ct", ct);
            rvd.Add("mtid", mtid);
            ViewBag.FocusPageUrl = false;// Set focus on Pageurl Field if same url exist
            using (var objContext = new db_KISDEntities())
            {
                if (string.IsNullOrEmpty(command))
                {
                    try
                    {
                        #region System Change Log
                        DataTable dtOld;
                        var oldresult = (from a in objContext.RightSections
                                         where a.RightSectionID == model.RightSectionID
                                         select a).ToList();
                        dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                        #endregion
                        var TypeMasterName = objContext.TypeMasters.Where(x => x.TypeMasterID == TypeMasterID).Select(x => x.TypeMasterName).FirstOrDefault();
                        ViewBag.Submit = model.RightSectionID == 0 ? "Save" : "Update";
                        ViewBag.Title = (model.RightSectionID > 0 ? " Edit " : " Add ") + "Right Section Details";
                        ViewBag.BreadCrumTtile = TypeMasterName+" - Right Section Listing";
                        var objRightSectionData = objContext.RightSections.Where(x => x.RightSectionID == model.RightSectionID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).FirstOrDefault();
                        if (model != null)
                        {
                            var titleexist = 0;
                            titleexist = objContext.RightSections.Where(x => x.TitleTxt.ToLower().Trim() == model.TitleTxt.ToLower().Trim() && x.RightSectionID != model.RightSectionID && x.TypeMasterID == TypeMasterID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            if (titleexist > 0)
                            {
                                ViewBag.InnerImages = new SelectList(Models.Common.GetInnerImages(), "ImageID", "TitleTxt");
                                ModelState.AddModelError("TitleTxt", model.TitleTxt + " already exists.");
                                ViewBag.StatusInd = GetStatusData(fm["StatusInd"].ToString() == "1" ? "1" : "0");
                                return View(model);
                            }
                        }
                        if (model != null && !string.IsNullOrEmpty(model.PageURLTxt) && model.IsExternalLinkInd == false)
                        {                        
                            var count = 0;
                            count = objContext.RightSections.Where(x => x.ExternalLinkURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.RightSectionID != model.RightSectionID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            count += objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            count += objContext.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            count += objContext.Departments.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            count += objContext.ExceptionOpportunities.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            count += objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            count += objContext.NewsEvents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
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
                                ViewBag.StatusInd = GetStatusData(fm["StatusInd"].ToString() == "1" ? "1" : "0");
                                return View(model);
                            }
                        }
                        var IsNew = objRightSectionData == null;
                        if (objRightSectionData == null)
                        {
                            objRightSectionData = new RightSection();
                        }

                        objRightSectionData.IsParentTitleInd = model.IsParentTitleInd;
                        objRightSectionData.TitleTxt = model.TitleTxt;
                        
                        objRightSectionData.RightSectionTitleTxt = model.RightSectionTitleTxt;
                        objRightSectionData.RightSectionAbstractTxt = model.RightSectionAbstractTxt;
                        objRightSectionData.DescriptionTxt = model.DescriptionTxt;
                        objRightSectionData.IsExternalLinkInd = model.IsExternalLinkInd;
                        objRightSectionData.ExternalLinkURLTxt = model.IsExternalLinkInd.Value == true ? model.ExternalLinkURLTxt : model.PageURLTxt;
                        objRightSectionData.PageTitleTxt = !model.IsExternalLinkInd.Value ? model.PageTitleTxt : string.Empty;
                        objRightSectionData.ExternalLinkTargetInd = model.IsExternalLinkInd.Value ? model.ExternalLinkTargetInd : false;
                        objRightSectionData.BannerImageID = model.BannerImageID;
                        objRightSectionData.AltBannerImageTxt = !model.IsExternalLinkInd.Value ? model.AltBannerImageTxt : string.Empty;//Display Order Num
                        objRightSectionData.BannerImageAbstractTxt = !model.IsExternalLinkInd.Value ? model.BannerImageAbstractTxt : string.Empty;
                        objRightSectionData.ParentID = model.ParentID;
                        objRightSectionData.RightSectionCreateDate = Convert.ToDateTime(model.strRightSectionCreateDate);
                        objRightSectionData.PageMetaTitleTxt = !model.IsExternalLinkInd.Value ? model.PageMetaTitleTxt : string.Empty;
                        objRightSectionData.PageMetaDescriptionTxt = !model.IsExternalLinkInd.Value ? model.PageMetaDescriptionTxt : string.Empty;
                        objRightSectionData.CreateDate = IsNew ? DateTime.Now : objRightSectionData.CreateDate;
                        objRightSectionData.CreatedByID = IsNew ? Convert.ToInt64(Membership.GetUser().ProviderUserKey) : objRightSectionData.CreatedByID;
                        objRightSectionData.LastModifyDate = DateTime.Now;
                        objRightSectionData.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        objRightSectionData.IsDeletedInd = IsNew ? false : model.IsDeletedInd;
                        objRightSectionData.StatusInd = fm["StatusInd"] == "1" ? true : false;
                        objRightSectionData.TableNameTxt = "RightSection";
                        objRightSectionData.ListingID = model.ListingID;
                        objRightSectionData.TypeMasterID = model.TypeMasterID;
                        objRightSectionData.IsFacebookSharingInd = model.IsFacebookSharingInd;
                        objRightSectionData.IsGooglePlusSharingInd = model.IsGooglePlusSharingInd;
                        objRightSectionData.IsTwitterSharingInd = model.IsTwitterSharingInd;
                        if (IsNew)
                        {
                            objContext.RightSections.Add(objRightSectionData);
                        }
                        TempData["AlertMessage"] = " Right Section details" + (IsNew ? " saved" : " updated") + " successfully.";
                        objContext.SaveChanges();
                        var newRightSectionID = objRightSectionData.RightSectionID;
                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = "Right Section";
                        objSCL.LogTypeTxt = objRightSectionData.RightSectionID > 0 ? "Update" : "Add";
                        objSCL.NotesTxt = "Right Section Details" + (objRightSectionData.RightSectionID > 0 ? " updated for " : "  added for ") + objRightSectionData.TitleTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();

                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.RightSections
                                         where x.RightSectionID == newRightSectionID
                                         select x);
                        DataTable dtNew = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(newResult);
                        foreach (DataColumn col in dtNew.Columns)
                        {
                            if (model.RightSectionID > 0)
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
                    return RedirectToAction("RightSectionListing", rvd);
                }
                else
                {
                    return RedirectToAction("RightSectionListing", rvd);
                }
            }
        }


        /// <summary>
        /// Delete the Menu 
        /// </summary>
        /// <param name="rsid">RightSectionID  </param>
        /// <param name="ct">Type of Content</param>
        /// <param name="mtid">Type of Master</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(string rsid, string ct, string mtid)
        {
            rsid = !string.IsNullOrEmpty(Convert.ToString(rsid)) ? EncryptDecrypt.Decrypt(rsid) : "0";
            long RightSectionID = Convert.ToInt32(rsid);
            ct = !string.IsNullOrEmpty(Convert.ToString(ct)) ? EncryptDecrypt.Decrypt(ct) : "0";
            long ListingID = Convert.ToInt32(ct);
            mtid = !string.IsNullOrEmpty(Convert.ToString(mtid)) ? EncryptDecrypt.Decrypt(mtid) : "0";
            long TypeMasterID = Convert.ToInt32(mtid);
            var rvd = new RouteValueDictionary();
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "RightSectionCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            rvd.Add("rsid", EncryptDecrypt.Decrypt(rsid));
            rvd.Add("ct", Request.QueryString["ct"]);
            rvd.Add("mtid", Request.QueryString["mtid"]);
            Session["Edit/Delete"] = "Delete";
            if (RightSectionID > 0)
            {
                try
                {
                    using (var objContext = new db_KISDEntities())
                    {

                        #region System Change Log
                        var oldresult = (from a in objContext.RightSections
                                         where a.RightSectionID == RightSectionID
                                         select a).ToList();
                        DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                        #endregion
                        //******************Check Sub Menu Available *********************************
                        var RightSectionlist_count = objContext.RightSections.Where(x => x.ParentID == RightSectionID && x.TypeMasterID==TypeMasterID && x.ListingID==ListingID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).ToList();
                        if (RightSectionlist_count.Any())
                        {
                            try
                            {
                                TempData["AlertMessage"] = "Right Section can not be deleted as it contains Sub Right Section details.";
                                return Json(Url.Action("RightSectionListing", "RightSection", rvd));
                            }
                            catch
                            {

                            }
                        }
                        /*****************************************************************************/
                        var obj = objContext.RightSections.Where(x => x.RightSectionID == RightSectionID && x.TypeMasterID == TypeMasterID && x.ListingID == ListingID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).ToList();
                        foreach (var value in obj)
                        {
                            value.IsDeletedInd = true;
                            objContext.SaveChanges();
                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Right Section";
                            objSCL.LogTypeTxt = "Delete";
                            objSCL.NotesTxt = "Right Section Details deleted for " + value.TitleTxt;
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();
                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in objContext.RightSections
                                             where x.RightSectionID == RightSectionID
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
                        }
                        TempData["AlertMessage"] = " Right Section details deleted successfully.";
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
                count = objContext.RightSections.Where(x => x.TypeMasterID == TypeMasterID && x.ListingID == ListingID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
            }
            var page = Request.QueryString["page"] ?? "1";
            var pagesize = Request.QueryString["pagesize"] ?? "10";
            if (Convert.ToInt32(page) > 1)
                Page = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
            rvd.Add("page", Page);
            return Json(Url.Action("RightSectionListing", "RightSection", rvd));
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
            count += objContext.RightSections.Where(x => x.ExternalLinkURLTxt.Contains(url)).Count();
            if (count > 0)
            {
                if (url != "")
                    url = url + count;
            }
            return Json(url);
        }
        #endregion

        #region Sub Right Section
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
        public ActionResult SubRightSectionListing(int? page, int? pagesize, GridSortOptions gridSortOptions, string pid, string ct, string mtid, FormCollection fm, string objresult)
        {
            if (string.IsNullOrEmpty(gridSortOptions.Column))
            {
                gridSortOptions.Direction = MvcContrib.Sorting.SortDirection.Descending;
            }
            var objRightSectionService = new RightSectionService();
            //decrypt parent id(mtid)
            pid = !string.IsNullOrEmpty(Convert.ToString(pid)) ? EncryptDecrypt.Decrypt(pid) : "0";
            long ParentId = Convert.ToInt64(pid);
            //decrypt content type id(ct)
            ct = !string.IsNullOrEmpty(Convert.ToString(ct)) ? EncryptDecrypt.Decrypt(ct) : "0";
            int ContentTypeID = !string.IsNullOrEmpty(ct) ? Convert.ToInt32(ct) : 0;
            mtid = !string.IsNullOrEmpty(Convert.ToString(mtid)) ? EncryptDecrypt.Decrypt(mtid) : "0";
            Int64 TypeMasterID = !string.IsNullOrEmpty(mtid) ? Convert.ToInt32(mtid) : 0;
            using (var objContext = new db_KISDEntities())
            {

                #region Check Tab is Accessible or Not
                var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
                var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
                var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Images), Convert.ToInt32(userId));
                if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                    || RoleID == Convert.ToInt32(UserType.Admin) || RoleID == Convert.ToInt32(UserType.DepartmentUser)))//if tab not accessible then redirect to home
                {
                    return RedirectToAction("Index", "Home");
                }
                #endregion
                var TypeMasterName = objContext.TypeMasters.Where(x => x.TypeMasterID == TypeMasterID).Select(x => x.TypeMasterName).FirstOrDefault();
                var SubRightSectionLevel = objContext.RightSections.Where(x => x.RightSectionID == ParentId).FirstOrDefault().ParentID;
                SubRightSectionLevel = SubRightSectionLevel != null && SubRightSectionLevel > 0 ? 2 : 1;
                ViewBag.InnerImages = new SelectList(Models.Common.GetInnerImages(), "ImageID", "TitleTxt");
                var parentRightSectionTitle = objContext.RightSections.Find(ParentId).TitleTxt;
                ViewBag.PageTitle = TypeMasterName + " - Right Section Listing";
                ViewBag.Title = parentRightSectionTitle + " - Sub Right Section Listing";
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

                    //Ajax Call for update status for RightSection
                    if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && objresult != null && !string.IsNullOrEmpty(objresult))
                    {
                        var RightSectionID = Convert.ToInt64(objAjaxRequest.hfid);
                        var objRightSection = objContext.RightSections.Find(RightSectionID);
                        #region System Change Log
                        var oldresult = (from a in objContext.RightSections
                                         where a.RightSectionID == RightSectionID
                                         select a).ToList();
                        DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                        #endregion
                        if (objRightSection != null)
                        {
                            objRightSection.StatusInd = objAjaxRequest.hfvalue == "1" ? true : false;
                            objContext.SaveChanges();
                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Right Section";
                            objSCL.LogTypeTxt = objRightSection.RightSectionID > 0 ? "Update" : "Add";
                            objSCL.NotesTxt = "Right Section Details" + (objRightSection.RightSectionID > 0 ? " updated for " : "  added for ") + objRightSection.TitleTxt;
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();

                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in objContext.RightSections
                                             where x.RightSectionID == RightSectionID
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
                        objAjaxRequest.hfid = null;//remove parameter value
                        objAjaxRequest.hfvalue = null;//remove parameter value
                        pagesize = (Request.QueryString["pagesize"] != null ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : pagesize);
                        page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
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
                    gridSortOptions.Column = "RightSectionCreateDate";
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
                    if (gridSortOptions.Column == "TitleTxt" || gridSortOptions.Column == "PageTitleTxt" ||
                        gridSortOptions.Column == "RightSectionCreateDate")
                    {
                    }
                    else
                    {
                        gridSortOptions.Column = "RightSectionCreateDate";
                    }
                }
                var pagedViewModel = new PagedViewModel<RightSectionModel>
                {
                    ViewData = ViewData,
                    Query = objRightSectionService.GetSubRightSection(ParentId, ContentTypeID, TypeMasterID).AsQueryable(),
                    GridSortOptions = gridSortOptions,
                    DefaultSortColumn = "RightSectionCreateDate",
                    Page = Page,
                    PageSize = pageSize,
                }
               .Setup();
                if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
                {
                    return View("SubRightSectionPartial", pagedViewModel);// ("partial view name ")
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
        public ActionResult CreateSubRightSection(string rsid, string pid, string ct, string mtid)
        {
            var objRightSectionModel = new RightSectionModel();
            ViewBag.FocusPageUrl = false;// Set focus on Pageurl Field if same url exist
            //decrypt sub menu type id(mtid)
            rsid = !string.IsNullOrEmpty(Convert.ToString(rsid)) ? EncryptDecrypt.Decrypt(rsid) : "0";
            //decrypt parent id(mtid)
            pid = !string.IsNullOrEmpty(Convert.ToString(pid)) ? EncryptDecrypt.Decrypt(pid) : "0";

            long RightSectionID = Convert.ToInt64(rsid);
            long ParentId = Convert.ToInt64(pid);
            ct = !string.IsNullOrEmpty(Convert.ToString(ct)) ? EncryptDecrypt.Decrypt(ct) : "0";
            Int64 ContentTypeID = !string.IsNullOrEmpty(ct) ? Convert.ToInt64(ct) : 0;
            mtid = !string.IsNullOrEmpty(Convert.ToString(mtid)) ? EncryptDecrypt.Decrypt(mtid) : "0";
            Int64 TypeMasterID = !string.IsNullOrEmpty(mtid) ? Convert.ToInt64(mtid) : 0;
            using (var objContext = new db_KISDEntities())
            {
                #region Check Tab is Accessible or Not
                var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
                var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
                var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Images), Convert.ToInt32(userId));
                if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                    || RoleID == Convert.ToInt32(UserType.Admin) || RoleID == Convert.ToInt32(UserType.DepartmentUser)))//if tab not accessible then redirect to home
                {
                    return RedirectToAction("Index", "Home");
                }
                #endregion
                var TypeMasterName = objContext.TypeMasters.Where(x => x.TypeMasterID == TypeMasterID).Select(x => x.TypeMasterName).FirstOrDefault();
                var objRightSectionData = objContext.RightSections.Where(x => x.RightSectionID == RightSectionID).FirstOrDefault();
                ViewBag.BreadCrumTtile = TypeMasterName+" - Right Section Listing";
                ViewBag.InnerImages = new SelectList(Models.Common.GetInnerImages(), "ImageID", "TitleTxt");
                objRightSectionModel.strRightSectionCreateDate = System.DateTime.Today.ToShortDateString();
                ViewBag.Title = (objRightSectionData != null ? " Edit " : " Add ") + " Sub Right Section Details";
                var objParentRightSectionData = objContext.RightSections.Where(x => x.RightSectionID == ParentId && x.IsDeletedInd == false).FirstOrDefault();
                ViewBag.ParentRightSectionTitle = objParentRightSectionData.TitleTxt;
                objRightSectionModel.strRightSectionCreateDate = System.DateTime.Today.ToString("MM/dd/yyyy");
                objRightSectionModel.ParentID = ParentId;
                if (objRightSectionData != null)
                {

                    if (objRightSectionData.IsExternalLinkInd == true)
                    {
                        objRightSectionModel.ExternalLinkURLTxt = objRightSectionData.ExternalLinkURLTxt;
                        objRightSectionModel.ExternalLinkTargetInd = objRightSectionData.ExternalLinkTargetInd;
                    }
                    else if (objRightSectionData.IsExternalLinkInd == false)
                    {
                        objRightSectionModel.PageURLTxt = objRightSectionData.ExternalLinkURLTxt;
                        objRightSectionModel.BannerImageID = objRightSectionData.BannerImageID;
                        objRightSectionModel.AltBannerImageTxt = objRightSectionData.AltBannerImageTxt;//Display Order Num
                        objRightSectionModel.BannerImageAbstractTxt = objRightSectionData.BannerImageAbstractTxt;

                        objRightSectionModel.RightSectionTitleTxt = objRightSectionData.RightSectionTitleTxt;
                        objRightSectionModel.RightSectionAbstractTxt = objRightSectionData.RightSectionAbstractTxt;
                        objRightSectionModel.DescriptionTxt = objRightSectionData.DescriptionTxt;
                        objRightSectionModel.PageTitleTxt = objRightSectionData.PageTitleTxt;
                        objRightSectionModel.PageMetaTitleTxt = objRightSectionData.PageMetaTitleTxt;
                        objRightSectionModel.PageMetaDescriptionTxt = objRightSectionData.PageMetaDescriptionTxt;
                        objRightSectionModel.IsGooglePlusSharingInd = objRightSectionData.IsGooglePlusSharingInd.HasValue ? objRightSectionData.IsGooglePlusSharingInd.Value : false;
                        objRightSectionModel.IsFacebookSharingInd = objRightSectionData.IsFacebookSharingInd.HasValue ? objRightSectionData.IsFacebookSharingInd.Value : false;
                        objRightSectionModel.IsTwitterSharingInd = objRightSectionData.IsTwitterSharingInd.HasValue ? objRightSectionData.IsTwitterSharingInd.Value : false;
                    }

                    objRightSectionModel.RightSectionID = objRightSectionData.RightSectionID;
                    objRightSectionModel.TitleTxt = objRightSectionData.TitleTxt;
                    objRightSectionModel.IsExternalLinkInd = objRightSectionData.IsExternalLinkInd;                   
                    objRightSectionModel.StatusInd = objRightSectionData.StatusInd;

                    objRightSectionModel.ParentID = objRightSectionData.ParentID;
                    objRightSectionModel.RightSectionCreateDate = objRightSectionData.RightSectionCreateDate;
                    objRightSectionModel.strRightSectionCreateDate = objRightSectionData.RightSectionCreateDate.HasValue ? objRightSectionData.RightSectionCreateDate.Value.ToShortDateString() : "";
                    objRightSectionModel.CreatedByID = objRightSectionData.CreatedByID;
                    objRightSectionModel.LastModifyDate = objRightSectionData.LastModifyDate;
                    objRightSectionModel.LastModifyByID = objRightSectionData.LastModifyByID;
                    objRightSectionModel.IsDeletedInd = objRightSectionData.IsDeletedInd;
                    ViewBag.StatusInd = GetStatusData(objRightSectionModel.StatusInd == true ? "1" : "0");
                    objRightSectionModel.TableNameTxt = objRightSectionData.TableNameTxt;
                    objRightSectionModel.ListingID = objRightSectionData.ListingID;
                    objRightSectionModel.TypeMasterID = objRightSectionData.TypeMasterID;                  
                    ViewBag.Submit = "Update";
                }
                else
                {
                    ViewBag.Submit = "Save";
                    objRightSectionModel.ParentID = ParentId;
                    objRightSectionModel.IsExternalLinkInd = false;
                    objRightSectionModel.CreateDate = DateTime.Now;
                    objRightSectionModel.ListingID = ContentTypeID;
                    objRightSectionModel.TypeMasterID = TypeMasterID;
                    objRightSectionModel.IsGooglePlusSharingInd = false;
                    objRightSectionModel.IsFacebookSharingInd = false;
                    objRightSectionModel.IsTwitterSharingInd = false;
                    ViewBag.StatusInd = GetStatusData(string.Empty);
                }

                return View(objRightSectionModel);
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
        public ActionResult CreateSubRightSection(RightSectionModel model, string command, FormCollection fm)
        {
            var ct = EncryptDecrypt.Encrypt(model.ListingID.ToString());
            var ContentTypeID = EncryptDecrypt.Decrypt(ct);
            var mtid = EncryptDecrypt.Encrypt(model.TypeMasterID.ToString());
            var TypeMasterID = Convert.ToInt64(EncryptDecrypt.Decrypt(mtid));
            var rvd = new RouteValueDictionary();
            rvd.Add("page", Request.QueryString["page"] ?? "1");
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "RightSectionCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            rvd.Add("pid", EncryptDecrypt.Encrypt(model.ParentID.ToString()));
            rvd.Add("rsid", EncryptDecrypt.Encrypt(model.RightSectionID.ToString()));
            rvd.Add("ct", ct);
            rvd.Add("mtid", mtid);
            rvd.Add("mpage", Request.QueryString["mpage"] ?? "1");
            rvd.Add("mpagesize", Request.QueryString["mpagesize"] ?? "10");
            rvd.Add("mColumn", Request.QueryString["mColumn"] ?? "RightSectionCreateDate");
            rvd.Add("mDirection", Request.QueryString["mDirection"] ?? "Descending");
            ViewBag.FocusPageUrl = false;// Set focus on Pageurl Field if same url exist
            using (var objContext = new db_KISDEntities())
            {
                if (string.IsNullOrEmpty(command))
                {
                    try
                    {
                        var TypeMasterName = objContext.TypeMasters.Where(x => x.TypeMasterID == TypeMasterID).Select(x => x.TypeMasterName).FirstOrDefault();
                        var objRightSectionData = objContext.RightSections.Where(x => x.RightSectionID == model.RightSectionID).FirstOrDefault();
                        ViewBag.BreadCrumTtile = TypeMasterName+" - Right Section Listing";
                        ViewBag.InnerImages = new SelectList(Models.Common.GetInnerImages(), "ImageID", "TitleTxt");
                        ViewBag.Title = (objRightSectionData != null ? " Edit " : " Add ") + " Sub Right Section Details";
                        var objParentRightSectionData = objContext.RightSections.Where(x => x.RightSectionID == model.ParentID && x.IsDeletedInd == false).FirstOrDefault();
                        ViewBag.ParentRightSectionTitle = objParentRightSectionData.TitleTxt;
                        ViewBag.Submit = model.RightSectionID == 0 ? "Save" : "Update";
                        if (model != null)
                        {
                            var titleexist = 0;
                            titleexist = objContext.RightSections.Where(x => x.TitleTxt.ToLower().Trim() == model.TitleTxt.ToLower().Trim() && x.RightSectionID != model.RightSectionID && x.TypeMasterID == TypeMasterID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            if (titleexist > 0)
                            {
                                ModelState.AddModelError("TitleTxt", model.TitleTxt + " already exists.");
                                ViewBag.StatusInd = GetStatusData(fm["StatusInd"].ToString() == "1" ? "1" : "0");
                                return View(model);
                            }
                        }
                        if (model != null && !string.IsNullOrEmpty(model.PageURLTxt) && model.IsExternalLinkInd == false)
                        {
                            var count = 0;
                            count = objContext.RightSections.Where(x => x.ExternalLinkURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.RightSectionID != model.RightSectionID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            count += objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            count += objContext.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            count += objContext.Departments.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            count += objContext.ExceptionOpportunities.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            count += objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            count += objContext.NewsEvents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();

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
                                ViewBag.StatusInd = GetStatusData(fm["StatusInd"].ToString() == "1" ? "1" : "0");
                                return View(model);
                            }
                        }
                        var IsNew = objRightSectionData == null;
                        if (objRightSectionData == null)
                        {
                            objRightSectionData = new RightSection();
                        }
                        objRightSectionData.IsParentTitleInd = model.IsParentTitleInd;
                        objRightSectionData.TitleTxt = model.TitleTxt;
                        objRightSectionData.PageTitleTxt = model.PageTitleTxt;
                        objRightSectionData.RightSectionTitleTxt = model.RightSectionTitleTxt;
                        objRightSectionData.RightSectionAbstractTxt = model.RightSectionAbstractTxt;
                        objRightSectionData.DescriptionTxt = model.DescriptionTxt;
                        objRightSectionData.IsExternalLinkInd = model.IsExternalLinkInd;
                        objRightSectionData.ExternalLinkURLTxt = model.IsExternalLinkInd.Value == true ? model.ExternalLinkURLTxt : model.PageURLTxt;
                        objRightSectionData.ExternalLinkTargetInd = model.ExternalLinkTargetInd;
                        objRightSectionData.BannerImageID = model.BannerImageID;
                        objRightSectionData.AltBannerImageTxt = model.AltBannerImageTxt;//Display Order Num
                        objRightSectionData.BannerImageAbstractTxt = model.BannerImageAbstractTxt;
                        objRightSectionData.ParentID = model.ParentID;
                        objRightSectionData.RightSectionCreateDate = Convert.ToDateTime(model.strRightSectionCreateDate);
                        objRightSectionData.PageMetaTitleTxt = model.PageMetaTitleTxt;
                        objRightSectionData.PageMetaDescriptionTxt = model.PageMetaDescriptionTxt;
                        objRightSectionData.CreateDate = IsNew ? DateTime.Now : objRightSectionData.CreateDate;
                        objRightSectionData.CreatedByID = IsNew ? Convert.ToInt64(Membership.GetUser().ProviderUserKey) : objRightSectionData.CreatedByID;
                        objRightSectionData.LastModifyDate = DateTime.Now;
                        objRightSectionData.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        objRightSectionData.IsDeletedInd = IsNew ? false : model.IsDeletedInd;
                        objRightSectionData.StatusInd = fm["StatusInd"] == "1" ? true : false;
                        objRightSectionData.TableNameTxt = "RightSection";
                        objRightSectionData.ListingID = model.ListingID;
                        objRightSectionData.TypeMasterID = model.TypeMasterID;
                        objRightSectionData.IsFacebookSharingInd = model.IsFacebookSharingInd;
                        objRightSectionData.IsGooglePlusSharingInd = model.IsGooglePlusSharingInd;
                        objRightSectionData.IsTwitterSharingInd = model.IsTwitterSharingInd;
                        if (IsNew)
                        {
                            objContext.RightSections.Add(objRightSectionData);
                        }
                        TempData["AlertMessage"] = " Right Section details" + (IsNew ? " saved" : " updated") + " successfully.";
                        objContext.SaveChanges();
                        TempData["AlertMessage"] = objParentRightSectionData.TitleTxt + " sub right section details" + (IsNew ? " saved successfully." : " updated successfully.");
                        objContext.SaveChanges();
                    }
                    catch (Exception ce)
                    {
                        TempData["AlertMessage"] = "Some error occured, Please try after some time.";
                    }
                    return RedirectToAction("SubRightSectionListing", rvd);
                }
                else
                {
                    return RedirectToAction("SubRightSectionListing", rvd);
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
        public JsonResult DeleteSubRightSection(string rsid, string pid, string ct, string mtid)
        {
            {
                //decrypt sub menu type id(mtid)
                rsid = !string.IsNullOrEmpty(Convert.ToString(rsid)) ? EncryptDecrypt.Decrypt(rsid) : "0";
                long RightSectionID = Convert.ToInt32(rsid);
                pid = !string.IsNullOrEmpty(Convert.ToString(pid)) ? EncryptDecrypt.Decrypt(pid) : "0";
                long ParentID = Convert.ToInt32(pid);
                ct = !string.IsNullOrEmpty(Convert.ToString(ct)) ? EncryptDecrypt.Decrypt(ct) : "0";
                long ListingID = Convert.ToInt32(ct);
                mtid = !string.IsNullOrEmpty(Convert.ToString(mtid)) ? EncryptDecrypt.Decrypt(mtid) : "0";
                long TypeMasterID = Convert.ToInt32(mtid);
                var rvd = new RouteValueDictionary();
                rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
                rvd.Add("Column", Request.QueryString["Column"] ?? "RightSectionCreateDate");
                rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
                rvd.Add("rsid", EncryptDecrypt.Encrypt(rsid.ToString()));
                rvd.Add("mpagesize", Request.QueryString["mpagesize"] ?? "10");
                rvd.Add("mColumn", Request.QueryString["mColumn"] ?? "RightSectionCreateDate");
                rvd.Add("mDirection", Request.QueryString["mDirection"] ?? "Descending");
                rvd.Add("pid", Request.QueryString["pid"] ?? EncryptDecrypt.Encrypt("0"));
                rvd.Add("ct", Request.QueryString["ct"]);
                rvd.Add("mtid", Request.QueryString["mtid"]);
                Session["Edit/Delete"] = "Delete";
                if (RightSectionID > 0)
                {
                    try
                    {
                        using (var _objdbContext = new db_KISDEntities())
                        {
                            var obj = _objdbContext.RightSections.Where(x => x.RightSectionID == RightSectionID && x.TypeMasterID == TypeMasterID && x.ListingID == ListingID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).FirstOrDefault();
                            #region System Change Log
                            var oldresult = (from a in _objdbContext.RightSections
                                             where a.RightSectionID == RightSectionID
                                             select a).ToList();
                            DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                            #endregion

                            obj.IsDeletedInd = true;
                            _objdbContext.SaveChanges();
                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = _objdbContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)_objdbContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Right Section";
                            objSCL.LogTypeTxt = "Delete";
                            objSCL.NotesTxt = "Right Section Details deleted for " + obj.TitleTxt;
                            objSCL.LogDateTime = DateTime.Now;
                            _objdbContext.SystemChangeLogs.Add(objSCL);
                            _objdbContext.SaveChanges();
                            objSCL = _objdbContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in _objdbContext.RightSections
                                             where x.RightSectionID == RightSectionID
                                             select x);
                            DataTable dtNew = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(newResult);
                            foreach (DataColumn col in dtNew.Columns)
                            {
                                SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                                objSCLD.ChangeLogID = objSCL.ChangeLogID;
                                objSCLD.FieldNameTxt = col.ColumnName.ToString();
                                objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                                objSCLD.NewValueTxt = "";// dtNew.Rows[0][col.ColumnName].ToString();
                                _objdbContext.SystemChangeLogDetails.Add(objSCLD);
                                _objdbContext.SaveChanges();
                            }
                            #endregion
                            TempData["AlertMessage"] = obj.TitleTxt + " sub right section details deleted successfully.";
                        }
                    }
                    catch
                    {
                        TempData["AlertMessage"] = "Some error occured while deleting the Sub Right Section, Please try again later.";
                    }
                }
                int? Page = 1;
                var count = 1;
                using (var _objdbContext = new db_KISDEntities())
                {
                    count = _objdbContext.RightSections.Where(x => x.ParentID == ParentID && x.TypeMasterID == TypeMasterID && x.ListingID == ListingID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                }
                var page = Request.QueryString["page"] ?? "1";
                var pagesize = Request.QueryString["pagesize"] ?? "10";
                if (Convert.ToInt32(page) > 1)
                    Page = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
                rvd.Add("page", Page);
                return Json(Url.Action("SubRightSectionListing", "RightSection", rvd));
            }
        }
        #endregion
    }
}