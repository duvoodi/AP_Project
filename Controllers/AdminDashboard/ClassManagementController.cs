using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using AP_Project.Helpers.FormUtils;
using Microsoft.EntityFrameworkCore;
using AP_Project.FormViewModels.CourseForm;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;
using AP_Project.Helpers;
using AP_Project.FormViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using AP_Project.Models.Courses;
using AP_Project.Models.Classrooms;
using System.Globalization;

namespace AP_Project.Controllers
{
    public class ClassManagementController : BaseAdminController
    {
        public ClassManagementController(AppDbContext db) : base(db)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            var admin = CurrentAdmin;

            // بارگذاری سکشن‌هایی که به یک کلس روم تخصیص یافته‌اند
            var sections = _db.Sections
                .AsNoTracking()
                .Where(s => s.Classroom != null)
                .Include(s => s.Classroom)
                .Include(s => s.Course)
                    .ThenInclude(c => c.CourseCode)
                .Include(s => s.Teaches)
                    .ThenInclude(t => t.Instructor)
                .Include(s => s.Takes)
                    .ThenInclude(tk => tk.Student)
                .Include(s => s.TimeSlot)
                .OrderBy(s => s.Classroom.Building)                                   // 1. ساختمان
                .ThenBy(s => s.Classroom.RoomNumber)                                  // 2. شماره کلاس
                .ThenBy(s => s.Classroom.Capacity)                                    // 3. ظرفیت
                .ThenBy(s => s.Course.CourseCode.Title)                               // 4. عنوان درس
                .ThenBy(s => s.Teaches != null && s.Teaches.Instructor != null        // 5. نام استاد
                    ? s.Teaches.Instructor.FirstName
                    : string.Empty)
                .ThenBy(s => s.Teaches != null && s.Teaches.Instructor != null        // 6. نام خانوادگی استاد
                    ? s.Teaches.Instructor.LastName
                    : string.Empty)
                .ToList();

            ViewBag.Sections = sections;
            return View("~/Views/AdminDashboard/ClassManagement/Index.cshtml", admin);
        }
    }
}

