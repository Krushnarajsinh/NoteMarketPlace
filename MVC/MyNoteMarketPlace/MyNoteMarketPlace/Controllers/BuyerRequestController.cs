using MyNoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MyNoteMarketPlace.Controllers
{
    public class BuyerRequestController : Controller
    {
        readonly private Datebase1Entities context = new Datebase1Entities();
        private string to;

        [Authorize]
        [Route("BuyerRequest")]
        // GET: BuyerRequest
        public ActionResult BuyerRequest(string search, string sort, int page = 1)
        {
           
            ViewBag.BuyerRequest = "active";

           
            ViewBag.Sort = sort;
            ViewBag.Search = search;
            ViewBag.PageNumber = page;

            //get logged in user
            Users user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get buyer requests
            IEnumerable<BuyerRequestViewModel> buyerrequest = from download in context.Downloads
                                                              join users in context.Users on download.Downloader equals users.ID
                                                              join userprofile in context.UserProfile on download.Downloader equals userprofile.User_ID
                                                              where download.Seller == user.ID && download.IsSellerHasAllowedDownload == false && download.AttachmentPath == null
                                                              select new BuyerRequestViewModel { download = download, User = users, userProfile = userprofile };

            // if search is not empty
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                buyerrequest = buyerrequest.Where(
                                                    x => x.download.NoteTitle.ToLower().Contains(search) ||
                                                         x.download.NoteCategory.ToLower().Contains(search) ||
                                                         x.User.EmailID.ToLower().Contains(search) ||
                                                         x.download.PurchasedPrice.ToString().ToLower().Contains(search) ||
                                                         x.userProfile.Phone_number.ToLower().Contains(search)
                                                 ).ToList();
            }

          
            buyerrequest = SortBuyerRequestTable(sort, buyerrequest);
            // get total pages of buyerrequest
            ViewBag.TotalPages = Math.Ceiling(buyerrequest.Count() / 10.0);
            // get result according to pagination
            buyerrequest = buyerrequest.Skip((page - 1) * 10).Take(10);

            return View(buyerrequest);
        }

        private IEnumerable<BuyerRequestViewModel> SortBuyerRequestTable(string sort, IEnumerable<BuyerRequestViewModel> buyerrequest)
        {
            switch (sort)
            {
                case "Title_Asc":
                    {
                        buyerrequest = buyerrequest.OrderBy(x => x.download.NoteTitle);
                        break;
                    }
                case "Title_Desc":
                    {
                        buyerrequest = buyerrequest.OrderByDescending(x => x.download.NoteTitle);
                        break;
                    }
                case "Category_Asc":
                    {
                        buyerrequest = buyerrequest.OrderBy(x => x.download.NoteCategory);
                        break;
                    }
                case "Category_Desc":
                    {
                        buyerrequest = buyerrequest.OrderByDescending(x => x.download.NoteCategory);
                        break;
                    }
                case "Buyer_Asc":
                    {
                        buyerrequest = buyerrequest.OrderBy(x => x.User.EmailID);
                        break;
                    }
                case "Buyer_Desc":
                    {
                        buyerrequest = buyerrequest.OrderByDescending(x => x.User.EmailID);
                        break;
                    }
                case "Phone_Asc":
                    {
                        buyerrequest = buyerrequest.OrderBy(x => x.userProfile.Phone_number);
                        break;
                    }
                case "Phone_Desc":
                    {
                        buyerrequest = buyerrequest.OrderByDescending(x => x.userProfile.Phone_number);
                        break;
                    }
                case "Type_Asc":
                    {
                        buyerrequest = buyerrequest.OrderBy(x => x.download.IsPaid);
                        break;
                    }
                case "Type_Desc":
                    {
                        buyerrequest = buyerrequest.OrderByDescending(x => x.download.IsPaid);
                        break;
                    }
                case "Price_Asc":
                    {
                        buyerrequest = buyerrequest.OrderBy(x => x.download.PurchasedPrice);
                        break;
                    }
                case "Price_Desc":
                    {
                        buyerrequest = buyerrequest.OrderByDescending(x => x.download.PurchasedPrice);
                        break;
                    }
                case "DownloadedDate_Asc":
                    {
                        buyerrequest = buyerrequest.OrderBy(x => x.download.CreatedDate);
                        break;
                    }
                case "DownloadedDate_Desc":
                    {
                        buyerrequest = buyerrequest.OrderByDescending(x => x.download.CreatedDate);
                        break;
                    }
                default:
                    {
                        buyerrequest = buyerrequest.OrderByDescending(x => x.download.CreatedDate);
                        break;
                    }
            }
            return buyerrequest;
        }

        [Authorize]
        [Route("BuyerRequest/AllowDownload/{id}")]
        public ActionResult AllowDownload(int id)
        {
           
            Users user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();
            
            Downloads download = context.Downloads.Find(id);
            
            if (user.ID == download.Seller)
            {
               
                SellerNotesAttachements attachement = context.SellerNotesAttachements.Where(x => x.NoteID == download.NoteID && x.IsActive == true).FirstOrDefault();

               
                context.Downloads.Attach(download);
                download.IsSellerHasAllowedDownload = true;
                download.AttachmentPath = attachement.FilePath;
                download.ModifiedBy = user.ID;
                download.ModifiedDate = DateTime.Now;
                context.SaveChanges();

             
                SendMailForAllowDownload(download, user);

                return RedirectToAction("BuyerRequest");

            }
            else
            {
                return RedirectToAction("BuyerRequest");

            }
        }

        public void SendMailForAllowDownload(Downloads download, Users seller)
        {
            var downloader = context.Users.Where(x => x.ID == download.Downloader).FirstOrDefault();
            SystemConfigurations syst = new SystemConfigurations();

          /*  var fromEmail = new MailAddress("rathodkrushnaraj8055@gmail.com"); */
            var email = context.SystemConfigurations.Where(x => x.Key == "supportemail").FirstOrDefault();
            var fromEmail=email.Value.Trim();
            /* var toEmail = new MailAddress(downloader.EmailID); */
            var toEmail = downloader.EmailID.Trim();
            var fromEmailPassword = "********"; // Replace with original password
            string subject = seller.FirstName + "Allows you to download a note";
            string body = "Hello" + downloader.FirstName + "," +
                "<br/><br/>We would like to inform you that, " + seller.FirstName + "Allows you to download a note." +
                "<br/> Please login and see My Download tabs to download particular note. " +
                "<br/><br/>Regardds,<br/>Notes Marketplace";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail, fromEmailPassword)
            };

          
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromEmail, "NotesMarketplace");
            mail.To.Add(new MailAddress(toEmail));
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            smtp.Send(mail);


        }
        }
}