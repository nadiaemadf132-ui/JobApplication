namespace JobApplication.Application.DTOs.Auth
{
    public class TokenResult
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
