using Hangfire;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JobApplication.Application.Features.Jobs.Commands.CloseExpiredJobs
{
    public class CloseExpiredJobsCommandHandler : IRequestHandler<CloseExpiredJobsCommand>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<CloseExpiredJobsCommandHandler> _logger;

        public CloseExpiredJobsCommandHandler(
            IJobRepository jobRepository,
            IBackgroundJobClient backgroundJobClient,
            TimeProvider timeProvider,
            ILogger<CloseExpiredJobsCommandHandler> logger)
        {
            _jobRepository = jobRepository;
            _backgroundJobClient = backgroundJobClient;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task Handle(CloseExpiredJobsCommand request, CancellationToken cancellationToken)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;

            var expiredJobs = await _jobRepository.GetExpiredOpenJobsAsync(now);

            if (expiredJobs.Count == 0)
            {
                _logger.LogInformation("CloseExpiredJobs: no expired open jobs found at {RunTimeUtc}.", now);
                return;
            }

            var closedApplicationsCount = 0;

            foreach (var job in expiredJobs)
            {
                job.IsActive = false;
                job.ClosedAt = now;
                job.ClosedBy = "system";

                var applicationsToClose = job.Applications
                    .Where(a => a.JobApplicationStatus == JobApplicationStatus.UnderReview)
                    .ToList();

                foreach (var application in applicationsToClose)
                {
                    application.JobApplicationStatus = JobApplicationStatus.Closed;
                    application.StatusUpdatedAt = now;
                    closedApplicationsCount++;

                    // Enqueue as its own Hangfire job so a slow/failing notification
                    // never blocks or rolls back the closing job itself.
                    var candidateId = application.CandidateId;
                    _backgroundJobClient.Enqueue<INotificationService>(
                        notificationService => notificationService.NotifyCandidate(candidateId));
                }
            }

            await _jobRepository.SaveChangesAsync();

            _logger.LogInformation(
                "CloseExpiredJobs: closed {JobCount} job(s) and {ApplicationCount} related application(s) at {RunTimeUtc}.",
                expiredJobs.Count, closedApplicationsCount, now);
        }
    }
}
