using AspNetIdentityTry1.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace AspNetIdentityTry1
{
    public class InitialConfig
    {
        UserManager<User> userManager;
        UserStore<User> userStore;
        AppDbContext dbContext;

        public InitialConfig()
        {
            dbContext = new AppDbContext();
            userStore = new UserStore<User>(dbContext);
            userManager = new UserManager<User>(userStore);
        }

        public string UpdateRoles()
        {          
            // available roles: SuperUser, Administrator, CommonUser

            List<IdentityRole> roles = dbContext.Roles.ToList();// as List<IdentityRole>;

            if (roles != null)
            {

                if (!roles.Any(r => r.Name == "SuperUser"))
                {
                    dbContext.Roles.Add(new IdentityRole { Name = "SuperUser" });
                    dbContext.SaveChanges();
                }

                if (!roles.Any(r => r.Name == "Administrator"))
                {
                    dbContext.Roles.Add(new IdentityRole { Name = "Administrator" });
                    dbContext.SaveChanges();
                }

                if (!roles.Any(r => r.Name == "CommonUser"))
                {
                    dbContext.Roles.Add(new IdentityRole { Name = "CommonUser" });
                    dbContext.SaveChanges();
                }

                Debug.WriteLine("Roles have been updated.");
                return "Roles have been updated.";
            }
            else
            {
                Debug.WriteLine("Something gone wrong with roles update.");
                return "Something gone wrong with role update.";
            }
        }

        public string AddSuperUserIfNotExist()
        {
            var superUser = userManager.Users.Where(u => u.UserName == "admin@admin.pl").FirstOrDefault();

            if (superUser == null)
            {
                var user = new User()
                {
                    UserName = "admin@admin.pl",
                    FirstName = "admin",
                    LastName = "admin",
                    Position = "admin",
                    Email = "admin@admin.pl"
                };

                var result = userManager.Create(user, "admin@admin.pl");

                var currentUser = userManager.FindByName(user.UserName);
                
                if (result.Succeeded)
                {
                    var roleResult = userManager.AddToRole(currentUser.Id, "SuperUser");

                    Debug.WriteLine("SuperUser admin has been added.");
                    return "SuperUser admin has been added.";
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Debug.WriteLine(error.ToString());
                    }

                    Debug.WriteLine("Something gone wrong with addition SuperUser admin.");
                    return "Something gone wrong with addition SuperUser admin.";
                }
            }
            else
            {
                Debug.WriteLine("User already exists.");
                return "User already exists.";
            }            
        }

    }
}