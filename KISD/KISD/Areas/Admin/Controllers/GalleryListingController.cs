using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using KISD.Areas.Admin.Models;
using GalleryListingTypeAlias = KISD.Areas.Admin.Models.GalleryListingService.TypeMaster;
using System.Web.Security;
using System.Data;
using static KISD.Areas.Admin.Models.Common;
using System.Data.Entity;

namespace KISD.Areas.Admin.Controllers
{
    public class GalleryListingController : Controller
    {
        private GalleryListingService _service;
        db_KISDEntities objContext = new db_KISDEntities();
        #region Gallery Listing
        /// <summary>
        /// Code to create instance Gallery Listing Service class in constructor
        /// </summary>
        public GalleryListingController()
        {
            _service = new GalleryListingService();
        }

        [Authorize]
        /// <summary>
        /// this method will show the Gallery listing with all type master.
        /// </summary>
        /// <param name="Page">this parameter is used to get page number to be shown.</param>
        /// <param name="PageSize">this parameter is used to get no of recorde to be shown.</param>
        /// <param name="gridSortOptions">this parameter is used to get grid sorting option.</param>
        /// <param name="glt">this parameter is used to get type id of the gallery listing i.e. 1,2 or 3</param>
        /// <param name="formCollection">this parameter is used to get controls collection on the page.</param>
        /// <param name="ObjResult"></param>
        /// <returns>view to enter image details.</returns>
        public ActionResult Index(int? Page, int? PageSize, GridSortOptions gridSortOptions, string glt, FormCollection formCollection, string ObjResult, PagedViewModel<NewsEvent> model, string command)
        {
            var db_obj = new db_KISDEntities();
            if (string.IsNullOrEmpty(gridSortOptions.Column))
            {
                gridSortOptions.Direction = MvcContrib.Sorting.SortDirection.Descending;
            }

            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Masters), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            //Check for valid ImageTypeID
            if (glt == null)
            {
                return RedirectToAction("Index", "Home");
            }
            //decrypt image type id(it)
            if (!string.IsNullOrEmpty(Convert.ToString(glt)))
            {
                glt = Convert.ToString(EncryptDecrypt.Decrypt(glt));
            }
            TempData["CroppedImage"] = null;
            var GalleryListingType = glt != null ? Convert.ToInt64(glt) : Convert.ToInt64(GalleryListingTypeAlias.ImageListing);
            ViewBag.ListingTypeId = GalleryListingType;

            //*******************Fill Values if Display order contains null values***************************
            var displayOrderList = objContext.GalleryListings.Where(x => x.TypeMasterID == GalleryListingType && x.IsDeletedInd == false).ToList();
            foreach (var item in displayOrderList)
            {
                if (string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()))
                {
                    var objContentData = objContext.GalleryListings.Where(x => x.ListingID == item.ListingID).FirstOrDefault();
                    var NewdisplayOrder = (displayOrderList.Max(x => x.DisplayOrderNbr)) == null ? 1 : displayOrderList.Max(x => x.DisplayOrderNbr).Value + 1;
                    objContentData.DisplayOrderNbr = NewdisplayOrder;
                    objContext.SaveChanges();
                }
            }
            //***********************************************************
            #region Ajax Call
            if (ObjResult != null)
            {
                #region Parameters
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
                    else if (objAjaxRequest.ajaxcall == "showonhome")//Ajax Call type = showonhome 
                    {
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }

                    //here we will check filter values
                    model.strCreateDate = objAjaxRequest.qs_FilterFromDate;
                    model.status = objAjaxRequest.qs_FilterUserType;
                    model.Title = objAjaxRequest.qs_FilterUserName;
                    model.ModuleType = objAjaxRequest.qs_FilterModuleType;
                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
                }
                #endregion

