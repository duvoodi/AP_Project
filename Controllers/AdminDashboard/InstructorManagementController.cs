using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using AP_Project.Data;
using AP_Project.Models.Users;

namespace AP_Project.Controllers
{
    public class InstructorManagementController : Controller
    {
        private readonly AppDbContext _db;

        public InstructorManagementController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index(string h)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
                return RedirectToAction("Index", "Login");

            var admin = _db.Admins.FirstOrDefault(a => a.AdminId == adminId.Value);
            if (admin == null)
                return RedirectToAction("Index", "Login");

            var instructors = _db.Instructors
                .OrderBy(i => i.LastName)
                .ThenBy(i => i.FirstName)
                .ToList();

            ViewBag.Instructors = instructors;
            ViewData["ActiveTab"] = "Instructor";
            ViewData["HashedId"] = h;

            return View("~/Views/AdminDashboard/Instructor.cshtml", admin);
        }

        [HttpGet]
        public IActionResult AddInstructor()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
                return RedirectToAction("Index", "Login");

            var admin = _db.Admins.FirstOrDefault(a => a.AdminId == adminId.Value);
            if (admin == null)
                return RedirectToAction("Index", "Login");

            ViewBag.CurrentAdmin = admin;
            ViewData["ActiveTab"] = "Instructor";

            // Generate unique instructor code
            int currentYear = DateTime.Now.Year;
            int instructorCode;
            do
            {
                instructorCode = CodeGenerator.GenerateInstructorCode(currentYear);
            }
            while (_db.Instructors.Any(i => i.InstructorId == instructorCode));

            var newInstructor = new Instructor
            {
                InstructorId = instructorCode
            };

            ViewData["HashedId"] = ComputeHash.Sha1(admin.AdminId.ToString());
            return View("~/Views/AdminDashboard/InstructorManagement/Add-Instructor.cshtml", newInstructor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddInstructor(Instructor instructor, string ConfirmPassword)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
                return RedirectToAction("Index", "Login");

            var admin = _db.Admins.FirstOrDefault(a => a.AdminId == adminId.Value);
            if (admin == null)
                return RedirectToAction("Index", "Login");

            ViewBag.CurrentAdmin = admin;
            ViewData["ActiveTab"] = "Instructor";

            // Password match check
            if (instructor.HashedPassword != ConfirmPassword)
            {
                ModelState.AddModelError("HashedPassword", "رمز عبور و تکرار آن یکسان نیستند");
            }

            if (!ModelState.IsValid)
            {
                // Preserve generated code
                ViewData["HashedId"] = ComputeHash.Sha1(admin.AdminId.ToString());
                return View("~/Views/AdminDashboard/InstructorManagement/Add-Instructor.cshtml", instructor);
            }

            // Ensure unique instructor code (in case model was tampered)
            int currentYear = DateTime.Now.Year;
            int generatedCode;
            do
            {
                generatedCode = CodeGenerator.GenerateInstructorCode(currentYear);
            }
            while (_db.Instructors.Any(i => i.InstructorId == generatedCode));

            instructor.InstructorId = generatedCode;
            instructor.CreatedAt = DateTime.Now;
            instructor.HashedPassword = ComputeHash.Sha1(instructor.HashedPassword);

            _db.Instructors.Add(instructor);
            _db.SaveChanges();

            // Assign user role
            var userRole = new UserRole
            {
                UserId = instructor.Id,
                RoleId = 2
            };
            _db.UserRoles.Add(userRole);
            _db.SaveChanges();

            var hashedId = ComputeHash.Sha1(admin.AdminId.ToString());
            return RedirectToAction("Index", new { h = hashedId });
        }
    }
}
