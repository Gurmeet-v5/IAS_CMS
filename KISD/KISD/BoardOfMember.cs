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
    
    public partial class BoardOfMember
    {
        public long BoardMemberID { get; set; }
        public string ImageURLTxt { get; set; }
        public string NameTxt { get; set; }
        public string TitleTxt { get; set; }
        public string URLTxt { get; set; }
        public string ContactInfoTxt { get; set; }
        public string TermTxt { get; set; }
        public string RightSectionTitleTxt { get; set; }
        public string RightSectionAbstractTxt { get; set; }
        public string DescriptionTxt { get; set; }
        public Nullable<long> DisplayOrderNbr { get; set; }
        public string PageMetaTitleTxt { get; set; }
        public string PageMetaDescriptionTxt { get; set; }
        public Nullable<bool> StatusInd { get; set; }
        public Nullable<System.DateTime> BOMCreateDate { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }
        public Nullable<long> BannerImageID { get; set; }
        public string AltBannerImageTxt { get; set; }
        public string BannerImageAbstractTxt { get; set; }
    
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
    }
}
