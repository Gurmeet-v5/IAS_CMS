using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using static KISD.Areas.Admin.Models.Common;

namespace KISD.Areas.Admin.Models
{
    /// <summary>
    /// Public enum method for getting the Content Type.
    /// These values must match with the values added in ContentType table in database.
    /// </summary>
    public enum ContentType : int
    {
        Home = 1,
        HomeMenu = 2,
        HomeSubMenu = 3,
        Syllabus = 4,
        SyllabusMenu = 5,
        SyllabusSubMenu = 6,
        DailyNews = 7,
        DailyNewsMenu = 8,
        DailyNewsSubMenu = 9,
        Downloads = 10,
        DownloadsMenu = 11,
        DownloadsSubMenu = 12,
        AboutUs = 13,
        AboutUsMenu = 14,
        AboutUsSubMenu = 15,
        Video = 16,
        VideoMenu = 17,
        VideoSubMenu = 18,
        ContactUs = 19,
        ContactUsMenu = 20,
        ContactUsSubMenu = 21,
        Header = 22,
        Footer = 23,
        Search = 24,
        Fly = 25,
        HomePageRightSection = 26,
        GenericError = 27,
    }

    public class ContentModel
    {
        public long ContentID { get; set; }
        public Nullable<long> ParentID { get; set; }
        public long ContentTypeID { get; set; }
        public bool IsExternalLinkInd { get; set; }
        public string ExternalLinkTxt { get; set; }
        public bool ExternalLinkTargetInd { get; set; }
        public string PageTitleTxt { get; set; }
        public string MenuTitleTxt { get; set; }
        public string PageURLTxt { get; set; }
        public string AbstractTxt { get; set; }
        public string DescriptionTxt { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public Nullable<long> DisplayOrderNbr { get; set; }
        public Nullable<long> BannerImageID { get; set; }
        public string AltBannerImageTxt { get; set; }
        public string BannerImageAbstractTxt { get; set; }
        public bool StatusInd { get; set; }
        public string PageMetaTitleTxt { get; set; }
        public string PageMetaDescriptionTxt { get; set; }
        public string RightSectionTitleTxt { get; set; }
        public string RightSectionAbstractTxt { get; set; }
        public Nullable<DateTime> ContentCreateDate { get; set; }
        public Nullable<DateTime> CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }
        public int TypeMasterID { get; set; }
        public bool IsFacebookSharingInd { get; set; }
        public bool IsTwitterSharingInd { get; set; }
        public bool IsGooglePlusSharingInd { get; set; }
        public string ContentTypeTitle { get; set; }
        public List<Content> ContentList { get; set; }
        public List<School> SchoolList { get; set; }
        public IPagedList<NewsEvent> NewsEventResult { get; set; }
        public IPagedList<BoardOfMember> BOMResult { get; set; }
        public IPagedList<Announcement> BoardScheduleResult { get; set; }
        public IPagedList<GalleryListing> VideoResult { get; set; }
        public IPagedList<School> SchoolCategoryResult { get; set; }
        public IPagedList<Department> DepartmentResult { get; set; }
        public IPagedList<School> SchoolResult { get; set; }
        public IPagedList<Staff> StaffResult { get; set; }
        public bool IsPagingVisible { get; set; }

        public IPagedList<pr_search_all_paged_Result> SearchAllResult { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public string strCreateDate { get; set; }
        public SelectList InnerImages { get; set; }
        public string ImageTitle { get; set; }
        public SelectList DisplayOrderNbrSelect { get; set; }//Display Order
        //public IPagedList<Event> PagedEvent { get; set; }
        //public IPagedList<pr_search_all_paged_Result> SearchAllResult { get; set; }
        public SelectList StatusNumSelect { get; set; }
        public string[] SelectedRightSections { get; set; }
        public SelectList RightSections { get; set; }
        //public IEnumerable<Event> AllEventList { get; set; }
        public string ReCaptchaSiteKey { get; set; }
        public string ListingParemeterName { get; set; }
        public List<RightSection> ParentRightSections { get; set; }
        public long DepartmentID { get; set; }
        /// <summary>
        /// Get Content Type Name from Content Type ID from database.
        /// </summary>
        /// <returns>Content Type Name</returns>
        public static string GetContentTypeName(int contentTypeId)
        {
            using (db_KISDEntities objKISD = new db_KISDEntities())
            {
                return objKISD.ContentTypes.Where(x => x.ContentTypeID == contentTypeId).Select(x => x.ContentTypeNameTxt).SingleOrDefault();
            }
        }
    }
    //Display Order 
    public class MenuDisplayOrder
    {
        public int DisplayID { get; set; }
        public string DisplayOrderNbr { get; set; }
    }

