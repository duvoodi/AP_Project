using AP_Project.Models.Courses;
using AP_Project.Models.Classrooms;
using AP_Project.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace AP_Project.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet ها
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Admin> Admins { get; set; }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Prerequisite> Prerequisites { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Takes> Takes { get; set; }
        public DbSet<Teaches> Teaches { get; set; }

        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // پیکربندی کلیدهای مرکب برای جدول‌های واسط

            // UserRole: کلید مرکب UserId + RoleId
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId);

            // Prerequisite: کلید مرکب CourseId + PrerequisiteCourseId
            modelBuilder.Entity<Prerequisite>()
                .HasKey(p => new { p.CourseId, p.PrerequisiteCourseId });

            modelBuilder.Entity<Prerequisite>()
                .HasOne(p => p.Course)
                .WithMany(c => c.Prerequisites)
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prerequisite>()
                .HasOne(p => p.PrerequisiteCourse)
                .WithMany()
                .HasForeignKey(p => p.PrerequisiteCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Takes: کلید مرکب StudentId + SectionId
            modelBuilder.Entity<Takes>()
                .HasKey(t => new { t.StudentId, t.SectionId });

            modelBuilder.Entity<Takes>()
                .HasOne(t => t.Student)
                .WithMany(s => s.Takes)
                .HasForeignKey(t => t.StudentId);

            modelBuilder.Entity<Takes>()
                .HasOne(t => t.Section)
                .WithMany(s => s.Takes)
                .HasForeignKey(t => t.SectionId);

            // Teaches: کلید مرکب InstructorId + SectionId
            modelBuilder.Entity<Teaches>()
                .HasKey(t => new { t.InstructorId, t.SectionId });

            modelBuilder.Entity<Teaches>()
                .HasOne(t => t.Instructor)
                .WithMany(i => i.Teaches)
                .HasForeignKey(t => t.InstructorId);

            modelBuilder.Entity<Teaches>()
                .HasOne(t => t.Section)
                .WithMany(s => s.Teaches)
                .HasForeignKey(t => t.SectionId);

            // پیکربندی روابط کلاس‌های اصلی

            // Section و Course
            modelBuilder.Entity<Section>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.CourseId);

            // Section و Classroom
            modelBuilder.Entity<Section>()
                .HasOne(s => s.Classroom)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.ClassroomId);

            // Section و TimeSlot
            modelBuilder.Entity<Section>()
                .HasOne(s => s.TimeSlot)
                .WithMany(t => t.Sections)
                .HasForeignKey(s => s.TimeSlotId);

            // نوع Day و TimeSpan در TimeSlot را به صورت عادی نگه دار (EF Core به طور پیش‌فرض پشتیبانی میکند)
            // در صورت نیاز می‌توان اینجا مبدل تعریف کرد، ولی الان نیاز نیست.

            // اگر لازم بود داده اولیه TimeSlot را اینجا بذاری
            var days = new[] { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday" };
            var timeRanges = new[]
            {
                (Start: new TimeSpan(9, 0, 0), End: new TimeSpan(10, 30, 0)),
                (Start: new TimeSpan(10, 30, 0), End: new TimeSpan(12, 0, 0)),
                (Start: new TimeSpan(14, 30, 0), End: new TimeSpan(16, 0, 0)),
                (Start: new TimeSpan(16, 0, 0), End: new TimeSpan(17, 30, 0)),
            };

            var timeSlots = new List<TimeSlot>();
            int dayIndex = 1;

            foreach (var day in days)
            {
                for (int i = 0; i < timeRanges.Length; i++)
                {
                    timeSlots.Add(new TimeSlot
                    {
                        Id = dayIndex * 10 + (i + 1).ToString(),
                        Day = day,
                        StartTime = timeRanges[i].Start,
                        EndTime = timeRanges[i].End
                    });
                }
                dayIndex++;
            }

            modelBuilder.Entity<TimeSlot>().HasData(timeSlots);
        }
    }
}
