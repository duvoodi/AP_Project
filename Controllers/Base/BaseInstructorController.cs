using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AP_Project.Data;
using AP_Project.Models.Users;

public class BaseInstructorController : Controller
{
    protected readonly AppDbContext _db;
    protected Instructor CurrentInstructor { get; private set; }

    public BaseInstructorController(AppDbContext db)
    {
        _db = db;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        var instructorId = HttpContext.Session.GetInt32("InstructorId");
        if (instructorId == null)
        {
            context.Result = RedirectToAction("Index", "Login");
            return;
        }

        CurrentInstructor = _db.Set<Instructor>().FirstOrDefault(i => i.InstructorId == instructorId.Value);
        if (CurrentInstructor == null)
        {
            context.Result = RedirectToAction("Index", "Login");
            return;
        }
    }
}
