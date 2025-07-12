using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using System.Globalization;
using AP_Project.Helpers.FormUtils;
using Microsoft.EntityFrameworkCore;
using AP_Project.FormViewModels.InstructorForm;

namespace AP_Project.Controllers
{
    public class InstructorManagementController : BaseAdminController
    {
        public InstructorManagementController(AppDbContext db) : base(db)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

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
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            var pc = new PersianCalendar();
            int currentPersianYear = pc.GetYear(DateTime.Now);
            ViewData["currentPersianYear"] = currentPersianYear;
            return View("~/Views/AdminDashboard/InstructorManagement/AddInstructor.cshtml", admin);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateInstructorCode(int year)
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد

            var codeGenerator = new CodeGenerator(_db);
            var code = await codeGenerator.GenerateInstructorCodeAsync(year);
            return Json(code);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInstructor(InstructorFormViewModel InstructorForm)
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            // تبدیل فیلد خالی فرم که اینجا نال میشوند و اینولید میشوند به فیلد امپتی ولید
            // تا در ادامه دستی چکشون کنیم
            ModelState.NullFieldsToValidEmpty(InstructorForm);

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
                ViewData["Form"] = InstructorForm;
                ViewData["Hash"] = ComputeHash.Sha1(admin.AdminId.ToString());

                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/InstructorManagement/AddInstructor.cshtml", admin);
            }

            try
            {
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
            catch (Exception ex)
            {
                ModelState.AddModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد. لطفاً دوباره تلاش کنید.");
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                ViewData["Hash"] = ComputeHash.Sha1(admin.AdminId.ToString());
                return View("~/Views/AdminDashboard/InstructorManagement/AddInstructor.cshtml", admin);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditInstructor(Guid id)
        {
            var admin = CurrentAdmin;
            var instructor = await _db.Instructors.FindAsync(id);
            if (instructor == null)
            {
                return NotFound();
            }

            var form = new InstructorFormViewModel
            {
                FirstName = instructor.FirstName,
                LastName = instructor.LastName,
                Email = instructor.Email,
                InstructorId = instructor.InstructorId.ToString(),
                HireYear = instructor.HireYear.ToString(),
                Salary = instructor.Salary.ToString("0"),
                // رمز عبور را خالی می‌گذاریم (برای امنیت)
                Password = "",
                ConfirmPassword = ""
            };

            ViewData["Form"] = form;
            ViewData["Hash"] = ComputeHash.Sha1(admin.AdminId.ToString());
            ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);

            return View("~/Views/AdminDashboard/InstructorManagement/EditInstructor.cshtml", admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInstructor(Guid id, InstructorFormViewModel InstructorForm)
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            // تبدیل فیلد خالی فرم که اینجا نال میشوند و اینولید میشوند به فیلد امپتی ولید
            // تا در ادامه دستی چکشون کنیم
            ModelState.NullFieldsToValidEmpty(InstructorForm);

            // چک همه فیلدها با شو ارور ترو
            foreach (var prop in typeof(InstructorFormViewModel).GetProperties())
            {
                ModelState.ValidateField(InstructorForm, prop.Name, true, true);
            }

            // پیدا کردن استاد
            var instructor = await _db.Instructors.FindAsync(id);
            if (instructor == null)
                return NotFound();

            // اگر سال استخدام تغییر کرده بود، کد مدرسی جدید تولید کن
            bool hireYearChanged = InstructorForm.HireYear != instructor.HireYear.ToString();
            int newHireYear = int.Parse(InstructorForm.HireYear);

            if (hireYearChanged)
            {
                // تولید کد مدرسی جدید با سال جدید (مثل اکشن افزودن استاد)
                var codeGenerator = new CodeGenerator(_db);
                var newInstructorId = await codeGenerator.GenerateInstructorCodeAsync(newHireYear);
                InstructorForm.InstructorId = newInstructorId.ToString();
            }

            // چک یکتایی کد مدرسی (در هر صورت)
            if (int.TryParse(InstructorForm.InstructorId, out int instructorIdInt))
            {
                var exists = await _db.Instructors
                    .AnyAsync(i => i.InstructorId == instructorIdInt && i.Id != id);
                if (exists)
                {
                    ModelState.ReplaceModelError("InstructorId", "کد مدرسی تکراری است.");
                }
            }

            // برگشت در صورت وجود خطا
            if (!ModelState.IsValid)
            {
                ViewData["Form"] = InstructorForm;
                ViewData["Hash"] = ComputeHash.Sha1(admin.AdminId.ToString());
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/InstructorManagement/EditInstructor.cshtml", admin);
            }

            try
            {
                instructor.FirstName = InstructorForm.FirstName;
                instructor.LastName = InstructorForm.LastName;
                instructor.Email = InstructorForm.Email;
                instructor.HireYear = newHireYear;
                instructor.Salary = decimal.Parse(InstructorForm.Salary);

                // اگر سال استخدام تغییر کرده بود، کد مدرسی جدید ست کن
                if (hireYearChanged)
                {
                    instructor.InstructorId = int.Parse(InstructorForm.InstructorId);
                }

                // اگر رمز عبور جدید وارد شده بود، هش کن و ذخیره کن
                if (!string.IsNullOrWhiteSpace(InstructorForm.Password))
                {
                    instructor.HashedPassword = ComputeHash.Sha1(InstructorForm.Password);
                }

                await _db.SaveChangesAsync();

                return RedirectToAction("Index", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch (Exception)
            {
                ModelState.AddModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد. لطفاً دوباره تلاش کنید.");
                ViewData["Form"] = InstructorForm;
                ViewData["Hash"] = ComputeHash.Sha1(admin.AdminId.ToString());
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/InstructorManagement/EditInstructor.cshtml", admin);
            }
        }
    }
}
