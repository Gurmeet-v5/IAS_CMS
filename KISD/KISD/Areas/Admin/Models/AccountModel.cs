using System.ComponentModel.DataAnnotations;

namespace KISD.Areas.Admin.Models
{
    public class AccountModel
    {
        [Display(Name = "UserName")]
        [Required(ErrorMessage = "User Name is required")]
        public string UserNameTxt { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        
        public string Email { get; set; }

        public string Message { get; set; }

        [Display(Name = "RememberMe")]
        public bool RememberMe { get; set; }
        public bool IsCheckedRememberMe { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "This field is required.")]
        [RegularExpression(@"^.*(?=.{6,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$", ErrorMessage = "Password must be 6 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [RegularExpression(@"^.*(?=.{6,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$", ErrorMessage = "Password must be 6 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [RegularExpression(@"^.*(?=.{6,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$", ErrorMessage = "Password must be 6 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.")]
        [Compare("NewPassword", ErrorMessage = "Confirm  New Password should be same as New Password.")]
        public string ConfirmNewPassword { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "This field is required.")]
        //[RegularExpression(@"^.*(?=.{8,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$", ErrorMessage = "Password must be 8 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        //[RegularExpression(@"^.*(?=.{8,20})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&+=]).*$", ErrorMessage = "Password must be 8 to 20 alphanumeric characters including one uppercase letter, one lowercase letter and one special character.")]
        [Compare("NewPassword", ErrorMessage = "Confirm  New Password should be same as New Password.")]
        public string ConfirmPassword { get; set; }

        public long UserID { get; set; }
    }
}
