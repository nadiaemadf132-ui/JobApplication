using System.ComponentModel.DataAnnotations;

namespace JobApplication.Infrastructure.Options
{
    public class EmailOptions
    {
        public const string SectionName = "Email";

        [Required]
        public string Host { get; set; }

        [Range(1, 65535)]
        public int Port { get; set; } = 587;

        [Required]
        public string UserName { get; set; }

        // Gmail App Password. Never stored in appsettings.json.
        // Supply through User Secrets or the Email__Password environment variable.
        public string? Password { get; set; }

        [Required, EmailAddress]
        public string FromAddress { get; set; }

        [Required]
        public string FromName { get; set; }
    }
}
