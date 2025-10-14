using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Key> Keys { get; set; }
        public DbSet<RememberMe> RememberMe { get; set; }
        public DbSet<Subject> Subjects { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
            {
            }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Students)
                .WithMany(u => u.Courses)
                .UsingEntity(j => j.ToTable("CourseStudents"));
            modelBuilder.Entity<Subject>()
                .HasMany(c => c.Teachers)
                .WithMany(u => u.Teaches)
                .UsingEntity(j => j.ToTable("SubjectTeachers"));
        }
    }
}
