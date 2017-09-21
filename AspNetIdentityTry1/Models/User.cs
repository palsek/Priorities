using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AspNetIdentityTry1.Models
{
    public class User : IdentityUser
    {
        [Display(Name = "User name")]
        public override string UserName
        {
            get
            {
                return base.UserName;
            }
            set
            {
                base.UserName = value;
            }
        }

        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        public string Position { get; set; }

        public string Email { get; set; }

        [Display(Name = "Parent name")]
        public string ParentName { get; set; }
       // public ICollection<Item> Items { get; set; }
    }
}