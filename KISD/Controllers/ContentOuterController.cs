using KISD.Areas.Admin.Models;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ContentTypeAlias = KISD.Areas.Admin.Models.ContentType;
using TypeMasterAlias = KISD.Areas.Admin.Models.GalleryListingService.TypeMaster;
namespace KISD.Controllers
{
    public class ContentOuterController : Controller
    {
        db_KISDEntities objContext = new db_KISDEntities();
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Content(string ContentUrl, int? page, int? type, FormCollection fm, string s, string p, string objresult)
        {
            var obj = new ContentModel();
            var Id = 0;
            int pageSize = Areas.Admin.Models.Common._pageSize;
            int pageNumber = (page ?? Areas.Admin.Models.Common._currentPage);

            int StaffTypeMasterID = Convert.ToInt32(TypeMasterAlias.Staff);
            int PhotoGalleryImagesTypeMasterID = Convert.ToInt32(TypeMasterAlias.PhotoGallery);
            int PodcastTypeMasterID = Convert.ToInt32(TypeMasterAlias.Podcast);
            int VideoTypeMasterID = Convert.ToInt32(TypeMasterAlias.Video);

            var content = objContext.Contents.Where(x => x.PageURLTxt.ToLower().Trim() == ContentUrl.ToLower().Trim() && x.StatusInd == true && x.IsDeletedInd == false).FirstOrDefault();
            var newsListing = objContext.NewsEvents.Where(x => x.PageURLTxt.ToLower().Trim() == ContentUrl.ToLower().Trim() && x.StatusInd == true && x.IsDeletedInd == false).FirstOrDefault();
            var deptListing = objContext.Departments.Where(x => x.URLTxt.ToLower().Trim() == ContentUrl.ToLower().Trim() && x.StatusInd == true && x.IsDeletedInd == false).FirstOrDefault();
            var bomListing = objContext.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == ContentUrl.ToLower().Trim() && x.StatusInd == true && x.IsDeletedInd == false).FirstOrDefault();
            var parentstudentstaffListing = objContext.Schools.Where(x => x.PageURLTxt.ToLower().Trim() == ContentUrl.ToLower().Trim() && x.StatusInd == true && x.IsDeletedInd == false).FirstOrDefault();
            var staffListing = objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == ContentUrl.ToLower().Trim() && x.StatusInd == true && x.IsDeletedInd == false && x.TypeMasterID == StaffTypeMasterID).FirstOrDefault();

