using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public partial class UsersModel
    {
        public UsersModel()
        {
            this.UserRoles = new HashSet<UserRoleModel>();
        }
        public long UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string UserNameTxt { get; set; }
        [RegularExpression(@"^.*(?=.{6,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$", ErrorMessage = "Password must be 6 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.")]
        public string Password { get; set; }
        [RegularExpression(@"^.*(?=.{6,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$", ErrorMessage = "Password must be 6 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.")]
        public string ChangedPassword { get; set; }
        public string Email { get; set; }
        public bool Status { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTime> UserCreateDate { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<long> CreatedByUserID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByUserID { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public Int32 UserRoleID { get; set; }

        public SelectList RolesList { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public List<SelectListItem> DepartmentUsersList { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string[] SelectedDepartment { get; set; }
        public virtual ICollection<UserRoleModel> UserRoles { get; set; }
    }

    public class UsersManagementListingService
    {
        private db_KISDEntities _context;

        /// <summary>
        /// Gallery Listing Service
        /// </summary>
        public UsersManagementListingService()
        {
            _context = new db_KISDEntities();
        }

        /// <summary>
        /// IQueryable<UsersModel> Method for Getting the User Listing Model.
        /// </summary>
        /// <returns>UserModel</returns>
        public IQueryable<UsersModel> GetUserListingView(Int64 CurrentUserRoleID,string username,string usertype)
        {
            var searchusername = !string.IsNullOrEmpty(username) ? username : "";
            var rollid = Convert.ToInt16(!string.IsNullOrEmpty(usertype) ? usertype : "0");
            var query = from x in GetUserListing()
                        join y in _context.UserRoles on x.UserID equals y.UserID
                        where y.RoleID > CurrentUserRoleID && (rollid == 0 || y.RoleID == rollid) && (string.IsNullOrEmpty(searchusername) || x.UserNameTxt.Contains(searchusername))
                        select new UsersModel
                        {
                            UserID = x.UserID,
                            FirstName = x.FirstNameTxt,
                            LastName = x.LastNameTxt,
                            Name = x.FirstNameTxt + " " + x.LastNameTxt,
                            UserNameTxt = x.UserNameTxt,
                            Password = x.PasswordTxt,
                            Email = x.EmailTxt,
                            Status = x.StatusInd,
                            IsDeleted = x.IsDeletedInd,
                            UserCreateDate = x.UserCreateDate.HasValue ? x.UserCreateDate.Value : DateTime.Now,
                            CreateDate = x.CreateDate.HasValue ? x.CreateDate.Value : DateTime.Now,
                            CreatedByUserID = x.CreateByID,
                            LastModifyDate = x.LastModifyDate.HasValue ? x.LastModifyDate.Value : DateTime.Now,
                            LastModifyByUserID = x.LastModifyByID,
                            UserRoleID = y.RoleID
                        };
            return query.AsQueryable();
        }

        /// <summary>
        /// IQueryable<User> get the User Listing Which are not deleted from the website.
        /// </summary>
        /// <returns>UserObject</returns>
        public IQueryable<User> GetUserListing()
        {
            return _context.Users.Where(x => (x.IsDeletedInd == false));
        }

    }
}
