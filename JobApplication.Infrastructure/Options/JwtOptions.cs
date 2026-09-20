using System.ComponentModel.DataAnnotations;

namespace JobApplication.Infrastructure.Options
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required]
        public string Issuer { get; set; }

        [Required]
        public string Audience { get; set; }

        // Never stored in appsettings.json. Supply through User Secrets or the Jwt__SecretKey environment variable.
        [Required, MinLength(32)]
        public string SecretKey { get; set; }

        [Range(1, 1440)]
        public int AccessTokenMinutes { get; set; } = 15;

        [Range(1, 365)]
        public int RefreshTokenDays { get; set; } = 7;
    }
}
