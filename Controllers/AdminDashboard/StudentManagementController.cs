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

            if (ModelState.TryGetValue("StudentId", out var sId) && sId.Errors.Count == 0)
            {
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
            }
            
            if (ModelState.TryGetValue("FirstName", out var fname) && fname.Errors.Count == 0
                && ModelState.TryGetValue("LastName", out var lname) && lname.Errors.Count == 0)
            {
                // چک یکتایی نام و نام خانوادگی
                var nameExists = await _db.Students.AnyAsync(i =>
                    i.FirstName == StudentForm.FirstName &&
                    i.LastName == StudentForm.LastName);
                if (nameExists)
                {
                    ModelState.AppendModelError("GeneralError", "نام و نام خانوادگی وارد شده قبلاً ثبت شده است.");
                }
            }

            if (ModelState.TryGetValue("Email", out var email) && email.Errors.Count == 0)
            {
                // چک یکتایی ایمیل
                var emailExists = await _db.Students.AnyAsync(i =>
                    i.Email.ToLower() == StudentForm.Email.ToLower());
                if (emailExists)
                {
                    ModelState.AppendModelError("GeneralError", "ایمیل وارد شده قبلاً ثبت شده است.");
                }
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
                    Id = Guid.NewGuid(), // ایجاد تا بشه به یوزر رول وصل کرد
                    FirstName = StudentForm.FirstName,
                    LastName = StudentForm.LastName,
                    Email = StudentForm.Email,
                    StudentId = int.Parse(StudentForm.StudentId),
                    EnrollmentYear = int.Parse(StudentForm.EnrollmentYear),
                    HashedPassword = ComputeHash.Sha1(StudentForm.Password)
                };

                _db.Students.Add(student);

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
                ModelState.AppendModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["Form"] = StudentForm;
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/StudentManagement/AddStudent.cshtml", admin);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(Guid id)
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            // پیدا کردن دانشجو جی یو آی دی گرفته شده
            var student = await _db.Students.FindAsync(id);
            if (student == null)
                return NotFound();

            var form = new StudentFormViewModel
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                StudentId = student.StudentId.ToString(),
                EnrollmentYear = student.EnrollmentYear.ToString(),
                // رمز عبور را خالی می‌گذاریم (برای امنیت)
                Password = "",
                ConfirmPassword = ""
            };
            ViewData["Form"] = form;
            ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
            return View("~/Views/AdminDashboard/StudentManagement/EditStudent.cshtml", admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(Guid id, StudentFormViewModel StudentForm)
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            // پیدا کردن دانشجو جی یو آی دی گرفته شده
            var student = await _db.Students.FindAsync(id);
            if (student == null)
                return NotFound();

            // ریست ارور های سمت سرور برای مقدار دهی مجدد
            ModelState.ReplaceModelError("GeneralError", "");


            // تبدیل فیلد خالی فرم که اینجا نال میشوند و اینولید میشوند به فیلد امپتی ولید
            // بدلیل فیلد های آپشنال یا خطای نال ندادن در چک ها
            ModelState.NullFieldsToValidEmpty(StudentForm);

            // چک همه فیلدها با شو ارور ترو
            foreach (var prop in typeof(StudentFormViewModel).GetProperties())
            {
                ModelState.ValidateField(StudentForm, prop.Name, true, true);
            }

            // چک تکراری نبودن داده های یکتا با سرور (اگر تغییر کردند)


            if (ModelState.TryGetValue("StudentId", out var sId) && sId.Errors.Count == 0)
                // چک یکتایی کد دانشجویی (اگر تغییر کرده)
                if (int.TryParse(StudentForm.StudentId, out int studentIdInt) &&
                    student.StudentId != studentIdInt)
                {
                    var StudentIdExists = await _db.Students.AnyAsync(i => i.StudentId == studentIdInt);
                    if (StudentIdExists)
                    {
                        ModelState.AppendModelError("GeneralError", "کد دانشجویی تولید شده تکراری بود و مجدداً تولید شد.");
                        ViewData["IsStudentIdDuplicate"] = true;
                    }
                }


            if (ModelState.TryGetValue("FirstName", out var fname) && fname.Errors.Count == 0
                    && ModelState.TryGetValue("LastName", out var lname) && lname.Errors.Count == 0)
                // چک یکتایی نام و نام خانوادگی (اگر تغییر کرده)
                if (student.FirstName != StudentForm.FirstName ||
                    student.LastName != StudentForm.LastName)
                {
                    var nameExists = await _db.Students.AnyAsync(i =>
                        i.FirstName == StudentForm.FirstName &&
                        i.LastName == StudentForm.LastName &&
                        i.Id != student.Id);
                    if (nameExists)
                    {
                        ModelState.AppendModelError("GeneralError", "نام و نام خانوادگی وارد شده قبلاً ثبت شده است.");
                    }
                }

            if (ModelState.TryGetValue("Email", out var email) && email.Errors.Count == 0)
                // چک یکتایی ایمیل (اگر تغییر کرده)
                if (!string.Equals(student.Email, StudentForm.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _db.Students.AnyAsync(i =>
                        i.Email.ToLower() == StudentForm.Email.ToLower() &&
                        i.Id != student.Id);
                    if (emailExists)
                    {
                        ModelState.AppendModelError("GeneralError", "ایمیل وارد شده قبلاً ثبت شده است.");
                    }
                }
                

            // برگشت در صورت وجود خطا
            if (!ModelState.IsValid)
            {
                ViewData["Form"] = StudentForm;
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/StudentManagement/EditStudent.cshtml", admin);
            }

            try
            {
                student.FirstName = StudentForm.FirstName;
                student.LastName = StudentForm.LastName;
                student.Email = StudentForm.Email;

                // اگر سال استخدام تغییر کرده بود، کد دانشجویی جدید ست کن
                if (StudentForm.EnrollmentYear != student.EnrollmentYear.ToString())
                {
                    student.StudentId = int.Parse(StudentForm.StudentId);
                }

                student.EnrollmentYear = int.Parse(StudentForm.EnrollmentYear);

                // اگر رمز عبور جدید وارد شده بود، هش کن و ذخیره کن
                if (!string.IsNullOrWhiteSpace(StudentForm.Password))
                {
                    student.HashedPassword = ComputeHash.Sha1(StudentForm.Password);
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch (Exception)
            {
                ModelState.AppendModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["Form"] = StudentForm;
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/StudentManagement/EditStudent.cshtml", admin);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            // پیدا کردن دانشجو جی یو آی دی گرفته شده
            var student = await _db.Students.FindAsync(id);
            if (student == null)
                return NotFound();

            // ریست ارور های سمت سرور برای مقدار دهی مجدد
            ModelState.ReplaceModelError("GeneralError", "");

            var form = new StudentFormViewModel
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                StudentId = student.StudentId.ToString(),
                EnrollmentYear = student.EnrollmentYear.ToString(),
            };

            ViewData["StudentGuid"] = id;

            ViewData["Form"] = form;
            return View("~/Views/AdminDashboard/StudentManagement/DeleteStudent.cshtml", admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(Guid id, StudentFormViewModel StudentForm) // ویو مدل فرم صرفا جهت متفاوت بودن پارامتر ها اکشن پست با گت اش گرفته شده و از آن هیچ استفاده ای نشده
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            // پیدا کردن دانشجو جی یو آی دی گرفته شده
            var student = await _db.Students.FindAsync(id);
            if (student == null)
                return NotFound();

            // ریست ارور های سمت سرور برای مقدار دهی مجدد
            ModelState.ReplaceModelError("GeneralError", "");

            try
            {
                // حذف دانشجو
                _db.Students.Remove(student);

                // حذف دستی یوزر بجای مانده از دانشجو چون امکان وان تو وان نداشتند
                var user = await _db.Users.FindAsync(id);
                if (user != null)
                    _db.Users.Remove(user);

                // طبق تنظیمات دی بی کانتکست یوزر رول یوزر به صورت اتومات حذف می شود

                // ذخیره تغییرات
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch (Exception)
            {
                ModelState.AppendModelError("GeneralError", "خطایی هنگام حذف اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                var form = new StudentFormViewModel
                {
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Email = student.Email,
                    StudentId = student.StudentId.ToString(),
                    EnrollmentYear = student.EnrollmentYear.ToString(),
                };
                ViewData["Form"] = form;
                return View("~/Views/AdminDashboard/StudentManagement/DeleteStudent.cshtml", admin);
            }
        }
    }
}
