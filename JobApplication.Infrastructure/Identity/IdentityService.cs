using JobApplication.Application.DTOs.Auth;
using JobApplication.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace JobApplication.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private const string InvalidTokenError = "Invalid or expired token.";

        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityOperationResult> CreateUserAsync(string email, string password)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);
            return result.Succeeded
                ? IdentityOperationResult.Success(user.Id)
                : ToFailure(result);
        }

        public async Task<IdentityUserDto?> FindByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user is null ? null : ToDto(user);
        }

        public async Task<IdentityUserDto?> FindByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user is null ? null : ToDto(user);
        }

        public async Task<PasswordCheckResult> CheckPasswordAsync(string userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return PasswordCheckResult.InvalidPassword;

            if (await _userManager.IsLockedOutAsync(user))
                return PasswordCheckResult.LockedOut;

            if (await _userManager.CheckPasswordAsync(user, password))
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                return PasswordCheckResult.Success;
            }

            await _userManager.AccessFailedAsync(user);
            return await _userManager.IsLockedOutAsync(user)
                ? PasswordCheckResult.LockedOut
                : PasswordCheckResult.InvalidPassword;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            var user = await GetRequiredUserAsync(userId);
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityOperationResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return IdentityOperationResult.Failure(new[] { InvalidTokenError });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded ? IdentityOperationResult.Success(user.Id) : ToFailure(result);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string userId)
        {
            var user = await GetRequiredUserAsync(userId);
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityOperationResult> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return IdentityOperationResult.Failure(new[] { InvalidTokenError });

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
                return ToFailure(result);

            // Owning the mailbox is proof of identity, so a reset also lifts any lockout.
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            return IdentityOperationResult.Success(user.Id);
        }

        public async Task<string> GenerateChangeEmailTokenAsync(string userId, string newEmail)
        {
            var user = await GetRequiredUserAsync(userId);
            return await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        }

        public async Task<IdentityOperationResult> ChangeEmailAsync(string userId, string newEmail, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return IdentityOperationResult.Failure(new[] { InvalidTokenError });

            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
            if (!result.Succeeded)
                return ToFailure(result);

            // The email is also the user name, so keep both in sync.
            var userNameResult = await _userManager.SetUserNameAsync(user, newEmail);
            return userNameResult.Succeeded ? IdentityOperationResult.Success(user.Id) : ToFailure(userNameResult);
        }

        private async Task<ApplicationUser> GetRequiredUserAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException($"User '{userId}' was not found.");
        }

        private static IdentityUserDto ToDto(ApplicationUser user)
        {
            return new IdentityUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed
            };
        }

        private static IdentityOperationResult ToFailure(IdentityResult result)
        {
            var isDuplicate = result.Errors.Any(e =>
                e.Code == nameof(IdentityErrorDescriber.DuplicateEmail) ||
                e.Code == nameof(IdentityErrorDescriber.DuplicateUserName));

            return IdentityOperationResult.Failure(result.Errors.Select(e => e.Description), isDuplicate);
        }
    }
}
