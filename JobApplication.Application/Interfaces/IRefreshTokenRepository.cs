using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task InsertAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByHashAsync(string tokenHash);
        Task<List<RefreshToken>> GetUnrevokedByUserIdAsync(string userId);
        Task SaveChangesAsync();
    }
}
