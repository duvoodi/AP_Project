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
            try
            {
                var codeGenerator = new CodeGenerator(_db);
                var code = await codeGenerator.GenerateInstructorCodeAsync(year);
                return Json(code);
            }
            catch (Exception)
            {
                return Json(" ");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInstructor(InstructorFormViewModel InstructorForm)
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            // ریست ارور های سمت سرور برای مقدار دهی مجدد
            ModelState.ReplaceModelError("GeneralError", "");

            // تبدیل فیلد خالی فرم که اینجا نال میشوند و اینولید میشوند به فیلد امپتی ولید
            // بدلیل فیلد های آپشنال یا خطای نال ندادن در چک ها
            ModelState.NullFieldsToValidEmpty(InstructorForm);

            // چک همه فیلدها با شو ارور ترو
            foreach (var prop in typeof(InstructorFormViewModel).GetProperties())
            {
                ModelState.ValidateField(InstructorForm, prop.Name, true);
            }

            // چک تکراری نبودن داده های یکتا با سرور

            if (ModelState.TryGetValue("InstructorId", out var iId) && iId.Errors.Count == 0)
                // چک یکتایی کد مدرسی
                if (int.TryParse(InstructorForm.InstructorId, out int instructorIdInt))
                {
                    var InstructorIdExists = await _db.Instructors.AnyAsync(i => i.InstructorId == instructorIdInt);
                    if (InstructorIdExists)
                    {
                        ModelState.AppendModelError("GeneralError", "کد مدرسی تولید شده تکراری بود و مجدداً تولید شد.");
                        ViewData["IsInstructorIdDuplicate"] = true;
                    }
                }
                
            if (ModelState.TryGetValue("FirstName", out var fname) && fname.Errors.Count == 0
                    && ModelState.TryGetValue("LastName", out var lname) && lname.Errors.Count == 0)
            {
                // چک یکتایی نام و نام خانوادگی
                var nameExists = await _db.Instructors.AnyAsync(i =>
                    i.FirstName == InstructorForm.FirstName &&
                    i.LastName == InstructorForm.LastName);
                if (nameExists)
                {
                    ModelState.AppendModelError("GeneralError", "نام و نام خانوادگی وارد شده قبلاً ثبت شده است.");
                }
            }
            if (ModelState.TryGetValue("Email", out var email) && email.Errors.Count == 0)
            {
                // چک یکتایی ایمیل
                var emailExists = await _db.Instructors.AnyAsync(i =>
                    i.Email.ToLower() == InstructorForm.Email.ToLower());
                if (emailExists)
                {
                    ModelState.AppendModelError("GeneralError", "ایمیل وارد شده قبلاً ثبت شده است.");
                }
            }

            // برگشت در صورت وجود خطا
            if (!ModelState.IsValid)
            {
                ViewData["Form"] = InstructorForm;
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/InstructorManagement/AddInstructor.cshtml", admin);
            }

            try
            {
                var instructor = new Instructor
                {
                    Id = Guid.NewGuid(), // ایجاد تا بشه به یوزر رول وصل کرد
                    FirstName = InstructorForm.FirstName,
                    LastName = InstructorForm.LastName,
                    Email = InstructorForm.Email,
                    InstructorId = int.Parse(InstructorForm.InstructorId),
                    HireYear = int.Parse(InstructorForm.HireYear),
                    Salary = decimal.Parse(InstructorForm.Salary),
                    HashedPassword = ComputeHash.Sha1(InstructorForm.Password)
                };

                _db.Instructors.Add(instructor);

                var userRole = new UserRole
                {
                    UserId = instructor.Id,
                    RoleId = 2
                };
                _db.UserRoles.Add(userRole);

                // ذخیر نهایی همه تغییرات
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch (Exception)
            {
                ModelState.AppendModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["Form"] = InstructorForm;
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/InstructorManagement/AddInstructor.cshtml", admin);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditInstructor(Guid id)
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            // پیدا کردن استاد جی یو آی دی گرفته شده
            var instructor = await _db.Instructors.FindAsync(id);
            if (instructor == null)
                return NotFound();

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

            // پیدا کردن استاد جی یو آی دی گرفته شده
            var instructor = await _db.Instructors.FindAsync(id);
            if (instructor == null)
                return NotFound();

            // ریست ارور های سمت سرور برای مقدار دهی مجدد
            ModelState.ReplaceModelError("GeneralError", "");


            // تبدیل فیلد خالی فرم که اینجا نال میشوند و اینولید میشوند به فیلد امپتی ولید
            // بدلیل فیلد های آپشنال یا خطای نال ندادن در چک ها
            ModelState.NullFieldsToValidEmpty(InstructorForm);

            // چک همه فیلدها با شو ارور ترو
            foreach (var prop in typeof(InstructorFormViewModel).GetProperties())
            {
                ModelState.ValidateField(InstructorForm, prop.Name, true, true);
            }

            // چک تکراری نبودن داده های یکتا با سرور (اگر تغییر کردند)

            if (ModelState.TryGetValue("InstructorId", out var iId) && iId.Errors.Count == 0)
                // چک یکتایی کد مدرسی (اگر تغییر کرده)
                if (int.TryParse(InstructorForm.InstructorId, out int instructorIdInt) &&
                    instructor.InstructorId != instructorIdInt)
                {
                    var InstructorIdExists = await _db.Instructors.AnyAsync(i => i.InstructorId == instructorIdInt);
                    if (InstructorIdExists)
                    {
                        ModelState.AppendModelError("GeneralError", "کد مدرسی تولید شده تکراری بود و مجدداً تولید شد.");
                        ViewData["IsInstructorIdDuplicate"] = true;
                    }
                }

            if (ModelState.TryGetValue("FirstName", out var fname) && fname.Errors.Count == 0
                    && ModelState.TryGetValue("LastName", out var lname) && lname.Errors.Count == 0)
                // چک یکتایی نام و نام خانوادگی (اگر تغییر کرده)
                if (instructor.FirstName != InstructorForm.FirstName ||
                    instructor.LastName != InstructorForm.LastName)
                {
                    var nameExists = await _db.Instructors.AnyAsync(i =>
                        i.FirstName == InstructorForm.FirstName &&
                        i.LastName == InstructorForm.LastName &&
                        i.Id != instructor.Id);
                    if (nameExists)
                    {
                        ModelState.AppendModelError("GeneralError", "نام و نام خانوادگی وارد شده قبلاً ثبت شده است.");
                    }
                }


            // چک یکتایی ایمیل (اگر تغییر کرده)
            if (ModelState.TryGetValue("Email", out var email) && email.Errors.Count == 0)
                if (!string.Equals(instructor.Email, InstructorForm.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _db.Instructors.AnyAsync(i =>
                        i.Email.ToLower() == InstructorForm.Email.ToLower() &&
                        i.Id != instructor.Id);
                    if (emailExists)
                    {
                        ModelState.AppendModelError("GeneralError", "ایمیل وارد شده قبلاً ثبت شده است.");
                    }
                }
                
            // برگشت در صورت وجود خطا
            if (!ModelState.IsValid)
            {
                ViewData["Form"] = InstructorForm;
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/InstructorManagement/EditInstructor.cshtml", admin);
            }

            try
            {
                instructor.FirstName = InstructorForm.FirstName;
                instructor.LastName = InstructorForm.LastName;
                instructor.Email = InstructorForm.Email;

                // اگر سال استخدام تغییر کرده بود، کد مدرسی جدید ست کن
                if (InstructorForm.HireYear != instructor.HireYear.ToString())
                {
                    instructor.InstructorId = int.Parse(InstructorForm.InstructorId);
                }

                instructor.HireYear = int.Parse(InstructorForm.HireYear);
                instructor.Salary = decimal.Parse(InstructorForm.Salary);

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
                ModelState.AppendModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["Form"] = InstructorForm;
                ViewData["currentPersianYear"] = new PersianCalendar().GetYear(DateTime.Now);
                return View("~/Views/AdminDashboard/InstructorManagement/EditInstructor.cshtml", admin);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteInstructor(Guid id)
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            // پیدا کردن استاد جی یو آی دی گرفته شده
            var instructor = await _db.Instructors.FindAsync(id);
            if (instructor == null)
                return NotFound();

            // ریست ارور های سمت سرور برای مقدار دهی مجدد
            ModelState.ReplaceModelError("GeneralError", "");

            // اگر استاد تیچزی دارد هشدار دهد
            var hasCourses = await _db.Teaches.AnyAsync(t => t.InstructorUserId == id);
            if(hasCourses)
                ModelState.AppendModelError("GeneralError", "با حذف این استاد، بعضی از دروس بدون استاد می‌مانند و باید در پنل مدیریت دروس آن دروس را ویرایش یا حذف کنید");

            var form = new InstructorFormViewModel
            {
                FirstName = instructor.FirstName,
                LastName = instructor.LastName,
                Email = instructor.Email,
                InstructorId = instructor.InstructorId.ToString(),
                HireYear = instructor.HireYear.ToString(),
                Salary = instructor.Salary.ToString("0")
            };
            ViewData["Form"] = form;
            return View("~/Views/AdminDashboard/InstructorManagement/DeleteInstructor.cshtml", admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstructor(Guid id, InstructorFormViewModel InstructorForm) // ویو مدل فرم صرفا جهت متفاوت بودن پارامتر ها اکشن پست با گت اش گرفته شده و از آن هیچ استفاده ای نشده
        {
            // کلاس پایه سشن را برای هر اکشن چک میکند
            // اگر درست نبود ریدایرکت به لاگین و گرنه شی سشن را میدهد
            var admin = CurrentAdmin;

            // پیدا کردن استاد جی یو آی دی گرفته شده
            var instructor = await _db.Instructors.FindAsync(id);
            if (instructor == null)
                return NotFound();

            // ریست ارور های سمت سرور برای مقدار دهی مجدد
            ModelState.ReplaceModelError("GeneralError", "");

            try
            {
                // نال کردن آی دی این استاد در تیچز ها
                var teachesToUpdate = await _db.Teaches
                    .Where(t => t.InstructorUserId == id)
                    .ToListAsync();
                foreach (var t in teachesToUpdate)
                    t.InstructorUserId = null;

                // حذف استاد
                _db.Instructors.Remove(instructor);

                // حذف دستی یوزر بجای مانده از استاد چون امکان وان تو وان نداشتند
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
                // اگر استاد تیچزی دارد هشدار دهد
                var hasCourses = await _db.Teaches.AnyAsync(t => t.InstructorUserId == id);
                if(hasCourses)
                    ModelState.AppendModelError("GeneralError", "با حذف این استاد، بعضی از دروس بدون استاد می‌مانند و باید در پنل مدیریت دروس آن دروس را ویرایش یا حذف کنید");
                var form = new InstructorFormViewModel
                {
                    FirstName = instructor.FirstName,
                    LastName = instructor.LastName,
                    Email = instructor.Email,
                    InstructorId = instructor.InstructorId.ToString(),
                    HireYear = instructor.HireYear.ToString(),
                    Salary = instructor.Salary.ToString("0")
                };
                ViewData["Form"] = form;
                return View("~/Views/AdminDashboard/InstructorManagement/DeleteInstructor.cshtml", admin);
            }
        }
    }
}
