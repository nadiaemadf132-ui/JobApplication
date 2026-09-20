using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.DTOs.Auth
{
    public class LogoutDto
    {
        [Required, StringLength(512)]
        public string RefreshToken { get; set; }
    }
}
