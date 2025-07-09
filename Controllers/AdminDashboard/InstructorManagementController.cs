using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using System.Globalization;
using AP_Project.Helpers.FormUtils;
using Microsoft.EntityFrameworkCore;
using AP_Project.FormViewModels.InstructorForm;

namespace AP_Project.Controllers
{
    public class InstructorManagementController : Controller
    {
        private readonly AppDbContext _db;
        public InstructorManagementController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Index()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
                return RedirectToAction("Index", "Login");

            var admin = _db.Set<Admin>().FirstOrDefault(a => a.AdminId == adminId.Value);
            if (admin == null)
                return RedirectToAction("Index", "Login");

            var instructors = _db.Instructors
                .OrderBy(i => i.LastName)
                .ThenBy(i => i.FirstName)
                .ToList();

            ViewBag.Instructors = instructors;
            return View("~/Views/AdminDashboard/InstructorManagement/Index.cshtml", admin);
        }

        [HttpGet]
        public IActionResult AddInstructorIndex()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
                return RedirectToAction("Index", "Login");

            var admin = _db.Set<Admin>().FirstOrDefault(a => a.AdminId == adminId.Value);
            if (admin == null)
                return RedirectToAction("Index", "Login");

            var pc = new PersianCalendar();
            int currentPersianYear = pc.GetYear(DateTime.Now);
            ViewData["currentPersianYear"] = currentPersianYear;
            return View("~/Views/AdminDashboard/InstructorManagement/AddInstructor.cshtml", admin);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateInstructorCode(int year)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
                return RedirectToAction("Index", "Login");

            var admin = await _db.Set<Admin>().FirstOrDefaultAsync(a => a.AdminId == adminId.Value);
            if (admin == null)
                return RedirectToAction("Index", "Login");

            var codeGenerator = new CodeGenerator(_db);
            var code = await codeGenerator.GenerateInstructorCodeAsync(year);
            return Json(code);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInstructor(InstructorFormViewModel InstructorForm)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
                return RedirectToAction("Index", "Login");

            var admin = _db.Admins.FirstOrDefault(a => a.AdminId == adminId.Value);
            if (admin == null)
                return RedirectToAction("Index", "Login");

            // تبدیل فیلد ها نال و گرفته نشده به رشته خالی
            // برای جلوگیری از اکسپشن حین چک کردنشون
            InstructorForm.NullFieldsToEmpty();

            // چک همه فیلدها با شو ارور ترو
            foreach (var prop in typeof(InstructorFormViewModel).GetProperties())
            {
                ModelState.ValidateField(InstructorForm, prop.Name, true);
            }

            // چک یکتایی کد مدرسی
            if (int.TryParse(InstructorForm.InstructorId, out int instructorIdInt))
            {
                var exists = await _db.Instructors.AnyAsync(i => i.InstructorId == instructorIdInt);
                if (exists)
                {
                    ModelState.ReplaceModelError("InstructorId", "کد مدرسی تکراری بود و مجدداً تولید شد");
                }
            }

            // برگشت در صورت وجود خطا
            if (!ModelState.IsValid)
            {
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/InstructorManagement/AddInstructor.cshtml", admin);
            }

            var instructor = new Instructor
            {
                FirstName = InstructorForm.FirstName,
                LastName = InstructorForm.LastName,
                Email = InstructorForm.Email,
                InstructorId = int.Parse(InstructorForm.InstructorId),
                HireYear = int.Parse(InstructorForm.HireYear),
                Salary = decimal.Parse(InstructorForm.Salary),
                HashedPassword = ComputeHash.Sha1(InstructorForm.Password)
            };

            _db.Instructors.Add(instructor);
            await _db.SaveChangesAsync();

            var userRole = new UserRole
            {
                UserId = instructor.Id,
                RoleId = 2
            };
            _db.UserRoles.Add(userRole);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
        }
    }
}