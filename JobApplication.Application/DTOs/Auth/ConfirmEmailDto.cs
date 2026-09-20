using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.DTOs.Auth
{
    public class ConfirmEmailDto
    {
        [Required, StringLength(450)]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
