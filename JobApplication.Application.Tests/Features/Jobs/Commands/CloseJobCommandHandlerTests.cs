using JobApplication.Application.Exceptions;
using JobApplication.Application.Features.Jobs.Commands.CloseJob;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace JobApplication.Application.Tests.Features.Jobs.Commands
{
    public class CloseJobCommandHandlerTests
    {
        private const string RecruiterId = "recruiter-1";
        private static readonly DateTimeOffset Now = new(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);

        private readonly IJobRepository _jobRepository = Substitute.For<IJobRepository>();
        private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
        private readonly FakeTimeProvider _timeProvider = new(Now);

        private CloseJobCommandHandler CreateHandler() =>
            new(_jobRepository, _currentUser, _timeProvider);

        private static Job CreateJob(string recruiterId = RecruiterId) => new()
        {
            Id = 1,
            Title = "Backend Developer",
            Description = "Builds APIs.",
            IsActive = true,
            RecruiterId = recruiterId
        };

        [Fact]
        public async Task Handle_ClosesTheJob_WhenTheOwningRecruiterRequestsIt()
        {
            var job = CreateJob();
            _currentUser.UserId.Returns(RecruiterId);
            _jobRepository.GetByIdAsync(job.Id).Returns(job);

            await CreateHandler().Handle(new CloseJobCommand(job.Id), CancellationToken.None);

            Assert.False(job.IsActive);
            Assert.Equal(Now.UtcDateTime, job.ClosedAt);
            Assert.Equal(RecruiterId, job.ClosedBy);
            _jobRepository.Received(1).Update(job);
            await _jobRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_Throws_WhenTheCallerIsNotAuthenticated()
        {
            _currentUser.UserId.Returns((string?)null);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                CreateHandler().Handle(new CloseJobCommand(1), CancellationToken.None));

            await _jobRepository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_Throws_WhenTheJobDoesNotExist()
        {
            _currentUser.UserId.Returns(RecruiterId);
            _jobRepository.GetByIdAsync(42).Returns((Job?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                CreateHandler().Handle(new CloseJobCommand(42), CancellationToken.None));

            await _jobRepository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_Throws_WhenTheCallerDoesNotOwnTheJob()
        {
            var job = CreateJob("another-recruiter");
            _currentUser.UserId.Returns(RecruiterId);
            _jobRepository.GetByIdAsync(job.Id).Returns(job);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                CreateHandler().Handle(new CloseJobCommand(job.Id), CancellationToken.None));

            Assert.True(job.IsActive);
            await _jobRepository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_Throws_WhenTheJobIsAlreadyClosed()
        {
            var job = CreateJob();
            job.IsActive = false;
            job.ClosedAt = Now.UtcDateTime.AddDays(-1);
            _currentUser.UserId.Returns(RecruiterId);
            _jobRepository.GetByIdAsync(job.Id).Returns(job);

            await Assert.ThrowsAsync<ConflictException>(() =>
                CreateHandler().Handle(new CloseJobCommand(job.Id), CancellationToken.None));

            await _jobRepository.DidNotReceive().SaveChangesAsync();
        }
    }
}
