using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// MyWebApp.Client/Program.cs 關鍵設定確認
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    // 必須指向你的 IdP 網址 (確認與 launchSettings.json 一致)
    options.Authority = "https://localhost:44350/";

    // 與 Worker.cs 註冊的 ClientId 一致
    options.ClientId = "my-web-app";
    options.ClientSecret = "846B62D0-DEF9-4215-A99D-86E6B0D1B0E6";

    options.ResponseType = "code";
    options.SaveTokens = true;

    // 必須與 Worker.cs 中的 RedirectUris 一致
    options.CallbackPath = "/signin-oidc";
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
