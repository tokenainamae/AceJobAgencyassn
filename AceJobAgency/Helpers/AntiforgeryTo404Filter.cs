using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AntiforgeryTo404Filter : IAsyncAuthorizationFilter
{
    private readonly IAntiforgery _antiforgery;

    public AntiforgeryTo404Filter(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;
        var method = http.Request.Method;

        if (!HttpMethods.IsPost(method) && !HttpMethods.IsPut(method) && !HttpMethods.IsDelete(method))
            return;

        var path = http.Request.Path.Value ?? string.Empty;

        var sensitivePaths = new[]
        {
            "/Home/ChangePassword",
            "/Account/ResetPassword",
            "/Account/ChangePassword",
            "/Account/ForgotPassword"
        };

        var isSensitive = sensitivePaths.Any(sp =>
            path.StartsWith(sp, StringComparison.OrdinalIgnoreCase));

        if (!isSensitive)
            return;

        try
        {
            await _antiforgery.ValidateRequestAsync(http);
        }
        catch (Exception)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status404NotFound);
        }
    }
}