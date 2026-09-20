using JobApplication.Application.DTOs.Auth;

namespace JobApplication.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<IdentityOperationResult> CreateUserAsync(string email, string password);
        Task<IdentityUserDto?> FindByEmailAsync(string email);
        Task<IdentityUserDto?> FindByIdAsync(string userId);
        Task<PasswordCheckResult> CheckPasswordAsync(string userId, string password);

        Task<string> GenerateEmailConfirmationTokenAsync(string userId);
        Task<IdentityOperationResult> ConfirmEmailAsync(string userId, string token);

        Task<string> GeneratePasswordResetTokenAsync(string userId);
        Task<IdentityOperationResult> ResetPasswordAsync(string userId, string token, string newPassword);

        Task<string> GenerateChangeEmailTokenAsync(string userId, string newEmail);
        Task<IdentityOperationResult> ChangeEmailAsync(string userId, string newEmail, string token);
    }
}
