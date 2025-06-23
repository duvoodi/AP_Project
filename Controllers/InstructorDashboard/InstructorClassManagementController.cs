using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using Microsoft.AspNetCore.Http;

namespace AP_Project.Controllers
{
    public class InstructorClassManagementController : Controller
    {
        private readonly AppDbContext _db;
        public InstructorClassManagementController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Index()
        {
            var instructorId = HttpContext.Session.GetInt32("InstructorId");
            if (instructorId == null)
                return RedirectToAction("Index", "Login");

            var instructor = _db.Set<Instructor>().FirstOrDefault(i => i.InstructorId == instructorId.Value);
            if (instructor == null)
                return RedirectToAction("Index", "Login");

            ViewData["ActiveTab"] = "Class";
            return View("~/Views/InstructorDashboard/Class.cshtml", instructor);
        }
    }
}