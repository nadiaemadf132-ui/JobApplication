using System.ComponentModel.DataAnnotations;

namespace JobApplication.Infrastructure.Options
{
    public class AppUrlOptions
    {
        public const string SectionName = "App";

        // Public base URL of this API, used to build the links sent by email (e.g. https://localhost:7237).
        [Required, Url]
        public string ApiBaseUrl { get; set; }
    }
}
