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
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            await dbContext.Database.MigrateAsync();

            string[] roles = { "Admin", "User", "Vip" };

            foreach (var role in roles)
            {
                bool roleExists = await roleManager.RoleExistsAsync(role);

                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            string adminEmail = configuration["AdminAccount:Email"] ?? "admin@raovat.com";
            string adminPassword = configuration["AdminAccount:Password"] ?? "Admin@123456";
            string adminFullName = configuration["AdminAccount:FullName"] ?? "Administrator";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminFullName,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            if (!await dbContext.Categories.AnyAsync())
            {
                dbContext.Categories.AddRange(
                    new Category { Name = "Mua bán sản phẩm", Description = "Các tin mua bán sản phẩm thông thường" },
                    new Category { Name = "Đồ điện tử", Description = "Điện thoại, laptop, thiết bị điện tử" },
                    new Category { Name = "Xe cộ", Description = "Xe máy, ô tô, phụ tùng xe" },
                    new Category { Name = "Đồ gia dụng", Description = "Nội thất, thiết bị gia đình" },
                    new Category { Name = "Thời trang", Description = "Quần áo, giày dép, phụ kiện" },
                    new Category { Name = "Cho thuê", Description = "Tin cho thuê tài sản, phòng trọ, mặt bằng" },
                    new Category { Name = "Dịch vụ", Description = "Tư vấn, sửa chữa, hỗ trợ dịch vụ" },
                    new Category { Name = "Việc làm", Description = "Tuyển dụng hoặc tìm việc" },
                    new Category { Name = "Trao đổi hàng hóa", Description = "Tin trao đổi sản phẩm, hàng hóa" },
                    new Category { Name = "Khác", Description = "Các loại tin rao vặt khác" }
                );

                await dbContext.SaveChangesAsync();
            }

            if (!await dbContext.Areas.AnyAsync())
            {
                dbContext.Areas.AddRange(
                    new Area { Name = "TP. Hồ Chí Minh" },
                    new Area { Name = "Hà Nội" },
                    new Area { Name = "Đà Nẵng" },
                    new Area { Name = "Cần Thơ" },
                    new Area { Name = "Bình Dương" },
                    new Area { Name = "Đồng Nai" }
                );

                await dbContext.SaveChangesAsync();
            }
        }
    }
}