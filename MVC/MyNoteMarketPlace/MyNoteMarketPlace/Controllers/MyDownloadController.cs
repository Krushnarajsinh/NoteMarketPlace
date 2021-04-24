using MyNoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MyNoteMarketPlace.Controllers
{
    [RoutePrefix("User")]
    public class MyDownloadController : Controller
    {
        readonly private Datebase1Entities context = new Datebase1Entities();
        // GET: MyDownload
        [HttpGet]
        [Authorize]
        [Route("MyDownload")]
        public ActionResult MyDownload(string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get downloaded notes
            IEnumerable<MyDownloadModel> mydownloads = from download in context.Downloads
                                                            join users in context.Users on download.Seller equals users.ID
                                                            join review in context.SellerNotesReviews on download.ID equals review.AgainstDownloadsID into r
                                                            from notereview in r.DefaultIfEmpty()
                                                            where download.Downloader == user.ID && download.IsSellerHasAllowedDownload == true && download.AttachmentPath != null
                                                            select new MyDownloadModel
                                                            {
                                                                NoteID = download.NoteID,
                                                                DownloadID = download.ID,
                                                                Title = download.NoteTitle,
                                                                Category = download.NoteCategory,
                                                                Seller = users.EmailID,
                                                                SellType = download.IsPaid == true ? "Paid" : "Free",
                                                                Price = download.PurchasedPrice,
                                                                DownloadedDate = download.AttachmentDownloadedDate.Value,
                                                                NoteDownloaded = download.IsAttachmentDownloaded,
                                                                ReviewID = notereview.ID,
                                                                Rating = notereview.Ratings,
                                                                Comment = notereview.Comments
                                                            };

            // get searched result if search is not empty
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                mydownloads = mydownloads.Where(x => x.Title.ToLower().Contains(search) ||
                                                     x.Category.ToLower().Contains(search) ||
                                                     x.Seller.ToLower().Contains(search) ||
                                                     x.Price.ToString().ToLower().Contains(search) ||
                                                     x.SellType.ToLower().Contains(search)
                                               );
            }

            // sorting result
            mydownloads = MyDownloadTableSorting(sort, mydownloads);

            // viewbag for count total pages
            ViewBag.TotalPages = Math.Ceiling(mydownloads.Count() / 10.0);

            // show result based on pagination
            mydownloads = mydownloads.Skip((page - 1) * 10).Take(10);

            // return results
            return View(mydownloads);
        }

        // sorting for my downloads results
        private IEnumerable<MyDownloadModel> MyDownloadTableSorting(string sort, IEnumerable<MyDownloadModel> table)
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
                        table = table.OrderBy(x => x.Category);
                        break;
                    }
                case "Category_Desc":
                    {
                        table = table.OrderByDescending(x => x.Category);
                        break;
                    }
                case "Seller_Asc":
                    {
                        table = table.OrderBy(x => x.Seller);
                        break;
                    }
                case "Seller_Desc":
                    {
                        table = table.OrderByDescending(x => x.Seller);
                        break;
                    }
                case "Type_Asc":
                    {
                        table = table.OrderBy(x => x.SellType);
                        break;
                    }
                case "Type_Desc":
                    {
                        table = table.OrderByDescending(x => x.SellType);
                        break;
                    }
                case "Price_Asc":
                    {
                        table = table.OrderBy(x => x.Price);
                        break;
                    }
                case "Price_Desc":
                    {
                        table = table.OrderByDescending(x => x.Price);
                        break;
                    }
                case "DownloadedDate_Asc":
                    {
                        table = table.OrderBy(x => x.DownloadedDate);
                        break;
                    }
                case "DownloadedDate_Desc":
                    {
                        table = table.OrderByDescending(x => x.DownloadedDate);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.DownloadedDate);
                        break;
                    }
            }
            return table;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [Route("Note/AddReview")]
        public ActionResult AddReview(SellerNotesReviews notereview)
        {
            // check if comment is null or not
            if (String.IsNullOrEmpty(notereview.Comments))
            {
                return RedirectToAction("MyDownload");
            }

            // check if rating is between 1 to 5
            if (notereview.Ratings < 1 || notereview.Ratings > 5)
            {
                return RedirectToAction("MyDownload");
            }

            // get Download object for check if user is downloaded note or not
            var notedownloaded = context.Downloads.Where(x => x.ID == notereview.AgainstDownloadsID && x.IsAttachmentDownloaded == true).FirstOrDefault();

            // user can provide review after downloading the note
            if (notedownloaded != null)
            {
                //get logged in user
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // check if user already provided review or not
                var alreadyprovidereview = context.SellerNotesReviews.Where(x => x.AgainstDownloadsID == notereview.AgainstDownloadsID && x.IsActive == true).FirstOrDefault();

                // if user not provide review then add review
                if (alreadyprovidereview == null)
                {
                    // create a sellernotesreview object and initialize it
                    SellerNotesReviews review = new SellerNotesReviews();

                    review.NoteID = notereview.NoteID;
                    review.AgainstDownloadsID = notereview.AgainstDownloadsID;
                    review.ReviewedByID = user.ID;
                    review.Ratings = notereview.Ratings;
                    review.Comments = notereview.Comments;
                    review.CreatedDate = DateTime.Now;
                    review.CreatedBy = user.ID;
                    review.IsActive = true;

                    // save sellernotesreview into database
                    context.SellerNotesReviews.Add(review);
                    context.SaveChanges();

                    return RedirectToAction("MyDownload");
                }
                // if user is already provided review then edit it
                else
                {
                    alreadyprovidereview.Ratings = notereview.Ratings;
                    alreadyprovidereview.Comments = notereview.Comments;
                    alreadyprovidereview.ModifiedDate = DateTime.Now;
                    alreadyprovidereview.ModifiedBy = user.ID;

                    // update review and save in database
                    context.Entry(alreadyprovidereview).State = EntityState.Modified;
                    context.SaveChanges();

                    return RedirectToAction("MyDownload");
                }
            }
            return RedirectToAction("MyDownload");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [Route("Note/ReportSpam")]
        public ActionResult SpamReport(FormCollection form)
        {
            // get download id by form 
            int downloadid = Convert.ToInt32(form["downloadid"]);

            // get ReportedIssues object 
            var alreadyreportedspam = context.SellerNotesReportedIssues.Where(x => x.AgainstDownloadID == downloadid).FirstOrDefault();

            // if user not slready reported spam 
            if (alreadyreportedspam == null)
            {
                //get logged in user
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // store logged in user name into variable
                string membername = user.FirstName + " " + user.LastName;

                // create a spam report object
                SellerNotesReportedIssues spamnote = new SellerNotesReportedIssues();

                spamnote.NoteID = Convert.ToInt32(form["noteid"]);
                spamnote.AgainstDownloadID = downloadid;
                spamnote.ReportedBYID = user.ID;
                spamnote.Remarks = form["spamreport"];
                spamnote.CreatedDate = DateTime.Now;
                spamnote.CreatedBy = user.ID;

                // save spam report object into database
                context.SellerNotesReportedIssues.Add(spamnote);
                context.SaveChanges();

                // send mail to admin that buyer reported the notes as inappropriate
                SpamReportTemplate(spamnote, membername);
            }
            return RedirectToAction("MyDownload");
        }

        // intializing the template that we want to send to admin for marking note as inappropriate
        private void SpamReportTemplate(SellerNotesReportedIssues spam, string membername)
        {
            // take from, to, subject, body
            string from, to, body, subject;

            // get notes and user by using SellerNotesReportedIssue object
            var note = context.SellerNotes.Find(spam.NoteID);
            var seller = context.Users.Find(note.SellerID);

          

            // get support email
            var fromemail = context.SystemConfigurations.Where(x => x.Key == "supportemail").FirstOrDefault();
            var fromEmailPassword = "rathod8055"; // Replace with original password
            var tomail = context.SystemConfigurations.Where(x => x.Key == "adminemail").FirstOrDefault();

            // set from, to, subject, body
            from = fromemail.Value.Trim();
            to = tomail.Value.Trim();

            subject = membername + " Reported an issue for " + note.Title;
            body= "Hello Admins" + "," +
                "<br/><br/>We want to inform you that, " + membername + " Reported an issue for " + seller.FirstName + " " + seller.LastName + "’s Note with " +
                "<br/>title " +note.Title+ ". Please look at the notes and take required actions. " +
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