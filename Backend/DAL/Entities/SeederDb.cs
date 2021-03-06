﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DAL.Entities
{
    public class SeederDb
    {
        public static void SeedUser(UserManager<DbUser> userManager,
          RoleManager<IdentityRole> roleManager)
        {
            var email = "admin@gmail.com";
            var roleName = "Admin";
            var count = userManager.Users.Count();
            if (count == 0)
            {
                var user = new DbUser
                {
                    Email = email,
                    UserName = email,
                    UserProfile = new UserProfile
                    {
                        FirstName = "Vasya",
                        LastName = "Pupkin",
                        Salary = 4501,
                        Age = 25
                    }
                };
                var result = userManager.CreateAsync(user, "Qwerty1-").Result;

                var roleresult = roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                }).Result;

                result = userManager.AddToRoleAsync(user, roleName).Result;
            }
        }
        public static void SeedData(IServiceProvider services)
        {
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var manager = scope.ServiceProvider.GetRequiredService<UserManager<DbUser>>();
                var managerRole = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                SeederDb.SeedUser(manager, managerRole);
            }
        }
    }
}
