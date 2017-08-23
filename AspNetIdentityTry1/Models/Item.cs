using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetIdentityTry1.Models
{
    public class Item
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Priority Priority { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
      
        [Required]
        public string UserName { get; set; }

        [Required]
        public string ParentUserName { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        public Status Status { get; set; }

        [NotMapped]
        public string InvokingViewAction { get; set; }
    }

    public enum Priority
    {
        VeryLow, Low, Medium, High, VeryHigh
    }

    public enum Status
    { 
        New, InProgress, Done
    }
}