using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AP_Project.Controllers
{
    public class InstructorClassManagementController : BaseInstructorController
    {
        public InstructorClassManagementController(AppDbContext db) : base(db)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            var instructor = CurrentInstructor;

            var classrooms = _db.Classrooms
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Course)
                        .ThenInclude(c => c.CourseCode)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.TimeSlot)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Teaches)
                        .ThenInclude(t => t.Instructor)
                .ToList();

            ViewBag.Classrooms = classrooms;
            return View("~/Views/InstructorDashboard/ClassManagement/Index.cshtml", instructor);
        }
        [HttpGet]
        public async Task<IActionResult> GetCourseInfo(Guid courseId)
        {
            var course = await _db.Courses
                .Include(c => c.CourseCode)
                .Include(c => c.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteCourseCode)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Teaches)
                        .ThenInclude(t => t.Instructor)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.TimeSlot)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound();

            return PartialView("~/Views/InstructorDashboard/ClassManagement/CourseDetailsPopup.cshtml", course);
        }

    }
}