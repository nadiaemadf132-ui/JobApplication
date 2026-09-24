using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace JobApplication.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<JobCandidateApplication> JobCandidateApplications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Job>(job =>
            {
                job.Property(j => j.RecruiterId).HasMaxLength(450);
                job.Property(j => j.ClosedBy).HasMaxLength(450);
                job.HasIndex(j => j.RecruiterId);
                job.HasIndex(j => new { j.IsActive, j.ExpiryDate });
            });

            builder.Entity<RefreshToken>(token =>
            {
                token.Property(t => t.UserId).IsRequired().HasMaxLength(450);
                token.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);
                token.Property(t => t.ReplacedByTokenHash).HasMaxLength(64);
                token.HasIndex(t => t.TokenHash).IsUnique();
                token.HasIndex(t => t.UserId);
                token.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
