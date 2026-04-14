using System.ComponentModel.DataAnnotations;

namespace online_store_api.Models.User
{
    public class ResetPasswordDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        [Required]
        public string NewPassword { get; set; } = string.Empty;
        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
