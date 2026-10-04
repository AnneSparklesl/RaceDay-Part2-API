using API_proj.Models;
using Microsoft.EntityFrameworkCore;

namespace API_proj.data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Venue> Venues => Set<Venue>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.ToTable(t => t.HasCheckConstraint("CK_User_Role", "[role] IN ('Organiser', 'Participant')"));
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_Event_EventType", "[event_type] IN ('Run', 'Walk', 'Cycle')"));
            });

            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.Property(e => e.EnrolmentDate).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasOne(r => r.Enrolment)
                      .WithOne(e => e.Result)
                      .HasForeignKey<Result>(r => r.EnrolmentId);
            });

            // No cascade deletes, same as your Part 1 SQL script. Keep this block last.
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}