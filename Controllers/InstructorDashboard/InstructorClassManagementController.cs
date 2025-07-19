using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AP_Project.Controllers
{
    public class InstructorClassManagementController : BaseInstructorController
    {
        public InstructorClassManagementController(AppDbContext db) : base(db)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            var instructor = CurrentInstructor;

            var classrooms = _db.Classrooms
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Course)
                        .ThenInclude(c => c.CourseCode)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.TimeSlot)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Teaches)
                        .ThenInclude(t => t.Instructor)
                .ToList();

            ViewBag.Classrooms = classrooms;
            return View("~/Views/InstructorDashboard/ClassManagement/Index.cshtml", instructor);
        }
        [HttpGet]
        public async Task<IActionResult> GetCourseInfo(Guid courseId)
        {
            var course = await _db.Courses
                .Include(c => c.CourseCode)
                .Include(c => c.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteCourseCode)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Teaches)
                        .ThenInclude(t => t.Instructor)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.TimeSlot)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound();

            return PartialView("~/Views/InstructorDashboard/ClassManagement/CourseDetailsPopup.cshtml", course);
        }

        [HttpGet]
        public IActionResult EditGrades(Guid id)
        {
            // دریافت سکشن با تمام اطلاعات مرتبط
            var section = _db.Sections
                .Include(s => s.Course)
                    .ThenInclude(c => c.CourseCode)
                .Include(s => s.Takes)
                    .ThenInclude(t => t.Student)
                .Include(s => s.Teaches)
                    .ThenInclude(t => t.Instructor)
                .FirstOrDefault(s => s.Id == id);

            if (section == null)
            {
                return NotFound();
            }

            // آماده‌سازی داده برای ویو
            ViewBag.Section = section;
            ViewBag.Students = section.Takes
                .Select(t => t.Student)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToList();
            ViewBag.Takes = section.Takes.ToList();
            ViewBag.SectionId = section.Id;

            return View("~/Views/InstructorDashboard/ClassManagement/SetGrade.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> EditGrades(List<string> grades, List<Guid> studentUserIds, Guid sectionId, string h)
        {
            if (grades == null || studentUserIds == null || grades.Count != studentUserIds.Count)
                return BadRequest();

            for (int i = 0; i < grades.Count; i++)
            {
                var gradeStr = grades[i]?.Trim();
                var studentId = studentUserIds[i];

                if (!double.TryParse(gradeStr, out var gradeValue))
                    continue;

                // محدودیت نمره بین 0 تا 20
                gradeValue = Math.Max(0, Math.Min(20, gradeValue));

                var take = await _db.Takes
                    .FirstOrDefaultAsync(t => t.StudentUserId == studentId && t.SectionId == sectionId);

                if (take != null)
                {
                    take.Grade = gradeValue.ToString("0.00");
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "InstructorClassManagement", new { h });
        }


        [HttpGet]
        public async Task<IActionResult> GetStudentList(Guid sectionId)
        {
            var students = _db.Takes
                .Where(t => t.SectionId == sectionId)
                .Include(t => t.Student)
                .Select(t => t.Student)
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .ThenBy(s => s.StudentId)
                .ToList();

            return PartialView("~/Views/InstructorDashboard/ClassManagement/StudentListPopup.cshtml", students);
        }

        // GET: نمایش صفحه حذف دانشجو برای یک سِکشِن خاص
        [HttpGet]
        public async Task<IActionResult> DeleteStudents(Guid sectionId, string h)
        {
            var section = await _db.Sections
                .Include(s => s.Course)
                    .ThenInclude(c => c.CourseCode)
                .Include(s => s.Takes)
                    .ThenInclude(t => t.Student)
                .Include(s => s.Teaches)
                    .ThenInclude(t => t.Instructor)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null)
            {
                return NotFound();
            }

            // آماده‌سازی داده‌ها برای View
            ViewBag.Section = section;
            ViewBag.Students = section.Takes
                .Select(t => t.Student)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToList();
            ViewBag.Takes = section.Takes.ToList();
            ViewBag.SectionId = section.Id;
            ViewBag.H = h;

            return View("~/Views/InstructorDashboard/ClassManagement/DeleteStudent.cshtml");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteStudents(Guid sectionId, List<Guid> studentIdsToDelete, string h)
        {
            if (studentIdsToDelete == null || studentIdsToDelete.Count == 0)
            {
                return RedirectToAction("DeleteStudents", new { sectionId, h });
            }

            var takesToRemove = await _db.Takes
                .Where(t => t.SectionId == sectionId && studentIdsToDelete.Contains(t.StudentUserId))
                .ToListAsync();

            _db.Takes.RemoveRange(takesToRemove);

            var studentsToRemove = await _db.Users.OfType<Student>()
                .Where(s => studentIdsToDelete.Contains(s.Id))
                .ToListAsync();

            _db.Users.RemoveRange(studentsToRemove);

            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { h });
        }

        [HttpPost]
        public IActionResult DeleteConfirmed([FromBody] List<Guid> ids)
        {
            var students = _db.Students.Where(s => ids.Contains(s.Id)).ToList();
            _db.Students.RemoveRange(students);
            _db.SaveChanges();
            return Ok();
        }



    }
}