using AspNetIdentityTry1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspNetIdentityTry1.Controllers
{
    public class SetupController : Controller
    {
        ItemsDbContext5 itemsDbContext;

        public SetupController()
        {
            itemsDbContext = new ItemsDbContext5();
        }

        [HttpGet]
        public ActionResult EditSetup()
        {
            try
            {
                string userName = User.Identity.Name;

                Setup setup = itemsDbContext.Setups.Where(s => s.UserName == userName).FirstOrDefault();

                // if not exists, create new one with default values
                // default for setup.AllowUserChangeItem == false

                if (setup == null)
                {
                    setup = new Setup()
                    {
                        UserName = userName,
                        AllowUserChangeItem = false
                    };

                    itemsDbContext.Setups.Add(setup);
                    itemsDbContext.SaveChanges();
                }

                return View(setup);
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult EditSetup(Setup setup2edit)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    itemsDbContext.Setups.Attach(setup2edit);
                    itemsDbContext.Entry(setup2edit).State = System.Data.Entity.EntityState.Modified;
                    itemsDbContext.SaveChanges();
                }

                return RedirectToAction("ShowUserItems", "ItemsPriority");
            }
            catch (Exception ex)
            {
                return new HttpNotFoundResult(ex.Message);
            }
        }

    }
}