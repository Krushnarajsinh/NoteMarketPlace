using MyNoteMarketPlace.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyNoteMarketPlace.Controllers
{
    [OutputCache(Duration = 0)]
    [RoutePrefix("Admin")]
    public class AdminReportsController : Controller
    {
        readonly private Datebase1Entities context = new Datebase1Entities();

        // GET: AdminReports
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("SpamReport")]
        public ActionResult SpamReport(string search, string sort, int? page )
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get spam report data for showing in table
            IEnumerable<SpamReportModel> reportlist = from spam in context.SellerNotesReportedIssues
                                                          join note in context.SellerNotes on spam.NoteID equals note.ID
                                                          join reportedby in context.Users on spam.ReportedBYID equals reportedby.ID
                                                          select new SpamReportModel
                                                          {
                                                              ID = spam.ID,
                                                              NoteID = note.ID,
                                                              ReportedBy = reportedby.FirstName + " " + reportedby.LastName,
                                                              NoteTitle = note.Title,
                                                              Category = note.NoteCategories.Name,
                                                              Remark = spam.Remarks,
                                                              DateAdded = spam.CreatedDate.Value
                                                          };

            // get search result
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                reportlist = reportlist.Where(x => x.ReportedBy.ToLower().Contains(search) ||
                                                   x.NoteTitle.ToLower().Contains(search) ||
                                                   x.Category.ToLower().Contains(search) ||
                                                   x.Remark.ToLower().Contains(search) ||
                                                   x.DateAdded.ToString("dd-MM-yyyy, hh:mm").Contains(search)).ToList();
            }

            // sort result
            reportlist = SortTableReportedIssue(sort, reportlist);

            var result = new List<SpamReportModel>();
            result = reportlist.ToList();

            return View(result.ToList().AsQueryable().ToPagedList(page ?? 1, 5));
        }

        // sorting for spam report
        private IEnumerable<SpamReportModel> SortTableReportedIssue(string sort, IEnumerable<SpamReportModel> table)
        {
            switch (sort)
            {
                case "ReportedBy_Asc":
                    {
                        table = table.OrderBy(x => x.ReportedBy);
                        break;
                    }
                case "ReportedBy_Desc":
                    {
                        table = table.OrderByDescending(x => x.ReportedBy);
                        break;
                    }
                case "Title_Asc":
                    {
                        table = table.OrderBy(x => x.NoteTitle);
                        break;
                    }
                case "Title_Desc":
                    {
                        table = table.OrderByDescending(x => x.NoteTitle);
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
                case "Remark_Asc":
                    {
                        table = table.OrderBy(x => x.Remark);
                        break;
                    }
                case "Remark_Desc":
                    {
                        table = table.OrderByDescending(x => x.Remark);
                        break;
                    }
                case "DateAdded_Asc":
                    {
                        table = table.OrderBy(x => x.DateAdded);
                        break;
                    }
                case "DateAdded_Desc":
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
            }
            return table;
        }


        [Authorize(Roles = "Super Admin,Admin")]
        [Route("SpamReport/Delete/{id}")]
        public ActionResult DeleteSpamReport(int id)
        {
            // get spam report object by id
            var spamreport = context.SellerNotesReportedIssues.Where(x => x.ID == id).FirstOrDefault();

            // check if object is null or not
            if (spamreport == null)
            {
                return HttpNotFound();
            }

            // delete object
            context.SellerNotesReportedIssues.Remove(spamreport);
            context.SaveChanges();

            return RedirectToAction("SpamReport");
        }
    }
}