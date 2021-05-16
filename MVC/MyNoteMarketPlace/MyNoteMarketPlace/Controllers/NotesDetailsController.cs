using MyNoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc; 



namespace MyNoteMarketPlace.Controllers
{
    [OutputCache(Duration = 0)]
    public class NotesDetailsController : Controller
    {
        readonly private Datebase1Entities context = new Datebase1Entities();
        // GET: NotesDetails
        [Route("NotesDetails/Notes/{id}")]
        [AllowAnonymous]
        public ActionResult Notes(int id)
        {
            // get logged in user if user is logged in 
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();
            // get note by id
            var NoteDetail = context.SellerNotes.Where(x => x.ID == id && x.IsActive == true).FirstOrDefault();
            // if note is not found
            if (NoteDetail == null)
            {
                return HttpNotFound();
            }
            // get reviews and user's full name and user's image (We are taking those users whose given a reviews )
            IEnumerable<ReviewsModel> reviews = from review in context.SellerNotesReviews
                                                    join users in context.Users on review.ReviewedByID equals users.ID
                                                    join userprofile in context.UserProfile on review.ReviewedByID equals userprofile.User_ID
                                                    where review.NoteID == id && review.IsActive == true
                                                    orderby review.Ratings descending
                                                    select new ReviewsModel { TblSellerNotesReview = review, TblUser = users, TblUserProfile = userprofile };
            // count reviews
            var reviewcounts = reviews.Count();
            // count average review
            decimal avgreview = 0;
            if (reviewcounts > 0)
            {
                avgreview = Math.Ceiling((from x in reviews select x.TblSellerNotesReview.Ratings).Average());
            }
            // count total spam report by calculating total entry in SellerNotesReportedIssues
            var spams = context.SellerNotesReportedIssues.Where(x => x.NoteID == id).Count();
            // create notesdetailviewmodel object
            NotesDetailsModel notesdetail = new NotesDetailsModel();
            //if user is authenticated
            if (user != null)
            {
                notesdetail.UserID = user.ID;
            }
            notesdetail.SellerNote = NoteDetail;
            notesdetail.NotesReview = reviews;
            notesdetail.AverageRating = Convert.ToInt32(avgreview);  // converting decimal to int
            notesdetail.TotalReview = reviewcounts;
            notesdetail.TotalSpamReport = spams;
            // check if user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                // check if this note is already requested by logged in user or not
                // if it's already requested then we need to hide download button until seller allows download
                var request =context.Downloads.Where(x => x.NoteID == id && x.Downloader == user.ID && x.IsSellerHasAllowedDownload == false && x.AttachmentPath == null).FirstOrDefault();
                // if logged in user is allow download this note
                var allowdownloadnotes = context.Downloads.Where(x => x.NoteID == id && x.Downloader == user.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).FirstOrDefault();

                // assign values according to if user is already requested note or allowdownload
                if (request == null && allowdownloadnotes == null)
                {
                    notesdetail.NoteRequested = false;
                }
                else
                {
                    notesdetail.NoteRequested = true;
                }

                if (allowdownloadnotes != null && request == null)
                {
                    notesdetail.AllowDownload = true;
                }
                else
                {
                    notesdetail.AllowDownload = false;
                }
            }

            return View(notesdetail);
        }

        [Authorize(Roles = "Member")]
        [Route("NotesDetails/Notes/{noteid}/Download")]
        public ActionResult DownloadNotes(int noteid)
        {
            // get note from SellerNotes table
            var note = context.SellerNotes.Find(noteid);
            // if note is not found
            if (note == null)
            {
                return HttpNotFound();
            }
            // get first object of seller note attachement for attachement
            var noteattachement = context.SellerNotesAttachements.Where(x => x.NoteID == note.ID).FirstOrDefault();
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // variable for attachement path
            string path;

            // note's seller if and logged in user's id is same it means user want to download his own book 
            // then we need to provide downloaded note without entry in download tables
            if (note.SellerID == user.ID)
            {
                // get attavhement path
                path = Server.MapPath(noteattachement.FilePath);

                DirectoryInfo dir = new DirectoryInfo(path);
                // create zip of attachement
                using (var memoryStream = new MemoryStream())
                {
                    using (var ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var item in dir.GetFiles())
                        {
                            // file path is attachement path + file name
                            string filepath = path + item.ToString();
                            ziparchive.CreateEntryFromFile(filepath, item.ToString());
                        }
                    }
                    // return zip
                    return File(memoryStream.ToArray(), "application/zip", note.Title + ".zip");
                }
            }

