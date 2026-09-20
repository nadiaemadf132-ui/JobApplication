using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.DTOs.Auth
{
    public class ResetPasswordDto
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required, StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; }

        [Required, Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}
