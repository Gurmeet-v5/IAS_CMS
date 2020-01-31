//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KISD
{
    using System;
    using System.Collections.Generic;
    
    public partial class NewsEvent
    {
        public long NewsEventID { get; set; }
        public string TitleTxt { get; set; }
        public string ImageURLTxt { get; set; }
        public Nullable<bool> IsExternalLinkInd { get; set; }
        public string PageURLTxt { get; set; }
        public Nullable<bool> ExternalLinkTargetInd { get; set; }
        public string AuthorNameTxt { get; set; }
        public string AbstractTxt { get; set; }
        public string DescriptionTxt { get; set; }
        public string RightSectionTitleTxt { get; set; }
        public string RightSectionAbstractTxt { get; set; }
        public Nullable<long> BannerImageID { get; set; }
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
        public Nullable<bool> IsSundayInd { get; set; }
        public Nullable<bool> IsMondayInd { get; set; }
        public Nullable<bool> IsTuesdayInd { get; set; }
        public Nullable<bool> IsWednesdayInd { get; set; }
        public Nullable<bool> IsThursdayInd { get; set; }
        public Nullable<bool> IsFridayInd { get; set; }
        public Nullable<bool> IsSaturdayInd { get; set; }
        public Nullable<int> MonthModeNbr { get; set; }
        public Nullable<int> MonthdayNbr { get; set; }
        public Nullable<int> MonthonNbr { get; set; }
        public Nullable<int> MonthondayNbr { get; set; }
        public Nullable<System.DateTime> EventsEndOnDate { get; set; }
    }
}