            // if note is free then we need to add entry in download table with allow download is true
            // downloaded date time is current date time for first time download
            // if user download again then we have to return zip of attachement without changes in data
            if (note.IsPaid == false)
            {
                // if user has already downloaded note then get download object
                var downloadfreenote = context.Downloads.Where(x => x.NoteID == noteid && x.Downloader == user.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).FirstOrDefault();
                // if user is not downloaded 
                if (downloadfreenote == null)
                {
                    // create download object
                    Downloads download = new Downloads
                    {
                        NoteID = note.ID,
                        Seller = note.SellerID,
                        Downloader = user.ID,
                        IsSellerHasAllowedDownload = true,
                        AttachmentPath = noteattachement.FilePath,
                        IsAttachmentDownloaded = true,
                        AttachmentDownloadedDate = DateTime.Now,
                        IsPaid = note.IsPaid,
                        PurchasedPrice = note.SellingPrice,
                        NoteTitle = note.Title,
                        NoteCategory = note.NoteCategories.Name,
                        CreatedDate = DateTime.Now,
                        CreatedBy = user.ID,
                    };

                    // save download object in database
                    context.Downloads.Add(download);
                    context.SaveChanges();

                    path = Server.MapPath(download.AttachmentPath);
                }
                // if user is already downloaded note then get attachement path
                else
                {
                    path = Server.MapPath(downloadfreenote.AttachmentPath);
                }

                DirectoryInfo dir = new DirectoryInfo(path);
                // create zip of attachement
                using (var memoryStream = new MemoryStream())
                {
                    using (var ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var item in dir.GetFiles())
                        {
                            // file path is attachement path + file name
                            string filepath = path + item.ToString();
                            ziparchive.CreateEntryFromFile(filepath, item.ToString());
                        }
                    }
                    // return zip
                    return File(memoryStream.ToArray(), "application/zip", note.Title + ".zip");
                }
            }
            // if note is paid 
            else
            {
                // get download object
                var downloadpaidnote = context.Downloads.Where(x => x.NoteID == noteid && x.Downloader == user.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).FirstOrDefault();

                // if user is not already downloaded
                if (downloadpaidnote != null)
                {
                    // if user is download note first time then we need to update following record in download table 
                    if (downloadpaidnote.IsAttachmentDownloaded == false)
                    {
                        downloadpaidnote.AttachmentDownloadedDate = DateTime.Now;
                        downloadpaidnote.IsAttachmentDownloaded = true;
                        downloadpaidnote.ModifiedDate = DateTime.Now;
                        downloadpaidnote.ModifiedBy = user.ID;

                        // update ans save data in database
                        context.Entry(downloadpaidnote).State = EntityState.Modified;
                        context.SaveChanges();
                    }

                    // get attachement path
                    path = Server.MapPath(downloadpaidnote.AttachmentPath);

                    DirectoryInfo dir = new DirectoryInfo(path);

                    // create zip of attachement
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                        {
                            foreach (var item in dir.GetFiles())
                            {
                                // file path is attachement path + file name
                                string filepath = path + item.ToString();
                                ziparchive.CreateEntryFromFile(filepath, item.ToString());
                            }
                        }
                        // return zip
                        return File(memoryStream.ToArray(), "application/zip", note.Title + ".zip");
                    }
                }
                return RedirectToAction("Notes", "NotesDetails", new { id = noteid });
            }
        }

        [Authorize(Roles = "Member")]
        [Route("NotesDetails/Notes/{noteid}/Request")]
        public ActionResult RequestPaidNotes(int noteid)
        {
            // get note
            var note = context.SellerNotes.Find(noteid);
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // create download object
            Downloads download = new Downloads
            {
                NoteID = note.ID,
                Seller = note.SellerID,
                Downloader = user.ID,
                IsSellerHasAllowedDownload = false,
                IsAttachmentDownloaded = false,
                IsPaid = note.IsPaid,
                PurchasedPrice = note.SellingPrice,
                NoteTitle = note.Title,
                NoteCategory = note.NoteCategories.Name,
                CreatedDate = DateTime.Now,
                CreatedBy = user.ID,
            };

            // add and save data in database
            context.Downloads.Add(download);
            context.SaveChanges();

            TempData["ShowModal"] = 1;
            

            // sned mail
            RequestPaidNotesTemplate(download, user);

            return RedirectToAction("Notes", new { id = note.ID });
        }

        // for request to download we need to send mail to seller 
        public void RequestPaidNotesTemplate(Downloads download, Users user)
        {
            
           
            //get seller
            var seller = context.Users.Where(x => x.ID == download.Seller).FirstOrDefault();
            // get support email
            var fromemail = context.SystemConfigurations.Where(x => x.Key == "supportemail").FirstOrDefault();

            // set from, to, subject, body
            string from, to, subject;
            from = fromemail.Value.Trim();
            var fromEmailPassword = "********"; // Replace with original password
            to = seller.EmailID.Trim();
            subject = user.FirstName + " wants to purchase your notes";
            string body = "Hello " + seller.FirstName + "," +
                "<br/><br/>We would like to inform you that, " + user.FirstName + " wants to purchase your notes. Please see" +
                "<br/> Buyer Requests tab and allow download access to Buyer if you have received the payment from" +
                "<br/> him." +
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