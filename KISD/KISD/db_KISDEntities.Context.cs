﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class db_KISDEntities : DbContext
    {
        public db_KISDEntities()
            : base("name=db_KISDEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Announcement> Announcements { get; set; }
        public virtual DbSet<BoardOfMember> BoardOfMembers { get; set; }
        public virtual DbSet<Content> Contents { get; set; }
        public virtual DbSet<ContentType> ContentTypes { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<DepartmentStaff> DepartmentStaffs { get; set; }
        public virtual DbSet<Email> Emails { get; set; }
        public virtual DbSet<EmailType> EmailTypes { get; set; }
        public virtual DbSet<ExceptionOpportunity> ExceptionOpportunities { get; set; }
        public virtual DbSet<FormEmail> FormEmails { get; set; }
        public virtual DbSet<GalleryListing> GalleryListings { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<ImageType> ImageTypes { get; set; }
        public virtual DbSet<ListingParameter> ListingParameters { get; set; }
        public virtual DbSet<NewsEvent> NewsEvents { get; set; }
        public virtual DbSet<RightSection> RightSections { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<School> Schools { get; set; }
        public virtual DbSet<SocialSharingSetting> SocialSharingSettings { get; set; }
        public virtual DbSet<Staff> Staffs { get; set; }
        public virtual DbSet<SystemAccessLog> SystemAccessLogs { get; set; }
        public virtual DbSet<SystemChangeLog> SystemChangeLogs { get; set; }
        public virtual DbSet<SystemChangeLogDetail> SystemChangeLogDetails { get; set; }
        public virtual DbSet<TypeMaster> TypeMasters { get; set; }
        public virtual DbSet<UserDepartment> UserDepartments { get; set; }
        public virtual DbSet<UserPermission> UserPermissions { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<User> Users { get; set; }
    
        public virtual ObjectResult<pr_search_all_paged_Result> pr_search_all_paged(string search)
        {
            var searchParameter = search != null ?
                new ObjectParameter("search", search) :
                new ObjectParameter("search", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<pr_search_all_paged_Result>("pr_search_all_paged", searchParameter);
        }
    }
}