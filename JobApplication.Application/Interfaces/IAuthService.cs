using JobApplication.Application.DTOs.Auth;

namespace JobApplication.Application.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterDto dto);
        Task ConfirmEmailAsync(ConfirmEmailDto dto);
        Task ResendConfirmationAsync(ResendConfirmationDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RefreshAsync(RefreshTokenDto dto);
        Task LogoutAsync(LogoutDto dto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task RequestEmailChangeAsync(ChangeEmailDto dto);
        Task ConfirmEmailChangeAsync(ConfirmEmailChangeDto dto);
    }
}
