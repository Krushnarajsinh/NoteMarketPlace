using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyNoteMarketPlace.Models
{
    public class DashboardViewModel
    {
        public IPagedList<SellerNotes> ProgressNotes { get; set; }
        public IPagedList<SellerNotes> PublishedNotes { get; set; }
        public int? MyDownloads { get; set; }
        public int? NumberOfSoldNotes { get; set; }
        public decimal? MoneyEarned { get; set; }
        public int? MyRejectedNotes { get; set; }
        public int? BuyerRequest { get; set; }
    }
}

