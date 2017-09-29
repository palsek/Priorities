using AspNetIdentityTry1.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace AspNetIdentityTry1.Controllers
{
    public class ItemsPriorityController : Controller
    {
        //=================================================== INITIALS =============================================================

        private ItemsDbContext5 itemsDbContext;
        private AppDbContext userDbContext;
        private List<User> allUsers;        

        public ItemsPriorityController()
        {
            itemsDbContext = new ItemsDbContext5();
            userDbContext = new AppDbContext();

            allUsers = userDbContext.Users.OrderBy(u => u.UserName).ToList();
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
            try
            {
                string parentUserName = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().ParentName;

                Item item = new Item() { ParentUserName = parentUserName };

                return View(item);
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNewItem(Item item)
        {
            try
            {
                User currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

                item.UserName = User.Identity.Name;
                item.ParentUserName = currentUser.ParentName;
                item.Created = DateTime.Now;
                item.Status = Status.New;

                if (!ModelState.IsValid)
                {
                    // TempData["Notification"] = "ModelState is NOT valid";

                    string parentUserName = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().ParentName;

                    Item item2add = new Item() { ParentUserName = parentUserName };

                    return View(item2add);
                }

                itemsDbContext.Items.Add(item);
                itemsDbContext.SaveChanges();

                return RedirectToAction("ShowUserItems");
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateItemForUser()
        {
            try
            {
                string parentUserName = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().ParentName;

                ViewBag.allUsersName = allUsers.Where(u => u.ParentName == User.Identity.Name).Select(u => u.UserName);

                Item item = new Item() { ParentUserName = parentUserName };

                return View(item);
            }
            catch(Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateItemForUser(Item item2add)
        {
            try
            {
                User currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

                item2add.Created = DateTime.Now;
                item2add.Status = Status.New;
                item2add.ParentUserName = currentUser.UserName;

                if (!ModelState.IsValid)
                {
                    //TempData["Notification"] = "ModelState is NOT valid";
                    string parentUserName = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().ParentName;
                    ViewBag.allUsersName = allUsers.Where(u => u.ParentName == User.Identity.Name).Select(u => u.UserName);
                    Item item = new Item() { ParentUserName = parentUserName };

                    return View(item);
                }

                itemsDbContext.Items.Add(item2add);
                itemsDbContext.SaveChanges();

                return RedirectToAction("ShowAllItems", "ItemsPriority");
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        //=================================================== READ =============================================================
                
        List<Item> GetData(string userName, int page, int numberPerPage, int allPageNumber, int allItemsNumber, string invokeAction, string orderBy = "Name", string orderDirection = "desc")
        {
            int itemsToSkip = (numberPerPage * page) - numberPerPage;

            if (itemsToSkip <= allItemsNumber)
            {
                List<Item> currentItems = null;
                                
                if (invokeAction == "ShowUserItems")
                {
                    var queryToItems = itemsDbContext.Items.Where(i => i.UserName == userName && (i.Status == Status.New || i.Status == Status.InProgress));

                    switch(orderBy)
                    {
                        case "Name":
                            if (orderDirection == "desc")
                            {
                                queryToItems = queryToItems.OrderByDescending(i => i.Name);
                            }
                            else
                            {
                                queryToItems = queryToItems.OrderBy(i => i.Name);
                            }
                        break;

                        case "Description":
                            if (orderDirection == "desc")
                            {
                                queryToItems = queryToItems.OrderByDescending(i => i.Name);
                            }
                            else
                            {
                                queryToItems = queryToItems.OrderBy(i => i.Name);
                            }
                        break;
                       
                        case "Status":
                            if (orderDirection == "desc")
                            {
                                queryToItems = queryToItems.OrderByDescending(i => i.Status);
                            }
                            else
                            {
                                queryToItems = queryToItems.OrderBy(i => i.Status);
                            }
                        break;

                        case "Created":
                            if (orderDirection == "desc")
                            {
                                queryToItems = queryToItems.OrderByDescending(i => i.Created);
                            }
                            else
                            {
                                queryToItems = queryToItems.OrderBy(i => i.Created);
                            }
                        break;

                        default:
                             if (orderDirection == "desc")
                            {
                                queryToItems = queryToItems.OrderByDescending(i => i.Priority);
                            }
                            else
                            {
                                queryToItems = queryToItems.OrderBy(i => i.Priority);
                            }
                        break;
                    }
                  
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
                                                            ;

                    switch (orderBy)
                    {
                        case "Name":
                            if (orderDirection == "desc")
                            {
                                queryToItems = queryToItems.OrderByDescending(i => i.Name);
                            }
                            else
                            {
                                queryToItems = queryToItems.OrderBy(i => i.Name);
                            }
                            break;

                        case "Description":
                            if (orderDirection == "desc")
                            {
                                queryToItems = queryToItems.OrderByDescending(i => i.Name);
                            }
                            else
                            {
                                queryToItems = queryToItems.OrderBy(i => i.Name);
                            }
                            break;

                        case "Status":
                            if (orderDirection == "desc")
                            {
                                queryToItems = queryToItems.OrderByDescending(i => i.Status);
                            }
                            else
                            {
                                queryToItems = queryToItems.OrderBy(i => i.Status);
                            }
                            break;

                        case "Created":
                            if (orderDirection == "desc")
                            {
                                queryToItems = queryToItems.OrderByDescending(i => i.Created);
                            }
                            else
                            {
                                queryToItems = queryToItems.OrderBy(i => i.Created);
                            }
                            break;

                        default:
                            if (orderDirection == "desc")
                            {
                                queryToItems = queryToItems.OrderByDescending(i => i.Priority);
                            }
                            else
                            {
                                queryToItems = queryToItems.OrderBy(i => i.Priority);
                            }
                            break;
                    }

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
                                                           ;

                        switch (orderBy)
                        {
                            case "Name":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Name);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Name);
                                }
                                break;

                            case "Description":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Name);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Name);
                                }
                                break;

                            case "Status":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Status);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Status);
                                }
                                break;

                            case "Created":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Created);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Created);
                                }
                                break;

                            default:
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Priority);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Priority);
                                }
                                break;
                        }

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
                                                          ;

                        switch (orderBy)
                        {
                            case "Name":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Name);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Name);
                                }
                                break;

                            case "Description":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Name);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Name);
                                }
                                break;

                            case "Status":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Status);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Status);
                                }
                                break;

                            case "Created":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Created);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Created);
                                }
                                break;

                            default:
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Priority);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Priority);
                                }
                                break;
                        }

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
                                                         ;
                        switch (orderBy)
                        {
                            case "Name":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Name);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Name);
                                }
                                break;

                            case "Description":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Name);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Name);
                                }
                                break;

                            case "Status":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Status);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Status);
                                }
                                break;

                            case "Created":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Created);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Created);
                                }
                                break;

                            default:
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Priority);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Priority);
                                }
                                break;
                        }

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
                                                         ;
                        switch (orderBy)
                        {
                            case "Name":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Name);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Name);
                                }
                                break;

                            case "Description":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Name);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Name);
                                }
                                break;

                            case "Status":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Status);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Status);
                                }
                                break;

                            case "Created":
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Created);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Created);
                                }
                                break;

                            default:
                                if (orderDirection == "desc")
                                {
                                    queryToItems = queryToItems.OrderByDescending(i => i.Priority);
                                }
                                else
                                {
                                    queryToItems = queryToItems.OrderBy(i => i.Priority);
                                }
                                break;
                        }

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
        public ActionResult ShowUserItems(int page = 1, int numberPerPage = 20, string orderBy = "Priority", string orderDirection = "desc")
        {
            try
            {
                string userName = User.Identity.Name;
                ViewBag.InvokingAction = "ShowUserItems";

                // --------------------------------------- Calculate pagination------------------------------------------
                int allItemsNumber = itemsDbContext.Items
                                                        .Where(i => i.UserName == userName && (i.Status == Status.New || i.Status == Status.InProgress))
                                                        .Count();

                if (numberPerPage <= 0 || numberPerPage > 999)
                {
                    numberPerPage = 20;
                }

                int allPageNumber = allItemsNumber / numberPerPage;
                int modulo = allItemsNumber % numberPerPage;

                if (allPageNumber == 0 || modulo > 0)
                {
                    allPageNumber++;
                }

                //------------------------------ WYCIAGNIECIE LOGIKI DO OSOBNEJ FUNKCJI ----------------------------------------------
                List<Item> currentItems = GetData(userName, page, numberPerPage, allPageNumber, allItemsNumber, "ShowUserItems", orderBy, orderDirection);

                User currentUser = allUsers.Where(u => u.UserName == userName).FirstOrDefault();

                if (currentItems != null)
                {
                    ItemsData itemsData = new ItemsData()
                    {
                        Items = currentItems,
                        ViewInfo = new ViewInfo()
                        {
                            Page = page,
                            NumberPerPage = numberPerPage,
                            AllPageNumber = allPageNumber,
                            OrderBy = orderBy,
                            OrderDirection = orderDirection,
                            InvokingViewAction = "ShowUserItems"
                        },
                        Setup = GetUserSetup(currentUser)
                    };
                                       
                    return View(itemsData);
                }
                else
                {
                    return new HttpNotFoundResult("Not found");
                }
            }
            catch(Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult ShowOldUserItems(int page = 1, int numberPerPage = 20, string orderBy = "Priority", string orderDirection = "desc")
        {
            try
            {
                string userName = User.Identity.Name;

                ViewBag.InvokingAction = "ShowOldUserItems";

                int allItemsNumber = itemsDbContext.Items
                                                        .Where(i => i.UserName == userName && i.Status == Status.Done).Count();

                if (numberPerPage <= 0 || numberPerPage > 999)
                {
                    numberPerPage = 20;
                }

                int allPageNumber = allItemsNumber / numberPerPage;
                int modulo = allItemsNumber % numberPerPage;


                if (allPageNumber == 0 || modulo > 0)
                {
                    allPageNumber++;
                }

                List<Item> currentItems = GetData(userName, page, numberPerPage, allPageNumber, allItemsNumber, "ShowOldUserItems", orderBy, orderDirection);

                User currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

                if (currentItems != null)
                {
                    ItemsData itemsData = new ItemsData()
                    {
                        Items = currentItems,
                        ViewInfo = new ViewInfo()
                        {
                            Page = page,
                            NumberPerPage = numberPerPage,
                            AllPageNumber = allPageNumber,
                            OrderBy = orderBy,
                            OrderDirection = orderDirection
                        },
                        Setup = GetUserSetup(currentUser)
                    };

                    return View("ShowUserItems", itemsData);
                }
                else
                {
                    return new HttpNotFoundResult("Not found");
                }
            }
            catch(Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult ShowAllItems(string userName = "", int page = 1, int numberPerPage = 20, string orderBy = "Priority", string orderDirection = "desc")
        {
            try
            {
                User currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault() as User;

                ViewBag.InvokingAction = "ShowAllItems";

                int allItemsNumber;

                if (userName == "" || userName == "All")
                {
                    allItemsNumber = itemsDbContext.Items
                                                        .Where(i => i.Status == Status.New || i.Status == Status.InProgress)
                                                        .Where(i => i.ParentUserName == currentUser.UserName)
                                                        .Count();
                }
                else
                {
                    allItemsNumber = itemsDbContext.Items
                                                        .Where(i => (i.Status == Status.New || i.Status == Status.InProgress) && i.UserName == userName).Count();
                }

                if (numberPerPage <= 0 || numberPerPage > 999)
                {
                    numberPerPage = 20;
                }

                int allPageNumber = allItemsNumber / numberPerPage;
                int modulo = allItemsNumber % numberPerPage;

                if (allPageNumber == 0 || modulo > 0)
                {
                    allPageNumber++;
                }

                List<Item> currentItems = GetData(userName, page, numberPerPage, allPageNumber, allItemsNumber, "ShowAllItems", orderBy, orderDirection)
                    .Where(i => i.ParentUserName == currentUser.UserName).ToList()
                    ;

                if (currentItems != null)
                {
                    ItemsData itemsData = new ItemsData()
                    {
                        Items = currentItems,
                        ViewInfo = new ViewInfo()
                        {
                            AllUsersName = allUsers.Where(u => u.ParentName == currentUser.UserName).Select(u => u.UserName).ToList<string>(),
                            UserName = userName,
                            Page = page,
                            NumberPerPage = numberPerPage,
                            AllPageNumber = allPageNumber,
                            OrderBy = orderBy,
                            OrderDirection = orderDirection,
                            InvokingViewAction = "ShowAllItems"
                        }
                    };

                    return View(itemsData);
                }
                else
                {
                    return new HttpNotFoundResult("Not found");
                }
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult ShowAllOldItems(string userName = "", int page = 1, int numberPerPage = 20, string orderBy = "Priority", string orderDirection = "desc")
        {
            try
            {
                User currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault() as User;

                ViewBag.InvokingAction = "ShowAllOldItems";

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

                if (numberPerPage <= 0)
                {
                    numberPerPage = 20;
                }

                int allPageNumber = allItemsNumber / numberPerPage;
                int modulo = allItemsNumber % numberPerPage;

                if (allPageNumber == 0 || modulo > 0)
                {
                    allPageNumber++;
                }

                List<Item> currentItems = GetData(userName, page, numberPerPage, allPageNumber, allItemsNumber, "ShowAllOldItems", orderBy, orderDirection)
                    .Where(i => i.ParentUserName == currentUser.UserName).ToList()
                    ;

                if (currentItems != null)
                {
                    ItemsData itemsData = new ItemsData()
                    {
                        Items = currentItems,
                        ViewInfo = new ViewInfo()
                        {
                            AllUsersName = allUsers.Where(u => u.ParentName == currentUser.UserName).Select(u => u.UserName).ToList<string>(),
                            UserName = userName,
                            Page = page,
                            NumberPerPage = numberPerPage,
                            AllPageNumber = allPageNumber,
                            OrderBy = orderBy,
                            OrderDirection = orderDirection
                        }
                    };

                    return View(itemsData);
                }
                else
                {
                    return new HttpNotFoundResult("Not found");
                }
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        //=================================================== UPDATE =============================================================

        [HttpGet]
        public ActionResult EditItem(int Id, string invokingViewAction = null)
        {
            try
            {
                string parentUserName = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().ParentName;

                List<string> allCurrentUsersNames = new List<string>();

                if (User.IsInRole("Administrator"))
                {
                    allCurrentUsersNames = allUsers.Where(u => u.ParentName == User.Identity.Name).Select(u => u.UserName).ToList();
                    allCurrentUsersNames.Add(User.Identity.Name);
                }
                else
                {
                    allCurrentUsersNames.Add(User.Identity.Name);
                }
                             

                Item item = itemsDbContext.Items.Where(i => i.Id == Id).FirstOrDefault();

                User currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

                if (item != null)
                {
                    ItemsData itemData = new ItemsData()
                    {
                        OneItem = item,
                        ViewInfo = new ViewInfo()
                        {
                            InvokingViewAction = invokingViewAction,
                            AllUsersName = allCurrentUsersNames
                        },
                        Setup = GetUserSetup(currentUser)
                    };

                    return View(itemData);
                }

                return View();
            }
            catch(Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditItem(ItemsData itemData2edit)
        {
            try
            {
                string parentUserName = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().ParentName;

                List<string> allCurrentUsersNames = new List<string>();

                if (User.IsInRole("Administrator"))
                {
                    allCurrentUsersNames = allUsers.Where(u => u.ParentName == User.Identity.Name).Select(u => u.UserName).ToList();
                    allCurrentUsersNames.Add(User.Identity.Name);
                }
                else
                {
                    allCurrentUsersNames.Add(User.Identity.Name);
                }

                User currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
                Setup setup = GetUserSetup(currentUser);

                if (!ModelState.IsValid)
                {
                    TempData["Notification"] = "ModelState is NOT valid";
                    ViewBag.allUsersName = allCurrentUsersNames;

                    Item item = new Item() { ParentUserName = parentUserName };
                                        
                    ItemsData itemData = new ItemsData()
                    {
                        OneItem = item,
                        ViewInfo = new ViewInfo()
                        {
                            InvokingViewAction = itemData2edit.ViewInfo.InvokingViewAction
                        },                        
                        Setup = GetUserSetup(currentUser)
                    };

                    return View(itemData);
                }

                if (setup.AllowUserChangeItem || User.IsInRole("Administrator"))
                {
                    itemsDbContext.Items.Attach(itemData2edit.OneItem);
                    itemsDbContext.Entry(itemData2edit.OneItem).State = EntityState.Modified;
                    itemsDbContext.SaveChanges();
                }

                return RedirectToAction(itemData2edit.ViewInfo.InvokingViewAction == null ? "ShowUserItems" : itemData2edit.ViewInfo.InvokingViewAction);
            }
            catch(Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        //=================================================== DELETE =============================================================

        [HttpGet]
        public ActionResult DeleteItem(int Id)
        {
            try
            {
                Item item2delete = itemsDbContext.Items.Where(i => i.Id == Id).FirstOrDefault();

                if (item2delete != null)
                {
                    return View(item2delete);
                }
                return new HttpNotFoundResult();
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteItem(Item item2delete)
        {
            try
            {
                User currentUser = allUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

                Setup setup = GetUserSetup(currentUser);

                if (setup.AllowUserChangeItem || User.IsInRole("Administrator"))
                {
                    Item item2del = itemsDbContext.Items.Where(i => i.Id == item2delete.Id).FirstOrDefault();

                    if (item2delete != null)
                    {
                        itemsDbContext.Items.Remove(item2del);
                        itemsDbContext.SaveChanges();
                    }

                    return RedirectToAction("ShowUserItems");
                }
                else
                {
                    return HttpNotFound();
                }
                
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        private Setup GetUserSetup(User currentUser)
        {
            // setup powinien być generalnie per uzytkownik (admin, user), ale parametry ustawiane by Admin dla Uzytkownikow, są zaczytywane z setupu Admina
            Setup setup = itemsDbContext.Setups.Where(s => s.UserName == User.Identity.Name).FirstOrDefault();

            if (setup != null && User.IsInRole("CommonUser"))
            {
                setup.AllowUserChangeItem = itemsDbContext.Setups.Where(s => s.UserName == currentUser.ParentName).FirstOrDefault().AllowUserChangeItem;
            }
            else if (User.IsInRole("CommonUser"))
            {
                setup = new Setup();
                
                Setup parentSetup = itemsDbContext.Setups.Where(s => s.UserName == currentUser.ParentName).FirstOrDefault();

                if (parentSetup != null)
                {
                    setup.AllowUserChangeItem = parentSetup.AllowUserChangeItem;
                }
            }           
                    
            return setup;
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