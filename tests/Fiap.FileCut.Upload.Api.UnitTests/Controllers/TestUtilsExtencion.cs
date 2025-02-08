using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fiap.FileCut.Upload.Api.UnitTests.Controllers;

internal static class TestUtilsExtencion
{
    public static void SetUserAuth(this ControllerBase controller, Guid userId, string email)
    {
        var claims = new List<Claim>
        {
            new ("preferred_username", email),
            new (ClaimTypes.NameIdentifier, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }
}