using System.ComponentModel.DataAnnotations;

namespace KISD.Areas.Admin.Models
{
    #region Bussiness objects
		
    public class RoleModel
    {
        [Key]
        public short RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }

        internal static bool IsUserInRole(string role)
        {
            throw new System.NotImplementedException();
        }
    } 
	#endregion
}