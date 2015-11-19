using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using TakeASeatApp.Models;

namespace TakeASeatApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
		{
			if (!App.IsInstalled)
			{
				return RedirectToAction("Index", "Setup");
			}
            else if (User.IsInRole("Client"))
            {
                return RedirectToAction("Search", "Restaurants");
            }
            else if (User.IsInRole("Manager"))
            {
                return RedirectToAction("Index", "Reservation");
            }
            else if (User.IsInRole("Administrator"))
            {
                return RedirectToAction("Index", "Users");
            }
            return View();
        }
    }
}