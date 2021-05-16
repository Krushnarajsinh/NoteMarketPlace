using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNoteMarketPlace.Models
{
    public class MyDownloadModel
    {
        public int NoteID { get; set; }
        public int DownloadID { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Seller { get; set; }
        public string SellType { get; set; }
        public decimal? Price { get; set; }
        public DateTime? DownloadedDate { get; set; }
        public bool NoteDownloaded { get; set; }
        public int? ReviewID { get; set; }
        public decimal? Rating { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string Comment { get; set; }
    }
}