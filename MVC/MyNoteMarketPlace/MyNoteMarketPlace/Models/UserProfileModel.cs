using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNoteMarketPlace.Models
{
    public class UserProfileModel
    {

        public int UserID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string LastName { get; set; }

       
        [Required(ErrorMessage = "This field is required")]
        public string Email { get; set; }

      
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> DOB { get; set; }

       
        public Nullable<int> Gender { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string PhoneNumberCountryCode { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression("[0-9]+", ErrorMessage = "Only Numbers allowed")]
        public string PhoneNumber { get; set; }

      
        public HttpPostedFileBase ProfilePicture { get; set; }

      
        [Required(ErrorMessage = "This field is required")]
        public string AddressLine1 { get; set; }

     
        [Required(ErrorMessage = "This field is required")]
        public string AddressLine2 { get; set; }

       
        [Required(ErrorMessage = "This field is required")]
        public string City { get; set; }

      
        [Required(ErrorMessage = "This field is required")]
        public string State { get; set; }

    
        [Required(ErrorMessage = "This field is required")]
        public string ZipCode { get; set; }

       
        [Required(ErrorMessage = "This field is required")]
        public string Country { get; set; }

       
        public string University { get; set; }

       
        public string College { get; set; }
        public IEnumerable<Countries> CountryList { get; set; }
        public IEnumerable<ReferenceData> GenderList { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}