using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaoVatWeb.Models;

namespace RaoVatWeb.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "User", "Vip" };

            foreach (var role in roles)
            {
                var roleExists = await roleManager.RoleExistsAsync(role);

                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = configuration["AdminAccount:Email"] ?? "admin@raovat.com";
            var adminPassword = configuration["AdminAccount:Password"] ?? "Admin@123456";
            var adminFullName = configuration["AdminAccount:FullName"] ?? "Administrator";

            await CreateUserIfNotExists(
                userManager,
                adminEmail,
                adminPassword,
                adminFullName,
                "Admin",
                isVip: false
            );

            await CreateUserIfNotExists(
                userManager,
                "vip@raovat.com",
                "Vip@123456",
                "Thành viên VIP Demo",
                "Vip",
                isVip: true
            );
        }
private static async Task SeedAreasAsync(ApplicationDbContext context)
{
    if (!await context.Areas.AnyAsync(a => a.Name == "Miền Nam"))
    {
        context.Areas.Add(new Area
        {
            Name = "Miền Nam",
            ParentAreaId = null,
            IsActive = true,
            CreatedAt = DateTime.Now
        });
    }

    if (!await context.Areas.AnyAsync(a => a.Name == "Miền Bắc"))
    {
        context.Areas.Add(new Area
        {
            Name = "Miền Bắc",
            ParentAreaId = null,
            IsActive = true,
            CreatedAt = DateTime.Now
        });
    }

    await SeedAreasAsync(context);
}
        private static async Task CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string roleName,
            bool isVip)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true,
                    IsVip = isVip,
                    VipExpiredAt = isVip ? DateTime.Now.AddMonths(1) : null,
                    IsLocked = false,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(user, roleName))
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
            }
        }
    }
}