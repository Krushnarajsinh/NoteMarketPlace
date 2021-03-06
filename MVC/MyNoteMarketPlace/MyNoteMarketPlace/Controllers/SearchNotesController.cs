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
    public class SearchNotesController : Controller
    {
        readonly private Datebase1Entities context = new Datebase1Entities();

        // GET: SearchNotes
        [HttpGet]
        [AllowAnonymous]
        [Route("Search")]
        public ActionResult Search(string search, string type, string category, string university, string course, string country, string ratings, int? page )
        {

            ViewBag.SearchNotes = "active";

            // viewbag for search results
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Type = type;
            ViewBag.University = university;
            ViewBag.Course = course;
            ViewBag.Country = country;
            ViewBag.Rating = ratings;


            ViewBag.CategoryList = context.NoteCategories.Where(x => x.IsActive == true).ToList();
            ViewBag.TypeList = context.NoteTypes.Where(x => x.IsActive == true).ToList();
            ViewBag.CountryList = context.Countries.Where(x => x.IsActive == true).ToList();
            ViewBag.UniversityList = context.SellerNotes.Where(x => x.IsActive == true && x.UniversityName != null && x.Status == 9).Select(x => x.UniversityName).Distinct();
            ViewBag.CourseList = context.SellerNotes.Where(x => x.IsActive == true && x.Course != null && x.Status == 9).Select(x => x.Course).Distinct();
            ViewBag.RatingList = new List<SelectListItem> { new SelectListItem { Text = "1+", Value = "1" }, new SelectListItem { Text = "2+", Value = "2" }, new SelectListItem { Text = "3+", Value = "3" }, new SelectListItem { Text = "4+", Value = "4" }, new SelectListItem { Text = "5", Value = "5" } };

            //Collect Published Notes details
            var notes = context.SellerNotes.Where(x => x.Status == 9);

            //If Search Is Not Empty
            if (!String.IsNullOrEmpty(search))
            {
                notes = notes.Where(x => x.Title.ToLower().Contains(search.ToLower()) ||
                                                 x.NoteCategories.Name.ToLower().Contains(search.ToLower())
                                            );
            }


            if (!String.IsNullOrEmpty(type))
            {
                notes = notes.Where(x => x.NoteType.ToString().ToLower().Contains(type.ToLower()));   //In SellerNotes table we defined NoType As Int So We Write .ToString()
            }

            if (!String.IsNullOrEmpty(category))
            {
                notes = notes.Where(x => x.Category.ToString().ToLower().Contains(category.ToLower()));
            }

            if (!String.IsNullOrEmpty(university))
            {
                notes = notes.Where(x => x.UniversityName.ToLower().Contains(university.ToLower()));
            }

            if (!String.IsNullOrEmpty(course))
            {
                notes = notes.Where(x => x.Course.ToLower().Contains(course.ToLower()));
            }

            if (!String.IsNullOrEmpty(country))
            {
                notes = notes.Where(x => x.Country.ToString().ToLower().Contains(country.ToLower()));
            }


            List<SearchNotesViewModel> searchnoteslist = new List<SearchNotesViewModel>();

            // if rating is empty

            if (String.IsNullOrEmpty(ratings))
            {
                foreach (var item in notes)
                {
                    //fetch rating from table
                    var review = context.SellerNotesReviews.Where(x => x.NoteID == item.ID && x.IsActive == true).Select(x => x.Ratings);

                    var totalreview = review.Count();

                    var avgreview = totalreview > 0 ? Math.Ceiling(review.Average()) : 0;
                    // get spam report count
                    var inappropriate_count = context.SellerNotesReportedIssues.Where(x => x.NoteID == item.ID).Count();

                    SearchNotesViewModel note = new SearchNotesViewModel()
                    {
                        Note = item,
                        AverageRating = Convert.ToInt32(avgreview),
                        TotalRating = totalreview,
                        TotalSpam = inappropriate_count
                    };

                    searchnoteslist.Add(note);
                }
            }
            // if rating is not empty
            else
            {
                foreach (var item in notes)
                {

                    var review = context.SellerNotesReviews.Where(x => x.NoteID == item.ID).Select(x => x.Ratings);

                    var totalreview = review.Count();

                    var avgreview = totalreview > 0 ? Math.Ceiling(review.Average()) : 0;

                    var spamcount = context.SellerNotesReportedIssues.Where(x => x.NoteID == item.ID).Count();
                    // check if average review is greater than or equal to given input rating
                    //Add Those Notes only Whose avgreview is > ratings
                    if (avgreview >= Convert.ToInt32(ratings))
                    {

                        SearchNotesViewModel note = new SearchNotesViewModel()
                        {
                            Note = item,
                            AverageRating = Convert.ToInt32(avgreview),
                            TotalRating = totalreview,
                            TotalSpam = spamcount
                        };

                        searchnoteslist.Add(note);
                    }

                }
            }

            // page number
            ViewBag.PageNumber = page;

            /* count total pages
            ViewBag.TotalPages = Math.Ceiling(searchnoteslist.Count() / 9.0);
            // show record according to pagination
            IEnumerable<SearchNotesViewModel> result = searchnoteslist.AsEnumerable().Skip((page - 1) * 9).Take(9); */

            var result = new List<SearchNotesViewModel>();
            result = searchnoteslist.ToList();

            // total Number Of Notes In Search Page
            ViewBag.ResultCount = searchnoteslist.Count();

            // return View(result);
            return View(result.ToList().AsQueryable().ToPagedList(page ?? 1, 10));

        }
    }
}