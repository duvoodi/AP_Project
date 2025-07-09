using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

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
    }
}