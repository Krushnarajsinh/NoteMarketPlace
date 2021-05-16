using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace MyNoteMarketPlace.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        [Route("")]
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }
    }
}