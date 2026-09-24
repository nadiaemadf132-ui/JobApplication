using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public class Job
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description  { get; set; }
        public bool IsActive { get; set; }
        public string RecruiterId { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? ClosedBy { get; set; }
        public DateTime ExpiryDate { get; set; }

        public ICollection<JobCandidateApplication> Applications { get; set; } = new List<JobCandidateApplication>();
    }
}