    public class Status
    {
        public int StatusID { get; set; }
        public string StatusNbr { get; set; }
    }

    public enum StatusType : int
    {
        Active = 1,
        InActive = 0
    }

    public class ContentService
    {
        private db_KISDEntities _context;
        public ContentService()
        {
            _context = new db_KISDEntities();
        }
        /// <summary>
        /// Get all Menus of defined type
        /// </summary>
        /// <param name="EmailType"></param>
        /// <returns></returns>
        public IQueryable<Content> GetAllMenus(int ContentTypeId, long UserID)
        {
            var RoleID = _context.UserRoles.Where(x => x.UserID == UserID).Select(x => x.RoleID).FirstOrDefault();
            if (ContentTypeId == Convert.ToInt32(ContentType.Fly) && RoleID != Convert.ToInt32(UserType.SuperAdmin)
                    && RoleID != Convert.ToInt32(UserType.Admin))
            {
                var AllFlyPages = _context.Contents.Where(z => z.ContentTypeID == ContentTypeId && z.ParentID == null
                && (z.IsDeletedInd == false || z.IsDeletedInd == null)).ToList();

                var FlyPagesCreatedByUser = _context.Contents.Where(z => z.ContentTypeID == ContentTypeId && z.ParentID == null
                && (z.IsDeletedInd == false || z.IsDeletedInd == null)
                && (z.CreateByID == UserID || z.LastModifyByID == UserID)).ToList();

                var AllDeptList = _context.Departments.Where(x => x.IsDeletedInd == false && x.StatusInd == true).Select(x => x.DescriptionTxt).ToArray();

                var result = AllFlyPages.Where(x => AllDeptList.Contains(x.PageURLTxt)).Distinct().ToList();
                return FlyPagesCreatedByUser.Union(result).AsQueryable();

                //return _context.Contents.Where(x => x.ContentTypeID == ContentTypeId && x.ParentID == null
                //&& (x.IsDeletedInd == false || x.IsDeletedInd == null)
                //&& (x.CreateByID == UserID || x.LastModifyByID == UserID));
            }
            else
                return _context.Contents.Where(x => x.ContentTypeID == ContentTypeId && x.ParentID == null
                && (x.IsDeletedInd == false || x.IsDeletedInd == null));
        }

        public List<ContentModel> GetMenus(int contentTypeID, long UserID)
        {
            var list = new List<ContentModel>();
            foreach (var item in GetAllMenus(contentTypeID, UserID))
            {
                list.Add(new ContentModel
                {
                    ContentID = item.ContentID,
                    ContentTypeID = item.ContentTypeID,
                    AbstractTxt = item.AbstractTxt,
                    CreateDate = item.CreateDate,
                    DescriptionTxt = item.DescriptionTxt,
                    ExternalLinkTargetInd = item.ExternalLinkTargetInd,
                    ExternalLinkTxt = item.ExternalLinkTxt,
                    BannerImageID = item.BannerImageID,
                    StatusInd = item.StatusInd,
                    IsExternalLinkInd = item.IsExternalLinkInd,
                    MenuTitleTxt = item.MenuTitleTxt,
                    PageMetaDescriptionTxt = item.PageMetaDescriptionTxt,
                    PageMetaTitleTxt = item.PageMetaTitleTxt,
                    PageTitleTxt = item.PageTitleTxt,
                    AltBannerImageTxt = item.AltBannerImageTxt,
                    PageURLTxt = item.PageURLTxt,
                    DisplayOrderNbr = string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()) ? 0 : item.DisplayOrderNbr.Value,
                    DisplayOrderNbrSelect = GetDisplayOrder(item.DisplayOrderNbr.ToString(), contentTypeID),
                    StatusNumSelect = GetStatus(item.StatusInd.ToString(), contentTypeID),
                    ContentCreateDate = item.ContentCreateDate
                });
            }
            return list;
        }

