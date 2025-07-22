using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using AP_Project.Helpers.FormUtils;
using Microsoft.EntityFrameworkCore;
using AP_Project.FormViewModels.CourseForm;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;
using AP_Project.Helpers;
using AP_Project.FormViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using AP_Project.Models.Courses;
using AP_Project.FormViewModels.ClassForm;
using AP_Project.Models.Classrooms;
using System.Globalization;

namespace AP_Project.Controllers
{
    public class ClassManagementController : BaseAdminController
    {
        public ClassManagementController(AppDbContext db) : base(db)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            var admin = CurrentAdmin;

            // بارگذاری سکشن‌هایی که به یک کلس روم تخصیص یافته‌اند
            var sections = _db.Sections
                .AsNoTracking()
                .Where(s => s.Classroom != null)
                .Include(s => s.Classroom)
                .Include(s => s.Course)
                    .ThenInclude(c => c.CourseCode)
                .Include(s => s.Teaches)
                    .ThenInclude(t => t.Instructor)
                .Include(s => s.Takes)
                    .ThenInclude(tk => tk.Student)
                .Include(s => s.TimeSlot)
                .OrderBy(s => s.Classroom.Building)                                   // 1. ساختمان
                .ThenBy(s => s.Classroom.RoomNumber)                                  // 2. شماره کلاس
                .ThenBy(s => s.Classroom.Capacity)                                    // 3. ظرفیت
                .ThenBy(s => s.Course.CourseCode.Title)                               // 4. عنوان درس
                .ThenBy(s => s.Teaches != null && s.Teaches.Instructor != null        // 5. نام استاد
                    ? s.Teaches.Instructor.FirstName
                    : string.Empty)
                .ThenBy(s => s.Teaches != null && s.Teaches.Instructor != null        // 6. نام خانوادگی استاد
                    ? s.Teaches.Instructor.LastName
                    : string.Empty)
                .ToList();

            ViewBag.Sections = sections;
            return View("~/Views/AdminDashboard/ClassManagement/Index.cshtml", admin);
        }

