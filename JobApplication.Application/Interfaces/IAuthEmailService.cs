namespace JobApplication.Application.Interfaces
{
    public interface IAuthEmailService
    {
        Task SendEmailConfirmationAsync(string userId, string email, string token);
        Task SendPasswordResetAsync(string email, string token);
        Task SendEmailChangeConfirmationAsync(string userId, string newEmail, string token);
    }
}
