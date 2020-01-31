using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using KISD.Areas.Admin.Models;
using ContentTypeAlias = KISD.Areas.Admin.Models.ContentType;
using ModuleTypeAlias = KISD.Areas.Admin.Models.Common.ModuleType;
using System.Data;
using System.Web.Security;
using static KISD.Areas.Admin.Models.Common;

namespace KISD.Areas.Admin.Controllers
{
    public class ContentController : Controller
    {
        /// <summary>
        /// Get data for content page
        /// </summary>
        /// <param name="ct">Content Type</param>
        /// <returns></returns>
        /// 
        [Authorize]
        [SessionExpire]
        public ActionResult Create(string ct)
        {
            TempData["CroppedImage"] = null;
            using (var objContext = new db_KISDEntities())
            {
                //decrypt content type id(ct)
                ct = !string.IsNullOrEmpty(Convert.ToString(ct)) ? EncryptDecrypt.Decrypt(ct) : "0";
                int ContentTypeID = !string.IsNullOrEmpty(ct) ? Convert.ToInt32(ct) : 0;

                #region Check Tab is Accessible or Not
                int TabType = 0;
                if (ContentTypeID == 1 || ContentTypeID == 2 || ContentTypeID == 3) { TabType = Convert.ToInt32(ModuleTypeAlias.Home); }
                if (ContentTypeID == 4) { TabType = Convert.ToInt32(ModuleTypeAlias.AboutKISD); }
                if (ContentTypeID == 5) { TabType = Convert.ToInt32(ModuleTypeAlias.School); }
                if (ContentTypeID == 6) { TabType = Convert.ToInt32(ModuleTypeAlias.NewToKISD); }
                if (ContentTypeID == 7 || ContentTypeID == 35) { TabType = Convert.ToInt32(ModuleTypeAlias.ParentStudents); }
                if (ContentTypeID == 8) { TabType = Convert.ToInt32(ModuleTypeAlias.Departments); }
                if (ContentTypeID == 9 || ContentTypeID == 34) { TabType = Convert.ToInt32(ModuleTypeAlias.SchoolBoard); }
                if (ContentTypeID == 10) { TabType = Convert.ToInt32(ModuleTypeAlias.Employment); }
                if (ContentTypeID == 11) { TabType = Convert.ToInt32(ModuleTypeAlias.GoogleAnalytic); }

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

                ViewBag.ContentTypeID = ContentTypeID;
                ViewBag.Date = DateTime.Now.ToShortDateString();
                var chkcontenttype = objContext.ContentTypes.Any(x => x.ContentTypeID == ContentTypeID && ContentTypeID <= 43);
                if (!chkcontenttype)
                {
                    return RedirectToAction("Index", "Home");
                }

                var objContentModel = new ContentModel();
                objContentModel.ContentTypeID = ContentTypeID;
                var objContent = objContext.Contents.Where(x => x.ContentTypeID == ContentTypeID).FirstOrDefault();
                var contenttypetitle = objContext.ContentTypes.Find(ContentTypeID).ContentTypeNameTxt;

                objContentModel.RightSections = GetAllRightSections();

                if (objContent != null)
                {
                    objContentModel.ContentID = objContent.ContentID;

                    objContentModel.AbstractTxt = objContent.AbstractTxt;
                    objContentModel.ContentTypeID = objContent.ContentTypeID;
                    objContentModel.IsExternalLinkInd = objContent.IsExternalLinkInd;
                    objContentModel.ExternalLinkTxt = objContent.ExternalLinkTxt;
                    objContentModel.ExternalLinkTargetInd = objContent.ExternalLinkTargetInd;
                    objContentModel.PageTitleTxt = objContent.PageTitleTxt;
                    objContentModel.MenuTitleTxt = objContent.MenuTitleTxt;
                    objContentModel.PageURLTxt = objContent.PageURLTxt;
                    objContentModel.BannerImageID = objContent.BannerImageID;
                    objContentModel.AltBannerImageTxt = objContent.AltBannerImageTxt;
                    objContentModel.BannerImageAbstractTxt = objContent.BannerImageAbstractTxt;
                    objContentModel.DescriptionTxt = objContent.DescriptionTxt;
                    objContentModel.ContentCreateDate = objContent.ContentCreateDate;
                    objContentModel.CreateDate = objContent.CreateDate;
                    objContentModel.StatusInd = objContent.StatusInd;
                    objContentModel.PageMetaTitleTxt = objContent.PageMetaTitleTxt;
                    objContentModel.RightSectionTitleTxt = objContent.RightSectionTitleTxt;
                    objContentModel.RightSectionAbstractTxt = objContent.RightSectionAbstractTxt;
                    objContentModel.PageMetaDescriptionTxt = objContent.PageMetaDescriptionTxt;
                    objContentModel.IsGooglePlusSharingInd = objContent.IsGooglePlusSharingInd.HasValue ? objContent.IsGooglePlusSharingInd.Value : false;
                    objContentModel.IsFacebookSharingInd = objContent.IsFacebookSharingInd.HasValue ? objContent.IsFacebookSharingInd.Value : false;
                    objContentModel.IsTwitterSharingInd = objContent.IsTwitterSharingInd.HasValue ? objContent.IsTwitterSharingInd.Value : false;
                    objContentModel.strCreateDate = objContent.CreateDate.HasValue ? objContent.CreateDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                    ViewBag.IsActiveInd = GetStatusData(objContentModel.StatusInd == true ? "1" : "0");
                    ViewBag.Date = objContent.ContentCreateDate != null ? objContent.ContentCreateDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                    objContentModel.ContentTypeTitle = "Edit " + contenttypetitle + (IsContent(ContentTypeID) ? " Content" : " Page Content");
                    ViewBag.Submit = "Update";
                }
                else
                {
                    ViewBag.Submit = "Save";
                    string[] ToEmailarr = new string[] { "0" };
                    objContentModel.ContentTypeID = ContentTypeID;
                    ViewBag.Date = DateTime.Now.ToShortDateString();
                    objContentModel.IsGooglePlusSharingInd = false;
                    objContentModel.IsFacebookSharingInd = false;
                    objContentModel.IsTwitterSharingInd = false;
                    objContentModel.ContentTypeTitle = "Add " + contenttypetitle + (IsContent(ContentTypeID) ? " Content" : " Page Content");
                    ViewBag.IsActiveInd = GetStatusData(string.Empty);
                }

                var InnerImagesTitle = Models.Common.GetInnerImages();
                ViewBag.InnerImagesTitle = InnerImagesTitle;//get all the inner image titles

                ViewBag.Title = (ContentModel.GetContentTypeName(Convert.ToInt32(ContentTypeID))) + (ContentTypeID == Convert.ToInt32(ContentTypeAlias.Fly) ? " Page" : "");
                return View(objContentModel);
            }
        }

