using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.DTOs.Auth
{
    public class ConfirmEmailChangeDto
    {
        [Required, StringLength(450)]
        public string UserId { get; set; }

        [Required, EmailAddress, StringLength(256)]
        public string NewEmail { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
