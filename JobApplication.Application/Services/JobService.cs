using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Services
{
    public class JobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly ICurrentUser _currentUser;
        private readonly TimeProvider _timeProvider;

        public JobService(IJobRepository jobRepository, ICurrentUser currentUser, TimeProvider timeProvider)
        {
            _jobRepository = jobRepository;
            _currentUser = currentUser;
            _timeProvider = timeProvider;
        }

        public async Task<int> CreateAsync(CreateJobDto createJobDto)
        {   
            var recruiterId = _currentUser.UserId
                ?? throw new UnauthorizedException("User is not authenticated.");

            var job = new Job()
            {
                Title = createJobDto.Title,
                Description = createJobDto.Description,
                IsActive = true,
                RecruiterId = recruiterId
            };
            await _jobRepository.InsertAsync(job);
            await _jobRepository.SaveChangesAsync();

            return job.Id; 
        }

        public async Task CloseAsync(int id)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException("User is not authenticated.");

            var job = await _jobRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Job {id} was not found.");

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
