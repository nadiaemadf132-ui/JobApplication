using JobApplication.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobApplication.Infrastructure.Notifications
{
    /// <summary>
    /// Placeholder notification service. The Candidate entity doesn't currently
    /// have an email address, so this only logs; once Candidate exposes an email
    /// (or is linked to an ApplicationUser), swap this to call the existing
    /// IEmailSender the same way AuthEmailService does.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task NotifyCandidate(int candidateId)
        {
            _logger.LogInformation(
                "Candidate {CandidateId} notified: one of their applications was closed because the job expired.",
                candidateId);

            return Task.CompletedTask;
        }
    }
}
