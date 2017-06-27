using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using AspNetIdentityTry1.Models;

namespace AspNetIdentityTry1
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext()
            : base("DefaultConnection")
        { 
        }
    }
}