        public IQueryable<Content> GetAllSubMenus(int SubMenuTypeID, int ParentId)
        {
            return _context.Contents.Where(x => x.ContentTypeID == SubMenuTypeID && x.ParentID == ParentId && (x.IsDeletedInd == false || x.IsDeletedInd == null));
        }

        public List<ContentModel> GetSubMenus(int SubMenuTypeID, int ParentId)
        {
            var list = new List<ContentModel>();
            foreach (var item in GetAllSubMenus(SubMenuTypeID, ParentId))
            {
                list.Add(new ContentModel
                {
                    ContentID = item.ContentID,
                    ContentTypeID = item.ContentTypeID,
                    AbstractTxt = item.AbstractTxt,
                    CreateDate = item.CreateDate,
                    ContentCreateDate = item.CreateDate,
                    DescriptionTxt = item.DescriptionTxt,
                    ExternalLinkTargetInd = item.ExternalLinkTargetInd,
                    ExternalLinkTxt = item.ExternalLinkTxt,
                    BannerImageID = item.BannerImageID,
                    StatusInd = item.StatusInd,
                    IsExternalLinkInd = item.IsExternalLinkInd,
                    MenuTitleTxt = item.MenuTitleTxt,
                    PageMetaDescriptionTxt = item.PageMetaDescriptionTxt,
                    PageMetaTitleTxt = item.PageMetaTitleTxt,
                    PageTitleTxt = item.PageTitleTxt,
                    AltBannerImageTxt = item.AltBannerImageTxt,
                    PageURLTxt = item.PageURLTxt,
                    DisplayOrderNbr = string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()) ? 0 : item.DisplayOrderNbr.Value,
                    DisplayOrderNbrSelect = GetDisplayOrderSubMenu(item.DisplayOrderNbr.ToString(), SubMenuTypeID, ParentId),
                    StatusNumSelect = GetStatus(item.StatusInd.ToString(), SubMenuTypeID)
                });
            }
            return list;
        }

