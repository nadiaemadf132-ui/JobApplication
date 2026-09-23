using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob
{
    /// <summary>
    /// Closes an open job posting. Only the recruiter who owns the job may close it,
    /// and a job can only be closed once.
    /// </summary>
    /// <param name="JobId">Identifier of the job to close.</param>
    public record CloseJobCommand(int JobId) : IRequest;
}
