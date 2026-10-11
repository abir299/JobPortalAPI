
using JobPortalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortalAPI.Data
{
    public class JobPortalDbContext : DbContext
    {
        public JobPortalDbContext(DbContextOptions<JobPortalDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Job>()
                .Property(j => j.Salary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Job>()
                .HasOne(j => j.Employer)
                .WithMany()
                .HasForeignKey(j => j.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.JobSeeker)
                .WithMany()
                .HasForeignKey(a => a.JobSeekerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.Job)
                .WithMany()
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobApplication>()
                .HasIndex(a => new { a.JobId, a.JobSeekerId })
                .IsUnique();
        }
    }
}
