using System.Text.Json;
using HSMS.MVC.DTOs.Auth;
using Microsoft.AspNetCore.Http;

namespace HSMS.MVC.Common;

public static class SessionKeys
{
    public const string CurrentUser = "CurrentUser";
}

public static class SessionExtensions
{
    public static void SetCurrentUser(this ISession session, LoginResponseDto user)
    {
        session.SetString(SessionKeys.CurrentUser, JsonSerializer.Serialize(user));
    }

    public static LoginResponseDto? GetCurrentUser(this ISession session)
    {
        var json = session.GetString(SessionKeys.CurrentUser);
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<LoginResponseDto>(json);
        }
        catch
        {
            return null;
        }
    }
}

