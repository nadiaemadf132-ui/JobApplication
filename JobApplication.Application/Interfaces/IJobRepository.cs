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
    }
}
