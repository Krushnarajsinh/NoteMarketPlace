using MyNoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

// note status id... draft=6,  submitted for review=7, inreview=8,  published=9,  rejected=10 

namespace MyNoteMarketPlace.Controllers
{
    public class SellYourNotesController : Controller

    {
        readonly private Datebase1Entities context = new Datebase1Entities();
        // GET: Dashboard
        [HttpGet]
        [Authorize]
        [Route("SellYourNotes")]
        public ActionResult Dashboard(string search1, string search2, string sort1, string sort2, int page1 = 1, int page2 = 1)
        {
           
            ViewBag.SellYourNotes = "active";
            //Creating viewbag for searching, sorting and pagination
            ViewBag.Sort1 = sort1;
            ViewBag.Sort2 = sort2;
            ViewBag.Page1 = page1;
            ViewBag.Page2 = page2;
            ViewBag.Search1 = search1;
            ViewBag.Search2 = search2;

           
            DashboardViewModel dashboard = new DashboardViewModel();

          
            Users user =context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            //firstly, get number of sold notes, money earned, my downloads, my rejected notes and buyer request data from their tables
            dashboard.NumberOfSoldNotes = context.Downloads.Where(x => x.Seller == user.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).Count();
            dashboard.MoneyEarned = context.Downloads.Where(x => x.Seller == user.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).Select(x => x.PurchasedPrice).Sum();
            dashboard.MyDownloads = context.Downloads.Where(x => x.Downloader == user.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).Count();
            dashboard.MyRejectedNotes = context.SellerNotes.Where(x => x.SellerID == user.ID && x.Status == 10 && x.IsActive == true).Count();
            //seller doesn't allow for download and attachmentpath is null
            dashboard.BuyerRequest = context.Downloads.Where(x => x.Seller == user.ID && x.IsSellerHasAllowedDownload == false && x.AttachmentPath == null).Count();

          //If User Doesn't Search Yet
            if (string.IsNullOrEmpty(search1))
            {
                dashboard.ProgressNotes = context.SellerNotes.Where(x => x.SellerID == user.ID && (x.Status == 6 || x.Status == 7 || x.Status == 8));
            }
            //If User Search Something
            else
            {
                search1 = search1.ToLower();
                dashboard.ProgressNotes = context.SellerNotes.Where
                                                                     (
                                                                        x => x.SellerID == user.ID &&
                                                                        (x.Status == 6 || x.Status == 7 || x.Status == 8) &&
                                                                        (x.Title.ToLower().Contains(search1) || x.NoteCategories.Name.ToLower().Contains(search1) || x.ReferenceData.Value.ToLower().Contains(search1))
                                                                     );
            }
            
            if (string.IsNullOrEmpty(search2))
            {
                dashboard.PublishedNotes = context.SellerNotes.Where(x => x.SellerID == user.ID && x.Status == 9);
            }
           
            else
            {
                search2 = search2.ToLower();
                dashboard.PublishedNotes = context.SellerNotes.Where
                                                    (
                                                        x => x.SellerID == user.ID &&
                                                        x.Status == 9 &&
                                                        (x.Title.ToLower().Contains(search2) || x.NoteCategories.Name.ToLower().Contains(search2) || x.SellingPrice.ToString().ToLower().Contains(search2))
                                                    );
            }


            dashboard.ProgressNotes = SortInProgressNoteTable(sort1, dashboard.ProgressNotes);
            dashboard.PublishedNotes = SortPublishNoteTable(sort2, dashboard.PublishedNotes);

            
            ViewBag.TotalPagesInProgress = Math.Ceiling(dashboard.ProgressNotes.Count() / 5.0);
            ViewBag.TotalPagesInPublished = Math.Ceiling(dashboard.PublishedNotes.Count() / 5.0);


            dashboard.ProgressNotes = dashboard.ProgressNotes.Skip((page1 - 1) * 5).Take(5);
            dashboard.PublishedNotes = dashboard.PublishedNotes.Skip((page2 - 1) * 5).Take(5);

            return View(dashboard);
        }

       
        private IEnumerable<SellerNotes> SortInProgressNoteTable(string sorting, IEnumerable<SellerNotes> progressnotes)
        {
            switch (sorting)
            {
                case "CreatedDate_Asc":
                    {
                        progressnotes = progressnotes.OrderBy(x => x.CreatedDate);
                        break;
                    }
                case "CreatedDate_Desc":
                    {
                        progressnotes = progressnotes.OrderByDescending(x => x.CreatedDate);
                        break;
                    }
                case "Title_Asc":
                    {
                        progressnotes = progressnotes.OrderBy(x => x.Title);
                        break;
                    }
                case "Title_Desc":
                    {
                        progressnotes = progressnotes.OrderByDescending(x => x.Title);
                        break;
                    }
                case "Category_Asc":
                    {
                        progressnotes = progressnotes.OrderBy(x => x.NoteCategories.Name);
                        break;
                    }
                case "Category_Desc":
                    {
                        progressnotes = progressnotes.OrderByDescending(x => x.NoteCategories.Name);
                        break;
                    }
                case "Status_Asc":
                    {
                        progressnotes = progressnotes.OrderBy(x => x.ReferenceData.Value);
                        break;
                    }
                case "Status_Desc":
                    {
                        progressnotes = progressnotes.OrderByDescending(x => x.ReferenceData.Value);
                        break;
                    }
                default:
                    {
                        progressnotes = progressnotes.OrderByDescending(x => x.CreatedDate);
                        break;
                    }
            }
            return progressnotes;
        }

        
        private IEnumerable<SellerNotes> SortPublishNoteTable(string sorting, IEnumerable<SellerNotes> publishnotes)
        {
            switch (sorting)
            {
                case "Title_Asc":
                    {
                         publishnotes = publishnotes.OrderBy(x => x.Title);
                        break;
                    }
                case "Title_Desc":
                    {
                        publishnotes = publishnotes.OrderByDescending(x => x.Title);
                        break;
                    }
                case "Category_Asc":
                    {
                        publishnotes = publishnotes.OrderBy(x => x.NoteCategories.Name);
                        break;
                    }
                case "Category_Desc":
                    {
                        publishnotes = publishnotes.OrderByDescending(x => x.NoteCategories.Name);
                        break;
                    }
                case "PublishedDate_Asc":
                    {
                        publishnotes = publishnotes.OrderBy(x => x.PublishedDate);
                        break;
                    }
                case "PublishedDate_Desc":
                    {
                        publishnotes = publishnotes.OrderByDescending(x => x.PublishedDate);
                        break;
                    }
                case "IsPaid_Asc":
                    {
                        publishnotes = publishnotes.OrderBy(x => x.IsPaid);
                        break;
                    }
                case "IsPaid_Desc":
                    {
                        publishnotes = publishnotes.OrderByDescending(x => x.IsPaid);
                        break;
                    }
                case "SellingPrice_Asc":
                    {
                        publishnotes = publishnotes.OrderBy(x => x.SellingPrice);
                        break;
                    }
                case "SellingPrice_Desc":
                    {
                        publishnotes = publishnotes.OrderByDescending(x => x.SellingPrice);
                        break;
                    }
                default:
                    {
                        publishnotes = publishnotes.OrderByDescending(x => x.PublishedDate);
                        break;
                    }
            }
            return publishnotes;
        }

