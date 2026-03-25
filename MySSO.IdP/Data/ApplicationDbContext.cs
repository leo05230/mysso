using Microsoft.EntityFrameworkCore;

namespace MySSO.IdP.Data
{
    // 確保這裡有繼承 DbContext
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 重要：這行會告訴 EF Core 建立 OpenIddict 所需的所有資料表結構
            // 包含：Applications, Authorizations, Scopes, Tokens
            modelBuilder.UseOpenIddict();
        }
    }
}