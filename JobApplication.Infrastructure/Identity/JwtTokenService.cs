using JobApplication.Application.DTOs.Auth;
using JobApplication.Application.Interfaces;
using JobApplication.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Buffers.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JobApplication.Infrastructure.Identity
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly TimeProvider _timeProvider;

        public JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
        {
            _options = options.Value;
            _timeProvider = timeProvider;
        }

        public TokenResult GenerateAccessToken(IdentityUserDto user)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                NotBefore = now,
                IssuedAt = now,
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            return new TokenResult
            {
                Token = new JsonWebTokenHandler().CreateToken(descriptor),
                ExpiresAt = expiresAt
            };
        }

        public TokenResult GenerateRefreshToken()
        {
            return new TokenResult
            {
                Token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(_options.RefreshTokenDays)
            };
        }

        public string HashToken(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }
    }
}
