using System.Text.Encodings.Web;
using HSMS.MVC.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace HSMS.MVC.Service;

// Cầu nối giữa Session có sẵn và [Authorize]; không dùng cookie xác thực riêng.
// UseSession phải chạy trước UseAuthentication để đọc được CurrentUser trong mỗi request.
public sealed class SessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ICurrentUserService current) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var user = await current.GetAsync(Context.RequestAborted);
        if (user == null) return AuthenticateResult.NoResult();

        var principal = UserPrincipal.Create(user.UserId, user.Name, user.Roles);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // POST hết phiên trả 401; trang GET chuyển về Login kèm ReturnUrl.
        if (!HttpMethods.IsGet(Request.Method))
            Response.StatusCode = StatusCodes.Status401Unauthorized;
        else
        {
            var returnUrl = Request.PathBase + Request.Path + Request.QueryString;
            Response.Redirect(QueryHelpers.AddQueryString(Request.PathBase + "/Auth/Login", "returnUrl", returnUrl));
        }
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        if (!HttpMethods.IsGet(Request.Method))
            Response.StatusCode = StatusCodes.Status403Forbidden;
        else Response.Redirect(Request.PathBase + "/Auth/AccessDenied");
        return Task.CompletedTask;
    }

}
