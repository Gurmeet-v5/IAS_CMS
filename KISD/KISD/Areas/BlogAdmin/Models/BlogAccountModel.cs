using System.ComponentModel.DataAnnotations;

namespace KISD.Areas.BlogAdmin.Models
{
    public class BlogAccountModel
    {
        /// <summary>
        /// This class is used in Account Controller Login View.
        /// To set the validations on Username and Password fields.
        /// </summary>

        [Display(Name = "Username")]
        [Required(ErrorMessage = "This field is required.")]
        public string UserNameTxt { get; set; }


        [Display(Name = "Password")]
        [Required(ErrorMessage = "This field is required.")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

    }
    /// <summary>
    /// This class is used for Change Password in Account Controller.
    /// </summary>
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "This field is required.")]
        [RegularExpression(@"^.*(?=.{8,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$", ErrorMessage = "Password must be 8 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [RegularExpression(@"^.*(?=.{8,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$", ErrorMessage = "Password must be 8 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [RegularExpression(@"^.*(?=.{8,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$", ErrorMessage = "Password must be 8 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.")]
        [Compare("NewPassword", ErrorMessage = "Confirm  New Password should be same as New Password.")]
        public string ConfirmPassword { get; set; }
    }
}


