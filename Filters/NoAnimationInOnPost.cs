using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class NoAnimationInOnPostAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var controller = context.Controller as Controller;
        if (controller != null && controller.HttpContext.Request.Method == "POST")
        {
            controller.ViewData["NoAnimationIn"] = true;
        }
    }
}
