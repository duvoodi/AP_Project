using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using System.Globalization;
using AP_Project.Helpers.FormUtils;
using Microsoft.EntityFrameworkCore;
using AP_Project.FormViewModels.CourseForm;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AP_Project.Models.Courses;
using System.Linq;
using AP_Project.Helpers;
using AP_Project.FormViewModels;

namespace AP_Project.Controllers
{
    public class CourseManagementController : BaseAdminController
    {
        public CourseManagementController(AppDbContext db) : base(db)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            var admin = CurrentAdmin;

            // بارگذاری سکشن‌هایی که کورس دارند
            var sections = _db.Sections
                .Where(s => s.Course != null)
                .Include(s => s.Course)
                    .ThenInclude(c => c.CourseCode)
                .Include(s => s.Teaches)
                    .ThenInclude(t => t.Instructor)
                .Include(s => s.TimeSlot)
                .Include(s => s.Course.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteCourseCode)
                .OrderBy(s => s.Course.CourseCode.Code)                            // 1. کد درس
                .ThenBy(s => s.Course.Unit)                                        // 2. تعداد واحد
                .ThenBy(s => s.Year * 10 + s.Semester)                             // 3. نیم‌سال به صورت Year*10 + Semester
                .ThenBy(s =>                                                       // 4. اسم استاد یا رشته خالی
                    s.Teaches != null && s.Teaches.Instructor != null
                    ? s.Teaches.Instructor.FirstName
                    : string.Empty
                )
                .ThenBy(s =>                                                       // 5. فامیلی استاد یا رشته خالی
                    s.Teaches != null && s.Teaches.Instructor != null
                    ? s.Teaches.Instructor.LastName
                    : string.Empty
                )
                .ThenBy(s => s.TimeSlotId)                                         // 6. شناسه‌ی تایم‌اسلات
                .ToList();

            ViewBag.Sections = sections;
            return View("~/Views/AdminDashboard/CourseManagement/Index.cshtml", admin);
        }

