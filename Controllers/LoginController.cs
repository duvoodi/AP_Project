using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using AP_Project.Helpers.FormUtils;
using AP_Project.FormViewModels.LoginForm;

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

            return View();
        }

        [HttpGet]
        //  اکشن ای جی ای ایکس برای برگرداندن رول و آی دی هش شده سشن فعلی
        public IActionResult CheckSession(bool withId)
        {
            string role = "none";
            string hash = null;

            if (HttpContext.Session.GetInt32("AdminId") is int adminId)
            {
                role = "admin";
                hash = ComputeHash.Sha1(adminId.ToString());
            }
            else if (HttpContext.Session.GetInt32("InstructorId") is int instructorId)
            {
                role = "instructor";
                hash = ComputeHash.Sha1(instructorId.ToString());
            }
            else if (HttpContext.Session.GetInt32("StudentId") is int studentId)
            {
                role = "student";
                hash = ComputeHash.Sha1(studentId.ToString());
            }

            if (withId && hash != null)
                return Json(new { role, id = hash });
            else
                return Json(new { role });
        }

        [HttpGet]
        public IActionResult Redirect()
        {
            return View("~/Views/Login/redirect.cshtml");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(LoginFormViewModel LoginForm)
        {
            // ریست ارور های سمت سرور برای مقدار دهی مجدد
            ModelState.ReplaceModelError("GeneralError", "");

            // تبدیل فیلد خالی فرم که اینجا نال میشوند و اینولید میشوند به فیلد امپتی ولید
            // تا در ادامه دستی چکشون کنیم
            ModelState.NullFieldsToValidEmpty(LoginForm);

            // چک همه فیلدها با شو ارور ترو
            foreach (var prop in typeof(LoginFormViewModel).GetProperties())
            {
                ModelState.ValidateField(LoginForm, prop.Name, true);
            }

            var hashedInputPassword = ComputeHash.Sha1(LoginForm.Password);

            try
            {
                // بررسی در جدول Admin
                var admin = _db.Set<Admin>()
                    .FirstOrDefault(a => a.AdminId.ToString() == LoginForm.Username && a.HashedPassword == hashedInputPassword);

                if (admin != null)
                {
                    HttpContext.Session.Clear(); // سشن ها قبلی اگر اشتباهی باز هست ببندد
                    HttpContext.Session.SetInt32("AdminId", admin.AdminId);
                    return View("~/Views/Login/redirect.cshtml");
                    // میره صفحه ریدایرکت و از اونجا میره داشبورد  تا با بک زدن از صفحه داشبورد به صفحه لاگین کش شده نره            }
                }

                // بررسی در جدول Instructor
                var instructor = _db.Set<Instructor>()
                    .FirstOrDefault(i => i.InstructorId.ToString() == LoginForm.Username && i.HashedPassword == hashedInputPassword);

                if (instructor != null)
                {
                    HttpContext.Session.Clear(); // سشن ها قبلی اگر اشتباهی باز هست ببندد
                    HttpContext.Session.SetInt32("InstructorId", instructor.InstructorId);
                    return View("~/Views/Login/redirect.cshtml");
                    // میره صفحه ریدایرکت و از اونجا میره داشبورد  تا با بک زدن از صفحه داشبورد به صفحه لاگین کش شده نره
                }

                // بررسی در جدول Student
                var student = _db.Set<Student>()
                    .FirstOrDefault(s => s.StudentId.ToString() == LoginForm.Username && s.HashedPassword == hashedInputPassword);

                if (student != null)
                {
                    HttpContext.Session.Clear(); // سشن ها قبلی اگر اشتباهی باز هست ببندد
                    HttpContext.Session.SetInt32("StudentId", student.StudentId);
                    return View("~/Views/Login/redirect.cshtml");
                    // میره صفحه ریدایرکت و از اونجا میره داشبورد  تا با بک زدن از صفحه داشبورد به صفحه لاگین کش شده نره
                }

                ModelState.AppendModelError("GeneralError", "نام کاربری یا رمز عبور وارد شده نامعتبر است.");
            }
            catch
            {
                ModelState.AppendModelError("GeneralError", "خطایی غیر منتظره هنگام بررسی اطلاعات رخ داد!");

            }

            var form = new LoginFormViewModel
            {
                Username = LoginForm.Username,
                Password = LoginForm.Password
            };
            ViewData["Form"] = form;
            return View("Index");
        }
    }
}
