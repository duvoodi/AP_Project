using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using Microsoft.AspNetCore.Http;

namespace AP_Project.Controllers
{
    public class CourseManagementController : Controller
    {
        private readonly AppDbContext _db;
        public CourseManagementController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Index()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
                return RedirectToAction("Index", "Login");

            var admin = _db.Set<Admin>().FirstOrDefault(a => a.AdminId == adminId.Value);
            if (admin == null)
                return RedirectToAction("Index", "Login");

            return View("~/Views/AdminDashboard/Course.cshtml", admin);
        }
    }
}