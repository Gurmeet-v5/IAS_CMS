using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImageTypeAlias = KISD.Areas.Admin.Models.ImageService.ImageType;
using ContentTypeAlias = KISD.Areas.Admin.Models.ContentType;
using TypeMasterAlias = KISD.Areas.Admin.Models.GalleryListingService.TypeMaster;
using KISD.Areas.Admin.Models;

namespace KISD.Controllers
{
    public class HomeController : Controller
    {
        db_KISDEntities objContext = new db_KISDEntities();
        public ActionResult Index()
        {
            int TypeMasterID = Convert.ToInt32(TypeMasterAlias.OnscreenAlert);
            var obj = objContext.Announcements.Where(x => x.TypeMasterID == TypeMasterID
                      && x.IsDeletedInd == false && x.StatusInd == true).OrderByDescending(x => x.AnnouncementCreateDate).Take(3).ToList();
            if (obj != null)
            {
                string html = string.Empty;
                foreach (var s in obj)
                {
                    html += @"<div class='alert-div'><div class='container'><span class='alerticon'></span>" + s.TitleTxt + "</div></div>";
                }
                ViewBag.TopAlertMessages = html;
            }

            var HomeContentType = Convert.ToInt64(ContentTypeAlias.Home);
            var HomeContent = objContext.Contents.Where(x => x.ContentTypeID == HomeContentType).FirstOrDefault();
            
            if (HomeContent != null)
            {
                ViewBag.Title = HomeContent.PageMetaTitleTxt;
                ViewBag.MetaDescription = HomeContent.PageMetaDescriptionTxt;
            }

            return View();
        }

        public ActionResult BannerImages()
        {
            int ImageType = Convert.ToInt32(ImageTypeAlias.BannerImage);
            var obj = objContext.Images.Where(x => x.ImageTypeID == ImageType && x.StatusInd == true && x.IsDeletedInd == false).OrderByDescending(x => x.CreateDate).ToList();
            return View(obj);
        }

        public ActionResult ImportantInfoImages()
        {
            int ImageType = Convert.ToInt32(ImageTypeAlias.ImportantInfo);
            var objWithDate = objContext.Images.Where(x => x.ImageTypeID == ImageType && x.StatusInd == true
                      && x.IsDeletedInd == false && x.DisplayStartDate.Value <= DateTime.Now
                      && x.DisplayEndDate.Value >= DateTime.Now).OrderBy(x => x.DisplayOrderNbr).ToList();

            var objWithoutDate = objContext.Images.Where(x => x.ImageTypeID == ImageType && x.StatusInd == true
                     && x.IsDeletedInd == false && x.DisplayStartDate.Value == null
                     && x.DisplayEndDate.Value == null).OrderBy(x => x.DisplayOrderNbr).ToList();

            var obj = objWithDate.Union(objWithoutDate);
            return View(obj);
        }

        public ActionResult IconsImages()
        {
            int ImageType = Convert.ToInt32(ImageTypeAlias.Icon);
            var obj = objContext.Images.Where(x => x.ImageTypeID == ImageType && x.StatusInd == true
                      && x.IsDeletedInd == false).OrderBy(x => x.DisplayOrderNbr).ToList();
            return View(obj);
        }

        public ActionResult News()
        {
            int TypeMasterID = Convert.ToInt32(TypeMasterAlias.News);
            var objWithDates = objContext.NewsEvents.Where(x => x.StatusInd == true
                      && (x.IsDeletedInd == false || x.IsDeletedInd == null) && x.TypeMasterID == TypeMasterID
                      && x.ShowOnHomeInd == true && x.DisplayStartDate.Value <= DateTime.Now
                      && x.DisplayEndDate.Value >= DateTime.Now).OrderByDescending(x => x.EventCreateDate).ToList();

            var objWithoutDates = objContext.NewsEvents.Where(x => x.StatusInd == true
                     && (x.IsDeletedInd == false || x.IsDeletedInd == null) && x.TypeMasterID == TypeMasterID
                     && x.ShowOnHomeInd == true && x.DisplayStartDate.Value == null
                     && x.DisplayEndDate.Value == null).OrderByDescending(x => x.EventCreateDate).ToList();

            var obj = objWithDates.Union(objWithoutDates);

            int HomeRightSectionContentType = Convert.ToInt32(ContentTypeAlias.HomeRightSection);
            var data = objContext.Contents.Where(x => x.ContentTypeID == HomeRightSectionContentType).Select(x => x.DescriptionTxt).FirstOrDefault();
            ViewBag.HomeRightSection = data;

            int NewsContentType = Convert.ToInt32(ContentTypeAlias.News);
            var NewsData = objContext.Contents.Where(x => x.ContentTypeID == NewsContentType).Select(x => x.PageURLTxt).FirstOrDefault();
            ViewBag.NewsURL = NewsData;

            return View(obj);
        }

