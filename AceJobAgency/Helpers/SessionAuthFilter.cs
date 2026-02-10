using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

public class SessionAuthFilter : IActionFilter
{
    private readonly ILogger<SessionAuthFilter> _logger;

    public SessionAuthFilter(ILogger<SessionAuthFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var controller = context.RouteData.Values["controller"]?.ToString() ?? string.Empty;
        var action = context.RouteData.Values["action"]?.ToString() ?? string.Empty;
        _logger.LogDebug("SessionAuthFilter running for {Controller}/{Action}; UserId present: {HasUserId}",
            controller, action, session.GetInt32("UserId") != null);

        if (session.GetInt32("UserId") == null)
        {
            // sensitive controller/action combinations that should return 404 when session is missing
            var sensitive = (controller.Equals("Home", StringComparison.OrdinalIgnoreCase) &&
                             action.Equals("ChangePassword", StringComparison.OrdinalIgnoreCase))
                            ||
                            (controller.Equals("Account", StringComparison.OrdinalIgnoreCase) &&
                             (action.Equals("ResetPassword", StringComparison.OrdinalIgnoreCase) ||
                              action.Equals("ChangePassword", StringComparison.OrdinalIgnoreCase)));

            if (sensitive)
            {
                _logger.LogInformation("Missing session for sensitive endpoint {Controller}/{Action} -> returning 404", controller, action);
                context.Result = new StatusCodeResult(StatusCodes.Status404NotFound);
            }
            else
            {
                _logger.LogInformation("Missing session for {Controller}/{Action} -> redirecting to Login", controller, action);
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}