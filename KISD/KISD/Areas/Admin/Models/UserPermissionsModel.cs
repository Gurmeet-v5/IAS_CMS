using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public class UserPermissionsModel
    {
        public long UserPermissionID { get; set; }
        public Nullable<long> UserID { get; set; }
        public Nullable<long> PageID { get; set; }
        public Nullable<bool> IsAccessInd { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<short> UserRoleID { get; set; }
        public List<SelectListItem> PermissionList { get; set; }
        [Required(ErrorMessage ="This field is required.")]
        public string[] SelectedUserPermissions { get; set; }
        public virtual Role Role { get; set; }
        public virtual User User { get; set; }

    }
}

