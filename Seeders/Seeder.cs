using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartMeetingRoomAPI.Models;
using System;
using System.Threading.Tasks;

namespace SmartMeetingRoomAPI.Seeders
{
    public class Seeder
    {
        public static async Task SeedTestUserAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string testEmail = "testuser@example.com";

            var existingUser = await userManager.FindByEmailAsync(testEmail);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = testEmail,
                    Email = testEmail,
                    FirstName = "Test",
                    LastName = "User"
                };

                var result = await userManager.CreateAsync(user, "Test@1234"); // Use a strong password!

                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                Console.WriteLine($"Seeded user with Id: {user.Id}");
            }
        }

    }
}
