using JobApplication.Application.DTOs;
using JobApplication.Application.Features.Jobs.Commands.CloseJob;
using JobApplication.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly JobService _JobService;
        private readonly IMediator _mediator;

        public JobsController(JobService jobService, IMediator mediator)
        {
            _JobService = jobService;
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateJobDto createJobDto)
        {
            var id = await _JobService.CreateAsync(createJobDto);
            return Ok(new
            {
                id = id 
            }); 
        }

        /// <summary>
        /// Closes a job posting.
        /// </summary>
        /// <remarks>
        /// Only the recruiter who owns the job can close it, and a job can only be closed once.
        /// </remarks>
        /// <param name="id">Identifier of the job to close.</param>
        /// <param name="cancellationToken">Token used to cancel the request.</param>
        /// <response code="204">The job was closed.</response>
        /// <response code="401">The caller is not authenticated.</response>
        /// <response code="403">The caller is not the recruiter who owns the job.</response>
        /// <response code="404">No job exists with the supplied identifier.</response>
        /// <response code="409">The job is already closed.</response>
        [HttpPut("{id:int}/close")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Close(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new CloseJobCommand(id), cancellationToken);
            return NoContent();
        }
    }
}
