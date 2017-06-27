using AspNetIdentityTry1.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspNetIdentityTry1.Controllers
{
    public class ItemsPriorityController : Controller
    {
        //=================================================== INITIALS =============================================================

        private ItemsDbContext5 itemsDbContext;
        private AppDbContext userDbContext;
        private List<string> allUserNames;

        public ItemsPriorityController()
        {
            itemsDbContext = new ItemsDbContext5();
            userDbContext = new AppDbContext();

            List<User> allUsers = userDbContext.Users.OrderBy(u => u.UserName).ToList();
            allUserNames = new List<string>();
                                    
            foreach (var user in allUsers)
            {
                allUserNames.Add(user.UserName);
            }           
        }

        // GET: ItemsPriority
        public ActionResult Index()
        {
            return RedirectToAction("CreateNewItem");
        }

        //=================================================== CREATE =============================================================

        [HttpGet]
        public ActionResult CreateNewItem()
        {
            Item item = new Item();

            return View(item);
        }

        [HttpPost]
        public ActionResult CreateNewItem(Item item)
        {
            item.UserName = User.Identity.Name;
            item.Created = DateTime.Now;
            item.Status = Status.New;

            if (!ModelState.IsValid)
            {
               // TempData["Notification"] = "ModelState is NOT valid";
               
                return View();
            }            

            itemsDbContext.Items.Add(item);            
            itemsDbContext.SaveChanges();

            return RedirectToAction("ShowUserItems");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateItemForUser()
        {
            ViewBag.allUsersName = allUserNames;

            Item item = new Item();

            return View(item);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateItemForUser(Item item2add)
        {
            item2add.Created = DateTime.Now;
            item2add.Status = Status.New;

            if (!ModelState.IsValid)
            {
                //TempData["Notification"] = "ModelState is NOT valid";
                ViewBag.allUsersName = allUserNames;

                return View();
            }

            itemsDbContext.Items.Add(item2add);
            itemsDbContext.SaveChanges();

            return RedirectToAction("ShowAllItems", "ItemsPriority");
        }

        //=================================================== READ =============================================================

        [HttpGet]
        public ActionResult ShowUserItems()
        {
            string userName = User.Identity.Name;            

            List<Item> userItems = itemsDbContext.Items
                                                    .Where(i => i.UserName == userName && (i.Status == Status.New || i.Status == Status.InProgress))
                                                    .OrderByDescending(i => i.Priority)                                                    
                                                    .ThenBy(i => i.Created)
                                                    .ToList();

            if (userItems != null)
            {
                return View(userItems);
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult ShowOldUserItems()
        {
            string userName = User.Identity.Name;            

            List<Item> userItems = itemsDbContext.Items
                                                    .Where(i => i.UserName == userName && i.Status == Status.Done)
                                                    .OrderByDescending(i => i.Priority)
                                                    .ThenBy(i => i.Created)
                                                    .ToList();

            if (userItems != null)
            {
                return View("ShowUserItems", userItems);
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult ShowAllItems(string userName = "")
        {
            ViewBag.allUsersName = allUserNames;

            if (userName == "" || userName == "All")
            {
                List<Item> allItems = itemsDbContext.Items
                    .Where(i => i.Status == Status.New || i.Status == Status.InProgress)
                    .OrderByDescending(i => i.Priority)
                    .ToList();

                return View(allItems);
            }
            else
            {
                List<Item> allItems = itemsDbContext.Items
                    .Where(i => (i.Status == Status.New || i.Status == Status.InProgress) && i.UserName == userName)
                    .OrderByDescending(i => i.Priority)
                    .ToList();

                return View(allItems);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult ShowAllOldItems(string userName = "")
        {
            ViewBag.allUsersName = allUserNames;

            if (userName == "" || userName == "All")
            {
                List<Item> allOldItems = itemsDbContext.Items
                    .Where(i => i.Status == Status.Done)
                    .OrderBy(i => i.Created)
                    .ToList();

                return View(allOldItems);
            }
            else
            {
                List<Item> allOldItems = itemsDbContext.Items
                    .Where(i => i.Status == Status.Done && i.UserName == userName)
                    .OrderBy(i => i.Created)
                    .ToList();

                return View(allOldItems);
            }
            
        }

        //=================================================== UPDATE =============================================================

        [HttpGet]
        public ActionResult EditItem(int Id)
        {
            ViewBag.allUsersName = allUserNames;

            Item item = itemsDbContext.Items.Where(i => i.Id == Id).FirstOrDefault();

            if (item != null)
            {
                return View(item);
            }

            return View();
        }

        [HttpPost]
        public ActionResult EditItem(Item item2edit)
        {
            if (!ModelState.IsValid)
            {
                //TempData["Notification"] = "ModelState is NOT valid";
                ViewBag.allUsersName = allUserNames;

                return View();
            }

            itemsDbContext.Items.Attach(item2edit);
            itemsDbContext.Entry(item2edit).State = EntityState.Modified;
            itemsDbContext.SaveChanges();

            return RedirectToAction("ShowUserItems");
        }

        //=================================================== DELETE =============================================================

        [HttpGet]
        public ActionResult DeleteItem(int Id)
        {
            Item item2delete = itemsDbContext.Items.Where(i => i.Id == Id).FirstOrDefault();

            if (item2delete != null)
            {
                return View(item2delete);
            }
            return null;
        }

        [HttpPost]
        public ActionResult DeleteItem(Item item2delete)
        {
            Item item2del = itemsDbContext.Items.Where(i => i.Id == item2delete.Id).FirstOrDefault();

            if(item2delete != null)
            {
                itemsDbContext.Items.Remove(item2del);
                itemsDbContext.SaveChanges();
            }

            return RedirectToAction("ShowUserItems");
        }       
                
    }
}