using AspNetIdentityTry1.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AspNetIdentityTry1
{
    public class ItemsDbContext5: DbContext
    {
        public ItemsDbContext5()
            : base("name=ItemsStoreConnection5")
        {
        }



        public DbSet<Item> Items { get; set; }
    }
}