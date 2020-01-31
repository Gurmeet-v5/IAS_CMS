using KISD.Areas.Admin.Models;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using static KISD.Areas.Admin.Models.Common;

namespace KISD.Areas.Admin.Controllers
{
    public class BoardMembersController : Controller
    {
        private BoardMembersModelService _service;
        db_KISDEntities objContext = new db_KISDEntities();
        /// <summary>
        /// Code to create instance Image Service class in constructor
        /// </summary>
        public BoardMembersController()
        {
            _service = new BoardMembersModelService();
        }

        [Authorize]
        [SessionExpire]
        public ActionResult Index(int? Page, int? PageSize, GridSortOptions gridSortOptions, FormCollection formCollection, string ObjResult)
        {
            var objContext = new db_KISDEntities();
            var currentLoggedUserId = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
            var CurrentUserRoleID = objContext.UserRoles.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault().RoleID;

            #region Check Tab is Accessible or Not
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.SchoolBoard), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            ViewBag.Title = ViewBag.PageTitle = "Board Members Listing";

            //*******************Fill Values if Display order contains null values***************************
            var displayOrderList = objContext.BoardOfMembers.Where(x => x.IsDeletedInd == false).ToList();
            foreach (var item in displayOrderList)
            {
                if (string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()))
                {
                    var objContentData = objContext.BoardOfMembers.Where(x => x.BoardMemberID == item.BoardMemberID && x.IsDeletedInd == false).FirstOrDefault();
                    var displayOrder1 = (displayOrderList.Max(x => x.DisplayOrderNbr)) == null ? 1 : displayOrderList.Max(x => x.DisplayOrderNbr).Value + 1;
                    objContentData.DisplayOrderNbr = displayOrder1;
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

                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
                }

