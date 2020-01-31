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
using ImageTypeAlias = KISD.Areas.Admin.Models.ImageService.ImageType;
using System.Web.Security;
using System.Data;
using static KISD.Areas.Admin.Models.Common;

namespace KISD.Areas.Admin.Controllers
{
    public class ImageController : Controller
    {
        private ImageService _service;
        db_KISDEntities objContext = new db_KISDEntities();
        /// <summary>
        /// Code to create instance Image Service class in constructor
        /// </summary>
        public ImageController()
        {
            _service = new ImageService();
        }

        [Authorize]
        /// <summary>
        /// this method will show the image listing with all image type.
        /// </summary>
        /// <param name="Page">this parameter is used to get page number to be shown.</param>
        /// <param name="PageSize">this parameter is used to get no of recorde to be shown.</param>
        /// <param name="gridSortOptions">this parameter is used to get grid sorting option.</param>
        /// <param name="it">this parameter is used to get type id of the image i.e. 1,2 or 3</param>
        /// <param name="CategoryId">show the category id in case of image type=3 i.e Photo Gallery</param>
        /// <param name="formCollection">this parameter is used to get controls collection on the page.</param>
        /// <param name="ObjResult"></param>
        /// <returns>view to enter image details.</returns>
        public ActionResult Index(int? Page, int? PageSize, GridSortOptions gridSortOptions, string it, int? CategoryId, FormCollection formCollection, string ObjResult)
        {
            var db_obj = new db_KISDEntities();
            //Check for valid ImageTypeID
            if (it == null)
            {
                return RedirectToAction("Index", "Home");
            }

            #region Check Tab is Accessible or Not
            var userId = db_obj.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = db_obj.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Images), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            //decrypt image type id(it)
            if (!string.IsNullOrEmpty(Convert.ToString(it)))
            {
                it = Convert.ToString(EncryptDecrypt.Decrypt(it));
            }
            TempData["CroppedImage"] = null;
            var ImageType = it != null ? Convert.ToInt32(it) : Convert.ToInt32(ImageTypeAlias.BannerImage);
            ViewBag.ImageTypeId = ImageType;

