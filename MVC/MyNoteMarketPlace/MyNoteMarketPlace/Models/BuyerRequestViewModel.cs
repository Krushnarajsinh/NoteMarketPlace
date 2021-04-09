using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyNoteMarketPlace.Models
{
    public class BuyerRequestViewModel
    {
        public Downloads download { get; set; }
        public Users User { get; set; }
        public UserProfile userProfile { get; set; }
    }
}