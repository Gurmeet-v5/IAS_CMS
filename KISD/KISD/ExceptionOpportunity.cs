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
    
    public partial class ExceptionOpportunity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ExceptionOpportunity()
        {
            this.ExceptionOpportunity1 = new HashSet<ExceptionOpportunity>();
        }
    
        public long ExOpportunityID { get; set; }
        public string TitleTxt { get; set; }
        public Nullable<bool> ExternalLinkInd { get; set; }
        public string URLTxt { get; set; }
        public Nullable<bool> ExternalLinkTargetInd { get; set; }
        public Nullable<long> BannerImageID { get; set; }
        public string AltBannerImageTxt { get; set; }
        public string BannerImageAbstractTxt { get; set; }
        public string RightSectionTitleTxt { get; set; }
        public string RightSectionAbstractTxt { get; set; }
        public string DescriptionTxt { get; set; }
        public Nullable<bool> ShowOnHomeInd { get; set; }
        public Nullable<long> DisplayOrderNbr { get; set; }
        public string PageMetaTitleTxt { get; set; }
        public string PageMetaDescriptionTxt { get; set; }
        public Nullable<bool> StatusInd { get; set; }
        public Nullable<long> ParentID { get; set; }
        public Nullable<long> SchoolCategoryID { get; set; }
        public Nullable<System.DateTime> ExOpportunityCreateDate { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExceptionOpportunity> ExceptionOpportunity1 { get; set; }
        public virtual ExceptionOpportunity ExceptionOpportunity2 { get; set; }
        public virtual Image Image { get; set; }
        public virtual School School { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
    }
}
