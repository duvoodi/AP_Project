using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using Microsoft.AspNetCore.Http;
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
        public IActionResult Index()
        {
            var student = CurrentStudent;

            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // دریافت همه سکشن‌هایی که دانشجو شرکت کرده
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

            // گرفتن جدیدترین ترم
            var latestTerm = allTakes
                .Where(t => t.Section != null)
                .OrderByDescending(t => t.Section.Year)
                .ThenByDescending(t => t.Section.Semester)
                .FirstOrDefault()?.Section;

            if (latestTerm == null)
            {
                ViewBag.Takes = new List<Takes>();
                return View("~/Views/StudentDashboard/Transcripts.cshtml", student);
            }

            // فقط سکشن‌های آخرین ترم رو نگه داریم
            var currentTermTakes = allTakes
                .Where(t => t.Section.Year == latestTerm.Year && t.Section.Semester == latestTerm.Semester)
                .OrderBy(t => t.Section.Course.CourseCode.Title)
                .ToList();

            ViewBag.Takes = currentTermTakes;
            return View("~/Views/StudentDashboard/Transcripts.cshtml", student);
        }

    }
}