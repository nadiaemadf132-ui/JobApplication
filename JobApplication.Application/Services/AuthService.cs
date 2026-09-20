using JobApplication.Application.DTOs.Auth;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Services
{
    public class AuthService : IAuthService
    {
        private const string InvalidCredentialsMessage = "Invalid email or password.";
        private const string InvalidTokenMessage = "Invalid or expired token.";

        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IAuthEmailService _authEmailService;
        private readonly ICurrentUser _currentUser;
        private readonly TimeProvider _timeProvider;

        public AuthService(
            IIdentityService identityService,
            ITokenService tokenService,
            IRefreshTokenRepository refreshTokenRepository,
            IAuthEmailService authEmailService,
            ICurrentUser currentUser,
            TimeProvider timeProvider)
        {
            _identityService = identityService;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _authEmailService = authEmailService;
            _currentUser = currentUser;
            _timeProvider = timeProvider;
        }

        public async Task RegisterAsync(RegisterDto dto)
        {
            var email = dto.Email.Trim();
            var result = await _identityService.CreateUserAsync(email, dto.Password);
            if (!result.Succeeded)
            {
                if (result.IsDuplicateEmail)
                    throw new ConflictException("Email is already registered.");
                throw new BusinessValidationException(result.Errors);
            }

            var token = await _identityService.GenerateEmailConfirmationTokenAsync(result.UserId!);
            await _authEmailService.SendEmailConfirmationAsync(result.UserId!, email, token);
        }

        public async Task ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var result = await _identityService.ConfirmEmailAsync(dto.UserId, dto.Token);
            if (!result.Succeeded)
                throw new BusinessValidationException(InvalidTokenMessage);
        }

        public async Task ResendConfirmationAsync(ResendConfirmationDto dto)
        {
            // Always succeeds silently so the endpoint cannot be used to discover registered emails.
            var user = await _identityService.FindByEmailAsync(dto.Email.Trim());
            if (user is null || user.EmailConfirmed)
                return;

            var token = await _identityService.GenerateEmailConfirmationTokenAsync(user.Id);
            await _authEmailService.SendEmailConfirmationAsync(user.Id, user.Email, token);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _identityService.FindByEmailAsync(dto.Email.Trim())
                ?? throw new UnauthorizedException(InvalidCredentialsMessage);

            var passwordCheck = await _identityService.CheckPasswordAsync(user.Id, dto.Password);
            if (passwordCheck == PasswordCheckResult.LockedOut)
                throw new ForbiddenException("Account is temporarily locked. Please try again later.");
            if (passwordCheck != PasswordCheckResult.Success)
                throw new UnauthorizedException(InvalidCredentialsMessage);

            if (!user.EmailConfirmed)
                throw new ForbiddenException("Email is not confirmed. Please confirm your email before logging in.");

            return await IssueTokensAsync(user);
        }

        public async Task<AuthResponseDto> RefreshAsync(RefreshTokenDto dto)
        {
            var now = UtcNow();
            var stored = await _refreshTokenRepository.GetByHashAsync(_tokenService.HashToken(dto.RefreshToken))
                ?? throw new UnauthorizedException("Invalid refresh token.");

            if (stored.RevokedAt is not null)
            {
                // A revoked token is being replayed: assume theft and kill every session of this user.
                await RevokeAllRefreshTokensAsync(stored.UserId);
                throw new UnauthorizedException("Invalid refresh token.");
            }

            if (stored.ExpiresAt <= now)
                throw new UnauthorizedException("Refresh token has expired.");

            var user = await _identityService.FindByIdAsync(stored.UserId);
            if (user is null || !user.EmailConfirmed)
                throw new UnauthorizedException("Invalid refresh token.");

            var response = await IssueTokensAsync(user, save: false);
            stored.RevokedAt = now;
            stored.ReplacedByTokenHash = _tokenService.HashToken(response.RefreshToken);
            await _refreshTokenRepository.SaveChangesAsync();

            return response;
        }

        public async Task LogoutAsync(LogoutDto dto)
        {
            var userId = GetCurrentUserId();
            var stored = await _refreshTokenRepository.GetByHashAsync(_tokenService.HashToken(dto.RefreshToken));

            // Idempotent: unknown / foreign / already revoked tokens are ignored.
            if (stored is null || stored.UserId != userId || stored.RevokedAt is not null)
                return;

            stored.RevokedAt = UtcNow();
            await _refreshTokenRepository.SaveChangesAsync();
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            // Always succeeds silently so the endpoint cannot be used to discover registered emails.
            var user = await _identityService.FindByEmailAsync(dto.Email.Trim());
            if (user is null)
                return;

            var token = await _identityService.GeneratePasswordResetTokenAsync(user.Id);
            await _authEmailService.SendPasswordResetAsync(user.Email, token);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _identityService.FindByEmailAsync(dto.Email.Trim())
                ?? throw new BusinessValidationException(InvalidTokenMessage);

            var result = await _identityService.ResetPasswordAsync(user.Id, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                throw new BusinessValidationException(result.Errors);

            await RevokeAllRefreshTokensAsync(user.Id);
        }

        public async Task RequestEmailChangeAsync(ChangeEmailDto dto)
        {
            var userId = GetCurrentUserId();
            var user = await _identityService.FindByIdAsync(userId)
                ?? throw new UnauthorizedException("User no longer exists.");

            var newEmail = dto.NewEmail.Trim();
            if (string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
                throw new BusinessValidationException("New email must be different from the current email.");

            if (await _identityService.CheckPasswordAsync(userId, dto.CurrentPassword) != PasswordCheckResult.Success)
                throw new BusinessValidationException("Current password is incorrect.");

            if (await _identityService.FindByEmailAsync(newEmail) is not null)
                throw new ConflictException("Email is already registered.");

            var token = await _identityService.GenerateChangeEmailTokenAsync(userId, newEmail);
            await _authEmailService.SendEmailChangeConfirmationAsync(userId, newEmail, token);
        }

        public async Task ConfirmEmailChangeAsync(ConfirmEmailChangeDto dto)
        {
            var result = await _identityService.ChangeEmailAsync(dto.UserId, dto.NewEmail.Trim(), dto.Token);
            if (!result.Succeeded)
            {
                if (result.IsDuplicateEmail)
                    throw new ConflictException("Email is already registered.");
                throw new BusinessValidationException(InvalidTokenMessage);
            }

            await RevokeAllRefreshTokensAsync(dto.UserId);
        }

        private async Task<AuthResponseDto> IssueTokensAsync(IdentityUserDto user, bool save = true)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _refreshTokenRepository.InsertAsync(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = _tokenService.HashToken(refreshToken.Token),
                CreatedAt = UtcNow(),
                ExpiresAt = refreshToken.ExpiresAt
            });

            if (save)
                await _refreshTokenRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken.Token,
                AccessTokenExpiresAt = accessToken.ExpiresAt,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresAt = refreshToken.ExpiresAt
            };
        }

        private async Task RevokeAllRefreshTokensAsync(string userId)
        {
            var now = UtcNow();
            var unrevokedTokens = await _refreshTokenRepository.GetUnrevokedByUserIdAsync(userId);
            foreach (var token in unrevokedTokens)
                token.RevokedAt = now;

            await _refreshTokenRepository.SaveChangesAsync();
        }

        private string GetCurrentUserId()
            => _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
    }
}
