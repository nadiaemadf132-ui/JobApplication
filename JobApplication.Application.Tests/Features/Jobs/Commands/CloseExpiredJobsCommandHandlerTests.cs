using Hangfire;
using Hangfire.States;
using JobApplication.Application.Features.Jobs.Commands.CloseExpiredJobs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace JobApplication.Application.Tests.Features.Jobs.Commands
{
    public class CloseExpiredJobsCommandHandlerTests
    {
        private static readonly DateTimeOffset Now = new(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);

        private readonly IJobRepository _jobRepository = Substitute.For<IJobRepository>();
        private readonly IBackgroundJobClient _backgroundJobClient = Substitute.For<IBackgroundJobClient>();
        private readonly ILogger<CloseExpiredJobsCommandHandler> _logger =
            Substitute.For<ILogger<CloseExpiredJobsCommandHandler>>();
        private readonly FakeTimeProvider _timeProvider = new(Now);

        private CloseExpiredJobsCommandHandler CreateHandler() =>
            new(_jobRepository, _backgroundJobClient, _timeProvider, _logger);

        private static Job CreateExpiredJob(params JobApplicationStatus[] applicationStatuses)
        {
            var job = new Job
            {
                Id = 1,
                Title = "Backend Developer",
                Description = "Builds APIs.",
                IsActive = true,
                RecruiterId = "recruiter-1",
                ExpiryDate = Now.UtcDateTime.AddDays(-1)
            };

            var candidateId = 100;
            foreach (var status in applicationStatuses)
            {
                job.Applications.Add(new JobCandidateApplication
                {
                    Id = candidateId,
                    CandidateId = candidateId++,
                    JobId = job.Id,
                    JobApplicationStatus = status,
                    AppliedAt = Now.UtcDateTime.AddDays(-10),
                    StatusUpdatedAt = Now.UtcDateTime.AddDays(-10)
                });
            }

            return job;
        }

        [Fact]
        public async Task Handle_ClosesTheJob_AndCascadesUnderReviewApplicationsToClosed()
        {
            var job = CreateExpiredJob(JobApplicationStatus.UnderReview, JobApplicationStatus.Accepted);
            _jobRepository.GetExpiredOpenJobsAsync(Now.UtcDateTime).Returns(new List<Job> { job });

            await CreateHandler().Handle(new CloseExpiredJobsCommand(), CancellationToken.None);

            Assert.False(job.IsActive);
            Assert.Equal(Now.UtcDateTime, job.ClosedAt);
            Assert.Equal("system", job.ClosedBy);

            var underReviewApp = job.Applications.Single(a => a.CandidateId == 100);
            Assert.Equal(JobApplicationStatus.Closed, underReviewApp.JobApplicationStatus);
            Assert.Equal(Now.UtcDateTime, underReviewApp.StatusUpdatedAt);

            var acceptedApp = job.Applications.Single(a => a.CandidateId == 101);
            Assert.Equal(JobApplicationStatus.Accepted, acceptedApp.JobApplicationStatus);

            await _jobRepository.Received(1).SaveChangesAsync();
            _backgroundJobClient.Received(1).Create(
                Arg.Any<Hangfire.Common.Job>(), Arg.Any<IState>());
        }

        [Fact]
        public async Task Handle_DoesNothing_WhenNoJobsAreExpired()
        {
            _jobRepository.GetExpiredOpenJobsAsync(Now.UtcDateTime).Returns(new List<Job>());

            await CreateHandler().Handle(new CloseExpiredJobsCommand(), CancellationToken.None);

            await _jobRepository.DidNotReceive().SaveChangesAsync();
            _backgroundJobClient.DidNotReceive().Create(Arg.Any<Hangfire.Common.Job>(), Arg.Any<IState>());
        }

        [Fact]
        public async Task Handle_DoesNotEnqueueNotifications_ForApplicationsNotUnderReview()
        {
            var job = CreateExpiredJob(JobApplicationStatus.Rejected, JobApplicationStatus.Applied);
            _jobRepository.GetExpiredOpenJobsAsync(Now.UtcDateTime).Returns(new List<Job> { job });

            await CreateHandler().Handle(new CloseExpiredJobsCommand(), CancellationToken.None);

            Assert.All(job.Applications, a => Assert.NotEqual(JobApplicationStatus.Closed, a.JobApplicationStatus));
            _backgroundJobClient.DidNotReceive().Create(Arg.Any<Hangfire.Common.Job>(), Arg.Any<IState>());
            await _jobRepository.Received(1).SaveChangesAsync();
        }
    }
}
