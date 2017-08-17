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
        private List<User> allUsers;
        //private User currentUser;

        public ItemsPriorityController()
        {
            Debug.WriteLine("ItemsPriorityController / CONSTRUCTOR");

            itemsDbContext = new ItemsDbContext5();
            userDbContext = new AppDbContext();

            allUsers = userDbContext.Users.OrderBy(u => u.UserName).ToList();
            allUserNames = new List<string>();

           // currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
                                    
            foreach (var user in allUsers)
            {
                //if (user.ParentName == currentUser.ParentName)
                //{
                    allUserNames.Add(user.UserName);
                //}
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
            string parentUserName = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().ParentName;

            Item item = new Item() { ParentUserName = parentUserName };

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNewItem(Item item)
        {
            User currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            item.UserName = User.Identity.Name;
            item.ParentUserName = currentUser.ParentName;
            //item.User = User.Identity;
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
            string parentUserName = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().ParentName;

            ViewBag.allUsersName = allUsers.Where(u => u.ParentName == User.Identity.Name).Select(u => u.UserName);            

            Item item = new Item() { ParentUserName = parentUserName };

            return View(item);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateItemForUser(Item item2add)
        {
            User currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            item2add.Created = DateTime.Now;
            item2add.Status = Status.New;
            item2add.ParentUserName = currentUser.UserName;

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

        List<Item> GetData(string userName, int page, int numberPerPage, int allPageNumber, int allItemsNumber, string invokeAction)
        {
            int itemsToSkip = (numberPerPage * page) - numberPerPage;

            if (itemsToSkip <= allItemsNumber)
            {
                List<Item> currentItems = null;
                                

                if (invokeAction == "ShowUserItems")
                {                   
                   var queryToItems = itemsDbContext.Items
                                                            .Where(i => i.UserName == userName && (i.Status == Status.New || i.Status == Status.InProgress))
                                                            .OrderByDescending(i => i.Priority)
                                                            .ThenBy(i => i.Created);

                   if (itemsToSkip + numberPerPage > allItemsNumber)  // For page which is NOT full of items per page (the last page)
                   {
                       int itemsPerLastPage = numberPerPage - (itemsToSkip + numberPerPage - allItemsNumber);

                       currentItems = queryToItems.ToList().GetRange(itemsToSkip, itemsPerLastPage);
                   }
                   else // For page which IS full of items per page
                   {
                       currentItems = queryToItems.ToList().GetRange(itemsToSkip, numberPerPage);
                   }
                }
                else if (invokeAction == "ShowOldUserItems")
                {
                    var queryToItems = itemsDbContext.Items
                                                            .Where(i => i.UserName == userName && (i.Status == Status.Done))
                                                            .OrderByDescending(i => i.Priority)
                                                            .ThenBy(i => i.Created);

                    if (itemsToSkip + numberPerPage > allItemsNumber)  // For page which is NOT full of items per page (the last page)
                    {
                        int itemsPerLastPage = numberPerPage - (itemsToSkip + numberPerPage - allItemsNumber);

                        currentItems = queryToItems.ToList().GetRange(itemsToSkip, itemsPerLastPage);
                    }
                    else // For page which IS full of items per page
                    {
                        int x = queryToItems.ToList().Count();

                        currentItems = queryToItems.ToList().GetRange(itemsToSkip, numberPerPage);
                    }
                }
                else if (invokeAction == "ShowAllItems")
                {
                    if (userName == "" || userName == "All")
                    {
                        var queryToItems = itemsDbContext.Items
                                                           .Where(i => i.Status == Status.New || i.Status == Status.InProgress)
                                                           .Where(i => i.ParentUserName == User.Identity.Name)
                                                           .OrderByDescending(i => i.Priority)
                                                           .ThenBy(i => i.Created);

                        if (itemsToSkip + numberPerPage > allItemsNumber)  // For page which is NOT full of items per page (the last page)
                        {
                            int itemsPerLastPage = numberPerPage - (itemsToSkip + numberPerPage - allItemsNumber);

                            currentItems = queryToItems.ToList().GetRange(itemsToSkip, itemsPerLastPage);
                        }
                        else // For page which IS full of items per page
                        {
                            currentItems = queryToItems.ToList().GetRange(itemsToSkip, numberPerPage);
                        }
                    }
                    else
                    {                       
                        var queryToItems = itemsDbContext.Items
                                                          .Where(i => (i.Status == Status.New || i.Status == Status.InProgress) && i.UserName == userName)
                                                          .OrderByDescending(i => i.Priority)
                                                          .ThenBy(i => i.Created);

                        if (itemsToSkip + numberPerPage > allItemsNumber)  // For page which is NOT full of items per page (the last page)
                        {
                            int itemsPerLastPage = numberPerPage - (itemsToSkip + numberPerPage - allItemsNumber);

                            currentItems = queryToItems.ToList().GetRange(itemsToSkip, itemsPerLastPage);                            
                        }
                        else // For page which IS full of items per page
                        {
                            currentItems = queryToItems.ToList().GetRange(itemsToSkip, numberPerPage);
                        }
                    }
                }
                else if (invokeAction == "ShowAllOldItems")
                {
                    if (userName == "" || userName == "All")
                    {                        
                        var queryToItems = itemsDbContext.Items
                                                         .Where(i => i.Status == Status.Done)
                                                         .Where(i => i.ParentUserName == User.Identity.Name)
                                                         .OrderByDescending(i => i.Priority)
                                                         .ThenBy(i => i.Created);

                        if (itemsToSkip + numberPerPage > allItemsNumber)  // For page which is NOT full of items per page (the last page)
                        {
                            int itemsPerLastPage = numberPerPage - (itemsToSkip + numberPerPage - allItemsNumber);

                            currentItems = queryToItems.ToList().GetRange(itemsToSkip, itemsPerLastPage);
                        }
                        else // For page which IS full of items per page
                        {
                            currentItems = queryToItems.ToList().GetRange(itemsToSkip, numberPerPage);
                        }
                    }
                    else
                    {                        
                        var queryToItems = itemsDbContext.Items
                                                         .Where(i => i.Status == Status.Done && i.UserName == userName)
                                                         .OrderByDescending(i => i.Priority)
                                                         .ThenBy(i => i.Created);

                        if (itemsToSkip + numberPerPage > allItemsNumber)  // For page which is NOT full of items per page (the last page)
                        {
                            int itemsPerLastPage = numberPerPage - (itemsToSkip + numberPerPage - allItemsNumber);

                            currentItems = queryToItems.ToList().GetRange(itemsToSkip, itemsPerLastPage);
                        }
                        else // For page which IS full of items per page
                        {
                            currentItems = queryToItems.ToList().GetRange(itemsToSkip, numberPerPage);
                        }
                    }
                }


                if (currentItems != null)
                {
                    return currentItems;
                }
                else
                {
                    return null;
                }

            }
            else
            {
                Debug.WriteLine("--------------OUT OF RANGE PAGE--------------------");

                return null;
            }

            
        }

        [HttpGet]
        public ActionResult ShowUserItems(int page = 1, int numberPerPage = 20)
        {                                                        
            string userName = User.Identity.Name;
            ViewBag.InvokingAction = "ShowUserItems";
                      
            // --------------------------------------- Calculate pagination------------------------------------------

            ViewBag.NumberPerPage = numberPerPage;
                        
            int allItemsNumber = itemsDbContext.Items
                                                    .Where(i => i.UserName == userName && (i.Status == Status.New || i.Status == Status.InProgress))
                                                    .Count();

            int allPageNumber = allItemsNumber / numberPerPage;
            int modulo = allItemsNumber % numberPerPage;

            if (allPageNumber == 0 || modulo > 0)
            {
                allPageNumber++;
            }

            ViewBag.AllPageNumber = allPageNumber;

            //------------------------------ WYCIAGNIECIE LOGIKI DO OSOBNEJ FUNKCJI ----------------------------------------------
            List<Item> currentItems = GetData(userName, page, numberPerPage, allPageNumber, allItemsNumber, "ShowUserItems");

            if (currentItems != null)
            {
                return View(currentItems);
            }
            else
            {
                return new HttpNotFoundResult("Not found");
            }
        }

        [HttpGet]
        public ActionResult ShowOldUserItems(int page = 1, int numberPerPage = 20)
        {           
            string userName = User.Identity.Name;

            ViewBag.NumberPerPage = numberPerPage;
            ViewBag.InvokingAction = "ShowOldUserItems";

            int allItemsNumber = itemsDbContext.Items
                                                    .Where(i => i.UserName == userName && i.Status == Status.Done).Count();

            int allPageNumber = allItemsNumber / numberPerPage;
            int modulo = allItemsNumber % numberPerPage;

            if (allPageNumber == 0 || modulo > 0)
            {
                allPageNumber++;
            }

            ViewBag.AllPageNumber = allPageNumber;

            List<Item> currentItems = GetData(userName, page, numberPerPage, allPageNumber, allItemsNumber, "ShowOldUserItems");

            if (currentItems != null)
            {
                return View("ShowUserItems", currentItems);
            }
            else
            {
                return new HttpNotFoundResult("Not found");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult ShowAllItems(string userName = "", int page = 1, int numberPerPage = 20)
        {
            User currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault() as User;

            ViewBag.allUsersName = allUsers.Where(u => u.ParentName == currentUser.UserName).Select(u => u.UserName);
            ViewBag.UserName = userName;
            ViewBag.NumberPerPage = numberPerPage;

            int allItemsNumber;

            if (userName == "" || userName == "All")
            {
                allItemsNumber = itemsDbContext.Items
                                                    .Where(i => i.Status == Status.New || i.Status == Status.InProgress)
                                                    .Where(i => i.ParentUserName == currentUser.UserName)
                                                    //.Where(i => i.ParentUserName == currentUser.Email)
                                                    .Count();
            }
            else
            {
                allItemsNumber = itemsDbContext.Items
                                                    .Where(i => (i.Status == Status.New || i.Status == Status.InProgress) && i.UserName == userName).Count();
            }


            int allPageNumber = allItemsNumber / numberPerPage;
            int modulo = allItemsNumber % numberPerPage;

            if (allPageNumber == 0 || modulo > 0)
            {
                allPageNumber++;
            }

            ViewBag.AllPageNumber = allPageNumber;

            List<Item> currentItems = GetData(userName, page, numberPerPage, allPageNumber, allItemsNumber, "ShowAllItems")
                .Where(i => i.ParentUserName == currentUser.UserName).ToList()
                //.Where(i => i.ParentUserName == currentUser.Email).ToList()
                ;

            if (currentItems != null)
            {
                return View(currentItems);
            }
            else
            {
                return new HttpNotFoundResult("Not found");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult ShowAllOldItems(string userName = "", int page = 1, int numberPerPage = 20)
        {
            User currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault() as User;

            ViewBag.allUsersName = allUsers.Where(u => u.ParentName == currentUser.UserName).Select(u => u.UserName);
            ViewBag.UserName = userName;
            ViewBag.NumberPerPage = numberPerPage;

            int allItemsNumber;

            if (userName == "" || userName == "All")
            {
                allItemsNumber = itemsDbContext.Items
                                                    .Where(i => i.Status == Status.Done)
                                                    .Where(i => i.ParentUserName == currentUser.UserName)
                                                    .Count();
            }
            else
            {
                allItemsNumber = itemsDbContext.Items
                                                    .Where(i => i.Status == Status.Done && i.UserName == userName).Count();
            }


            int allPageNumber = allItemsNumber / numberPerPage;
            int modulo = allItemsNumber % numberPerPage;

            if (allPageNumber == 0 || modulo > 0)
            {
                allPageNumber++;
            }

            ViewBag.AllPageNumber = allPageNumber;


            List<Item> currentItems = GetData(userName, page, numberPerPage, allPageNumber, allItemsNumber, "ShowAllOldItems")
                .Where(i => i.ParentUserName == currentUser.UserName).ToList()
                ;

            if (currentItems != null)
            {
                return View(currentItems);
            }
            else
            {
                return new HttpNotFoundResult("Not found");
            }

        }

        //=================================================== UPDATE =============================================================

        [HttpGet]
        public ActionResult EditItem(int Id)
        {
            string parentUserName = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().ParentName;

            List<string> allCurrentUsersNames = new List<string>();

            if (User.IsInRole("Administrator"))
            {
                allCurrentUsersNames = allUsers.Where(u => u.ParentName == User.Identity.Name).Select(u => u.UserName).ToList();
            }
            else
            {
                allCurrentUsersNames.Add(User.Identity.Name);
            }

            ViewBag.allUsersName = /*allUserNames*/ allCurrentUsersNames;

            Item item = itemsDbContext.Items.Where(i => i.Id == Id).FirstOrDefault();

            if (item != null)
            {
                //item.ParentUserName = parentUserName;
                return View(item);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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


        /*[HttpGet]
        public string GetSampleData()
        {
            string UserName = "admin1@admin1.pl";
            string parentUserName = "admin@admin.pl";

            int itemsCount = itemsDbContext.Items.Where(i => i.Status == Status.New || i.Status == Status.InProgress)
                                                    .Where(i => i.ParentUserName == UserName)
                                                    //.Where(i => i.ParentUserName == currentUser.Email)
                                                    .Count();

            List<Item> itemsList = itemsDbContext.Items.Where(i => i.Status == Status.New || i.Status == Status.InProgress)
                                                    .Where(i => i.ParentUserName == UserName)
                                                    .ToList();

            string itemsNames = null;

            foreach(Item i in itemsList)
            {
                itemsNames += i.Name;
                itemsNames += ", ";
            }

            return "Liczba itemow: " + itemsCount.ToString() + " " + itemsNames;
        }*/
    }
}