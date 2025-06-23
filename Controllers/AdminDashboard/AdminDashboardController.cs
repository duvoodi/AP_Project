using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;
using Microsoft.AspNetCore.Http;

namespace AP_Project.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly AppDbContext _db;
        public AdminDashboardController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Index()
        {
            int? adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
                return RedirectToAction("Index", "Login");

            var admin = _db.Set<Admin>()
                .FirstOrDefault(a => a.AdminId == adminId.Value);
            if (admin == null)
                return RedirectToAction("Index", "Login");

            return View(admin);
        }
    }
}