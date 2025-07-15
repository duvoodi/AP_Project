/*
برای وارد کردن داده های ساختگی اگر داده داریم اول باید تمام داده هامون رو پاک کنیم
اگر برای اولین بار اجرا میکنیم و داد نداریم مشکلی ندارد
اس کیو ال سرور منیجر استودیو رو باز میکنیم و به دیتا بیس که ادرسش در اپ ستینگ دات جیسون هست وصل میشیم
دیتا بیسمون رو در پوشه دیتابیسز بعد دیتابیس اسنپ شاتز یکبار روش کلیک کنیم تا انتخاب بشه بعد نیو کوئری بزنیم و کوئری زیر رو پیست و اجرا کنیم
DELETE FROM Takes;
DELETE FROM Teaches;
DELETE FROM Prerequisites;
DELETE FROM Sections;
DELETE FROM Courses;
DELETE FROM CourseCodes;
DELETE FROM Classrooms;
DELETE FROM Admins;
DELETE FROM Students;
DELETE FROM Instructors;
DELETE FROM UserRoles;
DELETE FROM Users;
*/

/*
سپس کد زیر را در پروگرم دات سی اس قبل اپ دات ران پیست و یکبار برنامه ران میکنیم و میبندیم
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedDataForFirstTime.Generate(
            context,
            adminCount : 1,
            instructorCount : 10,
            studentCount : 100,
            classroomCount : 20,
            courseCodeCount : 10,
            sectionCount : 30);
}
*/

