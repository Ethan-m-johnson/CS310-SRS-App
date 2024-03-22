using System.ComponentModel.DataAnnotations;

namespace CS310_SRS_App.Model.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
