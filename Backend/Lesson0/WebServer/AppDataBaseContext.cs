using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebServer
{
    public class AppDatabaseContext : IdentityDbContext<IdentityUser>
    {
        public AppDatabaseContext(DbContextOptions<AppDatabaseContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Uncommend if you want to rename standard table names

            builder.Entity<IdentityRole>(entity => entity.ToTable("Role"));

            builder.Entity<IdentityUser<string>>().ToTable("User");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRole");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaim");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserToken");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin");
        }
    }
}
