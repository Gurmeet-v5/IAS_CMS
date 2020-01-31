namespace KISD.Areas.Admin.Models
{
    public class UserRoleModel
    {
        public int UserRoleID { get; set; }
        public long UserID { get; set; }
        public short RoleID { get; set; }
        public virtual RoleModel Role { get; set; }
        public virtual UsersModel User { get; set; }
    }
}