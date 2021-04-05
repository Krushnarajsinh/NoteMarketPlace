using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MyNoteMarketPlace.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string EmailID { get; set; }
    }
}