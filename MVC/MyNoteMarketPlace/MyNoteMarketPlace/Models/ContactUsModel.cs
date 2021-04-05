using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNoteMarketPlace.Models
{
    public class ContactUsModel
    {
        public int ID { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string EmailID { get; set; }
        [Required]
        public string Subjects { get; set; }
        [Required]
        public string Comments { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public bool IsActive { get; set; }
    }
}