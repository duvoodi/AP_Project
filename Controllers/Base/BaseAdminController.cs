using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AP_Project.Data;
using AP_Project.Models.Users;

public class BaseAdminController : Controller
{
    protected readonly AppDbContext _db;
    protected Admin CurrentAdmin { get; private set; }

    public BaseAdminController(AppDbContext db)
    {
        _db = db;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        var adminId = HttpContext.Session.GetInt32("AdminId");
        if (adminId == null)
        {
            context.Result = RedirectToAction("Index", "Login");
            return;
        }

        CurrentAdmin = _db.Set<Admin>().FirstOrDefault(a => a.AdminId == adminId.Value);
        if (CurrentAdmin == null)
        {
            context.Result = RedirectToAction("Index", "Login");
            return;
        }
    }
}
