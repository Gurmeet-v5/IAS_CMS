using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KISD.Areas.BlogAdmin.Models
{
    public partial class UsersModel
    {
        public UsersModel()
        {
            //this.UserRoles = new HashSet<UserRoleModel>();
            this.UserID = 0;
            this.FirstName = "";
            this.LastName = "";
            this.UserNameTxt = "";
            this.Password = "";
            this.EmailTxt = "";
            this.StatusInd = true;
        }

        public UsersModel(int UserID, string FirstName, string LastName, string UserNameTxt, string Password, string EmailTxt, bool StatusInd)
        {
            //this.UserRoles = new HashSet<UserRoleModel>();
            this.UserID = UserID;
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.UserNameTxt = UserNameTxt;
            this.Password = Password;
            this.EmailTxt = EmailTxt;
            this.StatusInd = StatusInd;
        }

        public long UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [StringLength(20, MinimumLength = 6, ErrorMessage = "Username must be 6 to 20 characters.")]
        [RegularExpression(@"^.*(?=.{6,20})(?=.*\d)(?=.*[a-zA-Z]).*$", ErrorMessage = "Username must be minimum of 6 alphanumeric characters including atleast one numeric value.")]
        public string UserNameTxt { get; set; }
        public string UserNameTxt_ { get; set; }

        [RegularExpression(@"^.*(?=.{6,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$", ErrorMessage = "Password must be 6 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.")]
        public string Password { get; set; }
        public string Password_ { get; set; }
        public string EmailTxt { get; set; }
        public Boolean StatusInd { get; set; }

        public virtual ICollection<UserRoleModel> UserRoles { get; set; }
    }
}