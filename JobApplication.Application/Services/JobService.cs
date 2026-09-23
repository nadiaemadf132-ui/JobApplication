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
    }
}
