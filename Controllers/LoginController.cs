using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;

namespace AP_Project.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _db;

        public LoginController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Index()
        {
            // اگر قبلاً لاگین کرده و Session فعال است، به داشبورد منتقل شود
            if (HttpContext.Session.GetInt32("AdminId") != null
            || HttpContext.Session.GetInt32("InstructorId") != null
            || HttpContext.Session.GetInt32("StudentId") != null)

                return View("~/Views/Login/redirect.cshtml");

            // اگر خطای لاگین وجود دارد، نمایش داده شود
            if (TempData["LoginError"] != null)
            {
                ModelState.AddModelError("", TempData["LoginError"].ToString());
            }

            return View();
        }

        [HttpGet]
        public IActionResult CheckSession(bool hashed)
        //  اکشن ای جی ای ایکس برای برگرداندن رول و استرینگ آی دی یا هش سشن فعلی
        {
            string hash = "";
            if (HttpContext.Session.GetInt32("AdminId") is int adminId)
            {
                if (hashed)
                    hash = ComputeHash.Sha1(adminId.ToString());
                return Json(new { role = "admin", id = hashed ? hash : adminId.ToString() });
            }
            else if (HttpContext.Session.GetInt32("InstructorId") is int instructorId)
            {
                if (hashed)
                    hash = ComputeHash.Sha1(instructorId.ToString());
                return Json(new { role = "instructor", id = hashed ? hash : instructorId.ToString() });
            }
            else if (HttpContext.Session.GetInt32("StudentId") is int studentId)
            {
                if (hashed)
                    hash = ComputeHash.Sha1(studentId.ToString());
                return Json(new { role = "student", id = hashed ? hash : studentId.ToString() });
            }
            return Json(new { role = "none" });
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
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
                HttpContext.Session.Clear(); // سشن ها قبلی اگر اشتباهی باز هست ببندد
                HttpContext.Session.SetInt32("AdminId", admin.AdminId);
                return View("~/Views/Login/redirect.cshtml");
                // میره صفحه ریدایرکت و از اونجا میره داشبورد  تا با بک زدن از صفحه داشبورد به صفحه لاگین کش شده نره            }
            }

            // بررسی در جدول Instructor
            var instructor = _db.Set<Instructor>()
                .FirstOrDefault(i => i.InstructorId == userId && i.HashedPassword == hashedInputPassword);

            if (instructor != null)
            {
                HttpContext.Session.Clear(); // سشن ها قبلی اگر اشتباهی باز هست ببندد
                HttpContext.Session.SetInt32("InstructorId", instructor.InstructorId);
                return View("~/Views/Login/redirect.cshtml");
                // میره صفحه ریدایرکت و از اونجا میره داشبورد  تا با بک زدن از صفحه داشبورد به صفحه لاگین کش شده نره            }
            }

            // بررسی در جدول Student
            var student = _db.Set<Student>()
                .FirstOrDefault(s => s.StudentId == userId && s.HashedPassword == hashedInputPassword);

            if (student != null)
            {
                HttpContext.Session.Clear(); // سشن ها قبلی اگر اشتباهی باز هست ببندد
                HttpContext.Session.SetInt32("StudentId", student.StudentId);
                return View("~/Views/Login/redirect.cshtml");
                // میره صفحه ریدایرکت و از اونجا میره داشبورد  تا با بک زدن از صفحه داشبورد به صفحه لاگین کش شده نره
            }

            // اگر لاگین ناموفق بود
            TempData["LoginError"] = "نام کاربری یا رمز عبور اشتباه است.";
            return RedirectToAction("Index");
            
        }
    }
}
