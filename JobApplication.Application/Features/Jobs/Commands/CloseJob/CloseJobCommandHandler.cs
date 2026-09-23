using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob
{
    public class CloseJobCommandHandler : IRequestHandler<CloseJobCommand>
    {
        private readonly IJobRepository _jobRepository;
        private readonly ICurrentUser _currentUser;
        private readonly TimeProvider _timeProvider;

        public CloseJobCommandHandler(
            IJobRepository jobRepository, ICurrentUser currentUser, TimeProvider timeProvider)
        {
            _jobRepository = jobRepository;
            _currentUser = currentUser;
            _timeProvider = timeProvider;
        }

        public async Task Handle(CloseJobCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException("User is not authenticated.");

            var job = await _jobRepository.GetByIdAsync(request.JobId)
                ?? throw new NotFoundException($"Job {request.JobId} was not found.");

            if (job.RecruiterId != userId)
                throw new ForbiddenException("Only the recruiter who owns this job can close it.");

            if (job.ClosedAt.HasValue)
                throw new ConflictException("Job is already closed.");

            job.IsActive = false;
            job.ClosedAt = _timeProvider.GetUtcNow().UtcDateTime;
            job.ClosedBy = userId;

            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();
        }
    }
}
