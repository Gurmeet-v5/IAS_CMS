using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public class RightSectionModel
    {
        public long RightSectionID { get; set; }
        public Nullable<bool> IsParentTitleInd { get; set; }
        public string TitleTxt { get; set; }
        public string PageTitleTxt { get; set; }
        public string RightSectionTitleTxt { get; set; }
        public string RightSectionAbstractTxt { get; set; }
        public string DescriptionTxt { get; set; }
        public Nullable<bool> IsExternalLinkInd { get; set; }
        public string ExternalLinkURLTxt { get; set; }

        public string PageURLTxt { get; set; }
        public Nullable<bool> ExternalLinkTargetInd { get; set; }
        public Nullable<bool> StatusInd { get; set; }
        public Nullable<long> BannerImageID { get; set; }
        public string AltBannerImageTxt { get; set; }
        public string BannerImageAbstractTxt { get; set; }
        public Nullable<long> ParentID { get; set; }
        public Nullable<System.DateTime> RightSectionCreateDate { get; set; }
        public string PageMetaTitleTxt { get; set; }
        public string PageMetaDescriptionTxt { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<long> CreatedByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }
        public string strRightSectionCreateDate { get; set; }
        public int SubRightSectionLevel { get; set; }
        public SelectList StatusNbrSelect { get; set; }//Status
        public string TableNameTxt { get; set; }
        public Nullable<long> ListingID { get; set; }
        public Nullable<long> TypeMasterID { get; set; }
        public bool IsFacebookSharingInd { get; set; }
        public bool IsTwitterSharingInd { get; set; }
        public bool IsGooglePlusSharingInd { get; set; }
        public List<RightSection> ParentRightSections { get; set; }
    }

    public class ListingRightSectionModel
    {
        public string RightSectionTitleTxt { get; set; }
        public string RightSectionAbstractTxt { get; set; }
        public List<RightSection> ParentRightSections { get; set; }
        public List<RightSection> ChildRightSections { get; set; }
        public bool IsFacebookSharingInd { get; set; }
        public bool IsTwitterSharingInd { get; set; }
        public bool IsGooglePlusSharingInd { get; set; }
        public string PageTitleTxt { get; set; }
        public string PageURLTxt { get; set; }
    }

    public class RightSectionService
    {
        private db_KISDEntities _context;
        public RightSectionService()
        {
            _context = new db_KISDEntities();
        }

        /// <summary>
        /// IQueryable<ImageModel> Method for Getting the Right Section Model.
        /// And Query the From database by calling the GetRightSection() Method.
        /// </summary>
        /// <returns>RightSectionModel</returns>
        public IQueryable<RightSectionModel> GetRightSectionView(long ListingID, long TypeMasterID)
        {           
            var query = from x in GetRightSection(ListingID, TypeMasterID)
                        select new RightSectionModel
                        {
                            RightSectionID = x.RightSectionID,
                            IsParentTitleInd = x.IsParentTitleInd,
                            TitleTxt = x.TitleTxt,
                            PageTitleTxt = x.PageTitleTxt,
                            RightSectionTitleTxt = x.RightSectionTitleTxt,
                            RightSectionAbstractTxt = x.RightSectionAbstractTxt,
                            DescriptionTxt = x.DescriptionTxt,
                            IsExternalLinkInd = x.IsExternalLinkInd,
                            ExternalLinkURLTxt = x.ExternalLinkURLTxt,
                            ExternalLinkTargetInd = x.ExternalLinkTargetInd,
                            StatusInd = x.StatusInd,
                            BannerImageID = x.BannerImageID,
                            AltBannerImageTxt = x.AltBannerImageTxt,
                            BannerImageAbstractTxt = x.BannerImageAbstractTxt,
                            ParentID = x.ParentID,
                            RightSectionCreateDate = x.RightSectionCreateDate,
                            PageMetaTitleTxt = x.PageMetaTitleTxt,
                            PageMetaDescriptionTxt = x.PageMetaDescriptionTxt,
                            CreateDate = x.CreateDate,
                            CreatedByID = x.CreatedByID,
                            LastModifyDate = x.LastModifyDate,
                            LastModifyByID = x.LastModifyByID,
                            IsDeletedInd = x.IsDeletedInd
                        };
            return query.AsQueryable();
        }

        /// <summary>
        /// Get all Right Section of defined type
        /// </summary>

        /// <returns></returns>
        public IQueryable<RightSection> GetAllRightSection()
        {
            return _context.RightSections.Where(x => x.ParentID == null && (x.IsDeletedInd==null || x.IsDeletedInd == false));
        }

        /// <summary>
        /// IQueryable<> get the Right Section data.
        /// </summary>
        /// <returns>RightSectionObject</returns>
        public IQueryable<RightSection> GetRightSection(long ListingID, long TypeMasterID)
        {
            return _context.RightSections.Where(x => x.ParentID == null && x.ListingID== ListingID && x.TypeMasterID == TypeMasterID && (x.IsDeletedInd == null || x.IsDeletedInd == false));
        }

        /// <summary>
        /// IQueryable<ParrieHaynesRanch.Image> get the Right Section data by using main category.
        /// </summary>
        /// <returns>RightSectionObject</returns>
        public IQueryable<RightSection> GetRightSectionByParentID(long ParentID, long ListingID, long TypeMasterID)
        {
            return _context.RightSections.Where(x => x.ListingID == ListingID && x.TypeMasterID==TypeMasterID && (x.IsDeletedInd == null || x.IsDeletedInd == false) && x.ParentID == ParentID);
        }

        public IQueryable<RightSection> GetAllSubRightSections(long ParentID, long ListingID, long TypeMasterID)
        {
            return _context.RightSections.Where(x => x.ParentID == ParentID && x.ListingID==ListingID && x.TypeMasterID==TypeMasterID && (x.IsDeletedInd == null || x.IsDeletedInd == false));
        }

        public List<RightSectionModel> GetSubRightSection(long ParentID, long ListingID, long TypeMasterID)
        {
            var list = new List<RightSectionModel>();
            foreach (var item in GetAllSubRightSections(ParentID, ListingID, TypeMasterID))
            {
                list.Add(new RightSectionModel
                {
                    RightSectionID = item.RightSectionID,
                    IsParentTitleInd = item.IsParentTitleInd,
                    TitleTxt = item.TitleTxt,
                    PageTitleTxt = item.PageTitleTxt,
                    RightSectionTitleTxt = item.RightSectionTitleTxt,
                    RightSectionAbstractTxt = item.RightSectionAbstractTxt,
                    DescriptionTxt = item.DescriptionTxt,
                    IsExternalLinkInd = item.IsExternalLinkInd,
                    ExternalLinkURLTxt = item.ExternalLinkURLTxt,
                    ExternalLinkTargetInd = item.ExternalLinkTargetInd,
                    StatusInd = item.StatusInd,
                    StatusNbrSelect = GetStatus(item.StatusInd.ToString(), item.RightSectionID),
                    BannerImageID = item.BannerImageID,
                    AltBannerImageTxt = item.AltBannerImageTxt,
                    BannerImageAbstractTxt = item.BannerImageAbstractTxt,
                    ParentID = item.ParentID,
                    RightSectionCreateDate = item.RightSectionCreateDate,
                    PageMetaTitleTxt = item.PageMetaTitleTxt,
                    PageMetaDescriptionTxt = item.PageMetaDescriptionTxt,
                    CreateDate = item.CreateDate,
                    CreatedByID = item.CreatedByID,
                    LastModifyDate = item.LastModifyDate,
                    LastModifyByID = item.LastModifyByID,
                    IsDeletedInd = item.IsDeletedInd
                });
            }
            return list;
        }

        public SelectList GetStatus(string value, long RightSectionID)
        {
            Dictionary<string, string> StatusTypes = new Dictionary<string, string>();
            StatusTypes.Add("Active", "1");
            StatusTypes.Add("InActive", "0");

            List<SelectListItem> items = new List<SelectListItem>();
            var selectedStatus = _context.RightSections.Where(x => x.RightSectionID == RightSectionID && x.IsDeletedInd==false).Select(x => x.StatusInd).FirstOrDefault();
            bool IsSelected = false;
            foreach (KeyValuePair<string, string> stat in StatusTypes)
            {
                if (stat.Value == selectedStatus.ToString()) { IsSelected = true; }
                items.Add(new SelectListItem { Text = stat.Key, Value = stat.Value, Selected = IsSelected });
            }
            SelectList objinfo = new SelectList(items, "Value", "Text", value);
            return objinfo;
        }

    }
}