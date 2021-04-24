using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyNoteMarketPlace.Models
{
    public class NotesDetailsModel
    {
        public int? UserID { get; set; }
        public SellerNotes SellerNote { get; set; }
        public IEnumerable<ReviewsModel> NotesReview { get; set; }
        public int? AverageRating { get; set; }
        public int? TotalReview { get; set; }
        public int? TotalSpamReport { get; set; }
        public bool NoteRequested { get; set; }
        public bool AllowDownload { get; set; }
    }
    public class ReviewsModel
    {
        public Users TblUser { get; set; }
        public UserProfile TblUserProfile { get; set; }
        public SellerNotesReviews TblSellerNotesReview { get; set; }
    }
}