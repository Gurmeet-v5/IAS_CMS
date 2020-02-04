using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public class GalleryListingModel
    {
        public long ListingID { get; set; }
        public string TitleTxt { get; set; }
        public string FileURLTxt { get; set; }
        public string EmbededURLTxt { get; set; }
        public string AltImageTxt { get; set; }
        public string URLTxt { get; set; }
        public Nullable<long> DisplayOrderNbr { get; set; }
        public SelectList DisplayOrderNbrSelect { get; set; }//Display Order
        public Nullable<System.DateTime> DisplayStartDate { get; set; }
        public Nullable<System.DateTime> DisplayEndDate { get; set; }
        public Nullable<int> UploadTypeNbr { get; set; }
        public string DescriptionTxt { get; set; }
        public string AuthorTxt { get; set; }
        public Nullable<bool> ShowOnHomeInd { get; set; }
        public bool SelectedStatus { get; set; }
        public string PageMetaTitleTxt { get; set; }
        public string PageMetaDescriptionTxt { get; set; }
        public Nullable<bool> StatusInd { get; set; }
        public Nullable<System.DateTime> ListingCreateDate { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }
        public Nullable<long> TypeMasterID { get; set; }
        public Nullable<long> ParentID { get; set; }
        public Nullable<long> BannerImageID { get; set; }
        public string AltBannerImageTxt { get; set; }
        public string BannerImageAbstractTxt { get; set; }
        public IPagedList<GalleryListing> CommonPagedList { get; set; }
        public bool IsPagingVisible { get; set; }
    }
    public class GalleryListingService
    {
        private db_KISDEntities _context;

        /// <summary>
        /// Gallery Listing Service
        /// </summary>
        public GalleryListingService()
        {
            _context = new db_KISDEntities();
        }

        ///// <summary>
        ///// IQueryable<ImageModel> Method for Getting the Gallery Listing Model by passing the TypeMasterID
        ///// And Query the From database by calling the GetGalleryListing() Method.
        ///// </summary>
        ///// <param name="TypeMasterID">TypeMasterID</param>
        ///// <returns>GalleryListingModel</returns>
        //public IQueryable<GalleryListingModel> GetGalleryListingView(long? TypeMasterID)
        //{
        //    var query = from x in GetGalleryListing(TypeMasterID)
        //                select new GalleryListingModel
        //                {
        //                    ListingID = x.ListingID,
        //                    TitleTxt = x.TitleTxt,
        //                    FileURLTxt = x.FileURLTxt,
        //                    EmbededURLTxt = x.EmbededURLTxt,
        //                    AltImageTxt = x.AltImageTxt,
        //                    URLTxt = x.URLTxt,
        //                    DisplayOrderNbr = x.DisplayOrderNbr,
        //                    DisplayStartDate = x.DisplayStartDate,
        //                    DisplayEndDate = x.DisplayEndDate,
        //                    UploadTypeNbr = x.UploadTypeNbr,
        //                    DescriptionTxt = x.DescriptionTxt,
        //                    AuthorTxt = x.AuthorTxt,
        //                    ShowOnHomeInd = x.ShowOnHomeInd,
        //                    PageMetaTitleTxt = x.PageMetaTitleTxt,
        //                    PageMetaDescriptionTxt = x.PageMetaDescriptionTxt,
        //                    StatusInd = x.StatusInd,
        //                    ListingCreateDate = x.ListingCreateDate,
        //                    CreateDate = x.CreateDate,
        //                    CreateByID = x.CreateByID,
        //                    LastModifyDate = x.LastModifyDate,
        //                    LastModifyByID = x.LastModifyByID,
        //                    IsDeletedInd = x.IsDeletedInd,
        //                    TypeMasterID = x.TypeMasterID,
        //                    ParentID = x.ParentID,
        //                    DisplayOrderNbrSelect = GetDisplayOrder((x.DisplayOrderNbr != null ? x.DisplayOrderNbr.Value.ToString() : "0"), x.TypeMasterID.Value)

        //                };
        //    return query.AsQueryable();
        //}

        /// <summary>
        /// this will return list of all Videos in Video model
        /// </summary>
        /// <returns></returns>
        public List<GalleryListingModel> GetGalleryListingView(long TypeMasterID)
        {
            var list = new List<GalleryListingModel>();
            foreach (var x in GetGalleryListing(TypeMasterID))
            {
                list.Add(new GalleryListingModel
                {
                    ListingID = x.ListingID,
                    TitleTxt = x.TitleTxt,
                    FileURLTxt = x.FileURLTxt,
                    EmbededURLTxt = x.EmbededURLTxt,
                    AltImageTxt = x.AltImageTxt,
                    URLTxt = x.URLTxt,
                    DisplayOrderNbr = x.DisplayOrderNbr,
                    DisplayStartDate = x.DisplayStartDate,
                    DisplayEndDate = x.DisplayEndDate,
                    UploadTypeNbr = x.UploadTypeNbr,
                    DescriptionTxt = x.DescriptionTxt,
                    AuthorTxt = x.AuthorTxt,
                    ShowOnHomeInd = x.ShowOnHomeInd,
                    PageMetaTitleTxt = x.PageMetaTitleTxt,
                    PageMetaDescriptionTxt = x.PageMetaDescriptionTxt,
                    StatusInd = x.StatusInd,
                    ListingCreateDate = x.ListingCreateDate,
                    CreateDate = x.CreateDate,
                    CreateByID = x.CreateByID,
                    LastModifyDate = x.LastModifyDate,
                    LastModifyByID = x.LastModifyByID,
                    IsDeletedInd = x.IsDeletedInd,
                    TypeMasterID = x.TypeMasterID,
                    ParentID = x.ParentID,
                    DisplayOrderNbrSelect = GetDisplayOrder((x.DisplayOrderNbr != null ? x.DisplayOrderNbr.Value.ToString() : "0"), x.TypeMasterID.Value),
                    BannerImageID = x.BannerImageID,
                    AltBannerImageTxt = x.AltBannerImageTxt,
                    BannerImageAbstractTxt = x.BannerImageAbstractTxt
                });
            }
            return list;
        }

        /// <summary>
        /// this will return list of all Videos in Video model
        /// </summary>
        /// <returns></returns>
        public List<GalleryListingModel> GetGalleryListingView(long TypeMasterID, long ParentID)
        {
            var list = new List<GalleryListingModel>();
            foreach (var x in GetGalleryListing(TypeMasterID, ParentID))
            {
                list.Add(new GalleryListingModel
                {
                    ListingID = x.ListingID,
                    TitleTxt = x.TitleTxt,
                    FileURLTxt = x.FileURLTxt,
                    EmbededURLTxt = x.EmbededURLTxt,
                    AltImageTxt = x.AltImageTxt,
                    URLTxt = x.URLTxt,
                    DisplayOrderNbr = x.DisplayOrderNbr,
                    DisplayStartDate = x.DisplayStartDate,
                    DisplayEndDate = x.DisplayEndDate,
                    UploadTypeNbr = x.UploadTypeNbr,
                    DescriptionTxt = x.DescriptionTxt,
                    AuthorTxt = x.AuthorTxt,
                    ShowOnHomeInd = x.ShowOnHomeInd,
                    PageMetaTitleTxt = x.PageMetaTitleTxt,
                    PageMetaDescriptionTxt = x.PageMetaDescriptionTxt,
                    StatusInd = x.StatusInd,
                    ListingCreateDate = x.ListingCreateDate,
                    CreateDate = x.CreateDate,
                    CreateByID = x.CreateByID,
                    LastModifyDate = x.LastModifyDate,
                    LastModifyByID = x.LastModifyByID,
                    IsDeletedInd = x.IsDeletedInd,
                    TypeMasterID = x.TypeMasterID,
                    ParentID = x.ParentID,
                    DisplayOrderNbrSelect = GetDisplayOrder((x.DisplayOrderNbr != null ? x.DisplayOrderNbr.Value.ToString() : "0"), x.TypeMasterID.Value),
                    BannerImageID = x.BannerImageID,
                    AltBannerImageTxt = x.AltBannerImageTxt,
                    BannerImageAbstractTxt = x.BannerImageAbstractTxt

                });
            }
            return list;
        }

        /// <summary>
        /// IQueryable<ParrieHaynesRanch.Image> get the Gallery Listing Data by passing the TypeMasterID.
        /// </summary>
        /// <param name="ImageTypeID">TypeMasterID</param>
        /// <returns>GalleryListingObject</returns>
        public IQueryable<GalleryListing> GetGalleryListing(long? TypeMasterID)
        {
            return _context.GalleryListings.Where(x => (x.TypeMasterID == TypeMasterID && x.IsDeletedInd == false));
        }

        /// <summary>
        /// IQueryable<ParrieHaynesRanch.Image> get the Gallery Listing Data by passing the TypeMasterID.
        /// </summary>
        /// <param name="TypeMasterID">TypeMasterID</param>
        /// <returns>GalleryListingObject</returns>
        public IQueryable<GalleryListing> GetGalleryListing(long? TypeMasterID, long ParentID)
        {
            return _context.GalleryListings.Where(x => (x.TypeMasterID == TypeMasterID && x.ParentID == ParentID && x.IsDeletedInd == false));
        }


        /// <summary>
        /// Get all GalleryListing of defined type
        /// </summary>
        /// <param name="ImageType"></param>
        /// <returns></returns>
        public string GetGalleryListingType(long TypeMasterID)
        {
            return _context.TypeMasters.Where(x => x.TypeMasterID == TypeMasterID).Select(x => x.TypeMasterName).FirstOrDefault();
        }

        /// <summary>
        ///  All TypeMaster with static defined types.        
        /// </summary>        
        /// <returns></returns>
        public enum TypeMaster : long
        {
            ImageListing = 1,
            DocumentViewer = 2,
            Podcast = 3,
            Video = 4,
            PhotoGallery = 5,
            FAQCategory = 6,
            OnscreenAlert = 7,
            DailyNews = 8,
            Events = 9,
            FAQ = 10,
            SchoolCategory = 11,
            School = 12,
            PhotoGalleryImages = 13,
            Announcement = 14,
            BoardSchedule = 15,
            ExceptionOpportunity = 16,
            Department = 17,
            ManageEvents = 18,
            Content = 19,
            BoardOfMembers = 20,
            HomeMenu = 21,
            HomeSubMenu = 22,
            AboutUsMenu = 23,
            AboutUsSubMenu = 24,
            SchoolMenu = 25,
            SchoolSubMenu = 26,
            NewToKISDMenu = 27,
            NewToKISDSubMenu = 28,
            ParentStudentsMenu = 29,
            ParentStudentsSubMenu = 30,
            DepartmentsMenu = 31,
            DepartmentsSubMenu = 32,
            SchoolBoardMenu = 33,
            SchoolBoardSubMenu = 34,
            EmploymentMenu = 35,
            EmploymentSubMenu = 36,
            AboutKISD = 37,
            NewToKISD = 38,
            ParentStudents = 39,
            Staff = 40,
            SchoolBoard = 41,
            BoardMembersListing = 42,
            Employment = 43,
            NewsListing = 44,
            ExceptionalOpportunityListing = 45,
            DepartmentListing = 46,
            Fly = 47,
            Syllabus = 48,
            Downloads = 49,
            ContactUs = 50,
            ContactUsMenu = 51,
            ContactUsSubMenu = 52,
            SyllabusMenu = 53,
            SyllabusSubMenu = 54,
            DownloadsMenu = 55,
            DownloadsSubMenu = 56,
            DailyNewsMenu = 57,
            DailyNewsSubMenu = 58,
        }

        public SelectList GetDisplayOrder(string value, long TypeMasterID)
        {
            List<DisplayOrder> status = new List<DisplayOrder>();
            var varCount = _context.GalleryListings.Where(x => x.DisplayOrderNbr != 0 && x.TypeMasterID == TypeMasterID && x.IsDeletedInd == false).Count();
            for (var i = 1; i <= varCount; i++)
            {
                status.Add(new DisplayOrder { DisplayID = i, DisplayOrderNbr = i.ToString() });
            }
            SelectList objinfo = new SelectList(status, "DisplayID", "DisplayOrderNbr", value);
            return objinfo;
        }

        /// <summary>
        /// Change display order 
        /// </summary>
        /// <param name="sourceorder"></param>
        /// <param name="targetorder"></param>
        /// <param name="ListingID"></param>
        /// <returns></returns>
        public static bool ChangeDisplayOrder(long sourceorder, long targetorder, long ListingID, int TypeMasterID)
        {
            var _context = new db_KISDEntities();
            try
            {
                if (sourceorder < targetorder)
                {
                    var MyList = _context.GalleryListings.Where(x => x.DisplayOrderNbr >= sourceorder && x.DisplayOrderNbr <= targetorder && x.TypeMasterID == TypeMasterID && x.IsDeletedInd == false).ToList();
                    foreach (var item in MyList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr -= 1;
                    }
                }
                else
                {
                    var MyList = _context.GalleryListings.Where(x => x.DisplayOrderNbr >= targetorder && x.DisplayOrderNbr <= sourceorder && x.TypeMasterID == TypeMasterID && x.IsDeletedInd == false).ToList();
                    foreach (var item in MyList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr += 1;
                    }
                }
                _context.SaveChanges();

                #region System Change Log
                //SystemChangeLog objSCL = new SystemChangeLog();
                //long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                //User objuser = _context.Users.Where(x => x.UserID == userid).FirstOrDefault();
                //objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                //objSCL.UsernameTxt = objuser.UserNameTxt;
                //objSCL.UserRoleID = (short)_context.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                //objSCL.ModuleTxt = "Image";
                //objSCL.LogTypeTxt = images.ImageID > 0 ? "Update" : "Add";
                //objSCL.NotesTxt = "Right Section Details" + (images.ImageID > 0 ? " updated for " : "  added for ") + images.TitleTxt;
                //objSCL.LogDateTime = DateTime.Now;
                //objContext.SystemChangeLogs.Add(objSCL);
                //objContext.SaveChanges();

                //objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                //var newResult = (from x in objContext.Images
                //                 where x.ImageID == images.ImageID
                //                 select x);
                //DataTable dtNew = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(newResult);
                //foreach (DataColumn col in dtNew.Columns)
                //{
                //    // if(objSCL)
                //    if (dtOld.Rows[0][col.ColumnName].ToString() != dtNew.Rows[0][col.ColumnName].ToString())
                //    {
                //        SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                //        objSCLD.ChangeLogID = objSCL.ChangeLogID;
                //        objSCLD.FieldNameTxt = col.ColumnName.ToString();
                //        objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                //        objSCLD.NewValueTxt = dtNew.Rows[0][col.ColumnName].ToString();
                //        objContext.SystemChangeLogDetails.Add(objSCLD);
                //        objContext.SaveChanges();
                //    }
                //}
                #endregion

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
        /// <param name="ListingID"></param>
        public bool ChangeDeletedDisplayOrder(long sourceorder, long ListingID, long TypeMasterID)
        {
            try
            {
                var _objGalleryListing = _context.GalleryListings.Where(x => x.ListingID == ListingID).FirstOrDefault();
                var _objGalleryList = _context.GalleryListings.Where(x => x.DisplayOrderNbr > sourceorder && x.TypeMasterID == TypeMasterID && x.IsDeletedInd == false).ToList();
                foreach (var item in _objGalleryList)
                {
                    item.DisplayOrderNbr -= 1;
                }
                _objGalleryListing.DisplayOrderNbr = null;
                _objGalleryListing.IsDeletedInd = true;
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
        /// <param name="ListingID"></param>
        public bool ChangeDeletedDisplayOrder(long sourceorder, long ListingID, long TypeMasterID, long ParentID)
        {
            try
            {
                var _objGalleryListing = _context.GalleryListings.Where(x => x.ListingID == ListingID).FirstOrDefault();
                var _objGalleryList = _context.GalleryListings.Where(x => x.DisplayOrderNbr > sourceorder && x.TypeMasterID == TypeMasterID && x.ParentID == ParentID && x.IsDeletedInd == false).ToList();
                foreach (var item in _objGalleryList)
                {
                    item.DisplayOrderNbr -= 1;
                }
                _objGalleryListing.DisplayOrderNbr = null;
                _objGalleryListing.IsDeletedInd = true;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    //Display Order 

}