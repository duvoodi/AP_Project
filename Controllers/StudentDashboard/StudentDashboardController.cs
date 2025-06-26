using Microsoft.AspNetCore.Mvc;
using AP_Project.Data;
using AP_Project.Models.Users;

namespace AP_Project.Controllers
{
    public class StudentDashboardController : Controller { public IActionResult Index() => Content("Welcome Student"); }
}
