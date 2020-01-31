using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace KISD.Areas.BlogAdmin.Models
{
    public class UserRoleModel
    {
        public UserRoleModel()
        {
            //this.UserRoles = new HashSet<UserRoleModel>();
            this.UserRoleID = 0;
            this.UserID = 0;
            this.RoleID = 0;
        }

        public UserRoleModel(short RoleId, int UserRoleID, int UserID)
        {
            var _roleContext = new Contexts.RolesContexts();
            var _userContext = new Contexts.UsersContexts();
           
            //this.UserRoles = new HashSet<UserRoleModel>();
            this.UserRoleID = UserRoleID;
            this.UserID = UserID;
            this.RoleID = RoleId;
            this.Role=_roleContext.GetRoles().AsEnumerable();
            this.User = _userContext.GetUsers().AsEnumerable();
        }
        public int UserRoleID { get; set; }
        public long UserID { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public short RoleID { get; set; }
        public string UserName { get; set; }

        public virtual IEnumerable<RoleModel> Role { get; set; }
        public virtual IEnumerable<UsersModel> User { get; set; }
        
    }
}