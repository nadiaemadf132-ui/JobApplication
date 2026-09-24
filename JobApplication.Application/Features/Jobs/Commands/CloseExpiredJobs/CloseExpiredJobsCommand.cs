using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CloseExpiredJobs
{
    /// <summary>
    /// System-triggered command that closes every open job whose ExpiryDate has
    /// passed, and cascades the closure to any candidate applications that are
    /// still Under Review (moving them to Closed).
    ///
    /// Unlike <see cref="JobApplication.Application.Features.Jobs.Commands.CloseJob.CloseJobCommand"/>,
    /// this has no recruiter/ownership check - it's meant to be run by the
    /// Hangfire recurring job (see Program.cs), not by an authenticated user
    /// request, so it does not go through ICurrentUser.
    /// </summary>
    public record CloseExpiredJobsCommand : IRequest;
}