        [HttpGet]
        public IActionResult AddCourse()
        {
            var admin = CurrentAdmin;
        
            ViewData["Instructors"] = _db.Instructors
                .OrderBy(i => i.LastName)
                .ThenBy(i => i.FirstName)
                .ToList();

            ViewData["TimeSlots"] = _db.TimeSlots
                .OrderBy(ts => ts.Id)
                .ToList();

            ViewData["CourseCodes"] = _db.CourseCodes
                .OrderBy(cc => cc.Code)
                .ToList();

            return View("~/Views/AdminDashboard/CourseManagement/AddCourse.cshtml", admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(CourseFormViewModel courseForm)
        {
            var admin = CurrentAdmin;
            ModelState.ReplaceModelError("GeneralError", "");
            ModelState.NullFieldsToValidEmpty(courseForm);

            // بررسی جی یو آی دی های گرفته شده از فرم

            var courseCodeExists = await _db.CourseCodes.AnyAsync(cc => cc.Id == courseForm.CourseCodeId);
            if (!courseCodeExists)
            {
                ModelState.AppendModelError("GeneralError", "کد درس انتخاب شده یافت نشد!");
            }

            if (courseForm.InstructorId.HasValue) // تخصیص استاد دلخواه هست اگر انتخاب کرده بود چک
            {
                var instructorExists = await _db.Instructors.AnyAsync(i => i.Id == courseForm.InstructorId.Value);
                if (!instructorExists)
                {
                    ModelState.AppendModelError("GeneralError", "استاد انتخاب شده یافت نشد!");
                }
            }

            var timeSlotExists = await _db.TimeSlots.AnyAsync(ts => ts.Id == courseForm.TimeSlotId);
            if (!timeSlotExists)
            {
                ModelState.AppendModelError("GeneralError", "زمان برگزاری انتخاب شده یافت نشد!");
            }


            if (courseForm.PrerequisiteIds != null && courseForm.PrerequisiteIds.Any())
            {
                var existingPrerequisites = await _db.CourseCodes
                    .Where(cc => courseForm.PrerequisiteIds.Contains(cc.Id))
                    .Select(cc => cc.Id)
                    .ToListAsync();

                var missingPrerequisites = courseForm.PrerequisiteIds.Except(existingPrerequisites).ToList();
                if (missingPrerequisites.Any())
                {
                    ModelState.AppendModelError("GeneralError", "برخی از دروس پیش‌نیاز انتخاب شده یافت نشدند!");
                }

                courseForm.PrerequisiteIds = courseForm.PrerequisiteIds.Distinct().ToList(); // حذف موارد تکراری لیست
                    
                // جلوگیری از پیش‌نیاز بودن خودش
                if (courseForm.PrerequisiteIds.Contains(courseForm.CourseCodeId))
                {
                    ModelState.AppendModelError("PrerequisiteIds", "کد درس نمی‌تواند پیش‌نیاز خود باشد");
                }
            }

            // بررسی سایر ولیدیشن‌های فیلدها
            foreach (var prop in typeof(CourseFormViewModel).GetProperties())
            {
                ModelState.ValidateField(courseForm, prop.Name, true);
            }

            // بررسی تداخل زمانی و تکراری بودن اگر همه چیز ها معتبر بودن
            if (ModelState.IsValid)
            {
                var semesterStr = courseForm.Semester;

                if (semesterStr.Length == 5 &&
                    int.TryParse(semesterStr.Substring(0, 4), out int year) &&
                    int.TryParse(semesterStr.Substring(4, 1), out int semester))
                {
                    var timeSlotId = courseForm.TimeSlotId;

                    // چک تداخل برای استاد
                    if (courseForm.InstructorId.HasValue)
                    {
                        bool instructorConflict = await _db.Teaches
                            .Include(t => t.Section)
                            .AnyAsync(t =>
                                t.Section.Year == year &&
                                t.Section.Semester == semester &&
                                t.Section.TimeSlotId == timeSlotId &&
                                t.InstructorUserId == courseForm.InstructorId.Value
                            );

                        if (instructorConflict)
                        {
                            ModelState.AppendModelError("GeneralError", "این استاد در همین نیم‌سال و زمان، کلاس دیگری دارد.");
                        }
                    }

                    // چک وجود سکشن تکراری با مشخصات مشابه
                    int courseUnit = int.Parse(courseForm.Unit);

                    // ۱) فیلتر اولیه روی شرایط ساده
                    var candidateSections = await _db.Sections
                        .Include(s => s.Course)
                            .ThenInclude(c => c.Prerequisites)
                        .Where(s =>
                            s.Course.CodeId == courseForm.CourseCodeId &&
                            s.Course.Unit == courseUnit &&
                            s.Year == year &&
                            s.Semester == semester &&
                            s.TimeSlotId == timeSlotId
                        )
                        .ToListAsync();

                    // آماده‌سازی لیست مرتب پیش‌نیاز فرم
                    var formPrereqs = (courseForm.PrerequisiteIds ?? new List<Guid>())
                        .OrderBy(id => id)
                        .ToList();

                    // ۲) مقایسهٔ لیست پیش‌نیازها به‌صورت محلی
                    bool duplicateSectionExists = candidateSections.Any(s =>
                    {
                        var sectionPrereqs = s.Course.Prerequisites
                            .Select(p => p.PrerequisiteCourseCodeId)
                            .OrderBy(id => id)
                            .ToList();
                        return sectionPrereqs.SequenceEqual(formPrereqs);
                    });


                    if (duplicateSectionExists)
                    {
                        ModelState.AppendModelError("GeneralError", "درسی با همین کد درس، تعداد واحد، پیش‌نیازها، نیم‌سال و زمان برگزاری قبلاً ثبت شده است.");
                    }
                }
                else
                {
                    ModelState.AppendModelError("Semester", "فرمت نیم‌سال وارد شده نامعتبر است.");
                }
            }
            // برگشت در صورت خطا
            if (!ModelState.IsValid)
            {
                ViewData["Instructors"] = _db.Instructors.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToList();
                ViewData["TimeSlots"] = _db.TimeSlots.OrderBy(ts => ts.Id).ToList();
                ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                ViewData["Form"] = courseForm;
                return View("~/Views/AdminDashboard/CourseManagement/AddCourse.cshtml", admin);
            }

            // ذخیره‌سازی
            try
            {
                // ساخت Course
                var course = new Course
                {
                    Id = Guid.NewGuid(), // ایجاد تا بشه به سکشن وصل کرد
                    CodeId = courseForm.CourseCodeId,
                    Unit = int.Parse(courseForm.Unit),
                    Description = courseForm.Description,
                    FinalExamDate = DateTime.ParseExact(
                        courseForm.FinalExamDate, 
                        "yyyy/MM/dd", 
                        new CultureInfo("fa-IR") { DateTimeFormat = { Calendar = new PersianCalendar() } },
                        DateTimeStyles.None)
                };
                _db.Courses.Add(course);

                // ساخت Section مرتبط
                int year = int.Parse(courseForm.Semester.Substring(0, 4));
                int semester = int.Parse(courseForm.Semester.Substring(4, 1));
                var section = new Section
                {
                    Id = Guid.NewGuid(), // ایجاد تا بشه به تیکز وصل کرد
                    CourseId = course.Id,
                    Semester = semester,
                    Year = year,
                    ClassroomId = null,
                    TimeSlotId = courseForm.TimeSlotId
                };
                _db.Sections.Add(section);

                // Teaches (در صورت انتخاب استاد)
                if (courseForm.InstructorId.HasValue)
                {
                    _db.Teaches.Add(new Teaches
                    {
                        SectionId = section.Id,
                        InstructorUserId = courseForm.InstructorId.Value
                    });
                }

                // افزودن پیش‌نیازها به Course اصلی
                foreach (var pid in courseForm.PrerequisiteIds)
                {
                    _db.Prerequisites.Add(new Prerequisite
                    {
                        CourseId = course.Id,
                        PrerequisiteCourseCodeId = pid
                    });
                }

                // ذخیرهٔ نهایی همه تغییرات
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch (Exception)
            {
                ModelState.AppendModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["Instructors"] = _db.Instructors.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToList();
                ViewData["TimeSlots"] = _db.TimeSlots.OrderBy(ts => ts.Id).ToList();
                ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                ViewData["Form"] = courseForm;
                return View("~/Views/AdminDashboard/CourseManagement/AddCourse.cshtml", admin);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(Guid id)
        {
            var admin = CurrentAdmin;

            // بررسی جی یو آی دی گرفته شده از روت آی دی
            var section = await _db.Sections
                .Include(s => s.Course)
                .ThenInclude(c => c.Prerequisites)
                .Include(s => s.Teaches)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (section == null || section.CourseId == null)
                return NotFound();
            var Course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == section.CourseId);
            if (Course == null)
                return NotFound();


            // داده‌های کمکی برای دراپ‌داون‌ها
            var pc = new PersianCalendar();
            ViewData["Instructors"] = _db.Instructors.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToList();
            ViewData["TimeSlots"] = _db.TimeSlots.OrderBy(ts => ts.Id).ToList();
            ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
            var course = section.Course;
            var model = new CourseFormViewModel
            {
                CourseCodeId = course.CodeId,
                Unit = course.Unit.ToString(),
                Description = course.Description,
                FinalExamDate = course.FinalExamDate.ToString("yyyy/MM/dd", new CultureInfo("fa-IR")),
                Semester = $"{section.Year}{section.Semester}",
                TimeSlotId = section.TimeSlotId.Value,
                InstructorId = section.Teaches?.InstructorUserId,
                PrerequisiteIds = course.Prerequisites.Select(p => p.PrerequisiteCourseCodeId).ToList()
            };
            ViewData["Form"] = model;

            return View("~/Views/AdminDashboard/CourseManagement/EditCourse.cshtml", admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(Guid id, CourseFormViewModel courseForm)
        {
            var admin = CurrentAdmin;
            ModelState.ReplaceModelError("GeneralError", "");
            ModelState.NullFieldsToValidEmpty(courseForm);

            // بررسی جی یو آی دی گرفته شده از روت آی دی
            var section = await _db.Sections
                .Include(s => s.Course)
                .ThenInclude(c => c.Prerequisites)
                .Include(s => s.Teaches)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (section == null || section.CourseId == null)
                return NotFound();
            var Course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == section.CourseId);
            if (Course == null)
                return NotFound();

            // بررسی جی یو آی دی های گرفته شده از فرم

            // بررسی وجود CourseCode
            var courseCodeExists = await _db.CourseCodes.AnyAsync(cc => cc.Id == courseForm.CourseCodeId);
            if (!courseCodeExists)
                ModelState.AppendModelError("GeneralError", "کد درس انتخاب شده یافت نشد!");

            // بررسی وجود Instructor در صورت انتخاب
            if (courseForm.InstructorId.HasValue)
            {
                var instructorExists = await _db.Instructors.AnyAsync(i => i.Id == courseForm.InstructorId.Value);
                if (!instructorExists)
                    ModelState.AppendModelError("GeneralError", "استاد انتخاب شده یافت نشد!");
            }

            // بررسی وجود TimeSlot
            var timeSlotExists = await _db.TimeSlots.AnyAsync(ts => ts.Id == courseForm.TimeSlotId);
            if (!timeSlotExists)
                ModelState.AppendModelError("GeneralError", "زمان برگزاری انتخاب شده یافت نشد!");

            // بررسی پیش‌نیازها
            if (courseForm.PrerequisiteIds != null && courseForm.PrerequisiteIds.Any())
            {
                var existingPrerequisites = await _db.CourseCodes
                    .Where(cc => courseForm.PrerequisiteIds.Contains(cc.Id))
                    .Select(cc => cc.Id)
                    .ToListAsync();

                var missingPrerequisites = courseForm.PrerequisiteIds.Except(existingPrerequisites).ToList();
                if (missingPrerequisites.Any())
                    ModelState.AppendModelError("GeneralError", "برخی از دروس پیش‌نیاز انتخاب شده یافت نشدند!");

                courseForm.PrerequisiteIds = courseForm.PrerequisiteIds.Distinct().ToList(); // حذف موارد تکراری لیست

                // جلوگیری از پیش‌نیاز بودن خودش
                if (courseForm.PrerequisiteIds.Contains(courseForm.CourseCodeId))
                {
                    ModelState.AppendModelError("PrerequisiteIds", "کد درس نمی‌تواند پیش‌نیاز خود باشد");
                }
            }

            // اعتبارسنجی فیلدها
            foreach (var prop in typeof(CourseFormViewModel).GetProperties())
                ModelState.ValidateField(courseForm, prop.Name, true);

            // بررسی تداخل زمانی و تکراری نبودن 
            if (ModelState.IsValid)
            {
                if (courseForm.Semester.Length == 5 &&
                    int.TryParse(courseForm.Semester.Substring(0, 4), out int year) &&
                    int.TryParse(courseForm.Semester.Substring(4, 1), out int semester))
                {
                    if (courseForm.InstructorId.HasValue)
                    {
                        bool instructorConflict = await _db.Teaches
                            .Include(t => t.Section)
                            .AnyAsync(t =>
                                t.Section.Year == year &&
                                t.Section.Semester == semester &&
                                t.Section.TimeSlotId == courseForm.TimeSlotId &&
                                t.InstructorUserId == courseForm.InstructorId.Value &&
                                t.SectionId != id // با سکشنی که داریم ادیت میکنیم تداخل نگیره
                            );
                        if (instructorConflict)
                            ModelState.AppendModelError("GeneralError", "این استاد در همین نیم‌سال و زمان، کلاس دیگری دارد.");
                    }

                // بررسی تکراری نبودن Section
                var existing = _db.Sections
                    .Include(s => s.Course).ThenInclude(c => c.Prerequisites)
                    .Where(s =>
                        s.Course.CodeId == courseForm.CourseCodeId &&
                        s.Course.Unit == int.Parse(courseForm.Unit) &&
                        s.Year == year &&
                        s.Semester == semester &&
                        s.TimeSlotId == courseForm.TimeSlotId &&
                        s.Id != id &&  // با خودش تکراری نگیرد
                        s.Course.Prerequisites != null
                    )
                    .AsEnumerable() // ← از اینجا به بعد عملیات در حافظه انجام می‌شود
                    .Any(s =>
                        s.Course.Prerequisites
                            .Select(p => p.PrerequisiteCourseCodeId)
                            .OrderBy(x => x)
                            .SequenceEqual(courseForm.PrerequisiteIds.OrderBy(x => x))
                    );

                if (existing)
                    ModelState.AppendModelError("GeneralError", "درسی با همین مشخصات قبلاً ثبت شده است.");
                }
                else
                {
                    ModelState.AppendModelError("Semester", "فرمت نیم‌سال وارد شده نامعتبر است.");
                }
            }

            // بازگشت در صورت خطا
            if (!ModelState.IsValid)
            {
                ViewData["Instructors"] = _db.Instructors.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToList();
                ViewData["TimeSlots"] = _db.TimeSlots.OrderBy(ts => ts.Id).ToList();
                ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                ViewData["Form"] = courseForm;
                return View("~/Views/AdminDashboard/CourseManagement/EditCourse.cshtml", admin);
            }

            // ذخیره‌سازی
            try
            {
                var course = section.Course;
                course.CodeId = courseForm.CourseCodeId;
                course.Unit = int.Parse(courseForm.Unit);
                course.Description = courseForm.Description;
                course.FinalExamDate = DateTime.ParseExact(
                    courseForm.FinalExamDate,
                    "yyyy/MM/dd",
                    new CultureInfo("fa-IR") { DateTimeFormat = { Calendar = new PersianCalendar() } },
                    DateTimeStyles.None);

                _db.Courses.Update(course);

                // به‌روزرسانی Section
                section.TimeSlotId = courseForm.TimeSlotId;
                _db.Sections.Update(section);

                // به‌روزرسانی Teaches
                _db.Teaches.RemoveRange(section.Teaches);
                if (courseForm.InstructorId.HasValue)
                {
                    _db.Teaches.Add(new Teaches
                    {
                        SectionId = section.Id,
                        InstructorUserId  = courseForm.InstructorId.Value
                    });
                }

                // به‌روزرسانی Prerequisites
                _db.Prerequisites.RemoveRange(course.Prerequisites);
                foreach (var pid in courseForm.PrerequisiteIds)
                {
                    _db.Prerequisites.Add(new Prerequisite
                    {
                        CourseId = course.Id,
                        PrerequisiteCourseCodeId = pid
                    });
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { h = ComputeHash.Sha1(admin.AdminId.ToString()) });
            }
            catch
            {
                ModelState.AppendModelError("GeneralError", "خطایی هنگام ذخیره اطلاعات رخ داد! لطفاً دوباره تلاش کنید...");
                ViewData["Instructors"] = _db.Instructors.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToList();
                ViewData["TimeSlots"] = _db.TimeSlots.OrderBy(ts => ts.Id).ToList();
                ViewData["CourseCodes"] = _db.CourseCodes.OrderBy(cc => cc.Code).ToList();
                ViewData["Form"] = courseForm;
                return View("~/Views/AdminDashboard/CourseManagement/EditCourse.cshtml", admin);
            }
        }
    }
}