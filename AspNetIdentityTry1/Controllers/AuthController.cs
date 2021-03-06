﻿using AspNetIdentityTry1.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace AspNetIdentityTry1.Controllers
{
    //[AllowAnonymous]
    public class AuthController : Controller
    {
        UserManager<User> userManager;
        UserStore<User> userStore;
        AppDbContext dbContext;

        public AuthController()
        {
            dbContext = new AppDbContext();
            userStore = new UserStore<User>(dbContext);
            userManager = new UserManager<User>(userStore);
        }

        // GET: Auth
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
       // [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
                
        [HttpPost]
       // [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // TempData["Notification"] = "ModelState is NOT valid";

                    return View();
                }

                var user = new User()
                {
                    UserName = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Position = model.Position,
                    Email = model.Email,
                    ParentName = User.Identity.Name
                };

                var result = userManager.Create(user, model.Password);
                var currentUser = userManager.FindByName(user.UserName);

                // available roles: Administrator, CommonUser, SuperUser
                //var roleResult = userManager.AddToRole(currentUser.Id, "CommonUser");
                //var roleResult = userManager.AddToRole(currentUser.Id, User.Identity.Name == "admin@admin.pl" ? "Administrator" : "CommonUser");

                if (result.Succeeded)
                {
                    var roleResult = userManager.AddToRole(currentUser.Id, User.IsInRole("SuperUser") ? "Administrator" : "CommonUser");

                    TempData["Notification"] = "New user has been added";

                    // return RedirectToAction("LogIn");
                    return RedirectToAction("ShowUserItems", "ItemsPriority");
                }
                else
                {
                    string error = result.Errors.FirstOrDefault();

                    TempData["Notification"] = "Something gone wrong. " + error;

                    return View();
                }
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult LogIn()
        {
            // User was redirected here because of authorization section
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {                
               return RedirectToAction("NotAuthorized");                
            }
            else
            {
                return View();
            }            
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult LogIn(LogInModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    //TempData["Notification"] = "ModelState is NOT valid.";

                    return View();
                }

                var user = userManager.Find(model.Email, model.Password);

                if (user != null)
                {
                    // create identity
                    var identity = userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);

                    // create authentication manager;           
                    var ctx = Request.GetOwinContext();
                    ctx.Authentication.SignIn(identity);

                    return RedirectToAction("ShowUserItems", "ItemsPriority");
                }
                else
                {
                    TempData["Notification"] = "Wrong user or password";
                    return View();
                }
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        public ActionResult LogOut()
        {
            try
            {
                var ctx = Request.GetOwinContext();
                ctx.Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                TempData["Notification"] = "User has been logged out";

                return RedirectToAction("LogIn");
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            ChangePasswordModel user = new ChangePasswordModel();

            user.Email = User.Identity.Name;
            
            return View(user);
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Notification"] = "Provided data are wrong.";

                    return RedirectToAction("ChangePassword");
                }
                else
                {
                    // Debug.WriteLine("Auth / ChangePassword - ModelState is valid");
                    // TempData["Notification"] = "Auth / ChangePassword - ModelState is valid";

                    var user = userManager.Find(model.Email, model.OldPassword);

                    if (user != null && model.NewPassword == model.ConfirmNewPassword)
                    {
                        // create identity
                        var identity = userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);

                        string userId = identity.GetUserId();

                        if (userId != null)
                        {
                            IdentityResult result = userManager.ChangePassword(userId, model.OldPassword, model.NewPassword);

                            if (result.Succeeded)
                            {
                                TempData["Notification"] = "Password has been changed sucessfully";
                                return RedirectToAction("ChangePassword");
                            }
                            else
                            {
                                TempData["Notification"] = "Something gone wrong with password changing";
                                return RedirectToAction("ChangePassword");
                            }
                        }
                    }

                    //return RedirectToAction("ShowUserItems", "ItemsPriority");
                    TempData["Notification"] = "Something gone wrong with password changing. Probably you provide wrong password values.";
                    return RedirectToAction("ChangePassword");
                }
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        public ActionResult NotAuthorized()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles="Administrator")]        
        public string Sample()
        {
            try
            {
                var ctx = Request.GetOwinContext();
                string userId = ctx.Authentication.User.Identity.GetUserId();
                string userName = ctx.Authentication.User.Identity.Name;

                var user = userManager.FindById(userId);

                if (user != null)
                {
                    return "Auth / Sample - user != null" + user.FirstName + " " + user.LastName + " " + user.Email;
                }
                else
                {
                    return "Auth / Sample - user == null";
                }
            }
            catch (Exception ex)
            {
                return "something gone wrong: " + ex.Message;
            }
        }

        

        [HttpGet]
        [Authorize(Roles = "Administrator, SuperUser")]
        public ActionResult DeleteUser()
        {
            AppDbContext userDbContext = new AppDbContext();
                       
            ViewBag.allUsersName = userDbContext.Users.Where(u => u.ParentName == User.Identity.Name).OrderBy(u => u.UserName).Select(u => u.UserName);

            User appUser = userManager.FindByName(User.Identity.Name);

            if (appUser != null)
            {
                Debug.WriteLine("appUser.UserName: {0}, appUser.FirstName: {1}, appUser.LastName: {2}", appUser.UserName, appUser.FirstName, appUser.LastName);
            }

            //return View(appUser);
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, SuperUser")]
        public ActionResult DeleteUser(User model)
        {
            if (!ModelState.IsValid)
            {
                AppDbContext userDbContext = new AppDbContext();
                ViewBag.allUsersName = userDbContext.Users.Where(u => u.ParentName == User.Identity.Name).Select(u => u.UserName);

                return View();
            }
            

            var userToDelete = userManager.FindByName(model.UserName);

            var result = userManager.Delete(userToDelete);

            if (result.Succeeded)
            {
                ItemsDbContext5 itemDbContext = new ItemsDbContext5();
                List<Item> items2del = itemDbContext.Items.Where(i => i.UserName == model.UserName).ToList();

                if (items2del != null)
                {
                    foreach (Item item in items2del)
                    {
                        itemDbContext.Items.Remove(item);
                    }

                    itemDbContext.SaveChanges();
                }

                TempData["Notification"] = "User " + userToDelete.UserName + " has been deleted.";
               
                return RedirectToAction("DeleteUser");
            }
            else
            {
                Debug.WriteLine("Something gone wrong with user deletion");
            }

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult ShowAllUsers()
        {
            var userList = userManager.Users;

            if (userList != null)
            {
                return View(userList);
            }
            else
            {
                TempData["Notification"] = "No users have been found";

                return RedirectToAction("Index", "Home");
            }
        }

        /*[HttpPost]
        [Authorize(Roles = "Administrator")]
        public ViewResult ShowAllUsers(List<User> users)
        {
            return View();
        }*/

        [AllowAnonymous]
        [HttpGet]
        public string SampleFunction(string param)
        {
            return param + " funkcja wykonana";
        }

        [AllowAnonymous]
        public string AddRole(string roleToAdd)
        {
            // CommonUser, Administrator, SuperUser

            IdentityRole role = new IdentityRole(roleToAdd);

            dbContext.Roles.Add(role);
            dbContext.SaveChanges();

            return "Role " + roleToAdd + " has been added.";
        }

        [AllowAnonymous]
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

                return "Roles have been updated.";
            }
            else
            {
                return "Something gone wrong.";
            }

        }

    }
}