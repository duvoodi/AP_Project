using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using Microsoft.AspNetCore.Http;

namespace AP_Project.Controllers
{
    public class StudentCoursesListController : Controller
    {
        private readonly AppDbContext _db;
        public StudentCoursesListController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Index()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Index", "Login");

            var student = _db.Set<Student>().FirstOrDefault(a => a.StudentId == studentId.Value);
            if (student == null)
                return RedirectToAction("Index", "Login");

            ViewData["ActiveTab"] = "CoursesList";
            return View("~/Views/StudentDashboard/CoursesList.cshtml", student);
        }
    }
}