        public ActionResult Event()
        {
            int TypeMasterID = Convert.ToInt32(TypeMasterAlias.Events);
            var objWithDate = objContext.NewsEvents.Where(x => x.StatusInd == true
                      && (x.IsDeletedInd == false || x.IsDeletedInd == null)
                      && x.ShowOnHomeInd == true && x.TypeMasterID == TypeMasterID
                      && x.DisplayStartDate.Value <= DateTime.Now
                      && x.DisplayEndDate.Value >= DateTime.Now).OrderByDescending(x => x.EventDate).ToList();

            var objWithoutDate = objContext.NewsEvents.Where(x => x.StatusInd == true
                     && (x.IsDeletedInd == false || x.IsDeletedInd == null)
                     && x.ShowOnHomeInd == true && x.TypeMasterID == TypeMasterID
                     && x.DisplayStartDate.Value == null
                     && x.DisplayEndDate.Value == null).OrderByDescending(x => x.EventDate).ToList();

            var obj = objWithDate.Union(objWithoutDate);

            int NewToKISDContentType = Convert.ToInt32(ContentTypeAlias.NewToKISD);
            int EventsContentType = Convert.ToInt32(ContentTypeAlias.Events);

            ViewBag.MoreEventsURL = objContext.Contents.Where(x => x.ContentTypeID == EventsContentType).Select(x => x.PageURLTxt).FirstOrDefault();
            ViewBag.NewToKISDURL = objContext.Contents.Where(x => x.ContentTypeID == NewToKISDContentType).Select(x => x.PageURLTxt).FirstOrDefault();

            return View(obj);
        }

        public ActionResult HomePageData()
        {
            int ContentType = Convert.ToInt32(ContentTypeAlias.Home);
            var obj = objContext.Contents.Where(x => x.ContentTypeID == ContentType).FirstOrDefault();
            return View(obj);
        }

        public ActionResult Announcements()
        {
            int TypeMasterID = Convert.ToInt32(TypeMasterAlias.Announcement);
            var objWithDate = objContext.Announcements.Where(x => x.IsDeletedInd == false
                      && x.StatusInd == true && x.DisplayStartDate.Value <= DateTime.Now
                      && x.DisplayEndDate.Value >= DateTime.Now && x.TypeMasterID == TypeMasterID)
                      .OrderByDescending(x => x.AnnouncementCreateDate).ToList();

            var objWithoutDate = objContext.Announcements.Where(x => x.IsDeletedInd == false
                     && x.StatusInd == true && x.DisplayStartDate.Value <= DateTime.Now
                     && x.DisplayEndDate.Value >= DateTime.Now && x.TypeMasterID == TypeMasterID)
                     .OrderByDescending(x => x.AnnouncementCreateDate).ToList();
            var obj = objWithDate.Union(objWithoutDate);
            return View(obj);
        }

        public ActionResult KISDTV()
        {
            int TypeMasterID = Convert.ToInt32(TypeMasterAlias.Video);
            var objWithDate = objContext.GalleryListings.Where(x => x.TypeMasterID == TypeMasterID
                      && x.IsDeletedInd == false && x.StatusInd == true && x.ShowOnHomeInd == true
                      && x.DisplayStartDate.Value <= DateTime.Now
                      && x.DisplayEndDate.Value >= DateTime.Now).OrderBy(x => x.DisplayOrderNbr).ToList();

            var objWithoutDate = objContext.GalleryListings.Where(x => x.TypeMasterID == TypeMasterID
                     && x.IsDeletedInd == false && x.StatusInd == true && x.ShowOnHomeInd == true
                     && x.DisplayStartDate.Value == null
                     && x.DisplayEndDate.Value == null).OrderBy(x => x.DisplayOrderNbr).ToList();

            var obj = objWithDate.Union(objWithoutDate);
            return View(obj);
        }

        public ActionResult ExceptionalOppertunity()
        {
            ExceptionOpportunityModel EOModel = new ExceptionOpportunityModel();
            ExceptionOpportunityService _service = new ExceptionOpportunityService();
            EOModel.Categories = _service.GetAllHomeSchoolCategories();
            EOModel.ExceptionOpportunities = objContext.ExceptionOpportunities.Where(x => x.IsDeletedInd == false
                      && x.StatusInd == true && x.ShowOnHomeInd == true).OrderByDescending(x => x.ExOpportunityCreateDate).ToList();
            return View(EOModel);
        }

        public ActionResult HomePageBottomData()
        {
            int ContentType = Convert.ToInt32(ContentTypeAlias.Home);
            var obj = objContext.Contents.Where(x => x.ContentTypeID == ContentType).FirstOrDefault();
            return View(obj);
        }
    }
}