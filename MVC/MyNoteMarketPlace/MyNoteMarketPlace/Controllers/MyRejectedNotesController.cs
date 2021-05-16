using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyNoteMarketPlace.Controllers
{
    [OutputCache(Duration = 0)]
    public class MyRejectedNotesController : Controller
    {
        readonly private Datebase1Entities context = new Datebase1Entities();
        // GET: MyRejectedNotes
        [HttpGet]
        [Authorize(Roles = "Member")]
        [Route("User/MyRejectedNotes")]
        public ActionResult MyRejectedNotes(string search, string sort, int? page)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get rejected id from referencedata table
            var rejected_id = context.ReferenceData.Where(x => x.Value.ToLower() == "rejected").Select(x => x.ID).FirstOrDefault();

            // get user's rejected notes 
            IEnumerable<SellerNotes> rejectednotes = context.SellerNotes.Where(x => x.SellerID == user.ID && x.Status == rejected_id && x.IsActive == true).ToList();

            // get searched result if search is not empty
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                rejectednotes = rejectednotes.Where(x => x.Title.ToLower().Contains(search) ||
                                                     x.NoteCategories.Name.ToLower().Contains(search) ||
                                                     x.AdminRemarks.ToLower().Contains(search)).ToList();
            }

            // sort result
            rejectednotes = MyRejectedNotesTableSorting(sort, rejectednotes);

            /* viewbag for count total pages
            ViewBag.TotalPages = Math.Ceiling(rejectednotes.Count() / 10.0);

            // show result based on pagination
            rejectednotes = rejectednotes.Skip((page - 1) * 10).Take(10).ToList(); */

              var result = new List<SellerNotes>();
            result = rejectednotes.ToList();

            // return rejectedd notes result
           // return View(rejectednotes);

            return View(result.ToList().AsQueryable().ToPagedList(page ?? 1, 10));
        }
        //sorting for my rejected notes
        private IEnumerable<SellerNotes> MyRejectedNotesTableSorting(string sort, IEnumerable<SellerNotes> table)
        {
            switch (sort)
            {
                case "Title_Asc":
                    {
                        table = table.OrderBy(x => x.Title);
                        break;
                    }
                case "Title_Desc":
                    {
                        table = table.OrderByDescending(x => x.Title);
                        break;
                    }
                case "Category_Asc":
                    {
                        table = table.OrderBy(x => x.NoteCategories.Name);
                        break;
                    }
                case "Category_Desc":
                    {
                        table = table.OrderByDescending(x => x.NoteCategories.Name);
                        break;
                    }
                case "Remark_Asc":
                    {
                        table = table.OrderBy(x => x.AdminRemarks);
                        break;
                    }
                case "Remark_Desc":
                    {
                        table = table.OrderByDescending(x => x.AdminRemarks);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.ModifiedDate);
                        break;
                    }
            }
            return table;
        }

        [HttpGet]
        [Authorize(Roles = "Member")]
        [Route("User/MyRejectedNotes/{noteid}/Clone")]
        public ActionResult CloneNote(int noteid)
        {
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get rejected note by id
            var rejectednote = context.SellerNotes.Find(noteid);

            // create object of sellernotes table for create clone of note
            SellerNotes clonenote = new SellerNotes();

            clonenote.SellerID = rejectednote.SellerID;
            clonenote.Status = context.ReferenceData.Where(x => x.Value.ToLower() == "draft").Select(x => x.ID).FirstOrDefault();
            clonenote.Title = rejectednote.Title;
            clonenote.Category = rejectednote.Category;
            clonenote.NoteType = rejectednote.NoteType;
            clonenote.NumberofPages = rejectednote.NumberofPages;
            clonenote.Description = rejectednote.Description;
            clonenote.UniversityName = rejectednote.UniversityName;
            clonenote.Country = rejectednote.Country;
            clonenote.Course = rejectednote.Course;
            clonenote.CourseCode = rejectednote.CourseCode;
            clonenote.Professor = rejectednote.Professor;
            clonenote.IsPaid = rejectednote.IsPaid;
            clonenote.SellingPrice = rejectednote.SellingPrice;
            clonenote.CreatedBy = user.ID;
            clonenote.CreatedDate = DateTime.Now;
            clonenote.IsActive = true;

            // save note in database
            context.SellerNotes.Add(clonenote);
            context.SaveChanges();

            // get clonenote 
            clonenote = context.SellerNotes.Find(clonenote.ID);

            // if display picture is not null then copy file from rejected note's folder to clone note's new folder
            if (rejectednote.DisplayPicture != null)
            {
                var rejectednotefilepath = Server.MapPath(rejectednote.DisplayPicture);
                var clonenotefilepath = "~/Members/" + user.ID + "/" + clonenote.ID + "/";

                var filepath = Path.Combine(Server.MapPath(clonenotefilepath));
                Directory.CreateDirectory(filepath);

                FileInfo file = new FileInfo(rejectednotefilepath);

              
                if (file.Exists)
                {
                    System.IO.File.Copy(rejectednotefilepath, Path.Combine(filepath, Path.GetFileName(rejectednotefilepath)));
                }

                clonenote.DisplayPicture = Path.Combine(clonenotefilepath, Path.GetFileName(rejectednotefilepath));
                context.SaveChanges();
            }

            // if note preview is not null then copy file from rejected note's folder to clone note's new folder
            if (rejectednote.NotesPreview != null)
            {
                var rejectednotefilepath = Server.MapPath(rejectednote.NotesPreview);
                var clonenotefilepath = "~/Members/" + user.ID + "/" + clonenote.ID + "/";

                var filepath = Path.Combine(Server.MapPath(clonenotefilepath));
                Directory.CreateDirectory(filepath);

                FileInfo file = new FileInfo(rejectednotefilepath);

              

                if (file.Exists)
                {
                    System.IO.File.Copy(rejectednotefilepath, Path.Combine(filepath, Path.GetFileName(rejectednotefilepath)));
                }

                clonenote.NotesPreview = Path.Combine(clonenotefilepath, Path.GetFileName(rejectednotefilepath));
                context.SaveChanges();
            }

            // copy attachment path of rejected note to clone note
            var rejectednoteattachement = Server.MapPath("~/Members/" + user.ID + "/" + rejectednote.ID + "/Attachements/");
            var clonenoteattachement = "~/Members/" + user.ID + "/" + clonenote.ID + "/Attachements/";

            var attachementfilepath = Path.Combine(Server.MapPath(clonenoteattachement));

            // create directory for attachement folder
            Directory.CreateDirectory(attachementfilepath);

            // get attachements files from rejected note and copy to clone note
            foreach (var files in Directory.GetFiles(rejectednoteattachement))
            {

                FileInfo file = new FileInfo(files);

                if (file.Exists)
                {
                    System.IO.File.Copy(file.ToString(), Path.Combine(attachementfilepath, Path.GetFileName(file.ToString())));
                }

                // save attachment in database
                SellerNotesAttachements attachement = new SellerNotesAttachements();
                attachement.NoteID = clonenote.ID;
                attachement.FileName = Path.GetFileName(file.ToString());
                attachement.FilePath = clonenoteattachement;
                attachement.CreatedDate = DateTime.Now;
                attachement.CreatedBy = user.ID;
                attachement.IsActive = true;

                context.SellerNotesAttachements.Add(attachement);
                context.SaveChanges();
            }
            return RedirectToAction("Dashboard", "SellYourNotes");
        }
    }
}