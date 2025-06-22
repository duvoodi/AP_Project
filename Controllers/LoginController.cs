using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;

namespace AP_Project.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _db;

        public LoginController(AppDbContext db)
            => _db = db;

        [HttpGet]
        public IActionResult Index()
        {
            if (TempData["LoginError"] != null)
            {
                ModelState.AddModelError("", TempData["LoginError"].ToString());
            }

            ViewData["HeaderPartial"] = "_LoginHeaderPartial";
            return View();
        }

        [HttpPost]
        public IActionResult Index(int userId, string password)
        {
            var hashedInputPassword = ComputeHash.Sha1(password);

            // بررسی در جدول Admin
            var admin = _db.Set<Admin>()
                .FirstOrDefault(a => a.AdminId == userId && a.HashedPassword == hashedInputPassword);

            if (admin != null)
            {
                HttpContext.Session.SetInt32("AdminId", admin.AdminId);
                return RedirectToAction("Index", "AdminDashboard");
            }

            // بررسی در جدول Instructor
                var instructor = _db.Set<Instructor>()
                .FirstOrDefault(i => i.InstructorId == userId && i.HashedPassword == hashedInputPassword);

            if (instructor != null)
            {
                HttpContext.Session.SetInt32("InstructorId", instructor.InstructorId);
                return RedirectToAction("Index", "InstructorDashboard");
            }

            // بررسی در جدول Student
                var student = _db.Set<Student>()
                .FirstOrDefault(s => s.StudentId == userId && s.HashedPassword == hashedInputPassword);

            if (student != null)
            {
                HttpContext.Session.SetInt32("StudentId", student.StudentId);
                return RedirectToAction("Index", "StudentDashboard");
            }

            // اگر لاگین ناموفق بود
                TempData["LoginError"] = "نام کاربری یا رمز عبور اشتباه است.";
            return RedirectToAction("Index");
            }
    }
}
