using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MySSO.IdP.Controllers;

public class AuthorizationController : Controller
{
    // 關鍵：這裡會要求必須有 Cookie 才能進入，否則會自動跳轉到 Account/Login
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("發送了無效的 OIDC 請求。");

        // 1. 從 HttpContext 中取得由 AccountController 建立的使用者身分
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 2. 建立 OpenIddict 需要的 ClaimsIdentity
        // 讀取你在 AccountController 設定的 NameIdentifier (user-123)
        var claims = new List<Claim>
        {
            new Claim(Claims.Subject, result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown"),
            new Claim(Claims.Name, result.Principal.Identity?.Name ?? "unknown"),
            new Claim(Claims.Email, "leo05230@gmail.com")
        };

        var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // 3. 設定 Token 允許的 Scope
        claimsPrincipal.SetScopes(request.GetScopes());

        // 4. 設定 Claim 的目的地（確保 Client 端能讀取到這些資訊）
        foreach (var claim in claimsPrincipal.Claims)
        {
            claim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
        }

        // 回傳 SignIn 結果，OpenIddict 會自動生成 Authorization Code 並跳回 Client 端
        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    // 必須加入此端點，供 Client 端拿 Code 交換 Token
    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("發送了無效的 OIDC 請求。");

        if (request.IsAuthorizationCodeGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("不支援的 Grant Type。");
    }
}