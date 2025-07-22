using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using Microsoft.EntityFrameworkCore;
using AP_Project.Models.Courses;

namespace AP_Project.Controllers
{
    public class TranscriptsController : BaseStudentController
    {
        public TranscriptsController(AppDbContext db) : base(db)
        {
        }

        [HttpGet]
        public IActionResult Index(int? year = null, int? semester = null, string h = "")
        {
            var student = CurrentStudent;

            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // همه Takes دانشجو رو بارگذاری کن با اطلاعات لازم
            var allTakes = _db.Takes
                .Include(t => t.Section)
                    .ThenInclude(s => s.Course)
                        .ThenInclude(c => c.CourseCode)
                .Include(t => t.Section)
                    .ThenInclude(s => s.TimeSlot)
                .Include(t => t.Section)
                    .ThenInclude(s => s.Classroom)
                .Include(t => t.Section)
                    .ThenInclude(s => s.Teaches)
                        .ThenInclude(teaches => teaches.Instructor)
                .Where(t => t.StudentUserId == student.Id)
                .ToList();

            Section? targetTerm = null;

            if (year.HasValue && semester.HasValue)
            {
                // ترم مشخص شده توسط کاربر
                targetTerm = allTakes
                    .Where(t => t.Section != null)
                    .Select(t => t.Section)
                    .FirstOrDefault(s => s.Year == year.Value && s.Semester == semester.Value);
            }

            if (targetTerm == null)
            {
                // اگر ترم خاصی داده نشده یا پیدا نشد، جدیدترین ترم را بگیر
                targetTerm = allTakes
                    .Where(t => t.Section != null)
                    .OrderByDescending(t => t.Section.Year)
                    .ThenByDescending(t => t.Section.Semester)
                    .Select(t => t.Section)
                    .FirstOrDefault();
            }

            if (targetTerm == null)
            {
                ViewBag.Takes = new List<Takes>();
                return View("~/Views/StudentDashboard/Transcripts.cshtml", student);
            }

            // فقط سکشن‌های ترم هدف را نگه دار
            var currentTermTakes = allTakes
                .Where(t => t.Section != null && t.Section.Year == targetTerm.Year && t.Section.Semester == targetTerm.Semester)
                .OrderBy(t => t.Section?.Course?.CourseCode.Title)
                .ToList();


            ViewBag.Takes = currentTermTakes;
            ViewBag.HasTerm = targetTerm != null;
            ViewBag.IsSpecificTerm = year.HasValue && semester.HasValue;
            ViewBag.CurrentHash = h;


            return View("~/Views/StudentDashboard/Transcripts.cshtml", student);
        }
    }
}