        [Authorize]
       // [Route("SellYourNotes/DeleteDraft/{id}")]
        public ActionResult DeleteDraft(int id)
        {
            
            SellerNotes note = context.SellerNotes.Where(x => x.ID == id && x.IsActive == true).FirstOrDefault();
            
            if (note == null)
            {
                return HttpNotFound();
            }
           
            IEnumerable<SellerNotesAttachements>  attachement = context.SellerNotesAttachements.Where(x => x.NoteID == id && x.IsActive == true).ToList();
            
            if (attachement.Count() == 0)
            {
                return HttpNotFound();
            }
           
            string notefolderpath = Server.MapPath("~/Members/" + note.SellerID + "/" + note.ID);
            string noteattachmentfolderpath = Server.MapPath("~/Members/" + note.SellerID + "/" + note.ID + "/Attachements");

          
            DirectoryInfo folder = new DirectoryInfo(notefolderpath);
            DirectoryInfo attachementfolder = new DirectoryInfo(noteattachmentfolderpath);
           
            //This Methods defines below  
            DeleteDirectory(attachementfolder);
            DeleteDirectory(folder);
            
            Directory.Delete(notefolderpath);

           //Delete Data From Database Also
            context.SellerNotes.Remove(note);

            
            foreach (var item in attachement)
            {
                SellerNotesAttachements noteattachement = context.SellerNotesAttachements.Where(x => x.ID == item.ID).FirstOrDefault();
                context.SellerNotesAttachements.Remove(noteattachement);
            }

          
            context.SaveChanges();

            return RedirectToAction("Dashboard");
        }
        
