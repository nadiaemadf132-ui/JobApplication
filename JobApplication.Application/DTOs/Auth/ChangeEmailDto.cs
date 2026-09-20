using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.DTOs.Auth
{
    public class ChangeEmailDto
    {
        [Required, EmailAddress, StringLength(256)]
        public string NewEmail { get; set; }

        [Required, StringLength(100)]
        public string CurrentPassword { get; set; }
    }
}
