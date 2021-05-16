﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyNoteMarketPlace.Models
{
    public class PublishedNoteModel
    {
        public int NoteID { get; set; }
        public int SellerID { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string SellType { get; set; }
        public decimal? Price { get; set; }
        public string Seller { get; set; }
        public DateTime PublishedDate { get; set; }
        public string ApprovedBy { get; set; }
        public int NumberOfDownloads { get; set; }
    }
}