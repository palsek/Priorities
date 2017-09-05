using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspNetIdentityTry1.Models
{
    public class ItemsData
    {
        public List<Item> Items { get; set; }
        public ViewInfo ViewInfo { get; set; }
    }
}