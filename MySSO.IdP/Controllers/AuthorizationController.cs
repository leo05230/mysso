using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MySSO.IdP.Controllers;

public class AuthorizationController : Controller
{
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("發送了無效的 OIDC 請求。");

        // 這裡簡化流程：直接假設使用者已登入 (實際專案應跳轉到 Login 頁面)
        var claims = new List<Claim>
        {
            new Claim(Claims.Subject, "user-123"),
            new Claim(Claims.Name, "Leo Chen"),
            new Claim(Claims.Email, "leo05230@gmail.com")
        };

        var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // 設定 Token 允許的 Scope
        claimsPrincipal.SetScopes(request.GetScopes());

        // 回傳 SignIn 結果，OpenIddict 會自動處理後續的 Code 與 Redirect
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}