using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using Microsoft.AspNetCore.Http;
using AP_Project.Helpers.FormUtils;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;


namespace AP_Project.Controllers
{
    public class CourseManagementController : BaseAdminController
    {
        public CourseManagementController(AppDbContext db) : base(db)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            var admin = CurrentAdmin;

            var sections = _db.Sections
                .Include(s => s.Course)
                    .ThenInclude(c => c.CourseCode)
                .Include(s => s.Teaches)
                    .ThenInclude(t => t.Instructor)
                .Include(s => s.TimeSlot)
                .Include(s => s.Course.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteCourseCode)
                .ToList();

            bool anyWithoutInstructor = sections.Any(s => s.Teaches == null || s.Teaches.Instructor == null);
            ViewBag.AnyWithoutInstructor = anyWithoutInstructor;
            ViewBag.Sections = sections;

            return View("~/Views/AdminDashboard/CourseManagement/Index.cshtml", admin);
        }

        [HttpGet]
        public IActionResult AddCourseIndex()
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            var pc = new PersianCalendar();
            int currentPersianYear = pc.GetYear(DateTime.Now);
            ViewData["currentPersianYear"] = currentPersianYear;
            return View("~/Views/AdminDashboard/CourseManagement/AddCourse.cshtml", admin);
        }
    }
}