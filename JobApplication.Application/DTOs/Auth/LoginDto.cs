using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.DTOs.Auth
{
    public class LoginDto
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; }

        [Required, StringLength(100)]
        public string Password { get; set; }
    }
}
