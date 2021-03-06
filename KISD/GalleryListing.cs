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
    
    public partial class GalleryListing
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GalleryListing()
        {
            this.GalleryListing1 = new HashSet<GalleryListing>();
        }
    
        public long ListingID { get; set; }
        public string TitleTxt { get; set; }
        public string FileURLTxt { get; set; }
        public string EmbededURLTxt { get; set; }
        public string AltImageTxt { get; set; }
        public string URLTxt { get; set; }
        public Nullable<long> DisplayOrderNbr { get; set; }
        public Nullable<System.DateTime> DisplayStartDate { get; set; }
        public Nullable<System.DateTime> DisplayEndDate { get; set; }
        public Nullable<int> UploadTypeNbr { get; set; }
        public string DescriptionTxt { get; set; }
        public string AuthorTxt { get; set; }
        public Nullable<bool> ShowOnHomeInd { get; set; }
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
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GalleryListing> GalleryListing1 { get; set; }
        public virtual GalleryListing GalleryListing2 { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
    }
}
