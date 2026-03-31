using Microsoft.EntityFrameworkCore;
using MySSO.IdP.Data;
using MySSO.IdP.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// --- 加入以下區塊 ---
// 從 appsettings.json 取得連線字串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy.WithOrigins("http://localhost:44305") // 換成你前端的網址
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // 配置 PostgreSQL 驅動
    options.UseNpgsql(connectionString);

    // 告訴 OpenIddict 使用這個 DbContext
    options.UseOpenIddict();
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenIddict()
    .AddCore(options => {
        options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options => {
        options.SetAuthorizationEndpointUris("/connect/authorize")
               .SetTokenEndpointUris("/connect/token")
               .SetEndSessionEndpointUris("/connect/logout");

        // --- 加入這行：註冊伺服器支援的權限範圍 ---
        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);
        options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange(); // 強制使用 PKCE (大型專案標配)

        // GCP 部署關鍵：在正式環境應從 Cloud KMS 取得憑證
        if (builder.Environment.IsDevelopment())
        {
            options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
        }
        else
        {
            // 正式環境應載入由 KMS 保護的 X.509 憑證
            // options.AddSigningCertificate(myCert); 
        }

        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough();
    });
// 註冊初始化 Worker
builder.Services.AddHostedService<Worker>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