            var photoGalleryCategoryListing = objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == ContentUrl.ToLower().Trim() && x.StatusInd == true && x.IsDeletedInd == false && x.TypeMasterID == PhotoGalleryImagesTypeMasterID).FirstOrDefault();
            var podcastListing = objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == ContentUrl.ToLower().Trim() && x.StatusInd == true && x.IsDeletedInd == false && x.TypeMasterID == PodcastTypeMasterID).FirstOrDefault();
            var videoListing = objContext.GalleryListings.Where(x => x.URLTxt.ToLower().Trim() == ContentUrl.ToLower().Trim() && x.StatusInd == true && x.IsDeletedInd == false && x.TypeMasterID == VideoTypeMasterID).FirstOrDefault();
            var exceptionOpportunitiesListing = objContext.ExceptionOpportunities.Where(x => x.URLTxt.ToLower().Trim() == ContentUrl.ToLower().Trim() && x.StatusInd == true && x.IsDeletedInd == false).FirstOrDefault();
            var rightSectionListing = objContext.RightSections.Where(x => x.ExternalLinkURLTxt.ToLower().Trim() == ContentUrl.ToLower().Trim() && x.StatusInd == true && x.IsDeletedInd == false).FirstOrDefault();

            var AppPath = (Request.IsSecureConnection ? "https://" : "http://") + (System.Web.HttpContext.Current.Request.Url.DnsSafeHost)
                                                 + (System.Web.HttpContext.Current.Request.IsLocal ? (":" + System.Web.HttpContext.Current.Request.Url.Port.ToString()) :
                                                 (System.Web.HttpContext.Current.Request.ApplicationPath.Length > 1 ? System.Web.HttpContext.Current.Request.ApplicationPath : ""));
            AppPath = Areas.Admin.Models.Common.ReturnValidPath(AppPath);

            #region On Screen Top Alerts
            int TypeMasterID = Convert.ToInt32(TypeMasterAlias.OnscreenAlert);
            var objOnScreenAlerts = objContext.Announcements.Where(x => x.TypeMasterID == TypeMasterID
                      && x.IsDeletedInd == false && x.StatusInd == true).OrderByDescending(x => x.AnnouncementCreateDate).Take(3).ToList();
            if (objOnScreenAlerts != null)
            {
                string html = string.Empty;
                foreach (var alert in objOnScreenAlerts)
                {
                    html += @"<div class='alert-div'><div class='container'><span class='alerticon'></span>" + alert.TitleTxt + "</div></div>";
                }
                ViewBag.TopAlertMessages = html;
            }
            #endregion

            if (content != null && content.StatusInd == true)
            {
                #region Common Content
                ViewBag.ContentTypeID = content.ContentTypeID;
                obj.ContentID = content.ContentID;
                obj.PageTitleTxt = content.PageTitleTxt;
                obj.ContentTypeID = content.ContentTypeID;
                obj.PageTitleTxt = content.PageTitleTxt;
                obj.DescriptionTxt = content.DescriptionTxt;
                obj.PageURLTxt = content.PageURLTxt;
                obj.PageMetaTitleTxt = content.PageMetaTitleTxt;
                obj.PageMetaDescriptionTxt = content.PageMetaDescriptionTxt;
                obj.MenuTitleTxt = content.MenuTitleTxt;
                obj.AbstractTxt = content.AbstractTxt;
                obj.AltBannerImageTxt = content.AltBannerImageTxt;
                obj.BannerImageID = content.BannerImageID;
                obj.BannerImageAbstractTxt = content.BannerImageAbstractTxt;
                obj.RightSectionTitleTxt = content.RightSectionTitleTxt;
                obj.RightSectionAbstractTxt = content.RightSectionAbstractTxt;
                obj.IsFacebookSharingInd = content.IsFacebookSharingInd.HasValue ? content.IsFacebookSharingInd.Value : false;
                obj.IsGooglePlusSharingInd = content.IsGooglePlusSharingInd.HasValue ? content.IsGooglePlusSharingInd.Value : false;
                obj.IsTwitterSharingInd = content.IsTwitterSharingInd.HasValue ? content.IsTwitterSharingInd.Value : false;

                ViewBag.Title = content.PageMetaTitleTxt ?? "";
                ViewBag.MetaDescription = content.PageMetaDescriptionTxt ?? "";
                ViewBag.AltImgTxt = content.AltBannerImageTxt;
                #endregion

                #region Right Section
                obj.ParentRightSections = objContext.RightSections.Where(x => x.ListingID == content.ContentID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID == null).ToList();
                obj.IsFacebookSharingInd = content.IsFacebookSharingInd.HasValue ? content.IsFacebookSharingInd.Value : false;
                obj.IsGooglePlusSharingInd = content.IsGooglePlusSharingInd.HasValue ? content.IsGooglePlusSharingInd.Value : false;
                obj.IsTwitterSharingInd = content.IsTwitterSharingInd.HasValue ? content.IsTwitterSharingInd.Value : false;
                #endregion

                #region Breadcrumbs
                try
                {
                    var SubParent = (from db in objContext.Contents
                                     where db.ContentID == content.ParentID
                                     select new { db.ContentID }).FirstOrDefault();

                    if (SubParent == null)
                    {
                        SubParent = (from db in objContext.Contents
                                     where db.ContentID == content.ContentID
                                     select new { db.ContentID }).FirstOrDefault();
                    }
                    else
                    {
                        var MainParent = (from db in objContext.Contents
                                          where db.ContentID == SubParent.ContentID
                                          select new { db.MenuTitleTxt, db.PageURLTxt }).FirstOrDefault();
                        if (MainParent != null)
                        {
                            if (!string.IsNullOrEmpty(MainParent.MenuTitleTxt)) { ViewBag.MainParentName = MainParent.MenuTitleTxt; } else { ViewBag.MainParentName = ""; }
                            if (!string.IsNullOrEmpty(MainParent.PageURLTxt)) { ViewBag.MainURL = MainParent.PageURLTxt; } else { ViewBag.MainURL = ""; }
                        }
                    }

                    if (SubParent.ContentID > 0)
                    {
                        var MainParent = (from db in objContext.Contents
                                          where db.ContentID == SubParent.ContentID
                                          select new { db.PageURLTxt, db.MenuTitleTxt }).FirstOrDefault();
                    }
                    Id = GetMenuID(content.ContentTypeID);
                    var MainParentName = (from db in objContext.Contents
                                          where db.ContentTypeID == Id
                                          select new { db.MenuTitleTxt, db.PageURLTxt }).FirstOrDefault();

                    if (!string.IsNullOrEmpty(MainParentName.MenuTitleTxt)) { ViewBag.ParentName = MainParentName.MenuTitleTxt; } else { ViewBag.ParentName = ""; }
                    if (!string.IsNullOrEmpty(MainParentName.PageURLTxt)) { ViewBag.ParentURL = MainParentName.PageURLTxt; } else { ViewBag.ParentURL = ""; }
                }
                catch { ViewBag.ParentName = ""; ViewBag.ParentURL = ""; ViewBag.MainParentName = ""; ViewBag.MainURL = ""; }

                #endregion

                #region Inner Image
                try
                {
                    var data = (from db in objContext.Images
                                where db.ImageID == content.BannerImageID
                                select new { db.ImgPathTxt, db.TitleTxt }).FirstOrDefault();

                    if (data.ImgPathTxt != null)
                    {
                        ViewBag.InnerImgPath = data.ImgPathTxt.Replace("~", "");
                    }
                    ViewBag.PageTitle = content.PageTitleTxt == null ? string.Empty : content.PageTitleTxt;
                    if (data.TitleTxt != null)
                    {
                        ViewBag.InnerImgTitleTxt = data.TitleTxt;
                    }
                }
                catch (NullReferenceException ex) { ViewBag.InnerImgPath = ""; ViewBag.AbstractText = ""; ViewBag.PageTitle = ""; ViewBag.InnerImgTitleTxt = ""; }
                #endregion

                #region News
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.News))
                {
                    obj.TypeMasterID = Convert.ToInt32(TypeMasterAlias.News);
                    var item1 = objContext.NewsEvents.Where(x => x.StatusInd == true && x.TypeMasterID == obj.TypeMasterID && DateTime.Now >= x.DisplayStartDate && DateTime.Now < x.DisplayEndDate && (x.IsDeletedInd == false || x.IsDeletedInd == null)).OrderByDescending(x => x.EventCreateDate).ThenByDescending(x => x.NewsEventID).ToList();
                    obj.IsPagingVisible = item1.Count > pageSize;
                    obj.NewsEventResult = item1.ToPagedList(pageNumber, pageSize);
                    return View("NewsEvent", obj);
                }
                #endregion

                #region Events
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.Events))
                {
                    obj.TypeMasterID = Convert.ToInt32(TypeMasterAlias.Events);
                    var item1 = objContext.NewsEvents.Where(x => x.StatusInd == true && (x.IsDeletedInd == false || x.IsDeletedInd == null) && x.TypeMasterID == obj.TypeMasterID && DateTime.Now >= x.DisplayStartDate && DateTime.Now < x.DisplayEndDate).OrderByDescending(x => x.EventCreateDate).ThenByDescending(x => x.NewsEventID).ToList();
                    obj.IsPagingVisible = item1.Count > pageSize;
                    obj.NewsEventResult = item1.ToPagedList(pageNumber, pageSize);
                    return View("NewsEvent", obj);
                }
                #endregion

                #region School 
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.School))
                {
                    obj.TypeMasterID = Convert.ToInt32(TypeMasterAlias.SchoolCategory);
                    var item1 = objContext.Schools.Where(x => x.StatusInd == true && x.IsDeletedInd == false && x.TypeMasterID == obj.TypeMasterID
                    && (objContext.Schools.Where(y => y.StatusInd == true && y.IsDeletedInd == false && y.SchoolCategoryID == x.SchoolID).Count() > 0)).OrderByDescending(x => x.SchoolCreateDate).ThenByDescending(x => x.SchoolID).ToList();
                    obj.IsPagingVisible = item1.Count > pageSize;
                    obj.SchoolCategoryResult = item1.ToPagedList(pageNumber, pageSize);
                    obj.TypeMasterID = Convert.ToInt32(TypeMasterAlias.School);
                    item1 = objContext.Schools.Where(x => x.StatusInd == true && x.IsDeletedInd == false && x.TypeMasterID == obj.TypeMasterID).OrderByDescending(x => x.SchoolCreateDate).ThenByDescending(x => x.SchoolID).ToList();
                    obj.SchoolResult = item1.ToPagedList(pageNumber, item1.Count == 0 ? 10 : item1.Count);
                    return View("School", obj);
                }
                #endregion

                #region Parent & Students
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.ParentStudents))
                {
                    obj.TypeMasterID = Convert.ToInt32(TypeMasterAlias.SchoolCategory);
                    var item1 = objContext.Schools.Where(x => x.StatusInd == true && x.TypeMasterID == obj.TypeMasterID && x.IsDeletedInd == false).OrderByDescending(x => x.SchoolCreateDate).ThenByDescending(x => x.SchoolID).ToList();
                    obj.SchoolCategoryResult = item1.ToPagedList(pageNumber, 10);
                    obj.TypeMasterID = Convert.ToInt32(TypeMasterAlias.School);
                    item1 = objContext.Schools.Where(x => x.StatusInd == true && x.TypeMasterID == obj.TypeMasterID && x.IsDeletedInd == false).OrderByDescending(x => x.SchoolCreateDate).ThenByDescending(x => x.SchoolID).ToList();
                    obj.SchoolResult = item1.ToPagedList(pageNumber, item1.Count == 0 ? 10 : item1.Count);
                    var staffcontenttypeid = Convert.ToInt32(ContentTypeAlias.Staff);
                    obj.PageURLTxt = objContext.Contents.Where(x => x.ContentTypeID == staffcontenttypeid && x.StatusInd == true).Select(x => x.PageURLTxt).FirstOrDefault();
                    return View("ParentStudent", obj);
                }
                #endregion

                #region Parent and Student Staff Listing
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.Staff))
                {
                    var item = new List<Staff>();
                    SetMainParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.ParentStudents));
                    var schoolid = !string.IsNullOrEmpty(p) ? Convert.ToInt32(EncryptDecrypt.Decrypt(p)) : 0;
                    if (schoolid > 0)
                    {
                        obj.SchoolList = objContext.Schools.Where(x => x.IsDeletedInd == false && x.StatusInd == true && x.SchoolID == schoolid).ToList();
                        item = objContext.Staffs.Where(x => x.IsDeletedInd == false && x.StatusInd == true && x.SchoolID == schoolid).ToList();
                        obj.StaffResult = item.ToPagedList(pageNumber, pageSize);
                        obj.IsPagingVisible = item.Count() > pageSize;
                    }
                    else
                    {
                        Response.Redirect(AppPath);
                    }
                    return View("ParentStudentStaff", obj);
                }
                #endregion

                #region Department 
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.Departments))
                {
                    obj.TypeMasterID = Convert.ToInt32(TypeMasterAlias.Department);
                    var item1 = objContext.Departments.Where(x => x.StatusInd == true && x.IsDeletedInd == false && x.ParentID == null).OrderBy(x => x.DisplayOrderNbr).ToList();
                    obj.DepartmentResult = item1.ToPagedList(pageNumber, item1.Count == 0 ? 10 : item1.Count);
                    return View("Department", obj);
                }
                #endregion

                #region Board Members
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.BoardMembers))
                {
                    var item1 = objContext.BoardOfMembers.Where(x => x.StatusInd == true && x.IsDeletedInd == false).OrderByDescending(x => x.BOMCreateDate).ThenByDescending(x => x.BoardMemberID).ToList();
                    obj.IsPagingVisible = item1.Count > 10;
                    obj.BOMResult = item1.ToPagedList(pageNumber, 10);
                    return View("BoardOfMembers", obj);
                }
                #endregion

                #region Board Schedule
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.BoardSchedule))
                {
                    obj.TypeMasterID = Convert.ToInt32(TypeMasterAlias.BoardSchedule);
                    var item1 = objContext.Announcements.Where(x => x.StatusInd == true && x.IsDeletedInd == false && x.TypeMasterID == obj.TypeMasterID).OrderByDescending(x => x.AnnouncementCreateDate).ThenByDescending(x => x.AnnouncementID).ToList();
                    obj.IsPagingVisible = item1.Count > 10;
                    obj.BoardScheduleResult = item1.ToPagedList(pageNumber, 10);
                    return View("BoardSchedule", obj);
                }
                #endregion

                #region Videos
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.Videos))
                {
                    obj.TypeMasterID = Convert.ToInt32(TypeMasterAlias.Video);
                    var item1 = objContext.GalleryListings.Where(x => x.StatusInd == true
                                && x.IsDeletedInd == false && x.TypeMasterID == obj.TypeMasterID
                                && x.DisplayStartDate.Value <= DateTime.Now
                                && x.DisplayEndDate.Value >= DateTime.Now)
                                .OrderByDescending(x => x.ListingCreateDate).
                                ThenByDescending(x => x.ListingID).ToList();
                    obj.IsPagingVisible = item1.Count > 10;
                    obj.VideoResult = item1.ToPagedList(pageNumber, 10);
                    return View("Video", obj);
                }
                #endregion

                #region Search Content Page
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.Search))
                {
                    var SearchResults = objContext.pr_search_all_paged(s).ToList();
                    obj.IsPagingVisible = SearchResults.Count() > pageSize;
                    obj.SearchAllResult = SearchResults.ToPagedList(pageNumber, pageSize);
                    //ViewBag.BannerImage = content.BannerImageID != null ? objContext.Images.Where(x => x.StatusInd == true && x.IsDeletedInd == false && x.ImageID == content.BannerImageID).Select(x => x.ImgPathTxt).FirstOrDefault() : null;
                    return View("SearchListing", obj);
                }
                #endregion

                #region Personal directory
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.PersonalDirectory))
                {
                    AjaxRequest objAjaxRequest = !string.IsNullOrEmpty(objresult) ? JsonConvert.DeserializeObject<AjaxRequest>(objresult) : null;
                    var strsearch = objAjaxRequest != null ? objAjaxRequest.qs_value : "";
                    var strtype = objAjaxRequest != null ? objAjaxRequest.qs_Type : "";
                    obj.StaffResult = null;
                    obj.TypeMasterID = Convert.ToInt32(TypeMasterAlias.SchoolCategory);
                    var item = objContext.Schools.Where(x => x.StatusInd == true && (x.IsDeletedInd == false || x.IsDeletedInd == null) && x.TypeMasterID == obj.TypeMasterID).OrderByDescending(x => x.SchoolCreateDate).ThenByDescending(x => x.SchoolID).ToList();
                    obj.SchoolCategoryResult = item.ToPagedList(pageNumber, item.Count);
                    obj.TypeMasterID = Convert.ToInt32(TypeMasterAlias.School);
                    item = objContext.Schools.Where(x => x.StatusInd == true && (x.IsDeletedInd == false || x.IsDeletedInd == null) && x.TypeMasterID == obj.TypeMasterID).OrderByDescending(x => x.SchoolCreateDate).ThenByDescending(x => x.SchoolID).ToList();
                    obj.SchoolResult = item.ToPagedList(pageNumber, item.Count);
                    var dept = objContext.Departments.Where(x => x.StatusInd == true && (x.IsDeletedInd == false || x.IsDeletedInd == null) && x.ParentID == null).OrderByDescending(x => x.DepartmentCreateDate).ThenByDescending(x => x.DepartmentID).ToList();
                    obj.DepartmentResult = dept.ToPagedList(pageNumber, dept.Count);
                    var item1 = objContext.Staffs.Where(x => x.StatusInd == true && (x.IsDeletedInd == false || x.IsDeletedInd == null)).OrderByDescending(x => x.StaffCreateDate).ThenByDescending(x => x.StaffID).ToList();
                    if (strtype == "1")
                    {
                        item1 = item1.Where(x => (x.LastNameTxt).Contains(strsearch.Trim())).ToList();
                    }
                    else if (strtype == "2")
                    {
                        var schoolid = Convert.ToInt32(strsearch);
                        item1 = item1.Where(x => x.SchoolID == Convert.ToInt32(strsearch)).ToList();
                        ViewBag.Name = objContext.Schools.Where(x => x.SchoolID == schoolid).Select(x => x.AddressTxt).FirstOrDefault();
                        ViewBag.Phone = objContext.Schools.Where(x => x.SchoolID == schoolid).Select(x => x.PhoneNumberTxt).FirstOrDefault();
                    }
                    else if (strtype == "3")
                    {
                        var deptid = Convert.ToInt32(strsearch);
                        var deptstaff = objContext.DepartmentStaffs.Where(x => x.DepartmentID == deptid).Select(x => x.StaffID).ToList();
                        item1 = item1.Where(x => deptstaff.Contains(x.StaffID)).ToList();
                        ViewBag.Name = objContext.Departments.Where(x => x.DepartmentID == deptid).Select(x => x.AddressTxt).FirstOrDefault();
                        ViewBag.Phone = objContext.Departments.Where(x => x.DepartmentID == deptid).Select(x => x.PhoneNumberTxt).FirstOrDefault();

                    }
                    if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
                    {
                        obj.IsPagingVisible = item1.Count > pageSize;
                        obj.StaffResult = item1.ToPagedList(pageNumber, pageSize);
                        ViewBag.Type = strtype;
                        ViewBag.Search = strsearch;
                        return View("PersonalDirectoryPartial", obj);// ("partial view name ")
                    }
                    else
                    {
                        return View("PersonalDirectory", obj);
                    }
                }
                #endregion

                #region Department Events
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.DepartmentEvents))
                {
                    obj.DepartmentID = content.DepartmentID.HasValue ? content.DepartmentID.Value : 0;
                    obj.DescriptionTxt = content.DescriptionTxt;
                    SetDepartmentBreadcrumbs(content.DepartmentID.Value);
                    SetSecondParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.Departments));
                    return View("DepartmentEvents", obj);
                }
                #endregion

                #region Department Staff
                if (obj.ContentTypeID == Convert.ToInt32(ContentTypeAlias.DepartmentStaff))
                {
                    var item1 = objContext.DepartmentStaffs.Where(x => x.DepartmentID == content.DepartmentID).Select(x => x.Staff).ToList();
                    obj.IsPagingVisible = item1.Count > 10;
                    obj.StaffResult = item1.ToPagedList(pageNumber, 10);
                    obj.DescriptionTxt = content.DescriptionTxt;
                    SetSecondParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.Departments));
                    SetDepartmentBreadcrumbs(content.DepartmentID.Value);
                    return View("DepartmentStaff", obj);
                }
                #endregion

                return View(obj);
            }

            #region News Details
            else if (newsListing != null)
            {
                NewsEventModel objNews = new NewsEventModel();
                var Contnt = objContext.Images.Where(x => x.ImageID == newsListing.BannerImageID && x.StatusInd == true).FirstOrDefault();
                var newscontenttypeid = Convert.ToInt32(ContentTypeAlias.News);
                ViewBag.InnerImgPath = Contnt != null ? Contnt.ImgPathTxt.Replace("~", "") : null;
                ViewBag.InnerImgTitleTxt = Contnt != null ? Contnt.TitleTxt : null;
                ViewBag.InnerImgAbstractTxt = !string.IsNullOrEmpty(newsListing.BannerImageAbstractTxt) ? newsListing.BannerImageAbstractTxt : null;
                ViewBag.InnerImgAltImageTxt = !string.IsNullOrEmpty(newsListing.AltBannerImageTxt) ? newsListing.AltBannerImageTxt : null;
                SetMainParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.News));
                objNews.NewsEventID = newsListing.NewsEventID;
                objNews.ImageURLTxt = newsListing.ImageURLTxt;
                objNews.PageMetaTitleTxt = newsListing.PageMetaTitleTxt;
                objNews.BannerImageAbstractTxt = newsListing.BannerImageAbstractTxt;
                objNews.DescriptionTxt = newsListing.DescriptionTxt;
                objNews.PageURLTxt = newsListing.PageURLTxt;
                objNews.TitleTxt = newsListing.TitleTxt;
                objNews.EventCreateDate = newsListing.EventCreateDate;
                objNews.AuthorNameTxt = newsListing.AuthorNameTxt;
                objNews.TitleTxt = newsListing.TitleTxt;
                ViewBag.AltImgTxt = newsListing.AltBannerImageTxt ?? "";
                ViewBag.MetaDescription = newsListing.PageMetaDescriptionTxt ?? "";
                ViewBag.Title = newsListing.PageMetaTitleTxt ?? "";
                objNews.RightSectionTitleTxt = newsListing.RightSectionTitleTxt;
                objNews.RightSectionAbstractTxt = newsListing.RightSectionAbstractTxt;
                objNews.ParentRightSections = objContext.RightSections.Where(x => x.ListingID == newsListing.NewsEventID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID == null).ToList();
                objNews.ContentUrl = objContext.Contents.Where(x => x.ContentTypeID == newscontenttypeid && x.StatusInd == true).Select(x => x.PageURLTxt).SingleOrDefault();
                return View("NewsEventDetail", objNews);
            }
            #endregion

            #region Department Details
            else if (deptListing != null)
            {
                DepartmentModel objdept = new DepartmentModel();
                var Contnt = objContext.Images.Where(x => x.ImageID == obj.BannerImageID && x.StatusInd == true).FirstOrDefault();//Object will Change after bug fixes Admin section
                var newscontenttypeid = Convert.ToInt32(ContentTypeAlias.News);
                ViewBag.InnerImgPath = Contnt != null ? Contnt.ImgPathTxt : null;
                ViewBag.InnerImgTitleTxt = Contnt != null ? Contnt.TitleTxt : null;
                ViewBag.InnerImgAbstractTxt = !string.IsNullOrEmpty(obj.BannerImageAbstractTxt) ? obj.BannerImageAbstractTxt : null;//Object will Change after bug fixes Admin section
                ViewBag.InnerImgAltImageTxt = !string.IsNullOrEmpty(obj.AltBannerImageTxt) ? obj.AltBannerImageTxt : null;//Object will Change after bug fixes Admin section
                SetMainParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.Departments));
                objdept.AddressTxt = deptListing.AddressTxt;
                objdept.PageMetaTitleTxt = deptListing.PageMetaTitleTxt;
                objdept.DepartmentCreateDate = deptListing.DepartmentCreateDate;
                objdept.DepartmentID = deptListing.DepartmentID;
                objdept.DescriptionTxt = deptListing.DescriptionTxt;
                objdept.FaxNumberTxt = deptListing.FaxNumberTxt;
                objdept.NameTxt = deptListing.NameTxt;
                objdept.PageMetaDescription = deptListing.PageMetaDescription;
                objdept.PageMetaTitleTxt = deptListing.PageMetaTitleTxt;
                objdept.PhoneNumberTxt = deptListing.PhoneNumberTxt;
                objdept.URLTxt = deptListing.URLTxt;
                objdept.RightSectionTitleTxt = deptListing.RightSectionTitleTxt;
                objdept.RightSectionAbstractTxt = deptListing.RightSectionAbstractTxt;
                ViewBag.AltImgTxt = obj.AltBannerImageTxt ?? "";//Object will Change after bug fixes Admin section
                ViewBag.MetaDescription = deptListing.PageMetaDescription ?? "";
                ViewBag.Title = deptListing.PageMetaTitleTxt ?? "";
                objdept.SubDepartmentListing = objContext.Departments.Where(x => x.ParentID == deptListing.DepartmentID && x.StatusInd == true && x.IsDeletedInd == false).OrderByDescending(x => x.DepartmentCreateDate).ThenByDescending(x => x.DepartmentID).ToList();
                objdept.ParentRightSections = objContext.RightSections.Where(x => x.ListingID == deptListing.DepartmentID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID == null).ToList();
                return View("DepartmentDetail", objdept);
            }
            #endregion

            #region Board Of Memebers Details
            else if (bomListing != null)
            {
                BoardMembersModel objbom = new BoardMembersModel();
                var Contnt = objContext.Images.Where(x => x.ImageID == obj.BannerImageID && x.StatusInd == true).FirstOrDefault();//Object will Change after bug fixes Admin section
                var bomcontenttypeid = Convert.ToInt32(ContentTypeAlias.BoardMembers);
                ViewBag.InnerImgPath = Contnt != null ? Contnt.ImgPathTxt : null;
                ViewBag.InnerImgTitleTxt = Contnt != null ? Contnt.TitleTxt : null;
                ViewBag.InnerImgAbstractTxt = !string.IsNullOrEmpty(obj.BannerImageAbstractTxt) ? obj.BannerImageAbstractTxt : null;//Object will Change after bug fixes Admin section
                ViewBag.InnerImgAltImageTxt = !string.IsNullOrEmpty(obj.AltBannerImageTxt) ? obj.AltBannerImageTxt : null;//Object will Change after bug fixes Admin section
                SetMainParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.SchoolBoard));
                SetSecondParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.BoardMembers));
                objbom.ImageURLTxt = bomListing.ImageURLTxt;
                objbom.PageMetaTitleTxt = bomListing.PageMetaTitleTxt;
                objbom.BoardMemberID = bomListing.BoardMemberID;
                objbom.DescriptionTxt = bomListing.DescriptionTxt;
                objbom.ContactInfoTxt = bomListing.ContactInfoTxt;
                objbom.TitleTxt = bomListing.TitleTxt;
                objbom.DisplayOrderNbr = bomListing.DisplayOrderNbr;
                objbom.NameTxt = bomListing.NameTxt;
                objbom.TermTxt = bomListing.TermTxt;
                objbom.URLTxt = bomListing.URLTxt;
                objbom.RightSectionAbstractTxt = bomListing.RightSectionAbstractTxt;
                objbom.RightSectionTitleTxt = bomListing.RightSectionTitleTxt;
                objbom.ParentRightSections = objContext.RightSections.Where(x => x.ListingID == bomListing.BoardMemberID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID == null).ToList();
                ViewBag.AltImgTxt = obj.AltBannerImageTxt ?? "";
                ViewBag.MetaDescription = bomListing.PageMetaDescriptionTxt ?? "";
                ViewBag.Title = bomListing.PageMetaTitleTxt ?? "";
                return View("BoardOfMemberDetail", objbom);
            }
            #endregion

            #region Photo Gallery Images Details
            else if (photoGalleryCategoryListing != null)
            {
                GalleryListingModel objModel = new GalleryListingModel();
                var photoGalleryImages = objContext.GalleryListings.Where(x => x.StatusInd == true && x.IsDeletedInd == false && x.ParentID == photoGalleryCategoryListing.ListingID).ToList();
                //SetPhotoCategoryBreadcrumbs(Convert.ToInt32(photoGalleryCategoryListing.ListingID));
                ViewBag.InnerImgPath = objContext.Images.Where(x => x.ImageID == photoGalleryCategoryListing.BannerImageID).Select(x => x.ImgPathTxt.Replace("~", "")).FirstOrDefault();
                ViewBag.InnerImgTitleTxt = photoGalleryCategoryListing.AltBannerImageTxt;
                ViewBag.InnerImgAbstractTxt = !string.IsNullOrEmpty(objModel.BannerImageAbstractTxt) ? objModel.BannerImageAbstractTxt : null;//Object will Change after bug fixes Admin section
                ViewBag.InnerImgAltImageTxt = !string.IsNullOrEmpty(objModel.AltBannerImageTxt) ? objModel.AltBannerImageTxt : null;//Object will Change after bug fixes Admin section

                objModel.TypeMasterID = Convert.ToInt32(TypeMasterAlias.PhotoGallery);
                objModel.DescriptionTxt = photoGalleryCategoryListing.DescriptionTxt;
                objModel.IsPagingVisible = photoGalleryImages.Count > 20;
                objModel.CommonPagedList = photoGalleryImages.ToPagedList(pageNumber, 20);

                return View("PhotoGalleryImagesDetail", objModel);
            }
            #endregion

            #region Podcast Details
            else if (podcastListing != null)
            {
                GalleryListingModel objModel = new GalleryListingModel();
                ViewBag.InnerImgPath = objContext.Images.Where(x => x.ImageID == podcastListing.BannerImageID).Select(x => x.ImgPathTxt.Replace("~", "")).FirstOrDefault();
                ViewBag.InnerImgTitleTxt = podcastListing.AltBannerImageTxt;
                ViewBag.InnerImgAbstractTxt = !string.IsNullOrEmpty(podcastListing.BannerImageAbstractTxt) ? podcastListing.BannerImageAbstractTxt : null;//Object will Change after bug fixes Admin section
                ViewBag.InnerImgAltImageTxt = !string.IsNullOrEmpty(podcastListing.AltBannerImageTxt) ? podcastListing.AltBannerImageTxt : null;//Object will Change after bug fixes Admin section
                //SetMainParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.SchoolBoard));
                var podcasts = objContext.GalleryListings.Where(x => x.StatusInd == true && x.IsDeletedInd == false && x.ListingID == podcastListing.ListingID).FirstOrDefault();
                ViewBag.AltImgTxt = obj.AltBannerImageTxt ?? "";
                objModel.TypeMasterID = Convert.ToInt32(TypeMasterAlias.Podcast);
                if (podcasts != null)
                {
                    objModel.TitleTxt = podcasts.TitleTxt;
                    objModel.DescriptionTxt = podcasts.DescriptionTxt;
                    objModel.AuthorTxt = podcasts.AuthorTxt;
                    objModel.CreateDate = podcasts.CreateDate;
                    objModel.ListingCreateDate = podcasts.ListingCreateDate;
                    objModel.FileURLTxt = podcasts.FileURLTxt;
                    objModel.UploadTypeNbr = podcasts.UploadTypeNbr;
                    objModel.EmbededURLTxt = podcasts.EmbededURLTxt;
                }

                return View("PodcastDetails", objModel);
            }
            #endregion

            #region Video Details
            else if (videoListing != null)
            {
                GalleryListingModel objModel = new GalleryListingModel();
                ViewBag.InnerImgPath = objContext.Images.Where(x => x.ImageID == videoListing.BannerImageID).Select(x => x.ImgPathTxt.Replace("~", "")).FirstOrDefault();
                ViewBag.InnerImgTitleTxt = videoListing.AltBannerImageTxt;
                ViewBag.InnerImgAbstractTxt = !string.IsNullOrEmpty(videoListing.BannerImageAbstractTxt) ? videoListing.BannerImageAbstractTxt : null;//Object will Change after bug fixes Admin section
                ViewBag.InnerImgAltImageTxt = !string.IsNullOrEmpty(videoListing.AltBannerImageTxt) ? videoListing.AltBannerImageTxt : null;//Object will Change after bug fixes Admin section
                //SetMainParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.SchoolBoard));
                var videos = objContext.GalleryListings.Where(x => x.StatusInd == true && x.IsDeletedInd == false && x.ListingID == videoListing.ListingID).FirstOrDefault();
                ViewBag.AltImgTxt = obj.AltBannerImageTxt ?? "";
                objModel.TypeMasterID = Convert.ToInt32(TypeMasterAlias.Podcast);
                if (videos != null)
                {
                    objModel.TitleTxt = videos.TitleTxt;
                    objModel.DescriptionTxt = videos.DescriptionTxt;
                    objModel.AuthorTxt = videos.AuthorTxt;
                    objModel.CreateDate = videos.CreateDate;
                    objModel.ListingCreateDate = videos.ListingCreateDate;
                    objModel.FileURLTxt = videos.FileURLTxt;
                    objModel.UploadTypeNbr = videos.UploadTypeNbr;
                    objModel.EmbededURLTxt = videos.EmbededURLTxt;
                }

                return View("VideoDetails", objModel);
            }
            #endregion

            #region Exceptional Opportunities Details
            else if (exceptionOpportunitiesListing != null)
            {
                ExceptionOpportunityModel objModel = new ExceptionOpportunityModel();
                ViewBag.InnerImgPath = objContext.Images.Where(x => x.ImageID == exceptionOpportunitiesListing.BannerImageID).Select(x => x.ImgPathTxt.Replace("~", "")).FirstOrDefault();
                ViewBag.InnerImgTitleTxt = exceptionOpportunitiesListing.AltBannerImageTxt;
                ViewBag.InnerImgAbstractTxt = !string.IsNullOrEmpty(exceptionOpportunitiesListing.BannerImageAbstractTxt) ? exceptionOpportunitiesListing.BannerImageAbstractTxt : null;//Object will Change after bug fixes Admin section
                ViewBag.InnerImgAltImageTxt = !string.IsNullOrEmpty(exceptionOpportunitiesListing.AltBannerImageTxt) ? exceptionOpportunitiesListing.AltBannerImageTxt : null;//Object will Change after bug fixes Admin section
                //SetMainParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.SchoolBoard));
                var ExceptionOpportunities = objContext.ExceptionOpportunities.Where(x => x.StatusInd == true && x.IsDeletedInd == false && x.ExOpportunityID == exceptionOpportunitiesListing.ExOpportunityID).FirstOrDefault();
                ViewBag.AltImgTxt = obj.AltBannerImageTxt ?? "";

                if (ExceptionOpportunities != null)
                {
                    objModel.ExOpportunityID = ExceptionOpportunities.ExOpportunityID;
                    objModel.TitleTxt = ExceptionOpportunities.TitleTxt;
                    objModel.DescriptionTxt = ExceptionOpportunities.DescriptionTxt;
                    objModel.CreateDate = ExceptionOpportunities.CreateDate;
                    objModel.ParentRightSections = objContext.RightSections.Where(x => x.ListingID == exceptionOpportunitiesListing.ExOpportunityID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID == null).ToList();
                    objModel.ExOpportunityCreateDate = ExceptionOpportunities.ExOpportunityCreateDate.Value;
                    if (Request.QueryString["t"] != null)
                    {
                        if (Request.QueryString["t"].ToString() == "c")
                        {
                            SetMainParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.NewToKISD));
                            SetSecondParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.ExceptionalOpportunities));
                        }
                    }
                }

                return View("ExceptionOpportunitiesDetails", objModel);
            }
            #endregion

            #region Right Section Details
            else if (rightSectionListing != null)
            {
                RightSectionModel objModel = new RightSectionModel();
                ViewBag.InnerImgPath = objContext.Images.Where(x => x.ImageID == rightSectionListing.BannerImageID).Select(x => x.ImgPathTxt.Replace("~", "")).FirstOrDefault();
                ViewBag.InnerImgTitleTxt = rightSectionListing.AltBannerImageTxt;
                ViewBag.InnerImgAbstractTxt = !string.IsNullOrEmpty(rightSectionListing.BannerImageAbstractTxt) ? rightSectionListing.BannerImageAbstractTxt : null;//Object will Change after bug fixes Admin section
                ViewBag.InnerImgAltImageTxt = !string.IsNullOrEmpty(rightSectionListing.AltBannerImageTxt) ? rightSectionListing.AltBannerImageTxt : null;//Object will Change after bug fixes Admin section
                //SetMainParentBreadcrumbs(Convert.ToInt32(ContentTypeAlias.SchoolBoard));
                var rightSection = objContext.RightSections.Where(x => x.StatusInd == true && x.IsDeletedInd == false && x.RightSectionID == rightSectionListing.RightSectionID).FirstOrDefault();
                ViewBag.AltImgTxt = obj.AltBannerImageTxt ?? "";
                ViewBag.Title = rightSectionListing.PageMetaTitleTxt ?? "";
                ViewBag.MetaDescription = rightSectionListing.PageMetaDescriptionTxt ?? "";

                if (rightSection != null)
                {
                    objModel.RightSectionID = rightSection.RightSectionID;
                    objModel.TitleTxt = rightSection.TitleTxt;
                    objModel.PageTitleTxt = rightSection.PageTitleTxt;
                    objModel.DescriptionTxt = rightSection.DescriptionTxt;
                    objModel.CreateDate = rightSection.CreateDate;
                    objModel.IsFacebookSharingInd = rightSection.IsFacebookSharingInd.HasValue ? rightSection.IsFacebookSharingInd.Value : false;
                    objModel.IsGooglePlusSharingInd = rightSection.IsGooglePlusSharingInd.HasValue ? rightSection.IsGooglePlusSharingInd.Value : false;
                    objModel.IsTwitterSharingInd = rightSection.IsTwitterSharingInd.HasValue ? rightSection.IsTwitterSharingInd.Value : false;
                    objModel.ParentRightSections = objContext.RightSections.Where(x => x.ListingID == rightSection.RightSectionID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID == null).ToList();
                    objModel.RightSectionCreateDate = rightSection.RightSectionCreateDate.Value;
                }

                return View("RightSectionDetails", objModel);
            }
            #endregion

            else
            {
                if (ContentUrl.ToLower().Trim() == "sitemap")
                {
                    return View("SiteMap");
                }
                else if (ContentUrl == "error404")
                {
                    return View("GenericError");
                }
                else
                {
                    return Redirect(AppPath + "/error404");
                }
            }
        }

        private int GetMenuID(long ContentTypeID)
        {
            int MenuID = 0;
            if (ContentTypeID == Convert.ToInt32(ContentTypeAlias.AboutKISDMenu) || ContentTypeID == Convert.ToInt32(ContentTypeAlias.AboutKISDSubMenu))
            {
                MenuID = Convert.ToInt32(ContentTypeAlias.AboutKISD);
            }

            if (ContentTypeID == Convert.ToInt32(ContentTypeAlias.SchoolMenu) || ContentTypeID == Convert.ToInt32(ContentTypeAlias.SchoolSubMenu))
            {
                MenuID = Convert.ToInt32(ContentTypeAlias.School);
            }

            if (ContentTypeID == Convert.ToInt32(ContentTypeAlias.NewToKISDMenu) || ContentTypeID == Convert.ToInt32(ContentTypeAlias.NewToKISDSubMenu) || ContentTypeID == Convert.ToInt32(ContentTypeAlias.ExceptionalOpportunities))
            {
                MenuID = Convert.ToInt32(ContentTypeAlias.NewToKISD);
            }

            if (ContentTypeID == Convert.ToInt32(ContentTypeAlias.ParentStudentsMenu) || ContentTypeID == Convert.ToInt32(ContentTypeAlias.ParentStudentsSubMenu))
            {
                MenuID = Convert.ToInt32(ContentTypeAlias.ParentStudents);
            }

            if (ContentTypeID == Convert.ToInt32(ContentTypeAlias.DepartmentsMenu) || ContentTypeID == Convert.ToInt32(ContentTypeAlias.DepartmentsSubMenu))
            {
                MenuID = Convert.ToInt32(ContentTypeAlias.Departments);
            }

            if (ContentTypeID == Convert.ToInt32(ContentTypeAlias.SchoolBoardMenu) || ContentTypeID == Convert.ToInt32(ContentTypeAlias.SchoolBoardSubMenu) || ContentTypeID == Convert.ToInt32(ContentTypeAlias.BoardMembers))
            {
                MenuID = Convert.ToInt32(ContentTypeAlias.SchoolBoard);
            }
            return MenuID;
        }

        public ActionResult HeaderSection()
        {
            var objContent = new ContentModel();
            objContent.ContentList = objContext.Contents.Where(x => x.StatusInd == true && x.IsDeletedInd == false).OrderBy(x => x.ContentTypeID).ToList();
            objContent.DescriptionTxt = objContext.Contents.Where(x => x.StatusInd == true && x.ContentTypeID == 2).Select(x => x.DescriptionTxt).FirstOrDefault();
            return View(objContent);
        }

        public ActionResult FooterSection()
        {
            var objContent = new ContentModel();
            int ContentType = Convert.ToInt32(ContentTypeAlias.Footer);
            var data = objContext.Contents.Where(x => x.StatusInd == true && x.IsDeletedInd == false && x.ContentTypeID == ContentType).Select(x => new { x.DescriptionTxt }).FirstOrDefault();
            if (data != null)
            {
                objContent.DescriptionTxt = data.DescriptionTxt == null ? "" : data.DescriptionTxt;
            }
            return View(objContent);
        }

        public ActionResult RightSection(long ListingID, string ModuleType)
        {
            ListingRightSectionModel model = new ListingRightSectionModel();

            if (ModuleType == TypeMasterAlias.Content.ToString())
            {
                var content = objContext.Contents.Where(x => x.ContentID == ListingID && x.StatusInd == true && x.IsDeletedInd == false).FirstOrDefault();
                if (content != null)
                {
                    model.RightSectionTitleTxt = content.RightSectionTitleTxt;
                    model.RightSectionAbstractTxt = content.RightSectionAbstractTxt;
                    model.ParentRightSections = objContext.RightSections.Where(x => x.ListingID == content.ContentID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID == null).ToList();
                    model.ChildRightSections = objContext.RightSections.Where(x => x.ListingID == content.ContentID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID != null).ToList();
                    model.IsFacebookSharingInd = content.IsFacebookSharingInd.HasValue ? content.IsFacebookSharingInd.Value : false;
                    model.IsGooglePlusSharingInd = content.IsGooglePlusSharingInd.HasValue ? content.IsGooglePlusSharingInd.Value : false;
                    model.IsTwitterSharingInd = content.IsTwitterSharingInd.HasValue ? content.IsTwitterSharingInd.Value : false;
                    model.PageTitleTxt = content.PageTitleTxt;
                    model.PageURLTxt = content.PageURLTxt;
                }
            }

            if (ModuleType == TypeMasterAlias.BoardOfMembers.ToString())
            {
                var listing = objContext.BoardOfMembers.Where(x => x.BoardMemberID == ListingID && x.StatusInd == true && x.IsDeletedInd == false).FirstOrDefault();
                if (listing != null)
                {
                    model.RightSectionTitleTxt = listing.RightSectionTitleTxt;
                    model.RightSectionAbstractTxt = listing.RightSectionAbstractTxt;
                    model.ParentRightSections = objContext.RightSections.Where(x => x.ListingID == listing.BoardMemberID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID == null).ToList();
                    model.ChildRightSections = objContext.RightSections.Where(x => x.ListingID == listing.BoardMemberID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID != null).ToList();
                    model.IsFacebookSharingInd = false;
                    model.IsGooglePlusSharingInd = false;
                    model.IsTwitterSharingInd = false;
                    model.PageTitleTxt = listing.TitleTxt;
                    model.PageURLTxt = listing.URLTxt;
                }
            }

            if (ModuleType == TypeMasterAlias.Department.ToString())
            {
                var listing = objContext.Departments.Where(x => x.DepartmentID == ListingID && x.StatusInd == true && x.IsDeletedInd == false).FirstOrDefault();
                if (listing != null)
                {
                    model.RightSectionTitleTxt = listing.RightSectionTitleTxt;
                    model.RightSectionAbstractTxt = listing.RightSectionAbstractTxt;
                    model.ParentRightSections = objContext.RightSections.Where(x => x.ListingID == listing.DepartmentID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID == null).ToList();
                    model.ChildRightSections = objContext.RightSections.Where(x => x.ListingID == listing.DepartmentID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID != null).ToList();
                    model.IsFacebookSharingInd = false;
                    model.IsGooglePlusSharingInd = false;
                    model.IsTwitterSharingInd = false;
                    model.PageTitleTxt = listing.NameTxt;
                    model.PageURLTxt = listing.URLTxt;
                }
            }

            if (ModuleType == TypeMasterAlias.News.ToString())
            {
                var listing = objContext.NewsEvents.Where(x => x.NewsEventID == ListingID && x.StatusInd == true && x.IsDeletedInd == false).FirstOrDefault();
                if (listing != null)
                {
                    model.RightSectionTitleTxt = listing.RightSectionTitleTxt;
                    model.RightSectionAbstractTxt = listing.RightSectionAbstractTxt;
                    model.ParentRightSections = objContext.RightSections.Where(x => x.ListingID == listing.NewsEventID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID == null).ToList();
                    model.ChildRightSections = objContext.RightSections.Where(x => x.ListingID == listing.NewsEventID && x.StatusInd == true && x.IsDeletedInd == false && x.ParentID != null).ToList();
                    model.IsFacebookSharingInd = false;
                    model.IsGooglePlusSharingInd = false;
                    model.IsTwitterSharingInd = false;
                    model.PageTitleTxt = listing.TitleTxt;
                    model.PageURLTxt = listing.PageURLTxt;
                }
            }

            if (ModuleType == "RightSection")
            {
                var listing = objContext.RightSections.Where(x => x.RightSectionID == ListingID && x.StatusInd == true && x.IsDeletedInd == false).FirstOrDefault();
                if (listing != null)
                {
                    model.RightSectionTitleTxt = listing.RightSectionTitleTxt;
                    model.RightSectionAbstractTxt = listing.RightSectionAbstractTxt;
                    model.ParentRightSections = null;
                    model.ChildRightSections = null;
                    model.IsFacebookSharingInd = listing.IsFacebookSharingInd.HasValue ? listing.IsFacebookSharingInd.Value : false;
                    model.IsGooglePlusSharingInd = listing.IsGooglePlusSharingInd.HasValue ? listing.IsGooglePlusSharingInd.Value : false;
                    model.IsTwitterSharingInd = listing.IsTwitterSharingInd.HasValue ? listing.IsTwitterSharingInd.Value : false;
                    model.PageTitleTxt = listing.TitleTxt;
                    model.PageURLTxt = listing.ExternalLinkURLTxt;
                }
            }

            return View(model);
        }

        public ActionResult ExceptionalOppertunity()
        {
            ExceptionOpportunityModel EOModel = new ExceptionOpportunityModel();
            ExceptionOpportunityService _service = new ExceptionOpportunityService();
            EOModel.Categories = _service.GetAllHomeSchoolCategories();
            EOModel.ExceptionOpportunities = objContext.ExceptionOpportunities.Where(x => x.IsDeletedInd == false
                      && x.StatusInd == true).ToList();
            return View(EOModel);
        }

        public string GetListingHTML(string ListingParameter, string Description)
        {
            string html = string.Empty;
            string LParameter = ListingParameter.Replace("#", "");

            if (LParameter == Areas.Admin.Models.Common.ListingParameters.FAQ.ToString())
            {
                Description = Description.Replace(ListingParameter, "@{Html.RenderPartial('ListingParameterListing');}");
            }

            if (LParameter == Areas.Admin.Models.Common.ListingParameters.FAQByCategory.ToString())
            {

            }

            if (LParameter == Areas.Admin.Models.Common.ListingParameters.PhotoGallery.ToString())
            {

            }

            if (LParameter == Areas.Admin.Models.Common.ListingParameters.PhotoGalleryByCategory.ToString())
            {

            }

            if (LParameter == Areas.Admin.Models.Common.ListingParameters.PodCast.ToString())
            {
                int TypeMasterID = 1;
                var data = objContext.GalleryListings.Where(x => x.TypeMasterID == TypeMasterID && x.IsDeletedInd == false && x.StatusInd == true).ToList();
                if (data != null)
                {
                    foreach (var d in data)
                    {
                        html += @"<table><tr>";
                    }
                }
            }

            if (LParameter == Areas.Admin.Models.Common.ListingParameters.Video.ToString())
            {

            }

            return Description;
        }

        #region Breadcrumb Functions
        public void SetMainParentBreadcrumbs(int ParentID)
        {
            var Data = (from db in objContext.Contents
                        where db.ContentTypeID == ParentID
                        select new { db.PageURLTxt, db.MenuTitleTxt, db.IsExternalLinkInd, db.ExternalLinkTxt, db.ExternalLinkTargetInd }).FirstOrDefault();
            if (Data != null)
            {
                ViewBag.MainURL = Data.IsExternalLinkInd ? Data.ExternalLinkTxt : Data.PageURLTxt;
                ViewBag.MainParentName = Data.MenuTitleTxt;
                ViewBag.MainParentTargetWindow = Data.ExternalLinkTargetInd ? "_blank" : "_self";
            }
        }

        public void SetSecondParentBreadcrumbs(int ParentID)
        {
            var Data = (from db in objContext.Contents
                        where db.ContentTypeID == ParentID
                        select new { db.PageURLTxt, db.MenuTitleTxt, db.IsExternalLinkInd, db.ExternalLinkTxt, db.ExternalLinkTargetInd, db.PageTitleTxt }).FirstOrDefault();
            if (Data != null)
            {
                ViewBag.ParentURL = Data.IsExternalLinkInd ? Data.ExternalLinkTxt : Data.PageURLTxt;
                ViewBag.ParentName = !string.IsNullOrEmpty(Data.MenuTitleTxt) ? Data.MenuTitleTxt : Data.PageTitleTxt;
                ViewBag.MainParentTargetWindow = Data.ExternalLinkTargetInd ? "_blank" : "_self";
            }
        }

        public void SetDepartmentBreadcrumbs(long DeptID)
        {
            var Data = (from db in objContext.Departments
                        where db.DepartmentID == DeptID
                        select new { db.NameTxt, db.URLTxt }).FirstOrDefault();
            if (Data != null)
            {
                ViewBag.MainURL = Data.URLTxt;
                ViewBag.MainParentName = Data.NameTxt;
            }
        }

        public void SetPhotoCategoryBreadcrumbs(int ParentID)
        {
            var TypeMasterID = Convert.ToInt32(TypeMasterAlias.PhotoGallery);
            var Data = (from db in objContext.GalleryListings
                        where db.ListingID == ParentID
                        select db).FirstOrDefault();
            if (Data != null)
            {
                ViewBag.MainURL = Data.URLTxt;
                ViewBag.MainParentName = Data.TitleTxt;
            }
        }
        #endregion

        #region Event Calendar
        #region Get Calendar data method.
        public ActionResult GetCalendarData(string DepartmentID)//string DepartmentID
        {
            JsonResult result = new JsonResult();

            try
            {
                List<EventCalendar> data = this.LoadData(Convert.ToInt64(DepartmentID));
                result = this.Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
               
            }
            return result;
        }

        #endregion

        #region Load Data

        /// <summary>
        /// Load data method.
        /// </summary>
        /// <returns>Returns - Data</returns>
        private List<EventCalendar> LoadData(long DepartmentID)
        {
            List<EventCalendar> lst = new List<EventCalendar>();

            try
            {
                string line = string.Empty;
                var TypeMasterID = Convert.ToInt32(TypeMasterAlias.ManageEvents);
                var DepartmentEvents = objContext.NewsEvents.Where(x => x.TypeMasterID == TypeMasterID
                                       && x.IsDeletedInd == false && x.StatusInd == true
                                       && x.DepartmentID == DepartmentID).ToList();

                if (DepartmentEvents != null)
                {
                    for (int i = 0; i < DepartmentEvents.Count; i++)
                    {
                        if (!DepartmentEvents[i].IsRecurringInd != null)
                        {
                            if (!DepartmentEvents[i].IsRecurringInd.Value)
                            {
                                EventCalendar infoObj = new EventCalendar();
                                infoObj.Sr = i;
                                infoObj.Title = DepartmentEvents[i].TitleTxt;
                                infoObj.Desc = @"<ul class='model-desc'>
                                                 <li><strong>Event Date:</strong> " + DepartmentEvents[i].EventDate.Value.ToString("dd MMMM, yyyy hh:mm tt") + "</li>" +
                                                 "<li><strong>Is Recurring Event:</strong> No</li>" +
                                                 "</ul>";
                                infoObj.Start_Date = DepartmentEvents[i].EventDate.ToString();
                                infoObj.End_Date = DepartmentEvents[i].EventDate.Value.ToString();
                                lst.Add(infoObj);
                            }
                            else
                            {
                                DateTime EventEndDate = DepartmentEvents[i].EventsEndOnDate.Value;
                                //if event is daily
                                #region Daily Events
                                if (DepartmentEvents[i].RecurrenceTypeNbr == 1)
                                {
                                    EventCalendar infoObj = new EventCalendar();
                                    infoObj.Sr = i;
                                    infoObj.Title = DepartmentEvents[i].TitleTxt;
                                    infoObj.Desc = @"<ul class='model-desc'><li><strong>Start Date: </strong>" + DepartmentEvents[i].EventDate.Value.ToString("dd MMMM, yyyy hh:mm tt") + "</li>" +
                                                     "<li><strong>End Date: </strong> " + EventEndDate.ToString("dd MMMM, yyyy hh:mm tt") + "</li>" +
                                                     "<li><strong>Is Recurring Event: </strong>Yes</li>" +
                                                     "<li><strong>Recurring Type: </strong> Daily</li>" +
                                                     "</ul>";
                                    infoObj.Start_Date = DepartmentEvents[i].EventDate.Value.ToString("dd MMMM, yyyy hh:mm tt");
                                    //infoObj.End_Date = EventEndDate.AddDays(1).ToString("dd MMMM, yyyy hh:mm tt");
                                    infoObj.End_Date = EventEndDate.ToString("dd MMMM, yyyy hh:mm tt");
                                    lst.Add(infoObj);
                                }
                                #endregion

                                //if event is Weekly
                                #region Weekly Events
                                if (DepartmentEvents[i].RecurrenceTypeNbr == 2)
                                {
                                    string WeekDays = string.Empty;
                                    var start = Convert.ToDateTime(DepartmentEvents[i].EventDate.Value.ToShortDateString());
                                    var end = Convert.ToDateTime(DepartmentEvents[i].EventsEndOnDate.Value.ToShortDateString());
                                    int numberOfDays = end.Subtract(start).Days + 1;

                                    List<DayOfWeek> dayOfWeek = new List<DayOfWeek>();
                                    if (DepartmentEvents[i].IsSundayInd.Value)
                                    {
                                        dayOfWeek.Add(DayOfWeek.Sunday);
                                        WeekDays += DayOfWeek.Sunday.ToString() + ",";
                                    }
                                    if (DepartmentEvents[i].IsMondayInd.Value)
                                    {
                                        dayOfWeek.Add(DayOfWeek.Monday);
                                        WeekDays += DayOfWeek.Monday.ToString() + ",";
                                    }
                                    if (DepartmentEvents[i].IsTuesdayInd.Value)
                                    {
                                        dayOfWeek.Add(DayOfWeek.Tuesday);
                                        WeekDays += DayOfWeek.Tuesday.ToString() + ",";
                                    }
                                    if (DepartmentEvents[i].IsWednesdayInd.Value)
                                    {
                                        dayOfWeek.Add(DayOfWeek.Wednesday);
                                        WeekDays += DayOfWeek.Wednesday.ToString() + ",";
                                    }
                                    if (DepartmentEvents[i].IsThursdayInd.Value)
                                    {
                                        dayOfWeek.Add(DayOfWeek.Thursday);
                                        WeekDays += DayOfWeek.Thursday.ToString() + ",";
                                    }
                                    if (DepartmentEvents[i].IsFridayInd.Value)
                                    {
                                        dayOfWeek.Add(DayOfWeek.Friday);
                                        WeekDays += DayOfWeek.Friday.ToString() + ",";
                                    }
                                    if (DepartmentEvents[i].IsSaturdayInd.Value)
                                    {
                                        dayOfWeek.Add(DayOfWeek.Saturday);
                                        WeekDays += DayOfWeek.Saturday.ToString() + ",";
                                    }
                                    WeekDays = WeekDays.Substring(0, WeekDays.Length - 1);

                                    var dates = Enumerable.Range(0, numberOfDays)
                                                          .Select(p => start.AddDays(p))
                                                          .Where(d => dayOfWeek.Contains(d.DayOfWeek));

                                    if (dates != null)
                                    {
                                        foreach (var day in dates)
                                        {
                                            var startDateTime = day.ToShortDateString() + " " + DepartmentEvents[i].EventDate.Value.ToString("hh:mm tt");
                                            var endDateTime = day.ToShortDateString() + " " + DepartmentEvents[i].EventsEndOnDate.Value.ToString("hh:mm tt");

                                            EventCalendar infoObj = new EventCalendar();
                                            infoObj.Sr = i;
                                            infoObj.Title = DepartmentEvents[i].TitleTxt;
                                            infoObj.Desc = @"<ul class='model-desc'><li><strong>Start Date: </strong>" + DepartmentEvents[i].EventDate.Value.ToString("dd MMMM, yyyy hh:mm tt") + "</li>" +
                                                              "<li><strong>End Date: </strong>" + EventEndDate.ToString("dd MMMM, yyyy hh:mm tt") + "</li>" +
                                                              "<li><strong>Is Recurring Event: </strong> Yes</li>" +
                                                              "<li><strong>Recurring Type: </strong> Weekly</li>" +
                                                              "<li><strong>Week Days: </strong>" + WeekDays + "</li>" +
                                                              "</ul>";
                                            infoObj.Start_Date = startDateTime;
                                            infoObj.End_Date = endDateTime;
                                            lst.Add(infoObj);
                                        }
                                    }
                                }
                                #endregion

                                //if event is Monthly
                                #region Monthly Events
                                if (DepartmentEvents[i].RecurrenceTypeNbr == 3)
                                {
                                    var start = Convert.ToDateTime(DepartmentEvents[i].EventDate.Value.ToString("MM/dd/yyyy hh:mm tt"));
                                    var end = Convert.ToDateTime(DepartmentEvents[i].EventsEndOnDate.Value.ToString("MM/dd/yyyy hh:mm tt"));

                                    //one week day
                                    #region One Week Day
                                    if (DepartmentEvents[i].MonthondayNbr != null && DepartmentEvents[i].MonthonNbr != null)
                                    {
                                        var date = DepartmentEvents[i].MonthondayNbr.Value;
                                        var day = DepartmentEvents[i].MonthonNbr.Value;
                                        var _DayOfWeek =
                                            DepartmentEvents[i].MonthonNbr.Value == 1 ? DayOfWeek.Sunday
                                            : (DepartmentEvents[i].MonthonNbr.Value == 2 ? DayOfWeek.Monday
                                            : (DepartmentEvents[i].MonthonNbr.Value == 3 ? DayOfWeek.Tuesday
                                            : (DepartmentEvents[i].MonthonNbr.Value == 4 ? DayOfWeek.Wednesday
                                            : (DepartmentEvents[i].MonthonNbr.Value == 5 ? DayOfWeek.Thursday
                                            : (DepartmentEvents[i].MonthonNbr.Value == 6 ? DayOfWeek.Friday
                                            : DayOfWeek.Saturday)))));

                                        var data = GetNthDayOfMonth(start, end, _DayOfWeek);
                                        if (data != null)
                                        {
                                            foreach (var d in data)
                                            {
                                                if (Convert.ToDateTime(d) > start)
                                                {
                                                    EventCalendar infoObj = new EventCalendar();
                                                    infoObj.Sr = i;
                                                    infoObj.Title = DepartmentEvents[i].TitleTxt;
                                                    infoObj.Desc = @"<ul class='model-desc'><li><strong>Start Date: </strong>" + DepartmentEvents[i].EventDate.Value.ToString("dd MMMM, yyyy hh:mm tt") + "</li>" +
                                                                      "<li><strong>End Date: </strong>" + EventEndDate.ToString("dd MMMM, yyyy hh:mm tt") + "</li>" +
                                                                      "<li><strong>Is Recurring Event: </strong> Yes</li>" +
                                                                      "<li><strong>Recurring Type: </strong> Monthly</li>" +
                                                                      "</ul>";
                                                    infoObj.Start_Date = d;
                                                    infoObj.End_Date = d;
                                                    lst.Add(infoObj);
                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    //one date
                                    #region One Date
                                    if (DepartmentEvents[i].MonthdayNbr != null)
                                    {
                                        if (DepartmentEvents[i].MonthdayNbr.Value > 0)
                                        {
                                            // set end-date to end of month
                                            end = new DateTime(end.Year, end.Month, DateTime.DaysInMonth(end.Year, end.Month));

                                            var dates = Enumerable.Range(0, Int32.MaxValue)
                                                                 .Select(e => start.AddMonths(e))
                                                                 .TakeWhile(e => e <= end)
                                                                 .Select(e => e.ToString("MM/" + DepartmentEvents[i].MonthdayNbr.Value + "/yyyy hh:mm tt"));

                                            if (dates != null)
                                            {
                                                foreach (var d in dates)
                                                {
                                                    if (Convert.ToDateTime(d) > start)
                                                    {
                                                        EventCalendar infoObj = new EventCalendar();
                                                        infoObj.Sr = i;
                                                        infoObj.Title = DepartmentEvents[i].TitleTxt;
                                                        infoObj.Desc = @"<ul class='model-desc'><li><strong>Start Date: </strong>" + DepartmentEvents[i].EventDate.Value.ToString("dd MMMM, yyyy hh:mm tt") + "</li>" +
                                                                          "<li><strong>End Date: </strong>" + EventEndDate.ToString("dd MMMM, yyyy hh:mm tt") + "</li>" +
                                                                          "<li><strong>Is Recurring Event: </strong> Yes</li>" +
                                                                          "<li><strong>Recurring Type: </strong> Monthly</li>" +
                                                                          "</ul>";
                                                        infoObj.Start_Date = d.ToString();
                                                        infoObj.End_Date = d.ToString();
                                                        lst.Add(infoObj);
                                                    }
                                                }
                                            }
                                        }
                                    } 
                                    #endregion
                                }
                                #endregion

                                //if event is Yearly
                                #region Yearly Events
                                if (DepartmentEvents[i].RecurrenceTypeNbr == 4)
                                {
                                    var start = Convert.ToDateTime(DepartmentEvents[i].EventDate.Value.ToString("MM/dd/yyyy hh:mm"));
                                    var end = Convert.ToDateTime(DepartmentEvents[i].EventsEndOnDate.Value);
                                    for (int j = start.Year; j <= end.Year; j++)
                                    {
                                        var EventDate = start.Month + "/" + start.Day + "/" + j + " " + start.Hour + ":" + start.Minute + " " + start.ToString("tt");
                                        EventCalendar infoObj = new EventCalendar();
                                        infoObj.Sr = i;
                                        infoObj.Title = DepartmentEvents[i].TitleTxt;
                                        infoObj.Desc = @"<ul class='model-desc'><li><strong>Start Date: </strong>" + DepartmentEvents[i].EventDate.Value.ToString("dd MMMM, yyyy hh:mm tt") + "</li>" +
                                                          "<li><strong>End Date: </strong>" + EventEndDate.ToString("dd MMMM, yyyy hh:mm tt") + "</li>" +
                                                          "<li><strong>Is Recurring Event: </strong>Yes</li>" +
                                                          "<li><strong>Recurring Type: </strong>Yearly</li>" +
                                                          "</ul>";
                                        infoObj.Start_Date = EventDate;
                                        infoObj.End_Date = EventDate;
                                        lst.Add(infoObj);
                                    }
                                }
                                #endregion

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            return lst;
        }

        #endregion

        public List<string> GetNthDayOfMonth(DateTime startDate, DateTime endDate, DayOfWeek _DayOfWeek)
        {
            List<DateTime> dates = new List<DateTime>();
            if (startDate < endDate)
            {
                for (DateTime day = startDate.Date; day.Date <= endDate.Date; day = day.AddDays(1))
                {
                    dates.Add(day);
                }
            }
            DateTime[] mondays = dates.Where(d => d.DayOfWeek == _DayOfWeek).ToArray();
            List<string> startMondays = new List<string>();
            int month = 0;
            for (int i = 0; i < mondays.Length; i++)
            {
                if (month != mondays[i].Month)
                {
                    month = mondays[i].Month;
                    startMondays.Add(mondays[i].ToString("MM/dd/yyyy hh:mm tt"));
                }
            }

            return startMondays;
        }
        #endregion
    }
}