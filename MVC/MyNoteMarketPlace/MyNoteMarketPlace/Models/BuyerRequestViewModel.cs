using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyNoteMarketPlace.Models
{
    public class BuyerRequestViewModel
    {
        public Downloads TblDownload { get; set; }
        public Users TblUser { get; set; }
        public UserProfile TblUserProfile { get; set; }
    }
}