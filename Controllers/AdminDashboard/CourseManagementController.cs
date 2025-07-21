using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using System.Globalization;
using AP_Project.Helpers.FormUtils;
using Microsoft.EntityFrameworkCore;
using AP_Project.FormViewModels.CourseForm;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AP_Project.Models.Courses;
using System.Linq;
using AP_Project.Helpers;
using AP_Project.FormViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

            // بارگذاری سکشن‌هایی که کورس دارند
            var sections = _db.Sections
                .Where(s => s.Course != null)
                .Include(s => s.Course)
                    .ThenInclude(c => c.CourseCode)
                .Include(s => s.Teaches)
                    .ThenInclude(t => t.Instructor)
                .Include(s => s.TimeSlot)
                .Include(s => s.Course.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteCourseCode)
                .OrderBy(s => s.Course.CourseCode.Code)                            // 1. کد درس
                .ThenBy(s => s.Course.Unit)                                        // 2. تعداد واحد
                .ThenBy(s => ((s.Year ?? 0) * 10) + (s.Semester ?? 0))             // 3. نیم‌سال به صورت Year*10 + Semester
                .ThenBy(s =>                                                       // 4. اسم استاد یا رشته خالی
                    s.Teaches != null && s.Teaches.Instructor != null
                    ? s.Teaches.Instructor.FirstName
                    : string.Empty
                )
                .ThenBy(s =>                                                       // 5. فامیلی استاد یا رشته خالی
                    s.Teaches != null && s.Teaches.Instructor != null
                    ? s.Teaches.Instructor.LastName
                    : string.Empty
                )
                .ThenBy(s => s.TimeSlotId ?? 0)                                    // 6. شناسه‌ی تایم‌اسلات
                .ToList();

            ViewBag.Sections = sections;
            return View("~/Views/AdminDashboard/CourseManagement/Index.cshtml", admin);
        }

    }
}