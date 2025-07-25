﻿/* راه اندازی دیتابیس
dotnet ef migrations add InitialCreate
dotnet ef database update
*/

/* ثبت تغییرات انجام شده در مدل ها و دی بی کانتکست
dotnet ef migrations add <ModificationName>
dotnet ef database update
*/

/* آندو کردن آخرین میجریشن و تغییرات انجام شده روی دیتابیس
ef migrations remove
*/

/* راه اندازی مجدد دیتابیس
dotnet ef database drop --force
اگر با ورژن های قبلی دیتابیس کاری نداریم میجریشن هاشون پاک و دوباره
dotnet ef migrations add InitialCreate
dotnet ef database update
میکنیم
اگر میخوایم ورژن ها قبلی رو نگه داریم باید با نام جدید راه اندازی کنیم
dotnet ef migrations add InitialCreate2
dotnet ef database update
*/

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
        public DbSet<CourseCode> CourseCodes { get; set; }
        public DbSet<Prerequisite> Prerequisites { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Takes> Takes { get; set; }
        public DbSet<Teaches> Teaches { get; set; }

        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }

        // تنظیمات اولیه که فقط یکبار با اولین میجریشن اجرا میشوند
        // اولین میجیریشن زمانی هست که دیتابیس نداریم هنوز یا داشتیم دراپ کردیم و الان نداریم
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // پیکربندی کلیدهای مرکب برای جدول‌های واسط

            // UserRole: کلید مرکب UserId + RoleId
            modelBuilder.Entity<UserRole>(entity => // چون سه تا تنظیمات داریم بلاک کد میزنیم
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.User) // User و UserRole
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // اگر کاربر حذف شد، یوزر رولش هم حذف شوند

                entity.HasOne(ur => ur.Role) // Role و UserRole
                    .WithMany()
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Restrict); // یوزر رول اگر یوزرش هنوز هست نباید حذف بشود تا داده‌ها ناقص نشوند
            });

            // Prerequisite: کلید مرکب CourseId + PrerequisiteCourseCodeId
            modelBuilder.Entity<Prerequisite>(entity => // چون سه تا تنظیمات داریم بلاک کد میزنیم
            {
                entity.HasKey(p => new { p.CourseId, p.PrerequisiteCourseCodeId });

                entity.HasOne(p => p.Course) // Course و Prerequisite
                    .WithMany(c => c.Prerequisites)
                    .HasForeignKey(p => p.CourseId)
                    .OnDelete(DeleteBehavior.Cascade); // اگر درسی که پیش‌نیاز دارد حذف شد، رابطه‌ی پیش‌نیاز هم حذف شود

                entity.HasOne(p => p.PrerequisiteCourseCode) // Prerequisite و CourseCode
                    .WithMany()
                    .HasForeignKey(p => p.PrerequisiteCourseCodeId)
                    .OnDelete(DeleteBehavior.Restrict); // اگر کد درسی پیش‌نیاز درس‌های دیگر است، نباید بشود آن را حذف کرد تا داده‌ها ناقص نشوند
            });

            // Takes: کلید مرکب StudentId + SectionId
            modelBuilder.Entity<Takes>(entity => // چون سه تا تنظیمات داریم بلاک کد میزنیم
            {
                entity.HasKey(t => new { t.StudentUserId, t.SectionId }); 

                entity.HasOne(t => t.Student) // Student و Takes
                    .WithMany(s => s.Takes)
                    .HasForeignKey(t => t.StudentUserId)
                    .OnDelete(DeleteBehavior.Cascade);  // اگر دانشجو حذف شد، ثبت‌نام‌های او هم حذف شوند

                entity.HasOne(t => t.Section) // Section و Takes
                    .WithMany(s => s.Takes)
                    .HasForeignKey(t => t.SectionId)
                    .OnDelete(DeleteBehavior.Cascade); // اگر سکشن حذف شد، ثبت‌نام‌های مربوط به آن هم حذف شوند

            });

            // Teaches: کلید مرکب InstructorId + SectionId
            modelBuilder.Entity<Teaches>(entity => // چون سه تا تنظیمات داریم بلاک کد میزنیم
            {
                entity.HasKey(t => t.SectionId); // تا هر تیچز فقط برای یک سکشن بتواند باشد

                entity.HasOne(t => t.Instructor) // Instructor و Teaches
                    .WithMany(i => i.Teaches)
                    .HasForeignKey(t => t.InstructorUserId)
                    .OnDelete(DeleteBehavior.Restrict); // استاد اگر به تیچزی وصل هست هست نباید حذف بشود تا داده‌ها ناقص نشوند

                entity.HasOne(t => t.Section) // Section و Teaches
                    .WithOne(s => s.Teaches)
                    .HasForeignKey<Teaches>(t => t.SectionId)
                    .OnDelete(DeleteBehavior.Cascade); // اگر سکشن حذف شد، تخصیص استادهای مربوط به آن هم حذف شوند
            });

            // پیکربندی روابط کلاس‌های اصلی

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .HasDefaultValueSql("NEWID()");

            // ایجاد جدول کلاس‌های مشتق شده از یوزرز چون به صورت پیشفرض به صورت ادغام شده با هم نمایش داده میشوند
            modelBuilder.Entity<Student>().ToTable("Students");
            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<Instructor>().ToTable("Instructors");
            // دات تو تیبل جدول های جدا برای اینها از جدول مدل والدشان میزند ولی وان تو وان نمیگذارد بینشان تعریف کنیم
            // و دلیت بیهیویر بین اینها و مدل والدشان یعنی یوزر را باید دستی هندل کنیم

            modelBuilder.Entity<Instructor>().Property(i => i.Salary) // تنظیم دسیمال که داده بیشتر نگه دارد
                .HasPrecision(9, 0); // تا نه رقم در کل و صفز رقم اعشار 

            // Course و CourseCode
            modelBuilder.Entity<CourseCode>()
                .HasKey(cc => cc.Id);

            modelBuilder.Entity<Course>(entity => // چون دو تا تنظیمات داریم باید بلاک کد بزنیم
            {
                entity.Property(c => c.Id)
                    .HasDefaultValueSql("NEWID()");

                entity.HasOne(c => c.CourseCode) // CourseCode و Course
                    .WithMany(cc => cc.Courses)
                    .HasForeignKey(c => c.CodeId)
                    .OnDelete(DeleteBehavior.Restrict); // اگر کد درسی در حال استفاده در درسی باشد نباید حذف شود  تا داده ها ناقص نشوند
            });

            modelBuilder.Entity<Section>(entity => // چون چهار تا تنظیمات داریم بلاک کد میزنیم
            {
                entity.Property(s => s.Id)
                    .HasDefaultValueSql("NEWID()");

                entity.HasOne(s => s.Course) // Section و Course
                    .WithMany(c => c.Sections)
                    .HasForeignKey(s => s.CourseId)
                    .OnDelete(DeleteBehavior.Restrict); // اگر درس در حال استفاده در سکشنی باشد نباید حذف شود تا داده ها ناقص نشوند


                entity.HasOne(s => s.Classroom) // Section و Classroom
                    .WithMany(c => c.Sections)
                    .HasForeignKey(s => s.ClassroomId)
                    .OnDelete(DeleteBehavior.Restrict); // اگر کلاس در حال استفاده در سکشنی باشد نباید حذف شود تا داده ها ناقص نشوند

                entity.HasOne(s => s.TimeSlot) // Section و TimeSlot
                    .WithMany(t => t.Sections)
                    .HasForeignKey(s => s.TimeSlotId)
                    .OnDelete(DeleteBehavior.Restrict); // اگر تایم‌اسلات در حال استفاده در سکشنی باشد، نباید حذف شود تا داده ها ناقص نشوند
            });

            modelBuilder.Entity<Classroom>()
                .Property(c => c.Id)
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "admin" },
                new Role { Id = 2, Name = "instructor" },
                new Role { Id = 3, Name = "student" }
            );

            // ایجاد داده‌های اولیه برای تایم‌اسلات
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
                        Id = dayIndex * 100 + (i + 1),
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
