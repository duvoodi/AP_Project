using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AP_Project.Data;
using AP_Project.Models.Users;
using AP_Project.Models.Courses;

namespace AP_Project.Controllers
{
    public class StudentCoursesListController : BaseStudentController
    {
        public StudentCoursesListController(AppDbContext db) : base(db)
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

            // گرفتن همه Takes دانشجو به همراه اطلاعات کامل
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

            // پیدا کردن جدیدترین ترم از بین Takes
            var latestTerm = allTakes
                .Where(t => t.Section != null)
                .OrderByDescending(t => t.Section.Year)
                .ThenByDescending(t => t.Section.Semester)
                .FirstOrDefault()?.Section;

            if (latestTerm == null)
            {
                ViewBag.Takes = new List<Takes>();
                return View("~/Views/StudentDashboard/CoursesList.cshtml", student);
            }

            // فقط دروس آخرین ترم
            var currentTermTakes = allTakes
                .Where(t => t.Section.Year == latestTerm.Year && t.Section.Semester == latestTerm.Semester)
                .OrderBy(t => t.Section.Course.CourseCode.Title)
                .ThenBy(t => t.Section.TimeSlot.Day)
                .ThenBy(t => t.Section.TimeSlot.StartTime)
                .ToList();

            ViewBag.Takes = currentTermTakes;

            return View("~/Views/StudentDashboard/CoursesList.cshtml", student);
        }


        [HttpPost]
        public async Task<IActionResult> DropCourse([FromBody] DropCourseRequest request)
        {
            var studentId = CurrentStudent.Id;

            var takes = await _db.Takes
                .Where(t => t.Student.Id == studentId && t.SectionId == request.SectionId)
                .FirstOrDefaultAsync();

            if (takes == null)
            {
                return Json(new { success = false, message = "درس پیدا نشد یا شما به آن دسترسی ندارید." });
            }

            _db.Takes.Remove(takes);

            try
            {
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "خطا در حذف درس." });
            }
        }

        public class DropCourseRequest
        {
            public Guid SectionId { get; set; }
        }


    }
}
