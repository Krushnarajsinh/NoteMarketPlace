﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyNoteMarketPlace
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Datebase1Entities : DbContext
    {
        public Datebase1Entities()
            : base("name=Datebase1Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<UserRoles> UserRoles { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Countries> Countries { get; set; }
        public virtual DbSet<Downloads> Downloads { get; set; }
        public virtual DbSet<NoteCategories> NoteCategories { get; set; }
        public virtual DbSet<NoteTypes> NoteTypes { get; set; }
        public virtual DbSet<ReferenceData> ReferenceData { get; set; }
        public virtual DbSet<SellerNotesAttachements> SellerNotesAttachements { get; set; }
        public virtual DbSet<SellerNotesReportedIssues> SellerNotesReportedIssues { get; set; }
        public virtual DbSet<SellerNotesReviews> SellerNotesReviews { get; set; }
        public virtual DbSet<SystemConfigurations> SystemConfigurations { get; set; }
        public virtual DbSet<UserStates> UserStates { get; set; }
        public virtual DbSet<ContactUs> ContactUs { get; set; }
        public virtual DbSet<SellerNotes> SellerNotes { get; set; }
        public virtual DbSet<UserProfile> UserProfile { get; set; }
        public virtual DbSet<Admin> Admin { get; set; }
    }
}
