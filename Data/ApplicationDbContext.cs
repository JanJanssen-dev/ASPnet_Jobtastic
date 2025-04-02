using ASPnet_Jobtastic.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASPnet_Jobtastic.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<JobPostingModel> JobPostings { get; set; }
        public DbSet<JobSharingModel> JobSharings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguriere JobSharingModel
            modelBuilder.Entity<JobSharingModel>()
                .HasOne(j => j.JobPosting)
                .WithMany()
                .HasForeignKey(j => j.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade); // Wenn ein JobPosting gelöscht wird, werden auch alle zugehörigen Freigaben gelöscht
        }
    }
}