using App.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace App.Infrastructure.Data
{
    public static class AppDBInitializer
    {
        public static void SeedSuperAdminUser(UserManager<AppUser> userManager, AppDBContext context)
        {
            if (context.Users.FirstOrDefault(u => u.Email == "Citizen@app.com") == null)
            {
                var defaultUser = new AppUser
                {
                    UserName = "S1234567D",
                    Email = "Citizen@App.com",
                    FullName = "Remon Citizen App",
                    PhoneNumber = "00201285473767",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed=true,
                    CreationDate = DateTime.Now,                    
                };
                userManager.CreateAsync(defaultUser, "456123").Wait();
                userManager.AddToRoleAsync(defaultUser, "Citizen").Wait();
                var res = userManager.AddClaimsAsync(defaultUser, new[]
                {
                    new Claim("UserFullName", "Remon Citizen App")
                }).Result;
            }
        }

        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                IdentityRole role = new IdentityRole
                {
                    Name = "Admin"
                };
                _ = roleManager.CreateAsync(role).Result;
            }

            if (!roleManager.RoleExistsAsync("Citizen").Result)
            {
                IdentityRole role = new IdentityRole
                {
                    Name = "Citizen"
                };
                _ = roleManager.CreateAsync(role).Result;
            }
        }
    }
}