        private List<(Guid SectionId, string DetailText)> GetUnassignedSectionList(Guid sectionId = default)
        {
            var pc = new PersianCalendar();

            // اینکلود موارد لازم
            var query = _db.Sections
                .Include(s => s.Course).ThenInclude(c => c.CourseCode)
                .Include(s => s.Course.Prerequisites).ThenInclude(p => p.PrerequisiteCourseCode)
                .Include(s => s.Teaches).ThenInclude(t => t.Instructor)
                .Include(s => s.TimeSlot)
                .AsQueryable();

            var result = new List<(Guid SectionId, string DetailText)>();

            // اگر سکشن ای دی داده شده و کورس آن نال است، آن را دستی اول اضافه می‌کنیم
            if (sectionId != Guid.Empty)
            {
                var selected = query.FirstOrDefault(s => s.Id == sectionId);
                if (selected != null)
                {
                    if (selected.Course == null || selected.Course.CourseCode == null)
                    {
                        result.Add((selected.Id, "این درس حذف ایمن شده"));
                    }
                }
            }

            // سکشن خودمون اگر کورس غیر نال داره
            // به اضافه سایر سکشن‌ها یعنی با کورس نال که اساین نشدند یعنی با کلاس نال اند
            var list = query
                .Where(s => (s.Course != null && s.ClassroomId == null) || (s.Id == sectionId && s.Course != null))
                .AsEnumerable()
                .OrderBy(s => s.Course.CourseCode.Code)
                .ThenBy(s => s.Course.Unit)
                .ThenBy(s => (s.Year ?? 0) * 10 + (s.Semester ?? 0))
                .ThenBy(s => s.Teaches?.Instructor?.LastName)
                .ThenBy(s => s.TimeSlot?.Id)
                .Select(s =>
                {
                    // آماده‌سازی مقادیر با فرمت "PropName: Value یا ندارد"
                    string code = $"کد درس: {s.Course.CourseCode.Code}";
                    string unit = $"تعداد واحد: {s.Course.Unit}";

                    string prereqs = s.Course.Prerequisites.Any()
                        ? $"پیش‌نیازها: {string.Join(",", s.Course.Prerequisites
                                                    .Where(p => p.PrerequisiteCourseCode != null)
                                                    .Select(p => p.PrerequisiteCourseCode.Code))}"
                        : "پیش‌نیازها: ندارداختیاری";

                    int semValue = (s.Year ?? 0) * 10 + (s.Semester ?? 0);
                    string semester = $"نیم‌سال: {semValue}";

                    string instr = s.Teaches?.Instructor != null
                        ? $"استاد: {s.Teaches.Instructor.FirstName} {s.Teaches.Instructor.LastName}"
                        : "استاد: ندارد";

                    string when = s.TimeSlot != null
                        ? $"زمان برگزاری: {s.TimeSlot.Day} {s.TimeSlot.StartTime:hh\\:mm}–{s.TimeSlot.EndTime:hh\\:mm}"
                        : "زمان برگزاری: ندارد";

                    string exam = s.Course.FinalExamDate != default
                        ? $"تاریخ امتحان: {pc.GetYear(s.Course.FinalExamDate):0000}/{pc.GetMonth(s.Course.FinalExamDate):00}/{pc.GetDayOfMonth(s.Course.FinalExamDate):00}"
                        : "تاریخ امتحان: ندارد";

                    string desc = !string.IsNullOrWhiteSpace(s.Course.Description)
                        ? $"توضیحات: {s.Course.Description}"
                        : "توضیحات: ندارداختیاری";

                    // لیست قطعات را با " - " جدا می‌کنیم
                    var parts = new[]
                    {
                        code, unit, prereqs, semester,
                        instr, when, exam, desc
                    };

                    return (s.Id, string.Join(" - ", parts));
                })
                .ToList();

            result.AddRange(list);

            return result;
        }
        

        [HttpGet]
        public IActionResult AddClass()
        {
            var admin = CurrentAdmin;

            ViewData["Classroom"] = _db.Classrooms
                .OrderBy(cr => cr.Building)
                .ThenBy(cr => cr.RoomNumber)
                .ThenBy(cr => cr.Capacity)
                .ToList();

            ViewData["Sections"] = GetUnassignedSectionList();

            ViewData["Students"] = _db.Students
               .OrderBy(i => i.LastName)
               .ThenBy(i => i.FirstName)
               .ToList();

            return View("~/Views/AdminDashboard/ClassManagement/AddClass.cshtml", admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClass(ClassFormViewModel classForm)
        {
            var admin = CurrentAdmin;
            ModelState.ReplaceModelError("GeneralError", "");
            ModelState.NullFieldsToValidEmpty(classForm);

            // بررسی جی یو آی دی های گرفته شده از فرم

            var classExists = await _db.Classrooms.AnyAsync(cr => cr.Id == classForm.ClassLocationId);
            if (!classExists)
            {
                ModelState.AppendModelError("GeneralError", "محل برگزاری انتخاب شده یافت نشد!");
            }

            var section = await _db.Sections.FirstOrDefaultAsync(s => s.Id == classForm.SectionId);
            if (section == null)
            {
                ModelState.AppendModelError("GeneralError", "درس انتخاب شده یافت نشد!");
            }

            else if (section.ClassroomId != null)
            {
                ModelState.AppendModelError("GeneralError", "این درس قبلاً به یک کلاس تخصیص داده شده است!");
            }

            if (classForm.StudentIds != null && classForm.StudentIds.Any())
            {
                var existingStudents = await _db.Students
                    .Where(s => classForm.StudentIds.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync();

                var missingStudents = classForm.StudentIds.Except(existingStudents).ToList();
                if (missingStudents.Any())
                {
                    ModelState.AppendModelError("GeneralError", "برخی از دانشجوهای انتخاب شده یافت نشدند!");
                }

                classForm.StudentIds = classForm.StudentIds.Distinct().ToList(); // حذف موارد تکراری لیست

                // بررسی ظرفیت کلاس فقط وقتی کلاس و دانشجوها معتبر بودند
                if (classExists)
                {
                    var capacity = await _db.Classrooms
                        .Where(cr => cr.Id == classForm.ClassLocationId)
                        .Select(cr => cr.Capacity)
                        .FirstOrDefaultAsync();

                    if (classForm.StudentIds.Count > capacity)
                    {
                        ModelState.AppendModelError("StudentIds", "تعداد دانشجویان بیش از ظرفیت کلاس انتخاب شده است");
                    }
                }
            }

            // اعتبارسنجی فیلدها
            foreach (var prop in typeof(ClassFormViewModel).GetProperties())
            {
                ModelState.ValidateField(classForm, prop.Name, true);
            }

            // بررسی تداخل زمانی اگر همه چیز ها معتبر بودن
            if (ModelState.IsValid)
            {
                // چک تداخل برای کلاس با همین نیم‌سال و تایم‌اسلات
                var currentSection = await _db.Sections
                    .Where(s => s.Id == classForm.SectionId)
                    .Select(s => new { s.Year, s.Semester, s.TimeSlotId, s.ClassroomId })
                    .FirstOrDefaultAsync();

                if (currentSection != null && classForm.ClassLocationId != Guid.Empty)
                {
                    // بررسی تداخل کلاس در همان سال، نیم‌سال و بازه زمانی، به جز خودش
                    bool classConflict = await _db.Sections
                        .AnyAsync(s =>
                            s.Year == currentSection.Year &&
                            s.Semester == currentSection.Semester &&
                            s.TimeSlotId == currentSection.TimeSlotId &&
                            s.ClassroomId == classForm.ClassLocationId &&
                            s.Id != classForm.SectionId // از خودش صرف نظر شود
                        );

                    if (classConflict)
                    {
                        ModelState.AppendModelError("GeneralError", "در این زمان و مکان، کلاس دیگری قبلاً ثبت شده است.");
                    }
                }
            }

            // برگشت در صورت خطا
            if (!ModelState.IsValid)
            {
                ViewData["Classroom"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                ViewData["Sections"] = GetUnassignedSectionList();
                ViewData["Students"] = _db.Students.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToList();
                ViewData["Form"] = classForm;
                return View("~/Views/AdminDashboard/ClassManagement/AddClass.cshtml", admin);
            }

            // ذخیره سازی
            try
            {
                // ثبت کلاس در سکشن
                section.ClassroomId = classForm.ClassLocationId;
                _db.Sections.Update(section);

                // افزودن رکورد Takes برای هر دانشجو
                foreach (var studentId in classForm.StudentIds)
                {
                    _db.Takes.Add(new Takes
                    {
                        StudentUserId = studentId,
                        SectionId = section.Id,
                        Grade = ""
                    });
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch
            {
                ViewData["Classroom"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                ViewData["Sections"] = GetUnassignedSectionList();
                ViewData["Students"] = _db.Students.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToList();
                ViewData["Form"] = classForm;
                return View("~/Views/AdminDashboard/ClassManagement/AddClass.cshtml", admin);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditClass(Guid id)
        {
            var admin = CurrentAdmin;

            // بررسی جی یو آی دی گرفته شده از روت آی دی
            var section = await _db.Sections.FirstOrDefaultAsync(s => s.Id == id);
            if (section == null || section.ClassroomId == null)
                return NotFound();
            var classroom = await _db.Classrooms.FirstOrDefaultAsync(c => c.Id == section.ClassroomId);
            if (classroom == null)
                return NotFound();

            ViewData["Classroom"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
            ViewData["Sections"] = GetUnassignedSectionList(id);
            ViewData["Students"] = _db.Students.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToList();
            var form = new ClassFormViewModel
            {
                SectionId = section.Id,
                ClassLocationId = section.ClassroomId.Value,
                StudentIds = await _db.Takes
                    .Where(t => t.SectionId == id)
                    .Select(t => t.StudentUserId)
                    .ToListAsync()
            };
            ViewData["Form"] = form;

            return View("~/Views/AdminDashboard/ClassManagement/EditClass.cshtml", admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClass(Guid id, ClassFormViewModel classForm)
        {
            var admin = CurrentAdmin;
            ModelState.ReplaceModelError("GeneralError", "");
            ModelState.NullFieldsToValidEmpty(classForm);


            // بررسی جی یو آی دی گرفته شده از روت آی دی
            var routesection = await _db.Sections.FirstOrDefaultAsync(s => s.Id == id);
            if (routesection == null || routesection.ClassroomId == null)
                return NotFound();
            var classroom = await _db.Classrooms.FirstOrDefaultAsync(c => c.Id == routesection.ClassroomId);
            if (classroom == null)
                return NotFound();

            // بررسی جی یو آی دی های گرفته شده از فرم

            var classExists = await _db.Classrooms.AnyAsync(cr => cr.Id == classForm.ClassLocationId);
            if (!classExists)
            {
                ModelState.AppendModelError("GeneralError", "محل برگزاری انتخاب شده یافت نشد!");
            }
            var section = await _db.Sections.FirstOrDefaultAsync(s => s.Id == classForm.SectionId);
            if (section == null)
            {
                ModelState.AppendModelError("GeneralError", "درس انتخاب شده یافت نشد!");
            }
            if (classForm.StudentIds != null && classForm.StudentIds.Any())
            {
                var existingStudents = await _db.Students
                    .Where(s => classForm.StudentIds.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync();

                var missingStudents = classForm.StudentIds.Except(existingStudents).ToList();
                if (missingStudents.Any())
                {
                    ModelState.AppendModelError("GeneralError", "برخی از دانشجوهای انتخاب شده یافت نشدند!");
                }

                classForm.StudentIds = classForm.StudentIds.Distinct().ToList(); // حذف موارد تکراری لیست

                // بررسی ظرفیت کلاس فقط وقتی کلاس و دانشجوها معتبر بودند
                if (classExists)
                {
                    var capacity = await _db.Classrooms
                        .Where(cr => cr.Id == classForm.ClassLocationId)
                        .Select(cr => cr.Capacity)
                        .FirstOrDefaultAsync();

                    if (classForm.StudentIds.Count > capacity)
                    {
                        ModelState.AppendModelError("StudentIds", "تعداد دانشجویان بیش از ظرفیت کلاس انتخاب شده است");
                    }
                }
            }

            // اعتبارسنجی فیلدها
            foreach (var prop in typeof(ClassFormViewModel).GetProperties())
            {
                ModelState.ValidateField(classForm, prop.Name, true);
            }

            // بررسی تداخل زمانی اگر همه چیز ها مرتب بودند 
            if (ModelState.IsValid)
            {
                var currentSection = await _db.Sections
                    .Where(s => s.Id == classForm.SectionId)
                    .Select(s => new { s.Year, s.Semester, s.TimeSlotId, s.ClassroomId })
                    .FirstOrDefaultAsync();

                if (currentSection != null && classForm.ClassLocationId != Guid.Empty)
                {
                    bool classConflict = await _db.Sections
                        .AnyAsync(s =>
                            s.Year == currentSection.Year &&
                            s.Semester == currentSection.Semester &&
                            s.TimeSlotId == currentSection.TimeSlotId &&
                            s.ClassroomId == classForm.ClassLocationId &&
                            s.Id != id // با سکشنی که داریم ادیت میکنیم تداخل نگیره
                        );

                    if (classConflict)
                        ModelState.AppendModelError("GeneralError", "در این زمان و مکان، کلاس دیگری قبلاً ثبت شده است.");
                }
            }

            // برگشت در صورت خطا
            if (!ModelState.IsValid)
            {
                ViewData["Classroom"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                ViewData["Sections"] = GetUnassignedSectionList(id);
                ViewData["Students"] = _db.Students.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToList();
                ViewData["Form"] = classForm;
                return View("~/Views/AdminDashboard/ClassManagement/EditClass.cshtml", admin);
            }

            // ذخیره‌سازی
            try
            {
                // آزادسازی سکشن قبلی از کلاس
                var oldSection = await _db.Sections.FirstOrDefaultAsync(s => s.Id == id);
                if (oldSection != null)
                {
                    oldSection.ClassroomId = null;
                    _db.Sections.Update(oldSection);
                }

                // اختصاص کلاس جدید
                section.ClassroomId = classForm.ClassLocationId;
                _db.Sections.Update(section);

                // حذف Takes قبلی
                var oldTakes = _db.Takes.Where(t => t.SectionId == section.Id);
                _db.Takes.RemoveRange(oldTakes);

                // افزودن Takes جدید
                foreach (var studentId in classForm.StudentIds)
                {
                    _db.Takes.Add(new Takes
                    {
                        StudentUserId = studentId,
                        SectionId = section.Id,
                        Grade = ""
                    });
                }

                await _db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AppendModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["Classroom"] = _db.Classrooms.OrderBy(cr => cr.Building).ThenBy(cr => cr.RoomNumber).ThenBy(cr => cr.Capacity).ToList();
                ViewData["Sections"] = GetUnassignedSectionList(id);
                ViewData["Students"] = _db.Students.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToList();
                ViewData["Form"] = classForm;

                return View("~/Views/AdminDashboard/ClassManagement/EditClass.cshtml", admin);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteClass(Guid id)
        {
            var admin = CurrentAdmin;
            ModelState.ReplaceModelError("GeneralError", "");

            // بررسی جی یو آی دی گرفته شده از روت آی دی
            var section = await _db.Sections
                .Include(s => s.Classroom)
                .Include(s => s.Takes)
                .Include(s => s.Course)
                    .ThenInclude(c => c.Sections)
                        .ThenInclude(sec => sec.Teaches)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (section == null || section.ClassroomId == null)
                return NotFound();
            var classroom = await _db.Classrooms.FirstOrDefaultAsync(c => c.Id == section.ClassroomId);
            if (classroom == null)
                return NotFound();

            var form = new ClassFormViewModel
            {
                SectionId = section.Id,
                ClassLocationId = section.ClassroomId.Value,
                StudentIds = await _db.Takes
                    .Where(t => t.SectionId == id)
                    .Select(t => t.StudentUserId)
                    .ToListAsync()
            };
            ViewData["Form"] = form;
            ViewData["selectedClassroom"] = _db.Classrooms.ToList().FirstOrDefault(cr => cr.Id == section.ClassroomId.Value);
            ViewData["selectedSection"] = GetUnassignedSectionList(id).ToList().FirstOrDefault(s => s.SectionId == section.Id);
            ViewData["Students"] = _db.Students.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToList();

            return View("~/Views/AdminDashboard/ClassManagement/DeleteClass.cshtml", admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClass(Guid id, ClassFormViewModel model) // ویو مدل فرم صرفا جهت متفاوت بودن پارامتر ها اکشن پست با گت اش گرفته شده و از آن هیچ استفاده ای نشده
        {
            var admin = CurrentAdmin;
            ModelState.ReplaceModelError("GeneralError", "");

            // بررسی جی یو آی دی گرفته شده از روت آی دی
            var section = await _db.Sections
                .Include(s => s.Classroom)
                .Include(s => s.Takes)
                .Include(s => s.Course)
                    .ThenInclude(c => c.Sections)
                        .ThenInclude(sec => sec.Teaches)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (section == null || section.ClassroomId == null)
                return NotFound();
            var classroom = await _db.Classrooms.FirstOrDefaultAsync(c => c.Id == section.ClassroomId);
            if (classroom == null)
                return NotFound();
                
            try
            {
                // حذف همه تیکز های این سکشن
                _db.Takes.RemoveRange(section.Takes);

                if (section.CourseId != null)
                { // درس حذف ایمن نشده و موجود هست فقط کلاس روم آی دی رو نال کن
                    section.ClassroomId = null;
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Index", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
                }
                else
                { // درس حذف ایمن شده و ای دی اش نال شده برای حذف تخصیص کلاس سکشن را پاک کن

                    _db.Sections.Remove(section);
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Index", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
                }
            }
            catch
            {
                ModelState.AppendModelError("GeneralError", "خطایی هنگام حذف اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");

                var form = new ClassFormViewModel
                {
                    SectionId = section.Id,
                    ClassLocationId = section.ClassroomId.Value,
                    StudentIds = await _db.Takes
                        .Where(t => t.SectionId == id)
                        .Select(t => t.StudentUserId)
                        .ToListAsync()
                };
                ViewData["Form"] = form;
                ViewData["selectedClassroom"] = _db.Classrooms.ToList().FirstOrDefault(cr => cr.Id == section.ClassroomId.Value);
                ViewData["selectedSection"] = GetUnassignedSectionList(id).ToList().FirstOrDefault(s => s.SectionId == section.Id);
                ViewData["Students"] = _db.Students.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToList();

                return View("~/Views/AdminDashboard/ClassManagement/DeleteClass.cshtml", admin);
            }
        }
    }
}