        //Display Order 
        public SelectList GetDisplayOrder(string value, int contentTypeID)
        {
            List<MenuDisplayOrder> status = new List<MenuDisplayOrder>();
            var varCount = _context.Contents.Where(x => x.DisplayOrderNbr != 0 && x.ContentTypeID == contentTypeID && x.ParentID == null && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
            for (var i = 1; i <= varCount; i++)
            {
                status.Add(new MenuDisplayOrder { DisplayID = i, DisplayOrderNbr = i.ToString() });
            }
            SelectList objinfo = new SelectList(status, "DisplayID", "DisplayOrderNbr", value);
            return objinfo;
        }

        public SelectList GetStatus(string value, int contentTypeID)
        {
            Dictionary<string, string> StatusTypes = new Dictionary<string, string>();
            StatusTypes.Add("Active", "1");
            StatusTypes.Add("InActive", "0");

            List<SelectListItem> items = new List<SelectListItem>();
            var selectedStatus = _context.Contents.Where(x => x.ContentTypeID == contentTypeID).Select(x => x.StatusInd).FirstOrDefault();
            bool IsSelected = false;
            foreach (KeyValuePair<string, string> stat in StatusTypes)
            {
                if (stat.Value == selectedStatus.ToString()) { IsSelected = true; }
                items.Add(new SelectListItem { Text = stat.Key, Value = stat.Value, Selected = IsSelected });
            }
            SelectList objinfo = new SelectList(items, "Value", "Text", value);
            return objinfo;
        }

        //Display Order Sub Menu
        public SelectList GetDisplayOrderSubMenu(string value, int SubMenuTypeID, int parentID)
        {
            List<MenuDisplayOrder> status = new List<MenuDisplayOrder>();
            var varCount = _context.Contents.Where(x => x.DisplayOrderNbr != 0 && x.ContentTypeID == SubMenuTypeID && x.ParentID == parentID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
            for (var i = 1; i <= varCount; i++)
            {
                status.Add(new MenuDisplayOrder { DisplayID = i, DisplayOrderNbr = i.ToString() });
            }
            SelectList objinfo = new SelectList(status, "DisplayID", "DisplayOrderNbr", value);
            return objinfo;
        }

        /// <summary>
        /// Change display order 
        /// </summary>
        /// <param name="sourceorder"></param>
        /// <param name="targetorder"></param>
        /// <param name="ContentID"></param>
        /// <returns></returns>
        public bool ChangeImageDisplayOrder(long sourceorder, long targetorder, long ContentID, int ContentTypeID)
        {
            try
            {
                if (sourceorder < targetorder)
                {
                    var MenuList = _context.Contents.Where(x => x.DisplayOrderNbr >= sourceorder && x.DisplayOrderNbr <= targetorder && x.ContentTypeID == ContentTypeID && x.ParentID == null && (x.IsDeletedInd == false || x.IsDeletedInd == null)).ToList();
                    foreach (var item in MenuList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr -= 1;
                    }
                }
                else
                {
                    var MenuList = _context.Contents.Where(x => x.DisplayOrderNbr >= targetorder && x.DisplayOrderNbr <= sourceorder && x.ContentTypeID == ContentTypeID && x.ParentID == null && (x.IsDeletedInd == false || x.IsDeletedInd == null)).ToList();
                    foreach (var item in MenuList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr += 1;
                    }
                }
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Change display order of Sub Menu
        /// </summary>
        /// <param name="sourceorder"></param>
        /// <param name="targetorder"></param>
        /// <param name="ContentID"></param>
        /// <param name="ParentID"></param>
        /// <returns></returns>
        public bool ChangeImageDisplayOrderSubMenu(long sourceorder, long targetorder, long ContentID, int SubMenuTypeID, int ParentID)
        {
            try
            {
                if (sourceorder < targetorder)
                {
                    var MenuList = _context.Contents.Where(x => x.DisplayOrderNbr >= sourceorder && x.DisplayOrderNbr <= targetorder && x.ContentTypeID == SubMenuTypeID && x.ParentID == ParentID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).ToList();
                    foreach (var item in MenuList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr -= 1;
                    }
                }
                else
                {
                    var MenuList = _context.Contents.Where(x => x.DisplayOrderNbr >= targetorder && x.DisplayOrderNbr <= sourceorder && x.ContentTypeID == SubMenuTypeID && x.ParentID == ParentID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).ToList();
                    foreach (var item in MenuList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr += 1;
                    }
                }
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Change Display order of all.
        /// </summary>
        /// <param name="sourceorder"></param>
        /// <param name="ContentID"></param>
        public bool ChangeDeletedDisplayOrder(long sourceorder, long ContentID, int ContentTypeID)
        {
            try
            {
                var _objMenu = _context.Contents.Where(x => x.ContentID == ContentID).FirstOrDefault();
                var _obj_objMenuList = _context.Contents.Where(x => x.DisplayOrderNbr > sourceorder && x.ContentTypeID == ContentTypeID && x.ParentID == null && (x.IsDeletedInd == false || x.IsDeletedInd == null)).ToList();
                foreach (var item in _obj_objMenuList)
                {
                    item.DisplayOrderNbr -= 1;
                }
                _objMenu.DisplayOrderNbr = null;
                _objMenu.IsDeletedInd = true;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Change Display order of all Sub Menu.
        /// </summary>
        /// <param name="sourceorder"></param>
        /// <param name="ContentID"></param>
        /// <param name="ParentID"></param>
        public bool ChangeDeletedDisplayOrderSubMenu(long sourceorder, long ContentID, long SubMenuTypeID, int ParentID)
        {
            try
            {
                var _objMenu = _context.Contents.Where(x => x.ContentID == ContentID).FirstOrDefault();
                var _obj_objMenuList = _context.Contents.Where(x => x.DisplayOrderNbr > sourceorder && x.ContentTypeID == SubMenuTypeID && x.ParentID == ParentID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).ToList();
                foreach (var item in _obj_objMenuList)
                {
                    item.DisplayOrderNbr -= 1;
                }
                _objMenu.DisplayOrderNbr = null;
                _objMenu.IsDeletedInd = true;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