                //Ajax Call for update status for images
                if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && ObjResult != null && !string.IsNullOrEmpty(ObjResult))
                {
                    var ListingID = System.Convert.ToInt64(objAjaxRequest.hfid);
                    var gallerylisting = objContext.GalleryListings.Find(ListingID);
                    if (gallerylisting != null)
                    {
                        GalleryListingModel glm = new GalleryListingModel();
                        var NotesTxt = string.Empty;
                        #region System Change Log Old Value
                        var oldresult = (from a in objContext.GalleryListings
                                         where a.ListingID == ListingID
                                         select a).ToList();
                        DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                        #endregion

                        if (objAjaxRequest.qs_Type == "displayorder")
                        {
                            if (GalleryListingService.ChangeDisplayOrder(gallerylisting.DisplayOrderNbr.Value, Convert.ToInt64(objAjaxRequest.qs_value), gallerylisting.ListingID, Convert.ToInt32(gallerylisting.TypeMasterID)))
                            {
                                NotesTxt = "Display order updated for " + gallerylisting.TitleTxt;
                                TempData["AlertMessage"] = "Display Order has been changed successfully.";
                            }
                        }
                        else if (objAjaxRequest.qs_Type == "showonhome")
                        {
                            var ismorethenthree = objContext.GalleryListings.Where(x => x.TypeMasterID == 4 && x.ShowOnHomeInd == true
                             && x.IsDeletedInd == false && x.StatusInd == true).Count();
                            gallerylisting.ShowOnHomeInd = objAjaxRequest.hfvalue == "1";
                            //Check for videos listing
                            if (gallerylisting.ShowOnHomeInd == true && gallerylisting.StatusInd == false && gallerylisting.TypeMasterID == 4)
                            {
                                TempData["Message"] = "Inactive Videos cannot be set to Show on Home.";
                            }
                            else if (ismorethenthree >= 3 && gallerylisting.ShowOnHomeInd == true && gallerylisting.TypeMasterID == 4)
                            {
                                TempData["Message"] = "More than three Videos cannot be set as Active on Home.";
                            }
                            else
                            {
                                NotesTxt = "Show on Home updated for " + gallerylisting.TitleTxt;
                                objContext.SaveChanges();
                                TempData["AlertMessage"] = "Show on home updated successfully.";
                            }

                        }
                        else// for status
                        {
                            gallerylisting.StatusInd = objAjaxRequest.hfvalue == "1";
                            if (gallerylisting.StatusInd == false && gallerylisting.ShowOnHomeInd == true && gallerylisting.TypeMasterID == 4)
                            {
                                TempData["Message"] = "Show om Home Videos cannot be set to Inactive.";
                            }
                            else
                            {
                                objContext.SaveChanges();
                                NotesTxt = "Status updated for " + gallerylisting.TitleTxt;
                                TempData["AlertMessage"] = "Status updated successfully.";
                            }


                        }
                        if (!string.IsNullOrEmpty(NotesTxt))
                        {
                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Gallery Listing";
                            objSCL.LogTypeTxt = gallerylisting.ListingID > 0 ? "Update" : "Add";
                            objSCL.NotesTxt = "Image Listing Section Details" + (gallerylisting.ListingID > 0 ? " updated for " : "  added for ") + gallerylisting.TitleTxt;
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();
                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in objContext.GalleryListings
                                             where x.ListingID == gallerylisting.ListingID
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
                        objAjaxRequest.hfid = null;//remove parameter value
                        objAjaxRequest.hfvalue = null;//remove parameter value
                        PageSize = ((Request.QueryString["pagesize"] != null && Request.QueryString["pagesize"].ToString() != "All") ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : PageSize);
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
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

            ViewBag.Title = ViewBag.PageTitle = _service.GetGalleryListingType(GalleryListingType);//(GalleryListingType == 1 || GalleryListingType == 2 ? "s" : "")

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
                gridSortOptions.Column = "ListingCreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column == "TitleTxt" || gridSortOptions.Column == "ListingCreateDate" || gridSortOptions.Column == "DisplayOrderNbr")
            {

            }
            else
            {
                gridSortOptions.Column = "ListingCreateDate";
            }
            //.. Code for get records as page view model
            var pagesize = PageSize.HasValue ? PageSize.Value : Models.Common._pageSize;
            var page = Page.HasValue ? Page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;

            var pagedViewModel = new PagedViewModel<GalleryListingModel>
            {
                ViewData = ViewData,
                Query = _service.GetGalleryListingView(Convert.ToInt64(glt)).AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "ListingCreateDate",
                Page = page,
                PageSize = pagesize,
            };
            if (!string.IsNullOrEmpty(model.Title) && (string.IsNullOrEmpty(command) || command != "Cancel"))
            {
                pagedViewModel.AddFilter("title", model.Title,
            a => a.TitleTxt.Contains(model.Title));
            }
            if (model.status != "All" && (string.IsNullOrEmpty(command) || command != "Cancel"))
            {
                pagedViewModel.AddFilter("status", model.status, a => a.StatusInd.ToString() == model.status);
            }
            if (!string.IsNullOrEmpty(model.strCreateDate) && (string.IsNullOrEmpty(command) || command != "Cancel"))
            {
                model.dtCreateDate = Convert.ToDateTime(model.strCreateDate);
                pagedViewModel.AddFilter("dateadded", model.strCreateDate, a => a.ListingCreateDate == model.dtCreateDate);
            }
            pagedViewModel.Setup();
            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("GalleryListingPartial", pagedViewModel);// ("partial view name ")
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
        /// <param name="glt">gallery listing type id</param>
        /// <param name="glid"> isting id</param>
        /// <returns></returns>
        public ActionResult Create(string glt, string glid)
        {
            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Masters), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            var objGalleryListingModel = new GalleryListingModel();
            //Check for valid ImageTypeID
            ViewBag.ListingTypeId = glt;
            ViewBag.Title = "";
            if ((Request.QueryString["glt"] == null) && (Request.QueryString["glid"] == null))
            {
                return RedirectToAction("Index", "Home");
            }

            //decrypt image type id(it)
            glt = !string.IsNullOrEmpty(Convert.ToString(glt)) ? EncryptDecrypt.Decrypt(glt) : "0";

            //decrypt image id(iid)
            glid = !string.IsNullOrEmpty(Convert.ToString(glid)) ? EncryptDecrypt.Decrypt(glid) : "0";
            int ListingID = Convert.ToInt32(glid);
            if (ListingID > 0 && objContext.GalleryListings.Where(x => x.ListingID == ListingID && x.IsDeletedInd == true).Any())
            {
                return RedirectToAction("Index", "Home");
            }
            Session["Edit/Delete"] = "Edit";
            ViewBag.PageTitle = (glid == "0" ? "Add " : "Edit ") + (_service.GetGalleryListingType(Convert.ToInt64(glt)));
            ViewBag.Submit = (glid == "0" ? "Save" : "Update");
            ViewBag.ListingTypeId = glt;
            ViewBag.Title = (glid == "0" ? "Add " : "Edit ") + (_service.GetGalleryListingType(Convert.ToInt64(glt)));
            ViewBag.ImageTypeTitle = _service.GetGalleryListingType(Convert.ToInt64(glt));
            objGalleryListingModel.ListingCreateDate = DateTime.Now;
            ViewBag.Date = DateTime.Now.ToShortDateString();
            ViewBag.StartDateStr = "";
            ViewBag.EndDateStr = "";

            objGalleryListingModel.TypeMasterID = Convert.ToInt64(glt);
            if (Convert.ToInt32(glid) > 0)
            {
                var galleryListing = (from u in objContext.GalleryListings
                                      where u.ListingID == ListingID
                                      select u).FirstOrDefault();
                if (galleryListing != null)
                {
                    objGalleryListingModel.ListingID = galleryListing.ListingID;
                    objGalleryListingModel.TitleTxt = galleryListing.TitleTxt;
                    objGalleryListingModel.FileURLTxt = galleryListing.FileURLTxt;
                    objGalleryListingModel.EmbededURLTxt = galleryListing.EmbededURLTxt;
                    objGalleryListingModel.AltImageTxt = galleryListing.AltImageTxt;
                    objGalleryListingModel.URLTxt = galleryListing.URLTxt;
                    objGalleryListingModel.DisplayOrderNbr = galleryListing.DisplayOrderNbr;
                    objGalleryListingModel.DisplayStartDate = galleryListing.DisplayStartDate;
                    objGalleryListingModel.DisplayEndDate = galleryListing.DisplayEndDate;
                    objGalleryListingModel.UploadTypeNbr = galleryListing.UploadTypeNbr;
                    objGalleryListingModel.DescriptionTxt = galleryListing.DescriptionTxt;
                    objGalleryListingModel.AuthorTxt = galleryListing.AuthorTxt;
                    objGalleryListingModel.ShowOnHomeInd = galleryListing.ShowOnHomeInd;
                    objGalleryListingModel.PageMetaTitleTxt = galleryListing.PageMetaTitleTxt;
                    objGalleryListingModel.PageMetaDescriptionTxt = galleryListing.PageMetaDescriptionTxt;
                    objGalleryListingModel.StatusInd = galleryListing.StatusInd;
                    objGalleryListingModel.ListingCreateDate = Convert.ToDateTime(galleryListing.ListingCreateDate.Value.ToShortDateString());// galleryListing.ListingCreateDate;
                    objGalleryListingModel.CreateDate = galleryListing.CreateDate;
                    objGalleryListingModel.CreateByID = galleryListing.CreateByID;
                    objGalleryListingModel.LastModifyDate = galleryListing.LastModifyDate;
                    objGalleryListingModel.LastModifyByID = galleryListing.LastModifyByID;
                    objGalleryListingModel.IsDeletedInd = galleryListing.IsDeletedInd;
                    objGalleryListingModel.TypeMasterID = galleryListing.TypeMasterID;
                    objGalleryListingModel.ParentID = galleryListing.ParentID;
                    objGalleryListingModel.BannerImageID = galleryListing.BannerImageID;
                    objGalleryListingModel.AltBannerImageTxt = galleryListing.AltBannerImageTxt;
                    objGalleryListingModel.BannerImageAbstractTxt = galleryListing.BannerImageAbstractTxt;
                    ViewBag.StatusInd = GetStatusData(objGalleryListingModel.StatusInd.Value ? "1" : "0");
                    ViewBag.Date = objGalleryListingModel.ListingCreateDate.Value.ToShortDateString();
                    ViewBag.ShowonHomeInd = Models.Common.GetShowonHomeDataBoolean(galleryListing.ShowOnHomeInd != null && galleryListing.ShowOnHomeInd.Value ? "True" : "False");
                    ViewBag.StartDateStr = objGalleryListingModel.DisplayStartDate.HasValue ? objGalleryListingModel.DisplayStartDate.Value.ToString("MM/dd/yyyy hh:mm tt") : null;
                    ViewBag.EndDateStr = objGalleryListingModel.DisplayEndDate.HasValue ? objGalleryListingModel.DisplayEndDate.Value.ToString("MM/dd/yyyy hh:mm tt") : null;
                    objGalleryListingModel.SelectedStatus = true;
                }
            }
            else
            {
                ViewBag.StatusInd = GetStatusData(string.Empty);
                ViewBag.ShowonHomeInd = Models.Common.GetShowonHomeDataBoolean("False");
            }
            ViewBag.InnerImages = new SelectList(Models.Common.GetInnerImages(), "ImageID", "TitleTxt");
            return View(objGalleryListingModel);
        }

        /// <summary>
        /// this method wil post the details of gallery Listing filled by the admin.
        /// </summary>
        /// <param name="command">command name whether Save or Cancel.</param>
        /// <param name="fm">controls collection on the page.</param>
        /// <param name="model">object of gallery Listing model</param>
        /// <returns>view with status message.</returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(string command, FormCollection fm, GalleryListingModel model)
        {
            try
            {
                var width = 260;
                HttpPostedFileBase file = Request.Files.Count > 0 ? Request.Files[0] : null;
                ViewBag.Title = (model.ListingID == 0 ? "Add " : "Edit ") + _service.GetGalleryListingType(model.TypeMasterID.Value);
                ViewBag.StatusInd = GetStatusData(string.Empty);
                ViewBag.Submit = (model.ListingID == 0 ? "Save" : "Update");
                ViewBag.ListingTypeId = model.TypeMasterID;
                ViewBag.Date = model.ListingCreateDate.Value.ToShortDateString().Trim();
                ViewBag.StartDateStr = model.DisplayStartDate != null ? Convert.ToString(model.DisplayStartDate) : "";
                ViewBag.EndDateStr = model.DisplayEndDate != null ? Convert.ToString(model.DisplayEndDate) : "";
                var rvd = new RouteValueDictionary();
                rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
                rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
                rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "CreateDate");
                rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
                rvd.Add("glt", EncryptDecrypt.Encrypt(Convert.ToString(model.TypeMasterID)));
                rvd.Add("glid", EncryptDecrypt.Encrypt(Convert.ToString(model.ListingID)));
                ViewBag.PageTitle = (model.ListingID == 0 ? "Add " : "Edit ") + (model.TypeMasterID.Value == Convert.ToInt64(GalleryListingTypeAlias.ImageListing) ? _service.GetGalleryListingType(Convert.ToInt64(model.TypeMasterID.Value)) : (_service.GetGalleryListingType(Convert.ToInt64(model.TypeMasterID.Value))));
                ViewBag.ImageTypeTitle = _service.GetGalleryListingType(model.TypeMasterID.Value);

                #region System Change Log
                DataTable dtOld;
                var oldresult = (from a in objContext.GalleryListings
                                 where a.ListingID == model.ListingID
                                 select a).ToList();
                dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                #endregion

                if (string.IsNullOrEmpty(command))
                {
                    GalleryListing objGListing;
                    if (model.ListingID > 0)
                    {
                        objGListing = objContext.GalleryListings.Find(model.ListingID);
                    }
                    else
                    {
                        objGListing = new GalleryListing();
                        objGListing.IsDeletedInd = false;
                    }
                    ViewBag.InnerImages = new SelectList(Models.Common.GetInnerImages(), "ImageID", "TitleTxt");//get all the inner image titles
                    objGListing.TypeMasterID = model.TypeMasterID;
                    objGListing.StatusInd = fm["StatusInd"] !=null? fm["StatusInd"].ToString() == "True":true;
                    ViewBag.StatusInd = GetStatusData(fm["StatusInd"] != null ? fm["StatusInd"].ToString() == "True" ? "True" : "False":"True");
                    ViewBag.ShowonHomeInd = GetShowonHomeData(fm["ShowonHomeInd"] == "True" ? "True" : "False");
                    //if (fm["StatusInd"] == "False" && fm["ShowonHomeInd"] == "True")
                    //{
                    //    TempData["Message"] = "Inactive Video cannot be set to Show on Home.";
                    //    return View(model);
                    //}
                    //Code Restrict user to active only two or less than two listing in footer.
                    var ShowonHomeInd = 0;
                    if (model.ListingID == 0)
                        ShowonHomeInd = objContext.GalleryListings.Where(x => x.ShowOnHomeInd == true && x.TypeMasterID == model.TypeMasterID && x.StatusInd == true && x.IsDeletedInd == false).Count();
                    else
                        ShowonHomeInd = objContext.GalleryListings.Where(x => x.ShowOnHomeInd == true && x.TypeMasterID == model.TypeMasterID && x.StatusInd == true && x.ListingID != model.ListingID && x.IsDeletedInd == false).Count();

                    if (ShowonHomeInd >= 3 && (model.ShowOnHomeInd == true || fm["ShowonHomeInd"] == "True"))
                    {
                        TempData["Message"] = "More than three Videos cannot be set as Active on Home.";
                        return View(model);
                    }
                    //if (fm["StatusInd"] == "False" && model.ShowOnHomeInd == true)
                    //{
                    //    TempData["Message"] = "Show on home Video cannot be set as inactive.";
                    //    return View(model);
                    //}
                    if (!string.IsNullOrEmpty(model.TitleTxt))
                    {
                        var titletxt = model.TitleTxt.Trim().ToLower();
                        var chkListingTitle = objContext.GalleryListings.Where(x => x.TitleTxt.Trim().ToLower() == titletxt
                                                                    && x.TypeMasterID == model.TypeMasterID
                                                                    && x.ListingID != model.ListingID && x.IsDeletedInd == false).Count();
                        if (chkListingTitle > 0)//check image title on adding new image details or updating existing 1
                        {
                            TempData["CroppedImage"] = null;
                            ModelState.AddModelError("TitleTxt", model.TitleTxt + " title already exists.");
                            return View(model);
                        }
                    }
                    if (model != null && !string.IsNullOrEmpty(model.URLTxt))
                    {
                        #region Check for duplicate or error404 URL's
                        var count = 0;
                        count = objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim()).Count();
                        count += objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.ListingID != model.ListingID && x.IsDeletedInd == false).Count();
                        count += objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.ExceptionOpportunities.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.Departments.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.NewsEvents.Where(x => x.PageURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.RightSections.Where(x => x.ExternalLinkURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();

                        if (model.URLTxt.Trim().ToLower() == "error404")// Check for duplicate url and error404 url
                        {
                            count = count + 1;
                        }
                        if (count > 0)
                        {
                            ViewBag.FocusPageUrl = true;// Set focus on Pageurl Field if same url exist
                            if (model.URLTxt.ToLower().Trim() == "error404")//if user types url 'error404' below validation msg should display
                            {
                                ModelState.AddModelError("URLTxt", model.URLTxt + " URL is not allowed.");
                            }
                            else
                            {
                                ModelState.AddModelError("URLTxt", model.URLTxt + " URL already exists.");
                            }
                            return View(model);
                        }
                        #endregion
                    }

                    #region Fill Model

                    objGListing.ListingID = model.ListingID;
                    objGListing.TitleTxt = model.TitleTxt;
                    objGListing.FileURLTxt = model.FileURLTxt;
                    objGListing.EmbededURLTxt = model.EmbededURLTxt;
                    objGListing.AltImageTxt = model.AltImageTxt;
                    objGListing.URLTxt = model.URLTxt;
                    objGListing.DisplayStartDate = model.DisplayStartDate != null ? model.DisplayStartDate : null;
                    objGListing.DisplayEndDate = model.DisplayEndDate != null ? model.DisplayEndDate : null;
                    objGListing.UploadTypeNbr = model.UploadTypeNbr;
                    objGListing.DescriptionTxt = model.DescriptionTxt;
                    objGListing.AuthorTxt = model.AuthorTxt;
                    objGListing.ShowOnHomeInd = fm["ShowonHomeInd"] == "True";
                    objGListing.PageMetaTitleTxt = model.PageMetaTitleTxt;
                    objGListing.PageMetaDescriptionTxt = model.PageMetaDescriptionTxt;
                    objGListing.ListingCreateDate = Convert.ToDateTime(model.ListingCreateDate.Value.ToShortDateString());
                    objGListing.CreateDate = model.ListingID > 0 ? objGListing.CreateDate : DateTime.Now;
                    objGListing.CreateByID = model.ListingID > 0 ? objGListing.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    objGListing.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    objGListing.LastModifyDate = DateTime.Now;
                    objGListing.TypeMasterID = model.TypeMasterID;
                    objGListing.ParentID = model.ParentID;
                    objGListing.BannerImageID = model.BannerImageID;
                    objGListing.AltBannerImageTxt = model.AltBannerImageTxt;
                    objGListing.BannerImageAbstractTxt = model.BannerImageAbstractTxt;
                    ViewBag.StatusInd = GetStatusData(fm["StatusInd"] != null ? fm["StatusInd"].ToString() == "True" ? "True" : "False" : "True");
                    ViewBag.Date = model.ListingCreateDate.Value.ToShortDateString();
                    #endregion

                    if (TempData["CroppedImage"] == null)
                    {
                        #region  Image
                        if (file != null && file.ContentLength > 0)
                        {
                            try
                            {
                                Models.Common.DeleteImage(Server.MapPath(objGListing.FileURLTxt));
                            }
                            catch
                            {
                            }
                            var fileName = Path.GetFileName(file.FileName);
                            #region Upload Files
                            /*Create folder in the webdata folder if not created*/
                            Models.Common.CreateFolder();
                            //.. Get extension of the document
                            var fileExtension = fileName.Substring(fileName.LastIndexOf("."), fileName.Length - fileName.LastIndexOf("."));
                            //.. Set fullname of the document path
                            var MyGuid = Guid.NewGuid();
                            fileName = MyGuid.ToString() + fileExtension;
                            //.. Create path of the document to save  into the defined physical path.
                            var strfolder = (model.TypeMasterID.Value == (long)GalleryListingTypeAlias.ImageListing ? "WebData\\ImageListing\\" :
                                (model.TypeMasterID == (long)GalleryListingTypeAlias.DocumentViewer ? "WebData\\DocumentViewer\\" :
                                (model.TypeMasterID == (long)GalleryListingTypeAlias.Podcast ? "WebData\\PodCast\\" :
                                 (model.TypeMasterID == (long)GalleryListingTypeAlias.Video ? "WebData\\Video\\" :
                                 (model.TypeMasterID == (long)GalleryListingTypeAlias.PhotoGallery ? "WebData\\PhotoGallery\\Images\\" :
                                "WebData\\images\\")))));
                            var strPath = Request.PhysicalApplicationPath + strfolder + fileName;
                            file.SaveAs(strPath);
                            objGListing.FileURLTxt = "~/" + strfolder + fileName;
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        /*Create folder in the webdata folder if not created*/
                        Models.Common.CreateFolder();
                        #region Cropped Image
                        try
                        {
                            Models.Common.DeleteImage(Server.MapPath(objGListing.FileURLTxt));
                            Models.Common.DeleteImage(Server.MapPath(objGListing.FileURLTxt).Replace("images", "cropped"));
                            Models.Common.DeleteImage(Server.MapPath(objGListing.FileURLTxt).Replace("images", "thumbnails"));
                        }
                        catch { }

                        //.. Create path of the document to save  into the defined physical path.
                        var strfolder = (model.TypeMasterID.Value == (long)GalleryListingTypeAlias.ImageListing ? "WebData\\ImageListing\\" :
                            (model.TypeMasterID == (long)GalleryListingTypeAlias.DocumentViewer ? "WebData\\DocumentViewer\\" :
                            (model.TypeMasterID == (long)GalleryListingTypeAlias.Podcast ? "WebData\\PodCast\\" :
                             (model.TypeMasterID == (long)GalleryListingTypeAlias.Video ? "WebData\\Video\\" :
                             (model.TypeMasterID == (long)GalleryListingTypeAlias.PhotoGallery ? "WebData\\PhotoGallery\\" :
                            "WebData\\images\\")))));
                        var croppedfile = new FileInfo(Server.MapPath(TempData["CroppedImage"].ToString()));
                        var fileName = croppedfile.Name;
                        croppedfile = null;
                        var strPath = Request.PhysicalApplicationPath + strfolder + "images\\" + fileName;
                        file.SaveAs(strPath);/*Save orignal file in the image folder*/
                        var sourcePath = Server.MapPath(TempData["CroppedImage"].ToString());
                        var targetPath = Request.PhysicalApplicationPath + strfolder;
                        System.IO.File.Copy(Path.Combine(sourcePath.Replace(fileName, ""), fileName), Path.Combine(targetPath + "cropped\\", fileName), true);
                        var mysmallImage = Models.Common.CreateImageThumbnail(targetPath + (model.TypeMasterID == (long)GalleryListingTypeAlias.PhotoGallery || model.TypeMasterID == (long)GalleryListingTypeAlias.PhotoGalleryImages ? "cropped\\" : "images\\") + fileName, 100);
                        mysmallImage.Save(targetPath + "thumbnails\\" + fileName, System.Drawing.Imaging.ImageFormat.Png);
                        mysmallImage.Dispose();
                        objGListing.FileURLTxt = "~/" + strfolder + "cropped/" + fileName;
                        try
                        {
                            System.IO.File.Delete(Server.MapPath(TempData["CroppedImage"].ToString()));
                        }
                        catch
                        {

                        }
                        TempData["CroppedImage"] = null;
                        #endregion
                    }
                    if (model.ListingID > 0)
                    {
                        TempData["AlertMessage"] = _service.GetGalleryListingType(model.TypeMasterID.Value) + " details updated successfully.";
                        objContext.SaveChanges();
                    }
                    else
                    {
                        objContext.GalleryListings.Add(objGListing);
                        objContext.SaveChanges();
                        TempData["AlertMessage"] = _service.GetGalleryListingType(model.TypeMasterID.Value) + " details saved successfully.";
                    }
                    #region System Change Log
                    SystemChangeLog objSCL = new SystemChangeLog();
                    long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                    objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                    objSCL.UsernameTxt = objuser.UserNameTxt;
                    objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                    objSCL.ModuleTxt = "Gallery Listing";
                    objSCL.LogTypeTxt = model.ListingID > 0 ? "Update" : "Add";
                    objSCL.NotesTxt = (_service.GetGalleryListingType(model.TypeMasterID.Value)) + " Details" + (model.ListingID > 0 ? " updated for " : "  added for ") + model.TitleTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();
                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var newResult = (from x in objContext.GalleryListings
                                     where x.ListingID == model.ListingID
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
                    return RedirectToAction("Index", "GalleryListing", rvd);
                }
                else
                {
                    return RedirectToAction("Index", "GalleryListing", rvd);
                }
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = "Some error occured. Please try again after some time.";
                return View(model);
            }
        }

        [Authorize]
        /// <summary>
        /// this method will show the Gallery listing with all type master.
        /// </summary>
        /// <param name="Page">this parameter is used to get page number to be shown.</param>
        /// <param name="PageSize">this parameter is used to get no of recorde to be shown.</param>
        /// <param name="gridSortOptions">this parameter is used to get grid sorting option.</param>
        /// <param name="glt">this parameter is used to get type id of the gallery listing i.e. 1,2 or 3</param>
        /// <param name="formCollection">this parameter is used to get controls collection on the page.</param>
        /// <param name="ObjResult"></param>
        /// <returns>view to enter gallery Listing details.</returns>
        public ActionResult SubGalleryListing(int? Page, int? PageSize, GridSortOptions gridSortOptions, string glt, string pid, FormCollection formCollection, string ObjResult)
        {
            var db_obj = new db_KISDEntities();
            //Check for valid ImageTypeID
            if (glt == null && pid == null)
            {
                return RedirectToAction("Index", "Home");
            }
            //decrypt image type id(it)
            if (!string.IsNullOrEmpty(Convert.ToString(glt)))
            {
                glt = Convert.ToString(EncryptDecrypt.Decrypt(glt));
            }
            if (!string.IsNullOrEmpty(Convert.ToString(pid)))
            {
                pid = Convert.ToString(EncryptDecrypt.Decrypt(pid));
            }
            TempData["CroppedImage"] = null;
            long ParentID = Convert.ToInt64(pid);
            var GalleryListingType = glt != null ? Convert.ToInt64(glt) : Convert.ToInt64(GalleryListingTypeAlias.ImageListing);
            ViewBag.ListingTypeId = GalleryListingType;

            ViewBag.ParentTitle = objContext.GalleryListings.Where(x => x.ListingID == ParentID).FirstOrDefault().TitleTxt;

            //*******************Fill Values if Display order contains null values***************************
            var displayOrderList = objContext.GalleryListings.Where(x => x.TypeMasterID == GalleryListingType && x.ParentID == ParentID && x.IsDeletedInd == false).ToList();
            foreach (var item in displayOrderList)
            {
                if (string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()))
                {
                    var objContentData = objContext.GalleryListings.Where(x => x.ListingID == item.ListingID).FirstOrDefault();
                    var NewdisplayOrder = (displayOrderList.Max(x => x.DisplayOrderNbr)) == null ? 1 : displayOrderList.Max(x => x.DisplayOrderNbr).Value + 1;
                    objContentData.DisplayOrderNbr = NewdisplayOrder;
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
                    else if (objAjaxRequest.ajaxcall == "displayorder")//Ajax Call type = Display Order i.e. drop down values
                    {
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
                }

                //Ajax Call for update status for images
                if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && ObjResult != null && !string.IsNullOrEmpty(ObjResult))
                {
                    var ListingID = System.Convert.ToInt64(objAjaxRequest.hfid);
                    var gallerylisting = objContext.GalleryListings.Find(ListingID);
                    if (gallerylisting != null)
                    {
                        #region System Change Log
                        var oldresult = (from a in objContext.GalleryListings
                                         where a.ListingID == ListingID
                                         select a).ToList();
                        DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                        #endregion
                        gallerylisting.StatusInd = objAjaxRequest.hfvalue == "1";
                        var content = objContext.Contents.Where(x => x.DescriptionTxt.Contains(gallerylisting.FileURLTxt)).ToList();
                        if (content.Count > 0 && !gallerylisting.StatusInd.Value)
                        {
                            TempData["Message"] = "File is in use, cannot be set as Inactive.";
                        }
                        else
                        {
                            if (objAjaxRequest.qs_Type == "displayorder")
                            {
                                if (GalleryListingService.ChangeDisplayOrder(gallerylisting.DisplayOrderNbr.Value, Convert.ToInt64(objAjaxRequest.qs_value), gallerylisting.ListingID, Convert.ToInt32(gallerylisting.TypeMasterID)))
                                {
                                    TempData["AlertMessage"] = "Display Order has been changed successfully.";
                                }
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
                                objSCL.ModuleTxt = "Gallery Listing";
                                objSCL.LogTypeTxt = gallerylisting.ListingID > 0 ? "Update" : "Add";
                                objSCL.NotesTxt = "Image Listing Section Details" + (gallerylisting.ListingID > 0 ? " updated for " : "  added for ") + gallerylisting.TitleTxt;
                                objSCL.LogDateTime = DateTime.Now;
                                objContext.SystemChangeLogs.Add(objSCL);
                                objContext.SaveChanges();
                                objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                                var newResult = (from x in objContext.GalleryListings
                                                 where x.ListingID == gallerylisting.ListingID
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
                        PageSize = ((Request.QueryString["pagesize"] != null && Request.QueryString["pagesize"].ToString() != "All") ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : PageSize);
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
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

            ViewBag.Title = ViewBag.PageTitle = _service.GetGalleryListingType(GalleryListingType);

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
                gridSortOptions.Column = "ListingCreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column == "TitleTxt" || gridSortOptions.Column == "ListingCreateDate" || gridSortOptions.Column == "DisplayOrderNbr")
            {

            }
            else
            {
                gridSortOptions.Column = "ListingCreateDate";
            }
            //.. Code for get records as page view model
            var pagesize = PageSize.HasValue ? PageSize.Value : Models.Common._pageSize;
            var page = Page.HasValue ? Page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;

            var pagedViewModel = new PagedViewModel<GalleryListingModel>
            {
                ViewData = ViewData,
                Query = _service.GetGalleryListingView(Convert.ToInt64(glt), Convert.ToInt64(pid)).AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "ListingCreateDate",
                Page = page,
                PageSize = pagesize,
            }.Setup();
            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("SubGalleryListingPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }

        /// <summary>
        /// This method is used to delete the gallery Listing from database.It also chacks wheather gallery Listing is in use or not.
        /// If gallery Listing is in use then it return to view and show message "image is in use, cannot be deleted." else it delte the image and return to thye view.
        /// </summary>
        /// <param name="glt">This parameter is used to get the Type master id</param>
        /// <param name="glid">This parameter is used to get the listing id</param>
        /// <param name="fm">this parameter is used to get the form control values from view </param>
        /// <param name="returnUrl"></param>
        /// <returns>This method return the Json result (url) that will be passed to the Ajax post method on client side.</returns>
        [HttpPost]
        public JsonResult Delete(string glt, string glid, FormCollection fm)
        {
            //decrypt image type id(it)
            glt = !string.IsNullOrEmpty(Convert.ToString(glt)) ? EncryptDecrypt.Decrypt(glt) : "0";
            //decrypt image id(iid)
            glid = !string.IsNullOrEmpty(Convert.ToString(glid)) ? EncryptDecrypt.Decrypt(glid) : "0";

            //.. Code for get the route value directory
            RouteValueDictionary rvd = new RouteValueDictionary();
            ViewBag.ListingTypeId = glt;
            ViewBag.Title = _service.GetGalleryListingType(Convert.ToInt32(glt));
            var page = Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString();
            var pagesize = Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString();
            rvd.Add("pagesize", pagesize);
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "ListingCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
            rvd.Add("glt", EncryptDecrypt.Encrypt(glt));
            TempData["pager"] = pagesize;
            Session["Edit/Delete"] = "Delete";
            try
            {
                // TODO: Add delete logic here
                //.. Check for gallery Listing in use
                GalleryListing objListing = objContext.GalleryListings.Find(Convert.ToInt32(glid));
                long ListingID = Convert.ToInt64(glid);
                #region System Change Log
                var oldresult = (from a in objContext.GalleryListings
                                 where a.ListingID == ListingID
                                 select a).ToList();
                DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                #endregion

                if (objListing != null)
                {
                    //****************Display Order ************************
                    var objGalleryListing = objContext.GalleryListings.Where(x => x.TypeMasterID == objListing.TypeMasterID && x.IsDeletedInd == false).FirstOrDefault();
                    if (objGalleryListing != null)
                    {
                        try
                        {
                            var objGalleryListingService = new GalleryListingService();
                            objGalleryListingService.ChangeDeletedDisplayOrder(objListing.DisplayOrderNbr.Value, objListing.ListingID, objListing.TypeMasterID.Value);
                        }
                        catch { }
                    }
                    //***************************************************
                    #region System Change Log
                    SystemChangeLog objSCL = new SystemChangeLog();
                    long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                    objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                    objSCL.UsernameTxt = objuser.UserNameTxt;
                    objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                    objSCL.ModuleTxt = "Gallery Listing";
                    objSCL.LogTypeTxt = "Delete";
                    objSCL.NotesTxt = (_service.GetGalleryListingType(objListing.TypeMasterID.Value)) + " Details deleted for " + objListing.TitleTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();
                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var newResult = (from x in objContext.GalleryListings
                                     where x.ListingID == ListingID
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
                        Models.Common.DeleteImage(Server.MapPath(objListing.FileURLTxt.Replace("cropped","images")));
                        Models.Common.DeleteImage(Server.MapPath(objListing.FileURLTxt));
                        Models.Common.DeleteImage(Server.MapPath(objListing.FileURLTxt.Replace("Thumbnails", "images")));
                        Models.Common.DeleteImage(Server.MapPath(objListing.FileURLTxt.Replace("Thumbnails_Small", "images")));
                    }
                    catch
                    {
                    }
                    TempData["AlertMessage"] = _service.GetGalleryListingType(Convert.ToInt64(glt)) + " details deleted successfully.";
                }
                //.. Checks for no of records in current page if exists records then return same page number else decrease the page number
                int? CheckPage = 1;
                int ListingTypeID = Convert.ToInt32(glt);
                var count = objContext.GalleryListings.Where(x => x.TypeMasterID == ListingTypeID && x.IsDeletedInd == false).Count();
                if (Convert.ToInt32(page) > 1)
                    CheckPage = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
                rvd.Add("page", CheckPage);
                return Json(Url.Action("Index", "GalleryListing", rvd));
            }
            catch (Exception ex)
            {
                rvd.Add("page", page);
                return Json(Url.Action("Index", "GalleryListing", rvd));
            }
        }

        #endregion

        #region Sub Gallery Listing

        [Authorize]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="glt">gallery listing type id</param>
        /// <param name="glid"> isting id</param>
        /// <returns></returns>
        public ActionResult CreateSubGallery(string glt, string glid, string pid)
        {
            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Masters), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            var objGalleryListingModel = new GalleryListingModel();
            //Check for valid ImageTypeID
            ViewBag.ListingTypeId = glt;
            ViewBag.Title = "";
            if ((Request.QueryString["glt"] == null) && (Request.QueryString["glid"] == null) && (Request.QueryString["pid"] == null))
            {
                return RedirectToAction("Index", "Home");
            }

            //decrypt image type id(it)
            glt = !string.IsNullOrEmpty(Convert.ToString(glt)) ? EncryptDecrypt.Decrypt(glt) : "0";

            //decrypt image id(iid)
            glid = !string.IsNullOrEmpty(Convert.ToString(glid)) ? EncryptDecrypt.Decrypt(glid) : "0";
            pid = !string.IsNullOrEmpty(Convert.ToString(pid)) ? EncryptDecrypt.Decrypt(pid) : "0";
            long ListingID = Convert.ToInt64(glid);
            long ParentID = Convert.ToInt64(pid);
            ViewBag.ParentTitle = objContext.GalleryListings.Where(x => x.ListingID == ParentID).FirstOrDefault().TitleTxt;
            if (ListingID > 0 && objContext.GalleryListings.Where(x => x.ListingID == ListingID && x.IsDeletedInd == true).Any())
            {
                return RedirectToAction("Index", "Home");
            }
            Session["Edit/Delete"] = "Edit";
            ViewBag.PageTitle = (glid == "0" ? "Add " : "Edit ") + (_service.GetGalleryListingType(Convert.ToInt64(glt)));
            ViewBag.Submit = (glid == "0" ? "Save" : "Update");
            ViewBag.ListingTypeId = glt;
            ViewBag.Title = (glid == "0" ? "Add " : "Edit ") + (_service.GetGalleryListingType(Convert.ToInt64(glt)));
            ViewBag.ImageTypeTitle = _service.GetGalleryListingType(Convert.ToInt64(glt));
            objGalleryListingModel.ListingCreateDate = DateTime.Now;
            ViewBag.Date = DateTime.Now.ToShortDateString();
            ViewBag.StartDateStr = "";
            ViewBag.EndDateStr = "";
            objGalleryListingModel.TypeMasterID = Convert.ToInt64(glt);
            objGalleryListingModel.ParentID = ParentID;
            if (Convert.ToInt32(glid) > 0)
            {
                var galleryListing = (from u in objContext.GalleryListings
                                      where u.ListingID == ListingID
                                      select u).FirstOrDefault();
                if (galleryListing != null)
                {
                    objGalleryListingModel.ListingID = galleryListing.ListingID;
                    objGalleryListingModel.TitleTxt = galleryListing.TitleTxt;
                    objGalleryListingModel.FileURLTxt = galleryListing.FileURLTxt;
                    objGalleryListingModel.EmbededURLTxt = galleryListing.EmbededURLTxt;
                    objGalleryListingModel.AltImageTxt = galleryListing.AltImageTxt;
                    objGalleryListingModel.URLTxt = galleryListing.URLTxt;
                    objGalleryListingModel.DisplayOrderNbr = galleryListing.DisplayOrderNbr;
                    objGalleryListingModel.DisplayStartDate = galleryListing.DisplayStartDate;
                    objGalleryListingModel.DisplayEndDate = galleryListing.DisplayEndDate;
                    objGalleryListingModel.UploadTypeNbr = galleryListing.UploadTypeNbr;
                    objGalleryListingModel.DescriptionTxt = galleryListing.DescriptionTxt;
                    objGalleryListingModel.AuthorTxt = galleryListing.AuthorTxt;
                    objGalleryListingModel.ShowOnHomeInd = galleryListing.ShowOnHomeInd;
                    objGalleryListingModel.PageMetaTitleTxt = galleryListing.PageMetaTitleTxt;
                    objGalleryListingModel.PageMetaDescriptionTxt = galleryListing.PageMetaDescriptionTxt;
                    objGalleryListingModel.StatusInd = galleryListing.StatusInd;
                    objGalleryListingModel.ListingCreateDate = Convert.ToDateTime(galleryListing.ListingCreateDate.Value.ToShortDateString());// galleryListing.ListingCreateDate;
                    objGalleryListingModel.CreateDate = galleryListing.CreateDate;
                    objGalleryListingModel.CreateByID = galleryListing.CreateByID;
                    objGalleryListingModel.LastModifyDate = galleryListing.LastModifyDate;
                    objGalleryListingModel.LastModifyByID = galleryListing.LastModifyByID;
                    objGalleryListingModel.IsDeletedInd = galleryListing.IsDeletedInd;
                    objGalleryListingModel.TypeMasterID = galleryListing.TypeMasterID;
                    objGalleryListingModel.ParentID = galleryListing.ParentID;
                    ViewBag.StatusInd = GetStatusData(objGalleryListingModel.StatusInd.Value ? "1" : "0");
                    ViewBag.Date = objGalleryListingModel.ListingCreateDate.Value.ToShortDateString();
                    ViewBag.ShowonHomeInd = Models.Common.GetShowonHomeDataBoolean(galleryListing.ShowOnHomeInd != null && galleryListing.ShowOnHomeInd.Value ? "True" : "False");
                    objGalleryListingModel.SelectedStatus = true;
                    objGalleryListingModel.BannerImageID = galleryListing.BannerImageID;
                    objGalleryListingModel.AltBannerImageTxt = galleryListing.AltBannerImageTxt;
                    objGalleryListingModel.BannerImageAbstractTxt = galleryListing.BannerImageAbstractTxt;
                    ViewBag.StartDateStr = objGalleryListingModel.DisplayStartDate.HasValue ? objGalleryListingModel.DisplayStartDate.Value.ToString("MM/dd/yyyy hh:mm tt") : null;
                    ViewBag.EndDateStr = objGalleryListingModel.DisplayEndDate.HasValue ? objGalleryListingModel.DisplayEndDate.Value.ToString("MM/dd/yyyy hh:mm tt") : null;
                }
            }
            else
            {
                ViewBag.StatusInd = GetStatusData(string.Empty);
                ViewBag.ShowonHomeInd = Models.Common.GetShowonHomeDataBoolean("False");
            }
            return View(objGalleryListingModel);
        }

        /// <summary>
        /// this method wil post the details of gallery Listing filled by the admin.
        /// </summary>
        /// <param name="command">command name whether Save or Cancel.</param>
        /// <param name="fm">controls collection on the page.</param>
        /// <param name="model">object of gallery Listing model</param>
        /// <returns>view with status message.</returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateSubGallery(string command, FormCollection fm, GalleryListingModel model, string pid)
        {
            try
            {
                var width = 1600;
                HttpPostedFileBase file = Request.Files.Count > 0 ? Request.Files[0] : null;
                ViewBag.Title = (model.ListingID == 0 ? "Add " : "Edit ") + _service.GetGalleryListingType(model.TypeMasterID.Value);
                ViewBag.StatusInd = GetStatusData(string.Empty);
                ViewBag.Submit = (model.ListingID == 0 ? "Save" : "Update");
                ViewBag.ListingTypeId = model.TypeMasterID;
                ViewBag.Date = model.ListingCreateDate.Value.ToShortDateString().Trim();
                ViewBag.StartDateStr = model.DisplayStartDate != null ? Convert.ToString(model.DisplayStartDate) : "";
                ViewBag.EndDateStr = model.DisplayEndDate != null ? Convert.ToString(model.DisplayEndDate) : "";
                var rvd = new RouteValueDictionary();
                rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
                rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
                rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "CreateDate");
                rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");

                rvd.Add("gpage", Request.QueryString["gpage"] != null ? Request.QueryString["gpage"].ToString() : Models.Common._currentPage.ToString());
                rvd.Add("gpagesize", Request.QueryString["gpagesize"] != null ? Request.QueryString["gpagesize"].ToString() : Models.Common._pageSize.ToString());
                rvd.Add("gColumn", Request.QueryString["gColumn"] != null ? Request.QueryString["gColumn"].ToString() : "CreateDate");
                rvd.Add("gDirection", Request.QueryString["gDirection"] != null ? Request.QueryString["gDirection"].ToString() : "Descending");

                rvd.Add("glt", EncryptDecrypt.Encrypt(Convert.ToString(model.TypeMasterID)));
                rvd.Add("pid", EncryptDecrypt.Encrypt(Convert.ToString(model.ParentID)));
                ViewBag.PageTitle = (model.ListingID == 0 ? "Add " : "Edit ") + _service.GetGalleryListingType(model.TypeMasterID.Value) ;
                ViewBag.ImageTypeTitle = _service.GetGalleryListingType(model.TypeMasterID.Value);

                ViewBag.ParentTitle = objContext.GalleryListings.Where(x => x.ListingID == model.ParentID).FirstOrDefault().TitleTxt;

                #region System Change Log
                DataTable dtOld;
                var oldresult = (from a in objContext.GalleryListings
                                 where a.ListingID == model.ListingID
                                 select a).ToList();
                dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                #endregion

                if (string.IsNullOrEmpty(command))
                {
                    GalleryListing objGListing;
                    if (model.ListingID > 0)
                    {
                        objGListing = objContext.GalleryListings.Find(model.ListingID);
                    }
                    else
                    {
                        objGListing = new GalleryListing();
                        objGListing.IsDeletedInd = false;
                    }
                    objGListing.TypeMasterID = model.TypeMasterID;
                    objGListing.StatusInd = fm["StatusInd"].ToString() == "True";
                    ViewBag.StatusInd = GetStatusData(fm["StatusInd"].ToString() == "True" ? "True" : "False");

                    ViewBag.ShowonHomeInd = GetShowonHomeData(fm["ShowonHomeInd"] == "True" ? "True" : "False");
                    if (fm["StatusInd"] == "False" && fm["ShowonHomeInd"] == "True")
                    {
                        TempData["Message"] = "Inactive Video cannot be set to Show on Home.";
                        return View(model);
                    }
                    //Code Restrict user to active only two or less than two listing in footer.
                    var ShowonHomeInd = 0;
                    if (model.ListingID == 0)
                        ShowonHomeInd = objContext.GalleryListings.Where(x => x.ShowOnHomeInd == true && x.StatusInd == true && x.IsDeletedInd == false).Count();
                    else
                        ShowonHomeInd = objContext.GalleryListings.Where(x => x.ShowOnHomeInd == true && x.StatusInd == true && x.ListingID != model.ListingID && x.IsDeletedInd == false).Count();

                    if (ShowonHomeInd >= 3 && (model.ShowOnHomeInd == true || fm["ShowonHomeInd"] == "True"))
                    {
                        TempData["Message"] = "More than three Videos cannot be set as Active on Home.";
                        return View(model);
                    }
                    if (fm["StatusInd"] == "False" && model.ShowOnHomeInd == true)
                    {
                        TempData["Message"] = "Show on home Video cannot be set as inactive.";
                        return View(model);
                    }
                    if (!string.IsNullOrEmpty(model.TitleTxt))
                    {
                        var chkListingTitle = objContext.GalleryListings.Where(x => x.TitleTxt == model.TitleTxt.Trim()
                                                                    && x.TypeMasterID == model.TypeMasterID
                                                                    && x.ListingID != model.ListingID).Any();
                        if (chkListingTitle)//check image title on adding new image details or updating existing 1
                        {
                            TempData["CroppedImage"] = null;
                            ModelState.AddModelError("TitleTxt", model.TitleTxt + " title already exists.");
                            return View(model);
                        }
                    }

                    if (model != null && !string.IsNullOrEmpty(model.URLTxt))
                    {
                        var count = 0;
                        count = objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.ListingID != model.ListingID && x.IsDeletedInd == false).Count();
                        count += objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.ExceptionOpportunities.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.Departments.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.NewsEvents.Where(x => x.PageURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                        count += objContext.RightSections.Where(x => x.ExternalLinkURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                        if (model.URLTxt.Trim().ToLower() == "error404")// Check for duplicate url and error404 url
                        {
                            count = count + 1;
                        }
                        if (count > 0)
                        {
                            ViewBag.FocusPageUrl = true;// Set focus on Pageurl Field if same url exist
                            if (model.URLTxt.ToLower().Trim() == "error404")//if user types url 'error404' below validation msg should display
                            {
                                ModelState.AddModelError("URLTxt", model.URLTxt + " URL is not allowed.");
                            }
                            else
                            {
                                ModelState.AddModelError("URLTxt", model.URLTxt + " URL already exists.");
                            }
                            return View(model);
                        }
                    }

                    objGListing.ListingID = model.ListingID;
                    objGListing.TitleTxt = model.TitleTxt;
                    objGListing.FileURLTxt = model.FileURLTxt;
                    objGListing.EmbededURLTxt = model.EmbededURLTxt;
                    objGListing.AltImageTxt = model.AltImageTxt;
                    objGListing.URLTxt = model.URLTxt;
                    objGListing.DisplayStartDate = model.DisplayStartDate != null ? model.DisplayStartDate : null;
                    objGListing.DisplayEndDate = model.DisplayEndDate != null ? model.DisplayEndDate : null;
                    objGListing.UploadTypeNbr = model.UploadTypeNbr;
                    objGListing.DescriptionTxt = model.DescriptionTxt;
                    objGListing.AuthorTxt = model.AuthorTxt;
                    objGListing.ShowOnHomeInd = fm["ShowonHomeInd"] == "True";
                    objGListing.PageMetaTitleTxt = model.PageMetaTitleTxt;
                    objGListing.PageMetaDescriptionTxt = model.PageMetaDescriptionTxt;
                    objGListing.ListingCreateDate = Convert.ToDateTime(model.ListingCreateDate.Value.ToShortDateString());
                    objGListing.CreateDate = model.ListingID > 0 ? objGListing.CreateDate : DateTime.Now;
                    objGListing.CreateByID = model.ListingID > 0 ? objGListing.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    objGListing.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    objGListing.LastModifyDate = DateTime.Now;
                    objGListing.TypeMasterID = model.TypeMasterID;
                    objGListing.ParentID = model.ParentID;
                    objGListing.BannerImageID = model.BannerImageID;
                    objGListing.AltBannerImageTxt = model.AltBannerImageTxt;
                    objGListing.BannerImageAbstractTxt = model.BannerImageAbstractTxt;
                    ViewBag.StatusInd = GetStatusData(fm["StatusInd"].ToString() == "True" ? "True" : "False");
                    ViewBag.Date = model.ListingCreateDate.Value.ToShortDateString();
                    if (TempData["CroppedImage"] == null)
                    {
                        #region  Image
                        if (file != null && file.ContentLength > 0)
                        {
                            try
                            {
                                Models.Common.DeleteImage(Server.MapPath(objGListing.FileURLTxt));
                            }
                            catch
                            {
                            }
                            var fileName = Path.GetFileName(file.FileName);
                            #region Upload Files
                            /*Create folder in the webdata folder if not created*/
                            Models.Common.CreateFolder();
                            //.. Get extension of the document
                            var fileExtension = fileName.Substring(fileName.LastIndexOf("."), fileName.Length - fileName.LastIndexOf("."));
                            //.. Set fullname of the document path
                            var MyGuid = Guid.NewGuid();
                            fileName = MyGuid.ToString() + fileExtension;
                            //.. Create path of the document to save  into the defined physical path.
                            var strfolder = (model.TypeMasterID.Value == (long)GalleryListingTypeAlias.ImageListing ? "WebData\\ImageListing\\" :
                                (model.TypeMasterID == (long)GalleryListingTypeAlias.DocumentViewer ? "WebData\\DocumentViewer\\" :
                                (model.TypeMasterID == (long)GalleryListingTypeAlias.Podcast ? "WebData\\PodCast\\" :
                                 (model.TypeMasterID == (long)GalleryListingTypeAlias.Video ? "WebData\\Video\\" :
                                 (model.TypeMasterID == (long)GalleryListingTypeAlias.PhotoGallery ? "WebData\\PhotoGallery\\" :
                                "WebData\\images\\")))));
                            var strPath = Request.PhysicalApplicationPath + strfolder + fileName;
                            file.SaveAs(strPath);
                            objGListing.FileURLTxt = "~/" + strfolder + fileName;
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        /*Create folder in the webdata folder if not created*/
                        Models.Common.CreateFolder();

                        #region Cropped Image
                        try
                        {
                            Models.Common.DeleteImage(Server.MapPath(objGListing.FileURLTxt));
                            Models.Common.DeleteImage(Server.MapPath(objGListing.FileURLTxt).Replace("images", "thumbnails"));
                            Models.Common.DeleteImage(Server.MapPath(objGListing.FileURLTxt).Replace("images", "thumbnails_Small"));
                        }
                        catch { }

                        //.. Create path of the document to save  into the defined physical path.
                        var strfolder = (model.TypeMasterID.Value == (long)GalleryListingTypeAlias.ImageListing ? "WebData\\ImageListing\\" :
                            (model.TypeMasterID == (long)GalleryListingTypeAlias.DocumentViewer ? "WebData\\DocumentViewer\\" :
                            (model.TypeMasterID == (long)GalleryListingTypeAlias.Podcast ? "WebData\\PodCast\\" :
                             (model.TypeMasterID == (long)GalleryListingTypeAlias.Video ? "WebData\\Video\\" :
                             (model.TypeMasterID == (long)GalleryListingTypeAlias.PhotoGallery ? "WebData\\PhotoGallery\\" :
                             (model.TypeMasterID == (long)GalleryListingTypeAlias.PhotoGalleryImages ? "WebData\\PhotoGallery\\" :
                            "WebData\\images\\"))))));
                        var croppedfile = new FileInfo(Server.MapPath(TempData["CroppedImage"].ToString()));
                        var fileName = croppedfile.Name;
                        croppedfile = null;
                        var strPath = Request.PhysicalApplicationPath + strfolder + "images\\" + fileName;
                        file.SaveAs(strPath);/*Save orignal file in the image folder*/
                        var sourcePath = Server.MapPath(TempData["CroppedImage"].ToString());
                        var targetPath = Request.PhysicalApplicationPath + strfolder;
                        System.IO.File.Copy(Path.Combine(sourcePath.Replace(fileName, ""), fileName), Path.Combine(targetPath + "cropped\\", fileName), true);
                        var mysmallImage = Models.Common.CreateImageThumbnail(targetPath + (model.TypeMasterID == (long)GalleryListingTypeAlias.PhotoGallery || model.TypeMasterID == (long)GalleryListingTypeAlias.PhotoGalleryImages ? "cropped\\" : "images\\") + fileName, 100);
                        mysmallImage.Save(targetPath + "thumbnails\\" + fileName,
                                       System.Drawing.Imaging.ImageFormat.Png);
                        mysmallImage.Dispose();
                        objGListing.FileURLTxt = "~/" + strfolder + "/cropped/" + fileName;
                        try
                        {
                            System.IO.File.Delete(Server.MapPath(TempData["CroppedImage"].ToString()));
                        }
                        catch
                        {

                        }
                        TempData["CroppedImage"] = null;
                        #endregion

                    }
                    if (model.ListingID > 0)
                    {
                        TempData["AlertMessage"] = _service.GetGalleryListingType(model.TypeMasterID.Value) + " details updated successfully.";
                        objContext.SaveChanges();
                    }
                    else
                    {
                        objContext.GalleryListings.Add(objGListing);
                        objContext.SaveChanges();
                        TempData["AlertMessage"] = _service.GetGalleryListingType(model.TypeMasterID.Value) + " details saved successfully.";
                    }

                    #region System Change Log
                    SystemChangeLog objSCL = new SystemChangeLog();
                    long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                    objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                    objSCL.UsernameTxt = objuser.UserNameTxt;
                    objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                    objSCL.ModuleTxt = "Gallery Listing";
                    objSCL.LogTypeTxt = model.ListingID > 0 ? "Update" : "Add";
                    objSCL.NotesTxt = (_service.GetGalleryListingType(model.TypeMasterID.Value)) + " Details" + (model.ListingID > 0 ? " updated for " : "  added for ") + model.TitleTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();
                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var newResult = (from x in objContext.GalleryListings
                                     where x.ListingID == model.ListingID
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

                    return RedirectToAction("SubGalleryListing", "GalleryListing", rvd);
                }
                else
                {
                    return RedirectToAction("SubGalleryListing", "GalleryListing", rvd);
                }
            }
            catch (Exception ex)
            {
                return View(model);
            }
        }

        /// <summary>
        /// This method is used to delete the gallery Listing from database.It also chacks wheather gallery Listing is in use or not.
        /// If gallery Listing is in use then it return to view and show message "gallery Listing is in use, cannot be deleted." else it delte the image and return to thye view.
        /// </summary>
        /// <param name="glt">This parameter is used to get the Type master id</param>
        /// <param name="glid">This parameter is used to get the listing id</param>
        /// <param name="pid">This parameter is used to get the listing parent id</param>
        /// <param name="fm">this parameter is used to get the form control values from view </param>
        /// <param name="returnUrl"></param>
        /// <returns>This method return the Json result (url) that will be passed to the Ajax post method on client side.</returns>
        [HttpPost]
        public JsonResult DeleteSubGallery(string glt, string glid, string pid, FormCollection fm)
        {
            //decrypt image type id(it)
            glt = !string.IsNullOrEmpty(Convert.ToString(glt)) ? EncryptDecrypt.Decrypt(glt) : "0";
            //decrypt image id(iid)
            glid = !string.IsNullOrEmpty(Convert.ToString(glid)) ? EncryptDecrypt.Decrypt(glid) : "0";
            pid = !string.IsNullOrEmpty(Convert.ToString(pid)) ? EncryptDecrypt.Decrypt(pid) : "0";
            //.. Code for get the route value directory
            RouteValueDictionary rvd = new RouteValueDictionary();
            ViewBag.ListingTypeId = glt;
            ViewBag.Title = _service.GetGalleryListingType(Convert.ToInt32(glt));
            var page = Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString();
            var pagesize = Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString();
            rvd.Add("pagesize", pagesize);
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "ListingCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
            rvd.Add("gpage", Request.QueryString["gpage"] != null ? Request.QueryString["gpage"].ToString() : Models.Common._currentPage.ToString());
            rvd.Add("gpagesize", Request.QueryString["gpagesize"] != null ? Request.QueryString["gpagesize"].ToString() : Models.Common._pageSize.ToString());
            rvd.Add("gColumn", Request.QueryString["gColumn"] != null ? Request.QueryString["gColumn"].ToString() : "CreateDate");
            rvd.Add("gDirection", Request.QueryString["gDirection"] != null ? Request.QueryString["gDirection"].ToString() : "Descending");
            rvd.Add("glt", EncryptDecrypt.Encrypt(glt));
            rvd.Add("pid", EncryptDecrypt.Encrypt(pid));
            TempData["pager"] = pagesize;
            Session["Edit/Delete"] = "Delete";
            try
            {
                // TODO: Add delete logic here
                //.. Check for listing is in use

                GalleryListing objListing = objContext.GalleryListings.Find(Convert.ToInt32(glid));
                long ListingID = Convert.ToInt64(glid);
                long ParentID = Convert.ToInt64(pid);
                #region System Change Log
                var oldresult = (from a in objContext.GalleryListings
                                 where a.ListingID == ListingID
                                 select a).ToList();
                DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                #endregion

                var objGlisting = objContext.GalleryListings.Where(x => x.ParentID == ListingID).ToList();
                if (objGlisting.Count > 0)
                {
                    TempData["Message"] = "Listing is in use, cannot be deleted.";
                    rvd.Add("page", page);
                    return Json(Url.Action("SubGalleryListing", "GalleryListing", rvd));
                }
                if (objListing != null)
                {
                    //****************Display Order ************************
                    var objGalleryListing = objContext.GalleryListings.Where(x => x.TypeMasterID == objListing.TypeMasterID && x.ParentID.Value == ParentID && x.IsDeletedInd == false).FirstOrDefault();
                    if (objGalleryListing != null)
                    {
                        try
                        {
                            var objGalleryListingService = new GalleryListingService();
                            objGalleryListingService.ChangeDeletedDisplayOrder(objListing.DisplayOrderNbr.Value, objListing.ListingID, objListing.TypeMasterID.Value, ParentID);
                        }
                        catch { }
                    }
                    //***************************************************
                    #region System Change Log
                    SystemChangeLog objSCL = new SystemChangeLog();
                    long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                    objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                    objSCL.UsernameTxt = objuser.UserNameTxt;
                    objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                    objSCL.ModuleTxt = "Gallery Listing";
                    objSCL.LogTypeTxt = "Delete";
                    objSCL.NotesTxt = (_service.GetGalleryListingType(objListing.TypeMasterID.Value)) + " Details deleted for " + objListing.TitleTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();
                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var newResult = (from x in objContext.GalleryListings
                                     where x.ListingID == ListingID
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
                        Models.Common.DeleteImage(Server.MapPath(objListing.FileURLTxt.Replace("cropped", "images")));
                        Models.Common.DeleteImage(Server.MapPath(objListing.FileURLTxt));
                        Models.Common.DeleteImage(Server.MapPath(objListing.FileURLTxt.Replace("cropped", "Thumbnails")));
                        Models.Common.DeleteImage(Server.MapPath(objListing.FileURLTxt.Replace("cropped", "Thumbnails_Small")));
                    }
                    catch
                    {
                    }
                    TempData["AlertMessage"] = _service.GetGalleryListingType(Convert.ToInt64(glt)) + " details deleted successfully.";
                }
                //.. Checks for no of records in current page if exists records then return same page number else decrease the page number
                int? CheckPage = 1;
                int ListingTypeID = Convert.ToInt32(glt);
                var count = objContext.GalleryListings.Where(x => x.TypeMasterID == ListingTypeID && x.ParentID == ParentID && x.IsDeletedInd == false).Count();
                if (Convert.ToInt32(page) > 1)
                    CheckPage = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
                rvd.Add("page", CheckPage);
                return Json(Url.Action("SubGalleryListing", "GalleryListing", rvd));
            }
            catch (Exception ex)
            {
                rvd.Add("page", page);
                return Json(Url.Action("SubGalleryListing", "GalleryListing", rvd));
            }
        }

        #endregion

        #region Private Methods

        //<summary>
        //This method is used to bind the status data
        //</summary>
        //<returns>List<SelectListItem></returns>
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
        //<summary>
        //This method is used to bind the Show on home page status data
        //</summary>
        //<returns>List<SelectListItem></returns>
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
        [HttpPost]
        public JsonResult CheckURL(string url)
        {
            var objContext = new db_KISDEntities();
            var count = 0;

            //count = objContext.Events.Where(x => x.URLTxt.Contains(url)).Count();
            //count = objContext.News.Where(x => x.URLTxt.Contains(url)).Count();
            count += objContext.GalleryListings.Where(x => x.URLTxt.Contains(url)).Count();
            if (count > 0)
            {
                if (url != "")
                    url = url + count;
            }
            return Json(url);
        }
        #endregion
    }
}