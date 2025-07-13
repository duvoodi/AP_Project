using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using System.Globalization;
using AP_Project.Helpers.FormUtils;
using Microsoft.EntityFrameworkCore;
using AP_Project.FormViewModels.InstructorForm;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AP_Project.Controllers
{
    public class StudentManagementController : BaseAdminController
    {
        public StudentManagementController(AppDbContext db) : base(db)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            var students = _db.Students
                .OrderBy(i => i.LastName)
                .ThenBy(i => i.FirstName)
                .ToList();

            ViewBag.Students = students;
            return View("~/Views/AdminDashboard/StudentManagement/Index.cshtml", admin);
        }
    }
}
