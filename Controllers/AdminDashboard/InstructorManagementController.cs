using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using Microsoft.AspNetCore.Http;
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

            // Fetch instructors and related user data, ordered by LastName then FirstName
            var instructors = _db.Instructors
                .OrderBy(i => i.LastName)
                .ThenBy(i => i.FirstName)
                .ToList();

            ViewBag.Instructors = instructors;
            ViewData["ActiveTab"] = "Instructor";
            return View("~/Views/AdminDashboard/Instructor.cshtml", admin);
        }
    }
}