namespace JobApplication.Application.DTOs.Auth
{
    public class IdentityUserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
