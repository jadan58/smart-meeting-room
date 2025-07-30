using Microsoft.AspNetCore.Identity;
using SmartMeetingRoomAPI.Models;
namespace SmartMeetingRoomAPI.Seeders
{
    public static class Seeder
    {
        public static async Task SeedTestUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            // Create roles if they don't exist
            var roles = new[] { "Admin", "Employee", "Guest" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }

            // Create a test admin user
            var testUser = await userManager.FindByEmailAsync("admin@example.com");
            if (testUser == null)
            {
                testUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FirstName = "Test",
                    LastName = "Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(testUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testUser, "Admin");
                }
            }
        }
    }

}