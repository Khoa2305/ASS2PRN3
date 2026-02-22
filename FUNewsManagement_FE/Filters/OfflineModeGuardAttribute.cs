using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FUNewsManagement_FE.Services;

namespace FUNewsManagement_FE.Filters
{
    public class OfflineModeGuardAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var offlineService = context.HttpContext.RequestServices.GetService<IOfflineStateService>();

            if (offlineService != null && offlineService.IsOffline)
            {
                var method = context.HttpContext.Request.Method;
                
                // Block any POST, PUT, DELETE, PATCH requests
                if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                    method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                    method.Equals("DELETE", StringComparison.OrdinalIgnoreCase) ||
                    method.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
                {
                    context.Result = new BadRequestObjectResult(new 
                    { 
                        success = false, 
                        message = "Application is currently in Offline Mode. Creating, updating, or deleting data is disabled." 
                    });
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