                //Ajax Call for update status for images
                if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && ObjResult != null && !string.IsNullOrEmpty(ObjResult))
                {
                    var ListingID = Convert.ToInt64(objAjaxRequest.hfid);
                    var Listing = objContext.BoardOfMembers.Find(ListingID);
                    if (Listing != null)
                    {
                        #region System Change Log
                        var oldresult = (from a in objContext.BoardOfMembers
                                         where a.BoardMemberID == ListingID
                                         select a).ToList();

                        DataTable dtOld = Models.Common.LINQResultToDataTable(oldresult);
                        #endregion

                        if (objAjaxRequest.qs_Type == "status")
                        {
                            Listing.StatusInd = objAjaxRequest.hfvalue == "1";
                            TempData["Message"] = "Status updated successfully.";
                        }
                        else if (objAjaxRequest.qs_Type == "displayorder")
                        {
                            try
                            {
                                if (BoardMembersModelService.ChangeDisplayOrder(Convert.ToInt64(Listing.DisplayOrderNbr), Convert.ToInt64(objAjaxRequest.qs_value)))
                                {
                                    TempData["Message"] = "Display Order has been changed successfully.";
                                }
                            }
                            catch
                            {
                                TempData["Message"] = "Some Error Occured while changing Display Order, Please try again later.";
                            }
                        }
                        objContext.SaveChanges();

                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        User objuser = objContext.Users.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = "Board Member Listing";
                        objSCL.LogTypeTxt = "Update";
                        objSCL.NotesTxt = (objAjaxRequest.qs_Type == "status" ? "Status " : "Display order ") + "updated for " + Listing.NameTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();

                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.BoardOfMembers
                                         where x.BoardMemberID == Listing.BoardMemberID
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
                gridSortOptions.Column = "BOMCreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column == "NameTxt" || gridSortOptions.Column == "TitleTxt"
                || gridSortOptions.Column == "DisplayOrderNbr" || gridSortOptions.Column == "TitleTxt"
                )
            {

            }
            else
            {
                gridSortOptions.Column = "BOMCreateDate";
            }
            //.. Code for get records as page view model
            var pagesize = PageSize.HasValue ? PageSize.Value : Models.Common._pageSize;
            var page = Page.HasValue ? Page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;

            var pagedViewModel = new PagedViewModel<BoardMembersModel>
            {
                ViewData = ViewData,
                Query = _service.GetBoardMembers().AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "BOMCreateDate",
                Page = page,
                PageSize = pagesize,
            }.Setup();
            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("BoardMembersListingPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }

        [Authorize]
        [SessionExpire]
        public ActionResult Create(string bmid)
        {
            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.SchoolBoard), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            var currentLoggedUserId = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
            var CurrentUserRoleID = objContext.UserRoles.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault().RoleID;
            ViewBag.LiTitle = "Board Member Listing";
            bmid = !string.IsNullOrEmpty(Convert.ToString(bmid)) ? EncryptDecrypt.Decrypt(bmid) : "0";
            var BoardMemberId = Convert.ToInt64(bmid);
            var objModel = new BoardMembersModel();
            ViewBag.Title = ViewBag.PageTitle = (BoardMemberId > 0 ? "Edit " : "Add ") + " Board Member Details ";
            ViewBag.Submit = BoardMemberId > 0 ? "Update" : "Save";
            ViewBag.UserCreateDate = DateTime.Now.ToShortDateString();
            ViewBag.UserID = bmid;
            string isActive = "True";
            Session["Edit/Delete"] = "Edit";
            ViewBag.UserTypeID = 0;

            objModel.strBOMCreateDate = DateTime.Now.ToShortDateString();
            if (BoardMemberId > 0)
            {
                var objBOMContext = objContext.BoardOfMembers.Find(BoardMemberId);
                if (objBOMContext != null)
                {
                    objModel.BoardMemberID = objBOMContext.BoardMemberID;
                    objModel.StatusInd = objBOMContext.StatusInd.Value;
                    objModel.BOMCreateDate = objBOMContext.BOMCreateDate.HasValue ? objBOMContext.BOMCreateDate.Value : DateTime.Now;
                    objModel.ContactInfoTxt = objBOMContext.ContactInfoTxt;
                    objModel.BannerImageID = objBOMContext.BannerImageID;
                    objModel.AltBannerImageTxt = objBOMContext.AltBannerImageTxt;
                    objModel.BannerImageAbstractTxt = objBOMContext.BannerImageAbstractTxt;
                    objModel.CreateByID = objBOMContext.CreateByID;
                    objModel.CreateDate = objBOMContext.CreateDate.HasValue ? objBOMContext.CreateDate.Value : DateTime.Now;
                    objModel.DescriptionTxt = objBOMContext.DescriptionTxt;
                    objModel.DisplayOrderNbr = objBOMContext.DisplayOrderNbr;
                    objModel.ImageURLTxt = objBOMContext.ImageURLTxt;
                    objModel.IsDeletedInd = objBOMContext.IsDeletedInd;
                    objModel.LastModifyByID = objBOMContext.LastModifyByID;
                    objModel.LastModifyDate = objBOMContext.LastModifyDate.HasValue ? objBOMContext.LastModifyDate.Value : DateTime.Now;
                    objModel.NameTxt = objBOMContext.NameTxt;
                    objModel.PageMetaDescriptionTxt = objBOMContext.PageMetaDescriptionTxt;
                    objModel.PageMetaTitleTxt = objBOMContext.PageMetaTitleTxt;
                    objModel.RightSectionAbstractTxt = objBOMContext.RightSectionAbstractTxt;
                    objModel.RightSectionTitleTxt = objBOMContext.RightSectionTitleTxt;
                    objModel.TitleTxt = objBOMContext.TitleTxt;
                    objModel.TermTxt = objBOMContext.TermTxt;
                    objModel.URLTxt = objBOMContext.URLTxt;
                    objModel.strBOMCreateDate = objBOMContext.BOMCreateDate.HasValue ? objBOMContext.BOMCreateDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                    isActive = objBOMContext.StatusInd.ToString();
                }
            }
            else
            {
                objModel.StatusInd = true;
                objModel.BOMCreateDate = DateTime.Now;
            }
            ViewBag.IsActiveInd = Models.Common.GetStatusListBoolean(isActive);
            var InnerImagesTitle = Models.Common.GetInnerImages();
            ViewBag.InnerImagesTitle = InnerImagesTitle;//get all the inner image titles
            return View(objModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(BoardMembersModel model, string command, FormCollection fm)
        {
            var currentLoggedUserId = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
            var CurrentUserRoleID = objContext.UserRoles.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault().RoleID;
            ViewBag.LiTitle = "Board Member Listing";
            var EncryptBMID = EncryptDecrypt.Encrypt(model.BoardMemberID.ToString());

            var rvd = new RouteValueDictionary();
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "CreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
            rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
            rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
            ViewBag.Title = ViewBag.PageTitle = (model.BoardMemberID > 0 ? "Edit " : "Add ") + " Board Member Details ";

            ViewBag.Submit = model.BoardMemberID > 0 ? "Update" : "Save";
            ViewBag.CreateDate = DateTime.Now.ToShortDateString();
            ViewBag.UserCreateDate = DateTime.Now.ToShortDateString();
            //model.RightSections = GetAllRightSections();

            #region System Change Log
            DataTable dtOld;
            var oldresult = (from a in objContext.BoardOfMembers
                             where a.BoardMemberID == model.BoardMemberID
                             select a).ToList();
            dtOld = Models.Common.LINQResultToDataTable(oldresult);
            #endregion

            var objDBContent = new db_KISDEntities();
            var objctBM = new BoardOfMember();
            ViewBag.IsActiveInd = Models.Common.GetStatusListBoolean(!string.IsNullOrEmpty(fm["IsActiveInd"]) ? Convert.ToString(fm["IsActiveInd"]) : "0");
            ViewBag.BoardMemberID = model.BoardMemberID;
            ViewBag.isValid = "1";
            //ViewBag.RolesList = GetAllUserType(currentLoggedUserId);

            var file = Request.Files.Count > 0 ? Request.Files[0] : null;

            if (string.IsNullOrEmpty(command))
            {
                var InnerImagesTitle = Models.Common.GetInnerImages();
                ViewBag.InnerImagesTitle = InnerImagesTitle;//get all the inner image titles
                if (model.BoardMemberID > 0)
                {
                    objctBM = objDBContent.BoardOfMembers.Where(x => x.BoardMemberID == model.BoardMemberID).FirstOrDefault();
                }
                else
                {
                    objctBM = new BoardOfMember();
                }

                var count = objDBContent.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.BoardMemberID != model.BoardMemberID && x.IsDeletedInd == false).Count();
                count += objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                count += objContext.Departments.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                count += objContext.ExceptionOpportunities.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                count += objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                count += objContext.NewsEvents.Where(x => x.PageURLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                if (model.URLTxt.Trim().ToLower() == "error404")// Check for duplicate url and error404 url
                {
                    count = count + 1;
                }
                if (count > 0)
                {
                    if (model.URLTxt.ToLower().Trim() == "error404")//if user types url 'error404' below validation msg should display
                    {
                        ModelState.AddModelError("URLTxt", model.URLTxt + " URL is not allowed.");
                    }
                    else
                    {
                        ModelState.AddModelError("URLTxt", model.URLTxt + " URL already exists.");
                    }                   
                    ViewBag.isValid = "0";
                    return View(model);
                }

                if (file != null && file.ContentLength > 0)
                {
                    #region Cropped Image
                    var croppedfile = new FileInfo(Server.MapPath(TempData["CroppedImage"].ToString()));
                    var fileName = croppedfile.Name;
                    croppedfile = null;
                    var sourcePath = Server.MapPath(TempData["CroppedImage"].ToString());
                    var targetPath = Request.PhysicalApplicationPath + "WebData\\";
                    System.IO.File.Copy(Path.Combine(sourcePath.Replace(fileName, ""), fileName), Path.Combine(targetPath + "images\\", fileName), true);
                    try
                    {
                        Models.Common.DeleteImage(Server.MapPath(TempData["CroppedImage"].ToString()));
                    }
                    catch
                    {
                    }
                    objctBM.ImageURLTxt = "~/WebData/images/" + fileName;
                    var fileExtension = fileName.Substring(fileName.LastIndexOf("."), fileName.Length - fileName.LastIndexOf("."));
                    var strPath = Request.PhysicalApplicationPath + "WebData\\images\\" + fileName;
                    var myImage = CreateImageThumbnail(strPath, 100);
                    myImage.Save(Request.PhysicalApplicationPath + "WebData\\thumbnails_Small\\" + fileName,
                                   fileExtension.ToLower() == ".png" ?
                                   System.Drawing.Imaging.ImageFormat.Png :
                                   fileExtension.ToLower() == ".gif" ?
                                   System.Drawing.Imaging.ImageFormat.Gif :
                                   System.Drawing.Imaging.ImageFormat.Jpeg
                                   );
                    myImage.Dispose();

                    var myLargeImage = CreateImageThumbnail(strPath, 400);
                    myLargeImage.Save(Request.PhysicalApplicationPath + "WebData\\thumbnails\\" + fileName,
                                   fileExtension.ToLower() == ".png" ?
                                   System.Drawing.Imaging.ImageFormat.Png :
                                   fileExtension.ToLower() == ".gif" ?
                                   System.Drawing.Imaging.ImageFormat.Gif :
                                   System.Drawing.Imaging.ImageFormat.Jpeg
                                   );
                    myLargeImage.Dispose();
                    TempData["CroppedImage"] = null;
                    #endregion
                }

                objctBM.BOMCreateDate = Convert.ToDateTime(model.strBOMCreateDate + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
                objctBM.ContactInfoTxt = model.ContactInfoTxt;
                objctBM.CreateDate = DateTime.Now;
                objctBM.DescriptionTxt = model.DescriptionTxt;
                objctBM.DisplayOrderNbr = model.DisplayOrderNbr;
                objctBM.NameTxt = model.NameTxt;
                objctBM.PageMetaDescriptionTxt = model.PageMetaDescriptionTxt;
                objctBM.PageMetaTitleTxt = model.PageMetaTitleTxt;
                objctBM.RightSectionAbstractTxt = model.RightSectionAbstractTxt;
                objctBM.RightSectionTitleTxt = model.PageMetaTitleTxt;
                objctBM.BannerImageID = model.BannerImageID;
                objctBM.BannerImageAbstractTxt = model.BannerImageAbstractTxt;
                objctBM.AltBannerImageTxt = model.AltBannerImageTxt;
                objctBM.TermTxt = model.TermTxt;
                objctBM.TitleTxt = model.TitleTxt;
                objctBM.URLTxt = model.URLTxt;
                objctBM.StatusInd = Convert.ToBoolean(fm["IsActiveInd"]);
                objctBM.IsDeletedInd = false;
                objctBM.CreateDate = model.BoardMemberID > 0 ? objctBM.CreateDate : model.BOMCreateDate;
                objctBM.CreateByID = model.BoardMemberID > 0 ? objctBM.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                objctBM.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                objctBM.LastModifyDate = DateTime.Now;

                try
                {
                    if (model.BoardMemberID == 0)
                    {
                        objDBContent.BoardOfMembers.Add(objctBM);
                    }
                    objDBContent.SaveChanges();
                    var newID = objctBM.BoardMemberID;

                    #region System Change Log
                    SystemChangeLog objSCL = new SystemChangeLog();
                    long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                    objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                    objSCL.UsernameTxt = objuser.UserNameTxt;
                    objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                    objSCL.ModuleTxt = "Board Members";
                    objSCL.LogTypeTxt = model.BoardMemberID > 0 ? "Update" : "Add";
                    objSCL.NotesTxt = "Board Member Details" + (model.BoardMemberID > 0 ? " updated for " : "  added for ") + objctBM.TitleTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();

                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var newResult = (from x in objContext.BoardOfMembers
                                     where x.BoardMemberID == newID
                                     select x);
                    DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                    foreach (DataColumn col in dtNew.Columns)
                    {
                        if (model.BoardMemberID > 0)
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

                    TempData["Message"] = "Board Member details " + ((model.BoardMemberID == 0) ? "saved" : "updated") + " successfully.";
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Some error occured. Please try after some time.";
                }
            }
            return RedirectToAction("Index", "BoardMembers", rvd);
        }

        /// <summary>
        /// Set the status of the User as deleted. Donot delete from the table.
        /// </summary>
        /// <param name="bmid"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(string bmid)
        {
            bmid = !string.IsNullOrEmpty(Convert.ToString(bmid)) ? EncryptDecrypt.Decrypt(bmid) : "0";
            long BOMID = Convert.ToInt32(bmid);
            var rvd = new RouteValueDictionary();
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "BOMCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            Session["Edit/Delete"] = "Delete";
            if (BOMID > 0)
            {
                try
                {
                    using (var objContext = new db_KISDEntities())
                    {
                        #region System Change Log
                        var oldresult = (from a in objContext.BoardOfMembers
                                         where a.BoardMemberID == BOMID
                                         select a).ToList();
                        DataTable dtOld = Models.Common.LINQResultToDataTable(oldresult);
                        #endregion

                        //****************Display Order ************************
                        var objData = objContext.BoardOfMembers.Where(x => x.BoardMemberID == BOMID).FirstOrDefault();
                        if (objData != null)
                        {
                            try
                            {
                                var objModelService = new BoardMembersModelService();
                                var parentid = Request.QueryString["ParentId"] ?? "0";
                                objModelService.ChangeDeletedDisplayOrder(objData.DisplayOrderNbr.Value, objData.BoardMemberID);
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
                        objSCL.ModuleTxt = "Board Members";
                        objSCL.LogTypeTxt = "Delete";
                        objSCL.NotesTxt = "Board Members Details deleted for " + objData.NameTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();
                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.RightSections
                                         where x.RightSectionID == BOMID
                                         select x);
                        DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                        foreach (DataColumn col in dtNew.Columns)
                        {
                            try
                            {
                                SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                                objSCLD.ChangeLogID = objSCL.ChangeLogID;
                                objSCLD.FieldNameTxt = col.ColumnName.ToString();
                                objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                                objSCLD.NewValueTxt = col.ColumnName == "IsDeletedInd" ? dtNew.Rows[0][col.ColumnName].ToString() : "";
                                objContext.SystemChangeLogDetails.Add(objSCLD);
                                objContext.SaveChanges();
                            }
                            catch { }
                        }
                        #endregion
                        try
                        {
                            Models.Common.DeleteImage(Server.MapPath(objData.ImageURLTxt));
                        }
                        catch
                        {
                        }
                        TempData["Message"] = " Board Member details deleted successfully.";
                    }
                }
                catch (Exception e)
                {
                    TempData["Message"] = "Some error occured while deleting the Member, Please try again later.";
                }
            }
            int? Page = 1;
            var count = 1;
            using (var objContext = new db_KISDEntities())
            {
                count = objContext.Users.Where(x => x.IsDeletedInd == false).Count();
            }
            var page = Request.QueryString["page"] ?? "1";
            var pagesize = Request.QueryString["pagesize"] ?? "10";
            if (Convert.ToInt32(page) > 1)
                Page = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
            rvd.Add("page", Page);
            return Json(Url.Action("Index", "BoardMembers", rvd));
        }

        //private SelectList GetAllRightSections()
        //{
        //    var objContext = new db_KISDEntities();
        //    var list = objContext.RightSections.Where(x => x.StatusInd == true).OrderBy(x => x.TitleTxt).Select(x => new RightSectionModel()
        //    {
        //        RightSectionID = x.RightSectionID,
        //        TitleTxt = x.TitleTxt
        //    }).ToList();
        //    var obj = new RightSectionModel();
        //    obj.TitleTxt = "--- Select Right Section ---";
        //    obj.RightSectionID = 0;
        //    list.Insert(0, obj);
        //    var objselectlist = new SelectList(list, "RightSectionID", "TitleTxt");
        //    return objselectlist;
        //}

        private JsonResult UploadImage()
        {
            var retunedFilename = "";
            try
            {
                Models.Common.CreateFolder();
                foreach (string file in Request.Files)
                {
                    HttpPostedFileBase fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + "-" + Path.GetFileName(fileContent.FileName).Substring(fileContent.FileName.LastIndexOf("."), fileContent.FileName.Length - fileContent.FileName.LastIndexOf("."));
                        var path = Request.PhysicalApplicationPath + "WebData\\Images\\" + fileName;
                        fileContent.SaveAs(path);
                        retunedFilename = fileName;
                    }
                }
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }
            return Json(retunedFilename);
        }

        [HttpPost]
        public JsonResult CheckURL(string url)
        {
            var objContext = new db_KISDEntities();
            var count = 0;
            count += objContext.BoardOfMembers.Where(x => x.URLTxt.Contains(url)).Count();
            if (count > 0)
            {
                if (url != "")
                    url = url + count;
            }
            return Json(url);
        }
    }
}