        /// <summary>
        /// Get content type which does'nt have page content.
        /// </summary>
        private bool IsContent(long contenttype)
        {
            return ((Convert.ToInt32(ContentTypeAlias.Header) == contenttype)
                     || (Convert.ToInt32(ContentTypeAlias.Footer) == contenttype)
                     || (Convert.ToInt32(ContentTypeAlias.GoogleAnalytics) == contenttype)
                     || (Convert.ToInt32(ContentTypeAlias.HomeRightSection) == contenttype)
                    );
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
        public ActionResult Create(ContentModel model, string command, FormCollection fm)
        {
            try
            {
                using (var objContext = new db_KISDEntities())
                {
                    var file = Request.Files.Count > 0 ? Request.Files[0] : null;
                    if (string.IsNullOrEmpty(command))
                    {
                        var InnerImagesTitle = Models.Common.GetInnerImages();
                        ViewBag.InnerImagesTitle = InnerImagesTitle;//get all the inner image titles

                        var RightSectionTitle = (from db in objContext.RightSections
                                                 where db.StatusInd == true
                                                 select new { db.TitleTxt, db.RightSectionID }).ToList().OrderBy(x => x.TitleTxt);

                        ViewBag.RightSectionTitle = RightSectionTitle;

                        var ContentType = model.ContentTypeID;
                        ViewBag.ContentTypeID = ContentType;
                        var IsSave = false;
                        var contenttypetitle = objContext.ContentTypes.Find(ContentType).ContentTypeNameTxt;
                        ViewBag.Title = contenttypetitle;
                        ViewBag.PageTitle = model.PageTitleTxt ?? "";
                        ViewBag.IsActiveInd = GetStatusData(model.StatusInd == true ? "1" : "0");
                        var objContent = objContext.Contents.Where(x => x.ContentTypeID == ContentType).FirstOrDefault();
                        if (objContent == null)
                        {
                            IsSave = true;
                            objContent = new Content();
                        }

                        #region System Change Log
                        DataTable dtOld;
                        var oldResult = (from a in objContext.Contents
                                         where a.ContentID == model.ContentID
                                         select a).ToList();
                        dtOld = Models.Common.LINQResultToDataTable(oldResult);
                        #endregion

                        ViewBag.Submit = IsSave ? "Save" : "Update";

                        if (model != null && !string.IsNullOrEmpty(model.PageURLTxt))
                        {
                            var count = 0;
                            count = objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.ContentID != model.ContentID && x.IsDeletedInd == false).Count();
                            count += objContext.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.Departments.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.ExceptionOpportunities.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.NewsEvents.Where(x => x.PageURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && x.IsDeletedInd == false).Count();
                            count += objContext.RightSections.Where(x => x.ExternalLinkURLTxt.ToLower().Trim() == model.PageURLTxt.ToLower().Trim() && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
                            if (model.PageURLTxt.Trim().ToLower() == "error404")// Check for duplicate url and error404 url
                            {
                                count = count + 1;
                            }

                            if (count > 0)
                            {
                                #region Route value directory
                                var rvd = new RouteValueDictionary();
                                rvd.Add("ContentType", ContentType);
                                #endregion

                                ViewBag.FocusPageUrl = true;// Set focus on Pageurl Field if same url exist
                                ViewBag.InnerImages = new SelectList(objContext.Images.Where(x => x.ImageTypeID == 2 && x.StatusInd == true).ToList(), "ImageID", "TitleTxt");
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


                        objContent.ContentTypeID = ContentType;
                        objContent.ParentID = (int?)null;
                        objContent.IsExternalLinkInd = model.IsExternalLinkInd;
                        objContent.ExternalLinkTxt = string.IsNullOrEmpty(model.ExternalLinkTxt) ? string.Empty : model.ExternalLinkTxt;
                        objContent.ExternalLinkTargetInd = model.IsExternalLinkInd ? model.ExternalLinkTargetInd : false;
                        objContent.PageTitleTxt = model.IsExternalLinkInd ? string.Empty :( string.IsNullOrEmpty(model.PageTitleTxt) ? string.Empty : model.PageTitleTxt);
                        objContent.MenuTitleTxt = string.IsNullOrEmpty(model.MenuTitleTxt) ? (ContentType == 1 ? "home" : string.Empty) : model.MenuTitleTxt.Replace("<", "");
                        objContent.PageURLTxt = model.IsExternalLinkInd ? string.Empty : (string.IsNullOrEmpty(model.PageURLTxt) ? (ContentType == 1 ? "home" : string.Empty) : (ContentType == 1 ? "home" : model.PageURLTxt));
                        objContent.BannerImageID = model.IsExternalLinkInd ? 0 : model.BannerImageID;
                        objContent.AbstractTxt = string.IsNullOrEmpty(model.AbstractTxt) ? string.Empty : model.AbstractTxt;
                        objContent.BannerImageAbstractTxt = model.IsExternalLinkInd ? string.Empty :( string.IsNullOrEmpty(model.BannerImageAbstractTxt) ? string.Empty : model.BannerImageAbstractTxt);
                        objContent.DescriptionTxt = model.IsExternalLinkInd ? string.Empty :( string.IsNullOrEmpty(model.DescriptionTxt) ? string.Empty : model.DescriptionTxt);
                        objContent.StatusInd = (ContentType == Convert.ToInt32(ContentTypeAlias.Footer)
                                               || ContentType == Convert.ToInt32(ContentTypeAlias.Header)
                                               || ContentType == Convert.ToInt32(ContentTypeAlias.Search)
                                               || ContentType == Convert.ToInt32(ContentTypeAlias.Home)
                                               ) ? true : (fm["IsActiveInd"] == "1" ? true : false);
                        objContent.ContentCreateDate = model.ContentCreateDate;

                        objContent.PageMetaTitleTxt = model.IsExternalLinkInd ? string.Empty :( string.IsNullOrEmpty(model.PageMetaTitleTxt) ? string.Empty : model.PageMetaTitleTxt);
                        objContent.PageMetaDescriptionTxt = model.IsExternalLinkInd ? string.Empty :( string.IsNullOrEmpty(model.PageMetaDescriptionTxt) ? string.Empty : model.PageMetaDescriptionTxt);
                        objContent.AltBannerImageTxt = model.IsExternalLinkInd ? string.Empty : model.AltBannerImageTxt;
                        objContent.RightSectionAbstractTxt = model.IsExternalLinkInd ? string.Empty : model.RightSectionAbstractTxt;
                        objContent.RightSectionTitleTxt = model.IsExternalLinkInd ? string.Empty : model.RightSectionTitleTxt;
                        objContent.IsFacebookSharingInd = model.IsExternalLinkInd ? false : model.IsFacebookSharingInd;
                        objContent.IsGooglePlusSharingInd = model.IsExternalLinkInd ? false: model.IsGooglePlusSharingInd;
                        objContent.IsTwitterSharingInd = model.IsExternalLinkInd ? false : model.IsTwitterSharingInd;
                        objContent.IsDeletedInd = false;
                        objContent.CreateDate = model.ContentID > 0 ? objContent.CreateDate : DateTime.Now; ;
                        objContent.CreateByID = model.ContentID > 0 ? objContent.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        objContent.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        objContent.LastModifyDate = DateTime.Now;

                        //Save image path
                        if (file != null && file.ContentLength > 0)
                        {
                            #region Upload Image
                            Models.Common.CreateFolder();
                            var NewImgName = UploadImage();
                            //var croppedfile = new FileInfo(Server.MapPath(NewImgName.Data.ToString()));//err
                            var fileName = NewImgName.Data;
                            TempData["CroppedImage"] = null;
                            objContent.AbstractTxt = "~/WebData/images/" + fileName;
                            #endregion

                            if (!string.IsNullOrEmpty(model.AbstractTxt) && (
                                 (objContent.ContentTypeID == Convert.ToInt32(ContentTypeAlias.Header))
                                ))
                            {
                                try
                                {
                                    Models.Common.DeleteImage(Server.MapPath(model.AbstractTxt));
                                }
                                catch
                                {
                                }
                            }
                        }
                        if (IsSave)
                        {
                            objContext.Contents.Add(objContent);
                        }
                        objContext.SaveChanges();

                        var AlertText = IsContent(objContent.ContentTypeID) ? " Content" : " Page Content";
                        TempData["Alert"] = contenttypetitle + (IsSave ? AlertText + " saved successfully." : AlertText + " updated successfully.");

                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = "Content";
                        objSCL.LogTypeTxt = model.ContentID > 0 ? "Update" : "Add";
                        objSCL.NotesTxt = (GetContentType(model.ContentTypeID)) + " Details" + (model.ContentID > 0 ? " updated for " : "  added for ") + model.PageTitleTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();
                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.Contents
                                         where x.ContentID == objContent.ContentID
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

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Alert"] = " Some error occured. Please try again later.";
                return RedirectToAction("Index", "Home");
            }
        }

        #region Private Methods

        /// <summary>
        /// to save the images uploaded through file uploader
        /// </summary>
        /// <returns>new file name</returns>
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
            var list = objContext.RightSections.Where(x => x.StatusInd == true && x.IsDeletedInd == false).OrderBy(x => x.TitleTxt).Select(x => new RightSectionModel()
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