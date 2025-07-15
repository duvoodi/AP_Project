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

            var courses = _db.Courses
                .Include(c => c.CourseCode)
                .Include(c => c.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteCourseCode)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Teaches)
                        .ThenInclude(t => t.Instructor)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.TimeSlot) // اطمینان از لود TimeSlot
                .OrderBy(i => i.CourseCode.Title)
                .ThenBy(i => i.CourseCode.Code)
                .AsNoTracking() // برای بهبود عملکرد
                .ToList();

            ViewBag.Courses = courses;
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