using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MyNoteMarketPlace.Models
{
    public class AddNotesViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "This field is required")]
      
        public string Title { get; set; }

        [Required(ErrorMessage = "This field is required")]
       
        public int Category { get; set; }

        
        public HttpPostedFileBase DisplayPicture { get; set; }

        [Required(ErrorMessage = "This field is required")]
        
        public HttpPostedFileBase[] UploadNotes { get; set; }

       
        public Nullable<int> NoteType { get; set; }

       
        public Nullable<int> NumberofPages { get; set; }

        [Required(ErrorMessage = "This field is required")]
      
        public string Description { get; set; }

       
        public string UniversityName { get; set; }

      
        public Nullable<int> Country { get; set; }

        
        public string Course { get; set; }

        public string CourseCode { get; set; }

      
        public string Professor { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public bool IsPaid { get; set; }

        public Nullable<decimal> SellingPrice { get; set; }

       
        public HttpPostedFileBase NotesPreview { get; set; }

        public IEnumerable<NoteCategories> NoteCategoryList { get; set; }

        public IEnumerable<NoteTypes> NoteTypeList { get; set; }

        public IEnumerable<Countries> CountryList { get; set; }
    }
}