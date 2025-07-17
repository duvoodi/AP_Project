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
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var instructor = CurrentInstructor;

            var classrooms = _db.Classrooms
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Course)
                        .ThenInclude(c => c.CourseCode)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.TimeSlot)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Teaches)
                .ToList();

            ViewBag.Classrooms = classrooms;
            return View("~/Views/InstructorDashboard/ClassManagement/Index.cshtml", instructor);
        }

    }
}