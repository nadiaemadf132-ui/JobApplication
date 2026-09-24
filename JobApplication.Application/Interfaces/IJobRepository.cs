using JobApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Interfaces
{
    public interface IJobRepository
    {
        Task InsertAsync(Job job);
        Task<Job?> GetByIdAsync(int id);
        void Update(Job job);
        IQueryable<Job> Get();
        void Remove(Job job);
        Task SaveChangesAsync();

        /// <summary>
        /// Returns all jobs that are still open (IsActive) but whose ExpiryDate has
        /// passed (as of <paramref name="asOfUtc"/>), including their related
        /// candidate applications, so the caller can update everything in one unit
        /// of work.
        /// </summary>
        Task<List<Job>> GetExpiredOpenJobsAsync(DateTime asOfUtc);
    }
}