        private void DeleteDirectory(DirectoryInfo directory)
        {
            // check if directory have files
            if (directory.GetFiles() != null)
            {
                // delete all files of directory
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }
            }

            // check if directory have subdirectory
            if (directory.GetDirectories() != null)
            {
                // call emptyfolder and delete subdirectory of parent directory
                foreach (DirectoryInfo subdirectory in directory.GetDirectories())
                {
                    DeleteDirectory(subdirectory);
                    subdirectory.Delete();
                }
            }

        }

        [HttpGet]
        [Authorize]
        [Route("SellYourNotes/AddNotes")]
        public ActionResult AddNotes() {
            AddNotesViewModel Model = new AddNotesViewModel
            {
                NoteCategoryList = context.NoteCategories.Where(x => x.IsActive == true).ToList(),
                NoteTypeList = context.NoteTypes.Where(x => x.IsActive == true).ToList(),
                CountryList = context.Countries.Where(x => x.IsActive == true).ToList()
            };

            return View(Model);
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("SellYourNotes/AddNotes")]
        public ActionResult AddNotes(AddNotesViewModel addnotesmodel)
        {
            // check if upload note is null or not
            if (addnotesmodel.UploadNotes[0] == null)
            {
                ModelState.AddModelError("UploadNotes", "This field is required");
                return View(addnotesmodel);
            }
            // check and raise error when note preview is null for paid notes
            if (addnotesmodel.IsPaid == true && addnotesmodel.NotesPreview == null)
            {
                ModelState.AddModelError("NotesPreview", "This field is required if selling type is paid");
                return View(addnotesmodel);
            }
            // check model state
            if (ModelState.IsValid)
            {
                // create seller note object
                SellerNotes sellernotes = new SellerNotes();

                Users user = context.Users.FirstOrDefault(x => x.EmailID == User.Identity.Name);

                sellernotes.SellerID = user.ID;
                sellernotes.Title = addnotesmodel.Title;
                sellernotes.Status = 6;
                sellernotes.Category = addnotesmodel.Category;
                sellernotes.NoteType = addnotesmodel.NoteType;
                sellernotes.NumberofPages = addnotesmodel.NumberofPages;
                sellernotes.Description = addnotesmodel.Description;
                sellernotes.UniversityName = addnotesmodel.UniversityName;
                sellernotes.Country = addnotesmodel.Country;
                sellernotes.Course = addnotesmodel.Course;
                sellernotes.CourseCode = addnotesmodel.CourseCode;
                sellernotes.Professor = addnotesmodel.Professor;
                sellernotes.IsPaid = addnotesmodel.IsPaid;
                if (sellernotes.IsPaid)
                {
                    sellernotes.SellingPrice = addnotesmodel.SellingPrice;
                }
                else
                {
                    sellernotes.SellingPrice = 0;
                }
                sellernotes.CreatedDate = DateTime.Now;
                sellernotes.CreatedBy = user.ID;
                sellernotes.IsActive = true;

                // add note in database and save
                context.SellerNotes.Add(sellernotes);
                context.SaveChanges();

                // get seller note
                sellernotes = context.SellerNotes.Find(sellernotes.ID);

                // if display picture field is not null then save that picture into directory and save directory path into database
                if (addnotesmodel.DisplayPicture != null)
                {
                    string displaypicturefilename = System.IO.Path.GetFileName(addnotesmodel.DisplayPicture.FileName);
                    string displaypicturepath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/";  //this is vertual path it is convert into physical path using Server.MapPath()

                    CreateDirectoryIfMissing(displaypicturepath);

                    string displaypicturefilepath = Path.Combine(Server.MapPath(displaypicturepath), displaypicturefilename);
                    sellernotes.DisplayPicture = displaypicturepath + displaypicturefilename;
                    addnotesmodel.DisplayPicture.SaveAs(displaypicturefilepath);
                }

                // if note preview is not null then save picture into directory and directory path into database
                if (addnotesmodel.NotesPreview != null)
                {
                    string notespreviewfilename = System.IO.Path.GetFileName(addnotesmodel.NotesPreview.FileName);
                    string notespreviewpath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/";
                    CreateDirectoryIfMissing(notespreviewpath);
                    string notespreviewfilepath = Path.Combine(Server.MapPath(notespreviewpath), notespreviewfilename);
                    sellernotes.NotesPreview = notespreviewpath + notespreviewfilename;
                    addnotesmodel.NotesPreview.SaveAs(notespreviewfilepath);
                }

                // update note preview path and display picture path and save changes
                context.SellerNotes.Attach(sellernotes);
                context.Entry(sellernotes).Property(x => x.DisplayPicture).IsModified = true;
                context.Entry(sellernotes).Property(x => x.NotesPreview).IsModified = true;
                context.SaveChanges();

                // attachement files
                foreach (HttpPostedFileBase file in addnotesmodel.UploadNotes)
                {
                    // check if file is null or not
                    if (file != null)
                    {
                        // save file in directory
                        string notesattachementfilename = System.IO.Path.GetFileName(file.FileName);
                        string notesattachementpath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/Attachements/";
                        CreateDirectoryIfMissing(notesattachementpath);
                        string notesattachementfilepath = Path.Combine(Server.MapPath(notesattachementpath), notesattachementfilename);
                        file.SaveAs(notesattachementfilepath);

                        // create object of sellernotesattachement 
                        SellerNotesAttachements notesattachements = new SellerNotesAttachements
                        {
                            NoteID = sellernotes.ID,
                            FileName = notesattachementfilename,
                            FilePath = notesattachementpath,
                            CreatedDate = DateTime.Now,
                            CreatedBy = user.ID,
                            IsActive = true
                        };

                        // save seller notes attachement
                        context.SellerNotesAttachements.Add(notesattachements);
                        context.SaveChanges();
                    }
                }

                  return RedirectToAction("Dashboard", "SellYourNotes"); 
               
            }
            // if model state is not valid
            else
            {
                // create object of addnotesviewmodel
                AddNotesViewModel Model = new AddNotesViewModel
                {
                    NoteCategoryList = context.NoteCategories.Where(x => x.IsActive == true).ToList(),
                    NoteTypeList = context.NoteTypes.Where(x => x.IsActive == true).ToList(),
                    CountryList = context.Countries.Where(x => x.IsActive == true).ToList()
                };

                return View(Model);
            }
        }


