using System;
using System.Linq;
using AP_Project.Models.Users;

namespace AP_Project.Data
{
    public static class UniqueIdSeeder
    {
        private static readonly Random rnd = new Random();

        public static string GenerateUniqueId(DateTime dt)
        {
            int R1 = rnd.Next(0, 10);
            int R2 = rnd.Next(0, 10);

            string s = dt.Second.ToString("D2");
            string D = dt.Day.ToString("D2");
            string M = dt.Month.ToString("D2");
            string Y = dt.Year.ToString("D4");
            string h = dt.Hour.ToString("D2");
            string m = dt.Minute.ToString("D2");

            return $"{R1}{s[1]}{s[0]}{D[1]}{M[1]}{Y[3]}{D[0]}{Y[2]}{h[0]}{Y[1]}{M[0]}{m[1]}{Y[0]}{R2}{h[1]}{m[0]}";
        }

        public static string GetRoleId(string roleName)
        {
            return roleName.ToLower() switch
            {
                "admin" => "1",
                "instructor" => "2",
                "student" => "3",
                _ => throw new ArgumentException("Invalid role name")
            };
        }

        public static string GenerateInstructorCode(int year)
        {
            return GeneratePersonCode(year, isStudent: false, rangeStart: 385, rangeEnd: 399);
        }

        public static string GenerateStudentCode(int year)
        {
            return GeneratePersonCode(year, isStudent: true, rangeStart: 399, rangeEnd: 403);
        }

        private static string GeneratePersonCode(int year, bool isStudent, int rangeStart, int rangeEnd)
        {
            int prefix = year % 1000;
            prefix = Math.Clamp(prefix, rangeStart, rangeEnd);

            int mid = isStudent
                ? rnd.Next(0, 5)
                : rnd.Next(5, 10);

            int suffix = rnd.Next(0, 100_000);

            return $"{prefix:D3}{mid}{suffix:D5}";
        }

        public static string GenerateTimeSlotId(int dayOfWeek, int slotNumber)
        {
            return (dayOfWeek * 10 + slotNumber).ToString();
        }

        public static void SeedIds(AppDbContext context)
        {
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Id = "1", Name = "Admin" },
                    new Role { Id = "2", Name = "Instructor" },
                    new Role { Id = "3", Name = "Student" }
                );

                context.SaveChanges();
            }
        }
    }
}
