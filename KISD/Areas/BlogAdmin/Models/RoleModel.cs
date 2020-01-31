using System.ComponentModel.DataAnnotations;

namespace KISD.Areas.BlogAdmin.Models
{
    #region Bussiness objects

    public class RoleModel
    {
        public RoleModel()
        {
            //this.UserRoles = new HashSet<UserRoleModel>();
            this.RoleId = 0;
            this.RoleName = "";
            this.RoleDescription = "";
            this.LoweredRoleNameTxt = "";
        }

        public RoleModel(short RoleId, string RoleName, string RoleDescription, string LoweredRoleNameTxt)
        {
            //this.UserRoles = new HashSet<UserRoleModel>();
            this.RoleId = RoleId;
            this.RoleName = RoleName;
            this.RoleDescription = RoleDescription;
            this.LoweredRoleNameTxt = LoweredRoleNameTxt;
        }

        [Key]
        public short RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public string LoweredRoleNameTxt { get; set; }
        internal static bool IsUserInRole(string role)
        {
            throw new System.NotImplementedException();
        }
    }
    #endregion
}