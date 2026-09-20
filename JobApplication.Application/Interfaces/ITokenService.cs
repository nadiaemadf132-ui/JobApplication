using JobApplication.Application.DTOs.Auth;

namespace JobApplication.Application.Interfaces
{
    public interface ITokenService
    {
        TokenResult GenerateAccessToken(IdentityUserDto user);
        TokenResult GenerateRefreshToken();
        string HashToken(string token);
    }
}
