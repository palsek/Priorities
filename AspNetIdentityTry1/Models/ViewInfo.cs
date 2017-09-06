using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspNetIdentityTry1.Models
{
    public class ViewInfo
    {
        public int Page { get; set; }
        public int NumberPerPage { get; set; }
        public int AllPageNumber { get; set; }
        public string OrderBy { get; set; }
        public string OrderDirection { get; set; }        

        // for admin functionality
        public List<string> AllUsersName { get; set; }
        public string UserName { get; set; }
    }
}