            //*******************Fill Values if Display order contains null values***************************
            var displayOrderList = objContext.Images.Where(x => x.ImageTypeID == ImageType).ToList();
            foreach (var item in displayOrderList)
            {
                if (string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()))
                {
                    var objContentData = objContext.Images.Where(x => x.ImageID == item.ImageID).FirstOrDefault();
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
                    var ImgID = Convert.ToInt64(objAjaxRequest.hfid);
                    var images = objContext.Images.Find(ImgID);
                    if (images != null)
                    {
                        #region System Change Log
                        var oldresult = (from a in objContext.Images
                                         where a.ImageID == ImgID
                                         select a).ToList();
                        DataTable dtOld = Models.Common.LINQResultToDataTable(oldresult);
                        #endregion

                        CategoryId = Convert.ToInt32(Request.QueryString["CategoryID"]);
                        images.StatusInd = objAjaxRequest.hfvalue == "1";
                        var content = objContext.Contents.Where(x => x.BannerImageID == ImgID).ToList();
                        if (content.Count > 0 && !images.StatusInd)
                        {
                            TempData["Message"] = "Image is in use, cannot be set as Inactive.";
                        }
                        else
                        {
                            int ActiveBanners = objContext.Images.Where(x => x.StatusInd == true && x.ImageTypeID == ImageType && x.IsDeletedInd == false).Count();
                            if (ActiveBanners == 1 && objAjaxRequest.hfvalue == "0" && ImageType == Convert.ToInt32(ImageTypeAlias.BannerImage))
                            {
                                TempData["AlertMessage"] = "At-least one image must remain active to show for Home Page slider.";
                            }
                            else if (objAjaxRequest.qs_Type == "displayorder")
                            {
                                if (ImageService.ChangeImageDisplayOrder(images.DisplayOrderNbr.Value, Convert.ToInt64(objAjaxRequest.qs_value), images.ImageID, Convert.ToInt32(images.ImageTypeID)))
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
                                objSCL.ModuleTxt = "Images";
                                objSCL.LogTypeTxt = images.ImageID > 0 ? "Update" : "Add";
                                objSCL.NotesTxt = "Image Details" + (images.ImageID > 0 ? " updated for " : "  added for ") + images.TitleTxt;
                                objSCL.LogDateTime = DateTime.Now;
                                objContext.SystemChangeLogs.Add(objSCL);
                                objContext.SaveChanges();

                                objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                                var newResult = (from x in objContext.Images
                                                 where x.ImageID == images.ImageID
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

            ViewBag.Title = ViewBag.PageTitle = _service.GetImageType(ImageType) + (ImageType != 4 ? "s" : " Listing");

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
                gridSortOptions.Column = "ImageCreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column == "TitleTxt" || gridSortOptions.Column == "CreateDate"
                || gridSortOptions.Column == "ImageCreateDate" || gridSortOptions.Column == "DisplayOrderNbr")
            {

            }
            else
            {
                gridSortOptions.Column = "ImageCreateDate";
            }
            //.. Code for get records as page view model
            var pagesize = PageSize.HasValue ? PageSize.Value : Models.Common._pageSize;
            var page = Page.HasValue ? Page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;
            long imgType = Convert.ToInt64(it);
            var pagedViewModel = new PagedViewModel<ImageModel>
            {
                ViewData = ViewData,
                Query = _service.GetImages(imgType).AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "ImageCreateDate",
                Page = page,
                PageSize = pagesize,
            }.Setup();
            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("ImagePartial", pagedViewModel);// ("partial view name ")
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
        /// <param name="it">image type id</param>
        /// <param name="iid"> image id</param>
        /// <param name="CategoryID"></param>
        /// <returns></returns>
        public ActionResult Create(string it, string iid, int? CategoryID)
        {
            #region Check Tab is Accessible or Not
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Images), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            var objImageModel = new ImageModel();
            //Check for valid ImageTypeID
            ViewBag.ImageTypeId = it;
            ViewBag.Title = "";
            if ((Request.QueryString["it"] == null) && (Request.QueryString["iid"] == null)
                )
            {
                return RedirectToAction("Index", "Home");
            }
            //decrypt image type id(it)
            it = !string.IsNullOrEmpty(Convert.ToString(it)) ? EncryptDecrypt.Decrypt(it) : "0";

            //decrypt image id(iid)
            iid = !string.IsNullOrEmpty(Convert.ToString(iid)) ? EncryptDecrypt.Decrypt(iid) : "0";
            int ImageID = Convert.ToInt32(iid);
            if (ImageID > 0 && objContext.Images.Where(x => x.ImageID == ImageID && x.IsDeletedInd == true).Any())
            {
                return RedirectToAction("Index", "Home");
            }
            Session["Edit/Delete"] = "Edit";
            ViewBag.PageTitle = (iid == "0" ? "Add " : "Edit ") + (_service.GetImageType(Convert.ToInt32(it))) + " Details";
            ViewBag.Submit = (iid == "0" ? "Save" : "Update");
            ViewBag.ImageTypeId = it;
            ViewBag.Title = (iid == "0" ? "Add " : "Edit ") + (_service.GetImageType(Convert.ToInt32(it)));
            ViewBag.ImageTypeTitle = _service.GetImageType(Convert.ToInt32(it)) + (Convert.ToInt32(it) != 4 ? "s" : " Listing");
            objImageModel.ImageCreateDate = DateTime.Now;
           
            ViewBag.Date = DateTime.Now.ToShortDateString();
            ViewBag.StartDateStr = "";
            ViewBag.EndDateStr = "";
            objImageModel.ImageTypeID = Convert.ToInt32(it);
            if (Convert.ToInt32(iid) > 0)
            {
                var image = (from u in objContext.Images
                             where u.ImageID == ImageID
                             select u).FirstOrDefault();
                if (image != null)
                {
                    objImageModel.ImageID = image.ImageID;
                    objImageModel.TitleTxt = image.TitleTxt;
                    objImageModel.URLTxt = image.URLTxt;
                    objImageModel.TargetWindowInd = image.TargetWindowInd;
                    objImageModel.DisplayStartDate = image.DisplayStartDate;
                    objImageModel.DisplayEndDate = image.DisplayEndDate;
                    objImageModel.AbstractTxt = image.AbstractTxt;
                    objImageModel.AltImageTxt = image.AltImageTxt;
                    objImageModel.ImgPathTxt = image.ImgPathTxt;
                    objImageModel.ImageCreateDate = Convert.ToDateTime(image.ImageCreateDate.Value.ToShortDateString());
                    objImageModel.ImageTypeID = image.ImageTypeID;
                    objImageModel.StatusInd = image.StatusInd;
                    objImageModel.CreateByID = image.CreateByID;
                    objImageModel.CreateDate = image.CreateDate;
                    objImageModel.LastModifyByID = image.LastModifyByID;
                    objImageModel.LastModifyDate = image.LastModifyDate;
                    ViewBag.StartDateStr = objImageModel.DisplayStartDate.HasValue ? objImageModel.DisplayStartDate.Value.ToString("MM/dd/yyyy hh:mm tt") : null;
                    ViewBag.EndDateStr = objImageModel.DisplayEndDate.HasValue ? objImageModel.DisplayEndDate.Value.ToString("MM/dd/yyyy hh:mm tt") : null;
                    ViewBag.StatusInd = GetStatusData(objImageModel.StatusInd ? "1" : "0");
                    ViewBag.Date = objImageModel.ImageCreateDate.Value.ToShortDateString();
                }
            }
            else
            {
                ViewBag.StatusInd = GetStatusData(string.Empty);
            }

            return View(objImageModel);
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
        public ActionResult Create(string command, FormCollection fm, ImageModel model, int? CategoryID)
        {
            try
            {
                var width = 1600;
                HttpPostedFileBase file = Request.Files.Count > 0 ? Request.Files[0] : null;
                ViewBag.Title = (model.ImageID == 0 ? "Add " : "Edit ") + _service.GetImageType(model.ImageTypeID);
                ViewBag.StatusInd = GetStatusData(string.Empty);
                ViewBag.Submit = (model.ImageID == 0 ? "Save" : "Update");
                ViewBag.ImageTypeId = model.ImageTypeID;
                ViewBag.Date = model.ImageCreateDate.Value.ToShortDateString();
                ViewBag.StartDateStr = model.DisplayStartDate != null ? Convert.ToString(model.DisplayStartDate) : "";
                ViewBag.EndDateStr = model.DisplayEndDate != null ? Convert.ToString(model.DisplayEndDate) : "";
                var rvd = new RouteValueDictionary();
                rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
                rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
                rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "ImageCreateDate");
                rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
                rvd.Add("it", EncryptDecrypt.Encrypt(Convert.ToString(model.ImageTypeID)));
                ViewBag.PageTitle = (model.ImageID == 0 ? "Add " : "Edit ") + _service.GetImageType(model.ImageTypeID) + " Details";
                ViewBag.ImageTypeTitle = _service.GetImageType(model.ImageTypeID) + (model.ImageTypeID != 4 ? "s" : " Listing");

                #region System Change Log
                DataTable dtOld;
                var oldresult = (from a in objContext.Images
                                 where a.ImageID == model.ImageID
                                 select a).ToList();
                dtOld = Models.Common.LINQResultToDataTable(oldresult);
                #endregion
                if (string.IsNullOrEmpty(command))
                {
                    Image objImage;
                    if (model.ImageID > 0)
                    {
                        objImage = objContext.Images.Find(model.ImageID);
                    }
                    else
                    {
                        objImage = new Image();
                        objImage.IsDeletedInd = false;
                    }
                    ViewBag.StatusInd = GetStatusData(objImage.StatusInd ? "1" : "0");
                    objImage.ImageTypeID = model.ImageTypeID;
                    objImage.StatusInd = fm["StatusInd"].ToString() == "1";

                    if (!string.IsNullOrEmpty(model.TitleTxt))
                    {
                        var chkimage = objContext.Images.Where(x => x.TitleTxt.Trim() == model.TitleTxt.Trim()
                                                                    && x.ImageTypeID == model.ImageTypeID
                                                                    && x.ImageID != model.ImageID
                                                                    && ( x.IsDeletedInd == false || x.IsDeletedInd==null)).Any();

                        if (chkimage)//check image title on adding new image details or updating existing 1
                        {
                            TempData["CroppedImage"] = null;
                            ModelState.AddModelError("TitleTxt", model.TitleTxt + " title already exists.");
                            return View(model);
                        }

                        var content = objContext.Contents.Where(x => x.BannerImageID == model.ImageID).ToList();
                        if (content.Count > 0 && !objImage.StatusInd)
                        {
                            TempData["CroppedImage"] = null;
                            ViewBag.Date = model.ImageCreateDate.Value.ToShortDateString().Trim();
                            TempData["Message"] = "Image is in use, cannot be set as Inactive.";
                            return View(model);
                        }
                        int ActiveBanners = objContext.Images.Where(x => x.StatusInd == true && x.ImageTypeID == model.ImageTypeID && x.IsDeletedInd == false).Count();
                        var PrevStatus = objContext.Images.Where(x => x.ImageID == model.ImageID && x.ImageTypeID == model.ImageTypeID).Select(x => x.StatusInd).FirstOrDefault();
                        if (ActiveBanners == 1 && objImage.StatusInd == false && model.ImageTypeID == Convert.ToInt32(ImageTypeAlias.BannerImage) && PrevStatus == true)
                        {
                            TempData["Message"] = "At-least one image must remain active to show for Home Page slider.";
                            return View(model);
                        }
                    }
                    objImage.TitleTxt = model.TitleTxt ?? "";
                    objImage.AbstractTxt = !string.IsNullOrEmpty(model.AbstractTxt) ? model.AbstractTxt : string.Empty;
                    objImage.AltImageTxt = !string.IsNullOrEmpty(model.AltImageTxt) ? model.AltImageTxt.Replace(">", "").Replace("<", "") : string.Empty;
                    objImage.ImageCreateDate = model.ImageCreateDate;

                    objImage.URLTxt = model.URLTxt;
                    objImage.TargetWindowInd = model.TargetWindowInd;
                    objImage.DisplayStartDate = model.DisplayStartDate != null ? model.DisplayStartDate : null;
                    objImage.DisplayEndDate = model.DisplayEndDate != null ? model.DisplayEndDate : null;

                    objImage.CreateDate = model.ImageID > 0 ? objImage.CreateDate : DateTime.Now; ;
                    objImage.CreateByID = model.ImageID > 0 ? objImage.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    objImage.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    objImage.LastModifyDate = DateTime.Now;
                    if (TempData["CroppedImage"] == null)
                    {
                        #region  Image
                        if (file != null && file.ContentLength > 0)
                        {
                            try
                            {

                            }
                            catch
                            {

                            }
                            var fileName = Path.GetFileName(file.FileName);
                            #region Upload Image
                            Models.Common.CreateFolder();
                            //.. Get extension of the document
                            var fileExtension = fileName.Substring(fileName.LastIndexOf("."), fileName.Length - fileName.LastIndexOf("."));
                            //.. Set fullname of the document path

                            var MyGuid = Guid.NewGuid();
                            fileName = MyGuid.ToString() + fileExtension;
                            //.. Create path of the document to save  into the defined physical path.
                            var strPath = Request.PhysicalApplicationPath + "WebData\\images\\" + fileName;
                            file.SaveAs(strPath);
                            objImage.ImgPathTxt = "~/WebData/images/" + fileName;
                            var myImage = Models.Common.CreateImageThumbnail(strPath, width);
                            myImage.Save(Request.PhysicalApplicationPath + "WebData\\thumbnails\\" + fileName,
                                           fileExtension.ToLower() == ".png" ?
                                           System.Drawing.Imaging.ImageFormat.Png :
                                           fileExtension.ToLower() == ".gif" ?
                                           System.Drawing.Imaging.ImageFormat.Gif :
                                           System.Drawing.Imaging.ImageFormat.Jpeg
                                           );
                            myImage.Dispose();
                            var mysmallImage = Models.Common.CreateImageThumbnail(strPath, 100);
                            mysmallImage.Save(Request.PhysicalApplicationPath + "WebData\\thumbnails_Small\\" + fileName,
                                           fileExtension.ToLower() == ".png" ?
                                           System.Drawing.Imaging.ImageFormat.Png :
                                           fileExtension.ToLower() == ".gif" ?
                                           System.Drawing.Imaging.ImageFormat.Gif :
                                           System.Drawing.Imaging.ImageFormat.Jpeg
                                           );
                            mysmallImage.Dispose();
                            #endregion
                        }

                        #endregion
                    }
                    else
                    {
                        #region Cropped Image
                        try
                        {
                            //Models.Common.DeleteImage(Server.MapPath(objImage.ImgPathTxt));
                            //Models.Common.DeleteImage(Server.MapPath(objImage.ImgPathTxt).Replace("images", "thumbnails"));
                            //Models.Common.DeleteImage(Server.MapPath(objImage.ImgPathTxt).Replace("images", "thumbnails_Small"));
                        }
                        catch { }

                        var croppedfile = new FileInfo(Server.MapPath(TempData["CroppedImage"].ToString()));
                        var fileName = croppedfile.Name;
                        croppedfile = null;
                        var sourcePath = Server.MapPath(TempData["CroppedImage"].ToString());
                        var targetPath = Request.PhysicalApplicationPath + "WebData\\";
                        System.IO.File.Copy(Path.Combine(sourcePath.Replace(fileName, ""), fileName), Path.Combine(targetPath + "images\\", fileName), true);
                        System.IO.File.Copy(Path.Combine(sourcePath.Replace(fileName, ""), fileName), Path.Combine(targetPath + "thumbnails\\", fileName), true);

                        var mysmallImage = Models.Common.CreateImageThumbnail(targetPath + "images\\" + fileName, 100);
                        mysmallImage.Save(targetPath + "thumbnails_Small\\" + fileName,
                                       System.Drawing.Imaging.ImageFormat.Png);

                        mysmallImage.Dispose();

                        objImage.ImgPathTxt = "~/WebData/images/" + fileName;
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

                    if (model.ImageID > 0)
                    {
                        TempData["AlertMessage"] = _service.GetImageType(model.ImageTypeID) + " details updated successfully.";
                        objContext.SaveChanges();
                    }
                    else
                    {
                        objContext.Images.Add(objImage);
                        objContext.SaveChanges();
                        TempData["AlertMessage"] = _service.GetImageType(model.ImageTypeID) + " details saved successfully.";
                    }

                    #region System Change Log
                    SystemChangeLog objSCL = new SystemChangeLog();
                    long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                    objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                    objSCL.UsernameTxt = objuser.UserNameTxt;
                    objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                    objSCL.ModuleTxt = "Images";
                    objSCL.LogTypeTxt = model.ImageID > 0 ? "Update" : "Add";
                    objSCL.NotesTxt = (_service.GetImageType(model.ImageTypeID)) + " Details" + (model.ImageID > 0 ? " updated for " : "  added for ") + model.TitleTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();
                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var newResult = (from x in objContext.Images
                                     where x.ImageID == objImage.ImageID
                                     select x);
                    DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                    foreach (DataColumn col in dtNew.Columns)
                    {
                        if (model.ImageID > 0)
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
                    return RedirectToAction("Index", "Image", rvd);
                }
                else
                {
                    return RedirectToAction("Index", "Image", rvd);
                }
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = "Some error occured. Please try again after some time.";
                return View(model);
            }
        }
        /// <summary>
        /// This method is used to delete the image from database.It also chacks wheather image is in use or not.
        /// If image is in use then it return to view and show message "image is in use, cannot be deleted." else it delte the image and return to thye view.
        /// </summary>
        /// <param name="ImageTypeID">This parameter is used to get the imagetypeid</param>
        /// <param name="ImageID">This parameter is used to get the imageid</param>
        /// <param name="fm">this parameter is used to get the form control values from view </param>
        /// <param name="returnUrl"></param>
        /// <returns>This method return the Json result (url) that will be passed to the Ajax post method on client side.</returns>
        [HttpPost]
        public JsonResult Delete(string it, string iid, int? CategoryID, FormCollection fm)
        {
            //decrypt image type id(it)
            it = !string.IsNullOrEmpty(Convert.ToString(it)) ? EncryptDecrypt.Decrypt(it) : "0";
            //decrypt image id(iid)
            iid = !string.IsNullOrEmpty(Convert.ToString(iid)) ? EncryptDecrypt.Decrypt(iid) : "0";

            //.. Code for get the route value directory
            RouteValueDictionary rvd = new RouteValueDictionary();
            ViewBag.ImageTypeId = it;
            ViewBag.Title = _service.GetImageType(Convert.ToInt32(it));
            var page = Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString();
            var pagesize = Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString();
            rvd.Add("pagesize", pagesize);
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "ImageCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
            rvd.Add("it", EncryptDecrypt.Encrypt(it));
            TempData["pager"] = pagesize;
            Session["Edit/Delete"] = "Delete";
            try
            {
                // TODO: Add delete logic here
                //.. Check for image in use
                Image objImage = objContext.Images.Find(Convert.ToInt32(iid));
                long ImgID = Convert.ToInt64(iid);
                #region System Change Log
                var oldresult = (from a in objContext.Images
                                 where a.ImageID == ImgID
                                 select a).ToList();
                DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                #endregion
                var bannerimagetypeid = Convert.ToInt32(ImageTypeAlias.BannerImage);
                var imagetypeid = Convert.ToInt32(it);
                var imagecount = objContext.Images.Where(x => x.ImageTypeID == bannerimagetypeid && x.IsDeletedInd == false).Count();
                var content = objContext.Contents.Where(x => x.BannerImageID == ImgID).ToList();
                if (content.Count > 0)
                {
                    TempData["Message"] = "Image is in use, cannot be deleted.";
                    rvd.Add("page", page);
                    return Json(Url.Action("Index", "Image", rvd));
                }
                if (objImage != null)
                {
                    if (imagecount == 1 && imagetypeid == bannerimagetypeid)
                    {
                        TempData["AlertMessage"] = "At-least one image must remain active to show for Home Page slider";
                    }
                    else
                    {

                        //****************Display Order ************************
                        var objImages = objContext.Images.Where(x => x.ImageTypeID == objImage.ImageTypeID).FirstOrDefault();
                        if (objImages != null)
                        {
                            try
                            {
                                var objImageService = new ImageService();
                                objImageService.ChangeDeletedDisplayOrder(objImages.DisplayOrderNbr.Value, objImage.ImageID, objImage.ImageTypeID);
                            }
                            catch { }
                        }
                        //***************************************************
                        // objContext.SaveChanges();
                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = "Images";
                        objSCL.LogTypeTxt = "Delete";
                        objSCL.NotesTxt = (_service.GetImageType(objImage.ImageTypeID)) + " Details deleted for " + objImage.TitleTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();
                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var objContextnew = new db_KISDEntities();
                        var newResult = (from x in objContextnew.Images
                                         where x.ImageID == ImgID
                                         select x);
                        DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
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
                        try
                        {
                            Models.Common.DeleteImage(Server.MapPath(objImage.ImgPathTxt));
                        }
                        catch
                        {
                        }
                        TempData["AlertMessage"] = _service.GetImageType(Convert.ToInt32(it)) + " details deleted successfully.";
                    }
                }
                //.. Checks for no of records in current page if exists records then return same page number else decrease the page number
                int? CheckPage = 1;
                int ImageTypeID = Convert.ToInt32(it);
                var count = objContext.Images.Where(x => x.ImageTypeID == ImageTypeID).Count();
                if (Convert.ToInt32(page) > 1)
                    CheckPage = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
                rvd.Add("page", CheckPage);
                return Json(Url.Action("Index", "Image", rvd));
            }
            catch (Exception ex)
            {
                rvd.Add("page", page);
                return Json(Url.Action("Index", "Image", rvd));
            }
        }
        /// <summary>
        /// Get the value of Status with selected value(active or inactive)
        /// </summary>
        /// <param name="value">who's status to be found.</param>
        /// <returns>list with status values.</returns>
        private List<SelectListItem> GetStatusData(string value)
        {
            var items = new List<SelectListItem>();
            var data = new SelectListItem();
            data.Text = "Active";
            data.Value = "1";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "InActive";
            data.Value = "0";
            items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return items;
        }

    }
}