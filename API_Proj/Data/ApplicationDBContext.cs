using API_proj.Models;
using Microsoft.EntityFrameworkCore;

// 'Results' clashes with Microsoft.AspNetCore.Http.Results, so alias the model.
using ResultEntity = API_proj.Models.Results;

namespace API_proj.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }

          public DbSet<Category> Categories { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ResultEntity> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Events -> Users (organiser)
            modelBuilder.Entity<Event>()
                .HasOne<User>().WithMany()
            .HasForeignKey(e => e.OrganiserID)

                .OnDelete(DeleteBehavior.NoAction);



            // Events -> Categories
            modelBuilder.Entity<Event>()
                .HasOne<Category>().WithMany()

                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.NoAction);

            // Categories -> Events
            modelBuilder.Entity<Category>()
                .HasOne<Event>().WithMany()
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.NoAction);



            // Enrollments -> Users (participant) and Categories
            modelBuilder.Entity<Enrollment>()
                .HasOne<User>().WithMany()
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Enrollment>()
                .HasOne<Category>().WithMany()

             .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.NoAction);



     // A participant can only enrol once per category 
            modelBuilder.Entity<Enrollment>()
             .HasIndex(e => new { e.ParticipantID, e.CategoryID })

                .IsUnique();

            // Results -> Enrollments (one result per enrollment)
          modelBuilder.Entity<ResultEntity>()
                .HasOne<Enrollment>().WithMany()
              .HasForeignKey(r => r.EnrollmentID)

                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ResultEntity>()
                .HasIndex(r => r.EnrollmentID)
                .IsUnique();

       // Payments -> Users and Events
            modelBuilder.Entity<Payment>()
                .HasOne<User>().WithMany()

          .HasForeignKey(p => p.ParticipantID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()

                    .HasOne<Event>().WithMany()
                .HasForeignKey(p => p.EventID)

                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}