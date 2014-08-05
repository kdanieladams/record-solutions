using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RecordSolutions.Models;
using RecordSolutions.ViewModels;

namespace RecordSolutions.Controllers
{
    public class HomeController : Controller
    {
        // DbContext
        private RSEntities modelDb = new RSEntities();

        // Private methods and variables
        private IndexViewModel initIndexViewModel() 
        {
            // Get latest GeneralSettings object
            GeneralSettings settings = modelDb.GeneralSettings
                .OrderByDescending(g => g.EffectiveDate).FirstOrDefault();

            IndexViewModel ret = null;

            // Find the current user
            UserProfile user = modelDb.UserProfiles.SingleOrDefault(u => u.UserName == User.Identity.Name);
            if (user != null)
            {
                ret = new IndexViewModel
                {
                    UserName = user.UserName,
                    UserAvatarUrl = user.AvatarUrl
                };
            }
            else
            {
                ret = new IndexViewModel
                {
                    UserName = "Guest",
                    UserAvatarUrl = settings.DefaultAvatarUrl
                };
            }

            return ret;
        }

        public ActionResult Index()
        {
            IndexViewModel viewModel = initIndexViewModel();
            return View(viewModel);
        }

        public ActionResult Services() 
        {
            IndexViewModel viewModel = initIndexViewModel();
            return View(viewModel);
        }

        public ActionResult Pricing()
        {
            IndexViewModel viewModel = initIndexViewModel();
            return View(viewModel);
        }

        public ActionResult About()
        {
            IndexViewModel viewModel = initIndexViewModel();

            ViewBag.Message = "Your app description page.";

            return View(viewModel);
        }

        public ActionResult Contact()
        {
            IndexViewModel viewModel = initIndexViewModel();

            ViewBag.Message = "Your contact page.";

            return View(viewModel);
        }

        [Authorize(Roles = "Administrator")]
        public String AuthStr() 
        {
            return "You are an authorized user!";
        }
    }
}
