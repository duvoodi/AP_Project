using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using System.Globalization;
using AP_Project.Helpers.FormUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AP_Project.FormViewModels.StudentForm;

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

        [HttpGet]
        public IActionResult AddStudentIndex()
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            var pc = new PersianCalendar();
            int currentPersianYear = pc.GetYear(DateTime.Now);
            ViewData["currentPersianYear"] = currentPersianYear;
            ViewData["Form"] = new StudentFormViewModel();
            return View("~/Views/AdminDashboard/StudentManagement/AddStudent.cshtml", admin);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateStudentCode(int year)
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            try
            {
                var codeGenerator = new CodeGenerator(_db);
                var code = await codeGenerator.GenerateStudentCodeAsync(year);
                return Json(code);
            }
            catch (Exception)
            {
                return Json(" ");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(StudentFormViewModel StudentForm)
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            // ریست ارور های سمت سرور برای مقدار دهی مجدد
            ModelState.ReplaceModelError("GeneralError", "");

            // تبدیل فیلد خالی فرم که اینجا نال میشوند و اینولید میشوند به فیلد امپتی ولید
            // بدلیل فیلد های آپشنال یا خطای نال ندادن در چک ها
            ModelState.NullFieldsToValidEmpty(StudentForm);

            // چک همه فیلدها با شو ارور ترو
            foreach (var prop in typeof(StudentFormViewModel).GetProperties())
            {
                ModelState.ValidateField(StudentForm, prop.Name, true);
            }

            // چک تکراری نبودن داده های یکتا با سرور

            // چک یکتایی کد دانشجویی
            if (int.TryParse(StudentForm.StudentId, out int studentIdInt))
            {
                var StudentIdExists = await _db.Students.AnyAsync(i => i.StudentId == studentIdInt);
                if (StudentIdExists)
                {
                    ModelState.AppendModelError("GeneralError", "کد دانشجویی تولید شده تکراری بود و مجدداً تولید شد.");
                    ViewData["IsStudentIdDuplicate"] = true;
                }
            }

            // چک یکتایی نام و نام خانوادگی
            var nameExists = await _db.Students.AnyAsync(i =>
                i.FirstName == StudentForm.FirstName &&
                i.LastName == StudentForm.LastName);
            if (nameExists)
            {
                ModelState.AppendModelError("GeneralError", "نام و نام خانوادگی وارد شده قبلاً ثبت شده است.");
            }

            // چک یکتایی ایمیل
            var emailExists = await _db.Students.AnyAsync(i =>
                i.Email.ToLower() == StudentForm.Email.ToLower());
            if (emailExists)
            {
                ModelState.AppendModelError("GeneralError", "ایمیل وارد شده قبلاً ثبت شده است.");
            }

            // برگشت در صورت وجود خطا
            if (!ModelState.IsValid)
            {
                ViewData["Form"] = StudentForm;
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/StudentManagement/AddStudent.cshtml", admin);
            }

            try
            {
                var student = new Student
                {
                    FirstName = StudentForm.FirstName,
                    LastName = StudentForm.LastName,
                    Email = StudentForm.Email,
                    StudentId = int.Parse(StudentForm.StudentId),
                    EnrollmentYear = int.Parse(StudentForm.EnrollmentYear),
                    HashedPassword = ComputeHash.Sha1(StudentForm.Password)
                };

                _db.Students.Add(student);
                await _db.SaveChangesAsync();

                var userRole = new UserRole
                {
                    UserId = student.Id,
                    RoleId = 3
                };
                _db.UserRoles.Add(userRole);
                await _db.SaveChangesAsync();

                return RedirectToAction("Index", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch (Exception)
            {
                ModelState.AppendModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد؛ لطفاً دوباره تلاش کنید...");
                ViewData["Form"] = StudentForm;
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/StudentManagement/AddStudent.cshtml", admin);
            }
        }
    }
}
