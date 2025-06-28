using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using Microsoft.AspNetCore.Http;

namespace AP_Project.Controllers
{
    public class StudentDashboardController : Controller
    {
        private readonly AppDbContext _db;
        public StudentDashboardController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Index()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Index", "Login");

            var student = _db.Set<Student>().FirstOrDefault(a => a.StudentId == studentId.Value);
            if (student == null)
                return RedirectToAction("Index", "Login");

            return View("Index", student);
        }
    }
}