using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace MyNoteMarketPlace.Models
{
    using System;
    using System.Collections.Generic;
    public class UserLogin
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        [Required]
        [EmailAddress]
        public string EmailID { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }


    }
}