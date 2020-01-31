using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using KISD.Areas.Admin.Models;
using AnnouncementTypeAlias = KISD.Areas.Admin.Models.GalleryListingService.TypeMaster;
using System.Data;
using System.Web.Security;
using ContentTypeAlias = KISD.Areas.Admin.Models.ContentType;
using ModuleTypeAlias = KISD.Areas.Admin.Models.Common.ModuleType;
using static KISD.Areas.Admin.Models.Common;

namespace KISD.Areas.Admin.Controllers
{
    public class DepartmentController : Controller
    {
        #region Department
        [Authorize]
        /// <summary>
        /// This action method will be called when the user visits the Department listing page.        
        /// Set the page variables value according to the Department type id
        /// Code added to update the Status of the Department if formcollection parameters contains values for hdncheckboxselected and 
        /// hdnvalue params.
        /// </summary>
        /// <param name="page">This parameters is used for page number.It shows the records in the list of that page. </param>
        /// <param name="pagesize">This parameter is used for showing the number of records per page</param>
        /// <param name="gridSortOptions">This parameter is used for sorting the list of records in ascending/descending order</param>
        /// <param name="fm">This parameter is used to get the Form collection of the control from view</param>
        public ActionResult DepartmentListing(int? page, int? pagesize, GridSortOptions gridSortOptions, FormCollection fm, string objresult)
        {
            if (string.IsNullOrEmpty(gridSortOptions.Column))
            {
                gridSortOptions.Direction = MvcContrib.Sorting.SortDirection.Descending;
            }
            var objDepartmentservice = new DepartmentService();
            using (var objContext = new db_KISDEntities())
            {
                #region Check Tab is Accessible or Not
                var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
                var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
                var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Departments), Convert.ToInt32(userId));
                if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                    || RoleID == Convert.ToInt16(UserType.Admin) || RoleID == Convert.ToInt16(UserType.DepartmentUser)) || RoleID == Convert.ToInt16(UserType.User))//if tab not accessible then redirect to home
                {
                    return RedirectToAction("Index", "Home");
                }
                #endregion

                ViewBag.PageTitle = "Department Listing";
                ViewBag.Title = "Department Listing";
                //*******************Fill Values if Display order contains null values***************************
                var displayOrderList = objContext.Departments.Where(x => x.ParentID == null && x.IsDeletedInd == false).ToList();
                foreach (var item in displayOrderList)
                {
                    if (string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()))
                    {
                        var ObjDepartmentData = objContext.Departments.Where(x => x.DepartmentID == item.DepartmentID && x.IsDeletedInd == false).FirstOrDefault();
                        var displayOrder1 = (displayOrderList.Max(x => x.DisplayOrderNbr)) == null ? 1 : displayOrderList.Max(x => x.DisplayOrderNbr).Value + 1;
                        ObjDepartmentData.DisplayOrderNbr = displayOrder1;
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
                        var departmentID = Convert.ToInt64(objAjaxRequest.hfid);
                        var objDepartmentDepartment = objContext.Departments.Find(departmentID);
                        if (objDepartmentDepartment != null)
                        {
                            objDepartmentDepartment.StatusInd = objAjaxRequest.hfvalue == "1" ? true : false;
                            objContext.SaveChanges();
                            TempData["AlertMessage"] = "Status updated successfully.";
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
                        var departmentID = Convert.ToInt64(objAjaxRequest.qs_checkboxselected);
                        var objDepartmentContent = objContext.Departments.Find(departmentID);
                        if (objDepartmentContent != null)
                        {
                            try
                            {
                                //var displayOrder = string.IsNullOrEmpty(objDepartmentContent.DisplayOrderNbr.ToString()) ? objHAI.Departments.Where(x => x.ContentTypeID == DepartmenttypeId).Max(x => x.DisplayOrderNbr).Value : objDepartmentContent.DisplayOrderNbr.Value ; 
                                if (objDepartmentservice.ChangeImageDisplayOrder(objDepartmentContent.DisplayOrderNbr.Value, Convert.ToInt32(objAjaxRequest.qs_value), Convert.ToInt64(objDepartmentContent.DepartmentID)))
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
                    gridSortOptions.Column = "DepartmentCreateDate";
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
                        || gridSortOptions.Column == "NameTxt" || gridSortOptions.Column == "NameTxt"
                        || gridSortOptions.Column == "DepartmentCreateDate" || gridSortOptions.Column == "DisplayOrderNbr")
                    {

                    }
                    else
                    {
                        gridSortOptions.Column = "DepartmentCreateDate";
                    }
                }

                var pagedViewModel = new PagedViewModel<DepartmentModel>
                {
                    ViewData = ViewData,
                    Query = objDepartmentservice.GetDepartments().AsQueryable(),
                    GridSortOptions = gridSortOptions,
                    DefaultSortColumn = "DepartmentCreateDate",
                    Page = Page,
                    PageSize = pageSize,
                };
               
                if (RoleID == Convert.ToInt16(UserType.DepartmentUser))
                {
                    var objDepartUser = objContext.UserDepartments.Where(x => x.UserID == userId).Select(x => x.DepartmentID).ToList();
                    pagedViewModel.AddFilter("department", objDepartUser, a => objDepartUser.Contains(a.DepartmentID));
                }
                pagedViewModel.Setup();
                if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
                {
                    return View("DepartmentPartial", pagedViewModel);// ("partial view name ")
                }
                else
                {
                    return View(pagedViewModel);
                }
            }
        }

        [Authorize]
        /// <summary>
        /// This method will be called when admin add  or edits the Department page.
        /// </summary>
        /// <param name="contenttype"></param>
        /// <returns></returns>
        public ActionResult Create(string did)
        {
            //decrypt Department type id(did)
            did = !string.IsNullOrEmpty(Convert.ToString(did)) ? EncryptDecrypt.Decrypt(did) : "0";

            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Departments), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin) || RoleID == Convert.ToInt16(UserType.DepartmentUser)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            var objDepartmentModel = new DepartmentModel();
            Session["Edit/Delete"] = "Edit";
            ViewBag.FocusPageUrl = false;// Set focus on Pageurl Field if same url exist
            int DepartmentID = Convert.ToInt32(did);
            using (var obj_Context = new db_KISDEntities())
            {
                var ObjDepartmentData = obj_Context.Departments.Where(x => x.DepartmentID == DepartmentID).FirstOrDefault();
                ViewBag.BreadCrumTtile = "Department Listing";

                objDepartmentModel.strCreateDate = DateTime.Today.ToShortDateString();
                Int64 TypeMasterID = Convert.ToInt64(AnnouncementTypeAlias.Department);
                if (ObjDepartmentData != null)
                {
                    objDepartmentModel.DepartmentID = ObjDepartmentData.DepartmentID;
                    objDepartmentModel.NameTxt = ObjDepartmentData.NameTxt;
                    objDepartmentModel.NameTxt = ObjDepartmentData.NameTxt;
                    objDepartmentModel.URLTxt = ObjDepartmentData.URLTxt;
                    objDepartmentModel.DescriptionTxt = ObjDepartmentData.DescriptionTxt;
                    objDepartmentModel.strCreateDate = ObjDepartmentData.DepartmentCreateDate.Value.ToShortDateString();
                    objDepartmentModel.StatusInd = ObjDepartmentData.StatusInd.Value;
                    objDepartmentModel.DisplayOrderNbr = ObjDepartmentData.DisplayOrderNbr.Value;//Display Order Num
                    objDepartmentModel.PageMetaTitleTxt = ObjDepartmentData.PageMetaTitleTxt;
                    objDepartmentModel.PageMetaDescription = ObjDepartmentData.PageMetaDescription;
                    objDepartmentModel.RightSectionTitleTxt = ObjDepartmentData.RightSectionTitleTxt;
                    objDepartmentModel.AddressTxt = ObjDepartmentData.AddressTxt;
                    objDepartmentModel.BannerImageID = ObjDepartmentData.BannerImageID;
                    objDepartmentModel.AltBannerImageTxt = ObjDepartmentData.AltBannerImageTxt;
                    objDepartmentModel.BannerImageAbstractTxt = ObjDepartmentData.BannerImageAbstractTxt;
                    objDepartmentModel.PhoneNumberTxt = ObjDepartmentData.PhoneNumberTxt;
                    objDepartmentModel.FaxNumberTxt = ObjDepartmentData.FaxNumberTxt;
                    objDepartmentModel.RightSectionAbstractTxt = ObjDepartmentData.RightSectionAbstractTxt;
                    objDepartmentModel.CreateByID = ObjDepartmentData.CreateByID;
                    objDepartmentModel.CreateDate = ObjDepartmentData.CreateDate;
                    objDepartmentModel.LastModifyByID = ObjDepartmentData.LastModifyByID;
                    objDepartmentModel.LastModifyDate = ObjDepartmentData.LastModifyDate;
                    ViewBag.IsActiveInd = GetStatusData(objDepartmentModel.StatusInd == true ? "1" : "0");
                    ViewBag.Submit = "Update";
                }
                else
                {
                    ViewBag.Submit = "Save";
                    objDepartmentModel.DepartmentCreateDate = DateTime.Now;
                    ViewBag.IsActiveInd = GetStatusData(string.Empty);
                }
                ViewBag.Title = (ObjDepartmentData != null ? " Edit " : " Add ") + " Department";
                var InnerImagesTitle = Models.Common.GetInnerImages();
                ViewBag.InnerImagesTitle = InnerImagesTitle;//get all the inner image titles
                return View(objDepartmentModel);
            }
        }

        /// <summary>
        /// Save update the Department pages              
        /// </summary>
        /// <param name="model">Intialized DepartmentModel model object from view</param>        
        /// <param name="command">Defines Submit or Cancel </param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(DepartmentModel model, string command, FormCollection fm)
        {
            var rvd = new RouteValueDictionary();
            rvd.Add("page", Request.QueryString["page"] ?? "1");
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "DepartmentCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            rvd.Add("did", EncryptDecrypt.Encrypt(model.DepartmentID.ToString()));
            rvd.Add("DisplayOrderNbr", model.DisplayOrderNbr);//Display Order Num
            ViewBag.FocusPageUrl = false;// Set focus on Pageurl Field if same url exist
            using (var objContext = new db_KISDEntities())
            {
                if (string.IsNullOrEmpty(command))
                {
                    try
                    {
                        #region System Change Log
                        DataTable dtOld;
                        var oldResult = (from a in objContext.Departments
                                         where a.DepartmentID == model.DepartmentID
                                         select a).ToList();
                        dtOld = Models.Common.LINQResultToDataTable(oldResult);
                        #endregion

                        Int64 TypeMasterID = Convert.ToInt64(AnnouncementTypeAlias.Department);
                        ViewBag.Submit = model.DepartmentID == 0 ? "Save" : "Update";
                        ViewBag.Title = (model.DepartmentID > 0 ? " Edit " : " Add ") + "Department";
                        ViewBag.BreadCrumTtile = "Department Listing";
                        var ObjDepartmentData = objContext.Departments.Where(x => x.DepartmentID == model.DepartmentID).FirstOrDefault();

                        if (model != null && !string.IsNullOrEmpty(model.URLTxt))
                        {
                            var count = 0;
                            var dupname = objContext.Departments.Where(x => x.NameTxt.ToLower().Trim() == model.NameTxt.ToLower().Trim() && x.DepartmentID != model.DepartmentID && x.IsDeletedInd == false).Count();  //check duplicate department
                            count = dupname + objContext.Departments.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.DepartmentID != model.DepartmentID && x.IsDeletedInd == false).Count();

                            count += objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.ExceptionOpportunities.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.NewsEvents.Where(x => x.PageURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.RightSections.Where(x => x.ExternalLinkURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            if (model.URLTxt.Trim().ToLower() == "error404")
                            {
                                count = count + 1;
                            }
                            if (count > 0)
                            {
                                if (dupname > 0)
                                {
                                    ModelState.AddModelError("NameTxt", model.NameTxt + " already exist.");
                                }
                                else
                                {
                                    if (model.URLTxt.ToLower().Trim() == "error404")//if user types url 'error404' below validation msg should display
                                    {
                                        ModelState.AddModelError("URLTxt", model.URLTxt + " URL is not allowed.");
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("URLTxt", model.URLTxt + " URL already exists.");
                                    }
                                    ViewBag.FocusPageUrl = true;// Set focus on Pageurl Field if same url exist
                                }

                                ViewBag.IsActiveInd = GetStatusData(fm["IsActiveInd"].ToString() == "1" ? "1" : "0");
                                return View(model);
                            }
                        }
                        var IsNew = ObjDepartmentData == null;
                        if (ObjDepartmentData == null)
                        {
                            ObjDepartmentData = new Department();
                        }

                        ObjDepartmentData.ParentID = null;

                        ObjDepartmentData.NameTxt = model.NameTxt;
                        ObjDepartmentData.URLTxt = model.URLTxt;
                        ObjDepartmentData.BannerImageID = model.BannerImageID;
                        ObjDepartmentData.BannerImageAbstractTxt = (string.IsNullOrEmpty(model.BannerImageAbstractTxt) ? string.Empty : model.BannerImageAbstractTxt);
                        ObjDepartmentData.AltBannerImageTxt = model.AltBannerImageTxt;
                        ObjDepartmentData.DescriptionTxt = model.DescriptionTxt;
                        ObjDepartmentData.PageMetaTitleTxt = model.PageMetaTitleTxt;
                        ObjDepartmentData.PageMetaDescription = model.PageMetaDescription;
                        ObjDepartmentData.AddressTxt = model.AddressTxt;
                        ObjDepartmentData.PhoneNumberTxt = model.PhoneNumberTxt;
                        ObjDepartmentData.FaxNumberTxt = model.FaxNumberTxt;
                        ObjDepartmentData.RightSectionTitleTxt = model.RightSectionTitleTxt;
                        ObjDepartmentData.RightSectionAbstractTxt = model.RightSectionAbstractTxt;
                        ObjDepartmentData.IsDeletedInd = false;
                        DateTime dt_to = Convert.ToDateTime(model.strCreateDate, System.Globalization.CultureInfo.InvariantCulture);
                        ObjDepartmentData.DepartmentCreateDate = dt_to;
                        ObjDepartmentData.StatusInd = fm["IsActiveInd"] == "1" ? true : false;
                        ObjDepartmentData.CreateDate = model.DepartmentID > 0 ? ObjDepartmentData.CreateDate : DateTime.Now;
                        ObjDepartmentData.CreateByID = model.DepartmentID > 0 ? ObjDepartmentData.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        ObjDepartmentData.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        ObjDepartmentData.LastModifyDate = DateTime.Now;
                        //*************Display Order ************************
                        if (model.DepartmentID == 0)
                        {
                            var DepartmentCount = objContext.Departments.Where(x => x.ParentID == null && x.IsDeletedInd == false).ToList();
                            ObjDepartmentData.DisplayOrderNbr = DepartmentCount.Any() ? objContext.Departments.Where(x => x.ParentID == null && x.IsDeletedInd == false).Max(x => x.DisplayOrderNbr) + 1 : 1;
                            objContext.Departments.Add(ObjDepartmentData);
                        }
                        //**************************************************

                        ObjDepartmentData.NameTxt = model.NameTxt ?? string.Empty;

                        if (IsNew)
                        {
                            objContext.Departments.Add(ObjDepartmentData);
                        }
                        TempData["AlertMessage"] = "Department details" + (IsNew ? " saved" : " updated") + " successfully.";
                        objContext.SaveChanges();

                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = "Department";
                        objSCL.LogTypeTxt = model.DepartmentID > 0 ? "Update" : "Add";
                        objSCL.NotesTxt = " Details" + (model.DepartmentID > 0 ? " updated for " : "  added for ") + model.NameTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();
                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.Departments
                                         where x.DepartmentID == model.DepartmentID
                                         select x);
                        DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                        foreach (DataColumn col in dtNew.Columns)
                        {
                            if (model.DepartmentID > 0)
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
                    }
                    catch (Exception ex)
                    {
                        TempData["AlertMessage"] = "Some error occured, Please try after some time.";
                    }
                    return RedirectToAction("DepartmentListing", rvd);
                }
                else
                {
                    return RedirectToAction("DepartmentListing", rvd);
                }
            }
        }

        /// <summary>
        /// Delete the Department 
        /// </summary>
        /// <param name="departmentID">departmentID  </param>
        /// <param name="DepartmentTypeId">Type of content</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(string did, string mtid)
        {
            //decrypt Department type id(did)
            did = !string.IsNullOrEmpty(Convert.ToString(did)) ? EncryptDecrypt.Decrypt(did) : "0";
            mtid = !string.IsNullOrEmpty(Convert.ToString(mtid)) ? EncryptDecrypt.Decrypt(mtid) : "0";
            int departmentID = Convert.ToInt32(did);
            long TypeMasterID = Convert.ToInt64(mtid);
            var rvd = new RouteValueDictionary();
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "DepartmentCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            rvd.Add("did", EncryptDecrypt.Decrypt(did));
            Session["Edit/Delete"] = "Delete";
            if (departmentID > 0)
            {
                try
                {
                    using (var objContext = new db_KISDEntities())
                    {
                        var InnerImagesTitle = Models.Common.GetInnerImages();
                        ViewBag.InnerImagesTitle = InnerImagesTitle;//get all the inner image titles
                        //******************Check Sub Department Available *********************************
                        var Departmentlist_count = objContext.Departments.Where(x => x.ParentID == departmentID && x.IsDeletedInd == false).ToList();

                        if (Departmentlist_count.Any())
                        {
                            try
                            {
                                TempData["Message"] = "Department can not be deleted as it contains Sub Department details.";
                                return Json(Url.Action("DepartmentListing", "Department", rvd));
                            }
                            catch { }
                        }
                        /*****************************************************************************/

                        var obj = objContext.Departments.Where(x => x.DepartmentID == departmentID || x.ParentID == departmentID).FirstOrDefault();
                        if (obj != null)
                        {
                            #region System Change Log
                            DataTable dtOld;
                            var oldResult = (from a in objContext.Departments
                                             where a.DepartmentID == departmentID
                                             select a).ToList();
                            dtOld = Models.Common.LINQResultToDataTable(oldResult);
                            #endregion

                            //****************Display Order ************************
                            var objDepartmentDepartment = objContext.Departments.Where(x => x.DepartmentID == departmentID).FirstOrDefault();
                            if (objDepartmentDepartment != null)
                            {
                                try
                                {
                                    var objModelService = new DepartmentService();
                                    objModelService.ChangeDeletedDisplayOrder(objDepartmentDepartment.DisplayOrderNbr.Value, departmentID);
                                }
                                catch { }
                            }
                            //***************************************************

                            #region Delete Selected Right Section for the Department
                            var rightSections = objContext.RightSections.Where(x => x.ListingID == departmentID && x.TypeMasterID == TypeMasterID).ToList();
                            if (rightSections != null && rightSections.Count() > 0)
                            {
                                foreach (var section in rightSections)
                                {
                                    section.IsDeletedInd = true;
                                }
                                objContext.SaveChanges();
                            }
                            #endregion
                            TempData["AlertMessage"] = objDepartmentDepartment.NameTxt + " deleted successfully.";

                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Department";
                            objSCL.LogTypeTxt = "Delete";
                            objSCL.NotesTxt = "Department" + obj.NameTxt + " Details deleted.";
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();
                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in objContext.Departments
                                             where x.DepartmentID == departmentID
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
                                    objContext.SystemChangeLogDetails.Add(objSCLD);
                                    objContext.SaveChanges();
                                }
                            }
                            #endregion
                        }

                       
                        
                    }
                }
                catch
                {
                    TempData["AlertMessage"] = "Some error occured while deleting the Department, Please try again later.";
                }
            }
            int? Page = 1;
            var count = 1;
            using (var objContext = new db_KISDEntities())
            {
                count = objContext.Departments.Where(x => x.ParentID == null && x.IsDeletedInd == false).Count();
            }
            var page = Request.QueryString["page"] ?? "1";
            var pagesize = Request.QueryString["pagesize"] ?? "10";
            if (Convert.ToInt32(page) > 1)
                Page = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
            rvd.Add("page", Page);
            return Json(Url.Action("DepartmentListing", "Department", rvd));
        }
        #endregion

        #region Sub Department
        [Authorize]
        /// <summary>
        /// This action method will be called when the user visits the Department listing page.        
        /// Set the page variables value according to the Department type id
        /// Code added to update the Status of the Department if formcollection parameters contains values for hdncheckboxselected and 
        /// hdnvalue params.
        /// </summary>
        /// <param name="page">This parameters is used for page number.It shows the records in the list of that page. </param>
        /// <param name="pagesize">This parameter is used for showing the number of records per page</param>
        /// <param name="gridSortOptions">This parameter is used for sorting the list of records in ascending/descending order</param>
        /// <param name="fm">This parameter is used to get the Form collection of the control from view</param>
        public ActionResult SubDepartmentListing(int? page, int? pagesize, GridSortOptions gridSortOptions, string pid, FormCollection fm, string objresult)
        {
            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Departments), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin) || RoleID == Convert.ToInt32(UserType.DepartmentUser)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            if (string.IsNullOrEmpty(gridSortOptions.Column))
            {
                gridSortOptions.Direction = MvcContrib.Sorting.SortDirection.Descending;
            }
            var objDepartmentservice = new DepartmentService();

            //decrypt parent id(pid)
            pid = !string.IsNullOrEmpty(Convert.ToString(pid)) ? EncryptDecrypt.Decrypt(pid) : "0";

            int ParentId = Convert.ToInt32(pid);

            using (objContext = new db_KISDEntities())
            {
                var parentDepartmentTypeTitle = objContext.Departments.Find(ParentId).NameTxt;
                ViewBag.Title = "Department Listing";
                ViewBag.PageTitle = parentDepartmentTypeTitle;

                //****************************Fill Values if Display order contains null values*******************************************
                var displayOrderList = objContext.Departments.Where(x => x.ParentID == ParentId && x.IsDeletedInd == false).ToList();
                foreach (var item in displayOrderList)
                {
                    if (string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()))
                    {
                        var objDepartmentData = objContext.Departments.Where(x => x.DepartmentID == item.DepartmentID && x.IsDeletedInd == false).FirstOrDefault();
                        var displayOrder1 = (displayOrderList.Max(x => x.DisplayOrderNbr)) == null ? 1 : displayOrderList.Max(x => x.DisplayOrderNbr).Value + 1;
                        objDepartmentData.DisplayOrderNbr = displayOrder1;
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
                        var departmentID = Convert.ToInt64(objAjaxRequest.hfid);
                        var objDepartmentDepartment = objContext.Departments.Find(departmentID);
                        if (objDepartmentDepartment != null)
                        {
                            objDepartmentDepartment.StatusInd = objAjaxRequest.hfvalue == "1" ? true : false;
                            objContext.SaveChanges();
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
                    gridSortOptions.Column = "DepartmentCreateDate";
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
                        gridSortOptions.Column == "NameTxt" || gridSortOptions.Column == "DepartmentCreateDate")
                    {
                    }
                    else
                    {
                        gridSortOptions.Column = "DepartmentCreateDate";
                    }
                }
                var pagedViewModel = new PagedViewModel<DepartmentModel>
                {
                    ViewData = ViewData,
                    Query = objDepartmentservice.GetSubDepartments(ParentId).AsQueryable(),
                    GridSortOptions = gridSortOptions,
                    DefaultSortColumn = "CreateDate",
                    Page = Page,
                    PageSize = pageSize,
                }
               .Setup();
                if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
                {
                    return View("SubDepartmentPartial", pagedViewModel);// ("partial view name ")
                }
                else
                {
                    return View(pagedViewModel);
                }
            }
        }

        [Authorize]
        /// <summary>
        /// This method will be called when admin add  or edits the Department page.
        /// </summary>
        /// <param name="contenttype"></param>
        /// <returns></returns>
        public ActionResult CreateSubDepartment(string pid, string did)
        {
            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Departments), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin) || RoleID == Convert.ToInt32(UserType.DepartmentUser)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            var objDepartmentModel = new DepartmentModel();
            Session["Edit/Delete"] = "Edit";

            //decrypt parent id(pid)
            pid = !string.IsNullOrEmpty(Convert.ToString(pid)) ? EncryptDecrypt.Decrypt(pid) : "0";

            //decrypt content id(did)
            did = !string.IsNullOrEmpty(Convert.ToString(did)) ? EncryptDecrypt.Decrypt(did) : "0";
            int ParentId = Convert.ToInt32(pid);
            int DepartmentID = Convert.ToInt32(did);
            Int64 TypeMasterID = Convert.ToInt64(AnnouncementTypeAlias.Department);
            using (objContext = new db_KISDEntities())
            {
                var ObjDepartmentData = objContext.Departments.Where(x => x.DepartmentID == DepartmentID).FirstOrDefault();
                ViewBag.BreadCrumTtile = "Department Listing";

                var parentcontentTypeTitle = objContext.Departments.Find(ParentId).NameTxt;
                ViewBag.ParentcontentTypeTitle = parentcontentTypeTitle;

                objDepartmentModel.strCreateDate = DateTime.Today.ToString("MM/dd/yyyy");
                objDepartmentModel.ParentID = ParentId;

                if (ObjDepartmentData != null)
                {
                    objDepartmentModel.DepartmentID = ObjDepartmentData.DepartmentID;
                    objDepartmentModel.ParentID = ObjDepartmentData.ParentID;
                    objDepartmentModel.NameTxt = ObjDepartmentData.NameTxt;
                    objDepartmentModel.strCreateDate = ObjDepartmentData.DepartmentCreateDate.Value.ToString("MM/dd/yyyy");
                    objDepartmentModel.StatusInd = ObjDepartmentData.StatusInd.Value;
                    ViewBag.IsActiveInd = GetStatusData(objDepartmentModel.StatusInd == true ? "1" : "0");
                    objDepartmentModel.CreateByID = ObjDepartmentData.CreateByID;
                    objDepartmentModel.CreateDate = ObjDepartmentData.CreateDate;
                    objDepartmentModel.LastModifyByID = ObjDepartmentData.LastModifyByID;
                    objDepartmentModel.LastModifyDate = ObjDepartmentData.LastModifyDate;
                    ViewBag.Submit = "Update";
                }
                else
                {
                    ViewBag.Submit = "Save";
                    objDepartmentModel.DepartmentCreateDate = DateTime.Now;
                    ViewBag.IsActiveInd = GetStatusData(string.Empty);
                }
                ViewBag.Title = (ObjDepartmentData != null ? " Edit " : " Add ") + " Right Section Details";
                return View(objDepartmentModel);
            }
        }

        /// <summary>
        /// Save update the Department pages              
        /// </summary>
        /// <param name="model">Intialized DepartmentModel model object from view</param>        
        /// <param name="command">Defines Submit or Cancel </param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateSubdepartment(DepartmentModel model, string command, FormCollection fm)
        {
            var rvd = new RouteValueDictionary();
            rvd.Add("page", Request.QueryString["page"] ?? "1");
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "DepartmentCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            rvd.Add("pid", EncryptDecrypt.Encrypt(model.ParentID.ToString()));
            rvd.Add("did", EncryptDecrypt.Encrypt(model.DepartmentID.ToString()));
            rvd.Add("mpage", Request.QueryString["mpage"] ?? "1");
            rvd.Add("mpagesize", Request.QueryString["mpagesize"] ?? "10");
            rvd.Add("mColumn", Request.QueryString["mColumn"] ?? "DepartmentCreateDate");
            rvd.Add("mDirection", Request.QueryString["mDirection"] ?? "Descending");
            Int64 TypeMasterID = Convert.ToInt64(AnnouncementTypeAlias.Department);
            using (var objContext = new db_KISDEntities())
            {
                if (string.IsNullOrEmpty(command))
                {
                    try
                    {
                        #region System Change Log
                        DataTable dtOld;
                        var oldResult = (from a in objContext.Departments
                                         where a.DepartmentID == model.DepartmentID
                                         select a).ToList();
                        dtOld = Models.Common.LINQResultToDataTable(oldResult);
                        #endregion
                        
                        ViewBag.Submit = model.DepartmentID == 0 ? "Save" : "Update";
                        var parentcontentTypeTitle = objContext.Departments.Find(model.ParentID).NameTxt;
                        ViewBag.Title = (model.DepartmentID > 0 ? "Edit " : "Add ") + "Sub Department";
                        ViewBag.ParentcontentTypeTitle = parentcontentTypeTitle;
                        ViewBag.BreadCrumTtile = "Department Listing";
                        var ObjDepartmentData = objContext.Departments.Where(x => x.DepartmentID == model.DepartmentID).FirstOrDefault();
                        if (model != null && !string.IsNullOrEmpty(model.NameTxt))
                        {
                            var count = 0;
                            count = objContext.Departments.Where(x => x.NameTxt.ToLower().Trim() == model.NameTxt.ToLower().Trim() && x.DepartmentID == model.ParentID && x.IsDeletedInd == false).Count(); // Parent name check
                            count = count + objContext.Departments.Where(x => x.NameTxt.ToLower().Trim() == model.NameTxt.ToLower().Trim() && x.DepartmentID != model.DepartmentID && x.ParentID == model.ParentID && x.IsDeletedInd == false).Count();

                            if (count > 0)
                            {
                                ModelState.AddModelError("NameTxt", model.NameTxt + " already exists.");
                                ViewBag.IsActiveInd = GetStatusData(fm["IsActiveInd"].ToString() == "1" ? "1" : "0");
                                return View(model);
                            }
                        }

                        var IsNew = ObjDepartmentData == null;
                        if (ObjDepartmentData == null)
                        {
                            ObjDepartmentData = new Department();
                        }

                        ObjDepartmentData.ParentID = model.ParentID;
                        ObjDepartmentData.IsDeletedInd = false;
                        DateTime dt_to = Convert.ToDateTime(model.strCreateDate, System.Globalization.CultureInfo.InvariantCulture);
                        ObjDepartmentData.DepartmentCreateDate = dt_to;
                        ObjDepartmentData.StatusInd = fm["IsActiveInd"] == "1" ? true : false;
                        ObjDepartmentData.NameTxt = model.NameTxt ?? string.Empty;
                        ObjDepartmentData.CreateDate = model.DepartmentID > 0 ? ObjDepartmentData.CreateDate : DateTime.Now; ;
                        ObjDepartmentData.CreateByID = model.DepartmentID > 0 ? ObjDepartmentData.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        ObjDepartmentData.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        ObjDepartmentData.LastModifyDate = DateTime.Now;
                        if (IsNew)
                        {
                            objContext.Departments.Add(ObjDepartmentData);
                        }

                        objContext.SaveChanges();

                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = "Sub-Department";
                        objSCL.LogTypeTxt = model.DepartmentID > 0 ? "Update" : "Add";
                        objSCL.NotesTxt = "Sub-Department Details" + (model.DepartmentID > 0 ? " updated for " : "  added for ") + model.NameTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();
                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.Departments
                                         where x.DepartmentID == model.DepartmentID
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
                                objContext.SystemChangeLogDetails.Add(objSCLD);
                                objContext.SaveChanges();
                            }
                        }
                        #endregion

                        TempData["AlertMessage"] = parentcontentTypeTitle + " sub department " + (IsNew ? " saved successfully." : " updated successfully.");
                        objContext.SaveChanges();
                    }
                    catch
                    {
                        TempData["AlertMessage"] = "Some error occured, Please try after some time.";
                    }
                    return RedirectToAction("SubDepartmentListing", rvd);
                }
                else
                {
                    return RedirectToAction("SubDepartmentListing", rvd);
                }
            }
        }

        /// <summary>
        /// Delete the Department 
        /// </summary>
        /// <param name="departmentID">departmentID  </param>
        /// <param name="DepartmentTypeId">Type of Department</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteSubdepartment(string did)
        {
            {

                //decrypt parent id(did)
                did = !string.IsNullOrEmpty(Convert.ToString(did)) ? EncryptDecrypt.Decrypt(did) : "0";

                int DepartmentID = Convert.ToInt32(did);

                var rvd = new RouteValueDictionary();
                rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
                rvd.Add("Column", Request.QueryString["Column"] ?? "DepartmentCreateDate");
                rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
                rvd.Add("did", EncryptDecrypt.Encrypt(DepartmentID.ToString()));
                rvd.Add("mpagesize", Request.QueryString["mpagesize"] ?? "10");
                rvd.Add("mColumn", Request.QueryString["mColumn"] ?? "DepartmentCreateDate");
                rvd.Add("mDirection", Request.QueryString["mDirection"] ?? "Descending");
                rvd.Add("pid", Request.QueryString["pid"] ?? EncryptDecrypt.Encrypt("0"));
                Session["Edit/Delete"] = "Delete";
                if (DepartmentID > 0)
                {
                    try
                    {
                        using (var _objdbContext = new db_KISDEntities())
                        {
                            var obj = _objdbContext.Departments.Where(x => x.DepartmentID == DepartmentID).FirstOrDefault();
                            //_objdbContext.Departments.Remove(obj);
                            if (obj != null)
                            {
                                #region System Change Log
                                DataTable dtOld;
                                var oldResult = (from a in _objdbContext.Departments
                                                 where a.DepartmentID == DepartmentID
                                                 select a).ToList();
                                dtOld = Models.Common.LINQResultToDataTable(oldResult);
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
                                objSCL.ModuleTxt = "Sub-Department";
                                objSCL.LogTypeTxt = "Delete";
                                objSCL.NotesTxt = "Sub - Department " + obj.NameTxt + " Details deleted.";
                                objSCL.LogDateTime = DateTime.Now;
                                _objdbContext.SystemChangeLogs.Add(objSCL);
                                _objdbContext.SaveChanges();
                                objSCL = _objdbContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                                var newResult = (from x in _objdbContext.Departments
                                                 where x.DepartmentID == DepartmentID
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
                            var objContentDepartment = _objdbContext.Departments.Where(x => x.DepartmentID == DepartmentID).FirstOrDefault();
                            //_objdbContext.SaveChanges();
                            TempData["AlertMessage"] = objContentDepartment.NameTxt + " sub department details deleted successfully.";
                        }
                    }
                    catch
                    {
                        TempData["AlertMessage"] = "Some error occured while deleting the Sub Department, Please try again later.";
                    }
                }
                int? Page = 1;
                var count = 1;
                using (var _objdbContext = new db_KISDEntities())
                {
                    count = _objdbContext.Departments.Where(x => x.ParentID != null && x.IsDeletedInd == false).Count();
                }
                var page = Request.QueryString["page"] ?? "1";
                var pagesize = Request.QueryString["pagesize"] ?? "10";
                if (Convert.ToInt32(page) > 1)
                    Page = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
                rvd.Add("page", Page);
                return Json(Url.Action("SubDepartmentListing", "Department", rvd));
            }
        }
        #endregion

        #region Department Content

        [Authorize]
        /// <summary>
        /// This method will be called when admin add  or edits the Menu page.
        /// </summary>
        /// <param name="contenttype"></param>
        /// <returns></returns>
        public ActionResult DepartmentContent(string did, string mt, string cid)
        {
            //decrypt menu type id(mt)
            mt = !string.IsNullOrEmpty(Convert.ToString(mt)) ? EncryptDecrypt.Decrypt(mt) : "0";
            //decrypt menu type id(cid)
            cid = !string.IsNullOrEmpty(Convert.ToString(cid)) ? EncryptDecrypt.Decrypt(cid) : "0";
            //decrypt menu type id(did)
            did = !string.IsNullOrEmpty(Convert.ToString(did)) ? EncryptDecrypt.Decrypt(did) : "0";

            var objContentModel = new ContentModel();
            Session["Edit/Delete"] = "Edit";
            ViewBag.FocusPageUrl = false;// Set focus on Pageurl Field if same url exist
            int MenuTypeId = Convert.ToInt32(mt);
            int DepartmentContenttypeId = Convert.ToInt32(cid);
            int DepartmentId = Convert.ToInt32(did);
            using (var objContext = new db_KISDEntities())
            {
                #region Check Tab is Accessible or Not
                int TabType = 0;
                if (DepartmentContenttypeId == 41) { TabType = Convert.ToInt32(ContentTypeAlias.DepartmentEvents); }
                if (DepartmentContenttypeId == 42) { TabType = Convert.ToInt32(ContentTypeAlias.DepartmentStaff); }

                var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
                var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
                var HasTabAccess = GetAccessibleTabAccess(TabType, Convert.ToInt32(userId));
                if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                    || RoleID == Convert.ToInt32(UserType.Admin) || RoleID == Convert.ToInt32(UserType.DepartmentUser)))//if tab not accessible then redirect to home
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
                var objContentData = objContext.Contents.Where(x => x.ContentTypeID == MenuTypeId && x.DepartmentID == DepartmentId).FirstOrDefault();
                var contentTypeTitle = objContext.ContentTypes.Find(MenuTypeId).ContentTypeNameTxt;
                ViewBag.BreadCrumTtile = contentTypeTitle + " Page Listing";
                ViewBag.InnerImages = new SelectList(Models.Common.GetInnerImages(), "ImageID", "TitleTxt");
                objContentModel.strCreateDate = DateTime.Today.ToShortDateString();
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
        public ActionResult DepartmentContent(ContentModel model, string command, FormCollection fm)
        {
            var mt = EncryptDecrypt.Encrypt(model.ContentTypeID.ToString());
            var did = Request.QueryString["did"] ?? EncryptDecrypt.Encrypt("0");
            var rvd = new RouteValueDictionary();
            rvd.Add("page", Request.QueryString["page"] ?? "1");
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "ContentCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            rvd.Add("mt", mt);
            rvd.Add("did", did);
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
                                         where a.ContentTypeID == model.ContentTypeID
                                         select a).ToList();
                        dtOld = Models.Common.LINQResultToDataTable(oldResult);
                        #endregion
                        model.PageTitleTxt = model.PageTitleTxt;
                        ViewBag.Menu = model.ContentTypeID;
                        ViewBag.Submit = model.ContentID == 0 ? "Save" : "Update";
                        var contentType = model.ContentTypeID;
                        var contentTypeTitle = objContext.ContentTypes.Find(contentType).ContentTypeNameTxt;
                        ViewBag.Title = (model != null ? " Edit " : " Add ") + contentTypeTitle + (model.ContentTypeID == Convert.ToInt32(ContentTypeAlias.Fly) ? " Page" : "");
                        ViewBag.BreadCrumTtile = contentTypeTitle + " Page Listing";
                        var objContentData = objContext.Contents.Where(x => x.ContentTypeID == model.ContentTypeID).FirstOrDefault();

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
                        objContentData.DepartmentID = Convert.ToInt64(EncryptDecrypt.Decrypt(did));

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
                    return RedirectToAction("DepartmentListing", rvd);
                }
                else
                {
                    return RedirectToAction("DepartmentListing", rvd);
                }
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


        [HttpPost]
        public JsonResult CheckURL(string url)
        {
            var objContext = new db_KISDEntities();
            var count = 0;
            count += objContext.Departments.Where(x => x.URLTxt.Contains(url)).Count();
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

        #endregion
    }
}