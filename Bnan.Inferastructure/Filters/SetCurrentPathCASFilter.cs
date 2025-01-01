using Microsoft.AspNetCore.Mvc.Filters;

namespace Bnan.Inferastructure.Filters
{
    public class SetCurrentPathCASFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // احصل على المسار الحالي
            var currentPath = context.HttpContext.Request.Path.Value;

            // حفظه في HttpContext.Items لكي يكون متاحًا في الـ View
            context.HttpContext.Items["CurrentPath"] = currentPath;
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
