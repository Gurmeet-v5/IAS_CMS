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
using System.Web.Security;
using static KISD.Areas.Admin.Models.Common;

namespace KISD.Areas.Admin.Controllers
{
    public class SchoolController : Controller
    {
        db_KISDEntities objContext = new db_KISDEntities();

        public ActionResult Index(int? page, int? pagesize, GridSortOptions gridSortOptions, string tmi, string si, FormCollection fm, string objresult)
        {
            si = !string.IsNullOrEmpty(Convert.ToString(si)) ? EncryptDecrypt.Decrypt(si) : "0";
            long _tmi = !string.IsNullOrEmpty(Convert.ToString(tmi)) ? Convert.ToInt64(EncryptDecrypt.Decrypt(tmi)) : 0;

            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(
              _tmi == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ?
                Convert.ToInt32(ModuleType.Masters) : Convert.ToInt32(ModuleType.School)
                , Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            if (string.IsNullOrEmpty(gridSortOptions.Column))
            {
                gridSortOptions.Direction = MvcContrib.Sorting.SortDirection.Descending;
            }
            ModelService objModelService = new ModelService();

            using (objContext = new db_KISDEntities())
            {                
                int menutypeId = Convert.ToInt32(si);
                ViewBag.PageTitle = _tmi == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? " School Category Listing" : "School Listing";
                ViewBag.Title = _tmi == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? " School Category Listing" : "School Listing";
                //*******************Fill Values if Display order contains null values***************************
                //var displayOrderList = objContext.Contents.Where(x => x.ContentTypeID == menutypeId && x.ParentID == null).ToList();
                //foreach (var item in displayOrderList)
                //{
                //    if (string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()))
                //    {
                //        var objContentData = objContext.Contents.Where(x => x.ContentID == item.ContentID).FirstOrDefault();
                //        var displayOrder1 = (displayOrderList.Max(x => x.DisplayOrderNbr)) == null ? 1 : displayOrderList.Max(x => x.DisplayOrderNbr).Value + 1;
                //        objContentData.DisplayOrderNbr = displayOrder1;
                //        objContext.SaveChanges();
                //    }
                //}
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
                        var schoolID = Convert.ToInt64(objAjaxRequest.hfid);
                        var objSchool = objContext.Schools.Find(schoolID);
                        if (objSchool != null)
                        {
                            #region System Change Log
                            var oldresult = (from a in objContext.Schools
                                             where a.SchoolID == schoolID
                                             select a).ToList();
                            DataTable dtOld = Models.Common.LINQResultToDataTable(oldresult);
                            #endregion
                            objSchool.StatusInd = objAjaxRequest.hfvalue == "1" ? true : false;
                            var isexist = objContext.Schools.Where(x => x.SchoolCategoryID == schoolID && (x.IsDeletedInd==false || x.IsDeletedInd==null)).Count();
                             isexist += objContext.ExceptionOpportunities.Where(x => x.SchoolCategoryID == schoolID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            if (isexist > 0 && !objSchool.StatusInd.Value)
                            {
                                TempData["Message"] = "School Category is in use, cannot be set as Inactive.";
                            }
                            else
                            {
                                
                                objContext.SaveChanges();
                                TempData["AlertMessage"] = "Status updated successfully.";

                                #region System Change Log
                                SystemChangeLog objSCL = new SystemChangeLog();
                                long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                                User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                                objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                                objSCL.UsernameTxt = objuser.UserNameTxt;
                                objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                                objSCL.ModuleTxt = objSchool.TypeMasterID == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? "School Category" : "School ";
                                objSCL.LogTypeTxt = objSchool.SchoolID > 0 ? "Update" : "Add";
                                objSCL.NotesTxt = (objSchool.TypeMasterID == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? "School Category" : "School ") + " Details" + (objSchool.SchoolID > 0 ? " updated for " : "  added for ") + objSchool.NameTxt;
                                objSCL.LogDateTime = DateTime.Now;
                                objContext.SystemChangeLogs.Add(objSCL);
                                objContext.SaveChanges();

                                objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                                var newResult = (from x in objContext.Schools
                                                 where x.SchoolID == objSchool.SchoolID
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
                                //if (objModelService.ChangeImageDisplayOrder(objMenuContent.DisplayOrderNbr.Value, Convert.ToInt32(objAjaxRequest.qs_value), Convert.ToInt64(objMenuContent.ContentID), menutypeId))
                                //{
                                //    TempData["AlertMessage"] = "Display Order has been changed successfully.";
                                //}
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
                    gridSortOptions.Column = "SchoolCreateDate";
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
                    if (gridSortOptions.Column == "NameTxt"
                        || gridSortOptions.Column == "SchoolCategoryName" || gridSortOptions.Column == "PageTitleTxt"
                        || gridSortOptions.Column == "SchoolCreateDate" || gridSortOptions.Column == "DisplayOrderNum")
                    {

                    }
                    else
                    {
                        gridSortOptions.Column = "SchoolCreateDate";
                    }
                }
                var pagedViewModel = new PagedViewModel<SchoolModel>
                {
                    ViewData = ViewData,
                    Query = objModelService.GetData(_tmi).AsQueryable(),
                    GridSortOptions = gridSortOptions,
                    DefaultSortColumn = "SchoolCreateDate",
                    Page = Page,
                    PageSize = pageSize,
                }
               .Setup();
                if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
                {
                    return View("SchoolPartial", pagedViewModel);// ("partial view name ")
                }
                else
                {
                    return View(pagedViewModel);
                }
            }
        }

        public ActionResult Create(string tmi, string si)
        {
            var objSchoolModel = new SchoolModel();

            ViewBag.Title = "";
            if ((Request.QueryString["si"] == null))
            {
                return RedirectToAction("Index", "Home");
            }

            #region Check Tab is Accessible or Not
            long _tmi = !string.IsNullOrEmpty(Convert.ToString(tmi)) ? Convert.ToInt64(EncryptDecrypt.Decrypt(tmi)) : 0;
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(
              _tmi == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ?
                Convert.ToInt32(ModuleType.Masters) : Convert.ToInt32(ModuleType.School)
                , Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion
            ViewBag.DateAdded = DateTime.Now.ToShortDateString();
            //decrypt image id(iid)
            si = !string.IsNullOrEmpty(Convert.ToString(si)) ? EncryptDecrypt.Decrypt(si) : "0";

            int SchoolID = Convert.ToInt32(si);
            if (SchoolID > 0 && objContext.GalleryListings.Where(x => x.ListingID == SchoolID && x.IsDeletedInd == true).Any())
            {
                return RedirectToAction("Index", "Home");
            }
            Session["Edit/Delete"] = "Edit";
            
            ViewBag.LiTitle = _tmi == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? " School Category Listing" : "School Listing";
            ViewBag.PageTitle = (si == "0" ? "Add " : "Edit ") + (_tmi == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? " School Category" : "School Listing");
            ViewBag.Submit = (si == "0" ? "Save" : "Update");

            objSchoolModel.TypeMasterID = _tmi == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) : Convert.ToInt32(GalleryListingService.TypeMaster.School);

            if (Convert.ToInt32(si) > 0)
            {
                var schoolListing = (from u in objContext.Schools
                                     where u.SchoolID == SchoolID
                                     && u.TypeMasterID == _tmi
                                     select u).FirstOrDefault();
                if (schoolListing != null)
                {
                    objSchoolModel.SchoolID = schoolListing.SchoolID;
                    objSchoolModel.NameTxt = schoolListing.NameTxt;
                    objSchoolModel.AddressTxt = schoolListing.AddressTxt;
                    objSchoolModel.WebsiteURLTxt = schoolListing.WebsiteURLTxt;
                    objSchoolModel.PhoneNumberTxt = schoolListing.PhoneNumberTxt;
                    objSchoolModel.StatusInd = schoolListing.StatusInd.HasValue ? schoolListing.StatusInd.Value : false;
                    objSchoolModel.CreateDate = schoolListing.CreateDate.HasValue ? schoolListing.CreateDate.Value : DateTime.Now;
                    objSchoolModel.CreatedByID = schoolListing.CreateByID.HasValue ? schoolListing.CreateByID.Value : 0;
                    objSchoolModel.LastModifyDate = schoolListing.LastModifyDate.HasValue ? schoolListing.LastModifyDate.Value : DateTime.Now;
                    objSchoolModel.LastModifyByID = schoolListing.LastModifyByID.HasValue ? schoolListing.LastModifyByID.Value : 0;
                    objSchoolModel.IsDeletedInd = false;
                    objSchoolModel.PageURLTxt = schoolListing.PageURLTxt;
                    if (_tmi == Convert.ToInt32(GalleryListingService.TypeMaster.School))
                    {
                        ViewBag.SchoolCategory = GetSchoolCategories(schoolListing.SchoolCategoryID.ToString());
                    }
                    ViewBag.StatusInd = GetStatusData(schoolListing.StatusInd.Value ? "1" : "0");
                    ViewBag.DateAdded = schoolListing.SchoolCreateDate.Value.ToShortDateString();
                }
            }
            else
            {
                if (_tmi == Convert.ToInt32(GalleryListingService.TypeMaster.School))
                {
                    ViewBag.SchoolCategory = GetSchoolCategories("");
                }
                ViewBag.StatusInd = GetStatusData(string.Empty);
            }

            if (_tmi == Convert.ToInt32(GalleryListingService.TypeMaster.School))
            {
                //var TypeMasterID = Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory);

                //ViewBag.SchoolCategory = new SelectList(objContext.Schools.
                //    Where(x => x.TypeMasterID == TypeMasterID).ToList()
                //    .OrderBy(x => x.NameTxt), "SchoolID", "NameTxt");

                //ViewBag.SchoolCategory = GetSchoolCategories();
            }

            return View(objSchoolModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(SchoolModel objSchoolModel, string command, FormCollection fm)
        {
            ViewBag.Title = "";
            var rvd = new RouteValueDictionary();
            rvd.Add("page", Request.QueryString["page"] ?? "1");
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "SchoolCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            rvd.Add("si", Request.QueryString["si"] ?? EncryptDecrypt.Encrypt("0"));
            rvd.Add("tmi", Request.QueryString["tmi"] ?? EncryptDecrypt.Encrypt("0"));
            var masterid = EncryptDecrypt.Encrypt(objSchoolModel.TypeMasterID.ToString());
            var TypeMasterID = Convert.ToInt32(EncryptDecrypt.Decrypt(masterid));
            ViewBag.LiTitle = TypeMasterID == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? " School Category Listing" : "School Listing";
            ViewBag.PageTitle = (objSchoolModel.SchoolID == 0 ? "Add " : "Edit ") + (TypeMasterID == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? " School Category" : "School Listing");
            ViewBag.StatusInd = GetStatusData(objSchoolModel.StatusInd ? "1" : "0");
            objSchoolModel.strCreateDate = objSchoolModel.SchoolCreateDate.ToShortDateString();
            ViewBag.DateAdded = objSchoolModel.SchoolCreateDate.ToShortDateString();
            if (TypeMasterID == Convert.ToInt32(GalleryListingService.TypeMaster.School))
            {
                objSchoolModel.SchoolCategoryID = Convert.ToInt64(fm["SchoolCategory"]);
                ViewBag.SchoolCategory = GetSchoolCategories(objSchoolModel.SchoolCategoryID.ToString());
            }
            using (var objContext = new db_KISDEntities())
            {               
                if (string.IsNullOrEmpty(command))
                {
                    try
                    {
                        #region System Change Log
                        DataTable dtOld;
                        var oldResult = (from a in objContext.Schools
                                         where a.SchoolID == objSchoolModel.SchoolID
                                         select a).ToList();
                        dtOld = Models.Common.LINQResultToDataTable(oldResult);
                        #endregion

                        if (objSchoolModel != null && !string.IsNullOrEmpty(objSchoolModel.NameTxt)
                            //&& objSchoolModel.TypeMasterID == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory)
                            )
                        {
                            var count = 0;
                            count = objContext.Schools.Where(x => x.NameTxt.ToLower().Trim() == objSchoolModel.NameTxt.ToLower().Trim()
                                    && x.SchoolID != objSchoolModel.SchoolID && x.TypeMasterID == objSchoolModel.TypeMasterID && (x.IsDeletedInd==false || x.IsDeletedInd==null)).Count();
                            if (objSchoolModel.NameTxt.Trim().ToLower() == "error404")
                            {
                                count = count + 1;
                            }
                            if (count > 0)
                            {
                                //if user types url 'error404' below validation msg should display
                                if (objSchoolModel.NameTxt.ToLower().Trim() == "error404" || (objSchoolModel.NameTxt.ToLower().Trim() == "admin"))
                                {
                                    ModelState.AddModelError("NameTxt", objSchoolModel.NameTxt + " name is not allowed.");
                                }
                                else
                                {
                                    ModelState.AddModelError("NameTxt", objSchoolModel.NameTxt + " name already exists.");
                                }
                                //ViewBag.FocusPageUrl = true;// Set focus on Pageurl Field if same url exist
                                ViewBag.IsActiveInd = GetStatusData(fm["StatusInd"].ToString() == "1" ? "1" : "0");
                                return View(objSchoolModel);
                            }
                        }
                        objSchoolModel.StatusInd = fm["StatusInd"] == "1" ? true : false;
                        var isexist = objContext.Schools.Where(x => x.SchoolCategoryID == objSchoolModel.SchoolID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                        isexist += objContext.ExceptionOpportunities.Where(x => x.SchoolCategoryID == objSchoolModel.SchoolID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                        if (isexist > 0 && !objSchoolModel.StatusInd)
                        {
                            ViewBag.IsActiveInd = GetStatusData(fm["StatusInd"].ToString() == "1" ? "1" : "0");
                            TempData["Message"] = "School Category is in use, cannot be set as Inactive.";
                            return View(objSchoolModel);
                        }
                        long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();

                        School objSchool = null;
                        if (objSchoolModel.SchoolID == 0)
                        {
                            objSchool = new School();
                        }
                        else
                        {
                            objSchool = objContext.Schools.Where(x => x.SchoolID == objSchoolModel.SchoolID).FirstOrDefault();
                        }

                        if (objSchoolModel.TypeMasterID == Convert.ToInt32(GalleryListingService.TypeMaster.School))
                        {
                            objSchool.SchoolCategoryID = Convert.ToInt64(fm["SchoolCategory"]);
                            objSchool.AddressTxt = objSchoolModel.AddressTxt;
                            objSchool.PhoneNumberTxt = objSchoolModel.PhoneNumberTxt;
                            objSchool.WebsiteURLTxt = objSchoolModel.WebsiteURLTxt;
                        }
                        else
                        {
                            objSchool.PhoneNumberTxt = null;
                            objSchool.SchoolCategoryID = null;
                            objSchool.AddressTxt = null;
                            objSchool.WebsiteURLTxt = null;
                        }
                        objSchool.PageURLTxt = objSchoolModel.PageURLTxt;
                        objSchool.NameTxt = objSchoolModel.NameTxt;
                        objSchool.StatusInd = fm["StatusInd"] == "1" ? true : false;
                        DateTime dt_to = Convert.ToDateTime(objSchoolModel.SchoolCreateDate, System.Globalization.CultureInfo.InvariantCulture);
                        objSchool.SchoolCreateDate = dt_to;
                        objSchool.CreateByID = objSchoolModel.SchoolID > 0 ? objSchool.CreateByID : objuser.UserID;
                        objSchool.IsDeletedInd = false;
                        objSchool.LastModifyByID = objuser.UserID;
                        objSchool.LastModifyDate = DateTime.Now;
                        objSchool.CreateDate = objSchoolModel.SchoolID > 0 ? objSchool.CreateDate : DateTime.Now; 
                        objSchool.TypeMasterID = objSchoolModel.TypeMasterID;

                        if (objSchoolModel.SchoolID == 0)
                        {
                            objContext.Schools.Add(objSchool);
                        }

                        objContext.SaveChanges();

                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = objSchoolModel.TypeMasterID == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? "School Category" : "School";
                        objSCL.LogTypeTxt = objSchoolModel.SchoolID > 0 ? "Update" : "Add";
                        objSCL.NotesTxt = (objSchoolModel.TypeMasterID == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? "School Category" : "School") + " Details" + (objSchoolModel.SchoolID > 0 ? " updated for " : "  added for ") + objSchoolModel.NameTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();
                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.Schools
                                         where x.SchoolID == objSchool.SchoolID
                                         select x);
                        DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                        foreach (DataColumn col in dtNew.Columns)
                        {
                            if (objSchoolModel.SchoolID > 0)
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

                        TempData["AlertMessage"] = objSchool.NameTxt +(objSchoolModel.TypeMasterID == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? " School Category details" : " School details") + (objSchoolModel.SchoolID == 0 ? " saved" : " updated") + " successfully.";
                    }
                    catch (Exception e)
                    {
                        TempData["AlertMessage"] = "Some error occured, Please try after some time.";
                    }
                }

                return RedirectToAction("Index", rvd);
            }
        }

        [HttpPost]
        public JsonResult Delete(string si, string tmi)
        {
            var rvd = new RouteValueDictionary();
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "SchoolCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            rvd.Add("si", si);
            rvd.Add("tmi", tmi);

            //decrypt school id(mt)
            si = !string.IsNullOrEmpty(Convert.ToString(si)) ? EncryptDecrypt.Decrypt(si) : "0";

            int schoolID = Convert.ToInt32(si);
            Session["Edit/Delete"] = "Delete";
            long _tmi = !string.IsNullOrEmpty(Convert.ToString(tmi)) ? Convert.ToInt64(EncryptDecrypt.Decrypt(tmi)) : 0;
            if (schoolID > 0)
            {
                try
                {
                    using (var objContext = new db_KISDEntities())
                    {
                        var obj = objContext.Schools.Where(x => x.SchoolID == schoolID).First();
                        if (obj != null)
                        {
                            #region System Change Log
                            DataTable dtOld;
                            var oldResult = (from a in objContext.Schools
                                             where a.SchoolID == schoolID
                                             select a).ToList();
                            dtOld = Models.Common.LINQResultToDataTable(oldResult);
                            #endregion
                            var isexist = objContext.Schools.Where(x => x.SchoolCategoryID == obj.SchoolID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            isexist += objContext.ExceptionOpportunities.Where(x => x.SchoolCategoryID == obj.SchoolID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            if (isexist > 0)
                            {
                                TempData["Message"] = "School Category is in use, cannot be deleted.";
                            }
                            else
                            {
                                obj.IsDeletedInd = true;
                                objContext.SaveChanges();

                                #region System Change Log
                                SystemChangeLog objSCL = new SystemChangeLog();
                                long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                                User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                                objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                                objSCL.UsernameTxt = objuser.UserNameTxt;
                                objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                                objSCL.ModuleTxt = _tmi == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? "School Category" : "School ";
                                objSCL.LogTypeTxt = "Delete";
                                objSCL.NotesTxt = obj.NameTxt + " Details deleted";
                                objSCL.LogDateTime = DateTime.Now;
                                objContext.SystemChangeLogs.Add(objSCL);
                                objContext.SaveChanges();
                                objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                                var objContextNew = new db_KISDEntities();
                                var newResult = (from x in objContextNew.Schools
                                                 where x.SchoolID == schoolID
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
                                        objSCLD.NewValueTxt = col.ColumnName == "IsDeletedInd" ? dtNew.Rows[0][col.ColumnName].ToString() : "";
                                        objContext.SystemChangeLogDetails.Add(objSCLD);
                                        objContext.SaveChanges();
                                    }
                                }
                                #endregion
                                TempData["AlertMessage"] = obj.NameTxt + (obj.TypeMasterID == Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory) ? " School Category details" : " School details") + " deleted successfully.";
                            }
                        }
                        
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
                count = objContext.Schools.Where(x => x.SchoolID == schoolID).Count();
            }
            var page = Request.QueryString["page"] ?? "1";
            var pagesize = Request.QueryString["pagesize"] ?? "10";
            if (Convert.ToInt32(page) > 1)
                Page = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
            rvd.Add("page", Page);
            return Json(Url.Action("Index", "School", rvd));
        }

        [HttpPost]
        public JsonResult CheckURL(string url)
        {
            var objContext = new db_KISDEntities();
            var count = 0;

            //count = objContext.Events.Where(x => x.PageURLTxt.Contains(url)).Count();
            //count = objContext.News.Where(x => x.PageURLTxt.Contains(url)).Count();
            count += objContext.Schools.Where(x => x.PageURLTxt.Contains(url)).Count();
            if (count > 0)
            {
                if (url != "")
                    url = url + count;
            }
            return Json(url);
        }
        #region Private Methods
        private List<SelectListItem> GetStatusData(string value)
        {
            List<SelectListItem> lstStatus = new List<SelectListItem>();
            SelectListItem lstStatusData = new SelectListItem();
            lstStatusData.Text = "Active";
            lstStatusData.Value = "1";
            lstStatus.Add(lstStatusData);
            lstStatusData = new SelectListItem();
            lstStatusData.Text = "Inactive";
            lstStatusData.Value = "0";
            lstStatus.Add(lstStatusData);
            if (!string.IsNullOrEmpty(value))
            {
                lstStatus.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return lstStatus;
        }

        private List<SelectListItem> GetSchoolCategories(string schoolCategoryID)
        {
            List<SelectListItem> lstSelectListItem = new List<SelectListItem>();
            var TypeMasterID = Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory);
            var data = objContext.Schools.Where(x => x.TypeMasterID == TypeMasterID && x.StatusInd == true && (x.IsDeletedInd == false || x.IsDeletedInd==null)).ToList();

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