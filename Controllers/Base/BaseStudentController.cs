using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AP_Project.Data;
using AP_Project.Models.Users;

public class BaseStudentController : Controller
{
    protected readonly AppDbContext _db;
    protected Student CurrentStudent { get; private set; }

    public BaseStudentController(AppDbContext db)
    {
        _db = db;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        var studentId = HttpContext.Session.GetInt32("StudentId");
        if (studentId == null)
        {
            context.Result = RedirectToAction("Index", "Login");
            return;
        }

        CurrentStudent = _db.Set<Student>().FirstOrDefault(s => s.StudentId == studentId.Value);
        if (CurrentStudent == null)
        {
            context.Result = RedirectToAction("Index", "Login");
            return;
        }
    }
}