/*
بهتره تعداد یکسان و سید یکسان وارد کنیم تا دیتابیسهامون دقیقا عین هم شود 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AP_Project.Models.Classrooms;
using AP_Project.Models.Courses;
using AP_Project.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace AP_Project.Data
{

    public static class SeedDataForFirstTime
    {
        private static DateTime GenerateRandomExamDate(Random rand)
        {
            // بازه تاریخ امتحان برای نیم‌سال 14041
            DateTime examStart1 = new DateTime(2026, 1, 21);
            DateTime examEnd1 = new DateTime(2026, 2, 19);
            
            // بازه تاریخ امتحان برای نیم‌سال 14042
            DateTime examStart2 = new DateTime(2026, 6, 5);
            DateTime examEnd2 = new DateTime(2026, 7, 6);

            // انتخاب تصادفی یکی از دو نیم‌سال
            if (rand.Next(2) == 0)
            {
                int examDaysRange1 = (examEnd1 - examStart1).Days;
                return examStart1.AddDays(rand.Next(examDaysRange1));
            }
            else
            {
                int examDaysRange2 = (examEnd2 - examStart2).Days;
                return examStart2.AddDays(rand.Next(examDaysRange2));
            }
        }

        public static void Generate(
            AppDbContext context,
            int adminCount = 1,
            int instructorCount = 10,
            int studentCount = 100,
            int classroomCount = 20,
            int courseCodeCount = 10,
            int sectionCount = 30)
        {
            // اگر یوزر در جدول هست کاربر متوقف میشود
            // فقط در صورتی که جدول را با کد اول فایل خالی کرده باشیم داده ساختگی اضافه میشود
            if (context.Users.Any())
                return;

            var rand = new Random();

            // 1. Compute default password hash for "1234"
            var defaultHash = ComputeHash.Sha1("1234");

            // 2. Assume Roles seeded via first migration from AppDbContext.cs

            // 3. Create Users: Admins, Instructors, Students
            var usedAdminIds = new HashSet<int>();
            var admins = new List<Admin>();
            for (int i = 1; i <= adminCount; i++)
            {
                int adminId;
                do
                {
                    int prefix = 100;
                    int mid = rand.Next(5, 10);
                    int suffix = rand.Next(0, 100_000);
                    adminId = int.Parse($"{prefix:D3}{mid}{suffix:D5}");
                } while (!usedAdminIds.Add(adminId));

                admins.Add(new Admin
                {
                    AdminId = adminId,
                    FirstName = $"ادمین ساختگی",
                    LastName = $"شماره {NumberToPersianWords.ConvertToWords(i)}",
                    Email = $"admin{i}@school.local",
                    HashedPassword = defaultHash,
                    CreatedAt = DateTime.UtcNow
                });
            }
            var instructors = new List<Instructor>();
            var usedInstructorIds = new HashSet<int>();
            for (int i = 1; i <= instructorCount; i++)
            {
                int instructorId;
                do
                {
                    int prefix = rand.Next(385, 400); // 1385 1تا 399
                    int mid = rand.Next(5, 10);
                    int suffix = rand.Next(0, 100_000);
                    instructorId = int.Parse($"{prefix:D3}{mid}{suffix:D5}");
                } while (!usedInstructorIds.Add(instructorId)); // فقط اگر اضافه شد، یعنی یکتا بود

                int hireYear = int.Parse($"1{instructorId.ToString().Substring(0, 3)}");

                instructors.Add(new Instructor
                {
                    InstructorId = instructorId,
                    FirstName = $"استاد ساختگی",
                    LastName = $"شماره {NumberToPersianWords.ConvertToWords(i)}",
                    Email = $"instr{i}@school.com",
                    HashedPassword = defaultHash,
                    CreatedAt = DateTime.UtcNow,
                    Salary = rand.Next(30000000, 80000000),
                    HireYear = hireYear
                });
            }
            var students = new List<Student>();
            var usedStudentIds = new HashSet<int>();
            for (int i = 1; i <= studentCount; i++)
            {
                int studentId;
                do
                {
                    int prefix = rand.Next(399, 404); // 1399 1تا 403
                    int mid = rand.Next(0, 5);
                    int suffix = rand.Next(0, 100_000);
                    studentId = int.Parse($"{prefix:D3}{mid}{suffix:D5}");
                } while (!usedStudentIds.Add(studentId));

                int enrollmentYear = int.Parse($"1{studentId.ToString().Substring(0, 3)}");

                students.Add(new Student
                {
                    StudentId = studentId,
                    FirstName = $"دانشجوی ساختگی",
                    LastName = $"شماره {NumberToPersianWords.ConvertToWords(i)}",
                    Email = $"student{i}@school.com",
                    HashedPassword = defaultHash,
                    CreatedAt = DateTime.UtcNow,
                    EnrollmentYear = enrollmentYear
                });
            }


            context.Users.AddRange(admins);
            context.Users.AddRange(instructors);
            context.Users.AddRange(students);
            context.SaveChanges();

            // 4. Assign UserRoles
            var userRoles = new List<UserRole>();
            userRoles.AddRange(admins.Select(a => new UserRole { UserId = a.Id, RoleId = 1 }));
            userRoles.AddRange(instructors.Select(i => new UserRole { UserId = i.Id, RoleId = 2 }));
            userRoles.AddRange(students.Select(s => new UserRole { UserId = s.Id, RoleId = 3 }));
            context.UserRoles.AddRange(userRoles);

            // 5. Classrooms
            var classrooms = Enumerable.Range(1, classroomCount).Select(i => new Classroom
            {
                Building = $"کلاس {i}",
                RoomNumber = rand.Next(100, 500),
                Capacity = rand.Next(20, 100)
            }).ToList();
            context.Classrooms.AddRange(classrooms);

            // 6. Assume TimeSlots seeded via first migration from AppDbContext.cs

            // 7. CourseCodes
            var codes = Enumerable.Range(1, courseCodeCount).Select(i => new CourseCode
            {
                Code = 800 + i,
                Title = $"درس {800 + i}"
            }).ToList();
            context.CourseCodes.AddRange(codes);
            context.SaveChanges();

            // 8. Courses
            var courses = codes.Select(cc => new Course
            {
                CodeId = cc.Id,
                Unit = rand.Next(1, 4),
                Description = $"توضیحات {cc.Title}",
                FinalExamDate = GenerateRandomExamDate(rand) // تابع جدید برای تولید تاریخ تصادفی
            }).ToList();

            context.Courses.AddRange(courses);
            context.SaveChanges();

            // 9. Prerequisites (random subset) — اصلاح شده برای خطی بودن و عدم حلقه
            // این کد مطمئن می‌شود پیش‌نیازهای هر درس فقط از دروس با شماره کوچکتر باشند و بنابراین چرخه پیش‌نیاز ایجاد نمی‌شود.
            var prereqs = new List<Prerequisite>();
            double prerequisiteProbability = 0.6; // حدود 60 درصد درس‌ها پیش‌نیاز دارند
            int maxPrereqsPerCourse = 3;

            for (int i = 0; i < courses.Count; i++)
            {
                var course = courses[i];
                if (i == 0)
                {
                    // اولین درس نمی‌تواند پیش‌نیاز داشته باشد چون هیچ درسی قبلش نیست
                    continue;
                }

                if (rand.NextDouble() < prerequisiteProbability)
                {
                    // تعداد پیش‌نیاز بین 1 تا maxPrereqsPerCourse (مثلاً 3)
                    int prereqCount = rand.Next(1, Math.Min(maxPrereqsPerCourse, i) + 1);

                    // لیست ایندکس‌های دروس قبلی
                    var possiblePrereqsIndexes = Enumerable.Range(0, i).ToList();

                    // انتخاب رندوم پیش‌نیازها بدون تکرار
                    for (int p = 0; p < prereqCount; p++)
                    {
                        if (possiblePrereqsIndexes.Count == 0)
                            break;

                        int selectedIndexInList = rand.Next(possiblePrereqsIndexes.Count);
                        int prereqIndex = possiblePrereqsIndexes[selectedIndexInList];
                        possiblePrereqsIndexes.RemoveAt(selectedIndexInList);

                        prereqs.Add(new Prerequisite
                        {
                            CourseId = course.Id,
                            PrerequisiteCourseCodeId = courses[prereqIndex].CodeId
                        });
                    }
                }
            }
            context.Prerequisites.AddRange(prereqs);


            // 10. Sections
            var allSlots = context.TimeSlots.ToList();
            var sections = new List<Section>();

            // تابع کمکی برای تعیین نیم‌سال از تاریخ امتحان
            int GetSemesterFromDate(DateTime date)
            {
                if (date >= new DateTime(2026, 1, 21) && date <= new DateTime(2026, 2, 19))
                    return 1;
                else if (date >= new DateTime(2026, 6, 5) && date <= new DateTime(2026, 7, 6))
                    return 2;
                else
                    return 0; // تاریخ نامعتبر
            }

            // ابتدا برای هر درس یک سکشن بساز (حداقل یکی برای هر درس)
            foreach (var course in courses)
            {
                int semester = GetSemesterFromDate(course.FinalExamDate);
                if (semester == 0) continue; // از کورس‌هایی که تاریخ امتحان معتبر ندارند صرف‌نظر کن

                sections.Add(new Section
                {
                    CourseId = course.Id,
                    Year = 1404,
                    Semester = semester,
                    ClassroomId = classrooms[rand.Next(classrooms.Count)].Id,
                    TimeSlotId = allSlots[rand.Next(allSlots.Count)].Id
                });
            }

            // سپس با حلقه while ادامه بده تا به تعداد کلی sectionCount برسیم
            while (sections.Count < sectionCount)
            {
                var course = courses[rand.Next(courses.Count)];
                int semester = GetSemesterFromDate(course.FinalExamDate);
                if (semester == 0) continue;

                sections.Add(new Section
                {
                    CourseId = course.Id,
                    Year = 1404,
                    Semester = semester,
                    ClassroomId = classrooms[rand.Next(classrooms.Count)].Id,
                    TimeSlotId = allSlots[rand.Next(allSlots.Count)].Id
                });
            }

            // جلوگیری از سکشن‌های تکراری با کلاس و زمان تکراری در همان ترم و سال
            sections = sections
                .GroupBy(s => new { s.Year, s.Semester, s.ClassroomId, s.TimeSlotId })
                .Select(g => g.First())
                .ToList();

            context.Sections.AddRange(sections);
            context.SaveChanges();

            // 11. Teaches
            var teaches = sections.Select(s => new Teaches
            {
                SectionId = s.Id,
                InstructorUserId = instructors[rand.Next(instructors.Count)].Id
            }).ToList();
            context.Teaches.AddRange(teaches);

            // 12. Takes
            var classroomCaps = context.Classrooms
                           .Select(c => new { c.Id, c.Capacity })
                           .ToDictionary(x => x.Id, x => x.Capacity);
            var takes = new List<Takes>();
            foreach (var sec in sections)
            {
                int cap = classroomCaps[sec.ClassroomId ?? Guid.Empty];
                int maxEnroll = Math.Min(cap, students.Count);
                int enroll = rand.Next(5, maxEnroll + 1);
                var chosen = students.OrderBy(_ => rand.Next()).Take(enroll);
                foreach (var stu in chosen)
                {
                    takes.Add(new Takes
                    {
                        SectionId = sec.Id,
                        StudentUserId = stu.Id,
                        Grade = $"{rand.Next(0, 19)}.{rand.Next(0, 99)}"
                    });
                }
            }
            context.Takes.AddRange(takes);
            context.SaveChanges();
        }
    }
}

public static class NumberToPersianWords
{
    private static readonly string[] Units = 
    { 
        "", "یک", "دو", "سه", "چهار", "پنج", "شش", "هفت", "هشت", "نه", 
        "ده", "یازده", "دوازده", "سیزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده" 
    };

    private static readonly string[] Tens = 
    {
        "", "", "بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود"
    };

    private static readonly string[] Hundreds = 
    {
        "", "صد", "دویست", "سیصد", "چهارصد", "پانصد", "ششصد", "هفتصد", "هشتصد", "نهصد"
    };

    private static string ConvertThreeDigits(int number)
    {
        string result = "";

        int hundred = number / 100;
        int remainder = number % 100;

        if (hundred > 0)
        {
            result += Hundreds[hundred];
        }

        if (remainder > 0)
        {
            if (hundred > 0)
                result += " و ";

            if (remainder < 20)
            {
                result += Units[remainder];
            }
            else
            {
                int ten = remainder / 10;
                int unit = remainder % 10;

                result += Tens[ten];

                if (unit > 0)
                    result += " و " + Units[unit];
            }
        }

        return result;
    }

    public static string ConvertToWords(int number)
    {
        if (number == 0)
            return "صفر";

        if (number < 0)
            return "منفی " + ConvertToWords(Math.Abs(number));

        string[] sections = { "", "هزار", "میلیون", "میلیارد" };
        int sectionCount = 0;
        string words = "";

        while (number > 0)
        {
            int sectionNumber = number % 1000;

            if (sectionNumber != 0)
            {
                string sectionWords = ConvertThreeDigits(sectionNumber);
                if (sectionCount > 0)
                    sectionWords += " " + sections[sectionCount];

                if (words != "")
                    words = sectionWords + " و " + words;
                else
                    words = sectionWords;
            }

            number /= 1000;
            sectionCount++;
        }

        return words;
    }
}