using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyNoteMarketPlace.Models
{
    public class AddNotesModel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AddNotesModel()
        {
            this.Downloads = new HashSet<Downloads>();
            this.SellerNotesAttachements = new HashSet<SellerNotesAttachements>();
            this.SellerNotesReportedIssues = new HashSet<SellerNotesReportedIssues>();
            this.SellerNotesReviews = new HashSet<SellerNotesReviews>();
        }

        public int ID { get; set; }
        public int SellerID { get; set; }
        public int Status { get; set; }
        public Nullable<int> ActionedBy { get; set; }
        public string AdminRemarks { get; set; }
        public Nullable<System.DateTime> PublishedDate { get; set; }
        public string Title { get; set; }
        public int Category { get; set; }
        public string DisplayPicture { get; set; }
        public Nullable<int> NoteType { get; set; }
        public Nullable<int> NumberofPages { get; set; }
        public string Description { get; set; }
        public string UniversityName { get; set; }
        public Nullable<int> Country { get; set; }
        public string Course { get; set; }
        public string CourseCode { get; set; }
        public string Professor { get; set; }
        public bool IsPaid { get; set; }
        public Nullable<decimal> SellingPrice { get; set; }
        public string NotesPreview { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public bool IsActive { get; set; }

        public virtual Users Users { get; set; }
        public virtual Users Users1 { get; set; }
        public virtual Countries Countries { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Downloads> Downloads { get; set; }
        public virtual NoteCategories NoteCategories { get; set; }
        public virtual NoteTypes NoteTypes { get; set; }
        public virtual ReferenceData ReferenceData { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SellerNotesAttachements> SellerNotesAttachements { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SellerNotesReportedIssues> SellerNotesReportedIssues { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SellerNotesReviews> SellerNotesReviews { get; set; }
    }
}