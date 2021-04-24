using MyNoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyNoteMarketPlace.Controllers
{
    public class MySoldNotesController : Controller
    {
        readonly private Datebase1Entities context = new Datebase1Entities();
        // GET: MySoldNotes
        [HttpGet]
        [Authorize]
        [Route("User/MySoldNotes")]
        public ActionResult  MySoldNotes(string search, string sort, int page = 1)
        {
            //for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            //get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            //get my sold notes
            IEnumerable<MySoldNotesModel> mysoldnotes = from download in context.Downloads
                                                            join users in context.Users on download.Downloader equals users.ID
                                                            where download.Seller == user.ID && download.IsSellerHasAllowedDownload == true && download.AttachmentPath != null
                                                            select new MySoldNotesModel
                                                            {
                                                                DownloadID = download.ID,
                                                                NoteID = download.NoteID,
                                                                Title = download.NoteTitle,
                                                                Category = download.NoteCategory,
                                                                Buyer = users.EmailID,
                                                                SellType = download.IsPaid == true ? "Paid" : "Free",
                                                                Price = download.PurchasedPrice,
                                                                DownloadedDate = download.AttachmentDownloadedDate.Value,
                                                                NoteDownloaded = download.IsAttachmentDownloaded
                                                            };

            //get searched result if search is not empty
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                mysoldnotes = mysoldnotes.Where(x => x.Title.ToLower().Contains(search) ||
                                                     x.Category.ToLower().Contains(search) ||
                                                     x.Buyer.ToLower().Contains(search) ||
                                                     x.Price.ToString().ToLower().Contains(search) ||
                                                     x.SellType.ToLower().Contains(search)
                                               ).ToList();
            }

            //sort result
            mysoldnotes = MySoldNotesTableSorting(sort, mysoldnotes);

            //count total pages
            ViewBag.TotalPages = Math.Ceiling(mysoldnotes.Count() / 10.0);

            //show result based on pagination
            mysoldnotes = mysoldnotes.Skip((page - 1) * 10).Take(10);

            return View(mysoldnotes);
        }

        //sorting for my my sold notes
        private IEnumerable<MySoldNotesModel> MySoldNotesTableSorting(string sort, IEnumerable<MySoldNotesModel> table)
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
                case "Buyer_Asc":
                    {
                        table = table.OrderBy(x => x.Buyer);
                        break;
                    }
                case "Buyer_Desc":
                    {
                        table = table.OrderByDescending(x => x.Buyer);
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
    }
}