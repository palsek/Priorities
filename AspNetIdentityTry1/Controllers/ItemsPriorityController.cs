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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
            ViewBag.allUsersName = allUserNames;
            ViewBag.UserName = userName;

            ViewBag.NumberPerPage = numberPerPage;

            int allItemsNumber;

            if (userName == "" || userName == "All")
            {
                allItemsNumber = itemsDbContext.Items
                                                    .Where(i => i.Status == Status.New || i.Status == Status.InProgress).Count();
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


            List<Item> currentItems = GetData(userName, page, numberPerPage, allPageNumber, allItemsNumber, "ShowAllItems");

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
            ViewBag.allUsersName = allUserNames;
            ViewBag.UserName = userName;

            ViewBag.NumberPerPage = numberPerPage;

            int allItemsNumber;

            if (userName == "" || userName == "All")
            {
                allItemsNumber = itemsDbContext.Items
                                                    .Where(i => i.Status == Status.Done).Count();
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


            List<Item> currentItems = GetData(userName, page, numberPerPage, allPageNumber, allItemsNumber, "ShowAllOldItems");

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
            ViewBag.allUsersName = allUserNames;

            Item item = itemsDbContext.Items.Where(i => i.Id == Id).FirstOrDefault();

            if (item != null)
            {
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
                
    }
}