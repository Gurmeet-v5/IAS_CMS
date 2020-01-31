using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public class NewsEventModel
    {
        public long NewsEventID { get; set; }
        public string TitleTxt { get; set; }
        public string ImageURLTxt { get; set; }
        public Nullable<bool> IsExternalLinkInd { get; set; }
        public string PageURLTxt { get; set; }
        public string URLTxt { get; set; }
        public Nullable<bool> ExternalLinkTargetInd { get; set; }
        public string AuthorNameTxt { get; set; }
        public string AbstractTxt { get; set; }
        public string DescriptionTxt { get; set; }
        public string RightSectionTitleTxt { get; set; }
        public string RightSectionAbstractTxt { get; set; }
        public Nullable<long> BannerImageID { get; set; }
        public string[] SelectedRightSections { get; set; }
        public SelectList RightSections { get; set; }
        public string AltBannerImageTxt { get; set; }
        public string BannerImageAbstractTxt { get; set; }
        public Nullable<System.DateTime> EventDate { get; set; }
        public Nullable<System.DateTime> DisplayStartDate { get; set; }
        public Nullable<System.DateTime> DisplayEndDate { get; set; }
        public Nullable<bool> ShowOnHomeInd { get; set; }
        public string PageMetaTitleTxt { get; set; }
        public string PageMetaDescriptionTxt { get; set; }
        public Nullable<bool> StatusInd { get; set; }
        public Nullable<long> DepartmentID { get; set; }
        public Nullable<System.DateTime> EventCreateDate { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }
        public Nullable<long> TypeMasterID { get; set; }
        public Nullable<bool> IsFacebookSharingInd { get; set; }
        public Nullable<bool> IsTwitterSharingInd { get; set; }
        public Nullable<bool> IsGooglePlusSharingInd { get; set; }
        public Nullable<bool> IsRecurringInd { get; set; }
        public Nullable<int> RecurrenceTypeNbr { get; set; }
        public Nullable<bool> IsSundayInd { get; set; } = false;
        public Nullable<bool> IsMondayInd { get; set; } = false;
        public Nullable<bool> IsTuesdayInd { get; set; } = false;
        public Nullable<bool> IsWednesdayInd { get; set; } = false;
        public Nullable<bool> IsThursdayInd { get; set; } = false;
        public Nullable<bool> IsFridayInd { get; set; } = false;
        public Nullable<bool> IsSaturdayInd { get; set; } = false;
        public Nullable<int> MonthModeNbr { get; set; }
        [Range(1, 31,ErrorMessage = "This field must be between 1 and 31.")]
        public Nullable<int> MonthdayNbr { get; set; }
        public Nullable<int> MonthonNbr { get; set; }
        public Nullable<int> MonthondayNbr { get; set; }
        public Nullable<System.DateTime> EventsEndOnDate { get; set; }
        public DataSet dsError { get; set; }
        public virtual Department Department { get; set; }
        public virtual TypeMaster TypeMaster { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
        public string ContentUrl { get; set; }
        public bool NoIsSunday
        {
            get { return IsSundayInd ?? false; }
            set { IsSundayInd = value; }
        }
        public bool NoIsMonday
        {
            get { return IsMondayInd ?? false; }
            set { IsMondayInd = value; }
        }
        public bool NoIsTuesday
        {
            get { return IsTuesdayInd ?? false; }
            set { IsTuesdayInd = value; }
        }
        public bool NoIsWednesday
        {
            get { return IsWednesdayInd ?? false; }
            set { IsWednesdayInd = value; }
        }
        public bool NoIsThursday
        {
            get { return IsThursdayInd ?? false; }
            set { IsThursdayInd = value; }
        }
        public bool NoIsFriday
        {
            get { return IsFridayInd ?? false; }
            set { IsFridayInd = value; }
        }
        public bool NoIsSaturday
        {
            get { return IsSaturdayInd ?? false; }
            set { IsSaturdayInd = value; }
        }
        public List<RightSection> ParentRightSections { get; set; }
    }

    public class NewsEventServices
    {
        private db_KISDEntities _context;
        public NewsEventServices()
        {
            _context = new db_KISDEntities();
        }

        /// <summary>
        /// IQueryable<ImageModel> Method for Getting the News Event Model.
        /// And Query the From database by calling the GetNewsEvent() Method.
        /// </summary>
        /// <returns>NewsEventModel</returns>
        public IQueryable<NewsEventModel> GetNewsEventView(long TypeMasterID, long DepartmentId)
        {
            var query = from x in GetAllNewsEvent(TypeMasterID)
                        select new NewsEventModel
                        {
                            NewsEventID = x.NewsEventID,
                            TitleTxt = x.TitleTxt,
                            ImageURLTxt = x.ImageURLTxt,
                            IsExternalLinkInd = x.IsExternalLinkInd,
                            PageURLTxt = x.PageURLTxt,
                            ExternalLinkTargetInd = x.ExternalLinkTargetInd,
                            AuthorNameTxt = x.AuthorNameTxt,
                            AbstractTxt = x.AbstractTxt,
                            DescriptionTxt = x.DescriptionTxt,
                            RightSectionTitleTxt = x.RightSectionTitleTxt,
                            RightSectionAbstractTxt = x.RightSectionAbstractTxt,
                            BannerImageID = x.BannerImageID,
                            AltBannerImageTxt = x.AltBannerImageTxt,
                            BannerImageAbstractTxt = x.BannerImageAbstractTxt,
                            EventDate = x.EventDate,
                            DisplayStartDate = x.DisplayStartDate,
                            DisplayEndDate = x.DisplayEndDate,
                            ShowOnHomeInd = x.ShowOnHomeInd,
                            PageMetaTitleTxt = x.PageMetaTitleTxt,
                            PageMetaDescriptionTxt = x.PageMetaDescriptionTxt,
                            StatusInd = x.StatusInd,
                            DepartmentID = x.DepartmentID,
                            EventCreateDate = x.EventCreateDate,
                            CreateDate = x.CreateDate,
                            CreateByID = x.CreateByID,
                            LastModifyDate = x.LastModifyDate,
                            LastModifyByID = x.LastModifyByID,
                            IsDeletedInd = x.IsDeletedInd,
                            TypeMasterID = x.TypeMasterID,
                            IsFacebookSharingInd = x.IsFacebookSharingInd,
                            IsTwitterSharingInd = x.IsTwitterSharingInd,
                            IsGooglePlusSharingInd = x.IsGooglePlusSharingInd,
                            IsRecurringInd = x.IsRecurringInd,
                            RecurrenceTypeNbr = x.RecurrenceTypeNbr,
                            IsSundayInd = x.IsSundayInd,
                            IsMondayInd = x.IsMondayInd,
                            IsTuesdayInd = x.IsTuesdayInd,
                            IsWednesdayInd = x.IsWednesdayInd,
                            IsThursdayInd = x.IsThursdayInd,
                            IsFridayInd = x.IsFridayInd,
                            IsSaturdayInd = x.IsSaturdayInd,
                            MonthModeNbr = x.MonthModeNbr,
                            MonthdayNbr = x.MonthdayNbr,
                            MonthondayNbr = x.MonthondayNbr,
                            MonthonNbr = x.MonthonNbr,
                            EventsEndOnDate = x.EventsEndOnDate
                        };
            if (DepartmentId != 0)
            {
                query = query.Where(x => x.DepartmentID == DepartmentId);
            }
            return query.AsQueryable();
        }

        /// <summary>
        /// Get all News Event of defined type
        /// </summary>

        /// <returns></returns>
        public IQueryable<NewsEvent> GetAllNewsEvent(long TypeMasterID)
        {
            return _context.NewsEvents.Where(x => x.TypeMasterID == TypeMasterID && (x.IsDeletedInd == null || x.IsDeletedInd == false));
        }

        /// <summary>
        /// Get all GalleryListing of defined type
        /// </summary>
        /// <param name="ImageType"></param>
        /// <returns></returns>
        public string GetNewsEventType(long TypeMasterID)
        {
            return _context.TypeMasters.Where(x => x.TypeMasterID == TypeMasterID).Select(x => x.TypeMasterName).FirstOrDefault();
        }

        public SelectList GetStatus(string value, long NewsEventID)
        {
            Dictionary<string, string> StatusTypes = new Dictionary<string, string>();
            StatusTypes.Add("Active", "1");
            StatusTypes.Add("InActive", "0");
            List<SelectListItem> items = new List<SelectListItem>();
            var selectedStatus = _context.NewsEvents.Where(x => x.NewsEventID == NewsEventID && x.IsDeletedInd == false).Select(x => x.StatusInd).FirstOrDefault();
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

    public class EventCalendar
    {
        public int Sr { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; }
        public string Start_Date { get; set; }
        public string End_Date { get; set; }
    }
}