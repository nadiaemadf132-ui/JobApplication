using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.DTOs.Auth
{
    public class ResendConfirmationDto
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; }
    }
}