        // create directory if it is not created
        private void CreateDirectoryIfMissing(string folderpath)
        {
            // check if directory exists
            bool folderalreadyexists = Directory.Exists(Server.MapPath(folderpath));
            // if directory does not exists then create it
            if (!folderalreadyexists)
                Directory.CreateDirectory(Server.MapPath(folderpath));
        }


        [HttpGet]
        [Authorize]
        [Route("SellYourNotes/EditNotes/{id}")]
        public ActionResult EditNotes(int id)
        {
            // get current user
            Users user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get sellernotes table details
            SellerNotes note = context.SellerNotes.Where(x => x.ID == id && x.IsActive == true && x.SellerID == user.ID).FirstOrDefault();
            // get noteattachement table details
            SellerNotesAttachements attachement = context.SellerNotesAttachements.Where(x => x.NoteID == id).FirstOrDefault();
            if (note != null)
            {
                // create object of edit note viewmodel
                EditNotesViewModel Model = new EditNotesViewModel
                {
                    ID = note.ID,
                    NoteID = note.ID,
                    Title = note.Title,
                    Category = note.Category,
                    Picture = note.DisplayPicture,
                    Note = attachement.FilePath,
                    NumberofPages = note.NumberofPages,
                    Description = note.Description,
                    NoteType = note.NoteType,
                    UniversityName = note.UniversityName,
                    Course = note.Course,
                    CourseCode = note.CourseCode,
                    Country = note.Country,
                    Professor = note.Professor,
                    IsPaid = note.IsPaid,
                    SellingPrice = note.SellingPrice,


                    Preview = note.NotesPreview,
                    NoteCategoryList = context.NoteCategories.Where(x => x.IsActive == true).ToList(),
                    NoteTypeList = context.NoteTypes.Where(x => x.IsActive == true).ToList(),
                    CountryList = context.Countries.Where(x => x.IsActive == true).ToList()
                };

                // return viewmodel to edit notes page
                return View(Model);
            }
            else
            {
                // if note not found
                return HttpNotFound();
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("SellYourNotes/EditNotes/{id}")]
        public ActionResult EditNotes(int id, EditNotesViewModel notes)
        {
            // check if model state is valid or not
            if (ModelState.IsValid)
            {
                // get current user
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();
                // get Seller notes table 
                var sellernotes = context.SellerNotes.Where(x => x.ID == id && x.IsActive == true && x.SellerID == user.ID).FirstOrDefault();
                // if sellernote null
                if (sellernotes == null)
                {
                    return HttpNotFound();
                }
                // check if note is paid or preview is not null
                if (notes.IsPaid == true && notes.Preview == null && sellernotes.NotesPreview == null)
                {
                    ModelState.AddModelError("NotesPreview", "This field is required if selling type is paid");
                    return View(notes);
                }
                // get note attachement 
                var notesattachement = context.SellerNotesAttachements.Where(x => x.NoteID == notes.NoteID && x.IsActive == true).ToList();

                // attache seller note object and update
                context.SellerNotes.Attach(sellernotes);
                sellernotes.Title = notes.Title;
                sellernotes.Category = notes.Category;
                sellernotes.NoteType = notes.NoteType;
                sellernotes.NumberofPages = notes.NumberofPages;
                sellernotes.Description = notes.Description;
                sellernotes.Country = notes.Country;
                sellernotes.UniversityName = notes.UniversityName;
                sellernotes.Course = notes.Course;
                sellernotes.CourseCode = notes.CourseCode;
                sellernotes.Professor = notes.Professor;
                if (notes.IsPaid == true)
                {
                    sellernotes.IsPaid = true;
                    sellernotes.SellingPrice = notes.SellingPrice;
                }
                else
                {
                    sellernotes.IsPaid = false;
                    sellernotes.SellingPrice = 0;
                }
                sellernotes.ModifiedDate = DateTime.Now;
                sellernotes.ModifiedBy = user.ID;
                context.SaveChanges();

                // if display picture is not null
                if (notes.DisplayPicture != null)
                {
                    // if note object has already previously uploaded picture then delete it
                    if (sellernotes.DisplayPicture != null)
                    {
                        string path = Server.MapPath(sellernotes.DisplayPicture);
                        FileInfo file = new FileInfo(path);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    // save updated profile picture in directory and save directory path in database
                    string displaypicturefilename = System.IO.Path.GetFileName(notes.DisplayPicture.FileName);
                    string displaypicturepath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/";

                    CreateDirectoryIfMissing(displaypicturepath);

                    string displaypicturefilepath = Path.Combine(Server.MapPath(displaypicturepath), displaypicturefilename);
                    sellernotes.DisplayPicture = displaypicturepath + displaypicturefilename;
                    notes.DisplayPicture.SaveAs(displaypicturefilepath);
                }

                // if note preview is not null
                if (notes.NotesPreview != null)
                {
                    // if note object has already previously uploaded note preview then delete it
                    if (sellernotes.NotesPreview != null)
                    {
                        string path = Server.MapPath(sellernotes.NotesPreview);
                        FileInfo file = new FileInfo(path);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    // save updated note preview in directory and save directory path in database
                    string notespreviewfilename = System.IO.Path.GetFileName(notes.NotesPreview.FileName);
                    string notespreviewpath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/";

                    CreateDirectoryIfMissing(notespreviewpath);

                    string notespreviewfilepath = Path.Combine(Server.MapPath(notespreviewpath), notespreviewfilename);
                    sellernotes.NotesPreview = notespreviewpath + notespreviewfilename;
                    notes.NotesPreview.SaveAs(notespreviewfilepath);
                }

                // check if seller upload notes or not
                if (notes.UploadNotes[0] != null)
                {
                    // if user upload notes then delete directory that have previously uploaded notes
                    string path = Server.MapPath(notesattachement[0].FilePath);
                    DirectoryInfo dir = new DirectoryInfo(path);
                    DeleteDirectory(dir);

                    // remove previously uploaded attachement from database
                    foreach (var item in notesattachement)
                    {
                        SellerNotesAttachements attachement = context.SellerNotesAttachements.Where(x => x.ID == item.ID).FirstOrDefault();
                        context.SellerNotesAttachements.Remove(attachement);
                    }

                    // add newly uploaded attachement in database and save it in database
                    foreach (HttpPostedFileBase file in notes.UploadNotes)
                    {
                        // check if file is null or not
                        if (file != null)
                        {
                            // save file in directory
                            string notesattachementfilename = System.IO.Path.GetFileName(file.FileName);
                            string notesattachementpath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/Attachements/";
                            CreateDirectoryIfMissing(notesattachementpath);
                            string notesattachementfilepath = Path.Combine(Server.MapPath(notesattachementpath), notesattachementfilename);
                            file.SaveAs(notesattachementfilepath);

                            // create object of sellernotesattachement 
                            SellerNotesAttachements notesattachements = new SellerNotesAttachements
                            {
                                NoteID = sellernotes.ID,
                                FileName = notesattachementfilename,
                                FilePath = notesattachementpath,
                                CreatedDate = DateTime.Now,
                                CreatedBy = user.ID,
                                IsActive = true
                            };

                            // save seller notes attachement
                            context.SellerNotesAttachements.Add(notesattachements);
                            context.SaveChanges();
                        }
                    }
                }

                return RedirectToAction("Dashboard", "SellYourNotes");
            }
            else
            {
                return RedirectToAction("EditNotes", new { id = notes.ID });
            }

        }



        [Authorize]
        public ActionResult PublishNote(int id)
        {
            // get note for perticular id
            var note = context.SellerNotes.Find(id);
            // if note is not found
            if (note == null)
            {
                return HttpNotFound();
            }
            // get current user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get seller name
            string seller = user.FirstName + " " + user.LastName;

            if (user.ID == note.SellerID)
            {
                // update note status from draft to review = 7
                context.SellerNotes.Attach(note);
                note.Status = 7;
                note.ModifiedDate = DateTime.Now;
                note.ModifiedBy = user.ID;
                context.SaveChanges();

                // send mail to admin for publish note request
                SendPublishedNoteMail(note.Title, seller);
            }

            return RedirectToAction("Dashboard");
        }

        // send mail to admin for publish note request
        public void SendPublishedNoteMail(string note, string seller)
        {
            

            // get support email
            var email = context.SystemConfigurations.Where(x => x.Key == "supportemail").FirstOrDefault();
           
            var receiver = context.SystemConfigurations.Where(x => x.Key == "adminemail").FirstOrDefault();
            var fromEmailPassword = "***********"; // Replace with your original password
            

            string from, to, subject;
            from = email.Value.Trim();
            to = receiver.Value.Trim();
            subject = seller + " sent his note for review";
            
            string body = "Hello Admins,<br/><br/>" +
                           " We want to inform you that," + ViewBag.SellerName + "sent his note" + ViewBag.NoteTitle +
                           "for review.Please look at the notes and take required actions." +
                           "<br/><br/>Regards,<br/>Notes Marketplace";



            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(from, fromEmailPassword)
            };


            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(from, "NotesMarketplace");
            mail.To.Add(new MailAddress(to));
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            smtp.Send(mail);

        }



    }
}