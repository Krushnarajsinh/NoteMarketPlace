using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyNoteMarketPlace.Models
{
    public class AdminNoteDetailModel
    {
        public int? UserID { get; set; }
        public SellerNotes SellerNote { get; set; }
        public IEnumerable<UserReviewsModel> NotesReview { get; set; }
        public int? AverageRating { get; set; }
        public int? TotalReview { get; set; }
        public int? TotalSpamReport { get; set; }


    }

    public class UserReviewsModel
    {
        public Users TblUser { get; set; }
        public UserProfile TblUserProfile { get; set; }
        public SellerNotesReviews TblSellerNotesReview { get; set; }
    }
}